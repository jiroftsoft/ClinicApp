using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Interfaces;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Services.DataSeeding;

/// <summary>
/// سرویس Seed برای ServiceTemplate
/// بهترین روش برای مدیریت مقادیر پیش‌فرض
/// </summary>
public class ServiceTemplateSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;

    public ServiceTemplateSeedService(
        ApplicationDbContext context, 
        ILogger logger, 
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// ایجاد قالب‌های خدمات مطابق با مصوبه 1404
    /// </summary>
    public async Task SeedServiceTemplatesAsync()
    {
        try
        {
            _logger.Information("شروع ایجاد قالب‌های خدمات مطابق با مصوبه 1404");

            var systemUserId = await GetValidUserIdForSeedAsync();
            var currentTime = DateTime.UtcNow;

            var serviceTemplates = new List<ServiceTemplate>
            {
                // ویزیت‌های پزشک عمومی
                new ServiceTemplate
                {
                    ServiceCode = "970000",
                    ServiceName = "ویزیت پزشک عمومی در مراکز سرپایی",
                    DefaultTechnicalCoefficient = 0.5m,
                    DefaultProfessionalCoefficient = 1.3m,
                    Description = "قالب پیش‌فرض برای ویزیت پزشک عمومی - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970005",
                    ServiceName = "ویزیت دندان‌پزشک عمومی در مراکز سرپایی",
                    DefaultTechnicalCoefficient = 0.5m,
                    DefaultProfessionalCoefficient = 1.3m,
                    Description = "قالب پیش‌فرض برای ویزیت دندان‌پزشک عمومی - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970010",
                    ServiceName = "ویزیت PhD پروانه‌دار در مراکز سرپایی",
                    DefaultTechnicalCoefficient = 0.5m,
                    DefaultProfessionalCoefficient = 1.3m,
                    Description = "قالب پیش‌فرض برای ویزیت PhD پروانه‌دار - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },

                // ویزیت‌های پزشک متخصص
                new ServiceTemplate
                {
                    ServiceCode = "970015",
                    ServiceName = "ویزیت پزشک متخصص در مراکز سرپایی غیرتمام‌وقت",
                    DefaultTechnicalCoefficient = 0.7m,
                    DefaultProfessionalCoefficient = 1.8m,
                    Description = "قالب پیش‌فرض برای ویزیت پزشک متخصص غیرتمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970016",
                    ServiceName = "ویزیت پزشک متخصص در مراکز سرپایی تمام‌وقت",
                    DefaultTechnicalCoefficient = 0.7m,
                    DefaultProfessionalCoefficient = 1.8m,
                    Description = "قالب پیش‌فرض برای ویزیت پزشک متخصص تمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },

                // ویزیت‌های دندان‌پزشک متخصص
                new ServiceTemplate
                {
                    ServiceCode = "970020",
                    ServiceName = "ویزیت دندان‌پزشک متخصص در مراکز سرپایی غیرتمام‌وقت",
                    DefaultTechnicalCoefficient = 0.7m,
                    DefaultProfessionalCoefficient = 1.8m,
                    Description = "قالب پیش‌فرض برای ویزیت دندان‌پزشک متخصص غیرتمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970021",
                    ServiceName = "ویزیت دندان‌پزشک متخصص در مراکز سرپایی تمام‌وقت",
                    DefaultTechnicalCoefficient = 0.7m,
                    DefaultProfessionalCoefficient = 1.8m,
                    Description = "قالب پیش‌فرض برای ویزیت دندان‌پزشک متخصص تمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },

                // ویزیت‌های MD-PhD
                new ServiceTemplate
                {
                    ServiceCode = "970025",
                    ServiceName = "ویزیت MD-PhD در مراکز سرپایی غیرتمام‌وقت",
                    DefaultTechnicalCoefficient = 0.7m,
                    DefaultProfessionalCoefficient = 1.8m,
                    Description = "قالب پیش‌فرض برای ویزیت MD-PhD غیرتمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970026",
                    ServiceName = "ویزیت MD-PhD در مراکز سرپایی تمام‌وقت",
                    DefaultTechnicalCoefficient = 0.7m,
                    DefaultProfessionalCoefficient = 1.8m,
                    Description = "قالب پیش‌فرض برای ویزیت MD-PhD تمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },

                // ویزیت‌های فوق تخصص
                new ServiceTemplate
                {
                    ServiceCode = "970030",
                    ServiceName = "ویزیت پزشک فوق تخصص در مراکز سرپایی غیرتمام‌وقت",
                    DefaultTechnicalCoefficient = 0.8m,
                    DefaultProfessionalCoefficient = 2.3m,
                    Description = "قالب پیش‌فرض برای ویزیت پزشک فوق تخصص غیرتمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970031",
                    ServiceName = "ویزیت پزشک فوق تخصص در مراکز سرپایی تمام‌وقت",
                    DefaultTechnicalCoefficient = 0.8m,
                    DefaultProfessionalCoefficient = 2.3m,
                    Description = "قالب پیش‌فرض برای ویزیت پزشک فوق تخصص تمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },

                // ویزیت‌های فلوشیپ
                new ServiceTemplate
                {
                    ServiceCode = "970035",
                    ServiceName = "ویزیت پزشک فلوشیپ در مراکز سرپایی غیرتمام‌وقت",
                    DefaultTechnicalCoefficient = 0.8m,
                    DefaultProfessionalCoefficient = 2.3m,
                    Description = "قالب پیش‌فرض برای ویزیت پزشک فلوشیپ غیرتمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970036",
                    ServiceName = "ویزیت پزشک فلوشیپ در مراکز سرپایی تمام‌وقت",
                    DefaultTechnicalCoefficient = 0.8m,
                    DefaultProfessionalCoefficient = 2.3m,
                    Description = "قالب پیش‌فرض برای ویزیت پزشک فلوشیپ تمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },

                // ویزیت‌های روانپزشکی
                new ServiceTemplate
                {
                    ServiceCode = "970040",
                    ServiceName = "ویزیت متخصص روانپزشکی در مراکز سرپایی غیرتمام‌وقت",
                    DefaultTechnicalCoefficient = 0.8m,
                    DefaultProfessionalCoefficient = 2.3m,
                    Description = "قالب پیش‌فرض برای ویزیت متخصص روانپزشکی غیرتمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970041",
                    ServiceName = "ویزیت متخصص روانپزشکی در مراکز سرپایی تمام‌وقت",
                    DefaultTechnicalCoefficient = 0.8m,
                    DefaultProfessionalCoefficient = 2.3m,
                    Description = "قالب پیش‌فرض برای ویزیت متخصص روانپزشکی تمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },

                // ویزیت‌های فوق تخصص روانپزشکی
                new ServiceTemplate
                {
                    ServiceCode = "970045",
                    ServiceName = "ویزیت فوق تخصص روانپزشکی در مراکز سرپایی غیرتمام‌وقت",
                    DefaultTechnicalCoefficient = 0.9m,
                    DefaultProfessionalCoefficient = 2.7m,
                    Description = "قالب پیش‌فرض برای ویزیت فوق تخصص روانپزشکی غیرتمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970046",
                    ServiceName = "ویزیت فوق تخصص روانپزشکی در مراکز سرپایی تمام‌وقت",
                    DefaultTechnicalCoefficient = 0.9m,
                    DefaultProfessionalCoefficient = 2.7m,
                    Description = "قالب پیش‌فرض برای ویزیت فوق تخصص روانپزشکی تمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },

                // کارشناسان
                new ServiceTemplate
                {
                    ServiceCode = "970050",
                    ServiceName = "کارشناس ارشد پروانه‌دار در مراکز سرپایی",
                    DefaultTechnicalCoefficient = 0.4m,
                    DefaultProfessionalCoefficient = 1.1m,
                    Description = "قالب پیش‌فرض برای کارشناس ارشد پروانه‌دار - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970055",
                    ServiceName = "کارشناس پروانه‌دار در مراکز سرپایی",
                    DefaultTechnicalCoefficient = 0.35m,
                    DefaultProfessionalCoefficient = 0.9m,
                    Description = "قالب پیش‌فرض برای کارشناس پروانه‌دار - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },

                // فلوشیپ روانپزشکی
                new ServiceTemplate
                {
                    ServiceCode = "970090",
                    ServiceName = "ویزیت فلوشیپ روانپزشکی در مراکز سرپایی غیرتمام‌وقت",
                    DefaultTechnicalCoefficient = 0.9m,
                    DefaultProfessionalCoefficient = 2.7m,
                    Description = "قالب پیش‌فرض برای ویزیت فلوشیپ روانپزشکی غیرتمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970091",
                    ServiceName = "ویزیت فلوشیپ روانپزشکی در مراکز سرپایی تمام‌وقت",
                    DefaultTechnicalCoefficient = 0.9m,
                    DefaultProfessionalCoefficient = 2.7m,
                    Description = "قالب پیش‌فرض برای ویزیت فلوشیپ روانپزشکی تمام‌وقت - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },

                // خدمات روانشناسی
                new ServiceTemplate
                {
                    ServiceCode = "970096",
                    ServiceName = "خدمات روانشناسی و مشاوره توسط کارشناسان ارشد پروانه‌دار",
                    DefaultTechnicalCoefficient = 0.90m,
                    DefaultProfessionalCoefficient = 3.5m,
                    Description = "قالب پیش‌فرض برای خدمات روانشناسی کارشناس ارشد - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970097",
                    ServiceName = "خدمات روانشناسی و مشاوره توسط دکترای تخصصی پروانه‌دار",
                    DefaultTechnicalCoefficient = 1.20m,
                    DefaultProfessionalCoefficient = 4.0m,
                    Description = "قالب پیش‌فرض برای خدمات روانشناسی دکترای تخصصی - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "970098",
                    ServiceName = "خدمات روانشناسی و مشاوره برای سابقه بیش از پانزده سال کار بالینی",
                    DefaultTechnicalCoefficient = 0.20m,
                    DefaultProfessionalCoefficient = 0.4m,
                    Description = "قالب پیش‌فرض برای خدمات روانشناسی سابقه بیش از 15 سال - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },

                // ارزیابی کودکان
                new ServiceTemplate
                {
                    ServiceCode = "978000",
                    ServiceName = "ارزیابی و معاینه (ویزیت) سرپایی افراد با سن کمتر از 10 سال تمام",
                    DefaultTechnicalCoefficient = 0.15m,
                    DefaultProfessionalCoefficient = 0.5m,
                    Description = "قالب پیش‌فرض برای ارزیابی کودکان کمتر از 10 سال - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "978001",
                    ServiceName = "ارزیابی و معاینه (ویزیت) سرپایی افراد با سن کمتر از 7 سال تمام",
                    DefaultTechnicalCoefficient = 0.15m,
                    DefaultProfessionalCoefficient = 0.5m,
                    Description = "قالب پیش‌فرض برای ارزیابی کودکان کمتر از 7 سال - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                },
                new ServiceTemplate
                {
                    ServiceCode = "978005",
                    ServiceName = "پزشکان عمومی با سابقه بیش از پانزده سال کار بالینی",
                    DefaultTechnicalCoefficient = 0.00m,
                    DefaultProfessionalCoefficient = 0.4m,
                    Description = "قالب پیش‌فرض برای پزشکان عمومی سابقه بیش از 15 سال - مصوبه 1404",
                    IsActive = true,
                    CreatedAt = currentTime,
                    CreatedByUserId = systemUserId
                }
            };

            // بررسی وجود قالب‌ها
            var existingCodes = await _context.ServiceTemplates
                .Where(st => !st.IsDeleted)
                .Select(st => st.ServiceCode)
                .ToListAsync();

            var newTemplates = serviceTemplates
                .Where(st => !existingCodes.Contains(st.ServiceCode))
                .ToList();

            if (newTemplates.Any())
            {
                _context.ServiceTemplates.AddRange(newTemplates);
                _logger.Information("📍 TEMPLATE_SEED: تعداد {Count} قالب خدمت جدید به دیتابیس اضافه شد", newTemplates.Count);
            }
            else
            {
                _logger.Information("✅ TEMPLATE_SEED: همه قالب‌های خدمات قبلاً ایجاد شده‌اند");
            }

            // حذف SaveChangesAsync - انجام می‌شود در SystemSeedService
            _logger.Information("✅ TEMPLATE_SEED: قالب‌های خدمات آماده ذخیره‌سازی");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "❌ TEMPLATE_SEED: خطا در ایجاد قالب‌های خدمات");
            throw;
        }
    }

    /// <summary>
    /// دریافت کاربر معتبر برای Seed
    /// </summary>
    private async Task<string> GetValidUserIdForSeedAsync()
    {
        try
        {
            // تلاش برای دریافت کاربر فعلی
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (!string.IsNullOrEmpty(currentUserId))
            {
                return currentUserId;
            }

            // در صورت عدم وجود کاربر فعلی، از کاربر Admin استفاده می‌کنیم
            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == "Admin" && !u.IsDeleted);
            
            if (adminUser != null)
            {
                return adminUser.Id;
            }

            // در صورت عدم وجود Admin، از کاربر System استفاده می‌کنیم
            var systemUser = await _context.Users
                .FirstOrDefaultAsync(u => (u.UserName == "3031945451" || u.UserName == "system") && !u.IsDeleted);
            
            if (systemUser != null)
            {
                return systemUser.Id;
            }

            // در صورت عدم وجود هیچ کاربری، از SystemUsers استفاده می‌کنیم
            if (SystemUsers.IsInitialized && !string.IsNullOrEmpty(SystemUsers.SystemUserId))
            {
                return SystemUsers.SystemUserId;
            }

            // در صورت عدم وجود هیچ کاربری، از شناسه پیش‌فرض استفاده می‌کنیم
            return "6f999f4d-24b8-4142-a97e-20077850278b";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت کاربر معتبر برای Seed");
            // در صورت خطا، از شناسه پیش‌فرض استفاده می‌کنیم
            return "6f999f4d-24b8-4142-a97e-20077850278b";
        }
    }
}
