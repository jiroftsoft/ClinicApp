# 👤 ClinicApp – User Profile Module Implementation Prompt (Enterprise · Contract-Locked)

> **Use in Cursor:** Paste this prompt and attach the relevant files (Account/User controllers, services, viewmodels, layouts, existing helpers).  
> **Goal:** Implement **User Profile** module so a logged-in user can **view + edit their own profile** safely, maintainably, and in compliance with ClinicApp contracts.

---

## 🔒 Context (LOCKED)
Contracts are already read and enforced. **Do NOT restate them unless violated.**  
Hard rules:
- Preflight checklist before changes.
- Entity → ViewModel via **Factory Method only**.
- All outputs via **ServiceResult Enhanced** (no raw objects / no bool returns) fileciteturn16file6L17-L31.
- Security is higher than speed: CSRF protection, input validation, sensitive data masking, and no caching for sensitive pages fileciteturn16file6L72-L114.
- Reuse existing code/utilities; **do not create duplicates**.

---

## 🎯 Business Goal
A logged-in user can:
1) Open **Profile** page (view own info)
2) Edit allowed fields (e.g., FullName, Email, Mobile, Address, Avatar if supported)
3) Save changes safely (validation server+client, anti-forgery, friendly errors)
4) See a success notification and updated profile
5) System logs the operation without leaking sensitive info

**Out of scope unless explicitly requested:** Changing password, changing phone number OTP verification, admin editing other users.

---

## 📁 Scope (STRICT)
Attach and review ONLY what’s needed (adjust paths to repo):
- Controllers:
  - `Controllers/AccountController.cs` (or `UserController.cs`)
- Services:
  - `Services/*User*` / `Services/*Account*` (existing)
- ViewModels:
  - `ViewModels/Account/*` or `ViewModels/User/*`
- Views:
  - `Views/Account/Profile.cshtml` (new or existing)
  - `Views/Shared/_Layout.cshtml` (only if needed for navigation)
- Helpers/Filters:
  - Anti-forgery filter pattern (e.g., `ValidateAntiForgeryTokenOnPostsAttribute`) and NoCache usage
- Data layer:
  - User entity/repo used to persist profile fields

Do NOT refactor unrelated modules.

---

## ✅ Non‑Negotiable Security & UX Requirements
- **Authorization:** only authenticated users can access (no leakage across users)
- **Anti‑Forgery:** `@Html.AntiForgeryToken()` + `[ValidateAntiForgeryToken]` on POST fileciteturn16file6L90-L95
- **No‑Cache for profile pages** (medical/admin environment) fileciteturn16file6L110-L114
- **Validation:**
  - Server-side authoritative
  - Client-side validation for UX
- **Sensitive data:** never log raw national code, phone, tokens; mask if needed fileciteturn16file6L104-L109
- **Healthcare UI:** formal/admin, high readability, no flashy colors, no heavy animation

---

## ⚙️ Required Process (SYSTEMATIC)

### 1) Preflight
- Confirm existing patterns for:
  - ServiceResult Enhanced usage fileciteturn16file6L17-L31
  - Security rules: CSRF, validation, zero-cache, masking fileciteturn16file6L72-L114
- Identify existing user/profile code (search-first; no duplicates)

### 2) Discovery (Search-First, No Guessing)
Find existing equivalents before creating anything:
- existing profile actions/views
- existing User service methods (GetCurrentUser, UpdateUser, etc.)
- existing ViewModels and factories for user/account
- existing notification helpers (TempData notifications)

### 3) Module Map
Request → Controller → Service → Repo/DB → Response → View

### 4) Design (Minimal & Enterprise)
Implement the minimal structure:
- **GET** Profile/Edit action:
  - loads current user data → maps to `UserProfileEditViewModel` via Factory Method
  - returns strongly-typed view
- **POST** Profile/Edit action:
  - validates model
  - calls service `UpdateMyProfileAsync(model)` returning ServiceResult
  - on failure: returns view with validation errors + safe message
  - on success: redirect with success notification

### 5) Implementation Details (Diff-Style)
Provide minimal diffs for:
- Controller actions (GET/POST)
- Service method(s)
- ViewModel + Factory Method
- Razor view (formal UI, passive)
- NoCache + AntiForgery attributes
- ServiceResult handling & notifications

### 6) Tests & Verification (MANDATORY)
- Unit tests:
  - validation rules
  - service update logic (only allowed fields, ignores forbidden changes)
- Integration/functional (if test infra exists):
  - authenticated user can update profile
  - user cannot update another user
- Manual verification checklist:
  - valid update
  - invalid input
  - CSRF missing
  - cache headers (profile not cached)

### 7) Rollback
- How to revert safely (commit-level rollback)
- If risk is Medium/High: suggest feature flag / config toggle

---

## 📤 REQUIRED OUTPUT FORMAT (STRICT, Minimal Text)
1) Preflight Result  
2) Existing Reuse Findings (what already exists)  
3) Module Map  
4) Implementation Plan  
5) Diffs (per file, minimal)  
6) ServiceResult Examples (success + validation error)  
7) Tests  
8) Verification Steps  
9) Rollback  
10) Open Questions (only blocking)

---

## ✅ Acceptance Criteria
- [ ] Authenticated user can view + edit their own profile
- [ ] Unauthorized/unauthenticated users are blocked
- [ ] CSRF protection on POST
- [ ] Page is no-cache
- [ ] Server validation enforced + client validation present
- [ ] ServiceResult Enhanced used end-to-end fileciteturn16file6L17-L31
- [ ] Entity→ViewModel mapping via Factory Method only
- [ ] Tests added/updated

---
**Owner:** ClinicApp Engineering  
**Category:** Feature Implementation – User Profile  
**Style:** Enterprise · Contract-Locked · Minimal Diff · Test-First  
