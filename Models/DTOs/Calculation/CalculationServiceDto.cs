using System;

namespace ClinicApp.Models.DTOs.Calculation
{
    /// <summary>
    /// DTO برای سرویس در محاسبات تعرفه بیمه
    /// </summary>
    public class CalculationServiceDto
    {
        public int ServiceId { get; set; }
        public int ServiceCategoryId { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
