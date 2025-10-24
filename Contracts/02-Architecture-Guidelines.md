# 🏗️ **راهنمای معماری - ClinicApp**

## 📋 **اصول معماری کلی**

### **1️⃣ Clean Architecture Pattern**
```
Presentation Layer (MVC Controllers + Views)
    ↓
Business Logic Layer (Services)
    ↓
Data Access Layer (Repositories)
    ↓
Database Layer (Entity Framework)
```

### **2️⃣ Separation of Concerns**
- **Controllers**: فقط HTTP handling و ViewModel mapping
- **Services**: Business logic و Domain rules
- **Repositories**: Data access و CRUD operations
- **Entities**: Domain models با ISoftDelete و ITrackable

### **3️⃣ Dependency Injection (Unity Container)**
```csharp
// UnityConfig.cs
container.RegisterType<IService, ServiceImplementation>();
container.RegisterType<IRepository, RepositoryImplementation>();
```

---

## 🎯 **الگوهای طراحی اجباری**

### **1️⃣ Repository Pattern**
```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

### **2️⃣ Service Layer Pattern**
```csharp
public interface IService
{
    Task<ServiceResult<T>> ProcessAsync<T>(T request);
    Task<bool> ValidateAsync(object entity);
}
```

### **3️⃣ ViewModel Pattern**
```csharp
public class EntityViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    // فقط فیلدهای مورد نیاز UI
}
```

---

## 🔧 **استانداردهای کدنویسی**

### **1️⃣ Naming Conventions**
```csharp
// Classes: PascalCase
public class UserService { }

// Methods: PascalCase
public async Task<User> GetUserByIdAsync(int id) { }

// Properties: PascalCase
public string FirstName { get; set; }

// Private fields: camelCase
private readonly ILogger _logger;
```

### **2️⃣ Async/Await Pattern**
```csharp
// ✅ صحیح
public async Task<User> GetUserAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// ❌ غلط
public User GetUser(int id)
{
    return _repository.GetById(id);
}
```

### **3️⃣ Error Handling**
```csharp
try
{
    // Business logic
    var result = await _service.ProcessAsync(request);
    return Json(new { success = true, data = result });
}
catch (Exception ex)
{
    _logger.Error(ex, "خطا در پردازش درخواست");
    return Json(new { success = false, message = "خطای سیستم" });
}
```

---

## 🛡️ **Security Guidelines**

### **1️⃣ Authentication & Authorization**
```csharp
[Authorize]
[Authorize(Roles = "Admin,Doctor")]
public class SecureController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(CreateViewModel model)
    {
        // Implementation
    }
}
```

### **2️⃣ Input Validation**
```csharp
[Required(ErrorMessage = "نام الزامی است")]
[MaxLength(100, ErrorMessage = "نام نمی‌تواند بیش از 100 کاراکتر باشد")]
public string Name { get; set; }
```

### **3️⃣ SQL Injection Prevention**
```csharp
// ✅ صحیح - استفاده از Parameterized Queries
var users = await _context.Users
    .Where(u => u.Name == searchTerm)
    .ToListAsync();

// ❌ غلط - String Concatenation
var sql = $"SELECT * FROM Users WHERE Name = '{searchTerm}'";
```

---

## 📊 **Database Guidelines**

### **1️⃣ Entity Design**
```csharp
public class Entity : ISoftDelete, ITrackable
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string DeletedByUserId { get; set; }
    
    // ITrackable
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedByUserId { get; set; }
}
```

### **2️⃣ Decimal Precision (Money Fields)**
```csharp
// ✅ صحیح - decimal(18,0) برای ریال
Property(e => e.Price)
    .HasPrecision(18, 0);

// ❌ غلط - decimal(18,4) برای ریال
Property(e => e.Price)
    .HasPrecision(18, 4);
```

### **3️⃣ Indexing Strategy**
```csharp
// Single Column Indexes
Property(e => e.Name)
    .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Entity_Name")));

// Composite Indexes
HasIndex(e => new { e.IsDeleted, e.CreatedAt })
    .HasName("IX_Entity_IsDeleted_CreatedAt");
```

---

## 🎨 **Frontend Guidelines**

### **1️⃣ Persian RTL Support**
```html
<html dir="rtl" lang="fa">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
</head>
```

### **2️⃣ AJAX Pattern**
```javascript
// ServiceResult<T> Pattern
function makeAjaxRequest(url, data) {
    return $.ajax({
        url: url,
        type: 'POST',
        data: data,
        dataType: 'json'
    }).done(function(response) {
        if (response.success) {
            // Handle success
        } else {
            // Handle error
        }
    });
}
```

### **3️⃣ Form Validation**
```javascript
// Client-side validation
$('#form').validate({
    rules: {
        name: { required: true, maxlength: 100 },
        email: { required: true, email: true }
    },
    messages: {
        name: "نام الزامی است",
        email: "ایمیل معتبر وارد کنید"
    }
});
```

---

## 📝 **Documentation Standards**

### **1️⃣ XML Documentation**
```csharp
/// <summary>
/// دریافت کاربر بر اساس شناسه
/// </summary>
/// <param name="id">شناسه کاربر</param>
/// <returns>اطلاعات کاربر</returns>
public async Task<User> GetUserByIdAsync(int id)
{
    // Implementation
}
```

### **2️⃣ Code Comments**
```csharp
// بررسی وجود کاربر در سیستم
var userExists = await _context.Users.AnyAsync(u => u.Id == userId);

// محاسبه قیمت نهایی با در نظر گیری تخفیف
var finalPrice = basePrice * (1 - discountPercent / 100);
```

---

## ✅ **Checklist برای هر تغییر**

### **قبل از شروع:**
- [ ] آیا الگوی معماری رعایت شده؟
- [ ] آیا Security guidelines اعمال شده؟
- [ ] آیا Database guidelines رعایت شده؟
- [ ] آیا Frontend guidelines در نظر گرفته شده؟

### **بعد از تکمیل:**
- [ ] آیا کد قابل نگهداری است؟
- [ ] آیا Performance بهینه است؟
- [ ] آیا Security حفظ شده؟
- [ ] آیا Documentation کامل است؟
