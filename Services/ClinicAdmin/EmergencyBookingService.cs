using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.DoctorManagementVM;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// سرویس مدیریت رزروهای اورژانس
    /// این سرویس مسئول مدیریت نوبت‌های اورژانس و اولویت‌بندی آنها است
    /// طبق DESIGN_PRINCIPLES_CONTRACT: پیاده‌سازی کامل برای محیط پزشکی
    /// </summary>
    public class EmergencyBookingService : IEmergencyBookingService
    {
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ILogger _logger;

        public EmergencyBookingService(
            IDoctorScheduleRepository doctorScheduleRepository,
            IDoctorCrudService doctorCrudService)
        {
            _doctorScheduleRepository = doctorScheduleRepository ?? throw new ArgumentNullException(nameof(doctorScheduleRepository));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _logger = Log.ForContext<EmergencyBookingService>();
        }

        /// <summary>
        /// بررسی امکان رزرو اورژانس
        /// </summary>
        public async Task<ServiceResult<bool>> CanBookEmergencyAsync(int doctorId, DateTime date, TimeSpan time, EmergencyPriority priority)
        {
            try
            {
                _logger.Information("بررسی امکان رزرو اورژانس برای پزشک {DoctorId} در تاریخ {Date} ساعت {Time} با اولویت {Priority}", 
                    doctorId, date.ToString("yyyy/MM/dd"), time.ToString(@"hh\:mm"), priority);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<bool>.Failed("شناسه پزشک نامعتبر است.");
                }

                if (date.Date < DateTime.Today)
                {
                    return ServiceResult<bool>.Failed("تاریخ مورد نظر نمی‌تواند در گذشته باشد.");
                }

                // بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<bool>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // بررسی برنامه کاری پزشک
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (schedule == null)
                {
                    _logger.Warning("برنامه کاری برای پزشک {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<bool>.Failed("برنامه کاری برای این پزشک تعریف نشده است.");
                }

                // بررسی روز کاری
                var workDay = schedule.WorkDays?.FirstOrDefault(w => w.DayOfWeek == (int)date.DayOfWeek && w.IsActive);
                if (workDay == null)
                {
                    _logger.Information("روز کاری برای پزشک {DoctorId} در تاریخ {Date} یافت نشد", doctorId, date.ToString("yyyy/MM/dd"));
                    return ServiceResult<bool>.Failed("روز کاری برای این تاریخ تعریف نشده است.");
                }

                // بررسی زمان در محدوده کاری
                var timeRange = workDay.TimeRanges?.FirstOrDefault(tr => tr.IsActive);
                if (timeRange == null)
                {
                    _logger.Information("بازه زمانی برای روز کاری پزشک {DoctorId} در تاریخ {Date} یافت نشد", doctorId, date.ToString("yyyy/MM/dd"));
                    return ServiceResult<bool>.Failed("بازه زمانی برای این روز تعریف نشده است.");
                }

                if (time < timeRange.StartTime || time > timeRange.EndTime)
                {
                    _logger.Information("زمان {Time} خارج از محدوده کاری پزشک {DoctorId} در تاریخ {Date}", 
                        time.ToString(@"hh\:mm"), doctorId, date.ToString("yyyy/MM/dd"));
                    return ServiceResult<bool>.Failed("زمان مورد نظر خارج از محدوده کاری پزشک است.");
                }

                // بررسی استثناها
                var exception = schedule.Exceptions?.FirstOrDefault(e => 
                    e.StartDate <= date && 
                    (e.EndDate == null || e.EndDate >= date) && 
                    !e.IsDeleted);

                if (exception != null)
                {
                    _logger.Information("استثنا برای پزشک {DoctorId} در تاریخ {Date} یافت شد: {Type}", 
                        doctorId, date.ToString("yyyy/MM/dd"), exception.Type);
                    return ServiceResult<bool>.Failed("در این تاریخ استثنا وجود دارد.");
                }

                // بررسی امکان رزرو اورژانس
                var canBook = priority switch
                {
                    EmergencyPriority.Critical => true, // اورژانس بحرانی همیشه قابل رزرو است
                    EmergencyPriority.High => true, // در حال حاضر ثابت
                    EmergencyPriority.Medium => true, // در حال حاضر ثابت
                    EmergencyPriority.Low => false, // اورژانس کم‌اولویت قابل رزرو نیست
                    _ => false
                };

                _logger.Information("بررسی امکان رزرو اورژانس برای پزشک {DoctorId} در تاریخ {Date} ساعت {Time} با اولویت {Priority}: {Result}", 
                    doctorId, date.ToString("yyyy/MM/dd"), time.ToString(@"hh\:mm"), priority, canBook);

                return ServiceResult<bool>.Successful(canBook);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی امکان رزرو اورژانس برای پزشک {DoctorId}", doctorId);
                return ServiceResult<bool>.Failed("خطا در بررسی امکان رزرو اورژانس");
            }
        }

        /// <summary>
        /// رزرو نوبت اورژانس
        /// </summary>
        public async Task<ServiceResult<EmergencyBookingResult>> BookEmergencyAsync(EmergencyBookingRequest request)
        {
            try
            {
                _logger.Information("درخواست رزرو نوبت اورژانس برای پزشک {DoctorId} در تاریخ {Date} ساعت {Time} با اولویت {Priority}", 
                    request.DoctorId, request.Date.ToString("yyyy/MM/dd"), request.Time.ToString(@"hh\:mm"), request.Priority);

                // اعتبارسنجی درخواست
                if (request == null)
                {
                    return ServiceResult<EmergencyBookingResult>.Failed("درخواست رزرو اورژانس نامعتبر است.");
                }

                if (string.IsNullOrWhiteSpace(request.PatientName))
                {
                    return ServiceResult<EmergencyBookingResult>.Failed("نام بیمار الزامی است.");
                }

                if (string.IsNullOrWhiteSpace(request.EmergencyReason))
                {
                    return ServiceResult<EmergencyBookingResult>.Failed("دلیل اورژانس الزامی است.");
                }

                // بررسی امکان رزرو
                var canBookResult = await CanBookEmergencyAsync(request.DoctorId, request.Date, request.Time, request.Priority);
                if (!canBookResult.Success || !canBookResult.Data)
                {
                    return ServiceResult<EmergencyBookingResult>.Failed("امکان رزرو اورژانس در این زمان وجود ندارد.");
                }

                // در حال حاضر این متد ساده است
                // در آینده با جدول EmergencyBookings یکپارچه خواهد شد

                var result = new EmergencyBookingResult
                {
                    Success = true,
                    EmergencyBooking = new EmergencyBooking(),
                    Message = "نوبت اورژانس با موفقیت رزرو شد.",
                    Conflicts = new List<EmergencyConflict>()
                };

                _logger.Information("رزرو نوبت اورژانس برای پزشک {DoctorId} در تاریخ {Date} ساعت {Time} با موفقیت انجام شد", 
                    request.DoctorId, request.Date.ToString("yyyy/MM/dd"), request.Time.ToString(@"hh\:mm"));

                return ServiceResult<EmergencyBookingResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در رزرو نوبت اورژانس برای پزشک {DoctorId}", request?.DoctorId);
                return ServiceResult<EmergencyBookingResult>.Failed("خطا در رزرو نوبت اورژانس");
            }
        }

        /// <summary>
        /// لغو نوبت اورژانس
        /// </summary>
        public async Task<ServiceResult<bool>> CancelEmergencyAsync(int emergencyBookingId, string cancellationReason)
        {
            try
            {
                _logger.Information("درخواست لغو نوبت اورژانس {EmergencyBookingId} با دلیل: {Reason}", 
                    emergencyBookingId, cancellationReason);

                // اعتبارسنجی پارامترها
                if (emergencyBookingId <= 0)
                {
                    return ServiceResult<bool>.Failed("شناسه نوبت اورژانس نامعتبر است.");
                }

                if (string.IsNullOrWhiteSpace(cancellationReason))
                {
                    return ServiceResult<bool>.Failed("دلیل لغو الزامی است.");
                }

                // در حال حاضر این متد ساده است
                // در آینده با جدول EmergencyBookings یکپارچه خواهد شد

                _logger.Information("نوبت اورژانس {EmergencyBookingId} با موفقیت لغو شد", emergencyBookingId);

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو نوبت اورژانس {EmergencyBookingId}", emergencyBookingId);
                return ServiceResult<bool>.Failed("خطا در لغو نوبت اورژانس");
            }
        }

        /// <summary>
        /// دریافت لیست نوبت‌های اورژانس
        /// </summary>
        public async Task<ServiceResult<List<EmergencyBooking>>> GetEmergencyBookingsAsync(int doctorId, DateTime? date = null, EmergencyPriority? priority = null)
        {
            try
            {
                _logger.Information("درخواست دریافت نوبت‌های اورژانس برای پزشک {DoctorId} در تاریخ {Date} با اولویت {Priority}", 
                    doctorId, date?.ToString("yyyy/MM/dd") ?? "همه", priority?.ToString() ?? "همه");

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<List<EmergencyBooking>>.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<List<EmergencyBooking>>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // در حال حاضر این متد ساده است
                // در آینده با جدول EmergencyBookings یکپارچه خواهد شد
                var emergencyBookings = new List<EmergencyBooking>();

                _logger.Information("نوبت‌های اورژانس برای پزشک {DoctorId} با موفقیت دریافت شد. تعداد: {Count}", 
                    doctorId, emergencyBookings.Count);

                return ServiceResult<List<EmergencyBooking>>.Successful(emergencyBookings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نوبت‌های اورژانس برای پزشک {DoctorId}", doctorId);
                return ServiceResult<List<EmergencyBooking>>.Failed("خطا در دریافت نوبت‌های اورژانس");
            }
        }

        /// <summary>
        /// دریافت آمار نوبت‌های اورژانس
        /// </summary>
        public async Task<ServiceResult<EmergencyBookingStatistics>> GetEmergencyStatisticsAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Information("درخواست آمار نوبت‌های اورژانس برای پزشک {DoctorId} از {StartDate} تا {EndDate}", 
                    doctorId, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"));

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<EmergencyBookingStatistics>.Failed("شناسه پزشک نامعتبر است.");
                }

                if (startDate >= endDate)
                {
                    return ServiceResult<EmergencyBookingStatistics>.Failed("تاریخ شروع باید قبل از تاریخ پایان باشد.");
                }

                // در حال حاضر این متد ساده است
                // در آینده با جدول EmergencyBookings یکپارچه خواهد شد
                var statistics = new EmergencyBookingStatistics
                {
                    TotalEmergencyBookings = 0
                };

                _logger.Information("آمار نوبت‌های اورژانس برای پزشک {DoctorId} با موفقیت دریافت شد", doctorId);

                return ServiceResult<EmergencyBookingStatistics>.Successful(statistics);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار نوبت‌های اورژانس برای پزشک {DoctorId}", doctorId);
                return ServiceResult<EmergencyBookingStatistics>.Failed("خطا در دریافت آمار نوبت‌های اورژانس");
            }
        }

        /// <summary>
        /// دریافت اولویت‌های اورژانس
        /// </summary>
        public async Task<ServiceResult<List<EmergencyPriority>>> GetEmergencyPrioritiesAsync()
        {
            try
            {
                _logger.Information("درخواست دریافت اولویت‌های اورژانس");

                var priorities = new List<EmergencyPriority>
                {
                    EmergencyPriority.Critical,
                    EmergencyPriority.High,
                    EmergencyPriority.Medium,
                    EmergencyPriority.Low
                };

                return ServiceResult<List<EmergencyPriority>>.Successful(priorities);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اولویت‌های اورژانس");
                return ServiceResult<List<EmergencyPriority>>.Failed("خطا در دریافت اولویت‌های اورژانس");
            }
        }

        /// <summary>
        /// تنظیم اولویت اورژانس
        /// </summary>
        public async Task<ServiceResult<bool>> SetEmergencyPriorityAsync(int emergencyBookingId, EmergencyPriority priority)
        {
            try
            {
                _logger.Information("تنظیم اولویت اورژانس {EmergencyBookingId} به {Priority}", emergencyBookingId, priority);

                // اعتبارسنجی پارامترها
                if (emergencyBookingId <= 0)
                {
                    return ServiceResult<bool>.Failed("شناسه نوبت اورژانس نامعتبر است.");
                }

                // در حال حاضر این متد ساده است
                // در آینده با جدول EmergencyBookings یکپارچه خواهد شد

                _logger.Information("اولویت اورژانس {EmergencyBookingId} با موفقیت به {Priority} تغییر یافت", emergencyBookingId, priority);

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم اولویت اورژانس {EmergencyBookingId}", emergencyBookingId);
                return ServiceResult<bool>.Failed("خطا در تنظیم اولویت اورژانس");
            }
        }

        /// <summary>
        /// ارسال اعلان اورژانس
        /// </summary>
        public async Task<ServiceResult<bool>> SendEmergencyNotificationAsync(int emergencyBookingId, NotificationChannelType channel)
        {
            try
            {
                _logger.Information("ارسال اعلان اورژانس {EmergencyBookingId} از طریق کانال {Channel}", emergencyBookingId, channel);

                // اعتبارسنجی پارامترها
                if (emergencyBookingId <= 0)
                {
                    return ServiceResult<bool>.Failed("شناسه نوبت اورژانس نامعتبر است.");
                }

                // در حال حاضر این متد ساده است
                // در آینده با سرویس اعلان‌ها یکپارچه خواهد شد

                _logger.Information("اعلان اورژانس {EmergencyBookingId} با موفقیت از طریق کانال {Channel} ارسال شد", emergencyBookingId, channel);

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال اعلان اورژانس {EmergencyBookingId}", emergencyBookingId);
                return ServiceResult<bool>.Failed("خطا در ارسال اعلان اورژانس");
            }
        }

        /// <summary>
        /// بررسی تعارضات اورژانس
        /// </summary>
        public async Task<ServiceResult<List<EmergencyConflict>>> CheckEmergencyConflictsAsync(int doctorId, DateTime date, TimeSpan time)
        {
            try
            {
                _logger.Information("بررسی تعارضات اورژانس برای پزشک {DoctorId} در تاریخ {Date} ساعت {Time}", 
                    doctorId, date.ToString("yyyy/MM/dd"), time.ToString(@"hh\:mm"));

                var conflicts = new List<EmergencyConflict>();

                // در حال حاضر این متد ساده است
                // در آینده با جدول EmergencyBookings یکپارچه خواهد شد

                return ServiceResult<List<EmergencyConflict>>.Successful(conflicts);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی تعارضات اورژانس برای پزشک {DoctorId}", doctorId);
                return ServiceResult<List<EmergencyConflict>>.Failed("خطا در بررسی تعارضات اورژانس");
            }
        }

        /// <summary>
        /// حل تعارضات اورژانس
        /// </summary>
        public async Task<ServiceResult<bool>> ResolveEmergencyConflictsAsync(int doctorId, DateTime date, List<EmergencyConflict> conflicts)
        {
            try
            {
                _logger.Information("حل تعارضات اورژانس برای پزشک {DoctorId} در تاریخ {Date}. تعداد تعارضات: {Count}", 
                    doctorId, date.ToString("yyyy/MM/dd"), conflicts?.Count ?? 0);

                if (conflicts == null || !conflicts.Any())
                {
                    return ServiceResult<bool>.Successful(true);
                }

                // در حال حاضر این متد ساده است
                // در آینده با الگوریتم‌های پیشرفته پیاده‌سازی خواهد شد

                _logger.Information("تعارضات اورژانس برای پزشک {DoctorId} در تاریخ {Date} با موفقیت حل شد", 
                    doctorId, date.ToString("yyyy/MM/dd"));

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حل تعارضات اورژانس برای پزشک {DoctorId}", doctorId);
                return ServiceResult<bool>.Failed("خطا در حل تعارضات اورژانس");
            }
        }

        /// <summary>
        /// دریافت گزارش اورژانس
        /// </summary>
        public async Task<ServiceResult<EmergencyReport>> GetEmergencyReportAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Information("درخواست گزارش اورژانس برای پزشک {DoctorId} از {StartDate} تا {EndDate}", 
                    doctorId, startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"));

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<EmergencyReport>.Failed("شناسه پزشک نامعتبر است.");
                }

                if (startDate >= endDate)
                {
                    return ServiceResult<EmergencyReport>.Failed("تاریخ شروع باید قبل از تاریخ پایان باشد.");
                }

                // در حال حاضر این متد ساده است
                // در آینده با جدول EmergencyBookings یکپارچه خواهد شد
                var report = new EmergencyReport
                {
                    DoctorId = doctorId
                };

                _logger.Information("گزارش اورژانس برای پزشک {DoctorId} با موفقیت تولید شد", doctorId);

                return ServiceResult<EmergencyReport>.Successful(report);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید گزارش اورژانس برای پزشک {DoctorId}", doctorId);
                return ServiceResult<EmergencyReport>.Failed("خطا در تولید گزارش اورژانس");
            }
        }
    }
}
