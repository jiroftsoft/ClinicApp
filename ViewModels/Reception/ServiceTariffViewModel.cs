using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش تعرفه خدمات
    /// </summary>
    public class ServiceTariffViewModel
    {
        /// <summary>
        /// شناسه تعرفه
        /// </summary>
        public int TariffId { get; set; }

        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// نام خدمت
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// شناسه بیمه
        /// </summary>
        public int InsuranceId { get; set; }

        /// <summary>
        /// نام بیمه
        /// </summary>
        public string InsuranceName { get; set; }

        /// <summary>
        /// مبلغ تعرفه
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TariffAmount { get; set; }

        /// <summary>
        /// درصد پوشش بیمه
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:P0}")]
        public decimal CoveragePercentage { get; set; }

        /// <summary>
        /// مبلغ سهم بیمه
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal InsuranceShare { get; set; }

        /// <summary>
        /// مبلغ سهم بیمار
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PatientShare { get; set; }

        /// <summary>
        /// آیا فعال است
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تاریخ شروع اعتبار
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// تاریخ پایان اعتبار
        /// </summary>
        public DateTime? ValidTo { get; set; }
    }
}
