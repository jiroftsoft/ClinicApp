using System.ComponentModel;

namespace ClinicApp.Models.Enums;

/// <summary>
/// سطوح تریاژ ESI (Emergency Severity Index) - استاندارد بین‌المللی
/// </summary>
public enum TriageLevel
{
    /// <summary>
    /// ESI 1 - نیاز به مراقبت فوری (0-2 دقیقه) - قرمز
    /// </summary>
    [Description("ESI 1 - نیاز به مراقبت فوری")]
    ESI1 = 1,

    /// <summary>
    /// ESI 2 - نیاز به مراقبت فوری (2-10 دقیقه) - نارنجی
    /// </summary>
    [Description("ESI 2 - نیاز به مراقبت فوری")]
    ESI2 = 2,

    /// <summary>
    /// ESI 3 - نیاز به مراقبت فوری (10-60 دقیقه) - زرد
    /// </summary>
    [Description("ESI 3 - نیاز به مراقبت فوری")]
    ESI3 = 3,

    /// <summary>
    /// ESI 4 - نیاز به مراقبت فوری (60+ دقیقه) - سبز
    /// </summary>
    [Description("ESI 4 - نیاز به مراقبت فوری")]
    ESI4 = 4,

    /// <summary>
    /// ESI 5 - نیاز به مراقبت فوری (120+ دقیقه) - آبی
    /// </summary>
    [Description("ESI 5 - نیاز به مراقبت فوری")]
    ESI5 = 5
}

/// <summary>
/// وضعیت‌های ارزیابی تریاژ
/// </summary>
public enum TriageStatus
{
    /// <summary>
    /// در انتظار
    /// </summary>
    [Description("در انتظار")]
    Pending = 1,

    /// <summary>
    /// در حال انجام
    /// </summary>
    [Description("در حال انجام")]
    InProgress = 2,

    /// <summary>
    /// تکمیل شده
    /// </summary>
    [Description("تکمیل شده")]
    Completed = 3,

    /// <summary>
    /// لغو شده
    /// </summary>
    [Description("لغو شده")]
    Cancelled = 4,

    /// <summary>
    /// نیاز به بررسی مجدد
    /// </summary>
    [Description("نیاز به بررسی مجدد")]
    RequiresReview = 5
}

/// <summary>
/// وضعیت‌های صف تریاژ
/// </summary>
public enum TriageQueueStatus
{
    /// <summary>
    /// در انتظار
    /// </summary>
    [Description("در انتظار")]
    Waiting = 1,

    /// <summary>
    /// فراخوانده شده
    /// </summary>
    [Description("فراخوانده شده")]
    Called = 2,

    /// <summary>
    /// در حال بررسی
    /// </summary>
    [Description("در حال بررسی")]
    InProgress = 3,

    /// <summary>
    /// تکمیل شده
    /// </summary>
    [Description("تکمیل شده")]
    Completed = 4,

    /// <summary>
    /// لغو شده
    /// </summary>
    [Description("لغو شده")]
    Cancelled = 5,

    /// <summary>
    /// انتقال یافته
    /// </summary>
    [Description("انتقال یافته")]
    Transferred = 6
}

/// <summary>
/// انواع پروتکل‌های تریاژ
/// </summary>
public enum TriageProtocolType
{
    /// <summary>
    /// پروتکل عمومی
    /// </summary>
    [Description("پروتکل عمومی")]
    General = 1,

    /// <summary>
    /// پروتکل قلبی
    /// </summary>
    [Description("پروتکل قلبی")]
    Cardiac = 2,

    /// <summary>
    /// پروتکل تنفسی
    /// </summary>
    [Description("پروتکل تنفسی")]
    Respiratory = 3,

    /// <summary>
    /// پروتکل عصبی
    /// </summary>
    [Description("پروتکل عصبی")]
    Neurological = 4,

    /// <summary>
    /// پروتکل تروما
    /// </summary>
    [Description("پروتکل تروما")]
    Trauma = 5,

    /// <summary>
    /// پروتکل اطفال
    /// </summary>
    [Description("پروتکل اطفال")]
    Pediatric = 6,

    /// <summary>
    /// پروتکل زنان و زایمان
    /// </summary>
    [Description("پروتکل زنان و زایمان")]
    Obstetric = 7,

    /// <summary>
    /// پروتکل روانپزشکی
    /// </summary>
    [Description("پروتکل روانپزشکی")]
    Psychiatric = 8
}

/// <summary>
/// اولویت‌های تریاژ
/// </summary>
public enum TriagePriority
{
    /// <summary>
    /// اولویت 1 - بحرانی
    /// </summary>
    [Description("اولویت 1 - بحرانی")]
    Critical = 1,

    /// <summary>
    /// اولویت 2 - فوری
    /// </summary>
    [Description("اولویت 2 - فوری")]
    Urgent = 2,

    /// <summary>
    /// اولویت 3 - مهم
    /// </summary>
    [Description("اولویت 3 - مهم")]
    Important = 3,

    /// <summary>
    /// اولویت 4 - عادی
    /// </summary>
    [Description("اولویت 4 - عادی")]
    Normal = 4,

    /// <summary>
    /// اولویت 5 - غیرفوری
    /// </summary>
    [Description("اولویت 5 - غیرفوری")]
    NonUrgent = 5
}

/// <summary>
/// انواع علائم حیاتی
/// </summary>
public enum VitalSignType
{
    /// <summary>
    /// فشار خون
    /// </summary>
    [Description("فشار خون")]
    BloodPressure = 1,

    /// <summary>
    /// ضربان قلب
    /// </summary>
    [Description("ضربان قلب")]
    HeartRate = 2,

    /// <summary>
    /// دمای بدن
    /// </summary>
    [Description("دمای بدن")]
    Temperature = 3,

    /// <summary>
    /// تعداد تنفس
    /// </summary>
    [Description("تعداد تنفس")]
    RespiratoryRate = 4,

    /// <summary>
    /// اشباع اکسیژن
    /// </summary>
    [Description("اشباع اکسیژن")]
    OxygenSaturation = 5,

    /// <summary>
    /// سطح هوشیاری
    /// </summary>
    [Description("سطح هوشیاری")]
    ConsciousnessLevel = 6,

    /// <summary>
    /// سطح درد
    /// </summary>
    [Description("سطح درد")]
    PainLevel = 7
}

/// <summary>
/// وضعیت‌های علائم حیاتی
/// </summary>
public enum VitalSignStatus
{
    /// <summary>
    /// طبیعی
    /// </summary>
    [Description("طبیعی")]
    Normal = 1,

    /// <summary>
    /// غیرطبیعی
    /// </summary>
    [Description("غیرطبیعی")]
    Abnormal = 2,

    /// <summary>
    /// بحرانی
    /// </summary>
    [Description("بحرانی")]
    Critical = 3,

    /// <summary>
    /// نامشخص
    /// </summary>
    [Description("نامشخص")]
    Unknown = 4
}

/// <summary>
/// انواع اعلان‌های تریاژ
/// </summary>
public enum TriageNotificationType
{
    /// <summary>
    /// اعلان فوری
    /// </summary>
    [Description("اعلان فوری")]
    Urgent = 1,

    /// <summary>
    /// اعلان عادی
    /// </summary>
    [Description("اعلان عادی")]
    Normal = 2,

    /// <summary>
    /// اعلان هشدار
    /// </summary>
    [Description("اعلان هشدار")]
    Warning = 3,

    /// <summary>
    /// اعلان اطلاعاتی
    /// </summary>
    [Description("اعلان اطلاعاتی")]
    Information = 4
}

/// <summary>
/// انواع گزارش‌های تریاژ
/// </summary>
public enum TriageReportType
{
    /// <summary>
    /// گزارش روزانه
    /// </summary>
    [Description("گزارش روزانه")]
    Daily = 1,

    /// <summary>
    /// گزارش هفتگی
    /// </summary>
    [Description("گزارش هفتگی")]
    Weekly = 2,

    /// <summary>
    /// گزارش ماهانه
    /// </summary>
    [Description("گزارش ماهانه")]
    Monthly = 3,

    /// <summary>
    /// گزارش سالانه
    /// </summary>
    [Description("گزارش سالانه")]
    Yearly = 4,

    /// <summary>
    /// گزارش سفارشی
    /// </summary>
    [Description("گزارش سفارشی")]
    Custom = 5
}

/// <summary>
/// نوع ایزولاسیون
/// </summary>
public enum IsolationType
{
    /// <summary>
    /// بدون ایزولاسیون
    /// </summary>
    [Description("بدون ایزولاسیون")]
    None = 0,

    /// <summary>
    /// ایزولاسیون تماسی
    /// </summary>
    [Description("ایزولاسیون تماسی")]
    Contact = 1,

    /// <summary>
    /// ایزولاسیون قطراتی
    /// </summary>
    [Description("ایزولاسیون قطراتی")]
    Droplet = 2,

    /// <summary>
    /// ایزولاسیون هوایی
    /// </summary>
    [Description("ایزولاسیون هوایی")]
    Airborne = 3
}

/// <summary>
/// نوع دستگاه اکسیژن
/// </summary>
public enum OxygenDevice
{
    /// <summary>
    /// هوای اتاق
    /// </summary>
    [Description("هوای اتاق")]
    RoomAir = 0,

    /// <summary>
    /// کانولای بینی
    /// </summary>
    [Description("کانولای بینی")]
    NasalCannula = 1,

    /// <summary>
    /// ماسک ساده
    /// </summary>
    [Description("ماسک ساده")]
    SimpleMask = 2,

    /// <summary>
    /// ماسک با کیسه ذخیره
    /// </summary>
    [Description("ماسک با کیسه ذخیره")]
    NonRebreatherMask = 3,

    /// <summary>
    /// اکسیژن با فشار بالا
    /// </summary>
    [Description("اکسیژن با فشار بالا")]
    HighFlowNasalCannula = 4,

    /// <summary>
    /// ونتیلاتور
    /// </summary>
    [Description("ونتیلاتور")]
    Ventilator = 5
}

/// <summary>
/// علت ارزیابی مجدد
/// </summary>
public enum ReassessmentReason
{
    /// <summary>
    /// ارزیابی زمان‌بندی شده
    /// </summary>
    [Description("ارزیابی زمان‌بندی شده")]
    Scheduled = 1,

    /// <summary>
    /// بدتر شدن وضعیت
    /// </summary>
    [Description("بدتر شدن وضعیت")]
    Worsened = 2,

    /// <summary>
    /// قبل از عمل
    /// </summary>
    [Description("قبل از عمل")]
    PreProcedure = 3,

    /// <summary>
    /// بعد از عمل
    /// </summary>
    [Description("بعد از عمل")]
    PostProcedure = 4,

    /// <summary>
    /// درخواست پزشک
    /// </summary>
    [Description("درخواست پزشک")]
    DoctorRequest = 5,

    /// <summary>
    /// درخواست پرستار
    /// </summary>
    [Description("درخواست پرستار")]
    NurseRequest = 6,

    /// <summary>
    /// تغییر دارو
    /// </summary>
    [Description("تغییر دارو")]
    MedicationChange = 7,

    /// <summary>
    /// روتین
    /// </summary>
    [Description("روتین")]
    Routine = 8,

    /// <summary>
    /// سایر موارد
    /// </summary>
    [Description("سایر موارد")]
    Other = 9
}
