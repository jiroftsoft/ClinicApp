using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت ساعات کاری کلینیک (Clinic Working Hours)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class ClinicWorkingHoursController : BaseCMSController
    {
        private readonly IClinicWorkingHoursService _clinicWorkingHoursService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ClinicWorkingHoursController(
            IClinicWorkingHoursService clinicWorkingHoursService,
            ICurrentUserService currentUserService,
            ApplicationDbContext context)
        {
            _clinicWorkingHoursService = clinicWorkingHoursService ?? throw new ArgumentNullException(nameof(clinicWorkingHoursService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = Log.ForContext<ClinicWorkingHoursController>();
        }


        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(ClinicWorkingHoursSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست ساعات کاری کلینیک توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مدل جستجو
                if (searchModel == null)
                {
                    searchModel = new ClinicWorkingHoursSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _clinicWorkingHoursService.GetClinicWorkingHoursAsync(searchModel);

                // Strongly-Typed: بارگذاری کلینیک‌ها و روزهای هفته در ViewModel
                var clinics = await _context.Set<Clinic>()
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست ساعات کاری کلینیک: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    
                    // Strongly-Typed: ایجاد ViewModel حتی در صورت خطا
                    var errorPageViewModel = new ClinicWorkingHoursIndexPageViewModel
                    {
                        PagedResult = new PagedResult<ClinicWorkingHoursIndexViewModel>(new System.Collections.Generic.List<ClinicWorkingHoursIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize),
                        Clinics = clinics.Select(c => new SelectListItem
                        {
                            Value = c.ClinicId.ToString(),
                            Text = c.Name
                        }).ToList(),
                        DaysOfWeek = new List<SelectListItem>
                        {
                            new SelectListItem { Text = "شنبه", Value = "0" },
                            new SelectListItem { Text = "یکشنبه", Value = "1" },
                            new SelectListItem { Text = "دوشنبه", Value = "2" },
                            new SelectListItem { Text = "سه‌شنبه", Value = "3" },
                            new SelectListItem { Text = "چهارشنبه", Value = "4" },
                            new SelectListItem { Text = "پنج‌شنبه", Value = "5" },
                            new SelectListItem { Text = "جمعه", Value = "6" }
                        }
                    };
                    
                    return View(GetViewPath("Index"), errorPageViewModel);
                }

                var pageViewModel = new ClinicWorkingHoursIndexPageViewModel
                {
                    PagedResult = result.Data,
                    Clinics = clinics.Select(c => new SelectListItem
                    {
                        Value = c.ClinicId.ToString(),
                        Text = c.Name
                    }).ToList(),
                    DaysOfWeek = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "شنبه", Value = "0" },
                        new SelectListItem { Text = "یکشنبه", Value = "1" },
                        new SelectListItem { Text = "دوشنبه", Value = "2" },
                        new SelectListItem { Text = "سه‌شنبه", Value = "3" },
                        new SelectListItem { Text = "چهارشنبه", Value = "4" },
                        new SelectListItem { Text = "پنج‌شنبه", Value = "5" },
                        new SelectListItem { Text = "جمعه", Value = "6" }
                    }
                };

                return View(GetViewPath("Index"), pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست ساعات کاری کلینیک");
                TempData["Error"] = "خطا در بارگذاری لیست ساعات کاری کلینیک";
                
                // Strongly-Typed: ایجاد ViewModel حتی در صورت Exception
                var clinics = await _context.Set<Clinic>()
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                var errorPageViewModel = new ClinicWorkingHoursIndexPageViewModel
                {
                    PagedResult = new PagedResult<ClinicWorkingHoursIndexViewModel>(new System.Collections.Generic.List<ClinicWorkingHoursIndexViewModel>(), 0, 1, 10),
                    Clinics = clinics.Select(c => new SelectListItem
                    {
                        Value = c.ClinicId.ToString(),
                        Text = c.Name
                    }).ToList(),
                    DaysOfWeek = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "شنبه", Value = "0" },
                        new SelectListItem { Text = "یکشنبه", Value = "1" },
                        new SelectListItem { Text = "دوشنبه", Value = "2" },
                        new SelectListItem { Text = "سه‌شنبه", Value = "3" },
                        new SelectListItem { Text = "چهارشنبه", Value = "4" },
                        new SelectListItem { Text = "پنج‌شنبه", Value = "5" },
                        new SelectListItem { Text = "جمعه", Value = "6" }
                    }
                };
                
                return View(GetViewPath("Index"), errorPageViewModel);
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _clinicWorkingHoursService.GetClinicWorkingHoursDetailsAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات ساعات کاری کلینیک - ClinicWorkingHoursId: {ClinicWorkingHoursId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات ساعات کاری کلینیک";
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
                // Strongly-Typed: بارگذاری کلینیک‌ها و روزهای هفته در ViewModel
                var clinics = await _context.Set<Clinic>()
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                var model = new ClinicWorkingHoursCreateEditViewModel
                {
                    IsActive = true,
                    IsOpen = true,
                    DisplayOrder = 0,
                    Clinics = clinics.Select(c => new SelectListItem
                    {
                        Value = c.ClinicId.ToString(),
                        Text = c.Name
                    }).ToList(),
                    DaysOfWeek = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "شنبه", Value = "0" },
                        new SelectListItem { Text = "یکشنبه", Value = "1" },
                        new SelectListItem { Text = "دوشنبه", Value = "2" },
                        new SelectListItem { Text = "سه‌شنبه", Value = "3" },
                        new SelectListItem { Text = "چهارشنبه", Value = "4" },
                        new SelectListItem { Text = "پنج‌شنبه", Value = "5" },
                        new SelectListItem { Text = "جمعه", Value = "6" }
                    }
                };

                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد ساعات کاری کلینیک");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد ساعات کاری کلینیک";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ClinicWorkingHoursCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد ساعات کاری جدید توسط کاربر {UserId}", _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    // Strongly-Typed: بارگذاری مجدد Lists در ViewModel
                    var clinics = await _context.Set<Clinic>()
                        .Where(c => !c.IsDeleted)
                        .OrderBy(c => c.Name)
                        .ToListAsync();

                    model.Clinics = clinics.Select(c => new SelectListItem
                    {
                        Value = c.ClinicId.ToString(),
                        Text = c.Name
                    }).ToList();

                    model.DaysOfWeek = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "شنبه", Value = "0" },
                        new SelectListItem { Text = "یکشنبه", Value = "1" },
                        new SelectListItem { Text = "دوشنبه", Value = "2" },
                        new SelectListItem { Text = "سه‌شنبه", Value = "3" },
                        new SelectListItem { Text = "چهارشنبه", Value = "4" },
                        new SelectListItem { Text = "پنج‌شنبه", Value = "5" },
                        new SelectListItem { Text = "جمعه", Value = "6" }
                    };

                    return View(GetViewPath("Create"), model);
                }

                var result = await _clinicWorkingHoursService.CreateClinicWorkingHoursAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد ساعات کاری کلینیک: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    
                    // Strongly-Typed: بارگذاری مجدد Lists در ViewModel
                    var clinics = await _context.Set<Clinic>()
                        .Where(c => !c.IsDeleted)
                        .OrderBy(c => c.Name)
                        .ToListAsync();

                    model.Clinics = clinics.Select(c => new SelectListItem
                    {
                        Value = c.ClinicId.ToString(),
                        Text = c.Name
                    }).ToList();

                    model.DaysOfWeek = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "شنبه", Value = "0" },
                        new SelectListItem { Text = "یکشنبه", Value = "1" },
                        new SelectListItem { Text = "دوشنبه", Value = "2" },
                        new SelectListItem { Text = "سه‌شنبه", Value = "3" },
                        new SelectListItem { Text = "چهارشنبه", Value = "4" },
                        new SelectListItem { Text = "پنج‌شنبه", Value = "5" },
                        new SelectListItem { Text = "جمعه", Value = "6" }
                    };

                    return View(GetViewPath("Create"), model);
                }

                _logger.Information("ساعات کاری با موفقیت ایجاد شد - ClinicWorkingHoursId: {ClinicWorkingHoursId}", result.Data.ClinicWorkingHoursId);
                TempData["Success"] = "ساعات کاری با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد ساعات کاری کلینیک");
                TempData["Error"] = "خطا در ایجاد ساعات کاری کلینیک";
                
                // Strongly-Typed: بارگذاری Lists در ViewModel در صورت Exception
                var clinics = await _context.Set<Clinic>()
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                model.Clinics = clinics.Select(c => new SelectListItem
                {
                    Value = c.ClinicId.ToString(),
                    Text = c.Name
                }).ToList();

                model.DaysOfWeek = new List<SelectListItem>
                {
                    new SelectListItem { Text = "شنبه", Value = "0" },
                    new SelectListItem { Text = "یکشنبه", Value = "1" },
                    new SelectListItem { Text = "دوشنبه", Value = "2" },
                    new SelectListItem { Text = "سه‌شنبه", Value = "3" },
                    new SelectListItem { Text = "چهارشنبه", Value = "4" },
                    new SelectListItem { Text = "پنج‌شنبه", Value = "5" },
                    new SelectListItem { Text = "جمعه", Value = "6" }
                };
                
                return View(GetViewPath("Create"), model);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _clinicWorkingHoursService.GetClinicWorkingHoursForEditAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                // Strongly-Typed: بارگذاری کلینیک‌ها و روزهای هفته در ViewModel
                var clinics = await _context.Set<Clinic>()
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                result.Data.Clinics = clinics.Select(c => new SelectListItem
                {
                    Value = c.ClinicId.ToString(),
                    Text = c.Name,
                    Selected = c.ClinicId == result.Data.ClinicId
                }).ToList();

                result.Data.DaysOfWeek = new List<SelectListItem>
                {
                    new SelectListItem { Text = "شنبه", Value = "0", Selected = result.Data.DayOfWeek == 0 },
                    new SelectListItem { Text = "یکشنبه", Value = "1", Selected = result.Data.DayOfWeek == 1 },
                    new SelectListItem { Text = "دوشنبه", Value = "2", Selected = result.Data.DayOfWeek == 2 },
                    new SelectListItem { Text = "سه‌شنبه", Value = "3", Selected = result.Data.DayOfWeek == 3 },
                    new SelectListItem { Text = "چهارشنبه", Value = "4", Selected = result.Data.DayOfWeek == 4 },
                    new SelectListItem { Text = "پنج‌شنبه", Value = "5", Selected = result.Data.DayOfWeek == 5 },
                    new SelectListItem { Text = "جمعه", Value = "6", Selected = result.Data.DayOfWeek == 6 }
                };

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش ساعات کاری کلینیک - ClinicWorkingHoursId: {ClinicWorkingHoursId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش ساعات کاری کلینیک";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ClinicWorkingHoursCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی ساعات کاری - ClinicWorkingHoursId: {ClinicWorkingHoursId}", model.ClinicWorkingHoursId);

                if (!ModelState.IsValid)
                {
                    // Strongly-Typed: بارگذاری مجدد Lists در ViewModel
                    var clinics = await _context.Set<Clinic>()
                        .Where(c => !c.IsDeleted)
                        .OrderBy(c => c.Name)
                        .ToListAsync();

                    model.Clinics = clinics.Select(c => new SelectListItem
                    {
                        Value = c.ClinicId.ToString(),
                        Text = c.Name,
                        Selected = c.ClinicId == model.ClinicId
                    }).ToList();

                    model.DaysOfWeek = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "شنبه", Value = "0", Selected = model.DayOfWeek == 0 },
                        new SelectListItem { Text = "یکشنبه", Value = "1", Selected = model.DayOfWeek == 1 },
                        new SelectListItem { Text = "دوشنبه", Value = "2", Selected = model.DayOfWeek == 2 },
                        new SelectListItem { Text = "سه‌شنبه", Value = "3", Selected = model.DayOfWeek == 3 },
                        new SelectListItem { Text = "چهارشنبه", Value = "4", Selected = model.DayOfWeek == 4 },
                        new SelectListItem { Text = "پنج‌شنبه", Value = "5", Selected = model.DayOfWeek == 5 },
                        new SelectListItem { Text = "جمعه", Value = "6", Selected = model.DayOfWeek == 6 }
                    };

                    return View(GetViewPath("Edit"), model);
                }

                var result = await _clinicWorkingHoursService.UpdateClinicWorkingHoursAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی ساعات کاری کلینیک: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    
                    // Strongly-Typed: بارگذاری مجدد Lists در ViewModel
                    var clinics = await _context.Set<Clinic>()
                        .Where(c => !c.IsDeleted)
                        .OrderBy(c => c.Name)
                        .ToListAsync();

                    model.Clinics = clinics.Select(c => new SelectListItem
                    {
                        Value = c.ClinicId.ToString(),
                        Text = c.Name,
                        Selected = c.ClinicId == model.ClinicId
                    }).ToList();

                    model.DaysOfWeek = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "شنبه", Value = "0", Selected = model.DayOfWeek == 0 },
                        new SelectListItem { Text = "یکشنبه", Value = "1", Selected = model.DayOfWeek == 1 },
                        new SelectListItem { Text = "دوشنبه", Value = "2", Selected = model.DayOfWeek == 2 },
                        new SelectListItem { Text = "سه‌شنبه", Value = "3", Selected = model.DayOfWeek == 3 },
                        new SelectListItem { Text = "چهارشنبه", Value = "4", Selected = model.DayOfWeek == 4 },
                        new SelectListItem { Text = "پنج‌شنبه", Value = "5", Selected = model.DayOfWeek == 5 },
                        new SelectListItem { Text = "جمعه", Value = "6", Selected = model.DayOfWeek == 6 }
                    };

                    return View(GetViewPath("Edit"), model);
                }

                _logger.Information("ساعات کاری با موفقیت به‌روزرسانی شد - ClinicWorkingHoursId: {ClinicWorkingHoursId}", model.ClinicWorkingHoursId);
                TempData["Success"] = "ساعات کاری با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی ساعات کاری کلینیک - ClinicWorkingHoursId: {ClinicWorkingHoursId}", model.ClinicWorkingHoursId);
                TempData["Error"] = "خطا در به‌روزرسانی ساعات کاری کلینیک";
                
                // Strongly-Typed: بارگذاری Lists در ViewModel در صورت Exception
                var clinics = await _context.Set<Clinic>()
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                model.Clinics = clinics.Select(c => new SelectListItem
                {
                    Value = c.ClinicId.ToString(),
                    Text = c.Name,
                    Selected = c.ClinicId == model.ClinicId
                }).ToList();

                model.DaysOfWeek = new List<SelectListItem>
                {
                    new SelectListItem { Text = "شنبه", Value = "0", Selected = model.DayOfWeek == 0 },
                    new SelectListItem { Text = "یکشنبه", Value = "1", Selected = model.DayOfWeek == 1 },
                    new SelectListItem { Text = "دوشنبه", Value = "2", Selected = model.DayOfWeek == 2 },
                    new SelectListItem { Text = "سه‌شنبه", Value = "3", Selected = model.DayOfWeek == 3 },
                    new SelectListItem { Text = "چهارشنبه", Value = "4", Selected = model.DayOfWeek == 4 },
                    new SelectListItem { Text = "پنج‌شنبه", Value = "5", Selected = model.DayOfWeek == 5 },
                    new SelectListItem { Text = "جمعه", Value = "6", Selected = model.DayOfWeek == 6 }
                };
                
                return View(GetViewPath("Edit"), model);
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
                _logger.Information("درخواست حذف ساعات کاری - ClinicWorkingHoursId: {ClinicWorkingHoursId}", id);

                var result = await _clinicWorkingHoursService.DeleteClinicWorkingHoursAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "ساعات کاری با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف ساعات کاری کلینیک - ClinicWorkingHoursId: {ClinicWorkingHoursId}", id);
                TempData["Error"] = "خطا در حذف ساعات کاری کلینیک";
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
                var result = await _clinicWorkingHoursService.ActivateClinicWorkingHoursAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "ساعات کاری با موفقیت فعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی ساعات کاری کلینیک - ClinicWorkingHoursId: {ClinicWorkingHoursId}", id);
                TempData["Error"] = "خطا در فعال‌سازی ساعات کاری کلینیک";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _clinicWorkingHoursService.DeactivateClinicWorkingHoursAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "ساعات کاری با موفقیت غیرفعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی ساعات کاری کلینیک - ClinicWorkingHoursId: {ClinicWorkingHoursId}", id);
                TempData["Error"] = "خطا در غیرفعال‌سازی ساعات کاری کلینیک";
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}

