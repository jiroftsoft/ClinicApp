using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس تخصصی مدیریت اطلاعات پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ترکیب اطلاعات هویتی با اطلاعات پذیرش
    /// 2. مدیریت تاریخ و زمان پذیرش
    /// 3. مدیریت نوع و وضعیت پذیرش
    /// 4. مدیریت اولویت و اورژانس
    /// 5. بهینه‌سازی برای محیط درمانی
    /// 
    /// نکته حیاتی: این سرویس از ماژول‌های موجود استفاده می‌کند
    /// </summary>
    public class ReceptionInformationService
    {
        private readonly IReceptionService _receptionService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionInformationService(
            IReceptionService receptionService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _logger = logger.ForContext<ReceptionInformationService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Reception Information Management

        /// <summary>
        /// ایجاد اطلاعات پذیرش جدید
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="receptionType">نوع پذیرش</param>
        /// <param name="isEmergency">آیا اورژانس است؟</param>
        /// <param name="priority">اولویت</param>
        /// <returns>اطلاعات پذیرش ایجاد شده</returns>
        public async Task<ServiceResult<ReceptionInformationViewModel>> CreateReceptionInformationAsync(
            int patientId, 
            ReceptionType receptionType = ReceptionType.Normal, 
            bool isEmergency = false, 
            AppointmentPriority priority = AppointmentPriority.Normal)
        {
            try
            {
                _logger.Information("🏥 ایجاد اطلاعات پذیرش جدید. PatientId: {PatientId}, Type: {Type}, Emergency: {Emergency}, Priority: {Priority}, User: {UserName}", 
                    patientId, receptionType, isEmergency, priority, _currentUserService.UserName);

                var viewModel = new ReceptionInformationViewModel
                {
                    PatientId = patientId,
                    ReceptionType = receptionType,
                    IsEmergency = isEmergency,
                    Priority = priority,
                    Status = ReceptionStatus.Pending,
                    ReceptionDate = DateTime.Now,
                    ReceptionDateShamsi = DateTime.Now.ToPersianDateTime(),
                    ReceptionTime = DateTime.Now.ToString("HH:mm"),
                    CreatedBy = _currentUserService.UserName,
                    CreatedDate = DateTime.Now,
                    IsOnlineReception = false
                };

                _logger.Information("✅ اطلاعات پذیرش ایجاد شد. PatientId: {PatientId}, ReceptionDate: {Date}", 
                    patientId, viewModel.ReceptionDate);

                return ServiceResult<ReceptionInformationViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ایجاد اطلاعات پذیرش. PatientId: {PatientId}", patientId);
                return ServiceResult<ReceptionInformationViewModel>.Failed("خطا در ایجاد اطلاعات پذیرش");
            }
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات پذیرش
        /// </summary>
        /// <param name="receptionInfo">اطلاعات پذیرش</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        public async Task<ServiceResult<bool>> UpdateReceptionInformationAsync(ReceptionInformationViewModel receptionInfo)
        {
            try
            {
                _logger.Information("🔄 به‌روزرسانی اطلاعات پذیرش. PatientId: {PatientId}, Type: {Type}, User: {UserName}", 
                    receptionInfo.PatientId, receptionInfo.ReceptionType, _currentUserService.UserName);

                // اینجا باید منطق به‌روزرسانی را پیاده‌سازی کنید
                // بدون تغییر ماژول‌های موجود

                _logger.Information("✅ اطلاعات پذیرش به‌روزرسانی شد. PatientId: {PatientId}", receptionInfo.PatientId);
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در به‌روزرسانی اطلاعات پذیرش. PatientId: {PatientId}", receptionInfo.PatientId);
                return ServiceResult<bool>.Failed("خطا در به‌روزرسانی اطلاعات پذیرش");
            }
        }

        #endregion

        #region Reception Type and Status Management

        /// <summary>
        /// دریافت انواع پذیرش
        /// </summary>
        /// <returns>لیست انواع پذیرش</returns>
        public ServiceResult<List<ReceptionTypeViewModel>> GetReceptionTypes()
        {
            try
            {
                _logger.Information("📋 دریافت انواع پذیرش. User: {UserName}", _currentUserService.UserName);

                var types = Enum.GetValues(typeof(ReceptionType))
                    .Cast<ReceptionType>()
                    .Select(t => new ReceptionTypeViewModel
                    {
                        Value = t,
                        Text = GetReceptionTypeDisplayName(t),
                        Description = GetReceptionTypeDescription(t),
                        IsActive = true
                    }).ToList();

                _logger.Information("✅ {Count} نوع پذیرش دریافت شد", types.Count);
                return ServiceResult<List<ReceptionTypeViewModel>>.Successful(types);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت انواع پذیرش");
                return ServiceResult<List<ReceptionTypeViewModel>>.Failed("خطا در دریافت انواع پذیرش");
            }
        }

        /// <summary>
        /// دریافت اولویت‌های پذیرش
        /// </summary>
        /// <returns>لیست اولویت‌ها</returns>
        public ServiceResult<List<ReceptionPriorityViewModel>> GetReceptionPriorities()
        {
            try
            {
                _logger.Information("📋 دریافت اولویت‌های پذیرش. User: {UserName}", _currentUserService.UserName);

                var priorities = Enum.GetValues(typeof(AppointmentPriority))
                    .Cast<AppointmentPriority>()
                    .Select(p => new ReceptionPriorityViewModel
                    {
                        Value = p,
                        Text = GetPriorityDisplayName(p),
                        Description = GetPriorityDescription(p),
                        IsActive = true
                    }).ToList();

                _logger.Information("✅ {Count} اولویت پذیرش دریافت شد", priorities.Count);
                return ServiceResult<List<ReceptionPriorityViewModel>>.Successful(priorities);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت اولویت‌های پذیرش");
                return ServiceResult<List<ReceptionPriorityViewModel>>.Failed("خطا در دریافت اولویت‌های پذیرش");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت نام نمایشی نوع پذیرش
        /// </summary>
        private string GetReceptionTypeDisplayName(ReceptionType type)
        {
            return type switch
            {
                ReceptionType.Normal => "عادی",
                ReceptionType.Emergency => "اورژانس",
                ReceptionType.Special => "ویژه",
                ReceptionType.Online => "آنلاین",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// دریافت توضیحات نوع پذیرش
        /// </summary>
        private string GetReceptionTypeDescription(ReceptionType type)
        {
            return type switch
            {
                ReceptionType.Normal => "پذیرش عادی بیمار",
                ReceptionType.Emergency => "پذیرش اورژانسی بیمار",
                ReceptionType.Special => "پذیرش ویژه بیمار",
                ReceptionType.Online => "پذیرش آنلاین بیمار",
                _ => "نوع پذیرش نامشخص"
            };
        }

        /// <summary>
        /// دریافت نام نمایشی اولویت
        /// </summary>
        private string GetPriorityDisplayName(AppointmentPriority priority)
        {
            return priority switch
            {
                AppointmentPriority.Low => "کم",
                AppointmentPriority.Normal => "عادی",
                AppointmentPriority.High => "بالا",
                AppointmentPriority.Critical => "بحرانی",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// دریافت توضیحات اولویت
        /// </summary>
        private string GetPriorityDescription(AppointmentPriority priority)
        {
            return priority switch
            {
                AppointmentPriority.Low => "اولویت کم",
                AppointmentPriority.Normal => "اولویت عادی",
                AppointmentPriority.High => "اولویت بالا",
                AppointmentPriority.Critical => "اولویت بحرانی",
                _ => "اولویت نامشخص"
            };
        }

        #endregion
    }
}
