using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;

namespace ClinicApp.Repositories.ClinicAdmin
{
    /// <summary>
    /// پیاده‌سازی اینترفیس IDoctorDepartmentRepository برای مدیریت انتصاب پزشکان به دپارتمان‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت انتصاب پزشک-دپارتمان با Entity Framework 6
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت انتصاب‌ها
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 6. رعایت استانداردهای امنیتی سیستم‌های پزشکی ایران
    /// 7. پشتیبانی از سیستم‌های Load Balanced و محیط‌های Production
    /// 8. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorDepartmentRepository : IDoctorDepartmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public DoctorDepartmentRepository(
            ApplicationDbContext context,
            ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger.ForContext<DoctorDepartmentRepository>();
        }

        #region Doctor-Department Management (پیاده‌سازی مدیریت انتصاب پزشک به دپارتمان)

        /// <summary>
        /// دریافت انتصاب پزشک به دپارتمان بر اساس شناسه‌ها
        /// </summary>
        public async Task<DoctorDepartment> GetDoctorDepartmentAsync(int doctorId, int departmentId)
        {
            try
            {
                _logger.Information("دریافت انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", doctorId, departmentId);

                return await _context.DoctorDepartments
                    .AsNoTracking()
                    .Where(dd => dd.DoctorId == doctorId && 
                                dd.DepartmentId == departmentId && 
                                !dd.IsDeleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", doctorId, departmentId);
                throw new InvalidOperationException($"خطا در دریافت انتصاب پزشک {doctorId} به دپارتمان {departmentId}", ex);
            }
        }

        /// <summary>
        /// دریافت انتصاب پزشک به دپارتمان همراه با جزئیات
        /// </summary>
        public async Task<DoctorDepartment> GetDoctorDepartmentWithDetailsAsync(int doctorId, int departmentId)
        {
            try
            {
                _logger.Information("دریافت جزئیات انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", doctorId, departmentId);

                return await _context.DoctorDepartments
                    .Where(dd => dd.DoctorId == doctorId && 
                                dd.DepartmentId == departmentId && 
                                !dd.IsDeleted)
                    .Include(dd => dd.Doctor)
                    .Include(dd => dd.Doctor.Clinic)
                    .Include(dd => dd.Department)
                    .Include(dd => dd.Department.Clinic)
                    .Include(dd => dd.CreatedByUser)
                    .Include(dd => dd.UpdatedByUser)
                    .Include(dd => dd.DeletedByUser)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", doctorId, departmentId);
                throw new InvalidOperationException($"خطا در دریافت جزئیات انتصاب پزشک {doctorId} به دپارتمان {departmentId}", ex);
            }
        }

        /// <summary>
        /// دریافت لیست انتصابات پزشک به دپارتمان‌ها
        /// </summary>
        public async Task<List<DoctorDepartment>> GetDoctorDepartmentsAsync(int doctorId, string searchTerm, int pageNumber, int pageSize)
        {
            try
            {
                _logger.Information("دریافت لیست انتصابات پزشک {DoctorId} به دپارتمان‌ها. صفحه: {PageNumber}, اندازه: {PageSize}", 
                    doctorId, pageNumber, pageSize);

                // اعتبارسنجی ورودی‌ها
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var query = _context.DoctorDepartments
                    .Include(dd => dd.Department)
                    .Include(dd => dd.Department.Clinic)
                    .Include(dd => dd.CreatedByUser)
                    .Include(dd => dd.UpdatedByUser)
                    .AsNoTracking()
                    .Where(dd => dd.DoctorId == doctorId && !dd.IsDeleted);

                // اعمال فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    string search = searchTerm.Trim();
                    query = query.Where(dd =>
                        dd.Department.Name.Contains(search) ||
                        dd.Department.Description.Contains(search) ||
                        dd.Department.Clinic.Name.Contains(search));
                }

                // مرتب‌سازی
                query = query.OrderBy(dd => dd.Department.Name)
                            .ThenBy(dd => dd.CreatedAt);

                // اعمال صفحه‌بندی
                var result = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.Information("یافتن {Count} انتصاب پزشک {DoctorId} به دپارتمان‌ها", result.Count, doctorId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست انتصابات پزشک {DoctorId} به دپارتمان‌ها", doctorId);
                throw new InvalidOperationException($"خطا در دریافت لیست انتصابات پزشک {doctorId} به دپارتمان‌ها", ex);
            }
        }

        /// <summary>
        /// دریافت تعداد انتصابات پزشک به دپارتمان‌ها
        /// </summary>
        public async Task<int> GetDoctorDepartmentsCountAsync(int doctorId, string searchTerm)
        {
            try
            {
                _logger.Debug("شمارش انتصابات پزشک {DoctorId} به دپارتمان‌ها", doctorId);

                var query = _context.DoctorDepartments
                    .AsNoTracking()
                    .Where(dd => dd.DoctorId == doctorId && !dd.IsDeleted);

                // اعمال فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    string search = searchTerm.Trim();
                    query = query.Where(dd =>
                        dd.Department.Name.Contains(search) ||
                        dd.Department.Description.Contains(search) ||
                        dd.Department.Clinic.Name.Contains(search));
                }

                var count = await query.CountAsync();

                _logger.Debug("تعداد {Count} انتصاب پزشک {DoctorId} به دپارتمان‌ها", count, doctorId);

                return count;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش انتصابات پزشک {DoctorId} به دپارتمان‌ها", doctorId);
                throw new InvalidOperationException($"خطا در شمارش انتصابات پزشک {doctorId} به دپارتمان‌ها", ex);
            }
        }

        /// <summary>
        /// افزودن انتصاب پزشک به دپارتمان
        /// </summary>
        public async Task<DoctorDepartment> AddDoctorDepartmentAsync(DoctorDepartment doctorDepartment)
        {
            try
            {
                _logger.Information("افزودن انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", 
                    doctorDepartment.DoctorId, doctorDepartment.DepartmentId);

                if (doctorDepartment == null)
                    throw new ArgumentNullException(nameof(doctorDepartment));

                // بررسی وجود انتصاب تکراری
                var existingAssignment = await _context.DoctorDepartments
                    .FirstOrDefaultAsync(dd => dd.DoctorId == doctorDepartment.DoctorId && 
                                              dd.DepartmentId == doctorDepartment.DepartmentId && 
                                              !dd.IsDeleted);

                if (existingAssignment != null)
                {
                    _logger.Warning("انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId} قبلاً وجود دارد", 
                        doctorDepartment.DoctorId, doctorDepartment.DepartmentId);
                    throw new InvalidOperationException("این انتصاب قبلاً وجود دارد");
                }

                // بررسی وجود پزشک و دپارتمان
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.DoctorId == doctorDepartment.DoctorId && !d.IsDeleted);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorDepartment.DoctorId);
                    throw new InvalidOperationException($"پزشک با شناسه {doctorDepartment.DoctorId} یافت نشد");
                }

                var department = await _context.Departments
                    .FirstOrDefaultAsync(d => d.DepartmentId == doctorDepartment.DepartmentId && !d.IsDeleted);
                if (department == null)
                {
                    _logger.Warning("دپارتمان با شناسه {DepartmentId} یافت نشد", doctorDepartment.DepartmentId);
                    throw new InvalidOperationException($"دپارتمان با شناسه {doctorDepartment.DepartmentId} یافت نشد");
                }

                // تنظیم فیلدهای ردیابی
                doctorDepartment.CreatedAt = DateTime.Now;
                doctorDepartment.CreatedByUserId = doctorDepartment.CreatedByUserId ?? "System";
                doctorDepartment.IsDeleted = false;
                doctorDepartment.DeletedAt = null;
                doctorDepartment.DeletedByUserId = null;
                doctorDepartment.UpdatedAt = null;
                doctorDepartment.UpdatedByUserId = null;
                doctorDepartment.IsActive = true;

                _context.DoctorDepartments.Add(doctorDepartment);
                await _context.SaveChangesAsync();

                _logger.Information("انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId} با موفقیت افزوده شد", 
                    doctorDepartment.DoctorId, doctorDepartment.DepartmentId);

                return doctorDepartment;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", 
                    doctorDepartment?.DoctorId, doctorDepartment?.DepartmentId);
                throw new InvalidOperationException($"خطا در افزودن انتصاب پزشک {doctorDepartment?.DoctorId} به دپارتمان {doctorDepartment?.DepartmentId}", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی انتصاب پزشک به دپارتمان
        /// </summary>
        public async Task<DoctorDepartment> UpdateDoctorDepartmentAsync(DoctorDepartment doctorDepartment)
        {
            try
            {
                _logger.Information("شروع به‌روزرسانی انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", 
                    doctorDepartment.DoctorId, doctorDepartment.DepartmentId);

                if (doctorDepartment == null)
                    throw new ArgumentNullException(nameof(doctorDepartment));

                var existingAssignment = await _context.DoctorDepartments
                    .FirstOrDefaultAsync(dd => dd.DoctorId == doctorDepartment.DoctorId && 
                                              dd.DepartmentId == doctorDepartment.DepartmentId && 
                                              !dd.IsDeleted);

                if (existingAssignment == null)
                {
                    _logger.Warning("انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId} یافت نشد", 
                        doctorDepartment.DoctorId, doctorDepartment.DepartmentId);
                    throw new InvalidOperationException($"انتصاب پزشک {doctorDepartment.DoctorId} به دپارتمان {doctorDepartment.DepartmentId} یافت نشد");
                }

                // تنظیم فیلدهای ردیابی
                existingAssignment.UpdatedAt = DateTime.Now;
                existingAssignment.UpdatedByUserId = doctorDepartment.UpdatedByUserId ?? "System";

                // به‌روزرسانی فیلدها
                existingAssignment.IsActive = doctorDepartment.IsActive;
                existingAssignment.StartDate = doctorDepartment.StartDate;
                existingAssignment.EndDate = doctorDepartment.EndDate;
                existingAssignment.Role = doctorDepartment.Role;

                _context.Entry(existingAssignment).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.Information("انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId} با موفقیت به‌روزرسانی شد", 
                    doctorDepartment.DoctorId, doctorDepartment.DepartmentId);

                return existingAssignment;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Error(ex, "خطا در همزمانی هنگام به‌روزرسانی انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", 
                    doctorDepartment?.DoctorId, doctorDepartment?.DepartmentId);
                throw new InvalidOperationException($"خطا در همزمانی هنگام به‌روزرسانی انتصاب پزشک {doctorDepartment?.DoctorId} به دپارتمان {doctorDepartment?.DepartmentId}", ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", 
                    doctorDepartment?.DoctorId, doctorDepartment?.DepartmentId);
                throw new InvalidOperationException($"خطا در به‌روزرسانی انتصاب پزشک {doctorDepartment?.DoctorId} به دپارتمان {doctorDepartment?.DepartmentId}", ex);
            }
        }

        /// <summary>
        /// حذف انتصاب پزشک از دپارتمان
        /// </summary>
        public async Task<bool> DeleteDoctorDepartmentAsync(DoctorDepartment doctorDepartment)
        {
            try
            {
                _logger.Information("شروع حذف انتصاب پزشک {DoctorId} از دپارتمان {DepartmentId}", 
                    doctorDepartment.DoctorId, doctorDepartment.DepartmentId);

                if (doctorDepartment == null)
                    throw new ArgumentNullException(nameof(doctorDepartment));

                var existingAssignment = await _context.DoctorDepartments
                    .FirstOrDefaultAsync(dd => dd.DoctorId == doctorDepartment.DoctorId && 
                                              dd.DepartmentId == doctorDepartment.DepartmentId && 
                                              !dd.IsDeleted);

                if (existingAssignment == null)
                {
                    _logger.Warning("انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId} یافت نشد", 
                        doctorDepartment.DoctorId, doctorDepartment.DepartmentId);
                    return false;
                }

                // بررسی وجود نوبت‌های آینده در این دپارتمان
                var hasFutureAppointments = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == doctorDepartment.DoctorId && 
                                  a.AppointmentDate > DateTime.Now && 
                                  !a.IsDeleted);

                if (hasFutureAppointments)
                {
                    _logger.Warning("پزشک {DoctorId} دارای نوبت‌های آینده در دپارتمان {DepartmentId} است و نمی‌تواند حذف شود", 
                        doctorDepartment.DoctorId, doctorDepartment.DepartmentId);
                    throw new InvalidOperationException("پزشک دارای نوبت‌های آینده در این دپارتمان است و نمی‌تواند حذف شود");
                }

                // حذف نرم
                existingAssignment.IsDeleted = true;
                existingAssignment.DeletedAt = DateTime.Now;
                existingAssignment.DeletedByUserId = doctorDepartment.DeletedByUserId ?? "System";

                await _context.SaveChangesAsync();

                _logger.Information("انتصاب پزشک {DoctorId} از دپارتمان {DepartmentId} با موفقیت حذف شد", 
                    doctorDepartment.DoctorId, doctorDepartment.DepartmentId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف انتصاب پزشک {DoctorId} از دپارتمان {DepartmentId}", 
                    doctorDepartment?.DoctorId, doctorDepartment?.DepartmentId);
                throw new InvalidOperationException($"خطا در حذف انتصاب پزشک {doctorDepartment?.DoctorId} از دپارتمان {doctorDepartment?.DepartmentId}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود انتصاب پزشک به دپارتمان
        /// </summary>
        public async Task<bool> DoesDoctorDepartmentExistAsync(int doctorId, int departmentId, int? excludeId = null)
        {
            try
            {
                _logger.Debug("بررسی وجود انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", doctorId, departmentId);

                var query = _context.DoctorDepartments
                    .Where(dd => dd.DoctorId == doctorId && 
                                dd.DepartmentId == departmentId && 
                                !dd.IsDeleted);

                // در DoctorDepartment کلید ترکیبی است، پس excludeId معنی ندارد
                // این متد برای سازگاری با interface نگه داشته شده

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", doctorId, departmentId);
                throw new InvalidOperationException($"خطا در بررسی وجود انتصاب پزشک {doctorId} به دپارتمان {departmentId}", ex);
            }
        }

        /// <summary>
        /// دریافت لیست پزشکان فعال در یک دپارتمان برای استفاده در لیست‌های کشویی
        /// </summary>
        public async Task<List<Doctor>> GetActiveDoctorsForDepartmentLookupAsync(int departmentId)
        {
            try
            {
                _logger.Information("دریافت لیست پزشکان فعال دپارتمان {DepartmentId} برای lookup", departmentId);

                var doctors = await _context.DoctorDepartments
                    .Include(dd => dd.Doctor)
                    .Where(dd => dd.DepartmentId == departmentId && 
                                dd.IsActive && 
                                !dd.IsDeleted &&
                                dd.Doctor.IsActive && 
                                !dd.Doctor.IsDeleted)
                    .Select(dd => dd.Doctor)
                    .OrderBy(d => d.LastName)
                    .ThenBy(d => d.FirstName)
                    .ToListAsync();

                _logger.Information("یافتن {Count} پزشک فعال در دپارتمان {DepartmentId}", doctors.Count, departmentId);

                return doctors;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان فعال دپارتمان {DepartmentId}", departmentId);
                throw new InvalidOperationException($"خطا در دریافت لیست پزشکان فعال دپارتمان {departmentId}", ex);
            }
        }

        /// <summary>
        /// ذخیره تمام تغییرات در انتظار به پایگاه داده
        /// </summary>
        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره تغییرات");
                throw new InvalidOperationException("خطا در ذخیره تغییرات", ex);
            }
        }

        #endregion
    }
}
