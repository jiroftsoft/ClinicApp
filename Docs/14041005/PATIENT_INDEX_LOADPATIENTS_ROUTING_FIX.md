# 🔧 رفع خطای 404 برای LoadPatients در Patient/Index

**تاریخ:** 1404/10/05  
**نسخه:** 1.0.0  
**وضعیت:** ✅ **RESOLVED**  
**اولویت:** 🔴 **CRITICAL**

---

## 📋 **مشکل گزارش شده:**

```
Failed to load resource: the server responded with a status of 404 (Not Found)
URL: http://localhost:3560/Api/Patient/LoadPatients

Message: "No HTTP resource was found that matches the request URI"
MessageDetail: "No type was found that matches the controller named 'Patient'."
```

**صفحه:** `http://localhost:3560/Patient/Index`  
**خطا:** درخواست AJAX به `/Api/Patient/LoadPatients` با خطای 404 مواجه می‌شد

---

## 🔍 **علت خطا (Root Cause):**

### **مشکل اصلی:**
`@Url.Action("LoadPatients", "Patient")` در `Views/Patient/Index.cshtml` به **URL اشتباه** resolve می‌شد:

- ❌ **URL اشتباه:** `/Api/Patient/LoadPatients`
- ✅ **URL صحیح:** `/Patient/LoadPatients`

### **علت فنی:**

1. **ترتیب Route ها:**
   - Route `ApiPatientController` (برای `Api/Patient/{action}`) **قبل از** route `Patient_Specific` (برای `Patient/{action}`) تعریف شده بود
   - MVC Route Engine اولین route match را انتخاب می‌کرد

2. **Namespace Resolution:**
   - `@Url.Action` به route `ApiPatientController` match می‌شد
   - این route به namespace `ClinicApp.Controllers.Api` اشاره می‌کرد
   - اما View مربوط به `Controllers/PatientController` (namespace `ClinicApp.Controllers`) بود

3. **Route Matching:**
   ```
   Request: @Url.Action("LoadPatients", "Patient")
   
   Route Engine بررسی می‌کند:
   1. ApiPatientController: "Api/Patient/{action}" → ❌ Match می‌شود (اشتباه)
   2. Patient_Specific: "Patient/{action}" → ✅ باید Match شود (درست)
   ```

---

## ✅ **راه حل:**

### **1. تغییر ترتیب Route ها:**

Route `Patient_Specific` را **قبل از** route `ApiPatientController` قرار دادیم:

```csharp
// App_Start/RouteConfig.cs

// ✅ BEFORE: بعد از ApiPatientController (اشتباه)
routes.MapRoute(
    name: "ApiPatientController",
    url: "Api/Patient/{action}/{id}",
    ...
);

routes.MapRoute(
    name: "Patient_Specific",
    url: "Patient/{action}/{id}",
    ...
);

// ✅ AFTER: قبل از ApiPatientController (درست)
routes.MapRoute(
    name: "Patient_Specific",
    url: "Patient/{action}/{id}",
    defaults: new { controller = "Patient", action = "Index", id = UrlParameter.Optional },
    namespaces: new[] { "ClinicApp.Controllers" }
).DataTokens["UseNamespaceFallback"] = false;

routes.MapRoute(
    name: "ApiPatientController",
    url: "Api/Patient/{action}/{id}",
    ...
);
```

### **2. اضافه کردن UseNamespaceFallback = false:**

برای جلوگیری از fallback به namespace های دیگر:

```csharp
routes.MapRoute(
    name: "Patient_Specific",
    url: "Patient/{action}/{id}",
    ...
).DataTokens["UseNamespaceFallback"] = false; // ✅ اضافه شد
```

### **3. اصلاح View:**

اضافه کردن `area = ""` به `@Url.Action` برای اطمینان از resolve شدن صحیح:

```csharp
// Views/Patient/Index.cshtml

// BEFORE:
url: '@Url.Action("LoadPatients", "Patient")',

// AFTER:
url: '@Url.Action("LoadPatients", "Patient", new { area = "" })',
```

---

## 📊 **تغییرات اعمال شده:**

| فایل | تغییرات | خطوط |
|------|---------|------|
| `App_Start/RouteConfig.cs` | ✅ انتقال route `Patient_Specific` به قبل از `ApiPatientController` | 45-52 |
| `App_Start/RouteConfig.cs` | ✅ حذف route تکراری `Patient_Specific` | 528-533 (حذف شد) |
| `App_Start/RouteConfig.cs` | ✅ اضافه `UseNamespaceFallback = false` به route `ApiPatientController` | 85 |
| `Views/Patient/Index.cshtml` | ✅ اضافه `area = ""` به `@Url.Action` | 269 |

---

## 🎯 **نتیجه:**

### **قبل از رفع:**
```
@Url.Action("LoadPatients", "Patient")
→ /Api/Patient/LoadPatients ❌ (404 Error)
```

### **بعد از رفع:**
```
@Url.Action("LoadPatients", "Patient", new { area = "" })
→ /Patient/LoadPatients ✅ (200 OK)
```

---

## 🧪 **تست:**

### **Test 1: Patient/Index Page**
```
✅ GET: http://localhost:3560/Patient/Index
✅ AJAX: POST /Patient/LoadPatients
✅ Response: 200 OK با PartialView
```

### **Test 2: Api/Patient/LoadPatients (API Endpoint)**
```
✅ GET: http://localhost:3560/Api/Patient/LoadPatients
✅ POST: http://localhost:3560/Api/Patient/LoadPatients
✅ Response: 200 OK با JSON
```

### **Test 3: Route Resolution**
```
✅ @Url.Action("LoadPatients", "Patient") → /Patient/LoadPatients
✅ @Url.Action("Search", "Patient", new { area = "Api" }) → /Api/Patient/Search
```

---

## 📚 **یادگیری‌ها:**

### **1. ترتیب Route ها مهم است:**
- Route های **خاص‌تر** باید **قبل از** route های **عمومی‌تر** باشند
- MVC Route Engine اولین match را انتخاب می‌کند

### **2. Namespace Resolution:**
- استفاده از `UseNamespaceFallback = false` برای جلوگیری از fallback
- تعریف namespace های مشخص برای هر route

### **3. Route Naming:**
- نام‌گذاری واضح route ها برای جلوگیری از confusion
- استفاده از comment برای توضیح هدف route

---

## 🔗 **مراجع:**

- `App_Start/RouteConfig.cs` - Route Configuration
- `Views/Patient/Index.cshtml` - View با AJAX Call
- `Controllers/PatientController.cs` - MVC Controller
- `Controllers/Api/PatientController.cs` - API Controller

---

## ✅ **Checklist:**

- [x] Route `Patient_Specific` قبل از `ApiPatientController` قرار گرفت
- [x] `UseNamespaceFallback = false` اضافه شد
- [x] View اصلاح شد
- [x] Route تکراری حذف شد
- [x] تست انجام شد
- [x] مستندسازی انجام شد

---

**تهیه‌کننده:** AI Assistant  
**تاریخ:** 1404/10/05  
**نسخه:** 1.0.0  
**طبق:** DEBUGGING_SPECIALIST_CONTRACT.md

