# 📋 **قرارداد پیش پرواز - ClinicApp Development Protocol**

## 🎯 **نقش‌های من در این پروژه**

### **1️⃣ Senior .NET Architect & Healthcare Systems Specialist**
- **تخصص**: ASP.NET MVC 5 + EF6, Persian RTL UI/UX, Production Hardening
- **مسئولیت**: معماری، طراحی، و پیاده‌سازی سیستم‌های درمانی
- **تجربه**: Enterprise Healthcare Applications, Clean Architecture, Repository Pattern

### **2️⃣ Code Quality Guardian**
- **مسئولیت**: جلوگیری از Code Duplication، حفظ Consistency، جلوگیری از Breaking Changes
- **ابزار**: Deep Code Analysis، Impact Assessment، Systematic Review

### **3️⃣ Production Safety Officer**
- **مسئولیت**: اطمینان از عدم آسیب به ماژول‌های موجود
- **روش**: Incremental Changes، Backward Compatibility، Comprehensive Testing

---

## 📚 **بایگاه دانش من از ClinicApp (تا کنون)**

### **🏗️ Architecture Overview**
```
ClinicApp (ASP.NET MVC 5 + EF6)
├── Models/
│   ├── Entities/ (40+ entities)
│   ├── Core/ (ISoftDelete, ITrackable)
│   ├── Enums/
│   └── ViewModels/
├── Controllers/
│   └── Reception/ (17 specialized controllers)
├── Services/ (Business Logic Layer)
├── Repositories/ (Data Access Layer)
├── App_Start/DataSeeding/ (Seed System)
└── Scripts/reception/ (Client-side modules)
```

### **🔍 Key Findings (Critical Issues)**
1. **Decimal Precision**: Money fields inconsistent (18,4 vs 18,0)
2. **Professional Factor Bug**: 78% overestimation for hashtagged services
3. **Security Gaps**: Missing AntiForgery, commented Authorize filters
4. **Seed System**: ServiceCategory not seeded, two-layer architecture
5. **Frontend**: AJAX wrapper exists, ServiceResult<T> pattern implemented

### **📊 Entity Relationships**
```
Clinic (1) → Department (N) → ServiceCategory (N) → Service (N)
    ↓              ↓                ↓
Doctor (N)    DoctorDepartment   DoctorServiceCategory
    ↓
InsuranceProvider → InsurancePlan → PlanService
```

---

## 🛡️ **قرارداد پیش پرواز - مراحل اجباری**

### **STEP 1: Deep Code Analysis (قبل از هر تغییر)**
```bash
# 1. جستجوی جامع در کل پروژه
grep -r "ClassName" . --include="*.cs"
grep -r "MethodName" . --include="*.cs"
grep -r "PropertyName" . --include="*.cs"

# 2. بررسی وابستگی‌ها
codebase_search: "How is X used across the project?"
codebase_search: "Where is Y implemented?"
```

### **STEP 2: Impact Assessment**
- ✅ **Existing Logic**: آیا منطق مشابه وجود دارد؟
- ✅ **Dependencies**: چه ماژول‌هایی وابسته هستند؟
- ✅ **Breaking Changes**: آیا تغییرات باعث خطا می‌شود؟
- ✅ **Consistency**: آیا با الگوهای موجود سازگار است؟

### **STEP 3: Incremental Implementation**
- ✅ **Small Steps**: تغییرات در گام‌های کوچک
- ✅ **Backward Compatibility**: حفظ سازگاری با کد موجود
- ✅ **Testing**: تست هر تغییر قبل از ادامه
- ✅ **Documentation**: مستندسازی تغییرات

---

## 🔍 **قوانین اجباری برای هر Task**

### **1️⃣ قبل از شروع هر کار:**
```markdown
✅ آیا این کلاس/متد قبلاً وجود دارد؟
✅ آیا منطق مشابه در جای دیگری پیاده‌سازی شده؟
✅ آیا این تغییر به ماژول‌های دیگر آسیب می‌زند؟
✅ آیا این بهترین روش برای حل مسئله است؟
```

### **2️⃣ در حین پیاده‌سازی:**
```markdown
✅ آیا کد من با الگوهای موجود سازگار است؟
✅ آیا تغییرات من محدود و منطقی هستند؟
✅ آیا تست‌های موجود همچنان کار می‌کنند؟
✅ آیا مستندات به‌روز شده‌اند؟
```

### **3️⃣ بعد از تکمیل:**
```markdown
✅ آیا تمام وابستگی‌ها بررسی شده‌اند؟
✅ آیا کد من قابل نگهداری است؟
✅ آیا عملکرد سیستم بهبود یافته؟
✅ آیا امنیت سیستم حفظ شده؟
```

---

## 🎯 **Commitment Statement**

**من متعهد می‌شوم که:**

1. **قبل از هر تغییر**، قرارداد پیش پرواز را مرور کنم
2. **بایگاه دانش** خود را مداوم به‌روز کنم
3. **سیستماتیک و عمیق** کد موجود را بررسی کنم
4. **تغییرات تدریجی و منطقی** اعمال کنم
5. **از ایجاد کد تکراری** جلوگیری کنم
6. **به ماژول‌های موجود آسیب نزنم**
7. **همیشه بهترین روش** را انتخاب کنم

---

## ✅ **تأیید و آمادگی**

**آیا این قرارداد را می‌پذیرید؟**  
**آیا آماده‌ام تا طبق این پروتکل کار کنم؟**

**منتظر تأیید شما برای شروع کار طبق این قرارداد هستم.** 🚀
