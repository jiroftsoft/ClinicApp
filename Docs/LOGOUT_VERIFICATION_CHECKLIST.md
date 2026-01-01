# ✅ Logout Verification Checklist

**تاریخ:** 2025-01-27  
**ماژول:** Logout / SignOut / Authentication Clear  
**وضعیت:** نیاز به بررسی

---

## بررسی Logout Flow

### 1. Logout Action (Backend)
✅ **File:** `Controllers/AccountController.cs:516-538`
- `[HttpPost]` - فقط POST requests
- `[Authorize]` - فقط کاربران authenticated
- `[ValidateAntiForgeryToken]` - CSRF protection
- `_authService.SignOut()` - پاک کردن authentication
- `RedirectToAction("Index", "Home")` - redirect به Home

### 2. SignOut Service
✅ **File:** `Services/AuthService.cs:298-302`
- `_authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie)`
- این باید cookie را پاک کند

### 3. Logout Form (Frontend)
✅ **File:** `Views/Shared/_LoginPartial.cshtml:111-113`
- Form با `id="logoutForm"`
- POST به `Account/LogOff`
- `@Html.AntiForgeryToken()` - CSRF token

---

## مشکلات احتمالی

### ⚠️ Issue #1: Redirect بعد از Logout ممکن است UI را به‌روز نکند

**مشکل:**
- بعد از `SignOut()`، redirect به Home می‌شود
- اما اگر `Application_PostAuthenticateRequest` هنوز cookie را check کند، ممکن است sync اتفاق بیفتد
- یا اگر cookie هنوز در browser باشد (expire نشده)، ممکن است دوباره authenticate شود

**راه‌حل:**
- OWIN `SignOut()` باید cookie را expire کند
- باید cookie را expire کند یا حذف کند

### ⚠️ Issue #2: Application_PostAuthenticateRequest ممکن است بعد از SignOut sync کند

**مشکل:**
- `Application_PostAuthenticateRequest` در `Global.asax.cs` check می‌کند که آیا cookie وجود دارد
- اگر cookie هنوز در request باشد (قبل از expire)، ممکن است دوباره sync کند

**راه‌حل:**
- باید مطمئن شویم که `SignOut()` cookie را به درستی expire می‌کند
- یا باید check کنیم که آیا user در حال logout است

---

## تست‌های مورد نیاز

### Test #1: Logout و بررسی Cookie
1. Login کنید
2. در DevTools → Application → Cookies → بررسی کنید `ClinicAppAuth` وجود دارد
3. روی دکمه "خروج" کلیک کنید
4. بررسی کنید:
   - Cookie `ClinicAppAuth` حذف شده یا expire شده
   - Redirect به Home انجام شده
   - UI به حالت "ورود / ثبت‌نام" برگشته

### Test #2: Logout و بررسی Network
1. Login کنید
2. در DevTools → Network tab → Preserve log
3. روی دکمه "خروج" کلیک کنید
4. بررسی کنید:
   - Request به `/Account/LogOff` با POST
   - Response با redirect (302) به `/Home/Index`
   - در redirect request، cookie `ClinicAppAuth` ارسال نشده

### Test #3: Logout و بررسی UI
1. Login کنید
2. بررسی کنید منوی کاربر نمایش داده می‌شود
3. روی "خروج" کلیک کنید
4. بررسی کنید:
   - منوی کاربر مخفی شده
   - دکمه "ورود / ثبت‌نام" نمایش داده می‌شود
   - صفحه Home به درستی نمایش داده می‌شود

---

## راه‌حل‌های پیشنهادی

### Solution #1: اطمینان از Expire Cookie در SignOut

**File:** `Services/AuthService.cs`

```csharp
public void SignOut()
{
    // ✅ Clear authentication
    _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
    
    // ✅ Explicitly expire cookie (additional safety)
    var cookie = HttpContext.Current.Request.Cookies["ClinicAppAuth"];
    if (cookie != null)
    {
        cookie.Expires = DateTime.Now.AddDays(-1);
        cookie.Value = string.Empty;
        HttpContext.Current.Response.Cookies.Add(cookie);
    }
    
    _log.Information("کاربر از سیستم خارج شد.");
}
```

### Solution #2: بهبود Application_PostAuthenticateRequest برای Logout

**File:** `Global.asax.cs`

```csharp
protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
{
    // ✅ Skip sync if we're in the process of logging out
    if (Request.Path.Contains("/Account/LogOff", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }
    
    // ... existing sync code ...
}
```

---

## نتیجه‌گیری

✅ **Logout functionality وجود دارد و به نظر درست است:**
- Backend: `AccountController.LogOff()` ✅
- Service: `AuthService.SignOut()` ✅
- Frontend: Form در `_LoginPartial.cshtml` ✅

⚠️ **اما نیاز به تست دارد:**
- آیا cookie به درستی expire می‌شود؟
- آیا UI بعد از logout به درستی به‌روز می‌شود؟
- آیا `Application_PostAuthenticateRequest` بعد از logout sync نمی‌کند؟

---

## توصیه

**قبل از تست:**
1. بررسی کنید که `SignOut()` cookie را expire می‌کند
2. تست کنید که بعد از logout، UI به درستی تغییر می‌کند

**اگر مشکل داشت:**
- Solution #1 را اعمال کنید (explicit cookie expire)
- Solution #2 را اعمال کنید (skip sync در LogOff)

