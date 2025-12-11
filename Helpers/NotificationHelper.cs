using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper برای مدیریت پیام‌های کاربرپسند با Toaster
    /// طراحی شده بر اساس اصول SRP و Production-Ready
    /// </summary>
    public static class NotificationHelper
    {
        private const string SuccessKey = "Notification_Success";
        private const string ErrorKey = "Notification_Error";
        private const string WarningKey = "Notification_Warning";
        private const string InfoKey = "Notification_Info";

        /// <summary>
        /// نمایش پیام موفقیت
        /// </summary>
        public static void SetSuccess(TempDataDictionary tempData, string message, string title = "موفقیت")
        {
            if (tempData == null || string.IsNullOrWhiteSpace(message))
                return;

            tempData[SuccessKey] = new NotificationMessage
            {
                Message = message,
                Title = title,
                Type = NotificationType.Success
            };
        }

        /// <summary>
        /// نمایش پیام خطا
        /// </summary>
        public static void SetError(TempDataDictionary tempData, string message, string title = "خطا")
        {
            if (tempData == null || string.IsNullOrWhiteSpace(message))
                return;

            tempData[ErrorKey] = new NotificationMessage
            {
                Message = message,
                Title = title,
                Type = NotificationType.Error
            };
        }

        /// <summary>
        /// نمایش پیام هشدار
        /// </summary>
        public static void SetWarning(TempDataDictionary tempData, string message, string title = "هشدار")
        {
            if (tempData == null || string.IsNullOrWhiteSpace(message))
                return;

            tempData[WarningKey] = new NotificationMessage
            {
                Message = message,
                Title = title,
                Type = NotificationType.Warning
            };
        }

        /// <summary>
        /// نمایش پیام اطلاعات
        /// </summary>
        public static void SetInfo(TempDataDictionary tempData, string message, string title = "اطلاعات")
        {
            if (tempData == null || string.IsNullOrWhiteSpace(message))
                return;

            tempData[InfoKey] = new NotificationMessage
            {
                Message = message,
                Title = title,
                Type = NotificationType.Info
            };
        }

        /// <summary>
        /// دریافت پیام موفقیت
        /// </summary>
        public static NotificationMessage GetSuccess(TempDataDictionary tempData)
        {
            return tempData?[SuccessKey] as NotificationMessage;
        }

        /// <summary>
        /// دریافت پیام خطا
        /// </summary>
        public static NotificationMessage GetError(TempDataDictionary tempData)
        {
            return tempData?[ErrorKey] as NotificationMessage;
        }

        /// <summary>
        /// دریافت پیام هشدار
        /// </summary>
        public static NotificationMessage GetWarning(TempDataDictionary tempData)
        {
            return tempData?[WarningKey] as NotificationMessage;
        }

        /// <summary>
        /// دریافت پیام اطلاعات
        /// </summary>
        public static NotificationMessage GetInfo(TempDataDictionary tempData)
        {
            return tempData?[InfoKey] as NotificationMessage;
        }

        /// <summary>
        /// دریافت اولین پیام موجود (برای نمایش در View)
        /// </summary>
        public static NotificationMessage GetFirstAvailable(TempDataDictionary tempData)
        {
            if (tempData == null)
                return null;

            var success = GetSuccess(tempData);
            if (success != null) return success;

            var error = GetError(tempData);
            if (error != null) return error;

            var warning = GetWarning(tempData);
            if (warning != null) return warning;

            var info = GetInfo(tempData);
            if (info != null) return info;

            return null;
        }
    }

    /// <summary>
    /// نوع پیام
    /// </summary>
    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }

    /// <summary>
    /// مدل پیام اعلان
    /// </summary>
    public class NotificationMessage
    {
        public string Message { get; set; }
        public string Title { get; set; }
        public NotificationType Type { get; set; }
    }
}

