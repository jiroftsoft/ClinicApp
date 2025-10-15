using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش نتیجه تغییر بیمه در فرم پذیرش
    /// </summary>
    public class ReceptionInsuranceUpdateResult
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// شناسه بیمه اصلی جدید
        /// </summary>
        public int? PrimaryInsuranceId { get; set; }

        /// <summary>
        /// شناسه بیمه تکمیلی جدید
        /// </summary>
        public int? SupplementaryInsuranceId { get; set; }

        /// <summary>
        /// تاریخ تغییر بیمه
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// آیا تغییر موفق بود؟
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// پیام نتیجه
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// آیا بیمه اصلی تغییر کرد؟
        /// </summary>
        public bool PrimaryInsuranceChanged => PrimaryInsuranceId.HasValue;

        /// <summary>
        /// آیا بیمه تکمیلی تغییر کرد؟
        /// </summary>
        public bool SupplementaryInsuranceChanged => SupplementaryInsuranceId.HasValue;

        /// <summary>
        /// نمایش نتیجه تغییر (فرمات شده)
        /// </summary>
        public string UpdateSummary
        {
            get
            {
                if (!Success) return "تغییر بیمه ناموفق";
                
                var changes = new List<string>();
                if (PrimaryInsuranceChanged) changes.Add("بیمه اصلی");
                if (SupplementaryInsuranceChanged) changes.Add("بیمه تکمیلی");
                
                if (changes.Count == 0) return "تغییری اعمال نشد";
                if (changes.Count == 1) return $"بیمه {changes[0]} تغییر یافت";
                return $"بیمه‌های {string.Join(" و ", changes)} تغییر یافتند";
            }
        }
    }
}
