using System;

namespace ClinicApp.DataSeeding
{
    /// <summary>
    /// تمام مقادیر ثابت برای Data Seeding
    /// این کلاس تمام Hard-coded Values را مرکزی می‌کند
    /// </summary>
    public static class SeedConstants
    {
        #region کاربران سیستمی (System Users)

        /// <summary>
        /// نام کاربری (کد ملی) کاربر ادمین
        /// </summary>
        public const string AdminUserName = "3020347998";

        /// <summary>
        /// ایمیل کاربر ادمین
        /// </summary>
        public const string AdminEmail = "admin@clinic.com";

        /// <summary>
        /// شماره تلفن کاربر ادمین
        /// </summary>
        public const string AdminPhoneNumber = "09136381995";

        /// <summary>
        /// نام کاربری (کد ملی) کاربر سیستم
        /// </summary>
        public const string SystemUserName = "3031945451";

        /// <summary>
        /// ایمیل کاربر سیستم
        /// </summary>
        public const string SystemEmail = "system@clinic.com";

        /// <summary>
        /// شماره تلفن کاربر سیستم
        /// </summary>
        public const string SystemPhoneNumber = "09022487373";

        #endregion

        #region ارائه‌دهندگان بیمه (Insurance Providers)

        public static class InsuranceProviders
        {
            // ═══════════════════════════════════════════════════════════
            // بیمه آزاد (No Insurance)
            // ═══════════════════════════════════════════════════════════
            public const string FreeCode = "FREE";
            public const string FreeName = "بیمه آزاد (پرداخت نقدی)";
            public const string FreeContactInfo = "پرداخت کامل توسط بیمار - بدون پوشش بیمه‌ای";

            // ═══════════════════════════════════════════════════════════
            // بیمه‌های پایه (Primary Insurance - 70% بیمه، 30% بیمار)
            // ═══════════════════════════════════════════════════════════
            
            // تامین اجتماعی
            public const string SSOCode = "SSO";
            public const string SSOName = "سازمان تأمین اجتماعی";
            public const string SSOContactInfo = "تلفن: ۱۴۲۰، وب‌سایت: www.tamin.ir، آدرس: تهران، خیابان آزادی";

            // بیمه سلامت
            public const string HealthCode = "SALAMAT";
            public const string HealthName = "بیمه سلامت ایرانیان";
            public const string HealthContactInfo = "تلفن: ۱۵۳۰، وب‌سایت: www.salamat.gov.ir، آدرس: تهران، خیابان ولیعصر";

            // نیروهای مسلح
            public const string MilitaryCode = "MILITARY";
            public const string MilitaryName = "بیمه نیروهای مسلح";
            public const string MilitaryContactInfo = "تلفن: 021-66754000، وب‌سایت: www.bimesaipa.com، آدرس: تهران، خیابان انقلاب";

            // خدمات درمانی
            public const string MedicalServicesCode = "KHADAMAT";
            public const string MedicalServicesName = "بیمه خدمات درمانی";
            public const string MedicalServicesContactInfo = "تلفن: 021-88888888، وب‌سایت: www.ihio.gov.ir، آدرس: تهران";

            // ═══════════════════════════════════════════════════════════
            // بانک‌ها (Bank Insurance - 70% بیمه، 30% بیمار)
            // ═══════════════════════════════════════════════════════════
            
            // بانک ملی
            public const string BankMelliCode = "BANK_MELLI";
            public const string BankMelliName = "بیمه بانک ملی";
            public const string BankMelliContactInfo = "تلفن: 021-66770077، وب‌سایت: www.bmi.ir، آدرس: تهران، میدان فردوسی";

            // بانک صادرات
            public const string BankSaderatCode = "BANK_SADERAT";
            public const string BankSaderatName = "بیمه بانک صادرات";
            public const string BankSaderatContactInfo = "تلفن: 021-39903490، وب‌سایت: www.bsi.ir، آدرس: تهران، میدان ونک";

            // بانک سپه
            public const string BankSepahCode = "BANK_SEPAH";
            public const string BankSepahName = "بیمه بانک سپه";
            public const string BankSepahContactInfo = "تلفن: 021-88760909، وب‌سایت: www.banksepah.ir، آدرس: تهران، خیابان جمهوری";

            // ═══════════════════════════════════════════════════════════
            // بیمه‌های تکمیلی (Supplementary Insurance - 100% پوشش)
            // ═══════════════════════════════════════════════════════════
            
            // دانا
            public const string DanaCode = "DANA";
            public const string DanaName = "بیمه تکمیلی دانا";
            public const string DanaContactInfo = "تلفن: 021-42150000، وب‌سایت: www.dana-insurance.com، آدرس: تهران، خیابان ولیعصر";

            // بیمه ما
            public const string BimeMaCode = "BIME_MA";
            public const string BimeMaName = "بیمه تکمیلی بیمه ما";
            public const string BimeMaContactInfo = "تلفن: 021-88828282، وب‌سایت: www.bimema.ir، آدرس: تهران، خیابان آزادی";

            // بیمه دی
            public const string BimeDeyCode = "BIME_DEY";
            public const string BimeDeyName = "بیمه تکمیلی دی";
            public const string BimeDeyContactInfo = "تلفن: 021-88847070، وب‌سایت: www.dey-insurance.com، آدرس: تهران، میدان آرژانتین";

            // بیمه البرز
            public const string BimeAlborzCode = "BIME_ALBORZ";
            public const string BimeAlborzName = "بیمه تکمیلی البرز";
            public const string BimeAlborzContactInfo = "تلفن: 021-88401070، وب‌سایت: www.alborz-ins.ir، آدرس: تهران، خیابان استاد معین";

            // بیمه پاسارگاد
            public const string BimePasargadCode = "BIME_PASARGAD";
            public const string BimePasargadName = "بیمه تکمیلی پاسارگاد";
            public const string BimePasargadContactInfo = "تلفن: 021-84902000، وب‌سایت: www.bimepasargad.com، آدرس: تهران، میدان ونک";

            // بیمه آسیا
            public const string BimeAsiaCode = "BIME_ASIA";
            public const string BimeAsiaName = "بیمه تکمیلی آسیا";
            public const string BimeAsiaContactInfo = "تلفن: 021-88722200، وب‌سایت: www.asia-insurance.com، آدرس: تهران، میدان فاطمی";
        }

        #endregion

        #region طرح‌های بیمه (Insurance Plans)

        public static class InsurancePlans
        {
            // ═══════════════════════════════════════════════════════════
            // بیمه آزاد (0% بیمه، 100% بیمار)
            // ═══════════════════════════════════════════════════════════
            public const string FreeBasicCode = "FREE_BASIC";
            public const string FreeBasicName = "بیمه آزاد - پرداخت نقدی";
            public const int FreeBasicCoveragePercent = 0;
            public const decimal FreeBasicDeductible = 0m;

            // ═══════════════════════════════════════════════════════════
            // بیمه‌های پایه (70% بیمه، 30% بیمار)
            // ═══════════════════════════════════════════════════════════
            
            // تامین اجتماعی
            public const string SSOBasicCode = "SSO_BASIC";
            public const string SSOBasicName = "تأمین اجتماعی - طرح پایه";
            public const int SSOBasicCoveragePercent = 70;
            public const decimal SSOBasicDeductible = 0m;

            // بیمه سلامت
            public const string HealthBasicCode = "SALAMAT_BASIC";
            public const string HealthBasicName = "بیمه سلامت - طرح پایه";
            public const int HealthBasicCoveragePercent = 70;
            public const decimal HealthBasicDeductible = 0m;

            // نیروهای مسلح
            public const string MilitaryBasicCode = "MILITARY_BASIC";
            public const string MilitaryBasicName = "نیروهای مسلح - طرح پایه";
            public const int MilitaryBasicCoveragePercent = 70;
            public const decimal MilitaryBasicDeductible = 0m;

            // خدمات درمانی
            public const string MedicalServicesBasicCode = "KHADAMAT_BASIC";
            public const string MedicalServicesBasicName = "خدمات درمانی - طرح پایه";
            public const int MedicalServicesBasicCoveragePercent = 70;
            public const decimal MedicalServicesBasicDeductible = 0m;

            // ═══════════════════════════════════════════════════════════
            // بانک‌ها (70% بیمه، 30% بیمار)
            // ═══════════════════════════════════════════════════════════
            
            // بانک ملی
            public const string BankMelliBasicCode = "BANK_MELLI_BASIC";
            public const string BankMelliBasicName = "بیمه بانک ملی - طرح پایه";
            public const int BankMelliBasicCoveragePercent = 70;
            public const decimal BankMelliBasicDeductible = 0m;

            // بانک صادرات
            public const string BankSaderatBasicCode = "BANK_SADERAT_BASIC";
            public const string BankSaderatBasicName = "بیمه بانک صادرات - طرح پایه";
            public const int BankSaderatBasicCoveragePercent = 70;
            public const decimal BankSaderatBasicDeductible = 0m;

            // بانک سپه
            public const string BankSepahBasicCode = "BANK_SEPAH_BASIC";
            public const string BankSepahBasicName = "بیمه بانک سپه - طرح پایه";
            public const int BankSepahBasicCoveragePercent = 70;
            public const decimal BankSepahBasicDeductible = 0m;

            // ═══════════════════════════════════════════════════════════
            // بیمه‌های تکمیلی (100% پوشش بعد از بیمه پایه)
            // ═══════════════════════════════════════════════════════════
            
            // دانا
            public const string DanaSupplementaryCode = "DANA_SUPPLEMENTARY";
            public const string DanaSupplementaryName = "بیمه تکمیلی دانا - پوشش کامل";
            public const int DanaSupplementaryCoveragePercent = 100;
            public const decimal DanaSupplementaryDeductible = 0m;

            // بیمه ما
            public const string BimeMaSupplementaryCode = "BIME_MA_SUPPLEMENTARY";
            public const string BimeMaSupplementaryName = "بیمه تکمیلی بیمه ما - پوشش کامل";
            public const int BimeMaSupplementaryCoveragePercent = 100;
            public const decimal BimeMaSupplementaryDeductible = 0m;

            // بیمه دی
            public const string BimeDeySupplementaryCode = "BIME_DEY_SUPPLEMENTARY";
            public const string BimeDeySupplementaryName = "بیمه تکمیلی دی - پوشش کامل";
            public const int BimeDeySupplementaryCoveragePercent = 100;
            public const decimal BimeDeySupplementaryDeductible = 0m;

            // بیمه البرز
            public const string BimeAlborzSupplementaryCode = "BIME_ALBORZ_SUPPLEMENTARY";
            public const string BimeAlborzSupplementaryName = "بیمه تکمیلی البرز - پوشش کامل";
            public const int BimeAlborzSupplementaryCoveragePercent = 100;
            public const decimal BimeAlborzSupplementaryDeductible = 0m;

            // بیمه پاسارگاد
            public const string BimePasargadSupplementaryCode = "BIME_PASARGAD_SUPPLEMENTARY";
            public const string BimePasargadSupplementaryName = "بیمه تکمیلی پاسارگاد - پوشش کامل";
            public const int BimePasargadSupplementaryCoveragePercent = 100;
            public const decimal BimePasargadSupplementaryDeductible = 0m;

            // بیمه آسیا
            public const string BimeAsiaSupplementaryCode = "BIME_ASIA_SUPPLEMENTARY";
            public const string BimeAsiaSupplementaryName = "بیمه تکمیلی آسیا - پوشش کامل";
            public const int BimeAsiaSupplementaryCoveragePercent = 100;
            public const decimal BimeAsiaSupplementaryDeductible = 0m;
        }

        #endregion

        #region تنظیمات پوشش بیمه (Coverage Settings)

        public static class CoverageSettings
        {
            // ═══════════════════════════════════════════════════════════
            // سهم بیمار (Patient Share Percentage)
            // ═══════════════════════════════════════════════════════════
            
            // بیمه آزاد
            public const int FreePatientShare = 100;  // 100% بیمار
            public const int FreeInsuranceShare = 0;   // 0% بیمه
            
            // بیمه‌های پایه (طبق قانون)
            public const int PrimaryPatientShare = 30;     // 30% بیمار
            public const int PrimaryInsuranceShare = 70;   // 70% بیمه
            
            // بیمه‌های تکمیلی (پوشش کامل سهم بیمار)
            public const int SupplementaryPatientShare = 0;     // 0% بیمار بعد از بیمه پایه
            public const int SupplementaryInsuranceShare = 100; // 100% پوشش سهم بیمار
        }

        #endregion

        #region کلینیک پیش‌فرض (Default Clinic)

        public static class DefaultClinic
        {
            public const string Name = "کلینیک شفا";
            public const string Address = "جیرفت، خیابان آزادی، کوچه 12";
            public const string PhoneNumber = "034-12345678";
        }

        #endregion

        #region دپارتمان‌ها (Departments)

        public static class Departments
        {
            public static readonly (string Name, string Description)[] DefaultDepartments =
            {
                // دپارتمان‌های اورژانسی و حیاتی
                ("اورژانس", "بخش اورژانس - ارائه خدمات فوری و حیاتی"),
                ("تزریقات", "بخش تزریقات - انجام انواع تزریقات و سرم‌درمانی"),
                ("ICU", "بخش مراقبت‌های ویژه - مراقبت از بیماران بدحال"),
                
                // دپارتمان‌های تشخیصی
                ("آزمایشگاه", "بخش آزمایشگاه - انجام آزمایشات تشخیصی"),
                ("رادیولوژی", "بخش رادیولوژی - عکس‌برداری و تصویربرداری پزشکی"),
                ("سونوگرافی", "بخش سونوگرافی - سونوگرافی و تصویربرداری اولتراسوند"),
                ("اکوکاردیوگرافی", "بخش اکوی قلب - سونوگرافی تخصصی قلب"),
                
                // دپارتمان‌های تخصصی پزشکی
                ("داخلی", "بخش داخلی - تشخیص و درمان بیماری‌های داخلی"),
                ("قلب و عروق", "بخش قلب - تشخیص و درمان بیماری‌های قلبی"),
                ("گوارش و کبد", "بخش گوارش - تشخیص و درمان بیماری‌های دستگاه گوارش"),
                ("ریه و تنفس", "بخش ریه - تشخیص و درمان بیماری‌های تنفسی"),
                ("کلیه و دیالیز", "بخش کلیه - تشخیص و درمان بیماری‌های کلیوی و دیالیز"),
                ("غدد و دیابت", "بخش غدد - تشخیص و درمان دیابت و بیماری‌های غدد"),
                ("عفونی", "بخش عفونی - تشخیص و درمان بیماری‌های عفونی"),
                ("روانپزشکی", "بخش روانپزشکی - تشخیص و درمان اختلالات روانی"),
                
                // دپارتمان‌های جراحی
                ("جراحی عمومی", "بخش جراحی عمومی - انجام اعمال جراحی عمومی"),
                ("اتاق عمل", "بخش اتاق عمل - انجام جراحی‌ها و اعمال"),
                ("ریکاوری", "بخش ریکاوری - مراقبت بعد از عمل"),
                
                // دپارتمان‌های زنان و کودکان
                ("زنان و زایمان", "بخش زنان - ارائه خدمات زنان و زایمان"),
                ("زایشگاه", "بخش زایشگاه - زایمان طبیعی و سزارین"),
                ("نازایی", "بخش نازایی - درمان ناباروری"),
                ("کودکان", "بخش کودکان - مراقبت‌های پزشکی کودکان"),
                ("NICU", "بخش مراقبت‌های ویژه نوزادان - مراقبت از نوزادان نارس و بدحال"),
                
                // دپارتمان‌های تخصصی دیگر
                ("ارتوپدی", "بخش ارتوپدی - تشخیص و درمان بیماری‌های استخوان"),
                ("فیزیوتراپی", "بخش فیزیوتراپی - درمان فیزیکی و توانبخشی"),
                ("چشم پزشکی", "بخش چشم - تشخیص و درمان بیماری‌های چشم"),
                ("گوش و حلق و بینی", "بخش ENT - تشخیص و درمان بیماری‌های گوش و حلق و بینی"),
                ("پوست و مو", "بخش پوست - تشخیص و درمان بیماری‌های پوستی"),
                ("لیزر درمانی", "بخش لیزر - درمان‌های لیزری پوست و مو"),
                
                // دپارتمان‌های دندانپزشکی
                ("دندانپزشکی", "بخش دندانپزشکی - ارائه خدمات دندانپزشکی عمومی"),
                ("جراحی دهان و فک", "بخش جراحی دهان - جراحی‌های دهان و فک و صورت"),
                ("ارتودنسی", "بخش ارتودنسی - اصلاح ناهنجاری‌های دندان"),
                ("ایمپلنت", "بخش ایمپلنت - کاشت ایمپلنت دندان"),
                
                // دپارتمان‌های پشتیبانی
                ("پذیرش", "بخش پذیرش - ثبت‌نام و پذیرش بیماران"),
                ("داروخانه", "بخش داروخانه - ارائه دارو و مشاوره دارویی"),
                ("بستری", "بخش بستری - بستری بیماران"),
                ("ترخیص", "بخش ترخیص - ترخیص و تسویه حساب بیماران"),
                ("مددکاری", "بخش مددکاری اجتماعی - مشاوره و کمک به بیماران"),
                
                // دپارتمان‌های مدیریتی
                ("مدیریت", "بخش مدیریت - مدیریت و اداره کلینیک"),
                ("حسابداری", "بخش حسابداری - امور مالی و حسابداری")
            };
        }

        #endregion

        #region تخصص‌ها (Specializations)

        public static class Specializations
        {
            public static readonly (string Name, string Description, int DisplayOrder)[] DefaultSpecializations =
            {
                // پزشک عمومی و خانواده
                ("پزشکی عمومی", "پزشک عمومی - مراقبت‌های اولیه و عمومی", 1),
                ("پزشکی خانواده", "پزشک خانواده - مراقبت‌های اولیه و جامع خانواده", 2),
                
                // تخصص‌های داخلی
                ("داخلی", "متخصص داخلی - تشخیص و درمان بیماری‌های داخلی", 3),
                ("قلب و عروق", "متخصص قلب و عروق - تشخیص و درمان بیماری‌های قلبی و عروقی", 4),
                ("گوارش و کبد", "متخصص گوارش و کبد - تشخیص و درمان بیماری‌های دستگاه گوارش", 5),
                ("ریه و تنفس", "متخصص ریه - تشخیص و درمان بیماری‌های تنفسی", 6),
                ("کلیه و مجاری ادراری", "متخصص کلیه - تشخیص و درمان بیماری‌های کلیوی", 7),
                ("غدد و متابولیسم", "متخصص غدد - تشخیص و درمان بیماری‌های غدد و متابولیسم", 8),
                ("خون و سرطان", "متخصص خون و سرطان - تشخیص و درمان بیماری‌های خونی و سرطان", 9),
                ("روانپزشکی", "روانپزشک - تشخیص و درمان اختلالات روانی", 10),
                
                // تخصص‌های جراحی
                ("جراحی عمومی", "جراح عمومی - انجام اعمال جراحی عمومی", 11),
                ("جراحی قلب و عروق", "جراح قلب - انجام جراحی‌های قلب و عروق", 12),
                ("جراحی مغز و اعصاب", "جراح مغز و اعصاب - انجام جراحی‌های سیستم عصبی", 13),
                ("جراحی کلیه و مجاری ادراری", "جراح اورولوژی - جراحی دستگاه ادراری تناسلی", 14),
                ("جراحی پلاستیک و زیبایی", "جراح پلاستیک - جراحی‌های ترمیمی و زیبایی", 15),
                
                // تخصص‌های استخوان و مفاصل
                ("ارتوپدی", "متخصص ارتوپدی - تشخیص و درمان بیماری‌های استخوان و مفاصل", 16),
                ("فیزیوتراپی", "متخصص فیزیوتراپی - درمان فیزیکی و توانبخشی", 17),
                
                // تخصص‌های مغز و اعصاب
                ("نورولوژی", "متخصص مغز و اعصاب - تشخیص و درمان بیماری‌های عصبی", 18),
                
                // تخصص‌های زنان و زایمان
                ("زنان و زایمان", "متخصص زنان و زایمان - مراقبت‌های بهداشتی زنان", 19),
                ("نازایی و ناباروری", "متخصص نازایی - تشخیص و درمان ناباروری", 20),
                
                // تخصص‌های کودکان
                ("کودکان", "متخصص کودکان - مراقبت‌های پزشکی کودکان", 21),
                ("کودکان و نوزادان", "متخصص نوزادان - مراقبت‌های ویژه نوزادان", 22),
                
                // تخصص‌های چشم، گوش، حلق و بینی
                ("چشم پزشکی", "چشم پزشک - تشخیص و درمان بیماری‌های چشم", 23),
                ("گوش، حلق و بینی", "متخصص گوش و حلق و بینی - تشخیص و درمان بیماری‌های ENT", 24),
                
                // تخصص‌های پوست و مو
                ("پوست، مو و زیبایی", "متخصص پوست - تشخیص و درمان بیماری‌های پوستی", 25),
                
                // تخصص‌های دندانپزشکی
                ("دندانپزشکی عمومی", "دندانپزشک عمومی - مراقبت‌های دندانپزشکی عمومی", 26),
                ("جراحی دهان و فک و صورت", "جراح دهان و فک - جراحی‌های دهان و فک", 27),
                ("ارتودنسی", "متخصص ارتودنسی - اصلاح ناهنجاری‌های دندان و فک", 28),
                ("پریودنتیکس", "متخصص لثه - تشخیص و درمان بیماری‌های لثه", 29),
                
                // تخصص‌های تصویربرداری
                ("رادیولوژی", "متخصص رادیولوژی - تصویربرداری پزشکی", 30),
                ("سونوگرافی", "متخصص سونوگرافی - تصویربرداری سونوگرافی", 31),
                
                // تخصص‌های آزمایشگاهی
                ("پاتولوژی", "متخصص پاتولوژی - تشخیص بیماری‌ها از طریق آزمایشگاه", 32),
                
                // تخصص‌های دیگر
                ("طب سنتی و طب اسلامی", "متخصص طب سنتی - درمان با روش‌های سنتی", 33),
                ("داروسازی", "داروساز - مشاوره دارویی و تجویز دارو", 34),
                ("تغذیه و رژیم درمانی", "متخصص تغذیه - مشاوره تغذیه و رژیم درمانی", 35),
                ("پزشکی ورزشی", "متخصص پزشکی ورزشی - مراقبت از ورزشکاران و آسیب‌های ورزشی", 36),
                ("اورژانس", "متخصص اورژانس - مراقبت‌های اورژانسی", 37),
                ("آنستزیولوژی", "متخصص بیهوشی - مدیریت بیهوشی و درد", 38)
            };
        }

        #endregion

        #region الگوهای اطلاع‌رسانی (Notification Templates)

        public static class NotificationKeys
        {
            public const string Registration = "REGISTRATION";
            public const string AppointmentConfirmation = "APPOINTMENT_CONFIRMATION";
            public const string AppointmentReminder = "APPOINTMENT_REMINDER";
            public const string BirthdayWish = "BIRTHDAY_WISH";
            public const string PaymentConfirmation = "PAYMENT_CONFIRMATION";
        }

        #endregion

        #region تنظیمات کای و ضرایب (Factor Settings & Coefficients)

        /// <summary>
        /// مقادیر کای‌های مصوب سال مالی 1404
        /// </summary>
        public static class FactorSettings1404
        {
            // سال مالی
            public const int FinancialYear = 1404;
            
            // تاریخ شروع و پایان (میلادی)
            public static readonly DateTime EffectiveFrom = new DateTime(2025, 3, 21); // 1404/01/01
            public static readonly DateTime EffectiveTo = new DateTime(2026, 3, 20);   // 1404/12/29

            // ═══════════════════════════════════════════════════════════
            // کای‌های فنی (Technical Factors) - طبق مصوبه 1404
            // ═══════════════════════════════════════════════════════════
            
            /// <summary>
            /// کای فنی پایه برای خدمات عادی (مصوبه 1404)
            /// </summary>
            public const decimal TechnicalNormal = 4_350_000m; // 4,350,000 ریال
            
            /// <summary>
            /// کای فنی برای خدمات هشتگ‌دار کدهای ۱ تا ۷ (مصوبه 1404)
            /// </summary>
            public const decimal TechnicalHash_1_7 = 2_750_000m; // 2,750,000 ریال

            /// <summary>
            /// کای فنی برای خدمات هشتگ‌دار کدهای ۸ و ۹ (مصوبه 1404)
            /// </summary>
            public const decimal TechnicalHash_8_9 = 2_600_000m; // 2,600,000 ریال

            /// <summary>
            /// کای فنی دندانپزشکی (مصوبه 1404)
            /// </summary>
            public const decimal TechnicalDental = 1_900_000m; // 1,900,000 ریال

            /// <summary>
            /// کای فنی مواد و لوازم مصرفی دندانپزشکی (مصوبه 1404)
            /// </summary>
            public const decimal TechnicalDentalConsumables = 1_000_000m; // 1,000,000 ریال

            // ═══════════════════════════════════════════════════════════
            // کای‌های حرفه‌ای (Professional Factors) - طبق مصوبه 1404
            // ═══════════════════════════════════════════════════════════
            
            /// <summary>
            /// کای حرفه‌ای پایه برای خدمات عادی (مصوبه 1404)
            /// </summary>
            public const decimal ProfessionalNormal = 1_370_000m; // 1,370,000 ریال
            
            /// <summary>
            /// کای حرفه‌ای برای خدمات هشتگ‌دار (مصوبه 1404)
            /// </summary>
            public const decimal ProfessionalHash = 770_000m; // 770,000 ریال

            /// <summary>
            /// کای حرفه‌ای دندانپزشکی (مصوبه 1404)
            /// </summary>
            public const decimal ProfessionalDental = 850_000m; // 850,000 ریال
        }

        /// <summary>
        /// ضرایب پیش‌فرض برای قالب‌های خدمات
        /// </summary>
        public static class ServiceTemplateCoefficients
        {
            // ═══════════════════════════════════════════════════════════
            // پزشکان عمومی
            // ═══════════════════════════════════════════════════════════
            public const decimal GP_Technical = 0.5m;
            public const decimal GP_Professional = 1.3m;
            public const decimal GP_Over15Years_Technical = 0.0m;
            public const decimal GP_Over15Years_Professional = 0.4m;

            // ═══════════════════════════════════════════════════════════
            // متخصصین
            // ═══════════════════════════════════════════════════════════
            public const decimal Specialist_Technical = 0.7m;
            public const decimal Specialist_Professional = 1.8m;

            // ═══════════════════════════════════════════════════════════
            // فوق تخصص‌ها
            // ═══════════════════════════════════════════════════════════
            public const decimal SuperSpecialist_Technical = 0.8m;
            public const decimal SuperSpecialist_Professional = 2.3m;

            // ═══════════════════════════════════════════════════════════
            // روانپزشک
            // ═══════════════════════════════════════════════════════════
            public const decimal Psychiatrist_Technical = 0.8m;
            public const decimal Psychiatrist_Professional = 2.3m;

            // ═══════════════════════════════════════════════════════════
            // کارشناس ارشد
            // ═══════════════════════════════════════════════════════════
            public const decimal SeniorExpert_Technical = 0.4m;
            public const decimal SeniorExpert_Professional = 1.1m;

            // ═══════════════════════════════════════════════════════════
            // کارشناس
            // ═══════════════════════════════════════════════════════════
            public const decimal Expert_Technical = 0.35m;
            public const decimal Expert_Professional = 0.9m;

            // ═══════════════════════════════════════════════════════════
            // روانشناسی
            // ═══════════════════════════════════════════════════════════
            public const decimal Psychology_SeniorExpert_Technical = 0.90m;
            public const decimal Psychology_SeniorExpert_Professional = 3.5m;
            public const decimal Psychology_PhD_Technical = 1.20m;
            public const decimal Psychology_PhD_Professional = 4.0m;
            public const decimal Psychology_Over15Years_Technical = 0.20m;
            public const decimal Psychology_Over15Years_Professional = 0.4m;

            // ═══════════════════════════════════════════════════════════
            // ارزیابی کودک
            // ═══════════════════════════════════════════════════════════
            public const decimal ChildAssessment_Technical = 0.15m;
            public const decimal ChildAssessment_Professional = 0.5m;
        }

        #endregion

        #region قوانین کسب‌وکار (Business Rules)

        public static class BusinessRules
        {
            // اولویت‌های قوانین
            public const int HighestPriority = 10;
            public const int HighPriority = 9;
            public const int MediumPriority = 5;
            public const int LowPriority = 1;

            // سن‌های مرجع
            public const int SeniorAge = 65;
            public const int ElderlyAge = 60;

            // تخفیف‌ها
            public const decimal SeniorDiscount = 10m;
            public const decimal FemaleDiscount = 5m;
            public const decimal ExpensiveServiceDiscount = 15m;
            public const decimal SupplementaryDiscount = 20m;

            // مبالغ
            public const decimal ExpensiveServiceThreshold = 500000m;
            public const decimal ElderlyDeductible = 50000m;
            public const decimal EmergencyPaymentLimit = 5000000m;
        }

        #endregion

        #region تنظیمات زمانی (Time Settings)

        /// <summary>
        /// تعداد سال‌های گذشته برای ValidFrom
        /// </summary>
        public const int PastYearsForValidFrom = 10;

        /// <summary>
        /// تعداد سال‌های آینده برای ValidTo
        /// </summary>
        public const int FutureYearsForValidTo = 10;

        #endregion
    }
}

