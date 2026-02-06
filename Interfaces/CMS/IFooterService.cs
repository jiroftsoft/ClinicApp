using System.Threading.Tasks;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.CMS;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// سرویس فوتر برای خواندن داده‌های عمومی و مدیریت از CMS
    /// </summary>
    public interface IFooterService
    {
        /// <summary>
        /// دریافت داده‌های فوتر برای نمایش در سایت.
        /// اگر رکورد FooterSettings در دیتابیس نباشد، null برمی‌گرداند (fallback به منطق قبلی در HomePageService).
        /// </summary>
        Task<FooterViewModel> GetPublicFooterAsync(int? clinicId = null);

        /// <summary>
        /// دریافت تنظیمات فوتر برای ویرایش در پنل ادمین
        /// </summary>
        Task<ServiceResult<FooterSettingsEditViewModel>> GetSettingsForEditAsync(int? clinicId = null);

        /// <summary>
        /// ذخیره تنظیمات فوتر از پنل ادمین (ایجاد/ویرایش)
        /// </summary>
        Task<ServiceResult> SaveSettingsAsync(FooterSettingsEditViewModel model, int? clinicId = null);
    }
}
