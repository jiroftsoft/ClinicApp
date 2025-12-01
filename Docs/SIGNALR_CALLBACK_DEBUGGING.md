# راهنمای Debug برای مشکل عدم دریافت پیام از POS

**تاریخ:** 1404-09-10  
**مشکل:** پیام لغو از POS به برنامه نمی‌رسد

---

## 🔍 مراحل بررسی

### 1. بررسی Log های Backend

```powershell
# بررسی آخرین Log ها
Get-Content "App_Data\Logs\clinicapp-*.log" -Tail 200 | Select-String -Pattern "GetTransactionResponse|CALLBACK INVOKED|SendAmount1Step|Connection|Hub" -Context 3
```

**چیزهایی که باید بررسی شوند:**

1. ✅ آیا Callback ها ثبت شده‌اند؟
   - باید این Log را ببینید: `✅ SamanKish SignalR: GetTransactionResponse callback registered successfully`

2. ✅ آیا `SendAmount1Step` فراخوانی شده است؟
   - باید این Log را ببینید: `✅ SamanKish SignalR: SendAmount1Step invoked successfully`

3. ✅ آیا `GetTransactionResponse CALLBACK INVOKED` در Log دیده می‌شود؟
   - اگر این Log را نمی‌بینید، یعنی Response از Hub نمی‌آید

### 2. بررسی Windows Service

```powershell
# بررسی وضعیت Service
Get-Service -Name "SSP1126Service1" | Select-Object Name, Status

# بررسی Log های Service
Get-Content "C:\Log\*.log" -Tail 100 | Select-String -Pattern "GetTransactionResponse|98|لغو" -Context 2
```

**مهم:** Service باید در حال اجرا باشد و Response را به Hub ارسال کند.

### 3. بررسی Connection State

در Log باید این موارد را ببینید:
- `Connection State: Connected`
- `HubProxy: Valid`
- `HubName: SSP1126HUB`

### 4. بررسی Hub Name

Hub Name باید دقیقاً `SSP1126HUB` باشد (مطابق Sample HTML).

### 5. تست دستی با Sample HTML

1. فایل `Sample(SSP1126)Page.html` را باز کنید
2. Connection را برقرار کنید
3. `SendAmount1Step` را فراخوانی کنید
4. دکمه لغو را روی دستگاه بزنید
5. بررسی کنید که Response در HTML نمایش داده می‌شود

اگر در Sample HTML کار می‌کند، مشکل از کد C# است.

---

## 🔧 راه‌حل‌های احتمالی

### راه‌حل 1: بررسی Hub Name

مطمئن شوید که Hub Name درست است:
```csharp
private const string HubName = "SSP1126HUB";
```

### راه‌حل 2: بررسی Method Name

مطمئن شوید که Method Name درست است:
```csharp
_hubProxy.On<IList<object>>("GetTransactionResponse", ...);
```

### راه‌حل 3: بررسی Connection State

مطمئن شوید که Connection در تمام مدت Connected است:
```csharp
if (_hubConnection.State != ConnectionState.Connected)
{
    // Reconnect
}
```

### راه‌حل 4: بررسی Callback Registration

مطمئن شوید که Callback ها بعد از Connection ثبت می‌شوند:
```csharp
// ✅ درست: بعد از Start
_hubConnection.Start(...);
if (State == Connected) {
    RegisterClientCallbacks();
}
```

---

## 📋 چک‌لیست Debug

- [ ] Service در حال اجرا است
- [ ] Connection State = Connected
- [ ] HubProxy null نیست
- [ ] Callback ها ثبت شده‌اند (Log نشان می‌دهد)
- [ ] `SendAmount1Step` فراخوانی شده است
- [ ] `GetTransactionResponse CALLBACK INVOKED` در Log دیده می‌شود
- [ ] Response از POS می‌آید (Log Service نشان می‌دهد)

---

## 🎯 مراحل بعدی

1. Log های Backend را بررسی کنید
2. Log های Service را بررسی کنید
3. اگر Callback فراخوانی نمی‌شود، مشکل از Hub است
4. اگر Callback فراخوانی می‌شود اما Response null است، مشکل از Parse است

---

**لطفاً Log های Backend و Service را ارسال کنید تا مشکل را دقیق‌تر بررسی کنیم.**

