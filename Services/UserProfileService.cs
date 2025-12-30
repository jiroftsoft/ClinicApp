using System;
using System.Threading.Tasks;
using ClinicApp.Constants;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.Account;
using Microsoft.AspNet.Identity;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// Service for user self-profile management
    /// Single Responsibility: Manage user's own profile (not admin operations)
    /// </summary>
    public class UserProfileService : IUserProfileService
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public UserProfileService(
            ApplicationUserManager userManager,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger?.ForContext<UserProfileService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<ServiceResult<UserProfileEditViewModel>> GetMyProfileAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult<UserProfileEditViewModel>.Failed(
                        UserProfileConstants.Messages.InvalidUserId,
                        UserProfileConstants.ErrorCodes.InvalidUserId);
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    _logger?.Warning("کاربر یافت نشد یا حذف شده - UserId: {UserId}", userId);
                    return ServiceResult<UserProfileEditViewModel>.Failed(
                        UserProfileConstants.Messages.UserNotFound,
                        UserProfileConstants.ErrorCodes.UserNotFound);
                }

                var viewModel = UserProfileEditViewModel.FromEntity(user);
                return ServiceResult<UserProfileEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در دریافت اطلاعات پروفایل - UserId: {UserId}", userId);
                return ServiceResult<UserProfileEditViewModel>.Failed(
                    UserProfileConstants.Messages.GetProfileError);
            }
        }

        public async Task<ServiceResult> UpdateMyProfileAsync(string userId, UserProfileEditViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult.Failed(
                        UserProfileConstants.Messages.InvalidUserId,
                        UserProfileConstants.ErrorCodes.InvalidUserId);
                }

                // ✅ Security: Ensure user can only update their own profile
                if (userId != _currentUserService.UserId)
                {
                    _logger?.Warning("تلاش برای ویرایش پروفایل کاربر دیگر - RequestedUserId: {RequestedUserId}, CurrentUserId: {CurrentUserId}",
                        userId, _currentUserService.UserId);
                    return ServiceResult.Failed(
                        UserProfileConstants.Messages.Unauthorized,
                        UserProfileConstants.ErrorCodes.Unauthorized);
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    _logger?.Warning("کاربر یافت نشد یا حذف شده - UserId: {UserId}", userId);
                    return ServiceResult.Failed(
                        UserProfileConstants.Messages.UserNotFound,
                        UserProfileConstants.ErrorCodes.UserNotFound);
                }

                // ✅ Update only allowed fields (no NationalCode, no Roles, no IsActive, no PhoneNumber)
                user.FirstName = model.FirstName?.Trim();
                user.LastName = model.LastName?.Trim();
                user.Email = model.Email?.Trim();
                user.Gender = model.Gender;
                user.Address = model.Address?.Trim();
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedByUserId = userId;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors);
                    _logger?.Warning("خطا در به‌روزرسانی پروفایل - UserId: {UserId}, Errors: {Errors}", userId, errors);
                    return ServiceResult.Failed($"{UserProfileConstants.Messages.UpdateProfileError}: {errors}");
                }

                _logger?.Information("پروفایل با موفقیت به‌روزرسانی شد - UserId: {UserId}", userId);
                return ServiceResult.Successful(UserProfileConstants.Messages.ProfileUpdatedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در به‌روزرسانی پروفایل - UserId: {UserId}", userId);
                return ServiceResult.Failed(UserProfileConstants.Messages.UpdateProfileError);
            }
        }
    }
}

