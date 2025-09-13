# 📋 قرارداد سیستم اعلان‌ها (Notification System Contract)

## 🎯 هدف
این قرارداد استانداردهای پیاده‌سازی و استفاده از سیستم اعلان‌ها در کلینیک درمانی شفا را تعریف می‌کند.

## 📁 فایل‌های مرتبط

### Core Files
- `Areas/Admin/Views/Shared/_NotificationMessages.cshtml` - نمایش اعلان‌ها
- `Areas/Admin/Controllers/BaseController.cs` - مدیریت اعلان‌ها
- `Content/css/notification-system.css` - استایل‌های اعلان‌ها
- `Areas/Admin/Views/Shared/_AdminLayout.cshtml` - Layout اصلی

## 🔧 ویژگی‌های پیاده‌سازی شده

### ✅ jQuery Safety Pattern
```javascript
// استفاده از ensureJQuery برای اطمینان از لود jQuery
function ensureJQuery(callback) {
    if (typeof jQuery !== 'undefined' && typeof $.fn !== 'undefined') {
        callback();
    } else {
        setTimeout(function() {
            ensureJQuery(callback);
        }, 50);
    }
}
```

### ✅ Auto-Hide Functionality
- **زمان:** 5 ثانیه
- **انیمیشن:** fadeOut با transition
- **قابلیت توقف:** hover برای توقف موقت

### ✅ Manual Close
- **دکمه X:** بستن دستی اعلان
- **انیمیشن:** slide-out با transition
- **حذف از DOM:** پس از انیمیشن

### ✅ Session Management
- **پاک‌سازی خودکار:** پس از نمایش
- **Error Handling:** مدیریت خطاهای AJAX
- **Logging:** ثبت موفقیت/شکست

### ✅ Responsive Design
- **Mobile:** تنظیمات مخصوص موبایل
- **Dark Mode:** پشتیبانی از حالت تاریک
- **Accessibility:** رعایت استانداردهای دسترسی

## 🎨 استایل‌های CSS

### انواع اعلان‌ها
```css
/* Success */
.notification-message.alert-success {
    background-color: #d4edda;
    color: #155724;
    border-left: 4px solid #28a745;
}

/* Error */
.notification-message.alert-danger {
    background-color: #f8d7da;
    color: #721c24;
    border-left: 4px solid #dc3545;
}

/* Warning */
.notification-message.alert-warning {
    background-color: #fff3cd;
    color: #856404;
    border-left: 4px solid #ffc107;
}

/* Info */
.notification-message.alert-info {
    background-color: #d1ecf1;
    color: #0c5460;
    border-left: 4px solid #17a2b8;
}
```

### انیمیشن‌ها
```css
/* Slide In */
@keyframes slideInRight {
    from { transform: translateX(100%); opacity: 0; }
    to { transform: translateX(0); opacity: 1; }
}

/* Fade Out */
.notification-message.fade-out {
    opacity: 0;
    transform: translateX(100%);
    transition: all 0.5s ease-in-out;
}
```

## 🔄 جریان کار (Workflow)

### 1. ایجاد اعلان
```csharp
// در Controller
_messageNotificationService.AddSuccessMessage("طرح بیمه جدید با موفقیت ایجاد شد");
```

### 2. نمایش اعلان
```html
<!-- در _NotificationMessages.cshtml -->
<div class="alert alert-success alert-dismissible fade show notification-message">
    <i class="fas fa-check-circle"></i>
    <strong>پیام موفقیت</strong>
    <button type="button" class="close" data-dismiss="alert">
        <span>&times;</span>
    </button>
</div>
```

### 3. مدیریت JavaScript
```javascript
// Auto-hide after 5 seconds
setTimeout(function() {
    hideAllNotifications();
}, 5000);

// Clear from session
clearNotificationsFromSession();
```

## 🛡️ امنیت و عملکرد

### Security
- ✅ **XSS Protection:** Escaping تمام ورودی‌ها
- ✅ **CSRF Protection:** استفاده از Anti-Forgery Token
- ✅ **Content Security Policy:** رعایت CSP headers

### Performance
- ✅ **Lazy Loading:** لود CSS فقط در صورت نیاز
- ✅ **Memory Management:** حذف اعلان‌ها از DOM
- ✅ **Event Cleanup:** پاک‌سازی event listeners

## 📱 Responsive Design

### Mobile (< 768px)
```css
@media (max-width: 768px) {
    .notification-container {
        top: 10px;
        right: 10px;
        left: 10px;
        max-width: none;
    }
}
```

### Dark Mode
```css
@media (prefers-color-scheme: dark) {
    .notification-message.alert-success {
        background-color: #1e3a1e;
        color: #90ee90;
    }
}
```

## 🧪 تست و Debug

### Console Logging
```javascript
// Success
console.log('✅ Notifications cleared from session');

// Error
console.error('❌ Failed to clear notifications:', error);
```

### Manual Testing
1. ✅ ایجاد اعلان موفقیت
2. ✅ ایجاد اعلان خطا
3. ✅ Auto-hide پس از 5 ثانیه
4. ✅ Manual close با دکمه X
5. ✅ Hover pause/resume
6. ✅ Session clearing
7. ✅ Mobile responsiveness

## 🔄 نگهداری و توسعه

### Best Practices
- ✅ **DRY Principle:** عدم تکرار کد
- ✅ **Separation of Concerns:** جداسازی CSS, JS, HTML
- ✅ **Error Handling:** مدیریت خطاها
- ✅ **Performance:** بهینه‌سازی عملکرد

### Future Enhancements
- 🔄 **Sound Notifications:** اعلان صوتی
- 🔄 **Push Notifications:** اعلان‌های مرورگر
- 🔄 **Notification History:** تاریخچه اعلان‌ها
- 🔄 **Custom Templates:** قالب‌های سفارشی

## 📋 چک‌لیست پیاده‌سازی

### ✅ تکمیل شده
- [x] jQuery Safety Pattern
- [x] Auto-hide functionality
- [x] Manual close buttons
- [x] Session management
- [x] CSS styling
- [x] Responsive design
- [x] Error handling
- [x] Console logging
- [x] Animation effects
- [x] Hover interactions

### 🔄 در حال توسعه
- [ ] Sound notifications
- [ ] Push notifications
- [ ] Notification history
- [ ] Custom templates

---

**📅 تاریخ ایجاد:** $(date)  
**👨‍💻 توسعه‌دهنده:** AI Assistant  
**📋 نسخه:** 1.0.0  
**🔄 آخرین به‌روزرسانی:** $(date)
