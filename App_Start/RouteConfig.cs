using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ClinicApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            // Enable Attribute Routing
            routes.MapMvcAttributeRoutes();

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
               action = @"^(Load|Save|ValidatePatientInsurance|QuickValidateInsurance|GetPatientInsuranceStatus|CalculateInsuranceShare|InquiryPatientIdentity|GetInsuranceProviders|GetPrimaryInsuranceProviders|GetSupplementaryInsuranceProviders|GetInsurancePlans|GetPrimaryInsurancePlans|GetSupplementaryInsurancePlans|GetSupplementaryInsurances|CalculateInsurance|ChangePatientInsurance)$"
           }
       );

            // 🏥 Reception List Routes - مسیرهای لیست پذیرش‌ها
            routes.MapRoute(
                name: "ReceptionList",
                url: "Reception/ReceptionList/{action}",
                defaults: new { 
                    controller = "ReceptionList", 
                    action = "Index", 
                    area = ""
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(Index|GetReceptionList)$"
                }
            );

            // 🏥 Reception History Routes - مسیرهای سوابق پذیرش
            routes.MapRoute(
                name: "ReceptionHistory",
                url: "Reception/ReceptionHistory/{action}",
                defaults: new { 
                    controller = "ReceptionHistory", 
                    action = "Index", 
                    area = ""
                },
                constraints: new { 
                    httpMethod = new HttpMethodConstraint("GET", "POST"),
                    action = @"^(Index|SearchHistory)$"
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

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Controllers" }
            );
        }
    }
}
