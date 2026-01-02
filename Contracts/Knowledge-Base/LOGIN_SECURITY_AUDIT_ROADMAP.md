# 🔐 Login Security & Audit Trail - Roadmap & Implementation Plan

**تاریخ:** 2025-01-XX  
**وضعیت:** 🟡 **در حال بررسی**  
**اولویت:** 🔴 **CRITICAL** - برای محیط Production درمانی

---

## 📊 وضعیت فعلی (Current State Analysis)

### ✅ **موجود (Implemented)**
1. **LastLoginDate** در `ApplicationUser`
   - فقط تاریخ آخرین ورود را ذخیره می‌کند
   - ❌ IP Address ندارد
   - ❌ UserAgent ندارد
   - ❌ تاریخچه کامل ندارد

2. **OtpRequest Table**
   - ✅ OTP requests را لاگ می‌کند
   - ✅ PhoneNumber, OtpCodeHash, RequestTime
   - ✅ IsVerified flag
   - ❌ IP Address ندارد
   - ❌ UserAgent ندارد
   - ❌ فقط OTP requests را لاگ می‌کند، نه login history

3. **SecurityLogger** (Serilog)
   - ✅ Login attempts را لاگ می‌کند
   - ✅ IP Address و UserAgent را می‌گیرد
   - ❌ فقط در فایل لاگ (Serilog) ذخیره می‌شود
   - ❌ در Database ذخیره نمی‌شود
   - ❌ Query و Report نمی‌توان گرفت

4. **OtpState** (In-Memory)
   - ✅ IP Address و UserAgent را ذخیره می‌کند
   - ❌ فقط در Memory است
   - ❌ بعد از Expiry پاک می‌شود
   - ❌ برای Audit Trail مناسب نیست

### ❌ **مفقود (Missing)**
1. **UserLoginHistory Table**
   - ❌ جدول برای ذخیره تاریخچه کامل ورودها
   - ❌ IP Address tracking
   - ❌ UserAgent tracking
   - ❌ Device/Browser detection
   - ❌ Location detection (optional)
   - ❌ Failed login attempts
   - ❌ Success/failure status

2. **Login History Service**
   - ❌ Service برای ثبت login history
   - ❌ Service برای query login history
   - ❌ Service برای گزارش‌گیری

3. **Integration در AuthService**
   - ❌ SignInUserAsync لاگ نمی‌کند
   - ❌ Failed login attempts لاگ نمی‌شود
   - ❌ SecurityLogger استفاده نمی‌شود

---

## 🎯 الگوهای روتین جهانی (Industry Best Practices)

### 1. **Login History Table Structure**
```sql
UserLoginHistory
- Id (PK)
- UserId (FK)
- LoginTime (DateTime)
- LogoutTime (DateTime, nullable)
- IpAddress (string, 50)
- UserAgent (string, 500)
- DeviceType (string, 50) -- Mobile, Desktop, Tablet
- BrowserName (string, 50) -- Chrome, Firefox, Safari
- BrowserVersion (string, 20)
- OSName (string, 50) -- Windows, iOS, Android
- OSVersion (string, 20)
- Location (string, 100) -- Optional: City, Country
- IsSuccessful (bool)
- FailureReason (string, 200) -- Optional: Invalid OTP, Account Locked, etc.
- SessionId (string, 128) -- ASP.NET Session ID
- CreatedAt (DateTime)
```

### 2. **Security Best Practices**
- ✅ **IP Tracking**: برای تشخیص suspicious activity
- ✅ **UserAgent Tracking**: برای تشخیص device fingerprinting
- ✅ **Device Detection**: برای تشخیص تغییر device
- ✅ **Location Tracking** (Optional): برای تشخیص تغییر location
- ✅ **Session Tracking**: برای ردیابی session ها
- ✅ **Failed Attempts Logging**: برای تشخیص brute force
- ✅ **Success/Failure Status**: برای گزارش‌گیری

### 3. **Audit Trail Requirements**
- ✅ **Immutable Logs**: لاگ‌ها نباید تغییر کنند
- ✅ **Retention Policy**: نگهداری لاگ‌ها برای مدت مشخص (مثلاً 1 سال)
- ✅ **Indexing**: Indexes برای query های سریع
- ✅ **Archiving**: آرشیو لاگ‌های قدیمی
- ✅ **Compliance**: رعایت قوانین حریم خصوصی (GDPR, etc.)

### 4. **Security Monitoring**
- ✅ **Anomaly Detection**: تشخیص ورود از IP/Device جدید
- ✅ **Rate Limiting**: محدودیت تعداد تلاش‌های ورود
- ✅ **Account Lockout**: قفل حساب بعد از تلاش‌های ناموفق
- ✅ **Alert System**: هشدار برای suspicious activity

---

## 📋 TODO List - Implementation Plan

### **Phase 1: Database & Entity (2-3 روز)**

#### **Task 1.1: UserLoginHistory Entity** 🔴 CRITICAL
- [ ] ایجاد `Models/Entities/Security/UserLoginHistory.cs`
- [ ] ایجاد Configuration Class
- [ ] ایجاد Migration
- [ ] اضافه کردن DbSet به ApplicationDbContext
- [ ] ایجاد Indexes (UserId, LoginTime, IpAddress, IsSuccessful)

**Entity Structure:**
```csharp
public class UserLoginHistory
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public string DeviceType { get; set; } // Mobile, Desktop, Tablet
    public string BrowserName { get; set; }
    public string BrowserVersion { get; set; }
    public string OSName { get; set; }
    public string OSVersion { get; set; }
    public string Location { get; set; } // Optional
    public bool IsSuccessful { get; set; }
    public string FailureReason { get; set; }
    public string SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public virtual ApplicationUser User { get; set; }
}
```

#### **Task 1.2: Device Detection Helper** 🟡 HIGH
- [ ] ایجاد `Helpers/Security/DeviceDetectionHelper.cs`
- [ ] Parse UserAgent برای Browser, OS, Device
- [ ] استفاده از library مثل `UAParser` (optional)

---

### **Phase 2: Service Layer (2-3 روز)**

#### **Task 2.1: LoginHistoryService** 🔴 CRITICAL
- [ ] ایجاد `Services/Security/LoginHistoryService.cs`
- [ ] ایجاد `Interfaces/Security/ILoginHistoryService.cs`
- [ ] متد `LogLoginAsync()` - ثبت ورود موفق
- [ ] متد `LogFailedLoginAsync()` - ثبت ورود ناموفق
- [ ] متد `LogLogoutAsync()` - ثبت خروج
- [ ] متد `GetUserLoginHistoryAsync()` - دریافت تاریخچه کاربر
- [ ] متد `GetRecentLoginsAsync()` - دریافت ورودهای اخیر
- [ ] متد `GetSuspiciousActivityAsync()` - تشخیص فعالیت مشکوک
- [ ] متد `GetLoginStatisticsAsync()` - آمار ورودها

#### **Task 2.2: ClientInfoProvider Enhancement** 🟡 HIGH
- [ ] بررسی `IClientInfoProvider`
- [ ] اضافه کردن Device Detection
- [ ] اضافه کردن Location Detection (optional)

---

### **Phase 3: Integration (1-2 روز)**

#### **Task 3.1: AuthService Integration** 🔴 CRITICAL
- [ ] Inject `ILoginHistoryService` در `AuthService`
- [ ] Call `LogLoginAsync()` در `SignInUserAsync()`
- [ ] Call `LogFailedLoginAsync()` در `VerifyLoginOtpAndSignInAsync()` (on failure)
- [ ] Call `LogFailedLoginAsync()` در `SendLoginOtpAsync()` (on account lockout)
- [ ] استفاده از `SecurityLogger` برای Serilog logging

#### **Task 3.2: Logout Integration** 🟡 MEDIUM
- [ ] ایجاد `Logout()` action در `AccountController`
- [ ] Call `LogLogoutAsync()` قبل از SignOut

---

### **Phase 4: UI & Reporting (2-3 روز)**

#### **Task 4.1: Admin Login History View** 🟡 HIGH
- [ ] ایجاد `Areas/Admin/Controllers/LoginHistoryController.cs`
- [ ] ایجاد `Areas/Admin/Views/LoginHistory/Index.cshtml`
- [ ] فیلتر بر اساس User, Date Range, IP, Status
- [ ] DataTables برای نمایش
- [ ] Export به Excel

#### **Task 4.2: User Profile Login History** 🟢 LOW
- [ ] ایجاد Partial View برای نمایش تاریخچه ورود کاربر
- [ ] نمایش در User Profile
- [ ] محدود به 10-20 ورود اخیر

#### **Task 4.3: Security Dashboard** 🟡 MEDIUM
- [ ] ایجاد Dashboard برای نمایش:
  - تعداد ورودهای امروز
  - ورودهای مشکوک
  - IP های جدید
  - Device های جدید
  - Failed attempts

---

### **Phase 5: Security Enhancements (2-3 روز)**

#### **Task 5.1: Anomaly Detection** 🟡 HIGH
- [ ] تشخیص IP جدید
- [ ] تشخیص Device جدید
- [ ] تشخیص Location جدید (optional)
- [ ] Alert برای suspicious activity

#### **Task 5.2: Rate Limiting Enhancement** 🟡 HIGH
- [ ] بررسی `IRateLimiter`
- [ ] اضافه کردن IP-based rate limiting
- [ ] اضافه کردن User-based rate limiting
- [ ] Logging rate limit violations

#### **Task 5.3: Account Lockout Enhancement** 🟡 HIGH
- [ ] بررسی Account Lockout logic
- [ ] اضافه کردن Lockout reason
- [ ] اضافه کردن Lockout duration
- [ ] Logging lockout events

---

### **Phase 6: Testing & Documentation (1-2 روز)**

#### **Task 6.1: Unit Tests** 🟡 MEDIUM
- [ ] Tests برای `LoginHistoryService`
- [ ] Tests برای `DeviceDetectionHelper`
- [ ] Tests برای Integration

#### **Task 6.2: Integration Tests** 🟡 MEDIUM
- [ ] Test complete login flow
- [ ] Test failed login flow
- [ ] Test logout flow
- [ ] Test anomaly detection

#### **Task 6.3: Documentation** 🟢 LOW
- [ ] مستندسازی API
- [ ] مستندسازی Database Schema
- [ ] مستندسازی Security Features

---

## 🔒 Security Patterns Implementation

### **1. IP Address Tracking**
```csharp
// در LoginHistoryService
private string GetClientIpAddress()
{
    var request = HttpContext.Current?.Request;
    if (request == null) return "Unknown";
    
    // Check for forwarded IP (behind proxy/load balancer)
    var forwardedIp = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
    if (!string.IsNullOrEmpty(forwardedIp))
    {
        return forwardedIp.Split(',')[0].Trim();
    }
    
    return request.ServerVariables["REMOTE_ADDR"] ?? "Unknown";
}
```

### **2. UserAgent Parsing**
```csharp
// استفاده از UAParser (NuGet Package)
var parser = Parser.GetDefault();
var clientInfo = parser.Parse(userAgent);

var deviceType = clientInfo.Device.Family; // Mobile, Desktop, Tablet
var browserName = clientInfo.UA.Family; // Chrome, Firefox, Safari
var browserVersion = $"{clientInfo.UA.Major}.{clientInfo.UA.Minor}";
var osName = clientInfo.OS.Family; // Windows, iOS, Android
var osVersion = $"{clientInfo.OS.Major}.{clientInfo.OS.Minor}";
```

### **3. Anomaly Detection**
```csharp
public async Task<bool> IsSuspiciousActivityAsync(string userId, string ipAddress, string userAgent)
{
    // Check if IP is new for this user
    var previousLogins = await _context.UserLoginHistories
        .Where(l => l.UserId == userId && l.IsSuccessful)
        .OrderByDescending(l => l.LoginTime)
        .Take(10)
        .ToListAsync();
    
    // IP never seen before
    if (!previousLogins.Any(l => l.IpAddress == ipAddress))
    {
        return true;
    }
    
    // Device never seen before
    var deviceFingerprint = GetDeviceFingerprint(userAgent);
    if (!previousLogins.Any(l => GetDeviceFingerprint(l.UserAgent) == deviceFingerprint))
    {
        return true;
    }
    
    return false;
}
```

### **4. Session Tracking**
```csharp
// در SignInUserAsync
var sessionId = HttpContext.Current?.Session?.SessionID;
await _loginHistoryService.LogLoginAsync(
    userId: user.Id,
    ipAddress: GetClientIpAddress(),
    userAgent: GetUserAgent(),
    sessionId: sessionId,
    isSuccessful: true
);
```

---

## 📊 Database Schema

### **UserLoginHistory Table**
```sql
CREATE TABLE [dbo].[UserLoginHistories] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] NVARCHAR(128) NOT NULL,
    [LoginTime] DATETIME2 NOT NULL,
    [LogoutTime] DATETIME2 NULL,
    [IpAddress] NVARCHAR(50) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [DeviceType] NVARCHAR(50) NULL,
    [BrowserName] NVARCHAR(50) NULL,
    [BrowserVersion] NVARCHAR(20) NULL,
    [OSName] NVARCHAR(50) NULL,
    [OSVersion] NVARCHAR(20) NULL,
    [Location] NVARCHAR(100) NULL,
    [IsSuccessful] BIT NOT NULL DEFAULT(1),
    [FailureReason] NVARCHAR(200) NULL,
    [SessionId] NVARCHAR(128) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
    
    CONSTRAINT [FK_UserLoginHistories_AspNetUsers] 
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
);

-- Indexes
CREATE INDEX [IX_UserLoginHistories_UserId] ON [dbo].[UserLoginHistories]([UserId]);
CREATE INDEX [IX_UserLoginHistories_LoginTime] ON [dbo].[UserLoginHistories]([LoginTime]);
CREATE INDEX [IX_UserLoginHistories_IpAddress] ON [dbo].[UserLoginHistories]([IpAddress]);
CREATE INDEX [IX_UserLoginHistories_IsSuccessful] ON [dbo].[UserLoginHistories]([IsSuccessful]);
CREATE INDEX [IX_UserLoginHistories_UserId_LoginTime] ON [dbo].[UserLoginHistories]([UserId], [LoginTime]);
```

---

## 🎯 Success Criteria

### **Functional Requirements**
- ✅ تمام ورودهای موفق در Database ذخیره می‌شوند
- ✅ تمام تلاش‌های ناموفق در Database ذخیره می‌شوند
- ✅ IP Address و UserAgent برای هر ورود ذخیره می‌شوند
- ✅ Device/Browser/OS detection کار می‌کند
- ✅ Admin می‌تواند تاریخچه ورودها را ببیند
- ✅ User می‌تواند تاریخچه ورود خود را ببیند
- ✅ Anomaly detection کار می‌کند

### **Non-Functional Requirements**
- ✅ Performance: Query ها باید سریع باشند (< 500ms)
- ✅ Scalability: باید برای 100K+ users کار کند
- ✅ Security: لاگ‌ها immutable هستند
- ✅ Compliance: رعایت قوانین حریم خصوصی

---

## 📝 Notes

1. **Privacy Considerations:**
   - IP Address و UserAgent داده‌های حساس هستند
   - باید Retention Policy داشته باشیم
   - باید GDPR compliance داشته باشیم

2. **Performance Considerations:**
   - Indexes برای query های سریع
   - Archiving برای لاگ‌های قدیمی
   - Partitioning برای جداول بزرگ (optional)

3. **Security Considerations:**
   - لاگ‌ها نباید تغییر کنند
   - باید Access Control داشته باشیم
   - باید Audit Trail برای تغییرات لاگ‌ها داشته باشیم

---

**Last Updated:** 2025-01-XX  
**Status:** 🟡 **In Planning**

