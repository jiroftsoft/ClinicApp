# 🛣️ ASP.NET MVC Routing - Best Practices

**نسخه:** 1.0.0  
**تاریخ:** 1404/10/05  
**منبع:** تجربه واقعی از پروژه ClinicApp

---

## 🎯 هدف این مستند

این مستند حاوی **درس‌های گرانبها** از رفع خطاهای واقعی routing در پروژه است.

---

## 🚨 قانون طلایی: ترتیب Route ها مهم است!

### ❌ اشتباه رایج:

```csharp
// App_Start/RouteConfig.cs

// ❌ WRONG ORDER: Route عمومی قبل از خاص
routes.MapRoute(
    name: "ApiPatientController",
    url: "Api/Patient/{action}/{id}",  // عمومی‌تر
    ...
);

routes.MapRoute(
    name: "Patient_Specific",
    url: "Patient/{action}/{id}",      // خاص‌تر
    ...
);
```

**مشکل:**
- `@Url.Action("LoadPatients", "Patient")` به route اول (Api) match می‌شود
- URL اشتباه: `/Api/Patient/LoadPatients` به جای `/Patient/LoadPatients`
- نتیجه: 404 Error!

---

### ✅ روش صحیح:

```csharp
// App_Start/RouteConfig.cs

// ✅ CORRECT ORDER: Route خاص قبل از عمومی
routes.MapRoute(
    name: "Patient_Specific",
    url: "Patient/{action}/{id}",      // ✅ خاص‌تر - اول
    defaults: new { controller = "Patient", action = "Index", id = UrlParameter.Optional },
    namespaces: new[] { "ClinicApp.Controllers" }
).DataTokens["UseNamespaceFallback"] = false;

routes.MapRoute(
    name: "ApiPatientController",
    url: "Api/Patient/{action}/{id}",  // ✅ عمومی‌تر - بعد
    defaults: new { controller = "Patient", action = "Search", area = "", id = UrlParameter.Optional },
    namespaces: new[] { "ClinicApp.Controllers.Api" }
).DataTokens["UseNamespaceFallback"] = false;
```

**چرا کار می‌کند:**
- MVC Route Engine از **بالا به پایین** routes را بررسی می‌کند
- اولین route که match شود، انتخاب می‌شود
- Route خاص‌تر (`Patient/{action}`) قبل از route عمومی‌تر (`Api/Patient/{action}`) چک می‌شود

---

## 🔒 UseNamespaceFallback = false

### چیست؟

وقتی `UseNamespaceFallback = false` تنظیم می‌شود، MVC **فقط** در namespace مشخص شده دنبال controller می‌گردد.

### چرا مهم است؟

```csharp
// ❌ بدون UseNamespaceFallback (مشکل دارد)
routes.MapRoute(
    name: "Patient_Specific",
    url: "Patient/{action}/{id}",
    namespaces: new[] { "ClinicApp.Controllers" }
);
// اگر controller در این namespace نباشد، در namespace های دیگر هم می‌گردد!
```

```csharp
// ✅ با UseNamespaceFallback = false (صحیح)
routes.MapRoute(
    name: "Patient_Specific",
    url: "Patient/{action}/{id}",
    namespaces: new[] { "ClinicApp.Controllers" }
).DataTokens["UseNamespaceFallback"] = false;
// فقط در این namespace می‌گردد، جای دیگر نگاه نمی‌کند
```

**فایده:**
- جلوگیری از Ambiguous Match
- عملکرد بهتر (کمتر جستجو می‌کند)
- خطاهای واضح‌تر (404 فوری اگر controller نباشد)

---

## 🎯 Area در @Url.Action

### مشکل:

```csharp
// Views/Patient/Index.cshtml

// ❌ مبهم: به کدام namespace اشاره می‌کند؟
@Url.Action("LoadPatients", "Patient")
```

**نتیجه:**
- MVC باید حدس بزند: `Controllers.Patient` یا `Controllers.Api.Patient`؟
- ممکن است به route اشتباه match شود

---

### راه‌حل:

```csharp
// ✅ واضح: مشخص کردن area
@Url.Action("LoadPatients", "Patient", new { area = "" })
```

**فایده:**
- واضح است که به root area اشاره دارد
- جلوگیری از match شدن به route های area
- خطای کمتر، عملکرد بهتر

---

## 📋 Checklist قبل از تعریف Route

### 1. ترتیب Routes

- [ ] Route های **خاص** قبل از **عمومی**
- [ ] Route های **Area** قبل از **Global**
- [ ] Route های با **Constraint** قبل از **بدون Constraint**

### 2. Namespace

- [ ] `namespaces` مشخص شده
- [ ] `UseNamespaceFallback = false` تنظیم شده
- [ ] Namespace واقعاً وجود دارد و Controller در آن است

### 3. Defaults

- [ ] `controller` صحیح است
- [ ] `action` پیش‌فرض معنادار است
- [ ] `id = UrlParameter.Optional` اضافه شده (در صورت نیاز)

### 4. Testing

- [ ] `@Url.Action` را در View test کن
- [ ] URL نهایی را بررسی کن (View Source)
- [ ] با Postman/curl test کن

---

## 🛠️ دستور العمل رفع خطای 404 در Route

### Step 1: بررسی ترتیب Routes

```csharp
// RouteConfig.cs را باز کن
// از بالا به پایین بخوان
// Route اشتباه را پیدا کن
```

### Step 2: بررسی Namespace

```csharp
// Controller واقعاً در این namespace است؟
namespace ClinicApp.Controllers  // ✅
{
    public class PatientController : Controller { }
}
```

### Step 3: بررسی View

```csharp
// @Url.Action صحیح است؟
@Url.Action("LoadPatients", "Patient", new { area = "" })  // ✅
```

### Step 4: Testing

```bash
# URL نهایی چیست؟
curl http://localhost:3560/Patient/LoadPatients
```

---

## 📊 مقایسه: قبل و بعد

### قبل از رفع (❌ اشتباه):

```
Route Order:
1. ApiPatientController: "Api/Patient/{action}"
2. Patient_Specific:     "Patient/{action}"

Result:
@Url.Action("LoadPatients", "Patient") → /Api/Patient/LoadPatients ❌
```

### بعد از رفع (✅ صحیح):

```
Route Order:
1. Patient_Specific:     "Patient/{action}"  ← UseNamespaceFallback = false
2. ApiPatientController: "Api/Patient/{action}" ← UseNamespaceFallback = false

Result:
@Url.Action("LoadPatients", "Patient", new { area = "" }) → /Patient/LoadPatients ✅
```

---

## 🎓 یادگیری‌های کلیدی

### 1. **ترتیب = اولویت**

```
Route Engine از بالا به پایین می‌خواند
اولین Match = برنده
```

### 2. **UseNamespaceFallback = امنیت**

```
false = فقط namespace مشخص شده
true  = همه namespace ها (خطرناک!)
```

### 3. **area = وضوح**

```
بدون area = مبهم
با area = واضح
```

### 4. **Test, Test, Test!**

```
فرض نکن، تست کن!
```

---

## 📚 مثال‌های واقعی از پروژه

### مثال 1: Patient Routing

```csharp
// ✅ صحیح
routes.MapRoute(
    name: "Patient_Specific",
    url: "Patient/{action}/{id}",
    defaults: new { controller = "Patient", action = "Index", id = UrlParameter.Optional },
    namespaces: new[] { "ClinicApp.Controllers" }
).DataTokens["UseNamespaceFallback"] = false;
```

**URLs:**
- `/Patient/Index` → `Controllers.PatientController.Index()`
- `/Patient/Edit/123` → `Controllers.PatientController.Edit(123)`
- `/Patient/LoadPatients` → `Controllers.PatientController.LoadPatients()`

---

### مثال 2: Api Patient Routing

```csharp
// ✅ صحیح
routes.MapRoute(
    name: "ApiPatientController",
    url: "Api/Patient/{action}/{id}",
    defaults: new { controller = "Patient", action = "Search", area = "", id = UrlParameter.Optional },
    namespaces: new[] { "ClinicApp.Controllers.Api" }
).DataTokens["UseNamespaceFallback"] = false;
```

**URLs:**
- `/Api/Patient/Search` → `Controllers.Api.PatientController.Search()`
- `/Api/Patient/LoadPatients` → `Controllers.Api.PatientController.LoadPatients()`
- `/Api/Patient/GetDetails/123` → `Controllers.Api.PatientController.GetDetails(123)`

---

### مثال 3: Patient Area Routing

```csharp
// Areas/Patient/PatientAreaRegistration.cs

// ✅ صحیح: با constraint محدود شده
context.MapRoute(
    "Patient_default",
    "Patient/{controller}/{action}/{id}",
    new { action = "Index", id = UrlParameter.Optional },
    new { controller = @"^(Appointment|AppointmentBooking)$" },  // ✅ فقط این controllers
    namespaces: new[] { "ClinicApp.Areas.Patient.Controllers" }
);
```

**URLs:**
- `/Patient/Appointment/Index` → Area: `Patient`, Controller: `Appointment`
- `/Patient/AppointmentBooking/SelectDoctor` → Area: `Patient`, Controller: `AppointmentBooking`
- `/Patient/Edit/123` → Area: (none), Controller: `Patient` (از Global route)

---

## 🚨 اشتباهات رایج

### 1. Route تکراری

```csharp
// ❌ اشتباه: دو route با نام یکسان
routes.MapRoute(
    name: "Patient_Specific",
    url: "Patient/{action}/{id}",
    ...
);

// ... 500 خط بعد ...

routes.MapRoute(
    name: "Patient_Specific",  // ❌ تکراری!
    url: "Patient/{action}/{id}",
    ...
);
```

**راه‌حل:** نام‌های منحصر به فرد، جستجو قبل از اضافه کردن

---

### 2. فراموش کردن UseNamespaceFallback

```csharp
// ❌ خطرناک
routes.MapRoute(
    name: "Patient_Specific",
    url: "Patient/{action}/{id}",
    namespaces: new[] { "ClinicApp.Controllers" }
);
// اگر controller در namespace دیگری هم باشد، Ambiguous Match!
```

**راه‌حل:** همیشه `UseNamespaceFallback = false`

---

### 3. فراموش کردن area در View

```csharp
// ❌ مبهم
@Url.Action("LoadPatients", "Patient")

// ✅ واضح
@Url.Action("LoadPatients", "Patient", new { area = "" })
```

---

## 🔗 مراجع

- `App_Start/RouteConfig.cs` - Route Configuration
- `Areas/Patient/PatientAreaRegistration.cs` - Area Routes
- `Docs/14041005/PATIENT_INDEX_LOADPATIENTS_ROUTING_FIX.md` - مستند اصلی

---

## ✅ خلاصه

1. **ترتیب مهم است:** خاص قبل از عمومی
2. **UseNamespaceFallback = false:** همیشه
3. **area = "":** در View برای وضوح
4. **Test:** قبل از commit

---

**تهیه‌کننده:** AI Assistant  
**منبع:** تجربه واقعی پروژه ClinicApp  
**تاریخ:** 1404/10/05

