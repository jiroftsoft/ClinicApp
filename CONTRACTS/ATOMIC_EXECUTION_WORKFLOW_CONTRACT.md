# ⚛️ **قرارداد روند اجرایی اتمیک (Atomic Execution Workflow Contract) - ClinicApp**

## 🎯 **هدف:**
این قرارداد شامل روند اجباری برای اجرای تغییرات به‌صورت اتمیک، قابل‌ردگیری و قابل‌برگشت در پروژه ClinicApp است.

---

## 🚨 **قانون اجباری:**
**تمام تغییرات باید طبق این روند 9 مرحله‌ای اجرا شوند.**

---

## **لیست B — روند اجرایی اتمیک (Atomic Execution Workflow)**

### **گام 0 — شروع (Produce Plan Header)**

#### **0.1 تولید شناسه تغییر یکتا:**
```markdown
شناسه: change-YYYYMMDD-xxxx
مثال: change-20250104-0001
```

#### **0.2 تعیین نوع کار:**
- `analyze` - تحلیل و بررسی
- `propose` - پیشنهاد تغییر
- `patch` - اعمال تغییر

#### **0.3 ارائه خلاصه یک‌خطی:**
```markdown
هدف: [توضیح کوتاه هدف]
دامنه: [فایل/ماژول مورد نظر]
```

### **گام 1 — جستجوی نهایی (Verify No-Duplicate)**

#### **1.1 جستجوی کامل برای identifiers:**
```bash
# جستجوهای اجباری:
grep -r "class.*IdentifierName" .
grep -r "method.*IdentifierName" .
grep -r "view.*IdentifierName" .
grep -r "route.*IdentifierName" .
```

#### **1.2 اگر مشابه یافت شد:**
- [ ] فورا برگرد
- [ ] پیشنهاد بازاستفاده/رفکتور بده
- [ ] مسیرهای موجود را ذکر کن

#### **1.3 اگر مشابه یافت نشد:**
- [ ] ذکر صریح شواهد
- [ ] تایم‌استمپ جستجو
- [ ] نتایج grep/rg

### **گام 2 — پیشنهاد کوچک اتمی**

#### **2.1 اگر نیاز به افزودن است:**
- [ ] پیشنهاد کوچک‌ترین واحد ممکن
- [ ] single method یا small helper file
- [ ] single ViewModel property

#### **2.2 هر پیشنهاد باید شامل:**
- [ ] **عنوان کوتاه**
- [ ] **توجیه فنی/بیزینسی**
- [ ] **مسیر هدف دقیق** (مثال: `Areas/Admin/Controllers/DoctorDepartmentController.cs`)
- [ ] **diff_unified کامل** (حداکثر 200 خط)
- [ ] **ورودی/خروجی توضیحی** (signature)
- [ ] **تست‌های پیشنهادی** (نام و هدف)

### **گام 3 — ارزیابی ریسک و rollback**

#### **3.1 هر پیشنهاد باید شامل:**
- [ ] **بخش ریسک** (کوتاه)
- [ ] **راه برگشت** شامل فرمان git نمونه

#### **3.2 مثال‌های rollback:**
```bash
# برگشت commit
git revert <commit-hash>

# برگشت فایل
git checkout HEAD~1 -- path/to/file

# برگشت branch
git checkout main
git branch -D feature-branch
```

### **گام 4 — تست‌های لازم (Required Tests)**

#### **4.1 پیشنهاد حداقل:**
- [ ] **یک Unit Test** برای منطق جدید
- [ ] **یک Integration Test** برای سناریوی انتها-به-انتها

#### **4.2 اگر تغییر DB دارد:**
- [ ] Migration test
- [ ] Seed test

#### **4.3 فرمت تست‌ها:**
```csharp
[Test]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    // Act
    // Assert
}
```

### **گام 5 — ارائه شواهد و درخواست تایید انسانی**

#### **5.1 جمع‌کردن همه شواهد:**
- [ ] نتایج جستجو
- [ ] diff
- [ ] تست‌ها
- [ ] ریسک
- [ ] rollback

#### **5.2 ارسال پیام آماده:**
```markdown
## 🚨 درخواست تایید انسانی

برای اعمال change-id: change-YYYYMMDD-xxxx لطفاً عبارت زیر را بنویسید:

**I APPROVE APPLY**
```

### **گام 6 — پس از تأیید (apply plan)**

#### **6.1 پس از دریافت عبارت تأیید:**
- [ ] تولید دقیق مجموعه patch (unified diff)
- [ ] ارائه دستورالعمل‌های گام‌به‌گام برای اجرا

#### **6.2 دستورالعمل‌های اجرا:**
```bash
# اعمال patch
git apply change-YYYYMMDD-xxxx.patch

# commit تغییرات
git add .
git commit -m "Apply change-YYYYMMDD-xxxx: [توضیح کوتاه]"

# ایجاد PR (اختیاری)
git push origin feature-branch
```

#### **6.3 محدودیت‌های AI:**
- [ ] AI هرگز خودش push نکند
- [ ] AI هرگز merge نکند
- [ ] AI تنها patch و دستورالعمل تحویل دهد

### **گام 7 — اعتبارسنجی پس از اعمال**

#### **7.1 دستورالعمل اجرای تست‌ها:**
```bash
# اجرای Unit Tests
dotnet test --filter "Category=Unit"

# اجرای Integration Tests
dotnet test --filter "Category=Integration"

# اجرای Smoke Tests
dotnet test --filter "Category=Smoke"
```

#### **7.2 چک‌لیست اعتبارسنجی:**
- [ ] compile success
- [ ] unit tests passed
- [ ] integration tests passed
- [ ] smoke test on key flows

### **گام 8 — به‌روزرسانی پایگاه دانش و مستندات**

#### **8.1 پس از اعمال و تست موفق:**
- [ ] تولید entry قابل ذخیره در KB

#### **8.2 KB entry باید شامل:**
- [ ] چه تغییر کرد (change-id)
- [ ] فایل‌ها و مسیرها
- [ ] خلاصه تاثیر بر معماری
- [ ] نمونه درخواست/پاسخ (برای API)
- [ ] نام تست‌ها و نتایج

### **گام 9 — گزارش نهایی و پیشنهاد گام بعدی**

#### **9.1 گزارش 1-پاراگرافی:**
- [ ] نتیجه تغییر
- [ ] پیشنهاد گام بعدی
- [ ] بهبودهای مرتبط
- [ ] refactorهای آینده

---

## **📏 قواعد سخت (Constraints — الزام‌آور)**

### **1. محدودیت‌های فایل:**
- [ ] حداکثر فایل در هر گام: **1**
- [ ] حداکثر خطوط پچ اتمی: **200 خط**

### **2. فرمت پچ:**
- [ ] unified diff یا فایل کامل (در قالب کد)

### **3. الزامات جستجو:**
- [ ] الزام به ارائه search-evidence برای هر Identifier جدید

### **4. الزامات تست:**
- [ ] هر تغییر بدون تست پیشنهادی پذیرفته نیست

### **5. تایید انسانی:**
- [ ] عبارت تأیید: **"I APPROVE APPLY"**
- [ ] فقط پس از این عبارت، پیشنهاد patch برای اجرا ارائه می‌شود

### **6. ممنوعیت‌های AI:**
- [ ] ممنوعیت خودکار push/merge/deploy توسط AI

---

## **📊 فرمت‌های استاندارد**

### **فرمت شناسه تغییر:**
```markdown
change-YYYYMMDD-xxxx
مثال: change-20250104-0001
```

### **فرمت diff:**
```diff
--- a/path/to/file.cs
+++ b/path/to/file.cs
@@ -line,count +line,count @@
 context line
-old line
+new line
 context line
```

### **فرمت تست:**
```csharp
[Test]
[Category("Unit")]
public void ClassName_MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var sut = new ClassName();
    
    // Act
    var result = sut.MethodName(input);
    
    // Assert
    Assert.AreEqual(expected, result);
}
```

---

## **🚫 ممنوعیت‌های مطلق:**

### **1. تغییرات غیراتمیک:**
```markdown
❌ ممنوع - تغییرات گسترده یکجا
❌ ممنوع - تغییر چندین فایل همزمان
❌ ممنوع - تغییرات بدون تست
```

### **2. عدم رعایت روند:**
```markdown
❌ ممنوع - رد شدن از مراحل
❌ ممنوع - عدم جستجوی کامل
❌ ممنوع - عدم ارزیابی ریسک
```

### **3. خودکارسازی غیرمجاز:**
```markdown
❌ ممنوع - push خودکار
❌ ممنوع - merge خودکار
❌ ممنوع - deploy خودکار
```

---

## **✅ استانداردهای اجباری:**

### **1. اتمیک بودن:**
- هر تغییر باید کوچک و مستقل باشد
- قابل تست و برگشت باشد
- تاثیر محدود داشته باشد

### **2. قابل‌ردگیری بودن:**
- شناسه یکتا برای هر تغییر
- مستندسازی کامل مراحل
- شواهد کافی برای تصمیم‌گیری

### **3. قابل‌برگشت بودن:**
- راه‌های برگشت مشخص
- دستورات git آماده
- تست‌های rollback

---

## **📝 مثال عملی:**

### **ورودی:**
```markdown
کاربر: "اضافه کردن متد جدید به DoctorServiceCategoryService"
```

### **خروجی:**
```markdown
## گام 0 - شروع
شناسه: change-20250104-0001
نوع: propose
هدف: اضافه کردن متد GetDoctorServiceCategoryByIdAsync

## گام 1 - جستجوی نهایی
✅ هیچ متد مشابه یافت نشد

## گام 2 - پیشنهاد کوچک اتمی
- عنوان: اضافه کردن متد GetDoctorServiceCategoryByIdAsync
- مسیر: Services/ClinicAdmin/DoctorServiceCategoryService.cs
- diff: [200 خط diff]

## گام 3 - ارزیابی ریسک
ریسک: کم - اضافه کردن متد جدید
rollback: git checkout HEAD~1 -- Services/ClinicAdmin/DoctorServiceCategoryService.cs

## گام 4 - تست‌های لازم
- Unit Test: GetDoctorServiceCategoryByIdAsync_ValidId_ReturnsServiceCategory
- Integration Test: DoctorServiceCategoryService_GetById_EndToEnd

## گام 5 - درخواست تایید
🚨 برای اعمال change-20250104-0001 لطفاً عبارت زیر را بنویسید:
**I APPROVE APPLY**
```

---

## **🔄 به‌روزرسانی قرارداد:**

این قرارداد باید در صورت:
- تغییر در ساختار پروژه
- اضافه شدن ابزارهای جدید
- شناسایی نقاط ضعف در روند

به‌روزرسانی شود.

---

**نسخه**: 1.0  
**تاریخ ایجاد**: 2025-01-04  
**آخرین به‌روزرسانی**: 2025-01-04  
**وضعیت**: فعال
