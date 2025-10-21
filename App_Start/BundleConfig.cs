using System.Web;
using System.Web.Optimization;

namespace ClinicApp
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Disable minification for debugging
            BundleTable.EnableOptimizations = false;
                    bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            "~/Scripts/jquery-3.7.1.min.js",
            "~/Content/js/jquery-protection.js"));

        bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/aos.js",
                      "~/Scripts/jquery-ui.min.js",
                      "~/Scripts/toastr.min.js",
                      "~/Scripts/persian-date.min.js",
                      "~/Scripts/persian-datepicker.min.js",
                      "~/Content/plugins/SweetAlert2/sweetalert2@11.js"
                      ));
            // --- CSS Bundle ---
            bundles.Add(new StyleBundle("~/Content/plugins/css").Include(
                "~/Content/plugins/datatables/css/dataTables.bootstrap4.min.css",
                "~/Content/plugins/datatables/css/responsive.bootstrap4.min.css",
                "~/Content/plugins/select2/css/select2.min.css",
                "~/Content/plugins/SweetAlert2/sweetalert2.min.css"
            ));

            // --- JS Bundle ---
            bundles.Add(new ScriptBundle("~/bundles/plugins").Include(
                "~/Content/plugins/datatables/js/jquery.dataTables.min.js",
                "~/Content/plugins/datatables/js/dataTables.bootstrap4.min.js",
                "~/Content/plugins/datatables/js/dataTables.responsive.min.js",
                "~/Content/plugins/datatables/js/responsive.bootstrap4.min.js",
                "~/Content/plugins/select2/js/select2.full.min.js",
                "~/Content/plugins/select2/js/fa.min.js",
                "~/Content/plugins/SweetAlert2/sweetalert2@11.js"
            ));

            // Reception Module CSS Bundle
            bundles.Add(new StyleBundle("~/Content/reception/css").Include(
                "~/Content/css/reception/reception-accordion.css",
                "~/Content/css/reception/realtime-insurance-binding.css"
            ));

            // Reception Module JS Bundle
            bundles.Add(new ScriptBundle("~/bundles/reception/js").Include(
                "~/Scripts/reception/reception-modules.js"
            ));

            // Admin Layout CSS Bundle
            bundles.Add(new StyleBundle("~/Content/admin").Include(
                "~/Content/css/admin-layout.css",
                "~/Content/css/notifications.css"
            ));

            // Main CSS Bundle
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/bootstrap-rtl.css",
                "~/Content/Site.css",
                "~/Content/aos.css",
                "~/Content/toastr.min.css",
                "~/Content/persian-datepicker.min.css"
            ));
            // Add a new bundle for Toastr styles
            bundles.Add(new StyleBundle("~/Content/toastr").Include(
                "~/Content/plugins/toastr/toastr.min.css"));

            // Add a new bundle for Toastr script
            bundles.Add(new ScriptBundle("~/bundles/toastr").Include(
                "~/Content/plugins/toastr/toastr.min.js"));

            // Patient Accordion CSS Bundle
            bundles.Add(new StyleBundle("~/Content/patient-accordion").Include(
                "~/Content/css/reception/patient-accordion.css"));

            // Reception Toastr Service Bundle
            bundles.Add(new ScriptBundle("~/bundles/reception-toastr").Include(
                "~/Scripts/reception/reception-toastr-service.js"));

            // Reception Core Modules Bundle
            bundles.Add(new ScriptBundle("~/bundles/reception-core").Include(
                "~/Scripts/reception/core/event-bus.js",
                "~/Scripts/reception/core/error-handler.js",
                "~/Scripts/reception/core/reception-coordinator.js"));

            // Reception Specialized Modules Bundle (Load First)
            bundles.Add(new ScriptBundle("~/bundles/reception-specialized").Include(
                "~/Scripts/reception/modules/validation-engine.js",
                "~/Scripts/reception/modules/form-change-detector.js",
                "~/Scripts/reception/modules/save-processor.js",
                "~/Scripts/reception/modules/edit-mode-manager.js",
                "~/Scripts/reception/modules/insurance-orchestrator.js"));

            // Advanced Insurance System Bundle (New Modern Architecture) - No Minification
            var advancedInsuranceBundle = new ScriptBundle("~/bundles/advanced-insurance").Include(
                "~/Scripts/reception/modules/advanced-change-detector.js",
                "~/Scripts/reception/modules/advanced-state-manager.js",
                "~/Scripts/reception/modules/advanced-insurance-coordinator.js",
                "~/Scripts/reception/modules/advanced-insurance-system.js");
            advancedInsuranceBundle.ConcatenationToken = "/* Advanced Insurance System */";
            bundles.Add(advancedInsuranceBundle);

            // Advanced Insurance System Test Bundle - No Minification
            var advancedInsuranceTestBundle = new ScriptBundle("~/bundles/advanced-insurance-test").Include(
                "~/Scripts/reception/test-advanced-insurance-system.js");
            advancedInsuranceTestBundle.ConcatenationToken = "/* Advanced Insurance System Test */";
            bundles.Add(advancedInsuranceTestBundle);

            // Reception Feature Modules Bundle
            bundles.Add(new ScriptBundle("~/bundles/reception-modules").Include(
                "~/Scripts/reception/modules/patient-search.js",
                "~/Scripts/reception/modules/patient-insurance.js",
                "~/Scripts/reception/modules/realtime-insurance-binding.js",
                "~/Scripts/reception/modules/department-selection.js",
                "~/Scripts/reception/modules/service-calculation.js",
                "~/Scripts/reception/modules/payment-processing.js"));

            // Reception Main Module Bundle
            bundles.Add(new ScriptBundle("~/bundles/reception-main").Include(
                "~/Scripts/reception/reception-main.js"));

            // FIX: مطابق با VIEW_OPTIMIZATION_CONTRACT - اضافه کردن Bundle های پزشکی
            // Medical Environment CSS Bundle
            bundles.Add(new StyleBundle("~/Content/css/medical-environment").Include(
                "~/Content/css/medical-environment-styles.css"
            ));

            // Insurance Supplementary Tariff CSS Bundle
            bundles.Add(new StyleBundle("~/Content/css/insurance/supplementary-tariff").Include(
                "~/Content/css/insurance/supplementary-tariff-views.css"
            ));
        }
    }
}
