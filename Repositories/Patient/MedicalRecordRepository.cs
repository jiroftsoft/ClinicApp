using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Repositories;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Repositories.Base;
using Serilog;

namespace ClinicApp.Repositories.Patient
{
    /// <summary>
    /// Repository برای دسترسی به داده‌های EMR
    /// Single Responsibility: فقط Data Access
    /// </summary>
    public class MedicalRecordRepository : BaseRepository<MedicalHistory>, IMedicalRecordRepository
    {
        public MedicalRecordRepository(ApplicationDbContext context, ILogger logger) 
            : base(context, logger)
        {
        }
        
        /// <summary>
        /// دریافت تاریخچه پزشکی بیمار
        /// </summary>
        public async Task<List<MedicalHistory>> GetMedicalHistoriesByPatientIdAsync(
            int patientId, bool includeDeleted = false)
        {
            try
            {
                _logger.Debug("دریافت تاریخچه پزشکی - PatientId: {PatientId}, IncludeDeleted: {IncludeDeleted}", 
                    patientId, includeDeleted);
                
                var query = _dbSet.Where(mh => mh.PatientId == patientId);
                
                if (!includeDeleted)
                {
                    query = query.Where(mh => !mh.IsDeleted);
                }
                
                var result = await query
                    .OrderByDescending(mh => mh.StartDate ?? mh.CreatedAt)
                    .ToListAsync();
                
                _logger.Debug("تعداد {Count} تاریخچه پزشکی یافت شد - PatientId: {PatientId}", 
                    result.Count, patientId);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تاریخچه پزشکی - PatientId: {PatientId}", patientId);
                throw;
            }
        }
        
        /// <summary>
        /// دریافت تاریخچه پزشکی با شناسه
        /// </summary>
        public async Task<MedicalHistory> GetMedicalHistoryByIdAsync(int medicalHistoryId)
        {
            try
            {
                _logger.Debug("دریافت تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}", medicalHistoryId);
                
                var result = await _dbSet
                    .FirstOrDefaultAsync(mh => mh.MedicalHistoryId == medicalHistoryId && !mh.IsDeleted);
                
                if (result == null)
                {
                    _logger.Warning("تاریخچه پزشکی یافت نشد - MedicalHistoryId: {MedicalHistoryId}", medicalHistoryId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}", medicalHistoryId);
                throw;
            }
        }
        
        /// <summary>
        /// ایجاد تاریخچه پزشکی جدید
        /// </summary>
        public async Task<MedicalHistory> CreateMedicalHistoryAsync(MedicalHistory entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));
                
                _logger.Debug("ایجاد تاریخچه پزشکی - PatientId: {PatientId}, Title: {Title}", 
                    entity.PatientId, entity.Title);
                
                _dbSet.Add(entity);
                await _context.SaveChangesAsync();
                
                _logger.Information("تاریخچه پزشکی با موفقیت ایجاد شد - MedicalHistoryId: {MedicalHistoryId}, PatientId: {PatientId}", 
                    entity.MedicalHistoryId, entity.PatientId);
                
                return entity;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تاریخچه پزشکی - PatientId: {PatientId}", entity?.PatientId);
                throw;
            }
        }
        
        /// <summary>
        /// به‌روزرسانی تاریخچه پزشکی
        /// </summary>
        public async Task<MedicalHistory> UpdateMedicalHistoryAsync(MedicalHistory entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));
                
                _logger.Debug("به‌روزرسانی تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}", entity.MedicalHistoryId);
                
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                _logger.Information("تاریخچه پزشکی با موفقیت به‌روزرسانی شد - MedicalHistoryId: {MedicalHistoryId}", 
                    entity.MedicalHistoryId);
                
                return entity;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}", entity?.MedicalHistoryId);
                throw;
            }
        }
        
        /// <summary>
        /// حذف نرم تاریخچه پزشکی
        /// </summary>
        public async Task<bool> DeleteMedicalHistoryAsync(int medicalHistoryId, string deletedByUserId)
        {
            try
            {
                _logger.Debug("حذف نرم تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}", medicalHistoryId);
                
                var entity = await GetMedicalHistoryByIdAsync(medicalHistoryId);
                if (entity == null)
                {
                    _logger.Warning("تاریخچه پزشکی برای حذف یافت نشد - MedicalHistoryId: {MedicalHistoryId}", medicalHistoryId);
                    return false;
                }
                
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.Now;
                entity.DeletedByUserId = deletedByUserId;
                
                await _context.SaveChangesAsync();
                
                _logger.Information("تاریخچه پزشکی با موفقیت حذف شد - MedicalHistoryId: {MedicalHistoryId}", medicalHistoryId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}", medicalHistoryId);
                throw;
            }
        }
    }
}

