using System.Threading.Tasks;
using ClinicApp.ViewModels;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس صفحه اصلی کلینیک
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IHomePageService
    {
        /// <summary>
        /// دریافت تمام داده‌های صفحه اصلی
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری - اگر null باشد، کلینیک پیش‌فرض استفاده می‌شود)</param>
        /// <returns>ViewModel جامع صفحه اصلی</returns>
        Task<HomePageViewModel> GetHomePageDataAsync(int? clinicId = null);

        /// <summary>
        /// دریافت داده‌های بخش Hero
        /// </summary>
        Task<HeroSectionViewModel> GetHeroSectionAsync(int? clinicId = null);

        /// <summary>
        /// دریافت داده‌های بخش Value Proposition
        /// </summary>
        Task<ValuePropositionViewModel> GetValuePropositionAsync(int? clinicId = null);

        /// <summary>
        /// دریافت داده‌های بخش Services
        /// </summary>
        /// <param name="count">تعداد خدمات برای نمایش (پیش‌فرض: 6)</param>
        Task<ServicesSectionViewModel> GetServicesSectionAsync(int count = 6, int? clinicId = null);

        /// <summary>
        /// دریافت داده‌های بخش Doctors
        /// </summary>
        /// <param name="count">تعداد پزشکان برای نمایش (پیش‌فرض: 4)</param>
        Task<DoctorsSectionViewModel> GetDoctorsSectionAsync(int count = 4, int? clinicId = null);

        /// <summary>
        /// دریافت داده‌های بخش Quick Appointment
        /// </summary>
        Task<QuickAppointmentViewModel> GetQuickAppointmentSectionAsync(int? clinicId = null);

        /// <summary>
        /// دریافت داده‌های بخش Testimonials
        /// </summary>
        /// <param name="count">تعداد نظرات برای نمایش (پیش‌فرض: 3)</param>
        Task<TestimonialsSectionViewModel> GetTestimonialsSectionAsync(int count = 3, int? clinicId = null);

        /// <summary>
        /// دریافت داده‌های بخش Gallery
        /// </summary>
        /// <param name="count">تعداد تصاویر برای نمایش (پیش‌فرض: 6)</param>
        Task<GallerySectionViewModel> GetGallerySectionAsync(int count = 6, int? clinicId = null);

        /// <summary>
        /// دریافت داده‌های بخش Blog
        /// </summary>
        /// <param name="count">تعداد مقالات برای نمایش (پیش‌فرض: 3)</param>
        Task<BlogSectionViewModel> GetBlogSectionAsync(int count = 3, int? clinicId = null);

        /// <summary>
        /// دریافت داده‌های بخش Video
        /// </summary>
        /// <param name="count">تعداد ویدیوها برای نمایش (پیش‌فرض: 6)</param>
        /// <param name="category">دسته‌بندی ویدیو (اختیاری)</param>
        Task<VideoSectionViewModel> GetVideoSectionAsync(int count = 6, string category = null, int? clinicId = null);

        /// <summary>
        /// دریافت داده‌های بخش Contact
        /// </summary>
        Task<ContactSectionViewModel> GetContactSectionAsync(int? clinicId = null);
    }
}

