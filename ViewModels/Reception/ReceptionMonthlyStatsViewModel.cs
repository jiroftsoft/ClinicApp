using System;
using System.Collections.Generic;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel for monthly reception statistics
    /// </summary>
    public class ReceptionMonthlyStatsViewModel
    {
        /// <summary>
        /// Month
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Year
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Total receptions count
        /// </summary>
        public int TotalReceptions { get; set; }

        /// <summary>
        /// Emergency receptions count
        /// </summary>
        public int EmergencyReceptions { get; set; }

        /// <summary>
        /// Online receptions count
        /// </summary>
        public int OnlineReceptions { get; set; }

        /// <summary>
        /// Normal receptions count
        /// </summary>
        public int NormalReceptions { get; set; }

        /// <summary>
        /// Special receptions count
        /// </summary>
        public int SpecialReceptions { get; set; }

        /// <summary>
        /// Total revenue
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Insurance covered amount
        /// </summary>
        public decimal InsuranceCovered { get; set; }

        /// <summary>
        /// Patient paid amount
        /// </summary>
        public decimal PatientPaid { get; set; }

        /// <summary>
        /// Average reception value
        /// </summary>
        public decimal AverageReceptionValue { get; set; }

        /// <summary>
        /// Daily statistics
        /// </summary>
        public List<ClinicApp.ViewModels.Reception.ReceptionDailyStatsViewModel> DailyStats { get; set; } = new List<ClinicApp.ViewModels.Reception.ReceptionDailyStatsViewModel>();

        /// <summary>
        /// Doctor statistics
        /// </summary>
        public List<ClinicApp.ViewModels.Reception.ReceptionDoctorStatsViewModel> DoctorStats { get; set; } = new List<ClinicApp.ViewModels.Reception.ReceptionDoctorStatsViewModel>();

        /// <summary>
        /// Service statistics
        /// </summary>
        public List<ReceptionServiceStatsViewModel> ServiceStats { get; set; } = new List<ReceptionServiceStatsViewModel>();
    }


    /// <summary>
    /// ViewModel for service reception statistics
    /// </summary>
    public class ReceptionServiceStatsViewModel
    {
        /// <summary>
        /// Service ID
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Service code
        /// </summary>
        public string ServiceCode { get; set; }

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total revenue
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Average price
        /// </summary>
        public decimal AveragePrice { get; set; }
    }
}
