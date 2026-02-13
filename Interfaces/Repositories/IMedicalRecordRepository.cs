using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Patient;

namespace ClinicApp.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface برای دسترسی به داده‌های EMR
    /// Single Responsibility: تعریف قرارداد Data Access
    /// </summary>
    public interface IMedicalRecordRepository
    {
        /// <summary>
        /// دریافت تاریخچه پزشکی بیمار
        /// </summary>
        Task<List<MedicalHistory>> GetMedicalHistoriesByPatientIdAsync(
            int patientId, bool includeDeleted = false);

        /// <summary>
        /// دریافت تاریخچه پزشکی با صفحه‌بندی و فیلتر (برای پرونده غنی و مقیاس‌پذیر).
        /// </summary>
        Task<(List<MedicalHistory> Items, int TotalCount)> GetMedicalHistoriesPagedAsync(
            int patientId,
            int pageNumber,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate,
            string searchText);
        
        /// <summary>
        /// دریافت تاریخچه پزشکی با شناسه
        /// </summary>
        Task<MedicalHistory> GetMedicalHistoryByIdAsync(int medicalHistoryId);
        
        /// <summary>
        /// ایجاد تاریخچه پزشکی جدید
        /// </summary>
        Task<MedicalHistory> CreateMedicalHistoryAsync(MedicalHistory entity);
        
        /// <summary>
        /// به‌روزرسانی تاریخچه پزشکی
        /// </summary>
        Task<MedicalHistory> UpdateMedicalHistoryAsync(MedicalHistory entity);
        
        /// <summary>
        /// حذف نرم تاریخچه پزشکی
        /// </summary>
        Task<bool> DeleteMedicalHistoryAsync(int medicalHistoryId, string deletedByUserId);
    }
}

