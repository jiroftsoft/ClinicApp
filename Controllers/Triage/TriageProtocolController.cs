using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Triage;
using ClinicApp.ViewModels.Triage;
using Serilog;

namespace ClinicApp.Controllers.Triage
{
    /// <summary>
    /// کنترلر مدیریت پروتکل‌های تریاژ - SRP محور
    /// مدیریت پروتکل‌ها، قوانین و الگوریتم‌های تریاژ
    /// </summary>
    [Authorize]
    public class TriageProtocolController : BaseController
    {
        private readonly ITriageService _triageService;
        private readonly ILogger _logger;

        public TriageProtocolController(
            ITriageService triageService,
            ILogger logger) : base(logger)
        {
            _triageService = triageService;
            _logger = logger;
        }

        #region مدیریت پروتکل‌ها (Protocol Management)

        /// <summary>
        /// لیست پروتکل‌های تریاژ
        /// </summary>
        public async Task<ActionResult> Index(int? protocolType = null, string searchTerm = "")
        {
            try
            {
                var protocols = await _triageService.GetTriageProtocolsAsync(protocolType, searchTerm);
                if (!protocols.Success)
                {
                    TempData["Error"] = protocols.Message;
                    return View(new List<TriageProtocol>());
                }

                ViewBag.ProtocolType = protocolType;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.ProtocolTypes = GetProtocolTypes();

                return View(protocols.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پروتکل‌های تریاژ");
                TempData["Error"] = "خطا در دریافت لیست پروتکل‌ها";
                return View(new List<TriageProtocol>());
            }
        }

        /// <summary>
        /// جزئیات پروتکل تریاژ
        /// </summary>
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var protocol = await _triageService.GetTriageProtocolAsync(id);
                if (!protocol.Success)
                {
                    TempData["Error"] = protocol.Message;
                    return RedirectToAction("Index");
                }

                return View(protocol.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات پروتکل تریاژ {ProtocolId}", id);
                TempData["Error"] = "خطا در دریافت جزئیات پروتکل";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// فرم ایجاد پروتکل تریاژ
        /// </summary>
        public ActionResult Create()
        {
            var model = new TriageProtocolViewModel
            {
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            ViewBag.ProtocolTypes = GetProtocolTypes();
            return View(model);
        }

        /// <summary>
        /// ایجاد پروتکل تریاژ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TriageProtocolViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.ProtocolTypes = GetProtocolTypes();
                    return View(model);
                }

                var protocol = new TriageProtocol
                {
                    Name = model.Name,
                    Description = model.Description,
                    Type = model.ProtocolType,
                    Criteria = model.Criteria,
                    RequiredActions = model.Actions,
                    Priority = model.Priority,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = GetCurrentUserId()
                };

                var result = await _triageService.CreateTriageProtocolAsync(protocol);
                if (result.Success)
                {
                    TempData["Success"] = "پروتکل تریاژ با موفقیت ایجاد شد";
                    return RedirectToAction("Details", new { id = result.Data.TriageProtocolId });
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    ViewBag.ProtocolTypes = GetProtocolTypes();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد پروتکل تریاژ");
                ModelState.AddModelError("", "خطا در ایجاد پروتکل تریاژ");
                ViewBag.ProtocolTypes = GetProtocolTypes();
                return View(model);
            }
        }

        /// <summary>
        /// فرم ویرایش پروتکل تریاژ
        /// </summary>
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var protocol = await _triageService.GetTriageProtocolAsync(id);
                if (!protocol.Success)
                {
                    TempData["Error"] = protocol.Message;
                    return RedirectToAction("Index");
                }

                var model = new TriageProtocolViewModel
                {
                    TriageProtocolId = protocol.Data.TriageProtocolId,
                    Name = protocol.Data.Name,
                    Description = protocol.Data.Description,
                    ProtocolType = protocol.Data.ProtocolType,
                    Criteria = protocol.Data.Criteria,
                    Actions = protocol.Data.Actions,
                    Priority = protocol.Data.Priority,
                    IsActive = protocol.Data.IsActive,
                    CreatedAt = protocol.Data.CreatedAt,
                    UpdatedAt = protocol.Data.UpdatedAt
                };

                ViewBag.ProtocolTypes = GetProtocolTypes();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت فرم ویرایش پروتکل تریاژ {ProtocolId}", id);
                TempData["Error"] = "خطا در دریافت فرم ویرایش";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ویرایش پروتکل تریاژ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TriageProtocolViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.ProtocolTypes = GetProtocolTypes();
                    return View(model);
                }

                var result = await _triageService.UpdateTriageProtocolAsync(
                    model.TriageProtocolId,
                    model.Name,
                    model.Description,
                    model.ProtocolType,
                    model.Criteria,
                    model.Actions,
                    model.Priority,
                    model.IsActive);

                if (result.Success)
                {
                    TempData["Success"] = "پروتکل تریاژ با موفقیت به‌روزرسانی شد";
                    return RedirectToAction("Details", new { id = model.TriageProtocolId });
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    ViewBag.ProtocolTypes = GetProtocolTypes();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش پروتکل تریاژ {ProtocolId}", model.TriageProtocolId);
                ModelState.AddModelError("", "خطا در ویرایش پروتکل تریاژ");
                ViewBag.ProtocolTypes = GetProtocolTypes();
                return View(model);
            }
        }

        /// <summary>
        /// حذف پروتکل تریاژ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _triageService.DeleteTriageProtocolAsync(id);
                if (result.Success)
                {
                    TempData["Success"] = "پروتکل تریاژ با موفقیت حذف شد";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف پروتکل تریاژ {ProtocolId}", id);
                TempData["Error"] = "خطا در حذف پروتکل تریاژ";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region AJAX Actions

        /// <summary>
        /// دریافت پروتکل‌های فعال (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetActiveProtocols(int? protocolType = null)
        {
            try
            {
                var result = await _triageService.GetActiveProtocolsAsync(protocolType);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پروتکل‌های فعال");
                return Json(new { success = false, message = "خطا در دریافت پروتکل‌های فعال" });
            }
        }

        /// <summary>
        /// اعمال پروتکل به ارزیابی (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ApplyProtocolToAssessment(int assessmentId, int protocolId)
        {
            try
            {
                var result = await _triageService.ApplyProtocolAsync(assessmentId, protocolId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعمال پروتکل به ارزیابی {AssessmentId}, {ProtocolId}", assessmentId, protocolId);
                return Json(new { success = false, message = "خطا در اعمال پروتکل" });
            }
        }

        /// <summary>
        /// دریافت پیشنهادات پروتکل (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetProtocolSuggestions(int assessmentId)
        {
            try
            {
                var result = await _triageService.GetProtocolSuggestionsAsync(assessmentId);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پیشنهادات پروتکل {AssessmentId}", assessmentId);
                return Json(new { success = false, message = "خطا در دریافت پیشنهادات" });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت انواع پروتکل‌ها
        /// </summary>
        private List<LookupItemViewModel> GetProtocolTypes()
        {
            return new List<LookupItemViewModel>
            {
                new LookupItemViewModel { Value = "", Text = "همه انواع" },
                new LookupItemViewModel { Value = "1", Text = "پروتکل عمومی" },
                new LookupItemViewModel { Value = "2", Text = "پروتکل قلبی" },
                new LookupItemViewModel { Value = "3", Text = "پروتکل تنفسی" },
                new LookupItemViewModel { Value = "4", Text = "پروتکل عصبی" },
                new LookupItemViewModel { Value = "5", Text = "پروتکل تروما" },
                new LookupItemViewModel { Value = "6", Text = "پروتکل اطفال" }
            };
        }

        /// <summary>
        /// دریافت شناسه کاربر فعلی
        /// </summary>
        private string GetCurrentUserId()
        {
            // TODO: پیاده‌سازی دریافت شناسه کاربر فعلی
            return "system";
        }

        #endregion
    }

}
