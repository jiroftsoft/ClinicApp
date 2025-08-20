using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// مدل داده‌های کلینیک برای ایجاد و ویرایش
    /// این مدل تمام فیلدهای لازم برای مدیریت کلینیک‌ها در سیستم پزشکی را دارد
    /// </summary>
    public class ClinicCreateEditViewModel
    {
        public int ClinicId { get; set; }

        [Required(ErrorMessage = "نام کلینیک الزامی است")]
        [StringLength(200, ErrorMessage = "نام کلینیک نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "نام کلینیک")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "آدرس نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "شماره تلفن نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }
    }


    /// <summary>
    /// مدل داده‌های کلینیک برای نمایش در لیست
    /// </summary>
    public class ClinicIndexViewModel
    {
        public int ClinicId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public int DepartmentCount { get; set; }
        public int DoctorCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
   
        /// <summary>
        /// مدل داده‌های جزئیات کلینیک
        /// </summary>
        public class ClinicDetailsViewModel : ClinicCreateEditViewModel
        {
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public DateTime? DeletedAt { get; set; }
            public string CreatedBy { get; set; }
            public string UpdatedBy { get; set; }
            public string DeletedBy { get; set; }
            public int DepartmentCount { get; set; }
            public int DoctorCount { get; set; }
            public bool IsActive { get; set; }
        }
    }