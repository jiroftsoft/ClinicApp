using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;
using ClinicApp.ViewModels;

namespace ClinicApp.ViewModels.CMS
{
    #region AboutPage Index & Search

    public class AboutPageSearchViewModel
    {
        public string SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AboutPageIndexPageViewModel
    {
        public Interfaces.PagedResult<AboutPageIndexViewModel> PagedResult { get; set; }
    }

    public class AboutPageIndexViewModel
    {
        public int AboutPageId { get; set; }
        public string ClinicName { get; set; }
        public string ClinicDescription { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    #endregion

    #region AboutPage Create & Edit

    public class AboutPageCreateEditViewModel
    {
        public int AboutPageId { get; set; }

        [Required(ErrorMessage = "نام کلینیک الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام کلینیک نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام کلینیک")]
        public string ClinicName { get; set; }

        [Required(ErrorMessage = "توضیحات کلینیک الزامی است.")]
        [Display(Name = "توضیحات کلینیک")]
        public string ClinicDescription { get; set; }

        [MaxLength(50, ErrorMessage = "سال تأسیس نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "سال تأسیس")]
        public string EstablishedYear { get; set; }

        [Display(Name = "مأموریت و رویکرد درمانی")]
        public List<MissionValueViewModel> MissionValues { get; set; } = new List<MissionValueViewModel>();

        [Display(Name = "مجوزها و اعتبارها")]
        public List<LicenseViewModel> Licenses { get; set; } = new List<LicenseViewModel>();

        [MaxLength(500, ErrorMessage = "نهاد ناظر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "نهاد ناظر")]
        public string RegulatoryBody { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات کادر درمان نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "توضیحات کادر درمان")]
        public string MedicalTeamDescription { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات تجهیزات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "توضیحات تجهیزات و زیرساخت‌ها")]
        public string InfrastructureDescription { get; set; }

        [Display(Name = "تعهدات اخلاقی")]
        public List<EthicalCommitmentViewModel> EthicalCommitments { get; set; } = new List<EthicalCommitmentViewModel>();

        [MaxLength(500, ErrorMessage = "آدرس تصویر Hero نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر Hero Section")]
        public string HeroImageUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر Background نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر Background")]
        public string BackgroundImageUrl { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "ترتیب نمایش الزامی است.")]
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [MaxLength(500, ErrorMessage = "عنوان متا نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "عنوان متا (SEO)")]
        public string MetaTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "توضیحات متا (SEO)")]
        public string MetaDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Slug نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "Slug (آدرس URL)")]
        public string Slug { get; set; }
    }

    #endregion

    #region AboutPage Details

    public class AboutPageDetailsViewModel
    {
        public int AboutPageId { get; set; }
        public string ClinicName { get; set; }
        public string ClinicDescription { get; set; }
        public string EstablishedYear { get; set; }
        public List<MissionValueViewModel> MissionValues { get; set; } = new List<MissionValueViewModel>();
        public List<LicenseViewModel> Licenses { get; set; } = new List<LicenseViewModel>();
        public string RegulatoryBody { get; set; }
        public string MedicalTeamDescription { get; set; }
        public string InfrastructureDescription { get; set; }
        public List<EthicalCommitmentViewModel> EthicalCommitments { get; set; } = new List<EthicalCommitmentViewModel>();
        public string HeroImageUrl { get; set; }
        public string BackgroundImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string Slug { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion

    #region AboutPage Public (برای استفاده در HomePageService)

    public class AboutPagePublicViewModel
    {
        public int AboutPageId { get; set; }
        public string ClinicName { get; set; }
        public string ClinicDescription { get; set; }
        public string EstablishedYear { get; set; }
        public List<MissionValueViewModel> MissionValues { get; set; } = new List<MissionValueViewModel>();
        public List<LicenseViewModel> Licenses { get; set; } = new List<LicenseViewModel>();
        public string RegulatoryBody { get; set; }
        public string MedicalTeamDescription { get; set; }
        public string InfrastructureDescription { get; set; }
        public List<EthicalCommitmentViewModel> EthicalCommitments { get; set; } = new List<EthicalCommitmentViewModel>();
        public string HeroImageUrl { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string Slug { get; set; }
    }

    #endregion
}
