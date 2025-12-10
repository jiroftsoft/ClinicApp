using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;

namespace ClinicApp.ViewModels.CMS
{
    #region Emergency Contact Index & Search

    public class EmergencyContactSearchViewModel
    {
        public string SearchTerm { get; set; }
        public string ContactType { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsAlwaysVisible { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class EmergencyContactIndexViewModel
    {
        public int EmergencyContactId { get; set; }
        public string ContactType { get; set; }
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
        public string SecondaryPhoneNumber { get; set; }
        public string Address { get; set; }
        public string IconUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsAlwaysVisible { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region Emergency Contact Create & Edit

    public class EmergencyContactCreateEditViewModel
    {
        public int EmergencyContactId { get; set; }

        [Required(ErrorMessage = "نوع تماس الزامی است.")]
        [Display(Name = "نوع تماس")]
        public string ContactType { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [Required(ErrorMessage = "شماره تماس الزامی است.")]
        [MaxLength(50, ErrorMessage = "شماره تماس نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "شماره تماس")]
        public string PhoneNumber { get; set; }

        [MaxLength(50, ErrorMessage = "شماره تماس دوم نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "شماره تماس دوم")]
        public string SecondaryPhoneNumber { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [MaxLength(2000, ErrorMessage = "دستورالعمل‌ها نمی‌توانند بیش از 2000 کاراکتر باشند.")]
        [Display(Name = "دستورالعمل‌ها")]
        public string Instructions { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس نقشه نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس نقشه (Google Maps)")]
        public string MapUrl { get; set; }

        [MaxLength(500, ErrorMessage = "لینک واتساپ نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "لینک واتساپ")]
        public string WhatsAppUrl { get; set; }

        [MaxLength(500, ErrorMessage = "لینک تلگرام نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "لینک تلگرام")]
        public string TelegramUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس ایمیل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس وب‌سایت نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "وب‌سایت")]
        public string WebsiteUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس آیکون نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس آیکون")]
        public string IconUrl { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "همیشه قابل مشاهده (مثلاً در Header)")]
        public bool IsAlwaysVisible { get; set; }

        [Required(ErrorMessage = "ترتیب نمایش الزامی است.")]
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات کوتاه نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات کوتاه")]
        public string ShortDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Slug نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "Slug (آدرس URL)")]
        public string Slug { get; set; }

        [MaxLength(500, ErrorMessage = "عنوان متا نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "عنوان متا (SEO)")]
        public string MetaTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "توضیحات متا (SEO)")]
        public string MetaDescription { get; set; }
    }

    #endregion

    #region Emergency Contact Details

    public class EmergencyContactDetailsViewModel
    {
        public int EmergencyContactId { get; set; }
        public string ContactType { get; set; }
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
        public string SecondaryPhoneNumber { get; set; }
        public string Address { get; set; }
        public string Instructions { get; set; }
        public string MapUrl { get; set; }
        public string WhatsAppUrl { get; set; }
        public string TelegramUrl { get; set; }
        public string Email { get; set; }
        public string WebsiteUrl { get; set; }
        public string IconUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsAlwaysVisible { get; set; }
        public int DisplayOrder { get; set; }
        public string ShortDescription { get; set; }
        public string Slug { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion

    #region Emergency Contact Public (برای نمایش در سایت)

    public class EmergencyContactPublicViewModel
    {
        public int EmergencyContactId { get; set; }
        public string ContactType { get; set; }
        public string TypeDisplayName { get; set; }
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
        public string SecondaryPhoneNumber { get; set; }
        public string Address { get; set; }
        public string Instructions { get; set; }
        public string MapUrl { get; set; }
        public string WhatsAppUrl { get; set; }
        public string TelegramUrl { get; set; }
        public string Email { get; set; }
        public string WebsiteUrl { get; set; }
        public string IconUrl { get; set; }
        public string ShortDescription { get; set; }
        public string Slug { get; set; }
    }

    #endregion
}

