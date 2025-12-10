using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ViewModel جامع برای صفحه اصلی کلینیک
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class HomePageViewModel
    {
        public HeroSectionViewModel Hero { get; set; }
        public ValuePropositionViewModel ValueProposition { get; set; }
        public ServicesSectionViewModel Services { get; set; }
        public DoctorsSectionViewModel Doctors { get; set; }
        public QuickAppointmentViewModel QuickAppointment { get; set; }
        public TestimonialsSectionViewModel Testimonials { get; set; }
        public GallerySectionViewModel Gallery { get; set; }
        public BlogSectionViewModel Blog { get; set; }
        public ContactSectionViewModel Contact { get; set; }
        public List<ClinicApp.ViewModels.CMS.MedicalEquipmentPublicViewModel> MedicalEquipments { get; set; }
    }

    #region Hero Section

    /// <summary>
    /// ViewModel برای بخش Hero (بخش اصلی و جذاب)
    /// </summary>
    public class HeroSectionViewModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string BackgroundVideoUrl { get; set; }
        public string PrimaryButtonText { get; set; }
        public string PrimaryButtonUrl { get; set; }
        public string SecondaryButtonText { get; set; }
        public string SecondaryButtonUrl { get; set; }
        public List<StatisticItemViewModel> Statistics { get; set; } = new List<StatisticItemViewModel>();
    }

    public class StatisticItemViewModel
    {
        public string Icon { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
    }

    #endregion

    #region Value Proposition

    /// <summary>
    /// ViewModel برای بخش معرفی سریع کلینیک (Value Proposition)
    /// </summary>
    public class ValuePropositionViewModel
    {
        public List<ValueItemViewModel> Items { get; set; } = new List<ValueItemViewModel>();
    }

    public class ValueItemViewModel
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    #endregion

    #region Services Section

    /// <summary>
    /// ViewModel برای بخش خدمات کلینیک
    /// </summary>
    public class ServicesSectionViewModel
    {
        public string SectionTitle { get; set; }
        public string SectionSubtitle { get; set; }
        public List<ServiceCardViewModel> Services { get; set; } = new List<ServiceCardViewModel>();
    }

    public class ServiceCardViewModel
    {
        public int ServiceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public string ServiceCode { get; set; }
        public string CategoryName { get; set; }
        public string DetailsUrl { get; set; }
    }

    #endregion

    #region Doctors Section

    /// <summary>
    /// ViewModel برای بخش معرفی پزشکان
    /// </summary>
    public class DoctorsSectionViewModel
    {
        public string SectionTitle { get; set; }
        public string SectionSubtitle { get; set; }
        public List<DoctorCardViewModel> Doctors { get; set; } = new List<DoctorCardViewModel>();
        public string ViewAllDoctorsUrl { get; set; }
    }

    public class DoctorCardViewModel
    {
        public int DoctorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Specialization { get; set; }
        public string PhotoUrl { get; set; }
        public string Bio { get; set; }
        public decimal? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public string ProfileUrl { get; set; }
        public string DoctorCode { get; set; }
    }

    #endregion

    #region Quick Appointment

    /// <summary>
    /// ViewModel برای بخش نوبت‌دهی سریع
    /// </summary>
    public class QuickAppointmentViewModel
    {
        public string SectionTitle { get; set; }
        public string SectionSubtitle { get; set; }
        public List<SpecializationLookupViewModel> Specializations { get; set; } = new List<SpecializationLookupViewModel>();
        public string AppointmentUrl { get; set; }
    }

    public class SpecializationLookupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region Testimonials Section

    /// <summary>
    /// ViewModel برای بخش نظرات بیماران
    /// </summary>
    public class TestimonialsSectionViewModel
    {
        public string SectionTitle { get; set; }
        public string SectionSubtitle { get; set; }
        public List<TestimonialViewModel> Testimonials { get; set; } = new List<TestimonialViewModel>();
    }

    public class TestimonialViewModel
    {
        public int TestimonialId { get; set; }
        public string PatientName { get; set; }
        public string PatientInitials { get; set; }
        public string Comment { get; set; }
        public decimal Rating { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string DoctorName { get; set; }
        public string VideoUrl { get; set; }
        public string PhotoUrl { get; set; }
    }

    #endregion

    #region Gallery Section

    /// <summary>
    /// ViewModel برای بخش گالری محیط کلینیک
    /// </summary>
    public class GallerySectionViewModel
    {
        public string SectionTitle { get; set; }
        public string SectionSubtitle { get; set; }
        public List<GalleryItemViewModel> Items { get; set; } = new List<GalleryItemViewModel>();
    }

    public class GalleryItemViewModel
    {
        public int GalleryId { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    #endregion

    #region Blog Section

    /// <summary>
    /// ViewModel برای بخش بلاگ / مقالات
    /// </summary>
    public class BlogSectionViewModel
    {
        public string SectionTitle { get; set; }
        public string SectionSubtitle { get; set; }
        public List<BlogPostViewModel> Posts { get; set; } = new List<BlogPostViewModel>();
        public string ViewAllPostsUrl { get; set; }
    }

    public class BlogPostViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string ImageUrl { get; set; }
        public string AuthorName { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string CategoryName { get; set; }
        public string PostUrl { get; set; }
    }

    #endregion

    #region Contact Section

    /// <summary>
    /// ViewModel برای بخش نقشه و اطلاعات تماس
    /// </summary>
    public class ContactSectionViewModel
    {
        public string SectionTitle { get; set; }
        public ClinicInfoViewModel ClinicInfo { get; set; }
        public string GoogleMapsEmbedUrl { get; set; }
        public string GoogleMapsLink { get; set; }
        public string WhatsAppNumber { get; set; }
        public string WhatsAppLink { get; set; }
    }

    public class ClinicInfoViewModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string WorkingHours { get; set; }
        public List<WorkingDayViewModel> WorkingDays { get; set; } = new List<WorkingDayViewModel>();
    }

    public class WorkingDayViewModel
    {
        public string DayName { get; set; }
        public string Hours { get; set; }
        public bool IsOpen { get; set; }
    }

    #endregion
}

