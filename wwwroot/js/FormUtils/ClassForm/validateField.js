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

  function replaceError_IfAllowed(Id, message) {
    if (showError) replaceError(Id, message);
    return;
  }

  function appendError_IfAllowed(Id, message) {
    if (showError) appendError(Id, message);
    return;
  }

  // ارور لیست دروس پیش‌نیاز تو خود صفحه هندل کردیم و اینجا اسکیپش میکنیم
  // تو صفحه تو اد و دلیت تگ و سابمیت پیاده کردیم
  if (inputId === 'StudentIds') {
    return true;
  }

  // 1. چک خالی بودن فیلدهای اجباری
  const requiredFields = ['ClassLocationId', 'SectionId'];
  if (requiredFields.includes(inputId)) {

      if (!value || value.trim() === "") {
          if (showError) {
              replaceError(inputId, "لطفاً یک مورد را انتخاب کنید");
          }
          return false;
      }
  }

  replaceError_IfAllowed(inputId, '');
  return true;
}