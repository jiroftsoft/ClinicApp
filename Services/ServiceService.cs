using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Extensions;
using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Services;

/// <summary>
/// سرویس مدیریت خدمات پزشکی با رعایت کامل استانداردهای سیستم‌های پزشکی و امنیت اطلاعات
/// این سرویس تمام عملیات مربوط به خدمات پزشکی را پشتیبانی می‌کند
/// 
/// ویژگی‌های کلیدی:
/// 1. رعایت کامل اصول Soft Delete برای حفظ اطلاعات پزشکی (مطابق استانداردهای قانونی ایران)
/// 2. پیاده‌سازی سیستم ردیابی کامل (Audit Trail) با ذخیره اطلاعات کاربر انجام‌دهنده عملیات
/// 3. استفاده از زمان UTC برای تمام تاریخ‌ها به منظور رعایت استانداردهای بین‌المللی
/// 4. مدیریت تراکنش‌های پایگاه داده برای اطمینان از یکپارچگی داده‌ها
/// 5. اعمال قوانین کسب‌وکار پزشکی در تمام سطوح
/// 6. پشتیبانی کامل از Dependency Injection برای افزایش قابلیت تست و نگهداری
/// 7. مدیریت خطاها با پیام‌های کاربرپسند و لاگ‌گیری حرفه‌ای
/// 8. پشتیبانی کامل از محیط‌های ایرانی با تبدیل تاریخ‌ها به شمسی
/// 9. ارائه امکانات پیشرفته برای سیستم‌های پزشکی ایرانی
/// </summary>
public class ServiceService : IServiceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger _log;
    private readonly ICurrentUserService _currentUserService;
    private readonly IServiceCalculationService _serviceCalculationService;

    public ServiceService(
        ApplicationDbContext context,
        ILogger logger,
        ICurrentUserService currentUserService,
        IServiceCalculationService serviceCalculationService)
    {
        _context = context;
        _log = logger;
        _currentUserService = currentUserService;
        _serviceCalculationService = serviceCalculationService;
    }

    /// <summary>
    /// ایجاد یک خدمات جدید با رعایت تمام استانداردهای امنیتی و پزشکی
    /// </summary>
    /// <param name="model">مدل اطلاعات خدمات جدید</param>
    /// <returns>نتیجه عملیات با شناسه خدمات ایجاد شده</returns>
    public async Task<ServiceResult<int>> CreateServiceAsync(ServiceCreateEditViewModel model)
    {
        _log.Information(
            "درخواست ایجاد خدمات جدید با نام {Title}. User: {UserName} (Id: {UserId})",
            model.Title,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی‌ها
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                _log.Warning(
                    "درخواست ایجاد خدمات با عنوان خالی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("عنوان خدمات الزامی است.");
            }

            if (model.Title.Length > 250)
            {
                _log.Warning(
                    "درخواست ایجاد خدمات با عنوان طولانی. Length: {Length}. User: {UserName} (Id: {UserId})",
                    model.Title.Length,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("عنوان خدمات نمی‌تواند بیشتر از 250 کاراکتر باشد.");
            }

            if (string.IsNullOrWhiteSpace(model.ServiceCode))
            {
                _log.Warning(
                    "درخواست ایجاد خدمات با کد خالی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("کد خدمات الزامی است.");
            }

            if (model.ServiceCode.Length > 50)
            {
                _log.Warning(
                    "درخواست ایجاد خدمات با کد طولانی. Length: {Length}. User: {UserName} (Id: {UserId})",
                    model.ServiceCode.Length,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("کد خدمات نمی‌تواند بیشتر از 50 کاراکتر باشد.");
            }

            if (!RegexHelper.IsValidServiceCode(model.ServiceCode))
            {
                _log.Warning(
                    "درخواست ایجاد خدمات با کد نامعتبر. ServiceCode: {ServiceCode}. User: {UserName} (Id: {UserId})",
                    model.ServiceCode,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("کد خدمات فقط می‌تواند شامل حروف، اعداد و زیرخط باشد.");
            }

            if (model.Price < 0)
            {
                _log.Warning(
                    "درخواست ایجاد خدمات با قیمت منفی. Price: {Price}. User: {UserName} (Id: {UserId})",
                    model.Price,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("قیمت نمی‌تواند منفی باشد.");
            }

            if (model.ServiceCategoryId <= 0)
            {
                _log.Warning(
                    "درخواست ایجاد خدمات با دسته‌بندی نامعتبر. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    model.ServiceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("دسته‌بندی خدمات انتخاب شده معتبر نیست.");
            }

            // بررسی وجود کد خدمات تکراری
            if (await IsDuplicateServiceCodeAsync(model.ServiceCode))
            {
                _log.Warning(
                    "درخواست ایجاد خدمات با کد تکراری. ServiceCode: {ServiceCode}. User: {UserName} (Id: {UserId})",
                    model.ServiceCode,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("خدماتی با این کد از قبل وجود دارد.");
            }

            // بررسی وجود دسته‌بندی خدمات معتبر
            if (!await IsServiceCategoryValidAsync(model.ServiceCategoryId))
            {
                _log.Warning(
                    "درخواست ایجاد خدمات با دسته‌بندی نامعتبر. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    model.ServiceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("دسته‌بندی خدمات انتخاب شده معتبر نیست یا حذف شده است.");
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _log.Information(
                        "شروع تراکنش ایجاد خدمات جدید. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    // ساخت نمونه جدید خدمات
                    var service = new Service
                    {
                        Title = model.Title,
                        ServiceCode = model.ServiceCode,
                        Price = model.Price, // قیمت اولیه از model
                        Description = model.Description,
                        ServiceCategoryId = model.ServiceCategoryId,
                        IsDeleted = false,
                        CreatedAt = _currentUserService.UtcNow,
                        CreatedByUserId = _currentUserService.UserId
                    };

                    _context.Services.Add(service);
                    await _context.SaveChangesAsync();

                    // محاسبه قیمت نهایی بر اساس ServiceComponents (اگر موجود باشد)
                    // ابتدا ServiceComponents را بارگیری کن
                    await _context.Entry(service)
                        .Collection(s => s.ServiceComponents)
                        .LoadAsync();

                    if (service.ServiceComponents != null && service.ServiceComponents.Any())
                    {
                        var calculatedPrice = _serviceCalculationService.CalculateServicePrice(service);
                        if (calculatedPrice > 0 && calculatedPrice != service.Price)
                        {
                            service.Price = calculatedPrice;
                            service.UpdatedAt = _currentUserService.UtcNow;
                            service.UpdatedByUserId = _currentUserService.UserId;
                            
                            _log.Information(
                                "قیمت خدمت محاسبه و به‌روزرسانی شد. ServiceId: {ServiceId}, OriginalPrice: {OriginalPrice}, CalculatedPrice: {CalculatedPrice}. User: {UserName} (Id: {UserId})",
                                service.ServiceId, model.Price, calculatedPrice, _currentUserService.UserName, _currentUserService.UserId);
                            
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        _log.Information(
                            "ServiceComponents موجود نیست - قیمت اولیه حفظ شد. ServiceId: {ServiceId}, Price: {Price}. User: {UserName} (Id: {UserId})",
                            service.ServiceId, service.Price, _currentUserService.UserName, _currentUserService.UserId);
                    }

                    transaction.Commit();

                    _log.Information(
                        "خدمات جدید با موفقیت ایجاد شد. ServiceId: {ServiceId}, Title: {Title}, ServiceCode: {ServiceCode}, CreatedBy: {CreatedBy}",
                        service.ServiceId,
                        service.Title,
                        service.ServiceCode,
                        _currentUserService.UserId);

                    return ServiceResult<int>.Successful(
                        service.ServiceId,
                        "خدمات با موفقیت ایجاد شد.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در ایجاد خدمات جدید. Title: {Title}, ServiceCode: {ServiceCode}. User: {UserName} (Id: {UserId})",
                model.Title,
                model.ServiceCode,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<int>.Failed("خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
        }
    }

    /// <summary>
    /// به‌روزرسانی خدمات موجود با رعایت تمام استانداردهای امنیتی و پزشکی
    /// </summary>
    /// <param name="model">مدل اطلاعات به‌روزرسانی شده خدمات</param>
    /// <returns>نتیجه عملیات به‌روزرسانی</returns>
    public async Task<ServiceResult> UpdateServiceAsync(ServiceCreateEditViewModel model)
    {
        _log.Information(
            "درخواست به‌روزرسانی خدمات با شناسه {ServiceId}. User: {UserName} (Id: {UserId})",
            model.ServiceId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی‌ها
            if (model.ServiceId <= 0)
            {
                _log.Warning(
                    "درخواست ویرایش خدمات با شناسه نامعتبر. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.ServiceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("شناسه خدمات معتبر نیست.");
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                _log.Warning(
                    "درخواست ویرایش خدمات با عنوان خالی. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.ServiceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("عنوان خدمات الزامی است.");
            }

            if (model.Title.Length > 250)
            {
                _log.Warning(
                    "درخواست ویرایش خدمات با عنوان طولانی. Length: {Length}. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.Title.Length,
                    model.ServiceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("عنوان خدمات نمی‌تواند بیشتر از 250 کاراکتر باشد.");
            }

            if (string.IsNullOrWhiteSpace(model.ServiceCode))
            {
                _log.Warning(
                    "درخواست ویرایش خدمات با کد خالی. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.ServiceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("کد خدمات الزامی است.");
            }

            if (model.ServiceCode.Length > 50)
            {
                _log.Warning(
                    "درخواست ویرایش خدمات با کد طولانی. Length: {Length}. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.ServiceCode.Length,
                    model.ServiceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("کد خدمات نمی‌تواند بیشتر از 50 کاراکتر باشد.");
            }

            if (!RegexHelper.IsValidServiceCode(model.ServiceCode))
            {
                _log.Warning(
                    "درخواست ویرایش خدمات با کد نامعتبر. ServiceCode: {ServiceCode}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.ServiceCode,
                    model.ServiceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("کد خدمات فقط می‌تواند شامل حروف، اعداد و زیرخط باشد.");
            }

            if (model.Price < 0)
            {
                _log.Warning(
                    "درخواست ویرایش خدمات با قیمت منفی. Price: {Price}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.Price,
                    model.ServiceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("قیمت نمی‌تواند منفی باشد.");
            }

            if (model.ServiceCategoryId <= 0)
            {
                _log.Warning(
                    "درخواست ویرایش خدمات با دسته‌بندی نامعتبر. ServiceCategoryId: {ServiceCategoryId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.ServiceCategoryId,
                    model.ServiceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("دسته‌بندی خدمات انتخاب شده معتبر نیست.");
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // دریافت خدمات موجود با فیلتر Soft Delete
                    var existingService = await _context.Services
                        .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId && !s.IsDeleted);

                    if (existingService == null)
                    {
                        _log.Warning(
                            "درخواست ویرایش برای خدمات غیرموجود. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            model.ServiceId,
                            _currentUserService.UserName,
                            _currentUserService.UserId);

                        return ServiceResult.Failed("خدماتی که در حال ویرایش آن هستید پیدا نشد یا حذف شده است.");
                    }

                    // بررسی وجود کد خدمات تکراری
                    if (await IsDuplicateServiceCodeAsync(model.ServiceCode, model.ServiceId))
                    {
                        _log.Warning(
                            "درخواست ویرایش خدمات با کد تکراری. ServiceCode: {ServiceCode}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            model.ServiceCode,
                            model.ServiceId,
                            _currentUserService.UserName,
                            _currentUserService.UserId);

                        return ServiceResult.Failed("خدماتی با این کد از قبل وجود دارد.");
                    }

                    // بررسی وجود دسته‌بندی خدمات معتبر
                    if (!await IsServiceCategoryValidAsync(model.ServiceCategoryId))
                    {
                        _log.Warning(
                            "درخواست ویرایش خدمات با دسته‌بندی نامعتبر. ServiceCategoryId: {ServiceCategoryId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            model.ServiceCategoryId,
                            model.ServiceId,
                            _currentUserService.UserName,
                            _currentUserService.UserId);

                        return ServiceResult.Failed("دسته‌بندی خدمات انتخاب شده معتبر نیست یا حذف شده است.");
                    }

                    // به‌روزرسانی خدمات
                    existingService.Title = model.Title;
                    existingService.ServiceCode = model.ServiceCode;
                    existingService.Price = model.Price;
                    existingService.Description = model.Description;
                    existingService.ServiceCategoryId = model.ServiceCategoryId;
                    existingService.UpdatedAt = _currentUserService.UtcNow;
                    existingService.UpdatedByUserId = _currentUserService.UserId;
                    _context.Entry(existingService).State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    _log.Information(
                        "خدمات با موفقیت به‌روزرسانی شد. ServiceId: {ServiceId}, UpdatedBy: {UpdatedBy}",
                        model.ServiceId,
                        _currentUserService.UserId);

                    return ServiceResult.Successful("اطلاعات خدمات با موفقیت به‌روزرسانی شد.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطای غیرمنتظره در به‌روزرسانی خدمات. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                model.ServiceId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult.Failed("خطای سیستم در حین ذخیره تغییرات رخ داده است.");
        }
    }

    /// <summary>
    /// حذف نرم (Soft-delete) یک خدمات با رعایت تمام استانداردهای پزشکی و حفظ اطلاعات مالی
    /// این عملیات تمام رکوردهای مرتبط را نیز به صورت نرم حذف می‌کند
    /// </summary>
    /// <param name="serviceId">شناسه خدمات مورد نظر</param>
    /// <returns>نتیجه عملیات حذف</returns>
    public async Task<ServiceResult> DeleteServiceAsync(int serviceId)
    {
        _log.Information(
            "درخواست حذف خدمات با شناسه {ServiceId}. User: {UserName} (Id: {UserId})",
            serviceId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (serviceId <= 0)
            {
                _log.Warning(
                    "درخواست حذف خدمات با شناسه نامعتبر. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("شناسه خدمات معتبر نیست.");
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // دریافت خدمات موجود با فیلتر Soft Delete
                    var service = await _context.Services
                        .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                    if (service == null)
                    {
                        _log.Warning(
                            "درخواست حذف برای خدمات غیرموجود یا حذف شده. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            serviceId,
                            _currentUserService.UserName,
                            _currentUserService.UserId);

                        return ServiceResult.Failed("خدماتی که در حال حذف آن هستید پیدا نشد یا قبلاً حذف شده است.");
                    }

                    // بررسی وجود استفاده از خدمات در نوبت‌ها
                    var usageCount = await _context.ReceptionItems
                        .CountAsync(ri => ri.ServiceId == serviceId);

                    if (usageCount > 0)
                    {
                        _log.Warning(
                            "تلاش برای حذف خدمات با {UsageCount} استفاده. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            usageCount,
                            serviceId,
                            _currentUserService.UserName,
                            _currentUserService.UserId);

                        return ServiceResult.Failed($"این خدمات در {usageCount} نوبت استفاده شده و نمی‌تواند حذف شود.");
                    }

                    // تنظیمات Soft Delete برای خدمات
                    service.IsDeleted = true;
                    service.DeletedAt = _currentUserService.UtcNow;
                    service.DeletedByUserId = _currentUserService.UserId;
                    _context.Entry(service).State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    _log.Information(
                        "خدمات با موفقیت حذف شد. ServiceId: {ServiceId}, DeletedBy: {DeletedBy}",
                        serviceId,
                        _currentUserService.UserId);

                    return ServiceResult.Successful("خدمات با موفقیت حذف شد.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در حذف خدمات. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                serviceId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult.Failed("خطای سیستم در حین حذف خدمات رخ داده است.");
        }
    }

    /// <summary>
    /// بازیابی داده‌های یک خدمات برای پر کردن فرم ویرایش
    /// این متد فقط خدمات‌های فعال (غیرحذف شده) را بازمی‌گرداند
    /// </summary>
    /// <param name="serviceId">شناسه خدمات مورد نظر</param>
    /// <returns>مدل داده‌های خدمات برای ویرایش</returns>
    /// <summary>
    /// بازیابی داده‌های یک خدمات برای پر کردن فرم ویرایش
    /// این متد فقط خدمات‌های فعال (غیرحذف شده) را بازمی‌گرداند
    /// </summary>
    /// <param name="serviceId">شناسه خدمات مورد نظر</param>
    /// <returns>مدل داده‌های خدمات برای ویرایش</returns>
    public async Task<ServiceResult<ServiceCreateEditViewModel>> GetServiceForEditAsync(int serviceId)
    {
        _log.Information(
            "درخواست دریافت اطلاعات خدمات برای ویرایش. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
            serviceId,
            _currentUserService.UserName,
            _currentUserService.UserId);
        try
        {
            // اعتبارسنجی ورودی
            if (serviceId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت اطلاعات خدمات با شناسه نامعتبر. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);
                return ServiceResult<ServiceCreateEditViewModel>.Failed("شناسه خدمات معتبر نیست.");
            }
            // دریافت خدمات با روابط مورد نیاز
            var service = await _context.Services
                .Include(s => s.ServiceCategory)
                .Include(s => s.ServiceCategory.Department)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);
            if (service == null)
            {
                _log.Warning(
                    "درخواست ویرایش برای خدمات غیرموجود یا حذف شده. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);
                return ServiceResult<ServiceCreateEditViewModel>.Failed("خدمات مشخص‌شده پیدا نشد یا حذف شده است.");
            }
            // دریافت اطلاعات کاربر ایجاد کننده
            string createdBy = "سیستم";
            if (!string.IsNullOrEmpty(service.CreatedByUserId))
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == service.CreatedByUserId);
                if (user != null)
                {
                    createdBy = $"{user.FirstName} {user.LastName}";
                }
            }
            // دریافت اطلاعات کاربر ویرایش کننده
            string updatedBy = "";
            if (!string.IsNullOrEmpty(service.UpdatedByUserId))
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == service.UpdatedByUserId);
                if (user != null)
                {
                    updatedBy = $"{user.FirstName} {user.LastName}";
                }
            }

            // محاسبه تعداد استفاده‌ها و درآمد کل
            var usageCount = await GetUsageCountAsync(serviceId);
            var totalRevenue = await GetTotalRevenueAsync(serviceId);

            // ساخت ViewModel به صورت دستی
            var viewModel = new ServiceCreateEditViewModel
            {
                ServiceId = service.ServiceId,
                Title = service.Title,
                ServiceCode = service.ServiceCode,
                Price = service.Price,
                Description = service.Description,
                ServiceCategoryId = service.ServiceCategoryId,
                ServiceCategoryTitle = service.ServiceCategory?.Title,
                IsActive = !service.IsDeleted,
                CreatedAt = service.CreatedAt,
                CreatedBy = createdBy,
                UpdatedAt = service.UpdatedAt,
                UpdatedBy = updatedBy,
                UsageCount = usageCount,
                TotalRevenue = totalRevenue
            };
            // تبدیل تاریخ به شمسی برای محیط‌های پزشکی ایرانی
            viewModel.CreatedAtShamsi = DateTimeExtensions.ToPersianDateTime(viewModel.CreatedAt);
            if (viewModel.UpdatedAt.HasValue)
                viewModel.UpdatedAtShamsi = DateTimeExtensions.ToPersianDateTime(viewModel.UpdatedAt.Value);
            return ServiceResult<ServiceCreateEditViewModel>.Successful(viewModel);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت اطلاعات خدمات برای ویرایش. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                serviceId,
                _currentUserService.UserName,
                _currentUserService.UserId);
            return ServiceResult<ServiceCreateEditViewModel>.Failed("خطای سیستم رخ داده است.");
        }
    }
    // اضافه کردن فضای نام ضروری برای استفاده از ThenInclude


    /// <summary>
    /// بازیابی جزئیات کامل یک خدمات برای نمایش اطلاعات
    /// این متد فقط خدمات‌های فعال (غیرحذف شده) را بازمی‌گرداند و تمام استانداردهای سیستم‌های پزشکی را رعایت می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. رعایت کامل استانداردهای امنیتی پزشکی در مدیریت اطلاعات
    /// 2. پشتیبانی کامل از محیط‌های ایرانی و تبدیل تاریخ به شمسی
    /// 3. ارائه اطلاعات کامل برای تصمیم‌گیری‌های مدیریتی
    /// 4. بهینه‌سازی عملکرد برای سیستم‌های پزشکی با ترافیک بالا
    /// 5. مدیریت حرفه‌ای خطاها و ارائه پیام‌های کاربرپسند
    /// 6. ارائه آمار و تحلیل‌های پزشکی برای بهبود کیفیت خدمات
    /// 7. رعایت استانداردهای حفظ اطلاعات پزشکی (حداقل 10 سال)
    /// 8. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات
    /// 9. ارائه اطلاعات کاربران مرتبط با عملیات‌های سیستم
    /// 10. ارائه اطلاعات کامل برای گزارش‌گیری پزشکی
    /// </summary>
    /// <param name="serviceId">شناسه خدمات مورد نظر</param>
    /// <returns>مدل جزئیات کامل خدمات</returns>
    public async Task<ServiceResult<ViewModels.Reception.ServiceDetailsViewModel>> GetServiceDetailsAsync(int serviceId)
    {
        _log.Information(
            "درخواست دریافت جزئیات خدمات. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
            serviceId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (serviceId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت جزئیات خدمات با شناسه نامعتبر. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);
                return ServiceResult<ViewModels.Reception.ServiceDetailsViewModel>.Failed("شناسه خدمات معتبر نیست.");
            }

            // دریافت خدمات با روابط مورد نیاز - استفاده صحیح از ThenInclude
            // دریافت خدمات با روابط مورد نیاز - استفاده صحیح از Include برای EF 6.1+
            var service = await _context.Services
                .Include(s => s.ServiceCategory)
                .Include(s => s.ServiceCategory.Department)
                .Include(s => s.ServiceCategory.Department.Clinic)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

            if (service == null)
            {
                _log.Warning(
                    "درخواست جزئیات برای خدمات غیرموجود یا حذف شده. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);
                return ServiceResult<ViewModels.Reception.ServiceDetailsViewModel>.Failed("خدمات مشخص‌شده پیدا نشد یا حذف شده است.");
            }

            // دریافت اطلاعات کاربران مرتبط به صورت یکجا برای کاهش تعداد کوئری‌ها
            var userIds = new HashSet<string>();
            if (!string.IsNullOrEmpty(service.CreatedByUserId)) userIds.Add(service.CreatedByUserId);
            if (!string.IsNullOrEmpty(service.UpdatedByUserId)) userIds.Add(service.UpdatedByUserId);
            if (!string.IsNullOrEmpty(service.DeletedByUserId)) userIds.Add(service.DeletedByUserId);

            var users = userIds.Any()
                ? await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id)
                : new Dictionary<string, ApplicationUser>();

            // محاسبه آمار استفاده و درآمد
            var usageCount = await GetUsageCountAsync(serviceId);
            var totalRevenue = await GetTotalRevenueAsync(serviceId);

            // محاسبه تاریخ آخرین استفاده
            DateTime? lastUsageDate = null;
            var lastUsage = await _context.ReceptionItems
                .Where(ri => ri.ServiceId == serviceId && !ri.IsDeleted)
                .OrderByDescending(ri => ri.CreatedAt)
                .Select(ri => ri.CreatedAt)
                .FirstOrDefaultAsync();

            // بررسی اینکه آیا مقداری پیدا شده است یا نه
            if (lastUsage != default(DateTime))
            {
                lastUsageDate = lastUsage;
            }

            // ساخت ViewModel به صورت دستی
            var details = new ViewModels.Reception.ServiceDetailsViewModel
            {
                ServiceId = service.ServiceId,
                Title = service.Title,
                ServiceName = service.Title,
                ServiceCode = service.ServiceCode,
                ServiceCategoryTitle = service.ServiceCategory?.Title,
                DepartmentTitle = service.ServiceCategory?.Department?.Name,
                ClinicTitle = service.ServiceCategory?.Department?.Clinic?.Name,
                Price = service.Price,
                BasePrice = service.Price,
                Description = service.Description,
                IsActive = !service.IsDeleted,
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt ?? DateTime.MinValue,
                DeletedAt = service.DeletedAt,
                // افزودن فیلدهای مورد نیاز
                ServiceCategoryId = service.ServiceCategoryId,
                DepartmentId = service.ServiceCategory?.DepartmentId ?? 0,
                ClinicId = service.ServiceCategory?.Department?.ClinicId ?? 0,
                DepartmentName = service.ServiceCategory?.Department?.Name,
                ClinicName = service.ServiceCategory?.Department?.Clinic?.Name
            };

            // تنظیم اطلاعات کاربران
            details.CreatedBy = GetUserName(users, service.CreatedByUserId) ?? "سیستم";
            details.UpdatedBy = GetUserName(users, service.UpdatedByUserId) ?? "";
            details.DeletedBy = GetUserName(users, service.DeletedByUserId) ?? "";

            // تبدیل تاریخ به شمسی برای محیط‌های پزشکی ایرانی
            details.CreatedAtShamsi = DateTimeExtensions.ToPersianDateTime(details.CreatedAt);
            if (details.UpdatedAt != DateTime.MinValue)
                details.UpdatedAtShamsi = DateTimeExtensions.ToPersianDateTime(details.UpdatedAt);
            if (details.DeletedAt.HasValue)
                details.DeletedAtShamsi = DateTimeExtensions.ToPersianDateTime(details.DeletedAt.Value);
            if (details.LastUsageDate.HasValue)
                details.LastUsageDateShamsi = DateTimeExtensions.ToPersianDateTime(details.LastUsageDate.Value);

            // افزودن اطلاعات اضافی برای گزارش‌گیری پزشکی
            details.UsageCount = usageCount;
            details.TotalRevenue = totalRevenue;
            details.LastUsageDate = lastUsageDate;
            details.LastUsageDateShamsi = details.LastUsageDate.HasValue ?
                DateTimeExtensions.ToPersianDateTime(details.LastUsageDate.Value) : "نامشخص";

            _log.Information(
                "دریافت جزئیات خدمات با شناسه {ServiceId} با موفقیت انجام شد. User: {UserName} (Id: {UserId})",
                serviceId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<ViewModels.Reception.ServiceDetailsViewModel>.Successful(details);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت جزئیات خدمات. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                serviceId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<ViewModels.Reception.ServiceDetailsViewModel>.Failed(
                "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید و در صورت تکرار خطا با پشتیبانی تماس بگیرید.");
        }
    }

    /// <summary>
    /// دریافت قیمت خدمت
    /// </summary>
    public async Task<ServiceResult<decimal>> GetServicePriceAsync(int serviceId)
    {
        try
        {
            _log.Information("درخواست دریافت قیمت خدمت. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                serviceId, _currentUserService.UserName, _currentUserService.UserId);

            var service = await _context.Services
                .Where(s => s.ServiceId == serviceId && !s.IsDeleted)
                .Select(s => new { s.Price })
                .FirstOrDefaultAsync();

            if (service != null)
            {
                _log.Information("قیمت خدمت دریافت شد. ServiceId: {ServiceId}, Price: {Price}. User: {UserName} (Id: {UserId})",
                    serviceId, service.Price, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<decimal>.Successful(service.Price);
            }
            else
            {
                _log.Warning("خدمت یافت نشد. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<decimal>.Failed("خدمت یافت نشد");
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در دریافت قیمت خدمت. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                serviceId, _currentUserService.UserName, _currentUserService.UserId);
            return ServiceResult<decimal>.Failed("خطا در دریافت قیمت خدمت");
        }
    }

    /// <summary>
    /// کمک‌کننده برای دریافت نام کاربر
    /// </summary>
    private string GetUserName(Dictionary<string, ApplicationUser> users, string userId)
    {
        if (string.IsNullOrEmpty(userId) || !users.ContainsKey(userId))
            return null;

        var user = users[userId];
        return $"{user.FirstName} {user.LastName}".Trim();
    }


    /// <summary>
    /// دریافت تاریخ آخرین استفاده از خدمات
    /// </summary>
    private async Task<DateTime?> GetLastUsageDateAsync(int serviceId)
    {
        try
        {
            return await _context.ReceptionItems
                .Where(ri => ri.ServiceId == serviceId && !ri.IsDeleted)
                .Select(ri => ri.CreatedAt)
                .MaxAsync();
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت تاریخ آخرین استفاده برای خدمات {ServiceId}",
                serviceId);
            return null;
        }
    }

    /// <summary>
    /// جستجو و صفحه‌بندی خدمات با رعایت فیلترهای ضروری و عملکرد بهینه
    /// این متد فقط خدمات‌های فعال (غیرحذف شده) را بازمی‌گرداند
    /// </summary>
    /// <param name="searchTerm">عبارت جستجو</param>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات برای فیلتر</param>
    /// <param name="pageNumber">شماره صفحه</param>
    /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
    /// <returns>نتایج جستجو به همراه اطلاعات صفحه‌بندی</returns>
    public async Task<ServiceResult<PagedResult<ServiceIndexViewModel>>> SearchServicesAsync(string searchTerm, int? serviceCategoryId, int pageNumber, int pageSize)
    {
        _log.Information(
            "درخواست جستجو خدمات. Term: {SearchTerm}, ServiceCategoryId: {ServiceCategoryId}, Page: {PageNumber}, Size: {PageSize}. User: {UserName} (Id: {UserId})",
            searchTerm,
            serviceCategoryId,
            pageNumber,
            pageSize,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی‌ها
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

            // شروع با تمام خدمات فعال (غیرحذف شده)
            var query = _context.Services
                .Include(s => s.ServiceCategory)
                .Where(s => !s.IsDeleted)
                .AsQueryable();

            // اعمال فیلتر دسته‌بندی خدمات
            if (serviceCategoryId.HasValue && serviceCategoryId > 0)
            {
                query = query.Where(s => s.ServiceCategoryId == serviceCategoryId);
            }

            // اعمال فیلتر جستجو
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s =>
                    s.Title.Contains(searchTerm) ||
                    s.ServiceCode.Contains(searchTerm) ||
                    (s.ServiceCategory != null && s.ServiceCategory.Title.Contains(searchTerm))
                );
            }

            // محاسبه تعداد کل آیتم‌ها
            var totalItems = await query.CountAsync();

            // اعمال صفحه‌بندی
            var items = await query
                .OrderBy(s => s.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ساخت ViewModelها به صورت دستی
            var viewModels = new List<ServiceIndexViewModel>();
            foreach (var item in items)
            {
                // دریافت اطلاعات کاربر ایجاد کننده
                string createdBy = "سیستم";
                if (!string.IsNullOrEmpty(item.CreatedByUserId))
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == item.CreatedByUserId);

                    if (user != null)
                    {
                        createdBy = $"{user.FirstName} {user.LastName}";
                    }
                }

                viewModels.Add(new ServiceIndexViewModel
                {
                    ServiceId = item.ServiceId,
                    Title = item.Title,
                    ServiceCode = item.ServiceCode,
                    ServiceCategoryTitle = item.ServiceCategory?.Title,
                    Price = item.Price,
                    IsActive = !item.IsDeleted,
                    CreatedAt = item.CreatedAt
                });
            }

            // تبدیل تاریخ‌ها به شمسی
            foreach (var viewModel in viewModels)
            {
                viewModel.CreatedAtShamsi = DateTimeExtensions.ToPersianDate(viewModel.CreatedAt);
            }

            // ایجاد نتیجه صفحه‌بندی شده
            var pagedResult = new PagedResult<ServiceIndexViewModel>
            {
                Items = viewModels,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            _log.Information(
                "جستجوی خدمات با موفقیت انجام شد. Found: {TotalItems}. User: {UserName} (Id: {UserId})",
                totalItems,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<PagedResult<ServiceIndexViewModel>>.Successful(pagedResult);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در جستجوی خدمات. Term: {SearchTerm}, ServiceCategoryId: {ServiceCategoryId}, Page: {PageNumber}, Size: {PageSize}. User: {UserName} (Id: {UserId})",
                searchTerm,
                serviceCategoryId,
                pageNumber,
                pageSize,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<PagedResult<ServiceIndexViewModel>>.Failed("خطای سیستم در حین جستجو رخ داده است.");
        }
    }

    /// <summary>
    /// بررسی وجود کد خدمات تکراری
    /// </summary>
    /// <param name="serviceCode">کد خدمات</param>
    /// <param name="serviceId">شناسه خدمات (برای ویرایش)</param>
    /// <returns>آیا کد خدمات تکراری است؟</returns>
    public async Task<bool> IsDuplicateServiceCodeAsync(string serviceCode, int serviceId = 0)
    {
        return await _context.Services
            .AnyAsync(s =>
                s.ServiceCode == serviceCode &&
                s.ServiceId != serviceId &&
                !s.IsDeleted);
    }

    /// <summary>
    /// بررسی اعتبار دسته‌بندی خدمات
    /// </summary>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
    /// <returns>آیا دسته‌بندی خدمات معتبر است؟</returns>
    public async Task<bool> IsServiceCategoryValidAsync(int serviceCategoryId)
    {
        return await _context.ServiceCategories
            .AnyAsync(sc =>
                sc.ServiceCategoryId == serviceCategoryId &&
                !sc.IsDeleted);
    }

    /// <summary>
    /// دریافت لیست خدمات فعال برای استفاده در کنترل‌های انتخاب
    /// </summary>
    /// <returns>لیست خدمات فعال</returns>
    public async Task<IEnumerable<ServiceSelectItem>> GetActiveServicesAsync()
    {
        _log.Information(
            "درخواست دریافت لیست خدمات فعال. User: {UserName} (Id: {UserId})",
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            var services = await _context.Services
                .Include(s => s.ServiceCategory)
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Title)
                .Select(s => new ServiceSelectItem
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title,
                    ServiceCode = s.ServiceCode,
                    Price = s.Price,
                    ServiceCategoryId = s.ServiceCategoryId,
                    ServiceCategoryTitle = s.ServiceCategory.Title,
                    UsageCount = s.ReceptionItems.Count(ri => !ri.IsDeleted)
                })
                .ToListAsync();

            _log.Information(
                "دریافت لیست خدمات فعال با موفقیت انجام شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                services.Count,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return services;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت لیست خدمات فعال. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName,
                _currentUserService.UserId);

            return new List<ServiceSelectItem>();
        }
    }

    /// <summary>
    /// جستجوی خدمات برای Select2 با پشتیبانی از AJAX
    /// </summary>
    /// <param name="searchTerm">عبارت جستجو</param>
    /// <param name="page">شماره صفحه</param>
    /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
    /// <returns>نتیجه جستجو با صفحه‌بندی</returns>
    public async Task<ServiceResult<PagedResult<ServiceSelectItem>>> SearchServicesForSelect2Async(string searchTerm = "", int page = 1, int pageSize = 20)
    {
        _log.Information(
            "درخواست جستجوی خدمات برای Select2. SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
            searchTerm, page, pageSize, _currentUserService.UserName, _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;
            if (pageSize > 100) pageSize = 100; // محدودیت برای جلوگیری از بارگذاری بیش از حد

            var query = _context.Services
                .AsNoTracking()
                .Where(s => !s.IsDeleted && s.IsActive);

            // اعمال فیلتر جستجو
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var trimmedSearchTerm = searchTerm.Trim();
                query = query.Where(s => 
                    s.Title.Contains(trimmedSearchTerm) ||
                    s.ServiceCode.Contains(trimmedSearchTerm));
            }

            // محاسبه تعداد کل
            var totalCount = await query.CountAsync();

            // دریافت آیتم‌های صفحه جاری
            var items = await query
                .OrderBy(s => s.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ServiceSelectItem
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title,
                    ServiceCode = s.ServiceCode
                })
                .ToListAsync();

            var result = new PagedResult<ServiceSelectItem>
            {
                Items = items,
                TotalItems = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };

            _log.Information(
                "جستجوی خدمات برای Select2 موفق. Found: {Count}, Total: {TotalCount}, SearchTerm: {SearchTerm}. User: {UserName} (Id: {UserId})",
                items.Count, totalCount, searchTerm, _currentUserService.UserName, _currentUserService.UserId);

            return ServiceResult<PagedResult<ServiceSelectItem>>.Successful(result);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در جستجوی خدمات برای Select2. SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                searchTerm, page, pageSize, _currentUserService.UserName, _currentUserService.UserId);

            return ServiceResult<PagedResult<ServiceSelectItem>>.Failed("خطا در جستجوی خدمات");
        }
    }

    /// <summary>
    /// دریافت لیست خدمات فعال برای یک دسته‌بندی خاص
    /// </summary>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
    /// <returns>لیست خدمات فعال برای دسته‌بندی مورد نظر</returns>
    public async Task<IEnumerable<ServiceSelectItem>> GetActiveServicesByCategoryAsync(int serviceCategoryId)
    {
        _log.Information(
            "درخواست دریافت لیست خدمات فعال برای دسته‌بندی {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
            serviceCategoryId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (serviceCategoryId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت لیست خدمات با شناسه دسته‌بندی نامعتبر. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    serviceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return new List<ServiceSelectItem>();
            }

            var services = await _context.Services
                .Include(s => s.ServiceCategory)
                .Where(s => s.ServiceCategoryId == serviceCategoryId && !s.IsDeleted)
                .OrderBy(s => s.Title)
                .Select(s => new ServiceSelectItem
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title,
                    ServiceCode = s.ServiceCode,
                    Price = s.Price,
                    ServiceCategoryId = s.ServiceCategoryId,
                    ServiceCategoryTitle = s.ServiceCategory.Title,
                    UsageCount = s.ReceptionItems.Count(ri => !ri.IsDeleted)
                })
                .ToListAsync();

            _log.Information(
                "دریافت لیست خدمات فعال برای دسته‌بندی {ServiceCategoryId} با موفقیت انجام شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                serviceCategoryId,
                services.Count,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return services;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت لیست خدمات فعال برای دسته‌بندی {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                serviceCategoryId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return new List<ServiceSelectItem>();
        }
    }

    /// <summary>
    /// بررسی اینکه آیا خدمات قابل حذف است یا خیر
    /// </summary>
    /// <param name="serviceId">شناسه خدمات</param>
    /// <returns>آیا خدمات قابل حذف است؟</returns>
    public async Task<ServiceResult> CanDeleteServiceAsync(int serviceId)
    {
        _log.Information(
            "درخواست بررسی امکان حذف خدمات با شناسه {ServiceId}. User: {UserName} (Id: {UserId})",
            serviceId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (serviceId <= 0)
            {
                _log.Warning(
                    "درخواست بررسی امکان حذف خدمات با شناسه نامعتبر. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("شناسه خدمات معتبر نیست.");
            }

            // دریافت خدمات موجود با فیلتر Soft Delete
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

            if (service == null)
            {
                _log.Warning(
                    "درخواست بررسی امکان حذف برای خدمات غیرموجود یا حذف شده. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("خدماتی که در حال بررسی امکان حذف آن هستید پیدا نشد یا حذف شده است.");
            }

            // بررسی وجود استفاده از خدمات در نوبت‌ها
            var usageCount = await _context.ReceptionItems
                .CountAsync(ri => ri.ServiceId == serviceId);

            if (usageCount > 0)
            {
                _log.Information(
                    "خدمات با {UsageCount} استفاده قابل حذف نیست. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    usageCount,
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed($"این خدمات در {usageCount} نوبت استفاده شده و نمی‌تواند حذف شود.");
            }

            return ServiceResult.Successful("خدمات قابل حذف است.");
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در بررسی امکان حذف خدمات. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                serviceId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult.Failed("خطای سیستم در حین بررسی امکان حذف رخ داده است.");
        }
    }

    /// <summary>
    /// بازیابی تعداد استفاده‌های یک خدمات
    /// </summary>
    /// <param name="serviceId">شناسه خدمات</param>
    /// <returns>تعداد استفاده‌ها</returns>
    public async Task<int> GetUsageCountAsync(int serviceId)
    {
        _log.Information(
            "درخواست دریافت تعداد استفاده‌های خدمات با شناسه {ServiceId}. User: {UserName} (Id: {UserId})",
            serviceId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (serviceId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت تعداد استفاده‌ها با شناسه نامعتبر. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return 0;
            }

            var count = await _context.ReceptionItems
                .CountAsync(ri => ri.ServiceId == serviceId && !ri.IsDeleted);

            _log.Information(
                "دریافت تعداد استفاده‌های خدمات با شناسه {ServiceId} با موفقیت انجام شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                serviceId,
                count,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return count;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت تعداد استفاده‌های خدمات با شناسه {ServiceId}. User: {UserName} (Id: {UserId})",
                serviceId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return 0;
        }
    }

    /// <summary>
    /// بازیابی درآمد کل ایجاد شده توسط یک خدمات
    /// </summary>
    /// <param name="serviceId">شناسه خدمات</param>
    /// <returns>درآمد کل</returns>
    public async Task<decimal> GetTotalRevenueAsync(int serviceId)
    {
        _log.Information(
            "درخواست دریافت درآمد کل خدمات با شناسه {ServiceId}. User: {UserName} (Id: {UserId})",
            serviceId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (serviceId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت درآمد کل با شناسه نامعتبر. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return 0;
            }

            var totalRevenue = await _context.ReceptionItems
                .Where(ri => ri.ServiceId == serviceId && !ri.IsDeleted)
                .SumAsync(ri => ri.UnitPrice);

            _log.Information(
                "دریافت درآمد کل خدمات با شناسه {ServiceId} با موفقیت انجام شد. Revenue: {Revenue}. User: {UserName} (Id: {UserId})",
                serviceId,
                totalRevenue,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return totalRevenue;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت درآمد کل خدمات با شناسه {ServiceId}. User: {UserName} (Id: {UserId})",
                serviceId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return 0;
        }
    }

    /// <summary>
    /// بازیابی آمار استفاده از خدمات در بازه زمانی مشخص
    /// این متد برای گزارش‌گیری پزشکی طراحی شده و تمام استانداردهای سیستم‌های پزشکی را رعایت می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. رعایت کامل استانداردهای امنیتی پزشکی در مدیریت اطلاعات
    /// 2. پشتیبانی کامل از محیط‌های ایرانی و تبدیل تاریخ به شمسی
    /// 3. ارائه اطلاعات کامل برای تصمیم‌گیری‌های مدیریتی
    /// 4. بهینه‌سازی عملکرد برای سیستم‌های پزشکی با ترافیک بالا
    /// 5. مدیریت حرفه‌ای خطاها و ارائه پیام‌های کاربرپسند
    /// 6. ارائه آمار و تحلیل‌های پزشکی برای بهبود کیفیت خدمات
    /// 7. رعایت استانداردهای حفظ اطلاعات پزشکی (حداقل 10 سال)
    /// 8. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات
    /// 9. ارائه اطلاعات کاربران مرتبط با عملیات‌های سیستم
    /// 10. ارائه اطلاعات کامل برای گزارش‌گیری پزشکی
    /// </summary>
    /// <param name="serviceId">شناسه خدمات</param>
    /// <param name="startDate">تاریخ شروع</param>
    /// <param name="endDate">تاریخ پایان</param>
    /// <returns>آمار استفاده</returns>
    public async Task<ServiceUsageStatistics> GetUsageStatisticsAsync(int serviceId, DateTime startDate, DateTime endDate)
    {
        _log.Information(
            "درخواست دریافت آمار استفاده از خدمات با شناسه {ServiceId} برای بازه زمانی {StartDate} تا {EndDate}. User: {UserName} (Id: {UserId})",
            serviceId,
            startDate,
            endDate,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی‌ها
            if (serviceId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت آمار استفاده با شناسه نامعتبر. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return new ServiceUsageStatistics
                {
                    TotalUsage = 0,
                    TotalRevenue = 0,
                    StartDate = startDate,
                    EndDate = endDate,
                    DailyUsage = new Dictionary<string, int>(),
                    DailyRevenue = new Dictionary<string, decimal>()
                };
            }

            // اطمینان از صحت بازه زمانی
            if (endDate < startDate)
            {
                _log.Warning(
                    "درخواست دریافت آمار استفاده با بازه زمانی نامعتبر. StartDate: {StartDate}, EndDate: {EndDate}. User: {UserName} (Id: {UserId})",
                    startDate,
                    endDate,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                // تبادل تاریخ‌ها برای جلوگیری از خطا
                var temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            // محاسبه آمار کلی - رفع خطای Sum برای مجموعه خالی
            var totalUsage = await _context.ReceptionItems
                .CountAsync(ri => ri.ServiceId == serviceId &&
                                 !ri.IsDeleted &&
                                 ri.CreatedAt >= startDate &&
                                 ri.CreatedAt <= endDate);

            // رفع خطای اصلی: استفاده از DefaultIfEmpty برای جلوگیری از خطا در SumAsync
            var totalRevenue = await _context.ReceptionItems
                .Where(ri => ri.ServiceId == serviceId &&
                            !ri.IsDeleted &&
                            ri.CreatedAt >= startDate &&
                            ri.CreatedAt <= endDate)
                .Select(ri => (decimal?)ri.UnitPrice)
                .DefaultIfEmpty(0)
                .SumAsync() ?? 0;

            // محاسبه آمار روزانه
            var dailyUsage = new Dictionary<string, int>();
            var dailyRevenue = new Dictionary<string, decimal>();

            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                var nextDate = currentDate.AddDays(1);

                var usageCount = await _context.ReceptionItems
                    .CountAsync(ri => ri.ServiceId == serviceId &&
                                     !ri.IsDeleted &&
                                     ri.CreatedAt >= currentDate &&
                                     ri.CreatedAt < nextDate);

                // رفع خطای اصلی: استفاده از DefaultIfEmpty برای جلوگیری از خطا در SumAsync برای روزهای خالی
                var revenue = await _context.ReceptionItems
                    .Where(ri => ri.ServiceId == serviceId &&
                                !ri.IsDeleted &&
                                ri.CreatedAt >= currentDate &&
                                ri.CreatedAt < nextDate)
                    .Select(ri => (decimal?)ri.UnitPrice)
                    .DefaultIfEmpty(0)
                    .SumAsync() ?? 0;

                // تبدیل تاریخ به شمسی برای کلید دیکشنری
                var persianDate = DateTimeExtensions.ToPersianDate(currentDate);
                dailyUsage[persianDate] = usageCount;
                dailyRevenue[persianDate] = revenue;

                currentDate = nextDate;
            }

            _log.Information(
                "دریافت آمار استفاده از خدمات با شناسه {ServiceId} برای بازه زمانی {StartDate} تا {EndDate} با موفقیت انجام شد. TotalUsage: {TotalUsage}, TotalRevenue: {TotalRevenue}. User: {UserName} (Id: {UserId})",
                serviceId,
                startDate,
                endDate,
                totalUsage,
                totalRevenue,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return new ServiceUsageStatistics
            {
                TotalUsage = totalUsage,
                TotalRevenue = totalRevenue,
                StartDate = startDate,
                EndDate = endDate,
                DailyUsage = dailyUsage,
                DailyRevenue = dailyRevenue
            };
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت آمار استفاده از خدمات با شناسه {ServiceId} برای بازه زمانی {StartDate} تا {EndDate}. User: {UserName} (Id: {UserId})",
                serviceId,
                startDate,
                endDate,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return new ServiceUsageStatistics
            {
                TotalUsage = 0,
                TotalRevenue = 0,
                StartDate = startDate,
                EndDate = endDate,
                DailyUsage = new Dictionary<string, int>(),
                DailyRevenue = new Dictionary<string, decimal>()
            };
        }
    }

    /// <summary>
    /// بررسی وجود خدمات فعال با شناسه مشخص
    /// </summary>
    /// <param name="serviceId">شناسه خدمات</param>
    /// <returns>آیا خدمات فعال وجود دارد؟</returns>
    public async Task<bool> IsActiveServiceExistsAsync(int serviceId)
    {
        _log.Information(
            "درخواست بررسی وجود خدمات فعال با شناسه {ServiceId}. User: {UserName} (Id: {UserId})",
            serviceId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (serviceId <= 0)
            {
                _log.Warning(
                    "درخواست بررسی وجود خدمات با شناسه نامعتبر. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return false;
            }

            var exists = await _context.Services
                .AnyAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

            _log.Information(
                "بررسی وجود خدمات فعال با شناسه {ServiceId} با موفقیت انجام شد. Exists: {Exists}. User: {UserName} (Id: {UserId})",
                serviceId,
                exists,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return exists;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در بررسی وجود خدمات فعال با شناسه {ServiceId}. User: {UserName} (Id: {UserId})",
                serviceId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return false;
        }
    }

    /// <summary>
    /// دریافت لیست خدمات پرطرفدار
    /// </summary>
    /// <param name="topCount">تعداد خدمات مورد نظر</param>
    /// <param name="daysBack">تعداد روزهای بررسی</param>
    /// <returns>لیست خدمات پرطرفدار</returns>
    public async Task<IEnumerable<TopServiceItem>> GetTopServicesAsync(int topCount = 10, int daysBack = 30)
    {
        _log.Information(
            "درخواست دریافت لیست {TopCount} خدمات پرطرفدار در {DaysBack} روز گذشته. User: {UserName} (Id: {UserId})",
            topCount,
            daysBack,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی‌ها
            topCount = topCount < 1 ? 1 : (topCount > 50 ? 50 : topCount);
            daysBack = daysBack < 1 ? 1 : (daysBack > 365 ? 365 : daysBack);

            var startDate = DateTime.UtcNow.AddDays(-daysBack);

            var topServices = await _context.ReceptionItems
                .Include(ri => ri.Service)
                .Include(ri => ri.Service.ServiceCategory)
                .Where(ri => !ri.IsDeleted && ri.CreatedAt >= startDate)
                .GroupBy(ri => ri.ServiceId)
                .Select(g => new
                {
                    ServiceId = g.Key,
                    UsageCount = g.Count(),
                    TotalRevenue = g.Sum(ri => ri.UnitPrice)
                })
                .OrderByDescending(x => x.UsageCount)
                .Take(topCount)
                .ToListAsync();

            var serviceIds = topServices.Select(x => x.ServiceId).ToList();
            var services = await _context.Services
                .Include(s => s.ServiceCategory)
                .Where(s => serviceIds.Contains(s.ServiceId) && !s.IsDeleted)
                .ToDictionaryAsync(s => s.ServiceId);

            var result = topServices
                .Where(x => services.ContainsKey(x.ServiceId))
                .Select(x => new TopServiceItem
                {
                    ServiceId = x.ServiceId,
                    Title = services[x.ServiceId].Title,
                    ServiceCode = services[x.ServiceId].ServiceCode,
                    UsageCount = x.UsageCount,
                    TotalRevenue = x.TotalRevenue,
                    ServiceCategoryTitle = services[x.ServiceId].ServiceCategory.Title
                })
                .ToList();

            _log.Information(
                "دریافت لیست {Count} خدمات پرطرفدار در {DaysBack} روز گذشته با موفقیت انجام شد. User: {UserName} (Id: {UserId})",
                result.Count,
                daysBack,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return result;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت لیست خدمات پرطرفدار. TopCount: {TopCount}, DaysBack: {DaysBack}. User: {UserName} (Id: {UserId})",
                topCount,
                daysBack,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return new List<TopServiceItem>();
        }
    }

    /// <summary>
    /// دریافت همه خدمات یک دپارتمان برای Select2
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>لیست خدمات دپارتمان</returns>
    public async Task<ServiceResult<List<ViewModels.Insurance.InsuranceCalculation.ServiceLookupViewModel>>> GetServicesByDepartmentAsync(int departmentId)
    {
        try
        {
            _log.Information(
                "درخواست دریافت خدمات دپارتمان. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                departmentId, _currentUserService.UserName, _currentUserService.UserId);

            // دریافت همه خدمات فعال دپارتمان از طریق ServiceCategory
            var services = await _context.Services
                .Where(s => s.ServiceCategory.DepartmentId == departmentId && s.IsActive && !s.IsDeleted)
                .Select(s => new ViewModels.Insurance.InsuranceCalculation.ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title,
                    ServiceCode = s.ServiceCode
                })
                .OrderBy(s => s.Title)
                .ToListAsync();

            _log.Information(
                "دریافت {Count} خدمت برای دپارتمان {DepartmentId} با موفقیت انجام شد. User: {UserName} (Id: {UserId})",
                services.Count, departmentId, _currentUserService.UserName, _currentUserService.UserId);

            return ServiceResult<List<ViewModels.Insurance.InsuranceCalculation.ServiceLookupViewModel>>.Successful(services);
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "خطا در دریافت خدمات دپارتمان. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                departmentId, _currentUserService.UserName, _currentUserService.UserId);

            return ServiceResult<List<ViewModels.Insurance.InsuranceCalculation.ServiceLookupViewModel>>.Failed("خطا در دریافت خدمات دپارتمان");
        }
    }

    /// <summary>
    /// به‌روزرسانی قیمت خدمت بر اساس ServiceComponents
    /// این method بعد از ایجاد یا ویرایش ServiceComponents فراخوانی می‌شود
    /// </summary>
    /// <param name="serviceId">شناسه خدمت</param>
    /// <returns>نتیجه عملیات</returns>
    public async Task<ServiceResult<decimal>> UpdateServicePriceAsync(int serviceId)
    {
        try
        {
            _log.Information(
                "درخواست به‌روزرسانی قیمت خدمت. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                serviceId, _currentUserService.UserName, _currentUserService.UserId);

            // دریافت خدمت با ServiceComponents
            var service = await _context.Services
                .Include(s => s.ServiceComponents)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

            if (service == null)
            {
                _log.Warning(
                    "خدمت یافت نشد. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<decimal>.Failed("خدمت یافت نشد");
            }

            // محاسبه قیمت جدید
            var calculatedPrice = _serviceCalculationService.CalculateServicePrice(service);
            
            if (calculatedPrice != service.Price)
            {
                var oldPrice = service.Price;
                service.Price = calculatedPrice;
                service.UpdatedAt = _currentUserService.UtcNow;
                service.UpdatedByUserId = _currentUserService.UserId;

                await _context.SaveChangesAsync();

                _log.Information(
                    "قیمت خدمت به‌روزرسانی شد. ServiceId: {ServiceId}, OldPrice: {OldPrice}, NewPrice: {NewPrice}. User: {UserName} (Id: {UserId})",
                    serviceId, oldPrice, calculatedPrice, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<decimal>.Successful(calculatedPrice, "قیمت خدمت با موفقیت به‌روزرسانی شد");
            }
            else
            {
                _log.Information(
                    "قیمت خدمت تغییری نکرده است. ServiceId: {ServiceId}, Price: {Price}. User: {UserName} (Id: {UserId})",
                    serviceId, calculatedPrice, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<decimal>.Successful(calculatedPrice, "قیمت خدمت تغییری نکرده است");
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "خطا در به‌روزرسانی قیمت خدمت. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                serviceId, _currentUserService.UserName, _currentUserService.UserId);

            return ServiceResult<decimal>.Failed("خطا در به‌روزرسانی قیمت خدمت");
        }
    }

    /// <summary>
    /// دریافت خدمات بر اساس دسته‌بندی
    /// </summary>
    /// <param name="categoryId">شناسه دسته‌بندی</param>
    /// <returns>لیست خدمات</returns>
    public async Task<ServiceResult<List<ViewModels.Reception.ServiceLookupViewModel>>> GetServicesByCategoryAsync(int categoryId)
    {
        try
        {
            _log.Information("درخواست دریافت خدمات بر اساس دسته‌بندی. CategoryId: {CategoryId}", categoryId);

            var services = await _context.Services
                .Where(s => s.ServiceCategoryId == categoryId && s.IsActive && !s.IsDeleted)
                .Select(s => new ViewModels.Reception.ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.Title,
                    ServiceCode = s.ServiceCode,
                    BasePrice = s.Price,
                    CategoryId = s.ServiceCategoryId,
                    CategoryName = s.ServiceCategory.Title,
                    IsActive = s.IsActive,
                    Description = s.Description,
                    RequiresSpecialization = false,
                    RequiresDoctor = false,
                    DisplayName = s.Title
                })
                .ToListAsync();

            return ServiceResult<List<ViewModels.Reception.ServiceLookupViewModel>>.Successful(services);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در دریافت خدمات بر اساس دسته‌بندی. CategoryId: {CategoryId}", categoryId);
            return ServiceResult<List<ViewModels.Reception.ServiceLookupViewModel>>.Failed("خطا در دریافت خدمات");
        }
    }

    /// <summary>
    /// دریافت خدمت بر اساس شناسه
    /// </summary>
    /// <param name="serviceId">شناسه خدمت</param>
    /// <returns>خدمت</returns>
    public async Task<ServiceResult<ViewModels.Reception.ServiceLookupViewModel>> GetServiceByIdAsync(int serviceId)
    {
        try
        {
            _log.Information("درخواست دریافت خدمت. ServiceId: {ServiceId}", serviceId);

            var service = await _context.Services
                .Where(s => s.ServiceId == serviceId && s.IsActive && !s.IsDeleted)
                .Select(s => new ViewModels.Reception.ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.Title,
                    ServiceCode = s.ServiceCode,
                    BasePrice = s.Price,
                    CategoryId = s.ServiceCategoryId,
                    CategoryName = s.ServiceCategory.Title,
                    IsActive = s.IsActive,
                    Description = s.Description,
                    RequiresSpecialization = false,
                    RequiresDoctor = false,
                    DisplayName = s.Title
                })
                .FirstOrDefaultAsync();

            if (service == null)
            {
                return ServiceResult<ViewModels.Reception.ServiceLookupViewModel>.Failed("خدمت یافت نشد");
            }

            return ServiceResult<ViewModels.Reception.ServiceLookupViewModel>.Successful(service);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در دریافت خدمت. ServiceId: {ServiceId}", serviceId);
            return ServiceResult<ViewModels.Reception.ServiceLookupViewModel>.Failed("خطا در دریافت خدمت");
        }
    }

}

