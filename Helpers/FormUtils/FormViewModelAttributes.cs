namespace AP_Project.Helpers.FormUtils
{    // صفت‌های متا دیتا بدون اضافه کردن ولیدیشن
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MaxLengthMetaAttribute : Attribute
    {
        public int MaxLength { get; }
        public MaxLengthMetaAttribute(int maxLength) => MaxLength = maxLength;
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AllowSliceMetaAttribute : Attribute
    {
        public bool AllowSlice { get; }
        public AllowSliceMetaAttribute(bool allowSlice) => AllowSlice = allowSlice;
    }

    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SelectListMetaAttribute : Attribute
    {
        public bool SelectList { get; }
        public SelectListMetaAttribute(bool selectList) => SelectList = selectList;
    }
}