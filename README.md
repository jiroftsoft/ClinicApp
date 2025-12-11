# 🏥 **ClinicApp - سیستم مدیریت کلینیک**

## 🎯 **درباره پروژه**
ClinicApp یک سیستم جامع مدیریت کلینیک است که با ASP.NET MVC 5 و Entity Framework 6 توسعه یافته است. این سیستم برای مدیریت کامل عملیات کلینیک‌های پزشکی طراحی شده است.

---

## 📋 **قراردادهای پروژه**

### **🚨 مهم: قبل از هر تغییر، قراردادها را مطالعه کنید**

تمام قراردادهای الزام‌آور پروژه در پوشه‌های `CONTRACTS/` و `Docs/` قرار دارند:

#### **قراردادهای اصلی:**
- **[قرارداد توسعه و استانداردهای کدنویسی](Docs/DEVELOPMENT_CONTRACT.md)** ⭐ **الزامی برای تمام توسعه‌ها**
- **[Template TODO List](Docs/TODO_TEMPLATE.md)** - الگوی TODO List برای تمام ماژول‌ها

#### **قراردادهای موجود:**
- **[فهرست کامل قراردادها](CONTRACTS/README.md)** - راهنمای کامل تمام قراردادها
- **[قرارداد تبعیت هوش مصنوعی](CONTRACTS/AI_COMPLIANCE_CONTRACT.md)** - 15 بند الزام‌آور
- **[چک‌لیست پیش‌پرواز](CONTRACTS/PREFLIGHT_CHECKLIST_CONTRACT.md)** - بررسی قبل از هر تغییر
- **[روند اجرایی اتمیک](CONTRACTS/ATOMIC_EXECUTION_WORKFLOW_CONTRACT.md)** - 9 گام اجرای تغییرات

---

## 🏗️ **معماری پروژه**

### **لایه‌های اصلی:**
- **Controllers**: مدیریت درخواست‌ها و پاسخ‌ها
- **Services**: منطق کسب‌وکار
- **Repositories**: دسترسی به داده‌ها
- **ViewModels**: مدل‌های نمایش
- **Entities**: مدل‌های دیتابیس

### **اصول طراحی:**
- **SOLID Principles**: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **Clean Architecture**: جداسازی نگرانی‌ها
- **Factory Method Pattern**: تبدیل Entity به ViewModel
- **ServiceResult Enhanced**: مدیریت پاسخ‌ها و خطاها

---

## 🛠️ **تکنولوژی‌های استفاده شده**

- **Framework**: ASP.NET MVC 5
- **ORM**: Entity Framework 6
- **Database**: SQL Server
- **Frontend**: Bootstrap, jQuery, Persian DatePicker
- **Validation**: FluentValidation
- **Logging**: Serilog
- **Authentication**: ASP.NET Identity

---

## 📁 **ساختار پروژه**

```
ClinicApp/
├── Areas/
│   └── Admin/                 # ناحیه مدیریت
│       ├── Controllers/       # کنترلرهای مدیریت
│       └── Views/            # ویوهای مدیریت
├── CONTRACTS/                # قراردادهای پروژه
├── Content/                  # فایل‌های استاتیک
├── Controllers/              # کنترلرهای اصلی
├── Models/                   # مدل‌ها و Entity ها
├── Services/                 # سرویس‌های کسب‌وکار
├── Repositories/             # مخازن داده
├── ViewModels/               # مدل‌های نمایش
├── Helpers/                  # کلاس‌های کمکی
├── Extensions/               # متدهای توسعه
└── TEMPLATES/                # قالب‌های استاندارد
```

---

## 🚀 **راه‌اندازی پروژه**

### **پیش‌نیازها:**
- Visual Studio 2019 یا بالاتر
- SQL Server 2016 یا بالاتر
- .NET Framework 4.8

### **مراحل راه‌اندازی:**
1. کلون کردن پروژه
2. باز کردن در Visual Studio
3. تنظیم connection string در `web.config`
4. اجرای Migration ها
5. اجرای پروژه

---

## 📖 **راهنمای توسعه**

### **قبل از شروع:**
1. **[قراردادهای پروژه](CONTRACTS/README.md)** را مطالعه کنید
2. **[چک‌لیست پیش‌پرواز](CONTRACTS/PREFLIGHT_CHECKLIST_CONTRACT.md)** را اجرا کنید
3. **[روند اجرایی اتمیک](CONTRACTS/ATOMIC_EXECUTION_WORKFLOW_CONTRACT.md)** را دنبال کنید

### **استانداردهای کدنویسی:**
- از **Factory Method Pattern** برای تبدیل Entity به ViewModel استفاده کنید
- از **ServiceResult Enhanced** برای مدیریت پاسخ‌ها استفاده کنید
- از **FluentValidation** برای اعتبارسنجی استفاده کنید
- از **Serilog** برای لاگ‌گیری استفاده کنید

### **استانداردهای UI/UX:**
- از **[استانداردهای طراحی](CONTRACTS/DESIGN_PRINCIPLES_CONTRACT.md)** پیروی کنید
- از **[استانداردهای فرم‌ها](CONTRACTS/FormStandards.md)** استفاده کنید
- از **[استانداردهای نمایش جزئیات](CONTRACTS/DETAILS_DISPLAY_STANDARDS.md)** پیروی کنید

### **استانداردهای تقویم شمسی:**
- از **[راهنمای تقویم شمسی](Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md)** برای استفاده از Persian DatePicker پیروی کنید
- تمام فیلدهای تاریخ باید از `_PersianDatePicker` partial view استفاده کنند
- تمام Controller ها باید از `ParseDateFromHiddenInput` برای parse کردن تاریخ استفاده کنند
- تمام نمایش تاریخ‌ها باید از `PersianDateHelper.ToPersianDate` استفاده کنند

### **استانداردهای آپلود تصویر:**
- از **[راهنمای سیستم آپلود تصویر](Docs/IMAGE_UPLOAD_SYSTEM_GUIDE.md)** برای استفاده از Image Upload System پیروی کنید
- تمام آپلودهای تصویر باید از `IImageUploadService` استفاده کنند
- تمام Controller ها باید متد `ProcessImageUpload` را پیاده‌سازی کنند
- تمام فرم‌ها باید `enctype="multipart/form-data"` داشته باشند
- تمام File Input ها باید Preview و Validation JavaScript داشته باشند

---

## 🧪 **تست‌ها**

### **انواع تست‌ها:**
- **Unit Tests**: تست منطق کسب‌وکار
- **Integration Tests**: تست تعامل با دیتابیس
- **Smoke Tests**: تست عملکرد کلی

### **اجرای تست‌ها:**
```bash
# اجرای تمام تست‌ها
dotnet test

# اجرای Unit Tests
dotnet test --filter "Category=Unit"

# اجرای Integration Tests
dotnet test --filter "Category=Integration"
```

---

## 📝 **مستندات**

### **مستندات موجود:**
- **[تحلیل جامع پروژه](PROJECT_COMPREHENSIVE_ANALYSIS.md)**
- **[قراردادهای پروژه](CONTRACTS/)**
- **[قالب‌های استاندارد](TEMPLATES/)**
- **[راهنمای تقویم شمسی](Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md)** - راهنمای کامل استفاده از Persian DatePicker
- **[راهنمای سیستم پیام‌ها](Docs/NOTIFICATION_SYSTEM_GUIDE.md)** - راهنمای استفاده از Toastr و SweetAlert
- **[راهنمای سیستم آپلود تصویر](Docs/IMAGE_UPLOAD_SYSTEM_GUIDE.md)** - راهنمای کامل سیستم آپلود تصویر

### **مستندسازی:**
- تمام کلاس‌ها و متدها باید مستندسازی شوند
- از XML Documentation استفاده کنید
- مثال‌های کاربردی ارائه دهید

---

## 🔒 **امنیت**

### **اقدامات امنیتی:**
- **Anti-Forgery Token**: حفاظت از CSRF
- **Authorization**: کنترل دسترسی
- **Validation**: اعتبارسنجی ورودی‌ها
- **Logging**: ردیابی فعالیت‌ها

### **داده‌های حساس:**
- اطلاعات پزشکی محرمانه
- اطلاعات شخصی بیماران
- اطلاعات مالی

---

## 🤝 **مشارکت**

### **راه‌های مشارکت:**
1. گزارش باگ‌ها
2. پیشنهاد بهبودها
3. مشارکت در کد
4. بهبود مستندات

### **فرآیند مشارکت:**
1. مطالعه قراردادها
2. اجرای چک‌لیست پیش‌پرواز
3. دنبال کردن روند اتمیک
4. ارائه تست‌ها
5. درخواست تایید

---

## 📞 **پشتیبانی**

### **در صورت مشکل:**
- قراردادها را مطالعه کنید
- از چک‌لیست پیش‌پرواز استفاده کنید
- با تیم توسعه تماس بگیرید

### **گزارش مشکلات:**
- مشکلات فنی
- نقض قراردادها
- پیشنهادات بهبود

---

## 📄 **مجوز**

این پروژه تحت مجوز [MIT License](LICENSE) منتشر شده است.

---

## 🙏 **تشکر**

از تمامی مشارکت‌کنندگان و توسعه‌دهندگان که در بهبود این پروژه نقش داشته‌اند، تشکر می‌کنیم.

---

**نسخه**: 1.0  
**تاریخ ایجاد**: 2025-01-04  
**آخرین به‌روزرسانی**: 2025-01-04  
**وضعیت**: فعال
