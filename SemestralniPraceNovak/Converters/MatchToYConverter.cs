using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SemestralniPraceNovak.Converters
{
    public class MatchToYConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            if (!int.TryParse(value.ToString(), out int id)) return 0;
            return id * 60; 
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}