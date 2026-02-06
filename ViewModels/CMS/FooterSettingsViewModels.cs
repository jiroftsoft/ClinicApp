using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.CMS
{
    /// <summary>
    /// ViewModel ویرایش تنظیمات اصلی فوتر در پنل CMS
    /// (Brand + Contact + Legal + WorkingHoursTitle)
    /// </summary>
    public class FooterSettingsEditViewModel
    {
        public int FooterSettingsId { get; set; }

        [Display(Name = "نام کلینیک")]
        [MaxLength(200)]
        public string BrandClinicName { get; set; }

        [Display(Name = "آدرس لوگو")]
        [MaxLength(500)]
        public string BrandLogoUrl { get; set; }

        [Display(Name = "شعار کوتاه")]
        [MaxLength(300)]
        public string BrandTagline { get; set; }

        [Display(Name = "توضیح کوتاه")]
        [MaxLength(1000)]
        public string BrandDescription { get; set; }

        [Display(Name = "لینک صفحه اصلی")]
        [MaxLength(200)]
        public string BrandHomeUrl { get; set; }

        [Display(Name = "تلفن")]
        [MaxLength(50)]
        public string ContactPhone { get; set; }

        [Display(Name = "تلفن اورژانس")]
        [MaxLength(50)]
        public string ContactEmergencyPhone { get; set; }

        [Display(Name = "ایمیل")]
        [MaxLength(200)]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
        public string ContactEmail { get; set; }

        [Display(Name = "آدرس")]
        [MaxLength(500)]
        public string ContactAddress { get; set; }

        [Display(Name = "شماره واتساپ")]
        [MaxLength(50)]
        public string ContactWhatsAppNumber { get; set; }

        [Display(Name = "متن کپی‌رایت")]
        [MaxLength(500)]
        public string LegalCopyrightText { get; set; }

        [Display(Name = "لینک حریم خصوصی")]
        [MaxLength(500)]
        public string LegalPrivacyPolicyUrl { get; set; }

        [Display(Name = "لینک قوانین و مقررات")]
        [MaxLength(500)]
        public string LegalTermsOfServiceUrl { get; set; }

        [Display(Name = "لینک شکایات")]
        [MaxLength(500)]
        public string LegalComplaintsUrl { get; set; }

        [Display(Name = "متن محرمانگی پزشکی")]
        [MaxLength(1000)]
        public string LegalMedicalPrivacyNotice { get; set; }

        [Display(Name = "عنوان ساعات کاری")]
        [MaxLength(100)]
        public string WorkingHoursTitle { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }
}

