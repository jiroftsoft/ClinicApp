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
    /// <summary>
    /// Final, production-ready service for managing Service Categories and Services.
    /// This service contains all business logic and orchestrates data operations via repositories.
    /// 
    /// Architecture Principles Applied:
    /// ✅ Single Responsibility: Owns all service/category business logic
    /// ✅ Dependency Inversion: Depends on repository abstractions
    /// ✅ Clean Architecture: Service layer orchestrates domain operations
    /// ✅ Medical Standards: Implements healthcare industry best practices
    /// ✅ Persian Support: Full localization for Iranian medical environments
    /// 
    /// Flow: Controller -> ServiceManagementService -> Repository -> Database
    /// </summary>
    public class ServiceManagementService : IServiceManagementService
    {
        private readonly IServiceCategoryRepository _categoryRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IValidator<ServiceCategoryCreateEditViewModel> _categoryValidator;
        private readonly IValidator<ServiceCreateEditViewModel> _serviceValidator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _log;

        public ServiceManagementService(
            IServiceCategoryRepository categoryRepository,
            IServiceRepository serviceRepository,
            IValidator<ServiceCategoryCreateEditViewModel> categoryValidator,
            IValidator<ServiceCreateEditViewModel> serviceValidator,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _categoryRepo = categoryRepository;
            _serviceRepo = serviceRepository;
            _categoryValidator = categoryValidator;
            _serviceValidator = serviceValidator;
            _currentUserService = currentUserService;
            _log = logger.ForContext<ServiceManagementService>();
        }

        #region Service Category Management (مدیریت دسته‌بندی خدمات)

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدمات یک دپارتمان خاص.
        /// </summary>
        public async Task<ServiceResult<PagedResult<ServiceCategoryIndexViewModel>>> GetServiceCategoriesAsync(
            int departmentId, string searchTerm, int pageNumber, int pageSize)
        {
            _log.Information("درخواست لیست دسته‌بندی‌ها. DepartmentId: {DepartmentId}, Page: {Page}. User: {UserId}",
                departmentId, pageNumber, _currentUserService.UserId);

            try
            {
                // Validation
                if (departmentId <= 0)
                {
                    return ServiceResult<PagedResult<ServiceCategoryIndexViewModel>>.Failed("شناسه دپارتمان معتبر نیست.");
                }

                // Fetch data from repository
                var categories = await _categoryRepo.GetServiceCategoriesAsync(departmentId, searchTerm);
                
                // Convert to ViewModels
                var viewModels = categories.Select(ServiceCategoryIndexViewModel.FromEntity).ToList();
                
                // Apply pagination (simple in-memory pagination for now)
                var totalItems = viewModels.Count;
                var pagedItems = viewModels
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagedResult = new PagedResult<ServiceCategoryIndexViewModel>
                {
                    Items = pagedItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };

                _log.Information("لیست دسته‌بندی‌ها بازیابی شد. Total: {Total}. User: {UserId}",
                    totalItems, _currentUserService.UserId);

                return ServiceResult<PagedResult<ServiceCategoryIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی لیست دسته‌بندی‌ها. DepartmentId: {DepartmentId}. User: {UserId}",
                    departmentId, _currentUserService.UserId);
                return ServiceResult<PagedResult<ServiceCategoryIndexViewModel>>.Failed("خطا در بازیابی اطلاعات.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت جزئیات کامل یک دسته‌بندی خدمات.
        /// </summary>
        public async Task<ServiceResult<ServiceCategoryDetailsViewModel>> GetServiceCategoryDetailsAsync(int serviceCategoryId)
        {
            _log.Information("درخواست جزئیات دسته‌بندی. Id: {Id}. User: {UserId}",
                serviceCategoryId, _currentUserService.UserId);

            try
            {
                if (serviceCategoryId <= 0)
                {
                    return ServiceResult<ServiceCategoryDetailsViewModel>.Failed("شناسه دسته‌بندی معتبر نیست.");
                }

                var category = await _categoryRepo.GetByIdAsync(serviceCategoryId);
                if (category == null)
                {
                    return ServiceResult<ServiceCategoryDetailsViewModel>.Failed("دسته‌بندی مورد نظر یافت نشد.");
                }

                var viewModel = ServiceCategoryDetailsViewModel.FromEntity(category);
                return ServiceResult<ServiceCategoryDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی جزئیات دسته‌بندی. Id: {Id}. User: {UserId}",
                    serviceCategoryId, _currentUserService.UserId);
                return ServiceResult<ServiceCategoryDetailsViewModel>.Failed("خطا در بازیابی اطلاعات.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت اطلاعات یک دسته‌بندی خدمات برای فرم ویرایش.
        /// </summary>
        public async Task<ServiceResult<ServiceCategoryCreateEditViewModel>> GetServiceCategoryForEditAsync(int serviceCategoryId)
        {
            _log.Information("درخواست ویرایش دسته‌بندی. Id: {Id}. User: {UserId}",
                serviceCategoryId, _currentUserService.UserId);

            try
            {
                if (serviceCategoryId <= 0)
                {
                    return ServiceResult<ServiceCategoryCreateEditViewModel>.Failed("شناسه دسته‌بندی معتبر نیست.");
                }

                var category = await _categoryRepo.GetByIdAsync(serviceCategoryId);
                if (category == null)
                {
                    return ServiceResult<ServiceCategoryCreateEditViewModel>.Failed("دسته‌بندی مورد نظر یافت نشد.");
                }

                var viewModel = ServiceCategoryCreateEditViewModel.FromEntity(category);
                return ServiceResult<ServiceCategoryCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی اطلاعات ویرایش دسته‌بندی. Id: {Id}. User: {UserId}",
                    serviceCategoryId, _currentUserService.UserId);
                return ServiceResult<ServiceCategoryCreateEditViewModel>.Failed("خطا در بازیابی اطلاعات.", "DB_ERROR");
            }
        }

        /// <summary>
        /// ایجاد یک دسته‌بندی خدمات جدید.
        /// </summary>
        public async Task<ServiceResult> CreateServiceCategoryAsync(ServiceCategoryCreateEditViewModel model)
        {
            _log.Information("درخواست ایجاد دسته‌بندی. Title: {Title}. User: {UserId}",
                model?.Title, _currentUserService.UserId);

            try
            {
                // Validation
                var validationResult = await _categoryValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => 
                        new ValidationError(e.PropertyName, e.ErrorMessage));
                    return ServiceResult.FailedWithValidationErrors("اطلاعات ورودی نامعتبر است.", errors);
                }

                // Create entity
                var category = new ServiceCategory
                {
                    Title = model.Title?.Trim(),
                    Description = model.Description?.Trim(),
                    DepartmentId = model.DepartmentId,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId,
                    IsDeleted = false
                };

                // Save to repository
                _categoryRepo.Add(category);
                await _categoryRepo.SaveChangesAsync();
                
                _log.Information("دسته‌بندی جدید ایجاد شد. Id: {Id}, Title: {Title}. User: {UserId}",
                    category.ServiceCategoryId, category.Title, _currentUserService.UserId);

                return ServiceResult.Successful("دسته‌بندی خدمات با موفقیت ایجاد شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ایجاد دسته‌بندی. Title: {Title}. User: {UserId}",
                    model?.Title, _currentUserService.UserId);
                return ServiceResult.Failed("خطای سیستمی در ایجاد دسته‌بندی خدمات رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// به‌روزرسانی یک دسته‌بندی خدمات موجود.
        /// </summary>
        public async Task<ServiceResult> UpdateServiceCategoryAsync(ServiceCategoryCreateEditViewModel model)
        {
            _log.Information("درخواست به‌روزرسانی دسته‌بندی. Id: {Id}. User: {UserId}",
                model?.ServiceCategoryId, _currentUserService.UserId);

            try
            {
                // Validation
                var validationResult = await _categoryValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => 
                        new ValidationError(e.PropertyName, e.ErrorMessage));
                    return ServiceResult.FailedWithValidationErrors("اطلاعات ورودی نامعتبر است.", errors);
                }

                // Get existing entity
                var category = await _categoryRepo.GetByIdAsync(model.ServiceCategoryId);
                if (category == null)
                {
                    return ServiceResult.Failed("دسته‌بندی مورد نظر یافت نشد.");
                }

                // Update entity
                category.Title = model.Title?.Trim();
                category.Description = model.Description?.Trim();
                category.DepartmentId = model.DepartmentId;
                category.IsActive = model.IsActive;
                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedByUserId = _currentUserService.UserId;

                // Save changes
                _categoryRepo.Update(category);
                await _categoryRepo.SaveChangesAsync();
                
                _log.Information("دسته‌بندی به‌روزرسانی شد. Id: {Id}. User: {UserId}",
                    category.ServiceCategoryId, _currentUserService.UserId);

                return ServiceResult.Successful("دسته‌بندی خدمات با موفقیت به‌روزرسانی شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در به‌روزرسانی دسته‌بندی. Id: {Id}. User: {UserId}",
                    model?.ServiceCategoryId, _currentUserService.UserId);
                return ServiceResult.Failed("خطای سیستمی در به‌روزرسانی دسته‌بندی خدمات رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// حذف نرم یک دسته‌بندی خدمات.
        /// </summary>
        public async Task<ServiceResult> SoftDeleteServiceCategoryAsync(int serviceCategoryId)
        {
            _log.Information("🏥 MEDICAL: درخواست حذف نرم دسته‌بندی. Id: {Id}. User: {UserId}",
                serviceCategoryId, _currentUserService?.UserId ?? "NULL");

            try
            {
                if (serviceCategoryId <= 0)
                {
                    _log.Warning("🏥 MEDICAL: شناسه دسته‌بندی نامعتبر. Id: {Id}. User: {UserId}",
                        serviceCategoryId, _currentUserService?.UserId ?? "NULL");
                    return ServiceResult.Failed("شناسه دسته‌بندی معتبر نیست.");
                }

                var category = await _categoryRepo.GetByIdAsync(serviceCategoryId);
                if (category == null)
                {
                    _log.Warning("🏥 MEDICAL: دسته‌بندی یافت نشد. Id: {Id}. User: {UserId}",
                        serviceCategoryId, _currentUserService?.UserId ?? "NULL");
                    return ServiceResult.Failed("دسته‌بندی مورد نظر یافت نشد.");
                }

                // Check if category is already deleted
                if (category.IsDeleted)
                {
                    _log.Warning("🏥 MEDICAL: دسته‌بندی قبلاً حذف شده. Id: {Id}. User: {UserId}",
                        serviceCategoryId, _currentUserService?.UserId ?? "NULL");
                    return ServiceResult.Failed("دسته‌بندی مورد نظر قبلاً حذف شده است.");
                }

                // Business logic: Check if category has active services
                var activeServices = await _serviceRepo.GetActiveServicesAsync(serviceCategoryId);
                if (activeServices.Any())
                {
                    _log.Warning("🏥 MEDICAL: تلاش برای حذف دسته‌بندی دارای خدمات فعال. Id: {Id}, ActiveServices: {Count}. User: {UserId}",
                        serviceCategoryId, activeServices.Count(), _currentUserService?.UserId ?? "NULL");
                    return ServiceResult.Failed("نمی‌توان دسته‌بندی‌ای را حذف کرد که حاوی خدمات فعال است.");
                }

                // 🔒 تعیین شناسه کاربر معتبر برای عملیات حذف
                string validUserId = GetValidUserIdForOperation();

                _log.Information("🏥 MEDICAL: شناسه کاربر معتبر برای عملیات: {ValidUserId}. Original User: {OriginalUserId}",
                    validUserId, _currentUserService?.UserId ?? "NULL");

                // Soft delete
                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;
                category.DeletedByUserId = validUserId;
                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedByUserId = validUserId;

                _categoryRepo.Update(category);
                await _categoryRepo.SaveChangesAsync();
                
                _log.Information("🏥 MEDICAL: دسته‌بندی حذف شد. Id: {Id}. User: {UserId}",
                    serviceCategoryId, _currentUserService?.UserId ?? "NULL");

                return ServiceResult.Successful("دسته‌بندی خدمات با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در حذف دسته‌بندی. Id: {Id}, ExceptionType: {ExceptionType}, Message: {Message}. User: {UserId}",
                    serviceCategoryId, ex.GetType().Name, ex.Message, _currentUserService?.UserId ?? "NULL");
                return ServiceResult.Failed("خطای سیستمی در حذف دسته‌بندی رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// بازیابی یک دسته‌بندی خدمات حذف شده.
        /// </summary>
        public async Task<ServiceResult> RestoreServiceCategoryAsync(int serviceCategoryId)
        {
            _log.Information("درخواست بازیابی دسته‌بندی. Id: {Id}. User: {UserId}",
                serviceCategoryId, _currentUserService.UserId);

            try
            {
                if (serviceCategoryId <= 0)
                {
                    return ServiceResult.Failed("شناسه دسته‌بندی معتبر نیست.");
                }

                var category = await _categoryRepo.GetByIdAsync(serviceCategoryId);
                if (category == null)
                {
                    return ServiceResult.Failed("دسته‌بندی مورد نظر یافت نشد.");
                }

                if (!category.IsDeleted)
                {
                    return ServiceResult.Failed("دسته‌بندی حذف نشده است.");
                }

                // Restore
                category.IsDeleted = false;
                category.DeletedAt = null;
                category.DeletedByUserId = null;
                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedByUserId = _currentUserService.UserId;

                _categoryRepo.Update(category);
                await _categoryRepo.SaveChangesAsync();
                
                _log.Information("دسته‌بندی بازیابی شد. Id: {Id}. User: {UserId}",
                    serviceCategoryId, _currentUserService.UserId);

                return ServiceResult.Successful("دسته‌بندی خدمات با موفقیت بازیابی شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی دسته‌بندی. Id: {Id}. User: {UserId}",
                    serviceCategoryId, _currentUserService.UserId);
                return ServiceResult.Failed("خطای سیستمی در بازیابی دسته‌بندی خدمات رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های فعال یک دپارتمان برای لیست‌های کشویی.
        /// </summary>
        public async Task<ServiceResult<List<LookupItemViewModel>>> GetActiveServiceCategoriesForLookupAsync(int departmentId)
        {
            _log.Information("درخواست لیست دسته‌بندی‌های فعال. DepartmentId: {DepartmentId}. User: {UserId}",
                departmentId, _currentUserService.UserId);

            try
            {
                if (departmentId <= 0)
                {
                    return ServiceResult<List<LookupItemViewModel>>.Failed("شناسه دپارتمان معتبر نیست.");
                }

                var categories = await _categoryRepo.GetActiveServiceCategoriesAsync(departmentId);
                var lookupItems = categories.Select(c => new LookupItemViewModel
                {
                    Id = c.ServiceCategoryId,
                    Title = c.Title,
                    Description = c.Description
                }).ToList();

                return ServiceResult<List<LookupItemViewModel>>.Successful(lookupItems);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی لیست دسته‌بندی‌های فعال. DepartmentId: {DepartmentId}. User: {UserId}",
                    departmentId, _currentUserService.UserId);
                return ServiceResult<List<LookupItemViewModel>>.Failed("خطا در بازیابی اطلاعات.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت لیست تمام دسته‌بندی‌های خدمات (Medical Environment).
        /// </summary>
        public async Task<ServiceResult<PagedResult<ServiceCategoryIndexViewModel>>> GetAllServiceCategoriesAsync(
            string searchTerm, int pageNumber, int pageSize)
        {
            _log.Information("درخواست لیست تمام دسته‌بندی‌های خدمات. Page: {Page}. User: {UserId}",
                pageNumber, _currentUserService.UserId);

            try
            {
                // Fetch all categories from repository
                var categories = await _categoryRepo.GetAllServiceCategoriesAsync(searchTerm);
                
                // Convert to ViewModels
                var viewModels = categories.Select(ServiceCategoryIndexViewModel.FromEntity).ToList();
                
                // Apply pagination
                var totalItems = viewModels.Count;
                var pagedItems = viewModels
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagedResult = new PagedResult<ServiceCategoryIndexViewModel>
                {
                    Items = pagedItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };

                _log.Information("لیست تمام دسته‌بندی‌ها بازیابی شد. Total: {Total}. User: {UserId}",
                    totalItems, _currentUserService.UserId);

                return ServiceResult<PagedResult<ServiceCategoryIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی لیست تمام دسته‌بندی‌ها. User: {UserId}",
                    _currentUserService.UserId);
                return ServiceResult<PagedResult<ServiceCategoryIndexViewModel>>.Failed("خطا در بازیابی اطلاعات.", "DB_ERROR");
            }
        }

        #endregion

        #region Service Management (مدیریت خدمات)

        /// <summary>
        /// دریافت لیست خدمات یک دسته‌بندی خاص.
        /// </summary>
        public async Task<ServiceResult<PagedResult<ServiceIndexViewModel>>> GetServicesAsync(
            int serviceCategoryId, string searchTerm, int pageNumber, int pageSize)
        {
            _log.Information("درخواست لیست خدمات. CategoryId: {CategoryId}, Page: {Page}. User: {UserId}",
                serviceCategoryId, pageNumber, _currentUserService.UserId);

            try
            {
                if (serviceCategoryId <= 0)
                {
                    return ServiceResult<PagedResult<ServiceIndexViewModel>>.Failed("شناسه دسته‌بندی معتبر نیست.");
                }

                var services = await _serviceRepo.GetServicesAsync(serviceCategoryId, searchTerm);
                var viewModels = services.Select(ServiceIndexViewModel.FromEntity).ToList();
                
                // Apply pagination
                var totalItems = viewModels.Count;
                var pagedItems = viewModels
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagedResult = new PagedResult<ServiceIndexViewModel>
                {
                    Items = pagedItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };

                return ServiceResult<PagedResult<ServiceIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی لیست خدمات. CategoryId: {CategoryId}. User: {UserId}",
                    serviceCategoryId, _currentUserService.UserId);
                return ServiceResult<PagedResult<ServiceIndexViewModel>>.Failed("خطا در بازیابی اطلاعات.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت جزئیات کامل یک خدمت.
        /// </summary>
        public async Task<ServiceResult<ServiceDetailsViewModel>> GetServiceDetailsAsync(int serviceId)
        {
            _log.Information("درخواست جزئیات خدمت. Id: {Id}. User: {UserId}",
                serviceId, _currentUserService.UserId);

            try
            {
                if (serviceId <= 0)
                {
                    return ServiceResult<ServiceDetailsViewModel>.Failed("شناسه خدمت معتبر نیست.");
                }

                var service = await _serviceRepo.GetByIdAsync(serviceId);
                if (service == null)
                {
                    return ServiceResult<ServiceDetailsViewModel>.Failed("خدمت مورد نظر یافت نشد.");
                }

                var viewModel = ServiceDetailsViewModel.FromEntity(service);
                return ServiceResult<ServiceDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی جزئیات خدمت. Id: {Id}. User: {UserId}",
                    serviceId, _currentUserService.UserId);
                return ServiceResult<ServiceDetailsViewModel>.Failed("خطا در بازیابی اطلاعات.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت اطلاعات یک خدمت برای فرم ویرایش.
        /// </summary>
        public async Task<ServiceResult<ServiceCreateEditViewModel>> GetServiceForEditAsync(int serviceId)
        {
            _log.Information("درخواست ویرایش خدمت. Id: {Id}. User: {UserId}",
                serviceId, _currentUserService.UserId);

            try
            {
                if (serviceId <= 0)
                {
                    return ServiceResult<ServiceCreateEditViewModel>.Failed("شناسه خدمت معتبر نیست.");
                }

                var service = await _serviceRepo.GetByIdAsync(serviceId);
                if (service == null)
                {
                    return ServiceResult<ServiceCreateEditViewModel>.Failed("خدمت مورد نظر یافت نشد.");
                }

                var viewModel = ServiceCreateEditViewModel.FromEntity(service);
                return ServiceResult<ServiceCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی اطلاعات ویرایش خدمت. Id: {Id}. User: {UserId}",
                    serviceId, _currentUserService.UserId);
                return ServiceResult<ServiceCreateEditViewModel>.Failed("خطا در بازیابی اطلاعات.", "DB_ERROR");
            }
        }

        /// <summary>
        /// ایجاد یک خدمت جدید.
        /// </summary>
        public async Task<ServiceResult> CreateServiceAsync(ServiceCreateEditViewModel model)
        {
            _log.Information("درخواست ایجاد خدمت. Title: {Title}. User: {UserId}",
                model?.Title, _currentUserService.UserId);

            try
            {
                // Validation
                var validationResult = await _serviceValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => 
                        new ValidationError(e.PropertyName, e.ErrorMessage));
                    return ServiceResult.FailedWithValidationErrors("اطلاعات ورودی نامعتبر است.", errors);
                }

                // Create entity
                var service = new Service
                {
                    Title = model.Title?.Trim(),
                    ServiceCode = model.ServiceCode?.Trim(),
                    Price = model.Price,
                    Description = model.Description?.Trim(),
                    ServiceCategoryId = model.ServiceCategoryId,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId,
                    IsDeleted = false
                };

                // Save to repository
                _serviceRepo.Add(service);
                await _serviceRepo.SaveChangesAsync();
                
                _log.Information("خدمت جدید ایجاد شد. Id: {Id}, Title: {Title}. User: {UserId}",
                    service.ServiceId, service.Title, _currentUserService.UserId);

                return ServiceResult.Successful("خدمت با موفقیت ایجاد شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ایجاد خدمت. Title: {Title}. User: {UserId}",
                    model?.Title, _currentUserService.UserId);
                return ServiceResult.Failed("خطای سیستمی در ایجاد خدمت رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// به‌روزرسانی یک خدمت موجود.
        /// </summary>
        public async Task<ServiceResult> UpdateServiceAsync(ServiceCreateEditViewModel model)
        {
            _log.Information("درخواست به‌روزرسانی خدمت. Id: {Id}. User: {UserId}",
                model?.ServiceId, _currentUserService.UserId);

            try
            {
                // Validation
                var validationResult = await _serviceValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => 
                        new ValidationError(e.PropertyName, e.ErrorMessage));
                    return ServiceResult.FailedWithValidationErrors("اطلاعات ورودی نامعتبر است.", errors);
                }

                // Get existing entity
                var service = await _serviceRepo.GetByIdAsync(model.ServiceId);
                if (service == null)
                {
                    return ServiceResult.Failed("خدمت مورد نظر یافت نشد.");
                }

                // Update entity
                service.Title = model.Title?.Trim();
                service.ServiceCode = model.ServiceCode?.Trim();
                service.Price = model.Price;
                service.Description = model.Description?.Trim();
                service.ServiceCategoryId = model.ServiceCategoryId;
                service.IsActive = model.IsActive;
                service.UpdatedAt = DateTime.UtcNow;
                service.UpdatedByUserId = _currentUserService.UserId;

                // Save changes
                _serviceRepo.Update(service);
                await _serviceRepo.SaveChangesAsync();
                
                _log.Information("خدمت به‌روزرسانی شد. Id: {Id}. User: {UserId}",
                    service.ServiceId, _currentUserService.UserId);

                return ServiceResult.Successful("خدمت با موفقیت به‌روزرسانی شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در به‌روزرسانی خدمت. Id: {Id}. User: {UserId}",
                    model?.ServiceId, _currentUserService.UserId);
                return ServiceResult.Failed("خطای سیستمی در به‌روزرسانی خدمت رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// حذف نرم یک خدمت.
        /// </summary>
        public async Task<ServiceResult> SoftDeleteServiceAsync(int serviceId)
        {
            _log.Information("🏥 MEDICAL: درخواست حذف نرم خدمت. Id: {Id}. User: {UserId}",
                serviceId, _currentUserService?.UserId ?? "NULL");

            try
            {
                // 🔒 اعتبارسنجی ورودی
                if (serviceId <= 0)
                {
                    _log.Warning("🏥 MEDICAL: شناسه خدمت نامعتبر. Id: {Id}. User: {UserId}",
                        serviceId, _currentUserService?.UserId ?? "NULL");
                    return ServiceResult.Failed("شناسه خدمت معتبر نیست.");
                }

                _log.Information("🏥 MEDICAL: بازیابی خدمت از دیتابیس. Id: {Id}. User: {UserId}",
                    serviceId, _currentUserService?.UserId ?? "NULL");

                var service = await _serviceRepo.GetByIdAsync(serviceId);
                if (service == null)
                {
                    _log.Warning("🏥 MEDICAL: خدمت یافت نشد. Id: {Id}. User: {UserId}",
                        serviceId, _currentUserService?.UserId ?? "NULL");
                    return ServiceResult.Failed("خدمت مورد نظر یافت نشد.");
                }

                _log.Information("🏥 MEDICAL: خدمت یافت شد. Id: {Id}, Title: {Title}, IsDeleted: {IsDeleted}. User: {UserId}",
                    serviceId, service.Title, service.IsDeleted, _currentUserService?.UserId ?? "NULL");

                // Check if service is already deleted
                if (service.IsDeleted)
                {
                    _log.Warning("🏥 MEDICAL: خدمت قبلاً حذف شده. Id: {Id}. User: {UserId}",
                        serviceId, _currentUserService?.UserId ?? "NULL");
                    return ServiceResult.Failed("خدمت مورد نظر قبلاً حذف شده است.");
                }

                _log.Information("🏥 MEDICAL: شروع حذف نرم. Id: {Id}. User: {UserId}",
                    serviceId, _currentUserService?.UserId ?? "NULL");

                // 🔒 تعیین شناسه کاربر معتبر برای عملیات حذف
                string validUserId = GetValidUserIdForOperation();

                _log.Information("🏥 MEDICAL: شناسه کاربر معتبر برای عملیات: {ValidUserId}. Original User: {OriginalUserId}",
                    validUserId, _currentUserService?.UserId ?? "NULL");

                // Soft delete
                service.IsDeleted = true;
                service.DeletedAt = DateTime.UtcNow;
                service.DeletedByUserId = validUserId;
                service.UpdatedAt = DateTime.UtcNow;
                service.UpdatedByUserId = validUserId;

                _log.Information("🏥 MEDICAL: به‌روزرسانی خدمت. Id: {Id}, DeletedBy: {DeletedBy}, UpdatedBy: {UpdatedBy}. User: {UserId}",
                    serviceId, service.DeletedByUserId, service.UpdatedByUserId, _currentUserService?.UserId ?? "NULL");

                _serviceRepo.Update(service);
                
                _log.Information("🏥 MEDICAL: ذخیره تغییرات در دیتابیس. Id: {Id}. User: {UserId}",
                    serviceId, _currentUserService?.UserId ?? "NULL");
                
                await _serviceRepo.SaveChangesAsync();
                
                _log.Information("🏥 MEDICAL: خدمت با موفقیت حذف شد. Id: {Id}. User: {UserId}",
                    serviceId, _currentUserService?.UserId ?? "NULL");

                return ServiceResult.Successful("خدمت با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در حذف خدمت. Id: {Id}, ExceptionType: {ExceptionType}, Message: {Message}. User: {UserId}",
                    serviceId, ex.GetType().Name, ex.Message, _currentUserService?.UserId ?? "NULL");
                return ServiceResult.Failed("خطای سیستمی در حذف خدمت رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// بازیابی یک خدمت حذف شده.
        /// </summary>
        public async Task<ServiceResult> RestoreServiceAsync(int serviceId)
        {
            _log.Information("درخواست بازیابی خدمت. Id: {Id}. User: {UserId}",
                serviceId, _currentUserService.UserId);

            try
            {
                if (serviceId <= 0)
                {
                    return ServiceResult.Failed("شناسه خدمت معتبر نیست.");
                }

                var service = await _serviceRepo.GetByIdAsync(serviceId);
                if (service == null)
                {
                    return ServiceResult.Failed("خدمت مورد نظر یافت نشد.");
                }

                if (!service.IsDeleted)
                {
                    return ServiceResult.Failed("خدمت حذف نشده است.");
                }

                // Restore
                service.IsDeleted = false;
                service.DeletedAt = null;
                service.DeletedByUserId = null;
                service.UpdatedAt = DateTime.UtcNow;
                service.UpdatedByUserId = _currentUserService.UserId;

                _serviceRepo.Update(service);
                await _serviceRepo.SaveChangesAsync();
                
                _log.Information("خدمت بازیابی شد. Id: {Id}. User: {UserId}",
                    serviceId, _currentUserService.UserId);

                return ServiceResult.Successful("خدمت با موفقیت بازیابی شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی خدمت. Id: {Id}. User: {UserId}",
                    serviceId, _currentUserService.UserId);
                return ServiceResult.Failed("خطای سیستمی در بازیابی خدمت رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت لیست خدمات فعال یک دسته‌بندی برای لیست‌های کشویی.
        /// </summary>
        public async Task<ServiceResult<List<LookupItemViewModel>>> GetActiveServicesForLookupAsync(int serviceCategoryId)
        {
            _log.Information("درخواست لیست خدمات فعال. CategoryId: {CategoryId}. User: {UserId}",
                serviceCategoryId, _currentUserService.UserId);

            try
            {
                if (serviceCategoryId <= 0)
                {
                    return ServiceResult<List<LookupItemViewModel>>.Failed("شناسه دسته‌بندی معتبر نیست.");
                }

                var services = await _serviceRepo.GetActiveServicesAsync(serviceCategoryId);
                var lookupItems = services.Select(s => new LookupItemViewModel
                {
                    Id = s.ServiceId,
                    Title = s.Title,
                    Description = $"کد: {s.ServiceCode} - قیمت: {s.Price:N0} تومان"
                }).ToList();

                return ServiceResult<List<LookupItemViewModel>>.Successful(lookupItems);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی لیست خدمات فعال. CategoryId: {CategoryId}. User: {UserId}",
                    serviceCategoryId, _currentUserService.UserId);
                return ServiceResult<List<LookupItemViewModel>>.Failed("خطا در بازیابی اطلاعات.", "DB_ERROR");
            }
        }

        /// <summary>
        /// بررسی تکراری بودن کد خدمت - Medical Environment
        /// </summary>
        public async Task<bool> IsServiceCodeDuplicateAsync(string serviceCode, int? serviceCategoryId = null, int? excludeServiceId = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: بررسی تکراری بودن کد خدمت. ServiceCode: {ServiceCode}, CategoryId: {CategoryId}, ExcludeId: {ExcludeId}, User: {UserId}",
                    serviceCode, serviceCategoryId, excludeServiceId, _currentUserService.UserId);

                if (string.IsNullOrWhiteSpace(serviceCode))
                {
                    return false; // کد خالی تکراری نیست
                }

                // اگر serviceCategoryId مشخص شده، فقط در همان دسته‌بندی بررسی کن
                if (serviceCategoryId.HasValue)
                {
                    var isDuplicate = await _serviceRepo.DoesServiceExistAsync(serviceCategoryId.Value, serviceCode.Trim(), excludeServiceId);
                    _log.Information("🏥 MEDICAL: نتیجه بررسی تکراری در دسته‌بندی. ServiceCode: {ServiceCode}, CategoryId: {CategoryId}, IsDuplicate: {IsDuplicate}",
                        serviceCode, serviceCategoryId, isDuplicate);
                    return isDuplicate;
                }
                else
                {
                    // بررسی در تمام دسته‌بندی‌ها
                    var isDuplicate = await _serviceRepo.DoesServiceCodeExistGloballyAsync(serviceCode.Trim(), excludeServiceId);
                    _log.Information("🏥 MEDICAL: نتیجه بررسی تکراری جهانی. ServiceCode: {ServiceCode}, IsDuplicate: {IsDuplicate}",
                        serviceCode, isDuplicate);
                    return isDuplicate;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در بررسی تکراری بودن کد خدمت. ServiceCode: {ServiceCode}, User: {UserId}",
                    serviceCode, _currentUserService.UserId);
                return false; // در صورت خطا، اجازه ادامه فرآیند را می‌دهیم
            }
        }

        #endregion

        /// <summary>
        /// 🔒 تعیین شناسه کاربر معتبر برای عملیات‌های دیتابیس
        /// این متد اطمینان حاصل می‌کند که همیشه یک شناسه کاربر معتبر برای عملیات‌های حذف نرم استفاده شود
        /// </summary>
        private string GetValidUserIdForOperation()
        {
            try
            {
                // 1. ابتدا سعی می‌کنیم از کاربر جاری استفاده کنیم
                if (!string.IsNullOrWhiteSpace(_currentUserService?.UserId))
                {
                    _log.Debug("🏥 MEDICAL: استفاده از شناسه کاربر جاری: {UserId}", _currentUserService.UserId);
                    return _currentUserService.UserId;
                }

                // 2. اگر کاربر جاری موجود نبود، از کاربر سیستم استفاده می‌کنیم
                if (!string.IsNullOrWhiteSpace(SystemUsers.SystemUserId))
                {
                    _log.Debug("🏥 MEDICAL: استفاده از شناسه کاربر سیستم: {SystemUserId}", SystemUsers.SystemUserId);
                    return SystemUsers.SystemUserId;
                }

                // 3. اگر کاربر سیستم هم موجود نبود، از کاربر ادمین استفاده می‌کنیم
                if (!string.IsNullOrWhiteSpace(SystemUsers.AdminUserId))
                {
                    _log.Debug("🏥 MEDICAL: استفاده از شناسه کاربر ادمین: {AdminUserId}", SystemUsers.AdminUserId);
                    return SystemUsers.AdminUserId;
                }

                // 4. در نهایت، اگر هیچ کاربر سیستمی موجود نبود، یک شناسه پیش‌فرض استفاده می‌کنیم
                // این حالت فقط در محیط‌های توسعه یا تست رخ می‌دهد
                _log.Warning("🏥 MEDICAL: هیچ کاربر سیستمی یافت نشد. استفاده از شناسه پیش‌فرض.");
                return "00000000-0000-0000-0000-000000000000"; // شناسه پیش‌فرض
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در تعیین شناسه کاربر معتبر. استفاده از شناسه پیش‌فرض.");
                return "00000000-0000-0000-0000-000000000000"; // شناسه پیش‌فرض
            }
        }
    }
}