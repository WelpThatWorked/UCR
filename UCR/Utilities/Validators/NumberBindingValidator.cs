﻿using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using HidWizards.UCR.Views.Controls.Plugin;

namespace HidWizards.UCR.Utilities.Validators
{
    public class NumberBindingValidator : ValidationRule
    {
        public PluginPropertyDependencyObject PluginPropertyDependencyObject { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = (string) value;
            if (text == null) return new ValidationResult(false, "No input");

            var regex = new Regex(@"^[-+]?[0-9]+(\.?[0-9]+)?$");
            if (!regex.IsMatch(text)) return new ValidationResult(false, "Invalid input for number");

            if (int.TryParse(text, out var result))
            {
                var propertyValidationResult = PluginPropertyDependencyObject.PluginProperty.Validate(result);
                if (!propertyValidationResult.IsValid)
                    return new ValidationResult(false, propertyValidationResult.ErrorMessage);
            }

            return ValidationResult.ValidResult;
        }
    }
}