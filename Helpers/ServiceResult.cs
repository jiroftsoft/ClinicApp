using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// سطوح امنیتی برای سیستم‌های پزشکی
    /// این سطوح بر اساس استانداردهای امنیتی سیستم‌های پزشکی تعریف شده‌اند
    /// </summary>
    public enum SecurityLevel
    {
        [Description("پایین - اطلاعات عمومی")]
        Low = 1,

        [Description("متوسط - اطلاعات داخلی سیستم")]
        Medium = 2,

        [Description("بالا - اطلاعات حساس پزشکی")]
        High = 3,

        [Description("بحرانی - اطلاعات حیاتی و محرمانه پزشکی")]
        Critical = 4
    }

    /// <summary>
    /// دسته‌بندی خطاها برای سیستم‌های پزشکی
    /// این دسته‌بندی‌ها بر اساس استانداردهای سیستم‌های پزشکی تعریف شده‌اند
    /// </summary>
    public enum ErrorCategory
    {
        [Description("خطای عمومی")]
        General = 0,

        [Description("خطای اعتبارسنجی")]
        Validation = 1,

        [Description("مورد یافت نشد")]
        NotFound = 2,

        [Description("عدم دسترسی")]
        Unauthorized = 3,

        [Description("خطای منطق کسب‌وکار")]
        BusinessLogic = 4,

        [Description("خطای پایگاه داده")]
        Database = 5,

        [Description("خطای امنیتی")]
        Security = 6,

        [Description("خطای ارتباطی")]
        Communication = 7,

        [Description("خطای پزشکی")]
        Medical = 8,

        [Description("خطای مالی")]
        Financial = 9
    }

    /// <summary>
    /// وضعیت عملیات برای سیستم‌های پزشکی
    /// این وضعیت‌ها بر اساس استانداردهای سیستم‌های پزشکی تعریف شده‌اند
    /// </summary>
    public enum OperationStatus
    {
        [Description("در انتظار")]
        Pending = 0,

        [Description("در حال انجام")]
        InProgress = 1,

        [Description("تکمیل شده")]
        Completed = 2,

        [Description("ناموفق")]
        Failed = 3,

        [Description("لغو شده")]
        Canceled = 4,

        [Description("معلق")]
        Suspended = 5
    }

    /// <summary>
    /// کلاس حرفه‌ای برای مدیریت نتایج سرویس‌ها در سیستم‌های پزشکی
    /// این کلاس با توجه به استانداردهای سیستم‌های پزشکی طراحی شده و:
    /// 
    /// 1. کاملاً سازگار با سیستم پسورد‌لس و OTP
    /// 2. پشتیبانی کامل از محیط‌های وب و غیر-وب
    /// 3. رعایت اصول امنیتی سیستم‌های پزشکی
    /// 4. قابلیت تست‌پذیری بالا
    /// 5. مدیریت خطاها و لاگ‌گیری حرفه‌ای
    /// 6. پشتیبانی از سیستم حذف نرم و ردیابی
    /// 
    /// استفاده:
    /// var result = ServiceResult.Successful("عملیات با موفقیت انجام شد.");
    /// var patientResult = ServiceResult<Patient>.Successful(patient);
    /// 
    /// نکته حیاتی: این کلاس برای سیستم‌های پزشکی طراحی شده و تمام نیازهای خاص را پوشش می‌دهد
    /// </summary>
    public class ServiceResult
    {
        #region Properties

        public bool Success { get; protected set; }
        public string Message { get; protected set; }
        public string Code { get; protected set; }
        public SecurityLevel SecurityLevel { get; protected set; }
        public ErrorCategory Category { get; protected set; }
        public DateTime Timestamp { get; protected set; }
        public string TimestampShamsi { get; protected set; }
        public Dictionary<string, object> Metadata { get; protected set; }
        public string OperationId { get; protected set; }
        public string UserId { get; protected set; }
        public string UserName { get; protected set; }
        public string UserFullName { get; protected set; }
        public string UserRole { get; protected set; }
        public string SystemName { get; protected set; }
        public string OperationName { get; protected set; }
        public OperationStatus Status { get; protected set; }
        public List<ValidationError> ValidationErrors { get; protected set; }

        #endregion

        #region Constructor

        protected ServiceResult()
        {
            Timestamp = DateTime.UtcNow;
            TimestampShamsi = Timestamp.ToPersianDateTime();
            Metadata = new Dictionary<string, object>();
            ValidationErrors = new List<ValidationError>();
            OperationId = Guid.NewGuid().ToString();
            Status = OperationStatus.Pending;
            SecurityLevel = SecurityLevel.Medium;
            Category = ErrorCategory.General;
            SystemName = "کلینیک شفا";
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// ایجاد نتیجه موفق
        /// </summary>
        public static ServiceResult Successful(
            string message = "عملیات با موفقیت انجام شد.",
            string operationName = null,
            string userId = null,
            string userName = null,
            string userFullName = null,
            string userRole = null)
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
                Status = OperationStatus.Completed,
                SecurityLevel = SecurityLevel.Low
            };
        }

        /// <summary>
        /// ایجاد نتیجه ناموفق
        /// </summary>
        public static ServiceResult Failed(
            string message,
            string code = "GENERAL_ERROR",
            ErrorCategory category = ErrorCategory.General,
            SecurityLevel securityLevel = SecurityLevel.Medium)
        {
            var result = new ServiceResult
            {
                Success = false,
                Message = message,
                Code = code,
                Category = category,
                SecurityLevel = securityLevel,
                Status = OperationStatus.Failed
            };

            // لاگ‌گیری خطا
            LogError(result);
            return result;
        }

        /// <summary>
        /// ایجاد نتیجه ناموفق با خطاهای اعتبارسنجی
        /// </summary>
        public static ServiceResult FailedWithValidationErrors(
            string message,
            IEnumerable<ValidationError> validationErrors,
            string code = "VALIDATION_ERROR")
        {
            var result = new ServiceResult
            {
                Success = false,
                Message = message,
                Code = code,
                Category = ErrorCategory.Validation,
                SecurityLevel = SecurityLevel.Low,
                Status = OperationStatus.Failed
            };

            result.ValidationErrors.AddRange(validationErrors);
            result.Metadata["ValidationErrorsCount"] = validationErrors.Count();

            // لاگ‌گیری خطاهای اعتبارسنجی
            LogValidationErrors(result);
            return result;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// افزودن متادیتای جدید
        /// </summary>
        public ServiceResult WithMetadata(string key, object value)
        {
            Metadata[key] = value;
            return this;
        }

        /// <summary>
        /// افزودن خطای اعتبارسنجی
        /// </summary>
        public ServiceResult WithValidationError(string field, string errorMessage)
        {
            ValidationErrors.Add(new ValidationError(field, errorMessage));
            return this;
        }

        /// <summary>
        /// افزودن چندین خطا به صورت دسته‌ای
        /// </summary>
        public ServiceResult WithValidationErrors(IEnumerable<ValidationError> errors)
        {
            ValidationErrors.AddRange(errors);
            return this;
        }

        /// <summary>
        /// تنظیم سطح امنیتی
        /// </summary>
        public ServiceResult WithSecurityLevel(SecurityLevel securityLevel)
        {
            SecurityLevel = securityLevel;
            return this;
        }

        /// <summary>
        /// تنظیم دسته‌بندی خطا
        /// </summary>
        public ServiceResult WithCategory(ErrorCategory category)
        {
            Category = category;
            return this;
        }

        #endregion

        #region Logging Methods

        /// <summary>
        /// لاگ‌گیری خطا
        /// </summary>
        public static void LogError(ServiceResult result)
        {
            var logMessage = $"خطا در {result.OperationName ?? "عملیات نامشخص"}: {result.Message} (کد: {result.Code})";

            switch (result.SecurityLevel)
            {
                case SecurityLevel.Low:
                    Log.Warning(logMessage);
                    break;
                case SecurityLevel.Medium:
                    Log.Error(logMessage);
                    break;
                case SecurityLevel.High:
                    Log.Error("خطای حساس: {LogMessage}", logMessage);
                    break;
                case SecurityLevel.Critical:
                    Log.Fatal("خطای بحرانی: {LogMessage}", logMessage);
                    break;
            }
        }

        /// <summary>
        /// لاگ‌گیری خطاهای اعتبارسنجی
        /// </summary>
        public static void LogValidationErrors(ServiceResult result)
        {
            if (result.ValidationErrors.Count == 0)
                return;

            var errors = string.Join(", ", result.ValidationErrors.Select(e => $"{e.Field}: {e.ErrorMessage}"));
            Log.Warning("خطاهای اعتبارسنجی در {Operation}: {Errors}",
                result.OperationName ?? "عملیات نامشخص", errors);
        }

        #endregion
    }

    /// <summary>
    /// کلاس حرفه‌ای برای مدیریت نتایج سرویس‌ها با داده در سیستم‌های پزشکی
    /// </summary>
    public class ServiceResult<T> : ServiceResult
    {
        #region Properties

        public T Data { get; private set; }
        public int? TotalCount { get; private set; }
        public int? PageNumber { get; private set; }
        public int? PageSize { get; private set; }
        public bool HasNextPage { get; private set; }
        public bool HasPreviousPage { get; private set; }
        public string PatientId { get; private set; }
        public string PatientNationalCode { get; private set; }
        public string PatientFullName { get; private set; }
        public string DoctorId { get; private set; }
        public string DoctorFullName { get; private set; }
        public string InsuranceId { get; private set; }
        public string InsuranceName { get; private set; }
        public string AppointmentId { get; private set; }
        public string ReceptionId { get; private set; }

        #endregion

        #region Constructor

        private ServiceResult() : base() { }

        #endregion

        #region Factory Methods

        /// <summary>
        /// ایجاد نتیجه موفق با داده
        /// </summary>
        public static ServiceResult<T> Successful(
            T data,
            string message = "عملیات با موفقیت انجام شد.",
            string operationName = null,
            string userId = null,
            string userName = null,
            string userFullName = null,
            string userRole = null)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Status = OperationStatus.Completed,
                Code = "SUCCESS",
                OperationName = operationName,
                UserId = userId,
                UserName = userName,
                UserFullName = userFullName,
                UserRole = userRole,
                SecurityLevel = SecurityLevel.Low
            };
        }

        /// <summary>
        /// ایجاد نتیجه موفق با داده و اطلاعات پزشکی
        /// </summary>
        public static ServiceResult<T> SuccessfulWithMedicalInfo(
            T data,
            string patientId,
            string patientNationalCode,
            string patientFullName,
            string doctorId = null,
            string doctorFullName = null,
            string insuranceId = null,
            string insuranceName = null,
            string appointmentId = null,
            string receptionId = null,
            string message = "عملیات با موفقیت انجام شد.")
        {
            return new ServiceResult<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Status = OperationStatus.Completed,
                Code = "SUCCESS",
                SecurityLevel = SecurityLevel.Low,
                PatientId = patientId,
                PatientNationalCode = patientNationalCode,
                PatientFullName = patientFullName,
                DoctorId = doctorId,
                DoctorFullName = doctorFullName,
                InsuranceId = insuranceId,
                InsuranceName = insuranceName,
                AppointmentId = appointmentId,
                ReceptionId = receptionId
            };
        }

        /// <summary>
        /// ایجاد نتیجه ناموفق
        /// </summary>
        public static new ServiceResult<T> Failed(
            string message,
            string code = "GENERAL_ERROR",
            ErrorCategory category = ErrorCategory.General,
            SecurityLevel securityLevel = SecurityLevel.Medium)
        {
            var result = new ServiceResult<T>
            {
                Success = false,
                Message = message,
                Code = code,
                Category = category,
                SecurityLevel = securityLevel,
                Data = default(T),
                Status = OperationStatus.Failed
            };

            // لاگ‌گیری خطا
            LogError(result);
            return result;
        }

        /// <summary>
        /// ایجاد نتیجه ناموفق با خطاهای اعتبارسنجی
        /// </summary>
        public static ServiceResult<T> FailedWithValidationErrors(
            string message,
            IEnumerable<ValidationError> validationErrors,
            string code = "VALIDATION_ERROR")
        {
            var result = new ServiceResult<T>
            {
                Success = false,
                Message = message,
                Code = code,
                Category = ErrorCategory.Validation,
                SecurityLevel = SecurityLevel.Low,
                Status = OperationStatus.Failed
            };

            result.ValidationErrors.AddRange(validationErrors);
            result.Metadata["ValidationErrorsCount"] = validationErrors.Count();

            // لاگ‌گیری خطاهای اعتبارسنجی
            LogValidationErrors(result);
            return result;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// افزودن اطلاعات پزشکی
        /// </summary>
        public ServiceResult<T> WithMedicalInfo(
            string patientId,
            string patientNationalCode,
            string patientFullName,
            string doctorId = null,
            string doctorFullName = null,
            string insuranceId = null,
            string insuranceName = null,
            string appointmentId = null,
            string receptionId = null)
        {
            PatientId = patientId;
            PatientNationalCode = patientNationalCode;
            PatientFullName = patientFullName;
            DoctorId = doctorId;
            DoctorFullName = doctorFullName;
            InsuranceId = insuranceId;
            InsuranceName = insuranceName;
            AppointmentId = appointmentId;
            ReceptionId = receptionId;
            return this;
        }

        /// <summary>
        /// افزودن اطلاعات صفحه‌بندی
        /// </summary>
        public ServiceResult<T> WithPagination(int totalCount, int pageNumber, int pageSize)
        {
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            CalculatePagination();
            return this;
        }

        #endregion

        #region Private Methods

        private void CalculatePagination()
        {
            if (PageNumber.HasValue && PageSize.HasValue && PageSize > 0 && TotalCount.HasValue)
            {
                var totalPages = (int)Math.Ceiling((double)TotalCount.Value / PageSize.Value);
                HasNextPage = PageNumber < totalPages;
                HasPreviousPage = PageNumber > 1;
            }
        }

        #endregion
    }

    /// <summary>
    /// کلاس خطاها برای اعتبارسنجی
    /// </summary>
    public class ValidationError
    {
        public string Field { get; }
        public string ErrorMessage { get; }

        public ValidationError(string field, string errorMessage)
        {
            Field = field;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// کلاس کمکی برای ایجاد ServiceResult
    /// </summary>
    public static class ServiceResultFactory
    {
        #region General Results

        public static ServiceResult Success(string message = "عملیات با موفقیت انجام شد.")
        {
            return ServiceResult.Successful(message);
        }

        public static ServiceResult Error(string message, string code = "ERROR")
        {
            return ServiceResult.Failed(message, code);
        }

        public static ServiceResult NotFound(string entityName, string identifier)
        {
            return ServiceResult.Failed(
                $"مورد مورد نظر ({entityName}) با شناسه {identifier} یافت نشد.",
                "NOT_FOUND",
                ErrorCategory.NotFound);
        }

        public static ServiceResult Unauthorized(string message = "دسترسی غیرمجاز")
        {
            return ServiceResult.Failed(
                message,
                "UNAUTHORIZED",
                ErrorCategory.Unauthorized,
                SecurityLevel.High);
        }

        public static ServiceResult ValidationErrors(IEnumerable<ValidationError> errors)
        {
            return ServiceResult.FailedWithValidationErrors(
                "خطاهای اعتبارسنجی رخ داده است.",
                errors);
        }

        #endregion

        #region Medical Results

        public static ServiceResult<T> MedicalSuccess<T>(T data, string message = "عملیات پزشکی با موفقیت انجام شد.")
        {
            return ServiceResult<T>.Successful(data, message);
        }

        public static ServiceResult<T> MedicalSuccessWithInfo<T>(
            T data,
            string patientId,
            string patientNationalCode,
            string patientFullName,
            string doctorId = null,
            string doctorFullName = null,
            string insuranceId = null,
            string insuranceName = null,
            string appointmentId = null,
            string receptionId = null)
        {
            return ServiceResult<T>.SuccessfulWithMedicalInfo(
                data,
                patientId,
                patientNationalCode,
                patientFullName,
                doctorId,
                doctorFullName,
                insuranceId,
                insuranceName,
                appointmentId,
                receptionId);
        }

        public static ServiceResult MedicalError(string message, string code = "MEDICAL_ERROR")
        {
            return ServiceResult.Failed(
                message,
                code,
                ErrorCategory.Medical,
                SecurityLevel.High);
        }

        public static ServiceResult FinancialError(string message, string code = "FINANCIAL_ERROR")
        {
            return ServiceResult.Failed(
                message,
                code,
                ErrorCategory.Financial,
                SecurityLevel.Critical);
        }

        #endregion
    }
}