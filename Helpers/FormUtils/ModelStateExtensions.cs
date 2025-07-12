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
            if (modelState.ContainsKey(propertyName))
                modelState[propertyName].Errors.Clear();

            if (!string.IsNullOrWhiteSpace(message))
                modelState.AddModelError(propertyName, message);
        }

        // تابع افزون خطا به خط بعد
        public static void AppendModelError(this ModelStateDictionary modelState, string propertyName, string message)
        {
            if (modelState.ContainsKey(propertyName) && modelState[propertyName].Errors.Any())
            {
                var existingErrors = modelState[propertyName].Errors.Select(e => e.ErrorMessage).ToList();
                if (existingErrors.Contains(message)) return; // از قبل بود، اضافه نکن
                // اگر خطا وجود دارد، پیام جدید را به خط بعدی اضافه می‌کنیم
                existingErrors.Add(message);
                modelState[propertyName].Errors.Clear();
                modelState[propertyName].Errors.Add(string.Join("\n", existingErrors));
            }
            else
                // اگر خطایی وجود ندارد، پیام را اضافه می‌کنیم
                modelState.AddModelError(propertyName, message);
        }
    }
}
