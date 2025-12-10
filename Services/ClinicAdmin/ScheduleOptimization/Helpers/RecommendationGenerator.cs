using System.Collections.Generic;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Helpers
{
    /// <summary>
    /// Helper Class برای تولید توصیه‌های بهینه‌سازی
    /// 
    /// مسئولیت (SRP):
    /// - تولید توصیه‌های بهینه‌سازی
    /// - تولید پیام‌های راهنما
    /// - تولید پیشنهادات بهبود
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط تولید توصیه‌ها
    /// - Static Methods: بدون state، thread-safe
    /// </summary>
    public static class RecommendationGenerator
    {
        /// <summary>
        /// تولید توصیه‌های بهینه‌سازی بر اساس وضعیت بار کاری
        /// </summary>
        /// <param name="status">وضعیت بار کاری</param>
        /// <param name="appointmentCount">تعداد نوبت‌ها</param>
        /// <param name="maxCapacity">حداکثر ظرفیت</param>
        /// <param name="breakTimeMinutes">زمان استراحت (دقیقه)</param>
        /// <returns>لیست توصیه‌ها</returns>
        public static List<string> GenerateRecommendations(
            WorkloadBalanceStatus status, 
            int appointmentCount, 
            int maxCapacity, 
            int breakTimeMinutes)
        {
            var recommendations = new List<string>();

            switch (status)
            {
                case WorkloadBalanceStatus.Light:
                    recommendations.Add("افزایش تعداد نوبت‌ها برای استفاده بهتر از زمان");
                    recommendations.Add("اضافه کردن خدمات مشاوره‌ای یا ویزیت‌های کوتاه");
                    recommendations.Add("بررسی امکان اضافه کردن روز کاری");
                    break;

                case WorkloadBalanceStatus.Balanced:
                    recommendations.Add("حفظ وضعیت فعلی - بار کاری در حد مطلوب است");
                    recommendations.Add("بررسی امکان بهبود کیفیت خدمات");
                    recommendations.Add("بررسی رضایت بیماران");
                    break;

                case WorkloadBalanceStatus.Heavy:
                    recommendations.Add("کاهش تعداد نوبت‌ها برای حفظ کیفیت خدمات");
                    recommendations.Add("افزایش زمان استراحت بین نوبت‌ها");
                    recommendations.Add("استفاده از سیستم نوبت‌دهی هوشمند");
                    recommendations.Add("بررسی امکان توزیع نوبت‌ها در روزهای دیگر");
                    break;

                case WorkloadBalanceStatus.Overloaded:
                    recommendations.Add("کاهش فوری تعداد نوبت‌ها - بار کاری بیش از حد است");
                    recommendations.Add("افزایش زمان استراحت برای حفظ سلامت پزشک");
                    recommendations.Add("استفاده از پزشک کمکی یا دستیار");
                    recommendations.Add("بررسی مجدد برنامه کاری و بازنگری کامل");
                    recommendations.Add("در نظر گیری تعطیلات یا کاهش ساعات کاری");
                    break;

                case WorkloadBalanceStatus.NoWorkDay:
                    recommendations.Add("تعریف برنامه کاری برای این روز");
                    break;
            }

            // توصیه‌های عمومی بر اساس زمان استراحت
            if (breakTimeMinutes < 60)
            {
                recommendations.Add("افزایش زمان استراحت برای حفظ کیفیت خدمات و سلامت پزشک");
            }

            // توصیه‌های عمومی بر اساس ظرفیت
            if (maxCapacity > 0)
            {
                var utilizationPercentage = (decimal)appointmentCount / maxCapacity * 100;
                
                if (utilizationPercentage > 90)
                {
                    recommendations.Add("ظرفیت تقریباً کامل است - در نظر گیری رزرو اضافی");
                }
                else if (utilizationPercentage < 30)
                {
                    recommendations.Add("ظرفیت استفاده نشده - بررسی علل کاهش نوبت‌ها");
                }
            }

            return recommendations;
        }

        /// <summary>
        /// تولید توصیه‌های بهینه‌سازی هزینه
        /// </summary>
        /// <param name="revenue">درآمد</param>
        /// <param name="costs">هزینه‌ها</param>
        /// <param name="appointmentCount">تعداد نوبت‌ها</param>
        /// <returns>لیست توصیه‌ها</returns>
        public static List<string> GenerateCostOptimizationRecommendations(
            decimal revenue, 
            decimal costs, 
            int appointmentCount)
        {
            var recommendations = new List<string>();

            if (costs > revenue)
            {
                recommendations.Add("هزینه‌ها بیشتر از درآمد است - نیاز به بررسی فوری");
                recommendations.Add("کاهش هزینه‌های عملیاتی");
                recommendations.Add("افزایش تعداد نوبت‌ها یا قیمت خدمات");
            }
            else
            {
                var profitMargin = ((revenue - costs) / revenue) * 100;
                
                if (profitMargin < 10)
                {
                    recommendations.Add("حاشیه سود پایین است - نیاز به بهینه‌سازی");
                }
                else if (profitMargin > 50)
                {
                    recommendations.Add("حاشیه سود بالا است - بررسی امکان کاهش قیمت یا بهبود خدمات");
                }
            }

            if (appointmentCount > 0)
            {
                var revenuePerAppointment = revenue / appointmentCount;
                recommendations.Add($"درآمد متوسط هر نوبت: {revenuePerAppointment:N0} ریال");
            }

            return recommendations;
        }

        /// <summary>
        /// تولید توصیه‌های بهینه‌سازی زمان استراحت
        /// </summary>
        /// <param name="totalWorkMinutes">کل زمان کار (دقیقه)</param>
        /// <param name="breakTimeMinutes">زمان استراحت فعلی (دقیقه)</param>
        /// <param name="minimumBreakTime">حداقل زمان استراحت مورد نیاز (دقیقه)</param>
        /// <returns>لیست توصیه‌ها</returns>
        public static List<string> GenerateBreakTimeRecommendations(
            int totalWorkMinutes, 
            int breakTimeMinutes, 
            int minimumBreakTime)
        {
            var recommendations = new List<string>();

            if (breakTimeMinutes < minimumBreakTime)
            {
                var deficit = minimumBreakTime - breakTimeMinutes;
                recommendations.Add($"افزایش زمان استراحت به میزان {deficit} دقیقه برای رعایت استانداردهای کار");
            }

            if (totalWorkMinutes > 480) // بیش از 8 ساعت
            {
                recommendations.Add("زمان کار بیش از 8 ساعت است - در نظر گیری استراحت بیشتر");
            }

            if (breakTimeMinutes < 30)
            {
                recommendations.Add("زمان استراحت کمتر از 30 دقیقه است - حداقل 30 دقیقه استراحت توصیه می‌شود");
            }

            return recommendations;
        }
    }
}

