# 🔍 گزارش کامل بررسی قابلیت ویرایش بیمه در پذیرش (Reception V2)

**تاریخ ایجاد**: 2024-12-20  
**نویسنده**: ClinicApp Development Team  
**هدف**: بررسی کامل قابلیت ویرایش بیمه در ماژول پذیرش نسخه 2 از Frontend تا Backend

---

## 📋 فهرست مطالب

1. [خلاصه اجرایی](#خلاصه-اجرایی)
2. [معماری و جریان داده](#معماری-و-جریان-داده)
3. [Frontend](#frontend)
4. [Backend API](#backend-api)
5. [Business Logic](#business-logic)
6. [Database & Entity](#database--entity)
7. [Validation & Security](#validation--security)
8. [مشکلات و راه‌حل‌ها](#مشکلات-و-راه‌حل‌ها)
9. [تست و تایید](#تست-و-تایید)

---

## 🎯 خلاصه اجرایی

### وضعیت فعلی
✅ **قابلیت ویرایش بیمه به طور کامل پیاده‌سازی شده است** و شامل موارد زیر می‌شود:

1. ✅ **Frontend**: 
   - Dropdown انتخاب بیمه پایه و تکمیلی
   - Auto-save هنگام تغییر
   - Update خودکار Totals
   - نمایش پیام‌های موفقیت/خطا

2. ✅ **Backend API**:
   - `POST /api/v1/reception/insurances/set` (V1)
   - `POST /Api/ReceptionApi/insurances/set` (Legacy)
   - Validation کامل
   - Anti-Forgery Protection
   - Zero-Cache Policy

3. ✅ **Business Logic**:
   - Validation پلن‌های بیمه
   - محاسبه مجدد Totals
   - Audit Trail

4. ✅ **Database**:
   - `Reception.BasePlanId` (Nullable)
   - `Reception.SupplementaryPlanId` (Nullable)
   - Indexes بهینه

### بهبودهای اعمال شده در این بررسی

1. ✅ **Validation در Facade**: 
   - بررسی وجود و فعال بودن پلن بیمه پایه
   - بررسی وجود و فعال بودن پلن بیمه تکمیلی
   - بررسی نوع بیمه (Primary vs Supplementary)

2. ✅ **Validation در Controllers**:
   - بررسی ReceptionId
   - بررسی وجود Reception
   - Logging کامل

3. ✅ **Error Handling**:
   - پیام‌های خطای واضح
   - لاگ‌های کامل
   - ServiceResult یکپارچه

---

## 🏗️ معماری و جریان داده

### Flow Diagram

```
[Frontend: insurance-panel.js]
    |
    | User changes BasePlanId or SuppPlanId
    |
    | 1. persist() called automatically (on 'change' event)
    |
    | 2. API.post('/insurances/set', { receptionId, basePlanId, supplementaryPlanId })
    |
    v
[API Wrapper: reception-api.js]
    |
    | POST /api/v1/reception/insurances/set
    | Headers: RequestVerificationToken, X-Requested-With
    | Body: JSON { receptionId, basePlanId, supplementaryPlanId }
    |
    v
[Controller: ReceptionApiV1Controller]
    |
    | [ValidateAntiForgeryTokenOnPosts]
    | Validation: ReceptionId exists, Reception exists
    |
    | Call: _facade.SetInsurancesAsync(facadeRequest)
    |
    v
[Facade: ReceptionFacade]
    |
    | 1. Get Draft (Status = Pending)
    | 2. Validate BasePlanId (if provided):
    |    - Exists in InsurancePlans
    |    - IsActive = true
    |    - IsDeleted = false
    |    - InsuranceType = Primary
    | 3. Validate SupplementaryPlanId (if provided):
    |    - Exists in InsurancePlans
    |    - IsActive = true
    |    - IsDeleted = false
    |    - InsuranceType = Supplementary
    | 4. Update Draft:
    |    - BasePlanId = request.BasePlanId
    |    - SupplementaryPlanId = request.SupplementaryPlanId
    |    - UpdatedAt = DateTime.Now
    | 5. Save Changes
    | 6. Recalculate Totals
    |
    v
[RecalculateDraftAsync]
    |
    | 1. Calculate Gross = Sum(Items.UnitPrice * Items.Quantity)
    | 2. Get BasePlan.CoveragePercent
    | 3. Get SuppPlan.CoveragePercent
    | 4. Calculate:
    |    - basePay = gross * (basePercent / 100)
    |    - patientAfterBase = gross - basePay
    |    - suppPay = patientAfterBase * (suppPercent / 100)
    |    - patient = patientAfterBase - suppPay
    | 5. Return ItemsAndTotalsDto
    |
    v
[Response: ServiceResult<ItemsAndTotalsDto>]
    |
    | {
    |   Success: true,
    |   Data: {
    |     Items: [...],
    |     Totals: {
    |       Gross: decimal,
    |       Base: decimal,
    |       Supplementary: decimal,
    |       Patient: decimal
    |     }
    |   }
    | }
    |
    v
[Frontend: insurance-panel.js]
    |
    | Update UI Totals:
    | - $('#Gross').text(U.toIRR(totals.gross))
    | - $('#InsurancePayable').text(U.toIRR(totals.base))
    | - $('#SuppPayable').text(U.toIRR(totals.supplementary))
    | - $('#PatientPayable').text(U.toIRR(totals.patient))
    |
    v
[User sees updated totals]
```

---

## 🎨 Frontend

### فایل: `Scripts/reception.v2/insurance-panel.js`

#### ساختار

```javascript
// References
const $basePlan = $('#BasePlanId');
const $suppPlan = $('#SuppPlanId');

// Functions
- loadPlans()          // بارگذاری لیست پلن‌ها از API
- set(dto)             // تنظیم مقادیر از DTO (patient-lookup)
- persist()             // ذخیره تغییرات در سرور
- removeSupplementary() // حذف بیمه تکمیلی

// Event Handlers
- $basePlan.on('change', persist)
- $suppPlan.on('change', persist)
- $btnRemoveSupp.on('click', removeSupplementary)
```

#### جریان کار

1. **Initialization**: 
   - `loadPlans()` در ابتدا صدا زده می‌شود
   - Dropdowns با لیست پلن‌ها پر می‌شوند

2. **Setting from Patient Lookup**:
   - `set(dto)` از `patient-lookup.js` صدا زده می‌شود
   - ابتدا `loadPlans()` برای اطمینان از وجود options
   - سپس مقادیر `BasePlanId` و `SupplementaryPlanId` set می‌شوند

3. **Manual Edit**:
   - کاربر dropdown را تغییر می‌دهد
   - Event `change` trigger می‌شود
   - `persist()` خودکار صدا زده می‌شود

4. **Persistence**:
   - `API.post('/insurances/set', payload)`
   - Response را parse می‌کند
   - Totals را update می‌کند

#### مشکلات احتمالی و راه‌حل‌ها

✅ **مشکل**: Response string است نه object
- **راه‌حل**: Manual `JSON.parse()` در صورت نیاز

✅ **مشکل**: Totals update نمی‌شود
- **راه‌حل**: Check برای `response.totals` یا `response.Data.totals`

✅ **مشکل**: Option در dropdown نیست
- **راه‌حل**: Value را set می‌کند حتی اگر option نباشد

---

## 🔌 Backend API

### Endpoint 1: V1 API

**Route**: `POST /api/v1/reception/insurances/set`  
**Controller**: `ReceptionApiV1Controller.SetInsurances`  
**File**: `Controllers/Api/ReceptionApiV1Controller.cs`

#### Validation

```csharp
// 1. Request Validation
if (request == null || request.ReceptionId <= 0)
    return Json(ServiceResult.Failed("درخواست نامعتبر است. ReceptionId الزامی است.", "VALIDATION"));

// 2. Reception Exists
var receptionExists = await _context.Receptions
    .AnyAsync(r => r.ReceptionId == request.ReceptionId && !r.IsDeleted);

if (!receptionExists)
    return Json(ServiceResult.Failed("پذیرش یافت نشد.", "NOT_FOUND"));
```

#### Security

- ✅ `[ValidateAntiForgeryTokenOnPosts]` - Anti-Forgery Protection
- ✅ `[OutputCache(NoStore = true, Duration = 0)]` - Zero-Cache
- ✅ `RequestVerificationToken` در Header

#### Response

```json
{
  "Success": true,
  "Data": {
    "Items": [...],
    "Totals": {
      "Gross": 100000,
      "Base": 70000,
      "Supplementary": 15000,
      "Patient": 15000
    }
  },
  "Message": "بیمه‌های پیش‌نویس با موفقیت تنظیم شد",
  "Code": "SUCCESS"
}
```

### Endpoint 2: Legacy API

**Route**: `POST /Api/ReceptionApi/insurances/set`  
**Controller**: `ReceptionApiController.SetInsurances`  
**File**: `Controllers/Api/ReceptionApiController.cs`

#### تفاوت با V1

- ✅ پشتیبانی از `SuppPlanId` (legacy name)
- ✅ همان Validation و Security

---

## 💼 Business Logic

### Facade: `ReceptionFacade.SetInsurancesAsync`

**File**: `Services/Reception/ReceptionFacade.cs`

#### Flow

```csharp
1. Get Draft (Status = Pending)
   └─ If null → return Failed("پیش‌نویس یافت نشد")

2. Validate BasePlanId (if provided)
   └─ Check exists, active, not deleted, type = Primary
   └─ If invalid → return Failed("پلن بیمه پایه یافت نشد یا غیرفعال است.")

3. Validate SupplementaryPlanId (if provided)
   └─ Check exists, active, not deleted, type = Supplementary
   └─ If invalid → return Failed("پلن بیمه تکمیلی یافت نشد یا غیرفعال است.")

4. Update Draft
   └─ BasePlanId = request.BasePlanId
   └─ SupplementaryPlanId = request.SupplementaryPlanId
   └─ UpdatedAt = DateTime.Now

5. Save Changes
   └─ await _context.SaveChangesAsync()

6. Recalculate Totals
   └─ return await RecalculateDraftAsync(draft)
```

#### Validation Details

```csharp
// BasePlanId Validation
if (request.BasePlanId.HasValue)
{
    var basePlan = await _context.InsurancePlans
        .FirstOrDefaultAsync(p => 
            p.InsurancePlanId == request.BasePlanId.Value && 
            !p.IsDeleted && 
            p.IsActive);
    
    if (basePlan == null)
        return ServiceResult<ItemsAndTotalsDto>.Failed("پلن بیمه پایه یافت نشد یا غیرفعال است.");
    
    if (basePlan.InsuranceType != InsuranceType.Primary)
        return ServiceResult<ItemsAndTotalsDto>.Failed("پلن انتخاب شده بیمه پایه نیست.");
}
```

### Recalculate Totals

**Method**: `RecalculateDraftAsync(draft)`

#### Formula

```csharp
// 1. Gross Amount
var gross = draft.ReceptionItems.Sum(i => i.UnitPrice * i.Quantity);

// 2. Get Insurance Percentages
var basePercent = basePlan?.CoveragePercent ?? 0m;
var suppPercent = suppPlan?.CoveragePercent ?? 0m;

// 3. Calculate Base Insurance Share
var basePay = gross * (basePercent / 100m);
var patientAfterBase = gross - basePay;

// 4. Calculate Supplementary Insurance Share (from remaining)
var suppPay = patientAfterBase * (suppPercent / 100m);
var patient = patientAfterBase - suppPay;
```

#### Example

```
Gross = 100,000 IRR
BasePlan Coverage = 70%
SuppPlan Coverage = 50%

basePay = 100,000 * 0.70 = 70,000
patientAfterBase = 100,000 - 70,000 = 30,000
suppPay = 30,000 * 0.50 = 15,000
patient = 30,000 - 15,000 = 15,000
```

---

## 🗄️ Database & Entity

### Entity: `Reception`

**File**: `Models/Entities/Reception/Reception.cs`

#### Fields

```csharp
public int? BasePlanId { get; set; }
public int? SupplementaryPlanId { get; set; }
```

#### Configuration

**File**: `Models/Entities/Reception/ReceptionConfig.cs`

```csharp
Property(r => r.BasePlanId)
    .IsOptional()
    .HasColumnAnnotation("Index",
        new IndexAnnotation(new IndexAttribute("IX_Reception_BasePlanId")));

Property(r => r.SupplementaryPlanId)
    .IsOptional()
    .HasColumnAnnotation("Index",
        new IndexAnnotation(new IndexAttribute("IX_Reception_SupplementaryPlanId")));
```

#### Relationships

```csharp
// Optional relationship with InsurancePlan
HasOptional(r => r.BaseInsurancePlan)
    .WithMany()
    .HasForeignKey(r => r.BasePlanId)
    .WillCascadeOnDelete(false);

HasOptional(r => r.SupplementaryInsurancePlan)
    .WithMany()
    .HasForeignKey(r => r.SupplementaryPlanId)
    .WillCascadeOnDelete(false);
```

---

## 🔒 Validation & Security

### Validation Rules

1. ✅ **ReceptionId**: 
   - Required
   - Must exist in database
   - Must not be deleted

2. ✅ **BasePlanId**:
   - Optional (nullable)
   - If provided: Must exist, active, not deleted, type = Primary

3. ✅ **SupplementaryPlanId**:
   - Optional (nullable)
   - If provided: Must exist, active, not deleted, type = Supplementary

### Security Measures

1. ✅ **Anti-Forgery Token**: 
   - `[ValidateAntiForgeryTokenOnPosts]`
   - Token در Header: `RequestVerificationToken`

2. ✅ **Zero-Cache Policy**:
   - `[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]`

3. ✅ **Input Validation**:
   - Model validation
   - Business rule validation
   - Database constraint validation

4. ✅ **Logging**:
   - All operations logged with Serilog
   - CorrelationId for tracing
   - UserName for audit

---

## 🐛 مشکلات و راه‌حل‌ها

### مشکل 1: Validation ناقص

**مشکل**: در نسخه قبلی، validation برای insurance plans وجود نداشت.

**راه‌حل**: 
- ✅ اضافه شدن validation در `ReceptionFacade.SetInsurancesAsync`
- ✅ بررسی وجود، فعال بودن، و نوع بیمه

### مشکل 2: Error Messages نامشخص

**مشکل**: پیام‌های خطا vague بودند.

**راه‌حل**:
- ✅ پیام‌های واضح و مشخص
- ✅ Logging کامل برای debugging

### مشکل 3: Frontend Totals Update

**مشکل**: Totals همیشه update نمی‌شد.

**راه‌حل**:
- ✅ Check برای `response.totals` و `response.Data.totals`
- ✅ Fallback برای هر دو ساختار

---

## ✅ تست و تایید

### Test Cases

#### Test 1: تغییر بیمه پایه

```
Given: Reception exists with BasePlanId = 1
When: User changes BasePlanId to 2
Then:
  - BasePlanId updated to 2
  - Totals recalculated
  - Response contains updated totals
  - UI shows updated totals
```

#### Test 2: اضافه کردن بیمه تکمیلی

```
Given: Reception exists with no SupplementaryPlanId
When: User selects SupplementaryPlanId = 5
Then:
  - SupplementaryPlanId updated to 5
  - Totals recalculated with supplementary
  - Response contains updated totals
  - UI shows updated totals
```

#### Test 3: حذف بیمه تکمیلی

```
Given: Reception exists with SupplementaryPlanId = 5
When: User clears SupplementaryPlanId
Then:
  - SupplementaryPlanId updated to null
  - Totals recalculated without supplementary
  - Response contains updated totals
  - UI shows updated totals
```

#### Test 4: Validation - Invalid BasePlanId

```
Given: Reception exists
When: User sets BasePlanId = 99999 (non-existent)
Then:
  - Response returns Failed("پلن بیمه پایه یافت نشد یا غیرفعال است.")
  - Draft not updated
  - UI shows error message
```

#### Test 5: Validation - Wrong Insurance Type

```
Given: Reception exists
When: User sets BasePlanId to a Supplementary plan
Then:
  - Response returns Failed("پلن انتخاب شده بیمه پایه نیست.")
  - Draft not updated
  - UI shows error message
```

---

## 📊 جمع‌بندی

### ✅ موارد پیاده‌سازی شده

1. ✅ Frontend: Complete UI with auto-save
2. ✅ Backend API: V1 and Legacy endpoints
3. ✅ Business Logic: Full validation and recalculation
4. ✅ Database: Proper entity structure with indexes
5. ✅ Security: Anti-Forgery and Zero-Cache
6. ✅ Error Handling: Comprehensive with logging
7. ✅ User Experience: Auto-update totals, clear messages

### 🔄 بهبودهای اعمال شده

1. ✅ **Validation در Facade**: بررسی کامل پلن‌های بیمه
2. ✅ **Validation در Controllers**: بررسی ReceptionId و Reception
3. ✅ **Error Messages**: پیام‌های واضح و مشخص
4. ✅ **Logging**: لاگ کامل برای debugging و audit

### 🎯 نتیجه

**قابلیت ویرایش بیمه به طور کامل و سیستماتیک پیاده‌سازی شده است و آماده استفاده در Production است.**

---

**تاریخ آخرین به‌روزرسانی**: 2024-12-20  
**نسخه**: 1.0  
**وضعیت**: ✅ Complete & Production Ready

