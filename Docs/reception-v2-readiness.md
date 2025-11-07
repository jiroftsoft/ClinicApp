# ✅ **چک‌لیست آمادگی Reception V2 - Route/DI/CSRF**

**تاریخ ایجاد:** 2025-01-27  
**هدف:** بررسی صحت Route/DI/CSRF برای ماژول Reception V2  
**نسخه:** 1.0.0

---

## 📋 **خلاصه اجرایی**

| مورد | وضعیت | توضیحات |
|-----|-------|---------|
| **Route Configuration** | ✅ OK | Attribute routing فعال و Legacy fallback موجود |
| **Dependency Injection** | ✅ OK | تمام وابستگی‌های لازم در Unity ثبت شده‌اند |
| **CSRF Protection** | ✅ OK | Anti-Forgery Token در View و JS و Filter کامل است |
| **Error Handling** | ✅ OK | پاسخ JSON 400 با کد ANTIFORGERY_MISSING در Dev |

---

## 1️⃣ **Route Configuration**

### **1.1 Attribute Routing**

#### ✅ **وضعیت: OK**

**بررسی:**
- ✅ `App_Start/RouteConfig.cs` دارای `routes.MapMvcAttributeRoutes()` (خط 17)
- ✅ `Controllers/Api/ReceptionApiV1Controller.cs` دارای `[RoutePrefix("api/v1/reception")]` (خط 27)
- ✅ تمام Actions دارای `[Route("...")]` attribute:
  - `[Route("health")]` → `GET /api/v1/reception/health`
  - `[Route("bootstrap")]` → `GET /api/v1/reception/bootstrap`
  - `[Route("draft/create")]` → `POST /api/v1/reception/draft/create`
  - `[Route("patient/lookup-or-create")]` → `POST /api/v1/reception/patient/lookup-or-create`
  - `[Route("item/add")]` → `POST /api/v1/reception/item/add`
  - `[Route("insurances/set")]` → `POST /api/v1/reception/insurances/set`
  - `[Route("doctors/by-department")]` → `GET /api/v1/reception/doctors/by-department`
  - `[Route("doctors/by-service")]` → `GET /api/v1/reception/doctors/by-service`
  - `[Route("finalize/pos")]` → `POST /api/v1/reception/finalize/pos`
  - `[Route("finalize/cash")]` → `POST /api/v1/reception/finalize/cash`
  - و سایر endpoints...

**نتیجه:** ✅ Attribute routing برای `api/v1/reception` فعال است.

---

### **1.2 Legacy Fallback**

#### ✅ **وضعیت: OK**

**بررسی:**
- ✅ `App_Start/RouteConfig.cs` دارای Legacy Route (خطوط 35-40):
  ```csharp
  routes.MapRoute(
      name: "ReceptionApiLegacy",
      url: "Api/ReceptionApi/{action}",
      defaults: new { controller = "ReceptionApi", action = "Index", area = "" },
      namespaces: new[] { "ClinicApp.Controllers.Api" }
  );
  ```
- ✅ `Scripts/reception.v2/reception-api.js` دارای fallback logic:
  - تابع `shouldFallback(jqXHR)` برای تشخیص 404/500
  - تابع `toLegacyPath(path)` برای تبدیل مسیر v1 به Legacy
  - تابع `ajaxWithFallback()` برای تلاش v1 و سپس Legacy

**نتیجه:** ✅ الگوی Legacy fallback برای `/Api/ReceptionApi/*` موجود است.

---

## 2️⃣ **Dependency Injection**

### **2.1 Unity Container Registration**

#### ✅ **وضعیت: OK**

**بررسی ثبت‌های Unity:**

| Interface | Implementation | Lifetime | وضعیت |
|-----------|---------------|----------|-------|
| `IReceptionFacade` | `ReceptionFacade` | `PerRequestLifetimeManager` | ✅ |
| `IReceptionPricingService` | `ReceptionPricingService` | `PerRequestLifetimeManager` | ✅ |
| `ILogger` (Serilog) | `Log.Logger` | `RegisterInstance` | ✅ |
| `IPosTerminalRepository` | `PosTerminalRepository` | `PerRequestLifetimeManager` | ✅ |
| `IPaymentTransactionRepository` | `PaymentTransactionRepository` | `PerRequestLifetimeManager` | ✅ |
| `IPosManagementService` | `PosManagementService` | `PerRequestLifetimeManager` | ✅ |

**جزئیات ثبت:**

1. **IReceptionFacade:**
   ```csharp
   // خط 526 در UnityConfig.cs
   container.RegisterType<IReceptionFacade, ReceptionFacade>(new PerRequestLifetimeManager());
   ```

2. **IReceptionPricingService:**
   ```csharp
   // خط 498 در UnityConfig.cs
   container.RegisterType<ClinicApp.Interfaces.Reception.IReceptionPricingService, 
       ClinicApp.Services.Reception.ReceptionPricingService>(new PerRequestLifetimeManager());
   ```

3. **ILogger:**
   ```csharp
   // خط 291 در UnityConfig.cs (RegisterLogger)
   container.RegisterInstance<Serilog.ILogger>(Log.Logger);
   ```

4. **IPosTerminalRepository:**
   ```csharp
   // خط 544 در UnityConfig.cs
   container.RegisterType<IPosTerminalRepository, PosTerminalRepository>(new PerRequestLifetimeManager());
   ```

5. **IPaymentTransactionRepository:**
   ```csharp
   // خط 542 در UnityConfig.cs
   container.RegisterType<IPaymentTransactionRepository, PaymentTransactionRepository>(new PerRequestLifetimeManager());
   ```

6. **IPosManagementService:**
   ```csharp
   // خط 547 در UnityConfig.cs
   container.RegisterType<IPosManagementService, PosManagementService>(new PerRequestLifetimeManager());
   ```

**نتیجه:** ✅ تمام وابستگی‌های لازم در Unity ثبت شده‌اند.

---

### **2.2 Controller Dependency Injection**

#### ✅ **وضعیت: OK**

**بررسی `ReceptionApiV1Controller`:**
- ✅ Constructor دارای تمام وابستگی‌های لازم:
  ```csharp
  public ReceptionApiV1Controller(
      IFinancialYearService fy,
      IReceptionFacade facade,
      IReceptionPricingService pricing,
      ILogger logger,
      ApplicationDbContext context)
  ```
- ✅ Fallback Constructor برای سازگاری با Legacy:
  ```csharp
  public ReceptionApiV1Controller()
      : this(
          DependencyResolver.Current.GetService<IFinancialYearService>(),
          DependencyResolver.Current.GetService<IReceptionFacade>(),
          DependencyResolver.Current.GetService<IReceptionPricingService>(),
          DependencyResolver.Current.GetService<ILogger>(),
          DependencyResolver.Current.GetService<ApplicationDbContext>())
  ```

**نتیجه:** ✅ Controller از DI استفاده می‌کند و Fallback Constructor موجود است.

---

## 3️⃣ **CSRF Protection**

### **3.1 Anti-Forgery Token در View**

#### ✅ **وضعیت: OK**

**بررسی `Views/ReceptionV2/Index.cshtml`:**
- ✅ دارای `@Html.AntiForgeryToken()` در فرم مخفی (خطوط 8-11):
  ```cshtml
  @using (Html.BeginForm("Index", "ReceptionV2", FormMethod.Post, new { id = "v2_af_form", style = "display:none" }))
  {
      @Html.AntiForgeryToken()
  }
  ```
- ✅ فرم مخفی قبل از Scripts قرار دارد (برای اطمینان از وجود token در DOM)

**نتیجه:** ✅ `@Html.AntiForgeryToken()` در View موجود است.

---

### **3.2 Anti-Forgery Token در JavaScript**

#### ✅ **وضعیت: OK**

**بررسی `Scripts/reception.v2/reception-api.js`:**
- ✅ تابع `token()` برای خواندن token از DOM (خطوط 5-7):
  ```javascript
  function token() {
    return $('input[name="__RequestVerificationToken"]').val() || '';
  }
  ```
- ✅ تابع `headers(method)` برای تزریق token در header (خطوط 9-22):
  ```javascript
  function headers(method) {
    const h = {};
    if (method.toUpperCase() !== 'GET') {
      const t = token();
      if (t) {
        // MVC 5 accepts token in header as RequestVerificationToken
        h['RequestVerificationToken'] = t;
        // Also add X-RequestVerificationToken as fallback
        h['X-RequestVerificationToken'] = t;
      }
    }
    h['X-Requested-With'] = 'XMLHttpRequest';
    return h;
  }
  ```
- ✅ استفاده از `headers(method)` در تمام AJAX calls (خطوط 123, 155, 188)

**نتیجه:** ✅ هدر `__RequestVerificationToken` در همه POSTها تزریق می‌شود.

---

### **3.3 Anti-Forgery Filter**

#### ✅ **وضعیت: OK**

**بررسی `Filters/ValidateAntiForgeryTokenOnPostsAttribute.cs`:**

1. **پشتیبانی از Header Token:**
   - ✅ خواندن token از `RequestVerificationToken` یا `X-RequestVerificationToken` (خط 40)
   - ✅ خواندن cookie token از `__RequestVerificationToken` (خطوط 46-76)
   - ✅ پشتیبانی از الگوی "cookie:form" در یک هدر (خطوط 78-88)

2. **Validation:**
   - ✅ `AntiForgery.Validate(cookieToken, formToken)` برای Ajax (خط 103)
   - ✅ `AntiForgery.Validate()` برای فرم معمولی (خط 108)

3. **Error Handling:**
   - ✅ پاسخ JSON 400 با کد `ANTIFORGERY_MISSING` برای Ajax (خطوط 118-133):
     ```csharp
     filterContext.Result = new JsonResult
     {
         Data = ServiceResult.Failed(
             "توکن امنیتی منقضی یا نامعتبر است. صفحه را نوسازی کنید.",
             code: "ANTIFORGERY_MISSING",
             category: ErrorCategory.Security,
             securityLevel: SecurityLevel.High
         ).WithExceptionDev(ex),
         JsonRequestBehavior = JsonRequestBehavior.AllowGet
     };
     filterContext.HttpContext.Response.StatusCode = 400;
     ```
   - ✅ Redirect با TempData برای فرم معمولی (خطوط 135-141)

4. **Logging:**
   - ✅ استفاده از Serilog برای لاگ خطا (خط 114)

**نتیجه:** ✅ Filter با پاسخ JSON 400 و کد `ANTIFORGERY_MISSING` در Dev کامل است.

---

### **3.4 استفاده از Filter در Controller**

#### ✅ **وضعیت: OK**

**بررسی `ReceptionApiV1Controller`:**
- ✅ تمام POST actions دارای `[ValidateAntiForgeryTokenOnPosts]`:
  - `CreateDraft` (خط 174)
  - `DeleteIncompleteDraft` (خط 216)
  - `PatientLookup` (خط 281)
  - `UpdatePatientBasic` (خط 728)
  - `SetInsurances` (خط 838)
  - `AddItem` (خط 996)
  - `RemoveItem` (خط 1123)
  - `UpdateItemService` (خط 1208)
  - `UpdateDraft` (خط 1319)
  - `FinalizePos` (خط 1425)
  - `FinalizeCash` (خط 1489)
  - `UpdateReception` (خط 1578)
  - `CancelReception` (خط 1613)

**نتیجه:** ✅ تمام POST actions دارای `[ValidateAntiForgeryTokenOnPosts]` هستند.

---

### **3.5 Frontend Error Handling**

#### ✅ **وضعیت: OK**

**بررسی `Scripts/reception.v2/reception-api.js`:**
- ✅ تابع `handleErrorJson(res)` برای مدیریت خطاهای خاص (خطوط 69-105):
  ```javascript
  function handleErrorJson(res) {
    if (!res) return false;

    // بررسی ANTIFORGERY_MISSING
    if (res.Code === 'ANTIFORGERY_MISSING' || res.code === 'ANTIFORGERY_MISSING') {
      console.warn('🏥 V2: CSRF token missing/expired', res);
      toastr.error('توکن امنیتی منقضی شده است. لطفاً صفحه را نوسازی کنید.', 'خطای امنیتی', {
        timeOut: 5000,
        extendedTimeOut: 3000
      });
      
      // پیشنهاد Refresh (اختیاری)
      if (confirm('آیا می‌خواهید صفحه را نوسازی کنید؟')) {
        window.location.reload();
      }
      
      return true; // خطا مصرف شد
    }
    // ...
  }
  ```
- ✅ استفاده از `handleErrorJson` در `.done()` و `.fail()` (خطوط 126, 135, 159, 170, 192, 201)

**نتیجه:** ✅ Frontend دارای مدیریت خطای `ANTIFORGERY_MISSING` است.

---

## 4️⃣ **Zero-Cache Policy**

### **4.1 Controller Cache Control**

#### ✅ **وضعیت: OK**

**بررسی `ReceptionApiV1Controller`:**
- ✅ دارای `[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]` (خط 28)
- ✅ دارای `[ReceptionV2Controller.NoCache]` (خط 29)

**نتیجه:** ✅ Zero-Cache روی Controller اعمال شده است.

---

## 📊 **خلاصه وضعیت**

| مورد | وضعیت | جزئیات |
|-----|-------|---------|
| **Attribute Routing** | ✅ OK | `api/v1/reception` فعال است |
| **Legacy Fallback** | ✅ OK | `/Api/ReceptionApi/*` موجود است |
| **IReceptionFacade** | ✅ OK | در Unity ثبت شده است |
| **IReceptionPricingService** | ✅ OK | در Unity ثبت شده است |
| **ILogger** | ✅ OK | در Unity ثبت شده است |
| **IPosTerminalRepository** | ✅ OK | در Unity ثبت شده است |
| **IPaymentTransactionRepository** | ✅ OK | در Unity ثبت شده است |
| **IPosManagementService** | ✅ OK | در Unity ثبت شده است |
| **Anti-Forgery Token (View)** | ✅ OK | `@Html.AntiForgeryToken()` موجود است |
| **Anti-Forgery Token (JS)** | ✅ OK | Header تزریق می‌شود |
| **ValidateAntiForgeryTokenOnPosts** | ✅ OK | Filter کامل است |
| **Error Handling (Filter)** | ✅ OK | JSON 400 با کد `ANTIFORGERY_MISSING` |
| **Error Handling (Frontend)** | ✅ OK | مدیریت خطا در JS موجود است |
| **Zero-Cache** | ✅ OK | Controller دارای NoCache است |

---

## 🎯 **اقدامات لازم**

### **✅ تکمیل شده:**
1. ✅ Route Configuration
2. ✅ Dependency Injection
3. ✅ CSRF Protection
4. ✅ Error Handling

### **⚠️ نیازمند بررسی (فاز‌های بعدی):**
1. ⚠️ Bootstrap Endpoint (فاز C): بررسی وجود `PosTerminals` و `DefaultPosTerminalId`
2. ⚠️ Pricing Endpoints (فاز D): بررسی Reprice-on-change
3. ⚠️ Doctor/Department Filters (فاز E): بررسی Validation
4. ⚠️ POS Payment (فاز F): بررسی وجود `IPosProviderResolver` و `IPosProviderClient`

---

## 🔍 **نقاط قوت**

1. ✅ **Route Configuration کامل:** Attribute routing + Legacy fallback
2. ✅ **DI کامل:** تمام وابستگی‌های لازم ثبت شده‌اند
3. ✅ **CSRF Protection کامل:** View + JS + Filter + Error Handling
4. ✅ **Error Handling حرفه‌ای:** JSON 400 با کد یکتا + Frontend handling

---

## 📝 **نکات مهم**

1. **ILogger Registration:**
   - `ILogger` به صورت `RegisterInstance` ثبت شده است (Singleton)
   - استفاده از `Log.Logger` از Serilog
   - برای Context-specific logging از `.ForContext<T>()` استفاده می‌شود

2. **Anti-Forgery Token Flow:**
   - View: `@Html.AntiForgeryToken()` → Hidden Input
   - JS: `token()` → خواندن از DOM → `headers()` → تزریق در Header
   - Filter: خواندن از Header + Cookie → Validation → JSON 400 در صورت خطا

3. **Legacy Fallback:**
   - Fallback فقط برای 404/500/0 یا خطاهای "not found" فعال می‌شود
   - برای endpoint های `/patient/*` fallback غیرفعال است (فقط v1)

---

**تاریخ به‌روزرسانی:** 2025-01-27  
**نسخه:** 1.0.0  
**وضعیت:** ✅ فاز B تکمیل شد

