// تنظیمات اینپوت های ولید پسورد
document.querySelectorAll('.valid-password').forEach(input => {
  // جلو گیری از تایپ کاراکترهای غیر مجاز
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

    // کاراکترهای مجاز: حروف انگلیسی، ارقام، کاراکترهای خاص استاندارد
    const allowedChars = /^[a-zA-Z0-9!#$%&'*+/=?^_`{|}~@\-.,:;"()\\[\]{}<>]$/;
    if (!allowedChars.test(e.key)) {
      e.preventDefault(); // جلوگیری از ورود کاراکتر غیر مجاز
    }
  });
  // جلوگیری از پیست یا ورودی غیرمجاز به هر نحو
  input.addEventListener('input', function () {
    const invalidFinder = /[^a-zA-Z0-9!#$%&'*+/=?^_`{|}~@\-.,:;"()\\[\]{}<>]/g;
    this.value = this.value.replace(invalidFinder, '');
  });
});

// چک ممکن بودن ورودی برای کاربر در فیلد ولید پسورد
function isPossibleToUser_Password(value) {
  // کاربر میتونه ورودی رو خالی بگذاره
  if (!value) return true;
  const allowed = /^[a-zA-Z0-9!#$%&'*+/=?^_`{|}~@\-.,:;"()\[\]{}<>]*$/;
  return allowed.test(value);
}

// تابع برای بررسی ولید پسورد بودن
function isValidPassword(password) {
  if (!password) return false;
  return isPossibleToUser_Password(password);
}

// دکمه هاید اند شو ورودی های ولید پسورد
document.querySelectorAll('.password-wrapper').forEach(wrapper => {
  const input = wrapper.querySelector('input.valid-password');
  const toggleBtn = wrapper.querySelector('.toggle-password');
  const icon = toggleBtn.querySelector('i');

  toggleBtn.addEventListener('click', () => {
    if (input.type === 'password') {
      input.type = 'text';
      icon.classList.replace('fa-eye', 'fa-eye-slash');
    } else {
      input.type = 'password';
      icon.classList.replace('fa-eye-slash', 'fa-eye');
    }
  });
});
