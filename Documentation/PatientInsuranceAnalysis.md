# 📋 تحلیل کامل ماژول PatientInsurance

## 🎯 مقدمه

این سند شامل تحلیل کامل ماژول `PatientInsurance` است که **بسیار مهم** است و برای **انتصاب بیمه به بیمار** استفاده می‌شود. این ماژول شامل:
1. **ساختار Entity و Model**
2. **منطق انتصاب بیمه به بیمار**
3. **منطق مدیریت بیمه اصلی و تکمیلی**
4. **منطق Priority و اولویت‌بندی**
5. **Validation و Business Rules**
6. **Views و UI/UX**
7. **مشکلات احتمالی و راه‌حل‌ها**

---

## 📌 ساختار Entity

### PatientInsurance Entity:

```csharp
public class PatientInsurance : ISoftDelete, ITrackable
{
    public int PatientInsuranceId { get; set; }
    public int PatientId { get; set; }
    public int InsurancePlanId { get; set; }
    public string PolicyNumber { get; set; }
    public string SupplementaryPolicyNumber { get; set; }
    public int InsuranceProviderId { get; set; }
    public int? SupplementaryInsuranceProviderId { get; set; }
    public int? SupplementaryInsurancePlanId { get; set; }
    public string CardNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public InsurancePriority Priority { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string DeletedByUserId { get; set; }
    
    // Audit Trail
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedByUserId { get; set; }
    
    // Navigation Properties
    public virtual Patient Patient { get; set; }
    public virtual InsuranceProvider InsuranceProvider { get; set; }
    public virtual InsurancePlan InsurancePlan { get; set; }
    public virtual InsuranceProvider SupplementaryInsuranceProvider { get; set; }
    public virtual InsurancePlan SupplementaryInsurancePlan { get; set; }
}
```

### ویژگی‌های کلیدی:
- **پشتیبانی از بیمه اصلی و تکمیلی:** هر دو در یک Entity
- **Soft Delete:** حفظ اطلاعات برای Audit
- **Audit Trail:** ردیابی کامل تغییرات
- **Priority:** مدیریت اولویت‌ها با Enum

---

## 🔧 منطق انتصاب بیمه به بیمار

### 1. ایجاد بیمه جدید (Create):

#### الگوریتم:
```csharp
public async Task<ServiceResult<int>> CreateAsync(PatientInsuranceCreateEditViewModel model)
{
    // 1. تبدیل ViewModel به Entity
    var patientInsurance = ConvertToEntity(model);
    
    // 2. تنظیم فیلدهای Audit
    patientInsurance.CreatedAt = DateTime.UtcNow;
    patientInsurance.CreatedByUserId = _currentUserService.UserId;
    patientInsurance.IsActive = true;
    patientInsurance.IsDeleted = false;
    
    // 3. تنظیم خودکار Priority
    if (model.IsPrimary)
    {
        patientInsurance.Priority = InsurancePriority.Primary;
    }
    else
    {
        var existingInsurances = await _patientInsuranceRepository.GetByPatientIdAsync(model.PatientId);
        var existingPriorities = existingInsurances.Where(pi => !pi.IsPrimary).Select(pi => pi.Priority);
        patientInsurance.Priority = InsurancePriorityHelper.GetNextSupplementaryPriority(existingPriorities);
    }
    
    // 4. ذخیره در Repository
    _patientInsuranceRepository.Add(patientInsurance);
    await _patientInsuranceRepository.SaveChangesAsync();
    
    return ServiceResult<int>.Successful(patientInsurance.PatientInsuranceId);
}
```

#### کلاس مسئول:
- **`PatientInsuranceService.CreateAsync`**
- **`PatientInsuranceController.Create`**

### 2. به‌روزرسانی بیمه (Update):

#### الگوریتم:
```csharp
public async Task<ServiceResult<bool>> UpdateAsync(PatientInsuranceCreateEditViewModel model)
{
    // 1. دریافت Entity موجود
    var existingPatientInsurance = await _patientInsuranceRepository.GetByIdAsync(model.PatientInsuranceId);
    
    // 2. به‌روزرسانی فیلدها
    existingPatientInsurance.PatientId = model.PatientId;
    existingPatientInsurance.InsurancePlanId = model.InsurancePlanId;
    existingPatientInsurance.PolicyNumber = model.PolicyNumber;
    existingPatientInsurance.IsPrimary = model.IsPrimary;
    existingPatientInsurance.StartDate = model.StartDate;
    existingPatientInsurance.EndDate = model.EndDate;
    existingPatientInsurance.IsActive = model.IsActive;
    
    // 3. به‌روزرسانی فیلدهای بیمه تکمیلی
    existingPatientInsurance.SupplementaryInsuranceProviderId = model.SupplementaryInsuranceProviderId;
    existingPatientInsurance.SupplementaryInsurancePlanId = model.SupplementaryInsurancePlanId;
    existingPatientInsurance.SupplementaryPolicyNumber = model.SupplementaryPolicyNumber;
    
    // 4. تنظیم فیلدهای Audit
    existingPatientInsurance.UpdatedAt = DateTime.UtcNow;
    existingPatientInsurance.UpdatedByUserId = _currentUserService.UserId;
    
    // 5. ذخیره در Repository
    _patientInsuranceRepository.Update(existingPatientInsurance);
    await _patientInsuranceRepository.SaveChangesAsync();
    
    return ServiceResult<bool>.Successful(true);
}
```

#### کلاس مسئول:
- **`PatientInsuranceService.UpdateAsync`**
- **`PatientInsuranceController.Edit`**

### 3. حذف نرم (Soft Delete):

#### الگوریتم:
```csharp
public async Task<ServiceResult<bool>> DeleteAsync(int id)
{
    // 1. دریافت Entity
    var patientInsurance = await _patientInsuranceRepository.GetByIdAsync(id);
    
    // 2. حذف نرم
    patientInsurance.IsDeleted = true;
    patientInsurance.IsActive = false;
    patientInsurance.UpdatedAt = DateTime.UtcNow;
    patientInsurance.UpdatedByUserId = _currentUserService.UserId;
    
    // 3. ذخیره در Repository
    _patientInsuranceRepository.Update(patientInsurance);
    await _patientInsuranceRepository.SaveChangesAsync();
    
    return ServiceResult<bool>.Successful(true);
}
```

#### کلاس مسئول:
- **`PatientInsuranceService.DeleteAsync`**
- **`PatientInsuranceController.Delete`**

---

## 🏥 منطق مدیریت بیمه اصلی و تکمیلی

### 1. تنظیم بیمه اصلی (SetPrimaryInsurance):

#### الگوریتم:
```csharp
public async Task<ServiceResult> SetPrimaryInsuranceAsync(int patientInsuranceId)
{
    // 1. دریافت بیمه بیمار
    var patientInsurance = await _patientInsuranceRepository.GetByIdAsync(patientInsuranceId);
    
    // 2. بررسی فعال بودن
    if (!patientInsurance.IsActive)
    {
        return ServiceResult.Failed("بیمه غیرفعال نمی‌تواند بیمه اصلی باشد");
    }
    
    // 3. استفاده از Transaction
    using (var transaction = await _patientInsuranceRepository.BeginTransactionAsync())
    {
        try
        {
            // 4. حذف وضعیت اصلی از سایر بیمه‌های بیمار
            var otherInsurances = await _patientInsuranceRepository.GetByPatientIdAsync(patientInsurance.PatientId);
            foreach (var insurance in otherInsurances.Where(i => i.PatientInsuranceId != patientInsuranceId && i.IsPrimary))
            {
                insurance.IsPrimary = false;
                insurance.UpdatedByUserId = _currentUserService.GetCurrentUserId();
                insurance.UpdatedAt = DateTime.UtcNow;
                _patientInsuranceRepository.Update(insurance);
            }
            
            // 5. تنظیم بیمه جدید به عنوان اصلی
            patientInsurance.IsPrimary = true;
            patientInsurance.UpdatedByUserId = _currentUserService.GetCurrentUserId();
            patientInsurance.UpdatedAt = DateTime.UtcNow;
            _patientInsuranceRepository.Update(patientInsurance);
            
            // 6. Commit Transaction
            transaction.Commit();
            
            return ServiceResult.Successful("بیمه اصلی با موفقیت تنظیم شد");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

#### کلاس مسئول:
- **`PatientInsuranceService.SetPrimaryInsuranceAsync`**
- **`PatientInsuranceController.SetPrimaryInsurance`**

### 2. دریافت بیمه اصلی:

#### الگوریتم:
```csharp
public async Task<PatientInsurance> GetPrimaryByPatientIdAsync(int patientId)
{
    return await _context.PatientInsurances
        .Where(pi => pi.PatientId == patientId && pi.IsPrimary && pi.IsActive)
        .Include(pi => pi.InsurancePlan.InsuranceProvider)
        .AsNoTracking()
        .FirstOrDefaultAsync();
}
```

#### کلاس مسئول:
- **`PatientInsuranceRepository.GetPrimaryByPatientIdAsync`**
- **`PatientInsuranceService.GetPrimaryByPatientIdAsync`**

### 3. دریافت بیمه‌های تکمیلی:

#### الگوریتم:
```csharp
public async Task<List<PatientInsurance>> GetSupplementaryByPatientIdAsync(int patientId)
{
    // 🚨 CRITICAL FIX: منطق صحیح برای سیستم درمانی
    // بیمه‌های تکمیلی: 
    // 1. رکوردهایی که IsPrimary = false هستند (رکوردهای جداگانه)
    // 2. رکوردهایی که IsPrimary = true اما SupplementaryInsuranceProviderId دارند (فیلدهای تکمیلی)
    return await _context.PatientInsurances
        .Where(pi => pi.PatientId == patientId 
                 && pi.IsActive
                 && !pi.IsDeleted
                 && (
                     pi.IsPrimary == false || // رکوردهای جداگانه بیمه تکمیلی
                     (pi.IsPrimary == true && pi.SupplementaryInsuranceProviderId.HasValue && pi.SupplementaryInsurancePlanId.HasValue) // فیلدهای تکمیلی در بیمه اصلی
                 ))
        .Include(pi => pi.InsurancePlan)
        .Include(pi => pi.SupplementaryInsuranceProvider)
        .Include(pi => pi.SupplementaryInsurancePlan)
        .Include(pi => pi.Patient)
        .OrderBy(pi => pi.Priority)
        .ThenBy(pi => pi.StartDate)
        .AsNoTracking()
        .ToListAsync();
}
```

#### کلاس مسئول:
- **`PatientInsuranceRepository.GetSupplementaryByPatientIdAsync`**
- **`PatientInsuranceService.GetSupplementaryByPatientIdAsync`**

---

## 📊 منطق Priority و اولویت‌بندی

### InsurancePriority Enum:

```csharp
public enum InsurancePriority
{
    Primary = 1,        // بیمه اصلی
    Secondary = 2,      // بیمه تکمیلی اول
    Tertiary = 3,       // بیمه تکمیلی دوم
    Quaternary = 4,     // بیمه تکمیلی سوم
    Quinary = 5        // بیمه تکمیلی چهارم
}
```

### تنظیم خودکار Priority:

#### برای بیمه اصلی:
```csharp
if (model.IsPrimary)
{
    patientInsurance.Priority = InsurancePriority.Primary;
}
```

#### برای بیمه تکمیلی:
```csharp
else
{
    var existingInsurances = await _patientInsuranceRepository.GetByPatientIdAsync(model.PatientId);
    var existingPriorities = existingInsurances.Where(pi => !pi.IsPrimary).Select(pi => pi.Priority);
    patientInsurance.Priority = InsurancePriorityHelper.GetNextSupplementaryPriority(existingPriorities);
}
```

### کلاس مسئول:
- **`InsurancePriorityHelper.GetNextSupplementaryPriority`**

---

## ✅ Validation و Business Rules

### 1. Client-Side Validation (JavaScript):

#### بررسی فیلدهای الزامی:
```javascript
// بررسی PatientId
if (patientId <= 0) {
    alert('لطفاً بیمار را انتخاب کنید.');
    return false;
}

// بررسی InsurancePlanId
if (insurancePlanId <= 0) {
    alert('لطفاً طرح بیمه را انتخاب کنید.');
    return false;
}

// بررسی PolicyNumber
if (!policyNumber || policyNumber.trim() === '') {
    alert('شماره بیمه الزامی است.');
    return false;
}
```

### 2. Server-Side Validation (C#):

#### در ViewModel:
```csharp
[Required(ErrorMessage = "بیمار الزامی است")]
public int PatientId { get; set; }

[Required(ErrorMessage = "طرح بیمه الزامی است")]
public int InsurancePlanId { get; set; }

[Required(ErrorMessage = "شماره بیمه الزامی است")]
[StringLength(100, ErrorMessage = "شماره بیمه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
[RegularExpression(@"^[A-Za-z0-9\-_]+$", ErrorMessage = "شماره بیمه فقط می‌تواند شامل حروف انگلیسی، اعداد، خط تیره و زیرخط باشد")]
public string PolicyNumber { get; set; }

[Required(ErrorMessage = "تاریخ شروع الزامی است")]
[CustomValidation(typeof(PatientInsuranceCreateEditViewModel), "ValidateStartDate")]
public DateTime StartDate { get; set; }

[CustomValidation(typeof(PatientInsuranceCreateEditViewModel), "ValidateEndDate")]
public DateTime? EndDate { get; set; }
```

#### در Service:
```csharp
// 🏥 Medical Environment: اعتبارسنجی فیلدهای الزامی
if (model.PatientId <= 0)
{
    errors.Add("PatientId", "شناسه بیمار الزامی است");
}

if (model.InsurancePlanId <= 0)
{
    errors.Add("InsurancePlanId", "انتخاب طرح بیمه الزامی است");
}

if (string.IsNullOrWhiteSpace(model.PolicyNumber))
{
    errors.Add("PolicyNumber", "شماره بیمه الزامی است");
}

if (model.StartDate == DateTime.MinValue)
{
    errors.Add("StartDate", "تاریخ شروع الزامی است");
}

// 🏥 Medical Environment: اعتبارسنجی بیمه تکمیلی
if (model.SupplementaryInsuranceProviderId.HasValue && !model.SupplementaryInsurancePlanId.HasValue)
{
    errors.Add("SupplementaryInsurancePlanId", "اگر بیمه‌گذار تکمیلی انتخاب شده، طرح بیمه تکمیلی نیز باید انتخاب شود");
}

if (!model.SupplementaryInsuranceProviderId.HasValue && model.SupplementaryInsurancePlanId.HasValue)
{
    errors.Add("SupplementaryInsuranceProviderId", "اگر طرح بیمه تکمیلی انتخاب شده، بیمه‌گذار تکمیلی نیز باید انتخاب شود");
}
```

### 3. Custom Validation Methods:

#### ValidateStartDate:
```csharp
public static ValidationResult ValidateStartDate(DateTime startDate, ValidationContext validationContext)
{
    if (startDate == default(DateTime))
    {
        return ValidationResult.Success; // Required attribute handles this
    }

    var now = DateTime.Now;
    var oneYearAgo = now.AddYears(-1);

    // فقط بررسی می‌کنیم که تاریخ شروع بیش از 1 سال در گذشته نباشد
    if (startDate < oneYearAgo)
    {
        return new ValidationResult("تاریخ شروع نمی‌تواند بیش از 1 سال در گذشته باشد.");
    }

    return ValidationResult.Success;
}
```

#### ValidateEndDate:
```csharp
public static ValidationResult ValidateEndDate(DateTime? endDate, ValidationContext validationContext)
{
    var model = (PatientInsuranceCreateEditViewModel)validationContext.ObjectInstance;
    
    if (endDate.HasValue && model.StartDate != default(DateTime))
    {
        if (endDate.Value <= model.StartDate)
        {
            return new ValidationResult("تاریخ پایان باید بعد از تاریخ شروع باشد.");
        }
        
        // بررسی اینکه تاریخ پایان در گذشته نباشد
        var now = DateTime.Now;
        if (endDate.Value < now)
        {
            return new ValidationResult("تاریخ پایان نمی‌تواند در گذشته باشد.");
        }
    }
    
    return ValidationResult.Success;
}
```

#### ValidatePolicyNumber:
```csharp
public static ValidationResult ValidatePolicyNumber(string policyNumber, ValidationContext validationContext)
{
    if (string.IsNullOrWhiteSpace(policyNumber))
    {
        return ValidationResult.Success; // Required attribute handles this
    }

    // بررسی فرمت شماره بیمه
    if (policyNumber.Length < 3)
    {
        return new ValidationResult("شماره بیمه باید حداقل 3 کاراکتر باشد.");
    }

    // بررسی اینکه شماره بیمه فقط شامل کاراکترهای مجاز باشد
    if (!System.Text.RegularExpressions.Regex.IsMatch(policyNumber, @"^[A-Za-z0-9\-_]+$"))
    {
        return new ValidationResult("شماره بیمه فقط می‌تواند شامل حروف انگلیسی، اعداد، خط تیره و زیرخط باشد.");
    }

    return ValidationResult.Success;
}
```

---

## 📱 Views و UI/UX

### 1. Index.cshtml:
- **کاربرد:** نمایش لیست بیمه‌های بیماران
- **ویژگی‌ها:**
  - فیلتر بر اساس ارائه‌دهنده، نوع بیمه، وضعیت
  - جستجو بر اساس نام یا کد ملی بیمار
  - صفحه‌بندی
  - اعتبارسنجی Real-time
  - عملیات سریع (ایجاد بیمه پیش‌فرض آزاد، بررسی وضعیت)

### 2. Create.cshtml:
- **کاربرد:** ایجاد بیمه بیمار جدید
- **ویژگی‌ها:**
  - انتخاب بیمار با Select2
  - انتخاب بیمه‌گذار و طرح بیمه
  - ورود شماره بیمه
  - انتخاب بیمه تکمیلی (اختیاری)
  - ورود تاریخ شروع و پایان
  - تنظیم بیمه اصلی/تکمیلی
  - Validation Real-time

### 3. Edit.cshtml:
- **کاربرد:** ویرایش بیمه بیمار موجود
- **ویژگی‌ها:**
  - مشابه Create.cshtml
  - نمایش اطلاعات فعلی
  - امکان تغییر تمام فیلدها

### 4. Details.cshtml:
- **کاربرد:** مشاهده جزئیات بیمه بیمار
- **ویژگی‌ها:**
  - نمایش تمام اطلاعات بیمه
  - نمایش اطلاعات بیمار
  - نمایش Audit Trail
  - دکمه‌های ویرایش و حذف

---

## 🔍 مشکلات احتمالی و راه‌حل‌ها

### مشکل 1: عدم تنظیم InsuranceProviderId در Create

#### علت:
- در `ConvertToEntity` ممکن است `InsuranceProviderId` تنظیم نشود

#### راه‌حل:
```csharp
// 🚨 CRITICAL FIX: اضافه کردن InsuranceProviderId
private PatientInsurance ConvertToEntity(PatientInsuranceCreateEditViewModel model)
{
    return new PatientInsurance
    {
        // ...
        InsuranceProviderId = model.InsuranceProviderId, // ✅ اضافه شد
        // ...
    };
}
```

### مشکل 2: عدم در نظر گیری بیمه تکمیلی در رکورد اصلی

#### علت:
- منطق دریافت بیمه‌های تکمیلی فقط رکوردهای جداگانه را در نظر می‌گیرد

#### راه‌حل:
```csharp
// 🚨 CRITICAL FIX: منطق صحیح برای سیستم درمانی
// بیمه‌های تکمیلی: 
// 1. رکوردهایی که IsPrimary = false هستند (رکوردهای جداگانه)
// 2. رکوردهایی که IsPrimary = true اما SupplementaryInsuranceProviderId دارند (فیلدهای تکمیلی)
return await _context.PatientInsurances
    .Where(pi => pi.PatientId == patientId 
             && pi.IsActive
             && !pi.IsDeleted
             && (
                 pi.IsPrimary == false || 
                 (pi.IsPrimary == true && pi.SupplementaryInsuranceProviderId.HasValue && pi.SupplementaryInsurancePlanId.HasValue)
             ))
    // ...
```

### مشکل 3: عدم استفاده از Transaction در SetPrimaryInsurance

#### علت:
- ممکن است در صورت خطا، وضعیت بیمه‌ها ناهماهنگ شود

#### راه‌حل:
```csharp
// استفاده از Transaction برای اطمینان از consistency
using (var transaction = await _patientInsuranceRepository.BeginTransactionAsync())
{
    try
    {
        // عملیات
        transaction.Commit();
    }
    catch (Exception ex)
    {
        transaction.Rollback();
        throw;
    }
}
```

### مشکل 4: عدم بررسی تکراری بودن PolicyNumber

#### علت:
- ممکن است شماره بیمه تکراری ذخیره شود

#### راه‌حل:
```csharp
// بررسی وجود شماره بیمه تکراری
var existingInsurance = await _patientInsuranceRepository.GetByPolicyNumberAsync(model.PolicyNumber);
if (existingInsurance != null && existingInsurance.PatientInsuranceId != model.PatientInsuranceId)
{
    errors.Add("PolicyNumber", "شماره بیمه تکراری است");
}
```

### مشکل 5: عدم بررسی اعتبار تاریخ‌ها

#### علت:
- ممکن است تاریخ شروع/پایان نامعتبر باشد

#### راه‌حل:
```csharp
// بررسی اعتبار تاریخ شروع
if (model.StartDate < DateTime.Now.AddYears(-1))
{
    errors.Add("StartDate", "تاریخ شروع نمی‌تواند بیش از 1 سال در گذشته باشد");
}

// بررسی اعتبار تاریخ پایان
if (model.EndDate.HasValue && model.EndDate.Value <= model.StartDate)
{
    errors.Add("EndDate", "تاریخ پایان باید بعد از تاریخ شروع باشد");
}
```

---

## 📋 چک‌لیست صحت‌سنجی

### ✅ ایجاد بیمه:
- [ ] PatientId معتبر است
- [ ] InsurancePlanId معتبر است
- [ ] InsuranceProviderId تنظیم شده است
- [ ] PolicyNumber منحصر به فرد است
- [ ] تاریخ شروع معتبر است
- [ ] تاریخ پایان (در صورت وجود) معتبر است
- [ ] Priority به درستی تنظیم شده است
- [ ] فیلدهای Audit تنظیم شده‌اند

### ✅ به‌روزرسانی بیمه:
- [ ] Entity موجود است
- [ ] تمام فیلدها به‌روزرسانی می‌شوند
- [ ] فیلدهای Audit به‌روزرسانی می‌شوند
- [ ] Validation انجام می‌شود

### ✅ حذف نرم:
- [ ] Entity موجود است
- [ ] IsDeleted = true تنظیم می‌شود
- [ ] IsActive = false تنظیم می‌شود
- [ ] فیلدهای Audit تنظیم می‌شوند

### ✅ تنظیم بیمه اصلی:
- [ ] بیمه فعال است
- [ ] سایر بیمه‌های اصلی غیرفعال می‌شوند
- [ ] Transaction استفاده می‌شود
- [ ] فیلدهای Audit تنظیم می‌شوند

### ✅ دریافت بیمه‌ها:
- [ ] بیمه اصلی به درستی دریافت می‌شود
- [ ] بیمه‌های تکمیلی به درستی دریافت می‌شوند
- [ ] فیلترهای IsActive و IsDeleted اعمال می‌شوند
- [ ] Navigation Properties لود می‌شوند

---

## 🧪 تست‌های پیشنهادی

### تست 1: ایجاد بیمه اصلی
```csharp
// ورودی:
// PatientId: 1
// InsurancePlanId: 1
// PolicyNumber: "1234567890"
// IsPrimary: true

// خروجی مورد انتظار:
// PatientInsuranceId > 0
// Priority = Primary
// IsActive = true
// IsDeleted = false
```

### تست 2: ایجاد بیمه تکمیلی
```csharp
// ورودی:
// PatientId: 1
// InsurancePlanId: 2
// PolicyNumber: "0987654321"
// IsPrimary: false

// خروجی مورد انتظار:
// PatientInsuranceId > 0
// Priority = Secondary (یا بعدی)
// IsActive = true
// IsDeleted = false
```

### تست 3: تنظیم بیمه اصلی
```csharp
// ورودی:
// PatientInsuranceId: 2 (بیمه تکمیلی)

// خروجی مورد انتظار:
// PatientInsuranceId: 2 → IsPrimary = true
// سایر بیمه‌های اصلی → IsPrimary = false
```

### تست 4: دریافت بیمه‌های فعال
```csharp
// ورودی:
// PatientId: 1

// خروجی مورد انتظار:
// لیست بیمه‌های فعال (IsActive = true, IsDeleted = false)
// بیمه اصلی اول
// بیمه‌های تکمیلی به ترتیب Priority
```

---

## 📚 منابع و مراجع

1. **`Models/Entities/Patient/PatientInsurance.cs`** - Entity Definition
2. **`Services/Insurance/PatientInsuranceService.cs`** - Business Logic
3. **`Repositories/Insurance/PatientInsuranceRepository.cs`** - Data Access
4. **`Areas/Admin/Controllers/Insurance/PatientInsuranceController.cs`** - Controller
5. **`ViewModels/Insurance/PatientInsurance/PatientInsuranceCreateEditViewModel.cs`** - ViewModel
6. **`Areas/Admin/Views/PatientInsurance/`** - Views
7. **`Services/Insurance/PatientInsuranceValidationService.cs`** - Validation Service

---

## ✅ خلاصه

### ساختار:
- **Entity:** `PatientInsurance` با پشتیبانی از بیمه اصلی و تکمیلی
- **Service:** `PatientInsuranceService` برای منطق کسب‌وکار
- **Repository:** `PatientInsuranceRepository` برای دسترسی به داده
- **Controller:** `PatientInsuranceController` برای HTTP Actions
- **ViewModels:** `PatientInsuranceCreateEditViewModel`, `PatientInsuranceDetailsViewModel`, etc.

### منطق انتصاب:
- **ایجاد:** تبدیل ViewModel به Entity، تنظیم Priority، ذخیره
- **به‌روزرسانی:** به‌روزرسانی فیلدها، تنظیم Audit
- **حذف:** Soft Delete با حفظ اطلاعات

### مدیریت بیمه اصلی/تکمیلی:
- **تنظیم اصلی:** Transaction برای اطمینان از consistency
- **دریافت اصلی:** فیلتر بر اساس IsPrimary = true
- **دریافت تکمیلی:** منطق پیچیده برای رکوردهای جداگانه و فیلدهای تکمیلی

### Validation:
- **Client-Side:** JavaScript برای UX بهتر
- **Server-Side:** C# Data Annotations و Custom Validation
- **Business Rules:** بررسی تکراری بودن، اعتبار تاریخ‌ها

---

**⚠️ توجه:** این مستندات بر اساس کد فعلی سیستم تهیه شده است. در صورت تغییر منطق، این مستندات باید به‌روزرسانی شوند.

