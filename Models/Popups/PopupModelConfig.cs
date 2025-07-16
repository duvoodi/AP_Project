using System.Collections.Generic;

namespace AP_Project.Models.Popups
{
    public class PopupModelConfig
    {
        // نمایش ویژگی‌های ساده مدل واحد
        public List<string> SimplePropsOrder { get; set; } = new();
        public Dictionary<string, string> SimplePropDisplayNames { get; set; } = new();

        // لیست‌های داخل مدل
        public bool ShowListProps { get; set; } = false;
        public List<string> ListProps { get; set; } = new();

        // ترتیب نمایش اعضا و پراپرتی‌های اعضای لیست
        public Dictionary<string, string> ListPropSortKey1 { get; set; } = new();
        public Dictionary<string, string> ListPropSortKey2 { get; set; } = new(); // اختیاری

        public Dictionary<string, List<string>> ListPropFieldOrder { get; set; } = new();
        public Dictionary<string, Dictionary<string, string>> ListPropItemDisplayNames { get; set; } = new();
    }
}
