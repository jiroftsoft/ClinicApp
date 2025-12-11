using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.CMS
{
    #region BlogPost Comment Create

    public class BlogPostCommentCreateViewModel
    {
        [Required(ErrorMessage = "شناسه مقاله الزامی است.")]
        public int BlogPostId { get; set; }

        [Required(ErrorMessage = "متن کامنت الزامی است.")]
        [MaxLength(2000, ErrorMessage = "متن کامنت نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        [Display(Name = "متن کامنت")]
        public string CommentText { get; set; }

        [MaxLength(200, ErrorMessage = "نام نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام")]
        public string AuthorName { get; set; }

        [MaxLength(500, ErrorMessage = "ایمیل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        [Display(Name = "ایمیل")]
        public string AuthorEmail { get; set; }

        [MaxLength(50, ErrorMessage = "شماره تماس نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "شماره تماس")]
        public string AuthorPhone { get; set; }

        public int? ParentCommentId { get; set; }

        // برای کاربران لاگین شده
        public string AuthorUserId { get; set; }

        // برای امنیت
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }

    #endregion

    #region BlogPost Comment ViewModel

    public class BlogPostCommentViewModel
    {
        public int BlogPostCommentId { get; set; }
        public int BlogPostId { get; set; }
        public string CommentText { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public string AuthorPhone { get; set; }
        public string AuthorUserId { get; set; }
        public string AuthorUserName { get; set; }
        public bool IsApproved { get; set; }
        public bool IsSpam { get; set; }
        public bool IsReported { get; set; }
        public int? ParentCommentId { get; set; }
        public string ParentCommentAuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ReplyCount { get; set; }
        public List<BlogPostCommentViewModel> Replies { get; set; }
    }

    #endregion

    #region BlogPost Comment Search

    public class BlogPostCommentSearchViewModel
    {
        public int? BlogPostId { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsSpam { get; set; }
        public bool? IsReported { get; set; }
        public string SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    #endregion
}

