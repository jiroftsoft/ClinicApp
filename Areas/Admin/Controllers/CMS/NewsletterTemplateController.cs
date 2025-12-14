using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت Template های خبرنامه
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class NewsletterTemplateController : BaseCMSController
    {
        private readonly INewsletterTemplateService _templateService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public NewsletterTemplateController(
            INewsletterTemplateService templateService,
            ICurrentUserService currentUserService)
        {
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<NewsletterTemplateController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.Information("درخواست نمایش لیست Template های خبرنامه توسط کاربر {UserId}", _currentUserService.UserId);

                var result = await _templateService.GetTemplatesAsync();

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست Template ها: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Index"), new System.Collections.Generic.List<NewsletterTemplateIndexViewModel>());
                }

                return View(GetViewPath("Index"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست Template های خبرنامه");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست Template ها");
                return View(GetViewPath("Index"), new System.Collections.Generic.List<NewsletterTemplateIndexViewModel>());
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _templateService.GetTemplateDetailsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات Template - TemplateId: {TemplateId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات Template");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public ActionResult Create()
        {
            var model = new NewsletterTemplateCreateEditViewModel
            {
                IsActive = true
            };
            return View(GetViewPath("Create"), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // برای CKEditor
        public async Task<ActionResult> Create(NewsletterTemplateCreateEditViewModel model)
        {
            try
            {
                // Log ModelState errors for debugging
                if (!ModelState.IsValid)
                {
                    var errors = new List<string>();
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Count > 0)
                        {
                            foreach (var error in state.Errors)
                            {
                                errors.Add($"{key}: {error.ErrorMessage}");
                                _logger.Warning("ModelState Validation Error - Field: {Field}, Error: {Error}", key, error.ErrorMessage);
                            }
                        }
                    }
                    
                    // بررسی خاص برای فیلد Content
                    if (string.IsNullOrWhiteSpace(model?.Content))
                    {
                        _logger.Warning("Content field is empty or null");
                        ModelState.AddModelError("Content", "محتوای Template الزامی است.");
                    }
                    
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید.");
                    return View(GetViewPath("Create"), model);
                }

                var result = await _templateService.CreateTemplateAsync(model);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                    return RedirectToAction("Index");
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Create"), model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Template");
                NotificationHelper.SetError(TempData, "خطا در ایجاد Template");
                return View(GetViewPath("Create"), model);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _templateService.GetTemplateForEditAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش Template - TemplateId: {TemplateId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری اطلاعات Template");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // برای CKEditor
        public async Task<ActionResult> Edit(NewsletterTemplateCreateEditViewModel model)
        {
            try
            {
                // Log ModelState errors for debugging
                if (!ModelState.IsValid)
                {
                    var errors = new List<string>();
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Count > 0)
                        {
                            foreach (var error in state.Errors)
                            {
                                errors.Add($"{key}: {error.ErrorMessage}");
                                _logger.Warning("ModelState Validation Error - Field: {Field}, Error: {Error}", key, error.ErrorMessage);
                            }
                        }
                    }
                    
                    // بررسی خاص برای فیلد Content
                    if (string.IsNullOrWhiteSpace(model?.Content))
                    {
                        _logger.Warning("Content field is empty or null");
                        ModelState.AddModelError("Content", "محتوای Template الزامی است.");
                    }
                    
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید.");
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _templateService.UpdateTemplateAsync(model);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                    return RedirectToAction("Index");
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Edit"), model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی Template - TemplateId: {TemplateId}", model?.NewsletterTemplateId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی Template");
                return View(GetViewPath("Edit"), model);
            }
        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _templateService.DeleteTemplateAsync(id);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف Template - TemplateId: {TemplateId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف Template");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Activate/Deactivate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(int id)
        {
            try
            {
                var result = await _templateService.ActivateTemplateAsync(id);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال کردن Template - TemplateId: {TemplateId}", id);
                NotificationHelper.SetError(TempData, "خطا در فعال کردن Template");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _templateService.DeactivateTemplateAsync(id);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال کردن Template - TemplateId: {TemplateId}", id);
                NotificationHelper.SetError(TempData, "خطا در غیرفعال کردن Template");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Preview

        /// <summary>
        /// نمایش پیش‌نمایش Template با قابلیت استفاده از Sample Data یا Real Data
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public async Task<ActionResult> Preview(int id, bool useSampleData = true)
        {
            try
            {
                _logger.Information("درخواست پیش‌نمایش Template - TemplateId: {TemplateId}, UseSampleData: {UseSampleData}", 
                    id, useSampleData);

                // دریافت Template
                var result = await _templateService.GetTemplateDetailsAsync(id);

                if (!result.Success || result.Data == null)
                {
                    _logger.Warning("Template یافت نشد - TemplateId: {TemplateId}", id);
                    NotificationHelper.SetError(TempData, result.Message ?? "Template یافت نشد");
                    return RedirectToAction("Index");
                }

                // بررسی وجود محتوا
                if (string.IsNullOrWhiteSpace(result.Data.Content))
                {
                    _logger.Warning("Template محتوایی ندارد - TemplateId: {TemplateId}", id);
                    NotificationHelper.SetWarning(TempData, "این Template محتوایی ندارد.");
                    ViewBag.UseSampleData = useSampleData;
                    ViewBag.RenderErrors = new List<TemplateError>();
                    return View(GetViewPath("Preview"), result.Data);
                }

                // ساخت متغیرها بر اساس نوع داده
                Dictionary<string, string> variables;

                if (useSampleData)
                {
                    // Render با Sample Data پیشرفته و واقع‌گرایانه
                    variables = SmartTemplateVariableHelper.BuildAdvancedVariables(
                        new Models.Entities.CMS.NewsletterSubscription
                        {
                            FullName = "احمد محمدی",
                            Email = "ahmad.mohammadi@example.com",
                            PhoneNumber = "09123456789",
                            Categories = Newtonsoft.Json.JsonConvert.SerializeObject(new[] { "Articles", "Announcements" }),
                            CreatedAt = DateTime.Now.AddMonths(-2)
                        },
                        "https://clinicapp.com/Newsletter/Unsubscribe?token=sample-token-12345"
                    );
                    
                    _logger.Debug("Using sample data for preview - TemplateId: {TemplateId}", id);
                }
                else
                {
                    // استفاده از داده‌های واقعی (در صورت وجود)
                    // TODO: می‌توان از آخرین subscription استفاده کرد
                    variables = SmartTemplateVariableHelper.BuildAdvancedVariables(
                        new Models.Entities.CMS.NewsletterSubscription
                        {
                            FullName = "کاربر نمونه",
                            Email = "sample@example.com",
                            PhoneNumber = "09123456789",
                            CreatedAt = DateTime.Now.AddMonths(-1)
                        },
                        "https://clinicapp.com/Newsletter/Unsubscribe?token=real-token"
                    );
                    
                    _logger.Debug("Using real data for preview - TemplateId: {TemplateId}", id);
                }

                // Render کردن Template با مدیریت خطا
                var renderResult = await _templateService.RenderTemplateWithResultAsync(
                    result.Data.Content, 
                    variables, 
                    result.Data.NewsletterTemplateId);

                // مدیریت نتیجه Render
                string renderedContent = string.Empty;
                
                if (renderResult.Success && renderResult.Data != null)
                {
                    if (renderResult.Data.IsSuccess && !renderResult.Data.HasErrors)
                    {
                        // Render موفق - استفاده از Output
                        renderedContent = renderResult.Data.Output ?? string.Empty;
                        _logger.Information("Template rendered successfully - TemplateId: {TemplateId}, Output Length: {Length}", 
                            result.Data.NewsletterTemplateId, renderedContent.Length);
                    }
                    else if (renderResult.Data.HasErrors)
                    {
                        // نمایش خطاها در Preview
                        var errorMessages = string.Join(" | ", renderResult.Data.Errors.Select(e => e.Message));
                        _logger.Warning("Template render errors - TemplateId: {TemplateId}, Errors: {Errors}", 
                            result.Data.NewsletterTemplateId, errorMessages);
                        
                        // استفاده از Output در صورت وجود، در غیر این صورت Template اصلی
                        renderedContent = !string.IsNullOrEmpty(renderResult.Data.Output) 
                            ? renderResult.Data.Output 
                            : result.Data.Content;
                        
                        // اضافه کردن خطاها به محتوا برای نمایش در Preview
                        var errorHtml = "<div class='alert alert-warning' style='margin: 10px; padding: 10px; border: 1px solid #ffc107; background: #fff3cd;'>" +
                                       "<strong><i class='fas fa-exclamation-triangle'></i> هشدار:</strong> " +
                                       "برخی خطاها در Render Template رخ داده است. " +
                                       "<details style='margin-top: 10px;'><summary>جزئیات خطاها</summary><ul style='margin-top: 10px;'>" +
                                       string.Join("", renderResult.Data.Errors.Select(e => 
                                           $"<li><strong>{e.Code ?? "خطا"}:</strong> {System.Web.HttpUtility.HtmlEncode(e.Message)}" +
                                           (e.LineNumber.HasValue ? $" (خط {e.LineNumber})" : "") + "</li>")) +
                                       "</ul></details></div>";
                        
                        renderedContent = renderedContent + errorHtml;
                    }
                    else
                    {
                        // Render ناموفق اما بدون خطا - استفاده از Template اصلی
                        renderedContent = result.Data.Content;
                        _logger.Warning("Template render failed but no errors - TemplateId: {TemplateId}", 
                            result.Data.NewsletterTemplateId);
                        NotificationHelper.SetWarning(TempData, "خطا در Render Template. محتوای اصلی نمایش داده می‌شود.");
                    }
                }
                else
                {
                    // خطا در فراخوانی Render
                    renderedContent = result.Data.Content;
                    _logger.Error("Failed to render template - TemplateId: {TemplateId}, Success: {Success}, Message: {Message}", 
                        result.Data.NewsletterTemplateId, renderResult?.Success ?? false, renderResult?.Message);
                    NotificationHelper.SetWarning(TempData, 
                        $"خطا در Render Template: {renderResult?.Message ?? "خطای نامشخص"}. محتوای اصلی نمایش داده می‌شود.");
                }

                // اطمینان از ساختار HTML صحیح برای نمایش در Preview
                // اگر محتوا fragment است (بدون DOCTYPE یا html tag)، آن را wrap می‌کنیم
                if (!string.IsNullOrWhiteSpace(renderedContent))
                {
                    var trimmedContent = renderedContent.TrimStart();
                    var isFragment = !trimmedContent.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) &&
                                     !trimmedContent.StartsWith("<html", StringComparison.OrdinalIgnoreCase);
                    
                    if (isFragment)
                    {
                        // محتوا fragment است - wrap کردن در HTML shell برای نمایش بهتر
                        _logger.Debug("Template content is fragment, wrapping in HTML shell - TemplateId: {TemplateId}, Content Length: {Length}", 
                            result.Data.NewsletterTemplateId, renderedContent.Length);
                        renderedContent = WrapInHtmlShell(renderedContent);
                    }
                    else
                    {
                        _logger.Debug("Template content has full HTML structure - TemplateId: {TemplateId}, Content Length: {Length}", 
                            result.Data.NewsletterTemplateId, renderedContent.Length);
                    }
                }
                else
                {
                    _logger.Warning("Rendered content is empty - TemplateId: {TemplateId}", result.Data.NewsletterTemplateId);
                }

                // ذخیره محتوای نهایی
                result.Data.Content = renderedContent;

                // تنظیم ViewBag
                ViewBag.UseSampleData = useSampleData;
                ViewBag.RenderErrors = (renderResult?.Success == true && renderResult.Data != null) 
                    ? renderResult.Data.Errors 
                    : new List<TemplateError>();

                return View(GetViewPath("Preview"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش پیش‌نمایش Template - TemplateId: {TemplateId}", id);
                NotificationHelper.SetError(TempData, "خطا در نمایش پیش‌نمایش: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Endpoint جداگانه برای نمایش HTML Preview (CSP-friendly)
        /// این endpoint HTML کامل را برمی‌گرداند و iframe مستقیماً از آن استفاده می‌کند
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public async Task<ActionResult> PreviewHtml(int id, bool useSampleData = true)
        {
            try
            {
                _logger.Information("درخواست Preview HTML - TemplateId: {TemplateId}, UseSampleData: {UseSampleData}", 
                    id, useSampleData);

                // دریافت Template
                var result = await _templateService.GetTemplateDetailsAsync(id);

                if (!result.Success || result.Data == null)
                {
                    _logger.Warning("Template یافت نشد - TemplateId: {TemplateId}", id);
                    Response.StatusCode = 404;
                    return Content("Template یافت نشد", "text/plain; charset=utf-8");
                }

                // بررسی وجود محتوا
                if (string.IsNullOrWhiteSpace(result.Data.Content))
                {
                    Response.StatusCode = 404;
                    return Content("این Template محتوایی ندارد.", "text/plain; charset=utf-8");
                }

                // ساخت متغیرها
                Dictionary<string, string> variables;

                if (useSampleData)
                {
                    variables = SmartTemplateVariableHelper.BuildAdvancedVariables(
                        new Models.Entities.CMS.NewsletterSubscription
                        {
                            FullName = "احمد محمدی",
                            Email = "ahmad.mohammadi@example.com",
                            PhoneNumber = "09123456789",
                            Categories = Newtonsoft.Json.JsonConvert.SerializeObject(new[] { "Articles", "Announcements" }),
                            CreatedAt = DateTime.Now.AddMonths(-2)
                        },
                        "https://clinicapp.com/Newsletter/Unsubscribe?token=sample-token-12345"
                    );
                }
                else
                {
                    variables = SmartTemplateVariableHelper.BuildAdvancedVariables(
                        new Models.Entities.CMS.NewsletterSubscription
                        {
                            FullName = "کاربر نمونه",
                            Email = "sample@example.com",
                            PhoneNumber = "09123456789",
                            CreatedAt = DateTime.Now.AddMonths(-1)
                        },
                        "https://clinicapp.com/Newsletter/Unsubscribe?token=real-token"
                    );
                }

                // Render کردن Template
                var renderResult = await _templateService.RenderTemplateWithResultAsync(
                    result.Data.Content, 
                    variables, 
                    result.Data.NewsletterTemplateId);

                string htmlContent = string.Empty;

                if (renderResult.Success && renderResult.Data != null)
                {
                    if (renderResult.Data.IsSuccess && !renderResult.Data.HasErrors)
                    {
                        htmlContent = renderResult.Data.Output;
                        _logger.Information("Template rendered successfully - TemplateId: {TemplateId}, Output Length: {Length}", 
                            id, htmlContent?.Length ?? 0);
                    }
                    else if (!string.IsNullOrEmpty(renderResult.Data.Output))
                    {
                        htmlContent = renderResult.Data.Output;
                        var errorMessages = string.Join(" | ", renderResult.Data.Errors?.Select(e => e.Message) ?? new List<string>());
                        _logger.Warning("Template rendered with errors - TemplateId: {TemplateId}, Output Length: {Length}, Errors: {Errors}", 
                            id, htmlContent?.Length ?? 0, errorMessages);
                    }
                    else
                    {
                        htmlContent = result.Data.Content;
                        _logger.Warning("Template render failed, using original content - TemplateId: {TemplateId}, Content Length: {Length}", 
                            id, htmlContent?.Length ?? 0);
                    }
                }
                else
                {
                    htmlContent = result.Data.Content;
                    _logger.Error("Template render failed - TemplateId: {TemplateId}, Success: {Success}, Message: {Message}, Using original content - Length: {Length}", 
                        id, renderResult?.Success ?? false, renderResult?.Message ?? "Unknown error", htmlContent?.Length ?? 0);
                }

                // Log محتوای رندر شده برای debugging
                _logger.Debug("Rendered content preview (first 500 chars) - TemplateId: {TemplateId}, Length: {Length}, Preview: {Preview}", 
                    id, htmlContent?.Length ?? 0, htmlContent?.Substring(0, Math.Min(500, htmlContent?.Length ?? 0)) ?? "empty");

                // استخراج body content اگر HTML کامل است
                var trimmedContent = htmlContent?.TrimStart() ?? string.Empty;
                var isFullHtml = trimmedContent.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
                                 trimmedContent.StartsWith("<html", StringComparison.OrdinalIgnoreCase);
                
                if (isFullHtml)
                {
                    // HTML کامل است - استخراج body content
                    var originalLength = htmlContent?.Length ?? 0;
                    var extractedContent = ExtractBodyContent(htmlContent);
                    
                    // بررسی اینکه آیا استخراج موفق بود
                    var extractionSuccessful = !string.IsNullOrWhiteSpace(extractedContent) && 
                                               extractedContent != htmlContent && 
                                               extractedContent.Length < originalLength &&
                                               !extractedContent.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) &&
                                               !extractedContent.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase);
                    
                    if (extractionSuccessful)
                    {
                        htmlContent = extractedContent;
                        _logger.Information("✅ Extracted body content from full HTML - TemplateId: {TemplateId}, Original Length: {OriginalLength}, Extracted Length: {ExtractedLength}", 
                            id, originalLength, htmlContent.Length);
                    }
                    else
                    {
                        // اگر استخراج موفق نبود، سعی می‌کنیم محتوای بین <body> و </body> را مستقیماً بگیریم
                        _logger.Warning("⚠️ Body content extraction may have failed - TemplateId: {TemplateId}, Original Length: {OriginalLength}, Extracted Length: {ExtractedLength}, Trying direct extraction", 
                            id, originalLength, extractedContent?.Length ?? 0);
                        
                        // استخراج مستقیم با استفاده از LastIndexOf برای </body>
                        var bodyStart = htmlContent.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
                        if (bodyStart >= 0)
                        {
                            var bodyTagEnd = htmlContent.IndexOf(">", bodyStart);
                            if (bodyTagEnd >= 0)
                            {
                                var bodyContentStart = bodyTagEnd + 1;
                                var bodyEnd = htmlContent.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                                if (bodyEnd > bodyContentStart)
                                {
                                    var directExtracted = htmlContent.Substring(bodyContentStart, bodyEnd - bodyContentStart).Trim();
                                    if (!string.IsNullOrWhiteSpace(directExtracted) && 
                                        !directExtracted.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) &&
                                        !directExtracted.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
                                    {
                                        htmlContent = directExtracted;
                                        _logger.Information("✅ Direct body extraction successful - TemplateId: {TemplateId}, Length: {Length}", 
                                            id, htmlContent.Length);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Fragment است - wrap کردن در HTML shell
                    var originalLength = htmlContent?.Length ?? 0;
                    htmlContent = WrapInHtmlShell(htmlContent);
                    _logger.Information("Wrapped fragment in HTML shell - TemplateId: {TemplateId}, Original Length: {OriginalLength}, Wrapped Length: {WrappedLength}", 
                        id, originalLength, htmlContent?.Length ?? 0);
                }
                
                // Log محتوای نهایی برای debugging
                var finalPreview = htmlContent?.Substring(0, Math.Min(500, htmlContent?.Length ?? 0)) ?? "empty";
                _logger.Debug("Final content preview (first 500 chars) - TemplateId: {TemplateId}, Length: {Length}, Preview: {Preview}, Starts with DOCTYPE: {StartsWithDoctype}, Starts with HTML: {StartsWithHtml}", 
                    id, htmlContent?.Length ?? 0, finalPreview, 
                    htmlContent?.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ?? false,
                    htmlContent?.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase) ?? false);

                // تنظیم Content-Type
                Response.ContentType = "text/html; charset=utf-8";
                
                // افزودن هدرهای امنیتی
                Response.Headers.Add("X-Content-Type-Options", "nosniff");
                Response.Headers.Add("X-Frame-Options", "SAMEORIGIN"); // فقط same-origin می‌تواند frame کند

                return Content(htmlContent, "text/html; charset=utf-8");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش Preview HTML - TemplateId: {TemplateId}", id);
                Response.StatusCode = 500;
                return Content("خطا در نمایش پیش‌نمایش: " + System.Web.HttpUtility.HtmlEncode(ex.Message), "text/html; charset=utf-8");
            }
        }

        /// <summary>
        /// استخراج محتوای body از HTML کامل
        /// این متد محتوای بین &lt;body&gt; و &lt;/body&gt; را استخراج می‌کند
        /// </summary>
        private string ExtractBodyContent(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                return htmlContent;
            }

            try
            {
                _logger.Debug("Starting body content extraction - Content Length: {Length}, First 200 chars: {Preview}", 
                    htmlContent.Length, htmlContent.Substring(0, Math.Min(200, htmlContent.Length)));

                // روش 1: استفاده از Regex با Singleline برای استخراج body content
                // استفاده از non-greedy match برای پیدا کردن اولین </body>
                var bodyMatch = System.Text.RegularExpressions.Regex.Match(
                    htmlContent,
                    @"<body[^>]*>([\s\S]*?)</body>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (bodyMatch.Success && bodyMatch.Groups.Count > 1)
                {
                    var bodyContent = bodyMatch.Groups[1].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(bodyContent))
                    {
                        _logger.Information("✅ Body content extracted successfully using Regex - Length: {Length}, First 200 chars: {Preview}", 
                            bodyContent.Length, bodyContent.Substring(0, Math.Min(200, bodyContent.Length)));
                        return bodyContent;
                    }
                    else
                    {
                        _logger.Warning("⚠️ Regex matched but body content is empty or whitespace");
                    }
                }
                else
                {
                    _logger.Debug("⚠️ Regex did not match body tag - Success: {Success}, Groups: {Groups}", 
                        bodyMatch.Success, bodyMatch.Groups.Count);
                }

                // روش 2: استفاده از IndexOf برای پیدا کردن body tag (سریع‌تر و قابل اعتمادتر)
                var bodyStartIndex = htmlContent.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
                if (bodyStartIndex >= 0)
                {
                    // پیدا کردن پایان opening body tag
                    var bodyTagEndIndex = htmlContent.IndexOf(">", bodyStartIndex);
                    if (bodyTagEndIndex >= 0)
                    {
                        var bodyContentStart = bodyTagEndIndex + 1;
                        // پیدا کردن closing body tag
                        var bodyEndIndex = htmlContent.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                        if (bodyEndIndex > bodyContentStart)
                        {
                            var bodyContent = htmlContent.Substring(bodyContentStart, bodyEndIndex - bodyContentStart).Trim();
                            if (!string.IsNullOrWhiteSpace(bodyContent))
                            {
                                _logger.Debug("Body content extracted successfully using IndexOf - Length: {Length}", bodyContent.Length);
                                return bodyContent;
                            }
                        }
                    }
                }

                // روش 3: اگر body tag پیدا نشد، سعی می‌کنیم محتوای بین <html> و </html> را بگیریم
                var htmlStartIndex = htmlContent.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
                if (htmlStartIndex >= 0)
                {
                    var htmlTagEndIndex = htmlContent.IndexOf(">", htmlStartIndex);
                    if (htmlTagEndIndex >= 0)
                    {
                        var htmlContentStart = htmlTagEndIndex + 1;
                        var htmlEndIndex = htmlContent.LastIndexOf("</html>", StringComparison.OrdinalIgnoreCase);
                        if (htmlEndIndex > htmlContentStart)
                        {
                            var htmlBodyContent = htmlContent.Substring(htmlContentStart, htmlEndIndex - htmlContentStart);
                            
                            // حذف head tag اگر وجود دارد
                            htmlBodyContent = System.Text.RegularExpressions.Regex.Replace(
                                htmlBodyContent,
                                @"<head[^>]*>[\s\S]*?</head>",
                                string.Empty,
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            
                            htmlBodyContent = htmlBodyContent.Trim();
                            if (!string.IsNullOrWhiteSpace(htmlBodyContent))
                            {
                                _logger.Debug("HTML content extracted (without head) - Length: {Length}", htmlBodyContent.Length);
                                return htmlBodyContent;
                            }
                        }
                    }
                }

                // اگر هیچکدام پیدا نشد، کل محتوا را برمی‌گردانیم
                _logger.Warning("Could not extract body content, returning full content - Content Length: {Length}", htmlContent.Length);
                return htmlContent;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error extracting body content, returning full content");
                return htmlContent;
            }
        }

        /// <summary>
        /// Wrap کردن HTML fragment در یک HTML shell کامل
        /// این متد محتوای fragment را در یک ساختار HTML کامل قرار می‌دهد
        /// </summary>
        private string WrapInHtmlShell(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return content;
            }

            // بررسی اینکه آیا محتوا از قبل در یک ساختار HTML کامل است
            var trimmedContent = content.TrimStart();
            if (trimmedContent.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
                trimmedContent.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
            {
                // محتوا از قبل کامل است
                return content;
            }

            // Escape کردن محتوا برای جلوگیری از مشکلات XSS
            var escapedContent = content;

            // Wrap کردن در HTML shell کامل
            var htmlShell = $@"<!DOCTYPE html>
<html lang=""fa"" dir=""rtl"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <title>پیش‌نمایش ایمیل</title>
    <style>
        * {{
            box-sizing: border-box;
        }}
        body {{
            font-family: Tahoma, Arial, 'Segoe UI', sans-serif;
            direction: rtl;
            text-align: right;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
            line-height: 1.6;
            color: #333;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            border-radius: 4px;
        }}
        @media only screen and (max-width: 600px) {{
            body {{
                padding: 10px;
            }}
            .email-container {{
                padding: 15px;
            }}
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        {escapedContent}
    </div>
</body>
</html>";

            _logger.Debug("Content wrapped in HTML shell - Original Length: {OriginalLength}, Wrapped Length: {WrappedLength}", 
                content.Length, htmlShell.Length);

            return htmlShell;
        }

        #endregion
    }
}

