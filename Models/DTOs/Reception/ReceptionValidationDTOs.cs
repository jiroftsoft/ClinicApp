using System;
using System.Collections.Generic;

namespace ClinicApp.Models.DTOs.Reception
{
    /// <summary>
    /// DTOs برای اعتبارسنجی پذیرش
    /// </summary>
    
    #region Insurance Validation DTOs

    public class InsuranceValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string InsuranceType { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal CoveragePercent { get; set; }
        public decimal Deductible { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class PrimaryInsuranceValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string InsuranceName { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal CoveragePercent { get; set; }
        public decimal Deductible { get; set; }
        public bool IsActive { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class SupplementaryInsuranceValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string InsuranceName { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal CoveragePercent { get; set; }
        public decimal Deductible { get; set; }
        public bool IsActive { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class InsuranceCoverageResult
    {
        public bool IsCovered { get; set; }
        public string Message { get; set; }
        public decimal CoveragePercent { get; set; }
        public decimal PatientShare { get; set; }
        public decimal InsuranceShare { get; set; }
        public List<string> Limitations { get; set; } = new List<string>();
    }

    #endregion

    #region Patient Validation DTOs

    public class PatientDebtResult
    {
        public bool HasDebt { get; set; }
        public decimal TotalDebt { get; set; }
        public int DebtCount { get; set; }
        public List<DebtDetail> DebtDetails { get; set; } = new List<DebtDetail>();
        public string Message { get; set; }
        public bool CanProceed { get; set; }
    }

    public class DebtDetail
    {
        public int ReceptionId { get; set; }
        public DateTime ReceptionDate { get; set; }
        public decimal Amount { get; set; }
        public string ServiceName { get; set; }
        public string DoctorName { get; set; }
    }

    public class PatientLimitationResult
    {
        public bool HasLimitations { get; set; }
        public List<string> Limitations { get; set; } = new List<string>();
        public string Message { get; set; }
        public bool CanProceed { get; set; }
    }

    public class PatientMedicalHistoryResult
    {
        public bool HasHistory { get; set; }
        public List<MedicalHistoryItem> HistoryItems { get; set; } = new List<MedicalHistoryItem>();
        public string Message { get; set; }
        public bool RequiresAttention { get; set; }
    }

    public class MedicalHistoryItem
    {
        public DateTime Date { get; set; }
        public string ServiceName { get; set; }
        public string DoctorName { get; set; }
        public string Diagnosis { get; set; }
        public string Notes { get; set; }
    }

    #endregion

    #region Doctor Validation DTOs

    public class DoctorCapacityResult
    {
        public bool HasCapacity { get; set; }
        public int AvailableSlots { get; set; }
        public int TotalSlots { get; set; }
        public List<TimeSlot> AvailableTimeSlots { get; set; } = new List<TimeSlot>();
        public string Message { get; set; }
    }

    public class TimeSlot
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class DoctorSpecializationResult
    {
        public bool IsSpecialized { get; set; }
        public string Specialization { get; set; }
        public string Message { get; set; }
        public bool CanProvideService { get; set; }
    }

    public class DoctorAvailabilityResult
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; }
        public List<DateTime> AvailableDates { get; set; } = new List<DateTime>();
        public List<TimeSpan> AvailableTimes { get; set; } = new List<TimeSpan>();
    }

    #endregion

    #region Service Validation DTOs

    public class ServiceValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string ServiceName { get; set; }
        public decimal Tariff { get; set; }
        public bool IsActive { get; set; }
        public List<string> Requirements { get; set; } = new List<string>();
    }

    public class ServiceDependencyResult
    {
        public bool HasDependencies { get; set; }
        public List<ServiceDependency> Dependencies { get; set; } = new List<ServiceDependency>();
        public string Message { get; set; }
        public bool CanProceed { get; set; }
    }

    public class ServiceDependency
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public bool IsRequired { get; set; }
        public string Description { get; set; }
    }

    public class ServiceLimitationResult
    {
        public bool HasLimitations { get; set; }
        public List<string> Limitations { get; set; } = new List<string>();
        public string Message { get; set; }
        public bool CanProceed { get; set; }
    }

    #endregion

    #region Time Validation DTOs

    public class TimeValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class TimeConflictResult
    {
        public bool HasConflict { get; set; }
        public string Message { get; set; }
        public List<ConflictDetail> Conflicts { get; set; } = new List<ConflictDetail>();
    }

    public class ConflictDetail
    {
        public int ReceptionId { get; set; }
        public string PatientName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    #endregion

    #region Comprehensive Validation DTOs

    public class ComprehensiveValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public InsuranceValidationResult InsuranceValidation { get; set; }
        public PatientDebtResult PatientDebt { get; set; }
        public DoctorCapacityResult DoctorCapacity { get; set; }
        public ServiceValidationResult ServiceValidation { get; set; }
        public TimeValidationResult TimeValidation { get; set; }
        public List<string> AllWarnings { get; set; } = new List<string>();
        public List<string> AllErrors { get; set; } = new List<string>();
    }

    public class RealTimeValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public DateTime ValidationTime { get; set; }
        public List<ValidationItem> ValidationItems { get; set; } = new List<ValidationItem>();
    }

    public class ValidationItem
    {
        public string Type { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion
}
