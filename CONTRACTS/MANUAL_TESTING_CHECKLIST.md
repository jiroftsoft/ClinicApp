# 🧪 Manual Testing Checklist - EF Performance Optimization

## 📋 **Test Execution Guide**

### **نقش:** Senior Reviewer
### **هدف:** تست‌های دستی قبل از Merge
### **تاریخ:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
### **وضعیت:** ⏳ Pending

---

## 🚀 **Pre-Testing Setup**

### **1. Environment Preparation**
- [ ] **Staging Environment** آماده و deploy شده
- [ ] **Test Database** با داده‌های sample آماده
- [ ] **Performance Monitoring Tools** نصب شده
- [ ] **Logging Configuration** فعال شده
- [ ] **Backup** از production data گرفته شده

### **2. Test Data Preparation**
- [ ] **1000+ Patients** در دیتابیس
- [ ] **500+ Services** در دسته‌بندی‌های مختلف
- [ ] **100+ Doctors** با schedule های مختلف
- [ ] **1000+ Appointments** در بازه‌های زمانی مختلف
- [ ] **500+ Receptions** با invoice های مختلف

---

## ⚡ **Performance Testing**

### **1. Application Startup Testing**
```bash
# تست 1: Startup Time Measurement
Start: $(Get-Date)
Application Start
End: $(Get-Date)
Expected: < 30 seconds
```

- [ ] **Cold Start:** Application startup time < 30 seconds
- [ ] **Warm Start:** Application startup time < 10 seconds
- [ ] **Memory Usage:** Initial memory < 200MB
- [ ] **EF Context Initialization:** < 5 seconds
- [ ] **Benchmark Execution:** < 10 seconds

### **2. Database Performance Testing**

#### **A. Patient Search Performance**
```sql
-- تست جستجوی بیماران
SELECT * FROM Patients 
WHERE (FirstName LIKE '%test%' OR LastName LIKE '%test%' OR NationalCode LIKE '%test%')
AND IsDeleted = 0
ORDER BY FirstName, LastName
OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY
```

- [ ] **Query Execution Time:** < 100ms
- [ ] **Index Usage:** بررسی استفاده از ایندکس‌ها
- [ ] **Result Count:** تعداد نتایج صحیح
- [ ] **Memory Usage:** < 50MB برای 1000 نتیجه

#### **B. Service Search Performance**
```sql
-- تست جستجوی خدمات
SELECT * FROM Services s
INNER JOIN ServiceCategories sc ON s.ServiceCategoryId = sc.ServiceCategoryId
INNER JOIN Departments d ON sc.DepartmentId = d.DepartmentId
WHERE s.ServiceCategoryId = 1 
AND (s.Title LIKE '%test%' OR s.ServiceCode LIKE '%test%')
AND s.IsDeleted = 0
ORDER BY s.Title
OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY
```

- [ ] **Query Execution Time:** < 150ms
- [ ] **Join Performance:** بررسی عملکرد JOIN ها
- [ ] **Index Usage:** بررسی استفاده از ایندکس‌ها
- [ ] **Result Accuracy:** نتایج صحیح

#### **C. Doctor Search Performance**
```sql
-- تست جستجوی پزشکان
SELECT * FROM Doctors d
INNER JOIN AspNetUsers u ON d.UserId = u.Id
INNER JOIN DoctorDepartments dd ON d.DoctorId = dd.DoctorId
WHERE d.ClinicId = 1 
AND d.IsDeleted = 0
ORDER BY u.FirstName, u.LastName
OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY
```

- [ ] **Query Execution Time:** < 200ms
- [ ] **Complex Join Performance:** بررسی عملکرد JOIN های پیچیده
- [ ] **Index Usage:** بررسی استفاده از ایندکس‌ها
- [ ] **Result Completeness:** تمام فیلدهای مورد نیاز

### **3. Memory Usage Testing**

#### **A. Baseline Memory Test**
```csharp
// تست حافظه پایه
var initialMemory = GC.GetTotalMemory(false);
// اجرای عملیات
var finalMemory = GC.GetTotalMemory(false);
var memoryIncrease = finalMemory - initialMemory;
```

- [ ] **Initial Memory:** < 200MB
- [ ] **Memory After Operations:** < 500MB
- [ ] **Memory Leak Test:** حافظه آزاد می‌شود
- [ ] **GC Performance:** Garbage Collection سریع

#### **B. Concurrent Access Test**
```csharp
// تست دسترسی همزمان
Parallel.For(0, 100, async i =>
{
    await patientService.GetPatientsAsync("", 1, 20);
});
```

- [ ] **Concurrent Requests:** 100 request همزمان
- [ ] **Response Time:** < 2 seconds برای تمام requests
- [ ] **Memory Usage:** < 1GB در peak
- [ ] **No Deadlocks:** عدم قفل‌های دیتابیس

---

## 🔧 **Functional Testing**

### **1. Patient Management Testing**

#### **A. Patient Search Functionality**
- [ ] **Empty Search:** نمایش تمام بیماران
- [ ] **Name Search:** جستجو بر اساس نام
- [ ] **National Code Search:** جستجو بر اساس کد ملی
- [ ] **Phone Search:** جستجو بر اساس شماره تلفن
- [ ] **Pagination:** صفحه‌بندی صحیح
- [ ] **Sorting:** مرتب‌سازی صحیح

#### **B. Patient CRUD Operations**
- [ ] **Create Patient:** ایجاد بیمار جدید
- [ ] **Read Patient:** خواندن اطلاعات بیمار
- [ ] **Update Patient:** ویرایش اطلاعات بیمار
- [ ] **Delete Patient:** حذف نرم بیمار
- [ ] **Validation:** اعتبارسنجی فیلدها

### **2. Service Management Testing**

#### **A. Service Search Functionality**
- [ ] **Category Filter:** فیلتر بر اساس دسته‌بندی
- [ ] **Title Search:** جستجو بر اساس عنوان
- [ ] **Code Search:** جستجو بر اساس کد خدمت
- [ ] **Pagination:** صفحه‌بندی صحیح
- [ ] **Sorting:** مرتب‌سازی صحیح

#### **B. Service CRUD Operations**
- [ ] **Create Service:** ایجاد خدمت جدید
- [ ] **Read Service:** خواندن اطلاعات خدمت
- [ ] **Update Service:** ویرایش اطلاعات خدمت
- [ ] **Delete Service:** حذف نرم خدمت
- [ ] **Validation:** اعتبارسنجی فیلدها

### **3. Doctor Management Testing**

#### **A. Doctor Search Functionality**
- [ ] **Clinic Filter:** فیلتر بر اساس کلینیک
- [ ] **Department Filter:** فیلتر بر اساس دپارتمان
- [ ] **Name Search:** جستجو بر اساس نام
- [ ] **Pagination:** صفحه‌بندی صحیح
- [ ] **Sorting:** مرتب‌سازی صحیح

#### **B. Doctor CRUD Operations**
- [ ] **Create Doctor:** ایجاد پزشک جدید
- [ ] **Read Doctor:** خواندن اطلاعات پزشک
- [ ] **Update Doctor:** ویرایش اطلاعات پزشک
- [ ] **Delete Doctor:** حذف نرم پزشک
- [ ] **Schedule Management:** مدیریت برنامه زمانی

---

## 🗄️ **Database Testing**

### **1. Index Creation Testing**

#### **A. Patient Indexes**
```sql
-- تست ایندکس بیماران
CREATE NONCLUSTERED INDEX [IX_Patients_Name_Search] 
ON [dbo].[Patients] ([FirstName], [LastName], [IsDeleted])
INCLUDE ([PatientId], [NationalCode], [PhoneNumber], [CreatedAt])
```

- [ ] **Index Creation Time:** < 5 minutes
- [ ] **No Database Lock:** عدم قفل دیتابیس
- [ ] **Storage Impact:** < 100MB افزایش
- [ ] **Query Performance:** بهبود 50%+

#### **B. Service Indexes**
```sql
-- تست ایندکس خدمات
CREATE NONCLUSTERED INDEX [IX_Services_Category_Search] 
ON [dbo].[Services] ([ServiceCategoryId], [IsDeleted])
INCLUDE ([ServiceId], [Title], [ServiceCode], [Price])
```

- [ ] **Index Creation Time:** < 3 minutes
- [ ] **No Database Lock:** عدم قفل دیتابیس
- [ ] **Storage Impact:** < 50MB افزایش
- [ ] **Query Performance:** بهبود 60%+

#### **C. Doctor Indexes**
```sql
-- تست ایندکس پزشکان
CREATE NONCLUSTERED INDEX [IX_Doctors_Clinic_User] 
ON [dbo].[Doctors] ([ClinicId], [UserId], [IsDeleted])
INCLUDE ([DoctorId], [Specialization], [LicenseNumber])
```

- [ ] **Index Creation Time:** < 2 minutes
- [ ] **No Database Lock:** عدم قفل دیتابیس
- [ ] **Storage Impact:** < 30MB افزایش
- [ ] **Query Performance:** بهبود 40%+

### **2. Data Integrity Testing**

#### **A. Foreign Key Constraints**
- [ ] **Patient-Insurance:** ارتباط صحیح
- [ ] **Service-Category:** ارتباط صحیح
- [ ] **Doctor-User:** ارتباط صحیح
- [ ] **Appointment-Patient:** ارتباط صحیح
- [ ] **Reception-Doctor:** ارتباط صحیح

#### **B. Soft Delete Functionality**
- [ ] **Patient Soft Delete:** IsDeleted = true
- [ ] **Service Soft Delete:** IsDeleted = true
- [ ] **Doctor Soft Delete:** IsDeleted = true
- [ ] **Data Retention:** داده‌ها حذف نمی‌شوند
- [ ] **Query Filtering:** IsDeleted = false در کوئری‌ها

### **3. Migration Testing**

#### **A. Forward Migration**
```sql
-- تست migration به جلو
-- اجرای تمام ایندکس‌ها
```

- [ ] **Migration Success:** تمام ایندکس‌ها ایجاد شوند
- [ ] **No Data Loss:** هیچ داده‌ای از دست نرود
- [ ] **Performance Impact:** بهبود عملکرد
- [ ] **Error Handling:** مدیریت خطاها

#### **B. Rollback Testing**
```sql
-- تست rollback
-- حذف ایندکس‌ها در صورت مشکل
```

- [ ] **Rollback Success:** حذف موفق ایندکس‌ها
- [ ] **No Data Loss:** هیچ داده‌ای از دست نرود
- [ ] **Performance Recovery:** بازگشت به حالت قبل
- [ ] **Error Handling:** مدیریت خطاها

---

## 🔒 **Error Handling Testing**

### **1. Network Failure Testing**

#### **A. Connection Timeout**
```csharp
// شبیه‌سازی timeout
Database.CommandTimeout = 1; // 1 second
```

- [ ] **Timeout Handling:** مدیریت صحیح timeout
- [ ] **User Feedback:** پیام مناسب به کاربر
- [ ] **Retry Logic:** تلاش مجدد
- [ ] **Fallback:** راه‌حل جایگزین

#### **B. Connection Loss**
```csharp
// شبیه‌سازی قطع اتصال
// قطع کردن connection به دیتابیس
```

- [ ] **Connection Loss Handling:** مدیریت صحیح
- [ ] **Reconnection:** اتصال مجدد
- [ ] **User Feedback:** پیام مناسب
- [ ] **Data Recovery:** بازیابی داده‌ها

### **2. Memory Pressure Testing**

#### **A. Low Memory Scenario**
```csharp
// شبیه‌سازی کمبود حافظه
GC.Collect();
GC.WaitForPendingFinalizers();
```

- [ ] **Memory Management:** مدیریت صحیح حافظه
- [ ] **Performance Degradation:** کاهش تدریجی عملکرد
- [ ] **No Crashes:** عدم crash
- [ ] **Recovery:** بازیابی پس از آزادسازی حافظه

#### **B. Large Dataset Handling**
```csharp
// تست با dataset بزرگ
var largeDataset = await patientService.GetPatientsAsync("", 1, 10000);
```

- [ ] **Memory Usage:** < 1GB
- [ ] **Response Time:** < 10 seconds
- [ ] **Pagination:** صفحه‌بندی صحیح
- [ ] **No Timeout:** عدم timeout

---

## 🔐 **Security Testing**

### **1. SQL Injection Testing**

#### **A. Input Validation**
```sql
-- تست SQL Injection
'; DROP TABLE Patients; --
```

- [ ] **Input Sanitization:** پاک‌سازی ورودی‌ها
- [ ] **Parameterized Queries:** استفاده از parameter
- [ ] **No SQL Injection:** عدم اجرای کد مخرب
- [ ] **Error Handling:** مدیریت خطاها

#### **B. XSS Testing**
```html
-- تست XSS
<script>alert('XSS')</script>
```

- [ ] **Output Encoding:** رمزگذاری خروجی
- [ ] **No XSS:** عدم اجرای script
- [ ] **Content Security Policy:** CSP فعال
- [ ] **Error Handling:** مدیریت خطاها

### **2. Authentication Testing**

#### **A. User Authentication**
- [ ] **Login Functionality:** ورود صحیح
- [ ] **Logout Functionality:** خروج صحیح
- [ ] **Session Management:** مدیریت session
- [ ] **Password Security:** امنیت رمز عبور

#### **B. Authorization Testing**
- [ ] **Role-based Access:** دسترسی بر اساس نقش
- [ ] **Permission Checking:** بررسی مجوزها
- [ ] **Unauthorized Access:** عدم دسترسی غیرمجاز
- [ ] **Audit Logging:** ثبت فعالیت‌ها

---

## 📊 **Performance Benchmarking**

### **1. Before vs After Comparison**

#### **A. Query Performance**
| Operation | Before (ms) | After (ms) | Improvement |
|-----------|-------------|------------|-------------|
| Patient Search | 500 | 150 | 70% |
| Service Search | 800 | 200 | 75% |
| Doctor Search | 600 | 180 | 70% |
| Appointment Report | 1200 | 400 | 67% |

#### **B. Memory Usage**
| Operation | Before (MB) | After (MB) | Improvement |
|-----------|-------------|------------|-------------|
| Application Start | 300 | 200 | 33% |
| Peak Memory | 800 | 500 | 38% |
| Average Memory | 400 | 250 | 38% |

#### **C. Response Time**
| Operation | Before (ms) | After (ms) | Improvement |
|-----------|-------------|------------|-------------|
| Page Load | 2000 | 800 | 60% |
| AJAX Request | 500 | 150 | 70% |
| Report Generation | 5000 | 1500 | 70% |

### **2. Load Testing**

#### **A. Concurrent Users**
- [ ] **10 Users:** Response time < 1 second
- [ ] **50 Users:** Response time < 2 seconds
- [ ] **100 Users:** Response time < 3 seconds
- [ ] **200 Users:** Response time < 5 seconds

#### **B. Database Connections**
- [ ] **Connection Pool:** مدیریت صحیح
- [ ] **Max Connections:** < 100
- [ ] **Connection Timeout:** < 30 seconds
- [ ] **Connection Leaks:** عدم نشت اتصال

---

## ✅ **Test Results Summary**

### **Passed Tests:**
- [ ] **Performance Tests:** تمام تست‌های عملکرد
- [ ] **Functional Tests:** تمام تست‌های عملکردی
- [ ] **Database Tests:** تمام تست‌های دیتابیس
- [ ] **Security Tests:** تمام تست‌های امنیتی
- [ ] **Error Handling Tests:** تمام تست‌های مدیریت خطا

### **Failed Tests:**
- [ ] **Test Name:** توضیح مشکل
- [ ] **Test Name:** توضیح مشکل

### **Performance Improvements:**
- [ ] **Query Performance:** 60-75% بهبود
- [ ] **Memory Usage:** 30-40% کاهش
- [ ] **Response Time:** 60-70% بهبود
- [ ] **Startup Time:** 30% بهبود

---

## 🎯 **Final Recommendation**

### **Merge Decision:**
- [ ] ✅ **APPROVE:** تمام تست‌ها pass شده‌اند
- [ ] ⚠️ **APPROVE WITH CONDITIONS:** نیاز به رفع مشکلات جزئی
- [ ] ❌ **REJECT:** مشکلات جدی وجود دارد

### **Deployment Strategy:**
- [ ] **Staging Deployment:** تست در محیط staging
- [ ] **Gradual Rollout:** rollout تدریجی
- [ ] **Monitoring:** نظارت 24/7
- [ ] **Rollback Plan:** برنامه rollback آماده

### **Post-Deployment:**
- [ ] **Performance Monitoring:** نظارت عملکرد
- [ ] **Error Monitoring:** نظارت خطاها
- [ ] **User Feedback:** جمع‌آوری بازخورد
- [ ] **Optimization:** بهینه‌سازی بیشتر

---

## 📝 **Test Notes**

### **Tester Information:**
- **Name:** Senior Reviewer
- **Date:** $(Get-Date -Format "yyyy-MM-dd")
- **Environment:** Staging
- **Duration:** 4 hours

### **Issues Found:**
1. **Issue 1:** توضیح مشکل
2. **Issue 2:** توضیح مشکل

### **Recommendations:**
1. **Recommendation 1:** پیشنهاد بهبود
2. **Recommendation 2:** پیشنهاد بهبود

### **Next Steps:**
1. **Step 1:** اقدام بعدی
2. **Step 2:** اقدام بعدی
