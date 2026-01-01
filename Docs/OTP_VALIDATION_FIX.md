# 🔧 Fix: OTP Validation در Full Page POST

**تاریخ:** 2025-01-27  
**مشکل:** OTP در Full Page POST به درستی ارسال نمی‌شود

---

## 🎯 مشکل

وقتی form به صورت Full Page POST submit می‌شود:
- OTP از inputs خوانده می‌شود
- اما ممکن است در `combined-otp-code` set نشود قبل از submit
- Server OTP خالی دریافت می‌کند
- Validation fail می‌شود
- Redirect به `/Account/Login` انجام می‌شود

---

## ✅ Fix اعمال شده

### تغییر در `Views/Account/_LoginModal.cshtml`

**قبل:**
```javascript
if (!state.isRegistrationFlow) {
    // ... returnUrl handling ...
    return true; // Allow form submission
}
```

**بعد:**
```javascript
if (!state.isRegistrationFlow) {
    // ✅ CRITICAL: Ensure OTP is set in combined field before submit
    var otpValue = $('#combined-otp-code').val();
    if (!otpValue || otpValue.length !== config.otpLength) {
        // Rebuild from inputs
        otpValue = $('#otp-inputs .otp-input').map(function() {
            return $(this).val();
        }).get().join('').replace(/\D/g, '');
        
        if (!otpValue || otpValue.length !== config.otpLength) {
            e.preventDefault();
            $('#otp-error').text('لطفاً کد تایید ۶ رقمی را کامل وارد کنید');
            return false;
        }
        
        // Update combined field
        $('#combined-otp-code').val(otpValue);
    }
    
    // ... rest of code ...
    return true; // Allow form submission
}
```

---

## 🔍 چرا این Fix کار می‌کند

1. **قبل از submit:** OTP از inputs خوانده می‌شود
2. **Validation:** بررسی می‌شود که OTP 6 رقم است
3. **Set in form:** OTP در `combined-otp-code` field set می‌شود
4. **Submit:** Form با OTP submit می‌شود
5. **Server:** OTP را دریافت می‌کند و validate می‌کند ✅

---

## 🧪 تست

1. Login کنید
2. OTP را وارد کنید
3. Submit کنید
4. بررسی کنید:
   - OTP به درستی ارسال می‌شود ✅
   - Server OTP را validate می‌کند ✅
   - Login موفق می‌شود ✅
   - Redirect به Home انجام می‌شود ✅

---

**آماده برای تست!**

