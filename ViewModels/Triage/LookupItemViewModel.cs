using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Triage
{
    /// <summary>
    /// ViewModel برای آیتم‌های Lookup در DropDown ها
    /// </summary>
    public class LookupItemViewModel
    {
        [Display(Name = "مقدار")]
        public string Value { get; set; }

        [Display(Name = "متن")]
        public string Text { get; set; }

        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "گروه")]
        public string Group { get; set; }

        [Display(Name = "ترتیب")]
        public int Order { get; set; }
    }
}
