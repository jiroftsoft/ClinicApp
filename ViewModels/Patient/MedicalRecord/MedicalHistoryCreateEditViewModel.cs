using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Patient.MedicalRecord
{
    /// <summary>
    /// ViewModel برای ایجاد/ویرایش تاریخچه پزشکی
    /// Single Responsibility: دریافت داده‌های ورودی برای Create/Edit
    /// </summary>
    public class MedicalHistoryCreateEditViewModel
    {
        public int? MedicalHistoryId { get; set; }
        
        [Required(ErrorMessage = "نوع تاریخچه پزشکی الزامی است.")]
        [Display(Name = "نوع")]
        public MedicalHistoryType Type { get; set; }
        
        [Required(ErrorMessage = "عنوان الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }
        
        [MaxLength(2000, ErrorMessage = "توضیحات نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }
        
        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }
        
        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }
        
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
        
        [MaxLength(50, ErrorMessage = "شدت نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "شدت")]
        public string Severity { get; set; }
        
        [MaxLength(100, ErrorMessage = "نام پزشک نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "نام پزشک معالج")]
        public string DoctorName { get; set; }
        
        [MaxLength(200, ErrorMessage = "نام مرکز درمانی نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "مرکز درمانی")]
        public string MedicalCenter { get; set; }
        
        [MaxLength(1000, ErrorMessage = "مسیر فایل‌ها نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "فایل‌های ضمیمه")]
        public string Attachments { get; set; }
    }
}

