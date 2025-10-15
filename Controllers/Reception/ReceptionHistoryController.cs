using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر تخصصی مدیریت سوابق پذیرش
    /// </summary>
    [RoutePrefix("Reception/History")]
    public class ReceptionHistoryController : BaseController
    {
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionHistoryController(
            ILogger logger,
            ICurrentUserService currentUserService) : base(logger)
        {
            _logger = logger.ForContext<ReceptionHistoryController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Reception History Management

        /// <summary>
        /// دریافت سوابق پذیرش
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetReceptionHistory()
        {
            try
            {
                _logger.Information("🏥 دریافت سوابق پذیرش. کاربر: {UserName}", _currentUserService.UserName);

                // شبیه‌سازی سوابق پذیرش
                var history = new List<object>
                {
                    new { 
                        receptionId = 1, 
                        patientName = "احمد محمدی", 
                        nationalCode = "1234567890", 
                        serviceName = "معاینه قلب", 
                        doctorName = "دکتر احمدی", 
                        receptionDate = "1403/10/15", 
                        status = "completed", 
                        statusText = "تکمیل شده" 
                    },
                    new { 
                        receptionId = 2, 
                        patientName = "فاطمه احمدی", 
                        nationalCode = "0987654321", 
                        serviceName = "آزمایش خون", 
                        doctorName = "دکتر رضایی", 
                        receptionDate = "1403/10/14", 
                        status = "pending", 
                        statusText = "در انتظار" 
                    },
                    new { 
                        receptionId = 3, 
                        patientName = "علی حسینی", 
                        nationalCode = "1122334455", 
                        serviceName = "رادیولوژی", 
                        doctorName = "دکتر کریمی", 
                        receptionDate = "1403/10/13", 
                        status = "completed", 
                        statusText = "تکمیل شده" 
                    }
                };

                var result = new
                {
                    success = true,
                    data = history
                };

                _logger.Information("✅ سوابق پذیرش دریافت شد: تعداد={Count}", history.Count);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت سوابق پذیرش");
                return Json(new { success = false, message = "خطا در دریافت سوابق پذیرش" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// جستجوی سوابق پذیرش با فیلتر
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> SearchReceptionHistory(string dateFrom, string dateTo, string nationalCode, string status)
        {
            try
            {
                _logger.Information("🔍 جستجوی سوابق پذیرش. فیلترها: از={DateFrom}, تا={DateTo}, کد ملی={NationalCode}, وضعیت={Status}. کاربر: {UserName}", 
                    dateFrom, dateTo, nationalCode, status, _currentUserService.UserName);

                // شبیه‌سازی جستجو
                var allHistory = new List<object>
                {
                    new { 
                        receptionId = 1, 
                        patientName = "احمد محمدی", 
                        nationalCode = "1234567890", 
                        serviceName = "معاینه قلب", 
                        doctorName = "دکتر احمدی", 
                        receptionDate = "1403/10/15", 
                        status = "completed", 
                        statusText = "تکمیل شده" 
                    },
                    new { 
                        receptionId = 2, 
                        patientName = "فاطمه احمدی", 
                        nationalCode = "0987654321", 
                        serviceName = "آزمایش خون", 
                        doctorName = "دکتر رضایی", 
                        receptionDate = "1403/10/14", 
                        status = "pending", 
                        statusText = "در انتظار" 
                    }
                };

                // اعمال فیلترها
                var filteredHistory = allHistory.AsQueryable();

                if (!string.IsNullOrEmpty(nationalCode))
                {
                    filteredHistory = filteredHistory.Where(h => h.GetType().GetProperty("nationalCode").GetValue(h).ToString().Contains(nationalCode));
                }

                if (!string.IsNullOrEmpty(status))
                {
                    filteredHistory = filteredHistory.Where(h => h.GetType().GetProperty("status").GetValue(h).ToString() == status);
                }

                var result = new
                {
                    success = true,
                    data = filteredHistory.ToList()
                };

                _logger.Information("✅ جستجوی سوابق پذیرش موفق: تعداد={Count}", filteredHistory.Count());
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در جستجوی سوابق پذیرش");
                return Json(new { success = false, message = "خطا در جستجوی سوابق پذیرش" });
            }
        }

        /// <summary>
        /// دریافت جزئیات پذیرش
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetReceptionDetails(int receptionId)
        {
            try
            {
                _logger.Information("👁️ دریافت جزئیات پذیرش {ReceptionId}. کاربر: {UserName}", receptionId, _currentUserService.UserName);

                // شبیه‌سازی جزئیات پذیرش
                var details = new
                {
                    receptionId = receptionId,
                    patientName = "احمد محمدی",
                    nationalCode = "1234567890",
                    serviceName = "معاینه قلب",
                    doctorName = "دکتر احمدی",
                    receptionDate = "1403/10/15",
                    status = "completed",
                    statusText = "تکمیل شده",
                    amount = 150000,
                    insuranceShare = 105000,
                    patientShare = 45000
                };

                var result = new
                {
                    success = true,
                    data = details
                };

                _logger.Information("✅ جزئیات پذیرش {ReceptionId} دریافت شد", receptionId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت جزئیات پذیرش {ReceptionId}", receptionId);
                return Json(new { success = false, message = "خطا در دریافت جزئیات پذیرش" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
