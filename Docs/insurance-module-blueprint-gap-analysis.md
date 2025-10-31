# 📊 Gap Analysis: Insurance Module Blueprint vs Current Implementation

**تاریخ**: 2025-01-17  
**بلوپرینت**: Insurance Module (end‑to‑end Blueprint v1.0)  
**وضعیت**: 🟡 Gap Analysis

---

## 🎯 خلاصه اجرایی

| بخش | بلوپرینت | وضعیت فعلی | Gap | اولویت |
|-----|----------|------------|-----|--------|
| Entities | ✅ کامل | 🟡 جزئی | ⚠️ متوسط | High |
| Services | ✅ کامل | 🟡 جزئی | ⚠️ متوسط | High |
| API v1 | ✅ کامل | 🔴 ندارد | ❌ بالا | High |
| Snapshot | ✅ کامل | 🟢 SnapshotJson | ✅ کم | Medium |
| Pricing | ✅ کامل | 🟡 ساده | ⚠️ متوسط | High |
| Coverage | ✅ کامل | 🔴 ندارد | ❌ بالا | High |

---

## 📋 مقایسه جزئی

### 1. Entities

#### ✅ موجود در پروژه:
- `InsuranceProvider` ✅
- `InsurancePlan` ✅
- `InsuranceTariff` ✅
- `PatientInsurance` ✅
- `ReceptionItem.SnapshotJson` ✅

#### ❌ **Missing (طبق بلوپرینت)**:

##### 1.1 InsuranceContract
```csharp
// بلوپرینت پیشنهاد می‌کند:
InsuranceContract {
    Id, PlanId*, FromDate, ToDate, ContractCode, IsActive
}
```
**وضعیت فعلی**: ❌ ندارد  
**جایگزین فعلی**: احتمالاً از `InsurancePlan.FromDate/ToDate` استفاده می‌شود  
**Gap**: بدون Contract، نمی‌توان قراردادهای متعدد برای یک Plan داشت

##### 1.2 PlanCoverage
```csharp
// بلوپرینت پیشنهاد می‌کند:
PlanCoverage {
    Id, ContractId*, ServiceGroupId*, CoverageRate, FranchisePct, 
    AnnualCap, PerServiceCap, DailyCap, VisitCap, Notes
}
```
**وضعیت فعلی**: ❌ ندارد  
**جایگزین فعلی**: احتمالاً از `InsurancePlan.CoveragePercent` و `InsurancePlan.Deductible` استفاده می‌شود  
**Gap**: بدون PlanCoverage، سقف‌های سالانه/روزانه/خدمت قابل پیاده‌سازی نیست

##### 1.3 PlanTariff
```csharp
// بلوپرینت پیشنهاد می‌کند:
PlanTariff {
    Id, ContractId*, ServiceCode*, HashtagGroup, 
    K_Professional, K_Technical, PriceOverride?, Notes
}
```
**وضعیت فعلی**: ✅ `InsuranceTariff` موجود است (اما ساختار متفاوت)  
**Gap**: ساختار `InsuranceTariff` فعلی احتمالاً با `PlanTariff` بلوپرینت متفاوت است

##### 1.4 ReceptionInsuranceSnapshot
```csharp
// بلوپرینت پیشنهاد می‌کند:
ReceptionInsuranceSnapshot {
    ReceptionId*, BasePlanId?, SuppPlanId?, FactorSettingId, 
    ProviderSummaries(JSON), CreatedAtUtc, RowVersion
}
```
**وضعیت فعلی**: ❌ ندارد  
**جایگزین فعلی**: احتمالاً از `Reception.BasePlanId/SupplementaryPlanId` استفاده می‌شود  
**Gap**: بدون Snapshot جداگانه، نمی‌توان تاریخچه کامل بیمه در پذیرش را ذخیره کرد

##### 1.5 ReceptionItemSnapshot (Entity جداگانه)
```csharp
// بلوپرینت پیشنهاد می‌کند:
ReceptionItemSnapshot {
    ItemId*, ServiceCode, HashtagGroup, K_Prof, K_Tech, ...
    CalcDetails(JSON), CreatedAtUtc, RowVersion
}
```
**وضعیت فعلی**: ✅ `ReceptionItem.SnapshotJson` (nvarchar(MAX))  
**Gap**: ⚠️ کم - در حال حاضر JSON string هست، بلوپرینت Entity جداگانه پیشنهاد می‌کند (اما `SnapshotJson` کافی است)

##### 1.6 ServiceGroup
```csharp
// بلوپرینت پیشنهاد می‌کند:
ServiceGroup {
    Id, Code, Name
}
```
**وضعیت فعلی**: ❓ احتمالاً از `Service.GroupCode` استفاده می‌شود  
**Gap**: اگر `GroupCode` کافی باشد، نیازی به Entity جداگانه نیست

---

### 2. Services

#### ✅ موجود در پروژه:
- `InsurancePlanService` ✅
- `InsuranceProviderService` ✅
- `PatientInsuranceService` ✅
- `CombinedInsuranceCalculationService` ✅
- `InsuranceTariffService` ✅
- `InsuranceValidationService` ✅

#### ❌ **Missing (طبق بلوپرینت)**:

##### 2.1 IInsuranceDictionaryService
```csharp
// بلوپرینت پیشنهاد می‌کند:
IInsuranceDictionaryService {
    GetProvidersAsync()
    GetPlansAsync(providerId?, type?)
    GetContractsAsync(planId?)
    GetGroupsAsync()
}
```
**وضعیت فعلی**: ❌ ندارد  
**جایگزین فعلی**: احتمالاً از `InsuranceProviderService` و `InsurancePlanService` استفاده می‌شود  
**Gap**: بدون Dictionary Service یکپارچه، API v1 نمی‌تواند endpointهای `/providers`, `/plans`, `/contracts` را ارائه دهد

##### 2.2 IInsuranceCoverageService
```csharp
// بلوپرینت پیشنهاد می‌کند:
IInsuranceCoverageService {
    GetCoverageSummaryAsync(patientId?, receptionId?)
    GetAccumulatorsAsync(patientId, receptionDate)
    CheckCapsAsync(...)
}
```
**وضعیت فعلی**: ❌ ندارد  
**جایگزین فعلی**: احتمالاً منطق Coverage در `CombinedInsuranceCalculationService` هست  
**Gap**: بدون Coverage Service جداگانه، API `/coverage/summary` قابل پیاده‌سازی نیست

##### 2.3 IInsurancePricingService
```csharp
// بلوپرینت پیشنهاد می‌کند:
IInsurancePricingService {
    QuoteAsync(request)
    RepriceAsync(receptionId)
}
```
**وضعیت فعلی**: ❌ ندارد  
**جایگزین فعلی**: منطق Pricing در `ReceptionFacade.AddItemAsync` و `SetInsurancesAsync` هست  
**Gap**: بدون Pricing Service جداگانه، API `/quote` و `/reprice` قابل پیاده‌سازی نیست

---

### 3. API Endpoints

#### ❌ **Missing (طبق بلوپرینت)**:

##### 3.1 `/api/v1/insurance/providers` (GET)
**وضعیت فعلی**: ❌ ندارد  
**Action**: باید `InsuranceApiV1Controller` ایجاد شود

##### 3.2 `/api/v1/insurance/plans` (GET)
**وضعیت فعلی**: ❌ ندارد  
**Action**: باید در `InsuranceApiV1Controller` اضافه شود

##### 3.3 `/api/v1/insurance/contracts` (GET)
**وضعیت فعلی**: ❌ ندارد (چون `InsuranceContract` Entity ندارد)  
**Action**: اول باید Entity و سپس API ایجاد شود

##### 3.4 `/api/v1/insurance/coverage/summary` (GET)
**وضعیت فعلی**: ❌ ندارد  
**Action**: باید `IInsuranceCoverageService` و سپس API ایجاد شود

##### 3.5 `/api/v1/insurance/patient/{id}/insurances` (GET)
**وضعیت فعلی**: ❌ ندارد  
**Action**: باید در `InsuranceApiV1Controller` اضافه شود

##### 3.6 `/api/v1/insurance/patient/{id}/insurances/set` (POST)
**وضعیت فعلی**: ❌ ندارد  
**Action**: باید در `InsuranceApiV1Controller` اضافه شود (با CSRF)

##### 3.7 `/api/v1/insurance/reprice` (POST)
**وضعیت فعلی**: ❌ ندارد  
**Action**: باید `IInsurancePricingService` و سپس API ایجاد شود

##### 3.8 `/api/v1/insurance/quote` (POST)
**وضعیت فعلی**: ✅ `/api/v1/reception/item/price/preview` موجود است (اما در ReceptionApiV1Controller)  
**Gap**: بلوپرینت `/api/v1/insurance/quote` پیشنهاد می‌کند

---

### 4. Pricing Logic

#### ✅ موجود در پروژه:
- `ReceptionFacade.AddItemAsync` ✅ (محاسبه سهم بیمار/بیمه)
- `ReceptionFacade.SetInsurancesAsync` ✅ (Reprice-on-change)
- `ServiceCalculationEngine.CalculateUnitPriceIRRAsync` ✅ (محاسبه قیمت پایه)
- `CombinedInsuranceCalculationService` ✅ (محاسبه ترکیبی)

#### ⚠️ **Gap (طبق بلوپرینت)**:

##### 4.1 فرمول پایه
```pseudo
// بلوپرینت:
BasePrice = (K_Professional * Factor.K_prof_IRR) + (K_Technical * Factor.K_tech_IRR)
If PlanTariff.PriceOverride exists => BasePrice = PriceOverride
```
**وضعیت فعلی**: ✅ `ServiceCalculationEngine` همین فرمول را پیاده کرده  
**Gap**: ⚠️ کم - احتمالاً `PriceOverride` در `InsuranceTariff` نیست

##### 4.2 فرانشیز و سقف‌ها
```pseudo
// بلوپرینت:
PatientBaseFr = CoveredBase * FranchisePct_Base
Caps Applied → Remaining to PatientShare
```
**وضعیت فعلی**: ❌ فرانشیز از `InsurancePlan.Deductible` استفاده می‌شود، اما سقف‌ها (AnnualCap, DailyCap, VisitCap) پیاده نیست  
**Gap**: ❌ بالا - سقف‌ها و Accumulatorها پیاده نشده

##### 4.3 Hashtag Groups (G1..G7)
```pseudo
// بلوپرینت:
Service.HashtagGroup(G1..G7)
PlanCoverage.ServiceGroupId
HashtagMultipliers in FactorSetting
```
**وضعیت فعلی**: ✅ `Service.GroupCode` موجود است  
**Gap**: ⚠️ کم - احتمالاً `HashtagMultipliers` در `FactorSetting` نیست

---

### 5. Snapshot

#### ✅ موجود در پروژه:
- `ReceptionItem.SnapshotJson` ✅ (nvarchar(MAX))

#### ⚠️ **Gap (طبق بلوپرینت)**:

##### 5.1 ReceptionInsuranceSnapshot (Entity جداگانه)
**وضعیت فعلی**: ❌ ندارد  
**جایگزین فعلی**: از `Reception.BasePlanId/SupplementaryPlanId` استفاده می‌شود  
**Gap**: ⚠️ متوسط - بدون Entity جداگانه، نمی‌توان `ProviderSummaries(JSON)` و `FactorSettingId` را ذخیره کرد

##### 5.2 CalcDetails در SnapshotJson
**وضعیت فعلی**: ✅ `ReceptionFacade.AddItemAsync` `SnapshotJson` را با جزئیات کامل ایجاد می‌کند  
**Gap**: ✅ کم - محتوای `SnapshotJson` احتمالاً با `CalcDetails` بلوپرینت همخوان است

---

## 🎯 Action Plan (موارد ضروری)

### Phase 1: Entities (High Priority)

#### 1.1 InsuranceContract ✅ ضروری
- [ ] Entity ایجاد شود
- [ ] Fluent API Configuration
- [ ] Migration
- [ ] Repository Interface + Implementation

#### 1.2 PlanCoverage ✅ ضروری (برای سقف‌ها)
- [ ] Entity ایجاد شود
- [ ] Fluent API Configuration
- [ ] Migration
- [ ] Repository Interface + Implementation

#### 1.3 ReceptionInsuranceSnapshot ⚠️ متوسط (اختیاری اگر Reception.BasePlanId کافی است)
- [ ] تصمیم: آیا Entity جداگانه لازم است؟
- [ ] اگر بله: Entity + Migration + Repository

---

### Phase 2: Services (High Priority)

#### 2.1 IInsuranceDictionaryService ✅ ضروری
- [ ] Interface ایجاد شود
- [ ] Implementation: `InsuranceDictionaryService`
- [ ] DI Registration

#### 2.2 IInsuranceCoverageService ✅ ضروری
- [ ] Interface ایجاد شود
- [ ] Implementation: `InsuranceCoverageService`
- [ ] DI Registration

#### 2.3 IInsurancePricingService ✅ ضروری
- [ ] Interface ایجاد شود
- [ ] Implementation: `InsurancePricingService` (استفاده از منطق موجود در `ReceptionFacade`)
- [ ] DI Registration

---

### Phase 3: API v1 (High Priority)

#### 3.1 InsuranceApiV1Controller ✅ ضروری
- [ ] Controller ایجاد شود: `[RoutePrefix("api/v1/insurance")]`
- [ ] `[OutputCache(NoStore = true)]` روی class
- [ ] `[ValidateAntiForgeryToken]` روی POSTها
- [ ] Endpoints:
  - [ ] `GET /providers`
  - [ ] `GET /plans`
  - [ ] `GET /contracts` (اگر InsuranceContract ایجاد شد)
  - [ ] `GET /coverage/summary`
  - [ ] `GET /patient/{id}/insurances`
  - [ ] `POST /patient/{id}/insurances/set`
  - [ ] `POST /reprice`
  - [ ] `POST /quote`

---

### Phase 4: Pricing Logic Enhancements (Medium Priority)

#### 4.1 سقف‌ها و Accumulatorها
- [ ] `PlanCoverage` Entity (Phase 1)
- [ ] Accumulator Logic در `InsuranceCoverageService`
- [ ] Check Caps در `AddItemAsync`

#### 4.2 Hashtag Multipliers
- [ ] `FactorSetting.HashtagMultipliersJson` اضافه شود
- [ ] Migration
- [ ] Logic در `ServiceCalculationEngine`

#### 4.3 PriceOverride در Tariff
- [ ] `InsuranceTariff.PriceOverride` اضافه شود (اگر نیست)
- [ ] Migration
- [ ] Logic در Pricing

---

### Phase 5: Frontend Integration (Medium Priority)

#### 5.1 insurance-panel.js
- [ ] استفاده از `/api/v1/insurance/coverage/summary`
- [ ] نمایش فرانشیز و سقف‌ها
- [ ] Toast برای OverCap

#### 5.2 Coverage Modal
- [ ] استفاده از `/api/v1/insurance/coverage/summary`
- [ ] نمایش جزئیات پوشش

---

## ⚠️ تصمیم‌های مهم

### 1. InsuranceContract Entity
**سوال**: آیا قراردادهای متعدد برای یک Plan لازم است؟  
**پیشنهاد**: اگر `InsurancePlan.FromDate/ToDate` کافی است، نیازی به Entity جداگانه نیست  
**Action**: بررسی نیاز کسب‌وکار

### 2. ReceptionInsuranceSnapshot Entity
**سوال**: آیا Entity جداگانه لازم است یا `Reception.BasePlanId/SupplementaryPlanId` کافی است؟  
**پیشنهاد**: اگر `SnapshotJson` در `ReceptionItem` کافی است، نیازی به Entity جداگانه نیست  
**Action**: بررسی نیاز Audit/Reporting

### 3. ReceptionItemSnapshot Entity
**سوال**: آیا Entity جداگانه لازم است یا `ReceptionItem.SnapshotJson` کافی است؟  
**پیشنهاد**: ✅ `SnapshotJson` (nvarchar(MAX)) کافی است  
**Action**: ✅ فعلاً نیازی به Entity جداگانه نیست

---

## 📝 جمع‌بندی

### ✅ موارد ضروری (High Priority):
1. **Entities**: `InsuranceContract`, `PlanCoverage` (برای سقف‌ها)
2. **Services**: `IInsuranceDictionaryService`, `IInsuranceCoverageService`, `IInsurancePricingService`
3. **API v1**: `InsuranceApiV1Controller` با تمام endpoints

### ⚠️ موارد متوسط (Medium Priority):
1. **Pricing Logic**: سقف‌ها، Accumulatorها، Hashtag Multipliers
2. **Frontend**: ادغام با `insurance-panel.js`

### ✅ موارد اختیاری (Low Priority):
1. `ReceptionInsuranceSnapshot` Entity (اگر نیاز Audit/Reporting باشد)
2. `ReceptionItemSnapshot` Entity (اگر `SnapshotJson` کافی نباشد)

---

**آماده برای پیاده‌سازی Phase 1!** 🚀

