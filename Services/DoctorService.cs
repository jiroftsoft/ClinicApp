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
    /// سرویس مدیریت پزشکان با رعایت کامل استانداردهای سیستم‌های پزشکی و امنیت اطلاعات
    /// این سرویس تمام عملیات مربوط به پزشکان از جمله ایجاد، ویرایش، حذف و جستجو را پشتیبانی می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. رعایت کامل اصول Soft Delete برای حفظ اطلاعات پزشکی
    /// 2. پیاده‌سازی سیستم ردیابی کامل (Audit Trail) با ذخیره اطلاعات کاربر انجام‌دهنده عملیات
    /// 3. استفاده از زمان UTC برای تمام تاریخ‌ها به منظور رعایت استانداردهای بین‌المللی
    /// 4. مدیریت تراکنش‌های پایگاه داده برای اطمینان از یکپارچگی داده‌ها
    /// 5. اعمال قوانین کسب‌وکار پزشکی در تمام سطوح
    /// </summary>
    public class DoctorService : IDoctorService
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationUserManager _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// سازنده سرویس پزشکان برای استفاده در محیط واقعی
        /// </summary>
        public DoctorService(
            ApplicationDbContext context,
            ApplicationUserManager userManager,
            IMapper mapper,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _log = logger;
            _currentUserService = currentUserService;
        }



        /// <summary>
        /// ایجاد یک پزشک جدید و حساب کاربری مربوطه با رعایت تمام استانداردهای امنیتی و پزشکی
        /// </summary>
        /// <param name="model">مدل اطلاعات پزشک جدید</param>
        /// <returns>نتیجه عملیات با شناسه پزشک ایجاد شده</returns>
        public async Task<ServiceResult<int>> CreateDoctorAsync(DoctorCreateEditViewModel model)
        {
            _log.Information("درخواست ایجاد پزشک جدید با نام {FirstName} {LastName}", model.FirstName, model.LastName);

            // اعتبارسنجی شماره تلفن بین‌المللی
            if (!IsValidInternationalPhoneNumber(model.PhoneNumber))
            {
                _log.Warning("شماره تلفن نامعتبر برای پزشک جدید: {PhoneNumber}", model.PhoneNumber);
                return ServiceResult<int>.Failed("شماره تلفن نامعتبر است. لطفاً از فرمت بین‌المللی استفاده کنید (مثال: +989123456789)");
            }

            // بررسی وجود شماره تلفن تکراری
            if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber))
            {
                _log.Warning("شماره تلفن تکراری برای پزشک جدید: {PhoneNumber}", model.PhoneNumber);
                return ServiceResult<int>.Failed("این شماره تلفن قبلاً ثبت شده است.");
            }

            var user = new ApplicationUser
            {
                UserName = model.PhoneNumber,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumberConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow // استفاده از زمان UTC برای تاریخ ایجاد
            };

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _log.Information("شروع تراکنش ایجاد پزشک جدید");

                    // ایجاد کاربر
                    var identityResult = await _userManager.CreateAsync(user);
                    if (!identityResult.Succeeded)
                    {
                        _log.Error("خطا در ایجاد کاربر پزشک: {Errors}", string.Join(", ", identityResult.Errors));
                        transaction.Rollback();
                        return ServiceResult<int>.Failed(string.Join(", ", identityResult.Errors));
                    }

                    // افزودن نقش پزشک
                    await _userManager.AddToRoleAsync(user.Id, AppRoles.Doctor);

                    // ایجاد پروفایل پزشک
                    var doctor = _mapper.Map<Doctor>(model);

                    // تنظیم فیلدهای ردیابی (Audit Trail)
                    doctor.ApplicationUserId = user.Id;
                    doctor.CreatedAt = DateTime.UtcNow; // استفاده از زمان UTC
                    doctor.CreatedByUserId = _currentUserService.UserId; // ثبت کاربر ایجاد کننده
                    doctor.IsDeleted = false;

                    _context.Doctors.Add(doctor);
                    await _context.SaveChangesAsync();

                    transaction.Commit();

                    _log.Information(
                        "پزشک جدید با موفقیت ایجاد شد. DoctorId: {DoctorId}, UserId: {UserId}, CreatedBy: {CreatedBy}",
                        doctor.DoctorId,
                        user.Id,
                        _currentUserService.UserId);

                    return ServiceResult<int>.Successful(
                        doctor.DoctorId,
                        "پزشک با موفقیت ایجاد شد.");
                }
                catch (Exception ex)
                {
                    _log.Error(
                        ex,
                        "خطا در ایجاد پزشک جدید. Phone: {PhoneNumber}, CreatedBy: {CreatedBy}",
                        model.PhoneNumber,
                        _currentUserService.UserId);

                    transaction.Rollback();
                    return ServiceResult<int>.Failed("خطای سیستم رخ داده است. عملیات لغو شد.");
                }
            }
        }

        /// <summary>
        /// به‌روزرسانی پروفایل یک پزشک موجود با رعایت تمام استانداردهای امنیتی و پزشکی
        /// </summary>
        /// <param name="model">مدل اطلاعات به‌روزرسانی شده پزشک</param>
        /// <returns>نتیجه عملیات به‌روزرسانی</returns>
        public async Task<ServiceResult> UpdateDoctorAsync(DoctorCreateEditViewModel model)
        {
            _log.Information("درخواست به‌روزرسانی پزشک با شناسه {DoctorId}", model.DoctorId);

            // اعتبارسنجی شماره تلفن بین‌المللی
            if (!IsValidInternationalPhoneNumber(model.PhoneNumber))
            {
                _log.Warning("شماره تلفن نامعتبر برای ویرایش پزشک {DoctorId}: {PhoneNumber}", model.DoctorId, model.PhoneNumber);
                return ServiceResult.Failed("شماره تلفن نامعتبر است. لطفاً از فرمت بین‌المللی استفاده کنید (مثال: +989123456789)");
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // دریافت پزشک موجود با فیلتر Soft Delete
                    var existingDoctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.DoctorId == model.DoctorId && !d.IsDeleted);

                    if (existingDoctor == null)
                    {
                        _log.Warning("درخواست ویرایش برای پزشک غیرموجود. DoctorId: {DoctorId}", model.DoctorId);
                        return ServiceResult.Failed("پزشکی که در حال ویرایش آن هستید پیدا نشد یا حذف شده است.");
                    }

                    // بررسی شماره تلفن تکراری
                    if (await _userManager.Users.AnyAsync(u =>
                        u.PhoneNumber == model.PhoneNumber &&
                        u.Id != existingDoctor.ApplicationUserId))
                    {
                        _log.Warning(
                            "شماره تلفن تکراری برای ویرایش پزشک {DoctorId}: {PhoneNumber}",
                            model.DoctorId,
                            model.PhoneNumber);

                        return ServiceResult.Failed("این شماره تلفن در حال حاضر متعلق به کاربر دیگری است.");
                    }

                    // به‌روزرسانی پروفایل پزشک
                    _mapper.Map(model, existingDoctor);
                    existingDoctor.UpdatedAt = DateTime.UtcNow; // زمان به‌روزرسانی با UTC
                    existingDoctor.UpdatedByUserId = _currentUserService.UserId; // کاربر به‌روزرسانی کننده

                    // به‌روزرسانی اطلاعات کاربر
                    var user = await _userManager.FindByIdAsync(existingDoctor.ApplicationUserId);
                    if (user != null)
                    {
                        user.FirstName = model.FirstName;
                        user.LastName = model.LastName;
                        user.PhoneNumber = model.PhoneNumber;
                        user.UserName = model.PhoneNumber;
                        user.UpdatedAt = DateTime.UtcNow; // زمان به‌روزرسانی کاربر با UTC

                        var userResult = await _userManager.UpdateAsync(user);
                        if (!userResult.Succeeded)
                        {
                            _log.Error("خطا در به‌روزرسانی اطلاعات کاربر پزشک {DoctorId}: {Errors}",
                                model.DoctorId,
                                string.Join(", ", userResult.Errors));

                            throw new Exception("خطا در به‌روزرسانی اطلاعات کاربر");
                        }
                    }

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    _log.Information(
                        "پروفایل پزشک با موفقیت به‌روزرسانی شد. DoctorId: {DoctorId}, UpdatedBy: {UpdatedBy}",
                        model.DoctorId,
                        _currentUserService.UserId);

                    return ServiceResult.Successful("اطلاعات پزشک با موفقیت به‌روزرسانی شد.");
                }
                catch (Exception ex)
                {
                    _log.Error(
                        ex,
                        "خطای غیرمنتظره در به‌روزرسانی پزشک. DoctorId: {DoctorId}, UpdatedBy: {UpdatedBy}",
                        model.DoctorId,
                        _currentUserService.UserId);

                    transaction.Rollback();
                    return ServiceResult.Failed("خطای سیستم در حین ذخیره تغییرات رخ داده است.");
                }
            }
        }

        /// <summary>
        /// حذف نرم (Soft-delete) یک پزشک با رعایت تمام استانداردهای پزشکی و حفظ اطلاعات مالی
        /// این عملیات تمام رکوردهای مرتبط را نیز به صورت نرم حذف می‌کند
        /// </summary>
        /// <param name="doctorId">شناسه پزشک مورد نظر</param>
        /// <returns>نتیجه عملیات حذف</returns>
        public async Task<ServiceResult> DeleteDoctorAsync(int doctorId)
        {
            _log.Information("درخواست حذف پزشک با شناسه {DoctorId}", doctorId);

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // دریافت پزشک موجود با فیلتر Soft Delete
                    var doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.DoctorId == doctorId && !d.IsDeleted);

                    if (doctor == null)
                    {
                        _log.Warning("درخواست حذف برای پزشک غیرموجود یا حذف شده. DoctorId: {DoctorId}", doctorId);
                        return ServiceResult.Failed("پزشکی که در حال حذف آن هستید پیدا نشد یا قبلاً حذف شده است.");
                    }

                    // تنظیمات Soft Delete برای پزشک
                    doctor.IsDeleted = true;
                    doctor.DeletedAt = DateTime.UtcNow; // زمان حذف با UTC
                    doctor.DeletedById = _currentUserService.UserId; // کاربر حذف کننده

                    // Soft Delete برای نوبت‌های فعال پزشک
                    var activeAppointments = await _context.Appointments
                        .Where(a => a.DoctorId == doctorId && !a.IsDeleted)
                        .ToListAsync();

                    foreach (var appointment in activeAppointments)
                    {
                        appointment.IsDeleted = true;
                        appointment.DeletedAt = DateTime.UtcNow;
                        appointment.DeletedById = _currentUserService.UserId;
                    }

                    // غیرفعال کردن حساب کاربری پزشک
                    var user = await _userManager.FindByIdAsync(doctor.ApplicationUserId);
                    if (user != null)
                    {
                        user.IsActive = false;
                        user.UpdatedAt = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                    }

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    _log.Information(
                        "پزشک با موفقیت حذف شد. DoctorId: {DoctorId}, DeletedBy: {DeletedBy}",
                        doctorId,
                        _currentUserService.UserId);

                    return ServiceResult.Successful("پزشک با موفقیت حذف شد.");
                }
                catch (Exception ex)
                {
                    _log.Error(
                        ex,
                        "خطا در حذف پزشک. DoctorId: {DoctorId}, DeletedBy: {DeletedBy}",
                        doctorId,
                        _currentUserService.UserId);

                    transaction.Rollback();
                    return ServiceResult.Failed("خطای سیستم در حین حذف پزشک رخ داده است.");
                }
            }
        }

        /// <summary>
        /// بازیابی داده‌های یک پزشک برای پر کردن فرم ویرایش
        /// این متد فقط پزشکان فعال (غیرحذف شده) را بازمی‌گرداند
        /// </summary>
        /// <param name="doctorId">شناسه پزشک مورد نظر</param>
        /// <returns>مدل داده‌های پزشک برای ویرایش</returns>
        public async Task<ServiceResult<DoctorCreateEditViewModel>> GetDoctorForEditAsync(int doctorId)
        {
            _log.Information("درخواست دریافت اطلاعات پزشک برای ویرایش. DoctorId: {DoctorId}", doctorId);

            try
            {
                // فیلتر Soft Delete برای جلوگیری از نمایش پزشکان حذف شده
                var doctorViewModel = await _context.Doctors
                    .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
                    .ProjectTo<DoctorCreateEditViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                if (doctorViewModel == null)
                {
                    _log.Warning("درخواست ویرایش برای پزشک غیرموجود یا حذف شده. DoctorId: {DoctorId}", doctorId);
                    return ServiceResult<DoctorCreateEditViewModel>.Failed("پزشک مشخص‌شده پیدا نشد یا حذف شده است.");
                }

                return ServiceResult<DoctorCreateEditViewModel>.Successful(doctorViewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت اطلاعات پزشک برای ویرایش. DoctorId: {DoctorId}", doctorId);
                return ServiceResult<DoctorCreateEditViewModel>.Failed("خطای سیستم رخ داده است.");
            }
        }

        /// <summary>
        /// بازیابی جزئیات کامل یک پزشک برای نمایش اطلاعات
        /// این متد فقط پزشکان فعال (غیرحذف شده) را بازمی‌گرداند
        /// </summary>
        /// <param name="doctorId">شناسه پزشک مورد نظر</param>
        /// <returns>مدل جزئیات کامل پزشک</returns>
        public async Task<ServiceResult<DoctorDetailsViewModel>> GetDoctorDetailsAsync(int doctorId)
        {
            _log.Information("درخواست دریافت جزئیات پزشک. DoctorId: {DoctorId}", doctorId);

            try
            {
                // فیلتر Soft Delete برای جلوگیری از نمایش پزشکان حذف شده
                var doctorDetails = await _context.Doctors
                    .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
                    .ProjectTo<DoctorDetailsViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                if (doctorDetails == null)
                {
                    _log.Warning("درخواست جزئیات برای پزشک غیرموجود یا حذف شده. DoctorId: {DoctorId}", doctorId);
                    return ServiceResult<DoctorDetailsViewModel>.Failed("پزشک مشخص‌شده پیدا نشد یا حذف شده است.");
                }

                return ServiceResult<DoctorDetailsViewModel>.Successful(doctorDetails);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت جزئیات پزشک. DoctorId: {DoctorId}", doctorId);
                return ServiceResult<DoctorDetailsViewModel>.Failed("خطای سیستم رخ داده است.");
            }
        }

        /// <summary>
        /// جستجو و صفحه‌بندی پزشکان با رعایت فیلترهای ضروری و عملکرد بهینه
        /// این متد فقط پزشکان فعال (غیرحذف شده) را بازمی‌گرداند
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
        /// <returns>نتایج جستجو به همراه اطلاعات صفحه‌بندی</returns>
        public async Task<ServiceResult<PagedResult<DoctorIndexViewModel>>> SearchDoctorsAsync(string searchTerm, int pageNumber, int pageSize)
        {
            _log.Information("درخواست جستجو پزشکان. Term: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}", searchTerm, pageNumber, pageSize);

            try
            {
                // شروع با تمام پزشکان فعال (غیرحذف شده)
                var query = _context.Doctors
                    .Where(d => !d.IsDeleted)
                    .AsQueryable();

                // اعمال فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    // استفاده از شرایط جداگانه برای بهبود عملکرد و استفاده از ایندکس‌ها
                    query = query.Where(d =>
                        d.FirstName.Contains(searchTerm) ||
                        d.LastName.Contains(searchTerm) ||
                        (d.Specialization != null && d.Specialization.Contains(searchTerm)) ||
                        (d.PhoneNumber != null && d.PhoneNumber.Contains(searchTerm))
                    );
                }

                // محاسبه تعداد کل آیتم‌ها
                var totalItems = await query.CountAsync();

                // اعمال صفحه‌بندی و تبدیل به مدل نمایشی
                var items = await query
                    .OrderBy(d => d.LastName)
                    .ThenBy(d => d.FirstName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ProjectTo<DoctorIndexViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                // ایجاد نتیجه صفحه‌بندی شده
                var pagedResult = new PagedResult<DoctorIndexViewModel>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };

                _log.Information("جستجوی پزشکان با موفقیت انجام شد. Found: {TotalItems}", totalItems);
                return ServiceResult<PagedResult<DoctorIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در جستجوی پزشکان. Term: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}", searchTerm, pageNumber, pageSize);
                return ServiceResult<PagedResult<DoctorIndexViewModel>>.Failed("خطای سیستم در حین جستجو رخ داده است.");
            }
        }

        #region متد‌های کمکی

        /// <summary>
        /// اعتبارسنجی شماره تلفن بر اساس استاندارد بین‌المللی E.164
        /// </summary>
        /// <param name="phoneNumber">شماره تلفن برای اعتبارسنجی</param>
        /// <returns>آیا شماره تلفن معتبر است؟</returns>
        private bool IsValidInternationalPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // الگوی استاندارد E.164 برای شماره‌های تلفن بین‌المللی
            // مثال: +989123456789
            return System.Text.RegularExpressions.Regex.IsMatch(
                phoneNumber,
                @"^\+[1-9]\d{1,14}$"
            );
        }

        #endregion
    }
}