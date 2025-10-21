# ماژول‌های پیشرفته سیستم پذیرش - Advanced Reception Modules

## نمای کلی

این پوشه شامل ماژول‌های پیشرفته سیستم پذیرش بیماران است که از جدیدترین تکنولوژی‌های JavaScript استفاده می‌کند.

## ساختار فایل‌ها

```
Scripts/reception/modules/
├── advanced-change-detector.js          # تشخیص‌دهنده پیشرفته تغییرات
├── advanced-state-manager.js            # مدیریت‌کننده پیشرفته حالت
├── advanced-insurance-coordinator.js    # هماهنگ‌کننده پیشرفته بیمه
├── advanced-insurance-system.js         # سیستم پیشرفته بیمه
├── test-advanced-insurance-system.js    # تست سیستم پیشرفته
└── README.md                            # این فایل
```

## ماژول‌های اصلی

### 1. Advanced Change Detector

**فایل**: `advanced-change-detector.js`

**توضیحات**: تشخیص‌دهنده پیشرفته تغییرات با استفاده از Proxy و MutationObserver

**ویژگی‌ها**:
- Proxy-based Change Detection
- MutationObserver برای DOM changes
- WeakMap برای بهینه‌سازی حافظه
- Event Delegation Pattern
- Debouncing با AbortController

**استفاده**:
```javascript
const detector = window.AdvancedChangeDetector;
detector.captureInitialValues();
detector.observeChanges(callback);
```

### 2. Advanced State Manager

**فایل**: `advanced-state-manager.js`

**توضیحات**: مدیریت‌کننده پیشرفته حالت با State Machine Pattern

**ویژگی‌ها**:
- State Machine Pattern
- Observer Pattern
- Command Pattern
- Strategy Pattern
- Event Sourcing

**استفاده**:
```javascript
const stateManager = window.AdvancedStateManager;
stateManager.transitionTo('EDITING');
stateManager.executeCommand('enableEditMode');
```

### 3. Advanced Insurance Coordinator

**فایل**: `advanced-insurance-coordinator.js`

**توضیحات**: هماهنگ‌کننده پیشرفته بیمه با Event Delegation

**ویژگی‌ها**:
- Event Delegation Pattern
- Modern Event Handling
- Performance Monitoring
- Error Handling
- State Management

**استفاده**:
```javascript
const coordinator = window.AdvancedInsuranceCoordinator;
coordinator.handleFormChange(event);
coordinator.handleSaveClick(event);
```

### 4. Advanced Insurance System

**فایل**: `advanced-insurance-system.js`

**توضیحات**: سیستم پیشرفته بیمه - هماهنگ‌کننده اصلی

**ویژگی‌ها**:
- Module Coordination
- Event Coordination
- Performance Monitoring
- Error Handling
- Status Management

**استفاده**:
```javascript
const system = window.AdvancedInsuranceSystem;
const module = system.getModule('changeDetector');
const report = system.getPerformanceReport();
```

## تست و اعتبارسنجی

### فایل تست

**فایل**: `test-advanced-insurance-system.js

**توضیحات**: تست خودکار سیستم پیشرفته بیمه

**تست‌ها**:
- Module Availability Test
- Change Detection Test
- State Management Test
- Form Interaction Test
- Performance Test

**استفاده**:
```javascript
const testSystem = window.AdvancedInsuranceSystemTest;
testSystem.runAllTests();
```

## نصب و راه‌اندازی

### 1. تنظیم BundleConfig.cs

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

### 2. تنظیم View

```html
@section Scripts {
    @Scripts.Render("~/bundles/advanced-insurance")
    @Scripts.Render("~/bundles/advanced-insurance-test")
    <!-- سایر اسکریپت‌ها -->
}
```

## API Reference

### Advanced Change Detector

#### Methods

```javascript
// ثبت مقادیر اولیه
captureInitialValues()

// به‌روزرسانی مقادیر اولیه
updateOriginalValues()

// تشخیص تغییرات
detectChanges()

// دریافت تغییرات
getChanges()

// مشاهده تغییرات
observeChanges(callback)

// بازنشانی تغییرات
resetChanges()

// دریافت وضعیت
getStatus()
```

### Advanced State Manager

#### Methods

```javascript
// انتقال به حالت جدید
transitionTo(newState)

// بررسی امکان انتقال
canTransitionTo(newState)

// اجرای دستور
executeCommand(commandName, data)

// اجرای استراتژی
executeStrategy(strategyName, data)

// دریافت حالت فعلی
getCurrentState()

// دریافت وضعیت
getStatus()
```

### Advanced Insurance Coordinator

#### Methods

```javascript
// مدیریت تغییر فرم
handleFormChange(event)

// مدیریت کلیک ذخیره
handleSaveClick(event)

// مدیریت جستجوی بیمار
handlePatientSearchSuccess(data)

// بارگذاری اطلاعات بیمه
loadPatientInsurance()

// ذخیره اطلاعات بیمه
performSave()
```

### Advanced Insurance System

#### Methods

```javascript
// دریافت ماژول
getModule(name)

// دریافت گزارش عملکرد
getPerformanceReport()

// دریافت وضعیت
getStatus()

// پاکسازی
cleanup()
```

## Performance Monitoring

### نظارت بر عملکرد

```javascript
// دریافت گزارش عملکرد
const report = system.getPerformanceReport();

console.log('Uptime:', report.uptime);
console.log('Event Count:', report.eventCount);
console.log('Error Count:', report.errorCount);
console.log('Memory Usage:', report.memoryUsage);
console.log('Average Memory Usage:', report.averageMemoryUsage);
```

### نظارت بر حافظه

```javascript
// ثبت استفاده از حافظه
performanceMonitor.recordMemoryUsage();

// دریافت میانگین استفاده از حافظه
const average = performanceMonitor.calculateAverageMemoryUsage();
```

## مدیریت خطا

### خطاهای عادی

```javascript
// مدیریت خطاهای عادی
system.handleError(error);
```

### خطاهای بحرانی

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

### 4. AbortController

```javascript
// استفاده از AbortController برای لغو عملیات
const abortController = new AbortController();
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
| Maintainability | Low | High |
| Scalability | Limited | High |

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
- **Documentation**: [Advanced Insurance System Docs](../Documentation/AdvancedInsuranceSystem.md)
- **Issues**: [GitHub Issues](https://github.com/clinicapp/issues)

## نسخه‌ها

- **v3.0.0**: نسخه اولیه سیستم پیشرفته
- **v2.x.x**: سیستم قدیمی (پشتیبانی می‌شود)
- **v1.x.x**: سیستم اولیه (منسوخ شده)

## مجوز

این سیستم تحت مجوز MIT منتشر شده است.

## مشارکت

برای مشارکت در توسعه:

1. Fork کردن پروژه
2. ایجاد Branch جدید
3. اعمال تغییرات
4. ارسال Pull Request

## تغییرات

### v3.0.0 (2025-01-20)

- اضافه شدن Advanced Change Detector
- اضافه شدن Advanced State Manager
- اضافه شدن Advanced Insurance Coordinator
- اضافه شدن Advanced Insurance System
- اضافه شدن سیستم تست خودکار
- بهبود Performance Monitoring
- بهبود Error Handling
- بهبود Memory Management
