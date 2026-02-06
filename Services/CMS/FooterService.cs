using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Interfaces;
using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس فوتر: ترکیب تنظیمات، لینک‌ها، شبکه‌ها و مجوزها برای نمایش در سایت
    /// </summary>
    public class FooterService : IFooterService
    {
        private readonly IFooterSettingsRepository _footerSettingsRepository;
        private readonly IFooterLinkRepository _footerLinkRepository;
        private readonly IFooterSocialRepository _footerSocialRepository;
        private readonly IFooterCertificationRepository _footerCertificationRepository;
        private readonly IClinicWorkingHoursService _clinicWorkingHoursService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        private const byte LinkTypeQuick = 1;
        private const byte LinkTypeService = 2;

        public FooterService(
            IFooterSettingsRepository footerSettingsRepository,
            IFooterLinkRepository footerLinkRepository,
            IFooterSocialRepository footerSocialRepository,
            IFooterCertificationRepository footerCertificationRepository,
            IClinicWorkingHoursService clinicWorkingHoursService,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _footerSettingsRepository = footerSettingsRepository ?? throw new ArgumentNullException(nameof(footerSettingsRepository));
            _footerLinkRepository = footerLinkRepository ?? throw new ArgumentNullException(nameof(footerLinkRepository));
            _footerSocialRepository = footerSocialRepository ?? throw new ArgumentNullException(nameof(footerSocialRepository));
            _footerCertificationRepository = footerCertificationRepository ?? throw new ArgumentNullException(nameof(footerCertificationRepository));
            _clinicWorkingHoursService = clinicWorkingHoursService ?? throw new ArgumentNullException(nameof(clinicWorkingHoursService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FooterViewModel> GetPublicFooterAsync(int? clinicId = null)
        {
            try
            {
                var settings = await _footerSettingsRepository.GetByClinicAsync(clinicId);
                if (settings == null)
                    return null;

                var quickLinksTask = _footerLinkRepository.GetActiveByTypeAsync(LinkTypeQuick, clinicId);
                var serviceLinksTask = _footerLinkRepository.GetActiveByTypeAsync(LinkTypeService, clinicId);
                var socialTask = _footerSocialRepository.GetActiveAsync(clinicId);
                var certTask = _footerCertificationRepository.GetActiveAsync(clinicId);
                var workingHoursTask = _clinicWorkingHoursService.GetActiveWorkingHoursAsync(clinicId);

                await Task.WhenAll(quickLinksTask, serviceLinksTask, socialTask, certTask, workingHoursTask);

                var quickLinks = await quickLinksTask;
                var serviceLinks = await serviceLinksTask;
                var socials = await socialTask;
                var certs = await certTask;
                var workingHoursResult = await workingHoursTask;

                var workingDays = new List<WorkingDayViewModel>();
                var isOpenNow = false;
                var currentStatus = "بسته";

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

                var phoneClean = NormalizePhone(settings.ContactPhone);
                var emergencyClean = NormalizePhone(settings.ContactEmergencyPhone);
                var whatsAppClean = NormalizePhone(settings.ContactWhatsAppNumber);

                return new FooterViewModel
                {
                    BrandInfo = new BrandInfoFooterViewModel
                    {
                        ClinicName = settings.BrandClinicName ?? "کلینیک",
                        LogoUrl = settings.BrandLogoUrl,
                        Tagline = settings.BrandTagline,
                        Description = settings.BrandDescription,
                        HomeUrl = settings.BrandHomeUrl ?? "/"
                    },
                    ContactInfo = new ContactInfoFooterViewModel
                    {
                        PhoneNumber = settings.ContactPhone,
                        PhoneLink = string.IsNullOrEmpty(phoneClean) ? null : "tel:" + phoneClean,
                        EmergencyPhone = settings.ContactEmergencyPhone,
                        EmergencyPhoneLink = string.IsNullOrEmpty(emergencyClean) ? null : "tel:" + emergencyClean,
                        Email = settings.ContactEmail,
                        EmailLink = string.IsNullOrEmpty(settings.ContactEmail) ? null : "mailto:" + settings.ContactEmail,
                        Address = settings.ContactAddress,
                        GoogleMapsLink = null,
                        WhatsAppNumber = settings.ContactWhatsAppNumber,
                        WhatsAppLink = string.IsNullOrEmpty(whatsAppClean) ? null : "https://wa.me/" + whatsAppClean
                    },
                    QuickLinks = quickLinks.Select(f => new FooterLinkViewModel
                    {
                        Title = f.Title,
                        Url = f.Url,
                        Icon = f.Icon,
                        IsExternal = f.IsExternal,
                        Order = f.DisplayOrder
                    }).OrderBy(f => f.Order).ToList(),
                    ServiceLinks = serviceLinks.Select(f => new FooterLinkViewModel
                    {
                        Title = f.Title,
                        Url = f.Url,
                        Icon = f.Icon,
                        IsExternal = f.IsExternal,
                        Order = f.DisplayOrder
                    }).OrderBy(f => f.Order).ToList(),
                    SocialMedia = socials.Select(s => new SocialMediaViewModel
                    {
                        Platform = s.Platform,
                        Url = s.Url,
                        Icon = s.Icon,
                        AriaLabel = s.AriaLabel,
                        Order = s.DisplayOrder
                    }).OrderBy(s => s.Order).ToList(),
                    Certifications = certs.Select(c => new CertificationViewModel
                    {
                        Title = c.Title,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        LinkUrl = c.LinkUrl,
                        LicenseNumber = c.LicenseNumber,
                        Order = c.DisplayOrder
                    }).OrderBy(c => c.Order).ToList(),
                    LegalInfo = new LegalInfoFooterViewModel
                    {
                        CopyrightText = settings.LegalCopyrightText,
                        CurrentYear = DateTime.Now.Year,
                        PrivacyPolicyUrl = settings.LegalPrivacyPolicyUrl,
                        TermsOfServiceUrl = settings.LegalTermsOfServiceUrl,
                        ComplaintsUrl = settings.LegalComplaintsUrl,
                        MedicalPrivacyNotice = settings.LegalMedicalPrivacyNotice
                    },
                    WorkingHours = new WorkingHoursFooterViewModel
                    {
                        Title = settings.WorkingHoursTitle ?? "ساعات کاری",
                        WorkingDays = workingDays,
                        IsOpenNow = isOpenNow,
                        CurrentStatus = currentStatus
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های فوتر از CMS - ClinicId: {ClinicId}", clinicId);
                return null;
            }
        }

        public async Task<ServiceResult<FooterSettingsEditViewModel>> GetSettingsForEditAsync(int? clinicId = null)
        {
            try
            {
                var settings = await _footerSettingsRepository.GetByClinicAsync(clinicId);
                if (settings == null)
                {
                    // مدل پیش‌فرض برای اولین بار
                    return ServiceResult<FooterSettingsEditViewModel>.Successful(new FooterSettingsEditViewModel
                    {
                        BrandClinicName = "کلینیک شفا جیرفت",
                        BrandHomeUrl = "/",
                        WorkingHoursTitle = "ساعات کاری",
                        IsActive = true
                    });
                }

                return ServiceResult<FooterSettingsEditViewModel>.Successful(new FooterSettingsEditViewModel
                {
                    FooterSettingsId = settings.FooterSettingsId,
                    BrandClinicName = settings.BrandClinicName,
                    BrandLogoUrl = settings.BrandLogoUrl,
                    BrandTagline = settings.BrandTagline,
                    BrandDescription = settings.BrandDescription,
                    BrandHomeUrl = settings.BrandHomeUrl,
                    ContactPhone = settings.ContactPhone,
                    ContactEmergencyPhone = settings.ContactEmergencyPhone,
                    ContactEmail = settings.ContactEmail,
                    ContactAddress = settings.ContactAddress,
                    ContactWhatsAppNumber = settings.ContactWhatsAppNumber,
                    LegalCopyrightText = settings.LegalCopyrightText,
                    LegalPrivacyPolicyUrl = settings.LegalPrivacyPolicyUrl,
                    LegalTermsOfServiceUrl = settings.LegalTermsOfServiceUrl,
                    LegalComplaintsUrl = settings.LegalComplaintsUrl,
                    LegalMedicalPrivacyNotice = settings.LegalMedicalPrivacyNotice,
                    WorkingHoursTitle = settings.WorkingHoursTitle,
                    IsActive = settings.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تنظیمات فوتر برای ویرایش - ClinicId: {ClinicId}", clinicId);
                return ServiceResult<FooterSettingsEditViewModel>.Failed("خطا در بارگذاری تنظیمات فوتر");
            }
        }

        public async Task<ServiceResult> SaveSettingsAsync(FooterSettingsEditViewModel model, int? clinicId = null)
        {
            try
            {
                if (model == null)
                    return ServiceResult.Failed("اطلاعات معتبر نیست.");

                FooterSettings entity = null;
                if (model.FooterSettingsId > 0)
                {
                    entity = await _footerSettingsRepository.GetByIdAsync(model.FooterSettingsId);
                }

                var now = DateTime.Now;
                var userId = _currentUserService.UserId;

                if (entity == null)
                {
                    entity = new FooterSettings
                    {
                        ClinicId = clinicId,
                        CreatedAt = now,
                        CreatedByUserId = userId
                    };
                    _footerSettingsRepository.Add(entity);
                }
                else
                {
                    entity.UpdatedAt = now;
                    entity.UpdatedByUserId = userId;
                }

                entity.BrandClinicName = model.BrandClinicName;
                entity.BrandLogoUrl = model.BrandLogoUrl;
                entity.BrandTagline = model.BrandTagline;
                entity.BrandDescription = model.BrandDescription;
                entity.BrandHomeUrl = model.BrandHomeUrl;
                entity.ContactPhone = model.ContactPhone;
                entity.ContactEmergencyPhone = model.ContactEmergencyPhone;
                entity.ContactEmail = model.ContactEmail;
                entity.ContactAddress = model.ContactAddress;
                entity.ContactWhatsAppNumber = model.ContactWhatsAppNumber;
                entity.LegalCopyrightText = model.LegalCopyrightText;
                entity.LegalPrivacyPolicyUrl = model.LegalPrivacyPolicyUrl;
                entity.LegalTermsOfServiceUrl = model.LegalTermsOfServiceUrl;
                entity.LegalComplaintsUrl = model.LegalComplaintsUrl;
                entity.LegalMedicalPrivacyNotice = model.LegalMedicalPrivacyNotice;
                entity.WorkingHoursTitle = model.WorkingHoursTitle;
                entity.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                return ServiceResult.Successful("تنظیمات فوتر با موفقیت ذخیره شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره تنظیمات فوتر - ClinicId: {ClinicId}", clinicId);
                return ServiceResult.Failed("خطا در ذخیره تنظیمات فوتر");
            }
        }

        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.Length >= 10 && !digits.StartsWith("98"))
                return "98" + digits.TrimStart('0');
            return digits;
        }
    }
}
