# 🏥 Medical Forms Design & Implementation Contract
## Senior Full-Stack .NET Architect Guidelines

### 📋 Original Principles (User Provided)

#### 1. سادگی و روانی (Simplicity and Fluency)
- فقط فیلدهای ضروری نمایش داده شود (حداقل داده‌ی لازم برای عملیات)
- اطلاعات کمتر استفاده‌شده در بخش‌های پیشرفته (Collapsible / Accordion) یا تب جدا
- جلوگیری از فیلدهای غیرضروری (مثل توضیحات اضافی که کاربرد عملی ندارند)

#### 2. استانداردهای UX برای درمان (Medical UX Standards)
- فیلدها دسته‌بندی‌شده و گروه‌بندی باشند (اطلاعات شخصی، پزشکی، بیمه)
- استفاده از Labelهای واضح و فارسی رسمی
- استفاده از Placeholder راهنما
- دکمه‌ها با رنگ استاندارد:
  - سبز/آبی → ذخیره
  - خاکستری → انصراف/بازگشت
  - قرمز → حذف (فقط در فرم ویرایش)

#### 3. خطاها و اعتبارسنجی (Validation)
- خطاها شفاف و دوستانه نمایش داده شوند
- جلوگیری از خطاهای تکراری با Masked Input
- اعتبارسنجی همزمان (Real-time Validation)

#### 4. دسترس‌پذیری (Accessibility)
- اندازه فونت‌ها خوانا (۱۴–۱۶ پیکسل)
- رنگ‌ها با کنتراست بالا
- پشتیبانی از کیبورد ناوبری

#### 5. عملکردی (Functional)
- دکمه‌های واضح: ذخیره و بستن یا ذخیره و ادامه
- امکان Auto Save یا هشدار خروج بدون ذخیره
- قابلیت Prefill (پر شدن خودکار)
- قابلیت Searchable Dropdown

#### 6. ظاهری (UI)
- استفاده از Grid ساده (دو ستونه) برای فرم‌های طولانی
- استفاده از Iconهای کوچک فقط برای کمک
- استفاده از Modal یا Drawer برای فرم‌های کوچک

---

### 🚀 Enhanced Principles (Senior Architect Additions)

#### 7. امنیت و اعتبارسنجی پیشرفته (Advanced Security & Validation)
```csharp
// Server-side validation with FluentValidation
public class ServiceCreateEditValidator : AbstractValidator<ServiceCreateEditViewModel>
{
    public ServiceCreateEditValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان خدمت الزامی است")
            .MaximumLength(200).WithMessage("عنوان نمی‌تواند بیش از ۲۰۰ کاراکتر باشد")
            .Matches(@"^[\u0600-\u06FF\s\d\-\(\)]+$").WithMessage("فقط حروف فارسی، اعداد و علائم مجاز");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد خدمت الزامی است")
            .Matches(@"^\d{3,10}$").WithMessage("کد باید ۳ تا ۱۰ رقم باشد")
            .MustAsync(BeUniqueCodeAsync).WithMessage("کد خدمت تکراری است");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("قیمت باید بزرگتر از صفر باشد")
            .LessThanOrEqualTo(999999999).WithMessage("قیمت نمی‌تواند بیش از ۹۹۹,۹۹۹,۹۹۹ باشد");
    }
}
```

#### 8. مدیریت State و Performance (State Management & Performance)
```javascript
// Medical Environment State Management
const MedicalFormState = {
    // Auto-save every 30 seconds
    autoSaveInterval: 30000,
    
    // Form validation state
    isValid: false,
    validationErrors: {},
    
    // Unsaved changes tracking
    hasUnsavedChanges: false,
    
    // Loading states
    isSubmitting: false,
    isSaving: false,
    
    // Initialize form state
    init: function() {
        this.trackChanges();
        this.setupAutoSave();
        this.setupBeforeUnload();
    },
    
    // Track form changes
    trackChanges: function() {
        $('form :input').on('change keyup paste', function() {
            MedicalFormState.hasUnsavedChanges = true;
            MedicalFormState.validateField($(this));
        });
    },
    
    // Auto-save functionality
    setupAutoSave: function() {
        setInterval(() => {
            if (MedicalFormState.hasUnsavedChanges && MedicalFormState.isValid) {
                MedicalFormState.autoSave();
            }
        }, this.autoSaveInterval);
    },
    
    // Warn before leaving with unsaved changes
    setupBeforeUnload: function() {
        window.addEventListener('beforeunload', function(e) {
            if (MedicalFormState.hasUnsavedChanges) {
                e.preventDefault();
                e.returnValue = 'تغییرات ذخیره نشده‌اند. آیا مطمئن هستید؟';
            }
        });
    }
};
```

#### 9. Error Handling & Recovery (مدیریت خطا و بازیابی)
```csharp
// Global exception handling for medical forms
public class MedicalFormExceptionHandler : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        var logger = context.HttpContext.RequestServices.GetService<ILogger>();
        
        // Log with medical context
        logger.LogError(exception, "🏥 MEDICAL FORM ERROR: {Message} | User: {UserId} | Action: {Action}",
            exception.Message,
            context.HttpContext.User?.Identity?.Name ?? "Anonymous",
            context.RouteData.Values["action"]);
        
        // Return appropriate response
        if (context.HttpContext.Request.IsAjaxRequest())
        {
            context.Result = new JsonResult(new
            {
                success = false,
                message = "خطای سیستمی رخ داد. لطفاً دوباره تلاش کنید.",
                errorCode = "MEDICAL_FORM_ERROR",
                timestamp = DateTime.Now
            });
        }
        else
        {
            context.Result = new RedirectToActionResult("Error", "Home", new { area = "" });
        }
        
        context.ExceptionHandled = true;
    }
}
```

#### 10. Accessibility & Medical Standards (دسترس‌پذیری و استانداردهای پزشکی)
```html
<!-- Medical Form Accessibility Template -->
<form id="medicalForm" class="medical-form" novalidate>
    <!-- Form Group with proper labeling -->
    <div class="form-group">
        <label for="patientName" class="form-label required">
            نام و نام خانوادگی بیمار
            <span class="required-indicator" aria-label="فیلد اجباری">*</span>
        </label>
        <input type="text" 
               id="patientName" 
               name="patientName" 
               class="form-control"
               required
               maxlength="100"
               pattern="[\u0600-\u06FF\s]+"
               aria-describedby="patientNameHelp patientNameError"
               autocomplete="name">
        <div id="patientNameHelp" class="form-text">
            نام کامل بیمار را وارد کنید (فقط حروف فارسی)
        </div>
        <div id="patientNameError" class="invalid-feedback" role="alert"></div>
    </div>
    
    <!-- Medical Code Input with validation -->
    <div class="form-group">
        <label for="medicalCode" class="form-label required">
            کد پزشکی
            <span class="required-indicator" aria-label="فیلد اجباری">*</span>
        </label>
        <input type="text" 
               id="medicalCode" 
               name="medicalCode" 
               class="form-control medical-code-input"
               required
               maxlength="10"
               pattern="\d{3,10}"
               inputmode="numeric"
               aria-describedby="medicalCodeHelp medicalCodeError"
               autocomplete="off">
        <div id="medicalCodeHelp" class="form-text">
            کد ۳ تا ۱۰ رقمی پزشکی را وارد کنید
        </div>
        <div id="medicalCodeError" class="invalid-feedback" role="alert"></div>
    </div>
</form>
```

#### 11. Real-time Validation & User Feedback (اعتبارسنجی همزمان)
```javascript
// Medical Form Real-time Validation
const MedicalFormValidation = {
    // Validation rules for medical forms
    rules: {
        patientName: {
            required: true,
            minlength: 3,
            maxlength: 100,
            pattern: /^[\u0600-\u06FF\s]+$/
        },
        medicalCode: {
            required: true,
            pattern: /^\d{3,10}$/,
            remote: {
                url: '/Admin/Service/CheckServiceCode',
                type: 'POST',
                data: {
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                }
            }
        },
        price: {
            required: true,
            min: 1,
            max: 999999999,
            number: true
        }
    },
    
    // Custom validation messages
    messages: {
        patientName: {
            required: "نام بیمار الزامی است",
            minlength: "نام باید حداقل ۳ کاراکتر باشد",
            maxlength: "نام نمی‌تواند بیش از ۱۰۰ کاراکتر باشد",
            pattern: "فقط حروف فارسی مجاز است"
        },
        medicalCode: {
            required: "کد پزشکی الزامی است",
            pattern: "کد باید ۳ تا ۱۰ رقم باشد",
            remote: "کد پزشکی تکراری است"
        },
        price: {
            required: "قیمت الزامی است",
            min: "قیمت باید بزرگتر از صفر باشد",
            max: "قیمت نمی‌تواند بیش از ۹۹۹,۹۹۹,۹۹۹ باشد",
            number: "فقط عدد مجاز است"
        }
    },
    
    // Initialize validation
    init: function(formSelector) {
        $(formSelector).validate({
            rules: this.rules,
            messages: this.messages,
            errorElement: 'div',
            errorClass: 'invalid-feedback',
            errorPlacement: function(error, element) {
                error.insertAfter(element);
            },
            highlight: function(element) {
                $(element).addClass('is-invalid').removeClass('is-valid');
            },
            unhighlight: function(element) {
                $(element).removeClass('is-invalid').addClass('is-valid');
            },
            submitHandler: function(form) {
                MedicalFormValidation.submitForm(form);
            }
        });
    },
    
    // Custom form submission
    submitForm: function(form) {
        const $form = $(form);
        const $submitBtn = $form.find('button[type="submit"]');
        
        // Show loading state
        $submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> در حال ذخیره...');
        
        // Submit form
        $.ajax({
            url: $form.attr('action'),
            type: 'POST',
            data: $form.serialize(),
            success: function(response) {
                if (response.success) {
                    showMedicalToast('✅ موفقیت', response.message, 'success');
                    MedicalFormState.hasUnsavedChanges = false;
                    
                    // Redirect or close modal
                    setTimeout(() => {
                        if (response.redirectUrl) {
                            window.location.href = response.redirectUrl;
                        } else {
                            $('#medicalModal').modal('hide');
                        }
                    }, 1500);
                } else {
                    showMedicalToast('❌ خطا', response.message, 'error');
                }
            },
            error: function(xhr, status, error) {
                showMedicalToast('❌ خطا', 'خطای سیستمی رخ داد. لطفاً دوباره تلاش کنید.', 'error');
                console.error('🏥 MEDICAL: Form submission error:', error);
            },
            complete: function() {
                // Reset button state
                $submitBtn.prop('disabled', false).html('ذخیره');
            }
        });
    }
};
```

#### 12. Medical Data Integrity & Audit Trail (یکپارچگی داده و Audit)
```csharp
// Medical data integrity with audit trail
public class MedicalFormAuditService
{
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;
    
    public MedicalFormAuditService(ILogger logger, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }
    
    // Audit form submission
    public async Task AuditFormSubmissionAsync<T>(T formData, string formType, string action)
    {
        var auditEntry = new FormAuditEntry
        {
            FormType = formType,
            Action = action,
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName,
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent(),
            FormData = JsonConvert.SerializeObject(formData),
            Timestamp = DateTime.Now,
            SessionId = GetSessionId()
        };
        
        _logger.Information("🏥 MEDICAL AUDIT: Form {FormType} {Action} by {User} | Data: {Data}",
            formType, action, _currentUserService.UserName, auditEntry.FormData);
        
        // Save to audit table
        await SaveAuditEntryAsync(auditEntry);
    }
    
    // Validate medical data integrity
    public async Task<ValidationResult> ValidateMedicalDataAsync<T>(T data)
    {
        var result = new ValidationResult();
        
        // Check for duplicate entries
        if (await HasDuplicateAsync(data))
        {
            result.AddError("DuplicateEntry", "رکورد تکراری یافت شد");
        }
        
        // Validate business rules
        if (!await ValidateBusinessRulesAsync(data))
        {
            result.AddError("BusinessRuleViolation", "قوانین تجاری نقض شده است");
        }
        
        return result;
    }
}
```

#### 13. Responsive Design & Mobile Optimization (طراحی Responsive)
```css
/* Medical Form Responsive Design */
.medical-form {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
}

/* Mobile-first approach */
@media (max-width: 768px) {
    .medical-form {
        padding: 10px;
    }
    
    .form-group {
        margin-bottom: 15px;
    }
    
    .form-label {
        font-size: 14px;
        margin-bottom: 5px;
    }
    
    .form-control {
        font-size: 16px; /* Prevents zoom on iOS */
        padding: 12px;
        border-radius: 8px;
    }
    
    /* Stack form fields vertically on mobile */
    .row .col-md-6 {
        flex: 0 0 100%;
        max-width: 100%;
    }
    
    /* Optimize buttons for touch */
    .btn {
        min-height: 44px;
        font-size: 16px;
        padding: 12px 20px;
    }
    
    /* Modal optimization for mobile */
    .modal-dialog {
        margin: 10px;
        max-width: calc(100% - 20px);
    }
}

/* Tablet optimization */
@media (min-width: 769px) and (max-width: 1024px) {
    .medical-form {
        padding: 15px;
    }
    
    .form-control {
        font-size: 15px;
        padding: 10px;
    }
}

/* High contrast mode for accessibility */
@media (prefers-contrast: high) {
    .form-control {
        border: 2px solid #000;
    }
    
    .btn {
        border: 2px solid currentColor;
    }
    
    .invalid-feedback {
        color: #d00;
        font-weight: bold;
    }
}
```

#### 14. Performance Optimization & Caching (بهینه‌سازی عملکرد)
```csharp
// Medical form performance optimization
public class MedicalFormOptimizationService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;
    
    public MedicalFormOptimizationService(IMemoryCache cache, ILogger logger)
    {
        _cache = cache;
        _logger = logger;
    }
    
    // Cache frequently used dropdown data
    public async Task<List<SelectListItem>> GetCachedDropdownDataAsync(string cacheKey, Func<Task<List<SelectListItem>>> dataProvider)
    {
        if (_cache.TryGetValue(cacheKey, out List<SelectListItem> cachedData))
        {
            _logger.Debug("🏥 MEDICAL: Using cached dropdown data for {CacheKey}", cacheKey);
            return cachedData;
        }
        
        var data = await dataProvider();
        
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
            .SetAbsoluteExpiration(TimeSpan.FromHours(2));
        
        _cache.Set(cacheKey, data, cacheOptions);
        
        _logger.Information("🏥 MEDICAL: Cached dropdown data for {CacheKey} with {Count} items", 
            cacheKey, data.Count);
        
        return data;
    }
    
    // Optimize form loading
    public async Task<FormLoadResult> OptimizeFormLoadAsync<T>(int id, string formType)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Parallel loading of form data
        var tasks = new[]
        {
            LoadFormDataAsync<T>(id),
            LoadDropdownDataAsync(formType),
            LoadValidationRulesAsync(formType)
        };
        
        await Task.WhenAll(tasks);
        
        stopwatch.Stop();
        
        _logger.Information("🏥 MEDICAL: Form {FormType} loaded in {ElapsedMs}ms", 
            formType, stopwatch.ElapsedMilliseconds);
        
        return new FormLoadResult
        {
            FormData = tasks[0].Result,
            DropdownData = tasks[1].Result,
            ValidationRules = tasks[2].Result,
            LoadTime = stopwatch.ElapsedMilliseconds
        };
    }
}
```

---

### 🎯 Implementation Checklist (چک‌لیست پیاده‌سازی)

#### ✅ Form Structure
- [ ] فیلدهای ضروری در صفحه اصلی
- [ ] دسته‌بندی فیلدها (شخصی، تماس، بیمه)
- [ ] Label واضح + Placeholder نمونه
- [ ] Validation همزمان + Mask برای تاریخ/کد ملی/موبایل
- [ ] دکمه‌های استاندارد رنگی
- [ ] Auto Save یا هشدار خروج بدون ذخیره
- [ ] طراحی Responsive (موبایل و دسکتاپ)

#### ✅ Security & Validation
- [ ] Server-side validation با FluentValidation
- [ ] Client-side validation با jQuery Validate
- [ ] Anti-forgery token در تمام فرم‌ها
- [ ] Input sanitization و encoding
- [ ] SQL Injection prevention
- [ ] XSS protection

#### ✅ Performance & UX
- [ ] Lazy loading برای dropdown‌های بزرگ
- [ ] Caching برای داده‌های ثابت
- [ ] Optimistic UI updates
- [ ] Progressive enhancement
- [ ] Keyboard navigation support
- [ ] Screen reader compatibility

#### ✅ Medical Standards
- [ ] Audit trail برای تمام تغییرات
- [ ] Data integrity validation
- [ ] Business rule enforcement
- [ ] Error recovery mechanisms
- [ ] Backup and restore capabilities
- [ ] Compliance with medical regulations

---

### 📝 Contract Agreement

**As a Senior Full-Stack .NET Architect and Mentor, I commit to:**

1. **Always follow these principles** in all medical form implementations
2. **Ensure bulletproof design** with comprehensive error handling
3. **Maintain performance standards** with caching and optimization
4. **Implement security best practices** for medical data protection
5. **Provide clear documentation** for all implementations
6. **Test thoroughly** before deployment
7. **Monitor and improve** continuously

**This contract is now stored in my knowledge base and will be applied to all future medical form implementations in the ClinicApp project.**

---

*Last Updated: 2025-01-23*
*Version: 1.0*
*Status: Active Contract*
