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
    /// کنترلر تخصصی مدیریت سرفصل‌ها و خدمات در فرم پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. بارگذاری سرفصل‌ها بر اساس دپارتمان
    /// 2. بارگذاری خدمات بر اساس سرفصل
    /// 3. جستجو با کد خدمت
    /// 4. مدیریت cascade loading
    /// 5. بهینه‌سازی برای محیط درمانی
    /// 
    /// نکته حیاتی: این کنترلر از سرویس‌های تخصصی استفاده می‌کند
    /// </summary>
    [RoutePrefix("Reception/ServiceManagement")]
    public class ReceptionServiceManagementController : BaseController
    {
        private readonly ReceptionServiceManagementService _serviceManagementService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionServiceManagementController(
            ReceptionServiceManagementService serviceManagementService,
            ILogger logger,
            ICurrentUserService currentUserService) : base(logger)
        {
            _serviceManagementService = serviceManagementService ?? throw new ArgumentNullException(nameof(serviceManagementService));
            _logger = logger.ForContext<ReceptionServiceManagementController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Service Category Management

        /// <summary>
        /// دریافت سرفصل‌های دپارتمان برای فرم پذیرش
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>لیست سرفصل‌های دپارتمان</returns>
        [HttpPost]
        [Route("GetDepartmentServiceCategories")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetDepartmentServiceCategories(int departmentId)
        {
            try
            {
                _logger.Information("📋 دریافت سرفصل‌های دپارتمان برای فرم پذیرش. DepartmentId: {DepartmentId}, User: {UserName}", 
                    departmentId, _currentUserService.UserName);

                var result = await _serviceManagementService.GetDepartmentServiceCategoriesForReceptionAsync(departmentId);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت سرفصل‌های دپارتمان. DepartmentId: {DepartmentId}", departmentId);
                return Json(new { success = false, message = "خطا در دریافت سرفصل‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Service Management

        /// <summary>
        /// دریافت خدمات سرفصل برای فرم پذیرش
        /// </summary>
        /// <param name="categoryId">شناسه سرفصل</param>
        /// <returns>لیست خدمات سرفصل</returns>
        [HttpPost]
        [Route("GetCategoryServices")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetCategoryServices(int categoryId)
        {
            try
            {
                _logger.Information("🔧 دریافت خدمات سرفصل برای فرم پذیرش. CategoryId: {CategoryId}, User: {UserName}", 
                    categoryId, _currentUserService.UserName);

                var result = await _serviceManagementService.GetCategoryServicesForReceptionAsync(categoryId);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت خدمات سرفصل. CategoryId: {CategoryId}", categoryId);
                return Json(new { success = false, message = "خطا در دریافت خدمات" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// جستجوی خدمت با کد خدمت
        /// </summary>
        /// <param name="serviceCode">کد خدمت</param>
        /// <returns>اطلاعات خدمت</returns>
        [HttpPost]
        [Route("SearchServiceByCode")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SearchServiceByCode(string serviceCode)
        {
            try
            {
                _logger.Information("🔍 جستجوی خدمت با کد. ServiceCode: {ServiceCode}, User: {UserName}", 
                    serviceCode, _currentUserService.UserName);

                var result = await _serviceManagementService.SearchServiceByCodeAsync(serviceCode);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در جستجوی خدمت با کد. ServiceCode: {ServiceCode}", serviceCode);
                return Json(new { success = false, message = "خطا در جستجوی خدمت" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت اطلاعات کامل خدمت
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>اطلاعات کامل خدمت</returns>
        [HttpPost]
        [Route("GetServiceDetails")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetServiceDetails(int serviceId)
        {
            try
            {
                _logger.Information("🔧 دریافت اطلاعات خدمت برای فرم پذیرش. ServiceId: {ServiceId}, User: {UserName}", 
                    serviceId, _currentUserService.UserName);

                var result = await _serviceManagementService.GetServiceDetailsForReceptionAsync(serviceId);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت اطلاعات خدمت. ServiceId: {ServiceId}", serviceId);
                return Json(new { success = false, message = "خطا در دریافت اطلاعات خدمت" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Cascade Loading

        /// <summary>
        /// بارگذاری cascade: دپارتمان → سرفصل → خدمت
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="categoryId">شناسه سرفصل (اختیاری)</param>
        /// <param name="serviceId">شناسه خدمت (اختیاری)</param>
        /// <returns>اطلاعات کامل cascade</returns>
        [HttpPost]
        [Route("LoadServiceCascade")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LoadServiceCascade(int departmentId, int? categoryId = null, int? serviceId = null)
        {
            try
            {
                _logger.Information("🔄 بارگذاری cascade خدمات برای فرم پذیرش. DepartmentId: {DepartmentId}, CategoryId: {CategoryId}, ServiceId: {ServiceId}, User: {UserName}", 
                    departmentId, categoryId, serviceId, _currentUserService.UserName);

                var result = await _serviceManagementService.LoadServiceCascadeForReceptionAsync(departmentId, categoryId, serviceId);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بارگذاری cascade خدمات. DepartmentId: {DepartmentId}", departmentId);
                return Json(new { success = false, message = "خطا در بارگذاری cascade خدمات" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
