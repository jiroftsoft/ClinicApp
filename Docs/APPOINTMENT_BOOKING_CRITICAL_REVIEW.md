# ⚡ AppointmentBooking – Critical Review (Production)

---

## 1) Preflight

**Scope:** `AppointmentBookingController.cs` (7 actions) + `AppointmentBookingService.cs` + `AppointmentRepository.cs`  
**Risk:** 🔴 **CRITICAL**  
**Tests:** ⚠️ None found

---

## 2) Critical Findings (7)

### 🔴 #1: Authentication Bypass

**Evidence:** `AppointmentBookingController.cs:318,490,620`
```csharp
[AllowAnonymous] // ⚠️ TEMPORARY
var patientId = 1; // ⚠️ TEMPORARY: فقط برای تست
```

**Impact:** Any user books as Patient #1. Data ownership violation.

---

### 🔴 #2: No Idempotency for Booking

**Evidence:** `AppointmentBookingController.cs:601` - `Reserve()` method
- No `_idempotencyService.TryUseKeyAsync()` before `ReserveAppointmentAsync()`
- Idempotency only in `ProcessPayment` (line 719)

**Impact:** Double-submit → duplicate appointments.

---

### 🔴 #3: Race Condition

**Evidence:** 
- `AppointmentBookingService.cs:546` - `CheckSlotAvailabilityAsync()` called in `ConfirmBooking()` (outside transaction)
- `AppointmentBookingService.cs:491` - `ReserveAppointmentAsync()` creates appointment in separate transaction
- Gap between check and create → TOCTOU

**Impact:** Two users book same slot concurrently.

---

### 🟡 #4: Missing Flow State

**Evidence:** `AppointmentBookingController.cs:318` - `SelectDate()` has `[AllowAnonymous]` but no `returnUrl` preservation

**Impact:** Auth redirect → booking progress lost.

---

### 🟡 #5: No Final Check in Reserve

**Evidence:** `AppointmentBookingService.cs:491-620` - `ReserveAppointmentAsync()` transaction does NOT call `CheckSlotAvailabilityAsync()` inside

**Impact:** Slot taken between Confirm and Reserve.

---

### 🟢 #6: PII Leak Risk

**Evidence:** `AppointmentBookingService.cs:90,111` - Exceptions may leak in user-facing errors

**Impact:** Information disclosure.

---

### 🟢 #7: JS Retry Risk

**Evidence:** `confirm-booking.js:58` - `maxRetries: 1` but no idempotency key from client

**Impact:** Network retry → duplicate booking.

---

## 3) Root Cause

- **#1-4:** Auth disabled for testing, never re-enabled
- **#2,7:** Idempotency incomplete (payment only)
- **#3,5:** Transaction boundary wrong (check outside, create inside)

---

## 4) Fix (Minimal)

**Change:** Re-enable auth + Add idempotency + Fix race condition  
**Files:** `AppointmentBookingController.cs`, `AppointmentBookingService.cs`

---

## 5) Diff

**File 1:** `AppointmentBookingController.cs`
```csharp
// Line 318
- [AllowAnonymous] // ⚠️ TEMPORARY
+ // Auth required

// Lines 339-342, 399, 487, 617
- // ⚠️ AUTHENTICATION DISABLED
- // var patientId = await GetCurrentPatientIdAsync();
- var patientId = 1; // ⚠️ TEMPORARY
+ var patientId = await GetCurrentPatientIdAsync();
+ if (patientId == null)
+     return RedirectToAction("Login", "Account", new { returnUrl = Request.Url.PathAndQuery });

// Line 601 - Reserve()
+ var idempotencyKey = $"booking_{model.DoctorId}_{model.AppointmentDate:yyyyMMdd}_{model.StartTime}_{patientId}";
+ var canProcess = await _idempotencyService.TryUseKeyAsync(idempotencyKey, 30, "appointment_booking");
+ if (!canProcess) {
+     var existing = await _context.Appointments.FirstOrDefaultAsync(a => 
+         a.DoctorId == model.DoctorId && a.AppointmentDate.Date == model.AppointmentDate.Date &&
+         a.AppointmentDate.TimeOfDay == model.StartTime && a.PatientId == patientId && !a.IsDeleted);
+     if (existing != null) return Json(new { success = true, message = "نوبت قبلاً رزرو شده است", appointmentId = existing.AppointmentId });
+     return Json(new { success = false, message = "درخواست تکراری" });
+ }
```

**File 2:** `AppointmentBookingService.cs`
```csharp
// Line 491 - ReserveAppointmentAsync() - Inside transaction, before CreateAppointmentAsync()
+ var availabilityCheck = await _appointmentRepository.CheckSlotAvailabilityAsync(
+     request.DoctorId, request.AppointmentDate, request.StartTime, request.EndTime);
+ if (!availabilityCheck) {
+     transaction.Rollback();
+     return ServiceResult<AppointmentEntity>.Failed("این زمان دیگر در دسترس نیست");
+ }
```

---

## 6) Tests

**Unit:** `ReserveAppointmentAsync` concurrent → one succeeds | `GetCurrentPatientIdAsync` null → null | Idempotency reuse → existing  
**Integration:** Two users same slot → one succeeds | Auth redirect → resume  
**Manual:** Happy path | Auth interruption | Double submit

## 7) Verify

1. Auth required SelectDate/SelectTime/Confirm/Reserve
2. Double-click Reserve → one appointment
3. Two users same slot → one succeeds
4. Auth redirect → returns to step
5. Network retry → no duplicate

## 8) Rollback

`git revert <commit>` | No DB migration | Restore `[AllowAnonymous]` + `patientId = 1` | Remove idempotency | Move check outside transaction

## 9) Verdict

⚠️ **Go with Risk**
- Auth re-enable may break test users
- Idempotency key format validation needed
- Transaction performance under load
- Deploy staging first, monitor auth failures/duplicates/deadlocks

