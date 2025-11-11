# 📋 توضیح کامل فیلد GroupCode در جدول Service

## 🎯 مقدمه

فیلد `GroupCode` در جدول `Service` برای **دسته‌بندی خدمات بر اساس گروه‌های کتاب ارزش نسبی خدمات سلامت** استفاده می‌شود. این فیلد نقش مهمی در **فیلتر کردن پزشکان** و **قوانین تعرفه** دارد.

---

## 📌 تعریف GroupCode

### در مدل Service:

```csharp
/// <summary>
/// گروه خدمات (۱–۷)
/// طبق نقشه پیوندی: برای قواعد هشتگ‌دار و تعرفه
/// </summary>
[Range(1, 7, ErrorMessage = "گروه خدمات باید بین 1 تا 7 باشد.")]
public int? GroupCode { get; set; }
```

### ویژگی‌ها:
- **نوع:** `int?` (Nullable)
- **محدوده:** 1 تا 7
- **اختیاری:** بله (Nullable)
- **ایندکس:** دارد (`IX_Service_GroupCode`)

---

## 🔢 گروه‌های خدمات (GroupCode 1-7)

### نقشه گروه‌ها:

| GroupCode | محدوده کدهای خدمت | نوع خدمات | مثال |
|-----------|-------------------|-----------|------|
| **1** | 100xxx | اعمال و رویه‌های گروه 1 | پانسمان، شست‌وشوی زخم |
| **2** | 200xxx | اعمال و رویه‌های گروه 2 | کشیدن بخیه، بخیه ساده |
| **3** | 300xxx | اعمال و رویه‌های گروه 3 | دبریدمان، ترمیم زخم |
| **4** | 400xxx | اعمال و رویه‌های گروه 4 | بیوپسی، اکسیزیون پوستی |
| **5** | 500xxx | اعمال و رویه‌های گروه 5 | درناژ آبسه، تزریق |
| **6** | 600xxx | اعمال و رویه‌های گروه 6 | تزریقات وریدی/عضلانی |
| **7** | 700xxx | اعمال و رویه‌های گروه 7 | لوله‌گذاری، رویه‌های پاراکلینیک |

### ⚠️ نکته مهم:
- **کدهای 97xxxx (ویزیت‌ها)** در **کد 7 کتاب ارزش نسبی** قرار دارند
- اما `GroupCode` برای ویزیت‌ها معمولاً **NULL** یا **7** است
- `GroupCode` بیشتر برای **اعمال و رویه‌های پزشکی** (100xxx تا 700xxx) استفاده می‌شود

---

## 🎯 کاربردهای GroupCode در سیستم

### 1️⃣ فیلتر کردن پزشکان (ReceptionFacade.cs)

`GroupCode` برای **فیلتر کردن پزشکان مناسب** برای هر خدمت استفاده می‌شود:

```csharp
// برای خدمات تخصصی (GroupCode = 1-7)، فقط پزشکان متخصص را برگردان
// برای خدمات عمومی، همه پزشکان را برگردان
if (service.GroupCode.HasValue && service.GroupCode.Value > 1)
{
    // خدمت تخصصی: فقط پزشکان متخصص (نه عمومی)
    filteredDoctors = doctorsBase
        .Where(d => !string.IsNullOrEmpty(d.Specialization) && 
                   !d.Specialization.Contains("عمومی") &&
                   !d.Specialization.Contains("General"))
        .ToList();
}
else
{
    // خدمت عمومی: همه پزشکان
    filteredDoctors = doctorsBase.ToList();
}
```

#### منطق:
- **GroupCode = NULL یا 1:** خدمت عمومی → همه پزشکان (عمومی + متخصص)
- **GroupCode = 2-7:** خدمت تخصصی → فقط پزشکان متخصص

### 2️⃣ قوانین تعرفه (TariffResolver.cs)

`GroupCode` در `TariffResolver` خوانده می‌شود اما فعلاً استفاده مستقیمی از آن نمی‌شود. احتمالاً برای **قوانین آینده** یا **استثناهای خاص** در نظر گرفته شده است.

### 3️⃣ قواعد هشتگ‌دار

طبق کامنت در مدل: "برای قواعد هشتگ‌دار و تعرفه"

`GroupCode` می‌تواند برای **تعیین قوانین خاص** برای خدمات هشتگ‌دار در گروه‌های مختلف استفاده شود.

---

## 🔍 تفاوت GroupCode با ServiceCode

### GroupCode:
- **محدوده:** 1 تا 7
- **نوع:** عدد صحیح (int)
- **کاربرد:** دسته‌بندی کلی خدمات
- **مثال:** GroupCode = 3 (گروه 3)

### ServiceCode:
- **محدوده:** رشته (string)
- **نوع:** کد کامل خدمت
- **کاربرد:** شناسه یکتای خدمت
- **مثال:** ServiceCode = "300001" (خدمت خاص در گروه 3)

### رابطه:
```
ServiceCode = "300001" → GroupCode = 3
ServiceCode = "500100" → GroupCode = 5
ServiceCode = "970000" → GroupCode = NULL یا 7 (ویزیت)
```

---

## 🧮 استفاده از GroupCode در محاسبه K-Factor

### ⚠️ نکته مهم:

**فعلاً در `ServiceCalculationService` از `GroupCode` استفاده نمی‌شود!**

به جای آن، از **رقم اول `ServiceCode`** برای انتخاب K-Factor استفاده می‌شود:

```csharp
// ❌ فعلاً استفاده نمی‌شود:
// if (service.GroupCode.HasValue && service.GroupCode.Value >= 1 && service.GroupCode.Value <= 7)

// ✅ استفاده فعلی:
var firstDigit = GetFirstDigit(service.ServiceCode);
if (firstDigit >= 1 && firstDigit <= 7)
{
    targetScope = FactorScope.Hash_1_7; // 2,750,000 ریال
}
```

### پیشنهاد بهبود:

می‌توان از `GroupCode` به عنوان **Fallback** یا **تأیید** استفاده کرد:

```csharp
// استفاده از GroupCode برای تأیید
if (service.GroupCode.HasValue)
{
    if (service.GroupCode.Value >= 1 && service.GroupCode.Value <= 7)
    {
        targetScope = FactorScope.Hash_1_7;
    }
    // GroupCode = 8 یا 9 وجود ندارد (محدوده 1-7 است)
}
else
{
    // Fallback به منطق ServiceCode
    var firstDigit = GetFirstDigit(service.ServiceCode);
    // ...
}
```

---

## 📊 جدول کامل کاربردها

| کاربرد | استفاده فعلی | استفاده پیشنهادی |
|--------|--------------|------------------|
| **فیلتر پزشکان** | ✅ استفاده می‌شود | ✅ ادامه استفاده |
| **محاسبه K-Factor** | ❌ استفاده نمی‌شود | ✅ می‌تواند به عنوان Fallback استفاده شود |
| **قوانین تعرفه** | ⚠️ خوانده می‌شود اما استفاده نمی‌شود | ✅ برای قوانین آینده |
| **گزارش‌گیری** | ❌ استفاده نمی‌شود | ✅ می‌تواند برای دسته‌بندی گزارش‌ها استفاده شود |

---

## 🔧 نحوه تعیین GroupCode

### برای خدمات جدید:

#### 1. اعمال و رویه‌های پزشکی (100xxx تا 700xxx):
```
ServiceCode = "300001" → GroupCode = 3
ServiceCode = "500100" → GroupCode = 5
ServiceCode = "700050" → GroupCode = 7
```

#### 2. ویزیت‌ها (97xxxx):
```
ServiceCode = "970000" → GroupCode = NULL یا 7
ServiceCode = "970015" → GroupCode = NULL یا 7
```

#### 3. خدمات تخصصی (800xxx):
```
ServiceCode = "800001" → GroupCode = NULL (خارج از محدوده 1-7)
```

### الگوریتم پیشنهادی:

```csharp
public int? DetermineGroupCode(string serviceCode)
{
    if (string.IsNullOrWhiteSpace(serviceCode))
        return null;
    
    // کدهای 97xxxx (ویزیت‌ها)
    if (serviceCode.StartsWith("97"))
        return 7; // یا NULL
    
    // استخراج رقم اول
    var firstDigit = GetFirstDigit(serviceCode);
    
    // کدهای 1-7
    if (firstDigit >= 1 && firstDigit <= 7)
        return firstDigit;
    
    // کدهای 8-9 خارج از محدوده GroupCode هستند
    return null;
}
```

---

## 📋 مثال‌های عملی

### مثال 1: خدمت با GroupCode = 3

```csharp
Service service = new Service
{
    ServiceCode = "300001",
    Title = "دبریدمان زخم",
    GroupCode = 3, // گروه 3
    IsHashtagged = true
};

// استفاده در فیلتر پزشکان:
// GroupCode = 3 > 1 → فقط پزشکان متخصص
```

### مثال 2: ویزیت با GroupCode = NULL

```csharp
Service service = new Service
{
    ServiceCode = "970000",
    Title = "ویزیت پزشک عمومی",
    GroupCode = null, // یا 7
    IsHashtagged = true
};

// استفاده در فیلتر پزشکان:
// GroupCode = NULL → همه پزشکان (عمومی + متخصص)
```

### مثال 3: خدمت عمومی با GroupCode = 1

```csharp
Service service = new Service
{
    ServiceCode = "100001",
    Title = "پانسمان ساده",
    GroupCode = 1, // گروه 1 (عمومی)
    IsHashtagged = false
};

// استفاده در فیلتر پزشکان:
// GroupCode = 1 → همه پزشکان (عمومی + متخصص)
```

---

## ⚠️ نکات مهم

### 1. محدوده GroupCode:
- **محدوده:** 1 تا 7
- **NULL:** مجاز است (برای خدمات خارج از محدوده 1-7)
- **مثال:** کدهای 800xxx و 900xxx → GroupCode = NULL

### 2. رابطه با ServiceCode:
- `GroupCode` معمولاً از **رقم اول `ServiceCode`** استخراج می‌شود
- **استثنا:** کدهای 97xxxx → GroupCode = 7 یا NULL

### 3. استفاده در محاسبه:
- **فعلاً:** از `GroupCode` در محاسبه K-Factor استفاده نمی‌شود
- **پیشنهاد:** می‌تواند به عنوان Fallback یا تأیید استفاده شود

### 4. فیلتر پزشکان:
- **GroupCode = NULL یا 1:** همه پزشکان
- **GroupCode = 2-7:** فقط پزشکان متخصص

---

## 🔧 پیشنهادات بهبود

### 1. استفاده از GroupCode در محاسبه K-Factor:

```csharp
// پیشنهاد: استفاده از GroupCode به عنوان اولویت اول
if (service.GroupCode.HasValue && service.GroupCode.Value >= 1 && service.GroupCode.Value <= 7)
{
    targetScope = FactorScope.Hash_1_7; // 2,750,000 ریال
}
else if (service.ServiceCode.StartsWith("97"))
{
    targetScope = FactorScope.Hash_1_7; // 2,750,000 ریال
}
// ...
```

### 2. خودکارسازی تعیین GroupCode:

```csharp
// در CreateServiceAsync یا UpdateServiceAsync
if (!model.GroupCode.HasValue && !string.IsNullOrEmpty(model.ServiceCode))
{
    model.GroupCode = DetermineGroupCode(model.ServiceCode);
}
```

### 3. اعتبارسنجی GroupCode:

```csharp
// در ServiceValidator
if (model.GroupCode.HasValue)
{
    if (model.GroupCode.Value < 1 || model.GroupCode.Value > 7)
    {
        // خطا: GroupCode باید بین 1 تا 7 باشد
    }
    
    // بررسی تطابق با ServiceCode
    var expectedGroupCode = DetermineGroupCode(model.ServiceCode);
    if (expectedGroupCode.HasValue && model.GroupCode.Value != expectedGroupCode.Value)
    {
        // هشدار: GroupCode با ServiceCode تطابق ندارد
    }
}
```

---

## 📚 منابع و مراجع

1. **مدل Service.cs** - تعریف `GroupCode`
2. **ReceptionFacade.cs** - استفاده در فیلتر پزشکان
3. **TariffResolver.cs** - خواندن `GroupCode`
4. **Documentation/TariffCodesAndGroups.md** - گروه‌بندی کدها

---

## ✅ خلاصه

### GroupCode چیست؟
- **فیلد:** `int?` در جدول `Service`
- **محدوده:** 1 تا 7 (یا NULL)
- **کاربرد:** دسته‌بندی خدمات بر اساس گروه‌های کتاب ارزش نسبی

### کاربردهای فعلی:
1. ✅ **فیلتر پزشکان:** تعیین اینکه خدمت عمومی است یا تخصصی
2. ⚠️ **قوانین تعرفه:** خوانده می‌شود اما استفاده مستقیمی ندارد
3. ❌ **محاسبه K-Factor:** فعلاً استفاده نمی‌شود

### پیشنهاد:
- استفاده از `GroupCode` به عنوان **اولویت اول** در انتخاب K-Factor
- **خودکارسازی** تعیین `GroupCode` از `ServiceCode`
- **اعتبارسنجی** تطابق `GroupCode` با `ServiceCode`

---

**⚠️ توجه:** این مستندات بر اساس کد فعلی سیستم تهیه شده است. در صورت تغییر منطق، این مستندات باید به‌روزرسانی شوند.

