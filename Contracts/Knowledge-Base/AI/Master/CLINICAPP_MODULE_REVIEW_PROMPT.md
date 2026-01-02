# 🧩 ClinicApp – Ultra-Lean Module Review Prompt (Healthcare)

> **Purpose:**  
> Fast, systematic review of ONE module in **ClinicApp** (ASP.NET MVC5 + Web API2)  
> Focused on **critical issues only**, real **root cause**, and **safe, architecture-aligned fixes**.  
> Optimized for daily use by engineering teams (Google-style execution).

---

## 🔒 Assumptions (Locked)
- All contracts under `CONTRACTS/` are already read and enforced.
- Preflight checklist is mandatory.
- Entity → ViewModel via **Factory Method only**.
- All outputs via **ServiceResult Enhanced**.
- Every change must include tests.

_Do not restate contracts unless violated._

---

## 🎯 TASK INPUT
- **Module name:** `<MODULE_NAME>`
- **Scope / Files:** `<FOLDERS or FILE PATHS>`
- **Primary responsibility:** `<WHAT THIS MODULE SHOULD DO>`
- **Current issue (optional):** `<BUG / ERROR / WRONG BEHAVIOR>`

---

## ⚙️ PROCESS (FAST & STRICT)

### 1) Preflight
- Scope confirmed
- Risk level: **Critical / High / Medium / Low**

---

### 2) Module Snapshot
Identify only what matters:
- Entry points (MVC / API)
- Services
- Helpers / Factories
- ViewModels / DTOs
- DB / external touchpoints

---

### 3) Critical Issues ONLY (Max 3–5)
Identify **only high-impact problems**, such as:
- Architecture boundary violations
- Security or data-correctness risks
- Performance bottlenecks
- High coupling / duplication
- SRP violations

For each issue provide:
- **Evidence:** file + method + behavior
- **Impact:** why it matters

---

### 4) Root Cause Analysis
For each issue:
- True root cause (not symptom)
- Why it produces the observed behavior
- Why other causes are unlikely

---

### 5) Fix Plan (Minimal & Safe)
- Smallest possible change
- Reuse existing classes/utilities (do not recreate)
- No breaking changes
- Rank alternatives if needed

---

### 6) Implementation Details
- Exact files/classes/methods to change
- Minimal diff-style snippets
- Factory Method usage (if mapping)
- ServiceResult Enhanced usage

---

### 7) Tests & Verification
- Tests to add/update (unit/integration)
- Manual verification steps
- Regression scenarios covered

---

### 8) Rollback
- Safe rollback steps
- Guards / flags if risk is Medium or High

---

## 📤 REQUIRED OUTPUT FORMAT
1) Preflight Result  
2) Module Snapshot  
3) Critical Issues (Evidence)  
4) Root Cause  
5) Fix Plan  
6) Implementation Details  
7) Tests & Verification  
8) Rollback  
9) Open Questions (if any)

---

**Owner:** ClinicApp Engineering  
**Category:** Module Review  
**Style:** Ultra-Lean · Execution-First  
