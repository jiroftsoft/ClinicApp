using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// سرویس تخصصی برای گزارش‌گیری پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل گزارش‌گیری و آمار پزشکان
    /// 2. رعایت استانداردهای پزشکی ایران در گزارش‌گیری
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای گزارش‌گیری
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 6. بررسی دسترسی‌های پزشکان به خدمات و دسته‌بندی‌ها
    /// 7. تولید گزارش‌های تخصصی برای مدیریت پزشکان
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorReportingService : IDoctorReportingService
    {
        private readonly IDoctorReportingRepository _doctorReportingRepository;
        private readonly IDoctorCrudRepository _doctorRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DoctorReportingService(
            IDoctorReportingRepository doctorReportingRepository,
            IDoctorCrudRepository doctorRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _doctorReportingRepository = doctorReportingRepository ?? throw new ArgumentNullException(nameof(doctorReportingRepository));
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = Log.ForContext<DoctorReportingService>();
        }

        #region Specialized Queries (پرس‌وجوهای تخصصی)

        /// <summary>
        /// بررسی دسترسی پزشک به یک دسته‌بندی خدمات خاص
        /// </summary>
        public async Task<ServiceResult<bool>> HasAccessToServiceCategoryAsync(int doctorId, int serviceCategoryId)
        {
            try
            {
                _logger.Information("درخواست بررسی دسترسی پزشک {DoctorId} به دسته‌بندی خدمات {ServiceCategoryId}", doctorId, serviceCategoryId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<bool>.Failed("شناسه پزشک نامعتبر است.");
                }

                if (serviceCategoryId <= 0)
                {
                    return ServiceResult<bool>.Failed("شناسه دسته‌بندی خدمات نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<bool>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // بررسی دسترسی از طریق بررسی انتسابات پزشک
                var hasAccess = doctor.DoctorServiceCategories?.Any(dsc => 
                    dsc.ServiceCategoryId == serviceCategoryId && 
                    dsc.IsActive && 
                    !dsc.IsDeleted) ?? false;

                _logger.Information("نتیجه بررسی دسترسی پزشک {DoctorId} به دسته‌بندی خدمات {ServiceCategoryId}: {HasAccess}", 
                    doctorId, serviceCategoryId, hasAccess);

                return ServiceResult<bool>.Successful(hasAccess);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی پزشک {DoctorId} به دسته‌بندی خدمات {ServiceCategoryId}", doctorId, serviceCategoryId);
                return ServiceResult<bool>.Failed("خطا در بررسی دسترسی پزشک");
            }
        }

        /// <summary>
        /// بررسی دسترسی پزشک به یک خدمت خاص
        /// </summary>
        public async Task<ServiceResult<bool>> HasAccessToServiceAsync(int doctorId, int serviceId)
        {
            try
            {
                _logger.Information("درخواست بررسی دسترسی پزشک {DoctorId} به خدمت {ServiceId}", doctorId, serviceId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<bool>.Failed("شناسه پزشک نامعتبر است.");
                }

                if (serviceId <= 0)
                {
                    return ServiceResult<bool>.Failed("شناسه خدمت نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<bool>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // بررسی دسترسی از طریق بررسی انتسابات پزشک
                var hasAccess = doctor.DoctorServiceCategories?.Any(dsc => 
                    dsc.ServiceCategory?.Services?.Any(s => s.ServiceId == serviceId && !s.IsDeleted) == true && 
                    dsc.IsActive && 
                    !dsc.IsDeleted) ?? false;

                _logger.Information("نتیجه بررسی دسترسی پزشک {DoctorId} به خدمت {ServiceId}: {HasAccess}", 
                    doctorId, serviceId, hasAccess);

                return ServiceResult<bool>.Successful(hasAccess);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی پزشک {DoctorId} به خدمت {ServiceId}", doctorId, serviceId);
                return ServiceResult<bool>.Failed("خطا در بررسی دسترسی پزشک");
            }
        }

        /// <summary>
        /// دریافت لیست پزشکان فعال در یک بازه زمانی برای گزارش‌گیری
        /// </summary>
        public async Task<ServiceResult<ActiveDoctorsReportViewModel>> GetActiveDoctorsReportAsync(int clinicId, int? departmentId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Information("درخواست گزارش پزشکان فعال برای کلینیک {ClinicId} از {StartDate} تا {EndDate}", 
                    clinicId, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"));

                // اعتبارسنجی پارامترها
                if (clinicId <= 0)
                {
                    return ServiceResult<ActiveDoctorsReportViewModel>.Failed("شناسه کلینیک نامعتبر است.");
                }

                if (startDate >= endDate)
                {
                    return ServiceResult<ActiveDoctorsReportViewModel>.Failed("تاریخ شروع باید قبل از تاریخ پایان باشد.");
                }

                if (startDate < DateTime.Today.AddYears(-1))
                {
                    return ServiceResult<ActiveDoctorsReportViewModel>.Failed("تاریخ شروع نمی‌تواند بیش از یک سال گذشته باشد.");
                }

                if (endDate > DateTime.Today.AddYears(1))
                {
                    return ServiceResult<ActiveDoctorsReportViewModel>.Failed("تاریخ پایان نمی‌تواند بیش از یک سال آینده باشد.");
                }

                // دریافت پزشکان فعال از repository
                var activeDoctors = await _doctorReportingRepository.GetActiveDoctorsReportAsync(clinicId, departmentId, startDate, endDate);

                // ایجاد فیلترهای گزارش
                var filters = new ReportFilterViewModel
                {
                    ClinicId = clinicId,
                    DepartmentId = departmentId,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsActive = true
                };

                // تبدیل به ViewModel
                var report = ActiveDoctorsReportViewModel.FromEntity(activeDoctors, filters);

                // تنظیم اطلاعات تولید کننده گزارش
                report.GeneratedBy = _currentUserService.UserName ?? _currentUserService.UserId;

                _logger.Information("گزارش پزشکان فعال با موفقیت تولید شد. تعداد پزشکان: {DoctorCount}", report.Doctors.Count);

                return ServiceResult<ActiveDoctorsReportViewModel>.Successful(report);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید گزارش پزشکان فعال برای کلینیک {ClinicId}", clinicId);
                return ServiceResult<ActiveDoctorsReportViewModel>.Failed("خطا در تولید گزارش پزشکان فعال");
            }
        }

        #endregion

        #region Data & Reporting (داده و گزارش‌گیری)

        /// <summary>
        /// دریافت داده‌های کلیدی برای داشبورد یک پزشک (مانند تعداد نوبت‌های امروز)
        /// </summary>
        public async Task<ServiceResult<DoctorDashboardViewModel>> GetDoctorDashboardDataAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست داده‌های داشبورد برای پزشک {DoctorId}", doctorId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<DoctorDashboardViewModel>.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<DoctorDashboardViewModel>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت داده‌های کامل پزشک از repository
                var doctorWithDetails = await _doctorReportingRepository.GetDoctorDashboardDataAsync(doctorId);
                if (doctorWithDetails == null)
                {
                    _logger.Warning("داده‌های داشبورد برای پزشک {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<DoctorDashboardViewModel>.Failed("داده‌های داشبورد یافت نشد.");
                }

                // تبدیل به ViewModel
                var dashboardData = DoctorDashboardViewModel.FromEntity(doctorWithDetails);

                _logger.Information("داده‌های داشبورد پزشک {DoctorId} با موفقیت دریافت شد. نوبت‌های امروز: {TodayCount}, نوبت‌های فردا: {TomorrowCount}", 
                    doctorId, dashboardData.TodayAppointmentsCount, dashboardData.TomorrowAppointmentsCount);

                return ServiceResult<DoctorDashboardViewModel>.Successful(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های داشبورد پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorDashboardViewModel>.Failed("خطا در دریافت داده‌های داشبورد");
            }
        }

        #endregion

        #region Private Helper Methods (متدهای کمکی خصوصی)

        /// <summary>
        /// اعتبارسنجی تاریخ‌های گزارش
        /// </summary>
        private bool ValidateReportDates(DateTime startDate, DateTime endDate, out string errorMessage)
        {
            errorMessage = null;

            if (startDate >= endDate)
            {
                errorMessage = "تاریخ شروع باید قبل از تاریخ پایان باشد.";
                return false;
            }

            if (startDate < DateTime.Today.AddYears(-1))
            {
                errorMessage = "تاریخ شروع نمی‌تواند بیش از یک سال گذشته باشد.";
                return false;
            }

            if (endDate > DateTime.Today.AddYears(1))
            {
                errorMessage = "تاریخ پایان نمی‌تواند بیش از یک سال آینده باشد.";
                return false;
            }

            if ((endDate - startDate).TotalDays > 365)
            {
                errorMessage = "فاصله بین تاریخ شروع و پایان نمی‌تواند بیش از یک سال باشد.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// اعتبارسنجی شناسه‌های کلینیک و دپارتمان
        /// </summary>
        private bool ValidateClinicAndDepartment(int clinicId, int? departmentId, out string errorMessage)
        {
            errorMessage = null;

            if (clinicId <= 0)
            {
                errorMessage = "شناسه کلینیک نامعتبر است.";
                return false;
            }

            if (departmentId.HasValue && departmentId.Value <= 0)
            {
                errorMessage = "شناسه دپارتمان نامعتبر است.";
                return false;
            }

            return true;
        }

        #endregion
    }
}
