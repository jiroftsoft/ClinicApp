using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر اصلاح داده‌های بیمه
    /// </summary>
    public class DataFixController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _log;

        public DataFixController(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _log = logger.ForContext<DataFixController>();
        }

        /// <summary>
        /// صفحه اصلی اصلاح داده‌ها
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// بررسی وضعیت فعلی داده‌ها
        /// </summary>
        public async Task<ActionResult> CheckCurrentData()
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع بررسی وضعیت فعلی داده‌های بیمه");

                // بررسی وضعیت فعلی InsuranceType
                var insuranceTypeStatus = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted)
                    .GroupBy(t => t.InsuranceType)
                    .Select(g => new
                    {
                        InsuranceType = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.InsuranceType)
                    .ToListAsync();

                // بررسی وضعیت بر اساس InsurancePlanId
                var planStatus = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted)
                    .GroupBy(t => new { t.InsurancePlanId, t.InsuranceType })
                    .Select(g => new
                    {
                        g.Key.InsurancePlanId,
                        g.Key.InsuranceType,
                        Count = g.Count(),
                        AvgPatientShare = g.Average(t => t.PatientShare ?? 0),
                        AvgInsurerShare = g.Average(t => t.InsurerShare ?? 0)
                    })
                    .OrderBy(x => x.InsurancePlanId)
                    .ThenBy(x => x.InsuranceType)
                    .ToListAsync();

                // بررسی رکوردهای مشکل‌دار
                var problematicRecords = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted && (
                        t.InsuranceType == 0 ||
                        (t.InsuranceType == InsuranceType.Supplementary && t.PatientShare == 100) ||
                        (t.InsuranceType == InsuranceType.Primary && t.InsurancePlanId == 4)
                    ))
                    .Select(t => new
                    {
                        t.InsuranceTariffId,
                        t.InsurancePlanId,
                        t.InsuranceType,
                        t.PatientShare,
                        t.InsurerShare
                    })
                    .ToListAsync();

                var result = new
                {
                    InsuranceTypeStatus = insuranceTypeStatus,
                    PlanStatus = planStatus,
                    ProblematicRecords = problematicRecords,
                    TotalProblematicRecords = problematicRecords.Count
                };

                _log.Information("🏥 MEDICAL: بررسی وضعیت فعلی تکمیل شد - ProblematicRecords: {Count}", 
                    problematicRecords.Count);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در بررسی وضعیت فعلی داده‌ها");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// اجرای اصلاح داده‌ها
        /// </summary>
        public async Task<ActionResult> FixData()
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع اصلاح داده‌های بیمه");

                var results = new List<string>();

                // 1. اصلاح InsuranceType = 0 به مقادیر صحیح
                // Plan 1: بیمه پایه (Primary)
                var plan1Records = await _context.InsuranceTariffs
                    .Where(t => t.InsurancePlanId == 1 && t.InsuranceType == 0 && !t.IsDeleted)
                    .ToListAsync();

                foreach (var record in plan1Records)
                {
                    record.InsuranceType = InsuranceType.Primary;
                }
                results.Add($"Plan 1: {plan1Records.Count} records fixed to Primary");

                // Plan 2: بیمه پایه (Primary) 
                var plan2Records = await _context.InsuranceTariffs
                    .Where(t => t.InsurancePlanId == 2 && t.InsuranceType == 0 && !t.IsDeleted)
                    .ToListAsync();

                foreach (var record in plan2Records)
                {
                    record.InsuranceType = InsuranceType.Primary;
                }
                results.Add($"Plan 2: {plan2Records.Count} records fixed to Primary");

                // Plan 4: بیمه تکمیلی (Supplementary)
                var plan4Records = await _context.InsuranceTariffs
                    .Where(t => t.InsurancePlanId == 4 && t.InsuranceType == InsuranceType.Primary && !t.IsDeleted)
                    .ToListAsync();

                foreach (var record in plan4Records)
                {
                    record.InsuranceType = InsuranceType.Supplementary;
                }
                results.Add($"Plan 4: {plan4Records.Count} records fixed to Supplementary");

                // 2. اصلاح منطق بیمه تکمیلی - Plan 1 باید Primary باشد نه Supplementary
                var plan1SupplementaryRecords = await _context.InsuranceTariffs
                    .Where(t => t.InsurancePlanId == 1 && 
                               t.InsuranceType == InsuranceType.Supplementary && 
                               !t.IsDeleted)
                    .ToListAsync();

                foreach (var record in plan1SupplementaryRecords)
                {
                    record.InsuranceType = InsuranceType.Primary;
                }
                results.Add($"Plan 1 Supplementary to Primary: {plan1SupplementaryRecords.Count} records fixed");

                // 3. اصلاح منطق بیمه تکمیلی - Plan 4 باید Supplementary باشد
                var plan4PrimaryRecords = await _context.InsuranceTariffs
                    .Where(t => t.InsurancePlanId == 4 && 
                               t.InsuranceType == InsuranceType.Primary && 
                               !t.IsDeleted)
                    .ToListAsync();

                foreach (var record in plan4PrimaryRecords)
                {
                    record.InsuranceType = InsuranceType.Supplementary;
                }
                results.Add($"Plan 4 Primary to Supplementary: {plan4PrimaryRecords.Count} records fixed");

                // ذخیره تغییرات
                await _context.SaveChangesAsync();

                _log.Information("🏥 MEDICAL: اصلاح داده‌ها تکمیل شد - Total Changes: {Total}", 
                    plan1Records.Count + plan2Records.Count + plan4Records.Count + 
                    plan1SupplementaryRecords.Count + plan4PrimaryRecords.Count);

                return Json(new { success = true, results = results }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اصلاح داده‌ها");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی نتایج پس از اصلاح
        /// </summary>
        public async Task<ActionResult> VerifyFix()
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع بررسی نتایج اصلاح");

                // بررسی وضعیت نهایی InsuranceType
                var finalInsuranceTypeStatus = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted)
                    .GroupBy(t => t.InsuranceType)
                    .Select(g => new
                    {
                        InsuranceType = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.InsuranceType)
                    .ToListAsync();

                // بررسی وضعیت نهایی بر اساس InsurancePlanId
                var finalPlanStatus = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted)
                    .GroupBy(t => new { t.InsurancePlanId, t.InsuranceType })
                    .Select(g => new
                    {
                        g.Key.InsurancePlanId,
                        g.Key.InsuranceType,
                        Count = g.Count(),
                        AvgPatientShare = g.Average(t => t.PatientShare ?? 0),
                        AvgInsurerShare = g.Average(t => t.InsurerShare ?? 0)
                    })
                    .OrderBy(x => x.InsurancePlanId)
                    .ThenBy(x => x.InsuranceType)
                    .ToListAsync();

                // بررسی رکوردهای بیمه تکمیلی اصلاح شده
                var supplementaryStatus = await _context.InsuranceTariffs
                    .Where(t => !t.IsDeleted && t.InsuranceType == InsuranceType.Supplementary)
                    .GroupBy(t => t.InsurancePlanId)
                    .Select(g => new
                    {
                        InsurancePlanId = g.Key,
                        Count = g.Count(),
                        AvgPatientShare = g.Average(t => t.PatientShare ?? 0),
                        AvgInsurerShare = g.Average(t => t.InsurerShare ?? 0)
                    })
                    .OrderBy(x => x.InsurancePlanId)
                    .ToListAsync();

                var result = new
                {
                    FinalInsuranceTypeStatus = finalInsuranceTypeStatus,
                    FinalPlanStatus = finalPlanStatus,
                    SupplementaryStatus = supplementaryStatus
                };

                _log.Information("🏥 MEDICAL: بررسی نتایج اصلاح تکمیل شد");

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در بررسی نتایج اصلاح");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
