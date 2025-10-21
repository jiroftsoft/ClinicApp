# سیستم پیشرفته بیمه - Advanced Insurance System

## نمای کلی

سیستم پیشرفته بیمه یک راه‌حل مدرن و بهینه‌شده برای مدیریت اطلاعات بیمه در سیستم پذیرش بیماران است. این سیستم از جدیدترین تکنولوژی‌های JavaScript استفاده می‌کند و مشکلات موجود در سیستم قدیمی را برطرف می‌کند.

## ویژگی‌های کلیدی

### 1. معماری مدرن
- **Event Delegation Pattern**: مدیریت کارآمد رویدادها
- **Observer Pattern**: ارتباط غیرمستقیم بین ماژول‌ها
- **State Machine Pattern**: مدیریت حالت‌های سیستم
- **Proxy-based Change Detection**: تشخیص تغییرات با Proxy
- **Command Pattern**: اجرای دستورات
- **Strategy Pattern**: اجرای استراتژی‌های مختلف

### 2. ماژول‌های پیشرفته

#### Advanced Change Detector
- تشخیص تغییرات با استفاده از Proxy
- MutationObserver برای تغییرات DOM
- WeakMap برای بهینه‌سازی حافظه
- Debouncing با AbortController

#### Advanced State Manager
- ماشین حالت پیشرفته
- مدیریت انتقال حالت‌ها
- Observer Pattern برای تغییرات حالت
- Command Pattern برای اجرای عملیات

#### Advanced Insurance Coordinator
- هماهنگی بین ماژول‌ها
- مدیریت رویدادهای پیچیده
- Event Delegation
- Performance Monitoring

#### Advanced Insurance System
- هماهنگ‌کننده اصلی سیستم
- مدیریت خطاهای بحرانی
- Performance Monitoring
- Event Coordination

## نصب و راه‌اندازی

### 1. فایل‌های مورد نیاز

```javascript
// ماژول‌های اصلی
~/Scripts/reception/modules/advanced-change-detector.js
~/Scripts/reception/modules/advanced-state-manager.js
~/Scripts/reception/modules/advanced-insurance-coordinator.js
~/Scripts/reception/modules/advanced-insurance-system.js

// فایل تست
~/Scripts/reception/test-advanced-insurance-system.js
```

### 2. تنظیم BundleConfig.cs

```csharp
// Advanced Insurance System Bundle
bundles.Add(new ScriptBundle("~/bundles/advanced-insurance").Include(
    "~/Scripts/reception/modules/advanced-change-detector.js",
    "~/Scripts/reception/modules/advanced-state-manager.js",
    "~/Scripts/reception/modules/advanced-insurance-coordinator.js",
    "~/Scripts/reception/modules/advanced-insurance-system.js"));

// Advanced Insurance System Test Bundle
bundles.Add(new ScriptBundle("~/bundles/advanced-insurance-test").Include(
    "~/Scripts/reception/test-advanced-insurance-system.js"));
```

### 3. تنظیم View

```html
@section Scripts {
    @Scripts.Render("~/bundles/advanced-insurance")
    @Scripts.Render("~/bundles/advanced-insurance-test")
    <!-- سایر اسکریپت‌ها -->
}
```

## استفاده

### 1. راه‌اندازی خودکار

سیستم به صورت خودکار راه‌اندازی می‌شود:

```javascript
$(document).ready(function() {
    // سیستم به صورت خودکار راه‌اندازی می‌شود
    console.log('Advanced Insurance System initialized');
});
```

### 2. استفاده دستی

```javascript
// دسترسی به ماژول‌ها
const changeDetector = window.AdvancedChangeDetector;
const stateManager = window.AdvancedStateManager;
const coordinator = window.AdvancedInsuranceCoordinator;
const system = window.AdvancedInsuranceSystem;

// تست سیستم
const testSystem = window.AdvancedInsuranceSystemTest;
testSystem.runAllTests();
```

## API Reference

### Advanced Change Detector

#### Methods

```javascript
// ثبت مقادیر اولیه
changeDetector.captureInitialValues();

// به‌روزرسانی مقادیر اولیه
changeDetector.updateOriginalValues();

// تشخیص تغییرات
const hasChanges = changeDetector.detectChanges();

// دریافت تغییرات
const changes = changeDetector.getChanges();

// مشاهده تغییرات
changeDetector.observeChanges(callback);

// بازنشانی تغییرات
changeDetector.resetChanges();
```

### Advanced State Manager

#### Methods

```javascript
// انتقال به حالت جدید
stateManager.transitionTo('EDITING');

// بررسی امکان انتقال
const canTransition = stateManager.canTransitionTo('SAVING');

// اجرای دستور
stateManager.executeCommand('enableEditMode');

// اجرای استراتژی
const result = stateManager.executeStrategy('validateForm', data);

// دریافت وضعیت
const status = stateManager.getStatus();
```

### Advanced Insurance Coordinator

#### Methods

```javascript
// مدیریت تغییر فرم
coordinator.handleFormChange(event);

// مدیریت کلیک ذخیره
coordinator.handleSaveClick(event);

// مدیریت جستجوی بیمار
coordinator.handlePatientSearchSuccess(data);
```

### Advanced Insurance System

#### Methods

```javascript
// دریافت ماژول
const module = system.getModule('changeDetector');

// دریافت گزارش عملکرد
const report = system.getPerformanceReport();

// دریافت وضعیت
const status = system.getStatus();
```

## تست و اعتبارسنجی

### 1. تست خودکار

سیستم به صورت خودکار تست می‌شود:

```javascript
// تست‌های خودکار
- Module Availability Test
- Change Detection Test
- State Management Test
- Form Interaction Test
- Performance Test
```

### 2. تست دستی

```javascript
// اجرای تست‌ها
const testSystem = window.AdvancedInsuranceSystemTest;
testSystem.runAllTests();

// دریافت نتایج
const results = testSystem.getTestResults();
```

## Performance Monitoring

### 1. نظارت بر حافظه

```javascript
// دریافت گزارش عملکرد
const report = system.getPerformanceReport();

console.log('Uptime:', report.uptime);
console.log('Event Count:', report.eventCount);
console.log('Error Count:', report.errorCount);
console.log('Memory Usage:', report.memoryUsage);
```

### 2. نظارت بر رویدادها

```javascript
// ثبت رویداد
performanceMonitor.recordEvent();

// ثبت خطا
performanceMonitor.recordError();

// ثبت استفاده از حافظه
performanceMonitor.recordMemoryUsage();
```

## مدیریت خطا

### 1. خطاهای عادی

```javascript
// مدیریت خطاهای عادی
system.handleError(error);
```

### 2. خطاهای بحرانی

```javascript
// مدیریت خطاهای بحرانی
system.handleCriticalError(error);
```

## بهینه‌سازی

### 1. Event Delegation

```javascript
// استفاده از Event Delegation
$(document).on('change.advancedInsurance', '#insuranceProvider', handler);
```

### 2. Debouncing

```javascript
// استفاده از Debouncing
const debouncedHandler = createDebouncedHandler(handler, 300);
```

### 3. WeakMap

```javascript
// استفاده از WeakMap برای بهینه‌سازی حافظه
const observers = new WeakMap();
```

## مقایسه با سیستم قدیمی

| ویژگی | سیستم قدیمی | سیستم پیشرفته |
|--------|-------------|----------------|
| Event Handling | Multiple Listeners | Event Delegation |
| Change Detection | Manual Comparison | Proxy-based |
| State Management | Manual Flags | State Machine |
| Memory Usage | High | Optimized |
| Performance | Slow | Fast |
| Error Handling | Basic | Advanced |
| Testing | Manual | Automated |

## نکات مهم

### 1. سازگاری

سیستم جدید با سیستم قدیمی سازگار است و می‌تواند به صورت موازی اجرا شود.

### 2. Migration

برای مهاجرت کامل به سیستم جدید:

1. تست سیستم جدید
2. غیرفعال کردن سیستم قدیمی
3. فعال کردن سیستم جدید
4. نظارت بر عملکرد

### 3. Troubleshooting

در صورت بروز مشکل:

1. بررسی Console Logs
2. اجرای تست‌های خودکار
3. بررسی Performance Report
4. تماس با تیم توسعه

## پشتیبانی

برای پشتیبانی و گزارش مشکلات:

- **Email**: support@clinicapp.com
- **Documentation**: [Advanced Insurance System Docs](./AdvancedInsuranceSystem.md)
- **Issues**: [GitHub Issues](https://github.com/clinicapp/issues)

## نسخه‌ها

- **v3.0.0**: نسخه اولیه سیستم پیشرفته
- **v2.x.x**: سیستم قدیمی (پشتیبانی می‌شود)
- **v1.x.x**: سیستم اولیه (منسوخ شده)

## مجوز

این سیستم تحت مجوز MIT منتشر شده است.
