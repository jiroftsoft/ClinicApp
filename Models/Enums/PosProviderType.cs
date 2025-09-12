using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// نوع ارائه‌دهنده پوز
/// </summary>
public enum PosProviderType
{
    [Display(Name = "سامان کیش")]
    SamanKish = 1,
    [Display(Name = "آسان پرداخت")]
    AsanPardakht = 2,
    [Display(Name = "به‌پرداخت")]
    BehPardakht = 3,
    [Display(Name = "فناوا")]
    Fanava = 4,
    [Display(Name = "ایران کیش")]
    IranKish = 5,
    [Display(Name = "پرداخت آریا")]
    PardakhtAria = 6,
    [Display(Name = "ندا پی")]
    NadaPay = 7
}