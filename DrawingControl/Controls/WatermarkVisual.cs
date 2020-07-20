using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DrawingControl
{
	// Draws watermarks images in given rectangle area.
	// Call it and draw watermarks over other graphics.
	public class WatermarkVisual : DrawingVisual
	{
		public WatermarkVisual(FrameworkElement owner)
		{
			m_Owner = owner;
		}

		//=============================================================================
		private FrameworkElement m_Owner = null;

		//=============================================================================
		/// <summary>
		/// Return null as HitTest for the watermark visual.
		/// It removes WatermarkVisual from any mouse\wheel clicks events.
		/// Otherwise, WatermarkVisual blocks rotate 3D model when user clicks
		/// mouse wheel on WatermarkVisual inside RackViewport3D.
		/// </summary>
		protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
		{
			//return base.HitTestCore(hitTestParameters);
			return null;
		}
		//=============================================================================
		protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
		{
			//return base.HitTestCore(hitTestParameters);
			return null;
		}

		//=============================================================================
		public void Draw(ImageSource watermarkImage)
		{
			using (DrawingContext thisDC = this.RenderOpen())
			{
				if (m_Owner == null)
					return;

				if(watermarkImage != null)
					WatermarkVisual.sDrawWatermark(thisDC, m_Owner.ActualWidth, m_Owner.ActualHeight, watermarkImage);
			}
		}

		//=============================================================================
		/// <summary>
		/// Default watermark image
		/// </summary>
		private static BitmapImage sWatermarkImage = null;
		public static BitmapImage WatermarkImage
		{
			get
			{
				if (sWatermarkImage == null)
					sWatermarkImage = new BitmapImage(new Uri("pack://application:,,,/DrawingFactoryApp;component/Properties/Watermark.png"));

				return sWatermarkImage;
			}
		}

		//=============================================================================
		public static void sDrawWatermark(DrawingContext dc, double visualWidth, double visualHeight, ImageSource watermarkImage)
		{
			if (dc == null)
				return;

			if (watermarkImage == null)
				return;

			if (Utils.FLE(watermarkImage.Width, 0.0) || Utils.FLE(watermarkImage.Height, 0.0))
				return;

			if (Utils.FLE(visualWidth, 0.0) || Utils.FLE(visualHeight, 0.0))
				return;

			// draw watermark by diagonal
			double rAngRad = Utils.ConvertToRadians(WatermarkInfo.WatermarkAngle);
			// Grid cell size in pixels, this cell should contains watermark image with margins.
			int columnWidthInPixels = (int)Math.Floor(visualWidth / WatermarkInfo.Columns);
			int rowHeightInPixels = (int)Math.Floor(visualHeight / WatermarkInfo.Rows);
			//
			int resultImageWidthInPixels = (int)Math.Floor((double)columnWidthInPixels - 2 * WatermarkInfo.MarginX);
			int resultImageHeightInPixels = (int)Math.Floor((double)rowHeightInPixels - 2 * WatermarkInfo.MarginY);

			// cut all watermarks outside control area
			dc.PushClip(new RectangleGeometry(new Rect(0.0, 0.0, visualWidth, visualHeight)));

			// apply rotate
			dc.PushTransform(new RotateTransform(WatermarkInfo.WatermarkAngle, visualWidth / 2, visualHeight / 2));

			ImageBrush imageBrush = new ImageBrush(watermarkImage);
			//imageBrush.RelativeTransform = new ScaleTransform(1.0, -1.0, 0.5, 0.5);
			imageBrush.Opacity = WatermarkInfo.Opacity;
			imageBrush.Stretch = Stretch.Uniform;

			// width and height are greater than visualWidth and visualHeight
			// need to calc start pnt
			// visualWidth and visualHeight rectangle should be centered in width and height rectangle
			Point startPnt = new Point(0.0, 0.0);

			// draw
			for (int iRowIndex = 0; iRowIndex < WatermarkInfo.Rows; ++iRowIndex)
			{
				for (int iColumnIndex = 0; iColumnIndex <= WatermarkInfo.Columns; ++iColumnIndex)
				{
					Point rectStartPnt = startPnt;
					//
					rectStartPnt.X += iColumnIndex * columnWidthInPixels;
					rectStartPnt.Y += iRowIndex * rowHeightInPixels;
					// add margin
					rectStartPnt.X += WatermarkInfo.MarginX;
					rectStartPnt.Y += WatermarkInfo.MarginY;

					Point rectEndPnt = rectStartPnt;
					rectEndPnt.X += resultImageWidthInPixels;
					rectEndPnt.Y += resultImageHeightInPixels;

					dc.DrawRectangle(imageBrush, null, new Rect(rectStartPnt, rectEndPnt));
				}
			}

			// pop rotate transform
			dc.Pop();
			// pop clip
			dc.Pop();
		}

		//=============================================================================
		public static void sDrawSingleWatermark(DrawingContext dc, double visualWidth, double visualHeight, ImageSource watermarkImage)
		{
			if (dc == null)
				return;

			if (watermarkImage == null)
				return;

			if (Utils.FLE(watermarkImage.Width, 0.0) || Utils.FLE(watermarkImage.Height, 0.0))
				return;

			if (Utils.FLE(visualWidth, 0.0) || Utils.FLE(visualHeight, 0.0))
				return;

			double scale = Utils.CalcScale(visualWidth, visualHeight, watermarkImage.Width, watermarkImage.Height);
			//
			Point startPnt = new Point(0.0, 0.0);
			startPnt.X = (visualWidth - watermarkImage.Width * scale) / 2;
			startPnt.Y = (visualHeight - watermarkImage.Height * scale) / 2;
			//
			Point endPnt = startPnt;
			endPnt.X += scale * watermarkImage.Width;
			endPnt.Y += scale * watermarkImage.Height;

			ImageBrush imageBrush = new ImageBrush(watermarkImage);
			imageBrush.RelativeTransform = new ScaleTransform(1.0, -1.0, 0.5, 0.5);
			imageBrush.Opacity = 0.1;

			dc.DrawRectangle(imageBrush, null, new Rect(startPnt, endPnt));
		}
	}
}
