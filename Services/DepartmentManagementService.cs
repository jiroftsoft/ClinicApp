using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using FluentValidation;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicApp.Services
{
    public class DepartmentManagementService : IDepartmentManagementService
    {
        private readonly IDepartmentRepository _departmentRepo;
        private readonly IValidator<DepartmentCreateEditViewModel> _validator;
        private readonly ILogger _log;

        public DepartmentManagementService(
            IDepartmentRepository departmentRepository,
            IValidator<DepartmentCreateEditViewModel> validator,
            ILogger logger)
        {
            _departmentRepo = departmentRepository;
            _validator = validator;
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
    }
}