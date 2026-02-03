using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.PromotionalEvent;
using ClinicApp.Models;
using ClinicApp.Models.DTOs.PromotionalEvent;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Services.PromotionalEvent
{
    /// <summary>
    /// سرویس مدیریت ایونت‌های تبلیغاتی
    /// طراحی شده بر اساس اصول SRP و ServiceResult Pattern
    /// </summary>
    public class PromotionalEventService : IPromotionalEventService
    {
        private readonly IPromotionalEventRepository _repository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public PromotionalEventService(
            IPromotionalEventRepository repository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger?.ForContext<PromotionalEventService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>> CreateAsync(Models.Entities.PromotionalEvent.PromotionalEvent promotionalEvent)
        {
            try
            {
                if (promotionalEvent == null)
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("اطلاعات ایونت نامعتبر است");
                }

                // Validation
                if (promotionalEvent.StartDate >= promotionalEvent.EndDate)
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("تاریخ شروع باید قبل از تاریخ پایان باشد");
                }

                if (promotionalEvent.DiscountValue <= 0)
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("مقدار تخفیف باید بیشتر از صفر باشد");
                }

                if (promotionalEvent.DiscountType == DiscountType.Percentage && promotionalEvent.DiscountValue > 100)
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("تخفیف درصدی نمی‌تواند بیشتر از 100% باشد");
                }

                if (promotionalEvent.IsDoctorSpecific && string.IsNullOrWhiteSpace(promotionalEvent.DoctorIds))
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("در صورت انتخاب محدودیت پزشک، حداقل یک پزشک باید انتخاب شود");
                }

                if (promotionalEvent.TotalSlots.HasValue && promotionalEvent.TotalSlots.Value <= 0)
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("تعداد کل نوبت‌ها باید بیشتر از صفر باشد");
                }

                // Set tracking fields
                promotionalEvent.CreatedAt = DateTime.Now;
                promotionalEvent.CreatedByUserId = _currentUserService.UserId;
                promotionalEvent.UsedSlots = 0;

                // Transaction Management
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        _repository.Add(promotionalEvent);
                        await _context.SaveChangesAsync();

                        // Verification
                        var saved = await _repository.GetByIdAsync(promotionalEvent.EventId);
                        if (saved == null)
                        {
                            transaction.Rollback();
                            return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("ایونت ذخیره نشد");
                        }

                        transaction.Commit();

                        _logger.Information("🎁 PROMOTIONAL EVENT: ایونت جدید ایجاد شد - EventId: {EventId}, Title: {Title}, DiscountType: {DiscountType}, DiscountValue: {DiscountValue}",
                            promotionalEvent.EventId, promotionalEvent.Title, promotionalEvent.DiscountType, promotionalEvent.DiscountValue);

                        return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Successful(promotionalEvent, "ایونت با موفقیت ایجاد شد");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در ایجاد ایونت - Title: {Title}", promotionalEvent?.Title);
                return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("خطا در ایجاد ایونت");
            }
        }

        public async Task<ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>> UpdateAsync(int eventId, Models.Entities.PromotionalEvent.PromotionalEvent promotionalEvent)
        {
            try
            {
                if (promotionalEvent == null)
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("اطلاعات ایونت نامعتبر است");
                }

                var existing = await _repository.GetByIdAsync(eventId);
                if (existing == null)
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("ایونت یافت نشد");
                }

                // Validation
                if (promotionalEvent.StartDate >= promotionalEvent.EndDate)
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("تاریخ شروع باید قبل از تاریخ پایان باشد");
                }

                if (promotionalEvent.DiscountValue <= 0)
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("مقدار تخفیف باید بیشتر از صفر باشد");
                }

                if (promotionalEvent.DiscountType == DiscountType.Percentage && promotionalEvent.DiscountValue > 100)
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("تخفیف درصدی نمی‌تواند بیشتر از 100% باشد");
                }

                if (promotionalEvent.IsDoctorSpecific && string.IsNullOrWhiteSpace(promotionalEvent.DoctorIds))
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("در صورت انتخاب محدودیت پزشک، حداقل یک پزشک باید انتخاب شود");
                }

                if (promotionalEvent.TotalSlots.HasValue && promotionalEvent.TotalSlots.Value < existing.UsedSlots)
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed($"تعداد کل نوبت‌ها نمی‌تواند کمتر از تعداد استفاده شده ({existing.UsedSlots}) باشد");
                }

                // Update fields
                existing.Title = promotionalEvent.Title.Trim();
                existing.Description = promotionalEvent.Description;
                existing.StartDate = promotionalEvent.StartDate;
                existing.EndDate = promotionalEvent.EndDate;
                existing.DiscountType = promotionalEvent.DiscountType;
                existing.DiscountValue = promotionalEvent.DiscountValue;
                existing.TotalSlots = promotionalEvent.TotalSlots;
                existing.IsDoctorSpecific = promotionalEvent.IsDoctorSpecific;
                existing.DoctorIds = promotionalEvent.DoctorIds;
                existing.IsActive = promotionalEvent.IsActive;
                existing.UpdatedAt = DateTime.Now;
                existing.UpdatedByUserId = _currentUserService.UserId;

                // Transaction Management
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        _repository.Update(existing);
                        await _context.SaveChangesAsync();

                        // Verification
                        var saved = await _repository.GetByIdAsync(eventId);
                        if (saved == null)
                        {
                            transaction.Rollback();
                            return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("ایونت به‌روزرسانی نشد");
                        }

                        transaction.Commit();

                        _logger.Information("🎁 PROMOTIONAL EVENT: ایونت به‌روزرسانی شد - EventId: {EventId}, Title: {Title}",
                            eventId, existing.Title);

                        return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Successful(existing, "ایونت با موفقیت به‌روزرسانی شد");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در به‌روزرسانی ایونت - EventId: {EventId}", eventId);
                return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("خطا در به‌روزرسانی ایونت");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int eventId)
        {
            try
            {
                var promotionalEvent = await _repository.GetByIdAsync(eventId);
                if (promotionalEvent == null)
                {
                    return ServiceResult<bool>.Failed("ایونت یافت نشد");
                }

                // Transaction Management
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        _repository.Delete(promotionalEvent);
                        promotionalEvent.DeletedByUserId = _currentUserService.UserId;
                        await _context.SaveChangesAsync();

                        transaction.Commit();

                        _logger.Information("🎁 PROMOTIONAL EVENT: ایونت حذف شد - EventId: {EventId}", eventId);

                        return ServiceResult<bool>.Successful(true, "ایونت با موفقیت حذف شد");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در حذف ایونت - EventId: {EventId}", eventId);
                return ServiceResult<bool>.Failed("خطا در حذف ایونت");
            }
        }

        public async Task<ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>> GetByIdAsync(int eventId)
        {
            try
            {
                var promotionalEvent = await _repository.GetByIdAsync(eventId);
                if (promotionalEvent == null)
                {
                    return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("ایونت یافت نشد");
                }

                return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Successful(promotionalEvent);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در دریافت ایونت - EventId: {EventId}", eventId);
                return ServiceResult<Models.Entities.PromotionalEvent.PromotionalEvent>.Failed("خطا در دریافت ایونت");
            }
        }

        public async Task<ServiceResult<List<Models.Entities.PromotionalEvent.PromotionalEvent>>> GetAllAsync(bool includeDeleted = false)
        {
            try
            {
                var events = await _repository.GetAllAsync(includeDeleted);
                return ServiceResult<List<Models.Entities.PromotionalEvent.PromotionalEvent>>.Successful(events);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در دریافت تمام ایونت‌ها");
                return ServiceResult<List<Models.Entities.PromotionalEvent.PromotionalEvent>>.Failed("خطا در دریافت ایونت‌ها");
            }
        }

        public async Task<ServiceResult<List<Models.Entities.PromotionalEvent.PromotionalEvent>>> GetActiveEventsAsync(DateTime? appointmentDate = null)
        {
            try
            {
                var events = await _repository.GetActiveEventsAsync(appointmentDate);
                return ServiceResult<List<Models.Entities.PromotionalEvent.PromotionalEvent>>.Successful(events);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در دریافت ایونت‌های فعال");
                return ServiceResult<List<Models.Entities.PromotionalEvent.PromotionalEvent>>.Failed("خطا در دریافت ایونت‌های فعال");
            }
        }

        public async Task<ServiceResult<decimal>> CalculateDiscountAsync(int doctorId, decimal basePrice, DateTime? appointmentDate = null)
        {
            try
            {
                // ✅ استفاده از متد جدید با جزئیات
                var result = await CalculateDiscountWithDetailsAsync(doctorId, basePrice, appointmentDate);
                if (!result.Success)
                {
                    return ServiceResult<decimal>.Failed(result.Message);
                }
                return ServiceResult<decimal>.Successful(result.Data.TotalDiscount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ DISCOUNT: خطا در محاسبه تخفیف - DoctorId: {DoctorId}, BasePrice: {BasePrice}", doctorId, basePrice);
                return ServiceResult<decimal>.Failed("خطا در محاسبه تخفیف");
            }
        }

        public async Task<ServiceResult<DiscountResult>> CalculateDiscountWithDetailsAsync(int doctorId, decimal basePrice, DateTime? appointmentDate = null)
        {
            try
            {
                _logger.Information("💰 DISCOUNT: شروع محاسبه تخفیف با جزئیات - DoctorId: {DoctorId}, BasePrice: {BasePrice}, AppointmentDate: {AppointmentDate}",
                    doctorId, basePrice, appointmentDate);

                var appointmentDateTime = appointmentDate ?? DateTime.Now;

                // دریافت ایونت‌های فعال مرتبط با این پزشک
                var activeEvents = await _repository.GetEventsByDoctorAsync(doctorId, appointmentDateTime);

                decimal totalDiscount = 0m;
                int? appliedEventId = null;
                string appliedEventTitle = null;
                decimal maxDiscount = 0m; // برای پیدا کردن ایونت با بیشترین تخفیف

                foreach (var evt in activeEvents)
                {
                    // بررسی محدودیت تعداد استفاده شده
                    if (evt.TotalSlots.HasValue && evt.UsedSlots >= evt.TotalSlots.Value)
                    {
                        _logger.Debug("💰 DISCOUNT: ایونت {EventId} به حداکثر تعداد استفاده رسیده است - UsedSlots: {UsedSlots}/{TotalSlots}",
                            evt.EventId, evt.UsedSlots, evt.TotalSlots);
                        continue;
                    }

                    // محاسبه تخفیف
                    decimal discount = 0m;
                    if (evt.DiscountType == DiscountType.Percentage)
                    {
                        discount = basePrice * (evt.DiscountValue / 100m);
                    }
                    else if (evt.DiscountType == DiscountType.FixedAmount)
                    {
                        discount = evt.DiscountValue;
                    }

                    // محدودیت: تخفیف نمی‌تواند بیشتر از قیمت پایه باشد
                    discount = Math.Min(discount, basePrice);

                    totalDiscount += discount;

                    // ✅ ذخیره ایونت با بیشترین تخفیف
                    if (discount > maxDiscount)
                    {
                        maxDiscount = discount;
                        appliedEventId = evt.EventId;
                        appliedEventTitle = evt.Title;
                    }

                    _logger.Information("💰 DISCOUNT: تخفیف ایونت اعمال شد - EventId: {EventId}, Title: {Title}, Discount: {Discount}, Type: {Type}",
                        evt.EventId, evt.Title, discount, evt.DiscountType);
                }

                // محدودیت: مجموع تخفیف‌ها نمی‌تواند بیشتر از 100% باشد
                totalDiscount = Math.Min(totalDiscount, basePrice);

                var discountResult = new DiscountResult
                {
                    TotalDiscount = totalDiscount,
                    PromotionalEventId = appliedEventId,
                    PromotionalEventTitle = appliedEventTitle
                };

                _logger.Information("💰 DISCOUNT: محاسبه تخفیف تکمیل شد - DoctorId: {DoctorId}, BasePrice: {BasePrice}, TotalDiscount: {TotalDiscount}, PromotionalEventId: {PromotionalEventId}",
                    doctorId, basePrice, totalDiscount, appliedEventId);

                return ServiceResult<DiscountResult>.Successful(discountResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ DISCOUNT: خطا در محاسبه تخفیف با جزئیات - DoctorId: {DoctorId}, BasePrice: {BasePrice}", doctorId, basePrice);
                return ServiceResult<DiscountResult>.Failed("خطا در محاسبه تخفیف");
            }
        }

        public async Task<ServiceResult<bool>> IncrementUsedSlotsAsync(int eventId)
        {
            try
            {
                _logger.Information("📊 PROMOTIONAL EVENT: افزایش تعداد استفاده - EventId: {EventId}", eventId);

                // Transaction Management
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        await _repository.IncrementUsedSlotsAsync(eventId);
                        await _context.SaveChangesAsync();

                        // Verification
                        var updated = await _repository.GetByIdAsync(eventId);
                        if (updated == null)
                        {
                            transaction.Rollback();
                            return ServiceResult<bool>.Failed("ایونت یافت نشد");
                        }

                        transaction.Commit();

                        _logger.Information("✅ PROMOTIONAL EVENT: تعداد استفاده افزایش یافت - EventId: {EventId}, UsedSlots: {UsedSlots}/{TotalSlots}",
                            eventId, updated.UsedSlots, updated.TotalSlots);

                        return ServiceResult<bool>.Successful(true);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در افزایش تعداد استفاده - EventId: {EventId}", eventId);
                return ServiceResult<bool>.Failed("خطا در افزایش تعداد استفاده");
            }
        }
    }
}

