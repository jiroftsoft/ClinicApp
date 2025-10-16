using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش پزشک در فرم پذیرش
    /// </summary>
    public class ReceptionDoctorLookupViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Required(ErrorMessage = "نام پزشک الزامی است")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی پزشک
        /// </summary>
        [Required(ErrorMessage = "نام خانوادگی پزشک الزامی است")]
        public string LastName { get; set; }

        /// <summary>
        /// نام کامل پزشک
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// کد پزشک
        /// </summary>
        public string DoctorCode { get; set; }

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        public string SpecializationName { get; set; }
        
        /// <summary>
        /// تخصص پزشک (alias)
        /// </summary>
        public string Specialization { get; set; }
        
        /// <summary>
        /// نام پزشک (alias)
        /// </summary>
        public string DoctorName { get; set; }

        /// <summary>
        /// شناسه تخصص
        /// </summary>
        public int? SpecializationId { get; set; }

        /// <summary>
        /// شماره پروانه پزشکی
        /// </summary>
        public string MedicalLicenseNumber { get; set; }

        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// وضعیت فعال بودن پزشک
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// وضعیت در دسترس بودن پزشک
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// نام نمایشی پزشک
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// تعداد پذیرش امروز
        /// </summary>
        public int TodayReceptionsCount { get; set; }

        /// <summary>
        /// حداکثر تعداد پذیرش روزانه
        /// </summary>
        public int MaxDailyReceptions { get; set; }

        /// <summary>
        /// شماره تلفن پزشک
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// ایمیل پزشک
        /// </summary>
        public string Email { get; set; }
    }
}
