using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Renderer برای Template هوشمند
    /// تبدیل AST به HTML نهایی با داده‌های واقعی
    /// استفاده از SmartTemplateService برای Cache و Error Handling
    /// </summary>
    public class SmartTemplateRenderer
    {
        private static readonly SmartTemplateService _service = new SmartTemplateService();

        /// <summary>
        /// Render کردن Template با استفاده از SmartTemplateService
        /// </summary>
        public static string Render(string template, Dictionary<string, object> variables, string templateId = null)
        {
            var result = _service.Render(template, variables, templateId);
            
            if (result.HasErrors)
            {
                // در صورت خطا، Template اصلی را برمی‌گردانیم (Fallback)
                return template;
            }

            return result.Output;
        }

        /// <summary>
        /// Render کردن Template با نتیجه کامل (شامل خطاها)
        /// </summary>
        public static TemplateRenderResult RenderWithResult(string template, Dictionary<string, object> variables, string templateId = null)
        {
            return _service.Render(template, variables, templateId);
        }
    }
}

