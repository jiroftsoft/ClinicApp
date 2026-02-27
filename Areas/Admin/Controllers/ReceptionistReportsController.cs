using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Core;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// گزارش‌های منشی — مسئولیت: فقط هماهنگی درخواست و نمایش (داده توسط سرویس).
    /// </summary>
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    public class ReceptionistReportsController : Controller
    {
        private readonly IPatientBookedAppointmentsReportService _reportService;
        private readonly ILogger _logger = Log.ForContext<ReceptionistReportsController>();

        public ReceptionistReportsController(IPatientBookedAppointmentsReportService reportService)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        }

        /// <summary>
        /// لیست نوبت‌های رزرو شده توسط بیماران — با فیلتر تاریخ، نوع ویزیت و قابلیت چاپ.
        /// visitType: all | inperson | online
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> PatientBookedAppointments(string fromDateShamsi, string toDateShamsi, string visitType = "all")
        {
            DateTime? from = null, to = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(fromDateShamsi))
                    from = PersianDateHelper.ToGregorianDate(fromDateShamsi.Trim());
                if (!string.IsNullOrWhiteSpace(toDateShamsi))
                    to = PersianDateHelper.ToGregorianDate(toDateShamsi.Trim());
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "تاریخ شمسی نامعتبر در فیلتر گزارش: از={From} تا={To}", fromDateShamsi, toDateShamsi);
            }

            var vt = string.IsNullOrWhiteSpace(visitType) ? "all" : visitType.Trim();
            if (vt != "inperson" && vt != "online") vt = "all";

            var result = await _reportService.GetReportAsync(from, to, vt).ConfigureAwait(false);
            ViewBag.FromDateShamsi = fromDateShamsi;
            ViewBag.ToDateShamsi = toDateShamsi;
            ViewBag.VisitType = vt;
            return View(result);
        }

        /// <summary>
        /// صفحهٔ خالص چاپ گزارش (بدون منو و هدر ادمین) — قالب حرفه‌ای مخصوص پروداکشن.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> PatientBookedAppointmentsPrint(string fromDateShamsi, string toDateShamsi, string visitType = "all")
        {
            DateTime? from = null, to = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(fromDateShamsi))
                    from = PersianDateHelper.ToGregorianDate(fromDateShamsi.Trim());
                if (!string.IsNullOrWhiteSpace(toDateShamsi))
                    to = PersianDateHelper.ToGregorianDate(toDateShamsi.Trim());
            }
            catch { /* ignore */ }

            var vt = string.IsNullOrWhiteSpace(visitType) ? "all" : visitType.Trim();
            if (vt != "inperson" && vt != "online") vt = "all";

            var result = await _reportService.GetReportAsync(from, to, vt).ConfigureAwait(false);
            ViewBag.FromDateShamsi = fromDateShamsi ?? "—";
            ViewBag.ToDateShamsi = toDateShamsi ?? "—";
            ViewBag.VisitType = vt;
            ViewBag.Title = "چاپ گزارش نوبت‌های رزرو شده توسط بیماران";
            return View("PatientBookedAppointmentsPrint", result);
        }
    }
}
