using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
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
                return await _context.Doctors
                    .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
                    .Include(d => d.Specializations)
                    .FirstOrDefaultAsync();
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
                    .Include(d => d.DoctorDepartments.Select(dd => dd.Department))
                    .Include(d => d.DoctorServiceCategories.Select(dsc => dsc.ServiceCategory))
                    .Include(d => d.Schedules)
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
                    .Include(d => d.Specializations)
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
                existingDoctor.ConsultationFee = doctor.ConsultationFee;
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
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.DoctorId == doctorId && d.IsDeleted);

                if (doctor == null)
                    return false;

                doctor.IsDeleted = false;
                doctor.DeletedAt = null;
                doctor.DeletedByUserId = null;
                doctor.UpdatedAt = DateTime.Now;
                doctor.UpdatedByUserId = restoredByUserId;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در بازیابی پزشک با شناسه {doctorId}", ex);
            }
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
                    .Include(d => d.Specializations)
                    .AsQueryable();

                // اعمال فیلترهای جستجو
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim();
                    query = query.Where(d =>
                        d.FirstName.Contains(searchTerm) ||
                        d.LastName.Contains(searchTerm) ||
                        (d.NationalCode != null && d.NationalCode.Contains(searchTerm)) ||
                        (d.MedicalCouncilCode != null && d.MedicalCouncilCode.Contains(searchTerm)) ||
                        (d.University != null && d.University.Contains(searchTerm)) ||
                        d.Specializations.Any(s => s.Name.Contains(searchTerm))
                    );
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(d => d.IsActive == filter.IsActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.Specialization))
                {
                    query = query.Where(d => d.Specializations.Any(s => s.Name.Contains(filter.Specialization)));
                }

                // مرتب‌سازی
                switch (filter.SortBy?.ToLower())
                {
                    case "firstname":
                        query = filter.SortOrder == "desc" 
                            ? query.OrderByDescending(d => d.FirstName)
                            : query.OrderBy(d => d.FirstName);
                        break;
                    case "lastname":
                        query = filter.SortOrder == "desc" 
                            ? query.OrderByDescending(d => d.LastName)
                            : query.OrderBy(d => d.LastName);
                        break;
                    case "specialization":
                        query = filter.SortOrder == "desc" 
                            ? query.OrderByDescending(d => d.Specializations.FirstOrDefault().Name)
                            : query.OrderBy(d => d.Specializations.FirstOrDefault().Name);
                        break;
                    case "createdat":
                        query = filter.SortOrder == "desc" 
                            ? query.OrderByDescending(d => d.CreatedAt)
                            : query.OrderBy(d => d.CreatedAt);
                        break;
                    default:
                        query = query.OrderBy(d => d.FirstName).ThenBy(d => d.LastName);
                        break;
                }

                // صفحه‌بندی
                if (filter.PageNumber > 0 && filter.PageSize > 0)
                {
                    query = query.Skip((filter.PageNumber - 1) * filter.PageSize)
                                .Take(filter.PageSize);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException("خطا در جستجوی پزشکان", ex);
            }
        }

        /// <summary>
        /// دریافت تعداد پزشکان مطابق با فیلترهای جستجو
        /// </summary>
        public async Task<int> GetDoctorsCountAsync(DoctorSearchViewModel filter)
        {
            try
            {
                var query = _context.Doctors
                    .Where(d => !d.IsDeleted)
                    .AsQueryable();

                // اعمال فیلترهای جستجو (بدون صفحه‌بندی)
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim();
                    query = query.Where(d =>
                        d.FirstName.Contains(searchTerm) ||
                        d.LastName.Contains(searchTerm) ||
                        (d.NationalCode != null && d.NationalCode.Contains(searchTerm)) ||
                        (d.MedicalCouncilCode != null && d.MedicalCouncilCode.Contains(searchTerm)) ||
                        (d.University != null && d.University.Contains(searchTerm)) ||
                        d.Specializations.Any(s => s.Name.Contains(searchTerm))
                    );
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(d => d.IsActive == filter.IsActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.Specialization))
                {
                    query = query.Where(d => d.Specializations.Any(s => s.Name.Contains(filter.Specialization)));
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException("خطا در شمارش پزشکان", ex);
            }
        }

        #endregion
    }
}
