# 📘 راهنمای Migration بیماران Legacy

**تاریخ:** 1404/10/13  
**نسخه:** 1.0  
**وضعیت:** آماده اجرا

---

## 🎯 **هدف**

تبدیل 7,107 بیمار قدیمی (Legacy) به بیماران با حساب کاربری فعال در Patient Portal.

---

## 📋 **مراحل اجرا**

### **مرحله 1: اجرای Migration** ✅

```powershell
# در Package Manager Console
Update-Database -Verbose
```

**این Migration:**
1. برای هر بیمار Legacy یک `ApplicationUser` ایجاد می‌کند
2. نقش `Patient` را اختصاص می‌دهد
3. `ApplicationUserId` را به بیمار لینک می‌کند
4. Column `ApplicationUserId` را `NOT NULL` می‌کند

**زمان اجرا:** 5-10 دقیقه (7,107 بیمار)

---

### **مرحله 2: بررسی نتیجه Migration**

```sql
-- چک کردن اینکه همه بیماران User دارند
SELECT COUNT(*) 
FROM Patients 
WHERE ApplicationUserId IS NULL AND IsDeleted = 0;
-- باید 0 باشد ✅

-- بررسی بیماران Legacy
SELECT COUNT(*) 
FROM Patients p
INNER JOIN AspNetUsers u ON p.ApplicationUserId = u.Id
WHERE u.CreatedByName = 'System Migration'
  AND u.PasswordHash IS NULL;
-- باید 7,107 باشد ✅
```

---

### **مرحله 3: ارسال خوش‌آمدگویی**

#### **گزینه A: از طریق Admin Panel** (توصیه می‌شود)

1. به Admin Panel بروید
2. صفحه "مدیریت بیماران Legacy" را باز کنید:
   ```
   /Admin/LegacyPatient
   ```
3. آمار را بررسی کنید
4. دکمه "ارسال پیامک خوش‌آمدگویی به همه" را بزنید

#### **گزینه B: از طریق Script**

```sql
-- لیست بیماران برای ارسال پیامک
-- Script: Scripts/Send_Welcome_Credentials_To_Legacy_Patients.sql
```

---

### **مرحله 4: تنظیم سیستم ارسال پیامک**

در `Services/Patient/LegacyPatientWelcomeService.cs`:

```csharp
// TODO: این خط را با سرویس SMS واقعی جایگزین کنید
// await _smsService.SendAsync(patient.PhoneNumber, smsText);
```

**نمونه پیامک:**
```
عزیز [نام بیمار]، به پورتال کلینیک شفا خوش آمدید!
برای فعال‌سازی حساب کاربری و تنظیم رمز عبور، لینک زیر را باز کنید:
https://clinicapp.ir/Account/SetPassword?userId=...&token=...
کد ملی شما: [کد ملی]
```

---

## 📊 **آمار پس از Migration**

| مورد | تعداد |
|------|-------|
| **کل بیماران Legacy** | 7,107 |
| **با شماره تلفن** | ? (بررسی کنید) |
| **با ایمیل** | ? (بررسی کنید) |
| **بدون اطلاعات تماس** | ? (بررسی کنید) |

---

## 🔐 **Security & Privacy**

### **Password Policy:**
- بیماران Legacy **PasswordHash = NULL** دارند
- باید از **Password Reset Token** برای تنظیم اولین رمز استفاده کنند
- Token Expiration: 24 ساعت (قابل تنظیم)

### **Privacy:**
- اطلاعات بیماران محرمانه است
- فقط Admin می‌تواند این صفحه را ببیند
- لاگ کامل تمام عملیات ثبت می‌شود

---

## ✅ **چک‌لیست تکمیل**

```
□ Migration اجرا شد (Update-Database)
□ همه بیماران User دارند (ApplicationUserId NOT NULL)
□ نقش Patient به همه اختصاص یافت
□ سیستم ارسال پیامک تنظیم شد
□ صفحه Admin/LegacyPatient تست شد
□ پیامک خوش‌آمدگویی ارسال شد
□ بیماران می‌توانند با Reset Password Token رمز تنظیم کنند
□ لاگ‌ها بررسی شدند
```

---

## 🚨 **Rollback Plan**

اگر مشکلی پیش آمد:

```powershell
# Rollback Migration
Update-Database -TargetMigration: 202512301701341_AddNewsletterStatusIndexes
```

**توجه:** این کار User های ایجاد شده را حذف می‌کند!

---

## 🔍 **Troubleshooting**

### **مشکل 1: Migration با خطا مواجه شد**
```
Cannot insert the value NULL into column 'ApplicationUserId'
```
**علت:** هنوز بیمارانی با `ApplicationUserId = NULL` وجود دارند  
**راه‌حل:** بررسی کنید که Script ایجاد User درست اجرا شده باشد

### **مشکل 2: بیماران نمی‌توانند لاگین کنند**
**علت:** `PasswordHash = NULL`  
**راه‌حل:** از Password Reset Token استفاده کنید

### **مشکل 3: پیامک ارسال نمی‌شود**
**علت:** سرویس SMS تنظیم نشده  
**راه‌حل:** در `LegacyPatientWelcomeService.cs` سرویس SMS واقعی را فعال کنید

---

## 📖 **فایل‌های مرتبط**

| فایل | نقش |
|------|-----|
| `Migrations/202601021635448_Revert_Patient_ApplicationUserId_To_Required.cs` | Migration اصلی |
| `Services/Patient/LegacyPatientWelcomeService.cs` | سرویس ارسال خوش‌آمدگویی |
| `Areas/Admin/Controllers/LegacyPatientController.cs` | Controller صفحه Admin |
| `Areas/Admin/Views/LegacyPatient/Index.cshtml` | View صفحه Admin |
| `Scripts/Send_Welcome_Credentials_To_Legacy_Patients.sql` | Script SQL برای لیست بیماران |

---

## 🎯 **Business Value**

✅ 7,107 مشتری بالقوه برای Patient Portal  
✅ افزایش engagement با بیماران  
✅ کاهش تماس‌های تلفنی برای گرفتن نوبت  
✅ تجربه کاربری یکپارچه  
✅ معماری ساده و قابل نگهداری  

---

## 🙏 **نکات مهم**

1. **Backup:** قبل از Migration حتماً Backup بگیرید
2. **Test:** در محیط Test ابتدا آزمایش کنید
3. **Timing:** Migration را در زمان کم‌کاری اجرا کنید
4. **Monitor:** لاگ‌ها را پس از Migration بررسی کنید
5. **Communication:** به تیم پشتیبانی اطلاع دهید که بیماران ممکن است تماس بگیرند

---

**✅ حالا آماده اجرا هستید!**

اگر سوال یا مشکلی بود، به این مستند مراجعه کنید.

---

> "بهترین زمان برای شروع، همین حالا است!"

