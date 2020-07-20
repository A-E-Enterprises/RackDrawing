using DrawingControl;
using System.Windows;
using System.Windows.Media;

namespace RackDrawingApp
{
	/// <summary>
	/// Geometry display settings for export sheet layout with geometry dimensions.
	/// Geometry has some opacity with this settings, because dimensions(length and depth) are displayed
	/// for the gemetry(rack, aisle space etc).
	/// </summary>
	public class DimensionsMode_GeomDisplaySettings : IGeomDisplaySettings
	{
		// Protected constructor - Singleton pattern.
		protected DimensionsMode_GeomDisplaySettings() { }

		#region Properties

		/// <summary>
		/// Geometry fill opacity
		/// </summary>
		public double FillBrushOpacity { get { return 0.3; } }

		//=============================================================================
		public bool DisplayText { get { return true; } }

		public FontWeight TextWeight { get { return FontWeights.SemiBold; } }

		private double m_TextFontSize = 14.0;
		public double TextFontSize
		{
			get { return m_TextFontSize; }
			set
			{
				if (Utils.FGT(value, 0.0) && Utils.FNE(value, m_TextFontSize))
				{
					m_TextFontSize = value;
				}
			}
		}

		#endregion

		#region Methods

		//=============================================================================
		public Color GetTextColor(BaseRectangleGeometry geom) { return Colors.Black; }

		//=============================================================================
		public Color GetFillColor(BaseRectangleGeometry geom)
		{
			// always return fill color
			if (geom != null)
				return geom.FillColor;

			return Colors.White;
		}

		#endregion

		//=============================================================================
		private static DimensionsMode_GeomDisplaySettings m_Instance = null;
		public static IGeomDisplaySettings GetInstance()
		{
			if (m_Instance == null)
				m_Instance = new DimensionsMode_GeomDisplaySettings();

			return m_Instance;
		}
	}
}
