# ✅ ClinicApp – One-Page View Review Checklist (Healthcare UI)

> **Purpose:**  
> Fast, consistent, contract-driven review of Razor Views / UI modules in **ClinicApp**  
> Designed for healthcare admin environments (formal, readable, fast).

---

## 0) Preflight
- [ ] Contracts under `CONTRACTS/` respected (report only violations)
- [ ] Scope is clear (Views / Partials / Controller / ViewModels)
- [ ] Change risk assessed: **Low / Medium / High**

---

## 1) Healthcare UI Standards (Formal & Readable)
- [ ] Formal administrative look (no flashy / jelf colors)
- [ ] High readability (spacing, typography, labels)
- [ ] No heavy animations or visual noise
- [ ] Fast rendering (no unnecessary assets or DOM bloat)

---

## 2) SRP & Responsibility Boundaries
- [ ] View is **PASSIVE** (no business logic in Razor)
- [ ] Controller is orchestration-only
- [ ] Business rules live in Services
- [ ] No Entity used directly in View
- [ ] Entity → ViewModel via **Factory Method only**

---

## 3) AJAX & No-Refresh Modules
- [ ] Parts that must not refresh are **AJAX-based**
- [ ] Partial endpoints are small and focused
- [ ] Loading state is simple and clear
- [ ] Error state + retry handled
- [ ] No full-page refresh where unnecessary

---

## 4) Validation (Sensitive Forms)
- [ ] Server-side validation is authoritative
- [ ] Client-side validation exists for UX
- [ ] Error messages are clear and near fields
- [ ] Edge cases handled (null, range, format, duplicates)

---

## 5) ServiceResult Enhanced Usage
- [ ] Controller/Service returns **ServiceResult Enhanced**
- [ ] UI handles Success / ValidationError / Error states
- [ ] No sensitive data exposed in error messages

---

## 6) Reuse & Anti-Duplication
- [ ] Existing Partial / Helper / Template searched first
- [ ] No duplicate ViewModels or Helpers created
- [ ] Repeated markup extracted to Partial/Template
- [ ] Naming and folder structure respected

---

## 7) Performance & Anti-Bottleneck
- [ ] No chatty AJAX calls
- [ ] No N+1 queries affecting rendering or submit
- [ ] Payload size minimized
- [ ] Heavy forms optimized

---

## 8) Tests & Verification
- [ ] Relevant tests added/updated (unit/integration)
- [ ] Regression scenarios identified
- [ ] Manual verification steps documented
- [ ] Rollback path defined (especially for Medium/High risk)

---

## 🔹 PR Review Summary (Fill This)
- **Issues (max 3):**
- **Root Cause:**
- **Fix (minimal & safe):**
- **Tests / Verification:**
- **Rollback:**

---

**Owner:** ClinicApp Engineering  
**Category:** UI / View Review  
**Format:** One-page checklist  
