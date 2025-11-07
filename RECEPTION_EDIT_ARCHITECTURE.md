# 🏗️ نقشه معماری فرم ویرایش پذیرش ReceptionV2

## 📋 Executive Summary

**هدف**: طراحی و پیاده‌سازی فرم ویرایش پذیرش برای ReceptionV2 با رعایت اصول معماری، امنیت، و قوانین کسب‌وکار

**اهمیت**: بسیار مهم، حساس و حیاتی - نیاز به دقت بالا و رعایت تمامی اصول سیستماتیک

---

## 🎯 اهداف و الزامات

### 1. اهداف اصلی
- ✅ امکان ویرایش پذیرش‌های موجود (Pending/Completed)
- ✅ حفظ سازگاری با معماری ReceptionV2 (API-based, Facade Pattern)
- ✅ رعایت قوانین کسب‌وکار و محدودیت‌های ویرایش
- ✅ امنیت و اعتبارسنجی کامل
- ✅ UX بهینه و کاربرپسند

### 2. محدودیت‌های ویرایش

#### پذیرش‌های Pending (در انتظار)
- ✅ قابل ویرایش: تمام فیلدها
- ✅ قابل تغییر: بیمار، پزشک، دپارتمان، خدمات، بیمه‌ها، تاریخ
- ✅ نیاز به بازمحاسبه: قیمت‌ها و سهم‌ها

#### پذیرش‌های Completed (تکمیل شده)
- ⚠️ محدودیت: فقط فیلدهای خاص قابل ویرایش
- ✅ قابل ویرایش: یادداشت‌ها، اولویت، نوع پذیرش
- ❌ غیرقابل ویرایش: بیمار، خدمات، مبالغ پرداخت شده
- ⚠️ نیاز به تایید: تغییرات مهم نیاز به تایید مدیر

#### پذیرش‌های Cancelled (لغو شده)
- ❌ غیرقابل ویرایش: هیچ فیلدی قابل ویرایش نیست

---

## 🏗️ معماری پیشنهادی

### 1. لایه‌های معماری

```
┌─────────────────────────────────────────┐
│   Presentation Layer (Views/JS)        │
│   - Edit.cshtml                        │
│   - reception-edit.js                  │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   Controller Layer                      │
│   - ReceptionControllerV2.Edit()        │
│   - ReceptionControllerV2.Update()      │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   API Layer                             │
│   - ReceptionApiV1Controller            │
│   - GetReceptionForEdit()              │
│   - UpdateReception()                  │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   Facade Layer                          │
│   - IReceptionFacade                   │
│   - LoadReceptionForEditAsync()        │
│   - UpdateReceptionAsync()             │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   Business Logic Layer                  │
│   - ReceptionBusinessRulesEngine       │
│   - ReceptionValidationOrchestrator    │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   Data Access Layer                     │
│   - IReceptionRepository               │
│   - ApplicationDbContext                │
└─────────────────────────────────────────┘
```

### 2. جریان داده (Data Flow)

#### بارگذاری فرم ویرایش
```
1. User clicks "Edit" → ReceptionList
2. GET /ReceptionV2/reception/edit/{id}
3. Controller → Facade.LoadReceptionForEditAsync(id)
4. Facade → Repository.GetByIdWithDetailsAsync(id)
5. Facade → BusinessRules.ValidateEditPermission()
6. Facade → Build EditViewModel
7. Controller → View(EditViewModel)
8. View → Load JavaScript modules
9. JavaScript → API.GetReceptionForEdit(id)
10. Populate form fields
```

#### ذخیره تغییرات
```
1. User clicks "Save" → JavaScript
2. JavaScript → Validate form
3. JavaScript → API.UpdateReception(data)
4. API → Facade.UpdateReceptionAsync(request)
5. Facade → BusinessRules.ValidateEditRules()
6. Facade → RecalculatePrices() [if needed]
7. Facade → Repository.Update()
8. Facade → Repository.SaveChangesAsync()
9. API → Return ServiceResult
10. JavaScript → Show success/error
```

---

## 📐 طراحی جزئیات

### 1. ViewModel: ReceptionEditViewModelV2

```csharp
public class ReceptionEditViewModelV2
{
    // شناسه پذیرش
    public int ReceptionId { get; set; }
    
    // وضعیت پذیرش (برای تعیین محدودیت‌های ویرایش)
    public ReceptionStatus Status { get; set; }
    
    // اطلاعات بیمار (readonly اگر Completed)
    public int PatientId { get; set; }
    public string PatientFullName { get; set; }
    public string PatientNationalCode { get; set; }
    
    // اطلاعات پزشک و دپارتمان
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public int ClinicId { get; set; }
    
    // تاریخ پذیرش
    public DateTime ReceptionDate { get; set; }
    public string ReceptionDateShamsi { get; set; }
    
    // بیمه‌ها
    public int? BasePlanId { get; set; }
    public int? SupplementaryPlanId { get; set; }
    
    // خدمات (لیست آیتم‌های پذیرش)
    public List<ReceptionItemEditDto> Items { get; set; }
    
    // مبالغ
    public decimal TotalAmount { get; set; }
    public decimal InsurerShareAmount { get; set; }
    public decimal PatientCoPay { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    
    // یادداشت‌ها و تنظیمات
    public string Notes { get; set; }
    public ReceptionType Type { get; set; }
    public AppointmentPriority Priority { get; set; }
    public bool IsEmergency { get; set; }
    
    // محدودیت‌های ویرایش
    public EditPermissions Permissions { get; set; }
    
    // لیست‌های کمکی
    public List<SelectListItem> Doctors { get; set; }
    public List<SelectListItem> Departments { get; set; }
    public List<SelectListItem> Services { get; set; }
}

public class ReceptionItemEditDto
{
    public int ReceptionItemId { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PatientShareAmount { get; set; }
    public decimal InsurerShareAmount { get; set; }
}

public class EditPermissions
{
    public bool CanEditPatient { get; set; }
    public bool CanEditDoctor { get; set; }
    public bool CanEditDepartment { get; set; }
    public bool CanEditServices { get; set; }
    public bool CanEditInsurances { get; set; }
    public bool CanEditAmounts { get; set; }
    public bool CanEditDate { get; set; }
    public bool RequiresApproval { get; set; }
}
```

### 2. API Endpoints

#### GET: دریافت اطلاعات پذیرش برای ویرایش
```
GET /api/v1/reception/edit/{id}
Response: ServiceResult<ReceptionEditViewModelV2>
```

#### POST: به‌روزرسانی پذیرش
```
POST /api/v1/reception/update
Request: UpdateReceptionRequest
Response: ServiceResult<UpdateReceptionResponse>
```

### 3. Business Rules

#### قوانین ویرایش بر اساس وضعیت

**Pending (در انتظار)**
- ✅ تمام فیلدها قابل ویرایش
- ✅ نیاز به بازمحاسبه قیمت‌ها
- ✅ نیاز به اعتبارسنجی کامل

**Completed (تکمیل شده)**
- ⚠️ فقط یادداشت‌ها، اولویت، نوع قابل ویرایش
- ❌ تغییر بیمار/خدمات/مبالغ ممنوع
- ⚠️ تغییرات مهم نیاز به تایید

**Cancelled (لغو شده)**
- ❌ هیچ فیلدی قابل ویرایش نیست

#### قوانین اعتبارسنجی
- ✅ بیمار باید معتبر باشد
- ✅ پزشک باید در دپارتمان انتخاب شده فعال باشد
- ✅ حداقل یک خدمت باید وجود داشته باشد
- ✅ تاریخ پذیرش نمی‌تواند در آینده باشد
- ✅ مبالغ نمی‌توانند منفی باشند

---

## 🔧 پیاده‌سازی

### مرحله 1: API Endpoints
1. `GetReceptionForEditAsync` در `IReceptionFacade`
2. `UpdateReceptionAsync` در `IReceptionFacade`
3. API endpoints در `ReceptionApiV1Controller`

### مرحله 2: Business Logic
1. `ValidateEditPermissionAsync` در `ReceptionBusinessRulesEngine`
2. `ValidateEditRulesAsync` در `ReceptionBusinessRulesEngine`
3. `RecalculateAfterEditAsync` در `ReceptionPricingService`

### مرحله 3: View و JavaScript
1. `Views/ReceptionV2/Edit.cshtml`
2. `Scripts/reception.v2/reception-edit.js`
3. Integration با modules موجود

### مرحله 4: Controller Actions
1. `ReceptionControllerV2.Edit(int id)`
2. `ReceptionControllerV2.Update(UpdateReceptionRequest)`

---

## 🔒 امنیت

### 1. Authorization
- ✅ بررسی مجوز کاربر برای ویرایش پذیرش
- ✅ بررسی مالکیت پذیرش (در صورت نیاز)
- ✅ بررسی نقش کاربر (Admin/Doctor/Receptionist)

### 2. Validation
- ✅ Server-side validation کامل
- ✅ Client-side validation برای UX بهتر
- ✅ Business rules validation

### 3. Audit Trail
- ✅ لاگ تمام تغییرات
- ✅ ذخیره تاریخچه ویرایش‌ها
- ✅ ردیابی کاربر ویرایش‌کننده

---

## 📊 Performance

### 1. بهینه‌سازی Query
- ✅ استفاده از `AsNoTracking()` برای read operations
- ✅ استفاده از projection برای کاهش حجم داده
- ✅ Lazy loading برای navigation properties

### 2. Caching
- ❌ عدم استفاده از cache (طبق قرارداد)
- ✅ استفاده از `NoCache` attribute

---

## 🧪 Testing Strategy

### 1. Unit Tests
- ✅ Business rules validation
- ✅ Permission checks
- ✅ Price recalculation

### 2. Integration Tests
- ✅ API endpoints
- ✅ Database operations
- ✅ Facade orchestration

### 3. Manual Testing
- ✅ ویرایش پذیرش Pending
- ✅ ویرایش پذیرش Completed (محدود)
- ✅ تست محدودیت‌ها و validation

---

## 📝 TODO برای PROD

- [ ] افزودن [Authorize] به Controller actions
- [ ] پیاده‌سازی approval workflow برای تغییرات مهم
- [ ] افزودن email notification برای تغییرات مهم
- [ ] پیاده‌سازی rollback mechanism
- [ ] افزودن rate limiting برای API endpoints

---

## 🚀 مراحل پیاده‌سازی

### Phase 1: Foundation (Backend)
1. ✅ طراحی ViewModel
2. ✅ پیاده‌سازی Facade methods
3. ✅ پیاده‌سازی Business Rules
4. ✅ پیاده‌سازی API endpoints

### Phase 2: Frontend
1. ✅ ایجاد View
2. ✅ پیاده‌سازی JavaScript modules
3. ✅ Integration با modules موجود
4. ✅ UI/UX improvements

### Phase 3: Testing & Refinement
1. ✅ Unit tests
2. ✅ Integration tests
3. ✅ Manual testing
4. ✅ Bug fixes

---

**تاریخ ایجاد**: 2025-01-17  
**نسخه**: 1.0  
**وضعیت**: طراحی اولیه

