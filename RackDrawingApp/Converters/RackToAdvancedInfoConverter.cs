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
	public class RackToAdvancedInfoConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string strResult = string.Empty;

			Rack _rack = value as Rack;
			if(_rack != null)
			{
				strResult += _rack.Text;
				strResult += " ";
				strResult += _rack.Length_X.ToString(".");
				strResult += "x";
				strResult += _rack.Length_Y.ToString(".");
				strResult += "x";
				strResult += _rack.Length_Z.ToString(".");
			}

			return strResult;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}
}
