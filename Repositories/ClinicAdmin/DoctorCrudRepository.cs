using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Repositories.ClinicAdmin
{
    /// <summary>
    /// پیاده‌سازی اینترفیس IDoctorCrudRepository برای مدیریت عملیات CRUD پزشکان
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل عملیات CRUD با Entity Framework 6
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت پزشکان
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorCrudRepository : IDoctorCrudRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorCrudRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Core CRUD Operations (عملیات اصلی CRUD)

        /// <summary>
        /// دریافت پزشک بر اساس شناسه
        /// </summary>
        public async Task<Doctor> GetByIdAsync(int doctorId)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
                    .Include(d => d.DoctorSpecializations)
                    .Include(d => d.DoctorSpecializations.Select(ds => ds.Specialization))
                    .FirstOrDefaultAsync();



                return doctor;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت پزشک با شناسه {doctorId}", ex);
            }
        }

        /// <summary>
        /// دریافت پزشک بر اساس شناسه همراه با تمام روابط برای نمایش جزئیات
        /// </summary>
        public async Task<Doctor> GetByIdWithDetailsAsync(int doctorId)
        {
            try
            {
                return await _context.Doctors
                    .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
                    .Include(d => d.DoctorSpecializations.Select(ds => ds.Specialization))
                    .Include(d => d.DoctorDepartments.Select(dd => dd.Department))
                    .Include(d => d.DoctorServiceCategories.Select(dsc => dsc.ServiceCategory))
                    .Include(d => d.Schedules)
                    .Include(d => d.CreatedByUser)
                    .Include(d => d.UpdatedByUser)
                    .Include(d => d.DeletedByUser)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت جزئیات پزشک با شناسه {doctorId}", ex);
            }
        }

        /// <summary>
        /// دریافت پزشک بر اساس شناسه همراه با تخصص‌ها
        /// </summary>
        public async Task<Doctor> GetByIdWithSpecializationsAsync(int doctorId)
        {
            try
            {
                return await _context.Doctors
                    .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
                    .Include(d => d.DoctorSpecializations)
                    .Include(d => d.DoctorSpecializations.Select(ds => ds.Specialization))
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت پزشک با تخصص‌ها برای شناسه {doctorId}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود پزشک
        /// </summary>
        public async Task<bool> DoesDoctorExistAsync(int doctorId)
        {
            try
            {
                return await _context.Doctors
                    .AnyAsync(d => d.DoctorId == doctorId && !d.IsDeleted);
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در بررسی وجود پزشک با شناسه {doctorId}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود کد نظام پزشکی
        /// </summary>
        public async Task<bool> DoesMedicalCouncilCodeExistAsync(string medicalCouncilCode, int? excludeDoctorId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(medicalCouncilCode))
                    return false;

                var query = _context.Doctors
                    .Where(d => d.MedicalCouncilCode == medicalCouncilCode && !d.IsDeleted);

                if (excludeDoctorId.HasValue)
                    query = query.Where(d => d.DoctorId != excludeDoctorId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در بررسی وجود کد نظام پزشکی {medicalCouncilCode}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود کد ملی
        /// </summary>
        public async Task<bool> DoesNationalCodeExistAsync(string nationalCode, int? excludeDoctorId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nationalCode))
                    return false;

                var query = _context.Doctors
                    .Where(d => d.NationalCode == nationalCode && !d.IsDeleted);

                if (excludeDoctorId.HasValue)
                    query = query.Where(d => d.DoctorId != excludeDoctorId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در بررسی وجود کد ملی {nationalCode}", ex);
            }
        }

        /// <summary>
        /// افزودن پزشک جدید
        /// </summary>
        public async Task<Doctor> AddAsync(Doctor doctor)
        {
            try
            {
                if (doctor == null)
                    throw new ArgumentNullException(nameof(doctor));

                // تنظیم مقادیر پیش‌فرض
                doctor.CreatedAt = DateTime.Now;
                doctor.IsDeleted = false;
                doctor.IsActive = true;

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                return doctor;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException("خطا در افزودن پزشک جدید", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی پزشک موجود
        /// </summary>
        public async Task<Doctor> UpdateAsync(Doctor doctor)
        {
            try
            {
                if (doctor == null)
                    throw new ArgumentNullException(nameof(doctor));

                var existingDoctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.DoctorId == doctor.DoctorId && !d.IsDeleted);

                if (existingDoctor == null)
                    throw new InvalidOperationException($"پزشک با شناسه {doctor.DoctorId} یافت نشد");

                // به‌روزرسانی فیلدها
                existingDoctor.FirstName = doctor.FirstName;
                existingDoctor.LastName = doctor.LastName;
                existingDoctor.NationalCode = doctor.NationalCode;
                existingDoctor.MedicalCouncilCode = doctor.MedicalCouncilCode;
                existingDoctor.PhoneNumber = doctor.PhoneNumber;
                existingDoctor.Email = doctor.Email;
                existingDoctor.Degree = doctor.Degree;
                existingDoctor.GraduationYear = doctor.GraduationYear;
                existingDoctor.University = doctor.University;
                existingDoctor.Gender = doctor.Gender;
                existingDoctor.DateOfBirth = doctor.DateOfBirth;
                existingDoctor.HomeAddress = doctor.HomeAddress;
                existingDoctor.OfficeAddress = doctor.OfficeAddress;

                existingDoctor.ExperienceYears = doctor.ExperienceYears;
                existingDoctor.ProfileImageUrl = doctor.ProfileImageUrl;
                existingDoctor.NationalCode = doctor.NationalCode;
                existingDoctor.MedicalCouncilCode = doctor.MedicalCouncilCode;
                existingDoctor.IsActive = doctor.IsActive;
                existingDoctor.UpdatedAt = DateTime.Now;
                existingDoctor.UpdatedByUserId = doctor.UpdatedByUserId;

                await _context.SaveChangesAsync();

                return existingDoctor;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در به‌روزرسانی پزشک با شناسه {doctor.DoctorId}", ex);
            }
        }

        /// <summary>
        /// حذف نرم پزشک
        /// </summary>
        public async Task<bool> SoftDeleteAsync(int doctorId, string deletedByUserId)
        {
            try
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.DoctorId == doctorId && !d.IsDeleted);

                if (doctor == null)
                    return false;

                doctor.IsDeleted = true;
                doctor.DeletedAt = DateTime.Now;
                doctor.DeletedByUserId = deletedByUserId;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در حذف نرم پزشک با شناسه {doctorId}", ex);
            }
        }

        /// <summary>
        /// بازیابی پزشک حذف شده
        /// </summary>
        public async Task<bool> RestoreAsync(int doctorId, string restoredByUserId)
        {
            try
            {
                var doctor = await _context.Doctors.FindAsync(doctorId);
                if (doctor == null || !doctor.IsDeleted)
                {
                    return false;
                }

                doctor.IsDeleted = false;
                doctor.DeletedAt = null;
                doctor.DeletedByUserId = null;
                doctor.UpdatedAt = DateTime.Now;
                doctor.UpdatedByUserId = restoredByUserId;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// دریافت پزشک بر اساس کد ملی
        /// </summary>
        public async Task<Doctor> GetByNationalCodeAsync(string nationalCode)
        {
            return await _context.Doctors
                .FirstOrDefaultAsync(d => d.NationalCode == nationalCode && !d.IsDeleted);
        }

        /// <summary>
        /// دریافت پزشک بر اساس کد نظام پزشکی
        /// </summary>
        public async Task<Doctor> GetByMedicalCouncilCodeAsync(string medicalCouncilCode)
        {
            return await _context.Doctors
                .FirstOrDefaultAsync(d => d.MedicalCouncilCode == medicalCouncilCode && !d.IsDeleted);
        }

        #endregion

        #region Lookup & Search (جستجو و لیست‌ها)

        /// <summary>
        /// جستجوی پزشکان بر اساس فیلترهای مختلف
        /// </summary>
        public async Task<List<Doctor>> SearchDoctorsAsync(DoctorSearchViewModel filter)
        {
            try
            {
                var query = _context.Doctors
                    .Where(d => !d.IsDeleted)
                    .Include(d => d.DoctorSpecializations)
                    .Include(d => d.DoctorSpecializations.Select(ds => ds.Specialization))
                    .AsQueryable();

                // اعمال فیلترهای جستجو
                if (!string.IsNullOrWhiteSpace(filter?.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim();
                    query = query.Where(d =>
                        d.FirstName.Contains(searchTerm) ||
                        d.LastName.Contains(searchTerm) ||
                        d.NationalCode.Contains(searchTerm) ||
                        d.MedicalCouncilCode.Contains(searchTerm)
                    );
                }

                if (filter?.ClinicId.HasValue == true)
                {
                    // فیلتر بر اساس کلینیک (از طریق دپارتمان‌ها)
                    query = query.Where(d => d.DoctorDepartments.Any(dd => dd.Department.ClinicId == filter.ClinicId.Value));
                }

                if (filter?.DepartmentId.HasValue == true)
                {
                    // فیلتر بر اساس دپارتمان
                    query = query.Where(d => d.DoctorDepartments.Any(dd => dd.DepartmentId == filter.DepartmentId.Value));
                }

                if (filter?.SpecializationId.HasValue == true)
                {
                    // فیلتر بر اساس تخصص
                    query = query.Where(d => d.DoctorSpecializations.Any(ds => ds.SpecializationId == filter.SpecializationId.Value));
                }

                if (filter?.IsActive.HasValue == true)
                {
                    query = query.Where(d => d.IsActive == filter.IsActive.Value);
                }

                // مرتب‌سازی
                query = query.OrderBy(d => d.LastName)
                            .ThenBy(d => d.FirstName);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در جستجوی پزشکان", ex);
            }
        }

        /// <summary>
        /// دریافت تعداد کل پزشکان
        /// </summary>
        public async Task<int> GetAllDoctorsCountAsync()
        {
            try
            {
                return await _context.Doctors
                    .Where(d => !d.IsDeleted)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException("خطا در شمارش کل پزشکان", ex);
            }
        }

        /// <summary>
        /// دریافت تعداد پزشکان فعال
        /// </summary>
        public async Task<int> GetActiveDoctorsCountAsync()
        {
            try
            {
                return await _context.Doctors
                    .Where(d => d.IsActive && !d.IsDeleted)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException("خطا در شمارش پزشکان فعال", ex);
            }
        }

        #endregion
    }
}
