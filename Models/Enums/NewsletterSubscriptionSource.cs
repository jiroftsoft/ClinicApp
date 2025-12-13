using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// منبع ثبت‌نام در خبرنامه
    /// </summary>
    public enum NewsletterSubscriptionSource : byte
    {
        /// <summary>
        /// ثبت‌نام از سایت
        /// </summary>
        [Description("سایت")]
        [Display(Name = "سایت")]
        Website = 1,

        /// <summary>
        /// ثبت‌نام توسط ادمین
        /// </summary>
        [Description("ادمین")]
        [Display(Name = "ادمین")]
        Admin = 2,

        /// <summary>
        /// وارد کردن دستی (Import)
        /// </summary>
        [Description("وارد کردن دستی")]
        [Display(Name = "وارد کردن دستی")]
        Import = 3,

        /// <summary>
        /// ثبت‌نام از طریق API
        /// </summary>
        [Description("API")]
        [Display(Name = "API")]
        Api = 4
    }
}

