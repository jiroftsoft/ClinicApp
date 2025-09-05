# 📋 **ServiceResult Enhanced Contract - قرارداد کلاس بهبود یافته**

## 🎯 **هدف و مسئولیت**

این قرارداد نحوه استفاده صحیح از کلاس `ServiceResult` بهبود یافته را در پروژه کلینیک شفا تعریف می‌کند.

---

## 📋 **قوانین اجباری**

### **1. استفاده از ServiceResult**
- ✅ **اجباری:** تمام سرویس‌ها باید از `ServiceResult` یا `ServiceResult<T>` استفاده کنند
- ✅ **اجباری:** برای خطاهای اعتبارسنجی از `ValidationError` با کد استفاده شود
- ✅ **اجباری:** برای مدیریت خطاهای پیشرفته از `AdvancedValidationResult` استفاده شود

### **2. نام‌گذاری و ساختار**
- ✅ **اجباری:** از نام `AdvancedValidationResult` استفاده شود (نه `ValidationResult`)
- ✅ **اجباری:** متد `ToAdvancedServiceResult` برای تبدیل استفاده شود
- ✅ **اجباری:** از Factory Methods (`ServiceResult.Successful`, `ServiceResult.Failed`) استفاده شود

### **3. مدیریت خطا**
- ✅ **اجباری:** تمام خطاها باید کد (`Code`) داشته باشند
- ✅ **اجباری:** از `WithErrorCode` در Validator ها استفاده شود
- ✅ **اجباری:** خطاها بر اساس سطح (`ValidationErrorLevel`) دسته‌بندی شوند

---

## 🚫 **قوانین ممنوع**

### **1. استفاده مستقیم از Constructor**
- ❌ **ممنوع:** `new ServiceResult<T>(IsValid, data, message)`
- ❌ **ممنوع:** `new ValidationResult()` (تداخل نام)

### **2. نام‌های تکراری**
- ❌ **ممنوع:** استفاده از نام `ValidationResult` (تداخل با System.ComponentModel.DataAnnotations)
- ❌ **ممنوع:** ایجاد کلاس‌های تکراری ServiceResult

### **3. مدیریت خطای ساده**
- ❌ **ممنوع:** استفاده از `string` ساده برای خطاها
- ❌ **ممنوع:** عدم استفاده از کدهای خطا

---

## 🔧 **الگوهای استفاده صحیح**

### **1. ایجاد نتیجه موفق**
```csharp
// ✅ صحیح
var result = ServiceResult.Successful("عملیات با موفقیت انجام شد.");
var dataResult = ServiceResult<DoctorSchedule>.Successful(schedule);

// ❌ نادرست
var result = new ServiceResult(); // Constructor محافظت شده
```

### **2. ایجاد نتیجه ناموفق**
```csharp
// ✅ صحیح
var result = ServiceResult.Failed("خطا در عملیات", "OPERATION_ERROR");
var validationResult = ServiceResult.FailedWithValidationErrors("خطا در اعتبارسنجی", errors);

// ❌ نادرست
var result = new ServiceResult(); // Constructor محافظت شده
```

### **3. استفاده از ValidationError**
```csharp
// ✅ صحیح
var error = new ValidationError("DoctorId", "شناسه پزشک الزامی است.", "REQUIRED_FIELD");
var warning = new ValidationError("AppointmentDuration", "مدت زمان کم", "LOW_DURATION") 
{ 
    Level = ValidationErrorLevel.Warning 
};

// ❌ نادرست
var error = "خطا در فیلد"; // string ساده
```

### **4. استفاده از AdvancedValidationResult**
```csharp
// ✅ صحیح
var validationResult = new AdvancedValidationResult();
validationResult.AddError("DoctorId", "شناسه پزشک الزامی است.", "REQUIRED_FIELD");
validationResult.AddWarning("AppointmentDuration", "مدت زمان کم", "LOW_DURATION");

var serviceResult = validationResult.ToAdvancedServiceResult(data: null, message: "خطا در اعتبارسنجی");

// ❌ نادرست
var validationResult = new ValidationResult(); // نام اشتباه
var serviceResult = validationResult.ToServiceResult(); // متد اشتباه
```

### **5. استفاده در Validator ها**
```csharp
// ✅ صحیح
RuleFor(x => x.DoctorId)
    .GreaterThan(0)
    .WithMessage("شناسه پزشک نامعتبر است.")
    .WithErrorCode("INVALID_DOCTOR_ID");

// ❌ نادرست
RuleFor(x => x.DoctorId)
    .GreaterThan(0)
    .WithMessage("شناسه پزشک نامعتبر است.");
    // بدون WithErrorCode
```

---

## 🏥 **الگوهای پزشکی**

### **1. نتایج پزشکی**
```csharp
// ✅ صحیح
var result = ServiceResultFactory.MedicalSuccess(patient, "بیمار با موفقیت ثبت شد.");
var error = ServiceResultFactory.MedicalError("خطا در ثبت اطلاعات پزشکی", "MEDICAL_REGISTRATION_ERROR");

// ❌ نادرست
var result = ServiceResult.Successful("بیمار ثبت شد."); // بدون اطلاعات پزشکی
```

### **2. خطاهای اعتبارسنجی پزشکی**
```csharp
// ✅ صحیح
var result = ServiceResultFactory.MedicalValidationError<Patient>(
    "NationalCode", 
    "کد ملی نامعتبر است.", 
    "INVALID_NATIONAL_CODE"
);

// ❌ نادرست
var result = ServiceResult.Failed("خطا در اعتبارسنجی"); // بدون جزئیات پزشکی
```

---

## 📝 **نمونه‌های عملی**

### **1. در DoctorScheduleService**
```csharp
public async Task<ServiceResult<DoctorSchedule>> SetDoctorScheduleAsync(DoctorScheduleViewModel model)
{
    try
    {
        // اعتبارسنجی
        var validationResult = new AdvancedValidationResult();
        
        if (model.DoctorId <= 0)
            validationResult.AddError("DoctorId", "شناسه پزشک الزامی است.", "REQUIRED_DOCTOR_ID");
        
        if (!validationResult.IsValid)
            return validationResult.ToAdvancedServiceResult<DoctorSchedule>(null, "خطا در اعتبارسنجی");
        
        // عملیات اصلی
        var schedule = await _repository.CreateAsync(model.ToEntity());
        
        return ServiceResult<DoctorSchedule>.Successful(schedule, "برنامه کاری با موفقیت ایجاد شد.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "خطا در ایجاد برنامه کاری");
        return ServiceResult<DoctorSchedule>.Failed("خطا در ایجاد برنامه کاری", "SCHEDULE_CREATION_ERROR");
    }
}
```

### **2. در Controller**
```csharp
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
    
    // نمایش خطاهای اعتبارسنجی
    foreach (var error in result.ValidationErrors)
    {
        ModelState.AddModelError(error.Field, error.ErrorMessage);
    }
    
    return View("Schedule", model);
}
```

---

## ⚠️ **نکات مهم**

### **1. سازگاری**
- تمام متدهای قبلی حفظ شده‌اند
- هیچ breaking change وجود ندارد
- کدهای موجود بدون تغییر کار می‌کنند

### **2. عملکرد**
- Build موفق بدون خطا
- سازگار با تمام کلاس‌های موجود
- پشتیبانی کامل از سیستم‌های پزشکی

### **3. نگهداری**
- تغییرات فقط در `Helpers/ServiceResult.cs`
- مستندات در `CONTRACTS/ServiceResult_Enhanced.md`
- قرارداد در `CONTRACTS/ServiceResult_Enhanced_Contract.md`

---

## 📚 **مراجع**

- **فایل اصلی:** `Helpers/ServiceResult.cs`
- **مستندات:** `CONTRACTS/ServiceResult_Enhanced.md`
- **قرارداد:** `CONTRACTS/ServiceResult_Enhanced_Contract.md`
- **Namespace:** `ClinicApp.Helpers`

---

## 🎉 **نتیجه‌گیری**

این قرارداد تضمین می‌کند که:
1. **استفاده صحیح** از ServiceResult Enhanced
2. **مدیریت خطای پیشرفته** در تمام سرویس‌ها
3. **سازگاری کامل** با کدهای موجود
4. **استانداردسازی** نحوه مدیریت خطاها
5. **پشتیبانی پزشکی** برای نیازهای خاص کلینیک

**تاریخ ایجاد:** 2025  
**نسخه:** 1.0  
**وضعیت:** فعال و اجباری

---

## Integration with AI Compliance Contract

This contract works in conjunction with `CONTRACTS/AI_COMPLIANCE_CONTRACT.md` which defines mandatory rules for AI interactions with the ClinicApp project. All ServiceResult development work must comply with both contracts.

**Key Integration Points for ServiceResult**:
- All ServiceResult changes must follow Atomic Changes Rule (AI_COMPLIANCE_CONTRACT Section 1)

## Integration with Form Standards Contract

This contract works in conjunction with `form-standards.css` and `AI_COMPLIANCE_CONTRACT.md` (Rules 49-65) which define mandatory standards for form creation and editing. All form-related ServiceResult work must comply with these contracts.
- Pre-creation verification required for new ServiceResult methods (AI_COMPLIANCE_CONTRACT Section 2)
- No duplication of existing ServiceResult patterns (AI_COMPLIANCE_CONTRACT Section 3)
- Mandatory documentation for all ServiceResult changes (AI_COMPLIANCE_CONTRACT Section 4)
- Stop and approval process required for ServiceResult modifications (AI_COMPLIANCE_CONTRACT Section 5)
- Security and quality standards enforced (AI_COMPLIANCE_CONTRACT Section 6)
- Transparent output format required for ServiceResult change proposals (AI_COMPLIANCE_CONTRACT Section 7)
- No auto-execution of ServiceResult changes (AI_COMPLIANCE_CONTRACT Section 8)
- Project scope compliance for ServiceResult features (AI_COMPLIANCE_CONTRACT Section 9)
- Mandatory compliance with all AI interaction rules (AI_COMPLIANCE_CONTRACT Section 10)

**Reference**: See `CONTRACTS/AI_COMPLIANCE_CONTRACT.md` for complete AI interaction guidelines.
