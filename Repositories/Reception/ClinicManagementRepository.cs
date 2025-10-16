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
    /// Repository تخصصی برای مدیریت کلینیک‌ها در ماژول پذیرش
    /// </summary>
    public class ClinicManagementRepository : IClinicManagementRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ClinicManagementRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger.ForContext<ClinicManagementRepository>();
        }

        /// <summary>
        /// دریافت لیست کلینیک‌های فعال
        /// </summary>
        public async Task<ServiceResult<List<ClinicLookupViewModel>>> GetActiveClinicsAsync()
        {
            try
            {
                _logger.Information("🏥 دریافت لیست کلینیک‌های فعال");

                var clinics = await _context.Clinics
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new ClinicLookupViewModel
                    {
                        ClinicId = c.ClinicId,
                        ClinicName = c.Name,
                        ClinicCode = c.Code,
                        Address = c.Address,
                        PhoneNumber = c.PhoneNumber,
                        IsActive = c.IsActive
                    })
                    .ToListAsync();

                _logger.Information("✅ {Count} کلینیک فعال یافت شد", clinics.Count);

                return ServiceResult<List<ClinicLookupViewModel>>.Successful(
                    clinics, "کلینیک‌های فعال با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت کلینیک‌های فعال");
                return ServiceResult<List<ClinicLookupViewModel>>.Failed(
                    "خطا در بارگذاری کلینیک‌های فعال");
            }
        }

        /// <summary>
        /// دریافت کلینیک بر اساس شناسه
        /// </summary>
        public async Task<ServiceResult<ClinicLookupViewModel>> GetClinicByIdAsync(int clinicId)
        {
            try
            {
                _logger.Information("🏥 دریافت کلینیک بر اساس شناسه: {ClinicId}", clinicId);

                var clinic = await _context.Clinics
                    .AsNoTracking()
                    .Where(c => c.ClinicId == clinicId && !c.IsDeleted)
                    .Select(c => new ClinicLookupViewModel
                    {
                        ClinicId = c.ClinicId,
                        ClinicName = c.Name,
                        ClinicCode = c.Code,
                        Address = c.Address,
                        PhoneNumber = c.PhoneNumber,
                        IsActive = c.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (clinic == null)
                {
                    _logger.Warning("⚠️ کلینیک با شناسه {ClinicId} یافت نشد", clinicId);
                    return ServiceResult<ClinicLookupViewModel>.Failed(
                        "کلینیک مورد نظر یافت نشد");
                }

                _logger.Information("✅ کلینیک {ClinicName} یافت شد", clinic.ClinicName);

                return ServiceResult<ClinicLookupViewModel>.Successful(
                    clinic, "کلینیک با موفقیت بارگذاری شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت کلینیک {ClinicId}", clinicId);
                return ServiceResult<ClinicLookupViewModel>.Failed(
                    "خطا در بارگذاری کلینیک");
            }
        }

        /// <summary>
        /// دریافت دپارتمان‌های کلینیک
        /// </summary>
        public async Task<ServiceResult<List<DepartmentLookupViewModel>>> GetClinicDepartmentsAsync(int clinicId)
        {
            try
            {
                _logger.Information("🏥 دریافت دپارتمان‌های کلینیک: {ClinicId}", clinicId);

                var departments = await _context.Departments
                    .AsNoTracking()
                    .Where(d => d.ClinicId == clinicId && !d.IsDeleted && d.IsActive)
                    .OrderBy(d => d.Name)
                    .Select(d => new DepartmentLookupViewModel
                    {
                        DepartmentId = d.DepartmentId,
                        DepartmentName = d.Name,
                        DepartmentCode = d.Code,
                        Description = d.Description,
                        IsActive = d.IsActive,
                        ClinicId = d.ClinicId
                    })
                    .ToListAsync();

                _logger.Information("✅ {Count} دپارتمان برای کلینیک {ClinicId} یافت شد", departments.Count, clinicId);

                return ServiceResult<List<DepartmentLookupViewModel>>.Successful(
                    departments, "دپارتمان‌های کلینیک با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت دپارتمان‌های کلینیک {ClinicId}", clinicId);
                return ServiceResult<List<DepartmentLookupViewModel>>.Failed(
                    "خطا در بارگذاری دپارتمان‌های کلینیک");
            }
        }

        /// <summary>
        /// دریافت دپارتمان‌های فعال بر اساس شیفت
        /// </summary>
        public async Task<ServiceResult<List<DepartmentLookupViewModel>>> GetActiveDepartmentsByShiftAsync(int clinicId, string shiftType)
        {
            try
            {
                _logger.Information("🏥 دریافت دپارتمان‌های فعال بر اساس شیفت - کلینیک: {ClinicId}, شیفت: {ShiftType}", clinicId, shiftType);

                // دریافت دپارتمان‌هایی که در این کلینیک و شیفت فعال هستند
                var departments = await _context.Departments
                    .AsNoTracking()
                    .Where(d => d.ClinicId == clinicId && d.IsActive && !d.IsDeleted)
                    .Where(d => d.DoctorDepartments.Any(dd => 
                        dd.Doctor.IsActive && !dd.Doctor.IsDeleted &&
                        dd.Doctor.DoctorSchedules.Any(ds => 
                            ds.IsShiftActive && !ds.IsDeleted && 
                            ds.ShiftType.ToString() == shiftType)))
                    .Select(d => new DepartmentLookupViewModel
                    {
                        DepartmentId = d.DepartmentId,
                        DepartmentName = d.Name,
                        DepartmentCode = d.Code,
                        Description = d.Description,
                        IsActive = d.IsActive,
                        ClinicId = d.ClinicId
                    })
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync();

                _logger.Information("✅ {Count} دپارتمان فعال برای شیفت {ShiftType} یافت شد", departments.Count, shiftType);

                return ServiceResult<List<DepartmentLookupViewModel>>.Successful(
                    departments, "دپارتمان‌های فعال بر اساس شیفت با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت دپارتمان‌های فعال بر اساس شیفت");
                return ServiceResult<List<DepartmentLookupViewModel>>.Failed(
                    "خطا در بارگذاری دپارتمان‌های فعال بر اساس شیفت");
            }
        }
    }
}