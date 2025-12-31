using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Patient.MedicalRecord;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Service interface برای مدیریت پرونده الکترونیک بیمار
    /// Single Responsibility: تعریف قرارداد Business Logic
    /// </summary>
    public interface IPatientMedicalRecordService
    {
        /// <summary>
        /// دریافت پرونده الکترونیک بیمار
        /// ✅ ServiceResult Enhanced
        /// </summary>
        Task<ServiceResult<MedicalRecordIndexViewModel>> GetMedicalRecordAsync(int patientId);
        
        /// <summary>
        /// دریافت تاریخچه پزشکی بیمار
        /// ✅ ServiceResult Enhanced
        /// </summary>
        Task<ServiceResult<List<MedicalHistoryViewModel>>> GetMedicalHistoriesAsync(int patientId);
        
        /// <summary>
        /// دریافت تاریخچه پزشکی با شناسه
        /// ✅ ServiceResult Enhanced
        /// </summary>
        Task<ServiceResult<MedicalHistoryViewModel>> GetMedicalHistoryByIdAsync(
            int medicalHistoryId, int patientId);
        
        /// <summary>
        /// ایجاد تاریخچه پزشکی جدید
        /// ✅ ServiceResult Enhanced
        /// </summary>
        Task<ServiceResult> CreateMedicalHistoryAsync(
            MedicalHistoryCreateEditViewModel model, int patientId);
        
        /// <summary>
        /// به‌روزرسانی تاریخچه پزشکی
        /// ✅ ServiceResult Enhanced
        /// </summary>
        Task<ServiceResult> UpdateMedicalHistoryAsync(
            MedicalHistoryCreateEditViewModel model, int patientId);
        
        /// <summary>
        /// حذف تاریخچه پزشکی
        /// ✅ ServiceResult Enhanced
        /// </summary>
        Task<ServiceResult> DeleteMedicalHistoryAsync(int medicalHistoryId, int patientId);
        
        /// <summary>
        /// دریافت نوبت‌های پزشکی بیمار
        /// ✅ ServiceResult Enhanced
        /// </summary>
        Task<ServiceResult<List<MedicalRecordAppointmentViewModel>>> GetAppointmentsAsync(
            int patientId, int pageNumber = 1, int pageSize = 10);
        
        /// <summary>
        /// دریافت پذیرش‌های بیمار
        /// ✅ ServiceResult Enhanced
        /// </summary>
        Task<ServiceResult<List<MedicalRecordReceptionViewModel>>> GetReceptionsAsync(
            int patientId, int pageNumber = 1, int pageSize = 10);
        
        /// <summary>
        /// دریافت ارزیابی‌های تریاژ بیمار
        /// ✅ ServiceResult Enhanced
        /// </summary>
        Task<ServiceResult<List<MedicalRecordTriageViewModel>>> GetTriageAssessmentsAsync(
            int patientId, int pageNumber = 1, int pageSize = 10);
    }
}

