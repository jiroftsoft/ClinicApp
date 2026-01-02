# 📱 ClinicApp – Main Menu (Home) Mobile-First Optimization Prompt (Healthcare · Production)

> **Use in Cursor:** Paste this prompt after:
> 1) `CLINICAPP_CURSOR_MASTER_CONTEXT.md`
> 2) `CLINICAPP_CURSOR_FLOW_DISCIPLINE_CONTRACT.md`
> 3) `CLINICAPP_VIEW_UI_BEAST_MODE_PROMPT_FINAL.md`
>
> Then attach the Home/MainMenu view + layout + partials + CSS/JS/bundles used by that page.

---

## 🎯 Mission
Optimize **Main Menu (Home)** to be:
- **100% mobile-first** (primary target)
- **Healthcare-formal** but **not cold/blank**
- **Fast** (minimal assets, no heavy animations)
- **Clear navigation** (no user confusion)
- **Reusable components** (partials/templates), SRP-compliant
- Production-safe, scenario-driven, tested

---

## 🧠 Reference Patterns (No Web Copying, Pattern-Level Only)
Use **pattern-level inspiration** from modern high-traffic products (مثل مکتبخونه/دیجی‌کالا) but adapt to healthcare:
1) **Sticky header** with:
   - Page title / clinic branding (small)
   - Primary action (e.g., “Reserve Appointment”) if appropriate
   - Optional search (if your app supports it)
2) **Primary CTA above the fold** (one obvious action)
3) **Quick actions grid** (icon + label) in 2×3 or 2×4 for mobile
4) **Card-based sections** with subtle surfaces (avoid pure white blankness)
5) **Bottom navigation (optional)** for top-level areas (Home, Appointments, Records, Profile)
6) **Clear hierarchy**: typography + spacing + grouping > loud colors
7) **Empty/error states** for dynamic items (AJAX components)

---

## 🏥 Healthcare UI Constraints (Strict)
- No flashy colors / no jelf styling
- No heavy animations
- High readability (spacing, font sizes, contrast)
- Calm palette: use **subtle surfaces** (off-white / light gray) + minimal accent
- Touch-friendly controls (44px min targets)
- Reduce cognitive load: max 6–10 primary actions visible

---

## ⚡ Performance Constraints (Strict)
- Avoid loading full heavy bundles for home if not needed
- Prefer existing CSS utilities and bundles; don’t introduce new frameworks
- Minimize DOM depth
- Lazy-load non-critical sections via AJAX (if they exist)

---

## 🧩 Componentization Requirements
Main menu must be componentized into partials:
- `_MainMenuHeader.cshtml`
- `_MainMenuQuickActions.cshtml`
- `_MainMenuSections.cshtml` (optional)
- `_MainMenuRecentActivity.cshtml` (AJAX, optional)
Reuse existing components if present (search first).

---

## 🌳 Flow Integrity (Production Critical)
If user clicks an action that requires auth (e.g., reservation):
- Preserve context across login/register/OTP
- Return user to the exact intended destination
- No silent redirects to home/dashboard
Flow break = CRITICAL BUG

---

## 🧪 Scenario Matrix (You MUST Produce)
Use `CLINICAPP_SCENARIO_MATRIX_TEMPLATE.md` for these flows at minimum:
- Home → Reserve Appointment (logged in)
- Home → Reserve Appointment (not logged in → login/register → return)
- Home → Profile
- Home → Patient Dashboard
- Network slow / API error for any AJAX components
- Back button / refresh / multi-tab

---

## 🔍 What to Fix (Given Problem Statement)
User reports:
- “Menu is too white / lifeless”
- “Must be 100% mobile optimized”

You MUST:
1) Identify the root cause of “too white” (CSS/layout/spacing/hierarchy) with evidence
2) Propose a calm, healthcare-appropriate visual structure:
   - subtle surfaces (cards, separators, section backgrounds)
   - better hierarchy (titles, spacing, grouping)
   - consistent icon+label buttons
3) Keep diffs minimal and consistent with existing design system
4) Ensure accessibility: focus states, labels, contrast

---

## 📤 REQUIRED OUTPUT (STRICT)
1) Preflight Result  
2) Current UI Map (layout/assets/components)  
3) Reuse Scan (exists vs missing)  
4) Scenario Matrix (all branches)  
5) Critical Issues (max 7, evidence)  
6) Root Causes  
7) Fix Plan (ranked)  
8) Implementation Diffs (Razor/CSS/JS)  
9) Tests + Verification Steps  
10) Rollback Strategy  

---

## ✅ Acceptance Criteria
- Mobile-first layout passes (small screen first)
- No “blank white” feel: improved hierarchy via surfaces/spacing/typography
- Clear CTAs, no user confusion
- Components are reusable partials
- No heavy assets/animations introduced
- Flow integrity preserved for auth-required actions
- Tests + verification + rollback included

---

**END – MAIN MENU OPTIMIZATION READY**
