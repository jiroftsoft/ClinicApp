using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// DTO برای اطلاعات هویتی کامل بیمار
    /// </summary>
    public class PatientIdentityDto
    {
        public int PatientId { get; set; }
        public string NationalCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; } // "Male"/"Female" یا کد عددی
        public string BirthDateShamsi { get; set; } // "YYYY/MM/DD"
    }

    /// <summary>
    /// DTO برای پاسخ جستجوی بیمار (شامل هویت + بیمه)
    /// </summary>
    public class PatientLookupResponseDto
    {
        public PatientIdentityDto Identity { get; set; }
        public InsuranceSelectionDto Insurance { get; set; }
    }

    /// <summary>
    /// DTO برای درخواست به‌روزرسانی اطلاعات پایه بیمار
    /// </summary>
    public class PatientUpdateBasicRequest
    {
        public int PatientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string BirthDateShamsi { get; set; }
    }

    /// <summary>
    /// DTO برای انتخاب بیمه‌ها
    /// </summary>
    public class InsuranceSelectionDto
    {
        public int? BaseInsuranceId { get; set; }
        public int? BasePlanId { get; set; }
        public int? SupplementaryInsuranceId { get; set; }
        public int? SupplementaryPlanId { get; set; }

        // برای UI: پیشنهاد پلن پیشفرض هر بیمه
        public int? SuggestedBasePlanId { get; set; }
        public int? SuggestedSupplementaryPlanId { get; set; }
    }

    /// <summary>
    /// DTO برای ثبت سریع بیمار (Fast Create)
    /// </summary>
    public class PatientQuickCreateDto
    {
        [Required, StringLength(10, MinimumLength = 10)]
        public string NationalCode { get; set; }
        
        [Required, StringLength(50)]
        public string FirstName { get; set; }
        
        [Required, StringLength(50)]
        public string LastName { get; set; }
        
        [Required, StringLength(11)]
        public string Mobile { get; set; }
        
        public string Gender { get; set; } // "Male"/"Female" یا Enum
        
        public string BirthDateShamsi { get; set; } // "yyyy/MM/dd"
        
        public string Address { get; set; }
        
        public int? BaseInsurancePlanId { get; set; }
        
        public int? SupplementaryInsurancePlanId { get; set; }
    }

    /// <summary>
    /// DTO برای خلاصه اطلاعات بیمار (با بیمه‌ها)
    /// </summary>
    public class PatientSummaryDto
    {
        public int PatientId { get; set; }
        public string NationalCode { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Gender { get; set; }
        public string BirthDateShamsi { get; set; }
        public string Address { get; set; }
        public int? BaseInsurancePlanId { get; set; }
        public string BaseInsurancePlanName { get; set; }
        public int? SupplementaryInsurancePlanId { get; set; }
        public string SupplementaryInsurancePlanName { get; set; }
    }

    #region Coverage DTOs

    /// <summary>
    /// DTO برای جزئیات پوشش یک طرح بیمه (پایه یا تکمیلی)
    /// </summary>
    public class InsuranceCoverageSliceDto
    {
        public string PlanName { get; set; }
        public decimal? FranchisePercent { get; set; }   // فرانشیز
        public decimal? CoveragePercent { get; set; }    // درصد پوشش پایه/تکمیلی
        
        // ✅ Friendly string for Franchise (مبلغ یا درصد)
        public string FranchisePercentStr { get; set; }
        
        public decimal? CeilingPerService { get; set; }
        public decimal? CeilingPerVisit { get; set; }
        public decimal? CeilingMonthly { get; set; }
        public decimal? RemainingCeiling { get; set; }
        
        // Friendly strings
        public string CeilingPerServiceStr { get; set; }
        public string CeilingPerVisitStr { get; set; }
        public string CeilingMonthlyStr { get; set; }
        public string RemainingCeilingStr { get; set; }
    }

    /// <summary>
    /// DTO برای پوشش مؤثر ترکیبی (بعد از اعمال پایه + تکمیلی)
    /// </summary>
    public class InsuranceCoverageEffectiveDto
    {
        public decimal EffectiveCoveragePercent { get; set; } // بعد از ترکیب پایه/تکمیلی
        public decimal PatientSharePercent { get; set; }
        public string Notes { get; set; } // توضیح قواعد ترکیب
    }

    /// <summary>
    /// DTO برای پاسخ پوشش بیمه (پایه + تکمیلی + مؤثر)
    /// </summary>
    public class InsuranceCoverageDto
    {
        public InsuranceCoverageSliceDto Base { get; set; }
        public InsuranceCoverageSliceDto Supplementary { get; set; }
        public InsuranceCoverageEffectiveDto Effective { get; set; }
    }

    /// <summary>
    /// DTO برای درخواست پیش‌نمایش قیمت
    /// </summary>
    public class PricePreviewRequestDto
    {
        public int? PatientId { get; set; }
        public int? DepartmentId { get; set; }
        public int? DoctorId { get; set; }
        public int? BasePlanId { get; set; }
        public int? SupplementaryPlanId { get; set; }
        public string ServiceCodeOrName { get; set; }
    }

    /// <summary>
    /// DTO برای نتیجه پیش‌نمایش قیمت
    /// </summary>
    public class PricePreviewResultDto
    {
        public decimal Price { get; set; }
        public decimal PatientShare { get; set; }
        public decimal EffectiveCoveragePercent { get; set; }
        
        // Friendly strings
        public string PriceStr { get; set; }
        public string PatientShareStr { get; set; }
    }

    #endregion
}

