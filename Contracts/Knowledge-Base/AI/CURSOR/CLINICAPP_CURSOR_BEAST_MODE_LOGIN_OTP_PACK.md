# 🧟‍♂️ ClinicApp – Cursor Beast Mode Pack (Login/OTP) 🔥

> **Purpose:**  
> Maximize practical output from Cursor AI: minimum narration, maximum search/analysis/diffs/tests.  
> Use this pack for **Login + Registration via OTP** module upgrades and audits.

---

## ✅ How to Use (Fast)
1) Ensure your **Cursor Rules** include the Beast Mode rules (below).  
2) Run **PASS 1 (Discovery)** first.  
3) Then run **PASS 2 (Patch)** to generate diffs + tests.  
4) Attach the files listed in **Scope** to Cursor for best results.

---

## 0) Cursor Rules – Beast Mode (Paste into Cursor Rules / Project Rules)
```text
BEAST MODE (ClinicApp):
- Contracts are LOCKED; never restate them unless violated.
- Prefer search + evidence + diffs over explanation.
- Work in iterations: Search → Map → Top 3–5 issues → Fix → Tests.
- Do not stop for questions unless truly blocking.
- Reuse existing code; never create duplicates.
- Views are PASSIVE; Entity→ViewModel via Factory only.
- Outputs must use ServiceResult Enhanced.
- Keep text minimal; maximize actionable output.
```

---

## 1) Target Scope (Login + Registration via OTP)
Attach at least these (adjust paths to your repo):
- `Controllers/AccountController.cs`
- `Views/Account/Login.cshtml`
- `Views/Account/Register.cshtml` (if OTP registration is here)
- `Views/Shared/_LoginLayout.cshtml` (or login layout currently used)
- `Services/AuthService.cs` (or equivalent)
- OTP models/state: `Models/**/Otp*.cs`, `Helpers/**Otp*`, `Services/**Otp*`
- Any rate limit / client provider: `Helpers/ClientProvider.cs`, rate-limit helpers
- Any audit/security model: `SecurityAuditEntry` and related persistence layer

---

## 2) PASS 1 – Discovery (Fast, Deep, Evidence-Based)
> **Goal:** Build dependency map + find top 3–5 critical issues with evidence. No diffs yet.

```text
MODE: BEAST – DISCOVERY (no code changes yet)

Context:
ClinicApp (ASP.NET MVC5 + Web API2, Healthcare). Contracts are LOCKED and enforced.
Do NOT restate contracts unless violated.

Task:
Deeply review Login + OTP module. Use search aggressively to find all related code.
Output ONLY: dependency map + top critical issues + root causes + missing info.

Scope:
- Module: Login + Registration via OTP
- Files attached: (the ones I provided)

Execution Steps (mandatory):
1) Search-first: locate all OTP/auth/audit/rate-limit/anti-forgery usage.
2) Build a dependency & impact map:
   - Depends on: ...
   - Used by: ...
3) Identify ONLY top 3–5 critical issues with evidence:
   - Security holes, audit gaps, rate-limit gaps, session binding gaps,
     SRP violations, heavy layout/assets, validation inconsistencies.
4) Root cause for each issue (evidence-based, no guessing).
5) List ONLY blocking missing info (if any).

Output (STRICT, no extra text):
1) Dependency/Impact Map
2) Top Issues (max 5) + Evidence (file + method)
3) Root Causes
4) Quick Fix Directions (no diffs yet)
5) Missing/Blocking Info
```

---

## 3) PASS 2 – Patch (Diffs + Tests, Minimal & Safe)
> **Goal:** Produce minimal diffs + tests + verification + rollback. Reuse existing utilities.

```text
MODE: BEAST – PATCH (produce diffs + tests)

Context:
Contracts are LOCKED and enforced. Do NOT restate contracts unless violated.

Input:
Use the issues/root causes from PASS 1.

Task:
Implement the minimal safe fixes to reach enterprise-grade OTP login/registration quality.
- Do NOT create duplicates: search and reuse existing code first.
- Keep changes incremental and backward compatible.

Mandatory Requirements:
- Views passive; Entity→ViewModel via Factory method.
- Use ServiceResult Enhanced for responses (success, validation, rate-limit, lockout, provider failure).
- Add/update tests for each fix.
- Provide verification and rollback steps.

Output (STRICT, no extra text):
1) Patch Plan (ordered)
2) Diffs (code blocks per file)
3) Tests (new/updated) + rationale
4) Verification Steps (manual + automated)
5) Rollback Plan
```

---

## 4) Optional – Enterprise Audit Add-on (Login History / IP / User-Agent)
> Use this add-on when you specifically want “professional-grade login history” like large teams.

```text
ADD-ON: ENTERPRISE LOGIN AUDIT

Check whether the system persists a usable audit trail for:
- login success/failure
- OTP send/verify success/failure
- IP address, User-Agent, correlation id
- lockout/rate-limit events

Output:
- Current status: Exists / Partial / Missing (with evidence)
- Minimal DB/schema/model changes if needed
- Minimal service/controller integration
- Tests + verification
```

---

## 5) Tip for Maximum Performance
- Attach the exact files in scope.
- Provide one real example: stack trace OR “steps to reproduce” OR “current bad UX behavior”.
- Keep the request short; let the Beast Mode prompts do the work.

---
**Owner:** ClinicApp Engineering  
**Category:** Cursor / AI Productivity  
**Pack:** Beast Mode (Discovery + Patch)  
