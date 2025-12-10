using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface Repository برای عملیات داده‌ای ClinicWorkingHours
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IClinicWorkingHoursRepository
    {
        Task<ClinicWorkingHours> GetByIdAsync(int clinicWorkingHoursId);
        Task<List<ClinicWorkingHours>> GetAllAsync(bool includeDeleted = false);
        Task<List<ClinicWorkingHours>> GetActiveWorkingHoursAsync(int? clinicId = null);
        Task<List<ClinicWorkingHours>> GetByClinicIdAsync(int clinicId);
        Task<ClinicWorkingHours> GetByDayOfWeekAsync(int dayOfWeek, int? clinicId = null);
        void Add(ClinicWorkingHours clinicWorkingHours);
        void Update(ClinicWorkingHours clinicWorkingHours);
        void Delete(ClinicWorkingHours clinicWorkingHours);
    }
}

