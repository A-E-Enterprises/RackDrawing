using System.Windows;

namespace DrawingControl
{
	/// <summary>
	/// Converts global drawing values(points, dimensions) to local(screen or control) values.
	/// </summary>
	public interface ICoordinateSystem
	{
		/// <summary>
		/// Returns local point(screen point) relative to drawing global point.
		/// </summary>
		Point GetLocalPoint(Point globalPoint, double UnitsPerCameraPixel, Vector cameraOffset);

		double GetWidthInPixels(double globalWidthValue, double UnitsPerCameraPixel);
		double GetGlobalWidth(double widthInPixels, double UnitsPerCameraPixel);

		double GetHeightInPixels(double globalHeightValue, double UnitsPerCameraPixel);
		double GetGlobalHeight(double heightInPixels, double UnitsPerCameraPixel);
	}
}
