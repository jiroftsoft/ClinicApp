using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت تماس‌های اضطراری (Emergency Contact)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class EmergencyContactService : IEmergencyContactService
    {
        private readonly IEmergencyContactRepository _emergencyContactRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public EmergencyContactService(
            IEmergencyContactRepository emergencyContactRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _emergencyContactRepository = emergencyContactRepository ?? throw new ArgumentNullException(nameof(emergencyContactRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<PagedResult<EmergencyContactIndexViewModel>>> GetEmergencyContactsAsync(EmergencyContactSearchViewModel filter)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست تماس‌های اضطراری - Filter: {@Filter}", filter);

                var allContacts = await _emergencyContactRepository.GetAllAsync(includeDeleted: false);

                // اعمال فیلترها
                var query = allContacts.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim();
                    query = query.Where(e => e.Title.Contains(searchTerm) || 
                                            (e.PhoneNumber != null && e.PhoneNumber.Contains(searchTerm)) ||
                                            (e.ContactType != null && e.ContactType.Contains(searchTerm)) ||
                                            (e.ShortDescription != null && e.ShortDescription.Contains(searchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(filter.ContactType))
                {
                    query = query.Where(e => e.ContactType == filter.ContactType);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(e => e.IsActive == filter.IsActive.Value);
                }

                if (filter.IsAlwaysVisible.HasValue)
                {
                    query = query.Where(e => e.IsAlwaysVisible == filter.IsAlwaysVisible.Value);
                }

                var totalCount = query.Count();
                var contacts = query
                    .OrderBy(e => e.DisplayOrder)
                    .ThenBy(e => e.Title)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var viewModels = contacts.Select(e => new EmergencyContactIndexViewModel
                {
                    EmergencyContactId = e.EmergencyContactId,
                    ContactType = e.ContactType,
                    Title = e.Title,
                    PhoneNumber = e.PhoneNumber,
                    SecondaryPhoneNumber = e.SecondaryPhoneNumber,
                    Address = e.Address,
                    IconUrl = e.IconUrl,
                    IsActive = e.IsActive,
                    IsAlwaysVisible = e.IsAlwaysVisible,
                    DisplayOrder = e.DisplayOrder,
                    CreatedAt = e.CreatedAt
                }).ToList();

                var pagedResult = new PagedResult<EmergencyContactIndexViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ServiceResult<PagedResult<EmergencyContactIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست تماس‌های اضطراری");
                return ServiceResult<PagedResult<EmergencyContactIndexViewModel>>.Failed("خطا در دریافت لیست تماس‌های اضطراری");
            }
        }

        public async Task<ServiceResult<EmergencyContactDetailsViewModel>> GetEmergencyContactDetailsAsync(int emergencyContactId)
        {
            try
            {
                var emergencyContact = await _emergencyContactRepository.GetByIdAsync(emergencyContactId);
                if (emergencyContact == null)
                {
                    return ServiceResult<EmergencyContactDetailsViewModel>.Failed("تماس اضطراری یافت نشد");
                }

                var viewModel = new EmergencyContactDetailsViewModel
                {
                    EmergencyContactId = emergencyContact.EmergencyContactId,
                    ContactType = emergencyContact.ContactType,
                    Title = emergencyContact.Title,
                    PhoneNumber = emergencyContact.PhoneNumber,
                    SecondaryPhoneNumber = emergencyContact.SecondaryPhoneNumber,
                    Address = emergencyContact.Address,
                    Instructions = emergencyContact.Instructions,
                    MapUrl = emergencyContact.MapUrl,
                    WhatsAppUrl = emergencyContact.WhatsAppUrl,
                    TelegramUrl = emergencyContact.TelegramUrl,
                    Email = emergencyContact.Email,
                    WebsiteUrl = emergencyContact.WebsiteUrl,
                    IconUrl = emergencyContact.IconUrl,
                    IsActive = emergencyContact.IsActive,
                    IsAlwaysVisible = emergencyContact.IsAlwaysVisible,
                    DisplayOrder = emergencyContact.DisplayOrder,
                    ShortDescription = emergencyContact.ShortDescription,
                    Slug = emergencyContact.Slug,
                    MetaTitle = emergencyContact.MetaTitle,
                    MetaDescription = emergencyContact.MetaDescription,
                    CreatedAt = emergencyContact.CreatedAt,
                    CreatedByUserName = emergencyContact.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = emergencyContact.UpdatedAt,
                    UpdatedByUserName = emergencyContact.UpdatedByUser?.UserName
                };

                return ServiceResult<EmergencyContactDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات تماس اضطراری - EmergencyContactId: {EmergencyContactId}", emergencyContactId);
                return ServiceResult<EmergencyContactDetailsViewModel>.Failed("خطا در دریافت جزئیات تماس اضطراری");
            }
        }

        public async Task<ServiceResult<EmergencyContactCreateEditViewModel>> GetEmergencyContactForEditAsync(int emergencyContactId)
        {
            try
            {
                var emergencyContact = await _emergencyContactRepository.GetByIdAsync(emergencyContactId);
                if (emergencyContact == null)
                {
                    return ServiceResult<EmergencyContactCreateEditViewModel>.Failed("تماس اضطراری یافت نشد");
                }

                var viewModel = new EmergencyContactCreateEditViewModel
                {
                    EmergencyContactId = emergencyContact.EmergencyContactId,
                    ContactType = emergencyContact.ContactType,
                    Title = emergencyContact.Title,
                    PhoneNumber = emergencyContact.PhoneNumber,
                    SecondaryPhoneNumber = emergencyContact.SecondaryPhoneNumber,
                    Address = emergencyContact.Address,
                    Instructions = emergencyContact.Instructions,
                    MapUrl = emergencyContact.MapUrl,
                    WhatsAppUrl = emergencyContact.WhatsAppUrl,
                    TelegramUrl = emergencyContact.TelegramUrl,
                    Email = emergencyContact.Email,
                    WebsiteUrl = emergencyContact.WebsiteUrl,
                    IconUrl = emergencyContact.IconUrl,
                    IsActive = emergencyContact.IsActive,
                    IsAlwaysVisible = emergencyContact.IsAlwaysVisible,
                    DisplayOrder = emergencyContact.DisplayOrder,
                    ShortDescription = emergencyContact.ShortDescription,
                    Slug = emergencyContact.Slug,
                    MetaTitle = emergencyContact.MetaTitle,
                    MetaDescription = emergencyContact.MetaDescription
                };

                return ServiceResult<EmergencyContactCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تماس اضطراری برای ویرایش - EmergencyContactId: {EmergencyContactId}", emergencyContactId);
                return ServiceResult<EmergencyContactCreateEditViewModel>.Failed("خطا در دریافت تماس اضطراری برای ویرایش");
            }
        }

        public async Task<ServiceResult<EmergencyContact>> CreateEmergencyContactAsync(EmergencyContactCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد تماس اضطراری جدید - Title: {Title}", model.Title);

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug))
                {
                    var existingSlug = await _emergencyContactRepository.GetBySlugAsync(model.Slug);
                    if (existingSlug != null)
                    {
                        return ServiceResult<EmergencyContact>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                var emergencyContact = new EmergencyContact
                {
                    ContactType = model.ContactType,
                    Title = model.Title,
                    PhoneNumber = model.PhoneNumber,
                    SecondaryPhoneNumber = model.SecondaryPhoneNumber,
                    Address = model.Address,
                    Instructions = model.Instructions,
                    MapUrl = model.MapUrl,
                    WhatsAppUrl = model.WhatsAppUrl,
                    TelegramUrl = model.TelegramUrl,
                    Email = model.Email,
                    WebsiteUrl = model.WebsiteUrl,
                    IconUrl = model.IconUrl,
                    IsActive = model.IsActive,
                    IsAlwaysVisible = model.IsAlwaysVisible,
                    DisplayOrder = model.DisplayOrder,
                    ShortDescription = model.ShortDescription,
                    Slug = model.Slug ?? GenerateSlug(model.Title),
                    MetaTitle = model.MetaTitle,
                    MetaDescription = model.MetaDescription,
                    CreatedByUserId = _currentUserService.UserId
                };

                _emergencyContactRepository.Add(emergencyContact);
                await _context.SaveChangesAsync();

                _logger.Information("تماس اضطراری با موفقیت ایجاد شد - EmergencyContactId: {EmergencyContactId}", emergencyContact.EmergencyContactId);
                return ServiceResult<EmergencyContact>.Successful(emergencyContact, "تماس اضطراری با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تماس اضطراری");
                return ServiceResult<EmergencyContact>.Failed("خطا در ایجاد تماس اضطراری");
            }
        }

        public async Task<ServiceResult<EmergencyContact>> UpdateEmergencyContactAsync(EmergencyContactCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی تماس اضطراری - EmergencyContactId: {EmergencyContactId}", model.EmergencyContactId);

                var emergencyContact = await _emergencyContactRepository.GetByIdAsync(model.EmergencyContactId);
                if (emergencyContact == null)
                {
                    return ServiceResult<EmergencyContact>.Failed("تماس اضطراری یافت نشد");
                }

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug) && model.Slug != emergencyContact.Slug)
                {
                    var existingSlug = await _emergencyContactRepository.GetBySlugAsync(model.Slug);
                    if (existingSlug != null && existingSlug.EmergencyContactId != model.EmergencyContactId)
                    {
                        return ServiceResult<EmergencyContact>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                emergencyContact.ContactType = model.ContactType;
                emergencyContact.Title = model.Title;
                emergencyContact.PhoneNumber = model.PhoneNumber;
                emergencyContact.SecondaryPhoneNumber = model.SecondaryPhoneNumber;
                emergencyContact.Address = model.Address;
                emergencyContact.Instructions = model.Instructions;
                emergencyContact.MapUrl = model.MapUrl;
                emergencyContact.WhatsAppUrl = model.WhatsAppUrl;
                emergencyContact.TelegramUrl = model.TelegramUrl;
                emergencyContact.Email = model.Email;
                emergencyContact.WebsiteUrl = model.WebsiteUrl;
                emergencyContact.IconUrl = model.IconUrl;
                emergencyContact.IsActive = model.IsActive;
                emergencyContact.IsAlwaysVisible = model.IsAlwaysVisible;
                emergencyContact.DisplayOrder = model.DisplayOrder;
                emergencyContact.ShortDescription = model.ShortDescription;
                emergencyContact.Slug = model.Slug ?? emergencyContact.Slug ?? GenerateSlug(model.Title);
                emergencyContact.MetaTitle = model.MetaTitle;
                emergencyContact.MetaDescription = model.MetaDescription;
                emergencyContact.UpdatedByUserId = _currentUserService.UserId;

                _emergencyContactRepository.Update(emergencyContact);
                await _context.SaveChangesAsync();

                _logger.Information("تماس اضطراری با موفقیت به‌روزرسانی شد - EmergencyContactId: {EmergencyContactId}", emergencyContact.EmergencyContactId);
                return ServiceResult<EmergencyContact>.Successful(emergencyContact, "تماس اضطراری با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تماس اضطراری - EmergencyContactId: {EmergencyContactId}", model.EmergencyContactId);
                return ServiceResult<EmergencyContact>.Failed("خطا در به‌روزرسانی تماس اضطراری");
            }
        }

        public async Task<ServiceResult> DeleteEmergencyContactAsync(int emergencyContactId)
        {
            try
            {
                _logger.Information("حذف تماس اضطراری - EmergencyContactId: {EmergencyContactId}", emergencyContactId);

                var emergencyContact = await _emergencyContactRepository.GetByIdAsync(emergencyContactId);
                if (emergencyContact == null)
                {
                    return ServiceResult.Failed("تماس اضطراری یافت نشد");
                }

                _emergencyContactRepository.Delete(emergencyContact);
                await _context.SaveChangesAsync();

                _logger.Information("تماس اضطراری با موفقیت حذف شد - EmergencyContactId: {EmergencyContactId}", emergencyContactId);
                return ServiceResult.Successful("تماس اضطراری با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تماس اضطراری - EmergencyContactId: {EmergencyContactId}", emergencyContactId);
                return ServiceResult.Failed("خطا در حذف تماس اضطراری");
            }
        }

        public async Task<ServiceResult> ActivateEmergencyContactAsync(int emergencyContactId)
        {
            try
            {
                var emergencyContact = await _emergencyContactRepository.GetByIdAsync(emergencyContactId);
                if (emergencyContact == null)
                {
                    return ServiceResult.Failed("تماس اضطراری یافت نشد");
                }

                emergencyContact.IsActive = true;
                emergencyContact.UpdatedByUserId = _currentUserService.UserId;

                _emergencyContactRepository.Update(emergencyContact);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("تماس اضطراری با موفقیت فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی تماس اضطراری - EmergencyContactId: {EmergencyContactId}", emergencyContactId);
                return ServiceResult.Failed("خطا در فعال‌سازی تماس اضطراری");
            }
        }

        public async Task<ServiceResult> DeactivateEmergencyContactAsync(int emergencyContactId)
        {
            try
            {
                var emergencyContact = await _emergencyContactRepository.GetByIdAsync(emergencyContactId);
                if (emergencyContact == null)
                {
                    return ServiceResult.Failed("تماس اضطراری یافت نشد");
                }

                emergencyContact.IsActive = false;
                emergencyContact.UpdatedByUserId = _currentUserService.UserId;

                _emergencyContactRepository.Update(emergencyContact);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("تماس اضطراری با موفقیت غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی تماس اضطراری - EmergencyContactId: {EmergencyContactId}", emergencyContactId);
                return ServiceResult.Failed("خطا در غیرفعال‌سازی تماس اضطراری");
            }
        }

        public async Task<ServiceResult> SetAlwaysVisibleAsync(int emergencyContactId, bool isAlwaysVisible)
        {
            try
            {
                var emergencyContact = await _emergencyContactRepository.GetByIdAsync(emergencyContactId);
                if (emergencyContact == null)
                {
                    return ServiceResult.Failed("تماس اضطراری یافت نشد");
                }

                emergencyContact.IsAlwaysVisible = isAlwaysVisible;
                emergencyContact.UpdatedByUserId = _currentUserService.UserId;

                _emergencyContactRepository.Update(emergencyContact);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful(isAlwaysVisible ? "تماس اضطراری به عنوان همیشه قابل مشاهده تنظیم شد" : "تماس اضطراری از حالت همیشه قابل مشاهده خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت همیشه قابل مشاهده تماس اضطراری - EmergencyContactId: {EmergencyContactId}", emergencyContactId);
                return ServiceResult.Failed("خطا در تنظیم وضعیت همیشه قابل مشاهده تماس اضطراری");
            }
        }

        public async Task<ServiceResult<List<EmergencyContactPublicViewModel>>> GetActiveContactsAsync()
        {
            try
            {
                var contacts = await _emergencyContactRepository.GetActiveContactsAsync();
                
                var viewModels = contacts.Select(e => new EmergencyContactPublicViewModel
                {
                    EmergencyContactId = e.EmergencyContactId,
                    ContactType = e.ContactType,
                    TypeDisplayName = GetTypeDisplayName(e.ContactType),
                    Title = e.Title,
                    PhoneNumber = e.PhoneNumber,
                    SecondaryPhoneNumber = e.SecondaryPhoneNumber,
                    Address = e.Address,
                    Instructions = e.Instructions,
                    MapUrl = e.MapUrl,
                    WhatsAppUrl = e.WhatsAppUrl,
                    TelegramUrl = e.TelegramUrl,
                    Email = e.Email,
                    WebsiteUrl = e.WebsiteUrl,
                    IconUrl = e.IconUrl,
                    ShortDescription = e.ShortDescription,
                    Slug = e.Slug
                }).ToList();

                return ServiceResult<List<EmergencyContactPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تماس‌های اضطراری فعال");
                return ServiceResult<List<EmergencyContactPublicViewModel>>.Failed("خطا در دریافت تماس‌های اضطراری فعال");
            }
        }

        public async Task<ServiceResult<List<EmergencyContactPublicViewModel>>> GetAlwaysVisibleContactsAsync()
        {
            try
            {
                var contacts = await _emergencyContactRepository.GetAlwaysVisibleContactsAsync();
                
                var viewModels = contacts.Select(e => new EmergencyContactPublicViewModel
                {
                    EmergencyContactId = e.EmergencyContactId,
                    ContactType = e.ContactType,
                    TypeDisplayName = GetTypeDisplayName(e.ContactType),
                    Title = e.Title,
                    PhoneNumber = e.PhoneNumber,
                    SecondaryPhoneNumber = e.SecondaryPhoneNumber,
                    Address = e.Address,
                    Instructions = e.Instructions,
                    MapUrl = e.MapUrl,
                    WhatsAppUrl = e.WhatsAppUrl,
                    TelegramUrl = e.TelegramUrl,
                    Email = e.Email,
                    WebsiteUrl = e.WebsiteUrl,
                    IconUrl = e.IconUrl,
                    ShortDescription = e.ShortDescription,
                    Slug = e.Slug
                }).ToList();

                return ServiceResult<List<EmergencyContactPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تماس‌های اضطراری همیشه قابل مشاهده");
                return ServiceResult<List<EmergencyContactPublicViewModel>>.Failed("خطا در دریافت تماس‌های اضطراری همیشه قابل مشاهده");
            }
        }

        public async Task<ServiceResult<List<EmergencyContactPublicViewModel>>> GetByContactTypeAsync(string contactType)
        {
            try
            {
                var contacts = await _emergencyContactRepository.GetByContactTypeAsync(contactType);
                
                var viewModels = contacts.Select(e => new EmergencyContactPublicViewModel
                {
                    EmergencyContactId = e.EmergencyContactId,
                    ContactType = e.ContactType,
                    TypeDisplayName = GetTypeDisplayName(e.ContactType),
                    Title = e.Title,
                    PhoneNumber = e.PhoneNumber,
                    SecondaryPhoneNumber = e.SecondaryPhoneNumber,
                    Address = e.Address,
                    Instructions = e.Instructions,
                    MapUrl = e.MapUrl,
                    WhatsAppUrl = e.WhatsAppUrl,
                    TelegramUrl = e.TelegramUrl,
                    Email = e.Email,
                    WebsiteUrl = e.WebsiteUrl,
                    IconUrl = e.IconUrl,
                    ShortDescription = e.ShortDescription,
                    Slug = e.Slug
                }).ToList();

                return ServiceResult<List<EmergencyContactPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تماس‌های اضطراری بر اساس نوع");
                return ServiceResult<List<EmergencyContactPublicViewModel>>.Failed("خطا در دریافت تماس‌های اضطراری بر اساس نوع");
            }
        }

        public async Task<ServiceResult<List<EmergencyContactPublicViewModel>>> SearchContactsAsync(string searchTerm)
        {
            try
            {
                var contacts = await _emergencyContactRepository.SearchContactsAsync(searchTerm);
                
                var viewModels = contacts.Select(e => new EmergencyContactPublicViewModel
                {
                    EmergencyContactId = e.EmergencyContactId,
                    ContactType = e.ContactType,
                    TypeDisplayName = GetTypeDisplayName(e.ContactType),
                    Title = e.Title,
                    PhoneNumber = e.PhoneNumber,
                    SecondaryPhoneNumber = e.SecondaryPhoneNumber,
                    Address = e.Address,
                    Instructions = e.Instructions,
                    MapUrl = e.MapUrl,
                    WhatsAppUrl = e.WhatsAppUrl,
                    TelegramUrl = e.TelegramUrl,
                    Email = e.Email,
                    WebsiteUrl = e.WebsiteUrl,
                    IconUrl = e.IconUrl,
                    ShortDescription = e.ShortDescription,
                    Slug = e.Slug
                }).ToList();

                return ServiceResult<List<EmergencyContactPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی تماس‌های اضطراری");
                return ServiceResult<List<EmergencyContactPublicViewModel>>.Failed("خطا در جستجوی تماس‌های اضطراری");
            }
        }

        public async Task<ServiceResult<EmergencyContact>> GetBySlugAsync(string slug)
        {
            try
            {
                var emergencyContact = await _emergencyContactRepository.GetBySlugAsync(slug);
                if (emergencyContact == null)
                {
                    return ServiceResult<EmergencyContact>.Failed("تماس اضطراری یافت نشد");
                }

                return ServiceResult<EmergencyContact>.Successful(emergencyContact);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تماس اضطراری بر اساس Slug - Slug: {Slug}", slug);
                return ServiceResult<EmergencyContact>.Failed("خطا در دریافت تماس اضطراری");
            }
        }

        #region Helper Methods

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return Guid.NewGuid().ToString("N").Substring(0, 8);

            // تبدیل به حروف کوچک و حذف کاراکترهای خاص
            var slug = title.ToLower()
                .Replace(" ", "-")
                .Replace("آ", "a")
                .Replace("ا", "a")
                .Replace("ب", "b")
                .Replace("پ", "p")
                .Replace("ت", "t")
                .Replace("ث", "s")
                .Replace("ج", "j")
                .Replace("چ", "ch")
                .Replace("ح", "h")
                .Replace("خ", "kh")
                .Replace("د", "d")
                .Replace("ذ", "z")
                .Replace("ر", "r")
                .Replace("ز", "z")
                .Replace("ژ", "zh")
                .Replace("س", "s")
                .Replace("ش", "sh")
                .Replace("ص", "s")
                .Replace("ض", "z")
                .Replace("ط", "t")
                .Replace("ظ", "z")
                .Replace("ع", "a")
                .Replace("غ", "gh")
                .Replace("ف", "f")
                .Replace("ق", "gh")
                .Replace("ک", "k")
                .Replace("گ", "g")
                .Replace("ل", "l")
                .Replace("م", "m")
                .Replace("ن", "n")
                .Replace("و", "v")
                .Replace("ه", "h")
                .Replace("ی", "y");

            // حذف کاراکترهای غیرمجاز
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                slug = slug.Replace(c.ToString(), "");
            }

            return slug;
        }

        private string GetTypeDisplayName(string contactType)
        {
            return contactType switch
            {
                "Emergency" => "اورژانس",
                "Ambulance" => "آمبولانس",
                "Poison Control" => "مرکز مسمومیت",
                "Fire" => "آتش‌نشانی",
                "Police" => "پلیس",
                "Hospital" => "بیمارستان",
                "Clinic" => "کلینیک",
                _ => contactType ?? "عمومی"
            };
        }

        #endregion
    }
}

