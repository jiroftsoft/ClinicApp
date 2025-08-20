using System;
using AutoMapper;
using ClinicApp.Interfaces;
using Serilog;

namespace ClinicApp.Services;

/// <summary>
/// پیاده‌سازی سرویس تبدیل ایمن برای سیستم‌های پزشکی
/// </summary>
public class SafeMapper : ISafeMapper
{
    private readonly IMapper _mapper;
    private readonly ILogger _log;

    /// <summary>
    /// سازنده سرویس تبدیل ایمن
    /// </summary>
    public SafeMapper(IMapper mapper, ILogger logger)
    {
        _mapper = mapper;
        _log = logger.ForContext<SafeMapper>();
    }

    /// <summary>
    /// تبدیل ایمن مدل منبع به مدل مقصد
    /// </summary>
    public TDestination Map<TSource, TDestination>(TSource source) where TDestination : class
    {
        try
        {
            return _mapper.Map<TDestination>(source);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در تبدیل مدل از {SourceType} به {DestinationType}",
                typeof(TSource).Name,
                typeof(TDestination).Name);

            throw new InvalidOperationException(
                $"خطا در تبدیل مدل از {typeof(TSource).Name} به {typeof(TDestination).Name}. لطفاً با پشتیبانی تماس بگیرید.");
        }
    }

    /// <summary>
    /// تبدیل ایمن مدل منبع به مدل مقصد با بررسی null
    /// </summary>
    public TDestination MapSafe<TSource, TDestination>(TSource source) where TDestination : class
    {
        if (source == null)
            return null;

        try
        {
            return _mapper.Map<TDestination>(source);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در تبدیل مدل ایمن از {SourceType} به {DestinationType}",
                typeof(TSource).Name,
                typeof(TDestination).Name);

            return null;
        }
    }
}