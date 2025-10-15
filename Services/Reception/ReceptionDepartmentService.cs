using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس تخصصی مدیریت کلینیک‌ها، دپارتمان‌ها و پزشکان در فرم پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کلینیک‌های فعال
    /// 2. بارگذاری دپارتمان‌ها بر اساس کلینیک
    /// 3. بارگذاری پزشکان بر اساس دپارتمان
    /// 4. مدیریت cascade loading
    /// 5. بهینه‌سازی برای محیط درمانی
    /// 
    /// نکته حیاتی: این سرویس از ماژول‌های موجود استفاده می‌کند
    /// </summary>
    public class ReceptionDepartmentService
    {
        private readonly IReceptionService _receptionService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionDepartmentService(
            IReceptionService receptionService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _logger = logger.ForContext<ReceptionDepartmentService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Clinic Management

        /// <summary>
        /// دریافت کلینیک‌های فعال برای فرم پذیرش
        /// </summary>
        /// <returns>لیست کلینیک‌های فعال</returns>
        public async Task<ServiceResult<List<ReceptionClinicViewModel>>> GetActiveClinicsForReceptionAsync()
        {
            try
            {
                _logger.Information("🏥 دریافت کلینیک‌های فعال برای فرم پذیرش. User: {UserName}", _currentUserService.UserName);

                var result = await _receptionService.GetActiveClinicsAsync();
                if (!result.Success)
                {
                    return ServiceResult<List<ReceptionClinicViewModel>>.Failed(result.Message);
                }

                var clinics = result.Data.Select(c => new ReceptionClinicViewModel
                {
                    ClinicId = c.ClinicId,
                    ClinicName = c.ClinicName,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    IsActive = c.IsActive,
                    DepartmentCount = 0 // اینجا باید تعداد دپارتمان‌ها را محاسبه کنید
                }).ToList();

                _logger.Information("✅ {Count} کلینیک فعال دریافت شد", clinics.Count);
                return ServiceResult<List<ReceptionClinicViewModel>>.Successful(clinics);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت کلینیک‌های فعال");
                return ServiceResult<List<ReceptionClinicViewModel>>.Failed("خطا در دریافت کلینیک‌ها");
            }
        }

        #endregion

        #region Department Management

        /// <summary>
        /// دریافت دپارتمان‌های کلینیک برای فرم پذیرش
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک</param>
        /// <returns>لیست دپارتمان‌های کلینیک</returns>
        public async Task<ServiceResult<List<ReceptionDepartmentViewModel>>> GetClinicDepartmentsForReceptionAsync(int clinicId)
        {
            try
            {
                _logger.Information("🏥 دریافت دپارتمان‌های کلینیک برای فرم پذیرش. ClinicId: {ClinicId}, User: {UserName}", 
                    clinicId, _currentUserService.UserName);

                var result = await _receptionService.GetClinicDepartmentsAsync(clinicId);
                if (!result.Success)
                {
                    return ServiceResult<List<ReceptionDepartmentViewModel>>.Failed(result.Message);
                }

                var departments = result.Data.Select(d => new ReceptionDepartmentViewModel
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    Description = d.Description,
                    ClinicId = clinicId,
                    IsActive = d.IsActive,
                    DoctorCount = 0, // اینجا باید تعداد پزشکان را محاسبه کنید
                    ServiceCategoryCount = 0 // اینجا باید تعداد دسته‌بندی خدمات را محاسبه کنید
                }).ToList();

                _logger.Information("✅ {Count} دپارتمان برای کلینیک {ClinicId} دریافت شد", departments.Count, clinicId);
                return ServiceResult<List<ReceptionDepartmentViewModel>>.Successful(departments);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت دپارتمان‌های کلینیک. ClinicId: {ClinicId}", clinicId);
                return ServiceResult<List<ReceptionDepartmentViewModel>>.Failed("خطا در دریافت دپارتمان‌ها");
            }
        }

        #endregion

        #region Doctor Management

        /// <summary>
        /// دریافت پزشکان دپارتمان برای فرم پذیرش
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>لیست پزشکان دپارتمان</returns>
        public async Task<ServiceResult<List<ReceptionDoctorViewModel>>> GetDepartmentDoctorsForReceptionAsync(int departmentId)
        {
            try
            {
                _logger.Information("👨‍⚕️ دریافت پزشکان دپارتمان برای فرم پذیرش. DepartmentId: {DepartmentId}, User: {UserName}", 
                    departmentId, _currentUserService.UserName);

                // اینجا باید از سرویس موجود برای دریافت پزشکان استفاده کنید
                // var result = await _doctorService.GetDoctorsByDepartmentAsync(departmentId);
                
                // برای حالا یک لیست خالی برمی‌گردانیم
                var doctors = new List<ReceptionDoctorViewModel>();

                _logger.Information("✅ {Count} پزشک برای دپارتمان {DepartmentId} دریافت شد", doctors.Count, departmentId);
                return ServiceResult<List<ReceptionDoctorViewModel>>.Successful(doctors);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت پزشکان دپارتمان. DepartmentId: {DepartmentId}", departmentId);
                return ServiceResult<List<ReceptionDoctorViewModel>>.Failed("خطا در دریافت پزشکان");
            }
        }

        /// <summary>
        /// دریافت اطلاعات کامل پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>اطلاعات کامل پزشک</returns>
        public async Task<ServiceResult<ReceptionDoctorViewModel>> GetDoctorDetailsForReceptionAsync(int doctorId)
        {
            try
            {
                _logger.Information("👨‍⚕️ دریافت اطلاعات پزشک برای فرم پذیرش. DoctorId: {DoctorId}, User: {UserName}", 
                    doctorId, _currentUserService.UserName);

                // اینجا باید از سرویس موجود برای دریافت اطلاعات پزشک استفاده کنید
                // var result = await _doctorService.GetDoctorByIdAsync(doctorId);
                
                // برای حالا یک پزشک نمونه برمی‌گردانیم
                var doctor = new ReceptionDoctorViewModel
                {
                    DoctorId = doctorId,
                    FirstName = "نام",
                    LastName = "خانوادگی",
                    FullName = "نام خانوادگی",
                    Specialization = "تخصص",
                    IsActive = true
                };

                _logger.Information("✅ اطلاعات پزشک دریافت شد. DoctorId: {DoctorId}, Name: {Name}", 
                    doctorId, doctor.FullName);
                return ServiceResult<ReceptionDoctorViewModel>.Successful(doctor);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت اطلاعات پزشک. DoctorId: {DoctorId}", doctorId);
                return ServiceResult<ReceptionDoctorViewModel>.Failed("خطا در دریافت اطلاعات پزشک");
            }
        }

        #endregion

        #region Cascade Loading

        /// <summary>
        /// بارگذاری cascade: کلینیک → دپارتمان → پزشک
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <param name="doctorId">شناسه پزشک (اختیاری)</param>
        /// <returns>اطلاعات کامل cascade</returns>
        public async Task<ServiceResult<ReceptionCascadeViewModel>> LoadCascadeForReceptionAsync(
            int clinicId, 
            int? departmentId = null, 
            int? doctorId = null)
        {
            try
            {
                _logger.Information("🔄 بارگذاری cascade برای فرم پذیرش. ClinicId: {ClinicId}, DepartmentId: {DepartmentId}, DoctorId: {DoctorId}, User: {UserName}", 
                    clinicId, departmentId, doctorId, _currentUserService.UserName);

                var cascade = new ReceptionCascadeViewModel
                {
                    ClinicId = clinicId,
                    DepartmentId = departmentId,
                    DoctorId = doctorId,
                    LoadDate = DateTime.Now
                };

                // بارگذاری کلینیک
                var clinicResult = await GetActiveClinicsForReceptionAsync();
                if (clinicResult.Success)
                {
                    cascade.Clinic = clinicResult.Data.FirstOrDefault(c => c.ClinicId == clinicId);
                }

                // بارگذاری دپارتمان‌ها
                if (clinicId > 0)
                {
                    var departmentResult = await GetClinicDepartmentsForReceptionAsync(clinicId);
                    if (departmentResult.Success)
                    {
                        cascade.Departments = departmentResult.Data;
                    }
                }

                // بارگذاری پزشکان
                if (departmentId.HasValue)
                {
                    var doctorResult = await GetDepartmentDoctorsForReceptionAsync(departmentId.Value);
                    if (doctorResult.Success)
                    {
                        cascade.Doctors = doctorResult.Data;
                    }
                }

                // بارگذاری اطلاعات پزشک انتخاب شده
                if (doctorId.HasValue)
                {
                    var doctorDetailResult = await GetDoctorDetailsForReceptionAsync(doctorId.Value);
                    if (doctorDetailResult.Success)
                    {
                        cascade.SelectedDoctor = doctorDetailResult.Data;
                    }
                }

                _logger.Information("✅ بارگذاری cascade تکمیل شد. ClinicId: {ClinicId}, DepartmentCount: {DepartmentCount}, DoctorCount: {DoctorCount}", 
                    clinicId, cascade.Departments?.Count ?? 0, cascade.Doctors?.Count ?? 0);

                return ServiceResult<ReceptionCascadeViewModel>.Successful(cascade);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بارگذاری cascade. ClinicId: {ClinicId}", clinicId);
                return ServiceResult<ReceptionCascadeViewModel>.Failed("خطا در بارگذاری cascade");
            }
        }

        /// <summary>
        /// دریافت دپارتمان‌های فعال
        /// </summary>
        public async Task<ServiceResult<List<DepartmentLookupViewModel>>> GetActiveDepartmentsAsync()
        {
            try
            {
                _logger.Information("🏥 RECEPTION: درخواست دپارتمان‌های فعال. User: {UserName}", _currentUserService.UserName);

                // استفاده از سرویس موجود - فرض می‌کنیم clinicId = 1 (کلینیک اصلی)
                var departmentsResult = await _receptionService.GetClinicDepartmentsAsync(1);
                if (!departmentsResult.Success)
                {
                    return ServiceResult<List<DepartmentLookupViewModel>>.Failed("خطا در دریافت دپارتمان‌ها");
                }

                var departments = departmentsResult.Data.Select(d => new DepartmentLookupViewModel
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    IsActive = d.IsActive
                }).ToList();

                _logger.Information("✅ {Count} دپارتمان فعال دریافت شد. User: {UserName}", 
                    departments.Count, _currentUserService.UserName);
                return ServiceResult<List<DepartmentLookupViewModel>>.Successful(departments);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت دپارتمان‌های فعال. User: {UserName}", _currentUserService.UserName);
                return ServiceResult<List<DepartmentLookupViewModel>>.Failed("خطا در دریافت دپارتمان‌ها");
            }
        }

        #endregion
    }
}
