using System.Threading.Tasks;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// خواندن تنظیمات کانال (ایمیل/SMS) با اولویت DB و fallback به Web.config — برای پروداکشن.
    /// </summary>
    public interface IChannelConfigProvider
    {
        /// <summary>
        /// مقدار را بر اساس کلید کامل برمی‌گرداند؛ مثلاً "Email:FromAddress", "Asanak:Username".
        /// اول از DB، در صورت نبود از Web.config.
        /// </summary>
        string GetValue(string fullKey);

        /// <summary>
        /// نسخهٔ غیرهمگام — برای استفاده در SendAsync و غیره.
        /// </summary>
        Task<string> GetValueAsync(string fullKey);

        /// <summary>
        /// پس از ذخیره از فرم تنظیمات فراخوانی شود تا کش پاک شود.
        /// </summary>
        void InvalidateCache();
    }
}
