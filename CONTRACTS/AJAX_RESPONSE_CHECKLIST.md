# چک‌لیست بررسی AJAX Response Parsing

## 📋 **قبل از commit هر AJAX code:**

### ✅ **بررسی‌های اجباری:**

- [ ] **آیا `typeof response` بررسی شده؟**
  ```javascript
  console.log('Response Type:', typeof response);
  ```

- [ ] **آیا `JSON.parse` در try-catch قرار دارد؟**
  ```javascript
  try {
      parsedResponse = JSON.parse(response);
  } catch (e) {
      console.error('JSON Parse Error:', e);
      return;
  }
  ```

- [ ] **آیا از `parsedResponse` استفاده شده؟**
  ```javascript
  if (parsedResponse.success) {
      // استفاده از parsedResponse
  }
  ```

- [ ] **آیا لاگ‌گذاری کامل وجود دارد؟**
  ```javascript
  console.log('Raw Response:', response);
  console.log('Parsed Response:', parsedResponse);
  ```

- [ ] **آیا error handling مناسب است؟**
  ```javascript
  if (onError) {
      onError(error);
  } else {
      showError('خطای پیش‌فرض');
  }
  ```

### 🔧 **استفاده از Helper Functions:**

- [ ] **آیا از `parseAjaxResponse()` استفاده شده؟**
  ```javascript
  var parsedResponse = parseAjaxResponse(response);
  ```

- [ ] **آیا از `createAjaxSuccessHandler()` استفاده شده؟**
  ```javascript
  success: createAjaxSuccessHandler(onSuccess, onError)
  ```

- [ ] **آیا از `createAjaxErrorHandler()` استفاده شده؟**
  ```javascript
  error: createAjaxErrorHandler(onError)
  ```

### 📊 **لاگ‌های مورد انتظار:**

- [ ] **Raw Response logged**
- [ ] **Response Type logged**
- [ ] **Parsed Response logged**
- [ ] **Success/Error states logged**

### 🚫 **ممنوعیت‌ها:**

- [ ] **مستقیماً از response استفاده نشده**
- [ ] **JSON.parse بدون try-catch استفاده نشده**
- [ ] **لاگ‌گذاری حذف نشده**
- [ ] **Error handling نادیده گرفته نشده**

---

## 📁 **فایل‌های بررسی:**

### **فایل‌های JavaScript:**
- [ ] `Views/**/*.cshtml` - تمام AJAX calls
- [ ] `Scripts/**/*.js` - تمام AJAX functions
- [ ] `Content/**/*.js` - تمام AJAX implementations

### **فایل‌های Controller:**
- [ ] `Controllers/**/*.cs` - تمام AJAX actions
- [ ] `Areas/**/Controllers/**/*.cs` - تمام AJAX actions

---

## 🎯 **نتیجه:**

### ✅ **اگر همه چک‌ها ✅ هستند:**
- **کد آماده commit است**
- **مشکل Response Parsing رخ نخواهد داد**

### ❌ **اگر هر چک ❌ است:**
- **کد را اصلاح کنید**
- **دوباره چک‌ها را انجام دهید**
- **تا زمانی که همه ✅ نشوند، commit نکنید**

---

## 📋 **امضای چک‌لیست:**

- **تاریخ بررسی:** ___________
- **بررسی کننده:** ___________
- **وضعیت:** ✅ PASS / ❌ FAIL
- **یادداشت‌ها:** ___________

---

**⚠️ این چک‌لیست باید برای تمام AJAX implementations استفاده شود.**
