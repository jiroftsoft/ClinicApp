using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Helpers.HtmlHelpers
{
    /// <summary>
    /// HTML Helpers تخصصی برای ناوبری - Strongly Typed
    /// </summary>
    public static class NavigationHtmlHelpers
    {
        /// <summary>
        /// ایجاد ناوبری پزشکی تخصصی - Strongly Typed
        /// </summary>
        public static MvcHtmlString MedicalNavigation(this HtmlHelper htmlHelper, MedicalNavigationViewModel model)
        {
            if (model == null)
                return MvcHtmlString.Empty;

            var sb = new StringBuilder();
            
            sb.AppendLine("<div class=\"medical-navigation-sidebar\" id=\"medicalNavigationSidebar\">");
            sb.AppendLine(htmlHelper.MedicalNavigationHeader(model).ToString());
            sb.AppendLine("<div class=\"sidebar-content\">");
            
            foreach (var section in model.Sections.OrderBy(s => s.Order))
            {
                sb.AppendLine(htmlHelper.MedicalNavigationSection(section, model).ToString());
            }
            
            sb.AppendLine(htmlHelper.MedicalQuickActions(model.QuickActions).ToString());
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            
            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// هدر ناوبری پزشکی
        /// </summary>
        public static MvcHtmlString MedicalNavigationHeader(this HtmlHelper htmlHelper, MedicalNavigationViewModel model)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("<div class=\"sidebar-header\">");
            sb.AppendLine("<div class=\"sidebar-title\">");
            sb.AppendLine($"<i class=\"fas fa-bars me-2\"></i>");
            sb.AppendLine($"<span>ناوبری پذیرش</span>");
            sb.AppendLine("</div>");
            sb.AppendLine("<button type=\"button\" class=\"sidebar-toggle\" id=\"sidebarToggle\">");
            sb.AppendLine("<i class=\"fas fa-chevron-left\"></i>");
            sb.AppendLine("</button>");
            sb.AppendLine("</div>");
            
            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// بخش ناوبری پزشکی
        /// </summary>
        public static MvcHtmlString MedicalNavigationSection(this HtmlHelper htmlHelper, NavigationSection section, MedicalNavigationViewModel model)
        {
            if (section == null || !section.Items.Any())
                return MvcHtmlString.Empty;

            var sb = new StringBuilder();
            
            sb.AppendLine($"<div class=\"navigation-section\" id=\"{section.SectionId}\">");
            sb.AppendLine("<div class=\"section-header\">");
            sb.AppendLine($"<h6 class=\"section-title\">");
            sb.AppendLine($"<i class=\"{section.Icon} me-2\"></i>");
            sb.AppendLine($"{section.Title}");
            sb.AppendLine("</h6>");
            sb.AppendLine("</div>");
            sb.AppendLine("<nav class=\"navigation-menu\">");
            
            foreach (var item in section.Items.OrderBy(i => i.Order))
            {
                sb.AppendLine(htmlHelper.MedicalNavigationItem(item, model).ToString());
            }
            
            sb.AppendLine("</nav>");
            sb.AppendLine("</div>");
            
            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// آیتم ناوبری پزشکی - Strongly Typed
        /// </summary>
        public static MvcHtmlString MedicalNavigationItem(this HtmlHelper htmlHelper, NavigationItem item, MedicalNavigationViewModel model)
        {
            if (item == null || !item.IsVisible)
                return MvcHtmlString.Empty;

            var sb = new StringBuilder();
            
            // تعیین کلاس‌های CSS
            var cssClasses = new List<string> { "nav-item" };
            if (item.IsActive)
                cssClasses.Add("active");
            if (!string.IsNullOrEmpty(item.CssClass))
                cssClasses.Add(item.CssClass);
            
            var cssClass = string.Join(" ", cssClasses);
            
            // ایجاد لینک با Strongly Typed Helper
            var linkText = $"<i class=\"{item.Icon}\"></i><span>{item.Title}</span>";
            if (!string.IsNullOrEmpty(item.BadgeText))
            {
                linkText += $"<span class=\"badge {item.BadgeClass}\">{item.BadgeText}</span>";
            }
            
            var routeValues = item.RouteValues ?? new { };
            var actionLink = htmlHelper.ActionLink(
                linkText,
                item.Action,
                item.Controller,
                new { area = item.Area },
                new { 
                    @class = cssClass,
                    title = item.Tooltip,
                    data_permission = item.Permission
                }
            ).ToString();
            
            sb.AppendLine(actionLink);
            
            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// عملیات سریع پزشکی
        /// </summary>
        public static MvcHtmlString MedicalQuickActions(this HtmlHelper htmlHelper, List<QuickAction> quickActions)
        {
            if (quickActions == null || !quickActions.Any())
                return MvcHtmlString.Empty;

            var sb = new StringBuilder();
            
            sb.AppendLine("<div class=\"navigation-section quick-actions\">");
            sb.AppendLine("<div class=\"section-header\">");
            sb.AppendLine("<h6 class=\"section-title\">");
            sb.AppendLine("<i class=\"fas fa-bolt me-2\"></i>");
            sb.AppendLine("عملیات سریع");
            sb.AppendLine("</h6>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"quick-actions-grid\">");
            
            foreach (var action in quickActions.Where(a => a.IsVisible))
            {
                sb.AppendLine(htmlHelper.MedicalQuickAction(action).ToString());
            }
            
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            
            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// عملیات سریع منفرد
        /// </summary>
        public static MvcHtmlString MedicalQuickAction(this HtmlHelper htmlHelper, QuickAction action)
        {
            if (action == null || !action.IsVisible)
                return MvcHtmlString.Empty;

            var routeValues = action.RouteValues ?? new { };
            var actionLink = htmlHelper.ActionLink(
                $"<i class=\"{action.Icon}\"></i><span>{action.Title}</span>",
                action.Action,
                action.Controller,
                new { area = "" },
                new { 
                    @class = $"quick-action-btn {action.CssClass}",
                    title = action.Tooltip,
                    data_permission = action.Permission
                }
            ).ToString();
            
            return MvcHtmlString.Create(actionLink);
        }

        /// <summary>
        /// ناوبری دپارتمان‌ها - Strongly Typed
        /// </summary>
        public static MvcHtmlString DepartmentNavigation(this HtmlHelper htmlHelper, DepartmentNavigationViewModel model)
        {
            if (model == null || !model.Departments.Any())
                return MvcHtmlString.Empty;

            var sb = new StringBuilder();
            
            sb.AppendLine("<div class=\"department-navigation\">");
            sb.AppendLine("<div class=\"department-header\">");
            sb.AppendLine("<h6>دپارتمان‌ها</h6>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"department-list\">");
            
            foreach (var department in model.Departments)
            {
                sb.AppendLine(htmlHelper.DepartmentNavigationItem(department, model.SelectedDepartmentId).ToString());
            }
            
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            
            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// آیتم ناوبری دپارتمان
        /// </summary>
        public static MvcHtmlString DepartmentNavigationItem(this HtmlHelper htmlHelper, DepartmentNavigationItem department, int selectedDepartmentId)
        {
            if (department == null)
                return MvcHtmlString.Empty;

            var isActive = department.DepartmentId == selectedDepartmentId;
            var cssClass = $"department-item {(isActive ? "active" : "")} {department.CssClass}";
            
            var actionLink = htmlHelper.ActionLink(
                $"<i class=\"{department.Icon}\"></i><span>{department.DepartmentName}</span><small>{department.PatientCount} بیمار</small>",
                "Index",
                "Reception",
                new { departmentId = department.DepartmentId },
                new { 
                    @class = cssClass,
                    title = $"دپارتمان {department.DepartmentName} - {department.PatientCount} بیمار"
                }
            ).ToString();
            
            return MvcHtmlString.Create(actionLink);
        }
    }
}
