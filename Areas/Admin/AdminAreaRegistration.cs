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

            // ✅ Insurance Tariff Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_InsuranceTariff_Index",
                url: "Admin/InsuranceTariff/{action}/{id}",
                defaults: new { controller = "InsuranceTariff", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            context.MapRoute(
                name: "Admin_PatientInsurance_Index",
                url: "Admin/Insurance/PatientInsurance/{action}/{id}",
                defaults: new { controller = "PatientInsurance", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            // اضافه کردن routing برای URL کوتاه‌تر - اصلاح شده
            context.MapRoute(
                name: "Admin_PatientInsurance_Short",
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

            // Supplementary Insurance Routes
            context.MapRoute(
                name: "Admin_SupplementaryInsurance_Index",
                url: "Admin/Insurance/Supplementary/{action}/{id}",
                defaults: new { controller = "SupplementaryInsurance", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            // Supplementary Tariff Routes
            context.MapRoute(
                name: "Admin_SupplementaryTariff_Index",
                url: "Admin/SupplementaryTariff/{action}/{id}",
                defaults: new { controller = "SupplementaryTariff", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            // ✅ BusinessRule در Insurance است — مسیر صریح برای Admin/BusinessRule و Admin/CMS/BusinessRule
            context.MapRoute(
                name: "Admin_BusinessRule_Index",
                url: "Admin/BusinessRule/{action}/{id}",
                defaults: new { controller = "BusinessRule", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            context.MapRoute(
                name: "Admin_CombinedInsuranceCalculation_Index",
                url: "Admin/Insurance/CombinedInsuranceCalculation/{action}/{id}",
                defaults: new { controller = "CombinedInsuranceCalculation", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            // Supplementary Insurance API Routes
            context.MapRoute(
                name: "Admin_SupplementaryInsurance_Calculate",
                url: "Admin/Insurance/CombinedInsuranceCalculation/CalculateSupplementary",
                defaults: new { controller = "CombinedInsuranceCalculation", action = "CalculateSupplementary" },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            context.MapRoute(
                name: "Admin_SupplementaryInsurance_Tariffs",
                url: "Admin/Insurance/CombinedInsuranceCalculation/SupplementaryTariffs/{planId}",
                defaults: new { controller = "CombinedInsuranceCalculation", action = "GetSupplementaryTariffs" },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            context.MapRoute(
                name: "Admin_SupplementaryInsurance_Settings",
                url: "Admin/Insurance/CombinedInsuranceCalculation/SupplementarySettings/{planId}",
                defaults: new { controller = "CombinedInsuranceCalculation", action = "GetSupplementarySettings" },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            );

            // ✅ Clinic Routes - باید قبل از CMS باشد تا تداخل نکند
            context.MapRoute(
                name: "Admin_Clinic_Routes",
                url: "Admin/Clinic/{action}/{id}",
                defaults: new { controller = "Clinic", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ Department Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_Department_Routes",
                url: "Admin/Department/{action}/{id}",
                defaults: new { controller = "Department", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ Doctor Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_Doctor_Routes",
                url: "Admin/Doctor/{action}/{id}",
                defaults: new { controller = "Doctor", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ Service Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_Service_Routes",
                url: "Admin/Service/{action}/{id}",
                defaults: new { controller = "Service", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ SystemSeed Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_SystemSeed_Routes",
                url: "Admin/SystemSeed/{action}/{id}",
                defaults: new { controller = "SystemSeed", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ FactorSetting Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_FactorSetting_Routes",
                url: "Admin/FactorSetting/{action}/{id}",
                defaults: new { controller = "FactorSetting", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ InsuranceTypeUpdate Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_InsuranceTypeUpdate_Routes",
                url: "Admin/InsuranceTypeUpdate/{action}/{id}",
                defaults: new { controller = "InsuranceTypeUpdate", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ ClinicBankAccount Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_ClinicBankAccount_Routes",
                url: "Admin/ClinicBankAccount/{action}/{id}",
                defaults: new { controller = "ClinicBankAccount", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ Specialization Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_Specialization_Routes",
                url: "Admin/Specialization/{action}/{id}",
                defaults: new { controller = "Specialization", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ Doctor Related Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_DoctorSchedule_Routes",
                url: "Admin/DoctorSchedule/{action}/{id}",
                defaults: new { controller = "DoctorSchedule", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_DoctorAssignment_Routes",
                url: "Admin/DoctorAssignment/{action}/{id}",
                defaults: new { controller = "DoctorAssignment", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_DoctorDashboard_Routes",
                url: "Admin/DoctorDashboard/{action}/{id}",
                defaults: new { controller = "DoctorDashboard", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // مشاوره آنلاین تصویری — Admin/OnlineConsultation/Join/123
            context.MapRoute(
                name: "Admin_OnlineConsultation_Join",
                url: "Admin/OnlineConsultation/Join/{id}",
                defaults: new { controller = "OnlineConsultation", action = "Join", area = "Admin" },
                constraints: new { id = @"^\d+$" },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_DoctorReporting_Routes",
                url: "Admin/DoctorReporting/{action}/{id}",
                defaults: new { controller = "DoctorReporting", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ DoctorTimeSlot Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_DoctorTimeSlot_Routes",
                url: "Admin/DoctorTimeSlot/{action}/{id}",
                defaults: new { controller = "DoctorTimeSlot", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ LoginHistory Routes - Security Module
            context.MapRoute(
                name: "Admin_LoginHistory_Routes",
                url: "Admin/Security/LoginHistory/{action}/{id}",
                defaults: new { controller = "LoginHistory", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Security" }
            );

            // ✅ Service Related Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_ServiceComponent_Routes",
                url: "Admin/ServiceComponent/{action}/{id}",
                defaults: new { controller = "ServiceComponent", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_ServiceTemplate_Routes",
                url: "Admin/ServiceTemplate/{action}/{id}",
                defaults: new { controller = "ServiceTemplate", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_SharedService_Routes",
                url: "Admin/SharedService/{action}/{id}",
                defaults: new { controller = "SharedService", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_ServiceManagement_Routes",
                url: "Admin/ServiceManagement/{action}/{id}",
                defaults: new { controller = "ServiceManagement", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ Appointment Related Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_EmergencyBooking_Routes",
                url: "Admin/EmergencyBooking/{action}/{id}",
                defaults: new { controller = "EmergencyBooking", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_AppointmentAvailability_Routes",
                url: "Admin/AppointmentAvailability/{action}/{id}",
                defaults: new { controller = "AppointmentAvailability", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            context.MapRoute(
                name: "Admin_ScheduleOptimization_Routes",
                url: "Admin/ScheduleOptimization/{action}/{id}",
                defaults: new { controller = "ScheduleOptimization", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ UserManagement Routes - باید قبل از CMS باشد
            context.MapRoute(
                name: "Admin_UserManagement_Routes",
                url: "Admin/UserManagement/{action}/{id}",
                defaults: new { controller = "UserManagement", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            );

            // ✅ PromotionalEvent Routes - طبق 08-MVC-Routing-Best-Practices.md
            // Route خاص قبل از CMS route برای جلوگیری از match شدن به CMS
            // ⚠️ CRITICAL: باید قبل از Admin_CMS_Default باشد (خاص قبل از عمومی)
            context.MapRoute(
                name: "Admin_PromotionalEvent_Routes",
                url: "Admin/PromotionalEvent/{action}/{id}",
                defaults: new { controller = "PromotionalEvent", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false; // ✅ طبق Best Practices

            // ✅ DoctorServiceCategory در Admin است نه CMS — مسیر صریح تا لینک اشتباه /Admin/CMS/DoctorServiceCategory هم کار کند
            context.MapRoute(
                name: "Admin_DoctorServiceCategory_Routes",
                url: "Admin/DoctorServiceCategory/{action}/{id}",
                defaults: new { controller = "DoctorServiceCategory", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false;

            // مسیر جایگزین: اگر لینک اشتباه /Admin/CMS/DoctorServiceCategory زده شد، همان کنترلر Admin سرویس دهد
            context.MapRoute(
                name: "Admin_CMS_DoctorServiceCategory_Fix",
                url: "Admin/CMS/DoctorServiceCategory/{action}/{id}",
                defaults: new { controller = "DoctorServiceCategory", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false;

            // مسیر جایگزین: اگر لینک /Admin/CMS/BusinessRule زده شد — کنترلر BusinessRule در Insurance است نه CMS
            context.MapRoute(
                name: "Admin_CMS_BusinessRule_Fix",
                url: "Admin/CMS/BusinessRule/{action}/{id}",
                defaults: new { controller = "BusinessRule", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.Insurance" }
            ).DataTokens["UseNamespaceFallback"] = false;

            // مسیر جایگزین: اگر لینک /Admin/CMS/PaymentManagement زده شد — کنترلر PaymentManagement در Admin است نه CMS
            context.MapRoute(
                name: "Admin_CMS_PaymentManagement_Fix",
                url: "Admin/CMS/PaymentManagement/{action}/{id}",
                defaults: new { controller = "PaymentManagement", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false;

            // مسیر جایگزین: داشبورد صندوق — /Admin/CMS/CashierDashboard
            context.MapRoute(
                name: "Admin_CMS_CashierDashboard_Fix",
                url: "Admin/CMS/CashierDashboard/{action}/{id}",
                defaults: new { controller = "CashierDashboard", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false;

            // مسیر جایگزین: داشبورد منشی — /Admin/CMS/ReceptionistDashboard → کنترلر Admin (نه CMS)
            context.MapRoute(
                name: "Admin_CMS_ReceptionistDashboard_Fix",
                url: "Admin/CMS/ReceptionistDashboard/{action}/{id}",
                defaults: new { controller = "ReceptionistDashboard", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false;

            // CMS Home: صفحهٔ ورود CMS — گرید ماژول‌ها
            context.MapRoute(
                name: "Admin_CMS_Home",
                url: "Admin/CMS",
                defaults: new { controller = "CmsHome", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.CMS" }
            ).DataTokens["UseNamespaceFallback"] = false;

            // CMS Routes - مسیرهای CMS (باید قبل از Admin default باشد تا اولویت داشته باشد)
            context.MapRoute(
                name: "Admin_CMS_Default",
                url: "Admin/CMS/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers.CMS" }
            ).DataTokens["UseNamespaceFallback"] = false; // ✅ طبق Best Practices

            // Admin default route - باید بعد از CMS route باشد
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Areas.Admin.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false; // ✅ طبق Best Practices - اضافه شد
        }
    }
}