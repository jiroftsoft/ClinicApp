using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش پزشک در فرم پذیرش
    /// </summary>
    public class ReceptionDoctorViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی پزشک
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// نام کامل پزشک
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        public string Specialization { get; set; }

        /// <summary>
        /// شماره نظام پزشکی
        /// </summary>
        public string MedicalLicenseNumber { get; set; }

        /// <summary>
        /// شماره تلفن پزشک
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// ایمیل پزشک
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// آیا پزشک فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// آیا پزشک در دسترس است؟
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// ساعت شروع کار
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// ساعت پایان کار
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// نمایش نام کامل پزشک
        /// </summary>
        public string DisplayName => FullName;

        /// <summary>
        /// نمایش تخصص پزشک
        /// </summary>
        public string SpecializationDisplay => Specialization ?? "نامشخص";

        /// <summary>
        /// نمایش وضعیت فعال
        /// </summary>
        public string StatusDisplay => IsActive ? "فعال" : "غیرفعال";

        /// <summary>
        /// نمایش وضعیت در دسترس
        /// </summary>
        public string AvailabilityDisplay => IsAvailable ? "در دسترس" : "غیرقابل دسترس";

        /// <summary>
        /// نمایش اطلاعات پزشک (فرمات شده)
        /// </summary>
        public string DoctorInfoDisplay => $"{FullName} - {Specialization}";

        /// <summary>
        /// نمایش ساعت کار
        /// </summary>
        public string WorkingHoursDisplay => $"{StartTime} - {EndTime}";

        /// <summary>
        /// آیا پزشک در دسترس است؟
        /// </summary>
        public bool IsCurrentlyAvailable => IsActive && IsAvailable;
    }
}
