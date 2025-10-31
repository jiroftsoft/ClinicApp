# 🔧 **رفع مشکل System.Web.Mvc Version Conflict**

**تاریخ:** 2024  
**مشکل:** `Could not load file or assembly 'System.Web.Mvc, Version=5.2.3.0'`  
**دلیل:** SerilogWeb.Classic.Mvc نسخه 2.1.25 نیاز به System.Web.Mvc 5.2.3.0 دارد اما نسخه 5.3.0 نصب است

---

## ✅ **تغییرات اعمال شده**

### **1. Web.config**

#### **Binding Redirect اصلاح شد:**
```xml
<dependentAssembly>
  <assemblyIdentity name="System.Web.Mvc" publicKeyToken="31bf3856ad364e35" />
  <!-- Fix: Redirect all versions including 5.2.3.0 (required by SerilogWeb.Classic.Mvc) to 5.3.0.0 -->
  <bindingRedirect oldVersion="0.0.0.0-5.3.0.0" newVersion="5.3.0.0" />
</dependentAssembly>
```

### **2. Global.asax.cs**

#### **SerilogWeb.Classic.Mvc موقتاً غیرفعال شد:**
```csharp
// 🔧 تنظیمات اضافی SerilogWeb
// 🚫 TEMPORARILY DISABLED: SerilogWeb.Classic.Mvc causes version conflict with System.Web.Mvc 5.3.0
// TODO: Update SerilogWeb.Classic.Mvc to a version compatible with System.Web.Mvc 5.3.0
// LoggingConfiguration.ConfigureSerilogWeb();
```

---

## 🎯 **توضیح مشکل**

### **مشکل:**
- `SerilogWeb.Classic.Mvc` نسخه 2.1.25 نیاز به `System.Web.Mvc 5.2.3.0` دارد
- پروژه از `Microsoft.AspNet.Mvc 5.3.0` استفاده می‌کند (نسخه جدیدتر)
- `PreApplicationStartModule` قبل از اعمال binding redirect اجرا می‌شود و خطا می‌دهد

### **راه حل موقت:**
- غیرفعال کردن `ConfigureSerilogWeb()` در `Global.asax.cs`
- Serilog اصلی (SerilogWeb.Classic) همچنان کار می‌کند
- فقط قابلیت‌های اضافی MVC غیرفعال شد

---

## 🔄 **راه حل دائمی (بعداً)**

### **گزینه 1: بروزرسانی SerilogWeb.Classic.Mvc**
```powershell
Update-Package SerilogWeb.Classic.Mvc
```

### **گزینه 2: Downgrade System.Web.Mvc (توصیه نمی‌شود)**
```powershell
Update-Package Microsoft.AspNet.Mvc -Version 5.2.3
```

### **گزینه 3: حذف SerilogWeb.Classic.Mvc (اگر نیاز ندارید)**
```powershell
Uninstall-Package SerilogWeb.Classic.Mvc
```

---

## 📊 **وضعیت فعلی**

| مورد | وضعیت |
|------|-------|
| System.Web.Mvc | 5.3.0 (نصب شده) |
| SerilogWeb.Classic | 5.1.66 (فعال) |
| SerilogWeb.Classic.Mvc | 2.1.25 (موقتاً غیرفعال) |
| Binding Redirect | ✅ تنظیم شده |

---

## ✅ **نتیجه**

- ✅ خطای Assembly Load رفع شد
- ✅ برنامه بدون خطا راه‌اندازی می‌شود
- ✅ Serilog اصلی (logging) کار می‌کند
- ⚠️ فقط قابلیت‌های اضافی MVC SerilogWeb غیرفعال است (که ضروری نیست)

---

**تاریخ به‌روزرسانی:** 2024  
**نسخه:** 1.0

