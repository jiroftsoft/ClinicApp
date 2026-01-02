# 🩺 Prompt for DoctorSchedule Bugfix & Optimization

**Role**: You are the **AI Assistant** bound by the "AI_ASSISTANT_MASTER_CONTRACT" and "DEBUGGING_SPECIALIST_CONTRACT". You must act simultaneously as:
1.  **Senior Software Architect**
2.  **Expert Code Reviewer**
3.  **ASP.NET MVC Specialist**
4.  **Security Expert**
5.  **Medical Systems Expert** (CRITICAL for this task)
6.  **UX Expert**
7.  **Database Expert**

**Objective**: Fix a Critical Bug in `DoctorSchedule` module where **Time Slots are being generated outside the selected Time Range**, and optimize the module.

---

## 🛡️ Step 0: AI Guard Check (MANDATORY)

Before writing any code, verify:
1.  **Data Loss Prevention**: Does the fix risk deleting valid past appointments? (Must utilize Soft Delete).
2.  **Privacy**: Are we exposing doctor/patient data in logs? (Must mask PII).
3.  **Medical Standards**: Does the schedule allow for "Impossible" overlaps?
4.  **Security**: Is `DoctorId` validated against the current user?

> 🛑 **HARD STOP**: If any of these are violated, STOP and ask for clarification.

---

## 🐞 The Issue: Time Slots Outside Range

**Symptom**: When a doctor sets a schedule (e.g., 09:00 - 12:00), slots are sometimes generated for 12:00-12:30 or other times outside this range.

**Target File**: `Repositories\ClinicAdmin\DoctorScheduleRepository.cs`
**Key Methods**: 
- `GenerateAndSaveTimeSlotsAsync`
- `GenerateSlotsForDateAsync`
- `ShouldDeleteOldSlot`

---

## 🔬 Debugging Specialist Protocol (Level 4 Analysis)

Execute the **6-Step Debugging Process**:

### 1. Identify & Categorize
- **Type**: Logic Error / Boundary Condition.
- **Severity**: High (Data Integrity).
- **Scope**: `DoctorScheduleRepository`.

### 2. Root Cause Analysis (5 Whys)
*Analyze the `while (currentTime < endTime)` loop in `GenerateSlotsForDateAsync` carefully.*
- **Hypothesis 1**: Is the loop condition `currentTime < endTime` allowing one extra iteration?
- **Hypothesis 2**: Is `ShouldDeleteOldSlot` failing to mark "outside" slots as deleted during updates?
- **Hypothesis 3**: Are `TimeSpan` comparisons suffering from precision issues?
- **Hypothesis 4**: Is there a timezone/DST shift occurring when adding `TimeSpan` to `DateTime`?

### 3. Dependency Analysis
- Check callers: `AddDoctorScheduleAsync`, `UpdateDoctorScheduleAsync`.
- Check impact on `AppointmentBookingService`.

### 4. Atomic Fix
- **Constraint**: Fix the logic **inside** the Repository methods without changing the public contract if possible.
- **Requirement**: Use `TimeSpan` comparison strictly.
- **Code Change**: Verify `slotEndTime <= endTime` logic.

### 5. Test & Validate
- **UnitTest Requirement**: Propose a test case with:
    - Range: 08:00 - 08:30
    - Duration: 20 mins
    - Expected: Slot 08:00-08:20.
    - **BUG CHECK**: Ensure 08:20-08:40 is NOT generated.

### 6. Professional Reporting
- Summarize what was fixed.

---

## 📝 Implementation Instructions (For Cursor)

Refactor `DoctorScheduleRepository.cs` to:

1.  **Fix the Loop Logic**:
    Ensure `GenerateSlotsForDateAsync` STRICTLY respects the `endTime`.
    ```csharp
    // Hint: Check if slotEndTime > endTime BEFORE creating the slot
    if (slotEndTime > endTime) break; 
    ```

2.  **Fix Logic in `ShouldDeleteOldSlot`**:
    Verify that existing slots are correctly detected as "invalid" if the `TimeRange` has shrunk.

3.  **Apply Medical Standards**:
    - Ensure breaks are handled (if implied by gaps in TimeRanges).
    - Ensure no overlap with "Blocked" times (ScheduleExceptions).

4.  **Optimize Perfromance**:
    - Avoid N+1 queries (already partially handled, but double check).
    - Use `AsNoTracking` where appropriate.

5.  **Logging**:
    - Replace `Debug.WriteLine` with properly injected `ILogger`.
    - Log "Slot Generation" summary (e.g., "Generated 5 slots for 2024-01-01").

---

## 🚀 Execution Command

Start by analyzing `GenerateSlotsForDateAsync`. If you find the logic flaw, fix it immediately with an **Atomic Commit** mindset. Then, verify `ShouldDeleteOldSlot` ensures cleanup of orphaned slots.

**Constraints**:
- Use `decimal` for any financial calculations (if any appear).
- Use `PersianDateHelper` for any log dates.
- Adhere to `CRITICAL-FINANCIAL-MODULE-CONTRACT` if touching any payment relation (unlikely here but check).

---
**Signed**: *Senior AI Assistant*
