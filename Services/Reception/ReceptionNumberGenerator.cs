using System;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using ClinicApp.Helpers;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// 🏥 MEDICAL: سرویس تولید شماره پذیرش استاندارد
    /// 
    /// الگوهای استاندارد:
    /// - ReceptionNo: YYYY-MMDD-XXXXX (مثل 1404-0816-00123)
    ///   - YYYY: سال شمسی (4 رقم)
    ///   - MMDD: ماه و روز شمسی (4 رقم)
    ///   - XXXXX: شماره ترتیب روزانه (5 رقم)
    /// 
    /// - ElectronicReceptionNumber: PATIENTID-YYYY-MMDD-XXXXX (مثل 167-1404-0816-00123)
    ///   - PATIENTID: شناسه بیمار (عدد)
    ///   - YYYY-MMDD-XXXXX: همان ReceptionNo
    /// </summary>
    public class ReceptionNumberGenerator
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ReceptionNumberGenerator(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// تولید شماره پذیرش رسمی (ReceptionNo)
        /// الگو: YYYY-MMDD-XXXXX
        /// </summary>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>شماره پذیرش منحصر به فرد</returns>
        public async Task<string> GenerateReceptionNoAsync(DateTime receptionDate)
        {
            try
            {
                _logger.Debug("🏥 NUMBER: شروع تولید ReceptionNo برای تاریخ {Date}", receptionDate);

                // تبدیل به تاریخ شمسی
                var persianDate = PersianDateHelper.ToPersianDate(receptionDate); // فرمت: YYYY/MM/DD
                var parts = persianDate.Split('/');
                
                if (parts.Length != 3)
                {
                    _logger.Error("❌ NUMBER: فرمت تاریخ شمسی نامعتبر: {PersianDate}", persianDate);
                    throw new InvalidOperationException($"فرمت تاریخ شمسی نامعتبر: {persianDate}");
                }

                var year = parts[0]; // YYYY
                var month = parts[1].PadLeft(2, '0'); // MM
                var day = parts[2].PadLeft(2, '0'); // DD
                var datePart = $"{year}-{month}{day}"; // YYYY-MMDD

                // محاسبه شماره ترتیب روزانه
                var todayStart = new DateTime(receptionDate.Year, receptionDate.Month, receptionDate.Day);
                var todayEnd = todayStart.AddDays(1);

                // شمارش پذیرش‌های امروز (فقط ReceptionNo های معتبر)
                var todayCount = await _context.Receptions
                    .AsNoTracking()
                    .Where(r => 
                        r.CreatedAt >= todayStart && 
                        r.CreatedAt < todayEnd && 
                        !r.IsDeleted &&
                        r.ReceptionNo != null &&
                        r.ReceptionNo.StartsWith(datePart))
                    .CountAsync();

                var sequence = (todayCount + 1).ToString("00000"); // 5 رقم
                var receptionNo = $"{datePart}-{sequence}"; // YYYY-MMDD-XXXXX

                _logger.Information("✅ NUMBER: ReceptionNo تولید شد - {ReceptionNo}, Count: {Count}", 
                    receptionNo, todayCount + 1);

                // بررسی Unique بودن (احتمال بسیار کم، اما برای اطمینان)
                var exists = await _context.Receptions
                    .AsNoTracking()
                    .AnyAsync(r => r.ReceptionNo == receptionNo && !r.IsDeleted);

                if (exists)
                {
                    _logger.Warning("⚠️ NUMBER: ReceptionNo تکراری شناسایی شد، افزایش sequence - {ReceptionNo}", 
                        receptionNo);
                    
                    // افزایش sequence تا پیدا کردن شماره منحصر به فرد
                    var receptionNos = await _context.Receptions
                        .AsNoTracking()
                        .Where(r => 
                            r.CreatedAt >= todayStart && 
                            r.CreatedAt < todayEnd && 
                            !r.IsDeleted &&
                            r.ReceptionNo != null &&
                            r.ReceptionNo.StartsWith(datePart))
                        .Select(r => r.ReceptionNo)
                        .ToListAsync();
                    
                    var maxSequence = receptionNos
                        .Select(rn =>
                        {
                            var parts = rn.Split('-');
                            if (parts.Length == 3 && int.TryParse(parts[2], out int seq))
                                return seq;
                            return 0;
                        })
                        .DefaultIfEmpty(0)
                        .Max();

                    sequence = (maxSequence + 1).ToString("00000");
                    receptionNo = $"{datePart}-{sequence}";
                    
                    _logger.Information("✅ NUMBER: ReceptionNo جدید تولید شد - {ReceptionNo}", receptionNo);
                }

                return receptionNo;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ NUMBER: خطا در تولید ReceptionNo");
                throw;
            }
        }

        /// <summary>
        /// تولید شماره الکترونیکی پذیرش (ElectronicReceptionNumber)
        /// الگو: PATIENTID-YYYY-MMDD-XXXXX
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="receptionNo">شماره پذیرش رسمی</param>
        /// <returns>شماره الکترونیکی</returns>
        public string GenerateElectronicReceptionNumber(int patientId, string receptionNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(receptionNo))
                {
                    _logger.Error("❌ NUMBER: ReceptionNo خالی است برای تولید ElectronicReceptionNumber");
                    throw new ArgumentException("ReceptionNo نمی‌تواند خالی باشد", nameof(receptionNo));
                }

                var electronicNumber = $"{patientId}-{receptionNo}";
                
                _logger.Debug("✅ NUMBER: ElectronicReceptionNumber تولید شد - {ElectronicNumber}", electronicNumber);
                
                return electronicNumber;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ NUMBER: خطا در تولید ElectronicReceptionNumber");
                throw;
            }
        }

        /// <summary>
        /// تولید هر دو شماره به صورت همزمان
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>Tuple شامل (ReceptionNo, ElectronicReceptionNumber)</returns>
        public async Task<(string ReceptionNo, string ElectronicReceptionNumber)> GenerateBothAsync(
            int patientId, 
            DateTime receptionDate)
        {
            try
            {
                var receptionNo = await GenerateReceptionNoAsync(receptionDate);
                var electronicNumber = GenerateElectronicReceptionNumber(patientId, receptionNo);
                
                _logger.Information("✅ NUMBER: هر دو شماره تولید شدند - ReceptionNo: {ReceptionNo}, Electronic: {Electronic}", 
                    receptionNo, electronicNumber);
                
                return (receptionNo, electronicNumber);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ NUMBER: خطا در تولید شماره‌ها");
                throw;
            }
        }
    }
}

