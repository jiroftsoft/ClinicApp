using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس ناوبری تخصصی برای ماژول پذیرش - Strongly Typed
    /// </summary>
    public class ReceptionNavigationService : IReceptionNavigationService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionNavigationService(
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// ایجاد ناوبری پزشکی تخصصی - Strongly Typed
        /// </summary>
        public async Task<ServiceResult<MedicalNavigationViewModel>> GetMedicalNavigationAsync(string currentController, string currentAction)
        {
            try
            {
                _logger.Information("🏥 ایجاد ناوبری پزشکی تخصصی - Controller: {Controller}, Action: {Action}", 
                    currentController, currentAction);

                var navigation = new MedicalNavigationViewModel
                {
                    CurrentController = currentController,
                    CurrentAction = currentAction,
                    UserName = _currentUserService.UserName,
                    ClinicName = "کلینیک تخصصی",
                    CurrentDate = DateTime.Now,
                    CurrentShift = GetCurrentShift(),
                    IsEmergencyMode = false,
                    Sections = await GetNavigationSectionsAsync(currentController, currentAction),
                    QuickActions = (await GetQuickActionsAsync()).Data ?? new List<QuickAction>()
                };

                _logger.Information("✅ ناوبری پزشکی با موفقیت ایجاد شد - تعداد بخش‌ها: {SectionCount}", 
                    navigation.Sections.Count);

                return ServiceResult<MedicalNavigationViewModel>.Successful(navigation, "ناوبری پزشکی با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ایجاد ناوبری پزشکی");
                return ServiceResult<MedicalNavigationViewModel>.Failed("خطا در ایجاد ناوبری پزشکی");
            }
        }

        /// <summary>
        /// دریافت بخش‌های ناوبری - Strongly Typed
        /// </summary>
        private async Task<List<NavigationSection>> GetNavigationSectionsAsync(string currentController, string currentAction)
        {
            var sections = new List<NavigationSection>
            {
                new NavigationSection
                {
                    SectionId = "reception-management",
                    Title = "مدیریت پذیرش",
                    Icon = "fas fa-clipboard-list",
                    Order = 1,
                    Items = GetReceptionManagementItems(currentController, currentAction)
                },
                new NavigationSection
                {
                    SectionId = "patient-management",
                    Title = "مدیریت بیماران",
                    Icon = "fas fa-users",
                    Order = 2,
                    Items = GetPatientManagementItems(currentController, currentAction)
                },
                new NavigationSection
                {
                    SectionId = "service-management",
                    Title = "مدیریت خدمات",
                    Icon = "fas fa-cogs",
                    Order = 3,
                    Items = GetServiceManagementItems(currentController, currentAction)
                },
                new NavigationSection
                {
                    SectionId = "financial-management",
                    Title = "مدیریت مالی",
                    Icon = "fas fa-credit-card",
                    Order = 4,
                    Items = GetFinancialManagementItems(currentController, currentAction)
                },
                new NavigationSection
                {
                    SectionId = "reports-statistics",
                    Title = "گزارش‌ها و آمار",
                    Icon = "fas fa-chart-bar",
                    Order = 5,
                    Items = GetReportsStatisticsItems(currentController, currentAction)
                },
                new NavigationSection
                {
                    SectionId = "settings",
                    Title = "تنظیمات",
                    Icon = "fas fa-cog",
                    Order = 6,
                    Items = GetSettingsItems(currentController, currentAction)
                }
            };

            return sections;
        }

        /// <summary>
        /// آیتم‌های مدیریت پذیرش - Strongly Typed
        /// </summary>
        private List<NavigationItem> GetReceptionManagementItems(string currentController, string currentAction)
        {
            return new List<NavigationItem>
            {
                new NavigationItem
                {
                    ItemId = "new-reception",
                    Title = "فرم پذیرش جدید",
                    Description = "ثبت پذیرش جدید",
                    Icon = "fas fa-plus-circle",
                    Controller = "Reception",
                    Action = "Index",
                    IsActive = currentController == "Reception" && currentAction == "Index",
                    Order = 1,
                    Tooltip = "ایجاد پذیرش جدید برای بیمار"
                },
                new NavigationItem
                {
                    ItemId = "patient-search",
                    Title = "جستجوی بیمار",
                    Description = "جستجوی پیشرفته بیماران",
                    Icon = "fas fa-search",
                    Controller = "ReceptionPatientSearch",
                    Action = "Index",
                    IsActive = currentController == "ReceptionPatientSearch",
                    Order = 2,
                    Tooltip = "جستجوی بیماران در سیستم"
                },
                new NavigationItem
                {
                    ItemId = "reception-list",
                    Title = "لیست پذیرش‌ها",
                    Description = "مشاهده تمام پذیرش‌ها",
                    Icon = "fas fa-list",
                    Controller = "ReceptionList",
                    Action = "Index",
                    IsActive = currentController == "ReceptionList",
                    Order = 3,
                    Tooltip = "لیست تمام پذیرش‌های سیستم"
                },
                new NavigationItem
                {
                    ItemId = "reception-history",
                    Title = "سوابق پذیرش",
                    Description = "مشاهده سوابق پذیرش",
                    Icon = "fas fa-history",
                    Controller = "ReceptionHistory",
                    Action = "Index",
                    IsActive = currentController == "ReceptionHistory",
                    Order = 4,
                    Tooltip = "سوابق و تاریخچه پذیرش‌ها"
                }
            };
        }

        /// <summary>
        /// آیتم‌های مدیریت بیماران - Strongly Typed
        /// </summary>
        private List<NavigationItem> GetPatientManagementItems(string currentController, string currentAction)
        {
            return new List<NavigationItem>
            {
                new NavigationItem
                {
                    ItemId = "patient-management",
                    Title = "مدیریت بیماران",
                    Description = "مدیریت اطلاعات بیماران",
                    Icon = "fas fa-users",
                    Controller = "ReceptionPatientManagement",
                    Action = "Index",
                    IsActive = currentController == "ReceptionPatientManagement",
                    Order = 1,
                    Tooltip = "مدیریت کامل اطلاعات بیماران"
                },
                new NavigationItem
                {
                    ItemId = "patient-registration",
                    Title = "ثبت بیمار جدید",
                    Description = "ثبت بیمار جدید در سیستم",
                    Icon = "fas fa-user-plus",
                    Controller = "ReceptionPatientRegistration",
                    Action = "Index",
                    IsActive = currentController == "ReceptionPatientRegistration",
                    Order = 2,
                    Tooltip = "ثبت بیمار جدید در سیستم"
                },
                new NavigationItem
                {
                    ItemId = "patient-insurance",
                    Title = "بیمه بیماران",
                    Description = "مدیریت بیمه بیماران",
                    Icon = "fas fa-shield-alt",
                    Controller = "ReceptionPatientInsurance",
                    Action = "Index",
                    IsActive = currentController == "ReceptionPatientInsurance",
                    Order = 3,
                    Tooltip = "مدیریت بیمه‌های بیماران"
                }
            };
        }

        /// <summary>
        /// آیتم‌های مدیریت خدمات - Strongly Typed
        /// </summary>
        private List<NavigationItem> GetServiceManagementItems(string currentController, string currentAction)
        {
            return new List<NavigationItem>
            {
                new NavigationItem
                {
                    ItemId = "service-management",
                    Title = "مدیریت خدمات",
                    Description = "مدیریت خدمات پزشکی",
                    Icon = "fas fa-cogs",
                    Controller = "ReceptionServiceManagement",
                    Action = "Index",
                    IsActive = currentController == "ReceptionServiceManagement",
                    Order = 1,
                    Tooltip = "مدیریت خدمات پزشکی"
                },
                new NavigationItem
                {
                    ItemId = "department-management",
                    Title = "مدیریت دپارتمان‌ها",
                    Description = "مدیریت دپارتمان‌های پزشکی",
                    Icon = "fas fa-building",
                    Controller = "ReceptionDepartmentManagement",
                    Action = "Index",
                    IsActive = currentController == "ReceptionDepartmentManagement",
                    Order = 2,
                    Tooltip = "مدیریت دپارتمان‌های پزشکی"
                },
                new NavigationItem
                {
                    ItemId = "doctor-management",
                    Title = "مدیریت پزشکان",
                    Description = "مدیریت اطلاعات پزشکان",
                    Icon = "fas fa-user-md",
                    Controller = "ReceptionDoctorManagement",
                    Action = "Index",
                    IsActive = currentController == "ReceptionDoctorManagement",
                    Order = 3,
                    Tooltip = "مدیریت اطلاعات پزشکان"
                }
            };
        }

        /// <summary>
        /// آیتم‌های مدیریت مالی - Strongly Typed
        /// </summary>
        private List<NavigationItem> GetFinancialManagementItems(string currentController, string currentAction)
        {
            return new List<NavigationItem>
            {
                new NavigationItem
                {
                    ItemId = "payment-management",
                    Title = "مدیریت پرداخت‌ها",
                    Description = "مدیریت پرداخت‌های بیماران",
                    Icon = "fas fa-credit-card",
                    Controller = "ReceptionPaymentManagement",
                    Action = "Index",
                    IsActive = currentController == "ReceptionPaymentManagement",
                    Order = 1,
                    Tooltip = "مدیریت پرداخت‌های بیماران"
                },
                new NavigationItem
                {
                    ItemId = "invoice-management",
                    Title = "مدیریت فاکتورها",
                    Description = "مدیریت فاکتورهای پزشکی",
                    Icon = "fas fa-file-invoice",
                    Controller = "ReceptionInvoiceManagement",
                    Action = "Index",
                    IsActive = currentController == "ReceptionInvoiceManagement",
                    Order = 2,
                    Tooltip = "مدیریت فاکتورهای پزشکی"
                },
                new NavigationItem
                {
                    ItemId = "financial-reports",
                    Title = "گزارش‌های مالی",
                    Description = "گزارش‌های مالی کلینیک",
                    Icon = "fas fa-chart-line",
                    Controller = "ReceptionFinancialReports",
                    Action = "Index",
                    IsActive = currentController == "ReceptionFinancialReports",
                    Order = 3,
                    Tooltip = "گزارش‌های مالی کلینیک"
                }
            };
        }

        /// <summary>
        /// آیتم‌های گزارش‌ها و آمار - Strongly Typed
        /// </summary>
        private List<NavigationItem> GetReportsStatisticsItems(string currentController, string currentAction)
        {
            return new List<NavigationItem>
            {
                new NavigationItem
                {
                    ItemId = "daily-reports",
                    Title = "گزارش روزانه",
                    Description = "گزارش‌های روزانه کلینیک",
                    Icon = "fas fa-calendar-day",
                    Controller = "ReceptionDailyReports",
                    Action = "Index",
                    IsActive = currentController == "ReceptionDailyReports",
                    Order = 1,
                    Tooltip = "گزارش‌های روزانه کلینیک"
                },
                new NavigationItem
                {
                    ItemId = "monthly-reports",
                    Title = "گزارش ماهانه",
                    Description = "گزارش‌های ماهانه کلینیک",
                    Icon = "fas fa-calendar-alt",
                    Controller = "ReceptionMonthlyReports",
                    Action = "Index",
                    IsActive = currentController == "ReceptionMonthlyReports",
                    Order = 2,
                    Tooltip = "گزارش‌های ماهانه کلینیک"
                },
                new NavigationItem
                {
                    ItemId = "statistics",
                    Title = "آمار کلی",
                    Description = "آمار و نمودارهای کلی",
                    Icon = "fas fa-chart-bar",
                    Controller = "ReceptionStatistics",
                    Action = "Index",
                    IsActive = currentController == "ReceptionStatistics",
                    Order = 3,
                    Tooltip = "آمار و نمودارهای کلی"
                }
            };
        }

        /// <summary>
        /// آیتم‌های تنظیمات - Strongly Typed
        /// </summary>
        private List<NavigationItem> GetSettingsItems(string currentController, string currentAction)
        {
            return new List<NavigationItem>
            {
                new NavigationItem
                {
                    ItemId = "reception-settings",
                    Title = "تنظیمات پذیرش",
                    Description = "تنظیمات ماژول پذیرش",
                    Icon = "fas fa-cog",
                    Controller = "ReceptionSettings",
                    Action = "Index",
                    IsActive = currentController == "ReceptionSettings",
                    Order = 1,
                    Tooltip = "تنظیمات ماژول پذیرش"
                },
                new NavigationItem
                {
                    ItemId = "user-preferences",
                    Title = "تنظیمات کاربری",
                    Description = "تنظیمات شخصی کاربر",
                    Icon = "fas fa-user-cog",
                    Controller = "ReceptionUserPreferences",
                    Action = "Index",
                    IsActive = currentController == "ReceptionUserPreferences",
                    Order = 2,
                    Tooltip = "تنظیمات شخصی کاربر"
                }
            };
        }

        /// <summary>
        /// دریافت عملیات سریع - Strongly Typed
        /// </summary>
        public async Task<ServiceResult<List<QuickAction>>> GetQuickActionsAsync()
        {
            try
            {
                _logger.Information("🏥 دریافت عملیات سریع");

                var quickActions = new List<QuickAction>
                {
                    new QuickAction
                    {
                        ActionId = "emergency-reception",
                        Title = "پذیرش اورژانس",
                        Icon = "fas fa-ambulance",
                        Controller = "Reception",
                        Action = "Emergency",
                        CssClass = "btn-danger",
                        Tooltip = "پذیرش فوری بیمار اورژانسی",
                        IsVisible = true
                    },
                    new QuickAction
                    {
                        ActionId = "quick-search",
                        Title = "جستجوی سریع",
                        Icon = "fas fa-search",
                        Controller = "ReceptionPatientSearch",
                        Action = "QuickSearch",
                        CssClass = "btn-info",
                        Tooltip = "جستجوی سریع بیمار",
                        IsVisible = true
                    },
                    new QuickAction
                    {
                        ActionId = "today-receptions",
                        Title = "پذیرش‌های امروز",
                        Icon = "fas fa-calendar-day",
                        Controller = "ReceptionList",
                        Action = "Today",
                        CssClass = "btn-primary",
                        Tooltip = "مشاهده پذیرش‌های امروز",
                        IsVisible = true
                    }
                };

                return ServiceResult<List<QuickAction>>.Successful(quickActions, "عملیات سریع با موفقیت دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت عملیات سریع");
                return ServiceResult<List<QuickAction>>.Failed("خطا در دریافت عملیات سریع");
            }
        }

        /// <summary>
        /// دریافت ناوبری دپارتمان‌ها - Strongly Typed
        /// </summary>
        public async Task<ServiceResult<DepartmentNavigationViewModel>> GetDepartmentNavigationAsync(int? selectedDepartmentId = null)
        {
            try
            {
                _logger.Information("🏥 دریافت ناوبری دپارتمان‌ها - SelectedDepartmentId: {DepartmentId}", selectedDepartmentId);

                var navigation = new DepartmentNavigationViewModel
                {
                    SelectedDepartmentId = selectedDepartmentId ?? 0,
                    SelectedDepartmentName = "همه دپارتمان‌ها",
                    Departments = new List<DepartmentNavigationItem>
                    {
                        new DepartmentNavigationItem
                        {
                            DepartmentId = 1,
                            DepartmentName = "اورژانس",
                            Icon = "fas fa-ambulance",
                            CssClass = "department-emergency",
                            IsActive = selectedDepartmentId == 1,
                            PatientCount = 5,
                            ReceptionCount = 8,
                            Status = "فعال"
                        },
                        new DepartmentNavigationItem
                        {
                            DepartmentId = 2,
                            DepartmentName = "قلب",
                            Icon = "fas fa-heart",
                            CssClass = "department-cardiology",
                            IsActive = selectedDepartmentId == 2,
                            PatientCount = 3,
                            ReceptionCount = 5,
                            Status = "فعال"
                        }
                    }
                };

                return ServiceResult<DepartmentNavigationViewModel>.Successful(navigation, "ناوبری دپارتمان‌ها با موفقیت دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت ناوبری دپارتمان‌ها");
                return ServiceResult<DepartmentNavigationViewModel>.Failed("خطا در دریافت ناوبری دپارتمان‌ها");
            }
        }

        /// <summary>
        /// بررسی دسترسی کاربر به آیتم ناوبری - Strongly Typed
        /// </summary>
        public async Task<bool> HasNavigationPermissionAsync(string controller, string action, string permission = null)
        {
            try
            {
                _logger.Information("🔐 بررسی دسترسی ناوبری - Controller: {Controller}, Action: {Action}, Permission: {Permission}", 
                    controller, action, permission);

                // TODO: Implement actual permission checking
                // For now, return true for all navigation items
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بررسی دسترسی ناوبری");
                return false;
            }
        }

        /// <summary>
        /// دریافت ناوبری بر اساس نقش کاربر - Strongly Typed
        /// </summary>
        public async Task<ServiceResult<MedicalNavigationViewModel>> GetNavigationByUserRoleAsync(string userRole, string currentController, string currentAction)
        {
            try
            {
                _logger.Information("🏥 دریافت ناوبری بر اساس نقش - UserRole: {UserRole}, Controller: {Controller}, Action: {Action}", 
                    userRole, currentController, currentAction);

                // Get base navigation
                var baseNavigation = await GetMedicalNavigationAsync(currentController, currentAction);
                
                if (!baseNavigation.Success)
                {
                    return baseNavigation;
                }

                // Filter navigation based on user role
                var filteredNavigation = baseNavigation.Data;
                
                // TODO: Implement role-based filtering
                // For now, return all navigation items
                
                return ServiceResult<MedicalNavigationViewModel>.Successful(filteredNavigation, "ناوبری بر اساس نقش با موفقیت دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت ناوبری بر اساس نقش");
                return ServiceResult<MedicalNavigationViewModel>.Failed("خطا در دریافت ناوبری بر اساس نقش");
            }
        }

        /// <summary>
        /// تعیین شیفت فعلی
        /// </summary>
        private string GetCurrentShift()
        {
            var hour = DateTime.Now.Hour;
            if (hour >= 6 && hour < 14)
                return "صبح";
            else if (hour >= 14 && hour < 22)
                return "عصر";
            else
                return "شب";
        }
    }
}
