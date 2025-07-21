using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AP_Project.Data;
using AP_Project.Helpers;
using AP_Project.Helpers.FormUtils;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AP_Project.FormViewModels.ClassForm
{
    public class ClassFormViewModel
    {
        [SelectListMeta(true)]
        public Guid ClassLocationId { get; set; }

        [SelectListMeta(true)]
        public Guid SectionId { get; set; }

        public List<Guid> StudentIds { get; set; } = new List<Guid>();
    }

    public static class ClassFormViewModelExtensions
    {
        public static bool ValidateField(this ModelStateDictionary modelState, ClassFormViewModel model, string propertyName, bool showError = true)
        {
            var propInfo = typeof(ClassFormViewModel).GetProperty(propertyName);
            var value = propInfo?.GetValue(model)?.ToString() ?? "";

            void ReplaceError(string message) => modelState.ReplaceModelError(propertyName, message);

            // تو خود کنترلر مقدارش با دیتابیس گرفتیم و کردیم
            if (propertyName == nameof(model.StudentIds))
                return true;

            // فیلد های اجباری
            var requiredFields = new[] {
                nameof(model.ClassLocationId),
                nameof(model.SectionId)
            };

            // چک خالی نبودن فیلد های اجباری
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

            if (showError) ReplaceError("");
            return true;
        }
    }
}