using FluentValidation;
using ClinicApp.ViewModels.DoctorManagementVM;
using System.Linq;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// ولیدیتور برای مدل انتسابات پزشک
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل انتسابات پزشک به دپارتمان‌ها و سرفصل‌های خدماتی
    /// 2. بررسی تداخل و سازگاری انتسابات
    /// 3. رعایت استانداردهای پزشکی ایران در اعتبارسنجی
    /// 4. پشتیبانی از قوانین کسب‌وکار سیستم‌های پزشکی
    /// 5. اعتبارسنجی سطح بالا برای عملیات ترکیبی
    /// </summary>
    public class DoctorAssignmentsViewModelValidator : AbstractValidator<DoctorAssignmentsViewModel>
    {
        public DoctorAssignmentsViewModelValidator()
        {
            // اعتبارسنجی شناسه پزشک
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است.")
                .WithErrorCode("INVALID_DOCTOR_ID");

            // اعتبارسنجی نام پزشک
            RuleFor(x => x.DoctorName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.DoctorName))
                .WithMessage("نام پزشک نمی‌تواند بیشتر از 100 کاراکتر باشد.")
                .WithErrorCode("DOCTOR_NAME_TOO_LONG");

            // اعتبارسنجی کد ملی پزشک
            RuleFor(x => x.DoctorNationalCode)
                .Matches(@"^\d{10}$")
                .When(x => !string.IsNullOrEmpty(x.DoctorNationalCode))
                .WithMessage("کد ملی باید 10 رقم باشد.")
                .WithErrorCode("INVALID_NATIONAL_CODE");

            // اعتبارسنجی لیست انتسابات دپارتمان‌ها
            RuleFor(x => x.DoctorDepartments)
                .NotNull()
                .WithMessage("لیست انتسابات دپارتمان‌ها نمی‌تواند خالی باشد.")
                .WithErrorCode("DEPARTMENT_ASSIGNMENTS_NULL");

            // اعتبارسنجی لیست انتسابات سرفصل‌های خدماتی
            RuleFor(x => x.DoctorServiceCategories)
                .NotNull()
                .WithMessage("لیست انتسابات سرفصل‌های خدماتی نمی‌تواند خالی باشد.")
                .WithErrorCode("SERVICE_CATEGORY_ASSIGNMENTS_NULL");

            // اعتبارسنجی حداقل یک انتساب فعال
            RuleFor(x => x)
                .Must(x => x.HasActiveAssignments)
                .WithMessage("پزشک باید حداقل یک انتساب فعال داشته باشد.")
                .WithErrorCode("NO_ACTIVE_ASSIGNMENTS");

            // اعتبارسنجی انتسابات دپارتمان‌ها
            RuleForEach(x => x.DoctorDepartments)
                .SetValidator(new DoctorDepartmentViewModelValidator());

            // اعتبارسنجی انتسابات سرفصل‌های خدماتی
            RuleForEach(x => x.DoctorServiceCategories)
                .SetValidator(new DoctorServiceCategoryViewModelValidator());

            // اعتبارسنجی عدم تداخل دپارتمان‌ها
            RuleFor(x => x.DoctorDepartments)
                .Must(departments => departments == null || 
                    departments.Select(d => d.DepartmentId).Distinct().Count() == departments.Count)
                .WithMessage("انتساب تکراری به دپارتمان‌ها مجاز نیست.")
                .WithErrorCode("DUPLICATE_DEPARTMENT_ASSIGNMENT");

            // اعتبارسنجی عدم تداخل سرفصل‌های خدماتی
            RuleFor(x => x.DoctorServiceCategories)
                .Must(services => services == null || 
                    services.Select(s => s.ServiceCategoryId).Distinct().Count() == services.Count)
                .WithMessage("انتساب تکراری به سرفصل‌های خدماتی مجاز نیست.")
                .WithErrorCode("DUPLICATE_SERVICE_CATEGORY_ASSIGNMENT");

            // اعتبارسنجی حداکثر تعداد دپارتمان‌ها
            RuleFor(x => x.DoctorDepartments)
                .Must(departments => departments == null || departments.Count <= 5)
                .WithMessage("پزشک نمی‌تواند به بیش از 5 دپارتمان انتساب داشته باشد.")
                .WithErrorCode("TOO_MANY_DEPARTMENT_ASSIGNMENTS");

            // اعتبارسنجی حداکثر تعداد سرفصل‌های خدماتی
            RuleFor(x => x.DoctorServiceCategories)
                .Must(services => services == null || services.Count <= 20)
                .WithMessage("پزشک نمی‌تواند به بیش از 20 سرفصل خدماتی انتساب داشته باشد.")
                .WithErrorCode("TOO_MANY_SERVICE_CATEGORY_ASSIGNMENTS");

            // اعتبارسنجی سازگاری انتسابات (قانون کسب‌وکار)
            RuleFor(x => x)
                .Must(ValidateAssignmentCompatibility)
                .WithMessage("انتسابات انتخاب شده با قوانین سیستم سازگار نیستند.")
                .WithErrorCode("INCOMPATIBLE_ASSIGNMENTS");
        }

        /// <summary>
        /// اعتبارسنجی سازگاری انتسابات بر اساس قوانین کسب‌وکار
        /// </summary>
        /// <param name="assignments">مدل انتسابات</param>
        /// <returns>آیا انتسابات سازگار هستند</returns>
        private bool ValidateAssignmentCompatibility(DoctorAssignmentsViewModel assignments)
        {
            if (assignments?.DoctorDepartments == null || assignments?.DoctorServiceCategories == null)
                return false;

            // بررسی اینکه آیا پزشک حداقل یک دپارتمان فعال دارد
            var hasActiveDepartment = assignments.DoctorDepartments.Any(d => d.IsActive);
            if (!hasActiveDepartment)
                return false;

            // بررسی اینکه آیا پزشک حداقل یک سرفصل خدماتی فعال دارد
            var hasActiveServiceCategory = assignments.DoctorServiceCategories.Any(s => s.IsActive);
            if (!hasActiveServiceCategory)
                return false;

            // قوانین کسب‌وکار اضافی می‌توانند اینجا اضافه شوند
            // مثلاً بررسی سازگاری دپارتمان و سرفصل‌های خدماتی

            return true;
        }
    }
}
