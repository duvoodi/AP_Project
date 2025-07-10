  // تول‌تیپ
  const tooltip = document.createElement('div');
  tooltip.className = 'custom-tooltip';
  document.body.appendChild(tooltip);

  document.querySelectorAll('[data-tooltip]').forEach(el => {
    el.addEventListener('mouseenter', () => {
      const text = el.getAttribute('data-tooltip');
      const align = el.getAttribute('data-tooltip-align') || 'center';
      tooltip.textContent = text;

      tooltip.textContent = text;
      tooltip.classList.remove('align-left','align-center','align-right','show');
      tooltip.classList.add(`align-${align}`, 'show');

      const r = el.getBoundingClientRect();
      tooltip.style.top = (r.bottom + 6) + 'px';
      if (align === 'left') {
        tooltip.style.left = r.left + 'px';
      } else if (align === 'right') {
        tooltip.style.left = r.right + 'px';
      } else { // center
        tooltip.style.left = (r.left + r.width/2) + 'px';
      }
    });

    el.addEventListener('mouseleave', () => {
      tooltip.classList.remove('show');
    });
  });