# برنامه بهینه‌سازی Reception V2 (مرحله‌به‌مرحله)

## ✅ وضعیت فعلی (Pre-Audit)

### گام 0 — Pre-flight: ✅ **تأیید شد**
- ✅ `RouteConfig.cs`: `MapMvcAttributeRoutes()` فعال است
- ✅ `UnityConfig.cs`: `IReceptionFacade` ثبت شده است
- ✅ `Index.cshtml`: `@Html.AntiForgeryToken()` موجود است
- ✅ `reception-api.js`: CSRF token header در POSTها set می‌شود
- ✅ `ReceptionApiV1Controller`: `[ValidateAntiForgeryTokenOnPosts]` روی POSTها موجود است

### گام 1 — Bootstrap ثانویه: ✅ **عملاً OK است**
- ✅ `bootstrap()` فقط یک بار در `init()` صدا می‌زند
- ✅ وقتی `DepartmentId` تغییر می‌کند، `loadDoctorsForDepartment()` صدا می‌زند (نه bootstrap)
- ⚠️ وقتی `ClinicId` تغییر می‌کند، `bootstrap(true)` صدا می‌زند (OK - چون باید departments را reload کند)

### گام 2 — Draft Orchestrator: ⚠️ **نیاز به بهینه‌سازی**
- ✅ `AutoDraftManager.createDraft()` موجود است
- ✅ `insurance-panel.js` از `AutoDraftManager` استفاده می‌کند
- ⚠️ **مشکل**: `ensureDraftOrSkip` به عنوان یک تابع مستقل و قابل استفاده در همه جا وجود ندارد
- **اقدام**: ایجاد `ensureDraftOrSkip()` در `auto-draft-manager.js` و استفاده از آن در همه ماژول‌ها

### گام 3 — Patient Lookup + Fast-Create: ✅ **موجود است**
- ✅ `patient-lookup.js`: Lookup با NationalCode
- ✅ `_PatientFastCreateModal.cshtml`: Modal برای ساخت سریع
- ✅ پس از ایجاد، اطلاعات هویتی و PatientInsurance روی فرم می‌نشیند

### گام 4 — Insurance Persist + Reprice: ✅ **اتصال شده**
- ✅ `SetInsurancesAsync` از `PricingEngine.RepriceReceptionAsync` استفاده می‌کند
- ✅ `insurance-panel.js` از `AutoDraftManager` برای ایجاد draft استفاده می‌کند

### گام 5 — Services by Department: ✅ **موجود است**
- ✅ `GetServicesByDepartmentAsync` موجود است
- ⚠️ **اختیاری**: فیلتر پزشکان بر اساس خدمت (در `GetDoctorsByServiceAsync` موجود است)

### گام 6 — Add Item → Pricing Engine: ✅ **اتصال شده**
- ✅ `AddItemAsync` از `PricingEngine.QuoteAsync` استفاده می‌کند
- ✅ Snapshot کامل در `ReceptionItem.SnapshotJson` ذخیره می‌شود

### گام 7 — Finalize: ✅ **موجود است**
- ✅ `FinalizePosAsync` و `FinalizeCashAsync` موجود هستند
- ⚠️ **نیاز به بهبود**: اعتبارسنجی کامل Draft و بیمه قبل از Finalize

### گام 8 — امنیت، لاگ، خطاها: ✅ **عملاً OK است**
- ✅ `[ValidateAntiForgeryTokenOnPosts]` روی POSTها
- ✅ Serilog با CorrelationId
- ⚠️ **نیاز به بهبود**: پیام‌های خطای فارسی در Dev (با Exception/StackTrace)

## 📋 اقدامات بهینه‌سازی

### 1. Draft Orchestrator (گام 2)
**فایل**: `Scripts/reception.v2/auto-draft-manager.js`
- اضافه کردن `ensureDraftOrSkip(state)` به عنوان تابع عمومی
- استفاده از آن در `insurance-panel.js`, `service-lookup.js`, و سایر ماژول‌ها

### 2. Finalize Validation (گام 7)
**فایل**: `Services/Reception/ReceptionFacade.cs`
- اضافه کردن اعتبارسنجی کامل Draft (PatientId, ClinicId, DepartmentId, DoctorId)
- اضافه کردن اعتبارسنجی بیمه (BasePlanId برای خدمات بیمه‌ای)

### 3. Error Messages در Dev (گام 8)
**فایل**: `Controllers/Api/ReceptionApiV1Controller.cs`
- اضافه کردن `WithExceptionDev(ex)` به `ServiceResult` در catch blocks

### 4. Doctors by Service (گام 5 - اختیاری)
**فایل**: `Scripts/reception.v2/service-lookup.js`
- فعال کردن فیلتر پزشکان بر اساس خدمت انتخاب شده

---

## 🎯 اولویت‌بندی

### اولویت بالا (Blocker):
1. ✅ **گام 0** - Pre-flight: تأیید شد
2. ⚠️ **گام 2** - Draft Orchestrator: نیاز به بهینه‌سازی
3. ⚠️ **گام 7** - Finalize Validation: نیاز به بهبود

### اولویت متوسط:
4. ⚠️ **گام 8** - Error Messages: نیاز به بهبود
5. ⚠️ **گام 5** - Doctors by Service: اختیاری اما مفید

### اولویت پایین (Optional):
6. ✅ **گام 1** - Bootstrap: عملاً OK است
7. ✅ **گام 3** - Patient Lookup: موجود است
8. ✅ **گام 4** - Insurance Persist: اتصال شده
9. ✅ **گام 6** - Add Item: اتصال شده

---

## 🔄 مراحل اجرا

### مرحله 1: Draft Orchestrator (گام 2)
- [ ] اضافه کردن `ensureDraftOrSkip()` به `auto-draft-manager.js`
- [ ] به‌روزرسانی `insurance-panel.js` برای استفاده از `ensureDraftOrSkip`
- [ ] به‌روزرسانی `service-lookup.js` برای استفاده از `ensureDraftOrSkip`

### مرحله 2: Finalize Validation (گام 7)
- [ ] اضافه کردن `ValidateDraftForFinalize()` به `ReceptionFacade`
- [ ] استفاده از آن در `FinalizePosAsync` و `FinalizeCashAsync`

### مرحله 3: Error Messages (گام 8)
- [ ] بررسی `ServiceResult` برای پشتیبانی از `WithExceptionDev`
- [ ] به‌روزرسانی catch blocks در `ReceptionApiV1Controller`

### مرحله 4: Doctors by Service (گام 5 - اختیاری)
- [ ] فعال کردن فیلتر پزشکان در `service-lookup.js`

---

## ✅ معیارهای پذیرش

### Draft Orchestrator:
- ✅ وقتی چهار فیلد اصلی (patient/clinic/department/doctor) تکمیل است، اولین persist به‌صورت خودکار Draft می‌سازد
- ✅ اگر ناقص باشد، هیچ POSTی ارسال نشود و پیام مناسب نمایش داده شود

### Finalize Validation:
- ✅ قبل از Finalize، تمام فیلدهای الزامی Draft بررسی شوند
- ✅ بیمه پایه برای خدمات بیمه‌ای بررسی شود

### Error Messages:
- ✅ در Dev، پیام‌های خطا شامل Exception/StackTrace باشند
- ✅ در Prod، پیام‌های عمومی نمایش داده شوند

---

## 📝 کامیت‌های پیشنهادی

1. `feat(reception): add ensureDraftOrSkip to auto-draft-manager`
2. `feat(reception): add draft validation before finalize`
3. `feat(reception): enhance error messages with dev details`
4. `feat(reception): enable doctors filter by service (optional)`

