using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    public class AssignmentHistoryViewModel
    {   
        public int Id { get; set; }

        [Display(Name = "نوع عملیات")]
        public string ActionType { get; set; } // Create, Update, Delete, Transfer

        [Display(Name = "عنوان عملیات")]
        public string ActionTitle { get; set; }

        [Display(Name = "توضیحات عملیات")]
        public string ActionDescription { get; set; }

        [Display(Name = "تاریخ عملیات")]
        public DateTime ActionDate { get; set; }

        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        [Display(Name = "صلاحیت‌های خدماتی")]
        public string ServiceCategories { get; set; }

        [Display(Name = "انجام شده توسط")]
        public string PerformedBy { get; set; }

        [Display(Name = "یادداشت")]
        public string Notes { get; set; }

        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "کد ملی پزشک")]
        public string DoctorNationalCode { get; set; }

        [Display(Name = "شناسه دپارتمان")]
        public int? DepartmentId { get; set; }

        [Display(Name = "شناسه کاربر انجام دهنده")]
        public string PerformedByUserId { get; set; }

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ بروزرسانی")]
        public DateTime? UpdatedAt { get; set; }

        // Additional properties for views
        [Display(Name = "سطح اهمیت")]
        public string Importance { get; set; }

        [Display(Name = "تخصص پزشک")]
        public string DoctorSpecialization { get; set; }

        [Display(Name = "نام کاربر انجام دهنده")]
        public string PerformedByUserName { get; set; }

        // Additional properties for Details view
        [Display(Name = "نام کلینیک")]
        public string ClinicName { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "تاریخ بروزرسانی")]
        public DateTime? UpdatedDate { get; set; }

        [Display(Name = "حذف شده")]
        public bool IsDeleted { get; set; }

        // Helper properties
        public string ActionDateFormatted => ActionDate.ToString("yyyy/MM/dd HH:mm");
        public string ActionTypeText => GetActionTypeText(ActionType);
        public string ActionTypeBadgeClass => GetActionTypeBadgeClass(ActionType);
        public string TimelineMarkerClass => GetTimelineMarkerClass(ActionType);
        public string TimelineIcon => GetTimelineIcon(ActionType);

        // Static helper methods
        public static string GetActionTypeText(string actionType)
        {
            return actionType switch
            {
                "Create" => "ایجاد",
                "Update" => "ویرایش",
                "Delete" => "حذف",
                "Transfer" => "انتقال",
                "Assign" => "انتساب",
                "Remove" => "حذف انتساب",
                _ => "عملیات"
            };
        }

        public static string GetActionTypeBadgeClass(string actionType)
        {
            return actionType switch
            {
                "Create" => "bg-success",
                "Update" => "bg-warning text-dark",
                "Delete" => "bg-danger",
                "Transfer" => "bg-info",
                "Assign" => "bg-primary",
                "Remove" => "bg-danger",
                _ => "bg-secondary"
            };
        }

        public static string GetTimelineMarkerClass(string actionType)
        {
            return actionType switch
            {
                "Create" => "bg-success",
                "Update" => "bg-warning",
                "Delete" => "bg-danger",
                "Transfer" => "bg-info",
                "Assign" => "bg-primary",
                "Remove" => "bg-danger",
                _ => "bg-secondary"
            };
        }

        public static string GetTimelineIcon(string actionType)
        {
            return actionType switch
            {
                "Create" => "fas fa-plus",
                "Update" => "fas fa-edit",
                "Delete" => "fas fa-trash",
                "Transfer" => "fas fa-exchange-alt",
                "Assign" => "fas fa-user-plus",
                "Remove" => "fas fa-user-minus",
                _ => "fas fa-circle"
            };
        }
    }
}
