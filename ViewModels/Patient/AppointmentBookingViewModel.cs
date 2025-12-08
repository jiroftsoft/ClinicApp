using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// ViewModel برای رزرو نوبت
    /// </summary>
    public class AppointmentBookingViewModel
    {
        [Required]
        public int DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "تخصص")]
        public string DoctorSpecialization { get; set; }

        [Required]
        [Display(Name = "تاریخ نوبت")]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [Display(Name = "زمان شروع")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "زمان پایان")]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "مبلغ")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "دسته‌بندی خدمت")]
        public int? ServiceCategoryId { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(500)]
        public string Description { get; set; }
    }
}

