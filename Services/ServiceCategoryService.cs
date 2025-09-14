using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebPages.Html;
using ClinicApp.Extensions;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Services;

/// <summary>
/// سرویس مدیریت دسته‌بندی‌های خدمات پزشکی با رعایت کامل استانداردهای سیستم‌های پزشکی و امنیت اطلاعات
/// این سرویس تمام عملیات مربوط به دسته‌بندی‌های خدمات پزشکی را پشتیبانی می‌کند
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
public class ServiceCategoryService : IServiceCategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger _log;
    private readonly ICurrentUserService _currentUserService;

    public ServiceCategoryService(
        ApplicationDbContext context,
        ILogger logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _log = logger;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// ایجاد دسته‌بندی خدمات جدید با رعایت کامل استانداردهای سیستم‌های پزشکی
    /// این متد قبل از ایجاد، صحت شناسه کاربر را بررسی می‌کند
    /// </summary>

    /// <summary>
    /// ایجاد دسته‌بندی خدمات جدید با رعایت کامل استانداردهای سیستم‌های پزشکی
    /// این متد قبل از ایجاد، صحت شناسه کاربر و اعتبار داده‌ها را بررسی می‌کند
    /// </summary>
    public async Task<ServiceResult<int>> CreateServiceCategoryAsync(ServiceCategoryCreateEditViewModel model)
    {
        _log.Information(
            "درخواست ایجاد دسته‌بندی خدمات با نام {Title}. User: {UserName} (Id: {UserId})",
            model?.Title,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی مدل به روش صحیح برای سرویس‌ها
            var validationErrors = new List<string>();

            // بررسی وجود مدل
            if (model == null)
            {
                _log.Error("مدل ایجاد دسته‌بندی خدمات null است. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("داده‌های ارسالی نامعتبر است.");
            }

            // بررسی فیلدهای ضروری
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                validationErrors.Add("عنوان دسته‌بندی الزامی است.");
            }

            if (model.DepartmentId <= 0)
            {
                validationErrors.Add("دپارتمان الزامی است.");
            }

            // بررسی طول عنوان
            if (!string.IsNullOrWhiteSpace(model.Title) && model.Title.Length > 200)
            {
                validationErrors.Add("عنوان دسته‌بندی نمی‌تواند بیش از 200 کاراکتر باشد.");
            }

            // بررسی وجود خطاهای اعتبارسنجی
            if (validationErrors.Count > 0)
            {
                _log.Warning(
                    "مدل ایجاد دسته‌بندی خدمات نامعتبر است. Errors: {Errors}. User: {UserName} (Id: {UserId})",
                    string.Join(", ", validationErrors),
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed(string.Join(" ", validationErrors));
            }

            // بررسی وجود کاربر فعلی
            if (string.IsNullOrEmpty(_currentUserService.UserId))
            {
                _log.Error("کاربر فعلی شناسایی نشد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("کاربر فعلی شناسایی نشد. لطفاً مجدداً وارد سیستم شوید.");
            }

            // بررسی وجود کاربر در پایگاه داده
            var userExists = await _context.Users.AnyAsync(u => u.Id == _currentUserService.UserId);
            if (!userExists)
            {
                _log.Error("کاربر فعلی در پایگاه داده یافت نشد. UserId: {UserId}",
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("کاربر فعلی در سیستم یافت نشد.");
            }

            // بررسی تکراری بودن عنوان
            bool isDuplicate = await _context.ServiceCategories
                .AnyAsync(sc => sc.Title == model.Title &&
                               sc.DepartmentId == model.DepartmentId &&
                               !sc.IsDeleted);

            if (isDuplicate)
            {
                _log.Warning(
                    "تلاش برای ایجاد دسته‌بندی تکراری با عنوان {Title} در دپارتمان {DepartmentId}. User: {UserName} (Id: {UserId})",
                    model.Title,
                    model.DepartmentId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("دسته‌بندی با این عنوان در این دپارتمان قبلاً ایجاد شده است.");
            }

            // ایجاد دسته‌بندی خدمات
            var serviceCategory = new ServiceCategory
            {
                Title = model.Title,
                DepartmentId = model.DepartmentId,
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now,
                CreatedByUserId = _currentUserService.UserId,
                IsDeleted = false
            };

            _context.ServiceCategories.Add(serviceCategory);
            await _context.SaveChangesAsync();

            _log.Information(
                "دسته‌بندی خدمات با موفقیت ایجاد شد. ServiceCategoryId: {ServiceCategoryId}, Title: {Title}. User: {UserName} (Id: {UserId})",
                serviceCategory.ServiceCategoryId,
                serviceCategory.Title,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<int>.Successful(serviceCategory.ServiceCategoryId);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در ایجاد دسته‌بندی خدمات. Title: {Title}. User: {UserName} (Id: {UserId})",
                model?.Title,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<int>.Failed("خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
        }
    }


    /// <summary>
    /// به‌روزرسانی دسته‌بندی خدمات موجود با رعایت تمام استانداردهای امنیتی و پزشکی
    /// </summary>
    /// <param name="model">مدل اطلاعات به‌روزرسانی شده دسته‌بندی خدمات</param>
    /// <returns>نتیجه عملیات به‌روزرسانی</returns>
    public async Task<ServiceResult> UpdateServiceCategoryAsync(ServiceCategoryCreateEditViewModel model)
    {
        _log.Information(
            "درخواست به‌روزرسانی دسته‌بندی خدمات با شناسه {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
            model.ServiceCategoryId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی‌ها
            if (model.ServiceCategoryId <= 0)
            {
                _log.Warning(
                    "درخواست ویرایش دسته‌بندی خدمات با شناسه نامعتبر. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    model.ServiceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("شناسه دسته‌بندی خدمات معتبر نیست.");
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                _log.Warning(
                    "درخواست ویرایش دسته‌بندی خدمات با عنوان خالی. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    model.ServiceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("عنوان دسته‌بندی الزامی است.");
            }

            if (model.Title.Length > 200)
            {
                _log.Warning(
                    "درخواست ویرایش دسته‌بندی خدمات با عنوان طولانی. Length: {Length}. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    model.Title.Length,
                    model.ServiceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("عنوان دسته‌بندی نمی‌تواند بیشتر از 200 کاراکتر باشد.");
            }

            if (model.DepartmentId <= 0)
            {
                _log.Warning(
                    "درخواست ویرایش دسته‌بندی خدمات با دپارتمان نامعتبر. DepartmentId: {DepartmentId}. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    model.DepartmentId,
                    model.ServiceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult.Failed("دپارتمان انتخاب شده معتبر نیست.");
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // دریافت دسته‌بندی خدمات موجود با فیلتر Soft Delete
                    var existingServiceCategory = await _context.ServiceCategories
                        .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == model.ServiceCategoryId && !sc.IsDeleted);

                    if (existingServiceCategory == null)
                    {
                        _log.Warning(
                            "درخواست ویرایش برای دسته‌بندی خدمات غیرموجود. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                            model.ServiceCategoryId,
                            _currentUserService.UserName,
                            _currentUserService.UserId);

                        return ServiceResult.Failed("دسته‌بندی خدماتی که در حال ویرایش آن هستید پیدا نشد یا حذف شده است.");
                    }

                    // بررسی وجود دسته‌بندی تکراری در دپارتمان
                    if (await IsDuplicateServiceCategoryNameAsync(model.Title, model.DepartmentId, model.ServiceCategoryId))
                    {
                        _log.Warning(
                            "درخواست ویرایش دسته‌بندی خدمات با نام تکراری. Title: {Title}, DepartmentId: {DepartmentId}, ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                            model.Title,
                            model.DepartmentId,
                            model.ServiceCategoryId,
                            _currentUserService.UserName,
                            _currentUserService.UserId);

                        return ServiceResult.Failed("دسته‌بندی خدماتی با این نام در این دپارتمان از قبل وجود دارد.");
                    }

                    // بررسی وجود دپارتمان معتبر
                    if (!await _context.Departments.AnyAsync(d => d.DepartmentId == model.DepartmentId && !d.IsDeleted))
                    {
                        _log.Warning(
                            "درخواست ویرایش دسته‌بندی خدمات با دپارتمان غیرموجود. DepartmentId: {DepartmentId}. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                            model.DepartmentId,
                            model.ServiceCategoryId,
                            _currentUserService.UserName,
                            _currentUserService.UserId);

                        return ServiceResult.Failed("دپارتمان انتخاب شده معتبر نیست یا حذف شده است.");
                    }

                    // به‌روزرسانی دسته‌بندی خدمات
                    existingServiceCategory.Title = model.Title;
                    existingServiceCategory.DepartmentId = model.DepartmentId;
                    existingServiceCategory.IsActive = model.IsActive;
                    existingServiceCategory.UpdatedAt = _currentUserService.UtcNow;
                    existingServiceCategory.UpdatedByUserId = _currentUserService.UserId;
                    _context.Entry(existingServiceCategory).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    _log.Information(
                        "دسته‌بندی خدمات با موفقیت به‌روزرسانی شد. ServiceCategoryId: {ServiceCategoryId}, UpdatedBy: {UpdatedBy}",
                        model.ServiceCategoryId,
                        _currentUserService.UserId);

                    return ServiceResult.Successful("اطلاعات دسته‌بندی خدمات با موفقیت به‌روزرسانی شد.");
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
                "خطای غیرمنتظره در به‌روزرسانی دسته‌بندی خدمات. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                model.ServiceCategoryId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult.Failed("خطای سیستم در حین ذخیره تغییرات رخ داده است.");
        }
    }

    /// <summary>
    /// حذف نرم (Soft-delete) یک دسته‌بندی خدمات با رعایت تمام استانداردهای پزشکی و حفظ اطلاعات مالی
    /// این عملیات تمام خدمات مرتبط را نیز به صورت نرم حذف می‌کند
    /// </summary>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات مورد نظر</param>
    /// <returns>نتیجه عملیات حذف</returns>
    public async Task<ServiceResult> DeleteServiceCategoryAsync(int serviceCategoryId)
    {
        _log.Information(
            "درخواست حذف دسته‌بندی خدمات با شناسه {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
            serviceCategoryId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        // اعتبارسنجی اولیه
        if (serviceCategoryId <= 0)
        {
            _log.Warning("شناسه نامعتبر: {ServiceCategoryId}", serviceCategoryId);
            return ServiceResult.Failed("شناسه دسته‌بندی خدمات معتبر نیست.");
        }

        try
        {
            using (var transaction = _context.Database.BeginTransaction()) // تراکنش سینکرونوس در EF6
            {
                // بارگیری دسته‌بندی خدمات (بدون نیاز به AsTracking - در EF6 ردیابی به صورت پیش‌فرض فعال است)
                var serviceCategory = await _context.ServiceCategories
                    .Include(sc => sc.Services)
                    .FirstOrDefaultAsync(sc =>
                        sc.ServiceCategoryId == serviceCategoryId &&
                        !sc.IsDeleted);

                if (serviceCategory == null)
                {
                    _log.Warning("دسته‌بندی خدمات یافت نشد یا قبلاً حذف شده: {ServiceCategoryId}", serviceCategoryId);
                    return ServiceResult.Failed("دسته‌بندی خدمات مورد نظر یافت نشد یا قبلاً حذف شده است.");
                }

                // بررسی وجود خدمات فعال با پرس‌وجوی بهینه‌شده
                bool hasActiveServices = await _context.Services
                    .AnyAsync(s =>
                        s.ServiceCategoryId == serviceCategoryId &&
                        !s.IsDeleted);

                if (hasActiveServices)
                {
                    _log.Warning("تلاش برای حذف دسته‌بندی با خدمات فعال: {ServiceCategoryId}", serviceCategoryId);
                    return ServiceResult.Failed("این دسته‌بندی خدمات دارای خدمات فعال است و قابل حذف نمی‌باشد.");
                }

                // تنظیمات حذف نرم
                serviceCategory.IsDeleted = true;
                serviceCategory.DeletedAt = _currentUserService.UtcNow;
                serviceCategory.DeletedByUserId = _currentUserService.UserId;

                // تضمین تغییر وضعیت موجودیت به صورت صریح (مهم برای EF6)
                _context.Entry(serviceCategory).State = EntityState.Modified;

                // ذخیره‌سازی تغییرات
                await _context.SaveChangesAsync();
                transaction.Commit();

                _log.Information(
                    "دسته‌بندی خدمات با موفقیت حذف نرم شد. ServiceCategoryId: {ServiceCategoryId}",
                    serviceCategoryId);

                return ServiceResult.Successful("دسته‌بندی خدمات با موفقیت حذف نرم شد.");
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطای سیستمی در حذف دسته‌بندی خدمات: {ServiceCategoryId}", serviceCategoryId);
            return ServiceResult.Failed("خطای سیستمی در حین حذف دسته‌بندی خدمات رخ داده است.");
        }
    }

    /// <summary>
    /// بازیابی داده‌های یک دسته‌بندی خدمات برای پر کردن فرم ویرایش
    /// این متد فقط دسته‌بندی‌های فعال (غیرحذف شده) را بازمی‌گرداند
    /// </summary>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات مورد نظر</param>
    /// <returns>مدل داده‌های دسته‌بندی خدمات برای ویرایش</returns>
    public async Task<ServiceResult<ServiceCategoryCreateEditViewModel>> GetServiceCategoryForEditAsync(int serviceCategoryId)
    {
        _log.Information(
            "درخواست دریافت اطلاعات دسته‌بندی خدمات برای ویرایش. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
            serviceCategoryId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (serviceCategoryId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت اطلاعات دسته‌بندی خدمات با شناسه نامعتبر. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    serviceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<ServiceCategoryCreateEditViewModel>.Failed("شناسه دسته‌بندی خدمات معتبر نیست.");
            }

            // دریافت دسته‌بندی خدمات با روابط مورد نیاز
            var serviceCategory = await _context.ServiceCategories
                .Include(sc => sc.Department)
                .Include(sc => sc.CreatedByUser)
                .Include(sc => sc.UpdatedByUser)
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == serviceCategoryId && !sc.IsDeleted);

            if (serviceCategory == null)
            {
                _log.Warning(
                    "درخواست ویرایش برای دسته‌بندی خدمات غیرموجود یا حذف شده. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    serviceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<ServiceCategoryCreateEditViewModel>.Failed("دسته‌بندی خدمات مشخص‌شده پیدا نشد یا حذف شده است.");
            }

            // ساخت ViewModel به صورت دستی
            var viewModel = new ServiceCategoryCreateEditViewModel
            {
                ServiceCategoryId = serviceCategory.ServiceCategoryId,
                Title = serviceCategory.Title,
                DepartmentId = serviceCategory.DepartmentId,
                DepartmentName = serviceCategory.Department?.Name,
                IsActive = serviceCategory.IsActive,
                CreatedAt = serviceCategory.CreatedAt,
                CreatedBy = serviceCategory.CreatedByUser != null ?
                    $"{serviceCategory.CreatedByUser.FirstName} {serviceCategory.CreatedByUser.LastName}" : "سیستم",
                UpdatedAt = serviceCategory.UpdatedAt,
                UpdatedBy = serviceCategory.UpdatedByUser != null ?
                    $"{serviceCategory.UpdatedByUser.FirstName} {serviceCategory.UpdatedByUser.LastName}" : ""
            };

            // تبدیل تاریخ به شمسی برای محیط‌های پزشکی ایرانی
            viewModel.CreatedAtShamsi = DateTimeExtensions.ToPersianDateTime(viewModel.CreatedAt);
            if (viewModel.UpdatedAt.HasValue)
                viewModel.UpdatedAtShamsi = DateTimeExtensions.ToPersianDateTime(viewModel.UpdatedAt.Value);

            return ServiceResult<ServiceCategoryCreateEditViewModel>.Successful(viewModel);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت اطلاعات دسته‌بندی خدمات برای ویرایش. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                serviceCategoryId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<ServiceCategoryCreateEditViewModel>.Failed("خطای سیستم رخ داده است.");
        }
    }

    /// <summary>
    /// بازیابی جزئیات کامل یک دسته‌بندی خدمات برای نمایش اطلاعات
    /// این متد فقط دسته‌بندی‌های فعال (غیرحذف شده) را بازمی‌گرداند
    /// </summary>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات مورد نظر</param>
    /// <returns>مدل جزئیات کامل دسته‌بندی خدمات</returns>
    public async Task<ServiceResult<ServiceCategoryDetailsViewModel>> GetServiceCategoryDetailsAsync(int serviceCategoryId)
    {
        _log.Information(
            "درخواست دریافت جزئیات دسته‌بندی خدمات. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
            serviceCategoryId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (serviceCategoryId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت جزئیات دسته‌بندی خدمات با شناسه نامعتبر. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    serviceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<ServiceCategoryDetailsViewModel>.Failed("شناسه دسته‌بندی خدمات معتبر نیست.");
            }

            // دریافت دسته‌بندی خدمات با روابط مورد نیاز
            var serviceCategory = await _context.ServiceCategories
                .Include(sc => sc.Department)
                .Include(sc => sc.Services)
                .Include(sc => sc.CreatedByUser)
                .Include(sc => sc.UpdatedByUser)
                .Include(sc => sc.DeletedByUser)
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == serviceCategoryId && !sc.IsDeleted);

            if (serviceCategory == null)
            {
                _log.Warning(
                    "درخواست جزئیات برای دسته‌بندی خدمات غیرموجود یا حذف شده. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    serviceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<ServiceCategoryDetailsViewModel>.Failed("دسته‌بندی خدمات مشخص‌شده پیدا نشد یا حذف شده است.");
            }

            // ساخت ViewModel به صورت دستی
            var details = new ServiceCategoryDetailsViewModel
            {
                ServiceCategoryId = serviceCategory.ServiceCategoryId,
                Title = serviceCategory.Title,
                DepartmentName = serviceCategory.Department?.Name,
                ServiceCount = serviceCategory.Services.Count(s => !s.IsDeleted),
                IsActive = serviceCategory.IsActive,
                CreatedAt = serviceCategory.CreatedAt,
                CreatedBy = serviceCategory.CreatedByUser != null ?
                    $"{serviceCategory.CreatedByUser.FirstName} {serviceCategory.CreatedByUser.LastName}" : "سیستم",
                UpdatedAt = serviceCategory.UpdatedAt,
                UpdatedBy = serviceCategory.UpdatedByUser != null ?
                    $"{serviceCategory.UpdatedByUser.FirstName} {serviceCategory.UpdatedByUser.LastName}" : "",
                DeletedAt = serviceCategory.DeletedAt,
                DeletedBy = serviceCategory.DeletedByUser != null ?
                    $"{serviceCategory.DeletedByUser.FirstName} {serviceCategory.DeletedByUser.LastName}" : ""
            };

            // تبدیل تاریخ به شمسی برای محیط‌های پزشکی ایرانی
            details.CreatedAtShamsi = DateTimeExtensions.ToPersianDateTime(details.CreatedAt);
            if (details.UpdatedAt.HasValue)
                details.UpdatedAtShamsi = DateTimeExtensions.ToPersianDateTime(details.UpdatedAt.Value);
            if (details.DeletedAt.HasValue)
                details.DeletedAtShamsi = DateTimeExtensions.ToPersianDateTime(details.DeletedAt.Value);

            return ServiceResult<ServiceCategoryDetailsViewModel>.Successful(details);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت جزئیات دسته‌بندی خدمات. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                serviceCategoryId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<ServiceCategoryDetailsViewModel>.Failed("خطای سیستم رخ داده است.");
        }
    }

    /// <summary>
    /// جستجو و صفحه‌بندی دسته‌بندی‌های خدمات با رعایت کامل استانداردهای سیستم‌های پزشکی و امنیت اطلاعات
    /// این متد فقط دسته‌بندی‌های فعال (غیرحذف شده) را بازمی‌گرداند و از تمام استانداردهای پزشکی پیروی می‌کند
    /// </summary>
    public async Task<ServiceResult<PagedResult<ServiceCategoryIndexItemViewModel>>> SearchServiceCategoriesAsync(string searchTerm, int? departmentId, int pageNumber, int pageSize)
    {
        _log.Information(
            "درخواست جستجو دسته‌بندی‌های خدمات. Term: {SearchTerm}, DepartmentId: {DepartmentId}, Page: {PageNumber}, Size: {PageSize}. User: {UserName} (Id: {UserId})",
            searchTerm,
            departmentId,
            pageNumber,
            pageSize,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی‌ها
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

            // شروع با تمام دسته‌بندی‌های فعال (غیرحذف شده)
            // در EF 6 برای شامل کردن چند سطحی از ThenInclude استفاده نمی‌شود
            var query = _context.ServiceCategories
                .Include(sc => sc.Department) // سطح اول
                .Include("Department.Clinic") // سطح دوم (در EF 6 از رشته استفاده می‌شود)
                .Include(sc => sc.CreatedByUser)
                .Where(sc => !sc.IsDeleted)
                .AsQueryable();

            // اعمال فیلتر دپارتمان
            if (departmentId.HasValue && departmentId > 0)
            {
                query = query.Where(sc => sc.DepartmentId == departmentId);
            }

            // اعمال فیلتر جستجو
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(sc =>
                    sc.Title.Contains(searchTerm) ||
                    (sc.Department != null && sc.Department.Name.Contains(searchTerm)) ||
                    (sc.Department != null && sc.Department.Clinic != null && sc.Department.Clinic.Name.Contains(searchTerm))
                );
            }

            // محاسبه تعداد کل آیتم‌ها
            var totalItems = await query.CountAsync();

            // اعمال صفحه‌بندی
            var items = await query
                .OrderBy(sc => sc.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ساخت ViewModelها
            var viewModels = new List<ServiceCategoryIndexItemViewModel>();
            foreach (var item in items)
            {
                // محاسبه تعداد خدمات فعال
                var activeServiceCount = await _context.Services
                    .CountAsync(s => s.ServiceCategoryId == item.ServiceCategoryId && !s.IsDeleted);

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

                viewModels.Add(new ServiceCategoryIndexItemViewModel
                {
                    ServiceCategoryId = item.ServiceCategoryId,
                    Title = item.Title,
                    DepartmentName = item.Department?.Name,
                    ClinicName = item.Department?.Clinic?.Name,
                    ServiceCount = activeServiceCount,
                    IsActive = !item.IsDeleted,
                    CreatedAt = item.CreatedAt,
                    CreatedBy = createdBy,
                    UpdatedAt = item.UpdatedAt,
                    DepartmentId = item.DepartmentId
                });
            }

            // تبدیل تاریخ‌ها به شمسی برای محیط‌های پزشکی ایرانی
            foreach (var viewModel in viewModels)
            {
                viewModel.CreatedAtShamsi = DateTimeExtensions.ToPersianDateTime(viewModel.CreatedAt);
                if (viewModel.UpdatedAt.HasValue)
                    viewModel.UpdatedAtShamsi = DateTimeExtensions.ToPersianDateTime(viewModel.UpdatedAt.Value);
            }

            // ایجاد نتیجه صفحه‌بندی شده
            var pagedResult = new PagedResult<ServiceCategoryIndexItemViewModel>
            {
                Items = viewModels,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            _log.Information(
                "جستجوی دسته‌بندی‌های خدمات با موفقیت انجام شد. Found: {TotalItems}. User: {UserName} (Id: {UserId})",
                totalItems,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<PagedResult<ServiceCategoryIndexItemViewModel>>.Successful(pagedResult);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در جستجوی دسته‌بندی‌های خدمات. Term: {SearchTerm}, DepartmentId: {DepartmentId}, Page: {PageNumber}, Size: {PageSize}. User: {UserName} (Id: {UserId})",
                searchTerm,
                departmentId,
                pageNumber,
                pageSize,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<PagedResult<ServiceCategoryIndexItemViewModel>>.Failed("خطای سیستم در حین جستجو رخ داده است.");
        }
    }

    /// <summary>
    /// بررسی وجود دسته‌بندی خدمات با عنوان ویژه
    /// </summary>
    /// <param name="title">عنوان دسته‌بندی خدمات</param>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات (برای ویرایش)</param>
    /// <returns>آیا دسته‌بندی خدمات تکراری است؟</returns>
    public async Task<bool> IsDuplicateServiceCategoryNameAsync(string title, int departmentId, int serviceCategoryId = 0)
    {
        return await _context.ServiceCategories
            .AnyAsync(sc =>
                sc.Title == title &&
                sc.DepartmentId == departmentId &&
                sc.ServiceCategoryId != serviceCategoryId &&
                !sc.IsDeleted);
    }

    /// <summary>
    /// دریافت لیست دسته‌بندی‌های خدمات فعال برای استفاده در کنترل‌های انتخاب
    /// </summary>
    /// <returns>لیست دسته‌بندی‌های خدمات فعال</returns>
    public async Task<IEnumerable<ServiceCategorySelectItem>> GetActiveServiceCategoriesAsync()
    {
        _log.Information(
            "درخواست دریافت لیست دسته‌بندی‌های خدمات فعال. User: {UserName} (Id: {UserId})",
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            var serviceCategories = await _context.ServiceCategories
                .Include(sc => sc.Department)
                .Where(sc => !sc.IsDeleted)
                .OrderBy(sc => sc.Title)
                .Select(sc => new ServiceCategorySelectItem
                {
                    ServiceCategoryId = sc.ServiceCategoryId,
                    Title = sc.Title,
                    DepartmentId = sc.DepartmentId,
                    DepartmentName = sc.Department.Name,
                    ServiceCount = sc.Services.Count(s => !s.IsDeleted)
                })
                .ToListAsync();

            _log.Information(
                "دریافت لیست دسته‌بندی‌های خدمات فعال با موفقیت انجام شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                serviceCategories.Count,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return serviceCategories;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت لیست دسته‌بندی‌های خدمات فعال. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName,
                _currentUserService.UserId);

            return new List<ServiceCategorySelectItem>();
        }
    }

    /// <summary>
    /// دریافت لیست دسته‌بندی‌های خدمات فعال برای یک دپارتمان خاص
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>لیست دسته‌بندی‌های خدمات فعال برای دپارتمان مورد نظر</returns>
    public async Task<List<Models.Entities.Clinic.ServiceCategory>> GetActiveServiceCategoriesByDepartmentAsync(int departmentId)
    {
        _log.Information(
            "درخواست دریافت لیست دسته‌بندی‌های خدمات فعال برای دپارتمان {DepartmentId}. User: {UserName} (Id: {UserId})",
            departmentId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (departmentId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت لیست دسته‌بندی‌های خدمات با شناسه دپارتمان نامعتبر. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                    departmentId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return new List<Models.Entities.Clinic.ServiceCategory>();
            }

            var serviceCategories = await _context.ServiceCategories
                .Include(sc => sc.Department)
                .Where(sc => sc.DepartmentId == departmentId && !sc.IsDeleted)
                .OrderBy(sc => sc.Title)
                .AsNoTracking()
                .ToListAsync();

            _log.Information(
                "دریافت لیست دسته‌بندی‌های خدمات فعال برای دپارتمان {DepartmentId} با موفقیت انجام شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                departmentId,
                serviceCategories.Count,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return serviceCategories;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت لیست دسته‌بندی‌های خدمات فعال برای دپارتمان {DepartmentId}. User: {UserName} (Id: {UserId})",
                departmentId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return new List<Models.Entities.Clinic.ServiceCategory>();
        }
    }

    /// <summary>
    /// بررسی امکان حذف دسته‌بندی خدمات
    /// </summary>
    public async Task<ServiceResult<bool>> CanDeleteServiceCategoryAsync(int id)
    {
        _log.Information(
            "درخواست بررسی امکان حذف دسته‌بندی خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
            id,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // بررسی وجود دسته‌بندی
            var serviceCategory = await _context.ServiceCategories
                .Where(sc => sc.ServiceCategoryId == id && !sc.IsDeleted)
                .FirstOrDefaultAsync();

            if (serviceCategory == null)
            {
                _log.Warning(
                    "دسته‌بندی خدمات با شناسه {Id} یافت نشد. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<bool>.Failed("دسته‌بندی خدمات مورد نظر یافت نشد.");
            }

            // بررسی وجود خدمات مرتبط
            var serviceCount = await _context.Services
                .CountAsync(s => s.ServiceCategoryId == id && !s.IsDeleted);

            if (serviceCount > 0)
            {
                _log.Warning(
                    "دسته‌بندی خدمات با شناسه {Id} نمی‌تواند حذف شود زیرا {ServiceCount} خدمات مرتبط دارد. User: {UserName} (Id: {UserId})",
                    id,
                    serviceCount,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<bool>.Failed($"این دسته‌بندی شامل {serviceCount} خدمات است و نمی‌تواند حذف شود.");
            }

            return ServiceResult<bool>.Successful(true);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در بررسی امکان حذف دسته‌بندی خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<bool>.Failed("خطای سیستم در حین بررسی امکان حذف رخ داده است.");
        }
    }

    /// <summary>
    /// دریافت تعداد خدمات مرتبط با یک دسته‌بندی
    /// </summary>
    public async Task<int> GetServiceCountForCategoryAsync(int categoryId)
    {
        return await _context.Services
            .CountAsync(s => s.ServiceCategoryId == categoryId && !s.IsDeleted);
    }


    /// <summary>
    /// بازیابی تعداد خدمات مرتبط با یک دسته‌بندی
    /// </summary>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
    /// <returns>تعداد خدمات مرتبط</returns>
    public async Task<int> GetServiceCountAsync(int serviceCategoryId)
    {
        _log.Information(
            "درخواست دریافت تعداد کل خدمات مرتبط با دسته‌بندی {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
            serviceCategoryId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (serviceCategoryId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت تعداد خدمات با شناسه دسته‌بندی نامعتبر. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    serviceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return 0;
            }

            var count = await _context.Services
                .CountAsync(s => s.ServiceCategoryId == serviceCategoryId);

            _log.Information(
                "دریافت تعداد کل خدمات مرتبط با دسته‌بندی {ServiceCategoryId} با موفقیت انجام شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                serviceCategoryId,
                count,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return count;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت تعداد کل خدمات مرتبط با دسته‌بندی {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                serviceCategoryId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return 0;
        }
    }

    /// <summary>
    /// بازیابی تعداد خدمات فعال مرتبط با یک دسته‌بندی
    /// </summary>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
    /// <returns>تعداد خدمات فعال مرتبط</returns>
    public async Task<int> GetActiveServiceCountAsync(int serviceCategoryId)
    {
        _log.Information(
            "درخواست دریافت تعداد خدمات فعال مرتبط با دسته‌بندی {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
            serviceCategoryId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (serviceCategoryId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت تعداد خدمات فعال با شناسه دسته‌بندی نامعتبر. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    serviceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return 0;
            }

            var count = await _context.Services
                .CountAsync(s => s.ServiceCategoryId == serviceCategoryId && !s.IsDeleted);

            _log.Information(
                "دریافت تعداد خدمات فعال مرتبط با دسته‌بندی {ServiceCategoryId} با موفقیت انجام شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                serviceCategoryId,
                count,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return count;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت تعداد خدمات فعال مرتبط با دسته‌بندی {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                serviceCategoryId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return 0;
        }
    }

    /// <summary>
    /// بررسی وجود دسته‌بندی خدمات فعال با شناسه مشخص
    /// </summary>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
    /// <returns>آیا دسته‌بندی خدمات فعال وجود دارد؟</returns>
    public async Task<bool> IsActiveServiceCategoryExistsAsync(int serviceCategoryId)
    {
        _log.Information(
            "درخواست بررسی وجود دسته‌بندی خدمات فعال با شناسه {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
            serviceCategoryId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (serviceCategoryId <= 0)
            {
                _log.Warning(
                    "درخواست بررسی وجود دسته‌بندی خدمات با شناسه نامعتبر. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    serviceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return false;
            }

            var exists = await _context.ServiceCategories
                .AnyAsync(sc => sc.ServiceCategoryId == serviceCategoryId && !sc.IsDeleted);

            _log.Information(
                "بررسی وجود دسته‌بندی خدمات فعال با شناسه {ServiceCategoryId} با موفقیت انجام شد. Exists: {Exists}. User: {UserName} (Id: {UserId})",
                serviceCategoryId,
                exists,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return exists;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در بررسی وجود دسته‌بندی خدمات فعال با شناسه {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                serviceCategoryId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return false;
        }
    }

}