using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// Interface برای مدیریت قوانین کسب‌وکار پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل پذیرش
    /// 2. بررسی قوانین کسب‌وکار
    /// 3. اعتبارسنجی بیمار، پزشک، خدمات
    /// 4. بررسی تداخل زمانی
    /// 5. اعتبارسنجی بیمه
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط قوانین کسب‌وکار
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    public interface IReceptionBusinessRules
    {
        #region Core Validation Methods

        /// <summary>
        /// اعتبارسنجی کامل پذیرش
        /// </summary>
        /// <param name="model">مدل ایجاد پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateReceptionAsync(ReceptionCreateViewModel model);

        /// <summary>
        /// اعتبارسنجی ویرایش پذیرش
        /// </summary>
        /// <param name="model">مدل ویرایش پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateReceptionEditAsync(ReceptionEditViewModel model);

        #endregion

        #region Entity Validation Methods

        /// <summary>
        /// اعتبارسنجی بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidatePatientAsync(int patientId);

        /// <summary>
        /// اعتبارسنجی پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateDoctorAsync(int doctorId, DateTime receptionDate);

        /// <summary>
        /// اعتبارسنجی خدمات
        /// </summary>
        /// <param name="serviceIds">شناسه‌های خدمات</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateServicesAsync(List<int> serviceIds);

        /// <summary>
        /// اعتبارسنجی بیمه
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateInsuranceAsync(int patientId, DateTime receptionDate);

        #endregion

        #region Business Rules Validation

        /// <summary>
        /// بررسی تداخل زمانی
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <param name="excludeReceptionId">شناسه پذیرش برای حذف از بررسی (در ویرایش)</param>
        /// <returns>نتیجه بررسی</returns>
        Task<CustomValidationResult> ValidateTimeConflictAsync(int patientId, int doctorId, DateTime receptionDate, int? excludeReceptionId = null);

        /// <summary>
        /// بررسی ظرفیت پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<CustomValidationResult> ValidateDoctorCapacityAsync(int doctorId, DateTime receptionDate);

        /// <summary>
        /// بررسی تاریخ پذیرش
        /// </summary>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<CustomValidationResult> ValidateReceptionDateAsync(DateTime receptionDate);

        /// <summary>
        /// بررسی ساعات کاری
        /// </summary>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<CustomValidationResult> ValidateWorkingHoursAsync(DateTime receptionDate);

        #endregion

        #region Advanced Validation Methods

        /// <summary>
        /// بررسی قوانین خاص بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<CustomValidationResult> ValidatePatientSpecificRulesAsync(int patientId, DateTime receptionDate);

        /// <summary>
        /// بررسی قوانین خاص پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<CustomValidationResult> ValidateDoctorSpecificRulesAsync(int doctorId, DateTime receptionDate);

        /// <summary>
        /// بررسی قوانین خاص خدمات
        /// </summary>
        /// <param name="serviceIds">شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<CustomValidationResult> ValidateServiceSpecificRulesAsync(List<int> serviceIds, DateTime receptionDate);

        #endregion

        #region Emergency and Special Cases

        /// <summary>
        /// اعتبارسنجی پذیرش اورژانس
        /// </summary>
        /// <param name="model">مدل پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateEmergencyReceptionAsync(ReceptionCreateViewModel model);

        /// <summary>
        /// اعتبارسنجی پذیرش آنلاین
        /// </summary>
        /// <param name="model">مدل پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateOnlineReceptionAsync(ReceptionCreateViewModel model);

        /// <summary>
        /// اعتبارسنجی پذیرش ویژه
        /// </summary>
        /// <param name="model">مدل پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateSpecialReceptionAsync(ReceptionCreateViewModel model);

        #endregion

        #region Batch Validation Methods

        /// <summary>
        /// اعتبارسنجی دسته‌ای پذیرش‌ها
        /// </summary>
        /// <param name="models">مدل‌های پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateBatchReceptionsAsync(List<ReceptionCreateViewModel> models);

        /// <summary>
        /// اعتبارسنجی دسته‌ای بیماران
        /// </summary>
        /// <param name="patientIds">شناسه‌های بیماران</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateBatchPatientsAsync(List<int> patientIds);

        /// <summary>
        /// اعتبارسنجی دسته‌ای پزشکان
        /// </summary>
        /// <param name="doctorIds">شناسه‌های پزشکان</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<CustomValidationResult> ValidateBatchDoctorsAsync(List<int> doctorIds, DateTime receptionDate);

        #endregion
    }
}
