# راهنمای کانفیگ Web API - Production Ready

## 📋 فهرست مطالب
1. [معرفی](#معرفی)
2. [کانفیگ انجام شده](#کانفیگ-انجام-شده)
3. [تست کانفیگ](#تست-کانفیگ)
4. [استفاده از Web API](#استفاده-از-web-api)

---

## معرفی

Web API Configuration برای پروژه ClinicApp به درستی کانفیگ شده است. این کانفیگ امکان استفاده از ASP.NET Web API را فراهم می‌کند.

---

## کانفیگ انجام شده

### ✅ 1. فایل WebApiConfig.cs

**مسیر**: `App_Start/WebApiConfig.cs`

```csharp
namespace ClinicApp
{
    using System.Web.Http;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // مسیر پیش‌فرض Web API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
```

**ویژگی‌ها**:
- ✅ پشتیبانی از Attribute Routing (`MapHttpAttributeRoutes`)
- ✅ Route پیش‌فرض: `api/{controller}/{id}`
- ✅ پشتیبانی از Optional Parameter

---

### ✅ 2. ثبت در Global.asax.cs

**مسیر**: `Global.asax.cs`

```csharp
using System.Web.Http;

protected void Application_Start()
{
    // ...
    AreaRegistration.RegisterAllAreas();
    
    // ✅ ثبت Web API Configuration
    GlobalConfiguration.Configure(WebApiConfig.Register);
    
    // ...
}
```

**ویژگی‌ها**:
- ✅ ثبت شده در `Application_Start`
- ✅ استفاده از `GlobalConfiguration.Configure`
- ✅ ترتیب صحیح: قبل از RouteConfig

---

## تست کانفیگ

### 1. بررسی کامپایل

```bash
# Build پروژه
dotnet build
# یا در Visual Studio: Build > Build Solution
```

### 2. تست Route

برای تست Web API، می‌توانید یک Web API Controller ساده بسازید:

```csharp
using System.Web.Http;

namespace ClinicApp.Controllers.Api
{
    [RoutePrefix("api/test")]
    public class TestApiController : ApiController
    {
        [HttpGet]
        [Route("ping")]
        public IHttpActionResult Ping()
        {
            return Ok(new { message = "Web API is working!", timestamp = DateTime.Now });
        }
    }
}
```

**تست URL**: `http://localhost:3560/api/test/ping`

---

## استفاده از Web API

### ساخت Web API Controller

```csharp
using System.Web.Http;

namespace ClinicApp.Controllers.Api
{
    [RoutePrefix("api/blogpost")]
    public class BlogPostApiController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            // Logic here
            return Ok(new { data = "..." });
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            // Logic here
            return Ok(new { data = "..." });
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] BlogPostViewModel model)
        {
            // Logic here
            return Created("", new { data = "..." });
        }
    }
}
```

### Route Patterns

1. **Convention-based Routing**:
   - URL: `/api/{controller}/{id}`
   - مثال: `/api/blogpost/1`

2. **Attribute Routing**:
   - URL: `[RoutePrefix("api/blogpost")]` + `[Route("custom")]`
   - مثال: `/api/blogpost/custom`

---

## تفاوت MVC Controller و Web API Controller

### MVC Controller
```csharp
public class BlogPostController : Controller
{
    public ActionResult Index()
    {
        return View();
    }
}
```

### Web API Controller
```csharp
public class BlogPostApiController : ApiController
{
    public IHttpActionResult Get()
    {
        return Ok(new { data = "..." });
    }
}
```

---

## نکات مهم

1. ✅ **Web API Controller** باید از `ApiController` ارث‌بری کند
2. ✅ **MVC Controller** باید از `Controller` ارث‌بری کند
3. ✅ Web API به صورت پیش‌فرض JSON برمی‌گرداند
4. ✅ MVC Controller می‌تواند View یا JSON برگرداند

---

## خلاصه

- ✅ WebApiConfig.cs ایجاد شده
- ✅ در Global.asax.cs ثبت شده
- ✅ Route Configuration صحیح است
- ✅ پشتیبانی از Attribute Routing
- ✅ آماده برای استفاده در Production

---

**تاریخ ایجاد**: 2024  
**نسخه**: 1.0  
**نویسنده**: ClinicApp Development Team

