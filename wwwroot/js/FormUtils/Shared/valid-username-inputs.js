// تنظیمات اینپوت‌های ولید یوزرنیم
document.querySelectorAll('.valid-username').forEach(input => {
  // جلوگیری از تایپ کاراکترهای غیرمجاز
  input.addEventListener('keydown', function (e) {
    const value = this.value;
    const cursorPos = this.selectionStart;

    // اجازه به کلیدهای سیستمی و کنترلی
    if (
      e.ctrlKey || e.metaKey ||
      e.key === "Backspace" || e.key === "Delete" ||
      e.key === "ArrowLeft" || e.key === "ArrowRight" ||
      e.key === "ArrowUp" || e.key === "ArrowDown" ||
      e.key === "Tab" || e.key === "Home" || e.key === "End"
    ) return;

    // جلوگیری از تایپ فاصله
    if (e.key === " ") {
      e.preventDefault();
      return;
    }

    // کاراکترهای مجاز: حروف انگلیسی ارقام آندرلاین و نقطه
    const allowedChars = /^[a-zA-Z0-9_.]$/;
    if (!allowedChars.test(e.key)) {
      e.preventDefault();
    }
  });

  // پاکسازی ورودی در صورت پیست یا تغییر مستقیم
  input.addEventListener('input', function () {
    let cleaned = this.value
      .replace(/[^a-zA-Z0-9_.]/g, '')  // حذف کاراکتر غیرمجاز
      .replace(/(\.\.+|__+)/g, '.')    // تبدیل __ یا .. به یک نقطه

    this.value = cleaned;
  });
});

// بررسی امکان‌پذیری یوزرنیم (ورودی کاربر معتبر باشد یا نه)
function isPossibleToUser_Username(value) {
  if (!value) return true;

  // نباید فاصله داشته باشد
  if (/\s/.test(value)) return false;

  // نباید شامل کاراکترهای غیرمجاز باشد
  if (!/^[a-zA-Z0-9_.]+$/.test(value)) return false;

  // نباید شامل __ یا .. باشد
  if (/(\.\.|__)/.test(value)) return false;

  return true;
}

// بررسی ولید بودن نهایی یوزرنیم
function isValidUsername(value) {
  if (!value) return false;
  return isPossibleToUser_Username(value);
}
