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

      // فعال کردن انیمیشن نمایش پیام با افزودن کلاس ویزیبل در فریم بعدی
      requestAnimationFrame(() => {
        errorSpan.classList.add('visible');
      });
    } else {
      // اگر پیام جدید خالی است یعنی باید پیام مخفی شود
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
// ارور های فرم گروپ با 300 میلی ثانیه تاخیر (برای 250 میلی ثانیه انیمیشن)
// ارور جنرال ارور با 850 میلی ثانیه تاخیر (برای 800 میلی ثانیه انیمیشن)


(function () {
  // تابع نمایش پیام خطاهای سرور روی فرم
  function showErrorsFromServer() {
    document.querySelectorAll('.error-message').forEach(errorSpan => {
      const serverErrorEl = errorSpan.querySelector('.field-validation-error'); // ارور های سمت سرور درون این المنت قرار میگیرن
      const serverErrorText = serverErrorEl?.textContent.trim() || '';
      if (!serverErrorText) return;

      // پیدا کردن آی‌دی ورودی مربوط به این ارور
      const relatedInputId = errorSpan.id || errorSpan.closest('.form-group')?.querySelector('[id]')?.id;
      // اگر ورودی‌ای پیدا نشد، یعنی جنرال ارور است
      const isGeneralError = !relatedInputId;

      // تنظیم تاخیر بر اساس نوع ارور
      const delay = isGeneralError ? 850 : 300;

      setTimeout(() => { // صدا زدن replaceError با تاخیر مناسب
        if (relatedInputId) {
          replaceError(relatedInputId, serverErrorText);
        } else {
          // برای جنرال ارور می‌تونید یک المان placeholder در صفحه داشته باشید
          // یا مثلاً المان .general-error-message
          const genEl = document.querySelector('.general-error-message');
          if (genEl) {
            // فرض می‌کنیم کلاس visible و مکانیزم انیمیشن یکسان است
            replaceError(genEl.id, serverErrorText);
          }
        }
      }, delay);
    });
  }

  // اجرا پس از بارگذاری DOM
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', showErrorsFromServer);
  } else {
    showErrorsFromServer();
  }
})();
