using System;
using System.Windows;

namespace DrawingControl
{
	/// <summary>
	/// Converts global point to local point and vice versa.
	/// </summary>
	public static class CoordinateSystemConverter
	{
		//=============================================================================
		// 
		// screenPoint - point relative DrawingControl's top left corner
		//
		// Returns "global" point in Length_Y and Length_Z values
		// It allow to show drawing always correct, DrawingContol's
		// real width and height doesnt matter. Because all geometry
		// save only global points. If you change application's window size
		// then drawing will be shown correct also.
		//
		// It is NECESSARY to save all geometry in global points.
		//
		static public Point sGetGlobalPoint(Point screenPoint, DrawingSheet sheet)
		{
			if (sheet == null)
				return new Point();

			return sGetGlobalPoint(screenPoint, sheet.UnitsPerCameraPixel, sheet.GetCameraOffset());
		}
		//
		static public Point sGetGlobalPoint(Point imagePoint, double UnitsPerCameraPixel, Vector cameraOffset)
		{
			try
			{
				Point globalPoint = new Point();
				globalPoint.X = cameraOffset.X + UnitsPerCameraPixel * imagePoint.X;
				globalPoint.X = Convert.ToInt32(Math.Truncate(globalPoint.X));
				//
				globalPoint.Y = cameraOffset.Y + UnitsPerCameraPixel * imagePoint.Y;
				globalPoint.Y = Convert.ToInt32(Math.Truncate(globalPoint.Y));

				return globalPoint;
			}
			catch { }

			return new Point();
		}

		//=============================================================================
		// Read comment above.
		// Returns local point which can be drawn in DrawingControl.
		static public Point sGetLocalPoint(Point globalPoint, double UnitsPerCameraPixel, Vector cameraOffset)
		{
			try
			{
				Point localPoint = new Point();
				// Convert global size to pixels
				localPoint.X = globalPoint.X / UnitsPerCameraPixel - cameraOffset.X / UnitsPerCameraPixel;
				localPoint.Y = globalPoint.Y / UnitsPerCameraPixel - cameraOffset.Y / UnitsPerCameraPixel;

				return localPoint;
			}
			catch { }

			return new Point();
		}

		//=============================================================================
		static public double _sConvertToScreenLength(double rGlobalLengthValue, double UnitsPerCameraPixel)
		{
			try
			{
				return rGlobalLengthValue / UnitsPerCameraPixel;
			}
			catch { }

			return 0.0;
		}
		//=============================================================================
		static public double _sConvertToGlobalLength(double rLengthInPixels, double UnitsPerCameraPixel)
		{
			try
			{
				return rLengthInPixels * UnitsPerCameraPixel;
			}
			catch { }

			return 0.0;
		}

		//=============================================================================
		static public double _sConvertToScreenWidth(double rGlobalWidthValue, double UnitsPerCameraPixel)
		{
			try
			{
				return rGlobalWidthValue / UnitsPerCameraPixel;
			}
			catch { }

			return 0.0;
		}
		//=============================================================================
		static public double _sConvertToGlobalWidth(double rWidthInPixels, double UnitsPerCameraPixel)
		{
			try
			{
				return rWidthInPixels * UnitsPerCameraPixel;
			}
			catch { }

			return 0.0;
		}
	}
}
