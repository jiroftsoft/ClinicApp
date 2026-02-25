using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;
using ClinicApp.Controllers;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using ReceptionEntity = ClinicApp.Models.Entities.Reception.Reception;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Extensions;
using ClinicApp.Filters;
using Serilog;
using Newtonsoft.Json;

namespace ClinicApp.Controllers.ReceptionV2
{
    /// <summary>
    /// کنترلر لیست پذیرش‌ها V2 - نسخه بهینه‌شده و حرفه‌ای
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. Zero Cache برای محیط درمانی
    /// 2. فیلتر پیشرفته و صفحه‌بندی
    /// 3. پرداخت مجدد با POS
    /// 4. چاپ قبض و بیمه تکمیلی
    /// 5. مدیریت بدهی‌ها
    /// فقط کاربران با نقش Admin یا Receptionist (منشی).
    /// </summary>
    [RoutePrefix("ReceptionV2/ReceptionList")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    [NoCache]
    public class ReceptionListV2Controller : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// Constructor - Dependency Injection
        /// توجه: _receptionService حذف شد چون در این کنترلر استفاده نمی‌شود
        /// </summary>
        public ReceptionListV2Controller(
            ApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger logger) : base(logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// صفحه لیست پذیرش‌ها
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                _logger.Information("🏥 V2: نمایش صفحه لیست پذیرش‌ها");
                
                var viewModel = new ReceptionListViewModel
                {
                    CurrentPage = 1,
                    PageSize = 20
                };
                
                return View("~/Views/ReceptionV2/ReceptionList/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ V2: خطا در نمایش صفحه لیست پذیرش‌ها");
                return View("Error");
            }
        }

        /// <summary>
        /// دریافت لیست پذیرش‌ها با فیلتر و صفحه‌بندی
        /// بهینه‌سازی شده برای محیط درمانی با:
        /// - AsNoTracking برای performance
        /// - Validation کامل برای page و pageSize
        /// - Error handling جامع
        /// - Logging حرفه‌ای
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryTokenOnPosts]
        [Route("GetReceptionList")]
        public async Task<JsonResult> GetReceptionList(ReceptionListFilterViewModel filters, int page = 1, int pageSize = 20)
        {
            try
            {
                // Validation برای page و pageSize
                if (page < 1)
                {
                    _logger.Warning("⚠️ V2: صفحه نامعتبر: {Page}", page);
                    page = 1;
                }
                
                if (pageSize < 1 || pageSize > 100)
                {
                    _logger.Warning("⚠️ V2: اندازه صفحه نامعتبر: {PageSize}, تنظیم به 20", pageSize);
                    pageSize = 20;
                }
                
                // بررسی ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.Warning("⚠️ V2: ModelState نامعتبر - Errors: {@Errors}", errors);
                    
                    // در صورت خطای ModelState، filters را null می‌کنیم تا query بدون فیلتر اجرا شود
                    filters = null;
                }
                
                _logger.Information("📋 V2: دریافت لیست پذیرش‌ها - صفحه: {Page}, اندازه: {PageSize}, Filters: {@Filters}", 
                    page, pageSize, filters);

                // بررسی null safety - 🏥 MEDICAL: Critical checks for clinical environment
                if (_context == null)
                {
                    _logger.Error("❌ V2: ApplicationDbContext is null");
                    return Json(ServiceResult<ReceptionListViewModel>.Failed(
                        "خطا در اتصال به پایگاه داده", 
                        "DB_CONTEXT_NULL", 
                        ErrorCategory.System, 
                        SecurityLevel.High), 
                        JsonRequestBehavior.AllowGet);
                }

                if (_context.Receptions == null)
                {
                    _logger.Error("❌ V2: Receptions DbSet is null");
                    return Json(ServiceResult<ReceptionListViewModel>.Failed(
                        "خطا در دسترسی به داده‌های پذیرش", 
                        "DBSET_NULL", 
                        ErrorCategory.System, 
                        SecurityLevel.High), 
                        JsonRequestBehavior.AllowGet);
                }

                // شروع query با logging و null safety
                // 🏥 MEDICAL: استفاده از AsNoTracking برای performance بهتر در محیط درمانی
                // ⚠️ مهم: Doctor را Include نمی‌کنیم چون Degree enum مشکل دارد - از Select projection استفاده می‌کنیم
                _logger.Information("📋 V2: شروع ساخت query");
                
                IQueryable<ReceptionEntity> query;
                try
                {
                    _logger.Debug("📋 V2: شروع ساخت query با AsNoTracking و Include ها");
                    
                    // ساخت query پایه - Doctor را Include نمی‌کنیم (مشکل Degree enum)
                    query = _context.Receptions
                        .AsNoTracking() // 🚀 Performance: عدم track کردن entity ها برای read-only operations
                        .Include(r => r.Patient)
                        // .Include(r => r.Doctor) // ❌ حذف شد: مشکل Degree enum (string در DB اما enum در model)
                        .Include(r => r.Department)
                        .Include(r => r.Transactions)
                        .Include(r => r.ReceptionItems)
                        .Where(r => !r.IsDeleted)
                        // 🏥 MEDICAL: فیلتر Draft های ناقص (بدون خدمت) - فقط Draft هایی که دارای خدمت هستند نمایش داده می‌شوند
                        .Where(r => r.Status != ReceptionStatus.Pending || 
                                   r.TotalAmount > 0 || 
                                   r.ReceptionItems.Any(ri => !ri.IsDeleted));
                    
                    _logger.Information("📋 V2: Query اولیه با AsNoTracking و Include ها ساخته شد (بدون Doctor)");
                }
                catch (Exception queryEx)
                {
                    _logger.Error(queryEx, "❌ V2: خطا در ساخت query اولیه - ExceptionType: {ExceptionType}, Message: {Message}, InnerException: {InnerException}", 
                        queryEx.GetType().Name, 
                        queryEx.Message, 
                        queryEx.InnerException?.Message ?? "None");
                    return Json(ServiceResult<ReceptionListViewModel>.Failed(
                        $"خطا در ساخت درخواست پایگاه داده: {queryEx.Message}", 
                        "QUERY_BUILD_ERROR", 
                        ErrorCategory.System, 
                        SecurityLevel.Medium), 
                        JsonRequestBehavior.AllowGet);
                }

                // فیلتر بر اساس کد ملی - استفاده از join با Patients table
                if (filters != null && !string.IsNullOrWhiteSpace(filters.NationalCode))
                {
                    var nationalCode = filters.NationalCode.Trim();
                    // استفاده از navigation property بعد از Include
                    query = query.Where(r => r.Patient != null && r.Patient.NationalCode != null && r.Patient.NationalCode.Contains(nationalCode));
                    _logger.Information("📋 V2: فیلتر کد ملی اعمال شد: {NationalCode}", nationalCode);
                }

                // فیلتر بر اساس نام بیمار - استفاده از navigation property بعد از Include
                if (filters != null && !string.IsNullOrWhiteSpace(filters.PatientName))
                {
                    var patientName = filters.PatientName.Trim();
                    query = query.Where(r => r.Patient != null && 
                        ((r.Patient.FirstName != null && r.Patient.FirstName.Contains(patientName)) || 
                         (r.Patient.LastName != null && r.Patient.LastName.Contains(patientName))));
                    _logger.Information("📋 V2: فیلتر نام بیمار اعمال شد: {PatientName}", patientName);
                }

                // فیلتر بر اساس تاریخ - با null safety
                if (filters != null && !string.IsNullOrWhiteSpace(filters.DateFrom))
                {
                    try
                    {
                        var dateFrom = filters.DateFrom.ToDateTimeNullable();
                        if (dateFrom.HasValue)
                        {
                            query = query.Where(r => r.ReceptionDate >= dateFrom.Value);
                            _logger.Information("📋 V2: فیلتر تاریخ از اعمال شد: {DateFrom}", dateFrom.Value);
                        }
                    }
                    catch (Exception dateEx)
                    {
                        _logger.Warning(dateEx, "⚠️ V2: خطا در parse کردن تاریخ از: {DateFrom}", filters.DateFrom);
                    }
                }

                if (filters != null && !string.IsNullOrWhiteSpace(filters.DateTo))
                {
                    try
                    {
                        var dateTo = filters.DateTo.ToDateTimeNullable();
                        if (dateTo.HasValue)
                        {
                            query = query.Where(r => r.ReceptionDate <= dateTo.Value);
                            _logger.Information("📋 V2: فیلتر تاریخ تا اعمال شد: {DateTo}", dateTo.Value);
                        }
                    }
                    catch (Exception dateEx)
                    {
                        _logger.Warning(dateEx, "⚠️ V2: خطا در parse کردن تاریخ تا: {DateTo}", filters.DateTo);
                    }
                }

                // فیلتر بر اساس وضعیت
                if (filters != null && filters.Status.HasValue)
                {
                    query = query.Where(r => r.Status == filters.Status.Value);
                    _logger.Information("📋 V2: فیلتر وضعیت اعمال شد: {Status}", filters.Status.Value);
                }

                // فیلتر بر اساس پزشک
                if (filters != null && filters.DoctorId.HasValue)
                {
                    query = query.Where(r => r.DoctorId == filters.DoctorId.Value);
                    _logger.Information("📋 V2: فیلتر پزشک اعمال شد: {DoctorId}", filters.DoctorId.Value);
                }

                // فیلتر بر اساس دپارتمان
                if (filters != null && filters.DepartmentId.HasValue)
                {
                    query = query.Where(r => r.DepartmentId == filters.DepartmentId.Value);
                    _logger.Information("📋 V2: فیلتر دپارتمان اعمال شد: {DepartmentId}", filters.DepartmentId.Value);
                }

                // فیلتر بر اساس بدهی - استفاده از subquery برای بهینه‌سازی
                // 🏥 MEDICAL: بدهی باید بر اساس سهم بیمار (PatientCoPay) باشد، نه TotalAmount
                if (filters != null && filters.HasDebt.HasValue && filters.HasDebt.Value)
                {
                    // استفاده از navigation property بعد از Include
                    // بدهی = سهم بیمار - مبلغ پرداخت شده > 0
                    query = query.Where(r => 
                        r.PatientCoPay > (r.Transactions
                            .Where(t => t.Status == PaymentStatus.Success && !t.IsDeleted)
                            .Sum(t => (decimal?)t.Amount) ?? 0m));
                    _logger.Information("📋 V2: فیلتر بدهی اعمال شد (بر اساس سهم بیمار)");
                }

                // 🏥 MEDICAL: فیلتر بر اساس شماره پذیرش رسمی (ReceptionNo)
                if (filters != null && !string.IsNullOrWhiteSpace(filters.ReceptionNo))
                {
                    query = query.Where(r => r.ReceptionNo != null && r.ReceptionNo.Contains(filters.ReceptionNo.Trim()));
                    _logger.Information("📋 V2: فیلتر شماره پذیرش اعمال شد: {ReceptionNo}", filters.ReceptionNo);
                }

                // 🏥 MEDICAL: فیلتر بر اساس شماره الکترونیکی (ElectronicReceptionNumber)
                if (filters != null && !string.IsNullOrWhiteSpace(filters.ElectronicReceptionNumber))
                {
                    var electronicNumber = filters.ElectronicReceptionNumber.Trim();
                    
                    // استخراج PatientId از ElectronicReceptionNumber برای جستجوی سریع‌تر
                    var parts = electronicNumber.Split('-');
                    if (parts.Length >= 1 && int.TryParse(parts[0], out int patientId))
                    {
                        // جستجو بر اساس PatientId (سریع‌تر)
                        query = query.Where(r => r.PatientId == patientId);
                        _logger.Information("📋 V2: فیلتر شماره الکترونیکی اعمال شد (بر اساس PatientId): {PatientId}, ElectronicNumber: {ElectronicNumber}", 
                            patientId, electronicNumber);
                    }
                    else
                    {
                        // جستجو بر اساس خود ElectronicReceptionNumber
                        query = query.Where(r => r.ElectronicReceptionNumber != null && 
                            r.ElectronicReceptionNumber.Contains(electronicNumber));
                        _logger.Information("📋 V2: فیلتر شماره الکترونیکی اعمال شد (مستقیم): {ElectronicNumber}", electronicNumber);
                    }
                }

                // شمارش کل با error handling
                int totalCount = 0;
                try
                {
                    _logger.Information("📋 V2: شمارش کل رکوردها");
                    totalCount = await query.CountAsync();
                    _logger.Information("📋 V2: تعداد کل رکوردها: {TotalCount}", totalCount);
                }
                catch (Exception countEx)
                {
                    _logger.Error(countEx, "❌ V2: خطا در شمارش رکوردها");
                    return Json(ServiceResult<ReceptionListViewModel>.Failed(
                        "خطا در شمارش رکوردها", 
                        "COUNT_ERROR", 
                        ErrorCategory.Database, 
                        SecurityLevel.Medium), 
                        JsonRequestBehavior.AllowGet);
                }

                // اعمال صفحه‌بندی با error handling
                List<ReceptionEntity> receptions = new List<ReceptionEntity>();
                try
                {
                    _logger.Information("📋 V2: اعمال صفحه‌بندی - Skip: {Skip}, Take: {Take}", 
                        (page - 1) * pageSize, pageSize);
                    
                    // بررسی query قبل از اجرا
                    _logger.Debug("📋 V2: Query قبل از OrderBy: {QueryType}", query.GetType().Name);
                    
                    var orderedQuery = query.OrderByDescending(r => r.ReceptionDate);
                    _logger.Debug("📋 V2: Query بعد از OrderBy ساخته شد");
                    
                    var pagedQuery = orderedQuery.Skip((page - 1) * pageSize).Take(pageSize);
                    _logger.Debug("📋 V2: Query بعد از Skip/Take ساخته شد");
                    
                    _logger.Information("📋 V2: شروع اجرای ToListAsync...");
                    receptions = await pagedQuery.ToListAsync();
                    
                    _logger.Information("📋 V2: تعداد رکوردهای دریافت شده: {Count}", receptions.Count);
                }
                catch (Exception listEx)
                {
                    _logger.Error(listEx, "❌ V2: خطا در دریافت لیست پذیرش‌ها از پایگاه داده - ExceptionType: {ExceptionType}, Message: {Message}, InnerException: {InnerException}, StackTrace: {StackTrace}", 
                        listEx.GetType().Name, 
                        listEx.Message, 
                        listEx.InnerException?.Message ?? "None", 
                        listEx.StackTrace);
                    
                    // بررسی نوع exception برای پیام بهتر
                    string errorMessage = "خطا در دریافت لیست پذیرش‌ها از پایگاه داده";
                    string errorCode = "DATA_FETCH_ERROR";
                    
                    if (listEx is System.Data.Entity.Core.EntityException entityEx)
                    {
                        errorMessage = $"خطا در اتصال به پایگاه داده: {entityEx.Message}";
                        errorCode = "DB_CONNECTION_ERROR";
                    }
                    else if (listEx is System.Data.SqlClient.SqlException sqlEx)
                    {
                        errorMessage = $"خطا در اجرای SQL: {sqlEx.Message} (Error Number: {sqlEx.Number})";
                        errorCode = "SQL_EXECUTION_ERROR";
                    }
                    else if (listEx.InnerException != null)
                    {
                        errorMessage = $"خطا: {listEx.InnerException.Message}";
                    }
                    else
                    {
                        errorMessage = $"خطا: {listEx.Message}";
                    }
                    
                    return Json(ServiceResult<ReceptionListViewModel>.Failed(
                        errorMessage, 
                        errorCode, 
                        ErrorCategory.Database, 
                        SecurityLevel.Medium), 
                        JsonRequestBehavior.AllowGet);
                }

                // تبدیل به ViewModel با null safety
                _logger.Information("📋 V2: شروع تبدیل به ViewModel");
                var items = new List<ReceptionListItemViewModel>();
                
                // دریافت اطلاعات Doctor ها به صورت جداگانه برای جلوگیری از مشکل Degree enum
                var doctorIds = receptions.Select(r => r.DoctorId).Distinct().ToList();
                var doctors = new Dictionary<int, string>();
                
                if (doctorIds.Any())
                {
                    try
                    {
                        _logger.Debug("📋 V2: دریافت اطلاعات Doctor ها - تعداد: {Count}", doctorIds.Count);
                        var doctorNames = await _context.Doctors
                            .AsNoTracking()
                            .Where(d => doctorIds.Contains(d.DoctorId) && !d.IsDeleted)
                            .Select(d => new { d.DoctorId, FirstName = d.FirstName ?? "", LastName = d.LastName ?? "" })
                            .ToListAsync();
                        
                        foreach (var doc in doctorNames)
                        {
                            doctors[doc.DoctorId] = $"{doc.FirstName} {doc.LastName}".Trim();
                        }
                        
                        _logger.Debug("📋 V2: اطلاعات {Count} Doctor دریافت شد", doctors.Count);
                    }
                    catch (Exception docEx)
                    {
                        _logger.Warning(docEx, "⚠️ V2: خطا در دریافت اطلاعات Doctor ها - ادامه بدون آن");
                    }
                }
                
                foreach (var r in receptions)
                {
                    try
                    {
                        // محاسبه مبلغ پرداخت شده با null safety
                        var paidAmount = 0m;
                        if (r.Transactions != null)
                        {
                            var successfulTransactions = r.Transactions
                                .Where(t => t.Status == PaymentStatus.Success && !t.IsDeleted)
                                .ToList();
                            paidAmount = successfulTransactions.Sum(t => (decimal?)t.Amount) ?? 0m;
                        }
                        
                        // 🏥 MEDICAL: محاسبه سهم‌های بیمه از ReceptionItems و SnapshotJson
                        // این روش دقیق‌تر است چون از داده‌های واقعی محاسبه شده استفاده می‌کند
                        decimal baseInsuranceShare = 0m;
                        decimal supplementaryInsuranceShare = 0m;
                        decimal patientShareAmount = 0m;
                        
                        if (r.ReceptionItems != null && r.ReceptionItems.Any(i => !i.IsDeleted))
                        {
                            var activeItems = r.ReceptionItems.Where(i => !i.IsDeleted).ToList();
                            
                            foreach (var receptionItem in activeItems)
                            {
                                // استخراج سهم‌ها از SnapshotJson (دقیق‌ترین روش)
                                long itemBaseCovered = 0;
                                long itemSuppCovered = 0;
                                
                                if (!string.IsNullOrEmpty(receptionItem.SnapshotJson))
                                {
                                    try
                                    {
                                        var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(receptionItem.SnapshotJson);
                                        if (snapshot != null)
                                        {
                                            if (snapshot.PrimaryPays != null)
                                                itemBaseCovered = (long)snapshot.PrimaryPays;
                                            if (snapshot.SupplementaryPays != null)
                                                itemSuppCovered = (long)snapshot.SupplementaryPays;
                                        }
                                    }
                                    catch (Exception snapshotEx)
                                    {
                                        _logger.Warning(snapshotEx, "⚠️ V2: خطا در parse کردن SnapshotJson برای ReceptionItem {ReceptionItemId}", receptionItem.ReceptionItemId);
                                    }
                                }
                                
                                // Fallback: اگر SnapshotJson موجود نبود، از InsurerShareAmount استفاده کن
                                if (itemBaseCovered == 0 && itemSuppCovered == 0)
                                {
                                    var insurerShare = (long)receptionItem.InsurerShareAmount;
                                    if (r.BasePlanId.HasValue && r.SupplementaryPlanId.HasValue)
                                    {
                                        // اگر هر دو بیمه وجود دارد، تقسیم مساوی (یا می‌توان منطق بهتری اعمال کرد)
                                        itemBaseCovered = insurerShare / 2;
                                        itemSuppCovered = insurerShare - itemBaseCovered;
                                    }
                                    else if (r.BasePlanId.HasValue)
                                    {
                                        itemBaseCovered = insurerShare;
                                    }
                                    else if (r.SupplementaryPlanId.HasValue)
                                    {
                                        itemSuppCovered = insurerShare;
                                    }
                                }
                                
                                baseInsuranceShare += itemBaseCovered;
                                supplementaryInsuranceShare += itemSuppCovered;
                                patientShareAmount += receptionItem.PatientShareAmount;
                            }
                        }
                        else
                        {
                            // Fallback: اگر ReceptionItems موجود نبود، از فیلدهای Reception استفاده کن
                            baseInsuranceShare = r.BasePay;
                            supplementaryInsuranceShare = r.SuppPay;
                            patientShareAmount = r.PatientPay;
                        }
                        
                        // 🏥 MEDICAL: محاسبه RemainingAmount بر اساس سهم بیمار
                        // RemainingAmount = سهم بیمار - مبلغ پرداخت شده
                        var remainingAmount = patientShareAmount - paidAmount;
                        if (remainingAmount < 0)
                            remainingAmount = 0m;
                        
                        // دریافت نام Doctor از dictionary
                        var doctorName = "—";
                        if (r.DoctorId > 0 && doctors.ContainsKey(r.DoctorId))
                        {
                            doctorName = doctors[r.DoctorId];
                        }
                        
                        var item = new ReceptionListItemViewModel
                        {
                            ReceptionId = r.ReceptionId,
                            PatientId = r.PatientId,
                            PatientName = r.Patient != null 
                                ? $"{r.Patient.FirstName ?? ""} {r.Patient.LastName ?? ""}".Trim()
                                : "—",
                            PatientNationalCode = r.Patient?.NationalCode ?? "—",
                            DoctorName = doctorName,
                            DepartmentName = r.Department?.Name ?? "—",
                            ReceptionDate = r.ReceptionDate,
                            ReceptionDateShamsi = r.ReceptionDate.ToPersianDate(),
                            Status = r.Status,
                            StatusText = GetStatusText(r.Status),
                            TotalAmount = r.TotalAmount,
                            PaidAmount = paidAmount,
                            RemainingAmount = remainingAmount,
                            PaymentMethod = r.Transactions?
                                .Where(t => t.Status == PaymentStatus.Success && !t.IsDeleted)
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.Method,
                            ServiceCount = r.ReceptionItems?.Count(i => !i.IsDeleted) ?? 0,
                            ReceiptNo = r.ReceptionNo ?? r.ReceptionNumber ?? "—",
                            ReceptionNo = r.ReceptionNo ?? "—", // 🏥 MEDICAL: شماره پذیرش رسمی
                            ElectronicReceptionNumber = r.ElectronicReceptionNumber ?? "—", // 🏥 MEDICAL: شماره الکترونیکی
                            Notes = r.Notes,
                            SupplementaryPlanId = r.SupplementaryPlanId, // 🏥 MEDICAL: شناسه بیمه تکمیلی برای چاپ
                            // 🏥 MEDICAL: محاسبات دقیق سهم بیمه‌ها از ReceptionItems و SnapshotJson
                            BaseInsuranceShare = baseInsuranceShare,
                            SupplementaryInsuranceShare = supplementaryInsuranceShare,
                            PatientShareAmount = patientShareAmount
                        };
                        
                        items.Add(item);
                    }
                    catch (Exception itemEx)
                    {
                        _logger.Warning(itemEx, "⚠️ V2: خطا در تبدیل Reception {ReceptionId} به ViewModel", r?.ReceptionId);
                        // ادامه با آیتم بعدی
                    }
                }
                
                _logger.Information("📋 V2: تعداد آیتم‌های تبدیل شده: {Count}", items.Count);

                var result = new ReceptionListViewModel
                {
                    Items = items,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Filters = filters ?? new ReceptionListFilterViewModel()
                };

                _logger.Information("📋 V2: نتیجه آماده شد - Items: {ItemsCount}, TotalCount: {TotalCount}", 
                    result.Items.Count, result.TotalCount);

                var serviceResult = ServiceResult<ReceptionListViewModel>.Successful(
                    result, 
                    $"لیست پذیرش‌ها با موفقیت دریافت شد. تعداد: {totalCount}"
                );
                
                _logger.Information("📋 V2: ServiceResult ایجاد شد - Success: {Success}, Message: {Message}", 
                    serviceResult.Success, serviceResult.Message);
                
                return Json(serviceResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ V2: خطا در دریافت لیست پذیرش‌ها - Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                    ex.GetType().Name, ex.Message, ex.StackTrace);
                
                // بررسی نوع exception برای پیام بهتر - 🏥 MEDICAL: User-friendly error messages
                string errorMessage = "خطا در دریافت لیست پذیرش‌ها";
                string errorCode = "GENERAL_ERROR";
                ErrorCategory errorCategory = ErrorCategory.General;
                SecurityLevel securityLevel = SecurityLevel.Medium;
                
                if (ex is System.Data.Entity.Core.EntityException)
                {
                    errorMessage = "خطا در اتصال به پایگاه داده. لطفاً دوباره تلاش کنید.";
                    errorCode = "DB_CONNECTION_ERROR";
                    errorCategory = ErrorCategory.Database;
                }
                else if (ex is System.Data.SqlClient.SqlException)
                {
                    errorMessage = "خطا در اجرای درخواست پایگاه داده. لطفاً با پشتیبانی تماس بگیرید.";
                    errorCode = "SQL_EXECUTION_ERROR";
                    errorCategory = ErrorCategory.Database;
                    securityLevel = SecurityLevel.High; // SQL errors might expose sensitive info
                }
                else if (ex is NullReferenceException)
                {
                    errorMessage = "خطا در دسترسی به داده‌ها. لطفاً صفحه را نوسازی کنید.";
                    errorCode = "NULL_REFERENCE_ERROR";
                    errorCategory = ErrorCategory.System;
                }
                else if (ex is ArgumentException || ex is ArgumentNullException)
                {
                    errorMessage = "اطلاعات ورودی نامعتبر است. لطفاً دوباره تلاش کنید.";
                    errorCode = "INVALID_ARGUMENT";
                    errorCategory = ErrorCategory.Validation;
                }
                
                return Json(ServiceResult<ReceptionListViewModel>.Failed(
                    errorMessage, 
                    errorCode, 
                    errorCategory, 
                    securityLevel), 
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت متن وضعیت پذیرش
        /// </summary>
        private string GetStatusText(ReceptionStatus status)
        {
            return status switch
            {
                ReceptionStatus.Pending => "در انتظار",
                ReceptionStatus.Completed => "تکمیل شده",
                ReceptionStatus.Cancelled => "لغو شده",
                ReceptionStatus.InProgress => "در حال انجام",
                ReceptionStatus.NeedsAdditionalPayment => "نیاز به پرداخت بیشتر",
                _ => status.ToString()
            };
        }
    }
}

