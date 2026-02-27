using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.DTOs.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Repository مطالبات بیمه — CRUD و گزارش‌گیری Aging / تفکیک بیمه‌گذار
    /// </summary>
    public interface IInsuranceClaimRepository
    {
        Task<InsuranceClaim> GetByIdAsync(int id);
        Task<InsuranceClaim> GetByIdWithDetailsAsync(int id);
        Task<List<InsuranceClaim>> GetByDateRangeAsync(DateTime start, DateTime end, int? insuranceProviderId = null, ClaimStatus? status = null);
        Task<List<InsuranceClaim>> GetByPlanIdAsync(int planId);
        Task<List<InsuranceClaim>> GetByBatchIdAsync(int batchId);
        Task<InsuranceClaim> AddAsync(InsuranceClaim entity);
        Task<InsuranceClaim> UpdateAsync(InsuranceClaim entity);
        Task<bool> SoftDeleteAsync(int id, string deletedByUserId);

        /// <summary>
        /// گزارش Aging مطالبات (معوق) بر اساس بازه سنی از تاریخ ارسال
        /// </summary>
        Task<List<InsuranceClaimAgingRow>> GetAgingReportAsync(DateTime? asOfDate = null);

        /// <summary>
        /// تحلیل به تفکیک بیمه‌گذار در بازه تاریخ
        /// </summary>
        Task<List<InsuranceProviderBreakdownRow>> GetProviderBreakdownAsync(DateTime start, DateTime end);
    }
}
