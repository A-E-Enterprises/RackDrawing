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
	public class BracingTypeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// return string
			if(value is eBracingType)
			{
				eBracingType _type = (eBracingType)value;
				if (_type == eBracingType.eGI)
					return Rack.BRACING_TYPE_GI;
				else if (_type == eBracingType.ePowderCoated)
					return Rack.BRACING_TYPE_POWDER_COATED;
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// return eBracingType
			string strVal = value as string;
			if(!string.IsNullOrEmpty(strVal))
			{
				if (strVal == Rack.BRACING_TYPE_GI)
					return eBracingType.eGI;
				else if (strVal == Rack.BRACING_TYPE_POWDER_COATED)
					return eBracingType.ePowderCoated;
			}

			return eBracingType.eGI;
		}
	}
}
