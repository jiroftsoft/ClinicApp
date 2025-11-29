# 🗺️ نقشه راه پیاده‌سازی Tariff Warning در Frontend

**تاریخ**: 2025-11-29  
**اولویت**: 🔴 **بالا**  
**وضعیت**: 📋 **آماده پیاده‌سازی**

---

## 📋 خلاصه اجرایی

این سند نقشه راه کامل برای پیاده‌سازی **Warning در Frontend** برای تعرفه‌های ناقص را ارائه می‌دهد. این پیاده‌سازی باید به صورت **گام‌های اتمیک** و **تست شده** انجام شود تا از شکست در ماژول جلوگیری شود.

---

## 🎯 هدف

افزودن قابلیت نمایش **Warning** در Frontend زمانی که:
- تعیین ست بیمه‌ای برای خدمت موجود است (Validation موفق)
- اما از **Fallback Logic** استفاده شده است (تعرفه دقیق در DB نیست)

---

## 📊 تحلیل وضعیت فعلی

### ✅ آنچه انجام شده:

1. **Validation قبل از افزودن خدمت**: ✅
   - `CheckInsuranceSetAsync` در `AddItemAsync` فراخوانی می‌شود
   - اگر تعیین ست ناقص باشد، خدمت افزوده نمی‌شود

2. **SnapshotJson در ReceptionItem**: ✅
   - Snapshot به صورت JSON در `ReceptionItem.SnapshotJson` ذخیره می‌شود
   - شامل: `ServiceId`, `ServiceCode`, `UnitPrice`, `PrimaryPays`, `SupplementaryPays`, etc.

### ❌ آنچه انجام نشده:

1. **TariffWarning در SnapshotJson**: ❌
   - هیچ فیلدی برای Warning در Snapshot وجود ندارد

2. **TariffWarning در ReceptionItemDto**: ❌
   - `ReceptionItemDto` فیلد `TariffWarning` ندارد

3. **نمایش Warning در Frontend**: ❌
   - هیچ کدی برای نمایش Warning در JavaScript وجود ندارد

---

## 🗺️ نقشه راه (Roadmap)

### **فاز 1: Backend - افزودن TariffWarning به SnapshotJson** ⚙️

#### گام 1.1: بررسی وجود تعرفه در AddItemAsync

**محل**: `Services/Reception/ReceptionFacade.cs` - خط ~2188-2229

**اقدام**:
- بعد از محاسبه `quoteResult` و قبل از ساخت `snapshot`
- بررسی وجود `InsuranceTariff` در دیتابیس برای:
  - `BasePlanId` + `ServiceId` (اگر `BasePlanId` موجود باشد)
  - `SupplementaryPlanId` + `ServiceId` (اگر `SupplementaryPlanId` موجود باشد)

**کد پیشنهادی**:
```csharp
// ✅ بررسی وجود تعرفه در دیتابیس
bool hasBaseTariff = true;
bool hasSuppTariff = true;
string tariffWarning = null;

if (draft.BasePlanId.HasValue)
{
    var baseTariff = await _context.InsuranceTariffs
        .FirstOrDefaultAsync(t =>
            t.InsurancePlanId == draft.BasePlanId.Value &&
            t.ServiceId == service.ServiceId &&
            t.InsuranceType == InsuranceType.Primary &&
            t.IsActive && !t.IsDeleted
        );
    hasBaseTariff = (baseTariff != null);
}

if (draft.SupplementaryPlanId.HasValue)
{
    var suppTariff = await _context.InsuranceTariffs
        .FirstOrDefaultAsync(t =>
            t.InsurancePlanId == draft.SupplementaryPlanId.Value &&
            t.ServiceId == service.ServiceId &&
            t.InsuranceType == InsuranceType.Supplementary &&
            t.IsActive && !t.IsDeleted
        );
    hasSuppTariff = (suppTariff != null);
}

// ساخت پیام Warning
if (!hasBaseTariff && !hasSuppTariff)
{
    tariffWarning = "تعرفه پایه و تکمیلی تعریف نشده";
}
else if (!hasBaseTariff)
{
    tariffWarning = "تعرفه پایه تعریف نشده";
}
else if (!hasSuppTariff)
{
    tariffWarning = "تعرفه تکمیلی تعریف نشده";
}
```

**تست**:
- ✅ اگر تعیین ست ناقص باشد → Validation خطا می‌دهد (قبلاً پیاده‌سازی شده)
- ✅ اگر تعیین ست موجود باشد اما تعرفه نباشد → `tariffWarning` پر می‌شود

---

#### گام 1.2: افزودن TariffWarning به Snapshot

**محل**: `Services/Reception/ReceptionFacade.cs` - خط ~2189-2219

**اقدام**:
- افزودن `TariffWarning` به object `snapshot` قبل از serialize

**کد پیشنهادی**:
```csharp
// ایجاد Snapshot
var snapshot = new
{
    ServiceId = service.ServiceId,
    ServiceCode = service.ServiceCode,
    ServiceName = service.Title,
    Quantity = qty,
    UnitPrice = unit,
    // ... سایر فیلدها ...
    CalculatedAt = DateTime.Now,
    GroupCode = service.GroupCode,
    IsHashtagged = service.IsHashtagged,
    // ✅ افزودن TariffWarning
    TariffWarning = tariffWarning // null اگر تعرفه موجود باشد
};
```

**تست**:
- ✅ بررسی `SnapshotJson` در دیتابیس → باید `TariffWarning` داشته باشد
- ✅ اگر تعرفه موجود باشد → `TariffWarning` باید `null` باشد

---

### **فاز 2: Backend - افزودن TariffWarning به ReceptionItemDto** ⚙️

#### گام 2.1: افزودن فیلد TariffWarning به ReceptionItemDto

**محل**: `ViewModels/Reception/ReceptionDraftDtos.cs` - خط ~44-56

**اقدام**:
- افزودن `public string TariffWarning { get; set; }` به `ReceptionItemDto`

**کد پیشنهادی**:
```csharp
public class ReceptionItemDto
{
    public int ServiceId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int Qty { get; set; }
    public decimal UnitPriceIRR { get; set; }
    public decimal TotalIRR { get; set; }
    public ItemInsuranceCalculationDto InsuranceCalculation { get; set; }
    // ✅ افزودن TariffWarning
    public string TariffWarning { get; set; }
}
```

**تست**:
- ✅ Build موفق
- ✅ هیچ خطای compile وجود ندارد

---

#### گام 2.2: استخراج TariffWarning از SnapshotJson در RecalculateDraftAsync

**محل**: `Services/Reception/ReceptionFacade.cs` - خط ~3448-3500 (جایی که `ReceptionItemDto` ساخته می‌شود)

**اقدام**:
- هنگام ساخت `ReceptionItemDto` از `ReceptionItem`
- Parse کردن `SnapshotJson` و استخراج `TariffWarning`

**کد پیشنهادی**:
```csharp
// در RecalculateDraftAsync - جایی که ReceptionItemDto ساخته می‌شود
foreach (var item in draft.ReceptionItems.Where(i => !i.IsDeleted))
{
    var service = services.FirstOrDefault(s => s.ServiceId == item.ServiceId);
    
    // ✅ استخراج TariffWarning از SnapshotJson
    string tariffWarning = null;
    if (!string.IsNullOrEmpty(item.SnapshotJson))
    {
        try
        {
            var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(item.SnapshotJson);
            if (snapshot?.TariffWarning != null)
            {
                tariffWarning = snapshot.TariffWarning.ToString();
            }
        }
        catch (Exception snapshotEx)
        {
            _logger.Warning(snapshotEx, "⚠️ FACADE: خطا در parse کردن SnapshotJson برای استخراج TariffWarning - ReceptionItemId: {ReceptionItemId}", 
                item.ReceptionItemId);
        }
    }
    
    var itemDto = new ReceptionItemDto
    {
        ServiceId = item.ServiceId,
        Code = service?.ServiceCode ?? "—",
        Name = service?.Title ?? "—",
        Qty = item.Quantity,
        UnitPriceIRR = item.UnitPrice,
        TotalIRR = item.UnitPrice * item.Quantity,
        InsuranceCalculation = insuranceCalculations?.ContainsKey(item.ServiceId) == true
            ? insuranceCalculations[item.ServiceId]
            : null,
        // ✅ افزودن TariffWarning
        TariffWarning = tariffWarning
    };
    
    items.Add(itemDto);
}
```

**تست**:
- ✅ اگر `TariffWarning` در Snapshot موجود باشد → باید به DTO منتقل شود
- ✅ اگر `TariffWarning` موجود نباشد → باید `null` باشد

---

### **فاز 3: Frontend - نمایش Warning** 🎨

#### گام 3.1: بررسی محل نمایش آیتم‌ها در Frontend

**محل**: `Scripts/reception.v2/pricing-ui.js` یا `Scripts/reception.v2/reception-main.js`

**اقدام**:
- پیدا کردن تابعی که آیتم‌ها را در جدول نمایش می‌دهد
- بررسی ساختار HTML جدول

**نکته**: باید بررسی کنیم که آیا از `pricing-ui.js` استفاده می‌شود یا `reception-main.js`

---

#### گام 3.2: افزودن نماد هشدار به جدول

**اقدام**:
- افزودن `<i class="fas fa-exclamation-triangle text-warning">` در ستون "کد خدمت" یا "وضعیت پوشش"
- نمایش Tooltip با پیام `TariffWarning`

**کد پیشنهادی**:
```javascript
// در تابع renderItem یا displayItem
function renderItem(item) {
    var html = '<tr>';
    html += '<td>' + item.Code;
    
    // ✅ نمایش Warning
    if (item.TariffWarning) {
        html += ' <i class="fas fa-exclamation-triangle text-warning" ' +
                'data-bs-toggle="tooltip" ' +
                'data-bs-placement="top" ' +
                'title="' + item.TariffWarning + '" ' +
                'style="cursor: pointer;"></i>';
    }
    
    html += '</td>';
    // ... بقیه columns
    return html;
}
```

**تست**:
- ✅ اگر `TariffWarning` موجود باشد → نماد هشدار نمایش داده می‌شود
- ✅ Hover روی نماد → Tooltip با پیام نمایش داده می‌شود

---

#### گام 3.3: فعال‌سازی Bootstrap Tooltip

**اقدام**:
- بعد از render کردن آیتم‌ها، فعال‌سازی Tooltip

**کد پیشنهادی**:
```javascript
// بعد از render کردن جدول
$('[data-bs-toggle="tooltip"]').tooltip();
```

**تست**:
- ✅ Tooltip به درستی نمایش داده می‌شود

---

## ✅ چک‌لیست پیاده‌سازی

### Backend:

- [ ] **گام 1.1**: بررسی وجود تعرفه در `AddItemAsync`
- [ ] **گام 1.2**: افزودن `TariffWarning` به `snapshot`
- [ ] **گام 2.1**: افزودن `TariffWarning` به `ReceptionItemDto`
- [ ] **گام 2.2**: استخراج `TariffWarning` از `SnapshotJson` در `RecalculateDraftAsync`

### Frontend:

- [ ] **گام 3.1**: بررسی محل نمایش آیتم‌ها
- [ ] **گام 3.2**: افزودن نماد هشدار به جدول
- [ ] **گام 3.3**: فعال‌سازی Bootstrap Tooltip

### تست:

- [ ] **تست 1**: افزودن خدمت با تعیین ست ناقص → Validation خطا می‌دهد
- [ ] **تست 2**: افزودن خدمت با تعیین ست کامل اما تعرفه ناقص → Warning نمایش داده می‌شود
- [ ] **تست 3**: افزودن خدمت با تعیین ست کامل و تعرفه موجود → Warning نمایش داده نمی‌شود
- [ ] **تست 4**: بررسی `SnapshotJson` در دیتابیس → `TariffWarning` باید موجود باشد
- [ ] **تست 5**: بررسی Response API → `TariffWarning` باید در `ReceptionItemDto` موجود باشد

---

## 🚨 نکات مهم

### 1. **Backward Compatibility**:
- ✅ `TariffWarning` اختیاری است (`null` اگر موجود نباشد)
- ✅ آیتم‌های قدیمی که `TariffWarning` ندارند، بدون مشکل کار می‌کنند

### 2. **Performance**:
- ✅ بررسی تعرفه فقط یک بار در `AddItemAsync` انجام می‌شود
- ✅ استخراج از `SnapshotJson` فقط هنگام ساخت DTO انجام می‌شود

### 3. **Error Handling**:
- ✅ اگر Parse کردن `SnapshotJson` با خطا مواجه شود، `TariffWarning` را `null` می‌کنیم
- ✅ لاگ Warning برای دیباگ

---

## 📝 مراحل تست

### تست 1: افزودن خدمت با تعیین ست ناقص

**سناریو**:
1. ایجاد Draft
2. تنظیم بیمه پایه
3. افزودن خدمتی که تعیین ست ندارد

**نتیجه مورد انتظار**:
- ❌ Validation خطا می‌دهد
- ❌ خدمت افزوده نمی‌شود
- ✅ پیام خطای واضح نمایش داده می‌شود

---

### تست 2: افزودن خدمت با تعیین ست کامل اما تعرفه ناقص

**سناریو**:
1. ایجاد Draft
2. تنظیم بیمه پایه
3. افزودن خدمتی که:
   - ✅ تعیین ست دارد (Validation موفق)
   - ❌ تعرفه در DB ندارد (Fallback استفاده می‌شود)

**نتیجه مورد انتظار**:
- ✅ خدمت افزوده می‌شود
- ✅ `TariffWarning` در `SnapshotJson` ذخیره می‌شود
- ✅ `TariffWarning` در `ReceptionItemDto` موجود است
- ✅ نماد هشدار در Frontend نمایش داده می‌شود

---

### تست 3: افزودن خدمت با تعیین ست کامل و تعرفه موجود

**سناریو**:
1. ایجاد Draft
2. تنظیم بیمه پایه
3. افزودن خدمتی که:
   - ✅ تعیین ست دارد
   - ✅ تعرفه در DB موجود است

**نتیجه مورد انتظار**:
- ✅ خدمت افزوده می‌شود
- ✅ `TariffWarning` در `SnapshotJson` `null` است
- ✅ `TariffWarning` در `ReceptionItemDto` `null` است
- ✅ نماد هشدار در Frontend نمایش داده نمی‌شود

---

## 🎯 نتیجه‌گیری

این نقشه راه شامل **6 گام اتمیک** است که باید به ترتیب انجام شوند:

1. ✅ **Backend**: بررسی وجود تعرفه و افزودن به Snapshot
2. ✅ **Backend**: افزودن به DTO و استخراج از Snapshot
3. ✅ **Frontend**: نمایش Warning در جدول

هر گام باید **تست شود** قبل از رفتن به گام بعدی.

---

**تاریخ**: 2025-11-29  
**وضعیت**: 📋 آماده پیاده‌سازی  
**اولویت**: 🔴 بالا

