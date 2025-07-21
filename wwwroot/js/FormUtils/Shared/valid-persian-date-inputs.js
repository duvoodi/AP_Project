document.querySelectorAll('.valid-persian-date').forEach(input => {
  // اجازه ورود اعداد و اسلش
  input.addEventListener('keydown', function(e) {
    const allowedKeys = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '/', 'Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'Tab'];
    if (!allowedKeys.includes(e.key) && !e.ctrlKey && !e.metaKey) {
      e.preventDefault();
    }
  });

  // پردازش پیست و حذف کاراکترهای غیرمجاز
  input.addEventListener('input', function() {
    this.value = this.value.replace(/[^0-9/]/g, '');
  });
});

function isPossibleToUser_Date(str) {
  return /^[0-9/]*$/.test(str); // فقط اعداد و اسلش
}

function isValidPersianDate(dateStr) {
    // انتظار: فرمت "yyyy/mm/dd"
    const parts = dateStr.split('/');
    if (parts.length !== 3) return false;

    const [year, month, day] = parts.map(Number);
    if (isNaN(year) || isNaN(month) || isNaN(day)) return false;

    if (year < 1300 || year > 1500) return false;
    if (month < 1 || month > 12) return false;
    if (day < 1 || day > 31) return false;

    // بررسی ساده برای ماه‌ها (پیشرفته‌تر هم میشه کرد)
    const maxDays = [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29];
    if (day > maxDays[month - 1]) return false;

    return true;
}

function isValidSemesterDate(semester) {
    let errorMessage = null;

    if (!semester || semester.length !== 5) {
        errorMessage = "نیم‌سال باید عددی ۵ رقمی باشد";
        return { isValid: false, errorMessage };
    }

    const yearPart = parseInt(semester.substring(0, 4), 10);
    const semesterPart = parseInt(semester[4], 10);

    if (yearPart < 1300) {
        errorMessage = "سال ترم باید از 1300 به بعد باشد";
        return { isValid: false, errorMessage };
    }

    if (semesterPart < 1 || semesterPart > 3) {
        errorMessage = "بخش ترم باید بین 1 تا 3 باشد";
        return { isValid: false, errorMessage };
    }

    return { isValid: true, errorMessage: null };
}

function isValidExamDateWithSemester(examDate, semester) {
    let errorMessage = null;

    if (!isValidPersianDate(examDate)) {
        errorMessage = "تاریخ نامعتبر است";
        return { isValid: false, errorMessage };
    }

    const [examYearStr, examMonthStr] = examDate.split('/');
    const examYear = parseInt(examYearStr, 10);
    const examMonth = parseInt(examMonthStr, 10);

    if (!semester || semester.length !== 5 || isNaN(semester)) {
        return { isValid: true, errorMessage: null };
    }

    const yearPart = parseInt(semester.substring(0, 4), 10);
    const semesterPart = parseInt(semester[4], 10);

    if (yearPart < 1300 || semesterPart < 1 || semesterPart > 3) {
        return { isValid: true, errorMessage: null };
    }

    const yearMatches =
        (semesterPart === 1 && examYear === yearPart) ||
        (semesterPart !== 1 && examYear === yearPart + 1);

    if (!yearMatches) {
        errorMessage = "سال تاریخ امتحان با ترم همخوانی ندارد";
        return { isValid: false, errorMessage };
    }

    const monthMatchesSemester = {
        1: examMonth === 10 || examMonth === 11,
        2: examMonth === 3 || examMonth === 4,
        3: examMonth === 5 || examMonth === 6
    }[semesterPart] || false;

    if (!monthMatchesSemester) {
        errorMessage = "ماه تاریخ امتحان با ترم همخوانی ندارد";
        return { isValid: false, errorMessage };
    }

    return { isValid: true, errorMessage: null };
}