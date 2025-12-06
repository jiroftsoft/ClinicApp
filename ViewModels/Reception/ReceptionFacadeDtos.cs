using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    #region Load DTOs

    /// <summary>
    /// DTO برای بارگذاری اولیه فرم پذیرش
    /// </summary>
    public class ReceptionLoadDto
    {
        public List<ClinicDto> Clinics { get; set; } = new List<ClinicDto>();
        public List<DepartmentDto> Departments { get; set; } = new List<DepartmentDto>();
        public List<ServiceDto> Services { get; set; } = new List<ServiceDto>();
        public List<ServiceDto> SharedServices { get; set; } = new List<ServiceDto>();
        public List<DoctorDto> Doctors { get; set; } = new List<DoctorDto>();
        public FactorSettingDto FactorSetting { get; set; }
        public List<PosTerminalDto> PosTerminals { get; set; } = new List<PosTerminalDto>();
        public int? DefaultPosTerminalId { get; set; }
    }

    /// <summary>
    /// DTO برای تنظیمات ضرایب (FactorSetting)
    /// </summary>
    public class FactorSettingDto
    {
        public int FinancialYear { get; set; }
        public decimal? TechnicalFactor { get; set; }
        public decimal? TechnicalFactorHashtagged { get; set; }
        public decimal? ProfessionalFactor { get; set; }
        public decimal? ProfessionalFactorHashtagged { get; set; }
        public bool IsActive { get; set; }
        public bool IsFrozen { get; set; }
    }

    /// <summary>
    /// DTO برای بیمار
    /// </summary>
    public class PatientDto
    {
        public int PatientId { get; set; }
        public string NationalCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
    }

    /// <summary>
    /// DTO برای ایجاد بیمار
    /// </summary>
    public class PatientCreateDto
    {
        public string NationalCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
    }

    /// <summary>
    /// DTO برای بسته بیمه‌های بیمار
    /// </summary>
    public class InsuranceBundleDto
    {
        public int PatientId { get; set; }
        public List<InsuranceDto> BaseInsurances { get; set; } = new List<InsuranceDto>();
        public List<InsuranceDto> SupplementaryInsurances { get; set; } = new List<InsuranceDto>();
    }

    /// <summary>
    /// DTO برای بیمه
    /// </summary>
    public class InsuranceDto
    {
        public int InsuranceId { get; set; }
        public string InsuranceName { get; set; }
        public string InsuranceType { get; set; }
        public decimal CoveragePercentage { get; set; }
        public decimal Franchise { get; set; }
        public decimal Ceiling { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    #endregion

    #region Service DTOs

    /// <summary>
    /// DTO برای لیست انتخاب خدمات
    /// </summary>
    public class ServicePickListDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public List<ServicePickItemDto> Services { get; set; } = new List<ServicePickItemDto>();
    }

    /// <summary>
    /// DTO برای آیتم انتخاب خدمات
    /// </summary>
    public class ServicePickItemDto
    {
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// DTO برای محاسبه بیمه یک آیتم - Real-Time
    /// 🚨 PROFESSIONAL: برای محاسبه real-time بیمه در زمان افزودن خدمت
    /// </summary>
    public class ItemInsuranceCalculationDto
    {
        public decimal PrimaryCoverage { get; set; }
        public decimal SupplementaryCoverage { get; set; }
        public decimal TotalInsuranceCoverage { get; set; }
        public decimal PatientShare { get; set; }
        public string CoverageStatus { get; set; } // "پوشش کامل", "پوشش ناقص", "بدون پوشش"
        public decimal PrimaryCoveragePercent { get; set; }
        public decimal SupplementaryCoveragePercent { get; set; }
        public decimal TotalCoveragePercent { get; set; }
    }

    public class AddItemResultDto
    {
        public int ReceptionId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ItemTotal { get; set; }
        /// <summary>
        /// 🚨 PROFESSIONAL: محاسبه real-time بیمه برای این آیتم
        /// </summary>
        public ItemInsuranceCalculationDto InsuranceCalculation { get; set; }
        public ReceptionTotalsDto ReceptionTotals { get; set; }
    }

    #endregion

    #region Payment DTOs

    // DTO های PosPaymentDto و CashPaymentDto در ReceptionDraftDtos.cs تعریف شده‌اند

    /// <summary>
    /// DTO برای نتیجه نهایی‌سازی
    /// </summary>
    public class FinalizeResultDto
    {
        public int ReceptionId { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime FinalizedAt { get; set; }
        public string ReceptionNumber { get; set; }
    }

    #endregion

    #region Department & Doctor DTOs

    /// <summary>
    /// DTO برای دپارتمان
    /// </summary>
    public class DepartmentDto
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// DTO برای پزشک
    /// </summary>
    public class DoctorDto
    {
        public int DoctorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Name => FullName; // Alias برای سازگاری با frontend
        public string DoctorCode { get; set; }
        public string Specialization { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO برای کلینیک
    /// </summary>
    public class ClinicDto
    {
        public int ClinicId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO برای خدمت
    /// </summary>
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool IsHashtagged { get; set; }
    }

    /// <summary>
    /// DTO برای ترمینال POS
    /// </summary>
    public class PosTerminalDto
    {
        public int PosTerminalId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string TerminalId { get; set; }
        public string MerchantId { get; set; }
        public string SerialNumber { get; set; }
        public string IpAddress { get; set; }
        public int? Port { get; set; }
        public string MacAddress { get; set; }
        public Models.Enums.PosProviderType Provider { get; set; }
        public Models.Enums.PosProviderType ProviderType { get; set; }
        public Models.Enums.PosProtocol Protocol { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public string ConnectionString { get; set; }
        public string Description { get; set; }
    }

    #endregion

    #region Totals DTOs

    /// <summary>
    /// DTO برای مجموع‌های پذیرش
    /// </summary>
    public class ReceptionTotalsDto
    {
        public decimal GrossAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DeductionAmount { get; set; }
        public decimal BaseInsurancePayable { get; set; }
        public decimal SupplementaryInsurancePayable { get; set; }
        public decimal PatientPayable { get; set; }
        public decimal NetAmount => GrossAmount - DiscountAmount + DeductionAmount;
        public decimal TotalInsurancePayable => BaseInsurancePayable + SupplementaryInsurancePayable;
    }

    #endregion

    #region Edit DTOs

    /// <summary>
    /// DTO برای بارگذاری پذیرش برای ویرایش
    /// </summary>
    public class ReceptionEditLoadDto
    {
        public int ReceptionId { get; set; }
        public ReceptionStatus Status { get; set; }
        
        // اطلاعات بیمار
        public int PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientNationalCode { get; set; }
        public string PatientFirstName { get; set; }
        public string PatientLastName { get; set; }
        public string PatientFatherName { get; set; }
        public string PatientGender { get; set; }
        public string PatientBirthDateShamsi { get; set; }
        public string PatientMobile { get; set; }
        public string PatientPhone { get; set; }
        public string PatientAddress { get; set; }
        
        // اطلاعات پزشک و دپارتمان
        public int DoctorId { get; set; }
        public string DoctorFullName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }
        
        // تاریخ پذیرش
        public DateTime ReceptionDate { get; set; }
        public string ReceptionDateShamsi { get; set; }
        
        // بیمه‌ها
        public int? BasePlanId { get; set; }
        public string BasePlanName { get; set; }
        public int? SupplementaryPlanId { get; set; }
        public string SupplementaryPlanName { get; set; }
        
        // خدمات
        public List<ReceptionItemEditDto> Items { get; set; } = new List<ReceptionItemEditDto>();
        
        // مبالغ
        public decimal TotalAmount { get; set; }
        public decimal InsurerShareAmount { get; set; }
        public decimal PatientCoPay { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        
        // یادداشت‌ها و تنظیمات
        public string Notes { get; set; }
        public ReceptionType Type { get; set; }
        public AppointmentPriority Priority { get; set; }
        public bool IsEmergency { get; set; }
        public bool IsOnlineReception { get; set; }
        
        // محدودیت‌های ویرایش
        public EditPermissionsDto Permissions { get; set; }
        
        // لیست‌های کمکی
        public List<DoctorDto> AvailableDoctors { get; set; } = new List<DoctorDto>();
        public List<DepartmentDto> AvailableDepartments { get; set; } = new List<DepartmentDto>();
        public List<ServicePickItemDto> AvailableServices { get; set; } = new List<ServicePickItemDto>();
        public InsuranceBundleDto PatientInsurances { get; set; }
    }

    /// <summary>
    /// DTO برای آیتم پذیرش در ویرایش
    /// </summary>
    public class ReceptionItemEditDto
    {
        public int ReceptionItemId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal PatientShareAmount { get; set; }
        public decimal InsurerShareAmount { get; set; }
        public string SnapshotJson { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// DTO برای مجوزهای ویرایش
    /// </summary>
    public class EditPermissionsDto
    {
        public bool CanEditPatient { get; set; }
        public bool CanEditDoctor { get; set; }
        public bool CanEditDepartment { get; set; }
        public bool CanEditServices { get; set; }
        public bool CanEditInsurances { get; set; }
        public bool CanEditAmounts { get; set; }
        public bool CanEditDate { get; set; }
        public bool CanEditNotes { get; set; }
        public bool RequiresApproval { get; set; }
    }

    /// <summary>
    /// DTO برای درخواست به‌روزرسانی پذیرش
    /// </summary>
    public class UpdateReceptionRequest
    {
        public int ReceptionId { get; set; }
        
        // فیلدهای قابل ویرایش
        public int? DoctorId { get; set; }
        public int? DepartmentId { get; set; }
        public int? ClinicId { get; set; }
        public DateTime? ReceptionDate { get; set; }
        public string ReceptionDateShamsi { get; set; }
        
        // بیمه‌ها
        public int? BasePlanId { get; set; }
        public int? SupplementaryPlanId { get; set; }
        
        // خدمات (لیست تغییرات)
        public List<ReceptionItemUpdateDto> Items { get; set; } = new List<ReceptionItemUpdateDto>();
        
        // یادداشت‌ها و تنظیمات
        public string Notes { get; set; }
        public ReceptionType? Type { get; set; }
        public AppointmentPriority? Priority { get; set; }
        public bool? IsEmergency { get; set; }
        
        // برای بازمحاسبه
        public bool RecalculatePrices { get; set; } = true;
    }

    /// <summary>
    /// DTO برای به‌روزرسانی آیتم پذیرش
    /// </summary>
    public class ReceptionItemUpdateDto
    {
        public int? ReceptionItemId { get; set; } // null = جدید
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public bool IsDeleted { get; set; } // true = حذف
    }

    /// <summary>
    /// DTO برای پاسخ به‌روزرسانی پذیرش
    /// </summary>
    public class UpdateReceptionResponse
    {
        public int ReceptionId { get; set; }
        public ReceptionStatus Status { get; set; }
        public List<ReceptionItemEditDto> Items { get; set; } = new List<ReceptionItemEditDto>();
        public ReceptionTotalsDto Totals { get; set; }
        public bool RequiresApproval { get; set; }
        public string Message { get; set; }
    }

    #endregion

    #region Cancellation DTOs

    /// <summary>
    /// DTO برای درخواست لغو پذیرش
    /// </summary>
    public class CancelReceptionRequest
    {
        public int ReceptionId { get; set; }
        
        [Required(ErrorMessage = "دلیل لغو الزامی است")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "دلیل لغو باید بین 10 تا 500 کاراکتر باشد")]
        public string Reason { get; set; }
        
        public bool ProcessRefund { get; set; }
        
        public string RefundReason { get; set; }
    }

    /// <summary>
    /// DTO برای پاسخ لغو پذیرش
    /// </summary>
    public class CancelReceptionResponse
    {
        public int ReceptionId { get; set; }
        public ReceptionStatus PreviousStatus { get; set; }
        public ReceptionStatus NewStatus { get; set; }
        public bool RefundProcessed { get; set; }
        public decimal? RefundAmount { get; set; }
        public string Message { get; set; }
        public bool RequiresApproval { get; set; }
        public DateTime CancelledAt { get; set; }
        public string CancelledBy { get; set; }
    }

    /// <summary>
    /// DTO برای جزئیات کامل پذیرش (برای نمایش در Modal)
    /// </summary>
    public class ReceptionDetailsFullDto
    {
        // اطلاعات اصلی پذیرش
        public int ReceptionId { get; set; }
        public string ReceptionNo { get; set; }
        public string ElectronicReceptionNumber { get; set; }
        public ReceptionStatus Status { get; set; }
        public string StatusText { get; set; }
        public ReceptionType Type { get; set; }
        public string TypeText { get; set; }
        public AppointmentPriority Priority { get; set; }
        public string PriorityText { get; set; }
        public bool IsEmergency { get; set; }
        public bool IsOnlineReception { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string ReceptionDateShamsi { get; set; }
        public string Notes { get; set; }

        // اطلاعات بیمار
        public int PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientNationalCode { get; set; }
        public string PatientPhoneNumber { get; set; }
        public string PatientGender { get; set; }
        public string PatientBirthDateShamsi { get; set; }
        public string PatientAddress { get; set; }

        // اطلاعات پزشک
        public int DoctorId { get; set; }
        public string DoctorFullName { get; set; }
        public string DoctorSpecialization { get; set; }
        public string DoctorDegree { get; set; }

        // اطلاعات دپارتمان و کلینیک
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }

        // اطلاعات بیمه
        public int? BasePlanId { get; set; }
        public string BasePlanName { get; set; }
        public int? SupplementaryPlanId { get; set; }
        public string SupplementaryPlanName { get; set; }

        // اطلاعات مالی
        public decimal TotalAmount { get; set; }
        public decimal Gross { get; set; }
        public decimal PatientCoPay { get; set; }
        public decimal PatientPay { get; set; }
        public decimal BasePay { get; set; }
        public decimal SuppPay { get; set; }
        public decimal InsurerShareAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool HasDebt => RemainingAmount > 0;

        // آیتم‌های پذیرش
        public List<ReceptionItemDetailsDto> Items { get; set; } = new List<ReceptionItemDetailsDto>();

        // تراکنش‌های پرداخت
        public List<PaymentTransactionDetailsDto> Transactions { get; set; } = new List<PaymentTransactionDetailsDto>();

        // اطلاعات ردیابی
        public DateTime CreatedAt { get; set; }
        public string CreatedAtShamsi { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedAtShamsi { get; set; }
        public string UpdatedBy { get; set; }
    }

    /// <summary>
    /// DTO برای جزئیات آیتم پذیرش
    /// </summary>
    public class ReceptionItemDetailsDto
    {
        public int ReceptionItemId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public decimal PatientShareAmount { get; set; }
        public decimal InsurerShareAmount { get; set; }
        public string SnapshotJson { get; set; }
    }

    /// <summary>
    /// DTO برای جزئیات تراکنش پرداخت
    /// </summary>
    public class PaymentTransactionDetailsDto
    {
        public int PaymentTransactionId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string StatusText { get; set; }
        public PaymentMethod Method { get; set; }
        public string MethodText { get; set; }
        public string TransactionId { get; set; }
        public string ReferenceCode { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtShamsi { get; set; }
        public string CreatedBy { get; set; }
        public int? CashSessionId { get; set; }
        public string CashSessionNumber { get; set; }
    }

    #endregion
}
