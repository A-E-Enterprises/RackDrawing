using DrawingControl;
using System;
using System.Globalization;
using System.Windows.Data;

namespace RackDrawingApp
{
	// Convert pitch direction(ePitchDirection) to the string, which is used in the ShedRoof direction combobox in EditRoofDialog.
	public class ShedRoofDirectionConverter : IValueConverter
	{
		//
		public static string STR_SHEDROOF_LEFT_TO_RIGHT = "Left to right";
		public static string STR_SHEDROOF_RIGHT_TO_LEFT = "Right to left";
		public static string STR_SHEDROOF_TOP_TO_BOTTOM = "Top to bottom";
		public static string STR_SHEDROOF_BOTTOM_TO_TOP = "Bottom to top";

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is ShedRoof.ePitchDirection)
			{
				ShedRoof.ePitchDirection pitchDirection = (ShedRoof.ePitchDirection)value;
				if (ShedRoof.ePitchDirection.eLeftToRight == pitchDirection)
					return STR_SHEDROOF_LEFT_TO_RIGHT;
				else if (ShedRoof.ePitchDirection.eRightToLeft == pitchDirection)
					return STR_SHEDROOF_RIGHT_TO_LEFT;
				else if (ShedRoof.ePitchDirection.eTopToBot == pitchDirection)
					return STR_SHEDROOF_TOP_TO_BOTTOM;
				else if (ShedRoof.ePitchDirection.eBotToTop == pitchDirection)
					return STR_SHEDROOF_BOTTOM_TO_TOP;
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && value is string)
			{
				string strValue = (string)value;
				if (STR_SHEDROOF_LEFT_TO_RIGHT == strValue)
					return ShedRoof.ePitchDirection.eLeftToRight;
				else if (STR_SHEDROOF_RIGHT_TO_LEFT == strValue)
					return ShedRoof.ePitchDirection.eRightToLeft;
				else if (STR_SHEDROOF_TOP_TO_BOTTOM == strValue)
					return ShedRoof.ePitchDirection.eTopToBot;
				else if (STR_SHEDROOF_BOTTOM_TO_TOP == strValue)
					return ShedRoof.ePitchDirection.eBotToTop;
			}

			return ShedRoof.ePitchDirection.eLeftToRight;
		}
	}
}
