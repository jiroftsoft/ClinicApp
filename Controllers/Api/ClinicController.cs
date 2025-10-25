using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers.Base;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// API Controller مدیریت کلینیک‌ها و دپارتمان‌ها در پذیرش
    /// 
    /// Responsibilities:
    /// 1. دریافت لیست کلینیک‌ها
    /// 2. دریافت دپارتمان‌های کلینیک
    /// 3. دریافت دپارتمان‌های فعال
    /// 
    /// Architecture:
    /// ✅ Single Responsibility: فقط Clinic/Department
    /// ✅ No Cache: طبق سیاست
    /// ✅ Conditional Authorization
    /// </summary>
    public class ClinicController : BaseController
    {
        #region Fields

        private readonly ApplicationDbContext _context;
        private readonly IReceptionService _receptionService;

        #endregion

        #region Constructor

        public ClinicController(
            ApplicationDbContext context,
            IReceptionService receptionService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(currentUserService, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
        }

        #endregion

        #region Clinic Actions

        /// <summary>
        /// دریافت لیست کلینیک‌های فعال
        /// </summary>
        /// <returns>لیست کلینیک‌ها</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetAll()
        {
            using (StartPerformanceMonitoring("GetClinics"))
            {
                try
                {
                    _logger.Information("🏥 دریافت کلینیک‌ها. کاربر: {UserName}", 
                        _currentUserService.UserName);

                    AddSecurityHeaders();

                    var result = await _receptionService.GetActiveClinicsAsync();

                    if (!result.Success)
                    {
                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        });
                    }

                    _logger.Information("✅ {Count} کلینیک دریافت شد", 
                        result.Data?.Count() ?? 0);

                    return SuccessResponse(result.Data, "کلینیک‌ها با موفقیت دریافت شدند");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetClinics");
                }
            }
        }

        #endregion

        #region Department Actions

        /// <summary>
        /// دریافت دپارتمان‌های کلینیک
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک</param>
        /// <returns>لیست دپارتمان‌ها</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetDepartments(int clinicId)
        {
            using (StartPerformanceMonitoring("GetClinicDepartments"))
            {
                try
                {
                    _logger.Information(
                        "🏥 دریافت دپارتمان‌های کلینیک. کلینیک: {ClinicId}, کاربر: {UserName}",
                        clinicId, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (clinicId <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه کلینیک نامعتبر است"
                        });
                    }

                    var departments = await _context.Departments
                        .Where(d => d.ClinicId == clinicId && d.IsActive)
                        .OrderBy(d => d.Name)
                        .Select(d => new
                        {
                            d.DepartmentId,
                            d.Name,
                            d.Code,
                            d.Description,
                            d.ClinicId,
                            d.IsActive,
                            DoctorCount = d.DoctorDepartments.Count(dd => dd.IsActive),
                            ServiceCount = d.ServiceCategories.Count(sc => sc.IsActive)
                        })
                        .ToListAsync();

                    _logger.Information("✅ {Count} دپارتمان دریافت شد", departments.Count);

                    return SuccessResponse(departments, 
                        $"{departments.Count} دپارتمان دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetClinicDepartments", new { clinicId });
                }
            }
        }

        /// <summary>
        /// دریافت تمام دپارتمان‌های فعال (بدون فیلتر کلینیک)
        /// </summary>
        /// <returns>لیست دپارتمان‌ها</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetAllDepartments()
        {
            using (StartPerformanceMonitoring("GetAllDepartments"))
            {
                try
                {
                    _logger.Information("🏥 دریافت تمام دپارتمان‌ها. کاربر: {UserName}", 
                        _currentUserService.UserName);

                    AddSecurityHeaders();

                    var departments = await _context.Departments
                        .Where(d => d.IsActive)
                        .OrderBy(d => d.Clinic.Name)
                        .ThenBy(d => d.Name)
                        .Select(d => new
                        {
                            d.DepartmentId,
                            d.Name,
                            d.Code,
                            d.Description,
                            d.ClinicId,
                            ClinicName = d.Clinic.Name,
                            d.IsActive,
                            DoctorCount = d.DoctorDepartments.Count(dd => dd.IsActive)
                        })
                        .ToListAsync();

                    _logger.Information("✅ {Count} دپارتمان دریافت شد", departments.Count);

                    return SuccessResponse(departments, 
                        $"{departments.Count} دپارتمان دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetAllDepartments");
                }
            }
        }

        #endregion
    }
}

