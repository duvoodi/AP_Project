// تنظیمات اینپوت های ولید نیم
document.querySelectorAll('.valid-person-name').forEach(input => {
  // جلوگیری از تایپ کاراکترهای غیر مجاز
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

    // جلوگیری از تایپ فاصله در ابتدای متن یا فاصله متوالی
    if (e.key === " ") {
      if (cursorPos === 0 || value[cursorPos - 1] === " ") {
        e.preventDefault();
        return;
      }
    }

    // کاراکترهای مجاز: حروف فارسی و انگلیسی، اعداد و فاصله
    const allowedChars = /^[\u0600-\u06FFa-zA-Z0-9 ]$/;
    if (!allowedChars.test(e.key)) {
      e.preventDefault();
    }
  });

  // جلوگیری از پیست یا ورودی غیرمجاز به هر نحو
  input.addEventListener('input', function () {
    const invalidFinder = /[^\u0600-\u06FFa-zA-Z0-9 ]/g;
    let cleaned = this.value
      .replace(invalidFinder, '') // حذف کاراکتر غیر مجاز
      .replace(/ +/g, ' ')       // تبدیل اسپیس‌های متوالی به یکی
      .replace(/^ /, '');         // حذف فاصله ابتدا 

    this.value = cleaned;
  });
});

/* فاصله آخر مجاز است برای اینکه کاربر بتواند بین کلمات اسپیس بزند
* با بیرون آمدن کاربر از فیلد یعنی با ایونت بلور باید فاصله اخر حذف شود
* اگر این مورد رو جدا بزنیم با ایونت بلور ولیدیت فیلد ها تداخل میکند
* با خروج کاربر اگر ارور مکس لنگت داشته باشیم این حذف فاصله آخر ممکن است طول  را مجاز کند و ارور یهویی بپرد
* پس باید داخل همان ایونت بلور ولیدیت فیلد ها بعد از ارور مکس لنگت بزنیم که اگر ولید نیم یا ولید پرسن نیم بود تریم شود  
*/

// چک ممکن بودن ورودی برای کاربر در فیلد ولید نیم
function isPossibleToUser_Name(value) {
  // کاربر میتونه ورودی رو خالی بگذاره
  if (!value) return true;
  // فاصله ابتدا و متوالی نباید باشد
  // چون همه چک ها بعد از مکس لنگت انجام میدهیم فاصله آخر باید حذف شده باشد
  if (value.startsWith(' ') || value.endsWith(' ')) return false;
  if (/ {2,}/.test(value)) return false;
  // فقط حروف فارسی، انگلیسی، اعداد و فاصله
  const allowed = /^[\u0600-\u06FFa-zA-Z0-9 ]*$/;
  return allowed.test(value);
}

// تابع برای بررسی ولید نیم بودن
function isValidName(value) {
  if (!value) return false;
  return isPossibleToUser_Name(value);
}