using AppColorTheme;
using System;
using System.Windows.Media;

namespace DrawingControl
{
	/// <summary>
	/// Current colors theme.
	/// Which colors should be used for display geometry.
	/// </summary>
	public static class CurrentGeometryColorsTheme
	{
		private static IGeometryColorsTheme m_CurrentTheme = new DefaultGeometryColorsTheme();
		public static IGeometryColorsTheme CurrentTheme
		{
			get { return m_CurrentTheme; }
			set { m_CurrentTheme = value; }
		}

		public static Brush GetRackAdvProps_ColumnFillBrush()
		{
			Color columnFillColor = Colors.DarkBlue;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackAdvProp_ColumnFillColor, out colorValue))
				columnFillColor = colorValue;

			return new SolidColorBrush(columnFillColor);
		}

		public static Brush GetRackAdvProps_BottomLineBrush()
		{
			Color bottomLineColor = Colors.Black;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackAdvProp_BottomLineColor, out colorValue))
				bottomLineColor = colorValue;

			return new SolidColorBrush(bottomLineColor);
		}

		public static Brush GetRackAdvProps_LevelShelfBrush()
		{
			Color levelShelfColor = Colors.DarkOrange;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackAdvProp_LevelShelfColor, out colorValue))
				levelShelfColor = colorValue;

			return new SolidColorBrush(levelShelfColor);
		}

		public static Brush GetRackAdvProps_DimensionsBrush()
		{
			Color dimensionsColor = Colors.DarkBlue;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackAdvProp_DimensionsColor, out colorValue))
				dimensionsColor = colorValue;

			return new SolidColorBrush(dimensionsColor);
		}

		public static Brush GetRackAdvProps_BracingLinesBrush()
		{
			Color bracingLinesColor = Colors.Gray;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackAdvProp_BracingLinesColor, out colorValue))
				bracingLinesColor = colorValue;

			return new SolidColorBrush(bracingLinesColor);
		}

		public static Brush GetRackAdvProps_TextBrush()
		{
			Color textColor = Colors.Black;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackAdvProp_TextColor, out colorValue))
				textColor = colorValue;

			return new SolidColorBrush(textColor);
		}

		public static Brush GetRackAdvProps_PalletBorderBrush()
		{
			Color palletBorderColor = Colors.SaddleBrown;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackAdvProp_PalletBorderColor, out colorValue))
				palletBorderColor = colorValue;

			return new SolidColorBrush(palletBorderColor);
		}

		public static Brush GetRackAdvProps_PalletFillBrush()
		{
			Color palletFillColor = Colors.Tan;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackAdvProp_PalletFillColor, out colorValue))
				palletFillColor = colorValue;

			return new SolidColorBrush(palletFillColor);
		}

		public static Brush GetRackAdvProps_PalletRiserBorderBrush()
		{
			Color palletRiserBorderColor = Colors.SaddleBrown;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackAdvProp_PalletRiserBorderColor, out colorValue))
				palletRiserBorderColor = colorValue;

			return new SolidColorBrush(palletRiserBorderColor);
		}

		public static Brush GetRackAdvProps_PalletRiserFillBrush()
		{
			Color palletRiserFillColor = Colors.Sienna;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackAdvProp_PalletRiserFillColor, out colorValue))
				palletRiserFillColor = colorValue;

			return new SolidColorBrush(palletRiserFillColor);
		}

		public static Brush GetRackAdvProps_DeckingPlateFillBrush()
		{
			Color deckingPlateFillColor = Colors.DarkSlateGray;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackAdvProp_DeckingPlateFillColor, out colorValue))
				deckingPlateFillColor = colorValue;

			return new SolidColorBrush(deckingPlateFillColor);
		}

		/// <summary>
		/// Returns fill color type for geometry
		/// </summary>
		public static eColorType GeometryToFillColorType(BaseRectangleGeometry geom)
		{
			eColorType colorType = eColorType.eUndefined;

			if (geom is Block)
				colorType = eColorType.eFill_Block;
			else if (geom is AisleSpace)
				colorType = eColorType.eFill_AisleSpace;
			else if (geom is Column)
				colorType = eColorType.eFill_Column;
			else if (geom is Shutter)
				colorType = eColorType.eFill_Shutter;
			else if (geom is TieBeam)
				colorType = eColorType.eFill_TieBeam;
			else if (geom is Wall)
				colorType = eColorType.eFill_Wall;
			else if (geom is SheetElevationGeometry)
				colorType = eColorType.eFill_SheetElevations;

			return colorType;
		}

		/// <summary>
		/// Returns rack fill color type, which depends on rack index.
		/// </summary>
		public static eColorType RackToFillColorType(int rackSizeIndex)
		{
			eColorType colorType = eColorType.eFillRackDefault;

			if (0 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_00;
			else if (1 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_01;
			else if (2 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_02;
			else if (3 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_03;
			else if (4 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_04;
			else if (5 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_05;
			else if (6 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_06;
			else if (7 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_07;
			else if (8 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_08;
			else if (9 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_09;
			else if (10 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_10;
			else if (11 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_11;
			else if (12 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_12;
			else if (13 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_13;
			else if (14 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_14;
			else if (15 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_15;
			else if (16 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_16;
			else if (17 == rackSizeIndex)
				colorType = eColorType.eFillRackIndex_17;

			return colorType;
		}

        internal static Brush GetRackAdvProps_RackGuardMainBrush()
        {
			Color defaultColor = Colors.Black;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackGuardMainColorDefault, out colorValue))
				defaultColor = colorValue;

			return new SolidColorBrush(defaultColor);
		}

        internal static Brush GetRackAdvProps_RackGuardAltBrush()
        {
			Color defaultColor = Colors.DarkSlateGray;

			Color colorValue;
			if (m_CurrentTheme != null && m_CurrentTheme.GetGeometryColor(eColorType.eRackGuardAltColorDefault, out colorValue))
				defaultColor = colorValue;

			return new SolidColorBrush(defaultColor);
		}
    }
}
