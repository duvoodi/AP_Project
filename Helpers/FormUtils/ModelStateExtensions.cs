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
                if (prop.PropertyType == typeof(string) && prop.CanRead && prop.CanWrite)
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
                modelState.AddModelError(propertyName, message);
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
