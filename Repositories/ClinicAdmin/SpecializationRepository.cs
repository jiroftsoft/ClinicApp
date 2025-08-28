using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities;

namespace ClinicApp.Repositories.ClinicAdmin
{
    /// <summary>
    /// پیاده‌سازی اینترفیس ISpecializationRepository برای مدیریت عملیات تخصص‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل عملیات CRUD با Entity Framework 6
    /// 2. مدیریت رابطه Many-to-Many با پزشکان
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete)
    /// 4. مدیریت ردیابی (Audit Trail)
    /// 5. پشتیبانی از ترتیب نمایش (DisplayOrder)
    /// </summary>
    public class SpecializationRepository : ISpecializationRepository
    {
        private readonly ApplicationDbContext _context;

        public SpecializationRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Core CRUD Operations

        /// <summary>
        /// دریافت تخصص بر اساس شناسه
        /// </summary>
        public async Task<Specialization> GetByIdAsync(int specializationId)
        {
            try
            {
                return await _context.Specializations
                    .Where(s => s.SpecializationId == specializationId && !s.IsDeleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت تخصص با شناسه {specializationId}", ex);
            }
        }

        /// <summary>
        /// دریافت تمام تخصص‌های فعال
        /// </summary>
        public async Task<List<Specialization>> GetActiveSpecializationsAsync()
        {
            try
            {
                return await _context.Specializations
                    .Where(s => s.IsActive && !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("خطا در دریافت تخصص‌های فعال", ex);
            }
        }

        /// <summary>
        /// دریافت تمام تخصص‌ها (فعال و غیرفعال)
        /// </summary>
        public async Task<List<Specialization>> GetAllSpecializationsAsync()
        {
            try
            {
                return await _context.Specializations
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("خطا در دریافت تمام تخصص‌ها", ex);
            }
        }

        /// <summary>
        /// بررسی وجود تخصص
        /// </summary>
        public async Task<bool> DoesSpecializationExistAsync(int specializationId)
        {
            try
            {
                return await _context.Specializations
                    .AnyAsync(s => s.SpecializationId == specializationId && !s.IsDeleted);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در بررسی وجود تخصص با شناسه {specializationId}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود نام تخصص
        /// </summary>
        public async Task<bool> DoesSpecializationNameExistAsync(string name, int? excludeSpecializationId = null)
        {
            try
            {
                var query = _context.Specializations
                    .Where(s => s.Name == name && !s.IsDeleted);

                if (excludeSpecializationId.HasValue)
                {
                    query = query.Where(s => s.SpecializationId != excludeSpecializationId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در بررسی وجود نام تخصص: {name}", ex);
            }
        }

        /// <summary>
        /// دریافت تعداد پزشکان فعال مرتبط با تخصص
        /// </summary>
        public async Task<int> GetActiveDoctorsCountAsync(int specializationId)
        {
            try
            {
                return await _context.DoctorSpecializations
                    .Where(ds => ds.SpecializationId == specializationId)
                    .Include(ds => ds.Doctor)
                    .Where(ds => ds.Doctor.IsActive && !ds.Doctor.IsDeleted)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت تعداد پزشکان فعال برای تخصص {specializationId}", ex);
            }
        }

        /// <summary>
        /// افزودن تخصص جدید
        /// </summary>
        public async Task<Specialization> AddAsync(Specialization specialization)
        {
            try
            {
                if (specialization == null)
                    throw new ArgumentNullException(nameof(specialization));

                // تنظیم مقادیر پیش‌فرض
                specialization.CreatedAt = DateTime.Now;
                specialization.IsDeleted = false;
                specialization.IsActive = true;

                _context.Specializations.Add(specialization);
                await _context.SaveChangesAsync();

                return specialization;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("خطا در افزودن تخصص جدید", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی تخصص موجود
        /// </summary>
        public async Task<Specialization> UpdateAsync(Specialization specialization)
        {
            try
            {
                if (specialization == null)
                    throw new ArgumentNullException(nameof(specialization));

                var existingSpecialization = await _context.Specializations
                    .FirstOrDefaultAsync(s => s.SpecializationId == specialization.SpecializationId && !s.IsDeleted);

                if (existingSpecialization == null)
                    throw new InvalidOperationException($"تخصص با شناسه {specialization.SpecializationId} یافت نشد");

                // به‌روزرسانی فیلدها
                existingSpecialization.Name = specialization.Name;
                existingSpecialization.Description = specialization.Description;
                existingSpecialization.IsActive = specialization.IsActive;
                existingSpecialization.DisplayOrder = specialization.DisplayOrder;
                existingSpecialization.UpdatedAt = DateTime.Now;
                existingSpecialization.UpdatedByUserId = specialization.UpdatedByUserId;

                await _context.SaveChangesAsync();

                return existingSpecialization;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در به‌روزرسانی تخصص با شناسه {specialization.SpecializationId}", ex);
            }
        }

        /// <summary>
        /// حذف نرم تخصص
        /// </summary>
        public async Task<bool> SoftDeleteAsync(int specializationId, string deletedByUserId)
        {
            try
            {
                var specialization = await _context.Specializations
                    .FirstOrDefaultAsync(s => s.SpecializationId == specializationId && !s.IsDeleted);

                if (specialization == null)
                    return false;

                specialization.IsDeleted = true;
                specialization.DeletedAt = DateTime.Now;
                specialization.DeletedByUserId = deletedByUserId;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در حذف نرم تخصص با شناسه {specializationId}", ex);
            }
        }

        /// <summary>
        /// بازیابی تخصص حذف شده
        /// </summary>
        public async Task<bool> RestoreAsync(int specializationId, string restoredByUserId)
        {
            try
            {
                var specialization = await _context.Specializations
                    .FirstOrDefaultAsync(s => s.SpecializationId == specializationId && s.IsDeleted);

                if (specialization == null)
                    return false;

                specialization.IsDeleted = false;
                specialization.DeletedAt = null;
                specialization.DeletedByUserId = null;
                specialization.UpdatedAt = DateTime.Now;
                specialization.UpdatedByUserId = restoredByUserId;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در بازیابی تخصص با شناسه {specializationId}", ex);
            }
        }

        #endregion

        #region Doctor-Specialization Relationship

        /// <summary>
        /// دریافت تخصص‌های یک پزشک
        /// </summary>
        public async Task<List<Specialization>> GetDoctorSpecializationsAsync(int doctorId)
        {
            try
            {
                var specializations = await _context.DoctorSpecializations
                    .Where(ds => ds.DoctorId == doctorId)
                    .Include(ds => ds.Specialization)
                    .Select(ds => ds.Specialization)
                    .Where(s => !s.IsDeleted)
                    .ToListAsync();

                return specializations ?? new List<Specialization>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت تخصص‌های پزشک با شناسه {doctorId}", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی تخصص‌های یک پزشک
        /// </summary>
        public async Task<bool> UpdateDoctorSpecializationsAsync(int doctorId, List<int> specializationIds)
        {
            try
            {
                // حذف تخصص‌های قبلی
                var existingSpecializations = await _context.DoctorSpecializations
                    .Where(ds => ds.DoctorId == doctorId)
                    .ToListAsync();

                _context.DoctorSpecializations.RemoveRange(existingSpecializations);

                // افزودن تخصص‌های جدید
                if (specializationIds != null && specializationIds.Any())
                {
                    var newSpecializations = specializationIds.Select(specializationId => new DoctorSpecialization
                    {
                        DoctorId = doctorId,
                        SpecializationId = specializationId
                    });

                    _context.DoctorSpecializations.AddRange(newSpecializations);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// دریافت تخصص‌ها بر اساس لیست شناسه‌ها
        /// </summary>
        public async Task<List<Specialization>> GetSpecializationsByIdsAsync(List<int> specializationIds)
        {
            try
            {
                return await _context.Specializations
                    .Where(s => specializationIds.Contains(s.SpecializationId) && !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("خطا در دریافت تخصص‌ها بر اساس شناسه‌ها", ex);
            }
        }

        #endregion
    }
}
