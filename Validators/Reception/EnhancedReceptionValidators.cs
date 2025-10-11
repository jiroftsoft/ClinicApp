using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Areas.Admin.Controllers;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;
using FluentValidation;
using Serilog;
using ClinicApp.Helpers;

namespace ClinicApp.Validators.Reception
{
    /// <summary>
    /// Validator های پیشرفته برای ماژول پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی جامع ViewModels
    /// 2. قوانین کسب‌وکار پیچیده
    /// 3. اعتبارسنجی امنیتی
    /// 4. اعتبارسنجی عملکرد
    /// 5. اعتبارسنجی یکپارچگی داده‌ها
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط اعتبارسنجی
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    public class EnhancedReceptionValidators
    {
        #region Fields and Constructor

        private readonly IReceptionBusinessRules _businessRules;
        private readonly IReceptionSecurityService _securityService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public EnhancedReceptionValidators(
            IReceptionBusinessRules businessRules,
            IReceptionSecurityService securityService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _businessRules = businessRules ?? throw new ArgumentNullException(nameof(businessRules));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Reception Create Validation

        /// <summary>
        /// اعتبارسنجی پیشرفته ایجاد پذیرش
        /// </summary>
        /// <param name="model">مدل ایجاد پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public async Task<CustomValidationResult> ValidateReceptionCreateAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی پیشرفته ایجاد پذیرش. بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.PatientId, model?.DoctorId, _currentUserService.UserName);

                var errors = new List<string>();
                var warnings = new List<string>();

                // Basic validation
                var basicValidation = await ValidateBasicReceptionCreateAsync(model);
                if (!basicValidation.IsValid)
                {
                    errors.AddRange(basicValidation.Errors);
                }

                // Business rules validation
                var businessRulesValidation = await _businessRules.ValidateReceptionAsync(model);
                if (!businessRulesValidation.IsValid)
                {
                    errors.AddRange(businessRulesValidation.Errors);
                }

                // Security validation
                var securityValidation = await ValidateReceptionCreateSecurityAsync(model);
                if (!securityValidation.IsValid)
                {
                    errors.AddRange(securityValidation.Errors);
                }

                // Performance validation
                var performanceValidation = await ValidateReceptionCreatePerformanceAsync(model);
                if (!performanceValidation.IsValid)
                {
                    warnings.AddRange(performanceValidation.Errors);
                }

                // Data integrity validation
                var integrityValidation = await ValidateReceptionCreateIntegrityAsync(model);
                if (!integrityValidation.IsValid)
                {
                    errors.AddRange(integrityValidation.Errors);
                }

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی پیشرفته ایجاد پذیرش ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                if (warnings.Any())
                {
                    _logger.Information("اعتبارسنجی پیشرفته ایجاد پذیرش با هشدار. هشدارها: {@Warnings}", warnings);
                }

                _logger.Debug("اعتبارسنجی پیشرفته ایجاد پذیرش موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی پیشرفته ایجاد پذیرش");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی پایه ایجاد پذیرش
        /// </summary>
        /// <param name="model">مدل ایجاد پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        private async Task<CustomValidationResult> ValidateBasicReceptionCreateAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی پایه ایجاد پذیرش");

                var errors = new List<string>();

                if (model == null)
                {
                    errors.Add("مدل پذیرش نمی‌تواند null باشد");
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                if (model.PatientId <= 0)
                {
                    errors.Add("شناسه بیمار باید بزرگتر از صفر باشد");
                }

                if (model.DoctorId <= 0)
                {
                    errors.Add("شناسه پزشک باید بزرگتر از صفر باشد");
                }

                if (model.ReceptionDate == default(DateTime))
                {
                    errors.Add("تاریخ پذیرش باید مشخص باشد");
                }

                if (model.SelectedServiceIds == null || !model.SelectedServiceIds.Any())
                {
                    errors.Add("حداقل یک خدمت باید انتخاب شود");
                }

                if (model.SelectedServiceIds != null && model.SelectedServiceIds.Count > 10)
                {
                    errors.Add("تعداد خدمات نمی‌تواند بیش از 10 باشد");
                }

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی پایه ایجاد پذیرش ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                _logger.Debug("اعتبارسنجی پایه ایجاد پذیرش موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی پایه ایجاد پذیرش");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی امنیتی ایجاد پذیرش
        /// </summary>
        /// <param name="model">مدل ایجاد پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        private async Task<CustomValidationResult> ValidateReceptionCreateSecurityAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی امنیتی ایجاد پذیرش");

                var errors = new List<string>();

                // Check user permissions
                var canCreate = await _securityService.CanCreateReceptionAsync(_currentUserService.UserId);
                if (!canCreate)
                {
                    errors.Add("شما مجوز ایجاد پذیرش را ندارید");
                }

                // Validate input security
                if (!string.IsNullOrEmpty(model.Notes))
                {
                    var inputValidation = await _securityService.ValidateReceptionInputAsync(model.Notes);
                    if (!inputValidation.IsValid)
                    {
                        errors.AddRange(inputValidation.Errors);
                    }
                }

                // Validate patient data security
                var patientSecurity = await _securityService.ValidatePatientDataSecurityAsync(model.PatientId, _currentUserService.UserId);
                if (!patientSecurity)
                {
                    errors.Add("شما مجوز دسترسی به اطلاعات این بیمار را ندارید");
                }

                // Validate doctor data security
                var doctorSecurity = await _securityService.ValidateDoctorDataSecurityAsync(model.DoctorId, _currentUserService.UserId);
                if (!doctorSecurity)
                {
                    errors.Add("شما مجوز دسترسی به اطلاعات این پزشک را ندارید");
                }

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی امنیتی ایجاد پذیرش ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                _logger.Debug("اعتبارسنجی امنیتی ایجاد پذیرش موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی امنیتی ایجاد پذیرش");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی عملکرد ایجاد پذیرش
        /// </summary>
        /// <param name="model">مدل ایجاد پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        private async Task<CustomValidationResult> ValidateReceptionCreatePerformanceAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی عملکرد ایجاد پذیرش");

                var warnings = new List<string>();

                // Check if reception date is too far in the future
                if (model.ReceptionDate > DateTime.Now.AddMonths(3))
                {
                    warnings.Add("تاریخ پذیرش بیش از 3 ماه آینده است");
                }

                // Check if reception date is in the past
                if (model.ReceptionDate < DateTime.Now.AddDays(-1))
                {
                    warnings.Add("تاریخ پذیرش در گذشته است");
                }

                // Check if too many services are selected
                if (model.SelectedServiceIds != null && model.SelectedServiceIds.Count > 5)
                {
                    warnings.Add("تعداد خدمات انتخاب شده زیاد است");
                }

                if (warnings.Any())
                {
                    _logger.Information("اعتبارسنجی عملکرد ایجاد پذیرش با هشدار. هشدارها: {@Warnings}", warnings);
                    return CustomValidationResult.Failed(string.Join(", ", warnings), warnings.ToArray());
                }

                _logger.Debug("اعتبارسنجی عملکرد ایجاد پذیرش موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی عملکرد ایجاد پذیرش");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی یکپارچگی ایجاد پذیرش
        /// </summary>
        /// <param name="model">مدل ایجاد پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        private async Task<CustomValidationResult> ValidateReceptionCreateIntegrityAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی یکپارچگی ایجاد پذیرش");

                var errors = new List<string>();

                // Check for duplicate services
                if (model.SelectedServiceIds != null && model.SelectedServiceIds.Count != model.SelectedServiceIds.Distinct().Count())
                {
                    errors.Add("خدمات تکراری انتخاب شده است");
                }

                // Check for invalid service IDs
                if (model.SelectedServiceIds != null)
                {
                    foreach (var serviceId in model.SelectedServiceIds)
                    {
                        if (serviceId <= 0)
                        {
                            errors.Add($"شناسه خدمت {serviceId} نامعتبر است");
                        }
                    }
                }

                // Check for invalid patient ID
                if (model.PatientId <= 0)
                {
                    errors.Add("شناسه بیمار نامعتبر است");
                }

                // Check for invalid doctor ID
                if (model.DoctorId <= 0)
                {
                    errors.Add("شناسه پزشک نامعتبر است");
                }

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی یکپارچگی ایجاد پذیرش ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                _logger.Debug("اعتبارسنجی یکپارچگی ایجاد پذیرش موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی یکپارچگی ایجاد پذیرش");
                throw;
            }
        }

        #endregion

        #region Reception Edit Validation

        /// <summary>
        /// اعتبارسنجی پیشرفته ویرایش پذیرش
        /// </summary>
        /// <param name="model">مدل ویرایش پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public async Task<CustomValidationResult> ValidateReceptionEditAsync(ReceptionEditViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی پیشرفته ویرایش پذیرش. شناسه: {Id}, بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.ReceptionId, model?.PatientId, model?.DoctorId, _currentUserService.UserName);

                var errors = new List<string>();
                var warnings = new List<string>();

                // Basic validation
                var basicValidation = await ValidateBasicReceptionEditAsync(model);
                if (!basicValidation.IsValid)
                {
                    errors.AddRange(basicValidation.Errors);
                }

                // Business rules validation
                var businessRulesValidation = await _businessRules.ValidateReceptionEditAsync(model);
                if (!businessRulesValidation.IsValid)
                {
                    errors.AddRange(businessRulesValidation.Errors);
                }

                // Security validation
                var securityValidation = await ValidateReceptionEditSecurityAsync(model);
                if (!securityValidation.IsValid)
                {
                    errors.AddRange(securityValidation.Errors);
                }

                // Performance validation
                var performanceValidation = await ValidateReceptionEditPerformanceAsync(model);
                if (!performanceValidation.IsValid)
                {
                    warnings.AddRange(performanceValidation.Errors);
                }

                // Data integrity validation
                var integrityValidation = await ValidateReceptionEditIntegrityAsync(model);
                if (!integrityValidation.IsValid)
                {
                    errors.AddRange(integrityValidation.Errors);
                }

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی پیشرفته ویرایش پذیرش ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                if (warnings.Any())
                {
                    _logger.Information("اعتبارسنجی پیشرفته ویرایش پذیرش با هشدار. هشدارها: {@Warnings}", warnings);
                }

                _logger.Debug("اعتبارسنجی پیشرفته ویرایش پذیرش موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی پیشرفته ویرایش پذیرش");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی پایه ویرایش پذیرش
        /// </summary>
        /// <param name="model">مدل ویرایش پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        private async Task<CustomValidationResult> ValidateBasicReceptionEditAsync(ReceptionEditViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی پایه ویرایش پذیرش");

                var errors = new List<string>();

                if (model == null)
                {
                    errors.Add("مدل پذیرش نمی‌تواند null باشد");
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                if (model.ReceptionId <= 0)
                {
                    errors.Add("شناسه پذیرش باید بزرگتر از صفر باشد");
                }

                if (model.PatientId <= 0)
                {
                    errors.Add("شناسه بیمار باید بزرگتر از صفر باشد");
                }

                if (model.DoctorId <= 0)
                {
                    errors.Add("شناسه پزشک باید بزرگتر از صفر باشد");
                }

                if (model.ReceptionDate == default(DateTime))
                {
                    errors.Add("تاریخ پذیرش باید مشخص باشد");
                }

                if (model.SelectedServiceIds == null || !model.SelectedServiceIds.Any())
                {
                    errors.Add("حداقل یک خدمت باید انتخاب شود");
                }

                if (model.SelectedServiceIds != null && model.SelectedServiceIds.Count > 10)
                {
                    errors.Add("تعداد خدمات نمی‌تواند بیش از 10 باشد");
                }

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی پایه ویرایش پذیرش ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                _logger.Debug("اعتبارسنجی پایه ویرایش پذیرش موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی پایه ویرایش پذیرش");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی امنیتی ویرایش پذیرش
        /// </summary>
        /// <param name="model">مدل ویرایش پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        private async Task<CustomValidationResult> ValidateReceptionEditSecurityAsync(ReceptionEditViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی امنیتی ویرایش پذیرش");

                var errors = new List<string>();

                // Check user permissions
                var canEdit = await _securityService.CanEditReceptionAsync(_currentUserService.UserId, model.ReceptionId);
                if (!canEdit)
                {
                    errors.Add("شما مجوز ویرایش این پذیرش را ندارید");
                }

                // Validate input security
                if (!string.IsNullOrEmpty(model.Notes))
                {
                    var inputValidation = await _securityService.ValidateReceptionInputAsync(model.Notes);
                    if (!inputValidation.IsValid)
                    {
                        errors.AddRange(inputValidation.Errors);
                    }
                }

                // Validate patient data security
                var patientSecurity = await _securityService.ValidatePatientDataSecurityAsync(model.PatientId, _currentUserService.UserId);
                if (!patientSecurity)
                {
                    errors.Add("شما مجوز دسترسی به اطلاعات این بیمار را ندارید");
                }

                // Validate doctor data security
                var doctorSecurity = await _securityService.ValidateDoctorDataSecurityAsync(model.DoctorId, _currentUserService.UserId);
                if (!doctorSecurity)
                {
                    errors.Add("شما مجوز دسترسی به اطلاعات این پزشک را ندارید");
                }

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی امنیتی ویرایش پذیرش ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                _logger.Debug("اعتبارسنجی امنیتی ویرایش پذیرش موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی امنیتی ویرایش پذیرش");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی عملکرد ویرایش پذیرش
        /// </summary>
        /// <param name="model">مدل ویرایش پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        private async Task<CustomValidationResult> ValidateReceptionEditPerformanceAsync(ReceptionEditViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی عملکرد ویرایش پذیرش");

                var warnings = new List<string>();

                // Check if reception date is too far in the future
                if (model.ReceptionDate > DateTime.Now.AddMonths(3))
                {
                    warnings.Add("تاریخ پذیرش بیش از 3 ماه آینده است");
                }

                // Check if reception date is in the past
                if (model.ReceptionDate < DateTime.Now.AddDays(-1))
                {
                    warnings.Add("تاریخ پذیرش در گذشته است");
                }

                // Check if too many services are selected
                if (model.SelectedServiceIds != null && model.SelectedServiceIds.Count > 5)
                {
                    warnings.Add("تعداد خدمات انتخاب شده زیاد است");
                }

                if (warnings.Any())
                {
                    _logger.Information("اعتبارسنجی عملکرد ویرایش پذیرش با هشدار. هشدارها: {@Warnings}", warnings);
                    return CustomValidationResult.Failed(string.Join(", ", warnings), warnings.ToArray());
                }

                _logger.Debug("اعتبارسنجی عملکرد ویرایش پذیرش موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی عملکرد ویرایش پذیرش");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی یکپارچگی ویرایش پذیرش
        /// </summary>
        /// <param name="model">مدل ویرایش پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        private async Task<CustomValidationResult> ValidateReceptionEditIntegrityAsync(ReceptionEditViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی یکپارچگی ویرایش پذیرش");

                var errors = new List<string>();

                // Check for duplicate services
                if (model.SelectedServiceIds != null && model.SelectedServiceIds.Count != model.SelectedServiceIds.Distinct().Count())
                {
                    errors.Add("خدمات تکراری انتخاب شده است");
                }

                // Check for invalid service IDs
                if (model.SelectedServiceIds != null)
                {
                    foreach (var serviceId in model.SelectedServiceIds)
                    {
                        if (serviceId <= 0)
                        {
                            errors.Add($"شناسه خدمت {serviceId} نامعتبر است");
                        }
                    }
                }

                // Check for invalid patient ID
                if (model.PatientId <= 0)
                {
                    errors.Add("شناسه بیمار نامعتبر است");
                }

                // Check for invalid doctor ID
                if (model.DoctorId <= 0)
                {
                    errors.Add("شناسه پزشک نامعتبر است");
                }

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی یکپارچگی ویرایش پذیرش ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                _logger.Debug("اعتبارسنجی یکپارچگی ویرایش پذیرش موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی یکپارچگی ویرایش پذیرش");
                throw;
            }
        }

        #endregion

        #region Search Validation

        /// <summary>
        /// اعتبارسنجی جستجوی پذیرش‌ها
        /// </summary>
        /// <param name="model">مدل جستجو</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public async Task<CustomValidationResult> ValidateReceptionSearchAsync(ReceptionSearchViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی جستجوی پذیرش‌ها. کاربر: {UserName}", _currentUserService.UserName);

                var errors = new List<string>();

                if (model == null)
                {
                    errors.Add("مدل جستجو نمی‌تواند null باشد");
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                // Validate date range
                if (model.StartDate > model.EndDate)
                {
                    errors.Add("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد");
                }

                // Validate date range span
                if (model.EndDate - model.StartDate > TimeSpan.FromDays(365))
                {
                    errors.Add("بازه زمانی جستجو نمی‌تواند بیش از یک سال باشد");
                }

                // Validate search term
                if (!string.IsNullOrEmpty(model.SearchTerm))
                {
                    var inputValidation = await _securityService.ValidateReceptionInputAsync(model.SearchTerm);
                    if (!inputValidation.IsValid)
                    {
                        errors.AddRange(inputValidation.Errors);
                    }
                }

                // Validate pagination
                if (model.PageNumber < 0)
                {
                    errors.Add("شماره صفحه نمی‌تواند منفی باشد");
                }

                if (model.PageSize <= 0 || model.PageSize > 100)
                {
                    errors.Add("اندازه صفحه باید بین 1 تا 100 باشد");
                }

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی جستجوی پذیرش‌ها ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                _logger.Debug("اعتبارسنجی جستجوی پذیرش‌ها موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی جستجوی پذیرش‌ها");
                throw;
            }
        }

        #endregion

        #region Batch Validation

        /// <summary>
        /// اعتبارسنجی دسته‌ای پذیرش‌ها
        /// </summary>
        /// <param name="models">مدل‌های پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public async Task<CustomValidationResult> ValidateBatchReceptionsAsync(List<ReceptionCreateViewModel> models)
        {
            try
            {
                _logger.Debug("اعتبارسنجی دسته‌ای پذیرش‌ها. تعداد: {Count}, کاربر: {UserName}", models?.Count ?? 0, _currentUserService.UserName);

                var errors = new List<string>();

                if (models == null || !models.Any())
                {
                    errors.Add("لیست پذیرش‌ها نمی‌تواند خالی باشد");
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                if (models.Count > 50)
                {
                    errors.Add("تعداد پذیرش‌ها نمی‌تواند بیش از 50 باشد");
                }

                // Validate each reception
                for (int i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    var validation = await ValidateReceptionCreateAsync(model);
                    if (!validation.IsValid)
                    {
                        errors.Add($"پذیرش {i + 1}: {validation.Message}");
                    }
                }

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی دسته‌ای پذیرش‌ها ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                _logger.Debug("اعتبارسنجی دسته‌ای پذیرش‌ها موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی دسته‌ای پذیرش‌ها");
                throw;
            }
        }

        #endregion

        #region Special Cases Validation

        /// <summary>
        /// اعتبارسنجی پذیرش اورژانس
        /// </summary>
        /// <param name="model">مدل پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public async Task<CustomValidationResult> ValidateEmergencyReceptionAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی پذیرش اورژانس. بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.PatientId, model?.DoctorId, _currentUserService.UserName);

                var errors = new List<string>();

                // Emergency receptions have relaxed rules
                // Only validate essential requirements

                // Validate Patient (essential)
                var patientValidation = await _businessRules.ValidatePatientAsync(model.PatientId);
                if (!patientValidation.IsValid)
                {
                    errors.AddRange(patientValidation.Errors);
                }

                // Validate Doctor (essential)
                var doctorValidation = await _businessRules.ValidateDoctorAsync(model.DoctorId, model.ReceptionDate);
                if (!doctorValidation.IsValid)
                {
                    errors.AddRange(doctorValidation.Errors);
                }

                // Skip time conflict validation for emergency
                // Skip working hours validation for emergency
                // Skip capacity validation for emergency

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی پذیرش اورژانس ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                _logger.Debug("اعتبارسنجی پذیرش اورژانس موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی پذیرش اورژانس");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی پذیرش آنلاین
        /// </summary>
        /// <param name="model">مدل پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public async Task<CustomValidationResult> ValidateOnlineReceptionAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی پذیرش آنلاین. بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.PatientId, model?.DoctorId, _currentUserService.UserName);

                var errors = new List<string>();

                // Online receptions have specific rules
                // Validate all standard requirements
                var standardValidation = await ValidateReceptionCreateAsync(model);
                if (!standardValidation.IsValid)
                {
                    errors.AddRange(standardValidation.Errors);
                }

                // Additional online-specific validations
                // TODO: Implement online-specific rules
                // This could include:
                // 1. Internet connection validation
                // 2. Device compatibility validation
                // 3. Online payment validation
                // 4. Digital signature validation

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی پذیرش آنلاین ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                _logger.Debug("اعتبارسنجی پذیرش آنلاین موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی پذیرش آنلاین");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی پذیرش ویژه
        /// </summary>
        /// <param name="model">مدل پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public async Task<CustomValidationResult> ValidateSpecialReceptionAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("اعتبارسنجی پذیرش ویژه. بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.PatientId, model?.DoctorId, _currentUserService.UserName);

                var errors = new List<string>();

                // Special receptions have enhanced rules
                // Validate all standard requirements
                var standardValidation = await ValidateReceptionCreateAsync(model);
                if (!standardValidation.IsValid)
                {
                    errors.AddRange(standardValidation.Errors);
                }

                // Additional special-specific validations
                // TODO: Implement special-specific rules
                // This could include:
                // 1. VIP patient validation
                // 2. Special service validation
                // 3. Enhanced security validation
                // 4. Premium insurance validation

                if (errors.Any())
                {
                    _logger.Warning("اعتبارسنجی پذیرش ویژه ناموفق. خطاها: {@Errors}", errors);
                    return CustomValidationResult.Failed(string.Join(", ", errors), errors.ToArray());
                }

                _logger.Debug("اعتبارسنجی پذیرش ویژه موفق");
                return CustomValidationResult.Success("اعتبارسنجی موفق بود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی پذیرش ویژه");
                throw;
            }
        }

        #endregion
    }
}
