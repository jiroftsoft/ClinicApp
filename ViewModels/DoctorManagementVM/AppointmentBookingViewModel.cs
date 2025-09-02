using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// ViewModel برای رزرو نوبت پزشکی
    /// </summary>
    public class AppointmentBookingViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        [Display(Name = "تخصص")]
        public string Specialization { get; set; }

        /// <summary>
        /// نام کلینیک
        /// </summary>
        [Display(Name = "کلینیک")]
        public string ClinicName { get; set; }

        /// <summary>
        /// آدرس کلینیک
        /// </summary>
        [Display(Name = "آدرس")]
        public string ClinicAddress { get; set; }

        /// <summary>
        /// هزینه ویزیت (ریال)
        /// </summary>
        [Display(Name = "هزینه ویزیت")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ConsultationFee { get; set; }

        /// <summary>
        /// تاریخ‌های در دسترس
        /// </summary>
        [Display(Name = "تاریخ‌های در دسترس")]
        public List<DateTime> AvailableDates { get; set; } = new List<DateTime>();

        /// <summary>
        /// اسلات‌های زمانی در دسترس
        /// </summary>
        [Display(Name = "زمان‌های در دسترس")]
        public List<TimeSlotViewModel> AvailableTimeSlots { get; set; } = new List<TimeSlotViewModel>();

        /// <summary>
        /// شناسه بیمار (اختیاری)
        /// </summary>
        public int? PatientId { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        [Display(Name = "نام بیمار")]
        [Required(ErrorMessage = "نام بیمار الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام بیمار نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string PatientName { get; set; }

        /// <summary>
        /// شماره تلفن بیمار
        /// </summary>
        [Display(Name = "شماره تلفن")]
        [Required(ErrorMessage = "شماره تلفن الزامی است.")]
        [MaxLength(20, ErrorMessage = "شماره تلفن نمی‌تواند بیش از 20 کاراکتر باشد.")]
        public string PatientPhone { get; set; }

        /// <summary>
        /// کد ملی بیمار
        /// </summary>
        [Display(Name = "کد ملی")]
        [MaxLength(10, ErrorMessage = "کد ملی نمی‌تواند بیش از 10 کاراکتر باشد.")]
        public string PatientNationalCode { get; set; }

        /// <summary>
        /// آیا بیمار جدید است؟
        /// </summary>
        [Display(Name = "بیمار جدید")]
        public bool IsNewPatient { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی خدمت (اختیاری)
        /// </summary>
        public int? ServiceCategoryId { get; set; }

        /// <summary>
        /// نام دسته‌بندی خدمت
        /// </summary>
        [Display(Name = "دسته‌بندی خدمت")]
        public string ServiceCategoryName { get; set; }

        /// <summary>
        /// قیمت خدمت (ریال)
        /// </summary>
        [Display(Name = "قیمت خدمت")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ServicePrice { get; set; }

        /// <summary>
        /// شناسه اسلات انتخاب شده
        /// </summary>
        [Required(ErrorMessage = "لطفاً زمان نوبت را انتخاب کنید.")]
        public int? SelectedSlotId { get; set; }

        /// <summary>
        /// تاریخ نوبت انتخاب شده
        /// </summary>
        [Display(Name = "تاریخ نوبت")]
        public DateTime? SelectedDate { get; set; }

        /// <summary>
        /// زمان نوبت انتخاب شده
        /// </summary>
        [Display(Name = "زمان نوبت")]
        public TimeSpan? SelectedTime { get; set; }

        /// <summary>
        /// توضیحات اضافی
        /// </summary>
        [Display(Name = "توضیحات")]
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Notes { get; set; }

        /// <summary>
        /// آیا رزرو اورژانس است؟
        /// </summary>
        [Display(Name = "رزرو اورژانس")]
        public bool IsEmergencyBooking { get; set; }

        /// <summary>
        /// دلیل اورژانس (در صورت رزرو اورژانس)
        /// </summary>
        [Display(Name = "دلیل اورژانس")]
        [MaxLength(200, ErrorMessage = "دلیل اورژانس نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string EmergencyReason { get; set; }

        /// <summary>
        /// تاریخ نمایشی انتخاب شده (فارسی)
        /// </summary>
        [Display(Name = "تاریخ نوبت")]
        public string SelectedDateDisplay => SelectedDate?.ToString("yyyy/MM/dd") ?? "انتخاب نشده";

        /// <summary>
        /// زمان نمایشی انتخاب شده (فارسی)
        /// </summary>
        [Display(Name = "زمان نوبت")]
        public string SelectedTimeDisplay => SelectedTime?.ToString(@"hh\:mm") ?? "انتخاب نشده";

        /// <summary>
        /// روز هفته تاریخ انتخاب شده (فارسی)
        /// </summary>
        [Display(Name = "روز هفته")]
        public string SelectedDayOfWeekDisplay
        {
            get
            {
                if (!SelectedDate.HasValue) return "انتخاب نشده";
                
                return SelectedDate.Value.DayOfWeek switch
                {
                    DayOfWeek.Saturday => "شنبه",
                    DayOfWeek.Sunday => "یکشنبه",
                    DayOfWeek.Monday => "دوشنبه",
                    DayOfWeek.Tuesday => "سه‌شنبه",
                    DayOfWeek.Wednesday => "چهارشنبه",
                    DayOfWeek.Thursday => "پنج‌شنبه",
                    DayOfWeek.Friday => "جمعه",
                    _ => "نامشخص"
                };
            }
        }

        /// <summary>
        /// کل مبلغ قابل پرداخت (ریال)
        /// </summary>
        [Display(Name = "کل مبلغ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalAmount => ConsultationFee + ServicePrice;

        /// <summary>
        /// آیا فرم کامل است؟
        /// </summary>
        public bool IsFormComplete => 
            DoctorId > 0 && 
            !string.IsNullOrEmpty(PatientName) && 
            !string.IsNullOrEmpty(PatientPhone) && 
            SelectedSlotId.HasValue && 
            SelectedDate.HasValue && 
            SelectedTime.HasValue;

        /// <summary>
        /// پیام خطا یا هشدار
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// آیا فرم معتبر است؟
        /// </summary>
        public bool IsValid => string.IsNullOrEmpty(ValidationMessage);
    }
}
