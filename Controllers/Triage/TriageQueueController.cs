using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Services.Triage;
using ClinicApp.ViewModels.Triage;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Controllers.Triage
{
    /// <summary>
    /// کنترلر مدیریت صف تریاژ - SRP محور
    /// </summary>
    [Authorize]
    public class TriageQueueController : BaseController
    {
        private readonly ITriageQueueService _queueService;
        private readonly ITriageService _triageService;
        private readonly ILogger _logger;

        public TriageQueueController(
            ITriageQueueService queueService,
            ITriageService triageService,
            ILogger logger) : base(logger)
        {
            _queueService = queueService;
            _triageService = triageService;
            _logger = logger;
        }

        #region مدیریت صف (Queue Management)

        /// <summary>
        /// لیست صف تریاژ
        /// </summary>
        public async Task<ActionResult> Index(int? departmentId = null, string status = "Waiting", string priority = "All", string level = "All", string searchTerm = "")
        {
            try
            {
                var model = new TriageQueueViewModel
                {
                    DepartmentId = departmentId,
                    Status = status,
                    Priority = priority,
                    Level = level,
                    SearchTerm = searchTerm
                };

                // دریافت لیست صف انتظار
                var queueResult = await _queueService.GetWaitingAsync(departmentId);
                if (queueResult.Success)
                {
                    model.QueueItems = queueResult.Data;
                }

                // دریافت آمار صف
                var statsResult = await _queueService.GetQueueStatsAsync(departmentId);
                if (statsResult.Success)
                {
                    model.Stats = new ViewModels.Triage.TriageQueueStats
                    {
                        WaitingCount = statsResult.Data.TotalWaiting,
                        CalledCount = statsResult.Data.TotalInProgress,
                        CompletedCount = statsResult.Data.TotalCompleted,
                        AverageWaitTimeMinutes = statsResult.Data.AverageWaitTimeMinutes,
                        LastUpdated = statsResult.Data.LastUpdated
                    };
                }

                // فیلتر کردن نتایج
                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    model.QueueItems = model.QueueItems.Where(q => q.Status == status).ToList();
                }

                if (!string.IsNullOrEmpty(priority) && priority != "All")
                {
                    var priorityValue = int.Parse(priority);
                    model.QueueItems = model.QueueItems.Where(q => q.Priority == priorityValue).ToList();
                }

                if (!string.IsNullOrEmpty(level) && level != "All")
                {
                    if (Enum.TryParse<TriageLevel>(level, out var levelEnum))
                    {
                        model.QueueItems = model.QueueItems.Where(q => q.Level == levelEnum).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    model.QueueItems = model.QueueItems.Where(q => 
                        q.PatientFullName.Contains(searchTerm) || 
                        q.ChiefComplaint.Contains(searchTerm)).ToList();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست صف تریاژ");
                TempData["Error"] = "خطا در دریافت لیست صف";
                return View(new TriageQueueViewModel());
            }
        }

        /// <summary>
        /// فراخوانی بیمار بعدی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CallNext(int? departmentId = null)
        {
            try
            {
                var result = await _queueService.CallNextAsync(departmentId);
                if (result.Success)
                {
                    TempData["Success"] = "بیمار فراخوانی شد";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction("Index", new { departmentId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فراخوانی بیمار بعدی");
                TempData["Error"] = "خطا در فراخوانی بیمار";
                return RedirectToAction("Index", new { departmentId });
            }
        }

        /// <summary>
        /// تکمیل صف
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Complete(int queueId)
        {
            try
            {
                var result = await _queueService.CompleteAsync(queueId);
                if (result.Success)
                {
                    TempData["Success"] = "صف تریاژ تکمیل شد";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تکمیل صف تریاژ {QueueId}", queueId);
                TempData["Error"] = "خطا در تکمیل صف";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// مرتب‌سازی صف بر اساس اولویت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reorder(int? departmentId = null)
        {
            try
            {
                var result = await _queueService.ReorderQueueByPriorityAsync(departmentId);
                if (result.Success)
                {
                    TempData["Success"] = "صف بر اساس اولویت مرتب شد";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction("Index", new { departmentId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مرتب‌سازی صف تریاژ");
                TempData["Error"] = "خطا در مرتب‌سازی صف";
                return RedirectToAction("Index", new { departmentId });
            }
        }

        #endregion

        #region AJAX Actions

        /// <summary>
        /// دریافت لیست صف (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetQueueList(int? departmentId = null)
        {
            try
            {
                var result = await _queueService.GetWaitingAsync(departmentId);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست صف");
                return Json(new { success = false, message = "خطا در دریافت لیست صف" });
            }
        }

        /// <summary>
        /// دریافت آمار صف (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetQueueStats(int? departmentId = null)
        {
            try
            {
                var result = await _queueService.GetQueueStatsAsync(departmentId);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار صف");
                return Json(new { success = false, message = "خطا در دریافت آمار صف" });
            }
        }

        /// <summary>
        /// فراخوانی بیمار بعدی (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CallNextAjax(int? departmentId = null)
        {
            try
            {
                var result = await _queueService.CallNextAsync(departmentId);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فراخوانی بیمار بعدی");
                return Json(new { success = false, message = "خطا در فراخوانی بیمار" });
            }
        }

        /// <summary>
        /// تکمیل صف (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CompleteAjax(int queueId)
        {
            try
            {
                var result = await _queueService.CompleteAsync(queueId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تکمیل صف تریاژ {QueueId}", queueId);
                return Json(new { success = false, message = "خطا در تکمیل صف" });
            }
        }

        /// <summary>
        /// مرتب‌سازی صف (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ReorderAjax(int? departmentId = null)
        {
            try
            {
                var result = await _queueService.ReorderQueueByPriorityAsync(departmentId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مرتب‌سازی صف تریاژ");
                return Json(new { success = false, message = "خطا در مرتب‌سازی صف" });
            }
        }

        #endregion
    }
}
