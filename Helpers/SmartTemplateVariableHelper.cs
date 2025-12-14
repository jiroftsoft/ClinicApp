using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Extensions;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper برای مدیریت متغیرهای پیشرفته Template
    /// طراحی شده برای سیستم Template هوشمند
    /// </summary>
    public static class SmartTemplateVariableHelper
    {
        /// <summary>
        /// ایجاد Dictionary کامل از متغیرهای پیشرفته برای Template
        /// </summary>
        public static Dictionary<string, string> BuildAdvancedVariables(
            NewsletterSubscription subscription,
            string unsubscribeUrl = null)
        {
            var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (subscription == null)
            {
                return variables;
            }

            // ========== متغیرهای کاربری ==========
            
            // FullName
            variables["FullName"] = subscription.FullName ?? "کاربر گرامی";
            
            // FirstName و LastName از FullName استخراج می‌شوند
            var nameParts = SplitFullName(subscription.FullName);
            variables["FirstName"] = nameParts.FirstName;
            variables["LastName"] = nameParts.LastName;
            
            // Email
            variables["Email"] = subscription.Email ?? string.Empty;
            
            // PhoneNumber
            variables["PhoneNumber"] = subscription.PhoneNumber ?? string.Empty;
            
            // SubscriptionDate (تاریخ عضویت)
            if (subscription.CreatedAt != null && subscription.CreatedAt != DateTime.MinValue)
            {
                variables["SubscriptionDate"] = subscription.CreatedAt.ToPersianDate();
                variables["SubscriptionDateLong"] = GetPersianDateLong(subscription.CreatedAt);
            }
            else
            {
                variables["SubscriptionDate"] = string.Empty;
                variables["SubscriptionDateLong"] = string.Empty;
            }
            
            // Category (دسته‌بندی‌های خبرنامه)
            if (!string.IsNullOrWhiteSpace(subscription.Categories))
            {
                try
                {
                    // Categories به صورت JSON Array ذخیره می‌شود
                    var categories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(subscription.Categories);
                    if (categories != null && categories.Any())
                    {
                        variables["Category"] = string.Join("، ", categories);
                        variables["Categories"] = variables["Category"];
                    }
                    else
                    {
                        variables["Category"] = "عمومی";
                        variables["Categories"] = "عمومی";
                    }
                }
                catch
                {
                    variables["Category"] = "عمومی";
                    variables["Categories"] = "عمومی";
                }
            }
            else
            {
                variables["Category"] = "عمومی";
                variables["Categories"] = "عمومی";
            }
            
            // UnsubscribeUrl
            variables["UnsubscribeUrl"] = unsubscribeUrl ?? string.Empty;

            // ========== متغیرهای سیستم ==========
            
            var now = DateTime.Now;
            
            // CurrentDate (تاریخ امروز شمسی)
            variables["CurrentDate"] = now.ToPersianDate();
            variables["CurrentDateLong"] = GetPersianDateLong(now);
            variables["CurrentDateShort"] = now.ToPersianDate();
            
            // CurrentTime (زمان فعلی)
            variables["CurrentTime"] = now.ToString("HH:mm");
            variables["CurrentTimeLong"] = now.ToString("HH:mm:ss");
            
            // CurrentDateTime (تاریخ و زمان کامل)
            variables["CurrentDateTime"] = $"{now.ToPersianDate()} {now.ToString("HH:mm")}";
            
            // ========== متغیرهای کلینیک ==========
            
            // ClinicName
            variables["ClinicName"] = GetClinicName();
            
            // ClinicPhone
            variables["ClinicPhone"] = GetClinicPhone();
            
            // ClinicAddress
            variables["ClinicAddress"] = GetClinicAddress();
            
            // ClinicEmail
            variables["ClinicEmail"] = GetClinicEmail();
            
            // ClinicWebsite
            variables["ClinicWebsite"] = GetClinicWebsite();

            return variables;
        }

        /// <summary>
        /// ایجاد تاریخ شمسی طولانی با نام روز و ماه
        /// </summary>
        private static string GetPersianDateLong(DateTime dateTime)
        {
            try
            {
                var persianCalendar = new System.Globalization.PersianCalendar();
                var year = persianCalendar.GetYear(dateTime);
                var month = persianCalendar.GetMonth(dateTime);
                var day = persianCalendar.GetDayOfMonth(dateTime);
                var dayOfWeek = dateTime.DayOfWeek;

                var dayName = PersianDateHelper.GetPersianDayOfWeekName(dayOfWeek, true);
                var monthName = PersianDateHelper.GetPersianMonthName(month, true);

                return $"{dayName}، {day} {monthName} {year}";
            }
            catch
            {
                // Fallback به فرمت ساده
                return dateTime.ToPersianDate();
            }
        }

        /// <summary>
        /// استخراج FirstName و LastName از FullName
        /// </summary>
        private static (string FirstName, string LastName) SplitFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return ("کاربر", "گرامی");
            }

            var parts = fullName.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 0)
            {
                return ("کاربر", "گرامی");
            }
            
            if (parts.Length == 1)
            {
                return (parts[0], string.Empty);
            }
            
            // در فارسی، نام خانوادگی آخرین کلمه است
            var firstName = string.Join(" ", parts.Take(parts.Length - 1));
            var lastName = parts[parts.Length - 1];
            
            return (firstName, lastName);
        }

        /// <summary>
        /// دریافت نام کلینیک از تنظیمات
        /// </summary>
        private static string GetClinicName()
        {
            return ConfigurationManager.AppSettings["Clinic:Name"] 
                ?? ConfigurationManager.AppSettings["SiteName"] 
                ?? "کلینیک شفا جیرفت";
        }

        /// <summary>
        /// دریافت شماره تلفن کلینیک از تنظیمات
        /// </summary>
        private static string GetClinicPhone()
        {
            return ConfigurationManager.AppSettings["Clinic:Phone"] 
                ?? ConfigurationManager.AppSettings["Clinic:PhoneNumber"] 
                ?? "034-32220000";
        }

        /// <summary>
        /// دریافت آدرس کلینیک از تنظیمات
        /// </summary>
        private static string GetClinicAddress()
        {
            return ConfigurationManager.AppSettings["Clinic:Address"] 
                ?? "جیرفت، خیابان امام خمینی، کلینیک شفا";
        }

        /// <summary>
        /// دریافت ایمیل کلینیک از تنظیمات
        /// </summary>
        private static string GetClinicEmail()
        {
            return ConfigurationManager.AppSettings["Clinic:Email"] 
                ?? ConfigurationManager.AppSettings["Email:FromAddress"] 
                ?? "info@clinicapp.com";
        }

        /// <summary>
        /// دریافت وب‌سایت کلینیک از تنظیمات
        /// </summary>
        private static string GetClinicWebsite()
        {
            return ConfigurationManager.AppSettings["Clinic:Website"] 
                ?? ConfigurationManager.AppSettings["SiteUrl"] 
                ?? "https://clinicapp.com";
        }

        /// <summary>
        /// دریافت لیست تمام متغیرهای موجود
        /// </summary>
        public static List<TemplateVariableInfo> GetAvailableVariables()
        {
            return new List<TemplateVariableInfo>
            {
                // متغیرهای کاربری
                new TemplateVariableInfo { Name = "FullName", Description = "نام و نام خانوادگی کامل", Category = "کاربری" },
                new TemplateVariableInfo { Name = "FirstName", Description = "نام کوچک", Category = "کاربری" },
                new TemplateVariableInfo { Name = "LastName", Description = "نام خانوادگی", Category = "کاربری" },
                new TemplateVariableInfo { Name = "Email", Description = "ایمیل", Category = "کاربری" },
                new TemplateVariableInfo { Name = "PhoneNumber", Description = "شماره تماس", Category = "کاربری" },
                new TemplateVariableInfo { Name = "SubscriptionDate", Description = "تاریخ عضویت (yyyy/MM/dd)", Category = "کاربری" },
                new TemplateVariableInfo { Name = "SubscriptionDateLong", Description = "تاریخ عضویت (طولانی)", Category = "کاربری" },
                new TemplateVariableInfo { Name = "Category", Description = "دسته‌بندی خبرنامه", Category = "کاربری" },
                new TemplateVariableInfo { Name = "Categories", Description = "دسته‌بندی‌های خبرنامه", Category = "کاربری" },
                new TemplateVariableInfo { Name = "UnsubscribeUrl", Description = "لینک لغو اشتراک", Category = "کاربری" },
                
                // متغیرهای سیستم
                new TemplateVariableInfo { Name = "CurrentDate", Description = "تاریخ امروز (yyyy/MM/dd)", Category = "سیستم" },
                new TemplateVariableInfo { Name = "CurrentDateLong", Description = "تاریخ امروز (طولانی)", Category = "سیستم" },
                new TemplateVariableInfo { Name = "CurrentTime", Description = "زمان فعلی (HH:mm)", Category = "سیستم" },
                new TemplateVariableInfo { Name = "CurrentTimeLong", Description = "زمان فعلی (HH:mm:ss)", Category = "سیستم" },
                new TemplateVariableInfo { Name = "CurrentDateTime", Description = "تاریخ و زمان کامل", Category = "سیستم" },
                
                // متغیرهای کلینیک
                new TemplateVariableInfo { Name = "ClinicName", Description = "نام کلینیک", Category = "کلینیک" },
                new TemplateVariableInfo { Name = "ClinicPhone", Description = "شماره تلفن کلینیک", Category = "کلینیک" },
                new TemplateVariableInfo { Name = "ClinicAddress", Description = "آدرس کلینیک", Category = "کلینیک" },
                new TemplateVariableInfo { Name = "ClinicEmail", Description = "ایمیل کلینیک", Category = "کلینیک" },
                new TemplateVariableInfo { Name = "ClinicWebsite", Description = "وب‌سایت کلینیک", Category = "کلینیک" }
            };
        }
    }

    /// <summary>
    /// اطلاعات متغیر Template
    /// </summary>
    public class TemplateVariableInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Example => $"{{{{{Name}}}}}";
    }
}

