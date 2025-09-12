namespace ClinicApp.Models.Enums;

/// <summary>
/// وضعیت تعادل کار و زندگی
/// </summary>
public enum WorkLifeBalanceStatus
{
    /// <summary>
    /// تعادل مناسب
    /// </summary>
    Balanced = 1,

    /// <summary>
    /// کار بیش از حد
    /// </summary>
    WorkOverload = 2,

    /// <summary>
    /// استراحت بیش از حد
    /// </summary>
    RestOverload = 3,

    /// <summary>
    /// نیاز به بهبود
    /// </summary>
    NeedsImprovement = 4
}