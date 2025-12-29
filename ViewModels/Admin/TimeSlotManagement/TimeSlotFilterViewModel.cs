using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Admin.TimeSlotManagement
{
    /// <summary>
    /// ViewModel برای فیلتر و جستجوی اسلات‌های زمانی
    /// </summary>
    public class TimeSlotFilterViewModel
    {
        [Display(Name = "شناسه پزشک")]
        public int? DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "تاریخ شروع")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "وضعیت")]
        public AppointmentStatus? Status { get; set; }

        [Display(Name = "جستجو")]
        public string SearchTerm { get; set; }

        [Display(Name = "شماره صفحه")]
        public int PageNumber { get; set; } = 1;

        [Display(Name = "تعداد در هر صفحه")]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// اعتبارسنجی و تنظیم مقادیر پیش‌فرض
        /// </summary>
        public void ValidateAndSetDefaults()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 20;
            if (PageSize > 100) PageSize = 100;

            // اگر تاریخ شروع مشخص نشده باشد، از امروز استفاده می‌کنیم
            if (!StartDate.HasValue)
            {
                StartDate = DateTime.Today;
            }

            // اگر تاریخ پایان مشخص نشده باشد، 30 روز بعد از تاریخ شروع
            if (!EndDate.HasValue)
            {
                EndDate = StartDate.Value.AddDays(30);
            }

            // اطمینان از اینکه تاریخ پایان بعد از تاریخ شروع است
            if (EndDate.HasValue && StartDate.HasValue && EndDate.Value < StartDate.Value)
            {
                EndDate = StartDate.Value.AddDays(30);
            }
        }
    }
}

