# Reception V2 – Performance & Scalability Analysis

**Scope:** Backend only (EF queries, services, transactions, indexes, concurrency).  
**URL:** `http://localhost:3560/ReceptionV2/Index`  
**Focus:** Production readiness, no UI design.

---

## 1. Top Performance Issues (Critical)

### P1. N+1 in RecalculateDraftAsync (insurance calculation per item)

**Where:** `ReceptionFacade.RecalculateDraftAsync` (lines ~4056–4144)  
**Issue:** When `draft.PatientId > 0` and `insuranceCalculations` does not already contain all service IDs, the code runs:

```csharp
foreach (var item in draft.ReceptionItems.Where(i => !i.IsDeleted))
{
    if (!insuranceCalculations.ContainsKey(item.ServiceId))
    {
        var quoteResult = await _pricingEngine.QuoteAsync(quoteRequest);  // 1 round-trip per item
        // ...
    }
}
```

Each `QuoteAsync` is an async round-trip (DB + pricing logic). With 10 items this adds **10 sequential round-trips** on every AddItem, RemoveItem, SetInsurances, and on every Finalize (totals path). This is the **single largest performance bottleneck** in the reception hot path.

**Impact:** Add item with 5 services → 5 extra QuoteAsync calls inside RecalculateDraftAsync. SetInsurances after adding 10 services → 10 QuoteAsync. Latency scales linearly with item count.

**Fix:**

- **Option A (recommended):** In RecalculateDraftAsync, **do not** call QuoteAsync per item when totals are already available from `CalculateTotalsAsync` (SnapshotJson). Use SnapshotJson for DTO display (base/supp/patient share) and only call QuoteAsync when you truly need a fresh quote (e.g. insurance changed and Snapshot not yet updated). After SetInsurances you already call `RepriceReceptionAsync`, so RecalculateDraftAsync can rely on updated SnapshotJson and avoid per-item QuoteAsync.
- **Option B:** If real-time insurance display is required for every item, add a **batch** method to PricingEngine, e.g. `QuoteBatchAsync(IEnumerable<QuoteRequestDto>)`, and call it once per RecalculateDraftAsync with all items, then map results by ServiceId.

---

### P2. LoadInitialAsync: four sequential factor calls + diagnostic Count queries

**Where:** `ReceptionFacade.LoadInitialAsync`  
**Issue:**

- When `deptId.HasValue`, the code runs **4 CountAsync** (step1–step4) for “diagnostic” logging, then one heavy ToListAsync with multiple Includes for doctors. That is **5 extra round-trips** per page load when a department is selected (and the Index currently calls `LoadInitialAsync(1, null)`, so deptId is null and these are skipped for initial load; they still run when frontend later requests doctors by department).
- **Factor settings:** Four separate calls to `GetActiveFactorByTypeAndHashtaggedAsync` (Technical false/true, Professional false/true) run **sequentially**. Each may hit DB.

**Impact:** First load (deptId = null): 1 (clinics) + departments service + shared services + **4 factor calls**. When user selects department: +1 department + **4 CountAsync** + 1 doctors ToListAsync = slow.

**Fix:**

- **Remove or gate the 4 CountAsync** (step1–step4) behind a debug/log-level check or remove for production. They are not needed for business logic.
- **Batch or parallelize factor loading:** Either add `GetActiveFactorsForYearAsync(int financialYear)` that returns all 4 factors in one query (or one round-trip), or call the 4 existing methods with `Task.WhenAll(...)` so they run in parallel instead of sequentially.

---

### P3. RecalculateDraftAsync called twice in Finalize path

**Where:** `ReceptionFacade.FinalizePosAsync` / `FinalizeCashAsync`  
**Issue:** Finalize flow:

1. Load draft (Include ReceptionItems).
2. `ValidateDraftForFinalizeAsync(draft)`.
3. **`RecalculateDraftAsync(draft)`** → returns totals (and inside it: CalculateTotalsAsync, optional per-item QuoteAsync N+1, SaveChanges, Services lookup, etc.).
4. Amount validation.
5. Then transaction + SaveChanges + verification query.

So **RecalculateDraftAsync** runs once for “totals” and does full work (including N+1 if insurance calc runs). If you later add any other call to RecalculateDraft in the same request, that would double the cost.

**Impact:** Every finalize pays the full RecalculateDraftAsync cost (including N+1). Not “double” currently, but RecalculateDraftAsync is already heavy; any duplicate call would be critical.

**Fix:** Keep a single RecalculateDraftAsync for totals in Finalize. Ensure RecalculateDraftAsync itself is optimized (see P1) so this single call is cheap. Do not add another RecalculateDraftAsync in the same finalize flow.

---

### P4. No composite index for duplicate-draft lookup (CreateDraft)

**Where:** `ReceptionFacade.CreateDraftAsync`  
**Query:** Finds existing Pending draft with same PatientId, DoctorId, ClinicId, DepartmentId, TotalAmount == 0, CreatedAt > fiveMinutesAgo, CreatedByUserId. Filter uses:  
`PatientId, DoctorId, ClinicId, DepartmentId, Status, TotalAmount, IsDeleted, CreatedAt, CreatedByUserId`.

**Issue:** Table has single-column indexes (PatientId, DoctorId, Status, etc.) and composite indexes `(PatientId, ReceptionDate, Status)` and `(DoctorId, ReceptionDate, Status)`. The duplicate-draft query does **not** use ReceptionDate; it uses **CreatedAt** and **CreatedByUserId**. So the duplicate check may do index scans + residual filters.

**Impact:** Under concurrent create-draft load, this query runs often and can be slower than necessary.

**Fix:** Add a composite index for the duplicate-draft scenario, e.g.:

- `(Status, CreatedByUserId, CreatedAt)`  
  or, if one wants to cover the full predicate:
- `(PatientId, DoctorId, ClinicId, DepartmentId, Status)`  
  (and optionally include `CreatedAt` for a covering index if the engine supports it).

Choose based on actual query plans and table size. At least one of the above will significantly speed up the duplicate check.

---

### P5. ReceptionPricingService.RepriceAllAsync N+1

**Where:** `ReceptionPricingService.RepriceAllAsync`  
**Issue:** After `RepriceReceptionAsync(receptionId)`, the code loads reception with items and then:

```csharp
foreach (var item in reception.ReceptionItems.Where(i => !i.IsDeleted))
{
    var pricing = await PriceItemAsync(receptionId, item.ReceptionItemId);  // 1 query + QuoteAsync per item
    pricings.Add(pricing);
}
```

Each `PriceItemAsync` does at least one DB query (ReceptionItem + Reception) and one `QuoteAsync`. So **2N round-trips** for N items.

**Impact:** Used from SetInsurances path (after setting insurance, “reprice on change”). With many items, SetInsurances becomes very slow.

**Fix:** Either:

- Remove the per-item `PriceItemAsync` loop and only call `CalculateTotalsAsync(receptionId)` after Reprice (if UI only needs totals), or
- Introduce a batch pricing API (e.g. “get pricings for all items of this reception”) that does one or two round-trips and map results to items.

---

## 2. Medium Performance Risks

### M1. Transaction scope duration in Finalize

**Where:** `FinalizePosAsync` / `FinalizeCashAsync` — `using (var transaction = _context.Database.BeginTransaction())`  
**Issue:** The transaction spans: SaveChangesAsync, verification query (FirstOrDefaultAsync for savedPayment), and any further logic (receipt, logging) before Commit. If receipt generation or logging is slow or does I/O, the transaction and row locks are held longer than necessary.

**Impact:** Under concurrency, longer-held locks increase blocking and deadlock risk (even if current design is short).

**Fix:** Keep the transaction minimal: only Add/Update entities and SaveChanges + one verification query, then Commit. Move receipt URL generation, non-critical logging, and any external calls **after** Commit. If receipt must be inside the same “logical” operation, consider a separate, short-lived transaction or fire-and-forget job after the main commit.

---

### M2. LoadInitialAsync doctor query with large Include graph (when deptId set)

**Where:** LoadInitialAsync when `deptId.HasValue`  
**Issue:** Final doctor list is loaded with:

```csharp
.Include(dd => dd.Doctor)
.Include(dd => dd.Department)
.Include(dd => dd.Doctor.DoctorSpecializations)
.Include(dd => dd.Doctor.DoctorSpecializations.Select(ds => ds.Specialization))
```

This materializes a large graph. For “reception list” you only need a few fields per doctor (Id, Name, Code, Specialization).

**Impact:** More data than needed transferred and tracked; slower and more memory.

**Fix:** Use a **projection** (Select) to DTO instead of Include when loading for dropdown/list, e.g. like `GetDoctorsByDepartmentAsync` which uses Select to anonymous then to DoctorDto. Use the same pattern in LoadInitialAsync for doctors so no Include of Doctor/Department/Specializations is needed.

---

### M3. GetReceptionDetailsFullAsync / Print view

**Where:** Used for Print and possibly edit/detail views  
**Issue:** Query uses many Includes (Patient, Department, Clinic, ActivePatientInsurance, InsurancePlan, SupplementaryInsurancePlan, ReceptionItems.Service, Transactions, CreatedByUser, etc.). Full graph load for one reception.

**Impact:** One heavy query per print/open; acceptable for low frequency but can be optimized.

**Fix:** For Print, use a **projection** or a dedicated “reception print DTO” query that selects only the columns needed for the receipt (reception no, date, patient name, items, totals, etc.) in one or two queries, instead of loading full entities with Includes.

---

### M4. CleanupOldIncompleteDraftsAsync loads all matching drafts into memory

**Where:** `ReceptionFacade.CleanupOldIncompleteDraftsAsync`  
**Issue:** Loads all Pending receptions with `CreatedAt < cutoff` and Include(ReceptionItems) into a list, then iterates and Remove() on each. Single SaveChanges at the end.

**Impact:** If there are thousands of old drafts, this loads thousands of receptions + items into memory and can cause high memory and long-running transactions.

**Fix:** Process in **batches** (e.g. take 100 ReceptionIds, load and delete those, SaveChanges, repeat). Optionally add a limit (e.g. max 500 per run) and run the job more frequently.

---

### M5. SetInsurances: RepriceReceptionAsync + LoadAsync + RecalculateDraftAsync

**Where:** `ReceptionFacade.SetInsurancesAsync`  
**Issue:** After updating draft and PatientInsurances, the code calls:

1. `_pricingEngine.RepriceReceptionAsync(draft.ReceptionId)` (may do N updates),
2. `_context.Entry(draft).Collection(x => x.ReceptionItems).LoadAsync()`,
3. `RecalculateDraftAsync(draft)` (which does CalculateTotalsAsync + N+1 QuoteAsync when building insurance DTOs).

So SetInsurances does reprice + reload + full recalc with N+1. Combined with P1 and P5, this path is heavy.

**Fix:** Optimize RecalculateDraftAsync (P1) so it does not call QuoteAsync per item when SnapshotJson is already updated by RepriceReceptionAsync. Then SetInsurances becomes reprice + reload + one lightweight recalc (totals from SnapshotJson + single Services lookup).

---

## 3. Scalability & Concurrency

### Already in good shape

- **IdempotencyKey** required for Finalize; duplicate payment is prevented.
- **“Already finalized”** check by PaymentTransactions for the same ReceptionId avoids double finalize when draft is gone (race).
- **Finalize** uses a single DB transaction and verification query; different receptionists finalizing different receptions touch different rows (no unnecessary contention).
- **Receptions** have indexes on Status, PatientId, DoctorId, ClinicId, DepartmentId, etc. PK lookup by ReceptionId is O(1).

### Recommendations

- Add **composite index** for duplicate-draft check (see P4).
- Keep **transaction scope** in Finalize as short as possible (see M1).
- If you need to detect concurrent edits on the same draft (e.g. two tabs), consider **RowVersion** on Reception and handle DbUpdateConcurrencyException; currently last-write-wins.

---

## 4. Query & Code Improvements (Summary)

| Area | Current | Improvement |
|------|---------|-------------|
| RecalculateDraftAsync | 1 CalculateTotalsAsync + N × QuoteAsync (when building insurance DTOs) | Use SnapshotJson for item-level display when draft is already repriced; or add QuoteBatchAsync and call once. |
| LoadInitialAsync (doctors) | 4 CountAsync + 1 big Include ToListAsync when deptId set | Remove or gate CountAsync; use Select projection to DTO (no deep Include). |
| LoadInitialAsync (factors) | 4 sequential GetActiveFactor* | One batched method or Task.WhenAll for 4 calls. |
| CreateDraft duplicate check | Filter on many columns, no tailored index | Add composite index (Status, CreatedByUserId, CreatedAt) or (PatientId, DoctorId, ClinicId, DepartmentId, Status). |
| RepriceAllAsync | N × PriceItemAsync (2N round-trips) | Use CalculateTotalsAsync only, or batch pricing API. |
| Finalize transaction | Includes post-save work in same transaction | Commit immediately after SaveChanges + verification; do receipt/other work after. |
| CleanupOldIncompleteDrafts | Load all + single SaveChanges | Process in batches (e.g. 100 IDs per batch) with limit per run. |
| GetReceptionDetailsFull / Print | Full graph Include | Projection or dedicated print DTO query. |

---

## 5. Architecture Suggestions

1. **Pricing / Quote:** Introduce a **batch quote** API in PricingEngine (`QuoteBatchAsync`) and use it in RecalculateDraftAsync and anywhere you need quotes for multiple items. This removes N+1 at the cost of one slightly larger call.
2. **Read model for list/print:** For “reception list” and “print” views, consider thin read DTOs filled by a single (or two) targeted query with projection, instead of loading full entity graphs. This reduces load and keeps controllers/services simple.
3. **Background jobs:** CleanupOldIncompleteDrafts and any heavy reporting could run as background jobs with batch limits and run-length limits to avoid long-running transactions and memory spikes.
4. **API efficiency:** Ensure all reception API actions are async end-to-end and return compressed JSON where applicable (e.g. Enable response compression in host). No change to business logic, but better throughput under load.

---

## 6. Index Checklist (Existing vs Suggested)

**Existing (Reception):**  
PatientId, DoctorId, Status, ClinicId, DepartmentId, CreatedAt, CreatedByUserId, IsDeleted, …  
Composite: (PatientId, ReceptionDate, Status), (DoctorId, ReceptionDate, Status).

**Suggested / Applied:**

- Receptions: `(Status, CreatedByUserId, CreatedAt)` برای کوئری Duplicate-Draft در CreateDraftAsync در پیکربندی entity اضافه شده است (`IX_Reception_Status_CreatedByUserId_CreatedAt`). **ایندکس با Add-Migration و Update-Database اعمال می‌شود.**
- ReceptionItems: Already has (ReceptionId, ServiceId). No change needed for current hot paths.
- PaymentTransactions: Already has ReceptionId and IdempotencyKey. No change needed.

---

**Document version:** 1.0  
**Last updated:** 2025-02
