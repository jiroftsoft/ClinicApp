using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// انواع مختلف قالب‌های برنامه کاری
/// </summary>
public enum TemplateType : byte
{
    [Display(Name = "قالب هفتگی")]
    Weekly = 0,
    [Display(Name = "قالب ماهانه")]
    Monthly = 1,
    [Display(Name = "قالب فصلی")]
    Seasonal = 2,
    [Display(Name = "قالب تعطیلات")]
    Holiday = 3,
    [Display(Name = "قالب اورژانس")]
    Emergency = 4,
    [Display(Name = "قالب سفارشی")]
    Custom = 5
}