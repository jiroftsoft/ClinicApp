using System;

namespace ClinicApp.ViewModels.Insurance.InsuranceTariff
{
    /// <summary>
    /// DTO برای نمایش لیست تعرفه‌ها - بهینه‌سازی شده برای performance
    /// </summary>
    public class TariffIndexDto
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int InsurancePlanId { get; set; }
        public string InsurancePlanName { get; set; }
        public int InsuranceProviderId { get; set; }
        public string InsuranceProviderName { get; set; }
        public decimal TariffPrice { get; set; }
        public decimal PatientShare { get; set; }
        public decimal InsurerShare { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO برای آمار تعرفه‌ها - بهینه‌سازی شده
    /// </summary>
    public class TariffStatisticsDto
    {
        public int TotalTariffs { get; set; }
        public int ActiveTariffs { get; set; }
        public int InactiveTariffs { get; set; }
        public decimal AverageTariffPrice { get; set; }
        public decimal TotalTariffValue { get; set; }
        public int PlansWithTariffs { get; set; }
        public int ServicesWithTariffs { get; set; }
    }

    /// <summary>
    /// DTO برای جزئیات تعرفه - بهینه‌سازی شده
    /// </summary>
    public class TariffDetailsDto
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }
        public int InsurancePlanId { get; set; }
        public string InsurancePlanName { get; set; }
        public int InsuranceProviderId { get; set; }
        public string InsuranceProviderName { get; set; }
        public decimal TariffPrice { get; set; }
        public decimal PatientShare { get; set; }
        public decimal InsurerShare { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
