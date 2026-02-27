using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// ذخیره و خواندن تنظیمات کانال (ایمیل/SMS) از DB — برای محیط پروداکشن.
    /// </summary>
    public interface IChannelConfigRepository
    {
        Task<Dictionary<string, string>> GetByCategoryAsync(string category);
        Task<string> GetValueAsync(string category, string settingKey);
        Task SetValueAsync(string category, string settingKey, string value, string updatedByUserId);
        Task SetBulkAsync(string category, IReadOnlyDictionary<string, string> keyValues, string updatedByUserId);
    }
}
