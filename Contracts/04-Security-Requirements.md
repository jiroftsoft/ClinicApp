# 🔒 **الزامات امنیتی - ClinicApp**

## 📋 **اصول امنیتی کلی**

### **1️⃣ Defense in Depth**
- **Layer 1**: Network Security (Firewall, SSL/TLS)
- **Layer 2**: Application Security (Authentication, Authorization)
- **Layer 3**: Data Security (Encryption, Access Control)
- **Layer 4**: Database Security (Row-level Security, Audit)

### **2️⃣ Zero Trust Architecture**
- **Never Trust, Always Verify**: هر درخواست باید تأیید شود
- **Least Privilege**: حداقل دسترسی لازم
- **Continuous Monitoring**: نظارت مداوم

### **3️⃣ Healthcare Data Protection**
- **HIPAA Compliance**: حفاظت از اطلاعات پزشکی
- **Data Encryption**: رمزنگاری داده‌های حساس
- **Audit Trail**: ردیابی کامل عملیات

---

## 🔐 **Authentication & Authorization**

### **1️⃣ Authentication Requirements**
```csharp
// ✅ صحیح - Strong Authentication
[Authorize]
public class SecureController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<ActionResult> CreatePatient(CreatePatientRequest request)
    {
        // Implementation
    }
}

// ❌ غلط - بدون Authentication
public class InsecureController : Controller
{
    [HttpPost]
    public async Task<ActionResult> CreatePatient(CreatePatientRequest request)
    {
        // Implementation - خطرناک!
    }
}
```

### **2️⃣ Role-Based Access Control (RBAC)**
```csharp
// تعریف نقش‌های سیستم
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Doctor = "Doctor";
    public const string Receptionist = "Receptionist";
    public const string Patient = "Patient";
}

// استفاده از نقش‌ها
[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller { }

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Doctor}")]
public class MedicalController : Controller { }
```

### **3️⃣ Permission-Based Authorization**
```csharp
// تعریف مجوزها
public static class Permissions
{
    public const string ViewPatients = "ViewPatients";
    public const string CreatePatients = "CreatePatients";
    public const string EditPatients = "EditPatients";
    public const string DeletePatients = "DeletePatients";
}

// استفاده از مجوزها
[Authorize(Policy = Permissions.ViewPatients)]
public async Task<ActionResult> ViewPatients() { }
```

---

## 🛡️ **Input Validation & Sanitization**

### **1️⃣ Server-Side Validation**
```csharp
// ✅ صحیح - Comprehensive Validation
public class CreatePatientRequest
{
    [Required(ErrorMessage = "نام الزامی است")]
    [StringLength(100, ErrorMessage = "نام نمی‌تواند بیش از 100 کاراکتر باشد")]
    [RegularExpression(@"^[\u0600-\u06FF\s]+$", ErrorMessage = "نام باید فارسی باشد")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "کد ملی الزامی است")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید عدد باشد")]
    public string NationalCode { get; set; }

    [Required(ErrorMessage = "تاریخ تولد الزامی است")]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }
}

// ❌ غلط - بدون Validation
public class InsecureRequest
{
    public string FirstName { get; set; } // بدون validation
    public string NationalCode { get; set; } // بدون validation
}
```

### **2️⃣ SQL Injection Prevention**
```csharp
// ✅ صحیح - Parameterized Queries
var patients = await _context.Patients
    .Where(p => p.NationalCode == nationalCode)
    .ToListAsync();

// ❌ غلط - String Concatenation
var sql = $"SELECT * FROM Patients WHERE NationalCode = '{nationalCode}'";
var patients = _context.Database.SqlQuery<Patient>(sql).ToList();
```

### **3️⃣ XSS Prevention**
```csharp
// ✅ صحیح - HTML Encoding
@Html.DisplayFor(model => model.FirstName)
@Html.TextBoxFor(model => model.FirstName)

// ❌ غلط - Raw HTML
@Html.Raw(model.FirstName)
```

---

## 🔒 **Data Protection**

### **1️⃣ Encryption at Rest**
```csharp
// رمزنگاری داده‌های حساس
public class Patient
{
    public int Id { get; set; }
    
    [Encrypted] // Custom Attribute
    public string NationalCode { get; set; }
    
    [Encrypted]
    public string PhoneNumber { get; set; }
    
    public string FirstName { get; set; } // غیرحساس
}
```

### **2️⃣ Encryption in Transit**
```csharp
// ✅ صحیح - HTTPS Only
[RequireHttps]
public class SecureController : Controller { }

// Web.config
<system.webServer>
    <rewrite>
        <rules>
            <rule name="Redirect to HTTPS" stopProcessing="true">
                <match url="(.*)" />
                <conditions>
                    <add input="{HTTPS}" pattern="off" ignoreCase="true" />
                </conditions>
                <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" 
                        redirectType="Permanent" />
            </rule>
        </rules>
    </rewrite>
</system.webServer>
```

### **3️⃣ Sensitive Data Masking**
```csharp
// نمایش داده‌های حساس به صورت ماسک شده
public string GetMaskedNationalCode(string nationalCode)
{
    if (string.IsNullOrEmpty(nationalCode) || nationalCode.Length != 10)
        return nationalCode;
    
    return $"{nationalCode.Substring(0, 3)}***{nationalCode.Substring(7)}";
}

// نتیجه: 123***7890
```

---

## 🚨 **Security Headers**

### **1️⃣ HTTP Security Headers**
```xml
<!-- Web.config -->
<system.webServer>
    <httpProtocol>
        <customHeaders>
            <!-- HSTS -->
            <add name="Strict-Transport-Security" 
                 value="max-age=31536000; includeSubDomains" />
            
            <!-- X-Frame-Options -->
            <add name="X-Frame-Options" value="DENY" />
            
            <!-- X-Content-Type-Options -->
            <add name="X-Content-Type-Options" value="nosniff" />
            
            <!-- X-XSS-Protection -->
            <add name="X-XSS-Protection" value="1; mode=block" />
            
            <!-- Content Security Policy -->
            <add name="Content-Security-Policy" 
                 value="default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';" />
            
            <!-- Referrer Policy -->
            <add name="Referrer-Policy" value="strict-origin-when-cross-origin" />
        </customHeaders>
    </httpProtocol>
</system.webServer>
```

### **2️⃣ Cookie Security**
```csharp
// Startup.Auth.cs
app.UseCookieAuthentication(new CookieAuthenticationOptions
{
    CookieName = "ClinicAppAuth",
    CookieSecure = CookieSecureOption.Always, // HTTPS Only
    CookieHttpOnly = true, // Prevent XSS
    CookieSameSite = SameSiteMode.Strict, // CSRF Protection
    ExpireTimeSpan = TimeSpan.FromHours(8), // Session Timeout
    SlidingExpiration = true
});
```

---

## 🔍 **Audit & Logging**

### **1️⃣ Security Event Logging**
```csharp
// Logging Security Events
public class SecurityLogger
{
    private readonly ILogger _logger;
    
    public void LogLoginAttempt(string userId, bool success, string ipAddress)
    {
        _logger.Information("ورود کاربر {UserId} - موفق: {Success} - IP: {IP}", 
            userId, success, ipAddress);
    }
    
    public void LogDataAccess(string userId, string entityType, string action)
    {
        _logger.Information("دسترسی به داده - کاربر: {UserId} - نوع: {EntityType} - عمل: {Action}", 
            userId, entityType, action);
    }
    
    public void LogSecurityViolation(string userId, string violation, string details)
    {
        _logger.Warning("نقض امنیتی - کاربر: {UserId} - نوع: {Violation} - جزئیات: {Details}", 
            userId, violation, details);
    }
}
```

### **2️⃣ Audit Trail Implementation**
```csharp
// Audit Trail برای Entity Changes
public class AuditService
{
    public async Task LogEntityChangeAsync<T>(T entity, string action, string userId)
    {
        var audit = new AuditLog
        {
            EntityType = typeof(T).Name,
            EntityId = GetEntityId(entity),
            Action = action,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            Changes = SerializeChanges(entity)
        };
        
        await _context.AuditLogs.AddAsync(audit);
        await _context.SaveChangesAsync();
    }
}
```

---

## 🚫 **Security Anti-Patterns**

### **1️⃣ Common Vulnerabilities**
```csharp
// ❌ غلط - Hardcoded Secrets
public class DatabaseConfig
{
    public string ConnectionString = "Server=localhost;Database=ClinicApp;User=admin;Password=123456";
}

// ✅ صحیح - Configuration
public class DatabaseConfig
{
    public string ConnectionString => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
}
```

### **2️⃣ Insecure Direct Object References**
```csharp
// ❌ غلط - Direct Access
public async Task<ActionResult> GetPatient(int id)
{
    var patient = await _context.Patients.FindAsync(id);
    return View(patient); // خطرناک - دسترسی مستقیم
}

// ✅ صحیح - Authorization Check
public async Task<ActionResult> GetPatient(int id)
{
    var patient = await _context.Patients.FindAsync(id);
    
    if (patient == null)
        return NotFound();
    
    // بررسی مجوز دسترسی
    if (!await _authorizationService.CanAccessPatientAsync(User.Identity.Name, patient.Id))
        return Forbid();
    
    return View(patient);
}
```

### **3️⃣ Insecure File Upload**
```csharp
// ❌ غلط - بدون Validation
[HttpPost]
public async Task<ActionResult> UploadFile(HttpPostedFileBase file)
{
    var fileName = Path.GetFileName(file.FileName);
    var path = Path.Combine(Server.MapPath("~/uploads/"), fileName);
    file.SaveAs(path);
    return Json(new { success = true });
}

// ✅ صحیح - Secure Upload
[HttpPost]
public async Task<ActionResult> UploadFile(HttpPostedFileBase file)
{
    // Validation
    if (file == null || file.ContentLength == 0)
        return Json(new { success = false, message = "فایل انتخاب نشده" });
    
    // File Type Validation
    var allowedTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
    if (!allowedTypes.Contains(file.ContentType))
        return Json(new { success = false, message = "نوع فایل مجاز نیست" });
    
    // File Size Validation
    if (file.ContentLength > 5 * 1024 * 1024) // 5MB
        return Json(new { success = false, message = "حجم فایل بیش از حد مجاز" });
    
    // Secure File Name
    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
    var path = Path.Combine(Server.MapPath("~/uploads/"), fileName);
    
    file.SaveAs(path);
    return Json(new { success = true, fileName = fileName });
}
```

---

## 🔧 **Security Testing**

### **1️⃣ Penetration Testing Checklist**
- [ ] SQL Injection Testing
- [ ] XSS Testing
- [ ] CSRF Testing
- [ ] Authentication Bypass Testing
- [ ] Authorization Testing
- [ ] File Upload Testing
- [ ] Session Management Testing

### **2️⃣ Security Code Review**
- [ ] Input Validation Review
- [ ] Authentication Review
- [ ] Authorization Review
- [ ] Error Handling Review
- [ ] Logging Review
- [ ] Configuration Review

---

## ✅ **Security Checklist**

### **قبل از Deploy:**
- [ ] آیا تمام Input ها Validate شده‌اند؟
- [ ] آیا Authentication و Authorization صحیح است؟
- [ ] آیا Security Headers تنظیم شده‌اند؟
- [ ] آیا HTTPS فعال است؟
- [ ] آیا Sensitive Data رمزنگاری شده؟
- [ ] آیا Audit Logging فعال است؟
- [ ] آیا Security Testing انجام شده؟

### **بعد از Deploy:**
- [ ] آیا Security Monitoring فعال است؟
- [ ] آیا Log Analysis انجام می‌شود؟
- [ ] آیا Security Updates به‌روز هستند؟
- [ ] آیا Backup Strategy امن است؟
- [ ] آیا Incident Response Plan آماده است؟
