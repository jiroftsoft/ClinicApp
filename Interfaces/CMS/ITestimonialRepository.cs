using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Repository interface for Testimonial entity operations
    /// </summary>
    public interface ITestimonialRepository
    {
        Task<Testimonial> GetByIdAsync(int testimonialId);
        Task<List<Testimonial>> GetApprovedTestimonialsAsync(int count = 10);
        Task<List<Testimonial>> GetFeaturedTestimonialsAsync(int count = 3);
        Task<List<Testimonial>> GetAllAsync(bool includeDeleted = false);
        Task<List<Testimonial>> GetPendingApprovalAsync();
        void Add(Testimonial testimonial);
        void Update(Testimonial testimonial);
        void Delete(Testimonial testimonial);
        Task<bool> ExistsAsync(int testimonialId);
    }
}

