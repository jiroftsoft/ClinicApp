# 🔍 تحلیل عمیق معماری سیستم پرداخت آنلاین

**تاریخ:** 2026-01-06  
**هدف:** بررسی کامل معماری، شناسایی مشکلات و ارائه راه‌حل‌های Best Practice

---

## 📋 خلاصه اجرایی

### ✅ نقاط قوت:
1. Gateway Pattern پیاده‌سازی شده (IGatewayDriver)
2. Separation of Concerns رعایت شده (Service, Repository, Driver)
3. Dependency Injection استفاده شده
4. Logging کامل

### ❌ مشکلات شناسایی شده:
1. **Gateway Driver Selection**: فقط یک Driver در DI ثبت شده (ZarinPal)
2. **Gateway Selection Logic**: در `CreatePaymentRequestAsync` فقط `FirstOrDefault` استفاده می‌شود
3. **GetDefaultPaymentGatewayAsync**: در `CreatePaymentRequestAsync` استفاده نمی‌شود
4. **Gateway Factory Missing**: هیچ Factory Pattern برای انتخاب Driver بر اساس GatewayType وجود ندارد
5. **Hard Dependency**: `WebPaymentService` به یک `IGatewayDriver` وابسته است (نه Factory)

---

## 🏗️ معماری فعلی

### 1. Dependency Injection (UnityConfig.cs)

```csharp
// ❌ مشکل: فقط یک Driver ثبت شده
container.RegisterType<IGatewayDriver, ZarinPalDriver>(
    new PerRequestLifetimeManager()
);

// ❌ مشکل: WebPaymentService به یک IGatewayDriver وابسته است
container.RegisterType<IWebPaymentService, WebPaymentService>(
    new InjectionConstructor(
        ...,
        new ResolvedParameter<IGatewayDriver>(), // همیشه ZarinPalDriver
        ...
    )
);
```

**مشکل:**
- نمی‌توان چند Gateway مختلف داشت
- نمی‌توان Gateway را بر اساس `GatewayType` انتخاب کرد
- اگر Gateway دیگری اضافه شود، باید کد تغییر کند

---

### 2. WebPaymentService.CreatePaymentRequestAsync

```csharp
// ❌ مشکل: فقط اولین Gateway را انتخاب می‌کند
var gateways = await _paymentGatewayRepository.GetByTypeAsync(request.GatewayType);
var gateway = gateways.FirstOrDefault(); // ❌ باید GetDefaultPaymentGatewayAsync استفاده شود

// ❌ مشکل: از یک IGatewayDriver استفاده می‌کند (همیشه ZarinPal)
var gatewayResponse = await CreateGatewayPaymentRequestAsync(gateway, request);
```

**مشکل:**
- `GetDefaultPaymentGatewayAsync` وجود دارد اما استفاده نمی‌شود
- Gateway Selection Logic ناقص است
- Driver Selection بر اساس GatewayType انجام نمی‌شود

---

### 3. CreateGatewayPaymentRequestAsync

```csharp
// ❌ مشکل: از یک IGatewayDriver استفاده می‌کند (همیشه ZarinPal)
var driverResult = await _gatewayDriver.RequestPaymentAsync(driverRequest);
```

**مشکل:**
- `_gatewayDriver` همیشه `ZarinPalDriver` است
- نمی‌توان Gateway دیگری استفاده کرد
- اگر GatewayType = PayPing باشد، باز هم ZarinPalDriver استفاده می‌شود!

---

## 🎯 Best Practice: Gateway Factory Pattern

### معماری پیشنهادی:

```
┌─────────────────────────────────────────────────────────┐
│              AppointmentBookingController                │
│  ProcessPayment → GetDefaultPaymentGatewayAsync          │
└─────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────┐
│                  WebPaymentService                       │
│  CreatePaymentRequestAsync                               │
│  → GetDefaultPaymentGatewayAsync (Gateway Selection)    │
│  → IGatewayDriverFactory.GetDriver(gatewayType)         │
└─────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────┐
│              IGatewayDriverFactory                       │
│  GetDriver(GatewayType) → IGatewayDriver                 │
│  - ZarinPalDriver (برای GatewayType.ZarinPal)            │
│  - PayPingDriver (برای GatewayType.PayPing)              │
│  - ...                                                    │
└─────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────┐
│                  IGatewayDriver                          │
│  RequestPaymentAsync, VerifyPaymentAsync, ...             │
└─────────────────────────────────────────────────────────┘
```

---

## 🔧 راه‌حل‌های پیشنهادی

### راه‌حل 1: Gateway Driver Factory (توصیه می‌شود)

#### 1.1. ایجاد IGatewayDriverFactory

```csharp
public interface IGatewayDriverFactory
{
    IGatewayDriver GetDriver(PaymentGatewayType gatewayType);
    bool IsSupported(PaymentGatewayType gatewayType);
}
```

#### 1.2. پیاده‌سازی Factory

```csharp
public class GatewayDriverFactory : IGatewayDriverFactory
{
    private readonly Dictionary<PaymentGatewayType, Func<IGatewayDriver>> _drivers;
    private readonly ILogger _logger;

    public GatewayDriverFactory(
        IZarinPalDriver zarinPalDriver,
        IPayPingDriver payPingDriver, // در آینده
        ILogger logger)
    {
        _logger = logger;
        _drivers = new Dictionary<PaymentGatewayType, Func<IGatewayDriver>>
        {
            { PaymentGatewayType.ZarinPal, () => zarinPalDriver },
            // { PaymentGatewayType.PayPing, () => payPingDriver },
        };
    }

    public IGatewayDriver GetDriver(PaymentGatewayType gatewayType)
    {
        if (!_drivers.ContainsKey(gatewayType))
        {
            throw new NotSupportedException($"Gateway type {gatewayType} is not supported");
        }

        return _drivers[gatewayType]();
    }

    public bool IsSupported(PaymentGatewayType gatewayType)
    {
        return _drivers.ContainsKey(gatewayType);
    }
}
```

#### 1.3. تغییر WebPaymentService

```csharp
public class WebPaymentService : IWebPaymentService
{
    private readonly IGatewayDriverFactory _driverFactory; // ✅ به جای IGatewayDriver

    public WebPaymentService(
        ...,
        IGatewayDriverFactory driverFactory, // ✅ Factory
        ...)
    {
        _driverFactory = driverFactory;
    }

    private async Task<ServiceResult<PaymentGatewayResponse>> CreateGatewayPaymentRequestAsync(
        PaymentGateway gateway, 
        CreatePaymentRequest request)
    {
        // ✅ انتخاب Driver بر اساس GatewayType
        var driver = _driverFactory.GetDriver(gateway.GatewayType);
        
        var driverResult = await driver.RequestPaymentAsync(driverRequest);
        // ...
    }
}
```

#### 1.4. ثبت در UnityConfig

```csharp
// ✅ ثبت Drivers
container.RegisterType<IZarinPalDriver, ZarinPalDriver>(new PerRequestLifetimeManager());
// container.RegisterType<IPayPingDriver, PayPingDriver>(new PerRequestLifetimeManager());

// ✅ ثبت Factory
container.RegisterType<IGatewayDriverFactory, GatewayDriverFactory>(
    new PerRequestLifetimeManager(),
    new InjectionConstructor(
        new ResolvedParameter<IZarinPalDriver>(),
        // new ResolvedParameter<IPayPingDriver>(),
        new ResolvedParameter<Serilog.ILogger>()
    )
);

// ✅ ثبت WebPaymentService با Factory
container.RegisterType<IWebPaymentService, WebPaymentService>(
    new InjectionConstructor(
        ...,
        new ResolvedParameter<IGatewayDriverFactory>(), // ✅ Factory
        ...
    )
);
```

---

### راه‌حل 2: استفاده از GetDefaultPaymentGatewayAsync (سریع‌تر)

#### 2.1. تغییر CreatePaymentRequestAsync

```csharp
public async Task<ServiceResult<PaymentGatewayResponse>> CreatePaymentRequestAsync(CreatePaymentRequest request)
{
    // ✅ استفاده از GetDefaultPaymentGatewayAsync
    var gatewayResult = await GetDefaultPaymentGatewayAsync();
    if (!gatewayResult.Success || gatewayResult.Data == null)
    {
        return ServiceResult<PaymentGatewayResponse>.Failed(gatewayResult.Message);
    }

    var gateway = gatewayResult.Data;

    // ✅ بررسی GatewayType
    if (gateway.GatewayType != request.GatewayType)
    {
        _logger.Warning("⚠️ GatewayType mismatch - Request: {RequestType}, Gateway: {GatewayType}",
            request.GatewayType, gateway.GatewayType);
        // یا خطا برگردان یا Gateway را تغییر بده
    }

    // ✅ استفاده از Driver
    var gatewayResponse = await CreateGatewayPaymentRequestAsync(gateway, request);
    // ...
}
```

**مشکل این راه‌حل:**
- هنوز مشکل Driver Selection حل نشده
- اگر GatewayType متفاوت باشد، چه کنیم؟

---

### راه‌حل 3: Gateway Selection در CreatePaymentRequestAsync

#### 3.1. تغییر CreatePaymentRequestAsync

```csharp
public async Task<ServiceResult<PaymentGatewayResponse>> CreatePaymentRequestAsync(CreatePaymentRequest request)
{
    // ✅ دریافت Gateway مناسب
    PaymentGateway gateway;
    
    if (request.GatewayType == PaymentGatewayType.ZarinPal)
    {
        var gatewayResult = await GetDefaultPaymentGatewayAsync();
        if (!gatewayResult.Success || gatewayResult.Data == null)
        {
            return ServiceResult<PaymentGatewayResponse>.Failed(gatewayResult.Message);
        }
        gateway = gatewayResult.Data;
    }
    else
    {
        // ✅ برای Gateway های دیگر
        var gateways = await _paymentGatewayRepository.GetByTypeAsync(request.GatewayType);
        gateway = gateways?.FirstOrDefault(g => g.IsActive && !g.IsDeleted);
        
        if (gateway == null)
        {
            return ServiceResult<PaymentGatewayResponse>.Failed($"درگاه پرداخت {request.GatewayType} یافت نشد");
        }
    }

    // ✅ بررسی Driver Support
    if (!_driverFactory.IsSupported(gateway.GatewayType))
    {
        return ServiceResult<PaymentGatewayResponse>.Failed($"درگاه پرداخت {gateway.GatewayType} پشتیبانی نمی‌شود");
    }

    // ✅ انتخاب Driver
    var driver = _driverFactory.GetDriver(gateway.GatewayType);
    
    // ✅ ایجاد درخواست
    var gatewayResponse = await CreateGatewayPaymentRequestAsync(gateway, request, driver);
    // ...
}
```

---

## 🔍 مشکلات شناسایی شده در کد فعلی

### مشکل 1: Gateway Selection Logic ناقص

**فایل:** `Services/Payment/Web/WebPaymentService.cs`  
**خط:** 109-115

```csharp
// ❌ مشکل: فقط اولین Gateway را انتخاب می‌کند
var gateways = await _paymentGatewayRepository.GetByTypeAsync(request.GatewayType);
if (gateways == null || !gateways.Any())
{
    return ServiceResult<PaymentGatewayResponse>.Failed("درگاه پرداخت یافت نشد");
}

var gateway = gateways.FirstOrDefault(); // ❌ باید GetDefaultPaymentGatewayAsync استفاده شود
```

**راه‌حل:**
```csharp
// ✅ استفاده از GetDefaultPaymentGatewayAsync
var gatewayResult = await GetDefaultPaymentGatewayAsync();
if (!gatewayResult.Success || gatewayResult.Data == null)
{
    return ServiceResult<PaymentGatewayResponse>.Failed(gatewayResult.Message);
}
var gateway = gatewayResult.Data;
```

---

### مشکل 2: Driver Selection بر اساس GatewayType انجام نمی‌شود

**فایل:** `Services/Payment/Web/WebPaymentService.cs`  
**خط:** 346-440

```csharp
// ❌ مشکل: از یک IGatewayDriver استفاده می‌کند (همیشه ZarinPal)
var driverResult = await _gatewayDriver.RequestPaymentAsync(driverRequest);
```

**راه‌حل:**
```csharp
// ✅ انتخاب Driver بر اساس GatewayType
var driver = _driverFactory.GetDriver(gateway.GatewayType);
var driverResult = await driver.RequestPaymentAsync(driverRequest);
```

---

### مشکل 3: GetDefaultPaymentGatewayAsync استفاده نمی‌شود

**فایل:** `Services/Payment/Web/WebPaymentService.cs`  
**خط:** 73-141

**مشکل:**
- `GetDefaultPaymentGatewayAsync` وجود دارد (خط 200+)
- اما در `CreatePaymentRequestAsync` استفاده نمی‌شود
- در `ProcessPayment` استفاده می‌شود اما در `CreatePaymentRequestAsync` نه!

---

## 📊 مقایسه معماری فعلی با Best Practice

| جنبه | معماری فعلی | Best Practice | وضعیت |
|------|------------|--------------|-------|
| **Gateway Pattern** | ✅ پیاده‌سازی شده | ✅ | ✅ |
| **Driver Selection** | ❌ Hard-coded | ✅ Factory Pattern | ❌ |
| **Gateway Selection** | ❌ FirstOrDefault | ✅ GetDefaultPaymentGatewayAsync | ❌ |
| **Multiple Gateways** | ❌ پشتیبانی نمی‌شود | ✅ پشتیبانی می‌شود | ❌ |
| **Dependency Injection** | ✅ استفاده شده | ✅ | ✅ |
| **Separation of Concerns** | ✅ رعایت شده | ✅ | ✅ |
| **Error Handling** | ✅ کامل | ✅ | ✅ |
| **Logging** | ✅ کامل | ✅ | ✅ |

---

## 🎯 توصیه‌های نهایی

### اولویت 1: Gateway Driver Factory (CRITICAL)

**چرا:**
- بدون Factory، نمی‌توان Gateway های مختلف داشت
- کد Hard-coded است و قابل توسعه نیست
- اگر Gateway دیگری اضافه شود، باید کد تغییر کند

**اقدام:**
1. ایجاد `IGatewayDriverFactory`
2. پیاده‌سازی `GatewayDriverFactory`
3. تغییر `WebPaymentService` برای استفاده از Factory
4. ثبت Factory در UnityConfig

---

### اولویت 2: استفاده از GetDefaultPaymentGatewayAsync (HIGH)

**چرا:**
- منطق انتخاب Gateway تکراری است
- `GetDefaultPaymentGatewayAsync` قبلاً پیاده‌سازی شده
- Consistency در کد

**اقدام:**
1. تغییر `CreatePaymentRequestAsync` برای استفاده از `GetDefaultPaymentGatewayAsync`
2. حذف کد تکراری

---

### اولویت 3: Gateway Selection Logic (MEDIUM)

**چرا:**
- باید Gateway مناسب را انتخاب کند (Default, Active, Not Deleted)
- باید Priority را در نظر بگیرد

**اقدام:**
1. بهبود `GetDefaultPaymentGatewayAsync`
2. اضافه کردن Priority Logic

---

## ✅ نتیجه‌گیری

**وضعیت فعلی:**
- ✅ معماری کلی درست است
- ✅ Gateway Pattern پیاده‌سازی شده
- ❌ Driver Selection Hard-coded است
- ❌ Gateway Selection Logic ناقص است

**اقدامات لازم:**
1. ✅ ایجاد Gateway Driver Factory
2. ✅ استفاده از GetDefaultPaymentGatewayAsync
3. ✅ بهبود Gateway Selection Logic

**زمان تخمینی:**
- Factory Pattern: 2-3 ساعت
- Refactoring: 1-2 ساعت
- Testing: 1-2 ساعت
- **جمع: 4-7 ساعت**

---

**📌 مرجع:** 
- Gateway Pattern: Design Patterns (Gang of Four)
- Factory Pattern: Design Patterns (Gang of Four)
- Best Practices: Microsoft Payment Gateway Integration Guide

