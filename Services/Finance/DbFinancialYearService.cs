using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using ClinicApp.Interfaces.Finance;
using ClinicApp.Models;

namespace ClinicApp.Services.Finance
{
    /// <summary>
    /// سرویس سال مالی از دیتابیس - Production-Grade
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. دریافت سال مالی از FactorSettings
    /// 2. پشتیبانی از بازه تاریخ
    /// 3. فلگ جاری
    /// 4. Fallback به PersianCalendar
    /// </summary>
    public class DbFinancialYearService : IFinancialYearService
    {
        private readonly ApplicationDbContext _ctx;
        
        public DbFinancialYearService(ApplicationDbContext ctx) 
        { 
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx)); 
        }

        /// <summary>
        /// دریافت سال مالی جاری از دیتابیس
        /// </summary>
        /// <returns>سال مالی شمسی</returns>
        public int GetCurrentYear()
        {
            var now = DateTime.Now;

            // 1) در بازه تاریخ
            var byDate = _ctx.FactorSettings
                .Where(f => !f.IsDeleted && f.EffectiveFrom <= now && (f.EffectiveTo == null || now <= f.EffectiveTo))
                .OrderByDescending(f => f.FinancialYear)
                .Select(f => (int?)f.FinancialYear)
                .FirstOrDefault();
            if (byDate.HasValue) return byDate.Value;

            // 2) فلگ جاری
            var byFlag = _ctx.FactorSettings
                .Where(f => !f.IsDeleted && f.IsActiveForCurrentYear)
                .OrderByDescending(f => f.FinancialYear)
                .Select(f => (int?)f.FinancialYear)
                .FirstOrDefault();
            if (byFlag.HasValue) return byFlag.Value;

            // 3) آخرین سال
            var maxYear = _ctx.FactorSettings
                .Where(f => !f.IsDeleted)
                .OrderByDescending(f => f.FinancialYear)
                .Select(f => (int?)f.FinancialYear)
                .FirstOrDefault();
            if (maxYear.HasValue) return maxYear.Value;

            // 4) fallback
            var pc = new PersianCalendar();
            return pc.GetYear(now);
        }
    }
}
