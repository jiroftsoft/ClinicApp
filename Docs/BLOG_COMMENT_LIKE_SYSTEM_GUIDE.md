# راهنمای سیستم کامنت و لایک مقالات - Production Ready

## 📋 فهرست مطالب
1. [معرفی](#معرفی)
2. [معماری سیستم](#معماری-سیستم)
3. [ویژگی‌ها](#ویژگی‌ها)
4. [استفاده در Admin](#استفاده-در-admin)
5. [استفاده در Public](#استفاده-در-public)
6. [API Endpoints](#api-endpoints)
7. [امنیت](#امنیت)

---

## معرفی

سیستم کامنت و لایک برای **مقالات بلاگ** طراحی شده است و شامل:

- ✅ **سیستم کامنت**: کامنت‌گذاری با قابلیت پاسخ
- ✅ **سیستم لایک**: لایک/آنلایک با پشتیبانی از کاربران لاگین و مهمان
- ✅ **مدیریت ادمین**: تأیید، رد، حذف، اسپم، گزارش
- ✅ **Production-Ready**: امنیت، Logging، Error Handling

---

## معماری سیستم

### Entities
- `BlogPostComment`: کامنت‌های مقالات
- `BlogPostLike`: لایک‌های مقالات

### Repositories
- `IBlogPostCommentRepository` / `BlogPostCommentRepository`
- `IBlogPostLikeRepository` / `BlogPostLikeRepository`

### Services
- `IBlogPostCommentService` / `BlogPostCommentService`
- `IBlogPostLikeService` / `BlogPostLikeService`

### Controllers
- `BlogPostCommentController` (Admin): مدیریت کامنت‌ها
- `BlogPostCommentApiController` (Public): API برای کامنت و لایک

---

## ویژگی‌ها

### 1. سیستم کامنت
- ✅ کامنت‌گذاری با نام، ایمیل، شماره تماس
- ✅ پشتیبانی از کاربران لاگین و مهمان
- ✅ پاسخ به کامنت (Nested Comments)
- ✅ تأیید ادمین قبل از نمایش
- ✅ گزارش کامنت توسط کاربران
- ✅ علامت‌گذاری اسپم

### 2. سیستم لایک
- ✅ لایک/آنلایک (Toggle)
- ✅ پشتیبانی از کاربران لاگین و مهمان (Guest Identifier)
- ✅ جلوگیری از لایک تکراری (Unique Constraint)
- ✅ نمایش تعداد لایک‌ها

### 3. امنیت
- ✅ IP Address Tracking
- ✅ User Agent Tracking
- ✅ Anti-Forgery Token
- ✅ Validation کامل
- ✅ Soft Delete برای کامنت‌ها

---

## استفاده در Admin

### مدیریت کامنت‌ها

**URL**: `/Admin/CMS/BlogPostComment`

**فیلترها**:
- تأیید شده‌ها
- در انتظار تأیید
- گزارش شده‌ها
- اسپم

**عملیات**:
- ✅ تأیید کامنت
- ❌ رد کامنت
- 🗑️ حذف کامنت
- 🚫 علامت‌گذاری اسپم
- 🚩 گزارش/لغو گزارش

---

## استفاده در Public

### API Endpoints

#### 1. ایجاد کامنت
```javascript
POST /BlogPostCommentApi/CreateComment
{
    BlogPostId: 1,
    CommentText: "متن کامنت",
    AuthorName: "نام",
    AuthorEmail: "email@example.com",
    AuthorPhone: "09123456789"
}
```

#### 2. دریافت کامنت‌ها
```javascript
GET /BlogPostCommentApi/GetComments?blogPostId=1&pageNumber=1&pageSize=10
```

#### 3. گزارش کامنت
```javascript
POST /BlogPostCommentApi/ReportComment
{
    commentId: 1
}
```

#### 4. Toggle Like
```javascript
POST /BlogPostCommentApi/ToggleLike
{
    blogPostId: 1,
    guestIdentifier: "guest_123456" // اختیاری
}
```

#### 5. دریافت تعداد لایک‌ها
```javascript
GET /BlogPostCommentApi/GetLikeCount?blogPostId=1
```

#### 6. بررسی لایک کاربر
```javascript
GET /BlogPostCommentApi/HasLiked?blogPostId=1&guestIdentifier=guest_123456
```

---

## API Endpoints

### Comment API

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/BlogPostCommentApi/CreateComment` | ایجاد کامنت جدید |
| GET | `/BlogPostCommentApi/GetComments` | دریافت کامنت‌ها |
| POST | `/BlogPostCommentApi/ReportComment` | گزارش کامنت |

### Like API

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/BlogPostCommentApi/ToggleLike` | لایک/آنلایک |
| GET | `/BlogPostCommentApi/GetLikeCount` | تعداد لایک‌ها |
| GET | `/BlogPostCommentApi/HasLiked` | بررسی لایک کاربر |

---

## امنیت

### 1. Validation
- ✅ بررسی وجود مقاله
- ✅ بررسی طول متن کامنت (حداکثر 2000 کاراکتر)
- ✅ بررسی فرمت ایمیل
- ✅ بررسی تکراری نبودن لایک

### 2. Tracking
- ✅ IP Address
- ✅ User Agent
- ✅ Guest Identifier (برای کاربران مهمان)

### 3. Anti-Forgery
- ✅ استفاده از `[ValidateAntiForgeryToken]`
- ✅ بررسی Token در تمام POST requests

---

## مثال استفاده

### در View (Public)

```html
<!-- Like Section -->
@Html.Partial("_LikeSection", Model.BlogPostId)

<!-- Comment Section -->
@Html.Partial("_CommentSection", Model.BlogPostId)
```

### در JavaScript

```javascript
// Toggle Like
toggleLike(blogPostId);

// Submit Comment
$('#commentForm').on('submit', function(e) {
    e.preventDefault();
    // AJAX call to CreateComment
});

// Report Comment
reportComment(commentId);
```

---

## خلاصه

- ✅ سیستم کامنت کامل با قابلیت پاسخ
- ✅ سیستم لایک با پشتیبانی کاربران مهمان
- ✅ مدیریت ادمین حرفه‌ای
- ✅ API های RESTful
- ✅ امنیت بالا
- ✅ Production-Ready
- ✅ Strongly-Typed
- ✅ SRP

---

**تاریخ ایجاد**: 2024  
**نسخه**: 1.0  
**نویسنده**: ClinicApp Development Team

