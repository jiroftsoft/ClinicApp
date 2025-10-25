using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.ClinicAdmin
{
    /// <summary>
    /// DTO برای دپارتمان
    /// </summary>
    public class DepartmentDto
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }

    /// <summary>
    /// DTO برای خدمت
    /// </summary>
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool IsHashtagged { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
    }
}
