using System.Web.Mvc;

namespace ClinicApp.Areas.Patient
{
    /// <summary>
    /// ثبت Area برای Patient Portal
    /// این Area برای دسترسی بیماران به سیستم رزرو نوبت آنلاین استفاده می‌شود
    /// </summary>
    public class PatientAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Patient";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            // ✅ Route برای Appointment Booking
            context.MapRoute(
                name: "Patient_AppointmentBooking_SelectDoctor",
                url: "Patient/Appointment/Book/SelectDoctor",
                defaults: new { controller = "AppointmentBooking", action = "SelectDoctor" },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers" }
            );

            context.MapRoute(
                name: "Patient_AppointmentBooking_SelectDate",
                url: "Patient/Appointment/Book/SelectDate/{doctorId}",
                defaults: new { controller = "AppointmentBooking", action = "SelectDate" },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers" }
            );

            context.MapRoute(
                name: "Patient_AppointmentBooking_SelectTime",
                url: "Patient/Appointment/Book/SelectTime/{doctorId}/{date}",
                defaults: new { controller = "AppointmentBooking", action = "SelectTime" },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers" }
            );

            context.MapRoute(
                name: "Patient_AppointmentBooking_Confirm",
                url: "Patient/Appointment/Book/Confirm",
                defaults: new { controller = "AppointmentBooking", action = "ConfirmBooking" },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers" }
            );

            // ✅ Route برای API Endpoints
            context.MapRoute(
                name: "Patient_API_Appointments",
                url: "api/patient/appointments/{action}/{id}",
                defaults: new { controller = "PatientAppointmentApi", action = "GetAppointments", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers.Api" }
            );

            context.MapRoute(
                name: "Patient_API_Doctors",
                url: "api/patient/doctors/{action}/{id}",
                defaults: new { controller = "AppointmentBookingApi", action = "GetDoctors", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers.Api" }
            );

            context.MapRoute(
                name: "Patient_API_Slots",
                url: "api/patient/doctors/{doctorId}/slots/{date}",
                defaults: new { controller = "AppointmentBookingApi", action = "GetAvailableSlots" },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers.Api" }
            );

            // ✅ Default Route برای Patient Area - با constraint برای جلوگیری از conflict
            // فقط Appointment و AppointmentBooking controllers را قبول می‌کند
            context.MapRoute(
                "Patient_default",
                "Patient/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new { controller = @"^(Appointment|AppointmentBooking)$" }, // ✅ CRITICAL: فقط controllers موجود
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers" }
            );
        }
    }
}

