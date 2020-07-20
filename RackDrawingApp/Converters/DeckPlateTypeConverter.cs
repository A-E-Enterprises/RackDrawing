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
	public class DeckPlateTypeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// return string
			if(value is eDeckPlateType)
			{
				eDeckPlateType _type = (eDeckPlateType)value;
				if (_type == eDeckPlateType.eAlongDepth_UDL)
					return Rack.DECK_PLATE_TYPE_ALONG_DEPTH_UDL;
				else if (_type == eDeckPlateType.eAlongDepth_PalletSupport)
					return Rack.DECK_PLATE_TYPE_ALONG_DEPTH_PALLET_SUPPORT;
				else if (_type == eDeckPlateType.eAlongLength)
					return Rack.DECK_PLATE_TYPE_ALONG_LENGTH;
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// return eDeckingPanelType
			string strVal = value as string;
			if(!string.IsNullOrEmpty(strVal))
			{
				if (strVal == Rack.DECK_PLATE_TYPE_ALONG_DEPTH_UDL)
					return eDeckPlateType.eAlongDepth_UDL;
				if (strVal == Rack.DECK_PLATE_TYPE_ALONG_DEPTH_PALLET_SUPPORT)
					return eDeckPlateType.eAlongDepth_PalletSupport;
				else if (strVal == Rack.DECK_PLATE_TYPE_ALONG_LENGTH)
					return eDeckPlateType.eAlongLength;
			}

			return eDeckPlateType.eAlongDepth_UDL;
		}
	}
}
