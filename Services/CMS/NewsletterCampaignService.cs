using System;
using System.Collections.Generic;
using System.Linq;
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
    /// سرویس مدیریت Campaign های خبرنامه
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class NewsletterCampaignService : INewsletterCampaignService
    {
        private readonly INewsletterCampaignRepository _campaignRepository;
        private readonly INewsletterCampaignRecipientRepository _recipientRepository;
        private readonly INewsletterSubscriptionRepository _subscriptionRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public NewsletterCampaignService(
            INewsletterCampaignRepository campaignRepository,
            INewsletterCampaignRecipientRepository recipientRepository,
            INewsletterSubscriptionRepository subscriptionRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
            _recipientRepository = recipientRepository ?? throw new ArgumentNullException(nameof(recipientRepository));
            _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<PagedResult<NewsletterCampaignIndexViewModel>>> GetCampaignsAsync(NewsletterCampaignSearchViewModel searchModel)
        {
            try
            {
                if (searchModel == null)
                {
                    searchModel = new NewsletterCampaignSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var campaigns = await _campaignRepository.SearchAsync(
                    searchModel.SearchTerm,
                    searchModel.Status,
                    searchModel.FromDate,
                    searchModel.ToDate,
                    includeDeleted: false);

                var totalCount = campaigns.Count;
                var pagedItems = campaigns
                    .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(c => new NewsletterCampaignIndexViewModel
                    {
                        NewsletterCampaignId = c.NewsletterCampaignId,
                        Title = c.Title,
                        Subject = c.Subject,
                        Status = c.Status,
                        StatusDisplay = GetEnumDescription(c.Status),
                        TotalRecipients = c.TotalRecipients,
                        SentCount = c.SentCount,
                        OpenedCount = c.OpenedCount,
                        ClickedCount = c.ClickedCount,
                        OpenRate = c.TotalRecipients > 0 ? (double)c.OpenedCount / c.TotalRecipients * 100 : 0,
                        ClickRate = c.TotalRecipients > 0 ? (double)c.ClickedCount / c.TotalRecipients * 100 : 0,
                        ScheduledAt = c.ScheduledAt,
                        SentAt = c.SentAt,
                        CreatedAt = c.CreatedAt
                    })
                    .ToList();

                var pagedResult = new PagedResult<NewsletterCampaignIndexViewModel>(
                    pagedItems,
                    totalCount,
                    searchModel.PageNumber,
                    searchModel.PageSize);

                return ServiceResult<PagedResult<NewsletterCampaignIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست Campaign های خبرنامه");
                return ServiceResult<PagedResult<NewsletterCampaignIndexViewModel>>.Failed("خطا در دریافت لیست Campaign ها");
            }
        }

        public async Task<ServiceResult<NewsletterCampaignDetailsViewModel>> GetCampaignDetailsAsync(int campaignId)
        {
            try
            {
                var campaign = await _campaignRepository.GetByIdAsync(campaignId);
                if (campaign == null)
                {
                    return ServiceResult<NewsletterCampaignDetailsViewModel>.Failed("Campaign یافت نشد");
                }

                var categories = ParseCategories(campaign.Categories);

                var viewModel = new NewsletterCampaignDetailsViewModel
                {
                    NewsletterCampaignId = campaign.NewsletterCampaignId,
                    Title = campaign.Title,
                    Subject = campaign.Subject,
                    Content = campaign.Content,
                    NewsletterTemplateId = campaign.NewsletterTemplateId,
                    TemplateName = campaign.Template?.Name,
                    CategoriesDisplay = GetCategoriesDisplay(campaign.Categories),
                    Categories = categories,
                    SendToAll = campaign.SendToAll,
                    ScheduledAt = campaign.ScheduledAt,
                    SentAt = campaign.SentAt,
                    Status = campaign.Status,
                    StatusDisplay = GetEnumDescription(campaign.Status),
                    TotalRecipients = campaign.TotalRecipients,
                    SentCount = campaign.SentCount,
                    FailedCount = campaign.FailedCount,
                    OpenedCount = campaign.OpenedCount,
                    ClickedCount = campaign.ClickedCount,
                    OpenRate = campaign.TotalRecipients > 0 ? (double)campaign.OpenedCount / campaign.TotalRecipients * 100 : 0,
                    ClickRate = campaign.TotalRecipients > 0 ? (double)campaign.ClickedCount / campaign.TotalRecipients * 100 : 0,
                    CreatedAt = campaign.CreatedAt,
                    CreatedByUserName = campaign.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = campaign.UpdatedAt,
                    UpdatedByUserName = campaign.UpdatedByUser?.UserName
                };

                return ServiceResult<NewsletterCampaignDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات Campaign - CampaignId: {CampaignId}", campaignId);
                return ServiceResult<NewsletterCampaignDetailsViewModel>.Failed("خطا در دریافت جزئیات Campaign");
            }
        }

        public async Task<ServiceResult<NewsletterCampaignCreateEditViewModel>> GetCampaignForEditAsync(int campaignId)
        {
            try
            {
                var campaign = await _campaignRepository.GetByIdAsync(campaignId);
                if (campaign == null)
                {
                    return ServiceResult<NewsletterCampaignCreateEditViewModel>.Failed("Campaign یافت نشد");
                }

                var viewModel = new NewsletterCampaignCreateEditViewModel
                {
                    NewsletterCampaignId = campaign.NewsletterCampaignId,
                    Title = campaign.Title,
                    Subject = campaign.Subject,
                    Content = campaign.Content,
                    NewsletterTemplateId = campaign.NewsletterTemplateId,
                    SelectedCategories = ParseCategories(campaign.Categories),
                    SendToAll = campaign.SendToAll,
                    ScheduledAt = campaign.ScheduledAt
                };

                return ServiceResult<NewsletterCampaignCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت Campaign برای ویرایش - CampaignId: {CampaignId}", campaignId);
                return ServiceResult<NewsletterCampaignCreateEditViewModel>.Failed("خطا در دریافت Campaign");
            }
        }

        public async Task<ServiceResult<NewsletterCampaign>> CreateCampaignAsync(NewsletterCampaignCreateEditViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return ServiceResult<NewsletterCampaign>.Failed("اطلاعات Campaign نامعتبر است");
                }

                var campaign = new NewsletterCampaign
                {
                    Title = model.Title.Trim(),
                    Subject = model.Subject.Trim(),
                    Content = model.Content,
                    NewsletterTemplateId = model.NewsletterTemplateId,
                    Categories = SerializeCategories(model.SelectedCategories),
                    SendToAll = model.SendToAll,
                    ScheduledAt = model.ScheduledAt,
                    Status = NewsletterCampaignStatus.Draft,
                    TotalRecipients = 0,
                    SentCount = 0,
                    FailedCount = 0,
                    OpenedCount = 0,
                    ClickedCount = 0,
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = _currentUserService.UserId
                };

                _campaignRepository.Add(campaign);
                await _context.SaveChangesAsync();

                _logger.Information("Campaign جدید ایجاد شد - Title: {Title}, CampaignId: {CampaignId}", 
                    campaign.Title, campaign.NewsletterCampaignId);

                return ServiceResult<NewsletterCampaign>.Successful(campaign, "Campaign با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Campaign - Title: {Title}", model?.Title);
                return ServiceResult<NewsletterCampaign>.Failed("خطا در ایجاد Campaign");
            }
        }

        public async Task<ServiceResult<NewsletterCampaign>> UpdateCampaignAsync(NewsletterCampaignCreateEditViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return ServiceResult<NewsletterCampaign>.Failed("اطلاعات Campaign نامعتبر است");
                }

                var campaign = await _campaignRepository.GetByIdAsync(model.NewsletterCampaignId);
                if (campaign == null)
                {
                    return ServiceResult<NewsletterCampaign>.Failed("Campaign یافت نشد");
                }

                // اگر Campaign ارسال شده باشد، نمی‌توان آن را ویرایش کرد
                if (campaign.Status == NewsletterCampaignStatus.Sent || campaign.Status == NewsletterCampaignStatus.Sending)
                {
                    return ServiceResult<NewsletterCampaign>.Failed("Campaign ارسال شده را نمی‌توان ویرایش کرد");
                }

                campaign.Title = model.Title.Trim();
                campaign.Subject = model.Subject.Trim();
                campaign.Content = model.Content;
                campaign.NewsletterTemplateId = model.NewsletterTemplateId;
                campaign.Categories = SerializeCategories(model.SelectedCategories);
                campaign.SendToAll = model.SendToAll;
                campaign.ScheduledAt = model.ScheduledAt;
                campaign.UpdatedAt = DateTime.Now;
                campaign.UpdatedByUserId = _currentUserService.UserId;

                _campaignRepository.Update(campaign);
                await _context.SaveChangesAsync();

                _logger.Information("Campaign به‌روزرسانی شد - CampaignId: {CampaignId}", campaign.NewsletterCampaignId);

                return ServiceResult<NewsletterCampaign>.Successful(campaign, "Campaign با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی Campaign - CampaignId: {CampaignId}", model?.NewsletterCampaignId);
                return ServiceResult<NewsletterCampaign>.Failed("خطا در به‌روزرسانی Campaign");
            }
        }

        public async Task<ServiceResult> DeleteCampaignAsync(int campaignId)
        {
            try
            {
                var campaign = await _campaignRepository.GetByIdAsync(campaignId);
                if (campaign == null)
                {
                    return ServiceResult.Failed("Campaign یافت نشد");
                }

                // اگر Campaign در حال ارسال است، نمی‌توان آن را حذف کرد
                if (campaign.Status == NewsletterCampaignStatus.Sending)
                {
                    return ServiceResult.Failed("Campaign در حال ارسال است و نمی‌توان آن را حذف کرد");
                }

                _campaignRepository.Delete(campaign);
                campaign.DeletedByUserId = _currentUserService.UserId;
                await _context.SaveChangesAsync();

                _logger.Information("Campaign حذف شد - CampaignId: {CampaignId}", campaignId);

                return ServiceResult.Successful("Campaign با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف Campaign - CampaignId: {CampaignId}", campaignId);
                return ServiceResult.Failed("خطا در حذف Campaign");
            }
        }

        public async Task<ServiceResult<NewsletterCampaignSendViewModel>> GetCampaignForSendAsync(int campaignId)
        {
            try
            {
                var campaign = await _campaignRepository.GetByIdAsync(campaignId);
                if (campaign == null)
                {
                    return ServiceResult<NewsletterCampaignSendViewModel>.Failed("Campaign یافت نشد");
                }

                // محاسبه تعداد Recipients
                var estimatedRecipients = await EstimateRecipientsAsync(
                    ParseCategories(campaign.Categories),
                    campaign.SendToAll);

                var viewModel = new NewsletterCampaignSendViewModel
                {
                    NewsletterCampaignId = campaign.NewsletterCampaignId,
                    Title = campaign.Title,
                    Subject = campaign.Subject,
                    Content = campaign.Content,
                    EstimatedRecipients = estimatedRecipients.Data,
                    SendEmail = true,
                    SendSms = false,
                    ScheduledAt = null
                };

                return ServiceResult<NewsletterCampaignSendViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت Campaign برای ارسال - CampaignId: {CampaignId}", campaignId);
                return ServiceResult<NewsletterCampaignSendViewModel>.Failed("خطا در دریافت Campaign");
            }
        }

        public async Task<ServiceResult> SendCampaignAsync(int campaignId, bool sendEmail, bool sendSms)
        {
            try
            {
                var campaign = await _campaignRepository.GetByIdAsync(campaignId);
                if (campaign == null)
                {
                    return ServiceResult.Failed("Campaign یافت نشد");
                }

                if (campaign.Status != NewsletterCampaignStatus.Draft && campaign.Status != NewsletterCampaignStatus.Scheduled)
                {
                    return ServiceResult.Failed("Campaign در وضعیت قابل ارسال نیست");
                }

                // دریافت لیست Recipients
                List<NewsletterSubscription> recipients;
                if (campaign.SendToAll)
                {
                    recipients = await _subscriptionRepository.GetActiveAndVerifiedAsync();
                }
                else
                {
                    var categories = ParseCategories(campaign.Categories);
                    recipients = await _subscriptionRepository.GetByCategoriesAsync(categories);
                }

                if (!recipients.Any())
                {
                    return ServiceResult.Failed("هیچ مشترک فعالی برای ارسال یافت نشد");
                }

                // ایجاد Recipient Records
                var recipientRecords = recipients.Select(s => new NewsletterCampaignRecipient
                {
                    NewsletterCampaignId = campaign.NewsletterCampaignId,
                    NewsletterSubscriptionId = s.NewsletterSubscriptionId,
                    Email = s.Email,
                    Status = NewsletterRecipientStatus.Pending,
                    CreatedAt = DateTime.Now
                }).ToList();

                await _recipientRepository.BulkInsertAsync(recipientRecords);

                // به‌روزرسانی Campaign
                campaign.Status = NewsletterCampaignStatus.Sending;
                campaign.TotalRecipients = recipients.Count;
                campaign.SentAt = DateTime.Now;
                campaign.UpdatedAt = DateTime.Now;
                campaign.UpdatedByUserId = _currentUserService.UserId;

                _campaignRepository.Update(campaign);
                await _context.SaveChangesAsync();

                _logger.Information("Campaign شروع به ارسال شد - CampaignId: {CampaignId}, Recipients: {Count}", 
                    campaignId, recipients.Count);

                // TODO: در Phase 3، ارسال واقعی ایمیل/SMS از طریق Background Job انجام می‌شود
                // فعلاً فقط Status را به Sending تغییر می‌دهیم

                return ServiceResult.Successful($"Campaign شروع به ارسال شد. {recipients.Count} مشترک در صف ارسال قرار گرفتند.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال Campaign - CampaignId: {CampaignId}", campaignId);
                return ServiceResult.Failed("خطا در ارسال Campaign");
            }
        }

        public async Task<ServiceResult> ScheduleCampaignAsync(int campaignId, DateTime scheduledAt, bool sendEmail, bool sendSms)
        {
            try
            {
                var campaign = await _campaignRepository.GetByIdAsync(campaignId);
                if (campaign == null)
                {
                    return ServiceResult.Failed("Campaign یافت نشد");
                }

                if (campaign.Status != NewsletterCampaignStatus.Draft)
                {
                    return ServiceResult.Failed("فقط Campaign های پیش‌نویس را می‌توان زمان‌بندی کرد");
                }

                if (scheduledAt <= DateTime.Now)
                {
                    return ServiceResult.Failed("تاریخ زمان‌بندی باید در آینده باشد");
                }

                campaign.ScheduledAt = scheduledAt;
                campaign.Status = NewsletterCampaignStatus.Scheduled;
                campaign.UpdatedAt = DateTime.Now;
                campaign.UpdatedByUserId = _currentUserService.UserId;

                _campaignRepository.Update(campaign);
                await _context.SaveChangesAsync();

                _logger.Information("Campaign زمان‌بندی شد - CampaignId: {CampaignId}, ScheduledAt: {ScheduledAt}", 
                    campaignId, scheduledAt);

                return ServiceResult.Successful("Campaign با موفقیت زمان‌بندی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در زمان‌بندی Campaign - CampaignId: {CampaignId}", campaignId);
                return ServiceResult.Failed("خطا در زمان‌بندی Campaign");
            }
        }

        public async Task<ServiceResult> CancelScheduledCampaignAsync(int campaignId)
        {
            try
            {
                var campaign = await _campaignRepository.GetByIdAsync(campaignId);
                if (campaign == null)
                {
                    return ServiceResult.Failed("Campaign یافت نشد");
                }

                if (campaign.Status != NewsletterCampaignStatus.Scheduled)
                {
                    return ServiceResult.Failed("فقط Campaign های زمان‌بندی شده را می‌توان لغو کرد");
                }

                campaign.ScheduledAt = null;
                campaign.Status = NewsletterCampaignStatus.Draft;
                campaign.UpdatedAt = DateTime.Now;
                campaign.UpdatedByUserId = _currentUserService.UserId;

                _campaignRepository.Update(campaign);
                await _context.SaveChangesAsync();

                _logger.Information("زمان‌بندی Campaign لغو شد - CampaignId: {CampaignId}", campaignId);

                return ServiceResult.Successful("زمان‌بندی Campaign با موفقیت لغو شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو زمان‌بندی Campaign - CampaignId: {CampaignId}", campaignId);
                return ServiceResult.Failed("خطا در لغو زمان‌بندی Campaign");
            }
        }

        public async Task<ServiceResult<NewsletterCampaignStatisticsViewModel>> GetCampaignStatisticsAsync(int campaignId)
        {
            try
            {
                var campaign = await _campaignRepository.GetByIdAsync(campaignId);
                if (campaign == null)
                {
                    return ServiceResult<NewsletterCampaignStatisticsViewModel>.Failed("Campaign یافت نشد");
                }

                var openedCount = await _recipientRepository.GetOpenedCountByCampaignIdAsync(campaignId);
                var clickedCount = await _recipientRepository.GetClickedCountByCampaignIdAsync(campaignId);

                var viewModel = new NewsletterCampaignStatisticsViewModel
                {
                    NewsletterCampaignId = campaign.NewsletterCampaignId,
                    Title = campaign.Title,
                    TotalRecipients = campaign.TotalRecipients,
                    SentCount = campaign.SentCount,
                    OpenedCount = openedCount,
                    ClickedCount = clickedCount,
                    OpenRate = campaign.TotalRecipients > 0 ? (double)openedCount / campaign.TotalRecipients * 100 : 0,
                    ClickRate = campaign.TotalRecipients > 0 ? (double)clickedCount / campaign.TotalRecipients * 100 : 0,
                    SentAt = campaign.SentAt
                };

                return ServiceResult<NewsletterCampaignStatisticsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار Campaign - CampaignId: {CampaignId}", campaignId);
                return ServiceResult<NewsletterCampaignStatisticsViewModel>.Failed("خطا در دریافت آمار Campaign");
            }
        }

        public async Task<ServiceResult> TrackEmailOpenAsync(int campaignId, int recipientId)
        {
            try
            {
                var recipient = await _recipientRepository.GetByIdAsync(recipientId);
                if (recipient == null || recipient.NewsletterCampaignId != campaignId)
                {
                    return ServiceResult.Failed("Recipient یافت نشد");
                }

                if (!recipient.OpenedAt.HasValue)
                {
                    recipient.OpenedAt = DateTime.Now;
                    recipient.UpdatedAt = DateTime.Now;

                    _recipientRepository.Update(recipient);

                    // به‌روزرسانی Campaign Statistics
                    var campaign = await _campaignRepository.GetByIdAsync(campaignId);
                    if (campaign != null)
                    {
                        campaign.OpenedCount++;
                        _campaignRepository.Update(campaign);
                    }

                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Tracking باز شدن ایمیل - CampaignId: {CampaignId}, RecipientId: {RecipientId}", 
                    campaignId, recipientId);
                return ServiceResult.Failed("خطا در Tracking");
            }
        }

        public async Task<ServiceResult> TrackEmailClickAsync(int campaignId, int recipientId, string url)
        {
            try
            {
                var recipient = await _recipientRepository.GetByIdAsync(recipientId);
                if (recipient == null || recipient.NewsletterCampaignId != campaignId)
                {
                    return ServiceResult.Failed("Recipient یافت نشد");
                }

                if (!recipient.ClickedAt.HasValue)
                {
                    recipient.ClickedAt = DateTime.Now;
                    recipient.ClickedUrl = url;
                    recipient.UpdatedAt = DateTime.Now;

                    _recipientRepository.Update(recipient);

                    // به‌روزرسانی Campaign Statistics
                    var campaign = await _campaignRepository.GetByIdAsync(campaignId);
                    if (campaign != null)
                    {
                        campaign.ClickedCount++;
                        _campaignRepository.Update(campaign);
                    }

                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Tracking کلیک - CampaignId: {CampaignId}, RecipientId: {RecipientId}", 
                    campaignId, recipientId);
                return ServiceResult.Failed("خطا در Tracking");
            }
        }

        public async Task<ServiceResult<int>> ProcessScheduledCampaignsAsync()
        {
            try
            {
                var scheduledCampaigns = await _campaignRepository.GetScheduledAsync();
                if (!scheduledCampaigns.Any())
                {
                    return ServiceResult<int>.Successful(0);
                }

                int processedCount = 0;
                foreach (var campaign in scheduledCampaigns)
                {
                    try
                    {
                        // ارسال Campaign
                        var result = await SendCampaignAsync(campaign.NewsletterCampaignId, true, false);
                        if (result.Success)
                        {
                            processedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "خطا در پردازش Campaign زمان‌بندی شده - CampaignId: {CampaignId}", 
                            campaign.NewsletterCampaignId);
                    }
                }

                return ServiceResult<int>.Successful(processedCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش Campaign های زمان‌بندی شده");
                return ServiceResult<int>.Failed("خطا در پردازش Campaign های زمان‌بندی شده");
            }
        }

        public async Task<ServiceResult<int>> EstimateRecipientsAsync(List<NewsletterCategory> categories, bool sendToAll)
        {
            try
            {
                List<NewsletterSubscription> recipients;
                if (sendToAll)
                {
                    recipients = await _subscriptionRepository.GetActiveAndVerifiedAsync();
                }
                else
                {
                    recipients = await _subscriptionRepository.GetByCategoriesAsync(categories ?? new List<NewsletterCategory>());
                }

                return ServiceResult<int>.Successful(recipients.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه تعداد Recipients");
                return ServiceResult<int>.Failed("خطا در محاسبه تعداد Recipients");
            }
        }

        #region Private Helper Methods

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

