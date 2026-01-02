# User Profile Component - Enterprise-Grade Reusable Component

## 📋 Overview

کامپوننت پروفایل کاربر به صورت **Enterprise-Grade** و **Fully Reusable** پیاده‌سازی شده است، مشابه سیستم‌های بانکی و شرکت‌های بزرگ مثل Google و Oracle.

## ✅ Features

- ✅ **Fully Modular**: کاملاً ماژولار و قابل استفاده مجدد
- ✅ **Multiple Instances**: پشتیبانی از چند instance همزمان
- ✅ **Component Lifecycle**: مدیریت کامل lifecycle
- ✅ **Event-Driven**: معماری event-driven
- ✅ **Error Recovery**: Retry logic و error recovery
- ✅ **Performance Optimized**: بهینه‌سازی شده برای performance
- ✅ **API-First**: طراحی API-First
- ✅ **Configurable**: کاملاً قابل تنظیم

## 🚀 Usage Examples

### 1. Auto-Initialize on Page Load

```html
<div data-profile-component="true">
    @Html.Partial("_UserProfileComponent", model)
</div>
```

### 2. Manual Initialization

```javascript
// Initialize with default config
var instance = UserProfileComponent.init('#my-profile-container');

// Initialize with custom config
var instance = UserProfileComponent.init('#my-profile-container', {
    apiUrl: '/Account/Profile',
    formId: 'my-profile-form',
    enableValidation: true,
    enableToastr: true
});
```

### 3. Load via AJAX (Dynamic Loading)

```javascript
// Load component dynamically
UserProfileComponent.load('#container', {
    containerClass: 'col-12',
    showHeader: false,
    formId: 'dashboard-profile-form',
    apiUrl: '/Account/Profile',
    cancelUrl: '/Dashboard'
}).done(function(instance) {
    console.log('Component loaded:', instance);
}).fail(function(error) {
    console.error('Failed to load component:', error);
});
```

### 4. Use in Dashboard

```csharp
// In Controller
public ActionResult Dashboard()
{
    // ... other code
    return View();
}
```

```html
<!-- In Dashboard View -->
<div id="profile-section" class="col-md-6">
    <!-- Component will be loaded here -->
</div>

<script>
$(document).ready(function() {
    UserProfileComponent.load('#profile-section', {
        containerClass: 'col-12',
        showHeader: true,
        showHeader: false // Hide header in dashboard
    });
});
</script>
```

### 5. Use in Modal

```html
<!-- Modal -->
<div class="modal fade" id="profileModal">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-body" id="profile-modal-body">
                <!-- Component will be loaded here -->
            </div>
        </div>
    </div>
</div>

<script>
$('#profileModal').on('show.bs.modal', function() {
    UserProfileComponent.load('#profile-modal-body', {
        containerClass: 'col-12',
        showHeader: false,
        cancelUrl: null // Hide cancel button in modal
    });
});
</script>
```

### 6. Use Partial View Directly

```csharp
// In Controller
public ActionResult MyPage()
{
    var profileResult = await _userProfileService.GetMyProfileAsync(userId);
    ViewBag.ContainerClass = "col-12";
    ViewBag.ShowHeader = false;
    return View(profileResult.Data);
}
```

```html
<!-- In View -->
@Html.Partial("_UserProfileComponent", Model)
```

## 🔧 Configuration Options

### JavaScript Configuration

```javascript
{
    formSelector: '#profile-form',           // Form selector
    submitButtonSelector: 'button[type="submit"]', // Submit button selector
    cancelButtonSelector: '.btn-secondary',   // Cancel button selector
    validationSummarySelector: '.alert-danger', // Validation summary selector
    apiUrl: '/Account/Profile',              // API endpoint
    containerSelector: null,                 // Container selector
    autoInit: true,                          // Auto-initialize on page load
    enableValidation: true,                   // Enable client-side validation
    enableToastr: true,                      // Enable toastr notifications
    retryAttempts: 3,                        // Number of retry attempts
    retryDelay: 1000                         // Delay between retries (ms)
}
```

### Partial View Configuration (ViewBag)

```csharp
ViewBag.ContainerClass = "col-12 col-md-6";  // Container CSS class
ViewBag.ShowHeader = true;                    // Show/hide header
ViewBag.FormId = "custom-profile-form";       // Custom form ID
ViewBag.ApiUrl = "/Account/Profile";          // Custom API URL
ViewBag.CancelUrl = "/Dashboard";             // Cancel button URL
ViewBag.CancelButtonText = "انصراف";          // Cancel button text
ViewBag.SubmitButtonText = "ذخیره";           // Submit button text
```

## 📡 API Endpoints

### GET /Account/GetProfile
دریافت اطلاعات پروفایل به صورت JSON

**Response:**
```json
{
    "success": true,
    "message": "اطلاعات پروفایل با موفقیت دریافت شد.",
    "code": "SUCCESS",
    "data": {
        "UserId": "...",
        "FirstName": "...",
        "LastName": "...",
        "Email": "...",
        "Gender": 1,
        "Address": "..."
    }
}
```

### POST /Account/Profile
به‌روزرسانی پروفایل

**Request:**
```json
{
    "UserId": "...",
    "FirstName": "...",
    "LastName": "...",
    "Email": "...",
    "Gender": 1,
    "Address": "..."
}
```

**Response:**
```json
{
    "success": true,
    "message": "پروفایل با موفقیت به‌روزرسانی شد.",
    "code": "SUCCESS",
    "data": { ... }
}
```

### GET /Account/LoadProfileComponent
بارگذاری Partial View با تنظیمات دلخواه

**Query Parameters:**
- `containerClass`: CSS class for container
- `showHeader`: Show/hide header (true/false)
- `formId`: Custom form ID
- `apiUrl`: Custom API URL
- `cancelUrl`: Cancel button URL
- `cancelButtonText`: Cancel button text
- `submitButtonText`: Submit button text

## 🎯 Events

کامپوننت events زیر را trigger می‌کند:

```javascript
// Component initialized
$form.on('profileComponent:initialized', function(e, instance) {
    console.log('Component initialized:', instance);
});

// Validation failed
$form.on('profileComponent:validationFailed', function(e, instance) {
    console.log('Validation failed');
});

// Update success
$form.on('profileComponent:updateSuccess', function(e, response, instance) {
    console.log('Update success:', response);
});

// Update error
$form.on('profileComponent:updateError', function(e, response, instance) {
    console.log('Update error:', response);
});

// AJAX error
$form.on('profileComponent:ajaxError', function(e, xhr, status, error, instance) {
    console.log('AJAX error:', error);
});

// Cancel clicked
$form.on('profileComponent:cancel', function(e, instance) {
    console.log('Cancel clicked');
});
```

## 🔄 Component Lifecycle

1. **Initialization**: `UserProfileComponent.init()` یا auto-init
2. **Event Binding**: Bind events to form
3. **Validation Setup**: Initialize validation
4. **Ready**: Component ready for use
5. **Destroy**: `UserProfileComponent.destroy()` برای cleanup

## 🛠️ Advanced Usage

### Get Component Instance

```javascript
var instance = UserProfileComponent.getInstance('#my-container');
if (instance) {
    console.log('Instance found:', instance);
}
```

### Destroy Component

```javascript
UserProfileComponent.destroy('#my-container');
```

### Multiple Instances

```javascript
// Initialize multiple instances
var instance1 = UserProfileComponent.init('#container1', { formId: 'form1' });
var instance2 = UserProfileComponent.init('#container2', { formId: 'form2' });
var instance3 = UserProfileComponent.init('#container3', { formId: 'form3' });
```

## 📝 Best Practices

1. **Always use Partial View**: برای reusability
2. **Configure via ViewBag**: برای server-side configuration
3. **Use Events**: برای custom logic
4. **Handle Errors**: همیشه error handling داشته باشید
5. **Destroy on Unload**: در صورت نیاز component را destroy کنید

## 🎨 Styling

کامپوننت از `user-profile.css` استفاده می‌کند. برای custom styling:

```css
.profile-card {
    /* Custom styles */
}

.profile-card .card-header {
    /* Custom header styles */
}
```

## ✅ Enterprise-Grade Features

- ✅ **Retry Logic**: Automatic retry on network errors
- ✅ **Error Recovery**: Graceful error handling
- ✅ **Performance**: Optimized for large-scale applications
- ✅ **Security**: Anti-forgery tokens, validation
- ✅ **Accessibility**: ARIA attributes, keyboard navigation
- ✅ **Mobile-First**: Responsive design
- ✅ **Documentation**: Comprehensive documentation

## 📚 Related Files

- `Views/Account/_UserProfileComponent.cshtml` - Partial View
- `Content/js/user-profile-component.js` - JavaScript Module
- `Controllers/AccountController.cs` - Controller Actions
- `Content/css/user-profile.css` - Styles

