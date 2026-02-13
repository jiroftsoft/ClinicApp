using System;
using System.Collections.Generic;
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
        
        /// <summary>آلرژی بحرانی (فقط برای نوع آلرژی)</summary>
        public bool? IsCritical { get; set; }
        
        #region فیلدهای دارو (وقتی Type = Medication؛ یک دارو در فرم ساده)
        
        [MaxLength(200, ErrorMessage = "نام دارو نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "نام دارو")]
        public string DrugName { get; set; }
        
        [MaxLength(100)]
        [Display(Name = "دوز")]
        public string Dosage { get; set; }
        
        [MaxLength(50)]
        [Display(Name = "واحد دوز")]
        public string DosageUnit { get; set; }
        
        [MaxLength(100)]
        [Display(Name = "نحوه مصرف")]
        public string Frequency { get; set; }
        
        [MaxLength(50)]
        [Display(Name = "راه مصرف")]
        public string Route { get; set; }
        
        [MaxLength(300)]
        [Display(Name = "دلیل مصرف")]
        public string Indication { get; set; }
        
        [MaxLength(100)]
        [Display(Name = "پزشک تجویزکننده")]
        public string PrescribingDoctor { get; set; }
        
        #endregion
        
        /// <summary>
        /// داروهای مرتبط با بیماری — وقتی نوع = بیماری، چند دارو (مثلاً ASA، والزومکیس، داروی فشار)
        /// </summary>
        public List<MedicalHistoryMedicationItemDto> MedicationsList { get; set; } = new List<MedicalHistoryMedicationItemDto>();
        
        #region فیلدهای آزمایش (وقتی Type = Disease؛ یک نتیجه آزمایش در فرم ساده)
        
        [MaxLength(100)]
        [Display(Name = "نام آزمایش")]
        public string LabName { get; set; }
        
        [MaxLength(50)]
        [Display(Name = "مقدار")]
        public string LabValue { get; set; }
        
        [MaxLength(50)]
        [Display(Name = "واحد")]
        public string LabUnit { get; set; }
        
        [Display(Name = "تاریخ آزمایش")]
        public DateTime? LabDate { get; set; }
        
        [MaxLength(100)]
        [Display(Name = "محدوده مرجع")]
        public string LabReferenceRange { get; set; }
        
        #endregion
    }
}

