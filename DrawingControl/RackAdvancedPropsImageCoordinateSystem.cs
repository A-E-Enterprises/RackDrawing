using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DrawingControl
{
	public class RackAdvancedPropsImageCoordinateSystem : ICoordinateSystem
	{
		public RackAdvancedPropsImageCoordinateSystem(double imageLengthInPixels, double imageHeightInPixels, Size drawingGlobalSize)
		{
			m_ImageLengthInPixels = imageLengthInPixels;
			m_ImageHeightInPixels = imageHeightInPixels;
			m_DrawingGlobalSize = drawingGlobalSize;

			m_Scale = Utils.CalcScale(m_ImageLengthInPixels, m_ImageHeightInPixels, m_DrawingGlobalSize.Width, m_DrawingGlobalSize.Height);
			// Dont center align here, it is aligned outside, by applying transform to DrawingContext.
			//m_ZeroPointInPixels.X = (m_ImageLengthInPixels - m_DrawingGlobalSize.Width * m_Scale) / 2;
			//m_ZeroPointInPixels.Y = (m_ImageHeightInPixels - m_DrawingGlobalSize.Height * m_Scale) / 2;
		}

		#region Properties

		//=============================================================================
		/// <summary>
		/// Total image(picture) length in pixels
		/// </summary>
		private double m_ImageLengthInPixels = 1;
		//=============================================================================
		/// <summary>
		/// Total image(picture) height in pixels
		/// </summary>
		private double m_ImageHeightInPixels = 1;
		//=============================================================================
		/// <summary>
		/// Size of drawing which will be displayed at picture.
		/// It is measured in global units, not image pixels.
		/// </summary>
		private Size m_DrawingGlobalSize = new Size(0.0, 0.0);
		//=============================================================================
		/// <summary>
		/// Scale applied to global units for convert them in image units.
		/// </summary>
		private double m_Scale = 1.0;

		#endregion

		//=============================================================================
		/// <summary>
		/// Convert height value in global drawing coordinates to height value in picture pixels.
		/// </summary>
		public double GetHeightInPixels(double globalHeightValue, double UnitsPerCameraPixel)
		{
			return m_Scale * globalHeightValue;
		}
		//=============================================================================
		public double GetGlobalHeight(double heightInPixels, double UnitsPerCameraPixel)
		{
			return heightInPixels / m_Scale;
		}

		//=============================================================================
		/// <summary>
		/// Convert width value in global drawing coordinates to width value in picture pixels.
		/// </summary>
		public double GetWidthInPixels(double globalWidthValue, double UnitsPerCameraPixel)
		{
			return m_Scale * globalWidthValue;
		}
		//=============================================================================
		public double GetGlobalWidth(double widthInPixels, double UnitsPerCameraPixel)
		{
			return widthInPixels / m_Scale;
		}

		//=============================================================================
		public Point GetLocalPoint(Point globalPoint, double UnitsPerCameraPixel, Vector cameraOffset)
		{
			// UnitsPerCameraPixel and cameraOffset doesnt affect on calculation
			Point localPoint = new Point(0.0, 0.0);
			localPoint.X += m_Scale * globalPoint.X;
			localPoint.Y += m_Scale * globalPoint.Y;

			return localPoint;
		}
	}
}
