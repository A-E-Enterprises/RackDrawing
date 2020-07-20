using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public interface IGeomDisplaySettings
	{
		// Opacity of the fill geometry brush
		double FillBrushOpacity { get; }

		// If true than rectangle name will be displayed.
		bool DisplayText { get; }

		// Weight of the geometry text font
		FontWeight TextWeight { get; }

		// Font size of any geometry text.
		double TextFontSize { get; set; }

		// color for the text inside geometry rectangle
		Color GetTextColor(BaseRectangleGeometry geom);

		// Returns fill color for geometry.
		// Implementation of this interface can override this method for return
		// different colors if geometry is selected, highlighted, incorrect or something else.
		Color GetFillColor(BaseRectangleGeometry geom);
	}
}
