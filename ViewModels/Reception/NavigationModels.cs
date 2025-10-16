using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// مدل ناوبری تخصصی برای ماژول پذیرش - Strongly Typed
    /// </summary>
    public class NavigationViewModel
    {
        public List<NavigationSection> Sections { get; set; } = new List<NavigationSection>();
        public string CurrentController { get; set; }
        public string CurrentAction { get; set; }
        public bool IsSidebarOpen { get; set; }
    }

    /// <summary>
    /// بخش ناوبری - Strongly Typed
    /// </summary>
    public class NavigationSection
    {
        public string SectionId { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string CssClass { get; set; }
        public List<NavigationItem> Items { get; set; } = new List<NavigationItem>();
        public bool IsExpanded { get; set; } = true;
        public int Order { get; set; }
    }

    /// <summary>
    /// آیتم ناوبری - Strongly Typed
    /// </summary>
    public class NavigationItem
    {
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Area { get; set; }
        public object RouteValues { get; set; }
        public string CssClass { get; set; }
        public bool IsActive { get; set; }
        public bool IsVisible { get; set; } = true;
        public int Order { get; set; }
        public string BadgeText { get; set; }
        public string BadgeClass { get; set; }
        public string Tooltip { get; set; }
        public string Permission { get; set; }
        public bool RequiresAuthentication { get; set; } = true;
    }

    /// <summary>
    /// مدل ناوبری پزشکی تخصصی
    /// </summary>
    public class MedicalNavigationViewModel : NavigationViewModel
    {
        public string UserRole { get; set; }
        public string UserName { get; set; }
        public string ClinicName { get; set; }
        public DateTime CurrentDate { get; set; } = DateTime.Now;
        public string CurrentShift { get; set; }
        public bool IsEmergencyMode { get; set; }
        public List<QuickAction> QuickActions { get; set; } = new List<QuickAction>();
    }

    /// <summary>
    /// عملیات سریع - Strongly Typed
    /// </summary>
    public class QuickAction
    {
        public string ActionId { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public object RouteValues { get; set; }
        public string CssClass { get; set; }
        public bool IsVisible { get; set; } = true;
        public string Permission { get; set; }
        public string Tooltip { get; set; }
    }

    /// <summary>
    /// مدل ناوبری دپارتمان‌ها
    /// </summary>
    public class DepartmentNavigationViewModel
    {
        public List<DepartmentNavigationItem> Departments { get; set; } = new List<DepartmentNavigationItem>();
        public int SelectedDepartmentId { get; set; }
        public string SelectedDepartmentName { get; set; }
    }

    /// <summary>
    /// آیتم ناوبری دپارتمان
    /// </summary>
    public class DepartmentNavigationItem
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Icon { get; set; }
        public string CssClass { get; set; }
        public bool IsActive { get; set; }
        public int PatientCount { get; set; }
        public int ReceptionCount { get; set; }
        public string Status { get; set; }
    }
}
