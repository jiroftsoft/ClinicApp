# 🧰 راهنمای استفاده از Helper ها و Extension ها

## 📌 خلاصه

این پروژه شامل **14 فایل** Helper و Extension است که به صورت سیستماتیک و با رعایت اصول SRP پیاده‌سازی شده‌اند.

---

## 📂 Extensions (پوشه /Extensions)

### 1. **StringExtensions.cs**
```csharp
"متن طولانی".Truncate(10);                    // برش با ...
"1234567890".Mask(4);                          // ************7890
"Title Page".ToSlug();                          // title-page
email.HasValue();                               // بررسی null/empty
email.IsValidEmail();                           // اعتبارسنجی ایمیل
```

### 2. **DateTimeExtensions.cs**
```csharp
DateTime.Now.StartOfDay();                      // 00:00:00
DateTime.Now.EndOfWeek();                       // انتهای هفته
birthDate.CalculateAge();                       // محاسبه سن
postDate.ToRelativeTime();                      // "3 ساعت پیش"
date.IsBetween(start, end);                     // بررسی محدوده
DateTime.Now.ToPersianDate();                   // تبدیل به شمسی
```

### 3. **NumericExtensions.cs**
```csharp
1500000m.ToCurrency();                          // "1,500,000 ریال"
price.ApplyDiscount(10);                        // اعمال تخفیف
age.IsBetween(18, 65);                          // بررسی محدوده
1536000L.ToFileSize();                          // "1.46 MB"
```

### 4. **CollectionExtensions.cs**
```csharp
list.IsNullOrEmpty();                           // بررسی امن
items.ForEach(x => Console.WriteLine(x));       // حلقه
users.DistinctBy(u => u.Id);                    // یکتا بر اساس Property
numbers.Chunk(10);                              // تقسیم به دسته
cards.Shuffle();                                // به هم زدن ترتیب
```

### 5. **ObjectExtensions.cs**
```csharp
user.DeepClone();                               // کپی عمیق
user.ToDictionary();                            // تبدیل به Dictionary
obj.GetPropertyValue("Name");                   // دریافت Property
obj.SetPropertyValue("Name", "Ali");            // تنظیم Property
object.ToJson();                                // تبدیل به JSON
```

### 6. **EnumExtensions.cs** (موجود)
```csharp
status.GetDescription();                        // دریافت Description
status.ToSelectList();                          // تبدیل به DropDown
"Active".ParseEnum(Status.Inactive);            // تبدیل string به Enum
```

---

## 🛠️ Helpers (پوشه /Helpers)

### 7. **CacheHelper.cs**
```csharp
CacheHelper.GetOrCreate("users", () => db.Users.ToList(), 30);
CacheHelper.Set("key", value, 60);
CacheHelper.Get<List<User>>("users");
CacheHelper.Remove("key");
CacheHelper.Clear();
```

### 8. **RetryHelper.cs**
```csharp
RetryHelper.Retry(() => CallApi(), 3, 2000);
await RetryHelper.RetryAsync(async () => await CallApiAsync(), 3);
RetryHelper.RetryWithExponentialBackoff(() => CallExternal());
```

### 9. **SecurityHelper.cs**
```csharp
SecurityHelper.HashPassword(password);
SecurityHelper.GenerateSalt();
SecurityHelper.Encrypt(text, key);
SecurityHelper.Decrypt(encrypted, key);
SecurityHelper.GenerateRandomToken(32);
SecurityHelper.SanitizeInput(userInput);
```

### 10. **ValidationHelper.cs**
```csharp
ValidationHelper.IsValid(value);
ValidationHelper.IsInRange(age, 18, 65);
ValidationHelper.IsInList(status, "Active", "Inactive");
var result = ValidationHelper.ValidateRequired("Name", name);
```

### 11. **FileHelper.cs**
```csharp
FileHelper.ReadJson<Config>("config.json");
FileHelper.WriteJson("data.json", data);
FileHelper.GenerateUniqueFileName("photo.jpg");
FileHelper.SafeCopy(source, destination);
FileHelper.HasExtension(fileName, "jpg", "png");
```

### 12. **HtmlHelper.cs**
```csharp
HtmlHelper.StripHtml(html);
HtmlHelper.TextToHtml(text);
HtmlHelper.BuildLink(url, "Link Text", "btn-primary");
HtmlHelper.SanitizeHtml(userHtml);
HtmlHelper.BuildUnorderedList(items);
```

### 13. **UrlHelper.cs**
```csharp
UrlHelper.BuildQueryString(new Dictionary<string, string> { ... });
UrlHelper.ParseQueryString(url);
UrlHelper.CombineUrl("api", "users", "123");
UrlHelper.IsValidUrl(url);
UrlHelper.GetDomain(url);
```

### 14. **ImageHelper.cs**
```csharp
ImageHelper.ResizeImage(imageBytes, 800, 600);
ImageHelper.ToBase64(imageBytes);
ImageHelper.CreateThumbnail(imageBytes, 150);
ImageHelper.IsValidImage(bytes);
ImageHelper.ConvertToJpeg(imageBytes, 90);
```

---

## ✅ ویژگی‌های کلیدی

1. **رعایت SRP**: هر کلاس یک مسئولیت واحد دارد
2. **XML Documentation**: تمام متدها مستند شده‌اند
3. **Null Safety**: بررسی null در تمام متدها
4. **Error Handling**: مدیریت خطا در متدهای حساس
5. **مثال‌های کاربردی**: در کامنت‌ها ذکر شده
6. **Performance**: بهینه‌سازی شده

---

## 📊 آمار

- **تعداد فایل Extension**: 5 (+ 1 موجود)
- **تعداد فایل Helper**: 8
- **مجموع متدها**: 100+ متد کاربردی
- **پوشش**: String, DateTime, Numeric, Collections, Objects, Enums, Cache, Security, Files, Images, HTML, URLs

---

## 🚀 نحوه استفاده

### 1. اضافه کردن Using:
```csharp
using ClinicApp.Extensions;
using ClinicApp.Helpers;
```

### 2. استفاده مستقیم:
```csharp
// Extensions
var short = longText.Truncate(100);
var age = birthDate.CalculateAge();

// Helpers
var cached = CacheHelper.GetOrCreate("key", () => GetData());
var encrypted = SecurityHelper.Encrypt(password, key);
```

---

## 📝 نکات مهم

1. تمام Extension methods بدون نیاز به instance جدید قابل استفاده هستند
2. Helper methods static هستند
3. از آنها در Controller، Service، View استفاده کنید
4. مناسب برای ASP.NET MVC 5 و .NET Framework 4.8

---

**تاریخ ایجاد:** ۱۴۰۳/۱۰/۰۵  
**نسخه:** 1.0  
**وضعیت:** آماده استفاده ✅
