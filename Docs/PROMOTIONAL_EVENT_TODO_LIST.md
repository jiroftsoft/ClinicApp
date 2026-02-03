# 📋 Todo List: سیستم تخفیف برای ایونت‌ها (Promotional Events)

**تاریخ:** 2026-01-07  
**نسخه:** 1.0.0  
**وضعیت:** 🟡 در حال پیاده‌سازی

---

## 🎯 **مرحله 1: Entity Layer (پایه داده)**

### ✅ **1.1. ایجاد Enum `DiscountType`**
**فایل:** `Models/Enums/DiscountType.cs`

**Todo:**
- [ ] ایجاد Enum با دو مقدار: `Percentage = 1`, `FixedAmount = 2`
- [ ] اضافه کردن XML Documentation
- [ ] اضافه کردن `[Display]` attributes برای نمایش فارسی

**کد نمونه:**
```csharp
public enum DiscountType : byte
{
    [Display(Name = "درصدی")]
    Percentage = 1,
    
    [Display(Name = "مبلغ ثابت")]
    FixedAmount = 2
}
```

---

### ✅ **1.2. ایجاد Entity `PromotionalEvent`**
**فایل:** `Models/Entities/PromotionalEvent/PromotionalEvent.cs`

**Todo:**
- [ ] ایجاد Class با `ISoftDelete` و `ITrackable`
- [ ] Properties: `EventId`, `Title`, `Description`, `StartDate`, `EndDate`
- [ ] Properties: `DiscountType`, `DiscountValue` (decimal(18,0))
- [ ] Properties: `TotalSlots`, `UsedSlots`, `IsDoctorSpecific`, `DoctorIds` (JSON)
- [ ] Property: `IsActive`
- [ ] Navigation Property: `ICollection<Appointment> Appointments`
- [ ] اضافه کردن `[Required]`, `[MaxLength]`, `[Range]` attributes
- [ ] XML Documentation برای همه Properties

**کد نمونه:**
```csharp
public class PromotionalEvent : ISoftDelete, ITrackable
{
    public int EventId { get; set; }
    
    [Required, MaxLength(200)]
    public string Title { get; set; }
    
    [MaxLength(1000)]
    public string Description { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    public DiscountType DiscountType { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,0)")]
    public decimal DiscountValue { get; set; }
    
    public int? TotalSlots { get; set; }
    
    public int UsedSlots { get; set; } = 0;
    
    public bool IsDoctorSpecific { get; set; } = false;
    
    [Column(TypeName = "nvarchar(max)")]
    public string DoctorIds { get; set; } // JSON: [1,2,3]
    
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
```

---

### ✅ **1.3. ایجاد Configuration `PromotionalEventConfig`**
**فایل:** `Models/Entities/PromotionalEvent/PromotionalEventConfig.cs`

**Todo:**
- [ ] ایجاد Class با `EntityTypeConfiguration<PromotionalEvent>`
- [ ] `ToTable("PromotionalEvents")`
- [ ] `HasKey(e => e.EventId)`
- [ ] تنظیم `HasPrecision(18, 0)` برای `DiscountValue`
- [ ] ایجاد Index برای `StartDate`, `EndDate`, `IsActive`, `IsDeleted`
- [ ] ایجاد Composite Index برای `StartDate`, `EndDate`, `IsActive`
- [ ] تنظیم `HasMany(e => e.Appointments)` با `HasForeignKey`

**کد نمونه:**
```csharp
public class PromotionalEventConfig : EntityTypeConfiguration<PromotionalEvent>
{
    public PromotionalEventConfig()
    {
        ToTable("PromotionalEvents");
        HasKey(e => e.EventId);
        
        Property(e => e.DiscountValue)
            .IsRequired()
            .HasPrecision(18, 0);
        
        HasIndex(e => new { e.StartDate, e.EndDate, e.IsActive })
            .HasName("IX_PromotionalEvent_StartDate_EndDate_IsActive");
    }
}
```

---

### ✅ **1.4. به‌روزرسانی Entity `Appointment`**
**فایل:** `Models/Entities/Appointment/Appointment.cs`

**Todo:**
- [ ] اضافه کردن Property: `int? PromotionalEventId`
- [ ] اضافه کردن Property: `decimal DiscountAmount` (decimal(18,0))
- [ ] اضافه کردن Navigation Property: `virtual PromotionalEvent PromotionalEvent`
- [ ] اضافه کردن XML Documentation

**کد نمونه:**
```csharp
/// <summary>
/// شناسه ایونت تبلیغاتی (در صورت اعمال تخفیف)
/// </summary>
public int? PromotionalEventId { get; set; }

/// <summary>
/// مبلغ تخفیف اعمال شده (ریال)
/// </summary>
[Column(TypeName = "decimal(18,0)")]
public decimal DiscountAmount { get; set; } = 0;

public virtual PromotionalEvent PromotionalEvent { get; set; }
```

---

### ✅ **1.5. به‌روزرسانی `ApplicationDbContext`**
**فایل:** `Models/IdentityModels.cs`

**Todo:**
- [ ] اضافه کردن `DbSet<PromotionalEvent> PromotionalEvents { get; set; }`
- [ ] اضافه کردن `modelBuilder.Configurations.Add(new PromotionalEventConfig());` در `OnModelCreating`

---

## 🎯 **مرحله 2: Repository Layer**

### ✅ **2.1. ایجاد Interface `IPromotionalEventRepository`**
**فایل:** `Repositories/PromotionalEvent/IPromotionalEventRepository.cs`

**Todo:**
- [ ] ایجاد Interface با متدهای CRUD
- [ ] `Task<PromotionalEvent> GetByIdAsync(int eventId)`
- [ ] `Task<IEnumerable<PromotionalEvent>> GetAllAsync()`
- [ ] `Task<IEnumerable<PromotionalEvent>> GetActiveEventsAsync()`
- [ ] `Task<IEnumerable<PromotionalEvent>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)`
- [ ] `Task<IEnumerable<PromotionalEvent>> GetEventsByDoctorAsync(int doctorId)`
- [ ] `Task<PromotionalEvent> AddAsync(PromotionalEvent entity)`
- [ ] `Task UpdateAsync(PromotionalEvent entity)`
- [ ] `Task DeleteAsync(int eventId)`
- [ ] `Task IncrementUsedSlotsAsync(int eventId)`

---

### ✅ **2.2. ایجاد Repository `PromotionalEventRepository`**
**فایل:** `Repositories/PromotionalEvent/PromotionalEventRepository.cs`

**Todo:**
- [ ] پیاده‌سازی `IPromotionalEventRepository`
- [ ] Dependency Injection: `ApplicationDbContext`, `ILogger`
- [ ] استفاده از `AsNoTracking()` برای Read-only queries
- [ ] Logging با Serilog در همه متدها
- [ ] Error Handling با try-catch
- [ ] `GetActiveEventsAsync()`: فیلتر `IsActive = true`, `IsDeleted = false`, `StartDate <= now`, `EndDate >= now`
- [ ] `GetEventsByDoctorAsync()`: فیلتر `IsDoctorSpecific = true` و `DoctorIds` شامل `doctorId`
- [ ] `IncrementUsedSlotsAsync()`: استفاده از `Interlocked.Increment` یا `UPDATE` مستقیم

---

## 🎯 **مرحله 3: Service Layer**

### ✅ **3.1. ایجاد Interface `IPromotionalEventService`**
**فایل:** `Services/PromotionalEvent/IPromotionalEventService.cs`

**Todo:**
- [ ] ایجاد Interface با متدهای Business Logic
- [ ] `Task<ServiceResult<PromotionalEvent>> CreateAsync(PromotionalEvent entity)`
- [ ] `Task<ServiceResult<PromotionalEvent>> UpdateAsync(int eventId, PromotionalEvent entity)`
- [ ] `Task<ServiceResult<bool>> DeleteAsync(int eventId)`
- [ ] `Task<ServiceResult<IEnumerable<PromotionalEvent>>> GetActiveEventsAsync()`
- [ ] `Task<ServiceResult<decimal>> CalculateDiscountAsync(int doctorId, decimal basePrice, DateTime? appointmentDate = null)`
- [ ] `Task<ServiceResult<bool>> IncrementUsedSlotsAsync(int eventId)`

---

### ✅ **3.2. ایجاد Service `PromotionalEventService`**
**فایل:** `Services/PromotionalEvent/PromotionalEventService.cs`

**Todo:**
- [ ] پیاده‌سازی `IPromotionalEventService`
- [ ] Dependency Injection: `IPromotionalEventRepository`, `ILogger`, `ICurrentUserService`
- [ ] استفاده از `ServiceResult<T>` Pattern
- [ ] Validation در `CreateAsync()` و `UpdateAsync()`
- [ ] Transaction Management در `CreateAsync()`, `UpdateAsync()`, `DeleteAsync()`
- [ ] Logging کامل با Serilog
- [ ] `CalculateDiscountAsync()`: محاسبه تخفیف بر اساس ایونت‌های فعال
- [ ] `IncrementUsedSlotsAsync()`: افزایش `UsedSlots` و بررسی `TotalSlots`

**کد نمونه برای `CalculateDiscountAsync()`:**
```csharp
public async Task<ServiceResult<decimal>> CalculateDiscountAsync(
    int doctorId, 
    decimal basePrice, 
    DateTime? appointmentDate = null)
{
    try
    {
        var appointmentDateTime = appointmentDate ?? DateTime.Now;
        
        var activeEvents = await _repository.GetActiveEventsAsync();
        
        decimal totalDiscount = 0m;
        
        foreach (var evt in activeEvents)
        {
            // بررسی محدودیت پزشک
            if (evt.IsDoctorSpecific)
            {
                var doctorIds = JsonConvert.DeserializeObject<List<int>>(evt.DoctorIds ?? "[]");
                if (!doctorIds.Contains(doctorId))
                    continue;
            }
            
            // بررسی تاریخ
            if (evt.StartDate > appointmentDateTime || evt.EndDate < appointmentDateTime)
                continue;
            
            // بررسی تعداد استفاده شده
            if (evt.TotalSlots.HasValue && evt.UsedSlots >= evt.TotalSlots.Value)
                continue;
            
            // محاسبه تخفیف
            decimal discount = 0m;
            if (evt.DiscountType == DiscountType.Percentage)
            {
                discount = basePrice * (evt.DiscountValue / 100m);
            }
            else if (evt.DiscountType == DiscountType.FixedAmount)
            {
                discount = evt.DiscountValue;
            }
            
            // محدودیت: تخفیف نمی‌تواند بیشتر از قیمت پایه باشد
            discount = Math.Min(discount, basePrice);
            
            totalDiscount += discount;
        }
        
        // محدودیت: مجموع تخفیف‌ها نمی‌تواند بیشتر از 100% باشد
        totalDiscount = Math.Min(totalDiscount, basePrice);
        
        return ServiceResult<decimal>.Success(totalDiscount);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در محاسبه تخفیف");
        return ServiceResult<decimal>.Failed("خطا در محاسبه تخفیف");
    }
}
```

---

### ✅ **3.3. به‌روزرسانی `AppointmentPricingService`**
**فایل:** `Services/Appointment/AppointmentPricingService.cs`

**Todo:**
- [ ] Dependency Injection: `IPromotionalEventService`
- [ ] به‌روزرسانی `CalculateDiscountAsync()`: استفاده از `IPromotionalEventService.CalculateDiscountAsync()`
- [ ] اضافه کردن `PromotionalEventId` و `DiscountAmount` به `AppointmentPriceResult`
- [ ] Logging کامل

**کد نمونه:**
```csharp
private async Task<decimal> CalculateDiscountAsync(int doctorId, int? patientId, decimal basePrice, DateTime? appointmentDate = null)
{
    try
    {
        var discountResult = await _promotionalEventService.CalculateDiscountAsync(
            doctorId, 
            basePrice, 
            appointmentDate);
        
        if (!discountResult.Success)
        {
            _logger.Warning("خطا در محاسبه تخفیف: {Error}", discountResult.Message);
            return 0m;
        }
        
        return discountResult.Data;
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در محاسبه تخفیف");
        return 0m;
    }
}
```

---

## 🎯 **مرحله 4: ViewModel Layer**

### ✅ **4.1. ایجاد `PromotionalEventViewModel`**
**فایل:** `ViewModels/PromotionalEventVM/PromotionalEventViewModel.cs`

**Todo:**
- [ ] ایجاد Class با Properties مشابه Entity
- [ ] اضافه کردن `[Display]`, `[Required]`, `[Range]` attributes
- [ ] اضافه کردن `List<int> SelectedDoctorIds` برای Multi-Select
- [ ] اضافه کردن `List<SelectListItem> AvailableDoctors` برای Dropdown
- [ ] متد `FromEntity(PromotionalEvent entity)`
- [ ] متد `ToEntity()`

---

### ✅ **4.2. ایجاد Factory `PromotionalEventViewModelFactory`**
**فایل:** `ViewModels/PromotionalEventVM/PromotionalEventViewModelFactory.cs`

**Todo:**
- [ ] ایجاد Static Factory Class
- [ ] `CreateEmpty()`: ایجاد ViewModel خالی
- [ ] `CreateFromEntity(PromotionalEvent entity)`: تبدیل Entity به ViewModel
- [ ] `CreateList(IEnumerable<PromotionalEvent> entities)`: تبدیل لیست

---

### ✅ **4.3. ایجاد Validator `PromotionalEventViewModelValidator`**
**فایل:** `ViewModels/PromotionalEventVM/PromotionalEventViewModelValidator.cs`

**Todo:**
- [ ] ایجاد Class با `AbstractValidator<PromotionalEventViewModel>`
- [ ] Rule: `Title` الزامی و حداکثر 200 کاراکتر
- [ ] Rule: `StartDate` < `EndDate`
- [ ] Rule: `DiscountValue` > 0
- [ ] Rule: اگر `DiscountType = Percentage` → `DiscountValue` <= 100
- [ ] Rule: اگر `IsDoctorSpecific = true` → حداقل یک پزشک انتخاب شود
- [ ] Rule: اگر `TotalSlots` مشخص شده → `TotalSlots` > 0

---

## 🎯 **مرحله 5: Controller Layer**

### ✅ **5.1. ایجاد `PromotionalEventController`**
**فایل:** `Areas/Admin/Controllers/PromotionalEventController.cs`

**Todo:**
- [ ] ایجاد Controller با `[Authorize(Roles = "Admin,ClinicAdmin")]`
- [ ] Dependency Injection: `IPromotionalEventService`, `IDoctorCrudService`, `ILogger`, `ICurrentUserService`
- [ ] `Index()`: نمایش لیست ایونت‌ها
- [ ] `Create()`: نمایش فرم ایجاد
- [ ] `Create(PromotionalEventViewModel model)`: POST - ایجاد ایونت
- [ ] `Edit(int id)`: نمایش فرم ویرایش
- [ ] `Edit(int id, PromotionalEventViewModel model)`: POST - ویرایش ایونت
- [ ] `Details(int id)`: نمایش جزئیات
- [ ] `Delete(int id)`: نمایش فرم حذف
- [ ] `DeleteConfirmed(int id)`: POST - حذف ایونت
- [ ] استفاده از `NotificationHelper` برای پیام‌ها
- [ ] استفاده از `ServiceResult` Pattern
- [ ] Error Handling کامل

---

## 🎯 **مرحله 6: View Layer**

### ✅ **6.1. ایجاد View `Index.cshtml`**
**فایل:** `Areas/Admin/Views/PromotionalEvent/Index.cshtml`

**Todo:**
- [ ] طراحی Card-based Layout
- [ ] جدول با ستون‌ها: عنوان، تاریخ شروع/پایان، نوع تخفیف، مقدار تخفیف، تعداد استفاده شده/کل، وضعیت
- [ ] فیلتر: تاریخ، وضعیت (فعال/غیرفعال)
- [ ] Actions: Create, Edit, Details, Delete
- [ ] استفاده از رنگ‌های استاندارد (--medical-primary)
- [ ] فونت Vazir
- [ ] RTL Support

---

### ✅ **6.2. ایجاد View `Create.cshtml`**
**فایل:** `Areas/Admin/Views/PromotionalEvent/Create.cshtml`

**Todo:**
- [ ] فرم با Validation
- [ ] Input: `Title` (Text)
- [ ] Input: `Description` (TextArea)
- [ ] Persian DatePicker برای `StartDate` و `EndDate`
- [ ] Radio Buttons برای `DiscountType` (درصدی/مبلغ ثابت)
- [ ] Input Number برای `DiscountValue`
- [ ] Checkbox برای `IsDoctorSpecific`
- [ ] Multi-Select برای انتخاب پزشکان (اگر `IsDoctorSpecific = true`)
- [ ] Input Number برای `TotalSlots` (اختیاری)
- [ ] Checkbox برای `IsActive`
- [ ] دکمه Submit و Cancel
- [ ] Validation Messages

---

### ✅ **6.3. ایجاد View `Edit.cshtml`**
**فایل:** `Areas/Admin/Views/PromotionalEvent/Edit.cshtml`

**Todo:**
- [ ] مشابه `Create.cshtml` اما با مقداردهی اولیه
- [ ] Hidden Input برای `EventId`

---

### ✅ **6.4. ایجاد View `Details.cshtml`**
**فایل:** `Areas/Admin/Views/PromotionalEvent/Details.cshtml`

**Todo:**
- [ ] نمایش کامل اطلاعات ایونت
- [ ] لیست نوبت‌های استفاده شده (با لینک به Appointment Details)
- [ ] آمار: تعداد استفاده شده، تعداد باقیمانده
- [ ] دکمه‌های Edit و Delete

---

## 🎯 **مرحله 7: Database Migration**

### ✅ **7.1. ایجاد Migration**
**فایل:** `Migrations/YYYYMMDDHHMMSS_Add_PromotionalEvents.cs`

**Todo:**
- [ ] `CreateTable("PromotionalEvents")` با تمام Columns
- [ ] `AddColumn("Appointments", "PromotionalEventId")` (nullable INT)
- [ ] `AddColumn("Appointments", "DiscountAmount")` (decimal(18,0), nullable)
- [ ] `CreateIndex` برای Performance
- [ ] `AddForeignKey` برای `Appointments.PromotionalEventId` → `PromotionalEvents.EventId`
- [ ] `Down()` Method برای Rollback

---

## 🔄 **Integration & Testing**

### ✅ **8.1. به‌روزرسانی `AppointmentBookingService`**
**فایل:** `Services/Appointment/AppointmentBookingService.cs`

**Todo:**
- [ ] در `ReserveAppointmentAsync()`: پس از محاسبه قیمت، ذخیره `PromotionalEventId` و `DiscountAmount` در `Appointment`
- [ ] پس از ذخیره `Appointment`: فراخوانی `PromotionalEventService.IncrementUsedSlotsAsync()`

---

### ✅ **8.2. به‌روزرسانی `UnityConfig`**
**فایل:** `App_Start/UnityConfig.cs`

**Todo:**
- [ ] Register `IPromotionalEventRepository` → `PromotionalEventRepository`
- [ ] Register `IPromotionalEventService` → `PromotionalEventService`

---

### ✅ **8.3. Testing**
**Todo:**
- [ ] Build موفق
- [ ] Migration اجرا شد
- [ ] Manual Test: ایجاد ایونت
- [ ] Manual Test: محاسبه تخفیف در نوبت
- [ ] Manual Test: نمایش ایونت‌ها
- [ ] Manual Test: ویرایش ایونت
- [ ] Manual Test: حذف ایونت
- [ ] Manual Test: بررسی محدودیت تعداد استفاده شده
- [ ] Manual Test: بررسی محدودیت پزشک

---

## 📝 **Notes**

- ✅ همه مبالغ باید `decimal(18,0)` باشند (طبق قرارداد مالی)
- ✅ همه عملیات باید Logging داشته باشند
- ✅ همه عملیات باید Transaction Management داشته باشند
- ✅ همه Views باید Strongly-Typed باشند
- ✅ همه Controllers باید `[Authorize]` داشته باشند
- ✅ همه POST Actions باید `[ValidateAntiForgeryToken]` داشته باشند

---

**🎯 این Todo List، راهنمای کامل برای پیاده‌سازی مرحله به مرحله است. هر Todo باید به ترتیب و با دقت کامل انجام شود.**

