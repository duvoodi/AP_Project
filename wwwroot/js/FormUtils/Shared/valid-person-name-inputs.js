// تنظیمات اینپوت های ولید پرسن نیم
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

    // کاراکترهای مجاز فقط حروف فارسی و فاصله
    const allowedChars = /^[\u0600-\u06FF ]$/;
    if (!allowedChars.test(e.key)) {
      e.preventDefault();
    }
  });

  // جلوگیری از پیست یا ورودی غیرمجاز به هر نحو
  input.addEventListener('input', function () {
    const invalidFinder = /[^\u0600-\u06FF ]/g;
    this.value = this.value
      .replace(invalidFinder, '') // حذف کاراکتر غیر مجاز
      .replace(/ +/g, ' ')        // تبدیل اسپیس‌های متوالی به یکی
      .replace(/^ /, '');         // حذف فاصله ابتدا 
  });
});

/* فاصله آخر مجاز است برای اینکه کاربر بتواند بین کلمات اسپیس بزند
* با بیرون آمدن کاربر از فیلد یعنی با ایونت بلور باید فاصله اخر حذف شود
* اگر این مورد رو جدا بزنیم با ایونت بلور ولیدیت فیلد ها تداخل میکند
* با خروج کاربر اگر ارور مکس لنگت داشته باشیم این حذف فاصله آخر ممکن است طول  را مجاز کند و ارور یهویی بپرد
* پس باید داخل همان ایونت بلور ولیدیت فیلد ها بعد از ارور مکس لنگت بزنیم که اگر ولید نیم یا ولید پرسن نیم بود تریم شود  
* یعنی دقیقا اول سوییچ کیس مواردی که ولید نیم یا ولید پرسن نیم اند
*/

// چک ممکن بودن ورودی برای کاربر در فیلد ولید پرسن نیم
function isPossibleToUser_PersonName(value) {
  // کاربر میتونه ورودی رو خالی بگذاره
  if (!value) return true;
  // فاصله ابتدا و متوالی نباید باشد
  // چون همه چک ها بعد از مکس لنگت انجام میدهیم فاصله آخر باید حذف شده باشد
  if (value.startsWith(' ') || value.endsWith(' ')) return false;
  if (/ {2,}/.test(value)) return false;
  // فقط حروف فارسی و فاصله
  const allowed = /^[\u0600-\u06FF ]*$/;
  return allowed.test(value);
}

// تابع برای بررسی ولید پرسن نیم بودن
function isValidPersonName(value) {
  if (!value) return false;
  return isPossibleToUser_PersonName(value);
}
