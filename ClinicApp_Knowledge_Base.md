# 📚 پایگاه دانش پروژه کلینیک شفا

## 🏥 **اطلاعات کلی پروژه**

### **نام پروژه**: ClinicApp - سیستم مدیریت کلینیک شفا
### **فناوری**: .NET MVC 5 + Entity Framework Code First
### **معماری**: Clean Architecture + Repository Pattern + Service Layer
### **پایگاه داده**: SQL Server
### **لاگ‌گیری**: Serilog
### **تزریق وابستگی**: Unity Container

---

## 🎯 **ماژول‌های بررسی شده**

### **1️⃣ ماژول کلینیک، دپارتمان، سرصفحه‌ها و خدمات** ✅
- **مدل‌های دیتابیس**: Clinic, Department, Service, ServiceCategory, ServiceComponent
- **کنترلرها**: ClinicController, DepartmentController, ServiceController
- **سرویس‌ها**: ClinicManagementService, DepartmentManagementService, ServiceManagementService
- **Repository ها**: ClinicRepository, DepartmentRepository, ServiceRepository
- **View ها**: Views/Clinic, Views/Department, Areas/Admin/Views/Service

### **2️⃣ ماژول دکتر و بیمار** ✅
- **مدل‌های دیتابیس**: Doctor, Patient, DoctorDepartment, DoctorServiceCategory
- **کنترلرها**: DoctorController, PatientController, ReceptionPatientController
- **سرویس‌ها**: DoctorCrudService, DoctorDashboardService, PatientService
- **Repository ها**: DoctorCrudRepository, DoctorDashboardRepository, PatientInsuranceRepository
- **View ها**: Views/Patient, Areas/Admin/Views/Doctor

### **3️⃣ ماژول سرویس و تعرفه‌ها** ✅
- **مدل‌های دیتابیس**: Service, ServiceCategory, ServiceComponent, InsuranceTariff
- **کنترلرها**: ReceptionServiceController, ServiceController, ServiceManagementController
- **سرویس‌ها**: ServiceCalculationService, ServiceManagementService, ServiceService
- **Repository ها**: ServiceRepository, ServiceCategoryRepository
- **View ها**: Views/Reception/Components, Areas/Admin/Views/Service

---

## 🏗️ **ساختار پروژه**

### **📁 فولدرهای اصلی:**
```
ClinicApp/
├── Models/                    # مدل‌های دیتابیس
│   ├── Entities/             # موجودیت‌های اصلی
│   │   ├── Clinic/           # مدل‌های کلینیک
│   │   ├── Doctor/           # مدل‌های دکتر
│   │   ├── Patient/          # مدل‌های بیمار
│   │   ├── Reception/        # مدل‌های پذیرش
│   │   ├── Insurance/        # مدل‌های بیمه
│   │   └── Payment/          # مدل‌های پرداخت
│   ├── Core/                 # مدل‌های هسته
│   └── Enums/                # شمارش‌ها
├── Controllers/              # کنترلرهای اصلی
├── Areas/Admin/              # ناحیه مدیریت
│   ├── Controllers/          # کنترلرهای مدیریت
│   └── Views/                # View های مدیریت
├── Services/                 # سرویس‌های کسب‌وکار
├── Repositories/             # Repository ها
├── Interfaces/               # Interface ها
├── Helpers/                  # کلاس‌های کمکی
├── Extensions/               # Extension ها
└── Views/                    # View های اصلی
```

---

## 🔧 **فولدرهای Helper و Extensions**

### **📁 Helpers:**
- **PersianDateHelper.cs**: مدیریت تاریخ شمسی
- **IranianNationalCodeValidator.cs**: اعتبارسنجی کد ملی
- **ServiceResult.cs**: مدیریت نتایج سرویس‌ها
- **PersianNumberHelper.cs**: تبدیل اعداد فارسی

### **📁 Extensions:**
- **DateTimeExtensions.cs**: Extension های تاریخ
- **PersianDateExtensions.cs**: Extension های تاریخ شمسی
- **CultureExtensions.cs**: Extension های فرهنگ

---

## 🌱 **کلاس‌های Seed**

### **📁 App_Start/DataSeeding:**
- **BaseSeedService.cs**: کلاس پایه Seeding
- **UserSeedService.cs**: Seeding کاربران
- **RoleSeedService.cs**: Seeding نقش‌ها
- **InsuranceSeedService.cs**: Seeding بیمه‌ها
- **SeedConstants.cs**: ثابت‌های Seeding

### **📁 Services/DataSeeding:**
- **SystemSeedService.cs**: Seeding سیستم
- **ServiceSeedService.cs**: Seeding خدمات
- **FactorSettingSeedService.cs**: Seeding تنظیمات

---

## 🏥 **مدل‌های کلیدی بررسی شده**

### **🏥 Doctor Entity:**
```csharp
public class Doctor : ISoftDelete, ITrackable
{
    public int DoctorId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string NationalCode { get; set; }
    public string MedicalCouncilCode { get; set; }
    public string LicenseNumber { get; set; }
    public int? ClinicId { get; set; }
    
    // روابط
    public virtual Clinic.Clinic Clinic { get; set; }
    public virtual ICollection<DoctorDepartment> DoctorDepartments { get; set; }
    public virtual ICollection<DoctorServiceCategory> DoctorServiceCategories { get; set; }
}
```

### **🏥 Patient Entity:**
```csharp
public class Patient : ISoftDelete, ITrackable
{
    public int PatientId { get; set; }
    public string PatientCode { get; set; }
    public string NationalCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string UserId { get; set; }
    
    // روابط
    public virtual ApplicationUser User { get; set; }
    public virtual ICollection<Reception.Reception> Receptions { get; set; }
    public virtual ICollection<PatientInsurance> PatientInsurances { get; set; }
}
```

### **🏥 Service Entity:**
```csharp
public class Service : ISoftDelete, ITrackable
{
    public int ServiceId { get; set; }
    public string ServiceCode { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    public bool IsHashtagged { get; set; }
    public int ServiceCategoryId { get; set; }
    
    // روابط
    public virtual ServiceCategory ServiceCategory { get; set; }
    public virtual ICollection<ServiceComponent> ServiceComponents { get; set; }
    public virtual ICollection<ReceptionItem> ReceptionItems { get; set; }
}
```

---

## 🔄 **روابط کلیدی بین موجودیت‌ها**

### **🏥 روابط Doctor:**
- **One-to-Many**: Doctor → Receptions
- **One-to-Many**: Doctor → Appointments
- **Many-to-Many**: Doctor ↔ Department (via DoctorDepartment)
- **Many-to-Many**: Doctor ↔ ServiceCategory (via DoctorServiceCategory)

### **🏥 روابط Patient:**
- **One-to-One**: Patient → ApplicationUser
- **One-to-Many**: Patient → Receptions
- **One-to-Many**: Patient → Appointments
- **One-to-Many**: Patient → PatientInsurances

### **🏥 روابط Service:**
- **Many-to-One**: Service → ServiceCategory
- **One-to-Many**: Service → ServiceComponents
- **One-to-Many**: Service → ReceptionItems
- **One-to-Many**: Service → InsuranceTariffs

---

## 🎨 **ویژگی‌های فنی کلیدی**

### **🔒 امنیت:**
- **Soft Delete**: ISoftDelete interface
- **Audit Trail**: ITrackable interface
- **Encryption**: EncryptionService برای داده‌های حساس
- **Anti-Forgery**: ValidateAntiForgeryTokenOnPostsAttribute

### **📊 لاگ‌گیری:**
- **Serilog**: لاگ‌گیری حرفه‌ای
- **Structured Logging**: با Emoji و Context
- **Error Handling**: مدیریت کامل خطاها

### **🌐 محلی‌سازی:**
- **Persian Date**: تبدیل تاریخ میلادی به شمسی
- **Persian Numbers**: تبدیل اعداد انگلیسی به فارسی
- **Culture Support**: پشتیبانی از فرهنگ فارسی

### **⚡ عملکرد:**
- **Lazy Loading**: بارگذاری تنبل
- **AsNoTracking**: بهینه‌سازی برای خواندن
- **Indexing**: ایندکس‌های بهینه
- **Caching**: کش برای داده‌های ثابت

---

## 📋 **TODO List - ماژول‌های باقی‌مانده**

### **4️⃣ ماژول بیمه و تعرفه‌ها** ✅
- **مدل‌های دیتابیس**: InsuranceProvider, InsurancePlan, InsuranceTariff, InsuranceCalculation, BusinessRule
- **کنترلرها**: InsuranceProviderController, InsurancePlanController, InsuranceTariffController, PatientInsuranceController
- **سرویس‌ها**: InsuranceProviderService, InsurancePlanService, InsuranceTariffService, PatientInsuranceService
- **Repository ها**: IInsuranceProviderRepository, IInsurancePlanRepository, IInsuranceTariffRepository
- **View ها**: Areas/Admin/Views/InsuranceProvider, Areas/Admin/Views/InsurancePlan, Areas/Admin/Views/InsuranceTariff

### **5️⃣ ماژول محاسبات بیمه‌ای** ✅
- **مدل‌های دیتابیس**: InsuranceCalculation, PatientInsurance
- **کنترلرها**: InsuranceCalculationController, CombinedInsuranceCalculationController
- **سرویس‌ها**: InsuranceCalculationService, CombinedInsuranceCalculationService, AdvancedInsuranceCalculationService, SupplementaryInsuranceService
- **Repository ها**: IInsuranceCalculationRepository
- **View ها**: Areas/Admin/Views/InsuranceCalculation, Areas/Admin/Views/CombinedInsuranceCalculation

### **🔄 ماژول‌های در انتظار بررسی:**
- [ ] **ماژول پذیرش (Reception)**
- [ ] **ماژول نوبت‌دهی (Appointment)**
- [ ] **ماژول پرداخت (Payment)**
- [ ] **ماژول تریاژ (Triage)**
- [ ] **ماژول گزارش‌گیری (Reporting)**
- [ ] **ماژول تنظیمات (Settings)**
- [ ] **ماژول امنیت (Security)**

---

## 🎯 **نکات مهم برای ادامه کار**

### **✅ اصول بررسی:**
1. **سیستماتیک**: از صفر تا صد
2. **کامل**: مدل، کنترلر، سرویس، Repository، View
3. **روابط**: بررسی روابط بین موجودیت‌ها
4. **ویژگی‌ها**: ویژگی‌های فنی و کسب‌وکار

### **✅ الگوی بررسی:**
1. **مدل‌های دیتابیس** → موجودیت‌ها و روابط
2. **کنترلرها** → منطق HTTP و UI
3. **سرویس‌ها** → منطق کسب‌وکار
4. **Repository ها** → دسترسی به داده
5. **View ها** → رابط کاربری
6. **روابط** → ارتباطات بین موجودیت‌ها

### **✅ نکات فنی:**
- **Clean Architecture**: رعایت اصول معماری
- **SOLID Principles**: اصول برنامه‌نویسی
- **Medical Standards**: استانداردهای پزشکی
- **Persian Support**: پشتیبانی فارسی
- **Security**: امنیت و حریم خصوصی

---

## 📝 **یادداشت‌های مهم**

### **🔍 نکات کشف شده:**
- سیستم از **Soft Delete** استفاده می‌کند
- **Audit Trail** کامل برای تمام موجودیت‌ها
- **Persian Date** و **Persian Numbers** پشتیبانی می‌شود
- **Serilog** برای لاگ‌گیری حرفه‌ای
- **Unity Container** برای Dependency Injection
- **FluentValidation** برای اعتبارسنجی
- **ServiceResult** pattern برای مدیریت نتایج

### **⚠️ نکات احتیاط:**
- برخی فایل‌ها بسیار بزرگ هستند (بیش از 25000 token)
- از `offset` و `limit` برای خواندن فایل‌های بزرگ استفاده کنید
- برخی Repository ها ممکن است در فولدرهای مختلف باشند
- Interface ها و Implementation ها ممکن است جدا باشند

---

## 🚀 **آماده برای ادامه**

پایگاه دانش آماده است! می‌توانیم به بررسی ماژول‌های باقی‌مانده ادامه دهیم. 

**ماژول بعدی پیشنهادی**: ماژول پذیرش (Reception) - قلب تپنده سیستم کلینیک
