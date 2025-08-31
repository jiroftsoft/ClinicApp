using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using FluentValidation;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل آیتم‌های لیست کشویی (Dropdown) برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از لیست‌های کشویی در تمام فرآیندهای مدیریتی
    /// 2. رعایت استانداردهای پزشکی ایران در طراحی رابط کاربری
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// </summary>
    public class LookupItemViewModel
    {
        /// <summary>
        /// شناسه آیتم
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// نام آیتم
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// کد آیتم
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// توضیحات آیتم (در صورت وجود)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// آیا آیتم انتخاب شده است؟
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// شناسه گروه (در صورت وجود)
        /// </summary>
        public int? GroupId { get; set; }

        /// <summary>
        /// نام گروه (در صورت وجود)
        /// </summary>
        public string GroupName { get; set; }
        
        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک موجودیت می‌سازد.
        /// </summary>
        public static LookupItemViewModel FromEntity<T>(T entity, Func<T, int> idSelector, Func<T, string> nameSelector, Func<T, string> descriptionSelector = null, Func<T, int?> groupIdSelector = null, Func<T, string> groupNameSelector = null, Func<T, string> codeSelector = null) where T : class
        {
            if (entity == null) return null;
            
            return new LookupItemViewModel
            {
                Id = idSelector(entity),
                Name = nameSelector(entity),
                Code = codeSelector?.Invoke(entity),
                Description = descriptionSelector?.Invoke(entity),
                GroupId = groupIdSelector?.Invoke(entity),
                GroupName = groupNameSelector?.Invoke(entity)
            };
        }
        
        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک موجودیت ساده می‌سازد.
        /// </summary>
        public static LookupItemViewModel FromEntity(int id, string name, string description = null, int? groupId = null, string groupName = null, string code = null)
        {
            return new LookupItemViewModel
            {
                Id = id,
                Name = name,
                Code = code,
                Description = description,
                GroupId = groupId,
                GroupName = groupName
            };
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل آیتم‌های لیست کشویی
    /// </summary>
    public class LookupItemViewModelValidator : AbstractValidator<LookupItemViewModel>
    {
        public LookupItemViewModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("شناسه آیتم نامعتبر است.");
                
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("نام آیتم نمی‌تواند خالی باشد.")
                .MaximumLength(200)
                .WithMessage("نام آیتم نمی‌تواند بیش از 200 کاراکتر باشد.");
                
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات آیتم نمی‌تواند بیش از 500 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.Description));
                
            RuleFor(x => x.GroupName)
                .MaximumLength(200)
                .WithMessage("نام گروه نمی‌تواند بیش از 200 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.GroupName));
        }
    }
}
