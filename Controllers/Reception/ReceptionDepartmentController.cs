using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.Services.Reception;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر تخصصی مدیریت کلینیک‌ها، دپارتمان‌ها و پزشکان در فرم پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کلینیک‌های فعال
    /// 2. بارگذاری دپارتمان‌ها بر اساس کلینیک
    /// 3. بارگذاری پزشکان بر اساس دپارتمان
    /// 4. مدیریت cascade loading
    /// 5. بهینه‌سازی برای محیط درمانی
    /// 
    /// نکته حیاتی: این کنترلر از سرویس‌های تخصصی استفاده می‌کند
    /// </summary>
    [RoutePrefix("Reception/Department")]
    public class ReceptionDepartmentController : BaseController
    {
        private readonly ReceptionDepartmentService _departmentService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionDepartmentController(
            ReceptionDepartmentService departmentService,
            ILogger logger,
            ICurrentUserService currentUserService) : base(logger)
        {
            _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
            _logger = logger.ForContext<ReceptionDepartmentController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Clinic Management

        /// <summary>
        /// دریافت کلینیک‌های فعال برای فرم پذیرش
        /// </summary>
        /// <returns>لیست کلینیک‌های فعال</returns>
        [HttpGet]
        [Route("GetActiveClinics")]
        public async Task<JsonResult> GetActiveClinics()
        {
            try
            {
                _logger.Information("🏥 دریافت کلینیک‌های فعال برای فرم پذیرش. User: {UserName}", _currentUserService.UserName);

                var result = await _departmentService.GetActiveClinicsForReceptionAsync();
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت کلینیک‌های فعال");
                return Json(new { success = false, message = "خطا در دریافت کلینیک‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Department Management

        /// <summary>
        /// دریافت دپارتمان‌های کلینیک برای فرم پذیرش
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک</param>
        /// <returns>لیست دپارتمان‌های کلینیک</returns>
        [HttpGet]
        [Route("GetClinicDepartments")]
         public async Task<JsonResult> GetClinicDepartments(int? clinicId)
        {
            try
            {
                _logger.Information("🏥 دریافت دپارتمان‌های کلینیک برای فرم پذیرش. ClinicId: {ClinicId}, User: {UserName}", 
                    clinicId, _currentUserService.UserName);

                // Validate clinicId
                if (!clinicId.HasValue || clinicId.Value <= 0)
                {
                    _logger.Warning("شناسه کلینیک نامعتبر: {ClinicId}", clinicId);
                    return Json(new { success = false, message = "شناسه کلینیک نامعتبر است" }, JsonRequestBehavior.AllowGet);
                }

                var result = await _departmentService.GetClinicDepartmentsForReceptionAsync(clinicId.Value);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت دپارتمان‌های کلینیک. ClinicId: {ClinicId}", clinicId);
                return Json(new { success = false, message = "خطا در دریافت دپارتمان‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Doctor Management

        /// <summary>
        /// دریافت پزشکان دپارتمان برای فرم پذیرش
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>لیست پزشکان دپارتمان</returns>
        [HttpGet]
        [Route("GetDepartmentDoctors")]
        public async Task<JsonResult> GetDepartmentDoctors(int departmentId)
        {
            try
            {
                _logger.Information("👨‍⚕️ دریافت پزشکان دپارتمان برای فرم پذیرش. DepartmentId: {DepartmentId}, User: {UserName}", 
                    departmentId, _currentUserService.UserName);

                var result = await _departmentService.GetDepartmentDoctorsForReceptionAsync(departmentId);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت پزشکان دپارتمان. DepartmentId: {DepartmentId}", departmentId);
                return Json(new { success = false, message = "خطا در دریافت پزشکان" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت اطلاعات کامل پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>اطلاعات کامل پزشک</returns>
        [HttpPost]
        [Route("GetDoctorDetails")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetDoctorDetails(int doctorId)
        {
            try
            {
                _logger.Information("👨‍⚕️ دریافت اطلاعات پزشک برای فرم پذیرش. DoctorId: {DoctorId}, User: {UserName}", 
                    doctorId, _currentUserService.UserName);

                var result = await _departmentService.GetDoctorDetailsForReceptionAsync(doctorId);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت اطلاعات پزشک. DoctorId: {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در دریافت اطلاعات پزشک" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Cascade Loading

        /// <summary>
        /// بارگذاری cascade: کلینیک → دپارتمان → پزشک
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <param name="doctorId">شناسه پزشک (اختیاری)</param>
        /// <returns>اطلاعات کامل cascade</returns>
        [HttpPost]
        [Route("LoadCascade")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LoadCascade(int clinicId, int? departmentId = null, int? doctorId = null)
        {
            try
            {
                _logger.Information("🔄 بارگذاری cascade برای فرم پذیرش. ClinicId: {ClinicId}, DepartmentId: {DepartmentId}, DoctorId: {DoctorId}, User: {UserName}", 
                    clinicId, departmentId, doctorId, _currentUserService.UserName);

                var result = await _departmentService.LoadCascadeForReceptionAsync(clinicId, departmentId, doctorId);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بارگذاری cascade. ClinicId: {ClinicId}", clinicId);
                return Json(new { success = false, message = "خطا در بارگذاری cascade" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Department List

        /// <summary>
        /// دریافت لیست دپارتمان‌ها برای سایدبار
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDepartments()
        {
            try
            {
                _logger.Information("🏥 دریافت لیست دپارتمان‌ها برای سایدبار. کاربر: {UserName}", _currentUserService.UserName);

                // دریافت دپارتمان‌های فعال
                var departmentsResult = await _departmentService.GetActiveDepartmentsAsync();

                var result = new
                {
                    success = departmentsResult.Success,
                    data = departmentsResult.Data
                };

                _logger.Information("✅ دپارتمان‌ها دریافت شد: تعداد={Count}", departmentsResult.Data?.Count ?? 0);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت دپارتمان‌ها");
                return Json(new { success = false, message = "خطا در دریافت دپارتمان‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
