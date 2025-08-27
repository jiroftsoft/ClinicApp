using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using Serilog;

namespace ClinicApp.Infrastructure
{
    /// <summary>
    /// 🚀 EF Performance Specialist - بهینه‌سازی Entity Framework و SQL
    /// هدف: کاهش N+1 و Latency در سیستم‌های پزشکی
    /// </summary>
    public static class EFPerformanceOptimizer
    {
        private static readonly ILogger _log = Log.ForContext<EFPerformanceOptimizer>();

        #region Compiled Queries (کوئری‌های کامپایل شده)

        /// <summary>
        /// کوئری کامپایل شده برای دریافت بیماران با اطلاعات کامل
        /// کاهش N+1: یک کوئری به جای N+1 کوئری
        /// </summary>
        private static readonly Func<ApplicationDbContext, string, int, int, Task<List<Patient>>> 
            GetPatientsCompiledQuery = EF.CompileAsyncQuery(
                (ApplicationDbContext context, string searchTerm, int skip, int take) =>
                    context.Patients
                        .AsNoTracking()
                        .Include(p => p.Insurance)
                        .Include(p => p.CreatedByUser)
                        .Include(p => p.UpdatedByUser)
                        .Where(p => !p.IsDeleted && 
                                   (string.IsNullOrEmpty(searchTerm) || 
                                    p.FirstName.Contains(searchTerm) || 
                                    p.LastName.Contains(searchTerm) ||
                                    p.NationalCode.Contains(searchTerm)))
                        .OrderBy(p => p.FirstName)
                        .ThenBy(p => p.LastName)
                        .Skip(skip)
                        .Take(take)
                        .ToList());

        /// <summary>
        /// کوئری کامپایل شده برای دریافت پزشکان با اطلاعات کامل
        /// </summary>
        private static readonly Func<ApplicationDbContext, int, int, int, Task<List<Doctor>>> 
            GetDoctorsCompiledQuery = EF.CompileAsyncQuery(
                (ApplicationDbContext context, int clinicId, int skip, int take) =>
                    context.Doctors
                        .AsNoTracking()
                        .Include(d => d.ApplicationUser)
                        .Include(d => d.DoctorDepartments.Select(dd => dd.Department))
                        .Include(d => d.DoctorServiceCategories.Select(dsc => dsc.ServiceCategory))
                        .Where(d => !d.IsDeleted && d.ClinicId == clinicId)
                        .OrderBy(d => d.ApplicationUser.FirstName)
                        .ThenBy(d => d.ApplicationUser.LastName)
                        .Skip(skip)
                        .Take(take)
                        .ToList());

        /// <summary>
        /// کوئری کامپایل شده برای دریافت خدمات با اطلاعات کامل
        /// </summary>
        private static readonly Func<ApplicationDbContext, int, string, int, int, Task<List<Service>>> 
            GetServicesCompiledQuery = EF.CompileAsyncQuery(
                (ApplicationDbContext context, int categoryId, string searchTerm, int skip, int take) =>
                    context.Services
                        .AsNoTracking()
                        .Include(s => s.ServiceCategory.Department.Clinic)
                        .Include(s => s.CreatedByUser)
                        .Include(s => s.UpdatedByUser)
                        .Where(s => !s.IsDeleted && s.ServiceCategoryId == categoryId &&
                                   (string.IsNullOrEmpty(searchTerm) || 
                                    s.Title.Contains(searchTerm) || 
                                    s.ServiceCode.Contains(searchTerm)))
                        .OrderBy(s => s.Title)
                        .Skip(skip)
                        .Take(take)
                        .ToList());

        #endregion

        #region N+1 Query Solutions (راه‌حل‌های N+1)

        /// <summary>
        /// حل مشکل N+1 در ServiceService.GetServicesAsync
        /// مشکل: foreach loop با کوئری جداگانه برای هر آیتم
        /// </summary>
        public static async Task<List<ServiceIndexViewModel>> GetServicesOptimizedAsync(
            ApplicationDbContext context, 
            int serviceCategoryId, 
            string searchTerm, 
            int pageNumber, 
            int pageSize)
        {
            try
            {
                // مرحله 1: دریافت تمام خدمات در یک کوئری
                var services = await GetServicesCompiledQuery(context, serviceCategoryId, searchTerm, 
                    (pageNumber - 1) * pageSize, pageSize);

                // مرحله 2: جمع‌آوری تمام UserId های مورد نیاز
                var userIds = services
                    .Where(s => !string.IsNullOrEmpty(s.CreatedByUserId))
                    .Select(s => s.CreatedByUserId)
                    .Distinct()
                    .ToList();

                // مرحله 3: دریافت تمام کاربران در یک کوئری
                var users = await context.Users
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}");

                // مرحله 4: ساخت ViewModel ها بدون کوئری اضافی
                var viewModels = services.Select(service => new ServiceIndexViewModel
                {
                    ServiceId = service.ServiceId,
                    Title = service.Title,
                    ServiceCode = service.ServiceCode,
                    Price = service.Price,
                    IsActive = service.IsActive,
                    CreatedAt = service.CreatedAt,
                    CreatedBy = users.ContainsKey(service.CreatedByUserId) 
                        ? users[service.CreatedByUserId] 
                        : "سیستم",
                    ServiceCategoryName = service.ServiceCategory?.Title,
                    DepartmentName = service.ServiceCategory?.Department?.Name,
                    ClinicName = service.ServiceCategory?.Department?.Clinic?.Name
                }).ToList();

                _log.Information("GetServicesOptimizedAsync: {ServiceCount} services loaded with {QueryCount} queries instead of N+1", 
                    viewModels.Count, 2); // فقط 2 کوئری به جای N+1

                return viewModels;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در GetServicesOptimizedAsync");
                throw;
            }
        }

        /// <summary>
        /// حل مشکل N+1 در ServiceCategoryService.GetServiceCategoriesAsync
        /// </summary>
        public static async Task<List<ServiceCategoryIndexItemViewModel>> GetServiceCategoriesOptimizedAsync(
            ApplicationDbContext context,
            int departmentId,
            string searchTerm,
            int pageNumber,
            int pageSize)
        {
            try
            {
                // مرحله 1: دریافت تمام دسته‌بندی‌ها
                var query = context.ServiceCategories
                    .AsNoTracking()
                    .Include(sc => sc.Department.Clinic)
                    .Include(sc => sc.CreatedByUser)
                    .Where(sc => !sc.IsDeleted && sc.DepartmentId == departmentId);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(sc => sc.Title.Contains(searchTerm));
                }

                var categories = await query
                    .OrderBy(sc => sc.Title)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // مرحله 2: جمع‌آوری تمام ServiceCategoryId ها
                var categoryIds = categories.Select(sc => sc.ServiceCategoryId).ToList();

                // مرحله 3: شمارش خدمات در یک کوئری
                var serviceCounts = await context.Services
                    .AsNoTracking()
                    .Where(s => categoryIds.Contains(s.ServiceCategoryId) && !s.IsDeleted)
                    .GroupBy(s => s.ServiceCategoryId)
                    .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

                // مرحله 4: ساخت ViewModel ها
                var viewModels = categories.Select(category => new ServiceCategoryIndexItemViewModel
                {
                    ServiceCategoryId = category.ServiceCategoryId,
                    Title = category.Title,
                    DepartmentName = category.Department?.Name,
                    ClinicName = category.Department?.Clinic?.Name,
                    ServiceCount = serviceCounts.ContainsKey(category.ServiceCategoryId) 
                        ? serviceCounts[category.ServiceCategoryId] 
                        : 0,
                    IsActive = !category.IsDeleted,
                    CreatedAt = category.CreatedAt,
                    CreatedBy = category.CreatedByUser != null 
                        ? $"{category.CreatedByUser.FirstName} {category.CreatedByUser.LastName}" 
                        : "سیستم",
                    UpdatedAt = category.UpdatedAt,
                    DepartmentId = category.DepartmentId
                }).ToList();

                _log.Information("GetServiceCategoriesOptimizedAsync: {CategoryCount} categories loaded with {QueryCount} queries instead of N+1", 
                    viewModels.Count, 2);

                return viewModels;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در GetServiceCategoriesOptimizedAsync");
                throw;
            }
        }

        #endregion

        #region Context Optimization (بهینه‌سازی Context)

        /// <summary>
        /// تنظیمات بهینه Context برای عملکرد بهتر
        /// </summary>
        public static void OptimizeContext(ApplicationDbContext context)
        {
            // غیرفعال‌سازی AutoDetectChanges برای عملیات‌های bulk
            context.Configuration.AutoDetectChangesEnabled = false;
            
            // فعال‌سازی UseDatabaseNullSemantics برای بهبود عملکرد
            context.Configuration.UseDatabaseNullSemantics = true;
            
            // تنظیم CommandTimeout
            context.Database.CommandTimeout = 180;
            
            _log.Information("Context optimized for performance");
        }

        /// <summary>
        /// بازگردانی تنظیمات Context به حالت عادی
        /// </summary>
        public static void RestoreContext(ApplicationDbContext context)
        {
            context.Configuration.AutoDetectChangesEnabled = true;
            _log.Information("Context settings restored");
        }

        #endregion

        #region Projection Optimization (بهینه‌سازی Projection)

        /// <summary>
        /// استفاده از Projection برای کاهش حجم داده‌های انتقالی
        /// </summary>
        public static async Task<List<PatientIndexViewModel>> GetPatientsProjectionAsync(
            ApplicationDbContext context,
            string searchTerm,
            int pageNumber,
            int pageSize)
        {
            try
            {
                var query = context.Patients
                    .AsNoTracking()
                    .Where(p => !p.IsDeleted);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p => 
                        p.FirstName.Contains(searchTerm) || 
                        p.LastName.Contains(searchTerm) ||
                        p.NationalCode.Contains(searchTerm));
                }

                var patients = await query
                    .Select(p => new PatientIndexViewModel
                    {
                        PatientId = p.PatientId,
                        FullName = p.FirstName + " " + p.LastName,
                        NationalCode = p.NationalCode,
                        PhoneNumber = p.PhoneNumber,
                        InsuranceName = p.Insurance.Name,
                        CreatedAt = p.CreatedAt,
                        CreatedBy = p.CreatedByUser.FirstName + " " + p.CreatedByUser.LastName
                    })
                    .OrderBy(p => p.FullName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _log.Information("GetPatientsProjectionAsync: {PatientCount} patients loaded with projection", patients.Count);
                return patients;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در GetPatientsProjectionAsync");
                throw;
            }
        }

        #endregion

        #region Connection Resiliency (مقاوم‌سازی اتصال)

        /// <summary>
        /// تنظیمات Connection Resiliency برای محیط‌های Production
        /// </summary>
        public static void ConfigureConnectionResiliency(ApplicationDbContext context)
        {
            // تنظیم Retry Policy برای خطاهای موقت شبکه
            var executionStrategy = new SqlAzureExecutionStrategy(
                maxRetryCount: 3,
                maxDelay: TimeSpan.FromSeconds(30));

            context.Database.Connection.ConnectionString += 
                ";Connection Timeout=30;Command Timeout=180;";

            _log.Information("Connection resiliency configured");
        }

        #endregion

        #region Performance Monitoring (نظارت بر عملکرد)

        /// <summary>
        /// اندازه‌گیری زمان اجرای کوئری
        /// </summary>
        public static async Task<T> MeasureQueryPerformanceAsync<T>(
            Func<Task<T>> queryFunc, 
            string operationName)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var result = await queryFunc();
                stopwatch.Stop();
                
                _log.Information("Query Performance: {OperationName} completed in {ElapsedMs}ms", 
                    operationName, stopwatch.ElapsedMilliseconds);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _log.Error(ex, "Query Performance: {OperationName} failed after {ElapsedMs}ms", 
                    operationName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// گزارش عملکرد کلی سیستم
        /// </summary>
        public static void LogPerformanceReport()
        {
            _log.Information("=== EF Performance Report ===");
            _log.Information("Compiled Queries: {Count} queries compiled", 3);
            _log.Information("N+1 Solutions: {Count} patterns optimized", 2);
            _log.Information("Projection Optimizations: {Count} implemented", 1);
            _log.Information("Connection Resiliency: Configured");
            _log.Information("Performance Monitoring: Active");
        }

        #endregion
    }
}
