using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Constants;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر مدیریت خدمات در ماژول پذیرش
    /// </summary>
    public class ReceptionServiceManagementController : BaseController
    {
        private readonly IReceptionServiceManagementService _serviceManagementService;
        private readonly ILogger _logger;

        public ReceptionServiceManagementController(
            IReceptionServiceManagementService serviceManagementService,
            ILogger logger) : base(logger)
        {
            _serviceManagementService = serviceManagementService;
            _logger = logger;
        }

        /// <summary>
        /// دریافت دسته‌بندی‌های خدمات
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceCategories()
        {
            try
            {
                _logger.Information("دریافت دسته‌بندی‌های خدمات");

                var result = await _serviceManagementService.GetServiceCategoriesAsync();

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "دسته‌بندی‌ها با موفقیت بارگذاری شدند"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دسته‌بندی‌های خدمات");
                return Json(new
                {
                    success = false,
                    message = "خطا در بارگذاری دسته‌بندی‌ها"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت تمام خدمات
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetAllServices()
        {
            try
            {
                _logger.Information("دریافت تمام خدمات");

                var result = await _serviceManagementService.GetAllServicesAsync();

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "خدمات با موفقیت بارگذاری شدند"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات");
                return Json(new
                {
                    success = false,
                    message = "خطا در بارگذاری خدمات"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت خدمات بر اساس دسته‌بندی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServicesByCategory(int categoryId)
        {
            try
            {
                _logger.Information("دریافت خدمات برای دسته‌بندی {CategoryId}", categoryId);

                var result = await _serviceManagementService.GetServicesByCategoryAsync(categoryId);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "خدمات با موفقیت بارگذاری شدند"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات دسته‌بندی {CategoryId}", categoryId);
                return Json(new
                {
                    success = false,
                    message = "خطا در بارگذاری خدمات"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// جستجوی خدمات
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> SearchServices(string searchTerm)
        {
            try
            {
                _logger.Information("جستجوی خدمات با عبارت: {SearchTerm}", searchTerm);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Json(new
                    {
                        success = false,
                        message = "عبارت جستجو نمی‌تواند خالی باشد"
                    }, JsonRequestBehavior.AllowGet);
                }

                var result = await _serviceManagementService.SearchServicesAsync(searchTerm);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "نتایج جستجو با موفقیت بارگذاری شدند"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی خدمات");
                return Json(new
                {
                    success = false,
                    message = "خطا در جستجوی خدمات"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// محاسبه هزینه خدمات
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateServiceCosts(List<int> services, string patientId)
        {
            try
            {
                _logger.Information("محاسبه هزینه خدمات برای {ServiceCount} خدمت", services?.Count ?? 0);

                if (services == null || !services.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "هیچ خدمتی انتخاب نشده است"
                    });
                }

                var request = new ServiceCalculationRequest
                {
                    ServiceIds = services,
                    PatientId = int.Parse(patientId)
                };
                var result = await _serviceManagementService.CalculateServiceCostsAsync(request);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "محاسبه با موفقیت انجام شد"
                    });
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه هزینه خدمات");
                return Json(new
                {
                    success = false,
                    message = "خطا در محاسبه هزینه‌ها"
                });
            }
        }

        /// <summary>
        /// دریافت جزئیات خدمت
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceDetails(int serviceId)
        {
            try
            {
                _logger.Information("دریافت جزئیات خدمت {ServiceId}", serviceId);

                var result = await _serviceManagementService.GetServiceDetailsAsync(serviceId);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "جزئیات خدمت با موفقیت بارگذاری شد"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات خدمت {ServiceId}", serviceId);
                return Json(new
                {
                    success = false,
                    message = "خطا در بارگذاری جزئیات خدمت"
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}