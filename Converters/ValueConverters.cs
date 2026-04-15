using System;
using System.Globalization;
using Avalonia.Data.Converters;
using PhantomOS.Models;
using Avalonia.Media;

namespace PhantomOS.Converters
{
    public class EqualsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return value.ToString() == parameter.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SeverityToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Severity severity)
            {
                return severity switch
                {
                    Severity.Critical => Brush.Parse("#FF3333"), // Red
                    Severity.High => Brush.Parse("#FF9933"),     // Orange
                    Severity.Medium => Brush.Parse("#FFFF33"),   // Yellow
                    Severity.Low => Brush.Parse("#33CCFF"),      // Blue
                    _ => Brush.Parse("#AAAAAA")                  // Gray
                };
            }
            return Brush.Parse("#AAAAAA");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
