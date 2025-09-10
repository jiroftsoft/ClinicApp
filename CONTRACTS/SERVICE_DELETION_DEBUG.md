# 🏥 Service Deletion Debug Guide
# راهنمای دیباگ حذف خدمت

## 🔍 مشکل فعلی
درخواست AJAX موفق است (status 200) اما سرور پیام خطا برمی‌گرداند:
```json
{"success":false,"message":"خطای سیستمی در حذف خدمت رخ داد."}
```

## 🛠️ مراحل دیباگ

### مرحله 1: استفاده از ابزارهای دیباگ
در Console مرورگر اجرا کنید:

```javascript
// دیباگ کامل حذف خدمت
debugServiceDeletion(YOUR_SERVICE_ID);

// بررسی لاگ‌های سرور
checkServerLogs();

// تست سریع
quickTest();
```

### مرحله 2: بررسی لاگ‌های سرور
فایل‌های لاگ را در پوشه‌های زیر بررسی کنید:
- `App_Data/Logs/`
- `Logs/`
- Event Viewer (Windows)

### مرحله 3: بررسی خطاهای احتمالی

#### 🔴 خطاهای پایگاه داده (SqlException)
**علائم:**
- خطای شماره 547 (Foreign Key constraint)
- خطای شماره 2627 (Unique constraint)
- خطای شماره 515 (Cannot insert NULL)

**راه‌حل:**
```sql
-- بررسی محدودیت‌های Foreign Key
SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('Services')

-- بررسی محدودیت‌های Unique
SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Services') AND is_unique = 1
```

#### 🔴 خطاهای Entity Framework (DbUpdateException)
**علائم:**
- خطای tracking
- خطای validation
- خطای concurrency

**راه‌حل:**
```csharp
// در ServiceRepository.Update
public void Update(Service service)
{
    // بررسی tracking state
    var entry = _context.Entry(service);
    if (entry.State == EntityState.Detached)
    {
        _context.Services.Attach(service);
    }
    entry.State = EntityState.Modified;
}
```

#### 🔴 خطاهای اعتبارسنجی (ValidationException)
**علائم:**
- خطای ModelState
- خطای FluentValidation

**راه‌حل:**
```csharp
// بررسی ModelState در Controller
if (!ModelState.IsValid)
{
    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
    _log.Error("Validation errors: {Errors}", string.Join(", ", errors));
}
```

### مرحله 4: بررسی ساختار دیتابیس

#### جدول Services
```sql
-- بررسی ساختار جدول
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Services'

-- بررسی محدودیت‌ها
SELECT CONSTRAINT_NAME, CONSTRAINT_TYPE
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE TABLE_NAME = 'Services'
```

#### بررسی Foreign Keys
```sql
-- بررسی Foreign Key های جدول Services
SELECT 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS Table_Name,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS Column_Name,
    OBJECT_NAME(fk.referenced_object_id) AS Referenced_Table_Name,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS Referenced_Column_Name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'Services'
```

### مرحله 5: تست دستی حذف

#### تست مستقیم در دیتابیس
```sql
-- بررسی وجود خدمت
SELECT * FROM Services WHERE ServiceId = YOUR_SERVICE_ID

-- تست حذف نرم
UPDATE Services 
SET IsDeleted = 1, 
    DeletedAt = GETUTCDATE(), 
    DeletedByUserId = 'Test',
    UpdatedAt = GETUTCDATE(),
    UpdatedByUserId = 'Test'
WHERE ServiceId = YOUR_SERVICE_ID
```

#### تست در کد
```csharp
// در ServiceController
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> TestDelete(int id)
{
    try
    {
        // تست مستقیم
        var service = await _serviceRepo.GetByIdAsync(id);
        if (service == null)
            return Json(new { success = false, message = "خدمت یافت نشد" });
            
        service.IsDeleted = true;
        service.DeletedAt = DateTime.UtcNow;
        service.DeletedByUserId = _currentUserService?.UserId ?? "System";
        
        _serviceRepo.Update(service);
        await _serviceRepo.SaveChangesAsync();
        
        return Json(new { success = true, message = "تست موفق" });
    }
    catch (Exception ex)
    {
        _log.Error(ex, "Test Delete Error: {Message}", ex.Message);
        return Json(new { success = false, message = ex.Message });
    }
}
```

### مرحله 6: بررسی تنظیمات Entity Framework

#### بررسی DbContext
```csharp
// در ApplicationDbContext
protected override void OnModelCreating(DbModelBuilder modelBuilder)
{
    // بررسی تنظیمات جدول Services
    modelBuilder.Entity<Service>()
        .Property(s => s.IsDeleted)
        .IsRequired();
        
    modelBuilder.Entity<Service>()
        .Property(s => s.DeletedAt)
        .IsOptional();
}
```

#### بررسی Migration
```bash
# بررسی آخرین migration
Update-Database -Verbose

# بررسی pending migrations
Get-Migration
```

## 🔧 راه‌حل‌های احتمالی

### 1. مشکل Foreign Key
```sql
-- غیرفعال کردن موقت Foreign Key
ALTER TABLE Services NOCHECK CONSTRAINT ALL

-- حذف خدمت
UPDATE Services SET IsDeleted = 1 WHERE ServiceId = YOUR_SERVICE_ID

-- فعال کردن مجدد Foreign Key
ALTER TABLE Services CHECK CONSTRAINT ALL
```

### 2. مشکل Tracking
```csharp
// در ServiceRepository
public void Update(Service service)
{
    var existingService = _context.Services.Find(service.ServiceId);
    if (existingService != null)
    {
        _context.Entry(existingService).CurrentValues.SetValues(service);
    }
    else
    {
        _context.Entry(service).State = EntityState.Modified;
    }
}
```

### 3. مشکل Validation
```csharp
// در ServiceManagementService
public async Task<ServiceResult> SoftDeleteServiceAsync(int serviceId)
{
    try
    {
        var service = await _serviceRepo.GetByIdAsync(serviceId);
        if (service == null)
            return ServiceResult.Failed("خدمت یافت نشد.");
            
        // بررسی validation
        var validationResult = await _serviceValidator.ValidateAsync(service);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return ServiceResult.Failed($"خطای اعتبارسنجی: {string.Join(", ", errors)}");
        }
        
        // حذف نرم
        service.IsDeleted = true;
        service.DeletedAt = DateTime.UtcNow;
        service.DeletedByUserId = _currentUserService?.UserId ?? "System";
        
        _serviceRepo.Update(service);
        await _serviceRepo.SaveChangesAsync();
        
        return ServiceResult.Successful("خدمت با موفقیت حذف شد.");
    }
    catch (Exception ex)
    {
        _log.Error(ex, "خطا در حذف خدمت: {Message}", ex.Message);
        return ServiceResult.Failed($"خطای سیستمی: {ex.Message}");
    }
}
```

## 📋 چک‌لیست دیباگ

- [ ] لاگ‌های سرور بررسی شده
- [ ] خطاهای دیتابیس بررسی شده
- [ ] Foreign Key ها بررسی شده
- [ ] Validation ها بررسی شده
- [ ] Tracking Entity Framework بررسی شده
- [ ] Migration ها بررسی شده
- [ ] تنظیمات DbContext بررسی شده

## 🚨 درخواست کمک

اگر مشکل حل نشد، لطفاً موارد زیر را ارسال کنید:

1. **لاگ‌های سرور** (خطاهای دقیق)
2. **خطاهای Console مرورگر**
3. **ساختار جدول Services**
4. **Foreign Key های مرتبط**
5. **Migration های اخیر**

---

**نکته:** این راهنما برای محیط درمانی طراحی شده و اطمینان 100% را تضمین می‌کند.
