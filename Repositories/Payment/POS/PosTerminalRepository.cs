using ClinicApp.Interfaces.Payment.POS;
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

namespace ClinicApp.Repositories.Payment.POS
{
    /// <summary>
    /// پیاده‌سازی مخزن ترمینال‌های POS
    /// </summary>
    public class PosTerminalRepository : IPosTerminalRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public PosTerminalRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        #region CRUD Operations

        public async Task<PosTerminal> GetByIdAsync(int terminalId)
        {
            try
            {
                return await _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .FirstOrDefaultAsync(pt => pt.PosTerminalId == terminalId && !pt.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال POS. شناسه: {TerminalId}", terminalId);
                throw;
            }
        }

        public async Task<IEnumerable<PosTerminal>> GetAllAsync(int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                return await _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .Include(pt => pt.UpdatedByUser)
                    .Where(pt => !pt.IsDeleted)
                    .OrderByDescending(pt => pt.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست ترمینال‌های POS");
                throw;
            }
        }

        public async Task<PosTerminal> CreateAsync(PosTerminal terminal)
        {
            try
            {
                _context.PosTerminals.Add(terminal);
                await _context.SaveChangesAsync();
                return terminal;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد ترمینال POS");
                throw;
            }
        }

        public async Task<PosTerminal> AddAsync(PosTerminal terminal)
        {
            return await CreateAsync(terminal);
        }

        public async Task<PosTerminal> UpdateAsync(PosTerminal terminal)
        {
            try
            {
                _context.Entry(terminal).State = System.Data.Entity.EntityState.Modified;
                await _context.SaveChangesAsync();
                return terminal;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی ترمینال POS. شناسه: {TerminalId}", terminal.PosTerminalId);
                throw;
            }
        }

        public async Task<ServiceResult> SoftDeleteAsync(int terminalId, string deletedByUserId)
        {
            try
            {
                var terminal = await _context.PosTerminals
                    .FirstOrDefaultAsync(pt => pt.PosTerminalId == terminalId && !pt.IsDeleted);

                if (terminal == null)
                {
                    return ServiceResult.Failed("ترمینال POS یافت نشد");
                }

                terminal.IsDeleted = true;
                terminal.DeletedAt = DateTime.UtcNow;
                terminal.DeletedByUserId = deletedByUserId;

                await _context.SaveChangesAsync();
                return ServiceResult.Successful("ترمینال POS با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف ترمینال POS. شناسه: {TerminalId}", terminalId);
                return ServiceResult.Failed("خطا در حذف ترمینال POS");
            }
        }

        #endregion

        #region Query Operations

        public async Task<IEnumerable<PosTerminal>> GetActiveTerminalsAsync()
        {
            try
            {
                return await _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .Where(pt => !pt.IsDeleted && pt.IsActive)
                    .OrderBy(pt => pt.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال‌های فعال");
                throw;
            }
        }

        public async Task<PosTerminal> GetDefaultTerminalAsync()
        {
            try
            {
                return await _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .FirstOrDefaultAsync(pt => !pt.IsDeleted && pt.IsDefault);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال پیش‌فرض");
                throw;
            }
        }

        public async Task<IEnumerable<PosTerminal>> GetByProviderAsync(PosProviderType provider)
        {
            try
            {
                return await _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .Where(pt => !pt.IsDeleted && pt.Provider == provider)
                    .OrderBy(pt => pt.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال‌ها بر اساس ارائه‌دهنده. ارائه‌دهنده: {Provider}", provider);
                throw;
            }
        }

        public async Task<IEnumerable<PosTerminal>> GetByProtocolAsync(PosProtocol protocol)
        {
            try
            {
                return await _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .Where(pt => !pt.IsDeleted && pt.Protocol == protocol)
                    .OrderBy(pt => pt.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال‌ها بر اساس پروتکل. پروتکل: {Protocol}", protocol);
                throw;
            }
        }

        public async Task<PosTerminal> GetByTerminalIdAsync(string terminalId)
        {
            try
            {
                return await _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .FirstOrDefaultAsync(pt => !pt.IsDeleted && pt.TerminalId == terminalId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال بر اساس شماره ترمینال. شماره: {TerminalId}", terminalId);
                throw;
            }
        }

        public async Task<PosTerminal> GetBySerialNumberAsync(string serialNumber)
        {
            try
            {
                return await _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .FirstOrDefaultAsync(pt => !pt.IsDeleted && pt.SerialNumber == serialNumber);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال بر اساس شماره سریال. شماره: {SerialNumber}", serialNumber);
                throw;
            }
        }

        public async Task<PosTerminal> GetByMerchantIdAsync(string merchantId)
        {
            try
            {
                return await _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .FirstOrDefaultAsync(pt => !pt.IsDeleted && pt.MerchantId == merchantId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال بر اساس شماره پذیرنده. شماره: {MerchantId}", merchantId);
                throw;
            }
        }

        public async Task<PosTerminal> GetByIpAddressAsync(string ipAddress)
        {
            try
            {
                return await _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .FirstOrDefaultAsync(pt => !pt.IsDeleted && pt.IpAddress == ipAddress);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال بر اساس آدرس IP. آدرس: {IpAddress}", ipAddress);
                throw;
            }
        }

        public async Task<PosTerminal> GetByMacAddressAsync(string macAddress)
        {
            try
            {
                return await _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .FirstOrDefaultAsync(pt => !pt.IsDeleted && pt.MacAddress == macAddress);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال بر اساس آدرس MAC. آدرس: {MacAddress}", macAddress);
                throw;
            }
        }

        #endregion

        #region Search Operations

        public async Task<IEnumerable<PosTerminal>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .Where(pt => !pt.IsDeleted);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(pt => 
                        pt.Title.Contains(searchTerm) ||
                        pt.Description.Contains(searchTerm) ||
                        pt.TerminalId.Contains(searchTerm) ||
                        pt.MerchantId.Contains(searchTerm));
                }

                return await query
                    .OrderByDescending(pt => pt.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی ترمینال‌های POS. عبارت: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<IEnumerable<PosTerminal>> AdvancedSearchAsync(PosTerminalSearchFilters filters, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.PosTerminals
                    .Include(pt => pt.CreatedByUser)
                    .Where(pt => !pt.IsDeleted);

                if (filters != null)
                {
                    if (filters.Provider.HasValue)
                        query = query.Where(pt => pt.Provider == filters.Provider.Value);

                    if (filters.Protocol.HasValue)
                        query = query.Where(pt => pt.Protocol == filters.Protocol.Value);

                    if (filters.IsActive.HasValue)
                        query = query.Where(pt => pt.IsActive == filters.IsActive.Value);

                    if (!string.IsNullOrEmpty(filters.Title))
                        query = query.Where(pt => pt.Title.Contains(filters.Title));

                    if (!string.IsNullOrEmpty(filters.TerminalId))
                        query = query.Where(pt => pt.TerminalId.Contains(filters.TerminalId));

                    if (!string.IsNullOrEmpty(filters.MerchantId))
                        query = query.Where(pt => pt.MerchantId.Contains(filters.MerchantId));

                    if (!string.IsNullOrEmpty(filters.SerialNumber))
                        query = query.Where(pt => pt.SerialNumber.Contains(filters.SerialNumber));

                    if (!string.IsNullOrEmpty(filters.IpAddress))
                        query = query.Where(pt => pt.IpAddress.Contains(filters.IpAddress));

                    if (!string.IsNullOrEmpty(filters.MacAddress))
                        query = query.Where(pt => pt.MacAddress.Contains(filters.MacAddress));

                    if (filters.Port.HasValue)
                        query = query.Where(pt => pt.Port == filters.Port.Value);

                    if (filters.CreatedAfter.HasValue)
                        query = query.Where(pt => pt.CreatedAt >= filters.CreatedAfter.Value);

                    if (filters.CreatedBefore.HasValue)
                        query = query.Where(pt => pt.CreatedAt <= filters.CreatedBefore.Value);

                    if (!string.IsNullOrEmpty(filters.CreatedByUserId))
                        query = query.Where(pt => pt.CreatedByUserId == filters.CreatedByUserId);

                    if (filters.IsDeleted.HasValue)
                        query = query.Where(pt => pt.IsDeleted == filters.IsDeleted.Value);
                }

                return await query
                    .OrderByDescending(pt => pt.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پیشرفته ترمینال‌های POS");
                throw;
            }
        }

        #endregion

        #region Configuration Operations

        public async Task<ServiceResult> SetAsDefaultAsync(int terminalId, string updatedByUserId)
        {
            try
            {
                // پاک کردن ترمینال‌های پیش‌فرض قبلی
                var defaultTerminals = await _context.PosTerminals
                    .Where(pt => !pt.IsDeleted && pt.IsDefault)
                    .ToListAsync();

                foreach (var terminal in defaultTerminals)
                {
                    terminal.IsDefault = false;
                    terminal.UpdatedAt = DateTime.UtcNow;
                    terminal.UpdatedByUserId = updatedByUserId;
                }

                // تنظیم ترمینال جدید به عنوان پیش‌فرض
                var newDefaultTerminal = await _context.PosTerminals
                    .FirstOrDefaultAsync(pt => pt.PosTerminalId == terminalId && !pt.IsDeleted);

                if (newDefaultTerminal != null)
                {
                    newDefaultTerminal.IsDefault = true;
                    newDefaultTerminal.UpdatedAt = DateTime.UtcNow;
                    newDefaultTerminal.UpdatedByUserId = updatedByUserId;
                }

                await _context.SaveChangesAsync();
                return ServiceResult.Successful("ترمینال با موفقیت به عنوان پیش‌فرض تنظیم شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم ترمینال پیش‌فرض. شناسه: {TerminalId}", terminalId);
                return ServiceResult.Failed("خطا در تنظیم ترمینال پیش‌فرض");
            }
        }

        public async Task<ServiceResult> ClearDefaultTerminalsAsync(string updatedByUserId)
        {
            try
            {
                // تمام ترمینال‌ها را از حالت پیش‌فرض خارج می‌کنیم
                var allTerminals = await _context.PosTerminals
                    .Where(pt => !pt.IsDeleted && pt.IsDefault)
                    .ToListAsync();

                foreach (var terminal in allTerminals)
                {
                    terminal.IsDefault = false;
                    terminal.UpdatedAt = DateTime.UtcNow;
                    terminal.UpdatedByUserId = updatedByUserId;
                }

                await _context.SaveChangesAsync();
                return ServiceResult.Successful("وضعیت پیش‌فرض از تمام ترمینال‌ها پاک شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پاک کردن وضعیت پیش‌فرض ترمینال‌ها");
                return ServiceResult.Failed("خطا در پاک کردن وضعیت پیش‌فرض ترمینال‌ها");
            }
        }

        public async Task<ServiceResult> SetActiveStatusAsync(int terminalId, bool isActive, string updatedByUserId)
        {
            try
            {
                var terminal = await _context.PosTerminals
                    .FirstOrDefaultAsync(pt => pt.PosTerminalId == terminalId && !pt.IsDeleted);

                if (terminal != null)
                {
                    terminal.IsActive = isActive;
                    terminal.UpdatedAt = DateTime.UtcNow;
                    terminal.UpdatedByUserId = updatedByUserId;
                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Successful("وضعیت ترمینال با موفقیت تغییر کرد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تغییر وضعیت ترمینال. شناسه: {TerminalId}, وضعیت: {IsActive}", terminalId, isActive);
                return ServiceResult.Failed("خطا در تغییر وضعیت ترمینال");
            }
        }

        public async Task<ServiceResult> UpdateNetworkSettingsAsync(int terminalId, string ipAddress, string macAddress, int? port, string updatedByUserId)
        {
            try
            {
                var terminal = await _context.PosTerminals
                    .FirstOrDefaultAsync(pt => pt.PosTerminalId == terminalId && !pt.IsDeleted);

                if (terminal != null)
                {
                    terminal.IpAddress = ipAddress;
                    terminal.MacAddress = macAddress;
                    terminal.Port = port;
                    terminal.UpdatedAt = DateTime.UtcNow;
                    terminal.UpdatedByUserId = updatedByUserId;
                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Successful("تنظیمات شبکه ترمینال با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تنظیمات شبکه ترمینال. شناسه: {TerminalId}", terminalId);
                return ServiceResult.Failed("خطا در به‌روزرسانی تنظیمات شبکه ترمینال");
            }
        }

        public async Task<ServiceResult> UpdateProtocolSettingsAsync(int terminalId, PosProtocol protocol, int? port, string updatedByUserId)
        {
            try
            {
                var terminal = await _context.PosTerminals
                    .FirstOrDefaultAsync(pt => pt.PosTerminalId == terminalId && !pt.IsDeleted);

                if (terminal != null)
                {
                    terminal.Protocol = protocol;
                    terminal.Port = port;
                    terminal.UpdatedAt = DateTime.UtcNow;
                    terminal.UpdatedByUserId = updatedByUserId;
                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Successful("تنظیمات پروتکل ترمینال با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تنظیمات پروتکل ترمینال. شناسه: {TerminalId}", terminalId);
                return ServiceResult.Failed("خطا در به‌روزرسانی تنظیمات پروتکل ترمینال");
            }
        }

        #endregion

        #region Validation Operations

        public async Task<bool> ExistsAsync(int terminalId)
        {
            try
            {
                return await _context.PosTerminals
                    .AnyAsync(pt => pt.PosTerminalId == terminalId && !pt.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود ترمینال. شناسه: {TerminalId}", terminalId);
                throw;
            }
        }

        public async Task<bool> ExistsByTerminalIdAsync(string terminalId)
        {
            try
            {
                return await _context.PosTerminals
                    .AnyAsync(pt => !pt.IsDeleted && pt.TerminalId == terminalId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود ترمینال بر اساس شماره ترمینال. شماره: {TerminalId}", terminalId);
                throw;
            }
        }

        public async Task<bool> ExistsByMerchantIdAsync(string merchantId)
        {
            try
            {
                return await _context.PosTerminals
                    .AnyAsync(pt => !pt.IsDeleted && pt.MerchantId == merchantId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود ترمینال بر اساس شماره پذیرنده. شماره: {MerchantId}", merchantId);
                throw;
            }
        }

        public async Task<bool> IsTerminalIdUniqueAsync(string terminalId, int? excludeId = null)
        {
            try
            {
                var query = _context.PosTerminals
                    .Where(pt => !pt.IsDeleted && pt.TerminalId == terminalId);

                if (excludeId.HasValue)
                {
                    query = query.Where(pt => pt.PosTerminalId != excludeId.Value);
                }

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی یکتایی شماره ترمینال. شماره: {TerminalId}", terminalId);
                throw;
            }
        }

        public async Task<bool> IsMerchantIdUniqueAsync(string merchantId, int? excludeId = null)
        {
            try
            {
                var query = _context.PosTerminals
                    .Where(pt => !pt.IsDeleted && pt.MerchantId == merchantId);

                if (excludeId.HasValue)
                {
                    query = query.Where(pt => pt.PosTerminalId != excludeId.Value);
                }

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی یکتایی شماره پذیرنده. شماره: {MerchantId}", merchantId);
                throw;
            }
        }

        public async Task<bool> IsIpAddressUniqueAsync(string ipAddress, int? excludeId = null)
        {
            try
            {
                var query = _context.PosTerminals
                    .Where(pt => !pt.IsDeleted && pt.IpAddress == ipAddress);

                if (excludeId.HasValue)
                {
                    query = query.Where(pt => pt.PosTerminalId != excludeId.Value);
                }

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی یکتایی آدرس IP. آدرس: {IpAddress}", ipAddress);
                throw;
            }
        }

        public async Task<bool> IsMacAddressUniqueAsync(string macAddress, int? excludeId = null)
        {
            try
            {
                var query = _context.PosTerminals
                    .Where(pt => !pt.IsDeleted && pt.MacAddress == macAddress);

                if (excludeId.HasValue)
                {
                    query = query.Where(pt => pt.PosTerminalId != excludeId.Value);
                }

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی یکتایی آدرس MAC. آدرس: {MacAddress}", macAddress);
                throw;
            }
        }

        public async Task<int> GetCountAsync(PosTerminalSearchFilters filters = null)
        {
            try
            {
                var query = _context.PosTerminals.Where(pt => !pt.IsDeleted);

                if (filters != null)
                {
                    if (filters.Provider.HasValue)
                        query = query.Where(pt => pt.Provider == filters.Provider.Value);

                    if (filters.Protocol.HasValue)
                        query = query.Where(pt => pt.Protocol == filters.Protocol.Value);

                    if (filters.IsActive.HasValue)
                        query = query.Where(pt => pt.IsActive == filters.IsActive.Value);

                    if (!string.IsNullOrEmpty(filters.Title))
                        query = query.Where(pt => pt.Title.Contains(filters.Title));

                    if (!string.IsNullOrEmpty(filters.TerminalId))
                        query = query.Where(pt => pt.TerminalId.Contains(filters.TerminalId));

                    if (!string.IsNullOrEmpty(filters.MerchantId))
                        query = query.Where(pt => pt.MerchantId.Contains(filters.MerchantId));

                    if (!string.IsNullOrEmpty(filters.SerialNumber))
                        query = query.Where(pt => pt.SerialNumber.Contains(filters.SerialNumber));

                    if (!string.IsNullOrEmpty(filters.IpAddress))
                        query = query.Where(pt => pt.IpAddress.Contains(filters.IpAddress));

                    if (!string.IsNullOrEmpty(filters.MacAddress))
                        query = query.Where(pt => pt.MacAddress.Contains(filters.MacAddress));

                    if (filters.Port.HasValue)
                        query = query.Where(pt => pt.Port == filters.Port.Value);

                    if (filters.CreatedAfter.HasValue)
                        query = query.Where(pt => pt.CreatedAt >= filters.CreatedAfter.Value);

                    if (filters.CreatedBefore.HasValue)
                        query = query.Where(pt => pt.CreatedAt <= filters.CreatedBefore.Value);

                    if (!string.IsNullOrEmpty(filters.CreatedByUserId))
                        query = query.Where(pt => pt.CreatedByUserId == filters.CreatedByUserId);

                    if (filters.IsDeleted.HasValue)
                        query = query.Where(pt => pt.IsDeleted == filters.IsDeleted.Value);
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش ترمینال‌های POS");
                throw;
            }
        }

        #endregion

        #region Statistics Operations

        public async Task<PosTerminalStatistics> GetStatisticsAsync()
        {
            try
            {
                var totalTerminals = await _context.PosTerminals.CountAsync(pt => !pt.IsDeleted);
                var activeTerminals = await _context.PosTerminals.CountAsync(pt => !pt.IsDeleted && pt.IsActive);
                var defaultTerminals = await _context.PosTerminals.CountAsync(pt => !pt.IsDeleted && pt.IsDefault);

                return new PosTerminalStatistics
                {
                    TotalTerminals = totalTerminals,
                    ActiveTerminals = activeTerminals,
                    InactiveTerminals = totalTerminals - activeTerminals,
                    DefaultTerminals = defaultTerminals
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار ترمینال‌های POS");
                throw;
            }
        }

        public async Task<PosTerminalUsageStatistics> GetUsageStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var terminals = await _context.PosTerminals
                    .Where(pt => !pt.IsDeleted)
                    .ToListAsync();

                var terminalUsage = new Dictionary<int, PosTerminalUsageInfo>();

                foreach (var terminal in terminals)
                {
                    var transactions = await _context.PaymentTransactions
                        .Where(pt => pt.PosTerminalId == terminal.PosTerminalId &&
                                   pt.CreatedAt >= startDate &&
                                   pt.CreatedAt <= endDate)
                        .ToListAsync();

                    var successfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Successful);
                    var totalAmount = transactions.Where(t => t.Status == PaymentStatus.Successful).Sum(t => t.Amount);

                    terminalUsage[terminal.PosTerminalId] = new PosTerminalUsageInfo
                    {
                        TerminalId = terminal.PosTerminalId,
                        TerminalTitle = terminal.Title,
                        Provider = terminal.Provider,
                        TransactionCount = transactions.Count,
                        TotalAmount = totalAmount,
                        SuccessRate = transactions.Count > 0 ? (decimal)successfulTransactions / transactions.Count * 100 : 0,
                        AverageAmount = successfulTransactions > 0 ? totalAmount / successfulTransactions : 0,
                        SuccessfulTransactions = successfulTransactions,
                        FailedTransactions = transactions.Count - successfulTransactions,
                        LastTransactionDate = transactions.OrderByDescending(t => t.CreatedAt).FirstOrDefault()?.CreatedAt
                    };
                }

                return new PosTerminalUsageStatistics
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalTransactions = terminalUsage.Values.Sum(t => t.TransactionCount),
                    TotalAmount = terminalUsage.Values.Sum(t => t.TotalAmount),
                    TerminalUsage = terminalUsage
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار استفاده از ترمینال‌های POS");
                throw;
            }
        }

        public async Task<PosTerminalUsageInfo> GetTerminalUsageAsync(int terminalId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var terminal = await _context.PosTerminals
                    .FirstOrDefaultAsync(pt => pt.PosTerminalId == terminalId && !pt.IsDeleted);

                if (terminal == null)
                    return null;

                var transactions = await _context.PaymentTransactions
                    .Where(pt => pt.PosTerminalId == terminalId &&
                               pt.CreatedAt >= startDate &&
                               pt.CreatedAt <= endDate)
                    .ToListAsync();

                var successfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Successful);
                var totalAmount = transactions.Where(t => t.Status == PaymentStatus.Successful).Sum(t => t.Amount);

                return new PosTerminalUsageInfo
                {
                    TerminalId = terminal.PosTerminalId,
                    TerminalTitle = terminal.Title,
                    Provider = terminal.Provider,
                    TransactionCount = transactions.Count,
                    TotalAmount = totalAmount,
                    SuccessRate = transactions.Count > 0 ? (decimal)successfulTransactions / transactions.Count * 100 : 0,
                    AverageAmount = successfulTransactions > 0 ? totalAmount / successfulTransactions : 0,
                    SuccessfulTransactions = successfulTransactions,
                    FailedTransactions = transactions.Count - successfulTransactions,
                    LastTransactionDate = transactions.OrderByDescending(t => t.CreatedAt).FirstOrDefault()?.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار استفاده از ترمینال. شناسه: {TerminalId}", terminalId);
                throw;
            }
        }

        #endregion
    }
}