using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Insurance;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس ایجاد تعرفه‌های بیمه تکمیلی
    /// طراحی شده برای محیط درمانی کلینیک شفا
    /// </summary>
    public class SupplementaryTariffSeederService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public SupplementaryTariffSeederService(
            ApplicationDbContext context,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _log = logger.ForContext<SupplementaryTariffSeederService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// ایجاد تعرفه‌های بیمه تکمیلی برای خدمات موجود
        /// </summary>
        public async Task<ServiceResult> CreateSupplementaryTariffsAsync()
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع ایجاد تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // دریافت طرح بیمه تکمیلی
                var supplementaryPlan = await _context.InsurancePlans
                    .FirstOrDefaultAsync(ip => ip.PlanCode == "SUPPLEMENTARY_PLUS" && !ip.IsDeleted);

                if (supplementaryPlan == null)
                {
                    _log.Warning("🏥 MEDICAL: طرح بیمه تکمیلی یافت نشد. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("طرح بیمه تکمیلی یافت نشد");
                }

                // دریافت خدمات فعال
                var activeServices = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .ToListAsync();

                if (!activeServices.Any())
                {
                    _log.Warning("🏥 MEDICAL: هیچ خدمت فعالی یافت نشد. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("هیچ خدمت فعالی یافت نشد");
                }

                var createdTariffs = new List<InsuranceTariff>();
                var currentDate = DateTime.UtcNow;

                foreach (var service in activeServices)
                {
                    // بررسی وجود تعرفه بیمه تکمیلی برای این خدمت
                    var existingTariff = await _context.InsuranceTariffs
                        .FirstOrDefaultAsync(t => t.ServiceId == service.ServiceId && 
                                                t.InsurancePlanId == supplementaryPlan.InsurancePlanId &&
                                                t.InsuranceType == InsuranceType.Supplementary &&
                                                !t.IsDeleted);

                    if (existingTariff != null)
                    {
                        _log.Debug("🏥 MEDICAL: تعرفه بیمه تکمیلی برای خدمت {ServiceId} ({ServiceTitle}) از قبل موجود است. User: {UserName} (Id: {UserId})",
                            service.ServiceId, service.Title, _currentUserService.UserName, _currentUserService.UserId);
                        continue;
                    }

                    // محاسبه تعرفه بیمه تکمیلی
                    var supplementaryTariff = new InsuranceTariff
                    {
                        ServiceId = service.ServiceId,
                        InsurancePlanId = supplementaryPlan.InsurancePlanId,
                        InsuranceType = InsuranceType.Supplementary,
                        TariffPrice = service.Price, // استفاده از قیمت پایه خدمت
                        PatientShare = service.Price * 0.1m, // 10% سهم بیمار
                        InsurerShare = service.Price * 0.9m, // 90% سهم بیمه
                        SupplementaryCoveragePercent = 90m, // 90% پوشش
                        SupplementaryMaxPayment = service.Price * 0.9m, // حداکثر پرداخت بیمه
                        Priority = 2, // اولویت دوم (بعد از بیمه اصلی)
                        StartDate = currentDate,
                        EndDate = currentDate.AddYears(1), // اعتبار یک ساله
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = currentDate,
                        CreatedByUserId = _currentUserService.UserId,
                        UpdatedAt = currentDate,
                        UpdatedByUserId = _currentUserService.UserId
                    };

                    _context.InsuranceTariffs.Add(supplementaryTariff);
                    createdTariffs.Add(supplementaryTariff);

                    _log.Debug("🏥 MEDICAL: تعرفه بیمه تکمیلی برای خدمت {ServiceId} ({ServiceTitle}) ایجاد شد - Price: {Price}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}. User: {UserName} (Id: {UserId})",
                        service.ServiceId, service.Title, service.Price, supplementaryTariff.PatientShare, supplementaryTariff.InsurerShare, _currentUserService.UserName, _currentUserService.UserId);
                }

                // ذخیره تغییرات
                await _context.SaveChangesAsync();

                _log.Information("🏥 MEDICAL: تعرفه‌های بیمه تکمیلی با موفقیت ایجاد شدند - Count: {Count}, Services: {ServicesCount}. User: {UserName} (Id: {UserId})",
                    createdTariffs.Count, activeServices.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful($"تعرفه‌های بیمه تکمیلی برای {createdTariffs.Count} خدمت ایجاد شد");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("خطا در ایجاد تعرفه‌های بیمه تکمیلی");
            }
        }

        /// <summary>
        /// ایجاد تعرفه‌های بیمه تکمیلی برای خدمت خاص
        /// </summary>
        public async Task<ServiceResult> CreateSupplementaryTariffForServiceAsync(int serviceId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: ایجاد تعرفه بیمه تکمیلی برای خدمت {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت خدمت
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                if (service == null)
                {
                    _log.Warning("🏥 MEDICAL: خدمت {ServiceId} یافت نشد. User: {UserName} (Id: {UserId})",
                        serviceId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("خدمت یافت نشد");
                }

                // دریافت طرح بیمه تکمیلی
                var supplementaryPlan = await _context.InsurancePlans
                    .FirstOrDefaultAsync(ip => ip.PlanCode == "SUPPLEMENTARY_PLUS" && !ip.IsDeleted);

                if (supplementaryPlan == null)
                {
                    _log.Warning("🏥 MEDICAL: طرح بیمه تکمیلی یافت نشد. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("طرح بیمه تکمیلی یافت نشد");
                }

                // بررسی وجود تعرفه بیمه تکمیلی
                var existingTariff = await _context.InsuranceTariffs
                    .FirstOrDefaultAsync(t => t.ServiceId == serviceId && 
                                            t.InsurancePlanId == supplementaryPlan.InsurancePlanId &&
                                            t.InsuranceType == InsuranceType.Supplementary &&
                                            !t.IsDeleted);

                if (existingTariff != null)
                {
                    _log.Warning("🏥 MEDICAL: تعرفه بیمه تکمیلی برای خدمت {ServiceId} از قبل موجود است. User: {UserName} (Id: {UserId})",
                        serviceId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("تعرفه بیمه تکمیلی برای این خدمت از قبل موجود است");
                }

                // ایجاد تعرفه بیمه تکمیلی
                var currentDate = DateTime.UtcNow;
                var supplementaryTariff = new InsuranceTariff
                {
                    ServiceId = serviceId,
                    InsurancePlanId = supplementaryPlan.InsurancePlanId,
                    InsuranceType = InsuranceType.Supplementary,
                    TariffPrice = service.Price,
                    PatientShare = service.Price * 0.1m, // 10% سهم بیمار
                    InsurerShare = service.Price * 0.9m, // 90% سهم بیمه
                    SupplementaryCoveragePercent = 90m,
                    SupplementaryMaxPayment = service.Price * 0.9m,
                    Priority = 2,
                    StartDate = currentDate,
                    EndDate = currentDate.AddYears(1),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = currentDate,
                    CreatedByUserId = _currentUserService.UserId,
                    UpdatedAt = currentDate,
                    UpdatedByUserId = _currentUserService.UserId
                };

                _context.InsuranceTariffs.Add(supplementaryTariff);
                await _context.SaveChangesAsync();

                _log.Information("🏥 MEDICAL: تعرفه بیمه تکمیلی برای خدمت {ServiceId} ({ServiceTitle}) با موفقیت ایجاد شد. User: {UserName} (Id: {UserId})",
                    serviceId, service.Title, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("تعرفه بیمه تکمیلی با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه بیمه تکمیلی برای خدمت {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("خطا در ایجاد تعرفه بیمه تکمیلی");
            }
        }

        /// <summary>
        /// دریافت آمار تعرفه‌های بیمه تکمیلی
        /// </summary>
        public async Task<ServiceResult<SupplementaryTariffStats>> GetSupplementaryTariffStatsAsync()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست آمار تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var stats = new SupplementaryTariffStats
                {
                    TotalServices = await _context.Services.CountAsync(s => !s.IsDeleted && s.IsActive),
                    TotalSupplementaryTariffs = await _context.InsuranceTariffs.CountAsync(t => 
                        t.InsuranceType == InsuranceType.Supplementary && !t.IsDeleted),
                    ActiveSupplementaryTariffs = await _context.InsuranceTariffs.CountAsync(t => 
                        t.InsuranceType == InsuranceType.Supplementary && !t.IsDeleted && t.IsActive),
                    ExpiredSupplementaryTariffs = await _context.InsuranceTariffs.CountAsync(t => 
                        t.InsuranceType == InsuranceType.Supplementary && !t.IsDeleted && 
                        t.EndDate.HasValue && t.EndDate.Value < DateTime.UtcNow)
                };

                _log.Information("🏥 MEDICAL: آمار تعرفه‌های بیمه تکمیلی دریافت شد - TotalServices: {TotalServices}, TotalTariffs: {TotalTariffs}, ActiveTariffs: {ActiveTariffs}, ExpiredTariffs: {ExpiredTariffs}. User: {UserName} (Id: {UserId})",
                    stats.TotalServices, stats.TotalSupplementaryTariffs, stats.ActiveSupplementaryTariffs, stats.ExpiredSupplementaryTariffs, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<SupplementaryTariffStats>.Successful(stats);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت آمار تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<SupplementaryTariffStats>.Failed("خطا در دریافت آمار تعرفه‌های بیمه تکمیلی");
            }
        }
    }

    /// <summary>
    /// آمار تعرفه‌های بیمه تکمیلی
    /// </summary>
    public class SupplementaryTariffStats
    {
        public int TotalServices { get; set; }
        public int TotalSupplementaryTariffs { get; set; }
        public int ActiveSupplementaryTariffs { get; set; }
        public int ExpiredSupplementaryTariffs { get; set; }
    }
}
