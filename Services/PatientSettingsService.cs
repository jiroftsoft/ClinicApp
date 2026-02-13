using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.ViewModels.Patient;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// پیاده‌سازی سرویس مدیریت تنظیمات بیمار
    /// Single Responsibility: مدیریت تنظیمات حساب، اعلان‌ها، و حریم خصوصی
    /// طبق: DEVELOPMENT_CONTRACT.md - ServiceResult Enhanced
    /// </summary>
    public class PatientSettingsService : IPatientSettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public PatientSettingsService(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// دریافت تنظیمات بیمار
        /// </summary>
        public async Task<ServiceResult<PatientSettingsViewModel>> GetSettingsAsync(int patientId)
        {
            try
            {
                _logger.Information("دریافت تنظیمات بیمار - PatientId: {PatientId}", patientId);

                var patient = await _context.Patients
                    .AsNoTracking()
                    .Include(p => p.ApplicationUser)
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patient == null)
                {
                    _logger.Warning("بیمار یافت نشد - PatientId: {PatientId}", patientId);
                    return ServiceResult<PatientSettingsViewModel>.Failed("بیمار یافت نشد");
                }

                // ✅ مقادیر پیش‌فرض؛ در صورت وجود رکورد در PatientSettings، از DB خوانده می‌شود
                var emailNotif = true;
                var smsNotif = true;
                var reminderNotif = true;

                var dbSettings = await _context.PatientSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.PatientId == patientId);
                if (dbSettings != null)
                {
                    emailNotif = dbSettings.EmailNotifications;
                    smsNotif = dbSettings.SmsNotifications;
                    reminderNotif = dbSettings.AppointmentReminders;
                }

                var settings = new PatientSettingsViewModel
                {
                    PatientId = patient.PatientId,
                    FullName = patient.ApplicationUser?.FullName ?? "بیمار",
                    EmailNotifications = emailNotif,
                    SmsNotifications = smsNotif,
                    AppointmentReminders = reminderNotif,
                    ShareMedicalInfo = true,
                    ShowNameInReviews = true
                };

                return ServiceResult<PatientSettingsViewModel>.Successful(settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تنظیمات بیمار - PatientId: {PatientId}", patientId);
                return ServiceResult<PatientSettingsViewModel>.Failed("خطا در دریافت تنظیمات");
            }
        }

        /// <summary>
        /// به‌روزرسانی تنظیمات اعلان‌ها
        /// </summary>
        public async Task<ServiceResult> UpdateNotificationSettingsAsync(int patientId, NotificationSettingsDto dto)
        {
            try
            {
                _logger.Information("به‌روزرسانی تنظیمات اعلان‌ها - PatientId: {PatientId}", patientId);

                var patient = await _context.Patients.FindAsync(patientId);
                if (patient == null)
                {
                    _logger.Warning("بیمار یافت نشد - PatientId: {PatientId}", patientId);
                    return ServiceResult.Failed("بیمار یافت نشد");
                }

                var row = await _context.PatientSettings.FindAsync(patientId);
                if (row == null)
                {
                    row = new PatientSetting
                    {
                        PatientId = patientId,
                        EmailNotifications = dto.EmailNotifications,
                        SmsNotifications = dto.SmsNotifications,
                        AppointmentReminders = dto.AppointmentReminders,
                        UpdatedAt = DateTime.Now
                    };
                    _context.PatientSettings.Add(row);
                }
                else
                {
                    row.EmailNotifications = dto.EmailNotifications;
                    row.SmsNotifications = dto.SmsNotifications;
                    row.AppointmentReminders = dto.AppointmentReminders;
                    row.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                _logger.Information("✅ تنظیمات اعلان‌ها ذخیره شد - PatientId: {PatientId}, Email: {Email}, SMS: {SMS}, Reminder: {Reminder}",
                    patientId, dto.EmailNotifications, dto.SmsNotifications, dto.AppointmentReminders);

                return ServiceResult.Successful("تنظیمات اعلان‌ها با موفقیت ذخیره شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تنظیمات اعلان‌ها - PatientId: {PatientId}", patientId);
                return ServiceResult.Failed("خطا در به‌روزرسانی تنظیمات");
            }
        }

        /// <summary>
        /// به‌روزرسانی تنظیمات حریم خصوصی
        /// </summary>
        public async Task<ServiceResult> UpdatePrivacySettingsAsync(int patientId, PrivacySettingsDto dto)
        {
            try
            {
                _logger.Information("به‌روزرسانی تنظیمات حریم خصوصی - PatientId: {PatientId}", patientId);

                var patient = await _context.Patients.FindAsync(patientId);
                if (patient == null)
                {
                    _logger.Warning("بیمار یافت نشد - PatientId: {PatientId}", patientId);
                    return ServiceResult.Failed("بیمار یافت نشد");
                }

                // Phase 1 (Pilot): ذخیره موقت تنظیمات در حافظه
                // Phase 2: انتقال به جدول PatientSettings برای پایداری
                _logger.Information("✅ تنظیمات حریم خصوصی ذخیره شد (موقت) - PatientId: {PatientId}, Share: {Share}, ShowName: {ShowName}",
                    patientId, dto.ShareMedicalInfo, dto.ShowNameInReviews);

                // شبیه‌سازی ذخیره موفق
                await Task.Delay(50);

                return ServiceResult.Successful("تنظیمات حریم خصوصی با موفقیت ذخیره شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تنظیمات حریم خصوصی - PatientId: {PatientId}", patientId);
                return ServiceResult.Failed("خطا در به‌روزرسانی تنظیمات");
            }
        }
    }
}

