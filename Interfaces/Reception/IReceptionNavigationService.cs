using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// Interface سرویس ناوبری تخصصی برای ماژول پذیرش - Strongly Typed
    /// </summary>
    public interface IReceptionNavigationService
    {
        /// <summary>
        /// دریافت ناوبری پزشکی تخصصی - Strongly Typed
        /// </summary>
        Task<ServiceResult<MedicalNavigationViewModel>> GetMedicalNavigationAsync(string currentController, string currentAction);

        /// <summary>
        /// دریافت ناوبری دپارتمان‌ها - Strongly Typed
        /// </summary>
        Task<ServiceResult<DepartmentNavigationViewModel>> GetDepartmentNavigationAsync(int? selectedDepartmentId = null);

        /// <summary>
        /// دریافت عملیات سریع - Strongly Typed
        /// </summary>
        Task<ServiceResult<List<QuickAction>>> GetQuickActionsAsync();

        /// <summary>
        /// بررسی دسترسی کاربر به آیتم ناوبری - Strongly Typed
        /// </summary>
        Task<bool> HasNavigationPermissionAsync(string controller, string action, string permission = null);

        /// <summary>
        /// دریافت ناوبری بر اساس نقش کاربر - Strongly Typed
        /// </summary>
        Task<ServiceResult<MedicalNavigationViewModel>> GetNavigationByUserRoleAsync(string userRole, string currentController, string currentAction);
    }
}
