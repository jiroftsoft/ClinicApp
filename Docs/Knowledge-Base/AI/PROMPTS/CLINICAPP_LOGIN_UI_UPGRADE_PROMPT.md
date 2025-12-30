# 🔐 ClinicApp – Login Module UI Upgrade Prompt (Enterprise · Healthcare · Mobile-First)

> **Use in Cursor:** Paste this prompt into Cursor and attach the relevant files (AccountController, Login views, layout, CSS/JS).  
> **Goal:** Upgrade the **Login module** to an **enterprise-grade** healthcare UI with a **dedicated layout**, **mobile-first**, **fast**, **formal**, and **user-friendly** design — without breaking architecture/contracts.

---

## 🔒 Context (LOCKED)
- Project: **ClinicApp** (ASP.NET MVC5 + Web API2, .NET Framework), Healthcare domain.
- Contracts under `CONTRACTS/` are already read and enforced. **Do NOT restate them unless violated.**
- Mandatory rules:
  - Run **Preflight** before proposing changes.
  - Views are **PASSIVE** (no business logic in Razor).
  - Entity → ViewModel via **Factory Method** only.
  - Outputs handled via **ServiceResult Enhanced**.
  - Every change includes **tests + verification plan**.
- Reuse existing helpers/layouts/viewmodels; **do NOT recreate** equivalents.

---

## 🎯 Task
Review and optimize the **Login module** end-to-end and bring it to **Enterprise/Healthcare UI** quality:
- Dedicated **Login Layout** (not the main app layout if that adds weight or navigation noise)
- **Mobile-first** responsive design
- **Fast** (minimal CSS/JS, optimized assets, avoid heavy libraries/animations)
- **Formal & healthcare-appropriate** (high readability, neutral palette, no flashy colors)
- **User-friendly** (clear errors, good spacing, predictable forms)
- Maintain SRP and existing architecture patterns.

---

## 📁 Scope (STRICT)
Review ONLY these areas (add/remove paths to match repo):
- Controller:
  - `Controllers/AccountController.cs`
- Views:
  - `Views/Account/Login.cshtml`
  - `Views/Shared/_LoginLayout.cshtml` (create ONLY if not already present)
  - Any existing layout used by login
- ViewModels:
  - `ViewModels/Account/*` (or equivalent)
- Client assets used by login:
  - CSS for login layout (existing first; add minimal new if needed)
  - JS used by login form/validation (existing first)

Do NOT refactor unrelated modules.

---

## ✅ Healthcare UI Requirements (NON-NEGOTIABLE)
1) **Formal admin look + readability**
   - Clear labels, high contrast, consistent spacing
   - Neutral, non-flashy styling
   - No heavy animations; only subtle transitions if needed
2) **Mobile-first**
   - Works perfectly on small screens first
   - Touch-friendly inputs, proper keyboard types (email/number)
3) **Performance**
   - Login page should be lightweight: avoid loading full app nav, dashboards, large bundles
   - Avoid unnecessary JS; prefer simple unobtrusive validation
4) **Validation**
   - Server-side validation is authoritative
   - Client-side validation for UX
   - Error messages: clear, field-level, and safe (no sensitive info)
5) **Security hygiene**
   - Anti-forgery and auth flows consistent with project
   - No leaking of auth state via UI/messages/logs

---

## ⚙️ Required Process (FAST but SYSTEMATIC)

### 1) Preflight
- Confirm scope and risk level (Low/Med/High)
- Identify existing layouts/assets already available for reuse

### 2) Current-State Map (Architecture Snapshot)
- Which layout is used today and why it causes problems (if any)
- What assets (CSS/JS) are loaded
- Which ViewModel drives the view
- Where validation happens (server/client)

### 3) Critical Issues ONLY (max 5)
Report only high-impact issues with evidence:
- Layout/asset bloat hurting performance
- Non-mobile-first layout problems
- SRP violations (logic in view/controller)
- Missing/weak validation UX
- Styling violates healthcare formality/readability
For each: **Evidence (file + section)** + **Impact**.

### 4) Root Cause (Evidence-Based)
For each issue:
- True root cause (not symptom)
- Why it leads to the observed UX/perf problems

### 5) Fix Plan (Minimal & Enterprise)
Propose the smallest safe change set that achieves:
- A dedicated `_LoginLayout` (only if needed)
- Lightweight asset loading (only what login needs)
- Mobile-first layout structure
- Accessible, readable form design
- Correct validation + safe error rendering
Rank alternatives and trade-offs.

### 6) Implementation Details (Diff-Style)
Provide minimal diffs for:
- View/layout changes
- Asset references (bundles or direct includes)
- ViewModel usage (strongly typed, factory-based mapping upstream)
- ServiceResult consumption (if login flow uses it)
Do not add new frameworks unless already in project.

### 7) Tests & Verification
- Controller/service validation tests (unit)
- Basic integration test for login flow (if available)
- Manual verification checklist:
  - Mobile screen
  - Desktop
  - Invalid creds
  - Validation errors
  - Slow network / asset load sanity
- Rollback plan

---

## 📤 REQUIRED OUTPUT FORMAT (STRICT)
1) Preflight Result  
2) Current-State Map  
3) Critical Issues (Evidence + Impact)  
4) Root Cause Analysis  
5) Fix Plan (Ranked)  
6) Implementation Details (Diff snippets)  
7) Validation Plan (Server + Client)  
8) Tests & Verification  
9) Rollback Notes  
10) Open Questions / Missing Info  

---

## 🔎 Input You Must Ask Me ONLY If Missing (keep minimal)
- Exact file paths if different
- Current layout name used by login (if not obvious)
- Whether login must support any special flow (OTP, captcha, external providers)

**Start immediately with what is available; do not stall.**
