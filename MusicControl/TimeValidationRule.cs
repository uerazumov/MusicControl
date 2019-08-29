using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MusicControl
{
    class TimeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var regex = new Regex(@"^\d+$");

            if (!regex.IsMatch(value as string))
                return new ValidationResult(false, "Неправильный формат имени");
            return new ValidationResult(true, String.Empty);
        }
    }
}
