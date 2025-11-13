# 🏥 نقشه راه بهینه‌سازی ماژول پذیرش - محاسبه Real-Time

## 📋 خلاصه اجرایی

این سند نقشه راه کامل برای بهینه‌سازی ماژول پذیرش با تمرکز بر محاسبه real-time بیمه است. هدف: ایجاد یک سیستم مقاوم، سریع و قابل اعتماد برای محیط درمانی با ترافیک بالا.

---

## 🎯 اهداف

1. **محاسبه Real-Time**: محاسبه بیمه بلافاصله پس از افزودن خدمت
2. **بدون Cache**: همه محاسبات real-time و از دیتابیس
3. **مقاوم و پایدار**: مناسب محیط درمانی با ترافیک بالا
4. **عملکرد بالا**: پاسخ سریع برای تعداد زیادی بیمار

---

## 🔍 تحلیل وضعیت فعلی

### مشکل اصلی
- **در بار اول**: خدمت اضافه می‌شود اما "بدون پوشش" نمایش داده می‌شود
- **در بار دوم**: محاسبات درست انجام می‌شود
- **علت**: محاسبه بیمه در `AddItemAsync` انجام نمی‌شود، فقط در `RecalculateTotalsAsync`

### فرآیند فعلی

```
1. کاربر خدمت را انتخاب می‌کند
2. AddItemAsync فراخوانی می‌شود
   ├─ محاسبه قیمت پایه (K & FactorSetting)
   ├─ افزودن به ReceptionItems
   └─ RecalculateTotalsAsync
      └─ محاسبه بیمه (مشکل: ممکن است در بار اول کار نکند)
3. نمایش نتایج در UI
```

### مشکلات شناسایی شده

1. **عدم محاسبه بیمه در AddItemAsync**: 
   - `AddItemAsync` فقط قیمت پایه را محاسبه می‌کند
   - محاسبه بیمه در `RecalculateTotalsAsync` انجام می‌شود
   - ممکن است در بار اول `RecalculateTotalsAsync` به درستی کار نکند

2. **وابستگی به ReceptionId**:
   - محاسبه بیمه نیاز به `ReceptionId` دارد
   - اگر Reception هنوز ایجاد نشده باشد، محاسبه نمی‌شود

3. **عدم استفاده از تعرفه‌های فعال**:
   - قبلاً اصلاح شد، اما باید بررسی شود

---

## 🗺️ نقشه راه (Roadmap)

### فاز 1: تحلیل و طراحی (✅ در حال انجام)

#### 1.1 تحلیل فرآیند فعلی
- [x] بررسی `AddItemAsync`
- [x] بررسی `RecalculateTotalsAsync`
- [x] بررسی `CalculateCombinedInsuranceAsync`
- [ ] بررسی UI و JavaScript

#### 1.2 شناسایی نقاط مشکل
- [x] مشکل در بار اول
- [x] مشکل در محاسبه بیمه
- [ ] مشکل در UI update

#### 1.3 طراحی راه‌حل
- [ ] طراحی معماری جدید
- [ ] طراحی API endpoints
- [ ] طراحی UI flow

### فاز 2: پیاده‌سازی Core Logic (🔄 در حال انجام)

#### 2.1 اصلاح `AddItemAsync`
- [x] افزودن محاسبه بیمه به `AddItemAsync`
- [ ] تست و اعتبارسنجی

#### 2.2 بهینه‌سازی `RecalculateTotalsAsync`
- [ ] بررسی و بهینه‌سازی
- [ ] افزودن retry logic
- [ ] افزودن error handling

#### 2.3 اصلاح `CalculateCombinedInsuranceAsync`
- [x] استفاده از تعرفه‌های فعال
- [ ] بهینه‌سازی performance
- [ ] افزودن logging

### فاز 3: API و Endpoints (⏳ در انتظار)

#### 3.1 ایجاد Endpoint جدید
- [ ] `POST /api/reception/item/add-with-calculation`
- [ ] `POST /api/reception/item/recalculate`
- [ ] `GET /api/reception/item/{itemId}/insurance`

#### 3.2 بهینه‌سازی Endpoints موجود
- [ ] `POST /api/reception/item/add`
- [ ] `POST /api/reception/insurances/set`

### فاز 4: UI و Frontend (⏳ در انتظار)

#### 4.1 بهینه‌سازی JavaScript
- [ ] اصلاح `service-lookup.js`
- [ ] افزودن real-time update
- [ ] افزودن error handling

#### 4.2 بهبود UX
- [ ] Loading states
- [ ] Error messages
- [ ] Success feedback

### فاز 5: تست و بهینه‌سازی (⏳ در انتظار)

#### 5.1 تست عملکرد
- [ ] تست با ترافیک بالا
- [ ] تست با داده‌های مختلف
- [ ] تست edge cases

#### 5.2 بهینه‌سازی
- [ ] بهینه‌سازی queries
- [ ] بهینه‌سازی caching (اگر نیاز باشد)
- [ ] بهینه‌سازی logging

---

## 🏗️ معماری پیشنهادی

### سناریو 1: افزودن خدمت با محاسبه Real-Time

```
┌─────────────┐
│   UI/User   │
└──────┬──────┘
       │ 1. انتخاب خدمت
       ▼
┌─────────────────────┐
│  JavaScript Client  │
│  - service-lookup.js│
└──────┬──────────────┘
       │ 2. POST /api/reception/item/add
       ▼
┌─────────────────────┐
│ ReceptionApiController│
└──────┬──────────────┘
       │ 3. AddItemAsync
       ▼
┌─────────────────────┐
│  ReceptionFacade    │
│  - AddItemAsync     │
│  - CalculatePrice   │
│  - AddToReception   │
│  - CalculateInsurance│ ← 🚨 NEW: محاسبه بیمه در همین مرحله
└──────┬──────────────┘
       │ 4. CalculateCombinedInsuranceAsync
       ▼
┌─────────────────────┐
│CombinedInsurance    │
│CalculationService   │
│  - GetTariffByType  │
│  - CalculatePrimary │
│  - CalculateSupp    │
└──────┬──────────────┘
       │ 5. Return Results
       ▼
┌─────────────────────┐
│  Response to UI     │
│  - Item Data        │
│  - Insurance Data   │
│  - Totals           │
└─────────────────────┘
```

### سناریو 2: محاسبه مجدد (Recalculate)

```
┌─────────────┐
│   UI/User   │
└──────┬──────┘
       │ 1. تغییر بیمه یا خدمت
       ▼
┌─────────────────────┐
│  JavaScript Client  │
└──────┬──────────────┘
       │ 2. POST /api/reception/recalculate
       ▼
┌─────────────────────┐
│ ReceptionApiController│
└──────┬──────────────┘
       │ 3. RecalculateTotalsAsync
       ▼
┌─────────────────────┐
│  ReceptionRepository│
│  - GetItems          │
│  - ForEach Item:     │
│    CalculateInsurance│
└─────────────────────┘
```

---

## 💡 راه‌حل پیشنهادی

### راه‌حل 1: محاسبه بیمه در `AddItemAsync` (توصیه می‌شود)

**مزایا:**
- ✅ محاسبه بلافاصله پس از افزودن
- ✅ یک درخواست API
- ✅ Real-time
- ✅ ساده و قابل اعتماد

**معایب:**
- ⚠️ ممکن است زمان پاسخ کمی بیشتر شود (اما قابل قبول است)

**پیاده‌سازی:**
```csharp
public async Task<ServiceResult<AddItemResultDto>> AddItemAsync(...)
{
    // 1. محاسبه قیمت پایه
    var unitPrice = await _serviceCalculationEngine.CalculateUnitPriceIRRAsync(serviceId, year);
    
    // 2. افزودن به پذیرش
    var addResult = await _receptionWorkflowService.AddItemAsync(receptionId, serviceId, quantity, unitPrice);
    
    // 3. 🚨 NEW: محاسبه بیمه برای این آیتم
    var insuranceResult = await CalculateItemInsuranceAsync(receptionId, serviceId, unitPrice * quantity);
    
    // 4. محاسبه مجدد مجموع‌ها
    var totalsResult = await _receptionRepository.RecalculateTotalsAsync(receptionId);
    
    // 5. ترکیب نتایج
    return new AddItemResultDto
    {
        Item = addResult.Data,
        Insurance = insuranceResult.Data, // 🚨 NEW
        Totals = totalsResult.Data
    };
}
```

### راه‌حل 2: Endpoint جداگانه برای محاسبه بیمه

**مزایا:**
- ✅ جداسازی concerns
- ✅ امکان محاسبه مجدد بدون افزودن آیتم

**معایب:**
- ⚠️ نیاز به دو درخواست API
- ⚠️ پیچیده‌تر

---

## 🔧 پیاده‌سازی فنی

### 1. اصلاح `ReceptionFacade.AddItemAsync`

```csharp
public async Task<ServiceResult<AddItemResultDto>> AddItemAsync(AddItemRequest request)
{
    try
    {
        // 1. محاسبه قیمت پایه
        var unitPrice = await _serviceCalculationEngine.CalculateUnitPriceIRRAsync(
            request.ServiceId, request.Year);
        var itemTotal = unitPrice * request.Quantity;

        // 2. افزودن به پذیرش
        var addResult = await _receptionWorkflowService.AddItemAsync(
            request.ReceptionId, request.ServiceId, request.Quantity, unitPrice);
        
        if (!addResult.Success)
            return ServiceResult<AddItemResultDto>.Failed(addResult.Message);

        // 3. 🚨 NEW: دریافت اطلاعات بیمار و بیمه
        var reception = await _receptionRepository.GetByIdAsync(request.ReceptionId);
        if (reception == null)
            return ServiceResult<AddItemResultDto>.Failed("پذیرش یافت نشد");

        // 4. 🚨 NEW: محاسبه بیمه برای این آیتم
        var insuranceResult = await CalculateItemInsuranceRealTimeAsync(
            reception.PatientId, 
            request.ServiceId, 
            itemTotal, 
            reception.ReceptionDate ?? DateTime.Now);

        // 5. محاسبه مجدد مجموع‌ها
        var totalsResult = await _receptionRepository.RecalculateTotalsAsync(request.ReceptionId);
        if (!totalsResult.Success)
            return ServiceResult<AddItemResultDto>.Failed(totalsResult.Message);

        var result = new AddItemResultDto
        {
            ReceptionId = request.ReceptionId,
            ServiceId = request.ServiceId,
            Quantity = request.Quantity,
            UnitPrice = unitPrice,
            ItemTotal = itemTotal,
            InsuranceCalculation = insuranceResult.Data, // 🚨 NEW
            ReceptionTotals = totalsResult.Data
        };

        return ServiceResult<AddItemResultDto>.Successful(result);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "❌ FACADE: خطا در افزودن آیتم به پذیرش");
        return ServiceResult<AddItemResultDto>.Failed("خطا در افزودن آیتم به پذیرش");
    }
}

// 🚨 NEW: متد کمکی برای محاسبه بیمه real-time
private async Task<ServiceResult<ItemInsuranceCalculationDto>> CalculateItemInsuranceRealTimeAsync(
    int patientId, int serviceId, decimal serviceAmount, DateTime calculationDate)
{
    try
    {
        // استفاده از CombinedInsuranceCalculationService
        var result = await _combinedInsuranceCalculationService.CalculateCombinedInsuranceAsync(
            patientId, serviceId, serviceAmount, calculationDate);

        if (result.Success)
        {
            return ServiceResult<ItemInsuranceCalculationDto>.Successful(new ItemInsuranceCalculationDto
            {
                PrimaryCoverage = result.Data.PrimaryCoverage,
                SupplementaryCoverage = result.Data.SupplementaryCoverage,
                TotalInsuranceCoverage = result.Data.TotalInsuranceCoverage,
                PatientShare = result.Data.FinalPatientShare,
                CoverageStatus = result.Data.TotalInsuranceCoverage > 0 ? "پوشش کامل" : "بدون پوشش"
            });
        }

        return ServiceResult<ItemInsuranceCalculationDto>.Failed(result.Message);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "❌ FACADE: خطا در محاسبه بیمه آیتم");
        return ServiceResult<ItemInsuranceCalculationDto>.Failed("خطا در محاسبه بیمه");
    }
}
```

### 2. ایجاد DTO جدید

```csharp
public class ItemInsuranceCalculationDto
{
    public decimal PrimaryCoverage { get; set; }
    public decimal SupplementaryCoverage { get; set; }
    public decimal TotalInsuranceCoverage { get; set; }
    public decimal PatientShare { get; set; }
    public string CoverageStatus { get; set; } // "پوشش کامل", "پوشش ناقص", "بدون پوشش"
}
```

### 3. اصلاح `AddItemResultDto`

```csharp
public class AddItemResultDto
{
    public int ReceptionId { get; set; }
    public int ServiceId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ItemTotal { get; set; }
    public ItemInsuranceCalculationDto InsuranceCalculation { get; set; } // 🚨 NEW
    public ReceptionTotalsDto ReceptionTotals { get; set; }
}
```

---

## 📊 معیارهای موفقیت

1. **عملکرد**: 
   - زمان پاسخ < 500ms برای محاسبه بیمه
   - پشتیبانی از 100+ درخواست همزمان

2. **قابلیت اطمینان**:
   - 99.9% موفقیت در محاسبه
   - بدون خطا در بار اول

3. **تجربه کاربری**:
   - نمایش فوری نتایج
   - پیام‌های خطای واضح

---

## 🧪 استراتژی تست

### تست واحد (Unit Tests)
- تست `CalculateItemInsuranceRealTimeAsync`
- تست `AddItemAsync` با بیمه
- تست edge cases

### تست یکپارچگی (Integration Tests)
- تست کامل flow از UI تا دیتابیس
- تست با داده‌های واقعی
- تست با ترافیک بالا

### تست عملکرد (Performance Tests)
- تست با 100+ درخواست همزمان
- تست زمان پاسخ
- تست استفاده از منابع

---

## 📝 چک‌لیست پیاده‌سازی

- [ ] فاز 1: تحلیل و طراحی
  - [x] تحلیل فرآیند فعلی
  - [x] شناسایی مشکلات
  - [ ] طراحی معماری
- [ ] فاز 2: پیاده‌سازی Core
  - [ ] اصلاح `AddItemAsync`
  - [ ] ایجاد `CalculateItemInsuranceRealTimeAsync`
  - [ ] ایجاد DTOs
- [ ] فاز 3: API
  - [ ] اصلاح endpoints
  - [ ] تست API
- [ ] فاز 4: UI
  - [ ] اصلاح JavaScript
  - [ ] بهبود UX
- [ ] فاز 5: تست و بهینه‌سازی
  - [ ] تست کامل
  - [ ] بهینه‌سازی

---

**تاریخ ایجاد**: 2025-01-27  
**وضعیت**: 🚧 در حال پیاده‌سازی  
**اولویت**: 🔴 بالا

