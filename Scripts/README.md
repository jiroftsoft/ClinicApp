# 🏥 MEDICAL: Modular JavaScript Architecture

## 📋 Overview

این پروژه از معماری modular JavaScript برای مدیریت تعرفه‌های بیمه استفاده می‌کند. این ساختار باعث بهبود maintainability، reusability و performance می‌شود.

## 🏗️ Architecture

### 📁 File Structure
```
Scripts/
├── medical-tariff-utils.js      # توابع کمکی و utility functions
├── medical-tariff-statistics.js # ماژول آمار و real-time updates
├── medical-tariff-filters.js    # ماژول فیلتر و جستجو
├── medical-tariff-table.js      # ماژول جدول و تعاملات
├── medical-tariff-main.js       # ماژول اصلی و هماهنگ‌کننده
└── README.md                   # این فایل
```

## 🔧 Modules

### 1. **Utils Module** (`medical-tariff-utils.js`)
- **هدف**: توابع کمکی و utility functions
- **ویژگی‌ها**:
  - Cache Management
  - AJAX Configuration
  - UI Helpers
  - Error Handling
  - Data Formatters

```javascript
// مثال استفاده
MedicalTariffUtils.CacheManager.clearAllCaches();
MedicalTariffUtils.UIHelpers.showLoading($element, 'در حال بارگیری...');
```

### 2. **Statistics Module** (`medical-tariff-statistics.js`)
- **هدف**: مدیریت آمار و بروزرسانی real-time
- **ویژگی‌ها**:
  - Auto-refresh statistics
  - Manual refresh
  - Timer management
  - Animation effects

```javascript
// مثال استفاده
MedicalTariffStatistics.init();
MedicalTariffStatistics.refresh();
```

### 3. **Filters Module** (`medical-tariff-filters.js`)
- **هدف**: مدیریت فیلترها و جستجو
- **ویژگی‌ها**:
  - Insurance providers loading
  - Services filtering
  - Form validation
  - AJAX filtering

```javascript
// مثال استفاده
MedicalTariffFilters.loadInsuranceProviders();
MedicalTariffFilters.applyFilters();
```

### 4. **Table Module** (`medical-tariff-table.js`)
- **هدف**: مدیریت جدول و تعاملات
- **ویژگی‌ها**:
  - Table sorting
  - Pagination
  - Row actions
  - Empty states

```javascript
// مثال استفاده
MedicalTariffTable.init();
MedicalTariffTable.loadPage(2);
```

### 5. **Main Module** (`medical-tariff-main.js`)
- **هدف**: هماهنگ‌کننده تمام ماژول‌ها
- **ویژگی‌ها**:
  - Module initialization
  - Global event handling
  - Error management
  - Resource cleanup

```javascript
// مثال استفاده
MedicalTariffMain.init();
MedicalTariffMain.refreshAll();
```

## 🚀 Usage

### Basic Initialization
```javascript
// در View
$(document).ready(function() {
    if (typeof window.MedicalTariffMain !== 'undefined') {
        window.MedicalTariffMain.init();
    }
});
```

### Configuration
```javascript
// تنظیمات ماژول‌ها
window.medicalTariffConfig = {
    statisticsUrl: '/Admin/Insurance/InsuranceTariff/GetStatistics',
    providersUrl: '/Admin/Insurance/InsuranceTariff/GetInsuranceProviders',
    // ... سایر URL ها
};
```

## 🔄 Module Communication

### Event-Based Communication
```javascript
// ارسال event
$(document).trigger('medical:statistics:refresh');

// دریافت event
$(document).on('medical:statistics:refresh', function() {
    // Handle refresh
});
```

### Direct Method Calls
```javascript
// فراخوانی مستقیم متدها
MedicalTariffStatistics.refresh();
MedicalTariffFilters.applyFilters();
```

## 🛡️ Error Handling

### Module-Level Error Handling
```javascript
try {
    // Module operations
} catch (error) {
    MedicalTariffUtils.ErrorHandler.handleGeneralError(error, 'Module Name');
}
```

### Global Error Handling
```javascript
// در Main Module
handleInitializationError: function(error) {
    console.error('🏥 MEDICAL: Initialization error:', error);
    // Show user-friendly error message
}
```

## 📱 Responsive Design

### Mobile Adaptations
```javascript
// در Main Module
enableMobileMode: function() {
    $('.medical-table').addClass('table-responsive');
    $('.medical-actions').addClass('flex-column');
}
```

## 🎯 Performance Optimizations

### Lazy Loading
```javascript
// بارگیری ماژول‌ها فقط در صورت نیاز
if (needsStatistics) {
    MedicalTariffStatistics.init();
}
```

### Memory Management
```javascript
// پاک‌سازی منابع
cleanup: function() {
    if (this.modules.statistics) {
        this.modules.statistics.destroy();
    }
}
```

## 🔧 Development Guidelines

### 1. **Module Structure**
```javascript
window.ModuleName = (function(dependencies) {
    'use strict';
    
    // Private variables
    const privateVar = {};
    
    // Private methods
    const privateMethod = function() {};
    
    // Public API
    return {
        init: function() {},
        destroy: function() {}
    };
})(dependencies);
```

### 2. **Error Handling**
```javascript
// همیشه از try-catch استفاده کنید
try {
    // Risky operations
} catch (error) {
    Utils.ErrorHandler.handleGeneralError(error, 'Context');
}
```

### 3. **Event Binding**
```javascript
// استفاده از event delegation
$(document).on('click', '.medical-btn', function() {
    // Handle click
});
```

## 🧪 Testing

### Unit Testing
```javascript
// تست ماژول‌ها
describe('MedicalTariffStatistics', function() {
    it('should initialize correctly', function() {
        MedicalTariffStatistics.init();
        expect(MedicalTariffStatistics.isActive()).toBe(true);
    });
});
```

### Integration Testing
```javascript
// تست تعامل ماژول‌ها
describe('Module Integration', function() {
    it('should communicate between modules', function() {
        MedicalTariffMain.init();
        expect(MedicalTariffMain.getStatus().isInitialized).toBe(true);
    });
});
```

## 📚 Best Practices

### 1. **Separation of Concerns**
- هر ماژول مسئولیت مشخصی دارد
- وابستگی‌ها به حداقل رسانده شده
- Interface های واضح تعریف شده

### 2. **Error Handling**
- تمام خطاها catch می‌شوند
- پیام‌های خطای کاربرپسند نمایش داده می‌شوند
- Logging مناسب انجام می‌شود

### 3. **Performance**
- Lazy loading برای ماژول‌ها
- Memory cleanup مناسب
- Event delegation برای performance بهتر

### 4. **Maintainability**
- کد modular و قابل نگهداری
- Documentation مناسب
- Naming conventions واضح

## 🔄 Migration Guide

### از Monolithic به Modular

#### قبل (Monolithic)
```javascript
$(document).ready(function() {
    // 200+ lines of mixed code
    var statsTimer;
    function loadStats() { /* ... */ }
    function loadProviders() { /* ... */ }
    // ... more functions
});
```

#### بعد (Modular)
```javascript
// View
<script src="~/Scripts/medical-tariff-utils.js"></script>
<script src="~/Scripts/medical-tariff-statistics.js"></script>
// ... other modules

$(document).ready(function() {
    MedicalTariffMain.init();
});
```

## 🚀 Future Enhancements

### 1. **TypeScript Support**
```typescript
interface MedicalTariffConfig {
    statisticsUrl: string;
    providersUrl: string;
    // ...
}
```

### 2. **ES6 Modules**
```javascript
import { StatisticsManager } from './medical-tariff-statistics.js';
```

### 3. **Web Components**
```javascript
class MedicalTariffTable extends HTMLElement {
    // Custom element implementation
}
```

## 📞 Support

برای سوالات و مشکلات:
- بررسی console برای خطاها
- بررسی network tab برای AJAX calls
- بررسی module dependencies

---

**نکته**: این معماری برای محیط‌های درمانی بهینه‌سازی شده و از استانداردهای پزشکی ایران پیروی می‌کند.
