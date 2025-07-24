using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AP_Project.Helpers.FormUtils;

namespace AP_Project.FormViewModels
{
    public class AddCourseCodeFormViewModel
    {
        [MaxLengthMeta(64)]
        [AllowSliceMeta(false)]
        public string AddTitle { get; set; }

        [MaxLengthMeta(8)]
        [AllowSliceMeta(false)]
        public string AddCode { get; set; }
    }

    public class EditCourseCodeFormViewModel
    {
        [SelectListMeta(true)]
        public Guid EditId { get; set; }

        [MaxLengthMeta(64)]
        [AllowSliceMeta(false)]
        public string EditTitle { get; set; }

        [MaxLengthMeta(8)]
        [AllowSliceMeta(false)]
        public string EditCode { get; set; }
    }

    public static class CourseCodeFormViewModelExtensions
    {
        public static bool ValidateField(
            this ModelStateDictionary modelState,
            AddCourseCodeFormViewModel model,
            string propertyName,
            bool showError = true)
        {
            var propInfo = typeof(AddCourseCodeFormViewModel).GetProperty(propertyName);
            var value = propInfo?.GetValue(model)?.ToString() ?? "";
            var existingError = modelState[propertyName]?.Errors.FirstOrDefault()?.ErrorMessage;

            void ReplaceError(string message) => modelState.ReplaceModelError(propertyName, message);

            // فیلد های اجباری
            var requiredFields = new[] {
                nameof(model.AddCode),
                nameof(model.AddTitle)
            };

            // بررسی خالی نبودن فیلدهای اجباری
            if (requiredFields.Contains(propertyName) && string.IsNullOrWhiteSpace(value))
            {
                ReplaceError("لطفاً این فیلد را پر کنید");
                return false;
            }

            // چک مکس لنگت ارور
            // اگرارور مکس لنگت  داره اولیت با اونه اول اون باید برطرف
            string maxLength_error = model.MaxLengthError(propertyName);
            if (maxLength_error != null)
            {
                if (showError) ReplaceError(maxLength_error);
                return false;
            }
            // اگز بدون ارور و اسلایسی هست نباید برای کاربر بیشتر زدن ممکن باشه
            if (!model.IsPossibleToUser_MaxLength(propertyName))
            {
                propInfo?.SetValue(model, "");
                if (showError) ReplaceError("خطا در دریافت ورودی");
                return false;
            }

            switch (propertyName)
            {
                case nameof(model.AddCode):
                    if (!value.IsPossibleToUser_Numeric())
                    {
                        if (showError) ReplaceError("خطا در دریافت ورودی");
                        return false;
                    }
                    if (!value.IsValidNumeric())
                    {
                        if (showError) ReplaceError("ورودی نامعتبر است");
                        return false;
                    }
                    break;

                case nameof(model.AddTitle):
                    if (!value.IsPossibleToUser_Name())
                    {
                        if (showError) ReplaceError("خطا در دریافت ورودی");
                        return false;
                    }
                    if (!value.IsValidName())
                    {
                        if (showError) ReplaceError("ورودی نامعتبر است");
                        return false;
                    }
                    break;
            }

            if (showError) ReplaceError("");
            return true;
        }

        public static bool ValidateField(
            this ModelStateDictionary modelState,
            EditCourseCodeFormViewModel model,
            string propertyName,
            bool showError = true)
        {
            var propInfo = typeof(EditCourseCodeFormViewModel).GetProperty(propertyName);
            var value = propInfo?.GetValue(model)?.ToString() ?? "";
            var existingError = modelState[propertyName]?.Errors.FirstOrDefault()?.ErrorMessage;

            void ReplaceError(string message) => modelState.ReplaceModelError(propertyName, message);

            // فیلد های اجباری
            var requiredFields = new[] {
                nameof(model.EditId),
                nameof(model.EditCode),
                nameof(model.EditTitle)
            };

            // بررسی خالی بودن فیلدهای اجباری
            // + با در نظر گرفتن سلکت جی یو آی دی
            if (requiredFields.Contains(propertyName))
            {
                bool isSelect = propInfo.GetCustomAttributes(typeof(SelectListMetaAttribute), true).Any();

                var propType = propInfo?.PropertyType;

                if (isSelect && (propType == typeof(Guid) || propType == typeof(Guid?)))
                {
                    var guidValue = propInfo?.GetValue(model) as Guid?;
                    if (guidValue == null || guidValue == Guid.Empty)
                    {
                        if (showError) ReplaceError("لطفاً یک مورد را انتخاب کنید");
                        return false;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        if (showError)
                        {
                            ReplaceError(isSelect
                                ? "لطفاً یک مورد را انتخاب کنید"
                                : "لطفاً این فیلد را پر کنید");
                        }
                        return false;
                    }
                }
            }
            
            // چک مکس لنگت ارور
            // اگرارور مکس لنگت  داره اولیت با اونه اول اون باید برطرف
            string maxLength_error = model.MaxLengthError(propertyName);
            if (maxLength_error != null)
            {
                if (showError) ReplaceError(maxLength_error);
                return false;
            }
            // اگز بدون ارور و اسلایسی هست نباید برای کاربر بیشتر زدن ممکن باشه
            if (!model.IsPossibleToUser_MaxLength(propertyName))
            {
                propInfo?.SetValue(model, "");
                if (showError) ReplaceError("خطا در دریافت ورودی");
                return false;
            }

            switch (propertyName)
            {
                case nameof(model.EditCode):
                    if (!value.IsPossibleToUser_Numeric())
                    {
                        if (showError) ReplaceError("خطا در دریافت ورودی");
                        return false;
                    }
                    if (!value.IsValidNumeric())
                    {
                        if (showError) ReplaceError("ورودی نامعتبر است");
                        return false;
                    }
                    break;

                case nameof(model.EditTitle):
                    if (!value.IsPossibleToUser_Name())
                    {
                        if (showError) ReplaceError("خطا در دریافت ورودی");
                        return false;
                    }
                    if (!value.IsValidName())
                    {
                        if (showError) ReplaceError("ورودی نامعتبر است");
                        return false;
                    }
                    break;
            }

            if (showError) ReplaceError("");
            return true;
        }

    }
}