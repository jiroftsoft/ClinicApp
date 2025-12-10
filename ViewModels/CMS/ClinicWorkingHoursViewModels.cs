using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;

namespace ClinicApp.ViewModels.CMS
{
    #region Clinic Working Hours Index & Search

    public class ClinicWorkingHoursSearchViewModel
    {
        public int? ClinicId { get; set; }
        public int? DayOfWeek { get; set; }
        public bool? IsOpen { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ClinicWorkingHoursIndexViewModel
    {
        public int ClinicWorkingHoursId { get; set; }
        public int? ClinicId { get; set; }
        public string ClinicName { get; set; }
        public int DayOfWeek { get; set; }
        public string DayName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string TimeRange { get; set; }
        public bool IsOpen { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region Clinic Working Hours Create & Edit

    public class ClinicWorkingHoursCreateEditViewModel
    {
        public int ClinicWorkingHoursId { get; set; }

        [Display(Name = "کلینیک")]
        public int? ClinicId { get; set; }

        [Required(ErrorMessage = "روز هفته الزامی است.")]
        [Range(0, 6, ErrorMessage = "روز هفته باید بین 0 تا 6 باشد.")]
        [Display(Name = "روز هفته")]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessage = "نام روز هفته الزامی است.")]
        [MaxLength(20, ErrorMessage = "نام روز هفته نمی‌تواند بیش از 20 کاراکتر باشد.")]
        [Display(Name = "نام روز هفته")]
        public string DayName { get; set; }

        [Required(ErrorMessage = "زمان شروع الزامی است.")]
        [Display(Name = "زمان شروع")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "زمان پایان الزامی است.")]
        [Display(Name = "زمان پایان")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "باز است")]
        public bool IsOpen { get; set; } = true;

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "ترتیب نمایش الزامی است.")]
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Notes { get; set; }
    }

    #endregion

    #region Clinic Working Hours Details

    public class ClinicWorkingHoursDetailsViewModel
    {
        public int ClinicWorkingHoursId { get; set; }
        public int? ClinicId { get; set; }
        public string ClinicName { get; set; }
        public int DayOfWeek { get; set; }
        public string DayName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string TimeRange { get; set; }
        public bool IsOpen { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion

    #region Clinic Working Hours Public (برای نمایش در سایت)

    public class ClinicWorkingHoursPublicViewModel
    {
        public int ClinicWorkingHoursId { get; set; }
        public int DayOfWeek { get; set; }
        public string DayName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string TimeRange { get; set; }
        public bool IsOpen { get; set; }
        public string Notes { get; set; }
    }

    #endregion
}

