using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل انتسابات پزشک برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل انتسابات پزشک به دپارتمان‌ها و سرفصل‌های خدماتی
    /// 2. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان → پزشک)
    /// 3. رعایت استانداردهای پزشکی ایران در مدیریت پزشکان
    /// 4. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 5. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 6. پشتیبانی از عملیات ترکیبی و انتقال پزشکان
    /// </summary>
    public class DoctorAssignmentsViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "شناسه پزشک الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "شناسه پزشک نامعتبر است")]
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک (برای نمایش)
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// کد ملی پزشک (برای نمایش)
        /// </summary>
        [Display(Name = "کد ملی")]
        public string DoctorNationalCode { get; set; }

        /// <summary>
        /// لیست انتسابات پزشک به دپارتمان‌ها
        /// </summary>
        [Required(ErrorMessage = "لیست انتسابات دپارتمان‌ها الزامی است")]
        public List<DoctorDepartmentViewModel> DoctorDepartments { get; set; } = new List<DoctorDepartmentViewModel>();

        /// <summary>
        /// لیست انتسابات پزشک به سرفصل‌های خدماتی
        /// </summary>
        [Required(ErrorMessage = "لیست انتسابات سرفصل‌های خدماتی الزامی است")]
        public List<DoctorServiceCategoryViewModel> DoctorServiceCategories { get; set; } = new List<DoctorServiceCategoryViewModel>();

        /// <summary>
        /// تعداد کل انتسابات فعال
        /// </summary>
        [Display(Name = "تعداد انتسابات فعال")]
        public int TotalActiveAssignments { get; set; }

        /// <summary>
        /// تعداد انتسابات دپارتمان فعال
        /// </summary>
        [Display(Name = "تعداد دپارتمان‌های فعال")]
        public int ActiveDepartmentCount { get; set; }

        /// <summary>
        /// تعداد انتسابات سرفصل خدماتی فعال
        /// </summary>
        [Display(Name = "تعداد سرفصل‌های خدماتی فعال")]
        public int ActiveServiceCategoryCount { get; set; }

        /// <summary>
        /// آیا پزشک دارای انتسابات فعال است
        /// </summary>
        [Display(Name = "دارای انتسابات فعال")]
        public bool HasActiveAssignments => TotalActiveAssignments > 0;

        /// <summary>
        /// آیا پزشک فقط به یک دپارتمان انتساب دارد
        /// </summary>
        [Display(Name = "تک دپارتمانی")]
        public bool IsSingleDepartment => ActiveDepartmentCount == 1;

        /// <summary>
        /// آیا پزشک به چندین دپارتمان انتساب دارد
        /// </summary>
        [Display(Name = "چند دپارتمانی")]
        public bool IsMultiDepartment => ActiveDepartmentCount > 1;
    }
}
