using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر تخصصی مدیریت بیماران در پذیرش - رعایت اصل SRP
    /// مسئولیت: فقط مدیریت بیماران (جستجو، ایجاد، ویرایش)
    /// </summary>
    public class ReceptionPatientController : BaseController
    {
        private readonly IReceptionService _receptionService;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionPatientController(
            IReceptionService receptionService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Patient Search & Management

        /// <summary>
        /// جستجوی بیمار بر اساس کد ملی
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> SearchByNationalCode(string nationalCode)
        {
            try
            {
                _logger.Information("🔍 جستجوی بیمار با کد ملی: {NationalCode}, کاربر: {UserName}", 
                    nationalCode, _currentUserService.UserName);

                if (string.IsNullOrWhiteSpace(nationalCode))
                {
                    return Json(new { success = false, message = "کد ملی الزامی است" });
                }

                var result = await _receptionService.SearchPatientByNationalCodeAsync(nationalCode);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "بیمار با موفقیت یافت شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی بیمار با کد ملی: {NationalCode}", nationalCode);
                return Json(new { success = false, message = "خطا در جستجوی بیمار" });
            }
        }

        /// <summary>
        /// جستجوی بیمار بر اساس نام
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> SearchByName(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.Information("🔍 جستجوی بیمار با نام: {SearchTerm}, صفحه: {PageNumber}, کاربر: {UserName}", 
                    searchTerm, pageNumber, _currentUserService.UserName);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Json(new { success = false, message = "نام بیمار الزامی است" });
                }

                var result = await _receptionService.SearchPatientsByNameAsync(searchTerm, pageNumber, pageSize);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    totalCount = result.TotalCount,
                    pageNumber = pageNumber,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی بیمار با نام: {SearchTerm}", searchTerm);
                return Json(new { success = false, message = "خطا در جستجوی بیمار" });
            }
        }

        /// <summary>
        /// ایجاد بیمار جدید در حین پذیرش
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CreatePatient(PatientCreateEditViewModel model)
        {
            try
            {
                _logger.Information("➕ ایجاد بیمار جدید: {FirstName} {LastName}, کد ملی: {NationalCode}, کاربر: {UserName}", 
                    model.FirstName, model.LastName, model.NationalCode, _currentUserService.UserName);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new { 
                        success = false, 
                        message = "اطلاعات وارد شده نامعتبر است",
                        errors = errors
                    });
                }

                var result = await _receptionService.CreatePatientAsync(model);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "بیمار با موفقیت ایجاد شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد بیمار جدید");
                return Json(new { success = false, message = "خطا در ایجاد بیمار" });
            }
        }

        /// <summary>
        /// دریافت تاریخچه پذیرش‌های بیمار
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetPatientReceptionHistory(int patientId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.Information("📋 دریافت تاریخچه پذیرش‌های بیمار: {PatientId}, صفحه: {PageNumber}, کاربر: {UserName}", 
                    patientId, pageNumber, _currentUserService.UserName);

                var result = await _receptionService.GetPatientReceptionHistoryAsync(patientId, pageNumber, pageSize);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    totalCount = result.TotalCount,
                    pageNumber = pageNumber,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تاریخچه پذیرش‌های بیمار: {PatientId}", patientId);
                return Json(new { success = false, message = "خطا در دریافت تاریخچه" });
            }
        }

        #endregion
    }
}
