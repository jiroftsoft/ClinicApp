using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers.Base;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// API Controller مدیریت خدمات در پذیرش
    /// 
    /// Responsibilities:
    /// 1. دریافت دسته‌بندی خدمات
    /// 2. دریافت خدمات بر اساس دسته‌بندی
    /// 3. محاسبه قیمت خدمات
    /// 4. دریافت جزئیات محاسبه
    /// 5. وضعیت اجزای خدمت
    /// 
    /// Architecture:
    /// ✅ Single Responsibility: فقط Service
    /// ✅ No Cache: طبق سیاست
    /// ✅ Conditional Authorization
    /// </summary>
    public class ServiceController : ReceptionBaseController
    {
        #region Fields

        private readonly ApplicationDbContext _context;
        private readonly IServiceCalculationService _serviceCalculationService;

        #endregion

        #region Constructor

        public ServiceController(
            ApplicationDbContext context,
            IServiceCalculationService serviceCalculationService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(currentUserService, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serviceCalculationService = serviceCalculationService ?? 
                throw new ArgumentNullException(nameof(serviceCalculationService));
        }

        #endregion

        #region Service Category Actions

        /// <summary>
        /// دریافت دسته‌بندی خدمات
        /// </summary>
        /// <returns>لیست دسته‌بندی‌های فعال</returns>
        [HttpGet]
        public async Task<JsonResult> GetCategories()
        {
            using (StartPerformanceMonitoring("GetServiceCategories"))
            {
                try
                {
                    _logger.Information("📂 دریافت دسته‌بندی خدمات. کاربر: {UserName}", 
                        _currentUserService.UserName);

                    AddSecurityHeaders();

                    var categories = await _context.ServiceCategories
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.Title)
                        .Select(c => new
                        {
                            c.ServiceCategoryId,
                            c.Title,
                            c.Description,
                            ServiceCount = c.Services.Count(s => s.IsActive)
                        })
                        .ToListAsync();

                    _logger.Information("✅ {Count} دسته‌بندی خدمات دریافت شد", categories.Count);

                    return SuccessResponse(categories, 
                        $"{categories.Count} دسته‌بندی خدمات دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetServiceCategories");
                }
            }
        }

        /// <summary>
        /// دریافت دسته‌بندی خدمات بر اساس دپارتمان‌ها
        /// </summary>
        /// <param name="departmentIds">شناسه‌های دپارتمان (با کاما جدا شده)</param>
        /// <returns>لیست دسته‌بندی‌های مرتبط</returns>
        [HttpGet]
        public async Task<JsonResult> GetCategoriesByDepartments(string departmentIds)
        {
            using (StartPerformanceMonitoring("GetServiceCategoriesByDepartments"))
            {
                try
                {
                    _logger.Information(
                        "📂 دریافت دسته‌بندی خدمات بر اساس دپارتمان‌ها. دپارتمان‌ها: {DepartmentIds}, کاربر: {UserName}",
                        departmentIds, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (string.IsNullOrWhiteSpace(departmentIds))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه دپارتمان‌ها الزامی است"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var ids = departmentIds.Split(',')
                        .Select(id => int.TryParse(id.Trim(), out var result) ? result : 0)
                        .Where(id => id > 0)
                        .ToList();

                    if (!ids.Any())
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه دپارتمان‌ها نامعتبر است"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var categories = await _context.ServiceCategories
                        .Where(c => c.IsActive)
                        .Where(c => ids.Contains(c.DepartmentId))
                        .OrderBy(c => c.Title)
                        .Select(c => new
                        {
                            c.ServiceCategoryId,
                            c.Title,
                            c.Description,
                            ServiceCount = c.Services.Count(s => s.IsActive)
                        })
                        .ToListAsync();

                    _logger.Information("✅ {Count} دسته‌بندی خدمات دریافت شد", categories.Count);

                    return SuccessResponse(categories, 
                        $"{categories.Count} دسته‌بندی خدمات دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetServiceCategoriesByDepartments", 
                        new { departmentIds });
                }
            }
        }

        #endregion

        #region Service Actions

        /// <summary>
        /// دریافت خدمات بر اساس دسته‌بندی
        /// </summary>
        /// <param name="categoryId">شناسه دسته‌بندی</param>
        /// <returns>لیست خدمات</returns>
        [HttpGet]
        public async Task<JsonResult> GetByCategory(int categoryId)
        {
            using (StartPerformanceMonitoring("GetServicesByCategory"))
            {
                try
                {
                    _logger.Information(
                        "📋 دریافت خدمات بر اساس دسته‌بندی. دسته‌بندی: {CategoryId}, کاربر: {UserName}",
                        categoryId, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (categoryId <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه دسته‌بندی نامعتبر است"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var services = await _context.Services
                        .Where(s => s.IsActive && s.ServiceCategoryId == categoryId)
                        .OrderBy(s => s.ServiceCode)
                        .Select(s => new
                        {
                            s.ServiceId,
                            s.Title,
                            s.ServiceCode,
                            s.Description,
                            s.Price,
                            s.ServiceCategoryId,
                            CategoryName = s.ServiceCategory.Title,
                            s.IsActive
                        })
                        .ToListAsync();

                    _logger.Information("✅ {Count} خدمت دریافت شد", services.Count);

                    return SuccessResponse(services, $"{services.Count} خدمت دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetServicesByCategory", new { categoryId });
                }
            }
        }

        #endregion

        #region Service Calculation Actions

        /// <summary>
        /// محاسبه قیمت خدمت با اجزا
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <returns>قیمت محاسبه شده</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CalculatePrice(int serviceId, DateTime? calculationDate = null)
        {
            using (StartPerformanceMonitoring("CalculateServicePrice"))
            {
                try
                {
                    _logger.Information(
                        "💰 محاسبه قیمت خدمت. خدمت: {ServiceId}, تاریخ: {CalculationDate}, کاربر: {UserName}",
                        serviceId, calculationDate, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (serviceId <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه خدمت نامعتبر است"
                        });
                    }

                    var result = await _serviceCalculationService
                        .CalculateServicePriceWithComponentsAsync(serviceId, 0, _context);

                    if (!result.Success)
                    {
                        _logger.Warning("⚠️ خطا در محاسبه قیمت خدمت. پیام: {Message}", result.Message);
                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        });
                    }

                    _logger.Information("✅ محاسبه قیمت خدمت موفق. قیمت: {Price}", result.Data);

                    return SuccessResponse(result.Data, "محاسبه قیمت با موفقیت انجام شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "CalculateServicePrice", 
                        new { serviceId, calculationDate });
                }
            }
        }

        /// <summary>
        /// دریافت جزئیات محاسبه خدمت
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <returns>جزئیات محاسبه</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetCalculationDetails(int serviceId, DateTime? calculationDate = null)
        {
            using (StartPerformanceMonitoring("GetServiceCalculationDetails"))
            {
                try
                {
                    _logger.Information(
                        "📊 دریافت جزئیات محاسبه خدمت. خدمت: {ServiceId}, تاریخ: {CalculationDate}, کاربر: {UserName}",
                        serviceId, calculationDate, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (serviceId <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه خدمت نامعتبر است"
                        });
                    }

                    var result = await _serviceCalculationService
                        .GetServiceCalculationDetailsAsync(serviceId, 0, _context);

                    if (!result.Success)
                    {
                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        });
                    }

                    return SuccessResponse(result.Data, "جزئیات محاسبه با موفقیت دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetServiceCalculationDetails", 
                        new { serviceId, calculationDate });
                }
            }
        }

        /// <summary>
        /// دریافت وضعیت اجزای خدمت
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>وضعیت اجزا</returns>
        [HttpGet]
        public async Task<JsonResult> GetComponentsStatus(int serviceId)
        {
            using (StartPerformanceMonitoring("GetServiceComponentsStatus"))
            {
                try
                {
                    _logger.Information(
                        "🔍 دریافت وضعیت اجزای خدمت. خدمت: {ServiceId}, کاربر: {UserName}",
                        serviceId, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (serviceId <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه خدمت نامعتبر است"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var result = await _serviceCalculationService
                        .GetServiceComponentsStatusAsync(serviceId, 0, _context);

                    if (!result.Success)
                    {
                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return SuccessResponse(result.Data, "وضعیت اجزا با موفقیت دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetServiceComponentsStatus", new { serviceId });
                }
            }
        }

        #endregion
    }
}

