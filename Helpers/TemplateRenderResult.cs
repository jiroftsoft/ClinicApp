using System;
using System.Collections.Generic;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// نتیجه Render کردن Template
    /// شامل Output، خطاها و وضعیت
    /// </summary>
    public class TemplateRenderResult
    {
        /// <summary>
        /// خروجی Render شده
        /// </summary>
        public string Output { get; set; } = string.Empty;

        /// <summary>
        /// آیا خطایی رخ داده است؟
        /// </summary>
        public bool HasErrors { get; set; }

        /// <summary>
        /// لیست خطاها
        /// </summary>
        public List<TemplateError> Errors { get; set; } = new List<TemplateError>();

        /// <summary>
        /// آیا Render موفق بوده است؟
        /// </summary>
        public bool IsSuccess => !HasErrors && !string.IsNullOrEmpty(Output);

        /// <summary>
        /// ایجاد نتیجه موفق
        /// </summary>
        public static TemplateRenderResult Successful(string output)
        {
            return new TemplateRenderResult
            {
                Output = output ?? string.Empty,
                HasErrors = false,
                Errors = new List<TemplateError>()
            };
        }

        /// <summary>
        /// ایجاد نتیجه با خطا
        /// </summary>
        public static TemplateRenderResult Failed(string errorMessage, string errorCode = null)
        {
            return new TemplateRenderResult
            {
                Output = string.Empty,
                HasErrors = true,
                Errors = new List<TemplateError>
                {
                    new TemplateError
                    {
                        Message = errorMessage,
                        Code = errorCode,
                        LineNumber = null
                    }
                }
            };
        }

        /// <summary>
        /// ایجاد نتیجه با چند خطا
        /// </summary>
        public static TemplateRenderResult Failed(List<TemplateError> errors)
        {
            return new TemplateRenderResult
            {
                Output = string.Empty,
                HasErrors = true,
                Errors = errors ?? new List<TemplateError>()
            };
        }
    }

    /// <summary>
    /// خطای Template
    /// </summary>
    public class TemplateError
    {
        /// <summary>
        /// پیام خطا
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// کد خطا
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// شماره خط (در صورت وجود)
        /// </summary>
        public int? LineNumber { get; set; }

        /// <summary>
        /// نوع خطا
        /// </summary>
        public TemplateErrorType Type { get; set; } = TemplateErrorType.General;
    }

    /// <summary>
    /// نوع خطای Template
    /// </summary>
    public enum TemplateErrorType
    {
        General,
        Security,
        Performance,
        Syntax,
        MissingVariable,
        InvalidCondition,
        InvalidLoop
    }

    /// <summary>
    /// Exception امنیتی Template
    /// </summary>
    public class TemplateSecurityException : Exception
    {
        public TemplateSecurityException(string message) : base(message)
        {
        }
    }
}

