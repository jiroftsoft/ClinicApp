# گزارش ممیزی ماژول EndSession (پایان جلسه نقدی) — HIS Production

**مسیر:** `/PosManagement/EndSession`  
**کنترلر:** `Controllers/Payment/POS/PosManagementController.cs`  
**سرویس:** `Services/Payment/POS/PosManagementService.cs` — `EndCashSessionAsync`

---

## ۱. باگ‌ها و ریسک‌های بحرانی شناسایی‌شده

### ۱.۱ امنیت (برطرف شده در این مرحله)

| مورد | وضعیت قبل | اصلاح |
|------|------------|--------|
| **Authorize** | کنترلر بدون `[Authorize]` بود | اضافه شد: `[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]` روی کلاس کنترلر |
| **مالکیت جلسه** | هر کاربر احراز هویت‌شده می‌توانست با تغییر `sessionId` جلسه دیگری را ببندد | قبل از فراخوانی سرویس، با `GetSessionByIdAsync` جلسه گرفته می‌شود؛ فقط اگر `session.UserId == currentUser` یا کاربر در نقش Admin باشد درخواست قبول می‌شود؛ در غیر این صورت Redirect + TempData خطا |
| **Anti-Forgery** | وجود داشت | بدون تغییر (`[ValidateAntiForgeryToken]`) |

### ۱.۲ یکپارچگی مالی (نیاز به توجه در فاز بعد)

| مورد | وضعیت | توضیح |
|------|--------|--------|
| **ExpectedBalance از DB** | ریسک | در entity `CashSession` مقدار `ExpectedBalance` از رابطه `OpeningBalance + TotalIncome - TotalExpense` و با `TotalIncome = CashBalance + PosBalance` و `TotalExpense = 0` محاسبه می‌شود. اگر `CashBalance`/`PosBalance` از جدول تراکنش‌ها به‌روز نشوند، مانده مورد انتظار با واقعیت تراکنش‌ها هم‌خوان نیست. **پیشنهاد:** در فاز بعد، محاسبه Expected از مجموع تراکنش‌های جلسه (Payments/Transactions) و نمایش/لاگ آن. |
| **جمع‌آوری از ViewModel فقط** | خیر | مبلغ نهایی از فرم (`model.FinalCashAmount`) می‌آید و در سرویس روی `session.CashBalance` ذخیره می‌شود؛ تطابق با انتظار «مبلغ شمارش نقدی توسط منشی». |

### ۱.۳ Concurrency (پیشنهاد برای فاز بعد)

| مورد | وضعیت | توضیح |
|------|--------|--------|
| **تراکنش دیتابیس** | بدون تراکنش صریح | `GetByIdAsync` و سپس `UpdateAsync` دو فراخوانی جدا هستند. در رقابت همزمان دو درخواست بستن برای یک جلسه، هر دو ممکن است «باز» ببینند و هر دو به‌روزرسانی انجام دهند. |
| **پیشنهاد** | — | استفاده از تراکنش (مثلاً `DbContext.Database.BeginTransaction`) و خواندن مجدد جلسه داخل تراکنش با قفل به‌روزرسانی (مثلاً `SELECT ... WITH (UPDLOCK)`) و سپس بررسی وضعیت و به‌روزرسانی؛ یا استفاده از فیلد **RowVersion** برای Optimistic Concurrency در EF. |
| **جلوگیری از بستن دوباره** | انجام شده | در سرویس قبل از به‌روزرسانی بررسی می‌شود: `if (session.Status == Closed || session.ClosedAt.HasValue)` و در صورت بسته بودن، خطا برگردانده می‌شود. |

### ۱.۴ از دست رفتن داده

- قبل از بستن، جلسه از DB خوانده می‌شود و فقط فیلدهای لازم (وضعیت، مبلغ نقدی، زمان و کاربر به‌روزرسانی) عوض می‌شوند؛ `UpdateAsync` یک بار `SaveChanges` انجام می‌دهد.
- در صورت خطا در سرویس، نتیجه به کنترلر برگردانده می‌شود و Redirect به جزئیات جلسه انجام نمی‌شود؛ رفتار مطابق انتظار است.

---

## ۲. محل دقیق کدهای مرتبط

| بخش | فایل | خطوط (تقریبی) |
|------|------|----------------|
| اکشن EndSession | `Controllers/Payment/POS/PosManagementController.cs` | 587–640 |
| EndCashSessionAsync | `Services/Payment/POS/PosManagementService.cs` | 654–725 |
| GetByIdAsync جلسه | `Repositories/Payment/POS/CashSessionRepository.cs` | 31–45 |
| UpdateAsync جلسه | `Repositories/Payment/POS/CashSessionRepository.cs` | 81–94 |
| Entity CashSession (ExpectedBalance, Difference) | `Models/Entities/Payment/CashSession.cs` | 96–134 |
| مودال و فرم پایان جلسه | `Views/PosManagement/SessionDetails.cshtml` | 256–302 |
| اعتبارسنج EndSession | `ViewModels/Validators/Payment/POS/PosManagementValidators.cs` | 209–222 |

---

## ۳. اصلاحات اتمیک اعمال‌شده

1. **کنترلر**
   - اضافه شدن `[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]` روی کلاس.
   - بعد از اعتبارسنجی مدل، فراخوانی `GetSessionByIdAsync(sessionId)` و در صورت ناموفق بودن، برگرداندن همان خطای سرویس.
   - بررسی مالکیت: اگر `session.UserId != currentUserId` و کاربر Admin نباشد، لاگ هشدار، تنظیم TempData خطا و Redirect به `SessionDetails`.
2. **View**
   - در رویداد `submit` فرم پایان جلسه: در صورت معتبر نبودن فرم فقط `was-validated` و جلوگیری از ارسال.
   - در صورت معتبر بودن: غیرفعال کردن دکمه submit و نمایش متن «در حال بستن جلسه...» برای جلوگیری از double-submit.
3. **سرویس**
   - قبل از به‌روزرسانی، ذخیره `OldStatus` و `OldCashBalance` و محاسبه `expectedBalance` برای لاگ.
   - اضافه شدن یک لاگ حسابرسی (Serilog) با کلید `AUDIT EndSession` شامل SessionId, EndedBy, OldStatus, OldCashBalance, NewCashBalance, Difference.

---

## ۴. موارد پیشنهادی برای فاز بعد (بدون تغییر در این مرحله)

- **مالی:** محاسبه ExpectedBalance از جدول تراکنش/پرداخت جلسه و نمایش (و در صورت تمایل ذخیره) در مدل/ویو.
- **Concurrency:** استفاده از تراکنش دیتابیس + قفل به‌روزرسانی یا RowVersion برای بستن جلسه.
- **Audit رسمی:** فراخوانی `ICashSessionAuditService.LogActionAsync("SessionClosed", oldValue, newValue, description)` بعد از بستن موفق جلسه (نیاز به تزریق این سرویس در `PosManagementService`).
- **UI:** نمایش واضح «موجودی مورد انتظار» در مودال (در صورت تغییر منبع ExpectedBalance) و هایلایت قرمز/سبز برای اختلاف.

---

## ۵. خلاصه اولویت‌ها

```
یکپارچگی مالی > امنیت داده > امنیت دسترسی > قابلیت اطمینان > UX > عملکرد
```

- در این مرحله: **امنیت دسترسی** (Authorize + مالکیت جلسه)، **جلوگیری از double-submit** و **لاگ حسابرسی** در سرویس اعمال شد.
- برای محیط Production درمانی، در فاز بعد حتماً **تراکنش و قفل/RowVersion** و در صورت نیاز **محاسبه Expected از تراکنش‌ها** و **ثبت در CashSessionAuditLog** انجام شود.
