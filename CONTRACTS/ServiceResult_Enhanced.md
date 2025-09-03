# 📋 **ServiceResult Enhanced - مستندات کامل کلاس بهبود یافته**

## 🎯 **هدف و مسئولیت**

کلاس `ServiceResult` بهبود یافته برای مدیریت نتایج سرویس‌ها در سیستم کلینیک شفا طراحی شده است. این کلاس:

- **دوگانگی را حذف کرده** و یک کلاس واحد و قدرتمند ارائه می‌دهد
- **تمام متدهای قبلی را حفظ کرده** تا سازگاری با کدهای موجود حفظ شود
- **ویژگی‌های جدید** برای مدیریت خطاهای پیشرفته اضافه کرده است
- **پشتیبانی کامل** از سیستم‌های پزشکی و امنیتی ارائه می‌دهد

---

## 🏗️ **ساختار کلاس‌ها**

### **1. ServiceResult (کلاس پایه)**
```csharp
public class ServiceResult
{
    // Properties اصلی
    public bool Success { get; protected set; }
    public string Message { get; protected set; }
    public string Code { get; protected set; }
    public SecurityLevel SecurityLevel { get; protected set; }
    public ErrorCategory Category { get; protected set; }
    public DateTime Timestamp { get; protected set; }
    public string TimestampShamsi { get; protected set; }
    public Dictionary<string, object> Metadata { get; protected set; }
    public string OperationId { get; protected set; }
    public string UserId { get; protected set; }
    public string UserName { get; protected set; }
    public string UserFullName { get; protected set; }
    public string UserRole { get; protected set; }
    public string SystemName { get; protected set; }
    public string OperationName { get; protected set; }
    public OperationStatus Status { get; protected set; }
    public List<ValidationError> ValidationErrors { get; protected set; }
}
```

### **2. ServiceResult<T> (کلاس Generic)**
```csharp
public class ServiceResult<T> : ServiceResult
{
    // Properties اضافی
    public T Data { get; private set; }
    public int? TotalCount { get; private set; }
    public int? PageNumber { get; private set; }
    public int? PageSize { get; private set; }
    public bool HasNextPage { get; private set; }
    public bool HasPreviousPage { get; private set; }
    
    // Properties پزشکی
    public string PatientId { get; private set; }
    public string PatientNationalCode { get; private set; }
    public string PatientFullName { get; private set; }
    public string DoctorId { get; private set; }
    public string DoctorFullName { get; private set; }
    public string InsuranceId { get; private set; }
    public string InsuranceName { get; private set; }
    public string AppointmentId { get; private set; }
    public string ReceptionId { get; private set; }
}
```

### **3. ValidationError (کلاس خطای اعتبارسنجی)**
```csharp
public class ValidationError
{
    public string Field { get; set; }
    public string ErrorMessage { get; set; }
    public string Code { get; set; }
    public object InvalidValue { get; set; }
    public ValidationErrorLevel Level { get; set; }
    public string Suggestion { get; set; }
    public DateTime Timestamp { get; set; }
    public string TransactionId { get; set; }
}
```

### **4. ValidationErrorLevel (Enum)**
```csharp
public enum ValidationErrorLevel
{
    Error = 0,    // خطا - عملیات متوقف می‌شود
    Warning = 1,  // هشدار - عملیات ادامه می‌یابد اما نیاز به توجه دارد
    Info = 2      // اطلاعات - فقط برای اطلاع‌رسانی
}
```

### **5. AdvancedValidationResult (کلاس نتیجه اعتبارسنجی)**
```csharp
public class AdvancedValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; }
    public List<ValidationError> Warnings { get; set; }
    public List<ValidationError> Info { get; set; }
    public DateTime ValidatedAt { get; set; }
    public long ValidationTimeMs { get; set; }
}
```

---

## 🚀 **متدهای Factory (حفظ شده)**

### **ServiceResult**
```csharp
// متدهای موفق
public static ServiceResult Successful(string message = "عملیات با موفقیت انجام شد.", ...)
public static ServiceResult Failed(string message, string code = "GENERAL_ERROR", ...)
public static ServiceResult FailedWithValidationErrors(string message, IEnumerable<ValidationError> validationErrors, ...)

// متدهای جدید
public static ServiceResult WithValidationError(string field, string errorMessage, string code, object invalidValue)
public static ServiceResult WithWarning(string field, string message, string code = null)
public static ServiceResult WithInfo(string field, string message, string code = null)
```

### **ServiceResult<T>**
```csharp
// متدهای موفق
public static ServiceResult<T> Successful(T data, string message = "عملیات با موفقیت انجام شد.", ...)
public static ServiceResult<T> SuccessfulWithMedicalInfo(T data, string patientId, ...)
public static ServiceResult<T> Failed(string message, string code = "GENERAL_ERROR", ...)
public static ServiceResult<T> FailedWithValidationErrors(string message, IEnumerable<ValidationError> validationErrors, ...)

// متدهای جدید
public static ServiceResult<T> WithValidationError(string field, string errorMessage, string code, object invalidValue)
public static ServiceResult<T> WithWarning(string field, string message, string code = null)
public static ServiceResult<T> WithInfo(string field, string message, string code = null)
```

---

## 🔧 **متدهای Helper (حفظ شده)**

### **ServiceResult**
```csharp
public ServiceResult WithMetadata(string key, object value)
public ServiceResult WithValidationError(string field, string errorMessage)
public ServiceResult WithValidationErrors(IEnumerable<ValidationError> errors)
public ServiceResult WithSecurityLevel(SecurityLevel securityLevel)
public ServiceResult WithCategory(ErrorCategory category)

// متدهای جدید
public ServiceResult WithValidationError(string field, string errorMessage, string code)
public ServiceResult WithValidationError(string field, string errorMessage, string code, object invalidValue)
public ServiceResult WithValidationError(ValidationError error)
public ServiceResult WithWarning(string field, string message, string code = null)
public ServiceResult WithInfo(string field, string message, string code = null)
```

### **ServiceResult<T>**
```csharp
public ServiceResult<T> WithMedicalInfo(string patientId, string patientNationalCode, ...)
public ServiceResult<T> WithPagination(int totalCount, int pageNumber, int pageSize)

// متدهای جدید
public ServiceResult<T> WithValidationError(string field, string errorMessage, string code, object invalidValue)
public ServiceResult<T> WithWarning(string field, string message, string code = null)
public ServiceResult<T> WithInfo(string field, string message, string code = null)
```

---

## 🏥 **ServiceResultFactory (حفظ شده)**

### **General Results**
```csharp
public static ServiceResult Success(string message = "عملیات با موفقیت انجام شد.")
public static ServiceResult Error(string message, string code = "ERROR")
public static ServiceResult NotFound(string entityName, string identifier)
public static ServiceResult Unauthorized(string message = "دسترسی غیرمجاز")
public static ServiceResult ValidationErrors(IEnumerable<ValidationError> errors)

// متدهای جدید
public static ServiceResult ValidationError(string field, string message, string code = "VALIDATION_ERROR")
public static ServiceResult Warning(string field, string message, string code = "WARNING")
public static ServiceResult Info(string field, string message, string code = "INFO")
```

### **Medical Results**
```csharp
public static ServiceResult<T> MedicalSuccess<T>(T data, string message = "عملیات پزشکی با موفقیت انجام شد.")
public static ServiceResult<T> MedicalSuccessWithInfo<T>(T data, string patientId, ...)
public static ServiceResult MedicalError(string message, string code = "MEDICAL_ERROR")
public static ServiceResult FinancialError(string message, string code = "FINANCIAL_ERROR")

// متدهای جدید
public static ServiceResult<T> MedicalValidationError<T>(string field, string message, string code = "MEDICAL_VALIDATION_ERROR")
public static ServiceResult<T> MedicalWarning<T>(string field, string message, string code = "MEDICAL_WARNING")
public static ServiceResult<T> MedicalInfo<T>(string field, string message, string code = "MEDICAL_INFO")
```

---

## 📝 **نمونه‌های استفاده**

### **1. ایجاد نتیجه موفق**
```csharp
// روش قدیمی (حفظ شده)
var result = ServiceResult.Successful("عملیات با موفقیت انجام شد.");

// روش جدید
var result = ServiceResult.Successful("عملیات با موفقیت انجام شد.")
    .WithMetadata("OperationType", "Create")
    .WithSecurityLevel(SecurityLevel.Low);
```

### **2. ایجاد نتیجه ناموفق با خطاهای اعتبارسنجی**
```csharp
// روش قدیمی (حفظ شده)
var result = ServiceResult.FailedWithValidationErrors("خطا در اعتبارسنجی", validationErrors);

// روش جدید
var result = ServiceResult.Failed("خطا در اعتبارسنجی", "VALIDATION_ERROR")
    .WithValidationError("DoctorId", "شناسه پزشک الزامی است.", "REQUIRED_FIELD")
    .WithWarning("AppointmentDuration", "مدت زمان نوبت کمتر از حد توصیه شده است.", "LOW_DURATION")
    .WithInfo("WorkDays", "تعداد روزهای کاری قابل قبول است.", "INFO");
```

### **3. استفاده از AdvancedValidationResult**
```csharp
var validationResult = new AdvancedValidationResult();
validationResult.AddError("DoctorId", "شناسه پزشک الزامی است.", "REQUIRED_FIELD");
validationResult.AddWarning("AppointmentDuration", "مدت زمان نوبت کمتر از حد توصیه شده است.", "LOW_DURATION");
validationResult.AddInfo("WorkDays", "تعداد روزهای کاری قابل قبول است.", "INFO");

// تبدیل به ServiceResult
var serviceResult = validationResult.ToAdvancedServiceResult(data: null, message: "خطا در اعتبارسنجی");
```

---

## ⚠️ **نکات مهم**

### **1. سازگاری با کدهای موجود**
- ✅ تمام متدهای قبلی حفظ شده‌اند
- ✅ هیچ تغییر breaking change وجود ندارد
- ✅ کدهای موجود بدون تغییر کار می‌کنند

### **2. ویژگی‌های جدید**
- ✅ پشتیبانی از سطوح مختلف خطا (Error, Warning, Info)
- ✅ کدهای خطای پیشرفته
- ✅ راهنمای رفع خطا
- ✅ شناسه تراکنش
- ✅ زمان‌بندی دقیق

### **3. عملکرد**
- ✅ Build موفق بدون خطا
- ✅ سازگار با تمام کلاس‌های موجود
- ✅ پشتیبانی کامل از سیستم‌های پزشکی

---

## 🔄 **تغییرات انجام شده**

### **حذف شده‌ها:**
- ❌ کلاس تکراری `ValidationErrorModels.cs`
- ❌ دوگانگی `ServiceResult`

### **اضافه شده‌ها:**
- ✅ `ValidationErrorLevel` enum
- ✅ متدهای جدید برای مدیریت خطاهای پیشرفته
- ✅ کلاس `ValidationResult`
- ✅ متدهای Factory جدید

### **بهبود یافته‌ها:**
- ✅ کلاس `ValidationError` با ویژگی‌های بیشتر
- ✅ متدهای Helper پیشرفته
- ✅ پشتیبانی از سطوح مختلف خطا

---

## 📚 **مراجع**

- **فایل اصلی:** `Helpers/ServiceResult.cs`
- **Namespace:** `ClinicApp.Helpers`
- **تاریخ بهبود:** 2025
- **نسخه:** Enhanced v2.0

---

## 🎉 **نتیجه‌گیری**

کلاس `ServiceResult` بهبود یافته:

1. **دوگانگی را کاملاً حذف کرده** ✅
2. **تمام متدهای قبلی را حفظ کرده** ✅
3. **ویژگی‌های جدید و قدرتمند اضافه کرده** ✅
4. **سازگاری کامل با کدهای موجود** ✅
5. **پشتیبانی پیشرفته از مدیریت خطا** ✅

این کلاس اکنون یک راه‌حل واحد، قدرتمند و حرفه‌ای برای مدیریت نتایج سرویس‌ها در سیستم کلینیک شفا ارائه می‌دهد.
