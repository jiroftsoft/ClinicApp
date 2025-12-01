# مرجع نام Service SSP1126
**تاریخ:** 1402-06-29  
**کد درخواست:** SSP1126(WEB)

---

## ⚠️ نکته مهم: نام Service

### نام واقعی Service

**نام Service:** `SSP1126Service1`  
**Display Name:** `SSP1126WindowsService`

**❌ نام اشتباه:** `SSP1126SignalRWindowsService` (این نام فایل EXE است، نه نام Service)

---

## 🔍 بررسی و مدیریت Service

### 1. بررسی وضعیت Service

```powershell
# بررسی وضعیت
Get-Service -Name "SSP1126Service1"

# یا با جزئیات بیشتر
Get-Service -Name "SSP1126Service1" | Select-Object Name, DisplayName, Status, StartType
```

### 2. شروع Service

```powershell
Start-Service -Name "SSP1126Service1"
```

### 3. توقف Service

```powershell
Stop-Service -Name "SSP1126Service1"
```

### 4. Restart Service

```powershell
Restart-Service -Name "SSP1126Service1"
```

### 5. بررسی Log های Service

```powershell
# مسیر Log: C:\Log\ (طبق تنظیمات)
Get-ChildItem "C:\Log" | Select-Object Name, LastWriteTime, Length | Sort-Object LastWriteTime -Descending
```

---

## 📋 دستورات مفید

### بررسی تمام Service های مرتبط

```powershell
# جستجوی Service با نام SSP1126
Get-Service | Where-Object {$_.Name -like "*SSP1126*" -or $_.DisplayName -like "*SSP1126*"}
```

### بررسی Port 8080

```powershell
# بررسی اینکه Port 8080 در حال استفاده است
netstat -ano | findstr :8080

# یا
Test-NetConnection -ComputerName localhost -Port 8080
```

### بررسی Process Service

```powershell
# بررسی Process مربوط به Service
Get-Process | Where-Object {$_.ProcessName -like "*SSP1126*"}
```

---

## 🔧 عیب‌یابی

### مشکل: Service پیدا نمی‌شود

**راه‌حل:**
1. بررسی نام Service:
   ```powershell
   Get-Service | Where-Object {$_.DisplayName -like "*SSP1126*"}
   ```

2. اگر Service نصب نشده است:
   - نصب Service با `InstallUtil.exe`
   - یا استفاده از Installer

### مشکل: Service متوقف می‌شود

**راه‌حل:**
1. بررسی Log ها:
   ```powershell
   Get-Content "C:\Log\*.log" -Tail 50
   ```

2. بررسی Event Viewer:
   ```powershell
   Get-EventLog -LogName Application -Source "SSP1126Service1" -Newest 10
   ```

---

## 📝 خلاصه

| مورد | مقدار |
|------|-------|
| **نام Service** | `SSP1126Service1` |
| **Display Name** | `SSP1126WindowsService` |
| **نام فایل EXE** | `SSP1126SignalRWindowsService.exe` |
| **نام فایل Config** | `SSP1126SignalRWindowsService.exe.config` |
| **Port** | `8080` |
| **Log Path** | `C:\Log\` |

---

**تاریخ:** 1402-06-29  
**وضعیت:** ✅ به‌روزرسانی شده

