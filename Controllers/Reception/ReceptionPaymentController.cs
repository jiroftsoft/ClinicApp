using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Constants;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر پرداخت در ماژول پذیرش
    /// </summary>
    public class ReceptionPaymentController : BaseController
    {
        private readonly IReceptionPaymentService _paymentService;
        private readonly ILogger _logger;

        public ReceptionPaymentController(
            IReceptionPaymentService paymentService,
            ILogger logger) : base(logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// دریافت اطلاعات پرداخت
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetPaymentInfo(string patientId, decimal amount)
        {
            try
            {
                _logger.Information("دریافت اطلاعات پرداخت برای بیمار {PatientId} با مبلغ {Amount}", patientId, amount);

                var result = await _paymentService.GetPaymentInfoAsync(int.Parse(patientId));

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "اطلاعات پرداخت با موفقیت بارگذاری شد"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات پرداخت");
                return Json(new
                {
                    success = false,
                    message = "خطا در بارگذاری اطلاعات پرداخت"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// شروع پرداخت آنلاین
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> StartOnlinePayment(PaymentRequestViewModel paymentRequest)
        {
            try
            {
                _logger.Information("شروع پرداخت آنلاین برای مبلغ {Amount}", paymentRequest.Amount);

                var result = await _paymentService.StartOnlinePaymentAsync(paymentRequest);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "پرداخت آنلاین با موفقیت شروع شد"
                    });
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شروع پرداخت آنلاین");
                return Json(new
                {
                    success = false,
                    message = "خطا در شروع پرداخت آنلاین"
                });
            }
        }

        /// <summary>
        /// تایید پرداخت آنلاین
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ConfirmOnlinePayment(string transactionId, string verificationCode)
        {
            try
            {
                _logger.Information("تایید پرداخت آنلاین برای تراکنش {TransactionId}", transactionId);

                var result = await _paymentService.ConfirmOnlinePaymentAsync(transactionId, verificationCode);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "پرداخت آنلاین با موفقیت تایید شد"
                    });
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تایید پرداخت آنلاین");
                return Json(new
                {
                    success = false,
                    message = "خطا در تایید پرداخت آنلاین"
                });
            }
        }

        /// <summary>
        /// تکمیل پرداخت نقدی
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CompleteCashPayment(CashPaymentRequestViewModel cashPaymentRequest)
        {
            try
            {
                _logger.Information("تکمیل پرداخت نقدی برای مبلغ {Amount}", cashPaymentRequest.Amount);

                var result = await _paymentService.CompleteCashPaymentAsync(cashPaymentRequest);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "پرداخت نقدی با موفقیت تکمیل شد"
                    });
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تکمیل پرداخت نقدی");
                return Json(new
                {
                    success = false,
                    message = "خطا در تکمیل پرداخت نقدی"
                });
            }
        }

        /// <summary>
        /// دریافت وضعیت پرداخت
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetPaymentStatus(string transactionId)
        {
            try
            {
                _logger.Information("دریافت وضعیت پرداخت برای تراکنش {TransactionId}", transactionId);

                var result = await _paymentService.GetPaymentStatusAsync(int.Parse(transactionId));

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "وضعیت پرداخت با موفقیت بارگذاری شد"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت وضعیت پرداخت");
                return Json(new
                {
                    success = false,
                    message = "خطا در بارگذاری وضعیت پرداخت"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// لغو پرداخت
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CancelPayment(string transactionId, string reason)
        {
            try
            {
                _logger.Information("لغو پرداخت برای تراکنش {TransactionId} با دلیل {Reason}", transactionId, reason);

                var result = await _paymentService.CancelPaymentAsync(int.Parse(transactionId), reason);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "پرداخت با موفقیت لغو شد"
                    });
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو پرداخت");
                return Json(new
                {
                    success = false,
                    message = "خطا در لغو پرداخت"
                });
            }
        }

        /// <summary>
        /// دریافت رسید پرداخت
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetPaymentReceipt(string transactionId)
        {
            try
            {
                _logger.Information("دریافت رسید پرداخت برای تراکنش {TransactionId}", transactionId);

                var result = await _paymentService.GetPaymentReceiptAsync(transactionId);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data,
                        message = "رسید پرداخت با موفقیت بارگذاری شد"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت رسید پرداخت");
                return Json(new
                {
                    success = false,
                    message = "خطا در بارگذاری رسید پرداخت"
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}