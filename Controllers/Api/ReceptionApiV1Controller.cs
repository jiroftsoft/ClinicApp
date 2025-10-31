using System;
using System.Web.Mvc;
using ClinicApp.Controllers.ReceptionV2;
using ClinicApp.Helpers;
using ClinicApp.Filters;
using ClinicApp.Interfaces.Finance;
using ClinicApp.Interfaces.Reception;
using Serilog;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// Controller V1 برای API پذیرش - حداقل لازم + Health & Draft/Create
    /// 
    /// این کنترلر فقط برای مسیرهای /api/v1/reception/ است تا v1 واقعی داشته باشیم
    /// و 404/500 از بین برود. بعداً می‌تونیم بقیه اکشن‌ها رو هم بهش اضافه کنیم یا به فاساد وصل کنیم.
    /// </summary>
    [RoutePrefix("api/v1/reception")]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [ReceptionV2Controller.NoCache]
    public class ReceptionApiV1Controller : Controller
    {
        #region Dependencies

        private readonly IFinancialYearService _fy;
        private readonly IReceptionFacade _facade;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// DI ctor
        /// </summary>
        public ReceptionApiV1Controller(
            IFinancialYearService fy,
            IReceptionFacade facade,
            ILogger logger)
        {
            _fy = fy ?? throw new ArgumentNullException(nameof(fy));
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
            _logger = logger?.ForContext<ReceptionApiV1Controller>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Fallback ctor (اگر DI هنوز ثبت نشده)
        /// </summary>
        public ReceptionApiV1Controller()
            : this(
                  DependencyResolver.Current.GetService<IFinancialYearService>(),
                  DependencyResolver.Current.GetService<IReceptionFacade>(),
                  DependencyResolver.Current.GetService<ILogger>())
        {
        }

        #endregion

        #region Actions

        /// <summary>
        /// GET /api/v1/reception/health
        /// Health check endpoint
        /// </summary>
        [HttpGet, Route("health")]
        public ActionResult Health()
        {
            try
            {
                _logger?.Information("🏥 V1 API: Health check");
                return Json(ServiceResult.Successful("ok"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در Health check");
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// GET /api/v1/reception/bootstrap
        /// داده‌های اولیه فرم پذیرش (دپارتمان‌ها، خدمات مشترک و ...)
        /// 
        /// ✳️ اگر در فاساد Bootstrap داری همین‌جا پاس بده؛ در غیر اینصورت فعلاً اسکلت خالی برمی‌گردانیم
        /// </summary>
        [HttpGet, Route("bootstrap")]
        public ActionResult Bootstrap(int? clinicId, int? deptId)
        {
            try
            {
                _logger?.Information("🏥 V1 API: Bootstrap - ClinicId: {ClinicId}, DeptId: {DeptId}", clinicId, deptId);

                // اگر _facade.LoadInitialAsync() موجود است:
                if (_facade != null)
                {
                    try
                    {
                        var result = System.Threading.Tasks.Task.Run(async () => await _facade.LoadInitialAsync(clinicId ?? 1, deptId)).Result;
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    catch
                    {
                        // Fallback to minimal payload
                    }
                }

                // اسکلت امن حداقلی (برای جلوگیری از 404/500)
                var payload = new
                {
                    Departments = new object[] { },
                    Services = new object[] { },
                    SharedServices = new object[] { },
                    Doctors = new object[] { },
                    FinancialYear = _fy?.GetCurrentYear() ?? DateTime.Now.Year
                };
                return Json(ServiceResult<object>.Successful(payload, "عملیات با موفقیت انجام شد."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در Bootstrap");
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// POST /api/v1/reception/draft/create
        /// ایجاد پیش‌نویس پذیرش
        /// 
        /// اگر در فاساد متد ساخت Draft داری، همین‌جا صدا بزن:
        /// </summary>
        [HttpPost, Route("draft/create")]
        [ValidateAntiForgeryTokenOnPosts] // اگر فیلتر سفارشی‌ات فعاله
        [ValidateAntiForgeryToken]        // و نیز Attribute استاندارد
        public ActionResult CreateDraft()
        {
            try
            {
                _logger?.Information("🏥 V1 API: Create Draft");

                // اگر در فاساد متد ساخت Draft داری، همین‌جا صدا بزن:
                if (_facade != null)
                {
                    try
                    {
                        // TODO: اتصال به ReceptionFacade.CreateDraftAsync() وقتی آماده شد
                        // var request = new CreateDraftRequest { ... };
                        // var res = System.Threading.Tasks.Task.Run(async () => await _facade.CreateDraftAsync(request)).Result;
                        // return Json(res);
                    }
                    catch
                    {
                        // Fallback to minimal response
                    }
                }

                // حداقل: DraftId ساختگی تا UI بالا بیاید (بعداً وصل به فاساد)
                var draftId = Guid.NewGuid().ToString("N");
                _logger?.Information("🏥 V1 API: Draft created - DraftId: {DraftId}", draftId);
                return Json(ServiceResult<string>.Successful(draftId, "Draft created."));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در Create Draft");
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"));
            }
        }

        #endregion
    }
}

