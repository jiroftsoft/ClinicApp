using ClinicApp.Extensions;
using ClinicApp.Models.Entities.Clinic;
using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.ClinicAdmin
{
    /// <summary>
    /// ViewModel برای ایجاد و ویرایش حساب بانکی کلینیک
    /// </summary>
    public class ClinicBankAccountCreateEditViewModel
    {
        public int ClinicBankAccountId { get; set; }

        [Required(ErrorMessage = "کلینیک الزامی است")]
        [Display(Name = "کلینیک")]
        public int ClinicId { get; set; }

        [Required(ErrorMessage = "شماره شبا الزامی است")]
        [StringLength(26, MinimumLength = 26, ErrorMessage = "شماره شبا باید 26 کاراکتر باشد")]
        [RegularExpression(@"^IR\d{24}$", ErrorMessage = "فرمت شماره شبا نامعتبر است. باید با IR شروع شود و 24 رقم داشته باشد.")]
        [Display(Name = "شماره شبا")]
        public string IbanNumber { get; set; }

        [Required(ErrorMessage = "نام بانک الزامی است")]
        [StringLength(100, ErrorMessage = "نام بانک نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [Display(Name = "نام بانک")]
        public string BankName { get; set; }

        [StringLength(50, ErrorMessage = "شماره حساب نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [Display(Name = "شماره حساب")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "نام صاحب حساب الزامی است")]
        [StringLength(200, ErrorMessage = "نام صاحب حساب نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "نام صاحب حساب")]
        public string AccountHolderName { get; set; }

        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; } = true;

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        // برای نمایش در View
        [Display(Name = "نام کلینیک")]
        public string ClinicName { get; set; }

        /// <summary>
        /// Factory Method: ایجاد ViewModel از Entity
        /// </summary>
        public static ClinicBankAccountCreateEditViewModel FromEntity(ClinicBankAccount entity)
        {
            if (entity == null) return null;

            return new ClinicBankAccountCreateEditViewModel
            {
                ClinicBankAccountId = entity.ClinicBankAccountId,
                ClinicId = entity.ClinicId,
                ClinicName = entity.Clinic?.Name,
                IbanNumber = entity.IbanNumber,
                BankName = entity.BankName,
                AccountNumber = entity.AccountNumber,
                AccountHolderName = entity.AccountHolderName,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                Description = entity.Description
            };
        }

        /// <summary>
        /// Mapping: تبدیل ViewModel به Entity
        /// </summary>
        public void MapToEntity(ClinicBankAccount entity)
        {
            if (entity == null) return;

            entity.ClinicId = this.ClinicId;
            entity.IbanNumber = this.IbanNumber?.Trim().ToUpper();
            entity.BankName = this.BankName?.Trim();
            entity.AccountNumber = this.AccountNumber?.Trim();
            entity.AccountHolderName = this.AccountHolderName?.Trim();
            entity.IsDefault = this.IsDefault;
            entity.IsActive = this.IsActive;
            entity.Description = this.Description?.Trim();
        }
    }

    /// <summary>
    /// ViewModel برای نمایش حساب بانکی در لیست
    /// </summary>
    public class ClinicBankAccountIndexViewModel
    {
        public int ClinicBankAccountId { get; set; }
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }
        public string IbanNumber { get; set; }
        public string BankName { get; set; }
        public string AccountHolderName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public string CreatedAtShamsi { get; set; }

        /// <summary>
        /// Factory Method: ایجاد ViewModel از Entity
        /// </summary>
        public static ClinicBankAccountIndexViewModel FromEntity(ClinicBankAccount entity)
        {
            if (entity == null) return null;

            return new ClinicBankAccountIndexViewModel
            {
                ClinicBankAccountId = entity.ClinicBankAccountId,
                ClinicId = entity.ClinicId,
                ClinicName = entity.Clinic?.Name,
                IbanNumber = entity.IbanNumber,
                BankName = entity.BankName,
                AccountHolderName = entity.AccountHolderName,
                IsActive = entity.IsActive,
                IsDefault = entity.IsDefault,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDate()
            };
        }
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات کامل حساب بانکی
    /// </summary>
    public class ClinicBankAccountDetailsViewModel
    {
        public int ClinicBankAccountId { get; set; }
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }
        public string IbanNumber { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolderName { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public string CreatedAtShamsi { get; set; }
        public string UpdatedAtShamsi { get; set; }
        public string CreatedByUserName { get; set; }
        public string UpdatedByUserName { get; set; }

        /// <summary>
        /// Factory Method: ایجاد ViewModel از Entity
        /// </summary>
        public static ClinicBankAccountDetailsViewModel FromEntity(ClinicBankAccount entity)
        {
            if (entity == null) return null;

            return new ClinicBankAccountDetailsViewModel
            {
                ClinicBankAccountId = entity.ClinicBankAccountId,
                ClinicId = entity.ClinicId,
                ClinicName = entity.Clinic?.Name,
                IbanNumber = entity.IbanNumber,
                BankName = entity.BankName,
                AccountNumber = entity.AccountNumber,
                AccountHolderName = entity.AccountHolderName,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                Description = entity.Description,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = entity.UpdatedAt?.ToPersianDateTime(),
                CreatedByUserName = entity.CreatedByUser?.UserName,
                UpdatedByUserName = entity.UpdatedByUser?.UserName
            };
        }
    }
}

