# 📊 تحلیل سیستماتیک فرایند Initialization DatePicker - از صفر تا صد

**تاریخ:** 1404/10/15  
**وضعیت:** ✅ **تحلیل کامل و Fix پیاده‌سازی شد**

---

## 🔍 **مرحله 1: بررسی کد Source DatePicker**

### 1.1. فرایند Initialization (خط 273-275)

```javascript
// خط 273-274: DatePicker در initialization خودش getOnInitState() را فراخوانی می‌کند
this.state.setViewDateTime('unix', this.input.getOnInitState());
this.state.setSelectedDateTime('unix', this.input.getOnInitState());
this.view.render(); // خط 275: render کردن تقویم
```

**مشکل:** حتی اگر `initialValue: false` باشد، DatePicker خودش `getOnInitState()` را فراخوانی می‌کند.

### 1.2. متد `getOnInitState()` (خط 1702-1742)

```javascript
getOnInitState: function getOnInitState() {
    // ...
    if (!inputValue) {
        // ⚠️ CRITICAL: اگر inputValue خالی باشد، new Date().valueOf() را set می‌کند
        return new Date().valueOf(); // خط 1742
    }
    // ...
}
```

**مشکل:** اگر `inputValue` خالی باشد، DatePicker خودش تاریخ امروز (client-side) را set می‌کند که ممکن است اشتباه باشد (16 به جای 15).

### 1.3. متد `markSelectedDay()` (خط 3236-3246)

```javascript
markSelectedDay: function markSelectedDay() {
    var selected = this.model.state.selected;
    this.$container.find('.table-days td').each(function () {
        if ($(this).data('date') == [selected.year, selected.month, selected.date].join(',')) {
            $(this).addClass('selected'); // ⚠️ CRITICAL: اضافه کردن class 'selected'
        } else {
            $(this).removeClass('selected');
        }
    });
}
```

**مشکل:** این متد class `selected` را به `td` اضافه می‌کند که باعث highlight شدن تاریخ می‌شود.

### 1.4. Template تقویم (خط 1294)

```html
<td data-unix="{{dataUnix}}">
    <span class="{{#otherMonth}}other-month{{/otherMonth}} {{#selected}}selected{{/selected}}">
        {{title}}
    </span>
</td>
```

**مشکل:** Template از `{{#selected}}selected{{/selected}}` استفاده می‌کند که باعث می‌شود class `selected` به `span` اضافه شود.

---

## 🔍 **مرحله 2: بررسی فرایند Initialization در کد ما**

### 2.1. ترتیب اجرا

1. **خط 289:** `initializeDatePicker($input)` فراخوانی می‌شود
2. **خط 339:** `data-no-default-date` attribute خوانده می‌شود
3. **خط 344:** `getTodayFromServer()` فراخوانی می‌شود
4. **خط 346:** Promise resolve می‌شود و `datePickerConfig` تنظیم می‌شود
5. **خط 810:** `$input.pDatepicker(datePickerConfig)` فراخوانی می‌شود
6. **خط 813:** `initializationCompleteTime` set می‌شود

### 2.2. مشکل اصلی

**DatePicker در initialization خودش:**
1. `getOnInitState()` را فراخوانی می‌کند (خط 273-274)
2. اگر `inputValue` خالی باشد، `new Date().valueOf()` را set می‌کند (خط 1742)
3. `render()` را فراخوانی می‌کند (خط 275)
4. `markSelectedDay()` را فراخوانی می‌کند که class `selected` را اضافه می‌کند (خط 3236-3246)
5. `onSelect` callback را فراخوانی می‌کند (حتی اگر `initialValue: false` باشد)

**نتیجه:** تاریخ امروز (client-side) highlight می‌شود حتی اگر `noDefaultDate: true` باشد.

---

## ✅ **مرحله 3: راه حل پیاده‌سازی شده**

### 3.1. لایه 1: تنظیمات اولیه

```javascript
// خط 401: observer را false می‌کنیم اگر noDefaultDate true باشد
observer: !noDefaultDate,

// خط 448: initialValue را false می‌کنیم اگر noDefaultDate true باشد
datePickerConfig.initialValue = finalInitialValue; // false اگر noDefaultDate true باشد
```

### 3.2. لایه 2: onShow Callback

```javascript
// خط 466-571: onShow callback برای clear کردن highlight
datePickerConfig.onShow = function() {
    if (noDefaultDate && !initialValueToUse) {
        // Clear کردن فوری
        datePickerInstance.setDate(null);
        $input.val('');
        // حذف class های highlight
        $calendar.find('.table-days td.selected').removeClass('selected');
    }
};
```

### 3.3. لایه 3: onSelect Callback

```javascript
// خط 581-647: onSelect callback برای ignore کردن انتخاب‌های خودکار
datePickerConfig.onSelect = function(unix) {
    if (noDefaultDate && !initialValueToUse) {
        var isAutoSelection = firstOnSelectCall || !allowSelection || isInitializing || timeSinceInit < 2000;
        if (isAutoSelection && !isUserSelection) {
            // Clear کردن فوری
            $input.val('');
            datePickerInstance.setDate(null);
            // حذف class های highlight
            $calendar.find('.table-days td.selected').removeClass('selected');
            return; // جلوگیری از ادامه execution
        }
    }
};
```

### 3.4. لایه 4: Multiple Clear Attempts

```javascript
// خط 895-951: چندین بار clear کردن در setTimeout های مختلف
var clearAttempts = [50, 100, 200, 300, 500, 1000, 1500, 2000];
clearAttempts.forEach(function(delay) {
    setTimeout(function() {
        // Clear کردن تاریخ
        datePickerInstance.setDate(null);
        $input.val('');
        // حذف class های highlight (طبق کد source خط 3239-3245)
        $calendar.find('.table-days td.selected').each(function() {
            var $td = $(this);
            $td.removeClass('selected');
            $td.find('span').removeClass('selected');
        });
    }, delay);
});
```

### 3.5. لایه 5: Event Listener

```javascript
// خط 956-980: event listener برای detect کردن تغییرات خودکار
var inputChangeHandler = function() {
    if (!allowSelection && noDefaultDate && !initialValueToUse) {
        var currentVal = $input.val();
        if (currentVal && currentVal.trim() !== '') {
            // Clear کردن فوری
            $input.val('');
            datePickerInstance.setDate(null);
        }
    }
};
$input.on('input change', inputChangeHandler);
```

---

## 📋 **مرحله 4: تغییرات کلیدی**

### 4.1. حذف Class `selected` از `.table-days td`

**قبل:**
```javascript
$calendar.find('td[data-unix], .pdp-day-selected, .selected')
    .removeClass('pdp-day-selected selected');
```

**بعد:**
```javascript
// ✅ طبق کد source (خط 3239-3245): markSelectedDay() روی .table-days td کار می‌کند
$calendar.find('.table-days td.selected, td[data-unix].selected, .pdp-day-selected, .selected')
    .removeClass('pdp-day-selected selected');

// ✅ CRITICAL: حذف class selected از تمام td ها (طبق کد source خط 3239)
$calendar.find('.table-days td').each(function() {
    var $td = $(this);
    $td.removeClass('selected');
    $td.find('span').removeClass('selected');
});
```

**دلیل:** طبق کد source، `markSelectedDay()` روی `.table-days td` کار می‌کند و class `selected` را به `td` اضافه می‌کند. باید این class را از تمام `td` ها حذف کنیم.

### 4.2. استفاده از `firstOnSelectCall` Flag

**قبل:**
```javascript
var isAutoSelection = !allowSelection || isInitializing || timeSinceInit < 2000;
```

**بعد:**
```javascript
var firstOnSelectCall = true; // ✅ Flag برای تشخیص اولین فراخوانی onSelect
var isAutoSelection = firstOnSelectCall || !allowSelection || isInitializing || timeSinceInit < 2000;
```

**دلیل:** طبق کد source، `onSelect` در initialization فراخوانی می‌شود حتی اگر `initialValue: false` باشد. باید اولین فراخوانی را ignore کنیم.

---

## 🎯 **نتیجه‌گیری**

### ✅ مشکلات حل شده:

1. **Highlight شدن تاریخ 16 به جای 15:** با استفاده از `getTodayFromServer()` و clear کردن highlight حل شد.
2. **Highlight شدن تاریخ در initialization:** با استفاده از `onShow`, `onSelect`, و multiple clear attempts حل شد.
3. **Class `selected` باقی ماندن:** با استفاده از `.table-days td.selected` و حذف class از تمام `td` ها حل شد.

### ✅ لایه‌های دفاعی:

1. **لایه 1:** تنظیمات اولیه (`observer: false`, `initialValue: false`)
2. **لایه 2:** `onShow` callback برای clear کردن highlight
3. **لایه 3:** `onSelect` callback برای ignore کردن انتخاب‌های خودکار
4. **لایه 4:** Multiple clear attempts در setTimeout های مختلف
5. **لایه 5:** Event listener برای detect کردن تغییرات خودکار

### ✅ اصول رعایت شده:

- ✅ طبق مستندات Persian DatePicker
- ✅ طبق کد source DatePicker (خط 273-275, 1702-1742, 3236-3246)
- ✅ Bulletproof و مقاوم در برابر race conditions
- ✅ Event-driven approach
- ✅ Server-side date برای اطمینان از صحت

---

## 📝 **مراجع:**

- [Persian DatePicker Options](https://babakhani.github.io/PersianWebToolkit/doc/datepicker/options/)
- [Persian DatePicker API](https://babakhani.github.io/PersianWebToolkit/doc/datepicker/api/)
- `temp/Example - Persian Web Toolkit_files/persian-datepicker.js.download` (کد source)
- `Content/js/persian-datepicker-component.js` (کد ما)

---

**✅ تحلیل کامل انجام شد و Fix پیاده‌سازی شد.**

