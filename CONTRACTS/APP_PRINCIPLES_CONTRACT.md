# Design Principles Contract for Medical Environment Development

## Core Principles Contract

### 1. Strongly-Typed ViewModels (Mandatory)
**Principle**: Always use strongly-typed ViewModels instead of ViewBag for complex data structures.

**Implementation Rules**:
- ✅ **DO**: Use `List<System.Web.Mvc.SelectListItem>` for dropdown data
- ✅ **DO**: Define explicit properties in ViewModels for all data needed by views
- ✅ **DO**: Initialize ViewModel properties with default values to prevent null reference exceptions
- ❌ **DON'T**: Use `ViewBag` for complex data structures
- ❌ **DON'T**: Use anonymous types for dropdown data

**Example Implementation**:
```csharp
// ✅ Correct Approach
public class DoctorHistorySearchViewModel
{
    public List<System.Web.Mvc.SelectListItem> Doctors { get; set; } = new List<System.Web.Mvc.SelectListItem>();
    public List<System.Web.Mvc.SelectListItem> Departments { get; set; } = new List<System.Web.Mvc.SelectListItem>();
    public DoctorHistoryFilterViewModel Filter { get; set; } = new DoctorHistoryFilterViewModel();
}

// ❌ Avoid This
ViewBag.Doctors = doctorsResult.Data.Items.Select(d => new { Id = d.Id, Name = d.FullName });
```

### 2. Factory Method Pattern for Entity-ViewModel Conversion (CRITICAL - NO AUTOMAPPER)
**Principle**: Use Factory Method Pattern for converting Entity objects to ViewModels. **NEVER use AutoMapper** in this project.

### 3. Anti-Forgery Token Security (MANDATORY)
**Principle**: **ALWAYS use Anti-Forgery Token protection for ALL POST actions** to prevent CSRF attacks.

**Implementation Rules**:
- ✅ **DO**: Add `@Html.AntiForgeryToken()` to ALL forms
- ✅ **DO**: Use `[ValidateAntiForgeryToken]` attribute on ALL POST actions
- ✅ **DO**: Ensure token is included in AJAX requests
- ❌ **DON'T**: Create POST actions without Anti-Forgery protection
- ❌ **DON'T**: Forget to include token in form submissions

**Example Implementation**:
```csharp
// ✅ Controller Action (MANDATORY)
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(DoctorScheduleViewModel model)
{
    // Action logic
}

// ✅ View Form (MANDATORY)
<form id="createForm" method="post">
    @Html.AntiForgeryToken()  <!-- ALWAYS include this -->
    <!-- Form fields -->
</form>

// ✅ JavaScript AJAX (MANDATORY)
$.ajax({
    url: '@Url.Action("Create")',
    type: 'POST',
    data: $('#createForm').serialize(), // Includes token automatically
    success: function(data) { /* ... */ }
});

// ✅ Manual Token Inclusion for Custom AJAX
$.ajax({
    url: '@Url.Action("Create")',
    type: 'POST',
    data: { 
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
        // Other data
    },
    success: function(data) { /* ... */ }
});
```

**Security Warning**: Failure to implement Anti-Forgery protection will result in CSRF vulnerabilities!

### 4. ServiceResult Enhanced Usage (MANDATORY)
**Principle**: **ALWAYS use ServiceResult Enhanced classes for service operations and error handling** to ensure consistent error management across the application.

**Implementation Rules**:
- ✅ **DO**: Use `ServiceResult` or `ServiceResult<T>` for all service method returns
- ✅ **DO**: Use `ValidationError` with error codes for validation failures
- ✅ **DO**: Use `AdvancedValidationResult` for complex validation scenarios
- ✅ **DO**: Use Factory Methods (`ServiceResult.Successful`, `ServiceResult.Failed`)
- ✅ **DO**: Include error codes in all validation rules using `WithErrorCode`
- ❌ **DON'T**: Use `new ServiceResult<T>()` constructor directly
- ❌ **DON'T**: Use `ValidationResult` name (conflicts with System.ComponentModel.DataAnnotations)
- ❌ **DON'T**: Return simple strings or exceptions from services

**Example Implementation**:
```csharp
// ✅ Correct Approach - Service Method
public async Task<ServiceResult<DoctorSchedule>> SetDoctorScheduleAsync(DoctorScheduleViewModel model)
{
    try
    {
        // Validation using AdvancedValidationResult
        var validationResult = new AdvancedValidationResult();
        
        if (model.DoctorId <= 0)
            validationResult.AddError("DoctorId", "شناسه پزشک الزامی است.", "REQUIRED_DOCTOR_ID");
        
        if (!validationResult.IsValid)
            return validationResult.ToAdvancedServiceResult<DoctorSchedule>(null, "خطا در اعتبارسنجی");
        
        // Main operation
        var schedule = await _repository.CreateAsync(model.ToEntity());
        
        return ServiceResult<DoctorSchedule>.Successful(schedule, "برنامه کاری با موفقیت ایجاد شد.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطا در ایجاد برنامه کاری");
        return ServiceResult<DoctorSchedule>.Failed("خطا در ایجاد برنامه کاری", "SCHEDULE_CREATION_ERROR");
    }
}

// ✅ Correct Approach - Validation Rule
RuleFor(x => x.DoctorId)
    .GreaterThan(0)
    .WithMessage("شناسه پزشک نامعتبر است.")
    .WithErrorCode("INVALID_DOCTOR_ID");

// ✅ Correct Approach - Controller Usage
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> AssignSchedule(DoctorScheduleViewModel model)
{
    var result = await _doctorScheduleService.SetDoctorScheduleAsync(model);
    
    if (result.Success)
    {
        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("Index");
    }
    
    // Display validation errors
    foreach (var error in result.ValidationErrors)
    {
        ModelState.AddModelError(error.Field, error.ErrorMessage);
    }
    
    return View("Schedule", model);
}
```

**Reference**: See `CONTRACTS/ServiceResult_Enhanced_Contract.md` for complete usage guidelines.

**Implementation Rules**:
- ✅ **DO**: Implement static `FromEntity()` methods in ViewModels
- ✅ **DO**: Use explicit property mapping for full control over conversions
- ✅ **DO**: Handle complex relationships and nested objects manually
- ✅ **DO**: Ensure type safety and compile-time checking
- ❌ **DON'T**: Use AutoMapper or any external mapping libraries
- ❌ **DON'T**: Use dynamic mapping or reflection-based conversion
- ❌ **DON'T**: Import AutoMapper namespaces

**Example Implementation**:
```csharp
// ✅ Correct Approach - Factory Method Pattern
public class DoctorCreateEditViewModel
{
    public int DoctorId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public List<int> SpecializationIds { get; set; } = new List<int>();
    public List<string> SpecializationNames { get; set; } = new List<string>();

    /// <summary>
    /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
    /// </summary>
    public static DoctorCreateEditViewModel FromEntity(Doctor doctor)
    {
        if (doctor == null) return null;

        return new DoctorCreateEditViewModel
        {
            DoctorId = doctor.DoctorId,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            FullName = $"{doctor.FirstName} {doctor.LastName}",
            SpecializationIds = doctor.Specializations?.Select(s => s.SpecializationId).ToList() ?? new List<int>(),
            SpecializationNames = doctor.Specializations?.Select(s => s.Name).ToList() ?? new List<string>()
        };
    }

    /// <summary>
    /// ✅ (Factory Method) یک Entity جدید از روی ViewModel می‌سازد.
    /// </summary>
    public Doctor ToEntity()
    {
        return new Doctor
        {
            DoctorId = this.DoctorId,
            FirstName = this.FirstName,
            LastName = this.LastName
            // Specializations will be handled separately by the service layer
        };
    }
}

// ❌ NEVER DO THIS - No AutoMapper
// using AutoMapper;
// cfg.CreateMap<Doctor, DoctorCreateEditViewModel>();
```

**Controller Usage**:
```csharp
// ✅ Correct Approach in Controller
public async Task<ActionResult> Edit(int id)
{
    var result = await _doctorCrudService.GetDoctorForEditAsync(id);
    if (result.Success && result.Data != null)
    {
        // Use Factory Method instead of AutoMapper
        var viewModel = DoctorCreateEditViewModel.FromEntity(result.Data);
        return View(viewModel);
    }
    return RedirectToAction("Index");
}

// ❌ NEVER DO THIS
// var viewModel = _mapper.Map<DoctorCreateEditViewModel>(result.Data);
```

### 3. Avoid Dynamic Types (Critical)
**Principle**: Never use `dynamic` types in medical environment applications.

**Implementation Rules**:
- ✅ **DO**: Use explicit type definitions
- ✅ **DO**: Use nullable types (`DateTime?`, `int?`) for optional fields
- ✅ **DO**: Use proper type casting when necessary
- ❌ **DON'T**: Use `dynamic` keyword
- ❌ **DON'T**: Use `object` types for complex data

**Example Implementation**:
```csharp
// ✅ Correct Approach
public DateTime? StartDate { get; set; }
public DateTime? EndDate { get; set; }
public int? DoctorId { get; set; }

// ❌ Avoid This
public dynamic Filter { get; set; }
```

### 3. Robust Error-Resistant Design (Medical Environment Critical)
**Principle**: Design systems that are 100% resistant to runtime errors in medical environments.

**Implementation Rules**:
- ✅ **DO**: Always initialize ViewModel properties with default values
- ✅ **DO**: Use null-conditional operators (`?.`) in views
- ✅ **DO**: Implement defensive programming in all views
- ✅ **DO**: Validate all inputs before processing
- ✅ **DO**: Handle all possible null scenarios
- ❌ **DON'T**: Assume data will always be present
- ❌ **DON'T**: Skip null checks in critical paths

**Example Implementation**:
```csharp
// ✅ Correct Approach - Controller
public DoctorHistorySearchViewModel CreateEmptySearchViewModel(List<System.Web.Mvc.SelectListItem> doctors, List<System.Web.Mvc.SelectListItem> departments)
{
    return new DoctorHistorySearchViewModel
    {
        Doctors = doctors ?? new List<System.Web.Mvc.SelectListItem>(),
        Departments = departments ?? new List<System.Web.Mvc.SelectListItem>(),
        Filter = new DoctorHistoryFilterViewModel(),
        SearchResults = new List<AssignmentHistoryViewModel>()
    };
}

// ✅ Correct Approach - View
@if (Model?.Doctors != null)
{
    foreach (var doctor in Model.Doctors)
    {
        var selected = Model?.Filter?.DoctorId?.ToString() == doctor.Value ? "selected" : "";
        <option value="@doctor.Value" @selected>@doctor.Text</option>
    }
}
```

### 4. Agile and Professional Development (Operational Excellence)
**Principle**: Maintain high standards of code quality and maintainability.

**Implementation Rules**:
- ✅ **DO**: Follow SOLID principles strictly
- ✅ **DO**: Implement proper separation of concerns
- ✅ **DO**: Use async/await patterns consistently
- ✅ **DO**: Implement proper error handling and logging
- ✅ **DO**: Write self-documenting code with clear naming
- ✅ **DO**: Use dependency injection for all services
- ❌ **DON'T**: Create monolithic controllers
- ❌ **DON'T**: Mix business logic with presentation logic

### 5. Chart.js Integration Pattern (Critical for Data Visualization)
**Principle**: Ensure Chart.js is properly loaded and available before initializing charts.

**Implementation Rules**:
- ✅ **DO**: Include Chart.js in the plugins bundle (`~/bundles/plugins`)
- ✅ **DO**: Add defensive checks before initializing charts
- ✅ **DO**: Use local Chart.js files instead of CDN for reliability
- ✅ **DO**: Handle cases where Chart.js fails to load
- ❌ **DON'T**: Load Chart.js from CDN in individual views
- ❌ **DON'T**: Assume Chart.js will always be available

**Example Implementation**:
```javascript
// ✅ Correct Approach
$(document).ready(function() {
    // Ensure Chart.js is loaded before initializing charts
    if (typeof Chart === 'undefined') {
        console.error('Chart.js is not loaded. Please check the bundle configuration.');
        return;
    }
    
    // Initialize charts only if Chart.js is available
    const ctx = document.getElementById('chartCanvas').getContext('2d');
    new Chart(ctx, {
        // chart configuration
    });
});
```

**Bundle Configuration**:
```csharp
// ✅ Add to BundleConfig.cs
bundles.Add(new ScriptBundle("~/bundles/plugins").Include(
    // ... other plugins
    "~/Content/plugins/chartjs/chart.min.js"
));
```

### 6. Persian DatePicker Standard Implementation (CRITICAL - MEDICAL ENVIRONMENT)
**Principle**: Use standardized Persian DatePicker implementation for all date inputs in the medical environment.

**Implementation Rules**:
- ✅ **DO**: Use `class="persian-date"` for all date input fields
- ✅ **DO**: Use `type="text"` instead of `type="date"` for Persian DatePicker
- ✅ **DO**: Include proper placeholder text in Persian
- ✅ **DO**: Set default values using JavaScript for current date
- ✅ **DO**: Use consistent Persian DatePicker configuration across all forms
- ❌ **DON'T**: Use HTML5 date inputs (`type="date"`) in Persian environment
- ❌ **DON'T**: Mix different date picker implementations
- ❌ **DON'T**: Use English date formats in Persian interface

**Standard HTML Implementation**:
```html
<!-- ✅ Correct Approach - Persian DatePicker -->
<div class="form-group">
    <label for="grantedDate">تاریخ اعطا</label>
    <input type="text" id="grantedDate" name="GrantedDate" class="form-control persian-date" 
           placeholder="انتخاب تاریخ اعطا" />
</div>

<div class="form-group">
    <label for="expiryDate">تاریخ انقضا (اختیاری)</label>
    <input type="text" id="expiryDate" name="ExpiryDate" class="form-control persian-date" 
           placeholder="انتخاب تاریخ انقضا" />
</div>

<!-- ❌ Avoid This - HTML5 Date Input -->
<input type="date" id="grantedDate" name="GrantedDate" class="form-control" />
```

**Standard JavaScript Configuration**:
```javascript
// ✅ Standard Persian DatePicker Configuration
$this.persianDatepicker({
    format: 'YYYY/MM/DD',
    initialValue: false,
    autoClose: true,
    observer: true,
    persianDigit: true,
    calendar: {
        persian: {
            locale: 'fa',
            leapYearMode: 'astronomical',
            showHint: true
        }
    },
    toolbox: {
        todayBtn: { enabled: true, text: { fa: 'امروز' } },
        clearBtn: { enabled: true, text: { fa: 'پاک کردن' } }
    },
    onSelect: function(unix) {
        var date = new Date(unix);
        var persianDate = date.getFullYear() + '/' +
            String(date.getMonth() + 1).padStart(2, '0') + '/' +
            String(date.getDate()).padStart(2, '0');
        $this.val(persianDate);
        $this.trigger('change');
    }
});
```

**Default Date Setting Pattern**:
```javascript
// ✅ Set current date as default
var today = new Date();
var persianToday = today.getFullYear() + '/' + 
    String(today.getMonth() + 1).padStart(2, '0') + '/' + 
    String(today.getDate()).padStart(2, '0');
$('#grantedDate').val(persianToday);
```

**Bundle Configuration**:
```csharp
// ✅ Separate Persian DatePicker Bundle
bundles.Add(new ScriptBundle("~/bundles/persian-datepicker").Include(
    "~/Scripts/persian-date.min.js",
    "~/Scripts/persian-datepicker.min.js"
));
```

**Layout Integration**:
```html
<!-- ✅ Load Persian DatePicker Bundle -->
@Scripts.Render("~/bundles/persian-datepicker")
```

**Common Use Cases in Medical Environment**:
```html
<!-- Patient Registration Date -->
<div class="form-group">
    <label for="registrationDate">تاریخ ثبت نام بیمار</label>
    <input type="text" id="registrationDate" name="RegistrationDate" class="form-control persian-date" 
           placeholder="انتخاب تاریخ ثبت نام" />
</div>

<!-- Appointment Date -->
<div class="form-group">
    <label for="appointmentDate">تاریخ نوبت</label>
    <input type="text" id="appointmentDate" name="AppointmentDate" class="form-control persian-date" 
           placeholder="انتخاب تاریخ نوبت" />
</div>

<!-- Medical Certificate Expiry -->
<div class="form-group">
    <label for="certificateExpiry">تاریخ انقضای گواهی پزشکی</label>
    <input type="text" id="certificateExpiry" name="CertificateExpiry" class="form-control persian-date" 
           placeholder="انتخاب تاریخ انقضا" />
</div>

<!-- Birth Date -->
<div class="form-group">
    <label for="birthDate">تاریخ تولد</label>
    <input type="text" id="birthDate" name="BirthDate" class="form-control persian-date" 
           placeholder="انتخاب تاریخ تولد" />
</div>
```

**JavaScript Initialization Pattern**:
```javascript
$(document).ready(function() {
    // Set default dates for new records
    var today = new Date();
    var persianToday = today.getFullYear() + '/' + 
        String(today.getMonth() + 1).padStart(2, '0') + '/' + 
        String(today.getDate()).padStart(2, '0');
    
    // Set default for registration date
    $('#registrationDate').val(persianToday);
    
    // Set default for appointment date (tomorrow)
    var tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);
    var persianTomorrow = tomorrow.getFullYear() + '/' + 
        String(tomorrow.getMonth() + 1).padStart(2, '0') + '/' + 
        String(tomorrow.getDate()).padStart(2, '0');
    $('#appointmentDate').val(persianTomorrow);
});
```

**Migration Guide for Existing Forms**:
```html
<!-- ❌ Before - HTML5 Date Input -->
<input type="date" id="appointmentDate" name="AppointmentDate" class="form-control" />

<!-- ✅ After - Persian DatePicker -->
<input type="text" id="appointmentDate" name="AppointmentDate" class="form-control persian-date" 
       placeholder="انتخاب تاریخ نوبت" />
```

**Controller Date Handling**:
```csharp
// ✅ Handle Persian date strings in controllers using PersianDateHelper
[HttpPost]
public async Task<ActionResult> CreateAppointment(AppointmentViewModel model)
{
    try
    {
        // Convert Persian date string to DateTime for database storage
        if (!string.IsNullOrEmpty(model.AppointmentDateString))
        {
            var appointmentDate = PersianDateHelper.ConvertPersianToDateTime(model.AppointmentDateString);
            if (appointmentDate.HasValue)
            {
                model.AppointmentDate = appointmentDate.Value;
                _logger.Information("تاریخ نوبت تبدیل شد: {PersianDate} -> {GregorianDate}", 
                    model.AppointmentDateString, appointmentDate.Value.ToString("yyyy/MM/dd"));
            }
            else
            {
                _logger.Warning("فرمت تاریخ نوبت نامعتبر: {PersianDate}, کاربر: {UserId}", 
                    model.AppointmentDateString, _currentUserService.UserId);
                return Json(new { success = false, message = "فرمت تاریخ نوبت نامعتبر است." });
            }
        }
        
        // Process the appointment...
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Error parsing Persian date: {DateString}", model.AppointmentDateString);
        ModelState.AddModelError("AppointmentDateString", "فرمت تاریخ نامعتبر است");
    }
}
```

**ViewModel Date Properties**:
```csharp
// ✅ ViewModel with both DateTime and string properties
public class AppointmentViewModel
{
    // For database storage (Gregorian)
    public DateTime? AppointmentDate { get; set; }
    
    // For UI display (Persian)
    public string AppointmentDateShamsi { get; set; }
    
    // For user input (Persian string)
    public string AppointmentDateString { get; set; }
}
```

**PersianDateHelper Usage**:
```csharp
// ✅ Convert Persian to Gregorian for database
var gregorianDate = PersianDateHelper.ConvertPersianToDateTime("1403/09/15");

// ✅ Convert Gregorian to Persian for display
var persianDate = PersianDateHelper.ConvertDateTimeToPersian(DateTime.Now);

// ✅ Get current Persian date
var todayPersian = PersianDateHelper.GetCurrentPersianDate();

// ✅ Validate Persian date format
var isValid = PersianDateHelper.IsValidPersianDate("1403/09/15");
```

**Best Practices for Persian Date Handling**:
```csharp
// ✅ Always validate Persian dates before conversion
if (PersianDateHelper.IsValidPersianDate(model.DateString))
{
    var dateTime = PersianDateHelper.ConvertPersianToDateTime(model.DateString);
    // Process the date...
}

// ✅ Use proper logging for date conversions
_logger.Information("تاریخ تبدیل شد: {PersianDate} -> {GregorianDate}", 
    persianDateString, gregorianDate.ToString("yyyy/MM/dd"));

// ✅ Handle null/empty dates gracefully
var dateTime = PersianDateHelper.ConvertPersianToDateTime(model.DateString ?? string.Empty);

// ✅ Set default Persian dates in JavaScript
var todayPersian = PersianDateHelper.GetCurrentPersianDate();
$('#appointmentDate').val(todayPersian);
```

**Common Pitfalls to Avoid**:
```csharp
// ❌ Don't mix Persian and Gregorian date formats
var wrongDate = "2024/01/15"; // This is Gregorian, not Persian!

// ❌ Don't use DateTime.Parse for Persian dates
var wrongConversion = DateTime.Parse("1403/09/15"); // Will fail!

// ❌ Don't forget to validate before conversion
var unsafeConversion = PersianDateHelper.ConvertPersianToDateTime(userInput); // Could be null!

// ✅ Always validate first
if (PersianDateHelper.IsValidPersianDate(userInput))
{
    var safeConversion = PersianDateHelper.ConvertPersianToDateTime(userInput);
}
```

### 7. Efficient and Practical Solutions (Performance Critical)
**Principle**: Optimize for performance and practical usability in medical environments.

**Implementation Rules**:
- ✅ **DO**: Use efficient database queries with proper indexing
- ✅ **DO**: Implement pagination for large datasets
- ✅ **DO**: Use caching where appropriate
- ✅ **DO**: Optimize view rendering performance
- ✅ **DO**: Implement proper loading states and user feedback
- ❌ **DON'T**: Load unnecessary data
- ❌ **DON'T**: Block UI threads with long operations

## Error Prevention Patterns

### 1. CSRF Protection Pattern (Critical for Medical Environment)
**Issue**: Missing anti-forgery tokens causing `HttpAntiForgeryException` in POST actions.

**Solution**: Always include `@Html.AntiForgeryToken()` in views that contain forms or AJAX calls to POST actions.

**Implementation**:
```html
@model YourViewModel
@{
    ViewBag.Title = "Your Page Title";
    Layout = "~/Areas/Admin/Views/Shared/_AdminLayout.cshtml";
}

@Html.AntiForgeryToken()

<!-- Rest of your view content -->
```

**JavaScript Usage**:
```javascript
$.ajax({
    url: '@Url.Action("Delete")',
    type: 'POST',
    data: {
        id: id,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    },
    success: function(response) {
        // Handle response
    }
});
```

**Controller Action**:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<JsonResult> Delete(int id)
{
    // Action implementation
}
```

### 3. ViewModel Initialization Pattern
```csharp
public class RobustViewModel
{
    // Always initialize collections and complex objects
    public List<SelectListItem> Options { get; set; } = new List<SelectListItem>();
    public FilterViewModel Filter { get; set; } = new FilterViewModel();
    public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
    
    // Use nullable types for optional fields
    public DateTime? StartDate { get; set; }
    public int? SelectedId { get; set; }
}
```

### 5. Controller Defensive Pattern
```csharp
public async Task<ActionResult> Index()
{
    try
    {
        var (doctors, departments) = await LoadSearchDataAsync();
        
        var viewModel = new DoctorHistoryIndexViewModel
        {
            Doctors = doctors ?? new List<SelectListItem>(),
            Departments = departments ?? new List<SelectListItem>(),
            Filter = new DoctorHistoryFilterViewModel(),
            Histories = new List<AssignmentHistoryViewModel>()
        };
        
        return View(viewModel);
    }
    catch (Exception ex)
    {
        // Log error and return safe default
        return View(new DoctorHistoryIndexViewModel
        {
            Doctors = new List<SelectListItem>(),
            Departments = new List<SelectListItem>(),
            Filter = new DoctorHistoryFilterViewModel(),
            Histories = new List<AssignmentHistoryViewModel>(),
            ErrorMessage = "خطا در بارگذاری اطلاعات"
        });
    }
}
```

### 4. View Defensive Pattern
```html
@model DoctorHistorySearchViewModel

@if (Model?.Doctors != null && Model.Doctors.Any())
{
    <select id="doctorId" name="doctorId" class="form-select">
        <option value="">همه پزشکان</option>
        @foreach (var doctor in Model.Doctors)
        {
            var selected = Model?.Filter?.DoctorId?.ToString() == doctor.Value ? "selected" : "";
            <option value="@doctor.Value" @selected>@doctor.Text</option>
        }
    </select>
}
else
{
    <select id="doctorId" name="doctorId" class="form-select">
        <option value="">اطلاعات در دسترس نیست</option>
    </select>
}
```

## Medical Environment Specific Requirements

### 1. Data Integrity
- All medical data must be validated before processing
- Implement audit trails for all data changes
- Use soft delete patterns for data preservation
- Implement proper authorization checks

### 2. User Experience
- Provide clear error messages in Persian
- Implement loading states for all operations
- Use intuitive UI patterns familiar to medical staff
- Ensure accessibility compliance

### 3. Performance
- Optimize database queries for medical data volumes
- Implement proper caching strategies
- Use async operations for all I/O operations
- Monitor and log performance metrics

## Compliance Checklist

Before deploying any module to production:

- [ ] All ViewModels are strongly-typed
- [ ] No ViewBag usage for complex data
- [ ] No dynamic types used
- [ ] All ViewModel properties are properly initialized
- [ ] All views use null-conditional operators
- [ ] **CSRF protection implemented** (`@Html.AntiForgeryToken()` in all views with POST actions)
- [ ] **Chart.js integration pattern implemented** (local files in bundles, defensive checks)
- [ ] **Factory Method Pattern implemented** (FromEntity() and ToEntity() methods in ViewModels)
- [ ] **NO AutoMapper usage** (strictly forbidden - use Factory Methods instead)
- [ ] **Medical Environment Logging Standards implemented** (Serilog + Seq integration)
- [ ] **Comprehensive error tracking** (all operations logged with context)
- [ ] **Client IP tracking** (for security and audit purposes)
- [ ] **Systematic Review and Incremental Change Management implemented** (comprehensive review before changes)
- [ ] **To-Do List created** before any implementation
- [ ] **Changes documented** with reasons and impacts
- [ ] **Knowledge base updated** with all changes
- [ ] **Persian DatePicker Standard Implementation** (all date inputs use `class="persian-date"`)
- [ ] **No HTML5 date inputs** (`type="date"` avoided in Persian environment)
- [ ] **Persian DatePicker bundle loaded** (`@Scripts.Render("~/bundles/persian-datepicker")`)
- [ ] **Default date values set** (current date as default where appropriate)
- [ ] **Persian placeholder text** (proper Persian placeholders for date fields)
- [ ] Proper error handling implemented
- [ ] Performance optimized
- [ ] Medical data validation implemented
- [ ] Audit trails configured
- [ ] Authorization checks in place

## Knowledge Base Integration

This contract is now part of the project's knowledge base and must be referenced for all future development work. Any violation of these principles will result in immediate refactoring to ensure medical environment compliance.

**Critical Rules**:
1. **NEVER use AutoMapper** - Use Factory Method Pattern instead
2. **Always implement FromEntity() and ToEntity() methods** in ViewModels
3. **Maintain explicit control** over Entity-ViewModel conversions
4. **Follow Factory Method Pattern** for all data transformations
5. **Always conduct systematic review** before any code changes
6. **Implement changes incrementally** in small, testable units
7. **Create detailed To-Do List** before execution
8. **Document all changes** with reasons and impacts
9. **Update knowledge base** with every change
10. **Follow existing contracts strictly** - no violations allowed

**Last Updated**: Current Session
**Contract Status**: Active and Mandatory
**Scope**: All ClinicApp Development

---

## Integration with AI Compliance Contract

This contract works in conjunction with `CONTRACTS/AI_COMPLIANCE_CONTRACT.md` which defines mandatory rules for AI interactions with the ClinicApp project. All development work must comply with both contracts.

**Key Integration Points**:
- All changes must follow Atomic Changes Rule (AI_COMPLIANCE_CONTRACT Section 1)
- Pre-creation verification required (AI_COMPLIANCE_CONTRACT Section 2)
- No duplication allowed (AI_COMPLIANCE_CONTRACT Section 3)
- Mandatory documentation for all changes (AI_COMPLIANCE_CONTRACT Section 4)
- Stop and approval process required (AI_COMPLIANCE_CONTRACT Section 5)
- Security and quality standards enforced (AI_COMPLIANCE_CONTRACT Section 6)
- Transparent output format required (AI_COMPLIANCE_CONTRACT Section 7)
- No auto-execution allowed (AI_COMPLIANCE_CONTRACT Section 8)
- Project scope compliance (AI_COMPLIANCE_CONTRACT Section 9)
- Mandatory compliance with all rules (AI_COMPLIANCE_CONTRACT Section 10)

**Reference**: See `CONTRACTS/AI_COMPLIANCE_CONTRACT.md` for complete AI interaction guidelines.

### 7. Medical Environment Logging Standards (CRITICAL for Medical Data Integrity)
**Principle**: Implement comprehensive logging for medical environments using Serilog and Seq to ensure 100% traceability and error resistance.

**Implementation Rules**:
- ✅ **DO**: Use structured logging with Serilog for all medical operations
- ✅ **DO**: Log all user actions, data changes, and system events
- ✅ **DO**: Include user context (UserId, IP, Timestamp) in all logs
- ✅ **DO**: Use Seq for centralized log viewing and analysis
- ✅ **DO**: Implement defensive logging with null checks
- ✅ **DO**: Log both success and failure scenarios
- ❌ **DON'T**: Skip logging for critical medical operations
- ❌ **DON'T**: Use generic error messages without context
- ❌ **DON'T**: Log sensitive medical data (PII)

**Example Implementation**:
```csharp
// ✅ Correct Approach - Comprehensive Medical Logging
public async Task<ActionResult> Index(int page = 1, int pageSize = 20)
{
    try
    {
        _logger.Information("درخواست نمایش لیست صلاحیت‌های خدماتی پزشکان. صفحه: {Page}, اندازه: {PageSize}, کاربر: {UserId}, IP: {IPAddress}", 
            page, pageSize, _currentUserService.UserId, GetClientIPAddress());

        // Log input validation
        if (page <= 0 || pageSize <= 0)
        {
            _logger.Warning("پارامترهای نامعتبر صفحه. صفحه: {Page}, اندازه: {PageSize}", page, pageSize);
            TempData["Error"] = "پارامترهای صفحه نامعتبر است.";
            return View(new PagedResult<DoctorServiceCategoryViewModel> { Items = new List<DoctorServiceCategoryViewModel>() });
        }

        var result = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(0, "", page, pageSize);
        
        if (!result.Success)
        {
            _logger.Error("خطا در دریافت لیست صلاحیت‌ها. پیام: {ErrorMessage}, کد خطا: {ErrorCode}", 
                result.Message, result.ErrorCode);
            TempData["Error"] = result.Message;
            return View(new PagedResult<DoctorServiceCategoryViewModel> { Items = new List<DoctorServiceCategoryViewModel>() });
        }

        _logger.Information("لیست صلاحیت‌های خدماتی پزشکان با موفقیت بارگذاری شد. تعداد: {Count}, صفحه: {Page}", 
            result.Data?.Items?.Count ?? 0, page);
        
        return View(result.Data);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطای غیرمنتظره در بارگذاری لیست صلاحیت‌های خدماتی پزشکان. صفحه: {Page}, کاربر: {UserId}", 
            page, _currentUserService.UserId);
        TempData["Error"] = "خطا در بارگذاری لیست صلاحیت‌ها";
        return View(new PagedResult<DoctorServiceCategoryViewModel> { Items = new List<DoctorServiceCategoryViewModel>() });
    }
}

// ✅ Helper method for client IP tracking
private string GetClientIPAddress()
{
    try
    {
        var forwarded = Request.Headers["X-Forwarded-For"];
        if (!string.IsNullOrEmpty(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }
        return Request.UserHostAddress ?? "Unknown";
    }
    catch
    {
        return "Unknown";
    }
}
```

**Seq Integration**:
```csharp
// ✅ Add to Startup.cs or Global.asax.cs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq("http://localhost:5341") // Seq server URL
    .Enrich.WithProperty("Application", "ClinicApp")
    .Enrich.WithProperty("Environment", "Production")
    .CreateLogger();
```

**Medical Data Logging Checklist**:
- [ ] All medical operations logged with user context
- [ ] Input validation logged before processing
- [ ] Success/failure scenarios logged with details
- [ ] Client IP and timestamp included
- [ ] No sensitive medical data in logs
- [ ] Structured logging format used
- [ ] Seq integration configured
- [ ] Error tracking comprehensive

### 8. Systematic Review and Incremental Change Management (CRITICAL for System Integrity)
**Principle**: All changes must follow a systematic, incremental approach with comprehensive review and documentation to maintain system integrity and prevent duplicate implementations.

**Implementation Rules**:
- ✅ **DO**: Conduct comprehensive systematic review before any code changes
- ✅ **DO**: Implement changes incrementally in small, testable units
- ✅ **DO**: Create detailed To-Do List before execution
- ✅ **DO**: Follow existing contracts and knowledge base strictly
- ✅ **DO**: Document all changes with reasons and impacts
- ❌ **DON'T**: Make changes without systematic review
- ❌ **DON'T**: Implement large, monolithic changes
- ❌ **DON'T**: Violate existing design contracts
- ❌ **DON'T**: Skip documentation and knowledge base updates

**Systematic Review Process**:
```csharp
// ✅ Required: Complete module review before changes
// 1. Examine all related modules, controllers, and ViewModels
// 2. Identify potential duplicate methods or actions
// 3. Review database structure and architecture
// 4. Check existing implementations in knowledge base
// 5. Validate against design contracts
```

**Incremental Change Implementation**:
```csharp
// ✅ Correct Approach - Small, testable changes
// Step 1: Fix one specific issue
public async Task<ActionResult> FixSpecificIssue()
{
    // Single, focused change
    // Testable independently
    // Rollback possible if needed
}

// Step 2: Test and validate
// Step 3: Move to next incremental change
// ❌ Avoid: Large, complex changes that are hard to test
```

**To-Do List Template**:
```markdown
## Change Implementation Plan

### Objective
- [ ] Clear description of what needs to be changed

### Affected Components
- [ ] Modules involved
- [ ] Controllers affected
- [ ] ViewModels modified
- [ ] Database changes (if any)

### Implementation Steps
- [ ] Step 1: [Description]
- [ ] Step 2: [Description]
- [ ] Step 3: [Description]

### Testing Requirements
- [ ] Unit tests needed
- [ ] Integration tests required
- [ ] Manual testing scenarios

### Rollback Plan
- [ ] How to revert changes if needed
- [ ] Backup procedures

### Documentation Updates
- [ ] Update knowledge base
- [ ] Modify design contracts
- [ ] Update compliance checklist
```

**Change Documentation Template**:
```markdown
## Change Record

**Date**: [YYYY-MM-DD]
**Change Type**: [Bug Fix/Feature/Refactoring]
**Reason**: [Why this change was needed]

**Files Modified**:
- [File Path 1]: [Description of changes]
- [File Path 2]: [Description of changes]

**Impact Analysis**:
- [ ] No impact on other modules
- [ ] Minor impact: [Description]
- [ ] Major impact: [Description]

**Testing Results**:
- [ ] All tests passed
- [ ] Issues found: [Description]
- [ ] Rollback required: [Yes/No]

**Knowledge Base Updates**:
- [ ] Design contracts updated
- [ ] New patterns documented
- [ ] Compliance checklist updated
```

**Systematic Review Checklist**:
- [ ] All related modules examined
- [ ] No duplicate methods/actions identified
- [ ] Database structure reviewed
- [ ] Architecture impact assessed
- [ ] Existing implementations checked
- [ ] Design contracts validated
- [ ] Knowledge base consulted
- [ ] Change plan documented

**Incremental Change Checklist**:
- [ ] Changes are small and focused
- [ ] Each change is independently testable
- [ ] Rollback plan exists
- [ ] Testing requirements defined
- [ ] Documentation updated
- [ ] Knowledge base maintained
- [ ] Compliance verified
- [ ] Impact documented

**Contract Compliance Verification**:
- [ ] All changes follow existing contracts
- [ ] No AutoMapper usage introduced
- [ ] Factory Method Pattern maintained
- [ ] Strongly-typed ViewModels used
- [ ] Comprehensive logging implemented
- [ ] Medical environment standards met
- [ ] Error resistance maintained
- [ ] Performance optimized

## Compliance Checklist
