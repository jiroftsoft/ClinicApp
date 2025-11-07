# 🏛️ **Ground Rules & Project Conventions - Reception V2 Focus**

**تاریخ ایجاد:** 2025-01-27  
**هدف:** فهرست قراردادهای پروژه و وضعیت رعایت آن‌ها با تمرکز بر ماژول Reception V2  
**نسخه:** 2.0.0

---

## 📋 **قراردادهای اصلی پروژه**

### **1️⃣ Stack & Technology**

| مورد | مقدار | وضعیت |
|-----|-------|-------|
| Framework | ASP.NET MVC 5 | ✅ |
| ORM | Entity Framework 6 | ✅ |
| Database | SQL Server | ✅ |
| .NET Version | .NET Framework 4.8 | ✅ |
| Dependency Injection | Unity Container | ✅ |
| Logging | Serilog | ✅ |
| UI Framework | Bootstrap + jQuery | ✅ |
| Date Picker | Persian DatePicker | ✅ |
| Validation | FluentValidation | ✅ |

---

### **2️⃣ Money/Decimal Precision (غیرقابل مذاکره)**

#### ✅ **قرارداد:**
- **همه مبالغ IRR**: `decimal(18,0)` (بدون اعشار)
- **مثال:**
  ```csharp
  public decimal TotalAmount { get; set; } // باید decimal(18,0) باشد
  public decimal UnitPrice { get; set; }   // باید decimal(18,0) باشد
  public decimal PatientCoPay { get; set; } // باید decimal(18,0) باشد
  ```

#### ⚠️ **وضعیت فعلی:**
- ✅ `Reception.TotalAmount` → `decimal(18,0)`
- ✅ `ReceptionItem.UnitPrice` → `decimal(18,0)`
- ✅ `ReceptionItem.TotalPrice` → `decimal(18,0)`
- ✅ `PaymentTransaction.Amount` → `decimal(18,0)`
- ⚠️ **نیاز به بررسی**: سایر موجودیت‌ها (Insurance, FactorSetting, ...)

---

### **3️⃣ EF6 Conventions**

#### ✅ **قراردادها:**

1. **IndexAnnotation** برای ایندکس‌های DB:
   ```csharp
   [Index("IX_Reception_PatientId_Date", IsClustered = false)]
   public int PatientId { get; set; }
   ```

2. **RowVersion** برای Concurrency:
   ```csharp
   [Timestamp]
   public byte[] RowVersion { get; set; }
   ```

3. **SoftDelete** با `ISoftDelete`:
   ```csharp
   public bool IsDeleted { get; set; }
   ```

4. **Auditing** با `ITrackable`:
   ```csharp
   public string CreatedByUserId { get; set; }
   public DateTime CreatedAt { get; set; }
   public string UpdatedByUserId { get; set; }
   public DateTime? UpdatedAt { get; set; }
   ```

#### ⚠️ **وضعیت فعلی:**
- ✅ `ISoftDelete` و `ITrackable` موجود
- ✅ `ApplicationDbContext` با Dynamic Filters برای SoftDelete
- ✅ `Reception` و `ReceptionItem` دارای `RowVersion`
- ⚠️ **نیاز به بررسی**: سایر موجودیت‌ها

---

### **4️⃣ Security Requirements**

#### ✅ **قراردادها:**

1. **Anti-Forgery Token:**
   - ✅ View: `@Html.AntiForgeryToken()` در `Views/ReceptionV2/Index.cshtml`
   - ✅ JS: هدر `RequestVerificationToken` (یا `X-RequestVerificationToken`) در همه POSTها
   - ✅ Controller: `[ValidateAntiForgeryTokenOnPosts]` روی POSTها
   - ✅ Filter: `ValidateAntiForgeryTokenOnPostsAttribute` با پاسخ JSON 400 و کد `ANTIFORGERY_MISSING`

2. **Zero-Cache (Medical Environment):**
   - ✅ Controller: `[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]`
   - ✅ Filter: `[NoCache]` روی Controllers
   - ✅ JS: پارامتر `_ts` برای ضدکش

3. **Authorization:**
   - ⚠️ **DEV_MODE=true**: فعلاً `[Authorize]` اختیاری است
   - ✅ **TODO برای PROD**: باید فعال شود

4. **Sensitive Data Masking:**
   - ✅ Serilog با Mask کردن کدملی/موبایل/توکن/شماره کارت

#### ⚠️ **وضعیت فعلی - Reception V2:**
- ✅ `ReceptionApiV1Controller` دارای `[OutputCache(NoStore = true)]`
- ✅ `ReceptionApiV1Controller` دارای `[ValidateAntiForgeryTokenOnPosts]` روی POSTها
- ✅ `Views/ReceptionV2/Index.cshtml` دارای `@Html.AntiForgeryToken()`
- ✅ `Scripts/reception.v2/reception-api.js` دارای Anti-Forgery header
- ✅ `Filters/ValidateAntiForgeryTokenOnPostsAttribute.cs` با پاسخ JSON 400
- ⚠️ **نیاز به بررسی**: سایر Controllers

---

### **5️⃣ Financial Year Service (غیرقابل مذاکره)**

#### ✅ **قرارداد:**
- **هیچ مقدار سال مالی هاردکد نشود**
- **همیشه از `IFinancialYearService` استفاده شود**

#### ✅ **وضعیت فعلی - Reception V2:**
- ✅ `IFinancialYearService` موجود
- ✅ `DbFinancialYearService` پیاده‌سازی شده
- ✅ `ReceptionFacade` از `_financialYearService.GetCurrentYear()` استفاده می‌کند
- ✅ `ReceptionApiV1Controller` از `IFinancialYearService` استفاده می‌کند
- ✅ `ReceptionV2Controller` از `IFinancialYearService` استفاده می‌کند
- ⚠️ **نیاز به بررسی**: سایر سرویس‌ها (ServiceCalculationService, InsuranceCalculationService, ...)

---

### **6️⃣ ServiceResult Pattern**

#### ✅ **قرارداد:**
- **همه خروجی‌های API**: `ServiceResult<T>` یا `ServiceResult`
- **ساختار:**
  ```csharp
  public class ServiceResult<T>
  {
      public bool Success { get; set; }
      public T Data { get; set; }
      public string Message { get; set; }
      public string Code { get; set; }
      public ErrorCategory? Category { get; set; }
      public SecurityLevel? SecurityLevel { get; set; }
      public List<string> ValidationErrors { get; set; }
      public Dictionary<string, object> Metadata { get; set; }
  }
  ```

#### ✅ **Extensions:**
- ✅ `WithExceptionDev(this ServiceResult result, Exception ex)` - افزودن جزئیات Exception در Dev
- ✅ `WithCode(this ServiceResult result, string code)` - افزودن کد خطا

#### ⚠️ **وضعیت فعلی - Reception V2:**
- ✅ `ServiceResult<T>` موجود در `Helpers/ServiceResult.cs`
- ✅ `ServiceResultExtensions` موجود در `Helpers/ServiceResultExtensions.cs`
- ✅ `ReceptionApiV1Controller` همه پاسخ‌ها را `ServiceResult<T>` برمی‌گرداند
- ✅ `ReceptionFacade` همه متدها را `ServiceResult<T>` برمی‌گرداند
- ✅ `ReceptionPricingService` همه متدها را `ServiceResult<T>` برمی‌گرداند
- ⚠️ **نیاز به بررسی**: سایر Controllers و Services

---

### **7️⃣ Logging & Observability**

#### ✅ **قرارداد:**
- **Logger**: Serilog
- **CorrelationId**: در هر درخواست
- **Masking**: کدملی، موبایل، توکن، شماره کارت
- **Levels**: Information (عملیات عادی), Warning (هشدار), Error (خطا), Fatal (فاجعه)
- **Dev vs Prod**: در Dev شامل Exception/StackTrace/Source، در Prod مینیمال

#### ⚠️ **وضعیت فعلی - Reception V2:**
- ✅ Serilog تنظیم شده
- ✅ `CorrelationIdFilter` موجود
- ✅ `ReceptionFacade` و `ReceptionApiV1Controller` از `ILogger` استفاده می‌کنند
- ✅ `ReceptionPricingService` از `ILogger` استفاده می‌کند
- ✅ `WithExceptionDev` برای افزودن جزئیات در Dev
- ⚠️ **نیاز به بررسی**: Masking در تمام لاگ‌ها

---

### **8️⃣ API Routing Conventions**

#### ✅ **قراردادها:**

1. **Attribute Routing:**
   - ✅ `RoutePrefix("api/v1/reception")` برای `ReceptionApiV1Controller`
   - ✅ `MapMvcAttributeRoutes()` در `RouteConfig.cs`
   - ✅ `[Route("bootstrap")]`, `[Route("draft/create")]`, `[Route("item/add")]`, ...

2. **Legacy Routes:**
   - ✅ `/Api/ReceptionApi/{action}` برای سازگاری عقب‌رو
   - ✅ `RouteConfig.cs` دارای Legacy Route برای `ReceptionApi`

3. **Fallback:**
   - ✅ JS Wrapper با fallback از `/api/v1/reception/*` به `/Api/ReceptionApi/*`

#### ⚠️ **وضعیت فعلی - Reception V2:**
- ✅ `RouteConfig.cs` دارای `MapMvcAttributeRoutes()`
- ✅ `RouteConfig.cs` دارای Legacy Route برای `ReceptionApi`
- ✅ `ReceptionApiV1Controller` دارای `[RoutePrefix("api/v1/reception")]`
- ✅ `reception-api.js` دارای fallback logic
- ✅ `ReceptionV2Controller` دارای `[RoutePrefix("ReceptionV2")]`

---

### **9️⃣ Caching Policy**

#### ✅ **قرارداد:**
- **CACHING_POLICY=Do-Not-Implement (Clinical)**
- **کش عملیاتی پیاده‌سازی نشود**
- **فقط Cache-Control: no-cache**

#### ⚠️ **وضعیت فعلی - Reception V2:**
- ✅ هیچ کش عملیاتی پیاده‌سازی نشده
- ✅ Zero-Cache روی تمام پاسخ‌ها
- ✅ `[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]` روی `ReceptionApiV1Controller`
- ✅ `[NoCache]` روی `ReceptionV2Controller`

---

### **🔟 NO_DELETE Policy**

#### ✅ **قرارداد:**
- **حذف قطعی کد/فایل ممنوع**
- **فقط Patch/Refactor اتمیک**
- **Legacy/Obsolete code علامت‌گذاری شود**

#### ⚠️ **وضعیت فعلی:**
- ✅ هیچ فایلی حذف نشده
- ✅ Legacy Controllers موجود هستند (`ReceptionApiController`, `ReceptionController`)

---

### **1️⃣1️⃣ Dependency Injection Conventions**

#### ✅ **قراردادها:**

1. **Unity Container:**
   - ✅ همه سرویس‌ها و Repository ها در `UnityConfig.cs` ثبت شوند
   - ✅ Lifetime Management: `PerRequestLifetimeManager` برای Services/Repositories
   - ✅ Interface Segregation: همه سرویس‌ها دارای Interface

2. **Constructor Injection:**
   - ✅ همه وابستگی‌ها از طریق Constructor تزریق شوند
   - ✅ Fallback Constructor برای سازگاری با Legacy

#### ⚠️ **وضعیت فعلی - Reception V2:**
- ✅ `IReceptionFacade` → `ReceptionFacade` در `UnityConfig.cs`
- ✅ `IReceptionPricingService` → `ReceptionPricingService` در `UnityConfig.cs`
- ✅ `IPosManagementService` → `PosManagementService` در `UnityConfig.cs`
- ✅ `IPosTerminalRepository` → `PosTerminalRepository` در `UnityConfig.cs`
- ✅ `IPaymentTransactionRepository` → `PaymentTransactionRepository` در `UnityConfig.cs`
- ✅ `ILogger` → `Serilog.ILogger` در `UnityConfig.cs`
- ⚠️ **نیاز به بررسی**: سایر سرویس‌های Reception V2

---

### **1️⃣2️⃣ Error Handling Conventions**

#### ✅ **قراردادها:**

1. **Error Codes:**
   - ✅ کدهای خطا یکتا: `INSURANCE_SET_MISSING`, `PRICING_RECALCULATED`, `DOCTOR_NOT_IN_DEPARTMENT`, `AMOUNT_ZERO`, `DUPLICATE_PAYMENT`, `POS_NOT_AVAILABLE`, `ANTIFORGERY_MISSING`
   - ✅ کدهای خطا در `Helpers/ReceptionApiCodes.cs` تعریف شوند

2. **Error Messages:**
   - ✅ پیام‌های خطا فارسی برای کاربر
   - ✅ پیام‌های خطا شامل Metadata در Dev
   - ✅ پیام‌های خطا مینیمال در Prod

3. **Exception Handling:**
   - ✅ `GlobalExceptionFilter` برای مدیریت خطاهای سراسری
   - ✅ `try-catch` در همه متدهای API
   - ✅ `WithExceptionDev` برای افزودن جزئیات در Dev

#### ⚠️ **وضعیت فعلی - Reception V2:**
- ✅ `Helpers/ReceptionApiCodes.cs` موجود
- ✅ `GlobalExceptionFilter` موجود
- ✅ `ReceptionApiV1Controller` دارای `try-catch` در همه متدها
- ✅ `ReceptionFacade` دارای `try-catch` در همه متدها
- ✅ `WithExceptionDev` برای افزودن جزئیات در Dev
- ⚠️ **نیاز به بررسی**: پیام‌های خطا در سایر Controllers

---

## 📊 **خلاصه وضعیت رعایت قراردادها - Reception V2**

| قرارداد | وضعیت | توضیحات |
|---------|-------|---------|
| Money = decimal(18,0) | ✅ OK | Reception/ReceptionItem/PaymentTransaction OK |
| EF6 IndexAnnotation | ⚠️ نیاز به بررسی | برخی موجودیت‌ها دارای Index، نیاز به بررسی جامع |
| EF6 RowVersion | ✅ OK | Reception/ReceptionItem دارای RowVersion |
| SoftDelete/Auditing | ✅ OK | ISoftDelete/ITrackable موجود و استفاده می‌شود |
| Anti-Forgery | ✅ OK | ReceptionApiV1Controller و Views کامل |
| Zero-Cache | ✅ OK | ReceptionApiV1Controller و Filters OK |
| Financial Year Service | ✅ OK | ReceptionFacade و Controllers از Service استفاده می‌کنند |
| ServiceResult Pattern | ✅ OK | ReceptionApiV1Controller/Facade/PricingService OK |
| Logging (Serilog) | ✅ OK | تنظیم شده و استفاده می‌شود |
| CorrelationId | ✅ OK | CorrelationIdFilter موجود |
| Attribute Routing | ✅ OK | RouteConfig و ReceptionApiV1Controller OK |
| Legacy Routes | ✅ OK | RouteConfig دارای Legacy Routes |
| No-Cache Policy | ✅ OK | هیچ کش عملیاتی پیاده نشده |
| NO_DELETE Policy | ✅ OK | هیچ فایلی حذف نشده |
| Dependency Injection | ✅ OK | UnityConfig دارای ثبت‌های لازم |
| Error Handling | ✅ OK | GlobalExceptionFilter و WithExceptionDev OK |

---

## 🎯 **اقدامات لازم - Reception V2**

### **اولویت بالا:**
1. ✅ **ReceptionApiV1Controller**: کامل شده
2. ✅ **ReceptionFacade**: کامل شده
3. ✅ **ReceptionPricingService**: کامل شده
4. ⚠️ **بررسی Bootstrap Endpoint**: اطمینان از وجود `PosTerminals` و `DefaultPosTerminalId`
5. ⚠️ **بررسی Pricing Endpoints**: اطمینان از Reprice-on-change

### **اولویت متوسط:**
6. ⚠️ **بررسی Doctor/Department Filters**: اطمینان از Validation
7. ⚠️ **بررسی POS Payment**: اطمینان از وجود `IPosProviderResolver` و `IPosProviderClient`
8. ⚠️ **بررسی Error Messages**: اطمینان از پیام‌های خطای یکتا

### **اولویت پایین:**
9. ⚠️ **بهینه‌سازی**: بررسی N+1 queries و performance
10. ⚠️ **پاکسازی Legacy**: علامت‌گذاری Legacy Controllers

---

## 🔍 **نقاط نیازمند بررسی - Reception V2**

### **1. Bootstrap Endpoint (فاز C)**
- ⚠️ بررسی کامل بودن `GET /api/v1/reception/bootstrap`
- ⚠️ بررسی وجود `PosTerminals` و `DefaultPosTerminalId` در پاسخ
- ⚠️ بررسی Lazy Loading برای Doctors

### **2. Pricing Endpoints (فاز D)**
- ⚠️ بررسی کامل بودن `POST /api/v1/reception/insurances/set` با `totals` و `pricings[]`
- ⚠️ بررسی کامل بودن `POST /api/v1/reception/item/add` با `pricing` و `totals`
- ⚠️ بررسی کامل بودن `POST /api/v1/reception/item/update` با Reprice

### **3. Doctor/Department Filters (فاز E)**
- ⚠️ بررسی وجود `GET /api/v1/reception/doctors/by-department?deptId=`
- ⚠️ بررسی وجود `GET /api/v1/reception/doctors/by-service?deptId=&serviceId=`
- ⚠️ بررسی Validation برای مجاز بودن پزشک برای خدمت

### **4. POS Payment (فاز F)**
- ⚠️ بررسی وجود `IPosProviderResolver` و `IPosProviderClient`
- ⚠️ بررسی وجود `FakePosClient` برای تست
- ⚠️ بررسی وجود `PosPaymentService` برای مدیریت پرداخت POS
- ⚠️ بررسی وجود `pos-payment.js` برای مدیریت پرداخت POS

---

**تاریخ به‌روزرسانی:** 2025-01-27  
**نسخه:** 2.0.0  
**وضعیت:** ✅ فاز A تکمیل شد
