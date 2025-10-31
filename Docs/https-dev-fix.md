# 🔧 **رفع مشکل SSL/HTTPS در محیط توسعه**

**تاریخ:** 2024  
**مشکل:** `ERR_SSL_PROTOCOL_ERROR` در localhost  
**دلیل:** اجباری بودن HTTPS در Web.config و Startup.Auth.cs

---

## ✅ **تغییرات اعمال شده**

### **1. Web.config**

#### **HTTPS Redirect Rule - کامنت شد:**
```xml
<!--🚫 DISABLED IN DEVELOPMENT: HTTPS redirect disabled for local development-->
<!--
<rewrite>
  <rules>
    <rule name="Redirect to HTTPS" stopProcessing="true">
      <match url="(.*)" />
      <conditions>
        <add input="{HTTPS}" pattern="off" ignoreCase="true" />
        <add input="{HTTP_HOST}" pattern="localhost" negate="true" />
      </conditions>
      <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent" />
    </rule>
  </rules>
</rewrite>
-->
```

#### **HSTS Header - کامنت شد:**
```xml
<!--🚫 DISABLED IN DEVELOPMENT: HSTS disabled for local development-->
<!--<add name="Strict-Transport-Security" value="max-age=31536000; includeSubDomains" />-->
```

---

### **2. Startup.Auth.cs**

#### **CookieSecure - شرطی برای Development:**
```csharp
// 🚫 DEVELOPMENT: CookieSecure = None for local development (HTTP)
var isDevelopment = ConfigurationManager.AppSettings["Environment"]?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;
var cookieSecure = isDevelopment ? CookieSecureOption.None : CookieSecureOption.Always;

app.UseCookieAuthentication(new CookieAuthenticationOptions
{
    // ...
    CookieSecure = cookieSecure, // HTTPS Only in Production, HTTP allowed in Development
    // ...
});
```

---

## 🎯 **نتیجه**

حالا در محیط Development:
- ✅ HTTP روی localhost کار می‌کند
- ✅ هیچ Redirect به HTTPS وجود ندارد
- ✅ Cookie ها در HTTP کار می‌کنند
- ✅ HSTS header غیرفعال است

---

## 🔄 **برای Production**

وقتی می‌خواهید به Production بروید:

1. در `Web.config`:
   - کامنت را از `<rewrite>` بردارید
   - کامنت را از HSTS header بردارید

2. در `Startup.Auth.cs`:
   - `Environment` را به `Production` تغییر دهید
   - یا `CookieSecureOption.Always` را مستقیماً تنظیم کنید

---

**تاریخ به‌روزرسانی:** 2024  
**نسخه:** 1.0

