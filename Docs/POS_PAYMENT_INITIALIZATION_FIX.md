# ✅ گزارش اصلاح مشکلات Initialization پرداخت POS

**تاریخ:** 1404/09/11  
**وضعیت:** ✅ **اصلاح شد**

---

## 🔍 مشکلات شناسایی شده

### مشکل 1: SignalR Hubs URL به درستی render نمی‌شود
**خطا:**
```
Failed to load resource: the server responded with a status of 404 (Not Found)
@(System.Configuration.ConfigurationManager.AppSettings["SamanKishSignalRUrl"] ?? "http://localhost:8080/signalr")/hubs
```

**علت:**
- Razor syntax در فایل `.js` render نمی‌شود
- URL به عنوان literal string ارسال می‌شود

### مشکل 2: API endpoint برای دریافت ترمینال پیش‌فرض 404 می‌دهد
**خطا:**
```
POST http://localhost:3560/api/v1/reception/pos/terminals/default 404 (Not Found)
POST http://localhost:3560/Api/ReceptionApi/pos/terminals/default 404 (Not Found)
```

**علت:**
- Endpoint موجود `GET /api/v1/pos/terminals/default` است، نه `POST`
- مسیر درست `/api/v1/pos/terminals/default` است، نه `/api/v1/reception/pos/terminals/default`

---

## ✅ اصلاحات اعمال شده

### 1. اصلاح SignalR URL در View

**فایل:** `Views/ReceptionV2/Index.cshtml`

```csharp
@{
    // ...
    var signalRUrl = System.Configuration.ConfigurationManager.AppSettings["SamanKishSignalRUrl"] ?? "http://localhost:8080/signalr";
}

<script>
  window.ReceptionBootstrap = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Bootstrap));
  
  // ✅ تنظیم SignalR URL از Web.config برای استفاده در JavaScript
  window.SamanKishSignalRUrl = '@Html.Raw(signalRUrl)';
</script>
```

### 2. اصلاح استفاده از SignalR URL در JavaScript

**فایل:** `Scripts/reception.v2/payment-panel.js`

```javascript
// قبل:
var signalRUrl = '@(System.Configuration.ConfigurationManager.AppSettings["SamanKishSignalRUrl"] ?? "http://localhost:8080/signalr")';

// بعد:
var signalRUrl = window.SamanKishSignalRUrl || 'http://localhost:8080/signalr';
```

### 3. اصلاح API Call برای دریافت ترمینال پیش‌فرض

**فایل:** `Scripts/reception.v2/payment-panel.js`

```javascript
// قبل:
API.post('/pos/terminals/default', {})

// بعد:
API.get('/pos/terminals/default')
```

### 4. اصلاح Base URL در `ajaxWithFallback` برای POS API

**فایل:** `Scripts/reception.v2/reception-api.js`

```javascript
function ajaxWithFallback(method, path, data) {
  const d = $.Deferred();
  const cleanPath = path.replace(/^\//, ''); // Remove leading slash

  // ✅ تعیین base URL بر اساس نوع API
  let baseUrl;
  if (/^\/?pos\//i.test(path)) {
    baseUrl = '/api/v1'; // برای POS API از /api/v1 استفاده می‌کنیم
  } else {
    baseUrl = baseV1; // برای Reception API از baseV1 استفاده می‌کنیم
  }
  
  // ...
}
```

### 5. اضافه کردن POS API به `toLegacyPath` برای جلوگیری از Fallback

**فایل:** `Scripts/reception.v2/reception-api.js`

```javascript
function toLegacyPath(path) {
  // ...
  if (/^\/?pos\/terminals\/default/i.test(path)) {
    // ✅ POS Terminal API - no legacy fallback needed (new endpoint)
    return null; // Return null to skip legacy fallback
  }
  // ...
}
```

---

## 📊 Flow جدید

### 1. Initialization
```
1. View render می‌شود
2. SignalR URL از Web.config خوانده می‌شود
3. window.SamanKishSignalRUrl تنظیم می‌شود
4. payment-panel.js از window.SamanKishSignalRUrl استفاده می‌کند
5. PosPaymentClient با URL صحیح initialize می‌شود
```

### 2. دریافت ترمینال پیش‌فرض
```
1. API.get('/pos/terminals/default') فراخوانی می‌شود
2. ajaxWithFallback تشخیص می‌دهد که path با /pos/ شروع می‌شود
3. baseUrl = '/api/v1' تنظیم می‌شود
4. URL نهایی: /api/v1/pos/terminals/default
5. GET request ارسال می‌شود
6. PosTerminalApiController.GetDefault() پاسخ می‌دهد
```

---

## ✅ چک‌لیست

- [x] SignalR URL در View تنظیم شد
- [x] window.SamanKishSignalRUrl در JavaScript استفاده می‌شود
- [x] API call از POST به GET تغییر کرد
- [x] Base URL برای POS API اصلاح شد
- [x] Legacy fallback برای POS API غیرفعال شد
- [x] Endpoint path اصلاح شد (/api/v1/pos/terminals/default)

---

**مشکلات Initialization حل شد! ✅**

