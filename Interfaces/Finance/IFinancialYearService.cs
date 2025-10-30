using System;

namespace ClinicApp.Interfaces.Finance
{
    /// <summary>
    /// اینترفیس سرویس سال مالی
    /// </summary>
    public interface IFinancialYearService
    {
        /// <summary>
        /// دریافت سال مالی جاری
        /// </summary>
        /// <returns>سال مالی شمسی</returns>
        int GetCurrentYear();
    }
}
