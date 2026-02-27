using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.CMS
{
    /// <summary>
    /// تنظیمات کانال ارسال (ایمیل/SMS) برای محیط پروداکشن — ذخیره در DB بدون وابستگی به Web.config.
    /// </summary>
    [Table("ChannelConfigs")]
    public class ChannelConfig
    {
        public int ChannelConfigId { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; }

        /// <summary>مقدار — برای رمزها می‌توان از رمزنگاری استفاده کرد.</summary>
        [MaxLength]
        public string SettingValue { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(128)]
        public string UpdatedByUserId { get; set; }

        public static class Categories
        {
            public const string Email = "Email";
            public const string Sms = "Sms";
        }
    }

    public class ChannelConfigConfig : EntityTypeConfiguration<ChannelConfig>
    {
        public ChannelConfigConfig()
        {
            HasKey(e => e.ChannelConfigId);
            Property(e => e.SettingValue).IsOptional();
            Property(e => e.UpdatedByUserId).IsOptional();
            // ایندکس یکتا در migration به صورت دستی اضافه می‌شود: IX_ChannelConfig_Category_Key
        }
    }
}
