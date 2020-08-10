using AppColorTheme;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public class GuardColumnParameters
	{
		public const double GuardColumnFrontOffset = 45;
		public const double GuardColumnSideOffset = 40;
		public const double GuardColumnDepth = 108;
		public const double GuardColumnInnerSideDepth = 10;

		public const double GuardColumnHeight = 400;
		public const double GuardColumnForwardOffset = 125;
		public const double GuardColumnInnerAngleOffset = 25;
		public static double GuardColumnInnerHeight => (GuardColumnHeight - GuardColumnInnerAngleOffset);
		public static double GuardColumnTopDepth => (GuardColumnDepth - GuardColumnInnerAngleOffset);
	}

	public class GuardRowParameters
	{
		public const double GuardRowFoundationLength = 70;
		public const double GuardRowFoundationWidth = 160;
		public const double GuardRowFoundationHeight = 8;
		public const double GuardRowFoundationSupportHeight = 60;

		public const double GuardRowHeight = 400;
		public const double GuardRowWidth = 60;

		public const double GuardRowRackOffset = 50;

		public static double GuardColumnSuppotOffset => (GuardRowFoundationWidth - GuardRowWidth) / 2;
    }

	public class RackAdvancedDrawingSettings
	{
		public RackAdvancedDrawingSettings(
			double levelTextSize,
			double levelLoadTextSize,
			double viewTextSize,
			double dimensionsTextSize,
			double minDimensionsLinesOffset,
			double perpDimLineOffsetInPixels,
			double bracingLinesThickness,
			bool displayTextAndDimensions,
			bool isItSheetElevationPicture
			)
		{
			LevelTextSize = levelTextSize;
			LevelLoadTextSize = levelLoadTextSize;
			ViewTextSize = viewTextSize;
			DimensionsTextSize = dimensionsTextSize;
			MinDimensionsLinesOffset = minDimensionsLinesOffset;
			BracingLineThickness = bracingLinesThickness;
			PerpDimLinesOffsetInPixels = perpDimLineOffsetInPixels;
			DisplayTextAndDimensions = displayTextAndDimensions;
			IsItSheetElevationPicture = isItSheetElevationPicture;

			InitBrushesAndPens();
		}

		#region Properties

		/// <summary>
		/// Level name(for example - Level GR) text size
		/// </summary>
		public double LevelTextSize { get; protected set; }
		/// <summary>
		/// Level load text size.
		/// For example: 1000kg
		/// </summary>
		public double LevelLoadTextSize { get; protected set; }
		public double ViewTextSize { get; protected set; }
		public double DimensionsTextSize { get; protected set; }
		/// <summary>
		/// Dimensions line offset in pixels
		/// </summary>
		public double MinDimensionsLinesOffset { get; protected set; }
		/// <summary>
		/// Perpendicular dimensions line offset in pixels.
		/// </summary>
		public double PerpDimLinesOffsetInPixels { get; protected set; }

		public double BracingLineThickness { get; protected set; }

		/// <summary>
		/// Rack column fill brush
		/// </summary>
		public Brush ColumnFillBrush { get; protected set; }
		/// <summary>
		/// Rack column border
		/// </summary>
		public Pen ColumnBorderPen { get; protected set; }

		/// <summary>
		/// Bottom line brush, on which rack is placed
		/// </summary>
		public Brush BottomLineBrush { get; protected set; }
		public Pen BottomLinePen { get; protected set; }

		/// <summary>
		/// Rack level shelf brush
		/// </summary>
		public Brush LevelShelfBrush { get; protected set; }
		public Pen LevelShelfPen { get; protected set; }

		/// <summary>
		/// Rack pallet fill brush
		/// </summary>
		public Brush PalletFillBrush { get; protected set; }
		/// <summary>
		/// Rack pallet border
		/// </summary>
		public Pen PalletBorderPen { get; protected set; }

		/// <summary>
		/// Rack bracing lines brush
		/// </summary>
		public Brush BracingLineBrush { get; protected set; }
		/// <summary>
		/// Rack bracing lines
		/// </summary>
		public Pen BracingLinePen { get; protected set; }

		/// <summary>
		/// Rack pallet riser fill brush
		/// </summary>
		public Brush PalletRiserFillBrush { get; protected set; }
		/// <summary>
		/// Rack pallet riser border
		/// </summary>
		public Pen PalletRiserBorderPen { get; protected set; }

		public Brush DeckingPlateBrush { get; protected set; }

		public Brush DimensionsBrush { get; protected set; }

		public Brush TextBrush { get; protected set; }

		public Brush RackGuardMainBrush { get; protected set; }

		public Brush RackGuardAltBrush { get; protected set; }

		/// <summary>
		/// If true then need to display text(view name, level name and level load text) and dimensions.
		/// </summary>
		public bool DisplayTextAndDimensions { get; protected set; }

		/// <summary>
		/// If true then need to display sheet elevation picture.
		/// In this case need to use rack color for rack column, level shelf, bracing etc.
		/// </summary>
		public bool IsItSheetElevationPicture { get; protected set; }

		#endregion

		#region Methods

		private void InitBrushesAndPens()
		{
			this.ColumnFillBrush = CurrentGeometryColorsTheme.GetRackAdvProps_ColumnFillBrush();
			this.ColumnBorderPen = new Pen(this.ColumnFillBrush, 1.0);

			this.BottomLineBrush = CurrentGeometryColorsTheme.GetRackAdvProps_BottomLineBrush();
			this.BottomLinePen = new Pen(this.BottomLineBrush, 1.0);

			this.LevelShelfBrush = CurrentGeometryColorsTheme.GetRackAdvProps_LevelShelfBrush();
			this.LevelShelfPen = new Pen(this.LevelShelfBrush, 1.0);

			this.BracingLineBrush = CurrentGeometryColorsTheme.GetRackAdvProps_BracingLinesBrush();
			this.BracingLinePen = new Pen(this.BracingLineBrush, this.BracingLineThickness);

			this.PalletFillBrush = CurrentGeometryColorsTheme.GetRackAdvProps_PalletFillBrush();
			this.PalletBorderPen = new Pen(CurrentGeometryColorsTheme.GetRackAdvProps_PalletBorderBrush(), 1.0);

			this.PalletRiserFillBrush = CurrentGeometryColorsTheme.GetRackAdvProps_PalletRiserFillBrush();
			this.PalletRiserBorderPen = new Pen(CurrentGeometryColorsTheme.GetRackAdvProps_PalletRiserBorderBrush(), 1.0);

			this.DeckingPlateBrush = CurrentGeometryColorsTheme.GetRackAdvProps_DeckingPlateFillBrush();

			this.DimensionsBrush = CurrentGeometryColorsTheme.GetRackAdvProps_DimensionsBrush();

			this.TextBrush = CurrentGeometryColorsTheme.GetRackAdvProps_TextBrush();

			this.RackGuardMainBrush = CurrentGeometryColorsTheme.GetRackAdvProps_RackGuardMainBrush();

			this.RackGuardAltBrush = CurrentGeometryColorsTheme.GetRackAdvProps_RackGuardAltBrush();
		}

		#endregion

		public static RackAdvancedDrawingSettings GetDefaultSettings()
		{
			return new RackAdvancedDrawingSettings(
				14.0,
				16.0,
				32.0,
				14.0,
				20.0,
				5.0,
				2.0,
				true,
				false
				);
		}

		public static RackAdvancedDrawingSettings GetSheetElevationDefaultSettings()
		{
			RackAdvancedDrawingSettings defSettings = GetDefaultSettings();
			defSettings.DisplayTextAndDimensions = false;
			defSettings.IsItSheetElevationPicture = true;

			return defSettings;
		}
	}

	public class RackDrawingBrushes
    {
        public RackDrawingBrushes(RackAdvancedDrawingSettings displaySettings, Color rackFillColor)
        {
			// Need to clone brushes and pens, otherwise displaySetting's brushes and pens will be modified.
			// It is a problem for sheet elevation picture, because all racks will have border of the last exported rack.

			if (displaySettings.ColumnFillBrush != null)
				ColumnFillBrush = displaySettings.ColumnFillBrush.Clone();

			if (displaySettings.ColumnBorderPen != null)
				ColumnBorderPen = displaySettings.ColumnBorderPen.Clone();

			if (displaySettings.LevelShelfBrush != null)
				LevelShelfFillBrush = displaySettings.LevelShelfBrush.Clone();

			if (displaySettings.LevelShelfPen != null)
				LevelShelfBorderPen = displaySettings.LevelShelfPen.Clone();

			if (displaySettings.PalletFillBrush != null)
				PalletFillBrush = displaySettings.PalletFillBrush.Clone();

			if (displaySettings.PalletBorderPen != null)
				PalletBorderPen = displaySettings.PalletBorderPen.Clone();

			if (displaySettings.PalletRiserFillBrush != null)
				PalletRiserFillBrush = displaySettings.PalletRiserFillBrush.Clone();

			if (displaySettings.PalletRiserBorderPen != null)
				PalletRiserBorderPen = displaySettings.PalletRiserBorderPen.Clone();

			if (displaySettings.BracingLineBrush != null)
				BracingLineFillBrush = displaySettings.BracingLineBrush.Clone();

			if (displaySettings.BracingLinePen != null)
				BracingLineBorderPen = displaySettings.BracingLinePen.Clone();

			if (displaySettings.IsItSheetElevationPicture)
			{
				Brush rackFillBrush = new SolidColorBrush(rackFillColor);
				ColumnFillBrush = rackFillBrush;
				ColumnBorderPen.Brush = rackFillBrush;
				LevelShelfFillBrush = rackFillBrush;
				LevelShelfBorderPen.Brush = rackFillBrush;
				PalletFillBrush = null;
				PalletBorderPen.Brush = rackFillBrush;
				PalletRiserFillBrush = null;
				PalletRiserBorderPen.Brush = rackFillBrush;
				BracingLineFillBrush = null;
				BracingLineBorderPen.Brush = rackFillBrush;
			}
		}

		public Brush ColumnFillBrush { get; private set; }
		public Pen ColumnBorderPen { get; private set; }
		public Brush LevelShelfFillBrush { get; private set; }
		public Pen LevelShelfBorderPen { get; private set; }
		public Brush PalletFillBrush { get; private set; }
		public Pen PalletBorderPen { get; private set; }
		public Brush PalletRiserFillBrush { get; private set; }
		public Pen PalletRiserBorderPen { get; private set; }
		public Brush BracingLineFillBrush { get; private set; }
		public Pen BracingLineBorderPen { get; private set; }
	}

	// Displays rack advanced properties view.
	// Front and side view of the rack with pallets\levels\beams\etc.
	// Displayed only when ShowAdvancedProperties = true.
	public class RackAdvancedPropertiesControl : FrameworkElement
	{
		public RackAdvancedPropertiesControl()
		{
			//
			m_WatermarkVisual = new WatermarkVisual(this);
			this.AddLogicalChild(m_WatermarkVisual);
			this.AddVisualChild(m_WatermarkVisual);
		}

		#region Dependency properties

		/// <summary>
		/// Displayed document.
		/// </summary>
		public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(
			"Document",
			typeof(DrawingDocument),
			typeof(RackAdvancedPropertiesControl),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, On_Document_Changed)
			);
		public DrawingDocument Document
		{
			get { return (DrawingDocument)GetValue(RackAdvancedPropertiesControl.DocumentProperty); }
			set { SetValue(RackAdvancedPropertiesControl.DocumentProperty, value); }
		}
		private static void On_Document_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RackAdvancedPropertiesControl rackAdvPropsControl = d as RackAdvancedPropertiesControl;
			if (rackAdvPropsControl == null)
				return;

			rackAdvPropsControl.On_DocumentChanged(e.OldValue as DrawingDocument, e.NewValue as DrawingDocument);
		}
		private void On_DocumentChanged(DrawingDocument oldDocument, DrawingDocument newDocument)
		{
			if(oldDocument != null)
				oldDocument.StateChanged -= Document_StateChanged;

			if (newDocument != null)
				newDocument.StateChanged += Document_StateChanged;
		}
		/// <summary>
		/// Document State property was changed.
		/// Update control.
		/// </summary>
		private void Document_StateChanged(object sender, EventArgs e)
		{
			// Redraw control
			this.InvalidateVisual();
		}


		/// <summary>
		/// WatermarkImage which is displayed over this element.
		/// </summary>
		public static readonly DependencyProperty WatermarkImageProperty =
				DependencyProperty.Register("WatermarkImage",
				typeof(ImageSource),
				typeof(RackAdvancedPropertiesControl),
				new FrameworkPropertyMetadata(null,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public ImageSource WatermarkImage
		{
			get { return (ImageSource)GetValue(WatermarkImageProperty); }
			set { SetValue(WatermarkImageProperty, value); }
		}

		#endregion

		#region Properties

		// Draws watermarks images in given rectangle area.
		// Call it and draw watermarks over other graphics.
		private WatermarkVisual m_WatermarkVisual = null;

		/// <summary>
		/// Drawing settings
		/// </summary>
		private RackAdvancedDrawingSettings m_DrawingSettings = RackAdvancedDrawingSettings.GetDefaultSettings();

		/// <summary>
		/// Override visual children count.
		/// VisualChildrenCount and GetVisualChild should be overrided together.
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				int visualChildrenCount = 0;

				if (m_WatermarkVisual != null)
					++visualChildrenCount;

				return visualChildrenCount;
			}
		}

		#endregion

		#region Overrides methods

		/// <summary>
		/// VisualChildrenCount and GetVisualChild should be overrided together.
		/// </summary>
		protected override Visual GetVisualChild(int index)
		{
			int offset = 0;

			if (index == offset)
				return m_WatermarkVisual;
			++offset;

			return null;
		}

		/// <summary>
		/// Draw selected rack advanced properties.
		/// </summary>
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			// Dont draw content is control is not visible.
			if (!this.IsVisible)
				return;
			if (this.Visibility != Visibility.Visible)
				return;

			if (drawingContext == null)
				return;

			DrawingDocument doc = this.Document;
			if (doc == null)
				return;
			DrawingSheet currentSheet = doc.CurrentSheet;
			if (currentSheet == null)
				return;
			// display advanced properties only if single rack is selected
			if (currentSheet.SelectedGeometryCollection.Count != 1)
				return;

			Rack selectedRack = currentSheet.SelectedGeometryCollection[0] as Rack;
			if (selectedRack == null)
				return;

			// draw rack advanced properties
			RackAdvancedPropertiesControl.sDraw(drawingContext, selectedRack, this.RenderSize, m_DrawingSettings);

			//// draw border for debug
			//drawingContext.DrawRectangle(null, new Pen(Brushes.Green, 3.0), new Rect(new Point(0.0, 0.0), new Point(this.ActualWidth, this.ActualHeight)));

			// draw watermark
			if (m_WatermarkVisual != null)
				m_WatermarkVisual.Draw(this.WatermarkImage);
		}

		#endregion

		//
		private static Typeface m_TextTypeFace = new Typeface(new FontFamily("MaterialDesign"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
		// Minimum dimension text offset(in pixels) from dimension line
		private static double m_MinDimTextOffsetInPixels = 10;
		// Max clear available height coef, relative to rack.MaxHeight.
		// It limits clear available height value, because if it is included in calculation without limit
		// then rack drawing can be too small. For example Clear Available Height is 12000 and rack height is 6000.
		public static double m_ClearAvailableHeightLimitCoef = 1.1;

		// Break dimension line symbol in pixels.
		// For example, dimension line is horizontal, so break symbol
		// will be draw in the middle of dimension line and has size (2*30, 2*30).
		public static double m_BreakDimensionLineSymbol = 30;

		/// <summary>
		/// Draws advanced rack view on the DrawingContext
		/// </summary>
		public static void sDraw(DrawingContext dc, Rack rackForDraw, Size imageSize, RackAdvancedDrawingSettings drawingSettings = null)
		{
			if (dc == null)
				return;
			if (imageSize == null)
				return;

			if (Utils.FLE(imageSize.Height, 0.0) || Utils.FLE(imageSize.Width, 0.0))
				return;

			Rack selectedRack = rackForDraw;
			if (selectedRack == null)
				return;
			if (selectedRack.Column == null)
				return;

			RackAdvancedDrawingSettings displaySettings = drawingSettings;
			if (displaySettings == null)
				displaySettings = RackAdvancedDrawingSettings.GetDefaultSettings();

			FormattedText FrontViewText;
			FormattedText SideViewText;
			double rackLength;
			double rackWidth;
			Size drawingSize;
			double MaxRackHeight;
			if (!RackAdvancedPropertiesControl._GetSizes(
				selectedRack,
				displaySettings,
				out drawingSize,
				out FrontViewText,
				out SideViewText,
				out rackLength,
				out rackWidth,
				out MaxRackHeight))
			{
				return;
			}

			// If levels have different beams then display beam name for each level.
			// Otherwise display beam name for the first level only.
			double rBeamTextMaxSizeInPixels = 0.0;
			if (rackForDraw.Levels != null)
			{
				foreach (RackLevel level in rackForDraw.Levels)
				{
					if (level == null)
						continue;
					if (level.Beam == null || level.Beam.Beam == null)
						continue;

					FormattedText beamFmtedText = new FormattedText(level.Beam.Beam.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, m_TextTypeFace, displaySettings.LevelTextSize, displaySettings.TextBrush);
					if (Utils.FGT(beamFmtedText.Width, rBeamTextMaxSizeInPixels))
						rBeamTextMaxSizeInPixels = beamFmtedText.Width;
				}
			}
			rBeamTextMaxSizeInPixels *= 1.5;

			// Text height in pixels
			string exampleText = "TEST";
			double dimTextHeight_Pixels = new FormattedText(exampleText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, m_TextTypeFace, displaySettings.DimensionsTextSize, displaySettings.TextBrush).Height;
			double viewNameTextHeight_Pixels = Math.Max(FrontViewText.Height, SideViewText.Height);
			// Height(in pixels) reserved for inner length and length dimensions, which are displayed at the top of the rack.
			double heightForDimensionsAtTop_Pixels = 2 * displaySettings.MinDimensionsLinesOffset + dimTextHeight_Pixels;
			//
			double spaceForDimInPixels = displaySettings.MinDimensionsLinesOffset + displaySettings.PerpDimLinesOffsetInPixels + m_MinDimTextOffsetInPixels + dimTextHeight_Pixels;
			// Space for bracing step and height dimensions at the side view.
			double spaceForBracingDimsInPixels = spaceForDimInPixels;
			if (eColumnBracingType.eXBracing == rackForDraw.Bracing || eColumnBracingType.eXBracingWithStiffener == rackForDraw.Bracing)
				spaceForBracingDimsInPixels = 3 * displaySettings.MinDimensionsLinesOffset;

			// image margin from borders.
			double imageMarginInPixels = 25;

			Size correctedImageSize = imageSize;
			// Reserve 20% of image width as a distance between front and side view.
			double distanceBetweenViewsInPixels = imageSize.Width * 0.2;
			correctedImageSize.Width -= imageMarginInPixels;
			correctedImageSize.Width -= distanceBetweenViewsInPixels;
			// remove space for level name
			correctedImageSize.Width -= rBeamTextMaxSizeInPixels;
			// remove space for level height dimension
			correctedImageSize.Width -= spaceForDimInPixels + displaySettings.MinDimensionsLinesOffset; //displaySettings.MinDimensionsLinesOffset + 2 * dimTextHeight_Pixels;
			// remove space for dimensions at the right side of the front view
			correctedImageSize.Width -= 4.9 * displaySettings.MinDimensionsLinesOffset;
			// remove space for bracings dimensions at the right side of the side view
			correctedImageSize.Width -= spaceForBracingDimsInPixels;
			// HEIGHT
			correctedImageSize.Height -= imageMarginInPixels;
			// remove space for length dimensions
			correctedImageSize.Height -= heightForDimensionsAtTop_Pixels;
			// remove space for the view name
			correctedImageSize.Height -= viewNameTextHeight_Pixels + viewNameTextHeight_Pixels / 2;

			// Set max beam name size as a offset, otherwise it is drawn otside rectangle.
			//ICoordinateSystem cs = new RackAdvancedPropsImageCoordinateSystem(correctedImageSize.Width, correctedImageSize.Height, new Vector(rBeamTextMaxSizeInPixels, 0.0), drawingSize, _Scale);
			ICoordinateSystem cs = new RackAdvancedPropsImageCoordinateSystem(correctedImageSize.Width, correctedImageSize.Height, drawingSize);

			double drawingHeightInPixels = 0.0;
			drawingHeightInPixels += heightForDimensionsAtTop_Pixels;
			drawingHeightInPixels += cs.GetHeightInPixels(drawingSize.Height, 1.0);
			drawingHeightInPixels += viewNameTextHeight_Pixels + viewNameTextHeight_Pixels / 2;

			double drawingWidthInPixels = 0.0;
			drawingWidthInPixels += distanceBetweenViewsInPixels;
			drawingWidthInPixels += cs.GetWidthInPixels(drawingSize.Width, 1.0);
			drawingWidthInPixels += rBeamTextMaxSizeInPixels;
			//drawingWidthInPixels += 2 * displaySettings.MinDimensionsLinesOffset + dimTextHeight_Pixels;
			drawingWidthInPixels += spaceForDimInPixels;// + displaySettings.MinDimensionsLinesOffset;
			drawingWidthInPixels += 4.9 * displaySettings.MinDimensionsLinesOffset;
			drawingWidthInPixels += spaceForBracingDimsInPixels;

			Vector baseOffsetInPixels = new Vector(0.0, 0.0);
			baseOffsetInPixels.Y = imageSize.Height;
			baseOffsetInPixels.Y -= (imageSize.Height - drawingHeightInPixels) / 2;
			baseOffsetInPixels.X = (imageSize.Width - drawingWidthInPixels) / 2;

			// FRONT VIEW
			Vector frontViewOffsetInPixels = baseOffsetInPixels;
			frontViewOffsetInPixels.Y -= viewNameTextHeight_Pixels + viewNameTextHeight_Pixels / 2;
			// Add space for beam text and level height dimension
			frontViewOffsetInPixels.X += rBeamTextMaxSizeInPixels;// + spaceForDimInPixels;
			//
			dc.PushTransform(new TranslateTransform(frontViewOffsetInPixels.X, frontViewOffsetInPixels.Y));
            DrawRackFrontView(dc, cs, displaySettings, selectedRack, FrontViewText);
            dc.Pop();

			// SIDE VIEW
			Vector sideViewOffsetInPixels = frontViewOffsetInPixels;

			Rack backToBackRack = rackForDraw.Sheet.GetBackToBackRack(rackForDraw);
			Rack tieBeamConnectedRack = rackForDraw.Sheet.GetTieBeamConnectedRack(rackForDraw); ;

			
			// Add space for front view, height dimensions and space between views
			sideViewOffsetInPixels.X += cs.GetWidthInPixels(rackLength, 1.0) + 4.9 * displaySettings.MinDimensionsLinesOffset + distanceBetweenViewsInPixels;
            //
            dc.PushTransform(new TranslateTransform(sideViewOffsetInPixels.X, sideViewOffsetInPixels.Y));
            //DrawRackSideView(dc, cs, displaySettings, selectedRack, SideViewText);

            DrawRackExtendedSideView(dc, cs, displaySettings, selectedRack, backToBackRack, tieBeamConnectedRack, SideViewText);
            dc.Pop();
		}
		/// <summary>
		/// Draw rack front view in (0, 0) point.
		/// If you want to draw rack in other point then apply RotateTransform and TranslateTransform to DrawingContext before call this method.
		/// </summary>
		public static void DrawRackFrontView(DrawingContext dc, ICoordinateSystem cs, RackAdvancedDrawingSettings displaySettings, Rack rack, FormattedText viewNameText)
		{
			if (dc == null)
				return;
			if (cs == null)
				return;
			if (displaySettings == null)
				return;
			if (rack == null)
				return;

			// camera scale and offset are used for calculate screen(control) point based on global drawing point
			double defaultCameraScale = 1.0;
			Vector defaultCameraOffset = new Vector(0.0, 0.0);

			double rackLength = rack.Length;
			double rackDepth = rack.Depth;
			double rackHeight = rack.Length_Z;

			Point startGlobalPnt = new Point(0.0, 0.0);//zeroScreenPoint;

			// Need to clone brushes and pens, otherwise displaySetting's brushes and pens will be modified.
			// It is a problem for sheet elevation picture, because all racks will have border of the last exported rack.
			Brush columnFillBrush = null;
			if (displaySettings.ColumnFillBrush != null)
				columnFillBrush = displaySettings.ColumnFillBrush.Clone();
			Pen columnBorderPen = null;
			if (displaySettings.ColumnBorderPen != null)
				columnBorderPen = displaySettings.ColumnBorderPen.Clone();
			Brush levelShelfFillBrush = null;
			if (displaySettings.LevelShelfBrush != null)
				levelShelfFillBrush = displaySettings.LevelShelfBrush.Clone();
			Pen levelShelfBorderPen = null;
			if (displaySettings.LevelShelfPen != null)
				levelShelfBorderPen = displaySettings.LevelShelfPen.Clone();
			Brush palletFillBrush = null;
			if (displaySettings.PalletFillBrush != null)
				palletFillBrush = displaySettings.PalletFillBrush.Clone();
			Pen palletBorderPen = null;
			if (displaySettings.PalletBorderPen != null)
				palletBorderPen = displaySettings.PalletBorderPen.Clone();
			Brush palletRiserFillBrush = null;
			if (displaySettings.PalletRiserFillBrush != null)
				palletRiserFillBrush = displaySettings.PalletRiserFillBrush.Clone();
			Pen palletRiserBorderPen = null;
			if (displaySettings.PalletRiserBorderPen != null)
				palletRiserBorderPen = displaySettings.PalletRiserBorderPen.Clone();
			Brush bracingLineFillBrush = null;
			if (displaySettings.BracingLineBrush != null)
				bracingLineFillBrush = displaySettings.BracingLineBrush.Clone();
			Pen bracingLineBorderPen = null;
			if (displaySettings.BracingLinePen != null)
				bracingLineBorderPen = displaySettings.BracingLinePen.Clone();
			if (displaySettings.IsItSheetElevationPicture)
			{
				Brush rackFillBrush = new SolidColorBrush(rack.FillColor);
				columnFillBrush = rackFillBrush;
				columnBorderPen.Brush = rackFillBrush;
				levelShelfFillBrush = rackFillBrush;
				levelShelfBorderPen.Brush = rackFillBrush;
				palletFillBrush = null;
				palletBorderPen.Brush = rackFillBrush;
				palletRiserFillBrush = null;
				palletRiserBorderPen.Brush = rackFillBrush;
				bracingLineFillBrush = null;
				bracingLineBorderPen.Brush = rackFillBrush;
			}

			// draw columns
			// front view left column
			Point leftColumnStart_GlobalPnt = startGlobalPnt;
			Point leftColumnEnd_GlobalPoint = leftColumnStart_GlobalPnt;
			leftColumnEnd_GlobalPoint.Y -= rackHeight;
			// left column should be displayed only for the first rack in the row\column
			if (rack.IsFirstInRowColumn)
			{
				leftColumnEnd_GlobalPoint.X += rack.DiffBetween_M_and_A;
				_DrawRectangle(dc, columnFillBrush, columnBorderPen, leftColumnStart_GlobalPnt, leftColumnEnd_GlobalPoint, cs);

				// draw tie beam frame, it is equal to max material height + 500
				if (rack.TieBeamFrame.HasFlag(eTieBeamFrame.eStartFrame))
				{
					Point leftFrameStart_GlobalPoint = leftColumnStart_GlobalPnt;
					leftFrameStart_GlobalPoint.Y = leftColumnEnd_GlobalPoint.Y;
					Point leftFrameEnd_GlobalPoint = leftFrameStart_GlobalPoint;
					leftFrameEnd_GlobalPoint.X = leftColumnEnd_GlobalPoint.X;
					leftFrameEnd_GlobalPoint.Y -= rack.FrameHeight - rackHeight;

					_DrawRectangle(dc, columnFillBrush, columnBorderPen, leftFrameStart_GlobalPoint, leftFrameEnd_GlobalPoint, cs);
				}
			}
			// front view right column
			Point rightColumnStart_GlobalPoint = leftColumnStart_GlobalPnt;
			rightColumnStart_GlobalPoint.X += rackLength;
			Point rightColumnEnd_GlobalPoint = rightColumnStart_GlobalPoint;
			rightColumnEnd_GlobalPoint.X -= rack.DiffBetween_M_and_A;
			rightColumnEnd_GlobalPoint.Y -= rackHeight;
			_DrawRectangle(dc, columnFillBrush, columnBorderPen, rightColumnStart_GlobalPoint, rightColumnEnd_GlobalPoint, cs);

			// draw tie beam frame, it is equal to max material height + 500
			if (rack.TieBeamFrame.HasFlag(eTieBeamFrame.eEndFrame))
			{
				Point rightFrameStart_GlobalPoint = rightColumnStart_GlobalPoint;
				rightFrameStart_GlobalPoint.Y = rightColumnEnd_GlobalPoint.Y;
				Point rightFrameEnd_GlobalPoint = rightFrameStart_GlobalPoint;
				rightFrameEnd_GlobalPoint.X = rightColumnEnd_GlobalPoint.X;
				rightFrameEnd_GlobalPoint.Y -= rack.FrameHeight - rackHeight;

				_DrawRectangle(dc, columnFillBrush, columnBorderPen, rightFrameStart_GlobalPoint, rightFrameEnd_GlobalPoint, cs);
			}

			if (displaySettings.DisplayTextAndDimensions)
			{
				// draw floor line
				Point bottomLineStart_GlobalPoint = leftColumnStart_GlobalPnt;
				bottomLineStart_GlobalPoint.X -= 100;
				Point bottomLineEnd_GlobalPoint = rightColumnStart_GlobalPoint;
				bottomLineEnd_GlobalPoint.X += 100;
				bottomLineEnd_GlobalPoint.Y += 10;
				_DrawRectangle(dc, displaySettings.BottomLineBrush, displaySettings.BottomLinePen, bottomLineStart_GlobalPoint, bottomLineEnd_GlobalPoint, cs);

				//// draw column length dimension
				//if (rack.IsFirstInRowColumn)
				//{
				//	string strColumnLength = rack.Column.Length.ToString();
				//	Point dimGlobalPnt_02 = leftColumnStart_GlobalPnt;
				//	dimGlobalPnt_02.X = leftColumnEnd_GlobalPoint.X;
				//	_DrawDimension(dc, leftColumnStart_GlobalPnt, dimGlobalPnt_02, strColumnLength, displaySettings.MinDimensionsLinesOffset / 2, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffset, eDimensionPlacement.eBot, drawingSize, correctedImageSize, cs);
				//}

				// draw inner length dimension
				string strBeamLength = rack.InnerLength.ToString();
				Point dimensionGlobalPnt_01 = leftColumnEnd_GlobalPoint;
				Point dimensionGlobalPnt_02 = rightColumnEnd_GlobalPoint;
				//double supportLinesOffset = cs.GetHeightInPixels(scale * (Math.Max(Math.Abs(rack.MaterialHeight - rack.FrameHeight), Math.Abs(rack.Length_Z - rack.FrameHeight))), 1.0);
				double supportLinesOffset = cs.GetHeightInPixels(Math.Max(Math.Abs(rack.MaterialHeight - rack.FrameHeight), Math.Abs(rack.Length_Z - rack.FrameHeight)), 1.0);
				_DrawDimension(dc, dimensionGlobalPnt_01, dimensionGlobalPnt_02, strBeamLength, supportLinesOffset + displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eTop, cs, dimensionBrush: displaySettings.DimensionsBrush);
				// draw rack length dimension
				string strRackLength = rack.Length.ToString();
				dimensionGlobalPnt_01 = leftColumnStart_GlobalPnt;
				dimensionGlobalPnt_01.Y = leftColumnEnd_GlobalPoint.Y;
				dimensionGlobalPnt_02 = dimensionGlobalPnt_01;
				dimensionGlobalPnt_02.X = rightColumnStart_GlobalPoint.X;
				_DrawDimension(dc, dimensionGlobalPnt_01, dimensionGlobalPnt_02, strRackLength, supportLinesOffset + 2 * displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eTop, cs, dimensionBrush: displaySettings.DimensionsBrush);

				// draw max loading height dimension
				string strMaxLoadingHeight = "Max Loading Height = " + rack.MaxLoadingHeight.ToString();
				dimensionGlobalPnt_01 = rightColumnStart_GlobalPoint;
				dimensionGlobalPnt_02 = dimensionGlobalPnt_01;
				dimensionGlobalPnt_02.Y -= rack.MaxLoadingHeight;
				_DrawDimension(dc, dimensionGlobalPnt_01, dimensionGlobalPnt_02, strMaxLoadingHeight, displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eRight, cs, dimensionBrush: displaySettings.DimensionsBrush);
				// Display frame height only for the end column
				if (rack.TieBeamFrame.HasFlag(eTieBeamFrame.eEndFrame) && Utils.FGT(rack.FrameHeight, rack.MaterialHeight))
				{
					// draw max material height dimension
					string strMaxMaterialHeight = "Max Material Height = " + rack.MaterialHeight.ToString();
					dimensionGlobalPnt_01 = rightColumnStart_GlobalPoint;
					dimensionGlobalPnt_02 = dimensionGlobalPnt_01;
					dimensionGlobalPnt_02.Y -= rack.MaterialHeight;
					_DrawDimension(dc, dimensionGlobalPnt_01, dimensionGlobalPnt_02, strMaxMaterialHeight, 2.3 * displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eRight, cs, dimensionBrush: displaySettings.DimensionsBrush);
					// draw rack height dimension
					string strRackHeight = "Rack Height = " + rack.MaterialHeight.ToString() + " + TieBeam Extenstion";
					dimensionGlobalPnt_01 = rightColumnStart_GlobalPoint;
					dimensionGlobalPnt_02 = dimensionGlobalPnt_01;
					dimensionGlobalPnt_02.Y -= rack.FrameHeight;
					_DrawDimension(dc, dimensionGlobalPnt_01, dimensionGlobalPnt_02, strRackHeight, 3.6 * displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eRight, cs, dimensionBrush: displaySettings.DimensionsBrush);
				}
				else
				{
					// draw rack height dimension
					string strRackHeight = "Rack Height = " + rack.Length_Z.ToString();
					dimensionGlobalPnt_01 = rightColumnStart_GlobalPoint;
					dimensionGlobalPnt_02 = dimensionGlobalPnt_01;
					dimensionGlobalPnt_02.Y -= rack.Length_Z;
					_DrawDimension(dc, dimensionGlobalPnt_01, dimensionGlobalPnt_02, strRackHeight, 2.3 * displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eRight, cs, dimensionBrush: displaySettings.DimensionsBrush);
					// draw max material height dimension
					string strMaxMaterialHeight = "Max Material Height = " + rack.MaterialHeight.ToString();
					dimensionGlobalPnt_01 = rightColumnStart_GlobalPoint;
					dimensionGlobalPnt_02 = dimensionGlobalPnt_01;
					dimensionGlobalPnt_02.Y -= rack.MaterialHeight;
					_DrawDimension(dc, dimensionGlobalPnt_01, dimensionGlobalPnt_02, strMaxMaterialHeight, 3.6 * displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eRight, cs, dimensionBrush: displaySettings.DimensionsBrush);
				}
				// draw clear available height dimension
				string strClearAvailableHeight = "Clear Height = " + rack.RoofHeight.ToString();
				// need to limit roof height height
				double roofHeighteHeightValue = rack.RoofHeight;
				bool bBreakLine = false;
				if (Utils.FGT(roofHeighteHeightValue, m_ClearAvailableHeightLimitCoef * rack.MaxHeight))
				{
					roofHeighteHeightValue = m_ClearAvailableHeightLimitCoef * rack.MaxHeight;
					bBreakLine = true;
				}
				dimensionGlobalPnt_01 = rightColumnStart_GlobalPoint;
				dimensionGlobalPnt_02 = dimensionGlobalPnt_01;
				dimensionGlobalPnt_02.Y -= roofHeighteHeightValue;
				_DrawDimension(dc, dimensionGlobalPnt_01, dimensionGlobalPnt_02, strClearAvailableHeight, 4.9 * displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eRight, cs, bBreakLine, dimensionBrush: displaySettings.DimensionsBrush);
				// draw underpass dimension
				if (rack.IsUnderpassAvailable)
				{
					dimensionGlobalPnt_01 = leftColumnStart_GlobalPnt;
					dimensionGlobalPnt_02 = dimensionGlobalPnt_01;
					dimensionGlobalPnt_02.Y -= rack.Underpass;
					//
					_DrawDimension(dc, dimensionGlobalPnt_01, dimensionGlobalPnt_02, "Underpass = " + rack.Underpass.ToString(), displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eLeft, cs, dimensionBrush: displaySettings.DimensionsBrush);
				}

				// draw view name
				if (viewNameText != null)
				{
					Point FrontViewTextCenter_GlobalPoint = bottomLineEnd_GlobalPoint;
					FrontViewTextCenter_GlobalPoint.X -= (bottomLineEnd_GlobalPoint.X - bottomLineStart_GlobalPoint.X) / 2;
					//
					Point FrontViewTextCenter_ScreenPoint = cs.GetLocalPoint(FrontViewTextCenter_GlobalPoint, defaultCameraScale, defaultCameraOffset);
					// add more space for guards dimensions
					if (rack.Accessories.UprightGuard || rack.Accessories.RowGuard)
						FrontViewTextCenter_ScreenPoint.Y += viewNameText.Height;
					else
						FrontViewTextCenter_ScreenPoint.Y += viewNameText.Height / 2;
					dc.DrawText(viewNameText, FrontViewTextCenter_ScreenPoint);
				}
			}

			//draw levels
			double LevelOffset_Y = 0;
			if (rack.IsUnderpassAvailable)
				LevelOffset_Y = rack.Underpass;
			else if (rack.IsMaterialOnGround)
				LevelOffset_Y = 0;
			else
			{
				double firstLevelOffset = Rack.sFirstLevelOffset;
				if (rack.Levels != null)
				{
					RackLevel firstLevel = rack.Levels.FirstOrDefault(level => level != null && level.Index == 1);
					if (firstLevel != null && firstLevel.Beam != null)
						firstLevelOffset -= firstLevel.Beam.Height;
				}
				LevelOffset_Y = Utils.GetWholeNumber(firstLevelOffset);
			}

			//
			if (rack.Levels != null)
			{
				foreach (RackLevel level in rack.Levels)
				{
					if (level == null)
						continue;

					int _beamHeight = 0;
					if (level.Beam != null)
						_beamHeight = Utils.GetWholeNumber(level.Beam.Height);

					// level shelf
					Point LevelStart_GlobalPoint = leftColumnStart_GlobalPnt;
					LevelStart_GlobalPoint.X = leftColumnEnd_GlobalPoint.X;
					LevelStart_GlobalPoint.Y -= LevelOffset_Y;
					Point LevelEnd_GlobalPoint = leftColumnStart_GlobalPnt;
					LevelEnd_GlobalPoint.X += rackLength - rack.DiffBetween_M_and_A;
					if (level.Index != 0)
					{
						LevelEnd_GlobalPoint.Y -= LevelOffset_Y + _beamHeight;
						_DrawRectangle(dc, levelShelfFillBrush, levelShelfBorderPen, LevelStart_GlobalPoint, LevelEnd_GlobalPoint, cs);

						// draw "Decking plate" at the front view if it has along depth value.
						if (level.Accessories != null && level.Accessories.IsDeckPlateAvailable && level.Accessories.DeckPlateType == eDeckPlateType.eAlongDepth_UDL)
						{
							Point deckingPlateStartGlobalPnt = LevelEnd_GlobalPoint;
							deckingPlateStartGlobalPnt.X = LevelStart_GlobalPoint.X;
							deckingPlateStartGlobalPnt.Y += (_beamHeight / 2) / 2;
							Point deckingPlateEndGlobalPnt = deckingPlateStartGlobalPnt;
							deckingPlateEndGlobalPnt.X = LevelEnd_GlobalPoint.X;

							Point deckingPlateStartScreenPnt = cs.GetLocalPoint(deckingPlateStartGlobalPnt, defaultCameraScale, defaultCameraOffset);
							Point deckingPlateEndScreenPnt = cs.GetLocalPoint(deckingPlateEndGlobalPnt, defaultCameraScale, defaultCameraOffset);

							double deckPlateLineThickness = cs.GetHeightInPixels(_beamHeight / 3, 1.0);
							Pen deckingPlatePen = new Pen(displaySettings.DeckingPlateBrush, deckPlateLineThickness);
							deckingPlatePen.DashStyle = DashStyles.Dash;

							dc.DrawLine(deckingPlatePen, deckingPlateStartScreenPnt, deckingPlateEndScreenPnt);
						}
					}

					if (displaySettings.DisplayTextAndDimensions)
					{
						string[] accessoriesList = null;

						// print level index
						string strLevelName = "Level ";
						strLevelName += level.DisplayName;
						// Display beam name for each level except ground if beams are different.
						// Otherwise display beam name at the first(not ground) level.
						bool bDisplayBeamName = (rack.AreLevelsTheSame && level.Index == rack.Levels.Last().Index) || (!rack.AreLevelsTheSame && level.Index != 0);
						if (bDisplayBeamName)
						{
							if (level.Beam == null || level.Beam.Beam == null)
								strLevelName += "\n(Undefined beam)";
							else
								strLevelName += " (" + level.Beam.Beam.Name + ")";

                            if (level.Accessories != null)
								accessoriesList = level.GetAccessoriesDescription();
						}

						FormattedText LevelNameText = new FormattedText(strLevelName, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, m_TextTypeFace, displaySettings.LevelTextSize, displaySettings.TextBrush);
						if (bDisplayBeamName)
							LevelNameText.TextAlignment = TextAlignment.Right;
						else
							LevelNameText.TextAlignment = TextAlignment.Center;
						//
						Point LevelNameCenter_GlobalPoint = startGlobalPnt;
						//LevelNameCenter_GlobalPoint.X -= 1.5 * cs.GetGlobalWidth(displaySettings.MinDimensionsLinesOffset + displaySettings.LevelTextSize, 1.0);
						LevelNameCenter_GlobalPoint.Y -= LevelOffset_Y + _beamHeight;
						//
						Point LevelNameCenter_ScreenPoint = cs.GetLocalPoint(LevelNameCenter_GlobalPoint, defaultCameraScale, defaultCameraOffset);
						LevelNameCenter_ScreenPoint.X -= 2 * displaySettings.MinDimensionsLinesOffset + displaySettings.LevelTextSize;
						if (!bDisplayBeamName)
							LevelNameCenter_ScreenPoint.X -= LevelNameText.Width / 2;
						LevelNameCenter_ScreenPoint.Y -= LevelNameText.Height;
						//
						dc.DrawText(LevelNameText, LevelNameCenter_ScreenPoint);

						if (accessoriesList != null && accessoriesList.Length > 0)
                        {
							string levelAccessoriesText = string.Join(" | ", accessoriesList.OrderByDescending(x => x.Length));

							FormattedText levelAccesoriesFormattedText = new FormattedText(levelAccessoriesText, CultureInfo.CurrentCulture, 
								FlowDirection.LeftToRight, m_TextTypeFace, displaySettings.LevelTextSize * 0.8, displaySettings.TextBrush);

							levelAccesoriesFormattedText.MaxTextWidth = 300;
                            levelAccesoriesFormattedText.MaxTextHeight = level.LevelHeight;

							// place accesories list under name
							LevelNameCenter_ScreenPoint.X -= levelAccesoriesFormattedText.Width;
                            LevelNameCenter_ScreenPoint.Y += LevelNameText.Extent;

                            dc.DrawText(levelAccesoriesFormattedText, LevelNameCenter_ScreenPoint);
						}

						// Draw level dimension from the top of current beam to the top of the next level beam.
						// Dont draw level dimension for the last level;
						if (level != rack.Levels.LastOrDefault())
						{
							Point dimPntBot = LevelEnd_GlobalPoint;
							dimPntBot.X = LevelStart_GlobalPoint.X;
							Point dimPntTop = dimPntBot;
							//
							double displayHeight = level.LevelHeight;
							// add next level beam height
							int iCurrentLevelIndex = rack.Levels.IndexOf(level);
							if (iCurrentLevelIndex <= rack.Levels.Count - 2)
							{
								int iNextLevelIndex = iCurrentLevelIndex + 1;
								RackLevel nextLevel = rack.Levels[iNextLevelIndex];
								if (nextLevel != null && nextLevel.Beam != null)
									displayHeight += nextLevel.Beam.Height;
							}
							dimPntTop.Y -= displayHeight;
							double offset = cs.GetWidthInPixels(LevelStart_GlobalPoint.X - leftColumnStart_GlobalPnt.X, 1.0);
							_DrawDimension(dc, dimPntBot, dimPntTop, displayHeight.ToString(), offset + displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eLeft, cs, dimensionBrush: displaySettings.DimensionsBrush);
						}
					}

					// display pallets
					if (rack.ShowPallet)
					{
						// draw pallets
						if (level.Pallets != null && level.Pallets.Count > 0)
						{
							double DistanceBetweenPallets = rack.BeamLength;
							foreach (Pallet _pallet in level.Pallets)
							{
								if (_pallet == null)
									continue;

								DistanceBetweenPallets -= _pallet.Length;
							}
							DistanceBetweenPallets /= (level.Pallets.Count + 1);

							bool bAddPalletRiser = false;
							if (level.Accessories != null && level.Accessories.ForkEntryBar)
								bAddPalletRiser = true;

							//
							double PalletOffset_X = DistanceBetweenPallets;
							foreach (Pallet _pallet in level.Pallets)
							{
								if (_pallet == null)
									continue;

								// draw pallet rect
								Point PalletStart_GlobalPoint = LevelStart_GlobalPoint;
								PalletStart_GlobalPoint.Y = LevelEnd_GlobalPoint.Y;
								if (bAddPalletRiser)
									PalletStart_GlobalPoint.Y -= Rack.PALLET_RISER_HEIGHT;
								PalletStart_GlobalPoint.X += PalletOffset_X + Rack.INNER_LENGTH_ADDITIONAL_GAP / 2;
								Point PalletEnd_GlobalPoint = PalletStart_GlobalPoint;
								PalletEnd_GlobalPoint.X += _pallet.Length;
								PalletEnd_GlobalPoint.Y -= _pallet.Height;
								//
								_DrawRectangle(dc, palletFillBrush, palletBorderPen, PalletStart_GlobalPoint, PalletEnd_GlobalPoint, cs);

								// draw pallet riser
								if (bAddPalletRiser)
								{
									// left
									Point RiserStart_GlobalPoint = PalletStart_GlobalPoint;
									RiserStart_GlobalPoint.X += Rack.PALLET_RISER_HEIGHT / 2;
									//
									Point RiserEnd_GlobalPoint = RiserStart_GlobalPoint;
									RiserEnd_GlobalPoint.X += Rack.PALLET_RISER_HEIGHT;
									RiserEnd_GlobalPoint.Y += Rack.PALLET_RISER_HEIGHT;
									//
									_DrawRectangle(dc, palletRiserFillBrush, palletRiserBorderPen, RiserStart_GlobalPoint, RiserEnd_GlobalPoint, cs);

									// right
									RiserStart_GlobalPoint.X = PalletEnd_GlobalPoint.X - Rack.PALLET_RISER_HEIGHT / 2;
									//
									RiserEnd_GlobalPoint.X = RiserStart_GlobalPoint.X - Rack.PALLET_RISER_HEIGHT;
									//
									_DrawRectangle(dc, palletRiserFillBrush, palletRiserBorderPen, RiserStart_GlobalPoint, RiserEnd_GlobalPoint, cs);
								}

								if (displaySettings.DisplayTextAndDimensions)
								{
									// draw pallet load
									string strPalletLoad = _pallet.Load.ToString();
									strPalletLoad += " Kg";
									FormattedText PalletLoadText = new FormattedText(strPalletLoad, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, m_TextTypeFace, displaySettings.LevelLoadTextSize, displaySettings.TextBrush);
									Point PalletEnd_ScreenPoint = cs.GetLocalPoint(PalletEnd_GlobalPoint, defaultCameraScale, defaultCameraOffset);
									Point PalletStart_ScreenPoint = cs.GetLocalPoint(PalletStart_GlobalPoint, defaultCameraScale, defaultCameraOffset);
									PalletLoadText.MaxTextWidth = Math.Abs(PalletEnd_ScreenPoint.X - PalletStart_ScreenPoint.X);
									PalletLoadText.MaxTextHeight = Math.Abs(PalletEnd_ScreenPoint.Y - PalletStart_ScreenPoint.Y);
									PalletLoadText.TextAlignment = TextAlignment.Center;
									//
									Point PalletLoadTextCenter_GlobalPoint = PalletStart_GlobalPoint;
									PalletLoadTextCenter_GlobalPoint.Y -= _pallet.Height / 2;
									Point PalletLoadTextCenter_ScreenPoint = cs.GetLocalPoint(PalletLoadTextCenter_GlobalPoint, defaultCameraScale, defaultCameraOffset);
									PalletLoadTextCenter_ScreenPoint.Y -= PalletLoadText.Height / 2;
									//
									dc.DrawText(PalletLoadText, PalletLoadTextCenter_ScreenPoint);
								}

								PalletOffset_X += _pallet.Length + DistanceBetweenPallets;
							}
						}
					}
					else
					{
						if (displaySettings.DisplayTextAndDimensions)
						{
							// just draw level load
							string strLevelLoad = level.LevelLoad.ToString();
							strLevelLoad += " Kg";
							FormattedText LevelLoadText = new FormattedText(strLevelLoad, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, m_TextTypeFace, displaySettings.LevelLoadTextSize, displaySettings.TextBrush);
							LevelLoadText.TextAlignment = TextAlignment.Center;
							//
							Point LevelLoadCenter_GlobalPoint = LevelStart_GlobalPoint;
							LevelLoadCenter_GlobalPoint.Y = LevelEnd_GlobalPoint.Y - 10;
							LevelLoadCenter_GlobalPoint.X += (LevelEnd_GlobalPoint.X - LevelStart_GlobalPoint.X) / 2;
							//
							Point LevelLoadCenter_ScreenPoint = cs.GetLocalPoint(LevelLoadCenter_GlobalPoint, defaultCameraScale, defaultCameraOffset);
							LevelLoadCenter_ScreenPoint.Y -= LevelLoadText.Height;
							//
							dc.DrawText(LevelLoadText, LevelLoadCenter_ScreenPoint);
						}
					}

					if (level.Index != 0)
						LevelOffset_Y += _beamHeight;
					LevelOffset_Y += level.LevelHeight;
				}
			}

			bool isGuardHeighShown = false;

			if (rack.Accessories.UprightGuard)
			{
				_TryDrawFrontColumnGuard(dc, cs, displaySettings, rack, out isGuardHeighShown);
			}

			if (rack.Accessories.RowGuard)
			{
				_TryDrawFrontRowGuard(dc, cs, displaySettings, rack, isGuardHeighShown);
			}
		}

        /// <summary>
        /// Draws rack side view at (0.0, 0.0) point.
        /// If rack has overhang pallets then rack's left column will be placed at (pallet_overhang_value, 0.0) point.
        /// </summary>
        public static void DrawRackSideView(DrawingContext dc, ICoordinateSystem cs, RackAdvancedDrawingSettings displaySettings, Rack rack, FormattedText viewNameText)
		{
			if (dc == null)
				return;
			if (cs == null)
				return;
			if (displaySettings == null)
				return;
			if (rack == null)
				return;

			double rackLength = rack.Length;
			double rackDepth = rack.Depth;
			double rackHeight = rack.Length_Z;

			// camera scale and offset are used for calculate screen(control) point based on global drawing point
			double defaultCameraScale = 1.0;
			Vector defaultCameraOffset = new Vector(0.0, 0.0);

			Point startGlobalPnt = new Point(0.0, 0.0);

			// Need to clone brushes and pens, otherwise displaySetting's brushes and pens will be modified.
			// It is a problem for sheet elevation picture, because all racks will have border of the last exported rack.
			Brush columnFillBrush = null;
			if (displaySettings.ColumnFillBrush != null)
				columnFillBrush = displaySettings.ColumnFillBrush.Clone();
			Pen columnBorderPen = null;
			if (displaySettings.ColumnBorderPen != null)
				columnBorderPen = displaySettings.ColumnBorderPen.Clone();
			Brush levelShelfFillBrush = null;
			if (displaySettings.LevelShelfBrush != null)
				levelShelfFillBrush = displaySettings.LevelShelfBrush.Clone();
			Pen levelShelfBorderPen = null;
			if (displaySettings.LevelShelfPen != null)
				levelShelfBorderPen = displaySettings.LevelShelfPen.Clone();
			Brush palletFillBrush = null;
			if (displaySettings.PalletFillBrush != null)
				palletFillBrush = displaySettings.PalletFillBrush.Clone();
			Pen palletBorderPen = null;
			if (displaySettings.PalletBorderPen != null)
				palletBorderPen = displaySettings.PalletBorderPen.Clone();
			Brush palletRiserFillBrush = null;
			if (displaySettings.PalletRiserFillBrush != null)
				palletRiserFillBrush = displaySettings.PalletRiserFillBrush.Clone();
			Pen palletRiserBorderPen = null;
			if (displaySettings.PalletRiserBorderPen != null)
				palletRiserBorderPen = displaySettings.PalletRiserBorderPen.Clone();
			Brush bracingLineFillBrush = null;
			if (displaySettings.BracingLineBrush != null)
				bracingLineFillBrush = displaySettings.BracingLineBrush.Clone();
			Pen bracingLineBorderPen = null;
			if (displaySettings.BracingLinePen != null)
				bracingLineBorderPen = displaySettings.BracingLinePen.Clone();
			if (displaySettings.IsItSheetElevationPicture)
			{
				Brush rackFillBrush = new SolidColorBrush(rack.FillColor);
				columnFillBrush = rackFillBrush;
				columnBorderPen.Brush = rackFillBrush;
				levelShelfFillBrush = rackFillBrush;
				levelShelfBorderPen.Brush = rackFillBrush;
				palletFillBrush = null;
				palletBorderPen.Brush = rackFillBrush;
				palletRiserFillBrush = null;
				palletRiserBorderPen.Brush = rackFillBrush;
				bracingLineFillBrush = null;
				bracingLineBorderPen.Brush = rackFillBrush;
			}

			// calculate columns points, but draw it later - columns should overlay pallets
			Point leftColumnStart_GlobalPnt = startGlobalPnt;
			leftColumnStart_GlobalPnt.X += rack.PalletOverhangValue;
			//
			Point leftColumnEnd_GlobalPoint = leftColumnStart_GlobalPnt;
			leftColumnEnd_GlobalPoint.X += rack.Column.Depth ;
			leftColumnEnd_GlobalPoint.Y -= rackHeight;
			//
			Point rightColumnEnd_GlobalPoint = leftColumnStart_GlobalPnt;
			rightColumnEnd_GlobalPoint.X += rackDepth;
			//
			Point rightColumnStart_GlobalPoint = rightColumnEnd_GlobalPoint;
			rightColumnStart_GlobalPoint.X -= rack.Column.Depth;
			rightColumnStart_GlobalPoint.Y -= rackHeight;

			//// draw column width dimension
			//string strColumnWidth = rack.Column.Depth.ToString();
			//dimensionGlobalPnt_02 = leftColumnStart_GlobalPnt;
			//dimensionGlobalPnt_02.X = leftColumnEnd_GlobalPoint.X;
			//_DrawDimension(dc, leftColumnStart_GlobalPnt, dimensionGlobalPnt_02, strColumnWidth, displaySettings.MinDimensionsLinesOffset / 2, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffset, eDimensionPlacement.eBot, drawingSize, imageSize, cs);

			// draw levels
			double LevelOffset_Y = 0;
			if (rack.IsUnderpassAvailable)
				LevelOffset_Y = rack.Underpass;
			else if (rack.IsMaterialOnGround)
				LevelOffset_Y = 0;
			else
			{
				double firstLevelOffset = Rack.sFirstLevelOffset;
				if (rack.Levels != null)
				{
					RackLevel firstLevel = rack.Levels.FirstOrDefault(level => level != null && level.Index == 1);
					if (firstLevel != null && firstLevel.Beam != null)
						firstLevelOffset -= firstLevel.Beam.Height;
				}
				LevelOffset_Y = Utils.GetWholeNumber(firstLevelOffset);
			}
			//
			if (rack.Levels != null)
			{
				//
				Point _ZeroLevel_LeftGlobalPoint = leftColumnEnd_GlobalPoint;
				_ZeroLevel_LeftGlobalPoint.Y = leftColumnStart_GlobalPnt.Y;
				Point _ZeroLevel_RightGlobalPoint = rightColumnStart_GlobalPoint;
				_ZeroLevel_RightGlobalPoint.Y = leftColumnStart_GlobalPnt.Y;
				//
				foreach (RackLevel level in rack.Levels)
				{
					if (level == null)
						continue;

					int _beamHeight = 0;
					if (level.Beam != null)
						_beamHeight = Utils.GetWholeNumber(level.Beam.Height);

					// level shelf
					Point LevelStart_GlobalPoint = _ZeroLevel_LeftGlobalPoint;
					LevelStart_GlobalPoint.Y -= LevelOffset_Y;
					//
					Point LevelEnd_GlobalPoint = _ZeroLevel_RightGlobalPoint;
					if (level.Index != 0)
					{
						LevelEnd_GlobalPoint.Y -= LevelOffset_Y + _beamHeight;
						// dont draw level shelf at the side view
						//dc.DrawRectangle(m_LevelShelfFillBrush, m_LevelShelfBorderPen, new Rect(LevelStart_ScreenPoint, LevelEnd_ScreenPoint));
					}

					// draw "Decking plate" at the side view if it has along length value.
					if (level.Accessories != null && level.Accessories.IsDeckPlateAvailable && level.Accessories.DeckPlateType == eDeckPlateType.eAlongLength)
					{
						Point deckingPlateStartGlobalPnt = LevelEnd_GlobalPoint;
						deckingPlateStartGlobalPnt.X = LevelStart_GlobalPoint.X;
						deckingPlateStartGlobalPnt.Y += (_beamHeight / 2) / 2;
						Point deckingPlateEndGlobalPnt = deckingPlateStartGlobalPnt;
						deckingPlateEndGlobalPnt.X = LevelEnd_GlobalPoint.X;

						Point deckingPlateStartScreenPnt = cs.GetLocalPoint(deckingPlateStartGlobalPnt, defaultCameraScale, defaultCameraOffset);
						Point deckingPlateEndScreenPnt = cs.GetLocalPoint(deckingPlateEndGlobalPnt, defaultCameraScale, defaultCameraOffset);

						double deckPlateLineThickness = cs.GetHeightInPixels(_beamHeight / 3, 1.0);
						Pen deckingPlatePen = new Pen(displaySettings.DeckingPlateBrush, deckPlateLineThickness);
						deckingPlatePen.DashStyle = DashStyles.Dash;

						dc.DrawLine(deckingPlatePen, deckingPlateStartScreenPnt, deckingPlateEndScreenPnt);
					}

					bool bAddPalletRiser = false;
					if (level.Accessories != null && level.Accessories.ForkEntryBar)
						bAddPalletRiser = true;

					//
					if (rack.ShowPallet)
					{
						// draw pallets
						if (level.Pallets != null && level.Pallets.Count > 0)
						{
							// draw from the last pallet to the first
							for (int iPalletIndex = level.Pallets.Count - 1; iPalletIndex >= 0; --iPalletIndex)
							{
								Pallet pallet = level.Pallets[iPalletIndex];
								if (pallet == null)
									continue;

								// draw pallet
								Point PalletCenter_GlobalPoint = LevelStart_GlobalPoint;
								PalletCenter_GlobalPoint.Y = LevelEnd_GlobalPoint.Y;
								if (bAddPalletRiser)
									PalletCenter_GlobalPoint.Y -= Rack.PALLET_RISER_HEIGHT;
								PalletCenter_GlobalPoint.X += (LevelEnd_GlobalPoint.X - LevelStart_GlobalPoint.X) / 2;
								//
								Point PalletStart_GlobalPoint = PalletCenter_GlobalPoint;
								PalletStart_GlobalPoint.X -= pallet.Width / 2;
								//
								Point PalletEnd_GlobalPoint = PalletCenter_GlobalPoint;
								PalletEnd_GlobalPoint.X += pallet.Width / 2;
								PalletEnd_GlobalPoint.Y -= pallet.Height;
								//
								_DrawRectangle(dc, palletFillBrush, palletBorderPen, PalletStart_GlobalPoint, PalletEnd_GlobalPoint, cs);

								// draw pallet riser
								if (bAddPalletRiser)
								{
									Point RiserStart_GlobalPoint = PalletStart_GlobalPoint;
									RiserStart_GlobalPoint.X = leftColumnStart_GlobalPnt.X;
									//
									Point RiserEnd_GlobalPoint = RiserStart_GlobalPoint;
									RiserEnd_GlobalPoint.X = rightColumnEnd_GlobalPoint.X;
									RiserEnd_GlobalPoint.Y += Rack.PALLET_RISER_HEIGHT;
									//
									_DrawRectangle(dc, palletRiserFillBrush, palletRiserBorderPen, RiserStart_GlobalPoint, RiserEnd_GlobalPoint, cs);
								}
							}
						}
					}

					if (level.Index != 0)
						LevelOffset_Y += _beamHeight;
					LevelOffset_Y += level.LevelHeight;
				}
			}

			double dimensionsLinesOffset = cs.GetWidthInPixels(rack.PalletOverhangValue, 1.0);
			// draw bracing lines
			if (rack.Bracing != eColumnBracingType.eUndefined)
			{
				double heightOffset = Rack.sBracingLinesBottomOffset;
				Vector heightDirection = new Vector(0, -1);
				Point startPoint = new Point(leftColumnEnd_GlobalPoint.X, leftColumnStart_GlobalPnt.Y);
				Point endPoint = new Point(rightColumnStart_GlobalPoint.X, leftColumnStart_GlobalPnt.Y);
				// draw horizontal bracing
				Point lineStart_GlobalPoint = startPoint + heightOffset * heightDirection;
				Point lineEnd_GlobalPoint = endPoint + heightOffset * heightDirection;
				_DrawBracingLine(dc, cs, lineStart_GlobalPoint, lineEnd_GlobalPoint, bracingLineBorderPen);

				// draw X bracings
				if (rack.Bracing == eColumnBracingType.eXBracing || rack.Bracing == eColumnBracingType.eXBracingWithStiffener)
				{
					int xBracingsCount = rack.X_Bracings_Count;
					if (xBracingsCount > 0)
					{
						heightOffset += Rack.sXBracingVerticalOffset;

						for (int i = 1; i <= xBracingsCount; ++i)
						{
							Point xBracingStartPnt = startPoint + heightOffset * heightDirection;
							Point xBracingEndPnt = endPoint + (heightOffset + Rack.sBracingVerticalStep) * heightDirection;
							_DrawBracingLine(dc, cs, xBracingStartPnt, xBracingEndPnt, bracingLineBorderPen);

							double xBracingStartPnt_Y = xBracingStartPnt.Y;
							xBracingStartPnt.Y = xBracingEndPnt.Y;
							xBracingEndPnt.Y = xBracingStartPnt_Y;
							_DrawBracingLine(dc, cs, xBracingStartPnt, xBracingEndPnt, bracingLineBorderPen);

							heightOffset += Rack.sBracingVerticalStep;
						}

						heightOffset += Rack.sXBracingVerticalOffset;

						// draw horizontal bracing
						lineStart_GlobalPoint = startPoint + heightOffset * heightDirection;
						lineEnd_GlobalPoint = endPoint + heightOffset * heightDirection;
						_DrawBracingLine(dc, cs, lineStart_GlobalPoint, lineEnd_GlobalPoint, bracingLineBorderPen);
					}
				}

				// draw normal bracings
				double rightFrameHeight = rack.Length_Z;
				if (rack.TieBeamFrame.HasFlag(eTieBeamFrame.eEndFrame))
					rightFrameHeight = rack.FrameHeight;
				//
				int iTotalLines = (int)Math.Floor((Utils.GetWholeNumber(rightFrameHeight) - heightOffset - Rack.sTopHorizontalBracingOffset) / Rack.sBracingVerticalStep);
				for (int i = 1; i <= iTotalLines; ++i)
				{
					//
					if (i % 2 != 0)
					{
						lineStart_GlobalPoint = startPoint + heightOffset * heightDirection;
						lineEnd_GlobalPoint = endPoint + (heightOffset + Rack.sBracingVerticalStep) * heightDirection;
					}
					else
					{
						lineStart_GlobalPoint = endPoint + heightOffset * heightDirection;
						lineEnd_GlobalPoint = startPoint + (heightOffset + Rack.sBracingVerticalStep) * heightDirection;
					}

					//
					_DrawBracingLine(dc, cs, lineStart_GlobalPoint, lineEnd_GlobalPoint, bracingLineBorderPen);

					// draw horizontal lines
					if (i == iTotalLines)
					{
						Point vertLineEnd_GlobalPoint = lineEnd_GlobalPoint;
						if (i % 2 != 0)
							vertLineEnd_GlobalPoint.X = leftColumnEnd_GlobalPoint.X;
						else
							vertLineEnd_GlobalPoint.X = rightColumnStart_GlobalPoint.X;
						_DrawBracingLine(dc, cs, lineEnd_GlobalPoint, vertLineEnd_GlobalPoint, bracingLineBorderPen);
					}

					heightOffset += Rack.sBracingVerticalStep;
				}

				// if there is more than sTopHorizontalBracingMinDistance then show additional line
				if (iTotalLines > 0)
				{
					double topHeightRemainder = rightFrameHeight - heightOffset;
					if (Utils.FGE(topHeightRemainder, Rack.sTopHorizontalBracingMinDistance))
					{
						Point vertLine_StartPoint = leftColumnEnd_GlobalPoint;
						vertLine_StartPoint.Y = leftColumnStart_GlobalPnt.Y - (rightFrameHeight - Rack.sTopHorizontalBracingOffset);
						Point vertLine_EndPoint = rightColumnStart_GlobalPoint;
						vertLine_EndPoint.Y = vertLine_StartPoint.Y;
						_DrawBracingLine(dc, cs, vertLine_StartPoint, vertLine_EndPoint, bracingLineBorderPen);
					}
				}

				if (displaySettings.DisplayTextAndDimensions)
				{
					// draw bracing dimensions only once
					// draw bracing vertical step
					Point dimGlobalPnt_01 = startPoint + Rack.sBracingLinesBottomOffset * heightDirection;
					dimGlobalPnt_01.X = rightColumnEnd_GlobalPoint.X;
					if (rack.Bracing == eColumnBracingType.eXBracing || rack.Bracing == eColumnBracingType.eXBracingWithStiffener)
						dimGlobalPnt_01 += Rack.sXBracingVerticalOffset * heightDirection;
					//
					Point dimGlobalPnt_02 = dimGlobalPnt_01;
					dimGlobalPnt_02 += Rack.sBracingVerticalStep * heightDirection;
					_DrawDimension(dc, dimGlobalPnt_01, dimGlobalPnt_02, Rack.sBracingVerticalStep.ToString(), dimensionsLinesOffset + displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eRight, cs, dimensionBrush: displaySettings.DimensionsBrush);

					// draw X bracing height
					if (eColumnBracingType.eXBracing == rack.Bracing || eColumnBracingType.eXBracingWithStiffener == rack.Bracing)
					{
						dimGlobalPnt_01 = rightColumnStart_GlobalPoint;
						dimGlobalPnt_01.Y = rightColumnEnd_GlobalPoint.Y;
						dimGlobalPnt_02 = dimGlobalPnt_01;
						dimGlobalPnt_02 += rack.X_Bracing_Height * heightDirection;
						_DrawDimension(dc, dimGlobalPnt_01, dimGlobalPnt_02, rack.X_Bracing_Height.ToString(), dimensionsLinesOffset + 3 * displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eRight, cs, dimensionBrush: displaySettings.DimensionsBrush);
					}
				}
			}

			// draw columns
			_DrawRectangle(dc, columnFillBrush, columnBorderPen, leftColumnStart_GlobalPnt, leftColumnEnd_GlobalPoint, cs);
			_DrawRectangle(dc, columnFillBrush, columnBorderPen, rightColumnStart_GlobalPoint, rightColumnEnd_GlobalPoint, cs);

			// draw tie beam frame, it is equal to max material height + 500
			if (rack.TieBeamFrame.HasFlag(eTieBeamFrame.eEndFrame))
			{
				// left column
				Point leftFrameStart_GlobalPoint = leftColumnStart_GlobalPnt;
				leftFrameStart_GlobalPoint.Y = leftColumnEnd_GlobalPoint.Y;
				Point leftFrameEnd_GlobalPoint = leftFrameStart_GlobalPoint;
				leftFrameEnd_GlobalPoint.X = leftColumnEnd_GlobalPoint.X;
				leftFrameEnd_GlobalPoint.Y -= rack.FrameHeight - rackHeight;
				//
				_DrawRectangle(dc, columnFillBrush, columnBorderPen, leftFrameStart_GlobalPoint, leftFrameEnd_GlobalPoint, cs);

				// right column
				Point rightFrameStart_GlobalPoint = rightColumnStart_GlobalPoint;
				rightFrameStart_GlobalPoint.Y = leftFrameStart_GlobalPoint.Y;
				Point rightFrameEnd_GlobalPoint = leftFrameEnd_GlobalPoint;
				rightFrameEnd_GlobalPoint.X = rightColumnEnd_GlobalPoint.X;
				//
				_DrawRectangle(dc, columnFillBrush, columnBorderPen, rightFrameStart_GlobalPoint, rightFrameEnd_GlobalPoint, cs);
			}

			if (displaySettings.DisplayTextAndDimensions)
			{
				//double supportLinesOffset = cs.GetHeightInPixels(scale * (Math.Max(Math.Abs(rack.MaterialHeight - rack.FrameHeight), Math.Abs(rack.Length_Z - rack.FrameHeight))), 1.0);
				double supportLinesOffset = cs.GetHeightInPixels(Math.Max(Math.Abs(rack.MaterialHeight - rack.FrameHeight), Math.Abs(rack.Length_Z - rack.FrameHeight)), 1.0);
				// draw rack width dimension
				string strRackWidth = rack.Depth.ToString();
				Point dimensionGlobalPnt_01 = leftColumnStart_GlobalPnt;
				dimensionGlobalPnt_01.Y = leftColumnEnd_GlobalPoint.Y;
				Point dimensionGlobalPnt_02 = dimensionGlobalPnt_01;
				dimensionGlobalPnt_02.X = rightColumnEnd_GlobalPoint.X;
				_DrawDimension(dc, dimensionGlobalPnt_01, dimensionGlobalPnt_02, strRackWidth, supportLinesOffset + displaySettings.MinDimensionsLinesOffset, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eTop, cs, dimensionBrush: displaySettings.DimensionsBrush);
			}

			if (displaySettings.DisplayTextAndDimensions)
			{
				// draw floor line
				Point bottomLineStart_GlobalPoint = leftColumnStart_GlobalPnt;
				bottomLineStart_GlobalPoint.X -= 200;
				//
				Point bottomLineEnd_GlobalPoint = rightColumnEnd_GlobalPoint;
				bottomLineEnd_GlobalPoint.X += 200;
				bottomLineEnd_GlobalPoint.Y = bottomLineStart_GlobalPoint.Y + 10;
				//
				_DrawRectangle(dc, displaySettings.BottomLineBrush, displaySettings.BottomLinePen, bottomLineStart_GlobalPoint, bottomLineEnd_GlobalPoint, cs);

				// draw view name
				if (viewNameText != null)
				{
					Point SideViewTextCenter_GlobalPoint = bottomLineEnd_GlobalPoint;
					SideViewTextCenter_GlobalPoint.X -= (bottomLineEnd_GlobalPoint.X - bottomLineStart_GlobalPoint.X) / 2;
					//
					Point SideViewTextCenter_ScreenPoint = cs.GetLocalPoint(SideViewTextCenter_GlobalPoint, defaultCameraScale, defaultCameraOffset);
					// add more space for guards dimensions
                    if (rack.Accessories.UprightGuard || rack.Accessories.RowGuard)
						SideViewTextCenter_ScreenPoint.Y += viewNameText.Height;
					else
						SideViewTextCenter_ScreenPoint.Y += viewNameText.Height / 2;
					dc.DrawText(viewNameText, SideViewTextCenter_ScreenPoint);
				}
			}

			bool isHeightDisplayed = false;

			if (rack.Accessories.UprightGuard)
			{
				_TryDrawSideColumnGuard(dc, cs, startGlobalPnt, displaySettings, rack, displaySettings.DisplayTextAndDimensions, out isHeightDisplayed);
			}

			if (rack.Accessories.RowGuard)
			{
				_TryDrawSideRowGuard(dc, cs, startGlobalPnt, displaySettings, rack, displaySettings.DisplayTextAndDimensions, showHeight: !isHeightDisplayed);
			}
		}




		public static void DrawRackExtendedSideView(DrawingContext dc, ICoordinateSystem cs, RackAdvancedDrawingSettings displaySettings, 
			Rack mainRack, Rack backRack, Rack tieBeamRack, 
			FormattedText viewNameText)
		{
			if (dc == null)
				return;
			if (cs == null)
				return;
			if (displaySettings == null)
				return;
			if (mainRack == null)
				return;

			// camera scale and offset are used for calculate screen(control) point based on global drawing point
			double defaultCameraScale = 1.0;
			Vector defaultCameraOffset = new Vector(0.0, 0.0);

			List<Rack> orderedToDraw = new List<Rack>(3) { mainRack };

            if (backRack != null)
				orderedToDraw.Add(backRack);

			if (tieBeamRack != null)
				orderedToDraw.Add(tieBeamRack);

            if (mainRack.IsHorizontal)
            {
				orderedToDraw = orderedToDraw
					.OrderBy(x => x.Center_GlobalPoint.Y)
					.ToList();
			}
			else
            {
				orderedToDraw = orderedToDraw
					.OrderBy(x => x.Center_GlobalPoint.X)
					.ToList();
			}

			bool showDimensionsLeft = false;

			Point drawingGlobalPnt = new Point(0.0, 0.0);

			double tieBeamRackOffsetX = 0.0;
			double backToBackRackOffsetX = 0.0;
			double mainRackOffsetX = 0.0;

			Rack lastDrawn = null;
			foreach (Rack toDraw in orderedToDraw)
            {
                RackDrawingBrushes currentBrushes = new RackDrawingBrushes(displaySettings, toDraw.FillColor);

				if (lastDrawn != null)
				{
					drawingGlobalPnt.X += toDraw.IsHorizontal
						? toDraw.BottomRight_GlobalPoint.Y - lastDrawn.BottomLeft_GlobalPoint.Y
						: toDraw.BottomRight_GlobalPoint.X - lastDrawn.BottomRight_GlobalPoint.X;
				}

				// if last rack is back to back need to swap dimensions position to left
				if (!showDimensionsLeft)
					showDimensionsLeft = lastDrawn != null && lastDrawn != backRack;

				if (toDraw == tieBeamRack)
					tieBeamRackOffsetX = drawingGlobalPnt.X;

				if (toDraw == backRack)
					backToBackRackOffsetX = drawingGlobalPnt.X;

				if (toDraw == mainRack)
					mainRackOffsetX = drawingGlobalPnt.X;

				DrawRack(dc, cs, defaultCameraScale, defaultCameraOffset, toDraw, displaySettings, currentBrushes, drawingGlobalPnt, toDraw == mainRack, showDimensionsLeft);
				lastDrawn = toDraw;
			}

			// Draw tie beam
            if (tieBeamRack != null)
            {
				Pen tieBeamPen = new Pen(new SolidColorBrush(Colors.Gray), 2.0);
				tieBeamPen.DashStyle = new DashStyle(new double[] { 3, 3 }, 0);

				Point firstTieBeamPoint = default(Point);
				Point secondTieBeamPoint = default(Point);

				if (tieBeamRackOffsetX < mainRackOffsetX)
				{
					firstTieBeamPoint = new Point(tieBeamRackOffsetX + tieBeamRack.Depth + tieBeamRack.PalletOverhangValue, -(tieBeamRack.FrameHeight));
					secondTieBeamPoint = new Point(mainRackOffsetX + mainRack.PalletOverhangValue, -(tieBeamRack.FrameHeight));
                }
                else
                {
					firstTieBeamPoint = new Point(mainRackOffsetX + mainRack.Depth + mainRack.PalletOverhangValue, -(tieBeamRack.FrameHeight));
					secondTieBeamPoint = new Point(tieBeamRackOffsetX + tieBeamRack.PalletOverhangValue, -(tieBeamRack.FrameHeight));
				}

				// pull tie beam down by 100mm
				firstTieBeamPoint.Y += 100;
				secondTieBeamPoint.Y += 100;

				_DrawBracingLine(dc, cs, firstTieBeamPoint, secondTieBeamPoint, tieBeamPen);

				// second tie beam line
				firstTieBeamPoint.Y += 50;
				secondTieBeamPoint.Y += 50;

				_DrawBracingLine(dc, cs, firstTieBeamPoint, secondTieBeamPoint, tieBeamPen);

                if (displaySettings.DisplayTextAndDimensions)
                {
					_DrawDimension(dc, firstTieBeamPoint, secondTieBeamPoint,
						$"{secondTieBeamPoint.X - firstTieBeamPoint.X}",
						displaySettings.MinDimensionsLinesOffset,
						displaySettings.DimensionsTextSize,
						displaySettings.PerpDimLinesOffsetInPixels,
						eDimensionPlacement.eTop, cs,
						dimensionBrush: displaySettings.DimensionsBrush);
				}
			}

			// Draw rows connectors
            if (backRack != null)
            {
				Brush rackConnectorBrush = new SolidColorBrush(Colors.Red);
				Pen rackConnectorPen = new Pen(rackConnectorBrush, 2.0);

				//Rack.BackToBackRackConnectorHeight
				Point startConnectorPoint = new Point();
				Point endConnectorPoint = new Point();

                if (backToBackRackOffsetX < mainRackOffsetX)
                {
					startConnectorPoint.X = backToBackRackOffsetX + backRack.Depth + backRack.PalletOverhangValue - backRack.Column.Depth;

					endConnectorPoint.X = mainRackOffsetX + mainRack.PalletOverhangValue + mainRack.Column.Depth;
					endConnectorPoint.Y = startConnectorPoint.Y - Rack.BackToBackRackConnectorHeight;
				}
				else
                {
					startConnectorPoint.X = mainRackOffsetX + mainRack.Depth + mainRack.PalletOverhangValue - mainRack.Column.Depth;

					endConnectorPoint.X = backToBackRackOffsetX + backRack.PalletOverhangValue + backRack.Column.Depth;
					endConnectorPoint.Y = startConnectorPoint.Y - Rack.BackToBackRackConnectorHeight;
				}

                for (int i = 2000; i < mainRack.Length_Z; i+=2000)
                {
					startConnectorPoint.Y = -i;
					endConnectorPoint.Y = startConnectorPoint.Y - Rack.BackToBackRackConnectorHeight;

					_DrawRectangle(dc, rackConnectorBrush, rackConnectorPen, startConnectorPoint, endConnectorPoint, cs);
				}
			}

            if (mainRack.StiffenersHeight > 0)
            {
				FormattedText fmtedText = new FormattedText(
					"Stiffener", 
					CultureInfo.CurrentCulture, 
					FlowDirection.LeftToRight,
					m_TextTypeFace,
					displaySettings.DimensionsTextSize,
					displaySettings.DimensionsBrush);

				Point stiffenerTextPoint = new Point(mainRackOffsetX, -mainRack.X_Bracing_Height);

				if (mainRackOffsetX < tieBeamRackOffsetX)
				{
					stiffenerTextPoint.X += mainRack.Depth + (2 * mainRack.PalletOverhangValue);
				}
				else
				{
					stiffenerTextPoint.X -= fmtedText.Width;
				}

				stiffenerTextPoint = cs.GetLocalPoint(stiffenerTextPoint, defaultCameraScale, defaultCameraOffset);
				stiffenerTextPoint.Y -= fmtedText.Height;

				dc.DrawText(fmtedText, stiffenerTextPoint);
            }

			if (displaySettings.DisplayTextAndDimensions)
			{                
				// draw floor line
                Point bottomLineStart_GlobalPoint = new Point(0.0, 0.0);
                bottomLineStart_GlobalPoint.X -= 200;
                //
                Point bottomLineEnd_GlobalPoint = drawingGlobalPnt;
				
				bottomLineEnd_GlobalPoint.X += orderedToDraw.Last().Depth + orderedToDraw.Last().PalletOverhangValue + 200;
                bottomLineEnd_GlobalPoint.Y = bottomLineStart_GlobalPoint.Y + 10;
                //
                _DrawRectangle(dc, displaySettings.BottomLineBrush, displaySettings.BottomLinePen, bottomLineStart_GlobalPoint, bottomLineEnd_GlobalPoint, cs);

                // draw view name
                if (viewNameText != null)
                {
                    Point SideViewTextCenter_GlobalPoint = bottomLineEnd_GlobalPoint;
                    SideViewTextCenter_GlobalPoint.X -= (bottomLineEnd_GlobalPoint.X - bottomLineStart_GlobalPoint.X) / 2;
                    //
                    Point SideViewTextCenter_ScreenPoint = cs.GetLocalPoint(SideViewTextCenter_GlobalPoint, defaultCameraScale, defaultCameraOffset);
                    // add more space for guards dimensions
                    if (mainRack.Accessories.UprightGuard || mainRack.Accessories.RowGuard)
                        SideViewTextCenter_ScreenPoint.Y += viewNameText.Height;
                    else
                        SideViewTextCenter_ScreenPoint.Y += viewNameText.Height / 2;
                    dc.DrawText(viewNameText, SideViewTextCenter_ScreenPoint);
                }
            }
		}

		private static void DrawRack(DrawingContext dc, ICoordinateSystem cs,
			double defaultCameraScale, Vector defaultCameraOffset,
			Rack rack,
			RackAdvancedDrawingSettings displaySettings,
			RackDrawingBrushes brushes,
			Point drawStartPoint,
			bool showDimensions,
			bool showDimensionsLeft = false)
        {
			double rackDepth = rack.Depth;
			double rackHeight = rack.Length_Z;

			// calculate columns points, but draw it later - columns should overlay pallets
			Point leftColumnStart_GlobalPnt = drawStartPoint;
			leftColumnStart_GlobalPnt.X += rack.PalletOverhangValue;
			//
			Point leftColumnEnd_GlobalPoint = leftColumnStart_GlobalPnt;
			leftColumnEnd_GlobalPoint.X += rack.Column.Depth;
			leftColumnEnd_GlobalPoint.Y -= rackHeight;
			//
			Point rightColumnEnd_GlobalPoint = leftColumnStart_GlobalPnt;
			rightColumnEnd_GlobalPoint.X += rackDepth;
			//
			Point rightColumnStart_GlobalPoint = rightColumnEnd_GlobalPoint;
			rightColumnStart_GlobalPoint.X -= rack.Column.Depth;
			rightColumnStart_GlobalPoint.Y -= rackHeight;

			// draw levels
			double LevelOffset_Y = 0;
			if (rack.IsUnderpassAvailable)
				LevelOffset_Y = rack.Underpass;
			else if (rack.IsMaterialOnGround)
				LevelOffset_Y = 0;
			else
			{
				double firstLevelOffset = Rack.sFirstLevelOffset;
				if (rack.Levels != null)
				{
					RackLevel firstLevel = rack.Levels.FirstOrDefault(level => level != null && level.Index == 1);
					if (firstLevel != null && firstLevel.Beam != null)
						firstLevelOffset -= firstLevel.Beam.Height;
				}
				LevelOffset_Y = Utils.GetWholeNumber(firstLevelOffset);
			}
			//
			if (rack.Levels != null)
			{
				//
				Point _ZeroLevel_LeftGlobalPoint = leftColumnEnd_GlobalPoint;
				_ZeroLevel_LeftGlobalPoint.Y = leftColumnStart_GlobalPnt.Y;
				Point _ZeroLevel_RightGlobalPoint = rightColumnStart_GlobalPoint;
				_ZeroLevel_RightGlobalPoint.Y = leftColumnStart_GlobalPnt.Y;
				//
				foreach (RackLevel level in rack.Levels)
				{
					if (level == null)
						continue;

					int _beamHeight = 0;
					if (level.Beam != null)
						_beamHeight = Utils.GetWholeNumber(level.Beam.Height);

					// level shelf
					Point LevelStart_GlobalPoint = _ZeroLevel_LeftGlobalPoint;
					LevelStart_GlobalPoint.Y -= LevelOffset_Y;
					//
					Point LevelEnd_GlobalPoint = _ZeroLevel_RightGlobalPoint;
					if (level.Index != 0)
					{
						LevelEnd_GlobalPoint.Y -= LevelOffset_Y + _beamHeight;
						// dont draw level shelf at the side view
						//dc.DrawRectangle(m_LevelShelfFillBrush, m_LevelShelfBorderPen, new Rect(LevelStart_ScreenPoint, LevelEnd_ScreenPoint));
					}

					// draw "Decking plate" at the side view if it has along length value.
					if (level.Accessories != null && level.Accessories.IsDeckPlateAvailable && level.Accessories.DeckPlateType == eDeckPlateType.eAlongLength)
					{
						Point deckingPlateStartGlobalPnt = LevelEnd_GlobalPoint;
						deckingPlateStartGlobalPnt.X = LevelStart_GlobalPoint.X;
						deckingPlateStartGlobalPnt.Y += (_beamHeight / 2) / 2;
						Point deckingPlateEndGlobalPnt = deckingPlateStartGlobalPnt;
						deckingPlateEndGlobalPnt.X = LevelEnd_GlobalPoint.X;

						Point deckingPlateStartScreenPnt = cs.GetLocalPoint(deckingPlateStartGlobalPnt, defaultCameraScale, defaultCameraOffset);
						Point deckingPlateEndScreenPnt = cs.GetLocalPoint(deckingPlateEndGlobalPnt, defaultCameraScale, defaultCameraOffset);

						double deckPlateLineThickness = cs.GetHeightInPixels(_beamHeight / 3, 1.0);
						Pen deckingPlatePen = new Pen(displaySettings.DeckingPlateBrush, deckPlateLineThickness);
						deckingPlatePen.DashStyle = DashStyles.Dash;

						dc.DrawLine(deckingPlatePen, deckingPlateStartScreenPnt, deckingPlateEndScreenPnt);
					}

					bool bAddPalletRiser = false;
					if (level.Accessories != null && level.Accessories.ForkEntryBar)
						bAddPalletRiser = true;

					//
					if (rack.ShowPallet)
					{
						// draw pallets
						if (level.Pallets != null && level.Pallets.Count > 0)
						{
							// draw from the last pallet to the first
							for (int iPalletIndex = level.Pallets.Count - 1; iPalletIndex >= 0; --iPalletIndex)
							{
								Pallet pallet = level.Pallets[iPalletIndex];
								if (pallet == null)
									continue;

								// draw pallet
								Point PalletCenter_GlobalPoint = LevelStart_GlobalPoint;
								PalletCenter_GlobalPoint.Y = LevelEnd_GlobalPoint.Y;
								if (bAddPalletRiser)
									PalletCenter_GlobalPoint.Y -= Rack.PALLET_RISER_HEIGHT;
								PalletCenter_GlobalPoint.X += (LevelEnd_GlobalPoint.X - LevelStart_GlobalPoint.X) / 2;
								//
								Point PalletStart_GlobalPoint = PalletCenter_GlobalPoint;
								PalletStart_GlobalPoint.X -= pallet.Width / 2;
								//
								Point PalletEnd_GlobalPoint = PalletCenter_GlobalPoint;
								PalletEnd_GlobalPoint.X += pallet.Width / 2;
								PalletEnd_GlobalPoint.Y -= pallet.Height;
								//
								_DrawRectangle(dc, brushes.PalletFillBrush, brushes.PalletBorderPen, PalletStart_GlobalPoint, PalletEnd_GlobalPoint, cs);

								// draw pallet riser
								if (bAddPalletRiser)
								{
									Point RiserStart_GlobalPoint = PalletStart_GlobalPoint;
									RiserStart_GlobalPoint.X = leftColumnStart_GlobalPnt.X;
									//
									Point RiserEnd_GlobalPoint = RiserStart_GlobalPoint;
									RiserEnd_GlobalPoint.X = rightColumnEnd_GlobalPoint.X;
									RiserEnd_GlobalPoint.Y += Rack.PALLET_RISER_HEIGHT;
									//
									_DrawRectangle(dc, brushes.PalletRiserFillBrush, brushes.PalletRiserBorderPen, RiserStart_GlobalPoint, RiserEnd_GlobalPoint, cs);
								}
							}
						}
					}

					if (level.Index != 0)
						LevelOffset_Y += _beamHeight;
					LevelOffset_Y += level.LevelHeight;
				}
			}

			double dimensionsLinesOffset = cs.GetWidthInPixels(rack.PalletOverhangValue, 1.0);

			// draw bracing lines
			if (rack.Bracing != eColumnBracingType.eUndefined)
			{
				double heightOffset = Rack.sBracingLinesBottomOffset;
				Vector heightDirection = new Vector(0, -1);
				Point startPoint = new Point(leftColumnEnd_GlobalPoint.X, leftColumnStart_GlobalPnt.Y);
				Point endPoint = new Point(rightColumnStart_GlobalPoint.X, leftColumnStart_GlobalPnt.Y);
				// draw horizontal bracing
				Point lineStart_GlobalPoint = startPoint + heightOffset * heightDirection;
				Point lineEnd_GlobalPoint = endPoint + heightOffset * heightDirection;
				_DrawBracingLine(dc, cs, lineStart_GlobalPoint, lineEnd_GlobalPoint, brushes.BracingLineBorderPen);

				// draw X bracings
				if (rack.Bracing == eColumnBracingType.eXBracing || rack.Bracing == eColumnBracingType.eXBracingWithStiffener)
				{
					int xBracingsCount = rack.X_Bracings_Count;
					if (xBracingsCount > 0)
					{
						heightOffset += Rack.sXBracingVerticalOffset;

						for (int i = 1; i <= xBracingsCount; ++i)
						{
							Point xBracingStartPnt = startPoint + heightOffset * heightDirection;
							Point xBracingEndPnt = endPoint + (heightOffset + Rack.sBracingVerticalStep) * heightDirection;
							_DrawBracingLine(dc, cs, xBracingStartPnt, xBracingEndPnt, brushes.BracingLineBorderPen);

							double xBracingStartPnt_Y = xBracingStartPnt.Y;
							xBracingStartPnt.Y = xBracingEndPnt.Y;
							xBracingEndPnt.Y = xBracingStartPnt_Y;
							_DrawBracingLine(dc, cs, xBracingStartPnt, xBracingEndPnt, brushes.BracingLineBorderPen);

							heightOffset += Rack.sBracingVerticalStep;
						}

						heightOffset += Rack.sXBracingVerticalOffset;

						// draw horizontal bracing
						lineStart_GlobalPoint = startPoint + heightOffset * heightDirection;
						lineEnd_GlobalPoint = endPoint + heightOffset * heightDirection;
						_DrawBracingLine(dc, cs, lineStart_GlobalPoint, lineEnd_GlobalPoint, brushes.BracingLineBorderPen);
					}
				}

				// draw normal bracings
				double rightFrameHeight = rack.Length_Z;
				if (rack.TieBeamFrame.HasFlag(eTieBeamFrame.eEndFrame))
					rightFrameHeight = rack.FrameHeight;
				//
				int iTotalLines = (int)Math.Floor((Utils.GetWholeNumber(rightFrameHeight) - heightOffset - Rack.sTopHorizontalBracingOffset) / Rack.sBracingVerticalStep);
				for (int i = 1; i <= iTotalLines; ++i)
				{
					//
					if (i % 2 != 0)
					{
						lineStart_GlobalPoint = startPoint + heightOffset * heightDirection;
						lineEnd_GlobalPoint = endPoint + (heightOffset + Rack.sBracingVerticalStep) * heightDirection;
					}
					else
					{
						lineStart_GlobalPoint = endPoint + heightOffset * heightDirection;
						lineEnd_GlobalPoint = startPoint + (heightOffset + Rack.sBracingVerticalStep) * heightDirection;
					}

					//
					_DrawBracingLine(dc, cs, lineStart_GlobalPoint, lineEnd_GlobalPoint, brushes.BracingLineBorderPen);

					// draw horizontal lines
					if (i == iTotalLines)
					{
						Point vertLineEnd_GlobalPoint = lineEnd_GlobalPoint;
						if (i % 2 != 0)
							vertLineEnd_GlobalPoint.X = leftColumnEnd_GlobalPoint.X;
						else
							vertLineEnd_GlobalPoint.X = rightColumnStart_GlobalPoint.X;
						_DrawBracingLine(dc, cs, lineEnd_GlobalPoint, vertLineEnd_GlobalPoint, brushes.BracingLineBorderPen);
					}

					heightOffset += Rack.sBracingVerticalStep;
				}

				// if there is more than sTopHorizontalBracingMinDistance then show additional line
				if (iTotalLines > 0)
				{
					double topHeightRemainder = rightFrameHeight - heightOffset;
					if (Utils.FGE(topHeightRemainder, Rack.sTopHorizontalBracingMinDistance))
					{
						Point vertLine_StartPoint = leftColumnEnd_GlobalPoint;
						vertLine_StartPoint.Y = leftColumnStart_GlobalPnt.Y - (rightFrameHeight - Rack.sTopHorizontalBracingOffset);
						Point vertLine_EndPoint = rightColumnStart_GlobalPoint;
						vertLine_EndPoint.Y = vertLine_StartPoint.Y;
						_DrawBracingLine(dc, cs, vertLine_StartPoint, vertLine_EndPoint, brushes.BracingLineBorderPen);
					}
				}

				if (showDimensions)
				{
					// draw bracing dimensions only once
					// draw bracing vertical step
					Point dimGlobalPnt_01 = startPoint + Rack.sBracingLinesBottomOffset * heightDirection;
					dimGlobalPnt_01.X = showDimensionsLeft
						? leftColumnEnd_GlobalPoint.X
						: rightColumnEnd_GlobalPoint.X;
					if (rack.Bracing == eColumnBracingType.eXBracing || rack.Bracing == eColumnBracingType.eXBracingWithStiffener)
						dimGlobalPnt_01 += Rack.sXBracingVerticalOffset * heightDirection;
					//
					Point dimGlobalPnt_02 = dimGlobalPnt_01;
					dimGlobalPnt_02 += Rack.sBracingVerticalStep * heightDirection;
					_DrawDimension(dc, dimGlobalPnt_01, dimGlobalPnt_02, Rack.sBracingVerticalStep.ToString(), 
						dimensionsLinesOffset + displaySettings.MinDimensionsLinesOffset,
						displaySettings.DimensionsTextSize,
						displaySettings.PerpDimLinesOffsetInPixels,
						showDimensionsLeft
							? eDimensionPlacement.eLeft
							: eDimensionPlacement.eRight,
						cs, dimensionBrush: displaySettings.DimensionsBrush);

					// draw X bracing height
					if (eColumnBracingType.eXBracing == rack.Bracing || eColumnBracingType.eXBracingWithStiffener == rack.Bracing)
					{
						dimGlobalPnt_01 = showDimensionsLeft 
							? leftColumnStart_GlobalPnt 
							: rightColumnStart_GlobalPoint;
						dimGlobalPnt_01.Y = rightColumnEnd_GlobalPoint.Y;

						dimGlobalPnt_02 = dimGlobalPnt_01;
						dimGlobalPnt_02 += rack.X_Bracing_Height * heightDirection;

						_DrawDimension(dc, 
							dimGlobalPnt_01, 
							dimGlobalPnt_02, 
							rack.X_Bracing_Height.ToString(), 
							dimensionsLinesOffset + 3 * displaySettings.MinDimensionsLinesOffset, 
							displaySettings.DimensionsTextSize, 
							displaySettings.PerpDimLinesOffsetInPixels,
							showDimensionsLeft 
								? eDimensionPlacement.eLeft 
								: eDimensionPlacement.eRight, 
							cs,
							dimensionBrush: displaySettings.DimensionsBrush);
					}
				}
			}

			// draw columns
			_DrawRectangle(dc, brushes.ColumnFillBrush, brushes.ColumnBorderPen, leftColumnStart_GlobalPnt, leftColumnEnd_GlobalPoint, cs);
			_DrawRectangle(dc, brushes.ColumnFillBrush, brushes.ColumnBorderPen, rightColumnStart_GlobalPoint, rightColumnEnd_GlobalPoint, cs);

			// draw tie beam frame, it is equal to max material height + 500
			if (rack.TieBeamFrame.HasFlag(eTieBeamFrame.eEndFrame))
			{
				// left column
				Point leftFrameStart_GlobalPoint = leftColumnStart_GlobalPnt;
				leftFrameStart_GlobalPoint.Y = leftColumnEnd_GlobalPoint.Y;
				Point leftFrameEnd_GlobalPoint = leftFrameStart_GlobalPoint;
				leftFrameEnd_GlobalPoint.X = leftColumnEnd_GlobalPoint.X;
				leftFrameEnd_GlobalPoint.Y -= rack.FrameHeight - rackHeight;
				//
				_DrawRectangle(dc, brushes.ColumnFillBrush, brushes.ColumnBorderPen, leftFrameStart_GlobalPoint, leftFrameEnd_GlobalPoint, cs);

				// right column
				Point rightFrameStart_GlobalPoint = rightColumnStart_GlobalPoint;
				rightFrameStart_GlobalPoint.Y = leftFrameStart_GlobalPoint.Y;
				Point rightFrameEnd_GlobalPoint = leftFrameEnd_GlobalPoint;
				rightFrameEnd_GlobalPoint.X = rightColumnEnd_GlobalPoint.X;
				//
				_DrawRectangle(dc, brushes.ColumnFillBrush, brushes.ColumnBorderPen, rightFrameStart_GlobalPoint, rightFrameEnd_GlobalPoint, cs);
			}

			// top Depth dimension
			if (showDimensions)
			{
				//double supportLinesOffset = cs.GetHeightInPixels(scale * (Math.Max(Math.Abs(rack.MaterialHeight - rack.FrameHeight), Math.Abs(rack.Length_Z - rack.FrameHeight))), 1.0);
				double supportLinesOffset = cs.GetHeightInPixels(Math.Max(Math.Abs(rack.MaterialHeight - rack.FrameHeight), Math.Abs(rack.Length_Z - rack.FrameHeight)), 1.0);
				// draw rack width dimension
				string strRackWidth = rack.Depth.ToString();
				Point dimensionGlobalPnt_01 = leftColumnStart_GlobalPnt;
				dimensionGlobalPnt_01.Y = leftColumnEnd_GlobalPoint.Y;
				Point dimensionGlobalPnt_02 = dimensionGlobalPnt_01;
				dimensionGlobalPnt_02.X = rightColumnEnd_GlobalPoint.X;

				_DrawDimension(dc, dimensionGlobalPnt_01, dimensionGlobalPnt_02, strRackWidth, 
					supportLinesOffset + displaySettings.MinDimensionsLinesOffset, 
					displaySettings.DimensionsTextSize, 
					displaySettings.PerpDimLinesOffsetInPixels, 
					eDimensionPlacement.eTop, cs, 
					dimensionBrush: displaySettings.DimensionsBrush);

				//// draw column width dimension
				//string strColumnWidth = rack.Column.Depth.ToString();
				//dimensionGlobalPnt_02 = leftColumnStart_GlobalPnt;
				//dimensionGlobalPnt_02.X = leftColumnEnd_GlobalPoint.X;
				//_DrawDimension(dc, leftColumnStart_GlobalPnt, dimensionGlobalPnt_02, strColumnWidth, displaySettings.MinDimensionsLinesOffset / 2, displaySettings.DimensionsTextSize, displaySettings.PerpDimLinesOffsetInPixels, eDimensionPlacement.eBot, cs);
			}

            bool isHeightDisplayed = false;

            if (rack.Accessories.UprightGuard)
            {
                _TryDrawSideColumnGuard(dc, cs, drawStartPoint, displaySettings, rack, false/*showDimensions*/, out isHeightDisplayed);
            }

            if (rack.Accessories.RowGuard)
            {
                _TryDrawSideRowGuard(dc, cs, drawStartPoint, displaySettings, rack, false/*showDimensions*/, showHeight: !isHeightDisplayed);
            }
        }
		






		private static void _TryDrawFrontRowGuard(DrawingContext dc, ICoordinateSystem cs, RackAdvancedDrawingSettings displaySettings, Rack rack, bool isHeightDisplayed)
		{
			if (rack.ConectedAisleSpaceDirections == ConectedAisleSpaceDirection.NONE && !rack.IsUnderpassAvailable)
				return;

			Color rackGuardFillColor = Colors.Black;
			if (CurrentGeometryColorsTheme.CurrentTheme != null)
			{
				Color color;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackGuardMainColorDefault, out color))
					rackGuardFillColor = color;
			}

			Color rackGuardAltFillColor = Colors.Yellow;
			if (CurrentGeometryColorsTheme.CurrentTheme != null)
			{
				Color color;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackGuardAltColorDefault, out color))
					rackGuardAltFillColor = color;
			}

			Pen borderPen = new Pen(new SolidColorBrush(rackGuardFillColor), 1.0);

			double leftOffsetX = 0;
			double rightOffsetX = 0;

			double rackLength = rack.Length_X;
			if (!rack.IsHorizontal)
			{
				rackLength = rack.Length_Y;
			}

			if (rack.IsUnderpassAvailable)
			{
				leftOffsetX = GuardRowParameters.GuardColumnSuppotOffset;
				if (rack.IsFirstInRowColumn)
					leftOffsetX += rack.Column.Length;

				rightOffsetX = rackLength - rack.Column.Length - (GuardRowParameters.GuardColumnSuppotOffset + GuardRowParameters.GuardRowFoundationWidth);

				_DrawRowGuard(new Point(leftOffsetX, 0), dc, cs, displaySettings, borderPen, rackGuardFillColor, rackGuardAltFillColor, 
					showHeightDimensions: !rack.Accessories.UprightGuard, showWidthAndOffset: !rack.IsFirstInRowColumn, isUnderpass: true);

				_DrawRowGuard(new Point(rightOffsetX, 0), dc, cs, displaySettings, borderPen, rackGuardFillColor, rackGuardAltFillColor, 
					showWidthAndOffset: rack.IsFirstInRowColumn, isUnderpass: true);
                
				if (displaySettings.DisplayTextAndDimensions)
                {
					double rackUnderpassClearLength = rack.InnerLength - (2 * (GuardRowParameters.GuardRowRackOffset + GuardRowParameters.GuardRowFoundationWidth));
					Point dimStart = new Point(rack.Column.Length + GuardRowParameters.GuardRowRackOffset + GuardRowParameters.GuardRowFoundationWidth, 0);

                    if (!rack.IsFirstInRowColumn)
						dimStart.X -= rack.Column.Length;

					Point dimEnd = new Point(dimStart.X + rackUnderpassClearLength, 0);

					_DrawDimension(dc,
						dimStart,
						dimEnd,
						$"Clear Aisle {rackUnderpassClearLength}",
						displaySettings.MinDimensionsLinesOffset,
						displaySettings.DimensionsTextSize,
						displaySettings.PerpDimLinesOffsetInPixels,
						eDimensionPlacement.eTop,
						cs,
						dimensionBrush: displaySettings.DimensionsBrush
						);
                }
			}
			else
			{
				leftOffsetX = -(GuardRowParameters.GuardRowFoundationWidth + GuardRowParameters.GuardRowRackOffset);
				rightOffsetX = rackLength + GuardRowParameters.GuardRowRackOffset;

				bool isRightConnected = (rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.RIGHT))
					|| (!rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.TOP));

				bool isLeftConnected = (rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.LEFT))
					|| (!rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.BOTTOM));

				bool isShowAllDimensionsTogether = (isRightConnected && !isLeftConnected) || (!isRightConnected && isLeftConnected);

				if (isRightConnected)
				{
					_DrawRowGuard(new Point(rightOffsetX, 0), dc, cs, displaySettings, borderPen, rackGuardFillColor, rackGuardAltFillColor,
						showHeightDimensions: false, showWidthAndOffset: true, isRightSideRack: true);
					isHeightDisplayed = true;
				}

				if (isLeftConnected)
				{
					_DrawRowGuard(new Point(leftOffsetX, 0), dc, cs, displaySettings, borderPen, rackGuardFillColor, rackGuardAltFillColor,
						showHeightDimensions: !isHeightDisplayed, showWidthAndOffset: isShowAllDimensionsTogether);
				}
			}
		}

		private static void _TryDrawFrontColumnGuard(DrawingContext dc, ICoordinateSystem cs, RackAdvancedDrawingSettings displaySettings, Rack rack, out bool isHeightDisplayed)
		{
			isHeightDisplayed = false;

			if (rack.ConectedAisleSpaceDirections == ConectedAisleSpaceDirection.NONE)
				return;

			Color rackGuardFillColor = Colors.Black;
			if (CurrentGeometryColorsTheme.CurrentTheme != null)
			{
				Color color;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackGuardMainColorDefault, out color))
					rackGuardFillColor = color;
			}

			Color rackGuardAltFillColor = Colors.Yellow;
			if (CurrentGeometryColorsTheme.CurrentTheme != null)
			{
				Color color;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackGuardAltColorDefault, out color))
					rackGuardAltFillColor = color;
			}

			Brush rackGuardMainFillBrush = GetStripesBrush(rackGuardFillColor, rackGuardAltFillColor);
			Pen borderPen = new Pen(new SolidColorBrush(rackGuardFillColor), 1.0);

			Point start;
			Point end;
			bool isDrawnWidth = false;
			bool isDrawnHeight = false;
			bool isHeightFitLeft = false;
			bool isHeightFitRight = false;

			if ((rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.BOTTOM))
				|| (!rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.RIGHT)))
			{
				double rackLength = rack.Length_X;
				if (!rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.RIGHT))
					rackLength = rack.Length_Y;

				isHeightFitLeft = (rack.IsFirstInRowColumn && rack.IsHorizontal && !rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.LEFT))
					|| (rack.IsFirstInRowColumn && !rack.IsHorizontal && !rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.BOTTOM));
				isHeightFitRight = rack.IsUnderpassAvailable || ((rack.IsHorizontal && !rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.RIGHT))
					|| (!rack.IsHorizontal && !rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.TOP)));

				if (rack.IsFirstInRowColumn)
				{
					start = new Point(-GuardColumnParameters.GuardColumnSideOffset, 0);
					end = new Point(rack.Column.Length + GuardColumnParameters.GuardColumnSideOffset, -GuardColumnParameters.GuardColumnHeight);

					_DrawRectangle(dc, rackGuardMainFillBrush, borderPen, start, end, cs);

					if (displaySettings.DisplayTextAndDimensions)
					{
						if (isHeightFitLeft)
						{
							// height
							_DrawDimension(dc, start, end,
								GuardColumnParameters.GuardColumnHeight.ToString(),
								displaySettings.MinDimensionsLinesOffset,
								displaySettings.DimensionsTextSize,
								displaySettings.PerpDimLinesOffsetInPixels,
								eDimensionPlacement.eLeft,
								cs,
								dimensionBrush: displaySettings.DimensionsBrush);
							isDrawnHeight = true;
						}

						if (!isDrawnWidth && isHeightFitLeft)
						{
							isDrawnWidth = true;
							// width
							_DrawDimension(dc,
								new Point(start.X, 0),
								new Point(end.X, 0),
								$"{Math.Abs(end.X - start.X)}",
								displaySettings.MinDimensionsLinesOffset,
								displaySettings.DimensionsTextSize,
								displaySettings.PerpDimLinesOffsetInPixels,
								eDimensionPlacement.eBot,
								cs,
								dimensionBrush: displaySettings.DimensionsBrush,
								bMirrorTextRelativeToDimLine: true);
						}
					}
				}

				start = new Point(rackLength - rack.Column.Length - GuardColumnParameters.GuardColumnSideOffset, 0);
				end = new Point(start.X + rack.Column.Length + (2 * GuardColumnParameters.GuardColumnSideOffset), -GuardColumnParameters.GuardColumnHeight);

				_DrawRectangle(dc, rackGuardMainFillBrush, borderPen, start, end, cs);

				if (displaySettings.DisplayTextAndDimensions)
				{
                    if (isHeightFitRight && !isDrawnHeight)
						// height
						_DrawDimension(dc, start, end,
							GuardColumnParameters.GuardColumnHeight.ToString(),
							10,
							displaySettings.DimensionsTextSize,
							displaySettings.PerpDimLinesOffsetInPixels,
							eDimensionPlacement.eRight,
							cs,
							dimensionBrush: displaySettings.DimensionsBrush);

					if (!isDrawnWidth && isHeightFitRight)
					{
						// width
						_DrawDimension(dc,
							new Point(start.X, 0),
							new Point(end.X, 0),
							$"{Math.Abs(end.X - start.X)}",
							displaySettings.MinDimensionsLinesOffset,
							displaySettings.DimensionsTextSize,
							displaySettings.PerpDimLinesOffsetInPixels,
							eDimensionPlacement.eBot,
							cs,
							dimensionBrush: displaySettings.DimensionsBrush,
							bMirrorTextRelativeToDimLine: true);
					}
                }
			}
			else if ((rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.TOP))
				|| (!rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.LEFT)))
			{
				double rackLength = rack.Length_X;
				if (!rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.LEFT))
					rackLength = rack.Length_Y;

				isHeightFitLeft = (rack.IsFirstInRowColumn && rack.IsHorizontal && !rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.LEFT))
					|| (rack.IsFirstInRowColumn && !rack.IsHorizontal && !rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.TOP));
				isHeightFitRight = rack.IsUnderpassAvailable || ((rack.IsHorizontal && !rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.RIGHT))
					|| (!rack.IsHorizontal && !rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.TOP)));

				if (rack.IsFirstInRowColumn)
				{
					start = new Point(-GuardColumnParameters.GuardColumnSideOffset, 0);
					end = new Point(rack.Column.Length + GuardColumnParameters.GuardColumnSideOffset, -GuardColumnParameters.GuardColumnHeight);

					_DrawRectangle(dc, rackGuardMainFillBrush, borderPen, start, end, cs);

					if (displaySettings.DisplayTextAndDimensions)
					{
						// height
						if (isHeightFitLeft)
							_DrawDimension(dc, start, end,
						 		GuardColumnParameters.GuardColumnHeight.ToString(),
								displaySettings.MinDimensionsLinesOffset,
								displaySettings.DimensionsTextSize,
								displaySettings.PerpDimLinesOffsetInPixels,
								eDimensionPlacement.eLeft,
								cs,
								dimensionBrush: displaySettings.DimensionsBrush);

						// width
						if (!isDrawnWidth && isHeightFitLeft)
						{
							isDrawnWidth = true;
							_DrawDimension(dc,
								new Point(start.X, 0),
								new Point(end.X, 0),
								$"{Math.Abs(end.X - start.X)}",
								displaySettings.MinDimensionsLinesOffset,
								displaySettings.DimensionsTextSize,
								displaySettings.PerpDimLinesOffsetInPixels,
								eDimensionPlacement.eBot,
								cs,
								dimensionBrush: displaySettings.DimensionsBrush,
								bMirrorTextRelativeToDimLine: true);
						}
					}
				}

				start = new Point(rackLength - rack.Column.Length - GuardColumnParameters.GuardColumnSideOffset, 0);
				end = new Point(start.X + rack.Column.Length + (2 * GuardColumnParameters.GuardColumnSideOffset), -GuardColumnParameters.GuardColumnHeight);

				_DrawRectangle(dc, rackGuardMainFillBrush, borderPen, start, end, cs);

				if (displaySettings.DisplayTextAndDimensions && !isDrawnWidth)
				{
					// height
					if (isHeightFitRight)
						_DrawDimension(dc, new Point(end.X, start.Y), end,
							Math.Abs(start.Y - end.Y).ToString(),
							7,
							displaySettings.DimensionsTextSize,
							displaySettings.PerpDimLinesOffsetInPixels,
							eDimensionPlacement.eRight,
							cs,
							dimensionBrush: displaySettings.DimensionsBrush);

					if (!isDrawnWidth && isHeightFitRight)
					{
						isDrawnWidth = true;
						_DrawDimension(dc,
							new Point(start.X, 0),
							new Point(end.X, 0),
							$"{Math.Abs(end.X - start.X)}",
							displaySettings.MinDimensionsLinesOffset,
							displaySettings.DimensionsTextSize,
							displaySettings.PerpDimLinesOffsetInPixels,
							eDimensionPlacement.eBot,
							cs,
							dimensionBrush: displaySettings.DimensionsBrush,
							bMirrorTextRelativeToDimLine: true);
					}
				}
			}

			isHeightDisplayed = isHeightFitLeft || isHeightFitRight;
		}

		private static void _TryDrawSideRowGuard(DrawingContext dc, ICoordinateSystem cs, Point startPoint, RackAdvancedDrawingSettings displaySettings, Rack rack,
			bool showTextAndDimensions, bool showHeight = false)
		{
			if (rack.ConectedAisleSpaceDirections == ConectedAisleSpaceDirection.NONE && !rack.IsUnderpassAvailable)
				return;

			Color rackGuardFillColor = Colors.Black;
			if (CurrentGeometryColorsTheme.CurrentTheme != null)
			{
				Color color;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackGuardMainColorDefault, out color))
					rackGuardFillColor = color;
			}

			Color rackGuardAltFillColor = Colors.Black;
			if (CurrentGeometryColorsTheme.CurrentTheme != null)
			{
				Color color;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackGuardAltColorDefault, out color))
					rackGuardAltFillColor = color;
			}

			Brush rackGuardMainFillBrush = GetStripesBrush(rackGuardFillColor, rackGuardAltFillColor);
			Pen borderPen = new Pen(new SolidColorBrush(rackGuardFillColor), 1.0);

			Point start = new Point(startPoint.X + rack.PalletOverhangValue + GuardRowParameters.GuardRowRackOffset, 0);

			bool isRowGuardsVisible = false;

			if (rack.IsUnderpassAvailable)
			{
				double xOffset = rack.Column.Depth - GuardRowParameters.GuardRowRackOffset;
				_DrawSideRowGuard(dc, cs, rack, start, borderPen, rackGuardMainFillBrush, xOffset, true);
				isRowGuardsVisible = true;
			}
			else
			{
				if ((rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.RIGHT))
					|| (!rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.TOP)))
				{
					_DrawSideRowGuard(dc, cs, rack, start, borderPen, rackGuardMainFillBrush);
					isRowGuardsVisible = true;
				}
				else if ((rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.LEFT))
					|| (!rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.BOTTOM)))
				{
					_DrawSideRowGuard(dc, cs, rack, start, borderPen, rackGuardMainFillBrush);
					isRowGuardsVisible = true;
				}
			}

            if (showTextAndDimensions && showHeight && isRowGuardsVisible)
            {
				_DrawDimension(
					dc,
					start,
					new Point(start.X, start.Y - 400),
					"400",
					displaySettings.DimensionsTextSize,
					displaySettings.DimensionsTextSize,
					displaySettings.PerpDimLinesOffsetInPixels,
					eDimensionPlacement.eLeft,
					cs,
					dimensionBrush: displaySettings.DimensionsBrush);
			}
		}

		private static void _TryDrawSideColumnGuard(DrawingContext dc, ICoordinateSystem cs, Point startPoint, RackAdvancedDrawingSettings displaySettings, Rack rack,
			bool showTextAndDimensions, out bool isHeightDrawn)
		{
			isHeightDrawn = false;

			if (rack.ConectedAisleSpaceDirections == ConectedAisleSpaceDirection.NONE)
				return;

			Color rackGuardFillColor = Colors.Black;
			if (CurrentGeometryColorsTheme.CurrentTheme != null)
			{
				Color color;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackGuardMainColorDefault, out color))
					rackGuardFillColor = color;
			}

			Color rackGuardAltFillColor = Colors.Black;
			if (CurrentGeometryColorsTheme.CurrentTheme != null)
			{
				Color color;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackGuardAltColorDefault, out color))
					rackGuardAltFillColor = color;
			}

			Brush rackGuardMainFillBrush = GetStripesBrush(rackGuardFillColor, rackGuardAltFillColor);
			Pen borderPen = new Pen(new SolidColorBrush(rackGuardFillColor), 1.0);

			Point step;

			Point dimensionStart;
			Point dimensionEnd;
			Point dimensionHieght;

			bool isDimensionsDrawn = false;

			List<Point> path = new List<Point>();

			if ((rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.TOP))
				|| (!rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.LEFT)))
			{
				step = new Point(startPoint.X + rack.PalletOverhangValue - GuardColumnParameters.GuardColumnForwardOffset, 0);
				path.Add(step);
				dimensionStart = step;
				dimensionHieght = new Point(dimensionStart.X, dimensionStart.Y);

				step.X += GuardColumnParameters.GuardColumnDepth;
				path.Add(step);
				dimensionEnd = step;
				
				step.Y = -GuardColumnParameters.GuardColumnInnerHeight;
				path.Add(step);

				step.X -= GuardColumnParameters.GuardColumnInnerAngleOffset;
				step.Y = -GuardColumnParameters.GuardColumnHeight;
				path.Add(step);
				dimensionHieght.Y = step.Y;

				step.X -= GuardColumnParameters.GuardColumnTopDepth;
				path.Add(step);

				_DrawGeometry(dc, cs, borderPen, rackGuardMainFillBrush, path.ToArray());

				isDimensionsDrawn = true;
				if (showTextAndDimensions)
				{
					// depth
					_DrawDimension(dc, dimensionStart, dimensionEnd,
						GuardColumnParameters.GuardColumnDepth.ToString(),
						3,
						displaySettings.DimensionsTextSize,
						displaySettings.PerpDimLinesOffsetInPixels,
						eDimensionPlacement.eBot,
						cs,
						dimensionBrush: displaySettings.DimensionsBrush,
						dimensionTextOffset: 200,
						bMirrorTextRelativeToDimLine: false,
						drawAdditionalSupportDimLine: true);

					// offset
					Point rackColumnPosition = new Point(dimensionStart.X + 125, dimensionEnd.Y);
					_DrawDimension(
						dc,
						dimensionStart,
						rackColumnPosition,
						GuardColumnParameters.GuardColumnForwardOffset.ToString(),
						15,
						displaySettings.DimensionsTextSize,
						displaySettings.PerpDimLinesOffsetInPixels,
						eDimensionPlacement.eBot,
						cs,
						dimensionBrush: displaySettings.DimensionsBrush,
						bMirrorTextRelativeToDimLine: false);

                    // Guard Height
                    _DrawDimension(
                        dc,
                        new Point(dimensionHieght.X, dimensionHieght.Y),
						new Point(dimensionHieght.X, 0),
						GuardColumnParameters.GuardColumnHeight.ToString(),
                        displaySettings.DimensionsTextSize,
                        displaySettings.DimensionsTextSize,
                        displaySettings.PerpDimLinesOffsetInPixels,
                        eDimensionPlacement.eLeft,
                        cs,
                        dimensionBrush: displaySettings.DimensionsBrush,
                        bMirrorTextRelativeToDimLine: false);

                    isHeightDrawn = true;
                }
			}

			path.Clear();

			if ((rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.BOTTOM))
				|| (!rack.IsHorizontal && rack.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.RIGHT)))
			{
				step = new Point(startPoint.X + rack.PalletOverhangValue + rack.Depth + GuardColumnParameters.GuardColumnForwardOffset, 0);
				path.Add(step);
				dimensionStart = step;
				dimensionHieght = new Point(dimensionStart.X, dimensionStart.Y);

				step.X -= GuardColumnParameters.GuardColumnDepth;
				path.Add(step);
				dimensionEnd = step;

				step.Y = -GuardColumnParameters.GuardColumnInnerHeight;
				path.Add(step);

				step.X += GuardColumnParameters.GuardColumnInnerAngleOffset;
				step.Y = -GuardColumnParameters.GuardColumnHeight;
				path.Add(step);
				dimensionHieght.Y = step.Y;

				step.X += GuardColumnParameters.GuardColumnTopDepth;
				path.Add(step);

				_DrawGeometry(dc, cs, borderPen, rackGuardMainFillBrush, path.ToArray());

				if (showTextAndDimensions && !isDimensionsDrawn)
				{
					// depth
					_DrawDimension(dc,
						dimensionEnd,
						dimensionStart,
						Math.Abs(dimensionStart.X - dimensionEnd.X).ToString(),
						3,
						displaySettings.DimensionsTextSize,
						displaySettings.PerpDimLinesOffsetInPixels,
						eDimensionPlacement.eBot,
						cs,
						dimensionBrush: displaySettings.DimensionsBrush,
						dimensionTextOffset: -200,
						bMirrorTextRelativeToDimLine: false,
						drawAdditionalSupportDimLine: true);

					// offset
					Point rackColumnPosition = new Point(dimensionStart.X - 125, dimensionEnd.Y);
					_DrawDimension(
						dc,
						rackColumnPosition,
						dimensionStart,
						GuardColumnParameters.GuardColumnForwardOffset.ToString(),
						15,
						displaySettings.DimensionsTextSize,
						displaySettings.PerpDimLinesOffsetInPixels,
						eDimensionPlacement.eBot,
						cs,
						dimensionBrush: displaySettings.DimensionsBrush,
						bMirrorTextRelativeToDimLine: false);
				}
			}
		}

		private static void _DrawSideRowGuard(DrawingContext dc, ICoordinateSystem cs, Rack rack, Point start, Pen borderPen, Brush brush, 
			double xAxisRackDarwingLimitation = 0, bool showHorizontalGuardOnly = false)
		{
			// left guard foundation
			start.X += xAxisRackDarwingLimitation;
			Point end = new Point(start.X + GuardRowParameters.GuardRowFoundationLength, start.Y - GuardRowParameters.GuardRowFoundationHeight);
			end.X -= xAxisRackDarwingLimitation;
            if (!showHorizontalGuardOnly)
				_DrawRectangle(dc, Brushes.White, borderPen, start, end, cs);

			start.Y = end.Y;
			end.Y -= (GuardRowParameters.GuardRowHeight - GuardRowParameters.GuardRowFoundationHeight - GuardRowParameters.GuardRowWidth);
            if (!showHorizontalGuardOnly)
				_DrawRectangle(dc, brush, borderPen, start, end, cs);

			// guard horizontal plank
			start.Y = end.Y;
			end.X = start.X + rack.Depth - (2 * GuardRowParameters.GuardRowRackOffset);
			end.X -= (2 * xAxisRackDarwingLimitation);
			end.Y -= GuardRowParameters.GuardRowWidth;
			_DrawRectangle(dc, brush, borderPen, start, end, cs);

			start.X = end.X - GuardRowParameters.GuardRowFoundationLength;
			start.X += xAxisRackDarwingLimitation;
			end.Y = -GuardRowParameters.GuardRowFoundationHeight;
            if (!showHorizontalGuardOnly)
				_DrawRectangle(dc, brush, borderPen, start, end, cs);

			// right guard foundation
			start.Y = end.Y;
			end.Y = 0;
            if (!showHorizontalGuardOnly)
				_DrawRectangle(dc, Brushes.White, borderPen, start, end, cs);
		}

		private static void _DrawRowGuard(Point start, DrawingContext dc, ICoordinateSystem cs, RackAdvancedDrawingSettings displaySettings, 
			Pen borderPen, Color mainColor, Color secondaryColor, bool showHeightDimensions = false, bool showWidthAndOffset = false, bool isRightSideRack = false, bool isUnderpass = false)
		{
			Point end;

			// foundary binding
			end = new Point(start.X + GuardRowParameters.GuardRowFoundationWidth, -GuardRowParameters.GuardRowFoundationHeight);
			_DrawRectangle(dc, Brushes.White, borderPen, start, end, cs);

			Point offsetDimension = new Point(start.X, start.X + GuardRowParameters.GuardColumnSuppotOffset + GuardRowParameters.GuardRowFoundationWidth);
			if (isUnderpass)
			{
                offsetDimension.X -= GuardRowParameters.GuardColumnSuppotOffset;
                offsetDimension.Y -= GuardRowParameters.GuardColumnSuppotOffset;
            }
			// suport triangles
			start.Y -= GuardRowParameters.GuardRowFoundationHeight;

			double foundationSupportHeight = GuardRowParameters.GuardRowFoundationSupportHeight + GuardRowParameters.GuardRowFoundationHeight;

			_DrawGeometry(dc, cs, borderPen, Brushes.White, new Point[] {
				start,
				new Point(start.X + GuardRowParameters.GuardColumnSuppotOffset, -foundationSupportHeight),
				new Point(start.X + GuardRowParameters.GuardColumnSuppotOffset, -GuardRowParameters.GuardRowFoundationHeight)
			});

            _DrawGeometry(dc, cs, borderPen, Brushes.White, new Point[] {
                new Point(start.X + GuardRowParameters.GuardColumnSuppotOffset + GuardRowParameters.GuardRowWidth, -GuardRowParameters.GuardRowFoundationHeight),
				new Point(start.X + GuardRowParameters.GuardColumnSuppotOffset + GuardRowParameters.GuardRowWidth, -foundationSupportHeight),
				new Point(start.X + (2 * GuardRowParameters.GuardColumnSuppotOffset) + GuardRowParameters.GuardRowWidth, -GuardRowParameters.GuardRowFoundationHeight)
            });

            // main left guard body
            start.X += GuardRowParameters.GuardColumnSuppotOffset;
			end.X = start.X + GuardRowParameters.GuardRowWidth;
			end.Y = -GuardRowParameters.GuardRowHeight;
			_DrawRectangle(dc, GetStripesBrush(mainColor, secondaryColor), borderPen, start, end, cs);

			if (displaySettings.DisplayTextAndDimensions)
			{
                if (showHeightDimensions)
                {
					eDimensionPlacement heightDim = start.X < 0 ? eDimensionPlacement.eLeft : eDimensionPlacement.eRight;

					// height
					_DrawDimension(dc, 
						new Point(start.X + GuardRowParameters.GuardRowWidth, end.Y), 
						new Point(end.X, 0),
						GuardRowParameters.GuardRowHeight.ToString(),
						7,
						displaySettings.DimensionsTextSize,
						displaySettings.PerpDimLinesOffsetInPixels,
						heightDim,
						cs,
						dimensionBrush: displaySettings.DimensionsBrush,
						drawAdditionalSupportDimLine: true,
						bMirrorTextRelativeToDimLine: false	,
						dimensionTextOffset: 0);
                }

                if (showWidthAndOffset)
                {
					// guard body width
					_DrawDimension(dc, new Point(start.X, end.Y), new Point(end.X, end.Y),
						GuardRowParameters.GuardRowWidth.ToString(),
						7,
						displaySettings.DimensionsTextSize,
						displaySettings.PerpDimLinesOffsetInPixels,
						eDimensionPlacement.eTop,
						cs,
						dimensionBrush: displaySettings.DimensionsBrush);

					// offset
					_DrawDimension(dc, 
						new Point(isRightSideRack ? offsetDimension.X - GuardRowParameters.GuardColumnSuppotOffset : offsetDimension.X, 0), 
						new Point(isRightSideRack ? offsetDimension.Y - GuardRowParameters.GuardColumnSuppotOffset : offsetDimension.Y, 0),
						$"{GuardRowParameters.GuardColumnSuppotOffset + GuardRowParameters.GuardRowFoundationWidth}",
						displaySettings.MinDimensionsLinesOffset,
						displaySettings.DimensionsTextSize,
						displaySettings.PerpDimLinesOffsetInPixels,
						eDimensionPlacement.eBot,
						cs,
						dimensionBrush: displaySettings.DimensionsBrush,
						bMirrorTextRelativeToDimLine: true);
                }
			}
		}

		private static void _DrawGeometry(DrawingContext dc, ICoordinateSystem cs, Pen borderPen, Brush fillBrush, params Point[] points)
        {
			double defaultCameraScale = 1.0;
			Vector defaultCameraOffset = new Vector(0.0, 0.0);

			PathGeometry pathGeom = new PathGeometry();
			PathFigure figure = new PathFigure();

			if (points.Length > 0)
			{
				figure.StartPoint = cs.GetLocalPoint(points[0], defaultCameraScale, defaultCameraOffset);

				for (int i = 1; i < points.Length; i++)
				{
					LineSegment lineSegment = new LineSegment();
					lineSegment.Point = cs.GetLocalPoint(points[i], defaultCameraScale, defaultCameraOffset);

					figure.Segments.Add(lineSegment);
				}

				LineSegment lastLineSegment = new LineSegment();
				lastLineSegment.Point = figure.StartPoint;
				figure.Segments.Add(lastLineSegment);
			}

			pathGeom.Figures.Add(figure);
			
			dc.DrawGeometry(fillBrush, borderPen, pathGeom);
		}

		private static void _DrawBracingLine(DrawingContext dc, ICoordinateSystem cs, Point bracingStartPoint, Point bracingEndPoint, Pen bracingLinePen)
		{
			if (dc == null)
				return;
			if (cs == null)
				return;

			// camera scale and offset are used for calculate screen(control) point based on global drawing point
			double defaultCameraScale = 1.0;
			Vector defaultCameraOffset = new Vector(0.0, 0.0);

			Point bracingtart_ScreenPoint = cs.GetLocalPoint(bracingStartPoint, defaultCameraScale, defaultCameraOffset);
			Point bracingEnd_ScreenPoint = cs.GetLocalPoint(bracingEndPoint, defaultCameraScale, defaultCameraOffset);
			dc.DrawLine(bracingLinePen, bracingtart_ScreenPoint, bracingEnd_ScreenPoint);
		}
		private static void _DrawRectangle(DrawingContext dc, Brush rectFillBrush, Pen rectBorderPen, Point rectStartGlobalPnt, Point rectEndGlobalPnt, ICoordinateSystem cs)
		{
			if (dc == null)
				return;

			if (cs == null)
				return;

			// camera scale and offset are used for calculate screen(control) point based on global drawing point
			double defaultCameraScale = 1.0;
			Vector defaultCameraOffset = new Vector(0.0, 0.0);

			Point startPnt = cs.GetLocalPoint(rectStartGlobalPnt, defaultCameraScale, defaultCameraOffset);
			Point endPnt = cs.GetLocalPoint(rectEndGlobalPnt, defaultCameraScale, defaultCameraOffset);
			dc.DrawRectangle(rectFillBrush, rectBorderPen, new Rect(startPnt, endPnt));
		}

		private static DrawingBrush GetStripesBrush(Color main, Color secondary)
		{
			//========================Pattern1 - layout=======================
			#region path
			PathFigure layout = new PathFigure();
			layout.StartPoint = new Point(0, 0);
			layout.Segments.Add(new LineSegment(new Point(80, 0), false));
			layout.Segments.Add(new LineSegment(new Point(80, 80), false));
			layout.Segments.Add(new LineSegment(new Point(0, 80), false));
			#endregion

			PathGeometry blackFonPath = new PathGeometry(new PathFigure[] { layout });

			//========================Pattern2 - stripes===============

			#region path2
			PathFigure centerLine = new PathFigure();
			centerLine.StartPoint = new Point(0, 0);
			centerLine.Segments.Add(new LineSegment(new Point(10, 0), false));
			centerLine.Segments.Add(new LineSegment(new Point(80, 70), false));
			centerLine.Segments.Add(new LineSegment(new Point(80, 80), false));
			centerLine.Segments.Add(new LineSegment(new Point(70, 80), false));
			centerLine.Segments.Add(new LineSegment(new Point(0, 10), false));
			centerLine.Segments.Add(new LineSegment(new Point(0, 0), false));

			PathFigure upperCenterLine = new PathFigure();
			upperCenterLine.StartPoint = new Point(30, 0);
			upperCenterLine.Segments.Add(new LineSegment(new Point(50, 0), false));
			upperCenterLine.Segments.Add(new LineSegment(new Point(80, 30), false));
			upperCenterLine.Segments.Add(new LineSegment(new Point(80, 50), false));
			upperCenterLine.Segments.Add(new LineSegment(new Point(30, 0), false));

			PathFigure lowerCenterLine = new PathFigure();
			lowerCenterLine.StartPoint = new Point(0, 30);
			lowerCenterLine.Segments.Add(new LineSegment(new Point(50, 80), false));
			lowerCenterLine.Segments.Add(new LineSegment(new Point(30, 80), false));
			lowerCenterLine.Segments.Add(new LineSegment(new Point(0, 50), false));
			lowerCenterLine.Segments.Add(new LineSegment(new Point(0, 30), false));

			PathFigure upperCorner = new PathFigure();
			upperCorner.StartPoint = new Point(0, 70);
			upperCorner.Segments.Add(new LineSegment(new Point(0, 80), false));
			upperCorner.Segments.Add(new LineSegment(new Point(10, 80), false));
			upperCorner.Segments.Add(new LineSegment(new Point(0, 70), false));

			PathFigure lowerCorner = new PathFigure();
			lowerCorner.StartPoint = new Point(70, 0);
			lowerCorner.Segments.Add(new LineSegment(new Point(80, 10), false));
			lowerCorner.Segments.Add(new LineSegment(new Point(80, 0), false));
			lowerCorner.Segments.Add(new LineSegment(new Point(70, 0), false));
			#endregion

			PathGeometry stripesPath = new PathGeometry(new PathFigure[] {
				lowerCorner,
				lowerCenterLine,
				centerLine,
				upperCenterLine,
				upperCorner
			});

			//========================Geometry Groups=======================
			GeometryGroup blackGroup = new GeometryGroup();
			blackGroup.FillRule = FillRule.Nonzero;
			blackGroup.Children.Add(blackFonPath);

			GeometryGroup stripeGroup = new GeometryGroup();
			stripeGroup.FillRule = FillRule.Nonzero;
			stripeGroup.Children.Add(stripesPath);

			//========================Geometry Drawing=======================
			GeometryDrawing black = new GeometryDrawing();
			black.Brush = new SolidColorBrush(main);
			black.Geometry = blackGroup;

			GeometryDrawing stripes = new GeometryDrawing();
			stripes.Brush = new SolidColorBrush(secondary);
			stripes.Geometry = stripeGroup;

			//========================Group=======================
			DrawingGroup dgr = new DrawingGroup();
			dgr.Children.Add(black);
			dgr.Children.Add(stripes);

			//========================Brush=======================
			DrawingBrush db = new DrawingBrush();
			db.Stretch = Stretch.UniformToFill;
			db.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
			db.Viewport = new Rect(0, 0, 1, 1);
			db.TileMode = TileMode.Tile;
			db.Drawing = dgr;

			return db;
		}

		public enum eDimensionPlacement : int
		{
			eBot = 1,
			eTop,
			eLeft,
			eRight
		}
		/// <summary>
		/// Draw dimension
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="globalPnt_01"></param>
		/// <param name="globalPnt_02"></param>
		/// <param name="dimensionText"></param>
		/// <param name="supportLineOffset"></param>
		/// <param name="dimPlacement"></param>
		/// <param name="drawingSize"></param>
		/// <param name="imageSize"></param>
		/// <param name="cs"></param>
		/// <param name="bBreakLine">
		/// If true, then dimension line is broken in the middle and break symbol is drawn.
		/// </param>
		public static void _DrawDimension(
			DrawingContext dc,
			Point globalPnt_01,
			Point globalPnt_02,
			string dimensionText,
			double supportLineOffset,
			double textSize,
			double perpDimLineOffsetInPixels, 
			eDimensionPlacement dimPlacement,
			ICoordinateSystem cs,
			bool bBreakLine = false,
			Brush dimensionBrush = null,
			double textRotateAngleDegrees = 0.0,
			bool bMirrorTextRelativeToDimLine = false,
			double dimensionTextOffset = 0.0,
			bool drawAdditionalSupportDimLine = false)
		{
			if (dc == null)
				return;

			if (cs == null)
				return;

			Brush dimBrush = Brushes.DarkBlue;
			if (dimensionBrush != null)
				dimBrush = dimensionBrush;
			Pen dimPen = new Pen(dimBrush, 1.0);

			FormattedText fmtedText = new FormattedText(dimensionText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, m_TextTypeFace, textSize, dimBrush);

			//
			double textLength = fmtedText.Width;
			double textHeight = fmtedText.Height;

			//
			double dimLineOffsetGlobalUnits = 0.0;
			if (eDimensionPlacement.eBot == dimPlacement || eDimensionPlacement.eTop == dimPlacement)
				dimLineOffsetGlobalUnits = cs.GetGlobalHeight(supportLineOffset, 1.0);
			else
				dimLineOffsetGlobalUnits = cs.GetGlobalWidth(supportLineOffset, 1.0);

			// Text offset from dimension line
			double textOffsetGlobalUnits = 0.0;
			if (eDimensionPlacement.eBot == dimPlacement || eDimensionPlacement.eTop == dimPlacement)
				textOffsetGlobalUnits = cs.GetGlobalHeight(m_MinDimTextOffsetInPixels, 1.0);
			else
				textOffsetGlobalUnits = cs.GetGlobalWidth(m_MinDimTextOffsetInPixels, 1.0);

			// 
			double perpDimLineOffsetGlobalUnits = 0.0;
			if (eDimensionPlacement.eBot == dimPlacement || eDimensionPlacement.eTop == dimPlacement)
				perpDimLineOffsetGlobalUnits = cs.GetGlobalHeight(perpDimLineOffsetInPixels, 1.0);
			else
				perpDimLineOffsetGlobalUnits = cs.GetGlobalWidth(perpDimLineOffsetInPixels, 1.0);

			// Minimum support line offset in global units(not pixels)
			double calcMinSuppLineLength = dimLineOffsetGlobalUnits + textOffsetGlobalUnits + perpDimLineOffsetGlobalUnits;

			// global points for draw line
			Dictionary<int, List<Point>> lines = new Dictionary<int, List<Point>>();
			// top left point of text
			Point textDrawGlobalPnt = globalPnt_01;
			//
			double rotateAngleDegrees = 0.0;
			//
			if(eDimensionPlacement.eBot == dimPlacement)
			{
				//
				Point supportLineStart_GlobaltPnt = globalPnt_01;
				Point supportLineEnd_GlobalPnt = supportLineStart_GlobaltPnt;
				supportLineEnd_GlobalPnt.Y += Math.Max(dimLineOffsetGlobalUnits, calcMinSuppLineLength);
				lines.Add(0, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
				//
				supportLineStart_GlobaltPnt = globalPnt_02;
				supportLineEnd_GlobalPnt = supportLineStart_GlobaltPnt;
				supportLineEnd_GlobalPnt.Y = lines[0][1].Y;
				lines.Add(1, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
				//
				supportLineStart_GlobaltPnt = supportLineEnd_GlobalPnt;
				supportLineStart_GlobaltPnt.Y -= perpDimLineOffsetGlobalUnits;
				supportLineStart_GlobaltPnt.X += perpDimLineOffsetGlobalUnits;
				supportLineEnd_GlobalPnt = supportLineStart_GlobaltPnt;
				supportLineEnd_GlobalPnt.X = globalPnt_01.X;
				supportLineEnd_GlobalPnt.X -= perpDimLineOffsetGlobalUnits;
				lines.Add(2, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });

                if (drawAdditionalSupportDimLine)
                {
					Point startSupportLine = new Point(supportLineStart_GlobaltPnt.X, supportLineStart_GlobaltPnt.Y);
					Point endSupportLine = new Point(supportLineEnd_GlobalPnt.X - dimensionTextOffset, supportLineEnd_GlobalPnt.Y);

                    if (dimensionTextOffset < 0)
                    {
						startSupportLine = new Point(supportLineStart_GlobaltPnt.X - dimensionTextOffset, supportLineStart_GlobaltPnt.Y);
						endSupportLine = new Point(supportLineStart_GlobaltPnt.X , supportLineEnd_GlobalPnt.Y);
					}

					lines.Add(-1, new List<Point>{ startSupportLine, endSupportLine });
				}

				//
				textDrawGlobalPnt = supportLineStart_GlobaltPnt;
				textDrawGlobalPnt.X -= dimensionTextOffset;
				textDrawGlobalPnt += (supportLineEnd_GlobalPnt - supportLineStart_GlobaltPnt) / 2;
				textDrawGlobalPnt.X -= cs.GetGlobalWidth(textLength / 2, 1.0);
				if(bMirrorTextRelativeToDimLine)
					textDrawGlobalPnt.Y -= perpDimLineOffsetGlobalUnits + cs.GetGlobalHeight(textHeight, 1.0);
				else
					textDrawGlobalPnt.Y = lines[0][1].Y;
				// additional lines
				Point tempPnt01 = lines[0][1];
				tempPnt01.Y -= perpDimLineOffsetGlobalUnits;
				supportLineStart_GlobaltPnt = tempPnt01 + perpDimLineOffsetGlobalUnits * new Vector(-1.0, 1.0);
				supportLineEnd_GlobalPnt = tempPnt01 + perpDimLineOffsetGlobalUnits * new Vector(1.0, -1.0);
				lines.Add(3, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
				//
				tempPnt01 = lines[1][1];
				tempPnt01.Y -= perpDimLineOffsetGlobalUnits;
				supportLineStart_GlobaltPnt = tempPnt01 + perpDimLineOffsetGlobalUnits * new Vector(-1.0, 1.0);
				supportLineEnd_GlobalPnt = tempPnt01 + perpDimLineOffsetGlobalUnits * new Vector(1.0, -1.0);
				lines.Add(4, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
			}
			else if(eDimensionPlacement.eTop == dimPlacement)
			{
				//
				Point supportLineStart_GlobaltPnt = globalPnt_01;
				Point supportLineEnd_GlobalPnt = supportLineStart_GlobaltPnt;
				supportLineEnd_GlobalPnt.Y -= Math.Max(dimLineOffsetGlobalUnits, calcMinSuppLineLength);
				lines.Add(0, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
				//
				supportLineStart_GlobaltPnt = globalPnt_02;
				supportLineEnd_GlobalPnt = supportLineStart_GlobaltPnt;
				supportLineEnd_GlobalPnt.Y = lines[0][1].Y;
				lines.Add(1, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
				//
				supportLineStart_GlobaltPnt = supportLineEnd_GlobalPnt;
				supportLineStart_GlobaltPnt.Y += perpDimLineOffsetGlobalUnits;
				supportLineStart_GlobaltPnt.X += perpDimLineOffsetGlobalUnits;
				supportLineEnd_GlobalPnt = supportLineStart_GlobaltPnt;
				supportLineEnd_GlobalPnt.X = globalPnt_01.X;
				supportLineEnd_GlobalPnt.X -= perpDimLineOffsetGlobalUnits;
				lines.Add(2, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });

				if (drawAdditionalSupportDimLine)
				{
					lines.Add(-1, new List<Point>
					{
						new Point(supportLineStart_GlobaltPnt.X + dimensionTextOffset, supportLineStart_GlobaltPnt.Y),
						new Point(supportLineEnd_GlobalPnt.X + dimensionTextOffset, supportLineEnd_GlobalPnt.Y)
					});
				}

				//
				textDrawGlobalPnt = supportLineStart_GlobaltPnt;
				textDrawGlobalPnt.X += dimensionTextOffset;
				textDrawGlobalPnt += (supportLineEnd_GlobalPnt - supportLineStart_GlobaltPnt) / 2;
				textDrawGlobalPnt.X -= cs.GetGlobalWidth(textLength / 2, 1.0);
				if (bMirrorTextRelativeToDimLine)
					textDrawGlobalPnt.Y += perpDimLineOffsetGlobalUnits + cs.GetGlobalHeight(textHeight, 1.0);
				else
					textDrawGlobalPnt.Y -= cs.GetGlobalHeight(textHeight, 1.0);
				// additional lines
				Point tempPnt01 = lines[0][1];
				tempPnt01.Y += perpDimLineOffsetGlobalUnits;
				supportLineStart_GlobaltPnt = tempPnt01 + perpDimLineOffsetGlobalUnits * new Vector(-1.0, 1.0);
				supportLineEnd_GlobalPnt = tempPnt01 + perpDimLineOffsetGlobalUnits * new Vector(1.0, -1.0);
				lines.Add(3, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
				//
				tempPnt01 = lines[1][1];
				tempPnt01.Y += perpDimLineOffsetGlobalUnits;
				supportLineStart_GlobaltPnt = tempPnt01 + perpDimLineOffsetGlobalUnits * new Vector(-1.0, 1.0);
				supportLineEnd_GlobalPnt = tempPnt01 + perpDimLineOffsetGlobalUnits * new Vector(1.0, -1.0);
				lines.Add(4, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
			}
			else if(eDimensionPlacement.eLeft == dimPlacement)
			{
				Point botPnt = globalPnt_01;
				Point topPnt = globalPnt_02;
				if (Utils.FLT(globalPnt_02.Y, globalPnt_01.Y))
				{
					botPnt = globalPnt_02;
					topPnt = globalPnt_01;
				}

				//
				Point supportLineStart_GlobaltPnt = botPnt;
				Point supportLineEnd_GlobalPnt = supportLineStart_GlobaltPnt;
				supportLineEnd_GlobalPnt.X -= Math.Max(dimLineOffsetGlobalUnits, calcMinSuppLineLength);
				lines.Add(0, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
				//
				supportLineStart_GlobaltPnt = topPnt;
				supportLineEnd_GlobalPnt = supportLineStart_GlobaltPnt;
				supportLineEnd_GlobalPnt.X = lines[0][1].X;
				lines.Add(1, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
				//
				supportLineStart_GlobaltPnt = lines[0][1];
				supportLineStart_GlobaltPnt.Y -= perpDimLineOffsetGlobalUnits;
				supportLineStart_GlobaltPnt.X += perpDimLineOffsetGlobalUnits;
				supportLineEnd_GlobalPnt = lines[1][1];
				supportLineEnd_GlobalPnt.X += perpDimLineOffsetGlobalUnits;
				supportLineEnd_GlobalPnt.Y += perpDimLineOffsetGlobalUnits;
				lines.Add(2, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });

				if (drawAdditionalSupportDimLine)
				{
					lines.Add(-1, new List<Point>
					{
						new Point(supportLineEnd_GlobalPnt.X, supportLineStart_GlobaltPnt.Y),
						new Point(supportLineStart_GlobaltPnt.X, supportLineStart_GlobaltPnt.Y + dimensionTextOffset)
					});
				}

				//
				textDrawGlobalPnt = supportLineStart_GlobaltPnt;
                textDrawGlobalPnt.Y += dimensionTextOffset;
                textDrawGlobalPnt += (supportLineEnd_GlobalPnt - supportLineStart_GlobaltPnt) / 2;
				if (bMirrorTextRelativeToDimLine)
					textDrawGlobalPnt.X -= cs.GetGlobalWidth(textLength / 2, 1.0) - cs.GetGlobalHeight(textHeight / 2, 1.0);
				else
					textDrawGlobalPnt.X -= cs.GetGlobalWidth(textLength / 2, 1.0) + cs.GetGlobalHeight(textHeight / 2, 1.0);
				textDrawGlobalPnt.Y -= cs.GetGlobalHeight(textHeight / 2, 1.0);
				//
				rotateAngleDegrees = -90;

				// additional lines
				Point tempPnt01 = lines[0][1];
				tempPnt01.X += perpDimLineOffsetGlobalUnits;
				supportLineStart_GlobaltPnt = tempPnt01 - perpDimLineOffsetGlobalUnits * new Vector(1.0, 1.0);
				supportLineEnd_GlobalPnt = tempPnt01 + perpDimLineOffsetGlobalUnits * new Vector(1.0, 1.0);
				lines.Add(3, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
				//
				tempPnt01 = lines[1][1];
				tempPnt01.X += perpDimLineOffsetGlobalUnits;
				supportLineStart_GlobaltPnt = tempPnt01 - perpDimLineOffsetGlobalUnits * new Vector(1.0, 1.0);
				supportLineEnd_GlobalPnt = tempPnt01 + perpDimLineOffsetGlobalUnits * new Vector(1.0, 1.0);
				lines.Add(4, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
			}
			else if(eDimensionPlacement.eRight == dimPlacement)
			{
				Point botPnt = globalPnt_01;
				Point topPnt = globalPnt_02;
				if (Utils.FLT(globalPnt_02.Y, globalPnt_01.Y))
				{
					botPnt = globalPnt_02;
					topPnt = globalPnt_01;
				}

				//
				Point supportLineStart_GlobaltPnt = botPnt;
				Point supportLineEnd_GlobalPnt = supportLineStart_GlobaltPnt;
				supportLineEnd_GlobalPnt.X += Math.Max(dimLineOffsetGlobalUnits, calcMinSuppLineLength);
				lines.Add(0, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
				//
				supportLineStart_GlobaltPnt = topPnt;
				supportLineEnd_GlobalPnt = supportLineStart_GlobaltPnt;
				supportLineEnd_GlobalPnt.X = lines[0][1].X;
				lines.Add(1, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
				//
				supportLineStart_GlobaltPnt = lines[0][1];
				supportLineStart_GlobaltPnt.Y -= perpDimLineOffsetGlobalUnits;
				supportLineStart_GlobaltPnt.X -= perpDimLineOffsetGlobalUnits;
				supportLineEnd_GlobalPnt = lines[1][1];
				supportLineEnd_GlobalPnt.X -= perpDimLineOffsetGlobalUnits;
				supportLineEnd_GlobalPnt.Y += perpDimLineOffsetGlobalUnits;
				lines.Add(2, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });

				if (drawAdditionalSupportDimLine)
				{
					Point startSupportLine = new Point(supportLineStart_GlobaltPnt.X, supportLineEnd_GlobalPnt.Y);
					Point endSupportLine = new Point(supportLineEnd_GlobalPnt.X, supportLineEnd_GlobalPnt.Y + dimensionTextOffset);

					if (dimensionTextOffset < 0)
                    {
						startSupportLine = new Point(supportLineStart_GlobaltPnt.X, supportLineStart_GlobaltPnt.Y + dimensionTextOffset);
						endSupportLine = new Point(supportLineEnd_GlobalPnt.X, supportLineStart_GlobaltPnt.Y);
					}

					lines.Add(-1, new List<Point> { startSupportLine, endSupportLine });
				}

				//
				textDrawGlobalPnt = supportLineStart_GlobaltPnt;
				textDrawGlobalPnt.Y += dimensionTextOffset;
				textDrawGlobalPnt += (supportLineEnd_GlobalPnt - supportLineStart_GlobaltPnt) / 2;
				if (bMirrorTextRelativeToDimLine)
					textDrawGlobalPnt.X -= cs.GetGlobalWidth(textLength / 2, 1.0) - cs.GetGlobalHeight(textHeight / 2, 1.0);
				else
					textDrawGlobalPnt.X -= cs.GetGlobalWidth(textLength / 2, 1.0) + cs.GetGlobalHeight(textHeight / 2, 1.0);
				textDrawGlobalPnt.Y -= cs.GetGlobalHeight(textHeight / 2, 1.0);
				//
				rotateAngleDegrees = -90;

				// additional lines
				Point tempPnt01 = lines[0][1];
				tempPnt01.X -= perpDimLineOffsetGlobalUnits;
				supportLineStart_GlobaltPnt = tempPnt01 - perpDimLineOffsetGlobalUnits * new Vector(1.0, 1.0);
				supportLineEnd_GlobalPnt = tempPnt01 + perpDimLineOffsetGlobalUnits * new Vector(1.0, 1.0);
				lines.Add(3, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
				//
				tempPnt01 = lines[1][1];
				tempPnt01.X -= perpDimLineOffsetGlobalUnits;
				supportLineStart_GlobaltPnt = tempPnt01 - perpDimLineOffsetGlobalUnits * new Vector(1.0, 1.0);
				supportLineEnd_GlobalPnt = tempPnt01 + perpDimLineOffsetGlobalUnits * new Vector(1.0, 1.0);
				lines.Add(4, new List<Point> { supportLineStart_GlobaltPnt, supportLineEnd_GlobalPnt });
			}

			//
			if (bBreakLine)
			{
				if (eDimensionPlacement.eBot == dimPlacement || eDimensionPlacement.eTop == dimPlacement)
				{
					// remove straight line
					List<Point> straightLinePntsList = lines[2];
					lines.Remove(2);
					// break line in the middle
					Point middlePnt = straightLinePntsList[0];
					middlePnt += (straightLinePntsList[1] - straightLinePntsList[0]) / 2;
					//
					Point startPnt = straightLinePntsList[0];
					Point endPnt = middlePnt;
					endPnt.X -= m_BreakDimensionLineSymbol;
					lines.Add(5, new List<Point> { startPnt, endPnt });
					//
					startPnt = endPnt;
					endPnt.X += m_BreakDimensionLineSymbol / 2;
					endPnt.Y -= m_BreakDimensionLineSymbol;
					lines.Add(6, new List<Point> { startPnt, endPnt });
					//
					startPnt = endPnt;
					endPnt = middlePnt;
					endPnt.X += m_BreakDimensionLineSymbol / 2;
					endPnt.Y += m_BreakDimensionLineSymbol;
					lines.Add(7, new List<Point> { startPnt, endPnt });
					//
					startPnt = endPnt;
					endPnt = middlePnt;
					endPnt.X += m_BreakDimensionLineSymbol;
					lines.Add(8, new List<Point> { startPnt, endPnt });
					//
					startPnt = endPnt;
					endPnt = straightLinePntsList[1];
					lines.Add(9, new List<Point> { startPnt, endPnt });
				}
				else if(eDimensionPlacement.eLeft == dimPlacement || eDimensionPlacement.eRight == dimPlacement)
				{
					// remove straight line
					List<Point> straightLinePntsList = lines[2];
					lines.Remove(2);
					// break line in the middle
					Point middlePnt = straightLinePntsList[0];
					middlePnt += (straightLinePntsList[1] - straightLinePntsList[0]) / 2;
					//
					Point startPnt = straightLinePntsList[0];
					Point endPnt = middlePnt;
					endPnt.Y -= m_BreakDimensionLineSymbol;
					lines.Add(5, new List<Point> { startPnt, endPnt });
					//
					startPnt = endPnt;
					endPnt.Y += m_BreakDimensionLineSymbol / 2;
					endPnt.X -= m_BreakDimensionLineSymbol;
					lines.Add(6, new List<Point> { startPnt, endPnt });
					//
					startPnt = endPnt;
					endPnt = middlePnt;
					endPnt.Y += m_BreakDimensionLineSymbol / 2;
					endPnt.X += m_BreakDimensionLineSymbol;
					lines.Add(7, new List<Point> { startPnt, endPnt });
					//
					startPnt = endPnt;
					endPnt = middlePnt;
					endPnt.Y += m_BreakDimensionLineSymbol;
					lines.Add(8, new List<Point> { startPnt, endPnt });
					//
					startPnt = endPnt;
					endPnt = straightLinePntsList[1];
					lines.Add(9, new List<Point> { startPnt, endPnt });
				}
			}

			// camera scale and offset are used for calculate screen(control) point based on global drawing point
			double defaultCameraScale = 1.0;
			Vector defaultCameraOffset = new Vector(0.0, 0.0);

			// draw lines
			foreach (int iLineIndex in lines.Keys)
			{
				List<Point> pnts = lines[iLineIndex];
				if (pnts == null)
					continue;
				if (pnts.Count != 2)
					continue;

				Point imagePnt01 = cs.GetLocalPoint(pnts[0], defaultCameraScale, defaultCameraOffset);
				Point imagePnt02 = cs.GetLocalPoint(pnts[1], defaultCameraScale, defaultCameraOffset);
				dc.DrawLine(dimPen, imagePnt01, imagePnt02);
			}
			// draw text
			Point textPnt = cs.GetLocalPoint(textDrawGlobalPnt, defaultCameraScale, defaultCameraOffset);
			bool bPopTransform = false;
			rotateAngleDegrees += textRotateAngleDegrees;
			if(Utils.FNE(rotateAngleDegrees, 0.0))
			{
				Point rotateOriginPnt = textPnt;
				rotateOriginPnt.X += textLength / 2;
				rotateOriginPnt.Y += textHeight / 2;
				dc.PushTransform(new RotateTransform(rotateAngleDegrees, rotateOriginPnt.X, rotateOriginPnt.Y));
				bPopTransform = true;
			}
			//
			dc.DrawText(fmtedText, textPnt);
			//
			if (bPopTransform)
				dc.Pop();
		}

		//=============================================================================
		/// <summary>
		/// Returns size for rack advanced properties drawing
		/// </summary>
		public static bool _GetSizes(
			Rack rackForDraw,
			RackAdvancedDrawingSettings drawingSettings,
			//
			out Size drawingSize,
			out FormattedText frontViewText,
			out FormattedText sideViewText,
			out double rackLength,
			out double rackWidth,
			out double _MaxRackHeight
			)
		{
			drawingSize = new Size(0.0, 0.0);
			rackLength = 0.0;
			rackWidth = 0.0;
			_MaxRackHeight = 0.0;

			RackAdvancedDrawingSettings settings = drawingSettings;
			if (settings == null)
				settings = RackAdvancedDrawingSettings.GetDefaultSettings();

			Brush textBrush = CurrentGeometryColorsTheme.GetRackAdvProps_TextBrush();

			// calc text size
			string strFrontView = "FRONT VIEW";
			frontViewText = new FormattedText(strFrontView, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, m_TextTypeFace, settings.ViewTextSize, textBrush);
			frontViewText.TextAlignment = TextAlignment.Center;
			string strSideView = "SIDE VIEW";
			sideViewText = new FormattedText(strSideView, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, m_TextTypeFace, settings.ViewTextSize, textBrush);
			sideViewText.TextAlignment = TextAlignment.Center;

			if (rackForDraw == null)
				return false;

			rackLength = rackForDraw.Length;
			rackWidth = rackForDraw.Depth;

			//
			double _ViewsTotalLength = rackLength + rackWidth;
			//_FrontViewCenterOffset_X = rackLength / 2;
			//_SideViewCenterOffset_X = _ViewsTotalLength - rackWidth / 2;
			// Dont use ClearAvailableHeight, because if it has 12000 value and rack height is 4000
			// then rack draws too small.
			// Reserve 10% for draw ClearAvailableHeight.
			_MaxRackHeight = m_ClearAvailableHeightLimitCoef * rackForDraw.MaxHeight;

			drawingSize = new Size(_ViewsTotalLength, _MaxRackHeight);

			return true;
		}
	}
}
