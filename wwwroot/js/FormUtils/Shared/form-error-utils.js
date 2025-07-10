// تابع جایگزینی خطا
function replaceError(elementId, message) {
  const element = document.getElementById(elementId);
  if (!element) return;

  // فرم گروپ یا ارور گروپ را کلاس والد میگیریم
  const Group = element.closest('.form-group, .error-group');;
  if (!Group) return;

  const errorSpan = Group.querySelector('.error-message');
  if (!errorSpan) return;

  if (message) {
    // اگر پیام قبلاً نمایش داده نشده، انیمیشن ورود با تأخیر
    if (!errorSpan.classList.contains('visible')) {
      errorSpan.classList.remove('visible');
      errorSpan.textContent = '';
      // تاخیر برای اجرای ترنزیشن نمایشی
      setTimeout(() => {
        errorSpan.textContent = message;
        errorSpan.classList.add('visible');
      }, 10);
    } else {
      // اگر قبلاً نمایش داده می‌شد، مستقیم پیام را تغییر بده
      errorSpan.textContent = message;
    }
  } else {
    // حذف پیام با انیمیشن محو شدن
    errorSpan.classList.remove('visible');
    setTimeout(() => {
      errorSpan.textContent = '';
    }, 250); // مدت زمان انیمیشن در CSS
  }
}

// تابع افزون خطا به خط بعد
function appendError(elementId, message) {
  const element = document.getElementById(elementId);
  if (!element) return;

  // فرم گروپ یا ارور گروپ را کلاس والد میگیریم
  const Group = element.closest('.form-group, .error-group');;
  if (!Group) return;

  const errorSpan = Group.querySelector('.error-message');
  if (!errorSpan) return;

  const existingMessages = errorSpan.innerHTML.split('<br>').map(m => m.trim());
  if (existingMessages.includes(message)) return;  // از قبل بود، اضافه نکن

  if (errorSpan.innerHTML.trim()) {
    // پیام قبلی وجود دارد، پیام جدید را به خط بعدی اضافه می‌کنیم
    errorSpan.innerHTML += `<br>${message}`;
    if (!errorSpan.classList.contains('visible')) {
      // اگر visible نبود (مثل اولین بار)، با تاخیر اضافه کن
      setTimeout(() => errorSpan.classList.add('visible'), 10);
    }
  } else {
    // اگر پیام جدید هست، از replaceError استفاده کنیم که انیمیشن داشته باشه
    replaceError(elementId, message);
  }
}

// حذف ارور فیلد ها زمانی که تغییر میکند
// فیلد های ما حتما آی دی دارند آی دی نداشت نباید سلکت شود
document.querySelectorAll('input[id], textarea[id], select[id]').forEach(input => {
  input.addEventListener('input', () => {
    // فقط فورم گروپ ها فیلد ورودی دارند، ارور گروپ ها مخصوص کادر ارور اند
    const formGroup = input.closest('.form-group');;
    const errorSpan = formGroup?.querySelector('.error-message');
    if (errorSpan) {
      // اگر ارور مکس لنگت بود، حذفش نکنیم
      // ارور مکس لنگت نباید پاک شود این وظیفه خودش است
      if (isMaxLengthError(errorSpan.textContent.trim())) return;
      // انیمیشن محو شدن خطا
      errorSpan.classList.remove('visible');
      setTimeout(() => {
        // اگر ارور مکس لنگت بود، حذفش نکنیم
        // ارور مکس لنگت نباید پاک شود این وظیفه خودش است
        if (isMaxLengthError(errorSpan.textContent.trim())) return;
        errorSpan.textContent = '';
      }, 250); // مطابق مدت زمان transition در CSS
    }
  });
});


// نمایش خطا های دریافتی از سرور
(function() {
  function showErrors() {
    document.querySelectorAll('.error-message').forEach(errorSpan => {
      const innerValidation = errorSpan.querySelector('.field-validation-error');
      const text = innerValidation?.textContent.trim() || '';
      if (text.length > 0) {
        errorSpan.classList.add('visible');
      }
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
      setTimeout(showErrors, 100);
    });
  } else {
    setTimeout(showErrors, 100);
  }
})();
