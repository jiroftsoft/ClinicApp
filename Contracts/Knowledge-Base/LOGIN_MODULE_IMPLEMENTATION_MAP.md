# 🔐 Login Module - Implementation Map (Production-Ready)

## 📋 Overview
Complete implementation map for Login & Registration module with OTP authentication, covering all scenarios, device types, and edge cases.

---

## 🎯 Module Structure (SRP Compliance)

### 1. **View Layer** (`Views/Account/_LoginModal.cshtml`)
- **Responsibility:** UI rendering only (PASSIVE)
- **No business logic**
- **Delegates to:** OTPManager, Form Handlers

### 2. **OTP Manager** (`Content/js/login-otp-manager.js`)
- **Responsibility:** OTP input behavior management
- **Single Purpose:** Handle all OTP input interactions
- **Features:**
  - Digit-only input validation
  - Auto-focus navigation
  - Paste support
  - Arrow key navigation
  - Backspace/Delete handling

### 3. **Form Handlers** (`Views/Account/_LoginModal.cshtml` - JavaScript)
- **Responsibility:** AJAX form submissions
- **Handles:**
  - CheckUser
  - SendLoginOtp
  - SendRegistrationOtp
  - VerifyLoginOtp
  - VerifyRegistrationOtp

### 4. **Controller** (`Controllers/AccountController.cs`)
- **Responsibility:** Request handling, validation, service delegation
- **Actions:**
  - `LoginModal()` - Returns partial view
  - `CheckUser()` - Validates national code
  - `SendLoginOtp()` - Sends OTP for existing users
  - `SendRegistrationOtp()` - Sends OTP for new users
  - `VerifyLoginOtp()` - Verifies and signs in
  - `VerifyRegistrationOtp()` - Verifies and creates account

### 5. **Service Layer** (`Services/AuthService.cs`)
- **Responsibility:** Business logic, security, OTP management
- **Methods:**
  - `CheckUserExistsAsync()`
  - `SendLoginOtpAsync()`
  - `SendRegistrationOtpAsync()`
  - `VerifyLoginOtpAndSignInAsync()`
  - `VerifyRegistrationOtpAsync()`

---

## 📱 Device Support Matrix

### Mobile (320px - 767px)
- **OTP Input Size:** 3rem × 3.75rem
- **Font Size:** 1.5rem (20px to prevent iOS zoom)
- **Gap:** 0.5rem
- **Touch Target:** Minimum 44px × 44px ✅
- **Keyboard:** Numeric keypad
- **Features:**
  - Large, readable numbers
  - Touch-friendly spacing
  - No zoom on focus
  - Swipe navigation support

### Tablet (768px - 991px)
- **OTP Input Size:** 4rem × 4.5rem
- **Font Size:** 2rem
- **Gap:** 1rem
- **Features:**
  - Comfortable spacing
  - Clear visibility
  - Touch and mouse support

### Desktop (992px+)
- **OTP Input Size:** 4.5rem × 5rem (up to 5rem × 5.5rem on large screens)
- **Font Size:** 2.25rem - 2.5rem
- **Gap:** 1.25rem
- **Features:**
  - Keyboard navigation (Arrow keys)
  - Mouse hover effects
  - High contrast

---

## 🔄 Complete Flow Scenarios

### Scenario 1: Existing User Login
1. **User enters National Code** → `CheckUser`
2. **Response:** `USER_ALREADY_EXISTS` (success: false)
3. **Frontend:** Calls `sendLoginOtp()`
4. **Backend:** `SendLoginOtpAsync()`
   - Validates national code
   - Finds user
   - Checks account lockout
   - Applies rate limiting
   - Generates OTP
   - Stores in `OtpState` with expiration
   - Logs to `OtpRequest` table
   - Sends SMS
5. **Frontend:** Shows OTP step, starts countdown
6. **User enters OTP** → `VerifyLoginOtp`
7. **Backend:** `VerifyLoginOtpAndSignInAsync()`
   - Validates OTP hash
   - Checks expiration
   - Verifies session binding (IP/UserAgent)
   - Signs in user
   - Marks OTP as verified
8. **Frontend:** Redirects to dashboard

### Scenario 2: New User Registration
1. **User enters National Code** → `CheckUser`
2. **Response:** `USER_IS_NEW` (success: true)
3. **Frontend:** Shows phone number step
4. **User enters Phone Number** → `SendRegistrationOtp`
5. **Backend:** `SendRegistrationOtpAsync()`
   - Validates phone number
   - Applies rate limiting
   - Generates OTP
   - Stores in `OtpState`
   - Sends SMS
6. **Frontend:** Shows OTP step
7. **User enters OTP** → `VerifyRegistrationOtp`
8. **Backend:** `VerifyRegistrationOtpAsync()`
   - Validates OTP
   - Creates user account
   - Signs in
9. **Frontend:** Redirects to complete registration

### Scenario 3: Invalid OTP
1. **User enters wrong OTP**
2. **Backend:** Returns `OTP_INVALID`
3. **Frontend:** Shows error, clears inputs, focuses first
4. **Backend:** Increments failed attempts
5. **After max attempts:** Account locked

### Scenario 4: Expired OTP
1. **User enters OTP after expiration**
2. **Backend:** Returns `OTP_INVALID_OR_EXPIRED`
3. **Frontend:** Shows error, enables resend link
4. **User clicks resend** → New OTP sent

### Scenario 5: Rate Limiting
1. **User requests OTP multiple times**
2. **Backend:** Checks rate limits (per NationalCode, per IP)
3. **If exceeded:** Returns `RATE_LIMIT_EXCEEDED`
4. **Frontend:** Shows error, extends cooldown (30 seconds)

### Scenario 6: Account Locked
1. **User has too many failed attempts**
2. **Backend:** Returns `ACCOUNT_LOCKED`
3. **Frontend:** Shows lockout message with time remaining

### Scenario 7: Session Binding Mismatch
1. **User requests OTP on Device A**
2. **User tries to verify on Device B** (different IP/UserAgent)
3. **Backend:** Returns `SESSION_BINDING_ERROR`
4. **Frontend:** Shows security error, requires new OTP

---

## 🛡️ Security Measures

### Frontend
- ✅ Anti-forgery tokens
- ✅ Input validation (digits only)
- ✅ Rate limiting cooldown (5s normal, 30s on error)
- ✅ Button disable during requests
- ✅ No sensitive data in logs

### Backend
- ✅ Rate limiting (per NationalCode, per IP)
- ✅ OTP expiration (configurable, stored in DB)
- ✅ OTP hashing (with phone number as salt)
- ✅ Session binding (IP + UserAgent)
- ✅ Account lockout after failed attempts
- ✅ Audit logging (`OtpRequest` table)
- ✅ Secure OTP generation (cryptographically secure)

---

## 📊 Database Schema

### OtpRequest Table
- `OtpRequestId` (PK)
- `PhoneNumber`
- `OtpCodeHash` (hashed OTP)
- `RequestTime`
- `IsVerified` (boolean)
- `ExpiryTime` (calculated from RequestTime + OtpExpiryMinutes)

### OtpState (In-Memory Store)
- `NationalCode`
- `PhoneNumber`
- `OtpHash`
- `ExpiryUtc`
- `IpAddress`
- `UserAgent`

---

## 🎨 UI/UX Features

### OTP Inputs
- ✅ Large, readable numbers (20px+ on mobile)
- ✅ High contrast borders
- ✅ Focus indicators
- ✅ Auto-focus next on input
- ✅ Arrow key navigation
- ✅ Paste support (6 digits)
- ✅ Backspace navigation
- ✅ Clear visual feedback
- ✅ Error states
- ✅ Disabled states

### Responsive Design
- ✅ Mobile-first approach
- ✅ Breakpoints: 320px, 375px, 768px, 992px, 1200px
- ✅ Touch-friendly (44px minimum)
- ✅ No zoom on iOS (font-size: 16px+)
- ✅ Flexible gap spacing

### Accessibility
- ✅ ARIA labels
- ✅ Keyboard navigation
- ✅ Focus management
- ✅ Screen reader support
- ✅ High contrast mode support

---

## 🧪 Test Scenarios

### Unit Tests
- [ ] OTPManager input validation
- [ ] OTPManager paste handling
- [ ] OTPManager navigation
- [ ] Form submission handlers
- [ ] Error handling

### Integration Tests
- [ ] Complete login flow
- [ ] Complete registration flow
- [ ] OTP expiration
- [ ] Rate limiting
- [ ] Account lockout
- [ ] Session binding

### Manual Tests
- [ ] Mobile devices (iOS, Android)
- [ ] Tablets
- [ ] Desktop browsers
- [ ] Different screen sizes
- [ ] Slow network conditions
- [ ] Paste from clipboard
- [ ] Keyboard navigation
- [ ] Error scenarios

---

## 🔧 Configuration

### AuthSettings
- `OtpLength`: 6
- `OtpExpiryMinutes`: 5 (stored in DB)
- `OtpMaxSendsPerNationalCodePer5Min`: 3
- `OtpMaxSendsPerIpPer5Min`: 10
- `OtpLockoutMinutes`: 15
- `OtpFailedMaxAttempts`: 5

---

## 📝 Notes

1. **OTP Expiration:** Stored in `OtpState.ExpiryUtc` and verified on each check
2. **Rate Limiting:** Applied at service layer, logged for audit
3. **Session Binding:** IP + UserAgent must match for security
4. **Account Lockout:** Standard ASP.NET Identity feature
5. **Audit Trail:** All OTP requests logged to `OtpRequest` table

---

**Last Updated:** 2025-01-XX
**Status:** Production-Ready ✅

