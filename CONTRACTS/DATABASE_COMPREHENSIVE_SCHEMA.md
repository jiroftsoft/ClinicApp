# 📊 **مستند جامع Database Schema - کلینیک شفا**

## 🎯 **هدف**
این مستند یک نمای کامل از تمام Entities، روابط، و ساختار دیتابیس سیستم کلینیک شفا ارائه می‌دهد.

---

## 🏗️ **معماری کلی سیستم**

```
┌─────────────────────────────────────────────────────────────┐
│                         CORE LAYER                          │
├─────────────────────────────────────────────────────────────┤
│  • ApplicationUser (Identity + ISoftDelete + ITrackable)    │
│  • ISoftDelete Interface                                    │
│  • ITrackable Interface                                     │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│                     BUSINESS ENTITIES                       │
├─────────────────────────────────────────────────────────────┤
│  1. Clinic Management    →  Clinic, Department, Service...  │
│  2. Patient Management   →  Patient, PatientInsurance...    │
│  3. Doctor Management    →  Doctor, Specialization...       │
│  4. Insurance Management →  Insurance*, Tariff, Plan...     │
│  5. Reception Management →  Reception, ReceptionItem        │
│  6. Payment Management   →  Payment, Transaction, Gateway   │
│  7. Appointment          →  Appointment, Slot...            │
└─────────────────────────────────────────────────────────────┘

