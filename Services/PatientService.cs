using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس مدیریت بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ایجاد، ویرایش و حذف نرم بیماران
    /// 2. مدیریت کامل اطلاعات بیمه بیماران
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// 6. پشتیبانی از بیمه آزاد به عنوان پیش‌فرض برای بیماران بدون بیمه
    /// 7. پشتیبانی از محیط‌های پزشکی ایرانی با تاریخ شمسی و اعداد فارسی
    /// 8. عدم استفاده از AutoMapper برای کنترل کامل بر روی داده‌ها
    /// 9. امنیت بالا با رعایت استانداردهای سیستم‌های پزشکی
    /// 10. عملکرد بهینه برای محیط‌های Production
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationUserManager _userManager;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppSettings _appSettings;

        public PatientService(
            ApplicationDbContext context,
            ApplicationUserManager userManager,
            ILogger logger,
            ICurrentUserService currentUserService,
            IAppSettings appSettings)
        {
            _context = context;
            _userManager = userManager;
            _log = logger.ForContext<PatientService>();
            _currentUserService = currentUserService;
            _appSettings = appSettings;
        }

        private int PageSize => _appSettings.DefaultPageSize;

        /// <summary>
        /// تبدیل مدل بیمار به PatientIndexViewModel
        /// </summary>
        private PatientIndexViewModel ConvertToPatientIndexViewModel(Patient patient)
        {
            if (patient == null)
                return null;

            return new PatientIndexViewModel
            {
                PatientId = patient.PatientId,
                NationalCode = patient.NationalCode,
                FullName = $"{patient.FirstName} {patient.LastName}",
                PhoneNumber = patient.PhoneNumber,
                InsuranceName = patient.Insurance?.Name,
                CreatedAt = patient.CreatedAt,
                CreatedAtShamsi = patient.CreatedAt.ToPersianDateTime()
            };
        }

        /// <summary>
        /// تبدیل مدل بیمار به PatientDetailsViewModel
        /// </summary>
        private PatientDetailsViewModel ConvertToPatientDetailsViewModel(Patient patient)
        {
            if (patient == null)
                return null;

            return new PatientDetailsViewModel
            {
                PatientId = patient.PatientId,
                NationalCode = patient.NationalCode,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                BirthDate = patient.BirthDate,
                Address = patient.Address,
                PhoneNumber = patient.PhoneNumber,
                InsuranceId = patient.InsuranceId,
                InsuranceName = patient.Insurance?.Name,
                CreatedAt = patient.CreatedAt,
                CreatedByUser = patient.CreatedByUser != null ?
                    $"{patient.CreatedByUser.FirstName} {patient.CreatedByUser.LastName}" : "ناشناس",
                UpdatedAt = patient.UpdatedAt,
                UpdatedByUser = patient.UpdatedByUser != null ?
                    $"{patient.UpdatedByUser.FirstName} {patient.UpdatedByUser.LastName}" : null,
                LastLoginDate = patient.LastLoginDate,
                ReceptionCount = patient.Receptions != null ? patient.Receptions.Count(r => !r.IsDeleted) : 0,
                DebtBalance = 0 // محاسبه بعداً انجام می‌شود
            };
        }

        /// <summary>
        /// تبدیل مدل بیمار به PatientCreateEditViewModel
        /// </summary>
        private PatientCreateEditViewModel ConvertToPatientCreateEditViewModel(Patient patient)
        {
            if (patient == null)
                return null;

            return new PatientCreateEditViewModel
            {
                PatientId = patient.PatientId,
                NationalCode = patient.NationalCode,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                BirthDate = patient.BirthDate,
                Address = patient.Address,
                PhoneNumber = patient.PhoneNumber,
                InsuranceId = patient.InsuranceId,
                InsuranceName = patient.Insurance?.Name,
                CreatedAt = patient.CreatedAt,
                CreatedByUser = patient.CreatedByUser != null ?
                    $"{patient.CreatedByUser.FirstName} {patient.CreatedByUser.LastName}" : "ناشناس",
                UpdatedAt = patient.UpdatedAt,
                UpdatedByUser = patient.UpdatedByUser != null ?
                    $"{patient.UpdatedByUser.FirstName} {patient.UpdatedByUser.LastName}" : null
            };
        }

        /// <summary>
        /// تبدیل PatientCreateEditViewModel به مدل بیمار
        /// </summary>
        private void UpdatePatientModel(Patient patient, PatientCreateEditViewModel model)
        {
            if (patient == null || model == null)
                return;

            patient.NationalCode = model.NationalCode;
            patient.FirstName = model.FirstName;
            patient.LastName = model.LastName;
            patient.BirthDate = model.BirthDate;
            patient.Address = model.Address;
            patient.PhoneNumber = model.PhoneNumber;
            patient.InsuranceId = model.InsuranceId;
        }
        // ClinicApp/Services/PatientService.cs
        public async Task<IdentityResult> RegisterPatientAsync(RegisterPatientViewModel model, string userIp)
        {
            // ۱. محدودیت نرخ برای جلوگیری از اسپم
            string rateKey = $"RegisterAttempt_{PhoneNumberHelper.NormalizeToE164(model.PhoneNumber)}_{userIp}";
            if (!RateLimiter.TryIncrement(rateKey, SystemConstants.MaxLoginAttempts, TimeSpan.FromMinutes(SystemConstants.RateLimitMinutes)))
            {
                _log.Warning("محدودیت نرخ رد شد برای {PhoneNumber} از آی‌پی {IP}", model.PhoneNumber, userIp);
                return IdentityResult.Failed("شما از حد مجاز تلاش‌های ثبت‌نام عبور کرده‌اید. لطفاً بعداً دوباره امتحان کنید.");
            }

            // ۲. نرمالایز کردن شماره موبایل به فرمت E.164
            var normalizedPhone = PhoneNumberHelper.NormalizeToE164(model.PhoneNumber);
            if (string.IsNullOrEmpty(normalizedPhone))
            {
                return IdentityResult.Failed("فرمت شماره موبایل معتبر نیست.");
            }

            // ۳. دریافت بیمه آزاد به عنوان پیش‌فرض
            var freeInsurance = await _context.Insurances
                .FirstOrDefaultAsync(i => i.Name == SystemConstants.FreeInsuranceName && !i.IsDeleted);

            if (freeInsurance == null)
            {
                _log.Error("بیمه آزاد در پایگاه داده یافت نشد. لطفاً ابتدا بیمه آزاد را ایجاد کنید.");
                return IdentityResult.Failed("سیستم به درستی پیکربندی نشده است. لطفاً با پشتیبانی تماس بگیرید.");
            }

            // ۴. بررسی وجود کاربر با کد ملی
            var userByNationalCode = await _userManager.FindByNameAsync(model.NationalCode);
            if (userByNationalCode != null)
            {
                // بازیابی پروفایل بیمار موجود
                var patientProfile = await _context.Patients
                    .Include(p => p.Insurance)
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == userByNationalCode.Id);

                var normalizedExistingPhone = PhoneNumberHelper.NormalizeToE164(patientProfile?.PhoneNumber);

                if (normalizedPhone == normalizedExistingPhone)
                {
                    // موفق: کاربر موجود و شماره موبایل صحیح
                    _log.Information("حساب موجود از طریق OTP ادعا شد. کد ملی: {NationalCode}، آی‌پی: {IP}", model.NationalCode, userIp);

                    // تایید شماره موبایل اگر هنوز تایید نشده
                    if (!userByNationalCode.PhoneNumberConfirmed)
                    {
                        userByNationalCode.PhoneNumberConfirmed = true;
                        await _userManager.UpdateAsync(userByNationalCode);
                    }

                    // به‌روزرسانی تاریخ آخرین ورود
                    if (patientProfile != null)
                    {
                        patientProfile.LastLoginDate = DateTime.UtcNow;
                        patientProfile.UpdatedAt = DateTime.UtcNow;
                        patientProfile.UpdatedByUserId = _currentUserService.UserId;
                        await _context.SaveChangesAsync();
                    }

                    return IdentityResult.Success;
                }
                else
                {
                    // هشدار امنیتی: کد ملی موجود ولی شماره متفاوت
                    _log.Warning("هشدار امنیتی: کد ملی با شماره موبایل ناهمخوان ادعا شد. کد ملی: {NationalCode}، آی‌پی: {IP}", model.NationalCode, userIp);
                    return IdentityResult.Failed("کد ملی واردشده با شماره موبایل دیگری ثبت شده است. لطفاً با پشتیبانی تماس بگیرید.");
                }
            }

            // ۵. بررسی شماره موبایل موجود در جدول AspNetUsers
            var userByPhone = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone);
            if (userByPhone != null)
            {
                _log.Warning("تلاش برای ثبت‌نام با شماره موبایل تکراری: {PhoneNumber}، آی‌پی: {IP}", normalizedPhone, userIp);
                return IdentityResult.Failed("کاربری با این شماره موبایل قبلاً ثبت‌نام کرده است.");
            }

            // ۶. ایجاد کاربر جدید در AspNetUsers
            var newUser = new ApplicationUser
            {
                UserName = model.NationalCode,
                PhoneNumber = normalizedPhone,
                PhoneNumberConfirmed = true,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId
            };

            // ۷. تراکنش امن برای ثبت همزمان در AspNetUsers و Patients
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // ایجاد کاربر در Identity
                    var identityResult = await _userManager.CreateAsync(newUser);
                    if (!identityResult.Succeeded)
                    {
                        transaction.Rollback();
                        _log.Warning("ایجاد کاربر شکست خورد. کد ملی: {NationalCode}، آی‌پی: {IP}، خطاها: {@Errors}", model.NationalCode, userIp, identityResult.Errors);
                        return identityResult;
                    }

                    // اختصاص نقش "Patient"
                    await _userManager.AddToRoleAsync(newUser.Id, AppRoles.Patient);

                    // ایجاد پروفایل بیمار و ذخیره در دیتابیس
                    var newPatient = new Patient
                    {
                        NationalCode = model.NationalCode,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        BirthDate = model.BirthDate,
                        Address = model.Address,
                        PhoneNumber = normalizedPhone,
                        LastLoginDate = DateTime.UtcNow,
                        InsuranceId = freeInsurance.InsuranceId,
                        ApplicationUserId = newUser.Id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = _currentUserService.UserId,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedByUserId = _currentUserService.UserId,
                        IsDeleted = false
                    };

                    _context.Patients.Add(newPatient);
                    await _context.SaveChangesAsync();

                    transaction.Commit();
                    _log.Information("بیمار جدید با موفقیت ثبت‌نام شد. کد ملی: {NationalCode}، موبایل: {PhoneNumber}، آی‌پی: {IP}", model.NationalCode, normalizedPhone, userIp);

                    return IdentityResult.Success;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _log.Error(ex, "تراکنش در حین ایجاد بیمار جدید شکست خورد. کد ملی: {NationalCode}، آی‌پی: {IP}", model.NationalCode, userIp);
                    return IdentityResult.Failed("خطای سیستمی رخ داد. عملیات لغو شد.");
                }
            }
        }

        /// <summary>
        /// جستجو و صفحه‌بندی بیماران
        /// </summary>
        public async Task<ServiceResult<PagedResult<PatientIndexViewModel>>> SearchPatientsAsync(string searchTerm, int pageNumber, int pageSize)
        {
            _log.Information(
                "درخواست جستجوی بیماران. SearchTerm: {SearchTerm}, PageNumber: {PageNumber}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                searchTerm, pageNumber, pageSize, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var query = _context.Patients
                    .Include(p => p.Insurance)
                    .Where(p => !p.IsDeleted);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p =>
                      p.FirstName.Contains(searchTerm) ||
                      p.LastName.Contains(searchTerm) ||
                      p.NationalCode.Contains(searchTerm) ||
                      p.PhoneNumber.Contains(searchTerm));
                }

                var totalItems = await query.CountAsync();

                var patients = await query
                    .OrderBy(p => p.LastName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // تبدیل دستی به ViewModel
                var items = new List<PatientIndexViewModel>();
                foreach (var patient in patients)
                {
                    items.Add(ConvertToPatientIndexViewModel(patient));
                }

                var pagedResult = new PagedResult<PatientIndexViewModel>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };

                _log.Information(
                    "جستجوی بیماران با موفقیت انجام شد. Count: {Count}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    pagedResult.TotalItems, pageNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PagedResult<PatientIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در جستجوی بیماران. SearchTerm: {SearchTerm}, PageNumber: {PageNumber}. User: {UserName} (Id: {UserId})",
                    searchTerm, pageNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PagedResult<PatientIndexViewModel>>.Failed(
                    "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
            }
        }

        /// <summary>
        /// دریافت جزئیات یک بیمار به صورت امن و مرحله‌ای.
        /// </summary>
        public async Task<ServiceResult<PatientDetailsViewModel>> GetPatientDetailsAsync(int patientId)
        {
            _log.Information(
                "درخواست جزئیات بیمار با شناسه {PatientId}. User: {UserName} (Id: {UserId})",
                patientId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // مرحله 1: دریافت موجودیت کامل بیمار از دیتابیس
                var patientEntity = await _context.Patients
                    .Include(p => p.Insurance)
                    .Include(p => p.Receptions)
                    .Where(p => p.PatientId == patientId && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (patientEntity == null)
                {
                    _log.Warning(
                        "درخواست جزئیات برای بیمار غیرموجود انجام شد. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<PatientDetailsViewModel>.Failed("بیمار موردنظر یافت نشد.");
                }

                // مرحله 2: نگاشت موجودیت به ViewModel
                var patientViewModel = ConvertToPatientDetailsViewModel(patientEntity);

                // محاسبه مانده بدهی بیمار
                var totalDebt = await _context.PaymentTransactions
                    .Where(t => t.Reception.PatientId == patientId &&
                               t.Method == PaymentMethod.Debt &&
                               t.Status == PaymentStatus.Success &&
                               !t.IsDeleted)
                    .SumAsync(t => t.Amount);

                patientViewModel.DebtBalance = totalDebt;

                // تبدیل تاریخ‌ها به شمسی
                patientViewModel.CreatedAtShamsi = patientViewModel.CreatedAt.ToPersianDateTime();
                if (patientViewModel.UpdatedAt.HasValue)
                    patientViewModel.UpdatedAtShamsi = patientViewModel.UpdatedAt.Value.ToPersianDateTime();

                _log.Information(
                    "دریافت جزئیات بیمار شناسه {PatientId} با موفقیت انجام شد. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientDetailsViewModel>.Successful(patientViewModel);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای غیرمنتظره هنگام دریافت جزئیات بیمار رخ داد. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientDetailsViewModel>.Failed(
                    "خطای سیستمی رخ داد. لطفاً با پشتیبانی تماس بگیرید.");
            }
        }

        /// <summary>
        /// دریافت اطلاعات بیمار برای ویرایش
        /// </summary>
        public async Task<ServiceResult<PatientCreateEditViewModel>> GetPatientForEditAsync(int patientId)
        {
            _log.Information(
                "درخواست اطلاعات بیمار شناسه {PatientId} برای ویرایش. User: {UserName} (Id: {UserId})",
                patientId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var patient = await _context.Patients
                    .Include(p => p.Insurance)
                    .Where(p => p.PatientId == patientId && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    _log.Warning(
                        "دریافت اطلاعات بیمار شناسه {PatientId} برای ویرایش ناموفق بود. Error: بیمار یافت نشد. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<PatientCreateEditViewModel>.Failed("بیمار یافت نشد.");
                }

                var model = ConvertToPatientCreateEditViewModel(patient);

                // تبدیل تاریخ‌ها به شمسی
                model.CreatedAtShamsi = model.CreatedAt.ToPersianDateTime();
                if (model.UpdatedAt.HasValue)
                    model.UpdatedAtShamsi = model.UpdatedAt.Value.ToPersianDateTime();

                _log.Information(
                    "دریافت اطلاعات بیمار شناسه {PatientId} برای ویرایش با موفقیت انجام شد. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientCreateEditViewModel>.Successful(model);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت اطلاعات بیمار شناسه {PatientId} برای ویرایش. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientCreateEditViewModel>.Failed(
                    "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
            }
        }

        /// <summary>
        /// ایجاد یا واکنش‌گرایی یک بیمار جدید با حساب کاربری مرتبط به صورت امن و اتمیک
        /// </summary>
        public async Task<ServiceResult> CreatePatientAsync(PatientCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ایجاد بیمار جدید با کد ملی {NationalCode}. User: {UserName} (Id: {UserId})",
                model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

            // 1. دریافت بیمه آزاد به عنوان پیش‌فرض
            var freeInsurance = await _context.Insurances
                .FirstOrDefaultAsync(i => i.Name == SystemConstants.FreeInsuranceName && !i.IsDeleted);

            if (freeInsurance == null)
            {
                _log.Error(
                    "بیمه آزاد در پایگاه داده یافت نشد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed(
                    "سیستم به درستی پیکربندی نشده است. لطفاً با پشتیبانی تماس بگیرید.");
            }

            // 2. بررسی وجود کاربر با کد ملی
            var existingUser = await _userManager.FindByNameAsync(model.NationalCode);

            // 3. بررسی وجود بیمار (شامل آرشیو شده)
            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.NationalCode == model.NationalCode);

            // --- Scenario A: بیمار آرشیو شده ---
            if (existingPatient != null && existingPatient.IsDeleted)
            {
                _log.Information(
                    "بازیابی بیمار آرشیو شده. کد ملی: {NationalCode}. User: {UserName} (Id: {UserId})",
                    model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

                UpdatePatientModel(existingPatient, model);
                existingPatient.IsDeleted = false;
                existingPatient.DeletedAt = null;
                existingPatient.DeletedByUserId = null;
                existingPatient.UpdatedAt = DateTime.UtcNow;
                existingPatient.UpdatedByUserId = _currentUserService.UserId;

                // تنظیم بیمه آزاد اگر بیمه‌ای انتخاب نشده باشد
                if (model.InsuranceId <= 0)
                {
                    existingPatient.InsuranceId = freeInsurance.InsuranceId;
                }

                if (existingUser != null && !existingUser.IsActive)
                {
                    existingUser.IsActive = true;
                    existingUser.UpdatedAt = DateTime.UtcNow;
                    existingUser.UpdatedByUserId = _currentUserService.UserId;
                    await _userManager.UpdateAsync(existingUser);
                }

                await _context.SaveChangesAsync();

                _log.Information(
                    "بیمار آرشیو شده با کد ملی {NationalCode} با موفقیت بازیابی شد. User: {UserName} (Id: {UserId})",
                    model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("پروفایل بیمار آرشیو شده با موفقیت بازگردانی شد.");
            }

            // --- Scenario B: بیمار و کاربر فعال هستند ---
            if (existingPatient != null && existingUser != null && !existingPatient.IsDeleted)
            {
                _log.Warning(
                    "تلاش برای ایجاد بیمار تکراری. کد ملی: {NationalCode}. User: {UserName} (Id: {UserId})",
                    model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed(
                    "بیماری با این کد ملی هم‌اکنون در سیستم فعال است.", "DuplicateNationalCode");
            }

            // --- Scenario C: کاربر موجود است اما پروفایل بیمار موجود نیست ---
            if (existingPatient == null && existingUser != null)
            {
                _log.Warning(
                    "عدم تطابق داده‌ها: کاربر وجود دارد اما پروفایل بیمار یافت نشد. کد ملی: {NationalCode}. User: {UserName} (Id: {UserId})",
                    model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

                var newPatient = new Patient
                {
                    NationalCode = model.NationalCode,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BirthDate = model.BirthDate,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    ApplicationUserId = existingUser.Id,
                    InsuranceId = freeInsurance.InsuranceId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedByUserId = _currentUserService.UserId,
                    IsDeleted = false
                };

                // تنظیم بیمه آزاد اگر بیمه‌ای انتخاب نشده باشد
                if (model.InsuranceId <= 0)
                {
                    newPatient.InsuranceId = freeInsurance.InsuranceId;
                }

                _context.Patients.Add(newPatient);
                await _context.SaveChangesAsync();

                _log.Information(
                    "پروفایل بیمار برای کاربر موجود با کد ملی {NationalCode} ایجاد شد. User: {UserName} (Id: {UserId})",
                    model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful(
                    "یک حساب کاربری موجود پیدا شد. پروفایل بیمار جدید ایجاد و به آن متصل شد.");
            }

            // --- Scenario D: بیمار و کاربر جدید ---
            if (existingPatient == null && existingUser == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = model.NationalCode,
                    PhoneNumber = model.PhoneNumber,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumberConfirmed = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId
                };

                var identityResult = await _userManager.CreateAsync(newUser);
                if (!identityResult.Succeeded)
                {
                    _log.Warning(
                        "ایجاد کاربر ناموفق بود. کد ملی: {NationalCode}, Errors: {@Errors}. User: {UserName} (Id: {UserId})",
                        model.NationalCode, identityResult.Errors, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult.Failed(
                        "خطا در ایجاد کاربر: " + string.Join(", ", identityResult.Errors));
                }

                await _userManager.AddToRoleAsync(newUser.Id, AppRoles.Patient);

                var newPatient = new Patient
                {
                    NationalCode = model.NationalCode,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BirthDate = model.BirthDate,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    ApplicationUserId = newUser.Id,
                    InsuranceId = freeInsurance.InsuranceId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedByUserId = _currentUserService.UserId,
                    IsDeleted = false
                };

                // تنظیم بیمه آزاد اگر بیمه‌ای انتخاب نشده باشد
                if (model.InsuranceId <= 0)
                {
                    newPatient.InsuranceId = freeInsurance.InsuranceId;
                }

                _context.Patients.Add(newPatient);
                await _context.SaveChangesAsync();

                _log.Information(
                    "بیمار و کاربر جدید با کد ملی {NationalCode} با موفقیت ایجاد شد. User: {UserName} (Id: {UserId})",
                    newPatient.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("بیمار جدید با موفقیت ایجاد شد.");
            }

            // هر حالت غیرمنتظره
            _log.Error(
                "حالت غیرمنتظره در ایجاد بیمار. کد ملی: {NationalCode}. User: {UserName} (Id: {UserId})",
                model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

            return ServiceResult.Failed("خطای ناشناخته رخ داد.");
        }

        /// <summary>
        /// بروزرسانی پروفایل بیمار و همگام‌سازی با حساب کاربری مرتبط
        /// </summary>
        public async Task<ServiceResult> UpdatePatientAsync(PatientCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ویرایش بیمار شناسه {PatientId} با کد ملی {NationalCode}. User: {UserName} (Id: {UserId})",
                model.PatientId, model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

            // 1. یافتن بیمار
            var existingPatient = await _context.Patients
                .Include(p => p.Insurance)
                .FirstOrDefaultAsync(p => p.PatientId == model.PatientId && !p.IsDeleted);

            if (existingPatient == null)
            {
                _log.Warning(
                    "بیمار یافت نشد. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    model.PatientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed("بیمار مورد نظر یافت نشد.");
            }

            // 2. بررسی کد ملی تکراری
            if (await _context.Patients
                .AsNoTracking()
                .AnyAsync(p => p.NationalCode == model.NationalCode &&
                              p.PatientId != model.PatientId &&
                              !p.IsDeleted))
            {
                _log.Warning(
                    "تلاش برای استفاده از کد ملی تکراری. PatientId: {PatientId}, NationalCode: {NationalCode}. User: {UserName} (Id: {UserId})",
                    model.PatientId, model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed("کد ملی وارد شده قبلاً ثبت شده است.", "DuplicateNationalCode");
            }

            // 3. بررسی شماره موبایل تکراری
            if (await _userManager.Users
                .AsNoTracking()
                .AnyAsync(u => u.PhoneNumber == model.PhoneNumber &&
                              u.Id != existingPatient.ApplicationUserId))
            {
                _log.Warning(
                    "تلاش برای استفاده از شماره موبایل تکراری. PatientId: {PatientId}, PhoneNumber: {PhoneNumber}. User: {UserName} (Id: {UserId})",
                    model.PatientId, model.PhoneNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed("شماره موبایل وارد شده قبلاً ثبت شده است.");
            }

            try
            {
                // اعمال تغییرات
                UpdatePatientModel(existingPatient, model);

                // به‌روزرسانی اطلاعات ردیابی
                existingPatient.UpdatedAt = DateTime.UtcNow;
                existingPatient.UpdatedByUserId = _currentUserService.UserId;

                // اطمینان از اینکه بیمه تنظیم شده است
                if (existingPatient.InsuranceId <= 0)
                {
                    var freeInsurance = await _context.Insurances
                        .FirstOrDefaultAsync(i => i.Name == SystemConstants.FreeInsuranceName && !i.IsDeleted);

                    if (freeInsurance != null)
                    {
                        existingPatient.InsuranceId = freeInsurance.InsuranceId;
                    }
                }

                // علامت‌گذاری موجودیت به عنوان تغییر کرده
                _context.Entry(existingPatient).State = EntityState.Modified;

                // همگام‌سازی اطلاعات کاربر
                var user = await _userManager.FindByIdAsync(existingPatient.ApplicationUserId);
                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.UserName = model.NationalCode;
                    user.UpdatedAt = DateTime.UtcNow;
                    user.UpdatedByUserId = _currentUserService.UserId;
                    await _userManager.UpdateAsync(user);
                }

                await _context.SaveChangesAsync();

                _log.Information(
                    "اطلاعات بیمار شناسه {PatientId} با کد ملی {NationalCode} با موفقیت ویرایش شد. User: {UserName} (Id: {UserId})",
                    model.PatientId, model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("اطلاعات بیمار با موفقیت بروزرسانی شد.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _log.Warning(
                    ex,
                    "تداخل همزمانی هنگام ویرایش بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    model.PatientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed(
                    "این رکورد توسط کاربر دیگری تغییر یافته است. لطفاً صفحه را تازه‌سازی کنید.");
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای غیرمنتظره هنگام بروزرسانی بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    model.PatientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed(
                    "خطای سیستم هنگام ذخیره تغییرات رخ داد. لطفاً با پشتیبانی تماس بگیرید.");
            }
        }

        /// <summary>
        /// بیمار را به صورت نرم‌افزاری حذف می‌کند پس از بررسی اینکه هیچ رکورد فعال و غیرقابل حذفی نداشته باشد.
        /// </summary>
        public async Task<ServiceResult> DeletePatientAsync(int patientId)
        {
            _log.Information(
                "درخواست حذف بیمار شناسه {PatientId}. User: {UserName} (Id: {UserId})",
                patientId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                #region یافتن بیمار
                // بازیابی بیمار همراه با اطلاعات مرتبط برای اعتبارسنجی
                var patient = await _context.Patients
                    .Include(p => p.Receptions)   // شامل پذیرش‌ها برای بررسی وضعیت
                    .Include(p => p.Appointments) // شامل نوبت‌ها
                    .FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted);

                if (patient == null)
                {
                    _log.Warning(
                        "تلاش برای حذف بیمار ناموجود. شناسه بیمار: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult.Failed("بیمار مورد نظر یافت نشد.");
                }
                #endregion

                #region اعتبارسنجی قوانین کسب‌وکار
                // بررسی پذیرش‌های فعال یا در انتظار
                if (patient.Receptions.Any(r => r.Status != ReceptionStatus.Completed &&
                                              r.Status != ReceptionStatus.Cancelled &&
                                              !r.IsDeleted))
                {
                    _log.Warning(
                        "تلاش برای حذف بیمار با پذیرش‌های فعال. شناسه بیمار: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult.Failed(
                        "امکان حذف بیمار به دلیل وجود پذیرش‌های فعال یا در انتظار پرداخت وجود ندارد.");
                }

                // بررسی نوبت‌های آینده لغو نشده
                if (patient.Appointments.Any(a => a.AppointmentDate >= DateTime.Today &&
                                               a.Status != AppointmentStatus.Cancelled))
                {
                    _log.Warning(
                        "تلاش برای حذف بیمار با نوبت‌های آینده. شناسه بیمار: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult.Failed(
                        "امکان حذف بیمار به دلیل وجود نوبت‌های رزرو شده در آینده وجود ندارد.");
                }
                #endregion

                #region حذف نرم‌افزاری
                // انجام حذف نرم‌افزاری با علامت‌گذاری
                patient.IsDeleted = true;
                patient.DeletedAt = DateTime.UtcNow;
                patient.DeletedByUserId = _currentUserService.UserId;
                _context.Entry(patient).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                _log.Information(
                    "حذف نرم‌افزاری بیمار با موفقیت انجام شد. شناسه بیمار: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("بیمار با موفقیت حذف شد.");
                #endregion
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای غیرمنتظره در حذف بیمار. شناسه بیمار: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed(
                    "خطای سیستمی در حذف بیمار. لطفاً با پشتیبانی تماس بگیرید.");
            }
        }
    }
}