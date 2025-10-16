using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Constants;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    [RoutePrefix("Reception/Patient")]
    public class ReceptionPatientController : BaseController
    {
        private readonly IReceptionPatientService _receptionPatientService;
        private readonly IPatientService _patientService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionPatientController(
            IReceptionPatientService receptionPatientService,
            IPatientService patientService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(logger)
        {
            _receptionPatientService = receptionPatientService ?? throw new ArgumentNullException(nameof(receptionPatientService));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger.ForContext<ReceptionPatientController>();
        }

        /// <summary>
        /// جستجوی بیمار بر اساس کد ملی
        /// </summary>
        [HttpGet]
        [Route("SearchByNationalCode")]
        public async Task<JsonResult> SearchByNationalCode(string nationalCode)
        {
            try
            {
                _logger.Information($"جستجوی بیمار با کد ملی: {nationalCode}");

                // اعتبارسنجی کد ملی
                if (string.IsNullOrWhiteSpace(nationalCode))
                {
                    return Json(ServiceResult<object>.Failed(ReceptionFormConstants.Messages.NationalCodeInvalid), JsonRequestBehavior.AllowGet);
                }

                if (nationalCode.Length != ReceptionFormConstants.Validation.NationalCodeLength)
                {
                    return Json(ServiceResult<object>.Failed(ReceptionFormConstants.Messages.NationalCodeInvalid), JsonRequestBehavior.AllowGet);
                }

                // اعتبارسنجی الگوریتم کد ملی ایرانی
                if (!ValidateNationalCode(nationalCode))
                {
                    return Json(ServiceResult<object>.Failed(ReceptionFormConstants.Messages.NationalCodeInvalid), JsonRequestBehavior.AllowGet);
                }

                       // جستجوی بیمار در دیتابیس
                       var patient = await _patientService.GetPatientByNationalCodeAsync(nationalCode);

                       if (patient != null)
                       {
                    var patientViewModel = new PatientAccordionViewModel
                    {
                        PatientId = patient.PatientId,
                        NationalCode = patient.NationalCode,
                        FirstName = patient.FirstName,
                        LastName = patient.LastName,
                        BirthDate = patient.BirthDate,
                        Gender = patient.Gender.ToString(),
                        PhoneNumber = patient.PhoneNumber,
                        Address = patient.Address,
                        IsPatientFound = true,
                        StatusMessage = string.Format(ReceptionFormConstants.Messages.PatientFound, nationalCode),
                        StatusCssClass = "text-success"
                    };

                    _logger.Information($"بیمار یافت شد: {patient.FirstName} {patient.LastName}");

                    return Json(ServiceResult<PatientAccordionViewModel>.Successful(patientViewModel, string.Format(ReceptionFormConstants.Messages.PatientFound, nationalCode)), JsonRequestBehavior.AllowGet);
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

                    return Json(ServiceResult<PatientAccordionViewModel>.Successful(newPatientViewModel, string.Format(ReceptionFormConstants.Messages.PatientNotFound, nationalCode)), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در جستجوی بیمار با کد ملی {nationalCode}");
                
                return Json(ServiceResult<object>.Failed("خطا در جستجوی بیمار. لطفاً دوباره تلاش کنید."), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ذخیره اطلاعات بیمار جدید
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> SavePatient(PatientAccordionViewModel model)
        {
            try
            {
                _logger.Information($"ذخیره اطلاعات بیمار جدید: {model.NationalCode}");

                // اعتبارسنجی مدل
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                           return Json(ServiceResult<object>.Failed("اطلاعات وارد شده نامعتبر است"), JsonRequestBehavior.AllowGet);
                }

                // اعتبارسنجی کد ملی
                if (!ValidateNationalCode(model.NationalCode))
                {
                    return Json(ServiceResult<object>.Failed(ReceptionFormConstants.Messages.NationalCodeInvalid), JsonRequestBehavior.AllowGet);
                }

                       // بررسی تکراری نبودن کد ملی
                       var existingPatient = await _patientService.GetPatientByNationalCodeAsync(model.NationalCode);
                       if (existingPatient != null)
                       {
                           return Json(ServiceResult<object>.Failed("بیماری با این کد ملی قبلاً ثبت شده است"), JsonRequestBehavior.AllowGet);
                       }

                // ایجاد مدل برای ایجاد بیمار
                var patientCreateModel = new PatientCreateEditViewModel
                {
                    NationalCode = model.NationalCode,
                    FirstName = model.FirstName?.Trim(),
                    LastName = model.LastName?.Trim(),
                    BirthDate = model.BirthDate,
                    Gender = model.Gender,
                    PhoneNumber = model.PhoneNumber?.Trim(),
                    Address = model.Address?.Trim()
                };

                // ذخیره بیمار
                var result = await _patientService.CreatePatientAsync(patientCreateModel);

                if (!result.Success)
                {
                    return Json(ServiceResult<object>.Failed(result.Message), JsonRequestBehavior.AllowGet);
                }

                _logger.Information($"بیمار جدید با موفقیت ذخیره شد: {model.NationalCode}");

                return Json(ServiceResult<object>.Successful(null, ReceptionFormConstants.Messages.PatientSavedSuccess), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در ذخیره اطلاعات بیمار: {model.NationalCode}");
                
                return Json(ServiceResult<object>.Failed("خطا در ذخیره اطلاعات بیمار. لطفاً دوباره تلاش کنید."), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات بیمار موجود
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> UpdatePatient(PatientAccordionViewModel model)
        {
            try
            {
                _logger.Information($"به‌روزرسانی اطلاعات بیمار: {model.NationalCode}");

                // اعتبارسنجی مدل
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                           return Json(ServiceResult<object>.Failed("اطلاعات وارد شده نامعتبر است"), JsonRequestBehavior.AllowGet);
                }

                // اعتبارسنجی کد ملی
                if (!ValidateNationalCode(model.NationalCode))
                {
                    return Json(ServiceResult<object>.Failed(ReceptionFormConstants.Messages.NationalCodeInvalid), JsonRequestBehavior.AllowGet);
                }

                // بررسی وجود بیمار
                var existingPatient = await _patientService.GetPatientByNationalCodeAsync(model.NationalCode);
                if (existingPatient == null)
                {
                    return Json(ServiceResult<object>.Failed("بیمار مورد نظر یافت نشد"), JsonRequestBehavior.AllowGet);
                }

                // ایجاد مدل برای به‌روزرسانی بیمار
                var patientUpdateModel = new PatientCreateEditViewModel
                {
                    PatientId = model.PatientId ?? 0,
                    NationalCode = model.NationalCode,
                    FirstName = model.FirstName?.Trim(),
                    LastName = model.LastName?.Trim(),
                    BirthDate = model.BirthDate,
                    Gender = model.Gender,
                    PhoneNumber = model.PhoneNumber?.Trim(),
                    Address = model.Address?.Trim()
                };

                // به‌روزرسانی بیمار
                var result = await _patientService.UpdatePatientAsync(patientUpdateModel);

                if (!result.Success)
                {
                    return Json(ServiceResult<object>.Failed(result.Message), JsonRequestBehavior.AllowGet);
                }

                _logger.Information($"اطلاعات بیمار با موفقیت به‌روزرسانی شد: {model.NationalCode}");

                return Json(ServiceResult<object>.Successful(null, "اطلاعات بیمار با موفقیت به‌روزرسانی شد"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطا در به‌روزرسانی اطلاعات بیمار: {model.NationalCode}");
                
                return Json(ServiceResult<object>.Failed("خطا در به‌روزرسانی اطلاعات بیمار. لطفاً دوباره تلاش کنید."), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// جستجوی پیشرفته بیماران
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> AdvancedSearch(SearchParameterViewModel searchParams)
        {
            try
            {
                _logger.Information("جستجوی پیشرفته بیماران");

                // اعتبارسنجی پارامترهای جستجو
                if (string.IsNullOrWhiteSpace(searchParams.NationalCode) &&
                    string.IsNullOrWhiteSpace(searchParams.FirstName) &&
                    string.IsNullOrWhiteSpace(searchParams.LastName) &&
                    string.IsNullOrWhiteSpace(searchParams.PhoneNumber))
                {
                    return Json(ServiceResult<object>.Failed("حداقل یک پارامتر جستجو باید وارد شود"), JsonRequestBehavior.AllowGet);
                }

                // جستجوی بیماران
                var result = await _receptionPatientService.SearchPatientsAsync(searchParams);

                if (!result.Success)
                {
                    return Json(ServiceResult<object>.Failed(result.Message), JsonRequestBehavior.AllowGet);
                }

                _logger.Information($"جستجوی پیشرفته انجام شد - تعداد نتایج: {result.Data.Count}");

                return Json(ServiceResult<List<PatientSearchResultViewModel>>.Successful(result.Data, "جستجو با موفقیت انجام شد"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پیشرفته بیماران");
                
                return Json(ServiceResult<object>.Failed("خطا در جستجوی پیشرفته. لطفاً دوباره تلاش کنید."), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// اعتبارسنجی کد ملی ایرانی
        /// </summary>
        private bool ValidateNationalCode(string nationalCode)
        {
            if (string.IsNullOrWhiteSpace(nationalCode) || nationalCode.Length != 10)
                return false;

            if (!nationalCode.All(char.IsDigit))
                return false;

            // الگوریتم اعتبارسنجی کد ملی ایرانی
            var sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += int.Parse(nationalCode[i].ToString()) * (10 - i);
            }

            var remainder = sum % 11;
            var checkDigit = remainder < 2 ? remainder : 11 - remainder;

            return checkDigit == int.Parse(nationalCode[9].ToString());
        }
    }
}