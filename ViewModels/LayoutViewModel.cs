using System.Collections.Generic;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ViewModel برای داده‌های مشترک Layout
    /// طراحی شده بر اساس اصول Strongly-Typed
    /// استفاده می‌شود برای Stories، Footer، EmergencyContacts و سایر بخش‌های مشترک
    /// </summary>
    public class LayoutViewModel
    {
        /// <summary>
        /// Stories برای نمایش زیر منو (مشابه دیجی‌کالا)
        /// </summary>
        public List<StoryPublicViewModel> Stories { get; set; } = new List<StoryPublicViewModel>();

        /// <summary>
        /// Footer برای نمایش در پایین صفحه
        /// </summary>
        public FooterViewModel Footer { get; set; }

        /// <summary>
        /// Emergency Contacts برای نمایش Sticky Bar
        /// </summary>
        public List<EmergencyContactPublicViewModel> EmergencyContacts { get; set; } = new List<EmergencyContactPublicViewModel>();
    }
}
