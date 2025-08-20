namespace ClinicApp.Interfaces;

/// <summary>
/// سرویس تبدیل ایمن برای سیستم‌های پزشکی
/// این سرویس از خطاها در حین تبدیل جلوگیری می‌کند
/// </summary>
public interface ISafeMapper
{
    /// <summary>
    /// تبدیل ایمن مدل منبع به مدل مقصد
    /// </summary>
    TDestination Map<TSource, TDestination>(TSource source) where TDestination : class;

    /// <summary>
    /// تبدیل ایمن مدل منبع به مدل مقصد با بررسی null
    /// </summary>
    TDestination MapSafe<TSource, TDestination>(TSource source) where TDestination : class;
}