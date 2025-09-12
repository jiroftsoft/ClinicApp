using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// Base Controller برای عملیات مشترک تمام کنترلرها
    /// طبق AI_COMPLIANCE_CONTRACT: قانون 23 - پرهیز از پیچیدگی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت JSON responses استاندارد
    /// 2. مدیریت خطاهای ModelState
    /// 3. مدیریت خطاهای Validation
    /// 4. مدیریت خطاهای Service
    /// 5. مدیریت خطاهای Exception
    /// 6. Logging یکپارچه
    /// </summary>
    public abstract class BaseController : Controller
    {
        protected readonly ILogger _logger;

        protected BaseController(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// ایجاد JSON response استاندارد
        /// طبق AI_COMPLIANCE_CONTRACT: قانون 35 - AJAX Actions
        /// ساختار: { success, message, data, errors }
        /// </summary>
        /// <param name="success">وضعیت موفقیت</param>
        /// <param name="message">پیام اصلی</param>
        /// <param name="data">داده‌های بازگشتی</param>
        /// <param name="errors">لیست خطاها</param>
        /// <returns>JsonResult استاندارد</returns>
        protected JsonResult StandardJsonResponse(bool success, string message, object data = null, List<string> errors = null)
        {
            var response = new
            {
                success = success,
                message = message,
                data = data,
                errors = errors
            };

            return Json(response);
        }

        /// <summary>
        /// مدیریت خطاهای ModelState
        /// طبق AI_COMPLIANCE_CONTRACT: قانون 33 - Validation قبل از سرویس
        /// </summary>
        /// <returns>JsonResult با خطاهای ModelState</returns>
        protected JsonResult HandleModelStateErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return StandardJsonResponse(false, "اطلاعات وارد شده نامعتبر است.", null, errors);
        }

        /// <summary>
        /// مدیریت خطاهای Validation
        /// </summary>
        /// <param name="validationErrors">خطاهای اعتبارسنجی</param>
        /// <returns>JsonResult با خطاهای Validation</returns>
        protected JsonResult HandleValidationErrors(IEnumerable<string> validationErrors)
        {
            return StandardJsonResponse(false, "اطلاعات وارد شده نامعتبر است.", null, validationErrors.ToList());
        }

        /// <summary>
        /// مدیریت خطاهای Service
        /// </summary>
        /// <param name="serviceResult">نتیجه سرویس</param>
        /// <returns>JsonResult با خطای سرویس</returns>
        protected JsonResult HandleServiceError(ServiceResult serviceResult)
        {
            return StandardJsonResponse(false, serviceResult.Message);
        }

        /// <summary>
        /// مدیریت خطاهای Exception
        /// طبق AI_COMPLIANCE_CONTRACT: قانون 30 - Error Handling
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="operation">نام عملیات</param>
        /// <param name="userInfo">اطلاعات کاربر</param>
        /// <returns>JsonResult با خطای Exception</returns>
        protected JsonResult HandleException(Exception ex, string operation, string userInfo = null)
        {
            _logger.Error(ex, "خطا در {Operation}. کاربر: {UserInfo}", operation, userInfo);
            return StandardJsonResponse(false, $"خطا در {operation}. لطفاً مجدداً تلاش کنید.");
        }

        /// <summary>
        /// بررسی اعتبار ورودی‌های اجباری
        /// </summary>
        /// <param name="value">مقدار ورودی</param>
        /// <param name="fieldName">نام فیلد</param>
        /// <returns>true اگر معتبر باشد</returns>
        protected bool ValidateRequiredField(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ModelState.AddModelError(fieldName, $"{fieldName} الزامی است.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// بررسی اعتبار شناسه‌های عددی
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <param name="fieldName">نام فیلد</param>
        /// <returns>true اگر معتبر باشد</returns>
        protected bool ValidateId(int id, string fieldName)
        {
            if (id <= 0)
            {
                ModelState.AddModelError(fieldName, $"{fieldName} نامعتبر است.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// ایجاد JSON response موفق
        /// </summary>
        /// <param name="data">داده‌های بازگشتی</param>
        /// <param name="message">پیام موفقیت</param>
        /// <returns>JsonResult موفق</returns>
        protected JsonResult SuccessResponse(object data = null, string message = "عملیات با موفقیت انجام شد.")
        {
            return StandardJsonResponse(true, message, data);
        }

        /// <summary>
        /// ایجاد JSON response ناموفق
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="errors">خطاهای اضافی</param>
        /// <returns>JsonResult ناموفق</returns>
        protected JsonResult ErrorResponse(string message, List<string> errors = null)
        {
            return StandardJsonResponse(false, message, null, errors);
        }
    }
}
