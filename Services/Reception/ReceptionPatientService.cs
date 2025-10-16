using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.ViewModels.Reception;
using ClinicApp.ViewModels;
using ClinicApp.Core;
using ClinicApp.Constants;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس ساده برای مدیریت بیماران در ماژول پذیرش
    /// </summary>
    public class ReceptionPatientService : IReceptionPatientService
    {
        private readonly IPatientService _patientService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionPatientService(
            IPatientService patientService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _patientService = patientService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// جستجوی بیمار بر اساس کد ملی
        /// </summary>
        public async Task<ServiceResult<PatientAccordionViewModel>> SearchPatientByNationalCodeAsync(string nationalCode)
        {
            try
            {
                _logger.Information($"جستجوی بیمار با کد ملی: {nationalCode}");

                // اعتبارسنجی کد ملی
                if (string.IsNullOrWhiteSpace(nationalCode))
                {
                    return ServiceResult<PatientAccordionViewModel>.Failed(
                        ReceptionFormConstants.Messages.NationalCodeInvalid
                    );
                }

                if (nationalCode.Length != ReceptionFormConstants.Validation.NationalCodeLength)
                {
                    return ServiceResult<PatientAccordionViewModel>.Failed(
                        ReceptionFormConstants.Messages.NationalCodeInvalid
                    );
                }

                // اعتبارسنجی الگوریتم کد ملی ایرانی
                if (!ValidateNationalCode(nationalCode))
                {
                    return ServiceResult<PatientAccordionViewModel>.Failed(
                        ReceptionFormConstants.Messages.NationalCodeInvalid
                    );
                }

                // جستجوی بیمار در دیتابیس
                var patient = await _patientService.GetPatientByNationalCodeAsync(nationalCode);

                if (patient != null)
                {
                    var patientViewModel = new PatientAccordionViewModel
                    {
                        NationalCode = patient.NationalCode,
                        FirstName = patient.FirstName,
                        LastName = patient.LastName,
                        BirthDate = patient.BirthDate,
                        Gender = patient.Gender.ToString(),
                        PhoneNumber = patient.PhoneNumber,
                        Address = patient.Address,
                        PatientId = patient.PatientId,
                        IsPatientFound = true,
                        StatusMessage = string.Format(ReceptionFormConstants.Messages.PatientFound, nationalCode),
                        StatusCssClass = "text-success"
                    };

                    _logger.Information($"بیمار یافت شد: {patient.FirstName} {patient.LastName}");

                    return ServiceResult<PatientAccordionViewModel>.Successful(
                        patientViewModel,
                        string.Format(ReceptionFormConstants.Messages.PatientFound, nationalCode)
                    );
                }
                else
                {
                    // بیمار یافت نشد - ایجاد ViewModel برای بیمار جدید
                    var newPatientViewModel = new PatientAccordionViewModel
                    {
                        NationalCode = nationalCode,
                        IsPatientFound = false,
                        StatusMessage = string.Format(ReceptionFormConstants.Messages.PatientNotFound, nationalCode),
                        StatusCssClass = "text-warning"
                    };

                    _logger.Information($"بیمار با کد ملی {nationalCode} یافت نشد - آماده برای ثبت جدید");

                    return ServiceResult<PatientAccordionViewModel>.Successful(
                        newPatientViewModel,
                        string.Format(ReceptionFormConstants.Messages.PatientNotFound, nationalCode)
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در جستجوی بیمار با کد ملی {nationalCode}");
                
                return ServiceResult<PatientAccordionViewModel>.Failed(
                    "خطا در جستجوی بیمار. لطفاً دوباره تلاش کنید."
                );
            }
        }

        /// <summary>
        /// ذخیره اطلاعات بیمار جدید
        /// </summary>
        public async Task<ServiceResult<PatientAccordionViewModel>> SaveNewPatientAsync(PatientAccordionViewModel model)
        {
            try
            {
                _logger.Information($"ذخیره اطلاعات بیمار جدید: {model.NationalCode}");

                // اعتبارسنجی کد ملی
                if (!ValidateNationalCode(model.NationalCode))
                {
                    return ServiceResult<PatientAccordionViewModel>.Failed(
                        ReceptionFormConstants.Messages.NationalCodeInvalid
                    );
                }

                // بررسی تکراری نبودن کد ملی
                var existingPatient = await _patientService.GetPatientByNationalCodeAsync(model.NationalCode);
                if (existingPatient != null)
                {
                    return ServiceResult<PatientAccordionViewModel>.Failed(
                        "بیماری با این کد ملی قبلاً ثبت شده است"
                    );
                }

                // ایجاد ViewModel برای ذخیره
                var patientCreateModel = new PatientCreateEditViewModel
                {
                    NationalCode = model.NationalCode,
                    FirstName = model.FirstName?.Trim(),
                    LastName = model.LastName?.Trim(),
                    BirthDate = model.BirthDate,
                    Gender = (Models.Enums.Gender)Enum.Parse(typeof(Models.Enums.Gender), model.Gender),
                    PhoneNumber = model.PhoneNumber?.Trim(),
                    Address = model.Address?.Trim()
                };

                // ذخیره بیمار
                var saveResult = await _patientService.CreatePatientAsync(patientCreateModel);

                if (saveResult.Success)
                {
                    // دریافت بیمار ذخیره شده
                    var savedPatient = await _patientService.GetPatientByNationalCodeAsync(model.NationalCode);
                    
                    var patientViewModel = new PatientAccordionViewModel
                    {
                        NationalCode = savedPatient.NationalCode,
                        FirstName = savedPatient.FirstName,
                        LastName = savedPatient.LastName,
                        BirthDate = savedPatient.BirthDate,
                        Gender = savedPatient.Gender.ToString(),
                        PhoneNumber = savedPatient.PhoneNumber,
                        Address = savedPatient.Address,
                        PatientId = savedPatient.PatientId,
                        IsPatientFound = true,
                        StatusMessage = ReceptionFormConstants.Messages.PatientSavedSuccess,
                        StatusCssClass = "text-success"
                    };

                    _logger.Information($"بیمار جدید با موفقیت ذخیره شد: {savedPatient.FirstName} {savedPatient.LastName}");

                    return ServiceResult<PatientAccordionViewModel>.Successful(
                        patientViewModel,
                        ReceptionFormConstants.Messages.PatientSavedSuccess
                    );
                }
                else
                {
                    _logger.Error($"خطا در ذخیره بیمار: {saveResult.Message}");
                    
                    return ServiceResult<PatientAccordionViewModel>.Failed(
                        saveResult.Message ?? ReceptionFormConstants.Messages.PatientSaveError
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در ذخیره اطلاعات بیمار");
                
                return ServiceResult<PatientAccordionViewModel>.Failed(
                    ReceptionFormConstants.Messages.PatientSaveError
                );
            }
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات بیمار موجود
        /// </summary>
        public async Task<ServiceResult<PatientAccordionViewModel>> UpdatePatientAsync(PatientAccordionViewModel model)
        {
            try
            {
                _logger.Information($"به‌روزرسانی اطلاعات بیمار: {model.PatientId}");

                if (!model.PatientId.HasValue)
                {
                    return ServiceResult<PatientAccordionViewModel>.Failed(
                        "شناسه بیمار مشخص نشده است"
                    );
                }

                // دریافت بیمار موجود
                var existingPatient = await _patientService.GetPatientByNationalCodeAsync(model.NationalCode);
                if (existingPatient == null)
                {
                    return ServiceResult<PatientAccordionViewModel>.Failed(
                        "بیمار یافت نشد"
                    );
                }

                // ایجاد ViewModel برای به‌روزرسانی
                var patientUpdateModel = new PatientCreateEditViewModel
                {
                    PatientId = model.PatientId.Value,
                    NationalCode = model.NationalCode,
                    FirstName = model.FirstName?.Trim(),
                    LastName = model.LastName?.Trim(),
                    BirthDate = model.BirthDate,
                    Gender = (Models.Enums.Gender)Enum.Parse(typeof(Models.Enums.Gender), model.Gender),
                    PhoneNumber = model.PhoneNumber?.Trim(),
                    Address = model.Address?.Trim()
                };

                // ذخیره تغییرات
                var updateResult = await _patientService.UpdatePatientAsync(patientUpdateModel);

                if (updateResult.Success)
                {
                    // دریافت بیمار به‌روزرسانی شده
                    var updatedPatient = await _patientService.GetPatientByNationalCodeAsync(model.NationalCode);
                    
                    var patientViewModel = new PatientAccordionViewModel
                    {
                        NationalCode = updatedPatient.NationalCode,
                        FirstName = updatedPatient.FirstName,
                        LastName = updatedPatient.LastName,
                        BirthDate = updatedPatient.BirthDate,
                        Gender = updatedPatient.Gender.ToString(),
                        PhoneNumber = updatedPatient.PhoneNumber,
                        Address = updatedPatient.Address,
                        PatientId = updatedPatient.PatientId,
                        IsPatientFound = true,
                        StatusMessage = "اطلاعات بیمار با موفقیت به‌روزرسانی شد",
                        StatusCssClass = "text-success"
                    };

                    _logger.Information($"اطلاعات بیمار با موفقیت به‌روزرسانی شد: {updatedPatient.FirstName} {updatedPatient.LastName}");

                    return ServiceResult<PatientAccordionViewModel>.Successful(
                        patientViewModel,
                        "اطلاعات بیمار با موفقیت به‌روزرسانی شد"
                    );
                }
                else
                {
                    _logger.Error($"خطا در به‌روزرسانی بیمار: {updateResult.Message}");
                    
                    return ServiceResult<PatientAccordionViewModel>.Failed(
                        updateResult.Message ?? "خطا در به‌روزرسانی اطلاعات بیمار"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در به‌روزرسانی اطلاعات بیمار");
                
                return ServiceResult<PatientAccordionViewModel>.Failed(
                    "خطا در به‌روزرسانی اطلاعات بیمار"
                );
            }
        }

        /// <summary>
        /// جستجوی پیشرفته بیماران
        /// </summary>
        public async Task<ServiceResult<List<PatientSearchResultViewModel>>> SearchPatientsAsync(SearchParameterViewModel searchParams)
        {
            try
            {
                _logger.Information($"جستجوی پیشرفته بیماران");

                // جستجوی بیماران با استفاده از جستجوی ساده
                var searchTerm = $"{searchParams.NationalCode} {searchParams.FirstName} {searchParams.LastName} {searchParams.PhoneNumber}".Trim();
                var searchResult = await _patientService.SearchPatientsAsync(searchTerm, 1, 100);

                if (searchResult.Success)
                {
                    var patients = searchResult.Data.Items.Select(p => new PatientSearchResultViewModel
                    {
                        Id = p.PatientId,
                        NationalCode = p.NationalCode,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        PhoneNumber = p.PhoneNumber,
                        BirthDateShamsi = p.BirthDate?.ToPersianDate() ?? ""
                    }).ToList();

                    return ServiceResult<List<PatientSearchResultViewModel>>.Successful(
                        patients,
                        $"{patients.Count} بیمار یافت شد"
                    );
                }
                else
                {
                    return ServiceResult<List<PatientSearchResultViewModel>>.Failed(
                        searchResult.Message ?? "خطا در جستجوی بیماران"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در جستجوی پیشرفته بیماران");
                
                return ServiceResult<List<PatientSearchResultViewModel>>.Failed(
                    "خطا در جستجوی بیماران"
                );
            }
        }

        /// <summary>
        /// اعتبارسنجی اطلاعات بیمار
        /// </summary>
        public ServiceResult<bool> ValidatePatientInfo(PatientAccordionViewModel model)
        {
            try
            {
                var errors = new List<string>();

                // اعتبارسنجی کد ملی
                if (string.IsNullOrWhiteSpace(model.NationalCode))
                {
                    errors.Add("کد ملی الزامی است");
                }
                else if (!ValidateNationalCode(model.NationalCode))
                {
                    errors.Add("کد ملی نامعتبر است");
                }

                // اعتبارسنجی نام
                if (string.IsNullOrWhiteSpace(model.FirstName))
                {
                    errors.Add("نام الزامی است");
                }
                else if (model.FirstName.Length < ReceptionFormConstants.Validation.MinNameLength)
                {
                    errors.Add($"نام باید حداقل {ReceptionFormConstants.Validation.MinNameLength} کاراکتر باشد");
                }
                else if (model.FirstName.Length > ReceptionFormConstants.Validation.MaxNameLength)
                {
                    errors.Add($"نام باید حداکثر {ReceptionFormConstants.Validation.MaxNameLength} کاراکتر باشد");
                }

                // اعتبارسنجی نام خانوادگی
                if (string.IsNullOrWhiteSpace(model.LastName))
                {
                    errors.Add("نام خانوادگی الزامی است");
                }
                else if (model.LastName.Length < ReceptionFormConstants.Validation.MinNameLength)
                {
                    errors.Add($"نام خانوادگی باید حداقل {ReceptionFormConstants.Validation.MinNameLength} کاراکتر باشد");
                }
                else if (model.LastName.Length > ReceptionFormConstants.Validation.MaxNameLength)
                {
                    errors.Add($"نام خانوادگی باید حداکثر {ReceptionFormConstants.Validation.MaxNameLength} کاراکتر باشد");
                }

                // اعتبارسنجی شماره تلفن
                if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(model.PhoneNumber, ReceptionFormConstants.Validation.PhoneNumberRegex))
                    {
                        errors.Add("شماره تلفن نامعتبر است");
                    }
                }

                // اعتبارسنجی تاریخ تولد
                if (model.BirthDate.HasValue)
                {
                    var age = DateTime.Now.Year - model.BirthDate.Value.Year;
                    if (age < ReceptionFormConstants.Validation.MinAge || age > ReceptionFormConstants.Validation.MaxAge)
                    {
                        errors.Add($"سن باید بین {ReceptionFormConstants.Validation.MinAge} تا {ReceptionFormConstants.Validation.MaxAge} سال باشد");
                    }
                }

                return ServiceResult<bool>.Successful(
                    errors.Count == 0,
                    errors.Count == 0 ? "اطلاعات بیمار معتبر است" : "اطلاعات بیمار نامعتبر است"
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در اعتبارسنجی اطلاعات بیمار");
                
                return ServiceResult<bool>.Failed(
                    "خطا در اعتبارسنجی اطلاعات بیمار"
                );
            }
        }

        /// <summary>
        /// اعتبارسنجی کد ملی ایرانی
        /// </summary>
        private bool ValidateNationalCode(string nationalCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nationalCode) || nationalCode.Length != 10)
                    return false;

                if (!nationalCode.All(char.IsDigit))
                    return false;

                // الگوریتم اعتبارسنجی کد ملی ایرانی
                var digits = nationalCode.Select(c => int.Parse(c.ToString())).ToArray();
                var checkDigit = digits[9];
                
                var sum = 0;
                for (int i = 0; i < 9; i++)
                {
                    sum += digits[i] * (10 - i);
                }
                
                var remainder = sum % 11;
                var calculatedCheckDigit = remainder < 2 ? remainder : 11 - remainder;
                
                return checkDigit == calculatedCheckDigit;
            }
            catch
            {
                return false;
            }
        }
    }
}
