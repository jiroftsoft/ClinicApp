using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Triage
{
    /// <summary>
    /// ViewModel برای مدیریت پروتکل‌های تریاژ
    /// </summary>
    public class TriageProtocolViewModel
    {
        public int TriageProtocolId { get; set; }

        [Required(ErrorMessage = "نام پروتکل الزامی است")]
        [StringLength(200, ErrorMessage = "نام پروتکل نمی‌تواند بیش از 200 کاراکتر باشد")]
        [Display(Name = "نام پروتکل")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Required(ErrorMessage = "نوع پروتکل الزامی است")]
        [Display(Name = "نوع پروتکل")]
        public TriageProtocolType ProtocolType { get; set; }

        [Required(ErrorMessage = "معیارها الزامی است")]
        [StringLength(2000, ErrorMessage = "معیارها نمی‌تواند بیش از 2000 کاراکتر باشد")]
        [Display(Name = "معیارها")]
        public string Criteria { get; set; }

        [Required(ErrorMessage = "اقدامات الزامی است")]
        [StringLength(2000, ErrorMessage = "اقدامات نمی‌تواند بیش از 2000 کاراکتر باشد")]
        [Display(Name = "اقدامات")]
        public string Actions { get; set; }

        [Required(ErrorMessage = "اولویت الزامی است")]
        [Range(1, 10, ErrorMessage = "اولویت باید بین 1 تا 10 باشد")]
        [Display(Name = "اولویت")]
        public int Priority { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ به‌روزرسانی")]
        public DateTime? UpdatedAt { get; set; }
    }
}