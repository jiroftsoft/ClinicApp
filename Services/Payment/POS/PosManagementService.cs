using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Payment.POS;
using ClinicApp.ViewModels.Reception;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;
using ClinicApp.Interfaces.Payment;

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
        private readonly ICashSessionAuditService _cashSessionAuditService;
        private readonly ILogger _logger;
        private IPosManagementService _posManagementServiceImplementation;

        #endregion

        #region Constructor

        public PosManagementService(
            IPosTerminalRepository posTerminalRepository,
            ICashSessionRepository cashSessionRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            ICashSessionAuditService cashSessionAuditService,
            ILogger logger)
        {
            _posTerminalRepository = posTerminalRepository ?? throw new ArgumentNullException(nameof(posTerminalRepository));
            _cashSessionRepository = cashSessionRepository ?? throw new ArgumentNullException(nameof(cashSessionRepository));
            _paymentTransactionRepository = paymentTransactionRepository ?? throw new ArgumentNullException(nameof(paymentTransactionRepository));
            _cashSessionAuditService = cashSessionAuditService ?? throw new ArgumentNullException(nameof(cashSessionAuditService));
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
                    TerminalId = !string.IsNullOrWhiteSpace(request.TerminalId) ? request.TerminalId : request.SerialNumber, // استفاده از TerminalId یا SerialNumber به عنوان fallback
                    MerchantId = !string.IsNullOrWhiteSpace(request.MerchantId) ? request.MerchantId : "DEFAULT", // استفاده از MerchantId یا مقدار پیش‌فرض
                    SerialNumber = request.SerialNumber,
                    Provider = request.ProviderType,
                    Protocol = request.Protocol,
                    IpAddress = !string.IsNullOrWhiteSpace(request.IpAddress) ? request.IpAddress : ParseConnectionStringIp(request.ConnectionString),
                    Port = request.Port ?? ParseConnectionStringPort(request.ConnectionString),
                    MacAddress = string.IsNullOrWhiteSpace(request.MacAddress) ? null : request.MacAddress,
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
                _logger.Information("شروع به‌روزرسانی ترمینال POS: {TerminalId}, Name: {Name}, IP: {IpAddress}, Port: {Port}, MacAddress: {MacAddress}", 
                    request.Id, request.Name, request.IpAddress, request.Port, request.MacAddress);

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
                terminal.Title = !string.IsNullOrEmpty(request.Title) ? request.Title : request.Name;
                terminal.SerialNumber = request.SerialNumber;
                terminal.Provider = request.Provider != default(PosProviderType) ? request.Provider : request.ProviderType;
                terminal.Protocol = request.Protocol;
                
                // به‌روزرسانی TerminalId و MerchantId
                terminal.TerminalId = !string.IsNullOrWhiteSpace(request.TerminalId) ? request.TerminalId : terminal.TerminalId;
                terminal.MerchantId = !string.IsNullOrWhiteSpace(request.MerchantId) ? request.MerchantId : terminal.MerchantId;
                
                // به‌روزرسانی اطلاعات شبکه
                if (!string.IsNullOrEmpty(request.IpAddress))
                {
                    terminal.IpAddress = request.IpAddress;
                }
                else if (!string.IsNullOrEmpty(request.ConnectionString))
                {
                    // Fallback: اگر IpAddress نبود، از ConnectionString استفاده کن
                    terminal.IpAddress = ParseConnectionStringIp(request.ConnectionString);
                }
                
                // به‌روزرسانی Port: 
                // اگر request.Port.HasValue باشد، مقدار آن را تنظیم می‌کنیم
                // اگر request.Port null باشد، Port را null می‌کنیم (حتی اگر ConnectionString وجود داشته باشد)
                // این به کاربر اجازه می‌دهد که پورت را به null تغییر دهد
                if (request.Port.HasValue)
                {
                    terminal.Port = request.Port.Value;
                    _logger.Debug("Port به‌روزرسانی شد به: {Port}", request.Port.Value);
                }
                else
                {
                    // اگر Port null باشد، آن را null می‌کنیم (حتی اگر ConnectionString وجود داشته باشد)
                    // چون کاربر صراحتاً می‌خواهد پورت را null کند
                    terminal.Port = null;
                    _logger.Debug("Port به null تنظیم شد (کاربر می‌خواهد پورت را حذف کند)");
                }
                
                // MacAddress: اگر null یا empty باشد، null ست می‌شود
                terminal.MacAddress = string.IsNullOrWhiteSpace(request.MacAddress) ? null : request.MacAddress;
                
                _logger.Debug("به‌روزرسانی فیلدهای ترمینال. Title: {Title}, IP: {IpAddress}, Port: {Port}, MacAddress: {MacAddress}", 
                    terminal.Title, terminal.IpAddress, terminal.Port, terminal.MacAddress ?? "null");
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
            catch (DbUpdateException dbEx)
            {
                _logger.Error(dbEx, "خطای دیتابیس در به‌روزرسانی ترمینال POS: {TerminalId}. InnerException: {InnerException}", 
                    request.Id, dbEx.InnerException?.Message);
                
                // بررسی نوع خطای دیتابیس
                if (dbEx.InnerException != null && !string.IsNullOrEmpty(dbEx.InnerException.Message))
                {
                    var innerMsg = dbEx.InnerException.Message.ToLower();
                    if (innerMsg.Contains("constraint") || innerMsg.Contains("foreign key"))
                    {
                        return ServiceResult<PosTerminal>.Failed("خطا در به‌روزرسانی: محدودیت دیتابیس نقض شده است");
                    }
                    if (innerMsg.Contains("unique") || innerMsg.Contains("duplicate"))
                    {
                        return ServiceResult<PosTerminal>.Failed("خطا در به‌روزرسانی: شماره سریال یا شناسه تکراری است");
                    }
                }
                
                return ServiceResult<PosTerminal>.Failed("خطا در به‌روزرسانی ترمینال POS در دیتابیس");
            }
            catch (InvalidOperationException ioEx)
            {
                _logger.Error(ioEx, "خطای عملیاتی در به‌روزرسانی ترمینال POS: {TerminalId}", request.Id);
                return ServiceResult<PosTerminal>.Failed(ioEx.Message);
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

                // Fallback برای userId
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("UserId null یا empty است. استفاده از fallback");
                    userId = SystemUsers.SystemUserId ?? SystemUsers.AdminUserId ?? "00000000-0000-0000-0000-000000000000";
                    _logger.Information("استفاده از UserId fallback: {UserId}", userId);
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
            catch (DbUpdateException dbEx)
            {
                _logger.Error(dbEx, "خطای دیتابیس در تغییر وضعیت ترمینال POS: {TerminalId}. InnerException: {InnerException}", 
                    terminalId, dbEx.InnerException?.Message);
                return ServiceResult.Failed("خطا در تغییر وضعیت ترمینال POS در دیتابیس");
            }
            catch (InvalidOperationException ioEx)
            {
                _logger.Error(ioEx, "خطای عملیاتی در تغییر وضعیت ترمینال POS: {TerminalId}", terminalId);
                return ServiceResult.Failed(ioEx.Message);
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

            if (string.IsNullOrWhiteSpace(request.TerminalId))
                errors.Add("شماره ترمینال الزامی است");

            if (string.IsNullOrWhiteSpace(request.MerchantId))
                errors.Add("شماره پذیرنده الزامی است");

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

            if (string.IsNullOrWhiteSpace(request.TerminalId))
                errors.Add("شماره ترمینال الزامی است");

            if (string.IsNullOrWhiteSpace(request.MerchantId))
                errors.Add("شماره پذیرنده الزامی است");

            if (string.IsNullOrWhiteSpace(request.UpdatedByUserId))
                errors.Add("شناسه کاربر به‌روزرسانی‌کننده الزامی است");

            if (errors.Any())
                return ServiceResult.Failed("اطلاعات وارد شده نامعتبر است", string.Join("; ", errors));

            return ServiceResult.Successful();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// استخراج IP از ConnectionString
        /// </summary>
        private string ParseConnectionStringIp(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return null;

            // فرمت: IP:192.168.1.10,Port:5000 یا 192.168.1.10:5000
            if (connectionString.Contains("IP:"))
            {
                var ipPart = connectionString.Split(',')[0];
                return ipPart.Replace("IP:", "").Trim();
            }
            else if (connectionString.Contains(":"))
            {
                var parts = connectionString.Split(':');
                return parts[0].Trim();
            }

            return connectionString.Trim();
        }

        /// <summary>
        /// استخراج Port از ConnectionString
        /// </summary>
        private int? ParseConnectionStringPort(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return null;

            // فرمت: IP:192.168.1.10,Port:5000 یا 192.168.1.10:5000
            if (connectionString.Contains("Port:"))
            {
                var portPart = connectionString.Split(',')
                    .FirstOrDefault(p => p.Contains("Port:"));
                if (portPart != null)
                {
                    var portStr = portPart.Replace("Port:", "").Trim();
                    if (int.TryParse(portStr, out int port))
                        return port;
                }
            }
            else if (connectionString.Contains(":"))
            {
                var parts = connectionString.Split(':');
                if (parts.Length >= 2)
                {
                    var portStr = parts[1].Split(',')[0].Trim();
                    if (int.TryParse(portStr, out int port))
                        return port;
                }
            }

            return null;
        }

        #endregion

        #region Placeholder Methods (To be implemented in next parts)

        public async Task<ServiceResult<PosTerminal>> GetPosTerminalAsync(int terminalId)
        {
            try
            {
                var terminal = await _posTerminalRepository.GetByIdAsync(terminalId);
                if (terminal == null)
                {
                    return ServiceResult<PosTerminal>.Failed("ترمینال POS یافت نشد");
                }
                
                return ServiceResult<PosTerminal>.Successful(terminal);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال POS. شناسه: {TerminalId}", terminalId);
                return ServiceResult<PosTerminal>.Failed("خطا در دریافت ترمینال POS");
            }
        }

        public async Task<ServiceResult<IEnumerable<PosTerminal>>> GetActivePosTerminalsAsync()
        {
            try
            {
                var terminals = await _posTerminalRepository.GetActiveTerminalsAsync();
                return ServiceResult<IEnumerable<PosTerminal>>.Successful(terminals);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال‌های فعال POS");
                return ServiceResult<IEnumerable<PosTerminal>>.Failed("خطا در دریافت ترمینال‌های فعال");
            }
        }

        public async Task<ServiceResult<PosTerminal>> GetDefaultPosTerminalAsync()
        {
            try
            {
                var terminal = await _posTerminalRepository.GetDefaultTerminalAsync();
                if (terminal == null)
                {
                    // اگر ترمینال پیش‌فرض وجود نداشت، اولین ترمینال فعال را برگردان
                    var activeTerminals = await _posTerminalRepository.GetActiveTerminalsAsync();
                    terminal = activeTerminals.FirstOrDefault();
                    
                    if (terminal == null)
                    {
                        return ServiceResult<PosTerminal>.Failed("هیچ ترمینال POS فعالی یافت نشد. لطفاً ابتدا ترمینال را تنظیم کنید.");
                    }
                }
                
                return ServiceResult<PosTerminal>.Successful(terminal);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ترمینال پیش‌فرض POS");
                return ServiceResult<PosTerminal>.Failed("خطا در دریافت ترمینال پیش‌فرض");
            }
        }

        public async Task<ServiceResult> SetDefaultPosTerminalAsync(int terminalId, string userId)
        {
            try
            {
                // بررسی وجود ترمینال
                var terminal = await _posTerminalRepository.GetByIdAsync(terminalId);
                if (terminal == null)
                {
                    return ServiceResult.Failed("ترمینال POS یافت نشد");
                }

                // بررسی فعال بودن ترمینال
                if (!terminal.IsActive)
                {
                    return ServiceResult.Failed("فقط ترمینال‌های فعال می‌توانند به عنوان پیش‌فرض تنظیم شوند");
                }

                // تنظیم ترمینال به عنوان پیش‌فرض
                var result = await _posTerminalRepository.SetAsDefaultAsync(terminalId, userId);
                if (!result.Success)
                {
                    return result;
                }

                _logger.Information("ترمینال POS با شناسه {TerminalId} به عنوان پیش‌فرض تنظیم شد. کاربر: {UserId}", terminalId, userId);
                return ServiceResult.Successful("ترمینال با موفقیت به عنوان پیش‌فرض تنظیم شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم ترمینال پیش‌فرض. شناسه: {TerminalId}", terminalId);
                return ServiceResult.Failed("خطا در تنظیم ترمینال پیش‌فرض");
            }
        }

        public async Task<ServiceResult<CashSession>> StartCashSessionAsync(StartCashSessionRequest request)
        {
            // FIXME(Phase 2): Implement full StartCashSessionAsync with request object
            _logger.Warning("⚠️ POS MANAGEMENT: StartCashSessionAsync(StartCashSessionRequest) not implemented yet");
            return await Task.FromResult(ServiceResult<CashSession>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        /// <summary>
        /// شروع جلسه نقدی جدید (ساده)
        /// ✅ پیاده‌سازی واقعی برای Production
        /// </summary>
        public async Task<ServiceResult<CashSession>> StartCashSessionAsync(string userId, decimal initialAmount, string description)
        {
            try
            {
                _logger.Information("🏦 Starting cash session - UserId: {UserId}, InitialAmount: {InitialAmount}", userId, initialAmount);
                
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("⚠️ UserId is null or empty");
                    return ServiceResult<CashSession>.Failed("شناسه کاربر نامعتبر است.");
                }
                
                if (initialAmount < 0)
                {
                    _logger.Warning("⚠️ InitialAmount is negative: {InitialAmount}", initialAmount);
                    return ServiceResult<CashSession>.Failed("مبلغ اولیه نمی‌تواند منفی باشد.");
                }
                
                // ✅ بررسی اینکه آیا جلسه فعالی وجود دارد یا نه
                var hasActiveSession = await _cashSessionRepository.HasActiveSessionAsync(userId);
                if (hasActiveSession)
                {
                    _logger.Warning("⚠️ User already has an active session - UserId: {UserId}", userId);
                    return ServiceResult<CashSession>.Failed(
                        "شما در حال حاضر یک جلسه صندوق باز دارید. لطفاً ابتدا جلسه قبلی را ببندید.",
                        "ACTIVE_SESSION_EXISTS");
                }
                
                // ✅ ایجاد جلسه جدید
                var newSession = new CashSession
                {
                    UserId = userId,
                    Status = CashSessionStatus.Active, // یا Open (هر دو = 1)
                    OpenedAt = DateTime.Now,
                    OpeningBalance = initialAmount,
                    CashBalance = initialAmount, // مانده اولیه = مانده فعلی
                    PosBalance = 0, // مانده POS در ابتدا صفر است
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = userId,
                    UpdatedAt = DateTime.Now,
                    UpdatedByUserId = userId,
                    IsDeleted = false
                };
                
                // ✅ تولید شماره جلسه (اگر SessionNumber property وجود دارد)
                // در صورت نیاز می‌توان از یک Service برای تولید شماره استفاده کرد
                // فعلاً از CashSessionId استفاده می‌شود که بعد از ذخیره تولید می‌شود
                
                _logger.Information("💾 Saving new cash session to database - UserId: {UserId}, InitialAmount: {InitialAmount}",
                    userId, initialAmount);
                
                var savedSession = await _cashSessionRepository.AddAsync(newSession);
                
                if (savedSession == null)
                {
                    _logger.Error("❌ Failed to save cash session - UserId: {UserId}", userId);
                    return ServiceResult<CashSession>.Failed("خطا در ذخیره جلسه صندوق در دیتابیس.");
                }
                
                _logger.Information("✅ Cash session started successfully - SessionId: {SessionId}, UserId: {UserId}, InitialAmount: {InitialAmount}",
                    savedSession.CashSessionId, userId, initialAmount);
                _logger.Information("🏦 AUDIT StartSession | SessionId: {SessionId}, UserId: {UserId}, InitialAmount: {InitialAmount}, OpenedAt: {OpenedAt}",
                    savedSession.CashSessionId, userId, initialAmount, savedSession.OpenedAt);

                // ثبت در CashSessionAuditLogs برای ردیابی و دسترسی منشی/ادمین (PerformedByUserId از ICurrentUserService در AuditService پر می‌شود)
                var auditOpen = await _cashSessionAuditService.LogActionAsync(
                    savedSession.CashSessionId,
                    "Open",
                    null,
                    new { OpeningBalance = initialAmount, UserId = userId, OpenedAt = savedSession.OpenedAt, SessionNumber = savedSession.SessionNumber },
                    description ?? "شروع جلسه صندوق");
                if (!auditOpen.Success)
                    _logger.Warning("⚠️ CashSessionAuditLog Open ثبت نشد: {Message}", auditOpen.Message);
                
                return ServiceResult<CashSession>.Successful(savedSession, "جلسه صندوق با موفقیت شروع شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error starting cash session - UserId: {UserId}, InitialAmount: {InitialAmount}",
                    userId, initialAmount);
                return ServiceResult<CashSession>.Failed("خطا در شروع جلسه صندوق: " + ex.Message);
            }
        }

        public async Task<ServiceResult<CashSession>> EndCashSessionAsync(int sessionId, EndCashSessionRequest request)
        {
            // FIXME(Phase 2): Implement full EndCashSessionAsync with request object
            _logger.Warning("⚠️ POS MANAGEMENT: EndCashSessionAsync(EndCashSessionRequest) not implemented yet");
            return await Task.FromResult(ServiceResult<CashSession>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        /// <summary>
        /// پایان جلسه نقدی (ساده)
        /// ✅ پیاده‌سازی واقعی برای Production
        /// </summary>
        public async Task<ServiceResult<CashSession>> EndCashSessionAsync(int sessionId, decimal finalAmount, string description, string endedByUserId)
        {
            try
            {
                _logger.Information("🏦 Ending cash session - SessionId: {SessionId}, FinalAmount: {FinalAmount}, EndedByUserId: {EndedByUserId}",
                    sessionId, finalAmount, endedByUserId);
                
                if (sessionId <= 0)
                {
                    _logger.Warning("⚠️ SessionId is invalid: {SessionId}", sessionId);
                    return ServiceResult<CashSession>.Failed("شناسه جلسه نامعتبر است.");
                }
                
                if (string.IsNullOrWhiteSpace(endedByUserId))
                {
                    _logger.Warning("⚠️ EndedByUserId is null or empty");
                    return ServiceResult<CashSession>.Failed("شناسه کاربر پایان‌دهنده نامعتبر است.");
                }
                
                if (finalAmount < 0)
                {
                    _logger.Warning("⚠️ FinalAmount is negative: {FinalAmount}", finalAmount);
                    return ServiceResult<CashSession>.Failed("مبلغ نهایی نمی‌تواند منفی باشد.");
                }
                
                // ✅ دریافت جلسه از دیتابیس
                var session = await _cashSessionRepository.GetByIdAsync(sessionId);
                if (session == null)
                {
                    _logger.Warning("⚠️ Cash session not found - SessionId: {SessionId}", sessionId);
                    return ServiceResult<CashSession>.Failed("جلسه صندوق یافت نشد.");
                }
                
                // ✅ بررسی اینکه جلسه قبلاً بسته نشده باشد
                if (session.Status == CashSessionStatus.Closed || session.ClosedAt.HasValue)
                {
                    _logger.Warning("⚠️ Cash session already closed - SessionId: {SessionId}, Status: {Status}",
                        sessionId, session.Status);
                    return ServiceResult<CashSession>.Failed("این جلسه قبلاً بسته شده است.");
                }
                
                // ✅ Audit: مقادیر قبل از تغییر (برای ردیابی مالی)
                var oldStatus = session.Status;
                var oldCashBalance = session.CashBalance;
                var expectedBalance = session.OpeningBalance + session.CashBalance + session.PosBalance - 0; // TotalExpense=0 در entity

                _logger.Information("💾 Closing cash session (conditional update) - SessionId: {SessionId}, FinalAmount: {FinalAmount}, ExpectedBalance: {ExpectedBalance}, Difference: {Difference}",
                    sessionId, finalAmount, expectedBalance, finalAmount - expectedBalance);
                _logger.Information("🏦 AUDIT EndSession | SessionId: {SessionId}, EndedBy: {EndedBy}, OldStatus: {OldStatus}, OldCashBalance: {OldCashBalance}, NewCashBalance: {NewCashBalance}, Difference: {Difference}",
                    sessionId, endedByUserId, oldStatus, oldCashBalance, finalAmount, finalAmount - expectedBalance);

                // ✅ بستن با UPDATE شرطی در تراکنش — فقط یک درخواست موفق می‌شود (جلوگیری از race)
                var updatedSession = await _cashSessionRepository.TryCloseSessionConditionalAsync(
                    sessionId, DateTime.Now, finalAmount, endedByUserId);

                if (updatedSession == null)
                {
                    _logger.Warning("⚠️ Cash session was already closed (race or duplicate) - SessionId: {SessionId}", sessionId);
                    return ServiceResult<CashSession>.Failed("این جلسه قبلاً بسته شده است.");
                }

                // ثبت در CashSessionAuditLogs برای ردیابی و دسترسی منشی/ادمین (PerformedByUserId از ICurrentUserService در AuditService پر می‌شود)
                var oldValue = new { Status = oldStatus, CashBalance = oldCashBalance };
                var newValue = new { Status = (int)CashSessionStatus.Closed, FinalCashAmount = finalAmount, ClosedAt = DateTime.Now, EndedByUserId = endedByUserId };
                var auditClose = await _cashSessionAuditService.LogActionAsync(sessionId, "Close", oldValue, newValue, description ?? "پایان جلسه صندوق");
                if (!auditClose.Success)
                    _logger.Warning("⚠️ CashSessionAuditLog Close ثبت نشد: {Message}", auditClose.Message);
                
                _logger.Information("✅ Cash session ended successfully - SessionId: {SessionId}, FinalAmount: {FinalAmount}, Difference: {Difference}",
                    sessionId, finalAmount, updatedSession.Difference);
                
                return ServiceResult<CashSession>.Successful(updatedSession, "جلسه صندوق با موفقیت بسته شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error ending cash session - SessionId: {SessionId}, FinalAmount: {FinalAmount}",
                    sessionId, finalAmount);
                return ServiceResult<CashSession>.Failed("خطا در بستن جلسه صندوق: " + ex.Message);
            }
        }

        /// <summary>
        /// دریافت جلسه نقدی فعال/باز برای کاربر
        /// ✅ پیاده‌سازی واقعی برای Production
        /// </summary>
        public async Task<ServiceResult<CashSession>> GetActiveCashSessionAsync(string userId)
        {
            try
            {
                _logger.Information("🔍 Getting active cash session for user {UserId}", userId);
                
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("⚠️ UserId is null or empty");
                    return ServiceResult<CashSession>.Failed("شناسه کاربر نامعتبر است.");
                }
                
                // ✅ دریافت جلسات کاربر و پیدا کردن جلسه فعال/باز
                var userSessions = await _cashSessionRepository.GetByUserIdAsync(userId);
                var userSessionsList = userSessions?.ToList() ?? new List<CashSession>();
                
                _logger.Information("🔍 Found {Count} cash sessions for user {UserId}", userSessionsList.Count, userId);
                
                // ✅ Log تمام جلسات برای Debug
                foreach (var session in userSessionsList)
                {
                    _logger.Information("📋 Session - Id: {SessionId}, Status: {Status}, IsDeleted: {IsDeleted}, ClosedAt: {ClosedAt}, UserId: {UserId}",
                        session.CashSessionId, session.Status, session.IsDeleted, session.ClosedAt, session.UserId);
                }
                
                var activeSession = userSessionsList
                    .FirstOrDefault(cs => !cs.IsDeleted && 
                                         (cs.Status == CashSessionStatus.Active || cs.Status == CashSessionStatus.Open) &&
                                         cs.ClosedAt == null);
                
                if (activeSession == null)
                {
                    _logger.Warning("⚠️ No active/open cash session found for user {UserId}. Total sessions: {Count}, Filtered: {FilteredCount}",
                        userId, userSessionsList.Count, 
                        userSessionsList.Count(cs => !cs.IsDeleted && (cs.Status == CashSessionStatus.Active || cs.Status == CashSessionStatus.Open) && cs.ClosedAt == null));
                    return ServiceResult<CashSession>.Failed(
                        "جلسه صندوق باز/فعالی برای شما یافت نشد.",
                        "NO_ACTIVE_SESSION");
                }
                
                _logger.Information("✅ Found active/open cash session - SessionId: {SessionId}, UserId: {UserId}, Status: {Status}",
                    activeSession.CashSessionId, userId, activeSession.Status);
                
                return ServiceResult<CashSession>.Successful(activeSession);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting active cash session for user {UserId}", userId);
                return ServiceResult<CashSession>.Failed("خطا در دریافت جلسه نقدی فعال: " + ex.Message);
            }
        }

        /// <summary>
        /// دریافت جلسات نقدی کاربر
        /// ✅ پیاده‌سازی واقعی برای Production
        /// </summary>
        public async Task<ServiceResult<IEnumerable<CashSession>>> GetUserCashSessionsAsync(string userId, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                _logger.Information("🔍 Getting user cash sessions - UserId: {UserId}, Page: {PageNumber}, PageSize: {PageSize}",
                    userId, pageNumber, pageSize);
                
                // ✅ اگر UserId null یا empty است، از دیتابیس جلسات را بدون فیلتر UserId بگیریم
                // این برای حالتی است که کاربر لاگین نشده یا UserId در دسترس نیست
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("⚠️ UserId is null or empty - returning all sessions");
                    // دریافت تمام جلسات (بدون فیلتر UserId)
                    var allSessions = await _cashSessionRepository.GetAllAsync(pageNumber, pageSize);
                    return ServiceResult<IEnumerable<CashSession>>.Successful(allSessions);
                }
                
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;
                
                // ✅ صفحه‌بندی در سطح DB — بدون بارگذاری همه جلسات کاربر در حافظه
                var pagedSessions = await _cashSessionRepository.GetByUserIdPagedAsync(userId, pageNumber, pageSize);
                var list = pagedSessions as IList<CashSession> ?? pagedSessions.ToList();
                
                _logger.Information("✅ Found {Count} cash sessions for user {UserId} (page {Page})",
                    list.Count, userId, pageNumber);
                
                return ServiceResult<IEnumerable<CashSession>>.Successful(list);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting user cash sessions - UserId: {UserId}", userId);
                return ServiceResult<IEnumerable<CashSession>>.Failed("خطا در دریافت جلسات نقدی کاربر: " + ex.Message);
            }
        }

        /// <summary>
        /// دریافت جلسات نقدی بر اساس تاریخ
        /// ✅ پیاده‌سازی واقعی برای Production
        /// </summary>
        public async Task<ServiceResult<IEnumerable<CashSession>>> GetCashSessionsByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                _logger.Information("🔍 Getting cash sessions by date range - StartDate: {StartDate}, EndDate: {EndDate}, Page: {PageNumber}",
                    startDate, endDate, pageNumber);
                
                if (startDate > endDate)
                {
                    _logger.Warning("⚠️ StartDate is after EndDate");
                    return ServiceResult<IEnumerable<CashSession>>.Failed("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.");
                }
                
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;
                
                var sessions = await _cashSessionRepository.GetByDateRangeAsync(startDate, endDate);
                var pagedSessions = sessions
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                _logger.Information("✅ Found {Count} cash sessions in date range",
                    pagedSessions.Count);
                
                return ServiceResult<IEnumerable<CashSession>>.Successful(pagedSessions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting cash sessions by date range - StartDate: {StartDate}, EndDate: {EndDate}",
                    startDate, endDate);
                return ServiceResult<IEnumerable<CashSession>>.Failed("خطا در دریافت جلسات نقدی: " + ex.Message);
            }
        }

        public async Task<ServiceResult<CashBalance>> CalculateCashBalanceAsync(int sessionId)
        {
            // FIXME(Phase 2): Implement cash balance calculation
            _logger.Warning("⚠️ POS MANAGEMENT: CalculateCashBalanceAsync not implemented yet");
            return await Task.FromResult(ServiceResult<CashBalance>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult<CashBalance>> CalculateUserCashBalanceAsync(string userId)
        {
            // FIXME(Phase 2): Implement user cash balance calculation
            _logger.Warning("⚠️ POS MANAGEMENT: CalculateUserCashBalanceAsync not implemented yet");
            return await Task.FromResult(ServiceResult<CashBalance>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult<DailyCashBalance>> CalculateDailyCashBalanceAsync(DateTime date)
        {
            // FIXME(Phase 2): Implement daily cash balance calculation
            _logger.Warning("⚠️ POS MANAGEMENT: CalculateDailyCashBalanceAsync not implemented yet");
            return await Task.FromResult(ServiceResult<DailyCashBalance>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult> AddCashBalanceAsync(int sessionId, decimal amount, string description, string userId)
        {
            // FIXME(Phase 2): Implement add cash balance
            _logger.Warning("⚠️ POS MANAGEMENT: AddCashBalanceAsync not implemented yet");
            return await Task.FromResult(ServiceResult.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult> SubtractCashBalanceAsync(int sessionId, decimal amount, string description, string userId)
        {
            // FIXME(Phase 2): Implement subtract cash balance
            _logger.Warning("⚠️ POS MANAGEMENT: SubtractCashBalanceAsync not implemented yet");
            return await Task.FromResult(ServiceResult.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult<PosTerminalStatistics>> GetPosTerminalStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // FIXME(Phase 2): Implement POS terminal statistics
            _logger.Warning("⚠️ POS MANAGEMENT: GetPosTerminalStatisticsAsync not implemented yet");
            return await Task.FromResult(ServiceResult<PosTerminalStatistics>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult<CashSessionStatistics>> GetCashSessionStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // FIXME(Phase 2): Implement cash session statistics
            _logger.Warning("⚠️ POS MANAGEMENT: GetCashSessionStatisticsAsync not implemented yet");
            return await Task.FromResult(ServiceResult<CashSessionStatistics>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult<DailyPosStatistics>> GetDailyPosStatisticsAsync(DateTime date)
        {
            // FIXME(Phase 2): Implement daily POS statistics
            _logger.Warning("⚠️ POS MANAGEMENT: GetDailyPosStatisticsAsync not implemented yet");
            return await Task.FromResult(ServiceResult<DailyPosStatistics>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult> ValidatePosTerminalAsync(int terminalId)
        {
            // FIXME(Phase 2): Implement POS terminal validation
            _logger.Warning("⚠️ POS MANAGEMENT: ValidatePosTerminalAsync not implemented yet");
            return await Task.FromResult(ServiceResult.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult> CanUsePosTerminalAsync(int terminalId)
        {
            // FIXME(Phase 2): Implement POS terminal usage check
            _logger.Warning("⚠️ POS MANAGEMENT: CanUsePosTerminalAsync not implemented yet");
            return await Task.FromResult(ServiceResult.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult> ValidateCashSessionAsync(int sessionId)
        {
            // FIXME(Phase 2): Implement cash session validation
            _logger.Warning("⚠️ POS MANAGEMENT: ValidateCashSessionAsync not implemented yet");
            return await Task.FromResult(ServiceResult.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult<IEnumerable<PosTerminal>>> GetTerminalsAsync(int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var items = await _posTerminalRepository.GetAllAsync(pageNumber, pageSize);
                return ServiceResult<IEnumerable<PosTerminal>>.Successful(items);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting POS terminals list");
                return ServiceResult<IEnumerable<PosTerminal>>.Failed("خطا در دریافت ترمینال‌ها");
            }
        }

        public async Task<ServiceResult<PosTerminal>> GetTerminalByIdAsync(int terminalId)
        {
            return await GetPosTerminalAsync(terminalId);
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

        /// <summary>
        /// دریافت جلسه نقدی بر اساس شناسه
        /// ✅ پیاده‌سازی واقعی برای Production
        /// </summary>
        public async Task<ServiceResult<CashSession>> GetSessionByIdAsync(int sessionId)
        {
            try
            {
                _logger.Information("🔍 Getting cash session by ID - SessionId: {SessionId}", sessionId);
                
                if (sessionId <= 0)
                {
                    _logger.Warning("⚠️ SessionId is invalid: {SessionId}", sessionId);
                    return ServiceResult<CashSession>.Failed("شناسه جلسه نامعتبر است.");
                }
                
                var session = await _cashSessionRepository.GetByIdAsync(sessionId);
                if (session == null)
                {
                    _logger.Warning("⚠️ Cash session not found - SessionId: {SessionId}", sessionId);
                    return ServiceResult<CashSession>.Failed("جلسه صندوق یافت نشد.");
                }
                
                _logger.Information("✅ Found cash session - SessionId: {SessionId}, Status: {Status}, UserId: {UserId}",
                    sessionId, session.Status, session.UserId);
                
                return ServiceResult<CashSession>.Successful(session);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting cash session by ID - SessionId: {SessionId}", sessionId);
                return ServiceResult<CashSession>.Failed("خطا در دریافت جلسه صندوق: " + ex.Message);
            }
        }

        public async Task<ServiceResult<IEnumerable<CashSession>>> GetActiveSessionsAsync()
        {
            try
            {
                var sessions = await _cashSessionRepository.GetActiveSessionsAsync();
                return ServiceResult<IEnumerable<CashSession>>.Successful(sessions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting active cash sessions");
                return ServiceResult<IEnumerable<CashSession>>.Failed("خطا در دریافت جلسات فعال");
            }
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

        #region Payment Processing Methods

        /// <summary>
        /// اعتبارسنجی پرداخت
        /// </summary>
        public async Task<ServiceResult<bool>> ValidatePaymentAsync(int receptionId, decimal amount)
        {
            try
            {
                _logger.Information("Validating payment for reception {ReceptionId} with amount {Amount}", receptionId, amount);
                
                // TODO: Implement actual validation logic
                // This is a placeholder implementation
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error validating payment for reception {ReceptionId}", receptionId);
                return ServiceResult<bool>.Failed("خطا در اعتبارسنجی پرداخت");
            }
        }

        /// <summary>
        /// ثبت پرداخت POS
        /// </summary>
        public async Task<ServiceResult<bool>> RegisterPosPaymentAsync(int receptionId, PosPaymentDto posPayment)
        {
            try
            {
                _logger.Information("Registering POS payment for reception {ReceptionId} with amount {Amount}", receptionId, posPayment.Amount);
                
                // TODO: Implement actual POS payment registration logic
                // This is a placeholder implementation
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error registering POS payment for reception {ReceptionId}", receptionId);
                return ServiceResult<bool>.Failed("خطا در ثبت پرداخت POS");
            }
        }

        /// <summary>
        /// ثبت پرداخت نقدی
        /// </summary>
        public async Task<ServiceResult<bool>> RegisterCashPaymentAsync(int receptionId, CashPaymentDto cashPayment, int sessionId)
        {
            try
            {
                _logger.Information("Registering cash payment for reception {ReceptionId} with amount {Amount} in session {SessionId}", 
                    receptionId, cashPayment.Amount, sessionId);
                
                // TODO: Implement actual cash payment registration logic
                // This is a placeholder implementation
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error registering cash payment for reception {ReceptionId}", receptionId);
                return ServiceResult<bool>.Failed("خطا در ثبت پرداخت نقدی");
            }
        }

        /// <summary>
        /// دریافت جلسه نقدی باز/فعال برای کاربر
        /// ✅ پیاده‌سازی واقعی برای Production
        /// </summary>
        public async Task<ServiceResult<CashSession>> GetOpenCashSessionAsync(string userId)
        {
            try
            {
                _logger.Information("🔍 Getting open/active cash session for user {UserId}", userId);
                
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("⚠️ UserId is null or empty");
                    return ServiceResult<CashSession>.Failed("شناسه کاربر نامعتبر است.");
                }
                
                // ✅ بررسی وجود جلسه فعال/باز برای کاربر
                var hasActiveSession = await _cashSessionRepository.HasActiveSessionAsync(userId);
                if (!hasActiveSession)
                {
                    _logger.Warning("⚠️ No active/open cash session found for user {UserId}", userId);
                    return ServiceResult<CashSession>.Failed(
                        "جلسه صندوق باز/فعالی برای شما یافت نشد.\n\n" +
                        "لطفاً ابتدا جلسه صندوق را باز کنید.",
                        "NO_ACTIVE_SESSION");
                }
                
                // ✅ دریافت جلسات کاربر و پیدا کردن جلسه فعال/باز
                var userSessions = await _cashSessionRepository.GetByUserIdAsync(userId);
                var activeSession = userSessions
                    .FirstOrDefault(cs => !cs.IsDeleted && 
                                         (cs.Status == CashSessionStatus.Active || cs.Status == CashSessionStatus.Open) &&
                                         cs.ClosedAt == null);
                
                if (activeSession == null)
                {
                    _logger.Warning("⚠️ Active session check passed but session not found for user {UserId}", userId);
                    return ServiceResult<CashSession>.Failed(
                        "جلسه صندوق باز/فعالی برای شما یافت نشد.\n\n" +
                        "لطفاً ابتدا جلسه صندوق را باز کنید.",
                        "NO_ACTIVE_SESSION");
                }
                
                _logger.Information("✅ Found active/open cash session - SessionId: {SessionId}, UserId: {UserId}, Status: {Status}, OpenedAt: {OpenedAt}",
                    activeSession.CashSessionId, userId, activeSession.Status, activeSession.OpenedAt);
                
                return ServiceResult<CashSession>.Successful(activeSession);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting open cash session for user {UserId}", userId);
                return ServiceResult<CashSession>.Failed("خطا در دریافت جلسه نقدی باز: " + ex.Message);
            }
        }

        #endregion
    }
}
