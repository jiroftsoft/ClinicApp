using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت اطلاعات بیمه (Insurance Information)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class InsuranceInfoController : Controller
    {
        private readonly IInsuranceInfoService _insuranceInfoService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public InsuranceInfoController(
            IInsuranceInfoService insuranceInfoService,
            ICurrentUserService currentUserService)
        {
            _insuranceInfoService = insuranceInfoService ?? throw new ArgumentNullException(nameof(insuranceInfoService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<InsuranceInfoController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(InsuranceInfoSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست اطلاعات بیمه توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مدل جستجو
                if (searchModel == null)
                {
                    searchModel = new InsuranceInfoSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _insuranceInfoService.GetInsuranceInfosAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست اطلاعات بیمه: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(new PagedResult<InsuranceInfoIndexViewModel>(new System.Collections.Generic.List<InsuranceInfoIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize));
                }

                // بارگذاری انواع بیمه برای فیلتر
                var typesResult = await _insuranceInfoService.GetInsuranceTypesAsync();
                ViewBag.InsuranceTypes = typesResult.Success ? typesResult.Data : new System.Collections.Generic.List<InsuranceInfoTypeViewModel>();

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست اطلاعات بیمه");
                TempData["Error"] = "خطا در بارگذاری لیست اطلاعات بیمه";
                return View(new PagedResult<InsuranceInfoIndexViewModel>(new System.Collections.Generic.List<InsuranceInfoIndexViewModel>(), 0, 1, 10));
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _insuranceInfoService.GetInsuranceInfoDetailsAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات اطلاعات بیمه";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                var model = new InsuranceInfoCreateEditViewModel
                {
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 0,
                    InsuranceType = "basic"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد اطلاعات بیمه");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد اطلاعات بیمه";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in FullDescription field
        public async Task<ActionResult> Create(InsuranceInfoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد اطلاعات بیمه جدید توسط کاربر {UserId}", _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _insuranceInfoService.CreateInsuranceInfoAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد اطلاعات بیمه: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                _logger.Information("اطلاعات بیمه با موفقیت ایجاد شد - InsuranceInfoId: {InsuranceInfoId}", result.Data.InsuranceInfoId);
                TempData["Success"] = "اطلاعات بیمه با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اطلاعات بیمه");
                TempData["Error"] = "خطا در ایجاد اطلاعات بیمه";
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
                var result = await _insuranceInfoService.GetInsuranceInfoForEditAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش اطلاعات بیمه";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in FullDescription field
        public async Task<ActionResult> Edit(InsuranceInfoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", model.InsuranceInfoId);

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _insuranceInfoService.UpdateInsuranceInfoAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی اطلاعات بیمه: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                _logger.Information("اطلاعات بیمه با موفقیت به‌روزرسانی شد - InsuranceInfoId: {InsuranceInfoId}", model.InsuranceInfoId);
                TempData["Success"] = "اطلاعات بیمه با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", model.InsuranceInfoId);
                TempData["Error"] = "خطا در به‌روزرسانی اطلاعات بیمه";
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
                _logger.Information("درخواست حذف اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);

                var result = await _insuranceInfoService.DeleteInsuranceInfoAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اطلاعات بیمه با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);
                TempData["Error"] = "خطا در حذف اطلاعات بیمه";
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
                var result = await _insuranceInfoService.ActivateInsuranceInfoAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اطلاعات بیمه با موفقیت فعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);
                TempData["Error"] = "خطا در فعال‌سازی اطلاعات بیمه";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _insuranceInfoService.DeactivateInsuranceInfoAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اطلاعات بیمه با موفقیت غیرفعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);
                TempData["Error"] = "خطا در غیرفعال‌سازی اطلاعات بیمه";
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
                var result = await _insuranceInfoService.SetFeaturedAsync(id, isFeatured);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = isFeatured ? "اطلاعات بیمه به عنوان ویژه تنظیم شد" : "اطلاعات بیمه از حالت ویژه خارج شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);
                TempData["Error"] = "خطا در تنظیم وضعیت ویژه اطلاعات بیمه";
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}

