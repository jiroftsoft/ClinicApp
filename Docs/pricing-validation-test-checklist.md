# Pricing Validation Test - Checklist

**تاریخ:** 2025-11-07  
**هدف:** بررسی صحت محاسبات pricing و ذخیره در database

---

## 📋 **اطلاعات مورد نیاز برای Validation**

لطفاً این اطلاعات را از database برایم بفرستید:

### **1. اطلاعات Reception:**

```sql
SELECT 
    ReceptionId,
    PatientId,
    BasePlanId,
    SupplementaryPlanId,
    Status,
    TotalAmount,
    InsurerShareAmount,
    PatientCoPay,
    DiscountAmount,
    FinalPayableAmount,
    CreatedAt,
    UpdatedAt
FROM Receptions
WHERE ReceptionId = [ReceptionId شما]
```

### **2. اطلاعات ReceptionItems:**

```sql
SELECT 
    ReceptionItemId,
    ReceptionId,
    ServiceId,
    Quantity,
    UnitPrice,
    PatientShareAmount,
    InsurerShareAmount,
    SnapshotJson,
    IsDeleted
FROM ReceptionItems
WHERE ReceptionId = [ReceptionId شما]
AND IsDeleted = 0
ORDER BY ReceptionItemId
```

### **3. اطلاعات Service:**

```sql
SELECT 
    s.ServiceId,
    s.ServiceCode,
    s.Title,
    s.GroupCode,
    s.IsHashtagged
FROM Services s
WHERE s.ServiceId IN (
    SELECT ServiceId 
    FROM ReceptionItems 
    WHERE ReceptionId = [ReceptionId شما]
)
```

### **4. اطلاعات Insurance Plans:**

```sql
-- بیمه پایه
SELECT 
    InsurancePlanId,
    PlanName,
    InsuranceType,
    CoveragePercent,
    IsActive
FROM InsurancePlans
WHERE InsurancePlanId = [BasePlanId از Reception]

-- بیمه تکمیلی (اگر وجود دارد)
SELECT 
    InsurancePlanId,
    PlanName,
    InsuranceType,
    CoveragePercent,
    IsActive
FROM InsurancePlans
WHERE InsurancePlanId = [SupplementaryPlanId از Reception]
```

### **5. اطلاعات Tariffs:**

```sql
SELECT 
    it.InsuranceTariffId,
    it.InsurancePlanId,
    it.ServiceId,
    it.InsuranceType,
    it.TariffPrice,
    it.PatientShare,
    it.InsurerShare,
    it.SupplementaryCoveragePercent,
    it.SupplementaryMaxPayment,
    it.IsActive,
    ip.PlanName
FROM InsuranceTariffs it
INNER JOIN InsurancePlans ip ON it.InsurancePlanId = ip.InsurancePlanId
WHERE it.ServiceId IN (
    SELECT ServiceId 
    FROM ReceptionItems 
    WHERE ReceptionId = [ReceptionId شما]
)
AND it.InsurancePlanId IN ([BasePlanId], [SupplementaryPlanId])
AND it.IsActive = 1
AND it.IsDeleted = 0
ORDER BY it.ServiceId, it.InsuranceType
```

### **6. اطلاعات FactorSettings (ضرایب):**

```sql
SELECT 
    FactorSettingId,
    FinancialYear,
    ComponentType,
    Value,
    IsHashtagged,
    IsActive
FROM FactorSettings
WHERE FinancialYear = 1404  -- سال مالی جاری
AND IsActive = 1
AND IsDeleted = 0
```

---

## ✅ **فرمول‌های محاسبه برای Validation**

### **محاسبه هر آیتم (ReceptionItem):**

#### **گام 1: محاسبه قیمت پایه (UnitPrice)**

```
UnitPrice = (CoefTech × K_Tech) + (CoefProf × K_Prof)
```

**از کجا:**
- `CoefTech` و `CoefProf` → از `ServiceComponents` (Technical & Professional)
- `K_Tech` و `K_Prof` → از `FactorSettings` (با توجه به `IsHashtagged` و `FinancialYear`)

#### **گام 2: محاسبه Gross Amount**

```
GrossAmount = UnitPrice × Quantity
```

#### **گام 3: محاسبه سهم بیمه پایه**

**روش 1: از Tariff (اولویت دارد)**
```
اگر InsuranceTariff موجود است:
    BaseCovered = از TariffPrice یا محاسبه از InsurerShare
```

**روش 2: از Coverage درصد**
```
اگر Tariff نباشد (Fallback):
    BaseCovered = GrossAmount × (CoveragePercent / 100)
```

#### **گام 4: محاسبه سهم بیمه تکمیلی**

```
اگر بیمه تکمیلی وجود دارد:
    Remaining = GrossAmount - BaseCovered
    
    اگر SupplementaryCoveragePercent موجود است:
        SuppCovered = Remaining × (SupplementaryCoveragePercent / 100)
    
    اگر SupplementaryMaxPayment موجود است:
        SuppCovered = MIN(SuppCovered, SupplementaryMaxPayment)
```

#### **گام 5: محاسبه سهم بیمار**

```
PatientShare = GrossAmount - BaseCovered - SuppCovered
```

**توجه:** `PatientShare >= 0` باید باشد!

---

### **محاسبه مجموع Reception:**

```
TotalAmount = SUM(ReceptionItems.UnitPrice × Quantity)

InsurerShareAmount = SUM(ReceptionItems.InsurerShareAmount)
                   = SUM(BaseCovered + SuppCovered)

PatientCoPay = SUM(ReceptionItems.PatientShareAmount)

FinalPayableAmount = PatientCoPay - DiscountAmount
```

---

## 🧪 **Validation Checklist**

### **بررسی 1: صحت UnitPrice**

- [ ] `UnitPrice` در `ReceptionItems` مطابق با فرمول محاسبه شده؟
- [ ] `UnitPrice` مثبت است؟
- [ ] `UnitPrice` منطقی است (نه خیلی بزرگ، نه خیلی کوچک)

### **بررسی 2: صحت GrossAmount**

- [ ] `GrossAmount = UnitPrice × Quantity`؟
- [ ] `GrossAmount` در `SnapshotJson` ذخیره شده؟

### **بررسی 3: صحت سهم بیمه پایه**

- [ ] آیا `InsuranceTariff` برای بیمه پایه موجود است؟
- [ ] `BaseCovered` مطابق با Tariff محاسبه شده؟
- [ ] `BaseCovered <= GrossAmount`؟
- [ ] `BaseCovered >= 0`؟

### **بررسی 4: صحت سهم بیمه تکمیلی**

- [ ] آیا بیمار بیمه تکمیلی دارد؟
- [ ] اگر دارد، آیا `InsuranceTariff` برای بیمه تکمیلی موجود است؟
- [ ] `SuppCovered` مطابق با Tariff محاسبه شده؟
- [ ] `SuppCovered <= (GrossAmount - BaseCovered)`؟
- [ ] `SuppCovered >= 0`؟
- [ ] اگر `SupplementaryMaxPayment` وجود دارد، آیا اعمال شده؟

### **بررسی 5: صحت سهم بیمار**

- [ ] `PatientShare = GrossAmount - BaseCovered - SuppCovered`؟
- [ ] `PatientShare >= 0`؟
- [ ] اگر بیمار هر دو بیمه دارد و coverage 100% است، `PatientShare = 0`؟

### **بررسی 6: صحت SnapshotJson**

- [ ] `SnapshotJson` موجود است؟
- [ ] شامل تمام اطلاعات محاسبه (ServiceId, UnitPrice, BaseCovered, SuppCovered, PatientShare) است؟
- [ ] `CalculatedAt` timestamp دارد؟
- [ ] `BasePlanId` و `SupplementaryPlanId` در snapshot ذخیره شده؟

### **بررسی 7: صحت مجموع Reception**

- [ ] `TotalAmount` در `Receptions` = جمع `(UnitPrice × Quantity)` همه آیتم‌ها؟
- [ ] `InsurerShareAmount` = جمع `(BaseCovered + SuppCovered)` همه آیتم‌ها؟
- [ ] `PatientCoPay` = جمع `PatientShare` همه آیتم‌ها؟
- [ ] `TotalAmount = InsurerShareAmount + PatientCoPay`؟

### **بررسی 8: Consistency Checks**

- [ ] `InsurerShareAmount` در `ReceptionItems` مطابق با `Reception.InsurerShareAmount`؟
- [ ] `PatientShareAmount` در `ReceptionItems` مطابق با `Reception.PatientCoPay`؟
- [ ] همه آیتم‌ها `IsDeleted = 0` هستند؟
- [ ] `UpdatedAt` در `Reception` بعد از آخرین تغییر است؟

---

## 📊 **مثال Validation:**

### **سناریو نمونه:**

**خدمت:** ویزیت روانپزشکی (ServiceId: 487)  
**بیمه پایه:** بیمه سلامت (InsurancePlanId: 1012) - Coverage: 70%  
**بیمه تکمیلی:** بیمه دانا (InsurancePlanId: 1020) - Coverage: 100%  
**Quantity:** 1

**محاسبات مورد انتظار:**

```
UnitPrice = 3,851,000 ریال (از FactorSettings و ServiceComponents)
GrossAmount = 3,851,000 × 1 = 3,851,000 ریال

BaseCovered (70%) = 3,851,000 × 0.70 = 2,695,700 ریال
Remaining = 3,851,000 - 2,695,700 = 1,155,300 ریال

SuppCovered (100%) = 1,155,300 × 1.00 = 1,155,300 ریال

PatientShare = 3,851,000 - 2,695,700 - 1,155,300 = 0 ریال ✅
```

**انتظار در Database:**

```
ReceptionItems:
    UnitPrice = 3,851,000
    PatientShareAmount = 0
    InsurerShareAmount = 3,851,000

Receptions:
    TotalAmount = 3,851,000
    InsurerShareAmount = 3,851,000
    PatientCoPay = 0
```

---

## 🚨 **نشانه‌های مشکل:**

### **مشکل 1: PatientShare منفی**
```
❌ PatientShare = -500,000
```
**دلیل احتمالی:** محاسبه بیمه تکمیلی بیشتر از باقیمانده

### **مشکل 2: مجموع‌ها مطابقت ندارند**
```
❌ TotalAmount != InsurerShareAmount + PatientCoPay
```
**دلیل احتمالی:** خطا در محاسبه یا rounding

### **مشکل 3: Tariff موجود نیست**
```
⚠️ BaseCovered = GrossAmount × 0.70  (از درصد کلی، نه از Tariff)
```
**دلیل احتمالی:** InsuranceTariff تعریف نشده (مشکل Critical!)

### **مشکل 4: SnapshotJson ناقص**
```
❌ SnapshotJson = null یا فقط بخشی از اطلاعات دارد
```
**دلیل احتمالی:** خطا در serialize/deserialize

---

## 📝 **فرمت ارسال نتایج:**

لطفاً نتایج را به این صورت ارسال کنید:

```
=== اطلاعات سناریو ===
بیمار: [PatientId]
بیمه پایه: [PlanName] (Coverage: [X]%)
بیمه تکمیلی: [PlanName] (Coverage: [Y]%) یا "ندارد"
خدمات: [تعداد] آیتم

=== Reception (جدول Receptions) ===
ReceptionId: [X]
TotalAmount: [X] ریال
InsurerShareAmount: [X] ریال
PatientCoPay: [X] ریال
FinalPayableAmount: [X] ریال

=== ReceptionItems (جدول ReceptionItems) ===
آیتم 1:
    ServiceId: [X] - [Service Name]
    Quantity: [X]
    UnitPrice: [X] ریال
    PatientShareAmount: [X] ریال
    InsurerShareAmount: [X] ریال
    SnapshotJson: [paste کنید یا بگویید موجود/ناموجود]

آیتم 2: ...

=== Tariffs ===
آیا برای تمام خدمات InsuranceTariff موجود است؟
- Service 1: بله/خیر
- Service 2: بله/خیر

=== سوالات خاص ===
- آیا PatientCoPay صفر شد (با بیمه تکمیلی)؟
- آیا محاسبات دستی شما با database مطابقت دارد؟
- آیا پیغام خطایی در console دیدید؟
```

---

## ✅ **آماده هستم!**

حالا شما می‌توانید:
1. یک reception ایجاد کنید
2. خدمات اضافه کنید
3. بیمه‌ها را set کنید
4. Query‌های بالا را اجرا کنید
5. نتایج را برایم بفرستید

من با استفاده از این checklist، دقیقاً بررسی می‌کنم که آیا محاسبات **100% صحیح** هستند یا نه! 🎯

