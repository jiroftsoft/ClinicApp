using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر اصلی پذیرش - رعایت اصل SRP
    /// مسئولیت: فقط مدیریت CRUD پذیرش‌ها (ایجاد، ویرایش، حذف، نمایش)
    /// </summary>
    public class ReceptionCoreController : BaseController
    {
        private readonly IReceptionService _receptionService;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionCoreController(
            IReceptionService receptionService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Main Views

        /// <summary>
        /// صفحه اصلی پذیرش
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            _logger.Information("🏥 ورود به صفحه اصلی پذیرش, کاربر: {UserName}", _currentUserService.UserName);

            try
            {
                var model = new ReceptionSearchViewModel
                {
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today,
                    Status = null
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری صفحه اصلی پذیرش");
                return View("Error");
            }
        }

        /// <summary>
        /// صفحه ایجاد پذیرش جدید
        /// </summary>
        [HttpGet]
        public ActionResult Create()
        {
            _logger.Information("➕ ورود به صفحه ایجاد پذیرش جدید, کاربر: {UserName}", _currentUserService.UserName);

            try
            {
                var model = new ReceptionCreateViewModel
                {
                    ReceptionDate = DateTime.Now,
                    IsEmergency = false
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری صفحه ایجاد پذیرش");
                return View("Error");
            }
        }

        /// <summary>
        /// صفحه ویرایش پذیرش
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            _logger.Information("✏️ ورود به صفحه ویرایش پذیرش: {ReceptionId}, کاربر: {UserName}", 
                id, _currentUserService.UserName);

            try
            {
                var result = await _receptionService.GetReceptionByIdAsync(id);
                
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                var model = new ReceptionEditViewModel
                {
                    ReceptionId = result.Data.ReceptionId,
                    PatientId = result.Data.PatientId,
                    DoctorId = result.Data.DoctorId,
                    ReceptionDate = DateTime.Parse(result.Data.ReceptionDate),
                    Notes = result.Data.Notes,
                    IsEmergency = false
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری صفحه ویرایش پذیرش: {ReceptionId}", id);
                return View("Error");
            }
        }

        /// <summary>
        /// صفحه جزئیات پذیرش
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            _logger.Information("📋 ورود به صفحه جزئیات پذیرش: {ReceptionId}, کاربر: {UserName}", 
                id, _currentUserService.UserName);

            try
            {
                var result = await _receptionService.GetReceptionDetailsAsync(id);
                
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری صفحه جزئیات پذیرش: {ReceptionId}", id);
                return View("Error");
            }
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// ایجاد پذیرش جدید
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CreateReception(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Information("➕ ایجاد پذیرش جدید: بیمار {PatientId}, خدمت {ServiceId}, پزشک {DoctorId}, کاربر: {UserName}", 
                    model.PatientId, model.ServiceId, model.DoctorId, _currentUserService.UserName);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new { 
                        success = false, 
                        message = "اطلاعات وارد شده نامعتبر است",
                        errors = errors
                    });
                }

                var result = await _receptionService.CreateReceptionAsync(model);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "پذیرش با موفقیت ایجاد شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد پذیرش جدید");
                return Json(new { success = false, message = "خطا در ایجاد پذیرش" });
            }
        }

        /// <summary>
        /// ویرایش پذیرش
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> EditReception(ReceptionEditViewModel model)
        {
            try
            {
                _logger.Information("✏️ ویرایش پذیرش: {ReceptionId}, بیمار {PatientId}, کاربر: {UserName}", 
                    model.ReceptionId, model.PatientId, _currentUserService.UserName);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new { 
                        success = false, 
                        message = "اطلاعات وارد شده نامعتبر است",
                        errors = errors
                    });
                }

                var result = await _receptionService.UpdateReceptionAsync(model);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    message = "پذیرش با موفقیت ویرایش شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش پذیرش: {ReceptionId}", model.ReceptionId);
                return Json(new { success = false, message = "خطا در ویرایش پذیرش" });
            }
        }

        /// <summary>
        /// حذف پذیرش
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> DeleteReception(int id)
        {
            try
            {
                _logger.Information("🗑️ حذف پذیرش: {ReceptionId}, کاربر: {UserName}", 
                    id, _currentUserService.UserName);

                var result = await _receptionService.DeleteReceptionAsync(id);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    message = "پذیرش با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف پذیرش: {ReceptionId}", id);
                return Json(new { success = false, message = "خطا در حذف پذیرش" });
            }
        }

        #endregion

        #region Search & List

        /// <summary>
        /// جستجوی پذیرش‌ها
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> SearchReceptions(ReceptionSearchViewModel model, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("🔍 جستجوی پذیرش‌ها: صفحه {PageNumber}, اندازه {PageSize}, کاربر: {UserName}", 
                    pageNumber, pageSize, _currentUserService.UserName);

                var result = await _receptionService.SearchReceptionsAsync(model, pageNumber, pageSize);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    totalCount = result.TotalCount,
                    pageNumber = pageNumber,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پذیرش‌ها");
                return Json(new { success = false, message = "خطا در جستجو" });
            }
        }

        /// <summary>
        /// دریافت لیست پذیرش‌ها
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetReceptions(int? patientId, int? doctorId, string status, string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.Information("📋 دریافت لیست پذیرش‌ها: بیمار {PatientId}, پزشک {DoctorId}, وضعیت {Status}, صفحه {PageNumber}, کاربر: {UserName}", 
                    patientId, doctorId, status, pageNumber, _currentUserService.UserName);

                var result = await _receptionService.GetReceptionsAsync(patientId, doctorId, null, searchTerm, pageNumber, pageSize);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { 
                    success = true, 
                    data = result.Data,
                    totalCount = result.TotalCount,
                    pageNumber = pageNumber,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پذیرش‌ها");
                return Json(new { success = false, message = "خطا در دریافت لیست" });
            }
        }

        #endregion
    }
}
