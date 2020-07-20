using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public class DefaultGeomDisplaySettings : IGeomDisplaySettings
	{
		protected DefaultGeomDisplaySettings() { }

		//=============================================================================
		public double FillBrushOpacity { get { return 1.0; } }

		//=============================================================================
		private bool m_DisplayText = true;
		public bool DisplayText
		{
			get { return m_DisplayText; }
			set
			{
				if (m_DisplayText != value)
					m_DisplayText = value;
			}
		}


		//=============================================================================
		public FontWeight TextWeight { get { return FontWeights.Normal; } }

		//=============================================================================
		private double m_TextFontSize = 14.0;
		public double TextFontSize
		{
			get { return m_TextFontSize; }
			set
			{
				if(Utils.FGT(value, 0.0) && Utils.FNE(value, m_TextFontSize))
				{
					m_TextFontSize = value;
				}
			}
		}

		//=============================================================================
		public Color GetTextColor(BaseRectangleGeometry geom)
		{
			return Colors.Black;
		}

		//=============================================================================
		public Color GetFillColor(BaseRectangleGeometry geom)
		{
			// always return fill color
			if (geom != null)
				return geom.FillColor;

			return Colors.White;
		}

		//=============================================================================
		public static IGeomDisplaySettings GetInstance()
		{
			return new DefaultGeomDisplaySettings();
		}
	}
}
