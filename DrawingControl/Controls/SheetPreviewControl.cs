using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	/// <summary>
	/// WPF-control that displays the DrawingSheet content.
	/// Used at PlaceSheetDialog.xaml.
	/// </summary>
	public class SheetPreviewControl : FrameworkElement
	{
		#region Dependency properties

		//=============================================================================
		public static readonly DependencyProperty SheetProperty = DependencyProperty.Register(
			"Sheet",
			typeof(DrawingSheet),
			typeof(SheetPreviewControl),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, On_Sheet_Changed));
		//
		public DrawingSheet Sheet
		{
			get { return (DrawingSheet)GetValue(SheetPreviewControl.SheetProperty); }
			set { SetValue(SheetPreviewControl.SheetProperty, value); }
		}
		//
		private static void On_Sheet_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SheetPreviewControl spc = d as SheetPreviewControl;
			if (spc != null)
			{
				// call MeasureOverride and ArrangeOverride for calc new SheetPreviewControl size, because new sheet can has different length and width
				spc.InvalidateArrange();
				spc.InvalidateMeasure();
				spc.UpdateLayout();
				spc.InvalidateVisual();
			}
		}

		#endregion

		#region Functions overrides

		//=============================================================================
		protected override Size MeasureOverride(Size availableSize)
		{
			return _SizeWithRatio(availableSize);
		}
		//=============================================================================
		protected override Size ArrangeOverride(Size finalSize)
		{
			return _SizeWithRatio(finalSize);
		}

		//=============================================================================
		protected override void OnRender(DrawingContext drawingContext)
		{
			if (drawingContext == null)
				return;

			DrawingSheet sheet = this.Sheet;
			if (sheet == null)
				return;

			// STEP 1.
			// Draw sheet content with opacity.
			drawingContext.PushOpacity(0.2);
			//
			int LengthInPixels = Utils.GetWholeNumber(this.ActualWidth);//Utils.GetWholeNumber(cs.GetWidthInPixels(DrawingGlobalSize.Width, Length_X));
			int HeightInPixels = Utils.GetWholeNumber(this.ActualHeight);//Utils.GetWholeNumber(cs.GetHeightInPixels(DrawingGlobalSize.Height, Length_Y));
			//
			ImageCoordinateSystem ics = new ImageCoordinateSystem(LengthInPixels, HeightInPixels, new Vector(0.0, 0.0), new Size(sheet.Length, sheet.Width), 0.0);
			DefaultGeomDisplaySettings geomDisplaySettings = new DefaultGeomDisplaySettings();
			geomDisplaySettings.DisplayText = false;

			//
			if (sheet.Rectangles != null)
			{
				foreach (BaseRectangleGeometry geom in sheet.Rectangles)
				{
					if (geom == null)
						continue;

					// Dont display SheetElevationGeometry, because it has fixed size in pixels
					if (geom is SheetElevationGeometry)
						continue;

					geom.Draw(drawingContext, ics, geomDisplaySettings);
				}
			}
			// Pop opacity
			drawingContext.Pop();

			// STEP 2.
			// Draw gray sheet border.
			Size drawingSize = new Size(sheet.Length, sheet.Width);
			Point pt_01 = ics.GetLocalPoint(new Point(0.0, 0.0), sheet.UnitsPerCameraPixel, sheet.GetCameraOffset());
			Point pt_02 = ics.GetLocalPoint(new Point(sheet.Length, sheet.Width), sheet.UnitsPerCameraPixel, sheet.GetCameraOffset());
			drawingContext.DrawRectangle(null, new Pen(Brushes.Gray, 1.0), new Rect(pt_01, pt_02));
		}

		#endregion

		#region Protected functions

		//=============================================================================
		/// <summary>
		/// Calculates control size. It should have the same length\height ratio as a current sheet length\height ratio.
		/// </summary>
		/// <param name="availableSize"></param>
		/// <returns></returns>
		protected Size _SizeWithRatio(Size availableSize)
		{
			double _length = 1;
			double _width = 1;

			DrawingSheet _currSheet = Sheet;
			if (_currSheet != null)
			{
				_length = _currSheet.Length;
				_width = _currSheet.Width;
			}

			Size result = new Size();

			double rLengthWidthRatio = _length / _width;

			// 
			bool bGetLength = true;
			if (availableSize.Width > availableSize.Height)
			{
				if (rLengthWidthRatio >= 1)
				{
					double rNewWidth = availableSize.Width / rLengthWidthRatio;
					if (rNewWidth > availableSize.Height)
						bGetLength = false;
				}
				else
				{
					bGetLength = false;
					double rNewLength = availableSize.Height * rLengthWidthRatio;
					if (rNewLength > availableSize.Width)
						bGetLength = true;
				}
			}
			else
			{
				if (rLengthWidthRatio <= 1)
				{
					bGetLength = false;
					double rNewLength = availableSize.Height * rLengthWidthRatio;
					if (rNewLength > availableSize.Width)
						bGetLength = true;
				}
				else
				{
					double rNewWidth = availableSize.Width / rLengthWidthRatio;
					if (rNewWidth > availableSize.Height)
						bGetLength = false;
				}
			}

			if (bGetLength)
			{
				result.Width = availableSize.Width;
				result.Height = availableSize.Width / rLengthWidthRatio;
			}
			else
			{
				result.Height = availableSize.Height;
				result.Width = availableSize.Height * rLengthWidthRatio;
			}

			return result;
		}

		#endregion
	}
}
