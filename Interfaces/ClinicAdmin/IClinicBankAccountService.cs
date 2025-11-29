using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.ClinicAdmin;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// Service Interface برای مدیریت حساب بانکی کلینیک
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت حساب بانکی هر کلینیک (شماره شبا)
    /// 2. Validation کامل شماره شبا
    /// 3. مدیریت رابطه One-to-One با Clinic
    /// 4. استفاده از ServiceResult Pattern
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط منطق کسب‌وکار حساب بانکی
    /// ✅ Separation of Concerns: جدا از Repository و Controller
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Service layer فقط منطق کسب‌وکار
    /// </summary>
    public interface IClinicBankAccountService
    {
        /// <summary>
        /// ایجاد حساب بانکی جدید برای کلینیک
        /// </summary>
        Task<ServiceResult<int>> CreateAsync(ClinicBankAccountCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی حساب بانکی
        /// </summary>
        Task<ServiceResult> UpdateAsync(ClinicBankAccountCreateEditViewModel model);

        /// <summary>
        /// دریافت حساب بانکی بر اساس شناسه
        /// </summary>
        Task<ServiceResult<ClinicBankAccountDetailsViewModel>> GetByIdAsync(int clinicBankAccountId);

        /// <summary>
        /// دریافت حساب بانکی بر اساس شناسه کلینیک
        /// </summary>
        Task<ServiceResult<ClinicBankAccountDetailsViewModel>> GetByClinicIdAsync(int clinicId);

        /// <summary>
        /// دریافت لیست تمام حساب‌های بانکی
        /// </summary>
        Task<ServiceResult<List<ClinicBankAccountIndexViewModel>>> GetAllAsync();

        /// <summary>
        /// دریافت اطلاعات برای ویرایش
        /// </summary>
        Task<ServiceResult<ClinicBankAccountCreateEditViewModel>> GetForEditAsync(int clinicBankAccountId);

        /// <summary>
        /// حذف حساب بانکی (Soft Delete)
        /// </summary>
        Task<ServiceResult> DeleteAsync(int clinicBankAccountId);

        /// <summary>
        /// بررسی وجود حساب بانکی برای کلینیک
        /// </summary>
        Task<ServiceResult<bool>> ExistsForClinicAsync(int clinicId);
    }
}

