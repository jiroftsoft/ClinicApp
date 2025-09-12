# 📋 **قرارداد استانداردهای فرم - ClinicApp**

## 🎯 **هدف:**
این قرارداد شامل تمام استانداردهای لازم برای پیاده‌سازی فرم‌ها در سیستم ClinicApp است تا از تکرار خطاها جلوگیری شود.

---

## 🚫 **ممنوعیت‌های مطلق:**

### **1. استفاده از ViewBag:**
```csharp
// ❌ ممنوع - هرگز استفاده نکنید
ViewBag.Doctor = doctorResult.Data;
ViewBag.ClinicId = clinicId;

// ✅ صحیح - همیشه از Model استفاده کنید
Model.DoctorName = doctorResult.Data.FullName;
var overviewModel = new ScheduleOverviewViewModel { ClinicId = clinicId };
```

### **2. عدم استفاده از Anti-Forgery Token:**
```csharp
// ❌ ممنوع - هرگز استفاده نکنید
[HttpPost]
public ActionResult ActionName(Model model) // بدون ValidateAntiForgeryToken

// ✅ صحیح - همیشه از ValidateAntiForgeryToken استفاده کنید
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult ActionName(Model model)
```

### **3. عدم استفاده از ServiceResult Enhanced:**
```csharp
// ❌ ممنوع - هرگز استفاده نکنید
public async Task<string> CreateSchedule(Model model) // return string
public async Task<Exception> UpdateSchedule(Model model) // return Exception

// ✅ صحیح - همیشه از ServiceResult Enhanced استفاده کنید
public async Task<ServiceResult<Schedule>> CreateSchedule(Model model)
public async Task<ServiceResult<Schedule>> UpdateSchedule(Model model)
```

```csharp
// ❌ ممنوع - هرگز استفاده نکنید
RuleFor(x => x.DoctorId)
    .GreaterThan(0)
    .WithMessage("شناسه پزشک نامعتبر است.");
    // بدون WithErrorCode

// ✅ صحیح - همیشه از WithErrorCode استفاده کنید
RuleFor(x => x.DoctorId)
    .GreaterThan(0)
    .WithMessage("شناسه پزشک نامعتبر است.")
    .WithErrorCode("INVALID_DOCTOR_ID");
```

### **2. استفاده از setTimeout غیرقابل اعتماد:**
```javascript
// ❌ ممنوع - هرگز استفاده نکنید
setTimeout(function() {
    initializeSelect2();
}, 500);

// ✅ صحیح - از Promise استفاده کنید
loadDoctors().then(function() {
    initializeSelect2();
});
```

---

## ✅ **استانداردهای اجباری:**

### **1. Persian DatePicker (استاندارد اجباری):**
```html
<!-- ✅ همیشه این ساختار را استفاده کنید -->
<input type="text" 
       class="form-control persian-datepicker" 
       placeholder="انتخاب تاریخ" 
       value="" />
```

```javascript
// ✅ همیشه این تنظیمات را استفاده کنید
$('.persian-datepicker').each(function() {
    var $this = $(this);
    var currentValue = $this.val();
    
    // اگر مقدار اولیه مشکل‌ساز وجود دارد، آن را پاک کن
    if (currentValue && currentValue.includes('۷۸۳')) {
        $this.val('');
    }
    
    $this.persianDatepicker({
        format: 'YYYY/MM/DD',
        initialValue: false,
        autoClose: true,
        calendar: {
            persian: {
                locale: 'fa',
                showHint: true,
                leapYearMode: 'algorithmic'
            }
        }
    });
    
    // تنظیم مقدار اولیه صحیح
    setTimeout(function() {
        if (!$this.val() || $this.val().includes('۷۸۳')) {
            $this.val('');
        }
    }, 100);
});

// ✅ Event delegation برای تبدیل تاریخ شمسی به میلادی
$(document).on('change', '.persian-datepicker', function() {
    convertPersianDateToGregorian($(this));
});

$(document).on('input blur', '.persian-datepicker', function() {
    setTimeout(function() {
        convertPersianDateToGregorian($(this));
    }, 100);
});

// ✅ تابع جداگانه برای تبدیل تاریخ
function convertPersianDateToGregorian($element) {
    try {
        var fieldId = $element.attr('id');
        var persianDate = $element.val();
        
        if (persianDate && persianDate.trim() !== '') {
            // بررسی فرمت تاریخ فارسی
            var persianDatePattern = /^[۱۲۳۴۵۶۷۸۹۰]+[/][۱۲۳۴۵۶۷۸۹۰]+[/][۱۲۳۴۵۶۷۸۹۰]+$/;
            
            if (persianDatePattern.test(persianDate)) {
                // تبدیل تاریخ شمسی به میلادی
                var gregorianDate = persianDatepicker.parseDate(persianDate);
                if (gregorianDate) {
                    var isoDate = gregorianDate.toISOString().split('T')[0];
                    
                    // ذخیره در hidden field مربوطه
                    if (fieldId === 'startDateShamsi') {
                        $('#StartDate').val(isoDate);
                    } else if (fieldId === 'endDateShamsi') {
                        $('#EndDate').val(isoDate);
                    }
                    // برای سایر فیلدها نیز قابل تعمیم است
                }
            }
        }
    } catch (error) {
        console.error('خطا در تبدیل تاریخ:', error);
    }
}
```

**⚠️ نکات مهم Persian DatePicker:**
- **همیشه مقدار اولیه خالی** تنظیم کنید (`value=""`)
- **از onSelect callback استفاده نکنید** - باعث خطای JavaScript می‌شود
- **از Event Delegation استفاده کنید** برای مدیریت تغییرات
- **مقادیر مشکل‌ساز (۷۸۳) را پاک کنید** قبل از مقداردهی
- **تبدیل خودکار** تاریخ شمسی به میلادی انجام دهید
- **Validation فرمت** تاریخ فارسی را بررسی کنید

### **1.1. ViewModel برای Persian DatePicker:**
```csharp
// ✅ همیشه فیلدهای شمسی و میلادی را جداگانه تعریف کنید
public class SearchViewModel
{
    [Display(Name = "تاریخ شروع")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [Display(Name = "تاریخ شروع (شمسی)")]
    public string StartDateShamsi { get; set; }

    [Display(Name = "تاریخ پایان")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Display(Name = "تاریخ پایان (شمسی)")]
    public string EndDateShamsi { get; set; }
}
```

### **1.2. Controller برای Persian DatePicker:**
```csharp
// ✅ همیشه مقدار اولیه خالی برای فیلدهای شمسی تنظیم کنید
public ActionResult Index()
{
    var model = new SearchViewModel
    {
        StartDate = DateTime.Today,
        EndDate = DateTime.Today.AddDays(7),
        StartDateShamsi = "", // مقدار اولیه خالی
        EndDateShamsi = "", // مقدار اولیه خالی
        // سایر فیلدها...
    };
    return View(model);
}

// ✅ در Action های POST، اولویت با فیلدهای شمسی باشد
[HttpPost]
public async Task<JsonResult> Search(string startDateShamsi = null, string endDateShamsi = null, 
                                    string startDate = null, string endDate = null)
{
    DateTime? start = null;
    DateTime? end = null;

    // اولویت با فیلدهای شمسی
    if (!string.IsNullOrEmpty(startDateShamsi))
    {
        start = startDateShamsi.ToDateTimeFromPersian();
    }
    else if (!string.IsNullOrEmpty(startDate))
    {
        if (DateTime.TryParse(startDate, out var parsedStart))
            start = parsedStart;
    }

    if (!string.IsNullOrEmpty(endDateShamsi))
    {
        end = endDateShamsi.ToDateTimeFromPersian();
    }
    else if (!string.IsNullOrEmpty(endDate))
    {
        if (DateTime.TryParse(endDate, out var parsedEnd))
            end = parsedEnd;
    }

    // ادامه منطق...
}
```

### **1.3. Extension Methods برای تبدیل تاریخ:**
```csharp
// ✅ همیشه این Extension Methods را در DateTimeExtensions.cs اضافه کنید
public static class DateTimeExtensions
{
    /// <summary>
    /// تبدیل تاریخ شمسی به میلادی (alias برای ToDateTime)
    /// </summary>
    public static DateTime ToDateTimeFromPersian(this string persianDate)
    {
        return ToDateTime(persianDate);
    }

    /// <summary>
    /// تبدیل تاریخ شمسی به میلادی (nullable) (alias برای ToDateTimeNullable)
    /// </summary>
    public static DateTime? ToDateTimeFromPersianNullable(this string persianDate)
    {
        return ToDateTimeNullable(persianDate);
    }

    /// <summary>
    /// تبدیل تاریخ میلادی به رشته شمسی
    /// </summary>
    public static string ToPersianDateString(this DateTime date)
    {
        return PersianDateHelper.ToPersianDate(date);
    }
}
```

### **2. Anti-Forgery Token در View:**
```html
<!-- ✅ همیشه این ساختار را استفاده کنید -->
<form id="formId" class="needs-validation" novalidate>
    @Html.AntiForgeryToken()  <!-- اجباری برای تمام فرم‌ها -->
    <div class="form-body">
        <!-- محتوای فرم -->
    </div>
</form>
```

```javascript
// ✅ همیشه token را در AJAX ارسال کنید
$.ajax({
    url: '@Url.Action("ActionName", "ControllerName")',
    type: 'POST',
    data: formData, // formData شامل token است
    success: function(data) { /* ... */ }
});

// ✅ برای ارسال دستی token
$.ajax({
    url: '@Url.Action("ActionName", "ControllerName")',
    type: 'POST',
    data: { 
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
        // سایر داده‌ها
    },
    success: function(data) { /* ... */ }
});
```

### **3. Select2 Integration:**
```javascript
// ✅ برای دراپ‌داون‌های معمولی
$('#doctorFilter').select2({
    placeholder: 'انتخاب پزشک',
    allowClear: true,
    width: '100%'
});

// ✅ برای دراپ‌داون‌های داخل Modal
$('#doctorId').select2({
    placeholder: 'انتخاب پزشک',
    allowClear: true,
    width: '100%',
    dropdownParent: $('#modalId') // کلید حل مشکل Modal
});
```

### **3. Bootstrap Modal Integration:**
```javascript
// ✅ همیشه این Event Handler را استفاده کنید
$('#modalId').on('shown.bs.modal', function () {
    // Refresh Select2 if needed
    if ($('#selectElement').hasClass('select2-hidden-accessible')) {
        $('#selectElement').select2('destroy');
        $('#selectElement').select2({
            placeholder: 'انتخاب...',
            allowClear: true,
            width: '100%',
            dropdownParent: $('#modalId')
        });
    }
});
```

---

## 🔧 **الگوهای پیاده‌سازی:**

### **1. AJAX Response Parsing Pattern (الزامی برای تمام AJAX calls):**
```javascript
// ✅ الگوی صحیح - AJAX Response Parsing
function performAjaxAction() {
    $.ajax({
        url: '@Url.Action("ActionName", "ControllerName")',
        type: 'POST',
        dataType: 'json', // الزامی
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8', // الزامی
        data: { 
            param1: value1,
            param2: value2,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            // Parse response if it's a string (مهم!)
            if (typeof response === 'string') {
                try {
                    response = JSON.parse(response);
                } catch (e) {
                    console.error('Error parsing response:', e);
                    showAlert('error', 'خطا در پردازش پاسخ سرور');
                    return;
                }
            }
            
            if (response && response.success === true) {
                showAlert('success', response.message || 'عملیات با موفقیت انجام شد');
                // Additional success actions
            } else {
                showAlert('error', (response && response.message) || 'خطا در انجام عملیات');
            }
        },
        error: function() {
            showAlert('error', 'خطا در ارتباط با سرور');
        }
    });
}

// ❌ الگوی نادرست - بدون response parsing
function badAjaxExample() {
    $.ajax({
        url: '@Url.Action("ActionName", "ControllerName")',
        type: 'POST',
        // Missing dataType and contentType
        success: function(response) {
            // Direct response check without parsing
            if (response.success) { // ممکن است کار نکند
                // ...
            }
        }
    });
}
```

**نکات مهم:**
- همیشه `dataType: 'json'` و `contentType` را اضافه کنید
- همیشه response را parse کنید اگر string است
- از try-catch برای JSON.parse استفاده کنید
- همیشه `__RequestVerificationToken` را اضافه کنید

### **2. Loading Data Pattern:**
```javascript
function loadData() {
    return new Promise(function(resolve, reject) {
        $.ajax({
            url: '@Url.Action("ActionName", "ControllerName")',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                if (data.success && data.data) {
                    // Process data
                    resolve(data.data);
                } else {
                    reject(data.message);
                }
            },
            error: function (xhr, status, error) {
                reject(error);
            }
        });
    });
}

// Usage
loadData().then(function(data) {
    // Initialize components after data is loaded
    initializeComponents();
}).catch(function(error) {
    console.error('Error:', error);
});
```

### **2. Select2 Management Pattern:**
```javascript
function initializeSelect2() {
    // Initialize Select2 for filter dropdowns
    $('#filterElement').select2({
        placeholder: 'انتخاب...',
        allowClear: true,
        width: '100%'
    });
    
    // Initialize Select2 for modal dropdowns
    $('#modalElement').select2({
        placeholder: 'انتخاب...',
        allowClear: true,
        width: '100%',
        dropdownParent: $('#modalId')
    });
}

function refreshSelect2() {
    // Destroy existing Select2 instances
    if ($('#element').hasClass('select2-hidden-accessible')) {
        $('#element').select2('destroy');
    }
    
    // Re-initialize Select2
    initializeSelect2();
}
```

### **3. Form Validation Pattern:**
```html
<!-- ✅ همیشه این ساختار را استفاده کنید -->
<form id="formId" class="needs-validation" novalidate>
    <div class="form-group mb-3">
        <label for="fieldId" class="form-label">
            نام فیلد <span class="text-danger">*</span>
        </label>
        <input type="text" 
               id="fieldId" 
               name="FieldName" 
               class="form-control" 
               required />
        <div class="invalid-feedback">لطفاً این فیلد را پر کنید</div>
    </div>
</form>
```

```javascript
// ✅ همیشه این Validation را استفاده کنید
$('#formId').submit(function (e) {
    e.preventDefault();
    if (this.checkValidity()) {
        submitForm();
    } else {
        e.stopPropagation();
        $(this).addClass('was-validated');
    }
});
```

---

## 🎨 **استانداردهای طراحی:**

### **1. CSS Classes:**
```css
/* ✅ همیشه این کلاس‌ها را استفاده کنید */
.form-control, .form-select {
    border-radius: 10px;
    border: 2px solid #e9ecef;
    transition: all 0.3s ease;
}

.form-control:focus, .form-select:focus {
    border-color: #667eea;
    box-shadow: 0 0 0 0.2rem rgba(102, 126, 234, 0.25);
}

.btn-primary {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border: none;
    border-radius: 25px;
    padding: 0.75rem 2rem;
    font-weight: 600;
}
```

### **2. Modal Styling:**
```css
/* ✅ همیشه این استایل‌ها را برای Modal استفاده کنید */
.modal-content {
    border-radius: 20px;
    border: none;
    box-shadow: 0 20px 60px rgba(0,0,0,0.3);
}

.modal-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border-radius: 20px 20px 0 0;
    border: none;
}
```

---

## 📱 **Responsive Design:**

### **1. Bootstrap Grid:**
```html
<!-- ✅ همیشه این ساختار را استفاده کنید -->
<div class="row">
    <div class="col-md-6">
        <div class="form-group mb-3">
            <!-- Form field -->
        </div>
    </div>
    <div class="col-md-6">
        <div class="form-group mb-3">
            <!-- Form field -->
        </div>
    </div>
</div>
```

### **2. Table Responsive:**
```html
<!-- ✅ همیشه این ساختار را برای جداول استفاده کنید -->
<div class="table-responsive">
    <table id="tableId" class="table table-hover">
        <!-- Table content -->
    </table>
</div>
```

---

## 🔒 **امنیت و Validation:**

### **1. Anti-Forgery Token (اجباری):**
```html
<!-- ✅ همیشه این را در فرم‌های POST استفاده کنید -->
@Html.AntiForgeryToken()
```

```csharp
// ✅ در Controller همیشه از ValidateAntiForgeryToken استفاده کنید
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult ActionName(Model model)
{
    // Action logic
}
```

```javascript
// ✅ در JavaScript همیشه token را ارسال کنید
var formData = $('#formId').serialize(); // شامل token است
$.ajax({
    url: '@Url.Action("ActionName", "ControllerName")',
    type: 'POST',
    data: formData,
    success: function(data) { /* ... */ }
});
```

**⚠️ هشدار امنیتی:** عدم استفاده از Anti-Forgery Token منجر به حملات CSRF خواهد شد!

### **2. Input Validation:**
```html
<!-- ✅ همیشه این Validation ها را استفاده کنید -->
<input type="text" 
       required 
       minlength="3" 
       maxlength="50" 
       pattern="[A-Za-z0-9\s]+" />
```

### **3. ServiceResult Enhanced Validation:**
```csharp
// ✅ همیشه این Validation Pattern را استفاده کنید
public async Task<ServiceResult<Schedule>> CreateSchedule(Model model)
{
    var validationResult = new AdvancedValidationResult();
    
    if (string.IsNullOrEmpty(model.Title))
        validationResult.AddError("Title", "عنوان الزامی است.", "REQUIRED_TITLE");
    
    if (!validationResult.IsValid)
        return validationResult.ToAdvancedServiceResult<Schedule>(null, "خطا در اعتبارسنجی");
    
    // Main operation
    var schedule = await _repository.CreateAsync(model.ToEntity());
    return ServiceResult<Schedule>.Successful(schedule, "برنامه با موفقیت ایجاد شد.");
}
```

```csharp
// ✅ همیشه از WithErrorCode در Validator ها استفاده کنید
RuleFor(x => x.Title)
    .NotEmpty()
    .WithMessage("عنوان الزامی است.")
    .WithErrorCode("REQUIRED_TITLE");

RuleFor(x => x.Duration)
    .InclusiveBetween(15, 480)
    .WithMessage("مدت زمان باید بین 15 تا 480 دقیقه باشد.")
    .WithErrorCode("INVALID_DURATION");
```

---

## 📊 **Debug و Logging:**

### **1. Console Logging:**
```javascript
// ✅ همیشه این Logging را استفاده کنید
console.log('Function called with params:', params);
console.log('Data loaded successfully:', data.length);
console.error('Error occurred:', error);
```

### **2. Error Handling:**
```javascript
// ✅ همیشه این Error Handling را استفاده کنید
$.ajax({
    // ... ajax config
    success: function (data) {
        if (data.success && data.data) {
            // Success handling
        } else {
            console.error('Failed:', data.message);
            // User feedback
        }
    },
    error: function (xhr, status, error) {
        console.error('AJAX Error:', error);
        console.error('Status:', xhr.status);
        console.error('Response:', xhr.responseText);
    }
});
```

### **3. ServiceResult Enhanced Error Handling:**
```csharp
// ✅ همیشه این Error Handling Pattern را استفاده کنید
try
{
    var result = await _service.CreateAsync(model);
    
    if (result.Success)
    {
        _logger.LogInformation("عملیات موفق: {Message}", result.Message);
        return result;
    }
    
    // Log validation errors
    foreach (var error in result.ValidationErrors)
    {
        _logger.LogWarning("خطای اعتبارسنجی: {Field} - {Message} (کد: {Code})", 
            error.Field, error.ErrorMessage, error.Code);
    }
    
    return result;
}
catch (Exception ex)
{
    _logger.LogError(ex, "خطا در عملیات {OperationName}", "CreateAsync");
    return ServiceResult<Model>.Failed("خطا در عملیات", "OPERATION_ERROR");
}
```

```javascript
// ✅ همیشه این Error Handling را در JavaScript استفاده کنید
$.ajax({
    url: '@Url.Action("Create")',
    type: 'POST',
    data: formData,
    success: function (data) {
        if (data.success) {
            console.log('Success:', data.message);
            // Handle success
        } else {
            console.error('Validation failed:', data.message);
            
            // Display validation errors
            if (data.validationErrors && data.validationErrors.length > 0) {
                data.validationErrors.forEach(function(error) {
                    console.error('Field: {0}, Error: {1}, Code: {2}', 
                        error.field, error.errorMessage, error.code);
                });
            }
        }
    },
    error: function (xhr, status, error) {
        console.error('AJAX Error:', error);
        console.error('Status:', xhr.status);
        console.error('Response:', xhr.responseText);
    }
});
```

---

## 🚀 **نکات کلیدی:**

### **1. ترتیب Initialization:**
1. **Load Data** (AJAX)
2. **Update HTML** (Dropdowns, Tables)
3. **Initialize Components** (Select2, DatePicker)
4. **Setup Event Handlers**

### **2. Modal Handling:**
1. **همیشه از `dropdownParent` استفاده کنید**
2. **Event Handler `shown.bs.modal` اجباری است**
3. **Select2 را در Modal Refresh کنید**

### **3. Form Submission:**
1. **همیشه از `needs-validation` استفاده کنید**
2. **Client-side validation اجباری است**
3. **Server-side validation همیشه انجام دهید**

---

## 📝 **تاریخ و نسخه:**

- **تاریخ ایجاد:** 2025-01-01
- **نسخه:** 1.1
- **وضعیت:** فعال
- **آخرین به‌روزرسانی:** جلسه فعلی - اضافه شدن استاندارد اجباری Persian DatePicker
- **تغییرات نسخه 1.1:**
  - اضافه شدن استاندارد اجباری Persian DatePicker
  - راه‌حل کامل برای مشکل سال ۷۸۳
  - Event Delegation برای مدیریت تغییرات
  - Extension Methods برای تبدیل تاریخ
  - چک‌لیست کامل Persian DatePicker

---

## ✅ **تأیید:**

این قرارداد توسط تیم توسعه ClinicApp تأیید شده و باید در تمام پروژه‌ها رعایت شود.

**⚠️ توجه:** عدم رعایت این قرارداد منجر به خطاهای مکرر و مشکلات عملکردی خواهد شد.

---

## Integration with Details Display Standards Contract

### مرجع قرارداد استاندارد نمایش اطلاعات:
این قرارداد با `DETAILS_DISPLAY_STANDARDS.md` و `AI_COMPLIANCE_CONTRACT.md` (قوانین 40-48) یکپارچه است.

### الزامات یکپارچه برای فرم‌های جزئیات:
- **استفاده از فایل CSS مشترک**: `Content/css/details-standards.css`
- **رعایت ساختار کارتی**: Card با Header و Body
- **رنگ‌بندی یکپارچه**: پالت رنگ‌های تعریف شده
- **دسترس‌پذیری کامل**: فونت 14px، کنتراست مناسب
- **Responsive Design**: سازگاری با موبایل

### فایل‌های مرتبط:
- `CONTRACTS/DETAILS_DISPLAY_STANDARDS.md` - قرارداد کامل
- `TEMPLATES/DetailsPageTemplate.cshtml` - قالب استاندارد
- `Content/css/details-standards.css` - CSS مشترک

### نمونه پیاده‌سازی:
- `Areas/Admin/Views/DoctorServiceCategory/Details.cshtml` - نمونه کامل

---

## Integration with AI Compliance Contract

This contract works in conjunction with `CONTRACTS/AI_COMPLIANCE_CONTRACT.md` which defines mandatory rules for AI interactions with the ClinicApp project. All form development work must comply with both contracts.

**Key Integration Points for Forms**:
- All form changes must follow Atomic Changes Rule (AI_COMPLIANCE_CONTRACT Section 1)
- Pre-creation verification required for new form components (AI_COMPLIANCE_CONTRACT Section 2)
- No duplication of existing form patterns (AI_COMPLIANCE_CONTRACT Section 3)
- Mandatory documentation for all form changes (AI_COMPLIANCE_CONTRACT Section 4)
- Stop and approval process required for form modifications (AI_COMPLIANCE_CONTRACT Section 5)
- Security standards enforced (Anti-Forgery Token, Input Validation) (AI_COMPLIANCE_CONTRACT Section 6)
- Transparent output format required for form change proposals (AI_COMPLIANCE_CONTRACT Section 7)
- No auto-execution of form changes (AI_COMPLIANCE_CONTRACT Section 8)
- Project scope compliance for form features (AI_COMPLIANCE_CONTRACT Section 9)
- Mandatory compliance with all AI interaction rules (AI_COMPLIANCE_CONTRACT Section 10)

**Reference**: See `CONTRACTS/AI_COMPLIANCE_CONTRACT.md` for complete AI interaction guidelines.

---

## 🏥 **استانداردهای فرم‌های رسمی محیط درمانی:**

### **1. اصول کلی طراحی فرم‌ها:**
```markdown
### اصول اجباری:
- تمامی فرم‌ها باید رسمی و ساده طراحی شوند
- استفاده از رنگ‌های اصلی محدود به آبی تیره (primary) و خاکستری (neutral)
- هیچ‌گونه المان فانتزی (انیمیشن غیرضروری، آیکون‌های کارتونی، پس‌زمینه‌های رنگارنگ) مجاز نیست
- چینش فرم‌ها باید ساده، خوانا و با ساختار شبکه‌ای (grid-based) باشد
```

### **2. المان‌های ممنوع در فرم‌ها:**
```css
/* ❌ ممنوع - رنگ‌های تند */
background-color: #FF0000; /* قرمز خام */
background-color: #FFA500; /* نارنجی خام */
background-color: #800080; /* بنفش غیررسمی */

/* ❌ ممنوع - انیمیشن‌های غیرضروری */
animation: bounce 2s infinite;
transform: rotate(360deg);
transition: all 2s ease-in-out;

/* ❌ ممنوع - آیکون‌های غیررسمی */
.fa-smile-o, .fa-heart, .fa-star; /* آیکون‌های فانتزی */

/* ✅ مجاز - فقط برای هشدار ضروری */
.alert-danger { background-color: #dc3545; } /* قرمز Bootstrap */
```

### **3. دسترس‌پذیری (Accessibility) اجباری:**
```css
/* ✅ اجباری - فونت حداقل 14px */
.form-control, .form-label {
    font-size: 14px; /* حداقل اندازه برای افراد مسن */
    font-family: "Vazirmatn", "Tahoma", sans-serif;
}

/* ✅ اجباری - کنتراست مناسب */
.form-control {
    color: #212529; /* کنتراست بالا */
    background-color: #ffffff;
    border: 2px solid #dee2e6;
}

/* ✅ اجباری - Tab Navigation */
.form-control:focus {
    outline: 2px solid #0d6efd;
    outline-offset: 2px;
}
```

### **4. چک‌لیست استاندارد فرم‌های Razor:**
```html
<!-- ✅ بخش ۱: ساختار کلی -->
<div class="container-fluid">
    <div class="row">
        <div class="col-12">
            <h2 class="text-center mb-4">عنوان صفحه واضح و رسمی</h2>
            <p class="text-center text-muted mb-4">زیرعنوان هدف فرم در یک جمله</p>
            
            <!-- ✅ بخش ۲: طراحی بصری -->
            <div class="card">
                <div class="card-body">
                    <form method="post" class="needs-validation" novalidate>
                        @Html.AntiForgeryToken()
                        
                        <!-- ✅ بخش ۳: فیلدها و ورودی‌ها -->
                        <div class="row">
                            <div class="col-md-6">
                                <div class="form-group mb-3">
                                    <label for="patientName" class="form-label">
                                        نام بیمار <span class="text-danger">*</span>
                                    </label>
                                    <input type="text" id="patientName" name="PatientName" 
                                           class="form-control" required 
                                           placeholder="نام کامل بیمار را وارد کنید">
                                    <div class="invalid-feedback">
                                        لطفاً نام بیمار را وارد کنید
                                    </div>
                                </div>
                            </div>
                            
                            <div class="col-md-6">
                                <div class="form-group mb-3">
                                    <label for="appointmentDate" class="form-label">
                                        تاریخ نوبت <span class="text-danger">*</span>
                                    </label>
                                    <input type="text" id="appointmentDate" name="AppointmentDate" 
                                           class="form-control persian-date" required 
                                           placeholder="انتخاب تاریخ نوبت">
                                    <div class="invalid-feedback">
                                        لطفاً تاریخ نوبت را انتخاب کنید
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <!-- ✅ بخش ۵: دکمه‌ها -->
                        <div class="row mt-4">
                            <div class="col-12 text-center">
                                <button type="submit" class="btn btn-success me-2">
                                    <i class="fa fa-save"></i> ثبت نوبت
                                </button>
                                <a href="@Url.Action("Index")" class="btn btn-secondary">
                                    <i class="fa fa-arrow-right"></i> بازگشت
                                </a>
                            </div>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</div>
```

### **5. استانداردهای رنگ‌بندی فرم‌ها:**
```css
/* ✅ مجاز - رنگ‌های اصلی */
:root {
    --primary-blue: #0d6efd;      /* آبی اصلی */
    --success-green: #198754;     /* سبز ثبت موفق */
    --neutral-gray: #6c757d;      /* خاکستری بازگشت */
    --danger-red: #dc3545;        /* قرمز هشدار ضروری */
    --warning-orange: #fd7e14;    /* نارنجی هشدار */
}

/* ✅ استفاده صحیح از رنگ‌ها */
.btn-primary { background-color: var(--primary-blue); }
.btn-success { background-color: var(--success-green); }
.btn-secondary { background-color: var(--neutral-gray); }
.btn-danger { background-color: var(--danger-red); }
.btn-warning { background-color: var(--warning-orange); }
```

### **6. استانداردهای اعتبارسنجی:**
```csharp
// ✅ پیام‌های خطای رسمی و فارسی
RuleFor(x => x.PatientName)
    .NotEmpty()
    .WithMessage("لطفاً نام بیمار را وارد کنید")
    .WithErrorCode("REQUIRED_PATIENT_NAME");

RuleFor(x => x.AppointmentDate)
    .NotEmpty()
    .WithMessage("لطفاً تاریخ نوبت را انتخاب کنید")
    .WithErrorCode("REQUIRED_APPOINTMENT_DATE");
```

### **7. چک‌لیست کامل فرم‌های درمانی:**
```markdown
### ✅ بخش ۱: ساختار کلی
- [ ] عنوان صفحه واضح، رسمی و فارسی است
- [ ] زیرعنوان هدف فرم را در یک جمله توضیح می‌دهد
- [ ] فرم فقط شامل فیلدهای ضروری و مرتبط است
- [ ] ناوبری (بازگشت، ثبت) ساده و در پایین فرم قرار دارد

### ✅ بخش ۲: طراحی بصری
- [ ] استفاده از رنگ‌ها محدود به: آبی (Primary)، سبز (ثبت موفق)، خاکستری (بازگشت)
- [ ] پس‌زمینه سفید و ساده بدون تصاویر یا المان‌های اضافی
- [ ] فونت رسمی: Vazirmatn یا Tahoma، اندازه حداقل ۱۴px
- [ ] چینش منظم با Grid یا Bootstrap (دو ستونی یا تک ستونی)

### ✅ بخش ۳: فیلدها و ورودی‌ها
- [ ] هر فیلد Label فارسی و رسمی دارد
- [ ] فیلدهای اجباری با * یا پیام هشدار مشخص شده‌اند
- [ ] از ورودی‌های مناسب استفاده شده (TextBox، DropDown، DatePicker)
- [ ] تاریخ‌ها فقط با Persian DatePicker پیاده‌سازی شده‌اند
- [ ] هیچ placeholder غیررسمی یا فانتزی استفاده نشده

### ✅ بخش ۳.۱: Persian DatePicker (چک‌لیست اجباری)
- [ ] فیلدهای تاریخ دارای `class="persian-datepicker"` هستند
- [ ] مقدار اولیه خالی تنظیم شده (`value=""`)
- [ ] فیلدهای شمسی و میلادی جداگانه در ViewModel تعریف شده‌اند
- [ ] مقدار اولیه خالی برای فیلدهای شمسی در Controller تنظیم شده
- [ ] از onSelect callback استفاده نشده (باعث خطای JavaScript می‌شود)
- [ ] Event Delegation برای تغییرات پیاده‌سازی شده
- [ ] تابع convertPersianDateToGregorian پیاده‌سازی شده
- [ ] Extension Methods ToDateTimeFromPersian موجود است
- [ ] Validation فرمت تاریخ فارسی پیاده‌سازی شده
- [ ] مقادیر مشکل‌ساز (۷۸۳) پاک می‌شوند
- [ ] تبدیل خودکار تاریخ شمسی به میلادی کار می‌کند

### ✅ بخش ۴: اعتبارسنجی (Validation)
- [ ] همه فیلدهای مهم دارای Validation سمت سرور و کلاینت هستند
- [ ] پیام خطا رسمی و فارسی: «لطفاً نام بیمار را وارد کنید»
- [ ] هیچ متن غیررسمی یا فانتزی در پیام خطا وجود ندارد

### ✅ بخش ۵: دکمه‌ها (Actions)
- [ ] فقط دکمه‌های ضروری وجود دارند (ثبت / بازگشت)
- [ ] رنگ سبز فقط برای ثبت/تایید استفاده شده
- [ ] رنگ خاکستری فقط برای بازگشت/لغو استفاده شده
- [ ] دکمه‌ها در پایین و وسط یا راست‌چین فرم قرار دارند

### ✅ بخش ۶: دسترس‌پذیری (Accessibility)
- [ ] فرم با Tab قابل پیمایش کامل است
- [ ] همه Labelها به Input مربوطه متصل هستند
- [ ] کنتراست رنگ‌ها مناسب (خوانا برای همه سنین)
- [ ] پیام‌های خطا و موفقیت با متن و رنگ قابل فهم نمایش داده می‌شوند

### ✅ بخش ۷: المان‌های ممنوع
- [ ] هیچ انیمیشن غیرضروری وجود ندارد
- [ ] هیچ ایموجی یا آیکون غیررسمی استفاده نشده
- [ ] هیچ رنگ تند یا پس‌زمینه دکوراتیو وجود ندارد
- [ ] هیچ فیلد اضافی یا غیرمرتبط با فرآیند درمانی وجود ندارد
```

---

## Integration with Form Standards Contract

این قرارداد با قرارداد استاندارد فرم‌های ایجاد و ویرایش (`form-standards.css`) و قرارداد تبعیت هوش مصنوعی (`AI_COMPLIANCE_CONTRACT.md`) یکپارچه است.
