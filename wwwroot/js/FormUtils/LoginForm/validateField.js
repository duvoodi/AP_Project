/* ========================================
  تابع اعتبارسنجی فیلد با آی دی اش
   ======================================== */
function validateField(inputId, showError) {
  const input = document.getElementById(inputId);
  if (!input) return false;
  let value = input.value;
  const formGroup = input.closest('.form-group');
  const errorSpan = formGroup?.querySelector('.error-message');
  const errorSpanText = errorSpan ? errorSpan.textContent.trim() : '';

  function replaceError_IfAllowed(Id, message){
    if(showError) replaceError(Id, message);
    return;
  }

  function appendError_IfAllowed(Id, message){
    if(showError) appendError(Id, message);
    return;
  }

  // 1. چک خالی بودن فیلد های اجباری
  const requiredFields = ['Username', 'Password'];
  if (requiredFields.includes(inputId) && !value) {
    replaceError_IfAllowed(inputId, 'لطفاً این فیلد را پر کنید');
    return false;
  }

  // 2. چک مکس لنگت ارور
  // 2.1 اگرارور مکس لنگت  داره اولیت با اونه اول اون باید برطرف
  if (isMaxLengthError(errorSpanText)) {
    // همون ارور را نگه میداریم
    return false;
  }
  // 2.2 اگز بدون ارور و اسلایسی هست نباید برای کاربر بیشتر زدن ممکن باشه
  if (!isPossibleToUser_MaxLength(input)) {
    // ورودی غیر مجاز را پاک میکنیم
    input.value = '';
    replaceError_IfAllowed(inputId, 'خطا در دریافت ورودی');
    return false;
  }

  // 3. چک های اختصاصی هر فیلد
  switch (inputId) {
  // چک نام کاربری
  case 'Username':
    if (!isPossibleToUser_Username(value)) {
      // ورودی غیر مجاز را پاک میکنیم
      input.value = '';
      replaceError_IfAllowed(inputId, 'خطا در دریافت ورودی');
      return false;
    }
    if (!isValidUsername(value)) {
      replaceError_IfAllowed(inputId, 'ورودی نامعتبر است');
      return false;
    }
    break;

  // چک پسورد
  case 'Password':
    if (!isPossibleToUser_Password(value)) {
      // ورودی غیر مجاز را پاک میکنیم
      input.value = '';
      replaceError_IfAllowed(inputId, 'خطا در دریافت ورودی');
      return false;
    }
    if (!isValidPassword(value)) {
      replaceError_IfAllowed(inputId, 'ورودی نامعتبر است');
      return false;
    }
    break;
  default:
    break;
  }

  // اگر هیچ‌ مورد خطایی نداشت خطای قبلی را پاک کن
  replaceError_IfAllowed(inputId, '');
  return true;
}
