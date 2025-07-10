using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AP_Project.Helpers.FormUtils
{
    public static class ModelStateErrorExtensions
    {
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
