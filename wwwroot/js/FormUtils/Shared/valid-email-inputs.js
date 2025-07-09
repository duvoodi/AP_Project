// تنظیمات اینپوت های ولید ایمیل
document.querySelectorAll('.valid-email').forEach(input => {
  // جلوگیری از تایپ کاراکترهای غیرمجاز
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

    // اگر قبلاً @ وجود دارد، اجازه نده مجدد وارد شود
    const value = this.value;
    if (e.key === '@' && value.includes('@')) {
      e.preventDefault();
      return;
    }

    // کاراکترهای مجاز: حروف انگلیسی، ارقام، کاراکترهای خاص استاندارد
    const allowedChars = /^[A-Za-z0-9!#$%&'*+\-/=?^_`{|}~@.]$/;
    if (!allowedChars.test(e.key)) {
      e.preventDefault();
    }
  });

  // جلوگیری از پیست یا ورودی غیرمجاز به هر نحو
  input.addEventListener('input', function () {
    const invalidFinder = /[^A-Za-z0-9!#$%&'*+\-/=?^_`{|}~@.]/g;
    const raw = this.value.replace(invalidFinder, '');
    // فقط اجازه یک @
    const parts = raw.split('@');
    this.value = parts.length > 2 ? parts[0] + '@' + parts.slice(1).join('').replace(/@/g, '') : raw;
  });
});

// چک ممکن بودن ورودی برای کاربر در فیلد ولید میل
function isPossibleToUser_Email(value) {
  // کاربر میتونه ورودی رو خالی بگذاره
  if (!value) return true;
  // فقط یک @ مجاز است
  if ((value.match(/@/g) || []).length > 1) return false;
  // فقط کاراکترهای مجاز
  const allowed = /^[A-Za-z0-9!#$%&'*+\-/=?^_`{|}~@.]*$/;
  return allowed.test(value);
}


// چک ولید ایمیل بودن
function isValidEmail(email) {
  if (!email) return false;

  // ایمیل نباید فاصله داشته باشد
  if (email.includes(' ')) return false;

  // طول کل ایمیل نباید بیشتر از 320 کاراکتر باشد
  if (email.length > 320) return false;

  // بخش نام کاربری (local-part): حروف و اعداد و کاراکترهای خاص
  const localPartRegex = /^[a-zA-Z0-9!#$%&'*+\-/=?^_`{|}~-]+$/;
  const atParts = email.split('@');

  if (atParts.length !== 2) return false; // فقط یک اتساین باید باشد
  const [localPart, domainPart] = atParts;

  // طول بخش نام کاربری نباید بیشتر از 64 کاراکتر باشد
  if (localPart.length > 64) return false;

  // بخش نام کاربری باید با الگوی مجاز همخوانی داشته باشد
  if (!localPartRegex.test(localPart)) return false;

  // بخش دامنه باید حداقل یک نقطه داشته باشد و نقطه نباید در ابتدا یا انتها باشد
  const domainParts = domainPart.split('.');
  if (domainParts.length < 2) return false; // باید حداقل یک نقطه باشد
  if (domainPart.startsWith('.') || domainPart.endsWith('.')) return false;


  // طول بخش دامنه نباید بیشتر از 255 کاراکتر باشد
  if (domainPart.length > 255) return false;
  
  // پسوند دامنه باید حداقل دو کاراکتر داشته باشد
  const lastDomainSegment = domainParts[domainParts.length - 1];
  if (lastDomainSegment.length < 2) return false;

  // بخش دامنه باید با حروف یا اعداد شروع و تمام شود
  const domainRegex = /^[a-zA-Z0-9]+([-.][a-zA-Z0-9]+)*$/;
  if (!domainRegex.test(domainPart)) return false;

  return true;
}