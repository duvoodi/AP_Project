// تنظیمات اینپوت‌های ولید تکست
document.querySelectorAll('.valid-text').forEach(input => {
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

    // جلوگیری از فاصله در ابتدای متن یا فاصله متوالی
    if (e.key === " ") {
      if (cursorPos === 0 || value[cursorPos - 1] === " ") {
        e.preventDefault();
        return;
      }
    }

    // علائم مجاز: حروف فارسی و انگلیسی، اعداد، فاصله، آندرلاین، علائم نگارشی رایج
    const allowedChars = /^[\u0600-\u06FFa-zA-Z0-9 ؛،.,!?؟()\[\]{}\-_–—]$/;
    if (!allowedChars.test(e.key)) {
      e.preventDefault();
    }
  });

  // جلوگیری از پیست یا ورودی غیرمجاز
  input.addEventListener('input', function () {
    const invalidFinder = /[^\u0600-\u06FFa-zA-Z0-9 ؛،.,!?؟()\[\]{}\-_–—]/g;
    let cleaned = this.value
      .replace(invalidFinder, '') // حذف کاراکترهای غیرمجاز
      .replace(/ +/g, ' ')        // حذف فاصله‌های متوالی
      .replace(/^ /, '');         // حذف فاصله ابتدای متن

    this.value = cleaned;
  });
});

/* فاصله آخر مجاز است برای اینکه کاربر بتواند بین کلمات اسپیس بزند
* با بیرون آمدن کاربر از فیلد یعنی با ایونت بلر باید فاصله اخر حذف شود
* اگر این مورد رو در ایونت بلر بزنیم با ایونت بلر ولیدیت فیلد ها تداخل میکند
* با خروج کاربر اگر ارور مکس لنگت داشته باشیم این حذف فاصله آخر ممکن است طول  را مجاز کند و ارور یهویی بپرد
* پس باید داخل همان ایونت بلر ولیدیت فیلد ها بعد از ارور مکس لنگت بزنیم که اگر ولید نیم یا ولید پرسن نیم یا ولید تکست بود تریم شود  
*/

// چک ممکن بودن ورودی برای کاربر در فیلد ولید تکست
function isPossibleToUser_Text(value) {
  if (!value) return true;
  if (value.startsWith(' ') || / {2,}/.test(value)) return false;

  const allowed = /^[\u0600-\u06FFa-zA-Z0-9 ؛،.,!?؟()\[\]{}\-_–—]*$/;
  return allowed.test(value);
}

// تابع برای بررسی ولید تکست بودن
function isValidText(value) {
  if (!value) return false;
  return isPossibleToUser_Text(value);
}