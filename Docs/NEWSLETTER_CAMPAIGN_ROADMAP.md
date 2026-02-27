# نقشه راه و TODO – ماژول NewsletterCampaign

## هدف
تبدیل ماژول به سطح Production و بیمارستانی با امنیت، یکپارچگی داده، کارایی و UX بهینه.

---

## گام ۱ – امنیت (Security)
| # | تسک | وضعیت | فایل(ها) |
|---|-----|--------|----------|
| 1.1 | فعال‌سازی `[Authorize(Roles = AppRoles.Admin)]` روی کنترلر | ✅ | NewsletterCampaignController.cs |
| 1.2 | رفع Open Redirect در TrackClick (فقط همان دامنه/relative) | ✅ | NewsletterController.cs + IsSafeRedirectUrl |
| 1.3 | Parse امن زمان (TryParse) در Create/Edit/Send | ✅ | ControllerExtensions.ParseDateAndTimeFromForm + NewsletterCampaignController |

## گام ۲ – یکپارچگی داده (Data Integrity)
| # | تسک | وضعیت | فایل(ها) |
|---|-----|--------|----------|
| 2.1 | ست کردن DeletedByUserId قبل از Delete و SaveChanges | ✅ | NewsletterCampaignService.cs |
| 2.2 | پس از ارسال واقعی: به‌روزرسانی وضعیت به Sent/Failed و SentCount | ✅ | Hangfire Job + ProcessCampaignSendQueueAsync |

## گام ۳ – همزمانی (Concurrency)
| # | تسک | وضعیت | فایل(ها) |
|---|-----|--------|----------|
| 3.1 | قفل ارسال: بلافاصله وضعیت Sending + SaveChanges | ✅ | NewsletterCampaignService.SendCampaignAsync |
| 3.2 | افزایش اتمیک OpenedCount/ClickedCount (کوئری اتمیک) | ✅ | NewsletterCampaignRepository + TrackEmailOpen/Click |

## گام ۴ – استثنا و Null (Exception Safety)
| # | تسک | وضعیت | فایل(ها) |
|---|-----|--------|----------|
| 4.1 | استفاده از TryParse برای زمان در کنترلر | ✅ | ParseDateAndTimeFromForm |
| 4.2 | در catch از model استفاده نشود اگر null است | ✅ | NewsletterCampaignController Create/Edit/Send |

## گام ۵ – کارایی (Performance)
| # | تسک | وضعیت | فایل(ها) |
|---|-----|--------|----------|
| 5.1 | صفحه‌بندی در DB: SearchPagedAsync در Repository | ✅ | INewsletterCampaignRepository + NewsletterCampaignRepository |
| 5.2 | سرویس از SearchPagedAsync استفاده کند | ✅ | NewsletterCampaignService.GetCampaignsAsync |
| 5.3 | GetByCategoriesAsync با فیلتر در SQL | ⬜ | اختیاری؛ با حجم بالای مشترک می‌توان به SQL منتقل کرد |

## گام ۶ – لاگ و UX
| # | تسک | وضعیت | فایل(ها) |
|---|-----|--------|----------|
| 6.1 | لاگ audit قبل از Send/Schedule | ✅ | NewsletterCampaignService |
| 6.2 | تأیید ارسال با نمایش تعداد گیرنده در Send | ✅ | Send.cshtml + Swal |
| 6.3 | برچسب تأیید (ارسال فوری / زمان‌بندی) در دیالوگ | ✅ | Send.cshtml |

---

## اولویت اجرا
1. گام ۱ (امنیت)  
2. گام ۴ (Exception safety)  
3. گام ۲ و ۳ (Data + Concurrency)  
4. گام ۵ (Performance)  
5. گام ۶ (Logging + UX)
