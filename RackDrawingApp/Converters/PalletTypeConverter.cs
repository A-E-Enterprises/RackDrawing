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
	public class PalletTypeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//return string
			if(value is ePalletType)
			{
				ePalletType _type = (ePalletType)value;
				if (_type == ePalletType.eOverhang)
					return Rack.PALLET_OVERHANG;
				else if (_type == ePalletType.eFlush)
					return Rack.PALLET_FLUSH;
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// return ePalletType
			string strVal = value as string;
			if(!string.IsNullOrEmpty(strVal))
			{
				if (strVal == Rack.PALLET_OVERHANG)
					return ePalletType.eOverhang;
				else if (strVal == Rack.PALLET_FLUSH)
					return ePalletType.eFlush;
			}

			return ePalletType.eOverhang;
		}
	}
}
