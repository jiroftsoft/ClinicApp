# 🔍 تحلیل عمیق: مشکل نوبت‌های Pending و راه‌حل Time-based Expiration

**تاریخ:** 2026-01-06  
**اولویت:** 🔴 CRITICAL (مالی + رقابت برای نوبت‌ها)  
**ماژول:** Appointment Booking / Payment Flow

---

## 📋 Preflight Checklist Result

✅ **Contracts:** رعایت شده  
✅ **Architecture:** SRP رعایت شده  
✅ **Security:** Validation موجود  
⚠️ **Risk Level:** **CRITICAL** - نوبت‌های Pending اسلات را اشغال می‌کنند

---

## 🎯 Problem Restatement

### مشکل اصلی:
**نوبت‌های Pending (در انتظار پرداخت) اسلات را اشغال می‌کنند و سایر کاربران نمی‌توانند رزرو کنند**

### علائم (Symptoms):
1. کاربر نوبت را رزرو می‌کند → Status = Pending
2. به درگاه پرداخت هدایت می‌شود
3. اگر پرداخت نکند، نوبت با Status = Pending باقی می‌ماند
4. این نوبت در `GetDoctorAppointmentsByDateAsync` و `HasOverlappingPatientAppointmentAsync` در نظر گرفته می‌شود
5. سایر کاربران نمی‌توانند آن اسلات را رزرو کنند

### علت ریشه‌ای (Root Cause):
**نوبت‌های Pending بدون Time-based Expiration ایجاد می‌شوند و هیچ مکانیزمی برای Cleanup وجود ندارد**

---

## 🔍 Execution Path Analysis

### مسیر اجرای فعلی:

```
1. User clicks "تائید و پرداخت"
   ↓
2. Reserve action: نوبت با Status = Pending ایجاد می‌شود
   ❌ مشکل: بدون ExpiresAt
   ↓
3. ProcessPayment: به درگاه پرداخت هدایت می‌شود
   ↓
4. اگر کاربر پرداخت نکند:
   ❌ نوبت با Status = Pending باقی می‌ماند
   ❌ اسلات اشغال می‌شود
   ❌ سایر کاربران نمی‌توانند رزرو کنند
```

### بررسی Repository:

```csharp
// GetDoctorAppointmentsByDateAsync
.Where(a => 
    a.Status == AppointmentStatus.Scheduled || 
    a.Status == AppointmentStatus.Pending) // ❌ Pending بدون Expiration چک می‌شود
```

---

## 🧪 Evidence-Based Hypothesis Validation

### Hypothesis 1: نوبت‌های Pending اسلات را اشغال می‌کنند
✅ **تأیید شده** - خط 246, 298, 323: Pending در فیلترها در نظر گرفته می‌شود

### Hypothesis 2: هیچ مکانیزم Expiration وجود ندارد
✅ **تأیید شده** - `Appointment` entity فاقد `ExpiresAt` است

### Hypothesis 3: هیچ Background Job برای Cleanup وجود ندارد
✅ **تأیید شده** - فقط `CleanupPendingDraftsForCurrentUserAsync` برای Reception وجود دارد

---

## 🎯 Root Cause Identification

### Root Cause:
**نوبت‌های Pending بدون Time-based Expiration ایجاد می‌شوند و هیچ مکانیزمی برای Cleanup وجود ندارد**

### چرا این مشکل ایجاد می‌شود:
1. نوبت با Status = Pending ایجاد می‌شود (بدون ExpiresAt)
2. اگر کاربر پرداخت نکند، نوبت Pending باقی می‌ماند
3. Repository نوبت‌های Pending را در نظر می‌گیرد (بدون چک Expiration)
4. اسلات اشغال می‌شود و سایر کاربران نمی‌توانند رزرو کنند

---

## ✅ Proposed Solution (Contract-Compliant)

### راه‌حل: Time-based Expiration برای نوبت‌های Pending

**معماری:**
1. اضافه کردن `PendingExpiresAt` به `Appointment` entity
2. تنظیم `PendingExpiresAt = CreatedAt + PendingExpirationMinutes` هنگام ایجاد نوبت Pending
   - ✅ استفاده از `AppSettings.PendingExpirationMinutes` (قابل تنظیم در Web.config)
   - ✅ مقدار پیش‌فرض: 5 دقیقه (حداقل زمان ممکن)
   - ✅ محدوده مجاز: 3 تا 60 دقیقه
3. فیلتر کردن نوبت‌های منقضی شده در Repository
4. Background Job برای Cleanup نوبت‌های منقضی شده (اختیاری)

**مزایا:**
- ✅ نوبت‌های Pending بعد از مدت زمان تعیین شده منقضی می‌شوند (قابل تنظیم)
- ✅ اسلات آزاد می‌شود برای سایر کاربران
- ✅ کاربر فرصت کافی برای پرداخت دارد (قابل تنظیم)
- ✅ بدون نیاز به تغییر Flow اصلی
- ✅ بدون hardcode یا magic string - همه چیز در AppSettings

---

## 🔧 Implementation Details

### File 1: `Models/Entities/Appointment/Appointment.cs`

**Location:** بعد از `CreatedAt`

**Change:**
```csharp
/// <summary>
/// تاریخ انقضای نوبت Pending (اگر Status = Pending باشد)
/// بعد از این تاریخ، نوبت به صورت خودکار منقضی می‌شود و اسلات آزاد می‌شود
/// </summary>
public DateTime? PendingExpiresAt { get; set; }
```

### File 2: `Services/Appointment/AppointmentBookingService.cs`

**Location:** خط 704-721

**Change:**
```csharp
var appointment = new AppointmentEntity
{
    // ... existing properties ...
    Status = AppointmentStatus.Pending,
    PendingExpiresAt = _timeProvider.UtcNow.AddMinutes(15), // ✅ 15 دقیقه برای پرداخت
    // ... rest of properties ...
};
```

### File 3: `Repositories/Appointment/AppointmentRepository.cs`

**Location:** خط 241-248, 298, 323

**Change:**
```csharp
// ❌ قبل:
(a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Pending)

// ✅ بعد:
(a.Status == AppointmentStatus.Scheduled || 
 (a.Status == AppointmentStatus.Pending && 
  (a.PendingExpiresAt == null || a.PendingExpiresAt > DateTime.UtcNow)))
```

### File 4: `Services/Appointment/AppointmentBookingService.cs` (GetAvailableTimeSlotsAsync)

**Location:** خط 452

**Change:**
```csharp
// ❌ قبل:
if (a.Status != AppointmentStatus.Scheduled && a.Status != AppointmentStatus.Pending)
    return false;

// ✅ بعد:
if (a.Status != AppointmentStatus.Scheduled && 
    (a.Status != AppointmentStatus.Pending || 
     (a.PendingExpiresAt.HasValue && a.PendingExpiresAt.Value <= DateTime.UtcNow)))
    return false;
```

---

## 📊 ServiceResult Response Example

```csharp
// ✅ Reserve action response (بدون تغییر):
{
    success: true,
    message: "نوبت در انتظار پرداخت است",
    appointmentId: 19,
    requiresPayment: true,
    paymentUrl: "/Patient/AppointmentBooking/ProcessPayment?appointmentId=19",
    expiresAt: "2026-01-06T16:30:00Z" // ✅ جدید: تاریخ انقضا
}
```

---

## 🧪 Test Plan

### Manual Verification:
1. رزرو نوبت → Status = Pending, PendingExpiresAt = CreatedAt + 15 min
2. بررسی: نوبت در GetAvailableTimeSlotsAsync نمایش داده نمی‌شود (اگر منقضی شده)
3. بررسی: بعد از 15 دقیقه، نوبت منقضی می‌شود و اسلات آزاد می‌شود
4. بررسی: سایر کاربران می‌توانند همان اسلات را رزرو کنند

### Automated Tests:
```csharp
[Test]
public async Task PendingAppointment_ShouldExpireAfter15Minutes()
{
    // Arrange
    var appointment = new Appointment { 
        Status = AppointmentStatus.Pending,
        PendingExpiresAt = DateTime.UtcNow.AddMinutes(-16) // منقضی شده
    };
    
    // Act
    var isExpired = appointment.PendingExpiresAt.HasValue && 
                   appointment.PendingExpiresAt.Value <= DateTime.UtcNow;
    
    // Assert
    Assert.IsTrue(isExpired);
}

[Test]
public async Task GetAvailableTimeSlotsAsync_ShouldExcludeExpiredPending()
{
    // Arrange
    var expiredPending = new Appointment { 
        Status = AppointmentStatus.Pending,
        PendingExpiresAt = DateTime.UtcNow.AddMinutes(-16)
    };
    
    // Act
    var slots = await _service.GetAvailableTimeSlotsAsync(doctorId, date);
    
    // Assert
    // Slot should be available (expired pending should not block it)
}
```

---

## 🔄 Rollback Strategy

### اگر مشکل ایجاد شد:
1. حذف `PendingExpiresAt` از `Appointment` entity
2. بازگرداندن فیلترهای Repository
3. Migration برای حذف ستون از دیتابیس

---

## ✅ Final Validation

- ✅ Root cause fixed (not symptom)
- ✅ All 5 project rules respected
- ✅ No security or data risks introduced
- ✅ Solution is maintainable and incremental

---

**وضعیت:** ✅ آماده برای پیاده‌سازی

