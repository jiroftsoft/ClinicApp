using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت نظرات بیماران
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface ITestimonialService
    {
        Task<ServiceResult<List<TestimonialIndexViewModel>>> GetTestimonialsAsync(bool includePending = false);
        Task<ServiceResult<TestimonialDetailsViewModel>> GetTestimonialDetailsAsync(int testimonialId);
        Task<ServiceResult<TestimonialCreateEditViewModel>> GetTestimonialForEditAsync(int testimonialId);
        Task<ServiceResult<Testimonial>> CreateTestimonialAsync(TestimonialCreateEditViewModel model);
        Task<ServiceResult<Testimonial>> UpdateTestimonialAsync(TestimonialCreateEditViewModel model);
        Task<ServiceResult> DeleteTestimonialAsync(int testimonialId);
        Task<ServiceResult> ApproveTestimonialAsync(int testimonialId);
        Task<ServiceResult> RejectTestimonialAsync(int testimonialId);
        Task<ServiceResult> SetFeaturedAsync(int testimonialId, bool isFeatured);
        Task<ServiceResult<List<TestimonialIndexViewModel>>> GetPendingApprovalAsync();
    }
}

