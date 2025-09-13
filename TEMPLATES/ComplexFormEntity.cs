using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicApp.Core;

namespace ClinicApp.Models.Entities.[Module]
{
    /// <summary>
    /// Entity [Module] - استاندارد کامل برای فرم‌های پیچیده
    /// </summary>
    [Table("[Module]s")]
    public class [Module] : IEntity, ISoftDelete, ITrackable
    {
        #region Primary Key - کلید اصلی

        /// <summary>
        /// شناسه [Module]
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        #endregion

        #region Basic Properties - ویژگی‌های اصلی

        /// <summary>
        /// نام [Module]
        /// </summary>
        [Required(ErrorMessage = "نام الزامی است")]
        [StringLength(250, ErrorMessage = "نام نمی‌تواند بیش از 250 کاراکتر باشد")]
        [Column("Name")]
        public string Name { get; set; }

        /// <summary>
        /// کد [Module]
        /// </summary>
        [Required(ErrorMessage = "کد الزامی است")]
        [StringLength(100, ErrorMessage = "کد نمی‌تواند بیش از 100 کاراکتر باشد")]
        [Column("Code")]
        [Index("IX_[Module]_Code", IsUnique = true)]
        public string Code { get; set; }

        #endregion

        #region Date Properties - ویژگی‌های تاریخ

        /// <summary>
        /// تاریخ شروع اعتبار
        /// </summary>
        [Required(ErrorMessage = "تاریخ شروع الزامی است")]
        [Column("ValidFrom", TypeName = "datetime2")]
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// تاریخ پایان اعتبار
        /// </summary>
        [Column("ValidTo", TypeName = "datetime2")]
        public DateTime? ValidTo { get; set; }

        #endregion

        #region Financial Properties - ویژگی‌های مالی

        /// <summary>
        /// درصد پوشش بیمه
        /// </summary>
        [Required(ErrorMessage = "درصد پوشش بیمه الزامی است")]
        [Range(0, 100, ErrorMessage = "درصد پوشش بیمه باید بین 0 تا 100 باشد")]
        [Column("CoveragePercent", TypeName = "decimal(5,2)")]
        public decimal CoveragePercent { get; set; }

        /// <summary>
        /// مبلغ فرانشیز
        /// </summary>
        [Required(ErrorMessage = "مبلغ فرانشیز الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ فرانشیز باید بزرگتر یا مساوی صفر باشد")]
        [Column("Deductible", TypeName = "decimal(18,2)")]
        public decimal Deductible { get; set; }

        #endregion

        #region Additional Properties - ویژگی‌های تکمیلی

        /// <summary>
        /// توضیحات
        /// </summary>
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Column("Description")]
        public string Description { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        #endregion

        #region Foreign Keys - کلیدهای خارجی

        /// <summary>
        /// شناسه ارائه‌دهنده بیمه
        /// </summary>
        [Required(ErrorMessage = "ارائه‌دهنده بیمه الزامی است")]
        [Column("InsuranceProviderId")]
        [ForeignKey("InsuranceProvider")]
        public int InsuranceProviderId { get; set; }

        #endregion

        #region Navigation Properties - ویژگی‌های ناوبری

        /// <summary>
        /// ارائه‌دهنده بیمه
        /// </summary>
        public virtual InsuranceProvider InsuranceProvider { get; set; }

        /// <summary>
        /// بیمه‌های بیماران
        /// </summary>
        public virtual ICollection<PatientInsurance> PatientInsurances { get; set; } = new List<PatientInsurance>();

        /// <summary>
        /// خدمات طرح بیمه
        /// </summary>
        public virtual ICollection<PlanService> PlanServices { get; set; } = new List<PlanService>();

        #endregion

        #region ISoftDelete Implementation - پیاده‌سازی ISoftDelete

        /// <summary>
        /// وضعیت حذف
        /// </summary>
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// تاریخ حذف
        /// </summary>
        [Column("DeletedAt", TypeName = "datetime2")]
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// کاربر حذف کننده
        /// </summary>
        [Column("DeletedBy")]
        [StringLength(450)]
        public string DeletedBy { get; set; }

        #endregion

        #region ITrackable Implementation - پیاده‌سازی ITrackable

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        [Column("CreatedAt", TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// کاربر ایجاد کننده
        /// </summary>
        [Column("CreatedBy")]
        [StringLength(450)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        [Column("UpdatedAt", TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// کاربر آخرین به‌روزرسانی کننده
        /// </summary>
        [Column("UpdatedBy")]
        [StringLength(450)]
        public string UpdatedBy { get; set; }

        #endregion

        #region Business Logic Properties - ویژگی‌های منطق کسب‌وکار

        /// <summary>
        /// درصد پرداخت بیمار
        /// </summary>
        [NotMapped]
        public decimal PatientSharePercent => 100 - CoveragePercent;

        /// <summary>
        /// بررسی معتبر بودن در تاریخ مشخص
        /// </summary>
        [NotMapped]
        public bool IsValidOnDate(DateTime date)
        {
            return IsActive && 
                   !IsDeleted && 
                   ValidFrom <= date && 
                   (!ValidTo.HasValue || ValidTo >= date);
        }

        /// <summary>
        /// بررسی معتبر بودن در تاریخ فعلی
        /// </summary>
        [NotMapped]
        public bool IsCurrentlyValid => IsValidOnDate(DateTime.Now);

        /// <summary>
        /// بررسی منقضی شده بودن
        /// </summary>
        [NotMapped]
        public bool IsExpired => ValidTo.HasValue && ValidTo < DateTime.Now;

        /// <summary>
        /// بررسی در حال انقضا بودن (30 روز آینده)
        /// </summary>
        [NotMapped]
        public bool IsExpiringSoon => ValidTo.HasValue && 
                                     ValidTo >= DateTime.Now && 
                                     ValidTo <= DateTime.Now.AddDays(30);

        /// <summary>
        /// تعداد روزهای باقی‌مانده تا انقضا
        /// </summary>
        [NotMapped]
        public int DaysUntilExpiry
        {
            get
            {
                if (!ValidTo.HasValue) return int.MaxValue;
                return (ValidTo.Value - DateTime.Now).Days;
            }
        }

        /// <summary>
        /// تعداد روزهای اعتبار
        /// </summary>
        [NotMapped]
        public int ValidityDays
        {
            get
            {
                var endDate = ValidTo ?? DateTime.Now.AddYears(1);
                return (int)(endDate - ValidFrom).TotalDays;
            }
        }

        #endregion

        #region Business Logic Methods - متدهای منطق کسب‌وکار

        /// <summary>
        /// محاسبه مبلغ پرداخت بیمه
        /// </summary>
        public decimal CalculateInsuranceAmount(decimal totalAmount)
        {
            return totalAmount * (CoveragePercent / 100);
        }

        /// <summary>
        /// محاسبه مبلغ پرداخت بیمار
        /// </summary>
        public decimal CalculatePatientAmount(decimal totalAmount)
        {
            var insuranceAmount = CalculateInsuranceAmount(totalAmount);
            var patientAmount = totalAmount - insuranceAmount;
            
            // Apply deductible
            if (patientAmount > 0 && Deductible > 0)
            {
                patientAmount = Math.Max(patientAmount, Deductible);
            }
            
            return patientAmount;
        }

        /// <summary>
        /// بررسی امکان حذف
        /// </summary>
        public bool CanBeDeleted()
        {
            // Check if [Module] is in use
            return PatientInsurances?.Any(x => !x.IsDeleted) != true;
        }

        /// <summary>
        /// بررسی امکان تغییر وضعیت
        /// </summary>
        public bool CanToggleStatus()
        {
            return !IsDeleted;
        }

        /// <summary>
        /// بررسی امکان ویرایش
        /// </summary>
        public bool CanBeEdited()
        {
            return !IsDeleted;
        }

        #endregion

        #region Override Methods - متدهای Override

        /// <summary>
        /// نمایش رشته [Module]
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Code})";
        }

        /// <summary>
        /// مقایسه [Module]ها
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is [Module] other)
            {
                return Id == other.Id;
            }
            return false;
        }

        /// <summary>
        /// هش کد [Module]
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}
