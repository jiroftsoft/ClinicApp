using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.CMS;
using ClinicApp.Services;
using Microsoft.AspNet.Identity;
using Serilog;
using System.ComponentModel;
using ClinicApp.Extensions;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت فرم تماس
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class ContactFormService : IContactFormService
    {
        private readonly IContactFormRepository _contactFormRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ContactFormService(
            IContactFormRepository contactFormRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _contactFormRepository = contactFormRepository ?? throw new ArgumentNullException(nameof(contactFormRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<PagedResult<ContactFormIndexViewModel>>> GetContactFormsAsync(ContactFormSearchViewModel searchModel)
        {
            try
            {
                if (searchModel == null)
                {
                    searchModel = new ContactFormSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var contactForms = await _contactFormRepository.SearchAsync(
                    searchModel.SearchTerm,
                    searchModel.Category,
                    searchModel.Status,
                    searchModel.IsRead,
                    includeDeleted: false);

                // فیلتر بر اساس تاریخ (در صورت وجود)
                if (searchModel.FromDate.HasValue)
                {
                    contactForms = contactForms.Where(c => c.CreatedAt >= searchModel.FromDate.Value).ToList();
                }

                if (searchModel.ToDate.HasValue)
                {
                    contactForms = contactForms.Where(c => c.CreatedAt <= searchModel.ToDate.Value).ToList();
                }

                var totalCount = contactForms.Count;
                var pagedItems = contactForms
                    .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(c => new ContactFormIndexViewModel
                    {
                        ContactFormId = c.ContactFormId,
                        FullName = c.FullName,
                        Email = c.Email,
                        PhoneNumber = c.PhoneNumber,
                        Subject = c.Subject,
                        Message = c.Message,
                        Category = c.Category,
                        CategoryDisplay = GetEnumDescription(c.Category),
                        Status = c.Status,
                        StatusDisplay = GetEnumDescription(c.Status),
                        IsRead = c.IsRead,
                        ReadAt = c.ReadAt,
                        RepliedAt = c.RepliedAt,
                        RepliedByUserName = c.RepliedByUser?.UserName,
                        CreatedAt = c.CreatedAt
                    })
                    .ToList();

                var pagedResult = new PagedResult<ContactFormIndexViewModel>(
                    pagedItems,
                    totalCount,
                    searchModel.PageNumber,
                    searchModel.PageSize);

                return ServiceResult<PagedResult<ContactFormIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست فرم‌های تماس");
                return ServiceResult<PagedResult<ContactFormIndexViewModel>>.Failed("خطا در دریافت لیست فرم‌های تماس");
            }
        }

        public async Task<ServiceResult<ContactFormDetailsViewModel>> GetContactFormDetailsAsync(int contactFormId)
        {
            try
            {
                var contactForm = await _contactFormRepository.GetByIdAsync(contactFormId);
                if (contactForm == null)
                {
                    return ServiceResult<ContactFormDetailsViewModel>.Failed("فرم تماس یافت نشد");
                }

                var viewModel = new ContactFormDetailsViewModel
                {
                    ContactFormId = contactForm.ContactFormId,
                    FullName = contactForm.FullName,
                    Email = contactForm.Email,
                    PhoneNumber = contactForm.PhoneNumber,
                    Subject = contactForm.Subject,
                    Message = contactForm.Message,
                    Category = contactForm.Category,
                    CategoryDisplay = GetEnumDescription(contactForm.Category),
                    Status = contactForm.Status,
                    StatusDisplay = GetEnumDescription(contactForm.Status),
                    ReplyMessage = contactForm.ReplyMessage,
                    RepliedAt = contactForm.RepliedAt,
                    RepliedByUserName = contactForm.RepliedByUser?.UserName,
                    IsRead = contactForm.IsRead,
                    ReadAt = contactForm.ReadAt,
                    ReadByUserName = contactForm.ReadByUser?.UserName,
                    IpAddress = contactForm.IpAddress,
                    UserAgent = contactForm.UserAgent,
                    CreatedAt = contactForm.CreatedAt,
                    CreatedByUserName = contactForm.CreatedByUser?.UserName ?? "سیستم"
                };

                return ServiceResult<ContactFormDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات فرم تماس - ContactFormId: {ContactFormId}", contactFormId);
                return ServiceResult<ContactFormDetailsViewModel>.Failed("خطا در دریافت جزئیات فرم تماس");
            }
        }

        public async Task<ServiceResult<ContactFormCreateEditViewModel>> GetContactFormForEditAsync(int contactFormId)
        {
            try
            {
                var contactForm = await _contactFormRepository.GetByIdAsync(contactFormId);
                if (contactForm == null)
                {
                    return ServiceResult<ContactFormCreateEditViewModel>.Failed("فرم تماس یافت نشد");
                }

                var viewModel = new ContactFormCreateEditViewModel
                {
                    ContactFormId = contactForm.ContactFormId,
                    FullName = contactForm.FullName,
                    Email = contactForm.Email,
                    PhoneNumber = contactForm.PhoneNumber,
                    Subject = contactForm.Subject,
                    Message = contactForm.Message,
                    Category = contactForm.Category,
                    Status = contactForm.Status,
                    ReplyMessage = contactForm.ReplyMessage,
                    IsRead = contactForm.IsRead
                };

                return ServiceResult<ContactFormCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت فرم تماس برای ویرایش - ContactFormId: {ContactFormId}", contactFormId);
                return ServiceResult<ContactFormCreateEditViewModel>.Failed("خطا در دریافت فرم تماس برای ویرایش");
            }
        }

        public async Task<ServiceResult<ContactForm>> CreateContactFormAsync(PublicContactFormViewModel model, string ipAddress = null, string userAgent = null)
        {
            try
            {
                _logger.Information("ایجاد فرم تماس جدید - FullName: {FullName}, Email: {Email}", model.FullName, model.Email);

                var contactForm = new ContactForm
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Subject = model.Subject,
                    Message = model.Message,
                    Category = model.Category,
                    Status = ContactFormStatus.New,
                    IsRead = false,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedByUserId = _currentUserService.UserId
                };

                _contactFormRepository.Add(contactForm);
                await _context.SaveChangesAsync();

                _logger.Information("فرم تماس با موفقیت ایجاد شد - ContactFormId: {ContactFormId}", contactForm.ContactFormId);

                // ارسال SMS تایید (اگر شماره موبایل وارد شده باشد) - Fire and Forget
                if (!string.IsNullOrWhiteSpace(contactForm.PhoneNumber))
                {
                    try
                    {
                        var trackingId = $"CF-{contactForm.ContactFormId:D6}";
                        
                        // Fire and Forget - خطا در ارسال SMS نباید فرآیند را متوقف کند
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var smsService = new AsanakSmsService();
                                var smsMessage = new IdentityMessage
                                {
                                    Destination = contactForm.PhoneNumber,
                                    Body = $"✅ پیام شما با موفقیت دریافت شد\n" +
                                           $"📋 شماره پیگیری: {trackingId}\n" +
                                           $"📧 ایمیل: {contactForm.Email}\n" +
                                           $"📝 موضوع: {contactForm.Subject}\n" +
                                           $"⏰ زمان تقریبی پاسخ: در ساعات کاری (شنبه تا پنجشنبه: 8:00 - 20:00)\n" +
                                           $"🏥 کلینیک درمانی شفا"
                                };

                                await smsService.SendAsync(smsMessage);
                                _logger.Information("SMS تایید فرم تماس ارسال شد - ContactFormId: {ContactFormId}, Phone: {Phone}, TrackingId: {TrackingId}",
                                    contactForm.ContactFormId, contactForm.PhoneNumber, trackingId);
                            }
                            catch (Exception smsEx)
                            {
                                _logger.Error(smsEx, "خطا در ارسال SMS تایید فرم تماس - ContactFormId: {ContactFormId}, Phone: {Phone}",
                                    contactForm.ContactFormId, contactForm.PhoneNumber);
                                // خطا را لاگ می‌کنیم اما exception را throw نمی‌کنیم
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "خطا در ایجاد Task ارسال SMS - ContactFormId: {ContactFormId}",
                            contactForm.ContactFormId);
                        // خطا را لاگ می‌کنیم اما فرآیند را متوقف نمی‌کنیم
                    }
                }

                return ServiceResult<ContactForm>.Successful(contactForm, "فرم تماس با موفقیت ارسال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد فرم تماس");
                return ServiceResult<ContactForm>.Failed("خطا در ارسال فرم تماس");
            }
        }

        public async Task<ServiceResult<ContactForm>> UpdateContactFormAsync(ContactFormCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی فرم تماس - ContactFormId: {ContactFormId}", model.ContactFormId);

                var contactForm = await _contactFormRepository.GetByIdAsync(model.ContactFormId);
                if (contactForm == null)
                {
                    return ServiceResult<ContactForm>.Failed("فرم تماس یافت نشد");
                }

                contactForm.FullName = model.FullName;
                contactForm.Email = model.Email;
                contactForm.PhoneNumber = model.PhoneNumber;
                contactForm.Subject = model.Subject;
                contactForm.Message = model.Message;
                contactForm.Category = model.Category;
                contactForm.Status = model.Status;
                contactForm.ReplyMessage = model.ReplyMessage;
                contactForm.IsRead = model.IsRead;
                contactForm.UpdatedByUserId = _currentUserService.UserId;
                contactForm.UpdatedAt = DateTime.Now;

                _contactFormRepository.Update(contactForm);
                await _context.SaveChangesAsync();

                _logger.Information("فرم تماس با موفقیت به‌روزرسانی شد - ContactFormId: {ContactFormId}", contactForm.ContactFormId);
                return ServiceResult<ContactForm>.Successful(contactForm, "فرم تماس با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی فرم تماس - ContactFormId: {ContactFormId}", model.ContactFormId);
                return ServiceResult<ContactForm>.Failed("خطا در به‌روزرسانی فرم تماس");
            }
        }

        public async Task<ServiceResult> DeleteContactFormAsync(int contactFormId)
        {
            try
            {
                _logger.Information("حذف فرم تماس - ContactFormId: {ContactFormId}", contactFormId);

                var contactForm = await _contactFormRepository.GetByIdAsync(contactFormId);
                if (contactForm == null)
                {
                    return ServiceResult.Failed("فرم تماس یافت نشد");
                }

                _contactFormRepository.Delete(contactForm);
                contactForm.DeletedByUserId = _currentUserService.UserId;
                await _context.SaveChangesAsync();

                _logger.Information("فرم تماس با موفقیت حذف شد - ContactFormId: {ContactFormId}", contactFormId);
                return ServiceResult.Successful("فرم تماس با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف فرم تماس - ContactFormId: {ContactFormId}", contactFormId);
                return ServiceResult.Failed("خطا در حذف فرم تماس");
            }
        }

        public async Task<ServiceResult> ReplyToContactFormAsync(ContactFormReplyViewModel model, string userId)
        {
            try
            {
                _logger.Information("پاسخ به فرم تماس - ContactFormId: {ContactFormId}", model.ContactFormId);

                var contactForm = await _contactFormRepository.GetByIdAsync(model.ContactFormId);
                if (contactForm == null)
                {
                    return ServiceResult.Failed("فرم تماس یافت نشد");
                }

                contactForm.ReplyMessage = model.ReplyMessage;
                contactForm.Status = ContactFormStatus.Replied;
                contactForm.RepliedAt = DateTime.Now;
                contactForm.RepliedByUserId = userId;
                contactForm.UpdatedByUserId = userId;
                contactForm.UpdatedAt = DateTime.Now;

                _contactFormRepository.Update(contactForm);
                await _context.SaveChangesAsync();

                _logger.Information("پاسخ با موفقیت ثبت شد - ContactFormId: {ContactFormId}", model.ContactFormId);

                // ارسال SMS اطلاع‌رسانی پاسخ (اگر شماره موبایل وارد شده باشد) - Fire and Forget
                if (!string.IsNullOrWhiteSpace(contactForm.PhoneNumber))
                {
                    try
                    {
                        var trackingId = $"CF-{contactForm.ContactFormId:D6}";
                        
                        // Fire and Forget - خطا در ارسال SMS نباید فرآیند را متوقف کند
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var smsService = new AsanakSmsService();
                                // محتوای SMS گرم و حرفه‌ای برای اطلاع‌رسانی پاسخ
                                var smsMessage = new IdentityMessage
                                {
                                    Destination = contactForm.PhoneNumber,
                                    Body = $"✅ پاسخ شما آماده است\n" +
                                           $"📋 شماره پیگیری: {trackingId}\n" +
                                           $"👤 عزیز {contactForm.FullName}\n" +
                                           $"📝 موضوع: {contactForm.Subject}\n" +
                                           $"💬 پاسخ شما آماده است. در صورت نیاز از طریق ایمیل یا تماس تلفنی با شما ارتباط برقرار خواهیم کرد.\n" +
                                           $"🙏 از صبر و اعتماد شما به کلینیک شفا متشکریم\n" +
                                           $"🏥 کلینیک درمانی شفا"
                                };

                                await smsService.SendAsync(smsMessage);
                                _logger.Information("SMS اطلاع‌رسانی پاسخ فرم تماس ارسال شد - ContactFormId: {ContactFormId}, Phone: {Phone}, TrackingId: {TrackingId}",
                                    contactForm.ContactFormId, contactForm.PhoneNumber, trackingId);
                            }
                            catch (Exception smsEx)
                            {
                                _logger.Error(smsEx, "خطا در ارسال SMS اطلاع‌رسانی پاسخ فرم تماس - ContactFormId: {ContactFormId}, Phone: {Phone}",
                                    contactForm.ContactFormId, contactForm.PhoneNumber);
                                // خطا را لاگ می‌کنیم اما exception را throw نمی‌کنیم
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "خطا در ایجاد Task ارسال SMS اطلاع‌رسانی پاسخ - ContactFormId: {ContactFormId}",
                            contactForm.ContactFormId);
                        // خطا را لاگ می‌کنیم اما فرآیند را متوقف نمی‌کنیم
                    }
                }

                // TODO: ارسال ایمیل در صورت انتخاب کاربر
                // if (model.SendEmail) { ... }

                return ServiceResult.Successful("پاسخ با موفقیت ثبت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ثبت پاسخ - ContactFormId: {ContactFormId}", model.ContactFormId);
                return ServiceResult.Failed("خطا در ثبت پاسخ");
            }
        }

        public async Task<ServiceResult> MarkAsReadAsync(int contactFormId, string userId)
        {
            try
            {
                var contactForm = await _contactFormRepository.GetByIdAsync(contactFormId);
                if (contactForm == null)
                {
                    return ServiceResult.Failed("فرم تماس یافت نشد");
                }

                if (!contactForm.IsRead)
                {
                    contactForm.IsRead = true;
                    contactForm.ReadAt = DateTime.Now;
                    contactForm.ReadByUserId = userId;
                    contactForm.UpdatedByUserId = userId;
                    contactForm.UpdatedAt = DateTime.Now;

                    _contactFormRepository.Update(contactForm);
                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Successful("فرم تماس به عنوان خوانده شده علامت‌گذاری شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در علامت‌گذاری به عنوان خوانده شده - ContactFormId: {ContactFormId}", contactFormId);
                return ServiceResult.Failed("خطا در علامت‌گذاری به عنوان خوانده شده");
            }
        }

        public async Task<ServiceResult> MarkAsUnreadAsync(int contactFormId)
        {
            try
            {
                var contactForm = await _contactFormRepository.GetByIdAsync(contactFormId);
                if (contactForm == null)
                {
                    return ServiceResult.Failed("فرم تماس یافت نشد");
                }

                if (contactForm.IsRead)
                {
                    contactForm.IsRead = false;
                    contactForm.ReadAt = null;
                    contactForm.ReadByUserId = null;
                    contactForm.UpdatedByUserId = _currentUserService.UserId;
                    contactForm.UpdatedAt = DateTime.Now;

                    _contactFormRepository.Update(contactForm);
                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Successful("فرم تماس به عنوان خوانده نشده علامت‌گذاری شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در علامت‌گذاری به عنوان خوانده نشده - ContactFormId: {ContactFormId}", contactFormId);
                return ServiceResult.Failed("خطا در علامت‌گذاری به عنوان خوانده نشده");
            }
        }

        public async Task<ServiceResult> ChangeStatusAsync(int contactFormId, ContactFormStatus status)
        {
            try
            {
                var contactForm = await _contactFormRepository.GetByIdAsync(contactFormId);
                if (contactForm == null)
                {
                    return ServiceResult.Failed("فرم تماس یافت نشد");
                }

                contactForm.Status = status;
                contactForm.UpdatedByUserId = _currentUserService.UserId;
                contactForm.UpdatedAt = DateTime.Now;

                _contactFormRepository.Update(contactForm);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("وضعیت با موفقیت تغییر کرد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تغییر وضعیت - ContactFormId: {ContactFormId}", contactFormId);
                return ServiceResult.Failed("خطا در تغییر وضعیت");
            }
        }

        public async Task<ServiceResult<int>> GetUnreadCountAsync()
        {
            try
            {
                var count = await _contactFormRepository.GetUnreadCountAsync();
                return ServiceResult<int>.Successful(count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد خوانده نشده");
                return ServiceResult<int>.Failed("خطا در دریافت تعداد خوانده نشده");
            }
        }

        public async Task<ServiceResult<int>> GetNewCountAsync()
        {
            try
            {
                var contactForms = await _contactFormRepository.GetByStatusAsync(ContactFormStatus.New);
                return ServiceResult<int>.Successful(contactForms.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد جدید");
                return ServiceResult<int>.Failed("خطا در دریافت تعداد جدید");
            }
        }

        public async Task<ServiceResult<int>> GetInProgressCountAsync()
        {
            try
            {
                var contactForms = await _contactFormRepository.GetByStatusAsync(ContactFormStatus.InProgress);
                return ServiceResult<int>.Successful(contactForms.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد در حال بررسی");
                return ServiceResult<int>.Failed("خطا در دریافت تعداد در حال بررسی");
            }
        }

        public async Task<ServiceResult<int>> GetRepliedCountAsync()
        {
            try
            {
                var contactForms = await _contactFormRepository.GetByStatusAsync(ContactFormStatus.Replied);
                return ServiceResult<int>.Successful(contactForms.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد پاسخ داده شده");
                return ServiceResult<int>.Failed("خطا در دریافت تعداد پاسخ داده شده");
            }
        }

        public async Task<ServiceResult<ContactFormTrackingViewModel>> GetContactFormByTrackingIdAsync(string trackingId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(trackingId))
                {
                    return ServiceResult<ContactFormTrackingViewModel>.Failed("شماره پیگیری وارد نشده است");
                }

                // استخراج ContactFormId از Tracking ID (فرمت: CF-XXXXXX)
                int contactFormId;
                if (trackingId.StartsWith("CF-", StringComparison.OrdinalIgnoreCase))
                {
                    var idPart = trackingId.Substring(3);
                    if (!int.TryParse(idPart, out contactFormId))
                    {
                        return ServiceResult<ContactFormTrackingViewModel>.Failed("شماره پیگیری نامعتبر است");
                    }
                }
                else
                {
                    // اگر فقط عدد وارد شده باشد
                    if (!int.TryParse(trackingId, out contactFormId))
                    {
                        return ServiceResult<ContactFormTrackingViewModel>.Failed("شماره پیگیری نامعتبر است");
                    }
                }

                var contactForm = await _contactFormRepository.GetByIdAsync(contactFormId);
                if (contactForm == null || contactForm.IsDeleted)
                {
                    return ServiceResult<ContactFormTrackingViewModel>.Failed("پیامی با این شماره پیگیری یافت نشد");
                }

                var viewModel = new ContactFormTrackingViewModel
                {
                    ContactFormId = contactForm.ContactFormId,
                    TrackingId = $"CF-{contactForm.ContactFormId:D6}",
                    FullName = contactForm.FullName,
                    Email = contactForm.Email,
                    Subject = contactForm.Subject,
                    Category = contactForm.Category,
                    CategoryDisplay = GetEnumDescription(contactForm.Category),
                    Status = contactForm.Status,
                    StatusDisplay = GetEnumDescription(contactForm.Status),
                    CreatedAt = contactForm.CreatedAt,
                    CreatedAtPersian = contactForm.CreatedAt.ToPersianDate(),
                    IsRead = contactForm.IsRead,
                    ReadAt = contactForm.ReadAt,
                    HasReply = !string.IsNullOrWhiteSpace(contactForm.ReplyMessage),
                    RepliedAt = contactForm.RepliedAt,
                    RepliedAtPersian = contactForm.RepliedAt?.ToPersianDate()
                };

                return ServiceResult<ContactFormTrackingViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی فرم تماس با Tracking ID - TrackingId: {TrackingId}", trackingId);
                return ServiceResult<ContactFormTrackingViewModel>.Failed("خطا در جستجوی پیام");
            }
        }

        #region Helper Methods

        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
            return attribute?.Description ?? value.ToString();
        }

        #endregion
    }
}

