using System;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// دامنه‌های ضریب (طبق مصوبه 1404)
    /// برای تمایز دقیق بین انواع مختلف ضرایب
    /// </summary>
    public enum FactorScope
    {
        /// <summary>
        /// کای فنی عمومی بدون هشتگ
        /// </summary>
        General_NoHash = 1,

        /// <summary>
        /// کای فنی هشتگ‌دار (کدهای ۱ تا ۷)
        /// </summary>
        Hash_1_7 = 2,

        /// <summary>
        /// کای فنی هشتگ‌دار (کدهای ۸ و ۹)
        /// </summary>
        Hash_8_9 = 3,

        /// <summary>
        /// کای فنی دندانپزشکی
        /// </summary>
        Dent_Technical = 4,

        /// <summary>
        /// کای فنی مواد و لوازم مصرفی دندانپزشکی
        /// </summary>
        Dent_Consumables = 5,

        /// <summary>
        /// کای حرفه‌ای بدون هشتگ
        /// </summary>
        Prof_NoHash = 6,

        /// <summary>
        /// کای حرفه‌ای هشتگ‌دار
        /// </summary>
        Prof_Hash = 7,

        /// <summary>
        /// کای حرفه‌ای دندانپزشکی
        /// </summary>
        Prof_Dental = 8
    }
}
