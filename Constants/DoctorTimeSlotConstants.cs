namespace ClinicApp.Constants
{
    /// <summary>
    /// ثابت‌های ماژول مدیریت اسلات‌های زمانی پزشکان - طراحی شده برای محیط درمانی
    /// مسئولیت: مدیریت تمام ثابت‌های ماژول DoctorTimeSlot بدون Hard Code
    /// ✅ Strongly-typed constants برای جلوگیری از Magic Strings
    /// طبق: DEVELOPMENT_CONTRACT.md, AI_ASSISTANT_MASTER_CONTRACT.md
    /// </summary>
    public static class DoctorTimeSlotConstants
    {
        #region Query String Parameters

        /// <summary>
        /// پارامترهای Query String
        /// </summary>
        public static class QueryParameters
        {
            /// <summary>
            /// نام پارامتر Query String برای DoctorId
            /// </summary>
            public const string DoctorId = "DoctorId";

            /// <summary>
            /// نام پارامتر Query String برای StartDate
            /// </summary>
            public const string StartDate = "StartDate";

            /// <summary>
            /// نام پارامتر Query String برای EndDate
            /// </summary>
            public const string EndDate = "EndDate";

            /// <summary>
            /// نام پارامتر Query String برای Status
            /// </summary>
            public const string Status = "Status";

            /// <summary>
            /// نام پارامتر Query String برای SearchTerm
            /// </summary>
            public const string SearchTerm = "SearchTerm";

            /// <summary>
            /// نام پارامتر Query String برای Page
            /// </summary>
            public const string Page = "page";

            /// <summary>
            /// نام پارامتر Query String برای PageSize
            /// </summary>
            public const string PageSize = "pageSize";
        }

        #endregion

        #region JavaScript Element IDs

        /// <summary>
        /// شناسه‌های عناصر JavaScript
        /// </summary>
        public static class ElementIds
        {
            /// <summary>
            /// ID عنصر فیلتر پزشک
            /// </summary>
            public const string DoctorFilter = "doctorFilter";

            /// <summary>
            /// ID عنصر فیلتر تاریخ شروع
            /// </summary>
            public const string StartDateFilter = "startDateFilter";

            /// <summary>
            /// ID عنصر فیلتر تاریخ پایان
            /// </summary>
            public const string EndDateFilter = "endDateFilter";

            /// <summary>
            /// ID عنصر فیلتر وضعیت
            /// </summary>
            public const string StatusFilter = "statusFilter";

            /// <summary>
            /// ID عنصر جستجو
            /// </summary>
            public const string SearchTermInput = "searchTermInput";

            /// <summary>
            /// ID عنصر دکمه جستجو
            /// </summary>
            public const string SearchBtn = "searchBtn";
        }

        #endregion

        #region Default Values

        /// <summary>
        /// مقادیر پیش‌فرض
        /// </summary>
        public static class Defaults
        {
            /// <summary>
            /// مقدار پیش‌فرض PageSize
            /// </summary>
            public const int PageSize = 20;

            /// <summary>
            /// مقدار پیش‌فرض PageNumber
            /// </summary>
            public const int PageNumber = 1;

            /// <summary>
            /// حداکثر مقدار PageSize
            /// </summary>
            public const int MaxPageSize = 100;

            /// <summary>
            /// تعداد روزهای پیش‌فرض برای فیلتر تاریخ (30 روز)
            /// </summary>
            public const int DefaultDateRangeDays = 30;
        }

        #endregion

        #region Messages

        /// <summary>
        /// پیام‌های سیستم
        /// </summary>
        public static class Messages
        {
            /// <summary>
            /// پیام موفقیت حذف اسلات
            /// </summary>
            public const string TimeSlotDeletedSuccessfully = "اسلات زمانی با موفقیت حذف شد.";

            /// <summary>
            /// پیام موفقیت تغییر وضعیت
            /// </summary>
            public const string TimeSlotStatusUpdatedSuccessfully = "وضعیت اسلات زمانی با موفقیت تغییر یافت.";

            /// <summary>
            /// پیام موفقیت آزاد کردن اسلات
            /// </summary>
            public const string TimeSlotReleasedSuccessfully = "اسلات زمانی با موفقیت آزاد شد.";

            /// <summary>
            /// پیام خطای شناسه نامعتبر
            /// </summary>
            public const string InvalidTimeSlotId = "شناسه اسلات زمانی نامعتبر است.";

            /// <summary>
            /// پیام خطای یافت نشدن اسلات
            /// </summary>
            public const string TimeSlotNotFound = "اسلات زمانی مورد نظر یافت نشد.";

            /// <summary>
            /// پیام خطای بارگذاری لیست
            /// </summary>
            public const string ErrorLoadingTimeSlots = "خطا در بارگذاری لیست اسلات‌های زمانی";

            /// <summary>
            /// پیام خطای بارگذاری جزئیات
            /// </summary>
            public const string ErrorLoadingDetails = "خطا در بارگذاری جزئیات اسلات زمانی";

            /// <summary>
            /// پیام خطای حذف
            /// </summary>
            public const string ErrorDeletingTimeSlot = "خطا در حذف اسلات زمانی";

            /// <summary>
            /// پیام خطای تغییر وضعیت
            /// </summary>
            public const string ErrorUpdatingStatus = "خطا در تغییر وضعیت اسلات زمانی";

            /// <summary>
            /// پیام خطای آزاد کردن
            /// </summary>
            public const string ErrorReleasingTimeSlot = "خطا در آزاد کردن اسلات زمانی";

            /// <summary>
            /// پیام تأیید حذف
            /// </summary>
            public const string ConfirmDelete = "آیا از حذف این اسلات اطمینان دارید؟";

            /// <summary>
            /// پیام تأیید آزاد کردن
            /// </summary>
            public const string ConfirmRelease = "آیا از آزاد کردن این اسلات اطمینان دارید؟";
        }

        #endregion
    }
}

