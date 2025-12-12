using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.CMS
{
    #region ContactForm Index

    public class ContactFormIndexViewModel
    {
        public int ContactFormId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public ContactFormCategory Category { get; set; }
        public string CategoryDisplay { get; set; }
        public ContactFormStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? RepliedAt { get; set; }
        public string RepliedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ContactFormIndexPageViewModel
    {
        public PagedResult<ContactFormIndexViewModel> ContactForms { get; set; }
        public ContactFormSearchViewModel SearchModel { get; set; }
        public int UnreadCount { get; set; }
        public int NewCount { get; set; }
        public int InProgressCount { get; set; }
        public int RepliedCount { get; set; }
    }

    #endregion

    #region ContactForm Create & Edit

    public class ContactFormCreateEditViewModel
    {
        public int ContactFormId { get; set; }

        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام و نام خانوادگی نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "ایمیل الزامی است.")]
        [MaxLength(200, ErrorMessage = "ایمیل نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Required(ErrorMessage = "شماره تماس الزامی است.")]
        [MaxLength(50, ErrorMessage = "شماره تماس نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "شماره تماس")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "موضوع الزامی است.")]
        [MaxLength(500, ErrorMessage = "موضوع نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "موضوع")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "پیام الزامی است.")]
        [MaxLength(5000, ErrorMessage = "پیام نمی‌تواند بیش از 5000 کاراکتر باشد.")]
        [Display(Name = "پیام")]
        public string Message { get; set; }

        [Required(ErrorMessage = "دسته‌بندی الزامی است.")]
        [Display(Name = "دسته‌بندی")]
        public ContactFormCategory Category { get; set; }

        [Required(ErrorMessage = "وضعیت الزامی است.")]
        [Display(Name = "وضعیت")]
        public ContactFormStatus Status { get; set; }

        [MaxLength(5000, ErrorMessage = "پاسخ نمی‌تواند بیش از 5000 کاراکتر باشد.")]
        [Display(Name = "پاسخ")]
        public string ReplyMessage { get; set; }

        [Display(Name = "خوانده شده")]
        public bool IsRead { get; set; }
    }

    #endregion

    #region ContactForm Details

    public class ContactFormDetailsViewModel
    {
        public int ContactFormId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public ContactFormCategory Category { get; set; }
        public string CategoryDisplay { get; set; }
        public ContactFormStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public string ReplyMessage { get; set; }
        public DateTime? RepliedAt { get; set; }
        public string RepliedByUserName { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string ReadByUserName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
    }

    #endregion

    #region ContactForm Search

    public class ContactFormSearchViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public ContactFormCategory? Category { get; set; }
        public ContactFormStatus? Status { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    #endregion

    #region ContactForm Reply

    public class ContactFormReplyViewModel
    {
        public int ContactFormId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public ContactFormCategory Category { get; set; }

        [Required(ErrorMessage = "پاسخ الزامی است.")]
        [MaxLength(5000, ErrorMessage = "پاسخ نمی‌تواند بیش از 5000 کاراکتر باشد.")]
        [Display(Name = "پاسخ")]
        public string ReplyMessage { get; set; }

        [Display(Name = "ارسال ایمیل")]
        public bool SendEmail { get; set; } = true;

        [Display(Name = "ارسال پیامک")]
        public bool SendSms { get; set; } = false;
    }

    #endregion

    #region Public Contact Form

    public class PublicContactFormViewModel
    {
        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام و نام خانوادگی نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "ایمیل الزامی است.")]
        [MaxLength(200, ErrorMessage = "ایمیل نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Required(ErrorMessage = "شماره تماس الزامی است.")]
        [MaxLength(50, ErrorMessage = "شماره تماس نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "شماره تماس")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "موضوع الزامی است.")]
        [MaxLength(500, ErrorMessage = "موضوع نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "موضوع")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "پیام الزامی است.")]
        [MaxLength(5000, ErrorMessage = "پیام نمی‌تواند بیش از 5000 کاراکتر باشد.")]
        [Display(Name = "پیام")]
        public string Message { get; set; }

        [Required(ErrorMessage = "دسته‌بندی الزامی است.")]
        [Display(Name = "دسته‌بندی")]
        public ContactFormCategory Category { get; set; }
    }

    #endregion
}

