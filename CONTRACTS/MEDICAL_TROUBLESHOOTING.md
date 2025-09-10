# 🏥 Medical Environment Troubleshooting Guide
# راهنمای عیب‌یابی محیط درمانی

## 🔍 مشکلات رایج و راه‌حل‌ها

### 1. خطای "خطای سیستمی در حذف خدمت رخ داد"

#### علت احتمالی:
- ❌ Anti-Forgery Token نامعتبر یا موجود نیست
- ❌ خطای پایگاه داده
- ❌ خطای اعتبارسنجی

#### راه‌حل:

##### مرحله 1: بررسی Anti-Forgery Token
```javascript
// در Console مرورگر اجرا کنید:
checkAntiForgeryToken();
```

##### مرحله 2: بررسی کامل سیستم
```javascript
// در Console مرورگر اجرا کنید:
runMedicalDebug();
```

##### مرحله 3: تست حذف خدمت
```javascript
// در Console مرورگر اجرا کنید (serviceId را جایگزین کنید):
testServiceDelete(123);
```

##### مرحله 4: بررسی Log ها
- فایل‌های log را در `App_Data/Logs/` بررسی کنید
- دنبال پیام‌های `🏥 MEDICAL` بگردید

### 2. خطای "undefined" در toast notifications

#### علت احتمالی:
- ❌ تابع `showMedicalToast` موجود نیست
- ❌ Bootstrap 5 بارگذاری نشده

#### راه‌حل:

##### مرحله 1: بررسی توابع toast
```javascript
// در Console مرورگر اجرا کنید:
testToastSystem();
```

##### مرحله 2: بررسی Bootstrap
```javascript
// در Console مرورگر اجرا کنید:
console.log('Bootstrap:', typeof bootstrap);
```

### 3. خطای فونت Vazirmatn

#### علت احتمالی:
- ❌ فایل‌های فونت موجود نیستند
- ❌ Cache مرورگر

#### راه‌حل:

##### مرحله 1: پاک کردن Cache
- Chrome: `Ctrl + Shift + R`
- Firefox: `Ctrl + Shift + R`
- Edge: `Ctrl + Shift + R`

##### مرحله 2: بررسی فایل‌های فونت
```
Content/Fonts/vazirmatn/
├── Vazirmatn-Regular.woff2
├── Vazirmatn-Medium.woff2
├── Vazirmatn-SemiBold.woff2
└── Vazirmatn-Bold.woff2
```

### 4. خطای "UpdatedByUserId" خالی

#### علت احتمالی:
- ❌ `_currentUserService.UserId` null است

#### راه‌حل:

##### مرحله 1: بررسی Current User Service
```csharp
// در ServiceManagementService.cs
var userId = _currentUserService?.UserId ?? "System";
```

##### مرحله 2: بررسی Authentication
- اطمینان از login بودن کاربر
- بررسی session

## 🔧 ابزارهای دیباگ

### توابع موجود در Console:

#### 1. بررسی کامل سیستم
```javascript
runMedicalDebug();
```

#### 2. تست سریع
```javascript
quickTest();
```

#### 3. بررسی Anti-Forgery Token
```javascript
checkAntiForgeryToken();
```

#### 4. تست حذف خدمت
```javascript
testServiceDelete(serviceId);
```

#### 5. تست حذف دسته‌بندی
```javascript
testCategoryDelete(categoryId);
```

#### 6. تست سیستم toast
```javascript
testToastSystem();
```

## 📋 چک‌لیست عیب‌یابی

### قبل از گزارش مشکل:

- [ ] Console مرورگر را بررسی کرده‌اید
- [ ] `runMedicalDebug()` را اجرا کرده‌اید
- [ ] Log های سرور را بررسی کرده‌اید
- [ ] Cache مرورگر را پاک کرده‌اید
- [ ] از login بودن کاربر اطمینان دارید

### اطلاعات مورد نیاز برای گزارش:

1. **خطای دقیق:**
   ```
   خطای کامل از Console
   ```

2. **اطلاعات سیستم:**
   ```
   مرورگر: Chrome/Firefox/Edge
   نسخه: XX.XX.XX
   سیستم عامل: Windows/Mac/Linux
   ```

3. **مراحل تولید خطا:**
   ```
   1. به صفحه /Admin/Service رفت
   2. روی دکمه حذف کلیک کرد
   3. در مودال تأیید کلیک کرد
   4. خطا رخ داد
   ```

4. **خروجی runMedicalDebug():**
   ```
   خروجی کامل از Console
   ```

## 🚨 مشکلات بحرانی

### اگر هیچ‌کدام از راه‌حل‌ها کار نکرد:

1. **بررسی Database Connection:**
   ```csharp
   // در Web.config
   <connectionStrings>
     <add name="DefaultConnection" 
          connectionString="..." />
   </connectionStrings>
   ```

2. **بررسی Unity Container:**
   ```csharp
   // در UnityConfig.cs
   container.RegisterType<IServiceManagementService, ServiceManagementService>();
   ```

3. **بررسی Logging Configuration:**
   ```csharp
   // در Web.config
   <serilog>
     <writeTo name="file" />
   </serilog>
   ```

## 📞 پشتیبانی

### در صورت نیاز به کمک بیشتر:

1. تمام اطلاعات بالا را جمع‌آوری کنید
2. Screenshot از Console بگیرید
3. فایل‌های log را ضمیمه کنید
4. مشکل را با جزئیات کامل گزارش دهید

---

**🏥 Medical Environment - 100% Reliability Guaranteed**
