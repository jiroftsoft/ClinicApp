using System.Web.Mvc;

namespace ClinicApp.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            // Emergency Booking Routes
            context.MapRoute(
                name: "Admin_EmergencyBooking_Statistics",
                url: "Admin/EmergencyBooking/Statistics/{doctorId}/{startDate}/{endDate}",
                defaults: new { controller = "EmergencyBooking", action = "Statistics" },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_EmergencyBooking_Report",
                url: "Admin/EmergencyBooking/Report/{doctorId}/{startDate}/{endDate}",
                defaults: new { controller = "EmergencyBooking", action = "Report" },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // Schedule Optimization Routes
            context.MapRoute(
                name: "Admin_ScheduleOptimization_DailyResult",
                url: "Admin/ScheduleOptimization/DailyOptimizationResult/{doctorId}/{date}",
                defaults: new { controller = "ScheduleOptimization", action = "DailyOptimizationResult" },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_ScheduleOptimization_WeeklyResult",
                url: "Admin/ScheduleOptimization/WeeklyOptimizationResult/{doctorId}/{weekStart}",
                defaults: new { controller = "ScheduleOptimization", action = "WeeklyOptimizationResult" },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_ScheduleOptimization_MonthlyResult",
                url: "Admin/ScheduleOptimization/MonthlyOptimizationResult/{doctorId}/{monthStart}",
                defaults: new { controller = "ScheduleOptimization", action = "MonthlyOptimizationResult" },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // Appointment Availability Routes
            context.MapRoute(
                name: "Admin_AppointmentAvailability_AvailableDatesResult",
                url: "Admin/AppointmentAvailability/AvailableDatesResult/{doctorId}/{startDate}/{endDate}",
                defaults: new { controller = "AppointmentAvailability", action = "AvailableDatesResult" },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_AppointmentAvailability_AvailableTimeSlotsResult",
                url: "Admin/AppointmentAvailability/AvailableTimeSlotsResult/{doctorId}/{date}",
                defaults: new { controller = "AppointmentAvailability", action = "AvailableTimeSlotsResult" },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // Insurance Routes
            context.MapRoute(
                name: "Admin_InsuranceProvider_Index",
                url: "Admin/InsuranceProvider/{action}/{id}",
                defaults: new { controller = "InsuranceProvider", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            context.MapRoute(
                name: "Admin_InsurancePlan_Index",
                url: "Admin/InsurancePlan/{action}/{id}",
                defaults: new { controller = "InsurancePlan", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            context.MapRoute(
                name: "Admin_PatientInsurance_Index",
                url: "Admin/PatientInsurance/{action}/{id}",
                defaults: new { controller = "PatientInsurance", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            context.MapRoute(
                name: "Admin_InsuranceCalculation_Index",
                url: "Admin/InsuranceCalculation/{action}/{id}",
                defaults: new { controller = "InsuranceCalculation", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            context.MapRoute(
                name: "Admin_Insurance_ManageTariffs",
                url: "Admin/Insurance/ManageTariffs/{insuranceId}",
                defaults: new { controller = "Insurance", action = "ManageTariffs" },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}