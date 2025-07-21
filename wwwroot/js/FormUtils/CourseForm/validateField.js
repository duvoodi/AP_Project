/* ========================================
   نگاشت فیلدهای وابسته به هر فیلد
   ======================================== */
const fieldDependencies = {
  'Semester': ['FinalExamDate'],
  // 'A': [ids of fields that require the value of field A to check, separated by commas]
};

function checkDependentsOf(inputId) {
  for (const [key, dependents] of Object.entries(fieldDependencies)) {
    if (key === inputId) {
      for (const dep of dependents) {
        validateField(dep, false);
      }
    }
  }
}

function validateExamDateWith_Semester() {
  const examDate = document.getElementById('FinalExamDate')?.value;
  const semester = document.getElementById('Semester')?.value;

  if (!semester || !examDate) return null;

  const result = isValidExamDateWithSemester(examDate, semester);
  
  if (!result.isValid) {
    return { isValid: false, errorMessage: result.errorMessage };
  }

  return { isValid: true, errorMessage: '' };
}


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
  if (inputId === 'PrerequisiteIds') {
    return true;
  }

  // 0. چک نکردن فیلد های اختیاری خالی
  let optionalFields = ['InstructorId', 'Description'];
  if (optionalFields.includes(inputId) && !value) {
    replaceError_IfAllowed(inputId, '');  // خالی هست هیچ ارور ندهد و ولید باشد
    return true;
  }

  // 1. چک خالی بودن فیلدهای اجباری
  const requiredFields = ['CourseCodeId', 'Unit', 'Semester', 'TimeSlotId', 'FinalExamDate'];
  if (requiredFields.includes(inputId)) {
      const tagName = input.tagName?.toLowerCase();
      const isSelect = tagName === 'select';

      if (!value || value.trim() === "") {
          if (showError) {
              replaceError(inputId, isSelect
                  ? "لطفاً یک مورد را انتخاب کنید"
                  : "لطفاً این فیلد را پر کنید");
          }
          return false;
      }
  }

  // 2. چک مکس لنگت ارور
  // 2.1 اگرارور مکس لنگت داره اولیت با اونه اول اون باید برطرف
  if (isMaxLengthError(errorSpanText)) {
    return false;
  }
  // 2.2 اگز بدون ارور و اسلایسی هست نباید برای کاربر بیشتر زدن ممکن باشه
  if (!isPossibleToUser_MaxLength(input)) {
    input.value = '';
    replaceError_IfAllowed(inputId, 'خطا در دریافت ورودی');
    return false;
  }

  // 3. چک های اختصاصی هر فیلد
  switch (inputId) {
    case 'Unit':
      if (!isPossibleToUser_Numeric(value)) {
        input.value = '';
        replaceError_IfAllowed(inputId, 'خطا در دریافت ورودی');
        return false;
      }
      if (!isValidNumeric(value)) {
        replaceError_IfAllowed(inputId, 'ورودی نامعتبر است');
        return false;
      }
      var unitValue = parseInt(value);
      if (unitValue < 1 || unitValue > 4) {
        replaceError_IfAllowed(inputId, 'ورودی باید بین 1 تا 4 باشد');
        return false;
      }
      break;

    case 'Semester':
      if (!isPossibleToUser_Numeric(value)) {
        input.value = '';
        replaceError_IfAllowed(inputId, 'خطا در دریافت ورودی');
        return false;
      }

      const semesterResult = isValidSemesterDate(value);

      if (!semesterResult.isValid) {
        replaceError_IfAllowed(inputId, semesterResult.errorMessage);
        return false;
      }

      break;

    case 'FinalExamDate':
      if (!isPossibleToUser_Date(value)) {
        input.value = '';
        replaceError_IfAllowed(inputId, 'خطا در دریافت ورودی');
        return false;
      }

      if (!isValidPersianDate(value)) {
        replaceError_IfAllowed(inputId, 'تاریخ نامعتبر است');
        return false;
      }

      const examDateResult = validateExamDateWith_Semester();
      if (examDateResult !== null){
        replaceError_IfAllowed(inputId, examDateResult.errorMessage);
        return examDateResult.isValid;
      }

      break;

    case 'Description':
      if (!isPossibleToUser_Text(value)) {
        input.value = '';
        replaceError_IfAllowed(inputId, 'خطا در دریافت ورودی');
        return false;
      }

      if (!isValidText(value)) {
        replaceError_IfAllowed(inputId, 'ورودی نامعتبر است');
        return false;
      }

      break;
  }

  replaceError_IfAllowed(inputId, '');
  checkDependentsOf(inputId);
  return true;
}