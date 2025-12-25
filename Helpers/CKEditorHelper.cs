using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper Class برای استفاده آسان از CKEditor در Views
    /// </summary>
    public static class CkEditorHelper
    {
        /// <summary>
        /// ایجاد TextArea با CKEditor برای استفاده در فرم‌ها
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <param name="name">نام فیلد</param>
        /// <param name="value">مقدار اولیه</param>
        /// <param name="htmlAttributes">ویژگی‌های HTML</param>
        /// <param name="height">ارتفاع Editor (پیش‌فرض: 300)</param>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString CkEditorFor<TModel>(this HtmlHelper<TModel> htmlHelper, string name, string value = "", object htmlAttributes = null, int height = 300)
        {
            var tagBuilder = new TagBuilder("textarea");
            tagBuilder.Attributes.Add("name", name);
            tagBuilder.Attributes.Add("id", name);
            
            if (!string.IsNullOrEmpty(value))
            {
                tagBuilder.InnerHtml = HttpUtility.HtmlEncode(value);
            }
            
            if (htmlAttributes != null)
            {
                var attributes = new RouteValueDictionary(htmlAttributes);
                foreach (var attr in attributes)
                {
                    tagBuilder.Attributes.Add(attr.Key, attr.Value.ToString());
                }
            }
            
            // اضافه کردن class اگر وجود نداشته باشد
            if (!tagBuilder.Attributes.ContainsKey("class"))
            {
                tagBuilder.Attributes.Add("class", "form-control");
            }
            
            var html = tagBuilder.ToString(TagRenderMode.Normal);
            
            // اضافه کردن script برای initialize کردن CKEditor
            var script = $@"
                <script>
                    (function() {{
                        function initEditor() {{
                            if (typeof CKEDITOR !== 'undefined') {{
                                CKEDITOR.replace('{name}', {{
                                    language: 'fa',
                                    contentsLangDirection: 'rtl',
                                    height: {height}
                                }});
                            }} else {{
                                setTimeout(initEditor, 100);
                            }}
                        }}
                        if (document.readyState === 'loading') {{
                            document.addEventListener('DOMContentLoaded', initEditor);
                        }} else {{
                            initEditor();
                        }}
                    }})();
                </script>
            ";
            
            return new MvcHtmlString(html + script);
        }
        
        /// <summary>
        /// ایجاد Script Tag برای بارگذاری CKEditor
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString CkEditorScript<TModel>(this HtmlHelper<TModel> htmlHelper)
        {
            var useCDN = System.Configuration.ConfigurationManager.AppSettings["CKEditor:UseCDN"] ?? "false";
            var isCDN = useCDN.ToLower() == "true";
            
            var scriptTag = new TagBuilder("script");
            
            if (isCDN)
            {
                scriptTag.Attributes.Add("src", "https://cdn.ckeditor.com/4.22.1/standard/ckeditor.js");
            }
            else
            {
                var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
                scriptTag.Attributes.Add("src", urlHelper.Content("~/Content/plugins/ckeditor/ckeditor.js"));
            }
            
            return new MvcHtmlString(scriptTag.ToString(TagRenderMode.SelfClosing));
        }
        
        /// <summary>
        /// ایجاد Script Tag برای Config CKEditor
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper</param>
        /// <returns>MvcHtmlString</returns>
        public static MvcHtmlString CkEditorConfig<TModel>(this HtmlHelper<TModel> htmlHelper)
        {
            var useCDN = System.Configuration.ConfigurationManager.AppSettings["CKEditor:UseCDN"] ?? "false";
            var isCDN = useCDN.ToLower() != "true";
            
            if (!isCDN)
            {
                return MvcHtmlString.Empty;
            }
            
            var scriptTag = new TagBuilder("script");
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            scriptTag.Attributes.Add("src", urlHelper.Content("~/Content/plugins/ckeditor/config.js"));
            
            return new MvcHtmlString(scriptTag.ToString(TagRenderMode.SelfClosing));
        }
    }
}

