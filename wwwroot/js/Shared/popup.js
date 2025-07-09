window.addEventListener('DOMContentLoaded', () => {
  // همه‌ی المنت‌هایی که باید بعد از مدت کوتاهی ظاهر بشن
  const willShowPopups = document.querySelectorAll('.will-show');
  willShowPopups.forEach(el => {
    setTimeout(() => {
    el.classList.add('visible');
    }, 500); // تأخیر 500 میلی‌ثانیه‌ای
  });
});
