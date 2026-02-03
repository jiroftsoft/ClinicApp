# 🗺️ نقشه راه معماری: سیستم تخفیف برای ایونت‌ها (Promotional Events)

**تاریخ:** 2026-01-07  
**نسخه:** 1.0.0  
**وضعیت:** 🟡 در حال طراحی  
**هدف:** پیاده‌سازی کامل سیستم مدیریت ایونت‌های تبلیغاتی و اعمال تخفیف خودکار

---

## 📋 **خلاصه اجرایی**

این نقشه راه، پیاده‌سازی کامل سیستم تخفیف برای ایونت‌های تبلیغاتی را شامل می‌شود:

1. **Entity Layer:** ایجاد `PromotionalEvent` و `DiscountType` enum
2. **Service Layer:** به‌روزرسانی `AppointmentPricingService` برای محاسبه تخفیف
3. **Repository Layer:** ایجاد Repository برای CRUD ایونت‌ها
4. **Controller Layer:** ایجاد `PromotionalEventController` در Admin Area
5. **View Layer:** ایجاد Views برای مدیریت ایونت‌ها
6. **Integration:** اتصال به `Appointment` و به‌روزرسانی `AppointmentPricingService`
7. **Database:** Migration برای ایجاد جدول `PromotionalEvents`

---

## 🏗️ **معماری کلی**

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Areas/Admin/Views/PromotionalEvent/                  │  │
│  │  - Index.cshtml (لیست ایونت‌ها)                       │  │
│  │  - Create.cshtml (ایجاد ایونت جدید)                   │  │
│  │  - Edit.cshtml (ویرایش ایونت)                         │  │
│  │  - Details.cshtml (جزئیات ایونت)                      │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Areas/Admin/Controllers/PromotionalEventController   │  │
│  │  - Index()                                            │  │
│  │  - Create() / Create(POST)                           │  │
│  │  - Edit() / Edit(POST)                                │  │
│  │  - Details()                                          │  │
│  │  - Delete() / DeleteConfirmed()                       │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                      SERVICE LAYER                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Services/Appointment/AppointmentPricingService      │  │
│  │  - CalculateDiscountAsync() [به‌روزرسانی]            │  │
│  │  - GetActivePromotionalEventsAsync() [جدید]           │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Services/PromotionalEvent/                          │  │
│  │  PromotionalEventService [جدید]                      │  │
│  │  - CreateAsync()                                     │  │
│  │  - UpdateAsync()                                     │  │
│  │  - DeleteAsync()                                     │  │
│  │  - GetActiveEventsAsync()                            │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    REPOSITORY LAYER                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Repositories/PromotionalEvent/                      │  │
│  │  IPromotionalEventRepository [جدید]                 │  │
│  │  PromotionalEventRepository [جدید]                   │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                      DATA LAYER                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Models/Entities/PromotionalEvent/                   │  │
│  │  - PromotionalEvent.cs [جدید]                        │  │
│  │  - PromotionalEventConfig.cs [جدید]                  │  │
│  │  - DiscountType.cs (enum) [جدید]                      │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Models/Entities/Appointment/Appointment.cs          │  │
│  │  - PromotionalEventId (FK) [اضافه]                   │  │
│  │  - DiscountAmount [اضافه]                             │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    DATABASE LAYER                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Migrations/YYYYMMDDHHMMSS_Add_PromotionalEvents.cs  │  │
│  │  - CreateTable PromotionalEvents                     │  │
│  │  - AddColumn Appointment.PromotionalEventId          │  │
│  │  - AddColumn Appointment.DiscountAmount              │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 **مراحل پیاده‌سازی (7 مرحله)**

### **مرحله 1: Entity Layer (پایه داده)**
**هدف:** ایجاد Entity و Enum برای `PromotionalEvent`

**خروجی:**
- `Models/Entities/PromotionalEvent/PromotionalEvent.cs`
- `Models/Entities/PromotionalEvent/PromotionalEventConfig.cs`
- `Models/Enums/DiscountType.cs`
- به‌روزرسانی `Models/Entities/Appointment/Appointment.cs` (افزودن `PromotionalEventId` و `DiscountAmount`)

**الزامات:**
- ✅ `ISoftDelete` و `ITrackable` پیاده‌سازی شود
- ✅ `decimal(18,0)` برای `DiscountValue`
- ✅ Index برای `StartDate`, `EndDate`, `IsActive`
- ✅ Navigation Property به `Appointment`

---

### **مرحله 2: Repository Layer**
**هدف:** ایجاد Repository برای CRUD ایونت‌ها

**خروجی:**
- `Repositories/PromotionalEvent/IPromotionalEventRepository.cs`
- `Repositories/PromotionalEvent/PromotionalEventRepository.cs`

**الزامات:**
- ✅ Interface با متدهای CRUD
- ✅ `GetActiveEventsAsync()` برای ایونت‌های فعال
- ✅ `GetEventsByDateRangeAsync()` برای فیلتر تاریخ
- ✅ `GetEventsByDoctorAsync()` برای فیلتر پزشک
- ✅ Logging با Serilog

---

### **مرحله 3: Service Layer**
**هدف:** ایجاد Service برای Business Logic

**خروجی:**
- `Services/PromotionalEvent/PromotionalEventService.cs`
- به‌روزرسانی `Services/Appointment/AppointmentPricingService.cs`

**الزامات:**
- ✅ `ServiceResult<T>` Pattern
- ✅ Validation (FluentValidation)
- ✅ Transaction Management
- ✅ Logging کامل
- ✅ `CalculateDiscountAsync()` در `AppointmentPricingService`
- ✅ `IncrementUsedSlotsAsync()` برای افزایش تعداد استفاده شده

---

### **مرحله 4: ViewModel Layer**
**هدف:** ایجاد ViewModels برای Views

**خروجی:**
- `ViewModels/PromotionalEventVM/PromotionalEventViewModel.cs`
- `ViewModels/PromotionalEventVM/PromotionalEventViewModelFactory.cs`
- `ViewModels/PromotionalEventVM/PromotionalEventViewModelValidator.cs`

**الزامات:**
- ✅ Strongly-Typed ViewModels
- ✅ Factory Pattern
- ✅ FluentValidation
- ✅ `FromEntity()` و `ToEntity()` Methods

---

### **مرحله 5: Controller Layer**
**هدف:** ایجاد Controller برای Admin Area

**خروجی:**
- `Areas/Admin/Controllers/PromotionalEventController.cs`

**الزامات:**
- ✅ `[Authorize(Roles = "Admin,ClinicAdmin")]`
- ✅ `[ValidateAntiForgeryToken]` در POST
- ✅ `NotificationHelper` برای پیام‌ها
- ✅ ServiceResult Pattern
- ✅ Error Handling کامل

---

### **مرحله 6: View Layer**
**هدف:** ایجاد Views برای UI

**خروجی:**
- `Areas/Admin/Views/PromotionalEvent/Index.cshtml`
- `Areas/Admin/Views/PromotionalEvent/Create.cshtml`
- `Areas/Admin/Views/PromotionalEvent/Edit.cshtml`
- `Areas/Admin/Views/PromotionalEvent/Details.cshtml`

**الزامات:**
- ✅ طراحی رسمی و حرفه‌ای (Card Components)
- ✅ رنگ‌های استاندارد (--medical-primary)
- ✅ فونت Vazir
- ✅ RTL Support
- ✅ Persian DatePicker برای تاریخ‌ها
- ✅ Multi-Select برای انتخاب پزشکان
- ✅ Validation Messages

---

### **مرحله 7: Database Migration**
**هدف:** ایجاد Migration برای Database Schema

**خروجی:**
- `Migrations/YYYYMMDDHHMMSS_Add_PromotionalEvents.cs`

**الزامات:**
- ✅ CreateTable `PromotionalEvents`
- ✅ AddColumn `Appointment.PromotionalEventId` (nullable FK)
- ✅ AddColumn `Appointment.DiscountAmount` (decimal(18,0))
- ✅ CreateIndex برای Performance
- ✅ ForeignKey Constraint

---

## 🔄 **جریان کاری (Workflow)**

### **1. ایجاد ایونت تبلیغاتی (Admin)**
```
Admin → PromotionalEventController.Create()
  → PromotionalEventService.CreateAsync()
    → PromotionalEventRepository.AddAsync()
      → Database (PromotionalEvents)
```

### **2. محاسبه قیمت نوبت (Patient)**
```
Patient → AppointmentBookingController.Reserve()
  → AppointmentPricingService.CalculatePriceAsync()
    → GetBasePriceAsync() (از DoctorSchedule.ConsultationFee)
    → CalculateDiscountAsync() [جدید]
      → PromotionalEventRepository.GetActiveEventsAsync()
        → بررسی تاریخ، تعداد استفاده، محدودیت پزشک
        → محاسبه تخفیف (درصدی یا مبلغ ثابت)
    → FinalPrice = BasePrice - Discount
  → Appointment.Price = FinalPrice
  → Appointment.PromotionalEventId = EventId
  → Appointment.DiscountAmount = Discount
  → PromotionalEventService.IncrementUsedSlotsAsync()
```

### **3. نمایش ایونت‌ها (Admin)**
```
Admin → PromotionalEventController.Index()
  → PromotionalEventService.GetAllAsync()
    → PromotionalEventRepository.GetAllAsync()
      → Database (PromotionalEvents)
  → View (Index.cshtml)
```

---

## 📊 **ساختار Database**

### **جدول `PromotionalEvents`**
```sql
CREATE TABLE [dbo].[PromotionalEvents] (
    [EventId] INT IDENTITY(1,1) PRIMARY KEY,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(1000) NULL,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2 NOT NULL,
    [DiscountType] TINYINT NOT NULL, -- 1=Percentage, 2=FixedAmount
    [DiscountValue] DECIMAL(18,0) NOT NULL,
    [TotalSlots] INT NULL, -- NULL = نامحدود
    [UsedSlots] INT NOT NULL DEFAULT 0,
    [IsDoctorSpecific] BIT NOT NULL DEFAULT 0,
    [DoctorIds] NVARCHAR(MAX) NULL, -- JSON Array: [1,2,3]
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedAt] DATETIME2 NULL,
    [DeletedByUserId] NVARCHAR(128) NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedByUserId] NVARCHAR(128) NOT NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedByUserId] NVARCHAR(128) NULL
);

-- Indexes
CREATE INDEX IX_PromotionalEvent_StartDate_EndDate_IsActive 
    ON [dbo].[PromotionalEvents]([StartDate], [EndDate], [IsActive]);
CREATE INDEX IX_PromotionalEvent_IsDeleted ON [dbo].[PromotionalEvents]([IsDeleted]);
```

### **به‌روزرسانی جدول `Appointments`**
```sql
ALTER TABLE [dbo].[Appointments]
    ADD [PromotionalEventId] INT NULL,
        [DiscountAmount] DECIMAL(18,0) NULL;

ALTER TABLE [dbo].[Appointments]
    ADD CONSTRAINT FK_Appointments_PromotionalEvents 
    FOREIGN KEY ([PromotionalEventId]) 
    REFERENCES [dbo].[PromotionalEvents]([EventId]);
```

---

## 🎨 **UI/UX Requirements**

### **Index View:**
- ✅ Card-based Layout
- ✅ فیلتر: تاریخ، وضعیت (فعال/غیرفعال)
- ✅ نمایش: عنوان، تاریخ شروع/پایان، نوع تخفیف، تعداد استفاده شده/کل
- ✅ Actions: Create, Edit, Details, Delete

### **Create/Edit View:**
- ✅ فرم با Validation
- ✅ Persian DatePicker برای `StartDate` و `EndDate`
- ✅ Radio Buttons برای `DiscountType` (درصدی/مبلغ ثابت)
- ✅ Input Number برای `DiscountValue`
- ✅ Checkbox برای `IsDoctorSpecific`
- ✅ Multi-Select برای انتخاب پزشکان (اگر `IsDoctorSpecific = true`)
- ✅ Input Number برای `TotalSlots` (اختیاری)

### **Details View:**
- ✅ نمایش کامل اطلاعات ایونت
- ✅ لیست نوبت‌های استفاده شده (با لینک به Appointment Details)
- ✅ آمار: تعداد استفاده شده، تعداد باقیمانده

---

## 🔒 **Security & Validation**

### **Authorization:**
- ✅ فقط `Admin` و `ClinicAdmin` می‌توانند ایونت ایجاد/ویرایش/حذف کنند
- ✅ `[Authorize(Roles = "Admin,ClinicAdmin")]` در Controller

### **Validation:**
- ✅ `StartDate` < `EndDate`
- ✅ `DiscountValue` > 0
- ✅ اگر `DiscountType = Percentage` → `DiscountValue` <= 100
- ✅ اگر `IsDoctorSpecific = true` → حداقل یک پزشک انتخاب شود
- ✅ اگر `TotalSlots` مشخص شده → `TotalSlots` > 0

### **Business Rules:**
- ✅ ایونت فقط در بازه زمانی `StartDate` تا `EndDate` فعال است
- ✅ اگر `TotalSlots` مشخص شده و `UsedSlots >= TotalSlots` → ایونت غیرفعال می‌شود
- ✅ تخفیف نمی‌تواند بیشتر از قیمت پایه باشد
- ✅ اگر چند ایونت فعال باشد → مجموع تخفیف‌ها اعمال می‌شود (با محدودیت حداکثر 100% تخفیف)

---

## 📝 **Logging Requirements**

### **Serilog Logs:**
```csharp
// ایجاد ایونت
_logger.Information("🎁 PROMOTIONAL EVENT: ایجاد ایونت جدید - EventId: {EventId}, Title: {Title}, DiscountType: {DiscountType}, DiscountValue: {DiscountValue}", 
    eventId, title, discountType, discountValue);

// محاسبه تخفیف
_logger.Information("💰 DISCOUNT: محاسبه تخفیف - DoctorId: {DoctorId}, BasePrice: {BasePrice}, EventId: {EventId}, Discount: {Discount}", 
    doctorId, basePrice, eventId, discount);

// افزایش تعداد استفاده شده
_logger.Information("📊 PROMOTIONAL EVENT: افزایش تعداد استفاده - EventId: {EventId}, UsedSlots: {UsedSlots}/{TotalSlots}", 
    eventId, usedSlots, totalSlots);
```

---

## ✅ **Checklist نهایی**

### **قبل از شروع:**
- [ ] مطالعه کامل `Contracts/AI_CORE_COMMITMENT.md`
- [ ] مطالعه `Docs/APPOINTMENT_PRICING_ANALYSIS.md`
- [ ] بررسی Pattern های موجود در `Areas/Admin/Controllers/`

### **بعد از تکمیل:**
- [ ] Build موفق
- [ ] Migration اجرا شد
- [ ] Manual Test: ایجاد ایونت
- [ ] Manual Test: محاسبه تخفیف در نوبت
- [ ] Manual Test: نمایش ایونت‌ها
- [ ] Manual Test: حذف ایونت
- [ ] Code Review

---

**🎯 این نقشه راه، راهنمای کامل برای پیاده‌سازی سیستم تخفیف است. هر مرحله باید به ترتیب و با دقت کامل انجام شود.**

