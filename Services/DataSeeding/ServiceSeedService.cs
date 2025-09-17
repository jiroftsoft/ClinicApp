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

        public ServiceSeedService(
            ApplicationDbContext context, 
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// ایجاد خدمات نمونه با اجزای فنی و حرفه‌ای
        /// </summary>
        public async Task SeedSampleServicesAsync()
        {
            try
            {
                _logger.Information("شروع ایجاد خدمات نمونه");

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

                await _context.SaveChangesAsync();
                _logger.Information("خدمات نمونه با موفقیت ایجاد شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد خدمات نمونه");
                throw;
            }
        }

        /// <summary>
        /// ایجاد خدمات مشترک بین دپارتمان‌ها
        /// </summary>
        public async Task SeedSharedServicesAsync()
        {
            try
            {
                _logger.Information("شروع ایجاد خدمات مشترک");

                // دریافت خدمات و دپارتمان‌ها
                var services = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .ToListAsync();

                var departments = await _context.Departments
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .ToListAsync();

                _logger.Information($"تعداد خدمات یافت شده: {services.Count}");
                _logger.Information($"تعداد دپارتمان‌های یافت شده: {departments.Count}");

                // نمایش جزئیات خدمات و دپارتمان‌ها
                foreach (var service in services)
                {
                    _logger.Information($"خدمت: {service.Title} (ID: {service.ServiceId}, کد: {service.ServiceCode})");
                }

                foreach (var department in departments)
                {
                    _logger.Information($"دپارتمان: {department.Name} (ID: {department.DepartmentId})");
                }

                if (!services.Any())
                {
                    _logger.Warning("هیچ خدمتی یافت نشد. ابتدا خدمات را ایجاد کنید.");
                    return;
                }

                if (!departments.Any())
                {
                    _logger.Warning("هیچ دپارتمانی یافت نشد. ابتدا دپارتمان‌ها را ایجاد کنید.");
                    return;
                }

                var sharedServices = new List<SharedService>();

                // ایجاد خدمات مشترک برای تمام خدمات موجود
                foreach (var service in services.Take(3)) // برای 3 خدمت اول
                {
                    foreach (var department in departments.Take(2)) // برای 2 دپارتمان اول
                    {
                        sharedServices.Add(new SharedService
                        {
                            ServiceId = service.ServiceId,
                            DepartmentId = department.DepartmentId,
                            IsActive = true,
                            DepartmentSpecificNotes = $"{service.Title} در دپارتمان {department.Name}"
                        });
                    }
                }

                _logger.Information($"تعداد خدمات مشترک ایجاد شده: {sharedServices.Count}");

                // دریافت کاربر معتبر برای Seed
                var systemUserId = await GetValidUserIdForSeedAsync();

                foreach (var sharedService in sharedServices)
                {
                    // بررسی وجود سرویس مشترک
                    var existingShared = await _context.SharedServices
                        .FirstOrDefaultAsync(ss => ss.ServiceId == sharedService.ServiceId 
                                                && ss.DepartmentId == sharedService.DepartmentId 
                                                && !ss.IsDeleted);

                    if (existingShared == null)
                    {
                        sharedService.CreatedAt = DateTime.UtcNow;
                        sharedService.CreatedByUserId = systemUserId;
                        _context.SharedServices.Add(sharedService);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.Information("خدمات مشترک با موفقیت ایجاد شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد خدمات مشترک");
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

                // بررسی وجود Services
                var services = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .ToListAsync();

                _logger.Information($"تعداد خدمات یافت شده: {services.Count}");

                if (!services.Any())
                {
                    _logger.Warning("هیچ خدمتی یافت نشد. ابتدا خدمات را ایجاد کنید.");
                    return;
                }

                // بررسی وجود ServiceTemplates
                var serviceTemplates = await _context.ServiceTemplates
                    .Where(st => !st.IsDeleted && st.IsActive)
                    .ToListAsync();

                _logger.Information($"تعداد قالب‌های خدمات یافت شده: {serviceTemplates.Count}");

                if (!serviceTemplates.Any())
                {
                    _logger.Warning("هیچ قالب خدمتی یافت نشد. ابتدا ServiceTemplates را ایجاد کنید.");
                    return;
                }

                // دریافت کاربر معتبر برای Seed
                var systemUserId = await GetValidUserIdForSeedAsync();
                _logger.Information($"شناسه کاربر سیستم برای Seed: {systemUserId}");

                var serviceComponents = new List<ServiceComponent>();
                var addedCount = 0;
                var skippedCount = 0;

                foreach (var service in services)
                {
                    _logger.Information($"پردازش خدمت: {service.Title} (کد: {service.ServiceCode})");

                    // جزء فنی - استفاده از ServiceTemplate (بهترین روش)
                    var technicalCoefficient = await GetDefaultTechnicalCoefficientAsync(service.ServiceCode);
                    _logger.Information($"ضریب فنی برای {service.ServiceCode}: {technicalCoefficient}");

                    // بررسی وجود جزء فنی
                    var existingTechnical = await _context.ServiceComponents
                        .FirstOrDefaultAsync(sc => sc.ServiceId == service.ServiceId 
                                                && sc.ComponentType == ServiceComponentType.Technical 
                                                && !sc.IsDeleted);

                    if (existingTechnical == null)
                    {
                        serviceComponents.Add(new ServiceComponent
                        {
                            ServiceId = service.ServiceId,
                            ComponentType = ServiceComponentType.Technical,
                            Coefficient = technicalCoefficient,
                            Description = $"جزء فنی {service.Title}",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = systemUserId
                        });
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
                        serviceComponents.Add(new ServiceComponent
                        {
                            ServiceId = service.ServiceId,
                            ComponentType = ServiceComponentType.Professional,
                            Coefficient = professionalCoefficient,
                            Description = $"جزء حرفه‌ای {service.Title}",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = systemUserId
                        });
                        addedCount++;
                    }
                    else
                    {
                        _logger.Information($"جزء حرفه‌ای برای خدمت {service.Title} از قبل موجود است");
                        skippedCount++;
                    }
                }

                // اضافه کردن اجزای جدید
                if (serviceComponents.Any())
                {
                    _context.ServiceComponents.AddRange(serviceComponents);
                    await _context.SaveChangesAsync();
                    _logger.Information($"تعداد {serviceComponents.Count} جزء جدید اضافه شد");
                }

                _logger.Information($"خلاصه: {addedCount} جزء اضافه شد، {skippedCount} جزء رد شد");
                _logger.Information("اجزای خدمات با موفقیت ایجاد شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اجزای خدمات");
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
                var template = await _context.ServiceTemplates
                    .FirstOrDefaultAsync(st => st.ServiceCode == serviceCode && 
                                              st.IsActive && 
                                              !st.IsDeleted);
                return template?.DefaultTechnicalCoefficient ?? 1.0m;
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
                var template = await _context.ServiceTemplates
                    .FirstOrDefaultAsync(st => st.ServiceCode == serviceCode && 
                                              st.IsActive && 
                                              !st.IsDeleted);
                return template?.DefaultProfessionalCoefficient ?? 1.0m;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ضریب حرفه‌ای پیش‌فرض برای کد: {ServiceCode}", serviceCode);
                return 1.0m; // مقدار پیش‌فرض در صورت خطا
            }
        }

        /// <summary>
        /// بررسی صحت داده‌های ایجاد شده
        /// </summary>
        public async Task<bool> ValidateSeededDataAsync()
        {
            try
            {
                var servicesCount = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .CountAsync();

                var sharedServicesCount = await _context.SharedServices
                    .Where(ss => !ss.IsDeleted && ss.IsActive)
                    .CountAsync();

                var serviceComponentsCount = await _context.ServiceComponents
                    .Where(sc => !sc.IsDeleted && sc.IsActive)
                    .CountAsync();

                _logger.Information($"تعداد خدمات: {servicesCount}, خدمات مشترک: {sharedServicesCount}, اجزای خدمات: {serviceComponentsCount}");

                return servicesCount > 0 && sharedServicesCount > 0 && serviceComponentsCount > 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی داده‌های ایجاد شده");
                return false;
            }
        }
    }
}
