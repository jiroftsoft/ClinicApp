using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Entities.Reception;

namespace ClinicApp.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Doctor entity operations
    /// Production-ready with comprehensive operations for reception management
    /// </summary>
    public interface IDoctorRepository
    {
        /// <summary>
        /// Get doctor by ID
        /// </summary>
        /// <param name="doctorId">Doctor ID</param>
        /// <returns>Doctor entity or null if not found</returns>
        Task<Doctor> GetDoctorByIdAsync(int doctorId);

        /// <summary>
        /// Get all active doctors
        /// </summary>
        /// <returns>List of active doctors</returns>
        Task<List<Doctor>> GetAllDoctorsAsync();

        /// <summary>
        /// Get doctors by specialization
        /// </summary>
        /// <param name="specializationId">Specialization ID</param>
        /// <returns>List of doctors with specified specialization</returns>
        Task<List<Doctor>> GetDoctorsBySpecializationAsync(int specializationId);

        /// <summary>
        /// Get receptions by doctor and date
        /// </summary>
        /// <param name="doctorId">Doctor ID</param>
        /// <param name="date">Date</param>
        /// <returns>List of receptions</returns>
        Task<List<Models.Entities.Reception.Reception>> GetReceptionsByDoctorAndDateAsync(int doctorId, DateTime date);

        /// <summary>
        /// Check if doctor is available on specific date
        /// </summary>
        /// <param name="doctorId">Doctor ID</param>
        /// <param name="date">Date</param>
        /// <returns>True if available</returns>
        Task<bool> IsDoctorAvailableAsync(int doctorId, DateTime date);

        /// <summary>
        /// Get doctor's daily capacity
        /// </summary>
        /// <param name="doctorId">Doctor ID</param>
        /// <returns>Maximum receptions per day</returns>
        Task<int> GetDoctorDailyCapacityAsync(int doctorId);

        /// <summary>
        /// Get doctor's current reception count for date
        /// </summary>
        /// <param name="doctorId">Doctor ID</param>
        /// <param name="date">Date</param>
        /// <returns>Current reception count</returns>
        Task<int> GetDoctorReceptionCountAsync(int doctorId, DateTime date);
    }
}
