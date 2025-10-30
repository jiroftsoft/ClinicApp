using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;
using ClinicApp.Helpers;
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
    [NoCache]
    public class ReceptionV2Controller : Controller
    {
        #region Dependencies

        private readonly IReceptionFacade _receptionFacade;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public ReceptionV2Controller(IReceptionFacade receptionFacade, ILogger logger)
        {
            _receptionFacade = receptionFacade ?? throw new ArgumentNullException(nameof(receptionFacade));
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
                var model = await _receptionFacade.LoadInitialAsync(1, null);
                
                // تبدیل به ReceptionFormVM
                var vm = new ReceptionFormVM
                {
                    Bootstrap = new BootstrapVM
                    {
                        FinancialYear = DateTime.Now.Year // TODO: از FinancialYearService
                    }
                };

                return View("~/Views/ReceptionV2/Index.cshtml", vm);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری فرم پذیرش V2");
                return View("Error");
            }
        }

        /// <summary>
        /// چاپ رسید پذیرش
        /// </summary>
        [HttpGet]
        [Route("reception/print/{id:int}")]
        public ActionResult Print(int id)
        {
            try
            {
                _logger.Information("🏥 V2: چاپ رسید پذیرش - ID: {Id}", id);
                
                // TODO: از Reception رسمی / یا Draft Finalized داده‌ها را بخوان
                return View("~/Views/ReceptionV2/Print.cshtml", model: id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در چاپ رسید");
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