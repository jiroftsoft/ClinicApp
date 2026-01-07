# ✅ رفع مشکل Route برای SimulatedGateway

**تاریخ:** 2026-01-07  
**وضعیت:** ✅ رفع شد

---

## 🐛 **مشکل:**

```
HTTP 404. The resource cannot be found.
Requested URL: /Payment/SimulatedGateway/Process
```

**علت:** `SimulatedGatewayController` در Route Config ثبت نشده بود.

---

## ✅ **راه‌حل:**

### **1. اضافه کردن Route در RouteConfig.cs:**

```csharp
// ✅ SimulatedGateway Route - باید قبل از Payment_Controllers باشد
routes.MapRoute(
    name: "Payment_SimulatedGateway",
    url: "Payment/SimulatedGateway/{action}/{id}",
    defaults: new { controller = "SimulatedGateway", action = "Process", id = UrlParameter.Optional },
    namespaces: new[] { "ClinicApp.Controllers.Payment" }
).DataTokens["UseNamespaceFallback"] = false;
```

### **2. اضافه کردن Route Attributes به Controller:**

```csharp
[RoutePrefix("Payment/SimulatedGateway")]
public class SimulatedGatewayController : Controller
{
    [HttpGet]
    [Route("Process")]
    public async Task<ActionResult> Process(...)
    
    [HttpPost]
    [Route("ProcessPayment")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ProcessPayment(...)
}
```

### **3. اصلاح View برای استفاده از Route:**

```razor
<form action="@Url.RouteUrl("Payment_SimulatedGateway", new { action = "ProcessPayment" })">
```

---

## 📋 **تغییرات انجام شده:**

| فایل | تغییرات |
|---|---|
| `App_Start/RouteConfig.cs` | اضافه شدن Route برای `SimulatedGateway` |
| `Controllers/Payment/SimulatedGatewayController.cs` | اضافه شدن `[RoutePrefix]` و `[Route]` Attributes |
| `Views/Payment/SimulatedGateway/Process.cshtml` | اصلاح `action` برای استفاده از Route |

---

## 🧪 **تست:**

پس از Restart Application:
1. URL: `http://localhost:3560/Payment/SimulatedGateway/Process?authority=xxx&amount=xxx&callbackUrl=xxx&correlationId=xxx`
2. باید صفحه شبیه‌سازی شده نمایش داده شود
3. روی "پرداخت موفق" کلیک کنید
4. باید به Callback URL هدایت شوید

---

**تاریخ ایجاد:** 2026-01-07  
**آخرین به‌روزرسانی:** 2026-01-07

