using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Services;
using ClinicApp.ViewModels.Insurance.BusinessRule;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.Insurance
{
    /// <summary>
    /// کنترلر مدیریت قوانین کسب‌وکار بیمه
    /// طراحی شده برای محیط درمانی و پروداکشن
    /// </summary>
    //[Authorize]
    public class BusinessRuleController : BaseController
    {
        private readonly IBusinessRuleEngine _businessRuleEngine;
        private readonly IBusinessRuleRepository _businessRuleRepository;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public BusinessRuleController(
            IBusinessRuleEngine businessRuleEngine,
            IBusinessRuleRepository businessRuleRepository,
            ILogger logger,
            ICurrentUserService currentUserService,
            IMessageNotificationService messageNotificationService)
            : base(messageNotificationService)
        {
            _businessRuleEngine = businessRuleEngine ?? throw new ArgumentNullException(nameof(businessRuleEngine));
            _businessRuleRepository = businessRuleRepository ?? throw new ArgumentNullException(nameof(businessRuleRepository));
            _log = logger.ForContext<BusinessRuleController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// صفحه اصلی مدیریت قوانین کسب‌وکار
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                _log.Information("🏥 MEDICAL: دسترسی به صفحه مدیریت قوانین کسب‌وکار. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var rules = await _businessRuleRepository.GetAllAsync();
                var viewModel = new BusinessRuleIndexViewModel
                {
                    BusinessRules = rules.Select(r => new BusinessRuleItemViewModel
                    {
                        BusinessRuleId = r.BusinessRuleId,
                        RuleName = r.RuleName,
                        Description = r.Description,
                        RuleType = r.RuleType,
                        IsActive = r.IsActive,
                        Priority = r.Priority,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت لیست قوانین کسب‌وکار. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                TempData["ErrorMessage"] = "خطا در دریافت لیست قوانین کسب‌وکار";
                return View(new BusinessRuleIndexViewModel());
            }
        }

        /// <summary>
        /// صفحه ایجاد قانون جدید
        /// </summary>
        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                _log.Information("🏥 MEDICAL: دسترسی به صفحه ایجاد قانون جدید. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var viewModel = new BusinessRuleCreateEditViewModel
                {
                    RuleType = BusinessRuleType.CoveragePercent,
                    IsActive = true,
                    Priority = 1
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در نمایش صفحه ایجاد قانون. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                TempData["ErrorMessage"] = "خطا در نمایش صفحه ایجاد قانون";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ایجاد قانون جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(BusinessRuleCreateEditViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ایجاد قانون جدید. Name: {Name}, Type: {Type}. User: {UserName} (Id: {UserId})",
                    model.RuleName, model.RuleType, _currentUserService.UserName, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    _log.Warning("🏥 MEDICAL: اعتبارسنجی فرم ناموفق. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    return View(model);
                }

                var businessRule = new BusinessRule
                {
                    RuleName = model.RuleName,
                    Description = model.Description,
                    RuleType = model.RuleType,
                    Conditions = model.Conditions,
                    Actions = model.Actions,
                    IsActive = model.IsActive,
                    Priority = model.Priority,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId
                };

                await _businessRuleRepository.AddAsync(businessRule);
                await _businessRuleRepository.SaveChangesAsync();

                _log.Information("🏥 MEDICAL: قانون جدید با موفقیت ایجاد شد. Id: {Id}, Name: {Name}. User: {UserName} (Id: {UserId})",
                    businessRule.BusinessRuleId, businessRule.RuleName, _currentUserService.UserName, _currentUserService.UserId);

                TempData["SuccessMessage"] = "قانون جدید با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد قانون جدید. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                TempData["ErrorMessage"] = "خطا در ایجاد قانون جدید";
                return View(model);
            }
        }

        /// <summary>
        /// صفحه ویرایش قانون
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ویرایش قانون. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var businessRule = await _businessRuleRepository.GetByIdAsync(id);
                if (businessRule == null)
                {
                    _log.Warning("🏥 MEDICAL: قانون یافت نشد. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    
                    TempData["ErrorMessage"] = "قانون مورد نظر یافت نشد";
                    return RedirectToAction("Index");
                }

                var viewModel = new BusinessRuleCreateEditViewModel
                {
                    BusinessRuleId = businessRule.BusinessRuleId,
                    RuleName = businessRule.RuleName,
                    Description = businessRule.Description,
                    RuleType = businessRule.RuleType,
                    Conditions = businessRule.Conditions,
                    Actions = businessRule.Actions,
                    IsActive = businessRule.IsActive,
                    Priority = businessRule.Priority
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت قانون برای ویرایش. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                
                TempData["ErrorMessage"] = "خطا در دریافت قانون";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ویرایش قانون
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(BusinessRuleCreateEditViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ویرایش قانون. Id: {Id}, Name: {Name}. User: {UserName} (Id: {UserId})",
                    model.BusinessRuleId, model.RuleName, _currentUserService.UserName, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    _log.Warning("🏥 MEDICAL: اعتبارسنجی فرم ناموفق. Id: {Id}. User: {UserName} (Id: {UserId})",
                        model.BusinessRuleId, _currentUserService.UserName, _currentUserService.UserId);
                    return View(model);
                }

                var businessRule = await _businessRuleRepository.GetByIdAsync(model.BusinessRuleId.Value);
                if (businessRule == null)
                {
                    _log.Warning("🏥 MEDICAL: قانون یافت نشد برای ویرایش. Id: {Id}. User: {UserName} (Id: {UserId})",
                        model.BusinessRuleId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    TempData["ErrorMessage"] = "قانون مورد نظر یافت نشد";
                    return RedirectToAction("Index");
                }

                businessRule.RuleName = model.RuleName;
                businessRule.Description = model.Description;
                businessRule.RuleType = model.RuleType;
                businessRule.Conditions = model.Conditions;
                businessRule.Actions = model.Actions;
                businessRule.IsActive = model.IsActive;
                businessRule.Priority = model.Priority;
                businessRule.UpdatedAt = DateTime.UtcNow;
                businessRule.UpdatedByUserId = _currentUserService.UserId;

                await _businessRuleRepository.UpdateAsync(businessRule);
                await _businessRuleRepository.SaveChangesAsync();

                _log.Information("🏥 MEDICAL: قانون با موفقیت ویرایش شد. Id: {Id}, Name: {Name}. User: {UserName} (Id: {UserId})",
                    businessRule.BusinessRuleId, businessRule.RuleName, _currentUserService.UserName, _currentUserService.UserId);

                TempData["SuccessMessage"] = "قانون با موفقیت ویرایش شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ویرایش قانون. Id: {Id}. User: {UserName} (Id: {UserId})",
                    model.BusinessRuleId, _currentUserService.UserName, _currentUserService.UserId);
                
                TempData["ErrorMessage"] = "خطا در ویرایش قانون";
                return View(model);
            }
        }

        /// <summary>
        /// صفحه تأیید حذف قانون
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست نمایش صفحه حذف قانون. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var businessRule = await _businessRuleRepository.GetByIdAsync(id);
                if (businessRule == null)
                {
                    _log.Warning("🏥 MEDICAL: قانون یافت نشد برای نمایش صفحه حذف. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    
                    TempData["ErrorMessage"] = "قانون مورد نظر یافت نشد";
                    return RedirectToAction("Index");
                }

                var viewModel = new BusinessRuleCreateEditViewModel
                {
                    BusinessRuleId = businessRule.BusinessRuleId,
                    RuleName = businessRule.RuleName,
                    Description = businessRule.Description,
                    RuleType = businessRule.RuleType,
                    Conditions = businessRule.Conditions,
                    Actions = businessRule.Actions,
                    IsActive = businessRule.IsActive,
                    Priority = businessRule.Priority
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در نمایش صفحه حذف قانون. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                
                TempData["ErrorMessage"] = "خطا در نمایش صفحه حذف قانون";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// حذف قانون
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست حذف قانون. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var businessRule = await _businessRuleRepository.GetByIdAsync(id);
                if (businessRule == null)
                {
                    _log.Warning("🏥 MEDICAL: قانون یافت نشد برای حذف. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = "قانون مورد نظر یافت نشد" });
                }

                await _businessRuleRepository.DeleteAsync(id, _currentUserService.UserId);
                await _businessRuleRepository.SaveChangesAsync();

                _log.Information("🏥 MEDICAL: قانون با موفقیت حذف شد. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["SuccessMessage"] = "قانون با موفقیت حذف شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در حذف قانون. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                
                TempData["ErrorMessage"] = "خطا در حذف قانون";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// تغییر وضعیت فعال/غیرفعال قانون
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleStatus(int id)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست تغییر وضعیت قانون. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var businessRule = await _businessRuleRepository.GetByIdAsync(id);
                if (businessRule == null)
                {
                    _log.Warning("🏥 MEDICAL: قانون یافت نشد برای تغییر وضعیت. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = "قانون مورد نظر یافت نشد" });
                }

                businessRule.IsActive = !businessRule.IsActive;
                businessRule.UpdatedAt = DateTime.UtcNow;
                businessRule.UpdatedByUserId = _currentUserService.UserId;

                await _businessRuleRepository.UpdateAsync(businessRule);
                await _businessRuleRepository.SaveChangesAsync();

                _log.Information("🏥 MEDICAL: وضعیت قانون تغییر کرد. Id: {Id}, IsActive: {IsActive}. User: {UserName} (Id: {UserId})",
                    id, businessRule.IsActive, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { 
                    success = true, 
                    message = businessRule.IsActive ? "قانون فعال شد" : "قانون غیرفعال شد",
                    isActive = businessRule.IsActive
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در تغییر وضعیت قانون. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در تغییر وضعیت قانون" });
            }
        }

        /// <summary>
        /// تست قانون
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> TestRule(int id, decimal serviceAmount, int patientId, int serviceId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست تست قانون. RuleId: {RuleId}, ServiceAmount: {ServiceAmount}, PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    id, serviceAmount, patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                var businessRule = await _businessRuleRepository.GetByIdAsync(id);
                if (businessRule == null)
                {
                    return Json(new { success = false, message = "قانون مورد نظر یافت نشد" });
                }

                // ایجاد context برای تست
                var context = new InsuranceCalculationContext
                {
                    ServiceAmount = serviceAmount,
                    PatientId = patientId,
                    ServiceId = serviceId,
                    CalculationDate = DateTime.Now
                };

                var result = await _businessRuleEngine.EvaluateRuleAsync(businessRule, context);

                _log.Information("🏥 MEDICAL: تست قانون تکمیل شد. RuleId: {RuleId}, Result: {Result}. User: {UserName} (Id: {UserId})",
                    id, result, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { 
                    success = true, 
                    result = result,
                    message = result ? "قانون اعمال می‌شود" : "قانون اعمال نمی‌شود"
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در تست قانون. RuleId: {RuleId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در تست قانون" });
            }
        }
    }
}
