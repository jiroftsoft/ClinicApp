using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Extension Methods برای Controller
    /// برای مدیریت تاریخ‌های شمسی در فرم‌ها و نمایش خطاهای اعتبارسنجی
    /// 
    /// اصول طراحی:
    /// - SRP: هر متد یک مسئولیت دارد
    /// - DRY: بدون تکرار کد
    /// - Logging: لاگ‌گذاری کامل
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// تبدیل تاریخ شمسی به میلادی از hidden input
        /// این متد تاریخ را از hidden input می‌خواند و به DateTime تبدیل می‌کند
        /// </summary>
        /// <param name="controller">Controller</param>
        /// <param name="fieldName">نام فیلد (مثلاً "StartDate")</param>
        /// <param name="logger">Logger برای لاگ‌گذاری (اختیاری)</param>
        /// <returns>DateTime? - تاریخ میلادی یا null</returns>
        public static DateTime? ParseDateFromHiddenInput(this Controller controller, string fieldName, Serilog.ILogger logger = null)
        {
            try
            {
                var hiddenInputName = fieldName + "_Hidden";
                var hiddenValue = controller.Request.Form[hiddenInputName];
                
                if (logger != null)
                {
                    logger.Debug("دریافت hidden input - Field: {FieldName}, HiddenName: {HiddenName}, Value: {Value}",
                        fieldName, hiddenInputName, hiddenValue);
                }

                if (string.IsNullOrEmpty(hiddenValue))
                {
                    if (logger != null)
                    {
                        logger.Debug("Hidden input خالی است - Field: {FieldName}", fieldName);
                    }
                    return null;
                }

                // TryParse با CultureInfo.InvariantCulture برای ISO format
                if (DateTime.TryParse(hiddenValue, System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    // تاریخ به صورت Unspecified است، به عنوان local در نظر می‌گیریم
                    date = DateTime.SpecifyKind(date, DateTimeKind.Local);
                    
                    // فقط تاریخ را نگه دار (بدون زمان)
                    var result = date.Date;
                    
                    if (logger != null)
                    {
                        logger.Information("✅ تاریخ از hidden input parse شد - Field: {FieldName}, Date: {Date}",
                            fieldName, result);
                    }
                    
                    return result;
                }
                else
                {
                    if (logger != null)
                    {
                        logger.Warning("⚠️ خطا در parse کردن تاریخ از hidden input - Field: {FieldName}, Value: {Value}",
                            fieldName, hiddenValue);
                    }
                    
                    // Fallback: استفاده از PersianDateHelper
                    var persianValue = controller.Request.Form[fieldName];
                    if (!string.IsNullOrEmpty(persianValue))
                    {
                        var result = PersianDateHelper.ParsePersianDate(persianValue);
                        if (logger != null)
                        {
                            logger.Information("✅ تاریخ از PersianDateHelper parse شد - Field: {FieldName}, Date: {Date}",
                                fieldName, result);
                        }
                        return result;
                    }
                    
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.Error(ex, "❌ خطا در ParseDateFromHiddenInput - Field: {FieldName}", fieldName);
                }
                return null;
            }
        }

        /// <summary>
        /// تبدیل تاریخ‌های شمسی به میلادی برای یک ViewModel
        /// این متد تمام فیلدهای تاریخ را از hidden inputs می‌خواند و تبدیل می‌کند
        /// </summary>
        /// <typeparam name="T">نوع ViewModel</typeparam>
        /// <param name="controller">Controller</param>
        /// <param name="model">ViewModel</param>
        /// <param name="dateFields">نام فیلدهای تاریخ (مثلاً ["StartDate", "EndDate"])</param>
        /// <param name="logger">Logger برای لاگ‌گذاری (اختیاری)</param>
        public static void ParseDatesFromHiddenInputs<T>(this Controller controller, T model, string[] dateFields, Serilog.ILogger logger = null) where T : class
        {
            try
            {
                if (logger != null)
                {
                    logger.Debug("شروع parse کردن تاریخ‌ها - Fields: {Fields}", string.Join(", ", dateFields));
                }

                foreach (var fieldName in dateFields)
                {
                    var dateValue = controller.ParseDateFromHiddenInput(fieldName, logger);
                    
                    // استفاده از Reflection برای تنظیم مقدار
                    var property = typeof(T).GetProperty(fieldName);
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(model, dateValue);
                        
                        if (logger != null)
                        {
                            logger.Debug("مقدار تاریخ تنظیم شد - Field: {FieldName}, Value: {Value}", fieldName, dateValue);
                        }
                    }
                    else
                    {
                        if (logger != null)
                        {
                            logger.Warning("⚠️ Property یافت نشد یا قابل نوشتن نیست - Field: {FieldName}", fieldName);
                        }
                    }
                }

                if (logger != null)
                {
                    logger.Information("✅ تمام تاریخ‌ها parse شدند");
                }
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.Error(ex, "❌ خطا در ParseDatesFromHiddenInputs");
                }
            }
        }

        /// <summary>
        /// جمع‌آوری و نمایش خطاهای ModelState با Toastr
        /// طبق قرارداد: تمام خطاهای اعتبارسنجی باید با Toastr نمایش داده شوند
        /// </summary>
        /// <param name="controller">Controller</param>
        /// <param name="logger">Logger برای لاگ‌گذاری (اختیاری)</param>
        /// <returns>تعداد خطاهای ModelState</returns>
        public static int AddModelStateErrorsToNotification(this Controller controller, ILogger logger = null)
        {
            try
            {
                if (controller.ModelState == null || !controller.ModelState.IsValid)
                {
                    var errors = controller.ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(error => 
                            string.IsNullOrEmpty(error.ErrorMessage) 
                                ? $"خطا در فیلد {x.Key}" 
                                : error.ErrorMessage))
                        .ToList();

                    if (errors.Any())
                    {
                        var errorMessage = string.Join("<br/>", errors);
                        
                        if (logger != null)
                        {
                            logger.Warning("خطاهای اعتبارسنجی ModelState - تعداد: {ErrorCount}, خطاها: {Errors}", 
                                errors.Count, string.Join(" | ", errors));
                        }

                        // نمایش خطاها با Toastr
                        NotificationHelper.SetError(controller.TempData, errorMessage, "خطاهای اعتبارسنجی");
                        
                        return errors.Count;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.Error(ex, "❌ خطا در AddModelStateErrorsToNotification");
                }
                return 0;
            }
        }
    }
}

