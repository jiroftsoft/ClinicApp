using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Finance;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.ReceptionV2
{
    /// <summary>
    /// Controller V2 برای فرم پذیرش - Zero Cache, Production-Grade
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. Zero Cache برای محیط درمانی
    /// 2. API-محور و اتمیک
    /// 3. UX بهینه برای مانیتورهای 24-27 اینچ
    /// 4. SRP و Clean Architecture
    /// </summary>
    [RoutePrefix("ReceptionV2")]
    [NoCache]
    public class ReceptionV2Controller : Controller
    {
        #region Dependencies

        private readonly IReceptionFacade _receptionFacade;
        private readonly IFinancialYearService _financialYearService;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public ReceptionV2Controller(
            IReceptionFacade receptionFacade,
            IFinancialYearService financialYearService,
            ILogger logger)
        {
            _receptionFacade = receptionFacade ?? throw new ArgumentNullException(nameof(receptionFacade));
            _financialYearService = financialYearService ?? throw new ArgumentNullException(nameof(financialYearService));
            _logger = logger.ForContext<ReceptionV2Controller>();
        }

        #endregion

        #region Actions

        /// <summary>
        /// صفحه اصلی فرم پذیرش V2
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.Information("🏥 V2: بارگذاری فرم پذیرش V2");

                // بارگذاری داده‌های اولیه از Facade
                var loadResult = await _receptionFacade.LoadInitialAsync(1, null);
                if (!loadResult.Success)
                {
                    _logger.Warning("⚠️ V2: LoadInitialAsync ناموفق - Message: {Message}", loadResult.Message);
                    ViewBag.ErrorMessage = loadResult.Message ?? "در حال حاضر امکان بارگذاری فرم پذیرش وجود ندارد. لطفاً کمی بعد تلاش کنید.";
                    return View("Error");
                }

                // دریافت سال مالی جاری از سرویس
                var financialYear = _financialYearService.GetCurrentYear();

                // تبدیل به ReceptionFormVM (داده اولیه از طریق API/bootstrap در فرانت لود می‌شود)
                var vm = new ReceptionFormVM
                {
                    Bootstrap = new BootstrapVM
                    {
                        FinancialYear = financialYear
                    }
                };

                return View("~/Views/ReceptionV2/Index.cshtml", vm);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری فرم پذیرش V2");
                ViewBag.ErrorMessage = "خطا در بارگذاری فرم پذیرش. لطفاً مجدداً تلاش کنید.";
                return View("Error");
            }
        }

        /// <summary>
        /// چاپ رسید پذیرش
        /// ✅ Production-Grade: استفاده از Facade برای بارگذاری داده‌ها در Server-Side
        /// </summary>
        [HttpGet]
        [Route("Print/{id:int}", Name = "ReceptionV2_Print")]
        [Route("reception/print/{id:int}", Name = "ReceptionV2_Print_Legacy")] // Legacy route برای سازگاری
        public async Task<ActionResult> Print(int id)
        {
            try
            {
                _logger.Information("🏥 V2: چاپ رسید پذیرش - ReceptionId: {Id}", id);
                
                // ✅ استفاده از Facade برای دریافت داده‌های کامل پذیرش
                var receptionResult = await _receptionFacade.GetReceptionDetailsFullAsync(id);
                if (!receptionResult.Success || receptionResult.Data == null)
                {
                    _logger.Warning("⚠️ V2: پذیرش یافت نشد - ReceptionId: {Id}", id);
                    ViewBag.ErrorMessage = receptionResult.Message ?? "پذیرش یافت نشد";
                    return View("Error");
                }
                
                _logger.Information("✅ V2: داده‌های پذیرش با موفقیت بارگذاری شد - ReceptionId: {Id}", id);
                
                // ارسال داده‌ها به View به صورت Model
                return View("~/Views/ReceptionV2/Print.cshtml", receptionResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در چاپ رسید - ReceptionId: {Id}", id);
                ViewBag.ErrorMessage = "خطا در بارگذاری اطلاعات پذیرش";
                return View("Error");
            }
        }

        /// <summary>
        /// 🖨️ چاپ قبض پرداخت برای فیش پرینتر (58mm/80mm)
        /// فرمت مناسب برای دستگاه‌های فیش پرینتر:
        /// - Bixolon SRP-350III (80mm) - وقتی printer=thermal
        /// - SRP-330II (58mm) - وقتی printer=normal
        /// </summary>
        [HttpGet]
        [Route("PrintReceipt/{id:int}", Name = "ReceptionV2_PrintReceipt")]
        public async Task<ActionResult> PrintReceipt(int id, string type = "payment", string printer = "thermal")
        {
            try
            {
                _logger.Information("🖨️ V2: چاپ قبض {Type} برای فیش پرینتر - ReceptionId: {Id}, Printer: {Printer}", 
                    type, id, printer);
                
                // دریافت اطلاعات پذیرش از Facade
                var receptionResult = await _receptionFacade.GetReceptionDetailsFullAsync(id);
                if (!receptionResult.Success || receptionResult.Data == null)
                {
                    _logger.Warning("⚠️ V2: پذیرش یافت نشد - ReceptionId: {Id}", id);
                    return View("Error");
                }
                
                ViewBag.ReceptionId = id;
                ViewBag.ReceiptType = type; // payment یا insurance
                ViewBag.PrinterType = printer; // thermal یا normal
                ViewBag.ReceptionData = receptionResult.Data;
                
                return View("~/Views/ReceptionV2/PrintReceipt.cshtml", receptionResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در چاپ قبض - ReceptionId: {Id}, Type: {Type}", id, type);
                return View("Error");
            }
        }

        /// <summary>
        /// 🏥 MEDICAL: چاپ قبض بیمه تکمیلی
        /// ✅ Production-Grade: استفاده از Facade برای بارگذاری داده‌ها در Server-Side
        /// </summary>
        [HttpGet]
        [Route("PrintInsurance/{id:int}", Name = "ReceptionV2_PrintInsurance")]
        [Route("reception/print-insurance/{id:int}", Name = "ReceptionV2_PrintInsurance_Legacy")] // Legacy route برای سازگاری
        public async Task<ActionResult> PrintInsurance(int id)
        {
            try
            {
                _logger.Information("🏥 V2: چاپ قبض بیمه تکمیلی - ReceptionId: {Id}", id);
                
                // ✅ استفاده از Facade برای دریافت داده‌های کامل پذیرش
                var receptionResult = await _receptionFacade.GetReceptionDetailsFullAsync(id);
                if (!receptionResult.Success || receptionResult.Data == null)
                {
                    _logger.Warning("⚠️ V2: پذیرش یافت نشد - ReceptionId: {Id}", id);
                    ViewBag.ErrorMessage = receptionResult.Message ?? "پذیرش یافت نشد";
                    return View("Error");
                }
                
                _logger.Information("✅ V2: داده‌های پذیرش با موفقیت بارگذاری شد - ReceptionId: {Id}", id);
                
                // ارسال داده‌ها به View به صورت Model
                return View("~/Views/ReceptionV2/PrintInsurance.cshtml", receptionResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در چاپ قبض بیمه تکمیلی - ReceptionId: {Id}", id);
                ViewBag.ErrorMessage = "خطا در بارگذاری اطلاعات پذیرش";
                return View("Error");
            }
        }

        /// <summary>
        /// صفحه ویرایش پذیرش
        /// </summary>
        [HttpGet]
        [Route("reception/edit/{id:int}", Name = "ReceptionV2_Edit")]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                _logger.Information("🏥 V2: بارگذاری صفحه ویرایش پذیرش - ReceptionId: {Id}", id);

                // بارگذاری داده‌های اولیه از Facade
                var loadResult = await _receptionFacade.LoadInitialAsync(1, null);
                if (!loadResult.Success)
                {
                    _logger.Warning("⚠️ V2: LoadInitialAsync ناموفق در Edit - Message: {Message}", loadResult.Message);
                    ViewBag.ErrorMessage = loadResult.Message ?? "در حال حاضر امکان بارگذاری فرم ویرایش وجود ندارد. لطفاً کمی بعد تلاش کنید.";
                    return View("Error");
                }

                // دریافت سال مالی جاری از سرویس
                var financialYear = _financialYearService.GetCurrentYear();

                // تبدیل به ReceptionFormVM
                var vm = new ReceptionFormVM
                {
                    Bootstrap = new BootstrapVM
                    {
                        FinancialYear = financialYear
                    }
                };

                // ذخیره ReceptionId در ViewBag برای استفاده در JavaScript
                ViewBag.ReceptionId = id;

                return View("~/Views/ReceptionV2/Edit.cshtml", vm);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری صفحه ویرایش پذیرش - ReceptionId: {Id}", id);
                ViewBag.ErrorMessage = "خطا در بارگذاری صفحه ویرایش. لطفاً مجدداً تلاش کنید.";
                return View("Error");
            }
        }

        #endregion

        #region Custom Filters

        /// <summary>
        /// فیلتر سفارشی برای حذف کامل Cache
        /// </summary>
        public class NoCacheAttribute : ActionFilterAttribute
        {
            public override void OnResultExecuting(ResultExecutingContext context)
            {
                var response = context.HttpContext.Response;
                
                // حذف کامل Cache
                response.Cache.SetCacheability(HttpCacheability.NoCache);
                response.Cache.SetNoStore();
                response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                
                // Headers اضافی
                response.Headers["Pragma"] = "no-cache";
                response.Headers["Expires"] = "0";
                
                base.OnResultExecuting(context);
            }
        }

        #endregion
    }
}