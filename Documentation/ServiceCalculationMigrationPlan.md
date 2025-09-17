# برنامه مهاجرت سیستم محاسبات خدمات

## 🎯 هدف
یکپارچه‌سازی سیستم محاسبات خدمات با استفاده از `ServiceComponents` به عنوان روش اصلی

## 📋 مراحل مهاجرت

### مرحله 1: حذف فیلدهای مستقیم از Service
- حذف `TechnicalPart` و `ProfessionalPart` از مدل `Service`
- حذف فیلدهای مربوطه از ViewModels
- حذف فیلدهای مربوطه از Views

### مرحله 2: به‌روزرسانی ServiceCalculationService
- حذف Fallback به فیلدهای مستقیم
- استفاده انحصاری از `ServiceComponents`
- بهبود error handling

### مرحله 3: به‌روزرسانی UI
- حذف فیلدهای `TechnicalPart` و `ProfessionalPart` از فرم‌ها
- اضافه کردن مدیریت `ServiceComponents` در UI
- بهبود نمایش محاسبات

### مرحله 4: Migration دیتابیس
- ایجاد migration برای حذف فیلدهای مستقیم
- انتقال داده‌های موجود به `ServiceComponents`
- اعتبارسنجی داده‌ها

## 🔧 تغییرات مورد نیاز

### 1. مدل Service
```csharp
// حذف این فیلدها:
// public decimal TechnicalPart { get; set; }
// public decimal ProfessionalPart { get; set; }
```

### 2. ServiceCalculationService
```csharp
// حذف Fallback و استفاده انحصاری از ServiceComponents
decimal technicalPart = technicalComponent.Coefficient;
decimal professionalPart = professionalComponent.Coefficient;
```

### 3. ViewModels
```csharp
// حذف فیلدهای مستقیم
// اضافه کردن مدیریت ServiceComponents
public List<ServiceComponentViewModel> Components { get; set; }
```

## ⚠️ نکات مهم

1. **Backup داده‌ها:** قبل از شروع مهاجرت
2. **تست کامل:** در محیط Development
3. **Rollback Plan:** آماده‌سازی برنامه بازگشت
4. **Documentation:** به‌روزرسانی مستندات

## 📊 مزایای نهایی

1. **یکپارچگی:** یک روش محاسبه
2. **انعطاف‌پذیری:** قابلیت توسعه
3. **مدیریت بهتر:** کنترل مستقل اجزا
4. **استاندارد:** مطابقت با best practices
