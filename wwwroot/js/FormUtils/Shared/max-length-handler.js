// تابع هندلر فیلد هایی که دیتا مکس لنگت دارند
// فیلد های ما حتما آی دی دارند آی دی نداشت نباید سلکت شود
document.querySelectorAll('input[data-maxlength][id], textarea[data-maxlength][id]').forEach(input => {
  const maxLen = parseInt(input.dataset.maxlength);
  const shouldSlice = input.dataset.allowSlice?.toLowerCase() === "true";
  
  input.addEventListener('input', function () {
    if (this.value.length > maxLen) {
      if (shouldSlice) {
        // اگر باید برش دهد، برش دهد و مانع شود و ارور هم ندهد
        this.value = this.value.slice(0, maxLen);
      } else {
        // اگر نباید برش دهد، اجازه ادامه تایپ دهد ولی همراه با ارور
        replaceError(this.id, `حداکثر ${maxLen} کاراکتر مجاز است`);
      }
    } else {
      // وقتی معتبر شد
      const formGroup = this.closest('.form-group');
      const errorSpan = formGroup?.querySelector('.error-message');
      if (errorSpan) {
        const currentMessage = errorSpan.textContent.trim();
        if (shouldSlice || isMaxLengthError(currentMessage)) { // اگر ارور ماکس لنگت بود پاکش کند
          replaceError(this.id, '');
        }
      }
    }
  });
});

// تابع تشخیص ارور مکس لنگت
function isMaxLengthError(message) {
  const maxLengthRegex = /^حداکثر (\d+) کاراکتر مجاز است$/;
  return maxLengthRegex.test(message);
}

// چک ممکن بودن ورودی کاربر با وجود مکس لنگت
function isPossibleToUser_MaxLength(input) {  // تابع المنت اینپوت رو میگیرد
  if (!input || !input.dataset.maxlength) return true;

  const maxLen = parseInt(input.dataset.maxlength);
  const shouldSlice = input.dataset.allowSlice?.toLowerCase() === "true";

  if (shouldSlice && input.value.length > maxLen) {
    return false;
  }

  return true;
}

