# **✅ چک لیست عملی توسعه فرم‌های پیچیده**

## **🎯 هدف**
چک لیست گام‌به‌گام و عملی برای توسعه فرم‌های پیچیده با منطق پیشرفته

---

## **📋 مرحله 1: تحلیل و طراحی (Pre-Development)**

### **🔍 تحلیل نیازمندی‌ها**
- [ ] **درک منطق کسب‌وکار**: آیا منطق پیچیده‌ای وجود دارد؟
- [ ] **شناسایی وابستگی‌ها**: چه فیلدهایی به یکدیگر وابسته هستند؟
- [ ] **تعیین نوع اعتبارسنجی**: server-side، client-side، یا هر دو؟
- [ ] **شناسایی AJAX calls**: چه عملیات‌هایی نیاز به AJAX دارند؟
- [ ] **تعیین نوع تاریخ**: شمسی، میلادی، یا هر دو؟

### **🏗️ طراحی معماری**
- [ ] **تعیین ساختار فایل‌ها**: Controller، Service، Repository، ViewModel
- [ ] **طراحی ViewModel**: فیلدهای مورد نیاز و متدهای تبدیل
- [ ] **طراحی UI/UX**: ظاهر فرم و تجربه کاربری
- [ ] **تعیین استانداردهای کدنویسی**: نام‌گذاری، کامنت‌گذاری

---

## **📋 مرحله 2: توسعه Backend**

### **🎮 Controller Development**
- [ ] **ایجاد Controller اصلی**:
  ```csharp
  [Authorize(Roles = AppRoles.Admin)]
  public class [Module]Controller : Controller
  {
      private readonly I[Module]Service _service;
      private readonly ILogger<[Module]Controller> _logger;
      
      public [Module]Controller(I[Module]Service service, ILogger<[Module]Controller> logger)
      {
          _service = service;
          _logger = logger;
      }
  }
  ```

- [ ] **پیاده‌سازی Actions**:
  - [ ] `Index` - لیست
  - [ ] `Create` (GET) - نمایش فرم ایجاد
  - [ ] `Create` (POST) - پردازش فرم ایجاد
  - [ ] `Edit` (GET) - نمایش فرم ویرایش
  - [ ] `Edit` (POST) - پردازش فرم ویرایش
  - [ ] `Details` - نمایش جزئیات
  - [ ] `Delete` - حذف

- [ ] **پیاده‌سازی AJAX Actions**:
  ```csharp
  [HttpGet]
  public async Task<JsonResult> GetLookupData(string searchTerm)
  {
      try
      {
          _logger.LogInformation("🔄 Lookup request: {SearchTerm}", searchTerm);
          var result = await _service.GetLookupDataAsync(searchTerm);
          return Json(result, JsonRequestBehavior.AllowGet);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "❌ Lookup error for: {SearchTerm}", searchTerm);
          return Json(new { success = false, message = "خطا در دریافت اطلاعات" });
      }
  }
  ```

### **⚙️ Service Development**
- [ ] **ایجاد Service Interface**:
  ```csharp
  public interface I[Module]Service
  {
      Task<ServiceResult<[Module]>> CreateAsync([Module]CreateEditViewModel model);
      Task<ServiceResult<[Module]>> UpdateAsync(int id, [Module]CreateEditViewModel model);
      Task<ServiceResult<bool>> DeleteAsync(int id);
      Task<ServiceResult<[Module]>> GetByIdAsync(int id);
      Task<ServiceResult<List<[Module]LookupViewModel>>> GetLookupDataAsync(string searchTerm);
  }
  ```

- [ ] **پیاده‌سازی Service**:
  ```csharp
  public class [Module]Service : I[Module]Service
  {
      private readonly I[Module]Repository _repository;
      private readonly ILogger<[Module]Service> _logger;
      
      public async Task<ServiceResult<[Module]>> CreateAsync([Module]CreateEditViewModel model)
      {
          try
          {
              _logger.LogInformation("🔄 [Module] Create started");
              
              // Validation
              var validationResult = await ValidateModelAsync(model);
              if (!validationResult.IsValid)
              {
                  return ServiceResult<[Module]>.Failure(validationResult.Errors.First());
              }
              
              // Business Logic
              var entity = model.ToEntity();
              var result = await _repository.AddAsync(entity);
              await _repository.SaveChangesAsync();
              
              _logger.LogInformation("✅ [Module] Create successful - ID: {Id}", result.Id);
              return ServiceResult<[Module]>.Success(result);
          }
          catch (Exception ex)
          {
              _logger.LogError(ex, "💥 [Module] Create exception");
              return ServiceResult<[Module]>.Failure("خطای غیرمنتظره‌ای رخ داد");
          }
      }
  }
  ```

### **📊 Repository Development**
- [ ] **ایجاد Repository Interface**
- [ ] **پیاده‌سازی Repository**
- [ ] **بهینه‌سازی کوئری‌ها**
- [ ] **مدیریت تراکنش‌ها**

---

## **📋 مرحله 3: توسعه ViewModel**

### **📝 ViewModel Structure**
- [ ] **ایجاد ViewModel اصلی**:
  ```csharp
  public class [Module]CreateEditViewModel
  {
      // Basic Properties
      public int Id { get; set; }
      
      [Required(ErrorMessage = "نام الزامی است")]
      [StringLength(250, ErrorMessage = "نام نمی‌تواند بیش از 250 کاراکتر باشد")]
      [Display(Name = "نام")]
      public string Name { get; set; }
      
      // Persian Date Properties
      [Required(ErrorMessage = "تاریخ شروع الزامی است")]
      [PersianDate(IsRequired = true, MustBeFutureDate = false)]
      [Display(Name = "تاریخ شروع")]
      public string ValidFromShamsi { get; set; }
      
      [PersianDate(IsRequired = false, MustBeFutureDate = false)]
      [Display(Name = "تاریخ پایان")]
      public string ValidToShamsi { get; set; }
      
      // Hidden DateTime Properties
      [HiddenInput(DisplayValue = false)]
      public DateTime ValidFrom { get; set; }
      
      [HiddenInput(DisplayValue = false)]
      public DateTime ValidTo { get; set; }
      
      // Conversion Methods
      public [Module] ToEntity()
      {
          return new [Module]
          {
              Id = this.Id,
              Name = this.Name?.Trim(),
              ValidFrom = ConvertPersianToDateTime(this.ValidFromShamsi),
              ValidTo = ConvertPersianToDateTime(this.ValidToShamsi)
          };
      }
      
      public static [Module]CreateEditViewModel FromEntity([Module] entity)
      {
          return new [Module]CreateEditViewModel
          {
              Id = entity.Id,
              Name = entity.Name,
              ValidFrom = entity.ValidFrom,
              ValidTo = entity.ValidTo,
              ValidFromShamsi = entity.ValidFrom.ToPersianDateString(),
              ValidToShamsi = entity.ValidTo?.ToPersianDateString()
          };
      }
      
      private DateTime ConvertPersianToDateTime(string persianDate)
      {
          if (string.IsNullOrWhiteSpace(persianDate)) 
              return DateTime.Now.AddYears(1);
          
          try 
          { 
              return PersianDateHelper.ToGregorianDate(persianDate); 
          }
          catch 
          { 
              return DateTime.Now.AddYears(1); 
          }
      }
  }
  ```

### **🔍 Lookup ViewModels**
- [ ] **ایجاد ViewModel برای AJAX calls**:
  ```csharp
  public class [Module]LookupViewModel
  {
      public int Id { get; set; }
      public string Name { get; set; }
      public string Code { get; set; }
      public bool IsActive { get; set; }
  }
  ```

---

## **📋 مرحله 4: توسعه Frontend**

### **🎨 HTML Structure**
- [ ] **ایجاد Layout اصلی**:
  ```html
  <div class="form-container">
      <div class="form-header">
          <h2 class="form-title">
              <i class="fas fa-edit"></i>
              @(Model.Id > 0 ? "ویرایش" : "ایجاد") [Module]
          </h2>
      </div>
      
      <form method="post" class="needs-validation" novalidate>
          @Html.AntiForgeryToken()
          @Html.HiddenFor(m => m.Id)
          
          <div class="form-body">
              <!-- Form fields -->
          </div>
          
          <div class="form-footer">
              <button type="submit" class="btn btn-primary">
                  <i class="fas fa-save"></i>
                  ذخیره
              </button>
              <a href="@Url.Action("Index")" class="btn btn-secondary">
                  <i class="fas fa-arrow-right"></i>
                  بازگشت
              </a>
          </div>
      </form>
  </div>
  ```

- [ ] **ایجاد Form Cards**:
  ```html
  <div class="form-card">
      <div class="form-card-header">
          <h4 class="form-card-title">
              <i class="fas fa-info-circle"></i>
              اطلاعات اصلی
          </h4>
      </div>
      <div class="form-card-body">
          <div class="row">
              <div class="col-md-6">
                  <div class="form-group">
                      @Html.LabelFor(m => m.Name, new { @class = "form-label" })
                      @Html.TextBoxFor(m => m.Name, new { 
                          @class = "form-control", 
                          placeholder = "نام [Module]",
                          required = "required"
                      })
                      @Html.ValidationMessageFor(m => m.Name, "", new { @class = "text-danger" })
                  </div>
              </div>
          </div>
      </div>
  </div>
  ```

### **📅 Persian Date Implementation**
- [ ] **ایجاد فیلدهای تاریخ شمسی**:
  ```html
  <div class="form-group">
      @Html.LabelFor(m => m.ValidFromShamsi, new { @class = "form-label" })
      @Html.TextBoxFor(m => m.ValidFromShamsi, new { 
          @class = "form-control persian-datepicker", 
          placeholder = "مثال: 1404/06/23",
          required = "required"
      })
      @Html.ValidationMessageFor(m => m.ValidFromShamsi, "", new { @class = "text-danger" })
  </div>
  ```

### **🎨 CSS Styling**
- [ ] **ایجاد CSS Variables**:
  ```css
  :root {
      --medical-primary: #1e3a8a;
      --medical-secondary: #7c3aed;
      --medical-success: #059669;
      --medical-warning: #d97706;
      --medical-danger: #dc2626;
      --medical-info: #0891b2;
  }
  ```

- [ ] **ایجاد Form Styles**:
  ```css
  .form-container {
      background: #ffffff;
      border: 1px solid #e5e7eb;
      border-radius: 0.5rem;
      padding: 1.5rem;
      box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
  }
  
  .form-group.focused .form-control {
      border-color: var(--medical-primary);
      box-shadow: 0 0 0 0.2rem rgba(30, 58, 138, 0.25);
  }
  ```

### **⚡ JavaScript Functionality**
- [ ] **ایجاد فایل JavaScript اصلی**:
  ```javascript
  $(document).ready(function() {
      console.log('🏥 [Module] Form - Production Ready Version Loaded');
      
      // Initialize components
      initializePersianDatePickers();
      initializeValidation();
      initializeAjaxHandlers();
      initializeFormEvents();
  });
  ```

- [ ] **پیاده‌سازی Persian DatePicker**:
  ```javascript
  function initializePersianDatePickers() {
      $('.persian-datepicker').persianDatepicker({
          format: 'YYYY/MM/DD',
          calendar: {
              persian: {
                  locale: 'fa',
                  showHint: true,
                  leapYearMode: 'algorithmic'
              }
          },
          checkDate: function(unix) {
              return unix < Date.now();
          },
          autoClose: true,
          initialValue: false,
          position: 'auto',
          viewMode: 'day',
          inputDelay: 800,
          navigator: {
              enabled: true,
              scroll: { enabled: true }
          },
          toolbox: {
              enabled: true,
              todayButton: { enabled: true, text: 'امروز' },
              submitButton: { enabled: true, text: 'تأیید' },
              clearButton: { enabled: true, text: 'پاک کردن' }
          }
      });
  }
  ```

- [ ] **پیاده‌سازی Validation**:
  ```javascript
  function initializeValidation() {
      // Real-time validation
      $('.form-control').on('input change', function() {
          validateField($(this));
      });
      
      // Form submission validation
      $('form').on('submit', function(e) {
          if (!validateForm()) {
              e.preventDefault();
              return false;
          }
      });
  }
  ```

- [ ] **پیاده‌سازی AJAX Handlers**:
  ```javascript
  function initializeAjaxHandlers() {
      // Global AJAX error handler
      $(document).ajaxError(function(event, xhr, settings, error) {
          console.error('❌ AJAX Error:', {
              url: settings.url,
              status: xhr.status,
              error: error
          });
          handleAjaxError(xhr, settings, error);
      });
      
      // Global AJAX success handler
      $(document).ajaxSuccess(function(event, xhr, settings) {
          console.log('✅ AJAX Success:', {
              url: settings.url,
              status: xhr.status
          });
      });
  }
  ```

---

## **📋 مرحله 5: تست و اعتبارسنجی**

### **🧪 Unit Testing**
- [ ] **تست Controller Actions**
- [ ] **تست Service Methods**
- [ ] **تست Repository Methods**
- [ ] **تست ViewModel Conversions**

### **🔗 Integration Testing**
- [ ] **تست کامل Create Flow**
- [ ] **تست کامل Edit Flow**
- [ ] **تست AJAX Calls**
- [ ] **تست Validation**

### **🎨 UI Testing**
- [ ] **تست Persian DatePicker**
- [ ] **تست Form Validation**
- [ ] **تست Responsive Design**
- [ ] **تست Accessibility**

### **⚡ Performance Testing**
- [ ] **تست سرعت بارگذاری**
- [ ] **تست AJAX Response Time**
- [ ] **تست Memory Usage**
- [ ] **تست Database Query Performance**

---

## **📋 مرحله 6: بهینه‌سازی و پولیش**

### **🔧 Code Optimization**
- [ ] **بهینه‌سازی JavaScript**
- [ ] **بهینه‌سازی CSS**
- [ ] **بهینه‌سازی Database Queries**
- [ ] **حذف کدهای غیرضروری**

### **🛡️ Security Review**
- [ ] **بررسی XSS Protection**
- [ ] **بررسی CSRF Protection**
- [ ] **بررسی Input Validation**
- [ ] **بررسی Authorization**

### **📚 Documentation**
- [ ] **کامنت‌گذاری کد**
- [ ] **مستندسازی API**
- [ ] **راهنمای کاربر**
- [ ] **مستندسازی فنی**

---

## **📋 مرحله 7: Deployment و Monitoring**

### **🚀 Deployment**
- [ ] **تست در محیط Staging**
- [ ] **Deploy به Production**
- [ ] **بررسی عملکرد در Production**
- [ ] **بررسی Logs**

### **📊 Monitoring**
- [ ] **تنظیم Logging**
- [ ] **تنظیم Error Tracking**
- [ ] **تنظیم Performance Monitoring**
- [ ] **تنظیم User Analytics**

---

## **✅ چک لیست نهایی**

### **🎯 قبل از تحویل**
- [ ] **تمام تست‌ها پاس شده**
- [ ] **کد Review شده**
- [ ] **مستندات کامل است**
- [ ] **Performance قابل قبول است**
- [ ] **Security Review انجام شده**
- [ ] **User Acceptance Test انجام شده**

### **📋 تحویل**
- [ ] **کد در Repository قرار گرفته**
- [ ] **مستندات به‌روزرسانی شده**
- [ ] **تیم Development مطلع شده**
- [ ] **تیم QA مطلع شده**
- [ ] **تیم DevOps مطلع شده**

---

## **🎯 نتیجه‌گیری**

این چک لیست جامع و عملی برای توسعه فرم‌های پیچیده شامل:

1. **📋 تحلیل و طراحی**
2. **🔧 توسعه Backend**
3. **📝 توسعه ViewModel**
4. **🎨 توسعه Frontend**
5. **🧪 تست و اعتبارسنجی**
6. **⚡ بهینه‌سازی**
7. **🚀 Deployment**

با رعایت این چک لیست، فرم‌های پیچیده به صورت حرفه‌ای و استاندارد توسعه خواهند یافت.

---

**📅 تاریخ ایجاد**: 1404/06/23  
**👤 ایجادکننده**: AI Assistant  
**🔄 نسخه**: 1.0  
**📋 وضعیت**: فعال
