/* ========================================
   نگاشت فیلدهای وابسته به هر فیلد
   ======================================== */
const fieldDependencies = {
  'Password': ['ConfirmPassword'],
  // 'A': [ids of fields that require the value of field A to check, separated by commas]
};

function checkDependentsOf(inputId) {
  for (const [key, dependents] of Object.entries(fieldDependencies)) {
    if (key === inputId) {
      for (const dep of dependents) {
        validateField(dep, false); // ارورهای وابسته فقط اگر خودشون مشکل داشته باشن نشون داده میشه
      } // وابسته های به پسورد باید بروز بشن بدون نمایش ارور اگر مقدار درست داشتند چک وابسته به پسوردشون فروخوانی و ارورش بروز 
    } // مقدار غلط داشتند بدون نمایش ارور بیرون میره چون نایش ارور اون باید با بلر خودش باشه
  }
}


function validateConfirmPasswordWith_Password() {
  const pw = document.getElementById('Password')?.value;
  const cpw = document.getElementById('ConfirmPassword')?.value;

  if (!pw || !cpw) return null;

  if (pw !== cpw) {
    replaceError('ConfirmPassword', 'ورودی با رمز عبور مطابقت ندارد');
    return false;
  }

  replaceError('ConfirmPassword', '');
  return true;
}



/* ========================================
  تابع اعتبارسنجی فیلد با آی دی اش
   ======================================== */
function validateField(inputId, showError, AllowOptionalFieldsInEdit = false) {
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

  // 0. چک نکردن فیلد های اختیاری خالی
  let optionalFields = [];
  if (AllowOptionalFieldsInEdit) optionalFields.push('Password', 'ConfirmPassword');  
  if (optionalFields.includes(inputId) && !value) {
    // خالی هست هیچ ارور ندهد و ولید باشد
    replaceError_IfAllowed(inputId, '');
    return true;
  }

  // 1. چک خالی بودن فیلد های اجباری
  const requiredFields = ['FirstName', 'LastName', 'Email', 'HireYear', 'Salary', 'Password', 'ConfirmPassword'];
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
  // چک نام
  // چک نام خانوادگی
  case 'FirstName':
  case 'LastName':
    // حذف فاصله آخر بعد از خروج از فیلد
    value = value.trim()
    input.value = value;
    if (!isPossibleToUser_PersonName(value)) {
      // ورودی غیر مجاز را پاک میکنیم
      input.value = '';
      replaceError_IfAllowed(inputId, 'خطا در دریافت ورودی');
      return false;
    }
    if (!isValidPersonName(value)) {
      replaceError_IfAllowed(inputId, 'ورودی نامعتبر است');
      return false;
    }
    break;

  // چک ایمیل
  case 'Email':
    if (!isPossibleToUser_Email(value)) {
      // ورودی غیر مجاز را پاک میکنیم
      input.value = '';
      replaceError_IfAllowed(inputId, 'خطا در دریافت ورودی');
      return false;
    }
    if (!isValidEmail(value)) {
      replaceError_IfAllowed(inputId, 'ورودی نامعتبر است');
      return false;
    }
    break;

  case 'InstructorId':
    if (!validateField('HireYear', false)) { // اول بررسی معتبر بودن سال استخدام بدون نمایش ارور
      replaceError('InstructorId', ''); // سال اشتباهه درخواست به سرور نمیده و ارور دریافت هم نباید داشته باشد
      input.value = ''; // برای سال اشتباه نباید کدی نمایش بده
      return false;
    }
    const enteredHireYear = document.getElementById('HireYear')?.value;
    // امکان نداارد کد موارد زیر باشد
    if (!isValidNumeric(value) || value.length !== 9) {
      // ورودی غیر مجاز را پاک میکنیم
      input.value = '';
      replaceError_IfAllowed(inputId, 'خطا در دریافت کد مدرسی');
      return false;
    }
    const prefixExpected = enteredHireYear.substring(1, 4);
    const prefixActual = value.substring(0, 3);
    const mid = parseInt(value.charAt(3));
    // امکان نداارد کد موارد زیر باشد
    if (prefixActual !== prefixExpected || mid < 5 || mid > 9) {
      // ورودی غیر مجاز را پاک میکنیم
      input.value = '';
      replaceError_IfAllowed(inputId, 'خطا در دریافت کد مدرسی');
      return false;
    }
    break;

  // چک حقوق
  case 'Salary':
    if (!isPossibleToUser_Numeric(value)) {
      // ورودی غیر مجاز را پاک میکنیم
      input.value = '';
      replaceError_IfAllowed(inputId, 'خطا در دریافت ورودی');
      return false;
    }
    if (!isValidNumeric(value)) {
      replaceError_IfAllowed(inputId, 'ورودی نامعتبر است');
      return false;
    }

    break;

  // چک سال استخدام
  case 'HireYear':
    if (!isPossibleToUser_Numeric(value)) {
      // ورودی غیر مجاز را پاک میکنیم
      input.value = '';
      replaceError_IfAllowed(inputId, 'خطا در دریافت ورودی');
      return false;
    }
    if (!isValidNumeric(value)) {
      replaceError_IfAllowed(inputId, 'ورودی نامعتبر است');
      return false;
    }
    if (value.replace(/^0+/, '').length < 4) {
      replaceError_IfAllowed(inputId, 'ورودی خارج از بازه مجاز است');
      return false;
    }
    const parsedHireYear = parseInt(value, 10);
    if (parsedHireYear < 1300 || parsedHireYear > currentPersianYear + 1) { // تا سال بعدی رو میتواند زودتر وارد کند
      replaceError_IfAllowed(inputId, 'ورودی خارج از بازه مجاز است');
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
    
  // چک تکرار پسورد
  case 'ConfirmPassword':
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
    const result = validateConfirmPasswordWith_Password();
    if (result !== null) return result;
    break;

  default:
    break;
  }

  // اگر هیچ‌ مورد خطایی نداشت خطای قبلی را پاک کن
  replaceError_IfAllowed(inputId, '');

  checkDependentsOf(inputId); // بررسی موارد وابسته به مورد ما چون ارورشان باید بروز شود

  return true;
}