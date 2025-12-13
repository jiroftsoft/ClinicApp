# 📊 تحلیل کامل - Newsletter/Subscription Management Module
## مدیریت خبرنامه و اشتراک‌ها - کلینیک درمانی شفا جیرفت

**تاریخ تحلیل:** 2025-12-12  
**اولویت:** ⭐⭐⭐⭐ (بالا)  
**زمان تخمینی:** 3-4 روز

---

## 📋 فهرست مطالب

1. [تحلیل نیازمندی‌ها](#تحلیل-نیازمندی‌ها)
2. [معماری سیستم](#معماری-سیستم)
3. [طراحی Entity Models](#طراحی-entity-models)
4. [طراحی Enums](#طراحی-enums)
5. [طراحی ViewModels](#طراحی-viewmodels)
6. [طراحی Interfaces](#طراحی-interfaces)
7. [نقشه راه پیاده‌سازی](#نقشه-راه-پیاده‌سازی)

---

## 1️⃣ تحلیل نیازمندی‌ها

### 1.1 User Stories

#### US-1: ثبت اشتراک از سایت
**Actor:** بازدیدکننده سایت  
**Goal:** ثبت‌نام در خبرنامه  
**Value:** دریافت آخرین اخبار و اطلاعیه‌ها

**Flow:**
1. کاربر ایمیل و نام را وارد می‌کند
2. سیستم ایمیل تایید ارسال می‌کند (Double Opt-in)
3. کاربر روی لینک تایید کلیک می‌کند
4. اشتراک فعال می‌شود

**Acceptance Criteria:**
- ✅ فرم ساده (ایمیل + نام اختیاری)
- ✅ Validation ایمیل
- ✅ جلوگیری از ثبت تکراری
- ✅ Double Opt-in
- ✅ پیام موفقیت/خطا

#### US-2: مدیریت لیست اشتراک‌ها
**Actor:** Admin  
**Goal:** مدیریت کامل مشترکین  
**Value:** کنترل و سازماندهی

**Features:**
- نمایش لیست با Pagination
- جستجو (ایمیل، نام)
- فیلتر (وضعیت، دسته‌بندی، منبع)
- فعال/غیرفعال کردن
- حذف
- Export به Excel
- Import از Excel

#### US-3: گروه‌بندی اشتراک‌ها
**Actor:** Admin  
**Goal:** دسته‌بندی مشترکین بر اساس علاقه‌مندی  
**Value:** ارسال هدفمند

**Categories:**
- مقالات (Articles)
- اطلاعیه‌ها (Announcements)
- خدمات جدید (New Services)
- نکات سلامتی (Health Tips)
- رویدادها (Events)
- تخفیف‌ها (Promotions)

#### US-4: ارسال خبرنامه
**Actor:** Admin  
**Goal:** ارسال خبرنامه به گروه‌های مختلف  
**Value:** اطلاع‌رسانی موثر

**Features:**
- انتخاب دسته‌بندی یا تمام مشترکین
- استفاده از Template
- ارسال فوری
- زمان‌بندی ارسال
- ارسال ایمیل
- ارسال SMS (اختیاری)
- Preview قبل از ارسال

#### US-5: Template های خبرنامه
**Actor:** Admin  
**Goal:** ایجاد Template های حرفه‌ای  
**Value:** یکپارچگی و سرعت

**Features:**
- ایجاد/ویرایش Template
- CKEditor برای محتوا
- Variables ({{FullName}}, {{Email}}, ...)
- Preview
- فعال/غیرفعال

#### US-6: تاریخچه و آمار
**Actor:** Admin  
**Goal:** بررسی عملکرد  
**Value:** تحلیل و بهبود

**Features:**
- لیست تمام Campaign ها
- آمار هر Campaign (ارسال شده، موفق، ناموفق)
- Open Rate
- Click Rate
- نمودارهای آماری

#### US-7: لغو اشتراک
**Actor:** مشترک  
**Goal:** لغو اشتراک  
**Value:** کنترل کاربر

**Flow:**
1. کاربر روی لینک لغو در ایمیل کلیک می‌کند
2. صفحه تایید نمایش داده می‌شود
3. کاربر تایید می‌کند
4. اشتراک غیرفعال می‌شود

---

## 2️⃣ معماری سیستم

### 2.1 Component Diagram

```
┌─────────────────────────────────────────────────────────┐
│                    Public Layer                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │ Subscribe    │  │ Verify       │  │ Unsubscribe  │ │
│  │ Form         │  │ Email        │  │ Page         │ │
│  └──────────────┘  └──────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                  Controller Layer                       │
│  ┌──────────────────┐  ┌──────────────────┐           │
│  │ Newsletter        │  │ Newsletter        │           │
│  │ Controller       │  │ Subscription      │           │
│  │ (Public)         │  │ Controller (Admin)│           │
│  └──────────────────┘  └──────────────────┘           │
│  ┌──────────────────┐  ┌──────────────────┐           │
│  │ Newsletter        │  │ Newsletter        │           │
│  │ Template         │  │ Campaign         │           │
│  │ Controller       │  │ Controller       │           │
│  └──────────────────┘  └──────────────────┘           │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                   Service Layer                         │
│  ┌──────────────────┐  ┌──────────────────┐           │
│  │ Newsletter        │  │ Newsletter        │           │
│  │ Subscription     │  │ Template         │           │
│  │ Service          │  │ Service          │           │
│  └──────────────────┘  └──────────────────┘           │
│  ┌──────────────────┐  ┌──────────────────┐           │
│  │ Newsletter        │  │ Newsletter        │           │
│  │ Campaign         │  │ Email Service    │           │
│  │ Service          │  │                  │           │
│  └──────────────────┘  └──────────────────┘           │
│  ┌──────────────────┐                                  │
│  │ Newsletter        │                                  │
│  │ SMS Service      │                                  │
│  └──────────────────┘                                  │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                  Repository Layer                       │
│  ┌──────────────────┐  ┌──────────────────┐           │
│  │ Newsletter        │  │ Newsletter        │           │
│  │ Subscription     │  │ Template         │           │
│  │ Repository       │  │ Repository       │           │
│  └──────────────────┘  └──────────────────┘           │
│  ┌──────────────────┐  ┌──────────────────┐           │
│  │ Newsletter        │  │ Newsletter        │           │
│  │ Campaign         │  │ Campaign         │           │
│  │ Repository       │  │ Recipient        │           │
│  └──────────────────┘  │ Repository       │           │
│                        └──────────────────┘           │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                    Database Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │ Newsletter   │  │ Newsletter   │  │ Newsletter   │ │
│  │ Subscriptions│  │ Templates    │  │ Campaigns     │ │
│  └──────────────┘  └──────────────┘  └──────────────┘ │
│  ┌──────────────┐                                      │
│  │ Newsletter    │                                      │
│  │ Campaign      │                                      │
│  │ Recipients    │                                      │
│  └──────────────┘                                      │
└─────────────────────────────────────────────────────────┘
```

### 2.2 Data Flow

#### Flow 1: ثبت اشتراک از سایت
```
User → NewsletterController.Subscribe → NewsletterSubscriptionService.CreateSubscriptionAsync
  → NewsletterSubscriptionRepository.Add → Database
  → NewsletterEmailService.SendVerificationEmailAsync → EmailService
```

#### Flow 2: تایید ایمیل
```
User → NewsletterController.Verify → NewsletterSubscriptionService.VerifySubscriptionAsync
  → NewsletterSubscriptionRepository.Update → Database
```

#### Flow 3: ارسال Campaign
```
Admin → NewsletterCampaignController.Send → NewsletterCampaignService.SendCampaignAsync
  → NewsletterSubscriptionRepository.GetActiveByCategoriesAsync → Database
  → NewsletterCampaignRecipientRepository.BulkInsert → Database
  → NewsletterEmailService.SendNewsletterAsync (برای هر Recipient)
  → NewsletterCampaignRecipientRepository.Update (Status, SentAt)
```

#### Flow 4: Tracking
```
Email Open → NewsletterController.TrackOpen → NewsletterCampaignService.TrackEmailOpenAsync
  → NewsletterCampaignRecipientRepository.Update (OpenedAt)

Email Click → NewsletterController.TrackClick → NewsletterCampaignService.TrackEmailClickAsync
  → NewsletterCampaignRecipientRepository.Update (ClickedAt)
```

---

## 3️⃣ طراحی Entity Models

### 3.1 NewsletterSubscription

```csharp
public class NewsletterSubscription : ISoftDelete, ITrackable
{
    public int NewsletterSubscriptionId { get; set; }
    
    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; } // Unique Index
    
    [MaxLength(200)]
    public string FullName { get; set; }
    
    [MaxLength(50)]
    public string PhoneNumber { get; set; }
    
    // Categories: JSON Array یا جدول جداگانه
    // برای سادگی، از JSON استفاده می‌کنیم
    [Column(TypeName = "ntext")]
    public string Categories { get; set; } // JSON: ["Articles", "Announcements"]
    
    public NewsletterSubscriptionSource Source { get; set; }
    
    public bool IsActive { get; set; }
    
    public bool IsVerified { get; set; } // Double Opt-in
    
    [MaxLength(100)]
    public string VerificationToken { get; set; } // Unique Index
    
    public DateTime? VerifiedAt { get; set; }
    
    public DateTime? UnsubscribedAt { get; set; }
    
    [MaxLength(100)]
    public string UnsubscribeToken { get; set; } // Unique Index
    
    [MaxLength(500)]
    public string IpAddress { get; set; }
    
    [MaxLength(500)]
    public string UserAgent { get; set; }
    
    // ISoftDelete, ITrackable
}
```

### 3.2 NewsletterTemplate

```csharp
public class NewsletterTemplate : ISoftDelete, ITrackable
{
    public int NewsletterTemplateId { get; set; }
    
    [Required, MaxLength(200)]
    public string Name { get; set; }
    
    [Required, MaxLength(500)]
    public string Subject { get; set; }
    
    [Required, Column(TypeName = "ntext")]
    public string Content { get; set; } // HTML with Variables
    
    public bool IsActive { get; set; }
    
    // ISoftDelete, ITrackable
}
```

### 3.3 NewsletterCampaign

```csharp
public class NewsletterCampaign : ISoftDelete, ITrackable
{
    public int NewsletterCampaignId { get; set; }
    
    [Required, MaxLength(300)]
    public string Title { get; set; }
    
    [Required, MaxLength(500)]
    public string Subject { get; set; }
    
    [Required, Column(TypeName = "ntext")]
    public string Content { get; set; } // HTML
    
    public int? NewsletterTemplateId { get; set; }
    public virtual NewsletterTemplate Template { get; set; }
    
    [Column(TypeName = "ntext")]
    public string Categories { get; set; } // JSON Array
    
    public bool SendToAll { get; set; }
    
    public DateTime? ScheduledAt { get; set; }
    
    public DateTime? SentAt { get; set; }
    
    public NewsletterCampaignStatus Status { get; set; }
    
    public int TotalRecipients { get; set; }
    
    public int SentCount { get; set; }
    
    public int FailedCount { get; set; }
    
    public int OpenedCount { get; set; }
    
    public int ClickedCount { get; set; }
    
    // ISoftDelete, ITrackable
}
```

### 3.4 NewsletterCampaignRecipient

```csharp
public class NewsletterCampaignRecipient : ITrackable
{
    public int NewsletterCampaignRecipientId { get; set; }
    
    public int NewsletterCampaignId { get; set; }
    public virtual NewsletterCampaign Campaign { get; set; }
    
    public int NewsletterSubscriptionId { get; set; }
    public virtual NewsletterSubscription Subscription { get; set; }
    
    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; }
    
    public NewsletterRecipientStatus Status { get; set; }
    
    public DateTime? SentAt { get; set; }
    
    public DateTime? OpenedAt { get; set; }
    
    public DateTime? ClickedAt { get; set; }
    
    [MaxLength(1000)]
    public string ErrorMessage { get; set; }
    
    // ITrackable
}
```

---

## 4️⃣ طراحی Enums

### 4.1 NewsletterSubscriptionSource

```csharp
public enum NewsletterSubscriptionSource : byte
{
    [Display(Name = "سایت")]
    Website = 1,
    
    [Display(Name = "ادمین")]
    Admin = 2,
    
    [Display(Name = "وارد کردن دستی")]
    Import = 3,
    
    [Display(Name = "API")]
    Api = 4
}
```

### 4.2 NewsletterCampaignStatus

```csharp
public enum NewsletterCampaignStatus : byte
{
    [Display(Name = "پیش‌نویس")]
    Draft = 1,
    
    [Display(Name = "زمان‌بندی شده")]
    Scheduled = 2,
    
    [Display(Name = "در حال ارسال")]
    Sending = 3,
    
    [Display(Name = "ارسال شده")]
    Sent = 4,
    
    [Display(Name = "ناموفق")]
    Failed = 5
}
```

### 4.3 NewsletterRecipientStatus

```csharp
public enum NewsletterRecipientStatus : byte
{
    [Display(Name = "در انتظار")]
    Pending = 1,
    
    [Display(Name = "ارسال شده")]
    Sent = 2,
    
    [Display(Name = "ناموفق")]
    Failed = 3,
    
    [Display(Name = "بازگشت خورده")]
    Bounced = 4
}
```

### 4.4 NewsletterCategory

```csharp
public enum NewsletterCategory : byte
{
    [Display(Name = "مقالات")]
    Articles = 1,
    
    [Display(Name = "اطلاعیه‌ها")]
    Announcements = 2,
    
    [Display(Name = "خدمات جدید")]
    NewServices = 3,
    
    [Display(Name = "نکات سلامتی")]
    HealthTips = 4,
    
    [Display(Name = "رویدادها")]
    Events = 5,
    
    [Display(Name = "تخفیف‌ها")]
    Promotions = 6
}
```

---

## 5️⃣ طراحی ViewModels

### 5.1 Subscription ViewModels

```csharp
// Index
public class NewsletterSubscriptionIndexViewModel
{
    public int NewsletterSubscriptionId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string CategoriesDisplay { get; set; }
    public NewsletterSubscriptionSource Source { get; set; }
    public string SourceDisplay { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime SubscriptionDate { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? UnsubscribedAt { get; set; }
}

// Create/Edit
public class NewsletterSubscriptionCreateEditViewModel
{
    public int NewsletterSubscriptionId { get; set; }
    
    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; }
    
    [MaxLength(200)]
    public string FullName { get; set; }
    
    [MaxLength(50)]
    public string PhoneNumber { get; set; }
    
    public List<NewsletterCategory> SelectedCategories { get; set; }
    
    public NewsletterSubscriptionSource Source { get; set; }
    
    public bool IsActive { get; set; }
}

// Public Subscribe
public class PublicNewsletterSubscriptionViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; }
    
    public string FullName { get; set; }
}
```

### 5.2 Template ViewModels

```csharp
// Index
public class NewsletterTemplateIndexViewModel
{
    public int NewsletterTemplateId { get; set; }
    public string Name { get; set; }
    public string Subject { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Create/Edit
public class NewsletterTemplateCreateEditViewModel
{
    public int NewsletterTemplateId { get; set; }
    
    [Required, MaxLength(200)]
    public string Name { get; set; }
    
    [Required, MaxLength(500)]
    public string Subject { get; set; }
    
    [Required, AllowHtml]
    public string Content { get; set; }
    
    public bool IsActive { get; set; }
}
```

### 5.3 Campaign ViewModels

```csharp
// Index
public class NewsletterCampaignIndexViewModel
{
    public int NewsletterCampaignId { get; set; }
    public string Title { get; set; }
    public string Subject { get; set; }
    public NewsletterCampaignStatus Status { get; set; }
    public string StatusDisplay { get; set; }
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int OpenedCount { get; set; }
    public int ClickedCount { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Create/Edit
public class NewsletterCampaignCreateEditViewModel
{
    public int NewsletterCampaignId { get; set; }
    
    [Required, MaxLength(300)]
    public string Title { get; set; }
    
    [Required, MaxLength(500)]
    public string Subject { get; set; }
    
    [Required, AllowHtml]
    public string Content { get; set; }
    
    public int? NewsletterTemplateId { get; set; }
    
    public List<NewsletterCategory> SelectedCategories { get; set; }
    
    public bool SendToAll { get; set; }
    
    public DateTime? ScheduledAt { get; set; }
}

// Send
public class NewsletterCampaignSendViewModel
{
    public int NewsletterCampaignId { get; set; }
    public string Title { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public int EstimatedRecipients { get; set; }
    public bool SendEmail { get; set; }
    public bool SendSms { get; set; }
    public DateTime? ScheduledAt { get; set; }
}
```

### 5.4 Statistics ViewModels

```csharp
public class NewsletterStatisticsViewModel
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int VerifiedSubscriptions { get; set; }
    public int UnsubscribedCount { get; set; }
    public int TotalCampaigns { get; set; }
    public int SentCampaigns { get; set; }
    public double AverageOpenRate { get; set; }
    public double AverageClickRate { get; set; }
    public List<NewsletterCampaignStatisticsViewModel> RecentCampaigns { get; set; }
}

public class NewsletterCampaignStatisticsViewModel
{
    public int NewsletterCampaignId { get; set; }
    public string Title { get; set; }
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int OpenedCount { get; set; }
    public int ClickedCount { get; set; }
    public double OpenRate { get; set; }
    public double ClickRate { get; set; }
    public DateTime? SentAt { get; set; }
}
```

---

## 6️⃣ طراحی Interfaces

### 6.1 Repository Interfaces

```csharp
public interface INewsletterSubscriptionRepository
{
    Task<NewsletterSubscription> GetByIdAsync(int subscriptionId);
    Task<NewsletterSubscription> GetByEmailAsync(string email);
    Task<NewsletterSubscription> GetByVerificationTokenAsync(string token);
    Task<NewsletterSubscription> GetByUnsubscribeTokenAsync(string token);
    Task<List<NewsletterSubscription>> GetAllAsync(bool includeDeleted = false);
    Task<List<NewsletterSubscription>> GetActiveAsync(bool includeDeleted = false);
    Task<List<NewsletterSubscription>> GetByCategoriesAsync(List<NewsletterCategory> categories, bool includeDeleted = false);
    Task<List<NewsletterSubscription>> GetBySourceAsync(NewsletterSubscriptionSource source, bool includeDeleted = false);
    Task<List<NewsletterSubscription>> SearchAsync(string searchTerm, bool? isActive, bool? isVerified, NewsletterSubscriptionSource? source, bool includeDeleted = false);
    void Add(NewsletterSubscription subscription);
    void Update(NewsletterSubscription subscription);
    void Delete(NewsletterSubscription subscription);
    Task<bool> ExistsAsync(string email);
}

public interface INewsletterTemplateRepository
{
    Task<NewsletterTemplate> GetByIdAsync(int templateId);
    Task<List<NewsletterTemplate>> GetAllAsync(bool includeDeleted = false);
    Task<List<NewsletterTemplate>> GetActiveAsync(bool includeDeleted = false);
    void Add(NewsletterTemplate template);
    void Update(NewsletterTemplate template);
    void Delete(NewsletterTemplate template);
}

public interface INewsletterCampaignRepository
{
    Task<NewsletterCampaign> GetByIdAsync(int campaignId);
    Task<List<NewsletterCampaign>> GetAllAsync(bool includeDeleted = false);
    Task<List<NewsletterCampaign>> GetByStatusAsync(NewsletterCampaignStatus status, bool includeDeleted = false);
    Task<List<NewsletterCampaign>> GetScheduledAsync(bool includeDeleted = false);
    void Add(NewsletterCampaign campaign);
    void Update(NewsletterCampaign campaign);
    void Delete(NewsletterCampaign campaign);
}

public interface INewsletterCampaignRecipientRepository
{
    Task<List<NewsletterCampaignRecipient>> GetByCampaignIdAsync(int campaignId);
    Task<List<NewsletterCampaignRecipient>> GetBySubscriptionIdAsync(int subscriptionId);
    Task<NewsletterCampaignRecipient> GetByIdAsync(int recipientId);
    void Add(NewsletterCampaignRecipient recipient);
    void Update(NewsletterCampaignRecipient recipient);
    Task BulkInsertAsync(List<NewsletterCampaignRecipient> recipients);
}
```

### 6.2 Service Interfaces

```csharp
public interface INewsletterSubscriptionService
{
    Task<ServiceResult<PagedResult<NewsletterSubscriptionIndexViewModel>>> GetSubscriptionsAsync(NewsletterSubscriptionSearchViewModel searchModel);
    Task<ServiceResult<NewsletterSubscriptionDetailsViewModel>> GetSubscriptionDetailsAsync(int subscriptionId);
    Task<ServiceResult<NewsletterSubscription>> CreateSubscriptionAsync(PublicNewsletterSubscriptionViewModel model, string ipAddress, string userAgent);
    Task<ServiceResult<NewsletterSubscription>> CreateSubscriptionByAdminAsync(NewsletterSubscriptionCreateEditViewModel model);
    Task<ServiceResult<NewsletterSubscription>> UpdateSubscriptionAsync(NewsletterSubscriptionCreateEditViewModel model);
    Task<ServiceResult> DeleteSubscriptionAsync(int subscriptionId);
    Task<ServiceResult> ActivateSubscriptionAsync(int subscriptionId);
    Task<ServiceResult> DeactivateSubscriptionAsync(int subscriptionId);
    Task<ServiceResult> VerifySubscriptionAsync(string verificationToken);
    Task<ServiceResult> UnsubscribeAsync(string unsubscribeToken);
    Task<ServiceResult> ImportSubscriptionsAsync(List<NewsletterSubscriptionCreateEditViewModel> subscriptions);
    Task<ServiceResult<byte[]>> ExportSubscriptionsAsync(NewsletterSubscriptionSearchViewModel searchModel);
    Task<ServiceResult<NewsletterStatisticsViewModel>> GetStatisticsAsync();
}

public interface INewsletterTemplateService
{
    Task<ServiceResult<List<NewsletterTemplateIndexViewModel>>> GetTemplatesAsync();
    Task<ServiceResult<NewsletterTemplateDetailsViewModel>> GetTemplateDetailsAsync(int templateId);
    Task<ServiceResult<NewsletterTemplate>> CreateTemplateAsync(NewsletterTemplateCreateEditViewModel model);
    Task<ServiceResult<NewsletterTemplate>> UpdateTemplateAsync(NewsletterTemplateCreateEditViewModel model);
    Task<ServiceResult> DeleteTemplateAsync(int templateId);
    Task<ServiceResult<string>> RenderTemplateAsync(int templateId, Dictionary<string, string> variables);
    Task<ServiceResult<string>> RenderTemplateAsync(string content, Dictionary<string, string> variables);
}

public interface INewsletterCampaignService
{
    Task<ServiceResult<PagedResult<NewsletterCampaignIndexViewModel>>> GetCampaignsAsync(NewsletterCampaignSearchViewModel searchModel);
    Task<ServiceResult<NewsletterCampaignDetailsViewModel>> GetCampaignDetailsAsync(int campaignId);
    Task<ServiceResult<NewsletterCampaign>> CreateCampaignAsync(NewsletterCampaignCreateEditViewModel model);
    Task<ServiceResult<NewsletterCampaign>> UpdateCampaignAsync(NewsletterCampaignCreateEditViewModel model);
    Task<ServiceResult> DeleteCampaignAsync(int campaignId);
    Task<ServiceResult> SendCampaignAsync(int campaignId, bool sendEmail, bool sendSms);
    Task<ServiceResult> ScheduleCampaignAsync(int campaignId, DateTime scheduledAt, bool sendEmail, bool sendSms);
    Task<ServiceResult> CancelScheduledCampaignAsync(int campaignId);
    Task<ServiceResult<NewsletterCampaignStatisticsViewModel>> GetCampaignStatisticsAsync(int campaignId);
    Task<ServiceResult> TrackEmailOpenAsync(int campaignId, int recipientId);
    Task<ServiceResult> TrackEmailClickAsync(int campaignId, int recipientId, string url);
    Task<ServiceResult<int>> ProcessScheduledCampaignsAsync(); // Background Job
}

public interface INewsletterEmailService
{
    Task<ServiceResult> SendNewsletterAsync(NewsletterCampaign campaign, NewsletterSubscription subscription);
    Task<ServiceResult> SendVerificationEmailAsync(NewsletterSubscription subscription);
    Task<ServiceResult> SendUnsubscribeConfirmationAsync(NewsletterSubscription subscription);
    Task<ServiceResult<string>> RenderContentAsync(string content, Dictionary<string, string> variables);
}

public interface INewsletterSmsService
{
    Task<ServiceResult> SendNewsletterSmsAsync(NewsletterCampaign campaign, NewsletterSubscription subscription);
    Task<ServiceResult> SendVerificationSmsAsync(NewsletterSubscription subscription);
}
```

---

## 7️⃣ نقشه راه پیاده‌سازی

### Phase 1: Foundation (1 روز)
1. ایجاد Enums
2. ایجاد Entity Models
3. ایجاد Entity Configurations
4. ایجاد Migration

### Phase 2: Backend Core (1.5 روز)
1. ایجاد Repository Interfaces
2. پیاده‌سازی Repositories
3. ایجاد Service Interfaces
4. پیاده‌سازی Services (بدون Email/SMS)

### Phase 3: Email & SMS Integration (0.5 روز)
1. پیاده‌سازی NewsletterEmailService
2. پیاده‌سازی NewsletterSmsService
3. یکپارچه‌سازی با EmailService و AsanakSmsService

### Phase 4: Admin Controllers (1 روز)
1. NewsletterSubscriptionController
2. NewsletterTemplateController
3. NewsletterCampaignController
4. NewsletterStatisticsController

### Phase 5: Admin Views (1 روز)
1. Subscription Management Views
2. Template Management Views
3. Campaign Management Views
4. Statistics Dashboard

### Phase 6: Public Controllers & Views (0.5 روز)
1. NewsletterController (Public)
2. Subscribe Form (Partial View)
3. Verify Page
4. Unsubscribe Page

### Phase 7: Advanced Features (1 روز)
1. Export/Import (Excel)
2. Statistics & Charts
3. Email Tracking
4. Background Job (Scheduled Campaigns)

### Phase 8: Testing & Optimization (1 روز)
1. Unit Tests
2. Integration Tests
3. UI/UX Optimization
4. Performance Optimization

---

## 📝 نکات مهم پیاده‌سازی

### 1. Double Opt-in
- ارسال ایمیل تایید پس از ثبت‌نام
- لینک تایید با Token یکتا
- فعال شدن اشتراک فقط پس از تایید

### 2. Email Tracking
- Tracking Pixel: `<img src="/Newsletter/TrackOpen?campaignId=X&recipientId=Y" width="1" height="1">`
- Click Tracking: Rewrite تمام لینک‌ها به `/Newsletter/TrackClick?campaignId=X&recipientId=Y&url=ENCODED_URL`

### 3. Background Job
- برای Scheduled Campaigns، می‌توان از Windows Task Scheduler استفاده کرد
- یا یک Background Service ساده با Timer

### 4. Export/Import
- استفاده از EPPlus یا ClosedXML برای Excel
- Validation کامل داده‌های Import

### 5. Security
- Token های Verification و Unsubscribe باید یکتا و غیرقابل حدس باشند
- استفاده از GUID یا Cryptographically Secure Random

---

## ✅ Checklist نهایی

- [ ] تمام Entity Models طراحی شده
- [ ] تمام Enums طراحی شده
- [ ] تمام ViewModels طراحی شده
- [ ] تمام Interfaces طراحی شده
- [ ] نقشه راه کامل شده
- [ ] آماده برای شروع پیاده‌سازی

**وضعیت:** ✅ تحلیل کامل انجام شد - آماده برای پیاده‌سازی

