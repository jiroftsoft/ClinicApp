# 🧪 ClinicApp – Scenario Matrix Template (Healthcare · Production)

> **Purpose:** عملیاتی‌کردن همه‌ی سناریوهای واقعی کاربر در محیط درمانی قبل از کدنویسی یا تغییر.
> این فایل برای جلوگیری از Flow شکسته، سردرگمی کاربر و باگ Production طراحی شده.
> 
> **Contract Compliance:** Flow Discipline Contract (FINAL) - Section 3 (Mandatory Flow Discipline)

---

## 1) Flow Overview
- **Flow Name:** 
- **Entry Point:** 
- **Expected Final Destination:** 
- **Return Destination After Interruptions:** (Auth, OTP, Errors)
- **Criticality:** Low / Medium / High / CRITICAL

---

## 2) Actor
- [ ] Patient
- [ ] Doctor
- [ ] Staff
- [ ] Admin

---

## 3) Mandatory Branch Categories (MUST ENUMERATE ALL)

### 3.1) Authentication States
- [ ] Logged in
- [ ] Not logged in (requires auth redirect)
- [ ] Session expired during flow
- [ ] Multi-device login conflict
- [ ] Account locked/suspended

### 3.2) Network & System Failures
- [ ] Network failure (offline, timeout)
- [ ] Server timeout (5xx errors)
- [ ] Database unavailable
- [ ] External service down (payment, SMS, etc.)
- [ ] Rate limiting triggered

### 3.3) Navigation Interruptions
- [ ] Back button pressed
- [ ] Browser refresh (F5)
- [ ] Tab closure / reopening
- [ ] Multi-tab usage (same flow in multiple tabs)
- [ ] Direct URL navigation (bypassing flow)

### 3.4) Partial Completion
- [ ] Flow abandonment (user navigates away)
- [ ] Partial form submission (incomplete data)
- [ ] OTP sent but not verified
- [ ] Payment initiated but not completed
- [ ] Multi-step flow stopped mid-way

### 3.5) Validation & Error States
- [ ] Validation failure (client-side)
- [ ] Validation failure (server-side)
- [ ] OTP failure / expiry
- [ ] Concurrent modification conflict
- [ ] Business rule violation
- [ ] Duplicate submission attempt

### 3.6) Context Preservation
- [ ] Return destination after auth redirect
- [ ] Return destination after OTP verification
- [ ] Return destination after error recovery
- [ ] Return destination after back button
- [ ] Data preservation during interruptions

❗ **Missing scenario = BUG** - All categories above MUST have at least one scenario

---

## 4) Scenario Matrix

| # | Preconditions | User Action | System State | Expected Result | Return Destination | Safe Failure Behavior | Fallback / Recovery |
|---|---------------|-------------|--------------|-----------------|---------------------|----------------------|---------------------|
| 1 |               |             |              |                 |                     |                       |                     |

### Safe Failure Behavior Definition (per scenario):
For each failure scenario, define:
- **User knows what happened:** [Clear error message/notification]
- **User knows what to do next:** [Explicit action/CTA]
- **Data loss prevention:** [Method: TempData, Session, LocalStorage, etc.]

---

## 5) Return Destination Map

| Interruption Type | Return Destination | Context Preservation Method | Test Status |
|-------------------|-------------------|------------------------------|-------------|
| Auth redirect     |                    |                              | [ ] Pass    |
| OTP verification  |                    |                              | [ ] Pass    |
| Error recovery    |                    |                              | [ ] Pass    |
| Back button       |                    |                              | [ ] Pass    |
| Browser refresh   |                    |                              | [ ] Pass    |
| Tab closure       |                    |                              | [ ] Pass    |

---

## 6) Data Safety
- [ ] Context preserved (all interruption scenarios)
- [ ] No duplicate submit (idempotency verified)
- [ ] No PII loss (data recovery tested)
- [ ] No PII leakage (logs, errors, URLs checked)
- [ ] Session/TempData cleanup verified

---

## 7) Verification

### 7.1) Manual Test Steps
1. [Step 1: Happy path]
2. [Step 2: Branch scenario]
3. [Step 3: Failure scenario]
4. [Step 4: Recovery scenario]

### 7.2) Expected vs Actual

| Scenario # | Expected Behavior | Actual Behavior | Status | Notes |
|------------|-------------------|-----------------|--------|-------|
| 1          |                   |                 | [ ]    |       |
| 2          |                   |                 | [ ]    |       |

### 7.3) Edge Cases Tested
- [ ] Network failure simulation (offline mode)
- [ ] Session expiry simulation (timeout)
- [ ] Multi-tab behavior (same flow in 2+ tabs)
- [ ] Back button behavior (navigation history)
- [ ] Browser refresh behavior (F5 during flow)
- [ ] OTP expiry (time-based)
- [ ] Concurrent modification (2 users, same data)
- [ ] Rate limiting (multiple rapid requests)

### 7.4) Healthcare UX Compliance
- [ ] Error messages are clear and actionable
- [ ] No dead ends (user always has next step)
- [ ] No silent redirects (user knows where they're going)
- [ ] Mobile-first verified (responsive design)
- [ ] Accessibility verified (screen reader, keyboard nav)
- [ ] Formal, calm tone (no flashy colors/animations)

---

## 8) Regression Safety

### What Could Break?
- [List potential breaking points]

### How Is It Protected?
- [List protection mechanisms]

---

## 9) Open Questions (Blocking Only)

1. [Question 1 - if blocking]
2. [Question 2 - if blocking]

---

END
