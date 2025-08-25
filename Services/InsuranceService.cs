using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Extensions;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس مدیریت بیمه‌ها - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ایجاد، ویرایش و حذف نرم بیمه‌ها با رعایت استانداردهای سیستم‌های پزشکی
    /// 2. مدیریت تعرفه‌های بیمه‌ای برای خدمات مختلف با قابلیت تعیین سهم بیمار و بیمه
    /// 3. پشتیبانی کامل از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط با رعایت استانداردهای سیستم‌های پزشکی
    /// 6. پشتیبانی از بیمه آزاد به عنوان پیش‌فرض برای بیماران بدون بیمه
    /// 7. پشتیبانی کامل از محیط‌های پزشکی ایرانی با تاریخ شمسی و اعداد فارسی در تمام لایه‌ها
    /// 8. عدم استفاده از AutoMapper برای کنترل کامل بر روی داده‌ها و امنیت بالا
    /// 9. پیاده‌سازی مکانیزم‌های بهینه‌سازی عملکرد برای سیستم‌های پزشکی پراستفاده
    /// 10. رعایت کامل استانداردهای امنیتی و حفظ حریم خصوصی اطلاعات پزشکی
    /// </summary>
    public class InsuranceService : IInsuranceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        private const string FreeInsuranceName = "بیمه آزاد";
        private const int MaxRetryAttempts = 3;
        private const int RetryDelayMilliseconds = 200;

        public InsuranceService(
            ApplicationDbContext context,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _log = logger;
            _currentUserService = currentUserService;
        }

        #region Insurance Management

        /// <summary>
        /// دریافت لیست بیمه‌های فعال
        /// </summary>
        public async Task<ServiceResult<List<InsuranceViewModel>>> GetActiveInsurancesAsync()
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست دریافت لیست بیمه‌های فعال. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var insurances = await _context.Insurances
                    .Where(i => i.IsActive && !i.IsDeleted)
                    .Select(i => new
                    {
                        Insurance = i,
                        TariffCount = i.Tariffs.Count(t => !t.IsDeleted)
                    })
                    .ToListAsync();

                // تبدیل دستی به InsuranceViewModel
                var result = new List<InsuranceViewModel>();
                foreach (var item in insurances)
                {
                    result.Add(new InsuranceViewModel
                    {
                        InsuranceId = item.Insurance.InsuranceId,
                        Name = item.Insurance.Name,
                        DefaultPatientShare = item.Insurance.DefaultPatientShare,
                        DefaultInsurerShare = item.Insurance.DefaultInsurerShare,
                        IsActive = item.Insurance.IsActive,
                        TariffCount = item.TariffCount
                    });
                }

                _log.Information(
                    "لیست بیمه‌های فعال با موفقیت دریافت شد. Count: {Count}, OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    result.Count, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در دریافت لیست بیمه‌های فعال. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    operationId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceViewModel>>.Failed("خطای سیستمی در دریافت لیست بیمه‌ها رخ داده است.");
            }
        }

        /// <summary>
        /// دریافت جزئیات بیمه
        /// </summary>
        public async Task<ServiceResult<InsuranceDetailsViewModel>> GetInsuranceDetailsAsync(int insuranceId)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست دریافت جزئیات بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // دریافت بیمه همراه با تمام روابط ضروری
                var insurance = await _context.Insurances
                    .Include(i => i.CreatedByUser)
                    .Include(i => i.UpdatedByUser)
                    .Include(i => i.Tariffs) // اضافه کردن رابطه تعرفه‌ها
                    .FirstOrDefaultAsync(i => i.InsuranceId == insuranceId && !i.IsDeleted);

                if (insurance == null)
                {
                    _log.Warning(
                        "بیمه با شناسه {InsuranceId} یافت نشد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                        insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<InsuranceDetailsViewModel>.Failed("بیمه مورد نظر یافت نشد.");
                }

                // راه‌حل اول: استفاده از تعداد تعرفه‌های بارگیری شده (مناسب برای تعداد کم تعرفه‌ها)
                int tariffCount = insurance.Tariffs.Count(t => !t.IsDeleted);

                // راه‌حل دوم: دریافت تعداد تعرفه‌ها از پایگاه داده (مناسب برای عملکرد بهتر)
                // int tariffCount = await _context.InsuranceTariffs
                //     .CountAsync(t => t.InsuranceId == insuranceId && !t.IsDeleted);

                var details = new InsuranceDetailsViewModel
                {
                    InsuranceId = insurance.InsuranceId,
                    Name = insurance.Name,
                    Description = insurance.Description,
                    DefaultPatientShare = insurance.DefaultPatientShare,
                    DefaultInsurerShare = insurance.DefaultInsurerShare,
                    IsActive = insurance.IsActive,
                    CreatedAt = insurance.CreatedAt,
                    UpdatedAt = insurance.UpdatedAt,
                    TariffCount = tariffCount
                };

                // اطلاعات کاربران
                details.CreatedByUser = insurance.CreatedByUser != null ?
                    $"{insurance.CreatedByUser.FirstName} {insurance.CreatedByUser.LastName}" : "سیستم";

                details.UpdatedByUser = insurance.UpdatedByUser != null ?
                    $"{insurance.UpdatedByUser.FirstName} {insurance.UpdatedByUser.LastName}" : null;

                // تاریخ‌های شمسی
                details.CreatedAtShamsi = DateTimeExtensions.ToPersianDateTime(details.CreatedAt);
                if (details.UpdatedAt.HasValue)
                    details.UpdatedAtShamsi = DateTimeExtensions.ToPersianDateTime(details.UpdatedAt.Value);

                _log.Information(
                    "جزئیات بیمه با شناسه {InsuranceId} با موفقیت دریافت شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsuranceDetailsViewModel>.Successful(details);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در دریافت جزئیات بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsuranceDetailsViewModel>.Failed("خطای سیستمی در دریافت جزئیات بیمه رخ داده است.");
            }
        }

        /// <summary>
        /// دریافت بیمه آزاد
        /// </summary>
        public async Task<ServiceResult<Insurance>> GetFreeInsuranceAsync()
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست دریافت بیمه آزاد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var freeInsurance = await _context.Insurances
                    .FirstOrDefaultAsync(i => i.Name == FreeInsuranceName && !i.IsDeleted);

                if (freeInsurance == null)
                {
                    _log.Error(
                        "بیمه آزاد در پایگاه داده یافت نشد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                        operationId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<Insurance>.Failed("بیمه آزاد در پایگاه داده یافت نشد.");
                }

                _log.Information(
                    "بیمه آزاد با شناسه {InsuranceId} با موفقیت دریافت شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    freeInsurance.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<Insurance>.Successful(freeInsurance);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در دریافت بیمه آزاد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<Insurance>.Failed("خطای سیستمی در دریافت بیمه آزاد رخ داده است.");
            }
        }

        /// <summary>
        /// ایجاد بیمه جدید
        /// </summary>
        public async Task<ServiceResult> CreateInsuranceAsync(CreateInsuranceViewModel model)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست ایجاد بیمه جدید. Name: {Name}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                model?.Name, operationId, _currentUserService.UserName, _currentUserService.UserId);

            // اعتبارسنجی ورودی‌ها
            if (model == null)
            {
                _log.Warning(
                    "درخواست ایجاد بیمه با مدل null. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("داده‌های ورودی نامعتبر است.");
            }

            // بررسی وجود بیمه با همین نام
            var existingInsurance = await _context.Insurances.AnyAsync(i => i.Name == model.Name && !i.IsDeleted);
            if (existingInsurance)
            {
                _log.Warning(
                    "درخواست ایجاد بیمه با نام تکراری. Name: {Name}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.Name, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("بیمه‌ای با این نام قبلاً ایجاد شده است.");
            }

            // بررسی نام بیمه آزاد
            if (model.Name == FreeInsuranceName)
            {
                _log.Warning(
                    "درخواست ایجاد بیمه با نام ممنوعه. Name: {Name}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.Name, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("نام 'بیمه آزاد' برای بیمه‌های عادی ممنوع است. این نام به بیمه پیش‌فرض اختصاص یافته است.");
            }

            return await ExecuteWithRetryAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var insurance = ConvertToInsuranceModel(model);
                        insurance.CreatedAt = DateTime.UtcNow;
                        insurance.CreatedByUserId = _currentUserService.UserId;
                        insurance.UpdatedAt = DateTime.UtcNow;
                        insurance.UpdatedByUserId = _currentUserService.UserId;

                        _context.Insurances.Add(insurance);
                        await _context.SaveChangesAsync();

                        transaction.Commit();

                        _log.Information(
                            "بیمه جدید با نام {Name} و شناسه {InsuranceId} با موفقیت ایجاد شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            model.Name, insurance.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                        return ServiceResult.Successful("بیمه با موفقیت ایجاد شد.");
                    }
                    catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
                    {
                        transaction.Rollback();
                        _log.Warning(
                            "درخواست ایجاد بیمه با نام تکراری به دلیل رقابت همزمان. Name: {Name}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            model.Name, operationId, _currentUserService.UserName, _currentUserService.UserId);
                        return ServiceResult.Failed("بیمه‌ای با این نام قبلاً ایجاد شده است.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _log.Error(
                            ex,
                            "خطا در ایجاد بیمه با نام {Name}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            model.Name, operationId, _currentUserService.UserName, _currentUserService.UserId);
                        throw;
                    }
                }
            }, operationId);
        }

        /// <summary>
        /// ویرایش بیمه
        /// </summary>
        public async Task<ServiceResult> UpdateInsuranceAsync(EditInsuranceViewModel model)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست ویرایش بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                model?.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            // اعتبارسنجی ورودی‌ها
            if (model == null)
            {
                _log.Warning(
                    "درخواست ویرایش بیمه با مدل null. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("داده‌های ورودی نامعتبر است.");
            }

            if (!model.ValidateShares())
            {
                _log.Warning(
                    "درخواست ویرایش بیمه با سهم‌های نامعتبر. InsuranceId: {InsuranceId}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceId, model.DefaultPatientShare, model.DefaultInsurerShare, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("مجموع سهم بیمار و بیمه باید برابر با 100 درصد باشد.");
            }

            // بررسی وجود بیمه
            var insurance = await _context.Insurances
                .FirstOrDefaultAsync(i => i.InsuranceId == model.InsuranceId && !i.IsDeleted);

            if (insurance == null)
            {
                _log.Warning(
                    "درخواست ویرایش بیمه ناموجود. InsuranceId: {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("بیمه مورد نظر یافت نشد یا حذف شده است.");
            }

            // بررسی نام بیمه آزاد
            if (model.Name == FreeInsuranceName && insurance.Name != FreeInsuranceName)
            {
                _log.Warning(
                    "درخواست تغییر نام بیمه به نام ممنوعه. InsuranceId: {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("نام 'بیمه آزاد' برای بیمه‌های عادی ممنوع است. این نام به بیمه پیش‌فرض اختصاص یافته است.");
            }

            // بررسی تکراری بودن نام
            var existingInsurance = await _context.Insurances
                .AnyAsync(i => i.Name == model.Name && i.InsuranceId != model.InsuranceId && !i.IsDeleted);

            if (existingInsurance)
            {
                _log.Warning(
                    "درخواست ویرایش بیمه با نام تکراری. InsuranceId: {InsuranceId}, Name: {Name}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceId, model.Name, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("بیمه‌ای با این نام قبلاً ایجاد شده است.");
            }

            return await ExecuteWithRetryAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // بررسی مجدد وجود بیمه برای جلوگیری از تداخل همزمانی
                        var insuranceToUpdate = await _context.Insurances
                            .FirstOrDefaultAsync(i => i.InsuranceId == model.InsuranceId && !i.IsDeleted);

                        if (insuranceToUpdate == null)
                        {
                            _log.Warning(
                                "درخواست ویرایش بیمه ناموجود پس از تأیید اولیه. InsuranceId: {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                                model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                            return ServiceResult.Failed("بیمه مورد نظر یافت نشد یا حذف شده است.");
                        }

                        // اعمال تغییرات
                        UpdateInsuranceModel(insuranceToUpdate, model);
                        insuranceToUpdate.UpdatedAt = DateTime.UtcNow;
                        insuranceToUpdate.UpdatedByUserId = _currentUserService.UserId;

                        await _context.SaveChangesAsync();
                        transaction.Commit();

                        _log.Information(
                            "بیمه با شناسه {InsuranceId} با موفقیت ویرایش شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                        return ServiceResult.Successful("بیمه با موفقیت ویرایش شد.");
                    }
                    catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
                    {
                        transaction.Rollback();
                        _log.Warning(
                            "درخواست ویرایش بیمه با نام تکراری به دلیل رقابت همزمان. InsuranceId: {InsuranceId}, Name: {Name}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            model.InsuranceId, model.Name, operationId, _currentUserService.UserName, _currentUserService.UserId);
                        return ServiceResult.Failed("بیمه‌ای با این نام قبلاً ایجاد شده است.");
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        transaction.Rollback();
                        _log.Warning(
                            ex,
                            "تداخل همزمانی هنگام ویرایش بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                        return ServiceResult.Failed("داده‌ها توسط کاربر دیگری تغییر کرده‌اند. لطفاً صفحه را مجدداً بارگیری کنید.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _log.Error(
                            ex,
                            "خطا در ویرایش بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                        throw;
                    }
                }
            }, operationId);
        }

        /// <summary>
        /// حذف نرم بیمه
        /// </summary>
        public async Task<ServiceResult> DeleteInsuranceAsync(int insuranceId)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست حذف نرم بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            // اعتبارسنجی ورودی
            if (insuranceId <= 0)
            {
                _log.Warning(
                    "درخواست حذف بیمه با شناسه نامعتبر. InsuranceId: {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("شناسه بیمه معتبر نیست.");
            }

            // بررسی وجود بیمه
            var insurance = await _context.Insurances
                .Include(i => i.Patients)
                .Include(i => i.Receptions)
                .FirstOrDefaultAsync(i => i.InsuranceId == insuranceId && !i.IsDeleted);

            if (insurance == null)
            {
                _log.Warning(
                    "درخواست حذف بیمه ناموجود. InsuranceId: {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("بیمه مورد نظر یافت نشد یا حذف شده است.");
            }

            // بررسی استفاده از بیمه آزاد
            if (insurance.Name == FreeInsuranceName)
            {
                _log.Warning(
                    "درخواست حذف بیمه آزاد. InsuranceId: {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("بیمه آزاد قابل حذف نیست. این بیمه به عنوان بیمه پیش‌فرض برای بیماران بدون بیمه استفاده می‌شود.");
            }

            // بررسی وجود بیماران تحت پوشش
            if (insurance.Patients.Any(p => !p.IsDeleted))
            {
                _log.Warning(
                    "درخواست حذف بیمه با بیماران تحت پوشش. InsuranceId: {InsuranceId}, PatientCount: {PatientCount}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, insurance.Patients.Count(p => !p.IsDeleted), operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("امکان حذف بیمه به دلیل وجود بیماران تحت پوشش وجود ندارد.");
            }

            // بررسی وجود پذیرش‌ها
            if (insurance.Receptions.Any(r => !r.IsDeleted))
            {
                _log.Warning(
                    "درخواست حذف بیمه با پذیرش‌های مرتبط. InsuranceId: {InsuranceId}, ReceptionCount: {ReceptionCount}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, insurance.Receptions.Count(r => !r.IsDeleted), operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("امکان حذف بیمه به دلیل وجود پذیرش‌های مرتبط وجود ندارد.");
            }

            return await ExecuteWithRetryAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // بررسی مجدد وجود بیمه برای جلوگیری از تداخل همزمانی
                        var insuranceToDelete = await _context.Insurances
                            .FirstOrDefaultAsync(i => i.InsuranceId == insuranceId && !i.IsDeleted);

                        if (insuranceToDelete == null)
                        {
                            _log.Warning(
                                "درخواست حذف بیمه ناموجود پس از تأیید اولیه. InsuranceId: {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                                insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                            return ServiceResult.Failed("بیمه مورد نظر یافت نشد یا حذف شده است.");
                        }

                        // انجام حذف نرم
                        insuranceToDelete.IsDeleted = true;
                        insuranceToDelete.DeletedAt = DateTime.UtcNow;
                        insuranceToDelete.DeletedByUserId = _currentUserService.UserId;

                        await _context.SaveChangesAsync();
                        transaction.Commit();

                        _log.Information(
                            "حذف نرم بیمه با شناسه {InsuranceId} با موفقیت انجام شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                        return ServiceResult.Successful("بیمه با موفقیت حذف شد.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _log.Error(
                            ex,
                            "خطا در حذف نرم بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                        throw;
                    }
                }
            }, operationId);
        }

        #endregion

        #region Insurance Tariffs Management

        /// <summary>
        /// دریافت تعرفه‌های بیمه
        /// </summary>
        public async Task<ServiceResult<List<InsuranceTariffViewModel>>> GetInsuranceTariffsAsync(int insuranceId)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست دریافت تعرفه‌های بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var tariffs = await _context.InsuranceTariffs
                    .Where(t => t.InsuranceId == insuranceId && !t.IsDeleted)
                    .Select(t => new
                    {
                        Tariff = t,
                        ServiceTitle = t.Service.Title,
                        CreatedByUser = t.CreatedByUser != null ?
                            new { t.CreatedByUser.FirstName, t.CreatedByUser.LastName } : null
                    })
                    .OrderBy(t => t.ServiceTitle)
                    .ToListAsync();

                // تبدیل دستی به InsuranceTariffViewModel
                var result = new List<InsuranceTariffViewModel>();
                foreach (var item in tariffs)
                {
                    result.Add(new InsuranceTariffViewModel
                    {
                        InsuranceTariffId = item.Tariff.InsuranceTariffId,
                        ServiceTitle = item.ServiceTitle,
                        TariffPrice = item.Tariff.TariffPrice,
                        PatientShare = item.Tariff.PatientShare,
                        InsurerShare = item.Tariff.InsurerShare,
                        CreatedAt = item.Tariff.CreatedAt,
                        CreatedByUser = item.CreatedByUser != null ?
                            $"{item.CreatedByUser.FirstName} {item.CreatedByUser.LastName}" : "سیستم",
                        UpdatedAt = item.Tariff.UpdatedAt,
                        UpdatedByUser = item.Tariff.UpdatedByUser != null ?
                            $"{item.Tariff.UpdatedByUser.FirstName} {item.Tariff.UpdatedByUser.LastName}" : null
                    });
                }

                _log.Information(
                    "تعرفه‌های بیمه با شناسه {InsuranceId} با موفقیت دریافت شد. Count: {Count}, OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, result.Count, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceTariffViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در دریافت تعرفه‌های بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceTariffViewModel>>.Failed("خطای سیستمی در دریافت تعرفه‌ها رخ داده است.");
            }
        }

        /// <summary>
        /// دریافت اطلاعات تعرفه برای ویرایش
        /// </summary>
        public async Task<ServiceResult<EditInsuranceTariffViewModel>> GetInsuranceTariffForEditAsync(int tariffId)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست جزئیات برای تعرفه بیمه با شناسه {TariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                tariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var tariff = await _context.InsuranceTariffs
                    .Where(t => t.InsuranceTariffId == tariffId && !t.IsDeleted)
                    .Select(t => new
                    {
                        Tariff = t,
                        ServiceTitle = t.Service.Title
                    })
                    .FirstOrDefaultAsync();

                if (tariff == null)
                {
                    _log.Warning(
                        "درخواست جزئیات برای تعرفه بیمه غیرموجود. TariffId: {TariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                        tariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<EditInsuranceTariffViewModel>.Failed("تعرفه مورد نظر یافت نشد.");
                }

                var result = new EditInsuranceTariffViewModel
                {
                    InsuranceTariffId = tariff.Tariff.InsuranceTariffId,
                    InsuranceId = tariff.Tariff.InsuranceId,
                    ServiceId = tariff.Tariff.ServiceId,
                    TariffPrice = tariff.Tariff.TariffPrice,
                    PatientShare = tariff.Tariff.PatientShare,
                    InsurerShare = tariff.Tariff.InsurerShare
                };

                _log.Information(
                    "جزئیات تعرفه بیمه با شناسه {TariffId} با موفقیت دریافت شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    tariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<EditInsuranceTariffViewModel>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در دریافت جزئیات تعرفه بیمه با شناسه {TariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    tariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<EditInsuranceTariffViewModel>.Failed("خطای سیستمی در دریافت جزئیات تعرفه رخ داده است.");
            }
        }

        /// <summary>
        /// ایجاد تعرفه بیمه
        /// </summary>
        public async Task<ServiceResult> CreateInsuranceTariffAsync(CreateInsuranceTariffViewModel model)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست ایجاد تعرفه بیمه. InsuranceId: {InsuranceId}, ServiceId: {ServiceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                model?.InsuranceId, model?.ServiceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            // اعتبارسنجی ورودی‌ها
            if (model == null)
            {
                _log.Warning(
                    "درخواست ایجاد تعرفه بیمه با مدل null. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("داده‌های ورودی نامعتبر است.");
            }

            if (!model.ValidateShares())
            {
                _log.Warning(
                    "درخواست ایجاد تعرفه بیمه با سهم‌های نامعتبر. InsuranceId: {InsuranceId}, ServiceId: {ServiceId}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceId, model.ServiceId, model.PatientShare, model.InsurerShare, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("مجموع سهم بیمار و بیمه باید برابر با 100 درصد باشد.");
            }

            // بررسی وجود بیمه
            var insuranceExists = await _context.Insurances
                .AnyAsync(i => i.InsuranceId == model.InsuranceId && !i.IsDeleted);

            if (!insuranceExists)
            {
                _log.Warning(
                    "درخواست ایجاد تعرفه بیمه برای بیمه ناموجود. InsuranceId: {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("بیمه مورد نظر یافت نشد یا حذف شده است.");
            }

            // بررسی وجود خدمت
            var serviceExists = await _context.Services
                .AnyAsync(s => s.ServiceId == model.ServiceId && !s.IsDeleted);

            if (!serviceExists)
            {
                _log.Warning(
                    "درخواست ایجاد تعرفه بیمه برای خدمت ناموجود. ServiceId: {ServiceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.ServiceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("خدمت مورد نظر یافت نشد یا حذف شده است.");
            }

            // بررسی تکراری بودن تعرفه
            var tariffExists = await _context.InsuranceTariffs
                .AnyAsync(t => t.InsuranceId == model.InsuranceId &&
                               t.ServiceId == model.ServiceId &&
                               !t.IsDeleted);

            if (tariffExists)
            {
                _log.Warning(
                    "درخواست ایجاد تعرفه بیمه تکراری. InsuranceId: {InsuranceId}, ServiceId: {ServiceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceId, model.ServiceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("تعرفه برای این بیمه و خدمت قبلاً ایجاد شده است.");
            }

            return await ExecuteWithRetryAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // ایجاد تعرفه
                        var tariff = ConvertToInsuranceTariffModel(model);
                        tariff.CreatedAt = DateTime.UtcNow;
                        tariff.CreatedByUserId = _currentUserService.UserId;

                        _context.InsuranceTariffs.Add(tariff);
                        await _context.SaveChangesAsync();

                        transaction.Commit();

                        _log.Information(
                            "تعرفه بیمه با شناسه {InsuranceTariffId} با موفقیت ایجاد شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            tariff.InsuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                        return ServiceResult.Successful("تعرفه بیمه با موفقیت ایجاد شد.");
                    }
                    catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
                    {
                        transaction.Rollback();
                        _log.Warning(
                            "درخواست ایجاد تعرفه بیمه تکراری به دلیل رقابت همزمان. InsuranceId: {InsuranceId}, ServiceId: {ServiceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            model.InsuranceId, model.ServiceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                        return ServiceResult.Failed("تعرفه برای این بیمه و خدمت قبلاً ایجاد شده است.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _log.Error(
                            ex,
                            "خطا در ایجاد تعرفه بیمه. InsuranceId: {InsuranceId}, ServiceId: {ServiceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            model.InsuranceId, model.ServiceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                        throw;
                    }
                }
            }, operationId);
        }

        /// <summary>
        /// ویرایش تعرفه بیمه
        /// </summary>
        public async Task<ServiceResult> UpdateInsuranceTariffAsync(EditInsuranceTariffViewModel model)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست ویرایش تعرفه بیمه. InsuranceTariffId: {InsuranceTariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                model?.InsuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            // اعتبارسنجی ورودی‌ها
            if (model == null)
            {
                _log.Warning(
                    "درخواست ویرایش تعرفه بیمه با مدل null. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("داده‌های ورودی نامعتبر است.");
            }

            if (!model.ValidateShares())
            {
                _log.Warning(
                    "درخواست ویرایش تعرفه بیمه با سهم‌های نامعتبر. InsuranceTariffId: {InsuranceTariffId}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceTariffId, model.PatientShare, model.InsurerShare, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("مجموع سهم بیمار و بیمه باید برابر با 100 درصد باشد.");
            }

            // بررسی وجود بیمه
            var insuranceExists = await _context.Insurances
                .AnyAsync(i => i.InsuranceId == model.InsuranceId && !i.IsDeleted);

            if (!insuranceExists)
            {
                _log.Warning(
                    "درخواست ویرایش تعرفه بیمه برای بیمه ناموجود. InsuranceId: {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("بیمه مورد نظر یافت نشد یا حذف شده است.");
            }

            // بررسی وجود خدمت
            var serviceExists = await _context.Services
                .AnyAsync(s => s.ServiceId == model.ServiceId && !s.IsDeleted);

            if (!serviceExists)
            {
                _log.Warning(
                    "درخواست ویرایش تعرفه بیمه برای خدمت ناموجود. ServiceId: {ServiceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.ServiceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("خدمت مورد نظر یافت نشد یا حذف شده است.");
            }

            // بررسی تکراری بودن تعرفه برای همان بیمه و خدمت
            var tariffExists = await _context.InsuranceTariffs
                .AnyAsync(t => t.InsuranceId == model.InsuranceId &&
                               t.ServiceId == model.ServiceId &&
                               t.InsuranceTariffId != model.InsuranceTariffId &&
                               !t.IsDeleted);

            if (tariffExists)
            {
                _log.Warning(
                    "درخواست ویرایش تعرفه بیمه به تعرفه تکراری. InsuranceTariffId: {InsuranceTariffId}, InsuranceId: {InsuranceId}, ServiceId: {ServiceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceTariffId, model.InsuranceId, model.ServiceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("تعرفه برای این بیمه و خدمت قبلاً ایجاد شده است.");
            }

            return await ExecuteWithRetryAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // بررسی وجود تعرفه
                        var tariff = await _context.InsuranceTariffs
                            .FirstOrDefaultAsync(t => t.InsuranceTariffId == model.InsuranceTariffId && !t.IsDeleted);

                        if (tariff == null)
                        {
                            _log.Warning(
                                "درخواست ویرایش تعرفه بیمه ناموجود. InsuranceTariffId: {InsuranceTariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                                model.InsuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                            return ServiceResult.Failed("تعرفه مورد نظر یافت نشد یا حذف شده است.");
                        }

                        // اعمال تغییرات
                        UpdateInsuranceTariffModel(tariff, model);
                        tariff.UpdatedAt = DateTime.UtcNow;
                        tariff.UpdatedByUserId = _currentUserService.UserId;

                        await _context.SaveChangesAsync();
                        transaction.Commit();

                        _log.Information(
                            "تعرفه بیمه با شناسه {InsuranceTariffId} با موفقیت ویرایش شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            model.InsuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                        return ServiceResult.Successful("تعرفه بیمه با موفقیت ویرایش شد.");
                    }
                    catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
                    {
                        transaction.Rollback();
                        _log.Warning(
                            "درخواست ویرایش تعرفه بیمه به تعرفه تکراری به دلیل رقابت همزمان. InsuranceTariffId: {InsuranceTariffId}, InsuranceId: {InsuranceId}, ServiceId: {ServiceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            model.InsuranceTariffId, model.InsuranceId, model.ServiceId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                        return ServiceResult.Failed("تعرفه برای این بیمه و خدمت قبلاً ایجاد شده است.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _log.Error(
                            ex,
                            "خطا در ویرایش تعرفه بیمه با شناسه {InsuranceTariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            model.InsuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                        throw;
                    }
                }
            }, operationId);
        }

        /// <summary>
        /// حذف تعرفه بیمه
        /// </summary>
        public async Task<ServiceResult> DeleteInsuranceTariffAsync(int insuranceTariffId)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست حذف تعرفه بیمه با شناسه {InsuranceTariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                insuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            // اعتبارسنجی ورودی
            if (insuranceTariffId <= 0)
            {
                _log.Warning(
                    "درخواست حذف تعرفه بیمه با شناسه نامعتبر. InsuranceTariffId: {InsuranceTariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("شناسه تعرفه معتبر نیست.");
            }

            // بررسی وجود تعرفه
            var tariff = await _context.InsuranceTariffs
                .FirstOrDefaultAsync(t => t.InsuranceTariffId == insuranceTariffId && !t.IsDeleted);

            if (tariff == null)
            {
                _log.Warning(
                    "درخواست حذف تعرفه بیمه ناموجود. InsuranceTariffId: {InsuranceTariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("تعرفه مورد نظر یافت نشد یا حذف شده است.");
            }

            return await ExecuteWithRetryAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // بررسی مجدد وجود تعرفه برای جلوگیری از تداخل همزمانی
                        var tariffToDelete = await _context.InsuranceTariffs
                            .FirstOrDefaultAsync(t => t.InsuranceTariffId == insuranceTariffId && !t.IsDeleted);

                        if (tariffToDelete == null)
                        {
                            _log.Warning(
                                "درخواست حذف تعرفه بیمه ناموجود پس از تأیید اولیه. InsuranceTariffId: {InsuranceTariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                                insuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                            return ServiceResult.Failed("تعرفه مورد نظر یافت نشد یا حذف شده است.");
                        }

                        // انجام حذف نرم
                        tariffToDelete.IsDeleted = true;
                        tariffToDelete.DeletedAt = DateTime.UtcNow;
                        tariffToDelete.DeletedByUserId = _currentUserService.UserId;

                        await _context.SaveChangesAsync();
                        transaction.Commit();

                        _log.Information(
                            "حذف نرم تعرفه بیمه با شناسه {InsuranceTariffId} با موفقیت انجام شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            insuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                        return ServiceResult.Successful("تعرفه بیمه با موفقیت حذف شد.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _log.Error(
                            ex,
                            "خطا در حذف نرم تعرفه بیمه با شناسه {InsuranceTariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            insuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);
                        throw;
                    }
                }
            }, operationId);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// تبدیل مدل بیمه به InsuranceViewModel
        /// </summary>
        private InsuranceViewModel ConvertToInsuranceViewModel(Insurance insurance)
        {
            if (insurance == null)
                return null;

            return new InsuranceViewModel
            {
                InsuranceId = insurance.InsuranceId,
                Name = insurance.Name,
                DefaultPatientShare = insurance.DefaultPatientShare,
                DefaultInsurerShare = insurance.DefaultInsurerShare,
                IsActive = insurance.IsActive,
                TariffCount = insurance.Tariffs.Count(t => !t.IsDeleted)
            };
        }

        /// <summary>
        /// تبدیل CreateInsuranceViewModel به مدل بیمه
        /// </summary>
        private Insurance ConvertToInsuranceModel(CreateInsuranceViewModel model)
        {
            if (model == null)
                return null;

            return new Insurance
            {
                Name = model.Name,
                Description = model.Description,
                DefaultPatientShare = model.DefaultPatientShare,
                DefaultInsurerShare = model.DefaultInsurerShare,
                IsActive = model.IsActive,
                IsDeleted = false
            };
        }

        /// <summary>
        /// تبدیل EditInsuranceViewModel به مدل بیمه
        /// </summary>
        private void UpdateInsuranceModel(Insurance insurance, EditInsuranceViewModel model)
        {
            if (insurance == null || model == null)
                return;

            insurance.Name = model.Name;
            insurance.Description = model.Description;
            insurance.DefaultPatientShare = model.DefaultPatientShare;
            insurance.DefaultInsurerShare = model.DefaultInsurerShare;
            insurance.IsActive = model.IsActive;
        }

        /// <summary>
        /// تبدیل CreateInsuranceTariffViewModel به مدل تعرفه بیمه
        /// </summary>
        private InsuranceTariff ConvertToInsuranceTariffModel(CreateInsuranceTariffViewModel model)
        {
            if (model == null)
                return null;

            return new InsuranceTariff
            {
                InsuranceId = model.InsuranceId,
                ServiceId = model.ServiceId,
                TariffPrice = model.TariffPrice,
                PatientShare = model.PatientShare,
                InsurerShare = model.InsurerShare,
                IsDeleted = false
            };
        }

        /// <summary>
        /// تبدیل EditInsuranceTariffViewModel به مدل تعرفه بیمه
        /// </summary>
        private void UpdateInsuranceTariffModel(InsuranceTariff tariff, EditInsuranceTariffViewModel model)
        {
            if (tariff == null || model == null)
                return;

            tariff.InsuranceId = model.InsuranceId;
            tariff.ServiceId = model.ServiceId;
            tariff.TariffPrice = model.TariffPrice;
            tariff.PatientShare = model.PatientShare;
            tariff.InsurerShare = model.InsurerShare;
        }

        /// <summary>
        /// اجرای عملیات با مکانیزم تلاش مجدد برای مقابله با تداخل همزمانی
        /// </summary>
        private async Task<ServiceResult> ExecuteWithRetryAsync(Func<Task<ServiceResult>> operation, string operationId)
        {
            int attempt = 0;
            while (attempt < MaxRetryAttempts)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    attempt++;
                    if (attempt >= MaxRetryAttempts)
                    {
                        _log.Error(
                            ex,
                            "تلاش نهایی برای اجرای عملیات شکست خورد. OperationId: {OperationId}, Attempt: {Attempt}",
                            operationId, attempt);
                        throw;
                    }

                    _log.Warning(
                        ex,
                        "تلاش {Attempt} برای اجرای عملیات شکست خورد. OperationId: {OperationId}. در {Delay} میلی‌ثانیه مجدداً تلاش خواهد شد.",
                        attempt, operationId, RetryDelayMilliseconds);

                    await Task.Delay(RetryDelayMilliseconds * attempt);
                }
            }

            return ServiceResult.Failed("تلاش‌های مجدد برای اجرای عملیات شکست خورد.");
        }

        #endregion
    }
}