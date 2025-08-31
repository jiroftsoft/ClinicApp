using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل مسدود کردن بازه زمانی پزشک
    /// </summary>
    public class BlockTimeRangeViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "شناسه پزشک الزامی است")]
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        [Required(ErrorMessage = "تاریخ شروع الزامی است")]
        [Display(Name = "تاریخ شروع")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        [Required(ErrorMessage = "تاریخ پایان الزامی است")]
        [Display(Name = "تاریخ پایان")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// زمان شروع
        /// </summary>
        [Required(ErrorMessage = "زمان شروع الزامی است")]
        [Display(Name = "زمان شروع")]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان
        /// </summary>
        [Required(ErrorMessage = "زمان پایان الزامی است")]
        [Display(Name = "زمان پایان")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// دلیل مسدودیت
        /// </summary>
        [Required(ErrorMessage = "دلیل مسدودیت الزامی است")]
        [StringLength(500, ErrorMessage = "دلیل مسدودیت نمی‌تواند بیش از 500 کاراکتر باشد")]
        [Display(Name = "دلیل مسدودیت")]
        public string Reason { get; set; }

        /// <summary>
        /// آیا مسدودیت تکرار شونده است
        /// </summary>
        [Display(Name = "مسدودیت تکرار شونده")]
        public bool IsRecurring { get; set; }

        /// <summary>
        /// نوع تکرار (روزانه، هفتگی، ماهانه)
        /// </summary>
        [Display(Name = "نوع تکرار")]
        public string RecurrenceType { get; set; }

        /// <summary>
        /// تعداد تکرار
        /// </summary>
        [Range(1, 365, ErrorMessage = "تعداد تکرار باید بین 1 تا 365 باشد")]
        [Display(Name = "تعداد تکرار")]
        public int? RecurrenceCount { get; set; }
    }
}
