using System;
using System.Collections.Generic;
using ClinicApp.Models.Entities;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    #region Assignment Report ViewModels

    public class AssignmentReportViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorNationalCode { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int ServiceCategoryId { get; set; }
        public string ServiceCategoryName { get; set; }
        public DateTime AssignmentDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public string AssignedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public List<AssignmentViewModel> Assignments { get; set; } = new List<AssignmentViewModel>();
        public string GeneratedBy { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class AssignmentViewModel
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public DateTime AssignmentDate { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
    }

    public class AssignmentStatisticsViewModel
    {
        public int TotalAssignments { get; set; }
        public int ActiveAssignments { get; set; }
        public int InactiveAssignments { get; set; }
        public int AssignedDoctors { get; set; }
        public int TotalDoctors { get; set; }
        public int UnassignedDoctors { get; set; }
        public int ActiveDepartments { get; set; }
        public decimal CompletionPercentage { get; set; }
        public decimal AverageAssignmentsPerDoctor { get; set; }
        public string MostAssignedDepartment { get; set; }
        public string LeastAssignedDepartment { get; set; }
        public int DepartmentId { get; set; }
        public int ServiceCategoryId { get; set; }
        public int ActiveDoctors { get; set; }
        public List<DepartmentAssignmentViewModel> DepartmentAssignments { get; set; } = new List<DepartmentAssignmentViewModel>();
        public List<ServiceCategoryAssignmentViewModel> ServiceCategoryAssignments { get; set; } = new List<ServiceCategoryAssignmentViewModel>();
        public string GeneratedBy { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class DepartmentAssignmentViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int DoctorCount { get; set; }
    }

    public class ServiceCategoryAssignmentViewModel
    {
        public int ServiceCategoryId { get; set; }
        public string ServiceCategoryName { get; set; }
        public int DoctorCount { get; set; }
    }

    public class AssignmentHistoryStatisticsViewModel
    {
        public int TotalHistoryRecords { get; set; }
        public int AssignmentsCount { get; set; }
        public int TransfersCount { get; set; }
        public int RemovalsCount { get; set; }
        public int TotalChanges { get; set; }
        public int Assignments { get; set; }
        public int Transfers { get; set; }
        public int Removals { get; set; }
        public string MostActiveMonth { get; set; }
        public string MostActiveDoctor { get; set; }
        public string MostActiveDepartment { get; set; }
        public decimal AverageChangesPerMonth { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        
        // Properties for views
        public int TodayRecords { get; set; }
        public int TotalRecords { get; set; }
        public int CriticalRecords { get; set; }
        public int ImportantRecords { get; set; }
        public int NormalRecords { get; set; }
        public int UpdatesCount { get; set; }
    }
    public class AssignmentHistoryItemViewModel
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Action { get; set; }
        public DateTime ActionDate { get; set; }
        public string ActionBy { get; set; }
    }

    #endregion

    #region Doctor Performance ViewModels

    public class DoctorPerformanceViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorNationalCode { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal CompletionRate { get; set; }
        public int TotalPatients { get; set; }
        public decimal AverageRating { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalHours { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class DoctorPerformanceData
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public decimal CompletionRate { get; set; }
        public int TotalPatients { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    #endregion

    #region Department Report ViewModels

    public class DepartmentReportViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int TotalDoctors { get; set; }
        public int ActiveDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public decimal CompletionRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DoctorSummaryViewModel> Doctors { get; set; } = new List<DoctorSummaryViewModel>();
        public List<DepartmentReportItemViewModel> Departments { get; set; } = new List<DepartmentReportItemViewModel>();
        public string GeneratedBy { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class DepartmentReportItemViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int DoctorCount { get; set; }
        public int AppointmentCount { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class DoctorSummaryViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int AppointmentsCount { get; set; }
        public decimal CompletionRate { get; set; }
    }

    #endregion

    #region Service Category Report ViewModels

    public class ServiceCategoryReportViewModel
    {
        public int ServiceCategoryId { get; set; }
        public string ServiceCategoryName { get; set; }
        public int TotalDoctors { get; set; }
        public int ActiveDoctors { get; set; }
        public int TotalServices { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ServiceSummaryViewModel> Services { get; set; } = new List<ServiceSummaryViewModel>();
        public List<ServiceCategoryReportItemViewModel> ServiceCategories { get; set; } = new List<ServiceCategoryReportItemViewModel>();
        public string GeneratedBy { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class ServiceCategoryReportItemViewModel
    {
        public int ServiceCategoryId { get; set; }
        public string ServiceCategoryName { get; set; }
        public int DoctorCount { get; set; }
        public int ServiceCount { get; set; }
    }

    public class ServiceSummaryViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int UsageCount { get; set; }
        public decimal AveragePrice { get; set; }
    }

    #endregion

    #region Schedule Report ViewModels

    public class ScheduleReportViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
        public int TotalSlots { get; set; }
        public int BookedSlots { get; set; }
        public decimal UtilizationRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ScheduleReportItemViewModel> Schedules { get; set; } = new List<ScheduleReportItemViewModel>();
        public string GeneratedBy { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class ScheduleReportItemViewModel
    {
        public int ScheduleId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int TotalSlots { get; set; }
        public int BookedSlots { get; set; }
        public decimal UtilizationRate { get; set; }
    }

    public class ScheduleStatisticsViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int TotalSchedules { get; set; }
        public int ActiveSchedules { get; set; }
        public decimal TotalHours { get; set; }
        public decimal UtilizationRate { get; set; }
    }

    public class ScheduleStatisticsData
    {
        public int ScheduledDoctors { get; set; }
        public int TotalSchedules { get; set; }
        public decimal UtilizationRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    #endregion

    #region Chart Data ViewModels

    public class ChartDataViewModel
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<ChartDatasetViewModel> Datasets { get; set; } = new List<ChartDatasetViewModel>();
    }

    public class ChartDatasetViewModel
    {
        public string Label { get; set; }
        public List<decimal> Data { get; set; } = new List<decimal>();
        public List<string> BackgroundColor { get; set; } = new List<string>();
        public List<string> BorderColor { get; set; } = new List<string>();
        public int BorderWidth { get; set; } = 1;
    }

    #endregion

    #region Statistics Data ViewModels

    public class AssignmentStatisticsData
    {
        public int TotalAssignments { get; set; }
        public int ActiveAssignments { get; set; }
        public int AssignedDoctors { get; set; }
        public decimal CompletionPercentage { get; set; }
    }

    public class AssignmentHistoryStatisticsData
    {
        public int TotalHistoryRecords { get; set; }
        public int AssignmentsCount { get; set; }
        public int TransfersCount { get; set; }
        public int RemovalsCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    #endregion

    #region Main ViewModels

    public class DoctorReportingViewModel
    {
        public int? SelectedDepartmentId { get; set; }
        public int? SelectedServiceCategoryId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ReportType { get; set; }
        public List<Department> Departments { get; set; } = new List<Department>();
        public List<ServiceCategory> ServiceCategories { get; set; } = new List<ServiceCategory>();
        public string GeneratedBy { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    #endregion

    #region Dependency ViewModels

    public class DoctorDependenciesViewModel
    {
        public bool HasActiveAppointments { get; set; }
        public bool HasActiveSchedules { get; set; }
        public bool HasActiveAssignments { get; set; }
        public int AppointmentCount { get; set; }
        public int ScheduleCount { get; set; }
        public int AssignmentCount { get; set; }
        public bool CanBeRemoved { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    #endregion

 
}
