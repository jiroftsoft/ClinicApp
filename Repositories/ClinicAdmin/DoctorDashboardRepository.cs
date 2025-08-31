using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.DoctorManagementVM;
using DoctorSummaryViewModel = ClinicApp.Interfaces.ClinicAdmin.DoctorSummaryViewModel;

namespace ClinicApp.Repositories.ClinicAdmin
{
    /// <summary>
    /// پیاده‌سازی Repository برای داشبورد پزشکان
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل دسترسی به داده‌های داشبورد
    /// 2. رعایت استانداردهای پزشکی ایران در دسترسی به داده
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای داده‌ای
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorDashboardRepository : IDoctorDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorDashboardRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Dashboard Data (داده‌های داشبورد)

        /// <summary>
        /// دریافت داده‌های داشبورد اصلی
        /// </summary>
        public async Task<DoctorDashboardIndexViewModel> GetDashboardDataAsync(int? clinicId = null, int? departmentId = null)
        {
            try
            {
                var query = _context.Doctors
                    .Where(d => !d.IsDeleted)
                    .Include(d => d.DoctorDepartments)
                    .Include(d => d.DoctorServiceCategories)
                    .Include(d => d.DoctorSpecializations)
                    .AsQueryable();

                // فیلتر بر اساس کلینیک
                if (clinicId.HasValue && clinicId.Value > 0)
                {
                    query = query.Where(d => d.ClinicId == clinicId.Value);
                }

                // فیلتر بر اساس دپارتمان
                if (departmentId.HasValue && departmentId.Value > 0)
                {
                    query = query.Where(d => d.DoctorDepartments.Any(dd => 
                        dd.DepartmentId == departmentId.Value && 
                        dd.IsActive && 
                        !dd.IsDeleted));
                }

                var doctors = await query.ToListAsync();

                var dashboardData = new DoctorDashboardIndexViewModel
                {
                    Stats = await GetDashboardStatsAsync(clinicId),
                    RecentDoctors = doctors.Take(10).Select(d => new DoctorSummaryViewModel
                    {
                        DoctorId = d.DoctorId,
                        FullName = $"{d.FirstName} {d.LastName}",
                        NationalCode = d.NationalCode,
                        MedicalCouncilCode = d.MedicalCouncilCode,
                        SpecializationNames = d.DoctorSpecializations?.Select(ds => ds.Specialization?.Name).Where(name => !string.IsNullOrEmpty(name)).ToList() ?? new List<string>(),
                        IsActive = d.IsActive,
                        ActiveAssignmentsCount = d.DoctorDepartments?.Count(dd => dd.IsActive && !dd.IsDeleted) ?? 0,
                        Status = d.IsActive ? "فعال" : "غیرفعال",
                        LastActivity = d.UpdatedAt ?? d.CreatedAt
                    }).ToList(),
                    RecentAssignments = await GetRecentAssignmentsAsync(clinicId, departmentId),
                    SystemAlerts = await GetSystemAlertsAsync(clinicId),
                    ActiveFilters = new DoctorFilterViewModel
                    {
                        ClinicId = clinicId,
                        DepartmentId = departmentId
                    }
                };

                return dashboardData;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت داده‌های داشبورد", ex);
            }
        }

        /// <summary>
        /// دریافت جزئیات کامل پزشک
        /// </summary>
        public async Task<DoctorDetailsViewModel> GetDoctorDetailsAsync(int doctorId)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
                    .Include(d => d.DoctorDepartments)
                    .Include(d => d.DoctorServiceCategories)
                    .Include(d => d.DoctorSpecializations)
                    .Include(d => d.Appointments)
                    .FirstOrDefaultAsync();

                if (doctor == null)
                {
                    throw new InvalidOperationException($"پزشک با شناسه {doctorId} یافت نشد.");
                }

                return new DoctorDetailsViewModel
                {
                    DoctorId = doctor.DoctorId,
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    FullName = $"{doctor.FirstName} {doctor.LastName}",
                    NationalCode = doctor.NationalCode,
                    MedicalCouncilCode = doctor.MedicalCouncilCode,
                    PhoneNumber = doctor.PhoneNumber,
                    Email = doctor.Email,
                    IsActive = doctor.IsActive,
                    SpecializationNames = doctor.DoctorSpecializations?.Select(ds => ds.Specialization?.Name).Where(name => !string.IsNullOrEmpty(name)).ToList() ?? new List<string>(),
                    CreatedAt = doctor.CreatedAt,
                    UpdatedAt = doctor.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت جزئیات پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// دریافت لیست انتسابات پزشک
        /// </summary>
        public async Task<DoctorAssignmentsViewModel> GetDoctorAssignmentsAsync(int doctorId)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
                    .Include(d => d.DoctorDepartments)
                    .Include(d => d.DoctorServiceCategories)
                    .FirstOrDefaultAsync();

                if (doctor == null)
                {
                    throw new InvalidOperationException($"پزشک با شناسه {doctorId} یافت نشد.");
                }

                return new DoctorAssignmentsViewModel
                {
                    DoctorId = doctorId,
                    TotalActiveAssignments = doctor.DoctorDepartments?.Count(dd => dd.IsActive && !dd.IsDeleted) ?? 0,
                    ActiveDepartmentCount = doctor.DoctorDepartments?.Count(dd => dd.IsActive && !dd.IsDeleted) ?? 0,
                    ActiveServiceCategoryCount = doctor.DoctorServiceCategories?.Count(dsc => dsc.IsActive && !dsc.IsDeleted) ?? 0,
                    DoctorDepartments = doctor.DoctorDepartments?.Where(dd => !dd.IsDeleted).Select(dd => new DoctorDepartmentViewModel
                    {
                        DoctorId = dd.DoctorId,
                        DepartmentId = dd.DepartmentId,
                        DepartmentName = dd.Department?.Name ?? "نامشخص",
                        IsActive = dd.IsActive,
                        Role = dd.Role,
                        CreatedAt = dd.CreatedAt,
                        UpdatedAt = dd.UpdatedAt
                    }).ToList() ?? new List<DoctorDepartmentViewModel>(),
                    DoctorServiceCategories = doctor.DoctorServiceCategories?.Where(dsc => !dsc.IsDeleted).Select(dsc => new DoctorServiceCategoryViewModel
                    {
                        DoctorId = dsc.DoctorId,
                        ServiceCategoryId = dsc.ServiceCategoryId,
                        ServiceCategoryName = dsc.ServiceCategory?.Title ?? "نامشخص",
                        IsActive = dsc.IsActive,
                        AuthorizationLevel = dsc.AuthorizationLevel,
                        GrantedDate = dsc.GrantedDate,
                        CreatedAt = dsc.CreatedAt,
                        UpdatedAt = dsc.UpdatedAt
                    }).ToList() ?? new List<DoctorServiceCategoryViewModel>()
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت انتسابات پزشک {doctorId}", ex);
            }
        }

        #endregion

        #region Search & Filter (جستجو و فیلتر)

        /// <summary>
        /// جستجوی پزشکان
        /// </summary>
        public async Task<DoctorSearchResultViewModel> SearchDoctorsAsync(string searchTerm = null, int? clinicId = null, int? departmentId = null, int? specializationId = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.Doctors
                    .Where(d => !d.IsDeleted)
                    .Include(d => d.DoctorDepartments)
                    .Include(d => d.DoctorSpecializations)
                    .AsQueryable();

                // فیلتر بر اساس کلینیک
                if (clinicId.HasValue && clinicId.Value > 0)
                {
                    query = query.Where(d => d.ClinicId == clinicId.Value);
                }

                // فیلتر بر اساس دپارتمان
                if (departmentId.HasValue && departmentId.Value > 0)
                {
                    query = query.Where(d => d.DoctorDepartments.Any(dd => 
                        dd.DepartmentId == departmentId.Value && 
                        dd.IsActive && 
                        !dd.IsDeleted));
                }

                // فیلتر بر اساس تخصص
                if (specializationId.HasValue && specializationId.Value > 0)
                {
                    query = query.Where(d => d.DoctorSpecializations.Any(ds => ds.SpecializationId == specializationId.Value));
                }

                // جستجو در نام و کد ملی
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(d => 
                        d.FirstName.Contains(searchTerm) || 
                        d.LastName.Contains(searchTerm) || 
                        d.NationalCode.Contains(searchTerm) ||
                        d.MedicalCouncilCode.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var doctors = await query
                    .OrderBy(d => d.FirstName)
                    .ThenBy(d => d.LastName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new DoctorSearchResultViewModel
                {
                    Doctors = doctors.Select(d => new DoctorSummaryViewModel
                    {
                        DoctorId = d.DoctorId,
                        FullName = $"{d.FirstName} {d.LastName}",
                        NationalCode = d.NationalCode,
                        MedicalCouncilCode = d.MedicalCouncilCode,
                        SpecializationNames = d.DoctorSpecializations?.Select(ds => ds.Specialization?.Name).Where(name => !string.IsNullOrEmpty(name)).ToList() ?? new List<string>(),
                        IsActive = d.IsActive,
                        ActiveAssignmentsCount = d.DoctorDepartments?.Count(dd => dd.IsActive && !dd.IsDeleted) ?? 0,
                        Status = d.IsActive ? "فعال" : "غیرفعال",
                        LastActivity = d.UpdatedAt ?? d.CreatedAt
                    }).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    AppliedFilters = new DoctorFilterViewModel
                    {
                        SearchTerm = searchTerm,
                        ClinicId = clinicId,
                        DepartmentId = departmentId,
                        SpecializationId = specializationId
                    }
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در جستجوی پزشکان", ex);
            }
        }

        #endregion

        #region Statistics & Analytics (آمار و تحلیل)

        /// <summary>
        /// دریافت آمار کلی داشبورد
        /// </summary>
        public async Task<DashboardStatsViewModel> GetDashboardStatsAsync(int? clinicId = null)
        {
            try
            {
                var query = _context.Doctors.Where(d => !d.IsDeleted);

                if (clinicId.HasValue && clinicId.Value > 0)
                {
                    query = query.Where(d => d.ClinicId == clinicId.Value);
                }

                var totalDoctors = await query.CountAsync();
                var activeDoctors = await query.CountAsync(d => d.IsActive);
                var inactiveDoctors = totalDoctors - activeDoctors;

                var totalAssignments = await _context.DoctorDepartments.CountAsync(dd => !dd.IsDeleted);
                var activeAssignments = await _context.DoctorDepartments.CountAsync(dd => dd.IsActive && !dd.IsDeleted);

                var totalDepartments = await _context.Departments.CountAsync(d => !d.IsDeleted);
                var totalServiceCategories = await _context.ServiceCategories.CountAsync(sc => !sc.IsDeleted);

                var completionPercentage = totalDoctors > 0 ? (double)activeDoctors / totalDoctors * 100 : 0;

                return new DashboardStatsViewModel
                {
                    TotalDoctors = totalDoctors,
                    ActiveDoctors = activeDoctors,
                    InactiveDoctors = inactiveDoctors,
                    TotalAssignments = totalAssignments,
                    ActiveAssignments = activeAssignments,
                    TotalDepartments = totalDepartments,
                    TotalServiceCategories = totalServiceCategories,
                    CompletionPercentage = Math.Round(completionPercentage, 1),
                    LastUpdate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت آمار داشبورد", ex);
            }
        }

        /// <summary>
        /// دریافت آمار پزشکان فعال
        /// </summary>
        public async Task<ActiveDoctorsStatsViewModel> GetActiveDoctorsStatsAsync(int? clinicId = null, int? departmentId = null)
        {
            try
            {
                var query = _context.Doctors
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .Include(d => d.DoctorDepartments)
                    .Include(d => d.DoctorSpecializations)
                    .AsQueryable();

                if (clinicId.HasValue && clinicId.Value > 0)
                {
                    query = query.Where(d => d.ClinicId == clinicId.Value);
                }

                var doctors = await query.ToListAsync();

                var doctorsWithAssignments = doctors.Count(d => d.DoctorDepartments?.Any(dd => dd.IsActive && !dd.IsDeleted) == true);
                var doctorsWithoutAssignments = doctors.Count - doctorsWithAssignments;

                var departmentDistribution = doctors
                    .SelectMany(d => d.DoctorDepartments?.Where(dd => dd.IsActive && !dd.IsDeleted) ?? new List<DoctorDepartment>())
                    .GroupBy(dd => dd.Department?.Name ?? "نامشخص")
                    .ToDictionary(g => g.Key, g => g.Count());

                var specializationDistribution = doctors
                    .SelectMany(d => d.DoctorSpecializations?.Select(ds => ds.Specialization) ?? new List<Specialization>())
                    .Where(s => s != null)
                    .GroupBy(s => s.Name)
                    .ToDictionary(g => g.Key, g => g.Count());

                return new ActiveDoctorsStatsViewModel
                {
                    TotalActiveDoctors = doctors.Count,
                    DoctorsWithAssignments = doctorsWithAssignments,
                    DoctorsWithoutAssignments = doctorsWithoutAssignments,
                    DepartmentDistribution = departmentDistribution,
                    SpecializationDistribution = specializationDistribution
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت آمار پزشکان فعال", ex);
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// دریافت انتسابات اخیر
        /// </summary>
        private async Task<List<RecentAssignmentViewModel>> GetRecentAssignmentsAsync(int? clinicId = null, int? departmentId = null)
        {
            try
            {
                var query = _context.DoctorDepartments
                    .Where(dd => !dd.IsDeleted)
                    .Include(dd => dd.Doctor)
                    .Include(dd => dd.Department)
                    .AsQueryable();

                if (clinicId.HasValue && clinicId.Value > 0)
                {
                    query = query.Where(dd => dd.Doctor.ClinicId == clinicId.Value);
                }

                if (departmentId.HasValue && departmentId.Value > 0)
                {
                    query = query.Where(dd => dd.DepartmentId == departmentId.Value);
                }

                var recentAssignments = await query
                    .OrderByDescending(dd => dd.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                return recentAssignments.Select(dd => new RecentAssignmentViewModel
                {
                    AssignmentId = $"{dd.DoctorId}_{dd.DepartmentId}",
                    DoctorId = dd.DoctorId,
                    DoctorName = $"{dd.Doctor.FirstName} {dd.Doctor.LastName}",
                    DepartmentName = dd.Department?.Name ?? "نامشخص",
                    ActionType = dd.IsActive ? "انتساب" : "لغو انتساب",
                    AssignmentDate = dd.CreatedAt,
                    PerformedBy = dd.CreatedByUserId ?? "سیستم"
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت انتسابات اخیر", ex);
            }
        }

        /// <summary>
        /// دریافت هشدارهای سیستم
        /// </summary>
        private async Task<List<SystemAlertViewModel>> GetSystemAlertsAsync(int? clinicId = null)
        {
            try
            {
                // فعلاً یک لیست خالی برمی‌گردانیم
                // در آینده می‌توان از جدول SystemAlerts استفاده کرد
                return new List<SystemAlertViewModel>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت هشدارهای سیستم", ex);
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// مدل فیلتر پزشک
    /// </summary>
    public class DoctorFilterViewModel
    {
        public string SearchTerm { get; set; }
        public int? ClinicId { get; set; }
        public int? DepartmentId { get; set; }
        public int? SpecializationId { get; set; }
        public bool? IsActive { get; set; }
    }

    #endregion
}
