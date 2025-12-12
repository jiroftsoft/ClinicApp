# 🎨 Design System Documentation

مستند کامل Design System برای ClinicApp Homepage.

## 📋 فهرست مطالب

1. [Overview](#overview)
2. [Color Palette](#color-palette)
3. [Typography](#typography)
4. [Spacing](#spacing)
5. [Components](#components)
6. [Animations](#animations)
7. [Utilities](#utilities)

---

## 📖 Overview

Design System برای ClinicApp با هدف ایجاد یک تجربه کاربری یکپارچه، حرفه‌ای و مناسب برای محیط درمانی طراحی شده است.

### اصول طراحی

- **Formal & Administrative**: رنگ‌های رسمی و اداری
- **Medical Environment**: مناسب برای محیط درمانی
- **Accessibility**: رعایت WCAG AA Compliance
- **Responsive**: Mobile-First Design
- **Performance**: بهینه‌سازی شده برای Performance

---

## 🎨 Color Palette

### Primary Colors

```css
--primary-color: #2c6e7d;      /* Deep Teal - رنگ اصلی */
--secondary-color: #4a9da9;    /* Muted Cyan - رنگ ثانویه */
--accent-color: #c5a47e;       /* Muted Gold/Bronze - رنگ تاکیدی */
```

### Status Colors

```css
--success-color: #28a745;      /* سبز - موفقیت */
--danger-color: #dc3545;        /* قرمز - خطا/اضطراری */
--warning-color: #ffc107;       /* زرد - هشدار */
--info-color: #17a2b8;         /* آبی - اطلاعات */
```

### Neutral Colors

```css
--light-color: #f8f9fa;        /* روشن */
--dark-color: #2d3748;         /* تیره */
--text-color: #4a5568;         /* رنگ متن */
--heading-color: #2d3748;      /* رنگ عناوین */
--border-color: #e2e8f0;       /* رنگ border */
--background-light: #fdfdfd;   /* پس‌زمینه روشن */
--background-gray: #f0f2f5;    /* پس‌زمینه خاکستری */
```

### Usage Guidelines

- **Primary**: برای دکمه‌های اصلی، لینک‌ها، و عناصر مهم
- **Secondary**: برای دکمه‌های ثانویه و accents
- **Accent**: برای highlights و special elements
- **Status Colors**: فقط برای status messages و alerts

---

## 📝 Typography

### Font Family

```css
--font-family-primary: 'Vazirmatn', Tahoma, sans-serif;
```

### Font Sizes

```css
--font-size-base: 1rem;        /* 16px */
--font-size-sm: 0.875rem;      /* 14px */
--font-size-lg: 1.25rem;       /* 20px */
```

### Line Heights

```css
--line-height-base: 1.6;       /* برای متن عادی */
--heading-line-height: 1.3;    /* برای عناوین */
```

### Headings

```css
h1 { font-size: 2.5rem; font-weight: 700; }
h2 { font-size: 2rem; font-weight: 700; }
h3 { font-size: 1.5rem; font-weight: 600; }
h4 { font-size: 1.25rem; font-weight: 600; }
```

---

## 📏 Spacing

### Spacing Scale

```css
--space-xs: 0.25rem;    /* 4px */
--space-sm: 0.5rem;    /* 8px */
--space-md: 1rem;      /* 16px */
--space-lg: 1.5rem;    /* 24px */
--space-xl: 2rem;      /* 32px */
--space-xxl: 3rem;     /* 48px */
--space-xxxl: 4rem;    /* 64px */
```

### Usage

- **xs/sm**: برای spacing های کوچک (badges, icons)
- **md**: برای spacing های استاندارد (padding, margin)
- **lg/xl**: برای spacing های بزرگ (sections, cards)
- **xxl/xxxl**: برای spacing های بسیار بزرگ (section padding)

---

## 🧩 Components

### Buttons

#### Primary Button

```html
<a href="#" class="btn btn-medical-primary">
    دکمه اصلی
</a>
```

```css
.btn-medical-primary {
    background-color: var(--primary-color);
    color: white;
    border: 1px solid var(--primary-color);
    border-radius: var(--border-radius-md);
    padding: var(--space-md) var(--space-lg);
    font-weight: 600;
    transition: var(--transition-ease);
}
```

#### Outline Button

```html
<a href="#" class="btn btn-medical-outline-primary">
    دکمه Outline
</a>
```

### Cards

#### Medical Card

```html
<div class="medical-card">
    <div class="card-body">
        محتوا
    </div>
</div>
```

```css
.medical-card {
    background-color: white;
    border: 1px solid var(--border-color);
    border-radius: var(--border-radius-lg);
    box-shadow: var(--shadow-sm);
    transition: var(--transition-ease);
}
```

### Badges

```html
<span class="badge-category">
    دسته‌بندی
</span>
```

---

## 🎬 Animations

### Keyframes

```css
@keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}

@keyframes slideUp {
    from { opacity: 0; transform: translateY(20px); }
    to { opacity: 1; transform: translateY(0); }
}

@keyframes scaleIn {
    from { opacity: 0; transform: scale(0.9); }
    to { opacity: 1; transform: scale(1); }
}
```

### Animation Classes

```html
<div class="animate-fade-in">Fade In</div>
<div class="animate-slide-up">Slide Up</div>
<div class="animate-scale-in">Scale In</div>
```

### Transitions

```css
--transition-ease: all 0.3s ease-in-out;
--transition-fast: all 0.2s ease-out;
--transition-slow: all 0.5s ease-in-out;
```

---

## 🛠️ Utilities

### Shadows

```css
--shadow-sm: 0 1px 3px rgba(0, 0, 0, 0.08);
--shadow-md: 0 4px 12px rgba(0, 0, 0, 0.1);
--shadow-lg: 0 8px 25px rgba(0, 0, 0, 0.15);
```

### Border Radius

```css
--border-radius-sm: 0.25rem;   /* 4px */
--border-radius-md: 0.5rem;    /* 8px */
--border-radius-lg: 1rem;      /* 16px */
--border-radius-xl: 1.5rem;    /* 24px */
--border-radius-full: 9999px;  /* Full circle */
```

### Hover Effects

```html
<div class="hover-lift">Lift on Hover</div>
<div class="hover-scale">Scale on Hover</div>
```

---

## 📱 Responsive Design

### Breakpoints

```css
/* Mobile */
@media (max-width: 767.98px) { }

/* Tablet */
@media (max-width: 991.98px) { }

/* Desktop */
@media (min-width: 992px) { }
```

### Mobile-First Approach

- طراحی ابتدا برای Mobile
- سپس برای Tablet و Desktop
- استفاده از `min-width` در media queries

---

## ✅ Best Practices

1. **همیشه از CSS Variables استفاده کنید** - نه hard-coded values
2. **از Utility Classes استفاده کنید** - برای consistency
3. **Responsive Design** - همیشه mobile-first
4. **Accessibility** - ARIA labels و semantic HTML
5. **Performance** - از animations سبک استفاده کنید

---

## 📚 فایل‌های Design System

- `Content/css/design-system.css` - Design System اصلی
- `Content/css/modern-navigation.css` - Navigation styles
- `Content/css/[section]-section.css` - Section-specific styles

---

**آخرین به‌روزرسانی**: @DateTime.Now.ToString("yyyy-MM-dd")

