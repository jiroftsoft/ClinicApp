using System.ComponentModel;

namespace ClinicApp.Models.Entities
{
    /// <summary>
    /// انواع نوبت‌های پزشکی
    /// </summary>
    public enum AppointmentType
    {
        /// <summary>
        /// ویزیت عادی
        /// </summary>
        [Description("ویزیت عادی")]
        Regular = 1,

        /// <summary>
        /// ویزیت فوری
        /// </summary>
        [Description("ویزیت فوری")]
        Urgent = 2,

        /// <summary>
        /// ویزیت تخصصی
        /// </summary>
        [Description("ویزیت تخصصی")]
        Specialist = 3,

        /// <summary>
        /// ویزیت پیگیری
        /// </summary>
        [Description("ویزیت پیگیری")]
        FollowUp = 4,

        /// <summary>
        /// ویزیت مشاوره
        /// </summary>
        [Description("ویزیت مشاوره")]
        Consultation = 5,

        /// <summary>
        /// ویزیت اورژانس
        /// </summary>
        [Description("ویزیت اورژانس")]
        Emergency = 6,

        /// <summary>
        /// ویزیت آنلاین
        /// </summary>
        [Description("ویزیت آنلاین")]
        Online = 7,

        /// <summary>
        /// ویزیت در منزل
        /// </summary>
        [Description("ویزیت در منزل")]
        HomeVisit = 8
    }
}
