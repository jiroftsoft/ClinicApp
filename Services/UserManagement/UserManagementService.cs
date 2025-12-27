using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.UserManagement;
using ClinicApp.Models;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.Shared;
using ClinicApp.ViewModels.UserManagement;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Serilog;

namespace ClinicApp.Services.UserManagement
{
    /// <summary>
    /// Service پیاده‌سازی برای مدیریت کاربران سیستم
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// Architecture Principles Applied:
    /// ✅ Single Responsibility: Owns all user management business logic
    /// ✅ Dependency Inversion: Depends on repository abstractions
    /// ✅ Clean Architecture: Service layer orchestrates domain operations
    /// ✅ Medical Standards: Implements healthcare industry best practices
    /// ✅ Persian Support: Full localization for Iranian medical environments
    /// ✅ Security: Complete audit trail and validation
    /// 
    /// Flow: Controller -> UserManagementService -> UserRepository -> Database
    /// </summary>
    public class UserManagementService : IUserManagementService
    {
        #region Fields and Constructor

        private readonly IUserRepository _userRepository;
        private readonly ApplicationUserManager _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public UserManagementService(
            IUserRepository userRepository,
            ApplicationUserManager userManager,
            RoleManager<IdentityRole> roleManager,
            ICurrentUserService currentUserService,
            ILogger logger,
            ApplicationDbContext context)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<UserManagementService>() ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// دریافت لیست کاربران با فیلتر و Pagination
        /// </summary>
        public async Task<ServiceResult<UserIndexViewModel>> GetUsersAsync(
            UserSearchFilter filter,
            int pageNumber,
            int pageSize)
        {
            try
            {
                _logger.Information("درخواست لیست کاربران - SearchTerm: {SearchTerm}, IsActive: {IsActive}, Role: {Role}, Page: {Page}, Size: {Size}. User: {UserId}",
                    filter?.SearchTerm, filter?.IsActive, filter?.RoleName, pageNumber, pageSize, _currentUserService.UserId);

                // ✅ Validation
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100; // حداکثر 100 آیتم در هر صفحه

                // ✅ دریافت داده از Repository
                var pagedResult = await _userRepository.SearchAsync(
                    filter?.SearchTerm,
                    filter?.IsActive,
                    filter?.RoleName,
                    pageNumber,
                    pageSize);

                // ✅ تبدیل به ViewModel
                var users = pagedResult.Items.Select(u => 
                {
                    var userRoles = _userManager.GetRoles(u.Id).ToList();
                    return new UserListItemViewModel
                    {
                        UserId = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = u.FullName,
                        NationalCode = u.NationalCode,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Roles = userRoles.Select(r => RoleHelper.GetPersianName(r)).ToList(), // ✅ تبدیل به فارسی
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        CreatedAtShamsi = u.CreatedAt.ToPersianDateTime(),
                        LastLoginDate = u.LastLoginDate,
                        LastLoginDateShamsi = u.LastLoginDate.HasValue ? u.LastLoginDate.Value.ToPersianDateTime() : null,
                        DeletedAt = u.DeletedAt,
                        DeletedAtShamsi = u.DeletedAt.HasValue ? u.DeletedAt.Value.ToPersianDateTime() : null
                    };
                }).ToList();

                // ✅ دریافت آمار
                var statistics = await GetStatisticsAsync();

                // ✅ ساخت ViewModel
                var viewModel = new UserIndexViewModel
                {
                    Users = users,
                    Filter = filter ?? new UserSearchFilter(),
                    PagingInfo = new PaginationViewModel
                    {
                        CurrentPage = pageNumber,
                        TotalPages = pagedResult.TotalPages,
                        TotalCount = pagedResult.TotalItems,
                        PageSize = pageSize,
                        ActionName = "Index",
                        ControllerName = "UserManagement"
                    },
                    Statistics = statistics.Data
                };

                // ✅ دریافت نقش‌های موجود برای فیلتر (با نام فارسی)
                var allRoles = await _roleManager.Roles.ToListAsync();
                viewModel.Filter.AvailableRoles = allRoles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = RoleHelper.GetPersianName(r.Name), // ✅ نمایش نام فارسی
                    Selected = r.Name == filter?.RoleName
                }).ToList();

                _logger.Information("لیست کاربران بازیابی شد - Total: {Total}, Page: {Page}. User: {UserId}",
                    pagedResult.TotalItems, pageNumber, _currentUserService.UserId);

                return ServiceResult<UserIndexViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بازیابی لیست کاربران. User: {UserId}", _currentUserService.UserId);
                return ServiceResult<UserIndexViewModel>.Failed("خطا در بازیابی اطلاعات کاربران.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت جزئیات کاربر
        /// </summary>
        public async Task<ServiceResult<UserDetailsViewModel>> GetUserDetailsAsync(string userId)
        {
            try
            {
                _logger.Information("درخواست جزئیات کاربر - UserId: {UserId}. RequestedBy: {RequestedBy}",
                    userId, _currentUserService.UserId);

                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult<UserDetailsViewModel>.Failed("شناسه کاربر معتبر نیست.");
                }

                // ✅ دریافت کاربر
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("کاربر یافت نشد - UserId: {UserId}. RequestedBy: {RequestedBy}",
                        userId, _currentUserService.UserId);
                    return ServiceResult<UserDetailsViewModel>.Failed("کاربر مورد نظر یافت نشد.");
                }

                // ✅ دریافت نقش‌ها (با نام فارسی)
                var userRoles = await _userRepository.GetUserRolesAsync(userId);
                var roles = userRoles.Select(r => new RoleViewModel
                {
                    RoleName = r,
                    DisplayName = RoleHelper.GetPersianName(r) // ✅ نمایش نام فارسی
                }).ToList();

                // ✅ ساخت ViewModel
                var viewModel = new UserDetailsViewModel
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    NationalCode = user.NationalCode,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Gender = user.Gender,
                    Address = user.Address,
                    IsActive = user.IsActive,
                    IsDeleted = user.IsDeleted,
                    Roles = roles,
                    CreatedAt = user.CreatedAt,
                    CreatedAtShamsi = user.CreatedAt.ToPersianDateTime(),
                    CreatedByUser = user.CreatedByUser != null ? user.CreatedByUser.FullName : "ناشناس",
                    UpdatedAt = user.UpdatedAt,
                    UpdatedAtShamsi = user.UpdatedAt.HasValue ? user.UpdatedAt.Value.ToPersianDateTime() : null,
                    UpdatedByUser = user.UpdatedByUser != null ? user.UpdatedByUser.FullName : null,
                    DeletedAt = user.DeletedAt,
                    DeletedAtShamsi = user.DeletedAt.HasValue ? user.DeletedAt.Value.ToPersianDateTime() : null,
                    DeletedByUser = user.DeletedByUser != null ? user.DeletedByUser.FullName : null,
                    LastLoginDate = user.LastLoginDate,
                    LastLoginDateShamsi = user.LastLoginDate.HasValue ? user.LastLoginDate.Value.ToPersianDateTime() : null
                };

                _logger.Information("جزئیات کاربر بازیابی شد - UserId: {UserId}. RequestedBy: {RequestedBy}",
                    userId, _currentUserService.UserId);

                return ServiceResult<UserDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بازیابی جزئیات کاربر - UserId: {UserId}. RequestedBy: {RequestedBy}",
                    userId, _currentUserService.UserId);
                return ServiceResult<UserDetailsViewModel>.Failed("خطا در بازیابی اطلاعات کاربر.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت اطلاعات کاربر برای ویرایش
        /// </summary>
        public async Task<ServiceResult<UserCreateEditViewModel>> GetUserForEditAsync(string userId)
        {
            try
            {
                _logger.Information("درخواست ویرایش کاربر - UserId: {UserId}. RequestedBy: {RequestedBy}",
                    userId, _currentUserService.UserId);

                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult<UserCreateEditViewModel>.Failed("شناسه کاربر معتبر نیست.");
                }

                // ✅ دریافت کاربر
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<UserCreateEditViewModel>.Failed("کاربر مورد نظر یافت نشد.");
                }

                // ✅ دریافت نقش‌های کاربر
                var userRoles = await _userRepository.GetUserRolesAsync(userId);

                // ✅ دریافت نقش‌های موجود (با نام فارسی)
                var allRoles = await _roleManager.Roles.ToListAsync();
                var availableRoles = allRoles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = RoleHelper.GetPersianName(r.Name), // ✅ نمایش نام فارسی
                    Selected = userRoles.Contains(r.Name)
                }).ToList();

                // ✅ ساخت ViewModel
                var viewModel = new UserCreateEditViewModel
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    NationalCode = user.NationalCode,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Gender = user.Gender,
                    Address = user.Address,
                    IsActive = user.IsActive,
                    SelectedRoles = userRoles,
                    AvailableRoles = availableRoles
                };

                _logger.Information("اطلاعات ویرایش کاربر بازیابی شد - UserId: {UserId}. RequestedBy: {RequestedBy}",
                    userId, _currentUserService.UserId);

                return ServiceResult<UserCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بازیابی اطلاعات ویرایش کاربر - UserId: {UserId}. RequestedBy: {RequestedBy}",
                    userId, _currentUserService.UserId);
                return ServiceResult<UserCreateEditViewModel>.Failed("خطا در بازیابی اطلاعات کاربر.", "DB_ERROR");
            }
        }

        /// <summary>
        /// ایجاد کاربر جدید
        /// </summary>
        public async Task<ServiceResult<ApplicationUser>> CreateUserAsync(UserCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد کاربر - NationalCode: {NationalCode}, Email: {Email}. User: {UserId}",
                    model?.NationalCode, model?.Email, _currentUserService.UserId);

                // ✅ Validation
                var validationResult = await ValidateUserModelAsync(model, isEdit: false);
                if (!validationResult.Success)
                {
                    return ServiceResult<ApplicationUser>.Failed(validationResult.Message, validationResult.Code);
                }

                // ✅ بررسی تکراری بودن کد ملی
                var existsByNationalCode = await _userRepository.ExistsByNationalCodeAsync(model.NationalCode);
                if (existsByNationalCode)
                {
                    _logger.Warning("کد ملی تکراری - NationalCode: {NationalCode}. User: {UserId}",
                        model.NationalCode, _currentUserService.UserId);
                    return ServiceResult<ApplicationUser>.Failed("کاربری با این کد ملی قبلاً ثبت شده است.", "DUPLICATE_NATIONAL_CODE");
                }

                // ✅ بررسی تکراری بودن ایمیل
                var existsByEmail = await _userRepository.ExistsByEmailAsync(model.Email);
                if (existsByEmail)
                {
                    _logger.Warning("ایمیل تکراری - Email: {Email}. User: {UserId}",
                        model.Email, _currentUserService.UserId);
                    return ServiceResult<ApplicationUser>.Failed("کاربری با این ایمیل قبلاً ثبت شده است.", "DUPLICATE_EMAIL");
                }

                // ✅ ایجاد Entity
                var user = new ApplicationUser
                {
                    UserName = model.NationalCode, // در سیستم پسورد‌لس، UserName = NationalCode
                    NationalCode = model.NationalCode.Trim(),
                    Email = model.Email.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    Gender = model.Gender,
                    Address = model.Address?.Trim(),
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId,
                    IsDeleted = false
                };

                // ✅ ایجاد کاربر در Identity (بدون پسورد - سیستم پسورد‌لس)
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var identityResult = await _userManager.CreateAsync(user);
                        if (!identityResult.Succeeded)
                        {
                            transaction.Rollback();
                            var errors = string.Join(", ", identityResult.Errors.Select(e => e));
                            _logger.Error("خطا در ایجاد کاربر Identity - NationalCode: {NationalCode}, Errors: {Errors}. User: {UserId}",
                                model.NationalCode, errors, _currentUserService.UserId);
                            return ServiceResult<ApplicationUser>.Failed($"خطا در ایجاد کاربر: {errors}", "IDENTITY_ERROR");
                        }

                        // ✅ اختصاص نقش‌ها
                        if (model.SelectedRoles != null && model.SelectedRoles.Any())
                        {
                            foreach (var roleName in model.SelectedRoles)
                            {
                                if (!await _roleManager.RoleExistsAsync(roleName))
                                {
                                    _logger.Warning("نقش وجود ندارد - Role: {Role}. User: {UserId}",
                                        roleName, _currentUserService.UserId);
                                    continue;
                                }

                                var roleResult = await _userManager.AddToRoleAsync(user.Id, roleName);
                                if (!roleResult.Succeeded)
                                {
                                    _logger.Warning("خطا در اختصاص نقش - Role: {Role}, Errors: {Errors}. User: {UserId}",
                                        roleName, string.Join(", ", roleResult.Errors), _currentUserService.UserId);
                                }
                            }
                        }

                        transaction.Commit();

                        _logger.Information("کاربر جدید با موفقیت ایجاد شد - UserId: {UserId}, NationalCode: {NationalCode}. CreatedBy: {CreatedBy}",
                            user.Id, user.NationalCode, _currentUserService.UserId);

                        return ServiceResult<ApplicationUser>.Successful(user, "کاربر با موفقیت ایجاد شد.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد کاربر - NationalCode: {NationalCode}. User: {UserId}",
                    model?.NationalCode, _currentUserService.UserId);
                return ServiceResult<ApplicationUser>.Failed("خطای سیستمی در ایجاد کاربر رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// به‌روزرسانی کاربر
        /// </summary>
        public async Task<ServiceResult<ApplicationUser>> UpdateUserAsync(UserCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی کاربر - UserId: {UserId}. UpdatedBy: {UpdatedBy}",
                    model?.UserId, _currentUserService.UserId);

                if (string.IsNullOrEmpty(model.UserId))
                {
                    return ServiceResult<ApplicationUser>.Failed("شناسه کاربر معتبر نیست.");
                }

                // ✅ Validation
                var validationResult = await ValidateUserModelAsync(model, isEdit: true);
                if (!validationResult.Success)
                {
                    return ServiceResult<ApplicationUser>.Failed(validationResult.Message, validationResult.Code);
                }

                // ✅ دریافت کاربر موجود
                var user = await _userRepository.GetByIdAsync(model.UserId);
                if (user == null)
                {
                    return ServiceResult<ApplicationUser>.Failed("کاربر مورد نظر یافت نشد.");
                }

                // ✅ بررسی تکراری بودن کد ملی (به جز خود کاربر)
                var existsByNationalCode = await _userRepository.ExistsByNationalCodeAsync(model.NationalCode, model.UserId);
                if (existsByNationalCode)
                {
                    _logger.Warning("کد ملی تکراری - NationalCode: {NationalCode}, ExcludeUserId: {UserId}. User: {UserId}",
                        model.NationalCode, model.UserId, _currentUserService.UserId);
                    return ServiceResult<ApplicationUser>.Failed("کاربری با این کد ملی قبلاً ثبت شده است.", "DUPLICATE_NATIONAL_CODE");
                }

                // ✅ بررسی تکراری بودن ایمیل (به جز خود کاربر)
                var existsByEmail = await _userRepository.ExistsByEmailAsync(model.Email, model.UserId);
                if (existsByEmail)
                {
                    _logger.Warning("ایمیل تکراری - Email: {Email}, ExcludeUserId: {UserId}. User: {UserId}",
                        model.Email, model.UserId, _currentUserService.UserId);
                    return ServiceResult<ApplicationUser>.Failed("کاربری با این ایمیل قبلاً ثبت شده است.", "DUPLICATE_EMAIL");
                }

                // ✅ به‌روزرسانی فیلدها
                user.FirstName = model.FirstName.Trim();
                user.LastName = model.LastName.Trim();
                user.NationalCode = model.NationalCode.Trim();
                user.UserName = model.NationalCode; // در سیستم پسورد‌لس، UserName = NationalCode
                user.Email = model.Email.Trim();
                user.PhoneNumber = model.PhoneNumber.Trim();
                user.Gender = model.Gender;
                user.Address = model.Address?.Trim();
                user.IsActive = model.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedByUserId = _currentUserService.UserId;

                // ✅ به‌روزرسانی نقش‌ها
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // ✅ دریافت نقش‌های فعلی
                        var currentRoles = await _userRepository.GetUserRolesAsync(user.Id);

                        // ✅ حذف نقش‌های حذف شده
                        var rolesToRemove = currentRoles.Except(model.SelectedRoles ?? new List<string>()).ToList();
                        foreach (var roleName in rolesToRemove)
                        {
                            var removeResult = await _userManager.RemoveFromRoleAsync(user.Id, roleName);
                            if (!removeResult.Succeeded)
                            {
                                _logger.Warning("خطا در حذف نقش - Role: {Role}, Errors: {Errors}. User: {UserId}",
                                    roleName, string.Join(", ", removeResult.Errors), _currentUserService.UserId);
                            }
                        }

                        // ✅ اضافه کردن نقش‌های جدید
                        var rolesToAdd = (model.SelectedRoles ?? new List<string>()).Except(currentRoles).ToList();
                        foreach (var roleName in rolesToAdd)
                        {
                            if (!await _roleManager.RoleExistsAsync(roleName))
                            {
                                _logger.Warning("نقش وجود ندارد - Role: {Role}. User: {UserId}",
                                    roleName, _currentUserService.UserId);
                                continue;
                            }

                            var addResult = await _userManager.AddToRoleAsync(user.Id, roleName);
                            if (!addResult.Succeeded)
                            {
                                _logger.Warning("خطا در اختصاص نقش - Role: {Role}, Errors: {Errors}. User: {UserId}",
                                    roleName, string.Join(", ", addResult.Errors), _currentUserService.UserId);
                            }
                        }

                        // ✅ ذخیره تغییرات
                        await _userRepository.UpdateAsync(user);
                        await _context.SaveChangesAsync();

                        transaction.Commit();

                        _logger.Information("کاربر با موفقیت به‌روزرسانی شد - UserId: {UserId}. UpdatedBy: {UpdatedBy}",
                            user.Id, _currentUserService.UserId);

                        return ServiceResult<ApplicationUser>.Successful(user, "کاربر با موفقیت به‌روزرسانی شد.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی کاربر - UserId: {UserId}. UpdatedBy: {UpdatedBy}",
                    model?.UserId, _currentUserService.UserId);
                return ServiceResult<ApplicationUser>.Failed("خطای سیستمی در به‌روزرسانی کاربر رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// حذف نرم کاربر
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteUserAsync(string userId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست حذف نرم کاربر - UserId: {UserId}. DeletedBy: {DeletedBy}",
                    userId, _currentUserService.UserId);

                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult<bool>.Failed("شناسه کاربر معتبر نیست.");
                }

                // ✅ دریافت کاربر
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<bool>.Failed("کاربر مورد نظر یافت نشد.");
                }

                // ✅ بررسی حذف شده بودن
                if (user.IsDeleted)
                {
                    _logger.Warning("🏥 MEDICAL: کاربر قبلاً حذف شده - UserId: {UserId}. DeletedBy: {DeletedBy}",
                        userId, _currentUserService.UserId);
                    return ServiceResult<bool>.Failed("کاربر مورد نظر قبلاً حذف شده است.");
                }

                // ✅ Soft Delete
                var result = await _userRepository.SoftDeleteAsync(userId, _currentUserService.UserId);
                if (!result)
                {
                    return ServiceResult<bool>.Failed("خطا در حذف کاربر.");
                }

                _logger.Information("🏥 MEDICAL: کاربر با موفقیت حذف شد (Soft Delete) - UserId: {UserId}. DeletedBy: {DeletedBy}",
                    userId, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(true, "کاربر با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در حذف نرم کاربر - UserId: {UserId}. DeletedBy: {DeletedBy}",
                    userId, _currentUserService.UserId);
                return ServiceResult<bool>.Failed("خطای سیستمی در حذف کاربر رخ داد.", "DB_ERROR");
            }
        }

        #endregion

        #region Role Management

        /// <summary>
        /// اختصاص نقش به کاربر
        /// </summary>
        public async Task<ServiceResult<bool>> AssignRoleAsync(string userId, string roleName)
        {
            try
            {
                _logger.Information("درخواست اختصاص نقش - UserId: {UserId}, Role: {Role}. AssignedBy: {AssignedBy}",
                    userId, roleName, _currentUserService.UserId);

                if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(roleName))
                {
                    return ServiceResult<bool>.Failed("شناسه کاربر یا نام نقش معتبر نیست.");
                }

                // ✅ بررسی وجود نقش
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    return ServiceResult<bool>.Failed("نقش مورد نظر وجود ندارد.");
                }

                // ✅ بررسی وجود کاربر
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<bool>.Failed("کاربر مورد نظر یافت نشد.");
                }

                // ✅ بررسی اینکه آیا کاربر قبلاً این نقش را دارد
                if (await _userRepository.IsInRoleAsync(userId, roleName))
                {
                    _logger.Warning("کاربر قبلاً این نقش را دارد - UserId: {UserId}, Role: {Role}",
                        userId, roleName);
                    return ServiceResult<bool>.Failed("کاربر قبلاً این نقش را دارد.");
                }

                // ✅ اختصاص نقش
                var result = await _userManager.AddToRoleAsync(userId, roleName);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e));
                    _logger.Error("خطا در اختصاص نقش - UserId: {UserId}, Role: {Role}, Errors: {Errors}",
                        userId, roleName, errors);
                    return ServiceResult<bool>.Failed($"خطا در اختصاص نقش: {errors}", "ROLE_ASSIGN_ERROR");
                }

                _logger.Information("نقش با موفقیت اختصاص داده شد - UserId: {UserId}, Role: {Role}. AssignedBy: {AssignedBy}",
                    userId, roleName, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(true, "نقش با موفقیت اختصاص داده شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اختصاص نقش - UserId: {UserId}, Role: {Role}",
                    userId, roleName);
                return ServiceResult<bool>.Failed("خطای سیستمی در اختصاص نقش رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// حذف نقش از کاربر
        /// </summary>
        public async Task<ServiceResult<bool>> RemoveRoleAsync(string userId, string roleName)
        {
            try
            {
                _logger.Information("درخواست حذف نقش - UserId: {UserId}, Role: {Role}. RemovedBy: {RemovedBy}",
                    userId, roleName, _currentUserService.UserId);

                if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(roleName))
                {
                    return ServiceResult<bool>.Failed("شناسه کاربر یا نام نقش معتبر نیست.");
                }

                // ✅ بررسی وجود کاربر
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<bool>.Failed("کاربر مورد نظر یافت نشد.");
                }

                // ✅ بررسی اینکه آیا کاربر این نقش را دارد
                if (!await _userRepository.IsInRoleAsync(userId, roleName))
                {
                    _logger.Warning("کاربر این نقش را ندارد - UserId: {UserId}, Role: {Role}",
                        userId, roleName);
                    return ServiceResult<bool>.Failed("کاربر این نقش را ندارد.");
                }

                // ✅ حذف نقش
                var result = await _userManager.RemoveFromRoleAsync(userId, roleName);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e));
                    _logger.Error("خطا در حذف نقش - UserId: {UserId}, Role: {Role}, Errors: {Errors}",
                        userId, roleName, errors);
                    return ServiceResult<bool>.Failed($"خطا در حذف نقش: {errors}", "ROLE_REMOVE_ERROR");
                }

                _logger.Information("نقش با موفقیت حذف شد - UserId: {UserId}, Role: {Role}. RemovedBy: {RemovedBy}",
                    userId, roleName, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(true, "نقش با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نقش - UserId: {UserId}, Role: {Role}",
                    userId, roleName);
                return ServiceResult<bool>.Failed("خطای سیستمی در حذف نقش رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت لیست نقش‌های موجود
        /// </summary>
        public async Task<ServiceResult<List<RoleViewModel>>> GetAvailableRolesAsync()
        {
            try
            {
                var roles = await _roleManager.Roles.ToListAsync();
                var viewModels = roles.Select(r => new RoleViewModel
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    DisplayName = RoleHelper.GetPersianName(r.Name) // ✅ نمایش نام فارسی
                }).ToList();

                return ServiceResult<List<RoleViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست نقش‌ها");
                return ServiceResult<List<RoleViewModel>>.Failed("خطا در دریافت لیست نقش‌ها.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت نقش‌های کاربر
        /// </summary>
        public async Task<ServiceResult<List<RoleViewModel>>> GetUserRolesAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult<List<RoleViewModel>>.Failed("شناسه کاربر معتبر نیست.");
                }

                var userRoles = await _userRepository.GetUserRolesAsync(userId);
                var viewModels = userRoles.Select(r => new RoleViewModel
                {
                    RoleName = r,
                    DisplayName = RoleHelper.GetPersianName(r) // ✅ نمایش نام فارسی
                }).ToList();

                return ServiceResult<List<RoleViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نقش‌های کاربر - UserId: {UserId}", userId);
                return ServiceResult<List<RoleViewModel>>.Failed("خطا در دریافت نقش‌های کاربر.", "DB_ERROR");
            }
        }

        #endregion

        #region Activation/Deactivation

        /// <summary>
        /// فعال‌سازی کاربر
        /// </summary>
        public async Task<ServiceResult<bool>> ActivateUserAsync(string userId)
        {
            try
            {
                _logger.Information("درخواست فعال‌سازی کاربر - UserId: {UserId}. ActivatedBy: {ActivatedBy}",
                    userId, _currentUserService.UserId);

                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult<bool>.Failed("شناسه کاربر معتبر نیست.");
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<bool>.Failed("کاربر مورد نظر یافت نشد.");
                }

                if (user.IsActive)
                {
                    _logger.Warning("کاربر قبلاً فعال است - UserId: {UserId}", userId);
                    return ServiceResult<bool>.Failed("کاربر قبلاً فعال است.");
                }

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedByUserId = _currentUserService.UserId;

                await _userRepository.UpdateAsync(user);
                await _context.SaveChangesAsync();

                _logger.Information("کاربر با موفقیت فعال شد - UserId: {UserId}. ActivatedBy: {ActivatedBy}",
                    userId, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(true, "کاربر با موفقیت فعال شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی کاربر - UserId: {UserId}", userId);
                return ServiceResult<bool>.Failed("خطای سیستمی در فعال‌سازی کاربر رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// غیرفعال‌سازی کاربر
        /// </summary>
        public async Task<ServiceResult<bool>> DeactivateUserAsync(string userId)
        {
            try
            {
                _logger.Information("درخواست غیرفعال‌سازی کاربر - UserId: {UserId}. DeactivatedBy: {DeactivatedBy}",
                    userId, _currentUserService.UserId);

                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult<bool>.Failed("شناسه کاربر معتبر نیست.");
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<bool>.Failed("کاربر مورد نظر یافت نشد.");
                }

                if (!user.IsActive)
                {
                    _logger.Warning("کاربر قبلاً غیرفعال است - UserId: {UserId}", userId);
                    return ServiceResult<bool>.Failed("کاربر قبلاً غیرفعال است.");
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedByUserId = _currentUserService.UserId;

                await _userRepository.UpdateAsync(user);
                await _context.SaveChangesAsync();

                _logger.Information("کاربر با موفقیت غیرفعال شد - UserId: {UserId}. DeactivatedBy: {DeactivatedBy}",
                    userId, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(true, "کاربر با موفقیت غیرفعال شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی کاربر - UserId: {UserId}", userId);
                return ServiceResult<bool>.Failed("خطای سیستمی در غیرفعال‌سازی کاربر رخ داد.", "DB_ERROR");
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// بررسی معتبر بودن کد ملی
        /// </summary>
        public async Task<ServiceResult<bool>> ValidateNationalCodeAsync(string nationalCode, string excludeUserId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nationalCode))
                {
                    return ServiceResult<bool>.Failed("کد ملی نمی‌تواند خالی باشد.");
                }

                if (nationalCode.Length != 10)
                {
                    return ServiceResult<bool>.Failed("کد ملی باید 10 رقم باشد.");
                }

                var exists = await _userRepository.ExistsByNationalCodeAsync(nationalCode, excludeUserId);
                if (exists)
                {
                    return ServiceResult<bool>.Failed("کاربری با این کد ملی قبلاً ثبت شده است.", "DUPLICATE_NATIONAL_CODE");
                }

                return ServiceResult<bool>.Successful(true, "کد ملی معتبر است.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی کد ملی - NationalCode: {NationalCode}", nationalCode);
                return ServiceResult<bool>.Failed("خطا در بررسی کد ملی.", "VALIDATION_ERROR");
            }
        }

        /// <summary>
        /// بررسی معتبر بودن ایمیل
        /// </summary>
        public async Task<ServiceResult<bool>> ValidateEmailAsync(string email, string excludeUserId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return ServiceResult<bool>.Failed("ایمیل نمی‌تواند خالی باشد.");
                }

                if (!email.Contains("@"))
                {
                    return ServiceResult<bool>.Failed("فرمت ایمیل معتبر نیست.");
                }

                var exists = await _userRepository.ExistsByEmailAsync(email, excludeUserId);
                if (exists)
                {
                    return ServiceResult<bool>.Failed("کاربری با این ایمیل قبلاً ثبت شده است.", "DUPLICATE_EMAIL");
                }

                return ServiceResult<bool>.Successful(true, "ایمیل معتبر است.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی ایمیل - Email: {Email}", email);
                return ServiceResult<bool>.Failed("خطا در بررسی ایمیل.", "VALIDATION_ERROR");
            }
        }

        /// <summary>
        /// Validation کامل ViewModel
        /// </summary>
        private async Task<ServiceResult> ValidateUserModelAsync(UserCreateEditViewModel model, bool isEdit)
        {
            if (model == null)
            {
                return ServiceResult.Failed("اطلاعات کاربر نمی‌تواند خالی باشد.");
            }

            // ✅ Validation فیلدهای الزامی
            if (string.IsNullOrWhiteSpace(model.FirstName))
            {
                return ServiceResult.Failed("نام الزامی است.", "VALIDATION_ERROR");
            }

            if (string.IsNullOrWhiteSpace(model.LastName))
            {
                return ServiceResult.Failed("نام خانوادگی الزامی است.", "VALIDATION_ERROR");
            }

            if (string.IsNullOrWhiteSpace(model.NationalCode))
            {
                return ServiceResult.Failed("کد ملی الزامی است.", "VALIDATION_ERROR");
            }

            if (model.NationalCode.Length != 10)
            {
                return ServiceResult.Failed("کد ملی باید 10 رقم باشد.", "VALIDATION_ERROR");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return ServiceResult.Failed("ایمیل الزامی است.", "VALIDATION_ERROR");
            }

            if (!model.Email.Contains("@"))
            {
                return ServiceResult.Failed("فرمت ایمیل معتبر نیست.", "VALIDATION_ERROR");
            }

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                return ServiceResult.Failed("شماره تلفن الزامی است.", "VALIDATION_ERROR");
            }

            return ServiceResult.Successful();
        }

        #endregion

        #region Statistics

        /// <summary>
        /// دریافت آمار کاربران
        /// </summary>
        public async Task<ServiceResult<UserStatisticsViewModel>> GetStatisticsAsync()
        {
            try
            {
                var totalUsers = await _userRepository.GetTotalUsersCountAsync();
                var activeUsers = await _userRepository.GetActiveUsersCountAsync();
                var usersByRole = await _userRepository.GetUsersCountByRoleAsync();

                // ✅ دریافت تعداد کاربران حذف شده (مستقیم از دیتابیس)
                var deletedUsersCount = await _userRepository.GetDeletedUsersCountAsync();
                
                _logger.Debug("آمار کاربران - Total: {Total}, Active: {Active}, Deleted: {Deleted}", 
                    totalUsers, activeUsers, deletedUsersCount);

                var statistics = new UserStatisticsViewModel
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = totalUsers - activeUsers,
                    DeletedUsers = deletedUsersCount,
                    UsersByRole = usersByRole
                };

                return ServiceResult<UserStatisticsViewModel>.Successful(statistics);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار کاربران");
                return ServiceResult<UserStatisticsViewModel>.Failed("خطا در دریافت آمار کاربران.", "DB_ERROR");
            }
        }

        #endregion

        #region Deleted Users Management

        /// <summary>
        /// دریافت لیست کاربران حذف شده با فیلتر و Pagination
        /// </summary>
        public async Task<ServiceResult<UserIndexViewModel>> GetDeletedUsersAsync(
            UserSearchFilter filter,
            int pageNumber,
            int pageSize)
        {
            try
            {
                _logger.Information("درخواست لیست کاربران حذف شده - SearchTerm: {SearchTerm}, Page: {Page}, Size: {Size}. User: {UserId}",
                    filter?.SearchTerm, pageNumber, pageSize, _currentUserService.UserId);

                // ✅ Validation
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                // ✅ دریافت کاربران حذف شده از Repository
                var deletedUsers = await _userRepository.GetDeletedUsersAsync();
                
                // ✅ اعمال فیلتر Search Term
                if (!string.IsNullOrWhiteSpace(filter?.SearchTerm))
                {
                    var term = filter.SearchTerm.Trim();
                    deletedUsers = deletedUsers.Where(u =>
                        u.FirstName.Contains(term) ||
                        u.LastName.Contains(term) ||
                        u.NationalCode.Contains(term) ||
                        u.Email.Contains(term) ||
                        u.PhoneNumber.Contains(term) ||
                        (u.FirstName + " " + u.LastName).Contains(term)).ToList();
                }

                // ✅ اعمال فیلتر Role
                if (!string.IsNullOrWhiteSpace(filter?.RoleName))
                {
                    var usersInRole = await _userManager.Users
                        .Where(u => u.Roles.Any(r => r.RoleId == _roleManager.Roles
                            .FirstOrDefault(role => role.Name == filter.RoleName).Id))
                        .Select(u => u.Id)
                        .ToListAsync();
                    
                    deletedUsers = deletedUsers.Where(u => usersInRole.Contains(u.Id)).ToList();
                }

                // ✅ Pagination
                var totalCount = deletedUsers.Count;
                var pagedUsers = deletedUsers
                    .OrderByDescending(u => u.DeletedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // ✅ تبدیل به ViewModel
                var users = pagedUsers.Select(u =>
                {
                    // ✅ دریافت نقش‌ها از Repository (برای کاربران حذف شده)
                    var userRoles = _userRepository.GetUserRolesAsync(u.Id).Result ?? new List<string>();
                    return new UserListItemViewModel
                    {
                        UserId = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = u.FullName,
                        NationalCode = u.NationalCode,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Roles = userRoles.Select(r => RoleHelper.GetPersianName(r)).ToList(),
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        CreatedAtShamsi = u.CreatedAt.ToPersianDateTime(),
                        LastLoginDate = u.LastLoginDate,
                        LastLoginDateShamsi = u.LastLoginDate.HasValue ? u.LastLoginDate.Value.ToPersianDateTime() : null,
                        DeletedAt = u.DeletedAt,
                        DeletedAtShamsi = u.DeletedAt.HasValue ? u.DeletedAt.Value.ToPersianDateTime() : null
                    };
                }).ToList();

                // ✅ ساخت ViewModel
                var viewModel = new UserIndexViewModel
                {
                    Users = users,
                    Filter = filter ?? new UserSearchFilter(),
                    PagingInfo = new PaginationViewModel
                    {
                        CurrentPage = pageNumber,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        TotalCount = totalCount,
                        PageSize = pageSize,
                        ActionName = "DeletedUsers",
                        ControllerName = "UserManagement"
                    },
                    Statistics = null // آمار برای کاربران حذف شده نمایش داده نمی‌شود
                };

                // ✅ دریافت نقش‌های موجود برای فیلتر
                var allRoles = await _roleManager.Roles.ToListAsync();
                viewModel.Filter.AvailableRoles = allRoles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = RoleHelper.GetPersianName(r.Name),
                    Selected = r.Name == filter?.RoleName
                }).ToList();

                _logger.Information("لیست کاربران حذف شده بازیابی شد - Total: {Total}, Page: {Page}. User: {UserId}",
                    totalCount, pageNumber, _currentUserService.UserId);

                return ServiceResult<UserIndexViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بازیابی لیست کاربران حذف شده. User: {UserId}", _currentUserService.UserId);
                return ServiceResult<UserIndexViewModel>.Failed("خطا در بازیابی اطلاعات کاربران حذف شده.", "DB_ERROR");
            }
        }

        /// <summary>
        /// بازگردانی کاربر حذف شده
        /// </summary>
        public async Task<ServiceResult<bool>> RestoreUserAsync(string userId)
        {
            try
            {
                _logger.Information("درخواست بازگردانی کاربر - UserId: {UserId}. RestoredBy: {RestoredBy}",
                    userId, _currentUserService.UserId);

                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult<bool>.Failed("شناسه کاربر معتبر نیست.");
                }

                // ✅ بازگردانی کاربر
                var result = await _userRepository.RestoreAsync(userId, _currentUserService.UserId);
                if (!result)
                {
                    return ServiceResult<bool>.Failed("خطا در بازگردانی کاربر.");
                }

                _logger.Information("کاربر با موفقیت بازگردانی شد - UserId: {UserId}. RestoredBy: {RestoredBy}",
                    userId, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(true, "کاربر با موفقیت بازگردانی شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بازگردانی کاربر - UserId: {UserId}", userId);
                return ServiceResult<bool>.Failed("خطای سیستمی در بازگردانی کاربر رخ داد.", "DB_ERROR");
            }
        }

        #endregion
    }
}

