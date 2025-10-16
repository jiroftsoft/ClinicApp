using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس مدیریت دپارتمان و پزشک در ماژول پذیرش
    /// </summary>
    public class ReceptionDepartmentDoctorService : IReceptionDepartmentDoctorService
    {
        private readonly IReceptionService _receptionService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionDepartmentDoctorService(
            IReceptionService receptionService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _logger = logger.ForContext<ReceptionDepartmentDoctorService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// دریافت لیست کلینیک‌های فعال
        /// </summary>
        public async Task<ServiceResult<List<ClinicLookupViewModel>>> GetActiveClinicsAsync()
        {
            try
            {
                _logger.Information("🏥 دریافت لیست کلینیک‌های فعال");

                var result = await _receptionService.GetActiveClinicsAsync();
                if (!result.Success)
                {
                    return ServiceResult<List<ClinicLookupViewModel>>.Failed(
                        result.Message ?? "خطا در دریافت لیست کلینیک‌ها"
                    );
                }

                var clinics = result.Data.Select(c => new ClinicLookupViewModel
                {
                    ClinicId = c.ClinicId,
                    ClinicName = c.ClinicName,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    IsActive = c.IsActive
                }).ToList();

                _logger.Information($"تعداد کلینیک‌های فعال: {clinics.Count}");
                return ServiceResult<List<ClinicLookupViewModel>>.Successful(
                    clinics,
                    "لیست کلینیک‌های فعال با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست کلینیک‌های فعال");
                return ServiceResult<List<ClinicLookupViewModel>>.Failed(
                    "خطا در دریافت لیست کلینیک‌های فعال"
                );
            }
        }

        /// <summary>
        /// دریافت دپارتمان‌های یک کلینیک
        /// </summary>
        public async Task<ServiceResult<List<DepartmentLookupViewModel>>> GetClinicDepartmentsAsync(int clinicId)
        {
            try
            {
                _logger.Information($"🏥 دریافت دپارتمان‌های کلینیک: {clinicId}");

                var result = await _receptionService.GetClinicDepartmentsAsync(clinicId);
                if (!result.Success)
                {
                    return ServiceResult<List<DepartmentLookupViewModel>>.Failed(
                        result.Message ?? "خطا در دریافت دپارتمان‌های کلینیک"
                    );
                }

                var departments = result.Data.Select(d => new DepartmentLookupViewModel
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    Description = d.Description,
                    IsActive = d.IsActive
                }).ToList();

                _logger.Information($"تعداد دپارتمان‌های کلینیک {clinicId}: {departments.Count}");
                return ServiceResult<List<DepartmentLookupViewModel>>.Successful(
                    departments,
                    "دپارتمان‌های کلینیک با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت دپارتمان‌های کلینیک {clinicId}");
                return ServiceResult<List<DepartmentLookupViewModel>>.Failed(
                    "خطا در دریافت دپارتمان‌های کلینیک"
                );
            }
        }

        /// <summary>
        /// دریافت دپارتمان‌های فعال بر اساس شیفت
        /// </summary>
        public async Task<ServiceResult<List<DepartmentLookupViewModel>>> GetActiveDepartmentsByShiftAsync(int clinicId)
        {
            try
            {
                _logger.Information($"🏥 دریافت دپارتمان‌های فعال بر اساس شیفت - کلینیک: {clinicId}");

                // TODO: Implement GetActiveDepartmentsByShiftAsync in IReceptionService
                var result = ServiceResult<List<DepartmentLookupViewModel>>.Failed("متد GetActiveDepartmentsByShiftAsync هنوز پیاده‌سازی نشده است");
                if (!result.Success)
                {
                    return ServiceResult<List<DepartmentLookupViewModel>>.Failed(
                        result.Message ?? "خطا در دریافت دپارتمان‌های فعال"
                    );
                }

                var departments = result.Data.Select(d => new DepartmentLookupViewModel
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    Description = d.Description,
                    IsActive = d.IsActive
                }).ToList();

                _logger.Information($"تعداد دپارتمان‌های فعال کلینیک {clinicId}: {departments.Count}");
                return ServiceResult<List<DepartmentLookupViewModel>>.Successful(
                    departments,
                    "دپارتمان‌های فعال با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت دپارتمان‌های فعال کلینیک {clinicId}");
                return ServiceResult<List<DepartmentLookupViewModel>>.Failed(
                    "خطا در دریافت دپارتمان‌های فعال"
                );
            }
        }

        /// <summary>
        /// دریافت پزشکان یک دپارتمان
        /// </summary>
        public async Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDepartmentDoctorsAsync(int departmentId)
        {
            try
            {
                _logger.Information($"👨‍⚕️ دریافت پزشکان دپارتمان: {departmentId}");

                // TODO: Implement GetDepartmentDoctorsAsync in IReceptionService
                var result = ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed("متد GetDepartmentDoctorsAsync هنوز پیاده‌سازی نشده است");
                if (!result.Success)
                {
                    return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                        result.Message ?? "خطا در دریافت پزشکان دپارتمان"
                    );
                }

                var doctors = result.Data.Select(d => new ReceptionDoctorLookupViewModel
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    FullName = d.FullName,
                    MedicalLicenseNumber = d.MedicalLicenseNumber,
                    SpecializationName = d.SpecializationName,
                    SpecializationId = d.SpecializationId,
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    PhoneNumber = d.PhoneNumber,
                    Email = d.Email,
                    IsActive = d.IsActive,
                    DisplayName = d.DisplayName,
                    IsAvailable = d.IsAvailable,
                    TodayReceptionsCount = d.TodayReceptionsCount,
                    MaxDailyReceptions = d.MaxDailyReceptions
                }).ToList();

                _logger.Information($"تعداد پزشکان دپارتمان {departmentId}: {doctors.Count}");
                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Successful(
                    doctors,
                    "پزشکان دپارتمان با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت پزشکان دپارتمان {departmentId}");
                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                    "خطا در دریافت پزشکان دپارتمان"
                );
            }
        }

        /// <summary>
        /// دریافت پزشکان بر اساس تخصص
        /// </summary>
        public async Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDoctorsBySpecializationAsync(int specializationId)
        {
            try
            {
                _logger.Information($"👨‍⚕️ دریافت پزشکان تخصص: {specializationId}");

                var result = await _receptionService.GetDoctorsBySpecializationAsync(specializationId);
                if (!result.Success)
                {
                    return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                        result.Message ?? "خطا در دریافت پزشکان تخصص"
                    );
                }

                var doctors = result.Data.Select(d => new ReceptionDoctorLookupViewModel
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    FullName = d.FullName,
                    MedicalLicenseNumber = d.MedicalLicenseNumber,
                    SpecializationName = d.SpecializationName,
                    SpecializationId = d.SpecializationId,
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    PhoneNumber = d.PhoneNumber,
                    Email = d.Email,
                    IsActive = d.IsActive,
                    DisplayName = d.DisplayName,
                    IsAvailable = d.IsAvailable,
                    TodayReceptionsCount = d.TodayReceptionsCount,
                    MaxDailyReceptions = d.MaxDailyReceptions
                }).ToList();

                _logger.Information($"تعداد پزشکان تخصص {specializationId}: {doctors.Count}");
                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Successful(
                    doctors,
                    "پزشکان تخصص با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت پزشکان تخصص {specializationId}");
                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                    "خطا در دریافت پزشکان تخصص"
                );
            }
        }

        /// <summary>
        /// دریافت اطلاعات شیفت فعلی
        /// </summary>
        public async Task<ServiceResult<object>> GetCurrentShiftInfoAsync()
        {
            try
            {
                _logger.Information("🕐 دریافت اطلاعات شیفت فعلی");

                var shiftResult = await _receptionService.GetCurrentShiftAsync();
                if (!shiftResult.Success)
                {
                    return ServiceResult<object>.Failed(
                        shiftResult.Message ?? "خطا در دریافت شیفت فعلی"
                    );
                }

                var shiftInfoResult = await _receptionService.GetShiftInfoAsync(shiftResult.Data);
                if (!shiftInfoResult.Success)
                {
                    return ServiceResult<object>.Failed(
                        shiftInfoResult.Message ?? "خطا در دریافت اطلاعات شیفت"
                    );
                }

                var shiftInfo = shiftInfoResult.Data;
                var shiftViewModel = new // Anonymous object
                {
                    ShiftType = shiftResult.Data,
                    StartTime = shiftInfo.StartTime,
                    EndTime = shiftInfo.EndTime,
                    IsActive = shiftInfo.IsActive,
                    Description = "شیفت فعال",
                    DisplayName = GetShiftDisplayName(shiftResult.Data.ToString())
                };

                _logger.Information($"شیفت فعلی: {shiftViewModel.DisplayName}");
                return ServiceResult<object>.Successful(
                    shiftViewModel,
                    "اطلاعات شیفت فعلی با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات شیفت فعلی");
                
                return ServiceResult<object>.Failed(
                    "خطا در دریافت اطلاعات شیفت فعلی"
                );
            }
        }

        /// <summary>
        /// دریافت شیفت‌های فعال
        /// </summary>
        public async Task<ServiceResult<List<object>>> GetActiveShiftsAsync()
        {
            try
            {
                _logger.Information("🕐 دریافت شیفت‌های فعال");

                var result = await _receptionService.GetActiveShiftsAsync();
                if (!result.Success)
                {
                    return ServiceResult<List<object>>.Failed(
                        result.Message ?? "خطا در دریافت شیفت‌های فعال"
                    );
                }

                var shifts = result.Data.Select(s => new // Anonymous object
                {
                    ShiftId = s.ShiftId,
                    ShiftType = s.ShiftType,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    IsActive = s.IsActive,
                    Description = s.Description,
                    DisplayName = GetShiftDisplayName(s.ShiftType)
                }).Cast<object>().ToList();

                _logger.Information($"تعداد شیفت‌های فعال: {shifts.Count}");
                return ServiceResult<List<object>>.Successful(
                    shifts,
                    "شیفت‌های فعال با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت شیفت‌های فعال");
                
                return ServiceResult<List<object>>.Failed(
                    "خطا در دریافت شیفت‌های فعال"
                );
            }
        }

        /// <summary>
        /// بررسی فعال بودن شیفت
        /// </summary>
        public async Task<ServiceResult<bool>> IsShiftActiveAsync(int shiftId)
        {
            try
            {
                _logger.Information($"🕐 بررسی فعال بودن شیفت: {shiftId}");

                var result = await _receptionService.IsShiftActiveAsync(shiftId);
                if (!result.Success)
                {
                    return ServiceResult<bool>.Failed(
                        result.Message ?? "خطا در بررسی وضعیت شیفت"
                    );
                }

                _logger.Information($"وضعیت شیفت {shiftId}: {(result.Data ? "فعال" : "غیرفعال")}");
                return ServiceResult<bool>.Successful(
                    result.Data,
                    "وضعیت شیفت با موفقیت بررسی شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در بررسی وضعیت شیفت {shiftId}");
                
                return ServiceResult<bool>.Failed(
                    "خطا در بررسی وضعیت شیفت"
                );
            }
        }

        /// <summary>
        /// دریافت اطلاعات دپارتمان و پزشک
        /// </summary>
        public async Task<ServiceResult<object>> GetDepartmentDoctorInfoAsync(int clinicId, int? departmentId = null)
        {
            try
            {
                _logger.Information($"🏥 دریافت اطلاعات دپارتمان و پزشک - کلینیک: {clinicId}, دپارتمان: {departmentId}");

                var result = await _receptionService.GetDepartmentDoctorInfoAsync(clinicId, departmentId);
                if (!result.Success)
                {
                    return ServiceResult<object>.Failed(
                        result.Message ?? "خطا در دریافت اطلاعات دپارتمان و پزشک"
                    );
                }

                var info = new // Anonymous object
                {
                    ClinicId = clinicId,
                    DepartmentId = departmentId,
                    TotalDepartments = result.Data.TotalDepartments,
                    ActiveDepartments = result.Data.ActiveDepartments,
                    TotalDoctors = result.Data.TotalDoctors,
                    ActiveDoctors = result.Data.ActiveDoctors,
                    OnShiftDoctors = result.Data.OnShiftDoctors,
                    Departments = result.Data.Departments?.Select(d => new
                    {
                        DepartmentId = d.DepartmentId,
                        DepartmentName = d.DepartmentName,
                        DoctorCount = d.DoctorCount,
                        ActiveDoctorCount = d.ActiveDoctorCount
                    }).ToList()
                };

                _logger.Information($"اطلاعات دپارتمان و پزشک کلینیک {clinicId} با موفقیت دریافت شد");
                return ServiceResult<object>.Successful(
                    info,
                    "اطلاعات دپارتمان و پزشک با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت اطلاعات دپارتمان و پزشک کلینیک {clinicId}");
                
                return ServiceResult<object>.Failed(
                    "خطا در دریافت اطلاعات دپارتمان و پزشک"
                );
            }
        }

        /// <summary>
        /// جستجوی پزشکان
        /// </summary>
        public async Task<ServiceResult<List<object>>> SearchDoctorsAsync(object searchModel)
        {
            try
            {
                _logger.Information("🔍 جستجوی پزشکان");

                var result = await _receptionService.SearchDoctorsAsync(searchModel);
                if (!result.Success)
                {
                    return ServiceResult<List<object>>.Failed(
                        result.Message ?? "خطا در جستجوی پزشکان"
                    );
                }

                var doctors = result.Data.Select(d => new // Anonymous object
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    FullName = d.FullName,
                    SpecializationName = d.SpecializationName,
                    DepartmentName = d.DepartmentName,
                    IsActive = d.IsActive,
                    IsAvailable = d.IsAvailable,
                    DisplayName = d.DisplayName
                }).Cast<object>().ToList();

                _logger.Information($"تعداد نتایج جستجوی پزشکان: {doctors.Count}");
                return ServiceResult<List<object>>.Successful(
                    doctors,
                    "جستجوی پزشکان با موفقیت انجام شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پزشکان");
                
                return ServiceResult<List<object>>.Failed(
                    "خطا در جستجوی پزشکان"
                );
            }
        }

        /// <summary>
        /// دریافت آمار دپارتمان و پزشک
        /// </summary>
        public async Task<ServiceResult<object>> GetDepartmentDoctorStatsAsync(int clinicId)
        {
            try
            {
                _logger.Information($"📊 دریافت آمار دپارتمان و پزشک - کلینیک: {clinicId}");

                var result = await _receptionService.GetDepartmentDoctorStatsAsync(clinicId);
                
                if (result.Success)
                {
                    var stats = new // Anonymous object
                    {
                        TotalClinics = result.Data.TotalClinics,
                        ActiveClinics = result.Data.ActiveClinics,
                        TotalDepartments = result.Data.TotalDepartments,
                        ActiveDepartments = result.Data.ActiveDepartments,
                        TotalDoctors = result.Data.TotalDoctors,
                        ActiveDoctors = result.Data.ActiveDoctors,
                        OnShiftDoctors = result.Data.OnShiftDoctors,
                        TotalSpecializations = result.Data.TotalSpecializations,
                        ActiveSpecializations = result.Data.ActiveSpecializations
                    };

                    _logger.Information($"آمار دپارتمان و پزشک کلینیک {clinicId} با موفقیت دریافت شد");
                    return ServiceResult<object>.Successful(
                        stats,
                        "آمار دپارتمان و پزشک با موفقیت دریافت شد"
                    );
                }
                else
                {
                    _logger.Warning($"دریافت آمار دپارتمان و پزشک کلینیک {clinicId} با خطا مواجه شد: {result.Message}");
                    return ServiceResult<object>.Failed(
                        result.Message ?? "خطا در دریافت آمار دپارتمان و پزشک"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت آمار دپارتمان و پزشک کلینیک {clinicId}");
                
                return ServiceResult<object>.Failed(
                    "خطا در دریافت آمار دپارتمان و پزشک"
                );
            }
        }

        /// <summary>
        /// دریافت کلینیک بر اساس شناسه
        /// </summary>
        public async Task<ServiceResult<ClinicLookupViewModel>> GetClinicByIdAsync(int clinicId)
        {
            try
            {
                _logger.Information($"🏥 دریافت کلینیک بر اساس شناسه: {clinicId}");

                var result = await _receptionService.GetClinicByIdAsync(clinicId);
                if (!result.Success)
                {
                    return ServiceResult<ClinicLookupViewModel>.Failed(
                        result.Message ?? "خطا در دریافت کلینیک"
                    );
                }

                var clinic = new ClinicLookupViewModel
                {
                    ClinicId = result.Data.ClinicId,
                    ClinicName = result.Data.ClinicName,
                    Address = result.Data.Address,
                    PhoneNumber = result.Data.PhoneNumber,
                    IsActive = result.Data.IsActive
                };

                _logger.Information($"کلینیک {clinicId} با موفقیت دریافت شد");
                return ServiceResult<ClinicLookupViewModel>.Successful(
                    clinic,
                    "کلینیک با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت کلینیک {clinicId}");
                
                return ServiceResult<ClinicLookupViewModel>.Failed(
                    "خطا در دریافت کلینیک"
                );
            }
        }

        /// <summary>
        /// دریافت دپارتمان بر اساس شناسه
        /// </summary>
        public async Task<ServiceResult<DepartmentLookupViewModel>> GetDepartmentByIdAsync(int departmentId)
        {
            try
            {
                _logger.Information($"🏥 دریافت دپارتمان بر اساس شناسه: {departmentId}");

                var result = await _receptionService.GetDepartmentByIdAsync(departmentId);
                if (!result.Success)
                {
                    return ServiceResult<DepartmentLookupViewModel>.Failed(
                        result.Message ?? "خطا در دریافت دپارتمان"
                    );
                }

                var department = new DepartmentLookupViewModel
                {
                    DepartmentId = result.Data.DepartmentId,
                    DepartmentName = result.Data.DepartmentName,
                    Description = result.Data.Description,
                    IsActive = result.Data.IsActive
                };

                _logger.Information($"دپارتمان {departmentId} با موفقیت دریافت شد");
                return ServiceResult<DepartmentLookupViewModel>.Successful(
                    department,
                    "دپارتمان با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت دپارتمان {departmentId}");
                
                return ServiceResult<DepartmentLookupViewModel>.Failed(
                    "خطا در دریافت دپارتمان"
                );
            }
        }

        /// <summary>
        /// دریافت پزشکان فعال بر اساس شیفت
        /// </summary>
        public async Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetActiveDoctorsByShiftAsync(int departmentId)
        {
            try
            {
                _logger.Information($"👨‍⚕️ دریافت پزشکان فعال بر اساس شیفت - دپارتمان: {departmentId}");

                var result = await _receptionService.GetActiveDoctorsByShiftAsync(departmentId);
                if (!result.Success)
                {
                    return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                        result.Message ?? "خطا در دریافت پزشکان فعال"
                    );
                }

                var doctors = result.Data.Select(d => new ReceptionDoctorLookupViewModel
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    FullName = d.FullName,
                    MedicalLicenseNumber = d.MedicalLicenseNumber,
                    SpecializationName = d.SpecializationName,
                    SpecializationId = d.SpecializationId,
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    PhoneNumber = d.PhoneNumber,
                    Email = d.Email,
                    IsActive = d.IsActive,
                    DisplayName = d.DisplayName,
                    IsAvailable = d.IsAvailable,
                    TodayReceptionsCount = d.TodayReceptionsCount,
                    MaxDailyReceptions = d.MaxDailyReceptions
                }).ToList();

                _logger.Information($"تعداد پزشکان فعال دپارتمان {departmentId}: {doctors.Count}");
                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Successful(
                    doctors,
                    "پزشکان فعال با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت پزشکان فعال دپارتمان {departmentId}");
                
                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                    "خطا در دریافت پزشکان فعال"
                );
            }
        }

        /// <summary>
        /// دریافت پزشک بر اساس شناسه
        /// </summary>
        public async Task<ServiceResult<ReceptionDoctorLookupViewModel>> GetDoctorByIdAsync(int doctorId)
        {
            try
            {
                _logger.Information($"👨‍⚕️ دریافت پزشک بر اساس شناسه: {doctorId}");

                var result = await _receptionService.GetDoctorByIdAsync(doctorId);
                if (!result.Success)
                {
                    return ServiceResult<ReceptionDoctorLookupViewModel>.Failed(
                        result.Message ?? "خطا در دریافت پزشک"
                    );
                }

                var doctor = new ReceptionDoctorLookupViewModel
                {
                    DoctorId = result.Data.DoctorId,
                    FirstName = result.Data.FirstName,
                    LastName = result.Data.LastName,
                    FullName = result.Data.FullName,
                    MedicalLicenseNumber = result.Data.MedicalLicenseNumber,
                    SpecializationName = result.Data.SpecializationName,
                    SpecializationId = result.Data.SpecializationId,
                    DepartmentId = result.Data.DepartmentId,
                    DepartmentName = result.Data.DepartmentName,
                    PhoneNumber = result.Data.PhoneNumber,
                    Email = result.Data.Email,
                    IsActive = result.Data.IsActive,
                    DisplayName = result.Data.DisplayName,
                    IsAvailable = result.Data.IsAvailable,
                    TodayReceptionsCount = result.Data.TodayReceptionsCount,
                    MaxDailyReceptions = result.Data.MaxDailyReceptions
                };

                _logger.Information($"پزشک {doctorId} با موفقیت دریافت شد");
                return ServiceResult<ReceptionDoctorLookupViewModel>.Successful(
                    doctor,
                    "پزشک با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت پزشک {doctorId}");
                
                return ServiceResult<ReceptionDoctorLookupViewModel>.Failed(
                    "خطا در دریافت پزشک"
                );
            }
        }

        /// <summary>
        /// دریافت تخصص‌های فعال
        /// </summary>
        public async Task<ServiceResult<List<SpecializationLookupViewModel>>> GetActiveSpecializationsAsync()
        {
            try
            {
                _logger.Information("🎓 دریافت تخصص‌های فعال");

                var result = await _receptionService.GetActiveSpecializationsAsync();
                if (!result.Success)
                {
                    return ServiceResult<List<SpecializationLookupViewModel>>.Failed(
                        result.Message ?? "خطا در دریافت تخصص‌های فعال"
                    );
                }

                var specializations = result.Data.Select(s => new SpecializationLookupViewModel
                {
                    SpecializationId = s.SpecializationId,
                    Name = s.Name,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    DoctorsCount = s.DoctorsCount
                }).ToList();

                _logger.Information($"تعداد تخصص‌های فعال: {specializations.Count}");
                return ServiceResult<List<SpecializationLookupViewModel>>.Successful(
                    specializations,
                    "تخصص‌های فعال با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تخصص‌های فعال");
                
                return ServiceResult<List<SpecializationLookupViewModel>>.Failed(
                    "خطا در دریافت تخصص‌های فعال"
                );
            }
        }

        /// <summary>
        /// دریافت تخصص‌های یک دپارتمان
        /// </summary>
        public async Task<ServiceResult<List<SpecializationLookupViewModel>>> GetDepartmentSpecializationsAsync(int departmentId)
        {
            try
            {
                _logger.Information($"🎓 دریافت تخصص‌های دپارتمان: {departmentId}");

                // TODO: Implement GetDepartmentSpecializationsAsync in IReceptionService
                var result = ServiceResult<List<SpecializationLookupViewModel>>.Failed("متد GetDepartmentSpecializationsAsync هنوز پیاده‌سازی نشده است");
                if (!result.Success)
                {
                    return ServiceResult<List<SpecializationLookupViewModel>>.Failed(
                        result.Message ?? "خطا در دریافت تخصص‌های دپارتمان"
                    );
                }

                var specializations = result.Data.Select(s => new SpecializationLookupViewModel
                {
                    SpecializationId = s.SpecializationId,
                    Name = s.Name,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    DoctorsCount = s.DoctorsCount
                }).ToList();

                _logger.Information($"تعداد تخصص‌های دپارتمان {departmentId}: {specializations.Count}");
                return ServiceResult<List<SpecializationLookupViewModel>>.Successful(
                    specializations,
                    "تخصص‌های دپارتمان با موفقیت دریافت شد"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در دریافت تخصص‌های دپارتمان {departmentId}");
                
                return ServiceResult<List<SpecializationLookupViewModel>>.Failed(
                    "خطا در دریافت تخصص‌های دپارتمان"
                );
            }
        }

        /// <summary>
        /// دریافت نام نمایشی شیفت
        /// </summary>
        private string GetShiftDisplayName(string shiftType)
        {
            return shiftType switch
            {
                "Morning" => "شیفت صبح",
                "Afternoon" => "شیفت عصر",
                "Evening" => "شیفت شب",
                "Emergency" => "شیفت اورژانس",
                _ => $"شیفت {shiftType}"
            };
        }
    }
}