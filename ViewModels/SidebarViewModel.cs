using System;
using System.Collections.Generic;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ViewModel برای Sidebar صفحه اصلی - محیط کاربر (Public)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class SidebarViewModel
    {
        // Quick Appointment Section
        public QuickAppointmentSidebarViewModel QuickAppointment { get; set; }
        
        // Quick Links
        public List<QuickLinkViewModel> QuickLinks { get; set; } = new List<QuickLinkViewModel>();
        
        // Contact Info
        public ContactInfoSidebarViewModel ContactInfo { get; set; }
        
        // Emergency Contacts
        public List<EmergencyContactPublicViewModel> EmergencyContacts { get; set; } = new List<EmergencyContactPublicViewModel>();
        
        // Health Tips (Latest)
        public List<HealthTipPublicViewModel> HealthTips { get; set; } = new List<HealthTipPublicViewModel>();
        
        // Announcements (Latest)
        public List<AnnouncementIndexViewModel> Announcements { get; set; } = new List<AnnouncementIndexViewModel>();
        
        // Sidebar Sliders
        public List<SliderIndexViewModel> Sliders { get; set; } = new List<SliderIndexViewModel>();
        
        // Working Hours
        public WorkingHoursSidebarViewModel WorkingHours { get; set; }
    }

    #region Quick Appointment Sidebar

    public class QuickAppointmentSidebarViewModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string ButtonText { get; set; }
        public string AppointmentUrl { get; set; }
        public List<SpecializationLookupViewModel> Specializations { get; set; } = new List<SpecializationLookupViewModel>();
    }

    #endregion

    #region Quick Links

    public class QuickLinkViewModel
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public bool IsExternal { get; set; }
        public string BadgeText { get; set; }
        public string BadgeClass { get; set; }
        public int Order { get; set; }
    }

    #endregion

    #region Contact Info Sidebar

    public class ContactInfoSidebarViewModel
    {
        public string PhoneNumber { get; set; }
        public string PhoneLink { get; set; }
        public string Email { get; set; }
        public string EmailLink { get; set; }
        public string Address { get; set; }
        public string WhatsAppNumber { get; set; }
        public string WhatsAppLink { get; set; }
        public string GoogleMapsLink { get; set; }
    }

    #endregion

    #region Working Hours Sidebar

    public class WorkingHoursSidebarViewModel
    {
        public string Title { get; set; }
        public List<WorkingDayViewModel> WorkingDays { get; set; } = new List<WorkingDayViewModel>();
        public bool IsOpenNow { get; set; }
        public string CurrentStatus { get; set; }
    }

    #endregion
}

