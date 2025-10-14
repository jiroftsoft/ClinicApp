using System;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای اطلاعات شیفت کاری
    /// </summary>
    public class ShiftInfo
    {
        /// <summary>
        /// نوع شیفت
        /// </summary>
        public ShiftType ShiftType { get; set; }

        /// <summary>
        /// نام نمایشی شیفت
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// زمان شروع شیفت
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان شیفت
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// آیا شیفت فعال است
        /// </summary>
        public bool IsActive { get; set; }
    }
}
