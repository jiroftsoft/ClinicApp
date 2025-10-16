using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش شیفت‌ها در لیست‌های کشویی
    /// </summary>
    public class ShiftLookupViewModel
    {
        public int ShiftId { get; set; }
        public string ShiftName { get; set; }
        public string ShiftType { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش اطلاعات شیفت
    /// </summary>
    public class ShiftInfoViewModel
    {
        public int ShiftId { get; set; }
        public string ShiftName { get; set; }
        public string ShiftType { get; set; }
        public bool IsActive { get; set; }
        public string CurrentTime { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
