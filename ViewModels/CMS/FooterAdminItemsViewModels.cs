using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.CMS
{
    public class FooterLinksIndexViewModel
    {
        public byte LinkType { get; set; } // 1 quick, 2 service
        public string LinkTypeTitle => LinkType == 2 ? "لینک‌های خدمات" : "لینک‌های سریع";
        public List<FooterLinkItemViewModel> Items { get; set; } = new List<FooterLinkItemViewModel>();
    }

    public class FooterLinkItemViewModel
    {
        public int FooterLinkId { get; set; }
        public byte LinkType { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public bool IsExternal { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class FooterLinkCreateEditViewModel
    {
        public int FooterLinkId { get; set; }

        [Display(Name = "نوع لینک")]
        public byte LinkType { get; set; } // 1 quick, 2 service

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200)]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [Required(ErrorMessage = "URL الزامی است.")]
        [MaxLength(500)]
        [Display(Name = "URL")]
        public string Url { get; set; }

        [MaxLength(100)]
        [Display(Name = "آیکن (FontAwesome)")]
        public string Icon { get; set; }

        [Display(Name = "خارجی")]
        public bool IsExternal { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }

    public class FooterSocialIndexViewModel
    {
        public List<FooterSocialItemViewModel> Items { get; set; } = new List<FooterSocialItemViewModel>();
    }

    public class FooterSocialItemViewModel
    {
        public int FooterSocialId { get; set; }
        public string Platform { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public string AriaLabel { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class FooterSocialCreateEditViewModel
    {
        public int FooterSocialId { get; set; }

        [Required(ErrorMessage = "نام پلتفرم الزامی است.")]
        [MaxLength(100)]
        [Display(Name = "پلتفرم")]
        public string Platform { get; set; }

        [Required(ErrorMessage = "URL الزامی است.")]
        [MaxLength(500)]
        [Display(Name = "URL")]
        public string Url { get; set; }

        [MaxLength(100)]
        [Display(Name = "آیکن (FontAwesome)")]
        public string Icon { get; set; }

        [MaxLength(200)]
        [Display(Name = "Aria Label")]
        public string AriaLabel { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }

    public class FooterCertificationIndexViewModel
    {
        public List<FooterCertificationItemViewModel> Items { get; set; } = new List<FooterCertificationItemViewModel>();
    }

    public class FooterCertificationItemViewModel
    {
        public int FooterCertificationId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string LinkUrl { get; set; }
        public string LicenseNumber { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class FooterCertificationCreateEditViewModel
    {
        public int FooterCertificationId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200)]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [MaxLength(500)]
        [Display(Name = "توضیح")]
        public string Description { get; set; }

        [MaxLength(500)]
        [Display(Name = "آدرس تصویر")]
        public string ImageUrl { get; set; }

        [MaxLength(500)]
        [Display(Name = "لینک")]
        public string LinkUrl { get; set; }

        [MaxLength(100)]
        [Display(Name = "شماره مجوز")]
        public string LicenseNumber { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }
}

