# 🔄 OTP Login + Signup Flow Diagram

> **هدف:** نمایش کامل Flow لاگین و ثبت‌نام با OTP  
> **پروژه:** ClinicApp  
> **تاریخ:** 2025-01-27

---

## 📊 Flow Diagram - Login

```
┌─────────────────────────────────────────────────────────────────┐
│                    USER ENTERS NATIONAL CODE                    │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 1: Validate National Code Format                         │
│  - PersianNumberHelper.ToEnglishNumbers()                       │
│  - IranianNationalCodeValidator.IsValid()                      │
└────────────────────────────┬────────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │                 │
              Valid │                 │ Invalid
                    │                 │
                    ▼                 ▼
┌───────────────────────────┐  ┌──────────────────────────┐
│  Step 2: Check User      │  │  Return Error:            │
│  Exists in DB            │  │  "کد ملی معتبر نیست"      │
│  FindByNameAsync()       │  └──────────────────────────┘
└────────────┬──────────────┘
             │
    ┌────────┴────────┐
    │                 │
Exists │                 │ Not Exists
    │                 │
    ▼                 ▼
┌──────────────┐  ┌──────────────────────────────┐
│ LOGIN FLOW   │  │  Return: USER_IS_NEW         │
│              │  │  → Redirect to Signup Flow   │
└──────┬───────┘  └──────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 3: Security Checks                                        │
│  - User.IsActive?                                               │
│  - Account Locked?                                              │
│  - Rate Limiting (3 per 5 min per NC, 10 per 5 min per IP)     │
└────────────────────────────┬────────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │                 │
              Pass │                 │ Fail
                    │                 │
                    ▼                 ▼
┌───────────────────────────┐  ┌──────────────────────────┐
│  Step 4: Generate OTP    │  │  Return Error:         │
│  - GenerateSecureOtp(6)   │  │  Rate Limit / Locked   │
│  - HashOtp(otp, phone)    │  └────────────────────────┘
└────────────┬──────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 5: Store OTP State                                       │
│  - Session: OtpStateStore.SetState()                           │
│  - Database: OtpStateEntity (fallback)                         │
│  - OtpRequest (audit log)                                      │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 6: Send SMS                                               │
│  - AsanakSmsService.SendAsync()                                │
│  - Message: "کد ورود کلینیک شفا: {otp}"                        │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│  USER RECEIVES OTP ON PHONE                                     │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│  USER ENTERS OTP (6 digits)                                     │
│  - Separate inputs (auto-focus, auto-submit)                    │
│  - Paste support                                                │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 7: Verify OTP                                            │
│  - Get OTP State (Session → Database fallback)                │
│  - ValidateOtpState():                                         │
│    • State exists?                                             │
│    • Expired?                                                  │
│    • NationalCode match?                                       │
│    • PhoneNumber match?                                        │
│    • AttemptCount < Max?                                       │
│    • Hash match? (SlowEquals)                                  │
└────────────┬────────────────────────────────────────────────────┘
             │
    ┌────────┴────────┐
    │                 │
Valid │                 │ Invalid
    │                 │
    ▼                 ▼
┌──────────────┐  ┌──────────────────────────────┐
│  Step 8:     │  │  Increment AttemptCount     │
│  Sign In     │  │  If AttemptCount >= Max:    │
│  - SignInAsync()│  │    Lock Account           │
│  - Create    │  │  Return Error              │
│    Session   │  └──────────────────────────────┘
└──────┬───────┘
       │
       ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 9: Post-Login                                            │
│  - Clear OTP State                                             │
│  - Log Login History                                           │
│  - Redirect to Home/Dashboard                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 Flow Diagram - Signup

```
┌─────────────────────────────────────────────────────────────────┐
│                    USER ENTERS NATIONAL CODE                    │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 1: Validate National Code                                │
│  - PersianNumberHelper.ToEnglishNumbers()                      │
│  - IranianNationalCodeValidator.IsValid()                      │
└────────────────────────────┬────────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │                 │
              Valid │                 │ Invalid
                    │                 │
                    ▼                 ▼
┌───────────────────────────┐  ┌──────────────────────────┐
│  Step 2: Check User      │  │  Return Error:           │
│  Exists in DB            │  │  "کد ملی معتبر نیست"      │
│  FindByNameAsync()       │  └──────────────────────────┘
└────────────┬──────────────┘
             │
    ┌────────┴────────┐
    │                 │
Exists │                 │ Not Exists
    │                 │
    ▼                 ▼
┌──────────────┐  ┌──────────────────────────────┐
│  Return:     │  │  SIGNUP FLOW                 │
│  USER_EXISTS │  │                               │
│  → Login     │  └───────────────┬────────────────┘
└──────────────┘                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 3: User Enters Phone Number                               │
│  - Validate format                                              │
│  - Normalize (PersianNumberHelper)                              │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 4: Security Checks                                       │
│  - Rate Limiting (3 per 5 min per NC, 10 per 5 min per IP)     │
│  - Phone number format validation                               │
└────────────────────────────┬────────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │                 │
              Pass │                 │ Fail
                    │                 │
                    ▼                 ▼
┌───────────────────────────┐  ┌──────────────────────────┐
│  Step 5: Generate OTP     │  │  Return Error:           │
│  - GenerateSecureOtp(6)    │  │  Rate Limit              │
│  - HashOtp(otp, phone)     │  └──────────────────────────┘
└────────────┬──────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 6: Store OTP State                                       │
│  - Session: OtpStateStore.SetState()                           │
│  - Database: OtpRequest (audit log)                            │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 7: Send SMS                                               │
│  - AsanakSmsService.SendAsync()                                │
│  - Message: "کد تایید کلینیک شفا: {otp}"                       │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│  USER RECEIVES OTP ON PHONE                                     │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│  USER ENTERS OTP (6 digits)                                     │
│  - Separate inputs (auto-focus, auto-submit)                    │
│  - Paste support                                                │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 8: Verify OTP                                            │
│  - Get OTP State (Session)                                     │
│  - ValidateOtpState():                                         │
│    • State exists?                                             │
│    • Expired?                                                  │
│    • NationalCode match?                                       │
│    • PhoneNumber match?                                        │
│    • Hash match? (SlowEquals)                                  │
└────────────┬────────────────────────────────────────────────────┘
             │
    ┌────────┴────────┐
    │                 │
Valid │                 │ Invalid
    │                 │
    ▼                 ▼
┌──────────────┐  ┌──────────────────────────────┐
│  Step 9:     │  │  Return Error:               │
│  Create User│  │  "کد وارد شده صحیح نیست"      │
│  - Create   │  └──────────────────────────────┘
│    Account  │
│  - Sign In  │
└──────┬──────┘
       │
       ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 10: Post-Signup                                          │
│  - Clear OTP State                                             │
│  - Redirect to Profile Completion (optional)                    │
│  - Or Redirect to Home/Dashboard                                │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Unified Flow (Auto-Detection)

```
                    ┌─────────────────────┐
                    │  Enter National Code │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │  Validate Format    │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │  CheckUserExists()  │
                    └──────────┬──────────┘
                               │
                  ┌────────────┴────────────┐
                  │                         │
            USER_EXISTS              USER_IS_NEW
                  │                         │
                  ▼                         ▼
         ┌─────────────────┐      ┌─────────────────┐
         │   LOGIN FLOW    │      │   SIGNUP FLOW      │
         │                 │      │                    │
         │  SendLoginOtp() │      │  SendRegOtp()     │
         │  → Verify      │      │  → Verify         │
         │  → SignIn      │      │  → Create User    │
         │  → Redirect    │      │  → SignIn         │
         └─────────────────┘      │  → Redirect       │
                                 └─────────────────┘
```

---

## 🔒 Security Checkpoints

```
┌─────────────────────────────────────────────────────────────┐
│  SECURITY CHECKPOINTS (در هر درخواست OTP)                   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. Input Validation                                        │
│     ✓ National Code Format                                  │
│     ✓ Phone Number Format                                   │
│                                                              │
│  2. Rate Limiting                                           │
│     ✓ Per National Code: 3 / 5 min                         │
│     ✓ Per IP: 10 / 5 min                                   │
│                                                              │
│  3. Account Status                                          │
│     ✓ User Exists (Login)                                   │
│     ✓ User IsActive                                         │
│     ✓ Account Not Locked                                    │
│                                                              │
│  4. OTP Generation                                          │
│     ✓ Cryptographically Secure Random                       │
│     ✓ Hash with Salt (Phone Number)                         │
│                                                              │
│  5. OTP Storage                                             │
│     ✓ Session (Primary)                                     │
│     ✓ Database (Fallback)                                   │
│     ✓ Never Plain Text                                      │
│                                                              │
│  6. OTP Verification                                        │
│     ✓ State Exists                                          │
│     ✓ Not Expired                                           │
│     ✓ NationalCode Match                                    │
│     ✓ PhoneNumber Match                                     │
│     ✓ Hash Match (SlowEquals)                              │
│     ✓ AttemptCount < Max                                    │
│                                                              │
│  7. Post-Verification                                       │
│     ✓ Clear OTP State                                       │
│     ✓ Log Audit Trail                                       │
│     ✓ Create Secure Session                                 │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 📱 UI Flow States

```
State 1: National Code Input
  └─> [User enters code] → Validate → CheckUserExists()
      │
      ├─> USER_EXISTS → State 2 (Login OTP)
      └─> USER_IS_NEW → State 2 (Signup OTP)

State 2: OTP Input
  └─> [6 separate inputs]
      │
      ├─> Auto-focus next
      ├─> Paste support
      ├─> Auto-submit when complete
      └─> Countdown timer (TODO)

State 3: Verification
  └─> [Loading...]
      │
      ├─> Success → State 4 (Authenticated)
      └─> Error → State 2 (Show error, allow retry)

State 4: Authenticated
  └─> Redirect to Dashboard/Home
```

---

## 🚨 Error Handling Flow

```
┌─────────────────────────────────────────────────────────────┐
│  ERROR SCENARIOS & HANDLING                                  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. Invalid National Code                                   │
│     → Message: "کد ملی وارد شده معتبر نیست"                  │
│     → Code: INVALID_NATIONAL_CODE                           │
│     → Action: Stay on form, highlight input                  │
│                                                              │
│  2. User Not Found (Login)                                  │
│     → Message: "کاربری با این کد ملی یافت نشد"                │
│     → Code: USER_NOT_FOUND                                  │
│     → Action: Suggest Signup                                 │
│                                                              │
│  3. Account Locked                                          │
│     → Message: "حساب کاربری شما قفل شده است"                 │
│     → Code: ACCOUNT_LOCKED                                  │
│     → Action: Show lockout duration                         │
│                                                              │
│  4. Rate Limit Exceeded                                     │
│     → Message: "تعداد درخواست‌ها بیش از حد مجاز است"          │
│     → Code: RATE_LIMIT_EXCEEDED                             │
│     → Action: Disable button, show countdown                │
│                                                              │
│  5. OTP Expired                                             │
│     → Message: "کد تایید منقضی شده است"                      │
│     → Code: OTP_EXPIRED                                     │
│     → Action: Allow resend                                   │
│                                                              │
│  6. Invalid OTP                                            │
│     → Message: "کد وارد شده صحیح نمی‌باشد"                   │
│     → Code: OTP_INVALID                                     │
│     → Action: Clear inputs, allow retry                     │
│                                                              │
│  7. OTP State Not Found                                    │
│     → Message: "کد تایید یافت نشد"                          │
│     → Code: OTP_STATE_NOT_FOUND                             │
│     → Action: Request new OTP                               │
│                                                              │
│  8. System Error                                           │
│     → Message: "خطای سیستمی رخ داده است"                     │
│     → Code: SYSTEM_ERROR                                    │
│     → Action: Log error, show support contact               │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 📝 Notes

1. **Session Fallback:** اگر Session از دست برود، سیستم از Database (`OtpStateEntity`) OTP State را بازیابی می‌کند.

2. **Concurrent OTP:** قبل از ارسال OTP جدید، OTP قبلی برای همان کد ملی باطل می‌شود.

3. **Audit Trail:** تمام درخواست‌های OTP در `OtpRequest` table ثبت می‌شوند.

4. **Medical Compliance:** پیامک OTP فقط شامل کد است، بدون اطلاعات پزشکی.

---

**تاریخ:** 2025-01-27  
**نسخه:** 1.0

