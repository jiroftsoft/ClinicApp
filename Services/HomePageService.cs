using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.CMS;
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
            IEmergencyContactService emergencyContactService)
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
                var healthTipsTask = GetHealthTipsSectionAsync(6);
                var insuranceInfosTask = GetInsuranceInfosSectionAsync(8);
                var medicalServiceInfosTask = GetMedicalServiceInfosSectionAsync(6);
                var emergencyContactsTask = GetEmergencyContactsSectionAsync();

                // انتظار برای تمام Task ها
                await Task.WhenAll(
                    heroTask, valuePropTask, servicesTask, doctorsTask, quickAppointmentTask,
                    testimonialsTask, galleryTask, blogTask, videosTask, contactTask,
                    medicalEquipmentsTask, announcementsTask, faqsTask, healthTipsTask,
                    insuranceInfosTask, medicalServiceInfosTask, emergencyContactsTask);

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
                    EmergencyContacts = await emergencyContactsTask
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

                // دریافت اسلایدرهای فعال برای Hero
                var heroSliders = await _sliderRepository.GetActiveSlidersAsync("hero");
                var heroSlider = heroSliders.FirstOrDefault();

                return new HeroSectionViewModel
                {
                    Title = heroSlider?.Title ?? (clinic?.Name ?? "کلینیک درمانی مدرن"),
                    Subtitle = heroSlider?.Description ?? "همراه شما در مسیر سلامت",
                    BackgroundImageUrl = heroSlider?.ImageUrl ?? "/Content/Images/clinic-hero.jpg",
                    BackgroundVideoUrl = null, // TODO: اضافه کردن ویدیو در صورت نیاز
                    PrimaryButtonText = heroSlider?.ButtonText ?? "رزرو نوبت آنلاین",
                    PrimaryButtonUrl = heroSlider?.LinkUrl ?? "/Patient/Appointment/Index",
                    SecondaryButtonText = "مشاوره آنلاین",
                    SecondaryButtonUrl = "/Patient/Appointment/Consultation",
                    Statistics = new List<StatisticItemViewModel>
                    {
                        new StatisticItemViewModel { Icon = "fas fa-user-md", Label = "پزشک متخصص", Value = "45+" },
                        new StatisticItemViewModel { Icon = "fas fa-users", Label = "بیمار راضی", Value = "15,000+" },
                        new StatisticItemViewModel { Icon = "fas fa-headset", Label = "پشتیبانی", Value = "24/7" }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های Hero Section");
                // بازگرداندن داده‌های پیش‌فرض در صورت خطا
                return new HeroSectionViewModel
                {
                    Title = "کلینیک درمانی مدرن",
                    Subtitle = "همراه شما در مسیر سلامت",
                    PrimaryButtonText = "رزرو نوبت آنلاین",
                    PrimaryButtonUrl = "/Patient/Appointment/Index"
                };
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

        #endregion
    }
}

