# ✅ ClinicApp – OTP Login & Registration Module Review Checklist (Enterprise)

> **Purpose:**  
> Use this checklist to review/upgrade **OTP-based Login + Registration** in ClinicApp to an **enterprise healthcare** standard (security-first, reliable, fast, user-friendly).  
> Designed to be used in **Cursor** for systematic review and refactor.

---

## 0) Preflight (Mandatory)
- [ ] Contracts under `CONTRACTS/` are enforced (report only violations)
- [ ] Scope is defined (controllers, services, views, helpers, DB tables, SMS provider integration)
- [ ] Risk level: **Low / Medium / High / Critical**
- [ ] Backward compatibility plan exists (if replacing legacy flows)

---

## 1) Threat Model & Security Baseline (Non‑Negotiable)
- [ ] OTP is treated as **authentication factor**, not a password replacement
- [ ] No sensitive info in errors (e.g., “user exists/doesn’t exist”, phone validity)
- [ ] All OTP endpoints are HTTPS only (no mixed content)
- [ ] CSRF protection is correct for browser flows (anti-forgery where applicable)
- [ ] Rate limiting is applied (per IP + per phone + per account/device)
- [ ] Brute-force protection exists (attempt counters + temporary lockouts)
- [ ] Enumeration protection (same response for “phone not found” vs “phone found”)
- [ ] Audit logging exists (without storing OTP value)
- [ ] Secrets (SMS API keys) are not in source code; stored in config/secret store

---

## 2) OTP Code Policy (Enterprise Defaults)
- [ ] OTP length 6–8 digits (decide and document)
- [ ] OTP TTL is short (commonly 2–5 minutes)
- [ ] OTP is **single-use** (consumed on success)
- [ ] OTP is invalidated on new OTP issuance (latest OTP wins)
- [ ] OTP is stored **hashed** (never plaintext) or encrypted-at-rest
- [ ] Constant-time compare (or equivalent safe compare) is used for validation
- [ ] Max verification attempts per OTP (e.g., 5) enforced
- [ ] Cooldown between sends (e.g., 30–60 seconds) enforced

---

## 3) Delivery Channel & Provider Reliability
- [ ] SMS provider integration has timeouts + retries (with backoff)
- [ ] Provider failures handled gracefully (user-friendly message + retry)
- [ ] Idempotency for “send OTP” (avoid duplicate sends on retries)
- [ ] Delivery status is not trusted as proof of receipt
- [ ] Support for multiple providers or fallback plan (optional but recommended)
- [ ] No PII leakage in provider logs/requests beyond required fields

---

## 4) Flow Correctness (Login vs Registration)
### Login
- [ ] Flow: request OTP → verify OTP → create auth session/token
- [ ] If phone exists: proceed; if not, response remains non-enumerating
- [ ] Session creation is secure (cookie flags: HttpOnly/Secure/SameSite)

### Registration
- [ ] Registration is separated from login logic (SRP)
- [ ] Phone verification occurs **before** account creation (or creation is pending until verified)
- [ ] Duplicate phone handling is safe and consistent (no race)
- [ ] “Linking” rule exists if phone belongs to existing account

---

## 5) State & Data Model (Consistency + Concurrency)
- [ ] OTP request records include: phone, createdAt, expiresAt, attempts, consumedAt, channel, requestId
- [ ] Concurrency-safe “consume OTP” (transaction/atomic update)
- [ ] Race conditions prevented (two verifies cannot both succeed)
- [ ] Cleanup strategy for expired OTPs (job/cleanup query)
- [ ] Indexes exist for lookups (phone + status + expiresAt)
- [ ] PII storage is minimal; phone normalized (E.164) and validated server-side

---

## 6) UX (Healthcare Admin: Formal, Fast, Mobile-First)
- [ ] UI is formal/administrative (no flashy colors, no heavy animations)
- [ ] Mobile-first layout (touch-friendly inputs, proper keyboard types)
- [ ] OTP input UX:
  - [ ] Clear countdown / resend availability
  - [ ] Resend button disabled during cooldown
  - [ ] Paste-friendly OTP input
  - [ ] Clear error messages without leaking security details
- [ ] Accessibility:
  - [ ] Labels, focus order, aria attributes for errors where needed
- [ ] AJAX-first where no full refresh is required (partial loading, clean error state)

---

## 7) Validation (Sensitive Forms)
- [ ] Server-side validation is authoritative for:
  - [ ] phone format/normalization
  - [ ] OTP format
  - [ ] TTL expiry
  - [ ] attempts exceeded
- [ ] Client-side validation exists for UX (format hints), but never trusted
- [ ] Validation errors are returned via **ServiceResult Enhanced**
- [ ] Error display is consistent (field-level + summary)

---

## 8) Architecture & SRP (No God-Controllers)
- [ ] Views are passive (no business logic)
- [ ] Controller orchestrates only (no OTP logic)
- [ ] OTP logic resides in a dedicated service (e.g., `OtpService`)
- [ ] Provider integration behind interface (e.g., `ISmsSender`)
- [ ] Entity → ViewModel mapping uses Factory Method (no inline mapping)
- [ ] No duplicate utilities/classes created (search before adding)

---

## 9) ServiceResult Enhanced Contract
- [ ] All OTP endpoints return `ServiceResult` (Enhanced), including:
  - [ ] success
  - [ ] validation errors
  - [ ] rate limit / lockout responses
  - [ ] provider failure responses
- [ ] Codes/categories are consistent for client handling
- [ ] Metadata can carry retryAfter / cooldownSeconds safely

---

## 10) Observability (What pros always add)
- [ ] Structured logs with correlation id (requestId)
- [ ] Metrics tracked:
  - [ ] OTP send attempts / success rate
  - [ ] verification success rate
  - [ ] provider latency/failure rate
  - [ ] lockout events
- [ ] Alerts for abnormal spikes (send failures, brute force attempts)

---

## 11) Testing (Must Have)
### Unit Tests
- [ ] OTP generation policy (length, randomness, TTL)
- [ ] attempt counting and lockout
- [ ] consume OTP is single-use
- [ ] resend invalidates previous OTP

### Integration Tests
- [ ] end-to-end: request OTP → verify → login session created
- [ ] wrong OTP attempts capped
- [ ] expired OTP rejected
- [ ] provider failure path returns safe message

### Security Tests (at least checklist)
- [ ] enumeration resistance
- [ ] rate limiting effective
- [ ] CSRF protection for browser endpoints
- [ ] no OTP value ever logged

---

## 12) Rollback & Safe Deployment
- [ ] Feature flag / toggle for new OTP flow (recommended)
- [ ] Safe rollback plan documented
- [ ] Migration plan for existing users (if flow changes)

---

## 🔹 Cursor Review Output Template (Paste in PR)
- **Critical issues (max 5):**
- **Root causes:**
- **Minimal fix plan:**
- **Tests to add/update:**
- **Verification steps:**
- **Rollback plan:**

---

**Owner:** ClinicApp Engineering  
**Category:** Auth / OTP  
**Status:** Ready for Cursor Review  
