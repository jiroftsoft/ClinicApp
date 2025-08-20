using System;
using System.Collections.Generic;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس نتیجه استاندارد برای بازگرداندن نتایج عملیات از لایه سرویس در سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. استانداردسازی نتایج عملیات در سراسر سیستم‌های پزشکی
    /// 2. پشتیبانی کامل از مدیریت خطاها بر اساس استانداردهای سیستم‌های پزشکی ایران
    /// 3. امکان ردیابی کامل عملیات‌ها برای حسابرسی و رعایت استانداردهای قانونی
    /// 4. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 5. مدیریت کامل اطلاعات حساس پزشکی با رعایت حریم خصوصی
    /// 6. پشتیبانی از تاریخ و زمان شمسی برای محیط‌های ایرانی
    /// 7. امکان اضافه کردن متادیتای پزشکی برای گزارش‌گیری و تحلیل
    /// 8. پشتیبانی از سطوح امنیتی مختلف برای اطلاعات پزشکی
    /// 9. رعایت استانداردهای بین‌المللی در سیستم‌های پزشکی
    /// 10. طراحی شده برای استفاده در محیط‌های پزشکی ایرانی با توجه به الزامات محلی
    /// </summary>
    public class ServiceResult
    {
        #region Properties

        /// <summary>
        /// نشان‌دهنده موفقیت یا شکست عملیات
        /// </summary>
        public bool Success { get; protected set; }

        /// <summary>
        /// پیام توصیفی نتیجه عملیات
        /// </summary>
        public string Message { get; protected set; }

        /// <summary>
        /// کد خطا یا سناریو برای مدیریت برنامه‌ای و گزارش‌گیری پزشکی
        /// </summary>
        public string Code { get; protected set; }

        /// <summary>
        /// سطح امنیتی خطا برای مدیریت بر اساس استانداردهای پزشکی
        /// </summary>
        public SecurityLevel SecurityLevel { get; protected set; }

        /// <summary>
        /// دسته‌بندی خطا برای گزارش‌گیری و تحلیل در سیستم‌های پزشکی
        /// </summary>
        public ErrorCategory Category { get; protected set; }

        /// <summary>
        /// زمان اجرای عملیات برای مانیتورینگ عملکرد و حسابرسی
        /// </summary>
        public DateTime Timestamp { get; protected set; }

        /// <summary>
        /// زمان اجرای عملیات به شمسی برای محیط‌های ایرانی
        /// </summary>
        public string TimestampShamsi { get; protected set; }

        /// <summary>
        /// اطلاعات اضافی برای لاگ‌نویسی، ردیابی و گزارش‌گیری پزشکی
        /// </summary>
        public Dictionary<string, object> Metadata { get; protected set; }

        /// <summary>
        /// شناسه عملیات برای ردیابی کامل در سیستم‌های پزشکی
        /// </summary>
        public string OperationId { get; protected set; }

        /// <summary>
        /// شناسه کاربر انجام‌دهنده عملیات برای ردیابی در سیستم‌های پزشکی
        /// </summary>
        public string UserId { get; protected set; }

        /// <summary>
        /// نام کاربری انجام‌دهنده عملیات برای گزارش‌گیری پزشکی
        /// </summary>
        public string UserName { get; protected set; }

        /// <summary>
        /// نام واقعی کاربر انجام‌دهنده عملیات برای گزارش‌گیری پزشکی
        /// </summary>
        public string UserFullName { get; protected set; }

        /// <summary>
        /// نام نقش کاربر انجام‌دهنده عملیات برای گزارش‌گیری پزشکی
        /// </summary>
        public string UserRole { get; protected set; }

        /// <summary>
        /// نام سیستم یا ماژول انجام‌دهنده عملیات برای ردیابی
        /// </summary>
        public string SystemName { get; protected set; }

        /// <summary>
        /// نام عملیات انجام‌شده برای گزارش‌گیری پزشکی
        /// </summary>
        public string OperationName { get; protected set; }

        /// <summary>
        /// وضعیت عملیات برای سیستم‌های پزشکی
        /// </summary>
        public OperationStatus Status { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        protected ServiceResult()
        {
            Timestamp = DateTime.UtcNow;
            TimestampShamsi = Timestamp.ToPersianDateTime();
            Metadata = new Dictionary<string, object>();
            OperationId = Guid.NewGuid().ToString();
            Status = OperationStatus.Pending;
            SecurityLevel = SecurityLevel.Medium;
            Category = ErrorCategory.General;
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// ایجاد یک نتیجه موفق
        /// </summary>
        /// <param name="message">پیام موفقیت</param>
        /// <param name="operationName">نام عملیات</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="userName">نام کاربری</param>
        /// <param name="userFullName">نام واقعی کاربر</param>
        /// <param name="userRole">نقش کاربر</param>
        /// <param name="systemName">نام سیستم</param>
        /// <returns>نتیجه موفق</returns>
        public static ServiceResult Successful(
            string message = "عملیات با موفقیت انجام شد.",
            string operationName = null,
            string userId = null,
            string userName = null,
            string userFullName = null,
            string userRole = null,
            string systemName = "ClinicApp")
        {
            return new ServiceResult
            {
                Success = true,
                Message = message,
                Code = "SUCCESS",
                OperationName = operationName,
                UserId = userId,
                UserName = userName,
                UserFullName = userFullName,
                UserRole = userRole,
                SystemName = systemName,
                Status = OperationStatus.Completed
            };
        }

        /// <summary>
        /// ایجاد یک نتیجه ناموفق
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="code">کد خطا</param>
        /// <param name="category">دسته‌بندی خطا</param>
        /// <param name="securityLevel">سطح امنیتی خطا</param>
        /// <param name="operationName">نام عملیات</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="userName">نام کاربری</param>
        /// <param name="userFullName">نام واقعی کاربر</param>
        /// <param name="userRole">نقش کاربر</param>
        /// <param name="systemName">نام سیستم</param>
        /// <returns>نتیجه ناموفق</returns>
        public static ServiceResult Failed(
            string message,
            string code = "GENERAL_ERROR",
            ErrorCategory category = ErrorCategory.General,
            SecurityLevel securityLevel = SecurityLevel.Medium,
            string operationName = null,
            string userId = null,
            string userName = null,
            string userFullName = null,
            string userRole = null,
            string systemName = "ClinicApp")
        {
            return new ServiceResult
            {
                Success = false,
                Message = message,
                Code = code,
                Category = category,
                SecurityLevel = securityLevel,
                OperationName = operationName,
                UserId = userId,
                UserName = userName,
                UserFullName = userFullName,
                UserRole = userRole,
                SystemName = systemName,
                Status = OperationStatus.Failed
            };
        }

        #endregion

        #region Metadata Management

        /// <summary>
        /// اضافه کردن اطلاعات متادیتا برای ردیابی و گزارش‌گیری پزشکی
        /// </summary>
        /// <param name="key">کلید</param>
        /// <param name="value">مقدار</param>
        /// <returns>خود آبجکت برای زنجیره‌سازی</returns>
        public ServiceResult WithMetadata(string key, object value)
        {
            Metadata[key] = value;
            return this;
        }

        /// <summary>
        /// اضافه کردن اطلاعات متادیتای پزشکی برای حسابرسی
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="patientName">نام بیمار</param>
        /// <param name="insuranceId">شناسه بیمه</param>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>خود آبجکت برای زنجیره‌سازی</returns>
        public ServiceResult WithMedicalAuditInfo(
            int? patientId = null,
            string patientName = null,
            int? insuranceId = null,
            int? receptionId = null)
        {
            if (patientId.HasValue)
                Metadata["PatientId"] = patientId.Value;

            if (!string.IsNullOrEmpty(patientName))
                Metadata["PatientName"] = patientName;

            if (insuranceId.HasValue)
                Metadata["InsuranceId"] = insuranceId.Value;

            if (receptionId.HasValue)
                Metadata["ReceptionId"] = receptionId.Value;

            return this;
        }

        /// <summary>
        /// اضافه کردن اطلاعات حساس پزشکی با رعایت استانداردهای امنیتی
        /// </summary>
        /// <param name="sensitiveData">اطلاعات حساس پزشکی</param>
        /// <param name="securityLevel">سطح امنیتی اطلاعات</param>
        /// <returns>خود آبجکت برای زنجیره‌سازی</returns>
        public ServiceResult WithSensitiveData(object sensitiveData, SecurityLevel securityLevel = SecurityLevel.High)
        {
            Metadata["SensitiveData"] = sensitiveData;
            Metadata["SensitiveDataSecurityLevel"] = securityLevel;
            return this;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// تنظیم وضعیت عملیات برای سیستم‌های پزشکی
        /// </summary>
        /// <param name="status">وضعیت جدید</param>
        /// <returns>خود آبجکت برای زنجیره‌سازی</returns>
        public ServiceResult WithStatus(OperationStatus status)
        {
            Status = status;
            return this;
        }

        /// <summary>
        /// تنظیم سطح امنیتی خطا برای سیستم‌های پزشکی
        /// </summary>
        /// <param name="securityLevel">سطح امنیتی جدید</param>
        /// <returns>خود آبجکت برای زنجیره‌سازی</returns>
        public ServiceResult WithSecurityLevel(SecurityLevel securityLevel)
        {
            SecurityLevel = securityLevel;
            return this;
        }

        /// <summary>
        /// تنظیم دسته‌بندی خطا برای گزارش‌گیری پزشکی
        /// </summary>
        /// <param name="category">دسته‌بندی جدید</param>
        /// <returns>خود آبجکت برای زنجیره‌سازی</returns>
        public ServiceResult WithCategory(ErrorCategory category)
        {
            Category = category;
            return this;
        }

        /// <summary>
        /// تنظیم شناسه عملیات برای ردیابی کامل در سیستم‌های پزشکی
        /// </summary>
        /// <param name="operationId">شناسه جدید عملیات</param>
        /// <returns>خود آبجکت برای زنجیره‌سازی</returns>
        public ServiceResult WithOperationId(string operationId)
        {
            OperationId = operationId;
            return this;
        }

        #endregion
    }

    /// <summary>
    /// نسخه ژنریک از ServiceResult برای بازگرداندن یک داده (Payload) به همراه نتیجه عملیات در سیستم‌های پزشکی.
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از انواع مختلف داده‌های پزشکی
    /// 2. قابلیت مدیریت خطا همراه با داده
    /// 3. مناسب برای API‌های RESTful در سیستم‌های پزشکی
    /// 4. پشتیبانی از صفحه‌بندی برای لیست‌های پزشکی
    /// 5. مدیریت اطلاعات حساس پزشکی با رعایت استانداردهای امنیتی
    /// 6. پشتیبانی از تاریخ و زمان شمسی برای محیط‌های ایرانی
    /// 7. امکان اضافه کردن متادیتای پزشکی برای گزارش‌گیری
    /// 8. رعایت استانداردهای بین‌المللی در سیستم‌های پزشکی
    /// 9. طراحی شده برای استفاده در محیط‌های پزشکی ایرانی با توجه به الزامات محلی
    /// </summary>
    /// <typeparam name="T">نوع داده‌ای که قرار است بازگردانده شود.</typeparam>
    public class ServiceResult<T> : ServiceResult
    {
        #region Properties

        /// <summary>
        /// داده بازگشتی از عملیات
        /// </summary>
        public T Data { get; private set; }

        /// <summary>
        /// تعداد کل رکوردها (برای صفحه‌بندی لیست‌های پزشکی)
        /// </summary>
        public int? TotalCount { get; private set; }

        /// <summary>
        /// شماره صفحه فعلی (برای صفحه‌بندی لیست‌های پزشکی)
        /// </summary>
        public int? PageNumber { get; private set; }

        /// <summary>
        /// تعداد آیتم‌ها در هر صفحه (برای صفحه‌بندی لیست‌های پزشکی)
        /// </summary>
        public int? PageSize { get; private set; }

        /// <summary>
        /// نشان‌دهنده وجود صفحه بعدی (برای صفحه‌بندی لیست‌های پزشکی)
        /// </summary>
        public bool HasNextPage { get; private set; }

        /// <summary>
        /// نشان‌دهنده وجود صفحه قبلی (برای صفحه‌بندی لیست‌های پزشکی)
        /// </summary>
        public bool HasPreviousPage { get; private set; }

        #endregion

        #region Factory Methods

        /// <summary>
        /// ایجاد یک نتیجه موفق با داده
        /// </summary>
        /// <param name="data">داده بازگشتی</param>
        /// <param name="message">پیام موفقیت</param>
        /// <param name="totalCount">تعداد کل رکوردها</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
        /// <param name="operationName">نام عملیات</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="userName">نام کاربری</param>
        /// <param name="userFullName">نام واقعی کاربر</param>
        /// <param name="userRole">نقش کاربر</param>
        /// <param name="systemName">نام سیستم</param>
        /// <returns>نتیجه موفق با داده</returns>
        public static ServiceResult<T> Successful(
            T data,
            string message = "عملیات با موفقیت انجام شد.",
            int? totalCount = null,
            int? pageNumber = null,
            int? pageSize = null,
            string operationName = null,
            string userId = null,
            string userName = null,
            string userFullName = null,
            string userRole = null,
            string systemName = "ClinicApp")
        {
            var result = new ServiceResult<T>
            {
                Success = true,
                Message = message,
                Data = data,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                OperationName = operationName,
                UserId = userId,
                UserName = userName,
                UserFullName = userFullName,
                UserRole = userRole,
                SystemName = systemName,
                Status = OperationStatus.Completed,
                Code = "SUCCESS"
            };

            result.CalculatePagination();
            return result;
        }

        /// <summary>
        /// ایجاد یک نتیجه ناموفق با داده
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="code">کد خطا</param>
        /// <param name="category">دسته‌بندی خطا</param>
        /// <param name="securityLevel">سطح امنیتی خطا</param>
        /// <param name="operationName">نام عملیات</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="userName">نام کاربری</param>
        /// <param name="userFullName">نام واقعی کاربر</param>
        /// <param name="userRole">نقش کاربر</param>
        /// <param name="systemName">نام سیستم</param>
        /// <returns>نتیجه ناموفق بدون داده</returns>
        public static new ServiceResult<T> Failed(
            string message,
            string code = "GENERAL_ERROR",
            ErrorCategory category = ErrorCategory.General,
            SecurityLevel securityLevel = SecurityLevel.Medium,
            string operationName = null,
            string userId = null,
            string userName = null,
            string userFullName = null,
            string userRole = null,
            string systemName = "ClinicApp")
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                Code = code,
                Category = category,
                SecurityLevel = securityLevel,
                Data = default(T),
                OperationName = operationName,
                UserId = userId,
                UserName = userName,
                UserFullName = userFullName,
                UserRole = userRole,
                SystemName = systemName,
                Status = OperationStatus.Failed
            };
        }

        #endregion

        #region Pagination Methods

        /// <summary>
        /// محاسبه اطلاعات صفحه‌بندی
        /// </summary>
        private void CalculatePagination()
        {
            if (PageNumber.HasValue && PageSize.HasValue && TotalCount.HasValue)
            {
                HasNextPage = PageNumber < (TotalCount.Value + PageSize.Value - 1) / PageSize.Value;
                HasPreviousPage = PageNumber > 1;
            }
            else
            {
                HasNextPage = false;
                HasPreviousPage = false;
            }
        }

        /// <summary>
        /// تنظیم اطلاعات صفحه‌بندی
        /// </summary>
        /// <param name="totalCount">تعداد کل رکوردها</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
        /// <returns>خود آبجکت برای زنجیره‌سازی</returns>
        public ServiceResult<T> WithPagination(int totalCount, int pageNumber, int pageSize)
        {
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            CalculatePagination();
            return this;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// تنظیم داده بازگشتی
        /// </summary>
        /// <param name="data">داده جدید</param>
        /// <returns>خود آبجکت برای زنجیره‌سازی</returns>
        public ServiceResult<T> WithData(T data)
        {
            Data = data;
            return this;
        }

        /// <summary>
        /// اضافه کردن اطلاعات متادیتا
        /// </summary>
        /// <param name="key">کلید</param>
        /// <param name="value">مقدار</param>
        /// <returns>خود آبجکت برای زنجیره‌سازی</returns>
        public new ServiceResult<T> WithMetadata(string key, object value)
        {
            base.WithMetadata(key, value);
            return this;
        }

        /// <summary>
        /// اضافه کردن اطلاعات متادیتای پزشکی برای حسابرسی
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="patientName">نام بیمار</param>
        /// <param name="insuranceId">شناسه بیمه</param>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>خود آبجکت برای زنجیره‌سازی</returns>
        public new ServiceResult<T> WithMedicalAuditInfo(
            int? patientId = null,
            string patientName = null,
            int? insuranceId = null,
            int? receptionId = null)
        {
            base.WithMedicalAuditInfo(patientId, patientName, insuranceId, receptionId);
            return this;
        }

        /// <summary>
        /// اضافه کردن اطلاعات حساس پزشکی با رعایت استانداردهای امنیتی
        /// </summary>
        /// <param name="sensitiveData">اطلاعات حساس پزشکی</param>
        /// <param name="securityLevel">سطح امنیتی اطلاعات</param>
        /// <returns>خود آبجکت برای زنجیره‌سازی</returns>
        public new ServiceResult<T> WithSensitiveData(object sensitiveData, SecurityLevel securityLevel = SecurityLevel.High)
        {
            base.WithSensitiveData(sensitiveData, securityLevel);
            return this;
        }

        #endregion
    }
}