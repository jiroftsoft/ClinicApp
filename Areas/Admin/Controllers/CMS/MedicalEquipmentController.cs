using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت تجهیزات پزشکی (Medical Equipment)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class MedicalEquipmentController : Controller
    {
        private readonly IMedicalEquipmentService _medicalEquipmentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public MedicalEquipmentController(
            IMedicalEquipmentService medicalEquipmentService,
            ICurrentUserService currentUserService,
            ApplicationDbContext context)
        {
            _medicalEquipmentService = medicalEquipmentService ?? throw new ArgumentNullException(nameof(medicalEquipmentService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = Log.ForContext<MedicalEquipmentController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(MedicalEquipmentSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست تجهیزات پزشکی توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مدل جستجو
                if (searchModel == null)
                {
                    searchModel = new MedicalEquipmentSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _medicalEquipmentService.GetMedicalEquipmentsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست تجهیزات پزشکی: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(new PagedResult<MedicalEquipmentIndexViewModel>(new System.Collections.Generic.List<MedicalEquipmentIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize));
                }

                // بارگذاری دسته‌بندی‌ها برای فیلتر
                var categories = await _context.Set<MedicalEquipment>()
                    .Where(e => !e.IsDeleted)
                    .Select(e => e.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                ViewBag.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c,
                    Text = GetCategoryDisplayName(c)
                }).ToList();

                // بارگذاری وضعیت‌ها برای فیلتر
                ViewBag.Statuses = new SelectList(new[] { 
                    new SelectListItem { Text = "فعال", Value = "Active" },
                    new SelectListItem { Text = "تعمیر", Value = "Maintenance" },
                    new SelectListItem { Text = "غیرفعال", Value = "Inactive" }
                }, "Value", "Text");

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست تجهیزات پزشکی");
                TempData["Error"] = "خطا در بارگذاری لیست تجهیزات پزشکی";
                return View(new PagedResult<MedicalEquipmentIndexViewModel>(new System.Collections.Generic.List<MedicalEquipmentIndexViewModel>(), 0, 1, 10));
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _medicalEquipmentService.GetMedicalEquipmentDetailsAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات تجهیز پزشکی";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            try
            {
                // بارگذاری دسته‌بندی‌ها
                var categories = await _context.Set<MedicalEquipment>()
                    .Where(e => !e.IsDeleted)
                    .Select(e => e.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                ViewBag.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c,
                    Text = GetCategoryDisplayName(c)
                }).ToList();

                // اگر دسته‌بندی‌ای وجود ندارد، لیست پیش‌فرض
                if (!categories.Any())
                {
                    ViewBag.Categories = new SelectList(new[] { 
                        new SelectListItem { Text = "تصویربرداری", Value = "Imaging" },
                        new SelectListItem { Text = "آزمایشگاه", Value = "Laboratory" },
                        new SelectListItem { Text = "جراحی", Value = "Surgery" },
                        new SelectListItem { Text = "مانیتورینگ", Value = "Monitoring" },
                        new SelectListItem { Text = "درمانی", Value = "Therapy" },
                        new SelectListItem { Text = "تشخیصی", Value = "Diagnostic" },
                        new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                        new SelectListItem { Text = "توانبخشی", Value = "Rehabilitation" }
                    }, "Value", "Text");
                }

                ViewBag.Statuses = new SelectList(new[] { 
                    new SelectListItem { Text = "فعال", Value = "Active" },
                    new SelectListItem { Text = "تعمیر", Value = "Maintenance" },
                    new SelectListItem { Text = "غیرفعال", Value = "Inactive" }
                }, "Value", "Text", "Active");

                var model = new MedicalEquipmentCreateEditViewModel
                {
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 0,
                    Status = "Active"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد تجهیز پزشکی");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد تجهیز پزشکی";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MedicalEquipmentCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد تجهیز پزشکی جدید توسط کاربر {UserId}", _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    // بارگذاری مجدد ViewBag
                    var categories = await _context.Set<MedicalEquipment>()
                        .Where(e => !e.IsDeleted)
                        .Select(e => e.Category)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToListAsync();

                    ViewBag.Categories = categories.Select(c => new SelectListItem
                    {
                        Value = c,
                        Text = GetCategoryDisplayName(c)
                    }).ToList();

                    if (!categories.Any())
                    {
                        ViewBag.Categories = new SelectList(new[] { 
                            new SelectListItem { Text = "تصویربرداری", Value = "Imaging" },
                            new SelectListItem { Text = "آزمایشگاه", Value = "Laboratory" },
                            new SelectListItem { Text = "جراحی", Value = "Surgery" },
                            new SelectListItem { Text = "مانیتورینگ", Value = "Monitoring" },
                            new SelectListItem { Text = "درمانی", Value = "Therapy" },
                            new SelectListItem { Text = "تشخیصی", Value = "Diagnostic" },
                            new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                            new SelectListItem { Text = "توانبخشی", Value = "Rehabilitation" }
                        }, "Value", "Text", model.Category);
                    }

                    ViewBag.Statuses = new SelectList(new[] { 
                        new SelectListItem { Text = "فعال", Value = "Active" },
                        new SelectListItem { Text = "تعمیر", Value = "Maintenance" },
                        new SelectListItem { Text = "غیرفعال", Value = "Inactive" }
                    }, "Value", "Text", model.Status);

                    return View(model);
                }

                var result = await _medicalEquipmentService.CreateMedicalEquipmentAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد تجهیز پزشکی: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    
                    // بارگذاری مجدد ViewBag
                    var categories = await _context.Set<MedicalEquipment>()
                        .Where(e => !e.IsDeleted)
                        .Select(e => e.Category)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToListAsync();

                    ViewBag.Categories = categories.Select(c => new SelectListItem
                    {
                        Value = c,
                        Text = GetCategoryDisplayName(c)
                    }).ToList();

                    if (!categories.Any())
                    {
                        ViewBag.Categories = new SelectList(new[] { 
                            new SelectListItem { Text = "تصویربرداری", Value = "Imaging" },
                            new SelectListItem { Text = "آزمایشگاه", Value = "Laboratory" },
                            new SelectListItem { Text = "جراحی", Value = "Surgery" },
                            new SelectListItem { Text = "مانیتورینگ", Value = "Monitoring" },
                            new SelectListItem { Text = "درمانی", Value = "Therapy" },
                            new SelectListItem { Text = "تشخیصی", Value = "Diagnostic" },
                            new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                            new SelectListItem { Text = "توانبخشی", Value = "Rehabilitation" }
                        }, "Value", "Text", model.Category);
                    }

                    ViewBag.Statuses = new SelectList(new[] { 
                        new SelectListItem { Text = "فعال", Value = "Active" },
                        new SelectListItem { Text = "تعمیر", Value = "Maintenance" },
                        new SelectListItem { Text = "غیرفعال", Value = "Inactive" }
                    }, "Value", "Text", model.Status);

                    return View(model);
                }

                _logger.Information("تجهیز پزشکی با موفقیت ایجاد شد - MedicalEquipmentId: {MedicalEquipmentId}", result.Data.MedicalEquipmentId);
                TempData["Success"] = "تجهیز پزشکی با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تجهیز پزشکی");
                TempData["Error"] = "خطا در ایجاد تجهیز پزشکی";
                return View(model);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _medicalEquipmentService.GetMedicalEquipmentForEditAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                // بارگذاری دسته‌بندی‌ها
                var categories = await _context.Set<MedicalEquipment>()
                    .Where(e => !e.IsDeleted)
                    .Select(e => e.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                ViewBag.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c,
                    Text = GetCategoryDisplayName(c),
                    Selected = c == result.Data.Category
                }).ToList();

                if (!categories.Any())
                {
                    ViewBag.Categories = new SelectList(new[] { 
                        new SelectListItem { Text = "تصویربرداری", Value = "Imaging" },
                        new SelectListItem { Text = "آزمایشگاه", Value = "Laboratory" },
                        new SelectListItem { Text = "جراحی", Value = "Surgery" },
                        new SelectListItem { Text = "مانیتورینگ", Value = "Monitoring" },
                        new SelectListItem { Text = "درمانی", Value = "Therapy" },
                        new SelectListItem { Text = "تشخیصی", Value = "Diagnostic" },
                        new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                        new SelectListItem { Text = "توانبخشی", Value = "Rehabilitation" }
                    }, "Value", "Text", result.Data.Category);
                }

                ViewBag.Statuses = new SelectList(new[] { 
                    new SelectListItem { Text = "فعال", Value = "Active" },
                    new SelectListItem { Text = "تعمیر", Value = "Maintenance" },
                    new SelectListItem { Text = "غیرفعال", Value = "Inactive" }
                }, "Value", "Text", result.Data.Status);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش تجهیز پزشکی";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MedicalEquipmentCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", model.MedicalEquipmentId);

                if (!ModelState.IsValid)
                {
                    // بارگذاری مجدد ViewBag
                    var categories = await _context.Set<MedicalEquipment>()
                        .Where(e => !e.IsDeleted)
                        .Select(e => e.Category)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToListAsync();

                    ViewBag.Categories = categories.Select(c => new SelectListItem
                    {
                        Value = c,
                        Text = GetCategoryDisplayName(c),
                        Selected = c == model.Category
                    }).ToList();

                    if (!categories.Any())
                    {
                        ViewBag.Categories = new SelectList(new[] { 
                            new SelectListItem { Text = "تصویربرداری", Value = "Imaging" },
                            new SelectListItem { Text = "آزمایشگاه", Value = "Laboratory" },
                            new SelectListItem { Text = "جراحی", Value = "Surgery" },
                            new SelectListItem { Text = "مانیتورینگ", Value = "Monitoring" },
                            new SelectListItem { Text = "درمانی", Value = "Therapy" },
                            new SelectListItem { Text = "تشخیصی", Value = "Diagnostic" },
                            new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                            new SelectListItem { Text = "توانبخشی", Value = "Rehabilitation" }
                        }, "Value", "Text", model.Category);
                    }

                    ViewBag.Statuses = new SelectList(new[] { 
                        new SelectListItem { Text = "فعال", Value = "Active" },
                        new SelectListItem { Text = "تعمیر", Value = "Maintenance" },
                        new SelectListItem { Text = "غیرفعال", Value = "Inactive" }
                    }, "Value", "Text", model.Status);

                    return View(model);
                }

                var result = await _medicalEquipmentService.UpdateMedicalEquipmentAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی تجهیز پزشکی: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    
                    // بارگذاری مجدد ViewBag
                    var categories = await _context.Set<MedicalEquipment>()
                        .Where(e => !e.IsDeleted)
                        .Select(e => e.Category)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToListAsync();

                    ViewBag.Categories = categories.Select(c => new SelectListItem
                    {
                        Value = c,
                        Text = GetCategoryDisplayName(c),
                        Selected = c == model.Category
                    }).ToList();

                    if (!categories.Any())
                    {
                        ViewBag.Categories = new SelectList(new[] { 
                            new SelectListItem { Text = "تصویربرداری", Value = "Imaging" },
                            new SelectListItem { Text = "آزمایشگاه", Value = "Laboratory" },
                            new SelectListItem { Text = "جراحی", Value = "Surgery" },
                            new SelectListItem { Text = "مانیتورینگ", Value = "Monitoring" },
                            new SelectListItem { Text = "درمانی", Value = "Therapy" },
                            new SelectListItem { Text = "تشخیصی", Value = "Diagnostic" },
                            new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                            new SelectListItem { Text = "توانبخشی", Value = "Rehabilitation" }
                        }, "Value", "Text", model.Category);
                    }

                    ViewBag.Statuses = new SelectList(new[] { 
                        new SelectListItem { Text = "فعال", Value = "Active" },
                        new SelectListItem { Text = "تعمیر", Value = "Maintenance" },
                        new SelectListItem { Text = "غیرفعال", Value = "Inactive" }
                    }, "Value", "Text", model.Status);

                    return View(model);
                }

                _logger.Information("تجهیز پزشکی با موفقیت به‌روزرسانی شد - MedicalEquipmentId: {MedicalEquipmentId}", model.MedicalEquipmentId);
                TempData["Success"] = "تجهیز پزشکی با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", model.MedicalEquipmentId);
                TempData["Error"] = "خطا در به‌روزرسانی تجهیز پزشکی";
                return View(model);
            }
        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.Information("درخواست حذف تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);

                var result = await _medicalEquipmentService.DeleteMedicalEquipmentAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "تجهیز پزشکی با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);
                TempData["Error"] = "خطا در حذف تجهیز پزشکی";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Activate/Deactivate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(int id)
        {
            try
            {
                var result = await _medicalEquipmentService.ActivateMedicalEquipmentAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "تجهیز پزشکی با موفقیت فعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);
                TempData["Error"] = "خطا در فعال‌سازی تجهیز پزشکی";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _medicalEquipmentService.DeactivateMedicalEquipmentAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "تجهیز پزشکی با موفقیت غیرفعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);
                TempData["Error"] = "خطا در غیرفعال‌سازی تجهیز پزشکی";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Set Featured

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetFeatured(int id, bool isFeatured)
        {
            try
            {
                var result = await _medicalEquipmentService.SetFeaturedAsync(id, isFeatured);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = isFeatured ? "تجهیز پزشکی به عنوان ویژه تنظیم شد" : "تجهیز پزشکی از حالت ویژه خارج شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);
                TempData["Error"] = "خطا در تنظیم وضعیت ویژه تجهیز پزشکی";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Helper Methods

        private string GetCategoryDisplayName(string category)
        {
            return category switch
            {
                "Imaging" => "تصویربرداری",
                "Laboratory" => "آزمایشگاه",
                "Surgery" => "جراحی",
                "Monitoring" => "مانیتورینگ",
                "Therapy" => "درمانی",
                "Diagnostic" => "تشخیصی",
                "Emergency" => "اورژانس",
                "Rehabilitation" => "توانبخشی",
                _ => category ?? "عمومی"
            };
        }

        #endregion
    }
}

