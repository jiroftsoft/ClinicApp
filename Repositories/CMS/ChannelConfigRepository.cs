using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Repositories.CMS
{
    public class ChannelConfigRepository : IChannelConfigRepository
    {
        private readonly ApplicationDbContext _context;

        public ChannelConfigRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Dictionary<string, string>> GetByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category)) return new Dictionary<string, string>();

            var list = await _context.ChannelConfigs
                .Where(c => c.Category == category)
                .Select(c => new { c.SettingKey, c.SettingValue })
                .ToListAsync();

            return list.ToDictionary(x => x.SettingKey, x => x.SettingValue ?? string.Empty);
        }

        public async Task<string> GetValueAsync(string category, string settingKey)
        {
            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(settingKey))
                return null;

            var entity = await _context.ChannelConfigs
                .Where(c => c.Category == category && c.SettingKey == settingKey)
                .Select(c => c.SettingValue)
                .FirstOrDefaultAsync();

            return entity;
        }

        public async Task SetValueAsync(string category, string settingKey, string value, string updatedByUserId)
        {
            var entity = await _context.ChannelConfigs
                .FirstOrDefaultAsync(c => c.Category == category && c.SettingKey == settingKey);

            if (entity != null)
            {
                entity.SettingValue = value;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedByUserId = updatedByUserId;
            }
            else
            {
                _context.ChannelConfigs.Add(new ChannelConfig
                {
                    Category = category,
                    SettingKey = settingKey,
                    SettingValue = value,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedByUserId = updatedByUserId
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task SetBulkAsync(string category, IReadOnlyDictionary<string, string> keyValues, string updatedByUserId)
        {
            if (string.IsNullOrWhiteSpace(category) || keyValues == null || keyValues.Count == 0)
                return;

            var keys = keyValues.Keys.ToList();
            var existing = await _context.ChannelConfigs
                .Where(c => c.Category == category && keys.Contains(c.SettingKey))
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var kv in keyValues)
            {
                var entity = existing.FirstOrDefault(c => c.SettingKey == kv.Key);
                if (entity != null)
                {
                    entity.SettingValue = kv.Value;
                    entity.UpdatedAt = now;
                    entity.UpdatedByUserId = updatedByUserId;
                }
                else
                {
                    _context.ChannelConfigs.Add(new ChannelConfig
                    {
                        Category = category,
                        SettingKey = kv.Key,
                        SettingValue = kv.Value,
                        UpdatedAt = now,
                        UpdatedByUserId = updatedByUserId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
