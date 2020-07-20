using System;
using System.Globalization;
using System.Windows.Data;

namespace RackDrawingApp
{
	public class NullEmptyStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string strValue = value as string;
			return string.IsNullOrEmpty(strValue);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
