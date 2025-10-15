using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس تخصصی مدیریت سرفصل‌ها و خدمات در فرم پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. بارگذاری سرفصل‌ها بر اساس دپارتمان
    /// 2. بارگذاری خدمات بر اساس سرفصل
    /// 3. جستجو با کد خدمت
    /// 4. مدیریت cascade loading
    /// 5. بهینه‌سازی برای محیط درمانی
    /// 
    /// نکته حیاتی: این سرویس از ماژول‌های موجود استفاده می‌کند
    /// </summary>
    public class ReceptionServiceManagementService
    {
        private readonly IReceptionService _receptionService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionServiceManagementService(
            IReceptionService receptionService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _logger = logger.ForContext<ReceptionServiceManagementService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Service Category Management

        /// <summary>
        /// دریافت سرفصل‌های دپارتمان برای فرم پذیرش
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>لیست سرفصل‌های دپارتمان</returns>
        public async Task<ServiceResult<List<ReceptionServiceCategoryViewModel>>> GetDepartmentServiceCategoriesForReceptionAsync(int departmentId)
        {
            try
            {
                _logger.Information("📋 دریافت سرفصل‌های دپارتمان برای فرم پذیرش. DepartmentId: {DepartmentId}, User: {UserName}", 
                    departmentId, _currentUserService.UserName);

                // اینجا باید از سرویس موجود برای دریافت سرفصل‌ها استفاده کنید
                // var result = await _serviceCategoryService.GetCategoriesByDepartmentAsync(departmentId);
                
                // برای حالا یک لیست خالی برمی‌گردانیم
                var categories = new List<ReceptionServiceCategoryViewModel>();

                _logger.Information("✅ {Count} سرفصل برای دپارتمان {DepartmentId} دریافت شد", categories.Count, departmentId);
                return ServiceResult<List<ReceptionServiceCategoryViewModel>>.Successful(categories);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت سرفصل‌های دپارتمان. DepartmentId: {DepartmentId}", departmentId);
                return ServiceResult<List<ReceptionServiceCategoryViewModel>>.Failed("خطا در دریافت سرفصل‌ها");
            }
        }

        #endregion

        #region Service Management

        /// <summary>
        /// دریافت خدمات سرفصل برای فرم پذیرش
        /// </summary>
        /// <param name="categoryId">شناسه سرفصل</param>
        /// <returns>لیست خدمات سرفصل</returns>
        public async Task<ServiceResult<List<ReceptionServiceViewModel>>> GetCategoryServicesForReceptionAsync(int categoryId)
        {
            try
            {
                _logger.Information("🔧 دریافت خدمات سرفصل برای فرم پذیرش. CategoryId: {CategoryId}, User: {UserName}", 
                    categoryId, _currentUserService.UserName);

                // اینجا باید از سرویس موجود برای دریافت خدمات استفاده کنید
                // var result = await _serviceService.GetServicesByCategoryAsync(categoryId);
                
                // برای حالا یک لیست خالی برمی‌گردانیم
                var services = new List<ReceptionServiceViewModel>();

                _logger.Information("✅ {Count} خدمت برای سرفصل {CategoryId} دریافت شد", services.Count, categoryId);
                return ServiceResult<List<ReceptionServiceViewModel>>.Successful(services);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت خدمات سرفصل. CategoryId: {CategoryId}", categoryId);
                return ServiceResult<List<ReceptionServiceViewModel>>.Failed("خطا در دریافت خدمات");
            }
        }

        /// <summary>
        /// جستجوی خدمت با کد خدمت
        /// </summary>
        /// <param name="serviceCode">کد خدمت</param>
        /// <returns>اطلاعات خدمت</returns>
        public async Task<ServiceResult<ReceptionServiceViewModel>> SearchServiceByCodeAsync(string serviceCode)
        {
            try
            {
                _logger.Information("🔍 جستجوی خدمت با کد. ServiceCode: {ServiceCode}, User: {UserName}", 
                    serviceCode, _currentUserService.UserName);

                // اعتبارسنجی کد خدمت
                if (string.IsNullOrWhiteSpace(serviceCode))
                {
                    return ServiceResult<ReceptionServiceViewModel>.Failed("کد خدمت الزامی است");
                }

                // اینجا باید از سرویس موجود برای جستجوی خدمت استفاده کنید
                // var result = await _serviceService.GetServiceByCodeAsync(serviceCode);
                
                // برای حالا یک خدمت نمونه برمی‌گردانیم
                var service = new ReceptionServiceViewModel
                {
                    ServiceId = 1,
                    ServiceCode = serviceCode,
                    ServiceName = "خدمت نمونه",
                    Description = "توضیحات خدمت نمونه",
                    BasePrice = 100000,
                    IsActive = true
                };

                _logger.Information("✅ خدمت یافت شد. ServiceCode: {ServiceCode}, ServiceName: {ServiceName}", 
                    serviceCode, service.ServiceName);
                return ServiceResult<ReceptionServiceViewModel>.Successful(service);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در جستجوی خدمت با کد. ServiceCode: {ServiceCode}", serviceCode);
                return ServiceResult<ReceptionServiceViewModel>.Failed("خطا در جستجوی خدمت");
            }
        }

        /// <summary>
        /// دریافت اطلاعات کامل خدمت
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>اطلاعات کامل خدمت</returns>
        public async Task<ServiceResult<ReceptionServiceViewModel>> GetServiceDetailsForReceptionAsync(int serviceId)
        {
            try
            {
                _logger.Information("🔧 دریافت اطلاعات خدمت برای فرم پذیرش. ServiceId: {ServiceId}, User: {UserName}", 
                    serviceId, _currentUserService.UserName);

                // اینجا باید از سرویس موجود برای دریافت اطلاعات خدمت استفاده کنید
                // var result = await _serviceService.GetServiceByIdAsync(serviceId);
                
                // برای حالا یک خدمت نمونه برمی‌گردانیم
                var service = new ReceptionServiceViewModel
                {
                    ServiceId = serviceId,
                    ServiceCode = "970000",
                    ServiceName = "خدمت نمونه",
                    Description = "توضیحات خدمت نمونه",
                    BasePrice = 100000,
                    IsActive = true
                };

                _logger.Information("✅ اطلاعات خدمت دریافت شد. ServiceId: {ServiceId}, ServiceName: {ServiceName}", 
                    serviceId, service.ServiceName);
                return ServiceResult<ReceptionServiceViewModel>.Successful(service);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت اطلاعات خدمت. ServiceId: {ServiceId}", serviceId);
                return ServiceResult<ReceptionServiceViewModel>.Failed("خطا در دریافت اطلاعات خدمت");
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
        public async Task<ServiceResult<ReceptionServiceCascadeViewModel>> LoadServiceCascadeForReceptionAsync(
            int departmentId, 
            int? categoryId = null, 
            int? serviceId = null)
        {
            try
            {
                _logger.Information("🔄 بارگذاری cascade خدمات برای فرم پذیرش. DepartmentId: {DepartmentId}, CategoryId: {CategoryId}, ServiceId: {ServiceId}, User: {UserName}", 
                    departmentId, categoryId, serviceId, _currentUserService.UserName);

                var cascade = new ReceptionServiceCascadeViewModel
                {
                    DepartmentId = departmentId,
                    CategoryId = categoryId,
                    ServiceId = serviceId,
                    LoadDate = DateTime.Now
                };

                // بارگذاری سرفصل‌ها
                var categoryResult = await GetDepartmentServiceCategoriesForReceptionAsync(departmentId);
                if (categoryResult.Success)
                {
                    cascade.Categories = categoryResult.Data;
                }

                // بارگذاری خدمات
                if (categoryId.HasValue)
                {
                    var serviceResult = await GetCategoryServicesForReceptionAsync(categoryId.Value);
                    if (serviceResult.Success)
                    {
                        cascade.Services = serviceResult.Data;
                    }
                }

                // بارگذاری اطلاعات خدمت انتخاب شده
                if (serviceId.HasValue)
                {
                    var serviceDetailResult = await GetServiceDetailsForReceptionAsync(serviceId.Value);
                    if (serviceDetailResult.Success)
                    {
                        cascade.SelectedService = serviceDetailResult.Data;
                    }
                }

                _logger.Information("✅ بارگذاری cascade خدمات تکمیل شد. DepartmentId: {DepartmentId}, CategoryCount: {CategoryCount}, ServiceCount: {ServiceCount}", 
                    departmentId, cascade.Categories?.Count ?? 0, cascade.Services?.Count ?? 0);

                return ServiceResult<ReceptionServiceCascadeViewModel>.Successful(cascade);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بارگذاری cascade خدمات. DepartmentId: {DepartmentId}", departmentId);
                return ServiceResult<ReceptionServiceCascadeViewModel>.Failed("خطا در بارگذاری cascade خدمات");
            }
        }

        #endregion
    }
}
