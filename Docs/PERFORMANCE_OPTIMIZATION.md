# 🚀 راهنمای بهینه‌سازی Performance

این مستند شامل راهنمای کامل بهینه‌سازی Performance برای Homepage است.

## 📋 فهرست مطالب

1. [Image Optimization](#image-optimization)
2. [CSS Optimization](#css-optimization)
3. [JavaScript Optimization](#javascript-optimization)
4. [Resource Hints](#resource-hints)
5. [Caching Strategy](#caching-strategy)
6. [Performance Monitoring](#performance-monitoring)

---

## 🖼️ Image Optimization

### 1. Lazy Loading

تمام تصاویر در Sections از `loading="lazy"` استفاده می‌کنند:

```html
<img src="@imageUrl" 
     alt="@altText"
     loading="lazy"
     onerror="this.src='/Content/Images/default-image.jpg'">
```

### 2. Intersection Observer (Advanced)

برای تصاویر با `data-src`، از Intersection Observer استفاده می‌شود:

```html
<img data-src="@imageUrl" 
     alt="@altText"
     loading="lazy">
```

### 3. Error Handling

تمام تصاویر دارای fallback برای خطا هستند:

```html
<img src="@imageUrl" 
     onerror="this.onerror=null; this.src='/Content/Images/default-image.jpg'">
```

### 4. Responsive Images (Future Enhancement)

برای تصاویر responsive، می‌توان از `srcset` استفاده کرد:

```html
<img src="@imageUrl" 
     srcset="@smallImage 480w, @mediumImage 768w, @largeImage 1200w"
     sizes="(max-width: 480px) 100vw, (max-width: 768px) 50vw, 33vw"
     alt="@altText"
     loading="lazy">
```

### 5. Image Formats

- استفاده از WebP برای تصاویر (در صورت امکان)
- استفاده از Thumbnails برای تصاویر بزرگ
- بهینه‌سازی اندازه تصاویر قبل از آپلود

---

## 🎨 CSS Optimization

### 1. CSS Variables

تمام رنگ‌ها، spacing، و typography از CSS Variables استفاده می‌کنند:

```css
:root {
    --primary-color: #2c6e7d;
    --space-md: 1rem;
    --font-size-base: 1rem;
}
```

### 2. Modular CSS

هر Section دارای فایل CSS جداگانه است:

- `design-system.css` - Design System اصلی
- `modern-navigation.css` - Navigation
- `hero-section.css` - Hero Section
- `medical-services-section.css` - Medical Services
- و غیره...

### 3. Critical CSS

CSS های Critical (above the fold) باید inline شوند یا در `<head>` لود شوند.

### 4. Defer Non-Critical CSS

برای CSS های non-critical، از `data-defer="true"` استفاده کنید:

```html
<link rel="stylesheet" href="non-critical.css" data-defer="true">
```

### 5. Minification

در Production، تمام CSS ها باید minify شوند:

```bash
# استفاده از tool های minification
npm run build:css
```

---

## 📜 JavaScript Optimization

### 1. Vanilla JavaScript

تا حد امکان از Vanilla JavaScript استفاده شده است (کاهش dependency به jQuery).

### 2. Modular JavaScript

هر feature دارای فایل JavaScript جداگانه است:

- `modern-navigation.js` - Navigation interactions
- `gallery-lightbox.js` - Gallery lightbox
- `video-modal.js` - Video modal
- `faq-accordion.js` - FAQ accordion
- `image-optimization.js` - Image optimization
- `performance-optimizer.js` - Performance optimization

### 3. Defer/Async Scripts

برای scripts غیر-critical، از `defer` یا `async` استفاده کنید:

```html
<script src="script.js" defer></script>
```

### 4. Code Splitting

برای scripts بزرگ، از code splitting استفاده کنید.

### 5. Minification

در Production، تمام JavaScript ها باید minify شوند:

```bash
# استفاده از tool های minification
npm run build:js
```

---

## 🔗 Resource Hints

### 1. DNS Prefetch

برای external domains، از DNS Prefetch استفاده می‌شود:

```html
<link rel="dns-prefetch" href="https://fonts.googleapis.com">
```

### 2. Preconnect

برای critical resources:

```html
<link rel="preconnect" href="https://fonts.googleapis.com">
```

### 3. Preload

برای critical images:

```html
<link rel="preload" as="image" href="critical-image.jpg">
```

---

## 💾 Caching Strategy

### 1. Browser Caching

برای static resources (CSS, JS, Images):

```
Cache-Control: public, max-age=31536000
```

### 2. Versioning

تمام resources دارای version query string هستند:

```html
<link rel="stylesheet" href="style.css?v=1.0.0">
```

### 3. ETag

استفاده از ETag برای cache validation.

---

## 📊 Performance Monitoring

### 1. Core Web Vitals

- **LCP (Largest Contentful Paint)**: < 2.5s
- **FID (First Input Delay)**: < 100ms
- **CLS (Cumulative Layout Shift)**: < 0.1

### 2. Performance Observer

در Development mode، Performance Observer برای monitoring استفاده می‌شود:

```javascript
// Monitor LCP
const observer = new PerformanceObserver((list) => {
    const entries = list.getEntries();
    console.log('LCP:', entries[entries.length - 1].renderTime);
});
observer.observe({ entryTypes: ['largest-contentful-paint'] });
```

### 3. Tools

- **Google PageSpeed Insights**
- **Lighthouse**
- **WebPageTest**
- **Chrome DevTools Performance Tab**

---

## ✅ Checklist

### Image Optimization
- [x] Lazy Loading برای تمام تصاویر
- [x] Error Handling با Fallback
- [x] Intersection Observer برای Advanced Lazy Loading
- [ ] Responsive Images (srcset) - Future Enhancement
- [ ] WebP Format Support - Future Enhancement

### CSS Optimization
- [x] CSS Variables
- [x] Modular CSS Architecture
- [ ] Critical CSS Inline - Future Enhancement
- [ ] CSS Minification - Production Build
- [ ] CSS Purging - Production Build

### JavaScript Optimization
- [x] Vanilla JavaScript (کاهش jQuery dependency)
- [x] Modular JavaScript Architecture
- [ ] Code Splitting - Future Enhancement
- [ ] JavaScript Minification - Production Build
- [ ] Tree Shaking - Production Build

### Resource Hints
- [x] DNS Prefetch
- [ ] Preconnect - Future Enhancement
- [ ] Preload - Future Enhancement

### Caching
- [x] Version Query Strings
- [ ] Browser Caching Headers - Server Configuration
- [ ] ETag Support - Server Configuration

### Performance Monitoring
- [x] Performance Observer (Development)
- [ ] Production Monitoring - Future Enhancement
- [ ] Real User Monitoring (RUM) - Future Enhancement

---

## 🎯 Best Practices

1. **Images**: همیشه از `loading="lazy"` استفاده کنید
2. **CSS**: از CSS Variables برای consistency استفاده کنید
3. **JavaScript**: از Vanilla JS تا حد امکان استفاده کنید
4. **Caching**: از version query strings استفاده کنید
5. **Monitoring**: Performance را در Development mode monitor کنید

---

## 📚 منابع

- [Web.dev Performance](https://web.dev/performance/)
- [MDN Web Performance](https://developer.mozilla.org/en-US/docs/Web/Performance)
- [Google PageSpeed Insights](https://pagespeed.web.dev/)

---

**آخرین به‌روزرسانی**: 2024-12-19

