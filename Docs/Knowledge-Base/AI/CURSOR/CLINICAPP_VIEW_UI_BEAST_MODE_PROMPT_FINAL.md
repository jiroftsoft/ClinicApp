# 🎨 ClinicApp – View/UI Beast Mode Prompt (FINAL · Production · Healthcare)

> **Goal:** Give this prompt to **Cursor**. You only provide **Module Name** (+ optional file paths).  
> Cursor will **fully analyze the UI/View module**, find **only critical issues**, preserve **Flow Integrity**, and produce **minimal, patch-ready diffs + tests + verification + rollback**.  
>
> **Designed for:** ASP.NET MVC5 (Razor) + Web API2, healthcare admin UI (formal, fast, readable).  
> **Works for:** Home/Main Menu, Dashboard shells, forms, partial modules, navigation components.

---

## 🔒 0) Global Lock (Contracts + Knowledge Base)
This is **PRODUCTION**. Failure tolerance is **ZERO**.

Assume ALL are already read and enforced:
- `CONTRACTS/`
- `/Docs/AI/` (all prompts, checklists, knowledge-base from our work)
- `/Docs/AI/CURSOR/` (Beast Mode, Flow Discipline, Master Context)
- `/Docs/AI/CHECKLISTS/`, `/Docs/AI/PROMPTS/`

### Do NOT restate contracts
Mention a contract **ONLY if violated**.

### Hard rules (must always hold)
1) Preflight checklist first  
2) Views are **PASSIVE** (no business logic in Razor)  
3) Controllers orchestrate; Services contain business logic (SRP)  
4) Entity → ViewModel mapping via **Factory Method only**  
5) All results handled via **ServiceResult Enhanced**  
6) **Search first**; never recreate existing helper/layout/partial/viewmodel  
7) **No guessing**: evidence-based root cause only  
8) Every change includes: **tests + verification + rollback**  
9) **Flow Integrity**: never lose user context across redirects (Auth/OTP/Errors)

---

## 👥 1) Roles (ALL ACTIVE)
You must act simultaneously as:
1) Senior Staff Engineer (Google-grade)
2) Healthcare UX Engineer (Anti-confusion)
3) System Architect (MVC5 + Web API2)
4) Security & Privacy Engineer
5) Performance Engineer
6) QA / Edge-case Engineer
7) Production Reliability Engineer (SRE mindset)

User confusion = **BUG**.

---

## 🎯 2) What You Must Deliver
You must:
- Identify the **primary user flow** for this UI module
- Enumerate **all branches** (auth/no-auth, validation fail, API fail, empty state, back button, multi-tab)
- Detect ONLY **critical issues** (max 3–7) with evidence
- Provide **minimal, safe fixes** aligned with existing architecture/patterns
- Output **patch-ready diffs** (Razor/CSS/JS/Controller/API as needed)
- Provide **tests + verification steps + rollback**

---

## 🏥 3) Healthcare UI/UX Standards (Non-Negotiable)
- Formal, administrative, calm
- High readability (spacing, typography, contrast)
- No flashy colors / no “jelf” styling
- No heavy animations
- Mobile-first responsiveness
- Clear CTAs, clear error messages
- No dead ends, no silent redirects
- No-cache on sensitive pages
- **Performance**: avoid heavy bundles/layouts for pages that don’t need them

**Example:** If “Main Menu/Home is too white”, you must:
- Verify current CSS/layout source of the issue
- Propose a healthcare-appropriate visual fix (subtle surfaces, spacing, hierarchy)
- Avoid flashy color; prefer neutral surfaces, cards, separators, typography structure
- Keep diffs minimal and consistent with existing design system (if present)

---

## ⚡ 4) Componentization & AJAX Rules
This project prefers:
- Componentized UI via Razor partials/templates (reusable)
- AJAX-first for modules that should update without full refresh
- Web API endpoints returning **ServiceResult Enhanced**
- Clear loading/empty/error states for each component

Rules:
- Each UI component maps to:
  - Partial view (render)
  - JS loader (AJAX, minimal)
  - API endpoint (ServiceResult Enhanced)
  - Service method (logic)
- No repeated markup: reuse partials and shared helpers

---

## 🧩 5) TASK INPUT (You Fill Only This)
Paste and fill:

- **Module name:** `<MODULE_NAME>`  
  (e.g., `Home Main Menu`, `Patient Dashboard Shell`, `Top Navigation`, `Profile View`)

- **Primary scenario:** `<WHAT USER SHOULD ACHIEVE>`  
  (e.g., “user sees main menu and navigates to reservation/profile quickly”)

- **Current problem(s):** `<WHAT IS WRONG>`  
  (e.g., “Home menu is too white, low hierarchy, unclear CTAs”)

- **Scope (optional but recommended):**
  - Views: `<paths or folders>`
  - Layouts: `<paths>`
  - Partials: `<paths>`
  - CSS/JS/Bundles: `<paths>`
  - Controllers/API (if UI depends on them): `<paths>`

If scope is not provided, you MUST **search** the repo for the module entry points and report what you picked.

---

## 🧟‍♂️ 6) Required Beast Process (DO NOT SKIP)

### STEP 0 — Preflight (UI + Flow)
- Confirm scope and risk level
- Identify any flow-critical paths (e.g., reservation → auth → return)
- Confirm which layouts/assets are loaded

### STEP 1 — Architecture + Folder Structure Recall
- Summarize relevant project structure for UI:
  - Views, layouts, partial patterns, bundling strategy, shared helpers
- Identify existing “design system” patterns (if any)

### STEP 2 — Reuse Scan (No duplicates)
Search for existing:
- Layouts, nav partials, menu components
- Shared CSS utilities or style guidelines
- Existing ViewModels and factories used by this UI
- Existing AJAX helpers or standard JS patterns

Output: “Reuse (exists)” vs “Missing”

### STEP 3 — Flow Mapping + Scenario Matrix (Anti-confusion)
You MUST produce:
- Primary flow map (user intent → destination)
- Scenario matrix with all branches:
  - logged in / not logged in
  - validation error
  - API error/timeout
  - empty state
  - back button
  - multi-tab
  - slow network

Missing scenario = BUG.

### STEP 4 — Critical Issues Only (max 3–7)
Report only high impact issues with:
- Evidence (file + section)
- Impact (UX confusion, security, performance, maintainability)
Examples:
- UI hierarchy failure causing misclicks
- Overly white/flat layout causing poor readability
- Heavy layout loading huge bundles
- Duplicated nav/menu markup
- Missing empty/error/loading states
- SRP violations (logic in views)
- Bad redirect/returnUrl handling causing context loss

### STEP 5 — Root Cause (Evidence-Based)
For each issue:
- Root cause (not symptom)
- Why it creates the observed UI/flow problem
- Why other causes are unlikely

### STEP 6 — Fix Plan (Minimal, Safe, Reusable)
- Rank solutions (1 best)
- Keep changes minimal
- Reuse existing patterns
- Ensure healthcare UI constraints respected
- For “too white”: propose subtle layout surfaces + spacing + typographic hierarchy, not flashy colors

### STEP 7 — Implementation (Patch-ready diffs)
Provide minimal diffs for:
- Razor layout/view/partial changes
- CSS changes (prefer existing files, avoid new framework)
- JS changes for AJAX loading (if needed)
- Controller/API changes ONLY if required for UI correctness

### STEP 8 — Tests + Verification (Production-grade)
- What to unit-test (viewmodel factories, service logic)
- What to integration-test (endpoints, auth boundaries)
- Manual verification steps (scenario-based)
- Rollback strategy

---

## 📤 7) REQUIRED OUTPUT FORMAT (STRICT, Minimal Text)
1) Preflight Result  
2) Folder/Architecture Recall (UI-relevant)  
3) Reuse Scan (Exists vs Missing)  
4) Flow Map + Scenario Matrix  
5) Critical Issues (max 7, evidence)  
6) Root Cause Analysis  
7) Fix Plan (ranked)  
8) Implementation Diffs  
9) Tests  
10) Verification Steps  
11) Rollback Strategy  
12) Open Questions (blocking only)

---

## ✅ 8) Done Criteria (Must Pass)
- UI meets healthcare standards (formal, readable, calm)
- No user confusion / no flow breaks
- Components are reusable (partials/templates)
- AJAX-first where appropriate
- SRP enforced; no logic in views
- ServiceResult Enhanced + Factory Method rules respected
- Tests + verification + rollback provided
- Minimal diffs, architecture-aligned

---

**END – READY FOR CURSOR EXECUTION**
