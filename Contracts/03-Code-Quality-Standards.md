# 🎯 **استانداردهای کیفیت کد - ClinicApp**

## 📋 **اصول کلی کیفیت کد**

### **1️⃣ SOLID Principles**
- **S**ingle Responsibility: هر کلاس یک مسئولیت
- **O**pen/Closed: باز برای توسعه، بسته برای تغییر
- **L**iskov Substitution: قابلیت جایگزینی
- **I**nterface Segregation: جداسازی interface ها
- **D**ependency Inversion: وابستگی به abstraction

### **2️⃣ DRY Principle (Don't Repeat Yourself)**
```csharp
// ❌ غلط - تکرار کد
public async Task<User> GetUserByIdAsync(int id)
{
    var user = await _context.Users.FindAsync(id);
    if (user == null) throw new NotFoundException();
    return user;
}

public async Task<Department> GetDepartmentByIdAsync(int id)
{
    var dept = await _context.Departments.FindAsync(id);
    if (dept == null) throw new NotFoundException();
    return dept;
}

// ✅ صحیح - استفاده از Generic Repository
public async Task<T> GetByIdAsync<T>(int id) where T : class
{
    var entity = await _context.Set<T>().FindAsync(id);
    if (entity == null) throw new NotFoundException();
    return entity;
}
```

### **3️⃣ KISS Principle (Keep It Simple, Stupid)**
```csharp
// ❌ پیچیده
public bool IsUserEligibleForService(User user, Service service)
{
    if (user != null && user.IsActive && !user.IsDeleted && 
        service != null && service.IsActive && !service.IsDeleted &&
        user.Age >= 18 && user.InsuranceStatus == InsuranceStatus.Active)
    {
        return true;
    }
    return false;
}

// ✅ ساده
public bool IsUserEligibleForService(User user, Service service)
{
    return user?.IsActive == true && 
           service?.IsActive == true && 
           user.Age >= 18;
}
```

---

## 🔍 **Code Review Checklist**

### **1️⃣ Naming & Readability**
- [ ] نام‌های متغیرها واضح و معنادار هستند؟
- [ ] نام‌های متدها فعل + اسم هستند؟
- [ ] نام‌های کلاس‌ها اسم هستند؟
- [ ] کد قابل خواندن و فهم است؟

### **2️⃣ Performance**
- [ ] آیا N+1 Query Problem وجود دارد؟
- [ ] آیا از Async/Await استفاده شده؟
- [ ] آیا Memory Leak وجود دارد؟
- [ ] آیا Database queries بهینه هستند؟

### **3️⃣ Security**
- [ ] آیا Input Validation انجام شده؟
- [ ] آیا SQL Injection محافظت شده؟
- [ ] آیا XSS محافظت شده؟
- [ ] آیا CSRF محافظت شده؟

### **4️⃣ Error Handling**
- [ ] آیا Exception Handling مناسب است؟
- [ ] آیا Logging انجام شده؟
- [ ] آیا User-friendly messages ارائه شده؟
- [ ] آیا Rollback mechanism وجود دارد؟

---

## 🛠️ **Best Practices**

### **1️⃣ Exception Handling**
```csharp
// ✅ صحیح
try
{
    var result = await _service.ProcessAsync(request);
    return Json(new { success = true, data = result });
}
catch (ValidationException ex)
{
    _logger.Warning(ex, "خطا در اعتبارسنجی: {Message}", ex.Message);
    return Json(new { success = false, message = ex.Message });
}
catch (Exception ex)
{
    _logger.Error(ex, "خطای غیرمنتظره در پردازش درخواست");
    return Json(new { success = false, message = "خطای سیستم" });
}

// ❌ غلط
try
{
    var result = await _service.ProcessAsync(request);
    return Json(new { success = true, data = result });
}
catch (Exception ex)
{
    // خالی - هیچ کاری نمی‌کند
}
```

### **2️⃣ Logging Standards**
```csharp
// ✅ صحیح - Structured Logging
_logger.Information("کاربر {UserId} با موفقیت وارد شد", userId);
_logger.Warning("تلاش ناموفق ورود برای کاربر {UserId}", userId);
_logger.Error(ex, "خطا در پردازش درخواست کاربر {UserId}", userId);

// ❌ غلط - String Concatenation
_logger.Information("کاربر " + userId + " وارد شد");
```

### **3️⃣ Database Operations**
```csharp
// ✅ صحیح - استفاده از Transaction
using (var transaction = _context.Database.BeginTransaction())
{
    try
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}

// ❌ غلط - بدون Transaction
await _context.Users.AddAsync(user);
await _context.SaveChangesAsync();
```

---

## 🧪 **Testing Standards**

### **1️⃣ Unit Testing**
```csharp
[Test]
public async Task GetUserByIdAsync_ValidId_ReturnsUser()
{
    // Arrange
    var userId = 1;
    var expectedUser = new User { Id = userId, Name = "Test User" };
    _mockRepository.Setup(r => r.GetByIdAsync(userId))
                  .ReturnsAsync(expectedUser);

    // Act
    var result = await _userService.GetUserByIdAsync(userId);

    // Assert
    Assert.That(result, Is.EqualTo(expectedUser));
    _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
}
```

### **2️⃣ Integration Testing**
```csharp
[Test]
public async Task CreateUser_ValidData_UserCreated()
{
    // Arrange
    var user = new CreateUserRequest { Name = "Test User", Email = "test@example.com" };

    // Act
    var result = await _userController.Create(user);

    // Assert
    Assert.That(result, Is.InstanceOf<CreatedResult>());
    var createdUser = await _context.Users.FirstAsync(u => u.Name == "Test User");
    Assert.That(createdUser, Is.Not.Null);
}
```

---

## 📊 **Performance Guidelines**

### **1️⃣ Database Optimization**
```csharp
// ✅ صحیح - استفاده از Include
var users = await _context.Users
    .Include(u => u.Department)
    .Include(u => u.Insurance)
    .Where(u => u.IsActive)
    .ToListAsync();

// ❌ غلط - N+1 Query Problem
var users = await _context.Users.Where(u => u.IsActive).ToListAsync();
foreach (var user in users)
{
    var department = await _context.Departments.FindAsync(user.DepartmentId);
    // این باعث N+1 Query می‌شود
}
```

### **2️⃣ Memory Management**
```csharp
// ✅ صحیح - استفاده از using
using (var stream = new FileStream(path, FileMode.Open))
{
    // Process file
}

// ❌ غلط - عدم dispose
var stream = new FileStream(path, FileMode.Open);
// Process file
// stream.Dispose() فراموش شده
```

### **3️⃣ Caching Strategy**
```csharp
// ✅ صحیح - استفاده از Memory Cache
public async Task<List<Department>> GetDepartmentsAsync()
{
    const string cacheKey = "departments";
    
    if (_cache.TryGetValue(cacheKey, out List<Department> departments))
    {
        return departments;
    }
    
    departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
    _cache.Set(cacheKey, departments, TimeSpan.FromMinutes(30));
    
    return departments;
}
```

---

## 🔒 **Security Standards**

### **1️⃣ Input Validation**
```csharp
// ✅ صحیح - Server-side validation
[HttpPost]
public async Task<ActionResult> CreateUser(CreateUserRequest request)
{
    if (!ModelState.IsValid)
    {
        return Json(new { success = false, errors = ModelState });
    }
    
    // Process request
}

// ❌ غلط - فقط Client-side validation
// Client-side validation قابل دور زدن است
```

### **2️⃣ SQL Injection Prevention**
```csharp
// ✅ صحیح - Parameterized Queries
var users = await _context.Users
    .Where(u => u.Name.Contains(searchTerm))
    .ToListAsync();

// ❌ غلط - String Concatenation
var sql = $"SELECT * FROM Users WHERE Name LIKE '%{searchTerm}%'";
var users = _context.Database.SqlQuery<User>(sql).ToList();
```

### **3️⃣ XSS Prevention**
```csharp
// ✅ صحیح - HTML Encoding
@Html.DisplayFor(model => model.Name)

// ❌ غلط - Raw HTML
@Html.Raw(model.Name)
```

---

## 📝 **Documentation Standards**

### **1️⃣ XML Documentation**
```csharp
/// <summary>
/// ایجاد کاربر جدید در سیستم
/// </summary>
/// <param name="request">اطلاعات کاربر جدید</param>
/// <returns>نتیجه عملیات</returns>
/// <exception cref="ValidationException">در صورت نامعتبر بودن اطلاعات</exception>
public async Task<ServiceResult<User>> CreateUserAsync(CreateUserRequest request)
{
    // Implementation
}
```

### **2️⃣ Code Comments**
```csharp
// بررسی وجود کاربر در سیستم
var existingUser = await _context.Users
    .FirstOrDefaultAsync(u => u.Email == request.Email);

if (existingUser != null)
{
    // کاربر با این ایمیل قبلاً وجود دارد
    return ServiceResult<User>.Error("کاربر با این ایمیل قبلاً ثبت شده است");
}
```

---

## ✅ **Quality Checklist**

### **قبل از Commit:**
- [ ] آیا کد قابل خواندن است؟
- [ ] آیا Performance بهینه است؟
- [ ] آیا Security رعایت شده؟
- [ ] آیا Error Handling مناسب است؟
- [ ] آیا Logging انجام شده؟
- [ ] آیا Documentation کامل است؟
- [ ] آیا Tests نوشته شده؟

### **بعد از Commit:**
- [ ] آیا Build موفق است؟
- [ ] آیا Tests پاس می‌شوند؟
- [ ] آیا Code Review انجام شده؟
- [ ] آیا Performance تست شده؟
- [ ] آیا Security scan انجام شده؟
