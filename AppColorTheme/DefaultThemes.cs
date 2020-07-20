using System.Windows.Media;

namespace AppColorTheme
{
	/// <summary>
	/// Default light application theme
	/// </summary>
	public class DefaultLightTheme : ColorTheme
	{
		public DefaultLightTheme()
		{
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_AppBackground, (Color)ColorConverter.ConvertFromString("#FFFAFAFA"), "Application background", "Application background color. It is also used as text color at tooltips. For example, top toolbar commands tooltip."));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_AppCardBackground, (Color)ColorConverter.ConvertFromString("#FFFFFFFF"), "Cards background", "Application panels and dialogs background color. For example, top panel with commands buttons and properties panel."));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_AppWarningBackground, (Color)ColorConverter.ConvertFromString("#FFFF8C00"), "Warning background", "Warning background color. It is used for warning icon at Rack Advanced Properties picture."));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_MaterialDesignDivider, (Color)ColorConverter.ConvertFromString("#1F000000"), "Divider", "Divider color. It is used for lines which separates content, highlight scrollviewer and list item on mouse over. For example, separate items in MHE configurations list."));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_MaterialDesignToolTipBackground, (Color)ColorConverter.ConvertFromString("#FF757575"), "Tooltip background", "Tooltip background."));

			//AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_PrimaryHueLightBrush, (Color)ColorConverter.ConvertFromString("#FFB0BEC5"), "Primary light color. It is used as toolbar separator color.")); // primary 200
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_PrimaryHueMidBrush, (Color)ColorConverter.ConvertFromString("#FF607D8B"), "Primary color", "Primary color. It is used for toolbar commands buttons, OK CANCEL buttons, checkboxes, textboxes, etc")); // primary 500
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_PrimaryHueMidForegroundBrush, (Color)ColorConverter.ConvertFromString("#DDFFFFFF"), "Primary text color", "Color for text which is placed over primary color. For example, \"Rack Accessories\" button at Rack Advanced Properties tab.")); // Primary500Foreground
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_SecondaryAccentBrush, (Color)ColorConverter.ConvertFromString("#FF0091EA"), "Secondary color", "Secondary accent color. It is used for text headers, highlight command buttons, highlight selected sheet, etc")); // accent 700

			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_MaterialDesignBody, (Color)ColorConverter.ConvertFromString("#DD000000"), "Text color", "Default text color"));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_MaterialDesignSelection, (Color)ColorConverter.ConvertFromString("#FFDEDEDE"), "Scrollbar background", "It is used for scrollbar background."));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_MaterialDesignTextBoxBorder, (Color)ColorConverter.ConvertFromString("#89000000"), "TextBox border", "Text box border(underline) color"));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_MaterialDesignColumnHeader, (Color)ColorConverter.ConvertFromString("#BC000000"), "Column header text", "Column header text color. It is used in RackStatistics and PalletStatistics grids."));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_MaterialDesignFlatButtonClick, (Color)ColorConverter.ConvertFromString("#FFDEDEDE"), "Buttons mouse over", "OK CANCEL buttons background when mouse is over."));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_ValidationErrorBrush, (Color)ColorConverter.ConvertFromString("#FFF44336"), "Error background", "Error popup message background. Properties validation error text foreground."));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_ValidationErrorForeground, (Color)ColorConverter.ConvertFromString("#FFFFFFFF"), "Error text", "Error popup message foreground"));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_DrawingAreaBorder, (Color)ColorConverter.ConvertFromString("#FF000000"), "Drawing Area border", "Drawing area border color"));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_DrawingAreaSnappingLines, (Color)ColorConverter.ConvertFromString("#FFFFA07A"), "DA snapping lines", "Drawing area snapping lines color"));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_DrawingAreaSelectionRectangleFill, (Color)ColorConverter.ConvertFromString("#FF008000"), "DA selection rectangle fill", "Drawing area selection rectangle fill color"));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_DrawingAreaSelectionRectangleBorder, (Color)ColorConverter.ConvertFromString("#FF000000"), "DA selection rectangle border", "Drawing area selection rectangle border color"));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_DrawingAreaSelectedGeometryInfoBrush, (Color)ColorConverter.ConvertFromString("#FF4682B4"), "Selected geometry info", "Selected geometry info graphic color: lines from top left corner to the borders of drawing area, distance values text color, dimensions lines, dimensions text."));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_DrawingAreaNewSizePreviewFillBrush, (Color)ColorConverter.ConvertFromString("#FF008000"), "DA size preview rectangle fill", "Display new drawing size if drawing width or height will be decreased and some geometry will be deleted. Fill color of that displayed new size rectangle."));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_DrawingAreaNewSizePreviewBorderBrush, (Color)ColorConverter.ConvertFromString("#FF000000"), "DA size preview rectangle border", "Display new drawing size if drawing width or height will be decreased and some geometry will be deleted. Border color of that displayed new size rectangle."));
			AddInterfaceColor(new ColorInfo(ColorTheme.SYSNAME_DrawingAreaSheetBackgroundBrush, (Color)ColorConverter.ConvertFromString("#FFE5F4FC"), "DA sheet background", "Sheet background color in the Graphics Area."));
		}
	}
}