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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس مدیریت کلینیک‌ها برای سیستم‌های پزشکی
    /// این سرویس تمام عملیات مربوط به کلینیک‌ها از جمله ایجاد، ویرایش، حذف و جستجو را پشتیبانی می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. رعایت کامل اصول Soft Delete برای حفظ اطلاعات پزشکی
    /// 2. پیاده‌سازی سیستم ردیابی کامل (Audit Trail) با ذخیره اطلاعات کاربر انجام‌دهنده عملیات
    /// 3. استفاده از زمان UTC برای تمام تاریخ‌ها به منظور رعایت استانداردهای بین‌المللی
    /// 4. مدیریت تراکنش‌های پایگاه داده برای اطمینان از یکپارچگی داده‌ها
    /// 5. اعمال قوانین کسب‌وکار پزشکی در تمام سطوح
    /// </summary>
    public class ClinicService : IClinicService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// سازنده سرویس کلینیک‌ها
        /// </summary>
        public ClinicService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _log = logger.ForContext<ClinicService>();
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// ایجاد یک کلینیک جدید
        /// </summary>
        public async Task<ServiceResult<int>> CreateClinicAsync(ClinicCreateEditViewModel model)
        {
            _log.Information("درخواست ایجاد کلینیک جدید با نام {Name}. User: {UserName} (Id: {UserId})",
                model.Name,
                _currentUserService.UserName,
                _currentUserService.UserId);

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var clinic = _mapper.Map<Clinic>(model);

                    // تنظیم فیلدهای ردیابی (Audit Trail)
                    clinic.CreatedAt = DateTime.UtcNow;
                    clinic.UpdatedAt = DateTime.UtcNow; // اضافه کردن این خط
                    clinic.CreatedById = _currentUserService.UserId;
                    clinic.IsDeleted = false;
                    clinic.IsActive = true;
                    

                    _context.Clinics.Add(clinic);
                    await _context.SaveChangesAsync();

                    transaction.Commit();

                    _log.Information(
                        "کلینیک جدید با موفقیت ایجاد شد. ClinicId: {ClinicId}, CreatedBy: {CreatedBy}",
                        clinic.ClinicId,
                        _currentUserService.UserId);

                    return ServiceResult<int>.Successful(
                        clinic.ClinicId,
                        "کلینیک با موفقیت ایجاد شد.");
                }
                catch (Exception ex)
                {
                    _log.Error(
                        ex,
                        "خطا در ایجاد کلینیک جدید. Name: {Name}, CreatedBy: {CreatedBy}",
                        model.Name,
                        _currentUserService.UserId);

                    transaction.Rollback();
                    return ServiceResult<int>.Failed("خطای سیستم رخ داده است. عملیات لغو شد.");
                }
            }
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات کلینیک
        /// </summary>
        public async Task<ServiceResult> UpdateClinicAsync(ClinicCreateEditViewModel model)
        {
            _log.Information("درخواست به‌روزرسانی کلینیک با شناسه {ClinicId}", model.ClinicId);

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // دریافت کلینیک موجود با فیلتر Soft Delete
                    var existingClinic = await _context.Clinics
                        .FirstOrDefaultAsync(c => c.ClinicId == model.ClinicId && !c.IsDeleted);

                    if (existingClinic == null)
                    {
                        _log.Warning("درخواست ویرایش برای کلینیک غیرموجود یا حذف شده. ClinicId: {ClinicId}", model.ClinicId);
                        return ServiceResult.Failed("کلینیک مورد نظر پیدا نشد یا حذف شده است.");
                    }

                    // به‌روزرسانی کلینیک
                    _mapper.Map(model, existingClinic);
                    existingClinic.UpdatedAt = DateTime.UtcNow;
                    existingClinic.UpdatedById = _currentUserService.UserId;

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    _log.Information(
                        "کلینیک با موفقیت به‌روزرسانی شد. ClinicId: {ClinicId}, UpdatedBy: {UpdatedBy}",
                        model.ClinicId,
                        _currentUserService.UserId);

                    return ServiceResult.Successful("اطلاعات کلینیک با موفقیت به‌روزرسانی شد.");
                }
                catch (Exception ex)
                {
                    _log.Error(
                        ex,
                        "خطای غیرمنتظره در به‌روزرسانی کلینیک. ClinicId: {ClinicId}, UpdatedBy: {UpdatedBy}",
                        model.ClinicId,
                        _currentUserService.UserId);

                    transaction.Rollback();
                    return ServiceResult.Failed("خطای سیستم در حین ذخیره تغییرات رخ داده است.");
                }
            }
        }

        /// <summary>
        /// حذف نرم یک کلینیک
        /// </summary>
        public async Task<ServiceResult> DeleteClinicAsync(int clinicId)
        {
            _log.Information("درخواست حذف کلینیک با شناسه {ClinicId}", clinicId);

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // دریافت کلینیک موجود با فیلتر Soft Delete
                    var clinic = await _context.Clinics
                        .FirstOrDefaultAsync(c => c.ClinicId == clinicId && !c.IsDeleted);

                    if (clinic == null)
                    {
                        _log.Warning("درخواست حذف برای کلینیک غیرموجود یا حذف شده. ClinicId: {ClinicId}", clinicId);
                        return ServiceResult.Failed("کلینیک مورد نظر پیدا نشد یا قبلاً حذف شده است.");
                    }

                    // بررسی وجود دپارتمان‌های فعال مرتبط
                    var activeDepartments = await _context.Departments
                        .CountAsync(d => d.ClinicId == clinicId && !d.IsDeleted);

                    if (activeDepartments > 0)
                    {
                        _log.Warning("حذف کلینیک با شناسه {ClinicId} امکان‌پذیر نیست چون {DepartmentCount} دپارتمان فعال دارد",
                            clinicId,
                            activeDepartments);

                        return ServiceResult.Failed("امکان حذف کلینیک وجود ندارد چون دپارتمان‌های فعالی دارد.");
                    }

                    // تنظیمات Soft Delete برای کلینیک
                    clinic.IsDeleted = true;
                    clinic.DeletedAt = DateTime.UtcNow;
                    clinic.DeletedById = _currentUserService.UserId;

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    _log.Information(
                        "کلینیک با موفقیت حذف شد. ClinicId: {ClinicId}, DeletedBy: {DeletedBy}",
                        clinicId,
                        _currentUserService.UserId);

                    return ServiceResult.Successful("کلینیک با موفقیت حذف شد.");
                }
                catch (Exception ex)
                {
                    _log.Error(
                        ex,
                        "خطا در حذف کلینیک. ClinicId: {ClinicId}, DeletedBy: {DeletedBy}",
                        clinicId,
                        _currentUserService.UserId);

                    transaction.Rollback();
                    return ServiceResult.Failed("خطای سیستم در حین حذف کلینیک رخ داده است.");
                }
            }
        }

        /// <summary>
        /// دریافت اطلاعات کلینیک برای ویرایش
        /// </summary>
        public async Task<ServiceResult<ClinicCreateEditViewModel>> GetClinicForEditAsync(int clinicId)
        {
            _log.Information("درخواست دریافت اطلاعات کلینیک برای ویرایش. ClinicId: {ClinicId}", clinicId);

            try
            {
                // فیلتر Soft Delete برای جلوگیری از نمایش کلینیک‌های حذف شده
                var clinicViewModel = await _context.Clinics
                    .Where(c => c.ClinicId == clinicId && !c.IsDeleted)
                    .ProjectTo<ClinicCreateEditViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                if (clinicViewModel == null)
                {
                    _log.Warning("درخواست ویرایش برای کلینیک غیرموجود یا حذف شده. ClinicId: {ClinicId}", clinicId);
                    return ServiceResult<ClinicCreateEditViewModel>.Failed("کلینیک مورد نظر پیدا نشد یا حذف شده است.");
                }

                return ServiceResult<ClinicCreateEditViewModel>.Successful(clinicViewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت اطلاعات کلینیک برای ویرایش. ClinicId: {ClinicId}", clinicId);
                return ServiceResult<ClinicCreateEditViewModel>.Failed("خطای سیستم رخ داده است.");
            }
        }

        /// <summary>
        /// دریافت جزئیات کامل کلینیک
        /// </summary>
        public async Task<ServiceResult<ClinicDetailsViewModel>> GetClinicDetailsAsync(int clinicId)
        {
            _log.Information("درخواست دریافت جزئیات کلینیک. ClinicId: {ClinicId}", clinicId);

            try
            {
                // فیلتر Soft Delete برای جلوگیری از نمایش کلینیک‌های حذف شده
                var clinicDetails = await _context.Clinics
                    .Where(c => c.ClinicId == clinicId && !c.IsDeleted)
                    .ProjectTo<ClinicDetailsViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                if (clinicDetails == null)
                {
                    _log.Warning("درخواست جزئیات برای کلینیک غیرموجود یا حذف شده. ClinicId: {ClinicId}", clinicId);
                    return ServiceResult<ClinicDetailsViewModel>.Failed("کلینیک مورد نظر پیدا نشد یا حذف شده است.");
                }

                return ServiceResult<ClinicDetailsViewModel>.Successful(clinicDetails);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت جزئیات کلینیک. ClinicId: {ClinicId}", clinicId);
                return ServiceResult<ClinicDetailsViewModel>.Failed("خطای سیستم رخ داده است.");
            }
        }

        /// <summary>
        /// جستجو و صفحه‌بندی کلینیک‌ها
        /// </summary>
        public async Task<ServiceResult<PagedResult<ClinicIndexViewModel>>> SearchClinicsAsync(string searchTerm, int pageNumber, int pageSize)
        {
            _log.Information("درخواست جستجوی کلینیک‌ها. Term: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}", searchTerm, pageNumber, pageSize);

            try
            {
                // شروع با تمام کلینیک‌های فعال (غیرحذف شده)
                var query = _context.Clinics
                    .Where(c => !c.IsDeleted)
                    .AsQueryable();

                // اعمال فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(c =>
                        c.Name.Contains(searchTerm) ||
                        (c.Address != null && c.Address.Contains(searchTerm)) ||
                        (c.PhoneNumber != null && c.PhoneNumber.Contains(searchTerm))
                    );
                }

                // محاسبه تعداد کل آیتم‌ها
                var totalItems = await query.CountAsync();

                // اعمال صفحه‌بندی و تبدیل به مدل نمایشی
                var items = await query
                    .OrderBy(c => c.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ProjectTo<ClinicIndexViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                // محاسبه تعداد دپارتمان‌ها و پزشکان برای هر کلینیک
                foreach (var item in items)
                {
                    item.DepartmentCount = await _context.Departments
                        .CountAsync(d => d.ClinicId == item.ClinicId && !d.IsDeleted);

                    item.DoctorCount = await _context.Doctors
                        .CountAsync(d => d.ClinicId == item.ClinicId && !d.IsDeleted);
                }

                // ایجاد نتیجه صفحه‌بندی شده
                var pagedResult = new PagedResult<ClinicIndexViewModel>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };

                _log.Information("جستجوی کلینیک‌ها با موفقیت انجام شد. Found: {TotalItems}", totalItems);
                return ServiceResult<PagedResult<ClinicIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در جستجوی کلینیک‌ها. Term: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}", searchTerm, pageNumber, pageSize);
                return ServiceResult<PagedResult<ClinicIndexViewModel>>.Failed("خطای سیستم در حین جستجو رخ داده است.");
            }
        }
    }
}