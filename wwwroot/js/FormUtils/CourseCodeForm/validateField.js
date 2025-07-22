function validateField(inputId, showError) {
  const input = document.getElementById(inputId);
  if (!input) return false;

  let value = input.value;
  const formGroup = input.closest('.form-group');
  const errorSpan = formGroup?.querySelector('.error-message');
  const errorSpanText = errorSpan ? errorSpan.textContent.trim() : '';

  // تابع جایگزینی خطا (فقط اگر showError=true باشد)
  function replaceError_IfAllowed(message) {
    if (showError) replaceError(inputId, message);
  }

  
  // 1. چک خالی بودن فیلدهای اجباری
  const requiredFields = ['Id', 'Title', 'Code'];
  const isRequired = requiredFields.some(field => inputId.includes(field));

  if (isRequired) {
    value = input?.value?.trim();

    // اگر سلکت باشد و مقدارش خالی باشد
    if (input.tagName === 'SELECT') {
      if (!value) {
        replaceError_IfAllowed('لطفاً یک مورد را انتخاب کنید');
        return false;
      }
    }
    // اگر اینپوت باشد و مقدارش خالی باشد
    else if (!value) {
      replaceError_IfAllowed('لطفاً این فیلد را پر کنید');
      return false;
    }
  }

// 2.1 بررسی خطای طول بیشینه (اولویت دارد)
  if (isMaxLengthError(errorSpanText)) {
    return false;
  }

  // 2.2 بررسی امکان ورود داده توسط کاربر
  if (!isPossibleToUser_MaxLength(input)) {
    input.value = '';
    replaceError_IfAllowed('خطا در دریافت ورودی');
    return false;
  }

  // 3. اعتبارسنجی اختصاصی هر فیلد
  if (inputId.includes('Title')) {
    value = value.trim();
    input.value = value;

    // 3.1. بررسی خالی بودن
    if (!value) {
      replaceError_IfAllowed('لطفاً این فیلد را پر کنید');
      return false;
    }

    // 3.2. بررسی کاراکترهای مجاز
    if (!isPossibleToUser_Name(value)) {
      input.value = '';
      replaceError_IfAllowed('خطا در دریافت ورودی');
      return false;
    }

    // 3.3. بررسی فرمت صحیح عنوان
    if (!isValidName(value)) {
      replaceError_IfAllowed('ورودی نامعتبر است');
      return false;
    }
  } 
  else if (inputId.includes('Code')) {
    // 3.1. بررسی خالی بودن
    if (!value) {
      replaceError_IfAllowed('لطفاً این فیلد را پر کنید');
      return false;
    }

    // 3.2. بررسی کاراکترهای مجاز (فقط عدد)
    if (!isPossibleToUser_Numeric(value)) {
      input.value = '';
      replaceError_IfAllowed('خطا در دریافت ورودی');
      return false;
    }

    // 3.3. بررسی معتبر بودن عدد
    if (!isValidNumeric(value) ) {
      replaceError_IfAllowed('ورودی نامعتبر است');
      return false;
    }
  }

  // 4. پاک‌کردن خطا در صورت معتبر بودن
  replaceError_IfAllowed('');
  return true;
}