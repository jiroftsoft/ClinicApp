using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// دسته‌بندی مطالب آموزشی بیماران
    /// </summary>
    public enum PatientEducationCategory : byte
    {
        /// <summary>
        /// پیشگیری
        /// </summary>
        [Description("پیشگیری")]
        [Display(Name = "پیشگیری")]
        Prevention = 1,

        /// <summary>
        /// تغذیه
        /// </summary>
        [Description("تغذیه")]
        [Display(Name = "تغذیه")]
        Nutrition = 2,

        /// <summary>
        /// ورزش و فعالیت بدنی
        /// </summary>
        [Description("ورزش و فعالیت بدنی")]
        [Display(Name = "ورزش و فعالیت بدنی")]
        Exercise = 3,

        /// <summary>
        /// بیماری‌ها
        /// </summary>
        [Description("بیماری‌ها")]
        [Display(Name = "بیماری‌ها")]
        Diseases = 4,

        /// <summary>
        /// داروها
        /// </summary>
        [Description("داروها")]
        [Display(Name = "داروها")]
        Medications = 5,

        /// <summary>
        /// مراقبت‌های بعد از عمل
        /// </summary>
        [Description("مراقبت‌های بعد از عمل")]
        [Display(Name = "مراقبت‌های بعد از عمل")]
        PostOperative = 6,

        /// <summary>
        /// عمومی
        /// </summary>
        [Description("عمومی")]
        [Display(Name = "عمومی")]
        General = 7
    }
}

