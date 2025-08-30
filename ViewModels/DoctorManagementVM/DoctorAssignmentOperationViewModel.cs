using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل عملیات انتساب ترکیبی پزشک به دپارتمان و سرفصل‌های خدماتی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. انتساب همزمان پزشک به دپارتمان و سرفصل‌های خدماتی مرتبط
    /// 2. پشتیبانی از عملیات یکپارچه و تراکنشی
    /// 3. رعایت استانداردهای پزشکی ایران
    /// 4. اعتبارسنجی کامل قبل از اجرای عملیات
    /// </summary>
    public class DoctorAssignmentOperationViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "شناسه پزشک الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "شناسه پزشک نامعتبر است")]
        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک (برای نمایش)
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        [Required(ErrorMessage = "شناسه دپارتمان الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "شناسه دپارتمان نامعتبر است")]
        [Display(Name = "شناسه دپارتمان")]
        public int DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان (برای نمایش)
        /// </summary>
        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// لیست شناسه‌های سرفصل‌های خدماتی
        /// </summary>
        [Display(Name = "سرفصل‌های خدماتی")]
        public List<int> ServiceCategoryIds { get; set; } = new List<int>();

        /// <summary>
        /// آیا انتساب فعال باشد
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// توضیحات اضافی
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        /// <summary>
        /// تعداد سرفصل‌های خدماتی انتخاب شده
        /// </summary>
        [Display(Name = "تعداد سرفصل‌های خدماتی")]
        public int ServiceCategoryCount => ServiceCategoryIds?.Count ?? 0;

        /// <summary>
        /// آیا سرفصل خدماتی انتخاب شده است
        /// </summary>
        [Display(Name = "دارای سرفصل خدماتی")]
        public bool HasServiceCategories => ServiceCategoryCount > 0;

        public string DoctorNationalCode { get; set; }
    }

    /// <summary>
    /// مدل عملیات انتقال پزشک بین دپارتمان‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. انتقال پزشک از یک دپارتمان به دپارتمان دیگر
    /// 2. حفظ صلاحیت‌های خدماتی (اختیاری)
    /// 3. مدیریت تراکنش‌های چندگانه
    /// 4. اعتبارسنجی کامل قبل از انتقال
    /// </summary>
    public class DoctorTransferViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "شناسه پزشک الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "شناسه پزشک نامعتبر است")]
        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک (برای نمایش)
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// کد ملی پزشک (برای نمایش)
        /// </summary>
        [Display(Name = "کد ملی پزشک")]
        public string DoctorNationalCode { get; set; }

        /// <summary>
        /// شناسه دپارتمان مبدا
        /// </summary>
        [Required(ErrorMessage = "شناسه دپارتمان مبدا الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "شناسه دپارتمان مبدا نامعتبر است")]
        [Display(Name = "دپارتمان مبدا")]
        public int FromDepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان مبدا (برای نمایش)
        /// </summary>
        [Display(Name = "نام دپارتمان مبدا")]
        public string FromDepartmentName { get; set; }

        /// <summary>
        /// شناسه دپارتمان مقصد
        /// </summary>
        [Required(ErrorMessage = "شناسه دپارتمان مقصد الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "شناسه دپارتمان مقصد نامعتبر است")]
        [Display(Name = "دپارتمان مقصد")]
        public int ToDepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان مقصد (برای نمایش)
        /// </summary>
        [Display(Name = "نام دپارتمان مقصد")]
        public string ToDepartmentName { get; set; }

        /// <summary>
        /// آیا صلاحیت‌های خدماتی حفظ شوند
        /// </summary>
        [Display(Name = "حفظ صلاحیت‌های خدماتی")]
        public bool PreserveServiceCategories { get; set; } = true;

        /// <summary>
        /// توضیحات انتقال
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Display(Name = "توضیحات انتقال")]
        public string TransferReason { get; set; }

        /// <summary>
        /// آیا دپارتمان مبدا و مقصد متفاوت هستند
        /// </summary>
        public bool IsValidTransfer => FromDepartmentId != ToDepartmentId;
    }

    /// <summary>
    /// مدل عملیات حذف کامل انتسابات پزشک
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. حذف تمام انتسابات پزشک به دپارتمان‌ها و سرفصل‌های خدماتی
    /// 2. بررسی وابستگی‌ها قبل از حذف
    /// 3. مدیریت تراکنش‌های چندگانه
    /// 4. اعتبارسنجی کامل قبل از حذف
    /// </summary>
    public class DoctorAssignmentRemovalViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "شناسه پزشک الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "شناسه پزشک نامعتبر است")]
        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک (برای نمایش)
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// دلیل حذف انتسابات
        /// </summary>
        [Required(ErrorMessage = "دلیل حذف انتسابات الزامی است")]
        [MaxLength(500, ErrorMessage = "دلیل حذف نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Display(Name = "دلیل حذف انتسابات")]
        public string RemovalReason { get; set; }

        /// <summary>
        /// آیا حذف قطعی باشد (نه حذف نرم)
        /// </summary>
        [Display(Name = "حذف قطعی")]
        public bool IsPermanentRemoval { get; set; } = false;

        /// <summary>
        /// آیا وابستگی‌ها بررسی شده‌اند
        /// </summary>
        [Display(Name = "وابستگی‌ها بررسی شده")]
        public bool DependenciesChecked { get; set; } = false;

        /// <summary>
        /// تعداد انتسابات فعال
        /// </summary>
        [Display(Name = "تعداد انتسابات فعال")]
        public int ActiveAssignmentsCount { get; set; }

        /// <summary>
        /// آیا پزشک دارای انتسابات فعال است
        /// </summary>
        [Display(Name = "دارای انتسابات فعال")]
        public bool HasActiveAssignments => ActiveAssignmentsCount > 0;
    }
}
