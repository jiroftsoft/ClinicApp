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

### **1. Persian DatePicker:**
```html
<!-- ✅ همیشه این ساختار را استفاده کنید -->
<input type="text" 
       class="form-control persian-datepicker" 
       placeholder="انتخاب تاریخ" />
```

```javascript
// ✅ همیشه این تنظیمات را استفاده کنید
$('.persian-datepicker').persianDatepicker({
    format: 'YYYY/MM/DD',
    initialValue: false,
    autoClose: true
});
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

### **1. Loading Data Pattern:**
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
- **نسخه:** 1.0
- **وضعیت:** فعال
- **آخرین به‌روزرسانی:** 2025-01-01

---

## ✅ **تأیید:**

این قرارداد توسط تیم توسعه ClinicApp تأیید شده و باید در تمام پروژه‌ها رعایت شود.

**⚠️ توجه:** عدم رعایت این قرارداد منجر به خطاهای مکرر و مشکلات عملکردی خواهد شد.
