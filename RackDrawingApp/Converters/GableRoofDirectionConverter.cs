using System;
using System.Globalization;
using System.Windows.Data;

namespace RackDrawingApp
{
	// Convert roof direction(bool value) to the string, which is used in the GableRoof direction combobox in EditRoofDialog.
	public class GableRoofDirectionConverter : IValueConverter
	{
		// true - horizontal
		public static string STR_GABLEROOF_HORIZONTAL = "Horizontal";
		// false - vertical
		public static string STR_GABLEROOF_VERTICAL = "Vertical";

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is bool)
			{
				bool bValue = (bool)value;
				if (bValue)
					return STR_GABLEROOF_HORIZONTAL;
				else
					return STR_GABLEROOF_VERTICAL;
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value != null && value is string)
			{
				string strValue = (string)value;
				if (STR_GABLEROOF_HORIZONTAL == strValue)
					return true;
				else if (STR_GABLEROOF_VERTICAL == strValue)
					return false;
			}

			return true;
		}
	}
}
