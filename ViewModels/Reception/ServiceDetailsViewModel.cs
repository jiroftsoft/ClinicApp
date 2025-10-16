using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    public class ServiceDetailsViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }
        public decimal BasePrice { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public bool RequiresSpecialization { get; set; }
        public bool RequiresDoctor { get; set; }
        public string DisplayName { get; set; }
        public string Title { get; set; }
        public string ServiceCategoryTitle { get; set; }
        public string DepartmentTitle { get; set; }
        public string ClinicTitle { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string DeletedBy { get; set; }
        public string CreatedAtShamsi { get; set; }
        public string UpdatedAtShamsi { get; set; }
        public string DeletedAtShamsi { get; set; }
        public DateTime? LastUsageDate { get; set; }
        public string LastUsageDateShamsi { get; set; }
        public int UsageCount { get; set; }
        public decimal TotalRevenue { get; set; }
        
        // Additional properties for compatibility
        public int ServiceCategoryId { get; set; }
        public int DepartmentId { get; set; }
        public int ClinicId { get; set; }
        public string DepartmentName { get; set; }
        public string ClinicName { get; set; }
        public string CreatedByUser { get; set; }
        public string UpdatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string CreatedByUserId { get; set; }
        public string UpdatedByUserId { get; set; }
        public string DeletedByUserId { get; set; }
        
        // Additional properties for ReceptionServiceManagementService
        public List<ServiceTariffViewModel> Tariffs { get; set; } = new List<ServiceTariffViewModel>();
        public string Requirements { get; set; }
        public string PreparationInstructions { get; set; }
        public int EstimatedDuration { get; set; }
        public List<string> RelatedServices { get; set; } = new List<string>();
        public bool IsInsuranceCovered { get; set; }
        public decimal InsuranceCoveragePercentage { get; set; }
    }
}
