using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;
using ClinicApp.Controllers;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Extensions;
using ClinicApp.Filters;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر لیست پذیرش‌ها - تخصصی برای ماژول پذیرش
    /// </summary>
    [RoutePrefix("Reception/ReceptionList")]
    public class ReceptionListController : BaseController
    {
        private readonly IReceptionService _receptionService;
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionListController(
            IReceptionService receptionService,
            ApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger logger) : base(logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
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
                _logger.Information("🏥 نمایش صفحه لیست پذیرش‌ها");
                
                var viewModel = new ReceptionListViewModel
                {
                    CurrentPage = 1,
                    PageSize = 20
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در نمایش صفحه لیست پذیرش‌ها");
                return View("Error");
            }
        }

        /// <summary>
        /// دریافت لیست پذیرش‌ها با فیلتر و صفحه‌بندی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<JsonResult> GetReceptionList(ReceptionListFilterViewModel filters, int page = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("📋 دریافت لیست پذیرش‌ها - صفحه: {Page}, اندازه: {PageSize}", page, pageSize);

                var query = _context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.Department)
                    .Include(r => r.Transactions)
                    .Include(r => r.ReceptionItems)
                    .Where(r => !r.IsDeleted)
                    .AsQueryable();

                // فیلتر بر اساس کد ملی
                if (!string.IsNullOrWhiteSpace(filters?.NationalCode))
                {
                    query = query.Where(r => r.Patient.NationalCode.Contains(filters.NationalCode));
                }

                // فیلتر بر اساس نام بیمار
                if (!string.IsNullOrWhiteSpace(filters?.PatientName))
                {
                    query = query.Where(r => 
                        r.Patient.FirstName.Contains(filters.PatientName) || 
                        r.Patient.LastName.Contains(filters.PatientName));
                }

                // فیلتر بر اساس تاریخ
                if (!string.IsNullOrWhiteSpace(filters?.DateFrom))
                {
                    var dateFrom = filters.DateFrom.ToDateTimeNullable();
                    if (dateFrom.HasValue)
                        query = query.Where(r => r.ReceptionDate >= dateFrom.Value);
                }

                if (!string.IsNullOrWhiteSpace(filters?.DateTo))
                {
                    var dateTo = filters.DateTo.ToDateTimeNullable();
                    if (dateTo.HasValue)
                        query = query.Where(r => r.ReceptionDate <= dateTo.Value);
                }

                // فیلتر بر اساس وضعیت
                if (filters?.Status.HasValue == true)
                {
                    query = query.Where(r => r.Status == filters.Status.Value);
                }

                // فیلتر بر اساس پزشک
                if (filters?.DoctorId.HasValue == true)
                {
                    query = query.Where(r => r.DoctorId == filters.DoctorId.Value);
                }

                // فیلتر بر اساس دپارتمان
                if (filters?.DepartmentId.HasValue == true)
                {
                    query = query.Where(r => r.DepartmentId == filters.DepartmentId.Value);
                }

                // شمارش کل
                var totalCount = await query.CountAsync();

                // اعمال صفحه‌بندی
                var receptions = await query
                    .OrderByDescending(r => r.ReceptionDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // تبدیل به ViewModel
                var items = receptions.Select(r => new ReceptionListItemViewModel
                {
                    ReceptionId = r.ReceptionId,
                    PatientId = r.PatientId,
                    PatientName = $"{r.Patient.FirstName} {r.Patient.LastName}",
                    PatientNationalCode = r.Patient.NationalCode,
                    DoctorName = r.Doctor != null ? $"{r.Doctor.FirstName} {r.Doctor.LastName}" : "—",
                    DepartmentName = r.Department != null ? r.Department.Name : "—",
                    ReceptionDate = r.ReceptionDate,
                    ReceptionDateShamsi = r.ReceptionDate.ToPersianDate(),
                    Status = r.Status,
                    StatusText = GetStatusText(r.Status),
                    TotalAmount = r.TotalAmount,
                    PaidAmount = r.Transactions?.Where(t => t.Status == PaymentStatus.Success && !t.IsDeleted).Sum(t => t.Amount) ?? 0,
                    RemainingAmount = r.TotalAmount - (r.Transactions?.Where(t => t.Status == PaymentStatus.Success && !t.IsDeleted).Sum(t => t.Amount) ?? 0),
                    PaymentMethod = r.Transactions?.Where(t => t.Status == PaymentStatus.Success && !t.IsDeleted).OrderByDescending(t => t.CreatedAt).FirstOrDefault()?.Method,
                    ServiceCount = r.ReceptionItems?.Count(i => !i.IsDeleted) ?? 0,
                    ReceiptNo = r.ReceptionNo ?? r.ReceptionNumber,
                    Notes = r.Notes
                }).ToList();

                var result = new ReceptionListViewModel
                {
                    Items = items,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Filters = filters ?? new ReceptionListFilterViewModel()
                };

                return Json(ServiceResult<ReceptionListViewModel>.Successful(result, "لیست پذیرش‌ها با موفقیت دریافت شد"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت لیست پذیرش‌ها");
                return Json(ServiceResult<ReceptionListViewModel>.Failed("خطا در دریافت لیست پذیرش‌ها"), JsonRequestBehavior.AllowGet);
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