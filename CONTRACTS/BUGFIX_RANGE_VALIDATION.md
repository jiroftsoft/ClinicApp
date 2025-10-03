# 🐛 **رفع خطای Validation - Range Attribute**

**تاریخ:** ۲ اکتبر ۲۰۲۵  
**شدت:** 🔴 **Critical**  
**وضعیت:** ✅ **رفع شد**

---

## ❌ **خطای رخ داده:**

```
System.Data.Entity.Validation.DbUnexpectedValidationException
Message: An unexpected exception was thrown during validation of 'Value' 
         when invoking System.ComponentModel.DataAnnotations.RangeAttribute.IsValid

Inner Exception: 0.000001 is not a valid value for Decimal
Inner Exception: FormatException: Input string was not in a correct format
```

---

## 🔍 **علت خطا:**

### **1. FactorSetting.Value**

**قبل از اصلاح:**
```csharp
[Range(typeof(decimal), "0.000001", "999999999.999999")]
public decimal Value { get; set; }
```

**مشکل:**
- ❌ استفاده از `string` به جای `numeric` در Range
- ❌ مقدار حداقل نامعقول (0.000001) برای کای به ریال
- ❌ مشکل Culture در تبدیل string به decimal
- ❌ کای حداقل 1 ریال است نه 0.000001!

---

### **2. ServiceComponent.Coefficient**

**قبل از اصلاح:**
```csharp
[Range(typeof(decimal), "0.0001", "999999.9999")]
public decimal Coefficient { get; set; }
```

**مشکل:**
- ❌ استفاده از `string` به جای `numeric` در Range
- ❌ مقدار حداقل غیرمنطقی (0.0001) برای RVU
- ❌ RVU های معمول از 0.01 شروع می‌شوند

---

## ✅ **راه‌حل:**

### **1. FactorSetting.Value (کای به ریال)**

**بعد از اصلاح:**
```csharp
/// <summary>
/// مقدار ضریب (کای) به ریال
/// حداقل: 1 ریال، حداکثر: 999,999,999 ریال
/// </summary>
[Required, Column(TypeName = "decimal")]
[Range(1, 999999999, ErrorMessage = "مقدار کای باید بین 1 تا 999,999,999 ریال باشد.")]
public decimal Value { get; set; }
```

**دلیل تغییر:**
- ✅ استفاده از `numeric` به جای `string`
- ✅ حداقل منطقی: 1 ریال
- ✅ رفع مشکل Culture
- ✅ مطابق با واقعیت: کای‌های مصوب میلیون‌ها ریال هستند

**مثال مقادیر واقعی:**
```
کای فنی عادی (1404): 4,350,000 ریال
کای حرفه‌ای عادی (1404): 8,250,000 ریال
```

---

### **2. ServiceComponent.Coefficient (RVU)**

**بعد از اصلاح:**
```csharp
/// <summary>
/// ضریب RVU این جزء (فنی یا حرفه‌ای)
/// حداقل: 0.01، حداکثر: 999999.99
/// </summary>
[Required]
[Column(TypeName = "decimal")]
[Range(0.01, 999999.99, ErrorMessage = "ضریب RVU باید بین 0.01 تا 999999.99 باشد.")]
public decimal Coefficient { get; set; }
```

**دلیل تغییر:**
- ✅ استفاده از `numeric` به جای `string`
- ✅ حداقل منطقی: 0.01 (RVU معمول)
- ✅ رفع مشکل Culture
- ✅ مطابق با استاندارد RVU

**مثال مقادیر واقعی (از جدول مصوبه):**
```
RVU فنی: 0.5, 0.7, 0.8, 0.9, 1.2
RVU حرفه‌ای: 1.3, 1.8, 2.3, 3.5, 4.0
```

---

## 📊 **مقایسه قبل و بعد:**

| Entity | Property | قبل | بعد |
|--------|----------|-----|-----|
| **FactorSetting** | Value | `[Range(typeof(decimal), "0.000001", "999999999.999999")]` | `[Range(1, 999999999)]` |
| **ServiceComponent** | Coefficient | `[Range(typeof(decimal), "0.0001", "999999.9999")]` | `[Range(0.01, 999999.99)]` |

---

## 🎯 **چرا این خطا رخ داد؟**

### **1. Culture Issue**
```csharp
// ❌ مشکل:
[Range(typeof(decimal), "0.000001", "999999999.999999")]

// وقتی Culture "fa-IR" باشد:
// "0.000001" → تبدیل ناموفق → Exception

// ✅ راه حل:
[Range(1, 999999999)]
// مستقیم numeric، بدون نیاز به تبدیل string
```

### **2. Precision Issue**
```
"0.000001" → خیلی کوچک → مشکل در Validation
```

### **3. Logic Issue**
```
کای به ریال: حداقل 1 ریال (نه 0.000001!)
RVU: حداقل 0.01 (نه 0.0001!)
```

---

## 🧪 **تست:**

### **قبل از Fix:**
```
❌ DbUnexpectedValidationException
❌ SaveChangesAsync فیل می‌شود
❌ Rollback می‌شود
```

### **بعد از Fix:**
```
✅ Validation موفق
✅ SaveChangesAsync اجرا می‌شود
✅ داده‌ها ذخیره می‌شوند
```

---

## 📝 **بهترین روش‌ها (Best Practices):**

### **✅ DO:**
```csharp
// استفاده از numeric مستقیم
[Range(0.01, 999999.99)]

// یا با Type برای اعداد خیلی بزرگ
[Range(1, long.MaxValue)]
```

### **❌ DON'T:**
```csharp
// استفاده از string (Culture-dependent)
[Range(typeof(decimal), "0.01", "999999.99")]

// مقادیر غیرمنطقی
[Range(typeof(decimal), "0.000001", "999999999.999999")]
```

---

## 🔄 **مراحل Reproduce خطا:**

```
1. Run: SystemSeedController → SeedAllData
2. FactorSettingSeedService → Seed کای‌ها
3. SaveChangesAsync → Validation
4. Range Attribute → تبدیل string "0.000001" به decimal
5. Culture Issue → FormatException
6. DbUnexpectedValidationException
```

---

## ✅ **مراحل Fix:**

```
1. FactorSetting.Value: Range → [Range(1, 999999999)]
2. ServiceComponent.Coefficient: Range → [Range(0.01, 999999.99)]
3. Test → Run SeedAllData
4. Success → ✅
```

---

## 📚 **لینک‌های مرتبط:**

- [Data Annotations in EF6](https://docs.microsoft.com/en-us/ef/ef6/modeling/code-first/data-annotations)
- [Range Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.rangeattribute)
- [Culture Issues in .NET](https://docs.microsoft.com/en-us/dotnet/standard/globalization-localization/culture-insensitive-string-operations)

---

## 🎯 **خلاصه:**

| موضوع | قبل | بعد |
|-------|-----|-----|
| **FactorSetting.Value** | ❌ string + Culture Issue | ✅ numeric (1 - 999M) |
| **ServiceComponent.Coefficient** | ❌ string + غیرمنطقی | ✅ numeric (0.01 - 999K) |
| **Validation** | ❌ Exception | ✅ موفق |
| **SaveChanges** | ❌ فیل | ✅ موفق |

---

**تهیه‌کننده:** AI Assistant  
**تاریخ:** ۲ اکتبر ۲۰۲۵  
**نسخه:** 1.0

