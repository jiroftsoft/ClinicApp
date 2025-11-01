using System;
using System.Collections.Generic;
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
    /// DTO برای ثبت سریع بیمار (Fast Create) و جستجو (Lookup)
    /// برای Lookup: فقط NationalCode الزامی است
    /// برای Quick Create: FirstName, LastName, Mobile نیز الزامی هستند
    /// </summary>
    public class PatientQuickCreateDto
    {
        [Required(ErrorMessage = "کد ملی الزامی است."), StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد.")]
        public string NationalCode { get; set; }
        
        // ✅ Required نیست - فقط برای Quick Create الزامی است (در Controller بررسی می‌شود)
        [StringLength(50, ErrorMessage = "نام نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string FirstName { get; set; }
        
        // ✅ Required نیست - فقط برای Quick Create الزامی است (در Controller بررسی می‌شود)
        [StringLength(50, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string LastName { get; set; }
        
        [StringLength(50, ErrorMessage = "نام پدر نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string FatherName { get; set; }
        
        // ✅ Required نیست - فقط برای Quick Create الزامی است (در Controller بررسی می‌شود)
        [StringLength(11, MinimumLength = 11, ErrorMessage = "شماره موبایل باید 11 رقم باشد.")]
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

    /// <summary>
    /// DTO برای افزودن آیتم به پیش‌نویس
    /// </summary>
    public class AddItemRequestDto
    {
        public int ReceptionId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public int? Year { get; set; } // اختیاری - اگر null باشد از Reception استفاده می‌شود
    }

    /// <summary>
    /// DTO برای حذف آیتم از پیش‌نویس
    /// </summary>
    public class RemoveItemRequestDto
    {
        public int ReceptionId { get; set; }
        public int ServiceId { get; set; }
    }

    /// <summary>
    /// ✅ DTO برای درخواست تغییر خدمت/تعداد آیتم
    /// </summary>
    public sealed class UpdateItemServiceRequestDto
    {
        public int ReceptionId { get; set; }
        public int ReceptionItemId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public int? DepartmentId { get; set; }
        public int? DoctorId { get; set; }
        public int FinancialYearId { get; set; }
        public int? BasePlanId { get; set; }
        public int? SupplementaryPlanId { get; set; }
    }

    #endregion

    #region Doctor DTOs

    /// <summary>
    /// ✅ DTO برای گزینه‌های پزشک در Dropdown (ساده و تمیز)
    /// </summary>
    public class DoctorOptionDto
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }         // اختیاری: متخصص قلب، عمومی، ...
        public string DepartmentName { get; set; } // برای UI خلاصه
        public bool IsActive { get; set; }
    }

    #endregion

    #region Pricing DTOs

    /// <summary>
    /// ✅ وضعیت پوشش بیمه برای UI (badge و highlight)
    /// </summary>
    public enum CoverageState
    {
        None = 0,      // بدون پوشش
        Partial = 1,   // بخشی پوشش (مثلاً سقف)
        Full = 2       // پوشش کامل
    }

    /// <summary>
    /// ✅ کد علت پوشش/عدم پوشش برای UI (tooltip و modal)
    /// </summary>
    public enum CoverageReasonCode
    {
        None = 0,
        BaseCovered,               // پوشش توسط پایه
        SuppCovered,               // پوشش توسط تکمیلی
        BaseCapReached,            // سقف پایه پر شد
        SuppCapReached,            // سقف تکمیلی پر شد
        FranchiseApplied,          // فرانشیز اعمال شد
        NotInCoverage,             // در شمول پوشش نبود
        PlanExpired,               // پایان اعتبار پلن
        ServiceExcluded,           // خدمت مستثنی
        MissingPricing,            // تعرفه/تعین‌ست ناقص
        DoctorNotEligible         // پزشک مجاز برای آن خدمت/دپارتمان نیست
    }

    /// <summary>
    /// ✅ DTO برای یک segment پوشش (پرداخت‌کننده + مبلغ + علت)
    /// </summary>
    public sealed class CoverageSegmentDto
    {
        public string Payer { get; set; } // "BASE" | "SUPP" | "PATIENT"
        public long AmountIRR { get; set; }
        public CoverageReasonCode Reason { get; set; }
        public string Note { get; set; } // پیام کوتاه برای tooltip
    }

    /// <summary>
    /// ✅ DTO برای جزئیات پوشش بیمه (state + segments + caps/franchise)
    /// </summary>
    public sealed class CoverageDetailsDto
    {
        public CoverageState State { get; set; }
        public List<CoverageSegmentDto> Segments { get; set; } = new List<CoverageSegmentDto>();
        
        // ✅ اختیاری: اطلاعات سقف/فرانشیز برای modal
        public long? BaseCapRemainingIRR { get; set; }
        public long? SuppCapRemainingIRR { get; set; }
        public long? FranchiseIRR { get; set; }
        public List<string> Warnings { get; set; } = new List<string>(); // پیام‌های قابل نمایش
    }

    /// <summary>
    /// ✅ DTO برای شکست محاسبه هر آیتم (جزئیات کامل سهم‌ها)
    /// </summary>
    public sealed class PricingBreakdownDto
    {
        public int ReceptionItemId { get; set; }   // ✅ اضافه شده برای RepriceAll
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public long UnitPriceIRR { get; set; }      // مبلغ واحد قبل از پوشش
        public long GrossIRR { get; set; }          // UnitPrice × Qty
        public long BaseCoveredIRR { get; set; }    // سهم بیمه پایه
        public long SuppCoveredIRR { get; set; }    // سهم بیمه تکمیلی
        public long PatientPayableIRR { get; set; } // سهم بیمار نهایی
        public string[] Notes { get; set; }         // نکات/رول‌های اعمال شده (فرانشیز/سقف/استثنا)
        
        // ✅ Friendly strings
        public string UnitPriceIRRStr { get; set; }
        public string GrossIRRStr { get; set; }
        public string BaseCoveredIRRStr { get; set; }
        public string SuppCoveredIRRStr { get; set; }
        public string PatientPayableIRRStr { get; set; }
        
        // ✅ جزئیات پوشش برای UI (badge + highlight + modal)
        public CoverageDetailsDto Coverage { get; set; }
    }

    /// <summary>
    /// ✅ DTO برای جمع‌های پذیرش (مجموع همه آیتم‌ها)
    /// </summary>
    public sealed class ReceptionTotalsDto
    {
        public long GrossIRR { get; set; }          // مجموع مبالغ آیتم‌ها
        public long BaseCoveredIRR { get; set; }    // مجموع پوشش پایه
        public long SuppCoveredIRR { get; set; }    // مجموع پوشش تکمیلی
        public long PatientPayableIRR { get; set; } // قابل‌پرداخت بیمار
        
        // Friendly strings
        public string GrossIRRStr { get; set; }
        public string BaseCoveredIRRStr { get; set; }
        public string SuppCoveredIRRStr { get; set; }
        public string PatientPayableIRRStr { get; set; }
    }

    #endregion
}

