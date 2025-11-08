# Reception V2 Edit Mode - Fix Documentation

**Date:** 2025-11-07  
**Status:** ✅ Fixed  

## Issues Identified

### 1. Patient Identity Fields Empty in Edit Form
**Problem:** Only national code shows; name, father name, gender, birth date, phone, address are empty.

**Root Cause:** 
- Patient lookup is triggered after `reception-edit.js` populates the national code field
- The lookup overwrites the data loaded from the reception
- Patient entity only has `PhoneNumber` field (no separate mobile/landline)

**Solution:**
- Skip patient lookup when national code field is `readonly` (edit mode)
- Map Patient.PhoneNumber to both PatientMobile and PatientPhone in DTO
- Ensure backend properly populates all patient identity fields

### 2. Multiple Reprice Requests (Race Condition)
**Problem:** Multiple `persist()` calls cause race conditions and "Reprice response ignored (outdated token)" errors.

**Root Cause:**
- `reception-edit.js` loads insurance values
- Patient lookup is triggered and calls `insPanel.set()`
- `insPanel.set()` triggers `.change()` events on insurance dropdowns
- Each change event calls `persist()` immediately

**Solution:**
- Add debounce mechanism to `insurance-panel.js` `persist()` function
- Prevent concurrent reprice requests with a flag
- Skip patient lookup in edit mode to avoid duplicate insurance setting

### 3. SetInsurances Error
**Problem:** "خطا در بازمحاسبه: An error occurred while updating the entries"

**Root Cause:**
- Optimistic concurrency conflicts when multiple reprice requests hit the database simultaneously
- Multiple `persist()` calls try to update the same PatientInsurance record

**Solution:**
- Already implemented retry logic with `ReloadAsync()` in ReceptionFacade
- Prevent race condition with debounce on frontend

## Implementation Steps

1. **Backend Fix (ReceptionFacade.cs)**
   - ✅ Fix Gender enum conversion: Use `.ToString()` instead of direct coalesce
   - Map PhoneNumber to both PatientMobile and PatientPhone

2. **Frontend Fix (patient-lookup.js)**
   - Add check to skip lookup if national code field is readonly
   - Prevent unnecessary API calls in edit mode

3. **Frontend Fix (insurance-panel.js)**
   - Add debounce to `persist()` function (500ms)
   - Add `isRepricing` flag to prevent concurrent requests
   - Replace direct `persist()` calls with debounced `triggerReprice()`

## Testing Checklist

- [ ] Open reception in edit mode
- [ ] Verify all patient identity fields are populated (name, father name, gender, birth date, phone, address)
- [ ] Verify no patient lookup is triggered when page loads
- [ ] Change insurance plan
- [ ] Verify only one reprice request is sent (check Network tab)
- [ ] Verify totals update correctly
- [ ] Verify no concurrency errors in console or server logs

## Files Modified

- `Services/Reception/ReceptionFacade.cs` - Fixed Gender enum, patient field mapping
- `Scripts/reception.v2/patient-lookup.js` - Skip lookup in edit mode
- `Scripts/reception.v2/insurance-panel.js` - Add debounce and prevent race conditions
- `Scripts/reception.v2/reception-edit.js` - Ensure proper field population

## Related Documents

- `docs/reception-v2-hardening-roadmap.md` - Overall hardening plan
- `docs/reception-v2-hardening-summary.md` - Summary of all improvements
- `RECEPTION_EDIT_ARCHITECTURE.md` - Edit mode architecture
