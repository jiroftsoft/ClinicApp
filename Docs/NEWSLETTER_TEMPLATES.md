# قالب‌های خبرنامه حرفه‌ای - محیط پروداکشن درمانی

این فایل شامل 6 قالب خبرنامه حرفه‌ای، مدرن و بهینه برای استفاده در سیستم مدیریت خبرنامه کلینیک است.

---

## 📋 فهرست قالب‌ها

1. [خبرنامه مدرن عمومی](#1-خبرنامه-مدرن-عمومی)
2. [خبرنامه سلامت هوشمند](#2-خبرنامه-سلامت-هوشمند)
3. [معرفی خدمات و امکانات جدید](#3-معرفی-خدمات-و-امکانات-جدید)
4. [یادآوری هوشمند خدمات](#4-یادآوری-هوشمند-خدمات)
5. [اطلاعیه رسمی سازمانی](#5-اطلاعیه-رسمی-سازمانی)
6. [خبرنامه لیستی پویا](#6-خبرنامه-لیستی-پویا)

---

## 1. خبرنامه مدرن عمومی

**نام:** `خبرنامه مدرن عمومی`  
**موضوع:** `خبرنامه {{ClinicName}} - {{CurrentDate}}`  
**دسته‌بندی:** عمومی  
**توضیحات:** قالب مدرن و عمومی برای ارسال اخبار، اطلاعیه‌ها و پیام‌های عمومی کلینیک

### HTML Template:

```html
<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>خبرنامه {{ClinicName}}</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Vazir', 'Tahoma', Arial, sans-serif;
            direction: rtl;
            text-align: right;
            background-color: #f5f7fa;
            color: #2c3e50;
            line-height: 1.6;
        }
        .email-container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .email-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #ffffff;
            padding: 40px 30px;
            text-align: center;
        }
        .email-header h1 {
            font-size: 28px;
            font-weight: bold;
            margin-bottom: 10px;
        }
        .email-header p {
            font-size: 16px;
            opacity: 0.9;
        }
        .email-body {
            padding: 40px 30px;
        }
        .greeting {
            font-size: 18px;
            color: #2c3e50;
            margin-bottom: 20px;
            font-weight: 600;
        }
        .content {
            font-size: 16px;
            color: #34495e;
            margin-bottom: 30px;
            line-height: 1.8;
        }
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #ffffff;
            padding: 15px 40px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
            transition: transform 0.3s;
        }
        .cta-button:hover {
            transform: translateY(-2px);
        }
        .email-footer {
            background-color: #ecf0f1;
            padding: 30px;
            text-align: center;
            font-size: 14px;
            color: #7f8c8d;
        }
        .footer-links {
            margin: 20px 0;
        }
        .footer-links a {
            color: #667eea;
            text-decoration: none;
            margin: 0 10px;
        }
        .social-icons {
            margin: 20px 0;
        }
        .social-icons a {
            display: inline-block;
            margin: 0 10px;
            color: #667eea;
            font-size: 20px;
        }
        @media only screen and (max-width: 600px) {
            .email-container {
                width: 100% !important;
            }
            .email-header, .email-body, .email-footer {
                padding: 20px !important;
            }
        }
    </style>
</head>
<body>
    <div class="email-container">
        <div class="email-header">
            <h1>{{ClinicName}}</h1>
            <p>خبرنامه رسمی کلینیک</p>
        </div>
        
        <div class="email-body">
            <div class="greeting">
                سلام {{FullName}} عزیز،
            </div>
            
            <div class="content">
                <p>با تشکر از اعتماد شما به {{ClinicName}}، در این خبرنامه آخرین اخبار و اطلاعیه‌های کلینیک را با شما به اشتراک می‌گذاریم.</p>
                
                <p>امیدواریم این اطلاعات برای شما مفید باشد.</p>
            </div>
            
            <div style="text-align: center;">
                <a href="{{ClinicWebsite}}" class="cta-button">مشاهده وب‌سایت</a>
            </div>
        </div>
        
        <div class="email-footer">
            <p><strong>{{ClinicName}}</strong></p>
            <p>{{ClinicAddress}}</p>
            <p>تلفن: {{ClinicPhone}} | ایمیل: {{ClinicEmail}}</p>
            
            <div class="footer-links">
                <a href="{{ClinicWebsite}}">وب‌سایت</a>
                <a href="{{UnsubscribeUrl}}">لغو اشتراک</a>
            </div>
            
            <div class="social-icons">
                <a href="#">📱</a>
                <a href="#">📧</a>
                <a href="#">🌐</a>
            </div>
            
            <p style="margin-top: 20px; font-size: 12px; color: #95a5a6;">
                این ایمیل به آدرس {{Email}} ارسال شده است. تاریخ عضویت: {{SubscriptionDate}}
            </p>
        </div>
    </div>
</body>
</html>
```

---

## 2. خبرنامه سلامت هوشمند

**نام:** `خبرنامه سلامت هوشمند`  
**موضوع:** `نکات سلامت - {{CurrentDate}}`  
**دسته‌بندی:** سلامت و پزشکی  
**توضیحات:** قالب تخصصی برای ارسال نکات سلامت، مقالات پزشکی و توصیه‌های بهداشتی

### HTML Template:

```html
<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>نکات سلامت - {{ClinicName}}</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Vazir', 'Tahoma', Arial, sans-serif;
            direction: rtl;
            text-align: right;
            background-color: #f0f4f8;
            color: #2c3e50;
            line-height: 1.6;
        }
        .email-container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
        }
        .email-header {
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            color: #ffffff;
            padding: 50px 30px;
            text-align: center;
            position: relative;
            overflow: hidden;
        }
        .email-header::before {
            content: '🏥';
            font-size: 80px;
            position: absolute;
            opacity: 0.1;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
        }
        .email-header h1 {
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 10px;
            position: relative;
            z-index: 1;
        }
        .email-header p {
            font-size: 18px;
            opacity: 0.95;
            position: relative;
            z-index: 1;
        }
        .email-body {
            padding: 40px 30px;
        }
        .greeting {
            font-size: 20px;
            color: #11998e;
            margin-bottom: 25px;
            font-weight: 600;
        }
        .health-tip {
            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
            padding: 25px;
            border-radius: 10px;
            margin: 25px 0;
            border-right: 4px solid #11998e;
        }
        .health-tip h3 {
            color: #11998e;
            font-size: 20px;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
        }
        .health-tip h3::before {
            content: '💡';
            margin-left: 10px;
            font-size: 24px;
        }
        .health-tip p {
            font-size: 16px;
            color: #34495e;
            line-height: 1.8;
        }
        .content {
            font-size: 16px;
            color: #34495e;
            margin-bottom: 30px;
            line-height: 1.8;
        }
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            color: #ffffff;
            padding: 15px 40px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
            transition: transform 0.3s;
        }
        .email-footer {
            background-color: #2c3e50;
            color: #ffffff;
            padding: 30px;
            text-align: center;
            font-size: 14px;
        }
        .footer-links {
            margin: 20px 0;
        }
        .footer-links a {
            color: #38ef7d;
            text-decoration: none;
            margin: 0 10px;
        }
        @media only screen and (max-width: 600px) {
            .email-container {
                width: 100% !important;
            }
            .email-header, .email-body, .email-footer {
                padding: 20px !important;
            }
        }
    </style>
</head>
<body>
    <div class="email-container">
        <div class="email-header">
            <h1>نکات سلامت</h1>
            <p>اطلاعات پزشکی و بهداشتی برای شما</p>
        </div>
        
        <div class="email-body">
            <div class="greeting">
                {{FullName}} عزیز،
            </div>
            
            <div class="content">
                <p>در این شماره از خبرنامه سلامت، نکات مهم و مفیدی برای حفظ سلامتی شما گردآوری شده است.</p>
            </div>
            
            <div class="health-tip">
                <h3>نکته سلامت این هفته</h3>
                <p>برای حفظ سلامتی، روزانه حداقل 30 دقیقه فعالیت بدنی داشته باشید و رژیم غذایی متعادل را رعایت کنید.</p>
            </div>
            
            <div style="text-align: center; margin-top: 30px;">
                <a href="{{ClinicWebsite}}" class="cta-button">مشاهده مقالات بیشتر</a>
            </div>
        </div>
        
        <div class="email-footer">
            <p><strong>{{ClinicName}}</strong></p>
            <p>{{ClinicAddress}}</p>
            <p>تلفن: {{ClinicPhone}} | ایمیل: {{ClinicEmail}}</p>
            
            <div class="footer-links">
                <a href="{{ClinicWebsite}}">وب‌سایت</a>
                <a href="{{UnsubscribeUrl}}">لغو اشتراک</a>
            </div>
            
            <p style="margin-top: 20px; font-size: 12px; color: #95a5a6;">
                این ایمیل به آدرس {{Email}} ارسال شده است. تاریخ عضویت: {{SubscriptionDate}}
            </p>
        </div>
    </div>
</body>
</html>
```

---

## 3. معرفی خدمات و امکانات جدید

**نام:** `معرفی خدمات و امکانات جدید`  
**موضوع:** `خدمات جدید {{ClinicName}} - {{CurrentDate}}`  
**دسته‌بندی:** خدمات  
**توضیحات:** قالب جذاب برای معرفی خدمات جدید، امکانات تازه و پیشنهادات ویژه کلینیک

### HTML Template:

```html
<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>خدمات جدید - {{ClinicName}}</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Vazir', 'Tahoma', Arial, sans-serif;
            direction: rtl;
            text-align: right;
            background-color: #f8f9fa;
            color: #2c3e50;
            line-height: 1.6;
        }
        .email-container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 4px 20px rgba(0,0,0,0.1);
        }
        .email-header {
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: #ffffff;
            padding: 50px 30px;
            text-align: center;
            position: relative;
        }
        .email-header::after {
            content: '✨';
            font-size: 60px;
            position: absolute;
            opacity: 0.2;
            top: 20px;
            left: 20px;
        }
        .email-header h1 {
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 10px;
        }
        .email-header p {
            font-size: 18px;
            opacity: 0.95;
        }
        .email-body {
            padding: 40px 30px;
        }
        .greeting {
            font-size: 20px;
            color: #f5576c;
            margin-bottom: 25px;
            font-weight: 600;
        }
        .service-card {
            background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
            border: 2px solid #f093fb;
            border-radius: 15px;
            padding: 25px;
            margin: 25px 0;
            transition: transform 0.3s;
        }
        .service-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 8px 25px rgba(245, 87, 108, 0.2);
        }
        .service-card h3 {
            color: #f5576c;
            font-size: 22px;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
        }
        .service-card h3::before {
            content: '🎯';
            margin-left: 10px;
            font-size: 24px;
        }
        .service-card p {
            font-size: 16px;
            color: #34495e;
            line-height: 1.8;
        }
        .badge {
            display: inline-block;
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: #ffffff;
            padding: 5px 15px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: bold;
            margin-bottom: 15px;
        }
        .content {
            font-size: 16px;
            color: #34495e;
            margin-bottom: 30px;
            line-height: 1.8;
        }
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: #ffffff;
            padding: 15px 40px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
            transition: transform 0.3s;
        }
        .cta-button:hover {
            transform: translateY(-2px);
        }
        .email-footer {
            background-color: #2c3e50;
            color: #ffffff;
            padding: 30px;
            text-align: center;
            font-size: 14px;
        }
        .footer-links {
            margin: 20px 0;
        }
        .footer-links a {
            color: #f093fb;
            text-decoration: none;
            margin: 0 10px;
        }
        @media only screen and (max-width: 600px) {
            .email-container {
                width: 100% !important;
            }
            .email-header, .email-body, .email-footer {
                padding: 20px !important;
            }
        }
    </style>
</head>
<body>
    <div class="email-container">
        <div class="email-header">
            <h1>خدمات جدید</h1>
            <p>امکانات و پیشنهادات ویژه برای شما</p>
        </div>
        
        <div class="email-body">
            <div class="greeting">
                {{FullName}} عزیز،
            </div>
            
            <div class="content">
                <p>ما همیشه در تلاش هستیم تا بهترین خدمات را به شما ارائه دهیم. در این خبرنامه، خدمات و امکانات جدید کلینیک را به شما معرفی می‌کنیم.</p>
            </div>
            
            <div class="service-card">
                <span class="badge">جدید</span>
                <h3>خدمات جدید</h3>
                <p>ما با افتخار خدمات جدید و پیشرفته‌ای را به مجموعه خدمات خود اضافه کرده‌ایم. این خدمات با استفاده از آخرین تکنولوژی‌های روز دنیا ارائه می‌شوند.</p>
            </div>
            
            <div style="text-align: center; margin-top: 30px;">
                <a href="{{ClinicWebsite}}" class="cta-button">مشاهده جزئیات بیشتر</a>
            </div>
        </div>
        
        <div class="email-footer">
            <p><strong>{{ClinicName}}</strong></p>
            <p>{{ClinicAddress}}</p>
            <p>تلفن: {{ClinicPhone}} | ایمیل: {{ClinicEmail}}</p>
            
            <div class="footer-links">
                <a href="{{ClinicWebsite}}">وب‌سایت</a>
                <a href="{{UnsubscribeUrl}}">لغو اشتراک</a>
            </div>
            
            <p style="margin-top: 20px; font-size: 12px; color: #95a5a6;">
                این ایمیل به آدرس {{Email}} ارسال شده است. تاریخ عضویت: {{SubscriptionDate}}
            </p>
        </div>
    </div>
</body>
</html>
```

---

## 4. یادآوری هوشمند خدمات

**نام:** `یادآوری هوشمند خدمات`  
**موضوع:** `یادآوری مهم - {{CurrentDate}}`  
**دسته‌بندی:** یادآوری  
**توضیحات:** قالب حرفه‌ای برای یادآوری نوبت‌ها، ویزیت‌ها و خدمات مهم به بیماران

### HTML Template:

```html
<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>یادآوری مهم - {{ClinicName}}</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Vazir', 'Tahoma', Arial, sans-serif;
            direction: rtl;
            text-align: right;
            background-color: #fff5f5;
            color: #2c3e50;
            line-height: 1.6;
        }
        .email-container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 4px 20px rgba(0,0,0,0.1);
        }
        .email-header {
            background: linear-gradient(135deg, #fa709a 0%, #fee140 100%);
            color: #ffffff;
            padding: 50px 30px;
            text-align: center;
            position: relative;
        }
        .email-header::before {
            content: '🔔';
            font-size: 70px;
            position: absolute;
            opacity: 0.2;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
        }
        .email-header h1 {
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 10px;
            position: relative;
            z-index: 1;
        }
        .email-header p {
            font-size: 18px;
            opacity: 0.95;
            position: relative;
            z-index: 1;
        }
        .email-body {
            padding: 40px 30px;
        }
        .greeting {
            font-size: 20px;
            color: #fa709a;
            margin-bottom: 25px;
            font-weight: 600;
        }
        .reminder-box {
            background: linear-gradient(135deg, #fff5f5 0%, #ffe0e0 100%);
            border: 3px solid #fa709a;
            border-radius: 15px;
            padding: 30px;
            margin: 25px 0;
            text-align: center;
        }
        .reminder-box h3 {
            color: #fa709a;
            font-size: 24px;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .reminder-box h3::before {
            content: '⏰';
            margin-left: 10px;
            font-size: 28px;
        }
        .reminder-box p {
            font-size: 18px;
            color: #2c3e50;
            line-height: 1.8;
            margin: 15px 0;
        }
        .reminder-date {
            background-color: #fa709a;
            color: #ffffff;
            padding: 15px 30px;
            border-radius: 10px;
            font-size: 20px;
            font-weight: bold;
            display: inline-block;
            margin: 20px 0;
        }
        .content {
            font-size: 16px;
            color: #34495e;
            margin-bottom: 30px;
            line-height: 1.8;
        }
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #fa709a 0%, #fee140 100%);
            color: #ffffff;
            padding: 15px 40px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
            transition: transform 0.3s;
        }
        .cta-button:hover {
            transform: translateY(-2px);
        }
        .email-footer {
            background-color: #2c3e50;
            color: #ffffff;
            padding: 30px;
            text-align: center;
            font-size: 14px;
        }
        .footer-links {
            margin: 20px 0;
        }
        .footer-links a {
            color: #fee140;
            text-decoration: none;
            margin: 0 10px;
        }
        @media only screen and (max-width: 600px) {
            .email-container {
                width: 100% !important;
            }
            .email-header, .email-body, .email-footer {
                padding: 20px !important;
            }
        }
    </style>
</head>
<body>
    <div class="email-container">
        <div class="email-header">
            <h1>یادآوری مهم</h1>
            <p>یادآوری خدمات و نوبت‌های شما</p>
        </div>
        
        <div class="email-body">
            <div class="greeting">
                {{FullName}} عزیز،
            </div>
            
            <div class="content">
                <p>این ایمیل به عنوان یادآوری برای شما ارسال شده است.</p>
            </div>
            
            <div class="reminder-box">
                <h3>یادآوری مهم</h3>
                <div class="reminder-date">
                    {{CurrentDate}}
                </div>
                <p>لطفاً در تاریخ و زمان تعیین شده در کلینیک حضور داشته باشید.</p>
                <p>در صورت نیاز به تغییر یا لغو نوبت، لطفاً با ما تماس بگیرید.</p>
            </div>
            
            <div style="text-align: center; margin-top: 30px;">
                <a href="{{ClinicWebsite}}" class="cta-button">تماس با کلینیک</a>
            </div>
        </div>
        
        <div class="email-footer">
            <p><strong>{{ClinicName}}</strong></p>
            <p>{{ClinicAddress}}</p>
            <p>تلفن: {{ClinicPhone}} | ایمیل: {{ClinicEmail}}</p>
            
            <div class="footer-links">
                <a href="{{ClinicWebsite}}">وب‌سایت</a>
                <a href="{{UnsubscribeUrl}}">لغو اشتراک</a>
            </div>
            
            <p style="margin-top: 20px; font-size: 12px; color: #95a5a6;">
                این ایمیل به آدرس {{Email}} ارسال شده است. تاریخ عضویت: {{SubscriptionDate}}
            </p>
        </div>
    </div>
</body>
</html>
```

---

## 5. اطلاعیه رسمی سازمانی

**نام:** `اطلاعیه رسمی سازمانی`  
**موضوع:** `اطلاعیه رسمی {{ClinicName}} - {{CurrentDate}}`  
**دسته‌بندی:** اطلاعیه  
**توضیحات:** قالب رسمی و حرفه‌ای برای ارسال اطلاعیه‌های رسمی، تغییرات مهم و اعلانات کلینیک

### HTML Template:

```html
<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>اطلاعیه رسمی - {{ClinicName}}</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Vazir', 'Tahoma', Arial, sans-serif;
            direction: rtl;
            text-align: right;
            background-color: #f8f9fa;
            color: #2c3e50;
            line-height: 1.6;
        }
        .email-container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 4px 20px rgba(0,0,0,0.15);
            border-top: 5px solid #2c3e50;
        }
        .email-header {
            background-color: #2c3e50;
            color: #ffffff;
            padding: 40px 30px;
            text-align: center;
        }
        .email-header h1 {
            font-size: 28px;
            font-weight: bold;
            margin-bottom: 10px;
            letter-spacing: 1px;
        }
        .email-header p {
            font-size: 16px;
            opacity: 0.9;
        }
        .official-badge {
            background-color: #e74c3c;
            color: #ffffff;
            padding: 8px 20px;
            border-radius: 5px;
            font-size: 14px;
            font-weight: bold;
            display: inline-block;
            margin: 20px 0;
        }
        .email-body {
            padding: 40px 30px;
        }
        .greeting {
            font-size: 18px;
            color: #2c3e50;
            margin-bottom: 25px;
            font-weight: 600;
        }
        .announcement-box {
            background-color: #f8f9fa;
            border-right: 4px solid #2c3e50;
            padding: 25px;
            margin: 25px 0;
            border-radius: 5px;
        }
        .announcement-box h3 {
            color: #2c3e50;
            font-size: 20px;
            margin-bottom: 15px;
            font-weight: bold;
        }
        .announcement-box p {
            font-size: 16px;
            color: #34495e;
            line-height: 1.8;
            margin: 10px 0;
        }
        .content {
            font-size: 16px;
            color: #34495e;
            margin-bottom: 30px;
            line-height: 1.8;
        }
        .signature {
            margin-top: 40px;
            padding-top: 20px;
            border-top: 2px solid #ecf0f1;
        }
        .signature p {
            font-size: 14px;
            color: #7f8c8d;
            margin: 5px 0;
        }
        .email-footer {
            background-color: #34495e;
            color: #ffffff;
            padding: 30px;
            text-align: center;
            font-size: 14px;
        }
        .footer-links {
            margin: 20px 0;
        }
        .footer-links a {
            color: #ecf0f1;
            text-decoration: none;
            margin: 0 10px;
        }
        @media only screen and (max-width: 600px) {
            .email-container {
                width: 100% !important;
            }
            .email-header, .email-body, .email-footer {
                padding: 20px !important;
            }
        }
    </style>
</head>
<body>
    <div class="email-container">
        <div class="email-header">
            <h1>{{ClinicName}}</h1>
            <p>اطلاعیه رسمی</p>
        </div>
        
        <div class="email-body">
            <div style="text-align: center;">
                <span class="official-badge">اطلاعیه رسمی</span>
            </div>
            
            <div class="greeting">
                {{FullName}} محترم،
            </div>
            
            <div class="content">
                <p>با احترام، به استحضار می‌رساند:</p>
            </div>
            
            <div class="announcement-box">
                <h3>متن اطلاعیه</h3>
                <p>این اطلاعیه به منظور آگاهی شما از تغییرات و تصمیمات مهم کلینیک ارسال شده است.</p>
                <p>لطفاً این اطلاعیه را با دقت مطالعه فرمایید.</p>
            </div>
            
            <div class="signature">
                <p><strong>با احترام</strong></p>
                <p>مدیریت {{ClinicName}}</p>
                <p>تاریخ: {{CurrentDate}}</p>
            </div>
        </div>
        
        <div class="email-footer">
            <p><strong>{{ClinicName}}</strong></p>
            <p>{{ClinicAddress}}</p>
            <p>تلفن: {{ClinicPhone}} | ایمیل: {{ClinicEmail}}</p>
            
            <div class="footer-links">
                <a href="{{ClinicWebsite}}">وب‌سایت</a>
                <a href="{{UnsubscribeUrl}}">لغو اشتراک</a>
            </div>
            
            <p style="margin-top: 20px; font-size: 12px; color: #95a5a6;">
                این ایمیل به آدرس {{Email}} ارسال شده است. تاریخ عضویت: {{SubscriptionDate}}
            </p>
        </div>
    </div>
</body>
</html>
```

---

## 6. خبرنامه لیستی پویا

**نام:** `خبرنامه لیستی پویا`  
**موضوع:** `خلاصه اخبار {{ClinicName}} - {{CurrentDate}}`  
**دسته‌بندی:** لیست اخبار  
**توضیحات:** قالب پویا و مدرن برای نمایش لیست اخبار، مقالات و مطالب مختلف در قالب یک خبرنامه

### HTML Template:

```html
<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>خلاصه اخبار - {{ClinicName}}</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Vazir', 'Tahoma', Arial, sans-serif;
            direction: rtl;
            text-align: right;
            background-color: #f5f7fa;
            color: #2c3e50;
            line-height: 1.6;
        }
        .email-container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            box-shadow: 0 4px 20px rgba(0,0,0,0.1);
        }
        .email-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #ffffff;
            padding: 40px 30px;
            text-align: center;
        }
        .email-header h1 {
            font-size: 28px;
            font-weight: bold;
            margin-bottom: 10px;
        }
        .email-header p {
            font-size: 16px;
            opacity: 0.9;
        }
        .email-body {
            padding: 40px 30px;
        }
        .greeting {
            font-size: 18px;
            color: #2c3e50;
            margin-bottom: 25px;
            font-weight: 600;
        }
        .news-list {
            margin: 30px 0;
        }
        .news-item {
            background-color: #f8f9fa;
            border-right: 4px solid #667eea;
            padding: 20px;
            margin: 20px 0;
            border-radius: 8px;
            transition: transform 0.3s;
        }
        .news-item:hover {
            transform: translateX(-5px);
            box-shadow: 0 4px 15px rgba(102, 126, 234, 0.2);
        }
        .news-item h3 {
            color: #667eea;
            font-size: 20px;
            margin-bottom: 10px;
            font-weight: bold;
        }
        .news-item p {
            font-size: 15px;
            color: #34495e;
            line-height: 1.8;
            margin: 10px 0;
        }
        .news-date {
            font-size: 12px;
            color: #7f8c8d;
            margin-top: 10px;
        }
        .content {
            font-size: 16px;
            color: #34495e;
            margin-bottom: 30px;
            line-height: 1.8;
        }
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #ffffff;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 10px 5px;
            font-size: 14px;
            transition: transform 0.3s;
        }
        .cta-button:hover {
            transform: translateY(-2px);
        }
        .email-footer {
            background-color: #2c3e50;
            color: #ffffff;
            padding: 30px;
            text-align: center;
            font-size: 14px;
        }
        .footer-links {
            margin: 20px 0;
        }
        .footer-links a {
            color: #667eea;
            text-decoration: none;
            margin: 0 10px;
        }
        @media only screen and (max-width: 600px) {
            .email-container {
                width: 100% !important;
            }
            .email-header, .email-body, .email-footer {
                padding: 20px !important;
            }
            .news-item {
                padding: 15px !important;
            }
        }
    </style>
</head>
<body>
    <div class="email-container">
        <div class="email-header">
            <h1>خلاصه اخبار</h1>
            <p>آخرین اخبار و مطالب {{ClinicName}}</p>
        </div>
        
        <div class="email-body">
            <div class="greeting">
                {{FullName}} عزیز،
            </div>
            
            <div class="content">
                <p>در این شماره از خبرنامه، خلاصه‌ای از آخرین اخبار و مطالب کلینیک را برای شما گردآوری کرده‌ایم.</p>
            </div>
            
            <div class="news-list">
                <div class="news-item">
                    <h3>📰 عنوان خبر اول</h3>
                    <p>خلاصه خبر اول در اینجا قرار می‌گیرد. این متن می‌تواند شامل اطلاعات مهم و جذاب باشد.</p>
                    <div class="news-date">{{CurrentDate}}</div>
                    <a href="{{ClinicWebsite}}" class="cta-button">ادامه مطلب</a>
                </div>
                
                <div class="news-item">
                    <h3>📰 عنوان خبر دوم</h3>
                    <p>خلاصه خبر دوم در اینجا قرار می‌گیرد. این متن می‌تواند شامل اطلاعات مهم و جذاب باشد.</p>
                    <div class="news-date">{{CurrentDate}}</div>
                    <a href="{{ClinicWebsite}}" class="cta-button">ادامه مطلب</a>
                </div>
                
                <div class="news-item">
                    <h3>📰 عنوان خبر سوم</h3>
                    <p>خلاصه خبر سوم در اینجا قرار می‌گیرد. این متن می‌تواند شامل اطلاعات مهم و جذاب باشد.</p>
                    <div class="news-date">{{CurrentDate}}</div>
                    <a href="{{ClinicWebsite}}" class="cta-button">ادامه مطلب</a>
                </div>
            </div>
            
            <div style="text-align: center; margin-top: 30px;">
                <a href="{{ClinicWebsite}}" class="cta-button">مشاهده تمام اخبار</a>
            </div>
        </div>
        
        <div class="email-footer">
            <p><strong>{{ClinicName}}</strong></p>
            <p>{{ClinicAddress}}</p>
            <p>تلفن: {{ClinicPhone}} | ایمیل: {{ClinicEmail}}</p>
            
            <div class="footer-links">
                <a href="{{ClinicWebsite}}">وب‌سایت</a>
                <a href="{{UnsubscribeUrl}}">لغو اشتراک</a>
            </div>
            
            <p style="margin-top: 20px; font-size: 12px; color: #95a5a6;">
                این ایمیل به آدرس {{Email}} ارسال شده است. تاریخ عضویت: {{SubscriptionDate}}
            </p>
        </div>
    </div>
</body>
</html>
```

---

## 📝 نکات مهم استفاده از قالب‌ها

### متغیرهای قابل استفاده:

- `{{FullName}}` - نام کامل مشترک
- `{{FirstName}}` - نام کوچک
- `{{LastName}}` - نام خانوادگی
- `{{Email}}` - ایمیل مشترک
- `{{PhoneNumber}}` - شماره تماس
- `{{SubscriptionDate}}` - تاریخ عضویت (شمسی)
- `{{CurrentDate}}` - تاریخ جاری (شمسی)
- `{{CurrentTime}}` - زمان جاری
- `{{CurrentDateTime}}` - تاریخ و زمان جاری (شمسی)
- `{{ClinicName}}` - نام کلینیک
- `{{ClinicPhone}}` - شماره تماس کلینیک
- `{{ClinicAddress}}` - آدرس کلینیک
- `{{ClinicEmail}}` - ایمیل کلینیک
- `{{ClinicWebsite}}` - وب‌سایت کلینیک
- `{{UnsubscribeUrl}}` - لینک لغو اشتراک
- `{{VerificationUrl}}` - لینک تایید اشتراک

### ویژگی‌های قالب‌ها:

✅ **Responsive Design** - سازگار با موبایل و تبلت  
✅ **Modern UI/UX** - طراحی مدرن و کاربرپسند  
✅ **RTL Support** - پشتیبانی کامل از راست‌چین  
✅ **Professional** - مناسب محیط پروداکشن  
✅ **Medical Theme** - مخصوص محیط درمانی  
✅ **Optimized** - بهینه برای ارسال ایمیل  
✅ **Accessible** - قابل دسترس برای همه

### دستورالعمل استفاده:

1. هر قالب را در سیستم مدیریت خبرنامه کپی کنید
2. متغیرها را با داده‌های واقعی جایگزین کنید
3. محتوای خاص را بر اساس نیاز خود تغییر دهید
4. قبل از ارسال، پیش‌نمایش را بررسی کنید
5. تست کنید که تمام لینک‌ها و متغیرها درست کار می‌کنند

---

**تاریخ ایجاد:** {{CurrentDate}}  
**نسخه:** 1.0  
**وضعیت:** آماده برای Production

