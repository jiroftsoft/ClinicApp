using System;
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
    /// مدیریت فوتر در پنل CMS (تنظیمات اصلی)
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class FooterController : BaseCMSController
    {
        private readonly IFooterService _footerService;
        private readonly IFooterLinkRepository _footerLinkRepository;
        private readonly IFooterSocialRepository _footerSocialRepository;
        private readonly IFooterCertificationRepository _footerCertificationRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public FooterController(
            IFooterService footerService,
            IFooterLinkRepository footerLinkRepository,
            IFooterSocialRepository footerSocialRepository,
            IFooterCertificationRepository footerCertificationRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context)
        {
            _footerService = footerService ?? throw new ArgumentNullException(nameof(footerService));
            _footerLinkRepository = footerLinkRepository ?? throw new ArgumentNullException(nameof(footerLinkRepository));
            _footerSocialRepository = footerSocialRepository ?? throw new ArgumentNullException(nameof(footerSocialRepository));
            _footerCertificationRepository = footerCertificationRepository ?? throw new ArgumentNullException(nameof(footerCertificationRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = Log.ForContext<FooterController>();
        }

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(int? clinicId = null)
        {
            try
            {
                _logger.Information("درخواست مدیریت فوتر توسط کاربر {UserId} - ClinicId: {ClinicId}", _currentUserService.UserId, clinicId);
                var settingsResult = await _footerService.GetSettingsForEditAsync(clinicId);
                if (!settingsResult.Success)
                {
                    NotificationHelper.SetError(TempData, settingsResult.Message ?? "خطا در بارگذاری اطلاعات فوتر");
                }

                return View(GetViewPath("Index"), settingsResult.Data ?? new FooterSettingsEditViewModel());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه مدیریت فوتر");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری صفحه مدیریت فوتر");
                return View(GetViewPath("Index"), new FooterSettingsEditViewModel());
            }
        }

        [HttpGet]
        public async Task<ActionResult> EditSettings(int? clinicId = null)
        {
            try
            {
                var result = await _footerService.GetSettingsForEditAsync(clinicId);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message ?? "خطا در بارگذاری تنظیمات فوتر");
                }

                return View(GetViewPath("EditSettings"), result.Data ?? new FooterSettingsEditViewModel());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش تنظیمات فوتر");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم تنظیمات فوتر");
                return View(GetViewPath("EditSettings"), new FooterSettingsEditViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSettings(FooterSettingsEditViewModel model, int? clinicId = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً خطاهای فرم را بررسی کنید.");
                    return View(GetViewPath("EditSettings"), model);
                }

                var result = await _footerService.SaveSettingsAsync(model, clinicId);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message ?? "خطا در ذخیره تنظیمات فوتر");
                    return View(GetViewPath("EditSettings"), model);
                }

                NotificationHelper.SetSuccess(TempData, result.Message ?? "تنظیمات فوتر ذخیره شد");
                return RedirectToAction("Index", new { clinicId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره تنظیمات فوتر");
                NotificationHelper.SetError(TempData, "خطا در ذخیره تنظیمات فوتر");
                return View(GetViewPath("EditSettings"), model);
            }
        }

        #region Links (Quick/Service)

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> LinkIndex(byte type = 1, int? clinicId = null)
        {
            try
            {
                ViewBag.ClinicId = clinicId;
                var items = await _footerLinkRepository.GetActiveByTypeAsync(type, clinicId);
                var vm = new FooterLinksIndexViewModel
                {
                    LinkType = type,
                    Items = items.Select(x => new FooterLinkItemViewModel
                    {
                        FooterLinkId = x.FooterLinkId,
                        LinkType = x.LinkType,
                        Title = x.Title,
                        Url = x.Url,
                        Icon = x.Icon,
                        IsExternal = x.IsExternal,
                        DisplayOrder = x.DisplayOrder,
                        IsActive = x.IsActive
                    }).ToList()
                };
                return View(GetViewPath("LinkIndex"), vm);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست لینک‌های فوتر");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لینک‌های فوتر");
                return View(GetViewPath("LinkIndex"), new FooterLinksIndexViewModel { LinkType = type });
            }
        }

        [HttpGet]
        public ActionResult LinkCreate(byte type = 1, int? clinicId = null)
        {
            ViewBag.ClinicId = clinicId;
            return View(GetViewPath("LinkCreate"), new FooterLinkCreateEditViewModel { LinkType = type, IsActive = true, DisplayOrder = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LinkCreate(FooterLinkCreateEditViewModel model, int? clinicId = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(GetViewPath("LinkCreate"), model);

                if (!UrlSecurityHelper.IsSafeUrl(model.Url, allowRelative: true))
                {
                    ModelState.AddModelError(nameof(model.Url), "URL معتبر نیست یا از پروتکل‌های خطرناک استفاده می‌کند.");
                    return View(GetViewPath("LinkCreate"), model);
                }

                var entity = new FooterLink
                {
                    LinkType = model.LinkType,
                    Title = model.Title?.Trim(),
                    Url = model.Url?.Trim(),
                    Icon = model.Icon?.Trim(),
                    IsExternal = model.IsExternal,
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive,
                    ClinicId = clinicId,
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = _currentUserService.UserId
                };
                _footerLinkRepository.Add(entity);
                await _context.SaveChangesAsync();

                NotificationHelper.SetSuccess(TempData, "لینک فوتر با موفقیت ایجاد شد.");
                return RedirectToAction("LinkIndex", new { type = model.LinkType, clinicId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد لینک فوتر");
                NotificationHelper.SetError(TempData, "خطا در ایجاد لینک فوتر");
                return View(GetViewPath("LinkCreate"), model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> LinkEdit(int id)
        {
            var entity = await _footerLinkRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted)
                return HttpNotFound();

            ViewBag.ClinicId = entity.ClinicId;
            return View(GetViewPath("LinkEdit"), new FooterLinkCreateEditViewModel
            {
                FooterLinkId = entity.FooterLinkId,
                LinkType = entity.LinkType,
                Title = entity.Title,
                Url = entity.Url,
                Icon = entity.Icon,
                IsExternal = entity.IsExternal,
                DisplayOrder = entity.DisplayOrder,
                IsActive = entity.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LinkEdit(FooterLinkCreateEditViewModel model, int? clinicId = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(GetViewPath("LinkEdit"), model);

                if (!UrlSecurityHelper.IsSafeUrl(model.Url, allowRelative: true))
                {
                    ModelState.AddModelError(nameof(model.Url), "URL معتبر نیست یا از پروتکل‌های خطرناک استفاده می‌کند.");
                    return View(GetViewPath("LinkEdit"), model);
                }

                var entity = await _footerLinkRepository.GetByIdAsync(model.FooterLinkId);
                if (entity == null || entity.IsDeleted)
                    return HttpNotFound();

                entity.Title = model.Title?.Trim();
                entity.Url = model.Url?.Trim();
                entity.Icon = model.Icon?.Trim();
                entity.IsExternal = model.IsExternal;
                entity.DisplayOrder = model.DisplayOrder;
                entity.IsActive = model.IsActive;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedByUserId = _currentUserService.UserId;

                _footerLinkRepository.Update(entity);
                await _context.SaveChangesAsync();

                NotificationHelper.SetSuccess(TempData, "لینک فوتر با موفقیت ذخیره شد.");
                return RedirectToAction("LinkIndex", new { type = model.LinkType, clinicId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش لینک فوتر");
                NotificationHelper.SetError(TempData, "خطا در ذخیره لینک فوتر");
                return View(GetViewPath("LinkEdit"), model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LinkDelete(int id, byte type = 1, int? clinicId = null)
        {
            try
            {
                var entity = await _footerLinkRepository.GetByIdAsync(id);
                if (entity == null || entity.IsDeleted)
                    return HttpNotFound();

                entity.DeletedByUserId = _currentUserService.UserId;
                _footerLinkRepository.Delete(entity);
                await _context.SaveChangesAsync();

                NotificationHelper.SetSuccess(TempData, "لینک فوتر حذف شد.");
                return RedirectToAction("LinkIndex", new { type, clinicId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف لینک فوتر");
                NotificationHelper.SetError(TempData, "خطا در حذف لینک فوتر");
                return RedirectToAction("LinkIndex", new { type, clinicId });
            }
        }

        #endregion

        #region Social

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> SocialIndex(int? clinicId = null)
        {
            try
            {
                ViewBag.ClinicId = clinicId;
                var items = await _footerSocialRepository.GetActiveAsync(clinicId);
                var vm = new FooterSocialIndexViewModel
                {
                    Items = items.Select(x => new FooterSocialItemViewModel
                    {
                        FooterSocialId = x.FooterSocialId,
                        Platform = x.Platform,
                        Url = x.Url,
                        Icon = x.Icon,
                        AriaLabel = x.AriaLabel,
                        DisplayOrder = x.DisplayOrder,
                        IsActive = x.IsActive
                    }).ToList()
                };
                return View(GetViewPath("SocialIndex"), vm);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش شبکه‌های اجتماعی فوتر");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری شبکه‌های اجتماعی");
                return View(GetViewPath("SocialIndex"), new FooterSocialIndexViewModel());
            }
        }

        [HttpGet]
        public ActionResult SocialCreate(int? clinicId = null)
        {
            ViewBag.ClinicId = clinicId;
            return View(GetViewPath("SocialCreate"), new FooterSocialCreateEditViewModel { IsActive = true, DisplayOrder = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SocialCreate(FooterSocialCreateEditViewModel model, int? clinicId = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(GetViewPath("SocialCreate"), model);

                if (!UrlSecurityHelper.IsSafeUrl(model.Url, allowRelative: false))
                {
                    ModelState.AddModelError(nameof(model.Url), "URL معتبر نیست یا باید با http/https شروع شود.");
                    return View(GetViewPath("SocialCreate"), model);
                }

                var entity = new FooterSocial
                {
                    Platform = model.Platform?.Trim(),
                    Url = model.Url?.Trim(),
                    Icon = model.Icon?.Trim(),
                    AriaLabel = model.AriaLabel?.Trim(),
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive,
                    ClinicId = clinicId,
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = _currentUserService.UserId
                };
                _footerSocialRepository.Add(entity);
                await _context.SaveChangesAsync();

                NotificationHelper.SetSuccess(TempData, "شبکه اجتماعی ایجاد شد.");
                return RedirectToAction("SocialIndex", new { clinicId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد شبکه اجتماعی");
                NotificationHelper.SetError(TempData, "خطا در ایجاد شبکه اجتماعی");
                return View(GetViewPath("SocialCreate"), model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> SocialEdit(int id)
        {
            var entity = await _footerSocialRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted)
                return HttpNotFound();

            ViewBag.ClinicId = entity.ClinicId;
            return View(GetViewPath("SocialEdit"), new FooterSocialCreateEditViewModel
            {
                FooterSocialId = entity.FooterSocialId,
                Platform = entity.Platform,
                Url = entity.Url,
                Icon = entity.Icon,
                AriaLabel = entity.AriaLabel,
                DisplayOrder = entity.DisplayOrder,
                IsActive = entity.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SocialEdit(FooterSocialCreateEditViewModel model, int? clinicId = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(GetViewPath("SocialEdit"), model);

                if (!UrlSecurityHelper.IsSafeUrl(model.Url, allowRelative: false))
                {
                    ModelState.AddModelError(nameof(model.Url), "URL معتبر نیست یا باید با http/https شروع شود.");
                    return View(GetViewPath("SocialEdit"), model);
                }

                var entity = await _footerSocialRepository.GetByIdAsync(model.FooterSocialId);
                if (entity == null || entity.IsDeleted)
                    return HttpNotFound();

                entity.Platform = model.Platform?.Trim();
                entity.Url = model.Url?.Trim();
                entity.Icon = model.Icon?.Trim();
                entity.AriaLabel = model.AriaLabel?.Trim();
                entity.DisplayOrder = model.DisplayOrder;
                entity.IsActive = model.IsActive;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedByUserId = _currentUserService.UserId;

                _footerSocialRepository.Update(entity);
                await _context.SaveChangesAsync();

                NotificationHelper.SetSuccess(TempData, "شبکه اجتماعی ذخیره شد.");
                return RedirectToAction("SocialIndex", new { clinicId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش شبکه اجتماعی");
                NotificationHelper.SetError(TempData, "خطا در ذخیره شبکه اجتماعی");
                return View(GetViewPath("SocialEdit"), model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SocialDelete(int id, int? clinicId = null)
        {
            try
            {
                var entity = await _footerSocialRepository.GetByIdAsync(id);
                if (entity == null || entity.IsDeleted)
                    return HttpNotFound();

                entity.DeletedByUserId = _currentUserService.UserId;
                _footerSocialRepository.Delete(entity);
                await _context.SaveChangesAsync();

                NotificationHelper.SetSuccess(TempData, "شبکه اجتماعی حذف شد.");
                return RedirectToAction("SocialIndex", new { clinicId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف شبکه اجتماعی");
                NotificationHelper.SetError(TempData, "خطا در حذف شبکه اجتماعی");
                return RedirectToAction("SocialIndex", new { clinicId });
            }
        }

        #endregion

        #region Certifications

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> CertificationIndex(int? clinicId = null)
        {
            try
            {
                ViewBag.ClinicId = clinicId;
                var items = await _footerCertificationRepository.GetActiveAsync(clinicId);
                var vm = new FooterCertificationIndexViewModel
                {
                    Items = items.Select(x => new FooterCertificationItemViewModel
                    {
                        FooterCertificationId = x.FooterCertificationId,
                        Title = x.Title,
                        Description = x.Description,
                        ImageUrl = x.ImageUrl,
                        LinkUrl = x.LinkUrl,
                        LicenseNumber = x.LicenseNumber,
                        DisplayOrder = x.DisplayOrder,
                        IsActive = x.IsActive
                    }).ToList()
                };
                return View(GetViewPath("CertificationIndex"), vm);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش مجوزهای فوتر");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری مجوزها");
                return View(GetViewPath("CertificationIndex"), new FooterCertificationIndexViewModel());
            }
        }

        [HttpGet]
        public ActionResult CertificationCreate(int? clinicId = null)
        {
            ViewBag.ClinicId = clinicId;
            return View(GetViewPath("CertificationCreate"), new FooterCertificationCreateEditViewModel { IsActive = true, DisplayOrder = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CertificationCreate(FooterCertificationCreateEditViewModel model, int? clinicId = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(GetViewPath("CertificationCreate"), model);

                if (!string.IsNullOrWhiteSpace(model.LinkUrl) && !UrlSecurityHelper.IsSafeUrl(model.LinkUrl, allowRelative: true))
                {
                    ModelState.AddModelError(nameof(model.LinkUrl), "لینک معتبر نیست یا از پروتکل‌های خطرناک استفاده می‌کند.");
                    return View(GetViewPath("CertificationCreate"), model);
                }

                var entity = new FooterCertification
                {
                    Title = model.Title?.Trim(),
                    Description = model.Description?.Trim(),
                    ImageUrl = model.ImageUrl?.Trim(),
                    LinkUrl = model.LinkUrl?.Trim(),
                    LicenseNumber = model.LicenseNumber?.Trim(),
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive,
                    ClinicId = clinicId,
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = _currentUserService.UserId
                };
                _footerCertificationRepository.Add(entity);
                await _context.SaveChangesAsync();

                NotificationHelper.SetSuccess(TempData, "مجوز ایجاد شد.");
                return RedirectToAction("CertificationIndex", new { clinicId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد مجوز");
                NotificationHelper.SetError(TempData, "خطا در ایجاد مجوز");
                return View(GetViewPath("CertificationCreate"), model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> CertificationEdit(int id)
        {
            var entity = await _footerCertificationRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted)
                return HttpNotFound();

            ViewBag.ClinicId = entity.ClinicId;
            return View(GetViewPath("CertificationEdit"), new FooterCertificationCreateEditViewModel
            {
                FooterCertificationId = entity.FooterCertificationId,
                Title = entity.Title,
                Description = entity.Description,
                ImageUrl = entity.ImageUrl,
                LinkUrl = entity.LinkUrl,
                LicenseNumber = entity.LicenseNumber,
                DisplayOrder = entity.DisplayOrder,
                IsActive = entity.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CertificationEdit(FooterCertificationCreateEditViewModel model, int? clinicId = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(GetViewPath("CertificationEdit"), model);

                if (!string.IsNullOrWhiteSpace(model.LinkUrl) && !UrlSecurityHelper.IsSafeUrl(model.LinkUrl, allowRelative: true))
                {
                    ModelState.AddModelError(nameof(model.LinkUrl), "لینک معتبر نیست یا از پروتکل‌های خطرناک استفاده می‌کند.");
                    return View(GetViewPath("CertificationEdit"), model);
                }

                var entity = await _footerCertificationRepository.GetByIdAsync(model.FooterCertificationId);
                if (entity == null || entity.IsDeleted)
                    return HttpNotFound();

                entity.Title = model.Title?.Trim();
                entity.Description = model.Description?.Trim();
                entity.ImageUrl = model.ImageUrl?.Trim();
                entity.LinkUrl = model.LinkUrl?.Trim();
                entity.LicenseNumber = model.LicenseNumber?.Trim();
                entity.DisplayOrder = model.DisplayOrder;
                entity.IsActive = model.IsActive;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedByUserId = _currentUserService.UserId;

                _footerCertificationRepository.Update(entity);
                await _context.SaveChangesAsync();

                NotificationHelper.SetSuccess(TempData, "مجوز ذخیره شد.");
                return RedirectToAction("CertificationIndex", new { clinicId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش مجوز");
                NotificationHelper.SetError(TempData, "خطا در ذخیره مجوز");
                return View(GetViewPath("CertificationEdit"), model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CertificationDelete(int id, int? clinicId = null)
        {
            try
            {
                var entity = await _footerCertificationRepository.GetByIdAsync(id);
                if (entity == null || entity.IsDeleted)
                    return HttpNotFound();

                entity.DeletedByUserId = _currentUserService.UserId;
                _footerCertificationRepository.Delete(entity);
                await _context.SaveChangesAsync();

                NotificationHelper.SetSuccess(TempData, "مجوز حذف شد.");
                return RedirectToAction("CertificationIndex", new { clinicId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف مجوز");
                NotificationHelper.SetError(TempData, "خطا در حذف مجوز");
                return RedirectToAction("CertificationIndex", new { clinicId });
            }
        }

        #endregion

        #region Reorder (Drag & Drop)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LinkReorder(int[] ids, int[] orders)
        {
            try
            {
                if (ids == null || orders == null || ids.Length != orders.Length)
                    return Json(new { success = false, message = "درخواست نامعتبر است." });

                for (int i = 0; i < ids.Length; i++)
                {
                    var entity = await _footerLinkRepository.GetByIdAsync(ids[i]);
                    if (entity == null || entity.IsDeleted) continue;
                    entity.DisplayOrder = orders[i];
                    entity.UpdatedAt = DateTime.Now;
                    entity.UpdatedByUserId = _currentUserService.UserId;
                    _footerLinkRepository.Update(entity);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره ترتیب لینک‌های فوتر");
                return Json(new { success = false, message = "خطا در ذخیره ترتیب" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SocialReorder(int[] ids, int[] orders)
        {
            try
            {
                if (ids == null || orders == null || ids.Length != orders.Length)
                    return Json(new { success = false, message = "درخواست نامعتبر است." });

                for (int i = 0; i < ids.Length; i++)
                {
                    var entity = await _footerSocialRepository.GetByIdAsync(ids[i]);
                    if (entity == null || entity.IsDeleted) continue;
                    entity.DisplayOrder = orders[i];
                    entity.UpdatedAt = DateTime.Now;
                    entity.UpdatedByUserId = _currentUserService.UserId;
                    _footerSocialRepository.Update(entity);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره ترتیب شبکه‌های اجتماعی فوتر");
                return Json(new { success = false, message = "خطا در ذخیره ترتیب" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CertificationReorder(int[] ids, int[] orders)
        {
            try
            {
                if (ids == null || orders == null || ids.Length != orders.Length)
                    return Json(new { success = false, message = "درخواست نامعتبر است." });

                for (int i = 0; i < ids.Length; i++)
                {
                    var entity = await _footerCertificationRepository.GetByIdAsync(ids[i]);
                    if (entity == null || entity.IsDeleted) continue;
                    entity.DisplayOrder = orders[i];
                    entity.UpdatedAt = DateTime.Now;
                    entity.UpdatedByUserId = _currentUserService.UserId;
                    _footerCertificationRepository.Update(entity);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره ترتیب مجوزهای فوتر");
                return Json(new { success = false, message = "خطا در ذخیره ترتیب" });
            }
        }

        #endregion
    }
}

