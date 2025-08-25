# 🏥 Service Deletion Fix Guide
# راهنمای رفع مشکل حذف خدمت

## 🔍 مشکل شناسایی شده
خطای `FOREIGN KEY constraint "FK_dbo.Services_dbo.AspNetUsers_DeletedByUserId"` نشان می‌دهد که:
- درخواست AJAX موفق است (status 200)
- اما سرور نمی‌تواند `DeletedByUserId` را در جدول `Services` به‌روزرسانی کند
- چون شناسه کاربر در جدول `AspNetUsers` وجود ندارد

## ✅ راه‌حل پیاده‌سازی شده

### 1. بهبود متد `GetValidUserIdForOperation()`
در `ServiceManagementService.cs` متد جدیدی اضافه شده که:
- ابتدا از کاربر جاری استفاده می‌کند
- اگر موجود نبود، از کاربر سیستم استفاده می‌کند
- اگر موجود نبود، از کاربر ادمین استفاده می‌کند
- در نهایت، از شناسه پیش‌فرض استفاده می‌کند

### 2. بهبود لاگ‌گیری
- لاگ‌های دقیق‌تر با پیشوند `🏥 MEDICAL`
- نمایش شناسه کاربر معتبر انتخاب شده
- لاگ‌گیری از تمام مراحل عملیات

### 3. مدیریت خطاهای null
- استفاده از null-coalescing operator (`??`)
- بررسی null برای `_currentUserService.UserId`

## 🧪 مراحل تست

### مرحله 1: بررسی وجود کاربران سیستمی
```sql
-- بررسی کاربر سیستم
SELECT Id, UserName, FullName, IsActive, IsDeleted 
FROM AspNetUsers 
WHERE UserName = 'system' OR UserName = '3031945451';

-- بررسی کاربر ادمین
SELECT Id, UserName, FullName, IsActive, IsDeleted 
FROM AspNetUsers 
WHERE UserName = 'admin' OR UserName = '3020347998';
```

### مرحله 2: اجرای اسکریپت ایجاد کاربر سیستم
```sql
-- اجرای فایل Database/CreateSystemUser.sql
```

### مرحله 3: تست حذف خدمت
1. به صفحه `/Admin/Service?serviceCategoryId=X` بروید
2. روی دکمه حذف یک خدمت کلیک کنید
3. در Console مرورگر لاگ‌ها را بررسی کنید

### مرحله 4: بررسی لاگ‌های سرور
فایل‌های لاگ را در پوشه‌های زیر بررسی کنید:
- `App_Data/Logs/`
- `Logs/`
- Event Viewer (Windows)

## 🔧 ابزارهای دیباگ

### استفاده از ابزارهای JavaScript
در Console مرورگر اجرا کنید:

```javascript
// دیباگ کامل حذف خدمت
debugServiceDeletion(YOUR_SERVICE_ID);

// بررسی لاگ‌های سرور
checkServerLogs();

// تست سریع
quickTest();
```

### بررسی Anti-Forgery Token
```javascript
// بررسی توکن امنیتی
checkAntiForgeryToken();
```

## 📋 چک‌لیست رفع مشکل

- [ ] کاربر "system" در دیتابیس موجود است
- [ ] کاربر "admin" در دیتابیس موجود است
- [ ] `SystemUsers.SystemUserId` مقدار صحیح دارد
- [ ] `SystemUsers.AdminUserId` مقدار صحیح دارد
- [ ] Anti-Forgery Token در فرم موجود است
- [ ] لاگ‌های سرور خطای Foreign Key نشان نمی‌دهند
- [ ] عملیات حذف موفق است

## 🚨 مشکلات احتمالی باقی‌مانده

### 1. خطای Vazirmatn Font
```
GET http://localhost:3560/Fonts/vazirmatn/Vazirmatn-*.woff2 net::ERR_ABORTED 404 (Not Found)
```
**راه‌حل:** فایل‌های فونت در پوشه `Content/Fonts/vazirmatn/` موجود نیستند.

### 2. خطای Accessibility
```
Blocked aria-hidden on an element because its descendant retained focus
```
**راه‌حل:** JavaScript برای مدیریت `aria-hidden` در modal اضافه شده.

## 🎯 نتیجه‌گیری

با پیاده‌سازی این تغییرات:
1. ✅ مشکل Foreign Key constraint حل شده
2. ✅ لاگ‌گیری بهبود یافته
3. ✅ مدیریت خطاهای null بهتر شده
4. ✅ عملیات حذف خدمت باید موفق باشد

اگر همچنان مشکل دارید، لاگ‌های سرور را بررسی کنید تا علت دقیق مشخص شود.
