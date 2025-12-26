using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using FluentValidation;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.ViewModels.ClinicAdmin;

namespace ClinicApp.Services
{
    public class DepartmentManagementService : IDepartmentManagementService
    {
        private readonly IDepartmentRepository _departmentRepo;
        private readonly IValidator<DepartmentCreateEditViewModel> _validator;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _log;

        public DepartmentManagementService(
            IDepartmentRepository departmentRepository,
            IValidator<DepartmentCreateEditViewModel> validator,
            ApplicationDbContext context,
            ILogger logger)
        {
            _departmentRepo = departmentRepository;
            _validator = validator;
            _context = context;
            _log = logger.ForContext<DepartmentManagementService>();
        }

        // In Services/DepartmentManagementService.cs
        public async Task<ServiceResult<Department>> CreateDepartmentAsync(DepartmentCreateEditViewModel model)
        {
            _log.Information("Attempting to create a new department named {DepartmentName}", model.Name);

            var validationResult = await _validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                _log.Warning("Validation failed for creating department: {@ValidationErrors}", validationResult.Errors);
                // We return a failed result of the correct generic type
                return ServiceResult<Department>.FailedWithValidationErrors("اطلاعات ورودی نامعتبر است.", validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
            }

            try
            {
                var department = new Department();
                model.MapToEntity(department);

                _departmentRepo.Add(department);
                await _departmentRepo.SaveChangesAsync();

                _log.Information("Successfully created new department {DepartmentName} with ID {DepartmentId}", department.Name, department.DepartmentId);

                // ✅ **THE FIX:** Return the newly created 'department' object within a successful generic result.
                return ServiceResult<Department>.Successful(department, "دپارتمان با موفقیت ایجاد شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "A system error occurred while creating department {DepartmentName}", model.Name);
                return ServiceResult<Department>.Failed("خطای سیستمی در هنگام ایجاد دپارتمان رخ داد.", "DB_ERROR", ErrorCategory.Database);
            }
        }

        public async Task<ServiceResult<ClinicApp.Interfaces.PagedResult<DepartmentIndexViewModel>>> GetDepartmentsAsync(int clinicId, string searchTerm, int pageNumber, int pageSize)
        {
            try
            {
                // ابتدا تعداد کل دپارتمان‌ها را دریافت می‌کنیم
                var allDepartments = await _departmentRepo.GetDepartmentsAsync(clinicId, searchTerm);
                var totalCount = allDepartments.Count;

                // سپس pagination اعمال می‌کنیم
                var pagedDepartments = allDepartments
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // تبدیل به ViewModel
                var viewModels = pagedDepartments.Select(DepartmentIndexViewModel.FromEntity).ToList();
                
                // ایجاد PagedResult با تعداد صحیح
                var pagedResult = new ClinicApp.Interfaces.PagedResult<DepartmentIndexViewModel>(
                    viewModels, totalCount, pageNumber, pageSize);

                _log.Information("Retrieved {ItemCount} departments (page {PageNumber} of {TotalPages}) for clinic {ClinicId}", 
                    viewModels.Count, pageNumber, pagedResult.TotalPages, clinicId);

                return ServiceResult<ClinicApp.Interfaces.PagedResult<DepartmentIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error retrieving departments for ClinicId {ClinicId} with search term '{SearchTerm}'", clinicId, searchTerm);
                return ServiceResult<ClinicApp.Interfaces.PagedResult<DepartmentIndexViewModel>>.Failed("خطای سیستمی در بازیابی اطلاعات رخ داد.", "DB_ERROR");
            }
        }

        public async Task<ServiceResult<DepartmentDetailsViewModel>> GetDepartmentDetailsAsync(int departmentId)
        {
            try
            {
                var department = await _departmentRepo.GetByIdAsync(departmentId);
                if (department == null)
                    return ServiceResult<DepartmentDetailsViewModel>.Failed("دپارتمان مورد نظر یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);

                var viewModel = DepartmentDetailsViewModel.FromEntity(department);
                return ServiceResult<DepartmentDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error retrieving details for DepartmentId {DepartmentId}", departmentId);
                return ServiceResult<DepartmentDetailsViewModel>.Failed("خطای سیستمی در بازیابی اطلاعات رخ داد.", "DB_ERROR");
            }
        }

        public async Task<ServiceResult<DepartmentCreateEditViewModel>> GetDepartmentForEditAsync(int departmentId)
        {
            try
            {
                var department = await _departmentRepo.GetByIdAsync(departmentId);
                if (department == null)
                    return ServiceResult<DepartmentCreateEditViewModel>.Failed("دپارتمان مورد نظر یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);

                var viewModel = DepartmentCreateEditViewModel.FromEntity(department);
                return ServiceResult<DepartmentCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error retrieving department for edit: {DepartmentId}", departmentId);
                return ServiceResult<DepartmentCreateEditViewModel>.Failed("خطای سیستمی در بازیابی اطلاعات رخ داد.", "DB_ERROR");
            }
        }

        // In Services/DepartmentManagementService.cs
        public async Task<ServiceResult<Department>> UpdateDepartmentAsync(DepartmentCreateEditViewModel model)
        {
            _log.Information("Attempting to update DepartmentId {DepartmentId}", model.DepartmentId);

            var validationResult = await _validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                _log.Warning("Validation failed for updating department {DepartmentId}: {@ValidationErrors}", model.DepartmentId, validationResult.Errors);
                return ServiceResult<Department>.FailedWithValidationErrors("اطلاعات ورودی نامعتبر است.", validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)));
            }

            try
            {
                var department = await _departmentRepo.GetByIdAsync(model.DepartmentId);
                if (department == null)
                    return ServiceResult<Department>.Failed("دپارتمان مورد نظر برای ویرایش یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);

                model.MapToEntity(department);
                _departmentRepo.Update(department);
                await _departmentRepo.SaveChangesAsync();

                _log.Information("DepartmentId {DepartmentId} updated successfully.", department.DepartmentId);

                // ✅ **THE FIX:** Return the updated 'department' object within a successful generic result.
                return ServiceResult<Department>.Successful(department, "دپارتمان با موفقیت به‌روزرسانی شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "System error while updating DepartmentId {DepartmentId}", model.DepartmentId);
                return ServiceResult<Department>.Failed("خطای سیستمی در هنگام بروزرسانی دپارتمان رخ داد.", "DB_ERROR", ErrorCategory.Database);
            }
        }

        public async Task<ServiceResult> SoftDeleteDepartmentAsync(int departmentId)
        {
            try
            {
                var department = await _departmentRepo.GetByIdAsync(departmentId);
                if (department == null)
                    return ServiceResult.Failed("دپارتمان مورد نظر یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);

                // 🏥 MEDICAL: Business Rule - Check for active service categories before deleting
                if (department.ServiceCategories?.Any(sc => sc.IsActive && !sc.IsDeleted) == true)
                {
                    var activeCategoryCount = department.ServiceCategories.Count(sc => sc.IsActive && !sc.IsDeleted);
                    _log.Warning("🏥 MEDICAL: Attempted to delete department with active service categories. DepartmentId: {DepartmentId}, ActiveCategories: {ActiveCategoryCount}", 
                        departmentId, activeCategoryCount);
                    return ServiceResult.Failed($"امکان حذف دپارتمان دارای {activeCategoryCount} دسته‌بندی خدمات فعال وجود ندارد. ابتدا تمام دسته‌بندی‌های خدمات را حذف کنید.", "BUSINESS_RULE_VIOLATION");
                }

                // 🏥 MEDICAL: Business Rule - Check for active doctors before deleting
                if (department.DoctorDepartments?.Any(dd => dd.Doctor.IsActive && !dd.Doctor.IsDeleted) == true)
                {
                    var activeDoctorCount = department.DoctorDepartments.Count(dd => dd.Doctor.IsActive && !dd.Doctor.IsDeleted);
                    _log.Warning("🏥 MEDICAL: Attempted to delete department with active doctors. DepartmentId: {DepartmentId}, ActiveDoctors: {ActiveDoctorCount}", 
                        departmentId, activeDoctorCount);
                    return ServiceResult.Failed($"امکان حذف دپارتمان دارای {activeDoctorCount} پزشک فعال وجود ندارد. ابتدا تمام پزشکان را حذف کنید.", "BUSINESS_RULE_VIOLATION");
                }

                _departmentRepo.Delete(department);
                await _departmentRepo.SaveChangesAsync();

                _log.Information("🏥 MEDICAL: DepartmentId {DepartmentId} was soft-deleted successfully.", departmentId);
                return ServiceResult.Successful("دپارتمان با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: Error during soft-delete for DepartmentId: {DepartmentId}", departmentId);
                return ServiceResult.Failed("خطای سیستمی در حذف دپارتمان رخ داد.", "DB_ERROR");
            }
        }

        public async Task<ServiceResult> RestoreDepartmentAsync(int departmentId)
        {
            // The actual restoration logic (setting IsDeleted=false) is handled automatically 
            // by our ApplicationDbContext's SaveChanges override. Here, we just need to find
            // the entity and save it.
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<List<LookupItemViewModel>>> GetActiveDepartmentsForLookupAsync(int clinicId)
        {
            try
            {
                var activeDepartments = await _departmentRepo.GetActiveDepartmentsAsync(clinicId);
                var lookupItems = activeDepartments
                    .Select(d => new LookupItemViewModel { Id = d.DepartmentId, Name = d.Name })
                    .ToList();
                return ServiceResult<List<LookupItemViewModel>>.Successful(lookupItems);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error retrieving active departments for lookup for ClinicId {ClinicId}", clinicId);
                return ServiceResult<List<LookupItemViewModel>>.Failed("خطای سیستمی در بازیابی اطلاعات رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت تمام دپارتمان‌ها
        /// </summary>
        public async Task<ServiceResult<List<DepartmentDto>>> GetAllDepartmentsAsync()
        {
            try
            {
                _log.Information("Getting all departments");
                var departments = await _departmentRepo.GetDepartmentsAsync(1, ""); // TODO: Fix clinicId
                var departmentDtos = departments.Select(d => new DepartmentDto
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    Code = d.Code,
                    IsActive = d.IsActive,
                    Description = d.Description,
                    ClinicId = d.ClinicId,
                    ClinicName = d.Clinic?.Name ?? "",
                    CreatedAt = d.CreatedAt,
                    CreatedBy = d.CreatedByUser?.UserName ?? ""
                }).ToList();

                return ServiceResult<List<DepartmentDto>>.Successful(departmentDtos);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting all departments");
                return ServiceResult<List<DepartmentDto>>.Failed("خطا در دریافت دپارتمان‌ها");
            }
        }

        /// <summary>
        /// دریافت خدمات دپارتمان
        /// خدمات از طریق ServiceCategory.DepartmentId به دپارتمان لینک می‌شوند
        /// </summary>
        public async Task<ServiceResult<List<ServiceDto>>> GetDepartmentServicesAsync(int deptId)
        {
            try
            {
                _log.Information("🏥 Getting services for department {DeptId}", deptId);

                // دریافت خدمات از طریق ServiceCategory.DepartmentId
                var services = await _context.Services
                    .AsNoTracking()
                    .Include(s => s.ServiceCategory)
                    .Where(s => s.ServiceCategory.DepartmentId == deptId && 
                               !s.IsDeleted && 
                               s.IsActive &&
                               !s.ServiceCategory.IsDeleted &&
                               s.ServiceCategory.IsActive)
                    .OrderBy(s => s.Title)
                    .Select(s => new ServiceDto
                    {
                        ServiceId = s.ServiceId,
                        ServiceCode = s.ServiceCode,
                        ServiceName = s.Title,
                        Price = s.Price,
                        IsActive = s.IsActive
                    })
                    .ToListAsync();

                _log.Information("✅ Found {Count} services for department {DeptId}", services.Count, deptId);
                return ServiceResult<List<ServiceDto>>.Successful(services);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ Error getting department services for {DeptId}", deptId);
                return ServiceResult<List<ServiceDto>>.Failed("خطا در دریافت خدمات دپارتمان");
            }
        }

        /// <summary>
        /// دریافت خدمات مشترک
        /// خدمات مشترک از جدول SharedService که در چندین دپارتمان قابل استفاده هستند
        /// </summary>
        public async Task<ServiceResult<List<ServiceDto>>> GetSharedServicesAsync()
        {
            try
            {
                _log.Information("🏥 Getting shared services");

                // دریافت خدمات مشترک از جدول SharedService
                var sharedServices = await _context.SharedServices
                    .AsNoTracking()
                    .Include(ss => ss.Service)
                    .Include(ss => ss.Service.ServiceCategory)
                    .Where(ss => !ss.IsDeleted && 
                                ss.IsActive &&
                                !ss.Service.IsDeleted &&
                                ss.Service.IsActive &&
                                !ss.Service.ServiceCategory.IsDeleted &&
                                ss.Service.ServiceCategory.IsActive)
                    .Select(ss => ss.Service)
                    .Distinct()
                    .OrderBy(s => s.Title)
                    .Select(s => new ServiceDto
                    {
                        ServiceId = s.ServiceId,
                        ServiceCode = s.ServiceCode,
                        ServiceName = s.Title,
                        Price = s.Price,
                        IsActive = s.IsActive
                    })
                    .ToListAsync();

                _log.Information("✅ Found {Count} shared services", sharedServices.Count);
                return ServiceResult<List<ServiceDto>>.Successful(sharedServices);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ Error getting shared services");
                return ServiceResult<List<ServiceDto>>.Failed("خطا در دریافت خدمات مشترک");
            }
        }

        /// <summary>
        /// دریافت دپارتمان‌های مناسب برای نمایش در فرم پذیرش
        /// 
        /// این متد تنها دپارتمان‌هایی را برمی‌گرداند که:
        /// 1. فعال و حذف نشده باشند
        /// 2. نوع مناسبی داشته باشند (درمانی، پاراکلینیک، اورژانس، تزریقات، ...)
        /// 3. حداقل یک خدمت فعال داشته باشند
        /// 
        /// 🏥 MEDICAL ENVIRONMENT - PRODUCTION READY:
        /// - بهبود سرعت کار منشی
        /// - عدم نمایش دپارتمان‌های بدون خدمت
        /// - فیلتر خودکار بر اساس نوع دپارتمان
        /// - استفاده از Repository برای تفکیک concerns
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <returns>لیست دپارتمان‌های مناسب برای فرم پذیرش</returns>
        public async Task<ServiceResult<List<DepartmentDto>>> GetDepartmentsForReceptionAsync(int? clinicId = null)
        {
            try
            {
                _log.Information("🏥 RECEPTION: دریافت دپارتمان‌های مناسب برای پذیرش - ClinicId: {ClinicId}", clinicId);

                // دریافت دپارتمان‌ها از Repository
                var departments = await _departmentRepo.GetDepartmentsForReceptionAsync(clinicId);

                // تبدیل به DTO
                var departmentDtos = departments.Select(d => new DepartmentDto
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    Code = d.Code,
                    IsActive = d.IsActive,
                    Description = d.Description,
                    ClinicId = d.ClinicId,
                    ClinicName = d.Clinic?.Name ?? "",
                    CreatedAt = d.CreatedAt,
                    CreatedBy = d.CreatedByUser?.UserName ?? ""
                }).ToList();

                _log.Information("✅ RECEPTION: دپارتمان‌های مناسب دریافت شد - تعداد: {Count}", departmentDtos.Count);

                return ServiceResult<List<DepartmentDto>>.Successful(departmentDtos);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ RECEPTION: خطا در دریافت دپارتمان‌های مناسب برای پذیرش");
                return ServiceResult<List<DepartmentDto>>.Failed("خطا در دریافت دپارتمان‌ها برای پذیرش");
            }
        }
    }
}