using ClinicApp.Interfaces.Payment;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Repositories.Payment
{
    /// <summary>
    /// Repository برای مدیریت پرداخت‌های آنلاین
    /// طراحی شده طبق اصول SRP - مسئولیت: پیاده‌سازی عملیات CRUD پرداخت‌های آنلاین
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل IOnlinePaymentRepository
    /// 2. بهینه‌سازی برای عملکرد بالا (10k+ تراکنش)
    /// 3. پشتیبانی از Soft Delete و Audit Trail
    /// 4. مدیریت خطا و Logging کامل
    /// 5. استفاده از AsNoTracking برای بهبود عملکرد
    /// </summary>
    public class OnlinePaymentRepository : IOnlinePaymentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public OnlinePaymentRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region CRUD Operations

        public async Task<OnlinePayment> GetByIdAsync(int onlinePaymentId)
        {
            try
            {
                return await _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .Include(op => op.Appointment)
                    .Include(op => op.Patient)
                    .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePaymentId && !op.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت آنلاین با شناسه: {OnlinePaymentId}", onlinePaymentId);
                throw;
            }
        }

        public async Task<IEnumerable<OnlinePayment>> GetAllAsync(int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                return await _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .Include(op => op.Patient)
                    .Where(op => !op.IsDeleted)
                    .OrderByDescending(op => op.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پرداخت‌های آنلاین");
                throw;
            }
        }

        public async Task<OnlinePayment> AddAsync(OnlinePayment onlinePayment)
        {
            try
            {
                _context.OnlinePayments.Add(onlinePayment);
                await _context.SaveChangesAsync();
                return onlinePayment;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن پرداخت آنلاین");
                throw;
            }
        }

        public async Task<OnlinePayment> CreateAsync(OnlinePayment onlinePayment)
        {
            try
            {
                _context.OnlinePayments.Add(onlinePayment);
                await _context.SaveChangesAsync();
                return onlinePayment;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد پرداخت آنلاین");
                throw;
            }
        }

        public async Task<OnlinePayment> UpdateAsync(OnlinePayment onlinePayment)
        {
            try
            {
                _context.Entry(onlinePayment).State = System.Data.Entity.EntityState.Modified;
                await _context.SaveChangesAsync();
                return onlinePayment;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی پرداخت آنلاین");
                throw;
            }
        }

        public async Task<ServiceResult> SoftDeleteAsync(int onlinePaymentId, string deletedByUserId)
        {
            try
            {
                var payment = await _context.OnlinePayments
                    .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePaymentId && !op.IsDeleted);

                if (payment == null)
                {
                    return ServiceResult.Failed("پرداخت آنلاین یافت نشد");
                }

                payment.IsDeleted = true;
                payment.DeletedAt = DateTime.UtcNow;
                payment.DeletedByUserId = deletedByUserId;

                await _context.SaveChangesAsync();
                return ServiceResult.Successful("پرداخت آنلاین با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف پرداخت آنلاین. شناسه: {OnlinePaymentId}", onlinePaymentId);
                return ServiceResult.Failed("خطا در حذف پرداخت آنلاین");
            }
        }

        #endregion

        #region Query Operations

        public async Task<IEnumerable<OnlinePayment>> GetByGatewayIdAsync(int gatewayId, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                return await _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .Where(op => op.PaymentGatewayId == gatewayId && !op.IsDeleted)
                    .OrderByDescending(op => op.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت‌های آنلاین بر اساس درگاه");
                throw;
            }
        }

        public async Task<IEnumerable<OnlinePayment>> GetByPatientIdAsync(int patientId, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                return await _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .Include(op => op.Patient)
                    .Where(op => op.PatientId == patientId && !op.IsDeleted)
                    .OrderByDescending(op => op.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت‌های آنلاین بر اساس بیمار");
                throw;
            }
        }

        public async Task<IEnumerable<OnlinePayment>> GetByReceptionIdAsync(int receptionId)
        {
            try
            {
                return await _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .Where(op => op.ReceptionId == receptionId && !op.IsDeleted)
                    .OrderByDescending(op => op.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت‌های آنلاین بر اساس پذیرش");
                throw;
            }
        }

        public async Task<IEnumerable<OnlinePayment>> GetByAppointmentIdAsync(int appointmentId)
        {
            try
            {
                return await _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Appointment)
                    .Where(op => op.AppointmentId == appointmentId && !op.IsDeleted)
                    .OrderByDescending(op => op.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت‌های آنلاین بر اساس نوبت");
                throw;
            }
        }

        public async Task<IEnumerable<OnlinePayment>> GetByPaymentTypeAsync(OnlinePaymentType paymentType, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .Where(op => op.PaymentType == paymentType && !op.IsDeleted);

                if (startDate.HasValue)
                    query = query.Where(op => op.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(op => op.CreatedAt <= endDate.Value);

                return await query
                    .OrderByDescending(op => op.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت‌های آنلاین بر اساس نوع پرداخت");
                throw;
            }
        }

        public async Task<IEnumerable<OnlinePayment>> GetByStatusAsync(OnlinePaymentStatus status, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .Where(op => op.Status == status && !op.IsDeleted);

                if (startDate.HasValue)
                    query = query.Where(op => op.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(op => op.CreatedAt <= endDate.Value);

                return await query
                    .OrderByDescending(op => op.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت‌های آنلاین بر اساس وضعیت");
                throw;
            }
        }

        public async Task<IEnumerable<OnlinePayment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                return await _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .Where(op => op.CreatedAt >= startDate && op.CreatedAt <= endDate && !op.IsDeleted)
                    .OrderByDescending(op => op.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت‌های آنلاین بر اساس تاریخ");
                throw;
            }
        }

        public async Task<IEnumerable<OnlinePayment>> GetExpiredPaymentsAsync(DateTime currentDate)
        {
            try
            {
                return await _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .Where(op => op.ExpiresAt < currentDate && 
                               op.Status == OnlinePaymentStatus.Pending && 
                               !op.IsDeleted)
                    .OrderBy(op => op.ExpiresAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت‌های آنلاین منقضی شده");
                throw;
            }
        }

        public async Task<IEnumerable<OnlinePayment>> GetPendingPaymentsAsync(DateTime olderThan)
        {
            try
            {
                return await _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .Where(op => op.Status == OnlinePaymentStatus.Pending && 
                               op.CreatedAt < olderThan && 
                               !op.IsDeleted)
                    .OrderBy(op => op.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت‌های آنلاین در انتظار");
                throw;
            }
        }

        #endregion

        #region Search Operations

        public async Task<IEnumerable<OnlinePayment>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .Include(op => op.Patient)
                    .Where(op => !op.IsDeleted);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(op => 
                        op.GatewayTransactionId.Contains(searchTerm) ||
                        op.InternalTransactionId.Contains(searchTerm) ||
                        op.PaymentToken.Contains(searchTerm) ||
                        op.ReferenceCode.Contains(searchTerm));
                }

                return await query
                    .OrderByDescending(op => op.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پرداخت‌های آنلاین");
                throw;
            }
        }

        public async Task<IEnumerable<OnlinePayment>> AdvancedSearchAsync(OnlinePaymentSearchFilters filters, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .Include(op => op.Patient)
                    .Where(op => !op.IsDeleted);

                if (filters != null)
                {
                    if (filters.PaymentGatewayId.HasValue)
                        query = query.Where(op => op.PaymentGatewayId == filters.PaymentGatewayId.Value);

                    if (filters.ReceptionId.HasValue)
                        query = query.Where(op => op.ReceptionId == filters.ReceptionId.Value);

                    if (filters.AppointmentId.HasValue)
                        query = query.Where(op => op.AppointmentId == filters.AppointmentId.Value);

                    if (filters.PatientId.HasValue)
                        query = query.Where(op => op.PatientId == filters.PatientId.Value);

                    if (filters.PaymentType.HasValue)
                        query = query.Where(op => op.PaymentType == filters.PaymentType.Value);

                    if (filters.Status.HasValue)
                        query = query.Where(op => op.Status == filters.Status.Value);

                    if (filters.MinAmount.HasValue)
                        query = query.Where(op => op.Amount >= filters.MinAmount.Value);

                    if (filters.MaxAmount.HasValue)
                        query = query.Where(op => op.Amount <= filters.MaxAmount.Value);

                    if (filters.StartDate.HasValue)
                        query = query.Where(op => op.CreatedAt >= filters.StartDate.Value);

                    if (filters.EndDate.HasValue)
                        query = query.Where(op => op.CreatedAt <= filters.EndDate.Value);

                    if (!string.IsNullOrEmpty(filters.GatewayTransactionId))
                        query = query.Where(op => op.GatewayTransactionId.Contains(filters.GatewayTransactionId));

                    if (!string.IsNullOrEmpty(filters.InternalTransactionId))
                        query = query.Where(op => op.InternalTransactionId.Contains(filters.InternalTransactionId));

                    if (!string.IsNullOrEmpty(filters.PaymentToken))
                        query = query.Where(op => op.PaymentToken.Contains(filters.PaymentToken));

                    if (!string.IsNullOrEmpty(filters.ErrorCode))
                        query = query.Where(op => op.ErrorCode.Contains(filters.ErrorCode));

                    if (filters.IsRefunded.HasValue)
                        query = query.Where(op => op.IsRefunded == filters.IsRefunded.Value);

                    if (!string.IsNullOrEmpty(filters.CreatedByUserId))
                        query = query.Where(op => op.CreatedByUserId == filters.CreatedByUserId);

                    if (filters.IsDeleted.HasValue)
                        query = query.Where(op => op.IsDeleted == filters.IsDeleted.Value);
                }

                return await query
                    .OrderByDescending(op => op.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پیشرفته پرداخت‌های آنلاین");
                throw;
            }
        }

        #endregion

        #region Transaction Operations

        public async Task<OnlinePayment> GetByGatewayTransactionIdAsync(string gatewayTransactionId)
        {
            try
            {
                return await _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .FirstOrDefaultAsync(op => op.GatewayTransactionId == gatewayTransactionId && !op.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت آنلاین بر اساس شماره تراکنش درگاه");
                throw;
            }
        }

        public async Task<OnlinePayment> GetByInternalTransactionIdAsync(string internalTransactionId)
        {
            try
            {
                return await _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .FirstOrDefaultAsync(op => op.InternalTransactionId == internalTransactionId && !op.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت آنلاین بر اساس شماره تراکنش داخلی");
                throw;
            }
        }

        public async Task<OnlinePayment> GetByPaymentTokenAsync(string paymentToken)
        {
            try
            {
                return await _context.OnlinePayments
                    .AsNoTracking()
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Reception)
                    .FirstOrDefaultAsync(op => op.PaymentToken == paymentToken && !op.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرداخت آنلاین بر اساس توکن پرداخت");
                throw;
            }
        }

        public async Task<ServiceResult> UpdateStatusAsync(int onlinePaymentId, OnlinePaymentStatus status, string gatewayTransactionId = null, string gatewayReferenceCode = null, string errorCode = null, string errorMessage = null, string updatedByUserId = null)
        {
            try
            {
                var payment = await _context.OnlinePayments
                    .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePaymentId && !op.IsDeleted);

                if (payment == null)
                    return ServiceResult.Failed("پرداخت آنلاین یافت نشد");

                payment.Status = status;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.UpdatedByUserId = updatedByUserId;

                if (!string.IsNullOrEmpty(gatewayTransactionId))
                    payment.GatewayTransactionId = gatewayTransactionId;

                if (!string.IsNullOrEmpty(gatewayReferenceCode))
                    payment.ReferenceCode = gatewayReferenceCode;

                if (!string.IsNullOrEmpty(errorCode))
                    payment.ErrorCode = errorCode;

                if (!string.IsNullOrEmpty(errorMessage))
                    payment.ErrorMessage = errorMessage;

                if (status == OnlinePaymentStatus.Success)
                    payment.CompletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult.Successful("وضعیت پرداخت با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی وضعیت پرداخت آنلاین");
                return ServiceResult.Failed("خطا در به‌روزرسانی وضعیت پرداخت");
            }
        }

        public async Task<ServiceResult> CompletePaymentAsync(int onlinePaymentId, string gatewayTransactionId, string gatewayReferenceCode, decimal? gatewayFee = null, decimal? netAmount = null, string updatedByUserId = null)
        {
            try
            {
                var payment = await _context.OnlinePayments
                    .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePaymentId && !op.IsDeleted);

                if (payment == null)
                    return ServiceResult.Failed("پرداخت آنلاین یافت نشد");

                payment.Status = OnlinePaymentStatus.Success;
                payment.GatewayTransactionId = gatewayTransactionId;
                payment.ReferenceCode = gatewayReferenceCode;
                payment.GatewayFee = gatewayFee;
                payment.NetAmount = netAmount;
                payment.CompletedAt = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.UpdatedByUserId = updatedByUserId;

                await _context.SaveChangesAsync();
                return ServiceResult.Successful("پرداخت با موفقیت تکمیل شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تکمیل پرداخت آنلاین");
                return ServiceResult.Failed("خطا در تکمیل پرداخت");
            }
        }

        public async Task<ServiceResult> CancelPaymentAsync(int onlinePaymentId, string errorCode = null, string errorMessage = null, string updatedByUserId = null)
        {
            try
            {
                var payment = await _context.OnlinePayments
                    .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePaymentId && !op.IsDeleted);

                if (payment == null)
                    return ServiceResult.Failed("پرداخت آنلاین یافت نشد");

                payment.Status = OnlinePaymentStatus.Canceled;
                payment.ErrorCode = errorCode;
                payment.ErrorMessage = errorMessage;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.UpdatedByUserId = updatedByUserId;

                await _context.SaveChangesAsync();
                return ServiceResult.Successful("پرداخت با موفقیت لغو شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو پرداخت آنلاین");
                return ServiceResult.Failed("خطا در لغو پرداخت");
            }
        }

        public async Task<ServiceResult> RefundPaymentAsync(int onlinePaymentId, decimal refundAmount, string refundReason, string updatedByUserId)
        {
            try
            {
                var payment = await _context.OnlinePayments
                    .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePaymentId && !op.IsDeleted);

                if (payment == null)
                    return ServiceResult.Failed("پرداخت آنلاین یافت نشد");

                payment.IsRefunded = true;
                payment.RefundAmount = refundAmount;
                payment.RefundReason = refundReason;
                payment.RefundDate = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.UpdatedByUserId = updatedByUserId;

                await _context.SaveChangesAsync();
                return ServiceResult.Successful("پرداخت با موفقیت برگشت داده شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در برگشت پرداخت آنلاین");
                return ServiceResult.Failed("خطا در برگشت پرداخت");
            }
        }

        #endregion

        #region Validation Operations

        public async Task<bool> ExistsAsync(int onlinePaymentId)
        {
            try
            {
                return await _context.OnlinePayments
                    .AnyAsync(op => op.OnlinePaymentId == onlinePaymentId && !op.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود پرداخت آنلاین. شناسه: {OnlinePaymentId}", onlinePaymentId);
                throw;
            }
        }

        public async Task<bool> ExistsByGatewayTransactionIdAsync(string gatewayTransactionId)
        {
            try
            {
                return await _context.OnlinePayments
                    .AnyAsync(op => op.GatewayTransactionId == gatewayTransactionId && !op.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود پرداخت آنلاین بر اساس شماره تراکنش درگاه");
                throw;
            }
        }

        public async Task<bool> ExistsByInternalTransactionIdAsync(string internalTransactionId)
        {
            try
            {
                return await _context.OnlinePayments
                    .AnyAsync(op => op.InternalTransactionId == internalTransactionId && !op.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود پرداخت آنلاین بر اساس شماره تراکنش داخلی");
                throw;
            }
        }

        public async Task<int> GetCountAsync(OnlinePaymentSearchFilters filters = null)
        {
            try
            {
                var query = _context.OnlinePayments.Where(op => !op.IsDeleted);

                if (filters != null)
                {
                    if (filters.PaymentGatewayId.HasValue)
                        query = query.Where(op => op.PaymentGatewayId == filters.PaymentGatewayId.Value);

                    if (filters.ReceptionId.HasValue)
                        query = query.Where(op => op.ReceptionId == filters.ReceptionId.Value);

                    if (filters.AppointmentId.HasValue)
                        query = query.Where(op => op.AppointmentId == filters.AppointmentId.Value);

                    if (filters.PatientId.HasValue)
                        query = query.Where(op => op.PatientId == filters.PatientId.Value);

                    if (filters.PaymentType.HasValue)
                        query = query.Where(op => op.PaymentType == filters.PaymentType.Value);

                    if (filters.Status.HasValue)
                        query = query.Where(op => op.Status == filters.Status.Value);

                    if (filters.MinAmount.HasValue)
                        query = query.Where(op => op.Amount >= filters.MinAmount.Value);

                    if (filters.MaxAmount.HasValue)
                        query = query.Where(op => op.Amount <= filters.MaxAmount.Value);

                    if (filters.StartDate.HasValue)
                        query = query.Where(op => op.CreatedAt >= filters.StartDate.Value);

                    if (filters.EndDate.HasValue)
                        query = query.Where(op => op.CreatedAt <= filters.EndDate.Value);

                    if (!string.IsNullOrEmpty(filters.GatewayTransactionId))
                        query = query.Where(op => op.GatewayTransactionId.Contains(filters.GatewayTransactionId));

                    if (!string.IsNullOrEmpty(filters.InternalTransactionId))
                        query = query.Where(op => op.InternalTransactionId.Contains(filters.InternalTransactionId));

                    if (!string.IsNullOrEmpty(filters.PaymentToken))
                        query = query.Where(op => op.PaymentToken.Contains(filters.PaymentToken));

                    if (!string.IsNullOrEmpty(filters.ErrorCode))
                        query = query.Where(op => op.ErrorCode.Contains(filters.ErrorCode));

                    if (filters.IsRefunded.HasValue)
                        query = query.Where(op => op.IsRefunded == filters.IsRefunded.Value);

                    if (!string.IsNullOrEmpty(filters.CreatedByUserId))
                        query = query.Where(op => op.CreatedByUserId == filters.CreatedByUserId);

                    if (filters.IsDeleted.HasValue)
                        query = query.Where(op => op.IsDeleted == filters.IsDeleted.Value);
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش پرداخت‌های آنلاین");
                throw;
            }
        }

        #endregion

        #region Statistics Operations

        public async Task<OnlinePaymentStatistics> GetStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var payments = await _context.OnlinePayments
                    .AsNoTracking()
                    .Where(op => op.CreatedAt >= startDate && op.CreatedAt <= endDate && !op.IsDeleted)
                    .ToListAsync();

                return new OnlinePaymentStatistics
                {
                    TotalPayments = payments.Count,
                    TotalAmount = payments.Sum(p => p.Amount),
                    AverageAmount = payments.Any() ? payments.Average(p => p.Amount) : 0,
                    MinAmount = payments.Any() ? payments.Min(p => p.Amount) : 0,
                    MaxAmount = payments.Any() ? payments.Max(p => p.Amount) : 0,
                    SuccessfulPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Success),
                    FailedPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Failed),
                    PendingPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Pending),
                    CanceledPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Canceled),
                    RefundedPayments = payments.Count(p => p.IsRefunded),
                    ExpiredPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Expired),
                    SuccessRate = payments.Any() ? (decimal)payments.Count(p => p.Status == OnlinePaymentStatus.Success) / payments.Count * 100 : 0,
                    RefundRate = payments.Any() ? (decimal)payments.Count(p => p.IsRefunded) / payments.Count * 100 : 0,
                    TotalGatewayFees = payments.Sum(p => p.GatewayFee ?? 0),
                    TotalNetAmount = payments.Sum(p => p.NetAmount ?? 0),
                    PaymentsByType = payments.GroupBy(p => p.PaymentType).ToDictionary(g => g.Key, g => g.Count()),
                    PaymentsByStatus = payments.GroupBy(p => p.Status).ToDictionary(g => g.Key, g => g.Count()),
                    PaymentsByGateway = payments.GroupBy(p => p.PaymentGatewayId)
                        .ToDictionary(g => g.Key, g => new GatewayPaymentInfo
                        {
                            GatewayId = g.Key,
                            GatewayName = g.First().PaymentGateway?.Name ?? "نامشخص",
                            GatewayType = g.First().PaymentGateway?.GatewayType ?? PaymentGatewayType.ZarinPal,
                            PaymentCount = g.Count(),
                            TotalAmount = g.Sum(p => p.Amount),
                            SuccessRate = g.Any() ? (decimal)g.Count(p => p.Status == OnlinePaymentStatus.Success) / g.Count() * 100 : 0,
                            AverageAmount = g.Any() ? g.Average(p => p.Amount) : 0,
                            TotalGatewayFees = g.Sum(p => p.GatewayFee ?? 0)
                        })
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار پرداخت‌های آنلاین");
                throw;
            }
        }

        public async Task<OnlinePaymentStatistics> GetStatisticsByGatewayAsync(int gatewayId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var payments = await _context.OnlinePayments
                    .AsNoTracking()
                    .Where(op => op.PaymentGatewayId == gatewayId && 
                               op.CreatedAt >= startDate && 
                               op.CreatedAt <= endDate && 
                               !op.IsDeleted)
                    .ToListAsync();

                return new OnlinePaymentStatistics
                {
                    TotalPayments = payments.Count,
                    TotalAmount = payments.Sum(p => p.Amount),
                    AverageAmount = payments.Any() ? payments.Average(p => p.Amount) : 0,
                    MinAmount = payments.Any() ? payments.Min(p => p.Amount) : 0,
                    MaxAmount = payments.Any() ? payments.Max(p => p.Amount) : 0,
                    SuccessfulPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Success),
                    FailedPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Failed),
                    PendingPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Pending),
                    CanceledPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Canceled),
                    RefundedPayments = payments.Count(p => p.IsRefunded),
                    ExpiredPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Expired),
                    SuccessRate = payments.Any() ? (decimal)payments.Count(p => p.Status == OnlinePaymentStatus.Success) / payments.Count * 100 : 0,
                    RefundRate = payments.Any() ? (decimal)payments.Count(p => p.IsRefunded) / payments.Count * 100 : 0,
                    TotalGatewayFees = payments.Sum(p => p.GatewayFee ?? 0),
                    TotalNetAmount = payments.Sum(p => p.NetAmount ?? 0),
                    PaymentsByType = payments.GroupBy(p => p.PaymentType).ToDictionary(g => g.Key, g => g.Count()),
                    PaymentsByStatus = payments.GroupBy(p => p.Status).ToDictionary(g => g.Key, g => g.Count()),
                    PaymentsByGateway = new Dictionary<int, GatewayPaymentInfo>
                    {
                        [gatewayId] = new GatewayPaymentInfo
                        {
                            GatewayId = gatewayId,
                            GatewayName = payments.FirstOrDefault()?.PaymentGateway?.Name ?? "نامشخص",
                            GatewayType = payments.FirstOrDefault()?.PaymentGateway?.GatewayType ?? PaymentGatewayType.ZarinPal,
                            PaymentCount = payments.Count,
                            TotalAmount = payments.Sum(p => p.Amount),
                            SuccessRate = payments.Any() ? (decimal)payments.Count(p => p.Status == OnlinePaymentStatus.Success) / payments.Count * 100 : 0,
                            AverageAmount = payments.Any() ? payments.Average(p => p.Amount) : 0,
                            TotalGatewayFees = payments.Sum(p => p.GatewayFee ?? 0)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار پرداخت‌های آنلاین بر اساس درگاه");
                throw;
            }
        }

        public async Task<OnlinePaymentStatistics> GetStatisticsByPaymentTypeAsync(OnlinePaymentType paymentType, DateTime startDate, DateTime endDate)
        {
            try
            {
                var payments = await _context.OnlinePayments
                    .AsNoTracking()
                    .Where(op => op.PaymentType == paymentType && 
                               op.CreatedAt >= startDate && 
                               op.CreatedAt <= endDate && 
                               !op.IsDeleted)
                    .ToListAsync();

                return new OnlinePaymentStatistics
                {
                    TotalPayments = payments.Count,
                    TotalAmount = payments.Sum(p => p.Amount),
                    AverageAmount = payments.Any() ? payments.Average(p => p.Amount) : 0,
                    MinAmount = payments.Any() ? payments.Min(p => p.Amount) : 0,
                    MaxAmount = payments.Any() ? payments.Max(p => p.Amount) : 0,
                    SuccessfulPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Success),
                    FailedPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Failed),
                    PendingPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Pending),
                    CanceledPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Canceled),
                    RefundedPayments = payments.Count(p => p.IsRefunded),
                    ExpiredPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Expired),
                    SuccessRate = payments.Any() ? (decimal)payments.Count(p => p.Status == OnlinePaymentStatus.Success) / payments.Count * 100 : 0,
                    RefundRate = payments.Any() ? (decimal)payments.Count(p => p.IsRefunded) / payments.Count * 100 : 0,
                    TotalGatewayFees = payments.Sum(p => p.GatewayFee ?? 0),
                    TotalNetAmount = payments.Sum(p => p.NetAmount ?? 0),
                    PaymentsByType = new Dictionary<OnlinePaymentType, int> { [paymentType] = payments.Count },
                    PaymentsByStatus = payments.GroupBy(p => p.Status).ToDictionary(g => g.Key, g => g.Count()),
                    PaymentsByGateway = payments.GroupBy(p => p.PaymentGatewayId)
                        .ToDictionary(g => g.Key, g => new GatewayPaymentInfo
                        {
                            GatewayId = g.Key,
                            GatewayName = g.First().PaymentGateway?.Name ?? "نامشخص",
                            GatewayType = g.First().PaymentGateway?.GatewayType ?? PaymentGatewayType.ZarinPal,
                            PaymentCount = g.Count(),
                            TotalAmount = g.Sum(p => p.Amount),
                            SuccessRate = g.Any() ? (decimal)g.Count(p => p.Status == OnlinePaymentStatus.Success) / g.Count() * 100 : 0,
                            AverageAmount = g.Any() ? g.Average(p => p.Amount) : 0,
                            TotalGatewayFees = g.Sum(p => p.GatewayFee ?? 0)
                        })
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار پرداخت‌های آنلاین بر اساس نوع پرداخت");
                throw;
            }
        }

        #endregion
    }
}
