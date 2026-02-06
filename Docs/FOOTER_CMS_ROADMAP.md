# نقشه راه و TODO: ماژول CMS فوتر (Enterprise)

**تاریخ:** بهمن ۱۴۰۴  
**وضعیت Migration:** ✅ جدول‌ها در دیتابیس ایجاد شدند  
**مرجع قراردادها:** `Contracts/Knowledge-Base/AI/Master/03-Development-Contract-Quick-Guide.md`  
**مرجع CMS:** `Docs/CMS_REQUIRED_MODULES_ANALYSIS.md`  
**مرجع Routing:** `Contracts/Knowledge-Base/AI/Master/08-MVC-Routing-Best-Practices.md`

---

## ۱. تطابق با پایگاه دانش و قراردادها

| اصل | منبع | اعمال در ماژول فوتر |
|-----|------|----------------------|
| **Strongly-Typed** | 03-Development-Contract | همه Viewها با `@model` مشخص؛ بدون `dynamic` و ViewBag برای داده اصلی |
| **ViewModel برای همه** | همان | ViewModelهای جدا برای Index/Edit/Create و آیتم‌های لیست (مثل FAQ/Slider) |
| **SRP** | قرارداد پروژه | Repository فقط داده؛ Service منطق و تبدیل به ViewModel؛ Controller فقط HTTP |
| **مسیر CMS** | AdminAreaRegistration | `Admin/CMS/Footer/...` تحت route موجود `Admin_CMS_Default` |
| **BaseCMSController** | CMS ماژولها | `FooterController : BaseCMSController` و استفاده از `GetViewPath()` |
| **ServiceResult** | Helpers | خروجی سرویس‌ها `ServiceResult<T>` برای موفقیت/خطا و پیام |
| **ISoftDelete / ITrackable** | Entityهای CMS | لینک/سوشال/مجوز از این رابط‌ها استفاده می‌کنند (قبلاً در Entityها اعمال شده) |

---

## ۲. عناصر فوتر و نیازمندی‌های مدیریت

| بلوک | منبع داده فعلی | پس از پیاده‌سازی | توضیح |
|------|------------------|-------------------|--------|
| **Brand** | هاردکد در HomePageService | `FooterSettings` | نام، لوگو، تگ‌لاین، توضیح، لینک خانه |
| **Social Media** | هاردکد | `FooterSocial` (CRUD) | پلتفرم، URL، آیکن، AriaLabel، ترتیب |
| **Newsletter** | پارشال ثابت | بدون تغییر در فاز ۱ | فقط نمایش؛ مدیریت متن/تنظیمات اختیاری در فاز بعد |
| **Quick Links** | هاردکد | `FooterLink` با `LinkType=1` | عنوان، URL، آیکن، خارجی، ترتیب |
| **Service Links** | هاردکد | `FooterLink` با `LinkType=2` | همان ساختار |
| **Contact** | ContactSection + Emergency | `FooterSettings` + fallback به Contact | تلفن، اورژانس، ایمیل، آدرس، واتساپ |
| **Working Hours** | ClinicWorkingHours | `FooterSettings.WorkingHoursTitle` + همان سرویس فعلی | فقط عنوان بلوک از CMS؛ روز/ساعت از ClinicWorkingHours |
| **Certifications** | هاردکد | `FooterCertification` (CRUD) | عنوان، توضیح، تصویر، لینک، شماره مجوز، ترتیب |
| **Legal** | هاردکد | `FooterSettings` | کپی‌رایت، حریم خصوصی، قوانین، شکایات، متن محرمانگی پزشکی |

---

## ۳. نقشه راه (فازها)

### فاز ۱ – لایه داده و سرویس (امروز/فردا)

1. **Interface و Repository**
   - `Interfaces/CMS/IFooterSettingsRepository.cs`  
     - `GetByClinicAsync(int? clinicId)`, `GetDefaultAsync()`, `Add`, `Update`
   - `Interfaces/CMS/IFooterLinkRepository.cs`  
     - `GetActiveByTypeAsync(byte linkType, int? clinicId)`, CRUD
   - `Interfaces/CMS/IFooterSocialRepository.cs`  
     - `GetActiveAsync(int? clinicId)`, CRUD
   - `Interfaces/CMS/IFooterCertificationRepository.cs`  
     - `GetActiveAsync(int? clinicId)`, CRUD
   - پیاده‌سازی هر چهار در `Repositories/CMS/`

2. **سرویس فوتر (خواندن برای سایت)**
   - `Interfaces/CMS/IFooterService.cs`  
     - `GetPublicFooterAsync(int? clinicId)` → `FooterViewModel`
   - `Services/CMS/FooterService.cs`  
     - ترکیب Settings + Links + Social + Certifications؛ ساخت `PhoneLink`, `EmailLink`, `WhatsAppLink`؛ fallback به مقدار پیش‌فرض در صورت نبود رکورد

3. **اتصال به HomePageService**
   - در `GetFooterDataAsync` / `GetFooterDataInternalAsync`:  
     ابتدا `_footerService.GetPublicFooterAsync(clinicId)`؛ اگر داده داشت از آن استفاده شود، وگرنه همان منطق فعلی (Clinic + Contact + هاردکد).

4. **ثبت در DI**
   - در `App_Start/UnityConfig.cs` ثبت Interfaceهای Repository و `IFooterService`/`FooterService`.

---

### فاز ۲ – پنل ادمین (صفحه تنظیمات اصلی)

1. **ViewModelهای ادمین**
   - `ViewModels/CMS/FooterSettingsEditViewModel.cs` (برابر فیلدهای Brand + Contact + Legal + WorkingHoursTitle)
   - در صورت نیاز: ViewModelهای لیست برای لینک/سوشال/مجوز (یا استفاده از همان Entity/IndexViewModel ساده)

2. **FooterController در Admin**
   - `Areas/Admin/Controllers/CMS/FooterController.cs`  
     - ارث از `BaseCMSController`
     - `Index()` → نمایش یک صفحه با تب/بخش: تنظیمات اصلی | لینک‌ها | شبکه‌ها | مجوزها
     - `EditSettings()` GET/POST برای یک رکورد `FooterSettings` (با id یا clinicId)
     - استفاده از `GetViewPath("Index")` و `GetViewPath("EditSettings")`

3. **Viewهای اولیه**
   - `Areas/Admin/Views/CMS/Footer/Index.cshtml`  
     - لینک به «تنظیمات فوتر»، لینک به لیست لینک‌ها/سوشال/مجوزها
   - `Areas/Admin/Views/CMS/Footer/EditSettings.cshtml`  
     - فرم Brand، Contact، Legal، WorkingHoursTitle با Strongly-Typed و اعتبارسنجی

4. **سرویس ادمین (نوشتن)**
   - در `IFooterService`:  
     `GetSettingsForEditAsync(int? clinicId)`, `SaveSettingsAsync(FooterSettingsEditViewModel)`
   - یا متدهای جدا در همان سرویس برای ذخیره تنظیمات

---

### فاز ۳ – CRUD لینک‌ها، شبکه‌ها، مجوزها

1. **لینک‌ها (Quick + Service)**
   - `FooterController`: `LinkIndex(byte? type)`, `LinkCreate`, `LinkEdit`, `LinkDelete` (یا SoftDelete)
   - ViewModels: مثلاً `FooterLinkIndexViewModel`, `FooterLinkCreateEditViewModel`
   - Viewها: `LinkIndex.cshtml`, `LinkCreate.cshtml`, `LinkEdit.cshtml`

2. **شبکه‌های اجتماعی**
   - `FooterController`: `SocialIndex`, `SocialCreate`, `SocialEdit`, `SocialDelete`
   - ViewModels و Viewهای متناظر

3. **مجوزها**
   - `FooterController`: `CertificationIndex`, `CertificationCreate`, `CertificationEdit`, `CertificationDelete`
   - ViewModels و Viewها؛ در صورت نیاز آپلود تصویر (مطابق ماژولهای دیگر CMS)

4. **اعتبارسنجی و امنیت**
   - URL بدون `javascript:` و بدون Open Redirect
   - محدودیت طول فیلدها مطابق Entity
   - برای لینک خارجی: `rel="noopener noreferrer"` در خروجی View (یا در سرویس علامت‌گذاری شود)

---

### فاز ۴ – چندکلینیکی و بهینه‌سازی (اختیاری)

- فیلتر همه‌ی Repositoryها با `ClinicId`؛ در صورت نبود رکورد برای کلینیک، استفاده از رکورد با `ClinicId = null` (سراسری).
- کش کردن خروجی `GetPublicFooterAsync` با کلید مثلاً `footer_{clinicId}` و invalidation بعد از هر ذخیره در ادمین.
- در منوی ادمین، یک آیتم «مدیریت فوتر» زیر بخش CMS با لینک به `Admin/CMS/Footer`.

---

## ۴. TODO لیست (برای پیگیری)

### فاز ۱ – داده و سرویس ✅

- [x] **IFooterSettingsRepository** + **FooterSettingsRepository**
- [x] **IFooterLinkRepository** + **FooterLinkRepository**
- [x] **IFooterSocialRepository** + **FooterSocialRepository**
- [x] **IFooterCertificationRepository** + **FooterCertificationRepository**
- [x] **IFooterService** + **FooterService** (GetPublicFooterAsync)
- [x] **HomePageService**: استفاده از FooterService با fallback
- [x] **UnityConfig**: ثبت همه Interfaceها و پیاده‌سازی‌ها

### فاز ۲ – ادمین تنظیمات اصلی

- [ ] **FooterSettingsEditViewModel**
- [ ] **FooterController** (Index, EditSettings GET/POST)
- [ ] **View: Index.cshtml** (داشبورد فوتر با لینک به تنظیمات و لیست‌ها)
- [ ] **View: EditSettings.cshtml** (فرم Brand, Contact, Legal, WorkingHoursTitle)
- [ ] **IFooterService** گسترش: GetSettingsForEditAsync, SaveSettingsAsync

### فاز ۳ – CRUD لیست‌ها

- [ ] لینک‌ها: ViewModelها، اکشن‌ها، Viewها (LinkIndex, LinkCreate, LinkEdit)
- [ ] شبکه‌ها: ViewModelها، اکشن‌ها، Viewها (SocialIndex, SocialCreate, SocialEdit)
- [ ] مجوزها: ViewModelها، اکشن‌ها، Viewها + آپلود تصویر در صورت نیاز
- [ ] اعتبارسنجی URL و خروجی امن (noopener و غیره)

### فاز ۴ (اختیاری)

- [ ] پشتیبانی ClinicId در همه‌ی کوئری‌ها و fallback به سراسری
- [ ] کش GetPublicFooterAsync و invalidation
- [ ] آیتم منوی ادمین «مدیریت فوتر»

---

## ۵. مسیرها و نام‌گذاری

| مسیر | کنترلر | توضیح |
|------|--------|--------|
| `Admin/CMS/Footer` | FooterController.Index | صفحه اصلی مدیریت فوتر |
| `Admin/CMS/Footer/EditSettings` | FooterController.EditSettings | ویرایش تنظیمات (Brand, Contact, Legal, Hours Title) |
| `Admin/CMS/Footer/LinkIndex` | FooterController.LinkIndex | لیست لینک‌های سریع/خدمات |
| `Admin/CMS/Footer/SocialIndex` | FooterController.SocialIndex | لیست شبکه‌های اجتماعی |
| `Admin/CMS/Footer/CertificationIndex` | FooterController.CertificationIndex | لیست مجوزها |

همه تحت **Admin_CMS_Default** (`Admin/CMS/{controller}/{action}/{id}`) بدون نیاز به ثبت route جدید.

---

## ۶. خلاصه

- **Migration انجام شده است**؛ فقط لایه Repository، Service، Controller و View باقی مانده است.
- با طی کردن فاز ۱ و ۲، فوتر سایت از دیتابیس و پنل ادمین قابل مدیریت می‌شود.
- با انجام فاز ۳، همه‌ی عناصر (لینک‌ها، شبکه‌ها، مجوزها) به صورت Enterprise و قابل مدیریت خواهند بود.
- این سند با قراردادهای توسعه و پایگاه دانش پروژه هم‌خوان است و می‌توان آن را به عنوان مرجع برای پیاده‌سازی و بررسی استفاده کرد.
