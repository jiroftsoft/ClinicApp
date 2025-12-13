using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;
using System.Text.RegularExpressions;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت Template های خبرنامه
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class NewsletterTemplateService : INewsletterTemplateService
    {
        private readonly INewsletterTemplateRepository _templateRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public NewsletterTemplateService(
            INewsletterTemplateRepository templateRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<List<NewsletterTemplateIndexViewModel>>> GetTemplatesAsync()
        {
            try
            {
                var templates = await _templateRepository.GetAllAsync();

                var viewModels = templates.Select(t => new NewsletterTemplateIndexViewModel
                {
                    NewsletterTemplateId = t.NewsletterTemplateId,
                    Name = t.Name,
                    Subject = t.Subject,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToList();

                return ServiceResult<List<NewsletterTemplateIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست Template های خبرنامه");
                return ServiceResult<List<NewsletterTemplateIndexViewModel>>.Failed("خطا در دریافت لیست Template ها");
            }
        }

        public async Task<ServiceResult<NewsletterTemplateDetailsViewModel>> GetTemplateDetailsAsync(int templateId)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(templateId);
                if (template == null)
                {
                    return ServiceResult<NewsletterTemplateDetailsViewModel>.Failed("Template یافت نشد");
                }

                var viewModel = new NewsletterTemplateDetailsViewModel
                {
                    NewsletterTemplateId = template.NewsletterTemplateId,
                    Name = template.Name,
                    Subject = template.Subject,
                    Content = template.Content,
                    IsActive = template.IsActive,
                    CreatedAt = template.CreatedAt,
                    CreatedByUserName = template.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = template.UpdatedAt,
                    UpdatedByUserName = template.UpdatedByUser?.UserName
                };

                return ServiceResult<NewsletterTemplateDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات Template - TemplateId: {TemplateId}", templateId);
                return ServiceResult<NewsletterTemplateDetailsViewModel>.Failed("خطا در دریافت جزئیات Template");
            }
        }

        public async Task<ServiceResult<NewsletterTemplateCreateEditViewModel>> GetTemplateForEditAsync(int templateId)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(templateId);
                if (template == null)
                {
                    return ServiceResult<NewsletterTemplateCreateEditViewModel>.Failed("Template یافت نشد");
                }

                var viewModel = new NewsletterTemplateCreateEditViewModel
                {
                    NewsletterTemplateId = template.NewsletterTemplateId,
                    Name = template.Name,
                    Subject = template.Subject,
                    Content = template.Content,
                    IsActive = template.IsActive
                };

                return ServiceResult<NewsletterTemplateCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت Template برای ویرایش - TemplateId: {TemplateId}", templateId);
                return ServiceResult<NewsletterTemplateCreateEditViewModel>.Failed("خطا در دریافت Template");
            }
        }

        public async Task<ServiceResult<NewsletterTemplate>> CreateTemplateAsync(NewsletterTemplateCreateEditViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return ServiceResult<NewsletterTemplate>.Failed("اطلاعات Template نامعتبر است");
                }

                var template = new NewsletterTemplate
                {
                    Name = model.Name.Trim(),
                    Subject = model.Subject.Trim(),
                    Content = model.Content,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = _currentUserService.UserId
                };

                _templateRepository.Add(template);
                await _context.SaveChangesAsync();

                _logger.Information("Template جدید ایجاد شد - Name: {Name}, TemplateId: {TemplateId}", 
                    template.Name, template.NewsletterTemplateId);

                return ServiceResult<NewsletterTemplate>.Successful(template, "Template با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Template - Name: {Name}", model?.Name);
                return ServiceResult<NewsletterTemplate>.Failed("خطا در ایجاد Template");
            }
        }

        public async Task<ServiceResult<NewsletterTemplate>> UpdateTemplateAsync(NewsletterTemplateCreateEditViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return ServiceResult<NewsletterTemplate>.Failed("اطلاعات Template نامعتبر است");
                }

                var template = await _templateRepository.GetByIdAsync(model.NewsletterTemplateId);
                if (template == null)
                {
                    return ServiceResult<NewsletterTemplate>.Failed("Template یافت نشد");
                }

                template.Name = model.Name.Trim();
                template.Subject = model.Subject.Trim();
                template.Content = model.Content;
                template.IsActive = model.IsActive;
                template.UpdatedAt = DateTime.Now;
                template.UpdatedByUserId = _currentUserService.UserId;

                _templateRepository.Update(template);
                await _context.SaveChangesAsync();

                _logger.Information("Template به‌روزرسانی شد - TemplateId: {TemplateId}", template.NewsletterTemplateId);

                return ServiceResult<NewsletterTemplate>.Successful(template, "Template با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی Template - TemplateId: {TemplateId}", model?.NewsletterTemplateId);
                return ServiceResult<NewsletterTemplate>.Failed("خطا در به‌روزرسانی Template");
            }
        }

        public async Task<ServiceResult> DeleteTemplateAsync(int templateId)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(templateId);
                if (template == null)
                {
                    return ServiceResult.Failed("Template یافت نشد");
                }

                _templateRepository.Delete(template);
                template.DeletedByUserId = _currentUserService.UserId;
                await _context.SaveChangesAsync();

                _logger.Information("Template حذف شد - TemplateId: {TemplateId}", templateId);

                return ServiceResult.Successful("Template با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف Template - TemplateId: {TemplateId}", templateId);
                return ServiceResult.Failed("خطا در حذف Template");
            }
        }

        public async Task<ServiceResult> ActivateTemplateAsync(int templateId)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(templateId);
                if (template == null)
                {
                    return ServiceResult.Failed("Template یافت نشد");
                }

                template.IsActive = true;
                template.UpdatedAt = DateTime.Now;
                template.UpdatedByUserId = _currentUserService.UserId;

                _templateRepository.Update(template);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("Template فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال کردن Template - TemplateId: {TemplateId}", templateId);
                return ServiceResult.Failed("خطا در فعال کردن Template");
            }
        }

        public async Task<ServiceResult> DeactivateTemplateAsync(int templateId)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(templateId);
                if (template == null)
                {
                    return ServiceResult.Failed("Template یافت نشد");
                }

                template.IsActive = false;
                template.UpdatedAt = DateTime.Now;
                template.UpdatedByUserId = _currentUserService.UserId;

                _templateRepository.Update(template);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("Template غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال کردن Template - TemplateId: {TemplateId}", templateId);
                return ServiceResult.Failed("خطا در غیرفعال کردن Template");
            }
        }

        public async Task<ServiceResult<string>> RenderTemplateAsync(int templateId, Dictionary<string, string> variables)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(templateId);
                if (template == null)
                {
                    return ServiceResult<string>.Failed("Template یافت نشد");
                }

                return await RenderTemplateAsync(template.Content, variables);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Render Template - TemplateId: {TemplateId}", templateId);
                return ServiceResult<string>.Failed("خطا در Render Template");
            }
        }

        public async Task<ServiceResult<string>> RenderTemplateAsync(string content, Dictionary<string, string> variables)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return ServiceResult<string>.Successful(string.Empty);
                }

                if (variables == null || !variables.Any())
                {
                    return ServiceResult<string>.Successful(content);
                }

                var rendered = content;

                // جایگزینی Variables با الگوی {{VariableName}}
                foreach (var variable in variables)
                {
                    var pattern = $"\\{{\\{{{variable.Key}\\}}\\}}";
                    rendered = Regex.Replace(rendered, pattern, variable.Value ?? string.Empty, RegexOptions.IgnoreCase);
                }

                return ServiceResult<string>.Successful(rendered);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Render Template Content");
                return ServiceResult<string>.Failed("خطا در Render Template");
            }
        }
    }
}

