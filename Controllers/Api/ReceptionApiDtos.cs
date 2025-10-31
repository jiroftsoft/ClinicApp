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
}

