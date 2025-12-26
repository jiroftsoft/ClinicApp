using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// نوع دپارتمان در سیستم درمانی
    /// این Enum برای دسته‌بندی دپارتمان‌ها بر اساس نوع فعالیت آن‌ها استفاده می‌شود
    /// 
    /// 🏥 MEDICAL ENVIRONMENT:
    /// - برای فیلتر کردن دپارتمان‌ها در فرم پذیرش
    /// - برای گزارش‌گیری بر اساس نوع دپارتمان
    /// - برای مدیریت دسترسی‌های مختلف
    /// 
    /// 📋 استفاده:
    /// - درمانی: دپارتمان‌های ارائه‌دهنده خدمات پزشکی مستقیم
    /// - اداری: دپارتمان‌های پشتیبانی و مدیریت
    /// - پذیرش و ترخیص: بخش‌های ورود و خروج بیمار
    /// - پاراکلینیک: آزمایشگاه، رادیولوژی، و...
    /// - اورژانس: بخش فوریت‌های پزشکی
    /// - تزریقات: بخش تزریقات و درمان‌های کوتاه‌مدت
    /// </summary>
    public enum DepartmentType : byte
    {
        /// <summary>
        /// دپارتمان درمانی - ارائه‌دهنده خدمات پزشکی مستقیم
        /// مثال: دندانپزشکی، چشم‌پزشکی، ارتوپدی، اورولوژی
        /// 
        /// ویژگی‌ها:
        /// - دارای پزشک متخصص
        /// - ارائه خدمات تخصصی
        /// - نیاز به تجهیزات پزشکی
        /// - قابل نمایش در فرم پذیرش: ✅ بله
        /// </summary>
        [Display(Name = "درمانی", Description = "دپارتمان ارائه‌دهنده خدمات پزشکی مستقیم")]
        [Description("دپارتمان درمانی - ارائه خدمات تخصصی پزشکی")]
        Medical = 1,

        /// <summary>
        /// دپارتمان اداری - پشتیبانی و مدیریت
        /// مثال: امور مالی، منابع انسانی، IT، مدیریت
        /// 
        /// ویژگی‌ها:
        /// - بدون ارائه خدمات پزشکی
        /// - پشتیبانی از عملیات کلی
        /// - عدم نیاز به پزشک
        /// - قابل نمایش در فرم پذیرش: ❌ خیر
        /// </summary>
        [Display(Name = "اداری", Description = "دپارتمان پشتیبانی و مدیریت")]
        [Description("دپارتمان اداری - امور پشتیبانی و مدیریتی")]
        Administrative = 2,

        /// <summary>
        /// پذیرش و ترخیص - ورود و خروج بیمار
        /// مثال: پذیرش، ترخیص، اتاق بستری
        /// 
        /// ویژگی‌ها:
        /// - ثبت اطلاعات بیمار
        /// - مدیریت ورود/خروج
        /// - هماهنگی با دپارتمان‌های دیگر
        /// - قابل نمایش در فرم پذیرش: ⚠️ بستگی به پیکربندی
        /// </summary>
        [Display(Name = "پذیرش و ترخیص", Description = "بخش ورود و خروج بیمار")]
        [Description("پذیرش و ترخیص - مدیریت ورود و خروج بیماران")]
        AdmissionDischarge = 3,

        /// <summary>
        /// پاراکلینیک - خدمات تشخیصی
        /// مثال: آزمایشگاه، رادیولوژی، سونوگرافی، MRI، CT Scan
        /// 
        /// ویژگی‌ها:
        /// - ارائه خدمات تشخیصی
        /// - پشتیبانی از تشخیص پزشکی
        /// - نیاز به تجهیزات تخصصی
        /// - قابل نمایش در فرم پذیرش: ✅ بله
        /// </summary>
        [Display(Name = "پاراکلینیک", Description = "خدمات تشخیصی و آزمایشگاهی")]
        [Description("پاراکلینیک - خدمات تشخیصی و تصویربرداری")]
        Paraclinical = 4,

        /// <summary>
        /// اورژانس - فوریت‌های پزشکی
        /// مثال: اورژانس، ICU، CCU
        /// 
        /// ویژگی‌ها:
        /// - فعالیت 24/7
        /// - ارائه خدمات فوری
        /// - اولویت بالا
        /// - قابل نمایش در فرم پذیرش: ✅ بله (حیاتی)
        /// </summary>
        [Display(Name = "اورژانس", Description = "بخش فوریت‌های پزشکی")]
        [Description("اورژانس - خدمات فوری پزشکی 24/7")]
        Emergency = 5,

        /// <summary>
        /// تزریقات - درمان‌های کوتاه‌مدت
        /// مثال: تزریقات، سرم‌درمانی، واکسیناسیون
        /// 
        /// ویژگی‌ها:
        /// - درمان‌های کوتاه‌مدت
        /// - بدون نیاز به بستری
        /// - سرویس‌دهی سریع
        /// - قابل نمایش در فرم پذیرش: ✅ بله
        /// </summary>
        [Display(Name = "تزریقات", Description = "بخش تزریقات و درمان‌های کوتاه‌مدت")]
        [Description("تزریقات - درمان‌های تزریقی و سرم‌درمانی")]
        Injection = 6,

        /// <summary>
        /// جراحی - عمل‌های جراحی
        /// مثال: اتاق عمل، جراحی عمومی، جراحی تخصصی
        /// 
        /// ویژگی‌ها:
        /// - عمل‌های جراحی
        /// - نیاز به تجهیزات پیشرفته
        /// - تیم جراحی تخصصی
        /// - قابل نمایش در فرم پذیرش: ✅ بله
        /// </summary>
        [Display(Name = "جراحی", Description = "بخش عمل‌های جراحی")]
        [Description("جراحی - عمل‌های جراحی و اتاق عمل")]
        Surgery = 7,

        /// <summary>
        /// بستری - بخش‌های بستری بیماران
        /// مثال: بخش داخلی، بخش جراحی، بخش اطفال
        /// 
        /// ویژگی‌ها:
        /// - بستری چند روزه
        /// - مراقبت‌های مستمر
        /// - تیم پرستاری
        /// - قابل نمایش در فرم پذیرش: ⚠️ بستگی به نوع پذیرش
        /// </summary>
        [Display(Name = "بستری", Description = "بخش‌های بستری بیماران")]
        [Description("بستری - مراقبت‌های بستری چند روزه")]
        Inpatient = 8,

        /// <summary>
        /// توانبخشی - خدمات بازتوانی
        /// مثال: فیزیوتراپی، کاردرمانی، گفتاردرمانی
        /// 
        /// ویژگی‌ها:
        /// - خدمات بازتوانی
        /// - درمان‌های طولانی‌مدت
        /// - برنامه‌های تخصصی
        /// - قابل نمایش در فرم پذیرش: ✅ بله
        /// </summary>
        [Display(Name = "توانبخشی", Description = "خدمات بازتوانی و فیزیوتراپی")]
        [Description("توانبخشی - خدمات بازتوانی و درمانی")]
        Rehabilitation = 9,

        /// <summary>
        /// دارویی - داروخانه و خدمات دارویی
        /// مثال: داروخانه، انبار دارو، ترخیص دارو
        /// 
        /// ویژگی‌ها:
        /// - تهیه و توزیع دارو
        /// - مشاوره دارویی
        /// - کنترل کیفیت
        /// - قابل نمایش در فرم پذیرش: ⚠️ معمولاً خیر
        /// </summary>
        [Display(Name = "دارویی", Description = "داروخانه و خدمات دارویی")]
        [Description("دارویی - داروخانه و مشاوره دارویی")]
        Pharmacy = 10,

        /// <summary>
        /// سایر - دپارتمان‌های خاص یا تعریف نشده
        /// برای دپارتمان‌هایی که در دسته‌بندی‌های بالا قرار نمی‌گیرند
        /// 
        /// ویژگی‌ها:
        /// - دپارتمان‌های خاص
        /// - نیاز به تعریف دستی
        /// - قابل نمایش در فرم پذیرش: ⚠️ بستگی به تنظیمات
        /// </summary>
        [Display(Name = "سایر", Description = "دپارتمان‌های خاص یا تعریف نشده")]
        [Description("سایر - دپارتمان‌های خاص")]
        Other = 99
    }

    /// <summary>
    /// Extension Methods برای DepartmentType
    /// </summary>
    public static class DepartmentTypeExtensions
    {
        /// <summary>
        /// آیا این دپارتمان در فرم پذیرش نمایش داده شود؟
        /// </summary>
        /// <param name="type">نوع دپارتمان</param>
        /// <returns>true اگر باید در فرم پذیرش نمایش داده شود</returns>
        public static bool ShouldShowInReception(this DepartmentType type)
        {
            return type switch
            {
                DepartmentType.Medical => true,          // ✅ درمانی
                DepartmentType.Paraclinical => true,    // ✅ پاراکلینیک
                DepartmentType.Emergency => true,        // ✅ اورژانس
                DepartmentType.Injection => true,        // ✅ تزریقات
                DepartmentType.Surgery => true,          // ✅ جراحی
                DepartmentType.Rehabilitation => true,   // ✅ توانبخشی
                DepartmentType.Administrative => false,  // ❌ اداری
                DepartmentType.AdmissionDischarge => false, // ❌ پذیرش (خود فرم است!)
                DepartmentType.Inpatient => false,       // ⚠️ بستری (معمولاً از طریق دیگری)
                DepartmentType.Pharmacy => false,        // ❌ دارویی
                DepartmentType.Other => false,           // ⚠️ سایر (نیاز به بررسی)
                _ => false
            };
        }

        /// <summary>
        /// آیا این دپارتمان نیاز به پزشک دارد؟
        /// </summary>
        public static bool RequiresDoctor(this DepartmentType type)
        {
            return type switch
            {
                DepartmentType.Medical => true,
                DepartmentType.Emergency => true,
                DepartmentType.Surgery => true,
                DepartmentType.Inpatient => true,
                _ => false
            };
        }

        /// <summary>
        /// آیا این دپارتمان خدمات درمانی ارائه می‌دهد؟
        /// </summary>
        public static bool ProvidesMedicalServices(this DepartmentType type)
        {
            return type switch
            {
                DepartmentType.Medical => true,
                DepartmentType.Paraclinical => true,
                DepartmentType.Emergency => true,
                DepartmentType.Injection => true,
                DepartmentType.Surgery => true,
                DepartmentType.Rehabilitation => true,
                _ => false
            };
        }

        /// <summary>
        /// رنگ Badge برای UI
        /// </summary>
        public static string GetBadgeColor(this DepartmentType type)
        {
            return type switch
            {
                DepartmentType.Medical => "primary",
                DepartmentType.Administrative => "secondary",
                DepartmentType.AdmissionDischarge => "info",
                DepartmentType.Paraclinical => "success",
                DepartmentType.Emergency => "danger",
                DepartmentType.Injection => "warning",
                DepartmentType.Surgery => "danger",
                DepartmentType.Inpatient => "info",
                DepartmentType.Rehabilitation => "success",
                DepartmentType.Pharmacy => "warning",
                DepartmentType.Other => "secondary",
                _ => "secondary"
            };
        }

        /// <summary>
        /// آیکون برای UI
        /// </summary>
        public static string GetIcon(this DepartmentType type)
        {
            return type switch
            {
                DepartmentType.Medical => "fa-stethoscope",
                DepartmentType.Administrative => "fa-briefcase",
                DepartmentType.AdmissionDischarge => "fa-clipboard-check",
                DepartmentType.Paraclinical => "fa-microscope",
                DepartmentType.Emergency => "fa-ambulance",
                DepartmentType.Injection => "fa-syringe",
                DepartmentType.Surgery => "fa-user-md",
                DepartmentType.Inpatient => "fa-bed",
                DepartmentType.Rehabilitation => "fa-heartbeat",
                DepartmentType.Pharmacy => "fa-pills",
                DepartmentType.Other => "fa-ellipsis-h",
                _ => "fa-question"
            };
        }
    }
}

