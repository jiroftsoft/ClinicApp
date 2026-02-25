using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Repositories;
using ClinicApp.Models;
using Serilog;
using PatientEntity = ClinicApp.Models.Entities.Patient.Patient;

namespace ClinicApp.Repositories.Patient
{
    /// <summary>
    /// ریپازیتوری بیماران – جستجو و CRUD با منطق بهینه و ضد گلوله برای کد ملی
    /// </summary>
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        /// <summary>حداکثر تعداد نامزد در جستجوی suffix (جلوگیری از بار زیاد و DoS)</summary>
        private const int MaxSuffixCandidates = 200;

        /// <summary>طول استاندارد کد ملی ایران</summary>
        private const int NationalCodeLength = 10;

        public PatientRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger?.ForContext<PatientRepository>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// نرمال‌سازی امن کد ملی برای جستجو: فارسی/عربی → انگلیسی، حذف فاصله، بدون پرتاب استثنا
        /// </summary>
        private static string NormalizeNationalCodeForLookup(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            try
            {
                var converted = PersianNumberHelper.ToEnglishNumbers(input);
                var trimmed = (converted ?? input).Trim();
                return trimmed.Length > 0 ? trimmed : null;
            }
            catch
            {
                return input?.Trim();
            }
        }

        /// <summary>
        /// تطابق نرمال‌شده در حافظه: حذف صفرهای ابتدا و فاصله برای مقایسه
        /// </summary>
        private static string NormalizeStoredForCompare(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored)) return string.Empty;
            try
            {
                var en = PersianNumberHelper.ToEnglishNumbers(stored);
                return (en ?? stored).Trim().TrimStart('0');
            }
            catch
            {
                return (stored ?? "").Trim().TrimStart('0');
            }
        }

        /// <summary>
        /// آیا رشته فقط رقم انگلیسی است (برای اجازه به استراتژی suffix)
        /// </summary>
        private static bool IsAllDigits(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            for (int i = 0; i < value.Length; i++)
                if (value[i] < '0' || value[i] > '9') return false;
            return true;
        }

        public async Task<PatientEntity> GetPatientByIdAsync(int patientId)
        {
            if (patientId <= 0) return null;
            return await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted);
        }

        /// <summary>
        /// جستجوی بیمار با کد ملی – بهینه و ضد گلوله.
        /// از یک DbContext اختصاصی استفاده می‌کند تا از InvalidOperationException/NotSupportedException
        /// ناشی از استفادهٔ همزمان یا اشتراک context با درخواست جلوگیری شود.
        /// استراتژی: ۱) تطابق دقیق  ۲) جستجو با ۹ رقم انتهایی و تطابق نرمال در حافظه.
        /// </summary>
        public async Task<PatientEntity> GetPatientByNationalCodeAsync(string nationalCode)
        {
            var normalized = NormalizeNationalCodeForLookup(nationalCode);
            if (string.IsNullOrWhiteSpace(normalized))
                return null;

            // استفاده از context اختصاصی برای این عملیات (جلوگیری از second operation / context disposed)
            using (var ctx = new ApplicationDbContext())
            {
                try
                {
                    // ─── استراتژی ۱: تطابق دقیق (ایندکس‌دوست، بدون Trim در LINQ)
                    var patient = await ctx.Patients
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => !p.IsDeleted && p.NationalCode != null && p.NationalCode == normalized)
                        .ConfigureAwait(false);
                    if (patient != null)
                        return patient;

                    // ─── استراتژی ۲: فقط برای کد ۱۰ رقمی عددی – جستجو با ۹ رقم انتهایی (محدود به MaxSuffixCandidates)
                    if (normalized.Length == NationalCodeLength && IsAllDigits(normalized))
                    {
                        var suffix = normalized.Substring(1);
                        var candidates = await ctx.Patients
                            .AsNoTracking()
                            .Where(p => !p.IsDeleted && p.NationalCode != null && p.NationalCode.EndsWith(suffix))
                            .OrderBy(p => p.PatientId)
                            .Take(MaxSuffixCandidates)
                            .ToListAsync()
                            .ConfigureAwait(false);

                        patient = candidates.FirstOrDefault(p =>
                            NormalizeStoredForCompare(p.NationalCode) == normalized);
                        if (patient != null)
                            return patient;
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex,
                        "GetPatientByNationalCodeAsync: خطا در جستجو با context اختصاصی - Normalized: {Normalized}, ExceptionType: {ExceptionType}, Message: {Message}",
                        normalized, ex.GetType().FullName, ex.Message);
                    if (ex.InnerException != null)
                        _logger.Warning("GetPatientByNationalCodeAsync: InnerException: {InnerType}, {InnerMessage}",
                            ex.InnerException.GetType().FullName, ex.InnerException.Message);
                    throw;
                }
            }
        }

        public async Task<List<PatientEntity>> SearchPatientsAsync(string keyword, int pageNumber, int pageSize)
        {
            keyword = (keyword ?? "").Trim();
            var query = _context.Patients.AsNoTracking().Where(p => !p.IsDeleted);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p =>
                    p.FirstName.Contains(keyword) ||
                    p.LastName.Contains(keyword) ||
                    (p.NationalCode != null && p.NationalCode.Contains(keyword)) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(keyword)));
            }
            var skip = Math.Max(0, (pageNumber - 1) * pageSize);
            var take = Math.Max(1, pageSize);
            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<PatientEntity> CreatePatientAsync(PatientEntity patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<PatientEntity> UpdatePatientAsync(PatientEntity patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            _context.Entry(patient).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<bool> PatientExistsByNationalCodeAsync(string nationalCode)
        {
            var normalized = NormalizeNationalCodeForLookup(nationalCode);
            if (string.IsNullOrWhiteSpace(normalized)) return false;
            return await _context.Patients
                .AnyAsync(p => !p.IsDeleted && p.NationalCode != null && p.NationalCode == normalized);
        }

        public async Task<int> GetPatientCountAsync(string keyword = null)
        {
            keyword = (keyword ?? "").Trim();
            var query = _context.Patients.AsNoTracking().Where(p => !p.IsDeleted);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p =>
                    p.FirstName.Contains(keyword) ||
                    p.LastName.Contains(keyword) ||
                    (p.NationalCode != null && p.NationalCode.Contains(keyword)) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(keyword)));
            }
            return await query.CountAsync();
        }
    }
}
