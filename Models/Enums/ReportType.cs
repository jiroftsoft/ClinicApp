using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// نوع گزارش‌ها در سامانه کلینیک
/// </summary>
public enum ReportType : byte
{
    // گزارش‌های عمومی
    [Display(Name = "گزارش مالی")]
    Financial = 0,

    [Display(Name = "گزارش بیماران")]
    Patients = 1,

    [Display(Name = "گزارش نوبت‌ها")]
    Appointments = 2,

    [Display(Name = "گزارش پذیرش")]
    Receptions = 3,

    [Display(Name = "گزارش پزشکان")]
    Doctors = 4,

    [Display(Name = "گزارش بیمه")]
    Insurance = 5,

    [Display(Name = "گزارش عملکرد کلی")]
    Performance = 6,

    // گزارش‌های زمانی
    [Display(Name = "گزارش روزانه")]
    Daily = 10,

    [Display(Name = "گزارش هفتگی")]
    Weekly = 11,

    [Display(Name = "گزارش ماهانه")]
    Monthly = 12,

    [Display(Name = "گزارش سالانه")]
    Yearly = 13,

    [Display(Name = "گزارش بازه زمانی")]
    DateRange = 14,

    // گزارش‌های پرداخت
    [Display(Name = "گزارش پرداخت‌های روزانه")]
    DailyPayment = 20,

    [Display(Name = "گزارش پرداخت‌های هفتگی")]
    WeeklyPayment = 21,

    [Display(Name = "گزارش پرداخت‌های ماهانه")]
    MonthlyPayment = 22,

    [Display(Name = "گزارش پرداخت‌های سالانه")]
    YearlyPayment = 23,

    [Display(Name = "گزارش بازه‌ای پرداخت‌ها")]
    DateRangePayment = 24,

    [Display(Name = "آمار پرداخت‌ها")]
    PaymentStatistics = 25,

    [Display(Name = "آمار روش‌های پرداخت")]
    PaymentMethodStatistics = 26,

    [Display(Name = "آمار وضعیت پرداخت‌ها")]
    PaymentStatusStatistics = 27,

    [Display(Name = "آمار پرداخت کاربران")]
    UserPaymentStatistics = 28,

    [Display(Name = "پرداخت‌های سفارشی")]
    CustomPayment = 29,

    [Display(Name = "پرداخت‌های مقایسه‌ای")]
    ComparativePayment = 30,

    [Display(Name = "روند پرداخت‌ها")]
    TrendPayment = 31,

    // گزارش‌های مالی و عملکردی
    [Display(Name = "گزارش مالی روزانه")]
    DailyFinancial = 40,

    [Display(Name = "گزارش مالی ماهانه")]
    MonthlyFinancial = 41,

    [Display(Name = "درآمد و هزینه‌ها")]
    IncomeExpense = 42,

    [Display(Name = "کارمزد درگاه پرداخت")]
    GatewayFee = 43,

    [Display(Name = "عملکرد درگاه‌ها")]
    GatewayPerformance = 44,

    [Display(Name = "عملکرد پایانه‌های کارت‌خوان")]
    PosTerminalPerformance = 45,

    [Display(Name = "عملکرد صندوق نقدی")]
    CashSessionPerformance = 46,

    [Display(Name = "زمان پاسخ‌گویی سیستم")]
    ResponseTime = 47
}
