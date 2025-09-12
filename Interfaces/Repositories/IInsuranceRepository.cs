using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Patient;

namespace ClinicApp.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Insurance entity operations
    /// Production-ready with comprehensive insurance management
    /// </summary>
    public interface IInsuranceRepository
    {
        /// <summary>
        /// Get patient insurance by patient and insurance ID
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <param name="insuranceId">Insurance ID</param>
        /// <returns>Patient insurance or null if not found</returns>
        Task<PatientInsurance> GetPatientInsuranceAsync(int patientId, int insuranceId);

        /// <summary>
        /// Get all active insurances for patient
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <returns>List of active patient insurances</returns>
        Task<List<PatientInsurance>> GetPatientActiveInsurancesAsync(int patientId);

        /// <summary>
        /// Get insurance plan by ID
        /// </summary>
        /// <param name="planId">Insurance plan ID</param>
        /// <returns>Insurance plan or null if not found</returns>
        Task<InsurancePlan> GetInsurancePlanByIdAsync(int planId);

        /// <summary>
        /// Get all insurance providers
        /// </summary>
        /// <returns>List of insurance providers</returns>
        Task<List<InsuranceProvider>> GetInsuranceProvidersAsync();

        /// <summary>
        /// Get insurance provider by ID
        /// </summary>
        /// <param name="providerId">Provider ID</param>
        /// <returns>Insurance provider or null if not found</returns>
        Task<InsuranceProvider> GetInsuranceProviderByIdAsync(int providerId);

        /// <summary>
        /// Check if patient insurance is active and valid
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <param name="insuranceId">Insurance ID</param>
        /// <param name="serviceDate">Service date</param>
        /// <returns>True if active and valid</returns>
        Task<bool> IsPatientInsuranceValidAsync(int patientId, int insuranceId, DateTime serviceDate);

        /// <summary>
        /// Get primary insurance for patient
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <returns>Primary patient insurance or null</returns>
        Task<PatientInsurance> GetPatientPrimaryInsuranceAsync(int patientId);

        /// <summary>
        /// Get secondary insurance for patient
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <returns>Secondary patient insurance or null</returns>
        Task<PatientInsurance> GetPatientSecondaryInsuranceAsync(int patientId);
    }
}
