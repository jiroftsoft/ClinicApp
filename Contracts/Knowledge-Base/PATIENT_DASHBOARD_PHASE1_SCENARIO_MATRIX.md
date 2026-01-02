# Patient Dashboard - PHASE 1: Flow & Scenario Design

## 🎯 Primary Flow Identification

### Main Flow: Patient Dashboard Access
```
User clicks "داشبورد" in profile menu
  → DashboardController.Index()
  → Check Authorization (logged in + Patient role)
  → Load Dashboard skeleton (shell)
  → AJAX load sections:
    1. Recent Appointments
    2. Upcoming Appointments
    3. Recent Visits (if available)
    4. Documents (if available)
    5. Quick Stats
  → Render components
  → User can interact with each section
```

## 📊 Scenario Matrix

### Branch 1: Authentication State

| Scenario | Condition | Action | Return Destination |
|----------|-----------|--------|-------------------|
| **1.1** Logged in + Patient role | `User.Identity.IsAuthenticated && User.IsInRole("Patient")` | ✅ Show dashboard | Dashboard page |
| **1.2** Not logged in | `!User.Identity.IsAuthenticated` | ❌ Redirect to login | `/Account/Login?returnUrl=/Patient/Dashboard` |
| **1.3** Logged in but not Patient | `User.Identity.IsAuthenticated && !User.IsInRole("Patient")` | ❌ Show error | Error page or redirect to appropriate dashboard |
| **1.4** Session expired | `ICurrentUserService.GetPatientInfoAsync() == null` | ❌ Redirect to login | `/Account/Login?returnUrl=/Patient/Dashboard` |

### Branch 2: Section Loading (AJAX)

| Scenario | Condition | Action | UI State |
|----------|-----------|--------|----------|
| **2.1** API Success + Data exists | `response.success && response.data.length > 0` | ✅ Render section | Show data |
| **2.2** API Success + Empty data | `response.success && response.data.length === 0` | ✅ Show empty state | Show "هیچ موردی یافت نشد" |
| **2.3** API Error (401) | `xhr.status === 401` | ❌ Redirect to login | Open login modal with returnUrl |
| **2.4** API Error (404) | `xhr.status === 404` | ❌ Show error message | Show error in section |
| **2.5** API Error (500) | `xhr.status === 500` | ❌ Show error + retry button | Show error with retry option |
| **2.6** Network timeout | `status === 'timeout'` | ❌ Show error + retry | Show error with retry option |
| **2.7** Network error | `status === 'error'` | ❌ Show error + retry | Show error with retry option |

### Branch 3: Authorization & Security

| Scenario | Condition | Action | Security Level |
|----------|-----------|--------|----------------|
| **3.1** Valid patientId from auth | `patientId == currentUser.PatientId` | ✅ Allow access | ✅ Safe |
| **3.2** Tampered patientId in URL | `patientId != currentUser.PatientId` | ❌ Reject + Log | 🚨 Critical - Log security event |
| **3.3** Missing patientId | `patientId == null` | ❌ Redirect to login | ⚠️ Medium |
| **3.4** PatientId from query param | `Request.QueryString["patientId"]` | ❌ Ignore + Use auth | ✅ Safe - Use auth context only |

### Branch 4: Multi-Tab / Back Button

| Scenario | Condition | Action | Behavior |
|----------|-----------|--------|----------|
| **4.1** Browser back button | `window.popstate` | ✅ Reload from cache or API | Restore previous state |
| **4.2** Multiple tabs open | `Multiple tabs` | ✅ Each tab independent | No interference |
| **4.3** Tab refresh | `F5 or Ctrl+R` | ✅ Reload all sections | Fresh data |

### Branch 5: Pagination

| Scenario | Condition | Action | UI Update |
|----------|-----------|--------|-----------|
| **5.1** Load more clicked | `page < totalPages` | ✅ Load next page | Append to list |
| **5.2** Last page reached | `page >= totalPages` | ✅ Disable "Load more" | Show "تمام موارد" |
| **5.3** Pagination error | `API error during pagination` | ❌ Show error | Keep current page |

## 🔄 Flow State Machine

```
[Initial State]
  ↓
[Check Auth]
  ├─→ [Not Authenticated] → [Login Modal] → [Return to Dashboard]
  └─→ [Authenticated] → [Load Dashboard Shell]
                        ↓
                   [AJAX Load Sections]
                        ├─→ [Section 1: Recent Appointments]
                        │     ├─→ [Success] → [Render]
                        │     ├─→ [Empty] → [Show Empty State]
                        │     └─→ [Error] → [Show Error + Retry]
                        ├─→ [Section 2: Upcoming Appointments]
                        │     └─→ (Same branches)
                        ├─→ [Section 3: Quick Stats]
                        │     └─→ (Same branches)
                        └─→ [All Sections Loaded] → [Dashboard Ready]
```

## ⚠️ Flow Break Risks

### Critical Risks
1. **🚨 Authorization Bypass**: اگر `patientId` از query param استفاده شود
   - **Mitigation**: همیشه از `ICurrentUserService` استفاده شود

2. **🚨 Context Loss**: اگر user در وسط flow logout کند
   - **Mitigation**: Check auth در هر AJAX request

3. **🚨 Data Leakage**: اگر patient دیگری data ببیند
   - **Mitigation**: Validate `patientId` در service layer

### High Risks
4. **⚠️ Network Failure**: اگر همه sections fail شوند
   - **Mitigation**: Retry logic + graceful degradation

5. **⚠️ Performance**: اگر N+1 queries وجود داشته باشد
   - **Mitigation**: Batch loading + caching

### Medium Risks
6. **⚠️ Empty State Confusion**: اگر user هیچ data نداشته باشد
   - **Mitigation**: Clear empty state messages + CTAs

7. **⚠️ Loading State**: اگر loading خیلی طول بکشد
   - **Mitigation**: Skeleton screens + progress indicators

## ✅ Flow Continuity Guarantees

1. ✅ **Auth State Preserved**: هر AJAX request auth را check می‌کند
2. ✅ **Return Destination**: بعد از login، به dashboard برمی‌گردد
3. ✅ **Error Recovery**: هر section می‌تواند independently retry کند
4. ✅ **Data Persistence**: Cache برای performance
5. ✅ **No Dead Ends**: همیشه clear next action

## 🎨 UI/UX Compliance

- ✅ **Formal & Administrative**: Healthcare-appropriate styling
- ✅ **High Readability**: Clear fonts, spacing, contrast
- ✅ **No Flashy Colors**: Neutral palette
- ✅ **No Heavy Animations**: Subtle transitions only
- ✅ **Mobile-First**: Responsive design
- ✅ **Clear CTAs**: واضح next actions
- ✅ **No Dead Ends**: همیشه clear continuation

