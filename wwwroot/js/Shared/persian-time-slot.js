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
    const cells = document.querySelectorAll('.col-schedule, .col-day');
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

  // فارسی‌سازی روزها در المنت‌های انتخابی
  function convertAllSelectOptions() {
    const allSelects = document.querySelectorAll('select[id]');
    let totalConverted = 0;

    allSelects.forEach(select => {
      let selectConverted = 0;

      Array.from(select.options).forEach(option => {
        const originalText = option.text;
        let modifiedText = originalText;

        Object.entries(dayMap).forEach(([en, fa]) => {
          if (modifiedText.includes(en)) {
            modifiedText = modifiedText.replace(en, fa);
          }
        });

        if (modifiedText !== originalText) {
          option.text = modifiedText;
          selectConverted++;
          totalConverted++;
          debugLog(`Converted in select #${select.id}`, {
            from: originalText,
            to: modifiedText
          });
        }
      });

      if (selectConverted > 0) {
        debugLog(`Converted ${selectConverted} option(s) in #${select.id}`);
      }
    });

    debugLog(`Total converted options across all selects: ${totalConverted}`);
  }

  // فارسی سازی روز ورودی ریدانلی
  function convertReadonlyInputs() {
    const inputs = document.querySelectorAll('input[readonly]');

    inputs.forEach(input => {
      let original = input.value;
      let converted = original;

      Object.entries(dayMap).forEach(([en, fa]) => {
        if (converted.includes(en)) {
          converted = converted.replace(en, fa);
        }
      });

      if (converted !== original) {
        input.value = converted;
        debugLog('Converted readonly input', {
          from: original,
          to: converted
        });
      }
    });
  }

  // فارسی سازی روز در گلوبال پاپ آپ
  function convertTextNodesInPopup() {
    const popup = document.getElementById('globalPopupContainer');
    if (!popup) return;

    const walker = document.createTreeWalker(popup, NodeFilter.SHOW_TEXT, null, false);
    const textNodes = [];

    while (walker.nextNode()) {
      textNodes.push(walker.currentNode);
    }

    textNodes.forEach(node => {
      let replaced = node.nodeValue;
      Object.entries(dayMap).forEach(([en, fa]) => {
        const regex = new RegExp(`\\b${en}\\b`, 'g');
        replaced = replaced.replace(regex, fa);
      });

      if (replaced !== node.nodeValue) {
        debugLog('Converted popup text node', {
          from: node.nodeValue,
          to: replaced
        });
        node.nodeValue = replaced;
      }
    });
  }
  
  convertTableCells();
  convertAllSelectOptions();
  convertReadonlyInputs();
  convertTextNodesInPopup();

  const popupContainer = document.getElementById('globalPopupContainer');
  if (popupContainer) {
    const popupObserver = new MutationObserver(() => {
      convertTextNodesInPopup();
    });

    popupObserver.observe(popupContainer, {
      childList: true,
      subtree: true,
      characterData: true
    });
  }
});
