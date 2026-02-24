using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Interfaces.Payment.Management;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Admin.PaymentManagement;
using Serilog;

namespace ClinicApp.Repositories.Payment.Management
{
    /// <summary>
    /// Repository برای مدیریت پرداخت‌ها (Admin)
    /// طراحی شده طبق اصول SRP - مسئولیت: Data Access برای Payment Management
    /// </summary>
    public class PaymentManagementRepository : IPaymentManagementRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IOnlinePaymentRepository _onlinePaymentRepository;
        private readonly ILogger _logger;

        public PaymentManagementRepository(
            ApplicationDbContext context,
            IOnlinePaymentRepository onlinePaymentRepository,
            ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _onlinePaymentRepository = onlinePaymentRepository ?? throw new ArgumentNullException(nameof(onlinePaymentRepository));
            _logger = logger?.ForContext<PaymentManagementRepository>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// دریافت لیست پرداخت‌ها با فیلتر و Pagination
        /// </summary>
        public async Task<PagedResult<OnlinePayment>> GetPaymentsAsync(
            PaymentSearchFilter filter,
            int page,
            int pageSize)
        {
            try
            {
                _logger.Debug("دریافت لیست پرداخت‌ها - Page: {Page}, PageSize: {PageSize}", page, pageSize);

                var query = _context.OnlinePayments
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Patient)
                    .Include(op => op.Appointment)
                    .Include("Appointment.Doctor")
                    .Where(op => !op.IsDeleted)
                    .AsQueryable();

                // ✅ اعمال فیلترها
                query = ApplyFilters(query, filter);

                // ✅ شمارش کل
                var totalCount = await query.CountAsync();

                // ✅ Pagination
                var payments = await query
                    .OrderByDescending(op => op.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.Information("لیست پرداخت‌ها دریافت شد - Count: {Count}, Total: {Total}", payments.Count, totalCount);

                return new PagedResult<OnlinePayment>(payments, totalCount, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پرداخت‌ها");
                throw;
            }
        }

        /// <summary>
        /// دریافت جزئیات پرداخت
        /// </summary>
        public async Task<OnlinePayment> GetPaymentDetailsAsync(int onlinePaymentId)
        {
            try
            {
                _logger.Debug("دریافت جزئیات پرداخت - OnlinePaymentId: {OnlinePaymentId}", onlinePaymentId);

                var payment = await _context.OnlinePayments
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Patient)
                    .Include(op => op.Appointment)
                    .Include("Appointment.Doctor")
                    .Include("Appointment.Doctor.DoctorSpecializations")
                    .Include("Appointment.Doctor.DoctorSpecializations.Specialization")
                    .Include(op => op.Reception)
                    .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePaymentId && !op.IsDeleted);

                if (payment == null)
                {
                    _logger.Warning("پرداخت یافت نشد - OnlinePaymentId: {OnlinePaymentId}", onlinePaymentId);
                }

                return payment;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات پرداخت - OnlinePaymentId: {OnlinePaymentId}", onlinePaymentId);
                throw;
            }
        }

        /// <summary>
        /// دریافت آمار پرداخت‌ها
        /// </summary>
        public async Task<PaymentStatisticsViewModel> GetPaymentStatisticsAsync(
            PaymentSearchFilter filter)
        {
            try
            {
                _logger.Debug("دریافت آمار پرداخت‌ها");

                var query = _context.OnlinePayments
                    .Where(op => !op.IsDeleted)
                    .AsQueryable();

                // ✅ اعمال فیلترها
                query = ApplyFilters(query, filter);

                var payments = await query.ToListAsync();

                var statistics = new PaymentStatisticsViewModel
                {
                    TotalPayments = payments.Count,
                    SuccessfulPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Successful),
                    PendingPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Pending),
                    FailedPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Failed),
                    CanceledPayments = payments.Count(p => p.Status == OnlinePaymentStatus.Canceled),
                    TotalAmount = payments.Sum(p => p.Amount),
                    SuccessfulAmount = payments.Where(p => p.Status == OnlinePaymentStatus.Successful).Sum(p => p.Amount),
                    PendingAmount = payments.Where(p => p.Status == OnlinePaymentStatus.Pending).Sum(p => p.Amount),
                    FailedAmount = payments.Where(p => p.Status == OnlinePaymentStatus.Failed).Sum(p => p.Amount)
                };

                _logger.Information("آمار پرداخت‌ها دریافت شد - Total: {Total}, Successful: {Successful}",
                    statistics.TotalPayments, statistics.SuccessfulPayments);

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار پرداخت‌ها");
                throw;
            }
        }

        /// <summary>
        /// دریافت Timeline پرداخت
        /// </summary>
        public async Task<List<PaymentTimelineItemViewModel>> GetPaymentTimelineAsync(int onlinePaymentId)
        {
            try
            {
                _logger.Debug("دریافت Timeline پرداخت - OnlinePaymentId: {OnlinePaymentId}", onlinePaymentId);

                var payment = await GetPaymentDetailsAsync(onlinePaymentId);
                if (payment == null)
                {
                    return new List<PaymentTimelineItemViewModel>();
                }

                var timeline = new List<PaymentTimelineItemViewModel>();

                // ✅ ایجاد پرداخت
                timeline.Add(new PaymentTimelineItemViewModel
                {
                    Date = payment.CreatedAt,
                    DateDisplay = payment.CreatedAt.ToPersianDateTime(false),
                    Event = "ایجاد پرداخت",
                    Description = "پرداخت ایجاد شد",
                    UserName = payment.CreatedByUserId
                });

                // ✅ شروع پرداخت
                if (payment.PaymentStartDate.HasValue)
                {
                    timeline.Add(new PaymentTimelineItemViewModel
                    {
                        Date = payment.PaymentStartDate.Value,
                        DateDisplay = payment.PaymentStartDate.Value.ToPersianDateTime(false),
                        Event = "شروع پرداخت",
                        Description = "کاربر به درگاه پرداخت هدایت شد",
                        UserName = null
                    });
                }

                // ✅ تکمیل پرداخت
                if (payment.PaymentCompletionDate.HasValue)
                {
                    timeline.Add(new PaymentTimelineItemViewModel
                    {
                        Date = payment.PaymentCompletionDate.Value,
                        DateDisplay = payment.PaymentCompletionDate.Value.ToPersianDateTime(false),
                        Event = payment.Status == OnlinePaymentStatus.Successful ? "پرداخت موفق" : "پرداخت ناموفق",
                        Description = payment.Status == OnlinePaymentStatus.Successful
                            ? $"پرداخت با موفقیت انجام شد. شماره مرجع: {payment.GatewayReferenceCode}"
                            : payment.ErrorMessage ?? "پرداخت ناموفق بود",
                        UserName = payment.UpdatedByUserId
                    });
                }

                // ✅ به‌روزرسانی
                if (payment.UpdatedAt.HasValue && payment.UpdatedAt != payment.CreatedAt)
                {
                    timeline.Add(new PaymentTimelineItemViewModel
                    {
                        Date = payment.UpdatedAt.Value,
                        DateDisplay = payment.UpdatedAt.Value.ToPersianDateTime(false),
                        Event = "به‌روزرسانی",
                        Description = "وضعیت پرداخت به‌روزرسانی شد",
                        UserName = payment.UpdatedByUserId
                    });
                }

                timeline = timeline.OrderBy(t => t.Date).ToList();

                _logger.Information("Timeline پرداخت دریافت شد - Count: {Count}", timeline.Count);

                return timeline;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت Timeline پرداخت - OnlinePaymentId: {OnlinePaymentId}", onlinePaymentId);
                throw;
            }
        }

        /// <summary>
        /// تعداد اختلاف‌های مالی حل‌نشده (وضعیت Pending)
        /// </summary>
        public async Task<int> GetPendingDiscrepancyCountAsync()
        {
            try
            {
                return await _context.PaymentDiscrepancies
                    .CountAsync(pd => pd.Status == DiscrepancyStatus.Pending);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش اختلاف‌های حل‌نشده");
                return 0;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// اعمال فیلترها روی Query
        /// </summary>
        private IQueryable<OnlinePayment> ApplyFilters(IQueryable<OnlinePayment> query, PaymentSearchFilter filter)
        {
            if (filter == null)
                return query;

            // ✅ فیلتر وضعیت
            if (filter.Status.HasValue)
            {
                query = query.Where(op => op.Status == filter.Status.Value);
            }

            // ✅ فیلتر نوع پرداخت
            if (filter.PaymentType.HasValue)
            {
                query = query.Where(op => op.PaymentType == filter.PaymentType.Value);
            }

            // ✅ فیلتر بیمار
            if (filter.PatientId.HasValue)
            {
                query = query.Where(op => op.PatientId == filter.PatientId.Value);
            }

            // ✅ فیلتر پزشک (از طریق Appointment)
            if (filter.DoctorId.HasValue)
            {
                query = query.Where(op => op.Appointment != null && op.Appointment.DoctorId == filter.DoctorId.Value);
            }

            // ✅ فیلتر تاریخ شروع
            if (filter.StartDate.HasValue)
            {
                query = query.Where(op => op.CreatedAt >= filter.StartDate.Value);
            }

            // ✅ فیلتر تاریخ پایان
            if (filter.EndDate.HasValue)
            {
                query = query.Where(op => op.CreatedAt <= filter.EndDate.Value);
            }

            // ✅ فیلتر مبلغ حداقل
            if (filter.MinAmount.HasValue)
            {
                query = query.Where(op => op.Amount >= filter.MinAmount.Value);
            }

            // ✅ فیلتر مبلغ حداکثر
            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(op => op.Amount <= filter.MaxAmount.Value);
            }

            // ✅ فیلتر درگاه پرداخت
            if (filter.PaymentGatewayId.HasValue)
            {
                query = query.Where(op => op.PaymentGatewayId == filter.PaymentGatewayId.Value);
            }

            // ✅ جستجوی متنی
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim();
                query = query.Where(op =>
                    (op.PaymentToken != null && op.PaymentToken.Contains(searchTerm)) ||
                    (op.GatewayTransactionId != null && op.GatewayTransactionId.Contains(searchTerm)) ||
                    (op.GatewayReferenceCode != null && op.GatewayReferenceCode.Contains(searchTerm)) ||
                    (op.Patient != null && op.Patient.FullName != null && op.Patient.FullName.Contains(searchTerm)) ||
                    (op.Patient != null && op.Patient.NationalCode != null && op.Patient.NationalCode.Contains(searchTerm)) ||
                    (op.Appointment != null && op.Appointment.Doctor != null && op.Appointment.Doctor.FullName != null && op.Appointment.Doctor.FullName.Contains(searchTerm)));
            }

            return query;
        }

        #endregion
    }
}

