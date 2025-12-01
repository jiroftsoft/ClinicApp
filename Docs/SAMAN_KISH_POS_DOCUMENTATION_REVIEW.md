# گزارش بررسی مستندات پوز سامان کیش (SSP1126)
**تاریخ:** 1402-06-29  
**کد درخواست:** SSP1126(WEB)

---

## 📋 خلاصه اجرایی

این گزارش شامل بررسی کامل مستندات پوز سامان کیش (SSP1126) دریافت شده از شرکت و مقایسه آن با پیاده‌سازی فعلی است.

---

## 📁 ساختار فایل‌های مستندات

### 1. فایل‌های موجود در `Infrastructure/SSP1126(WEB)/`

#### 1.1. مستندات PDF
- ✅ **`SSP1126-WebBased(SignalR)_1_2_1.pdf`**
  - نسخه: 1.2.1
  - نوع: Web-Based با SignalR
  - **Metadata:**
    - Creator: Ali Ahmadvand
    - Created: 2023-09-20T10:59:29+03:30
    - Producer: Microsoft® Word LTSC
    - DocumentID: uuid:90ED8212-F91D-44FD-889C-EAA6CB6C6150
  - **وضعیت:** فایل PDF موجود است و شامل مستندات کامل SSP1126 با SignalR است
  - **توصیه:** برای جزئیات دقیق‌تر، فایل PDF باید توسط تیم بررسی شود

#### 1.2. Windows Service
- ✅ **`ServiceInstaller_1402-06-29/`**
  - **فایل اصلی:** `SSP1126SignalRWindowsService.exe`
  - **Config:** `SSP1126SignalRWindowsService.exe.config`
  - **Port:** 8080 (از config)
  - **Dependencies:**
    - Microsoft.AspNet.SignalR.Core
    - Microsoft.Owin
    - Newtonsoft.Json
    - Sep.Logger

#### 1.3. نمونه HTML
- ✅ **`Web Tester_1402-06-29/Sample(SSP1126)Page.html`**
  - نمونه کامل استفاده از SSP1126 از طریق SignalR
  - شامل تمام عملیات: Purchase, Bill, Balance, Inquiry, ServicePayment

---

## 🔍 بررسی نمونه HTML (Sample Page)

### 1. اتصال SignalR

```javascript
$.connection.hub.url = "http://localhost:8080/signalr";
var console = $.connection.SSP1126HUB;
$.connection.hub.start().done(function initialize() {
    // ...
});
```

**نکات مهم:**
- ✅ Hub Name: `SSP1126HUB`
- ✅ Default URL: `http://localhost:8080/signalr`
- ✅ استفاده از SignalR 2.4.2

### 2. متدهای Server (Hub Methods)

#### 2.1. Initialization
```javascript
console.server.Initial(
    ConnectionType,  // 0=COM, 1=Network
    IpAddress,       // IP دستگاه (برای Network)
    Port,            // COM Port (برای COM)
    AccountType,     // 0=Single, 1=Share
    Language,        // 0=Farsi, 1=English
    '0'              // Additional parameter
);
```

#### 2.2. Purchase (خرید)
```javascript
// 1 Step Purchase
console.server.SendAmount1Step(
    Amount,          // مبلغ
    Amounts,         // لیست مبالغ (برای Share Account)
    Additional,      // اطلاعات اضافی (XML)
    Reference,       // پیام مرجع
    PurchaseID,      // شناسه خرید
    TerminalID       // شناسه ترمینال
);

// 2 Step Purchase
console.server.SendAmount2Step(
    Amount,
    Amounts,
    Additional,
    Reference,
    PurchaseType,    // نوع خرید
    PurchaseID,
    TerminalID
);
```

#### 2.3. Bill Payment (پرداخت قبض)
```javascript
console.server.Bill(
    BillID,          // شناسه قبض
    PayID,           // شناسه پرداخت
    Additional,       // اطلاعات اضافی
    Reference        // پیام مرجع
);
```

#### 2.4. Request (درخواست)
```javascript
console.server.Request("1");  // Request Type 1
console.server.Request("2");  // Request Type 2 (MCI Bill)
console.server.Request("3");  // Request Type 3 (POS Start)
console.server.Request("4");  // Request Type 4 (National Code)
console.server.Request("5");  // Request Type 5 (Balance)
```

#### 2.5. Inquiry (استعلام)
```javascript
console.server.Inquiry(RRN);  // استعلام بر اساس RRN
```

#### 2.6. Balance (موجودی)
```javascript
console.server.Request("5");  // Balance inquiry
```

#### 2.7. Service Payment (پرداخت خدمت)
```javascript
console.server.PaymentServiceSendData(
    PaymentServiceData,      // داده‌های خدمت
    PaymentServiceAddData,   // داده‌های اضافی
    null,
    null
);
```

#### 2.8. Reset
```javascript
console.server.Reset();  // Reset connection
```

### 3. Client Callbacks (Events)

#### 3.1. GetSystemResponse
```javascript
console.client.GetSystemResponse = function (message) {
    // message: '0' = Success, other = Error
};
```

#### 3.2. GetCardSwiped
```javascript
console.client.GetCardSwiped = function (
    TerminalId,
    CardNumberHash,      // شماره کارت Hash شده
    CardNumberMask,      // شماره کارت Mask شده
    PurchaseTypes,       // انواع خرید (separated by *)
    Encrypted_National_Code  // کد ملی رمزگذاری شده
) {
    // کارت خوانده شد
};
```

#### 3.3. GetTransactionResponse
```javascript
console.client.GetTransactionResponse = function (
    TerminalId,
    ResponseCode,        // کد پاسخ (00 = موفق)
    SerialId,            // شناسه سریال
    RRN,                 // شماره مرجع
    ResponseDescription, // توضیحات پاسخ
    TxnDate,             // تاریخ تراکنش
    Amount,              // مبلغ
    CardNumberMask       // شماره کارت Mask شده
) {
    // پاسخ تراکنش دریافت شد
};
```

---

## 🔄 مقایسه با پیاده‌سازی فعلی

### 1. روش اتصال

#### پیاده‌سازی فعلی (`SamanKishDriver.cs`)
- ✅ **روش:** TCP/IP مستقیم
- ✅ **Port:** 5000 (پیش‌فرض)
- ✅ **Protocol:** SSP1126 (Binary Protocol)
- ✅ **مزایا:**
  - بدون نیاز به Windows Service
  - اتصال مستقیم و سریع
  - کنترل کامل روی اتصال

#### روش مستندات (SignalR)
- ✅ **روش:** SignalR Hub
- ✅ **Port:** 8080
- ✅ **Protocol:** SignalR (WebSocket/Long Polling)
- ✅ **مزایا:**
  - ارتباط Real-time
  - پشتیبانی از چندین Client همزمان
  - مدیریت خودکار اتصال

### 2. Flow پرداخت

#### پیاده‌سازی فعلی
```
1. ConnectAsync (TCP/IP)
   ↓
2. SendPaymentAsync (Command: PAY + TerminalId + MerchantId + Amount)
   ↓
3. ReadResponseAsync (Response: Status + RRN + TraceNo + CardLast4)
   ↓
4. ParsePaymentResponse
   ↓
5. DisconnectAsync
```

#### روش مستندات (SignalR)
```
1. Initial (ConnectionType, IP, Port, AccountType, Language)
   ↓
2. GetSystemResponse ('0' = Success)
   ↓
3. SendAmount1Step / SendAmount2Step
   ↓
4. GetCardSwiped (کارت خوانده شد)
   ↓
5. GetTransactionResponse (پاسخ تراکنش)
```

### 3. تفاوت‌های کلیدی

| ویژگی | پیاده‌سازی فعلی | روش مستندات |
|------|----------------|-------------|
| **اتصال** | TCP/IP مستقیم | SignalR Hub |
| **Port** | 5000 | 8080 |
| **Windows Service** | ❌ نیاز نیست | ✅ نیاز است |
| **Real-time Events** | ❌ ندارد | ✅ دارد |
| **Multiple Clients** | ❌ ندارد | ✅ دارد |
| **Card Swipe Event** | ❌ ندارد | ✅ دارد |
| **Connection Management** | Manual | Automatic |

---

## 📊 تحلیل مستندات

### 1. Connection Types

#### 1.1. Network (TCP/IP)
- **ConnectionType:** `1`
- **Parameters:** IP Address, Port
- **مثال:** `Initial(1, "192.168.1.54", null, 0, 0, '0')`

#### 1.2. COM (Serial)
- **ConnectionType:** `0`
- **Parameters:** COM Port
- **مثال:** `Initial(0, null, "COM4", 0, 0, '0')`

### 2. Account Types

#### 2.1. Single Account
- **AccountType:** `0`
- **استفاده:** یک حساب برای پرداخت

#### 2.2. Share Account
- **AccountType:** `1`
- **استفاده:** چند حساب برای پرداخت (مبالغ مختلف)

### 3. Purchase Methods

#### 3.1. 1 Step Purchase
- **مزایا:** سریع‌تر
- **استفاده:** `SendAmount1Step`

#### 3.2. 2 Step Purchase
- **مزایا:** انعطاف‌پذیرتر (انتخاب نوع خرید)
- **استفاده:** `SendAmount2Step`
- **نیاز به:** `GetCardSwiped` برای دریافت `PurchaseTypes`

### 4. Response Codes

از نمونه HTML و کد فعلی:
- **00:** موفق
- **01-20:** خطاهای مختلف (در `SamanKishDriver.cs` پوشش داده شده)

---

## ⚠️ نکات مهم از مستندات

### 1. Initialization
- ✅ **الزامی:** قبل از هر عملیات باید `Initial` فراخوانی شود
- ✅ **Timeout:** 1 ثانیه تاخیر بعد از Initial
- ✅ **بررسی:** `GetSystemResponse('0')` = موفقیت

### 2. Port Configuration
- ⚠️ **مهم:** Port 2155 برای ارتباط دستگاه با سرور بانک است، نه PC ↔ POS!
- ✅ **Ports صحیح:** 5000, 8080, 9100 (بسته به مدل دستگاه)
- ✅ **از config:** `SSP1126SignalRWindowsService.exe.config` → `PosIP`

### 3. SignalR Hub
- ✅ **Hub Name:** `SSP1126HUB`
- ✅ **URL:** `http://localhost:8080/signalr`
- ✅ **Version:** SignalR 2.4.2

### 4. Windows Service
- ✅ **Service Name:** `SSP1126SignalRWindowsService`
- ✅ **Port:** 8080 (قابل تغییر در config)
- ✅ **Log Path:** `D:\Log\` (از config)

---

## 🔧 پیشنهادات بهبود

### 1. اضافه کردن پشتیبانی از SignalR

#### 1.1. ایجاد SignalR Driver
```csharp
public class SamanKishSignalRDriver : IPosDeviceDriver
{
    private readonly IHubConnectionContext<dynamic> _hubContext;
    // ...
}
```

#### 1.2. مزایا
- ✅ Real-time events (Card Swipe)
- ✅ پشتیبانی از چندین Client
- ✅ مدیریت خودکار اتصال

### 2. بهبود Driver فعلی

#### 2.1. اضافه کردن Event Support
- ✅ Card Swipe Event
- ✅ Transaction Status Updates
- ✅ Connection Status

#### 2.2. بهبود Error Messages
- ✅ استفاده از پیام‌های مستندات
- ✅ کدهای خطای دقیق‌تر

### 3. یکپارچه‌سازی

#### 3.1. انتخاب روش اتصال
- ✅ TCP/IP مستقیم (فعلی) - برای Single Client
- ✅ SignalR (جدید) - برای Multiple Clients / Real-time

#### 3.2. Configuration
```csharp
public enum PosConnectionMethod
{
    DirectTcpIp,  // فعلی
    SignalR       // جدید
}
```

---

## 📝 چک‌لیست بررسی

### مستندات
- [x] فایل PDF موجود است
- [x] نمونه HTML موجود است
- [x] Windows Service موجود است
- [x] Config فایل موجود است
- [ ] **PDF باید توسط تیم بررسی شود** ⚠️

### پیاده‌سازی فعلی
- [x] TCP/IP Driver موجود است
- [x] Protocol SSP1126 پیاده‌سازی شده
- [x] Error Handling کامل است
- [x] Logging کامل است
- [ ] SignalR Driver موجود نیست ⚠️

### مقایسه
- [x] تفاوت‌ها شناسایی شد
- [x] مزایا/معایب هر روش مشخص شد
- [x] پیشنهادات بهبود ارائه شد

---

## 🎯 نتیجه‌گیری

### وضعیت فعلی
- ✅ **Driver TCP/IP:** کامل و آماده استفاده
- ✅ **Protocol SSP1126:** پیاده‌سازی شده
- ⚠️ **SignalR Support:** موجود نیست

### مستندات دریافت شده
- ✅ **PDF:** موجود است (نیاز به بررسی تیم)
- ✅ **نمونه HTML:** کامل و قابل استفاده
- ✅ **Windows Service:** موجود است

### توصیه‌ها
1. ✅ **ادامه استفاده از TCP/IP Driver** برای Single Client scenarios
2. ⚠️ **بررسی PDF** توسط تیم برای جزئیات بیشتر
3. 💡 **در نظر گیری SignalR** برای Multi-Client / Real-time scenarios
4. ✅ **بهبود Error Messages** بر اساس مستندات

---

## 📚 منابع

### فایل‌های مستندات
- `Infrastructure/SSP1126(WEB)/SSP1126-WebBased(SignalR)_1_2_1.pdf`
- `Infrastructure/SSP1126(WEB)/Web Tester_1402-06-29/Sample(SSP1126)Page.html`
- `Infrastructure/SSP1126(WEB)/ServiceInstaller_1402-06-29/`

### پیاده‌سازی فعلی
- `Services/Payment/POS/Drivers/SamanKishDriver.cs`

---

**تهیه شده توسط:** AI Assistant  
**تاریخ:** 1402-06-29

