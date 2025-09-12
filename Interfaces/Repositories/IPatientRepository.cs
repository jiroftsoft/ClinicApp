using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Patient;

namespace ClinicApp.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Patient entity operations
    /// Production-ready with comprehensive CRUD operations
    /// </summary>
    public interface IPatientRepository
    {
        /// <summary>
        /// Get patient by ID
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <returns>Patient entity or null if not found</returns>
        Task<Patient> GetPatientByIdAsync(int patientId);

        /// <summary>
        /// Get patient by national code
        /// </summary>
        /// <param name="nationalCode">National code</param>
        /// <returns>Patient entity or null if not found</returns>
        Task<Patient> GetPatientByNationalCodeAsync(string nationalCode);

        /// <summary>
        /// Search patients by keyword (name, national code, phone)
        /// </summary>
        /// <param name="keyword">Search keyword</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of matching patients</returns>
        Task<List<Patient>> SearchPatientsAsync(string keyword, int pageNumber, int pageSize);

        /// <summary>
        /// Create new patient
        /// </summary>
        /// <param name="patient">Patient entity</param>
        /// <returns>Created patient with ID</returns>
        Task<Patient> CreatePatientAsync(Patient patient);

        /// <summary>
        /// Update existing patient
        /// </summary>
        /// <param name="patient">Patient entity</param>
        /// <returns>Updated patient</returns>
        Task<Patient> UpdatePatientAsync(Patient patient);

        /// <summary>
        /// Check if patient exists by national code
        /// </summary>
        /// <param name="nationalCode">National code</param>
        /// <returns>True if exists</returns>
        Task<bool> PatientExistsByNationalCodeAsync(string nationalCode);

        /// <summary>
        /// Get patient count for pagination
        /// </summary>
        /// <param name="keyword">Search keyword</param>
        /// <returns>Total count</returns>
        Task<int> GetPatientCountAsync(string keyword = null);
    }
}
