using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Repositories.Reception
{
    /// <summary>
    /// Repository تخصصی برای مدیریت پزشکان در ماژول پذیرش
    /// </summary>
    public class DoctorManagementRepository : IDoctorManagementRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public DoctorManagementRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger.ForContext<DoctorManagementRepository>();
        }

        /// <summary>
        /// دریافت پزشکان دپارتمان
        /// </summary>
        public async Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDepartmentDoctorsAsync(int departmentId)
        {
            try
            {
                _logger.Information("👨‍⚕️ دریافت پزشکان دپارتمان: {DepartmentId}", departmentId);

                var doctors = await _context.DoctorDepartments
                    .AsNoTracking()
                    .Where(dd => dd.DepartmentId == departmentId && !dd.IsDeleted)
                    .Include(dd => dd.Doctor)
                    .Include(dd => dd.Doctor.DoctorSpecializations.Select(ds => ds.Specialization))
                    .Where(dd => dd.Doctor != null && !dd.Doctor.IsDeleted && dd.Doctor.IsActive)
                    .Select(dd => new ReceptionDoctorLookupViewModel
                    {
                        DoctorId = dd.Doctor.DoctorId,
                        DoctorName = dd.Doctor.FirstName + " " + dd.Doctor.LastName,
                        DoctorCode = dd.Doctor.DoctorCode,
                        Specialization = dd.Doctor.DoctorSpecializations
                            .Where(ds => ds.Specialization.IsActive && !ds.Specialization.IsDeleted)
                            .Select(ds => ds.Specialization.Name)
                            .FirstOrDefault() ?? "نامشخص",
                        IsActive = dd.Doctor.IsActive,
                        DepartmentId = dd.DepartmentId
                    })
                    .OrderBy(d => d.DoctorName)
                    .ToListAsync();

                _logger.Information("✅ {Count} پزشک برای دپارتمان {DepartmentId} یافت شد", doctors.Count, departmentId);

                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Successful(
                    doctors, "پزشکان دپارتمان با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت پزشکان دپارتمان {DepartmentId}", departmentId);
                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                    "خطا در بارگذاری پزشکان دپارتمان");
            }
        }

        /// <summary>
        /// دریافت پزشکان فعال بر اساس شیفت
        /// </summary>
        public async Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetActiveDoctorsByShiftAsync(int departmentId, string shiftType)
        {
            try
            {
                _logger.Information("👨‍⚕️ دریافت پزشکان فعال بر اساس شیفت - دپارتمان: {DepartmentId}, شیفت: {ShiftType}", departmentId, shiftType);

                // TODO: پیاده‌سازی منطق شیفت - فعلاً تمام پزشکان فعال را برمی‌گردانیم
                var doctors = await _context.DoctorDepartments
                    .AsNoTracking()
                    .Where(dd => dd.DepartmentId == departmentId && !dd.IsDeleted)
                    .Include(dd => dd.Doctor)
                    .Where(dd => dd.Doctor != null && !dd.Doctor.IsDeleted && dd.Doctor.IsActive)
                    .Select(dd => new ReceptionDoctorLookupViewModel
                    {
                        DoctorId = dd.Doctor.DoctorId,
                        DoctorName = dd.Doctor.FirstName + " " + dd.Doctor.LastName,
                        DoctorCode = dd.Doctor.DoctorCode,
                        Specialization = dd.Doctor.SpecializationName,
                        IsActive = dd.Doctor.IsActive,
                        DepartmentId = dd.DepartmentId
                    })
                    .OrderBy(d => d.DoctorName)
                    .ToListAsync();

                _logger.Information("✅ {Count} پزشک فعال برای شیفت {ShiftType} یافت شد", doctors.Count, shiftType);

                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Successful(
                    doctors, "پزشکان فعال بر اساس شیفت با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت پزشکان فعال بر اساس شیفت");
                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                    "خطا در بارگذاری پزشکان فعال بر اساس شیفت");
            }
        }

        /// <summary>
        /// دریافت پزشک بر اساس شناسه
        /// </summary>
        public async Task<ServiceResult<ReceptionDoctorLookupViewModel>> GetDoctorByIdAsync(int doctorId)
        {
            try
            {
                _logger.Information("👨‍⚕️ دریافت پزشک بر اساس شناسه: {DoctorId}", doctorId);

                var doctor = await _context.Doctors
                    .AsNoTracking()
                    .Where(d => d.DoctorId == doctorId && !d.IsDeleted && d.IsActive)
                    .Select(d => new ReceptionDoctorLookupViewModel
                    {
                        DoctorId = d.DoctorId,
                        DoctorName = d.FirstName + " " + d.LastName,
                        DoctorCode = d.DoctorCode,
                        Specialization = d.SpecializationName,
                        IsActive = d.IsActive,
                        DepartmentId = 0 // TODO: دریافت از DoctorDepartments
                    })
                    .FirstOrDefaultAsync();

                if (doctor == null)
                {
                    _logger.Warning("⚠️ پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<ReceptionDoctorLookupViewModel>.Failed(
                        "پزشک مورد نظر یافت نشد");
                }

                _logger.Information("✅ پزشک {DoctorName} یافت شد", doctor.DoctorName);

                return ServiceResult<ReceptionDoctorLookupViewModel>.Successful(
                    doctor, "پزشک با موفقیت بارگذاری شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت پزشک {DoctorId}", doctorId);
                return ServiceResult<ReceptionDoctorLookupViewModel>.Failed(
                    "خطا در بارگذاری پزشک");
            }
        }

        /// <summary>
        /// جستجوی پزشکان
        /// </summary>
        public async Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> SearchDoctorsAsync(string searchTerm, int? departmentId = null)
        {
            try
            {
                _logger.Information("🔍 جستجوی پزشکان - عبارت: {SearchTerm}, دپارتمان: {DepartmentId}", searchTerm, departmentId);

                var query = _context.Doctors
                    .AsNoTracking()
                    .Where(d => !d.IsDeleted && d.IsActive);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var normalizedTerm = searchTerm.Trim();
                    query = query.Where(d => 
                        d.FirstName.Contains(normalizedTerm) || 
                        d.LastName.Contains(normalizedTerm) ||
                        d.DoctorCode.Contains(normalizedTerm) ||
                        d.SpecializationName.Contains(normalizedTerm));
                }

                if (departmentId.HasValue)
                {
                    query = query.Where(d => d.DoctorDepartments.Any(dd => dd.DepartmentId == departmentId.Value && !dd.IsDeleted));
                }

                var doctors = await query
                    .Select(d => new ReceptionDoctorLookupViewModel
                    {
                        DoctorId = d.DoctorId,
                        DoctorName = d.FirstName + " " + d.LastName,
                        DoctorCode = d.DoctorCode,
                        Specialization = d.SpecializationName,
                        IsActive = d.IsActive,
                        DepartmentId = departmentId ?? 0
                    })
                    .OrderBy(d => d.DoctorName)
                    .ToListAsync();

                _logger.Information("✅ {Count} پزشک برای عبارت جستجو یافت شد", doctors.Count);

                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Successful(
                    doctors, "پزشکان با موفقیت یافت شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در جستجوی پزشکان");
                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                    "خطا در جستجوی پزشکان");
            }
        }

        /// <summary>
        /// دریافت پزشکان بر اساس تخصص
        /// </summary>
        public async Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDoctorsBySpecializationAsync(int specializationId)
        {
            try
            {
                _logger.Information("👨‍⚕️ دریافت پزشکان بر اساس تخصص: {SpecializationId}", specializationId);

                var doctors = await _context.DoctorSpecializations
                    .AsNoTracking()
                    .Where(ds => ds.SpecializationId == specializationId)
                    .Include(ds => ds.Doctor)
                    .Include(ds => ds.Specialization)
                    .Include(ds => ds.Doctor.DoctorDepartments)
                    .Where(ds => ds.Doctor != null && !ds.Doctor.IsDeleted && ds.Doctor.IsActive)
                    .Where(ds => ds.Specialization != null && !ds.Specialization.IsDeleted && ds.Specialization.IsActive)
                    .Select(ds => new ReceptionDoctorLookupViewModel
                    {
                        DoctorId = ds.Doctor.DoctorId,
                        DoctorName = ds.Doctor.FirstName + " " + ds.Doctor.LastName,
                        DoctorCode = ds.Doctor.DoctorCode,
                        Specialization = ds.Specialization.Name,
                        IsActive = ds.Doctor.IsActive,
                        DepartmentId = ds.Doctor.DoctorDepartments.FirstOrDefault().DepartmentId
                    })
                    .OrderBy(d => d.DoctorName)
                    .ToListAsync();

                _logger.Information("✅ {Count} پزشک برای تخصص {SpecializationId} یافت شد", doctors.Count, specializationId);

                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Successful(
                    doctors, "پزشکان بر اساس تخصص با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت پزشکان بر اساس تخصص");
                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                    "خطا در بارگذاری پزشکان بر اساس تخصص");
            }
        }

        /// <summary>
        /// دریافت تخصص‌های فعال
        /// </summary>
        public async Task<ServiceResult<List<SpecializationLookupViewModel>>> GetActiveSpecializationsAsync()
        {
            try
            {
                _logger.Information("🎓 دریافت تخصص‌های فعال");

                var specializations = await _context.Specializations
                    .AsNoTracking()
                    .Where(s => s.IsActive && !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .Select(s => new SpecializationLookupViewModel
                    {
                        SpecializationId = s.SpecializationId,
                        SpecializationName = s.Name,
                        SpecializationCode = s.Name.Substring(0, Math.Min(4, s.Name.Length)).ToUpper(),
                        Description = s.Description,
                        IsActive = s.IsActive
                    })
                    .ToListAsync();

                _logger.Information("✅ {Count} تخصص فعال یافت شد", specializations.Count);

                return ServiceResult<List<SpecializationLookupViewModel>>.Successful(
                    specializations, "تخصص‌های فعال با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت تخصص‌های فعال");
                return ServiceResult<List<SpecializationLookupViewModel>>.Failed(
                    "خطا در بارگذاری تخصص‌های فعال");
            }
        }
    }
}
