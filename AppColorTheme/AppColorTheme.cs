using System.IO;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Windows.Media;
using AppInterfaces;

namespace AppColorTheme
{
	/// <summary>
	/// Color info - system name, descriprion and color value.
	/// </summary>
	public class ColorInfo : IClonable
	{
		public ColorInfo(string systemName, Color value, string localname, string description)
		{
			m_SystemName = systemName;
			m_LocalName = localname;
			m_Value = value;
			m_Description = description;
		}

		#region Properties

		/// <summary>
		/// System name.
		/// For example - background or BlockFillColor.
		/// </summary>
		private string m_SystemName = string.Empty;
		public string SystemName { get { return m_SystemName; } }

		/// <summary>
		/// Local name.
		/// </summary>
		private string m_LocalName = string.Empty;
		public string LocalName { get { return m_LocalName; } }

		/// <summary>
		/// Color description.
		/// For example - this color is used for geometry borders.
		/// </summary>
		private string m_Description = string.Empty;
		public string Description { get { return m_Description; } }

		/// <summary>
		/// Color value
		/// </summary>
		private Color m_Value = Colors.Black;
		public Color Value
		{
			get { return m_Value; }
			set { m_Value = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Returns deep copy of this instance
		/// </summary>
		/// <returns></returns>
		public IClonable Clone()
		{
			return new ColorInfo(this.SystemName, m_Value, this.LocalName, this.Description);
		}

		#endregion
	}

	/// <summary>
	/// Class which read\write app theme and geometry colors to the TXT file.
	/// </summary>
	public class ColorTheme : IClonable
	{
		#region Constants

		/// <summary>
		/// Header in the TXT file from which interface colors are placed.
		/// </summary>
		public static string HEADER_INTERFACE_COLORS = "HEADER_INTERFACE_COLORS";
		/// <summary>
		/// Header in the TXT file from which geometry colors are placed.
		/// </summary>
		public static string HEADER_GEOMETRY_COLORS = "HEADER_GEOMETRY_COLORS";

		/// <summary>
		/// TXT file delimiter which is used for separate colors info values.
		/// For example:
		/// SystemName;#FF00FF;LocalName;Description
		/// </summary>
		public static char COLORINFO_VALUES_DELIMITER = ';';

		// Application interface colors system names
		public static string SYSNAME_AppBackground = "AppBackground";
		public static string SYSNAME_AppCardBackground = "AppCardBackground";
		public static string SYSNAME_AppWarningBackground = "AppWarningBackground";
		public static string SYSNAME_MaterialDesignDivider = "MaterialDesignDivider";
		public static string SYSNAME_MaterialDesignToolTipBackground = "MaterialDesignToolTipBackground";

		//public static string SYSNAME_PrimaryHueLightBrush = "PrimaryHueLightBrush";
		public static string SYSNAME_PrimaryHueMidBrush = "PrimaryHueMidBrush";
		public static string SYSNAME_PrimaryHueMidForegroundBrush = "PrimaryHueMidForegroundBrush";
		public static string SYSNAME_SecondaryAccentBrush = "SecondaryAccentBrush";

		public static string SYSNAME_MaterialDesignBody = "MaterialDesignBody";
		public static string SYSNAME_MaterialDesignTextBoxBorder = "MaterialDesignTextBoxBorder";
		public static string SYSNAME_MaterialDesignColumnHeader = "MaterialDesignColumnHeader";
		public static string SYSNAME_MaterialDesignSelection = "MaterialDesignSelection";
		public static string SYSNAME_MaterialDesignFlatButtonClick = "MaterialDesignFlatButtonClick";
		public static string SYSNAME_ValidationErrorBrush = "ValidationErrorBrush";
		public static string SYSNAME_ValidationErrorForeground = "ValidationErrorForeground";
		public static string SYSNAME_DrawingAreaBorder = "DrawingAreaBorder";
		public static string SYSNAME_DrawingAreaSnappingLines = "DrawingAreaSnappingLines";
		public static string SYSNAME_DrawingAreaSelectionRectangleFill = "DrawingAreaSelectionRectangleFill";
		public static string SYSNAME_DrawingAreaSelectionRectangleBorder = "DrawingAreaSelectionRectangleBorder";
		public static string SYSNAME_DrawingAreaSelectedGeometryInfoBrush = "DrawingAreaSelectedGeometryInfoBrush";
		public static string SYSNAME_DrawingAreaNewSizePreviewFillBrush = "DrawingAreaNewSizePreviewFillBrush";
		public static string SYSNAME_DrawingAreaNewSizePreviewBorderBrush = "DrawingAreaNewSizePreviewBorderBrush";
		public static string SYSNAME_DrawingAreaSheetBackgroundBrush = "DrawingAreaSheetBackgroundBrush";

		// Resources which depends on another resources
		public static string SYSNAME_MaterialDesignCheckBoxOff = "MaterialDesignCheckBoxOff";


		//
		public static string SYSNAME_FILL_BLOCK = "FillBlock";
		public static string SYSNAME_FILL_AISLESPACE = "FillAisleSpace";
		public static string SYSNAME_FILL_COLUMN = "FillColumn";
		public static string SYSNAME_FILL_SHUTTER = "FillShutter";
		public static string SYSNAME_FILL_TIEBEAM = "FillTieBeam";
		public static string SYSNAME_FILL_TIEBEAM_WITH_ERROR = "FillTieBeamWithError";
		public static string SYSNAME_FILL_WALL = "FillWall";
		public static string SYSNAME_FILL_ROOF = "FillRoof";
		public static string SYSNAME_FILL_FLOOR = "FillFloor";
		public static string SYSNAME_FILL_SHEET_ELEVATION_GEOMETRY = "FillSheetElevationGeometry";
		//
		public static string SYSNAME_GEOMETRY_BORDER_COLOR = "GeometryBorderColor";
		public static string SYSNAME_GEOMETRY_TEXT_COLOR = "GeometryTextColor";
		//
		public static string SYSNAME_FILL_RACK_INDEX_00 = "FillRackIndex_00";
		public static string SYSNAME_FILL_RACK_INDEX_01 = "FillRackIndex_01";
		public static string SYSNAME_FILL_RACK_INDEX_02 = "FillRackIndex_02";
		public static string SYSNAME_FILL_RACK_INDEX_03 = "FillRackIndex_03";
		public static string SYSNAME_FILL_RACK_INDEX_04 = "FillRackIndex_04";
		public static string SYSNAME_FILL_RACK_INDEX_05 = "FillRackIndex_05";
		public static string SYSNAME_FILL_RACK_INDEX_06 = "FillRackIndex_06";
		public static string SYSNAME_FILL_RACK_INDEX_07 = "FillRackIndex_07";
		public static string SYSNAME_FILL_RACK_INDEX_08 = "FillRackIndex_08";
		public static string SYSNAME_FILL_RACK_INDEX_09 = "FillRackIndex_09";
		public static string SYSNAME_FILL_RACK_INDEX_10 = "FillRackIndex_10";
		public static string SYSNAME_FILL_RACK_INDEX_11 = "FillRackIndex_11";
		public static string SYSNAME_FILL_RACK_INDEX_12 = "FillRackIndex_12";
		public static string SYSNAME_FILL_RACK_INDEX_13 = "FillRackIndex_13";
		public static string SYSNAME_FILL_RACK_INDEX_14 = "FillRackIndex_14";
		public static string SYSNAME_FILL_RACK_INDEX_15 = "FillRackIndex_15";
		public static string SYSNAME_FILL_RACK_INDEX_16 = "FillRackIndex_16";
		public static string SYSNAME_FILL_RACK_INDEX_17 = "FillRackIndex_17";
		//
		public static string SYSNAME_FILL_RACK_DEFAULT = "FillRackDefault";
		//
		public static string SYSNAME_RACK_DOTS_COLOR = "RackDotsColor";
		public static string SYSNAME_RACK_UNDERPASS_SYMBOL_COLOR = "RackUnderpassSymbolColor";
		public static string SYSNAME_RACK_TIE_BEAM_ERROR_RECTANGLE_COLOR = "RackTieBeamErrorRectangleColor";
		//
		public static string SYSNAME_PROPERTY_SOURCE_RACK_FILL = "PropertySourceRackFill";
		public static string SYSNAME_PROPERTY_SOURCE_RACK_TEXT_COLOR = "PropertySourceRackTextColor";
		//
		public static string SYSNAME_FILL_MULTISELECTION = "FillMultiselection";
		public static string SYSNAME_FILL_HIGHLIGHT_GEOMETRY = "FillHighlightGeometry";
		//
		public static string SYSNAME_TIE_BEAM_GROUP_BORDER_COLOR = "TieBeamGroupBorderColor";
		//
		public static string SYSNAME_GRIP_POINT_BORDER_COLOR = "GripPointBorderColor";
		public static string SYSNAME_GRIP_POINT_FILL_COLOR = "GripPointFillColor";
		public static string SYSNAME_CREATE_RACK_ROW_GRIP_POINT_FILL_COLOR = "CreateRackRowGripPointFillColor";
		public static string SYSNAME_SELECT_RACK_ROW_GRIP_POINT_FILL_COLOR = "SelectRackRowGripPointFillColor";
		public static string SYSNAME_RACKS_GROUP_GRIP_POINT_FILL_COLOR = "RacksGroupGripPointFillColor";
		public static string SYSNAME_CREATE_COLUMN_PATTERN_GRIP_POINT_FILL_COLOR = "CreateColumnPatternGripPointFillColor";
		public static string SYSNAME_COLUMN_PATTERN_DISTANCE_GRIP_POINT_FILL_COLOR = "ColumnPatternDistanceGripPointFillColor";
		//
		public static string SYSNAME_RACK_ADV_PROPS_COLUMN_FILL_COLOR = "RackAdvPropsColumnFillColor";
		public static string SYSNAME_RACK_ADV_PROPS_BOTTOM_LINE_COLOR = "RackAdvPropsBottomLineColor";
		public static string SYSNAME_RACK_ADV_PROPS_LEVEL_SHELF_COLOR = "RackAdvPropsLevelShelfColor";
		public static string SYSNAME_RACK_ADV_PROPS_DIMENSIONS_COLOR = "RackAdvPropsDimensionsColor";
		public static string SYSNAME_RACK_ADV_PROPS_BRACING_LINES_COLOR = "RackAdvPropsBracingLinesColor";
		public static string SYSNAME_RACK_ADV_PROPS_TEXT_COLOR = "RackAdvPropsTextColor";
		public static string SYSNAME_RACK_ADV_PALLET_BORDER_COLOR = "RackAdvPropsPalletBorderColor";
		public static string SYSNAME_RACK_ADV_PALLET_FILL_COLOR = "RackAdvPropsPalletFillColor";
		public static string SYSNAME_RACK_ADV_PALLET_RISER_BORDER_COLOR = "RackAdvPropsPalletRiserBorderColor";
		public static string SYSNAME_RACK_ADV_PALLET_RISER_FILL_COLOR = "RackAdvPropsPalletRiserFillColor";
		public static string SYSNAME_RACK_ADV_DECKING_PLATE_FILL_COLOR = "RackAdvPropsDeckingPlateFillColor";

		#endregion

		#region Properties

		/// <summary>
		/// Dictionary with application user interface colors.
		/// Background, text color, icons color, etc.
		/// 
		/// Key - color system name
		/// Value - color info
		/// </summary>
		private Dictionary<string, ColorInfo> m_AppInterfaceColorsDictionary = new Dictionary<string, ColorInfo>();
		public List<ColorInfo> InterfaceColorsList { get { return m_AppInterfaceColorsDictionary.Values.ToList(); } }

		/// <summary>
		/// Geometry colors theme.
		/// </summary>
		private IGeometryColorsTheme m_GeometryColorsTheme = new DefaultGeometryColorsTheme();
		public IGeometryColorsTheme GeometryColorsTheme
		{
			get { return m_GeometryColorsTheme; }
			set { m_GeometryColorsTheme = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Applies interface colors to the application
		/// </summary>
		public bool ApplyTheme(System.Windows.ResourceDictionary resDictionary)
		{
			if (resDictionary == null)
				return false;

			if (m_AppInterfaceColorsDictionary == null)
				return false;

			foreach(ColorInfo colorInfo in m_AppInterfaceColorsDictionary.Values)
			{
				if (colorInfo == null || string.IsNullOrEmpty(colorInfo.SystemName))
					continue;

				SolidColorBrush brush = new SolidColorBrush(colorInfo.Value);
				resDictionary[colorInfo.SystemName] = brush;

				if (SYSNAME_MaterialDesignTextBoxBorder == colorInfo.SystemName)
					resDictionary[SYSNAME_MaterialDesignCheckBoxOff] = brush;
			}

			return true;
		}

		/// <summary>
		/// Add color info to m_AppInterfaceColorsDictionary
		/// </summary>
		protected bool AddInterfaceColor(ColorInfo colorInfo)
		{
			if (colorInfo == null || string.IsNullOrEmpty(colorInfo.SystemName))
				return false;

			m_AppInterfaceColorsDictionary[colorInfo.SystemName] = colorInfo;
			return true;
		}

		/// <summary>
		/// Add color info to m_GeometryColorsDictionary
		/// </summary>
		protected bool AddGeometryColor(eColorType colorType, Color value)
		{
			if (m_GeometryColorsTheme == null)
				return false;

			m_GeometryColorsTheme.SetGeometryColor(colorType, value);
			return true;
		}

		/// <summary>
		/// Write color theme to file
		/// </summary>
		public bool WriteToFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return false;

			using (StreamWriter fs = new StreamWriter(filePath, false))
			{
				// STEP 1. Write interface colors
				fs.WriteLine(HEADER_INTERFACE_COLORS);
				foreach(ColorInfo interfaceColorInfo in m_AppInterfaceColorsDictionary.Values)
				{
					if (interfaceColorInfo == null || string.IsNullOrEmpty(interfaceColorInfo.SystemName))
						continue;

					string localName = interfaceColorInfo.LocalName;
					if (string.IsNullOrEmpty(localName))
						localName = interfaceColorInfo.SystemName;

					StringBuilder sb = new StringBuilder();
					sb.Append(interfaceColorInfo.SystemName);
					sb.Append(COLORINFO_VALUES_DELIMITER);
					sb.Append(interfaceColorInfo.Value.ToString());
					sb.Append(COLORINFO_VALUES_DELIMITER);
					sb.Append(localName);
					sb.Append(COLORINFO_VALUES_DELIMITER);
					sb.Append(interfaceColorInfo.Description);
					sb.Append(COLORINFO_VALUES_DELIMITER);

					fs.WriteLine(sb.ToString());
				}
				fs.WriteLine();

				// STEP 2. Write geometry colors
				fs.WriteLine(HEADER_GEOMETRY_COLORS);
				if (m_GeometryColorsTheme != null)
				{
					foreach (eColorType colorType in (eColorType[])Enum.GetValues(typeof(eColorType)))
					{
						if (eColorType.eUndefined == colorType)
							continue;

						Color colorValue;
						if (!m_GeometryColorsTheme.GetGeometryColor(colorType, out colorValue))
							continue;

						string sysName = ColorTheme.ColorTypeToSystemName(colorType);
						string locName = ColorTheme.ColorTypeToLocalName(colorType);
						string description = ColorTheme.ColorTypeToDescription(colorType);

						if (string.IsNullOrEmpty(sysName))
							continue;

						StringBuilder sb = new StringBuilder();
						sb.Append(sysName);
						sb.Append(COLORINFO_VALUES_DELIMITER);
						sb.Append(colorValue.ToString());
						sb.Append(COLORINFO_VALUES_DELIMITER);
						sb.Append(locName);
						sb.Append(COLORINFO_VALUES_DELIMITER);
						sb.Append(description);
						sb.Append(COLORINFO_VALUES_DELIMITER);

						fs.WriteLine(sb.ToString());
					}
				}
				fs.WriteLine();

				fs.Close();
			}

			return true;
		}

		/// <summary>
		/// Read colors from file.
		/// </summary>
		public bool ReadFromFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return false;

			using (StreamReader sr = new StreamReader(filePath))
			{
				ReadFromStream(sr);
				sr.Close();
			}

			return true;
		}
		/// <summary>
		/// Read colors from stream
		/// </summary>
		public bool ReadFromStream(StreamReader sr)
		{
			if (sr == null)
				return false;

			bool bInterfaceColor = false;
			bool bGeometryColor = false;

			string line;
			while ((line = sr.ReadLine()) != null)
			{
				if (HEADER_INTERFACE_COLORS == line)
				{
					bInterfaceColor = true;
					bGeometryColor = false;
				}
				else if (HEADER_GEOMETRY_COLORS == line)
				{
					bInterfaceColor = false;
					bGeometryColor = true;
				}
				else
				{
					string[] arr = line.Split(COLORINFO_VALUES_DELIMITER);
					if (arr.Count() >= 2)
					{
						string sysname = arr[0];
						string value = arr[1];
						string localname = string.Empty;
						if (arr.Count() >= 3)
							localname = arr[2];
						string description = string.Empty;
						if (arr.Count() >= 4)
							description = arr[3];

						if (string.IsNullOrEmpty(sysname))
							continue;

						Color colorValue = Colors.Black;
						try
						{
							colorValue = (Color)ColorConverter.ConvertFromString(value);
						}
						catch
						{
							continue;
						}

						if (bInterfaceColor)
						{
							ColorInfo colorInfo = new ColorInfo(sysname, colorValue, localname, description);
							AddInterfaceColor(colorInfo);
						}
						else if (bGeometryColor)
						{
							eColorType colorType = ColorTheme.SystemNameToColorType(sysname);
							AddGeometryColor(colorType, colorValue);
							//// replace local name and descriptions
							//if (m_ColorTypeToDescriptionDict != null && m_ColorTypeToDescriptionDict.ContainsKey(colorType))
							//	m_ColorTypeToDescriptionDict[colorType] = description;
							//if (m_ColorTypeToLocalNameDict != null && m_ColorTypeToLocalNameDict.ContainsKey(colorType))
							//	m_ColorTypeToLocalNameDict[colorType] = localname;
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Returns deep copy of this instance
		/// </summary>
		/// <returns></returns>
		public IClonable Clone()
		{
			ColorTheme thisClone = new ColorTheme();

			foreach(ColorInfo colorInfo in m_AppInterfaceColorsDictionary.Values)
			{
				if (colorInfo == null || string.IsNullOrEmpty(colorInfo.SystemName))
					continue;

				ColorInfo colorInfoClone = colorInfo.Clone() as ColorInfo;
				if (colorInfoClone == null || string.IsNullOrEmpty(colorInfoClone.SystemName))
					continue;

				thisClone.AddInterfaceColor(colorInfoClone);
			}

			if(m_GeometryColorsTheme != null)
				thisClone.GeometryColorsTheme = m_GeometryColorsTheme.Clone() as IGeometryColorsTheme;

			return thisClone;
		}

		#endregion

		/// <summary>
		/// Read ColorTheme from stream
		/// </summary>
		public static ColorTheme ReadFromStream(Stream stream)
		{
			if (stream == null)
				return null;

			using (StreamReader reader = new StreamReader(stream))
			{
				if (reader != null)
				{
					ColorTheme colorTheme = new DefaultLightTheme();
					if (!colorTheme.ReadFromStream(reader))
						return null;

					return colorTheme;
				}
			}

			return null;
		}

		/// <summary>
		/// If "false" then need to initialize static dictionaries for eColorType to system name, description conversion.
		/// </summary>
		private static bool m_AreDictInitialized = false;
		/// <summary>
		/// Key - eColorType
		/// Value - color SystemName
		/// </summary>
		private static Dictionary<eColorType, string> m_ColorTypeToSystemNameDict = new Dictionary<eColorType, string>();
		/// <summary>
		/// Key - color SystemName
		/// Value - eColorType
		/// </summary>
		private static Dictionary<string, eColorType> m_SystemNameToColorTypeDict = new Dictionary<string, eColorType>();
		/// <summary>
		/// Key - eColorType
		/// Value - color local name
		/// </summary>
		private static Dictionary<eColorType, string> m_ColorTypeToLocalNameDict = new Dictionary<eColorType, string>();
		/// <summary>
		/// Key - eColorType
		/// Value - color Description
		/// </summary>
		private static Dictionary<eColorType, string> m_ColorTypeToDescriptionDict = new Dictionary<eColorType, string>();

		private static void InitializeDict()
		{
			if (m_AreDictInitialized)
				return;

			m_ColorTypeToSystemNameDict.Add(eColorType.eFill_Block, SYSNAME_FILL_BLOCK);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFill_AisleSpace, SYSNAME_FILL_AISLESPACE);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFill_Column, SYSNAME_FILL_COLUMN);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFill_Shutter, SYSNAME_FILL_SHUTTER);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFill_TieBeam, SYSNAME_FILL_TIEBEAM);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFill_TieBeamWithError, SYSNAME_FILL_TIEBEAM_WITH_ERROR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFill_Wall, SYSNAME_FILL_WALL);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFill_Roof, SYSNAME_FILL_ROOF);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFill_Floor, SYSNAME_FILL_FLOOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFill_SheetElevations, SYSNAME_FILL_SHEET_ELEVATION_GEOMETRY);
			//
			m_ColorTypeToSystemNameDict.Add(eColorType.eGeometryBorderColor, SYSNAME_GEOMETRY_BORDER_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eGeometryTextColor, SYSNAME_GEOMETRY_TEXT_COLOR);
			//
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_00, SYSNAME_FILL_RACK_INDEX_00);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_01, SYSNAME_FILL_RACK_INDEX_01);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_02, SYSNAME_FILL_RACK_INDEX_02);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_03, SYSNAME_FILL_RACK_INDEX_03);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_04, SYSNAME_FILL_RACK_INDEX_04);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_05, SYSNAME_FILL_RACK_INDEX_05);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_06, SYSNAME_FILL_RACK_INDEX_06);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_07, SYSNAME_FILL_RACK_INDEX_07);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_08, SYSNAME_FILL_RACK_INDEX_08);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_09, SYSNAME_FILL_RACK_INDEX_09);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_10, SYSNAME_FILL_RACK_INDEX_10);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_11, SYSNAME_FILL_RACK_INDEX_11);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_12, SYSNAME_FILL_RACK_INDEX_12);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_13, SYSNAME_FILL_RACK_INDEX_13);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_14, SYSNAME_FILL_RACK_INDEX_14);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_15, SYSNAME_FILL_RACK_INDEX_15);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_16, SYSNAME_FILL_RACK_INDEX_16);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackIndex_17, SYSNAME_FILL_RACK_INDEX_17);
			//
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillRackDefault, SYSNAME_FILL_RACK_DEFAULT);
			//
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackDotsColor, SYSNAME_RACK_DOTS_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackUnderpassSymbolColor, SYSNAME_RACK_UNDERPASS_SYMBOL_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackTieBeamErrorRectangleColor, SYSNAME_RACK_TIE_BEAM_ERROR_RECTANGLE_COLOR);
			//
			m_ColorTypeToSystemNameDict.Add(eColorType.ePropertySourceRackFill, SYSNAME_PROPERTY_SOURCE_RACK_FILL);
			m_ColorTypeToSystemNameDict.Add(eColorType.ePropertySourceRackTextColor, SYSNAME_PROPERTY_SOURCE_RACK_TEXT_COLOR);
			//
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillMultiselection, SYSNAME_FILL_MULTISELECTION);
			m_ColorTypeToSystemNameDict.Add(eColorType.eFillHighlightGeometry, SYSNAME_FILL_HIGHLIGHT_GEOMETRY);
			//
			m_ColorTypeToSystemNameDict.Add(eColorType.eTieBeamGroupBorderColor, SYSNAME_TIE_BEAM_GROUP_BORDER_COLOR);
			//
			m_ColorTypeToSystemNameDict.Add(eColorType.eGripPointBorderColor, SYSNAME_GRIP_POINT_BORDER_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eGripPointFillColor, SYSNAME_GRIP_POINT_FILL_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eCreateRackRowGripPointFillColor, SYSNAME_CREATE_RACK_ROW_GRIP_POINT_FILL_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eSelectRackRowGripPointFillColor, SYSNAME_SELECT_RACK_ROW_GRIP_POINT_FILL_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRacksGroupGripPointFillColor, SYSNAME_RACKS_GROUP_GRIP_POINT_FILL_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eCreateColumnPatternGripPointFillColor, SYSNAME_CREATE_COLUMN_PATTERN_GRIP_POINT_FILL_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eColumnPatternDistanceGripPointFillColor, SYSNAME_COLUMN_PATTERN_DISTANCE_GRIP_POINT_FILL_COLOR);
			//
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackAdvProp_ColumnFillColor, SYSNAME_RACK_ADV_PROPS_COLUMN_FILL_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackAdvProp_BottomLineColor, SYSNAME_RACK_ADV_PROPS_BOTTOM_LINE_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackAdvProp_LevelShelfColor, SYSNAME_RACK_ADV_PROPS_LEVEL_SHELF_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackAdvProp_DimensionsColor, SYSNAME_RACK_ADV_PROPS_DIMENSIONS_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackAdvProp_BracingLinesColor, SYSNAME_RACK_ADV_PROPS_BRACING_LINES_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackAdvProp_TextColor, SYSNAME_RACK_ADV_PROPS_TEXT_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackAdvProp_PalletBorderColor, SYSNAME_RACK_ADV_PALLET_BORDER_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackAdvProp_PalletFillColor, SYSNAME_RACK_ADV_PALLET_FILL_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackAdvProp_PalletRiserBorderColor, SYSNAME_RACK_ADV_PALLET_RISER_BORDER_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackAdvProp_PalletRiserFillColor, SYSNAME_RACK_ADV_PALLET_RISER_FILL_COLOR);
			m_ColorTypeToSystemNameDict.Add(eColorType.eRackAdvProp_DeckingPlateFillColor, SYSNAME_RACK_ADV_DECKING_PLATE_FILL_COLOR);

			//
			foreach (eColorType colorType in m_ColorTypeToSystemNameDict.Keys)
				m_SystemNameToColorTypeDict.Add(m_ColorTypeToSystemNameDict[colorType], colorType);

			//
			// LOCAL NAME
			//
			m_ColorTypeToLocalNameDict.Add(eColorType.eFill_Block, "Block");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFill_AisleSpace, "AisleSpace");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFill_Column, "Column");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFill_Shutter, "Shutter");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFill_TieBeam, "TieBeam");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFill_TieBeamWithError, "TieBeam error");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFill_Wall, "Wall");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFill_Roof, "Roof");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFill_Floor, "Floor");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFill_SheetElevations, "SheetElevationGeometry");
			//
			m_ColorTypeToLocalNameDict.Add(eColorType.eGeometryBorderColor, "Geometry border");
			m_ColorTypeToLocalNameDict.Add(eColorType.eGeometryTextColor, "Geometry text color");
			//
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_00, "Rack index 00");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_01, "Rack index 01");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_02, "Rack index 02");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_03, "Rack index 03");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_04, "Rack index 04");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_05, "Rack index 05");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_06, "Rack index 06");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_07, "Rack index 07");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_08, "Rack index 08");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_09, "Rack index 09");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_10, "Rack index 10");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_11, "Rack index 11");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_12, "Rack index 12");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_13, "Rack index 13");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_14, "Rack index 14");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_15, "Rack index 15");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_16, "Rack index 16");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackIndex_17, "Rack index 17");
			//
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillRackDefault, "Rack default fill");
			//
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackDotsColor, "Rack dots");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackUnderpassSymbolColor, "Rack underpass symbol");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackTieBeamErrorRectangleColor, "Rack tie beam error");
			//
			m_ColorTypeToLocalNameDict.Add(eColorType.ePropertySourceRackFill, "Property source rack");
			m_ColorTypeToLocalNameDict.Add(eColorType.ePropertySourceRackTextColor, "Property source rack text");
			//
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillMultiselection, "Multiselection geometry");
			m_ColorTypeToLocalNameDict.Add(eColorType.eFillHighlightGeometry, "Highlight geometry");
			//
			m_ColorTypeToLocalNameDict.Add(eColorType.eTieBeamGroupBorderColor, "Tie Beam group border");
			//
			m_ColorTypeToLocalNameDict.Add(eColorType.eGripPointBorderColor, "Grip point border");
			m_ColorTypeToLocalNameDict.Add(eColorType.eGripPointFillColor, "Grip point fill");
			m_ColorTypeToLocalNameDict.Add(eColorType.eCreateRackRowGripPointFillColor, "Create rack row grip point");
			m_ColorTypeToLocalNameDict.Add(eColorType.eSelectRackRowGripPointFillColor, "Select entire rack row grip point");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRacksGroupGripPointFillColor, "Move or stretch entire racks group grip");
			m_ColorTypeToLocalNameDict.Add(eColorType.eCreateColumnPatternGripPointFillColor, "Create column pattern grip point");
			m_ColorTypeToLocalNameDict.Add(eColorType.eColumnPatternDistanceGripPointFillColor, "Column pattern distance grip point");
			//
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackAdvProp_ColumnFillColor, "Rack Adv Props column");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackAdvProp_BottomLineColor, "Rack Adv Props bottom line");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackAdvProp_LevelShelfColor, "Rack Adv Props level shelf");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackAdvProp_DimensionsColor, "Rack Adv Props dimensions");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackAdvProp_BracingLinesColor, "Rack Adv Props bracing lines");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackAdvProp_TextColor, "Rack Adv Props text");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackAdvProp_PalletBorderColor, "Rack Adv Props pallet border");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackAdvProp_PalletFillColor, "Rack Adv Props pallet fill");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackAdvProp_PalletRiserBorderColor, "Rack Adv Props pallet riser border");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackAdvProp_PalletRiserFillColor, "Rack Adv Props pallet riser fill");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackAdvProp_DeckingPlateFillColor, "Rack Adv Props decking plate fill");

			m_ColorTypeToLocalNameDict.Add(eColorType.eRackRowGuardDefault, "Rack Row Guard");
			m_ColorTypeToLocalNameDict.Add(eColorType.eRackColumnGuardDefault, "Rack Column Guard");

			//
			// DESCRIPTION
			//
			m_ColorTypeToDescriptionDict.Add(eColorType.eFill_Block, "Block fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFill_AisleSpace, "AisleSpace fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFill_Column, "Column fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFill_Shutter, "Shutter fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFill_TieBeam, "TieBeam fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFill_TieBeamWithError, "TieBeam with error fill color. Probably height of the rack is greater than the max rack height.");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFill_Wall, "Wall fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFill_Roof, "Roof fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFill_Floor, "Floor fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFill_SheetElevations, "SheetElevationGeometry fill color");
			//
			m_ColorTypeToDescriptionDict.Add(eColorType.eGeometryBorderColor, "Geometry border color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eGeometryTextColor, "Geometry text color, it also drives racks statistic text color");
			//
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_00, "00 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_01, "01 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_02, "02 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_03, "03 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_04, "04 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_05, "05 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_06, "06 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_07, "07 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_08, "08 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_09, "09 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_10, "10 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_11, "11 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_12, "12 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_13, "13 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_14, "14 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_15, "15 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_16, "16 index rack fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackIndex_17, "17 index rack fill color");
			//
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillRackDefault, "Rack fill color if rack index is greater than 17 or new not placed rack");
			//
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackDotsColor, "Rack small dots which are displayed at the middle of the length side of the rack");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackUnderpassSymbolColor, "Rack underpass symbol color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackTieBeamErrorRectangleColor, "Color of rectange, which is displayed over rack if it has tie beam error");
			//
			m_ColorTypeToDescriptionDict.Add(eColorType.ePropertySourceRackFill, "Fill color for the property source rack(it used in the rack match properties command)");
			m_ColorTypeToDescriptionDict.Add(eColorType.ePropertySourceRackTextColor, "Text color for the property source rack(it used in the rack match properties command)");
			//
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillMultiselection, "Fill color for geometry in multiselection");
			m_ColorTypeToDescriptionDict.Add(eColorType.eFillHighlightGeometry, "Fill color for highlight incorrect geometry and ask user to delete it. It is used on document check.");
			//
			m_ColorTypeToDescriptionDict.Add(eColorType.eTieBeamGroupBorderColor, "Border which displays tie beam grouping. Available only in the DEBUG mode.");
			//
			m_ColorTypeToDescriptionDict.Add(eColorType.eGripPointBorderColor, "Grip point border color. It is displayed when geometry is selected. This color is applied to all grip points border.");
			m_ColorTypeToDescriptionDict.Add(eColorType.eGripPointFillColor, "Grip point fill color. It is applied to change rectangle size and rotate grips.");
			m_ColorTypeToDescriptionDict.Add(eColorType.eCreateRackRowGripPointFillColor, "Fill color of create rack row grip point");
			m_ColorTypeToDescriptionDict.Add(eColorType.eSelectRackRowGripPointFillColor, "Select entire rack row grip point fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRacksGroupGripPointFillColor, "Move or stretch entire racks group grip point fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eCreateColumnPatternGripPointFillColor, "Create column pattern grip point fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eColumnPatternDistanceGripPointFillColor, "Fill color of grip point which changes distance between columns in the pattern");
			//
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackAdvProp_ColumnFillColor, "Rack advanced properties column fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackAdvProp_BottomLineColor, "Rack advanced properties bottom line color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackAdvProp_LevelShelfColor, "Rack advanced properties level shelf color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackAdvProp_DimensionsColor, "Rack advanced properties dimensions color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackAdvProp_BracingLinesColor, "Rack advanced properties bracing lines color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackAdvProp_TextColor, "Rack advanced properties views names and levels names text color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackAdvProp_PalletBorderColor, "Rack advanced properties pallet border color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackAdvProp_PalletFillColor, "Rack advanced properties pallet fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackAdvProp_PalletRiserBorderColor, "Rack advanced properties pallet riser border color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackAdvProp_PalletRiserFillColor, "Rack advanced properties pallet riser fill color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackAdvProp_DeckingPlateFillColor, "Rack advanced properties decking plate fill color");

			m_ColorTypeToDescriptionDict.Add(eColorType.eRackRowGuardDefault, "Rack Row Guard Draw Color");
			m_ColorTypeToDescriptionDict.Add(eColorType.eRackColumnGuardDefault, "Rack Column Draw Color");

			m_AreDictInitialized = true;
		}

		/// <summary>
		/// Converts eColorType to system name
		/// </summary>
		public static string ColorTypeToSystemName(eColorType colorType)
		{
			InitializeDict();

			if (m_ColorTypeToSystemNameDict == null || !m_ColorTypeToSystemNameDict.ContainsKey(colorType))
				return string.Empty;

			return m_ColorTypeToSystemNameDict[colorType];
		}

		/// <summary>
		/// Converts color system name to color type
		/// </summary>
		public static eColorType SystemNameToColorType(string systemName)
		{
			InitializeDict();

			if (string.IsNullOrEmpty(systemName) || !m_SystemNameToColorTypeDict.ContainsKey(systemName))
				return eColorType.eUndefined;

			return m_SystemNameToColorTypeDict[systemName];
		}

		/// <summary>
		/// Converts eColorType to local name
		/// </summary>
		public static string ColorTypeToLocalName(eColorType colorType)
		{
			InitializeDict();

			if (m_ColorTypeToLocalNameDict == null || !m_ColorTypeToLocalNameDict.ContainsKey(colorType))
				return string.Empty;

			return m_ColorTypeToLocalNameDict[colorType];
		}

		/// <summary>
		/// Converts eColorType to description
		/// </summary>
		public static string ColorTypeToDescription(eColorType colorType)
		{
			InitializeDict();

			if (m_ColorTypeToDescriptionDict == null || !m_ColorTypeToDescriptionDict.ContainsKey(colorType))
				return string.Empty;

			return m_ColorTypeToDescriptionDict[colorType];
		}
	}
}