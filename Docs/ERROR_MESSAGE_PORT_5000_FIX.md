# گزارش اصلاح پیام‌های خطا: تغییر Port از 8080 به 5000

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ **اصلاح شد**

---

## 🔍 مشکل شناسایی شده

### مشکل:
پیام‌های خطا در JavaScript هنوز به Port 8080 اشاره می‌کردند، در حالی که Service روی Port 5000 listen می‌کند.

### خطا:
```
عدم امکان بارگذاری SignalR Hubs.

🔍 وضعیت:
• Service در حال اجرا است (Status: Running)
• اما Port 8080 باز نیست (netstat خروجی ندارد)
```

---

## ✅ راه‌حل اعمال شده

### 1. اصلاح پیام خطا در pos-payment-client.js

**قبل:**
```javascript
'3. بررسی کنید که Service روی Port 8080 listen می‌کند:\n' +
'   PowerShell: netstat -ano | findstr :8080\n' +
```

**بعد:**
```javascript
'3. بررسی کنید که Service روی Port 5000 listen می‌کند:\n' +
'   PowerShell: netstat -ano | findstr :5000 | findstr LISTENING\n' +
```

**قبل:**
```javascript
troubleshooting: {
    serviceName: 'SSP1126Service1',
    port: 8080,
    ...
}
```

**بعد:**
```javascript
troubleshooting: {
    serviceName: 'SSP1126Service1',
    port: 5000,
    ...
}
```

### 2. اصلاح پیام خطا در Views/PosTest/Index.cshtml

**قبل:**
```javascript
errorMessage += '• اما Port 8080 باز نیست (netstat خروجی ندارد)\n\n';
errorMessage += '2. بررسی کنید که Service روی Port 8080 listen می‌کند:\n';
errorMessage += '   PowerShell: netstat -ano | findstr :8080 | findstr LISTENING\n\n';
```

**بعد:**
```javascript
errorMessage += '• اما Port 5000 باز نیست (netstat خروجی ندارد)\n\n';
errorMessage += '2. بررسی کنید که Service روی Port 5000 listen می‌کند:\n';
errorMessage += '   PowerShell: netstat -ano | findstr :5000 | findstr LISTENING\n\n';
```

---

## 📋 چک‌لیست

- [x] Web.config به Port 5000 تغییر یافت
- [x] Views به Port 5000 تغییر یافت
- [x] JavaScript files به Port 5000 تغییر یافت
- [x] CSP به Port 5000 تغییر یافت
- [x] پیام‌های خطا به Port 5000 تغییر یافت
- [ ] Application Pool Restart شده است
- [ ] تست در Application موفق است

---

## ⚠️ نکات مهم

1. **Port 5000:** Service به صورت پیش‌فرض روی Port 5000 listen می‌کند
2. **پیام‌های خطا:** باید به Port صحیح (5000) اشاره کنند
3. **Restart:** بعد از تغییرات، Application Pool باید Restart شود

---

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ پیام‌های خطا به Port 5000 تغییر یافت

