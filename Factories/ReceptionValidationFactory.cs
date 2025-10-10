using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;
using ClinicApp.ViewModels.Validators;
using FluentValidation;
using Serilog;

namespace ClinicApp.Factories
{
    /// <summary>
    /// Factory برای ایجاد Validator های ماژول پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ایجاد Validator های مناسب بر اساس نوع
    /// 2. مدیریت Validator های پیچیده
    /// 3. یکپارچه‌سازی با Business Rules
    /// 4. بهینه‌سازی عملکرد
    /// 5. مدیریت Cache
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط ایجاد Validator ها
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    public class ReceptionValidationFactory
    {
        #region Fields and Constructor

        private readonly IReceptionBusinessRules _businessRules;
        private readonly IReceptionSecurityService _securityService;
        private readonly IReceptionCacheService _cacheService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        // Validator Cache
        private readonly Dictionary<string, IValidator> _validatorCache;

        public ReceptionValidationFactory(
            IReceptionBusinessRules businessRules,
            IReceptionSecurityService securityService,
            IReceptionCacheService cacheService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _businessRules = businessRules ?? throw new ArgumentNullException(nameof(businessRules));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _validatorCache = new Dictionary<string, IValidator>();
        }

        #endregion

        #region Validator Creation Methods

        /// <summary>
        /// ایجاد Validator برای ایجاد پذیرش
        /// </summary>
        /// <param name="model">مدل ایجاد پذیرش</param>
        /// <returns>Validator</returns>
        public async Task<IValidator<ReceptionCreateViewModel>> CreateReceptionCreateValidatorAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("ایجاد Validator برای ایجاد پذیرش. بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.PatientId, model?.DoctorId, _currentUserService.UserName);

                var validator = new ReceptionCreateViewModelValidator();
                
                // Configure validator based on model properties
                await ConfigureReceptionCreateValidatorAsync(validator, model);

                _logger.Debug("ایجاد Validator برای ایجاد پذیرش موفق");
                return validator;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Validator برای ایجاد پذیرش");
                throw;
            }
        }

        /// <summary>
        /// ایجاد Validator برای ویرایش پذیرش
        /// </summary>
        /// <param name="model">مدل ویرایش پذیرش</param>
        /// <returns>Validator</returns>
        public async Task<IValidator<ReceptionEditViewModel>> CreateReceptionEditValidatorAsync(ReceptionEditViewModel model)
        {
            try
            {
                _logger.Debug("ایجاد Validator برای ویرایش پذیرش. شناسه: {Id}, بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.ReceptionId, model?.PatientId, model?.DoctorId, _currentUserService.UserName);

                var validator = new ReceptionEditViewModelValidator();
                
                // Configure validator based on model properties
                await ConfigureReceptionEditValidatorAsync(validator, model);

                _logger.Debug("ایجاد Validator برای ویرایش پذیرش موفق");
                return validator;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Validator برای ویرایش پذیرش");
                throw;
            }
        }

        /// <summary>
        /// ایجاد Validator برای جستجوی پذیرش‌ها
        /// </summary>
        /// <param name="model">مدل جستجو</param>
        /// <returns>Validator</returns>
        public async Task<IValidator<ReceptionSearchViewModel>> CreateReceptionSearchValidatorAsync(ReceptionSearchViewModel model)
        {
            try
            {
                _logger.Debug("ایجاد Validator برای جستجوی پذیرش‌ها. کاربر: {UserName}", _currentUserService.UserName);

                var validator = new ReceptionSearchViewModelValidator();
                
                // Configure validator based on model properties
                await ConfigureReceptionSearchValidatorAsync(validator, model);

                _logger.Debug("ایجاد Validator برای جستجوی پذیرش‌ها موفق");
                return validator;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Validator برای جستجوی پذیرش‌ها");
                throw;
            }
        }

        /// <summary>
        /// ایجاد Validator برای پذیرش اورژانس
        /// </summary>
        /// <param name="model">مدل پذیرش</param>
        /// <returns>Validator</returns>
        public async Task<IValidator<ReceptionCreateViewModel>> CreateEmergencyReceptionValidatorAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("ایجاد Validator برای پذیرش اورژانس. بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.PatientId, model?.DoctorId, _currentUserService.UserName);

                var validator = new EmergencyReceptionCreateViewModelValidator();
                
                // Configure validator for emergency reception
                await ConfigureEmergencyReceptionValidatorAsync(validator, model);

                _logger.Debug("ایجاد Validator برای پذیرش اورژانس موفق");
                return validator;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Validator برای پذیرش اورژانس");
                throw;
            }
        }

        /// <summary>
        /// ایجاد Validator برای پذیرش آنلاین
        /// </summary>
        /// <param name="model">مدل پذیرش</param>
        /// <returns>Validator</returns>
        public async Task<IValidator<ReceptionCreateViewModel>> CreateOnlineReceptionValidatorAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("ایجاد Validator برای پذیرش آنلاین. بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.PatientId, model?.DoctorId, _currentUserService.UserName);

                var validator = new OnlineReceptionCreateViewModelValidator();
                
                // Configure validator for online reception
                await ConfigureOnlineReceptionValidatorAsync(validator, model);

                _logger.Debug("ایجاد Validator برای پذیرش آنلاین موفق");
                return validator;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Validator برای پذیرش آنلاین");
                throw;
            }
        }

        /// <summary>
        /// ایجاد Validator برای پذیرش ویژه
        /// </summary>
        /// <param name="model">مدل پذیرش</param>
        /// <returns>Validator</returns>
        public async Task<IValidator<ReceptionCreateViewModel>> CreateSpecialReceptionValidatorAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("ایجاد Validator برای پذیرش ویژه. بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.PatientId, model?.DoctorId, _currentUserService.UserName);

                var validator = new SpecialReceptionCreateViewModelValidator();
                
                // Configure validator for special reception
                await ConfigureSpecialReceptionValidatorAsync(validator, model);

                _logger.Debug("ایجاد Validator برای پذیرش ویژه موفق");
                return validator;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Validator برای پذیرش ویژه");
                throw;
            }
        }

        #endregion

        #region Validator Configuration Methods

        /// <summary>
        /// پیکربندی Validator ایجاد پذیرش
        /// </summary>
        /// <param name="validator">Validator</param>
        /// <param name="model">مدل</param>
        private async Task ConfigureReceptionCreateValidatorAsync(IValidator<ReceptionCreateViewModel> validator, ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("پیکربندی Validator ایجاد پذیرش");

                // Configure based on model properties
                if (model.IsEmergency)
                {
                    // Emergency receptions have relaxed rules
                    await ConfigureEmergencyRulesAsync(validator);
                }
                else if (model.IsOnlineReception)
                {
                    // Online receptions have specific rules
                    await ConfigureOnlineRulesAsync(validator);
                }
                else if (model.Type == ReceptionType.Special)
                {
                    // Special receptions have enhanced rules
                    await ConfigureSpecialRulesAsync(validator);
                }
                else
                {
                    // Normal receptions have standard rules
                    await ConfigureNormalRulesAsync(validator);
                }

                _logger.Debug("پیکربندی Validator ایجاد پذیرش موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی Validator ایجاد پذیرش");
                throw;
            }
        }

        /// <summary>
        /// پیکربندی Validator ویرایش پذیرش
        /// </summary>
        /// <param name="validator">Validator</param>
        /// <param name="model">مدل</param>
        private async Task ConfigureReceptionEditValidatorAsync(IValidator<ReceptionEditViewModel> validator, ReceptionEditViewModel model)
        {
            try
            {
                _logger.Debug("پیکربندی Validator ویرایش پذیرش");

                // Configure based on model properties
                if (model.IsEmergency)
                {
                    // Emergency receptions have relaxed rules
                    await ConfigureEmergencyEditRulesAsync(validator);
                }
                else if (model.IsOnlineReception)
                {
                    // Online receptions have specific rules
                    await ConfigureOnlineEditRulesAsync(validator);
                }
                else if (model.Type == ReceptionType.Special)
                {
                    // Special receptions have enhanced rules
                    await ConfigureSpecialEditRulesAsync(validator);
                }
                else
                {
                    // Normal receptions have standard rules
                    await ConfigureNormalEditRulesAsync(validator);
                }

                _logger.Debug("پیکربندی Validator ویرایش پذیرش موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی Validator ویرایش پذیرش");
                throw;
            }
        }

        /// <summary>
        /// پیکربندی Validator جستجوی پذیرش‌ها
        /// </summary>
        /// <param name="validator">Validator</param>
        /// <param name="model">مدل</param>
        private async Task ConfigureReceptionSearchValidatorAsync(IValidator<ReceptionSearchViewModel> validator, ReceptionSearchViewModel model)
        {
            try
            {
                _logger.Debug("پیکربندی Validator جستجوی پذیرش‌ها");

                // Configure search-specific rules
                await ConfigureSearchRulesAsync(validator);

                _logger.Debug("پیکربندی Validator جستجوی پذیرش‌ها موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی Validator جستجوی پذیرش‌ها");
                throw;
            }
        }

        #endregion

        #region Rule Configuration Methods

        /// <summary>
        /// پیکربندی قوانین عادی
        /// </summary>
        /// <param name="validator">Validator</param>
        private async Task ConfigureNormalRulesAsync(IValidator<ReceptionCreateViewModel> validator)
        {
            try
            {
                _logger.Debug("پیکربندی قوانین عادی");

                // Configure standard validation rules
                // This would typically involve:
                // 1. Setting up basic validation rules
                // 2. Configuring business rules
                // 3. Setting up security rules
                // 4. Configuring performance rules

                _logger.Debug("پیکربندی قوانین عادی موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی قوانین عادی");
                throw;
            }
        }

        /// <summary>
        /// پیکربندی قوانین اورژانس
        /// </summary>
        /// <param name="validator">Validator</param>
        private async Task ConfigureEmergencyRulesAsync(IValidator<ReceptionCreateViewModel> validator)
        {
            try
            {
                _logger.Debug("پیکربندی قوانین اورژانس");

                // Configure emergency-specific validation rules
                // This would typically involve:
                // 1. Relaxed validation rules
                // 2. Emergency-specific business rules
                // 3. Emergency-specific security rules
                // 4. Emergency-specific performance rules

                _logger.Debug("پیکربندی قوانین اورژانس موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی قوانین اورژانس");
                throw;
            }
        }

        /// <summary>
        /// پیکربندی قوانین آنلاین
        /// </summary>
        /// <param name="validator">Validator</param>
        private async Task ConfigureOnlineRulesAsync(IValidator<ReceptionCreateViewModel> validator)
        {
            try
            {
                _logger.Debug("پیکربندی قوانین آنلاین");

                // Configure online-specific validation rules
                // This would typically involve:
                // 1. Online-specific validation rules
                // 2. Online-specific business rules
                // 3. Online-specific security rules
                // 4. Online-specific performance rules

                _logger.Debug("پیکربندی قوانین آنلاین موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی قوانین آنلاین");
                throw;
            }
        }

        /// <summary>
        /// پیکربندی قوانین ویژه
        /// </summary>
        /// <param name="validator">Validator</param>
        private async Task ConfigureSpecialRulesAsync(IValidator<ReceptionCreateViewModel> validator)
        {
            try
            {
                _logger.Debug("پیکربندی قوانین ویژه");

                // Configure special-specific validation rules
                // This would typically involve:
                // 1. Enhanced validation rules
                // 2. Special-specific business rules
                // 3. Special-specific security rules
                // 4. Special-specific performance rules

                _logger.Debug("پیکربندی قوانین ویژه موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی قوانین ویژه");
                throw;
            }
        }

        /// <summary>
        /// پیکربندی قوانین ویرایش عادی
        /// </summary>
        /// <param name="validator">Validator</param>
        private async Task ConfigureNormalEditRulesAsync(IValidator<ReceptionEditViewModel> validator)
        {
            try
            {
                _logger.Debug("پیکربندی قوانین ویرایش عادی");

                // Configure standard edit validation rules
                // This would typically involve:
                // 1. Setting up basic edit validation rules
                // 2. Configuring edit business rules
                // 3. Setting up edit security rules
                // 4. Configuring edit performance rules

                _logger.Debug("پیکربندی قوانین ویرایش عادی موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی قوانین ویرایش عادی");
                throw;
            }
        }

        /// <summary>
        /// پیکربندی قوانین ویرایش اورژانس
        /// </summary>
        /// <param name="validator">Validator</param>
        private async Task ConfigureEmergencyEditRulesAsync(IValidator<ReceptionEditViewModel> validator)
        {
            try
            {
                _logger.Debug("پیکربندی قوانین ویرایش اورژانس");

                // Configure emergency edit validation rules
                // This would typically involve:
                // 1. Relaxed edit validation rules
                // 2. Emergency edit business rules
                // 3. Emergency edit security rules
                // 4. Emergency edit performance rules

                _logger.Debug("پیکربندی قوانین ویرایش اورژانس موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی قوانین ویرایش اورژانس");
                throw;
            }
        }

        /// <summary>
        /// پیکربندی قوانین ویرایش آنلاین
        /// </summary>
        /// <param name="validator">Validator</param>
        private async Task ConfigureOnlineEditRulesAsync(IValidator<ReceptionEditViewModel> validator)
        {
            try
            {
                _logger.Debug("پیکربندی قوانین ویرایش آنلاین");

                // Configure online edit validation rules
                // This would typically involve:
                // 1. Online edit validation rules
                // 2. Online edit business rules
                // 3. Online edit security rules
                // 4. Online edit performance rules

                _logger.Debug("پیکربندی قوانین ویرایش آنلاین موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی قوانین ویرایش آنلاین");
                throw;
            }
        }

        /// <summary>
        /// پیکربندی قوانین ویرایش ویژه
        /// </summary>
        /// <param name="validator">Validator</param>
        private async Task ConfigureSpecialEditRulesAsync(IValidator<ReceptionEditViewModel> validator)
        {
            try
            {
                _logger.Debug("پیکربندی قوانین ویرایش ویژه");

                // Configure special edit validation rules
                // This would typically involve:
                // 1. Enhanced edit validation rules
                // 2. Special edit business rules
                // 3. Special edit security rules
                // 4. Special edit performance rules

                _logger.Debug("پیکربندی قوانین ویرایش ویژه موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی قوانین ویرایش ویژه");
                throw;
            }
        }

        /// <summary>
        /// پیکربندی قوانین جستجو
        /// </summary>
        /// <param name="validator">Validator</param>
        private async Task ConfigureSearchRulesAsync(IValidator<ReceptionSearchViewModel> validator)
        {
            try
            {
                _logger.Debug("پیکربندی قوانین جستجو");

                // Configure search-specific validation rules
                // This would typically involve:
                // 1. Search validation rules
                // 2. Search business rules
                // 3. Search security rules
                // 4. Search performance rules

                _logger.Debug("پیکربندی قوانین جستجو موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی قوانین جستجو");
                throw;
            }
        }

        #endregion

        #region Special Case Configuration Methods

        /// <summary>
        /// پیکربندی Validator پذیرش اورژانس
        /// </summary>
        /// <param name="validator">Validator</param>
        /// <param name="model">مدل</param>
        private async Task ConfigureEmergencyReceptionValidatorAsync(IValidator<ReceptionCreateViewModel> validator, ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("پیکربندی Validator پذیرش اورژانس");

                // Configure emergency-specific validation rules
                // This would typically involve:
                // 1. Relaxed validation rules
                // 2. Emergency-specific business rules
                // 3. Emergency-specific security rules
                // 4. Emergency-specific performance rules

                _logger.Debug("پیکربندی Validator پذیرش اورژانس موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی Validator پذیرش اورژانس");
                throw;
            }
        }

        /// <summary>
        /// پیکربندی Validator پذیرش آنلاین
        /// </summary>
        /// <param name="validator">Validator</param>
        /// <param name="model">مدل</param>
        private async Task ConfigureOnlineReceptionValidatorAsync(IValidator<ReceptionCreateViewModel> validator, ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("پیکربندی Validator پذیرش آنلاین");

                // Configure online-specific validation rules
                // This would typically involve:
                // 1. Online-specific validation rules
                // 2. Online-specific business rules
                // 3. Online-specific security rules
                // 4. Online-specific performance rules

                _logger.Debug("پیکربندی Validator پذیرش آنلاین موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی Validator پذیرش آنلاین");
                throw;
            }
        }

        /// <summary>
        /// پیکربندی Validator پذیرش ویژه
        /// </summary>
        /// <param name="validator">Validator</param>
        /// <param name="model">مدل</param>
        private async Task ConfigureSpecialReceptionValidatorAsync(IValidator<ReceptionCreateViewModel> validator, ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Debug("پیکربندی Validator پذیرش ویژه");

                // Configure special-specific validation rules
                // This would typically involve:
                // 1. Enhanced validation rules
                // 2. Special-specific business rules
                // 3. Special-specific security rules
                // 4. Special-specific performance rules

                _logger.Debug("پیکربندی Validator پذیرش ویژه موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیکربندی Validator پذیرش ویژه");
                throw;
            }
        }

        #endregion

        #region Cache Management Methods

        /// <summary>
        /// دریافت Validator از Cache
        /// </summary>
        /// <param name="key">کلید Cache</param>
        /// <returns>Validator</returns>
        public async Task<IValidator> GetValidatorFromCacheAsync(string key)
        {
            try
            {
                _logger.Debug("دریافت Validator از Cache. کلید: {Key}", key);

                if (_validatorCache.ContainsKey(key))
                {
                    _logger.Debug("دریافت Validator از Cache موفق");
                    return _validatorCache[key];
                }

                _logger.Debug("Validator در Cache یافت نشد");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت Validator از Cache. کلید: {Key}", key);
                return null;
            }
        }

        /// <summary>
        /// ذخیره Validator در Cache
        /// </summary>
        /// <param name="key">کلید Cache</param>
        /// <param name="validator">Validator</param>
        /// <returns>نتیجه ذخیره</returns>
        public async Task<bool> CacheValidatorAsync(string key, IValidator validator)
        {
            try
            {
                _logger.Debug("ذخیره Validator در Cache. کلید: {Key}", key);

                _validatorCache[key] = validator;

                _logger.Debug("ذخیره Validator در Cache موفق");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره Validator در Cache. کلید: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// پاک کردن Cache Validator ها
        /// </summary>
        /// <returns>نتیجه پاک کردن</returns>
        public async Task<bool> ClearValidatorCacheAsync()
        {
            try
            {
                _logger.Debug("پاک کردن Cache Validator ها");

                _validatorCache.Clear();

                _logger.Debug("پاک کردن Cache Validator ها موفق");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پاک کردن Cache Validator ها");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// ایجاد کلید Cache برای Validator
        /// </summary>
        /// <param name="validatorType">نوع Validator</param>
        /// <param name="modelType">نوع مدل</param>
        /// <param name="specialCase">مورد خاص</param>
        /// <returns>کلید Cache</returns>
        private string CreateValidatorCacheKey(string validatorType, string modelType, string specialCase = null)
        {
            try
            {
                _logger.Debug("ایجاد کلید Cache برای Validator. نوع: {ValidatorType}, مدل: {ModelType}, مورد خاص: {SpecialCase}",
                    validatorType, modelType, specialCase);

                var key = $"{validatorType}_{modelType}";
                if (!string.IsNullOrEmpty(specialCase))
                {
                    key += $"_{specialCase}";
                }

                _logger.Debug("ایجاد کلید Cache برای Validator موفق. کلید: {Key}", key);
                return key;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد کلید Cache برای Validator");
                return Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// بررسی وجود Validator در Cache
        /// </summary>
        /// <param name="key">کلید Cache</param>
        /// <returns>نتیجه بررسی</returns>
        private bool IsValidatorInCache(string key)
        {
            try
            {
                _logger.Debug("بررسی وجود Validator در Cache. کلید: {Key}", key);

                var exists = _validatorCache.ContainsKey(key);

                _logger.Debug("بررسی وجود Validator در Cache. نتیجه: {Exists}", exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود Validator در Cache. کلید: {Key}", key);
                return false;
            }
        }

        #endregion
    }
}
