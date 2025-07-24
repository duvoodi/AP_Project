using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AP_Project.Helpers.FormUtils
{
    public static class ModelStateExtensions
    {
        // تابع تبدیل فیلد خالی فرم که اینجا نال میشوند و اینولید میشوند به فیلد امپتی ولید
        public static void NullFieldsToValidEmpty<TModel>(this ModelStateDictionary ModelState, TModel FormViewModel)
        {
            if (FormViewModel == null) return;

            var properties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                var propType = prop.PropertyType;

                // برای string
                if (propType == typeof(string))
                {
                    var value = (string)prop.GetValue(FormViewModel);
                    if (value == null)
                    {
                        var state = ModelState[prop.Name];
                        if (state != null)
                        {
                            state.Errors.Clear();
                            state.ValidationState = ModelValidationState.Valid;
                        }
                        prop.SetValue(FormViewModel, "");
                    }
                }
                // برای Guid و Guid? 
                else if (propType == typeof(Guid) || propType == typeof(Guid?))
                {
                    var state = ModelState[prop.Name];
                    if (state != null && state.Errors.Count > 0)
                    {
                        // مقدار فعلی را چک کن
                        var currentValue = prop.GetValue(FormViewModel);
                        bool isEmpty = currentValue == null || currentValue.Equals(Guid.Empty);

                        // اگر مقدار ناصحیح است، مقدار پیشفرض ست کن و خطاها را پاک کن
                        if (isEmpty)
                        {
                            state.Errors.Clear();
                            state.ValidationState = ModelValidationState.Valid;
                            prop.SetValue(FormViewModel, Guid.Empty);
                        }
                    }
                }
                // برای int و int? 
                else if (propType == typeof(int) || propType == typeof(int?))
                {
                    var state = ModelState[prop.Name];
                    if (state != null && state.Errors.Count > 0)
                    {
                        var currentValue = prop.GetValue(FormViewModel);
                        bool isDefault = currentValue == null || currentValue.Equals(default(int));

                        if (isDefault)
                        {
                            state.Errors.Clear();
                            state.ValidationState = ModelValidationState.Valid;
                            prop.SetValue(FormViewModel, default(int));
                        }
                    }
                }
            }
        }

        // تابع جایگزینی خطا
        public static void ReplaceModelError(this ModelStateDictionary modelState, string propertyName, string message)
        {
            if (modelState.TryGetValue(propertyName, out var entry))
                entry.Errors.Clear();

            if (!string.IsNullOrWhiteSpace(message))
                modelState.AddModelError(propertyName, message);
        }

        // تابع افزون خطا به خط بعد
        public static void AppendModelError(this ModelStateDictionary modelState, string propertyName, string message)
        {

            if (string.IsNullOrWhiteSpace(message))
                return;

            if (!modelState.TryGetValue(propertyName, out var entry))
            {
                // اگر هنوز کلیدی وجود نداره، مستقیماً ارور رو اضافه کن
                modelState.ReplaceModelError(propertyName, message);
                return;
            }

            // استخراج خط‌ها به‌صورت جداگانه
            var existingErrors = entry.Errors
                .SelectMany(e => (e?.ErrorMessage ?? "").Split('\n'))
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            // جلوگیری از تکراری بودن
            if (existingErrors.Contains(message))
                return;

            // اضافه کردن ارور جدید
            existingErrors.Add(message);

            // پاک کردن و جایگزینی با رشته‌های متصل‌شده با \n
            modelState.ReplaceModelError(propertyName, string.Join("\n", existingErrors));
        }
    }
}
