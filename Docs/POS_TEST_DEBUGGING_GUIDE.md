# راهنمای عیب‌یابی تست اتصال POS
**تاریخ:** 1402-09-10  
**کد درخواست:** SSP1126(WEB)

---

## 🔍 بررسی سیستماتیک فرایند تست اتصال

### گام 1: بررسی Frontend (JavaScript)

**فایل:** `Views/PosTest/Index.cshtml`

**بررسی:**
1. آیا `terminalId` از `select` به درستی خوانده می‌شود؟
2. آیا `__RequestVerificationToken` به درستی ارسال می‌شود؟
3. آیا AJAX request به درستی ارسال می‌شود؟

**Log‌های مورد انتظار:**
- در Console Browser: بررسی کنید که آیا request ارسال می‌شود
- در Network Tab: بررسی کنید که آیا request با Status 200 یا 400 برمی‌گردد

---

### گام 2: بررسی AntiForgery Token Validation

**فایل:** `Filters/ValidateAntiForgeryTokenOnPostsAttribute.cs`

**Log‌های مورد انتظار:**
```
🔒 AntiForgery: OnAuthorization شروع شد - Path: /PosTest/TestConnection, Method: POST
🔒 AntiForgery: در حال بررسی Token برای POST - Path: /PosTest/TestConnection
```

**اگر Token نامعتبر باشد:**
```
❌ AntiForgery: Token validation failed - Path: /PosTest/TestConnection
```

**نکته:** اگر AntiForgery Token validation fail شود، Action اصلاً فراخوانی نمی‌شود و Status Code 400 برمی‌گردد.

---

### گام 3: بررسی OnActionExecuting

**فایل:** `Controllers/Payment/POS/PosTestController.cs`

**Log‌های مورد انتظار:**
```
🔍🔍🔍 POS Test: ========== OnActionExecuting شروع شد ==========
🔍🔍🔍 POS Test: Action: TestConnection, Controller: PosTest, Method: POST
🔍🔍🔍 POS Test: تعداد پارامترها: 2
🔍🔍🔍 POS Test: Parameter - terminalId: 1 (Type: Int32)
🔍🔍🔍 POS Test: Parameter - testAmount: null (Type: Nullable`1)
🔍🔍🔍 POS Test: Request Details - ContentType: application/x-www-form-urlencoded, IsAjax: True
🔍🔍🔍 POS Test: ========== OnActionExecuting پایان یافت ==========
```

**نکته:** اگر این Log‌ها نمایش داده نشوند، یعنی Action Filter دیگری Action را متوقف کرده است.

---

### گام 4: بررسی فراخوانی متد TestConnection

**Log‌های مورد انتظار:**
```
🔍🔍🔍 POS Test: ========== متد TestConnection فراخوانی شد ==========
🔍🔍🔍 POS Test: TerminalId: 1, TestAmount: null, User: Unknown
🔍🔍🔍 POS Test: WARNING LEVEL - متد TestConnection فراخوانی شد
🔍🔍🔍 POS Test: ERROR LEVEL - متد TestConnection فراخوانی شد
🔍🔍🔍 POS Test: ========== Request Details ==========
🔍🔍🔍 POS Test: Method: POST, ContentType: application/x-www-form-urlencoded
```

**نکته:** اگر این Log‌ها نمایش داده نشوند، یعنی متد اصلاً فراخوانی نمی‌شود.

---

### گام 5: بررسی Validation terminalId

**Log‌های مورد انتظار:**
```
🔍🔍🔍 POS Test: ========== Validation terminalId ==========
```

**اگر terminalId نامعتبر باشد:**
```
⚠️ POS Test: terminalId نامعتبر است - TerminalId: null
```

---

### گام 6: بررسی دریافت ترمینال از دیتابیس

**Log‌های مورد انتظار:**
```
🔍 POS Test: شروع تست اتصال - TerminalId: 1, User: Unknown
🔍 POS Test: در حال دریافت ترمینال از دیتابیس - TerminalId: 1
🔍 POS Test: نتیجه دریافت ترمینال - Success: True, HasData: True
🔍 POS Test: ترمینال دریافت شد - TerminalId: 41678252, IP: 192.168.1.104, Protocol: SignalR, Provider: SamanKish
```

---

### گام 7: بررسی Protocol Validation

**Log‌های مورد انتظار:**
```
🔍 POS Test: بررسی Protocol - Protocol: SignalR
```

**اگر Protocol نامعتبر باشد:**
```
⚠️ POS Test: ترمینال با Protocol = Tcp تنظیم شده است. برای استفاده از SignalR، Protocol باید = SignalR (4) باشد.
```

---

### گام 8: بررسی Driver Selection

**Log‌های مورد انتظار:**
```
🔍 POS Test: تلاش برای اتصال - TerminalId: 41678252, IP: 192.168.1.104, Protocol: SignalR
🏥 POS Test: استفاده از Driver - Provider: SamanKish, Protocol: SignalR
```

**اگر Driver یافت نشود:**
```
⚠️ POS Test: درایور یافت نشد - Provider: SamanKish
```

---

### گام 9: بررسی SignalR Connection

**Log‌های مورد انتظار:**
```
🔍 POS Test: فراخوانی driver.ConnectAsync - TerminalId: 41678252
🏥 SamanKish SignalR: Starting connection - TerminalId: 41678252, IP: 192.168.1.104
🏥 SamanKish SignalR: Connecting to Hub - http://localhost:8080/signalr
✅ SamanKish SignalR: Connected to Hub successfully
✅ SamanKish SignalR: Connection ready - TerminalId: 41678252
🔍 POS Test: نتیجه ConnectAsync - Success: True, Message: (empty)
```

**اگر Connection ناموفق باشد:**
```
❌ SamanKish SignalR: Connection timeout
یا
❌ SamanKish SignalR: Connection failed - State: Disconnected
```

---

## 🔧 مراحل عیب‌یابی

### مرحله 1: بررسی Log‌های Application

```powershell
# بررسی آخرین Log‌های مربوط به POS Test
Get-ChildItem "App_Data\Logs" -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 200 | Select-String -Pattern "POS Test|AntiForgery|TestConnection" -Context 3
```

### مرحله 2: بررسی Log‌های AntiForgery

```powershell
# بررسی Log‌های AntiForgery
Get-ChildItem "App_Data\Logs" -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content | Select-String -Pattern "AntiForgery" | Select-Object -Last 20
```

### مرحله 3: بررسی Log‌های OnActionExecuting

```powershell
# بررسی Log‌های OnActionExecuting
Get-ChildItem "App_Data\Logs" -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content | Select-String -Pattern "OnActionExecuting|🔍🔍🔍" | Select-Object -Last 30
```

### مرحله 4: بررسی Browser Console

1. باز کردن Developer Tools (F12)
2. رفتن به تب Console
3. بررسی خطاهای JavaScript
4. رفتن به تب Network
5. بررسی Request به `/PosTest/TestConnection`
6. بررسی Response Status Code و Body

---

## 🚨 مشکلات رایج و راه‌حل‌ها

### مشکل 1: متد TestConnection فراخوانی نمی‌شود

**علل احتمالی:**
1. AntiForgery Token validation fail می‌شود
2. Action Filter دیگری Action را متوقف می‌کند
3. Routing مشکل دارد

**راه‌حل:**
- بررسی Log‌های AntiForgery
- بررسی Log‌های OnActionExecuting
- بررسی Response Status Code (باید 200 باشد، نه 400)

---

### مشکل 2: terminalId null است

**علل احتمالی:**
1. Model Binding مشکل دارد
2. Frontend به درستی terminalId را ارسال نمی‌کند

**راه‌حل:**
- بررسی Log‌های OnActionExecuting (Parameter terminalId)
- بررسی Browser Network Tab (Request Body)
- بررسی JavaScript در Frontend

---

### مشکل 3: Protocol نامعتبر است

**علل احتمالی:**
1. Protocol در دیتابیس به درستی تنظیم نشده است
2. Protocol باید = 4 (SignalR) باشد

**راه‌حل:**
```sql
UPDATE PosTerminal SET Protocol = 4 WHERE PosTerminalId = 1;
```

---

### مشکل 4: SignalR Connection ناموفق است

**علل احتمالی:**
1. Windows Service در حال اجرا نیست
2. URL در Web.config صحیح نیست
3. Port 8080 باز نیست

**راه‌حل:**
- بررسی Windows Service: `Get-Service -Name "SSP1126Service1"`
- بررسی URL در Web.config: `SamanKishSignalRUrl`
- بررسی Port: `netstat -an | findstr 8080`

---

## 📋 چک‌لیست عیب‌یابی

- [ ] Log‌های AntiForgery بررسی شده است
- [ ] Log‌های OnActionExecuting بررسی شده است
- [ ] Log‌های TestConnection بررسی شده است
- [ ] Browser Console بررسی شده است
- [ ] Browser Network Tab بررسی شده است
- [ ] Windows Service در حال اجرا است
- [ ] URL در Web.config صحیح است
- [ ] Protocol در دیتابیس = 4 (SignalR) است
- [ ] terminalId در Frontend به درستی ارسال می‌شود

---

## 📞 در صورت نیاز به کمک بیشتر

لطفاً Log‌های زیر را ارسال کنید:
1. Log‌های AntiForgery (آخرین 20 خط)
2. Log‌های OnActionExecuting (آخرین 30 خط)
3. Log‌های TestConnection (آخرین 50 خط)
4. Response از Browser Network Tab
5. Console Errors از Browser

