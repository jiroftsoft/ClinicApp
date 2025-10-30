using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Filters;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    [Authorize]
    [NoCache]
    public class ReceptionControllerV2 : Controller
    {
        private readonly IReceptionFacade _receptionFacade;
        private readonly ILogger _logger;

        public ReceptionControllerV2(IReceptionFacade receptionFacade, ILogger logger)
        {
            _receptionFacade = receptionFacade ?? throw new ArgumentNullException(nameof(receptionFacade));
            _logger = logger.ForContext<ReceptionControllerV2>();
        }

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
    }
}
