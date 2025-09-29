using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Insurance;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس اصلاح داده‌های بیمه تکمیلی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اصلاح منطق اشتباه بیمه تکمیلی
    /// 2. تبدیل InsurerShare به PatientShare
    /// 3. تنظیم صحیح درصد پوشش
    /// 4. اعتبارسنجی داده‌ها
    /// </summary>
    public class SupplementaryInsuranceDataFixService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _log;

        public SupplementaryInsuranceDataFixService(
            ApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _log = logger.ForContext<SupplementaryInsuranceDataFixService>();
        }

        /// <summary>
        /// اصلاح رکورد خاص InsuranceTariffId 1202
        /// </summary>
        public async Task<ServiceResult> FixSpecificRecordAsync(int tariffId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع اصلاح رکورد خاص - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);

                var tariff = await _context.InsuranceTariffs
                    .FirstOrDefaultAsync(t => t.InsuranceTariffId == tariffId);

                if (tariff == null)
                {
                    _log.Warning("🏥 MEDICAL: رکورد یافت نشد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("رکورد یافت نشد");
                }

                if (tariff.InsuranceType != InsuranceType.Supplementary)
                {
                    _log.Warning("🏥 MEDICAL: رکورد بیمه تکمیلی نیست - TariffId: {TariffId}, Type: {Type}. User: {UserName} (Id: {UserId})",
                        tariffId, tariff.InsuranceType, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("رکورد بیمه تکمیلی نیست");
                }

                // ذخیره مقادیر قبلی برای لاگ
                var oldPatientShare = tariff.PatientShare;
                var oldInsurerShare = tariff.InsurerShare;
                var oldCoveragePercent = tariff.SupplementaryCoveragePercent;

                // اصلاح منطق بیمه تکمیلی
                if (tariff.TariffPrice.HasValue && tariff.InsurerShare.HasValue)
                {
                    // سهم بیمار = کل مبلغ - سهم بیمه قبلی
                    tariff.PatientShare = tariff.TariffPrice.Value - tariff.InsurerShare.Value;
                    
                    // بیمه تکمیلی سهم بیمه ندارد
                    tariff.InsurerShare = 0;
                    
                    // درصد پوشش 100% از سهم بیمار
                    tariff.SupplementaryCoveragePercent = 100m;
                    
                    // بروزرسانی یادداشت
                    tariff.Notes = string.IsNullOrEmpty(tariff.Notes) 
                        ? "اصلاح شده: منطق صحیح بیمه تکمیلی"
                        : tariff.Notes + " [اصلاح شده: منطق صحیح بیمه تکمیلی]";
                    
                    // بروزرسانی تاریخ
                    tariff.UpdatedAt = DateTime.Now;
                    tariff.UpdatedByUserId = _currentUserService.UserId;
                }

                await _context.SaveChangesAsync();

                _log.Information("🏥 MEDICAL: اصلاح رکورد موفق - TariffId: {TariffId}, OldPatientShare: {OldPatientShare}, NewPatientShare: {NewPatientShare}, OldInsurerShare: {OldInsurerShare}, NewInsurerShare: {NewInsurerShare}. User: {UserName} (Id: {UserId})",
                    tariffId, oldPatientShare, tariff.PatientShare, oldInsurerShare, tariff.InsurerShare, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("رکورد با موفقیت اصلاح شد");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اصلاح رکورد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed($"خطا در اصلاح رکورد: {ex.Message}");
            }
        }

        /// <summary>
        /// اصلاح تمام رکوردهای بیمه تکمیلی با منطق اشتباه
        /// </summary>
        public async Task<ServiceResult<SupplementaryDataFixResult>> FixAllIncorrectRecordsAsync()
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع اصلاح تمام رکوردهای بیمه تکمیلی اشتباه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // یافتن رکوردهای اشتباه
                var incorrectRecords = await _context.InsuranceTariffs
                    .Where(t => t.InsuranceType == InsuranceType.Supplementary
                               && t.InsurerShare > 0  // رکوردهایی که سهم بیمه دارند (اشتباه)
                               && !t.IsDeleted
                               && t.IsActive)
                    .ToListAsync();

                var result = new SupplementaryDataFixResult
                {
                    TotalRecords = incorrectRecords.Count,
                    FixedRecords = 0,
                    SkippedRecords = 0,
                    Errors = 0
                };

                foreach (var record in incorrectRecords)
                {
                    try
                    {
                        // ذخیره مقادیر قبلی
                        var oldPatientShare = record.PatientShare;
                        var oldInsurerShare = record.InsurerShare;

                        // اصلاح منطق
                        if (record.TariffPrice.HasValue)
                        {
                            record.PatientShare = record.TariffPrice.Value - record.InsurerShare.Value;
                            record.InsurerShare = 0;
                            record.SupplementaryCoveragePercent = 100m;
                            record.Notes = string.IsNullOrEmpty(record.Notes) 
                                ? "اصلاح شده: منطق صحیح بیمه تکمیلی"
                                : record.Notes + " [اصلاح شده: منطق صحیح بیمه تکمیلی]";
                            record.UpdatedAt = DateTime.Now;
                            record.UpdatedByUserId = _currentUserService.UserId;

                            result.FixedRecords++;
                        }
                        else
                        {
                            result.SkippedRecords++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "🏥 MEDICAL: خطا در اصلاح رکورد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                            record.InsuranceTariffId, _currentUserService.UserName, _currentUserService.UserId);
                        result.Errors++;
                    }
                }

                await _context.SaveChangesAsync();

                _log.Information("🏥 MEDICAL: اصلاح تمام رکوردها تکمیل شد - Total: {Total}, Fixed: {Fixed}, Skipped: {Skipped}, Errors: {Errors}. User: {UserName} (Id: {UserId})",
                    result.TotalRecords, result.FixedRecords, result.SkippedRecords, result.Errors, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<SupplementaryDataFixResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اصلاح تمام رکوردها. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<SupplementaryDataFixResult>.Failed($"خطا در اصلاح رکوردها: {ex.Message}");
            }
        }

        /// <summary>
        /// بررسی وضعیت رکوردهای بیمه تکمیلی
        /// </summary>
        public async Task<ServiceResult<SupplementaryDataStatusResult>> GetDataStatusAsync()
        {
            try
            {
                _log.Debug("🏥 MEDICAL: بررسی وضعیت رکوردهای بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var totalRecords = await _context.InsuranceTariffs
                    .CountAsync(t => t.InsuranceType == InsuranceType.Supplementary && !t.IsDeleted);

                var correctRecords = await _context.InsuranceTariffs
                    .CountAsync(t => t.InsuranceType == InsuranceType.Supplementary 
                                   && !t.IsDeleted 
                                   && t.InsurerShare == 0);

                var incorrectRecords = await _context.InsuranceTariffs
                    .CountAsync(t => t.InsuranceType == InsuranceType.Supplementary 
                                   && !t.IsDeleted 
                                   && t.InsurerShare > 0);

                var result = new SupplementaryDataStatusResult
                {
                    TotalRecords = totalRecords,
                    CorrectRecords = correctRecords,
                    IncorrectRecords = incorrectRecords,
                    CorrectPercentage = totalRecords > 0 ? (decimal)correctRecords / totalRecords * 100 : 0
                };

                _log.Information("🏥 MEDICAL: وضعیت رکوردها - Total: {Total}, Correct: {Correct}, Incorrect: {Incorrect}, CorrectPercentage: {CorrectPercentage}%. User: {UserName} (Id: {UserId})",
                    result.TotalRecords, result.CorrectRecords, result.IncorrectRecords, result.CorrectPercentage, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<SupplementaryDataStatusResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در بررسی وضعیت رکوردها. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<SupplementaryDataStatusResult>.Failed($"خطا در بررسی وضعیت: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// نتیجه اصلاح داده‌های بیمه تکمیلی
    /// </summary>
    public class SupplementaryDataFixResult
    {
        public int TotalRecords { get; set; }
        public int FixedRecords { get; set; }
        public int SkippedRecords { get; set; }
        public int Errors { get; set; }
    }

    /// <summary>
    /// وضعیت داده‌های بیمه تکمیلی
    /// </summary>
    public class SupplementaryDataStatusResult
    {
        public int TotalRecords { get; set; }
        public int CorrectRecords { get; set; }
        public int IncorrectRecords { get; set; }
        public decimal CorrectPercentage { get; set; }
    }
}
