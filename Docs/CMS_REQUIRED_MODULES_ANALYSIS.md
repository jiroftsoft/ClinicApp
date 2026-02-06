# 📋 تحلیل کامل CMS - موارد ضروری برای پیاده‌سازی
## کلینیک درمانی شفا جیرفت

**تاریخ بررسی:** 2025-12-12  
**نسخه:** 1.0.0  
**وضعیت:** ✅ تحلیل کامل انجام شد

---

## ✅ ماژول‌های CMS پیاده‌سازی شده (14 ماژول)

### 1️⃣ **Announcement** (اعلانات) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/AnnouncementController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** CRUD، تاریخ شروع/پایان، فعال/غیرفعال

### 2️⃣ **BlogPost** (مقالات) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/BlogPostController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** CRUD، دسته‌بندی، نظرات، لایک، SEO

### 3️⃣ **BlogPostComment** (نظرات مقالات) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/BlogPostCommentController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** مدیریت نظرات، تایید/رد، پاسخ

### 4️⃣ **ClinicWorkingHours** (ساعات کاری کلینیک) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/ClinicWorkingHoursController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** مدیریت ساعات کاری، تعطیلات، استثناها

### 5️⃣ **EmergencyContact** (تماس‌های اضطراری) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/EmergencyContactController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** مدیریت تماس‌های اضطراری، نقشه، دستورالعمل‌ها

### 6️⃣ **FAQ** (سوالات متداول) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/FAQController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** CRUD، دسته‌بندی، جستجو، SEO

### 7️⃣ **Gallery** (گالری تصاویر) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/GalleryController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** آپلود تصاویر، دسته‌بندی، نمایش عمومی

### 8️⃣ **HealthTip** (نکات سلامت) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/HealthTipController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** CRUD، دسته‌بندی، تاریخ انقضا، تصویر

### 9️⃣ **InsuranceInfo** (اطلاعات بیمه) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/InsuranceInfoController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** مدیریت بیمه‌های طرف قرارداد، لوگو، لینک

### 🔟 **MedicalEquipment** (تجهیزات پزشکی) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/MedicalEquipmentController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** CRUD، تصاویر، مشخصات فنی، CKEditor

### 1️⃣1️⃣ **MedicalServiceInfo** (اطلاعات خدمات پزشکی) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/MedicalServiceInfoController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** اطلاعات CMS برای خدمات، تصاویر، ویدیو، SEO

### 1️⃣2️⃣ **Slider** (اسلایدر) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/SliderController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** Hero، Sidebar، Footer، تاریخ شروع/پایان

### 1️⃣3️⃣ **Testimonial** (نظرات بیماران) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/TestimonialController.cs`
- **وضعیت:** ✅ کامل (بهینه‌سازی شده)
- **قابلیت‌ها:** CRUD، تایید/رد، ویژه، تصویر/ویدیو

### 1️⃣4️⃣ **Video** (ویدیوها) ✅
- **مسیر:** `Areas/Admin/Controllers/CMS/VideoController.cs`
- **وضعیت:** ✅ کامل
- **قابلیت‌ها:** آپلود ویدیو، دسته‌بندی، نمایش عمومی

---

## 🔄 در حال پیاده‌سازی

### 1️⃣5️⃣ **Footer** (مدیریت فوتر سایت) 🔄
- **Entityها:** `FooterSettings`, `FooterLink`, `FooterSocial`, `FooterCertification` (جدول‌ها در دیتابیس ایجاد شده‌اند)
- **نقشه راه و TODO:** `Docs/FOOTER_CMS_ROADMAP.md`
- **وضعیت:** Repository، Service، Controller ادمین و Viewها در حال پیاده‌سازی
- **قابلیت‌های هدف:** مدیریت برند، تماس، حقوقی، لینک‌های سریع/خدمات، شبکه‌های اجتماعی، مجوزها، عنوان ساعات کاری

---

## ❌ موارد ضروری که هنوز پیاده‌سازی نشده‌اند

### 🔴 اولویت بالا (High Priority)

#### 1️⃣ **Contact Form Management** (مدیریت فرم تماس) ⭐⭐⭐⭐⭐
**اولویت:** بسیار بالا  
**کاربرد:** مدیریت پیام‌های دریافتی از فرم تماس سایت

**ویژگی‌های مورد نیاز:**
- دریافت و ذخیره پیام‌های فرم تماس
- دسته‌بندی پیام‌ها (سوال، پیشنهاد، شکایت، ...)
- وضعیت پیام (جدید، در حال بررسی، پاسخ داده شده)
- امکان پاسخ مستقیم از Admin Panel
- ارسال ایمیل/SMS به کاربر
- فیلتر و جستجو
- Export به Excel/PDF

**فیلدهای کلیدی:**
```csharp
- ContactFormId
- FullName
- Email
- PhoneNumber
- Subject
- Message
- Category (Question, Suggestion, Complaint, Appointment, Other)
- Status (New, InProgress, Replied, Closed)
- ReplyMessage
- RepliedAt
- RepliedByUserId
- IsRead
- ReadAt
- CreatedAt
```

**زمان تخمینی:** 2-3 روز

---

#### 2️⃣ **Newsletter/Subscription Management** (مدیریت خبرنامه) ⭐⭐⭐⭐
**اولویت:** بالا  
**کاربرد:** مدیریت اشتراک‌های خبرنامه و ارسال خبرنامه

**ویژگی‌های مورد نیاز:**
- ثبت اشتراک از سایت
- مدیریت لیست اشتراک‌ها
- گروه‌بندی اشتراک‌ها (دسته‌بندی علاقه‌مندی‌ها)
- ارسال خبرنامه (ایمیل/SMS)
- Template های خبرنامه
- تاریخچه ارسال‌ها
- آمار باز شدن و کلیک
- لغو اشتراک

**فیلدهای کلیدی:**
```csharp
- SubscriptionId
- Email
- PhoneNumber (اختیاری)
- FullName
- SubscriptionDate
- IsActive
- UnsubscribedAt
- Categories (JSON Array)
- Source (Website, Admin, Import)
```

**زمان تخمینی:** 3-4 روز

---

#### 3️⃣ **Medical Staff/Team Management** (مدیریت تیم پزشکی) ⭐⭐⭐⭐
**اولویت:** بالا  
**کاربرد:** معرفی پرستاران، تکنسین‌ها، کارکنان اداری (غیر از پزشکان)

**ویژگی‌های مورد نیاز:**
- CRUD کارکنان غیر پزشک
- نقش‌ها (Nurse, Technician, Admin, Receptionist, ...)
- تصویر و بیوگرافی
- تخصص و سوابق
- لینک به Doctor (در صورت وجود)
- شبکه‌های اجتماعی
- نمایش در صفحه "درباره ما"

**فیلدهای کلیدی:**
```csharp
- MedicalStaffId
- StaffName
- Role (Nurse, Technician, Admin, Receptionist, LabTechnician, ...)
- DoctorId (FK - اختیاری)
- Bio
- ImageUrl
- Specialization
- ExperienceYears
- Education
- PhoneNumber
- Email
- SocialMediaLinks (JSON)
- DepartmentId (FK - اختیاری)
- IsActive
- DisplayOrder
```

**زمان تخمینی:** 2-3 روز

---

#### 4️⃣ **Patient Education Materials** (مطالب آموزشی بیماران) ⭐⭐⭐⭐
**اولویت:** بالا  
**کاربرد:** فایل‌های آموزشی و راهنما برای بیماران

**ویژگی‌های مورد نیاز:**
- آپلود فایل PDF/Word/Excel
- دسته‌بندی مطالب
- لینک به ویدیوهای آموزشی
- امکان دانلود
- نمایش در پنل بیمار
- تاریخ انتشار
- تعداد دانلود
- CKEditor برای محتوای HTML

**فیلدهای کلیدی:**
```csharp
- PatientEducationMaterialId
- Title
- Description
- Content (HTML - CKEditor)
- FileUrl (PDF/Word/Excel)
- VideoUrl
- Category
- DownloadCount
- ViewCount
- IsActive
- PublishedAt
- DisplayOrder
```

**زمان تخمینی:** 2-3 روز

---

### 🟡 اولویت متوسط (Medium Priority)

#### 5️⃣ **Clinic Policies & Rules** (قوانین و مقررات) ⭐⭐⭐
**اولویت:** متوسط  
**کاربرد:** قوانین، مقررات و سیاست‌های کلینیک

**ویژگی‌های مورد نیاز:**
- دسته‌بندی قوانین (نوبت‌دهی، پرداخت، لغو نوبت، حریم خصوصی، ...)
- نسخه‌بندی قوانین
- تاریخ اعمال
- امکان نمایش در صفحه اصلی
- لینک به PDF قوانین کامل
- CKEditor برای محتوای HTML

**فیلدهای کلیدی:**
```csharp
- ClinicPolicyId
- PolicyTitle
- PolicyContent (HTML - CKEditor)
- Category (Appointment, Payment, Cancellation, Privacy, Terms, ...)
- Version
- EffectiveDate
- PdfUrl
- IsActive
- DisplayOnHomepage
- DisplayOrder
```

**زمان تخمینی:** 2 روز

---

#### 6️⃣ **Room Management** (مدیریت اتاق‌ها) ⭐⭐⭐
**اولویت:** متوسط  
**کاربرد:** مدیریت اتاق‌های کلینیک برای نوبت‌دهی

**ویژگی‌های مورد نیاز:**
- CRUD اتاق‌ها
- شماره/نام اتاق
- نوع اتاق (معاینه، جراحی، آزمایشگاه، ...)
- دپارتمان مرتبط
- تجهیزات موجود در اتاق
- وضعیت (آزاد، اشغال، تعمیر)
- تصویر اتاق

**فیلدهای کلیدی:**
```csharp
- RoomId
- RoomNumber
- RoomName
- RoomType (Examination, Surgery, Laboratory, Imaging, ...)
- DepartmentId (FK)
- EquipmentIds (JSON Array - لینک به MedicalEquipment)
- Status (Available, Occupied, Maintenance)
- ImageUrl
- Capacity
- Description
- IsActive
```

**زمان تخمینی:** 2-3 روز

---

#### 7️⃣ **Equipment Maintenance** (نگهداری تجهیزات) ⭐⭐⭐
**اولویت:** متوسط  
**کاربرد:** مدیریت تعمیرات و نگهداری تجهیزات پزشکی

**ویژگی‌های مورد نیاز:**
- ثبت تعمیرات
- تاریخ تعمیرات
- هزینه تعمیرات
- شرکت تعمیرکننده
- وضعیت (در انتظار، در حال تعمیر، تکمیل شده)
- یادآوری تعمیرات دوره‌ای
- تاریخچه تعمیرات

**فیلدهای کلیدی:**
```csharp
- EquipmentMaintenanceId
- MedicalEquipmentId (FK)
- MaintenanceType (Repair, Service, Calibration, ...)
- MaintenanceDate
- NextMaintenanceDate
- Cost
- ServiceProvider
- Description
- Status (Pending, InProgress, Completed)
- CompletedAt
- TechnicianName
```

**زمان تخمینی:** 2-3 روز

---

#### 8️⃣ **Pricing & Packages** (قیمت‌گذاری و پکیج‌ها) ⭐⭐⭐
**اولویت:** متوسط  
**کاربرد:** مدیریت پکیج‌های خدماتی و قیمت‌های ویژه

**ویژگی‌های مورد نیاز:**
- ایجاد پکیج‌های خدماتی
- ترکیب چند خدمت در یک پکیج
- قیمت ویژه پکیج
- تاریخ شروع/پایان اعتبار
- نمایش در سایت
- تخفیف درصدی یا مبلغی

**فیلدهای کلیدی:**
```csharp
- ServicePackageId
- PackageName
- PackageDescription
- ServiceIds (JSON Array)
- OriginalPrice
- DiscountPrice
- DiscountPercentage
- StartDate
- EndDate
- IsActive
- DisplayOrder
- ImageUrl
```

**زمان تخمینی:** 3-4 روز

---

#### 9️⃣ **Promotions & Discounts** (پیشنهادات و تخفیف‌ها) ⭐⭐⭐
**اولویت:** متوسط  
**کاربرد:** مدیریت پیشنهادات ویژه و کدهای تخفیف

**ویژگی‌های مورد نیاز:**
- ایجاد کد تخفیف
- درصد یا مبلغ تخفیف
- تاریخ شروع/پایان
- تعداد استفاده محدود
- حداقل مبلغ خرید
- قابل استفاده برای خدمات خاص
- نمایش در سایت

**فیلدهای کلیدی:**
```csharp
- PromotionId
- PromotionCode
- Title
- Description
- DiscountType (Percentage, FixedAmount)
- DiscountValue
- StartDate
- EndDate
- MinPurchaseAmount
- MaxUsageCount
- CurrentUsageCount
- ServiceIds (JSON Array - اختیاری)
- IsActive
- ImageUrl
```

**زمان تخمینی:** 2-3 روز

---

### 🟢 اولویت پایین (Low Priority)

#### 🔟 **Analytics & Reporting** (تحلیل و گزارش‌گیری) ⭐⭐
**اولویت:** پایین  
**کاربرد:** گزارش‌گیری و تحلیل داده‌های CMS

**ویژگی‌های مورد نیاز:**
- آمار بازدید صفحات
- آمار دانلود فایل‌ها
- آمار باز شدن ایمیل‌ها
- گزارش‌های سفارشی
- Export به Excel/PDF
- Dashboard با نمودارها

**زمان تخمینی:** 4-5 روز

---

## 📊 ماتریس اولویت‌بندی و زمان‌بندی

| ماژول | اولویت | پیچیدگی | زمان تخمینی | ROI | وضعیت |
|-------|--------|---------|-------------|-----|-------|
| Contact Form Management | ⭐⭐⭐⭐⭐ | متوسط | 2-3 روز | بسیار بالا | ❌ پیاده‌سازی نشده |
| Newsletter/Subscription | ⭐⭐⭐⭐ | متوسط-بالا | 3-4 روز | بالا | ❌ پیاده‌سازی نشده |
| Medical Staff/Team | ⭐⭐⭐⭐ | متوسط | 2-3 روز | بالا | ❌ پیاده‌سازی نشده |
| Patient Education Materials | ⭐⭐⭐⭐ | متوسط | 2-3 روز | بالا | ❌ پیاده‌سازی نشده |
| Clinic Policies & Rules | ⭐⭐⭐ | پایین-متوسط | 2 روز | متوسط | ❌ پیاده‌سازی نشده |
| Room Management | ⭐⭐⭐ | متوسط | 2-3 روز | متوسط | ❌ پیاده‌سازی نشده |
| Equipment Maintenance | ⭐⭐⭐ | متوسط | 2-3 روز | متوسط | ❌ پیاده‌سازی نشده |
| Pricing & Packages | ⭐⭐⭐ | متوسط-بالا | 3-4 روز | متوسط | ❌ پیاده‌سازی نشده |
| Promotions & Discounts | ⭐⭐⭐ | متوسط | 2-3 روز | متوسط | ❌ پیاده‌سازی نشده |
| Analytics & Reporting | ⭐⭐ | بالا | 4-5 روز | پایین | ❌ پیاده‌سازی نشده |

---

## 🎯 پیشنهاد ترتیب پیاده‌سازی

### **فاز 1: اولویت بالا (2-3 هفته)**
1. ✅ **Contact Form Management** - ضروری برای ارتباط با بیماران
2. ✅ **Newsletter/Subscription** - بازاریابی و اطلاع‌رسانی
3. ✅ **Medical Staff/Team** - تکمیل صفحه "درباره ما"
4. ✅ **Patient Education Materials** - بهبود تجربه بیمار

### **فاز 2: اولویت متوسط (2-3 هفته)**
5. ✅ **Clinic Policies & Rules** - شفافیت و اعتماد
6. ✅ **Room Management** - مدیریت منابع
7. ✅ **Equipment Maintenance** - نگهداری تجهیزات
8. ✅ **Pricing & Packages** - بازاریابی خدمات
9. ✅ **Promotions & Discounts** - جذب بیمار

### **فاز 3: اولویت پایین (1 هفته)**
10. ✅ **Analytics & Reporting** - تحلیل و بهینه‌سازی

---

## 📝 نکات مهم

### ✅ استانداردهای پیاده‌سازی
- تمام ماژول‌ها باید طبق **قرارداد توسعه** (`Docs/DEVELOPMENT_CONTRACT.md`) پیاده‌سازی شوند
- استفاده از **Strongly-Typed ViewModels**
- استفاده از **GetViewPath()** برای مسیریابی
- استفاده از **NotificationHelper** برای پیام‌ها
- استفاده از **Persian DatePicker** برای تاریخ‌ها
- استفاده از **Image Upload System** برای تصاویر
- استفاده از **CKEditor** برای محتوای HTML
- رعایت **SRP** و **SOLID Principles**

### ✅ یکپارچگی با ماژول‌های موجود
- **Contact Form** باید با **Email/SMS Service** یکپارچه شود
- **Newsletter** باید با **Email Service** یکپارچه شود
- **Medical Staff** باید با **Doctor Module** یکپارچه شود (در صورت نیاز)
- **Room Management** باید با **Appointment Module** یکپارچه شود
- **Equipment Maintenance** باید با **MedicalEquipment** یکپارچه شود

### ✅ UI/UX
- تمام ماژول‌ها باید طبق **Design System** طراحی شوند
- استفاده از رنگ‌های رسمی و حرفه‌ای
- پشتیبانی کامل از **RTL**
- **Responsive Design** برای موبایل و تبلت
- **Accessibility** (WCAG 2.1 Level AA)

---

## ✅ نتیجه‌گیری

**ماژول‌های پیاده‌سازی شده:** 14 ماژول ✅  
**ماژول‌های ضروری باقی‌مانده:** 10 ماژول ❌

**زمان کل تخمینی برای پیاده‌سازی:** 25-35 روز کاری

**اولویت اصلی:** پیاده‌سازی **Contact Form Management** و **Newsletter/Subscription** برای بهبود ارتباط با بیماران و بازاریابی.

---

**تاریخ به‌روزرسانی:** 2025-12-12  
**نسخه:** 1.0.0

