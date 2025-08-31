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

### 2. Avoid Dynamic Types (Critical)
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

### 6. Efficient and Practical Solutions (Performance Critical)
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
- [ ] Proper error handling implemented
- [ ] Performance optimized
- [ ] Medical data validation implemented
- [ ] Audit trails configured
- [ ] Authorization checks in place

## Knowledge Base Integration

This contract is now part of the project's knowledge base and must be referenced for all future development work. Any violation of these principles will result in immediate refactoring to ensure medical environment compliance.

**Last Updated**: Current Session
**Contract Status**: Active and Mandatory
**Scope**: All ClinicApp Development
