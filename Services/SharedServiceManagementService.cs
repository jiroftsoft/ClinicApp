using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.ViewModels;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس مدیریت خدمات مشترک
    /// این سرویس برای مدیریت خدمات مشترک بین دپارتمان‌ها طراحی شده است
    /// </summary>
    public class SharedServiceManagementService : ISharedServiceManagementService
    {
        private readonly ApplicationDbContext _context;

        public SharedServiceManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// اضافه کردن خدمت به دپارتمان
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="notes">توضیحات خاص دپارتمان</param>
        /// <returns>نتیجه عملیات</returns>
        public ServiceResult AddServiceToDepartment(int serviceId, int departmentId, string userId, string notes = null)
        {
            try
            {
                // بررسی وجود خدمت
                var service = _context.Services.FirstOrDefault(s => s.ServiceId == serviceId && !s.IsDeleted);
                if (service == null)
                    return ServiceResult<bool>.Failed("خدمت مورد نظر یافت نشد.");

                // بررسی وجود دپارتمان
                var department = _context.Departments.FirstOrDefault(d => d.DepartmentId == departmentId && !d.IsDeleted);
                if (department == null)
                    return ServiceResult<bool>.Failed("دپارتمان مورد نظر یافت نشد.");

                // بررسی تکراری نبودن
                var existingSharedService = _context.SharedServices
                    .FirstOrDefault(ss => ss.ServiceId == serviceId && 
                                        ss.DepartmentId == departmentId && 
                                        !ss.IsDeleted);

                if (existingSharedService != null)
                    return ServiceResult<bool>.Failed("این خدمت قبلاً در این دپارتمان تعریف شده است.");

                // ایجاد خدمت مشترک
                var sharedService = new SharedService
                {
                    ServiceId = serviceId,
                    DepartmentId = departmentId,
                    IsActive = true,
                    DepartmentSpecificNotes = notes,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.Now
                };

                _context.SharedServices.Add(sharedService);
                _context.SaveChanges();

                return ServiceResult<bool>.Successful(true, "خدمت با موفقیت به دپارتمان اضافه شد.");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failed($"خطا در اضافه کردن خدمت: {ex.Message}");
            }
        }

        /// <summary>
        /// حذف خدمت از دپارتمان
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه عملیات</returns>
        public async Task<ServiceResult<bool>> RemoveServiceFromDepartment(int serviceId, int departmentId, string userId)
        {
            try
            {
                var sharedService = await _context.SharedServices
                    .FirstOrDefaultAsync(ss => ss.ServiceId == serviceId && 
                                        ss.DepartmentId == departmentId && 
                                        !ss.IsDeleted);

                if (sharedService == null)
                    return ServiceResult<bool>.Failed("خدمت مشترک مورد نظر یافت نشد.");

                // حذف نرم
                sharedService.IsDeleted = true;
                sharedService.DeletedAt = DateTime.UtcNow;
                sharedService.DeletedByUserId = userId;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failed($"خطا در حذف خدمت: {ex.Message}");
            }
        }

        /// <summary>
        /// دریافت تمام خدمات مشترک یک دپارتمان
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>لیست خدمات مشترک</returns>
        public List<SharedService> GetDepartmentSharedServices(int departmentId)
        {
            return _context.SharedServices
                .Include(ss => ss.Service)
                .Include(ss => ss.Department)
                .Where(ss => ss.DepartmentId == departmentId && 
                           ss.IsActive && 
                           !ss.IsDeleted)
                .ToList();
        }

        /// <summary>
        /// دریافت تمام دپارتمان‌هایی که یک خدمت در آن‌ها مشترک است
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>لیست دپارتمان‌ها</returns>
        public List<Department> GetServiceSharedDepartments(int serviceId)
        {
            return _context.SharedServices
                .Include(ss => ss.Department)
                .Where(ss => ss.ServiceId == serviceId && 
                           ss.IsActive && 
                           !ss.IsDeleted)
                .Select(ss => ss.Department)
                .ToList();
        }

        /// <summary>
        /// فعال/غیرفعال کردن خدمت در دپارتمان
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="isActive">وضعیت فعال/غیرفعال</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه عملیات</returns>
        public ServiceResult ToggleServiceInDepartment(int serviceId, int departmentId, bool isActive, string userId)
        {
            try
            {
                var sharedService = _context.SharedServices
                    .FirstOrDefault(ss => ss.ServiceId == serviceId && 
                                        ss.DepartmentId == departmentId && 
                                        !ss.IsDeleted);

                if (sharedService == null)
                    return ServiceResult<bool>.Failed("خدمت مشترک مورد نظر یافت نشد.");

                sharedService.IsActive = isActive;
                sharedService.UpdatedAt = DateTime.Now;
                sharedService.UpdatedByUserId = userId;

                _context.SaveChanges();

                return ServiceResult<bool>.Successful(true, $"خدمت با موفقیت {(isActive ? "فعال" : "غیرفعال")} شد.");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failed($"خطا در تغییر وضعیت خدمت: {ex.Message}");
            }
        }

        /// <summary>
        /// کپی کردن خدمت به دپارتمان‌های دیگر
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentIds">لیست شناسه‌های دپارتمان</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه عملیات</returns>
        public async Task<ServiceResult> CopyServiceToDepartments(int serviceId, List<int> departmentIds, string userId)
        {
            try
            {
                var results = new List<string>();

                foreach (var departmentId in departmentIds)
                {
                    var result = await AddServiceToDepartment(serviceId, departmentId, userId);
                    if (!result.Success)
                    {
                        results.Add($"دپارتمان {departmentId}: {result.Message}");
                    }
                }

                if (results.Any())
                {
                    return ServiceResult<bool>.Failed($"برخی عملیات‌ها ناموفق بودند:\n{string.Join("\n", results)}");
                }

                return ServiceResult<bool>.Successful(true, "خدمت با موفقیت به تمام دپارتمان‌های انتخابی اضافه شد.");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failed($"خطا در کپی کردن خدمت: {ex.Message}");
            }
        }

        /// <summary>
        /// بررسی اینکه آیا خدمت در دپارتمان موجود است یا نه
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>true اگر خدمت در دپارتمان موجود باشد</returns>
        public bool IsServiceInDepartment(int serviceId, int departmentId)
        {
            return _context.SharedServices
                .Any(ss => ss.ServiceId == serviceId && 
                          ss.DepartmentId == departmentId && 
                          ss.IsActive && 
                          !ss.IsDeleted);
        }

        /// <summary>
        /// دریافت آمار خدمات مشترک
        /// </summary>
        /// <returns>آمار خدمات مشترک</returns>
        public ServiceResult<SharedServiceStatisticsViewModel> GetSharedServiceStatistics()
        {
            var totalSharedServices = _context.SharedServices.Count(ss => !ss.IsDeleted);
            var activeSharedServices = _context.SharedServices.Count(ss => ss.IsActive && !ss.IsDeleted);
            var totalDepartments = _context.Departments.Count(d => !d.IsDeleted);
            var totalServices = _context.Services.Count(s => !s.IsDeleted);

            var averageServicesPerDepartment = totalDepartments > 0 ? (double)totalSharedServices / totalDepartments : 0;

            var viewModel = new SharedServiceStatisticsViewModel
            {
                TotalSharedServices = totalSharedServices,
                ActiveSharedServices = activeSharedServices,
                InactiveSharedServices = totalSharedServices - activeSharedServices,
                TotalDepartments = totalDepartments,
                TotalServices = totalServices,
                AverageServicesPerDepartment = Math.Round(averageServicesPerDepartment, 2),
                ActiveServicesPercentage = totalSharedServices > 0 ? 
                    (double)activeSharedServices / totalSharedServices * 100 : 0,
                InactiveServicesPercentage = totalSharedServices > 0 ? 
                    (double)(totalSharedServices - activeSharedServices) / totalSharedServices * 100 : 0
            };

            return ServiceResult<SharedServiceStatisticsViewModel>.Successful(viewModel);
        }

        #region ISharedServiceManagementService Implementation

        public async Task<ServiceResult<PagedResult<SharedServiceIndexViewModel>>> GetSharedServicesAsync(
            int? serviceId = null,
            int? departmentId = null,
            bool? isActive = null,
            string searchTerm = "",
            int pageNumber = 1,
            int pageSize = 20)
        {
            try
            {
                var query = _context.SharedServices
                    .Include(ss => ss.Service)
                    .Include(ss => ss.Department)
                    .Where(ss => !ss.IsDeleted);

                // اعمال فیلترها
                if (departmentId.HasValue)
                    query = query.Where(ss => ss.DepartmentId == departmentId.Value);

                if (serviceId.HasValue)
                    query = query.Where(ss => ss.ServiceId == serviceId.Value);

                if (isActive.HasValue)
                    query = query.Where(ss => ss.IsActive == isActive.Value);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(ss => 
                        ss.Service.Title.Contains(searchTerm) ||
                        ss.Service.ServiceCode.Contains(searchTerm) ||
                        ss.Department.Name.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(ss => ss.Service.Title)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModels = items.Select(SharedServiceIndexViewModel.FromEntity).ToList();
                var pagedResult = new PagedResult<SharedServiceIndexViewModel>(viewModels, totalCount, pageNumber, pageSize);

                return ServiceResult<PagedResult<SharedServiceIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<SharedServiceIndexViewModel>>.Failed($"خطا در دریافت لیست خدمات مشترک: {ex.Message}");
            }
        }

        public async Task<ServiceResult<SharedServiceDetailsViewModel>> GetSharedServiceDetailsAsync(int sharedServiceId)
        {
            try
            {
                var sharedService = await _context.SharedServices
                    .Include(ss => ss.Service)
                    .Include(ss => ss.Department)
                    .FirstOrDefaultAsync(ss => ss.SharedServiceId == sharedServiceId && !ss.IsDeleted);

                if (sharedService == null)
                    return ServiceResult<SharedServiceDetailsViewModel>.Failed("خدمت مشترک یافت نشد");

                var viewModel = SharedServiceDetailsViewModel.FromEntity(sharedService);
                return ServiceResult<SharedServiceDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                return ServiceResult<SharedServiceDetailsViewModel>.Failed($"خطا در دریافت جزئیات خدمت مشترک: {ex.Message}");
            }
        }

        public async Task<ServiceResult<SharedServiceCreateEditViewModel>> GetSharedServiceForEditAsync(int sharedServiceId)
        {
            try
            {
                var sharedService = await _context.SharedServices
                    .Include(ss => ss.Service)
                    .Include(ss => ss.Department)
                    .FirstOrDefaultAsync(ss => ss.SharedServiceId == sharedServiceId && !ss.IsDeleted);

                if (sharedService == null)
                    return ServiceResult<SharedServiceCreateEditViewModel>.Failed("خدمت مشترک یافت نشد");

                var viewModel = SharedServiceCreateEditViewModel.FromEntity(sharedService);
                return ServiceResult<SharedServiceCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                return ServiceResult<SharedServiceCreateEditViewModel>.Failed($"خطا در دریافت اطلاعات ویرایش: {ex.Message}");
            }
        }

        public async Task<ServiceResult> CreateSharedServiceAsync(SharedServiceCreateEditViewModel model)
        {
            try
            {
                // بررسی تکراری نبودن
                var isDuplicate = await IsSharedServiceDuplicateAsync(model.ServiceId, model.DepartmentId);
                if (isDuplicate)
                    return ServiceResult<SharedServiceDetailsViewModel>.Failed("این خدمت قبلاً در این دپارتمان تعریف شده است");

                var sharedService = model.ToEntity();
                sharedService.CreatedAt = DateTime.Now;
                sharedService.IsActive = true;
                sharedService.IsDeleted = false;

                _context.SharedServices.Add(sharedService);
                await _context.SaveChangesAsync();

                return ServiceResult<SharedServiceDetailsViewModel>.Successful(SharedServiceDetailsViewModel.FromEntity(sharedService), "خدمت مشترک با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                return ServiceResult<SharedServiceDetailsViewModel>.Failed($"خطا در ایجاد خدمت مشترک: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UpdateSharedServiceAsync(SharedServiceCreateEditViewModel model)
        {
            try
            {
                var sharedService = await _context.SharedServices
                    .FirstOrDefaultAsync(ss => ss.SharedServiceId == model.SharedServiceId && !ss.IsDeleted);

                if (sharedService == null)
                    return ServiceResult<SharedServiceDetailsViewModel>.Failed("خدمت مشترک یافت نشد");

                // بررسی تکراری نبودن (به جز خود رکورد)
                var isDuplicate = await IsSharedServiceDuplicateAsync(model.ServiceId, model.DepartmentId, model.SharedServiceId);
                if (isDuplicate)
                    return ServiceResult<SharedServiceDetailsViewModel>.Failed("این خدمت قبلاً در این دپارتمان تعریف شده است");

                // به‌روزرسانی فیلدها
                sharedService.ServiceId = model.ServiceId;
                sharedService.DepartmentId = model.DepartmentId;
                sharedService.IsActive = model.IsActive;
                sharedService.DepartmentSpecificNotes = model.DepartmentSpecificNotes;
                sharedService.OverrideTechnicalFactor = model.OverrideTechnicalFactor;
                sharedService.OverrideProfessionalFactor = model.OverrideProfessionalFactor;
                sharedService.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return ServiceResult<SharedServiceDetailsViewModel>.Successful(SharedServiceDetailsViewModel.FromEntity(sharedService), "خدمت مشترک با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                return ServiceResult<SharedServiceDetailsViewModel>.Failed($"خطا در به‌روزرسانی خدمت مشترک: {ex.Message}");
            }
        }

        public async Task<ServiceResult> SoftDeleteSharedServiceAsync(int sharedServiceId)
        {
            try
            {
                var sharedService = await _context.SharedServices
                    .FirstOrDefaultAsync(ss => ss.SharedServiceId == sharedServiceId && !ss.IsDeleted);

                if (sharedService == null)
                    return ServiceResult<SharedServiceDetailsViewModel>.Failed("خدمت مشترک یافت نشد");

                sharedService.IsDeleted = true;
                sharedService.DeletedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Successful(true, "خدمت مشترک با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failed($"خطا در حذف خدمت مشترک: {ex.Message}");
            }
        }

        public async Task<ServiceResult> RestoreSharedServiceAsync(int sharedServiceId)
        {
            try
            {
                var sharedService = await _context.SharedServices
                    .FirstOrDefaultAsync(ss => ss.SharedServiceId == sharedServiceId && ss.IsDeleted);

                if (sharedService == null)
                    return ServiceResult.Failed("خدمت مشترک حذف شده یافت نشد");

                sharedService.IsDeleted = false;
                sharedService.DeletedAt = null;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Successful(true, "خدمت مشترک با موفقیت بازیابی شد");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failed($"خطا در بازیابی خدمت مشترک: {ex.Message}");
            }
        }

        public async Task<ServiceResult> AddServiceToDepartmentAsync(int serviceId, int departmentId, string notes = null)
        {
            return await Task.FromResult(AddServiceToDepartment(serviceId, departmentId, "system", notes));
        }

        public async Task<ServiceResult> RemoveServiceFromDepartmentAsync(int serviceId, int departmentId)
        {
            var result = await RemoveServiceFromDepartment(serviceId, departmentId, "system");
            return ServiceResult.Successful(result.Message);
        }

        public async Task<ServiceResult> ToggleServiceInDepartmentAsync(int serviceId, int departmentId, bool isActive)
        {
            return await Task.FromResult(ToggleServiceInDepartment(serviceId, departmentId, isActive, "system"));
        }

        public async Task<ServiceResult> CopyServiceToDepartmentsAsync(int serviceId, List<int> departmentIds)
        {
            return await CopyServiceToDepartments(serviceId, departmentIds, "system");
        }

        public async Task<ServiceResult<List<SharedServiceIndexViewModel>>> GetDepartmentSharedServicesAsync(int departmentId)
        {
            try
            {
                var sharedServices = await _context.SharedServices
                    .Include(ss => ss.Service)
                    .Include(ss => ss.Department)
                    .Where(ss => ss.DepartmentId == departmentId && ss.IsActive && !ss.IsDeleted)
                    .ToListAsync();

                var viewModels = sharedServices.Select(SharedServiceIndexViewModel.FromEntity).ToList();
                return ServiceResult<List<SharedServiceIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<SharedServiceIndexViewModel>>.Failed($"خطا در دریافت خدمات دپارتمان: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<DepartmentLookupViewModel>>> GetServiceSharedDepartmentsAsync(int serviceId)
        {
            try
            {
                var departments = await _context.SharedServices
                    .Include(ss => ss.Department)
                    .Where(ss => ss.ServiceId == serviceId && ss.IsActive && !ss.IsDeleted)
                    .Select(ss => ss.Department)
                    .ToListAsync();

                var viewModels = departments.Select(DepartmentLookupViewModel.FromEntity).ToList();
                return ServiceResult<List<DepartmentLookupViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<DepartmentLookupViewModel>>.Failed($"خطا در دریافت دپارتمان‌های خدمت: {ex.Message}");
            }
        }

        public async Task<bool> IsServiceInDepartmentAsync(int serviceId, int departmentId)
        {
            return await Task.FromResult(IsServiceInDepartment(serviceId, departmentId));
        }

        public async Task<ServiceResult<List<LookupItemViewModel>>> GetActiveServicesForLookupAsync(int? excludeDepartmentId = null)
        {
            try
            {
                var query = _context.Services.Where(s => !s.IsDeleted && s.IsActive);

                if (excludeDepartmentId.HasValue)
                {
                    var excludedServiceIds = _context.SharedServices
                        .Where(ss => ss.DepartmentId == excludeDepartmentId.Value && !ss.IsDeleted)
                        .Select(ss => ss.ServiceId);
                    
                    query = query.Where(s => !excludedServiceIds.Contains(s.ServiceId));
                }

                var services = await query
                    .OrderBy(s => s.Title)
                    .Select(s => new LookupItemViewModel
                    {
                        Id = s.ServiceId,
                        Name = $"{s.ServiceCode} - {s.Title}"
                    })
                    .ToListAsync();

                return ServiceResult<List<LookupItemViewModel>>.Successful(services);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<LookupItemViewModel>>.Failed($"خطا در دریافت لیست خدمات: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<LookupItemViewModel>>> GetActiveDepartmentsForLookupAsync(int? excludeServiceId = null)
        {
            try
            {
                var query = _context.Departments.Where(d => !d.IsDeleted && d.IsActive);

                if (excludeServiceId.HasValue)
                {
                    var excludedDepartmentIds = _context.SharedServices
                        .Where(ss => ss.ServiceId == excludeServiceId.Value && !ss.IsDeleted)
                        .Select(ss => ss.DepartmentId);
                    
                    query = query.Where(d => !excludedDepartmentIds.Contains(d.DepartmentId));
                }

                var departments = await query
                    .OrderBy(d => d.Name)
                    .Select(d => new LookupItemViewModel
                    {
                        Id = d.DepartmentId,
                        Name = d.Name
                    })
                    .ToListAsync();

                return ServiceResult<List<LookupItemViewModel>>.Successful(departments);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<LookupItemViewModel>>.Failed($"خطا در دریافت لیست دپارتمان‌ها: {ex.Message}");
            }
        }

        public async Task<ServiceResult<SharedServiceStatisticsViewModel>> GetSharedServiceStatisticsAsync()
        {
            try
        {
            var totalSharedServices = _context.SharedServices.Count(ss => !ss.IsDeleted);
            var activeSharedServices = _context.SharedServices.Count(ss => ss.IsActive && !ss.IsDeleted);
            var totalDepartments = _context.Departments.Count(d => !d.IsDeleted);
            var totalServices = _context.Services.Count(s => !s.IsDeleted);

                var averageServicesPerDepartment = totalDepartments > 0 ? (double)totalSharedServices / totalDepartments : 0;

                var viewModel = new SharedServiceStatisticsViewModel
            {
                TotalSharedServices = totalSharedServices,
                ActiveSharedServices = activeSharedServices,
                    InactiveSharedServices = totalSharedServices - activeSharedServices,
                TotalDepartments = totalDepartments,
                TotalServices = totalServices,
                    AverageServicesPerDepartment = Math.Round(averageServicesPerDepartment, 2),
                    ActiveServicesPercentage = totalSharedServices > 0 ? 
                        (double)activeSharedServices / totalSharedServices * 100 : 0,
                    InactiveServicesPercentage = totalSharedServices > 0 ? 
                        (double)(totalSharedServices - activeSharedServices) / totalSharedServices * 100 : 0
                };

                return ServiceResult<SharedServiceStatisticsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                return ServiceResult<SharedServiceStatisticsViewModel>.Failed($"خطا در دریافت آمار: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<SharedServiceUsageReportViewModel>>> GetSharedServiceUsageReportAsync(
            int? departmentId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                // این متد نیاز به پیاده‌سازی کامل دارد
                // فعلاً یک پیاده‌سازی ساده ارائه می‌دهیم
                var report = new List<SharedServiceUsageReportViewModel>();
                return ServiceResult<List<SharedServiceUsageReportViewModel>>.Successful(report);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<SharedServiceUsageReportViewModel>>.Failed($"خطا در دریافت گزارش: {ex.Message}");
            }
        }

        public async Task<bool> IsSharedServiceDuplicateAsync(int serviceId, int departmentId, int? excludeSharedServiceId = null)
        {
            try
            {
                var query = _context.SharedServices
                    .Where(ss => ss.ServiceId == serviceId && 
                               ss.DepartmentId == departmentId && 
                               !ss.IsDeleted);

                if (excludeSharedServiceId.HasValue)
                    query = query.Where(ss => ss.SharedServiceId != excludeSharedServiceId.Value);

                return await query.AnyAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ServiceResult> ValidateSharedServiceModelAsync(SharedServiceCreateEditViewModel model)
        {
            try
            {
                // بررسی وجود خدمت
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId && !s.IsDeleted);
                if (service == null)
                    return ServiceResult.Failed("خدمت مورد نظر یافت نشد");

                // بررسی وجود دپارتمان
                var department = await _context.Departments
                    .FirstOrDefaultAsync(d => d.DepartmentId == model.DepartmentId && !d.IsDeleted);
                if (department == null)
                    return ServiceResult<bool>.Failed("دپارتمان مورد نظر یافت نشد");

                // بررسی تکراری نبودن
                var isDuplicate = await IsSharedServiceDuplicateAsync(model.ServiceId, model.DepartmentId, model.SharedServiceId);
                if (isDuplicate)
                    return ServiceResult<bool>.Failed("این خدمت قبلاً در این دپارتمان تعریف شده است");

                return ServiceResult<bool>.Successful(true, "مدل معتبر است");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failed($"خطا در اعتبارسنجی: {ex.Message}");
            }
        }

        #endregion

        public async Task<ServiceResult<bool>> AddServiceToDepartment(int serviceId, int departmentId, string userId)
        {
            try
            {
                // بررسی وجود قبلی
                var existing = await _context.SharedServices
                    .FirstOrDefaultAsync(ss => ss.ServiceId == serviceId && 
                                             ss.DepartmentId == departmentId && 
                                             !ss.IsDeleted);

                if (existing != null)
                {
                    if (existing.IsDeleted)
                    {
                        // بازیابی خدمت حذف شده
                        existing.IsDeleted = false;
                        existing.IsActive = true;
                        existing.UpdatedAt = DateTime.UtcNow;
                        existing.UpdatedByUserId = userId;
                    }
                    else
                    {
                        return ServiceResult<bool>.Failed("این خدمت قبلاً در این دپارتمان تعریف شده است");
                    }
                }
                else
                {
                    // ایجاد خدمت مشترک جدید
                    var sharedService = new SharedService
                    {
                        ServiceId = serviceId,
                        DepartmentId = departmentId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = userId
                    };

                    _context.SharedServices.Add(sharedService);
                }

                await _context.SaveChangesAsync();
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failed("خطا در اضافه کردن خدمت به دپارتمان");
            }
        }
    }

    /// <summary>
    /// آمار خدمات مشترک
    /// </summary>
    public class SharedServiceStatistics
    {
        public int TotalSharedServices { get; set; }
        public int ActiveSharedServices { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalServices { get; set; }
        public double AverageServicesPerDepartment { get; set; }
    }
}
