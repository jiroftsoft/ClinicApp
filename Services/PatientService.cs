using ClinicApp.Core;
using ClinicApp.Extensions;
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

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس مدیریت بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ایجاد، ویرایش و حذف نرم بیماران با رعایت استانداردهای پزشکی ایران
    /// 2. مدیریت کامل اطلاعات بیمه بیماران با توجه به قوانین بیمه‌ای ایران
    /// 3. پشتیبانی کامل از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط با تقویم شمسی
    /// 6. پشتیبانی از بیمه آزاد به عنوان پیش‌فرض برای بیماران بدون بیمه
    /// 7. پشتیبانی از محیط‌های پزشکی ایرانی با تاریخ شمسی و اعداد فارسی
    /// 8. عدم استفاده از AutoMapper برای کنترل کامل بر روی داده‌ها
    /// 9. امنیت بالا با رعایت استانداردهای سیستم‌های پزشکی ایران
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
        /// تبدیل مدل بیمار به PatientIndexViewModel با پشتیبانی کامل از تقویم شمسی
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
                CreatedAtShamsi = patient.CreatedAt.ToPersianDateTime(),
                BirthDateShamsi = patient.BirthDate.HasValue ?
                    patient.BirthDate.Value.ToPersianDate() : null
            };
        }

        /// <summary>
        /// تبدیل مدل بیمار به PatientDetailsViewModel با پشتیبانی کامل از تقویم شمسی
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
                BirthDateShamsi = patient.BirthDate.HasValue ?
                    patient.BirthDate.Value.ToPersianDate() : null,
                Address = patient.Address,
                PhoneNumber = patient.PhoneNumber,
                InsuranceId = patient.InsuranceId,
                InsuranceName = patient.Insurance?.Name,
                CreatedAt = patient.CreatedAt,
                CreatedAtShamsi = patient.CreatedAt.ToPersianDateTime(),
                CreatedByUser = patient.CreatedByUser != null ?
                    $"{patient.CreatedByUser.FirstName} {patient.CreatedByUser.LastName}" : "ناشناس",
                UpdatedAt = patient.UpdatedAt,
                UpdatedAtShamsi = patient.UpdatedAt.HasValue ?
                    patient.UpdatedAt.Value.ToPersianDateTime() : null,
                UpdatedByUser = patient.UpdatedByUser != null ?
                    $"{patient.UpdatedByUser.FirstName} {patient.UpdatedByUser.LastName}" : null,
                LastLoginDate = patient.LastLoginDate,
                LastLoginDateShamsi = patient.LastLoginDate.HasValue ?
                    patient.LastLoginDate.Value.ToPersianDateTime() : null,
                ReceptionCount = patient.Receptions != null ?
                    patient.Receptions.Count(r => !r.IsDeleted) : 0,
                DebtBalance = 0 // محاسبه بعداً انجام می‌شود
            };
        }

        /// <summary>
        /// تبدیل مدل بیمار به PatientCreateEditViewModel با پشتیبانی کامل از تقویم شمسی
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
                BirthDateShamsi = patient.BirthDate.HasValue ?
                    patient.BirthDate.Value.ToPersianDate() : null,
                Address = patient.Address,
                PhoneNumber = patient.PhoneNumber,
                InsuranceId = patient.InsuranceId,
                InsuranceName = patient.Insurance?.Name,
                CreatedAt = patient.CreatedAt,
                CreatedAtShamsi = patient.CreatedAt.ToPersianDateTime(),
                CreatedByUser = patient.CreatedByUser != null ?
                    $"{patient.CreatedByUser.FirstName} {patient.CreatedByUser.LastName}" : "ناشناس",
                UpdatedAt = patient.UpdatedAt,
                UpdatedAtShamsi = patient.UpdatedAt.HasValue ?
                    patient.UpdatedAt.Value.ToPersianDateTime() : null,
                UpdatedByUser = patient.UpdatedByUser != null ?
                    $"{patient.UpdatedByUser.FirstName} {patient.UpdatedByUser.LastName}" : null
            };
        }

        /// <summary>
        /// تبدیل PatientCreateEditViewModel به مدل بیمار با پشتیبانی کامل از تقویم شمسی
        /// </summary>
        private void UpdatePatientModel(Patient patient, PatientCreateEditViewModel model)
        {
            if (patient == null || model == null)
                return;

            // نرمال‌سازی و اعتبارسنجی کد ملی
            string normalizedNationalCode = PersianNumberHelper.ToEnglishNumbers(model.NationalCode);
            if (!PersianNumberHelper.IsValidNationalCode(normalizedNationalCode))
                throw new ArgumentException("کد ملی وارد شده معتبر نیست.");

            // نرمال‌سازی و اعتبارسنجی شماره موبایل
            string normalizedPhoneNumber = PersianNumberHelper.ToEnglishNumbers(model.PhoneNumber);
            if (!PersianNumberHelper.IsValidPhoneNumber(normalizedPhoneNumber))
                throw new ArgumentException("شماره موبایل وارد شده معتبر نیست.");

            patient.NationalCode = normalizedNationalCode;
            patient.FirstName = model.FirstName;
            patient.LastName = model.LastName;
            patient.Address = model.Address;
            patient.PhoneNumber = normalizedPhoneNumber;

            // تبدیل تاریخ شمسی به میلادی
            if (!string.IsNullOrWhiteSpace(model.BirthDateShamsi))
            {
                try
                {
                    patient.BirthDate = PersianDateHelper.ToGregorianDate(model.BirthDateShamsi);
                }
                catch
                {
                    throw new ArgumentException("تاریخ تولد وارد شده معتبر نیست.");
                }
            }
            else
            {
                patient.BirthDate = null;
            }

            patient.InsuranceId = model.InsuranceId;
        }

        /// <summary>
        /// ثبت‌نام بیمار جدید یا بازیابی حساب موجود با OTP
        /// </summary>
        public async Task<ServiceResult> RegisterPatientAsync(RegisterPatientViewModel model, string userIp)
        {
            try
            {
                _log.Information(
                    "درخواست ثبت‌نام بیمار جدید با کد ملی {NationalCode}. کاربر: {UserName} (شناسه: {UserId}), آی‌پی: {UserIp}",
                    model.NationalCode, _currentUserService.UserName, _currentUserService.UserId, userIp);

                // 2. نرمال‌سازی و اعتبارسنجی کد ملی
                string normalizedNationalCode = PersianNumberHelper.ToEnglishNumbers(model.NationalCode);
                if (!PersianNumberHelper.IsValidNationalCode(normalizedNationalCode))
                {
                    _log.Warning("کد ملی نامعتبر برای ثبت‌نام بیمار: {NationalCode}", normalizedNationalCode);
                    return ServiceResult.Failed(
                        "کد ملی وارد شده معتبر نیست.",
                        "INVALID_NATIONAL_CODE",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // 3. نرمال‌سازی و اعتبارسنجی شماره موبایل
                string normalizedPhoneNumber = PersianNumberHelper.ToEnglishNumbers(model.PhoneNumber);
                if (!PersianNumberHelper.IsValidPhoneNumber(normalizedPhoneNumber))
                {
                    _log.Warning("شماره موبایل نامعتبر برای ثبت‌نام بیمار: {PhoneNumber}", normalizedPhoneNumber);
                    return ServiceResult.Failed(
                        "شماره موبایل وارد شده معتبر نیست.",
                        "INVALID_PHONE_NUMBER",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // 4. دریافت بیمه آزاد به عنوان پیش‌فرض
                var freeInsurance = await _context.Insurances
                    .FirstOrDefaultAsync(i => i.Name == SystemConstants.FreeInsuranceName && !i.IsDeleted);

                if (freeInsurance == null)
                {
                    _log.Error("بیمه آزاد در پایگاه داده یافت نشد. لطفاً ابتدا بیمه آزاد را ایجاد کنید.");
                    return ServiceResult.Failed(
                        "سیستم به درستی پیکربندی نشده است. لطفاً با پشتیبانی تماس بگیرید.",
                        "SYSTEM_CONFIG_ERROR",
                        ErrorCategory.General,
                        SecurityLevel.High);
                }

                // 5. بررسی وجود کاربر با کد ملی
                var userByNationalCode = await _userManager.FindByNationalCodeAsync(normalizedNationalCode);
                if (userByNationalCode != null)
                {
                    // بازیابی پروفایل بیمار موجود
                    var patientProfile = await _context.Patients
                        .Include(p => p.Insurance)
                        .FirstOrDefaultAsync(p => p.ApplicationUserId == userByNationalCode.Id && !p.IsDeleted);

                    if (patientProfile != null)
                    {
                        var normalizedExistingPhone = PersianNumberHelper.ToEnglishNumbers(patientProfile.PhoneNumber);

                        if (normalizedPhoneNumber == normalizedExistingPhone)
                        {
                            // موفق: کاربر موجود و شماره موبایل صحیح
                            _log.Information("حساب موجود از طریق OTP ادعا شد. کد ملی: {NationalCode}، آی‌پی: {UserIp}",
                                normalizedNationalCode, userIp);

                            // تایید شماره موبایل اگر هنوز تایید نشده
                            if (!userByNationalCode.PhoneNumberConfirmed)
                            {
                                userByNationalCode.PhoneNumberConfirmed = true;
                                await _userManager.UpdateAsync(userByNationalCode);
                            }

                            // به‌روزرسانی تاریخ آخرین ورود
                            patientProfile.LastLoginDate = DateTime.UtcNow;
                            patientProfile.UpdatedAt = DateTime.UtcNow;
                            patientProfile.UpdatedByUserId = _currentUserService.UserId;
                            await _context.SaveChangesAsync();

                            return ServiceResult.Successful(
                                "حساب کاربری شما با موفقیت تأیید شد.",
                                operationName: "RegisterPatient",
                                userId: userByNationalCode.Id,
                                userFullName: userByNationalCode.FullName,
                                securityLevel: SecurityLevel.Medium);
                        }
                        else
                        {
                            // هشدار امنیتی: کد ملی موجود ولی شماره متفاوت
                            _log.Warning("هشدار امنیتی: کد ملی با شماره موبایل ناهمخوان ادعا شد. کد ملی: {NationalCode}، آی‌پی: {UserIp}",
                                normalizedNationalCode, userIp);
                            return ServiceResult.Failed(
                                "کد ملی واردشده با شماره موبایل دیگری ثبت شده است. لطفاً با پشتیبانی تماس بگیرید.",
                                "NATIONAL_CODE_PHONE_MISMATCH",
                                ErrorCategory.Security,
                                SecurityLevel.High);
                        }
                    }
                }

                // 6. بررسی شماره موبایل موجود در جدول Patients
                var patientByPhone = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PhoneNumber == normalizedPhoneNumber && !p.IsDeleted);

                if (patientByPhone != null)
                {
                    _log.Warning("تلاش برای ثبت‌نام با شماره موبایل تکراری: {PhoneNumber}، آی‌پی: {UserIp}",
                        normalizedPhoneNumber, userIp);
                    return ServiceResult.Failed(
                        "بیماری با این شماره موبایل قبلاً ثبت‌نام کرده است.",
                        "DUPLICATE_PHONE_NUMBER",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // 7. بررسی کد ملی تکراری در جدول Patients
                var patientByNationalCode = await _context.Patients
                    .FirstOrDefaultAsync(p => p.NationalCode == normalizedNationalCode && !p.IsDeleted);

                if (patientByNationalCode != null)
                {
                    _log.Warning("تلاش برای ثبت‌نام با کد ملی تکراری: {NationalCode}، آی‌پی: {UserIp}",
                        normalizedNationalCode, userIp);
                    return ServiceResult.Failed(
                        "بیماری با این کد ملی قبلاً ثبت‌نام کرده است.",
                        "DUPLICATE_NATIONAL_CODE",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // 8. ایجاد کاربر جدید در AspNetUsers
                var newUser = new ApplicationUser
                {
                    UserName = normalizedNationalCode,
                    NationalCode = normalizedNationalCode,
                    PhoneNumber = normalizedPhoneNumber,
                    PhoneNumberConfirmed = true,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    Email = model.Email,
                    Address = model.Address,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId
                };

                // 9. تراکنش امن برای ثبت همزمان در AspNetUsers و Patients
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // ایجاد کاربر در Identity
                        var identityResult = await _userManager.CreateAsync(newUser);
                        if (!identityResult.Succeeded)
                        {
                            transaction.Rollback();
                            _log.Warning("ایجاد کاربر شکست خورد. کد ملی: {NationalCode}، آی‌پی: {UserIp}، خطاها: {@Errors}",
                                normalizedNationalCode, userIp, identityResult.Errors);

                            return ServiceResult.FailedWithValidationErrors(
                                "خطاهای اعتبارسنجی رخ داده است.",
                                identityResult.Errors.Select(e => new ValidationError("Identity", e)),
                                "IDENTITY_VALIDATION_ERROR");
                        }

                        // اختصاص نقش "Patient"
                        await _userManager.AddToRoleAsync(newUser.Id, AppRoles.Patient);

                        // ایجاد پروفایل بیمار و ذخیره در دیتابیس
                        var newPatient = new Patient
                        {
                            NationalCode = normalizedNationalCode,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            BirthDate = !string.IsNullOrWhiteSpace(model.BirthDatePersian) ?
                                PersianDateHelper.ToGregorianDate(model.BirthDatePersian) : (DateTime?)null,
                            Address = model.Address,
                            PhoneNumber = normalizedPhoneNumber,
                            LastLoginDate = DateTime.UtcNow,
                            InsuranceId = model.InsuranceId > 0 ? model.InsuranceId : freeInsurance.InsuranceId,
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
                        _log.Information("بیمار جدید با موفقیت ثبت‌نام شد. کد ملی: {NationalCode}، موبایل: {PhoneNumber}، آی‌پی: {UserIp}",
                            normalizedNationalCode, normalizedPhoneNumber, userIp);

                        return ServiceResult.Successful(
                            "ثبت‌نام شما با موفقیت انجام شد.",
                            operationName: "RegisterPatient",
                            userId: newUser.Id,
                            userFullName: newUser.FullName,
                            securityLevel: SecurityLevel.Medium);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _log.Error(ex, "تراکنش در حین ایجاد بیمار جدید شکست خورد. کد ملی: {NationalCode}، آی‌پی: {UserIp}",
                            normalizedNationalCode, userIp);
                        return ServiceResult.Failed(
                            "خطای سیستمی رخ داد. عملیات لغو شد.",
                            "TRANSACTION_ERROR",
                            ErrorCategory.General,
                            SecurityLevel.High);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در ثبت‌نام بیمار با کد ملی {NationalCode}", model.NationalCode);
                return ServiceResult.Failed(
                    "خطا در ثبت‌نام. لطفاً دوباره تلاش کنید.",
                    "REGISTRATION_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// جستجو و صفحه‌بندی بیماران با عملکرد بهینه برای محیط‌های پزشکی
        /// </summary>
        public async Task<ServiceResult<PagedResult<PatientIndexViewModel>>> SearchPatientsAsync(string searchTerm, int pageNumber, int pageSize)
        {
            _log.Information(
                "درخواست جستجوی بیماران. عبارت جستجو: {SearchTerm}, شماره صفحه: {PageNumber}, اندازه صفحه: {PageSize}. کاربر: {UserName} (شناسه: {UserId})",
                searchTerm, pageNumber, pageSize, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی‌ها
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                // پاک‌سازی و نرمال‌سازی عبارت جستجو
                searchTerm = string.IsNullOrWhiteSpace(searchTerm) ? "" : searchTerm.Trim();
                string normalizedSearchTerm = PersianNumberHelper.ToEnglishNumbers(searchTerm);

                // ساخت پرس‌وجو
                var query = _context.Patients
                    .Include(p => p.Insurance)
                    .Include(p => p.CreatedByUser)
                    .Include(p => p.UpdatedByUser)
                    .Where(p => !p.IsDeleted);

                // اعمال فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p =>
                        p.FirstName.Contains(searchTerm) ||
                        p.LastName.Contains(searchTerm) ||
                        p.NationalCode.Contains(normalizedSearchTerm) ||
                        p.PhoneNumber.Contains(normalizedSearchTerm));
                }

                // محاسبه تعداد کل
                int totalItems = await query.CountAsync();

                // اعمال صفحه‌بندی
                var patients = await query
                    .OrderByDescending(p => p.CreatedAt)
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
                    "جستجوی بیماران با موفقیت انجام شد. تعداد نتایج: {Count}, صفحه: {Page}. کاربر: {UserName} (شناسه: {UserId})",
                    pagedResult.TotalItems, pageNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PagedResult<PatientIndexViewModel>>.Successful(
                    pagedResult,
                    "جستجو با موفقیت انجام شد.",
                    operationName: "SearchPatients",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در جستجوی بیماران. عبارت جستجو: {SearchTerm}, شماره صفحه: {PageNumber}. کاربر: {UserName} (شناسه: {UserId})",
                    searchTerm, pageNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PagedResult<PatientIndexViewModel>>.Failed(
                    "خطا در جستجو. لطفاً دوباره تلاش کنید.",
                    "SEARCH_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت جزئیات کامل یک بیمار برای نمایش اطلاعات
        /// </summary>
        public async Task<ServiceResult<PatientDetailsViewModel>> GetPatientDetailsAsync(int patientId)
        {
            _log.Information(
                "درخواست جزئیات بیمار با شناسه {PatientId}. کاربر: {UserName} (شناسه: {UserId})",
                patientId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // مرحله 1: دریافت موجودیت کامل بیمار از دیتابیس
                var patientEntity = await _context.Patients
                    .Include(p => p.Insurance)
                    .Include(p => p.CreatedByUser)
                    .Include(p => p.UpdatedByUser)
                    .Include(p => p.Receptions)
                    .Where(p => p.PatientId == patientId && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (patientEntity == null)
                {
                    _log.Warning(
                        "درخواست جزئیات برای بیمار غیرموجود انجام شد. شناسه بیمار: {PatientId}. کاربر: {UserName} (شناسه: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<PatientDetailsViewModel>.Failed(
                        "بیمار موردنظر یافت نشد یا حذف شده است.",
                        "PATIENT_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
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

                _log.Information(
                    "دریافت جزئیات بیمار شناسه {PatientId} با موفقیت انجام شد. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientDetailsViewModel>.Successful(
                    patientViewModel,
                    "اطلاعات بیمار با موفقیت دریافت شد.",
                    operationName: "GetPatientDetails",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای غیرمنتظره هنگام دریافت جزئیات بیمار رخ داد. شناسه بیمار: {PatientId}. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientDetailsViewModel>.Failed(
                    "خطای سیستمی رخ داد. لطفاً با پشتیبانی تماس بگیرید.",
                    "DETAILS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت اطلاعات یک بیمار برای پر کردن فرم ویرایش
        /// </summary>
        public async Task<ServiceResult<PatientCreateEditViewModel>> GetPatientForEditAsync(int patientId)
        {
            _log.Information(
                "درخواست اطلاعات بیمار شناسه {PatientId} برای ویرایش. کاربر: {UserName} (شناسه: {UserId})",
                patientId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var patient = await _context.Patients
                    .Include(p => p.Insurance)
                    .Include(p => p.CreatedByUser)
                    .Include(p => p.UpdatedByUser)
                    .Where(p => p.PatientId == patientId && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    _log.Warning(
                        "دریافت اطلاعات بیمار شناسه {PatientId} برای ویرایش ناموفق بود. خطا: بیمار یافت نشد. کاربر: {UserName} (شناسه: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<PatientCreateEditViewModel>.Failed(
                        "بیمار یافت نشد یا حذف شده است.",
                        "PATIENT_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                var model = ConvertToPatientCreateEditViewModel(patient);

                _log.Information(
                    "دریافت اطلاعات بیمار شناسه {PatientId} برای ویرایش با موفقیت انجام شد. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientCreateEditViewModel>.Successful(
                    model,
                    "اطلاعات بیمار برای ویرایش آماده شد.",
                    operationName: "GetPatientForEdit",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت اطلاعات بیمار شناسه {PatientId} برای ویرایش. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientCreateEditViewModel>.Failed(
                    "خطا در دریافت اطلاعات. لطفاً دوباره تلاش کنید.",
                    "EDIT_DATA_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// ایجاد یک بیمار جدید با رعایت تمام استانداردهای امنیتی و پزشکی
        /// </summary>
        public async Task<ServiceResult> CreatePatientAsync(PatientCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ایجاد بیمار جدید با کد ملی {NationalCode}. کاربر: {UserName} (شناسه: {UserId})",
                model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // نرمال‌سازی و اعتبارسنجی
                string normalizedNationalCode = PersianNumberHelper.ToEnglishNumbers(model.NationalCode);
                string normalizedPhoneNumber = PersianNumberHelper.ToEnglishNumbers(model.PhoneNumber);

                if (!PersianNumberHelper.IsValidNationalCode(normalizedNationalCode))
                {
                    _log.Warning("کد ملی نامعتبر برای ایجاد بیمار: {NationalCode}", normalizedNationalCode);
                    return ServiceResult.Failed(
                        "کد ملی وارد شده معتبر نیست.",
                        "INVALID_NATIONAL_CODE",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                if (!PersianNumberHelper.IsValidPhoneNumber(normalizedPhoneNumber))
                {
                    _log.Warning("شماره موبایل نامعتبر برای ایجاد بیمار: {PhoneNumber}", normalizedPhoneNumber);
                    return ServiceResult.Failed(
                        "شماره موبایل وارد شده معتبر نیست.",
                        "INVALID_PHONE_NUMBER",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // بررسی وجود کد ملی تکراری
                bool nationalCodeExists = await _context.Patients
                    .AnyAsync(p => p.NationalCode == normalizedNationalCode && !p.IsDeleted);

                if (nationalCodeExists)
                {
                    _log.Warning("تلاش برای ایجاد بیمار با کد ملی تکراری: {NationalCode}", normalizedNationalCode);
                    return ServiceResult.Failed(
                        "بیماری با این کد ملی قبلاً ثبت شده است.",
                        "DUPLICATE_NATIONAL_CODE",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // بررسی وجود شماره موبایل تکراری
                bool phoneNumberExists = await _context.Patients
                    .AnyAsync(p => p.PhoneNumber == normalizedPhoneNumber && !p.IsDeleted);

                if (phoneNumberExists)
                {
                    _log.Warning("تلاش برای ایجاد بیمار با شماره موبایل تکراری: {PhoneNumber}", normalizedPhoneNumber);
                    return ServiceResult.Failed(
                        "بیماری با این شماره موبایل قبلاً ثبت شده است.",
                        "DUPLICATE_PHONE_NUMBER",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // دریافت بیمه آزاد به عنوان پیش‌فرض
                var freeInsurance = await _context.Insurances
                    .FirstOrDefaultAsync(i => i.Name == SystemConstants.FreeInsuranceName && !i.IsDeleted);

                if (freeInsurance == null)
                {
                    _log.Error("بیمه آزاد در پایگاه داده یافت نشد. کاربر: {UserName} (شناسه: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult.Failed(
                        "سیستم به درستی پیکربندی نشده است. لطفاً با پشتیبانی تماس بگیرید.",
                        "SYSTEM_CONFIG_ERROR",
                        ErrorCategory.General,
                        SecurityLevel.High);
                }

                // ایجاد بیمار جدید
                var patient = new Patient
                {
                    NationalCode = normalizedNationalCode,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = normalizedPhoneNumber,
                    Gender = model.Gender,
                    Address = model.Address,
       
                };

                // تبدیل تاریخ شمسی به میلادی
                if (!string.IsNullOrWhiteSpace(model.BirthDateShamsi))
                {
                    try
                    {
                        patient.BirthDate = PersianDateHelper.ToGregorianDate(model.BirthDateShamsi);
                    }
                    catch
                    {
                        return ServiceResult.Failed(
                            "تاریخ تولد وارد شده معتبر نیست.",
                            "INVALID_BIRTH_DATE",
                            ErrorCategory.Validation,
                            SecurityLevel.Low);
                    }
                }

                // تنظیم بیمه
                patient.InsuranceId = model.InsuranceId > 0 ?
                    model.InsuranceId : freeInsurance.InsuranceId;

                // تنظیمات ردیابی
                patient.IsDeleted = false;
                patient.CreatedAt = DateTime.UtcNow;
                patient.CreatedByUserId = _currentUserService.UserId;
                patient.UpdatedAt = null;
                patient.UpdatedByUserId = null;

                // افزودن به دیتابیس
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                _log.Information("بیمار جدید با موفقیت ایجاد شد. کد ملی: {NationalCode}", normalizedNationalCode);
                return ServiceResult.Successful(
                    "بیمار با موفقیت ایجاد شد.",
                    operationName: "CreatePatient",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName,
                    securityLevel: SecurityLevel.Medium);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در ایجاد بیمار با کد ملی {NationalCode}", model.NationalCode);
                return ServiceResult.Failed(
                    "خطا در ایجاد بیمار. لطفاً دوباره تلاش کنید.",
                    "CREATE_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// به‌روزرسانی بیمار موجود با رعایت تمام استانداردهای امنیتی و پزشکی
        /// </summary>
        public async Task<ServiceResult> UpdatePatientAsync(PatientCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ویرایش بیمار شناسه {PatientId} با کد ملی {NationalCode}. کاربر: {UserName} (شناسه: {UserId})",
                model.PatientId, model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // دریافت بیمار
                var patient = await _context.Patients
                    .Include(p => p.Insurance)
                    .Where(p => p.PatientId == model.PatientId && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    _log.Warning(
                        "تلاش برای ویرایش بیمار غیرفعال یا حذف شده با شناسه: {PatientId}", model.PatientId);
                    return ServiceResult.Failed(
                        "بیمار مورد نظر یافت نشد یا حذف شده است.",
                        "PATIENT_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                // نرمال‌سازی و اعتبارسنجی
                string normalizedNationalCode = PersianNumberHelper.ToEnglishNumbers(model.NationalCode);
                string normalizedPhoneNumber = PersianNumberHelper.ToEnglishNumbers(model.PhoneNumber);

                if (!PersianNumberHelper.IsValidNationalCode(normalizedNationalCode))
                {
                    _log.Warning("کد ملی نامعتبر برای ویرایش بیمار: {NationalCode}", normalizedNationalCode);
                    return ServiceResult.Failed(
                        "کد ملی وارد شده معتبر نیست.",
                        "INVALID_NATIONAL_CODE",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                if (!PersianNumberHelper.IsValidPhoneNumber(normalizedPhoneNumber))
                {
                    _log.Warning("شماره موبایل نامعتبر برای ویرایش بیمار: {PhoneNumber}", normalizedPhoneNumber);
                    return ServiceResult.Failed(
                        "شماره موبایل وارد شده معتبر نیست.",
                        "INVALID_PHONE_NUMBER",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // بررسی وجود کد ملی تکراری (به جز خود بیمار جاری)
                bool nationalCodeExists = await _context.Patients
                    .AnyAsync(p => p.NationalCode == normalizedNationalCode &&
                                   p.PatientId != model.PatientId &&
                                   !p.IsDeleted);

                if (nationalCodeExists)
                {
                    _log.Warning("تلاش برای ویرایش بیمار با کد ملی تکراری: {NationalCode}", normalizedNationalCode);
                    return ServiceResult.Failed(
                        "بیماری با این کد ملی قبلاً ثبت شده است.",
                        "DUPLICATE_NATIONAL_CODE",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // بررسی وجود شماره موبایل تکراری (به جز خود بیمار جاری)
                bool phoneNumberExists = await _context.Patients
                    .AnyAsync(p => p.PhoneNumber == normalizedPhoneNumber &&
                                   p.PatientId != model.PatientId &&
                                   !p.IsDeleted);

                if (phoneNumberExists)
                {
                    _log.Warning("تلاش برای ویرایش بیمار با شماره موبایل تکراری: {PhoneNumber}", normalizedPhoneNumber);
                    return ServiceResult.Failed(
                        "بیماری با این شماره موبایل قبلاً ثبت شده است.",
                        "DUPLICATE_PHONE_NUMBER",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // به‌روزرسانی اطلاعات
                UpdatePatientModel(patient, model);
                patient.UpdatedAt = DateTime.UtcNow;
                patient.UpdatedByUserId = _currentUserService.UserId;

                // ذخیره تغییرات
                await _context.SaveChangesAsync();

                _log.Information("بیمار با شناسه {PatientId} با موفقیت به‌روزرسانی شد", model.PatientId);
                return ServiceResult.Successful(
                    "بیمار با موفقیت به‌روزرسانی شد.",
                    operationName: "UpdatePatient",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName,
                    securityLevel: SecurityLevel.Medium);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _log.Error(ex, "خطای تداخل همزمانی در ویرایش بیمار با شناسه: {PatientId}", model.PatientId);
                return ServiceResult.Failed(
                    "اطلاعات بیمار توسط کاربر دیگری تغییر کرده است. لطفاً صفحه را رفرش کنید و دوباره تلاش کنید.",
                    "CONCURRENCY_ERROR",
                    ErrorCategory.Validation,
                    SecurityLevel.Medium);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در ویرایش بیمار با شناسه: {PatientId}", model.PatientId);
                return ServiceResult.Failed(
                    "خطا در ویرایش بیمار. لطفاً دوباره تلاش کنید.",
                    "UPDATE_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// حذف نرم (Soft-delete) یک بیمار با رعایت تمام استانداردهای پزشکی و حفظ اطلاعات مالی
        /// </summary>
        public async Task<ServiceResult> DeletePatientAsync(int patientId)
        {
            _log.Information(
                "درخواست حذف بیمار شناسه {PatientId}. کاربر: {UserName} (شناسه: {UserId})",
                patientId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // دریافت بیمار
                var patient = await _context.Patients
                    .Include(p => p.Receptions)
                    .Include(p => p.Appointments)
                    .Where(p => p.PatientId == patientId && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    _log.Warning(
                        "تلاش برای حذف بیمار غیرفعال یا حذف شده با شناسه: {PatientId}", patientId);
                    return ServiceResult.Failed(
                        "بیمار مورد نظر یافت نشد یا قبلاً حذف شده است.",
                        "PATIENT_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                // بررسی پذیرش‌های فعال
                bool hasActiveReceptions = patient.Receptions.Any(r =>
                    r.Status == ReceptionStatus.Pending ||
                    r.Status == ReceptionStatus.InProgress);

                if (hasActiveReceptions)
                {
                    _log.Warning("تلاش برای حذف بیمار با پذیرش‌های فعال: {PatientId}", patientId);
                    return ServiceResult.Failed(
                        "امکان حذف بیمار با پذیرش‌های فعال وجود ندارد.",
                        "ACTIVE_RECEPTIONS_EXIST",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // بررسی نوبت‌های آینده
                bool hasFutureAppointments = patient.Appointments.Any(a =>
                    a.AppointmentDate > DateTime.Now &&
                    a.Status != AppointmentStatus.Cancelled);

                if (hasFutureAppointments)
                {
                    _log.Warning("تلاش برای حذف بیمار با نوبت‌های آینده: {PatientId}", patientId);
                    return ServiceResult.Failed(
                        "امکان حذف بیمار با نوبت‌های آینده وجود ندارد.",
                        "FUTURE_APPOINTMENTS_EXIST",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // انجام حذف نرم
                patient.IsDeleted = true;
                patient.DeletedAt = DateTime.UtcNow;
                patient.DeletedByUserId = _currentUserService.UserId;

                await _context.SaveChangesAsync();

                _log.Information("بیمار با شناسه {PatientId} با موفقیت حذف نرم شد", patientId);
                return ServiceResult.Successful(
                    "بیمار با موفقیت حذف شد.",
                    operationName: "DeletePatient",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName,
                    securityLevel: SecurityLevel.Medium);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در حذف بیمار با شناسه: {PatientId}", patientId);
                return ServiceResult.Failed(
                    "خطا در حذف بیمار. لطفاً دوباره تلاش کنید.",
                    "DELETE_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }
    }
}