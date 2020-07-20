using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	/// <summary>
	/// Displays current camera position relative to entire sheet.
	/// </summary>
	public class SheetMinimapControl : FrameworkElement
	{
		#region Dependency properties

		//=============================================================================
		/// <summary>
		/// Displayed sheet minimap
		/// </summary>
		public static readonly DependencyProperty SheetProperty = DependencyProperty.Register(
			"Sheet",
			typeof(DrawingSheet),
			typeof(SheetMinimapControl),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, On_Sheet_Changed));
		//
		public DrawingSheet Sheet
		{
			get { return (DrawingSheet)GetValue(SheetMinimapControl.SheetProperty); }
			set { SetValue(SheetMinimapControl.SheetProperty, value); }
		}
		//
		private static void On_Sheet_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SheetMinimapControl minimapControl = d as SheetMinimapControl;
			if (minimapControl != null)
			{
				minimapControl.On_Sheet_Changed(e.OldValue as DrawingSheet, e.NewValue as DrawingSheet);
				minimapControl.UpdateSize();
			}
		}
		//
		private void On_Sheet_Changed(DrawingSheet oldSheet, DrawingSheet newSheet)
		{
			if(oldSheet != null)
				oldSheet.PropertyChanged -= SheetPropertyChanged;

			if (newSheet != null)
				newSheet.PropertyChanged += SheetPropertyChanged;
		}

		//=============================================================================
		/// <summary>
		/// Minimap background brush
		/// </summary>
		public static readonly DependencyProperty BackgroundBrushProperty =
				DependencyProperty.Register("BackgroundBrush",
				typeof(Brush),
				typeof(SheetMinimapControl),
				new FrameworkPropertyMetadata(Brushes.DarkGray,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush BackgroundBrush
		{
			get { return (Brush)GetValue(BackgroundBrushProperty); }
			set { SetValue(BackgroundBrushProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Camera area brush
		/// </summary>
		public static readonly DependencyProperty CameraAreaBrushProperty =
				DependencyProperty.Register("CameraAreaBrush",
				typeof(Brush),
				typeof(SheetMinimapControl),
				new FrameworkPropertyMetadata(Brushes.DarkGray,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush CameraAreaBrush
		{
			get { return (Brush)GetValue(CameraAreaBrushProperty); }
			set { SetValue(CameraAreaBrushProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Minimap border brush
		/// </summary>
		public static readonly DependencyProperty BorderBrushProperty =
				DependencyProperty.Register("BorderBrush",
				typeof(Brush),
				typeof(SheetMinimapControl),
				new FrameworkPropertyMetadata(Brushes.Black,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush BorderBrush
		{
			get { return (Brush)GetValue(BorderBrushProperty); }
			set { SetValue(BorderBrushProperty, value); }
		}

		#endregion

		#region Overrides

		//=============================================================================
		protected override Size MeasureOverride(Size availableSize)
		{
			return CalculateControlSize(availableSize);
		}
		//=============================================================================
		protected override Size ArrangeOverride(Size finalSize)
		{
			return CalculateControlSize(finalSize);
		}

		//=============================================================================
		protected override void OnRender(DrawingContext drawingContext)
		{
			if (drawingContext == null)
				return;

			DrawingSheet sheet = this.Sheet;
			if (sheet == null)
				return;

			if (sheet.IsSheetFullyDisplayed)
				return;

			if (DrawingDocument._sDrawing == null)
				return;

			// Draw sheet border and background.
			Point topLeftPnt = new Point(0.0, 0.0);
			Point botRightPnt = new Point(this.ActualWidth, this.ActualHeight);
			drawingContext.DrawRectangle(this.BackgroundBrush, new Pen(this.BorderBrush, 1.0), new Rect(topLeftPnt, botRightPnt));

			Vector cameraOffset = sheet.GetCameraOffset();
			double cameraWidthInUnits = DrawingDocument._sDrawing.ActualWidth * sheet.UnitsPerCameraPixel;
			double cameraHeightInUnits = DrawingDocument._sDrawing.ActualHeight * sheet.UnitsPerCameraPixel;

			// Draw camera area
			Point cameraAreaTopLeftPnt = new Point(0.0, 0.0);
			cameraAreaTopLeftPnt.X += (cameraOffset.X / sheet.Length) * this.ActualWidth;
			if (Utils.FGT(cameraWidthInUnits, sheet.Length))
				cameraAreaTopLeftPnt.X = this.ActualWidth / 2 - ((cameraWidthInUnits / 2) / sheet.Length) * this.ActualWidth;
			cameraAreaTopLeftPnt.Y += (cameraOffset.Y / sheet.Width) * this.ActualHeight;
			if (Utils.FGT(cameraHeightInUnits, sheet.Width))
				cameraAreaTopLeftPnt.Y = this.ActualHeight - ((cameraHeightInUnits / 2) / sheet.Width) * this.ActualHeight;

			Point cameraAreaBotRightPnt = cameraAreaTopLeftPnt;
			cameraAreaBotRightPnt.X += (cameraWidthInUnits / sheet.Length) * this.ActualWidth;
			cameraAreaBotRightPnt.Y += (cameraHeightInUnits / sheet.Width) * this.ActualHeight;
			drawingContext.DrawRectangle(this.CameraAreaBrush, null, new Rect(cameraAreaTopLeftPnt, cameraAreaBotRightPnt));
		}

		#endregion

		#region Public functions

		/// <summary>
		/// Recalcultate control size, it depends on the sheet.
		/// </summary>
		public void UpdateSize()
		{
			// call MeasureOverride and ArrangeOverride for calc new SheetMinimapControl size, because new sheet can has different length and width
			this.InvalidateVisual();
			this.InvalidateArrange();
			this.InvalidateMeasure();
			this.UpdateLayout();
		}

		#endregion

		#region Protected functions

		//=============================================================================
		/// <summary>
		/// Calculates control size. It should have the same length\height ratio as a current sheet length\height ratio.
		/// </summary>
		/// <param name="availableSize"></param>
		/// <returns></returns>
		protected Size CalculateControlSize(Size availableSize)
		{
			double length = 1;
			double width = 1;

			DrawingSheet currSheet = Sheet;
			if (currSheet != null)
			{
				length = currSheet.Length;
				width = currSheet.Width;
			}

			Size result = new Size();

			double rLengthWidthRatio = length / width;

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

		//=============================================================================
		private void SheetPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			DrawingSheet sheet = sender as DrawingSheet;
			if (sheet == null)
				return;

			if (e.PropertyName == Utils.GetPropertyName(() => sheet.Length)
				|| e.PropertyName == Utils.GetPropertyName(() => sheet.Width))
			{
				UpdateSize();
			}
			else if(e.PropertyName == Utils.GetPropertyName(() => sheet.CameraOffset)
				|| e.PropertyName == Utils.GetPropertyName(() => sheet.UnitsPerCameraPixel)
				|| e.PropertyName == Utils.GetPropertyName(() => sheet.TemporaryCameraOffset))
			{
				this.InvalidateVisual();
			}
		}

		#endregion
	}
}
