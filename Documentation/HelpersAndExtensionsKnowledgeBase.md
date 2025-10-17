# 📚 پایگاه دانش Helpers و Extensions - ClinicApp

## 🎯 **هدف**
این سند شامل تمامی Helpers و Extensions موجود در پروژه ClinicApp است که برای بهینه‌سازی و استفاده مجدد در سراسر پروژه طراحی شده‌اند.

---

## 🔧 **Helpers موجود**

### **1. AgeCalculationHelper.cs**
**مسیر:** `Helpers/AgeCalculationHelper.cs`

**هدف:** محاسبه سن و تاریخ‌های مرتبط برای سیستم‌های پزشکی

**ویژگی‌های کلیدی:**
- محاسبه سن دقیق با در نظر گیری سال کبیسه
- پشتیبانی از تاریخ شمسی و میلادی
- Validation کامل برای محدوده‌های سنی (0-150 سال)
- Logging حرفه‌ای برای سیستم‌های پزشکی

**متدهای اصلی:**
```csharp
// محاسبه سن
int age = birthDate.CalculateAge();
int age = birthDate.CalculateAge(referenceDate);

// دریافت متن سن
string ageText = birthDate.GetAgeText();
string ageText = birthDate.GetAgeText(referenceDate);

// اعتبارسنجی تاریخ تولد
bool isValid = birthDate.IsValidBirthDate();
bool isValid = birthDate.IsValidBirthDate(referenceDate);

// محاسبه سن از تاریخ شمسی
int age = AgeCalculationHelper.CalculateAgeFromPersianDate("1400/01/01");
string ageText = AgeCalculationHelper.GetAgeTextFromPersianDate("1400/01/01");
```

**استفاده در پروژه:**
```csharp
// در ReceptionService
Age = patient.BirthDate.HasValue ? patient.BirthDate.Value.CalculateAge() : 0
```

---

### **2. PhoneNumberHelper.cs**
**مسیر:** `Helpers/PhoneNumberHelper.cs`

**هدف:** نرمال‌سازی شماره تلفن ایرانی

**ویژگی‌های کلیدی:**
- تبدیل به فرمت E.164 استاندارد
- پشتیبانی از ارقام فارسی/عربی
- نرمال‌سازی انواع فرمت‌های شماره تلفن ایرانی

**متدهای اصلی:**
```csharp
// نرمال‌سازی شماره تلفن
string normalized = PhoneNumberHelper.NormalizeToE164("09123456789");
// نتیجه: "+989123456789"
```

**فرمت‌های پشتیبانی شده:**
- `09123456789` → `+989123456789`
- `00989123456789` → `+989123456789`
- `989123456789` → `+989123456789`
- `9123456789` → `+989123456789`

---

### **3. IranianNationalCodeValidator.cs**
**مسیر:** `Helpers/IranianNationalCodeValidator.cs`

**هدف:** اعتبارسنجی کد ملی ایرانی

**ویژگی‌های کلیدی:**
- الگوریتم رسمی سازمان ثبت احوال
- پشتیبانی از ارقام فارسی/عربی
- Validation کامل با جزئیات خطا
- Logging حرفه‌ای

**متدهای اصلی:**
```csharp
// اعتبارسنجی ساده
bool isValid = IranianNationalCodeValidator.IsValid("0065831188");

// اعتبارسنجی با جزئیات
var result = IranianNationalCodeValidator.Validate("0065831188");
if (result.IsValid)
{
    // کد ملی معتبر است
    string normalizedCode = result.NormalizedCode;
}
else
{
    // نمایش پیام خطا
    string errorMessage = result.Message;
}
```

---

### **4. PersianDateHelper.cs**
**مسیر:** `Helpers/PersianDateHelper.cs`

**هدف:** کار با تاریخ‌های شمسی

**ویژگی‌های کلیدی:**
- تبدیل تاریخ میلادی به شمسی
- تبدیل تاریخ شمسی به میلادی
- پشتیبانی از فرمت‌های مختلف
- Validation تاریخ‌های شمسی

**متدهای اصلی:**
```csharp
// تبدیل میلادی به شمسی
string persianDate = PersianDateHelper.ToPersianDate(DateTime.Now);
string persianDateTime = PersianDateHelper.ToPersianDateTime(DateTime.Now);

// تبدیل شمسی به میلادی
DateTime gregorianDate = PersianDateHelper.ToGregorianDate("1400/01/01");
```

---

### **5. PhoneNumberValidator.cs**
**مسیر:** `Helpers/PhoneNumberValidator.cs`

**هدف:** اعتبارسنجی شماره تلفن

**ویژگی‌های کلیدی:**
- Validation شماره تلفن ایرانی
- پشتیبانی از فرمت‌های مختلف
- بررسی صحت شماره تلفن

---

### **6. RegexHelper.cs**
**مسیر:** `Helpers/RegexHelper.cs`

**هدف:** الگوهای Regex مشترک

**ویژگی‌های کلیدی:**
- الگوهای regex از پیش تعریف شده
- بهینه‌سازی عملکرد
- استفاده مجدد در سراسر پروژه

---

### **7. SafeSqlBuilder.cs**
**مسیر:** `Helpers/SafeSqlBuilder.cs`

**هدف:** ساخت SQL امن

**ویژگی‌های کلیدی:**
- جلوگیری از SQL Injection
- ساخت پویای کوئری‌ها
- Parameter binding امن

---

### **8. ValidationResult.cs**
**مسیر:** `Helpers/ValidationResult.cs`

**هدف:** نتیجه اعتبارسنجی

**ویژگی‌های کلیدی:**
- ساختار یکپارچه برای نتایج validation
- پیام‌های خطا
- وضعیت اعتبارسنجی

---

## 🔧 **Extensions موجود**

### **1. DateTimeExtensions.cs**
**مسیر:** `Extensions/DateTimeExtensions.cs`

**هدف:** کار با تاریخ‌ها

**متدهای اصلی:**
```csharp
// تبدیل به تاریخ شمسی
string persianDate = dateTime.ToPersianDate();
string persianDateTime = dateTime.ToPersianDateTime();

// تبدیل تاریخ شمسی به میلادی
DateTime gregorianDate = "1400/01/01".ToDateTime();
DateTime? nullableDate = "1400/01/01".ToDateTimeNullable();
```

---

### **2. PersianDateExtensions.cs**
**مسیر:** `Extensions/PersianDateExtensions.cs`

**هدف:** Persian DatePicker برای Views

**متدهای اصلی:**
```csharp
// DatePicker ساده
@Html.PersianDatePicker("PropertyName")

// DatePicker با مقایسه
@Html.PersianDatePickerWithComparison("StartDate", "EndDate")

// DatePicker با تنظیمات
@Html.PersianDatePickerWithOptions("PropertyName", options)

// محدوده تاریخ
@Html.PersianDateRangePicker()
```

---

### **3. CultureExtensions.cs**
**مسیر:** `Extensions/CultureExtensions.cs`

**هدف:** کار با فرهنگ‌ها

**ویژگی‌های کلیدی:**
- تنظیم فرهنگ فارسی
- مدیریت locale
- پشتیبانی از RTL

---

### **4. ApplicationUserManagerExtensions.cs**
**مسیر:** `Extensions/ApplicationUserManagerExtensions.cs`

**هدف:** مدیریت کاربران

**ویژگی‌های کلیدی:**
- متدهای کمکی برای UserManager
- مدیریت نقش‌ها
- اعتبارسنجی کاربران

---

## 🎯 **استفاده در پروژه**

### **در Services:**
```csharp
// محاسبه سن
var age = patient.BirthDate.CalculateAge();

// اعتبارسنجی کد ملی
var nationalCodeResult = IranianNationalCodeValidator.Validate(nationalCode);
if (!nationalCodeResult.IsValid)
{
    // مدیریت خطا
}

// نرمال‌سازی شماره تلفن
var normalizedPhone = PhoneNumberHelper.NormalizeToE164(phoneNumber);
```

### **در Views:**
```csharp
// DatePicker شمسی
@Html.PersianDatePickerFor(m => m.BirthDate)

// محدوده تاریخ
@Html.PersianDateRangePicker()
```

### **در Controllers:**
```csharp
// اعتبارسنجی کد ملی
if (!IranianNationalCodeValidator.IsValid(nationalCode))
{
    ModelState.AddModelError("NationalCode", "کد ملی نامعتبر است");
    return View(model);
}
```

---

## 📋 **چک‌لیست استفاده**

### **✅ قبل از ایجاد Helper جدید:**
1. بررسی کنید آیا Helper مشابهی وجود دارد
2. بررسی کنید آیا می‌توان از Helper موجود استفاده کرد
3. بررسی کنید آیا می‌توان Helper موجود را گسترش داد

### **✅ هنگام استفاده از Helpers:**
1. از using مناسب استفاده کنید
2. Exception handling مناسب پیاده‌سازی کنید
3. Logging مناسب اضافه کنید
4. Performance را در نظر بگیرید

### **✅ هنگام ایجاد Helper جدید:**
1. طبق اصول SOLID طراحی کنید
2. Documentation کامل اضافه کنید
3. Unit tests بنویسید
4. Logging مناسب پیاده‌سازی کنید
5. Performance بهینه‌سازی کنید

---

## 🚀 **بهینه‌سازی‌های آینده**

### **1. Caching:**
- Cache کردن نتایج محاسبات پیچیده
- Cache کردن validation results

### **2. Async Support:**
- پیاده‌سازی async/await برای عملیات سنگین
- استفاده از Task.Run برای CPU-bound operations

### **3. Configuration:**
- تنظیمات قابل تغییر برای محدوده‌ها
- Configuration-based validation rules

### **4. Internationalization:**
- پشتیبانی از زبان‌های مختلف
- Localized error messages

---

## 📞 **تماس و پشتیبانی**

برای سوالات و پیشنهادات در مورد Helpers و Extensions، با تیم توسعه تماس بگیرید.

**آخرین به‌روزرسانی:** 2024
**نسخه:** 1.0.0
