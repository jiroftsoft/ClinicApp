# 🏛️ **Ground Rules & Project Conventions - ClinicApp**

**تاریخ ایجاد:** 2024  
**هدف:** فهرست قراردادهای پروژه و وضعیت رعایت آن‌ها

---

## 📋 **قراردادهای اصلی پروژه**

### **1️⃣ Stack & Technology**

| مورد | مقدار |
|-----|-------|
| Framework | ASP.NET MVC 5 |
| ORM | Entity Framework 6 |
| Database | SQL Server |
| .NET Version | .NET Framework 4.8 |
| Dependency Injection | Unity Container |
| Logging | Serilog |
| UI Framework | Bootstrap + jQuery |
| Date Picker | Persian DatePicker |
| Validation | FluentValidation |

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
   - ✅ View: `@Html.AntiForgeryToken()`
   - ✅ JS: هدر `RequestVerificationToken` در همه POSTها
   - ✅ Controller: `[ValidateAntiForgeryToken]` روی POSTها

2. **Zero-Cache (Medical Environment):**
   - ✅ Controller: `[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]`
   - ✅ Filter: `[NoCache]` روی Controllers
   - ✅ JS: پارامتر `_ts` برای ضدکش

3. **Authorization:**
   - ⚠️ **DEV_MODE=true**: فعلاً `[Authorize]` اختیاری است
   - ✅ **TODO برای PROD**: باید فعال شود

4. **Sensitive Data Masking:**
   - ✅ Serilog با Mask کردن کدملی/موبایل/توکن/شماره کارت

#### ⚠️ **وضعیت فعلی:**
- ✅ `ReceptionApiController` دارای `[ValidateAntiForgeryToken]` روی POSTها
- ✅ `ReceptionApiController` دارای `[OutputCache(NoStore = true)]`
- ✅ `reception-api.js` دارای Anti-Forgery header
- ⚠️ **نیاز به بررسی**: سایر Controllers

---

### **5️⃣ Financial Year Service (غیرقابل مذاکره)**

#### ✅ **قرارداد:**
- **هیچ مقدار سال مالی هاردکد نشود**
- **همیشه از `IFinancialYearService` استفاده شود**

#### ✅ **وضعیت فعلی:**
- ✅ `IFinancialYearService` موجود
- ✅ `DbFinancialYearService` پیاده‌سازی شده
- ✅ `ReceptionFacade` از `_financialYearService.GetCurrentYear()` استفاده می‌کند
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
      public string ErrorCode { get; set; }
      public List<string> ValidationErrors { get; set; }
  }
  ```

#### ⚠️ **وضعیت فعلی:**
- ✅ `ServiceResult<T>` موجود در `Helpers/ServiceResult.cs`
- ✅ `ReceptionApiController` همه پاسخ‌ها را `ServiceResult<T>` برمی‌گرداند
- ✅ `ReceptionFacade` همه متدها را `ServiceResult<T>` برمی‌گرداند
- ⚠️ **نیاز به بررسی**: سایر Controllers و Services

---

### **7️⃣ Logging & Observability**

#### ✅ **قرارداد:**
- **Logger**: Serilog
- **CorrelationId**: در هر درخواست
- **Masking**: کدملی، موبایل، توکن، شماره کارت
- **Levels**: Information (عملیات عادی), Warning (هشدار), Error (خطا), Fatal (فاجعه)

#### ⚠️ **وضعیت فعلی:**
- ✅ Serilog تنظیم شده
- ✅ `CorrelationIdFilter` موجود
- ✅ `ReceptionFacade` و `ReceptionApiController` از `ILogger` استفاده می‌کنند
- ⚠️ **نیاز به بررسی**: Masking در تمام لاگ‌ها

---

### **8️⃣ API Routing Conventions**

#### ✅ **قراردادها:**

1. **Attribute Routing:**
   - ✅ `RoutePrefix("api/v1/reception")` برای V2 API
   - ✅ `MapMvcAttributeRoutes()` در `RouteConfig.cs`

2. **Legacy Routes:**
   - ✅ `/Api/ReceptionApi/{action}` برای سازگاری عقب‌رو

3. **Fallback:**
   - ✅ JS Wrapper با fallback از `/api/v1/reception/*` به `/Api/ReceptionApi/*`

#### ⚠️ **وضعیت فعلی:**
- ✅ `RouteConfig.cs` دارای `MapMvcAttributeRoutes()`
- ✅ `RouteConfig.cs` دارای Legacy Route برای `ReceptionApi`
- ✅ `ReceptionApiController` دارای `[RoutePrefix("api/v1/reception")]`
- ✅ `reception-api.js` دارای fallback logic

---

### **9️⃣ Caching Policy**

#### ✅ **قرارداد:**
- **CACHING_POLICY=Do-Not-Implement (Clinical)**
- **کش عملیاتی پیاده‌سازی نشود**
- **فقط Cache-Control: no-cache**

#### ⚠️ **وضعیت فعلی:**
- ✅ هیچ کش عملیاتی پیاده‌سازی نشده
- ✅ Zero-Cache روی تمام پاسخ‌ها

---

### **🔟 NO_DELETE Policy**

#### ✅ **قرارداد:**
- **حذف قطعی کد/فایل ممنوع**
- **فقط Patch/Refactor اتمیک**
- **Legacy/Obsolete code علامت‌گذاری شود**

#### ⚠️ **وضعیت فعلی:**
- ✅ هیچ فایلی حذف نشده
- ✅ Legacy Controllers موجود هستند

---

## 📊 **خلاصه وضعیت رعایت قراردادها**

| قرارداد | وضعیت | توضیحات |
|---------|-------|---------|
| Money = decimal(18,0) | ⚠️ نیاز به بررسی | اکثر موجودیت‌ها OK، نیاز به ممیزی جامع |
| EF6 IndexAnnotation | ⚠️ نیاز به بررسی | برخی موجودیت‌ها دارای Index، نیاز به بررسی جامع |
| EF6 RowVersion | ⚠️ نیاز به بررسی | Reception/ReceptionItem OK، سایر موجودیت‌ها نیاز به بررسی |
| SoftDelete/Auditing | ✅ OK | ISoftDelete/ITrackable موجود و استفاده می‌شود |
| Anti-Forgery | ✅ OK | ReceptionApiController کامل، سایر Controllers نیاز به بررسی |
| Zero-Cache | ✅ OK | ReceptionApiController و Filters OK |
| Financial Year Service | ⚠️ نیاز به بررسی | ReceptionFacade OK، سایر سرویس‌ها نیاز به بررسی |
| ServiceResult Pattern | ⚠️ نیاز به بررسی | ReceptionApiController/Facade OK، سایر Controllers نیاز به بررسی |
| Logging (Serilog) | ✅ OK | تنظیم شده و استفاده می‌شود |
| CorrelationId | ✅ OK | CorrelationIdFilter موجود |
| Attribute Routing | ✅ OK | RouteConfig و ReceptionApiController OK |
| Legacy Routes | ✅ OK | RouteConfig دارای Legacy Routes |
| No-Cache Policy | ✅ OK | هیچ کش عملیاتی پیاده نشده |
| NO_DELETE Policy | ✅ OK | هیچ فایلی حذف نشده |

---

## 🎯 **اقدامات لازم**

### **اولویت بالا:**
1. ✅ **ReceptionApiController**: کامل شده
2. ⚠️ **بررسی سایر Controllers**: اضافه کردن Anti-Forgery و Zero-Cache
3. ⚠️ **بررسی Financial Year**: حذف هاردکدها و استفاده از Service
4. ⚠️ **بررسی Money Fields**: اطمینان از decimal(18,0) در همه موجودیت‌ها

### **اولویت متوسط:**
5. ⚠️ **بررسی ServiceResult**: اطمینان از استفاده یکپارچه
6. ⚠️ **بررسی EF6 IndexAnnotation**: افزودن ایندکس‌های لازم
7. ⚠️ **بررسی RowVersion**: افزودن به موجودیت‌های حساس

### **اولویت پایین:**
8. ⚠️ **پاکسازی Legacy**: علامت‌گذاری Legacy Controllers
9. ⚠️ **بهینه‌سازی**: بررسی N+1 queries و performance

---

**تاریخ به‌روزرسانی:** 2024  
**نسخه:** 1.0

