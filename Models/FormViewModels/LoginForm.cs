using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AP_Project.Helpers.FormUtils;

namespace AP_Project.FormViewModels.LoginForm
{
    public class LoginFormViewModel
    {
        [MaxLengthMeta(32)]
        [AllowSliceMeta(false)]
        public string Username { get; set; }

        [MaxLengthMeta(32)]
        [AllowSliceMeta(false)]
        public string Password { get; set; }
    }

    public static class LoginFormViewModelExtensions
    {
        public static bool ValidateField(this ModelStateDictionary modelState, LoginFormViewModel model, string propertyName, bool showError = true)
        {
            var propInfo = typeof(LoginFormViewModel).GetProperty(propertyName);
            var value = propInfo?.GetValue(model)?.ToString() ?? "";
            var existingError = modelState[propertyName].Errors.FirstOrDefault()?.ErrorMessage;

            void ReplaceError_IfAllowed(string propname, string message)
            {
                if (showError)
                    modelState.ReplaceModelError(propname, message);
            }

            void AppendError_IfAllowed(string propname, string message)
            {
                if (showError)
                    modelState.AppendModelError(propname, message);
            }

            // 1. چک خالی بودن فیلدهای اجباری
            var required = new[] { nameof(model.Username), nameof(model.Password) };
            if (required.Contains(propertyName) && string.IsNullOrWhiteSpace(value))
            {
                ReplaceError_IfAllowed(propertyName, "لطفاً این فیلد را پر کنید");
                return false;
            }

            // 2. چک مکس لنگت ارور
            // 2.1 اگر ارور مکس لنگت داره اولویت با اونه، اول اون باید برطرف بشه
            if (existingError.IsMaxLengthError())
            {
                return false;
            }

            // 2.2 اگر بدون ارور و اسلایسی هست نباید برای کاربر بیشتر زدن ممکن باشه
            if (!model.IsPossibleToUser_MaxLength(propertyName))
            {
                propInfo?.SetValue(model, "");
                ReplaceError_IfAllowed(propertyName, "خطا در دریافت ورودی");
                return false;
            }

            // 3. چک‌های اختصاصی هر فیلد
            switch (propertyName)
            {
                case nameof(model.Username):
                    if (!value.IsPossibleToUser_Username())
                    {
                        propInfo?.SetValue(model, "");
                        ReplaceError_IfAllowed(propertyName, "خطا در دریافت ورودی");
                        return false;
                    }
                    if (!value.IsValidUsername())
                    {
                        ReplaceError_IfAllowed(propertyName, "ورودی نامعتبر است");
                        return false;
                    }
                    break;

                case nameof(model.Password):
                    if (!value.IsPossibleToUser_Password())
                    {
                        propInfo?.SetValue(model, "");
                        ReplaceError_IfAllowed(propertyName, "خطا در دریافت ورودی");
                        return false;
                    }
                    if (!value.IsValidPassword())
                    {
                        ReplaceError_IfAllowed(propertyName, "ورودی نامعتبر است");
                        return false;
                    }
                    break;
            }

            // اگر هیچ مورد خطایی نداشت، خطای قبلی را پاک کن
            ReplaceError_IfAllowed(propertyName, "");
            return true;
        }
    }
}