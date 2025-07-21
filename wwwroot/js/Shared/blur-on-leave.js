// خارج شدن دکمه از فوکوس بعد خروج موس
document.querySelectorAll('.blur-on-leave').forEach(btn => {
  btn.addEventListener('mouseup', function () {
    this.blur();
  });
});