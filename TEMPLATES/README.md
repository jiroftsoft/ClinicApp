# **📋 Templates برای فرم‌های پیچیده**

## **🎯 هدف**
این Templates برای ایجاد فرم‌های پیچیده با منطق پیشرفته، اعتبارسنجی کامل، و تجربه کاربری حرفه‌ای طراحی شده‌اند.

---

## **📁 فایل‌های موجود**

### **1. 🎨 Frontend Templates**
- **`ComplexFormTemplate.cshtml`** - Template کامل HTML برای فرم‌های پیچیده
- **`ComplexFormJavaScript.js`** - JavaScript استاندارد با لاگ‌گذاری حرفه‌ای
- **`ComplexFormCSS.css`** - CSS کامل با تم پزشکی و طراحی واکنش‌گرا

### **2. 🔧 Backend Templates**
- **`ComplexFormViewModel.cs`** - ViewModel کامل با اعتبارسنجی پیشرفته
- **`ComplexFormController.cs`** - Controller استاندارد با مدیریت خطا
- **`ComplexFormService.cs`** - Service layer با منطق کسب‌وکار
- **`ComplexFormRepository.cs`** - Repository pattern با کوئری‌های بهینه
- **`ComplexFormEntity.cs`** - Entity model با ویژگی‌های کامل

### **3. 📚 Documentation**
- **`COMPLEX_FORM_STANDARDS_CONTRACT.md`** - استانداردهای جامع
- **`FORM_DEVELOPMENT_CHECKLIST.md`** - چک لیست عملی توسعه

---

## **🚀 نحوه استفاده**

### **مرحله 1: کپی کردن Templates**
```bash
# کپی کردن فایل‌های Template
cp TEMPLATES/ComplexFormTemplate.cshtml Areas/Admin/Views/[Module]/
cp TEMPLATES/ComplexFormJavaScript.js Scripts/[module]-form.js
cp TEMPLATES/ComplexFormCSS.css Content/css/[module]-form.css
cp TEMPLATES/ComplexFormViewModel.cs ViewModels/[Module]/
cp TEMPLATES/ComplexFormController.cs Areas/Admin/Controllers/
cp TEMPLATES/ComplexFormService.cs Services/
cp TEMPLATES/ComplexFormRepository.cs Repositories/
cp TEMPLATES/ComplexFormEntity.cs Models/Entities/[Module]/
```

### **مرحله 2: جایگزینی Placeholders**
در تمام فایل‌ها، `[Module]` را با نام ماژول خود جایگزین کنید:

```bash
# مثال: برای ماژول InsurancePlan
sed -i 's/\[Module\]/InsurancePlan/g' Areas/Admin/Views/InsurancePlan/ComplexFormTemplate.cshtml
sed -i 's/\[Module\]/InsurancePlan/g' ViewModels/InsurancePlan/ComplexFormViewModel.cs
# ... و سایر فایل‌ها
```

### **مرحله 3: تنظیمات خاص**
- **Entity Model**: فیلدهای خاص ماژول خود را اضافه/حذف کنید
- **ViewModel**: ویژگی‌های مورد نیاز را تنظیم کنید
- **Service**: منطق کسب‌وکار خاص را پیاده‌سازی کنید
- **Repository**: کوئری‌های خاص را اضافه کنید

---

## **🔧 ویژگی‌های کلیدی**

### **1. 📅 مدیریت تاریخ شمسی**
- ✅ **Persian DatePicker** با تنظیمات کامل
- ✅ **اعتبارسنجی فرمت** تاریخ شمسی
- ✅ **تبدیل خودکار** شمسی به میلادی
- ✅ **اعتبارسنجی بازه زمانی**

### **2. 📝 لاگ‌گذاری حرفه‌ای**
- ✅ **Console Logging** با emoji و timestamp
- ✅ **Server-side Logging** با Serilog
- ✅ **AJAX Request Logging** کامل
- ✅ **Error Tracking** پیشرفته

### **3. 🛡️ مقاوم‌سازی در برابر خطا**
- ✅ **Global Error Handler** برای JavaScript
- ✅ **Try-Catch** کامل در C#
- ✅ **Validation** چندلایه
- ✅ **Graceful Degradation**

### **4. ⚡ بهینه‌سازی عملکرد**
- ✅ **Debouncing** برای AJAX calls
- ✅ **Caching** برای داده‌های lookup
- ✅ **Lazy Loading** برای کامپوننت‌های سنگین
- ✅ **Query Optimization** در Repository

### **5. 🎨 طراحی UI تمیز**
- ✅ **Medical Theme** مناسب محیط درمانی
- ✅ **Responsive Design** برای تمام دستگاه‌ها
- ✅ **Accessibility** رعایت استانداردهای دسترسی
- ✅ **Dark Mode Support** پشتیبانی حالت تاریک

---

## **📋 چک لیست استفاده**

### **قبل از شروع**
- [ ] **تحلیل نیازمندی‌ها** انجام شده
- [ ] **طراحی معماری** تعریف شده
- [ ] **Templates** کپی شده‌اند
- [ ] **Placeholders** جایگزین شده‌اند

### **توسعه Backend**
- [ ] **Entity Model** تنظیم شده
- [ ] **Repository** پیاده‌سازی شده
- [ ] **Service** منطق کسب‌وکار اضافه شده
- [ ] **Controller** Actions پیاده‌سازی شده
- [ ] **ViewModel** ویژگی‌ها تنظیم شده

### **توسعه Frontend**
- [ ] **HTML Structure** تنظیم شده
- [ ] **CSS Styling** شخصی‌سازی شده
- [ ] **JavaScript** منطق خاص اضافه شده
- [ ] **Validation** client-side پیاده‌سازی شده
- [ ] **AJAX Calls** تنظیم شده

### **تست و اعتبارسنجی**
- [ ] **Unit Tests** نوشته شده
- [ ] **Integration Tests** انجام شده
- [ ] **UI Tests** انجام شده
- [ ] **Performance Tests** انجام شده
- [ ] **Security Tests** انجام شده

---

## **🎯 مثال عملی**

### **ایجاد ماژول InsurancePlan**

#### **1. کپی Templates**
```bash
# کپی فایل‌ها
cp TEMPLATES/ComplexFormTemplate.cshtml Areas/Admin/Views/InsurancePlan/Create.cshtml
cp TEMPLATES/ComplexFormTemplate.cshtml Areas/Admin/Views/InsurancePlan/Edit.cshtml
cp TEMPLATES/ComplexFormViewModel.cs ViewModels/Insurance/InsurancePlan/InsurancePlanCreateEditViewModel.cs
cp TEMPLATES/ComplexFormController.cs Areas/Admin/Controllers/Insurance/InsurancePlanController.cs
cp TEMPLATES/ComplexFormService.cs Services/Insurance/InsurancePlanService.cs
cp TEMPLATES/ComplexFormRepository.cs Repositories/InsurancePlanRepository.cs
cp TEMPLATES/ComplexFormEntity.cs Models/Entities/Insurance/InsurancePlan.cs
```

#### **2. جایگزینی Placeholders**
```bash
# جایگزینی [Module] با InsurancePlan
find . -name "*.cs" -o -name "*.cshtml" | xargs sed -i 's/\[Module\]/InsurancePlan/g'
find . -name "*.cs" -o -name "*.cshtml" | xargs sed -i 's/\[module\]/insuranceplan/g'
```

#### **3. تنظیمات خاص**
```csharp
// در InsurancePlanCreateEditViewModel.cs
public class InsurancePlanCreateEditViewModel
{
    // فیلدهای خاص InsurancePlan
    public int InsuranceProviderId { get; set; }
    public decimal CoveragePercent { get; set; }
    public decimal Deductible { get; set; }
    
    // ... سایر فیلدها
}
```

#### **4. تنظیمات JavaScript**
```javascript
// در insuranceplan-form.js
const FormConfig = {
    moduleName: 'InsurancePlan',
    ajaxTimeout: 30000,
    // ... سایر تنظیمات
};
```

---

## **🔍 عیب‌یابی**

### **مشکلات رایج**

#### **1. خطای Compilation**
```bash
# بررسی using statements
# بررسی namespace ها
# بررسی dependency injection
```

#### **2. خطای JavaScript**
```javascript
// بررسی console logs
// بررسی AJAX calls
// بررسی validation
```

#### **3. خطای CSS**
```css
/* بررسی CSS variables */
/* بررسی responsive design */
/* بررسی browser compatibility */
```

---

## **📚 منابع بیشتر**

### **مستندات**
- **`COMPLEX_FORM_STANDARDS_CONTRACT.md`** - استانداردهای جامع
- **`FORM_DEVELOPMENT_CHECKLIST.md`** - چک لیست عملی

### **مثال‌های موجود**
- **`InsurancePlan`** - مثال کامل پیاده‌سازی شده
- **`Reception`** - مثال فرم پیچیده با cascade loading

### **ابزارهای مفید**
- **Persian DatePicker** - انتخابگر تاریخ شمسی
- **Serilog** - لاگ‌گذاری server-side
- **Bootstrap** - فریمورک CSS
- **jQuery** - کتابخانه JavaScript

---

## **🎯 نتیجه‌گیری**

این Templates یک راه‌حل کامل و استاندارد برای ایجاد فرم‌های پیچیده ارائه می‌دهند که شامل:

1. **📅 مدیریت کامل تاریخ شمسی**
2. **📝 لاگ‌گذاری حرفه‌ای**
3. **🛡️ مقاوم‌سازی در برابر خطا**
4. **⚡ بهینه‌سازی عملکرد**
5. **🎨 طراحی UI تمیز**
6. **✅ چک لیست کامل**

با استفاده از این Templates، فرم‌های پیچیده به صورت حرفه‌ای، امن، و قابل نگهداری پیاده‌سازی خواهند شد.

---

**📅 تاریخ ایجاد**: 1404/06/23  
**👤 ایجادکننده**: AI Assistant  
**🔄 نسخه**: 1.0  
**📋 وضعیت**: فعال