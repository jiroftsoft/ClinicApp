# 🔧 راهنمای Fix کردن Doctor.Degree Column

## 📋 مشکل

```
System.InvalidOperationException: 
The 'Degree' property on 'Doctor' could not be set to a 'System.String' value.
```

**علت:**
- در Database: ستون `Degree` به عنوان `String` یا `NULL` ذخیره شده
- در Entity Framework: انتظار `tinyint` (integer: 1-5) است

---

## ✅ راه حل (3 روش)

### 🚀 **روش 1: PowerShell Script** ⭐ **سریع و آسان**

```powershell
# در PowerShell:
cd "C:\Users\Developer\source\repos\ClinicApp\Scripts\SQL"
.\Run-Fix-Doctor-Degree.ps1
```

این script:
- ✅ خودکار Database را Backup می‌گیرد
- ✅ مقادیر String را به Integer تبدیل می‌کند
- ✅ نتیجه را نمایش می‌دهد

---

### 💻 **روش 2: SQL Server Management Studio (SSMS)**

1. **باز کردن SSMS:**
   ```
   Server: .
   Database: ClinicDb
   Authentication: Windows Authentication
   ```

2. **اجرای Script:**
   - Open File: `Fix-Doctor-Degree-Column.sql`
   - اطمینان از انتخاب Database: `ClinicDb`
   - کلیک Execute (F5)

---

### ⌨️ **روش 3: Command Line**

```bash
# در PowerShell یا CMD:
sqlcmd -S . -d ClinicDb -E -i "Fix-Doctor-Degree-Column.sql"
```

---

## 🔍 بررسی نتیجه

بعد از اجرای Script، این script را برای Verification اجرا کنید:

```powershell
sqlcmd -S . -d ClinicDb -E -i "Verify-Doctor-Degree-Fix.sql"
```

باید این پیام‌ها را ببینید:
```
✅ همه مقادیر صحیح
✅ نوع ستون: tinyint
✅ همه Status ها ✅
```

---

## 📊 Enum Values

| Value | Enum | فارسی |
|-------|------|-------|
| 1 | GeneralPhysician | پزشک عمومی |
| 2 | Specialist | متخصص |
| 3 | SubSpecialist | فوق تخصص |
| 4 | Dentist | دندانپزشک |
| 5 | Pharmacist | داروساز |
| NULL | - | (خالی - معتبر) |

---

## 🎯 مراحل بعد از Fix

1. **Restart Application:**
   ```powershell
   # Stop IIS Express / Kestrel
   # Rebuild Solution
   dotnet build
   # Start Application
   ```

2. **تست:**
   - Admin Panel → Doctors → Index
   - چک کنید لیست بدون خطا load می‌شود
   - چک کنید Degree صحیح نمایش داده می‌شود

3. **Reception Edit:**
   - Reception → Edit (مثلاً /Reception/Edit/1083)
   - چک کنید بیمه تکمیلی نمایش داده می‌شود
   - چک کنید همه اطلاعات کامل است

---

## ⚠️ در صورت بروز مشکل

### مشکل 1: "sqlcmd is not recognized"

**راه حل:**
```powershell
# نصب SQL Server Command Line Tools
# از Microsoft Download Center دانلود کنید:
# https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility
```

### مشکل 2: "Cannot open database ClinicDb"

**راه حل:**
```sql
-- چک کردن Database ها:
sqlcmd -S . -E -Q "SELECT name FROM sys.databases"

-- اگر ClinicDb نیست، نام صحیح را از Web.config بخوانید:
-- <connectionStrings> → Initial Catalog=...
```

### مشکل 3: "Login failed for user"

**راه حل:**
1. SQL Server Service را چک کنید (باید Running باشد)
2. Windows Authentication را چک کنید
3. User Permissions را بررسی کنید

### مشکل 4: همچنان خطا دارم

**بررسی:**
```sql
-- این Query را اجرا کنید و نتیجه را ارسال کنید:
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Doctors')
    AND c.name = 'Degree';

-- و این:
SELECT TOP 5
    DoctorId,
    FirstName,
    LastName,
    Degree,
    CAST(Degree AS VARBINARY(MAX)) AS DegreeHex
FROM Doctors;
```

---

## 📁 فایل‌های Script

| فایل | توضیح |
|------|-------|
| `Fix-Doctor-Degree-Column.sql` | Script اصلی برای Fix |
| `Verify-Doctor-Degree-Fix.sql` | Verification و بررسی نتیجه |
| `Run-Fix-Doctor-Degree.ps1` | PowerShell helper برای اجرای آسان |
| `README-Doctor-Degree-Fix.md` | این فایل |

---

## 🛡️ Backup

Script به طور خودکار Backup می‌گیرد:
```sql
-- جدول Backup:
SELECT * FROM Doctors_Backup_Degree
```

اگر مشکلی پیش آمد:
```sql
-- Restore از Backup:
DELETE FROM Doctors;
INSERT INTO Doctors SELECT * FROM Doctors_Backup_Degree;
```

---

## 📞 پشتیبانی

در صورت نیاز به کمک:
1. Output های Console را ذخیره کنید
2. Screenshot از خطاها بگیرید
3. نتیجه `Verify-Doctor-Degree-Fix.sql` را ارسال کنید

---

**تاریخ ایجاد**: 2025-01-08  
**نسخه**: 1.0  
**نویسنده**: ClinicApp Development Team

