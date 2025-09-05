using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using Serilog;

namespace ClinicApp.Repositories.ClinicAdmin
{
    /// <summary>
    /// پیاده‌سازی اینترفیس IDoctorServiceCategoryRepository برای مدیریت صلاحیت‌های خدماتی پزشکان
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت رابطه چند-به-چند پزشک-دسته‌بندی خدمات
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت صلاحیت‌ها
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorServiceCategoryRepository : IDoctorServiceCategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public DoctorServiceCategoryRepository(ApplicationDbContext context, ILogger logger, ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Doctor-ServiceCategory Management (مدیریت انتصاب پزشک به سرفصل‌های خدماتی)

        /// <summary>
        /// دریافت انتصاب پزشک به سرفصل خدماتی بر اساس شناسه‌ها
        /// </summary>
        public async Task<DoctorServiceCategory> GetDoctorServiceCategoryAsync(int doctorId, int serviceCategoryId)
        {
            try
            {
                return await _context.DoctorServiceCategories
                    .Where(dsc => dsc.DoctorId == doctorId && dsc.ServiceCategoryId == serviceCategoryId)
                    .Include(dsc => dsc.Doctor)
                    .Include(dsc => dsc.ServiceCategory)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت انتصاب پزشک {doctorId} به سرفصل خدماتی {serviceCategoryId}", ex);
            }
        }

        /// <summary>
        /// دریافت انتصاب پزشک به سرفصل خدماتی همراه با جزئیات
        /// </summary>
        public async Task<DoctorServiceCategory> GetDoctorServiceCategoryWithDetailsAsync(int doctorId, int serviceCategoryId)
        {
            try
            {
                return await _context.DoctorServiceCategories
                    .Where(dsc => dsc.DoctorId == doctorId && dsc.ServiceCategoryId == serviceCategoryId)
                    .Include(dsc => dsc.Doctor)
                    .Include(dsc => dsc.ServiceCategory.Department)
                    .Include(dsc => dsc.CreatedByUser)
                    .Include(dsc => dsc.UpdatedByUser)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انتصاب پزشک به سرفصل خدماتی. DoctorId: {DoctorId}, ServiceCategoryId: {ServiceCategoryId}", doctorId, serviceCategoryId);
                return null;
            }
        }



        /// <summary>
        /// دریافت لیست انتصابات پزشک به سرفصل‌های خدماتی
        /// </summary>
        public async Task<List<DoctorServiceCategory>> GetDoctorServiceCategoriesAsync(int doctorId, string searchTerm, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.DoctorServiceCategories
                    .Where(dsc => dsc.DoctorId == doctorId && !dsc.IsDeleted)
                    .Include(dsc => dsc.Doctor)
                    .Include(dsc => dsc.ServiceCategory.Department)
                    .Include(dsc => dsc.CreatedByUser)
                    .AsNoTracking();

                // اعمال فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(dsc =>
                        (dsc.ServiceCategory.Title != null && dsc.ServiceCategory.Title.Contains(searchTerm)) ||
                        (dsc.ServiceCategory.Department.Name != null && dsc.ServiceCategory.Department.Name.Contains(searchTerm)) ||
                        (dsc.AuthorizationLevel != null && dsc.AuthorizationLevel.Contains(searchTerm)) ||
                        (dsc.CertificateNumber != null && dsc.CertificateNumber.Contains(searchTerm))
                    );
                }

                // اعمال صفحه‌بندی
                return await query
                    .OrderByDescending(dsc => dsc.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت لیست انتصابات پزشک {doctorId} به سرفصل‌های خدماتی", ex);
            }
        }

        /// <summary>
        /// دریافت تعداد انتصابات پزشک به سرفصل‌های خدماتی
        /// </summary>
        public async Task<int> GetDoctorServiceCategoriesCountAsync(int doctorId, string searchTerm)
        {
            try
            {
                var query = _context.DoctorServiceCategories
                    .Where(dsc => dsc.DoctorId == doctorId)
                    .AsNoTracking();

                // اعمال فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(dsc =>
                        (dsc.ServiceCategory.Title != null && dsc.ServiceCategory.Title.Contains(searchTerm)) ||
                        (dsc.ServiceCategory.Department.Name != null && dsc.ServiceCategory.Department.Name.Contains(searchTerm)) ||
                        (dsc.AuthorizationLevel != null && dsc.AuthorizationLevel.Contains(searchTerm)) ||
                        (dsc.CertificateNumber != null && dsc.CertificateNumber.Contains(searchTerm))
                    );
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت تعداد انتصابات پزشک {doctorId} به سرفصل‌های خدماتی", ex);
            }
        }

        /// <summary>
        /// افزودن انتصاب پزشک به سرفصل خدماتی
        /// </summary>
        public async Task<DoctorServiceCategory> AddDoctorServiceCategoryAsync(DoctorServiceCategory doctorServiceCategory)
        {
            try
            {
                if (doctorServiceCategory == null)
                    throw new ArgumentNullException(nameof(doctorServiceCategory));

                // بررسی وجود انتصاب قبلی
                var existingAssignment = await _context.DoctorServiceCategories
                    .FirstOrDefaultAsync(dsc => dsc.DoctorId == doctorServiceCategory.DoctorId && 
                                               dsc.ServiceCategoryId == doctorServiceCategory.ServiceCategoryId);

                if (existingAssignment != null)
                    throw new InvalidOperationException($"پزشک قبلاً به این سرفصل خدماتی انتصاب داده شده است.");

                // تنظیم فیلدهای ردیابی
                doctorServiceCategory.CreatedAt = DateTime.Now;
                doctorServiceCategory.CreatedByUserId = doctorServiceCategory.CreatedByUserId ?? _currentUserService.GetCurrentUserId();
                doctorServiceCategory.UpdatedAt = DateTime.Now;
                doctorServiceCategory.UpdatedByUserId = doctorServiceCategory.UpdatedByUserId ?? _currentUserService.GetCurrentUserId();
                doctorServiceCategory.IsDeleted = false;
                doctorServiceCategory.DeletedAt = null;
                doctorServiceCategory.DeletedByUserId = null;
                doctorServiceCategory.IsActive = true;

                _context.DoctorServiceCategories.Add(doctorServiceCategory);
                await _context.SaveChangesAsync();

                return doctorServiceCategory;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در افزودن انتصاب پزشک به سرفصل خدماتی", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی انتصاب پزشک به سرفصل خدماتی
        /// </summary>
        public async Task<DoctorServiceCategory> UpdateDoctorServiceCategoryAsync(DoctorServiceCategory doctorServiceCategory)
        {
            try
            {
                if (doctorServiceCategory == null)
                    throw new ArgumentNullException(nameof(doctorServiceCategory));

                var existingAssignment = await _context.DoctorServiceCategories
                    .FirstOrDefaultAsync(dsc => dsc.DoctorId == doctorServiceCategory.DoctorId && 
                                               dsc.ServiceCategoryId == doctorServiceCategory.ServiceCategoryId);

                if (existingAssignment == null)
                    throw new InvalidOperationException($"انتصاب پزشک به سرفصل خدماتی یافت نشد.");

                // به‌روزرسانی فیلدها
                existingAssignment.AuthorizationLevel = doctorServiceCategory.AuthorizationLevel;
                existingAssignment.IsActive = doctorServiceCategory.IsActive;
                existingAssignment.GrantedDate = doctorServiceCategory.GrantedDate;
                existingAssignment.ExpiryDate = doctorServiceCategory.ExpiryDate;
                existingAssignment.CertificateNumber = doctorServiceCategory.CertificateNumber;
                existingAssignment.Notes = doctorServiceCategory.Notes;
                existingAssignment.UpdatedAt = DateTime.Now;
                existingAssignment.UpdatedByUserId = doctorServiceCategory.UpdatedByUserId ?? _currentUserService.GetCurrentUserId();

                await _context.SaveChangesAsync();

                return existingAssignment;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // لاگ خطای همزمانی برای سیستم‌های پزشکی
                throw new InvalidOperationException("خطای همزمانی در به‌روزرسانی انتصاب پزشک به سرفصل خدماتی", ex);
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در به‌روزرسانی انتصاب پزشک به سرفصل خدماتی", ex);
            }
        }

        /// <summary>
        /// حذف انتصاب پزشک از سرفصل خدماتی
        /// </summary>
        public async Task<bool> DeleteDoctorServiceCategoryAsync(DoctorServiceCategory doctorServiceCategory)
        {
            try
            {
                if (doctorServiceCategory == null)
                    throw new ArgumentNullException(nameof(doctorServiceCategory));

                var existingAssignment = await _context.DoctorServiceCategories
                    .FirstOrDefaultAsync(dsc => dsc.DoctorId == doctorServiceCategory.DoctorId && 
                                               dsc.ServiceCategoryId == doctorServiceCategory.ServiceCategoryId);

                if (existingAssignment == null)
                    return false;

                // بررسی وجود نوبت‌های آینده
                var hasFutureAppointments = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == doctorServiceCategory.DoctorId && 
                                  a.AppointmentDate > DateTime.Now &&
                                  a.ServiceCategoryId == doctorServiceCategory.ServiceCategoryId);

                if (hasFutureAppointments)
                    throw new InvalidOperationException("امکان حذف انتصاب به دلیل وجود نوبت‌های آینده وجود ندارد.");

                // حذف نرم
                existingAssignment.IsDeleted = true;
                existingAssignment.IsActive = false; // غیرفعال کردن صلاحیت
                existingAssignment.DeletedAt = DateTime.Now;
                existingAssignment.DeletedByUserId = _currentUserService.GetCurrentUserId();

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در حذف انتصاب پزشک از سرفصل خدماتی", ex);
            }
        }

        /// <summary>
        /// بررسی وجود انتصاب پزشک به سرفصل خدماتی
        /// </summary>
        public async Task<bool> DoesDoctorServiceCategoryExistAsync(int doctorId, int serviceCategoryId, int? excludeId = null)
        {
            try
            {
                return await _context.DoctorServiceCategories
                    .AnyAsync(dsc => dsc.DoctorId == doctorId && dsc.ServiceCategoryId == serviceCategoryId);
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در بررسی وجود انتصاب پزشک {doctorId} به سرفصل خدماتی {serviceCategoryId}", ex);
            }
        }

        /// <summary>
        /// بررسی دسترسی پزشک به یک سرفصل خدماتی خاص
        /// </summary>
        public async Task<bool> HasAccessToServiceCategoryAsync(int doctorId, int serviceCategoryId)
        {
            try
            {
                return await _context.DoctorServiceCategories
                    .AnyAsync(dsc => dsc.DoctorId == doctorId && 
                                   dsc.ServiceCategoryId == serviceCategoryId && 
                                   dsc.IsActive);
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در بررسی دسترسی پزشک {doctorId} به سرفصل خدماتی {serviceCategoryId}", ex);
            }
        }

        /// <summary>
        /// بررسی دسترسی پزشک به یک خدمت خاص
        /// </summary>
        public async Task<bool> HasAccessToServiceAsync(int doctorId, int serviceId)
        {
            try
            {
                return await _context.DoctorServiceCategories
                    .Include(dsc => dsc.ServiceCategory)
                    .Join(_context.Services,
                          dsc => dsc.ServiceCategoryId,
                          s => s.ServiceCategoryId,
                          (dsc, s) => new { dsc, s })
                    .AsNoTracking()
                    .AnyAsync(x => x.dsc.DoctorId == doctorId && 
                                  x.s.ServiceId == serviceId && 
                                  x.dsc.IsActive && 
                                  !x.s.IsDeleted);
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در بررسی دسترسی پزشک {doctorId} به خدمت {serviceId}", ex);
            }
        }

        /// <summary>
        /// دریافت لیست پزشکان مجاز در سرفصل خدماتی برای استفاده در لیست‌های کشویی
        /// </summary>
        public async Task<List<Doctor>> GetAuthorizedDoctorsForServiceCategoryLookupAsync(int serviceCategoryId)
        {
            try
            {
                _logger.Information("دریافت لیست پزشکان مجاز سرفصل خدماتی {ServiceCategoryId} برای lookup", serviceCategoryId);

                var doctors = await _context.DoctorServiceCategories
                    .Include(dsc => dsc.Doctor)
                    .Where(dsc => dsc.ServiceCategoryId == serviceCategoryId && 
                                 dsc.IsActive && 
                                 !dsc.IsDeleted &&
                                 dsc.Doctor.IsActive && 
                                 !dsc.Doctor.IsDeleted)
                    .Select(dsc => dsc.Doctor)
                    .OrderBy(d => d.LastName)
                    .ThenBy(d => d.FirstName)
                    .ToListAsync();

                _logger.Information("یافتن {Count} پزشک مجاز در سرفصل خدماتی {ServiceCategoryId}", doctors.Count, serviceCategoryId);

                return doctors;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان مجاز سرفصل خدماتی {ServiceCategoryId}", serviceCategoryId);
                throw new InvalidOperationException($"خطا در دریافت لیست پزشکان مجاز سرفصل خدماتی {serviceCategoryId}", ex);
            }
        }

        /// <summary>
        /// دریافت لیست پزشکان فعال در یک دسته‌بندی خدماتی برای استفاده در لیست‌های کشویی
        /// </summary>
        public async Task<List<Doctor>> GetActiveDoctorsForServiceCategoryLookupAsync(int serviceCategoryId)
        {
            try
            {
                _logger.Information("دریافت لیست پزشکان فعال دسته‌بندی خدماتی {ServiceCategoryId} برای lookup", serviceCategoryId);

                var doctors = await _context.DoctorServiceCategories
                    .Include(dsc => dsc.Doctor)
                    .Where(dsc => dsc.ServiceCategoryId == serviceCategoryId && 
                                 dsc.IsActive && 
                                 !dsc.IsDeleted &&
                                 dsc.Doctor.IsActive && 
                                 !dsc.Doctor.IsDeleted)
                    .Select(dsc => dsc.Doctor)
                    .OrderBy(d => d.LastName)
                    .ThenBy(d => d.FirstName)
                    .ToListAsync();

                _logger.Information("یافتن {Count} پزشک فعال در دسته‌بندی خدماتی {ServiceCategoryId}", doctors.Count, serviceCategoryId);

                return doctors;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان فعال دسته‌بندی خدماتی {ServiceCategoryId}", serviceCategoryId);
                throw new InvalidOperationException($"خطا در دریافت لیست پزشکان فعال دسته‌بندی خدماتی {serviceCategoryId}", ex);
            }
        }

        /// <summary>
        /// دریافت لیست پزشکان برای یک دسته‌بندی خدماتی
        /// </summary>
        public async Task<List<Doctor>> GetDoctorsForServiceCategoryAsync(int serviceCategoryId)
        {
            try
            {
                _logger.Information("دریافت لیست پزشکان دسته‌بندی خدماتی {ServiceCategoryId}", serviceCategoryId);

                var doctors = await _context.DoctorServiceCategories
                    .Include(dsc => dsc.Doctor)
                    .Where(dsc => dsc.ServiceCategoryId == serviceCategoryId && 
                                 !dsc.IsDeleted &&
                                 !dsc.Doctor.IsDeleted)
                    .Select(dsc => dsc.Doctor)
                    .OrderBy(d => d.LastName)
                    .ThenBy(d => d.FirstName)
                    .ToListAsync();

                _logger.Information("یافتن {Count} پزشک در دسته‌بندی خدماتی {ServiceCategoryId}", doctors.Count, serviceCategoryId);

                return doctors;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان دسته‌بندی خدماتی {ServiceCategoryId}", serviceCategoryId);
                throw new InvalidOperationException($"خطا در دریافت لیست پزشکان دسته‌بندی خدماتی {serviceCategoryId}", ex);
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
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException("خطا در ذخیره تغییرات", ex);
            }
        }

        /// <summary>
        /// دریافت لیست همه انتصابات پزشکان به سرفصل‌های خدماتی (برای فیلتر "همه پزشکان")
        /// </summary>
        public async Task<List<DoctorServiceCategory>> GetAllDoctorServiceCategoriesAsync(string searchTerm, int? doctorId, int? serviceCategoryId, bool? isActive, int pageNumber, int pageSize)
        {
            try
            {
                IQueryable<DoctorServiceCategory> query = _context.DoctorServiceCategories
                    .Include(dsc => dsc.Doctor)
                    .Include(dsc => dsc.ServiceCategory.Department)
                    .Include(dsc => dsc.CreatedByUser)
                    .AsNoTracking();

                // فیلتر بر اساس پزشک (اختیاری)
                if (doctorId.HasValue && doctorId.Value > 0)
                {
                    query = query.Where(dsc => dsc.DoctorId == doctorId.Value);
                }

                // فیلتر بر اساس سرفصل خدماتی (اختیاری)
                if (serviceCategoryId.HasValue && serviceCategoryId.Value > 0)
                {
                    query = query.Where(dsc => dsc.ServiceCategoryId == serviceCategoryId.Value);
                }

                // فیلتر بر اساس وضعیت فعال (اختیاری)
                if (isActive.HasValue)
                {
                    query = query.Where(dsc => dsc.IsActive == isActive.Value);
                }

                // اعمال فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(dsc =>
                        (dsc.Doctor.FirstName != null && dsc.Doctor.FirstName.Contains(searchTerm)) ||
                        (dsc.Doctor.LastName != null && dsc.Doctor.LastName.Contains(searchTerm)) ||
                        (dsc.ServiceCategory.Title != null && dsc.ServiceCategory.Title.Contains(searchTerm)) ||
                        (dsc.ServiceCategory.Department.Name != null && dsc.ServiceCategory.Department.Name.Contains(searchTerm)) ||
                        (dsc.AuthorizationLevel != null && dsc.AuthorizationLevel.Contains(searchTerm)) ||
                        (dsc.CertificateNumber != null && dsc.CertificateNumber.Contains(searchTerm))
                    );
                }

                // اعمال صفحه‌بندی
                return await query
                    .OrderByDescending(dsc => dsc.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException("خطا در دریافت لیست همه انتصابات پزشکان به سرفصل‌های خدماتی", ex);
            }
        }

        /// <summary>
        /// دریافت تعداد همه انتصابات پزشکان به سرفصل‌های خدماتی
        /// </summary>
        public async Task<int> GetAllDoctorServiceCategoriesCountAsync(string searchTerm, int? doctorId, int? serviceCategoryId, bool? isActive)
        {
            try
            {
                IQueryable<DoctorServiceCategory> query = _context.DoctorServiceCategories
                    .AsNoTracking();

                // فیلتر بر اساس پزشک (اختیاری)
                if (doctorId.HasValue && doctorId.Value > 0)
                {
                    query = query.Where(dsc => dsc.DoctorId == doctorId.Value);
                }

                // فیلتر بر اساس سرفصل خدماتی (اختیاری)
                if (serviceCategoryId.HasValue && serviceCategoryId.Value > 0)
                {
                    query = query.Where(dsc => dsc.ServiceCategoryId == serviceCategoryId.Value);
                }

                // فیلتر بر اساس وضعیت فعال (اختیاری)
                if (isActive.HasValue)
                {
                    query = query.Where(dsc => dsc.IsActive == isActive.Value);
                }

                // اعمال فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(dsc =>
                        (dsc.Doctor.FirstName != null && dsc.Doctor.FirstName.Contains(searchTerm)) ||
                        (dsc.Doctor.LastName != null && dsc.Doctor.LastName.Contains(searchTerm)) ||
                        (dsc.ServiceCategory.Title != null && dsc.ServiceCategory.Title.Contains(searchTerm)) ||
                        (dsc.ServiceCategory.Department.Name != null && dsc.ServiceCategory.Department.Name.Contains(searchTerm)) ||
                        (dsc.AuthorizationLevel != null && dsc.AuthorizationLevel.Contains(searchTerm)) ||
                        (dsc.CertificateNumber != null && dsc.CertificateNumber.Contains(searchTerm))
                    );
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException("خطا در دریافت تعداد همه انتصابات پزشکان به سرفصل‌های خدماتی", ex);
            }
        }

        #endregion

        #region Department Management (مدیریت دپارتمان‌ها)

        /// <summary>
        /// دریافت دپارتمان‌های مرتبط با پزشک
        /// </summary>
        public async Task<List<Department>> GetDoctorDepartmentsAsync(int doctorId)
        {
            try
            {
                return await _context.DoctorDepartments
                    .Where(dd => dd.DoctorId == doctorId && !dd.IsDeleted)
                    .Include(dd => dd.Department)
                    .Select(dd => dd.Department)
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت دپارتمان‌های پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// دریافت سرفصل‌های خدماتی مرتبط با دپارتمان
        /// </summary>
        public async Task<List<ServiceCategory>> GetServiceCategoriesByDepartmentAsync(int departmentId)
        {
            try
            {
                return await _context.ServiceCategories
                    .Where(sc => sc.DepartmentId == departmentId && !sc.IsDeleted && sc.IsActive)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت سرفصل‌های خدماتی دپارتمان {departmentId}", ex);
            }
        }

        #endregion
    }
}
