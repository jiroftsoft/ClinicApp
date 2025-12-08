using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces;
using ClinicApp.Models.DTOs.Appointment;
using System.Linq;
using System.Collections.Generic;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Patient;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers
{
    /// <summary>
    /// Controller برای مدیریت نوبت‌های بیمار
    /// </summary>
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentBookingService _bookingService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public AppointmentController(
            IAppointmentBookingService bookingService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<AppointmentController>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// نمایش لیست نوبت‌های بیمار
        /// GET: /Patient/Appointment/MyAppointments
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> MyAppointments(
            DateTime? startDate,
            DateTime? endDate,
            AppointmentStatus? status,
            string searchTerm,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                _logger.Information("درخواست نمایش نوبت‌های بیمار - UserId: {UserId}",
                    _currentUserService.UserId);

                // دریافت شناسه بیمار از کاربر فعلی
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    TempData["Error"] = "اطلاعات بیمار یافت نشد. لطفاً دوباره وارد شوید.";
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // دریافت نوبت‌ها
                var result = await _bookingService.GetPatientAppointmentsAsync(
                    patientId.Value,
                    startDate,
                    endDate);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message ?? "خطا در دریافت نوبت‌ها";
                    return View(new PatientAppointmentListViewModel
                    {
                        Appointments = new System.Collections.Generic.List<PatientAppointmentDto>(),
                        PageNumber = page,
                        PageSize = pageSize
                    });
                }

                // فیلتر بر اساس وضعیت
                var appointments = result.Data;
                if (status.HasValue)
                {
                    appointments = appointments.Where(a => a.Status == status.Value).ToList();
                }

                // جستجو بر اساس نام پزشک
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    appointments = appointments
                        .Where(a => a.DoctorName.ToLower().Contains(searchLower))
                        .ToList();
                }

                // Pagination
                var totalCount = appointments.Count;
                var pagedAppointments = appointments
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModel = new ViewModels.Patient.PatientAppointmentListViewModel
                {
                    Appointments = pagedAppointments,
                    StartDateFilter = startDate,
                    EndDateFilter = endDate,
                    StatusFilter = status,
                    SearchTerm = searchTerm,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش نوبت‌های بیمار");
                TempData["Error"] = "خطا در بارگذاری نوبت‌ها";
                return View(new ViewModels.Patient.PatientAppointmentListViewModel
                {
                    Appointments = new System.Collections.Generic.List<PatientAppointmentDto>(),
                    PageNumber = page,
                    PageSize = pageSize
                });
            }
        }

        /// <summary>
        /// نمایش جزئیات یک نوبت
        /// GET: /Patient/Appointment/Details/{id}
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return Json(new { success = false, message = "اطلاعات بیمار یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var result = await _bookingService.GetAppointmentDetailsAsync(id, patientId.Value);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات نوبت {AppointmentId}", id);
                return Json(new { success = false, message = "خطا در دریافت جزئیات نوبت" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// لغو نوبت
        /// POST: /Patient/Appointment/Cancel/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Cancel(int id)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return Json(new { success = false, message = "اطلاعات بیمار یافت نشد" });
                }

                var result = await _bookingService.CancelAppointmentAsync(id, patientId.Value);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                TempData["Success"] = "نوبت با موفقیت لغو شد";
                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو نوبت {AppointmentId}", id);
                return Json(new { success = false, message = "خطا در لغو نوبت" });
            }
        }

        #region Helper Methods

        /// <summary>
        /// دریافت شناسه بیمار از کاربر فعلی
        /// </summary>
        private async Task<int?> GetCurrentPatientIdAsync()
        {
            try
            {
                var patient = await _currentUserService.GetPatientInfoAsync();
                return patient?.PatientId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت شناسه بیمار");
                return null;
            }
        }

        #endregion
    }
}

