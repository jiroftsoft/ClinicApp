using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Enums;
using ClinicApp.Models;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس محاسبه مبلغ خدمات - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. محاسبه دقیق قیمت خدمات بر اساس اجزای فنی و حرفه‌ای
    /// 2. پشتیبانی از تنظیمات مرکزی (FactorSettings)
    /// 3. محاسبه خدمات مشترک با Override دپارتمان
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. رعایت استانداردهای پزشکی ایران
    /// 6. استفاده از Dependency Injection
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class ServiceCalculationService : IServiceCalculationService
    {
        /// <summary>
        /// محاسبه مبلغ نهایی خدمت (فرمول پایه - بدون FactorSettings)
        /// فرمول: (جزء فنی × کای فنی) + (جزء حرفه‌ای × کای حرفه‌ای) = مبلغ خدمت
        /// 
        /// ⚠️ هشدار: این متد فقط برای محاسبات پایه استفاده می‌شود.
        /// برای محاسبات دقیق از CalculateServicePriceWithFactorSettings استفاده کنید.
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <returns>مبلغ نهایی خدمت</returns>
        public decimal CalculateServicePrice(Service service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            // اگر خدمت دارای اجزای فنی و حرفه‌ای است
            if (service.ServiceComponents != null && service.ServiceComponents.Any())
            {
                var technicalComponent = service.ServiceComponents
                    .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical && sc.IsActive && !sc.IsDeleted);

                var professionalComponent = service.ServiceComponents
                    .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional && sc.IsActive && !sc.IsDeleted);

                // اگر هر دو جزء موجود باشند
                if (technicalComponent != null && professionalComponent != null)
                {
                    // استفاده از ضرایب ثابت (بدون FactorSettings)
                    // کای فنی: هشتگ‌دار = 65000، بدون هشتگ = 31000
                    // کای حرفه‌ای: 41000 (ثابت)
                    decimal technicalFactor = service.IsHashtagged ? 65000 : 31000;
                    decimal professionalFactor = 41000;
                    
                    return (technicalComponent.Coefficient * technicalFactor) + 
                           (professionalComponent.Coefficient * professionalFactor);
                }
            }

            // اگر اجزای خدمت تعریف نشده‌اند، از قیمت پایه استفاده کن
            return service.Price;
        }

        /// <summary>
        /// محاسبه مبلغ خدمت بر اساس شناسه خدمت
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>مبلغ نهایی خدمت</returns>
        public decimal CalculateServicePrice(int serviceId, ApplicationDbContext context)
        {
            var service = context.Services
                .Include(s => s.ServiceComponents)
                .FirstOrDefault(s => s.ServiceId == serviceId && !s.IsDeleted);

            return CalculateServicePrice(service);
        }

        /// <summary>
        /// بررسی اینکه آیا خدمت دارای اجزای کامل است یا نه
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <returns>true اگر خدمت دارای اجزای فنی و حرفه‌ای باشد</returns>
        public bool HasCompleteComponents(Service service)
        {
            if (service?.ServiceComponents == null)
                return false;

            var hasTechnical = service.ServiceComponents
                .Any(sc => sc.ComponentType == ServiceComponentType.Technical && sc.IsActive && !sc.IsDeleted);

            var hasProfessional = service.ServiceComponents
                .Any(sc => sc.ComponentType == ServiceComponentType.Professional && sc.IsActive && !sc.IsDeleted);

            return hasTechnical && hasProfessional;
        }

        /// <summary>
        /// دریافت جزء فنی خدمت
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <returns>جزء فنی یا null</returns>
        public ServiceComponent GetTechnicalComponent(Service service)
        {
            return service?.ServiceComponents?
                .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical && sc.IsActive && !sc.IsDeleted);
        }

        /// <summary>
        /// دریافت جزء حرفه‌ای خدمت
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <returns>جزء حرفه‌ای یا null</returns>
        public ServiceComponent GetProfessionalComponent(Service service)
        {
            return service?.ServiceComponents?
                .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional && sc.IsActive && !sc.IsDeleted);
        }

        /// <summary>
        /// محاسبه مبلغ خدمت با در نظر گیری تخفیف
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <param name="discountPercentage">درصد تخفیف (0-100)</param>
        /// <returns>مبلغ نهایی با تخفیف</returns>
        public decimal CalculateServicePriceWithDiscount(Service service, decimal discountPercentage)
        {
            var basePrice = CalculateServicePrice(service);
            var discountAmount = basePrice * (discountPercentage / 100);
            return basePrice - discountAmount;
        }

        /// <summary>
        /// محاسبه مبلغ خدمت با در نظر گیری مالیات
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <param name="taxPercentage">درصد مالیات (0-100)</param>
        /// <returns>مبلغ نهایی با مالیات</returns>
        public decimal CalculateServicePriceWithTax(Service service, decimal taxPercentage)
        {
            var basePrice = CalculateServicePrice(service);
            var taxAmount = basePrice * (taxPercentage / 100);
            return basePrice + taxAmount;
        }

        /// <summary>
        /// محاسبه مبلغ خدمت بر اساس منطق هشتگ
        /// کای فنی: هشتگ‌دار = 65000، بدون هشتگ = 31000
        /// کای حرفه‌ای: 41000 (ثابت)
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <returns>مبلغ نهایی خدمت</returns>
        public decimal CalculateServicePriceWithHashtagLogic(Service service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            // کای حرفه‌ای ثابت
            decimal professionalCoefficient = 41000;

            // کای فنی بر اساس هشتگ
            decimal technicalCoefficient = service.IsHashtagged ? 65000 : 31000;

            return technicalCoefficient * professionalCoefficient;
        }

        /// <summary>
        /// محاسبه مبلغ خدمت با در نظر گیری Override دپارتمان
        /// این متد برای خدمات مشترک که در دپارتمان‌های مختلف Override دارند استفاده می‌شود
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>مبلغ نهایی خدمت</returns>
        public decimal CalculateServicePriceWithDepartmentOverride(Service service, int? departmentId, ApplicationDbContext context)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            // کای‌های پیش‌فرض
            decimal technicalCoefficient = service.IsHashtagged ? 65000 : 31000;
            decimal professionalCoefficient = 41000;

            // اگر دپارتمان مشخص شده است، بررسی Override
            if (departmentId.HasValue)
            {
                var sharedService = context.SharedServices
                    .FirstOrDefault(ss => ss.ServiceId == service.ServiceId && 
                                        ss.DepartmentId == departmentId.Value && 
                                        ss.IsActive && !ss.IsDeleted);

                if (sharedService != null)
                {
                    // استفاده از Override اگر موجود باشد
                    technicalCoefficient = sharedService.OverrideTechnicalFactor ?? technicalCoefficient;
                    professionalCoefficient = sharedService.OverrideProfessionalFactor ?? professionalCoefficient;
                }
            }

            return technicalCoefficient * professionalCoefficient;
        }

        /// <summary>
        /// بررسی اینکه آیا خدمت در دپارتمان مشترک است یا نه
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>true اگر خدمت در دپارتمان مشترک باشد</returns>
        public bool IsServiceSharedInDepartment(int serviceId, int departmentId, ApplicationDbContext context)
        {
            return context.SharedServices
                .Any(ss => ss.ServiceId == serviceId && 
                          ss.DepartmentId == departmentId && 
                          ss.IsActive && 
                          !ss.IsDeleted);
        }

        /// <summary>
        /// دریافت لیست دپارتمان‌هایی که یک خدمت در آن‌ها مشترک است
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>لیست شناسه‌های دپارتمان</returns>
        public List<int> GetSharedDepartmentsForService(int serviceId, ApplicationDbContext context)
        {
            return context.SharedServices
                .Where(ss => ss.ServiceId == serviceId && 
                           ss.IsActive && 
                           !ss.IsDeleted)
                .Select(ss => ss.DepartmentId)
                .ToList();
        }

        /// <summary>
        /// محاسبه مبلغ خدمت مشترک با در نظر گیری Override دپارتمان
        /// </summary>
        /// <param name="sharedService">خدمت مشترک</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>مبلغ نهایی خدمت</returns>
        public decimal CalculateSharedServicePrice(SharedService sharedService, ApplicationDbContext context)
        {
            if (sharedService == null)
                throw new ArgumentNullException(nameof(sharedService));

            var service = context.Services
                .Include(s => s.ServiceComponents)
                .FirstOrDefault(s => s.ServiceId == sharedService.ServiceId && !s.IsDeleted);

            if (service == null)
                throw new InvalidOperationException($"خدمت با شناسه {sharedService.ServiceId} یافت نشد");

            // استفاده از Override دپارتمان اگر موجود باشد
            decimal technicalCoefficient = sharedService.OverrideTechnicalFactor ?? (service.IsHashtagged ? 65000 : 31000);
            decimal professionalCoefficient = sharedService.OverrideProfessionalFactor ?? 41000;

            return technicalCoefficient * professionalCoefficient;
        }

        /// <summary>
        /// محاسبه مبلغ خدمت بر اساس FactorSetting
        /// این متد از تنظیمات مرکزی ضرایب استفاده می‌کند
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <param name="date">تاریخ محاسبه (اختیاری، پیش‌فرض: امروز)</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <param name="financialYear">سال مالی (اختیاری، پیش‌فرض: سال جاری)</param>
        /// <returns>مبلغ نهایی خدمت</returns>
        public decimal CalculateServicePriceWithFactorSettings(Service service, ApplicationDbContext context, DateTime? date = null, int? departmentId = null, int? financialYear = null)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var calculationDate = date ?? DateTime.Now;
            var currentFinancialYear = financialYear ?? GetCurrentFinancialYear(calculationDate);

            // بررسی فریز بودن سال مالی
            if (IsFinancialYearFrozen(currentFinancialYear, context))
                throw new InvalidOperationException($"سال مالی {currentFinancialYear} فریز شده است و نمی‌توان محاسبات جدید انجام داد.");

            // دریافت ضریب حرفه‌ای (بر اساس هشتگ خدمت)
            var professionalFactor = context.FactorSettings
                .Where(fs => fs.FactorType == ServiceComponentType.Professional &&
                            fs.IsHashtagged == service.IsHashtagged && // کای حرفه‌ای بر اساس هشتگ خدمت
                            fs.FinancialYear == currentFinancialYear &&
                            fs.IsActive && !fs.IsDeleted &&
                            !fs.IsFrozen && // بررسی فریز نبودن
                            fs.EffectiveFrom <= calculationDate &&
                            (fs.EffectiveTo == null || fs.EffectiveTo >= calculationDate))
                .OrderByDescending(fs => fs.EffectiveFrom)
                .FirstOrDefault();

            if (professionalFactor == null)
                throw new InvalidOperationException($"ضریب حرفه‌ای برای خدمات {(service.IsHashtagged ? "هشتگ‌دار" : "بدون هشتگ")} در سال مالی {currentFinancialYear} یافت نشد. لطفاً ابتدا کای حرفه‌ای مربوطه را تعریف کنید.");

            // دریافت ضریب فنی (بر اساس هشتگ)
            var technicalFactor = context.FactorSettings
                .Where(fs => fs.FactorType == ServiceComponentType.Technical &&
                            fs.IsHashtagged == service.IsHashtagged &&
                            fs.FinancialYear == currentFinancialYear &&
                            fs.IsActive && !fs.IsDeleted &&
                            !fs.IsFrozen && // بررسی فریز نبودن
                            fs.EffectiveFrom <= calculationDate &&
                            (fs.EffectiveTo == null || fs.EffectiveTo >= calculationDate))
                .OrderByDescending(fs => fs.EffectiveFrom)
                .FirstOrDefault();

            if (technicalFactor == null)
                throw new InvalidOperationException($"ضریب فنی برای خدمات {(service.IsHashtagged ? "هشتگ‌دار" : "بدون هشتگ")} در سال مالی {currentFinancialYear} یافت نشد. لطفاً ابتدا کای فنی مربوطه را تعریف کنید.");

            // محاسبه مبلغ پایه بر اساس فرمول وزارت بهداشت
            // مبلغ = (جزء فنی × کای فنی) + (جزء حرفه‌ای × کای حرفه‌ای)
            // استفاده انحصاری از ServiceComponents
            
            if (service.ServiceComponents == null || !service.ServiceComponents.Any())
                throw new InvalidOperationException($"خدمت {service.Title} دارای اجزای محاسباتی نیست. لطفاً ابتدا اجزای فنی و حرفه‌ای را تعریف کنید.");

            var technicalComponent = service.ServiceComponents
                .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical && sc.IsActive && !sc.IsDeleted);
            var professionalComponent = service.ServiceComponents
                .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional && sc.IsActive && !sc.IsDeleted);

            if (technicalComponent == null)
                throw new InvalidOperationException($"جزء فنی برای خدمت {service.Title} یافت نشد.");
            if (professionalComponent == null)
                throw new InvalidOperationException($"جزء حرفه‌ای برای خدمت {service.Title} یافت نشد.");

            decimal basePrice = (technicalComponent.Coefficient * technicalFactor.Value) + 
                               (professionalComponent.Coefficient * professionalFactor.Value);

            // بررسی Override دپارتمان
            if (departmentId.HasValue)
            {
                var sharedService = context.SharedServices
                    .FirstOrDefault(ss => ss.ServiceId == service.ServiceId && 
                                        ss.DepartmentId == departmentId.Value && 
                                        ss.IsActive && !ss.IsDeleted);

                if (sharedService != null)
                {
                    // اگر Override موجود باشد، از آن استفاده کن
                    var overrideTechnicalFactor = sharedService.OverrideTechnicalFactor ?? technicalFactor.Value;
                    var overrideProfessionalFactor = sharedService.OverrideProfessionalFactor ?? professionalFactor.Value;
                    basePrice = (technicalComponent.Coefficient * overrideTechnicalFactor) + 
                               (professionalComponent.Coefficient * overrideProfessionalFactor);
                }
            }

            return basePrice;
        }

        /// <summary>
        /// محاسبه مبلغ خدمت با جزئیات کامل (متد قدیمی - حذف شده)
        /// </summary>
        [Obsolete("از متد جدید با پارامتر financialYear استفاده کنید")]
        public ServiceCalculationDetails CalculateServicePriceWithDetailsOld(Service service, ApplicationDbContext context, DateTime? date = null, int? departmentId = null)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var calculationDate = date ?? DateTime.Now;
            var currentFinancialYear = GetCurrentFinancialYear(calculationDate);

            // دریافت ضریب حرفه‌ای (همیشه ثابت برای همه خدمات)
            var professionalFactor = context.FactorSettings
                .Where(fs => fs.FactorType == ServiceComponentType.Professional &&
                            fs.IsHashtagged == false && // کای حرفه‌ای همیشه false است
                            fs.FinancialYear == currentFinancialYear &&
                            fs.IsActive && !fs.IsDeleted &&
                            !fs.IsFrozen &&
                            fs.EffectiveFrom <= calculationDate &&
                            (fs.EffectiveTo == null || fs.EffectiveTo >= calculationDate))
                .OrderByDescending(fs => fs.EffectiveFrom)
                .FirstOrDefault();

            if (professionalFactor == null)
                throw new InvalidOperationException($"ضریب حرفه‌ای برای خدمات {(service.IsHashtagged ? "هشتگ‌دار" : "بدون هشتگ")} در سال مالی {currentFinancialYear} یافت نشد. لطفاً ابتدا کای حرفه‌ای مربوطه را تعریف کنید.");

            // دریافت ضریب فنی (بر اساس هشتگ)
            var technicalFactor = context.FactorSettings
                .Where(fs => fs.FactorType == ServiceComponentType.Technical &&
                            fs.IsHashtagged == service.IsHashtagged &&
                            fs.FinancialYear == currentFinancialYear &&
                            fs.IsActive && !fs.IsDeleted &&
                            !fs.IsFrozen &&
                            fs.EffectiveFrom <= calculationDate &&
                            (fs.EffectiveTo == null || fs.EffectiveTo >= calculationDate))
                .OrderByDescending(fs => fs.EffectiveFrom)
                .FirstOrDefault();

            if (technicalFactor == null)
                throw new InvalidOperationException($"ضریب فنی برای خدمات {(service.IsHashtagged ? "هشتگ‌دار" : "بدون هشتگ")} در سال مالی {currentFinancialYear} یافت نشد. لطفاً ابتدا کای فنی مربوطه را تعریف کنید.");

            // بررسی Override دپارتمان
            decimal finalTechnicalFactor = technicalFactor.Value;
            decimal finalProfessionalFactor = professionalFactor.Value;
            bool hasDepartmentOverride = false;

            if (departmentId.HasValue)
            {
                var sharedService = context.SharedServices
                    .FirstOrDefault(ss => ss.ServiceId == service.ServiceId && 
                                        ss.DepartmentId == departmentId.Value && 
                                        ss.IsActive && !ss.IsDeleted);

                if (sharedService != null)
                {
                    finalTechnicalFactor = sharedService.OverrideTechnicalFactor ?? technicalFactor.Value;
                    finalProfessionalFactor = sharedService.OverrideProfessionalFactor ?? professionalFactor.Value;
                    hasDepartmentOverride = true;
                }
            }

            // محاسبه جزئیات - استفاده انحصاری از ServiceComponents
            if (service.ServiceComponents == null || !service.ServiceComponents.Any())
                throw new InvalidOperationException($"خدمت {service.Title} دارای اجزای محاسباتی نیست. لطفاً ابتدا اجزای فنی و حرفه‌ای را تعریف کنید.");

            var technicalComponent = service.ServiceComponents
                .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical && sc.IsActive && !sc.IsDeleted);
            var professionalComponent = service.ServiceComponents
                .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional && sc.IsActive && !sc.IsDeleted);

            if (technicalComponent == null)
                throw new InvalidOperationException($"جزء فنی برای خدمت {service.Title} یافت نشد.");
            if (professionalComponent == null)
                throw new InvalidOperationException($"جزء حرفه‌ای برای خدمت {service.Title} یافت نشد.");

            decimal technicalAmount = technicalComponent.Coefficient * finalTechnicalFactor;
            decimal professionalAmount = professionalComponent.Coefficient * finalProfessionalFactor;
            decimal totalAmount = technicalAmount + professionalAmount;

            return new ServiceCalculationDetails
            {
                ServiceId = service.ServiceId,
                ServiceTitle = service.Title,
                IsHashtagged = service.IsHashtagged,
                TechnicalPart = technicalComponent.Coefficient,
                ProfessionalPart = professionalComponent.Coefficient,
                TechnicalFactor = finalTechnicalFactor,
                ProfessionalFactor = finalProfessionalFactor,
                TechnicalAmount = technicalAmount,
                ProfessionalAmount = professionalAmount,
                TotalAmount = totalAmount,
                HasDepartmentOverride = hasDepartmentOverride,
                DepartmentId = departmentId,
                CalculationDate = calculationDate
            };
        }

        /// <summary>
        /// دریافت کای حرفه‌ای فعال
        /// </summary>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <param name="date">تاریخ (اختیاری، پیش‌فرض: امروز)</param>
        /// <returns>کای حرفه‌ای فعال</returns>
        public FactorSetting GetActiveProfessionalFactor(ApplicationDbContext context, DateTime? date = null)
        {
            var calculationDate = date ?? DateTime.Now;
            var currentFinancialYear = GetCurrentFinancialYear(calculationDate);

            return context.FactorSettings
                .Where(fs => fs.FactorType == ServiceComponentType.Professional &&
                            fs.IsHashtagged == false && // کای حرفه‌ای همیشه false است
                            fs.FinancialYear == currentFinancialYear &&
                            fs.IsActive && !fs.IsDeleted &&
                            !fs.IsFrozen &&
                            fs.EffectiveFrom <= calculationDate &&
                            (fs.EffectiveTo == null || fs.EffectiveTo >= calculationDate))
                .OrderByDescending(fs => fs.EffectiveFrom)
                .FirstOrDefault();
        }

        /// <summary>
        /// دریافت کای فنی فعال (هشتگ‌دار یا بدون هشتگ)
        /// </summary>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <param name="isHashtagged">آیا هشتگ‌دار است؟</param>
        /// <param name="date">تاریخ (اختیاری، پیش‌فرض: امروز)</param>
        /// <returns>کای فنی فعال</returns>
        public FactorSetting GetActiveTechnicalFactor(ApplicationDbContext context, bool isHashtagged, DateTime? date = null)
        {
            var calculationDate = date ?? DateTime.Now;
            var currentFinancialYear = GetCurrentFinancialYear(calculationDate);

            return context.FactorSettings
                .Where(fs => fs.FactorType == ServiceComponentType.Technical &&
                            fs.IsHashtagged == isHashtagged &&
                            fs.FinancialYear == currentFinancialYear &&
                            fs.IsActive && !fs.IsDeleted &&
                            !fs.IsFrozen &&
                            fs.EffectiveFrom <= calculationDate &&
                            (fs.EffectiveTo == null || fs.EffectiveTo >= calculationDate))
                .OrderByDescending(fs => fs.EffectiveFrom)
                .FirstOrDefault();
        }

        /// <summary>
        /// بررسی وجود کای‌های مورد نیاز
        /// </summary>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <param name="date">تاریخ (اختیاری، پیش‌فرض: امروز)</param>
        /// <returns>نتیجه بررسی</returns>
        public FactorValidationResult ValidateRequiredFactors(ApplicationDbContext context, DateTime? date = null)
        {
            var calculationDate = date ?? DateTime.Now;
            var result = new FactorValidationResult();

            // بررسی کای حرفه‌ای
            var professionalFactor = GetActiveProfessionalFactor(context, calculationDate);
            if (professionalFactor == null)
            {
                result.Errors.Add("کای حرفه‌ای تعریف نشده است.");
            }
            else
            {
                result.ProfessionalFactor = professionalFactor;
            }

            // بررسی کای فنی هشتگ‌دار
            var technicalFactorHashtagged = GetActiveTechnicalFactor(context, true, calculationDate);
            if (technicalFactorHashtagged == null)
            {
                result.Errors.Add("کای فنی هشتگ‌دار تعریف نشده است.");
            }
            else
            {
                result.TechnicalFactorHashtagged = technicalFactorHashtagged;
            }

            // بررسی کای فنی بدون هشتگ
            var technicalFactorNonHashtagged = GetActiveTechnicalFactor(context, false, calculationDate);
            if (technicalFactorNonHashtagged == null)
            {
                result.Errors.Add("کای فنی بدون هشتگ تعریف نشده است.");
            }
            else
            {
                result.TechnicalFactorNonHashtagged = technicalFactorNonHashtagged;
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        /// دریافت سال مالی جاری بر اساس تاریخ میلادی
        /// </summary>
        /// <param name="date">تاریخ (اختیاری، پیش‌فرض: امروز)</param>
        /// <returns>سال مالی شمسی</returns>
        public int GetCurrentFinancialYear(DateTime? date = null)
        {
            var targetDate = date ?? DateTime.Now;
            
            // تبدیل تاریخ میلادی به شمسی
            var persianCalendar = new System.Globalization.PersianCalendar();
            var persianYear = persianCalendar.GetYear(targetDate);
            
            return persianYear;
        }

        #region Shared Service Calculation Methods

        /// <summary>
        /// محاسبه قیمت خدمت مشترک با Override
        /// </summary>
        public async Task<ServiceCalculationResult> CalculateSharedServicePriceAsync(int serviceId, int departmentId, 
            ApplicationDbContext context, decimal? overrideTechnicalFactor = null, 
            decimal? overrideProfessionalFactor = null, DateTime? date = null)
        {
            try
            {
                Console.WriteLine($"🔍 [CALCULATION] شروع محاسبه قیمت خدمت مشترک - ServiceId: {serviceId}, DepartmentId: {departmentId}");
                Console.WriteLine($"🔍 [CALCULATION] Override Technical Factor: {overrideTechnicalFactor}, Override Professional Factor: {overrideProfessionalFactor}");
                Console.WriteLine($"🔍 [CALCULATION] تاریخ محاسبه: {date ?? DateTime.Now}");

                var service = await context.Services
                    .Include(s => s.ServiceComponents)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                if (service == null)
                {
                    Console.WriteLine($"❌ [CALCULATION] خدمت یافت نشد - ServiceId: {serviceId}");
                    return new ServiceCalculationResult
                    {
                        Success = false,
                        Message = "خدمت یافت نشد"
                    };
                }

                Console.WriteLine($"✅ [CALCULATION] خدمت یافت شد - Title: {service.Title}, Code: {service.ServiceCode}, IsHashtagged: {service.IsHashtagged}");

                // بررسی وجود اجزای محاسباتی
                if (service.ServiceComponents == null || !service.ServiceComponents.Any())
                {
                    Console.WriteLine($"❌ [CALCULATION] خدمت فاقد اجزای محاسباتی است - ServiceId: {serviceId}");
                    return new ServiceCalculationResult
                    {
                        Success = false,
                        Message = "خدمت فاقد اجزای محاسباتی است. لطفاً ابتدا اجزای فنی و حرفه‌ای را تعریف کنید."
                    };
                }

                Console.WriteLine($"📊 [CALCULATION] تعداد اجزای خدمت: {service.ServiceComponents.Count}");

                var technicalComponent = service.ServiceComponents
                    .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical && sc.IsActive && !sc.IsDeleted);
                var professionalComponent = service.ServiceComponents
                    .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional && sc.IsActive && !sc.IsDeleted);

                if (technicalComponent == null)
                {
                    Console.WriteLine($"❌ [CALCULATION] جزء فنی یافت نشد - ServiceId: {serviceId}");
                    return new ServiceCalculationResult
                    {
                        Success = false,
                        Message = "جزء فنی برای این خدمت تعریف نشده است."
                    };
                }

                if (professionalComponent == null)
                {
                    Console.WriteLine($"❌ [CALCULATION] جزء حرفه‌ای یافت نشد - ServiceId: {serviceId}");
                    return new ServiceCalculationResult
                    {
                        Success = false,
                        Message = "جزء حرفه‌ای برای این خدمت تعریف نشده است."
                    };
                }

                Console.WriteLine($"✅ [CALCULATION] اجزای خدمت یافت شد:");
                Console.WriteLine($"   🔧 Technical Component: Coefficient = {technicalComponent.Coefficient}");
                Console.WriteLine($"   👨‍⚕️ Professional Component: Coefficient = {professionalComponent.Coefficient}");

                // دریافت ضرایب از FactorSettings
                var currentDate = date ?? DateTime.Now;
                var currentFinancialYear = GetCurrentFinancialYear(currentDate);

                Console.WriteLine($"📅 [CALCULATION] سال مالی جاری: {currentFinancialYear}");
                Console.WriteLine($"📅 [CALCULATION] تاریخ محاسبه: {currentDate}");

                var professionalFactor = await context.FactorSettings
                    .Where(fs => fs.FactorType == ServiceComponentType.Professional &&
                                fs.IsHashtagged == false &&
                                fs.FinancialYear == currentFinancialYear &&
                                fs.IsActive && !fs.IsDeleted && !fs.IsFrozen &&
                                fs.EffectiveFrom <= currentDate &&
                                (fs.EffectiveTo == null || fs.EffectiveTo >= currentDate))
                    .OrderByDescending(fs => fs.EffectiveFrom)
                    .FirstOrDefaultAsync();

                var technicalFactor = await context.FactorSettings
                    .Where(fs => fs.FactorType == ServiceComponentType.Technical &&
                                fs.IsHashtagged == service.IsHashtagged &&
                                fs.FinancialYear == currentFinancialYear &&
                                fs.IsActive && !fs.IsDeleted && !fs.IsFrozen &&
                                fs.EffectiveFrom <= currentDate &&
                                (fs.EffectiveTo == null || fs.EffectiveTo >= currentDate))
                    .OrderByDescending(fs => fs.EffectiveFrom)
                    .FirstOrDefaultAsync();

                if (professionalFactor == null)
                {
                    Console.WriteLine($"❌ [CALCULATION] ضریب حرفه‌ای یافت نشد - FinancialYear: {currentFinancialYear}");
                    return new ServiceCalculationResult
                    {
                        Success = false,
                        Message = $"ضریب حرفه‌ای برای سال مالی {currentFinancialYear} یافت نشد."
                    };
                }

                if (technicalFactor == null)
                {
                    Console.WriteLine($"❌ [CALCULATION] ضریب فنی یافت نشد - FinancialYear: {currentFinancialYear}, IsHashtagged: {service.IsHashtagged}");
                    return new ServiceCalculationResult
                    {
                        Success = false,
                        Message = $"ضریب فنی برای خدمات {(service.IsHashtagged ? "هشتگ‌دار" : "بدون هشتگ")} در سال مالی {currentFinancialYear} یافت نشد."
                    };
                }

                Console.WriteLine($"✅ [CALCULATION] ضرایب یافت شد:");
                Console.WriteLine($"   🔧 Technical Factor: {technicalFactor.Value:N0} (IsHashtagged: {service.IsHashtagged})");
                Console.WriteLine($"   👨‍⚕️ Professional Factor: {professionalFactor.Value:N0}");

                // محاسبه قیمت با فرمول صحیح
                decimal finalTechnicalFactor = overrideTechnicalFactor ?? technicalFactor.Value;
                decimal finalProfessionalFactor = overrideProfessionalFactor ?? professionalFactor.Value;
                
                Console.WriteLine($"🔧 [CALCULATION] ضرایب نهایی:");
                Console.WriteLine($"   🔧 Final Technical Factor: {finalTechnicalFactor:N0}");
                Console.WriteLine($"   👨‍⚕️ Final Professional Factor: {finalProfessionalFactor:N0}");
                Console.WriteLine($"   🔧 Override Technical: {overrideTechnicalFactor?.ToString() ?? "None"}");
                Console.WriteLine($"   👨‍⚕️ Override Professional: {overrideProfessionalFactor?.ToString() ?? "None"}");
                
                decimal technicalAmount = technicalComponent.Coefficient * finalTechnicalFactor;
                decimal professionalAmount = professionalComponent.Coefficient * finalProfessionalFactor;
                decimal calculatedPrice = technicalAmount + professionalAmount;

                Console.WriteLine($"💰 [CALCULATION] محاسبه قیمت:");
                Console.WriteLine($"   🔧 Technical Amount: {technicalComponent.Coefficient} × {finalTechnicalFactor:N0} = {technicalAmount:N0}");
                Console.WriteLine($"   👨‍⚕️ Professional Amount: {professionalComponent.Coefficient} × {finalProfessionalFactor:N0} = {professionalAmount:N0}");
                Console.WriteLine($"   💰 Total Price: {technicalAmount:N0} + {professionalAmount:N0} = {calculatedPrice:N0}");

                var details = new ServiceCalculationDetails
                {
                    ServiceId = service.ServiceId,
                    ServiceTitle = service.Title,
                    ServiceCode = service.ServiceCode,
                    IsHashtagged = service.IsHashtagged,
                    TechnicalPart = technicalComponent.Coefficient,
                    ProfessionalPart = professionalComponent.Coefficient,
                    TechnicalFactor = finalTechnicalFactor,
                    ProfessionalFactor = finalProfessionalFactor,
                    TechnicalAmount = technicalAmount,
                    ProfessionalAmount = professionalAmount,
                    TotalAmount = calculatedPrice,
                    HasDepartmentOverride = overrideTechnicalFactor.HasValue || overrideProfessionalFactor.HasValue,
                    DepartmentId = departmentId,
                    CalculationDate = currentDate
                };

                // فرمول صحیح برای نمایش
                string calculationFormula = $"({technicalComponent.Coefficient} × {finalTechnicalFactor:N0}) + ({professionalComponent.Coefficient} × {finalProfessionalFactor:N0}) = {calculatedPrice:N0}";
                
                Console.WriteLine($"📝 [CALCULATION] فرمول محاسبه: {calculationFormula}");
                Console.WriteLine($"✅ [CALCULATION] محاسبه موفق - ServiceId: {serviceId}, Price: {calculatedPrice:N0}");

                return new ServiceCalculationResult
                {
                    Success = true,
                    CalculatedPrice = calculatedPrice,
                    Details = details,
                    CalculationFormula = calculationFormula,
                    HasOverride = overrideTechnicalFactor.HasValue || overrideProfessionalFactor.HasValue,
                    FinancialYear = currentFinancialYear
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CALCULATION] خطا در محاسبه - ServiceId: {serviceId}, Error: {ex.Message}");
                Console.WriteLine($"❌ [CALCULATION] Stack Trace: {ex.StackTrace}");
                return new ServiceCalculationResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// بررسی اینکه آیا خدمت در دپارتمان مشترک است
        /// </summary>
        public async Task<bool> IsServiceSharedInDepartmentAsync(int serviceId, int departmentId, ApplicationDbContext context)
        {
            return await context.SharedServices
                .AnyAsync(ss => ss.ServiceId == serviceId && 
                               ss.DepartmentId == departmentId && 
                               !ss.IsDeleted && ss.IsActive);
        }

        #endregion

        #region Advanced Calculation Methods

        /// <summary>
        /// محاسبه قیمت با تخفیف
        /// </summary>
        public decimal CalculateServicePriceWithDiscount(decimal basePrice, decimal discountPercent)
        {
            if (discountPercent < 0 || discountPercent > 100)
                throw new ArgumentException("درصد تخفیف باید بین 0 تا 100 باشد");

            return basePrice * (1 - discountPercent / 100);
        }

        /// <summary>
        /// محاسبه قیمت با مالیات
        /// </summary>
        public decimal CalculateServicePriceWithTax(decimal basePrice, decimal taxPercent)
        {
            if (taxPercent < 0)
                throw new ArgumentException("درصد مالیات نمی‌تواند منفی باشد");

            return basePrice * (1 + taxPercent / 100);
        }

        /// <summary>
        /// محاسبه قیمت با منطق هشتگ
        /// </summary>
        public decimal CalculateServicePriceWithHashtagLogic(Service service, ApplicationDbContext context, DateTime? date = null)
        {
            return CalculateServicePriceWithFactorSettings(service, context, date);
        }

        /// <summary>
        /// محاسبه قیمت با Override دپارتمان
        /// </summary>
        public decimal CalculateServicePriceWithDepartmentOverride(Service service, int departmentId, ApplicationDbContext context, DateTime? date = null)
        {
            return CalculateServicePriceWithFactorSettings(service, context, date, departmentId);
        }

        /// <summary>
        /// محاسبه قیمت خدمت با جزئیات کامل
        /// </summary>
        public ServiceCalculationDetails CalculateServicePriceWithDetails(Service service, ApplicationDbContext context,
            DateTime? date = null, int? departmentId = null, int? financialYear = null)
        {
            var calculationDate = date ?? DateTime.Now;
            var currentFinancialYear = financialYear ?? GetCurrentFinancialYear(calculationDate);

            // بررسی فریز بودن سال مالی
            if (IsFinancialYearFrozen(currentFinancialYear, context))
                throw new InvalidOperationException($"سال مالی {currentFinancialYear} فریز شده است و نمی‌توان محاسبات جدید انجام داد.");

            // دریافت ضریب حرفه‌ای (همیشه ثابت برای همه خدمات)
            var professionalFactor = context.FactorSettings
                .Where(fs => fs.FactorType == ServiceComponentType.Professional &&
                            fs.IsHashtagged == false &&
                            fs.FinancialYear == currentFinancialYear &&
                            fs.IsActive && !fs.IsDeleted && !fs.IsFrozen &&
                            fs.EffectiveFrom <= calculationDate &&
                            (fs.EffectiveTo == null || fs.EffectiveTo >= calculationDate))
                .OrderByDescending(fs => fs.EffectiveFrom)
                .FirstOrDefault();

            if (professionalFactor == null)
                throw new InvalidOperationException($"ضریب حرفه‌ای برای سال مالی {currentFinancialYear} یافت نشد. لطفاً ابتدا کای حرفه‌ای مربوطه را تعریف کنید.");

            // دریافت ضریب فنی (بر اساس نوع خدمت)
            var technicalFactor = context.FactorSettings
                .Where(fs => fs.FactorType == ServiceComponentType.Technical &&
                            fs.IsHashtagged == service.IsHashtagged &&
                            fs.FinancialYear == currentFinancialYear &&
                            fs.IsActive && !fs.IsDeleted && !fs.IsFrozen &&
                            fs.EffectiveFrom <= calculationDate &&
                            (fs.EffectiveTo == null || fs.EffectiveTo >= calculationDate))
                .OrderByDescending(fs => fs.EffectiveFrom)
                .FirstOrDefault();

            if (technicalFactor == null)
                throw new InvalidOperationException($"ضریب فنی برای خدمات {(service.IsHashtagged ? "هشتگ‌دار" : "بدون هشتگ")} در سال مالی {currentFinancialYear} یافت نشد. لطفاً ابتدا کای فنی مربوطه را تعریف کنید.");

            // بررسی Override دپارتمان
            decimal finalTechnicalFactor = technicalFactor.Value;
            decimal finalProfessionalFactor = professionalFactor.Value;
            bool hasDepartmentOverride = false;

            if (departmentId.HasValue)
            {
                var departmentOverride = context.SharedServices
                    .FirstOrDefault(ss => ss.ServiceId == service.ServiceId && 
                                        ss.DepartmentId == departmentId.Value && 
                                        !ss.IsDeleted && ss.IsActive);

                if (departmentOverride != null)
                {
                    if (departmentOverride.OverrideTechnicalFactor.HasValue)
                    {
                        finalTechnicalFactor = departmentOverride.OverrideTechnicalFactor.Value;
                        hasDepartmentOverride = true;
                    }
                    if (departmentOverride.OverrideProfessionalFactor.HasValue)
                    {
                        finalProfessionalFactor = departmentOverride.OverrideProfessionalFactor.Value;
                        hasDepartmentOverride = true;
                    }
                }
            }

            // بررسی وجود اجزای محاسباتی
            if (service.ServiceComponents == null || !service.ServiceComponents.Any())
                throw new InvalidOperationException($"خدمت {service.Title} دارای اجزای محاسباتی نیست. لطفاً ابتدا اجزای فنی و حرفه‌ای را تعریف کنید.");

            var technicalComponent = service.ServiceComponents
                .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical && sc.IsActive && !sc.IsDeleted);
            var professionalComponent = service.ServiceComponents
                .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional && sc.IsActive && !sc.IsDeleted);

            if (technicalComponent == null)
                throw new InvalidOperationException($"جزء فنی برای خدمت {service.Title} یافت نشد.");
            if (professionalComponent == null)
                throw new InvalidOperationException($"جزء حرفه‌ای برای خدمت {service.Title} یافت نشد.");

            decimal technicalAmount = technicalComponent.Coefficient * finalTechnicalFactor;
            decimal professionalAmount = professionalComponent.Coefficient * finalProfessionalFactor;
            decimal totalAmount = technicalAmount + professionalAmount;

            return new ServiceCalculationDetails
            {
                ServiceId = service.ServiceId,
                ServiceTitle = service.Title,
                ServiceCode = service.ServiceCode,
                IsHashtagged = service.IsHashtagged,
                TechnicalPart = technicalComponent.Coefficient,
                ProfessionalPart = professionalComponent.Coefficient,
                TechnicalFactor = finalTechnicalFactor,
                ProfessionalFactor = finalProfessionalFactor,
                TechnicalAmount = technicalAmount,
                ProfessionalAmount = professionalAmount,
                TotalAmount = totalAmount,
                HasDepartmentOverride = hasDepartmentOverride,
                DepartmentId = departmentId,
                CalculationDate = calculationDate
            };
        }

        #endregion

        /// <summary>
        /// فریز کردن محاسبات یک سال مالی
        /// </summary>
        /// <param name="financialYear">سال مالی</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>تعداد رکوردهای فریز شده</returns>
        public int FreezeFinancialYearCalculations(int financialYear, ApplicationDbContext context, string userId)
        {
            var factorsToFreeze = context.FactorSettings
                .Where(fs => fs.FinancialYear == financialYear && 
                            fs.IsActive && !fs.IsDeleted && 
                            !fs.IsFrozen)
                .ToList();

            foreach (var factor in factorsToFreeze)
            {
                factor.IsFrozen = true;
                factor.FrozenAt = DateTime.Now;
                factor.FrozenByUserId = userId;
                factor.UpdatedAt = DateTime.Now;
                factor.UpdatedByUserId = userId;
            }

            return factorsToFreeze.Count;
        }

        /// <summary>
        /// بررسی وضعیت فریز بودن یک سال مالی
        /// </summary>
        /// <param name="financialYear">سال مالی</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>وضعیت فریز</returns>
        public bool IsFinancialYearFrozen(int financialYear, ApplicationDbContext context)
        {
            return context.FactorSettings
                .Any(fs => fs.FinancialYear == financialYear && 
                          fs.IsActive && !fs.IsDeleted && 
                          fs.IsFrozen);
        }

        /// <summary>
        /// دریافت آمار سال مالی
        /// </summary>
        /// <param name="financialYear">سال مالی</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>آمار سال مالی</returns>
        public FinancialYearStats GetFinancialYearStats(int financialYear, ApplicationDbContext context)
        {
            var factors = context.FactorSettings
                .Where(fs => fs.FinancialYear == financialYear && !fs.IsDeleted)
                .ToList();

            return new FinancialYearStats
            {
                FinancialYear = financialYear,
                TotalFactors = factors.Count,
                ActiveFactors = factors.Count(f => f.IsActive),
                FrozenFactors = factors.Count(f => f.IsFrozen),
                ProfessionalFactors = factors.Count(f => f.FactorType == ServiceComponentType.Professional),
                TechnicalFactors = factors.Count(f => f.FactorType == ServiceComponentType.Technical),
                HashtaggedFactors = factors.Count(f => f.IsHashtagged),
                NonHashtaggedFactors = factors.Count(f => !f.IsHashtagged)
            };
        }
    }


    /// <summary>
    /// نتیجه بررسی کای‌های مورد نیاز
    /// </summary>
    public class FactorValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public FactorSetting ProfessionalFactor { get; set; }
        public FactorSetting TechnicalFactorHashtagged { get; set; }
        public FactorSetting TechnicalFactorNonHashtagged { get; set; }
    }

    /// <summary>
    /// آمار سال مالی
    /// </summary>
    public class FinancialYearStats
    {
        public int FinancialYear { get; set; }
        public int TotalFactors { get; set; }
        public int ActiveFactors { get; set; }
        public int FrozenFactors { get; set; }
        public int ProfessionalFactors { get; set; }
        public int TechnicalFactors { get; set; }
        public int HashtaggedFactors { get; set; }
        public int NonHashtaggedFactors { get; set; }
    }
}