using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Enums;
using ClinicApp.Interfaces;
using ClinicApp.Helpers;
using ClinicApp.Services;
using ClinicApp.DataSeeding;
using Serilog;
namespace ClinicApp.Services.DataSeeding
{
    /// <summary>
    /// سرویس ایجاد داده‌های اولیه برای خدمات
    /// </summary>
    public class ServiceSeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IServiceCalculationService _serviceCalculationService;
        private readonly StructuredLogger _structuredLogger;

        public ServiceSeedService(
            ApplicationDbContext context, 
            ILogger logger,
            ICurrentUserService currentUserService,
            IServiceCalculationService serviceCalculationService)
        {
            _context = context;
            _logger = logger;
            _currentUserService = currentUserService;
            _serviceCalculationService = serviceCalculationService;
            _structuredLogger = new StructuredLogger("ServiceSeedService");
        }

        /// <summary>
        /// ایجاد خدمات نمونه با اجزای فنی و حرفه‌ای
        /// </summary>
        public async Task SeedSampleServicesAsync()
        {
            try
            {
                _logger.Information("شروع ایجاد خدمات نمونه");

                // استفاده از StructuredLogger
                _structuredLogger.LogOperation("SeedSampleServices", new { UserId = _currentUserService.UserId });

                // دریافت دسته‌بندی خدمات
                var serviceCategories = await _context.ServiceCategories
                    .Where(sc => !sc.IsDeleted)
                    .ToListAsync();

                if (!serviceCategories.Any())
                {
                    _logger.Warning("هیچ دسته‌بندی خدماتی یافت نشد. ابتدا دسته‌بندی‌ها را ایجاد کنید.");
                    return;
                }

                var sampleServices = new List<Service>
                {
                    // ویزیت پزشک عمومی - کد 970000 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت پزشک عمومی در مراکز سرپایی",
                        ServiceCode = "970000",
                        Description = "ویزیت پزشک عمومی در مراکز سرپایی - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت دندان‌پزشک عمومی - کد 970005 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت دندان‌پزشک عمومی در مراکز سرپایی",
                        ServiceCode = "970005",
                        Description = "ویزیت دندان‌پزشک عمومی در مراکز سرپایی - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت PhD پروانه‌دار - کد 970010 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت PhD پروانه‌دار در مراکز سرپایی",
                        ServiceCode = "970010",
                        Description = "ویزیت PhD پروانه‌دار در مراکز سرپایی - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت پزشک متخصص غیرتمام‌وقت - کد 970015 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت پزشک متخصص در مراکز سرپایی غیرتمام‌وقت",
                        ServiceCode = "970015",
                        Description = "ویزیت پزشک متخصص در مراکز سرپایی غیرتمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت پزشک متخصص تمام‌وقت - کد 970016 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت پزشک متخصص در مراکز سرپایی تمام‌وقت",
                        ServiceCode = "970016",
                        Description = "ویزیت پزشک متخصص در مراکز سرپایی تمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت دندان‌پزشک متخصص غیرتمام‌وقت - کد 970020 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت دندان‌پزشک متخصص در مراکز سرپایی غیرتمام‌وقت",
                        ServiceCode = "970020",
                        Description = "ویزیت دندان‌پزشک متخصص در مراکز سرپایی غیرتمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت دندان‌پزشک متخصص تمام‌وقت - کد 970021 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت دندان‌پزشک متخصص در مراکز سرپایی تمام‌وقت",
                        ServiceCode = "970021",
                        Description = "ویزیت دندان‌پزشک متخصص در مراکز سرپایی تمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت MD-PhD غیرتمام‌وقت - کد 970025 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت MD-PhD در مراکز سرپایی غیرتمام‌وقت",
                        ServiceCode = "970025",
                        Description = "ویزیت MD-PhD در مراکز سرپایی غیرتمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت MD-PhD تمام‌وقت - کد 970026 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت MD-PhD در مراکز سرپایی تمام‌وقت",
                        ServiceCode = "970026",
                        Description = "ویزیت MD-PhD در مراکز سرپایی تمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت پزشک فوق تخصص غیرتمام‌وقت - کد 970030 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت پزشک فوق تخصص در مراکز سرپایی غیرتمام‌وقت",
                        ServiceCode = "970030",
                        Description = "ویزیت پزشک فوق تخصص در مراکز سرپایی غیرتمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت پزشک فوق تخصص تمام‌وقت - کد 970031 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت پزشک فوق تخصص در مراکز سرپایی تمام‌وقت",
                        ServiceCode = "970031",
                        Description = "ویزیت پزشک فوق تخصص در مراکز سرپایی تمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت پزشک فلوشیپ غیرتمام‌وقت - کد 970035 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت پزشک فلوشیپ در مراکز سرپایی غیرتمام‌وقت",
                        ServiceCode = "970035",
                        Description = "ویزیت پزشک فلوشیپ در مراکز سرپایی غیرتمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت پزشک فلوشیپ تمام‌وقت - کد 970036 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت پزشک فلوشیپ در مراکز سرپایی تمام‌وقت",
                        ServiceCode = "970036",
                        Description = "ویزیت پزشک فلوشیپ در مراکز سرپایی تمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت متخصص روانپزشکی غیرتمام‌وقت - کد 970040 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت متخصص روانپزشکی در مراکز سرپایی غیرتمام‌وقت",
                        ServiceCode = "970040",
                        Description = "ویزیت متخصص روانپزشکی در مراکز سرپایی غیرتمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت متخصص روانپزشکی تمام‌وقت - کد 970041 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت متخصص روانپزشکی در مراکز سرپایی تمام‌وقت",
                        ServiceCode = "970041",
                        Description = "ویزیت متخصص روانپزشکی در مراکز سرپایی تمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت فوق تخصص روانپزشکی غیرتمام‌وقت - کد 970045 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت فوق تخصص روانپزشکی در مراکز سرپایی غیرتمام‌وقت",
                        ServiceCode = "970045",
                        Description = "ویزیت فوق تخصص روانپزشکی در مراکز سرپایی غیرتمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت فوق تخصص روانپزشکی تمام‌وقت - کد 970046 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت فوق تخصص روانپزشکی در مراکز سرپایی تمام‌وقت",
                        ServiceCode = "970046",
                        Description = "ویزیت فوق تخصص روانپزشکی در مراکز سرپایی تمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت کارشناس ارشد پروانه‌دار - کد 970050 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "کارشناس ارشد پروانه‌دار در مراکز سرپایی",
                        ServiceCode = "970050",
                        Description = "کارشناس ارشد پروانه‌دار در مراکز سرپایی - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت کارشناس پروانه‌دار - کد 970055 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "کارشناس پروانه‌دار در مراکز سرپایی",
                        ServiceCode = "970055",
                        Description = "کارشناس پروانه‌دار در مراکز سرپایی - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت فلوشیپ روانپزشکی غیرتمام‌وقت - کد 970090 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت فلوشیپ روانپزشکی در مراکز سرپایی غیرتمام‌وقت",
                        ServiceCode = "970090",
                        Description = "ویزیت فلوشیپ روانپزشکی در مراکز سرپایی غیرتمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت فلوشیپ روانپزشکی تمام‌وقت - کد 970091 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ویزیت فلوشیپ روانپزشکی در مراکز سرپایی تمام‌وقت",
                        ServiceCode = "970091",
                        Description = "ویزیت فلوشیپ روانپزشکی در مراکز سرپایی تمام‌وقت - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // خدمات روانشناسی و مشاوره کارشناسان ارشد - کد 970096 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "خدمات روانشناسی و مشاوره توسط کارشناسان ارشد پروانه‌دار",
                        ServiceCode = "970096",
                        Description = "خدمات روانشناسی و مشاوره توسط کارشناسان ارشد پروانه‌دار - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #*
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // خدمات روانشناسی و مشاوره دکترای تخصصی - کد 970097 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "خدمات روانشناسی و مشاوره توسط دکترای تخصصی پروانه‌دار",
                        ServiceCode = "970097",
                        Description = "خدمات روانشناسی و مشاوره توسط دکترای تخصصی پروانه‌دار - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: #*
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // خدمات روانشناسی و مشاوره با سابقه - کد 970098 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "خدمات روانشناسی و مشاوره برای سابقه بیش از پانزده سال کار بالینی",
                        ServiceCode = "970098",
                        Description = "خدمات روانشناسی و مشاوره برای سابقه بیش از پانزده سال کار بالینی - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: +#*
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت کودکان زیر ۱۰ سال - کد 978000 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ارزیابی و معاینه (ویزیت) سرپایی افراد با سن کمتر از 10 سال تمام صرفا برای گروه تخصصی، دوره تکمیلی تخصصی (فلوشیپ) و فوق تخصص کودکان و نوزادان",
                        ServiceCode = "978000",
                        Description = "ارزیابی و معاینه (ویزیت) سرپایی افراد با سن کمتر از 10 سال تمام صرفا برای گروه تخصصی، دوره تکمیلی تخصصی (فلوشیپ) و فوق تخصص کودکان و نوزادان - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: +#
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت کودکان زیر ۷ سال - کد 978001 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "ارزیابی و معاینه (ویزیت) سرپایی افراد با سن کمتر از 7 سال تمام، برای سایر گروه‌های تخصصی، دوره تکمیلی تخصصی (فلوشیپ) و فوق تخصص",
                        ServiceCode = "978001",
                        Description = "ارزیابی و معاینه (ویزیت) سرپایی افراد با سن کمتر از 7 سال تمام، برای سایر گروه‌های تخصصی، دوره تکمیلی تخصصی (فلوشیپ) و فوق تخصص - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: +#
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    },

                    // ویزیت پزشک عمومی با سابقه - کد 978005 (جدول دقیق مصوبه 1404)
                    new Service
                    {
                        Title = "پزشکان عمومی با سابقه بیش از پانزده سال کار بالینی",
                        ServiceCode = "978005",
                        Description = "پزشکان عمومی با سابقه بیش از پانزده سال کار بالینی - مصوبه 1404",
                        // TechnicalPart و ProfessionalPart حذف شدند - استفاده از ServiceComponents
                        IsHashtagged = true, // ویژگی کد: +#
                        Price = 0, // قیمت پایه (محاسبه خواهد شد)
                        IsActive = true, // فعال کردن خدمت
                        ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
                    }
                };

                // دریافت کاربر معتبر برای Seed
                var systemUserId = await GetValidUserIdForSeedAsync();

                foreach (var service in sampleServices)
                {
                    // بررسی وجود خدمت با همین کد
                    var existingService = await _context.Services
                        .FirstOrDefaultAsync(s => s.ServiceCode == service.ServiceCode && !s.IsDeleted);

                    if (existingService == null)
                    {
                        service.CreatedAt = DateTime.UtcNow;
                        service.CreatedByUserId = systemUserId;
                        _context.Services.Add(service);
                    }
                }

                // حذف SaveChangesAsync - انجام می‌شود در SystemSeedService
                _logger.Information("✅ SERVICE_SEED: خدمات نمونه آماده ذخیره‌سازی");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SERVICE_SEED: خطا در ایجاد خدمات نمونه");
                throw;
            }
        }

        /// <summary>
        /// ایجاد خدمات مشترک بین دپارتمان‌ها - نسخه ضدگلوله و حرفه‌ای
        /// این متد خدمات را برای تمام دپارتمان‌های فعال ایجاد می‌کند
        /// </summary>
        public async Task SeedSharedServicesAsync()
        {
            try
            {
                _logger.Information("═══════════════════════════════════════════════");
                _logger.Information("🔗 SHARED_SERVICE: شروع ایجاد خدمات مشترک");
                _logger.Information("═══════════════════════════════════════════════");

                // 🚀 استفاده از LoggingHelper برای لاگ‌گیری حرفه‌ای
                LoggingHelper.LogSeedOperation("SharedServices_Start", 0, true, "شروع فرآیند ایجاد خدمات مشترک");

                // 📊 استفاده از StructuredLogger برای لاگ‌گیری ساختاریافته
                var structuredLogger = new StructuredLogger("ServiceSeedService");
                structuredLogger.LogOperation("SeedSharedServices", new { 
                    StartTime = DateTime.UtcNow,
                    Environment = "Development"
                });

                // مرحله 1: ابتدا Services را ذخیره کنیم تا ServiceId معتبر شود
                var localServices = _context.Services.Local
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .ToList();

                _logger.Information("📊 SHARED_SERVICE: خدمات در Context.Local: {Count}", localServices.Count);

                // اگر Services در Context.Local هستند، ابتدا آنها را ذخیره کنیم
                if (localServices.Any())
                {
                    _logger.Information("💾 SHARED_SERVICE: ذخیره Services برای دریافت ServiceId معتبر...");
                    await _context.SaveChangesAsync();
                    _logger.Information("✅ SHARED_SERVICE: Services ذخیره شدند");
                }

                // مرحله 2: دریافت Services از Database (حالا با ServiceId معتبر)
                var services = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .ToListAsync();

                _logger.Information("📊 SHARED_SERVICE: خدمات در دیتابیس: {Count}", services.Count);

                // مرحله 3: دریافت دپارتمان‌های فعال
                var departments = await _context.Departments
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .ToListAsync();

                _logger.Information("📊 SHARED_SERVICE: دپارتمان‌های فعال: {Count}", departments.Count);

                // مرحله 4: اعتبارسنجی اولیه
                if (!services.Any())
                {
                    _logger.Warning("⚠️ SHARED_SERVICE: هیچ خدمتی یافت نشد. ابتدا خدمات را ایجاد کنید.");
                    return;
                }

                if (!departments.Any())
                {
                    _logger.Warning("⚠️ SHARED_SERVICE: هیچ دپارتمانی یافت نشد. ابتدا دپارتمان‌ها را ایجاد کنید.");
                    return;
                }

                // مرحله 5: دریافت کاربر معتبر برای Seed
                var systemUserId = await GetValidUserIdForSeedAsync();
                _logger.Information("👤 SHARED_SERVICE: کاربر سیستم: {UserId}", systemUserId);

                // مرحله 6: ایجاد خدمات مشترک
                var addedCount = 0;
                var skippedCount = 0;
                var errorCount = 0;

                _logger.Information("🔄 SHARED_SERVICE: شروع ایجاد خدمات مشترک...");

                foreach (var service in services)
                {
                    try
                    {
                        _logger.Information("🔍 SHARED_SERVICE: پردازش خدمت {ServiceCode} - {Title}", 
                            service.ServiceCode, service.Title);

                        foreach (var department in departments)
                        {
                            try
                            {
                                // بررسی وجود سرویس مشترک (از Database - Context.Local ممکن است ناقص باشد)
                                var existingShared = await _context.SharedServices
                                    .FirstOrDefaultAsync(ss => ss.ServiceId == service.ServiceId
                                                            && ss.DepartmentId == department.DepartmentId
                                                            && !ss.IsDeleted);

                                if (existingShared != null)
                                {
                                    _logger.Debug("⏭️ SHARED_SERVICE: سرویس مشترک موجود - {ServiceCode} در {DepartmentName}",
                                        service.ServiceCode, department.Name);
                                    skippedCount++;
                                    continue;
                                }

                                // بررسی وجود در Context.Local (برای جلوگیری از Duplicate Key)
                                var localExisting = _context.SharedServices.Local
                                    .FirstOrDefault(ss => ss.ServiceId == service.ServiceId
                                                      && ss.DepartmentId == department.DepartmentId
                                                      && !ss.IsDeleted);

                                if (localExisting != null)
                                {
                                    _logger.Debug("⏭️ SHARED_SERVICE: سرویس مشترک در Context.Local موجود - {ServiceCode} در {DepartmentName}",
                                        service.ServiceCode, department.Name);
                                    skippedCount++;
                                    continue;
                                }

                                // ایجاد سرویس مشترک جدید با استفاده از Navigation Property
                                var sharedService = new SharedService
                                {
                                    Service = service,                                // ✅ Navigation Property
                                    DepartmentId = department.DepartmentId,           // ✅ DepartmentId تنظیم شده
                                    IsActive = true,
                                    DepartmentSpecificNotes = $"{service.Title} در دپارتمان {department.Name}",
                                    // تنظیم Override Factors (اختیاری - برای Override کردن کای‌های پیش‌فرض)
                                    OverrideTechnicalFactor = null,                   // استفاده از کای فنی پیش‌فرض
                                    OverrideProfessionalFactor = null,               // استفاده از کای حرفه‌ای پیش‌فرض
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedByUserId = systemUserId
                                };

                                // اضافه کردن از طریق Navigation Property (ایمن‌تر)
                                service.SharedServices.Add(sharedService);
                                addedCount++;

                                _logger.Debug("✅ SHARED_SERVICE: ایجاد شد - {ServiceCode} در {DepartmentName}",
                                    service.ServiceCode, department.Name);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "❌ SHARED_SERVICE: خطا در ایجاد سرویس مشترک {ServiceCode} در {DepartmentName}",
                                    service.ServiceCode, department.Name);
                                errorCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "❌ SHARED_SERVICE: خطا در پردازش خدمت {ServiceCode}",
                            service.ServiceCode);
                        errorCount++;
                    }
                }

                // مرحله 7: گزارش نهایی
                _logger.Information("═══════════════════════════════════════════════");
                _logger.Information("📊 SHARED_SERVICE: خلاصه عملیات:");
                _logger.Information("   ✅ ایجاد شده: {Added} سرویس مشترک", addedCount);
                _logger.Information("   ⏭️ رد شده: {Skipped} سرویس مشترک", skippedCount);
                _logger.Information("   ❌ خطا: {Error} سرویس مشترک", errorCount);
                _logger.Information("═══════════════════════════════════════════════");

                if (errorCount > 0)
                {
                    _logger.Warning("⚠️ SHARED_SERVICE: {ErrorCount} خطا در ایجاد خدمات مشترک رخ داد", errorCount);
                }

                _logger.Information("✅ SHARED_SERVICE: خدمات مشترک آماده ذخیره‌سازی");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SHARED_SERVICE: خطای کلی در ایجاد خدمات مشترک");
                throw;
            }
        }

        /// <summary>
        /// ایجاد اجزای خدمات (ServiceComponent)
        /// </summary>
        public async Task SeedServiceComponentsAsync()
        {
            try
            {
                _logger.Information("شروع ایجاد اجزای خدمات");

                // بررسی وجود Services - اول از Context.Local، سپس از Database
                var services = _context.Services.Local
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .ToList();

                _logger.Information($"تعداد خدمات در Context.Local: {services.Count}");

                // اگر در Local چیزی نیست یا کم است، از DB بخوان (Fallback)
                if (!services.Any())
                {
                    _logger.Information("⚠️ Context.Local خالی است - بررسی دیتابیس...");
                    services = await _context.Services
                        .Where(s => !s.IsDeleted && s.IsActive)
                        .ToListAsync();
                    _logger.Information($"تعداد خدمات در دیتابیس: {services.Count}");
                }
                else
                {
                    // بررسی اینکه آیا همه Services در Local هستند
                    var dbServicesCount = await _context.Services
                        .Where(s => !s.IsDeleted && s.IsActive)
                        .CountAsync();
                    
                    if (services.Count < dbServicesCount)
                    {
                        _logger.Information("⚠️ Context.Local ناقص است ({LocalCount}/{DbCount}) - بارگذاری از دیتابیس...", 
                            services.Count, dbServicesCount);
                        services = await _context.Services
                            .Where(s => !s.IsDeleted && s.IsActive)
                            .ToListAsync();
                        _logger.Information($"تعداد خدمات بارگذاری شده: {services.Count}");
                    }
                }

                if (!services.Any())
                {
                    _logger.Warning("هیچ خدمتی یافت نشد. ابتدا خدمات را ایجاد کنید.");
                    return;
                }

                // بررسی وجود ServiceTemplates - اول از Context.Local، سپس از Database
                var serviceTemplates = _context.ServiceTemplates.Local
                    .Where(st => !st.IsDeleted && st.IsActive)
                    .ToList();

                _logger.Information($"تعداد قالب‌های خدمات در Context.Local: {serviceTemplates.Count}");

                // اگر در Local چیزی نیست یا کم است، از DB بخوان (Fallback)
                if (!serviceTemplates.Any())
                {
                    _logger.Information("⚠️ Context.Local خالی است - بررسی دیتابیس...");
                    serviceTemplates = await _context.ServiceTemplates
                        .Where(st => !st.IsDeleted && st.IsActive)
                        .ToListAsync();
                    _logger.Information($"تعداد قالب‌های خدمات در دیتابیس: {serviceTemplates.Count}");
                }
                else
                {
                    // بررسی اینکه آیا همه ServiceTemplates در Local هستند
                    var dbTemplatesCount = await _context.ServiceTemplates
                        .Where(st => !st.IsDeleted && st.IsActive)
                        .CountAsync();
                    
                    if (serviceTemplates.Count < dbTemplatesCount)
                    {
                        _logger.Information("⚠️ Context.Local ناقص است ({LocalCount}/{DbCount}) - بارگذاری از دیتابیس...", 
                            serviceTemplates.Count, dbTemplatesCount);
                        serviceTemplates = await _context.ServiceTemplates
                            .Where(st => !st.IsDeleted && st.IsActive)
                            .ToListAsync();
                        _logger.Information($"تعداد قالب‌های خدمات بارگذاری شده: {serviceTemplates.Count}");
                    }
                }

                if (!serviceTemplates.Any())
                {
                    _logger.Warning("هیچ قالب خدمتی یافت نشد. ابتدا ServiceTemplates را ایجاد کنید.");
                    return;
                }

                // دریافت کاربر معتبر برای Seed
                var systemUserId = await GetValidUserIdForSeedAsync();
                _logger.Information($"شناسه کاربر سیستم برای Seed: {systemUserId}");

                var addedCount = 0;
                var skippedCount = 0;

                foreach (var service in services)
                {
                    _logger.Information($"🔍 SERVICE_COMPONENT: پردازش خدمت: {service.Title} (کد: {service.ServiceCode})");

                    // جزء فنی - استفاده از ServiceTemplate (بهترین روش)
                    var technicalCoefficient = await GetDefaultTechnicalCoefficientAsync(service.ServiceCode);
                    _logger.Information($"📊 SERVICE_COMPONENT: ضریب فنی برای {service.ServiceCode}: {technicalCoefficient}");
                    
                    // بررسی Services مشکل‌دار
                    if (service.ServiceCode == "970096" || service.ServiceCode == "970097" || service.ServiceCode == "970098" || 
                        service.ServiceCode == "978000" || service.ServiceCode == "978001" || service.ServiceCode == "978005")
                    {
                        _logger.Warning("⚠️ SERVICE_COMPONENT: پردازش Service مشکل‌دار: {ServiceCode} - {Title}", 
                            service.ServiceCode, service.Title);
                    }

                    // بررسی وجود جزء فنی
                    var existingTechnical = await _context.ServiceComponents
                        .FirstOrDefaultAsync(sc => sc.ServiceId == service.ServiceId 
                                                && sc.ComponentType == ServiceComponentType.Technical 
                                                && !sc.IsDeleted);

                    if (existingTechnical == null)
                    {
                        var technicalComponent = new ServiceComponent
                        {
                            ComponentType = ServiceComponentType.Technical,
                            Coefficient = technicalCoefficient,
                            Description = $"جزء فنی {service.Title}",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = systemUserId
                        };
                        
                        // اضافه کردن از طریق Navigation Property (ایمن‌تر)
                        service.ServiceComponents.Add(technicalComponent);
                        addedCount++;
                    }
                    else
                    {
                        _logger.Information($"جزء فنی برای خدمت {service.Title} از قبل موجود است");
                        skippedCount++;
                    }

                    // جزء حرفه‌ای - استفاده از ServiceTemplate (بهترین روش)
                    var professionalCoefficient = await GetDefaultProfessionalCoefficientAsync(service.ServiceCode);
                    _logger.Information($"ضریب حرفه‌ای برای {service.ServiceCode}: {professionalCoefficient}");

                    // بررسی وجود جزء حرفه‌ای
                    var existingProfessional = await _context.ServiceComponents
                        .FirstOrDefaultAsync(sc => sc.ServiceId == service.ServiceId 
                                                && sc.ComponentType == ServiceComponentType.Professional 
                                                && !sc.IsDeleted);

                    if (existingProfessional == null)
                    {
                        var professionalComponent = new ServiceComponent
                        {
                            ComponentType = ServiceComponentType.Professional,
                            Coefficient = professionalCoefficient,
                            Description = $"جزء حرفه‌ای {service.Title}",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = systemUserId
                        };
                        
                        // اضافه کردن از طریق Navigation Property (ایمن‌تر)
                        service.ServiceComponents.Add(professionalComponent);
                        addedCount++;
                    }
                    else
                    {
                        _logger.Information($"جزء حرفه‌ای برای خدمت {service.Title} از قبل موجود است");
                        skippedCount++;
                    }
                }

                // اجزای جدید مستقیماً به Context اضافه شده‌اند

                _logger.Information("📊 SERVICE_SEED: خلاصه - {Added} جزء اضافه، {Skipped} جزء رد شد", addedCount, skippedCount);
                
                // بررسی Services مشکل‌دار (بدون ServiceComponents)
                var servicesWithoutComponents = services.Where(s => 
                    !s.ServiceComponents.Any(sc => !sc.IsDeleted && sc.IsActive)).ToList();
                
                if (servicesWithoutComponents.Any())
                {
                    _logger.Warning("⚠️ SERVICE_COMPONENT: {Count} خدمت بدون اجزای محاسباتی:", servicesWithoutComponents.Count);
                    foreach (var service in servicesWithoutComponents)
                    {
                        _logger.Warning("   - {ServiceCode}: {Title}", service.ServiceCode, service.Title);
                    }
                }
                // حذف SaveChangesAsync - انجام می‌شود در SystemSeedService
                _logger.Information("✅ SERVICE_SEED: اجزای خدمات آماده ذخیره‌سازی");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SERVICE_SEED: خطا در ایجاد اجزای خدمات");
                throw;
            }
        }

        /// <summary>
        /// دریافت کاربر معتبر برای عملیات Seed
        /// </summary>
        private async Task<string> GetValidUserIdForSeedAsync()
        {
            try
            {
                // ابتدا سعی می‌کنیم کاربر فعلی را دریافت کنیم
                var currentUserId = _currentUserService.GetCurrentUserId();
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    // بررسی وجود کاربر در دیتابیس
                    var userExists = await _context.Users.AnyAsync(u => u.Id == currentUserId);
                    if (userExists)
                    {
                        _logger.Information("استفاده از کاربر فعلی برای Seed: {UserId}", currentUserId);
                        return currentUserId;
                    }
                }

                // اگر کاربر فعلی وجود ندارد، کاربر سیستم را پیدا می‌کنیم
                var systemUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == "3031945451" || u.UserName == "system");
                
                if (systemUser != null)
                {
                    _logger.Information("استفاده از کاربر سیستم برای Seed: {UserId}", systemUser.Id);
                    return systemUser.Id;
                }

                // اگر کاربر سیستم وجود ندارد، کاربر ادمین را پیدا می‌کنیم
                var adminUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == "3020347998" || u.UserName == "admin");
                
                if (adminUser != null)
                {
                    _logger.Information("استفاده از کاربر ادمین برای Seed: {UserId}", adminUser.Id);
                    return adminUser.Id;
                }

                // در صورت عدم وجود هیچ کاربری، از SystemUsers استفاده می‌کنیم
                if (SystemUsers.IsInitialized && !string.IsNullOrEmpty(SystemUsers.AdminUserId))
                {
                    _logger.Information("استفاده از SystemUsers.AdminUserId برای Seed: {UserId}", SystemUsers.AdminUserId);
                    return SystemUsers.AdminUserId;
                }

                // در نهایت، از شناسه پیش‌فرض استفاده می‌کنیم
                var fallbackUserId = "6f999f4d-24b8-4142-a97e-20077850278b";
                _logger.Warning("هیچ کاربر معتبری یافت نشد. استفاده از شناسه پیش‌فرض: {UserId}", fallbackUserId);
                return fallbackUserId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کاربر معتبر برای Seed");
                // در صورت خطا، از شناسه پیش‌فرض استفاده می‌کنیم
                return "6f999f4d-24b8-4142-a97e-20077850278b";
            }
        }

        /// <summary>
        /// دریافت ضریب فنی پیش‌فرض بر اساس کد خدمت
        /// استفاده از ServiceTemplateService (بهترین روش)
        /// </summary>
        private async Task<decimal> GetDefaultTechnicalCoefficientAsync(string serviceCode)
        {
            try
            {
                _logger.Debug("🔍 TEMPLATE: جستجوی ضریب فنی برای کد {Code}", serviceCode);
                
                // اول از Context.Local جستجو کن
                var localTemplate = _context.ServiceTemplates.Local
                    .FirstOrDefault(st => st.ServiceCode == serviceCode && 
                                         st.IsActive && 
                                         !st.IsDeleted);
                
                if (localTemplate != null)
                {
                    _logger.Information("🔍 TEMPLATE: ضریب فنی از Context.Local برای {Code}: {Value}", serviceCode, localTemplate.DefaultTechnicalCoefficient);
                    return localTemplate.DefaultTechnicalCoefficient;
                }

                _logger.Debug("⚠️ TEMPLATE: Context.Local خالی است - جستجو در Database برای {Code}", serviceCode);

                // اگر در Local نبود، از Database جستجو کن
                var template = await _context.ServiceTemplates
                    .FirstOrDefaultAsync(st => st.ServiceCode == serviceCode && 
                                              st.IsActive && 
                                              !st.IsDeleted);
                
                if (template != null)
                {
                    _logger.Information("🔍 TEMPLATE: ضریب فنی از Database برای {Code}: {Value}", serviceCode, template.DefaultTechnicalCoefficient);
                    return template.DefaultTechnicalCoefficient;
                }

                _logger.Warning("⚠️ TEMPLATE: هیچ قالب خدمتی برای کد {Code} یافت نشد - استفاده از مقدار پیش‌فرض 1.0", serviceCode);
                return 1.0m;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ضریب فنی پیش‌فرض برای کد: {ServiceCode}", serviceCode);
                return 1.0m; // مقدار پیش‌فرض در صورت خطا
            }
        }

        /// <summary>
        /// دریافت ضریب حرفه‌ای پیش‌فرض بر اساس کد خدمت
        /// استفاده از ServiceTemplateService (بهترین روش)
        /// </summary>
        private async Task<decimal> GetDefaultProfessionalCoefficientAsync(string serviceCode)
        {
            try
            {
                _logger.Debug("🔍 TEMPLATE: جستجوی ضریب حرفه‌ای برای کد {Code}", serviceCode);
                
                // اول از Context.Local جستجو کن
                var localTemplate = _context.ServiceTemplates.Local
                    .FirstOrDefault(st => st.ServiceCode == serviceCode && 
                                         st.IsActive && 
                                         !st.IsDeleted);
                
                if (localTemplate != null)
                {
                    _logger.Information("🔍 TEMPLATE: ضریب حرفه‌ای از Context.Local برای {Code}: {Value}", serviceCode, localTemplate.DefaultProfessionalCoefficient);
                    return localTemplate.DefaultProfessionalCoefficient;
                }

                _logger.Debug("⚠️ TEMPLATE: Context.Local خالی است - جستجو در Database برای {Code}", serviceCode);

                // اگر در Local نبود، از Database جستجو کن
                var template = await _context.ServiceTemplates
                    .FirstOrDefaultAsync(st => st.ServiceCode == serviceCode && 
                                              st.IsActive && 
                                              !st.IsDeleted);
                
                if (template != null)
                {
                    _logger.Information("🔍 TEMPLATE: ضریب حرفه‌ای از Database برای {Code}: {Value}", serviceCode, template.DefaultProfessionalCoefficient);
                    return template.DefaultProfessionalCoefficient;
                }

                _logger.Warning("⚠️ TEMPLATE: هیچ قالب خدمتی برای کد {Code} یافت نشد - استفاده از مقدار پیش‌فرض 1.0", serviceCode);
                return 1.0m;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ضریب حرفه‌ای پیش‌فرض برای کد: {ServiceCode}", serviceCode);
                return 1.0m; // مقدار پیش‌فرض در صورت خطا
            }
        }

        /// <summary>
        /// بررسی صحت داده‌های ایجاد شده با Logging دقیق و ضدگلوله‌سازی کامل
        /// این متد از Context.Local استفاده می‌کند تا قبل از Commit Transaction نیز کار کند
        /// </summary>
        public async Task<bool> ValidateSeededDataAsync()
        {
            try
            {
                _logger.Information("🔍 SERVICE_VALIDATION: شروع اعتبارسنجی داده‌های Seed شده");

                // بررسی Context.Local (entities که به context اضافه شده‌اند)
                var localServices = _context.Services.Local
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .ToList();

                var localSharedServices = _context.SharedServices.Local
                    .Where(ss => !ss.IsDeleted && ss.IsActive)
                    .ToList();

                var localServiceComponents = _context.ServiceComponents.Local
                    .Where(sc => !sc.IsDeleted && sc.IsActive)
                    .ToList();

                _logger.Information("📊 SERVICE_VALIDATION: Context.Local - خدمات: {Services}, خدمات مشترک: {Shared}, اجزا: {Components}",
                    localServices.Count, localSharedServices.Count, localServiceComponents.Count);

                // اگر در Local چیزی نیست، از DB بخوان (Fallback)
                if (localServices.Count == 0 && localSharedServices.Count == 0 && localServiceComponents.Count == 0)
                {
                    _logger.Information("⚠️ SERVICE_VALIDATION: Context.Local خالی است - بررسی دیتابیس...");

                    var dbServices = await _context.Services
                        .Where(s => !s.IsDeleted && s.IsActive)
                        .ToListAsync();

                    var dbSharedServices = await _context.SharedServices
                        .Where(ss => !ss.IsDeleted && ss.IsActive)
                        .ToListAsync();

                    var dbServiceComponents = await _context.ServiceComponents
                        .Where(sc => !sc.IsDeleted && sc.IsActive)
                        .ToListAsync();

                    _logger.Information("📊 SERVICE_VALIDATION: Database - خدمات: {Services}, خدمات مشترک: {Shared}, اجزا: {Components}",
                        dbServices.Count, dbSharedServices.Count, dbServiceComponents.Count);

                    return await ValidateServicesDataAsync(dbServices, dbSharedServices, dbServiceComponents, "Database");
                }

                // بررسی داده‌های Local
                return await ValidateServicesDataAsync(localServices, localSharedServices, localServiceComponents, "Context.Local");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SERVICE_VALIDATION: خطا در اعتبارسنجی داده‌های ایجاد شده");
                return false;
            }
        }

        /// <summary>
        /// اعتبارسنجی دقیق داده‌های خدمات
        /// </summary>
        private async Task<bool> ValidateServicesDataAsync(List<Service> services, List<SharedService> sharedServices, List<ServiceComponent> serviceComponents, string source)
        {
            try
            {
                _logger.Information("🔍 SERVICE_VALIDATION: شروع اعتبارسنجی از {Source}", source);

                // بررسی اولیه: تعداد کافی
                var basicValidation = services.Count > 0 && sharedServices.Count > 0 && serviceComponents.Count > 0;
                
                if (!basicValidation)
                {
                    _logger.Error("❌ SERVICE_VALIDATION: داده‌های لازم در {Source} یافت نشد!", source);
                    _logger.Error("   - خدمات: {Services} (حداقل: 1)", services.Count);
                    _logger.Error("   - خدمات مشترک: {Shared} (حداقل: 1)", sharedServices.Count);
                    _logger.Error("   - اجزای خدمات: {Components} (حداقل: 1)", serviceComponents.Count);
                    return false;
                }

                _logger.Information("✅ SERVICE_VALIDATION: بررسی اولیه موفق - {Source}", source);

                // بررسی دقیق‌تر: خدمات بدون اجزا
                var servicesWithoutComponents = services
                    .Where(s => s.ServiceComponents == null || !s.ServiceComponents.Any(sc => !sc.IsDeleted && sc.IsActive))
                    .ToList();

                if (servicesWithoutComponents.Any())
                {
                    _logger.Warning("⚠️ SERVICE_VALIDATION: {Count} خدمت بدون اجزای محاسباتی یافت شد:",
                        servicesWithoutComponents.Count);
                    
                    foreach (var service in servicesWithoutComponents.Take(5))
                    {
                        _logger.Warning("   - {Code}: {Title}", service.ServiceCode, service.Title);
                    }
                }

                // بررسی دقیق‌تر: خدمات با قیمت صفر
                var servicesWithoutPrice = services
                    .Where(s => s.Price == 0)
                    .ToList();

                if (servicesWithoutPrice.Any())
                {
                    _logger.Warning("⚠️ SERVICE_VALIDATION: {Count} خدمت با قیمت صفر یافت شد:",
                        servicesWithoutPrice.Count);
                    
                    foreach (var service in servicesWithoutPrice.Take(5))
                    {
                        _logger.Warning("   - {Code}: {Title} = {Price:N0} ریال", 
                            service.ServiceCode, service.Title, service.Price);
                    }
                }

                // بررسی دقیق‌تر: اجزای خدمات
                var technicalComponents = serviceComponents
                    .Where(sc => sc.ComponentType == ServiceComponentType.Technical)
                    .ToList();

                var professionalComponents = serviceComponents
                    .Where(sc => sc.ComponentType == ServiceComponentType.Professional)
                    .ToList();

                _logger.Information("📊 SERVICE_VALIDATION: اجزای خدمات - فنی: {Technical}, حرفه‌ای: {Professional}",
                    technicalComponents.Count, professionalComponents.Count);

                // بررسی دقیق‌تر: خدمات مشترک
                var uniqueServicesInShared = sharedServices
                    .Select(ss => ss.ServiceId)
                    .Distinct()
                    .Count();

                _logger.Information("📊 SERVICE_VALIDATION: خدمات مشترک - تعداد: {Count}, خدمات منحصر: {Unique}",
                    sharedServices.Count, uniqueServicesInShared);

                // نمایش نمونه‌ای از داده‌ها
                _logger.Information("📋 SERVICE_VALIDATION: نمونه خدمات:");
                foreach (var service in services.Take(3))
                {
                    var componentsCount = service.ServiceComponents?.Count(sc => !sc.IsDeleted && sc.IsActive) ?? 0;
                    _logger.Information("   - {Code}: {Title} (اجزا: {Components}, قیمت: {Price:N0} ریال)",
                        service.ServiceCode, service.Title, componentsCount, service.Price);
                }

                _logger.Information("✅ SERVICE_VALIDATION: اعتبارسنجی موفق - همه داده‌ها از {Source} آماده ذخیره‌سازی هستند", source);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SERVICE_VALIDATION: خطا در اعتبارسنجی داده‌های {Source}", source);
                return false;
            }
        }

        /// <summary>
        /// محاسبه و به‌روزرسانی قیمت تمام خدمات بر اساس FactorSettings و ServiceComponents
        /// این متد باید بعد از ایجاد ServiceComponents اجرا شود
        /// توجه: از Context.Local استفاده می‌کند چون entities هنوز ذخیره نشده‌اند
        /// </summary>
        public async Task CalculateAndUpdateServicePricesAsync()
        {
            try
            {
                _logger.Information("═══════════════════════════════════════════════");
                _logger.Information("💰 SERVICE_PRICE: شروع محاسبه خودکار قیمت خدمات");
                _logger.Information("═══════════════════════════════════════════════");

                // دریافت خدمات از Context.Local (entities که به context اضافه شده‌اند ولی هنوز ذخیره نشده‌اند)
                // این خدمات در SeedSampleServicesAsync و SeedServiceComponentsAsync به context اضافه شده‌اند
                var services = _context.Services.Local
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .ToList();

                if (!services.Any())
                {
                    _logger.Warning("⚠️ SERVICE_PRICE: هیچ خدمتی در Context.Local یافت نشد - احتمالاً قبلاً ذخیره شده‌اند");
                    
                    // اگر در Local نیست، از دیتابیس بخوان
                    services = await _context.Services
                        .Include(s => s.ServiceComponents)
                        .Where(s => !s.IsDeleted && s.IsActive)
                        .ToListAsync();

                    if (!services.Any())
                    {
                        _logger.Warning("⚠️ SERVICE_PRICE: هیچ خدمتی در دیتابیس یافت نشد");
                        return;
                    }
                }

                _logger.Information("📊 SERVICE_PRICE: تعداد خدمات یافت شده: {Count}", services.Count);

                var successCount = 0;
                var failedCount = 0;
                var skippedCount = 0;

                foreach (var service in services)
                {
                    try
                    {
                        _logger.Information("🔍 SERVICE_PRICE: پردازش خدمت {ServiceCode} - {ServiceName}",
                            service.ServiceCode, service.Title);

                        // بررسی وجود اجزای محاسباتی
                        var hasComponents = service.ServiceComponents != null && 
                                          service.ServiceComponents.Any(sc => !sc.IsDeleted && sc.IsActive);

                        if (!hasComponents)
                        {
                            _logger.Warning("⏭️ SERVICE_PRICE: خدمت {ServiceCode} - {ServiceName} فاقد اجزای محاسباتی است",
                                service.ServiceCode, service.Title);
                            skippedCount++;
                            continue;
                        }

                        // بررسی اجزای محاسباتی
                        var technicalComponent = service.ServiceComponents
                            .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical && sc.IsActive && !sc.IsDeleted);
                        var professionalComponent = service.ServiceComponents
                            .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional && sc.IsActive && !sc.IsDeleted);

                        _logger.Information("📊 SERVICE_PRICE: اجزای محاسباتی - فنی: {Technical}, حرفه‌ای: {Professional}",
                            technicalComponent?.Coefficient ?? 0, professionalComponent?.Coefficient ?? 0);

                        // بررسی وجود FactorSettings برای سال مالی جاری
                        var currentFinancialYear = SeedConstants.FactorSettings1404.FinancialYear;
                        var technicalFactor = _context.FactorSettings
                            .FirstOrDefault(fs => fs.FactorType == ServiceComponentType.Technical &&
                                                fs.IsHashtagged == service.IsHashtagged &&
                                                fs.FinancialYear == currentFinancialYear &&
                                                fs.IsActive && !fs.IsDeleted);
                        var professionalFactor = _context.FactorSettings
                            .FirstOrDefault(fs => fs.FactorType == ServiceComponentType.Professional &&
                                                fs.IsHashtagged == service.IsHashtagged &&
                                                fs.FinancialYear == currentFinancialYear &&
                                                fs.IsActive && !fs.IsDeleted);

                        _logger.Information("💰 SERVICE_PRICE: کای‌های محاسباتی - فنی: {TechnicalFactor:N0}, حرفه‌ای: {ProfessionalFactor:N0}",
                            technicalFactor?.Value ?? 0, professionalFactor?.Value ?? 0);

                        if (technicalFactor == null || professionalFactor == null)
                        {
                            _logger.Error("❌ SERVICE_PRICE: کای‌های مورد نیاز یافت نشد - فنی: {HasTechnical}, حرفه‌ای: {HasProfessional}",
                                technicalFactor != null, professionalFactor != null);
                            failedCount++;
                            continue;
                        }

                        // محاسبه قیمت با استفاده از ServiceCalculationService
                        // استفاده از سال مالی جاری (از Constants)
                        var calculatedPrice = _serviceCalculationService.CalculateServicePriceWithFactorSettings(
                            service, 
                            _context, 
                            DateTime.Now,  // تاریخ فعلی
                            null,         // بدون Override دپارتمان
                            currentFinancialYear  // سال مالی جاری (از Constants)
                        );

                        // به‌روزرسانی قیمت خدمت (به ریال - decimal(18,0))
                        service.Price = Math.Round(calculatedPrice, 0, MidpointRounding.AwayFromZero);
                        service.UpdatedAt = DateTime.UtcNow;
                        service.UpdatedByUserId = await GetValidUserIdForSeedAsync();

                        _logger.Information("✅ SERVICE_PRICE: {ServiceCode} - {ServiceName} = {Price:N0} ریال",
                            service.ServiceCode, service.Title, service.Price);

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "❌ SERVICE_PRICE: خطا در محاسبه قیمت خدمت {ServiceCode} - {ServiceName}",
                            service.ServiceCode, service.Title);
                        failedCount++;
                    }
                }

                // ذخیره تغییرات قیمت‌ها
                if (successCount > 0)
                {
                    _logger.Information("💾 SERVICE_PRICE: ذخیره تغییرات قیمت‌ها...");
                    await _context.SaveChangesAsync();
                    _logger.Information("✅ SERVICE_PRICE: تغییرات قیمت‌ها با موفقیت ذخیره شدند");
                }

                _logger.Information("═══════════════════════════════════════════════");
                _logger.Information("📊 SERVICE_PRICE: خلاصه محاسبات:");
                _logger.Information("   ✅ موفق: {Success} خدمت", successCount);
                _logger.Information("   ❌ ناموفق: {Failed} خدمت", failedCount);
                _logger.Information("   ⏭️ رد شده: {Skipped} خدمت", skippedCount);
                _logger.Information("═══════════════════════════════════════════════");

                if (failedCount > 0)
                {
                    _logger.Warning("⚠️ SERVICE_PRICE: تعدادی از خدمات قیمت‌گذاری نشدند. لطفاً لاگ‌ها را بررسی کنید.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SERVICE_PRICE: خطا در محاسبه خودکار قیمت خدمات");
                throw;
            }
        }
    }
}
