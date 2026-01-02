# 🎯 Unified Dashboard - Enterprise-Grade SPA Architecture

## ✅ **معماری: Tab-Based Single Page Application**

مثل AWS Console, Azure Portal, Google Cloud Console

---

## 📋 **ویژگی‌ها:**

### 1. **بدون رفرش صفحه (SPA Experience)**
- تمام محتوا با AJAX load می‌شود
- تغییر Tab بدون reload کل صفحه
- سرعت بالا و تجربه کاربری بهتر

### 2. **URL History Management**
- هر Tab یک URL منحصر به فرد دارد: `#profile`, `#appointments`, etc.
- Browser back/forward کار می‌کند
- Bookmark قابل استفاده برای هر Tab

### 3. **Smart Caching**
- Tab‌های cacheable (Profile, Settings) فقط یکبار load می‌شوند
- بهبود Performance
- کاهش درخواست‌های غیرضروری

### 4. **Bulletproof Validation**
- Client-side validation (jQuery Validate)
- Server-side validation
- AJAX Form submission
- Error handling با Retry mechanism

### 5. **Responsive Design**
- Mobile-first approach
- Tab navigation با scroll در موبایل
- Touch-friendly UI

---

## 🏗️ **ساختار فایل‌ها:**

```
Areas/Patient/
├── Views/
│   └── Dashboard/
│       ├── Index.cshtml                    // Shell اصلی با Tab Navigation
│       ├── _DashboardOverview.cshtml       // Tab: خانه (Dashboard stats)
│       ├── _ProfileTab.cshtml              // Tab: پروفایل (Profile edit form)
│       ├── _AppointmentsTab.cshtml         // Tab: نوبت‌ها
│       ├── _MedicalRecordTab.cshtml        // Tab: پرونده پزشکی
│       └── _SettingsTab.cshtml             // Tab: تنظیمات
│
├── Controllers/
│   ├── DashboardController.cs              // Tab content actions
│   └── Api/
│       ├── PatientDashboardApiController.cs  // Dashboard API
│       └── ProfileApiController.cs           // Profile API (CRUD)
│
Content/
├── js/
│   ├── unified-dashboard.js                // SPA Module (Tab management, AJAX, History)
│   └── patient-dashboard.js                // Dashboard stats loading
│
└── css/
    └── patient-dashboard-unified.css       // Tab styling (AWS Console style)
```

---

## 🔄 **جریان کار (Workflow):**

### 1. **بارگذاری اولیه:**
```
User → /Patient/Dashboard
  ↓
DashboardController.Index()
  ↓
Render: Index.cshtml (Tab Shell)
  ↓
Default Tab: Overview (Already loaded)
  ↓
UnifiedDashboard.init() (JavaScript)
```

### 2. **کلیک روی Tab:**
```
User clicks "پروفایل من" Tab
  ↓
UnifiedDashboard.switchTab('profile')
  ↓
Update URL: #profile (pushState)
  ↓
Check Cache → Not found
  ↓
AJAX: GET /Patient/Dashboard/ProfileTab
  ↓
DashboardController.ProfileTab()
  ↓
Render: _ProfileTab.cshtml
  ↓
JavaScript: Load profile data via AJAX
  ↓
GET /Patient/Api/Profile/GetProfile
  ↓
ProfileApiController.GetProfile()
  ↓
Return JSON → Populate form
```

### 3. **ذخیره Profile:**
```
User fills form → Click "ذخیره تغییرات"
  ↓
JavaScript intercepts submit (AJAX)
  ↓
POST /Patient/Dashboard/UpdateProfile
  ↓
Server-side validation
  ↓
Success → NotificationHelper.showSuccess()
  ↓
Clear cache → Tab remains open
```

### 4. **Browser Back Button:**
```
User clicks browser back
  ↓
popstate event (History API)
  ↓
UnifiedDashboard.switchTab(previousTab, skipHistory=true)
  ↓
Load tab content (from cache if available)
```

---

## 🎨 **Tab Navigation Styling:**

```css
/* AWS Console style */
.dashboard-tabs .nav-link {
    border-bottom: 3px solid transparent;
    transition: all 0.2s ease;
}

.dashboard-tabs .nav-link.active {
    color: var(--medical-primary);
    border-bottom-color: var(--medical-primary);
    font-weight: 600;
}
```

---

## 📡 **API Endpoints:**

### Dashboard Tabs:
- `GET /Patient/Dashboard/ProfileTab` - Profile form
- `GET /Patient/Dashboard/AppointmentsTab` - Appointments list
- `GET /Patient/Dashboard/MedicalRecordTab` - Medical records
- `GET /Patient/Dashboard/SettingsTab` - Settings form

### Profile API:
- `GET /Patient/Api/Profile/GetProfile` - Get profile data (JSON)
- `POST /Patient/Api/Profile/UpdateProfile` - Update profile

### Dashboard Stats API:
- `GET /Patient/Api/PatientDashboard/GetQuickStats` - Quick stats
- `GET /Patient/Api/PatientDashboard/GetRecentAppointments` - Recent appointments
- `GET /Patient/Api/PatientDashboard/GetUpcomingAppointments` - Upcoming appointments
- `GET /Patient/Api/PatientDashboard/GetRecentReceptions` - Recent receptions

---

## 🛡️ **Security:**

### 1. **Authorization:**
- `[Authorize]` attribute on all controllers
- `GetCurrentPatientIdAsync()` برای دریافت Patient ID
- Server-side validation برای تمام form submissions

### 2. **CSRF Protection:**
- `@Html.AntiForgeryToken()` in all forms
- `[ValidateAntiForgeryToken]` on POST actions

### 3. **XSS Prevention:**
- HTML encoding خودکار در Razor
- Input validation (maxlength, pattern, required)

### 4. **SQL Injection Prevention:**
- Entity Framework (parameterized queries)
- No raw SQL concatenation

---

## ⚡ **Performance:**

### 1. **Caching Strategy:**
```javascript
var config = {
    tabs: {
        profile: { cacheable: true },    // ✅ Cache
        settings: { cacheable: true },   // ✅ Cache
        appointments: { cacheable: false }, // ❌ Always fresh
        'medical-record': { cacheable: false } // ❌ Always fresh
    }
};
```

### 2. **AJAX Optimization:**
- `timeout: 30000` (30 seconds)
- Error handling با Retry
- Loading states برای UX بهتر

### 3. **CSS Animations:**
- `fadeIn`, `slideInUp` برای smooth transitions
- GPU-accelerated transforms

---

## 🧪 **Testing Checklist:**

### ✅ Functional Testing:
- [ ] Dashboard load می‌شود
- [ ] کلیک روی هر Tab محتوا را load می‌کند
- [ ] Profile form با موفقیت submit می‌شود
- [ ] Validation کار می‌کند (client + server)
- [ ] Browser back/forward کار می‌کند

### ✅ Security Testing:
- [ ] Unauthorized access redirect به Login
- [ ] CSRF token در تمام POST requests
- [ ] XSS prevention (HTML encoding)

### ✅ Performance Testing:
- [ ] Cache کار می‌کند (Network tab → 304 Not Modified)
- [ ] Tab switching < 500ms
- [ ] Form submission < 2s

### ✅ Responsive Testing:
- [ ] Mobile (< 768px) - Tab scroll کار می‌کند
- [ ] Tablet (768px - 1024px)
- [ ] Desktop (> 1024px)

---

## 📝 **نکات مهم:**

### 1. **برای اضافه کردن Tab جدید:**
```javascript
// 1. در unified-dashboard.js:
tabs: {
    'new-tab': {
        name: 'new-tab',
        title: 'تب جدید',
        url: '/Patient/Dashboard/NewTab',
        requiresAuth: true,
        cacheable: false
    }
}

// 2. در Index.cshtml:
<li class="nav-item">
    <button class="nav-link" data-bs-target="#content-new-tab" 
            data-tab-name="new-tab">تب جدید</button>
</li>
<div class="tab-pane" id="content-new-tab">...</div>

// 3. در DashboardController.cs:
[HttpGet]
public async Task<ActionResult> NewTab()
{
    return PartialView("_NewTab");
}
```

### 2. **برای اضافه کردن Form Validation:**
```html
<!-- Client-side validation -->
<input type="text" class="form-control" 
       required 
       maxlength="50" 
       data-val="true"
       data-val-required="این فیلد الزامی است" />

<!-- Server-side validation در Controller -->
if (string.IsNullOrWhiteSpace(value))
{
    return ErrorJsonResult("این فیلد الزامی است");
}
```

### 3. **برای Debugging:**
```javascript
// در Console:
UnifiedDashboard.getCurrentTab()  // Check current tab
UnifiedDashboard.reloadTab('profile')  // Force reload a tab

// Check cache:
console.log(config.cache);
```

---

## 🚀 **Next Phase (توسعه آینده):**

1. **Appointments Tab:**
   - لیست نوبت‌ها با فیلتر
   - لغو نوبت
   - جزئیات نوبت

2. **Medical Record Tab:**
   - تاریخچه پزشکی
   - آپلود اسناد
   - نمایش نسخه‌ها

3. **Settings Tab:**
   - تنظیمات اعلان‌ها
   - تغییر رمز عبور
   - حریم خصوصی

4. **Real-time Updates:**
   - SignalR برای نوبت‌های جدید
   - Notification system

---

## ✅ **Status: Phase 1 Complete**

- ✅ Tab-based Navigation
- ✅ AJAX Loading
- ✅ URL History
- ✅ Profile Tab با Form
- ✅ Validation (Client + Server)
- ✅ Error Handling
- ✅ Responsive Design

**Ready for Production Testing** 🎉

