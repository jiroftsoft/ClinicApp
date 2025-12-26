# ⚡ راهنمای سریع - حذف خدمات مشترک

## 🎯 **هدف:**
حذف نرم خدمات مشترک برای تمام دپارتمان‌ها **به جز اورژانس و تزریقات**

---

## 🚀 **دستورات سریع:**

### **1️⃣ Backup (الزامی)** 💾
```bash
sqlcmd -S . -d ClinicDb -E -Q "BACKUP DATABASE [ClinicDb] TO DISK = N'C:\Backup\ClinicDb_Before_Delete.bak' WITH FORMAT, INIT, STATS = 10"
```

---

### **2️⃣ مشاهده (توصیه می‌شود)** 👁️
```bash
cd C:\Users\Developer\source\repos\ClinicApp\Scripts
sqlcmd -S . -d ClinicDb -E -i "VIEW_SHARED_SERVICES_TO_DELETE.sql"
```

---

### **3️⃣ حذف (با احتیاط)** ⚠️

#### **گام 1: اجرای اولیه**
```bash
sqlcmd -S . -d ClinicDb -E -i "DELETE_SHARED_SERVICES_SAFE.sql"
```

#### **گام 2: ویرایش اسکریپت**
در فایل `DELETE_SHARED_SERVICES_SAFE.sql`:

1. حذف این خط:
```sql
RETURN; -- ⛔ این خط را حذف کنید برای اجرای واقعی
```

2. تغییر UserId:
```sql
DECLARE @DeletedByUserId NVARCHAR(450) = 'YOUR_USER_ID'; -- 🔧 تغییر دهید
```

#### **گام 3: اجرای نهایی**
```bash
sqlcmd -S . -d ClinicDb -E -i "DELETE_SHARED_SERVICES_SAFE.sql"
```

---

### **4️⃣ بازگردانی (در صورت نیاز)** 🔄
```bash
sqlcmd -S . -d ClinicDb -E -i "RESTORE_SHARED_SERVICES.sql"
```

---

## ⚠️ **یادآوری:**
- ✅ حتماً Backup بگیرید
- ✅ گزارش `VIEW` را بررسی کنید
- ✅ UserId واقعی را جایگزین کنید
- ✅ این یک Soft Delete است (برگشت‌پذیر)

---

## 📘 **راهنمای کامل:**
برای جزئیات بیشتر، فایل `SHARED_SERVICES_DELETE_GUIDE.md` را مطالعه کنید.

---

**✅ آماده اجرا!**

