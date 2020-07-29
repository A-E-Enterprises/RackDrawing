using AppInterfaces;
using System.Windows.Media;

namespace AppColorTheme
{
	public enum eColorType : int
	{
		eUndefined = 0,
		eFill_Block = 1,
		eFill_AisleSpace,
		eFill_Column,
		eFill_Shutter,
		eFill_TieBeam,
		eFill_SheetElevations,
		/// <summary>
		/// Tie beam with error, probably height of the rack is greater than the max rack height.
		/// </summary>
		eFill_TieBeamWithError,
		eFill_Wall,
		eFill_Roof,
		eFill_Floor,
		
		/// <summary>
		/// BaseRectangleGeometry border color
		/// </summary>
		eGeometryBorderColor,

		/// <summary>
		/// All geometry text color
		/// </summary>
		eGeometryTextColor,

		/// <summary>
		/// Rack fill color, which depends on rack index
		/// </summary>
		eFillRackIndex_00,
		eFillRackIndex_01,
		eFillRackIndex_02,
		eFillRackIndex_03,
		eFillRackIndex_04,
		eFillRackIndex_05,
		eFillRackIndex_06,
		eFillRackIndex_07,
		eFillRackIndex_08,
		eFillRackIndex_09,
		eFillRackIndex_10,
		eFillRackIndex_11,
		eFillRackIndex_12,
		eFillRackIndex_13,
		eFillRackIndex_14,
		eFillRackIndex_15,
		eFillRackIndex_16,
		eFillRackIndex_17,
		/// <summary>
		/// Rack fill color, if rack index is greater than 17 or new not placed rack
		/// </summary>
		eFillRackDefault,
		/// <summary>
		/// Rack small dots which are displayed at the middle of the length side of the rack.
		/// </summary>
		eRackDotsColor,
		/// <summary>
		/// Rack underpass symbol color.
		/// </summary>
		eRackUnderpassSymbolColor,
		/// <summary>
		/// Color of rectange, which is displayed over rack if it has tie beam error.
		/// </summary>
		eRackTieBeamErrorRectangleColor,

		/// <summary>
		/// fill color for the property source rack(it used in the rack match properties command)
		/// </summary>
		ePropertySourceRackFill,
		/// <summary>
		/// color for text inside property source rack geometry
		/// </summary>
		ePropertySourceRackTextColor,

		/// <summary>
		/// Fill color for multiselected geometry
		/// </summary>
		eFillMultiselection,
		/// <summary>
		/// Fill color for highlighted geometry
		/// </summary>
		eFillHighlightGeometry,

		/// <summary>
		/// Border which displays tie beam grouping. Available only in the DEBUG mode.
		/// </summary>
		eTieBeamGroupBorderColor,

		/// <summary>
		/// Grip point border color. It is displayed when geometry is selected.
		/// </summary>
		eGripPointBorderColor,
		/// <summary>
		/// Grip point fill color. It is applied to change rectangle size and rotate grips.
		/// </summary>
		eGripPointFillColor,
		/// <summary>
		/// Fill color of create rack row grip point
		/// </summary>
		eCreateRackRowGripPointFillColor,
		/// <summary>
		/// Select entire rack row grip point fill color
		/// </summary>
		eSelectRackRowGripPointFillColor,
		/// <summary>
		/// Move or stretch entire racks group grip point fill color
		/// </summary>
		eRacksGroupGripPointFillColor,
		/// <summary>
		/// Create column pattern grip point fill color
		/// </summary>
		eCreateColumnPatternGripPointFillColor,
		/// <summary>
		/// Fill color of grip point which changes distance between columns in the pattern
		/// </summary>
		eColumnPatternDistanceGripPointFillColor,

		/// <summary>
		/// Rack advanced properties column fill color
		/// </summary>
		eRackAdvProp_ColumnFillColor,
		/// <summary>
		/// Rack advanced properties bottom line color
		/// </summary>
		eRackAdvProp_BottomLineColor,
		/// <summary>
		/// Rack advanced properties level shelf color
		/// </summary>
		eRackAdvProp_LevelShelfColor,
		/// <summary>
		/// Rack advanced properties dimensions color
		/// </summary>
		eRackAdvProp_DimensionsColor,
		/// <summary>
		/// Rack advanced properties bracing lines color
		/// </summary>
		eRackAdvProp_BracingLinesColor,
		/// <summary>
		/// Rack advanced properties views names and levels names text color
		/// </summary>
		eRackAdvProp_TextColor,
		/// <summary>
		/// Rack advanced properties pallet border color
		/// </summary>
		eRackAdvProp_PalletBorderColor,
		/// <summary>
		/// Rack advanced properties pallet fill color
		/// </summary>
		eRackAdvProp_PalletFillColor,
		/// <summary>
		/// Rack advanced properties pallet riser border color
		/// </summary>
		eRackAdvProp_PalletRiserBorderColor,
		/// <summary>
		/// Rack advanced properties pallet riser fill color
		/// </summary>
		eRackAdvProp_PalletRiserFillColor,
		/// <summary>
		/// Rack advanced properties decking plate fill color
		/// </summary>
		eRackAdvProp_DeckingPlateFillColor,

		/// <summary>
		/// Rack column guard color
		/// </summary>
		eRackColumnGuardDefault,
		/// <summary>
		/// Rack column guard color
		/// </summary>
		eRackRowGuardDefault,
		/// <summary>
		/// Rack guard visualization main color to draw front and side view
		/// </summary>
		eRackGuardMainColorDefault,
		/// <summary>
		/// Rack guard visualization additional color to draw front and side view
		/// </summary>
		eRackGuardAltColorDefault,

	}

	/// <summary>
	/// Implements geometry colors theme.
	/// Which color should be used for display geometry.
	/// For example - block, rack fill color, grips color, etc.
	/// </summary>
	public interface IGeometryColorsTheme : IClonable
	{
		bool GetGeometryColor(eColorType colorType, out Color colorValue);

		void SetGeometryColor(eColorType colorType, Color colorValue);
	}
}
