using System.Windows.Media;

namespace AppColorTheme
{
	/// <summary>
	/// Geometry color theme with default geometry colors.
	/// </summary>
	public class DefaultGeometryColorsTheme : GeometryColorTheme
	{
		public DefaultGeometryColorsTheme()
		{
			// add default colors here
			SetGeometryColor(eColorType.eFill_Block, Colors.LightGray);
			SetGeometryColor(eColorType.eFill_AisleSpace, Colors.Khaki);
			SetGeometryColor(eColorType.eFill_Column, Colors.LightSkyBlue);
			SetGeometryColor(eColorType.eFill_Shutter, Colors.Pink);
			SetGeometryColor(eColorType.eFill_TieBeam, Colors.Gray);
			SetGeometryColor(eColorType.eFill_TieBeamWithError, Colors.Red);
			SetGeometryColor(eColorType.eFill_Wall, Colors.DarkSlateGray);
			SetGeometryColor(eColorType.eFill_Roof, Colors.LightSalmon);
			SetGeometryColor(eColorType.eFill_Floor, Colors.LightGray);
			SetGeometryColor(eColorType.eFill_SheetElevations, Colors.SpringGreen);

			SetGeometryColor(eColorType.eGeometryBorderColor, Colors.Gray);
			SetGeometryColor(eColorType.eGeometryTextColor, Colors.Black);

			SetGeometryColor(eColorType.eFillRackIndex_00, Colors.YellowGreen);
			SetGeometryColor(eColorType.eFillRackIndex_01, Colors.Violet);
			SetGeometryColor(eColorType.eFillRackIndex_02, Colors.Thistle);
			SetGeometryColor(eColorType.eFillRackIndex_03, Colors.Tomato);
			SetGeometryColor(eColorType.eFillRackIndex_04, Colors.SlateBlue);
			SetGeometryColor(eColorType.eFillRackIndex_05, Colors.SkyBlue);
			SetGeometryColor(eColorType.eFillRackIndex_06, Colors.Salmon);
			SetGeometryColor(eColorType.eFillRackIndex_07, Colors.RosyBrown);
			SetGeometryColor(eColorType.eFillRackIndex_08, Colors.Plum);
			SetGeometryColor(eColorType.eFillRackIndex_09, Colors.Pink);
			SetGeometryColor(eColorType.eFillRackIndex_10, Colors.PaleVioletRed);
			SetGeometryColor(eColorType.eFillRackIndex_11, Colors.PaleGreen);
			SetGeometryColor(eColorType.eFillRackIndex_12, Colors.PaleTurquoise);
			SetGeometryColor(eColorType.eFillRackIndex_13, Colors.PaleGoldenrod);
			SetGeometryColor(eColorType.eFillRackIndex_14, Colors.Olive);
			SetGeometryColor(eColorType.eFillRackIndex_15, Colors.LightCoral);
			SetGeometryColor(eColorType.eFillRackIndex_16, Colors.Chocolate);
			SetGeometryColor(eColorType.eFillRackIndex_17, Colors.LightSteelBlue);
			//
			SetGeometryColor(eColorType.eFillRackDefault, Colors.LightGray);
			//
			SetGeometryColor(eColorType.eRackDotsColor, Colors.Red);
			SetGeometryColor(eColorType.eRackUnderpassSymbolColor, Colors.Red);
			SetGeometryColor(eColorType.eRackTieBeamErrorRectangleColor, Colors.Red);
			//
			SetGeometryColor(eColorType.ePropertySourceRackFill, Colors.DarkSlateGray);
			SetGeometryColor(eColorType.ePropertySourceRackTextColor, Colors.White);

			SetGeometryColor(eColorType.eFillMultiselection, Colors.LightPink);
			SetGeometryColor(eColorType.eFillHighlightGeometry, Colors.Red);

			SetGeometryColor(eColorType.eTieBeamGroupBorderColor, Colors.Indigo);

			SetGeometryColor(eColorType.eGripPointBorderColor, Colors.Black);
			SetGeometryColor(eColorType.eGripPointFillColor, Colors.Blue);
			SetGeometryColor(eColorType.eCreateRackRowGripPointFillColor, Colors.Aqua);
			SetGeometryColor(eColorType.eSelectRackRowGripPointFillColor, Colors.Aqua);
			SetGeometryColor(eColorType.eRacksGroupGripPointFillColor, Colors.Red);
			SetGeometryColor(eColorType.eCreateColumnPatternGripPointFillColor, Colors.Aqua);
			SetGeometryColor(eColorType.eColumnPatternDistanceGripPointFillColor, Colors.Coral);

			SetGeometryColor(eColorType.eRackAdvProp_ColumnFillColor, Colors.DarkBlue);
			SetGeometryColor(eColorType.eRackAdvProp_BottomLineColor, Colors.Black);
			SetGeometryColor(eColorType.eRackAdvProp_LevelShelfColor, Colors.DarkOrange);
			SetGeometryColor(eColorType.eRackAdvProp_DimensionsColor, Colors.DarkBlue);
			SetGeometryColor(eColorType.eRackAdvProp_BracingLinesColor, Colors.Gray);
			SetGeometryColor(eColorType.eRackAdvProp_TextColor, Colors.Black);
			SetGeometryColor(eColorType.eRackAdvProp_PalletBorderColor, Colors.SaddleBrown);
			SetGeometryColor(eColorType.eRackAdvProp_PalletFillColor, Colors.Tan);
			SetGeometryColor(eColorType.eRackAdvProp_PalletRiserBorderColor, Colors.SaddleBrown);
			SetGeometryColor(eColorType.eRackAdvProp_PalletRiserFillColor, Colors.Sienna);
			SetGeometryColor(eColorType.eRackAdvProp_DeckingPlateFillColor, Colors.DarkSlateGray);
			SetGeometryColor(eColorType.eRackColumnGuardDefault, Colors.Red);
			SetGeometryColor(eColorType.eRackRowGuardDefault, Colors.Orange);
		}
	}
}
