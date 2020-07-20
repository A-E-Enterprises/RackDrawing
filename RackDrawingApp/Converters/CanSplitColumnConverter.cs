using DrawingControl;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RackDrawingApp
{
	public class CanSplitColumnConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// true - can split column
			// false - cant
			Rack _rack = value as Rack;
			if (_rack != null && _rack.Length_Z >= 5500)
				return true;

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}
}
