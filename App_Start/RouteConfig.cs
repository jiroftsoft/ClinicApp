using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ClinicApp
{
    /// <summary>
    /// Custom route constraint to match URLs ending with .map extension
    /// </summary>
    public class MapFileConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values[parameterName] == null)
                return false;

            string path = values[parameterName].ToString();
            return path.EndsWith(".map", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            // Ignore source map files (.map) to prevent 404 errors
            // These files should be served as static content, not routed through MVC
            routes.IgnoreRoute("{*mapfile}", new { mapfile = new MapFileConstraint() });
            
            // Ignore .well-known paths (used by browsers and tools like Chrome DevTools)
            routes.IgnoreRoute(".well-known/{*pathInfo}");
            
            // Ignore other static file extensions that might be requested
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("robots.txt");
            
            // Enable Attribute Routing
            routes.MapMvcAttributeRoutes();

            // 🏥 CRITICAL: Patient Controller Route - باید قبل از ApiPatientController باشد
            // این route باید قبل از Default route باشد و conflict با Api/PatientController را جلوگیری کند
            // ✅ CRITICAL FIX: اضافه کردن constraint برای جلوگیری از conflict با Area routes
            // فقط action های مشخص PatientController را قبول می‌کند (نه Appointment/Book/...)
            // ⚠️ IMPORTANT: این route فقط برای PatientController در root namespace است
            // Area routes (Patient/Appointment/Book/...) باید قبل از این route match شوند
            routes.MapRoute(
                name: "Patient_Specific",
                url: "Patient/{action}/{id}",
                defaults: new { controller = "Patient", action = "Index", id = UrlParameter.Optional },
                constraints: new { 
                    // ✅ CRITICAL: فقط action های مشخص PatientController را قبول می‌کند
                    // Negative lookahead: action نباید با "Appointment" شروع شود (برای جلوگیری از conflict با Area routes)
                    action = @"^(?!Appointment)(Index|Edit|Create|Delete|Details|LoadPatients|Search)$"
                },
                namespaces: new[] { "ClinicApp.Controllers" } // ✅ ONLY MVC PatientController
            ).DataTokens["UseNamespaceFallback"] = false; // ❌ جلوگیری از fallback به namespace های دیگر

            // Redirect route for incorrect View URLs to correct Controller URLs
            // مثال: /Areas/Admin/Views/CMS/ClinicWorkingHours/Index.cshtml -> /Admin/CMS/ClinicWorkingHours
            routes.MapRoute(
                name: "Redirect_ViewToController",
                url: "Areas/{area}/Views/{*path}",
                defaults: new { controller = "Redirect", action = "ViewToController" },
                constraints: new { area = @"Admin|Patient" }
            );

            // Redirect route for incorrect View URLs (non-Area Views)
            // مثال: /Views/Payment/CashierReport/Index.cshtml -> /Payment/CashierReport/Index
            // مثال: /Views/Payment/Index.cshtml -> /Payment/Index
            routes.MapRoute(
                name: "Redirect_ViewsToController",
                url: "Views/{*path}",
                defaults: new { controller = "Redirect", action = "ViewsToController" }
            );

            // Specific routes for dashed paths that don't map to action names directly
            routes.MapRoute(
                name: "ReceptionApiV1_DraftCreate",
                url: "api/v1/reception/draft/create",
                defaults: new { controller = "ReceptionApi", action = "CreateDraft", area = "" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") },
                namespaces: new[] { "ClinicApp.Controllers.Api" }
            );
            routes.MapRoute(
                name: "ReceptionApiV1_PatientLookupOrCreate",
                url: "api/v1/reception/patient/lookup-or-create",
                defaults: new { controller = "ReceptionApi", action = "PatientLookup", area = "" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") },
                namespaces: new[] { "ClinicApp.Controllers.Api" }
            );
            // Legacy API route for MVC controllers under ClinicApp.Controllers.Api
            routes.MapRoute(
                name: "ReceptionApiLegacy",
                url: "Api/ReceptionApi/{action}",
                defaults: new { controller = "ReceptionApi", action = "Index", area = "" },
                namespaces: new[] { "ClinicApp.Controllers.Api" }
            );
            
            // 🏥 API Patient Controller Route - برای جلوگیری از conflict با MVC Patient Controller
            // این controller در واقع یک MVC Controller است که در namespace Api قرار گرفته
            routes.MapRoute(
                name: "ApiPatientController",
                url: "Api/Patient/{action}/{id}",
                defaults: new { controller = "Patient", action = "Search", area = "", id = UrlParameter.Optional },
                // ❌ حذف constraint httpMethod برای support کردن GET/POST
                namespaces: new[] { "ClinicApp.Controllers.Api" }
            ).DataTokens["UseNamespaceFallback"] = false; // ✅ جلوگیری از fallback به namespace های دیگر
            
            // 🏥 API ReceptionApi Controller Route - برای Legacy API endpoints
            routes.MapRoute(
                name: "ApiReceptionApiController",
                url: "Api/ReceptionApi/{action}/{id}",
                defaults: new { controller = "ReceptionApi", action = "GetReceptionDetails", area = "", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Controllers.Api" }
            ).DataTokens["UseNamespaceFallback"] = false; // ✅ جلوگیری از fallback به namespace های دیگر
            
            // 🏥 V2: Reception V2 Route
            routes.MapRoute(
                name: "ReceptionV2",
                url: "reception/v2",
                defaults: new { controller = "ReceptionV2", action = "Index", area = "" },
                namespaces: new[] { "ClinicApp.Controllers.ReceptionV2" }
            );
            
            // 🏥 V2: Reception List V2 Route - نسخه بهینه‌شده
            routes.MapRoute(
                name: "ReceptionListV2",
                url: "ReceptionV2/ReceptionList/{action}",
                defaults: new { 
                    controller = "ReceptionListV2", 
                    action = "Index", 
                    area = ""
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(Index|GetReceptionList)$"
                },
                namespaces: new[] { "ClinicApp.Controllers.ReceptionV2" }
            );
            
            // 🏥 Legacy: Reception List Route - Redirect to V2
            routes.MapRoute(
                name: "ReceptionList",
                url: "Reception/ReceptionList/{action}",
                defaults: new { 
                    controller = "ReceptionListV2", 
                    action = "Index", 
                    area = ""
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(Index|GetReceptionList)$"
                },
                namespaces: new[] { "ClinicApp.Controllers.ReceptionV2" }
            );
            
            // 🏥 Legacy: Reception History Routes - Redirect to V2
            routes.MapRoute(
                name: "ReceptionHistoryShort",
                url: "Reception/History",
                defaults: new { 
                    controller = "ReceptionListV2", 
                    action = "Index", 
                    area = ""
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET")
                },
                namespaces: new[] { "ClinicApp.Controllers.ReceptionV2" }
            );
            
            routes.MapRoute(
                name: "ReceptionHistory",
                url: "Reception/ReceptionHistory/{action}",
                defaults: new { 
                    controller = "ReceptionListV2", 
                    action = "Index", 
                    area = ""
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(Index|GetReceptionList)$"
                },
                namespaces: new[] { "ClinicApp.Controllers.ReceptionV2" }
            );

            // 🏥 Reception Module Routes - مسیرهای ماژول پذیرش
            routes.MapRoute(
                name: "ReceptionAlert",
                url: "Reception/Alert/{action}",
                defaults: new { controller = "ReceptionAlert", action = "GetMedicalAlerts" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET", "POST") }
            );

            routes.MapRoute(
                name: "ReceptionStatistics",
                url: "Reception/Statistics/{action}",
                defaults: new { controller = "ReceptionStatistics", action = "GetStatistics" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET", "POST") }
            );

            routes.MapRoute(
                name: "ReceptionInsuranceStatus",
                url: "Reception/Insurance/{action}",
                defaults: new { controller = "ReceptionInsurance", action = "GetInsuranceStatus" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET", "POST") }
            );

            routes.MapRoute(
                name: "ReceptionPaymentStatus",
                url: "Reception/Payment/{action}",
                defaults: new { controller = "ReceptionPayment", action = "GetPaymentStatus" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET", "POST") }
            );

            routes.MapRoute(
                name: "ReceptionDepartmentList",
                url: "Reception/Department/{action}",
                defaults: new { controller = "ReceptionDepartment", action = "GetDepartments" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET", "POST") }
            );

            // 🧮 Reception Calculation Routes - مسیرهای محاسبات پذیرش
            routes.MapRoute(
                name: "ReceptionCalculation",
                url: "Reception/Calculation/{action}",
                defaults: new { controller = "ReceptionCalculation", action = "CalculateReception" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET", "POST") }
            );

            // 📋 Reception Form Routes - مسیرهای فرم پذیرش
            routes.MapRoute(
                name: "ReceptionForm",
                url: "Reception/Form/{action}",
                defaults: new { controller = "ReceptionForm", action = "CreateReception" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET", "POST") }
            );

            // 🔒 Medical Environment Routes - با اطمینان 100%
            routes.MapRoute(
                name: "ServiceIndex",
                url: "Admin/Service",
                defaults: new { controller = "Service", action = "Index", area = "Admin" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                name: "ServiceIndexWithCategory",
                url: "Admin/Service/{serviceCategoryId}",
                defaults: new { controller = "Service", action = "Index", area = "Admin" },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET"),
                    serviceCategoryId = @"^\d+$" // فقط اعداد
                }
            );

            // 🏥 Combined Insurance Calculation Routes - مسیرهای محاسبه بیمه ترکیبی
            routes.MapRoute(
                name: "CombinedInsuranceCalculation",
                url: "Admin/CombinedInsuranceCalculation/{action}/{id}",
                defaults: new { 
                    controller = "CombinedInsuranceCalculation", 
                    action = "Index", 
                    area = "Admin",
                    id = UrlParameter.Optional 
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(Index|Calculate|GetPatientInsurances|GetServices|GetSupplementaryTariffs|UpdateSupplementarySettings|CreateSupplementaryTariff|EditSupplementaryTariff|ViewSupplementaryTariffDetails|DeleteSupplementaryTariff|CalculateSupplementary)$"
                }
            );


            // 🔄 AJAX API Routes for Combined Insurance Calculation
            routes.MapRoute(
                name: "CombinedInsuranceCalculationAPI",
                url: "Admin/CombinedInsuranceCalculation/API/{action}",
                defaults: new { 
                    controller = "CombinedInsuranceCalculation", 
                    action = "GetPatientInsurances", 
                    area = "Admin"
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(GetPatientInsurances|GetServices|GetSupplementaryTariffs|CalculateSupplementary|UpdateSupplementarySettings)$"
                }
            );

            // 📋 Supplementary Tariff Management Routes
            routes.MapRoute(
                name: "SupplementaryTariffManagement",
                url: "Admin/CombinedInsuranceCalculation/Tariff/{action}/{id}",
                defaults: new { 
                    controller = "CombinedInsuranceCalculation", 
                    action = "CreateSupplementaryTariff", 
                    area = "Admin",
                    id = UrlParameter.Optional 
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(CreateSupplementaryTariff|EditSupplementaryTariff|ViewSupplementaryTariffDetails|DeleteSupplementaryTariff)$"
                }
            );

            // ⚙️ Settings and Configuration Routes
            routes.MapRoute(
                name: "CombinedInsuranceSettings",
                url: "Admin/CombinedInsuranceCalculation/Settings/{action}",
                defaults: new { 
                    controller = "CombinedInsuranceCalculation", 
                    action = "UpdateSupplementarySettings", 
                    area = "Admin"
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(UpdateSupplementarySettings|GetSupplementarySettings)$"
                }
            );

            // 📊 Calculation and Reporting Routes
            routes.MapRoute(
                name: "CombinedInsuranceCalculationReports",
                url: "Admin/CombinedInsuranceCalculation/Reports/{action}",
                defaults: new { 
                    controller = "CombinedInsuranceCalculation", 
                    action = "Calculate", 
                    area = "Admin"
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(Calculate|GetCalculationHistory|ExportCalculationReport)$"
                }
            );

            // 🔍 Search and Filter Routes
            routes.MapRoute(
                name: "CombinedInsuranceCalculationSearch",
                url: "Admin/CombinedInsuranceCalculation/Search/{action}",
                defaults: new { 
                    controller = "CombinedInsuranceCalculation", 
                    action = "SearchPatients", 
                    area = "Admin"
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(SearchPatients|SearchServices|FilterByDate|FilterByInsuranceType)$"
                }
            );

            // 🏥 Medical Environment Specific Routes
            routes.MapRoute(
                name: "CombinedInsuranceCalculationMedical",
                url: "Admin/CombinedInsuranceCalculation/Medical/{action}",
                defaults: new { 
                    controller = "CombinedInsuranceCalculation", 
                    action = "GetMedicalServices", 
                    area = "Admin"
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(GetMedicalServices|GetPatientMedicalHistory|CalculateMedicalInsurance|GetMedicalTariffs)$"
                }
            );

            // 🔐 Security and Authorization Routes
            routes.MapRoute(
                name: "CombinedInsuranceCalculationSecurity",
                url: "Admin/CombinedInsuranceCalculation/Security/{action}",
                defaults: new { 
                    controller = "CombinedInsuranceCalculation", 
                    action = "CheckAccess", 
                    area = "Admin"
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(CheckAccess|ValidateUser|CheckPermissions|AuditLog)$"
                }
            );

            // 📱 Mobile and API Routes
            routes.MapRoute(
                name: "CombinedInsuranceCalculationMobile",
                url: "Admin/CombinedInsuranceCalculation/Mobile/{action}",
                defaults: new { 
                    controller = "CombinedInsuranceCalculation", 
                    action = "GetMobileData", 
                    area = "Admin"
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(GetMobileData|MobileCalculate|MobileSearch|MobileReports)$"
                }
            );

            // 🏥 Reception Insurance Form Routes - مسیرهای تخصصی بیمه در فرم پذیرش
            routes.MapRoute(
                name: "ReceptionInsuranceForm",
                url: "ReceptionInsuranceForm/{action}",
                defaults: new { 
                    controller = "ReceptionInsuranceForm", 
                    action = "GetInsuranceProviders", 
                    area = ""
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(GetInsuranceProviders|GetInsuranceProvidersByType|GetInsurancePlans|GetAllInsurancePlans|GetPatientInsurances|SavePatientInsurance|CalculateInsuranceShare)$"
                }
            );

            // 🏥 Reception Patient Search Routes - مسیرهای جستجوی بیمار
            routes.MapRoute(
                name: "ReceptionPatientSearch",
                url: "Reception/PatientSearch/{action}",
                defaults: new { 
                    controller = "ReceptionPatientSearch", 
                    action = "Index", 
                    area = ""
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(Index|SearchPatients)$"
                }
            );

            // 🏥 Reception Patient Controller Routes - مسیرهای کنترلر بیمار
            routes.MapRoute(
                name: "ReceptionPatient",
                url: "Reception/Patient/{action}",
                defaults: new { 
                    controller = "ReceptionPatient", 
                    action = "Index", 
                    area = ""
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(Index|SearchByNationalCode|SavePatient|UpdatePatient)$"
                }
            );

       // 🏥 Reception Insurance Controller Routes - مسیرهای کنترلر بیمه
       routes.MapRoute(
           name: "ReceptionInsurance",
           url: "Reception/Insurance/{action}",
           defaults: new { 
               controller = "ReceptionInsurance", 
               action = "Load", 
               area = ""
           },
            constraints: new { 
                httpMethod = new HttpMethodConstraint("GET", "POST"),
                action = @"^(Load|Save|ValidatePatientInsurance|QuickValidateInsurance|GetPatientInsuranceStatus|CalculateInsuranceShare|InquiryPatientIdentity|GetInsuranceProviders|GetPrimaryInsuranceProviders|GetSupplementaryInsuranceProviders|GetInsurancePlans|GetPrimaryInsurancePlans|GetSupplementaryInsurancePlans|GetSupplementaryInsurances|CalculateInsurance|ChangePatientInsurance|SavePatientInsurance)$"
            }
       );


            routes.MapRoute(
                name: "ReceptionInsuranceAuto",
                url: "Reception/InsuranceAuto/{action}",
                defaults: new { controller = "ReceptionInsuranceAuto", action = "AutoBindPatientInsurance" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET", "POST") }
            );

            routes.MapRoute(
                name: "ReceptionDepartmentDoctor",
                url: "Reception/DepartmentDoctor/{action}",
                defaults: new { controller = "ReceptionDepartmentDoctor", action = "GetActiveClinics" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET", "POST") }
            );

            routes.MapRoute(
                name: "ReceptionServiceManagement",
                url: "Reception/ServiceManagement/{action}",
                defaults: new { controller = "ReceptionServiceManagement", action = "GetServiceCategories" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET", "POST") }
            );

            routes.MapRoute(
                name: "ReceptionPayment",
                url: "Reception/Payment/{action}",
                defaults: new { controller = "ReceptionPayment", action = "GetPaymentInfo" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET", "POST") }
            );

            // 🏥 POS Terminal API Routes
            routes.MapRoute(
                name: "PosTerminalApi_GetById",
                url: "api/v1/pos/terminals/{id}",
                defaults: new { controller = "PosTerminalApi", action = "Get" },
                constraints: new { id = @"\d+", httpMethod = new HttpMethodConstraint("GET") },
                namespaces: new[] { "ClinicApp.Controllers.Payment.POS" }
            );
            
            routes.MapRoute(
                name: "PosTerminalApi_Update",
                url: "api/v1/pos/terminals/{id}",
                defaults: new { controller = "PosTerminalApi", action = "Update" },
                constraints: new { id = @"\d+", httpMethod = new HttpMethodConstraint("PUT") },
                namespaces: new[] { "ClinicApp.Controllers.Payment.POS" }
            );
            
            routes.MapRoute(
                name: "PosTerminalApi_Default",
                url: "api/v1/pos/terminals/default",
                defaults: new { controller = "PosTerminalApi", action = "GetDefault" },
                namespaces: new[] { "ClinicApp.Controllers.Payment.POS" }
            );
            
            routes.MapRoute(
                name: "PosTerminalApi_SetDefault",
                url: "api/v1/pos/terminals/{id}/default",
                defaults: new { controller = "PosTerminalApi", action = "SetDefault" },
                constraints: new { id = @"\d+", httpMethod = new HttpMethodConstraint("POST") },
                namespaces: new[] { "ClinicApp.Controllers.Payment.POS" }
            );
            
            routes.MapRoute(
                name: "PosTerminalApi_ToggleActive",
                url: "api/v1/pos/terminals/{id}/active",
                defaults: new { controller = "PosTerminalApi", action = "ToggleActive" },
                constraints: new { id = @"\d+", httpMethod = new HttpMethodConstraint("POST") },
                namespaces: new[] { "ClinicApp.Controllers.Payment.POS" }
            );
            
            routes.MapRoute(
                name: "PosTerminalApi_ProcessPayment",
                url: "api/v1/pos/process-payment",
                defaults: new { controller = "PosTerminalApi", action = "ProcessPayment" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") },
                namespaces: new[] { "ClinicApp.Controllers.Payment.POS" }
            );
            
            routes.MapRoute(
                name: "PosTerminalApi",
                url: "api/v1/pos/{action}",
                defaults: new { controller = "PosTerminalApi", action = "List" },
                namespaces: new[] { "ClinicApp.Controllers.Payment.POS" }
            );

            // 🏥 Patient Controller Route - برای جلوگیری از تداخل با Api.PatientController
            // این route باید قبل از route پیش‌فرض قرار بگیرد تا اولویت داشته باشد
            
            // 🏥 Payment Controllers Route - برای Controllers در namespace Payment
            // مثال: /Payment/CashierReport -> CashierReportController.Index
            // 🏥 Account Controller Route - باید قبل از Payment route باشد
            // جلوگیری از conflict با Payment namespace
            routes.MapRoute(
                name: "Account",
                url: "Account/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Controllers" }
            ).DataTokens["UseNamespaceFallback"] = false;

            // مثال: /Payment/CashierReport/DailyReport -> CashierReportController.DailyReport
            // مثال: /Payment/Payment -> PaymentController.Index
            // ⚠️ محدود به controller های خاص Payment برای جلوگیری از conflict با AccountController
            routes.MapRoute(
                name: "Payment_Controllers",
                url: "Payment/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                constraints: new { controller = @"^(CashierReport|CashierDashboard|Payment)$" },
                namespaces: new[] { "ClinicApp.Controllers.Payment" }
            ).DataTokens["UseNamespaceFallback"] = false; // ❌ جلوگیری از fallback به namespace های دیگر
            
            // 📚 Blog Routes - برای نمایش عمومی مقالات
            routes.MapRoute(
                name: "BlogPost",
                url: "Blog/Post/{slug}",
                defaults: new { controller = "Blog", action = "Post", slug = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Controllers" }
            );

            routes.MapRoute(
                name: "Blog",
                url: "Blog/{action}/{id}",
                defaults: new { controller = "Blog", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Controllers" }
            );

            // 🏥 Default Route - با اولویت namespace ها
            // CRITICAL: Api namespace باید آخر باشد تا conflict با MVC controllers نداشته باشد
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { 
                    "ClinicApp.Controllers",                    // ✅ اولویت 1: MVC Controllers
                    "ClinicApp.Controllers.Payment",            // ✅ اولویت 2: Payment Controllers (CashierReport, CashierDashboard, Payment)
                    "ClinicApp.Controllers.ReceptionV2",       // ✅ اولویت 3
                    "ClinicApp.Controllers.Reception",         // ✅ اولویت 4
                    "ClinicApp.Controllers.Payment.POS"        // ✅ اولویت 5
                    // ❌ REMOVED: "ClinicApp.Controllers.Api" - باعث conflict با Patient/Index می‌شد
                }
            );
        }
    }
}
