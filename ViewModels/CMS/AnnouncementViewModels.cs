using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;

namespace ClinicApp.ViewModels.CMS
{
    #region Announcement Index

    public class AnnouncementIndexViewModel
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string LinkUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsImportant { get; set; }
        public int DisplayOrder { get; set; }
        public string Type { get; set; }
        public string TargetAudience { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    #endregion

    #region Announcement Create & Edit

    public class AnnouncementCreateEditViewModel
    {
        public int AnnouncementId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(300, ErrorMessage = "عنوان نمی‌تواند بیش از 300 کاراکتر باشد.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [MaxLength(2000, ErrorMessage = "محتوا نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        [Display(Name = "محتوا")]
        public string Content { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر")]
        public string ImageUrl { get; set; }

        [MaxLength(500, ErrorMessage = "لینک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "لینک")]
        public string LinkUrl { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "مهم")]
        public bool IsImportant { get; set; }

        [Required(ErrorMessage = "ترتیب نمایش الزامی است.")]
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        [MaxLength(50, ErrorMessage = "نوع نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "نوع")]
        public string Type { get; set; } // "info", "warning", "success", "error"

        [MaxLength(100, ErrorMessage = "مخاطب نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "مخاطب")]
        public string TargetAudience { get; set; } // "all", "patients", "doctors", "staff"
    }

    #endregion

    #region Announcement Details

    public class AnnouncementDetailsViewModel
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string LinkUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsImportant { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Type { get; set; }
        public string TargetAudience { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion
}

