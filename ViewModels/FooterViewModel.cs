using System;
using System.Collections.Generic;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ViewModel برای Footer حرفه‌ای محیط درمانی
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class FooterViewModel
    {
        // 1. Brand & Identity
        public BrandInfoFooterViewModel BrandInfo { get; set; }
        
        // 2. Contact Information
        public ContactInfoFooterViewModel ContactInfo { get; set; }
        
        // 3. Quick Links
        public List<FooterLinkViewModel> QuickLinks { get; set; } = new List<FooterLinkViewModel>();
        
        // 4. Services Links
        public List<FooterLinkViewModel> ServiceLinks { get; set; } = new List<FooterLinkViewModel>();
        
        // 5. Legal & Compliance
        public LegalInfoFooterViewModel LegalInfo { get; set; }
        
        // 6. Certifications & Licenses
        public List<CertificationViewModel> Certifications { get; set; } = new List<CertificationViewModel>();
        
        // 7. Social Media
        public List<SocialMediaViewModel> SocialMedia { get; set; } = new List<SocialMediaViewModel>();
        
        // 8. Working Hours
        public WorkingHoursFooterViewModel WorkingHours { get; set; }
    }

    #region Brand Info

    public class BrandInfoFooterViewModel
    {
        public string ClinicName { get; set; }
        public string LogoUrl { get; set; }
        public string Tagline { get; set; }
        public string Description { get; set; }
        public string HomeUrl { get; set; }
    }

    #endregion

    #region Contact Info

    public class ContactInfoFooterViewModel
    {
        public string PhoneNumber { get; set; }
        public string PhoneLink { get; set; }
        public string EmergencyPhone { get; set; }
        public string EmergencyPhoneLink { get; set; }
        public string Email { get; set; }
        public string EmailLink { get; set; }
        public string Address { get; set; }
        public string GoogleMapsLink { get; set; }
        public string WhatsAppNumber { get; set; }
        public string WhatsAppLink { get; set; }
    }

    #endregion

    #region Footer Links

    public class FooterLinkViewModel
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public bool IsExternal { get; set; }
        public int Order { get; set; }
    }

    #endregion

    #region Legal Info

    public class LegalInfoFooterViewModel
    {
        public string CopyrightText { get; set; }
        public int CurrentYear { get; set; }
        public string PrivacyPolicyUrl { get; set; }
        public string TermsOfServiceUrl { get; set; }
        public string ComplaintsUrl { get; set; }
        public string MedicalPrivacyNotice { get; set; }
    }

    #endregion

    #region Certifications

    public class CertificationViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string LinkUrl { get; set; }
        public string LicenseNumber { get; set; }
        public int Order { get; set; }
    }

    #endregion

    #region Social Media

    public class SocialMediaViewModel
    {
        public string Platform { get; set; } // Instagram, Telegram, WhatsApp, Facebook
        public string Url { get; set; }
        public string Icon { get; set; }
        public string AriaLabel { get; set; }
        public int Order { get; set; }
    }

    #endregion

    #region Working Hours

    public class WorkingHoursFooterViewModel
    {
        public string Title { get; set; }
        public List<WorkingDayViewModel> WorkingDays { get; set; } = new List<WorkingDayViewModel>();
        public bool IsOpenNow { get; set; }
        public string CurrentStatus { get; set; }
    }

    #endregion
}

