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

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت اسلایدرها
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class SliderService : ISliderService
    {
        private readonly ISliderRepository _sliderRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public SliderService(
            ISliderRepository sliderRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _sliderRepository = sliderRepository ?? throw new ArgumentNullException(nameof(sliderRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<List<SliderIndexViewModel>>> GetSlidersAsync(string position = null)
        {
            try
            {
                var sliders = await _sliderRepository.GetAllAsync(includeDeleted: false);
                
                var query = sliders.AsQueryable();
                if (!string.IsNullOrEmpty(position))
                {
                    query = query.Where(s => s.Position == position);
                }

                var viewModels = query.Select(s => new SliderIndexViewModel
                {
                    SliderId = s.SliderId,
                    Title = s.Title,
                    Description = s.Description,
                    ImageUrl = s.ImageUrl,
                    LinkUrl = s.LinkUrl,
                    ButtonText = s.ButtonText,
                    IsActive = s.IsActive,
                    DisplayOrder = s.DisplayOrder,
                    Position = s.Position,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate
                }).OrderBy(s => s.DisplayOrder).ToList();

                return ServiceResult<List<SliderIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست اسلایدرها");
                return ServiceResult<List<SliderIndexViewModel>>.Failed("خطا در دریافت لیست اسلایدرها");
            }
        }

        public async Task<ServiceResult<SliderDetailsViewModel>> GetSliderDetailsAsync(int sliderId)
        {
            try
            {
                var slider = await _sliderRepository.GetByIdAsync(sliderId);
                if (slider == null)
                {
                    return ServiceResult<SliderDetailsViewModel>.Failed("اسلایدر یافت نشد");
                }

                var viewModel = new SliderDetailsViewModel
                {
                    SliderId = slider.SliderId,
                    Title = slider.Title,
                    Description = slider.Description,
                    ImageUrl = slider.ImageUrl,
                    ThumbnailUrl = slider.ThumbnailUrl,
                    LinkUrl = slider.LinkUrl,
                    ButtonText = slider.ButtonText,
                    IsActive = slider.IsActive,
                    DisplayOrder = slider.DisplayOrder,
                    StartDate = slider.StartDate,
                    EndDate = slider.EndDate,
                    Position = slider.Position,
                    CreatedAt = slider.CreatedAt,
                    CreatedByUserName = slider.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = slider.UpdatedAt,
                    UpdatedByUserName = slider.UpdatedByUser?.UserName
                };

                return ServiceResult<SliderDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات اسلایدر - SliderId: {SliderId}", sliderId);
                return ServiceResult<SliderDetailsViewModel>.Failed("خطا در دریافت جزئیات اسلایدر");
            }
        }

        public async Task<ServiceResult<SliderCreateEditViewModel>> GetSliderForEditAsync(int sliderId)
        {
            try
            {
                var slider = await _sliderRepository.GetByIdAsync(sliderId);
                if (slider == null)
                {
                    return ServiceResult<SliderCreateEditViewModel>.Failed("اسلایدر یافت نشد");
                }

                var viewModel = new SliderCreateEditViewModel
                {
                    SliderId = slider.SliderId,
                    Title = slider.Title,
                    Description = slider.Description,
                    ImageUrl = slider.ImageUrl,
                    ThumbnailUrl = slider.ThumbnailUrl,
                    LinkUrl = slider.LinkUrl,
                    ButtonText = slider.ButtonText,
                    IsActive = slider.IsActive,
                    DisplayOrder = slider.DisplayOrder,
                    StartDate = slider.StartDate,
                    EndDate = slider.EndDate,
                    Position = slider.Position
                };

                return ServiceResult<SliderCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلایدر برای ویرایش - SliderId: {SliderId}", sliderId);
                return ServiceResult<SliderCreateEditViewModel>.Failed("خطا در دریافت اسلایدر برای ویرایش");
            }
        }

        public async Task<ServiceResult<Slider>> CreateSliderAsync(SliderCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد اسلایدر جدید - Title: {Title}", model.Title);

                var slider = new Slider
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImageUrl = model.ImageUrl,
                    ThumbnailUrl = model.ThumbnailUrl,
                    LinkUrl = model.LinkUrl,
                    ButtonText = model.ButtonText,
                    IsActive = model.IsActive,
                    DisplayOrder = model.DisplayOrder,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Position = model.Position ?? "hero",
                    CreatedByUserId = _currentUserService.UserId
                };

                _sliderRepository.Add(slider);
                await _context.SaveChangesAsync();

                _logger.Information("اسلایدر با موفقیت ایجاد شد - SliderId: {SliderId}", slider.SliderId);
                return ServiceResult<Slider>.Successful(slider, "اسلایدر با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اسلایدر");
                return ServiceResult<Slider>.Failed("خطا در ایجاد اسلایدر");
            }
        }

        public async Task<ServiceResult<Slider>> UpdateSliderAsync(SliderCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی اسلایدر - SliderId: {SliderId}", model.SliderId);

                var slider = await _sliderRepository.GetByIdAsync(model.SliderId);
                if (slider == null)
                {
                    return ServiceResult<Slider>.Failed("اسلایدر یافت نشد");
                }

                slider.Title = model.Title;
                slider.Description = model.Description;
                slider.ImageUrl = model.ImageUrl;
                slider.ThumbnailUrl = model.ThumbnailUrl;
                slider.LinkUrl = model.LinkUrl;
                slider.ButtonText = model.ButtonText;
                slider.IsActive = model.IsActive;
                slider.DisplayOrder = model.DisplayOrder;
                slider.StartDate = model.StartDate;
                slider.EndDate = model.EndDate;
                slider.Position = model.Position ?? slider.Position ?? "hero";
                slider.UpdatedByUserId = _currentUserService.UserId;

                _sliderRepository.Update(slider);
                await _context.SaveChangesAsync();

                _logger.Information("اسلایدر با موفقیت به‌روزرسانی شد - SliderId: {SliderId}", slider.SliderId);
                return ServiceResult<Slider>.Successful(slider, "اسلایدر با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اسلایدر - SliderId: {SliderId}", model.SliderId);
                return ServiceResult<Slider>.Failed("خطا در به‌روزرسانی اسلایدر");
            }
        }

        public async Task<ServiceResult> DeleteSliderAsync(int sliderId)
        {
            try
            {
                _logger.Information("حذف اسلایدر - SliderId: {SliderId}", sliderId);

                var slider = await _sliderRepository.GetByIdAsync(sliderId);
                if (slider == null)
                {
                    return ServiceResult.Failed("اسلایدر یافت نشد");
                }

                _sliderRepository.Delete(slider);
                await _context.SaveChangesAsync();

                _logger.Information("اسلایدر با موفقیت حذف شد - SliderId: {SliderId}", sliderId);
                return ServiceResult.Successful("اسلایدر با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اسلایدر - SliderId: {SliderId}", sliderId);
                return ServiceResult.Failed("خطا در حذف اسلایدر");
            }
        }

        public async Task<ServiceResult> ActivateSliderAsync(int sliderId)
        {
            try
            {
                var slider = await _sliderRepository.GetByIdAsync(sliderId);
                if (slider == null)
                {
                    return ServiceResult.Failed("اسلایدر یافت نشد");
                }

                slider.IsActive = true;
                slider.UpdatedByUserId = _currentUserService.UserId;

                _sliderRepository.Update(slider);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("اسلایدر با موفقیت فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی اسلایدر - SliderId: {SliderId}", sliderId);
                return ServiceResult.Failed("خطا در فعال‌سازی اسلایدر");
            }
        }

        public async Task<ServiceResult> DeactivateSliderAsync(int sliderId)
        {
            try
            {
                var slider = await _sliderRepository.GetByIdAsync(sliderId);
                if (slider == null)
                {
                    return ServiceResult.Failed("اسلایدر یافت نشد");
                }

                slider.IsActive = false;
                slider.UpdatedByUserId = _currentUserService.UserId;

                _sliderRepository.Update(slider);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("اسلایدر با موفقیت غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی اسلایدر - SliderId: {SliderId}", sliderId);
                return ServiceResult.Failed("خطا در غیرفعال‌سازی اسلایدر");
            }
        }

        public async Task<ServiceResult> UpdateDisplayOrderAsync(int sliderId, int newOrder)
        {
            try
            {
                var slider = await _sliderRepository.GetByIdAsync(sliderId);
                if (slider == null)
                {
                    return ServiceResult.Failed("اسلایدر یافت نشد");
                }

                slider.DisplayOrder = newOrder;
                slider.UpdatedByUserId = _currentUserService.UserId;

                _sliderRepository.Update(slider);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("ترتیب نمایش با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی ترتیب نمایش - SliderId: {SliderId}", sliderId);
                return ServiceResult.Failed("خطا در به‌روزرسانی ترتیب نمایش");
            }
        }
    }
}

