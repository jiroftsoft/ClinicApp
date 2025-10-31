namespace ClinicApp.Helpers
{
    /// <summary>
    /// ✅ کدهای خطا استاندارد برای Reception API V1
    /// برای استفاده در ServiceResult.Code
    /// </summary>
    public static class ReceptionApiCodes
    {
        public const string SUCCESS = "SUCCESS";
        public const string PRICING_RECALCULATED = "PRICING_RECALCULATED";
        public const string INSURANCE_SET_MISSING = "INSURANCE_SET_MISSING";   // تعیین‌ست بیمه‌ای موجود نیست
        public const string DOCTOR_NOT_ELIGIBLE = "DOCTOR_NOT_ELIGIBLE";       // پزشک مجاز خدمت/دپارتمان نیست
        public const string INVALID_STATE = "INVALID_STATE";
        public const string UNHANDLED = "UNHANDLED";
        public const string VALIDATION = "VALIDATION";
        public const string NOT_FOUND = "NOT_FOUND";
        public const string SERVICE_UNAVAILABLE = "SERVICE_UNAVAILABLE";
    }
}

