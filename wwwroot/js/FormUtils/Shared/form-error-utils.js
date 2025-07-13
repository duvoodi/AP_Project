// ***************
// توابع مکس لنگت ارور

function isMaxLengthError(msg) {
  return /^حداکثر \d+ کاراکتر مجاز است$/.test(msg);
}
function isPossibleToUser_MaxLength(input) {
  if (!input || !input.dataset.maxlength) return true;

  const maxLen = parseInt(input.dataset.maxlength, 10);
  // بررسی فعال بودن اجازه برش خودکار متن
  const allowSlice = input.dataset.allowSlice?.toLowerCase() === "true";

  if (allowSlice && input.value.length > maxLen) {
    return false;
  }

  return true;
}

function replaceError(id, message) {
  setError(id, message, false);
}

function appendError(id, message) {
  setError(id, message, true);
}

// ***************
// تابع اصلی نمایش یا بروز رسانی پیام خطا با انیمیشن نمایش و مخفی شدن

function setError(id, message, append = false) {
  const inputEl = document.getElementById(id);
  if (!inputEl) return;
  const formGroup = inputEl.closest('.form-group, .error-group');
  if (!formGroup) return;
  const errorSpan = formGroup.querySelector('.error-message');
  if (!errorSpan) return;
  const isErrorGroup = formGroup.classList.contains('error-group');
  const currentMessage = errorSpan.innerHTML.trim();
  const newMessage = message || '';
  if (currentMessage === newMessage) return;

  // تابع داخلی برای بروزرسانی پیام خطا و اجرای انیمیشن نمایش
  function updateMessage() {
    if (newMessage) {
      // اگر افزودن فعال است و پیام قبلی وجود دارد و پیام جدید در پیام قبلی نیست
      if (append && currentMessage && !currentMessage.split('<br>').includes(newMessage)) {
        // افزودن پیام جدید به انتهای پیام قبلی با <br>
        errorSpan.innerHTML = currentMessage + '<br>' + newMessage;
      } else {
        // در غیر این صورت جایگزینی پیام خطا با پیام جدید
        errorSpan.innerHTML = newMessage;
      }

      // اگر فرم گروه از نوع ارور گروپ باشد، ارتفاع ماکسیمم را مطابق ارتفاع محتوای پیام تنظیم کن
      if (isErrorGroup) {
        errorSpan.style.maxHeight = errorSpan.scrollHeight + 32 + 'px';
      }

      // فعال کردن انیمیشن نمایش پیام با افزودن کلاس ویزیبل در فریم بعدی
      requestAnimationFrame(() => {
        errorSpan.classList.add('visible');
      });
    } else {
      // اگر پیام جدید خالی است یعنی باید پیام مخفی شود

      // تنظیم ارتفاع ماکسیمم به صفر برای اجرای انیمیشن ارتفاع در گروه خطا
      if (isErrorGroup) {
        errorSpan.style.maxHeight = '0';
      }

      // حذف کلاس حذف برای شروع انیمیشن مخفی شدن
      requestAnimationFrame(() => {
        errorSpan.classList.remove('visible');
      });

      // حذف متن پیام خطا پس از اتمام انیمیشن در تابع لیسنر مربوط به پایان ترنزیشن انجام می‌شود
    }
  }

  // تابع داخلی برای هندل کردن پایان انیمیشن ترنزیتیشن پیام خطا
  function onTransitionEnd(event) {
    // بررسی اینکه آیا ترنزیتیشن ویژگی اوپسیتی یا مکس هایت تمام شده و اوپستی برابر صفر شده است
    if ((event.propertyName === 'opacity' || event.propertyName === 'max-height') &&
      window.getComputedStyle(errorSpan).opacity === '0') {

      // حذف لیسنر برای جلوگیری از اجرای چندباره
      errorSpan.removeEventListener('transitionend', onTransitionEnd);

      // اگر پیام جدید خالی است، متن پیام را اینجا پاک می‌کنیم
      if (!newMessage) {
        errorSpan.innerHTML = '';
      }

      // اگر پیام جدید داشتیم، پیام جدید را مجدداً آپدیت و نمایش می‌دهیم
      if (newMessage) {
        updateMessage();
      }
    }
  }

  // اگر الان پیام خطا در حالت ویزیبل است، ابتدا انیمیشن مخفی شدن را اجرا کن
  if (errorSpan.classList.contains('visible')) {
    // اضافه کردن لیسنر برای پایان انیمیشن تا پس از مخفی شدن پیام، متن پاک یا بروزرسانی شود
    errorSpan.addEventListener('transitionend', onTransitionEnd, { once: true });
    // شروع انیمیشن مخفی شدن پیام با حذف کلاس ویزیبل
    errorSpan.classList.remove('visible');
  } else {
    // اگر پیام الان مخفی است، مستقیم پیام را آپدیت و نمایش بده
    updateMessage();
  }
}

// ***************
// پاکسازی خودکار پیام خطا هنگام تایپ کاربر (به جز مکس لنگت ارور)

// انتخاب همه المنت‌های ورودی دارای شناسه (input، textarea، select)
document.querySelectorAll('input[id], textarea[id], select[id]').forEach(inputEl => {
  inputEl.addEventListener('input', () => {
    const errorSpan = inputEl.closest('.form-group')?.querySelector('.error-message');
    const currentErrorText = errorSpan?.textContent.trim() || '';
    if (errorSpan && !isMaxLengthError(currentErrorText)) {
      replaceError(inputEl.id, '');
    }
  });
});

// ***************
// کنترل و نمایش پیام خطای محدودیت طول ورودی با امکان برش یا نمایش پیام خطا

// انتخاب تمام اینپوت و تکست اریا هایی که داده حداکثر طول و شناسه دارند
document.querySelectorAll('input[data-maxlength][id], textarea[data-maxlength][id]').forEach(inputEl => {
  const maxLen = parseInt(inputEl.dataset.maxlength, 10);
  const allowSlice = inputEl.dataset.allowSlice?.toLowerCase() === 'true';

  inputEl.addEventListener('input', function () {
    const errorSpan = this.closest('.form-group, .error-group')?.querySelector('.error-message');
    const currentErrorText = errorSpan?.textContent.trim() || '';
    const maxLenErrorMessage = `حداکثر ${maxLen} کاراکتر مجاز است`;

    // اگر طول مقدار ورودی از حد مجاز بیشتر است
    if (this.value.length > maxLen) {
      if (allowSlice) {
        // اگر برش خودکار مجاز است، مقدار را برش بزن به اندازه مجاز
        this.value = this.value.slice(0, maxLen);
      } else if (currentErrorText !== maxLenErrorMessage) {
        // اگر برش مجاز نیست و پیام خطا هنوز نمایش داده نشده است، پیام خطا را نمایش بده
        replaceError(this.id, maxLenErrorMessage);
      }
    } else if (isMaxLengthError(currentErrorText)) {
      // اگر طول مقدار در حد مجاز است و پیام خطای محدودیت طول نمایش داده شده، آن را پاک کن
      replaceError(this.id, '');
    }
  });
});

// ***************
// نمایش پیام‌های خطای دریافتی از سرور هنگام بارگذاری فرم

// پیام‌های درون المنت‌هایی با کلاس 'field-validation-error' خوانده شده و به صورت انیمیشنی نمایش داده می‌شوند
(function () {
  // تابع نمایش پیام خطاهای سرور روی فرم
  function showErrorsFromServer() {
    document.querySelectorAll('.error-message').forEach(errorSpan => {
      const serverErrorText = errorSpan.querySelector('.field-validation-error')?.textContent.trim() || '';
      if (serverErrorText) {
        const relatedInputId = errorSpan.id || errorSpan.closest('.form-group')?.querySelector('[id]')?.id;
        if (relatedInputId) {
          // نمایش با انیمیشن پیام خطا آمده از سمت سرور درصورت تغییر
          replaceError(relatedInputId, serverErrorText); // اگر پیام تغییر کرده باشه ریپلیسش میکنه با انیمیشن اول ناپدیدش میکند سپس پیام جدید را نمایش میدهد
        }
      }
    });
  }

  // بررسی وضعیت آماده بودن صفحه برای اجرا
  if (document.readyState === 'loading') {
    // اگر صفحه در حال بارگذاری است، منتظر اتمام بارگذاری DOM بمان
    document.addEventListener('DOMContentLoaded', () => setTimeout(showErrorsFromServer, 300));
  } else {
    // اگر صفحه آماده است، پس از 300 میلی‌ثانیه تابع نمایش پیام خطاهای سرور را اجرا کن
    setTimeout(showErrorsFromServer, 300);
  }
})();
