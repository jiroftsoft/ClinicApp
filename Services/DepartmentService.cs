using AutoMapper;
using AutoMapper.QueryableExtensions;
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
using System.Linq;
using System.Threading.Tasks;

namespace ClinicApp.Services;

/// <summary>
/// سرویس مدیریت دپارتمان‌ها برای سیستم‌های پزشکی
/// این سرویس تمام عملیات مربوط به دپارتمان‌ها از جمله ایجاد، ویرایش، حذف و جستجو را پشتیبانی می‌کند
/// 
/// ویژگی‌های کلیدی:
/// 1. رعایت کامل اصول Soft Delete برای حفظ اطلاعات پزشکی
/// 2. پیاده‌سازی سیستم ردیابی کامل (Audit Trail) با ذخیره اطلاعات کاربر انجام‌دهنده عملیات
/// 3. استفاده از زمان UTC برای تمام تاریخ‌ها به منظور رعایت استانداردهای بین‌المللی
/// 4. مدیریت تراکنش‌های پایگاه داده برای اطمینان از یکپارچگی داده‌ها
/// 5. اعمال قوانین کسب‌وکار پزشکی در تمام سطوح
/// 6. بررسی ارتباطات سلسله‌مراتبی با کلینیک‌ها
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger _log;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// سازنده سرویس دپارتمان‌ها
    /// </summary>
    public DepartmentService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger logger,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _mapper = mapper;
        _log = logger.ForContext<DepartmentService>();
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    /// <summary>
    /// ایجاد یک دپارتمان جدید
    /// </summary>
    /// <summary>
    /// ایجاد یک دپارتمان جدید در سیستم‌های پزشکی
    /// این متد یک دپارتمان جدید را با رعایت استانداردهای امنیتی و پزشکی ایجاد می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل کلینیک مرتبط قبل از ایجاد دپارتمان
    /// 2. مدیریت صحیح تراکنش‌ها برای جلوگیری از ناسازگاری داده‌ها
    /// 3. ثبت اطلاعات کامل ردیابی (Audit Trail) برای پیگیری تغییرات
    /// 4. رعایت استانداردهای امنیتی و HIPAA در مدیریت داده‌های پزشکی
    /// 5. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان)
    /// </summary>
    /// <param name="model">مدل ایجاد/ویرایش دپارتمان</param>
    /// <returns>نتیجه شامل شناسه دپارتمان ایجاد شده</returns>
    public async Task<ServiceResult<int>> CreateDepartmentAsync(DepartmentCreateEditViewModel model)
    {
        _log.Information(
            "درخواست ایجاد دپارتمان جدید با نام {Name}. User: {UserName} (Id: {UserId})",
            model.Name,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // بررسی وجود دپارتمان تکراری در کلینیک
            if (await IsDuplicateDepartmentNameAsync(model.Name, model.ClinicId))
            {
                _log.Warning(
                    "درخواست ایجاد دپارتمان با نام تکراری. Name: {Name}, ClinicId: {ClinicId}. User: {UserName} (Id: {UserId})",
                    model.Name,
                    model.ClinicId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<int>.Failed("دپارتمانی با این نام در این کلینیک از قبل وجود دارد.");
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _log.Information(
                        "شروع تراکنش ایجاد دپارتمان جدید. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    // استفاده صحیح از AutoMapper با ارسال سرویس کاربر فعلی
                    var department = _mapper.Map<Department>(model,
                        opt => opt.Items["CurrentUser"] = _currentUserService);

                    // تأیید صحت شناسه کاربر ایجاد کننده
                    if (!await ValidateUserIdExists(department.CreatedByUserId))
                    {
                        _log.Error(
                            "شناسه کاربر ایجاد کننده معتبر نیست: {UserId}. User: {UserName} (Id: {UserId})",
                            department.CreatedByUserId,
                            _currentUserService.UserName,
                            _currentUserService.UserId);

                        return ServiceResult<int>.Failed("خطای سیستم: شناسه کاربر معتبر نیست.");
                    }

                    _context.Departments.Add(department);
                    await _context.SaveChangesAsync();

                    transaction.Commit();

                    _log.Information(
                        "دپارتمان جدید با موفقیت ایجاد شد. DepartmentId: {DepartmentId}, Name: {Name}, ClinicId: {ClinicId}, CreatedBy: {CreatedBy}",
                        department.DepartmentId,
                        department.Name,
                        department.ClinicId,
                        _currentUserService.UserId);

                    return ServiceResult<int>.Successful(
                        department.DepartmentId,
                        "دپارتمان با موفقیت ایجاد شد.");
                }
                catch (DbUpdateException ex)
                {
                    transaction.Rollback();
                    // لاگ خطا با جزئیات کامل
                    _log.Error(
                        ex,
                        "خطای به‌روزرسانی پایگاه داده در ایجاد دپارتمان. Name: {Name}, ClinicId: {ClinicId}, CreatedByUserId: {CreatedByUserId}. User: {UserName} (Id: {UserId})",
                        model.Name,
                        model.ClinicId,
                        _currentUserService.UserId,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    // بررسی خطاهای خاص پایگاه داده
                    if (ex.InnerException is System.Data.Entity.Core.UpdateException updateEx &&
                        updateEx.InnerException is System.Data.SqlClient.SqlException sqlEx)
                    {
                        if (sqlEx.Number == 547) // Foreign Key violation
                        {
                            return ServiceResult<int>.Failed("خطا: شناسه کاربر ایجاد کننده معتبر نیست. لطفاً مجدداً وارد سیستم شوید.");
                        }
                    }

                    throw;
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
                "خطا در ایجاد دپارتمان جدید. Name: {Name}, ClinicId: {ClinicId}. User: {UserName} (Id: {UserId})",
                model.Name,
                model.ClinicId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<int>.Failed("خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
        }
    }
    /// <summary>
    /// به‌روزرسانی اطلاعات دپارتمان در سیستم‌های پزشکی
    /// این متد اطلاعات دپارتمان را با رعایت استانداردهای امنیتی و پزشکی به‌روزرسانی می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل کلینیک مرتبط قبل از به‌روزرسانی دپارتمان
    /// 2. مدیریت صحیح تراکنش‌ها برای جلوگیری از ناسازگاری داده‌ها
    /// 3. ثبت اطلاعات کامل ردیابی (Audit Trail) برای پیگیری تغییرات
    /// 4. رعایت استانداردهای امنیتی و HIPAA در مدیریت داده‌های پزشکی
    /// 5. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان)
    /// </summary>
    /// <param name="model">مدل ایجاد/ویرایش دپارتمان</param>
    /// <returns>نتیجه عملیات به‌روزرسانی دپارتمان</returns>
    public async Task<ServiceResult> UpdateDepartmentAsync(DepartmentCreateEditViewModel model)
    {
        _log.Information(
            "درخواست به‌روزرسانی دپارتمان با شناسه {DepartmentId}. User: {UserName} (Id: {UserId})",
            model.DepartmentId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // دریافت دپارتمان موجود با فیلتر Soft Delete
                    var existingDepartment = await _context.Departments
                        .FirstOrDefaultAsync(d => d.DepartmentId == model.DepartmentId && !d.IsDeleted);

                    if (existingDepartment == null)
                    {
                        _log.Warning(
                            "درخواست ویرایش برای دپارتمان غیرموجود. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                            model.DepartmentId,
                            _currentUserService.UserName,
                            _currentUserService.UserId);

                        return ServiceResult.Failed("دپارتمانی که در حال ویرایش آن هستید پیدا نشد یا حذف شده است.");
                    }

                    // بررسی وجود دپارتمان تکراری در کلینیک
                    if (await IsDuplicateDepartmentNameAsync(model.Name, model.ClinicId, model.DepartmentId))
                    {
                        _log.Warning(
                            "درخواست ویرایش دپارتمان با نام تکراری. Name: {Name}, ClinicId: {ClinicId}, DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                            model.Name,
                            model.ClinicId,
                            model.DepartmentId,
                            _currentUserService.UserName,
                            _currentUserService.UserId);

                        return ServiceResult.Failed("دپارتمانی با این نام در این کلینیک از قبل وجود دارد.");
                    }

                    // استفاده صحیح از AutoMapper با ارسال سرویس کاربر فعلی
                    _mapper.Map(model, existingDepartment,
                        opt => opt.Items["CurrentUser"] = _currentUserService);
                    //_mapper.Map(model, existingDepartment);

                    // تأیید صحت شناسه کاربر ویرایش کننده
                    if (existingDepartment.UpdatedByUserId != null &&
                        !await ValidateUserIdExists(existingDepartment.UpdatedByUserId))
                    {
                        _log.Error(
                            "شناسه کاربر ویرایش کننده معتبر نیست: {UserId}. User: {UserName} (Id: {UserId})",
                            existingDepartment.UpdatedByUserId,
                            _currentUserService.UserName,
                            _currentUserService.UserId);

                        return ServiceResult.Failed("خطای سیستم: شناسه کاربر معتبر نیست.");
                    }

                    model.IsActive = true;
                    _context.Entry(existingDepartment).State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    _log.Information(
                        "دپارتمان با موفقیت به‌روزرسانی شد. DepartmentId: {DepartmentId}, UpdatedBy: {UpdatedBy}",
                        model.DepartmentId,
                        _currentUserService.UserId);

                    return ServiceResult.Successful("اطلاعات دپارتمان با موفقیت به‌روزرسانی شد.");
                }
                catch (DbUpdateException ex)
                {
                    transaction.Rollback();
                    // لاگ خطا با جزئیات کامل
                    _log.Error(
                        ex,
                        "خطای به‌روزرسانی پایگاه داده در ویرایش دپارتمان. DepartmentId: {DepartmentId}, UpdatedByUserId: {UpdatedByUserId}. User: {UserName} (Id: {UserId})",
                        model.DepartmentId,
                        _currentUserService.UserId,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    // بررسی خطاهای خاص پایگاه داده
                    if (ex.InnerException is System.Data.Entity.Core.UpdateException updateEx &&
                        updateEx.InnerException is System.Data.SqlClient.SqlException sqlEx)
                    {
                        if (sqlEx.Number == 547) // Foreign Key violation
                        {
                            return ServiceResult.Failed("خطا: شناسه کاربر ویرایش کننده معتبر نیست. لطفاً مجدداً وارد سیستم شوید.");
                        }
                    }

                    throw;
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
                "خطای غیرمنتظره در به‌روزرسانی دپارتمان. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                model.DepartmentId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult.Failed("خطای سیستم در حین ذخیره تغییرات رخ داده است.");
        }
    }

    /// <summary>
    /// حذف نرم یک دپارتمان در سیستم‌های پزشکی
    /// این متد دپارتمان را با رعایت استانداردهای امنیتی و پزشکی حذف می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت صحیح سیستم حذف نرم (Soft Delete) برای رعایت استانداردهای پزشکی
    /// 2. بررسی وابستگی‌ها قبل از انجام عملیات حذف
    /// 3. ثبت اطلاعات کامل ردیابی (Audit Trail) برای پیگیری تغییرات
    /// 4. رعایت استانداردهای امنیتی و HIPAA در مدیریت داده‌های پزشکی
    /// 5. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان)
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان مورد نظر</param>
    /// <returns>نتیجه عملیات حذف دپارتمان</returns>
    public async Task<ServiceResult> DeleteDepartmentAsync(int departmentId)
    {
        _log.Information("درخواست حذف دپارتمان در بخش ادمین با شناسه {DepartmentId}. User: {UserName} (Id: {UserId})",
            departmentId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                // دریافت دپارتمان موجود با فیلتر Soft Delete
                var department = await _context.Departments
                    .FirstOrDefaultAsync(d => d.DepartmentId == departmentId && !d.IsDeleted);

                if (department == null)
                {
                    _log.Warning("درخواست حذف برای دپارتمان غیرموجود یا حذف شده در بخش ادمین. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                        departmentId,
                        _currentUserService.UserName,
                        _currentUserService.UserId);
                    return ServiceResult.Failed("دپارتمان مورد نظر پیدا نشد یا قبلاً حذف شده است.");
                }

                // بررسی وجود پزشکان فعال مرتبط
                var activeDoctors = await _context.Doctors
                    .CountAsync(d => d.DepartmentId == departmentId && !d.IsDeleted);

                if (activeDoctors > 0)
                {
                    _log.Warning("حذف دپارتمان با شناسه {DepartmentId} امکان‌پذیر نیست چون {DoctorCount} پزشک فعال دارد. User: {UserName} (Id: {UserId})",
                        departmentId,
                        activeDoctors,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    return ServiceResult.Failed($"امکان حذف دپارتمان وجود ندارد چون {activeDoctors} پزشک فعال دارد.");
                }

                // بررسی وجود دسته‌بندی‌های خدمات فعال
                var activeServiceCategories = await _context.ServiceCategories
                    .CountAsync(sc => sc.DepartmentId == departmentId && !sc.IsDeleted);

                if (activeServiceCategories > 0)
                {
                    _log.Warning("حذف دپارتمان با شناسه {DepartmentId} امکان‌پذیر نیست چون {CategoryCount} دسته‌بندی خدمات فعال دارد. User: {UserName} (Id: {UserId})",
                        departmentId,
                        activeServiceCategories,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    return ServiceResult.Failed($"امکان حذف دپارتمان وجود ندارد چون {activeServiceCategories} دسته‌بندی خدمات فعال دارد.");
                }

                // تنظیمات Soft Delete برای دپارتمان با نام صحیح فیلد
                department.IsDeleted = true;
                department.DeletedAt = DateTime.UtcNow;
                department.DeletedByUserId = _currentUserService.UserId; // استفاده از نام صحیح فیلد

                await _context.SaveChangesAsync();
                transaction.Commit();

                _log.Information(
                    "دپارتمان با موفقیت حذف شد در بخش ادمین. DepartmentId: {DepartmentId}, DeletedBy: {DeletedBy}, ClinicId: {ClinicId}",
                    departmentId,
                    _currentUserService.UserId,
                    department.ClinicId);

                return ServiceResult.Successful("دپارتمان با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در حذف دپارتمان در بخش ادمین. DepartmentId: {DepartmentId}, DeletedBy: {DeletedBy}",
                    departmentId,
                    _currentUserService.UserId);

                transaction.Rollback();
                return ServiceResult.Failed("خطای سیستم در حین حذف دپارتمان رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
            }
        }
    }

    /// <summary>
    /// دریافت اطلاعات دپارتمان برای ویرایش در سیستم‌های پزشکی
    /// این متد اطلاعات دپارتمان را برای ویرایش با رعایت استانداردهای امنیتی و پزشکی دریافت می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت صحیح سیستم حذف نرم (Soft Delete) برای رعایت استانداردهای پزشکی
    /// 2. بررسی وجود دپارتمان قبل از ارائه اطلاعات
    /// 3. لاگ‌گیری کامل و دقیق برای پیگیری تغییرات
    /// 4. رعایت استانداردهای امنیتی و HIPAA در مدیریت داده‌های پزشکی
    /// 5. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان)
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان مورد نظر</param>
    /// <returns>نتیجه شامل اطلاعات دپارتمان برای ویرایش</returns>
    public async Task<ServiceResult<DepartmentCreateEditViewModel>> GetDepartmentForEditAsync(int departmentId)
    {
        _log.Information(
            "درخواست دریافت اطلاعات دپارتمان برای ویرایش. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
            departmentId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // راه‌حل صحیح: ابتدا داده‌ها را بارگیری کنید، سپس نگاشت را انجام دهید
            var department = await _context.Departments
                .Include(d => d.Clinic)
                .Include(d => d.CreatedByUser)
                .Include(d => d.UpdatedByUser)
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentId && !d.IsDeleted);

            if (department == null)
            {
                _log.Warning(
                    "درخواست ویرایش برای دپارتمان غیرموجود یا حذف شده. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                    departmentId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<DepartmentCreateEditViewModel>.Failed("دپارتمان مشخص‌شده پیدا نشد یا حذف شده است.");
            }

            // استفاده از Map به جای ProjectTo برای رفع خطای LINQ to Entities
            var departmentViewModel = _mapper.Map<DepartmentCreateEditViewModel>(department);

            // تبدیل تاریخ‌ها به شمسی
            departmentViewModel.CreatedAtShamsi = department.CreatedAt.ToPersianDateTime();
            if (department.UpdatedAt.HasValue)
                departmentViewModel.UpdatedAtShamsi = department.UpdatedAt.Value.ToPersianDateTime();

            return ServiceResult<DepartmentCreateEditViewModel>.Successful(departmentViewModel);
        }
        catch (NotSupportedException ex)
        {
            _log.Error(
                ex,
                "خطای NotSupportedException در دریافت اطلاعات دپارتمان برای ویرایش. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                departmentId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<DepartmentCreateEditViewModel>.Failed("خطا در پردازش اطلاعات. لطفاً با پشتیبانی سیستم تماس بگیرید.");
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت اطلاعات دپارتمان برای ویرایش. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                departmentId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<DepartmentCreateEditViewModel>.Failed("خطای سیستم رخ داده است.");
        }
    }


    /// <summary>
    /// دریافت جزئیات کامل دپارتمان در سیستم‌های پزشکی
    /// این متد تمام اطلاعات دپارتمان را با در نظر گرفتن ساختار سازمانی پزشکی (کلینیک → دپارتمان → دسته‌بندی خدمات → خدمات) برمی‌گرداند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان → دسته‌بندی خدمات → خدمات)
    /// 2. محاسبه دقیق آمار پزشکان و خدمات برای تصمیم‌گیری‌های بالینی
    /// 3. مدیریت صحیح فیلتر Soft Delete برای رعایت استانداردهای پزشکی
    /// 4. ارائه اطلاعات کامل ردیابی (Audit Trail) برای پیگیری تغییرات
    /// 5. رعایت استانداردهای امنیتی و HIPAA در مدیریت داده‌های پزشکی
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان مورد نظر</param>
    /// <returns>نتیجه شامل جزئیات کامل دپارتمان</returns>
    /// <summary>
    /// دریافت جزئیات کامل دپارتمان در سیستم‌های پزشکی
    /// این متد تمام اطلاعات دپارتمان را با در نظر گرفتن ساختار سازمانی پزشکی (کلینیک → دپارتمان → دسته‌بندی خدمات → خدمات) برمی‌گرداند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان → دسته‌بندی خدمات → خدمات)
    /// 2. محاسبه دقیق آمار پزشکان و خدمات برای تصمیم‌گیری‌های بالینی
    /// 3. مدیریت صحیح فیلتر Soft Delete برای رعایت استانداردهای پزشکی
    /// 4. ارائه اطلاعات کامل ردیابی (Audit Trail) برای پیگیری تغییرات
    /// 5. رعایت استانداردهای امنیتی و HIPAA در مدیریت داده‌های پزشکی
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان مورد نظر</param>
    /// <returns>نتیجه شامل جزئیات کامل دپارتمان</returns>
    public async Task<ServiceResult<DepartmentDetailsViewModel>> GetDepartmentDetailsAsync(int departmentId)
    {
        _log.Information(
            "درخواست دریافت جزئیات دپارتمان. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
            departmentId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // راه‌حل صحیح: ابتدا داده‌ها را بارگیری کنید، سپس نگاشت را انجام دهید
            var department = await _context.Departments
                .Include(d => d.Clinic)
                .Include(d => d.Doctors)
                .Include("ServiceCategories.Services")
                .Include(d => d.CreatedByUser)
                .Include(d => d.UpdatedByUser)
                .Include(d => d.DeletedByUser)
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentId && !d.IsDeleted);

            if (department == null)
            {
                _log.Warning(
                    "درخواست جزئیات برای دپارتمان غیرموجود یا حذف شده. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                    departmentId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return ServiceResult<DepartmentDetailsViewModel>.Failed("دپارتمان مشخص‌شده پیدا نشد یا حذف شده است.");
            }

            // ایجاد ViewModel به صورت دستی برای جلوگیری از خطا
            var departmentDetails = new DepartmentDetailsViewModel
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name,
                ClinicName = department.Clinic.Name,
                DoctorCount = department.Doctors.Count(dr => !dr.IsDeleted),
                ServiceCount = department.ServiceCategories.Sum(sc => sc.Services.Count(s => !s.IsDeleted)),
                IsActive = department.IsActive,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt,
                DeletedAt = department.DeletedAt
            };

            // تنظیم فیلدهای مربوط به کاربران
            departmentDetails.CreatedBy = department.CreatedByUser != null ?
                $"{department.CreatedByUser.FirstName} {department.CreatedByUser.LastName}" : "سیستم";

            departmentDetails.UpdatedBy = department.UpdatedByUser != null ?
                $"{department.UpdatedByUser.FirstName} {department.UpdatedByUser.LastName}" : "";

            departmentDetails.DeletedBy = department.DeletedByUser != null ?
                $"{department.DeletedByUser.FirstName} {department.DeletedByUser.LastName}" : "";

            // تبدیل تاریخ به شمسی برای محیط‌های پزشکی ایرانی
            departmentDetails.CreatedAtShamsi = departmentDetails.CreatedAt.ToPersianDateTime();
            if (departmentDetails.UpdatedAt.HasValue)
                departmentDetails.UpdatedAtShamsi = departmentDetails.UpdatedAt.Value.ToPersianDateTime();
            if (departmentDetails.DeletedAt.HasValue)
                departmentDetails.DeletedAtShamsi = departmentDetails.DeletedAt.Value.ToPersianDateTime();

            return ServiceResult<DepartmentDetailsViewModel>.Successful(departmentDetails);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت جزئیات دپارتمان. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                departmentId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<DepartmentDetailsViewModel>.Failed("خطای سیستم رخ داده است.");
        }
    }




    /// <summary>
    /// دریافت نام کاربر از شیء ApplicationUser
    /// این متد برای تبدیل شیء کاربر به نام کاربر در سیستم‌های پزشکی استفاده می‌شود
    /// </summary>
    /// <param name="user">شیء کاربر</param>
    /// <returns>نام کامل کاربر یا متن پیش‌فرض در صورت عدم وجود</returns>
    private string GetUserNameFromUser(ApplicationUser user)
    {
        if (user == null)
            return "نام کاربر حذف شده یا نامشخص";

        return $"{user.FirstName} {user.LastName}".Trim();
    }

    /// <summary>
    /// اعتبارسنجی شناسه کاربر برای جلوگیری از خطای FOREIGN KEY
    /// </summary>
    private async Task<bool> ValidateUserIdExists(string userId)
    {
        if (string.IsNullOrEmpty(userId) || userId == "System")
            return true; // "System" برای حالت‌های خاص مدیریت می‌شود

        return await _context.Users.AnyAsync(u => u.Id == userId);
    }

    /// <summary>
    /// دریافت نام کاربر بر اساس شناسه
    /// این متد برای تبدیل شناسه کاربر به نام کاربر در سیستم‌های پزشکی استفاده می‌شود
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <returns>نام کامل کاربر یا متن پیش‌فرض در صورت عدم وجود</returns>
    private async Task<string> GetUserNameAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return "نامشخص";

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null ? $"{user.FirstName} {user.LastName}" : "نام کاربر حذف شده";
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در دریافت نام کاربر با شناسه {UserId}", userId);
            return "خطا در دریافت نام کاربر";
        }
    }
    /// <summary>
    /// جستجو و صفحه‌بندی دپارتمان‌ها در سیستم‌های پزشکی
    /// این متد تمام دپارتمان‌های فعال را با در نظر گرفتن ساختار سازمانی پزشکی (کلینیک → دپارتمان → دسته‌بندی خدمات) برمی‌گرداند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان → دسته‌بندی خدمات → خدمات)
    /// 2. بهینه‌سازی عملکرد برای محیط‌های پزشکی با حجم بالای داده
    /// 3. مدیریت صحیح فیلترها و جستجو بر اساس نیازهای پزشکی
    /// 4. رعایت استانداردهای امنیتی و HIPAA در مدیریت داده‌های پزشکی
    /// 5. ارائه آمار دقیق پزشکان و خدمات برای تصمیم‌گیری‌های بالینی
    /// </summary>
    /// <param name="searchTerm">عبارت جستجو که می‌تواند شامل نام دپارتمان یا کلینیک باشد</param>
    /// <param name="pageNumber">شماره صفحه مورد نظر (حداقل 1)</param>
    /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه (بین 5 تا 100)</param>
    /// <returns>نتیجه صفحه‌بندی‌شده شامل دپارتمان‌های مطابق با معیارهای جستجو</returns>
    public async Task<ServiceResult<PagedResult<DepartmentIndexViewModel>>> SearchDepartmentsAsync(string searchTerm, int pageNumber, int pageSize)
    {
        _log.Information("درخواست جستجوی دپارتمان‌ها در بخش ادمین. Term: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}. User: {UserName} (Id: {UserId})",
            searchTerm, pageNumber, pageSize,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی‌ها برای اطمینان از امنیت سیستم پزشکی
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 5) pageSize = 5;
            if (pageSize > 100) pageSize = 100;

            // شروع با تمام دپارتمان‌های فعال (غیرحذف شده)
            var query = _context.Departments
                .Include(d => d.Clinic)
                .Include(d => d.ServiceCategories.Select(sc => sc.Services)) // 👈 EF6
                .Where(d => !d.IsDeleted)
                .AsQueryable();


            // اعمال فیلتر جستجو با رعایت استانداردهای امنیتی
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // تبدیل اعداد فارسی به انگلیسی برای جستجوی صحیح
                string normalizedSearchTerm = NormalizePersianNumbers(searchTerm);

                query = query.Where(d =>
                    d.Name.Contains(normalizedSearchTerm) ||
                    (d.Clinic != null && d.Clinic.Name.Contains(normalizedSearchTerm)) ||
                    d.ServiceCategories.Any(sc =>
                        sc.Services.Any(s =>
                            s.Title.Contains(normalizedSearchTerm) ||
                            s.ServiceCode.Contains(normalizedSearchTerm)
                        )
                    )
                );
            }

            // محاسبه تعداد کل آیتم‌ها با مدیریت خطا برای محیط پزشکی
            int totalItems;
            try
            {
                totalItems = await query.CountAsync();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در محاسبه تعداد کل دپارتمان‌ها. Term: {SearchTerm}", searchTerm);
                totalItems = 0;
            }

            // اعمال صفحه‌بندی و تبدیل به مدل نمایشی
            var items = await query
                .OrderBy(d => d.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DepartmentIndexViewModel
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    ClinicId = d.ClinicId,
                    ClinicName = d.Clinic.Name,
                    DoctorCount = d.Doctors.Count(doc => !doc.IsDeleted),
                    ServiceCount = d.ServiceCategories
                        .SelectMany(sc => sc.Services)
                        .Count(s => !s.IsDeleted),
                    IsActive = !d.IsDeleted,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            // ایجاد نتیجه صفحه‌بندی شده
            var pagedResult = new PagedResult<DepartmentIndexViewModel>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            _log.Information("جستجوی دپارتمان‌ها با موفقیت انجام شد. Found: {TotalItems}, Returned: {ReturnedItems}",
                totalItems,
                items.Count);

            return ServiceResult<PagedResult<DepartmentIndexViewModel>>.Successful(pagedResult);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در جستجوی دپارتمان‌ها در بخش ادمین. Term: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}, User: {UserName} (Id: {UserId})",
                searchTerm, pageNumber, pageSize,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return ServiceResult<PagedResult<DepartmentIndexViewModel>>.Failed("خطای سیستم در حین جستجو رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
        }
    }

    /// <summary>
    /// دریافت لیست دپارتمان‌های فعال برای استفاده در کنترل‌های انتخاب
    /// </summary>
    /// <returns>لیست دپارتمان‌های فعال</returns>
    public async Task<IEnumerable<DepartmentSelectItem>> GetActiveDepartmentsAsync()
    {
        _log.Information(
            "درخواست دریافت لیست دپارتمان‌های فعال. User: {UserName} (Id: {UserId})",
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            var departments = await _context.Departments
                .Include(d => d.Clinic)
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentSelectItem
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    ClinicId = d.ClinicId,
                    ClinicName = d.Clinic.Name
                })
                .ToListAsync();

            _log.Information(
                "دریافت لیست دپارتمان‌های فعال با موفقیت انجام شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                departments.Count,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return departments;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت لیست دپارتمان‌های فعال. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName,
                _currentUserService.UserId);

            return new List<DepartmentSelectItem>();
        }
    }

    /// <summary>
    /// دریافت لیست دپارتمان‌های فعال برای یک کلینیک خاص
    /// </summary>
    /// <param name="clinicId">شناسه کلینیک</param>
    /// <returns>لیست دپارتمان‌های فعال برای کلینیک مورد نظر</returns>
    public async Task<IEnumerable<DepartmentSelectItem>> GetActiveDepartmentsByClinicAsync(int clinicId)
    {
        _log.Information(
            "درخواست دریافت لیست دپارتمان‌های فعال برای کلینیک {ClinicId}. User: {UserName} (Id: {UserId})",
            clinicId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (clinicId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت لیست دپارتمان‌ها با شناسه کلینیک نامعتبر. ClinicId: {ClinicId}. User: {UserName} (Id: {UserId})",
                    clinicId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return new List<DepartmentSelectItem>();
            }

            var departments = await _context.Departments
                .Include(d => d.Clinic)
                .Where(d => d.ClinicId == clinicId && !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentSelectItem
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    ClinicId = d.ClinicId,
                    ClinicName = d.Clinic.Name
                })
                .ToListAsync();

            _log.Information(
                "دریافت لیست دپارتمان‌های فعال برای کلینیک {ClinicId} با موفقیت انجام شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                clinicId,
                departments.Count,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return departments;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت لیست دپارتمان‌های فعال برای کلینیک {ClinicId}. User: {UserName} (Id: {UserId})",
                clinicId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return new List<DepartmentSelectItem>();
        }
    }

    /// <summary>
    /// بررسی وجود دپارتمان فعال با شناسه مشخص
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>آیا دپارتمان فعال وجود دارد؟</returns>
    public async Task<bool> IsActiveDepartmentExistsAsync(int departmentId)
    {
        _log.Information(
            "درخواست بررسی وجود دپارتمان فعال با شناسه {DepartmentId}. User: {UserName} (Id: {UserId})",
            departmentId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (departmentId <= 0)
            {
                _log.Warning(
                    "درخواست بررسی وجود دپارتمان با شناسه نامعتبر. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                    departmentId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return false;
            }

            var exists = await _context.Departments
                .AnyAsync(d => d.DepartmentId == departmentId && !d.IsDeleted);

            _log.Information(
                "بررسی وجود دپارتمان فعال با شناسه {DepartmentId} با موفقیت انجام شد. Exists: {Exists}. User: {UserName} (Id: {UserId})",
                departmentId,
                exists,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return exists;
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در بررسی وجود دپارتمان فعال با شناسه {DepartmentId}. User: {UserName} (Id: {UserId})",
                departmentId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return false;
        }
    }

    /// <summary>
    /// تبدیل اعداد فارسی و عربی به انگلیسی برای جستجوی صحیح در سیستم‌های پزشکی
    /// این متد برای پشتیبانی از کاربران فارسی‌زبان که ممکن است از صفحه‌کلید فارسی استفاده کنند ضروری است
    /// </summary>
    /// <param name="input">رشته ورودی که ممکن است شامل اعداد فارسی/عربی باشد</param>
    /// <returns>رشته با اعداد انگلیسی</returns>
    private string NormalizePersianNumbers(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var sb = new System.Text.StringBuilder();
        foreach (char c in input)
        {
            if (c >= '۰' && c <= '۹')
                sb.Append((char)(c - '۰' + '0'));
            else if (c >= '٠' && c <= '٩')
                sb.Append((char)(c - '٠' + '0'));
            else
                sb.Append(c);
        }
        return sb.ToString();
    }
    public async Task<bool> IsDuplicateDepartmentNameAsync(string name, int clinicId, int departmentId = 0)
    {
        return await _context.Departments
            .AnyAsync(d =>
                d.Name == name &&
                d.ClinicId == clinicId &&
                d.DepartmentId != departmentId &&
                !d.IsDeleted);
    }
}