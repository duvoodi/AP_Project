// تنظیمات اینپوت های ولید نومریک
document.querySelectorAll('.valid-numeric').forEach(input => {
  // جلو گیری از تایپ کاراکتر های غیر مجاز
  input.addEventListener('keydown', function (e) {
    // اجازه به کلیدهای سیستمی و کنترلی
    if (
      e.ctrlKey || e.metaKey ||
      e.key === "Backspace" || e.key === "Delete" ||
      e.key === "ArrowLeft" || e.key === "ArrowRight" ||
      e.key === "ArrowUp" || e.key === "ArrowDown" ||
      e.key === "Tab" || e.key === "Home" || e.key === "End"
    ) {
      return; // جلوگیری از ادامه چک و اجازه به اینها
    }
    // کاراکترهای مجاز: فقط ارقام
    const allowedChars = /^[0-9]$/;
    if (!allowedChars.test(e.key)) {
      e.preventDefault(); // جلوگیری از ورود کاراکتر غیر مجاز
    }
  });
  // جلو گیری از پیست یا ورود به هر طریق کاراکتر های غیر مجاز
  input.addEventListener('input', function () {
    const invalidFinder = /[^0-9]/g;
    this.value = this.value.replace(invalidFinder, '');
  });
});

// چک ممکن بودن ورودی برای کاربر در فیلد ولید نومریک
function isPossibleToUser_Numeric(value) {
  // کاربر میتونه ورودی رو خالی بگذاره
  if (!value) return true;
  const allowed = /^[0-9]*$/;
  return allowed.test(value);
}

// تابع برای بررسی ولید نومریک بودن
function isValidNumeric(value) {
  if (!value) return false;
  return isPossibleToUser_Numeric(value);
}