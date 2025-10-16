using System;

namespace ClinicApp.Constants
{
    /// <summary>
    /// Constants تخصصی برای فرم پذیرش فوق حرفه‌ای - محیط درمانی
    /// </summary>
    public static class ReceptionFormConstants
    {
        /// <summary>
        /// تنظیمات رنگ‌ها برای محیط درمانی
        /// </summary>
        public static class Colors
        {
            // رنگ‌های اصلی
            public const string Primary = "#007bff";
            public const string Secondary = "#6c757d";
            public const string Success = "#28a745";
            public const string Warning = "#ffc107";
            public const string Danger = "#dc3545";
            public const string Info = "#17a2b8";
            
            // رنگ‌های مخصوص محیط درمانی
            public const string MedicalBlue = "#0056b3";
            public const string MedicalGreen = "#20c997";
            public const string MedicalRed = "#e74c3c";
            public const string MedicalOrange = "#fd7e14";
            
            // رنگ‌های پس‌زمینه
            public const string BackgroundLight = "#f8f9fa";
            public const string BackgroundWhite = "#ffffff";
            public const string BackgroundGray = "#e9ecef";
            
            // رنگ‌های آکاردئون
            public const string AccordionHeader = "#007bff";
            public const string AccordionContent = "#ffffff";
            public const string AccordionBorder = "#dee2e6";
            
            // رنگ‌های وضعیت
            public const string StatusReady = "#6c757d";
            public const string StatusActive = "#28a745";
            public const string StatusWarning = "#ffc107";
            public const string StatusError = "#dc3545";
        }

        /// <summary>
        /// تنظیمات اندازه‌ها برای مانیتور 22+ اینچ
        /// </summary>
        public static class Sizes
        {
            // اندازه‌های کلی
            public const string ContainerMaxWidth = "100%";
            public const string ContainerMinHeight = "85vh";
            
            // اندازه‌های آکاردئون
            public const string AccordionHeaderHeight = "60px";
            public const string AccordionContentPadding = "2rem";
            public const string AccordionBorderRadius = "8px";
            
            // اندازه‌های فیلدها
            public const string FieldHeight = "45px";
            public const string FieldPadding = "0.75rem";
            public const string FieldBorderRadius = "6px";
            
            // اندازه‌های دکمه‌ها
            public const string ButtonHeight = "45px";
            public const string ButtonPadding = "0.75rem 1.5rem";
            public const string ButtonBorderRadius = "6px";
            
            // اندازه‌های فونت
            public const string FontSizeSmall = "0.875rem";
            public const string FontSizeBase = "1rem";
            public const string FontSizeLarge = "1.125rem";
            public const string FontSizeXLarge = "1.25rem";
        }

        /// <summary>
        /// پیام‌های سیستم
        /// </summary>
        public static class Messages
        {
            // پیام‌های موفقیت
            public const string PatientFound = "بیمار با موفقیت یافت شد";
            public const string PatientSaved = "اطلاعات بیمار با موفقیت ذخیره شد";
            public const string PatientSavedSuccess = "اطلاعات بیمار با موفقیت ذخیره شد";
            public const string PatientSaveError = "خطا در ذخیره اطلاعات بیمار";
            public const string InsuranceLoaded = "اطلاعات بیمه بارگذاری شد";
            public const string ReceptionCreated = "پذیرش با موفقیت ثبت شد";
            public const string PaymentProcessed = "پرداخت با موفقیت انجام شد";
            
            // پیام‌های خطا
            public const string PatientNotFound = "بیمار یافت نشد";
            public const string InvalidNationalCode = "کد ملی نامعتبر است";
            public const string NationalCodeInvalid = "کد ملی نامعتبر است";
            public const string InsuranceNotFound = "بیمه‌ای برای این بیمار ثبت نشده است";
            public const string DepartmentRequired = "انتخاب دپارتمان الزامی است";
            public const string DoctorRequired = "انتخاب پزشک الزامی است";
            public const string ServiceRequired = "انتخاب حداقل یک خدمت الزامی است";
            public const string PaymentRequired = "پرداخت الزامی است";
            
            // پیام‌های هشدار
            public const string FormIncomplete = "فرم ناقص است. لطفاً تمام فیلدهای الزامی را پر کنید";
            public const string DataNotSaved = "اطلاعات ذخیره نشده است";
            public const string NetworkError = "خطا در ارتباط با سرور";
            
            // پیام‌های اطلاعاتی
            public const string Loading = "در حال بارگذاری...";
            public const string Searching = "در حال جستجو...";
            public const string Processing = "در حال پردازش...";
            public const string Calculating = "در حال محاسبه...";
        }

        /// <summary>
        /// تنظیمات اعتبارسنجی
        /// </summary>
        public static class Validation
        {
            // محدودیت‌های فیلدها
            public const int NationalCodeLength = 10;
            public const int NameMinLength = 2;
            public const int NameMaxLength = 50;
            public const int MinNameLength = 2;
            public const int MaxNameLength = 50;
            public const int PhoneMinLength = 11;
            public const int PhoneMaxLength = 13;
            public const int AddressMaxLength = 500;
            public const int MinAge = 0;
            public const int MaxAge = 120;
            
            // الگوهای اعتبارسنجی
            public const string NationalCodePattern = @"^\d{10}$";
            public const string PhonePattern = @"^(\+98|0)?9\d{9}$";
            public const string PhoneNumberRegex = @"^09\d{9}$";
            public const string NamePattern = @"^[\u0600-\u06FF\s]+$";
            
            // پیام‌های اعتبارسنجی
            public const string NationalCodeRequired = "کد ملی الزامی است";
            public const string NationalCodeInvalid = "کد ملی باید 10 رقم باشد";
            public const string NameRequired = "نام الزامی است";
            public const string NameMinLengthError = "نام باید حداقل 2 کاراکتر باشد";
            public const string PhoneInvalid = "شماره تلفن نامعتبر است";
            public const string BirthDateInvalid = "تاریخ تولد نامعتبر است";
        }

        /// <summary>
        /// تنظیمات آکاردئون
        /// </summary>
        public static class Accordion
        {
            // شناسه‌های بخش‌ها
            public const string PatientSectionId = "patientAccordion";
            public const string InsuranceSectionId = "insuranceAccordion";
            public const string DepartmentSectionId = "departmentAccordion";
            public const string ServiceSectionId = "serviceAccordion";
            public const string PaymentSectionId = "paymentAccordion";
            
            // عنوان‌های بخش‌ها
            public const string PatientSectionTitle = "اطلاعات بیمار";
            public const string InsuranceSectionTitle = "اطلاعات بیمه";
            public const string DepartmentSectionTitle = "دپارتمان و پزشک";
            public const string ServiceSectionTitle = "انتخاب خدمات";
            public const string PaymentSectionTitle = "پرداخت";
            
            // آیکون‌های بخش‌ها
            public const string PatientSectionIcon = "fas fa-user";
            public const string InsuranceSectionIcon = "fas fa-shield-alt";
            public const string DepartmentSectionIcon = "fas fa-hospital";
            public const string ServiceSectionIcon = "fas fa-stethoscope";
            public const string PaymentSectionIcon = "fas fa-credit-card";
        }

        /// <summary>
        /// تنظیمات Select2
        /// </summary>
        public static class Select2
        {
            // تنظیمات عمومی
            public const string Placeholder = "انتخاب کنید...";
            public const string NoResultsText = "نتیجه‌ای یافت نشد";
            public const string SearchingText = "در حال جستجو...";
            public const string LoadingText = "در حال بارگذاری...";
            
            // تنظیمات جستجو
            public const int MinimumInputLength = 2;
            public const int MaximumSelectionLength = 1;
            public const int Delay = 300;
            
            // تنظیمات نمایش
            public const int DropdownCssClass = 0;
            public const int Width = 100;
            public const bool AllowClear = true;
        }

        /// <summary>
        /// تنظیمات AJAX
        /// </summary>
        public static class Ajax
        {
            // Timeout ها
            public const int DefaultTimeout = 30000;
            public const int SearchTimeout = 10000;
            public const int SaveTimeout = 15000;
            public const int PaymentTimeout = 60000;
            
            // Retry attempts
            public const int MaxRetryAttempts = 3;
            public const int RetryDelay = 1000;
        }

        /// <summary>
        /// تنظیمات محیط درمانی
        /// </summary>
        public static class MedicalEnvironment
        {
            // تنظیمات امنیت
            public const bool RequireHttps = true;
            public const bool EnableAuditLog = true;
            public const bool EnableDataEncryption = true;
            
            // تنظیمات عملکرد
            public const bool EnableCaching = false;
            public const bool EnableCompression = true;
            public const bool EnableMinification = true;
            
            // تنظیمات UI
            public const bool EnableAnimations = false;
            public const bool EnableTooltips = true;
            public const bool EnableKeyboardShortcuts = true;
        }

        /// <summary>
        /// تنظیمات پیش‌فرض
        /// </summary>
        public static class Defaults
        {
            // مقادیر پیش‌فرض
            public const int DefaultClinicId = 1;
            public const int DefaultDepartmentId = 1;
            public const string DefaultReceptionType = "Normal";
            public const string DefaultPriority = "Normal";
            
            // تنظیمات تاریخ
            public const string DateFormat = "yyyy/MM/dd";
            public const string TimeFormat = "HH:mm:ss";
            public const string DateTimeFormat = "yyyy/MM/dd HH:mm:ss";
            
            // تنظیمات محلی
            public const string Locale = "fa-IR";
            public const string TimeZone = "Asia/Tehran";
        }
    }
}
