using System;
using System.Threading.Tasks;
using ClinicApp.Models.Insurance;
using Serilog;

namespace ClinicApp.Services.Financial
{
    /// <summary>
    /// سرویس محاسبات مالی تعرفه بیمه - الگوی امن و تراز‌شونده
    /// </summary>
    public class InsuranceTariffCalculationService
    {
        private readonly ILogger _logger;

        public InsuranceTariffCalculationService(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// محاسبه امن سهم‌ها با تراز نهایی تضمین‌شده
        /// </summary>
        public async Task<ShareCalculationResult> ComputeSharesAsync(
            decimal tariffPrice,           // ریال
            decimal deductible,            // ریال
            decimal primaryCoveragePct,    // 0..100 (از کل)
            decimal? userPatientPct,       // nullable 0..100
            decimal? userInsurerPct,       // nullable 0..100
            decimal supplementaryPctOfRemaining, // 0..100 (از باقیمانده بعد از پایه)
            string correlationId)
        {
            try
            {
                // 1) اعتبارسنجی ورودی‌ها
                if (tariffPrice <= 0) 
                    return new ShareCalculationResult(0, 0, 0, 0, "مبلغ تعرفه باید مثبت باشد");

                // 2) حالت ورود دستی درصدها
                if (userPatientPct.HasValue && userInsurerPct.HasValue)
                {
                    return ComputeManualPercentShares(tariffPrice, userPatientPct.Value, userInsurerPct.Value, correlationId);
                }

                // 3) حالت عادی: فرانشیز + پوشش پایه + تکمیلی
                return await ComputeAutomaticSharesAsync(tariffPrice, deductible, primaryCoveragePct, 
                    supplementaryPctOfRemaining, correlationId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه سهم‌ها - CorrelationId: {CorrelationId}", correlationId);
                return new ShareCalculationResult(0, 0, 0, 0, "خطا در محاسبه سهم‌ها");
            }
        }

        /// <summary>
        /// محاسبه سهم‌ها بر اساس درصدهای دستی با تراز نهایی
        /// </summary>
        private ShareCalculationResult ComputeManualPercentShares(decimal tariffPrice, decimal patientPct, decimal insurerPct, string correlationId)
        {
            // اعتبارسنجی درصدها
            if (patientPct < 0 || insurerPct < 0 || patientPct + insurerPct > 100)
            {
                return new ShareCalculationResult(0, 0, 0, 0, "درصدها باید بین 0-100 و مجموع آن‌ها ≤ 100 باشد");
            }

            // محاسبه خام
            var insurerRaw = tariffPrice * (insurerPct / 100m);
            var patientRaw = tariffPrice * (patientPct / 100m);

            // 🔧 CRITICAL FIX: تراز نهایی یک‌مرحله‌ای
            var insurer = Math.Round(insurerRaw, 0, MidpointRounding.AwayFromZero);
            var patient = tariffPrice - insurer; // تراز به نفع بیمار

            // محاسبه درصدهای نهایی
            var finalInsurerPct = tariffPrice > 0 ? (insurer / tariffPrice) * 100m : 0m;
            var finalPatientPct = tariffPrice > 0 ? (patient / tariffPrice) * 100m : 0m;

            _logger.Debug("🏥 MEDICAL: محاسبه دستی سهم‌ها - TariffPrice: {TariffPrice}, InsurerPct: {InsurerPct}%, PatientPct: {PatientPct}%, Insurer: {Insurer}, Patient: {Patient}, CorrelationId: {CorrelationId}",
                tariffPrice, finalInsurerPct, finalPatientPct, insurer, patient, correlationId);

            return new ShareCalculationResult(insurer, patient, 0, finalInsurerPct, null);
        }

        /// <summary>
        /// محاسبه خودکار سهم‌ها بر اساس فرانشیز و پوشش
        /// </summary>
        private async Task<ShareCalculationResult> ComputeAutomaticSharesAsync(
            decimal tariffPrice, decimal deductible, decimal primaryCoveragePct, 
            decimal supplementaryPctOfRemaining, string correlationId)
        {
            // محاسبه مبلغ قابل پوشش
            var coverable = Math.Max(0, tariffPrice - deductible);
            
            // محاسبه سهم بیمه پایه
            var insurerBaseRaw = coverable * (primaryCoveragePct / 100m);
            var insurerBase = Math.Round(insurerBaseRaw, 0, MidpointRounding.AwayFromZero);

            // محاسبه سهم بیمار پایه (تراز)
            var patientBase = tariffPrice - insurerBase;

            // محاسبه پوشش تکمیلی
            var remainingAfterPrimary = Math.Max(0, tariffPrice - insurerBase);
            var suppAmountRaw = remainingAfterPrimary * (supplementaryPctOfRemaining / 100m);
            var suppAmount = Math.Round(suppAmountRaw, 0, MidpointRounding.AwayFromZero);

            // محاسبه سهم نهایی بیمه (با سقف)
            var insurerFinal = Math.Min(tariffPrice, insurerBase + suppAmount);
            var patientFinal = tariffPrice - insurerFinal; // تراز نهایی

            // محاسبه درصد تکمیلی از کل
            var suppPctOfTotal = tariffPrice > 0 ? (suppAmount / tariffPrice) * 100m : 0m;
            var totalCoveragePct = tariffPrice > 0 ? (insurerFinal / tariffPrice) * 100m : 0m;

            _logger.Debug("🏥 MEDICAL: محاسبه خودکار سهم‌ها - TariffPrice: {TariffPrice}, Deductible: {Deductible}, PrimaryPct: {PrimaryPct}%, SuppPct: {SuppPct}%, Insurer: {Insurer}, Patient: {Patient}, SuppPctOfTotal: {SuppPctOfTotal}%, TotalCoverage: {TotalCoverage}%, CorrelationId: {CorrelationId}",
                tariffPrice, deductible, primaryCoveragePct, supplementaryPctOfRemaining, insurerFinal, patientFinal, suppPctOfTotal, totalCoveragePct, correlationId);

            return new ShareCalculationResult(insurerFinal, patientFinal, suppPctOfTotal, totalCoveragePct, null);
        }
    }

    /// <summary>
    /// نتیجه محاسبه سهم‌ها
    /// </summary>
    public class ShareCalculationResult
    {
        public decimal InsurerShare { get; }
        public decimal PatientShare { get; }
        public decimal SupplementaryPercentOfTotal { get; }
        public decimal TotalCoveragePercent { get; }
        public string ErrorMessage { get; }

        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        public ShareCalculationResult(decimal insurerShare, decimal patientShare, 
            decimal supplementaryPercentOfTotal, decimal totalCoveragePercent, string errorMessage)
        {
            InsurerShare = insurerShare;
            PatientShare = patientShare;
            SupplementaryPercentOfTotal = supplementaryPercentOfTotal;
            TotalCoveragePercent = totalCoveragePercent;
            ErrorMessage = errorMessage;
        }
    }
}
