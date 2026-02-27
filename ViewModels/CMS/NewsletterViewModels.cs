using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.CMS
{
    #region NewsletterSubscription Index

    public class NewsletterSubscriptionIndexViewModel
    {
        public int NewsletterSubscriptionId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string CategoriesDisplay { get; set; }
        public NewsletterSubscriptionSource Source { get; set; }
        public string SourceDisplay { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime SubscriptionDate { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime? UnsubscribedAt { get; set; }
    }

    public class NewsletterSubscriptionIndexPageViewModel
    {
        public PagedResult<NewsletterSubscriptionIndexViewModel> Subscriptions { get; set; }
        public NewsletterSubscriptionSearchViewModel SearchModel { get; set; }
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int VerifiedCount { get; set; }
        public int UnsubscribedCount { get; set; }
    }

    #endregion

    #region NewsletterSubscription Create & Edit

    public class NewsletterSubscriptionCreateEditViewModel
    {
        public int NewsletterSubscriptionId { get; set; }

        [Required(ErrorMessage = "ایمیل الزامی است.")]
        [MaxLength(200, ErrorMessage = "ایمیل نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [MaxLength(200, ErrorMessage = "نام و نام خانوادگی نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; }

        [MaxLength(50, ErrorMessage = "شماره تماس نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "شماره تماس")]
        public string PhoneNumber { get; set; }

        [Display(Name = "دسته‌بندی‌ها")]
        public List<NewsletterCategory> SelectedCategories { get; set; }

        [Required(ErrorMessage = "منبع ثبت‌نام الزامی است.")]
        [Display(Name = "منبع ثبت‌نام")]
        public NewsletterSubscriptionSource Source { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }
    }

    #endregion

    #region NewsletterSubscription Details

    public class NewsletterSubscriptionDetailsViewModel
    {
        public int NewsletterSubscriptionId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string CategoriesDisplay { get; set; }
        public List<NewsletterCategory> Categories { get; set; }
        public NewsletterSubscriptionSource Source { get; set; }
        public string SourceDisplay { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime? UnsubscribedAt { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
        public List<NewsletterCampaignRecipientViewModel> CampaignHistory { get; set; }
    }

    public class NewsletterCampaignRecipientViewModel
    {
        public int NewsletterCampaignId { get; set; }
        public string CampaignTitle { get; set; }
        public NewsletterRecipientStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? OpenedAt { get; set; }
        public DateTime? ClickedAt { get; set; }
    }

    #endregion

    #region NewsletterSubscription Search

    public class NewsletterSubscriptionSearchViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsVerified { get; set; }
        public NewsletterSubscriptionSource? Source { get; set; }
        public NewsletterCategory? Category { get; set; }
    }

    #endregion

    #region Public NewsletterSubscription

    public class PublicNewsletterSubscriptionViewModel
    {
        [Required(ErrorMessage = "ایمیل الزامی است.")]
        [MaxLength(200, ErrorMessage = "ایمیل نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [MaxLength(200, ErrorMessage = "نام و نام خانوادگی نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; }
    }

    #endregion

    #region NewsletterTemplate Index

    public class NewsletterTemplateIndexViewModel
    {
        public int NewsletterTemplateId { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    #endregion

    #region NewsletterTemplate Create & Edit

    public class NewsletterTemplateCreateEditViewModel
    {
        public int NewsletterTemplateId { get; set; }

        [Required(ErrorMessage = "نام Template الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام Template نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام Template")]
        public string Name { get; set; }

        [Required(ErrorMessage = "موضوع ایمیل الزامی است.")]
        [MaxLength(500, ErrorMessage = "موضوع ایمیل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "موضوع ایمیل")]
        public string Subject { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Required(ErrorMessage = "محتوای Template الزامی است.")]
        [AllowHtml] // برای CKEditor
        [Display(Name = "محتوای Template")]
        public string Content { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }
    }

    #endregion

    #region NewsletterTemplate Details

    public class NewsletterTemplateDetailsViewModel
    {
        public int NewsletterTemplateId { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion

    #region NewsletterCampaign Index

    public class NewsletterCampaignIndexViewModel
    {
        public int NewsletterCampaignId { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public NewsletterCampaignStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public int TotalRecipients { get; set; }
        public int SentCount { get; set; }
        public int OpenedCount { get; set; }
        public int ClickedCount { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NewsletterCampaignIndexPageViewModel
    {
        public PagedResult<NewsletterCampaignIndexViewModel> Campaigns { get; set; }
        public NewsletterCampaignSearchViewModel SearchModel { get; set; }
    }

    #endregion

    #region NewsletterCampaign Create & Edit

    public class NewsletterCampaignCreateEditViewModel
    {
        public int NewsletterCampaignId { get; set; }

        [Required(ErrorMessage = "عنوان Campaign الزامی است.")]
        [MaxLength(300, ErrorMessage = "عنوان Campaign نمی‌تواند بیش از 300 کاراکتر باشد.")]
        [Display(Name = "عنوان Campaign")]
        public string Title { get; set; }

        [Required(ErrorMessage = "موضوع ایمیل الزامی است.")]
        [MaxLength(500, ErrorMessage = "موضوع ایمیل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "موضوع ایمیل")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "محتوای Campaign الزامی است.")]
        [AllowHtml] // برای CKEditor
        [Display(Name = "محتوای Campaign")]
        public string Content { get; set; }

        [Display(Name = "Template")]
        public int? NewsletterTemplateId { get; set; }

        [Display(Name = "دسته‌بندی‌ها")]
        public List<NewsletterCategory> SelectedCategories { get; set; }

        [Display(Name = "ارسال به تمام مشترکین")]
        public bool SendToAll { get; set; }

        [Display(Name = "زمان‌بندی ارسال")]
        public DateTime? ScheduledAt { get; set; }
    }

    #endregion

    #region NewsletterCampaign Details

    /// <summary>یک خطای ارسال برای نمایش به مدیر (منشی).</summary>
    public class NewsletterCampaignSendErrorItem
    {
        public string RecipientEmail { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class NewsletterCampaignDetailsViewModel
    {
        public int NewsletterCampaignId { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public int? NewsletterTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string CategoriesDisplay { get; set; }
        public List<NewsletterCategory> Categories { get; set; }
        public bool SendToAll { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? SentAt { get; set; }
        public NewsletterCampaignStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public int TotalRecipients { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public int OpenedCount { get; set; }
        public int ClickedCount { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
        /// <summary>خطاهای ارسال برای نمایش به مدیر — لیست گیرندگانی که ارسال برایشان ناموفق بوده.</summary>
        public List<NewsletterCampaignSendErrorItem> SendErrors { get; set; } = new List<NewsletterCampaignSendErrorItem>();
        /// <summary>آیا می‌توان دکمه «ارسال مجدد» را نشان داد (وضعیت در حال ارسال یا ناموفق).</summary>
        public bool CanRetry { get; set; }
        /// <summary>تعداد گیرندگانی که هنوز در صف ارسال هستند.</summary>
        public int PendingCount { get; set; }
    }

    #endregion

    #region NewsletterCampaign Search

    public class NewsletterCampaignSearchViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public NewsletterCampaignStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    #endregion

    #region NewsletterCampaign Send

    public class NewsletterCampaignSendViewModel
    {
        public int NewsletterCampaignId { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public int EstimatedRecipients { get; set; }
        public bool SendEmail { get; set; }
        public bool SendSms { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }

    #endregion

    #region Newsletter Statistics

    public class NewsletterStatisticsViewModel
    {
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int VerifiedSubscriptions { get; set; }
        public int UnsubscribedCount { get; set; }
        public int TotalCampaigns { get; set; }
        public int SentCampaigns { get; set; }
        public double AverageOpenRate { get; set; }
        public double AverageClickRate { get; set; }
        public List<NewsletterCampaignStatisticsViewModel> RecentCampaigns { get; set; }
        public List<NewsletterSubscriptionGrowthViewModel> SubscriptionGrowth { get; set; }
    }

    public class NewsletterCampaignStatisticsViewModel
    {
        public int NewsletterCampaignId { get; set; }
        public string Title { get; set; }
        public int TotalRecipients { get; set; }
        public int SentCount { get; set; }
        public int OpenedCount { get; set; }
        public int ClickedCount { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
        public DateTime? SentAt { get; set; }
    }

    public class NewsletterSubscriptionGrowthViewModel
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    #endregion
}

