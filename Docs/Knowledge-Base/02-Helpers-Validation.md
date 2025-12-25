# ✅ راهنمای کامل Helpers اعتبارسنجی

**نسخه:** 1.0.0  
**تعداد Helpers:** 6

---

## 📋 فهرست

1. [IranianNationalCodeValidator](#1-iraniannationalcodevalidator)
2. [PhoneNumberValidator](#2-phonenumbervalidator)
3. [PhoneNumberHelper](#3-phonenumberhelper)
4. [IdentityValidators](#4-identityvalidators)
5. [ValidationResult](#5-validationresult)
6. [SecurityValidationResult](#6-securityvalidationresult)

---

## 1️⃣ IranianNationalCodeValidator

**مسیر:** `Helpers/IranianNationalCodeValidator.cs`  
**حجم:** 9,588 بایت  
**هدف:** اعتبارسنجی کد ملی ایرانی (الگوریتم رسمی)

### 📌 توابع:

```csharp
// ✅ Validation کد ملی
public static bool IsValid(string nationalCode)
```

**مثال‌ها:**
```csharp
// مثال 1: بررسی کد ملی
var isValid = IranianNationalCodeValidator.IsValid("0123456789");
// خروجی: true/false

// مثال 2: در Controller
if (!IranianNationalCodeValidator.IsValid(model.NationalCode))
{
    ModelState.AddModelError("NationalCode", "کد ملی نامعتبر است");
    return View(model);
}

// مثال 3: در ViewModel
[CustomValidation(typeof(IranianNationalCodeValidator), "IsValid")]
public string NationalCode { get; set; }
```

**قواعد:**
- ✅ باید 10 رقم باشد
- ✅ نباید تمام ارقام یکسان باشند (مثل "1111111111")
- ✅ رقم آخر باید با الگوریتم check digit مطابقت داشته باشد

---

## 2️⃣ PhoneNumberValidator

**مسیر:** `Helpers/PhoneNumberValidator.cs`  
**حجم:** 3,235 بایت

### 📌 توابع:

```csharp
// ✅ Validation شماره موبایل
public static bool IsValidMobile(string phoneNumber)

// ✅ Validation شماره تلفن ثابت
public static bool IsValidPhone(string phoneNumber)
```

**مثال‌ها:**
```csharp
// مثال 1: موبایل
var isValid = PhoneNumberValidator.IsValidMobile("09123456789");
// خروجی: true

var isValid = PhoneNumberValidator.IsValidMobile("09999999999");
// خروجی: true

var isValid = PhoneNumberValidator.IsValidMobile("0812345678"); // 10 رقم، نه 11
// خروجی: false

// مثال 2: تلفن ثابت
var isValid = PhoneNumberValidator.IsValidPhone("02112345678");
// خروجی: true

var isValid = PhoneNumberValidator.IsValidPhone("03432221234");
// خروجی: true

// مثال 3: در Controller
if (!PhoneNumberValidator.IsValidMobile(model.PhoneNumber))
{
    ModelState.AddModelError("PhoneNumber", "شماره موبایل نامعتبر است");
}
```

**قواعد موبایل:**
- ✅ باید 11 رقم باشد
- ✅ باید با "09" شروع شود
- ✅ فرمت: `09XXXXXXXXX`

**قواعد تلفن ثابت:**
- ✅ باید 11 رقم باشد
- ✅ باید با "0" شروع شود
- ✅ فرمت: `0XXXXXXXXXX`

---

## 3️⃣ PhoneNumberHelper

**مسیر:** `Helpers/PhoneNumberHelper.cs`  
**حجم:** 1,546 بایت  
**هدف:** نرمال‌سازی شماره تلفن

### 📌 توابع:

```csharp
// ✅ نرمال‌سازی شماره به E.164
public static string NormalizePhoneNumber(string phoneNumber)

// ✅ فرمت نمایشی
public static string FormatPhoneNumber(string phoneNumber)

// ✅ پاکسازی شماره
public static string CleanPhoneNumber(string phoneNumber)
```

**مثال‌ها:**
```csharp
// مثال 1: پاکسازی
var cleaned = PhoneNumberHelper.CleanPhoneNumber("(021) 1234-5678");
// خروجی: "02112345678"

var cleaned = PhoneNumberHelper.CleanPhoneNumber("0912 345 6789");
// خروجی: "09123456789"

// مثال 2: نرمال‌سازی
var normalized = PhoneNumberHelper.NormalizePhoneNumber("09123456789");
// خروجی: "+989123456789" (E.164 format)

// مثال 3: فرمت نمایشی
var formatted = PhoneNumberHelper.FormatPhoneNumber("09123456789");
// خروجی: "0912-345-6789"
```

**Use Cases:**
- ✅ قبل از ذخیره در دیتابیس
- ✅ قبل از ارسال SMS
- ✅ نمایش در UI
- ✅ Integration با API های خارجی

---

## 4️⃣ IdentityValidators

**مسیر:** `Helpers/Validation/IdentityValidators.cs`  
**حجم:** 1,450 بایت

### 📌 توابع:

```csharp
// ✅ Validation پسورد
public static bool IsValidPassword(string password)

// ✅ Validation نام کاربری
public static bool IsValidUsername(string username)
```

**مثال‌ها:**
```csharp
// مثال 1: پسورد
var isValid = IdentityValidators.IsValidPassword("MyP@ssw0rd123");
// خروجی: true

var isValid = IdentityValidators.IsValidPassword("123456");
// خروجی: false (کوتاه است)

// مثال 2: Username
var isValid = IdentityValidators.IsValidUsername("user123");
// خروجی: true
```

**قواعد پسورد:**
- ✅ حداقل 8 کاراکتر
- ✅ حداقل یک حرف بزرگ
- ✅ حداقل یک حرف کوچک
- ✅ حداقل یک عدد
- ✅ حداقل یک کاراکتر ویژه

---

## 5️⃣ ValidationResult

**مسیر:** `Helpers/ValidationResult.cs`  
**حجم:** 3,700 بایت

### 📌 ساختار:

```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<string, string> Errors { get; set; }
}
```

**مثال‌ها:**
```csharp
// مثال 1: ایجاد نتیجه موفق
var result = new ValidationResult 
{
    IsValid = true
};

// مثال 2: ایجاد نتیجه ناموفق
var result = new ValidationResult 
{
    IsValid = false,
    ErrorMessage = "کد ملی نامعتبر است"
};

// مثال 3: چند خطا
var result = new ValidationResult 
{
    IsValid = false,
    Errors = new Dictionary<string, string>
    {
        { "NationalCode", "کد ملی نامعتبر است" },
        { "PhoneNumber", "شماره موبایل نامعتبر است" }
    }
};

// مثال 4: استفاده در سرویس
public ValidationResult ValidatePatient(PatientViewModel model)
{
    var result = new ValidationResult { IsValid = true };
    
    if (!IranianNationalCodeValidator.IsValid(model.NationalCode))
    {
        result.IsValid = false;
        result.ErrorMessage = "کد ملی نامعتبر است";
    }
    
    return result;
}
```

---

## 6️⃣ SecurityValidationResult

**مسیر:** `Helpers/SecurityValidationResult.cs`  
**حجم:** 4,349 بایت

### 📌 ساختار:

```csharp
public class SecurityValidationResult : ValidationResult
{
    public string SecurityLevel { get; set; } // Low, Medium, High
    public List<string> SecurityIssues { get; set; }
}
```

**مثال:**
```csharp
// مثال: بررسی امنیت پسورد
var result = new SecurityValidationResult
{
    IsValid = true,
    SecurityLevel = "High",
    SecurityIssues = new List<string>()
};

if (password.Length < 12)
{
    result.SecurityLevel = "Medium";
    result.SecurityIssues.Add("پسورد کمتر از 12 کاراکتر است");
}
```

---

## 🎯 Workflow کامل (Validation در Form)

### **1. در ViewModel:**
```csharp
public class PatientCreateViewModel
{
    [Required(ErrorMessage = "کد ملی الزامی است")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
    public string NationalCode { get; set; }
    
    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "شماره موبایل باید 11 رقم باشد")]
    public string PhoneNumber { get; set; }
}
```

### **2. در Controller:**
```csharp
[HttpPost]
public async Task<ActionResult> Create(PatientCreateViewModel model)
{
    // ✅ Validation کد ملی
    if (!IranianNationalCodeValidator.IsValid(model.NationalCode))
    {
        ModelState.AddModelError("NationalCode", "کد ملی نامعتبر است");
    }
    
    // ✅ Validation شماره موبایل
    if (!PhoneNumberValidator.IsValidMobile(model.PhoneNumber))
    {
        ModelState.AddModelError("PhoneNumber", "شماره موبایل نامعتبر است");
    }
    
    if (!ModelState.IsValid)
    {
        return View(model);
    }
    
    // ✅ نرمال‌سازی شماره قبل از ذخیره
    model.PhoneNumber = PhoneNumberHelper.CleanPhoneNumber(model.PhoneNumber);
    
    // ادامه...
}
```

---

## 📊 خلاصه

| Helper | استفاده | مثال |
|--------|---------|------|
| `IranianNationalCodeValidator.IsValid()` | کد ملی | `true/false` |
| `PhoneNumberValidator.IsValidMobile()` | موبایل | `true/false` |
| `PhoneNumberValidator.IsValidPhone()` | تلفن ثابت | `true/false` |
| `PhoneNumberHelper.CleanPhoneNumber()` | پاکسازی | `"09123456789"` |
| `PhoneNumberHelper.FormatPhoneNumber()` | فرمت نمایشی | `"0912-345-6789"` |

---

**نسخه:** 1.0.0  
**آخرین به‌روزرسانی:** 1404/10/05

🎉 **راهنمای Validation آماده است!** 🎉

