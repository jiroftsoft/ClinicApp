# 📋 قرارداد اصول UX/UI مخصوص محیط درمانی

## 🎯 هدف
این قرارداد استانداردهای طراحی رابط کاربری و تجربه کاربری مخصوص محیط‌های درمانی و کلینیک‌ها را تعریف می‌کند.

## 🏥 اصول کلیدی محیط درمانی

### 1. ✅ سادگی و وضوح
- **کمترین پیچیدگی:** فرم‌ها و صفحات باید با کمترین المان‌های غیرضروری طراحی شوند
- **وضوح بالا:** تمام اطلاعات باید به وضوح قابل خواندن باشند
- **ساختار ساده:** ناوبری و ساختار صفحات باید ساده و منطقی باشد

### 2. ✅ Bootstrap 4/5
- **ریسپانسیو بودن:** سازگاری با تمام دستگاه‌ها
- **سادگی در طراحی:** استفاده از کامپوننت‌های آماده Bootstrap
- **فرم‌ها و جدول‌ها:** طراحی استاندارد و یکپارچه

### 3. ✅ فونت خوانا و رنگ‌های استاندارد

#### فونت‌های محیط درمانی:
```css
--medical-font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
--medical-font-size-base: 16px;
--medical-font-size-sm: 14px;
--medical-font-size-lg: 18px;
--medical-line-height: 1.6;
```

#### رنگ‌های استاندارد:
- **🟢 سبز (#28a745):** موارد موفق (فعال)
- **🔴 قرمز (#dc3545):** هشدار و خطا
- **🔵 آبی (#2c5aa0):** اطلاعات
- **🟡 زرد (#ffc107):** هشدار
- **⚫ خاکستری (#6c757d):** ثانویه

### 4. ✅ دکمه‌ها و اکشن‌ها

#### ویژگی‌های دکمه‌ها:
- **آیکون + متن:** هر دکمه باید آیکون و متن داشته باشد
- **Tooltip:** توضیح کوتاه برای هر دکمه
- **Hover Effect:** انیمیشن ملایم هنگام hover
- **Focus State:** قابل دسترسی با کیبورد

#### کد نمونه:
```html
<a href="#" class="btn btn-success btn-action" data-tooltip="توضیح دکمه">
    <i class="fas fa-icon"></i> متن دکمه
</a>
```

## 🎨 CSS Variables محیط درمانی

```css
:root {
    /* رنگ‌های اصلی */
    --medical-primary: #2c5aa0;        /* آبی - اطلاعات */
    --medical-secondary: #6c757d;      /* خاکستری - ثانویه */
    --medical-success: #28a745;        /* سبز - موفقیت/فعال */
    --medical-danger: #dc3545;         /* قرمز - هشدار/خطا */
    --medical-warning: #ffc107;        /* زرد - هشدار */
    --medical-info: #17a2b8;           /* آبی روشن - اطلاعات */
    
    /* فونت‌ها */
    --medical-font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    --medical-font-size-base: 16px;
    --medical-font-size-sm: 14px;
    --medical-font-size-lg: 18px;
    --medical-line-height: 1.6;
}
```

## 🔧 کلاس‌های CSS استاندارد

### دکمه‌های Action:
```css
.btn-action {
    margin: 0.25rem;
    font-weight: 600;
    font-family: var(--medical-font-family);
    font-size: var(--medical-font-size-sm);
    padding: 0.75rem 1.5rem;
    border-radius: 0.375rem;
    transition: all 0.2s ease-in-out;
    position: relative;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
}
```

### Tooltip:
```css
.btn-action[data-tooltip]:hover::after {
    content: attr(data-tooltip);
    position: absolute;
    bottom: 100%;
    left: 50%;
    transform: translateX(-50%);
    background: var(--medical-dark);
    color: white;
    padding: 0.5rem 0.75rem;
    border-radius: 0.25rem;
    font-size: 0.875rem;
    white-space: nowrap;
    z-index: 1000;
}
```

### Typography:
```css
.detail-label {
    font-weight: 600;
    color: var(--medical-text);
    margin-bottom: 0.5rem;
    font-size: var(--medical-font-size-sm);
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.detail-value {
    color: var(--medical-text-muted);
    font-size: var(--medical-font-size-base);
    font-weight: 400;
    line-height: var(--medical-line-height);
}
```

## ♿ دسترسی‌پذیری (Accessibility)

### Focus States:
```css
.btn-action:focus {
    outline: 2px solid var(--medical-primary);
    outline-offset: 2px;
}
```

### Reduced Motion:
```css
@media (prefers-reduced-motion: reduce) {
    .btn-action {
        transition: none;
    }
    
    .btn-action:hover {
        transform: none;
    }
}
```

## 📱 Responsive Design

### Mobile First:
```css
@media (max-width: 768px) {
    .details-container {
        padding: 1rem;
    }
    
    .btn-action {
        width: 100%;
        margin: 0.25rem 0;
    }
}
```

## 🖨️ Print Styles

```css
@media print {
    .btn-action {
        display: none;
    }
    
    .details-container {
        box-shadow: none;
        border: 1px solid #ccc;
    }
    
    .detail-label {
        color: #000 !important;
    }
    
    .detail-value {
        color: #333 !important;
    }
}
```

## 🌙 Dark Mode Support

```css
@media (prefers-color-scheme: dark) {
    .details-container {
        background: #2d3748;
        color: #e2e8f0;
    }
    
    .detail-label {
        color: #e2e8f0;
    }
    
    .detail-value {
        color: #a0aec0;
    }
}
```

## 📋 چک‌لیست پیاده‌سازی

### ✅ Typography:
- [x] فونت خوانا (Segoe UI)
- [x] سایز فونت مناسب (16px base)
- [x] Line height مناسب (1.6)
- [x] Letter spacing برای برچسب‌ها

### ✅ Colors:
- [x] رنگ سبز برای موفقیت
- [x] رنگ قرمز برای خطا
- [x] رنگ آبی برای اطلاعات
- [x] رنگ زرد برای هشدار

### ✅ Buttons:
- [x] آیکون + متن
- [x] Tooltip
- [x] Hover effect
- [x] Focus state
- [x] Accessibility

### ✅ Layout:
- [x] Bootstrap 4/5
- [x] Responsive design
- [x] Mobile first
- [x] Print styles

### ✅ Accessibility:
- [x] Focus indicators
- [x] Reduced motion support
- [x] High contrast
- [x] Keyboard navigation

## 🎯 Best Practices

### 1. سادگی:
- از المان‌های غیرضروری اجتناب کنید
- اطلاعات را به صورت واضح و ساده نمایش دهید
- از رنگ‌های محدود و استاندارد استفاده کنید

### 2. خوانایی:
- فونت‌های خوانا انتخاب کنید
- کنتراست مناسب بین متن و پس‌زمینه
- سایز فونت مناسب برای محیط درمانی

### 3. دسترسی‌پذیری:
- تمام دکمه‌ها قابل دسترسی با کیبورد باشند
- Tooltip برای توضیح عملکرد
- Focus state واضح

### 4. عملکرد:
- انیمیشن‌های ملایم
- بارگذاری سریع
- سازگاری با مرورگرهای مختلف

---

**📅 تاریخ ایجاد:** $(date)  
**👨‍💻 توسعه‌دهنده:** AI Assistant  
**📋 نسخه:** 1.0.0  
**🔄 آخرین به‌روزرسانی:** $(date)
