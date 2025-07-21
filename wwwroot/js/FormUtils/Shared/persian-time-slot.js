document.addEventListener('DOMContentLoaded', function () {
  const DEBUG_MODE = false; // برای فعال/غیرفعال کردن لاگ‌ها

  const dayMap = Object.freeze({
    'Saturday': 'شنبه',
    'Sunday': 'یکشنبه',
    'Monday': 'دوشنبه',
    'Tuesday': 'سه‌شنبه',
    'Wednesday': 'چهارشنبه',
    'Thursday': 'پنجشنبه',
    'Friday': 'جمعه'
  });

  // تابع کمکی برای لاگینگ
  function debugLog(message, data = null) {
    if (DEBUG_MODE) {
      console.log('[PersianDayConverter]', message, data || '');
    }
  }

  // فارسی‌سازی روزها در سلول‌های جدول
  function convertTableCells() {
    const cells = document.querySelectorAll('.col-schedule');
    let convertedCount = 0;

    cells.forEach(cell => {
      const daySpan = cell.querySelector('span:first-child');
      if (!daySpan) return;

      const originalDay = daySpan.textContent.trim();
      if (dayMap[originalDay]) {
        daySpan.textContent = dayMap[originalDay];
        convertedCount++;
        debugLog('Converted table cell', {
          from: originalDay,
          to: dayMap[originalDay]
        });
      }
    });

    debugLog(`Converted ${convertedCount} table cells`);
  }

  convertTableCells();
});
