# 🧟‍♂️ ClinicApp – Beast Mode Module Review & Build Prompt (FINAL)
### Enterprise · Healthcare · Cursor-Optimized · Contract-Locked

> **Inspired by:** smooth professional dashboards like *Maktabkhooneh*  
> **Adapted for:** Healthcare / Clinic environment (formal, secure, audit-ready)  
> **Use case:** Give this directly to **Cursor** to review, complete, or build modules such as  
> **User Profile** and **Patient Dashboard** with maximum AI performance and minimum noise.

---

## 🔒 0) Contract Lock (ONE-TIME, NON-NEGOTIABLE)

### Assumed (DO NOT RESTATE)
Cursor already fully understands and enforces:
- All contracts under `CONTRACTS/`
- Project knowledge base and decisions from this repository
- Mandatory patterns:
  - ServiceResult Enhanced
  - Factory Method (Entity → ViewModel)
  - Preflight checklist
  - SRP & clean architecture
  - Healthcare UI rules (formal, readable, fast)
  - AJAX-first where no full refresh is needed

⚠️ **Do NOT re-summarize contracts.**  
Mention a contract **ONLY if violated**.

### Hard Rules
1. Preflight checklist is mandatory
2. No duplicate code — **search first, reuse always**
3. No guessing — all findings must have evidence
4. Every change must include:
   - Tests
   - Verification steps
   - Rollback plan

---

## 🧠 1) Beast Mode Execution Policy
- Prefer **search + evidence + diffs** over explanation
- Work in iterations:
  **Search → Map → Top 3–7 issues → Fix → Tests**
- Minimal narration, maximum execution
- Do not pause for questions unless truly blocking

---

## 🎯 2) Task Definition (YOU FILL THIS)

### 2.1 Mode
Choose ONE:
- **MODE A – REVIEW + FIX** (exists, find critical issues, fix)
- **MODE B – COMPLETE** (partially exists, complete missing parts)
- **MODE C – BUILD** (missing, build minimal vertical slice first)

### 2.2 Target Modules
- Module 1: `User Profile`
- Module 2: `Patient Dashboard`

### 2.3 Scope (STRICT – Attach Only These)
- Controllers: `<paths>`
- Services: `<paths>`
- ViewModels / Factories: `<paths>`
- Views / Layouts / Partials: `<paths>`
- DB / Entities / Repos: `<paths>`
- Helpers / Filters / Extensions: `<paths>`

❌ Do NOT scan unrelated modules.

---

## 🏥 3) Business Goals (Healthcare-Adapted, Inspired by Maktabkhooneh)

### User Profile
- Patient views and edits **own information only**
- Allowed fields only (name, contact, avatar, preferences)
- Secure validation + audit logging
- Clean, mobile-first, formal UI

### Patient Dashboard
Patient can clearly and safely see:
- Visits & appointments history
- Electronic medical record summary
- Uploaded documents / reports
- Prescriptions & lab results (if supported)
- Invoices / payments (if applicable)
- Notifications / messages (if present)

⚠️ Patient must **ONLY see their own data**.

---

## 🎨 4) UI / UX Constraints (Healthcare Standard)
- Formal, administrative, high readability
- No flashy colors, no heavy animations
- Mobile-first, fast load
- Dashboard sections load via **AJAX/partials** if heavy
- Clear empty/error states
- No-cache on sensitive pages

---

## ⚙️ 5) Required Systematic Process (DO NOT SKIP)

### STEP 0 — Preflight
Output:
- Scope confirmed
- Risk: Critical / High / Medium / Low
- Existing test framework (what/where)

### STEP 1 — Search-First Reuse Scan
Search codebase for:
- Existing profile/dashboard logic
- Existing ViewModels, factories, partials
- Existing ServiceResult patterns
- Existing audit/security models

Output:
- **Reuse (exists):** exact files/classes
- **Missing:** what truly does not exist

### STEP 2 — Module Map
Map end-to-end flow:
Request → Filters → Controller → Service → DB → ServiceResult → View/AJAX → UI

Include:
- Direct dependencies
- Indirect dependencies
- Impacted modules

### STEP 3 — Critical Findings (MAX 3–7)
Only high-impact issues:
- Security & auth boundaries
- Data correctness / privacy
- SRP or architecture violations
- Performance bottlenecks (N+1, heavy layout)
- UX blockers for healthcare users

For each:
- Evidence (file + section)
- Impact (Security / Safety / Perf / UX / Maintainability)

### STEP 4 — Root Cause Analysis
For each finding:
- True root cause (not symptom)
- Why it causes the issue
- Why other causes are unlikely

### STEP 5 — Fix / Build Plan (Ranked)
- Minimal safe fix first
- Vertical slice first if building
- Reuse existing code
- Factory Method + ServiceResult enforced
- Incremental, backward compatible

### STEP 6 — Implementation (Patch-Ready)
- Exact file list
- Minimal diffs per file
- AJAX endpoints/partials if needed
- No new frameworks unless already used

### STEP 7 — Tests & Verification
- Unit tests (services, factories, auth boundaries)
- Integration tests (user sees ONLY own data)
- Manual verification steps (happy + error + security paths)

### STEP 8 — Rollback & Safety
- Rollback steps
- Feature flag/config toggle if risk ≥ Medium

---

## 📤 6) REQUIRED OUTPUT FORMAT (STRICT)
1. Preflight Result  
2. Reuse Scan Results  
3. Module Map + Dependency Graph  
4. Critical Findings (max 7)  
5. Root Cause Analysis  
6. Fix / Build Plan (ranked)  
7. Implementation Diffs  
8. ServiceResult Examples  
9. Tests  
10. Verification Steps  
11. Rollback Strategy  
12. Open Questions (blocking only)

---

## ✅ 7) Acceptance Criteria

### User Profile
- User edits ONLY own data
- CSRF protection on POST
- No-cache headers applied
- Server validation authoritative
- Factory Method mapping
- ServiceResult Enhanced everywhere
- Tests added

### Patient Dashboard
- Patient sees ONLY own activities
- Data is paginated & performant
- Heavy sections via AJAX
- No PII leakage
- Tests + verify + rollback present

---

## 🧱 8) Vertical Slice Rule (MODE B/C)
If module is missing or incomplete:
1. Build ONE vertical slice first (e.g., dashboard + recent visits)
2. Then iterate section by section

---

**END – READY FOR CURSOR EXECUTION**
