# 📊 گزارش بررسی سیستم قیمت‌گذاری نوبت‌ها

**تاریخ:** 2026-01-06  
**وضعیت:** 🔍 تحلیل کامل  
**نوع:** بررسی معماری و پیشنهاد پیاده‌سازی

---

## 🎯 **خلاصه اجرایی**

### **درخواست کاربر:**
1. ✅ مدیر سایت باید بتواند برای هر نوبت قیمت مشخص کند
2. ✅ مثلاً برای دکتر قلب: 500 هزار تومان
3. ✅ برای پزشک عمومی: 350 هزار تومان
4. ✅ برای یک ایونت خاص تخفیف بدهد (مثلاً از 10 نوبت، 5 تا با تخفیف)

---

## 📋 **وضعیت فعلی سیستم**

### ✅ **1. Entity و Database (موجود)**

#### **`DoctorSchedule.ConsultationFee`**
```csharp
// Models/Entities/Doctor/DoctorSchedule.cs:92
[Range(0, 10000000, ErrorMessage = "هزینه ویزیت باید بین 0 تا 10,000,000 ریال باشد.")]
public decimal ConsultationFee { get; set; } = 0;
```

**وضعیت:** ✅ موجود در Entity  
**محدوده:** 0 تا 10,000,000 ریال  
**ذخیره‌سازی:** در جدول `DoctorSchedules`

---

### ✅ **2. Service محاسبه قیمت (موجود)**

#### **`AppointmentPricingService`**
```csharp
// Services/Appointment/AppointmentPricingService.cs
public async Task<AppointmentPriceResult> CalculatePriceAsync(
    int doctorId,
    int? serviceCategoryId = null,
    int? patientId = null)
```

**فرآیند محاسبه:**
1. ✅ دریافت قیمت پایه از `DoctorSchedule.ConsultationFee`
2. ✅ محاسبه تخفیف‌ها (فعلاً 0 - TODO)
3. ✅ محاسبه مالیات (فعلاً 0%)
4. ✅ محاسبه قیمت نهایی

**قیمت پیش‌فرض:** 500,000 تومان (در صورت عدم وجود `ConsultationFee`)

---

### ✅ **3. استفاده در رزرو نوبت**

```csharp
// Services/Appointment/AppointmentBookingService.cs:754
Price = priceResult.Data, // قیمت از AppointmentPricingService
```

**وضعیت:** ✅ قیمت در `Appointment.Price` ذخیره می‌شود

---

## ❌ **مشکلات و کمبودها**

### **1. عدم وجود فیلد در ViewModel**

**مشکل:**
```csharp
// ViewModels/DoctorManagementVM/DoctorScheduleViewModel.cs
// ❌ ConsultationFee وجود ندارد!
```

**اثر:** مدیر نمی‌تواند قیمت را در فرم ویرایش تنظیم کند

---

### **2. عدم وجود فیلد در View**

**مشکل:**
```html
<!-- Areas/Admin/Views/DoctorSchedule/Edit.cshtml -->
<!-- ❌ فیلدی برای ConsultationFee وجود ندارد -->
```

**اثر:** مدیر نمی‌تواند قیمت را در UI تنظیم کند

---

### **3. عدم وجود سیستم تخفیف برای ایونت‌ها**

**مشکل:**
```csharp
// Services/Appointment/AppointmentPricingService.cs:126
private async Task<decimal> CalculateDiscountAsync(...)
{
    // TODO: در آینده می‌توان تخفیف‌های زیر را اضافه کرد:
    // 1. تخفیف بیمه
    // 2. تخفیف ویژه پزشک
    // 3. تخفیف دوره‌ای
    // 4. تخفیف گروهی
    
    return 0m; // فعلاً تخفیف 0 است
}
```

**اثر:** سیستم تخفیف برای ایونت‌های خاص وجود ندارد

---

## 🔧 **راه‌حل پیشنهادی**

### **مرحله 1: اضافه کردن ConsultationFee به ViewModel**

**فایل:** `ViewModels/DoctorManagementVM/DoctorScheduleViewModel.cs`

```csharp
/// <summary>
/// هزینه ویزیت پایه (ریال)
/// </summary>
[Range(0, 10000000, ErrorMessage = "هزینه ویزیت باید بین 0 تا 10,000,000 ریال باشد.")]
[Display(Name = "هزینه ویزیت (ریال)")]
public decimal ConsultationFee { get; set; } = 0;
```

**تغییرات در `FromEntity`:**
```csharp
ConsultationFee = doctorSchedule.ConsultationFee,
```

**تغییرات در `ToEntity`:**
```csharp
ConsultationFee = this.ConsultationFee,
```

---

### **مرحله 2: اضافه کردن فیلد به View**

**فایل:** `Areas/Admin/Views/DoctorSchedule/Edit.cshtml`

```html
<div class="form-group">
    <label class="form-label">
        <i class="fas fa-money-bill-wave me-2"></i>
        هزینه ویزیت (ریال)
    </label>
    @Html.TextBoxFor(m => m.ConsultationFee, 
        new { 
            @class = "form-control", 
            type = "number",
            min = "0",
            max = "10000000",
            step = "1000",
            placeholder = "مثال: 500000"
        })
    @Html.ValidationMessageFor(m => m.ConsultationFee, "", new { @class = "text-danger" })
    <small class="form-text text-muted">
        هزینه ویزیت به ریال (مثال: 500000 = 500 هزار تومان)
    </small>
</div>
```

---

### **مرحله 3: پیاده‌سازی سیستم تخفیف برای ایونت‌ها**

#### **3.1. ایجاد Entity جدید: `PromotionalEvent`**

**فایل:** `Models/Entities/Appointment/PromotionalEvent.cs`

```csharp
/// <summary>
/// ایونت‌های تبلیغاتی و تخفیف‌ها
/// </summary>
public class PromotionalEvent : ISoftDelete, ITrackable
{
    public int EventId { get; set; }
    
    /// <summary>
    /// عنوان ایونت (مثلاً: "تخفیف ویژه نوروز")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }
    
    /// <summary>
    /// توضیحات
    /// </summary>
    [MaxLength(1000)]
    public string Description { get; set; }
    
    /// <summary>
    /// تاریخ شروع
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// تاریخ پایان
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// نوع تخفیف (درصدی یا مبلغ ثابت)
    /// </summary>
    public DiscountType DiscountType { get; set; }
    
    /// <summary>
    /// مقدار تخفیف (درصد یا مبلغ)
    /// </summary>
    public decimal DiscountValue { get; set; }
    
    /// <summary>
    /// تعداد کل نوبت‌های قابل استفاده
    /// </summary>
    public int? TotalSlots { get; set; }
    
    /// <summary>
    /// تعداد نوبت‌های استفاده شده
    /// </summary>
    public int UsedSlots { get; set; } = 0;
    
    /// <summary>
    /// آیا فقط برای پزشکان خاص؟
    /// </summary>
    public bool IsDoctorSpecific { get; set; } = false;
    
    /// <summary>
    /// لیست شناسه‌های پزشکان (JSON)
    /// </summary>
    public string DoctorIds { get; set; }
    
    /// <summary>
    /// آیا فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // ISoftDelete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string DeletedByUserId { get; set; }
    
    // ITrackable
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedByUserId { get; set; }
    
    // Navigation
    public virtual ICollection<Appointment> Appointments { get; set; }
}

public enum DiscountType
{
    Percentage = 1,  // درصدی (مثلاً 20%)
    FixedAmount = 2  // مبلغ ثابت (مثلاً 100,000 ریال)
}
```

---

#### **3.2. به‌روزرسانی `AppointmentPricingService`**

**فایل:** `Services/Appointment/AppointmentPricingService.cs`

```csharp
/// <summary>
/// محاسبه تخفیف‌ها
/// </summary>
private async Task<decimal> CalculateDiscountAsync(
    int doctorId, 
    int? patientId, 
    decimal basePrice,
    DateTime? appointmentDate = null)
{
    decimal totalDiscount = 0m;
    
    try
    {
        var appointmentDateTime = appointmentDate ?? DateTime.Now;
        
        // 1. بررسی ایونت‌های تبلیغاتی فعال
        var activeEvents = await _context.PromotionalEvents
            .Where(e => e.IsActive 
                && !e.IsDeleted
                && e.StartDate <= appointmentDateTime
                && e.EndDate >= appointmentDateTime
                && (e.TotalSlots == null || e.UsedSlots < e.TotalSlots))
            .ToListAsync();
        
        foreach (var evt in activeEvents)
        {
            // بررسی محدودیت پزشک
            if (evt.IsDoctorSpecific)
            {
                var doctorIds = JsonConvert.DeserializeObject<List<int>>(evt.DoctorIds ?? "[]");
                if (!doctorIds.Contains(doctorId))
                    continue;
            }
            
            decimal discount = 0m;
            
            if (evt.DiscountType == DiscountType.Percentage)
            {
                discount = basePrice * (evt.DiscountValue / 100m);
            }
            else if (evt.DiscountType == DiscountType.FixedAmount)
            {
                discount = evt.DiscountValue;
            }
            
            // اطمینان از اینکه تخفیف بیشتر از قیمت پایه نباشد
            discount = Math.Min(discount, basePrice);
            
            totalDiscount += discount;
            
            _logger.Information(
                "تخفیف ایونت اعمال شد - EventId: {EventId}, Title: {Title}, Discount: {Discount}, Type: {Type}",
                evt.EventId, evt.Title, discount, evt.DiscountType);
        }
        
        _logger.Information(
            "محاسبه تخفیف تکمیل شد - DoctorId: {DoctorId}, BasePrice: {BasePrice}, TotalDiscount: {TotalDiscount}",
            doctorId, basePrice, totalDiscount);
        
        return totalDiscount;
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در محاسبه تخفیف");
        return 0m;
    }
}
```

---

#### **3.3. ایجاد Controller و View برای مدیریت ایونت‌ها**

**فایل:** `Areas/Admin/Controllers/PromotionalEventController.cs`

```csharp
[Authorize(Roles = "Admin,ClinicAdmin")]
public class PromotionalEventController : Controller
{
    // CRUD operations برای PromotionalEvent
    // Index, Create, Edit, Delete, Details
}
```

**View:** `Areas/Admin/Views/PromotionalEvent/`

---

## 📊 **نمودار جریان قیمت‌گذاری**

```
┌─────────────────────────────────────────────────────────┐
│ 1. مدیر وارد فرم ویرایش DoctorSchedule می‌شود          │
│    ConsultationFee = 500000 (500 هزار تومان)            │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 2. ذخیره در DoctorSchedule.ConsultationFee             │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 3. بیمار نوبت رزرو می‌کند                              │
│    AppointmentPricingService.CalculatePriceAsync()      │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 4. دریافت قیمت پایه از DoctorSchedule.ConsultationFee   │
│    BasePrice = 500000                                   │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 5. بررسی ایونت‌های تبلیغاتی فعال                      │
│    PromotionalEvent (مثلاً: 5 نوبت اول با 20% تخفیف)  │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 6. محاسبه تخفیف                                        │
│    Discount = 500000 * 0.20 = 100000                    │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 7. محاسبه قیمت نهایی                                   │
│    FinalPrice = 500000 - 100000 = 400000                │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 8. ذخیره در Appointment.Price                          │
│    Price = 400000                                       │
└─────────────────────────────────────────────────────────┘
```

---

## ✅ **چک‌لیست پیاده‌سازی**

### **مرحله 1: اضافه کردن ConsultationFee به ViewModel و View**
- [ ] اضافه کردن `ConsultationFee` به `DoctorScheduleViewModel`
- [ ] به‌روزرسانی `FromEntity` و `ToEntity`
- [ ] اضافه کردن فیلد به `Edit.cshtml`
- [ ] اضافه کردن Validation
- [ ] تست: ویرایش قیمت در فرم

### **مرحله 2: پیاده‌سازی سیستم تخفیف**
- [ ] ایجاد Entity `PromotionalEvent`
- [ ] ایجاد Migration
- [ ] به‌روزرسانی `AppointmentPricingService.CalculateDiscountAsync`
- [ ] ایجاد Controller `PromotionalEventController`
- [ ] ایجاد Views (Index, Create, Edit, Delete)
- [ ] تست: ایجاد ایونت و اعمال تخفیف

### **مرحله 3: تست و مستندسازی**
- [ ] تست سناریو: قیمت برای دکتر قلب = 500 هزار
- [ ] تست سناریو: قیمت برای پزشک عمومی = 350 هزار
- [ ] تست سناریو: ایونت با 5 نوبت تخفیف‌دار
- [ ] مستندسازی API
- [ ] راهنمای کاربری

---

## 🎯 **نتیجه‌گیری**

### **وضعیت فعلی:**
- ✅ Entity موجود (`DoctorSchedule.ConsultationFee`)
- ✅ Service محاسبه قیمت موجود (`AppointmentPricingService`)
- ❌ ViewModel و View برای ویرایش قیمت وجود ندارد
- ❌ سیستم تخفیف برای ایونت‌ها وجود ندارد

### **اقدامات لازم:**
1. ✅ اضافه کردن `ConsultationFee` به ViewModel و View (ساده)
2. ✅ پیاده‌سازی سیستم تخفیف برای ایونت‌ها (متوسط)

### **اولویت:**
1. **بالا:** اضافه کردن ConsultationFee به View (30 دقیقه)
2. **متوسط:** پیاده‌سازی سیستم تخفیف (2-3 ساعت)

---

**📌 این گزارش بر اساس تحلیل کامل کد موجود تهیه شده است.**

