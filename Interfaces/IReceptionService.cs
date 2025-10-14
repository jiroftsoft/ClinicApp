using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using ClinicApp.ViewModels.Payment;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Service Interface برای مدیریت پذیرش‌های بیماران در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل پذیرش‌های بیماران (Normal, Emergency, Special, Online)
    /// 2. استفاده از ServiceResult Enhanced pattern
    /// 3. پشتیبانی از FluentValidation
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. پشتیبانی از صفحه‌بندی و جستجو
    /// 6. مدیریت Lookup Lists برای UI
    /// 7. رعایت استانداردهای پزشکی ایران
    /// 8. یکپارچه‌سازی با سیستم بیمه و محاسبات
    /// 9. پشتیبانی از استعلام کمکی خارجی
    /// 10. مدیریت کامل تراکنش‌های مالی
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط منطق کسب‌وکار پذیرش
    /// ✅ Separation of Concerns: Repository layer فقط دسترسی به داده
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Service layer منطق کسب‌وکار
    /// </summary>
    public interface IReceptionService
    {
        #region Core CRUD Operations

        /// <summary>
        /// ایجاد پذیرش جدید
        /// </summary>
        /// <param name="model">مدل ایجاد پذیرش</param>
        /// <returns>نتیجه ایجاد پذیرش</returns>
        Task<ServiceResult<ReceptionDetailsViewModel>> CreateReceptionAsync(ReceptionCreateViewModel model);

        /// <summary>
        /// به‌روزرسانی پذیرش موجود
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <param name="model">مدل به‌روزرسانی پذیرش</param>
        /// <returns>نتیجه به‌روزرسانی پذیرش</returns>
        Task<ServiceResult<ReceptionDetailsViewModel>> UpdateReceptionAsync(ReceptionEditViewModel model);

        /// <summary>
        /// حذف نرم پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>نتیجه حذف پذیرش</returns>
        Task<ServiceResult> DeleteReceptionAsync(int id);

        /// <summary>
        /// دریافت جزئیات پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>جزئیات پذیرش</returns>
        Task<ServiceResult<ReceptionDetailsViewModel>> GetReceptionDetailsAsync(int id);

        /// <summary>
        /// دریافت پذیرش بر اساس شناسه (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>اطلاعات پذیرش</returns>
        Task<ServiceResult<ReceptionDetailsViewModel>> GetReceptionByIdAsync(int id);

        #endregion

        #region Search and List Operations

        /// <summary>
        /// دریافت لیست پذیرش‌ها با صفحه‌بندی
        /// </summary>
        /// <param name="patientId">شناسه بیمار (اختیاری)</param>
        /// <param name="doctorId">شناسه پزشک (اختیاری)</param>
        /// <param name="status">وضعیت پذیرش (اختیاری)</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پذیرش‌ها با صفحه‌بندی</returns>
        Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetReceptionsAsync(
            int? patientId = null,
            int? doctorId = null,
            ReceptionStatus? status = null,
            string searchTerm = null,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// جستجوی پذیرش‌ها
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه جستجو</returns>
        Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> SearchReceptionsAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// جستجوی پذیرش‌ها با مدل (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="model">مدل جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه جستجو</returns>
        Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> SearchReceptionsAsync(
            ReceptionSearchViewModel model,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// دریافت پذیرش‌های بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پذیرش‌های بیمار</returns>
        Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetPatientReceptionsAsync(
            int patientId,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// دریافت تاریخچه پذیرش‌های بیمار (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>تاریخچه پذیرش‌های بیمار</returns>
        Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetPatientReceptionHistoryAsync(
            int patientId,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// دریافت پذیرش‌های پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ (اختیاری)</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پذیرش‌های پزشک</returns>
        Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetDoctorReceptionsAsync(
            int doctorId,
            DateTime? date = null,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// دریافت پذیرش‌ها بر اساس بازه زمانی
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پذیرش‌ها در بازه زمانی</returns>
        Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetReceptionsByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// دریافت پذیرش‌های بیمار بر اساس شناسه
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پذیرش‌های بیمار</returns>
        Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetReceptionsByPatientIdAsync(
            int patientId,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// دریافت پذیرش‌های پزشک بر اساس شناسه
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پذیرش‌های پزشک</returns>
        Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetReceptionsByDoctorIdAsync(
            int doctorId,
            int pageNumber = 1,
            int pageSize = 10);

        #endregion

        #region Patient Lookup Operations

        /// <summary>
        /// جستجوی بیمار بر اساس کد ملی
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <returns>اطلاعات بیمار</returns>
        Task<ServiceResult<ReceptionPatientLookupViewModel>> LookupPatientByNationalCodeAsync(string nationalCode);

        /// <summary>
        /// جستجوی بیمار بر اساس کد ملی (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <returns>اطلاعات بیمار</returns>
        Task<ServiceResult<ReceptionPatientLookupViewModel>> SearchPatientByNationalCodeAsync(string nationalCode);

        /// <summary>
        /// جستجوی بیمار بر اساس نام
        /// </summary>
        /// <param name="name">نام بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست بیماران</returns>
        Task<ServiceResult<PagedResult<ReceptionPatientLookupViewModel>>> SearchPatientsByNameAsync(
            string name,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// ایجاد بیمار جدید در حین پذیرش
        /// </summary>
        /// <param name="model">مدل ایجاد بیمار</param>
        /// <returns>اطلاعات بیمار جدید</returns>
        Task<ServiceResult<ReceptionPatientLookupViewModel>> CreatePatientInlineAsync(PatientCreateEditViewModel model);

        /// <summary>
        /// ایجاد بیمار جدید (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="model">مدل ایجاد بیمار</param>
        /// <returns>اطلاعات بیمار جدید</returns>
        Task<ServiceResult<ReceptionPatientLookupViewModel>> CreatePatientAsync(PatientCreateEditViewModel model);

        #endregion

        #region Service and Doctor Lookup Operations

        /// <summary>
        /// دریافت لیست دسته‌بندی خدمات
        /// </summary>
        /// <returns>لیست دسته‌بندی خدمات</returns>
        Task<ServiceResult<List<ReceptionServiceCategoryLookupViewModel>>> GetServiceCategoriesAsync();

        /// <summary>
        /// دریافت لیست خدمات بر اساس دسته‌بندی
        /// </summary>
        /// <param name="categoryId">شناسه دسته‌بندی</param>
        /// <returns>لیست خدمات</returns>
        Task<ServiceResult<List<ReceptionServiceLookupViewModel>>> GetServicesByCategoryAsync(int categoryId);

        /// <summary>
        /// دریافت لیست پزشکان
        /// </summary>
        /// <returns>لیست پزشکان</returns>
        Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDoctorsAsync();

        /// <summary>
        /// دریافت لیست پزشکان بر اساس تخصص
        /// </summary>
        /// <param name="specializationId">شناسه تخصص</param>
        /// <returns>لیست پزشکان</returns>
        Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDoctorsBySpecializationAsync(int specializationId);

        /// <summary>
        /// دریافت دپارتمان‌های پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>لیست دپارتمان‌های پزشک</returns>
        Task<ServiceResult<List<ReceptionDoctorDepartmentLookupViewModel>>> GetDoctorDepartmentsAsync(int doctorId);

        /// <summary>
        /// دریافت سرفصل‌های خدمات بر اساس دپارتمان‌ها
        /// </summary>
        /// <param name="departmentIds">شناسه‌های دپارتمان‌ها</param>
        /// <returns>لیست سرفصل‌های خدمات</returns>
        Task<ServiceResult<List<ReceptionServiceCategoryLookupViewModel>>> GetServiceCategoriesByDepartmentsAsync(List<int> departmentIds);

        /// <summary>
        /// دریافت خدمات بر اساس دپارتمان‌ها (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="departmentIds">شناسه‌های دپارتمان‌ها</param>
        /// <returns>لیست خدمات</returns>
        Task<ServiceResult<List<ReceptionServiceLookupViewModel>>> GetServicesByDepartmentsAsync(List<int> departmentIds);

        #endregion

        #region Insurance Operations

        /// <summary>
        /// دریافت بیمه‌های فعال بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست بیمه‌های فعال</returns>
        Task<ServiceResult<List<ReceptionPatientInsuranceLookupViewModel>>> GetPatientActiveInsurancesAsync(int patientId);

        /// <summary>
        /// محاسبه هزینه‌های پذیرش
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">شناسه‌های خدمات</param>
        /// <param name="insuranceId">شناسه بیمه (اختیاری)</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه محاسبه هزینه‌ها</returns>
        Task<ServiceResult<ReceptionCostCalculationViewModel>> CalculateReceptionCostsAsync(
            int patientId,
            List<int> serviceIds,
            int? insuranceId,
            DateTime receptionDate);

        #endregion

        #region External Inquiry Operations

        /// <summary>
        /// استعلام کمکی اطلاعات بیمار
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <returns>نتیجه استعلام</returns>
        Task<ServiceResult<PatientInquiryViewModel>> InquiryPatientInfoAsync(string nationalCode, DateTime birthDate);

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// بررسی امکان پذیرش بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<ReceptionValidationViewModel>> ValidateReceptionAsync(
            int patientId,
            int doctorId,
            DateTime receptionDate);

        /// <summary>
        /// دریافت آمار پذیرش‌های روز
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار پذیرش‌ها</returns>
        Task<ServiceResult<ReceptionDailyStatsViewModel>> GetDailyStatsAsync(DateTime date);

        /// <summary>
        /// دریافت آمار پذیرش‌های پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار پذیرش‌های پزشک</returns>
        Task<ServiceResult<ReceptionDoctorStatsViewModel>> GetDoctorStatsAsync(int doctorId, DateTime date);

        /// <summary>
        /// جستجوی بیماران
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه جستجوی بیماران</returns>
        Task<ServiceResult<PagedResult<PatientDetailsViewModel>>> SearchPatientsAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10);

        #endregion

        #region Payment Operations

        /// <summary>
        /// افزودن پرداخت به پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="paymentModel">مدل پرداخت</param>
        /// <returns>نتیجه افزودن پرداخت</returns>
        Task<ServiceResult<PaymentTransactionViewModel>> AddPaymentAsync(
            int receptionId,
            PaymentTransactionCreateViewModel paymentModel);

        /// <summary>
        /// دریافت تراکنش‌های پرداخت پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>لیست تراکنش‌های پرداخت</returns>
        Task<ServiceResult<List<PaymentTransactionViewModel>>> GetReceptionPaymentsAsync(int receptionId);

        #endregion

        #region Lookup Lists for UI

        /// <summary>
        /// دریافت لیست‌های مورد نیاز برای UI
        /// </summary>
        /// <returns>لیست‌های مورد نیاز</returns>
        Task<ServiceResult<ReceptionLookupListsViewModel>> GetLookupListsAsync();

        /// <summary>
        /// استعلام هویت بیمار از سیستم خارجی (شبکه شمس)
        /// </summary>
        /// <param name="nationalCode">کد ملی بیمار</param>
        /// <param name="birthDate">تاریخ تولد بیمار</param>
        /// <returns>نتیجه استعلام هویت</returns>
        Task<ServiceResult<PatientInquiryViewModel>> InquiryPatientIdentityAsync(string nationalCode, DateTime birthDate);

        #endregion

        #region Clinic and Department Management

        /// <summary>
        /// دریافت کلینیک‌های فعال
        /// </summary>
        /// <returns>لیست کلینیک‌های فعال</returns>
        Task<ServiceResult<List<ReceptionClinicLookupViewModel>>> GetActiveClinicsAsync();

        /// <summary>
        /// دریافت دپارتمان‌های کلینیک
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک</param>
        /// <returns>لیست دپارتمان‌های کلینیک</returns>
        Task<ServiceResult<List<ReceptionDepartmentLookupViewModel>>> GetClinicDepartmentsAsync(int clinicId);

        #endregion

        #region Shift Management

        /// <summary>
        /// دریافت شیفت فعلی
        /// </summary>
        /// <returns>نوع شیفت فعلی</returns>
        Task<ServiceResult<ShiftType>> GetCurrentShiftAsync();

        /// <summary>
        /// دریافت اطلاعات شیفت
        /// </summary>
        /// <param name="shiftType">نوع شیفت</param>
        /// <returns>اطلاعات شیفت</returns>
        Task<ServiceResult<ShiftInfo>> GetShiftInfoAsync(ShiftType shiftType);

        /// <summary>
        /// دریافت پزشکان بر اساس شیفت
        /// </summary>
        /// <param name="shiftType">نوع شیفت</param>
        /// <returns>لیست پزشکان شیفت</returns>
        Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDoctorsByShiftAsync(ShiftType shiftType);

        #endregion

        #region Patient Field Management

        /// <summary>
        /// به‌روزرسانی فیلد بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="fieldName">نام فیلد</param>
        /// <param name="fieldValue">مقدار جدید</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult<bool>> UpdatePatientFieldAsync(int patientId, string fieldName, string fieldValue);

        #endregion

        #region Insurance Management

        /// <summary>
        /// دریافت بیمه‌های پایه و تکمیلی
        /// </summary>
        /// <returns>لیست بیمه‌های پایه و تکمیلی</returns>
        Task<ServiceResult<InsuranceProvidersViewModel>> GetInsuranceProvidersAsync();

        /// <summary>
        /// دریافت بیمه‌های تکمیلی بر اساس بیمه پایه
        /// </summary>
        /// <param name="baseInsuranceId">شناسه بیمه پایه</param>
        /// <returns>لیست بیمه‌های تکمیلی</returns>
        Task<ServiceResult<List<ReceptionInsuranceLookupViewModel>>> GetSupplementaryInsurancesAsync(int baseInsuranceId);

        /// <summary>
        /// محاسبه بیمه برای پذیرش
        /// </summary>
        /// <param name="baseInsuranceId">شناسه بیمه پایه</param>
        /// <param name="supplementaryInsuranceId">شناسه بیمه تکمیلی</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>نتیجه محاسبه بیمه</returns>
        Task<ServiceResult<ClinicApp.ViewModels.Reception.InsuranceCalculationViewModel>> CalculateInsuranceAsync(int baseInsuranceId, int? supplementaryInsuranceId, int serviceId);

        /// <summary>
        /// تغییر بیمه بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="baseInsuranceId">شناسه بیمه پایه</param>
        /// <param name="supplementaryInsuranceId">شناسه بیمه تکمیلی</param>
        /// <returns>نتیجه تغییر بیمه</returns>
        Task<ServiceResult<bool>> ChangePatientInsuranceAsync(int patientId, int baseInsuranceId, int? supplementaryInsuranceId);

        #endregion

        #region Service Search

        /// <summary>
        /// جستجوی خدمات بر اساس کد یا نام
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>نتایج جستجوی خدمات</returns>
        Task<ServiceResult<List<ServiceSearchResultViewModel>>> SearchServicesAsync(string searchTerm);

        #endregion
    }
}