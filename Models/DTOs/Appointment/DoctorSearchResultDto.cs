using System.Collections.Generic;

namespace ClinicApp.Models.DTOs.Appointment
{
    /// <summary>
    /// DTO برای نمایش نتایج جستجوی پزشک
    /// </summary>
    public class DoctorSearchResultDto
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; }
        public string Specialization { get; set; }
        public string MedicalCouncilCode { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool HasActiveSchedule { get; set; }
        public string ScheduleInfo { get; set; } // "شنبه تا چهارشنبه - 07:30 تا 12:00"
        public decimal? BasePrice { get; set; }
        
        /// <summary>
        /// آدرس عکس پروفایل پزشک
        /// </summary>
        public string ProfileImageUrl { get; set; }
        
        /// <summary>
        /// بیوگرافی پزشک
        /// </summary>
        public string Bio { get; set; }
        
        /// <summary>
        /// سال‌های تجربه
        /// </summary>
        public int? ExperienceYears { get; set; }
        
        /// <summary>
        /// امتیاز رضایت بیماران (۰ تا ۵) — مشابه دکترتو / پذیرش۲۴
        /// </summary>
        public decimal? Rating { get; set; }
        
        /// <summary>
        /// تعداد نظرات / ارزیابی بیماران
        /// </summary>
        public int? ReviewCount { get; set; }
        
        /// <summary>
        /// تاریخ‌های نوبت موجود با اطلاعات کامل (حداکثر 5 تاریخ آینده)
        /// </summary>
        public List<AvailableDateInfo> AvailableDates { get; set; } = new List<AvailableDateInfo>();

        /// <summary>
        /// آیا این پزشک امکان مشاوره آنلاین تصویری دارد (برای نمایش از همان مرحله انتخاب پزشک)
        /// </summary>
        public bool HasOnlineConsultation { get; set; }
    }
    
    /// <summary>
    /// اطلاعات تاریخ نوبت موجود برای نمایش روی کارت پزشک
    /// </summary>
    public class AvailableDateInfo
    {
        /// <summary>
        /// تاریخ شمسی (مثلاً: "1404/10/08")
        /// </summary>
        public string PersianDate { get; set; }
        
        /// <summary>
        /// تاریخ کوتاه برای نمایش (مثلاً: "08/10")
        /// </summary>
        public string ShortDate { get; set; }
        
        /// <summary>
        /// نام روز هفته (مثلاً: "یکشنبه", "دوشنبه")
        /// </summary>
        public string DayName { get; set; }
        
        /// <summary>
        /// نام کوتاه روز هفته (مثلاً: "ی", "د")
        /// </summary>
        public string DayNameShort { get; set; }
        
        /// <summary>
        /// زمان شروع (مثلاً: "7:00 قبل از ظهر")
        /// </summary>
        public string StartTime { get; set; }
        
        /// <summary>
        /// زمان پایان (مثلاً: "11:00 قبل از ظهر")
        /// </summary>
        public string EndTime { get; set; }
        
        /// <summary>
        /// محدوده زمانی کامل (مثلاً: "7:00 قبل از ظهر - 11:00 قبل از ظهر")
        /// </summary>
        public string TimeRange { get; set; }
    }
}

