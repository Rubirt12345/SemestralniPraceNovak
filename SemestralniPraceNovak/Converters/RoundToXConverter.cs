using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SemestralniPraceNovak.Converters
{
    public class RoundToXConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            if (!int.TryParse(value.ToString(), out int round)) return 0;
            return round * 180; 
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}