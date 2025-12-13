using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.CMS;
using Newtonsoft.Json;
using Serilog;
using System.ComponentModel;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت اشتراک‌های خبرنامه
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class NewsletterSubscriptionService : INewsletterSubscriptionService
    {
        private readonly INewsletterSubscriptionRepository _subscriptionRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public NewsletterSubscriptionService(
            INewsletterSubscriptionRepository subscriptionRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<PagedResult<NewsletterSubscriptionIndexViewModel>>> GetSubscriptionsAsync(NewsletterSubscriptionSearchViewModel searchModel)
        {
            try
            {
                if (searchModel == null)
                {
                    searchModel = new NewsletterSubscriptionSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var subscriptions = await _subscriptionRepository.SearchAsync(
                    searchModel.SearchTerm,
                    searchModel.IsActive,
                    searchModel.IsVerified,
                    searchModel.Source,
                    searchModel.Category,
                    includeDeleted: false);

                var totalCount = subscriptions.Count;
                var pagedItems = subscriptions
                    .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(s => new NewsletterSubscriptionIndexViewModel
                    {
                        NewsletterSubscriptionId = s.NewsletterSubscriptionId,
                        Email = s.Email,
                        FullName = s.FullName,
                        PhoneNumber = s.PhoneNumber,
                        CategoriesDisplay = GetCategoriesDisplay(s.Categories),
                        Source = s.Source,
                        SourceDisplay = GetEnumDescription(s.Source),
                        IsActive = s.IsActive,
                        IsVerified = s.IsVerified,
                        SubscriptionDate = s.CreatedAt,
                        VerifiedAt = s.VerifiedAt,
                        UnsubscribedAt = s.UnsubscribedAt
                    })
                    .ToList();

                var pagedResult = new PagedResult<NewsletterSubscriptionIndexViewModel>(
                    pagedItems,
                    totalCount,
                    searchModel.PageNumber,
                    searchModel.PageSize);

                return ServiceResult<PagedResult<NewsletterSubscriptionIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست اشتراک‌های خبرنامه");
                return ServiceResult<PagedResult<NewsletterSubscriptionIndexViewModel>>.Failed("خطا در دریافت لیست اشتراک‌های خبرنامه");
            }
        }

        public async Task<ServiceResult<NewsletterSubscriptionDetailsViewModel>> GetSubscriptionDetailsAsync(int subscriptionId)
        {
            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                if (subscription == null)
                {
                    return ServiceResult<NewsletterSubscriptionDetailsViewModel>.Failed("اشتراک یافت نشد");
                }

                var categories = ParseCategories(subscription.Categories);

                var viewModel = new NewsletterSubscriptionDetailsViewModel
                {
                    NewsletterSubscriptionId = subscription.NewsletterSubscriptionId,
                    Email = subscription.Email,
                    FullName = subscription.FullName,
                    PhoneNumber = subscription.PhoneNumber,
                    CategoriesDisplay = GetCategoriesDisplay(subscription.Categories),
                    Categories = categories,
                    Source = subscription.Source,
                    SourceDisplay = GetEnumDescription(subscription.Source),
                    IsActive = subscription.IsActive,
                    IsVerified = subscription.IsVerified,
                    VerifiedAt = subscription.VerifiedAt,
                    UnsubscribedAt = subscription.UnsubscribedAt,
                    IpAddress = subscription.IpAddress,
                    UserAgent = subscription.UserAgent,
                    CreatedAt = subscription.CreatedAt,
                    CreatedByUserName = subscription.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = subscription.UpdatedAt,
                    UpdatedByUserName = subscription.UpdatedByUser?.UserName
                };

                // دریافت تاریخچه Campaign ها
                // TODO: بعداً با INewsletterCampaignRecipientRepository پیاده‌سازی می‌شود

                return ServiceResult<NewsletterSubscriptionDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات اشتراک - SubscriptionId: {SubscriptionId}", subscriptionId);
                return ServiceResult<NewsletterSubscriptionDetailsViewModel>.Failed("خطا در دریافت جزئیات اشتراک");
            }
        }

        public async Task<ServiceResult<NewsletterSubscription>> CreateSubscriptionAsync(PublicNewsletterSubscriptionViewModel model, string ipAddress = null, string userAgent = null)
        {
            try
            {
                if (model == null)
                {
                    return ServiceResult<NewsletterSubscription>.Failed("اطلاعات اشتراک نامعتبر است");
                }

                // بررسی تکراری بودن ایمیل
                var exists = await _subscriptionRepository.ExistsAsync(model.Email);
                if (exists)
                {
                    return ServiceResult<NewsletterSubscription>.Failed("این ایمیل قبلاً ثبت شده است");
                }

                // ایجاد Verification Token
                var verificationToken = GenerateSecureToken();
                var unsubscribeToken = GenerateSecureToken();

                var subscription = new NewsletterSubscription
                {
                    Email = model.Email.Trim().ToLower(),
                    FullName = model.FullName?.Trim(),
                    PhoneNumber = null,
                    Categories = JsonConvert.SerializeObject(new List<string>()), // خالی
                    Source = NewsletterSubscriptionSource.Website,
                    IsActive = true,
                    IsVerified = false, // Double Opt-in
                    VerificationToken = verificationToken,
                    UnsubscribeToken = unsubscribeToken,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = null // از سایت ثبت شده
                };

                _subscriptionRepository.Add(subscription);
                await _context.SaveChangesAsync();

                _logger.Information("اشتراک جدید ایجاد شد - Email: {Email}, SubscriptionId: {SubscriptionId}", 
                    subscription.Email, subscription.NewsletterSubscriptionId);

                return ServiceResult<NewsletterSubscription>.Successful(subscription, "اشتراک با موفقیت ثبت شد. لطفاً ایمیل خود را تایید کنید.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اشتراک - Email: {Email}", model?.Email);
                return ServiceResult<NewsletterSubscription>.Failed("خطا در ثبت اشتراک");
            }
        }

        public async Task<ServiceResult<NewsletterSubscription>> CreateSubscriptionByAdminAsync(NewsletterSubscriptionCreateEditViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return ServiceResult<NewsletterSubscription>.Failed("اطلاعات اشتراک نامعتبر است");
                }

                // بررسی تکراری بودن ایمیل
                var exists = await _subscriptionRepository.ExistsAsync(model.Email);
                if (exists)
                {
                    return ServiceResult<NewsletterSubscription>.Failed("این ایمیل قبلاً ثبت شده است");
                }

                var subscription = new NewsletterSubscription
                {
                    Email = model.Email.Trim().ToLower(),
                    FullName = model.FullName?.Trim(),
                    PhoneNumber = model.PhoneNumber?.Trim(),
                    Categories = SerializeCategories(model.SelectedCategories),
                    Source = model.Source,
                    IsActive = model.IsActive,
                    IsVerified = true, // توسط ادمین ثبت شده، نیازی به تایید نیست
                    VerificationToken = GenerateSecureToken(),
                    UnsubscribeToken = GenerateSecureToken(),
                    VerifiedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = _currentUserService.UserId
                };

                _subscriptionRepository.Add(subscription);
                await _context.SaveChangesAsync();

                _logger.Information("اشتراک جدید توسط ادمین ایجاد شد - Email: {Email}, SubscriptionId: {SubscriptionId}", 
                    subscription.Email, subscription.NewsletterSubscriptionId);

                return ServiceResult<NewsletterSubscription>.Successful(subscription, "اشتراک با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اشتراک توسط ادمین - Email: {Email}", model?.Email);
                return ServiceResult<NewsletterSubscription>.Failed("خطا در ایجاد اشتراک");
            }
        }

        public async Task<ServiceResult<NewsletterSubscription>> UpdateSubscriptionAsync(NewsletterSubscriptionCreateEditViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return ServiceResult<NewsletterSubscription>.Failed("اطلاعات اشتراک نامعتبر است");
                }

                var subscription = await _subscriptionRepository.GetByIdAsync(model.NewsletterSubscriptionId);
                if (subscription == null)
                {
                    return ServiceResult<NewsletterSubscription>.Failed("اشتراک یافت نشد");
                }

                // بررسی تکراری بودن ایمیل (اگر تغییر کرده باشد)
                if (subscription.Email != model.Email.Trim().ToLower())
                {
                    var exists = await _subscriptionRepository.ExistsAsync(model.Email);
                    if (exists)
                    {
                        return ServiceResult<NewsletterSubscription>.Failed("این ایمیل قبلاً ثبت شده است");
                    }
                }

                subscription.Email = model.Email.Trim().ToLower();
                subscription.FullName = model.FullName?.Trim();
                subscription.PhoneNumber = model.PhoneNumber?.Trim();
                subscription.Categories = SerializeCategories(model.SelectedCategories);
                subscription.Source = model.Source;
                subscription.IsActive = model.IsActive;
                subscription.UpdatedAt = DateTime.Now;
                subscription.UpdatedByUserId = _currentUserService.UserId;

                _subscriptionRepository.Update(subscription);
                await _context.SaveChangesAsync();

                _logger.Information("اشتراک به‌روزرسانی شد - SubscriptionId: {SubscriptionId}", subscription.NewsletterSubscriptionId);

                return ServiceResult<NewsletterSubscription>.Successful(subscription, "اشتراک با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اشتراک - SubscriptionId: {SubscriptionId}", model?.NewsletterSubscriptionId);
                return ServiceResult<NewsletterSubscription>.Failed("خطا در به‌روزرسانی اشتراک");
            }
        }

        public async Task<ServiceResult> DeleteSubscriptionAsync(int subscriptionId)
        {
            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                if (subscription == null)
                {
                    return ServiceResult.Failed("اشتراک یافت نشد");
                }

                _subscriptionRepository.Delete(subscription);
                subscription.DeletedByUserId = _currentUserService.UserId;
                await _context.SaveChangesAsync();

                _logger.Information("اشتراک حذف شد - SubscriptionId: {SubscriptionId}", subscriptionId);

                return ServiceResult.Successful("اشتراک با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اشتراک - SubscriptionId: {SubscriptionId}", subscriptionId);
                return ServiceResult.Failed("خطا در حذف اشتراک");
            }
        }

        public async Task<ServiceResult> ActivateSubscriptionAsync(int subscriptionId)
        {
            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                if (subscription == null)
                {
                    return ServiceResult.Failed("اشتراک یافت نشد");
                }

                subscription.IsActive = true;
                subscription.UpdatedAt = DateTime.Now;
                subscription.UpdatedByUserId = _currentUserService.UserId;

                _subscriptionRepository.Update(subscription);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("اشتراک فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال کردن اشتراک - SubscriptionId: {SubscriptionId}", subscriptionId);
                return ServiceResult.Failed("خطا در فعال کردن اشتراک");
            }
        }

        public async Task<ServiceResult> DeactivateSubscriptionAsync(int subscriptionId)
        {
            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                if (subscription == null)
                {
                    return ServiceResult.Failed("اشتراک یافت نشد");
                }

                subscription.IsActive = false;
                subscription.UpdatedAt = DateTime.Now;
                subscription.UpdatedByUserId = _currentUserService.UserId;

                _subscriptionRepository.Update(subscription);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("اشتراک غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال کردن اشتراک - SubscriptionId: {SubscriptionId}", subscriptionId);
                return ServiceResult.Failed("خطا در غیرفعال کردن اشتراک");
            }
        }

        public async Task<ServiceResult> VerifySubscriptionAsync(string verificationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(verificationToken))
                {
                    return ServiceResult.Failed("Token تایید نامعتبر است");
                }

                var subscription = await _subscriptionRepository.GetByVerificationTokenAsync(verificationToken);
                if (subscription == null)
                {
                    return ServiceResult.Failed("Token تایید نامعتبر است");
                }

                if (subscription.IsVerified)
                {
                    return ServiceResult.Successful("اشتراک شما قبلاً تایید شده است");
                }

                subscription.IsVerified = true;
                subscription.VerifiedAt = DateTime.Now;
                subscription.UpdatedAt = DateTime.Now;

                _subscriptionRepository.Update(subscription);
                await _context.SaveChangesAsync();

                _logger.Information("اشتراک تایید شد - Email: {Email}, SubscriptionId: {SubscriptionId}", 
                    subscription.Email, subscription.NewsletterSubscriptionId);

                return ServiceResult.Successful("اشتراک شما با موفقیت تایید شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تایید اشتراک - Token: {Token}", verificationToken);
                return ServiceResult.Failed("خطا در تایید اشتراک");
            }
        }

        public async Task<ServiceResult> UnsubscribeAsync(string unsubscribeToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(unsubscribeToken))
                {
                    return ServiceResult.Failed("Token لغو اشتراک نامعتبر است");
                }

                var subscription = await _subscriptionRepository.GetByUnsubscribeTokenAsync(unsubscribeToken);
                if (subscription == null)
                {
                    return ServiceResult.Failed("Token لغو اشتراک نامعتبر است");
                }

                if (!subscription.IsActive)
                {
                    return ServiceResult.Successful("اشتراک شما قبلاً لغو شده است");
                }

                subscription.IsActive = false;
                subscription.UnsubscribedAt = DateTime.Now;
                subscription.UpdatedAt = DateTime.Now;

                _subscriptionRepository.Update(subscription);
                await _context.SaveChangesAsync();

                _logger.Information("اشتراک لغو شد - Email: {Email}, SubscriptionId: {SubscriptionId}", 
                    subscription.Email, subscription.NewsletterSubscriptionId);

                return ServiceResult.Successful("اشتراک شما با موفقیت لغو شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو اشتراک - Token: {Token}", unsubscribeToken);
                return ServiceResult.Failed("خطا در لغو اشتراک");
            }
        }

        public async Task<ServiceResult> ImportSubscriptionsAsync(List<NewsletterSubscriptionCreateEditViewModel> subscriptions)
        {
            try
            {
                if (subscriptions == null || !subscriptions.Any())
                {
                    return ServiceResult.Failed("لیست اشتراک‌ها خالی است");
                }

                var successCount = 0;
                var failCount = 0;
                var errors = new List<string>();

                foreach (var model in subscriptions)
                {
                    try
                    {
                        var exists = await _subscriptionRepository.ExistsAsync(model.Email);
                        if (exists)
                        {
                            failCount++;
                            errors.Add($"ایمیل {model.Email} قبلاً ثبت شده است");
                            continue;
                        }

                        var subscription = new NewsletterSubscription
                        {
                            Email = model.Email.Trim().ToLower(),
                            FullName = model.FullName?.Trim(),
                            PhoneNumber = model.PhoneNumber?.Trim(),
                            Categories = SerializeCategories(model.SelectedCategories),
                            Source = NewsletterSubscriptionSource.Import,
                            IsActive = model.IsActive,
                            IsVerified = true, // Import شده، نیازی به تایید نیست
                            VerificationToken = GenerateSecureToken(),
                            UnsubscribeToken = GenerateSecureToken(),
                            VerifiedAt = DateTime.Now,
                            CreatedAt = DateTime.Now,
                            CreatedByUserId = _currentUserService.UserId
                        };

                        _subscriptionRepository.Add(subscription);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errors.Add($"خطا در ثبت {model.Email}: {ex.Message}");
                        _logger.Warning(ex, "خطا در Import اشتراک - Email: {Email}", model.Email);
                    }
                }

                await _context.SaveChangesAsync();

                var message = $"Import انجام شد: {successCount} موفق، {failCount} ناموفق";
                if (errors.Any())
                {
                    message += $"\nخطاها:\n{string.Join("\n", errors.Take(10))}";
                }

                _logger.Information("Import اشتراک‌ها انجام شد - Success: {SuccessCount}, Fail: {FailCount}", 
                    successCount, failCount);

                return ServiceResult.Successful(message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Import اشتراک‌ها");
                return ServiceResult.Failed("خطا در Import اشتراک‌ها");
            }
        }

        public async Task<ServiceResult<byte[]>> ExportSubscriptionsAsync(NewsletterSubscriptionSearchViewModel searchModel)
        {
            // TODO: پیاده‌سازی Export به Excel
            // استفاده از EPPlus یا ClosedXML
            return ServiceResult<byte[]>.Failed("Export به Excel در حال پیاده‌سازی است");
        }

        public async Task<ServiceResult<NewsletterStatisticsViewModel>> GetStatisticsAsync()
        {
            // TODO: پیاده‌سازی آمار
            return ServiceResult<NewsletterStatisticsViewModel>.Failed("آمار در حال پیاده‌سازی است");
        }

        public async Task<ServiceResult<int>> GetActiveCountAsync()
        {
            try
            {
                var subscriptions = await _subscriptionRepository.GetActiveAsync();
                return ServiceResult<int>.Successful(subscriptions.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد اشتراک‌های فعال");
                return ServiceResult<int>.Failed("خطا در دریافت تعداد اشتراک‌های فعال");
            }
        }

        public async Task<ServiceResult<int>> GetVerifiedCountAsync()
        {
            try
            {
                var subscriptions = await _subscriptionRepository.GetActiveAndVerifiedAsync();
                return ServiceResult<int>.Successful(subscriptions.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد اشتراک‌های تایید شده");
                return ServiceResult<int>.Failed("خطا در دریافت تعداد اشتراک‌های تایید شده");
            }
        }

        #region Private Helper Methods

        private string GenerateSecureToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
            }
        }

        private string SerializeCategories(List<NewsletterCategory> categories)
        {
            if (categories == null || !categories.Any())
                return JsonConvert.SerializeObject(new List<string>());

            return JsonConvert.SerializeObject(categories.Select(c => c.ToString()).ToList());
        }

        private List<NewsletterCategory> ParseCategories(string categoriesJson)
        {
            if (string.IsNullOrWhiteSpace(categoriesJson))
                return new List<NewsletterCategory>();

            try
            {
                var categoryStrings = JsonConvert.DeserializeObject<List<string>>(categoriesJson);
                if (categoryStrings == null)
                    return new List<NewsletterCategory>();

                var categories = new List<NewsletterCategory>();
                foreach (var categoryString in categoryStrings)
                {
                    if (Enum.TryParse<NewsletterCategory>(categoryString, out var category))
                    {
                        categories.Add(category);
                    }
                }
                return categories;
            }
            catch
            {
                return new List<NewsletterCategory>();
            }
        }

        private string GetCategoriesDisplay(string categoriesJson)
        {
            var categories = ParseCategories(categoriesJson);
            if (!categories.Any())
                return "بدون دسته‌بندی";

            return string.Join(", ", categories.Select(c => GetEnumDescription(c)));
        }

        private string GetEnumDescription<T>(T enumValue) where T : struct
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field == null) return enumValue.ToString();

            var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;

            return attribute?.Description ?? enumValue.ToString();
        }

        #endregion
    }
}

