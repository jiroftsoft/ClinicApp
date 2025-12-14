# 📊 تحلیل و طراحی: Template هوشمند با شخصی‌سازی پیشرفته

## 📌 مقدمه

این سند تحلیل کامل برای پیاده‌سازی سیستم Template هوشمند با قابلیت‌های شخصی‌سازی پیشرفته برای Newsletter است.

**هدف:** ایجاد سیستم Template قدرتمند که امکان شخصی‌سازی پیشرفته، شرطی‌سازی، و استفاده از متغیرهای داینامیک را فراهم کند.

---

## 🎯 نیازمندی‌های کاربر (User Requirements)

### 1. متغیرهای پیشرفته
- **متغیرهای پایه:** `{{FullName}}`, `{{Email}}`, `{{UnsubscribeUrl}}`
- **متغیرهای پیشرفته:**
  - `{{FirstName}}` - نام کوچک
  - `{{LastName}}` - نام خانوادگی
  - `{{PhoneNumber}}` - شماره تماس
  - `{{SubscriptionDate}}` - تاریخ عضویت
  - `{{Category}}` - دسته‌بندی خبرنامه
  - `{{CurrentDate}}` - تاریخ امروز (شمسی)
  - `{{CurrentTime}}` - زمان فعلی
  - `{{ClinicName}}` - نام کلینیک
  - `{{ClinicPhone}}` - تلفن کلینیک
  - `{{ClinicAddress}}` - آدرس کلینیک

### 2. شرطی‌سازی (Conditional Logic)
- **IF/ELSE:** نمایش محتوای شرطی بر اساس شرایط
- **مثال:**
  ```
  {{#if Category == "Medical"}}
      محتوای پزشکی
  {{#else}}
      محتوای عمومی
  {{/if}}
  ```

### 3. حلقه‌ها (Loops)
- **FOR:** تکرار محتوا برای لیست‌ها
- **مثال:**
  ```
  {{#for items}}
      {{ItemName}} - {{ItemPrice}}
  {{/for}}
  ```

### 4. پیش‌نمایش هوشمند
- پیش‌نمایش با داده‌های واقعی
- پیش‌نمایش با داده‌های نمونه
- پیش‌نمایش برای دسته‌بندی‌های مختلف

### 5. Template Builder UI
- Drag & Drop برای المان‌ها
- Insert Variable Button
- Live Preview
- Syntax Highlighting

---

## 🏗️ معماری فنی (Technical Architecture)

### 1. Template Engine
- **Parser:** تجزیه Template و شناسایی متغیرها و دستورات
- **Renderer:** Render کردن Template با داده‌های واقعی
- **Validator:** اعتبارسنجی Syntax Template

### 2. متغیرهای پشتیبانی شده

```csharp
public class TemplateVariables
{
    // User Variables
    public string FullName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime? SubscriptionDate { get; set; }
    public string Category { get; set; }
    
    // System Variables
    public DateTime CurrentDate { get; set; }
    public string CurrentTime { get; set; }
    public string UnsubscribeUrl { get; set; }
    
    // Clinic Variables
    public string ClinicName { get; set; }
    public string ClinicPhone { get; set; }
    public string ClinicAddress { get; set; }
    public string ClinicEmail { get; set; }
}
```

### 3. Syntax Template

#### متغیرهای ساده:
```
{{VariableName}}
```

#### شرطی‌سازی:
```
{{#if Condition}}
    Content
{{#else}}
    Alternative Content
{{/if}}
```

#### حلقه:
```
{{#for Collection}}
    {{ItemProperty}}
{{/for}}
```

---

## 📋 فازبندی پیاده‌سازی

### Phase 1: متغیرهای پیشرفته (1-2 روز)
- [ ] اضافه کردن متغیرهای جدید به `TemplateVariables`
- [ ] به‌روزرسانی `RenderTemplateAsync` برای پشتیبانی از متغیرهای جدید
- [ ] اضافه کردن UI برای نمایش لیست متغیرهای موجود
- [ ] تست متغیرهای جدید

### Phase 2: شرطی‌سازی (2-3 روز)
- [ ] پیاده‌سازی Parser برای دستورات `{{#if}}`
- [ ] پیاده‌سازی Renderer برای شرطی‌سازی
- [ ] پشتیبانی از عملگرهای منطقی (`==`, `!=`, `>`, `<`, `&&`, `||`)
- [ ] تست شرطی‌سازی

### Phase 3: حلقه‌ها (2-3 روز)
- [ ] پیاده‌سازی Parser برای دستورات `{{#for}}`
- [ ] پیاده‌سازی Renderer برای حلقه‌ها
- [ ] پشتیبانی از Index و Count در حلقه
- [ ] تست حلقه‌ها

### Phase 4: پیش‌نمایش هوشمند (1-2 روز)
- [ ] ایجاد Action `Preview` با داده‌های واقعی
- [ ] ایجاد Action `PreviewWithSample` با داده‌های نمونه
- [ ] بهبود UI پیش‌نمایش
- [ ] تست پیش‌نمایش

### Phase 5: Template Builder UI (2-3 روز)
- [ ] اضافه کردن دکمه "Insert Variable"
- [ ] اضافه کردن Syntax Highlighting
- [ ] اضافه کردن Live Preview
- [ ] بهبود UX

---

## 🔧 پیاده‌سازی فنی

### 1. Template Parser

```csharp
public class TemplateParser
{
    public TemplateNode Parse(string template)
    {
        // Parse template into AST
    }
}

public abstract class TemplateNode
{
    public abstract string Render(Dictionary<string, object> variables);
}

public class VariableNode : TemplateNode
{
    public string VariableName { get; set; }
    // ...
}

public class ConditionalNode : TemplateNode
{
    public string Condition { get; set; }
    public TemplateNode TrueContent { get; set; }
    public TemplateNode FalseContent { get; set; }
    // ...
}

public class LoopNode : TemplateNode
{
    public string CollectionName { get; set; }
    public TemplateNode LoopContent { get; set; }
    // ...
}
```

### 2. Template Renderer

```csharp
public class TemplateRenderer
{
    public string Render(TemplateNode ast, Dictionary<string, object> variables)
    {
        return ast.Render(variables);
    }
}
```

### 3. Service Layer

```csharp
public interface ISmartTemplateService
{
    Task<ServiceResult<string>> RenderTemplateAsync(
        string template, 
        Dictionary<string, object> variables);
    
    Task<ServiceResult<List<string>>> GetAvailableVariablesAsync();
    
    Task<ServiceResult<bool>> ValidateTemplateAsync(string template);
}
```

---

## 📊 Database Schema

### تغییرات مورد نیاز:
- **هیچ تغییر Schema نیاز نیست** - تمام قابلیت‌ها در لایه Service پیاده‌سازی می‌شوند

---

## ✅ Acceptance Criteria

### 1. متغیرهای پیشرفته
- ✅ تمام متغیرهای تعریف شده به درستی render می‌شوند
- ✅ در صورت نبودن متغیر، مقدار پیش‌فرض نمایش داده می‌شود
- ✅ متغیرها case-insensitive هستند

### 2. شرطی‌سازی
- ✅ دستورات `{{#if}}` به درستی parse و render می‌شوند
- ✅ عملگرهای منطقی به درستی کار می‌کنند
- ✅ Nested conditions پشتیبانی می‌شوند

### 3. حلقه‌ها
- ✅ دستورات `{{#for}}` به درستی parse و render می‌شوند
- ✅ Index و Count در حلقه در دسترس هستند
- ✅ Empty collection handling

### 4. پیش‌نمایش
- ✅ پیش‌نمایش با داده‌های واقعی کار می‌کند
- ✅ پیش‌نمایش با داده‌های نمونه کار می‌کند
- ✅ UI پیش‌نمایش responsive است

---

## 🚀 زمان‌بندی

- **Phase 1:** 1-2 روز
- **Phase 2:** 2-3 روز
- **Phase 3:** 2-3 روز
- **Phase 4:** 1-2 روز
- **Phase 5:** 2-3 روز

**کل زمان:** 8-13 روز کاری

---

## 📝 Notes

### نکات مهم:
1. تمام کدها باید طبق `DEVELOPMENT_CONTRACT.md` باشند
2. رعایت کامل اصول SRP
3. استفاده از Strongly-Typed ViewModels
4. تست کامل تمام قابلیت‌ها
5. Documentation کامل

### ریسک‌ها:
- پیچیدگی Parser و Renderer
- Performance در Template های بزرگ
- Security (XSS) در Render محتوا

### راه‌حل‌ها:
- استفاده از Caching برای Template های Render شده
- HTML Encoding برای جلوگیری از XSS
- Unit Tests برای Parser و Renderer

---

**تاریخ ایجاد:** 2025-12-12  
**نسخه:** 1.0.0

