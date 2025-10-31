# 🚫 **غیرفعال‌سازی کامل SSL/HTTPS در محیط Development**

**تاریخ:** 2024  
**مشکل:** `ERR_SSL_PROTOCOL_ERROR` در `https://localhost:3560/ReceptionV2/Index`  
**هدف:** غیرفعال کردن کامل SSL/HTTPS برای محیط توسعه

---

## ✅ **تغییرات اعمال شده**

### **1. ClinicApp.csproj**

#### **IISExpressSSLPort کامنت شد:**
```xml
<!-- 🚫 DISABLED: SSL Port disabled for development -->
<!-- <IISExpressSSLPort>44363</IISExpressSSLPort> -->
```

### **2. Web.config**

#### **Content-Security-Policy موقتاً غیرفعال شد (برای جلوگیری از خطای HTTPS در CSP):**
```xml
<!-- 🚫 DISABLED IN DEVELOPMENT: CSP relaxed for local development -->
<!-- <add name="Content-Security-Policy" ... /> -->
```

### **3. سایر تنظیمات SSL (قبلاً اعمال شده)**

- ✅ **HTTPS Redirect** کامنت شده
- ✅ **HSTS Header** کامنت شده  
- ✅ **CookieSecure** = `SameAsRequest` (HTTP/HTTPS هر دو مجاز)
- ✅ **SerilogWeb:SkipPreApplicationStart** = `true`

---

## 📊 **وضعیت نهایی**

| تنظیمات | وضعیت | توضیحات |
|---------|-------|---------|
| IISExpressSSLPort | غیرفعال | کامنت شده در .csproj |
| HTTPS Redirect | غیرفعال | کامنت شده در Web.config |
| HSTS Header | غیرفعال | کامنت شده در Web.config |
| CookieSecure | SameAsRequest | HTTP/HTTPS هر دو مجاز |
| Content-Security-Policy | غیرفعال | کامنت شده برای development |

---

## 🔧 **مراحل بعدی**

### **1. Rebuild پروژه**
```powershell
# در Visual Studio
Build > Rebuild Solution
```

### **2. Restart IIS Express**
- Stop IIS Express در Task Manager
- یا Restart از Visual Studio

### **3. استفاده از HTTP فقط**
```
http://localhost:xxxxx/ReceptionV2/Index
```

**⚠️ توجه:** از `https://` استفاده نکنید، فقط `http://`

---

## ✅ **بررسی**

بعد از Rebuild و Restart:
1. Visual Studio > Debug > Start Without Debugging
2. مرورگر باید به صورت خودکار `http://localhost:xxxxx` را باز کند
3. اگر هنوز `https://` باز می‌شود:
   - مرورگر را در حالت Incognito/Private باز کنید
   - یا Cache مرورگر را پاک کنید
   - یا URL را به صورت دستی به `http://` تغییر دهید

---

## 🔄 **بازگردانی برای Production (بعداً)**

برای محیط Production، باید:
1. `IISExpressSSLPort` را فعال کنید
2. HTTPS Redirect را فعال کنید
3. HSTS Header را فعال کنید
4. `CookieSecure = Always` تنظیم شود
5. Content-Security-Policy را فعال کنید

---

**تاریخ به‌روزرسانی:** 2024  
**نسخه:** 1.0

