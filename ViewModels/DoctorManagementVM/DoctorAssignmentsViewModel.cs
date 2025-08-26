using System.Collections.Generic;

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
    /// </summary>
    public class DoctorAssignmentsViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// لیست انتسابات پزشک به دپارتمان‌ها
        /// </summary>
        public List<DoctorDepartmentViewModel> DoctorDepartments { get; set; } = new List<DoctorDepartmentViewModel>();

        /// <summary>
        /// لیست انتسابات پزشک به سرفصل‌های خدماتی
        /// </summary>
        public List<DoctorServiceCategoryViewModel> DoctorServiceCategories { get; set; } = new List<DoctorServiceCategoryViewModel>();
    }
}
