using Microsoft.AspNetCore.Mvc.ModelBinding;
using AP_Project.Helpers.FormUtils;
using System.Globalization;

namespace AP_Project.FormViewModels.InstructorForm
{
    public class InstructorFormViewModel
    {
        [MaxLengthMeta(32)]
        [AllowSliceMeta(false)]
        public string FirstName { get; set; }

        [MaxLengthMeta(32)]
        [AllowSliceMeta(false)]
        public string LastName { get; set; }

        [MaxLengthMeta(320)]
        [AllowSliceMeta(false)]
        public string Email { get; set; }

        public string InstructorId { get; set; }
        
        [MaxLengthMeta(9)]
        [AllowSliceMeta(true)]
        public string Salary { get; set; }

        [MaxLengthMeta(4)]
        [AllowSliceMeta(true)]
        public string HireYear { get; set; }

        [MaxLengthMeta(32)]
        [AllowSliceMeta(false)]
        public string Password { get; set; }

        [MaxLengthMeta(32)]
        [AllowSliceMeta(false)]
        public string ConfirmPassword { get; set; }
    }

    public static class InstructorFormViewModelExtensions
    {
        // تابع اعتبارسنجی یک فیلد بر اساس پراپرتی مدل
        public static bool ValidateField(this ModelStateDictionary modelState, InstructorFormViewModel model, string propertyName, bool showError = true)
        {
            
            var propInfo = typeof(InstructorFormViewModel).GetProperty(propertyName);
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
                    modelState.ReplaceModelError(propname, message);
            }

            // 1. چک خالی بودن فیلدهای اجباری
            var required = new[] {
                nameof(model.FirstName),
                nameof(model.LastName),
                nameof(model.Email),
                nameof(model.HireYear),
                nameof(model.Salary),
                nameof(model.Password),
                nameof(model.ConfirmPassword)
            };
            if (required.Contains(propertyName) && string.IsNullOrWhiteSpace(value))
            {
                ReplaceError_IfAllowed(propertyName, "لطفاً این فیلد را پر کنید");
                return false;
            }

            // 2. چک مکس لنگت ارور
            // 2.1 اگرارور مکس لنگت  داره اولیت با اونه اول اون باید برطرف
            if (existingError.IsMaxLengthError())
            {
                // همون ارور را نگه میداریم
                return false;
            }
            // 2.2 اگز بدون ارور و اسلایسی هست نباید برای کاربر بیشتر زدن ممکن باشه
            if (!model.IsPossibleToUser_MaxLength(propertyName))
            {
                propInfo?.SetValue(model, "");
                ReplaceError_IfAllowed(propertyName, "خطا در دریافت ورودی");
                return false;
            }

            // 3. چک های اختصاصی هر فیلد
            switch (propertyName)
            {
                case nameof(model.FirstName):
                case nameof(model.LastName):
                    value = value.Trim();
                    propInfo?.SetValue(model, value);
                    if (!value.IsPossibleToUser_PersonName())
                    {
                        propInfo?.SetValue(model, "");
                        ReplaceError_IfAllowed(propertyName, "خطا در دریافت ورودی");
                        return false;
                    }
                    if (!value.IsValidPersonName())
                    {
                        ReplaceError_IfAllowed(propertyName, "ورودی نامعتبر است");
                        return false;
                    }
                    break;

                // چک ایمیل
                case nameof(model.Email):
                    if (!value.isPossibleToUser_Email())
                    {
                        propInfo?.SetValue(model, "");
                        ReplaceError_IfAllowed(propertyName, "خطا در دریافت ورودی");
                        return false;
                    }
                    if (!value.IsValidEmail())
                    {
                        ReplaceError_IfAllowed(propertyName, "ورودی نامعتبر است");
                        return false;
                    }
                    break;

                // چک کد مدرسی (InstructorId)
                case nameof(model.InstructorId):
                    if (!modelState.ValidateField(model, nameof(model.HireYear), false))
                    {
                        ReplaceError_IfAllowed(propertyName, "");
                        propInfo?.SetValue(model, "");
                        return false;
                    }
                    var enteredHireYear = model.HireYear;
                    if (!value.IsValidNumeric() || value.Length != 9)
                    {
                        propInfo?.SetValue(model, "");
                        ReplaceError_IfAllowed(propertyName, "خطا در دریافت کد مدرسی");
                        return false;
                    }
                    var prefixExpected = enteredHireYear.Substring(1, 3);
                    var prefixActual = value.Substring(0, 3);
                    var mid = int.Parse(value[3].ToString());

                    if (prefixActual != prefixExpected || mid < 5 || mid > 9)
                    {
                        propInfo?.SetValue(model, "");
                        ReplaceError_IfAllowed(propertyName, "خطا در دریافت کد مدرسی");
                        return false;
                    }
                    break;

                // چک حقوق
                case nameof(model.Salary):
                    if (!value.IsPossibleToUser_Numeric())
                    {
                        propInfo?.SetValue(model, "");
                        ReplaceError_IfAllowed(propertyName, "خطا در دریافت ورودی");
                        return false;
                    }
                    if (!value.IsValidNumeric())
                    {
                        ReplaceError_IfAllowed(propertyName, "ورودی نامعتبر است");
                        return false;
                    }
                    break;

                // چک سال استخدام
                case nameof(model.HireYear):
                    if (!value.IsPossibleToUser_Numeric())
                    {
                        propInfo?.SetValue(model, "");
                        ReplaceError_IfAllowed(propertyName, "خطا در دریافت ورودی");
                        return false;
                    }
                    if (!value.IsValidNumeric())
                    {
                        ReplaceError_IfAllowed(propertyName, "ورودی نامعتبر است");
                        return false;
                    }
                    if (value.TrimStart('0').Length < 4)
                    {
                        ReplaceError_IfAllowed(propertyName, "ورودی خارج از بازه مجاز است");
                        return false;
                    }

                    var parsedHireYear = int.Parse(value);
                    var pc = new PersianCalendar();
                    int currentPersianYear = pc.GetYear(DateTime.Now);
                    if (parsedHireYear < 1300 || parsedHireYear > currentPersianYear + 1) // تا سال بعدی رو میتواند زودتر وارد کند
                    {
                        ReplaceError_IfAllowed(propertyName, "ورودی خارج از بازه مجاز است");
                        return false;
                    }
                    break;

                // چک پسورد
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

                // چک تکرار پسورد
                case nameof(model.ConfirmPassword):
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
                    if (!modelState.ValidateField(model, nameof(model.Password), false))
                    {
                        break;
                    }

                    var pw = model.Password;
                    var cpw = value;

                    if (pw != cpw)
                    {
                        ReplaceError_IfAllowed(propertyName, "ورودی با رمز عبور مطابقت ندارد");
                        return false;
                    }
                    break;
            }

            ReplaceError_IfAllowed(propertyName, "");
            return true;
        }
    }
}