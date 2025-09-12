using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Payment;
using ClinicApp.ViewModels.Payment.POS;
using CashSessionStatistics = ClinicApp.Interfaces.Payment.POS.CashSessionStatistics;

namespace ClinicApp.Services.Payment.POS
{
    /// <summary>
    /// Service برای مدیریت ترمینال‌های POS و جلسات نقدی
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت منطق کسب‌وکار POS
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل ترمینال‌های POS
    /// 2. مدیریت جلسات نقدی
    /// 3. محاسبه موجودی و تراز
    /// 4. گزارش‌گیری از تراکنش‌های POS
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public class PosManagementService : IPosManagementService
    {
        #region Fields

        private readonly IPosTerminalRepository _posTerminalRepository;
        private readonly ICashSessionRepository _cashSessionRepository;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly ILogger _logger;
        private IPosManagementService _posManagementServiceImplementation;

        #endregion

        #region Constructor

        public PosManagementService(
            IPosTerminalRepository posTerminalRepository,
            ICashSessionRepository cashSessionRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            ILogger logger)
        {
            _posTerminalRepository = posTerminalRepository ?? throw new ArgumentNullException(nameof(posTerminalRepository));
            _cashSessionRepository = cashSessionRepository ?? throw new ArgumentNullException(nameof(cashSessionRepository));
            _paymentTransactionRepository = paymentTransactionRepository ?? throw new ArgumentNullException(nameof(paymentTransactionRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region POS Terminal Management

        /// <summary>
        /// ایجاد ترمینال POS جدید
        /// </summary>
        public async Task<ServiceResult<PosTerminal>> CreatePosTerminalAsync(CreatePosTerminalRequest request)
        {
            try
            {
                _logger.Information("شروع ایجاد ترمینال POS جدید: {Name}", request.Name);

                // اعتبارسنجی درخواست
                var validationResult = await ValidateCreatePosTerminalRequestAsync(request);
                if (!validationResult.Success)
                {
                    _logger.Warning("اعتبارسنجی ایجاد ترمینال POS ناموفق: {Message}", validationResult.Message);
                    return ServiceResult<PosTerminal>.Failed(validationResult.Message);
                }

                // بررسی تکراری نبودن شماره سریال
                var existingTerminal = await _posTerminalRepository.GetBySerialNumberAsync(request.SerialNumber);
                if (existingTerminal != null)
                {
                    _logger.Warning("ترمینال POS با شماره سریال {SerialNumber} قبلاً وجود دارد", request.SerialNumber);
                    return ServiceResult<PosTerminal>.Failed("ترمینال POS با این شماره سریال قبلاً وجود دارد");
                }

                // اگر ترمینال پیش‌فرض است، سایر ترمینال‌ها را غیرپیش‌فرض کن
                if (request.IsDefault)
                {
                    await _posTerminalRepository.ClearDefaultTerminalsAsync(request.CreatedByUserId);
                }

                // ایجاد ترمینال POS
                var terminal = new PosTerminal
                {
                    Title = request.Name,
                    SerialNumber = request.SerialNumber,
                    Provider = request.ProviderType,
                    Protocol = request.Protocol,
                    IpAddress = request.ConnectionString?.Split(':')[0],
                    Port = request.ConnectionString?.Contains(':') == true ? 
                           int.TryParse(request.ConnectionString.Split(':')[1], out var port) ? port : null : null,
                    IsActive = true,
                    IsDefault = request.IsDefault,
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow
                };

                // ذخیره ترمینال
                var savedTerminal = await _posTerminalRepository.CreateAsync(terminal);
                if (savedTerminal == null)
                {
                    _logger.Error("خطا در ذخیره ترمینال POS");
                    return ServiceResult<PosTerminal>.Failed("خطا در ذخیره ترمینال POS");
                }

                _logger.Information("ترمینال POS با موفقیت ایجاد شد. شناسه: {TerminalId}", terminal.Id);
                return ServiceResult<PosTerminal>.Successful(terminal, "ترمینال POS با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد ترمینال POS: {Name}", request.Name);
                return ServiceResult<PosTerminal>.Failed("خطا در ایجاد ترمینال POS");
            }
        }

        /// <summary>
        /// به‌روزرسانی ترمینال POS
        /// </summary>
        public async Task<ServiceResult<PosTerminal>> UpdatePosTerminalAsync(UpdatePosTerminalRequest request)
        {
            try
            {
                _logger.Information("شروع به‌روزرسانی ترمینال POS: {TerminalId}", request.Id);

                // اعتبارسنجی درخواست
                var validationResult = await ValidateUpdatePosTerminalRequestAsync(request);
                if (!validationResult.Success)
                {
                    _logger.Warning("اعتبارسنجی به‌روزرسانی ترمینال POS ناموفق: {Message}", validationResult.Message);
                    return ServiceResult<PosTerminal>.Failed(validationResult.Message);
                }

                // دریافت ترمینال موجود
                var terminal = await _posTerminalRepository.GetByIdAsync(request.Id);
                if (terminal == null)
                {
                    _logger.Warning("ترمینال POS با شناسه {TerminalId} یافت نشد", request.Id);
                    return ServiceResult<PosTerminal>.Failed("ترمینال POS یافت نشد");
                }

                // بررسی تکراری نبودن شماره سریال (اگر تغییر کرده)
                if (terminal.SerialNumber != request.SerialNumber)
                {
                    var duplicateTerminal = await _posTerminalRepository.GetBySerialNumberAsync(request.SerialNumber);
                    if (duplicateTerminal != null)
                    {
                        _logger.Warning("ترمینال POS با شماره سریال {SerialNumber} قبلاً وجود دارد", request.SerialNumber);
                        return ServiceResult<PosTerminal>.Failed("ترمینال POS با این شماره سریال قبلاً وجود دارد");
                    }
                }

                // اگر ترمینال پیش‌فرض است، سایر ترمینال‌ها را غیرپیش‌فرض کن
                if (request.IsDefault && !terminal.IsDefault)
                {
                    await _posTerminalRepository.ClearDefaultTerminalsAsync(request.UpdatedByUserId);
                }

                // به‌روزرسانی اطلاعات ترمینال
                terminal.Title = request.Name; // Name is computed from Title
                terminal.SerialNumber = request.SerialNumber;
                terminal.Provider = request.ProviderType; // ProviderType is computed from Provider
                terminal.Protocol = request.Protocol;
                // Parse ConnectionString to IpAddress and Port
                if (!string.IsNullOrEmpty(request.ConnectionString) && request.ConnectionString.Contains(':'))
                {
                    var parts = request.ConnectionString.Split(':');
                    terminal.IpAddress = parts[0];
                    if (parts.Length > 1 && int.TryParse(parts[1], out var port))
                    {
                        terminal.Port = port;
                    }
                }
                // Description is computed from Title and Provider, no need to set it
                terminal.IsActive = request.IsActive;
                terminal.IsDefault = request.IsDefault;
                terminal.UpdatedByUserId = request.UpdatedByUserId;
                terminal.UpdatedAt = DateTime.UtcNow;

                // ذخیره تغییرات
                var updatedTerminal = await _posTerminalRepository.UpdateAsync(terminal);
                if (updatedTerminal == null)
                {
                    _logger.Error("خطا در به‌روزرسانی ترمینال POS");
                    return ServiceResult<PosTerminal>.Failed("خطا در به‌روزرسانی ترمینال POS");
                }

                _logger.Information("ترمینال POS با موفقیت به‌روزرسانی شد. شناسه: {TerminalId}", terminal.Id);
                return ServiceResult<PosTerminal>.Successful(terminal, "ترمینال POS با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی ترمینال POS: {TerminalId}", request.Id);
                return ServiceResult<PosTerminal>.Failed("خطا در به‌روزرسانی ترمینال POS");
            }
        }

        /// <summary>
        /// فعال/غیرفعال کردن ترمینال POS
        /// </summary>
        public async Task<ServiceResult> TogglePosTerminalStatusAsync(int terminalId, bool isActive, string userId)
        {
            try
            {
                _logger.Information("شروع تغییر وضعیت ترمینال POS: {TerminalId} به {Status}", terminalId, isActive ? "فعال" : "غیرفعال");

                // دریافت ترمینال
                var terminal = await _posTerminalRepository.GetByIdAsync(terminalId);
                if (terminal == null)
                {
                    _logger.Warning("ترمینال POS با شناسه {TerminalId} یافت نشد", terminalId);
                    return ServiceResult.Failed("ترمینال POS یافت نشد");
                }

                // اگر ترمینال پیش‌فرض است و می‌خواهیم آن را غیرفعال کنیم
                if (terminal.IsDefault && !isActive)
                {
                    _logger.Warning("نمی‌توان ترمینال پیش‌فرض را غیرفعال کرد");
                    return ServiceResult.Failed("نمی‌توان ترمینال پیش‌فرض را غیرفعال کرد");
                }

                // تغییر وضعیت
                terminal.IsActive = isActive;
                terminal.UpdatedByUserId = userId;
                terminal.UpdatedAt = DateTime.UtcNow;

                // ذخیره تغییرات
                var updatedTerminal = await _posTerminalRepository.UpdateAsync(terminal);
                if (updatedTerminal == null)
                {
                    _logger.Error("خطا در تغییر وضعیت ترمینال POS");
                    return ServiceResult.Failed("خطا در تغییر وضعیت ترمینال POS");
                }

                _logger.Information("وضعیت ترمینال POS با موفقیت تغییر کرد. شناسه: {TerminalId}", terminalId);
                return ServiceResult.Successful("وضعیت ترمینال POS با موفقیت تغییر کرد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تغییر وضعیت ترمینال POS: {TerminalId}", terminalId);
                return ServiceResult.Failed("خطا در تغییر وضعیت ترمینال POS");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// اعتبارسنجی درخواست ایجاد ترمینال POS
        /// </summary>
        private async Task<ServiceResult> ValidateCreatePosTerminalRequestAsync(CreatePosTerminalRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("درخواست ایجاد ترمینال نمی‌تواند خالی باشد");
                return ServiceResult.Failed("درخواست ایجاد ترمینال نامعتبر است", string.Join("; ", errors));
            }

            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add("نام ترمینال الزامی است");

            if (string.IsNullOrWhiteSpace(request.SerialNumber))
                errors.Add("شماره سریال الزامی است");

            if (string.IsNullOrWhiteSpace(request.CreatedByUserId))
                errors.Add("شناسه کاربر ایجادکننده الزامی است");

            if (errors.Any())
                return ServiceResult.Failed("اطلاعات وارد شده نامعتبر است", string.Join("; ", errors));

            return ServiceResult.Successful();
        }

        /// <summary>
        /// اعتبارسنجی درخواست به‌روزرسانی ترمینال POS
        /// </summary>
        private async Task<ServiceResult> ValidateUpdatePosTerminalRequestAsync(UpdatePosTerminalRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("درخواست به‌روزرسانی ترمینال نمی‌تواند خالی باشد");
                return ServiceResult.Failed("درخواست به‌روزرسانی ترمینال نامعتبر است", string.Join("; ", errors));
            }

            if (request.Id <= 0)
                errors.Add("شناسه ترمینال نامعتبر است");

            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add("نام ترمینال الزامی است");

            if (string.IsNullOrWhiteSpace(request.SerialNumber))
                errors.Add("شماره سریال الزامی است");

            if (string.IsNullOrWhiteSpace(request.UpdatedByUserId))
                errors.Add("شناسه کاربر به‌روزرسانی‌کننده الزامی است");

            if (errors.Any())
                return ServiceResult.Failed("اطلاعات وارد شده نامعتبر است", string.Join("; ", errors));

            return ServiceResult.Successful();
        }

        #endregion

        #region Placeholder Methods (To be implemented in next parts)

        public async Task<ServiceResult<PosTerminal>> GetPosTerminalAsync(int terminalId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetPosTerminalAsync will be implemented in next part");
        }

        public async Task<ServiceResult<IEnumerable<PosTerminal>>> GetActivePosTerminalsAsync()
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetActivePosTerminalsAsync will be implemented in next part");
        }

        public async Task<ServiceResult<PosTerminal>> GetDefaultPosTerminalAsync()
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetDefaultPosTerminalAsync will be implemented in next part");
        }

        public async Task<ServiceResult> SetDefaultPosTerminalAsync(int terminalId, string userId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("SetDefaultPosTerminalAsync will be implemented in next part");
        }

        public async Task<ServiceResult<CashSession>> StartCashSessionAsync(StartCashSessionRequest request)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("StartCashSessionAsync will be implemented in next part");
        }

        public Task<ServiceResult<CashSession>> StartCashSessionAsync(string userId, decimal initialAmount, string description)
        {
            return _posManagementServiceImplementation.StartCashSessionAsync(userId, initialAmount, description);
        }

        public async Task<ServiceResult<CashSession>> EndCashSessionAsync(int sessionId, EndCashSessionRequest request)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("EndCashSessionAsync will be implemented in next part");
        }

        public Task<ServiceResult<CashSession>> EndCashSessionAsync(int sessionId, decimal finalAmount, string description, string endedByUserId)
        {
            return _posManagementServiceImplementation.EndCashSessionAsync(sessionId, finalAmount, description, endedByUserId);
        }

        public async Task<ServiceResult<CashSession>> GetActiveCashSessionAsync(string userId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetActiveCashSessionAsync will be implemented in next part");
        }

        public async Task<ServiceResult<IEnumerable<CashSession>>> GetUserCashSessionsAsync(string userId, int pageNumber = 1, int pageSize = 50)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetUserCashSessionsAsync will be implemented in next part");
        }

        public async Task<ServiceResult<IEnumerable<CashSession>>> GetCashSessionsByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 50)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetCashSessionsByDateRangeAsync will be implemented in next part");
        }

        public async Task<ServiceResult<CashBalance>> CalculateCashBalanceAsync(int sessionId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("CalculateCashBalanceAsync will be implemented in next part");
        }

        public async Task<ServiceResult<CashBalance>> CalculateUserCashBalanceAsync(string userId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("CalculateUserCashBalanceAsync will be implemented in next part");
        }

        public async Task<ServiceResult<DailyCashBalance>> CalculateDailyCashBalanceAsync(DateTime date)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("CalculateDailyCashBalanceAsync will be implemented in next part");
        }

        public async Task<ServiceResult> AddCashBalanceAsync(int sessionId, decimal amount, string description, string userId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("AddCashBalanceAsync will be implemented in next part");
        }

        public async Task<ServiceResult> SubtractCashBalanceAsync(int sessionId, decimal amount, string description, string userId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("SubtractCashBalanceAsync will be implemented in next part");
        }

        public async Task<ServiceResult<PosTerminalStatistics>> GetPosTerminalStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetPosTerminalStatisticsAsync will be implemented in next part");
        }

        public async Task<ServiceResult<CashSessionStatistics>> GetCashSessionStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetCashSessionStatisticsAsync will be implemented in next part");
        }

        public async Task<ServiceResult<DailyPosStatistics>> GetDailyPosStatisticsAsync(DateTime date)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetDailyPosStatisticsAsync will be implemented in next part");
        }

        public async Task<ServiceResult> ValidatePosTerminalAsync(int terminalId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("ValidatePosTerminalAsync will be implemented in next part");
        }

        public async Task<ServiceResult> CanUsePosTerminalAsync(int terminalId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("CanUsePosTerminalAsync will be implemented in next part");
        }

        public async Task<ServiceResult> ValidateCashSessionAsync(int sessionId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("ValidateCashSessionAsync will be implemented in next part");
        }

        public Task<ServiceResult<IEnumerable<PosTerminal>>> GetTerminalsAsync(int pageNumber = 1, int pageSize = 50)
        {
            return _posManagementServiceImplementation.GetTerminalsAsync(pageNumber, pageSize);
        }

        public Task<ServiceResult<PosTerminal>> GetTerminalByIdAsync(int terminalId)
        {
            return _posManagementServiceImplementation.GetTerminalByIdAsync(terminalId);
        }

        public Task<ServiceResult<PosTerminal>> CreateTerminalAsync(PosTerminal terminal)
        {
            return _posManagementServiceImplementation.CreateTerminalAsync(terminal);
        }

        public Task<ServiceResult<PosTerminal>> UpdateTerminalAsync(PosTerminal terminal)
        {
            return _posManagementServiceImplementation.UpdateTerminalAsync(terminal);
        }

        public Task<ServiceResult> DeleteTerminalAsync(int terminalId, string userId)
        {
            return _posManagementServiceImplementation.DeleteTerminalAsync(terminalId, userId);
        }

        public Task<ServiceResult<CashSession>> GetSessionByIdAsync(int sessionId)
        {
            return _posManagementServiceImplementation.GetSessionByIdAsync(sessionId);
        }

        public Task<ServiceResult<IEnumerable<CashSession>>> GetActiveSessionsAsync()
        {
            return _posManagementServiceImplementation.GetActiveSessionsAsync();
        }

        public Task<ServiceResult<PosStatistics>> GetPosStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            return _posManagementServiceImplementation.GetPosStatisticsAsync(startDate, endDate);
        }

        public Task<ServiceResult<PosStatisticsViewModel>> GetPosStatisticsViewModelAsync(DateTime startDate, DateTime endDate)
        {
            return _posManagementServiceImplementation.GetPosStatisticsViewModelAsync(startDate, endDate);
        }

        #endregion
    }
}
