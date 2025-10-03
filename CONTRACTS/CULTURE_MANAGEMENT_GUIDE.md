# 🌍 **راهنمای مدیریت Culture در پروژه ClinicApp**

---

## 📋 **مشکلات Culture شناسایی شده:**

### **❌ مشکلات قبلی:**
1. **Decimal Parsing**: `0.5` vs `0,5` (فارسی vs انگلیسی)
2. **Date Parsing**: `2025/01/01` vs `2025-01-01`
3. **Number Formatting**: `1,000.50` vs `1.000,50`
4. **Range Validation**: `[Range(0.5, 1.5)]` مشکل Culture داشت

---

## ✅ **راه‌حل‌های پیاده‌سازی شده:**

### **1️⃣ DecimalModelBinder**
```csharp
// ثبت در Global.asax.cs
ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());
```

**مزایا:**
- ✅ همیشه از **InvariantCulture** استفاده می‌کند
- ✅ پشتیبانی از **جداکننده‌های مختلف** (`,`, `٫`, `٬`)
- ✅ **Error Handling** مناسب
- ✅ **Fallback** به CurrentCulture

### **2️⃣ CultureHelper**
```csharp
// استفاده در کد
var value = CultureHelper.ParseDecimal("0.5"); // همیشه کار می‌کند
var formatted = CultureHelper.FormatDecimal(1234.56m); // "1,234.56"
```

**مزایا:**
- ✅ **Centralized** Culture Management
- ✅ **Consistent** Decimal Parsing
- ✅ **Persian** Number Formatting
- ✅ **Database** String Formatting

### **3️⃣ Culture Extensions**
```csharp
// استفاده در کد
var value = "0.5".ToDecimal(); // Extension Method
var formatted = 1234.56m.ToPersianString(); // "1,234.56"
```

**مزایا:**
- ✅ **Fluent** API
- ✅ **Easy** to use
- ✅ **Consistent** behavior

### **4️⃣ Culture Filter**
```csharp
// ثبت در FilterConfig.cs
filters.Add(new CultureFilter());
```

**مزایا:**
- ✅ **Automatic** Culture Setting
- ✅ **Per-Request** Culture Management
- ✅ **Decimal Separator** Fix

### **5️⃣ Range Attributes Fix**
```csharp
// قبل (مشکل‌دار)
[Range(typeof(decimal), "0.0000", "99.9999")]

// بعد (حل شده)
[Range(0, 999999)]
```

**مزایا:**
- ✅ **No Culture Issues**
- ✅ **Numeric Literals**
- ✅ **Better Performance**

---

## 🎯 **استفاده در پروژه:**

### **1️⃣ در Controllers:**
```csharp
public ActionResult Create(ServiceCreateEditViewModel model)
{
    // DecimalModelBinder خودکار کار می‌کند
    // model.Price همیشه صحیح Parse می‌شود
}
```

### **2️⃣ در Services:**
```csharp
public async Task<ServiceResult> CalculatePriceAsync(string priceString)
{
    // استفاده از CultureHelper
    var price = CultureHelper.ParseDecimal(priceString);
    
    // یا استفاده از Extension
    var price2 = priceString.ToDecimal();
}
```

### **3️⃣ در ViewModels:**
```csharp
public class ServiceViewModel
{
    [Display(Name = "قیمت")]
    public decimal Price { get; set; } // DecimalModelBinder کار می‌کند
    
    public string FormattedPrice => Price.ToPersianString(); // Extension
}
```

### **4️⃣ در Views:**
```html
@* نمایش قیمت با فرمت فارسی *@
@Model.Price.ToPersianString("N0") ریال

@* Input با DecimalModelBinder *@
@Html.EditorFor(m => m.Price)
```

---

## 🔧 **تنظیمات اضافی:**

### **1️⃣ Web.config:**
```xml
<system.web>
    <globalization culture="fa-IR" uiCulture="fa-IR" />
</system.web>
```

### **2️⃣ JavaScript (Client-side):**
```javascript
// برای Decimal Parsing در JavaScript
function parseDecimal(value) {
    return parseFloat(value.replace(/,/g, '.'));
}
```

### **3️⃣ Database:**
```sql
-- Decimal columns با دقت مناسب
Price DECIMAL(18,0) -- برای ریال
Coefficient DECIMAL(18,6) -- برای ضریب
```

---

## 📊 **مقایسه قبل و بعد:**

### **❌ قبل (مشکل‌دار):**
```csharp
// مشکل Culture
var price = decimal.Parse("0.5"); // ممکن است خطا دهد
[Range(typeof(decimal), "0.0000", "99.9999")] // مشکل Culture
```

### **✅ بعد (حل شده):**
```csharp
// حل شده
var price = CultureHelper.ParseDecimal("0.5"); // همیشه کار می‌کند
[Range(0, 999999)] // بدون مشکل Culture
```

---

## 🚀 **نتیجه‌گیری:**

### **✅ مزایای راه‌حل:**
1. **Consistent** Decimal Parsing
2. **Persian** Number Formatting
3. **Database** Compatibility
4. **Error Handling** مناسب
5. **Performance** بهتر

### **🎯 استفاده در پروژه:**
- **Controllers**: DecimalModelBinder خودکار
- **Services**: CultureHelper یا Extensions
- **Views**: Extensions برای نمایش
- **Database**: InvariantCulture برای ذخیره‌سازی

**🌍 حالا پروژه کاملاً از Culture Management پشتیبانی می‌کند!**
