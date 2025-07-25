using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AP_Project.Helpers.FormUtils
{
    public static class InputValidatorExtensions
    {

        // تابع تشخیص ارور مکس لنگت
        public static string? MaxLengthError<TModel>(this TModel model, string propertyName)
        {
            var propInfo = typeof(TModel).GetProperty(propertyName);
            if (propInfo == null) 
                return null;

            // ۱) اگر AllowSliceMetaAttribute و AllowSlice==true داشته باشد، هیچ خطایی بازنمی‌گردانیم
            var allowSliceMeta = propInfo
                .GetCustomAttributes(typeof(AllowSliceMetaAttribute), false)
                .OfType<AllowSliceMetaAttribute>()
                .FirstOrDefault();
            if (allowSliceMeta?.AllowSlice == true)
                return null;

            // ۲) اگر MaxLengthMetaAttribute نداشته باشد، نال برگردان
            var maxLenMeta = propInfo
                .GetCustomAttributes(typeof(MaxLengthMetaAttribute), false)
                .OfType<MaxLengthMetaAttribute>()
                .FirstOrDefault();
            if (maxLenMeta == null) 
                return null;

            // ۳) مقدار فعلی پراپرتی را رشته‌‌ای کن
            var value = propInfo.GetValue(model)?.ToString() ?? string.Empty;

            // ۴) چک طول و در صورت تجاوز، پیام خطا را برگردان
            int maxLength = maxLenMeta.MaxLength;
            if (value.Length > maxLength)
            {
                return $"حداکثر {maxLength} کاراکتر مجاز است";
            }

            // ۵) در صورت عدم خطا
            return null;
        }

        // چک ممکن بودن ورودی با مکس لنگت
        public static bool IsPossibleToUser_MaxLength<TModel>(this TModel model, string propertyName)
        {
            var propInfo = typeof(TModel).GetProperty(propertyName);
            if (propInfo == null) return true;

            var value = propInfo.GetValue(model)?.ToString() ?? string.Empty;

            var maxLenMeta = propInfo
                .GetCustomAttributes(typeof(MaxLengthMetaAttribute), false)
                .OfType<MaxLengthMetaAttribute>()
                .FirstOrDefault();
            int? maxLength = maxLenMeta?.MaxLength;

            var sliceMeta = propInfo
                .GetCustomAttributes(typeof(AllowSliceMetaAttribute), false)
                .OfType<AllowSliceMetaAttribute>()
                .FirstOrDefault();
            bool allowSlice = sliceMeta?.AllowSlice ?? false;

            if (!maxLength.HasValue) return true;
            if (!allowSlice && value.Length > maxLength.Value) return false;
            return true;
        }

        // ولید ایمیل
        public static bool isPossibleToUser_Email(this string value)

        {
            if (string.IsNullOrEmpty(value)) return true;
            if (value.Count(c => c == '@') > 1) return false;
            return Regex.IsMatch(value, @"^[A-Za-z0-9!#$%&'*+\-/=?^_`{|}~@.]*$");
        }

        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            if (email.Contains(' ')) return false;
            if (email.Length > 320) return false;

            var parts = email.Split('@');
            if (parts.Length != 2) return false;

            var local = parts[0];
            var domain = parts[1];
            if (local.Length > 64) return false;
            if (!Regex.IsMatch(local, @"^[a-zA-Z0-9!#$%&'*+\-/=?^_`{|}~-]+$")) return false;

            var segments = domain.Split('.');
            if (segments.Length < 2) return false;
            if (domain.StartsWith('.') || domain.EndsWith('.')) return false;

            if (domain.Length > 255) return false;
            if (segments.Last().Length < 2) return false;
            if (!Regex.IsMatch(domain, @"^[a-zA-Z0-9]+([-.][a-zA-Z0-9]+)*$")) return false;

            return true;
        }

        // ولید نیم
        public static bool IsPossibleToUser_Name(this string value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            if (value.StartsWith(' ') || value.EndsWith(' ')) return false;
            if (Regex.IsMatch(value, @" {2,}")) return false;
            return Regex.IsMatch(value, @"^[\u0600-\u06FFa-zA-Z0-9 ]*$");
        }

        public static bool IsValidName(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value.IsPossibleToUser_Name();
        }

        // ولید نومریک
        public static bool IsPossibleToUser_Numeric(this string value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            return Regex.IsMatch(value, @"^[0-9]*$");
        }

        public static bool IsValidNumeric(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value.IsPossibleToUser_Numeric();
        }

        // ولید پسورد
        public static bool IsPossibleToUser_Password(this string value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            return Regex.IsMatch(value, @"^[a-zA-Z0-9!#$%&'*+/=?^_`{|}~@\-.,:;""()\[\]{}<>]*$");
        }

        public static bool IsValidPassword(this string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            return password.IsPossibleToUser_Password();
        }

        // ولید پرشین دیت
        public static bool IsPossibleToUser_Date(this string str)
        {
            if (string.IsNullOrEmpty(str)) return true;
            return Regex.IsMatch(str, @"^[0-9/]*$");
        }

        public static bool IsValidPersianDate(this string value)
        {
            return PersianCalendarUtils.IsValidPersianDate(value);
        }


        public static bool IsValidSemesterDate(this string value, out string errorMessage)
        {
            return PersianCalendarUtils.IsValidSemesterDate(value, out errorMessage);
        }

        public static bool IsValidExamDateWithSemester(this string value, string semester, out string errorMessage)
        {
            return PersianCalendarUtils.IsValidExamDateWithSemester(value, semester, out errorMessage);
        }

        // ولید پرسن نیم
        public static bool IsPossibleToUser_PersonName(this string value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            if (value.StartsWith(' ') || value.EndsWith(' ')) return false;
            if (Regex.IsMatch(value, " {2,}")) return false;
            return Regex.IsMatch(value, "^[\u0600-\u06FF ]*$");
        }
        
        public static bool IsValidPersonName(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value.IsPossibleToUser_PersonName();
        }

        // ولید تکست
        public static bool IsPossibleToUser_Text(this string value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            if (value.StartsWith(' ') || Regex.IsMatch(value, @" {2,}")) return false;

            // فقط کاراکترهای مجاز: فارسی، انگلیسی، اعداد، فاصله، و علائم نگارشی مجاز
            return Regex.IsMatch(value, @"^[\u0600-\u06FFa-zA-Z0-9 ؛،.,!?؟()\[\]{}\-_–—]*$");
        }

        public static bool IsValidText(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return value.IsPossibleToUser_Text();
        }

        // ولید یوزرنیم
        public static bool IsPossibleToUser_Username(this string value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            if (Regex.IsMatch(value, @"\s")) return false;
            if (!Regex.IsMatch(value, @"^[a-zA-Z0-9_.]+$")) return false;
            if (Regex.IsMatch(value, @"(\.\.|__)")) return false;
            return true;
        }

        public static bool IsValidUsername(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value.IsPossibleToUser_Username();
        }
    }
}
