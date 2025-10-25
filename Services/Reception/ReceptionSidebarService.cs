using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس تخصصی مدیریت سایدبار پذیرش - طراحی شده برای محیط درمانی
    /// مسئولیت: مدیریت کامل سایدبار شامل آمار، هشدارها، و اقدامات سریع
    /// </summary>
    public class ReceptionSidebarService : IReceptionSidebarService
    {
        private readonly IReceptionService _receptionService;
        private readonly IPatientService _patientService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionSidebarService(
            IReceptionService receptionService,
            IPatientService patientService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger.ForContext<ReceptionSidebarService>();
        }

        #region Sidebar Statistics

        /// <summary>
        /// دریافت آمار امروز برای سایدبار
        /// </summary>
        public async Task<ServiceResult<SidebarStatistics>> GetTodayStatisticsAsync()
        {
            try
            {
                _logger.Information("🏥 SIDEBAR: درخواست آمار امروز برای سایدبار. User: {UserName}", _currentUserService.UserName);

                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // دریافت آمار پذیرش‌های امروز
                var receptionsResult = await _receptionService.GetReceptionsByDateRangeAsync(today, tomorrow);
                
                var statistics = new SidebarStatistics
                {
                    TodayReceptions = receptionsResult.Success ? receptionsResult.Data.Count() : 0,
                    PendingReceptions = receptionsResult.Success ? receptionsResult.Data.Count(r => r.Status == "در انتظار") : 0,
                    CompletedReceptions = receptionsResult.Success ? receptionsResult.Data.Count(r => r.Status == "تکمیل شده") : 0,
                    TotalRevenue = receptionsResult.Success ? receptionsResult.Data.Sum(r => r.TotalAmount) : 0,
                    LastUpdated = DateTime.Now
                };

                _logger.Information("✅ آمار امروز دریافت شد - Today: {Today}, Pending: {Pending}, Completed: {Completed}. User: {UserName}",
                    statistics.TodayReceptions, statistics.PendingReceptions, statistics.CompletedReceptions, _currentUserService.UserName);

                return ServiceResult<SidebarStatistics>.Successful(statistics);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت آمار امروز. User: {UserName}", _currentUserService.UserName);
                return ServiceResult<SidebarStatistics>.Failed("خطا در دریافت آمار امروز");
            }
        }

        /// <summary>
        /// دریافت هشدارهای پزشکی برای سایدبار
        /// </summary>
        public async Task<ServiceResult<List<MedicalAlert>>> GetMedicalAlertsAsync()
        {
            try
            {
                _logger.Information("🏥 SIDEBAR: درخواست هشدارهای پزشکی برای سایدبار. User: {UserName}", _currentUserService.UserName);

                var alerts = new List<MedicalAlert>();
                var alertId = 1;
                
                foreach (var alertSample in ReceptionConstants.SampleData.MedicalAlerts)
                {
                    alerts.Add(new MedicalAlert
                    {
                        Id = alertId++,
                        Type = alertSample.Type,
                        Title = alertSample.Title,
                        Message = alertSample.Message,
                        Priority = alertSample.Priority,
                        CreatedAt = DateTime.Now.AddMinutes(-5 * alertId),
                        IsActive = true
                    });
                }

                _logger.Information("✅ {Count} هشدار پزشکی دریافت شد. User: {UserName}", alerts.Count, _currentUserService.UserName);
                return ServiceResult<List<MedicalAlert>>.Successful(alerts);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت هشدارهای پزشکی. User: {UserName}", _currentUserService.UserName);
                return ServiceResult<List<MedicalAlert>>.Failed("خطا در دریافت هشدارهای پزشکی");
            }
        }

        /// <summary>
        /// دریافت وضعیت بیمه‌ها برای سایدبار
        /// </summary>
        public async Task<ServiceResult<InsuranceStatus>> GetInsuranceStatusAsync()
        {
            try
            {
                _logger.Information("🏥 SIDEBAR: درخواست وضعیت بیمه‌ها برای سایدبار. User: {UserName}", _currentUserService.UserName);

                // در اینجا باید از سرویس بیمه استفاده کنیم
                // فعلاً داده‌های نمونه برمی‌گردانیم
                var status = new InsuranceStatus
                {
                    ActiveInsurances = 15,
                    ExpiredInsurances = 3,
                    PendingValidations = 2,
                    LastUpdated = DateTime.Now
                };

                _logger.Information("✅ وضعیت بیمه‌ها دریافت شد - Active: {Active}, Expired: {Expired}. User: {UserName}",
                    status.ActiveInsurances, status.ExpiredInsurances, _currentUserService.UserName);

                return ServiceResult<InsuranceStatus>.Successful(status);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت وضعیت بیمه‌ها. User: {UserName}", _currentUserService.UserName);
                return ServiceResult<InsuranceStatus>.Failed("خطا در دریافت وضعیت بیمه‌ها");
            }
        }

        /// <summary>
        /// دریافت وضعیت پرداخت‌ها برای سایدبار
        /// </summary>
        public async Task<ServiceResult<PaymentStatus>> GetPaymentStatusAsync()
        {
            try
            {
                _logger.Information("🏥 SIDEBAR: درخواست وضعیت پرداخت‌ها برای سایدبار. User: {UserName}", _currentUserService.UserName);

                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // دریافت آمار پرداخت‌های امروز
                var receptionsResult = await _receptionService.GetReceptionsByDateRangeAsync(today, tomorrow);
                
                var status = new PaymentStatus
                {
                    TodayPayments = receptionsResult.Success ? receptionsResult.Data.Count() : 0,
                    TotalAmount = receptionsResult.Success ? receptionsResult.Data.Sum(r => r.TotalAmount) : 0,
                    PendingPayments = receptionsResult.Success ? receptionsResult.Data.Count(r => r.Status == "در انتظار پرداخت") : 0,
                    LastUpdated = DateTime.Now
                };

                _logger.Information("✅ وضعیت پرداخت‌ها دریافت شد - Today: {Today}, Amount: {Amount}. User: {UserName}",
                    status.TodayPayments, status.TotalAmount, _currentUserService.UserName);

                return ServiceResult<PaymentStatus>.Successful(status);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت وضعیت پرداخت‌ها. User: {UserName}", _currentUserService.UserName);
                return ServiceResult<PaymentStatus>.Failed("خطا در دریافت وضعیت پرداخت‌ها");
            }
        }

        #endregion

        #region Sidebar Actions

        /// <summary>
        /// دریافت اقدامات سریع برای سایدبار
        /// </summary>
        public async Task<ServiceResult<List<QuickAction>>> GetQuickActionsAsync()
        {
            try
            {
                _logger.Information("🏥 SIDEBAR: درخواست اقدامات سریع برای سایدبار. User: {UserName}", _currentUserService.UserName);

                var actions = new List<QuickAction>();
                var actionId = 1;
                
                foreach (var actionSample in ReceptionConstants.SampleData.QuickActions)
                {
                    actions.Add(new QuickAction
                    {
                        ActionId = $"quick-action-{actionId++}",
                        Title = actionSample.Title,
                        Icon = actionSample.Icon,
                        Controller = "Reception",
                        Action = actionSample.Action,
                        CssClass = "btn-primary",
                        IsVisible = true,
                        Tooltip = actionSample.Description
                    });
                }

                _logger.Information("✅ {Count} اقدام سریع دریافت شد. User: {UserName}", actions.Count, _currentUserService.UserName);
                return ServiceResult<List<QuickAction>>.Successful(actions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت اقدامات سریع. User: {UserName}", _currentUserService.UserName);
                return ServiceResult<List<QuickAction>>.Failed("خطا در دریافت اقدامات سریع");
            }
        }

        #endregion
    }
}
