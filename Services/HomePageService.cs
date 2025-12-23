using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس صفحه اصلی کلینیک
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class HomePageService : IHomePageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly IDoctorCrudRepository _doctorRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IClinicRepository _clinicRepository;
        private readonly IBlogPostRepository _blogPostRepository;
        private readonly ISliderRepository _sliderRepository;
        private readonly ITestimonialRepository _testimonialRepository;
        private readonly IGalleryItemRepository _galleryItemRepository;
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly IClinicWorkingHoursService _clinicWorkingHoursService;
        private readonly IMedicalEquipmentService _medicalEquipmentService;
        private readonly IVideoService _videoService;
        private readonly IAnnouncementService _announcementService;
        private readonly IFAQService _faqService;
        private readonly IHealthTipService _healthTipService;
        private readonly IInsuranceInfoService _insuranceInfoService;
        private readonly IMedicalServiceInfoService _medicalServiceInfoService;
        private readonly IEmergencyContactService _emergencyContactService;
        private readonly IAboutPageService _aboutPageService;

        public HomePageService(
            ApplicationDbContext context,
            ILogger logger,
            IDoctorCrudRepository doctorRepository,
            IServiceRepository serviceRepository,
            IClinicRepository clinicRepository,
            IBlogPostRepository blogPostRepository,
            ISliderRepository sliderRepository,
            ITestimonialRepository testimonialRepository,
            IGalleryItemRepository galleryItemRepository,
            IAnnouncementRepository announcementRepository,
            IClinicWorkingHoursService clinicWorkingHoursService,
            IMedicalEquipmentService medicalEquipmentService,
            IVideoService videoService,
            IAnnouncementService announcementService,
            IFAQService faqService,
            IHealthTipService healthTipService,
            IInsuranceInfoService insuranceInfoService,
            IMedicalServiceInfoService medicalServiceInfoService,
            IEmergencyContactService emergencyContactService,
            IAboutPageService aboutPageService = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _clinicRepository = clinicRepository ?? throw new ArgumentNullException(nameof(clinicRepository));
            _blogPostRepository = blogPostRepository ?? throw new ArgumentNullException(nameof(blogPostRepository));
            _sliderRepository = sliderRepository ?? throw new ArgumentNullException(nameof(sliderRepository));
            _testimonialRepository = testimonialRepository ?? throw new ArgumentNullException(nameof(testimonialRepository));
            _galleryItemRepository = galleryItemRepository ?? throw new ArgumentNullException(nameof(galleryItemRepository));
            _announcementRepository = announcementRepository ?? throw new ArgumentNullException(nameof(announcementRepository));
            _clinicWorkingHoursService = clinicWorkingHoursService ?? throw new ArgumentNullException(nameof(clinicWorkingHoursService));
            _medicalEquipmentService = medicalEquipmentService ?? throw new ArgumentNullException(nameof(medicalEquipmentService));
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
            _faqService = faqService ?? throw new ArgumentNullException(nameof(faqService));
            _healthTipService = healthTipService ?? throw new ArgumentNullException(nameof(healthTipService));
            _insuranceInfoService = insuranceInfoService ?? throw new ArgumentNullException(nameof(insuranceInfoService));
            _medicalServiceInfoService = medicalServiceInfoService ?? throw new ArgumentNullException(nameof(medicalServiceInfoService));
            _emergencyContactService = emergencyContactService ?? throw new ArgumentNullException(nameof(emergencyContactService));
            _aboutPageService = aboutPageService; // Optional - اگر null باشد، از داده‌های پیش‌فرض استفاده می‌شود
        }

        /// <summary>
        /// دریافت تمام داده‌های صفحه اصلی
        /// </summary>
        public async Task<HomePageViewModel> GetHomePageDataAsync(int? clinicId = null)
        {
            try
            {
                var effectiveClinicId = clinicId ?? 1; // کلینیک پیش‌فرض: شفا
                _logger.Information("دریافت داده‌های صفحه اصلی - ClinicId: {ClinicId}", effectiveClinicId);

                // لود موازی تمام بخش‌ها برای بهینه‌سازی Performance
                var heroTask = GetHeroSectionAsync(effectiveClinicId);
                var valuePropTask = GetValuePropositionAsync(effectiveClinicId);
                var servicesTask = GetServicesSectionAsync(6, effectiveClinicId);
                var doctorsTask = GetDoctorsSectionAsync(4, effectiveClinicId);
                var quickAppointmentTask = GetQuickAppointmentSectionAsync(effectiveClinicId);
                var testimonialsTask = GetTestimonialsSectionAsync(3, effectiveClinicId);
                var galleryTask = GetGallerySectionAsync(6, effectiveClinicId);
                var blogTask = GetBlogSectionAsync(3, effectiveClinicId);
                var videosTask = GetVideoSectionAsync(6, "endoscopy", effectiveClinicId);
                var contactTask = GetContactSectionAsync(effectiveClinicId);
                var medicalEquipmentsTask = GetMedicalEquipmentsSectionAsync(6);
                
                // لود بخش‌های اضافی
                var announcementsTask = GetAnnouncementsSectionAsync(5);
                var faqsTask = GetFAQsSectionAsync(5);
                var healthTipsTask = GetHealthTipsSectionAsync(4); // 3-4 نکته برای هوم‌پیج
                var insuranceInfosTask = GetInsuranceInfosSectionAsync(8);
                var medicalServiceInfosTask = GetMedicalServiceInfosSectionAsync(6);
                var emergencyContactsTask = GetEmergencyContactsSectionAsync();
                
                // لود Slider Sections
                var sidebarSlidersTask = GetSidebarSlidersAsync();
                var footerSlidersTask = GetFooterSlidersAsync();
                
                // لود Sidebar Data
                var sidebarTask = GetSidebarDataAsync(effectiveClinicId, quickAppointmentTask, contactTask, 
                    emergencyContactsTask, healthTipsTask, announcementsTask, sidebarSlidersTask);
                
                // لود Footer Data
                var footerTask = GetFooterDataInternalAsync(effectiveClinicId, contactTask, emergencyContactsTask);

                // انتظار برای تمام Task ها
                await Task.WhenAll(
                    heroTask, valuePropTask, servicesTask, doctorsTask, quickAppointmentTask,
                    testimonialsTask, galleryTask, blogTask, videosTask, contactTask,
                    medicalEquipmentsTask, announcementsTask, faqsTask, healthTipsTask,
                    insuranceInfosTask, medicalServiceInfosTask, emergencyContactsTask,
                    sidebarSlidersTask, footerSlidersTask, sidebarTask, footerTask);

                var viewModel = new HomePageViewModel
                {
                    Hero = await heroTask,
                    ValueProposition = await valuePropTask,
                    Services = await servicesTask,
                    Doctors = await doctorsTask,
                    QuickAppointment = await quickAppointmentTask,
                    Testimonials = await testimonialsTask,
                    Gallery = await galleryTask,
                    Blog = await blogTask,
                    Videos = await videosTask,
                    Contact = await contactTask,
                    MedicalEquipments = await medicalEquipmentsTask,
                    Announcements = await announcementsTask,
                    FAQs = await faqsTask,
                    HealthTips = await healthTipsTask,
                    InsuranceInfos = await insuranceInfosTask,
                    MedicalServiceInfos = await medicalServiceInfosTask,
                    EmergencyContacts = await emergencyContactsTask,
                    SidebarSliders = await sidebarSlidersTask,
                    FooterSliders = await footerSlidersTask,
                    Sidebar = await sidebarTask,
                    Footer = await footerTask
                };

                _logger.Information("✅ داده‌های صفحه اصلی با موفقیت دریافت شد");
                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت داده‌های صفحه اصلی");
                throw;
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Hero
        /// </summary>
        public async Task<HeroSectionViewModel> GetHeroSectionAsync(int? clinicId = null)
        {
            try
            {
                var effectiveClinicId = clinicId ?? 1;
                var clinic = await _clinicRepository.GetByIdAsync(effectiveClinicId);

                // دریافت اسلایدرهای فعال برای Hero از دیتابیس
                var heroSliders = await _sliderRepository.GetActiveSlidersAsync("hero");
                
                _logger.Information("Hero Sliders - تعداد اسلایدرهای فعال از دیتابیس: {Count}, ClinicId: {ClinicId}", 
                    heroSliders?.Count ?? 0, effectiveClinicId);
                
                // تبدیل به ViewModel و اصلاح مسیر تصاویر - فقط از دیتابیس
                var slides = heroSliders
                    .Where(s => !string.IsNullOrWhiteSpace(s.ImageUrl) || !string.IsNullOrWhiteSpace(s.ThumbnailUrl)) // فقط اسلایدرهایی که تصویر یا Thumbnail دارند
                    .Select(s => {
                        // استفاده از ImagePathHelper برای نرمال‌سازی مسیرها
                        var imageUrl = ImagePathHelper.NormalizeImagePath(s.ImageUrl);
                        var thumbnailUrl = ImagePathHelper.NormalizeImagePath(s.ThumbnailUrl);
                        
                        // Logging برای Debug
                        _logger.Information("Hero Slider - SliderId: {SliderId}, Title: {Title}, ImageUrl: {ImageUrl}, ThumbnailUrl: {ThumbnailUrl}",
                            s.SliderId, s.Title, imageUrl, thumbnailUrl);
                        
                        return new HeroSlideViewModel
                        {
                            SliderId = s.SliderId,
                            Title = s.Title,
                            Description = s.Description,
                            ImageUrl = imageUrl, // فقط از دیتابیس
                            ThumbnailUrl = thumbnailUrl, // فقط از دیتابیس
                            LinkUrl = s.LinkUrl,
                            ButtonText = s.ButtonText,
                            DisplayOrder = s.DisplayOrder
                        };
                    })
                    .OrderBy(s => s.DisplayOrder) // مرتب‌سازی بر اساس DisplayOrder
                    .ToList();

                // اگر اسلایدری وجود نداشت، null برمی‌گردانیم (بدون hard code)
                if (!slides.Any())
                {
                    _logger.Warning("هیچ اسلایدر فعالی برای Hero Section یافت نشد. Hero Section نمایش داده نمی‌شود.");
                    return null; // بدون hard code - اگر اسلایدری وجود نداشت، null برمی‌گردانیم
                }

                var firstSlide = slides.FirstOrDefault();

                // فقط از داده‌های دیتابیس استفاده می‌کنیم - بدون hard code
                return new HeroSectionViewModel
                {
                    Title = firstSlide?.Title ?? string.Empty,
                    Subtitle = firstSlide?.Description ?? string.Empty,
                    BackgroundImageUrl = firstSlide?.ImageUrl, // فقط از دیتابیس
                    BackgroundVideoUrl = null,
                    PrimaryButtonText = firstSlide?.ButtonText ?? string.Empty,
                    PrimaryButtonUrl = firstSlide?.LinkUrl ?? string.Empty,
                    SecondaryButtonText = "مشاوره آنلاین",
                    SecondaryButtonUrl = "/Patient/Appointment/Consultation",
                    Statistics = new List<StatisticItemViewModel>
                    {
                        new StatisticItemViewModel { Icon = "fas fa-user-md", Label = "پزشک متخصص", Value = "45+" },
                        new StatisticItemViewModel { Icon = "fas fa-users", Label = "بیمار راضی", Value = "15,000+" },
                        new StatisticItemViewModel { Icon = "fas fa-headset", Label = "پشتیبانی", Value = "24/7" }
                    },
                    Slides = slides // فقط از دیتابیس
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Hero Section از دیتابیس");
                // در صورت خطا، null برمی‌گردانیم - بدون hard code
                return null;
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Value Proposition
        /// </summary>
        public async Task<ValuePropositionViewModel> GetValuePropositionAsync(int? clinicId = null)
        {
            return new ValuePropositionViewModel
            {
                Items = new List<ValueItemViewModel>
                {
                    new ValueItemViewModel
                    {
                        Icon = "fas fa-user-md",
                        Title = "پزشکان متخصص",
                        Description = "تیمی از پزشکان باتجربه و متخصص در حوزه‌های مختلف پزشکی"
                    },
                    new ValueItemViewModel
                    {
                        Icon = "fas fa-microscope",
                        Title = "تجهیزات مدرن",
                        Description = "استفاده از جدیدترین تجهیزات پزشکی و روش‌های درمانی"
                    },
                    new ValueItemViewModel
                    {
                        Icon = "fas fa-calendar-check",
                        Title = "نوبت‌دهی آنلاین",
                        Description = "رزرو سریع و آسان نوبت بدون نیاز به مراجعه حضوری"
                    },
                    new ValueItemViewModel
                    {
                        Icon = "fas fa-headset",
                        Title = "پشتیبانی و مشاوره",
                        Description = "پشتیبانی 24/7 و مشاوره آنلاین برای بیماران"
                    }
                }
            };
        }

        /// <summary>
        /// دریافت داده‌های بخش Services
        /// </summary>
        public async Task<ServicesSectionViewModel> GetServicesSectionAsync(int count = 6, int? clinicId = null)
        {
            try
            {
                var services = await _serviceRepository.GetAllActiveServicesAsync();
                var featuredServices = services
                    .Take(count)
                    .Select(s => new ServiceCardViewModel
                    {
                        ServiceId = s.ServiceId,
                        Title = s.Title,
                        Description = s.Description ?? "خدمات پزشکی با کیفیت",
                        Icon = GetServiceIcon(s.ServiceCategory?.Title ?? ""),
                        ServiceCode = s.ServiceCode,
                        Price = s.PriceToman,
                        CategoryName = s.ServiceCategory?.Title ?? "عمومی",
                        DetailsUrl = $"/Services/Details/{s.ServiceId}"
                    })
                    .ToList();

                return new ServicesSectionViewModel
                {
                    SectionTitle = "خدمات کلینیک",
                    SectionSubtitle = "خدمات متنوع و با کیفیت برای سلامت شما",
                    Services = featuredServices
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Services Section");
                return new ServicesSectionViewModel
                {
                    SectionTitle = "خدمات کلینیک",
                    SectionSubtitle = "خدمات متنوع و با کیفیت برای سلامت شما",
                    Services = new List<ServiceCardViewModel>()
                };
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Doctors
        /// </summary>
        public async Task<DoctorsSectionViewModel> GetDoctorsSectionAsync(int count = 4, int? clinicId = null)
        {
            try
            {
                var effectiveClinicId = clinicId ?? 1;
                var doctors = await _context.Doctors
                    .AsNoTracking()
                    .Where(d => !d.IsDeleted && d.IsActive && (d.ClinicId == effectiveClinicId || effectiveClinicId == 0))
                    .Include(d => d.DoctorSpecializations)
                    .Include(d => d.DoctorSpecializations.Select(ds => ds.Specialization))
                    .OrderBy(d => d.FirstName)
                    .Take(count)
                    .ToListAsync();

                var doctorCards = doctors.Select(d => new DoctorCardViewModel
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Specialization = d.DoctorSpecializations?.FirstOrDefault()?.Specialization?.Name ?? d.SpecializationName ?? "عمومی",
                    PhotoUrl = d.ProfileImageUrl ?? "/Content/Images/default-doctor.jpg",
                    Bio = d.Bio ?? "پزشک متخصص با تجربه",
                    Rating = 4.5m, // TODO: محاسبه از نظرات
                    ReviewCount = 0, // TODO: محاسبه از نظرات
                    ProfileUrl = $"/Patient/Appointment/DoctorDetails?doctorId={d.DoctorId}",
                    DoctorCode = d.DoctorCode
                }).ToList();

                return new DoctorsSectionViewModel
                {
                    SectionTitle = "پزشکان باتجربه ما",
                    SectionSubtitle = "تیمی از پزشکان متخصص که سلامت شما را در اولویت قرار داده‌اند",
                    Doctors = doctorCards,
                    ViewAllDoctorsUrl = "/Patient/Appointment/Doctors"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Doctors Section");
                return new DoctorsSectionViewModel
                {
                    SectionTitle = "پزشکان باتجربه ما",
                    SectionSubtitle = "تیمی از پزشکان متخصص که سلامت شما را در اولویت قرار داده‌اند",
                    Doctors = new List<DoctorCardViewModel>()
                };
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Quick Appointment
        /// </summary>
        public async Task<QuickAppointmentViewModel> GetQuickAppointmentSectionAsync(int? clinicId = null)
        {
            try
            {
                var specializations = await _context.Specializations
                    .AsNoTracking()
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .OrderBy(s => s.Name)
                    .Select(s => new SpecializationLookupViewModel
                    {
                        Id = s.SpecializationId,
                        Name = s.Name
                    })
                    .Take(10)
                    .ToListAsync();

                return new QuickAppointmentViewModel
                {
                    SectionTitle = "نوبت خود را آنلاین رزرو کنید",
                    SectionSubtitle = "در کمتر از 2 دقیقه نوبت خود را آنلاین رزرو کنید",
                    Specializations = specializations,
                    AppointmentUrl = "/Patient/Appointment/Index"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Quick Appointment Section");
                return new QuickAppointmentViewModel
                {
                    SectionTitle = "نوبت خود را آنلاین رزرو کنید",
                    SectionSubtitle = "در کمتر از 2 دقیقه نوبت خود را آنلاین رزرو کنید",
                    Specializations = new List<SpecializationLookupViewModel>(),
                    AppointmentUrl = "/Patient/Appointment/Index"
                };
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Testimonials
        /// </summary>
        public async Task<TestimonialsSectionViewModel> GetTestimonialsSectionAsync(int count = 3, int? clinicId = null)
        {
            try
            {
                var testimonials = await _testimonialRepository.GetApprovedTestimonialsAsync(count);
                
                var testimonialViewModels = testimonials.Select(t => new TestimonialViewModel
                {
                    TestimonialId = t.TestimonialId,
                    PatientName = t.PatientName,
                    PatientInitials = t.PatientInitials ?? GetInitials(t.PatientName),
                    Comment = t.Comment,
                    Rating = t.Rating,
                    CreatedAt = t.ApprovedAt ?? t.CreatedAt,
                    DoctorName = t.DoctorName,
                    VideoUrl = t.VideoUrl,
                    PhotoUrl = t.PhotoUrl
                }).ToList();

                return new TestimonialsSectionViewModel
                {
                    SectionTitle = "نظرات بیماران ما",
                    SectionSubtitle = "این چیزی است که بیماران ما درباره کلینیک می‌گویند",
                    Testimonials = testimonialViewModels
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Testimonials Section");
                return new TestimonialsSectionViewModel
                {
                    SectionTitle = "نظرات بیماران ما",
                    SectionSubtitle = "این چیزی است که بیماران ما درباره کلینیک می‌گویند",
                    Testimonials = new List<TestimonialViewModel>()
                };
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Gallery
        /// </summary>
        public async Task<GallerySectionViewModel> GetGallerySectionAsync(int count = 6, int? clinicId = null)
        {
            try
            {
                var galleryItems = await _galleryItemRepository.GetActiveItemsAsync(count);
                
                var galleryViewModels = galleryItems.Select(g => new GalleryItemViewModel
                {
                    GalleryId = g.GalleryItemId,
                    ImageUrl = g.ImageUrl,
                    ThumbnailUrl = g.ThumbnailUrl ?? g.ImageUrl,
                    Title = g.Title,
                    Description = g.Description
                }).ToList();

                return new GallerySectionViewModel
                {
                    SectionTitle = "گالری محیط کلینیک",
                    SectionSubtitle = "محیطی آرام و درمانی برای بیماران",
                    Items = galleryViewModels
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Gallery Section");
                return new GallerySectionViewModel
                {
                    SectionTitle = "گالری محیط کلینیک",
                    SectionSubtitle = "محیطی آرام و درمانی برای بیماران",
                    Items = new List<GalleryItemViewModel>()
                };
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Blog
        /// </summary>
        public async Task<BlogSectionViewModel> GetBlogSectionAsync(int count = 3, int? clinicId = null)
        {
            try
            {
                var blogPosts = await _blogPostRepository.GetPublishedPostsAsync(count);
                
                var blogViewModels = blogPosts.Select(b => new BlogPostViewModel
                {
                    PostId = b.BlogPostId,
                    Title = b.Title,
                    Summary = b.Summary,
                    ImageUrl = b.ImageUrl ?? b.ThumbnailUrl,
                    AuthorName = b.AuthorName,
                    PublishedAt = b.PublishedAt ?? b.CreatedAt,
                    CategoryName = b.CategoryName,
                    PostUrl = $"/Blog/Post/{b.Slug ?? b.BlogPostId.ToString()}"
                }).ToList();

                return new BlogSectionViewModel
                {
                    SectionTitle = "مقالات و آموزش سلامت",
                    SectionSubtitle = "آخرین مقالات و آموزش‌های سلامت",
                    Posts = blogViewModels,
                    ViewAllPostsUrl = "/Blog"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Blog Section");
                return new BlogSectionViewModel
                {
                    SectionTitle = "مقالات و آموزش سلامت",
                    SectionSubtitle = "آخرین مقالات و آموزش‌های سلامت",
                    Posts = new List<BlogPostViewModel>(),
                    ViewAllPostsUrl = "/Blog"
                };
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Video
        /// </summary>
        public async Task<VideoSectionViewModel> GetVideoSectionAsync(int count = 6, string category = null, int? clinicId = null)
        {
            try
            {
                var videosResult = await _videoService.GetVideosForHomePageAsync(count, category);
                
                if (!videosResult.Success || videosResult.Data == null || !videosResult.Data.Any())
                {
                    return new VideoSectionViewModel
                    {
                        SectionTitle = "ویدیوهای کلینیک",
                        SectionSubtitle = "آخرین ویدیوهای آموزشی و معرفی خدمات",
                        Videos = new List<VideoItemViewModel>()
                    };
                }

                var videoViewModels = videosResult.Data.Select(v => new VideoItemViewModel
                {
                    VideoId = v.VideoId,
                    Title = v.Title,
                    Description = v.Description,
                    VideoUrl = v.VideoUrl,
                    EmbedUrl = v.EmbedUrl,
                    ThumbnailUrl = v.ThumbnailUrl,
                    Category = v.Category,
                    Duration = v.Duration,
                    DurationFormatted = v.DurationFormatted,
                    ViewCount = v.ViewCount,
                    VideoType = v.VideoType,
                    VideoTypeName = GetVideoTypeName(v.VideoType)
                }).ToList();

                return new VideoSectionViewModel
                {
                    SectionTitle = category == "endoscopy" ? "ویدیوهای اندوسکوپی" : "ویدیوهای کلینیک",
                    SectionSubtitle = category == "endoscopy" 
                        ? "معرفی بخش اندوسکوپی و خدمات مرتبط" 
                        : "آخرین ویدیوهای آموزشی و معرفی خدمات",
                    Videos = videoViewModels
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Video Section");
                return new VideoSectionViewModel
                {
                    SectionTitle = "ویدیوهای کلینیک",
                    SectionSubtitle = "آخرین ویدیوهای آموزشی و معرفی خدمات",
                    Videos = new List<VideoItemViewModel>()
                };
            }
        }

        /// <summary>
        /// Helper method برای تبدیل VideoType به نام فارسی
        /// </summary>
        private string GetVideoTypeName(ClinicApp.Models.Enums.VideoType videoType)
        {
            switch (videoType)
            {
                case ClinicApp.Models.Enums.VideoType.YouTube:
                    return "YouTube";
                case ClinicApp.Models.Enums.VideoType.Vimeo:
                    return "Vimeo";
                case ClinicApp.Models.Enums.VideoType.Aparat:
                    return "آپارات";
                case ClinicApp.Models.Enums.VideoType.DirectUpload:
                    return "آپلود مستقیم";
                default:
                    return "نامشخص";
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Contact
        /// </summary>
        public async Task<ContactSectionViewModel> GetContactSectionAsync(int? clinicId = null)
        {
            try
            {
                var effectiveClinicId = clinicId ?? 1;
                var clinic = await _clinicRepository.GetByIdAsync(effectiveClinicId);

                // دریافت ساعات کاری از دیتابیس
                var workingHoursResult = await _clinicWorkingHoursService.GetActiveWorkingHoursAsync(effectiveClinicId);
                var workingDays = new List<WorkingDayViewModel>();

                if (workingHoursResult.Success && workingHoursResult.Data != null && workingHoursResult.Data.Any())
                {
                    workingDays = workingHoursResult.Data
                        .OrderBy(w => w.DayOfWeek)
                        .Select(w => new WorkingDayViewModel
                        {
                            DayName = w.DayName,
                            Hours = w.TimeRange,
                            IsOpen = w.IsOpen
                        }).ToList();
                }
                else
                {
                    // Fallback به داده‌های پیش‌فرض در صورت عدم وجود داده
                    workingDays = new List<WorkingDayViewModel>
                    {
                        new WorkingDayViewModel { DayName = "شنبه", Hours = "8:00 - 20:00", IsOpen = true },
                        new WorkingDayViewModel { DayName = "یکشنبه", Hours = "8:00 - 20:00", IsOpen = true },
                        new WorkingDayViewModel { DayName = "دوشنبه", Hours = "8:00 - 20:00", IsOpen = true },
                        new WorkingDayViewModel { DayName = "سه‌شنبه", Hours = "8:00 - 20:00", IsOpen = true },
                        new WorkingDayViewModel { DayName = "چهارشنبه", Hours = "8:00 - 20:00", IsOpen = true },
                        new WorkingDayViewModel { DayName = "پنج‌شنبه", Hours = "8:00 - 14:00", IsOpen = true },
                        new WorkingDayViewModel { DayName = "جمعه", Hours = "تعطیل", IsOpen = false }
                    };
                }

                // تولید متن WorkingHours از WorkingDays
                var workingHoursText = string.Join("، ", workingDays
                    .Where(w => w.IsOpen)
                    .Select(w => $"{w.DayName}: {w.Hours}"));

                if (string.IsNullOrEmpty(workingHoursText))
                {
                    workingHoursText = "تماس بگیرید";
                }

                // دریافت تماس‌های اضطراری برای نمایش در Contact Section
                var emergencyContactsResult = await _emergencyContactService.GetActiveContactsAsync();
                var emergencyContacts = emergencyContactsResult.Success && emergencyContactsResult.Data != null 
                    ? emergencyContactsResult.Data 
                    : new List<ClinicApp.ViewModels.CMS.EmergencyContactPublicViewModel>();

                return new ContactSectionViewModel
                {
                    SectionTitle = "تماس با ما",
                    ClinicInfo = new ClinicInfoViewModel
                    {
                        Name = clinic?.Name ?? "کلینیک شفا",
                        Address = clinic?.Address ?? "آدرس کلینیک",
                        PhoneNumber = clinic?.PhoneNumber ?? "034-3222-1234",
                        Email = "info@clinic.com", // TODO: اضافه کردن Email به Clinic entity
                        WorkingHours = workingHoursText,
                        WorkingDays = workingDays
                    },
                    GoogleMapsEmbedUrl = "https://www.google.com/maps/embed?pb=...",
                    GoogleMapsLink = "https://www.google.com/maps?q=...",
                    WhatsAppNumber = "09022487373",
                    WhatsAppLink = "https://wa.me/989123456789",
                    EmergencyContacts = emergencyContacts
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Contact Section");
                return new ContactSectionViewModel
                {
                    SectionTitle = "تماس با ما",
                    ClinicInfo = new ClinicInfoViewModel
                    {
                        Name = "کلینیک شفا",
                        PhoneNumber = "034-3222-1234"
                    }
                };
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Announcements
        /// </summary>
        private async Task<List<ClinicApp.ViewModels.CMS.AnnouncementIndexViewModel>> GetAnnouncementsSectionAsync(int count = 5)
        {
            try
            {
                var result = await _announcementService.GetImportantAnnouncementsAsync(count);
                return result.Success && result.Data != null ? result.Data : new List<ClinicApp.ViewModels.CMS.AnnouncementIndexViewModel>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Announcements Section");
                return new List<ClinicApp.ViewModels.CMS.AnnouncementIndexViewModel>();
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش FAQs
        /// </summary>
        private async Task<List<ClinicApp.ViewModels.CMS.FAQPublicViewModel>> GetFAQsSectionAsync(int count = 5)
        {
            try
            {
                var result = await _faqService.GetFeaturedFAQsAsync(count);
                return result.Success && result.Data != null ? result.Data : new List<ClinicApp.ViewModels.CMS.FAQPublicViewModel>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های FAQs Section");
                return new List<ClinicApp.ViewModels.CMS.FAQPublicViewModel>();
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Health Tips
        /// </summary>
        private async Task<List<ClinicApp.ViewModels.CMS.HealthTipPublicViewModel>> GetHealthTipsSectionAsync(int count = 6)
        {
            try
            {
                var result = await _healthTipService.GetFeaturedHealthTipsAsync(count);
                return result.Success && result.Data != null ? result.Data : new List<ClinicApp.ViewModels.CMS.HealthTipPublicViewModel>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Health Tips Section");
                return new List<ClinicApp.ViewModels.CMS.HealthTipPublicViewModel>();
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Insurance Info
        /// </summary>
        private async Task<List<ClinicApp.ViewModels.CMS.InsuranceInfoPublicViewModel>> GetInsuranceInfosSectionAsync(int count = 8)
        {
            try
            {
                var result = await _insuranceInfoService.GetFeaturedInsuranceInfosAsync(count);
                return result.Success && result.Data != null ? result.Data : new List<ClinicApp.ViewModels.CMS.InsuranceInfoPublicViewModel>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Insurance Info Section");
                return new List<ClinicApp.ViewModels.CMS.InsuranceInfoPublicViewModel>();
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Medical Service Info
        /// </summary>
        private async Task<List<ClinicApp.ViewModels.CMS.MedicalServiceInfoPublicViewModel>> GetMedicalServiceInfosSectionAsync(int count = 6)
        {
            try
            {
                var result = await _medicalServiceInfoService.GetFeaturedServiceInfosAsync(count);
                return result.Success && result.Data != null ? result.Data : new List<ClinicApp.ViewModels.CMS.MedicalServiceInfoPublicViewModel>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Medical Service Info Section");
                return new List<ClinicApp.ViewModels.CMS.MedicalServiceInfoPublicViewModel>();
            }
        }

        /// <summary>
        /// دریافت داده‌های بخش Emergency Contacts
        /// </summary>
        private async Task<List<ClinicApp.ViewModels.CMS.EmergencyContactPublicViewModel>> GetEmergencyContactsSectionAsync()
        {
            try
            {
                var result = await _emergencyContactService.GetActiveContactsAsync();
                return result.Success && result.Data != null ? result.Data : new List<ClinicApp.ViewModels.CMS.EmergencyContactPublicViewModel>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Emergency Contacts Section");
                return new List<ClinicApp.ViewModels.CMS.EmergencyContactPublicViewModel>();
            }
        }

        /// <summary>
        /// دریافت اسلایدرهای Sidebar
        /// </summary>
        private async Task<List<ClinicApp.ViewModels.CMS.SliderIndexViewModel>> GetSidebarSlidersAsync()
        {
            try
            {
                var sliders = await _sliderRepository.GetActiveSlidersAsync("sidebar");
                return sliders.Select(s => new ClinicApp.ViewModels.CMS.SliderIndexViewModel
                {
                    SliderId = s.SliderId,
                    Title = s.Title,
                    Description = s.Description,
                    ImageUrl = s.ImageUrl,
                    ThumbnailUrl = s.ThumbnailUrl,
                    LinkUrl = s.LinkUrl,
                    ButtonText = s.ButtonText,
                    IsActive = s.IsActive,
                    DisplayOrder = s.DisplayOrder,
                    Position = s.Position,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Sidebar Sliders");
                return new List<ClinicApp.ViewModels.CMS.SliderIndexViewModel>();
            }
        }

        /// <summary>
        /// دریافت اسلایدرهای Footer
        /// </summary>
        private async Task<List<SliderIndexViewModel>> GetFooterSlidersAsync()
        {
            try
            {
                var sliders = await _sliderRepository.GetActiveSlidersAsync("footer");
                return sliders.Select(s => new SliderIndexViewModel
                {
                    SliderId = s.SliderId,
                    Title = s.Title,
                    Description = s.Description,
                    ImageUrl = s.ImageUrl,
                    ThumbnailUrl = s.ThumbnailUrl,
                    LinkUrl = s.LinkUrl,
                    ButtonText = s.ButtonText,
                    IsActive = s.IsActive,
                    DisplayOrder = s.DisplayOrder,
                    Position = s.Position,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Footer Sliders");
                return new List<ClinicApp.ViewModels.CMS.SliderIndexViewModel>();
            }
        }

        /// <summary>
        /// دریافت داده‌های Sidebar برای صفحه اصلی
        /// </summary>
        private async Task<SidebarViewModel> GetSidebarDataAsync(
            int clinicId,
            Task<QuickAppointmentViewModel> quickAppointmentTask,
            Task<ContactSectionViewModel> contactTask,
            Task<List<ClinicApp.ViewModels.CMS.EmergencyContactPublicViewModel>> emergencyContactsTask,
            Task<List<ClinicApp.ViewModels.CMS.HealthTipPublicViewModel>> healthTipsTask,
            Task<List<ClinicApp.ViewModels.CMS.AnnouncementIndexViewModel>> announcementsTask,
            Task<List<ClinicApp.ViewModels.CMS.SliderIndexViewModel>> sidebarSlidersTask)
        {
            try
            {
                // انتظار برای تمام Task ها
                var quickAppointment = await quickAppointmentTask;
                var contact = await contactTask;
                var emergencyContacts = await emergencyContactsTask;
                var healthTips = await healthTipsTask;
                var announcements = await announcementsTask;
                var sidebarSliders = await sidebarSlidersTask;

                // دریافت ساعات کاری
                var workingHoursResult = await _clinicWorkingHoursService.GetActiveWorkingHoursAsync(clinicId);
                var workingDays = new List<WorkingDayViewModel>();
                bool isOpenNow = false;
                string currentStatus = "بسته";

                if (workingHoursResult.Success && workingHoursResult.Data != null && workingHoursResult.Data.Any())
                {
                    workingDays = workingHoursResult.Data
                        .OrderBy(w => w.DayOfWeek)
                        .Select(w => new WorkingDayViewModel
                        {
                            DayName = w.DayName,
                            Hours = w.TimeRange,
                            IsOpen = w.IsOpen
                        }).ToList();

                    // بررسی وضعیت فعلی (باز/بسته)
                    var now = DateTime.Now;
                    // تبدیل DayOfWeek: Sunday=0 در C# به شنبه=0 در سیستم ما
                    var currentDayOfWeek = (int)now.DayOfWeek;
                    // تبدیل: Sunday(0) -> شنبه(0), Monday(1) -> یکشنبه(1), ..., Saturday(6) -> جمعه(6)
                    // در سیستم ما: شنبه=0, یکشنبه=1, ..., جمعه=6
                    // در C#: Sunday=0, Monday=1, ..., Saturday=6
                    // پس باید یک روز جابجا کنیم: currentDayOfWeek + 1 و سپس mod 7
                    var persianDayOfWeek = (currentDayOfWeek + 1) % 7;
                    var currentWorkingDay = workingHoursResult.Data.FirstOrDefault(w => w.DayOfWeek == persianDayOfWeek);
                    
                    if (currentWorkingDay != null && currentWorkingDay.IsOpen)
                    {
                        var currentTime = now.TimeOfDay;
                        if (currentWorkingDay.StartTime <= currentTime && currentTime <= currentWorkingDay.EndTime)
                        {
                            isOpenNow = true;
                            currentStatus = "باز";
                        }
                    }
                }

                // ساخت Quick Links
                var quickLinks = new List<QuickLinkViewModel>
                {
                    new QuickLinkViewModel
                    {
                        Title = "پزشکان",
                        Icon = "fas fa-user-md",
                        Url = "/Doctors",
                        Description = "لیست پزشکان کلینیک",
                        Order = 1
                    },
                    new QuickLinkViewModel
                    {
                        Title = "خدمات",
                        Icon = "fas fa-stethoscope",
                        Url = "/MedicalServiceInfo",
                        Description = "خدمات درمانی کلینیک",
                        Order = 2
                    },
                    new QuickLinkViewModel
                    {
                        Title = "مقالات",
                        Icon = "fas fa-newspaper",
                        Url = "/Blog",
                        Description = "مقالات و مطالب پزشکی",
                        Order = 3
                    },
                    new QuickLinkViewModel
                    {
                        Title = "گالری",
                        Icon = "fas fa-images",
                        Url = "/Gallery",
                        Description = "گالری تصاویر کلینیک",
                        Order = 4
                    },
                    new QuickLinkViewModel
                    {
                        Title = "سوالات متداول",
                        Icon = "fas fa-question-circle",
                        Url = "/FAQ",
                        Description = "پاسخ به سوالات متداول",
                        Order = 5
                    }
                };

                // ساخت Contact Info
                var contactInfo = new ContactInfoSidebarViewModel
                {
                    PhoneNumber = contact?.ClinicInfo?.PhoneNumber ?? "034-3222-1234",
                    PhoneLink = $"tel:{(contact?.ClinicInfo?.PhoneNumber ?? "03432221234").Replace("-", "").Replace(" ", "")}",
                    Email = contact?.ClinicInfo?.Email ?? "info@clinic.com",
                    EmailLink = $"mailto:{contact?.ClinicInfo?.Email ?? "info@clinic.com"}",
                    Address = contact?.ClinicInfo?.Address ?? "آدرس کلینیک",
                    WhatsAppNumber = contact?.WhatsAppNumber ?? "09022487373",
                    WhatsAppLink = contact?.WhatsAppLink ?? "https://wa.me/989123456789",
                    GoogleMapsLink = contact?.GoogleMapsLink ?? "https://www.google.com/maps"
                };

                // ساخت Quick Appointment
                var quickAppointmentSidebar = new QuickAppointmentSidebarViewModel
                {
                    Title = "رزرو سریع نوبت",
                    Subtitle = "نوبت خود را آنلاین رزرو کنید",
                    ButtonText = "رزرو نوبت",
                    AppointmentUrl = quickAppointment?.AppointmentUrl ?? "/Appointment",
                    Specializations = quickAppointment?.Specializations ?? new List<SpecializationLookupViewModel>()
                };

                // ساخت Working Hours
                var workingHoursSidebar = new WorkingHoursSidebarViewModel
                {
                    Title = "ساعات کاری",
                    WorkingDays = workingDays,
                    IsOpenNow = isOpenNow,
                    CurrentStatus = currentStatus
                };

                return new SidebarViewModel
                {
                    QuickAppointment = quickAppointmentSidebar,
                    QuickLinks = quickLinks,
                    ContactInfo = contactInfo,
                    EmergencyContacts = emergencyContacts?.Take(3).ToList() ?? new List<ClinicApp.ViewModels.CMS.EmergencyContactPublicViewModel>(),
                    HealthTips = healthTips?.Take(3).ToList() ?? new List<ClinicApp.ViewModels.CMS.HealthTipPublicViewModel>(),
                    Announcements = announcements?.Take(3).ToList() ?? new List<ClinicApp.ViewModels.CMS.AnnouncementIndexViewModel>(),
                    Sliders = sidebarSliders ?? new List<ClinicApp.ViewModels.CMS.SliderIndexViewModel>(),
                    WorkingHours = workingHoursSidebar
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Sidebar");
                return new SidebarViewModel();
            }
        }

        /// <summary>
        /// دریافت داده‌های Footer برای صفحه اصلی و تمام صفحات
        /// </summary>
        public async Task<FooterViewModel> GetFooterDataAsync(int? clinicId = null)
        {
            try
            {
                var effectiveClinicId = clinicId ?? 1;
                
                // لود Contact و Emergency Contacts به صورت موازی
                var contactTask = GetContactSectionAsync(effectiveClinicId);
                var emergencyContactsTask = GetEmergencyContactsSectionAsync();
                
                // استفاده از Task ها برای GetFooterDataInternalAsync
                return await GetFooterDataInternalAsync(effectiveClinicId, contactTask, emergencyContactsTask);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Footer");
                return new FooterViewModel();
            }
        }

        /// <summary>
        /// دریافت داده‌های Footer برای صفحه اصلی (Internal)
        /// </summary>
        private async Task<FooterViewModel> GetFooterDataInternalAsync(
            int clinicId,
            Task<ContactSectionViewModel> contactTask,
            Task<List<ClinicApp.ViewModels.CMS.EmergencyContactPublicViewModel>> emergencyContactsTask)
        {
            try
            {
                // انتظار برای Task ها
                var contact = await contactTask;
                var emergencyContacts = await emergencyContactsTask;

                // دریافت اطلاعات کلینیک
                var clinic = await _clinicRepository.GetByIdAsync(clinicId);

                // دریافت ساعات کاری
                var workingHoursResult = await _clinicWorkingHoursService.GetActiveWorkingHoursAsync(clinicId);
                var workingDays = new List<WorkingDayViewModel>();
                bool isOpenNow = false;
                string currentStatus = "بسته";

                if (workingHoursResult.Success && workingHoursResult.Data != null && workingHoursResult.Data.Any())
                {
                    workingDays = workingHoursResult.Data
                        .OrderBy(w => w.DayOfWeek)
                        .Select(w => new WorkingDayViewModel
                        {
                            DayName = w.DayName,
                            Hours = w.TimeRange,
                            IsOpen = w.IsOpen
                        }).ToList();

                    // بررسی وضعیت فعلی (باز/بسته)
                    var now = DateTime.Now;
                    var currentDayOfWeek = (int)now.DayOfWeek;
                    var persianDayOfWeek = (currentDayOfWeek + 1) % 7;
                    var currentWorkingDay = workingHoursResult.Data.FirstOrDefault(w => w.DayOfWeek == persianDayOfWeek);
                    
                    if (currentWorkingDay != null && currentWorkingDay.IsOpen)
                    {
                        var currentTime = now.TimeOfDay;
                        if (currentWorkingDay.StartTime <= currentTime && currentTime <= currentWorkingDay.EndTime)
                        {
                            isOpenNow = true;
                            currentStatus = "باز";
                        }
                    }
                }

                // ساخت Brand Info
                var brandInfo = new BrandInfoFooterViewModel
                {
                    ClinicName = clinic?.Name ?? "کلینیک شفا جیرفت",
                    LogoUrl = "/Content/Images/logo/logoshafa.png",
                    Tagline = "مرکز تخصصی درمان و سلامت — مراقبت معتبر و مبتنی بر شواهد",
                    Description = "ارائه خدمات درمانی تخصصی با استفاده از پیشرفته‌ترین تجهیزات پزشکی و تیم متخصص برای سلامت شما.",
                    HomeUrl = "/"
                };

                // ساخت Contact Info
                var contactInfo = new ContactInfoFooterViewModel
                {
                    PhoneNumber = contact?.ClinicInfo?.PhoneNumber ?? "034-3222-1234",
                    PhoneLink = $"tel:{(contact?.ClinicInfo?.PhoneNumber ?? "03432221234").Replace("-", "").Replace(" ", "")}",
                    EmergencyPhone = emergencyContacts?.FirstOrDefault()?.PhoneNumber ?? "115",
                    EmergencyPhoneLink = emergencyContacts?.FirstOrDefault() != null 
                        ? $"tel:{emergencyContacts.First().PhoneNumber?.Replace("-", "").Replace(" ", "")}" 
                        : "tel:115",
                    Email = contact?.ClinicInfo?.Email ?? "info@clinic.com",
                    EmailLink = $"mailto:{contact?.ClinicInfo?.Email ?? "info@clinic.com"}",
                    Address = contact?.ClinicInfo?.Address ?? "جیرفت، خیابان اصلی، کوچه شفا، پلاک 10",
                    GoogleMapsLink = contact?.GoogleMapsLink ?? "https://www.google.com/maps",
                    WhatsAppNumber = contact?.WhatsAppNumber ?? "09022487373",
                    WhatsAppLink = contact?.WhatsAppLink ?? "https://wa.me/989123456789"
                };

                // ساخت Quick Links
                var quickLinks = new List<FooterLinkViewModel>
                {
                    new FooterLinkViewModel { Title = "خانه", Url = "/", Icon = "fas fa-home", Order = 1 },
                    new FooterLinkViewModel { Title = "درباره ما", Url = "/About", Icon = "fas fa-info-circle", Order = 2 },
                    new FooterLinkViewModel { Title = "پزشکان", Url = "/Doctors", Icon = "fas fa-user-md", Order = 3 },
                    new FooterLinkViewModel { Title = "مقالات", Url = "/Blog", Icon = "fas fa-newspaper", Order = 4 },
                    new FooterLinkViewModel { Title = "تماس با ما", Url = "/Contact", Icon = "fas fa-envelope", Order = 5 },
                    new FooterLinkViewModel { Title = "پیگیری پیام", Url = "/Contact/Track", Icon = "fas fa-search", Order = 6 },
                    new FooterLinkViewModel { Title = "سوالات متداول", Url = "/FAQ", Icon = "fas fa-question-circle", Order = 7 }
                };

                // ساخت Service Links
                var serviceLinks = new List<FooterLinkViewModel>
                {
                    new FooterLinkViewModel { Title = "خدمات درمانی", Url = "/MedicalServiceInfo", Icon = "fas fa-stethoscope", Order = 1 },
                    new FooterLinkViewModel { Title = "نوبت‌دهی", Url = "/Appointment", Icon = "fas fa-calendar-check", Order = 2 },
                    new FooterLinkViewModel { Title = "آزمایشگاه", Url = "/MedicalServiceInfo", Icon = "fas fa-flask", Order = 3 },
                    new FooterLinkViewModel { Title = "رادیولوژی", Url = "/MedicalServiceInfo", Icon = "fas fa-x-ray", Order = 4 }
                };

                // ساخت Legal Info
                var currentYear = DateTime.Now.Year;
                var legalInfo = new LegalInfoFooterViewModel
                {
                    CopyrightText = $"© {currentYear} کلینیک شفا جیرفت. تمامی حقوق محفوظ است.",
                    CurrentYear = currentYear,
                    PrivacyPolicyUrl = "/Privacy",
                    TermsOfServiceUrl = "/Terms",
                    ComplaintsUrl = "/Complaints",
                    MedicalPrivacyNotice = "اطلاعات پزشکی بیماران به صورت محرمانه نگهداری می‌شود و طبق قوانین حریم خصوصی و امنیت اطلاعات درمانی محافظت می‌گردد."
                };

                // ساخت Certifications
                var certifications = new List<CertificationViewModel>
                {
                    new CertificationViewModel
                    {
                        Title = "مجوز وزارت بهداشت",
                        Description = "دارای مجوز رسمی از وزارت بهداشت، درمان و آموزش پزشکی",
                        LicenseNumber = "12345",
                        Order = 1
                    },
                    new CertificationViewModel
                    {
                        Title = "نماد اعتماد",
                        Description = "دارای نماد اعتماد الکترونیکی",
                        Order = 2
                    }
                };

                // ساخت Social Media
                var socialMedia = new List<SocialMediaViewModel>
                {
                    new SocialMediaViewModel
                    {
                        Platform = "Instagram",
                        Url = "https://www.instagram.com/shafa_jiroft",
                        Icon = "fab fa-instagram",
                        AriaLabel = "اینستاگرام کلینیک شفا",
                        Order = 1
                    },
                    new SocialMediaViewModel
                    {
                        Platform = "Telegram",
                        Url = "https://www.telegram.me/shafa_jiroft",
                        Icon = "fab fa-telegram",
                        AriaLabel = "تلگرام کلینیک شفا",
                        Order = 2
                    },
                    new SocialMediaViewModel
                    {
                        Platform = "WhatsApp",
                        Url = contact?.WhatsAppLink ?? "https://wa.me/989123456789",
                        Icon = "fab fa-whatsapp",
                        AriaLabel = "واتساپ کلینیک شفا",
                        Order = 3
                    }
                };

                // ساخت Working Hours
                var workingHours = new WorkingHoursFooterViewModel
                {
                    Title = "ساعات کاری",
                    WorkingDays = workingDays,
                    IsOpenNow = isOpenNow,
                    CurrentStatus = currentStatus
                };

                return new FooterViewModel
                {
                    BrandInfo = brandInfo,
                    ContactInfo = contactInfo,
                    QuickLinks = quickLinks,
                    ServiceLinks = serviceLinks,
                    LegalInfo = legalInfo,
                    Certifications = certifications,
                    SocialMedia = socialMedia,
                    WorkingHours = workingHours
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Footer");
                return new FooterViewModel();
            }
        }

        #region Helper Methods

        /// <summary>
        /// دریافت حروف اول نام برای نمایش در آواتار
        /// </summary>
        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "?";

            var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return parts[0].Substring(0, Math.Min(1, parts[0].Length));
            }
            return name.Substring(0, Math.Min(1, name.Length));
        }

        /// <summary>
        /// دریافت آیکون مناسب برای هر خدمت بر اساس دسته‌بندی
        /// </summary>
        private string GetServiceIcon(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return "fas fa-stethoscope";

            var categoryLower = categoryName.ToLower();
            
            if (categoryLower.Contains("دندان"))
                return "fas fa-tooth";
            if (categoryLower.Contains("چشم"))
                return "fas fa-eye";
            if (categoryLower.Contains("قلب"))
                return "fas fa-heartbeat";
            if (categoryLower.Contains("کودک"))
                return "fas fa-baby";
            if (categoryLower.Contains("زنان"))
                return "fas fa-venus";
            if (categoryLower.Contains("مردان"))
                return "fas fa-mars";
            
            return "fas fa-stethoscope";
        }

        /// <summary>
        /// دریافت داده‌های بخش تجهیزات پزشکی
        /// </summary>
        private async Task<List<MedicalEquipmentPublicViewModel>> GetMedicalEquipmentsSectionAsync(int count = 6)
        {
            try
            {
                var result = await _medicalEquipmentService.GetFeaturedEquipmentsAsync(count);
                if (result.Success && result.Data != null)
                {
                    return result.Data;
                }
                return new List<MedicalEquipmentPublicViewModel>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تجهیزات پزشکی برای صفحه اصلی");
                return new List<MedicalEquipmentPublicViewModel>();
            }
        }

        /// <summary>
        /// دریافت داده‌های صفحه "درباره ما" - Production-Grade
        /// طبق استانداردهای کلینیک درمانی
        /// اولویت: داده‌های CMS > داده‌های پیش‌فرض
        /// </summary>
        public async Task<AboutPageViewModel> GetAboutPageDataAsync(int? clinicId = null)
        {
            try
            {
                var effectiveClinicId = clinicId ?? 1; // کلینیک پیش‌فرض: شفا
                _logger.Information("دریافت داده‌های صفحه About - ClinicId: {ClinicId}", effectiveClinicId);

                // تلاش برای دریافت داده‌های CMS
                AboutPagePublicViewModel cmsData = null;
                if (_aboutPageService != null)
                {
                    try
                    {
                        var cmsResult = await _aboutPageService.GetActiveAboutPageAsync();
                        if (cmsResult.Success && cmsResult.Data != null)
                        {
                            cmsData = cmsResult.Data;
                            _logger.Information("داده‌های CMS برای صفحه About یافت شد - AboutPageId: {AboutPageId}", cmsData.AboutPageId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "خطا در دریافت داده‌های CMS برای صفحه About - استفاده از داده‌های پیش‌فرض");
                    }
                }

                // اگر داده‌های CMS موجود باشد، از آن استفاده کن
                if (cmsData != null)
                {
                    // لود موازی داده‌های دینامیک (پزشکان، تخصص‌ها، تجهیزات)
                    var cmsDoctorsTask = _context.Doctors
                        .AsNoTracking()
                        .Where(d => !d.IsDeleted && d.IsActive && (d.ClinicId == effectiveClinicId || effectiveClinicId == 0))
                        .Include(d => d.DoctorSpecializations.Select(ds => ds.Specialization))
                        .ToListAsync();
                    var cmsEquipmentsTask = _medicalEquipmentService.GetActiveEquipmentsAsync();

                    await Task.WhenAll(cmsDoctorsTask, cmsEquipmentsTask);

                    var cmsDoctors = await cmsDoctorsTask;
                    var cmsEquipmentsResult = await cmsEquipmentsTask;

                    // محاسبه داده‌های دینامیک
                    var cmsDoctorCount = cmsDoctors.Count;
                    var cmsSpecializationGroups = cmsDoctors
                        .SelectMany(d => d.DoctorSpecializations ?? new List<DoctorSpecialization>())
                        .Where(ds => ds.Specialization != null && !ds.Specialization.IsDeleted)
                        .GroupBy(ds => ds.Specialization.Name)
                        .Select(g => new SpecializationSummaryViewModel
                        {
                            Name = g.Key,
                            DoctorCount = g.Count()
                        })
                        .OrderByDescending(s => s.DoctorCount)
                        .Take(6)
                        .ToList();

                    var cmsEquipmentCount = cmsEquipmentsResult.Success && cmsEquipmentsResult.Data != null ? cmsEquipmentsResult.Data.Count : 0;
                    var cmsEquipmentCategories = cmsEquipmentsResult.Success && cmsEquipmentsResult.Data != null
                        ? cmsEquipmentsResult.Data
                            .GroupBy(e => e.Category ?? "عمومی")
                            .Select(g => new EquipmentCategoryViewModel
                            {
                                CategoryName = g.Key,
                                EquipmentCount = g.Count()
                            })
                            .Take(4)
                            .ToList()
                        : new List<EquipmentCategoryViewModel>();

                    return new AboutPageViewModel
                    {
                        ClinicName = cmsData.ClinicName,
                        ClinicDescription = cmsData.ClinicDescription,
                        EstablishedYear = cmsData.EstablishedYear,
                        MissionValues = cmsData.MissionValues ?? new List<MissionValueViewModel>(),
                        Licenses = cmsData.Licenses ?? new List<LicenseViewModel>(),
                        RegulatoryBody = cmsData.RegulatoryBody,
                        DoctorCount = cmsDoctorCount,
                        Specializations = cmsSpecializationGroups,
                        MedicalTeamDescription = cmsData.MedicalTeamDescription,
                        EquipmentCount = cmsEquipmentCount,
                        EquipmentCategories = cmsEquipmentCategories,
                        InfrastructureDescription = cmsData.InfrastructureDescription,
                        EthicalCommitments = cmsData.EthicalCommitments ?? new List<EthicalCommitmentViewModel>()
                    };
                }

                // Fallback: استفاده از داده‌های پیش‌فرض (کد قبلی)
                var clinicTask = _clinicRepository.GetByIdAsync(effectiveClinicId);
                var doctorsTask = _context.Doctors
                    .AsNoTracking()
                    .Where(d => !d.IsDeleted && d.IsActive && (d.ClinicId == effectiveClinicId || effectiveClinicId == 0))
                    .Include(d => d.DoctorSpecializations.Select(ds => ds.Specialization))
                    .ToListAsync();
                var specializationsTask = _context.Specializations
                    .AsNoTracking()
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .ToListAsync();
                var equipmentsTask = _medicalEquipmentService.GetActiveEquipmentsAsync();

                await Task.WhenAll(clinicTask, doctorsTask, specializationsTask, equipmentsTask);

                var clinic = await clinicTask;
                var doctors = await doctorsTask;
                var specializations = await specializationsTask;
                var equipmentsResult = await equipmentsTask;

                // 1. معرفی کلینیک
                var clinicName = clinic?.Name ?? "کلینیک درمانی شفا";
                var clinicDescription = "کلینیک درمانی شفا با بهره‌گیری از کادر درمان مجرب و تجهیزات پزشکی به‌روز، خدمات تشخیصی و درمانی را در محیطی ایمن و حرفه‌ای ارائه می‌دهد.";
                var establishedYear = clinic?.CreatedAt.Year.ToString() ?? "1400";

                // 2. مأموریت و رویکرد درمانی
                var missionValues = new List<MissionValueViewModel>
                {
                    new MissionValueViewModel
                    {
                        Title = "رویکرد بیمارمحور",
                        Description = "تمرکز بر احترام، شفافیت و آرامش مراجعین",
                        Icon = "fas fa-heart"
                    },
                    new MissionValueViewModel
                    {
                        Title = "تشخیص دقیق و علمی",
                        Description = "استفاده از استانداردهای روز پزشکی",
                        Icon = "fas fa-stethoscope"
                    },
                    new MissionValueViewModel
                    {
                        Title = "رعایت اصول اخلاق پزشکی",
                        Description = "حفظ محرمانگی اطلاعات بیماران",
                        Icon = "fas fa-shield-alt"
                    }
                };

                // 3. مجوزها و اعتبارها
                var licenses = new List<LicenseViewModel>
                {
                    new LicenseViewModel
                    {
                        Title = "مجوز فعالیت",
                        IssuingAuthority = "وزارت بهداشت، درمان و آموزش پزشکی",
                        LicenseNumber = "مشخص می‌شود",
                        ValidUntil = "در حال اعتبار"
                    }
                };
                var regulatoryBody = "فعالیت تحت نظارت وزارت بهداشت، درمان و آموزش پزشکی";

                // 4. کادر درمان و تخصص‌ها
                var doctorCount = doctors.Count;
                var specializationGroups = doctors
                    .SelectMany(d => d.DoctorSpecializations ?? new List<DoctorSpecialization>())
                    .Where(ds => ds.Specialization != null && !ds.Specialization.IsDeleted)
                    .GroupBy(ds => ds.Specialization.Name)
                    .Select(g => new SpecializationSummaryViewModel
                    {
                        Name = g.Key,
                        DoctorCount = g.Count()
                    })
                    .OrderByDescending(s => s.DoctorCount)
                    .Take(6)
                    .ToList();

                var medicalTeamDescription = $"همکاری با {doctorCount} پزشک و کادر درمانی مجرب در حوزه‌های {string.Join("، ", specializationGroups.Take(3).Select(s => s.Name))} و خدمات عمومی";

                // 5. تجهیزات و زیرساخت‌ها
                var equipmentCount = equipmentsResult.Success && equipmentsResult.Data != null ? equipmentsResult.Data.Count : 0;
                var equipmentCategories = equipmentsResult.Success && equipmentsResult.Data != null
                    ? equipmentsResult.Data
                        .GroupBy(e => e.Category ?? "عمومی")
                        .Select(g => new EquipmentCategoryViewModel
                        {
                            CategoryName = g.Key,
                            EquipmentCount = g.Count()
                        })
                        .Take(4)
                        .ToList()
                    : new List<EquipmentCategoryViewModel>();

                var infrastructureDescription = "استفاده از تجهیزات تشخیصی مدرن از جمله سیستم‌های پیشرفته تصویربرداری و پایش قلب";

                // 6. تعهد به اخلاق پزشکی
                var ethicalCommitments = new List<EthicalCommitmentViewModel>
                {
                    new EthicalCommitmentViewModel
                    {
                        Title = "حفظ حریم خصوصی",
                        Description = "اطلاعات بیماران به صورت محرمانه نگهداری می‌شود",
                        Icon = "fas fa-lock"
                    },
                    new EthicalCommitmentViewModel
                    {
                        Title = "امنیت اطلاعات",
                        Description = "رعایت استانداردهای امنیتی برای حفاظت از داده‌های پزشکی",
                        Icon = "fas fa-shield-alt"
                    },
                    new EthicalCommitmentViewModel
                    {
                        Title = "عدم افشای داده پزشکی",
                        Description = "اطلاعات پزشکی بیماران بدون رضایت صریح افشا نمی‌شود",
                        Icon = "fas fa-user-secret"
                    }
                };

                return new AboutPageViewModel
                {
                    ClinicName = clinicName,
                    ClinicDescription = clinicDescription,
                    EstablishedYear = establishedYear,
                    MissionValues = missionValues,
                    Licenses = licenses,
                    RegulatoryBody = regulatoryBody,
                    DoctorCount = doctorCount,
                    Specializations = specializationGroups,
                    MedicalTeamDescription = medicalTeamDescription,
                    EquipmentCount = equipmentCount,
                    EquipmentCategories = equipmentCategories,
                    InfrastructureDescription = infrastructureDescription,
                    EthicalCommitments = ethicalCommitments
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های صفحه About");
                return new AboutPageViewModel
                {
                    ClinicName = "کلینیک درمانی شفا",
                    ClinicDescription = "کلینیک درمانی شفا با بهره‌گیری از کادر درمان مجرب و تجهیزات پزشکی به‌روز، خدمات تشخیصی و درمانی را در محیطی ایمن و حرفه‌ای ارائه می‌دهد.",
                    MissionValues = new List<MissionValueViewModel>(),
                    Licenses = new List<LicenseViewModel>(),
                    Specializations = new List<SpecializationSummaryViewModel>(),
                    EquipmentCategories = new List<EquipmentCategoryViewModel>(),
                    EthicalCommitments = new List<EthicalCommitmentViewModel>()
                };
            }
        }

        #endregion
    }
}

