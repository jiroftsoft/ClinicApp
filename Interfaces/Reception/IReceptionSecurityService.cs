using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// Interface برای مدیریت امنیت ماژول پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. بررسی مجوزهای کاربران
    /// 2. اعتبارسنجی دسترسی‌ها
    /// 3. مدیریت امنیت داده‌ها
    /// 4. بررسی قوانین امنیتی
    /// 5. مدیریت Audit Trail
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط امنیت
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    public interface IReceptionSecurityService
    {
        #region Authorization Methods

        /// <summary>
        /// بررسی مجوز ایجاد پذیرش
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> CanCreateReceptionAsync(string userId);

        /// <summary>
        /// بررسی مجوز ویرایش پذیرش
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> CanEditReceptionAsync(string userId, int receptionId);

        /// <summary>
        /// بررسی مجوز حذف پذیرش
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> CanDeleteReceptionAsync(string userId, int receptionId);

        /// <summary>
        /// بررسی مجوز مشاهده پذیرش
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> CanViewReceptionAsync(string userId, int receptionId);

        /// <summary>
        /// بررسی مجوز مشاهده لیست پذیرش‌ها
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> CanViewReceptionsListAsync(string userId);

        #endregion

        #region Role-Based Security Methods

        /// <summary>
        /// بررسی نقش کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleName">نام نقش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> IsUserInRoleAsync(string userId, string roleName);

        /// <summary>
        /// بررسی نقش‌های کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleNames">نام‌های نقش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> IsUserInAnyRoleAsync(string userId, List<string> roleNames);

        /// <summary>
        /// دریافت نقش‌های کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست نقش‌ها</returns>
        Task<List<string>> GetUserRolesAsync(string userId);

        #endregion

        #region Data Security Methods

        /// <summary>
        /// بررسی امنیت داده‌های پذیرش
        /// </summary>
        /// <param name="reception">پذیرش</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> ValidateReceptionDataSecurityAsync(Models.Entities.Reception.Reception reception, string userId);

        /// <summary>
        /// بررسی امنیت داده‌های بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> ValidatePatientDataSecurityAsync(int patientId, string userId);

        /// <summary>
        /// بررسی امنیت داده‌های پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> ValidateDoctorDataSecurityAsync(int doctorId, string userId);

        #endregion

        #region Input Validation Methods

        /// <summary>
        /// اعتبارسنجی ورودی‌های پذیرش
        /// </summary>
        /// <param name="input">ورودی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateReceptionInputAsync(string input);

        /// <summary>
        /// اعتبارسنجی ورودی‌های بیمار
        /// </summary>
        /// <param name="input">ورودی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidatePatientInputAsync(string input);

        /// <summary>
        /// اعتبارسنجی ورودی‌های پزشک
        /// </summary>
        /// <param name="input">ورودی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateDoctorInputAsync(string input);

        #endregion

        #region Anti-Forgery Methods

        /// <summary>
        /// بررسی Anti-Forgery Token
        /// </summary>
        /// <param name="token">توکن</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> ValidateAntiForgeryTokenAsync(string token);

        /// <summary>
        /// تولید Anti-Forgery Token
        /// </summary>
        /// <returns>توکن</returns>
        Task<string> GenerateAntiForgeryTokenAsync();

        #endregion

        #region Audit Trail Methods

        /// <summary>
        /// ثبت عملیات امنیتی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="action">عملیات</param>
        /// <param name="resource">منبع</param>
        /// <param name="result">نتیجه</param>
        /// <returns>نتیجه ثبت</returns>
        Task<bool> LogSecurityActionAsync(string userId, string action, string resource, bool result);

        /// <summary>
        /// دریافت تاریخچه امنیتی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="fromDate">از تاریخ</param>
        /// <param name="toDate">تا تاریخ</param>
        /// <returns>لیست تاریخچه</returns>
        Task<List<SecurityAuditEntry>> GetSecurityAuditTrailAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);

        #endregion

        #region Session Security Methods

        /// <summary>
        /// بررسی امنیت Session
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> ValidateSessionSecurityAsync(string userId);

        /// <summary>
        /// بررسی Timeout Session
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> ValidateSessionTimeoutAsync(string userId);

        /// <summary>
        /// تمدید Session
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه تمدید</returns>
        Task<bool> RenewSessionAsync(string userId);

        #endregion

        #region IP Security Methods

        /// <summary>
        /// بررسی IP Address
        /// </summary>
        /// <param name="ipAddress">آدرس IP</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> ValidateIpAddressAsync(string ipAddress);

        /// <summary>
        /// بررسی محدودیت IP
        /// </summary>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> ValidateIpRestrictionAsync(string ipAddress, string userId);

        #endregion

        #region Encryption Methods

        /// <summary>
        /// رمزنگاری داده‌های حساس
        /// </summary>
        /// <param name="data">داده</param>
        /// <returns>داده رمزنگاری شده</returns>
        Task<string> EncryptSensitiveDataAsync(string data);

        /// <summary>
        /// رمزگشایی داده‌های حساس
        /// </summary>
        /// <param name="encryptedData">داده رمزنگاری شده</param>
        /// <returns>داده رمزگشایی شده</returns>
        Task<string> DecryptSensitiveDataAsync(string encryptedData);

        #endregion

        #region Advanced Security Methods

        /// <summary>
        /// بررسی امنیت پیشرفته
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="action">عملیات</param>
        /// <param name="resource">منبع</param>
        /// <returns>نتیجه بررسی</returns>
        Task<SecurityCustomValidationResult> ValidateAdvancedSecurityAsync(string userId, string action, string resource);

        /// <summary>
        /// بررسی امنیت Real-time
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> ValidateRealTimeSecurityAsync(string userId);

        /// <summary>
        /// بررسی امنیت Multi-Factor
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> ValidateMultiFactorSecurityAsync(string userId);

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// ورودی تاریخچه امنیتی
    /// </summary>
    public class SecurityAuditEntry
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }
        public bool Result { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
    }

    /// <summary>
    /// نتیجه اعتبارسنجی امنیت
    /// </summary>
    public class SecurityValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public SecurityLevel Level { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// سطوح امنیت
    /// </summary>
    public enum SecurityLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    #endregion
}
