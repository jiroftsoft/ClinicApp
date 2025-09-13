# **📋 استاندارد جامع فرم‌های پیچیده با POST Actions**

## **🎯 هدف**
ایجاد استاندارد قوی و جامع برای فرم‌های پیچیده که دارای منطق پیچیده، اعتبارسنجی پیشرفته، و تعاملات کاربری پیچیده هستند.

## **📊 فرم‌های هدف**
- ✅ **فرم پذیرش (Reception)**: منطق cascade loading، جستجوی بیمار، انتخاب خدمات
- ✅ **فرم طرح بیمه (InsurancePlan)**: اعتبارسنجی تاریخ شمسی، محاسبات پیچیده
- ✅ **فرم‌های آینده**: هر فرمی که منطق پیچیده دارد

---

## **🏗️ معماری استاندارد**

### **1. 📁 ساختار فایل‌ها**
```
Controllers/
├── [Module]Controller.cs          # Controller اصلی
├── [Module]LookupController.cs    # Controller برای AJAX calls

ViewModels/
├── [Module]/
│   ├── [Module]CreateEditViewModel.cs
│   ├── [Module]LookupViewModels.cs
│   └── [Module]ValidationAttributes.cs

Views/
├── [Module]/
│   ├── Create.cshtml
│   ├── Edit.cshtml
│   └── _[Module]FormPartial.cshtml

Services/
├── [Module]Service.cs
├── [Module]ValidationService.cs
└── [Module]CalculationService.cs

Scripts/
├── [module]-form.js              # JavaScript اصلی
├── [module]-validation.js        # اعتبارسنجی client-side
└── [module]-lookup.js            # AJAX calls
```

---

## **🔧 استانداردهای فنی**

### **1. 📅 مدیریت تاریخ شمسی**

#### **A. ViewModel Pattern**
```csharp
public class [Module]CreateEditViewModel
{
    // فیلدهای شمسی برای UI
    [Required(ErrorMessage = "تاریخ شروع الزامی است")]
    [PersianDate(IsRequired = true, MustBeFutureDate = false)]
    [Display(Name = "تاریخ شروع")]
    public string ValidFromShamsi { get; set; }

    [PersianDate(IsRequired = false, MustBeFutureDate = false)]
    [Display(Name = "تاریخ پایان")]
    public string ValidToShamsi { get; set; }

    // فیلدهای مخفی برای Entity
    [HiddenInput(DisplayValue = false)]
    public DateTime ValidFrom { get; set; }

    [HiddenInput(DisplayValue = false)]
    public DateTime ValidTo { get; set; }

    // متدهای تبدیل
    private DateTime ConvertPersianToDateTime(string persianDate)
    {
        if (string.IsNullOrWhiteSpace(persianDate)) 
            return DateTime.Now.AddYears(1);
        
        try 
        { 
            return PersianDateHelper.ToGregorianDate(persianDate); 
        }
        catch 
        { 
            return DateTime.Now.AddYears(1); 
        }
    }

    private DateTime? ConvertPersianToDateTimeNullable(string persianDate)
    {
        if (string.IsNullOrWhiteSpace(persianDate)) return null;
        try { return PersianDateHelper.ToGregorianDate(persianDate); }
        catch { return null; }
    }
}
```

#### **B. View Pattern**
```html
<!-- تاریخ شمسی با Persian DatePicker -->
<div class="form-group">
    @Html.LabelFor(m => m.ValidFromShamsi, new { @class = "form-label" })
    @Html.TextBoxFor(m => m.ValidFromShamsi, new { 
        @class = "form-control persian-datepicker", 
        placeholder = "مثال: 1404/06/23",
        required = "required"
    })
    @Html.ValidationMessageFor(m => m.ValidFromShamsi, "", new { @class = "text-danger" })
</div>
```

#### **C. JavaScript Pattern**
```javascript
// Initialize Persian DatePicker
$('.persian-datepicker').persianDatepicker({
    format: 'YYYY/MM/DD',
    calendar: {
        persian: {
            locale: 'fa',
            showHint: true,
            leapYearMode: 'algorithmic'
        }
    },
    checkDate: function(unix) {
        return unix < Date.now();
    },
    autoClose: true,
    initialValue: false,
    position: 'auto',
    viewMode: 'day',
    inputDelay: 800,
    navigator: {
        enabled: true,
        scroll: { enabled: true }
    },
    toolbox: {
        enabled: true,
        todayButton: { enabled: true, text: 'امروز' },
        submitButton: { enabled: true, text: 'تأیید' },
        clearButton: { enabled: true, text: 'پاک کردن' }
    }
});

// Validation Helper
function isValidPersianDate(persianDate) {
    if (!persianDate || persianDate.trim() === '') return false;
    
    var englishDate = persianDate.replace(/[۰-۹]/g, function(d) {
        return String.fromCharCode(d.charCodeAt(0) - '۰'.charCodeAt(0) + '0'.charCodeAt(0));
    });
    
    var persianDatePattern = /^[12][0-9]{3}\/(0[1-9]|1[0-2])\/(0[1-9]|[12][0-9]|3[01])$/;
    return persianDatePattern.test(englishDate);
}

function convertPersianToGregorian(persianDate) {
    if (!persianDate) return null;
    
    try {
        console.log('🔄 Converting Persian date to Gregorian:', persianDate);
        
        var englishDate = persianDate.replace(/[۰-۹]/g, function(d) {
            return String.fromCharCode(d.charCodeAt(0) - '۰'.charCodeAt(0) + '0'.charCodeAt(0));
        });
        
        var parts = englishDate.split('/');
        if (parts.length === 3) {
            var year = parseInt(parts[0]);
            var month = parseInt(parts[1]);
            var day = parseInt(parts[2]);
            
            var gregorianYear = year + 621;
            var result = new Date(gregorianYear, month - 1, day);
            console.log('✅ Converted date:', result);
            return result;
        }
    } catch (e) {
        console.error('❌ Error converting Persian date:', e);
    }
    
    return null;
}
```

### **2. 📝 استاندارد لاگ‌گذاری**

#### **A. Console Logging Pattern**
```javascript
$(document).ready(function() {
    console.log('🏥 [Module] Form - Production Ready Version Loaded');
    console.log('📊 Form Data:', {
        id: '@Model.Id',
        name: '@Model.Name',
        // ... سایر فیلدها
        timestamp: new Date().toISOString()
    });
    
    // Event Logging
    $('.form-control').on('change', function() {
        var $input = $(this);
        var fieldName = $input.attr('name') || $input.attr('id');
        var value = $input.val();
        
        console.log('📝 Field Changed:', {
            field: fieldName,
            value: value,
            timestamp: new Date().toISOString()
        });
    });
    
    // AJAX Request Logging
    $(document).ajaxStart(function() {
        console.log('🔄 AJAX Request Started');
    }).ajaxComplete(function(event, xhr, settings) {
        console.log('✅ AJAX Request Completed:', {
            url: settings.url,
            method: settings.type,
            status: xhr.status,
            responseTime: new Date().toISOString()
        });
    }).ajaxError(function(event, xhr, settings, error) {
        console.error('❌ AJAX Request Failed:', {
            url: settings.url,
            method: settings.type,
            status: xhr.status,
            error: error,
            response: xhr.responseText
        });
    });
});
```

#### **B. Server-Side Logging Pattern**
```csharp
public class [Module]Controller : Controller
{
    private readonly ILogger<[Module]Controller> _logger;
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Module]CreateEditViewModel model)
    {
        _logger.LogInformation("🏥 [Module] Create action started by user {UserId}", 
            User.Identity.Name);
        
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("⚠️ [Module] Create failed - Model validation errors: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }
            
            var result = await _service.CreateAsync(model);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ [Module] Create successful - ID: {Id}", result.Data.Id);
                return RedirectToAction("Index");
            }
            else
            {
                _logger.LogError("❌ [Module] Create failed - {Error}", result.ErrorMessage);
                ModelState.AddModelError("", result.ErrorMessage);
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 [Module] Create exception occurred");
            ModelState.AddModelError("", "خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید.");
            return View(model);
        }
    }
}
```

### **3. 🛡️ مقاوم‌سازی در برابر خطا**

#### **A. Client-Side Error Handling**
```javascript
// Global Error Handler
window.addEventListener('error', function(e) {
    console.error('💥 Global Error:', {
        message: e.message,
        filename: e.filename,
        lineno: e.lineno,
        colno: e.colno,
        stack: e.error ? e.error.stack : 'No stack trace',
        timestamp: new Date().toISOString()
    });
});

// AJAX Error Handler
function handleAjaxError(xhr, status, error) {
    console.error('❌ AJAX Error:', {
        status: status,
        error: error,
        response: xhr.responseText,
        url: xhr.responseURL,
        timestamp: new Date().toISOString()
    });
    
    var errorMessage = 'خطا در ارتباط با سرور';
    
    try {
        var response = JSON.parse(xhr.responseText);
        if (response.message) {
            errorMessage = response.message;
        }
    } catch (e) {
        // Use default error message
    }
    
    showError(errorMessage);
}

// Form Validation Error Handler
function handleValidationError(fieldName, errorMessage) {
    console.warn('⚠️ Validation Error:', {
        field: fieldName,
        message: errorMessage,
        timestamp: new Date().toISOString()
    });
    
    var $field = $('[name="' + fieldName + '"]');
    $field.addClass('is-invalid');
    $field.closest('.form-group').find('.validation-feedback').remove();
    $field.closest('.form-group').append(
        '<div class="validation-feedback text-danger">' + errorMessage + '</div>'
    );
}
```

#### **B. Server-Side Error Handling**
```csharp
public class [Module]Service
{
    private readonly ILogger<[Module]Service> _logger;
    
    public async Task<ServiceResult<[Module]>> CreateAsync([Module]CreateEditViewModel model)
    {
        try
        {
            _logger.LogInformation("🔄 [Module] Create service started");
            
            // Validation
            var validationResult = await ValidateModelAsync(model);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("⚠️ [Module] Validation failed: {Errors}", 
                    string.Join(", ", validationResult.Errors));
                return ServiceResult<[Module]>.Failure(validationResult.Errors.First());
            }
            
            // Business Logic
            var entity = model.ToEntity();
            var result = await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            
            _logger.LogInformation("✅ [Module] Create successful - ID: {Id}", result.Id);
            return ServiceResult<[Module]>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 [Module] Create service exception");
            return ServiceResult<[Module]>.Failure("خطای غیرمنتظره‌ای رخ داد");
        }
    }
}
```

### **4. ⚡ بهینه‌سازی عملکرد**

#### **A. Lazy Loading Pattern**
```javascript
// Lazy load heavy components
function initializeForm() {
    // Load essential components first
    initializeBasicValidation();
    initializeDatePickers();
    
    // Load heavy components after a delay
    setTimeout(function() {
        initializeAdvancedFeatures();
        initializeLookupServices();
    }, 100);
}

// Debounced AJAX calls
var debouncedLookup = debounce(function(searchTerm) {
    performLookup(searchTerm);
}, 300);

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}
```

#### **B. Caching Pattern**
```javascript
// Simple cache for lookup data
var lookupCache = {};

function getCachedLookup(key, fetchFunction) {
    if (lookupCache[key]) {
        console.log('📦 Using cached data for:', key);
        return Promise.resolve(lookupCache[key]);
    }
    
    console.log('🔄 Fetching fresh data for:', key);
    return fetchFunction().then(function(data) {
        lookupCache[key] = data;
        return data;
    });
}
```

### **5. 🎨 طراحی UI تمیز**

#### **A. CSS Variables Pattern**
```css
:root {
    /* Medical Theme Colors */
    --medical-primary: #1e3a8a;
    --medical-secondary: #7c3aed;
    --medical-success: #059669;
    --medical-warning: #d97706;
    --medical-danger: #dc2626;
    --medical-info: #0891b2;
    
    /* Form Colors */
    --form-bg: #ffffff;
    --form-border: #e5e7eb;
    --form-focus: var(--medical-primary);
    --form-error: var(--medical-danger);
    --form-success: var(--medical-success);
    
    /* Spacing */
    --form-padding: 1.5rem;
    --form-margin: 1rem;
    --form-border-radius: 0.5rem;
    
    /* Typography */
    --form-font-size: 0.875rem;
    --form-font-weight: 500;
    --form-line-height: 1.5;
}

/* Form Container */
.form-container {
    background: var(--form-bg);
    border: 1px solid var(--form-border);
    border-radius: var(--form-border-radius);
    padding: var(--form-padding);
    margin: var(--form-margin);
    box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
}

/* Form Groups */
.form-group {
    margin-bottom: 1.5rem;
}

.form-group.focused .form-label {
    color: var(--form-focus);
    transform: translateY(-2px);
    transition: all 0.3s ease;
}

.form-group.focused .form-control {
    border-color: var(--form-focus);
    box-shadow: 0 0 0 0.2rem rgba(30, 58, 138, 0.25);
    transform: translateY(-1px);
}

/* Validation States */
.form-control.is-valid {
    border-color: var(--form-success);
    box-shadow: 0 0 0 0.2rem rgba(5, 150, 105, 0.25);
}

.form-control.is-invalid {
    border-color: var(--form-error);
    box-shadow: 0 0 0 0.2rem rgba(220, 38, 38, 0.25);
}

.validation-feedback {
    font-size: var(--form-font-size);
    margin-top: 0.5rem;
    padding: 0.5rem;
    border-radius: 0.375rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.validation-feedback i {
    font-size: 1rem;
}
```

#### **B. Component Pattern**
```html
<!-- Form Card Component -->
<div class="form-card">
    <div class="form-card-header">
        <h4 class="form-card-title">
            <i class="fas fa-edit"></i>
            اطلاعات اصلی
        </h4>
    </div>
    <div class="form-card-body">
        <!-- Form fields -->
    </div>
</div>

<!-- Input Group Component -->
<div class="form-group">
    <label class="form-label">درصد پوشش بیمه</label>
    <div class="input-group">
        <input type="number" class="form-control" placeholder="درصد پوشش">
        <span class="input-group-text">%</span>
    </div>
    <div class="form-text">
        <i class="fas fa-info-circle"></i>
        درصد پوشش بیمه (0 تا 100). مثال: 90 به معنای 90% پوشش بیمه و 10% پرداخت بیمار است.
    </div>
</div>
```

---

## **✅ چک لیست کامل**

### **📋 قبل از شروع توسعه**

- [ ] **تحلیل نیازمندی‌ها**: درک کامل منطق کسب‌وکار
- [ ] **طراحی معماری**: تعیین ساختار فایل‌ها و کلاس‌ها
- [ ] **تعریف ViewModel**: طراحی مدل داده‌ای مناسب
- [ ] **طراحی UI/UX**: تعیین ظاهر و تجربه کاربری

### **🔧 توسعه Backend**

#### **Controller**
- [ ] **Dependency Injection**: تزریق صحیح سرویس‌ها
- [ ] **Authorization**: اعمال کنترل دسترسی مناسب
- [ ] **Error Handling**: مدیریت خطاها با try-catch
- [ ] **Logging**: لاگ‌گذاری مناسب برای تمام عملیات
- [ ] **Validation**: اعتبارسنجی ModelState
- [ ] **Anti-Forgery Token**: حفاظت از CSRF

#### **Service**
- [ ] **Business Logic**: پیاده‌سازی منطق کسب‌وکار
- [ ] **Validation Service**: اعتبارسنجی پیشرفته
- [ ] **Error Handling**: مدیریت خطاها
- [ ] **Logging**: لاگ‌گذاری عملیات
- [ ] **Transaction Management**: مدیریت تراکنش‌ها

#### **Repository**
- [ ] **Data Access**: دسترسی به داده‌ها
- [ ] **Query Optimization**: بهینه‌سازی کوئری‌ها
- [ ] **Error Handling**: مدیریت خطاهای دیتابیس

### **🎨 توسعه Frontend**

#### **HTML Structure**
- [ ] **Semantic HTML**: استفاده از تگ‌های معنادار
- [ ] **Accessibility**: رعایت استانداردهای دسترسی
- [ ] **Form Structure**: ساختار صحیح فرم
- [ ] **Validation Messages**: پیام‌های خطای مناسب

#### **CSS Styling**
- [ ] **CSS Variables**: استفاده از متغیرهای CSS
- [ ] **Responsive Design**: طراحی واکنش‌گرا
- [ ] **Medical Theme**: تم پزشکی مناسب
- [ ] **Form Styling**: استایل‌دهی فرم‌ها
- [ ] **Validation States**: حالت‌های اعتبارسنجی

#### **JavaScript Functionality**
- [ ] **Form Initialization**: مقداردهی اولیه فرم
- [ ] **Date Picker**: پیاده‌سازی انتخابگر تاریخ شمسی
- [ ] **Validation**: اعتبارسنجی client-side
- [ ] **AJAX Calls**: فراخوانی‌های AJAX
- [ ] **Error Handling**: مدیریت خطاها
- [ ] **Logging**: لاگ‌گذاری کنسول
- [ ] **Performance**: بهینه‌سازی عملکرد

### **📅 مدیریت تاریخ شمسی**

- [ ] **Persian DatePicker**: پیاده‌سازی انتخابگر تاریخ شمسی
- [ ] **Date Validation**: اعتبارسنجی فرمت تاریخ
- [ ] **Date Conversion**: تبدیل تاریخ شمسی به میلادی
- [ ] **Date Range Validation**: اعتبارسنجی بازه زمانی
- [ ] **Error Handling**: مدیریت خطاهای تبدیل تاریخ

### **🛡️ امنیت و مقاوم‌سازی**

- [ ] **Input Validation**: اعتبارسنجی ورودی‌ها
- [ ] **XSS Protection**: حفاظت از XSS
- [ ] **CSRF Protection**: حفاظت از CSRF
- [ ] **SQL Injection**: حفاظت از SQL Injection
- [ ] **Error Handling**: مدیریت خطاها
- [ ] **Logging Security**: لاگ‌گذاری امنیتی

### **⚡ بهینه‌سازی عملکرد**

- [ ] **Lazy Loading**: بارگذاری تنبل
- [ ] **Caching**: کش کردن داده‌ها
- [ ] **Debouncing**: کاهش فراخوانی‌های غیرضروری
- [ ] **Minification**: فشرده‌سازی فایل‌ها
- [ ] **CDN**: استفاده از CDN

### **🧪 تست و اعتبارسنجی**

- [ ] **Unit Tests**: تست واحد
- [ ] **Integration Tests**: تست یکپارچگی
- [ ] **UI Tests**: تست رابط کاربری
- [ ] **Performance Tests**: تست عملکرد
- [ ] **Security Tests**: تست امنیت

### **📚 مستندسازی**

- [ ] **Code Comments**: کامنت‌گذاری کد
- [ ] **API Documentation**: مستندسازی API
- [ ] **User Guide**: راهنمای کاربر
- [ ] **Technical Documentation**: مستندسازی فنی

---

## **🎯 نتیجه‌گیری**

این استاندارد جامع برای فرم‌های پیچیده شامل:

1. **📅 مدیریت کامل تاریخ شمسی**
2. **📝 لاگ‌گذاری حرفه‌ای**
3. **🛡️ مقاوم‌سازی در برابر خطا**
4. **⚡ بهینه‌سازی عملکرد**
5. **🎨 طراحی UI تمیز**
6. **✅ چک لیست کامل**

با رعایت این استاندارد، فرم‌های پیچیده به صورت حرفه‌ای، امن، و قابل نگهداری پیاده‌سازی خواهند شد.

---

**📅 تاریخ ایجاد**: 1404/06/23  
**👤 ایجادکننده**: AI Assistant  
**🔄 نسخه**: 1.0  
**📋 وضعیت**: فعال
