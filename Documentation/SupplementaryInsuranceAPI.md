# 📋 مستندات API سیستم بیمه تکمیلی

## 🎯 **معرفی کلی**

سیستم بیمه تکمیلی برای محاسبه و مدیریت بیمه‌های تکمیلی بیماران در محیط‌های درمانی طراحی شده است. این سیستم شامل سرویس‌های تخصصی، کنترلرها، و API های مختلف برای مدیریت کامل فرآیند بیمه تکمیلی است.

---

## 🏗️ **معماری سیستم**

### **لایه‌های اصلی:**
1. **Entity Layer** - موجودیت‌های پایگاه داده
2. **Service Layer** - منطق کسب‌وکار
3. **Controller Layer** - API endpoints
4. **View Layer** - رابط کاربری
5. **Cache Layer** - بهینه‌سازی عملکرد

---

## 🔧 **سرویس‌های اصلی**

### **1. SupplementaryInsuranceService**

#### **متدهای اصلی:**

##### **CalculateSupplementaryInsuranceAsync**
```csharp
Task<ServiceResult<SupplementaryCalculationResult>> CalculateSupplementaryInsuranceAsync(
    int patientId, 
    int serviceId, 
    decimal serviceAmount, 
    decimal primaryCoverage, 
    DateTime calculationDate)
```

**توضیحات:**
- محاسبه بیمه تکمیلی برای یک بیمار و خدمت خاص
- در نظر گیری پوشش بیمه اصلی
- اعمال درصد پوشش و سقف پرداخت بیمه تکمیلی

**پارامترها:**
- `patientId`: شناسه بیمار
- `serviceId`: شناسه خدمت
- `serviceAmount`: مبلغ کل خدمت
- `primaryCoverage`: مبلغ پوشش بیمه اصلی
- `calculationDate`: تاریخ محاسبه

**مقدار بازگشتی:**
```csharp
public class SupplementaryCalculationResult
{
    public int PatientId { get; set; }
    public int ServiceId { get; set; }
    public decimal ServiceAmount { get; set; }
    public decimal PrimaryCoverage { get; set; }
    public decimal SupplementaryCoverage { get; set; }
    public decimal FinalPatientShare { get; set; }
    public decimal TotalCoverage { get; set; }
    public DateTime CalculationDate { get; set; }
    public string Notes { get; set; }
}
```

##### **CalculateAdvancedSupplementaryInsuranceAsync**
```csharp
Task<ServiceResult<SupplementaryCalculationResult>> CalculateAdvancedSupplementaryInsuranceAsync(
    int patientId, 
    int serviceId, 
    decimal serviceAmount, 
    decimal primaryCoverage, 
    DateTime calculationDate,
    Dictionary<string, object> advancedSettings = null)
```

**توضیحات:**
- محاسبه پیشرفته بیمه تکمیلی با تنظیمات خاص
- پشتیبانی از تخفیف، سقف پرداخت، و فرانشیز
- تحلیل و آمارگیری پیشرفته

**تنظیمات پیشرفته:**
```csharp
var advancedSettings = new Dictionary<string, object>
{
    {"discountPercent", 10},           // درصد تخفیف
    {"maxPatientPayment", 100000},     // سقف پرداخت بیمار
    {"deductible", 25000},             // فرانشیز
    {"timeRestriction", true}          // محدودیت زمانی
};
```

##### **GetSupplementaryTariffsAsync**
```csharp
Task<ServiceResult<List<SupplementaryTariffViewModel>>> GetSupplementaryTariffsAsync(int planId)
```

**توضیحات:**
- دریافت لیست تعرفه‌های بیمه تکمیلی برای یک طرح بیمه
- استفاده از Cache برای بهبود عملکرد

##### **GetSupplementarySettingsAsync**
```csharp
Task<ServiceResult<SupplementarySettings>> GetSupplementarySettingsAsync(int planId)
```

**توضیحات:**
- دریافت تنظیمات بیمه تکمیلی برای یک طرح بیمه
- شامل درصد پوشش، سقف پرداخت، و تنظیمات JSON

---

### **2. SupplementaryInsuranceCacheService**

#### **متدهای Cache:**

##### **GetCachedSupplementaryTariffsAsync**
```csharp
Task<ServiceResult<List<SupplementaryTariffViewModel>>> GetCachedSupplementaryTariffsAsync(int planId)
```

**ویژگی‌های Cache:**
- **مدت انقضا**: 30 دقیقه
- **حافظه**: درون‌حافظه‌ای
- **بهینه‌سازی**: برای ترافیک بالا

##### **GetCachedSupplementarySettingsAsync**
```csharp
Task<ServiceResult<SupplementarySettings>> GetCachedSupplementarySettingsAsync(int planId)
```

**ویژگی‌های Cache:**
- **مدت انقضا**: 60 دقیقه
- **مدیریت خودکار**: پاک‌سازی خودکار

##### **GetCachedCalculationResultAsync**
```csharp
Task<ServiceResult<SupplementaryCalculationResult>> GetCachedCalculationResultAsync(
    int patientId, 
    int serviceId, 
    decimal serviceAmount, 
    decimal primaryCoverage, 
    DateTime calculationDate,
    ISupplementaryInsuranceService supplementaryService)
```

**ویژگی‌های Cache:**
- **مدت انقضا**: 15 دقیقه
- **کلید Cache**: ترکیب پارامترهای محاسبه

---

### **3. SupplementaryInsuranceMonitoringService**

#### **متدهای Monitoring:**

##### **LogCalculationEvent**
```csharp
void LogCalculationEvent(CalculationEvent calculationEvent)
```

**رویدادهای ثبت شده:**
- محاسبات موفق
- محاسبات ناموفق
- زمان اجرا
- خطاها و استثناها

##### **GetPerformanceReport**
```csharp
PerformanceReport GetPerformanceReport(DateTime? fromDate = null, DateTime? toDate = null)
```

**آمار عملکرد:**
- درصد موفقیت
- درصد خطا
- تعداد محاسبات
- زمان متوسط اجرا

##### **GetUsageStatistics**
```csharp
UsageStatistics GetUsageStatistics(DateTime? fromDate = null, DateTime? toDate = null)
```

**آمار استفاده:**
- تعداد کل محاسبات
- محاسبات موفق
- محاسبات ناموفق
- روند استفاده

---

## 🌐 **API Endpoints**

### **Controller: PatientInsuranceController**

#### **GET /PatientInsurance/SupplementaryCalculation**
```http
GET /PatientInsurance/SupplementaryCalculation?patientId=1&serviceId=1&serviceAmount=1000000&primaryCoverage=500000
```

**پاسخ:**
```json
{
    "success": true,
    "data": {
        "patientId": 1,
        "serviceId": 1,
        "serviceAmount": 1000000,
        "primaryCoverage": 500000,
        "supplementaryCoverage": 250000,
        "finalPatientShare": 250000,
        "totalCoverage": 750000,
        "calculationDate": "2024-01-15T10:30:00Z",
        "notes": "محاسبه بر اساس تعرفه: 50%"
    },
    "message": "محاسبه با موفقیت انجام شد"
}
```

#### **POST /PatientInsurance/AdvancedCalculation**
```http
POST /PatientInsurance/AdvancedCalculation
Content-Type: application/json

{
    "patientId": 1,
    "serviceId": 1,
    "serviceAmount": 1500000,
    "primaryCoverage": 600000,
    "calculationDate": "2024-01-15T10:30:00Z",
    "advancedSettings": {
        "discountPercent": 10,
        "maxPatientPayment": 100000,
        "deductible": 25000
    }
}
```

#### **GET /PatientInsurance/SupplementaryTariffs/{planId}**
```http
GET /PatientInsurance/SupplementaryTariffs/1
```

**پاسخ:**
```json
{
    "success": true,
    "data": [
        {
            "tariffId": 1,
            "planId": 1,
            "serviceId": 1,
            "serviceName": "خدمت تست",
            "coveragePercent": 50,
            "maxPayment": 100000,
            "settings": "{\"priority\": \"high\"}"
        }
    ]
}
```

---

## 🔒 **امنیت و مجوزها**

### **نقش‌های مورد نیاز:**
- **Admin**: دسترسی کامل
- **Doctor**: دسترسی به محاسبات
- **Reception**: دسترسی به محاسبات و ثبت

### **اعتبارسنجی:**
- بررسی صحت شناسه بیمار
- بررسی فعال بودن بیمه تکمیلی
- اعتبارسنجی مبلغ خدمت
- بررسی محدودیت‌های زمانی

---

## ⚡ **بهینه‌سازی عملکرد**

### **استراتژی‌های Cache:**
1. **Cache درون‌حافظه‌ای** برای تعرفه‌ها
2. **Cache توزیع‌شده** برای تنظیمات
3. **Cache نتایج محاسبه** برای درخواست‌های تکراری

### **بهینه‌سازی‌های Database:**
1. **Indexing** روی فیلدهای کلیدی
2. **Query Optimization** برای جستجوهای پیچیده
3. **Connection Pooling** برای اتصالات

### **محدودیت‌های عملکرد:**
- **زمان پاسخ**: کمتر از 2 ثانیه
- **حافظه**: کمتر از 100MB
- **CPU**: کمتر از 50% استفاده

---

## 🚨 **مدیریت خطا**

### **انواع خطاها:**
1. **ValidationError**: خطاهای اعتبارسنجی
2. **CalculationError**: خطاهای محاسبه
3. **DatabaseError**: خطاهای پایگاه داده
4. **CacheError**: خطاهای Cache

### **لاگ‌گیری:**
- **Serilog** برای لاگ‌گیری ساختاریافته
- **سطح‌های لاگ**: Information, Warning, Error
- **متدهای لاگ**: Console, File, Database

---

## 📊 **مانیتورینگ و گزارش‌گیری**

### **معیارهای کلیدی:**
- **درصد موفقیت محاسبات**
- **زمان متوسط پاسخ**
- **تعداد درخواست‌ها در دقیقه**
- **استفاده از Cache**

### **هشدارها:**
- **درصد خطا بیش از 5%**
- **زمان پاسخ بیش از 5 ثانیه**
- **استفاده از حافظه بیش از 80%**

---

## 🔄 **مثال‌های کاربردی**

### **مثال 1: محاسبه ساده**
```csharp
var result = await _supplementaryService.CalculateSupplementaryInsuranceAsync(
    patientId: 1,
    serviceId: 1,
    serviceAmount: 1000000m,
    primaryCoverage: 500000m,
    calculationDate: DateTime.UtcNow
);

if (result.Success)
{
    var calculation = result.Data;
    Console.WriteLine($"سهم بیمار: {calculation.FinalPatientShare:N0} ریال");
    Console.WriteLine($"پوشش بیمه تکمیلی: {calculation.SupplementaryCoverage:N0} ریال");
}
```

### **مثال 2: محاسبه پیشرفته**
```csharp
var advancedSettings = new Dictionary<string, object>
{
    {"discountPercent", 15},
    {"maxPatientPayment", 50000},
    {"deductible", 10000}
};

var result = await _supplementaryService.CalculateAdvancedSupplementaryInsuranceAsync(
    patientId: 1,
    serviceId: 1,
    serviceAmount: 2000000m,
    primaryCoverage: 800000m,
    calculationDate: DateTime.UtcNow,
    advancedSettings: advancedSettings
);
```

### **مثال 3: دریافت آمار عملکرد**
```csharp
var performanceReport = _monitoringService.GetPerformanceReport(
    fromDate: DateTime.UtcNow.AddDays(-30),
    toDate: DateTime.UtcNow
);

Console.WriteLine($"درصد موفقیت: {performanceReport.SuccessRate:F2}%");
Console.WriteLine($"تعداد محاسبات: {performanceReport.TotalCalculations}");
```

---

## 📝 **نکات مهم**

### **برای توسعه‌دهندگان:**
1. **همیشه از async/await استفاده کنید**
2. **خطاها را به درستی مدیریت کنید**
3. **از Cache برای بهبود عملکرد استفاده کنید**
4. **لاگ‌گیری را جدی بگیرید**

### **برای مدیران سیستم:**
1. **مانیتورینگ را فعال نگه دارید**
2. **Cache را به‌روزرسانی کنید**
3. **لاگ‌ها را بررسی کنید**
4. **عملکرد را نظارت کنید**

---

## 🔗 **لینک‌های مفید**

- [مستندات Entity Framework](https://docs.microsoft.com/en-us/ef/)
- [راهنمای Serilog](https://serilog.net/)
- [مستندات ASP.NET MVC](https://docs.microsoft.com/en-us/aspnet/mvc/)
- [راهنمای Cache در .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/caching)

---

**آخرین به‌روزرسانی**: 15 دی 1403  
**نسخه**: 1.0.0  
**نویسنده**: تیم توسعه سیستم‌های درمانی
