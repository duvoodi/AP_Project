using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AP_Project.Helpers.FormUtils
{
    public static class FormViewModelExtensions
    {
        public static void NullFieldsToEmpty<TModel>(this TModel FormViewMode)
        {
            if (FormViewMode == null) return;

            var properties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(string) && prop.CanRead && prop.CanWrite)
                {
                    var value = (string)prop.GetValue(FormViewMode);
                    if (value == null)
                        prop.SetValue(FormViewMode, "");
                }
            }
        }
        
        public static void ValidateMaxLengths<TModel>(this ModelStateDictionary modelState, TModel model)
        {
            if (model == null) return;

            var properties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var stringLengthAttr = prop.GetCustomAttribute<StringLengthAttribute>();
                if (stringLengthAttr == null) continue;

                var value = prop.GetValue(model) as string;
                if (value != null && value.Length > stringLengthAttr.MaximumLength)
                {
                    var fieldName = prop.Name;
                    string errorMessage = stringLengthAttr.ErrorMessage
                        ?? $"حداکثر {stringLengthAttr.MaximumLength} کاراکتر مجاز است";
                    modelState.ReplaceModelError(fieldName, errorMessage);
                }
            }
        }
    }
}
