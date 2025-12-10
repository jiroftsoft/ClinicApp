using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Helpers;

namespace ClinicApp.ViewModels.CMS
{
    #region Testimonial Index

    public class TestimonialIndexViewModel
    {
        public int TestimonialId { get; set; }
        public string PatientName { get; set; }
        public string PatientInitials { get; set; }
        public string Comment { get; set; }
        public decimal Rating { get; set; }
        public string DoctorName { get; set; }
        public bool IsApproved { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region Testimonial Create & Edit

    public class TestimonialCreateEditViewModel
    {
        public int TestimonialId { get; set; }

        [Required(ErrorMessage = "نام بیمار الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [MaxLength(10, ErrorMessage = "حروف اول نمی‌تواند بیش از 10 کاراکتر باشد.")]
        [Display(Name = "حروف اول")]
        public string PatientInitials { get; set; }

        [Required(ErrorMessage = "نظر الزامی است.")]
        [MaxLength(2000, ErrorMessage = "نظر نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        [Display(Name = "نظر")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "امتیاز الزامی است.")]
        [Range(0, 5, ErrorMessage = "امتیاز باید بین 0 تا 5 باشد.")]
        [Display(Name = "امتیاز")]
        public decimal Rating { get; set; }

        [MaxLength(200, ErrorMessage = "نام پزشک نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس تصویر")]
        public string PhotoUrl { get; set; }

        [MaxLength(500, ErrorMessage = "آدرس ویدیو نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس ویدیو")]
        public string VideoUrl { get; set; }

        [Display(Name = "تایید شده")]
        public bool IsApproved { get; set; }

        [Display(Name = "ویژه")]
        public bool IsFeatured { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int? PatientId { get; set; }

        [Display(Name = "شناسه پزشک")]
        public int? DoctorId { get; set; }
    }

    #endregion

    #region Testimonial Details

    public class TestimonialDetailsViewModel
    {
        public int TestimonialId { get; set; }
        public string PatientName { get; set; }
        public string PatientInitials { get; set; }
        public string Comment { get; set; }
        public decimal Rating { get; set; }
        public string DoctorName { get; set; }
        public string PhotoUrl { get; set; }
        public string VideoUrl { get; set; }
        public bool IsApproved { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
    }

    #endregion
}

