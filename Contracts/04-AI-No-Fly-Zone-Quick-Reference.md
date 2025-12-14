# 🚫 AI No-Fly Zone - Quick Reference

> **نسخه کوتاه‌شده برای استفاده در AI Prompt**

---

## ⚡ 15 قانون ممنوعه (خلاصه)

### 1️⃣ **NO GUESSING**
❌ حدس نزن → ✅ بخوان + بپرس

### 2️⃣ **NO CONTRACT VIOLATION**
❌ قرارداد را نقض نکن → ✅ قرارداد بالاتر از همه

### 3️⃣ **NO ARCHITECTURE BYPASS**
❌ Controller → Repository مستقیم → ✅ Controller → Service → Repository

### 4️⃣ **NO SIMPLE RETURNS**
❌ `bool` / `string` → ✅ `ServiceResult<T>`

### 5️⃣ **NO SECURITY NEGLECT**
❌ داده حساس بدون Validation → ✅ امنیت بالاتر از سرعت

### 6️⃣ **NO CODE WITHOUT LOGGING**
❌ عملیات بدون Serilog → ✅ Audit + Trace + Forensic

### 7️⃣ **NO GREGORIAN DATES**
❌ `datetime-local` → ✅ Persian DatePicker + `ParseDateFromHiddenInput`

### 8️⃣ **NO DIRECT FILE UPLOAD**
❌ `file.SaveAs()` → ✅ `IImageUploadService`

### 9️⃣ **NO SILENT CHANGES**
❌ تغییر بدون توضیح → ✅ دلیل + ریسک + اثر جانبی

### 🔟 **NO CODE WITHOUT DOCS**
❌ کلاس بدون XML Docs → ✅ مستندسازی جزئی از کد

### 1️⃣1️⃣ **NO INCOMPATIBLE LIBRARIES**
❌ Framework جدید → ✅ تأیید مالک پروژه

### 1️⃣2️⃣ **NO CHANGES WITHOUT MENTAL TEST**
❌ فقط کد تولید کن → ✅ تحلیل سناریوهای واقعی

### 1️⃣3️⃣ **NO OVER-SIMPLIFICATION**
❌ حذف لایه‌ها → ✅ حفظ معماری

### 1️⃣4️⃣ **NO INDEPENDENT DECISIONS**
❌ تصمیم نهایی بگیر → ✅ پیشنهاد بده

### 1️⃣5️⃣ **HARD STOP ON CONFLICT**
❌ ادامه با تعارض → ✅ توقف + شفاف‌سازی

---

## 🎯 قبل از هر تغییر

```
1. خواندن قراردادهای مرتبط ✅
2. بررسی ساختار واقعی ✅
3. جستجوی الگوهای موجود ✅
4. چک‌لیست 15 قانون ✅
```

---

**نسخه:** 1.0.0  
**مرجع:** [`04-AI-No-Fly-Zone.md`](./04-AI-No-Fly-Zone.md)
