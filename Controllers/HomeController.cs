using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Filters;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر صفحه اصلی کلینیک
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IHomePageService _homePageService;
        private readonly IAnnouncementService _announcementService;
        private readonly IFAQService _faqService;
        private readonly IHealthTipService _healthTipService;
        private readonly IInsuranceInfoService _insuranceInfoService;
        private readonly IMedicalServiceInfoService _medicalServiceInfoService;
        private readonly IEmergencyContactService _emergencyContactService;
        private readonly IStoryService _storyService;

        public HomeController(
            IHomePageService homePageService,
            IAnnouncementService announcementService,
            IFAQService faqService,
            IHealthTipService healthTipService,
            IInsuranceInfoService insuranceInfoService,
            IMedicalServiceInfoService medicalServiceInfoService,
            IEmergencyContactService emergencyContactService,
            IStoryService storyService)
        {
            _homePageService = homePageService ?? throw new ArgumentNullException(nameof(homePageService));
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
            _faqService = faqService ?? throw new ArgumentNullException(nameof(faqService));
            _healthTipService = healthTipService ?? throw new ArgumentNullException(nameof(healthTipService));
            _insuranceInfoService = insuranceInfoService ?? throw new ArgumentNullException(nameof(insuranceInfoService));
            _medicalServiceInfoService = medicalServiceInfoService ?? throw new ArgumentNullException(nameof(medicalServiceInfoService));
            _emergencyContactService = emergencyContactService ?? throw new ArgumentNullException(nameof(emergencyContactService));
            _storyService = storyService ?? throw new ArgumentNullException(nameof(storyService));
        }

        /// <summary>
        /// صفحه اصلی کلینیک
        /// </summary>
        [OutputCache(Duration = 0, VaryByParam = "none", Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public async Task<ActionResult> Index()
        {
            try
            {
                var viewModel = await _homePageService.GetHomePageDataAsync();
                
                // ارسال Footer به ViewBag برای استفاده در Layout
                if (viewModel.Footer != null)
                {
                    ViewBag.Footer = viewModel.Footer;
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // لاگ خطا با جزئیات کامل
                System.Diagnostics.Debug.WriteLine($"❌ ERROR in HomeController.Index: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                // در صورت خطا، صفحه خالی با پیام خطا نمایش داده می‌شود
                ViewBag.ErrorMessage = "خطا در بارگذاری داده‌های صفحه اصلی. لطفاً دوباره تلاش کنید.";
                return View(new HomePageViewModel());
            }
        }

        /// <summary>
        /// صفحه "درباره ما" - Production-Grade
        /// طراحی شده طبق استانداردهای کلینیک درمانی
        /// </summary>
        public async Task<ActionResult> About()
        {
            try
            {
                var viewModel = await _homePageService.GetAboutPageDataAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // لاگ خطا با جزئیات کامل
                System.Diagnostics.Debug.WriteLine($"❌ ERROR in HomeController.About: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                // در صورت خطا، صفحه خالی با ViewModel پیش‌فرض نمایش داده می‌شود
                return View(new AboutPageViewModel());
            }
        }

        /// <summary>
        /// Redirect به ContactController برای استفاده از فرم تماس Production-Grade
        /// </summary>
        public ActionResult Contact()
        {
            return RedirectToAction("Index", "Contact");
        }

        #region Partial Actions (برای کش کردن هر سکشن جداگانه)

        /// <summary>
        /// بخش Announcements (اطلاعیه‌های مهم)
        /// </summary>
        [OutputCache(Duration = 300, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> AnnouncementsSection()
        {
            try
            {
                var announcements = await _announcementService.GetImportantAnnouncementsAsync(5);
                if (announcements.Success && announcements.Data != null && announcements.Data.Any())
                {
                    return PartialView("Sections/_AnnouncementsSection", announcements.Data);
                }
                return new EmptyResult();
            }
            catch
            {
                return new EmptyResult();
            }
        }

        /// <summary>
        /// بخش FAQ (سوالات متداول)
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> FAQSection()
        {
            try
            {
                var faqs = await _faqService.GetFeaturedFAQsAsync(5);
                if (faqs.Success && faqs.Data != null && faqs.Data.Any())
                {
                    return PartialView("Sections/_FAQSection", faqs.Data);
                }
                return new EmptyResult();
            }
            catch
            {
                return new EmptyResult();
            }
        }

        /// <summary>
        /// بخش Health Tips (نکات سلامت)
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> HealthTipsSection()
        {
            try
            {
                var healthTips = await _healthTipService.GetFeaturedHealthTipsAsync(6);
                if (healthTips.Success && healthTips.Data != null && healthTips.Data.Any())
                {
                    return PartialView("Sections/_HealthTipsSection", healthTips.Data);
                }
                return new EmptyResult();
            }
            catch
            {
                return new EmptyResult();
            }
        }

        /// <summary>
        /// بخش Insurance Info (بیمه‌های طرف قرارداد)
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> InsuranceInfoSection()
        {
            try
            {
                var insurances = await _insuranceInfoService.GetFeaturedInsuranceInfosAsync(8);
                if (insurances.Success && insurances.Data != null && insurances.Data.Any())
                {
                    return PartialView("Sections/_InsuranceInfoSection", insurances.Data);
                }
                return new EmptyResult();
            }
            catch
            {
                return new EmptyResult();
            }
        }

        /// <summary>
        /// بخش Medical Services Info (خدمات پزشکی)
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> MedicalServicesSection()
        {
            try
            {
                var services = await _medicalServiceInfoService.GetFeaturedServiceInfosAsync(6);
                if (services.Success && services.Data != null && services.Data.Any())
                {
                    return PartialView("Sections/_MedicalServicesSection", services.Data);
                }
                return new EmptyResult();
            }
            catch
            {
                return new EmptyResult();
            }
        }

        /// <summary>
        /// بخش Emergency Contacts (تماس‌های اضطراری)
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> EmergencyContactsSection()
        {
            try
            {
                var contacts = await _emergencyContactService.GetActiveContactsAsync();
                if (contacts.Success && contacts.Data != null && contacts.Data.Any())
                {
                    return PartialView("Sections/_EmergencyContactsSection", contacts.Data);
                }
                return new EmptyResult();
            }
            catch
            {
                return new EmptyResult();
            }
        }

        /// <summary>
        /// بخش Emergency Contacts Header (همیشه قابل مشاهده)
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> EmergencyContactsHeader()
        {
            try
            {
                var contacts = await _emergencyContactService.GetAlwaysVisibleContactsAsync();
                if (contacts.Success && contacts.Data != null && contacts.Data.Any())
                {
                    return PartialView("_EmergencyContactsHeader", contacts.Data);
                }
                return new EmptyResult();
            }
            catch
            {
                return new EmptyResult();
            }
        }

        /// <summary>
        /// بخش Medical Equipment (تجهیزات پزشکی)
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> MedicalEquipmentSection()
        {
            try
            {
                var viewModel = await _homePageService.GetHomePageDataAsync();
                if (viewModel?.MedicalEquipments != null && viewModel.MedicalEquipments.Any())
                {
                    return PartialView("Sections/_MedicalEquipmentSection", viewModel);
                }
                return new EmptyResult();
            }
            catch
            {
                return new EmptyResult();
            }
        }

        /// <summary>
        /// بخش Hero
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> HeroSection()
        {
            var hero = await _homePageService.GetHeroSectionAsync();
            return PartialView("Sections/_HeroSection", hero);
        }

        /// <summary>
        /// بخش Value Proposition
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> ValuePropositionSection()
        {
            var valueProp = await _homePageService.GetValuePropositionAsync();
            return PartialView("Sections/_ValuePropositionSection", valueProp);
        }

        /// <summary>
        /// بخش Services
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> ServicesSection()
        {
            var services = await _homePageService.GetServicesSectionAsync(6);
            return PartialView("Sections/_ServicesSection", services);
        }

        /// <summary>
        /// بخش Doctors
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> DoctorsSection()
        {
            var doctors = await _homePageService.GetDoctorsSectionAsync(4);
            return PartialView("Sections/_DoctorsSection", doctors);
        }

        /// <summary>
        /// بخش Quick Appointment
        /// </summary>
        [OutputCache(Duration = 300, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> QuickAppointmentSection()
        {
            var quickAppointment = await _homePageService.GetQuickAppointmentSectionAsync();
            return PartialView("Sections/_QuickAppointmentSection", quickAppointment);
        }

        /// <summary>
        /// بخش Testimonials
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> TestimonialsSection()
        {
            var testimonials = await _homePageService.GetTestimonialsSectionAsync(3);
            return PartialView("Sections/_TestimonialsSection", testimonials);
        }

        /// <summary>
        /// بخش Gallery
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> GallerySection()
        {
            var gallery = await _homePageService.GetGallerySectionAsync(6);
            return PartialView("Sections/_GallerySection", gallery);
        }

        /// <summary>
        /// بخش Blog
        /// </summary>
        [OutputCache(Duration = 300, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> BlogSection()
        {
            var blog = await _homePageService.GetBlogSectionAsync(3);
            return PartialView("Sections/_BlogSection", blog);
        }

        /// <summary>
        /// بخش Video
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "category")]
        [ChildActionOnly]
        public async Task<ActionResult> VideoSection(string category = null)
        {
            var videos = await _homePageService.GetVideoSectionAsync(6, category);
            return PartialView("Sections/_VideoSection", videos);
        }

        /// <summary>
        /// بخش Contact
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> ContactSection()
        {
            var contact = await _homePageService.GetContactSectionAsync();
            
            // بارگذاری تماس‌های اضطراری برای نمایش در Contact Section
            var emergencyContacts = await _emergencyContactService.GetActiveContactsAsync();
            if (emergencyContacts.Success && emergencyContacts.Data != null && emergencyContacts.Data.Any())
            {
                ViewBag.EmergencyContacts = emergencyContacts.Data;
            }
            
            return PartialView("Sections/_ContactSection", contact);
        }

        /// <summary>
        /// بخش Stories (برای نمایش زیر منو)
        /// </summary>
        [OutputCache(Duration = 300, VaryByParam = "none")]
        [ChildActionOnly]
        public async Task<ActionResult> StoriesSection()
        {
            try
            {
                var stories = await _storyService.GetActiveStoriesForPublicAsync();
                if (stories.Success && stories.Data != null && stories.Data.Any())
                {
                    return PartialView("Sections/_StoriesSection", stories.Data);
                }
                return new EmptyResult();
            }
            catch
            {
                return new EmptyResult();
            }
        }

        #endregion
    }
}