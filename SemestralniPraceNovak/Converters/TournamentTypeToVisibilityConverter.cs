using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SemestralniPraceNovak.Converters
{
	public class TournamentTypeToBoolConverter : IValueConverter
	{
		
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null) return false;
			var actual = value.ToString();
			var expected = parameter.ToString();
			return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}