# 📁 Scripts - ClinicApp Database Scripts

این پوشه شامل اسکریپت‌های SQL برای مدیریت دیتابیس `ClinicDb` است.

---

## 📋 **فهرست اسکریپت‌ها:**

### **🔗 خدمات مشترک (SharedServices):**

| اسکریپت | نوع | توضیحات | امنیت |
|---------|-----|---------|-------|
| `VIEW_SHARED_SERVICES_TO_DELETE.sql` | 👁️ مشاهده | نمایش خدمات مشترک که قرار است حذف شوند | ✅ 100% |
| `DELETE_SHARED_SERVICES_SAFE.sql` | 🗑️ حذف | حذف نرم خدمات مشترک (به جز اورژانس و تزریقات) | ⚠️ قابل برگشت |
| `RESTORE_SHARED_SERVICES.sql` | 🔄 بازگردانی | بازگردانی خدمات مشترک حذف شده | ✅ 100% |
| `SHARED_SERVICES_DELETE_GUIDE.md` | 📘 راهنما | راهنمای جامع اجرا | - |
| `QUICK_START.md` | ⚡ راهنمای سریع | دستورات سریع | - |

---

### **💳 بیمه (Insurance):**

| اسکریپت | نوع | توضیحات |
|---------|-----|---------|
| `TestPrimaryInsurancePlans.sql` | 🧪 تست | بررسی وضعیت بیمه‌های پایه |
| `FixInsuranceTypeData.sql` | 🔧 تصحیح | تصحیح نوع بیمه‌ها |
| `CheckInsuranceTypeStatus.sql` | 📊 گزارش | بررسی وضعیت کلی بیمه‌ها |

---

## 🚀 **راهنمای سریع:**

### **حذف خدمات مشترک:**

```bash
# 1. Backup
sqlcmd -S . -d ClinicDb -E -Q "BACKUP DATABASE [ClinicDb] TO DISK = N'C:\Backup\ClinicDb_Backup.bak' WITH FORMAT, INIT"

# 2. مشاهده
cd C:\Users\Developer\source\repos\ClinicApp\Scripts
sqlcmd -S . -d ClinicDb -E -i "VIEW_SHARED_SERVICES_TO_DELETE.sql"

# 3. حذف (با ویرایش RETURN و UserId)
sqlcmd -S . -d ClinicDb -E -i "DELETE_SHARED_SERVICES_SAFE.sql"

# 4. بازگردانی (در صورت نیاز)
sqlcmd -S . -d ClinicDb -E -i "RESTORE_SHARED_SERVICES.sql"
```

---

## 📘 **راهنماهای دقیق:**

- **حذف خدمات مشترک:** `SHARED_SERVICES_DELETE_GUIDE.md` (راهنمای کامل 400+ خط)
- **دستورات سریع:** `QUICK_START.md` (راهنمای خلاصه)
- **اتصال دیتابیس:** `../Docs/Database-Connection-Guide.md`

---

## ⚠️ **هشدارهای مهم:**

### **قبل از اجرای هر اسکریپت:**
1. ✅ **Backup کامل** دیتابیس بگیرید
2. ✅ اسکریپت **مشاهده** را اجرا کنید
3. ✅ گزارش را **دقیق بررسی** کنید
4. ✅ **تست** در محیط Development
5. ✅ در ساعات **خلوت** اجرا کنید

### **اسکریپت‌های خطرناک:**
- ⚠️ `DELETE_SHARED_SERVICES_SAFE.sql` - حذف دائمی داده‌ها (با Soft Delete)
- ⚠️ `FixInsuranceTypeData.sql` - تغییر داده‌های بیمه

### **اسکریپت‌های امن:**
- ✅ `VIEW_*.sql` - فقط مشاهده
- ✅ `Check*.sql` - فقط گزارش
- ✅ `Test*.sql` - فقط تست

---

## 🔧 **دستورات مفید:**

### **اتصال به دیتابیس:**
```bash
# Windows Authentication
sqlcmd -S . -d ClinicDb -E

# با Username/Password
sqlcmd -S . -d ClinicDb -U username -P password
```

### **اجرای اسکریپت:**
```bash
# از فایل
sqlcmd -S . -d ClinicDb -E -i "script.sql"

# Query مستقیم
sqlcmd -S . -d ClinicDb -E -Q "SELECT * FROM SharedServices"
```

### **Backup سریع:**
```bash
sqlcmd -S . -d ClinicDb -E -Q "BACKUP DATABASE [ClinicDb] TO DISK = N'C:\Backup\ClinicDb_$(date +%Y%m%d_%H%M%S).bak' WITH FORMAT, INIT"
```

---

## 📊 **آمار و گزارش:**

### **تعداد رکوردهای جداول اصلی:**
```sql
SELECT 'SharedServices' as TableName, COUNT(*) as Count FROM SharedServices WHERE IsDeleted = 0
UNION ALL
SELECT 'Services', COUNT(*) FROM Services WHERE IsDeleted = 0
UNION ALL
SELECT 'Departments', COUNT(*) FROM Departments WHERE IsDeleted = 0
UNION ALL
SELECT 'InsurancePlans', COUNT(*) FROM InsurancePlans WHERE IsDeleted = 0;
```

---

## 📞 **پشتیبانی:**

### **مشکلات رایج:**

1. **خطای اتصال:**
   - بررسی SQL Server Service
   - بررسی Connection String
   - بررسی User Permissions

2. **خطای Transaction:**
   - بررسی Transaction باز
   - اجرای `ROLLBACK TRANSACTION;`

3. **خطای Timeout:**
   - افزایش Command Timeout
   - بررسی Performance

---

## 🔒 **امنیت:**

### **Best Practices:**
- ✅ همیشه Backup بگیرید
- ✅ در محیط Test اجرا کنید
- ✅ گزارش را بررسی کنید
- ✅ از Transaction استفاده کنید
- ✅ Log کامل نگه دارید

### **دسترسی‌های مورد نیاز:**
- `SELECT` - مشاهده داده‌ها
- `UPDATE` - ویرایش داده‌ها
- `INSERT` - درج داده‌ها
- `DELETE` - حذف داده‌ها (برای Hard Delete)
- `CREATE TABLE` - ایجاد جداول موقت

---

## 📝 **تاریخچه:**

| تاریخ | نسخه | تغییرات |
|-------|------|---------|
| 1404/10/05 | 1.0.0 | ایجاد اسکریپت‌های SharedServices |
| - | - | - |

---

## 📚 **منابع مفید:**

- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/sql-server/)
- [Backup & Restore Best Practices](https://docs.microsoft.com/en-us/sql/relational-databases/backup-restore/)
- [Transaction Management](https://docs.microsoft.com/en-us/sql/t-sql/language-elements/transactions-transact-sql)

---

**📁 Scripts Directory - ClinicApp Database Management**

**🔧 استفاده با احتیاط | 📋 مستندسازی کامل | ✅ Production Ready**
