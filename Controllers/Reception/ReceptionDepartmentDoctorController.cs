using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Services.Reception;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر تخصصی مدیریت دپارتمان و پزشک در ماژول پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. لودینگ پویا دپارتمان‌ها بر اساس کلینیک
    /// 2. لودینگ پویا پزشکان بر اساس دپارتمان
    /// 3. مدیریت cascade loading برای محیط درمانی
    /// 4. پشتیبانی از Select2 برای انتخاب دپارتمان و پزشک
    /// 5. مدیریت شیفت کاری پزشکان
    /// 6. بهینه‌سازی برای محیط درمانی با استانداردهای بالا
    /// </summary>
    [RoutePrefix("Reception/DepartmentDoctor")]
    public class ReceptionDepartmentDoctorController : BaseController
    {
        private readonly ReceptionDepartmentDoctorService _departmentDoctorService;

        public ReceptionDepartmentDoctorController(
            ReceptionDepartmentDoctorService departmentDoctorService,
            ILogger logger) : base(logger)
        {
            _departmentDoctorService = departmentDoctorService ?? throw new ArgumentNullException(nameof(departmentDoctorService));
        }

        #region Clinic Management

        /// <summary>
        /// دریافت کلینیک‌های فعال برای فرم پذیرش
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetActiveClinics()
        {
            try
            {
                _logger.Information("🏥 درخواست دریافت کلینیک‌های فعال");

                var result = await _departmentDoctorService.GetActiveClinicsAsync();

                if (result.Success)
                {
                    _logger.Information($"کلینیک‌های فعال با موفقیت دریافت شد - تعداد: {result.Data.Count}");
                    return Json(new { 
                        success = true, 
                        data = result.Data, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning($"دریافت کلینیک‌های فعال با خطا مواجه شد: {result.Message}");
                    return Json(new { 
                        success = false, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کلینیک‌های فعال");
                
                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت کلینیک‌های فعال" 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Department Management

        /// <summary>
        /// دریافت دپارتمان‌های کلینیک برای فرم پذیرش
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetClinicDepartments(int clinicId)
        {
            try
            {
                _logger.Information($"🏢 درخواست دریافت دپارتمان‌های کلینیک: {clinicId}");

                var result = await _departmentDoctorService.GetClinicDepartmentsAsync(clinicId);

                if (result.Success)
                {
                    _logger.Information($"دپارتمان‌های کلینیک {clinicId} با موفقیت دریافت شد - تعداد: {result.Data.Count}");
                    return Json(new { 
                        success = true, 
                        data = result.Data, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning($"دریافت دپارتمان‌های کلینیک {clinicId} با خطا مواجه شد: {result.Message}");
                    return Json(new { 
                        success = false, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت دپارتمان‌های کلینیک {clinicId}");
                
                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت دپارتمان‌های کلینیک" 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت دپارتمان‌های فعال بر اساس شیفت فعلی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetActiveDepartmentsByShift(int clinicId)
        {
            try
            {
                _logger.Information($"🏢 درخواست دریافت دپارتمان‌های فعال بر اساس شیفت - کلینیک: {clinicId}");

                var result = await _departmentDoctorService.GetActiveDepartmentsByShiftAsync(clinicId);

                if (result.Success)
                {
                    _logger.Information($"دپارتمان‌های فعال کلینیک {clinicId} با موفقیت دریافت شد - تعداد: {result.Data.Count}");
                    return Json(new { 
                        success = true, 
                        data = result.Data, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning($"دریافت دپارتمان‌های فعال کلینیک {clinicId} با خطا مواجه شد: {result.Message}");
                    return Json(new { 
                        success = false, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت دپارتمان‌های فعال کلینیک {clinicId}");
                
                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت دپارتمان‌های فعال" 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Doctor Management

        /// <summary>
        /// دریافت پزشکان دپارتمان برای فرم پذیرش
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDepartmentDoctors(int departmentId)
        {
            try
            {
                _logger.Information($"👨‍⚕️ درخواست دریافت پزشکان دپارتمان: {departmentId}");

                var result = await _departmentDoctorService.GetDepartmentDoctorsAsync(departmentId);

                if (result.Success)
                {
                    _logger.Information($"پزشکان دپارتمان {departmentId} با موفقیت دریافت شد - تعداد: {result.Data.Count}");
                    return Json(new { 
                        success = true, 
                        data = result.Data, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning($"دریافت پزشکان دپارتمان {departmentId} با خطا مواجه شد: {result.Message}");
                    return Json(new { 
                        success = false, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت پزشکان دپارتمان {departmentId}");
                
                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت پزشکان دپارتمان" 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت پزشکان بر اساس تخصص
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDoctorsBySpecialization(int specializationId)
        {
            try
            {
                _logger.Information($"👨‍⚕️ درخواست دریافت پزشکان بر اساس تخصص: {specializationId}");

                var result = await _departmentDoctorService.GetDoctorsBySpecializationAsync(specializationId);

                if (result.Success)
                {
                    _logger.Information($"پزشکان تخصص {specializationId} با موفقیت دریافت شد - تعداد: {result.Data.Count}");
                    return Json(new { 
                        success = true, 
                        data = result.Data, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning($"دریافت پزشکان تخصص {specializationId} با خطا مواجه شد: {result.Message}");
                    return Json(new { 
                        success = false, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت پزشکان تخصص {specializationId}");
                
                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت پزشکان تخصص" 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Shift Management

        /// <summary>
        /// دریافت اطلاعات شیفت فعلی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetCurrentShiftInfo()
        {
            try
            {
                _logger.Information("🕐 درخواست دریافت اطلاعات شیفت فعلی");

                var result = await _departmentDoctorService.GetCurrentShiftInfoAsync();

                if (result.Success)
                {
                    _logger.Information($"اطلاعات شیفت فعلی با موفقیت دریافت شد");
                    return Json(new { 
                        success = true, 
                        data = result.Data, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning($"دریافت اطلاعات شیفت فعلی با خطا مواجه شد: {result.Message}");
                    return Json(new { 
                        success = false, 
                        message = result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات شیفت فعلی");
                
                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت اطلاعات شیفت فعلی" 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Combined Operations

        /// <summary>
        /// دریافت اطلاعات کامل دپارتمان و پزشک برای فرم پذیرش
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDepartmentDoctorInfo(int clinicId, int? departmentId = null)
        {
            try
            {
                _logger.Information($"📋 درخواست دریافت اطلاعات کامل دپارتمان و پزشک - کلینیک: {clinicId}, دپارتمان: {departmentId}");

                var result = new
                {
                    Clinics = await _departmentDoctorService.GetActiveClinicsAsync(),
                    Departments = await _departmentDoctorService.GetActiveDepartmentsByShiftAsync(clinicId),
                    Doctors = departmentId.HasValue ? 
                        await _departmentDoctorService.GetDepartmentDoctorsAsync(departmentId.Value) : 
                        ServiceResult<List<ReceptionDoctorLookupViewModel>>.Successful(new List<ReceptionDoctorLookupViewModel>(), "هیچ پزشکی انتخاب نشده"),
                    ShiftInfo = await _departmentDoctorService.GetCurrentShiftInfoAsync()
                };

                _logger.Information($"اطلاعات کامل دپارتمان و پزشک با موفقیت دریافت شد");
                return Json(new { 
                    success = true, 
                    data = result, 
                    message = "اطلاعات کامل دپارتمان و پزشک با موفقیت دریافت شد" 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت اطلاعات کامل دپارتمان و پزشک - کلینیک: {clinicId}");
                
                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت اطلاعات کامل دپارتمان و پزشک" 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
