using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Account;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Service for user self-profile management
    /// Single Responsibility: Manage user's own profile (not admin operations)
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Get current user's profile information
        /// </summary>
        Task<ServiceResult<UserProfileEditViewModel>> GetMyProfileAsync(string userId);

        /// <summary>
        /// Update current user's profile (only allowed fields)
        /// </summary>
        Task<ServiceResult> UpdateMyProfileAsync(string userId, UserProfileEditViewModel model);
    }
}

