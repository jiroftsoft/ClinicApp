using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت اطلاعات خدمات پزشکی (Medical Service Information)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class MedicalServiceInfoController : BaseCMSController
    {
        private readonly IMedicalServiceInfoService _medicalServiceInfoService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public MedicalServiceInfoController(
            IMedicalServiceInfoService medicalServiceInfoService,
            ICurrentUserService currentUserService,
            ApplicationDbContext context)
        {
            _medicalServiceInfoService = medicalServiceInfoService ?? throw new ArgumentNullException(nameof(medicalServiceInfoService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = Log.ForContext<MedicalServiceInfoController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(MedicalServiceInfoSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست اطلاعات خدمات پزشکی توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مدل جستجو
                if (searchModel == null)
                {
                    searchModel = new MedicalServiceInfoSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _medicalServiceInfoService.GetMedicalServiceInfosAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست اطلاعات خدمات پزشکی: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(new PagedResult<MedicalServiceInfoIndexViewModel>(new System.Collections.Generic.List<MedicalServiceInfoIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize));
                }

                // بارگذاری دسته‌بندی‌های خدمات برای فیلتر
                var serviceCategories = await _context.ServiceCategories
                    .Where(sc => !sc.IsDeleted)
                    .OrderBy(sc => sc.Title)
                    .Select(sc => new SelectListItem
                    {
                        Value = sc.ServiceCategoryId.ToString(),
                        Text = sc.Title
                    })
                    .ToListAsync();
                ViewBag.ServiceCategories = serviceCategories;

                // بارگذاری خدمات برای فیلتر
                var services = await _context.Services
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Title)
                    .Select(s => new SelectListItem
                    {
                        Value = s.ServiceId.ToString(),
                        Text = s.Title + " (" + s.ServiceCode + ")"
                    })
                    .ToListAsync();
                ViewBag.Services = services;

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست اطلاعات خدمات پزشکی");
                TempData["Error"] = "خطا در بارگذاری لیست اطلاعات خدمات پزشکی";
                return View(new PagedResult<MedicalServiceInfoIndexViewModel>(new System.Collections.Generic.List<MedicalServiceInfoIndexViewModel>(), 0, 1, 10));
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _medicalServiceInfoService.GetMedicalServiceInfoDetailsAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات اطلاعات خدمت پزشکی";
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
                // بارگذاری لیست خدمات برای dropdown
                var services = await _context.Services
                    .Include(s => s.ServiceCategory)
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .OrderBy(s => s.ServiceCategory.Title)
                    .ThenBy(s => s.Title)
                    .Select(s => new SelectListItem
                    {
                        Value = s.ServiceId.ToString(),
                        Text = s.ServiceCategory.Title + " - " + s.Title + " (" + s.ServiceCode + ")"
                    })
                    .ToListAsync();

                ViewBag.Services = services;

                var model = new MedicalServiceInfoCreateEditViewModel
                {
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 0
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد اطلاعات خدمت پزشکی");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد اطلاعات خدمت پزشکی";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in FullDescription field
        public async Task<ActionResult> Create(MedicalServiceInfoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد اطلاعات خدمت پزشکی جدید توسط کاربر {UserId}", _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    // بارگذاری مجدد لیست خدمات
                    var services = await _context.Services
                        .Include(s => s.ServiceCategory)
                        .Where(s => !s.IsDeleted && s.IsActive)
                        .OrderBy(s => s.ServiceCategory.Title)
                        .ThenBy(s => s.Title)
                        .Select(s => new SelectListItem
                        {
                            Value = s.ServiceId.ToString(),
                            Text = s.ServiceCategory.Title + " - " + s.Title + " (" + s.ServiceCode + ")"
                        })
                        .ToListAsync();
                    ViewBag.Services = services;

                    return View(model);
                }

                var result = await _medicalServiceInfoService.CreateMedicalServiceInfoAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد اطلاعات خدمت پزشکی: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    
                    // بارگذاری مجدد لیست خدمات
                    var services = await _context.Services
                        .Include(s => s.ServiceCategory)
                        .Where(s => !s.IsDeleted && s.IsActive)
                        .OrderBy(s => s.ServiceCategory.Title)
                        .ThenBy(s => s.Title)
                        .Select(s => new SelectListItem
                        {
                            Value = s.ServiceId.ToString(),
                            Text = s.ServiceCategory.Title + " - " + s.Title + " (" + s.ServiceCode + ")"
                        })
                        .ToListAsync();
                    ViewBag.Services = services;

                    return View(model);
                }

                _logger.Information("اطلاعات خدمت پزشکی با موفقیت ایجاد شد - MedicalServiceInfoId: {MedicalServiceInfoId}", result.Data.MedicalServiceInfoId);
                TempData["Success"] = "اطلاعات خدمت پزشکی با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اطلاعات خدمت پزشکی");
                TempData["Error"] = "خطا در ایجاد اطلاعات خدمت پزشکی";
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
                var result = await _medicalServiceInfoService.GetMedicalServiceInfoForEditAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                // بارگذاری لیست خدمات برای dropdown
                var services = await _context.Services
                    .Include(s => s.ServiceCategory)
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .OrderBy(s => s.ServiceCategory.Title)
                    .ThenBy(s => s.Title)
                    .Select(s => new SelectListItem
                    {
                        Value = s.ServiceId.ToString(),
                        Text = s.ServiceCategory.Title + " - " + s.Title + " (" + s.ServiceCode + ")",
                        Selected = s.ServiceId == result.Data.ServiceId
                    })
                    .ToListAsync();

                ViewBag.Services = services;

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش اطلاعات خدمت پزشکی";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in FullDescription field
        public async Task<ActionResult> Edit(MedicalServiceInfoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", model.MedicalServiceInfoId);

                if (!ModelState.IsValid)
                {
                    // بارگذاری مجدد لیست خدمات
                    var services = await _context.Services
                        .Include(s => s.ServiceCategory)
                        .Where(s => !s.IsDeleted && s.IsActive)
                        .OrderBy(s => s.ServiceCategory.Title)
                        .ThenBy(s => s.Title)
                        .Select(s => new SelectListItem
                        {
                            Value = s.ServiceId.ToString(),
                            Text = s.ServiceCategory.Title + " - " + s.Title + " (" + s.ServiceCode + ")",
                            Selected = s.ServiceId == model.ServiceId
                        })
                        .ToListAsync();
                    ViewBag.Services = services;

                    return View(model);
                }

                var result = await _medicalServiceInfoService.UpdateMedicalServiceInfoAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی اطلاعات خدمت پزشکی: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    
                    // بارگذاری مجدد لیست خدمات
                    var services = await _context.Services
                        .Include(s => s.ServiceCategory)
                        .Where(s => !s.IsDeleted && s.IsActive)
                        .OrderBy(s => s.ServiceCategory.Title)
                        .ThenBy(s => s.Title)
                        .Select(s => new SelectListItem
                        {
                            Value = s.ServiceId.ToString(),
                            Text = s.ServiceCategory.Title + " - " + s.Title + " (" + s.ServiceCode + ")",
                            Selected = s.ServiceId == model.ServiceId
                        })
                        .ToListAsync();
                    ViewBag.Services = services;

                    return View(model);
                }

                _logger.Information("اطلاعات خدمت پزشکی با موفقیت به‌روزرسانی شد - MedicalServiceInfoId: {MedicalServiceInfoId}", model.MedicalServiceInfoId);
                TempData["Success"] = "اطلاعات خدمت پزشکی با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", model.MedicalServiceInfoId);
                TempData["Error"] = "خطا در به‌روزرسانی اطلاعات خدمت پزشکی";
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
                _logger.Information("درخواست حذف اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);

                var result = await _medicalServiceInfoService.DeleteMedicalServiceInfoAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اطلاعات خدمت پزشکی با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);
                TempData["Error"] = "خطا در حذف اطلاعات خدمت پزشکی";
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
                var result = await _medicalServiceInfoService.ActivateMedicalServiceInfoAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اطلاعات خدمت پزشکی با موفقیت فعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);
                TempData["Error"] = "خطا در فعال‌سازی اطلاعات خدمت پزشکی";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _medicalServiceInfoService.DeactivateMedicalServiceInfoAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اطلاعات خدمت پزشکی با موفقیت غیرفعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);
                TempData["Error"] = "خطا در غیرفعال‌سازی اطلاعات خدمت پزشکی";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Featured

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetFeatured(int id, bool isFeatured)
        {
            try
            {
                var result = await _medicalServiceInfoService.SetFeaturedAsync(id, isFeatured);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = isFeatured ? "اطلاعات خدمت پزشکی به عنوان ویژه تنظیم شد" : "اطلاعات خدمت پزشکی از حالت ویژه خارج شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);
                TempData["Error"] = "خطا در تنظیم وضعیت ویژه اطلاعات خدمت پزشکی";
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}

