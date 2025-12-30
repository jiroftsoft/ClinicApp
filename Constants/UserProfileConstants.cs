namespace ClinicApp.Constants
{
    /// <summary>
    /// ثابت‌های ماژول پروفایل کاربر - طراحی شده برای محیط درمانی
    /// مسئولیت: مدیریت تمام ثابت‌های ماژول UserProfile بدون Hard Code
    /// ✅ Strongly-typed constants برای جلوگیری از Magic Strings
    /// طبق: DEVELOPMENT_CONTRACT.md, AI_ASSISTANT_MASTER_CONTRACT.md
    /// </summary>
    public static class UserProfileConstants
    {
        #region Error Codes

        /// <summary>
        /// کدهای خطا
        /// </summary>
        public static class ErrorCodes
        {
            /// <summary>
            /// کد خطا برای عدم دسترسی (کاربر نمی‌تواند پروفایل کاربر دیگر را ویرایش کند)
            /// </summary>
            public const string Unauthorized = "UNAUTHORIZED";

            /// <summary>
            /// کد خطا برای کاربر یافت نشد
            /// </summary>
            public const string UserNotFound = "USER_NOT_FOUND";

            /// <summary>
            /// کد خطا برای شناسه کاربر نامعتبر
            /// </summary>
            public const string InvalidUserId = "INVALID_USER_ID";
        }

        #endregion

        #region Messages

        /// <summary>
        /// پیام‌های سیستم
        /// </summary>
        public static class Messages
        {
            /// <summary>
            /// پیام موفقیت برای به‌روزرسانی پروفایل
            /// </summary>
            public const string ProfileUpdatedSuccessfully = "پروفایل با موفقیت به‌روزرسانی شد.";

            /// <summary>
            /// پیام خطا برای شناسه کاربر نامعتبر
            /// </summary>
            public const string InvalidUserId = "شناسه کاربر معتبر نیست.";

            /// <summary>
            /// پیام خطا برای کاربر یافت نشد
            /// </summary>
            public const string UserNotFound = "کاربر یافت نشد.";

            /// <summary>
            /// پیام خطا برای عدم دسترسی
            /// </summary>
            public const string Unauthorized = "شما فقط می‌توانید پروفایل خود را ویرایش کنید.";

            /// <summary>
            /// پیام خطا برای دریافت اطلاعات پروفایل
            /// </summary>
            public const string GetProfileError = "خطا در دریافت اطلاعات پروفایل.";

            /// <summary>
            /// پیام خطا برای به‌روزرسانی پروفایل
            /// </summary>
            public const string UpdateProfileError = "خطا در به‌روزرسانی پروفایل.";

            /// <summary>
            /// پیام خطا برای بارگذاری پروفایل
            /// </summary>
            public const string LoadProfileError = "خطا در بارگذاری پروفایل";

            /// <summary>
            /// پیام خطا برای لطفاً دوباره وارد شوید
            /// </summary>
            public const string PleaseLoginAgain = "لطفاً دوباره وارد شوید.";

            /// <summary>
            /// پیام خطا برای فیلدهای الزامی
            /// </summary>
            public const string RequiredFieldsMissing = "لطفاً تمام فیلدهای الزامی را پر کنید.";

            /// <summary>
            /// پیام اطلاعاتی برای کد ملی غیرقابل تغییر
            /// </summary>
            public const string NationalCodeNotEditable = "کد ملی قابل تغییر نیست.";

            /// <summary>
            /// پیام اطلاعاتی برای شماره تلفن غیرقابل تغییر
            /// </summary>
            public const string PhoneNumberNotEditable = "برای تغییر شماره تلفن با پشتیبانی تماس بگیرید.";
        }

        #endregion

        #region View Titles

        /// <summary>
        /// عناوین صفحات
        /// </summary>
        public static class ViewTitles
        {
            /// <summary>
            /// عنوان صفحه پروفایل
            /// </summary>
            public const string Profile = "پروفایل من";

            /// <summary>
            /// توضیحات صفحه پروفایل
            /// </summary>
            public const string ProfileDescription = "اطلاعات پروفایل خود را مشاهده و ویرایش کنید.";
        }

        #endregion

        #region Action Names

        /// <summary>
        /// نام Action ها
        /// </summary>
        public static class Actions
        {
            /// <summary>
            /// نام Action برای نمایش پروفایل
            /// </summary>
            public const string Profile = "Profile";

            /// <summary>
            /// نام Action برای لاگین
            /// </summary>
            public const string Login = "Login";

            /// <summary>
            /// نام Action برای صفحه اصلی
            /// </summary>
            public const string Index = "Index";

            /// <summary>
            /// نام Controller برای Account
            /// </summary>
            public const string Account = "Account";

            /// <summary>
            /// نام Controller برای Home
            /// </summary>
            public const string Home = "Home";
        }

        #endregion
    }
}

