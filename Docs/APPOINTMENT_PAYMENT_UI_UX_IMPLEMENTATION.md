# 🎨 UI/UX Implementation - نوبت‌های نیازمند پرداخت

**تاریخ:** 2026-01-06  
**وضعیت:** ✅ تکمیل شده  
**ماژول:** Patient Appointments / Payment Highlighting

---

## ✅ کارهای انجام شده

### 1. Backend Changes ✅

#### 1.1. PatientAppointmentDto Enhancement
- [x] اضافه شدن فیلد `RequiresPayment` (boolean)
- [x] اضافه شدن فیلد `PaymentTransactionId` (int?)

**فایل:** `Models/DTOs/Appointment/PatientAppointmentDto.cs`

```csharp
/// <summary>
/// آیا این نوبت نیاز به پرداخت دارد؟
/// true = نوبت رزرو شده اما پرداخت نشده (Status = Pending و PaymentTransactionId = null)
/// </summary>
public bool RequiresPayment { get; set; }

/// <summary>
/// شناسه تراکنش پرداخت (اگر پرداخت شده باشد)
/// </summary>
public int? PaymentTransactionId { get; set; }
```

#### 1.2. Service Logic Enhancement
- [x] به‌روزرسانی `GetPatientAppointmentsAsync` برای تشخیص نوبت‌های نیازمند پرداخت

**فایل:** `Services/Appointment/AppointmentBookingService.cs`

```csharp
RequiresPayment = (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Scheduled) && 
                  !a.PaymentTransactionId.HasValue && 
                  a.Price > 0,
PaymentTransactionId = a.PaymentTransactionId
```

---

### 2. Frontend Changes ✅

#### 2.1. Appointment Card Enhancement
- [x] Alert Banner برای نوبت‌های نیازمند پرداخت
- [x] Badge مخصوص "نیاز به پرداخت"
- [x] دکمه پرداخت سریع (Desktop + Mobile)
- [x] Highlight Border با Animation

**فایل:** `Areas/Patient/Views/Shared/_AppointmentCard.cshtml`

**ویژگی‌ها:**
- ✅ Alert Banner با گرادینت زرد
- ✅ Icon با Animation (Pulse)
- ✅ دکمه "پرداخت سریع" در Desktop و Mobile
- ✅ Badge "نیاز به پرداخت" به جای "در انتظار"
- ✅ Border Highlight با رنگ Warning

#### 2.2. CSS Styling & Animation
- [x] Highlight Border برای نوبت‌های نیازمند پرداخت
- [x] Payment Alert Banner Styling
- [x] Animation برای جلب توجه (Pulse, SlideDown)
- [x] Mobile-First Responsive Design
- [x] Button Styling (btn-medical-warning)

**فایل:** `Content/css/appointment-views.css`

**Animation‌ها:**
- `paymentPulse`: Pulse effect برای Border
- `slideDown`: Slide animation برای Banner
- `pulseIcon`: Pulse effect برای Icon
- `creditCardPulse`: Pulse effect برای Credit Card Icon
- `badgePulse`: Pulse effect برای Badge

**CSS Classes:**
- `.appointment-requires-payment`: Highlight برای کارت
- `.payment-alert-banner`: Banner هشدار
- `.payment-action-btn`: دکمه پرداخت
- `.badge-payment-pending`: Badge مخصوص

#### 2.3. JavaScript Functionality
- [x] Event Handler برای دکمه پرداخت سریع
- [x] تایید پرداخت با نمایش مبلغ
- [x] فراخوانی `ProcessPayment` action
- [x] Error Handling کامل
- [x] هدایت به درگاه پرداخت

**فایل:** `Scripts/patient/appointments.js`

**متدها:**
- `handleQuickPayment`: Event handler برای دکمه پرداخت
- `processPayment`: فراخوانی API و هدایت به درگاه

---

## 🎨 UI/UX Features

### Visual Highlights:
1. **Border Highlight:**
   - رنگ: Warning (#ffc107)
   - ضخامت: 2px
   - Animation: Pulse (2s infinite)

2. **Background Gradient:**
   - از: #fff9e6
   - به: #ffffff
   - Shadow: rgba(255, 193, 7, 0.25)

3. **Alert Banner:**
   - Background: Gradient زرد (#fff3cd → #ffe69c)
   - Border: 2px solid Warning
   - Animation: SlideDown (0.4s)

4. **Payment Button:**
   - Background: Gradient Warning (#ffc107 → #ff9800)
   - Shadow: rgba(255, 193, 7, 0.3)
   - Hover: Transform + Shadow Enhancement
   - Icon Animation: Pulse

5. **Badge:**
   - Background: Gradient Warning
   - Color: #212529
   - Animation: Pulse (2s infinite)

### Mobile Optimization:
- ✅ Responsive Banner Layout
- ✅ Full-Width Button در Mobile
- ✅ Font Size Adjustment
- ✅ Touch-Friendly Targets (48×48)

---

## 📱 Mobile-First Design

### Desktop (> 768px):
- Banner با دکمه در سمت راست
- دکمه پرداخت در Banner و Card

### Mobile (< 768px):
- Banner با Layout عمودی
- دکمه پرداخت Full-Width
- Font Size کاهش یافته

---

## 🔄 User Flow

```
1. کاربر وارد "نوبت‌های من" می‌شود
   ↓
2. نوبت‌های نیازمند پرداخت Highlight می‌شوند
   ↓
3. Alert Banner نمایش داده می‌شود
   ↓
4. کاربر روی "پرداخت سریع" کلیک می‌کند
   ↓
5. تایید پرداخت با نمایش مبلغ
   ↓
6. فراخوانی ProcessPayment API
   ↓
7. هدایت به درگاه پرداخت
```

---

## 🧪 Testing Checklist

### Visual Tests:
- [ ] نوبت‌های نیازمند پرداخت Highlight می‌شوند
- [ ] Alert Banner نمایش داده می‌شود
- [ ] Animation‌ها به درستی کار می‌کنند
- [ ] دکمه پرداخت در Desktop و Mobile نمایش داده می‌شود

### Functional Tests:
- [ ] کلیک روی "پرداخت سریع" → تایید نمایش داده می‌شود
- [ ] تایید → ProcessPayment فراخوانی می‌شود
- [ ] موفق → هدایت به درگاه پرداخت
- [ ] خطا → پیام خطا نمایش داده می‌شود

### Mobile Tests:
- [ ] Layout در Mobile درست است
- [ ] دکمه‌ها Touch-Friendly هستند
- [ ] Animation‌ها Performance خوبی دارند

---

## 📊 Performance Considerations

- ✅ CSS Animations با `transform` و `opacity` (GPU-accelerated)
- ✅ Animation Duration: 2s (نه خیلی سریع، نه خیلی کند)
- ✅ Mobile Optimization: کاهش Animation در Mobile (اختیاری)

---

## 🎯 Best Practices Applied

1. **Medical Color Palette:** ✅
   - استفاده از `--medical-warning` (#ffc107)
   - بدون رنگ‌های ممنوع (بنفش، صورتی، نارنجی تند)

2. **Mobile-First:** ✅
   - Responsive Design
   - Touch-Friendly Targets

3. **Accessibility:** ✅
   - Contrast Ratio مناسب
   - Focus States
   - Semantic HTML

4. **Performance:** ✅
   - GPU-accelerated Animations
   - Lazy Loading (اگر نیاز باشد)

---

## ✅ Status

- ✅ Backend: Complete
- ✅ Frontend: Complete
- ✅ CSS: Complete
- ✅ JavaScript: Complete
- ✅ Mobile Optimization: Complete
- ✅ Testing: Ready

**Implementation Status:** ✅ Complete  
**Ready for Testing:** ✅ Yes

---

## 📝 Notes

- Animation‌ها برای جلب توجه طراحی شده‌اند اما نه خیلی مزاحم
- رنگ Warning (#ffc107) طبق پالت رنگ Medical انتخاب شده است
- دکمه پرداخت در Desktop و Mobile به صورت جداگانه طراحی شده است
- Error Handling کامل برای تمام سناریوها

