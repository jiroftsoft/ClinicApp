using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Clinic;
using Serilog;

namespace ClinicApp.Services;

/// <summary>
/// سرویس مدیریت قالب‌های خدمات
/// بهترین روش برای مدیریت مقادیر پیش‌فرض اجزای خدمات
/// </summary>
public class ServiceTemplateService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger _logger;

    public ServiceTemplateService(ApplicationDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// دریافت قالب خدمت بر اساس کد
    /// </summary>
    public async Task<ServiceTemplate> GetTemplateByCodeAsync(string serviceCode)
    {
        try
        {
            return await _context.ServiceTemplates
                .FirstOrDefaultAsync(st => st.ServiceCode == serviceCode && 
                                          st.IsActive && 
                                          !st.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت قالب خدمت برای کد: {ServiceCode}", serviceCode);
            return null;
        }
    }

    /// <summary>
    /// دریافت قالب خدمت بر اساس کد (نام متد مطابق با Controller)
    /// </summary>
    public async Task<ServiceTemplate> GetByServiceCodeAsync(string serviceCode)
    {
        return await GetTemplateByCodeAsync(serviceCode);
    }

    /// <summary>
    /// دریافت تمام قالب‌های فعال
    /// </summary>
    public async Task<List<ServiceTemplate>> GetActiveTemplatesAsync()
    {
        try
        {
            return await _context.ServiceTemplates
                .Where(st => st.IsActive && !st.IsDeleted)
                .OrderBy(st => st.ServiceCode)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت قالب‌های فعال");
            return new List<ServiceTemplate>();
        }
    }

    /// <summary>
    /// دریافت ضریب فنی پیش‌فرض
    /// </summary>
    public async Task<decimal> GetDefaultTechnicalCoefficientAsync(string serviceCode)
    {
        var template = await GetTemplateByCodeAsync(serviceCode);
        return template?.DefaultTechnicalCoefficient ?? 1.0m;
    }

    /// <summary>
    /// دریافت ضریب حرفه‌ای پیش‌فرض
    /// </summary>
    public async Task<decimal> GetDefaultProfessionalCoefficientAsync(string serviceCode)
    {
        var template = await GetTemplateByCodeAsync(serviceCode);
        return template?.DefaultProfessionalCoefficient ?? 1.0m;
    }

    /// <summary>
    /// دریافت هر دو ضریب به صورت همزمان
    /// </summary>
    public async Task<(decimal Technical, decimal Professional)> GetDefaultCoefficientsAsync(string serviceCode)
    {
        var template = await GetTemplateByCodeAsync(serviceCode);
        if (template != null)
        {
            return (template.DefaultTechnicalCoefficient, template.DefaultProfessionalCoefficient);
        }
        
        return (1.0m, 1.0m); // مقادیر پیش‌فرض
    }

    /// <summary>
    /// بررسی وجود قالب برای کد خدمت
    /// </summary>
    public async Task<bool> TemplateExistsAsync(string serviceCode)
    {
        try
        {
            return await _context.ServiceTemplates
                .AnyAsync(st => st.ServiceCode == serviceCode && 
                               st.IsActive && 
                               !st.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در بررسی وجود قالب برای کد: {ServiceCode}", serviceCode);
            return false;
        }
    }

    /// <summary>
    /// ایجاد قالب جدید
    /// </summary>
    public async Task<bool> CreateTemplateAsync(ServiceTemplate template)
    {
        try
        {
            _context.ServiceTemplates.Add(template);
            await _context.SaveChangesAsync();
            _logger.Information("قالب خدمت جدید ایجاد شد: {ServiceCode}", template.ServiceCode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در ایجاد قالب خدمت: {ServiceCode}", template.ServiceCode);
            return false;
        }
    }

    /// <summary>
    /// به‌روزرسانی قالب
    /// </summary>
    public async Task<bool> UpdateTemplateAsync(ServiceTemplate template)
    {
        try
        {
            template.UpdatedAt = DateTime.UtcNow;
            _context.Entry(template).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.Information("قالب خدمت به‌روزرسانی شد: {ServiceCode}", template.ServiceCode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در به‌روزرسانی قالب خدمت: {ServiceCode}", template.ServiceCode);
            return false;
        }
    }
}
