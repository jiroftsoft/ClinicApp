using ClinicApp.Interfaces.Payment.Gateway;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Repositories.Payment.Gateway
{
    /// <summary>
    /// پیاده‌سازی مخزن درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayRepository : IPaymentGatewayRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public PaymentGatewayRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        #region CRUD Operations

        public async Task<PaymentGateway> GetByIdAsync(int gatewayId)
        {
            try
            {
                return await _context.PaymentGateways
                    .Include(pg => pg.CreatedByUser)
                    .Include(pg => pg.UpdatedByUser)
                    .FirstOrDefaultAsync(pg => pg.PaymentGatewayId == gatewayId && !pg.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت درگاه پرداخت. شناسه: {GatewayId}", gatewayId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentGateway>> GetAllAsync(int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                return await _context.PaymentGateways
                    .Include(pg => pg.CreatedByUser)
                    .Include(pg => pg.UpdatedByUser)
                    .Where(pg => !pg.IsDeleted)
                    .OrderByDescending(pg => pg.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست درگاه‌های پرداخت");
                throw;
            }
        }

        public async Task<PaymentGateway> CreateAsync(PaymentGateway gateway)
        {
            try
            {
                _context.PaymentGateways.Add(gateway);
                await _context.SaveChangesAsync();
                return gateway;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد درگاه پرداخت");
                throw;
            }
        }

        public async Task<PaymentGateway> AddAsync(PaymentGateway gateway)
        {
            return await CreateAsync(gateway);
        }

        public async Task<PaymentGateway> UpdateAsync(PaymentGateway gateway)
        {
            try
            {
                _context.Entry(gateway).State = System.Data.Entity.EntityState.Modified;
                await _context.SaveChangesAsync();
                return gateway;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی درگاه پرداخت. شناسه: {GatewayId}", gateway.PaymentGatewayId);
                throw;
            }
        }

        public async Task<ServiceResult> SoftDeleteAsync(int gatewayId, string deletedByUserId)
        {
            try
            {
                var gateway = await _context.PaymentGateways
                    .FirstOrDefaultAsync(pg => pg.PaymentGatewayId == gatewayId && !pg.IsDeleted);

                if (gateway == null)
                {
                    return ServiceResult.Failed("درگاه پرداخت یافت نشد");
                }

                gateway.IsDeleted = true;
                gateway.DeletedAt = DateTime.UtcNow;
                gateway.DeletedByUserId = deletedByUserId;

                await _context.SaveChangesAsync();
                return ServiceResult.Successful("درگاه پرداخت با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف درگاه پرداخت. شناسه: {GatewayId}", gatewayId);
                return ServiceResult.Failed("خطا در حذف درگاه پرداخت");
            }
        }

        #endregion

        #region Query Operations

        public async Task<IEnumerable<PaymentGateway>> GetActiveGatewaysAsync()
        {
            try
            {
                return await _context.PaymentGateways
                    .Include(pg => pg.CreatedByUser)
                    .Where(pg => !pg.IsDeleted && pg.IsActive)
                    .OrderBy(pg => pg.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت درگاه‌های فعال");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentGateway>> GetDefaultGatewaysAsync()
        {
            try
            {
                return await _context.PaymentGateways
                    .Include(pg => pg.CreatedByUser)
                    .Where(pg => !pg.IsDeleted && pg.IsDefault)
                    .OrderBy(pg => pg.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت درگاه‌های پیش‌فرض");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentGateway>> GetByTypeAsync(PaymentGatewayType gatewayType)
        {
            try
            {
                return await _context.PaymentGateways
                    .Include(pg => pg.CreatedByUser)
                    .Where(pg => !pg.IsDeleted && pg.GatewayType == gatewayType)
                    .OrderBy(pg => pg.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت درگاه‌ها بر اساس نوع. نوع: {GatewayType}", gatewayType);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentGateway>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.PaymentGateways
                    .Include(pg => pg.CreatedByUser)
                    .Where(pg => !pg.IsDeleted);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(pg => 
                        pg.Name.Contains(searchTerm) ||
                        pg.Description.Contains(searchTerm) ||
                        pg.MerchantId.Contains(searchTerm));
                }

                return await query
                    .OrderByDescending(pg => pg.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی درگاه‌های پرداخت. عبارت: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<PaymentGateway> GetByMerchantIdAsync(string merchantId)
        {
            try
            {
                return await _context.PaymentGateways
                    .Include(pg => pg.CreatedByUser)
                    .Include(pg => pg.UpdatedByUser)
                    .FirstOrDefaultAsync(pg => !pg.IsDeleted && pg.MerchantId == merchantId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت درگاه پرداخت بر اساس MerchantId. شناسه: {MerchantId}", merchantId);
                throw;
            }
        }

        #endregion

        #region Management Operations

        public async Task ClearDefaultGatewaysAsync()
        {
            try
            {
                var defaultGateways = await _context.PaymentGateways
                    .Where(pg => !pg.IsDeleted && pg.IsDefault)
                    .ToListAsync();

                foreach (var gateway in defaultGateways)
                {
                    gateway.IsDefault = false;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پاک کردن درگاه‌های پیش‌فرض");
                throw;
            }
        }

        public async Task SetAsDefaultAsync(int gatewayId)
        {
            try
            {
                // پاک کردن درگاه‌های پیش‌فرض قبلی
                await ClearDefaultGatewaysAsync();

                // تنظیم درگاه جدید به عنوان پیش‌فرض
                var gateway = await _context.PaymentGateways
                    .FirstOrDefaultAsync(pg => pg.PaymentGatewayId == gatewayId && !pg.IsDeleted);

                if (gateway != null)
                {
                    gateway.IsDefault = true;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم درگاه پیش‌فرض. شناسه: {GatewayId}", gatewayId);
                throw;
            }
        }

        public async Task ToggleStatusAsync(int gatewayId, bool isActive)
        {
            try
            {
                var gateway = await _context.PaymentGateways
                    .FirstOrDefaultAsync(pg => pg.PaymentGatewayId == gatewayId && !pg.IsDeleted);

                if (gateway != null)
                {
                    gateway.IsActive = isActive;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تغییر وضعیت درگاه. شناسه: {GatewayId}, وضعیت: {IsActive}", gatewayId, isActive);
                throw;
            }
        }

        #endregion

        #region Validation Operations

        public async Task<bool> ExistsAsync(int gatewayId)
        {
            try
            {
                return await _context.PaymentGateways
                    .AnyAsync(pg => pg.PaymentGatewayId == gatewayId && !pg.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود درگاه. شناسه: {GatewayId}", gatewayId);
                throw;
            }
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            try
            {
                var query = _context.PaymentGateways
                    .Where(pg => !pg.IsDeleted && pg.Name == name);

                if (excludeId.HasValue)
                {
                    query = query.Where(pg => pg.PaymentGatewayId != excludeId.Value);
                }

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی یکتایی نام درگاه. نام: {Name}", name);
                throw;
            }
        }

        public async Task<int> GetCountAsync()
        {
            try
            {
                return await _context.PaymentGateways
                    .CountAsync(pg => !pg.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش درگاه‌های پرداخت");
                throw;
            }
        }

        #endregion
    }
}
