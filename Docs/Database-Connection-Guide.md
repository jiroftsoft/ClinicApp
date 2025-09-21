# 🔗 راهنمای اتصال به دیتابیس ClinicApp

## 📋 فهرست مطالب
- [اطلاعات اتصال](#اطلاعات-اتصال)
- [روش‌های اتصال](#روش‌های-اتصال)
- [دستورات مفید](#دستورات-مفید)
- [اسکریپت‌های آماده](#اسکریپت‌های-آماده)
- [مثال‌های کاربردی](#مثال‌های-کاربردی)

## 🔧 اطلاعات اتصال

### Connection String
```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Data Source=.;Initial Catalog=ClinicDb;Integrated Security=True;MultipleActiveResultSets=true;Persist Security Info=True;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

### پارامترهای اتصال:
- **Server**: `.` (Local SQL Server)
- **Database**: `ClinicDb`
- **Authentication**: `Integrated Security=True` (Windows Authentication)
- **Multiple Active Result Sets**: `true`

## 🛠️ روش‌های اتصال

### 1️⃣ SQL Server Management Studio (SSMS)
```
Server: .
Database: ClinicDb
Authentication: Windows Authentication
```

### 2️⃣ SQL Server Command Line (sqlcmd)
```bash
# اتصال با Windows Authentication
sqlcmd -S . -d ClinicDb -E

# اتصال با Username/Password (در صورت نیاز)
sqlcmd -S . -d ClinicDb -U username -P password
```

### 3️⃣ Visual Studio Server Explorer
```
Data Source: .
Initial Catalog: ClinicDb
Integrated Security: True
```

### 4️⃣ Azure Data Studio
```
Server: localhost
Database: ClinicDb
Authentication: Windows Authentication
```

## 📝 دستورات مفید

### بررسی اتصال
```sql
-- بررسی اتصال
SELECT @@SERVERNAME as ServerName, 
       DB_NAME() as DatabaseName,
       GETDATE() as CurrentTime;
```

### بررسی جداول اصلی
```sql
-- لیست جداول
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

### بررسی ساختار دیتابیس
```sql
-- اطلاعات جداول و ستون‌ها
SELECT 
    t.TABLE_NAME,
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.IS_NULLABLE,
    c.COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.TABLES t
LEFT JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
WHERE t.TABLE_TYPE = 'BASE TABLE'
ORDER BY t.TABLE_NAME, c.ORDINAL_POSITION;
```

## 📁 اسکریپت‌های آماده

### 1️⃣ بررسی وضعیت InsuranceType
```sql
-- فایل: Scripts/TestPrimaryInsurancePlans.sql
-- بررسی وضعیت بیمه‌های پایه و تکمیلی
```

### 2️⃣ تصحیح داده‌های InsuranceType
```sql
-- فایل: Scripts/FixInsuranceTypeData.sql
-- تصحیح نوع بیمه‌ها
```

### 3️⃣ بررسی وضعیت کلی
```sql
-- فایل: Scripts/CheckInsuranceTypeStatus.sql
-- بررسی وضعیت کلی سیستم
```

## 🎯 مثال‌های کاربردی

### اجرای اسکریپت از Command Line
```bash
# اجرای اسکریپت بررسی
sqlcmd -S . -d ClinicDb -E -i "Scripts\TestPrimaryInsurancePlans.sql"

# اجرای اسکریپت تصحیح
sqlcmd -S . -d ClinicDb -E -i "Scripts\FixInsuranceTypeData.sql"

# اجرای اسکریپت بررسی وضعیت
sqlcmd -S . -d ClinicDb -E -i "Scripts\CheckInsuranceTypeStatus.sql"
```

### اجرای Query مستقیم
```bash
# اجرای Query ساده
sqlcmd -S . -d ClinicDb -E -Q "SELECT COUNT(*) FROM InsurancePlans"

# اجرای Query با خروجی فرمت شده
sqlcmd -S . -d ClinicDb -E -Q "SELECT * FROM InsurancePlans WHERE IsActive = 1" -W
```

### بررسی جداول بیمه
```sql
-- بررسی جداول بیمه
SELECT 'InsuranceProviders' as TableName, COUNT(*) as RecordCount FROM InsuranceProviders
UNION ALL
SELECT 'InsurancePlans', COUNT(*) FROM InsurancePlans
UNION ALL
SELECT 'InsuranceTariffs', COUNT(*) FROM InsuranceTariffs
UNION ALL
SELECT 'PatientInsurances', COUNT(*) FROM PatientInsurances;
```

## 🔍 دستورات عیب‌یابی

### بررسی اتصال
```sql
-- بررسی وضعیت اتصال
SELECT 
    session_id,
    login_name,
    host_name,
    program_name,
    client_interface_name,
    login_time,
    last_request_start_time
FROM sys.dm_exec_sessions 
WHERE database_id = DB_ID('ClinicDb');
```

### بررسی جداول فعال
```sql
-- بررسی جداول با بیشترین رکورد
SELECT 
    t.name as TableName,
    p.rows as RecordCount
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0,1)
ORDER BY p.rows DESC;
```

### بررسی فضاهای خالی
```sql
-- بررسی فضای استفاده شده
SELECT 
    DB_NAME() as DatabaseName,
    (size * 8.0 / 1024) as SizeMB
FROM sys.database_files;
```

## 📊 گزارش‌های مفید

### گزارش بیمه‌ها
```sql
-- گزارش کامل بیمه‌ها
SELECT 
    p.InsurancePlanId,
    p.Name as PlanName,
    pr.Name as ProviderName,
    p.CoveragePercent,
    p.InsuranceType,
    CASE 
        WHEN p.InsuranceType = 1 THEN 'بیمه پایه'
        WHEN p.InsuranceType = 2 THEN 'بیمه تکمیلی'
        ELSE 'نامشخص'
    END as TypeDescription
FROM InsurancePlans p
INNER JOIN InsuranceProviders pr ON p.InsuranceProviderId = pr.InsuranceProviderId
WHERE p.IsActive = 1 AND p.IsDeleted = 0
ORDER BY p.InsuranceType, p.Name;
```

### گزارش خدمات
```sql
-- گزارش خدمات فعال
SELECT 
    s.ServiceId,
    s.Title,
    s.ServiceCode,
    s.Price,
    sc.Name as CategoryName,
    d.Name as DepartmentName
FROM Services s
LEFT JOIN ServiceCategories sc ON s.ServiceCategoryId = sc.ServiceCategoryId
LEFT JOIN Departments d ON sc.DepartmentId = d.DepartmentId
WHERE s.IsActive = 1 AND s.IsDeleted = 0
ORDER BY d.Name, sc.Name, s.Title;
```

## ⚠️ نکات مهم

### 1️⃣ امنیت
- همیشه از Windows Authentication استفاده کنید
- از Connection String در کد استفاده نکنید
- از Web.config برای ذخیره Connection String استفاده کنید

### 2️⃣ عملکرد
- از `MultipleActiveResultSets=true` استفاده کنید
- از Connection Pooling استفاده کنید
- Query های طولانی را بهینه کنید

### 3️⃣ نگهداری
- به طور منظم Backup بگیرید
- Log های دیتابیس را بررسی کنید
- Index های دیتابیس را بهینه کنید

## 🚀 دستورات سریع

### اتصال سریع
```bash
# اتصال سریع به دیتابیس
sqlcmd -S . -d ClinicDb -E
```

### اجرای اسکریپت
```bash
# اجرای اسکریپت
sqlcmd -S . -d ClinicDb -E -i "Scripts\YourScript.sql"
```

### خروج از sqlcmd
```sql
-- خروج از sqlcmd
EXIT
```

## 📞 پشتیبانی

در صورت بروز مشکل:
1. بررسی Connection String
2. بررسی SQL Server Service
3. بررسی Firewall
4. بررسی User Permissions

---
**تاریخ ایجاد**: 2024-12-20  
**نسخه**: 1.0  
**نویسنده**: ClinicApp Development Team
