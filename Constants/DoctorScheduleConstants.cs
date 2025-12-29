namespace ClinicApp.Constants
{
    /// <summary>
    /// ثابت‌های ماژول برنامه کاری پزشکان - طراحی شده برای محیط درمانی
    /// مسئولیت: مدیریت تمام ثابت‌های ماژول DoctorSchedule بدون Hard Code
    /// ✅ Strongly-typed constants برای جلوگیری از Magic Strings
    /// طبق: DEVELOPMENT_CONTRACT.md, AI_ASSISTANT_MASTER_CONTRACT.md
    /// </summary>
    public static class DoctorScheduleConstants
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
            public const string DoctorId = "doctorId";

            /// <summary>
            /// نام پارامتر Query String برای DayOfWeek
            /// </summary>
            public const string DayOfWeek = "dayOfWeek";

            /// <summary>
            /// نام پارامتر Query String برای IsActive
            /// </summary>
            public const string IsActive = "isActive";

            /// <summary>
            /// نام پارامتر Query String برای SearchTerm
            /// </summary>
            public const string SearchTerm = "searchTerm";

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

        #region Filter Values

        /// <summary>
        /// مقادیر فیلتر وضعیت
        /// </summary>
        public static class FilterValues
        {
            /// <summary>
            /// مقدار فیلتر برای وضعیت فعال
            /// </summary>
            public const string Active = "true";

            /// <summary>
            /// مقدار فیلتر برای وضعیت غیرفعال
            /// </summary>
            public const string Inactive = "false";

            /// <summary>
            /// مقدار فیلتر برای همه وضعیت‌ها
            /// </summary>
            public const string All = "";
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
            /// ID عنصر فیلتر روز هفته
            /// </summary>
            public const string DayOfWeekFilter = "dayOfWeekFilter";

            /// <summary>
            /// ID عنصر فیلتر وضعیت
            /// </summary>
            public const string StatusFilter = "statusFilter";

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
            public const int PageSize = 10;

            /// <summary>
            /// مقدار پیش‌فرض PageNumber
            /// </summary>
            public const int PageNumber = 1;

            /// <summary>
            /// حداکثر مقدار PageSize
            /// </summary>
            public const int MaxPageSize = 100;
        }

        #endregion
    }
}

