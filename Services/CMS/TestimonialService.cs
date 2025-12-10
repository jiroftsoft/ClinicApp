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
    /// سرویس مدیریت نظرات بیماران
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class TestimonialService : ITestimonialService
    {
        private readonly ITestimonialRepository _testimonialRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public TestimonialService(
            ITestimonialRepository testimonialRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _testimonialRepository = testimonialRepository ?? throw new ArgumentNullException(nameof(testimonialRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<List<TestimonialIndexViewModel>>> GetTestimonialsAsync(bool includePending = false)
        {
            try
            {
                var testimonials = await _testimonialRepository.GetAllAsync(includeDeleted: false);
                
                var query = testimonials.AsQueryable();
                if (!includePending)
                {
                    query = query.Where(t => t.IsApproved);
                }

                var viewModels = query.Select(t => new TestimonialIndexViewModel
                {
                    TestimonialId = t.TestimonialId,
                    PatientName = t.PatientName,
                    PatientInitials = t.PatientInitials,
                    Comment = t.Comment,
                    Rating = t.Rating,
                    DoctorName = t.DoctorName,
                    IsApproved = t.IsApproved,
                    IsFeatured = t.IsFeatured,
                    DisplayOrder = t.DisplayOrder,
                    ApprovedAt = t.ApprovedAt,
                    CreatedAt = t.CreatedAt
                }).OrderByDescending(t => t.IsFeatured)
                  .ThenBy(t => t.DisplayOrder)
                  .ThenByDescending(t => t.ApprovedAt ?? DateTime.MinValue)
                  .ToList();

                return ServiceResult<List<TestimonialIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست نظرات");
                return ServiceResult<List<TestimonialIndexViewModel>>.Failed("خطا در دریافت لیست نظرات");
            }
        }

        public async Task<ServiceResult<TestimonialDetailsViewModel>> GetTestimonialDetailsAsync(int testimonialId)
        {
            try
            {
                var testimonial = await _testimonialRepository.GetByIdAsync(testimonialId);
                if (testimonial == null)
                {
                    return ServiceResult<TestimonialDetailsViewModel>.Failed("نظر یافت نشد");
                }

                var viewModel = new TestimonialDetailsViewModel
                {
                    TestimonialId = testimonial.TestimonialId,
                    PatientName = testimonial.PatientName,
                    PatientInitials = testimonial.PatientInitials,
                    Comment = testimonial.Comment,
                    Rating = testimonial.Rating,
                    DoctorName = testimonial.DoctorName,
                    PhotoUrl = testimonial.PhotoUrl,
                    VideoUrl = testimonial.VideoUrl,
                    IsApproved = testimonial.IsApproved,
                    IsFeatured = testimonial.IsFeatured,
                    DisplayOrder = testimonial.DisplayOrder,
                    ApprovedAt = testimonial.ApprovedAt,
                    PatientId = testimonial.PatientId,
                    DoctorId = testimonial.DoctorId,
                    CreatedAt = testimonial.CreatedAt,
                    CreatedByUserName = testimonial.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = testimonial.UpdatedAt,
                    UpdatedByUserName = testimonial.UpdatedByUser?.UserName
                };

                return ServiceResult<TestimonialDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات نظر - TestimonialId: {TestimonialId}", testimonialId);
                return ServiceResult<TestimonialDetailsViewModel>.Failed("خطا در دریافت جزئیات نظر");
            }
        }

        public async Task<ServiceResult<TestimonialCreateEditViewModel>> GetTestimonialForEditAsync(int testimonialId)
        {
            try
            {
                var testimonial = await _testimonialRepository.GetByIdAsync(testimonialId);
                if (testimonial == null)
                {
                    return ServiceResult<TestimonialCreateEditViewModel>.Failed("نظر یافت نشد");
                }

                var viewModel = new TestimonialCreateEditViewModel
                {
                    TestimonialId = testimonial.TestimonialId,
                    PatientName = testimonial.PatientName,
                    PatientInitials = testimonial.PatientInitials,
                    Comment = testimonial.Comment,
                    Rating = testimonial.Rating,
                    DoctorName = testimonial.DoctorName,
                    PhotoUrl = testimonial.PhotoUrl,
                    VideoUrl = testimonial.VideoUrl,
                    IsApproved = testimonial.IsApproved,
                    IsFeatured = testimonial.IsFeatured,
                    DisplayOrder = testimonial.DisplayOrder,
                    PatientId = testimonial.PatientId,
                    DoctorId = testimonial.DoctorId
                };

                return ServiceResult<TestimonialCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نظر برای ویرایش - TestimonialId: {TestimonialId}", testimonialId);
                return ServiceResult<TestimonialCreateEditViewModel>.Failed("خطا در دریافت نظر برای ویرایش");
            }
        }

        public async Task<ServiceResult<Testimonial>> CreateTestimonialAsync(TestimonialCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد نظر جدید - PatientName: {PatientName}", model.PatientName);

                var testimonial = new Testimonial
                {
                    PatientName = model.PatientName,
                    PatientInitials = model.PatientInitials ?? GenerateInitials(model.PatientName),
                    Comment = model.Comment,
                    Rating = model.Rating,
                    DoctorName = model.DoctorName,
                    PhotoUrl = model.PhotoUrl,
                    VideoUrl = model.VideoUrl,
                    IsApproved = model.IsApproved,
                    IsFeatured = model.IsFeatured,
                    DisplayOrder = model.DisplayOrder,
                    PatientId = model.PatientId,
                    DoctorId = model.DoctorId,
                    CreatedByUserId = _currentUserService.UserId
                };

                if (model.IsApproved)
                {
                    testimonial.ApprovedAt = DateTime.Now;
                }

                _testimonialRepository.Add(testimonial);
                await _context.SaveChangesAsync();

                _logger.Information("نظر با موفقیت ایجاد شد - TestimonialId: {TestimonialId}", testimonial.TestimonialId);
                return ServiceResult<Testimonial>.Successful(testimonial, "نظر با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد نظر");
                return ServiceResult<Testimonial>.Failed("خطا در ایجاد نظر");
            }
        }

        public async Task<ServiceResult<Testimonial>> UpdateTestimonialAsync(TestimonialCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی نظر - TestimonialId: {TestimonialId}", model.TestimonialId);

                var testimonial = await _testimonialRepository.GetByIdAsync(model.TestimonialId);
                if (testimonial == null)
                {
                    return ServiceResult<Testimonial>.Failed("نظر یافت نشد");
                }

                var wasApproved = testimonial.IsApproved;
                testimonial.PatientName = model.PatientName;
                testimonial.PatientInitials = model.PatientInitials ?? testimonial.PatientInitials ?? GenerateInitials(model.PatientName);
                testimonial.Comment = model.Comment;
                testimonial.Rating = model.Rating;
                testimonial.DoctorName = model.DoctorName;
                testimonial.PhotoUrl = model.PhotoUrl;
                testimonial.VideoUrl = model.VideoUrl;
                testimonial.IsApproved = model.IsApproved;
                testimonial.IsFeatured = model.IsFeatured;
                testimonial.DisplayOrder = model.DisplayOrder;
                testimonial.PatientId = model.PatientId;
                testimonial.DoctorId = model.DoctorId;
                testimonial.UpdatedByUserId = _currentUserService.UserId;

                if (model.IsApproved && !wasApproved)
                {
                    testimonial.ApprovedAt = DateTime.Now;
                }
                else if (!model.IsApproved && wasApproved)
                {
                    testimonial.ApprovedAt = null;
                }

                _testimonialRepository.Update(testimonial);
                await _context.SaveChangesAsync();

                _logger.Information("نظر با موفقیت به‌روزرسانی شد - TestimonialId: {TestimonialId}", testimonial.TestimonialId);
                return ServiceResult<Testimonial>.Successful(testimonial, "نظر با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی نظر - TestimonialId: {TestimonialId}", model.TestimonialId);
                return ServiceResult<Testimonial>.Failed("خطا در به‌روزرسانی نظر");
            }
        }

        public async Task<ServiceResult> DeleteTestimonialAsync(int testimonialId)
        {
            try
            {
                _logger.Information("حذف نظر - TestimonialId: {TestimonialId}", testimonialId);

                var testimonial = await _testimonialRepository.GetByIdAsync(testimonialId);
                if (testimonial == null)
                {
                    return ServiceResult.Failed("نظر یافت نشد");
                }

                _testimonialRepository.Delete(testimonial);
                await _context.SaveChangesAsync();

                _logger.Information("نظر با موفقیت حذف شد - TestimonialId: {TestimonialId}", testimonialId);
                return ServiceResult.Successful("نظر با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نظر - TestimonialId: {TestimonialId}", testimonialId);
                return ServiceResult.Failed("خطا در حذف نظر");
            }
        }

        public async Task<ServiceResult> ApproveTestimonialAsync(int testimonialId)
        {
            try
            {
                var testimonial = await _testimonialRepository.GetByIdAsync(testimonialId);
                if (testimonial == null)
                {
                    return ServiceResult.Failed("نظر یافت نشد");
                }

                testimonial.IsApproved = true;
                if (!testimonial.ApprovedAt.HasValue)
                {
                    testimonial.ApprovedAt = DateTime.Now;
                }
                testimonial.UpdatedByUserId = _currentUserService.UserId;

                _testimonialRepository.Update(testimonial);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("نظر با موفقیت تایید شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تایید نظر - TestimonialId: {TestimonialId}", testimonialId);
                return ServiceResult.Failed("خطا در تایید نظر");
            }
        }

        public async Task<ServiceResult> RejectTestimonialAsync(int testimonialId)
        {
            try
            {
                var testimonial = await _testimonialRepository.GetByIdAsync(testimonialId);
                if (testimonial == null)
                {
                    return ServiceResult.Failed("نظر یافت نشد");
                }

                testimonial.IsApproved = false;
                testimonial.ApprovedAt = null;
                testimonial.UpdatedByUserId = _currentUserService.UserId;

                _testimonialRepository.Update(testimonial);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("نظر با موفقیت رد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در رد نظر - TestimonialId: {TestimonialId}", testimonialId);
                return ServiceResult.Failed("خطا در رد نظر");
            }
        }

        public async Task<ServiceResult> SetFeaturedAsync(int testimonialId, bool isFeatured)
        {
            try
            {
                var testimonial = await _testimonialRepository.GetByIdAsync(testimonialId);
                if (testimonial == null)
                {
                    return ServiceResult.Failed("نظر یافت نشد");
                }

                testimonial.IsFeatured = isFeatured;
                testimonial.UpdatedByUserId = _currentUserService.UserId;

                _testimonialRepository.Update(testimonial);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful(isFeatured ? "نظر به عنوان ویژه تنظیم شد" : "نظر از حالت ویژه خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه نظر - TestimonialId: {TestimonialId}", testimonialId);
                return ServiceResult.Failed("خطا در تنظیم وضعیت ویژه نظر");
            }
        }

        public async Task<ServiceResult<List<TestimonialIndexViewModel>>> GetPendingApprovalAsync()
        {
            try
            {
                var testimonials = await _testimonialRepository.GetAllAsync(includeDeleted: false);
                
                var pending = testimonials
                    .Where(t => !t.IsApproved)
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new TestimonialIndexViewModel
                    {
                        TestimonialId = t.TestimonialId,
                        PatientName = t.PatientName,
                        PatientInitials = t.PatientInitials,
                        Comment = t.Comment,
                        Rating = t.Rating,
                        DoctorName = t.DoctorName,
                        IsApproved = t.IsApproved,
                        IsFeatured = t.IsFeatured,
                        DisplayOrder = t.DisplayOrder,
                        ApprovedAt = t.ApprovedAt,
                        CreatedAt = t.CreatedAt
                    })
                    .ToList();

                return ServiceResult<List<TestimonialIndexViewModel>>.Successful(pending);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نظرات در انتظار تایید");
                return ServiceResult<List<TestimonialIndexViewModel>>.Failed("خطا در دریافت نظرات در انتظار تایید");
            }
        }

        #region Helper Methods

        private string GenerateInitials(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "?";

            var parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return name.Substring(0, Math.Min(2, name.Length)).ToUpper();

            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        #endregion
    }
}

