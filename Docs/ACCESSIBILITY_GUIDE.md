# ♿ راهنمای Accessibility (دسترسی‌پذیری)

این مستند شامل راهنمای کامل Accessibility برای Homepage است.

## 📋 فهرست مطالب

1. [WCAG AA Compliance](#wcag-aa-compliance)
2. [ARIA Attributes](#aria-attributes)
3. [Keyboard Navigation](#keyboard-navigation)
4. [Screen Reader Support](#screen-reader-support)
5. [Color Contrast](#color-contrast)
6. [Focus Management](#focus-management)

---

## ✅ WCAG AA Compliance

### 1. Level AA Requirements

- **Contrast Ratio**: حداقل 4.5:1 برای text عادی، 3:1 برای text بزرگ
- **Keyboard Accessible**: تمام interactive elements قابل دسترسی با keyboard
- **Focus Indicators**: focus indicators واضح و قابل مشاهده
- **Labels**: تمام form inputs دارای labels واضح
- **Alt Text**: تمام images دارای alt text مناسب

### 2. Implementation Checklist

- [x] ARIA Labels برای تمام interactive elements
- [x] Semantic HTML (nav, main, footer, article, section)
- [x] Skip Links برای پرش به محتوای اصلی
- [x] Keyboard Navigation Support
- [x] Focus Indicators
- [x] Alt Text برای تمام images
- [x] Color Contrast Compliance
- [x] Screen Reader Support

---

## 🏷️ ARIA Attributes

### 1. Navigation

```html
<nav role="navigation" aria-label="منوی اصلی">
    <ul role="menubar">
        <li role="menuitem">
            <a href="#" aria-current="page">خانه</a>
        </li>
    </ul>
</nav>
```

### 2. Buttons

```html
<button type="button" 
        aria-label="بستن منو"
        aria-expanded="false"
        aria-controls="menu">
    <span aria-hidden="true">×</span>
</button>
```

### 3. Modals

```html
<div role="dialog" 
     aria-labelledby="modal-title"
     aria-modal="true">
    <h2 id="modal-title">عنوان Modal</h2>
</div>
```

### 4. Accordions

```html
<button aria-expanded="false" 
        aria-controls="accordion-content"
        aria-label="باز کردن بخش">
    عنوان
</button>
<div id="accordion-content" 
     role="region"
     aria-labelledby="accordion-button">
    محتوا
</div>
```

### 5. Images

```html
<img src="image.jpg" 
     alt="توضیحات کامل تصویر"
     aria-describedby="image-description">
<p id="image-description" class="sr-only">
    توضیحات بیشتر درباره تصویر
</p>
```

---

## ⌨️ Keyboard Navigation

### 1. Tab Order

تمام interactive elements باید در tab order منطقی قرار گیرند:

```html
<a href="#" tabindex="0">Link</a>
<button tabindex="0">Button</button>
<input type="text" tabindex="0">
```

### 2. Skip Links

```html
<a href="#mainContent" class="skip-link">
    پرش به محتوای اصلی
</a>
```

### 3. Keyboard Shortcuts

- **Tab**: حرکت به element بعدی
- **Shift + Tab**: حرکت به element قبلی
- **Enter/Space**: فعال‌سازی button/link
- **Escape**: بستن modal/menu
- **Arrow Keys**: navigation در dropdowns/accordions

### 4. Focus Trapping

در modals، focus باید trap شود:

```javascript
function trapFocus(modal) {
    const focusableElements = modal.querySelectorAll(
        'a[href], button:not([disabled]), textarea, input, select'
    );
    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];
    
    modal.addEventListener('keydown', function(e) {
        if (e.key === 'Tab') {
            if (e.shiftKey && document.activeElement === firstElement) {
                e.preventDefault();
                lastElement.focus();
            } else if (!e.shiftKey && document.activeElement === lastElement) {
                e.preventDefault();
                firstElement.focus();
            }
        }
    });
}
```

---

## 🔊 Screen Reader Support

### 1. Semantic HTML

استفاده از semantic HTML elements:

```html
<header role="banner">
    <nav role="navigation">...</nav>
</header>
<main role="main" id="mainContent">
    <article role="article">...</article>
    <section role="region" aria-labelledby="section-title">
        <h2 id="section-title">عنوان</h2>
    </section>
</main>
<footer role="contentinfo">...</footer>
```

### 2. Live Regions

برای dynamic content:

```html
<div role="status" 
     aria-live="polite"
     aria-atomic="true">
    پیام به کاربر
</div>
```

### 3. Hidden Text

برای اطلاعات اضافی:

```html
<span class="sr-only">اطلاعات اضافی برای screen reader</span>
```

CSS:

```css
.sr-only {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border-width: 0;
}
```

---

## 🎨 Color Contrast

### 1. Text Contrast

- **Normal Text**: حداقل 4.5:1
- **Large Text (18pt+)**: حداقل 3:1
- **UI Components**: حداقل 3:1

### 2. Implementation

تمام رنگ‌ها در Design System از contrast ratios مناسب استفاده می‌کنند:

```css
:root {
    --text-color: #4a5568; /* Contrast: 7.2:1 on white */
    --heading-color: #2d3748; /* Contrast: 9.1:1 on white */
    --primary-color: #2c6e7d; /* Contrast: 4.8:1 on white */
}
```

### 3. Tools

- [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)
- [Colour Contrast Analyser](https://www.tpgi.com/color-contrast-checker/)

---

## 🎯 Focus Management

### 1. Visible Focus Indicators

```css
a:focus,
button:focus,
input:focus,
select:focus,
textarea:focus {
    outline: 2px solid var(--primary-color);
    outline-offset: 2px;
}
```

### 2. Focus Order

Focus باید در logical order باشد:

1. Skip Link
2. Navigation
3. Main Content
4. Footer

### 3. Focus Restoration

بعد از بستن modal، focus باید به element قبلی برگردد:

```javascript
function openModal(modal, trigger) {
    const previousFocus = document.activeElement;
    modal.show();
    // Focus first element in modal
    const firstElement = modal.querySelector('input, button, a');
    if (firstElement) firstElement.focus();
    
    // Restore focus on close
    modal.addEventListener('hidden', function() {
        previousFocus.focus();
    });
}
```

---

## ✅ Implementation Checklist

### Navigation
- [x] ARIA labels برای navigation
- [x] Skip links
- [x] Keyboard navigation
- [x] Focus indicators

### Images
- [x] Alt text برای تمام images
- [x] Decorative images با `aria-hidden="true"`
- [x] Complex images با `aria-describedby`

### Forms
- [x] Labels برای تمام inputs
- [x] Error messages با `aria-describedby`
- [x] Required fields با `aria-required="true"`

### Interactive Elements
- [x] Buttons با `aria-label` یا text content
- [x] Links با descriptive text
- [x] Modals با `aria-modal="true"`
- [x] Accordions با `aria-expanded`

### Content
- [x] Semantic HTML
- [x] Headings hierarchy (h1 → h2 → h3)
- [x] Landmarks (nav, main, footer)
- [x] Live regions برای dynamic content

---

## 🧪 Testing

### 1. Screen Readers

- **NVDA** (Windows, Free)
- **JAWS** (Windows, Paid)
- **VoiceOver** (macOS/iOS, Built-in)
- **TalkBack** (Android, Built-in)

### 2. Keyboard Testing

- Tab through entire page
- Test all interactive elements
- Verify focus indicators
- Test keyboard shortcuts

### 3. Automated Tools

- **WAVE** (Web Accessibility Evaluation Tool)
- **axe DevTools**
- **Lighthouse Accessibility Audit**

---

## 📚 منابع

- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [MDN Accessibility](https://developer.mozilla.org/en-US/docs/Web/Accessibility)
- [WebAIM](https://webaim.org/)
- [A11y Project](https://www.a11yproject.com/)

---

**آخرین به‌روزرسانی**: @DateTime.Now.ToString("yyyy-MM-dd")

