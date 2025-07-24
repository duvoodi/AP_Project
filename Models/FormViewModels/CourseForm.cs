using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AP_Project.Helpers;
using AP_Project.Helpers.FormUtils;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AP_Project.FormViewModels.CourseForm
{
    public class CourseFormViewModel
    {
        [SelectListMeta(true)]
        public Guid CourseCodeId { get; set; }

        [MaxLengthMeta(1)]
        [AllowSliceMeta(true)]
        public string Unit { get; set; }

        [MaxLengthMeta(5)]
        [AllowSliceMeta(true)]
        public string Semester { get; set; }

        [SelectListMeta(true)]
        public List<Guid> PrerequisiteIds { get; set; } = new List<Guid>();

        [SelectListMeta(true)]
        public Guid? InstructorId { get; set; }

        [SelectListMeta(true)]
        public int TimeSlotId { get; set; }

        [MaxLengthMeta(10)]
        [AllowSliceMeta(false)]
        public string FinalExamDate { get; set; }

        [MaxLengthMeta(160)]
        [AllowSliceMeta(false)]
        public string Description { get; set; }
    }

    public static class CourseFormViewModelExtensions
    {
        public static bool ValidateField(this ModelStateDictionary modelState, CourseFormViewModel model, string propertyName, bool showError = true)
        {
            var propInfo = typeof(CourseFormViewModel).GetProperty(propertyName);
            var value = propInfo?.GetValue(model)?.ToString() ?? "";
            var existingError = modelState[propertyName]?.Errors.FirstOrDefault()?.ErrorMessage;

            void ReplaceError(string message) => modelState.ReplaceModelError(propertyName, message);

            // تو خود کنترلر مقدارش با دیتابیس گرفتیم و کردیم
            if (propertyName == nameof(model.PrerequisiteIds))
                return true;

            // چک نکردن فیلد های اختیاری خالی
            var optional = new[] { 
                nameof(model.InstructorId), 
                nameof(model.Description), 
            };
            if (optional.Contains(propertyName) && string.IsNullOrWhiteSpace(value))
            {
                if (showError) ReplaceError("");
                return true;
            }

            // فیلد های اجباری
            var requiredFields = new[] { 
                nameof(model.CourseCodeId), 
                nameof(model.Unit), 
                nameof(model.Semester), 
                nameof(model.TimeSlotId), 
                nameof(model.FinalExamDate) 
            };


            // چک خالی نبودن فیلد های اجباری
            // + با در نظر گرفتن سلکت جی یو آی دی و سلکت اینت
            if (requiredFields.Contains(propertyName))
            {
                bool isSelect = propInfo.GetCustomAttributes(typeof(SelectListMetaAttribute), true).Any();
                var propType = propInfo?.PropertyType;

                if (isSelect)
                {
                    // بررسی برای Guid یا Guid?
                    if (propType == typeof(Guid) || propType == typeof(Guid?))
                    {
                        var guidValue = propInfo?.GetValue(model) as Guid?;
                        if (guidValue == null || guidValue == Guid.Empty)
                        {
                            if (showError) ReplaceError("لطفاً یک مورد را انتخاب کنید");
                            return false;
                        }
                    }
                    // بررسی برای int
                    else if (propType == typeof(int))
                    {
                        var intValue = (int)(propInfo?.GetValue(model) ?? 0);
                        if (intValue == 0)
                        {
                            if (showError) ReplaceError("لطفاً یک مورد را انتخاب کنید");
                            return false;
                        }
                    }
                }

                // بررسی خالی بودن فیلدهای غیر Select (متنی یا رشته‌ای)
                if (!isSelect && string.IsNullOrWhiteSpace(value))
                {
                    if (showError) ReplaceError("لطفاً این فیلد را پر کنید");
                    return false;
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
                case nameof(model.Unit):
                    if (!value.IsPossibleToUser_Numeric())
                    {
                        if (showError) ReplaceError("خطا در دریافت ورودی");
                        return false;
                    }
                    if (!value.IsValidNumeric())
                    {
                        if (showError) ReplaceError("تعداد واحد نامعتبر است");
                        return false;
                    }
                    var unitValue = int.Parse(value);
                    if (unitValue < 1 || unitValue > 4)
                    {
                        if (showError) ReplaceError("تعداد واحد باید بین 1 تا 4 باشد");
                        return false;
                    }
                    break;

                case nameof(model.Semester):
                    if (!value.IsValidSemesterDate(out var Semester_errorMessage))
                    {
                        if (showError) ReplaceError(Semester_errorMessage);
                        return false;
                    }
                    break;

                case nameof(model.FinalExamDate):
                    if (!value.IsPossibleToUser_Date())
                    {
                        if (showError) ReplaceError("ورودی نامعتبر است");
                        return false;
                    }
                    if (!value.IsValidExamDateWithSemester(model.Semester, out var FinalExamDate_errorMessage))
                    {
                        if (showError) ReplaceError(FinalExamDate_errorMessage);
                        return false;
                    }
                    break;

                case nameof(model.Description):
                    if (!value.IsValidText())
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