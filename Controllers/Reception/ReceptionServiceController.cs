using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر تخصصی مدیریت خدمات در پذیرش - رعایت اصل SRP
    /// مسئولیت: فقط مدیریت خدمات (دسته‌بندی، انتخاب، محاسبه)
    /// </summary>
    [RoutePrefix("Reception/Service")]
    public class ReceptionServiceController : BaseController
    {
        private readonly IReceptionService _receptionService;
        private readonly IServiceCalculationService _serviceCalculationService;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionServiceController(
            IReceptionService receptionService,
            IServiceCalculationService serviceCalculationService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Service Categories & Services

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدمات
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetServiceCategories()
        {
            try
            {
                _logger.Information("📋 دریافت دسته‌بندی‌های خدمات, کاربر: {UserName}", _currentUserService.UserName);

                var result = await _receptionService.GetServiceCategoriesAsync();
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "دسته‌بندی‌های خدمات با موفقیت دریافت شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دسته‌بندی‌های خدمات");
                return Json(new { success = false, message = "خطا در دریافت دسته‌بندی‌ها" });
            }
        }

        /// <summary>
        /// دریافت خدمات بر اساس دسته‌بندی
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetServicesByCategory(int categoryId)
        {
            try
            {
                _logger.Information("📋 دریافت خدمات دسته‌بندی: {CategoryId}, کاربر: {UserName}", 
                    categoryId, _currentUserService.UserName);

                var result = await _receptionService.GetServicesByCategoryAsync(categoryId);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "خدمات با موفقیت دریافت شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات دسته‌بندی: {CategoryId}", categoryId);
                return Json(new { success = false, message = "خطا در دریافت خدمات" });
            }
        }

        /// <summary>
        /// دریافت خدمات بر اساس دپارتمان‌ها
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetServicesByDepartments(string departmentIds)
        {
            try
            {
                _logger.Information("📋 دریافت خدمات دپارتمان‌ها: {DepartmentIds}, کاربر: {UserName}", 
                    departmentIds, _currentUserService.UserName);

                if (string.IsNullOrWhiteSpace(departmentIds))
                {
                    return Json(new { success = false, message = "شناسه دپارتمان‌ها الزامی است" });
                }

                var departmentIdList = departmentIds.Split(',')
                    .Select(id => int.Parse(id.Trim()))
                    .ToList();

                var result = await _receptionService.GetServicesByDepartmentsAsync(departmentIdList);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "خدمات با موفقیت دریافت شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات دپارتمان‌ها: {DepartmentIds}", departmentIds);
                return Json(new { success = false, message = "خطا در دریافت خدمات" });
            }
        }

        #endregion

        #region Service Calculation

        /// <summary>
        /// محاسبه قیمت خدمت با اجزای فنی و حرفه‌ای
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateServicePrice(int serviceId, DateTime? calculationDate = null)
        {
            try
            {
                _logger.Information("💰 محاسبه قیمت خدمت: {ServiceId}, تاریخ: {Date}, کاربر: {UserName}", 
                    serviceId, calculationDate, _currentUserService.UserName);

                var result = await _serviceCalculationService.CalculateServicePriceWithComponentsAsync(
                    serviceId, 0, null);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "محاسبه قیمت با موفقیت انجام شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه قیمت خدمت: {ServiceId}", serviceId);
                return Json(new { success = false, message = "خطا در محاسبه قیمت" });
            }
        }

        /// <summary>
        /// دریافت جزئیات محاسبه خدمت
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetServiceCalculationDetails(int serviceId, DateTime? calculationDate = null)
        {
            try
            {
                _logger.Information("📊 دریافت جزئیات محاسبه خدمت: {ServiceId}, تاریخ: {Date}, کاربر: {UserName}", 
                    serviceId, calculationDate, _currentUserService.UserName);

                var result = await _serviceCalculationService.GetServiceCalculationDetailsAsync(
                    serviceId, 0, null);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "جزئیات محاسبه با موفقیت دریافت شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات محاسبه خدمت: {ServiceId}", serviceId);
                return Json(new { success = false, message = "خطا در دریافت جزئیات" });
            }
        }

        /// <summary>
        /// بررسی وضعیت اجزای خدمت
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetServiceComponentsStatus(int serviceId)
        {
            try
            {
                _logger.Information("🔍 بررسی وضعیت اجزای خدمت: {ServiceId}, کاربر: {UserName}", 
                    serviceId, _currentUserService.UserName);

                var result = await _serviceCalculationService.GetServiceComponentsStatusAsync(serviceId, 0, null);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "وضعیت اجزای خدمت با موفقیت دریافت شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وضعیت اجزای خدمت: {ServiceId}", serviceId);
                return Json(new { success = false, message = "خطا در بررسی وضعیت" });
            }
        }

        #endregion
    }
}
