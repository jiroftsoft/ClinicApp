using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// Repository Interface برای مدیریت حساب بانکی کلینیک
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت حساب بانکی هر کلینیک (شماره شبا)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete)
    /// 3. بهینه‌سازی عملکرد با AsNoTracking
    /// 4. مدیریت رابطه One-to-One با Clinic
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط دسترسی به داده‌های ClinicBankAccount
    /// ✅ Separation of Concerns: منطق کسب‌وکار در Service Layer
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Repository layer فقط دسترسی به داده
    /// </summary>
    public interface IClinicBankAccountRepository
    {
        #region Core CRUD Operations

        /// <summary>
        /// دریافت حساب بانکی بر اساس شناسه
        /// </summary>
        Task<ClinicBankAccount> GetByIdAsync(int clinicBankAccountId);

        /// <summary>
        /// دریافت حساب بانکی بر اساس شناسه کلینیک
        /// </summary>
        Task<ClinicBankAccount> GetByClinicIdAsync(int clinicId);

        /// <summary>
        /// دریافت تمام حساب‌های بانکی فعال
        /// </summary>
        Task<List<ClinicBankAccount>> GetAllAsync();

        /// <summary>
        /// بررسی وجود حساب بانکی برای کلینیک
        /// </summary>
        Task<bool> ExistsForClinicAsync(int clinicId);

        /// <summary>
        /// بررسی وجود شماره شبا (برای جلوگیری از تکراری)
        /// </summary>
        Task<bool> IbanNumberExistsAsync(string ibanNumber, int? excludeId = null);

        /// <summary>
        /// افزودن حساب بانکی جدید
        /// </summary>
        void Add(ClinicBankAccount clinicBankAccount);

        /// <summary>
        /// به‌روزرسانی حساب بانکی
        /// </summary>
        void Update(ClinicBankAccount clinicBankAccount);

        /// <summary>
        /// حذف حساب بانکی (Soft Delete)
        /// </summary>
        void Delete(ClinicBankAccount clinicBankAccount);

        /// <summary>
        /// ذخیره تغییرات در پایگاه داده
        /// </summary>
        Task SaveChangesAsync();

        #endregion
    }
}

