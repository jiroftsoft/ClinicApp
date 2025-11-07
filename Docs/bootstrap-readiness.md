# ✅ **چک‌لیست آمادگی Bootstrap Endpoint - Reception V2**

**تاریخ ایجاد:** 2025-01-27  
**هدف:** بررسی و تثبیت Bootstrap endpoint برای ماژول Reception V2  
**نسخه:** 1.0.0

---

## 📋 **خلاصه اجرایی**

| مورد | وضعیت | توضیحات |
|-----|-------|---------|
| **Bootstrap Endpoint** | ✅ OK | `GET /api/v1/reception/bootstrap` کامل است |
| **PosTerminals** | ✅ OK | در Bootstrap موجود است |
| **DefaultPosTerminalId** | ✅ OK | در Bootstrap موجود است |
| **Lazy Loading Doctors** | ✅ OK | `GET /api/v1/reception/doctors/by-department` موجود است |
| **Lazy Loading by Service** | ✅ OK | `GET /api/v1/reception/doctors/by-service` موجود است |
| **FactorSetting** | ✅ OK | در Bootstrap موجود است |
| **FinancialYear** | ✅ OK | در Bootstrap موجود است |

---

## 1️⃣ **Bootstrap Endpoint**

### **1.1 Endpoint Definition**

#### ✅ **وضعیت: OK**

**Endpoint:**
- `GET /api/v1/reception/bootstrap?clinicId=&deptId=`

**Controller:**
- `Controllers/Api/ReceptionApiV1Controller.cs` → `Bootstrap(int? clinicId, int? deptId)`
- Route: `[HttpGet, Route("bootstrap")]`

**Implementation:**
- فراخوانی `ReceptionFacade.LoadInitialAsync(cid, deptId)`
- Default ClinicId = 1 (Shafa) اگر ارائه نشده باشد

---

### **1.2 Response Structure**

#### ✅ **وضعیت: OK**

**Response شامل:**

```json
{
  "Success": true,
  "Data": {
    "Clinics": [...],
    "Departments": [...],
    "Services": [...],
    "SharedServices": [...],
    "Doctors": [...],  // Optional - فقط اگر deptId ارائه شده باشد
    "FactorSetting": {
      "FinancialYear": 1403,
      "TechnicalFactor": 1.5,
      "TechnicalFactorHashtagged": 2.0,
      "ProfessionalFactor": 1.2,
      "ProfessionalFactorHashtagged": 1.8,
      "IsActive": true,
      "IsFrozen": false
    },
    "PosTerminals": [...],  // ✅ اضافه شد
    "DefaultPosTerminalId": 1,  // ✅ اضافه شد
    "FinancialYear": 1403
  }
}
```

**جزئیات:**

1. **Clinics:**
   - ✅ لیست تمام کلینیک‌های فعال
   - ✅ شامل: `ClinicId`, `Name`, `Code`, `IsActive`

2. **Departments:**
   - ✅ لیست تمام دپارتمان‌های فعال
   - ✅ شامل: `DepartmentId`, `Name`, `Code`, `IsActive`, `Description`

3. **Services:**
   - ✅ لیست خدمات دپارتمان (اگر `deptId` ارائه شده باشد)
   - ✅ شامل: `ServiceId`, `ServiceCode`, `ServiceName`, `Price`, `IsActive`, `Category`, `Description`, `IsHashtagged`

4. **SharedServices:**
   - ✅ لیست خدمات مشترک (بدون وابستگی به دپارتمان)
   - ✅ شامل: `ServiceId`, `ServiceCode`, `ServiceName`, `Price`, `IsActive`, `Category`, `Description`, `IsHashtagged`

5. **Doctors:**
   - ✅ **Optional** - فقط اگر `deptId` ارائه شده باشد
   - ✅ شامل: `DoctorId`, `FirstName`, `LastName`, `DoctorCode`, `Specialization`, `IsActive`
   - ✅ **Lazy Loading:** اگر `deptId` ارائه نشده باشد، `Doctors` خالی است و باید از `/doctors/by-department` استفاده شود

6. **FactorSetting:**
   - ✅ تنظیمات ضرایب برای سال مالی جاری
   - ✅ شامل: `FinancialYear`, `TechnicalFactor`, `TechnicalFactorHashtagged`, `ProfessionalFactor`, `ProfessionalFactorHashtagged`, `IsActive`, `IsFrozen`

7. **PosTerminals:** ✅ **اضافه شد**
   - ✅ لیست تمام ترمینال‌های POS فعال
   - ✅ شامل: `PosTerminalId`, `Id`, `Title`, `Name`, `TerminalId`, `MerchantId`, `SerialNumber`, `IpAddress`, `Port`, `MacAddress`, `Provider`, `ProviderType`, `Protocol`, `IsActive`, `IsDefault`, `ConnectionString`, `Description`

8. **DefaultPosTerminalId:** ✅ **اضافه شد**
   - ✅ شناسه ترمینال پیش‌فرض
   - ✅ `null` اگر ترمینال پیش‌فرضی وجود نداشته باشد

9. **FinancialYear:**
   - ✅ سال مالی جاری
   - ✅ از `IFinancialYearService.GetCurrentYear()` دریافت می‌شود

---

## 2️⃣ **Lazy Loading برای Doctors**

### **2.1 Endpoint: Get Doctors by Department**

#### ✅ **وضعیت: OK**

**Endpoint:**
- `GET /api/v1/reception/doctors/by-department?deptId=&clinicId=`

**Controller:**
- `Controllers/Api/ReceptionApiV1Controller.cs` → `GetDoctorsByDepartment(int deptId, int? clinicId)`
- Route: `[HttpGet, Route("doctors/by-department")]`

**Implementation:**
- فراخوانی `ReceptionFacade.GetDoctorsByDepartmentAsync(deptId, clinicId)`
- فیلترها:
  - ✅ `DepartmentId == deptId`
  - ✅ `Doctor.IsActive == true && !Doctor.IsDeleted`
  - ✅ `DoctorDepartment.IsActive == true && !DoctorDepartment.IsDeleted`
  - ✅ `DoctorDepartment.EndDate == null || DoctorDepartment.EndDate > now` (فقط EndDate چک می‌شود، StartDate ignore می‌شود)

**Response:**
```json
{
  "Success": true,
  "Data": {
    "doctors": [
      {
        "DoctorId": 1,
        "FirstName": "علی",
        "LastName": "احمدی",
        "DoctorCode": "DOC001",
        "Specialization": "متخصص قلب",
        "IsActive": true
      }
    ]
  }
}
```

**Frontend:**
- ✅ `Scripts/reception.v2/clinic-dept-doctor.js` → `loadDoctorsForDepartment(deptId)`
- ✅ فراخوانی در `onChange` event از `#DepartmentId`
- ✅ استفاده از `API.get("/doctors/by-department", { deptId, clinicId })`

---

### **2.2 Endpoint: Get Doctors by Service**

#### ✅ **وضعیت: OK**

**Endpoint:**
- `GET /api/v1/reception/doctors/by-service?deptId=&serviceId=&clinicId=`

**Controller:**
- `Controllers/Api/ReceptionApiV1Controller.cs` → `GetDoctorsByService(int departmentId, int serviceId, int? clinicId)`
- Route: `[HttpGet, Route("doctors/by-service")]`

**Implementation:**
- فراخوانی `ReceptionFacade.GetDoctorsByServiceAsync(departmentId, serviceId, clinicId)`
- فیلترها:
  - ✅ `DoctorServiceCategory.ServiceCategoryId == serviceId`
  - ✅ `DoctorDepartment.DepartmentId == departmentId`
  - ✅ `Doctor.IsActive == true && !Doctor.IsDeleted`
  - ✅ `DoctorDepartment.IsActive == true && !DoctorDepartment.IsDeleted`
  - ✅ `DoctorServiceCategory.IsActive == true && !DoctorServiceCategory.IsDeleted`

**Response:**
```json
{
  "Success": true,
  "Data": {
    "doctors": [
      {
        "DoctorId": 1,
        "FirstName": "علی",
        "LastName": "احمدی",
        "DoctorCode": "DOC001",
        "Specialization": "متخصص قلب",
        "IsActive": true
      }
    ]
  }
}
```

**Frontend:**
- ✅ `Scripts/reception.v2/clinic-dept-doctor.js` → `window.loadDoctorsByService({ serviceId, deptId, clinicId })`
- ✅ فراخوانی در `onChange` event از `#ServiceId` (در `service-lookup.js`)

---

## 3️⃣ **Frontend Integration**

### **3.1 Bootstrap Call**

#### ✅ **وضعیت: OK**

**Script:**
- `Scripts/reception.v2/clinic-dept-doctor.js` → `bootstrap(reloadDepartments)`

**Implementation:**
- ✅ فراخوانی `API.get("/bootstrap", { clinicId, deptId })`
- ✅ پردازش پاسخ و پر کردن Dropdown ها:
  - `#ClinicId` → از `response.Clinics`
  - `#DepartmentId` → از `response.Departments`
  - `#DoctorId` → از `response.Doctors` (اگر `deptId` موجود باشد)
- ✅ ذخیره `FinancialYear` در `window.ReceptionBootstrap.FinancialYear`
- ✅ Trigger event `rv2:stateChanged` برای Summary Header

**View:**
- ✅ `Views/ReceptionV2/Index.cshtml` → `window.ReceptionBootstrap = @Html.Raw(...)`
- ✅ Bootstrap data در `Model.Bootstrap` از Controller دریافت می‌شود

---

### **3.2 Lazy Loading Doctors**

#### ✅ **وضعیت: OK**

**onChange Department:**
- ✅ `$("#DepartmentId").on('change', ...)` → `loadDoctorsForDepartment(deptId)`
- ✅ فراخوانی `API.get("/doctors/by-department", { deptId, clinicId })`
- ✅ پر کردن `#DoctorId` از پاسخ
- ✅ Trigger event `rv2:stateChanged` برای Summary Header

**onChange Service:**
- ✅ `window.loadDoctorsByService({ serviceId, deptId, clinicId })`
- ✅ فراخوانی `API.get("/doctors/by-service", { deptId, serviceId, clinicId })`
- ✅ پر کردن `#DoctorId` از پاسخ
- ✅ Validation: اگر پزشک انتخاب شده برای خدمت مجاز نیست، هشدار می‌دهد

---

## 4️⃣ **PosTerminals Integration**

### **4.1 Backend Implementation**

#### ✅ **وضعیت: OK**

**ReceptionFacade.LoadInitialAsync:**
- ✅ بارگذاری ترمینال‌های POS فعال از `IPosManagementService.GetActivePosTerminalsAsync()`
- ✅ بارگذاری ترمینال پیش‌فرض از `IPosManagementService.GetDefaultPosTerminalAsync()`
- ✅ Map کردن به `PosTerminalDto`
- ✅ Error Handling: در صورت خطا، `PosTerminals` خالی و `DefaultPosTerminalId` null می‌شود

**ReceptionApiV1Controller.Bootstrap:**
- ✅ اضافه کردن `PosTerminals` به payload
- ✅ اضافه کردن `DefaultPosTerminalId` به payload
- ✅ Logging برای PosTerminals

---

### **4.2 Frontend Usage**

#### ⚠️ **وضعیت: نیازمند بررسی**

**بررسی:**
- ⚠️ `Scripts/reception.v2/clinic-dept-doctor.js` از `PosTerminals` استفاده نمی‌کند
- ⚠️ `Scripts/reception.v2/payment-panel.js` باید از `PosTerminals` و `DefaultPosTerminalId` استفاده کند
- ⚠️ `Views/ReceptionV2/Partials/_Payment.cshtml` باید از `PosTerminals` استفاده کند

**TODO:**
- ⚠️ بررسی `payment-panel.js` برای استفاده از `PosTerminals`
- ⚠️ بررسی `_Payment.cshtml` برای نمایش `PosTerminals`

---

## 5️⃣ **بهینه‌سازی Bootstrap**

### **5.1 Doctors در Bootstrap**

#### ✅ **وضعیت: OK - Lazy Loading فعال**

**استراتژی:**
- ✅ اگر `deptId` ارائه شده باشد، Doctors در Bootstrap لود می‌شود
- ✅ اگر `deptId` ارائه نشده باشد، Doctors خالی است و باید از `/doctors/by-department` استفاده شود
- ✅ این رویکرد باعث می‌شود Bootstrap سبک‌تر باشد و فقط در صورت نیاز Doctors لود شود

**مزایا:**
- ✅ کاهش حجم پاسخ Bootstrap
- ✅ بهبود Performance
- ✅ کاهش بار سرور

---

### **5.2 Services در Bootstrap**

#### ✅ **وضعیت: OK - Conditional Loading**

**استراتژی:**
- ✅ اگر `deptId` ارائه شده باشد، Services دپارتمان در Bootstrap لود می‌شود
- ✅ اگر `deptId` ارائه نشده باشد، Services خالی است
- ✅ SharedServices همیشه لود می‌شود (بدون وابستگی به دپارتمان)

---

## 6️⃣ **Error Handling**

### **6.1 Fallback Payload**

#### ✅ **وضعیت: OK**

**Implementation:**
- ✅ اگر `LoadInitialAsync` ناموفق باشد، Fallback payload با ساختار حداقلی برگردانده می‌شود
- ✅ تمام فیلدها به صورت Empty List یا null برگردانده می‌شوند
- ✅ `PosTerminals` و `DefaultPosTerminalId` در Fallback موجود هستند

---

## 📊 **خلاصه وضعیت**

| مورد | وضعیت | جزئیات |
|-----|-------|---------|
| **Bootstrap Endpoint** | ✅ OK | `GET /api/v1/reception/bootstrap` کامل است |
| **Clinics** | ✅ OK | در Bootstrap موجود است |
| **Departments** | ✅ OK | در Bootstrap موجود است |
| **Services** | ✅ OK | Conditional - فقط اگر deptId موجود باشد |
| **SharedServices** | ✅ OK | همیشه در Bootstrap موجود است |
| **Doctors** | ✅ OK | Optional - Lazy Loading فعال است |
| **FactorSetting** | ✅ OK | در Bootstrap موجود است |
| **PosTerminals** | ✅ OK | در Bootstrap موجود است (اضافه شد) |
| **DefaultPosTerminalId** | ✅ OK | در Bootstrap موجود است (اضافه شد) |
| **FinancialYear** | ✅ OK | در Bootstrap موجود است |
| **Lazy Loading Doctors** | ✅ OK | `/doctors/by-department` موجود است |
| **Lazy Loading by Service** | ✅ OK | `/doctors/by-service` موجود است |
| **Frontend Integration** | ✅ OK | `clinic-dept-doctor.js` کامل است |
| **Error Handling** | ✅ OK | Fallback payload موجود است |

---

## 🎯 **اقدامات انجام شده**

### **✅ تکمیل شده:**
1. ✅ اضافه کردن `PosTerminalDto` به `ReceptionFacadeDtos.cs`
2. ✅ اضافه کردن `PosTerminals` و `DefaultPosTerminalId` به `ReceptionLoadDto`
3. ✅ به‌روزرسانی `ReceptionFacade.LoadInitialAsync` برای لود کردن PosTerminals
4. ✅ به‌روزرسانی `ReceptionApiV1Controller.Bootstrap` برای اضافه کردن PosTerminals به payload
5. ✅ بررسی Lazy Loading برای Doctors

### **⚠️ نیازمند بررسی (فاز F):**
1. ⚠️ بررسی `payment-panel.js` برای استفاده از `PosTerminals`
2. ⚠️ بررسی `_Payment.cshtml` برای نمایش `PosTerminals`
3. ⚠️ بررسی `pos-payment.js` برای استفاده از `DefaultPosTerminalId`

---

## 🔍 **نقاط قوت**

1. ✅ **Bootstrap کامل:** تمام داده‌های لازم در یک درخواست
2. ✅ **Lazy Loading:** Doctors به صورت Lazy لود می‌شوند
3. ✅ **PosTerminals:** ترمینال‌های POS در Bootstrap موجود هستند
4. ✅ **Error Handling:** Fallback payload برای خطاها
5. ✅ **Performance:** Bootstrap سبک و بهینه

---

## 📝 **نکات مهم**

1. **Doctors در Bootstrap:**
   - اگر `deptId` ارائه شده باشد، Doctors لود می‌شود
   - اگر `deptId` ارائه نشده باشد، Doctors خالی است و باید از `/doctors/by-department` استفاده شود

2. **PosTerminals:**
   - همیشه در Bootstrap لود می‌شود (فقط ترمینال‌های فعال)
   - `DefaultPosTerminalId` ممکن است `null` باشد اگر ترمینال پیش‌فرضی وجود نداشته باشد

3. **FactorSetting:**
   - برای سال مالی جاری لود می‌شود
   - ممکن است `null` باشد اگر تنظیمات ضرایب وجود نداشته باشد

---

## 🧪 **سناریوهای تست**

### **1. Bootstrap بدون deptId:**
```
GET /api/v1/reception/bootstrap?clinicId=1
→ Response باید شامل: Clinics, Departments, SharedServices, FactorSetting, PosTerminals, DefaultPosTerminalId, FinancialYear
→ Response باید شامل: Doctors = [] (خالی)
```

### **2. Bootstrap با deptId:**
```
GET /api/v1/reception/bootstrap?clinicId=1&deptId=5
→ Response باید شامل: Clinics, Departments, Services, SharedServices, Doctors, FactorSetting, PosTerminals, DefaultPosTerminalId, FinancialYear
→ Doctors باید شامل پزشکان دپارتمان 5 باشد
```

### **3. Lazy Loading Doctors:**
```
onChange(#DepartmentId) → loadDoctorsForDepartment(deptId)
→ GET /api/v1/reception/doctors/by-department?deptId=5&clinicId=1
→ Response باید شامل: { doctors: [...] }
→ #DoctorId باید پر شود
```

### **4. Lazy Loading by Service:**
```
onChange(#ServiceId) → loadDoctorsByService({ serviceId, deptId, clinicId })
→ GET /api/v1/reception/doctors/by-service?deptId=5&serviceId=10&clinicId=1
→ Response باید شامل: { doctors: [...] }
→ #DoctorId باید پر شود (فقط پزشکان مجاز برای خدمت)
```

---

**تاریخ به‌روزرسانی:** 2025-01-27  
**نسخه:** 1.0.0  
**وضعیت:** ✅ فاز C تکمیل شد

