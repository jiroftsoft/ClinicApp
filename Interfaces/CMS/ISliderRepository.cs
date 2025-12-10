using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Repository interface for Slider entity operations
    /// </summary>
    public interface ISliderRepository
    {
        Task<Slider> GetByIdAsync(int sliderId);
        Task<List<Slider>> GetActiveSlidersAsync(string position = null);
        Task<List<Slider>> GetAllAsync(bool includeDeleted = false);
        void Add(Slider slider);
        void Update(Slider slider);
        void Delete(Slider slider);
        Task<bool> ExistsAsync(int sliderId);
    }
}

