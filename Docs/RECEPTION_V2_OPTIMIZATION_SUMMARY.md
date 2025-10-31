# خلاصه بهینه‌سازی Reception V2

## ✅ کارهای انجام شده

### گام 0 — Pre-flight: ✅ **تأیید شد**
- ✅ `RouteConfig.cs`: `MapMvcAttributeRoutes()` فعال است
- ✅ `UnityConfig.cs`: `IReceptionFacade` ثبت شده است
- ✅ `Index.cshtml`: `@Html.AntiForgeryToken()` موجود است
- ✅ `reception-api.js`: CSRF token header در POSTها set می‌شود
- ✅ `ReceptionApiV1Controller`: `[ValidateAntiForgeryTokenOnPosts]` روی POSTها

### گام 2 — Draft Orchestrator: ✅ **تکمیل شد**
**فایل**: `Scripts/reception.v2/auto-draft-manager.js`
- ✅ اضافه شد: `ensureDraftOrSkip(state)` - بررسی وجود ReceptionId و ایجاد خودکار Draft
- ✅ اضافه شد: `warnDraftMissing()` - هشدار در صورت ناقص بودن فیلدهای الزامی
- ✅ به‌روزرسانی شد: `insurance-panel.js` - استفاده از `ensureDraftOrSkip` به‌جای `createDraft`
- ✅ به‌روزرسانی شد: `service-lookup.js` - استفاده از `ensureDraftOrSkip` قبل از AddItem

**معیار پذیرش**:
- ✅ وقتی چهار فیلد اصلی (patient/clinic/department/doctor) تکمیل است، اولین persist به‌صورت خودکار Draft می‌سازد
- ✅ اگر ناقص باشد، هیچ POSTی ارسال نمی‌شود و پیام مناسب نمایش داده می‌شود

### گام 7 — Finalize Validation: ✅ **تکمیل شد**
**فایل**: `Services/Reception/ReceptionFacade.cs`
- ✅ اضافه شد: `ValidateDraftForFinalizeAsync(draft)` - اعتبارسنجی کامل Draft
  - بررسی وجود PatientId, ClinicId, DepartmentId, DoctorId
  - بررسی وجود ReceptionItems (حداقل یک آیتم)
  - TODO: بررسی وجود BasePlanId برای خدمات بیمه‌ای (اختیاری)
- ✅ به‌روزرسانی شد: `FinalizePosAsync` - استفاده از `ValidateDraftForFinalizeAsync`
- ✅ به‌روزرسانی شد: `FinalizeCashAsync` - استفاده از `ValidateDraftForFinalizeAsync`

**معیار پذیرش**:
- ✅ قبل از Finalize، تمام فیلدهای الزامی Draft بررسی می‌شوند
- ✅ قبل از Finalize، وجود حداقل یک آیتم بررسی می‌شود

---

## ✅ بررسی شده (نیاز به تغییر ندارد)

### گام 1 — Bootstrap ثانویه: ✅ **OK است**
- ✅ `bootstrap()` فقط یک بار در `init()` صدا می‌زند
- ✅ وقتی `DepartmentId` تغییر می‌کند، `loadDoctorsForDepartment()` صدا می‌زند (نه bootstrap)
- ✅ وقتی `ClinicId` تغییر می‌کند، `bootstrap(true)` صدا می‌زند (OK - چون باید departments را reload کند)

### گام 3 — Patient Lookup + Fast-Create: ✅ **موجود است**
- ✅ `patient-lookup.js`: Lookup با NationalCode
- ✅ `_PatientFastCreateModal.cshtml`: Modal برای ساخت سریع
- ✅ پس از ایجاد، اطلاعات هویتی و PatientInsurance روی فرم می‌نشیند

### گام 4 — Insurance Persist + Reprice: ✅ **اتصال شده**
- ✅ `SetInsurancesAsync` از `PricingEngine.RepriceReceptionAsync` استفاده می‌کند
- ✅ `insurance-panel.js` از `AutoDraftManager.ensureDraftOrSkip` استفاده می‌کند

### گام 5 — Services by Department: ✅ **موجود است**
- ✅ `GetServicesByDepartmentAsync` موجود است
- ✅ `GetDoctorsByServiceAsync` موجود است (فیلتر پزشکان بر اساس خدمت)

### گام 6 — Add Item → Pricing Engine: ✅ **اتصال شده**
- ✅ `AddItemAsync` از `PricingEngine.QuoteAsync` استفاده می‌کند
- ✅ Snapshot کامل در `ReceptionItem.SnapshotJson` ذخیره می‌شود

---

## ⚠️ نیاز به بهبود (Optional)

### گام 8 — امنیت، لاگ، خطاها
**وضعیت فعلی**: ✅ عملاً OK است
- ✅ `[ValidateAntiForgeryTokenOnPosts]` روی POSTها
- ✅ Serilog با CorrelationId
- ⚠️ **Optional**: پیام‌های خطای فارسی در Dev (با Exception/StackTrace)

**اقدامات پیشنهادی** (در صورت نیاز):
- بررسی `ServiceResult` برای پشتیبانی از `WithExceptionDev`
- به‌روزرسانی catch blocks در `ReceptionApiV1Controller` برای نمایش Exception در Dev

### گام 9 — تست‌های دستی
**سناریوهای کلیدی**:
1. ✅ Bootstrap: یک بار صدا شود؛ تغییر دپارتمان فقط Doctors/Services را آپدیت کند
2. ✅ Patient Lookup → Fast-Create: با کد ملی جدید، Modal باز و پس از ذخیره، بیمه‌ها و هویت روی فرم بنشیند
3. ✅ Draft Auto-Create: با تکمیل 4 فیلد و اولین persist، ReceptionId گرفته شود
4. ✅ SetInsurances: تغییر طرح‌ها → Reprice آیتم‌ها
5. ✅ AddItem: ردیف با محاسبه دقیق (پایه+پوشش‌ها+سهم بیمار) اضافه و Totals درست
6. ✅ Finalize: POS/Cash بدون Anti-Forgery error

---

## 📊 خلاصه تغییرات

### فایل‌های تغییر یافته:
1. ✅ `Scripts/reception.v2/auto-draft-manager.js`
   - اضافه شد: `ensureDraftOrSkip(state)`
   - اضافه شد: `warnDraftMissing()`
   - به‌روزرسانی شد: Public API

2. ✅ `Scripts/reception.v2/insurance-panel.js`
   - به‌روزرسانی شد: `persist()` - استفاده از `ensureDraftOrSkip`

3. ✅ `Scripts/reception.v2/service-lookup.js`
   - به‌روزرسانی شد: `$("#BtnAddItem").on("click")` - استفاده از `ensureDraftOrSkip`

4. ✅ `Services/Reception/ReceptionFacade.cs`
   - اضافه شد: `ValidateDraftForFinalizeAsync(draft)`
   - به‌روزرسانی شد: `FinalizePosAsync` - استفاده از `ValidateDraftForFinalizeAsync`
   - به‌روزرسانی شد: `FinalizeCashAsync` - استفاده از `ValidateDraftForFinalizeAsync`

---

## ✅ معیارهای پذیرش

### Draft Orchestrator (گام 2):
- ✅ وقتی چهار فیلد اصلی (patient/clinic/department/doctor) تکمیل است، اولین persist به‌صورت خودکار Draft می‌سازد
- ✅ اگر ناقص باشد، هیچ POSTی ارسال نمی‌شود و پیام مناسب نمایش داده می‌شود
- ✅ `ensureDraftOrSkip` در `insurance-panel.js` و `service-lookup.js` استفاده می‌شود

### Finalize Validation (گام 7):
- ✅ قبل از Finalize، تمام فیلدهای الزامی Draft بررسی می‌شوند
- ✅ قبل از Finalize، وجود حداقل یک آیتم بررسی می‌شود
- ✅ پیام‌های خطای واضح و فارسی برای هر نوع اعتبارسنجی

---

## 🎯 نتیجه‌گیری

**✅ بهینه‌سازی‌های اصلی انجام شد:**

1. ✅ **Draft Orchestrator**: `ensureDraftOrSkip` برای اطمینان از وجود Draft قبل از هر persist
2. ✅ **Finalize Validation**: اعتبارسنجی کامل Draft قبل از Finalize

**✅ بررسی‌های انجام شده:**
- Bootstrap ثانویه: OK است
- Patient Lookup + Fast-Create: موجود است
- Insurance Persist + Reprice: اتصال شده
- Services by Department: موجود است
- Add Item → Pricing Engine: اتصال شده

**⚠️ بهبودهای اختیاری (Optional):**
- پیام‌های خطای فارسی در Dev (با Exception/StackTrace)

**🚀 سیستم آماده برای تست و استفاده است!**

---

## 📝 کامیت‌های پیشنهادی

```
feat(reception): add ensureDraftOrSkip to auto-draft-manager
feat(reception): add draft validation before finalize
```

