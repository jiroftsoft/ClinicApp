using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Serilog;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// سرویس اصلی برای Render کردن Template هوشمند
    /// شامل Cache، Security و Error Handling
    /// </summary>
    public class SmartTemplateService
    {
        private static readonly MemoryCache _astCache = MemoryCache.Default;
        private const int CACHE_EXPIRATION_MINUTES = 60; // 1 ساعت
        private readonly ILogger _logger;

        public SmartTemplateService(ILogger logger = null)
        {
            _logger = logger ?? Serilog.Log.ForContext<SmartTemplateService>();
        }

        /// <summary>
        /// Render کردن Template با Cache و Error Handling
        /// </summary>
        public TemplateRenderResult Render(string template, Dictionary<string, object> variables, string templateId = null)
        {
            var errors = new List<TemplateError>();

            try
            {
                if (string.IsNullOrWhiteSpace(template))
                {
                    return TemplateRenderResult.Successful(string.Empty);
                }

                // تبدیل Dictionary<string, string> به Dictionary<string, object> اگر نیاز باشد
                var objectVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                if (variables != null)
                {
                    foreach (var kvp in variables)
                    {
                        objectVariables[kvp.Key] = kvp.Value;
                    }
                }

                // 📈 Performance: Cache AST
                List<TemplateNode> nodes = null;
                string cacheKey = null;

                if (!string.IsNullOrEmpty(templateId))
                {
                    cacheKey = $"TemplateAST_{templateId}_{GetTemplateHash(template)}";
                    nodes = _astCache.Get(cacheKey) as List<TemplateNode>;
                }

                if (nodes == null)
                {
                    // Parse کردن Template
                    var parser = new SmartTemplateParser(template);
                    nodes = parser.Parse();

                    // Cache کردن AST
                    if (!string.IsNullOrEmpty(cacheKey))
                    {
                        var cachePolicy = new CacheItemPolicy
                        {
                            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(CACHE_EXPIRATION_MINUTES)
                        };
                        _astCache.Set(cacheKey, nodes, cachePolicy);
                        _logger?.Debug("Template AST cached - Key: {CacheKey}", cacheKey);
                    }
                }
                else
                {
                    _logger?.Debug("Template AST retrieved from cache - Key: {CacheKey}", cacheKey);
                }

                // Render کردن Node ها
                var output = RenderNodes(nodes, objectVariables, errors);

                if (errors.Any())
                {
                    return TemplateRenderResult.Failed(errors);
                }

                return TemplateRenderResult.Successful(output);
            }
            catch (TemplateSecurityException ex)
            {
                _logger?.Warning(ex, "Template Security Exception");
                errors.Add(new TemplateError
                {
                    Message = ex.Message,
                    Code = "SECURITY_LOOP_LIMIT",
                    Type = TemplateErrorType.Security
                });
                return TemplateRenderResult.Failed(errors);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در Render Template");
                errors.Add(new TemplateError
                {
                    Message = $"خطا در Render Template: {ex.Message}",
                    Code = "RENDER_ERROR",
                    Type = TemplateErrorType.General
                });
                return TemplateRenderResult.Failed(errors);
            }
        }

        /// <summary>
        /// Render کردن لیست Node ها با Error Handling
        /// </summary>
        private string RenderNodes(List<TemplateNode> nodes, Dictionary<string, object> variables, List<TemplateError> errors)
        {
            if (nodes == null || !nodes.Any())
            {
                return string.Empty;
            }

            var result = new StringBuilder();

            foreach (var node in nodes)
            {
                try
                {
                    var rendered = node.Render(variables);
                    result.Append(rendered);
                }
                catch (TemplateSecurityException ex)
                {
                    errors.Add(new TemplateError
                    {
                        Message = ex.Message,
                        Code = "SECURITY_ERROR",
                        Type = TemplateErrorType.Security
                    });
                    // ادامه می‌دهیم با سایر Node ها
                }
                catch (Exception ex)
                {
                    errors.Add(new TemplateError
                    {
                        Message = $"خطا در Render Node: {ex.Message}",
                        Code = "NODE_RENDER_ERROR",
                        Type = TemplateErrorType.General
                    });
                    // ادامه می‌دهیم با سایر Node ها
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// ایجاد Hash از Template برای Cache Key
        /// </summary>
        private string GetTemplateHash(string template)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(template);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash).Substring(0, 16); // 16 کاراکتر اول
            }
        }

        /// <summary>
        /// پاک کردن Cache برای Template خاص
        /// </summary>
        public static void ClearCache(string templateId)
        {
            if (string.IsNullOrEmpty(templateId))
            {
                return;
            }

            // پیدا کردن تمام Cache Key های مربوط به این Template
            var keysToRemove = new List<string>();
            foreach (var item in _astCache)
            {
                if (item.Key.StartsWith($"TemplateAST_{templateId}_", StringComparison.OrdinalIgnoreCase))
                {
                    keysToRemove.Add(item.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _astCache.Remove(key);
            }
        }

        /// <summary>
        /// پاک کردن تمام Cache
        /// </summary>
        public static void ClearAllCache()
        {
            var keysToRemove = new List<string>();
            foreach (var item in _astCache)
            {
                if (item.Key.StartsWith("TemplateAST_", StringComparison.OrdinalIgnoreCase))
                {
                    keysToRemove.Add(item.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _astCache.Remove(key);
            }
        }
    }
}

