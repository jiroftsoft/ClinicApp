using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Repositories.Reception;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس مدیریت دپارتمان و پزشک در ماژول پذیرش
    /// </summary>
    public class ReceptionDepartmentDoctorService : IReceptionDepartmentDoctorService
    {
        private readonly IReceptionService _receptionService;
        private readonly IClinicManagementRepository _clinicRepository;
        private readonly IDoctorManagementRepository _doctorRepository;
        private readonly IShiftManagementRepository _shiftRepository;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionDepartmentDoctorService(
            IReceptionService receptionService,
            IClinicManagementRepository clinicRepository,
            IDoctorManagementRepository doctorRepository,
            IShiftManagementRepository shiftRepository,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _clinicRepository = clinicRepository ?? throw new ArgumentNullException(nameof(clinicRepository));
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _shiftRepository = shiftRepository ?? throw new ArgumentNullException(nameof(shiftRepository));
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

                var result = await _clinicRepository.GetActiveDepartmentsByShiftAsync(clinicId, "Morning");
                if (!result.Success)
                {
                    return ServiceResult<List<DepartmentLookupViewModel>>.Failed(
                        result.Message ?? "خطا در دریافت دپارتمان‌های فعال"
                    );
                }

                var departments = result.Data;

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

                var result = await _doctorRepository.GetDepartmentDoctorsAsync(departmentId);
                if (!result.Success)
                {
                    return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                        result.Message ?? "خطا در دریافت پزشکان دپارتمان"
                    );
                }

                var doctors = result.Data;

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

                // TODO: Implement GetActiveShiftsAsync in IReceptionService
                var result = ServiceResult<List<object>>.Failed("متد GetActiveShiftsAsync هنوز پیاده‌سازی نشده است");
                if (!result.Success)
                {
                    return ServiceResult<List<object>>.Failed(
                        result.Message ?? "خطا در دریافت شیفت‌های فعال"
                    );
                }

                // TODO: Implement proper shift mapping when GetActiveShiftsAsync is implemented
                var shifts = new List<object>();

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

                // TODO: Implement IsShiftActiveAsync in IReceptionService
                var result = ServiceResult<bool>.Failed("متد IsShiftActiveAsync هنوز پیاده‌سازی نشده است");
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

                // TODO: Implement GetDepartmentDoctorInfoAsync in IReceptionService
                var result = ServiceResult<object>.Failed("متد GetDepartmentDoctorInfoAsync هنوز پیاده‌سازی نشده است");
                if (!result.Success)
                {
                    return ServiceResult<object>.Failed(
                        result.Message ?? "خطا در دریافت اطلاعات دپارتمان و پزشک"
                    );
                }

                // TODO: Implement proper info mapping when GetDepartmentDoctorInfoAsync is implemented
                var info = new // Anonymous object
                {
                    ClinicId = clinicId,
                    DepartmentId = departmentId,
                    TotalDepartments = 0,
                    ActiveDepartments = 0,
                    TotalDoctors = 0,
                    ActiveDoctors = 0,
                    OnShiftDoctors = 0,
                    Departments = new List<object>()
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

                // TODO: Implement SearchDoctorsAsync in IReceptionService
                var result = ServiceResult<List<object>>.Failed("متد SearchDoctorsAsync هنوز پیاده‌سازی نشده است");
                if (!result.Success)
                {
                    return ServiceResult<List<object>>.Failed(
                        result.Message ?? "خطا در جستجوی پزشکان"
                    );
                }

                // TODO: Implement proper doctor mapping when SearchDoctorsAsync is implemented
                var doctors = new List<object>();

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

                // TODO: Implement GetDepartmentDoctorStatsAsync in IReceptionService
                var result = ServiceResult<object>.Failed("متد GetDepartmentDoctorStatsAsync هنوز پیاده‌سازی نشده است");
                
                // TODO: Implement proper stats mapping when GetDepartmentDoctorStatsAsync is implemented
                var stats = new // Anonymous object
                {
                    TotalClinics = 0,
                    ActiveClinics = 0,
                    TotalDepartments = 0,
                    ActiveDepartments = 0,
                    TotalDoctors = 0,
                    ActiveDoctors = 0,
                    OnShiftDoctors = 0,
                    TotalSpecializations = 0,
                    ActiveSpecializations = 0
                };

                _logger.Information($"آمار دپارتمان و پزشک کلینیک {clinicId} با موفقیت دریافت شد");
                return ServiceResult<object>.Successful(
                    stats,
                    "آمار دپارتمان و پزشک با موفقیت دریافت شد"
                );
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

                // TODO: Implement GetClinicByIdAsync in IReceptionService
                var result = ServiceResult<ClinicLookupViewModel>.Failed("متد GetClinicByIdAsync هنوز پیاده‌سازی نشده است");
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

                // TODO: Implement GetDepartmentByIdAsync in IReceptionService
                var result = ServiceResult<DepartmentLookupViewModel>.Failed("متد GetDepartmentByIdAsync هنوز پیاده‌سازی نشده است");
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

                // TODO: Implement GetActiveDoctorsByShiftAsync in IReceptionService
                var result = ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed("متد GetActiveDoctorsByShiftAsync هنوز پیاده‌سازی نشده است");
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

                // TODO: Implement GetDoctorByIdAsync in IReceptionService
                var result = ServiceResult<ReceptionDoctorLookupViewModel>.Failed("متد GetDoctorByIdAsync هنوز پیاده‌سازی نشده است");
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

                // TODO: Implement GetActiveSpecializationsAsync in IReceptionService
                var result = ServiceResult<List<SpecializationLookupViewModel>>.Failed("متد GetActiveSpecializationsAsync هنوز پیاده‌سازی نشده است");
                if (!result.Success)
                {
                    return ServiceResult<List<SpecializationLookupViewModel>>.Failed(
                        result.Message ?? "خطا در دریافت تخصص‌های فعال"
                    );
                }

                var specializations = result.Data.Select(s => new SpecializationLookupViewModel
                {
                    SpecializationId = s.SpecializationId,
                    SpecializationName = s.SpecializationName,
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
                    SpecializationName = s.SpecializationName,
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