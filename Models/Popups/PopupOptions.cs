using System.Collections.Generic;

namespace AP_Project.Models.Popups
{
    public class PopupOptions
    {
        public string PopupTitle { get; set; }
        public string SimpleMessage { get; set; }

        // کنترل بسته شدن دستی
        public bool CanCloseManually { get; set; } = false;
        public bool CloseOnBackdropClick { get; set; } = false;
        public bool BlockScroll { get; set; } = true;


        // دکمه‌های پایینی
        public bool ShowActionButtons { get; set; } = false;
        public string GreenButtonText { get; set; }
        public string RedButtonText { get; set; }
        public string OnGreenClickJs { get; set; }
        public string OnRedClickJs { get; set; }

        // تنظیمات لیست (ویو بگ)
        public bool ShowList { get; set; } = false;
        public IEnumerable<object> PopupList { get; set; }
        public List<string> PopupListFieldOrder { get; set; }
        public Dictionary<string, string> PopupListDisplayNames { get; set; }
        public string PopupListSortKey1 { get; set; }
        public string PopupListSortKey2 { get; set; }

        // تنظیمات دکمه انتهای هر خط از لیست
        public bool ShowListItemButtons { get; set; } = false;
        public string ListItemButtonText { get; set; }
        public string ListItemButtonActionJs { get; set; }

        // ریدایرکت خودکار
        public string RedirectUrl { get; set; }
        public int RedirectDelayMs { get; set; } = 3000;
    }
}
