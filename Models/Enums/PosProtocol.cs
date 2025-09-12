using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// پروتکل ارتباطی پوز
/// </summary>
public enum PosProtocol
{
    [Display(Name = "TCP/IP")]
    Tcp = 1,
    [Display(Name = "سریال")]
    Serial = 2,
    [Display(Name = "API وب سرویس")]
    Api = 3
}