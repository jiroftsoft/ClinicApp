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
            // ✅ BEST PRACTICE: Route خاص قبل از default route + UseNamespaceFallback = false
            // ✅ CRITICAL FIX: اضافه کردن optional route parameter برای departmentId
            // این route هم URL بدون پارامتر و هم با departmentId را می‌پذیرد
            // مثال: /Patient/Appointment/Book/SelectDoctor و /Patient/Appointment/Book/SelectDoctor/2
            context.MapRoute(
                name: "Patient_AppointmentBooking_SelectDoctor",
                url: "Patient/Appointment/Book/SelectDoctor/{departmentId}",
                defaults: new { controller = "AppointmentBooking", action = "SelectDoctor", area = "Patient", departmentId = UrlParameter.Optional },
                constraints: new { departmentId = @"^\d*$" }, // ✅ فقط عدد (یا خالی برای optional)
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false; // ✅ طبق 08-MVC-Routing-Best-Practices.md

            // ✅ BEST PRACTICE: Route خاص قبل از default route + UseNamespaceFallback = false
            // ✅ CRITICAL FIX: اضافه کردن constraint برای doctorId تا فقط عدد باشد
            context.MapRoute(
                name: "Patient_AppointmentBooking_SelectDate",
                url: "Patient/Appointment/Book/SelectDate/{doctorId}",
                defaults: new { controller = "AppointmentBooking", action = "SelectDate", area = "Patient" },
                constraints: new { doctorId = @"^\d+$" }, // ✅ فقط عدد مثبت
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false; // ✅ طبق 08-MVC-Routing-Best-Practices.md

            // ✅ CRITICAL FIX: اضافه کردن constraint برای doctorId و date
            context.MapRoute(
                name: "Patient_AppointmentBooking_SelectTime",
                url: "Patient/Appointment/Book/SelectTime/{doctorId}/{date}",
                defaults: new { controller = "AppointmentBooking", action = "SelectTime", area = "Patient" },
                constraints: new { 
                    doctorId = @"^\d+$", // ✅ فقط عدد مثبت
                    date = @"^\d{4}-\d{2}-\d{2}$" // ✅ فرمت تاریخ: YYYY-MM-DD
                },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false;

            context.MapRoute(
                name: "Patient_AppointmentBooking_Confirm",
                url: "Patient/Appointment/Book/Confirm",
                defaults: new { controller = "AppointmentBooking", action = "ConfirmBooking", area = "Patient" },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false;

            // ✅ Route برای Dashboard
            context.MapRoute(
                name: "Patient_Dashboard",
                url: "Patient/Dashboard/{action}/{id}",
                defaults: new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers" }
            );

            // ✅ Route برای Dashboard API
            context.MapRoute(
                name: "Patient_API_Dashboard",
                url: "Patient/Api/PatientDashboard/{action}/{id}",
                defaults: new { controller = "PatientDashboardApi", action = "GetQuickStats", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers.Api" }
            );

            // ✅ Route برای Medical Record API
            context.MapRoute(
                name: "Patient_API_MedicalRecord",
                url: "Patient/Api/MedicalRecord/{action}/{id}",
                defaults: new { controller = "MedicalRecordApi", action = "GetMedicalHistories", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers.Api" }
            );

            // ✅ Route برای Profile API
            context.MapRoute(
                name: "Patient_API_Profile",
                url: "Patient/Api/Profile/{action}/{id}",
                defaults: new { controller = "ProfileApi", action = "GetProfile", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers.Api" }
            );

            // ✅ Route برای DoctorSearch API
            context.MapRoute(
                name: "Patient_API_DoctorSearch",
                url: "Patient/Api/DoctorSearch/{action}/{id}",
                defaults: new { controller = "DoctorSearchApi", action = "GetAvailableDoctors", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers.Api" }
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
            // فقط Patient Area controllers اصلی را قبول می‌کند
            // ✅ BEST PRACTICE: UseNamespaceFallback = false (طبق 08-MVC-Routing-Best-Practices.md)
            context.MapRoute(
                "Patient_default",
                "Patient/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional, area = "Patient" },
                new { controller = @"^(Appointment|AppointmentBooking|Dashboard|Settings|Profile|MedicalRecord)$" }, // ✅ CRITICAL: فقط controllers موجود
                namespaces: new[] { "ClinicApp.Areas.Patient.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false; // ✅ طبق 08-MVC-Routing-Best-Practices.md
        }
    }
}

