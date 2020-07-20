using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DrawingControl
{
	// Controls that contains children(block, column, rack etc) to draw and operations to manipulate them.
	public class DrawingControl : FrameworkElement, ICoordinateSystem
	{
		/// <summary>
		/// Visual collection with children GeometryWrappers.
		/// GeometryWrappers draw order depends on the position in m_GeometryWrappersCollection.
		/// </summary>
		private VisualCollection m_GeometryWrappersCollection = null;
		private List<GripPoint> m_grips = new List<GripPoint>();
		private GripPoint m_gripToMove = null;

		// Separate list with wrappers for TieBeams.
		// They should draw over all other geometry.
		private List<GeometryWrapper> m_TieBeamsWrappersList = new List<GeometryWrapper>();

		// coordinates of vertical and horizontal snap lines
		// if lines coordiantes is less 0, then no need to show them
		private double m_rVertSnapLineX = -1;
		private double m_rHorizSnapLineY = -1;

		// Draws snap lines
		private SnappingLinesControl m_SnappingLines = null;
		// Draw new rectangle area on changing drawing Length or Width
		// if there are some rectangles which are outside area and will be deleted.
		private PreviewBordersControl m_PreviewBordersControl = null;
		// Draws watermarks images in given rectangle area.
		// Call it and draw watermarks over other graphics.
		private WatermarkVisual m_WatermarkVisual = null;
		// Draws selection rectangle when user holds mouse left button and move mouse.
		private SelectionRectangleVisual m_SelectionRectVisual = null;
		// Draws distance to borders and dimensions for selected geometry
		private SelectedGeometryInfoVisual m_SelectedGeomInfoVisual = null;
		// Displays ToolTipText near mouse point
		private ToolTipVisual m_ToolTipVisual = null;
		// Displays tie beam groups(racks groups) which are used for calculate - should tie beam be placed?
		private TieBeamGroupsVisual m_TieBeamGroupsVisual = null;

		// If true, then on mouse left button click if user clicks on geometry
		// pass it to the current sheet MatchProperties() method wuthout clear selection.
		private bool m_bInGeomMathPropertiesCommand = false;
		// If property source rack and target rack have different depth in rack match properties command
		// then dialog with "Arte you sure?" text is displayed. Dialog has 3 buttons - OK, SKIP, CANCEL.
		// And "Remember the choice" combobox.
		// Only two states is need to remember - OK and SKIP. Because CANCEL will cancel command.
		// If this flag is true then dont display dialog and use sGeomMatchPropertiesCommand_MatchIfDepthDiff if rack's depth is different.
		public static bool sGeomMathPropertiesCommand_RememberTheChoice = false;
		// true - match properties if source and target rack's depth is different
		// false - skip
		public static bool sGeomMatchPropertiesCommand_MatchIfDepthDiff = false;
		
		// Is run Move command
		private bool m_bInSelectionMove = false;
		// m_PrevSelectionMovePoint contains actual value, need for moving selection by any point
		private bool m_bIsPrevMoveSelectionPointSet = false;
		// Previos move point, need to calculate move offset
		private Point m_PrevSelectionMovePoint = new Point();

		/// <summary>
		/// If true then need to select current sheet camera position and scale by selecting rectangle.
		/// </summary>
		private bool m_bInZoomWindowCommand = false;
		
		//
		private bool m_bCreateWall = false;

		/// <summary>
		/// Global point in DrawingSheet coordinates where mouse wheel button was pressed.
		/// It is used for m_TempCameraOffsetVector calculation.
		/// </summary>
		private Point m_MouseWheelButtonPressedPoint = new Point(0.0, 0.0);

		/// <summary>
		/// Size in pixels of the displayed sheet. Sheet is scaled and fully fit in the DrawingControl area.
		/// </summary>
		private Size m_FullyDisplayedSheetSize = new Size(0.0, 0.0);
		/// <summary>
		/// If displayed sheet is less than DrawingControl area then m_DrawOffset contains offset in pixels from DrawingControl top left point.
		/// It is necessary to display sheet horizontal or vertical center aligned.
		/// </summary>
		private Vector m_DrawOffset = new Vector(0.0, 0.0);

		public DrawingControl()
		{
			// Enable keyboard focus.
			Focusable = true;

			m_GeometryWrappersCollection = new VisualCollection(this);

			//
			m_SnappingLines = new SnappingLinesControl(this);
			this.AddVisualChild(m_SnappingLines);
			this.AddLogicalChild(m_SnappingLines);
			//
			m_PreviewBordersControl = new PreviewBordersControl(this);
			this.AddVisualChild(m_PreviewBordersControl);
			this.AddLogicalChild(m_PreviewBordersControl);

			//
			m_WatermarkVisual = new WatermarkVisual(this);
			this.AddLogicalChild(m_WatermarkVisual);
			this.AddVisualChild(m_WatermarkVisual);

			//
			m_SelectionRectVisual = new SelectionRectangleVisual(this);
			this.AddLogicalChild(m_SelectionRectVisual);
			this.AddVisualChild(m_SelectionRectVisual);

			//
			m_SelectedGeomInfoVisual = new SelectedGeometryInfoVisual(this);
			this.AddLogicalChild(m_SelectedGeomInfoVisual);
			this.AddVisualChild(m_SelectedGeomInfoVisual);

			//
			m_ToolTipVisual = new ToolTipVisual(this);
			this.AddLogicalChild(m_ToolTipVisual);
			this.AddVisualChild(m_ToolTipVisual);

#if DEBUG
			// Display tie beams groups only in DEBUG mode
			m_TieBeamGroupsVisual = new TieBeamGroupsVisual(this);
			this.AddLogicalChild(m_TieBeamGroupsVisual);
			this.AddVisualChild(m_TieBeamGroupsVisual);
#endif

			PreviewGlobalLength = -1;
			PreviewGlobalWidth = -1;

			//
			ToolTipText = string.Empty;
			CanDisplayToolTip = false;

			DrawingDocument._sDrawing = this;

			// cut all child graphics which goes out bounds of this control
			//this.ClipToBounds = true;
		}

		#region Properties

		//=============================================================================
		public Point MouseLocalPoint { get; protected set; }

		//=============================================================================
		public double VertLine_X { get { return m_rVertSnapLineX; } }

		//=============================================================================
		public double HorizLine_Y { get { return m_rHorizSnapLineY; } }

		//=============================================================================
		public int PreviewGlobalLength { get; set; }
		public int PreviewGlobalWidth { get; set; }

		//=============================================================================
		// The first point of selection rectangle in global current sheet coordinates.
		public Point SelectionFirstGlobalPnt { get; protected set; }
		//=============================================================================
		// The second point of selection rectangle in global current sheet coordinates.
		public Point SelectionSecondGlobalPnt { get; protected set; }
		//=============================================================================
		// If true then SelectionRectangleVisual draws selection rectangle from SelectionFirstGlobalPnt to SelectionSecondGlobalPnt.
		public bool DisplaySelectionRectangle { get; protected set; }

		//=============================================================================
		public bool CanDisplayToolTip { get; protected set; }
		//=============================================================================
		public string ToolTipText { get; protected set; }

		#endregion

		#region Dependency Properties

		//=============================================================================
		public static readonly DependencyProperty SheetProperty = DependencyProperty.Register(
			"Sheet",
			typeof(DrawingSheet),
			typeof(DrawingControl),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, On_Sheet_Changed));
		//
		public DrawingSheet Sheet
		{
			get { return (DrawingSheet)GetValue(DrawingControl.SheetProperty); }
			set { SetValue(DrawingControl.SheetProperty, value); }
		}
		//
		private static void On_Sheet_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DrawingControl dc = d as DrawingControl;
			if (dc != null)
			{
				// call MeasureOverride and ArrangeOverride for calc new DrawingControl size, because new sheet can has different length and width
				dc.InvalidateVisual();
				dc.InvalidateArrange();
				dc.InvalidateMeasure();
				dc.UpdateLayout();
				//
				dc.ResetGrips();
				dc.UpdateDrawing(true);
			}
		}
		//
		private void On_Sheet_Changed(DrawingSheet oldSheet, DrawingSheet newSheet)
		{
			if(oldSheet != null)
				oldSheet.PropertyChanged -= OnSheetPropertyChanged;
			if (newSheet != null)
				newSheet.PropertyChanged += OnSheetPropertyChanged;
		}

		//=============================================================================
		public static readonly DependencyProperty WatermarkImageProperty =
				DependencyProperty.Register("WatermarkImage",
				typeof(ImageSource),
				typeof(DrawingControl),
				new FrameworkPropertyMetadata(null,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public ImageSource WatermarkImage
		{
			get { return (ImageSource)GetValue(WatermarkImageProperty); }
			set { SetValue(WatermarkImageProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Tooltip background brush.
		/// It is displayed in Rack Match Properties command.
		/// </summary>
		public static readonly DependencyProperty TooltipBackgroundBrushProperty =
				DependencyProperty.Register("TooltipBackgroundBrush",
				typeof(Brush),
				typeof(DrawingControl),
				new FrameworkPropertyMetadata(Brushes.DimGray,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush TooltipBackgroundBrush
		{
			get { return (Brush)GetValue(TooltipBackgroundBrushProperty); }
			set { SetValue(TooltipBackgroundBrushProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Tooltip text brush.
		/// It is displayed in Rack Match Properties command.
		/// </summary>
		public static readonly DependencyProperty TooltipTextBrushProperty =
				DependencyProperty.Register("TooltipTextBrush",
				typeof(Brush),
				typeof(DrawingControl),
				new FrameworkPropertyMetadata(Brushes.LightCyan,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush TooltipTextBrush
		{
			get { return (Brush)GetValue(TooltipTextBrushProperty); }
			set { SetValue(TooltipTextBrushProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Drawing area border brush
		/// </summary>
		public static readonly DependencyProperty BorderBrushProperty =
				DependencyProperty.Register("BorderBrush",
				typeof(Brush),
				typeof(DrawingControl),
				new FrameworkPropertyMetadata(Brushes.Black,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush BorderBrush
		{
			get { return (Brush)GetValue(BorderBrushProperty); }
			set { SetValue(BorderBrushProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Snapping lines brush
		/// </summary>
		public static readonly DependencyProperty SnappingLinesBrushProperty =
				DependencyProperty.Register("SnappingLinesBrush",
				typeof(Brush),
				typeof(DrawingControl),
				new FrameworkPropertyMetadata(Brushes.LightSalmon,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush SnappingLinesBrush
		{
			get { return (Brush)GetValue(SnappingLinesBrushProperty); }
			set { SetValue(SnappingLinesBrushProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Selection rectangle fill brush
		/// </summary>
		public static readonly DependencyProperty SelectionRectangleFillBrushProperty =
				DependencyProperty.Register("SelectionRectangleFillBrush",
				typeof(Brush),
				typeof(DrawingControl),
				new FrameworkPropertyMetadata(Brushes.Green,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush SelectionRectangleFillBrush
		{
			get { return (Brush)GetValue(SelectionRectangleFillBrushProperty); }
			set { SetValue(SelectionRectangleFillBrushProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Selection rectangle border brush
		/// </summary>
		public static readonly DependencyProperty SelectionRectangleBorderBrushProperty =
				DependencyProperty.Register("SelectionRectangleBorderBrush",
				typeof(Brush),
				typeof(DrawingControl),
				new FrameworkPropertyMetadata(Brushes.Black,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush SelectionRectangleBorderBrush
		{
			get { return (Brush)GetValue(SelectionRectangleBorderBrushProperty); }
			set { SetValue(SelectionRectangleBorderBrushProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Selected geometry info graphic color: lines from top left corner to the borders of drawing area, distance values text color, dimensions lines, dimensions text.
		/// </summary>
		public static readonly DependencyProperty SelectedGeometryInfoBrushProperty =
				DependencyProperty.Register("SelectedGeometryInfoBrush",
				typeof(Brush),
				typeof(DrawingControl),
				new FrameworkPropertyMetadata(Brushes.SteelBlue,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush SelectedGeometryInfoBrush
		{
			get { return (Brush)GetValue(SelectedGeometryInfoBrushProperty); }
			set { SetValue(SelectedGeometryInfoBrushProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Display new drawing size if drawing width or height will be decreased and some geometry will be deleted.
		/// Fill color of that displayed new size rectangle.
		/// </summary>
		public static readonly DependencyProperty NewSizePreviewFillBrushProperty =
				DependencyProperty.Register("NewSizePreviewFillBrush",
				typeof(Brush),
				typeof(DrawingControl),
				new FrameworkPropertyMetadata(Brushes.Green,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush NewSizePreviewFillBrush
		{
			get { return (Brush)GetValue(NewSizePreviewFillBrushProperty); }
			set { SetValue(NewSizePreviewFillBrushProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Display new drawing size if drawing width or height will be decreased and some geometry will be deleted.
		/// Border color of that displayed new size rectangle.
		/// </summary>
		public static readonly DependencyProperty NewSizePreviewBorderBrushProperty =
				DependencyProperty.Register("NewSizePreviewBorderBrush",
				typeof(Brush),
				typeof(DrawingControl),
				new FrameworkPropertyMetadata(Brushes.Black,
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush NewSizePreviewBorderBrush
		{
			get { return (Brush)GetValue(NewSizePreviewBorderBrushProperty); }
			set { SetValue(NewSizePreviewBorderBrushProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Mouse cursor point position in sheet global coordinates.
		/// </summary>
		public static readonly DependencyProperty MouseGlobalPointProperty = DependencyProperty.Register(
			"MouseGlobalPoint",
			typeof(Point),
			typeof(DrawingControl),
			new FrameworkPropertyMetadata(new Point(0.0, 0.0))
			);
		//
		public Point MouseGlobalPoint
		{
			get { return (Point)GetValue(DrawingControl.MouseGlobalPointProperty); }
			set { SetValue(DrawingControl.MouseGlobalPointProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// Sheet background fill brush
		/// </summary>
		public static readonly DependencyProperty SheetBackgroundBrushProperty =
				DependencyProperty.Register("SheetBackgroundBrush",
				typeof(Brush),
				typeof(DrawingControl),
				new FrameworkPropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5F4FC")),
						FrameworkPropertyMetadataOptions.AffectsRender |
						FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		//
		public Brush SheetBackgroundBrush
		{
			get { return (Brush)GetValue(SheetBackgroundBrushProperty); }
			set { SetValue(SheetBackgroundBrushProperty, value); }
		}

		#endregion

		#region ICoordinateSystem implementation

		//=============================================================================
		public Point GetLocalPoint(DrawingSheet sheet, Point globalPoint)
		{
			return this.GetLocalPoint( globalPoint, sheet.UnitsPerCameraPixel, sheet.GetCameraOffset());
		}
		//
		public Point GetLocalPoint(Point globalPoint, double UnitsPerCameraPixel, Vector cameraOffset)
		{
			DrawingSheet currSheet = this.Sheet;
			if (currSheet == null)
				return new Point(0.0, 0.0);

			Point localPnt = CoordinateSystemConverter.sGetLocalPoint(globalPoint, UnitsPerCameraPixel, cameraOffset);
			localPnt += m_DrawOffset;
			return localPnt;
		}
		//
		public Point GetGlobalPoint(DrawingSheet sheet, Point pointOnControl)
		{
			DrawingSheet currSheet = this.Sheet;
			if (currSheet == null)
				return new Point(0.0, 0.0);

			Point localPnt = pointOnControl;
			localPnt -= m_DrawOffset;
			return CoordinateSystemConverter.sGetGlobalPoint(localPnt, sheet.UnitsPerCameraPixel, sheet.GetCameraOffset());
		}

		//=============================================================================
		public double GetWidthInPixels(double globalWidthValue, double UnitsPerCameraPixel)
		{
			return CoordinateSystemConverter._sConvertToScreenLength(globalWidthValue, UnitsPerCameraPixel);
		}
		//=============================================================================
		public double GetGlobalWidth(double widthInPixels, double UnitsPerCameraPixel)
		{
			return CoordinateSystemConverter._sConvertToGlobalLength(widthInPixels, UnitsPerCameraPixel);
		}

		//=============================================================================
		public double GetHeightInPixels(double globalHeightValue, double UnitsPerCameraPixel)
		{
			return CoordinateSystemConverter._sConvertToScreenWidth(globalHeightValue, UnitsPerCameraPixel);
		}
		//=============================================================================
		public double GetGlobalHeight(double heightInPixels, double UnitsPerCameraPixel)
		{
			return CoordinateSystemConverter._sConvertToGlobalWidth(heightInPixels, UnitsPerCameraPixel);
		}

		#endregion

		#region Overrides

		//=============================================================================
		protected override int VisualChildrenCount
		{
			get
			{
				int iChildrenCount = 0;
				if (m_SnappingLines != null)
					++iChildrenCount;

				if(m_GeometryWrappersCollection != null)
					iChildrenCount += m_GeometryWrappersCollection.Count;

				if (m_TieBeamsWrappersList != null)
					iChildrenCount += m_TieBeamsWrappersList.Count;

				if (m_TieBeamGroupsVisual != null)
					++iChildrenCount;

				if (m_grips != null)
					iChildrenCount += m_grips.Count;

				if (m_PreviewBordersControl != null)
					++iChildrenCount;

				if (m_SelectedGeomInfoVisual != null)
					++iChildrenCount;

				if (m_SelectionRectVisual != null)
					++iChildrenCount;

				if (m_WatermarkVisual != null)
					++iChildrenCount;

				if (m_ToolTipVisual != null)
					++iChildrenCount;

				return iChildrenCount;
			}
		}

		//=============================================================================
		protected override Visual GetVisualChild(int index)
		{
			int offset = 0;

			//
			if (index == offset)
				return m_SnappingLines;
			++offset;

			// geometry
			if (offset <= index && index - offset < m_GeometryWrappersCollection.Count)
				return m_GeometryWrappersCollection[index - offset];
			offset += m_GeometryWrappersCollection.Count;

			// tie beams
			int iTieBeamsCount = 0;
			iTieBeamsCount = this.m_TieBeamsWrappersList.Count;
			if (index >= offset && index - offset < iTieBeamsCount)
				return m_TieBeamsWrappersList[index - offset];
			offset += iTieBeamsCount;

			// tie beam groups
			if (index == offset && m_TieBeamGroupsVisual != null)
				return m_TieBeamGroupsVisual;
			++offset;

			//
			if (index >= offset && index - offset < m_grips.Count)
				return m_grips[index - offset];
			offset += m_grips.Count;

			//
			if (index == offset)
				return m_PreviewBordersControl;
			++offset;

			if (index == offset)
				return m_SelectedGeomInfoVisual;
			++offset;

			if (index == offset)
				return m_SelectionRectVisual;
			++offset;

			// draw watermarks over all other graphics
			if (index == offset)
				return m_WatermarkVisual;
			++offset;

			// draw tooltip over all
			if (index == offset)
				return m_ToolTipVisual;
			++offset;

			return null;
		}

		//=============================================================================
		protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
		{
			Point pt = hitTestParameters.HitPoint;

			// Perform custom actions during the hit test processing,
			// which may include verifying that the point actually
			// falls within the rendered content of the visual.

			// Return hit on bounding rectangle of visual object.
			return new PointHitTestResult(this, pt);
		}

		//=============================================================================
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			this.CanDisplayToolTip = true;
		}

		//=============================================================================
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			this.CanDisplayToolTip = false;
			this.UpdateDrawing(true);

			// check selection rectangle
			DrawingSheet currSheet = Sheet;
			if (currSheet == null)
				return;

			DrawingDocument currDoc = currSheet.Document;
			if (currDoc == null)
				return;

			// if user select by rectangle
			if (this.DisplaySelectionRectangle)
			{
				this.DisplaySelectionRectangle = false;
				//
				Point mousePoint = e.GetPosition(this);
				this.SelectionSecondGlobalPnt = this.GetGlobalPoint(currSheet, mousePoint);

				if (m_bInZoomWindowCommand)
				{
					this.CameraZoom(currSheet, this.SelectionFirstGlobalPnt, this.SelectionSecondGlobalPnt);
					currSheet.MarkStateChanged();
					_Cancel();
					return;
				}
				else
				{
					currSheet.SelectByRectangle(this.SelectionFirstGlobalPnt, this.SelectionSecondGlobalPnt);

					// pass selection to rack match properties command
					if (m_bInGeomMathPropertiesCommand && currDoc.PropertySourceRack != null)
						currSheet.RackMatchProperties(currDoc.PropertySourceRack, currSheet.SelectedGeometryCollection.ToList());
				}
			}
		}

		//=============================================================================
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (!this.IsEnabled)
				return;

			DrawingSheet currSheet = Sheet;
			if (currSheet == null)
				return;

			DrawingDocument currDoc = currSheet.Document;
			if (currDoc == null)
				return;

			// ignore any click in graphics area when advanced properties are editing
			// there is rack's front view in graphics area
			if (currDoc.ShowAdvancedProperties)
				return;

			Point mousePoint = e.GetPosition(this);
			this.MouseLocalPoint = mousePoint;
			//
			this.CanDisplayToolTip = true;
			if (Utils.FGT(mousePoint.X, this.ActualWidth) || Utils.FGT(mousePoint.Y, this.ActualHeight))
				this.CanDisplayToolTip = false;

			// user should be able click\move the second point of the selection rectangle outside control, 
			// so get the SelectionSecondGlobalPnt before checking borders for local mouse point
			if (e.LeftButton == MouseButtonState.Pressed && (currSheet.SelectedGeometryCollection.Count == 0 || m_bInZoomWindowCommand))
			{
				this.DisplaySelectionRectangle = true;
				this.SelectionSecondGlobalPnt = this.GetGlobalPoint(currSheet, mousePoint);
			}

			//
			if (mousePoint.X < 0)
				mousePoint.X = 0;
			if (mousePoint.X > this.ActualWidth)
				mousePoint.X = this.ActualWidth;
			if (mousePoint.Y < 0)
				mousePoint.Y = 0;
			if (mousePoint.Y > this.ActualHeight)
				mousePoint.Y = this.ActualHeight;

			//
			m_rHorizSnapLineY = -1;
			m_rVertSnapLineX = -1;

			// Dont use CameraOffseet and TemporaryCameraOffset if you want to get global point here.
			// Otherwise, when user holds mouse wheel button and changes camera position then DrawingSheet.TemporaryCameraOffset is changing.
			// But user still changes camera position and DrawingSheet.TemporaryCameraOffset affects on calculating global point.
			// Try to change camera position slowly, image flicks.
			Point globalPoint = CoordinateSystemConverter.sGetGlobalPoint(mousePoint - m_DrawOffset, currSheet.UnitsPerCameraPixel, currSheet.CameraOffset);
			this.MouseGlobalPoint = globalPoint;

			// Move displayed area.
			if (e.MiddleButton == MouseButtonState.Pressed)
			{
				// Dont change camera scale if user creates the wall.
				// User should see entire sheet area.
				if (m_bCreateWall)
					return;

				// Dont do anything if sheet is fully displayed.
				// It doesnt have any affect on the displayed image.
				if (currSheet.IsSheetFullyDisplayed)
					return;

				currSheet.TemporaryCameraOffset = m_MouseWheelButtonPressedPoint - globalPoint;
				currSheet.CheckCameraOffsetVector(this.ActualWidth, this.ActualHeight);

				this.InvalidateVisual();
				return;
			}

			// if create wall command is running
			if (m_bCreateWall)
			{
				eWallPosition wallPos = _GetWallPosition(globalPoint);
				if (eWallPosition.eUndefined == wallPos)
					return;

				if (currSheet.Rectangles.FirstOrDefault(r => r is Wall && r.IsInit && ((Wall)r).WallPosition == wallPos) != null)
					return;

				Wall notInitWall = currSheet.Rectangles.FirstOrDefault(r => r is Wall && !r.IsInit) as Wall;
				if (notInitWall == null)
					return;

				notInitWall.WallPosition = wallPos;

				UpdateDrawing(true);
				return;
			}

			//
			double snapLineX = -1;
			double snapLineY = -1;
			//
			if (true)//(IsSnappingEnabled)
			{
				BaseRectangleGeometry geomToMove = null;
				// If user dragging rectangle(change its position using center grip point) then need calculate best snap point using
				// rectangles corners.
				if (currSheet.SelectedGeometryCollection.Count == 1 && m_gripToMove != null && m_gripToMove.Index == BaseRectangleGeometry.GRIP_CENTER)
					geomToMove = currSheet.SelectedGeometryCollection[0];

				if (geomToMove != null)
				{
					Point snapCenterGlobalPoint;
					if (currSheet._GetBestCenterSnapPoint(geomToMove, globalPoint, out snapCenterGlobalPoint, out snapLineX, out snapLineY))
						globalPoint = snapCenterGlobalPoint;
				}
				else
				{
					Point snapGlobalPoint;
					if (currSheet._GetSnapPoint(globalPoint, out snapGlobalPoint, out snapLineX, out snapLineY))
						globalPoint = snapGlobalPoint;
				}
			}

			// User call Move Selection command
			if (m_bInSelectionMove)
			{
				// display snap lines
				m_rVertSnapLineX = snapLineX;
				m_rHorizSnapLineY = snapLineY;

				if (m_bIsPrevMoveSelectionPointSet)
				{
					// Dont pass globalPoint in Move command because globalPoint is corrected and can be outisde graphics area.
					// But Move command can process point wich lies outside GraphicsArea.
					Vector moveOffset = this.GetGlobalPoint(currSheet, e.GetPosition(this)) - m_PrevSelectionMovePoint;
					Vector appliedOffset;
					if (currSheet.MoveSelection(moveOffset, false, out appliedOffset))
						m_PrevSelectionMovePoint = m_PrevSelectionMovePoint + appliedOffset;
				}
			}

			if (m_gripToMove != null)
			{
				//
				m_rVertSnapLineX = snapLineX;
				m_rHorizSnapLineY = snapLineY;

				//
				if (m_gripToMove is CreateColumnPattern_GripPoint)
					currSheet.CreateColumnPattern(m_gripToMove.Geometry as Column, globalPoint, currSheet.Length, currSheet.Width);
				else
					m_gripToMove.Move(globalPoint, currSheet.Length, currSheet.Width);
			}

			UpdateDrawing(false);
		}

		//=============================================================================
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			if (!this.IsEnabled)
				return;

			DrawingSheet currSheet = Sheet;
			if (currSheet == null)
				return;

			DrawingDocument currDoc = currSheet.Document;
			if (currDoc == null)
				return;

			// ignore any click in graphics area when advanced properties are editing
			// there is rack's front view in graphics area
			if (currDoc.ShowAdvancedProperties)
				return;

			if(!this.IsFocused)
				this.Focus();

			Point mousePoint = e.GetPosition(this);
			this.MouseLocalPoint = mousePoint;

			//
			Point mouseGlobalPoint = this.GetGlobalPoint(currSheet, mousePoint);

			// if create wall command is running
			if (m_bCreateWall)
			{
				eWallPosition wallPos = _GetWallPosition(mouseGlobalPoint);
				if (eWallPosition.eUndefined == wallPos)
				{
					_Cancel();
					return;
				}

				if (currSheet.Rectangles.FirstOrDefault(r => r is Wall && r.IsInit && ((Wall)r).WallPosition == wallPos) != null)
				{
					_Cancel();
					return;
				}

				Wall notInitWall = currSheet.Rectangles.FirstOrDefault(r => r is Wall && !r.IsInit) as Wall;
				if (notInitWall == null)
				{
					_Cancel();
					return;
				}

				notInitWall.WallPosition = wallPos;
				notInitWall.IsInit = true;

				// After wall placement need to check all racks in the sheet, because
				// it is neccessary to have some margin in both X and Y direction between wall and rack.
				List<BaseRectangleGeometry> incorrectGeometryList = new List<BaseRectangleGeometry>();
				if (!currSheet.IsLayoutCorrect(out incorrectGeometryList))
					currSheet.DeleteGeometry(incorrectGeometryList, false, true);

				currSheet.MarkStateChanged();
				_Cancel();
				return;
			}

			this.SelectionFirstGlobalPnt = mouseGlobalPoint;
			if (m_bInZoomWindowCommand)
				return;

			//
			double snapLineX = -1;
			double snapLineY = -1;
			//
			if (true)//(IsSnappingEnabled)
			{
				BaseRectangleGeometry geomToMove = null;
				// If user dragging rectangle(change its position using center grip point) then need calculate best snap point using
				// rectangles corners.
				if (currSheet.SelectedGeometryCollection.Count == 1 && m_gripToMove != null && m_gripToMove.Index == BaseRectangleGeometry.GRIP_CENTER)
					geomToMove = currSheet.SelectedGeometryCollection[0];

				if(geomToMove != null)
				{
					Point snapCenterGlobalPoint;
					if (currSheet._GetBestCenterSnapPoint(geomToMove, mouseGlobalPoint, out snapCenterGlobalPoint, out snapLineX, out snapLineY))
						mouseGlobalPoint = snapCenterGlobalPoint;
				}
				else
				{
					Point snapGlobalPoint;
					if (currSheet._GetSnapPoint(mouseGlobalPoint, out snapGlobalPoint, out snapLineX, out snapLineY))
						mouseGlobalPoint = snapGlobalPoint;
				}
			}

			// if running match properties command
			if(m_bInGeomMathPropertiesCommand)
			{
				// Clear the contents of the list used for hit test results.
				hitResultsList.Clear();

				// Set up a callback to receive the hit test result enumeration.
				VisualTreeHelper.HitTest(
					this,
					null,
					new HitTestResultCallback(HitTestCallback),
					new PointHitTestParameters(mousePoint));

				// find first rack
				Rack selectedRack = null;
				foreach (DependencyObject depObj in hitResultsList)
				{
					GeometryWrapper geomWrapper = depObj as GeometryWrapper;
					if (geomWrapper == null)
						continue;
					if (geomWrapper.RectangleGeometry == null)
						continue;

					selectedRack = geomWrapper.RectangleGeometry as Rack;
					if (selectedRack != null)
						break;
				}

				if (currDoc.PropertySourceRack == null)
				{
					currDoc.PropertySourceRack = selectedRack;
					this.ToolTipText = "Select target rack";
				}
				else
					currSheet.RackMatchProperties(currDoc.PropertySourceRack, new List<BaseRectangleGeometry>() { selectedRack });

				this.UpdateDrawing(true);
				return;
			}

			// if running Move command
			if(m_bInSelectionMove)
			{
				// Clear the contents of the list used for hit test results.
				hitResultsList.Clear();

				// Set up a callback to receive the hit test result enumeration.
				VisualTreeHelper.HitTest(
					this,
					null,
					new HitTestResultCallback(HitTestCallback),
					new PointHitTestParameters(mousePoint));

				// find first geom
				BaseRectangleGeometry firstGeom = null;
				foreach (DependencyObject depObj in hitResultsList)
				{
					GeometryWrapper geomWrapper = depObj as GeometryWrapper;
					if (geomWrapper == null)
						continue;
					if (geomWrapper.RectangleGeometry == null)
						continue;

					firstGeom = geomWrapper.RectangleGeometry;
					if (firstGeom != null)
						break;
				}

				//
				if(firstGeom != null && currSheet.SelectedGeometryCollection.Contains(firstGeom))
				{
					m_bIsPrevMoveSelectionPointSet = true;
					m_PrevSelectionMovePoint = mouseGlobalPoint;
				}

				// 
				if(!m_bIsPrevMoveSelectionPointSet)
				{
					m_bInSelectionMove = false;
					// Probably it is copy-paste-move command, so need to delete non initialized geometry on click in empty drawing space.
					// Dont mark state changed right now, because SelectedGeometryCollection contains non initialized geometry.
					if (currSheet.Document.IsInCommand)
						currSheet.DeleteNonInitializedGeometry(false, false);

					currSheet.SelectedGeometryCollection.Clear();

					// If it is copy-paste-move command then mark state changed.
					if (currSheet.Document.IsInCommand)
						currSheet.MarkStateChanged();

					if (firstGeom != null)
						currSheet.SelectedGeometryCollection.Add(firstGeom);
				}

				this.UpdateDrawing(false);
				return;
			}

			if (m_gripToMove != null)
			{
				if (m_gripToMove is CreateRackRowGripPoint || m_gripToMove is CreateRackColumnGripPoint)
					currSheet.AddTempRacks();
				else if (m_gripToMove is CreateColumnPattern_GripPoint)
					currSheet.AddTempColumns();
				else
					m_gripToMove.Move(mouseGlobalPoint, currSheet.Length, currSheet.Width);

				//
				currSheet.OnGripPointMoved(m_gripToMove);

				ResetGrips();
			}
			else
			{
				// Clear the contents of the list used for hit test results.
				hitResultsList.Clear();

				// Set up a callback to receive the hit test result enumeration.
				VisualTreeHelper.HitTest(
					this,
					null,
					new HitTestResultCallback(HitTestCallback),
					new PointHitTestParameters(mousePoint));

				//
				DrawingVisual visualUnderPnt = _GetFirstVisual();

				// moving grip
				GripPoint _grip = visualUnderPnt as GripPoint;

				//
				RotateGripPoint rotateGrip = _grip as RotateGripPoint;
				if (rotateGrip != null)
				{
					rotateGrip.Rotate(currSheet.Length, currSheet.Width);

					// dont mark state changed if it is new non initialized rack
					if (rotateGrip.Geometry != null && rotateGrip.Geometry.IsInit)
					{
						currSheet.MarkStateChanged();
						ResetGrips();
					}

					UpdateDrawing(false);
					return;
				}

				//
				SelectRackGroupGripPoint selectRackRowColumnGrip = _grip as SelectRackGroupGripPoint;
				if(selectRackRowColumnGrip != null && selectRackRowColumnGrip.Geometry != null)
				{
					Rack selectedRack = selectRackRowColumnGrip.Geometry as Rack;
					if (selectedRack == null)
						return;

					currSheet.SelectedGeometryCollection.Clear();
					List<Rack> rackRowColumn = currSheet.GetRackGroup(selectedRack);
					foreach(Rack rack in rackRowColumn)
					{
						if (rack == null)
							continue;

						currSheet.SelectedGeometryCollection.Add(rack);
					}

					return;
				}

				//
				if (_grip != null)
				{
					m_gripToMove = _grip;
					return;
				}

				// if it is not init
				List<BaseRectangleGeometry> nonInitGeomList = currSheet.NonInitSelectedGeometryList;
				// dont accept click if layout is not correct
				foreach (BaseRectangleGeometry geom in nonInitGeomList)
				{
					if (geom == null)
						continue;

					// check layout
					if (!currSheet.IsLayoutCorrect(geom))
					{
						UpdateDrawing(true);
						return;
					}
				}
				//
				foreach (BaseRectangleGeometry geom in nonInitGeomList)
				{
					if (geom == null)
						continue;

					//
					Shutter shutter = geom as Shutter;
					if(shutter != null)
					{
						bool bShutterIncorrectPos = false;
						if (shutter.IsHorizontal)
						{
							if (Utils.FNE(shutter.TopLeft_GlobalPoint.Y, -shutter.Length_Y) && Utils.FNE(shutter.TopLeft_GlobalPoint.Y, currSheet.Width))
								bShutterIncorrectPos = true;
						}
						else
						{
							if (Utils.FNE(shutter.TopLeft_GlobalPoint.X, -shutter.Length_X) && Utils.FNE(shutter.TopLeft_GlobalPoint.X, currSheet.Length))
								bShutterIncorrectPos = true;
						}

						if(bShutterIncorrectPos)
						{
							UpdateDrawing(true);
							return;
						}
					}
				}
				//
				if(nonInitGeomList.Count > 0)
				{
					//
					currSheet.MarkStateChanged();
					// remove selection
					currSheet.SelectedGeometryCollection.Clear();
					UpdateDrawing(true);
					return;
				}

				// disable change selection if there is non initialized geometry
				if (currSheet.Rectangles.FirstOrDefault(g => !g.IsInit) != null)
					return;

				//
				GeometryWrapper selectedWrapper = visualUnderPnt as GeometryWrapper;
				//
				if (selectedWrapper != null && Keyboard.Modifiers == ModifierKeys.Control)
				{
					_AddToSelection(selectedWrapper.RectangleGeometry, true);

					// need to update snapping
					ResetGrips();
					UpdateDrawing(false);

					return;
				}

				BaseRectangleGeometry newSelectedGeom = null;
				if (selectedWrapper != null)
					newSelectedGeom = selectedWrapper.RectangleGeometry;
				currSheet.SelectedGeometryCollection.Clear();
				if(newSelectedGeom != null)
					currSheet.SelectedGeometryCollection.Add(newSelectedGeom);
			}

			//
			UpdateDrawing(false);
		}

		//=============================================================================
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);

			DrawingSheet currSheet = Sheet;
			if (currSheet == null)
				return;

			DrawingDocument currDoc = currSheet.Document;
			if (currDoc == null)
				return;

			if (this.DisplaySelectionRectangle)
			{
				this.DisplaySelectionRectangle = false;
				//
				Point mousePoint = e.GetPosition(this);
				this.SelectionSecondGlobalPnt = this.GetGlobalPoint(currSheet, mousePoint);

				if (m_bInZoomWindowCommand)
				{
					this.CameraZoom(currSheet, this.SelectionFirstGlobalPnt, this.SelectionSecondGlobalPnt);
					currSheet.MarkStateChanged();
					_Cancel();
					return;
				}
				else
				{
					currSheet.SelectByRectangle(this.SelectionFirstGlobalPnt, this.SelectionSecondGlobalPnt);

					// pass selection to rack match properties command
					if (m_bInGeomMathPropertiesCommand && currDoc.PropertySourceRack != null)
						currSheet.RackMatchProperties(currDoc.PropertySourceRack, currSheet.SelectedGeometryCollection.ToList());
				}
			}

			//
			if(m_bInSelectionMove && m_bIsPrevMoveSelectionPointSet)
			{
				Point mousePoint = e.GetPosition(this);
				Point mouseGlobalPnt = this.GetGlobalPoint(currSheet, mousePoint);

				Vector moveOffset = mouseGlobalPnt - m_PrevSelectionMovePoint;
				Vector appliedOffset;
				// Create vertical racks column:
				// -----
				// 
				//   M
				//
				// -----
				//   A
				// -----
				//   A
				// -----
				// Select 2 first racks and move them with "Move" command in place where they will hit the roof.
				// After move command ends all racks are moved. RackUtils.TryToReplaceRackInGroup() inside CheckRackHeight() inside MoveSelection() moves entire rack group.
				// Need to call currSheet.RegroupRacks() before check rack height.
				if (currSheet.MoveSelection(moveOffset, false, out appliedOffset))
				{
					m_bInSelectionMove = false;
					m_bIsPrevMoveSelectionPointSet = false;

					// Read comment above.
					currSheet.RegroupRacks();

					// user can copy-paste geometry and it will run move command, 
					// so need to make geometry initialized at the end of move command
					foreach (BaseRectangleGeometry geom in currSheet.SelectedGeometryCollection)
					{
						if (geom == null)
							continue;

						// If user copy-paste racks(IsInit=false) then need add them to racks unique sizes collection.
						// If user move already placed racks(IsInit=true) then dont add them to racks unique size collection, they are alreadry added.
						bool bAddRackUniqueSize = !geom.IsInit;
						// Mark geometry initialized, otherwise Rack.ClearAvailableHeight will return incorrect value.
						if (!geom.IsInit)
							geom.IsInit = true;
						Rack rackGeometry = geom as Rack;
						if(rackGeometry != null)
							rackGeometry.CheckRackHeight();

						// Probably user moved copy-paste racks which was not initialized.
						// Need add them to the rack unique size collection.
						if (bAddRackUniqueSize && rackGeometry != null)
							rackGeometry.SizeIndex = currDoc.AddRackUniqueSize(rackGeometry);
					}
					//
					currSheet.SelectedGeometryCollection.Clear();
					currSheet.MarkStateChanged();
				}
			}

			this.UpdateDrawing(false);
		}

		//=============================================================================
		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			base.OnMouseWheel(e);

			// Dont change camera scale if user creates the wall.
			// User should see entire sheet area.
			if (m_bCreateWall)
				return;

			DrawingSheet currentSheet = Sheet;
			if (currentSheet == null)
				return;

			double oldCameraScale = currentSheet.UnitsPerCameraPixel;
			Point globalPnt_UnderMouse = this.GetGlobalPoint(currentSheet, e.GetPosition(this));

			// Delta > 0 - увеличиваем масштаб
			// Delta < 0 - уменьшаем
			if (e.Delta > 0)
				currentSheet.UnitsPerCameraPixel /= 2;
			else
				currentSheet.UnitsPerCameraPixel *= 2;

			if (Utils.FNE(oldCameraScale, currentSheet.UnitsPerCameraPixel))
			{
				CalculateOffset(this.RenderSize);
				//
				Point localPnt_UnderMouse = e.GetPosition(this) - m_DrawOffset;
				// Dont change cursor position in sheet global coordinates.
				Point screenTopLeftPnt = new Point(0.0, 0.0);
				screenTopLeftPnt.X = localPnt_UnderMouse.X * currentSheet.UnitsPerCameraPixel;
				screenTopLeftPnt.Y = localPnt_UnderMouse.Y * currentSheet.UnitsPerCameraPixel;
				currentSheet.CameraOffset = globalPnt_UnderMouse - screenTopLeftPnt;
				currentSheet.TemporaryCameraOffset = new Vector(0.0, 0.0);

				currentSheet.CheckCameraOffsetVector(this.ActualWidth, this.ActualHeight);
			}

			InvalidateVisual();
		}

		//=============================================================================
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
			{
				DrawingSheet currentSheet = Sheet;
				if (currentSheet == null)
					return;

				m_MouseWheelButtonPressedPoint = this.GetGlobalPoint(currentSheet, e.GetPosition(this));
			}
		}

		//=============================================================================
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);

			if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
			{
				DrawingSheet currentSheet = Sheet;
				if (currentSheet == null)
					return;

				currentSheet.CameraOffset += currentSheet.TemporaryCameraOffset;
				currentSheet.TemporaryCameraOffset = new Vector(0.0, 0.0);
				currentSheet.CheckCameraOffsetVector(this.ActualWidth, this.ActualHeight);
			}
		}

		//=============================================================================
		protected override void OnRender(DrawingContext dc)
		{
			DrawingSheet currentSheet = Sheet;
			bool bDraw_RackAdvancedDrawing = false;
			if (currentSheet != null && currentSheet.Document != null)
				bDraw_RackAdvancedDrawing = currentSheet.Document.ShowAdvancedProperties;

			if (!bDraw_RackAdvancedDrawing)
			{
				// Using the Background brush, draw a rectangle that fills the
				// available bounds of the panel.

				// Draw not transparent backround with some additional space around control.
				// Otherwise user cant select the second selection rectangle point outside control - because
				// there is no any control's graphic. Draw white rectangle with extra space around control, so
				// when mouse will be over this white rectangle DrawingControl will recive mouse move events.
				int additionalSpace = 40;
				dc.DrawRectangle(
					new SolidColorBrush(Colors.Transparent), // Read comment above, all working fine with transparent background.
					null,
					new Rect(0.0 - additionalSpace, 0.0 - additionalSpace, this.ActualWidth + 2 * additionalSpace, this.ActualHeight + 2 * additionalSpace));

				// draw border
				Pen borderPen = new Pen(this.BorderBrush, 1.0);
				dc.DrawRectangle(
					null,
					borderPen,
					new Rect(new Point(0.0, 0.0), new Point(this.ActualWidth, this.ActualHeight)));
			}

			// draw sheet background under all geometry
			if (currentSheet != null && !bDraw_RackAdvancedDrawing)
			{
				bool bApplyClip = false;
				if (!currentSheet.IsSheetFullyDisplayed)
					bApplyClip = true;

				if (bApplyClip)
					dc.PushClip(new RectangleGeometry(new Rect(new Point(0, 0), new Point(this.ActualWidth, this.ActualHeight))));

				Point sheetBorder_TopLeftPoint = this.GetLocalPoint(currentSheet, new Point(0.0, 0.0));
				Point sheetBorder_BotRightPoint = this.GetLocalPoint(currentSheet, new Point(currentSheet.Length, currentSheet.Width));
				dc.DrawRectangle(this.SheetBackgroundBrush, null, new Rect(sheetBorder_TopLeftPoint, sheetBorder_BotRightPoint));

				if (bApplyClip)
					dc.Pop();
			}

			// Dont clear grips if m_gripToMove is not null.
			// Create block - select center grip point - move - scale graphics area with mouse wheel - m_gripToMove is null.
			if (m_gripToMove == null)
				ResetGrips();
			// Probably there will be a lot of UI freezes from call UpdateDrawing().
			UpdateDrawing(false);
		}

		//=============================================================================
		protected override Size MeasureOverride(Size availableSize)
		{
			m_FullyDisplayedSheetSize = _SizeWithRatio(availableSize);
			return availableSize;
		}
		//=============================================================================
		protected override Size ArrangeOverride(Size finalSize)
		{
			m_FullyDisplayedSheetSize = _SizeWithRatio(finalSize);
			CalculateOffset(finalSize);
			return finalSize;
		}
		//=============================================================================
		/// <summary>
		/// Calculates control size. It should have the same length\height ratio as a current sheet length\height ratio.
		/// </summary>
		/// <param name="availableSize"></param>
		/// <returns></returns>
		private Size _SizeWithRatio(Size availableSize)
		{
			double length = 1;
			double width = 1;

			DrawingSheet currSheet = Sheet;
			if(currSheet != null)
			{
				length = currSheet.Length;
				width = currSheet.Width;
			}

			Size result = new Size();

			double rLengthWidthRatio = length / width;

			// 
			bool bGetLength = true;
			if(availableSize.Width > availableSize.Height)
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

			if(bGetLength)
			{
				result.Width = availableSize.Width;
				result.Height = availableSize.Width / rLengthWidthRatio;
			}
			else
			{
				result.Height = availableSize.Height;
				result.Width = availableSize.Height * rLengthWidthRatio;
			}

			if (currSheet != null)
			{
				bool isSheetFullyDisplayed = currSheet.IsSheetFullyDisplayed;
				currSheet.MaxUnitsPerCameraPixel = currSheet.Length / result.Width;
				if (!currSheet.Is_UnitsPerCameraPixel_Init)
				{
					currSheet.UnitsPerCameraPixel = currSheet.MaxUnitsPerCameraPixel;
					currSheet.Is_UnitsPerCameraPixel_Init = true;
				}
				// If sheet is fully displayed, but DrawingControl size is changed then need to update
				// UnitsPerCameraPixel and make sheet fully displayed.
				if (isSheetFullyDisplayed)
					currSheet.FullyDisplaySheet();
			}

			return result;
		}

		//=============================================================================
		private eWallPosition _GetWallPosition(Point mouseGlobalPnt)
		{
			DrawingSheet curSheet = this.Sheet;
			if (curSheet == null)
				return eWallPosition.eUndefined;

			//
			Point leftPnt = new Point(0.0, curSheet.Width / 2);
			double leftDelta = Math.Abs((mouseGlobalPnt - leftPnt).Length);
			//
			Point topPnt = new Point(curSheet.Length / 2, 0.0);
			double topDelta = Math.Abs((mouseGlobalPnt - topPnt).Length);
			//
			Point rightPnt = new Point(curSheet.Length, curSheet.Width / 2);
			double rightDelta = Math.Abs((mouseGlobalPnt - rightPnt).Length);
			//
			Point botPnt = new Point(curSheet.Length / 2, curSheet.Width);
			double botDelta = Math.Abs((mouseGlobalPnt - botPnt).Length);

			if (Utils.FLE(leftDelta, topDelta) && Utils.FLE(leftDelta, rightDelta) && Utils.FLE(leftDelta, botDelta))
				return eWallPosition.eLeft;
			else if (Utils.FLE(topDelta, leftDelta) && Utils.FLE(topDelta, rightDelta) && Utils.FLE(topDelta, botDelta))
				return eWallPosition.eTop;
			else if (Utils.FLE(rightDelta, leftDelta) && Utils.FLE(rightDelta, topDelta) && Utils.FLE(rightDelta, botDelta))
				return eWallPosition.eRight;
			else if (Utils.FLE(botDelta, leftDelta) && Utils.FLE(botDelta, topDelta) && Utils.FLE(botDelta, rightDelta))
				return eWallPosition.eBot;

			return eWallPosition.eUndefined;
		}

		//=============================================================================
		private void CalculateOffset(Size controlSize)
		{
			m_DrawOffset = new Vector(0.0, 0.0);
			DrawingSheet currSheet = this.Sheet;
			if (currSheet == null)
				return;

			double scale = currSheet.MaxUnitsPerCameraPixel / currSheet.UnitsPerCameraPixel;

			double offsetX = (controlSize.Width - scale * m_FullyDisplayedSheetSize.Width) / 2;
			double offsetY = (controlSize.Height - scale * m_FullyDisplayedSheetSize.Height) / 2;

			if (Utils.FGT(offsetX, 0.0))
				m_DrawOffset.X = offsetX;
			if (Utils.FGT(offsetY, 0.0))
				m_DrawOffset.Y = offsetY;
		}

		#endregion

		#region Functions

		//=============================================================================
		public void SendKey(KeyEventArgs e)
		{
			if (!e.Handled)
			{
				DrawingSheet currSheet = Sheet;
				if (currSheet == null)
					return;

				DrawingDocument currDoc = currSheet.Document;
				if (currDoc == null)
					return;

				if (e.Key == Key.Escape)
				{
					// If rack match properties command is running, then stop it.
					// If create wall command is running, then cancel it.
					if(m_bInGeomMathPropertiesCommand || m_bCreateWall)
					{
						if (m_bInGeomMathPropertiesCommand)
						{
							// Call DrawingSheet.MarkStateChanged() instead DrawingDocument.MarkStateChanged().
							// Otherwise DrawingSheet._UpdateStatisticsCollections() is not called.
							//currDoc.MarkStateChanged();
							currSheet.MarkStateChanged();
						}
						else
							currDoc.SetTheLastState();
						_Cancel();
						e.Handled = true;
						return;
					}

					List<BaseRectangleGeometry> nonInitGeomList = currSheet.NonInitSelectedGeometryList;
					if (nonInitGeomList.Count > 0)
					{
						_Cancel();
						e.Handled = true;
					}
					else if (m_gripToMove != null || m_bInSelectionMove)
					{
						_Cancel();

						e.Handled = true;
					}
				}
				else if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
				{
					currDoc.Undo();
					e.Handled = true;
				}
			}
		}



		//=============================================================================
		public void UpdateDrawing(bool bUpdateGeometryWrappers)
		{
			if (bUpdateGeometryWrappers)
				this.UpdateGeometryWrappersCollection();

			//
			if (m_SnappingLines != null)
				m_SnappingLines.Draw(true);

			//
			DrawingSheet currSheet = Sheet;
			if (currSheet != null)
			{
				// If camera displays part of the current sheet then dont display geometry which is outside of displayed area.
				bool isSheetFullyDisplayed = currSheet.IsSheetFullyDisplayed;
				Point displayedArea_TopLeftPoint = new Point(0.0, 0.0);
				Point displayedArea_BotRightPoint = new Point(0.0, 0.0);
				if(!isSheetFullyDisplayed)
				{
					displayedArea_TopLeftPoint = this.GetGlobalPoint(currSheet, new Point(0.0, 0.0));
					displayedArea_BotRightPoint = this.GetGlobalPoint(currSheet, new Point(this.ActualWidth, this.ActualHeight));
				}

				foreach (BaseRectangleGeometry geom in currSheet.Rectangles)
				{
					if (geom == null || geom.Wrapper == null)
						continue;

					bool bForceHide = false;
					// Wall and shutter are displayed outside control, so dont clip them
					if (!isSheetFullyDisplayed && !(geom is Wall || geom is Shutter))
					{
						// if one rectangle is on the left side of other
						if (Utils.FGE(displayedArea_TopLeftPoint.X, geom.BottomRight_GlobalPoint.X) || Utils.FGE(geom.TopLeft_GlobalPoint.X, displayedArea_BotRightPoint.X))
							bForceHide = true;

						// if one rectangle is above other
						if (!bForceHide && Utils.FGE(displayedArea_TopLeftPoint.Y, geom.BottomRight_GlobalPoint.Y) || Utils.FGE(geom.TopLeft_GlobalPoint.Y, displayedArea_BotRightPoint.Y))
							bForceHide = true;
					}

					geom.Wrapper.Draw(true && !bForceHide);
				}

				_DrawTieBeams(currSheet, true && !currSheet.Document.IsInCommand);

				foreach (BaseRectangleGeometry geom in currSheet.TemporaryRacksList)
				{
					if (geom == null || geom.Wrapper == null)
						continue;

					bool bForceHide = false;
					if (!isSheetFullyDisplayed)
					{
						// if one rectangle is on the left side of other
						if (Utils.FGE(displayedArea_TopLeftPoint.X, geom.BottomRight_GlobalPoint.X) || Utils.FGE(geom.TopLeft_GlobalPoint.X, displayedArea_BotRightPoint.X))
							bForceHide = true;

						// if one rectangle is above other
						if (!bForceHide && Utils.FGE(displayedArea_TopLeftPoint.Y, geom.BottomRight_GlobalPoint.Y) || Utils.FGE(geom.TopLeft_GlobalPoint.Y, displayedArea_BotRightPoint.Y))
							bForceHide = true;
					}

					geom.Wrapper.Draw(true && !bForceHide);
				}

				foreach (List<BaseRectangleGeometry> row in currSheet.TemporaryColumnsInPattern)
				{
					foreach (BaseRectangleGeometry geom in row)
					{
						if (geom == null || geom.Wrapper == null)
							continue;

						bool bForceHide = false;
						if (!isSheetFullyDisplayed)
						{
							// if one rectangle is on the left side of other
							if (Utils.FGE(displayedArea_TopLeftPoint.X, geom.BottomRight_GlobalPoint.X) || Utils.FGE(geom.TopLeft_GlobalPoint.X, displayedArea_BotRightPoint.X))
								bForceHide = true;

							// if one rectangle is above other
							if (!bForceHide && Utils.FGE(displayedArea_TopLeftPoint.Y, geom.BottomRight_GlobalPoint.Y) || Utils.FGE(geom.TopLeft_GlobalPoint.Y, displayedArea_BotRightPoint.Y))
								bForceHide = true;
						}

						geom.Wrapper.Draw(true && !bForceHide);
					}
				}
			}

			// draw grip points
			if (m_grips != null)
			{
				foreach (GripPoint g in m_grips)
				{
					if (g == null)
						continue;

					g.Draw(true);
				}
			}

			if (m_TieBeamGroupsVisual != null)
				m_TieBeamGroupsVisual.Draw();

			//
			if (m_PreviewBordersControl != null)
				m_PreviewBordersControl.Draw(true);

			if (m_SelectedGeomInfoVisual != null)
				m_SelectedGeomInfoVisual.Draw();

			if (m_SelectionRectVisual != null)
				m_SelectionRectVisual.Draw();

			// draw watermarks over all other graphics
			if (m_WatermarkVisual != null)
				m_WatermarkVisual.Draw(this.WatermarkImage);

			//
			if (m_ToolTipVisual != null)
				m_ToolTipVisual.Draw();
		}
		/// <summary>
		/// Synchronize geometry wrappers collection with current sheet Rectangles.
		/// </summary>
		private void UpdateGeometryWrappersCollection()
		{
			if (m_GeometryWrappersCollection == null)
				return;

			HashSet<GeometryWrapper> wrappersForDeleteSet = new HashSet<GeometryWrapper>();
			foreach(Visual visual in m_GeometryWrappersCollection)
			{
				GeometryWrapper geometryWrapper = visual as GeometryWrapper;
				if (geometryWrapper == null || geometryWrapper.RectangleGeometry == null)
					continue;

				wrappersForDeleteSet.Add(geometryWrapper);
			}

			// Sync geometry with sheet
			List<BaseRectangleGeometry> newGeometryList = new List<BaseRectangleGeometry>();
			DrawingSheet currSheet = this.Sheet;
			if(currSheet != null)
			{
				// rectangles
				if (currSheet.Rectangles != null)
				{
					foreach (BaseRectangleGeometry geom in currSheet.Rectangles)
					{
						if (geom == null)
							continue;

						if(geom.Wrapper == null)
							newGeometryList.Add(geom);
						else
						{
							if (wrappersForDeleteSet.Contains(geom.Wrapper))
								wrappersForDeleteSet.Remove(geom.Wrapper);
							else
								newGeometryList.Add(geom);
						}
					}
				}

				// temp racks
				if(currSheet.TemporaryRacksList != null)
				{
					foreach(BaseRectangleGeometry tempRackGeom in currSheet.TemporaryRacksList)
					{
						if (tempRackGeom == null)
							continue;

						if (tempRackGeom.Wrapper == null)
							newGeometryList.Add(tempRackGeom);
						else
						{
							if (wrappersForDeleteSet.Contains(tempRackGeom.Wrapper))
								wrappersForDeleteSet.Remove(tempRackGeom.Wrapper);
							else
								newGeometryList.Add(tempRackGeom);
						}
					}
				}

				// temp columns
				if(currSheet.TemporaryColumnsInPattern != null)
				{
					foreach(List<BaseRectangleGeometry> tempColumnsRow in currSheet.TemporaryColumnsInPattern)
					{
						if (tempColumnsRow == null)
							continue;

						foreach(BaseRectangleGeometry tempColumnGeom in tempColumnsRow)
						{
							if (tempColumnGeom == null)
								continue;

							if (tempColumnGeom.Wrapper == null)
								newGeometryList.Add(tempColumnGeom);
							else
							{
								if (wrappersForDeleteSet.Contains(tempColumnGeom.Wrapper))
									wrappersForDeleteSet.Remove(tempColumnGeom.Wrapper);
								else
									newGeometryList.Add(tempColumnGeom);
							}
						}
					}
				}

				// tie beams
				if(currSheet.TieBeamsList != null)
				{
					foreach(TieBeam tieBeamGeom in currSheet.TieBeamsList)
					{
						if (tieBeamGeom == null)
							continue;

						if (tieBeamGeom.Wrapper == null)
							newGeometryList.Add(tieBeamGeom);
						else
						{
							if (wrappersForDeleteSet.Contains(tieBeamGeom.Wrapper))
								wrappersForDeleteSet.Remove(tieBeamGeom.Wrapper);
							else
								newGeometryList.Add(tieBeamGeom);
						}
					}
				}
			}

			// Delete unused wrappers
			foreach(GeometryWrapper geomWrapper in wrappersForDeleteSet)
			{
				if (geomWrapper == null)
					continue;

				m_GeometryWrappersCollection.Remove(geomWrapper);
			}

			// Add new geometry
			bool areNewGeometryAdded = false;
			foreach (BaseRectangleGeometry newGeom in newGeometryList)
			{
				if (newGeom == null)
					continue;

				GeometryWrapper newGeomWrapper = newGeom.Wrapper;
				if (newGeomWrapper == null)
				{
					newGeomWrapper = new GeometryWrapper(this, newGeom);
					newGeom.Wrapper = newGeomWrapper;
				}
				if (!m_GeometryWrappersCollection.Contains(newGeomWrapper))
				{
					if (!areNewGeometryAdded)
						areNewGeometryAdded = true;

					m_GeometryWrappersCollection.Add(newGeomWrapper);
				}
			}

			// Sort geometry wrappers
			if (areNewGeometryAdded)
				SortGeometryWrappersCollection();
		}
		private void SortGeometryWrappersCollection()
		{
			List<GeometryWrapper> wrappersList = new List<GeometryWrapper>();
			foreach(Visual visual in m_GeometryWrappersCollection)
			{
				GeometryWrapper geomWrapper = visual as GeometryWrapper;
				if (geomWrapper == null || geomWrapper.RectangleGeometry == null)
					continue;

				wrappersList.Add(geomWrapper);
			}

			try
			{
				wrappersList.Sort(new GeometryWrapperComparer());
			}
			catch { }
			m_GeometryWrappersCollection.Clear();

			foreach(GeometryWrapper geomWrapper in wrappersList)
			{
				if (geomWrapper == null)
					continue;

				m_GeometryWrappersCollection.Add(geomWrapper);
			}
		}
		/// <summary>
		/// BaseRectangleGeometry comparer.
		/// Sort them in display order.
		/// For example, column should be placed above block in display order.
		/// </summary>
		private class GeometryWrapperComparer : IComparer<GeometryWrapper>
		{
			// Returns:
			// -1 x < y
			//  0 x = y
			//  1 x > y
			public int Compare(GeometryWrapper x, GeometryWrapper y)
			{
				BaseRectangleGeometry geom_X = null;
				if (x != null)
					geom_X = x.RectangleGeometry;
				BaseRectangleGeometry geom_Y = null;
				if (y != null)
					geom_Y = y.RectangleGeometry;

				// SheetElevationGeometry should draws over all other geometry, so add it to the end of m_Children.
				//
				// Shutters should be on the top of walls, so add them to the end of m_Children.
				//
				// Column should be on the top of block and aisle place.
				// If wrapper is a column - place it to the end, otherwise - place it before the first column.
				// All columns should be in the end of m_Children.

				// Display SheetElevationGeometry over all other geometry, include non initialized geometry.
				if (geom_X is SheetElevationGeometry)
					return 1;
				else if (geom_Y is SheetElevationGeometry)
					return -1;

				// Display not initialized geometry over initialized.
				if (geom_X.IsInit != geom_Y.IsInit)
				{
					if (geom_X.IsInit)
						return -1;
					return 1;
				}

				// From top to bottom: tie beam, rack, shutter, wall, column, aisle space, all other geometry
				if (geom_X != null && geom_Y != null)
				{
					int index_X = GetGeometryOrderIndex(geom_X);
					int index_Y = GetGeometryOrderIndex(geom_Y);
					return index_X - index_Y;
				}
				else if (geom_X != null)
					return 1;
				else if (geom_Y != null)
					return -1;

				return 0;
			}

			/// <summary>
			/// Returns Z-index of geometry.
			/// Geometry with greater index is displayed over geometry with lower index.
			/// </summary>
			private int GetGeometryOrderIndex(BaseRectangleGeometry geom)
			{
				if (geom == null)
					return 0;

				//if (geom is SheetElevationGeometry)
				//	return 8;
				if (geom is TieBeam)
					return 7;
				else if (geom is Rack)
					return 6;
				else if (geom is Shutter)
					return 5;
				else if (geom is Wall)
					return 4;
				else if (geom is Column)
					return 3;
				else if (geom is AisleSpace)
					return 2;
				else if (geom is Block)
					return 1;

				return 1;
			}
		}

		//=============================================================================
		public void ResetGrips()
		{
			_ClearGrips();

			//
			DrawingSheet _currSheet = Sheet;
			if (_currSheet == null)
				return;

			//
			if (_currSheet.SelectedGeometryCollection.Count > 1)
			{
				bool bItIsRacksSingleRowCoulumn = true;
				int iMultiSelectionRacksCount = 0;
				bool bDisableChangeRackSize = false;
				// if it is rack's entire row\column
				List<List<Rack>> foundedRackGroups = new List<List<Rack>>();
				foreach (BaseRectangleGeometry selectedRect in _currSheet.SelectedGeometryCollection)
				{
					Rack selectedRack = selectedRect as Rack;
					if (selectedRack != null)
					{
						if (!bDisableChangeRackSize && selectedRack.DisableChangeSizeGripPoints)
							bDisableChangeRackSize = true;

						++iMultiSelectionRacksCount;
						//
						List<Rack> selectedRackGroup = _currSheet.GetRackGroup(selectedRack);
						if (!foundedRackGroups.Contains(selectedRackGroup))
						{
							foundedRackGroups.Add(selectedRackGroup);
							if (foundedRackGroups.Count > 1)
							{
								bItIsRacksSingleRowCoulumn = false;
								break;
							}
						}
					}
					else
					{
						bItIsRacksSingleRowCoulumn = false;
						break;
					}
				}

				if (bItIsRacksSingleRowCoulumn)
				{
					// only if selected all racks in the row\column
					bItIsRacksSingleRowCoulumn = false;
					if (foundedRackGroups.Count == 1 && foundedRackGroups[0].Count == iMultiSelectionRacksCount)
						bItIsRacksSingleRowCoulumn = true;
				}

				//
				if (bItIsRacksSingleRowCoulumn && foundedRackGroups.Count == 1)
				{
					//
					MoveRacksGroupGripPoint moveRacksRowColumnGrip = new MoveRacksGroupGripPoint(this, foundedRackGroups[0]);
					m_grips.Add(moveRacksRowColumnGrip);
					AddVisualChild(moveRacksRowColumnGrip);
					AddLogicalChild(moveRacksRowColumnGrip);

					//
					if (!bDisableChangeRackSize)
					{
						StretchRacksGroupGripPoint topLeft_GripPoint = new StretchRacksGroupGripPoint(this, foundedRackGroups[0], BaseRectangleGeometry.GRIP_TOP_LEFT);
						m_grips.Add(topLeft_GripPoint);
						AddVisualChild(topLeft_GripPoint);
						AddLogicalChild(topLeft_GripPoint);

						StretchRacksGroupGripPoint botRight_GripPoint = new StretchRacksGroupGripPoint(this, foundedRackGroups[0], BaseRectangleGeometry.GRIP_BOTTOM_RIGHT);
						m_grips.Add(botRight_GripPoint);
						AddVisualChild(botRight_GripPoint);
						AddLogicalChild(botRight_GripPoint);
					}
				}

				return;
			}
			else if (_currSheet.SelectedGeometryCollection.Count == 1)
			{
				//
				BaseRectangleGeometry selectedGeom = _currSheet.SelectedGeometryCollection[0];
				if (selectedGeom != null)
				{
					bool bDisableChangeSizeGripPoints = false;
					Rack selectedRack = selectedGeom as Rack;
					if (selectedRack != null)
						bDisableChangeSizeGripPoints = selectedRack.DisableChangeSizeGripPoints;

					List<Point> pnts = selectedGeom.GetGripPoints();
					if (pnts != null)
					{
						foreach (Point p in pnts)
						{
							int iGripIndex = pnts.IndexOf(p);

							if (bDisableChangeSizeGripPoints && (iGripIndex != BaseRectangleGeometry.GRIP_CENTER && iGripIndex != GripPoint.GRIP_ROTATE))
								continue;

							GripPoint newGrip = new GripPoint(this, selectedGeom, iGripIndex);
							m_grips.Add(newGrip);
							AddVisualChild(newGrip);
							AddLogicalChild(newGrip);
						}

						// add rotation grip for rack and column geometry
						if ((selectedGeom is Rack || selectedGeom is Column) && selectedGeom.ShowRotationGrips)
						{
							RotateGripPoint clockwiseGrip = new RotateGripPoint(this, selectedGeom, BaseRectangleGeometry.GRIP_TOP_LEFT, GripPoint.GRIP_ROTATE);
							m_grips.Add(clockwiseGrip);
							AddVisualChild(clockwiseGrip);
							AddLogicalChild(clockwiseGrip);
						}

						if (!selectedGeom.IsInit)
							return;

						// create rack row grip
						if (selectedRack != null)
						{
							if (selectedRack.CanCreateRow)
							{
								GripPoint rackGrip = null;

								if (selectedRack.IsHorizontal)
									rackGrip = new CreateRackRowGripPoint(this, selectedGeom, BaseRectangleGeometry.GRIP_BOTTOM_RIGHT);
								else
									rackGrip = new CreateRackColumnGripPoint(this, selectedGeom, BaseRectangleGeometry.GRIP_BOTTOM_RIGHT);

								m_grips.Add(rackGrip);
								AddVisualChild(rackGrip);
								AddLogicalChild(rackGrip);
							}

							//
							List<Rack> _rackGroup = _currSheet.GetRackGroup(selectedRack);
							if (_rackGroup.Count > 1)
							{
								SelectRackGroupGripPoint selectRowColumnGripPoint = new SelectRackGroupGripPoint(this, selectedGeom);
								m_grips.Add(selectRowColumnGripPoint);
								AddVisualChild(selectRowColumnGripPoint);
								AddLogicalChild(selectRowColumnGripPoint);
							}
						}

						// column
						Column selectedColumn = selectedGeom as Column;
						if (selectedColumn != null)
						{
							// Are offsetX and offsetY grip points in the graphics area?
							bool bOffsetX_inGA = false;
							bool bOffsetY_inGa = false;
							ColumnPattern _pattern = selectedColumn.Pattern;
							if (_pattern != null)
							{
								if ((selectedColumn.Center_GlobalPoint.X + _pattern.GlobalOffset_X + selectedColumn.Length_X / 2) <= _currSheet.Length)
									bOffsetX_inGA = true;
								if ((selectedColumn.Center_GlobalPoint.Y + _pattern.GlobalOffset_Y + selectedColumn.Length_Y / 2) <= _currSheet.Width)
									bOffsetY_inGa = true;
							}

							//
							if (selectedColumn.CanContinuePattern && (bOffsetX_inGA || bOffsetY_inGa))
							{
								CreateColumnPattern_GripPoint patternGrip = new CreateColumnPattern_GripPoint(this, selectedGeom);
								m_grips.Add(patternGrip);
								AddVisualChild(patternGrip);
								AddLogicalChild(patternGrip);
							}

							//
							if (bOffsetX_inGA)
							{
								ColumnPatternOffsetX_GripPoint offsetX_Grip = new ColumnPatternOffsetX_GripPoint(this, selectedGeom);
								m_grips.Add(offsetX_Grip);
								AddVisualChild(offsetX_Grip);
								AddLogicalChild(offsetX_Grip);
							}

							//
							if (bOffsetY_inGa)
							{
								ColumnPatternOffsetY_GripPoint offsetY_Grip = new ColumnPatternOffsetY_GripPoint(this, selectedGeom);
								m_grips.Add(offsetY_Grip);
								AddVisualChild(offsetY_Grip);
								AddLogicalChild(offsetY_Grip);
							}
						}
					}
				}
			}
		}



		//=============================================================================
		public void ClearAll()
		{
			//
			m_rVertSnapLineX = -1;
			m_rHorizSnapLineY = -1;

			//
			_ClearGrips();

			//
			m_bInGeomMathPropertiesCommand = false;
			sGeomMathPropertiesCommand_RememberTheChoice = false;
			sGeomMatchPropertiesCommand_MatchIfDepthDiff = false;
			this.CanDisplayToolTip = false;
			this.ToolTipText = string.Empty;

			//
			m_bInSelectionMove = false;
			m_bIsPrevMoveSelectionPointSet = false;

			//
			m_bCreateWall = false;

			m_bInZoomWindowCommand = false;
		}


		//=============================================================================
		public void GeometryMatchProperties()
		{
			DrawingSheet currSheet = this.Sheet;
			if (currSheet == null)
				return;

			DrawingDocument currDoc = currSheet.Document;
			if (currDoc == null)
				return;

			m_bInGeomMathPropertiesCommand = true;
			sGeomMathPropertiesCommand_RememberTheChoice = false;
			sGeomMatchPropertiesCommand_MatchIfDepthDiff = false;

			// if there is only one rack in the selection then take it as a property source rack
			// otherwise clear selection and ask user for select property source rack
			if (currSheet.SelectedGeometryCollection.Count == 1 && currSheet.SelectedGeometryCollection[0] is Rack)
				currDoc.PropertySourceRack = currSheet.SelectedGeometryCollection[0] as Rack;
			//
			if (currDoc.PropertySourceRack == null)
				this.ToolTipText = "Select the property source rack";
			else
				this.ToolTipText = "Select target rack";
			currSheet.SelectedGeometryCollection.Clear();
			//
			this.UpdateDrawing(false);
		}


		//=============================================================================
		public void MoveSelection()
		{
			m_bInSelectionMove = true;
			m_bIsPrevMoveSelectionPointSet = false;
		}


		//=============================================================================
		public void CreateWall()
		{
			this.ToolTipText = "Select wall placement";
			m_bCreateWall = true;
			this.UpdateDrawing(true);
		}

		//=============================================================================
		/// <summary>
		/// Set sheet camera scale and offset by selecting rectangle
		/// </summary>
		public void ZoomWindow()
		{
			DrawingSheet currSheet = this.Sheet;
			if (currSheet == null)
				return;

			DrawingDocument currDoc = currSheet.Document;
			if (currDoc == null)
				return;

			m_bInZoomWindowCommand = true;
			this.ToolTipText = "Select zoom window";
			this.UpdateDrawing(false);
		}

		#endregion

		#region Private Functions

		//=============================================================================
		private List<DependencyObject> hitResultsList = new List<DependencyObject>();
		// Return the result of the hit test to the callback.
		private HitTestResultBehavior HitTestCallback(HitTestResult result)
		{
			// WatermarkVisual has bigger Z-order than all other child on DrawingControl.
			// But when user clicks on DrawingControl, WatermarkVisual should be ignored.
			// So use this HitTestResult callback for collection ALL children under mouse point
			// and get first DrawingVisual with _GetFirstVisual().

			// Add the hit test result to the list that will be processed after the enumeration.
			hitResultsList.Add(result.VisualHit);

			// Set the behavior to return visuals at all z-order levels.
			return HitTestResultBehavior.Continue;
		}
		//=============================================================================
		private DrawingVisual _GetFirstVisual()
		{
			foreach (DependencyObject depObj in hitResultsList)
			{
				// ignore WatermarkVisual
				if (depObj is WatermarkVisual)
					continue;

				// ignore selected geometry info - distance to the sheet borders
				if (depObj is SelectedGeometryInfoVisual)
					continue;

				DrawingVisual dv = depObj as DrawingVisual;
				if (dv != null)
				{
					// ignore tie beams
					GeometryWrapper geomWrapper = dv as GeometryWrapper;
					if (geomWrapper != null && geomWrapper.RectangleGeometry is TieBeam)
						continue;

					return dv;
				}
			}

			return null;
		}

		//=============================================================================
		private void _ClearGrips()
		{
			foreach (GripPoint g in m_grips)
			{
				RemoveVisualChild(g);
				RemoveLogicalChild(g);
			}
			m_grips.Clear();

			m_gripToMove = null;
		}

		//=============================================================================
		private void _AddToSelection(List<BaseRectangleGeometry> geomList, bool bRemoveIfExists)
		{
			if (geomList == null)
				return;

			foreach(BaseRectangleGeometry geom in geomList)
			{
				if (geom == null)
					continue;

				_AddToSelection(geom, bRemoveIfExists);
			}
		}
		private void _AddToSelection(BaseRectangleGeometry selectedGeom, bool bRemoveIfExists)
		{
			if (selectedGeom == null)
				return;

			DrawingSheet _currSheet = Sheet;
			if (_currSheet == null)
				return;

			//
			if (_currSheet.SelectedGeometryCollection.Contains(selectedGeom))
			{
				if (bRemoveIfExists)
					_currSheet.SelectedGeometryCollection.Remove(selectedGeom);
			}
			else
				_currSheet.SelectedGeometryCollection.Add(selectedGeom);
		}

		//=============================================================================
		private void _Cancel()
		{
			m_gripToMove = null;
			m_bInSelectionMove = false;
			m_bIsPrevMoveSelectionPointSet = false;
			m_bCreateWall = false;
			m_bInGeomMathPropertiesCommand = false;
			sGeomMathPropertiesCommand_RememberTheChoice = false;
			sGeomMatchPropertiesCommand_MatchIfDepthDiff = false;
			m_bInZoomWindowCommand = false;

			this.ToolTipText = string.Empty;
			_ClearGrips();

			DrawingSheet currSheet = Sheet;
			if (currSheet != null)
			{
				DrawingDocument currDoc = currSheet.Document;
				if(currDoc != null)
					currDoc.SetTheLastState();
			}

			this.UpdateDrawing(true);
		}

		//=============================================================================
		private void _DrawTieBeams(DrawingSheet sheet, bool bShow)
		{
			// synchronize tie beams and draw them

			List<GeometryWrapper> wrappersForDelete_List = new List<GeometryWrapper>();
			wrappersForDelete_List.AddRange(m_TieBeamsWrappersList);
			if(sheet != null)
			{
				foreach (TieBeam tieBeamGeometry in sheet.TieBeamsList)
				{
					if (tieBeamGeometry == null)
						continue;

					// try to find tie beam
					GeometryWrapper foundWrapper = m_TieBeamsWrappersList.FirstOrDefault(wrapper => wrapper != null && wrapper.RectangleGeometry == tieBeamGeometry);
					if(foundWrapper == null)
					{
						// add new geometry
						GeometryWrapper newWrapper = new GeometryWrapper(this, tieBeamGeometry);
						this.AddVisualChild(newWrapper);
						this.AddLogicalChild(newWrapper);
						m_TieBeamsWrappersList.Add(newWrapper);
					}
					else
					{
						wrappersForDelete_List.Remove(foundWrapper);
					}
				}
			}

			// remove wrappers
			foreach(GeometryWrapper wrapper in wrappersForDelete_List)
			{
				if (wrapper == null)
					continue;

				this.RemoveVisualChild(wrapper);
				this.RemoveLogicalChild(wrapper);
				m_TieBeamsWrappersList.Remove(wrapper);
			}

			_UpTieBeamZOrder();

			// If camera displays part of the current sheet then dont display geometry which is outside of displayed area.
			bool isSheetFullyDisplayed = sheet.IsSheetFullyDisplayed;
			Point displayedArea_TopLeftPoint = new Point(0.0, 0.0);
			Point displayedArea_BotRightPoint = new Point(0.0, 0.0);
			if (!isSheetFullyDisplayed)
			{
				displayedArea_TopLeftPoint = this.GetGlobalPoint(sheet, new Point(0.0, 0.0));
				displayedArea_BotRightPoint = this.GetGlobalPoint(sheet, new Point(this.ActualWidth, this.ActualHeight));
			}

			// draw tie beams
			foreach (GeometryWrapper wrapper in m_TieBeamsWrappersList)
			{
				if (wrapper == null)
					continue;

				TieBeam tieBeamGeometry = wrapper.RectangleGeometry as TieBeam;
				if (tieBeamGeometry == null)
					continue;

				bool bForceHide = false;
				if (!isSheetFullyDisplayed)
				{
					// if one rectangle is on the left side of other
					if (Utils.FGE(displayedArea_TopLeftPoint.X, tieBeamGeometry.BottomRight_GlobalPoint.X) || Utils.FGE(tieBeamGeometry.TopLeft_GlobalPoint.X, displayedArea_BotRightPoint.X))
						bForceHide = true;

					// if one rectangle is above other
					if (!bForceHide && Utils.FGE(displayedArea_TopLeftPoint.Y, tieBeamGeometry.BottomRight_GlobalPoint.Y) || Utils.FGE(tieBeamGeometry.TopLeft_GlobalPoint.Y, displayedArea_BotRightPoint.Y))
						bForceHide = true;
				}

				wrapper.Draw(bShow && !bForceHide);
			}
		}
		//=============================================================================
		// Remove tie beams wrappers and add them.
		// It place them at the end of children, so they will have greater Z-order and draw over all other geometry.
		private void _UpTieBeamZOrder()
		{
			foreach (GeometryWrapper wrapper in m_TieBeamsWrappersList)
			{
				if (wrapper == null)
					continue;

				this.RemoveVisualChild(wrapper);
				this.RemoveLogicalChild(wrapper);
				//
				this.AddLogicalChild(wrapper);
				this.AddVisualChild(wrapper);
			}
		}



		//=============================================================================
		/// <summary>
		/// This functon is called when some sheet property was changed
		/// </summary>
		private void OnSheetPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			DrawingSheet sheet = sender as DrawingSheet;
			if (sheet != null)
				return;

			if(e.PropertyName == Utils.GetPropertyName(() => sheet.UnitsPerCameraPixel))
				CalculateOffset(this.RenderSize);
		}

		//=============================================================================
		/// <summary>
		/// Changes camera zoom and position so that it shows area from firstGlobalPoint to secondGlobalPoint.
		/// </summary>
		public void CameraZoom(DrawingSheet sheet, Point firstGlobalPoint, Point secondGlobalPoint)
		{
			if (sheet == null)
				return;

			Point topLeftPnt = new Point(0.0, 0.0);
			topLeftPnt.X = Math.Min(firstGlobalPoint.X, secondGlobalPoint.X);
			topLeftPnt.Y = Math.Min(firstGlobalPoint.Y, secondGlobalPoint.Y);
			Point botRightPnt = new Point(0.0, 0.0);
			botRightPnt.X = Math.Max(firstGlobalPoint.X, secondGlobalPoint.X);
			botRightPnt.Y = Math.Max(firstGlobalPoint.Y, secondGlobalPoint.Y);

			if (Utils.FLT(topLeftPnt.X, 0.0))
				topLeftPnt.X = 0.0;
			if (Utils.FLT(topLeftPnt.Y, 0.0))
				topLeftPnt.Y = 0.0;
			if (Utils.FGT(topLeftPnt.X, sheet.Length))
				topLeftPnt.X = sheet.Length;
			if (Utils.FGT(topLeftPnt.Y, sheet.Width))
				topLeftPnt.Y = sheet.Width;

			if (Utils.FLT(botRightPnt.X, 0.0))
				botRightPnt.X = 0.0;
			if (Utils.FLT(botRightPnt.Y, 0.0))
				botRightPnt.Y = 0.0;
			if (Utils.FGT(botRightPnt.X, sheet.Length))
				botRightPnt.X = sheet.Length;
			if (Utils.FGT(botRightPnt.Y, sheet.Width))
				botRightPnt.Y = sheet.Width;

			if (Utils.FEQ(topLeftPnt.X, botRightPnt.X) || Utils.FEQ(topLeftPnt.Y, botRightPnt.Y))
				return;

			// Calculate new units per pixel value
			double unitsPerPixel_Horizontal = sheet.UnitsPerCameraPixel / (this.ActualWidth / this.GetWidthInPixels(botRightPnt.X - topLeftPnt.X, sheet.UnitsPerCameraPixel));
			double unitsPerPixel_Vertical = sheet.UnitsPerCameraPixel / (this.ActualHeight / this.GetHeightInPixels(botRightPnt.Y - topLeftPnt.Y, sheet.UnitsPerCameraPixel));
			double newUnitsPerPixel = Math.Max(unitsPerPixel_Horizontal, unitsPerPixel_Vertical);

			sheet.UnitsPerCameraPixel = newUnitsPerPixel;
			sheet.Is_UnitsPerCameraPixel_Init = true;
			sheet.TemporaryCameraOffset = new Vector(0.0, 0.0);

			Point offsetPoint = topLeftPnt + (botRightPnt - topLeftPnt) / 2;
			offsetPoint.X -= sheet.UnitsPerCameraPixel * this.ActualWidth / 2;
			offsetPoint.Y -= sheet.UnitsPerCameraPixel * this.ActualHeight / 2;
			sheet.CameraOffset = new Vector(offsetPoint.X, offsetPoint.Y);
			sheet.CheckCameraOffsetVector(this.ActualWidth, this.ActualHeight);

			InvalidateVisual();
		}

		#endregion
	}
}
