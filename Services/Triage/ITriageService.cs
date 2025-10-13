using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Triage;
using ClinicApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Services.Triage;

/// <summary>
/// رابط سرویس تریاژ - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت کامل فرآیند تریاژ بیماران
/// 2. پشتیبانی از استانداردهای بین‌المللی تریاژ
/// 3. مدیریت صف تریاژ با اولویت‌بندی هوشمند
/// 4. پشتیبانی از علائم حیاتی و ارزیابی اولویت
/// 5. یکپارچگی با سیستم پذیرش موجود
/// </summary>
public interface ITriageService
{
    #region ارزیابی تریاژ (Triage Assessment)

    /// <summary>
    /// ایجاد ارزیابی تریاژ جدید
    /// </summary>
    /// <param name="patientId">شناسه بیمار</param>
    /// <param name="chiefComplaint">شکایت اصلی</param>
    /// <param name="vitalSigns">علائم حیاتی</param>
    /// <param name="assessmentNotes">یادداشت‌های ارزیابی</param>
    /// <returns>نتیجه ارزیابی تریاژ</returns>
    Task<ServiceResult<TriageAssessment>> CreateTriageAssessmentAsync(
        int patientId, 
        string chiefComplaint, 
        TriageVitalSigns vitalSigns, 
        string assessmentNotes = null);

    /// <summary>
    /// به‌روزرسانی ارزیابی تریاژ
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <param name="level">سطح تریاژ</param>
    /// <param name="priority">اولویت</param>
    /// <param name="notes">یادداشت‌ها</param>
    /// <returns>نتیجه به‌روزرسانی</returns>
    Task<ServiceResult<TriageAssessment>> UpdateTriageAssessmentAsync(
        int assessmentId, 
        TriageLevel level, 
        int priority, 
        string notes = null);

    /// <summary>
    /// تکمیل ارزیابی تریاژ
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <param name="recommendedDepartmentId">شناسه دپارتمان پیشنهادی</param>
    /// <param name="recommendedDoctorId">شناسه پزشک پیشنهادی</param>
    /// <returns>نتیجه تکمیل</returns>
    Task<ServiceResult<TriageAssessment>> CompleteTriageAssessmentAsync(
        int assessmentId, 
        int? recommendedDepartmentId = null, 
        int? recommendedDoctorId = null);

    /// <summary>
    /// دریافت ارزیابی تریاژ بر اساس شناسه
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <returns>ارزیابی تریاژ</returns>
    Task<ServiceResult<TriageAssessment>> GetTriageAssessmentByIdAsync(int assessmentId);

    /// <summary>
    /// دریافت ارزیابی‌های تریاژ بیمار
    /// </summary>
    /// <param name="patientId">شناسه بیمار</param>
    /// <param name="includeCompleted">شامل تکمیل شده‌ها</param>
    /// <returns>لیست ارزیابی‌ها</returns>
    Task<ServiceResult<List<TriageAssessment>>> GetPatientTriageAssessmentsAsync(
        int patientId, 
        bool includeCompleted = false);

    #endregion

    #region صف تریاژ (Triage Queue)

    /// <summary>
    /// اضافه کردن بیمار به صف تریاژ
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <param name="priority">اولویت</param>
    /// <param name="targetDepartmentId">شناسه دپارتمان مقصد</param>
    /// <param name="targetDoctorId">شناسه پزشک مقصد</param>
    /// <returns>نتیجه اضافه کردن به صف</returns>
    Task<ServiceResult<TriageQueue>> AddToTriageQueueAsync(
        int assessmentId, 
        int priority, 
        int? targetDepartmentId = null, 
        int? targetDoctorId = null);

    /// <summary>
    /// فراخوانی بیمار از صف تریاژ
    /// </summary>
    /// <param name="queueId">شناسه صف</param>
    /// <param name="calledByUserId">شناسه کاربر فراخواننده</param>
    /// <returns>نتیجه فراخوانی</returns>
    Task<ServiceResult<TriageQueue>> CallPatientFromQueueAsync(
        int queueId, 
        string calledByUserId);

    /// <summary>
    /// تکمیل صف تریاژ
    /// </summary>
    /// <param name="queueId">شناسه صف</param>
    /// <param name="completedByUserId">شناسه کاربر تکمیل کننده</param>
    /// <returns>نتیجه تکمیل</returns>
    Task<ServiceResult<TriageQueue>> CompleteTriageQueueAsync(
        int queueId, 
        string completedByUserId);

    /// <summary>
    /// دریافت صف تریاژ فعلی
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
    /// <param name="status">وضعیت صف (اختیاری)</param>
    /// <returns>لیست صف تریاژ</returns>
    Task<ServiceResult<List<TriageQueue>>> GetTriageQueueAsync(
        int? departmentId = null, 
        TriageQueueStatus? status = null);

    /// <summary>
    /// دریافت صف تریاژ بر اساس اولویت
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>صف تریاژ مرتب شده بر اساس اولویت</returns>
    Task<ServiceResult<List<TriageQueue>>> GetTriageQueueByPriorityAsync(int? departmentId = null);

    #endregion

    #region علائم حیاتی (Vital Signs)

    /// <summary>
    /// ثبت علائم حیاتی
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <param name="vitalSigns">علائم حیاتی</param>
    /// <returns>نتیجه ثبت</returns>
    Task<ServiceResult<TriageVitalSigns>> RecordVitalSignsAsync(
        int assessmentId, 
        TriageVitalSigns vitalSigns);

    /// <summary>
    /// دریافت علائم حیاتی ارزیابی
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <returns>لیست علائم حیاتی</returns>
    Task<ServiceResult<List<TriageVitalSigns>>> GetVitalSignsAsync(int assessmentId);

    /// <summary>
    /// ارزیابی علائم حیاتی
    /// </summary>
    /// <param name="vitalSigns">علائم حیاتی</param>
    /// <returns>نتیجه ارزیابی</returns>
    Task<ServiceResult<VitalSignStatus>> EvaluateVitalSignsAsync(TriageVitalSigns vitalSigns);

    #endregion

    #region پروتکل‌های تریاژ (Triage Protocols)

    /// <summary>
    /// دریافت پروتکل‌های تریاژ فعال
    /// </summary>
    /// <param name="type">نوع پروتکل (اختیاری)</param>
    /// <param name="targetLevel">سطح هدف (اختیاری)</param>
    /// <returns>لیست پروتکل‌ها</returns>
    Task<ServiceResult<List<TriageProtocol>>> GetActiveProtocolsAsync(
        TriageProtocolType? type = null, 
        TriageLevel? targetLevel = null);

    /// <summary>
    /// اعمال پروتکل تریاژ
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <param name="protocolId">شناسه پروتکل</param>
    /// <returns>نتیجه اعمال</returns>
    Task<ServiceResult<bool>> ApplyProtocolAsync(int assessmentId, int protocolId);

    /// <summary>
    /// پیشنهاد پروتکل‌های مناسب
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <returns>لیست پروتکل‌های پیشنهادی</returns>
    Task<ServiceResult<List<TriageProtocol>>> SuggestProtocolsAsync(int assessmentId);

    #endregion

    #region گزارش‌گیری و آمار (Reporting & Statistics)

    /// <summary>
    /// دریافت تعداد کل ارزیابی‌های تریاژ
    /// </summary>
    /// <returns>تعداد کل ارزیابی‌ها</returns>
    Task<ServiceResult<int>> GetTriageAssessmentsCountAsync();

    /// <summary>
    /// دریافت آمار تریاژ روزانه
    /// </summary>
    /// <param name="date">تاریخ</param>
    /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
    /// <returns>آمار روزانه</returns>
    Task<ServiceResult<TriageDailyStats>> GetDailyStatsAsync(
        DateTime date, 
        int? departmentId = null);

    /// <summary>
    /// دریافت آمار تریاژ هفتگی
    /// </summary>
    /// <param name="startDate">تاریخ شروع</param>
    /// <param name="endDate">تاریخ پایان</param>
    /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
    /// <returns>آمار هفتگی</returns>
    Task<ServiceResult<TriageWeeklyStats>> GetWeeklyStatsAsync(
        DateTime startDate, 
        DateTime endDate, 
        int? departmentId = null);

    /// <summary>
    /// دریافت گزارش تریاژ
    /// </summary>
    /// <param name="reportType">نوع گزارش</param>
    /// <param name="startDate">تاریخ شروع</param>
    /// <param name="endDate">تاریخ پایان</param>
    /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
    /// <returns>گزارش تریاژ</returns>
    Task<ServiceResult<TriageReport>> GenerateReportAsync(
        TriageReportType reportType, 
        DateTime startDate, 
        DateTime endDate, 
        int? departmentId = null);

    #endregion

    #region مدیریت اولویت (Priority Management)

    /// <summary>
    /// محاسبه اولویت تریاژ
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <returns>اولویت محاسبه شده</returns>
    Task<ServiceResult<int>> CalculateTriagePriorityAsync(int assessmentId);

    /// <summary>
    /// به‌روزرسانی اولویت صف
    /// </summary>
    /// <param name="queueId">شناسه صف</param>
    /// <param name="newPriority">اولویت جدید</param>
    /// <returns>نتیجه به‌روزرسانی</returns>
    Task<ServiceResult<TriageQueue>> UpdateQueuePriorityAsync(
        int queueId, 
        int newPriority);

    /// <summary>
    /// مرتب‌سازی صف بر اساس اولویت
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>نتیجه مرتب‌سازی</returns>
    Task<ServiceResult<bool>> ReorderQueueByPriorityAsync(int? departmentId = null);

    #endregion

    #region اعلان‌ها و هشدارها (Notifications & Alerts)

    /// <summary>
    /// ارسال اعلان تریاژ
    /// </summary>
    /// <param name="notificationType">نوع اعلان</param>
    /// <param name="message">پیام</param>
    /// <param name="targetUsers">کاربران هدف</param>
    /// <returns>نتیجه ارسال</returns>
    Task<ServiceResult<bool>> SendTriageNotificationAsync(
        TriageNotificationType notificationType, 
        string message, 
        List<string> targetUsers);

    /// <summary>
    /// بررسی هشدارهای تریاژ
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <returns>لیست هشدارها</returns>
    Task<ServiceResult<List<TriageAlert>>> CheckTriageAlertsAsync(int assessmentId);

    #endregion
    #region پایش مجدد (Reassessment)

    /// <summary>
    /// ایجاد پایش مجدد تریاژ
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <param name="dto">اطلاعات پایش مجدد</param>
    /// <returns>نتیجه پایش مجدد</returns>
    Task<ServiceResult<TriageReassessment>> CreateReassessmentAsync(int assessmentId, TriageReassessmentDto dto);

    /// <summary>
    /// دریافت لیست پایش‌های مجدد
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <returns>لیست پایش‌های مجدد</returns>
    Task<ServiceResult<List<TriageReassessment>>> GetReassessmentsAsync(int assessmentId);

    #endregion

    #region مدیریت پروتکل‌ها (Protocol Management)

    /// <summary>
    /// دریافت پروتکل‌های تریاژ
    /// </summary>
    /// <param name="protocolType">نوع پروتکل (اختیاری)</param>
    /// <param name="searchTerm">عبارت جستجو (اختیاری)</param>
    /// <returns>لیست پروتکل‌ها</returns>
    Task<ServiceResult<List<TriageProtocol>>> GetTriageProtocolsAsync(int? protocolType = null, string searchTerm = "");

    /// <summary>
    /// دریافت پروتکل تریاژ
    /// </summary>
    /// <param name="protocolId">شناسه پروتکل</param>
    /// <returns>پروتکل تریاژ</returns>
    Task<ServiceResult<TriageProtocol>> GetTriageProtocolAsync(int protocolId);

    /// <summary>
    /// ایجاد پروتکل تریاژ
    /// </summary>
    /// <param name="protocol">پروتکل تریاژ</param>
    /// <returns>نتیجه ایجاد</returns>
    Task<ServiceResult<TriageProtocol>> CreateTriageProtocolAsync(TriageProtocol protocol);

    /// <summary>
    /// به‌روزرسانی پروتکل تریاژ
    /// </summary>
    /// <param name="protocolId">شناسه پروتکل</param>
    /// <param name="name">نام</param>
    /// <param name="description">توضیحات</param>
    /// <param name="protocolType">نوع پروتکل</param>
    /// <param name="criteria">معیارها</param>
    /// <param name="actions">اقدامات</param>
    /// <param name="priority">اولویت</param>
    /// <param name="isActive">فعال</param>
    /// <returns>نتیجه به‌روزرسانی</returns>
    Task<ServiceResult<TriageProtocol>> UpdateTriageProtocolAsync(
        int protocolId,
        string name,
        string description,
        TriageProtocolType protocolType,
        string criteria,
        string actions,
        int priority,
        bool isActive);

    /// <summary>
    /// حذف پروتکل تریاژ
    /// </summary>
    /// <param name="protocolId">شناسه پروتکل</param>
    /// <returns>نتیجه حذف</returns>
    Task<ServiceResult> DeleteTriageProtocolAsync(int protocolId);

    /// <summary>
    /// دریافت لیست ارزیابی‌های تریاژ
    /// </summary>
    /// <param name="patientId">شناسه بیمار (اختیاری)</param>
    /// <param name="level">سطح تریاژ (اختیاری)</param>
    /// <param name="status">وضعیت (اختیاری)</param>
    /// <param name="startDate">تاریخ شروع (اختیاری)</param>
    /// <param name="endDate">تاریخ پایان (اختیاری)</param>
    /// <param name="pageNumber">شماره صفحه</param>
    /// <param name="pageSize">اندازه صفحه</param>
    /// <returns>لیست ارزیابی‌های تریاژ</returns>
    Task<ServiceResult<PagedResult<TriageAssessment>>> GetTriageAssessmentsAsync(
        int? patientId = null,
        TriageLevel? level = null,
        TriageStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 20);

    /// <summary>
    /// دریافت ارزیابی تریاژ بر اساس شناسه
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <returns>ارزیابی تریاژ</returns>
    Task<ServiceResult<TriageAssessment>> GetTriageAssessmentAsync(int assessmentId);

    /// <summary>
    /// دریافت ارزیابی‌های اخیر
    /// </summary>
    /// <param name="hours">تعداد ساعت‌های اخیر</param>
    /// <returns>لیست ارزیابی‌های اخیر</returns>
    Task<ServiceResult<List<TriageAssessment>>> GetRecentAssessmentsAsync(int hours = 24);

    /// <summary>
    /// دریافت هشدارهای فعال
    /// </summary>
    /// <returns>لیست هشدارهای فعال</returns>
    Task<ServiceResult<List<TriageAlert>>> GetActiveAlertsAsync();

    /// <summary>
    /// دریافت ارزیابی‌ها بر اساس بازه زمانی
    /// </summary>
    /// <param name="startDate">تاریخ شروع</param>
    /// <param name="endDate">تاریخ پایان</param>
    /// <returns>لیست ارزیابی‌ها</returns>
    Task<ServiceResult<List<TriageAssessment>>> GetAssessmentsByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// دریافت تعداد ارزیابی‌ها
    /// </summary>
    /// <param name="startDate">تاریخ شروع (اختیاری)</param>
    /// <param name="endDate">تاریخ پایان (اختیاری)</param>
    /// <returns>تعداد ارزیابی‌ها</returns>
    Task<ServiceResult<int>> GetAssessmentsCountAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// دریافت تعداد بیماران بحرانی
    /// </summary>
    /// <returns>تعداد بیماران بحرانی</returns>
    Task<ServiceResult<int>> GetCriticalPatientsCountAsync();

    /// <summary>
    /// دریافت میانگین زمان انتظار
    /// </summary>
    /// <param name="startDate">تاریخ شروع (اختیاری)</param>
    /// <param name="endDate">تاریخ پایان (اختیاری)</param>
    /// <returns>میانگین زمان انتظار</returns>
    Task<ServiceResult<double>> GetAverageWaitTimeAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// دریافت ارزیابی مجدد
    /// </summary>
    /// <param name="reassessmentId">شناسه ارزیابی مجدد</param>
    /// <returns>ارزیابی مجدد</returns>
    Task<ServiceResult<TriageReassessment>> GetReassessmentAsync(int reassessmentId);

    /// <summary>
    /// دریافت پروتکل‌های فعال
    /// </summary>
    /// <param name="protocolType">نوع پروتکل (اختیاری)</param>
    /// <returns>لیست پروتکل‌های فعال</returns>
    Task<ServiceResult<List<TriageProtocol>>> GetActiveProtocolsAsync(int? protocolType = null);

    /// <summary>
    /// دریافت پیشنهادات پروتکل
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <returns>لیست پیشنهادات</returns>
    Task<ServiceResult<List<TriageProtocol>>> GetProtocolSuggestionsAsync(int assessmentId);

    #endregion

    #region خلاصه و پری‌فیل (Summary & Profile)

    /// <summary>
    /// دریافت خلاصه ارزیابی تریاژ
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <returns>خلاصه ارزیابی</returns>
    Task<ServiceResult<TriageSummaryDto>> GetSummaryAsync(int assessmentId);

    #endregion

    #region مدیریت صف (Queue Management)

    /// <summary>
    /// بستن صف‌های فعال برای ارزیابی
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <returns>نتیجه عملیات</returns>
    Task<ServiceResult> CloseActiveQueueIfAnyAsync(int assessmentId);

    /// <summary>
    /// بررسی قابلیت لینک به پذیرش
    /// </summary>
    /// <param name="assessmentId">شناسه ارزیابی</param>
    /// <returns>قابلیت لینک</returns>
    Task<ServiceResult<bool>> CanLinkToReceptionAsync(int assessmentId);

    /// <summary>
    /// دریافت لیست آماده پذیرش
    /// </summary>
    /// <param name="departmentId">شناسه بخش (اختیاری)</param>
    /// <returns>لیست آماده پذیرش</returns>
    Task<ServiceResult<List<TriageAssessment>>> GetCompletedForAdmissionAsync(int? departmentId = null);

    #endregion
}



/// <summary>
/// آمار هفتگی تریاژ
/// </summary>
public class TriageWeeklyStats
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalAssessments { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalPending { get; set; }
    public decimal AverageWaitTime { get; set; }
    public decimal AverageAssessmentTime { get; set; }
    public List<TriageDailyStats> DailyStats { get; set; } = new List<TriageDailyStats>();
}

/// <summary>
/// گزارش تریاژ
/// </summary>
public class TriageReport
{
    public TriageReportType ReportType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalAssessments { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalPending { get; set; }
    public decimal AverageWaitTime { get; set; }
    public decimal AverageAssessmentTime { get; set; }
    public List<TriageAssessment> Assessments { get; set; } = new List<TriageAssessment>();
    public List<TriageQueue> Queues { get; set; } = new List<TriageQueue>();
}

/// <summary>
/// هشدار تریاژ
/// </summary>
public class TriageAlert
{
    public int AlertId { get; set; }
    public int AssessmentId { get; set; }
    public string AlertType { get; set; }
    public string Message { get; set; }
    public TriageLevel Severity { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

    


