const LOGGING_ENABLED = true;

function consoleLogIfAllowed(...args) {
  if (LOGGING_ENABLED) {
    console.log('[GlobalPopup]', ...args);
  }
}

// Sorting function for list items
function applySorting(list, sortKey1, sortKey2) {
  if (!sortKey1) return list;

  return [...list].sort((a, b) => {
    if (a[sortKey1] < b[sortKey1]) return -1;
    if (a[sortKey1] > b[sortKey1]) return 1;

    if (sortKey2) {
      if (a[sortKey2] < b[sortKey2]) return -1;
      if (a[sortKey2] > b[sortKey2]) return 1;
    }

    return 0;
  });
}

const GlobalPopup = (function () {
  let popupContainer = null;
  let currentOptions = null;
  let currentModel = null;
  let currentConfig = null;
  let selectedItems = new Set();
  let clientSideLists = {}; // Store client-side lists

  function createPopupStructure() {
    const backdrop = document.createElement('div');
    backdrop.id = 'globalPopupBackdrop';
    backdrop.className = 'global-popup-backdrop';

    const popupBox = document.createElement('div');
    popupBox.id = 'globalPopupBox';
    popupBox.className = 'global-popup-box';

    backdrop.appendChild(popupBox);
    popupContainer.appendChild(backdrop);

    return { backdrop, popupBox };
  }

  function createSafeElement(tag, text, className, html = '') {
    const el = document.createElement(tag);
    if (className) el.className = className;
    if (text) el.textContent = text;
    if (html) el.innerHTML = html;
    return el;
  }

  function createIcon(iconType) {
    const icons = {
      success: '<i class="fas fa-check-circle"></i>',
      info: '<i class="fas fa-info-circle"></i>',
      warning: '<i class="fas fa-exclamation-triangle"></i>',
      error: '<i class="fas fa-times-circle"></i>'
    };
    return createSafeElement('span', '', 'popup-icon', icons[iconType] || '');
  }

  function renderTitleWithIcon(popupBox, options) {
    const titleContainer = createSafeElement('div', '', 'popup-title-container');

    if (options.iconType) {
      titleContainer.appendChild(createIcon(options.iconType));
    }

    const title = createSafeElement('div', options.PopupTitle, 'popup-title');
    titleContainer.appendChild(title);
    popupBox.appendChild(titleContainer);
  }

  function renderModelContent(popupBox, model, config) {
    config.SimplePropsOrder?.forEach(prop => {
      const displayName = config.SimplePropDisplayNames?.[prop] || prop;
      const value = model[prop] || '';
      popupBox.appendChild(createSafeElement('div', `${displayName}: ${value}`, 'popup-content-line'));
    });

    if (config.ShowListProps && config.ListProps?.length > 0) {
      config.ListProps.forEach(listProp => {
        // Use client-side list if available
        const list = clientSideLists[listProp] || model[listProp];
        if (!list?.length) return;

        const sectionTitle = createSafeElement('div', config.SimplePropDisplayNames?.[listProp] || listProp, 'popup-section-title');
        popupBox.appendChild(sectionTitle);

        // Add bulk selection controls if needed
        if (config.ShowCheckboxes?.[listProp]) {
          renderBulkControls(popupBox, listProp);
        }

        renderListItems(popupBox, list, {
          field: listProp,
          fieldOrder: config.ListPropFieldOrder?.[listProp],
          displayNames: config.ListPropItemDisplayNames?.[listProp],
          sortKey1: config.ListPropSortKey1?.[listProp],
          sortKey2: config.ListPropSortKey2?.[listProp],
          showCheckbox: config.ShowCheckboxes?.[listProp],
          deleteUrl: config.DeleteUrls?.[listProp],
          deleteIdField: config.DeleteIdFields?.[listProp],
          clientSideDelete: config.ClientSideDelete?.[listProp]
        });
      });
    }
  }

  function renderBulkControls(popupBox, listProp) {
    const bulkContainer = createSafeElement('div', '', 'popup-bulk-actions');

    const selectAllBtn = createSafeElement('button', 'انتخاب همه', 'btn-yellow');
    selectAllBtn.addEventListener('click', () => {
      const checkboxes = popupBox.querySelectorAll(`.popup-checkbox[data-list="${listProp}"]`);
      checkboxes.forEach(checkbox => {
        checkbox.checked = true;
        selectedItems.add(checkbox.dataset.id);
      });
    });

    const deselectAllBtn = createSafeElement('button', 'لغو انتخاب همه', 'btn-yellow');
    deselectAllBtn.addEventListener('click', () => {
      const checkboxes = popupBox.querySelectorAll(`.popup-checkbox[data-list="${listProp}"]`);
      checkboxes.forEach(checkbox => {
        checkbox.checked = false;
        selectedItems.delete(checkbox.dataset.id);
      });
    });

    bulkContainer.appendChild(selectAllBtn);
    bulkContainer.appendChild(deselectAllBtn);

    // Add confirm button if needed
    if (currentConfig.BulkActionText?.[listProp]) {
      const confirmBtn = createSafeElement('button', currentConfig.BulkActionText[listProp], 'btn-green');
      confirmBtn.addEventListener('click', () => {
        if (currentConfig.OnBulkActionJs) {
          const selectedIds = Array.from(selectedItems);
          eval(`${currentConfig.OnBulkActionJs}(${JSON.stringify(selectedIds)})`);
        }
      });
      bulkContainer.appendChild(confirmBtn);
    }

    popupBox.appendChild(bulkContainer);
  }

  function renderListItems(popupBox, list, options = {}) {
    const sorted = applySorting(list, options.sortKey1, options.sortKey2);

    sorted.forEach((item, index) => {
      const itemEl = createSafeElement('div', '', 'popup-list-item');

      if (options.showCheckbox) {
        const checkbox = createSafeElement('input', '', 'popup-checkbox');
        checkbox.type = 'checkbox';
        checkbox.dataset.id = item[options.deleteIdField || 'Id'];
        checkbox.dataset.list = options.field;
        checkbox.addEventListener('change', handleCheckboxChange);
        itemEl.appendChild(checkbox);
      }

      const itemContent = createSafeElement('span', '', 'popup-list-item-content');
      itemContent.appendChild(createSafeElement('span', `${index + 1}. `, 'item-index'));

      (options.fieldOrder || []).forEach((field, i) => {
        const displayName = options.displayNames?.[field] || field;
        const value = item[field] || '';
        itemContent.appendChild(createSafeElement('span', `${displayName}: ${value}`));
      });

      // Add space between content and buttons
      const spacer = createSafeElement('span', '', 'popup-item-spacer');
      itemContent.appendChild(spacer);

      if (options.deleteUrl && item[options.deleteIdField]) {
        const deleteBtn = createSafeElement('button', 'حذف', 'popup-delete-btn');
        deleteBtn.dataset.url = options.deleteUrl;
        deleteBtn.dataset.id = item[options.deleteIdField];
        deleteBtn.dataset.list = options.field;
        deleteBtn.addEventListener('click', handleDeleteItem);
        itemEl.appendChild(deleteBtn);
      }

      // Client-side delete button
      if (options.clientSideDelete && item[options.deleteIdField]) {
        const deleteBtn = createSafeElement('button', 'حذف', 'popup-delete-btn');
        deleteBtn.dataset.id = item[options.deleteIdField];
        deleteBtn.dataset.list = options.field;
        deleteBtn.addEventListener('click', handleClientSideDelete);
        itemEl.appendChild(deleteBtn);
      }

      itemEl.appendChild(itemContent);
      popupBox.appendChild(itemEl);
    });
  }

  function handleCheckboxChange(e) {
    const id = e.target.dataset.id;
    if (e.target.checked) {
      selectedItems.add(id);
    } else {
      selectedItems.delete(id);
    }
  }

  function handleDeleteItem(e) {
    const url = e.target.dataset.url;
    const id = e.target.dataset.id;
    const listName = e.target.dataset.list;
    const listItem = e.target.closest('.popup-list-item');

    consoleLogIfAllowed(`Deleting item - URL: ${url}, ID: ${id}`);

    if (confirm('آیا از حذف این مورد مطمئن هستید؟')) {
      // Animate removal
      listItem.style.opacity = '0';
      listItem.style.transform = 'translateX(-20px)';
      listItem.style.transition = 'all 0.3s ease';

      // Make API call
      fetch(`${url}?id=${id}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
          'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
      })
        .then(response => {
          if (response.ok) {
            return response.json();
          }
          throw new Error('Deletion failed');
        })
        .then(data => {
          consoleLogIfAllowed(`Item deleted successfully - ID: ${id}`);
          // Show success popup with animation
          setTimeout(() => {
            showAnimatedPopup(
              'موفقیت',
              'مورد با موفقیت حذف شد',
              'success',
              300
            );
            listItem.remove();
          }, 300);
        })
        .catch(error => {
          consoleLogIfAllowed(`Deletion failed - ID: ${id}`, error);
          // Show error popup with animation
          showAnimatedPopup(
            'خطا',
            'خطا در حذف مورد',
            'error',
            300
          );
          // Reset animation if failed
          listItem.style.opacity = '1';
          listItem.style.transform = 'translateX(0)';
        });
    }
  }

  function handleClientSideDelete(e) {
    const id = e.target.dataset.id;
    const listName = e.target.dataset.list;
    const listItem = e.target.closest('.popup-list-item');

    consoleLogIfAllowed(`Client-side delete - ID: ${id}, List: ${listName}`);

    if (confirm('آیا از حذف این مورد مطمئن هستید؟')) {
      // Update client-side list
      if (clientSideLists[listName]) {
        clientSideLists[listName] = clientSideLists[listName].filter(item =>
          item.Id != id
        );
      }

      // Animate removal
      listItem.style.opacity = '0';
      listItem.style.transform = 'translateX(-20px)';
      listItem.style.transition = 'all 0.3s ease';

      setTimeout(() => {
        listItem.remove();
        showAnimatedPopup(
          'موفقیت',
          'مورد با موفقیت حذف شد',
          'success',
          300
        );
      }, 300);
    }
  }

  function showAnimatedPopup(title, message, type, delay = 0) {
    setTimeout(() => {
      GlobalPopup.show({
        PopupTitle: title,
        SimpleMessage: message,
        CanCloseManually: false,
        iconType: type,
        BlockScroll: false,
        AutoClose: true,
        AutoCloseDelay: 3000
      });
    }, delay);
  }

  function ensurePopupStructure() {
    let backdrop = document.getElementById('globalPopupBackdrop');
    let popupBox = document.getElementById('globalPopupBox');

    if (!backdrop || !popupBox) {
      return createPopupStructure();
    }

    return { backdrop, popupBox };
  }

  function renderContent(popupBox) {
    popupBox.innerHTML = '';

    // Render title with icon if exists
    if (currentOptions.PopupTitle) {
      renderTitleWithIcon(popupBox, currentOptions);
    }

    // Render simple message if exists
    if (currentOptions.SimpleMessage) {
      const messageClass = 'popup-simple-message' +
        (currentOptions.iconType === 'success' ? ' success' :
          currentOptions.iconType === 'error' ? ' error' : '');

      popupBox.appendChild(createSafeElement('div', currentOptions.SimpleMessage, messageClass));
    }

    // Render model content if exists
    if (currentModel && currentConfig) {
      renderModelContent(popupBox, currentModel, currentConfig);
    }

    // Render action buttons if needed
    if (currentOptions.ShowActionButtons) {
      const actionsContainer = createSafeElement('div', '', 'global-popup-actions');

      if (currentOptions.GreenButtonText) {
        const greenBtn = createSafeElement('button', currentOptions.GreenButtonText, 'btn-green');
        greenBtn.addEventListener('click', () => {
          if (currentOptions.OnGreenClickJs) {
            eval(currentOptions.OnGreenClickJs);
          } else {
            GlobalPopup.hide();
          }
        });
        actionsContainer.appendChild(greenBtn);
      }

      if (currentOptions.RedButtonText) {
        const redBtn = createSafeElement('button', currentOptions.RedButtonText, 'btn-red');
        redBtn.addEventListener('click', () => {
          if (currentOptions.OnRedClickJs) {
            eval(currentOptions.OnRedClickJs);
          }
        });
        actionsContainer.appendChild(redBtn);
      }

      popupBox.appendChild(actionsContainer);
    }

    // Add close button if allowed
    if (currentOptions.CanCloseManually) {
      const closeBtn = createSafeElement('button', '×', 'popup-close-btn');
      closeBtn.addEventListener('click', GlobalPopup.hide);
      popupBox.prepend(closeBtn);
    }
  }

  function show(options, model, config) {
    if (!popupContainer) {
      console.error('Popup container not initialized');
      return;
    }

    // Store client-side lists
    if (config?.ClientSideLists) {
      config.ClientSideLists.forEach(listName => {
        if (model[listName]) {
          clientSideLists[listName] = [...model[listName]];
        }
      });
    }

    // Hide current popup if visible
    const backdrop = document.getElementById('globalPopupBackdrop');
    if (backdrop.style.display === 'flex') {
      backdrop.classList.remove('visible');
      setTimeout(() => {
        _showPopup(options, model, config);
      }, 300);
    } else {
      _showPopup(options, model, config);
    }
  }

  function _showPopup(options, model, config) {
    currentOptions = options;
    currentModel = model;
    currentConfig = config;
    selectedItems.clear();

    let { backdrop, popupBox } = ensurePopupStructure();
    renderContent(popupBox);

    if (options.CloseOnBackdropClick) {
      backdrop.onclick = (e) => e.target === backdrop && hide();
    }

    if (options.BlockScroll) {
      document.body.style.overflow = 'hidden';
    }

    if (options.RedirectUrl) {
      setTimeout(() => {
        window.location.href = options.RedirectUrl;
      }, options.RedirectDelayMs || 3000);
    }

    // Auto close if enabled
    if (options.AutoClose) {
      setTimeout(() => {
        hide();
      }, options.AutoCloseDelay || 3000);
    }

    backdrop.style.display = 'flex';
    setTimeout(() => backdrop.classList.add('visible'), 10);
  }

  function hide() {
    return new Promise((resolve) => {
      const backdrop = document.getElementById('globalPopupBackdrop');
      const popupBox = document.getElementById('globalPopupBox');
      if (backdrop) {
        backdrop.classList.remove('visible');
        setTimeout(() => {
          backdrop.style.display = 'none';
          document.body.style.overflow = '';
          if (popupBox) popupBox.innerHTML = '';
          currentOptions = currentModel = currentConfig = null;
          selectedItems.clear();
          clientSideLists = {};
          resolve();
        }, 300);
      } else {
        resolve();
      }
    });
  }


  function init(containerId = 'globalPopupContainer') {
    popupContainer = document.getElementById(containerId);
    if (!popupContainer) {
      console.error('Popup container not found:', containerId);
      return;
    }
    createPopupStructure();
    hide();
  }

  return { init, show, hide, showAnimatedPopup };
})();

window.GlobalPopup = GlobalPopup;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
  GlobalPopup.init();

  // Event delegation for popup buttons
  document.addEventListener('click', function (e) {
    if (e.target.matches('.btn-popup')) {
      e.preventDefault(); // جلوگیری از رفتار پیش‌فرض
      try {
        const popupData = JSON.parse(e.target.dataset.popup);
        GlobalPopup.show(
          popupData.options,
          popupData.model || null,
          popupData.config || null
        ).catch(error => {
          console.error('Popup show error:', error);
        });
      } catch (error) {
        console.error('Error parsing popup data:', error);
      }
    }
  });
});