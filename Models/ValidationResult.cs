using System.Collections.Generic;

namespace ClinicApp.Models
{
    /// <summary>
    /// نتیجه اعتبارسنجی
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
