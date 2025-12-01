# راهنمای حل مشکل Protocol error: Unknown transport
**تاریخ:** 1402-06-29  
**کد درخواست:** SSP1126(WEB)

---

## 🔍 مشکل: Protocol error: Unknown transport

### خطا:
```
Protocol error: Unknown transport.
```

### علت:
SignalR Client نمی‌تواند Transport مناسب را negotiate کند. این معمولاً به این دلایل رخ می‌دهد:
1. WebSocket پشتیبانی نمی‌شود
2. Transport negotiation ناموفق است
3. نیاز به استفاده از LongPolling به جای WebSocket

---

## ✅ راه‌حل

### 1. استفاده از LongPolling Transport

در `SamanKishSignalRDriver.cs`، باید Transport را به LongPolling تنظیم کنیم:

```csharp
// قبل
_hubConnection = new HubConnection(hubUrl);
await _hubConnection.Start();

// بعد
_hubConnection = new HubConnection(hubUrl);
await _hubConnection.Start(new LongPollingTransport());
```

### 2. Import کردن Transport

```csharp
using Microsoft.AspNet.SignalR.Client.Transports;
```

---

## 🔧 مراحل اصلاح

### 1. به‌روزرسانی SamanKishSignalRDriver.cs

```csharp
using Microsoft.AspNet.SignalR.Client.Transports;

// در متد ConnectToHubAsync
_hubConnection = new HubConnection(hubUrl);
_hubProxy = _hubConnection.CreateHubProxy(HubName);

// Register client callbacks
RegisterClientCallbacks();

// Start connection with LongPolling transport
var startTask = _hubConnection.Start(new LongPollingTransport());
```

### 2. تست اتصال

بعد از تغییر، تست اتصال را انجام دهید.

---

## 📋 توضیحات Transport

### WebSocket (پیش‌فرض)
- سریع‌تر و کارآمدتر
- نیاز به پشتیبانی از WebSocket در Server و Client
- ممکن است در برخی محیط‌ها کار نکند

### LongPolling
- سازگارتر با محیط‌های مختلف
- کندتر از WebSocket
- همیشه کار می‌کند

### Server-Sent Events (SSE)
- یک طرفه (Server → Client)
- برای این مورد مناسب نیست

---

## 🔍 عیب‌یابی

### مشکل: هنوز Protocol error می‌گیرید

**راه‌حل:**
1. بررسی کنید که Service در حال اجرا است
2. بررسی کنید که URL صحیح است (`http://localhost:8080/signalr`)
3. بررسی Log های Service
4. تست با نمونه HTML (Web Tester)

### مشکل: Connection Timeout

**راه‌حل:**
1. افزایش ConnectionTimeout
2. بررسی Network
3. بررسی Firewall

---

## ✅ خلاصه

| مورد | مقدار |
|------|-------|
| **Transport** | `LongPolling` |
| **URL** | `http://localhost:8080/signalr` |
| **Hub Name** | `SSP1126HUB` |

---

**تاریخ:** 1402-06-29  
**وضعیت:** ✅ راه‌حل ارائه شده

