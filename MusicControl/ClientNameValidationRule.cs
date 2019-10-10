using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace MusicControl
{
    class ClientNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            //var regex = new Regex(@"^([А-Яа-я]{3,14})$");
            var regex = new Regex(@"^[\p{L} \.'\-]+$");

            if (!regex.IsMatch(value as string))
                return new ValidationResult(false, "Неправильный формат имени");
            return new ValidationResult(true, String.Empty);
        }
    }
}
