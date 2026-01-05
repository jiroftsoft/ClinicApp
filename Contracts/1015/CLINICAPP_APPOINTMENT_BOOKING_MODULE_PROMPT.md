# 📅 ClinicApp – Appointment / AppointmentBooking Module Prompt (ULTIMATE · Production · Evidence-First)

> **Use in Cursor:** Paste this prompt in a new Cursor chat.  
> سپس فایل‌های مرتبط ماژول AppointmentBooking (MVC + API + Services + Views + DB) را attach کن.  
> **هدف:** بررسی عمیق، شناسایی ایرادهای حیاتی، ریشه‌یابی واقعی، و ارائه اصلاحات امن/حداقلی با تست و برنامهٔ استقرار.

---

## 🔒 0) Contract & Knowledge Base Lock (READ BY REFERENCE)
Cursor MUST read & obey (do **NOT** restate; mention only violations):
- `CONTRACTS/` (ALL)
- `/Docs/AI/**` and `/Docs/AI/CURSOR/**` (all prompts/checklists/contracts we built)
- Key references (by path):
  - `/Docs/AI/CURSOR/CLINICAPP_CURSOR_MASTER_CONTEXT.md`
  - `/Docs/AI/CURSOR/CLINICAPP_CURSOR_FLOW_DISCIPLINE_CONTRACT.md`
  - `/Docs/AI/CURSOR/CLINICAPP_SAFE_MODULE_FIX_PROMPT.md`
  - `/Docs/AI/QUALITY/CLINICAPP_SCENARIO_MATRIX_TEMPLATE.md`
  - `/Docs/AI/ARCH/CLINICAPP_FLOW_STATE_MACHINE_TEMPLATE.md`
  - `/Docs/AI/RELEASE/CLINICAPP_PRODUCTION_READINESS_CHECKLIST.md`

**Production healthcare system. No guessing. No damage.**

---

## 👥 1) Roles (ALL ACTIVE)
You must act simultaneously as:
1) Senior Staff Engineer (Enterprise / Google-grade)
2) System Architect (ASP.NET MVC5 + Web API2 + OWIN/Cookie Auth)
3) Debugging Specialist (evidence-first root cause)
4) Security & Privacy Engineer (healthcare-grade)
5) UX Flow Guardian (anti-confusion, flow continuity)
6) Performance Engineer (N+1 / chatty DB / latency)
7) QA / Edge-case Engineer (scenario-driven)
8) Production Gatekeeper (Go/No-Go, rollback-ready)

---

## 🎯 2) Mission (AppointmentBooking)
### Primary user story (typical)
Patient selects:
- doctor/service/clinic → date → time slot → confirms booking → receives confirmation

### System guarantees (MUST hold)
- **No double booking** (concurrency-safe)
- **No broken flow** (auth interruptions return to intended step)
- **Clear states** (loading/empty/error) especially if AJAX/API-driven
- **Data ownership** (patient can only view/modify own bookings)
- **Auditability** (who booked/changed/cancelled and when)
- **Production safety** (idempotency, retries, rollback)

---

## 📥 3) Input (User fills only this)
- Module: `Appointment` / `AppointmentBooking`
- Current problem(s): `<1–5 lines>` (optional)
- Expected behavior: `<1–3 lines>` (optional)
- Entry points (optional): `<paths/folders>`

If user provides nothing else, you must discover entry points by searching repo.

---

## 🧠 4) Absolute Project Rules (Non-Negotiable)
- **Preflight first** (scope, risk, deps, tests status)
- **Root cause before fix** (no blind changes)
- **Search-first reuse** (never recreate existing helpers/classes/layouts)
- **SRP enforced**:
  - Views passive (no business logic)
  - Controllers orchestrate
  - Services contain business logic
- **Entity → ViewModel via Factory Method only**
- **ServiceResult Enhanced** everywhere (no raw objects)
- **Flow integrity**: preserve context across Auth/OTP/errors (returnUrl/flowState)
- Every change MUST include:
  - tests
  - verification steps
  - rollback plan

---

## 🧭 5) Scenario Matrix (MANDATORY – Missing scenario = BUG)
Use `CLINICAPP_SCENARIO_MATRIX_TEMPLATE.md`. You MUST cover at least:

### S1) Happy path
- choose doctor/service → pick slot → confirm → success page + booking visible in profile

### S2) Auth interruption
- start booking while logged out → login/register/OTP → return to exact step with context preserved

### S3) Concurrency / double booking
- two users try same slot concurrently → only one succeeds; other gets clear message and alternatives

### S4) Validation and error recovery
- invalid input / required fields missing → errors shown; data preserved
- API timeout / server error → friendly retry; no duplicate booking

### S5) Idempotency / double submit
- user double taps confirm / network retry → only one booking created

### S6) Cancellation/reschedule (if exists)
- patient cancels/reschedules → state consistent; audit updated

### S7) Edge cases
- back/refresh/multi-tab
- session expiry mid-flow
- slot becomes unavailable between selection and confirm
- timezone/date parsing (Persian date picker / hidden input parse) if used

---

## 🧩 6) What you MUST map (Deep Module Map)
Build a full module map with file evidence:
- MVC routes/controllers/actions
- Web API controllers/endpoints (if any)
- Filters (Authorize, AntiForgery, custom)
- Services & interfaces
- Repositories/DbContext/queries
- Entities (Appointment, Slot, Schedule, Patient, Doctor, Clinic)
- ViewModels + Factories
- Views/Partials + JS/CSS + Bundles
- External integrations (SMS/Email/Payment) if used

Also produce:
- Dependency/Impact graph (blast radius): who depends on booking module + what booking depends on

---

## 🔥 7) Critical Issues Only (MAX 7)
Report ONLY issues that can:
- cause wrong bookings / double booking
- break auth gating or leak patient data
- break flow and confuse users
- cause production incidents (timeouts, N+1, deadlocks)
- violate project contracts (ServiceResult, Factory, SRP, etc.)

For each issue include:
- Evidence: file + method + relevant snippet/condition
- Risk: Critical/High
- Why it matters in production

---

## 🧠 8) Root Cause Analysis (Evidence-Based)
For each critical issue:
- true root cause (not symptom)
- why it produces the observed behavior
- why other hypotheses are unlikely

No “maybe” fixes.

---

## 🛠️ 9) Fix Plan (Ranked, Minimal, Safe)
Provide ranked solutions:
1) Best minimal safe change
2) Alternative if constraints exist
3) Tradeoffs

Must ensure:
- concurrency safety (transaction/locking strategy appropriate to existing data layer)
- idempotency (token/request key or server-side guard if already in project patterns)
- consistent auth handling (MVC + API share identity)
- flow resume correctness (returnUrl/flowState)
- reuse existing helpers/patterns

---

## 🧾 10) Implementation (Patch-Ready Diffs)
Provide minimal diffs per file:
- Controllers/API endpoints
- Service methods
- Repo/query changes
- ViewModel factories
- Views/JS (AJAX) if required for UX correctness

Constraints:
- Do NOT introduce new frameworks
- Do NOT duplicate existing utilities
- Keep changes small and reviewable

---

## 🧪 11) Tests, Verification, Rollback (Mandatory)
### Tests (minimum)
- Unit tests:
  - booking service logic (slot availability, validation, idempotency)
  - factory mapping
- Integration tests (if infra exists):
  - authenticated booking success
  - double booking prevention
  - unauthorized access denied
- If no test infra: propose minimal test harness plan consistent with repo

### Verification steps (scenario-based)
- Steps for S1–S7 (short, checklist style)

### Rollback
- Revert commits and DB migration plan (if any)
- Feature-flag/guard suggestion only if risk is high

---

## 📤 12) REQUIRED OUTPUT FORMAT (STRICT)
1) Preflight Result (scope + risk)  
2) Module Map + Dependency/Impact Graph  
3) Scenario Matrix Coverage (S1–S7)  
4) Critical Issues (max 7, evidence)  
5) Root Cause Analysis  
6) Fix Plan (ranked)  
7) Implementation Diffs (per file)  
8) Tests  
9) Verification Steps  
10) Rollback Strategy  
11) Open Questions / Missing Info (blocking only)

---

## ✅ 13) Acceptance Criteria (Must Pass)
- Booking works end-to-end (S1)
- Auth interruption resumes correctly (S2)
- No double booking (S3)
- No duplicate booking on retry/double-submit (S5)
- Patient data isolation holds (security)
- ServiceResult Enhanced + Factory Method rules respected
- Tests + verification + rollback included

---

**END – EXECUTE APPOINTMENTBOOKING REVIEW & FIX (NO GUESSING)**
