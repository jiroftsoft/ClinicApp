using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// نوع شیفت کاری - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت شیفت‌های کاری پزشکان
    /// 2. پشتیبانی از انتخاب خودکار شیفت
    /// 3. سازگاری با سیستم‌های پزشکی
    /// 4. رعایت استانداردهای پزشکی ایران
    /// </summary>
    public enum ShiftType : byte
    {
        /// <summary>
        /// شیفت صبح (6:00 - 14:00)
        /// </summary>
        [Display(Name = "صبح")]
        Morning = 0,

        /// <summary>
        /// شیفت عصر (14:00 - 22:00)
        /// </summary>
        [Display(Name = "عصر")]
        Evening = 1,

        /// <summary>
        /// شیفت شب (22:00 - 6:00)
        /// </summary>
        [Display(Name = "شب")]
        Night = 2
    }
}
