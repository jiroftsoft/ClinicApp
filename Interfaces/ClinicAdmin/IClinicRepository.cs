using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// اینترفیس تخصصی برای عملیات داده مربوط به موجودیت کلینیک.
    /// این کلاس تمام جزئیات دسترسی به پایگاه داده را از لایه سرویس پنهان می‌کند.
    /// </summary>
    public interface IClinicRepository
    {
        /// <summary>
        /// جستجوی کلینیک‌ها بر اساس نام یا آدرس به صورت غیرهمگام.
        /// </summary>
        Task<List<Clinic>> GetClinicsAsync(string searchTerm);

        /// <summary>
        /// دریافت یک کلینیک با شناسه آن به همراه اطلاعات کاربران مرتبط به صورت غیرهمگام.
        /// </summary>
        Task<Clinic> GetByIdAsync(int clinicId);

        /// <summary>
        /// بررسی وجود کلینیک با نام تکراری به صورت غیرهمگام.
        /// </summary>
        Task<bool> DoesClinicExistAsync(string name, int? excludeId = null);

        /// <summary>
        /// ✅ **تغییر کلیدی:** این متد همگام (synchronous) است زیرا فقط وضعیت موجودیت را در حافظه تغییر می‌دهد.
        /// </summary>
        void Add(Clinic clinic);

        /// <summary>
        /// علامت‌گذاری یک کلینیک به عنوان "ویرایش شده" (عملیات همگام).
        /// </summary>
        void Update(Clinic clinic);

        /// <summary>
        /// علامت‌گذاری یک کلینیک برای حذف (عملیات همگام).
        /// </summary>
        void Delete(Clinic clinic);

        /// <summary>
        /// دریافت لیست تمام کلینیک‌های فعال به صورت غیرهمگام.
        /// </summary>
        Task<List<Clinic>> GetActiveClinicsAsync();

        /// <summary>
        /// ذخیره تمام تغییرات در صف در پایگاه داده به صورت غیرهمگام.
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// 🏥 MEDICAL: بررسی وابستگی‌های کلینیک قبل از حذف
        /// </summary>
        Task<ClinicDependencyInfo> GetClinicDependencyInfoAsync(int clinicId);

        /// <summary>
        /// 🏥 MEDICAL: بررسی امکان حذف کلینیک بر اساس وابستگی‌ها
        /// </summary>
        Task<bool> CanDeleteClinicAsync(int clinicId);
    }
}