using System.Web;
using System.Web.Optimization;

namespace ClinicApp
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Disable cache and minification for medical environment
            BundleTable.EnableOptimizations = false;
            BundleTable.Bundles.Clear();
                    bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            "~/Scripts/jquery-3.7.1.min.js",
            "~/Content/js/jquery-protection.js"));

        bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // ✅ BULLETPROOF: bootstrap.bundle.min.js includes Popper.js - don't load it separately
            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.min.js", // ✅ Includes Popper.js for dropdowns/tooltips
                      "~/Scripts/aos.js",
                      "~/Scripts/jquery-ui.min.js",
                      "~/Scripts/toastr.min.js",
                      "~/Scripts/persian-date.min.js",
                      "~/Scripts/persian-datepicker.min.js",
                      "~/Content/plugins/SweetAlert2/sweetalert2@11.js"
                      ));
            // --- CSS Bundle ---
            bundles.Add(new StyleBundle("~/Content/plugins/css").Include(
                "~/Content/js/plugins/DataTables/css/dataTables.bootstrap4.min.css",
                "~/Content/js/plugins/DataTables/css/responsive.bootstrap4.min.css",
                "~/Content/js/plugins/select2/css/select2.min.css",
                "~/Content/js/plugins/SweetAlert2/sweetalert2.min.css"
            ));

            // --- JS Bundle ---
            bundles.Add(new ScriptBundle("~/bundles/plugins").Include(
                "~/Content/js/plugins/DataTables/js/jquery.dataTables.min.js",
                "~/Content/js/plugins/DataTables/js/dataTables.bootstrap4.min.js",
                "~/Content/js/plugins/DataTables/js/dataTables.responsive.min.js",
                "~/Content/js/plugins/DataTables/js/responsive.bootstrap4.min.js",
                "~/Content/js/plugins/select2/js/select2.full.min.js",
                "~/Content/js/plugins/select2/js/fa.min.js",
                "~/Content/js/plugins/SweetAlert2/sweetalert2@11.js"
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

            // Homepage CSS Bundle - All section CSS files combined
            bundles.Add(new StyleBundle("~/Content/css/homepage-sections").Include(
                "~/Content/css/medical-services-section.css",
                "~/Content/css/doctors-section.css",
                "~/Content/css/testimonials-section.css",
                "~/Content/css/blog-section.css",
                "~/Content/css/health-tips-section.css",
                "~/Content/css/medical-equipment-section.css",
                "~/Content/css/insurance-info-section.css",
                "~/Content/css/contact-section.css",
                "~/Content/css/medical-sidebar.css"
            ));

            // FIX: مطابق با VIEW_OPTIMIZATION_CONTRACT - اضافه کردن Bundle های پزشکی
            // Medical Environment CSS Bundle
            bundles.Add(new StyleBundle("~/Content/css/medical-environment").Include(
                "~/Content/css/medical-environment-styles.css"
            ));

            // Insurance Supplementary Tariff CSS Bundle
            bundles.Add(new StyleBundle("~/Content/css/insurance/supplementary-tariff").Include(
                "~/Content/css/insurance/supplementary-tariff-views.css"
            ));

            // Reception V2 Bundles - Zero Cache, Medical-Grade
            bundles.Add(new StyleBundle("~/content/reception.v2").Include(
                "~/Content/bootstrap.min.css",          // ✅ Bootstrap اصلی (برای table-dark و سایر کلاس‌ها)
                "~/Content/bootstrap.rtl.min.css",      // ✅ Bootstrap RTL (برای راست‌چین)
                "~/Content/select2.min.css",
                "~/Content/persian-datepicker.min.css",
                "~/Content/toastr.min.css",
                "~/Content/css/breadcrumb-medical.css",  // ✅ Breadcrumb Navigation برای سیستم‌های درمانی
                "~/Content/css/reception-form-header.css",  // ✅ Enhanced Header برای فرم پذیرش
                "~/Content/css/reception-error-toast.css",  // ✅ استایل خطاهای کاربرپسند برای منشی
                "~/Content/reception.v2.css"
            ));

            var receptionV2 = new ScriptBundle("~/bundles/reception.v2");
            // Disable minification for this bundle to avoid WebGrease/JSParser issues with modern JS syntax
            receptionV2.Transforms.Clear();
            receptionV2.Include(
                "~/Scripts/jquery-3.7.1.min.js",
                "~/Scripts/bootstrap.bundle.min.js",
                "~/Scripts/select2.full.min.js",
                "~/Scripts/persian-date.min.js",
                "~/Scripts/persian-datepicker.min.js",
                "~/Scripts/toastr.min.js",
                "~/Scripts/lodash.debounce.min.js",
                "~/Scripts/jquery.inputmask.bundle.min.js",
                "~/Scripts/jquery.signalR-2.4.2.min.js", // ✅ SignalR Client برای POS Payment
                "~/Content/js/reception-error-handler.js", // ✅ NEW: مدیریت حرفه‌ای خطاها برای منشی
                "~/Scripts/reception.v2/reception-validator.js", // ✅ NEW: اعتبارسنجی قدرتمند (کد ملی، موبایل، فیلدهای الزامی)
                "~/Scripts/reception.v2/service-eligibility-validator.js", // ✅ NEW: اعتبارسنجی صلاحیت خدمت (سن/جنسیت)
                "~/Scripts/reception.v2/reception-api.js",
                "~/Scripts/reception.v2/reception-utils.js",
                "~/Scripts/reception.v2/pricing-ui.js",
                "~/Scripts/reception.v2/form-change-detector.js",
                "~/Scripts/reception.v2/auto-draft-manager.js",
                "~/Scripts/reception.v2/summary-header.js",
                "~/Scripts/reception.v2/patient-lookup.js",
                "~/Scripts/reception.v2/insurance-status-checker.js", // ✅ کامپوننت قابل استفاده مجدد
                "~/Scripts/reception.v2/insurance-panel.js",
                "~/Scripts/reception.v2/clinic-dept-doctor.js",
                "~/Scripts/reception.v2/service-lookup.js",
                "~/Scripts/reception.v2/coverage-modal.js",
                "~/Scripts/reception.v2/totals-panel.js",
                "~/Scripts/pos-payment/pos-payment-lock-manager.js", // ✅ NEW: Lock Manager برای جلوگیری از Stuck Payments
                "~/Scripts/pos-payment/pos-payment-client.js", // ✅ ماژول جدید POS Payment Client
                "~/Scripts/pos-payment/pos-payment-ui.js", // ✅ ماژول جدید POS Payment UI
                "~/Scripts/reception.v2/print-manager.js", // ✅ NEW: Print Manager برای مدیریت حرفه‌ای چاپ (Single Window Reuse, Queue, Debounce)
                "~/Scripts/reception.v2/payment-panel.js",
                "~/Scripts/reception.v2/reception-main.js"
            );
            bundles.Add(receptionV2);
        }
    }
}
