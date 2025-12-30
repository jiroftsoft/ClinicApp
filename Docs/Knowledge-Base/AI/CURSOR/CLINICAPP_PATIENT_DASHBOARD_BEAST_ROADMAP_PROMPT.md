# 🧟‍♂️ ClinicApp – Patient Dashboard (API/AJAX) Beast Mode Roadmap Prompt (Production · Healthcare)

> **Use in Cursor:** Paste this prompt into a new Cursor chat **after** pasting:
> 1) `CLINICAPP_CURSOR_MASTER_CONTEXT.md`  
> 2) `CLINICAPP_CURSOR_FLOW_DISCIPLINE_CONTRACT.md`  
> Then attach relevant project files (controllers/services/viewmodels/views/db/models).  
>
> **Goal:** Review existing code, design and implement an **enterprise-grade Patient Dashboard** that is:
> - **API-driven** + **AJAX-first** (no full refresh for sections)
> - **Componentized & reusable** (partials/templates/components)
> - **SRP / architecture compliant**
> - **Flow-safe / production-safe** (scenario-driven + tested)
> - Healthcare UI: formal, readable, fast, no flashy colors, no heavy animations

---

## 🔒 0) Production Lock & Contracts (DO NOT RESTATE)
- This is **PRODUCTION** (zero tolerance for broken flows).
- Contracts and knowledge base are **LOCKED** and already enforced.
- Do **NOT** summarize contracts; mention **only violations**.
- Mandatory patterns:
  - Preflight checklist first
  - ServiceResult Enhanced everywhere
  - Factory Method for Entity → ViewModel
  - Views passive, controllers orchestrate, services contain logic
  - Search-first reuse (no duplicates)
  - Healthcare UI rules (formal, high readability)
  - Flow continuity (no context loss)

---

## 👥 1) Roles (MUST BE ACTIVE)
You must act as:
1) Senior Staff Engineer (Google-grade)
2) System Architect (MVC5 + Web API2)
3) Healthcare UX Guardian (anti-confusion)
4) Security & Privacy Engineer
5) Performance Engineer (anti-bottleneck)
6) QA / Edge-case Engineer (scenario-driven)
7) Production Reliability Engineer (SRE mindset)

---

## 🎯 2) Mission
### Build **Patient Dashboard** for logged-in patient:
- A single “home” for patient activity and records
- Sections load via **AJAX** from **Web API** endpoints
- Only patient’s own data is accessible (authorization boundary)
- Dashboard is **fast** and scales (pagination, caching strategy where safe, no N+1)

### Next module (Phase 2):
- **Electronic Medical Record (EMR) Builder** (patient completes/updates electronic record)
- Must be designed after dashboard foundation is stable

---

## 📁 3) Scope (STRICT – attach exact paths)
Attach only what is needed (adjust to repo):
- Controllers (MVC + API):
  - `Controllers/*Patient*Controller.cs`
  - `Controllers/*Dashboard*Controller.cs`
  - `Controllers/Api/*` (or WebApi controllers)
- Services:
  - `Services/*Patient*`
  - `Services/*Appointment*`
  - `Services/*Visit*`
  - `Services/*Document*`
- ViewModels/Factories:
  - `ViewModels/Patient/*`
  - `Factories/*Patient*`
- Views/Layouts/Partials:
  - `Views/Patient/Dashboard.cshtml`
  - `Views/Shared/*`
- Data layer:
  - Entities/Repos for visits, appointments, documents, EMR tables

Do not refactor unrelated modules.

---

## 🧭 4) Roadmap (Cursor must follow in order)

### PHASE 0 — Discovery & Architecture Fit (Review-first)
**Output:**
- Folder structure & current architecture map (how MVC, WebApi, services, viewmodels are organized)
- Existing reuse scan (what already exists for patient/appointments/visits/documents)
- Current auth boundary for patient data access

**Deliverables:**
1) Module Map
2) Dependency/Impact Graph
3) Reuse Findings (Exists vs Missing)

### PHASE 1 — Flow & Scenario Design (Anti-confusion)
Use `CLINICAPP_SCENARIO_MATRIX_TEMPLATE.md` + `CLINICAPP_FLOW_STATE_MACHINE_TEMPLATE.md`:

**Must define:**
- Primary dashboard entry flow
- Branches: not logged in → login/register → return to dashboard
- Section loading branches: API error, empty state, slow network, pagination
- Security branches: trying to access another patient’s data, tampered ids

**Deliverables:**
1) Primary Flow Identification  
2) Scenario Matrix (ALL branches)  
3) Flow Break Risks

### PHASE 2 — Minimal Vertical Slice (Build-first, small & safe)
**Build one vertical slice end-to-end:**
- MVC Dashboard page skeleton (layout + placeholders)
- One section via AJAX (e.g., “Recent Appointments”)
- Web API endpoint returns **ServiceResult Enhanced**
- View consumes via AJAX and renders partial/component
- Pagination (if needed) and error UI state

**Deliverables:**
- Working dashboard skeleton
- 1 fully working section (API + UI)
- Tests for auth boundary + service logic

### PHASE 3 — Expand Sections (Componentized & Reusable)
Iterate section-by-section with **reusable components**:
Suggested sections (adjust to project reality):
1) Recent Appointments / Visits
2) Visit summaries / notes (as allowed)
3) Uploaded documents / reports
4) Prescriptions / labs (if supported)
5) Invoices / payments (if applicable)
6) Notifications/messages (if present)

**Rules:**
- Each section = its own service method + api endpoint + view component
- Avoid repeated markup: use partials/templates/components
- Keep payloads minimal, paginate lists

### PHASE 4 — Hardening (Performance, Security, Production)
- Identify and fix N+1 queries / chatty calls
- Enforce authorization everywhere (patient can only see own data)
- Add audit logs where needed (no PII leakage)
- Timeouts, retries, and clear error messages
- Production readiness checklist: `CLINICAPP_PRODUCTION_READINESS_CHECKLIST.md`

### PHASE 5 — Phase 2 Preparation: EMR Builder (Design-only now)
After dashboard stable:
- Define EMR domain model (forms/sections)
- Define state machine for multi-step EMR completion
- Define validation strategy (server authoritative + client UX)
- Plan versioning/migrations carefully

---

## 🧩 5) Component Model (Required)
Dashboard must be **componentized**:

- MVC page: `Dashboard.cshtml` (shell only)
- Components:
  - `_DashboardRecentAppointments.cshtml`
  - `_DashboardDocuments.cshtml`
  - etc.
- Each component loads via AJAX from a dedicated API endpoint:
  - `GET /api/patient/dashboard/recent-appointments?page=1`
- Each API endpoint returns `ServiceResult Enhanced` with:
  - data
  - pagination metadata
  - error codes and safe messages

No heavy JS frameworks unless already standard in project.

---

## 🔐 6) Security & Data Ownership (Critical)
Cursor must verify:
- Patient identity from auth context, not from query params
- Any `patientId` in URL must be ignored or validated against current user
- No PII leakage in errors/logs
- No-cache on sensitive dashboard pages

---

## ⚙️ 7) Required Systematic Process (Beast Mode)
Cursor MUST do:

1) Preflight Result  
2) Reuse Scan (Exists vs Missing)  
3) Module Map + Dependency Graph  
4) Critical Issues (max 7) with evidence  
5) Root Cause Analysis  
6) Roadmap (phased, ranked)  
7) Implementation Diffs (vertical slice first)  
8) ServiceResult examples  
9) Tests (unit + integration if possible)  
10) Verification Steps (scenario-based)  
11) Rollback Strategy  
12) Open Questions (blocking only)

**No extra text. No theory dumps.**

---

## ✅ 8) Acceptance Criteria (Phase 1–4)
- Dashboard loads for logged-in patient
- Sections load via AJAX from Web API endpoints
- Each section has loading + empty + error states
- Patient sees ONLY their own data
- No N+1; payloads paginated
- SRP enforced (view passive, controller orchestration, services logic)
- ServiceResult Enhanced end-to-end
- Factory Method mapping in place
- Tests + verification + rollback included
- Production readiness checklist passes

---

## 🔜 9) Next Step After Dashboard
Once Phase 1–4 are stable, proceed to:
**Electronic Medical Record (EMR) Builder**
- Multi-step, resumable, validated, audit-ready
- Built on same patterns (API/AJAX/components)
- Scenario-driven + state machine-based

---

**END – READY FOR CURSOR EXECUTION**
