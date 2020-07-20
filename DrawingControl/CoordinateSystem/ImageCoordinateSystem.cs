using System.Windows;

namespace DrawingControl
{
	public class ImageCoordinateSystem : ICoordinateSystem
	{
		public ImageCoordinateSystem(int imageLengthInPixels, int imageHeightInPixels, Vector offsetPixels, Size drawingGlobalSize, double minUnitsPerPixel)
		{
			m_ImageLengthInPixels = imageLengthInPixels;
			m_ImageHeightInPixels = imageHeightInPixels;
			m_OffsetPixels = offsetPixels;

			m_UnitsPerPixel = GetMaxUnitsPerPixel(imageLengthInPixels, imageHeightInPixels, drawingGlobalSize.Width, drawingGlobalSize.Height);
			if (Utils.FGT(minUnitsPerPixel, m_UnitsPerPixel))
				m_UnitsPerPixel = minUnitsPerPixel;

			// place drawing at the center
			double offset_X = (imageLengthInPixels - drawingGlobalSize.Width / m_UnitsPerPixel) / 2;
			if (Utils.FGT(offset_X, 0.0))
				m_OffsetPixels += new Vector(offset_X, 0.0);
			double offset_Y = (imageHeightInPixels - drawingGlobalSize.Height / m_UnitsPerPixel) / 2;
			if (Utils.FGT(offset_Y, 0.0))
				m_OffsetPixels += new Vector(0.0, offset_Y);
		}

		#region Properties

		//=============================================================================
		// (0, 0)-point offset
		private Vector m_OffsetPixels = new Vector(0.0, 0.0);
		public Vector OffsetInPixels { get { return m_OffsetPixels; } }

		//=============================================================================
		/// <summary>
		/// Image length in pixels
		/// </summary>
		private int m_ImageLengthInPixels = 1;
		//=============================================================================
		/// <summary>
		/// Image height in pixels
		/// </summary>
		private int m_ImageHeightInPixels = 1;
		//=============================================================================
		/// <summary>
		/// How many drawing units in 1 image pixel
		/// </summary>
		private double m_UnitsPerPixel = 1.0;

		#endregion

		#region Methods

		//=============================================================================
		public Point GetLocalPoint(Point globalPoint, double UnitsPerCameraPixel, Vector cameraOffset)
		{
			return CoordinateSystemConverter.sGetLocalPoint(globalPoint, m_UnitsPerPixel, new Vector(0.0, 0.0)) + m_OffsetPixels;
		}

		//=============================================================================
		public double GetWidthInPixels(double globalWidthValue, double UnitsPerCameraPixel)
		{
			// cameraScale doesnt affect on calcuations
			return CoordinateSystemConverter._sConvertToScreenLength(globalWidthValue, m_UnitsPerPixel);
		}
		//=============================================================================
		public double GetGlobalWidth(double widthInPixels, double UnitsPerCameraPixel)
		{
			// cameraScale doesnt affect on calcuations
			return CoordinateSystemConverter._sConvertToGlobalLength(widthInPixels, m_UnitsPerPixel);
		}

		//=============================================================================
		public double GetHeightInPixels(double globalHeightValue, double UnitsPerCameraPixel)
		{
			// cameraScale doesnt affect on calcuations
			return CoordinateSystemConverter._sConvertToScreenWidth(globalHeightValue, m_UnitsPerPixel);
		}
		//=============================================================================
		public double GetGlobalHeight(double heightInPixels, double UnitsPerCameraPixel)
		{
			// cameraScale doesnt affect on calcuations
			return CoordinateSystemConverter._sConvertToGlobalWidth(heightInPixels, m_UnitsPerPixel);
		}

		#endregion

		/// <summary>
		/// Calculates max units per one pixel value
		/// </summary>
		public static double GetMaxUnitsPerPixel(int imageLengthInPixels, int imageHeightInPixels, double drawingLength, double drawingHeight)
		{
			double horizUnitsPerPixel = drawingLength / imageLengthInPixels;
			double vertUnitsPerPixel = drawingHeight / imageHeightInPixels;
			return System.Math.Max(horizUnitsPerPixel, vertUnitsPerPixel);
		}
	}
}
