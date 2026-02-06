using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;

namespace ClinicApp.ViewModels.CMS
{
    #region BlogPost Index & Search

    public class BlogPostSearchViewModel
    {
        public string SearchTerm { get; set; }
        public string CategoryName { get; set; }
        public bool? IsPublished { get; set; }
        public bool? IsFeatured { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class BlogPostIndexViewModel
    {
        public int BlogPostId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string CategoryName { get; set; }
        public string AuthorName { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ViewCount { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Slug { get; set; }
    }

    /// <summary>
    /// ViewModel صفحه عمومی Index مقالات (Strongly-Typed - بدون ViewBag برای داده).
    /// قرارداد: 03-Development-Contract-Quick-Guide
    /// </summary>
    public class BlogIndexPageViewModel
    {
        public ClinicApp.Interfaces.PagedResult<BlogPostIndexViewModel> Posts { get; set; }
        public string Category { get; set; }
    }

    #endregion

    #region BlogPost Create & Edit

    public class BlogPostCreateEditViewModel
    {
        public int BlogPostId { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(500, ErrorMessage = "عنوان نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "خلاصه نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "خلاصه")]
        public string Summary { get; set; }

        [Required(ErrorMessage = "محتوا الزامی است.")]
        [Display(Name = "محتوا")]
        public string Content { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر")]
        public string ImageUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر کوچک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر کوچک")]
        public string ThumbnailUrl { get; set; }

        [MaxLength(100, ErrorMessage = "نام نویسنده نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "نام نویسنده")]
        public string AuthorName { get; set; }

        [MaxLength(50, ErrorMessage = "نام دسته‌بندی نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "دسته‌بندی")]
        public string CategoryName { get; set; }

        [Display(Name = "تاریخ انتشار")]
        public DateTime? PublishedAt { get; set; }

        [Display(Name = "منتشر شده")]
        public bool IsPublished { get; set; }

        [Display(Name = "ویژه")]
        public bool IsFeatured { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int? DisplayOrder { get; set; }

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

    #region BlogPost Details

    public class BlogPostDetailsViewModel
    {
        public int BlogPostId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string AuthorName { get; set; }
        public string CategoryName { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public int ViewCount { get; set; }
        public int? DisplayOrder { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string Slug { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات مقاله همراه با کامنت‌ها
    /// طراحی شده برای محیط Production درمانی با Strongly-Typed
    /// </summary>
    public class BlogPostDetailsWithCommentsViewModel
    {
        public BlogPostDetailsViewModel BlogPost { get; set; }
        public PagedResult<BlogPostCommentViewModel> Comments { get; set; }
    }

    #endregion
}

