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

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ClinicApp.Controllers" }
            );
        }
    }
}
