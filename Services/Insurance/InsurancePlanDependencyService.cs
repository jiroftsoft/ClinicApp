using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Insurance;


namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// سرویس بررسی وابستگی‌های طرح بیمه
    /// این سرویس برای جلوگیری از حذف طرح‌های بیمه‌ای که وابستگی دارند طراحی شده است
    /// </summary>
    public interface IInsurancePlanDependencyService
    {
        /// <summary>
        /// بررسی وابستگی‌های طرح بیمه
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>نتیجه بررسی وابستگی‌ها</returns>
        Task<ServiceResult<InsurancePlanDependencyInfo>> CheckDependenciesAsync(int planId);

        /// <summary>
        /// بررسی امکان حذف طرح بیمه
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>نتیجه بررسی امکان حذف</returns>
        Task<ServiceResult<bool>> CanDeletePlanAsync(int planId);
    }

    /// <summary>
    /// اطلاعات وابستگی‌های طرح بیمه
    /// </summary>
    public class InsurancePlanDependencyInfo
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public string PlanCode { get; set; }
        
        // وابستگی‌های خدمات
        public int PlanServicesCount { get; set; }
        public List<PlanServiceInfo> PlanServices { get; set; } = new List<PlanServiceInfo>();
        
        // وابستگی‌های بیماران
        public int PatientInsurancesCount { get; set; }
        public List<PatientInsuranceInfo> PatientInsurances { get; set; } = new List<PatientInsuranceInfo>();
        
        // وابستگی‌های محاسبات بیمه
        public int InsuranceCalculationsCount { get; set; }
        public List<InsuranceCalculationInfo> InsuranceCalculations { get; set; } = new List<InsuranceCalculationInfo>();
        
        // خلاصه وابستگی‌ها
        public bool HasDependencies => PlanServicesCount > 0 || PatientInsurancesCount > 0 || InsuranceCalculationsCount > 0;
        public string DependencySummary => GetDependencySummary();
        
        private string GetDependencySummary()
        {
            var dependencies = new List<string>();
            
            if (PlanServicesCount > 0)
                dependencies.Add($"{PlanServicesCount} خدمت");
            
            if (PatientInsurancesCount > 0)
                dependencies.Add($"{PatientInsurancesCount} بیمار");
            
            if (InsuranceCalculationsCount > 0)
                dependencies.Add($"{InsuranceCalculationsCount} محاسبه بیمه");
            
            return dependencies.Count > 0 ? string.Join("، ", dependencies) : "هیچ وابستگی";
        }
    }

    /// <summary>
    /// اطلاعات خدمت طرح بیمه
    /// </summary>
    public class PlanServiceInfo
    {
        public int PlanServiceId { get; set; }
        public string ServiceCategoryName { get; set; }
        public bool IsCovered { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// اطلاعات بیمه بیمار
    /// </summary>
    public class PatientInsuranceInfo
    {
        public int PatientInsuranceId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string PatientNationalCode { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// اطلاعات محاسبه بیمه
    /// </summary>
    public class InsuranceCalculationInfo
    {
        public int InsuranceCalculationId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal ServiceAmount { get; set; }
        public decimal InsuranceShare { get; set; }
        public decimal PatientShare { get; set; }
        public DateTime CalculationDate { get; set; }
        public bool IsValid { get; set; }
    }

}

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// پیاده‌سازی سرویس بررسی وابستگی‌های طرح بیمه
    /// </summary>
    public class InsurancePlanDependencyService : IInsurancePlanDependencyService
    {
        private readonly IInsurancePlanRepository _insurancePlanRepository;
        private readonly IPlanServiceRepository _planServiceRepository;
        private readonly IPatientInsuranceRepository _patientInsuranceRepository;
        private readonly IInsuranceCalculationRepository _insuranceCalculationRepository;

        public InsurancePlanDependencyService(
            IInsurancePlanRepository insurancePlanRepository,
            IPlanServiceRepository planServiceRepository,
            IPatientInsuranceRepository patientInsuranceRepository,
            IInsuranceCalculationRepository insuranceCalculationRepository)
        {
            _insurancePlanRepository = insurancePlanRepository;
            _planServiceRepository = planServiceRepository;
            _patientInsuranceRepository = patientInsuranceRepository;
            _insuranceCalculationRepository = insuranceCalculationRepository;
        }

        /// <summary>
        /// بررسی وابستگی‌های طرح بیمه
        /// </summary>
        public async Task<ServiceResult<InsurancePlanDependencyInfo>> CheckDependenciesAsync(int planId)
        {
            try
            {
                // دریافت اطلاعات طرح بیمه
                var plan = await _insurancePlanRepository.GetByIdAsync(planId);
                if (plan == null)
                {
                    return ServiceResult<InsurancePlanDependencyInfo>.Failed("طرح بیمه یافت نشد");
                }

                var dependencyInfo = new InsurancePlanDependencyInfo
                {
                    PlanId = plan.InsurancePlanId,
                    PlanName = plan.Name,
                    PlanCode = plan.PlanCode
                };

                // بررسی وابستگی‌های خدمات
                var planServices = await _planServiceRepository.GetByPlanIdAsync(planId);
                dependencyInfo.PlanServicesCount = planServices.Count(ps => !ps.IsDeleted);
                dependencyInfo.PlanServices = planServices
                    .Where(ps => !ps.IsDeleted)
                    .Select(ps => new PlanServiceInfo
                    {
                        PlanServiceId = ps.PlanServiceId,
                        ServiceCategoryName = ps.ServiceCategory?.Title ?? "نامشخص",
                        IsCovered = ps.IsCovered,
                        CreatedAt = ps.CreatedAt
                    })
                    .ToList();

                // بررسی وابستگی‌های بیماران
                var patientInsurances = await _patientInsuranceRepository.GetByPlanIdAsync(planId);
                dependencyInfo.PatientInsurancesCount = patientInsurances.Count(pi => !pi.IsDeleted);
                dependencyInfo.PatientInsurances = patientInsurances
                    .Where(pi => !pi.IsDeleted)
                    .Select(pi => new PatientInsuranceInfo
                    {
                        PatientInsuranceId = pi.PatientInsuranceId,
                        PatientId = pi.PatientId,
                        PatientName = pi.Patient?.FullName ?? "نامشخص",
                        PatientNationalCode = pi.Patient?.NationalCode ?? "نامشخص",
                        IsPrimary = pi.IsPrimary,
                        CreatedAt = pi.CreatedAt
                    })
                    .ToList();

                // بررسی وابستگی‌های محاسبات بیمه
                var insuranceCalculations = await _insuranceCalculationRepository.GetByPlanIdAsync(planId);
                dependencyInfo.InsuranceCalculationsCount = insuranceCalculations.Count(ic => !ic.IsDeleted);
                dependencyInfo.InsuranceCalculations = insuranceCalculations
                    .Where(ic => !ic.IsDeleted)
                    .Select(ic => new InsuranceCalculationInfo
                    {
                        InsuranceCalculationId = ic.InsuranceCalculationId,
                        PatientId = ic.PatientId,
                        PatientName = ic.Patient?.FullName ?? "نامشخص",
                        ServiceId = ic.ServiceId,
                        ServiceName = ic.Service?.Title ?? "نامشخص",
                        ServiceAmount = ic.ServiceAmount,
                        InsuranceShare = ic.InsuranceShare,
                        PatientShare = ic.PatientShare,
                        CalculationDate = ic.CalculationDate,
                        IsValid = ic.IsValid
                    })
                    .ToList();


                return ServiceResult<InsurancePlanDependencyInfo>.Successful(dependencyInfo);
            }
            catch (Exception ex)
            {
                return ServiceResult<InsurancePlanDependencyInfo>.Failed($"خطا در بررسی وابستگی‌ها: {ex.Message}");
            }
        }

        /// <summary>
        /// بررسی امکان حذف طرح بیمه
        /// </summary>
        public async Task<ServiceResult<bool>> CanDeletePlanAsync(int planId)
        {
            try
            {
                var dependencyResult = await CheckDependenciesAsync(planId);
                if (!dependencyResult.Success)
                {
                    return ServiceResult<bool>.Failed(dependencyResult.Message);
                }

                var canDelete = !dependencyResult.Data.HasDependencies;
                var message = canDelete 
                    ? "طرح بیمه قابل حذف است" 
                    : $"طرح بیمه قابل حذف نیست. وابستگی‌ها: {dependencyResult.Data.DependencySummary}";

                return ServiceResult<bool>.Successful(canDelete, message);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failed($"خطا در بررسی امکان حذف: {ex.Message}");
            }
        }
    }
}
