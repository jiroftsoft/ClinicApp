# 📊 تنظیمات حرفه‌ای Serilog برای ClinicApp

## 🎯 **نمای کلی**

این سند توضیح کاملی از تنظیمات بهینه‌سازی شده Serilog برای اپلیکیشن ClinicApp ارائه می‌دهد.

## 🏗️ **ساختار فایل‌ها**

### **1️⃣ فایل‌های اصلی:**
- `Global.asax.cs` - تنظیمات اصلی Serilog
- `Helpers/LoggingHelper.cs` - متدهای کمکی برای لاگ‌گیری
- `Helpers/StructuredLogger.cs` - لاگ‌گیری ساختاریافته
- `Helpers/LoggingConfiguration.cs` - پیکربندی پیشرفته

### **2️⃣ فایل‌های لاگ:**
- `App_Data/Logs/clinicapp-{date}.log` - لاگ اصلی
- `App_Data/Logs/errors-{date}.log` - لاگ خطاها
- `App_Data/Logs/performance-{date}.log` - لاگ Performance

## 🔧 **تنظیمات محیط‌ها**

### **Development:**
```csharp
LogLevel: Debug
Retention: 7 days
Console: Enabled
Seq: Enabled
Database: Enabled
Email: Disabled
```

### **Staging:**
```csharp
LogLevel: Information
Retention: 30 days
Console: Disabled
Seq: Enabled
Database: Enabled
Email: Disabled
```

### **Production:**
```csharp
LogLevel: Warning
Retention: 90 days
Console: Disabled
Seq: Disabled
Database: Enabled
Email: Enabled (Fatal only)
```

## 📁 **Sink های پیکربندی شده**

### **1️⃣ File Sinks:**
- **clinicapp-{date}.log**: لاگ اصلی با تمام سطوح
- **errors-{date}.log**: فقط خطاها (Error, Fatal)
- **performance-{date}.log**: لاگ Performance

### **2️⃣ Console Sink:**
- فقط در Development
- با رنگ‌بندی ANSI
- Template بهینه‌سازی شده

### **3️⃣ Database Sink:**
- جدول: `ApplicationLogs`
- Batch Size: 50
- Period: 5 seconds
- Auto Create Table: true

### **4️⃣ Seq Sink:**
- فقط در Development و Staging
- URL: `http://localhost:5341`
- API Key: از Web.config

### **5️⃣ Email Sink:**
- فقط در Production
- فقط برای Fatal errors
- SMTP از Web.config

## 🎨 **Template های خروجی**

### **File Template:**
```
{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}{Properties:j}{NewLine}---{NewLine}
```

### **Console Template:**
```
[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}
```

### **Email Template:**
```
خطای بحرانی در {Application} - {Environment}{NewLine}{NewLine}{Timestamp:yyyy-MM-dd HH:mm:ss}{NewLine}{Level}: {Message}{NewLine}{Exception}
```

## 🔍 **Enrichers فعال**

### **1️⃣ Built-in Enrichers:**
- `FromLogContext()` - Context properties
- `WithMachineName()` - نام سرور
- `WithEnvironmentUserName()` - نام کاربر سیستم
- `WithProcessId()` - شناسه Process
- `WithThreadId()` - شناسه Thread

### **2️⃣ Custom Properties:**
- `Application`: "ClinicApp"
- `Environment`: Development/Staging/Production
- `Version`: نسخه اپلیکیشن
- `ServerName`: نام سرور

## 🚫 **فیلترهای فعال**

### **1️⃣ Static Files:**
```csharp
.css, .js, .png, .jpg, .gif, .ico, .woff, .woff2, .ttf, .svg, .eot
```

### **2️⃣ Health Checks:**
```csharp
/health, /ping, /status
```

### **3️⃣ Framework Logs:**
```csharp
Microsoft.Owin, System.Web.Http, System.Web.Mvc, Unity
```

## 📊 **استفاده از LoggingHelper**

### **مثال‌های کاربردی:**

#### **1️⃣ Business Operations:**
```csharp
LoggingHelper.LogBusinessOperation("PatientCreate", userId, "ایجاد بیمار جدید", patientData);
```

#### **2️⃣ Security Events:**
```csharp
LoggingHelper.LogSecurityEvent("LoginAttempt", userId, ipAddress, "تلاش ورود ناموفق");
```

#### **3️⃣ Financial Operations:**
```csharp
LoggingHelper.LogFinancialOperation("Payment", 150000, "IRR", userId, "پرداخت موفق");
```

#### **4️⃣ Medical Operations:**
```csharp
LoggingHelper.LogMedicalOperation("ReceptionCreate", patientId, doctorId, "ثبت پذیرش");
```

#### **5️⃣ Performance Monitoring:**
```csharp
LoggingHelper.LogPerformance("DatabaseQuery", 250, "Query execution time");
```

## 📋 **استفاده از StructuredLogger**

### **مثال‌های کاربردی:**

#### **1️⃣ Basic Operation:**
```csharp
var logger = new StructuredLogger("ServiceName");
logger.LogOperation("OperationName", new { Id = 123, Name = "Test" });
```

#### **2️⃣ Operation with Timing:**
```csharp
logger.LogOperationWithTiming("DatabaseQuery", 150, new { Table = "Patients" });
```

#### **3️⃣ Operation with Result:**
```csharp
logger.LogOperationWithResult("UserLogin", true, new { UserId = 123 }, null);
```

#### **4️⃣ Progress Tracking:**
```csharp
logger.LogProgress("DataProcessing", 50, 100, "Processing records");
```

## 🎯 **Best Practices**

### **1️⃣ Log Levels:**
- **Debug**: اطلاعات تفصیلی برای Debug
- **Information**: رویدادهای مهم کسب‌وکار
- **Warning**: شرایط غیرعادی اما قابل مدیریت
- **Error**: خطاهای قابل بازیابی
- **Fatal**: خطاهای بحرانی

### **2️⃣ Structured Logging:**
```csharp
// ✅ خوب
Log.Information("User {UserId} created patient {PatientId}", userId, patientId);

// ❌ بد
Log.Information($"User {userId} created patient {patientId}");
```

### **3️⃣ Context Properties:**
```csharp
Log.Information("Operation completed with {@Result}", result);
```

### **4️⃣ Exception Logging:**
```csharp
try
{
    // operation
}
catch (Exception ex)
{
    Log.Error(ex, "Error occurred during {Operation}", operationName);
}
```

## 🔧 **تنظیمات Web.config**

### **AppSettings مورد نیاز:**
```xml
<add key="Environment" value="Development" />
<add key="SeqUrl" value="http://localhost:5341" />
<add key="SeqApiKey" value="" />
<add key="Email:AdminEmail" value="admin@clinicapp.com" />
<add key="Logging:EnableEmailAlerts" value="true" />
<add key="Logging:EnableSeq" value="true" />
<add key="Logging:EnableConsole" value="true" />
<add key="Logging:EnableDatabase" value="true" />
<add key="Logging:EnableFileLogging" value="true" />
```

## 📈 **مانیتورینگ و تحلیل**

### **1️⃣ Seq Dashboard:**
- Real-time log viewing
- Advanced filtering
- Performance metrics
- Error analysis

### **2️⃣ Database Queries:**
```sql
-- خطاهای امروز
SELECT * FROM ApplicationLogs 
WHERE Level = 'Error' AND Date >= GETDATE()

-- Performance queries
SELECT * FROM ApplicationLogs 
WHERE Message LIKE '%PERFORMANCE%' 
ORDER BY TimeStamp DESC
```

### **3️⃣ File Analysis:**
- Log rotation automatic
- Size-based rotation
- Retention policy
- Compression support

## 🚀 **Performance Optimization**

### **1️⃣ Async Sinks:**
- تمام Sink ها Async هستند
- Non-blocking operations
- Better performance

### **2️⃣ Batch Processing:**
- Database: 50 records per batch
- Period: 5 seconds
- Memory efficient

### **3️⃣ Filtering:**
- Static files excluded
- Health checks excluded
- Framework logs minimized

## 🛡️ **Security Considerations**

### **1️⃣ Sensitive Data:**
- Password ها لاگ نمی‌شوند
- Credit card numbers masked
- Personal data encrypted

### **2️⃣ Access Control:**
- Log files protected
- Database permissions
- Email notifications secure

## 📊 **Metrics و KPIs**

### **1️⃣ Application Metrics:**
- Request count
- Response time
- Error rate
- User activity

### **2️⃣ Business Metrics:**
- Patient registrations
- Appointment bookings
- Payment transactions
- Insurance claims

### **3️⃣ System Metrics:**
- Memory usage
- CPU utilization
- Database performance
- Cache hit rate

## 🔄 **Maintenance**

### **1️⃣ Log Rotation:**
- Daily rotation
- Size-based rotation
- Automatic cleanup
- Retention policy

### **2️⃣ Database Maintenance:**
```sql
-- حذف لاگ‌های قدیمی
DELETE FROM ApplicationLogs 
WHERE TimeStamp < DATEADD(day, -90, GETDATE())
```

### **3️⃣ File Cleanup:**
- Automatic file deletion
- Size monitoring
- Disk space management

## 🎯 **نتیجه‌گیری**

این تنظیمات Serilog برای ClinicApp بهینه‌سازی شده و شامل:

✅ **Performance بهینه**  
✅ **Security مناسب**  
✅ **Maintenance آسان**  
✅ **Monitoring کامل**  
✅ **Scalability بالا**  

**🚀 آماده برای استفاده در Production!**
