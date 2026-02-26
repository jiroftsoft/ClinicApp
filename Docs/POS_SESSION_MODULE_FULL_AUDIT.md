# گزارش ممیزی کامل ماژول جلسه صندوق (شروع، مدیریت، پایان) — HIS Production

**مسیرها:** `/PosManagement/StartSession`, `/PosManagement/Sessions`, `/PosManagement/SessionDetails/{id}`, `/PosManagement/EndSession`  
**کنترلر:** `Controllers/Payment/POS/PosManagementController.cs`  
**سرویس:** `Services/Payment/POS/PosManagementService.cs`  
**ریپازیتوری:** `Repositories/Payment/POS/CashSessionRepository.cs`

---

## ۱. خلاصه ریسک‌های بحرانی و وضعیت

| # | دسته | مورد | وضعیت | اقدام |
|---|------|--------|--------|------|
| 1 | **امنیت** | عدم Authorize روی کنترلر | ✅ برطرف | `[Authorize(Roles = Admin + Receptionist)]` روی کلاس |
| 2 | **امنیت** | بستن جلسه دیگران با تغییر sessionId | ✅ برطرف | بررسی مالکیت + فقط صاحب جلسه یا Admin |
| 3 | **مالی** | ExpectedBalance از entity نه از SUM(تراکنش‌ها) | ⚠️ ریسک | ReceptionFacade هنگام پرداخت CashBalance را به‌روز می‌کند؛ برای تطابق کامل در فاز بعد از جدول تراکنش‌ها محاسبه شود |
| 4 | **Concurrency** | بستن همزمان یک جلسه توسط دو درخواست | ✅ برطرف | `TryCloseSessionConditionalAsync`: UPDATE شرطی در تراکنش؛ فقط یک درخواست موفق |
| 5 | **Concurrency** | شروع دو جلسه همزمان توسط یک کاربر (دو تب) | ⚠️ قابل قبول | `HasActiveSessionAsync` قبل از Add؛ در رقابت شدید ممکن است هر دو پاس شوند — در فاز بعد با قفل یا unique constraint |
| 6 | **Data loss** | بستن قبل از اعتبارسنجی | ✅ خیر | اعتبارسنجی مدل و وضعیت جلسه قبل از به‌روزرسانی |
| 7 | **Audit** | لاگ شروع/پایان جلسه | ✅ اضافه شد | Serilog AUDIT StartSession / AUDIT EndSession |
| 8 | **UX** | double-submit پایان جلسه | ✅ برطرف | غیرفعال کردن دکمه و اسپینر |
| 9 | **UX** | double-submit شروع جلسه | ✅ موجود | در View قبلاً پیاده شده |
| 10 | **Performance** | GetUserCashSessionsAsync: بارگذاری همه جلسات کاربر سپس Skip/Take در حافظه | ✅ برطرف | متد `GetByUserIdPagedAsync` در ریپازیتوری + استفاده در سرویس |
| 11 | **Performance** | GetStatisticsAsync: `.ToListAsync()` سپس `.Sum()` / `.Count()` در حافظه | ✅ برطرف | تجمیع در SQL با `GroupBy`/`Sum`/`Count` + کوئری جدا برای میانگین مدت |
| 12 | **DB** | SearchAsync استفاده از `SessionNumber` در Where | ✅ برطرف | جستجو با `CashSessionId` (عددی یا پیشوند CS) و `User.UserName` |

---

## ۲. جریان منطقی (Backend)

### ۲.۱ شروع جلسه — StartSession

| مورد | وضعیت |
|------|--------|
| بررسی جلسه فعال قبلی | ✅ `HasActiveSessionAsync(userId)` قبل از ایجاد |
| اعتبارسنجی مبلغ اولیه (≥۰) | ✅ در Validator و سرویس |
| ذخیره در DB | ✅ `AddAsync` یک بار `SaveChanges` |
| لاگ حسابرسی | ✅ لاگ `AUDIT StartSession` با SessionId, UserId, InitialAmount, OpenedAt |
| تراکنش DB | ❌ فقط یک Insert؛ در صورت نیاز می‌توان با TransactionScope پوشاند |

### ۲.۲ لیست جلسات — Sessions

| مورد | وضعیت |
|------|--------|
| وابسته به کاربر | ✅ با `GetUserCashSessionsAsync(userId, 1, 50)`؛ وقتی userId خالی است همه جلسات برگردانده می‌شود |
| صفحه‌بندی | ⚠️ در سرویس: ابتدا `GetByUserIdAsync(userId)` (همه جلسات کاربر)، سپس `Skip/Take` در حافظه — برای کاربران با جلسات زیاد ناکارآمد است |

### ۲.۳ جزئیات جلسه — SessionDetails

| مورد | وضعیت |
|------|--------|
| دریافت جلسه | ✅ `GetSessionByIdAsync` با `Include(User, UpdatedByUser, Transactions)` |
| مقادیر نمایشی | از entity: CurrentBalance, ExpectedBalance, Difference (محاسبه‌شده از CashBalance, PosBalance, OpeningBalance) |
| به‌روزرسانی CashBalance در زمان پرداخت | ✅ در `ReceptionFacade` هنگام ثبت پرداخت نقدی، `CashSession.CashBalance` به‌روز می‌شود |

### ۲.۴ پایان جلسه — EndSession

| مورد | وضعیت |
|------|--------|
| اعتبارسنجی مدل | ✅ FluentValidation |
| بررسی وجود و وضعیت جلسه | ✅ GetById سپس چک Open/Active و عدم بسته بودن |
| بررسی مالکیت | ✅ فقط صاحب جلسه یا Admin |
| مبلغ نهایی منفی | ✅ رد در سرویس |
| به‌روزرسانی و ذخیره | ✅ یک بار Update و SaveChanges |
| لاگ حسابرسی | ✅ لاگ `AUDIT EndSession` با مقادیر قبل/بعد و اختلاف |
| تراکنش DB | ❌ دو فراخوانی جدا (Get + Update)؛ احتمال race در بستن همزمان |

---

## ۳. محاسبات مالی و یکپارچگی

- **منبع CashBalance/PosBalance:** در شروع جلسه: `CashBalance = InitialAmount`, `PosBalance = 0`. در حین جلسه: `ReceptionFacade` با ثبت پرداخت نقدی، `CashBalance` را افزایش می‌دهد (و در سناریو لغو کاهش).
- **ExpectedBalance در entity:**  
  `ExpectedBalance = OpeningBalance + TotalIncome - TotalExpense`  
  با `TotalIncome = CashBalance + PosBalance` و `TotalExpense = 0`.  
  اگر همه پرداخت‌ها از همان Facade عبور کنند، این مقدار با مجموع تراکنش‌ها هم‌خوان است؛ برای تطابق قطعی و گزارش تفکیکی، در فاز بعد می‌توان Expected را از جدول تراکنش‌ها (مثلاً در `PaymentReconciliationService`) محاسبه و در نمایش/لاگ استفاده کرد.
- **جمع‌ها از ViewModel تنها نیست:** مبلغ نهایی پایان جلسه از فرم می‌آید (شمارش فیزیکی منشی) و با مقدار ذخیره‌شده در جلسه مقایسه/ثبت می‌شود؛ جمع تراکنش‌ها در سرویس reconciliation و در entity از همان ستون‌ها استفاده می‌شود.

---

## ۴. لایه دیتابیس

| مورد | وضعیت |
|------|--------|
| ایندکس روی CashSessionId | ✅ کلید اصلی و ایندکسهای مختلف روی UserId, OpenedAt, ClosedAt, Status, IsDeleted |
| ایندکس روی PaymentTransaction.CashSessionId | ✅ `IX_PaymentTransaction_CashSessionId` و ترکیبی با Status, CreatedAt |
| N+1 در GetByIdAsync | ✅ با `Include(User, UpdatedByUser, Transactions)` |
| الگوی `.ToList().Sum()` | ❌ در `GetStatisticsAsync` و `GetStatisticsAsync(start, end)` — همه ردیف‌ها با `ToListAsync()` بارگذاری سپس `Sum`/`Count` در حافظه. پیشنهاد: کوئری تجمیعی در SQL. |

---

## ۵. UI/UX (منشی صندوق)

| مورد | وضعیت |
|------|--------|
| کارت خلاصه مالی در SessionDetails | ✅ موجودی فعلی، مبلغ نهایی، اختلاف (قرمز/زرد) در مودال پایان جلسه |
| مودال تأیید پایان جلسه | ✅ مودال با فیلد مبلغ نهایی و توضیحات |
| جلوگیری از کلیک مکرر (شروع) | ✅ در View با غیرفعال کردن دکمه و اسپینر |
| جلوگیری از کلیک مکرر (پایان) | ✅ در View با غیرفعال کردن دکمه و متن «در حال بستن جلسه...» |
| نمایش اختلاف (Expected vs Actual) | ✅ در مودال با `#differenceAlert` و رنگ هشدار/خطا |

---

## ۶. امنیت

| مورد | وضعیت |
|------|--------|
| Authorize | ✅ `[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]` روی کنترلر |
| Anti-forgery | ✅ روی POSTهای StartSession و EndSession |
| مالکیت جلسه در EndSession | ✅ فقط صاحب جلسه یا Admin مجاز به بستن |
| دسترسی مستقیم با URL به جلسه دیگران | برای SessionDetails: هر کاربری با شناسه جلسه می‌تواند جزئیات را ببیند — در صورت نیاز سیاست «فقط جلسات خودم یا Admin» در همان اکشن قابل اعمال است. |

---

## ۷. لاگ و حسابرسی

| رویداد | محل | محتوا |
|--------|------|--------|
| شروع جلسه | PosManagementService.StartCashSessionAsync | لاگ اطلاعاتی + `AUDIT StartSession` با SessionId, UserId, InitialAmount, OpenedAt |
| پایان جلسه | PosManagementService.EndCashSessionAsync | لاگ اطلاعاتی + `AUDIT EndSession` با SessionId, EndedBy, OldStatus, OldCashBalance, NewCashBalance, Difference |
| تلاش بستن جلسه دیگران | PosManagementController.EndSession | لاگ هشدار با UserId, SessionId, OwnerId |

ثبت رسمی در جدول `CashSessionAuditLog` (مثلاً با `ICashSessionAuditService.LogActionAsync`) در فاز بعد پیشنهاد می‌شود.

---

## ۸. اصلاحات اعمال‌شده (فاز بهینه‌سازی)

1. **تراکنش و بستن شرطی در EndSession:** ✅ `TryCloseSessionConditionalAsync` در ریپازیتوری — تراکنش + `UPDATE ... WHERE CashSessionId = @id AND ClosedAt IS NULL AND Status = 1`؛ فقط یک درخواست موفق می‌شود.
2. **صفحه‌بندی در DB برای GetUserCashSessionsAsync:** ✅ متد `GetByUserIdPagedAsync` در ریپازیتوری و استفاده در سرویس.
3. **آمار بدون بارگذاری کامل:** ✅ `BuildStatisticsFromQueryAsync` با `GroupBy`/`Sum`/`Count` در SQL و کوئری جدا برای `AverageSessionDuration`.
4. **SearchAsync:** ✅ جستجو با `CashSessionId` (عدد یا پیشوند CS) و `User.UserName`؛ حذف استفاده از `SessionNumber`/`Description` محاسبه‌شده در Where.

**پیشنهاد فاز بعد (اختیاری):**
- **محاسبه Expected از تراکنش‌ها:** در GetSessionByIdAsync یا DTO جدا، محاسبه Expected از `Transactions` برای نمایش.
- **ثبت در CashSessionAuditLog:** فراخوانی `ICashSessionAuditService.LogActionAsync` پس از شروع/بستن جلسه.

---

## ۹. اولویت تولید

```
یکپارچگی مالی > امنیت داده > امنیت دسترسی > قابلیت اطمینان > UX > عملکرد
```

- امنیت دسترسی و مالکیت و لاگ حسابرسی برای شروع/پایان در این مرحله پوشش داده شده است.
- برای رسیدن به سطح بالای Production درمانی، در فاز بعد: تراکنش و قفل در EndSession، صفحه‌بندی واقعی در لیست جلسات، و تجمیع آماری در DB ضروری است.
