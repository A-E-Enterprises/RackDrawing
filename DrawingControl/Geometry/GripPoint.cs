using AppColorTheme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public class GripPoint : DrawingVisual
	{
		public static int GRIP_ROTATE = 123123;
		public static int GRIP_RACK_CREATE_ROW = 123124;

		public static double sGripSize = 4.0;

		public GripPoint(DrawingControl dc, BaseRectangleGeometry geometry, int gripIndex)
		{
			m_DC = dc;
			m_Geometry = geometry;
			m_gripIndex = gripIndex;

			this.Draw(true);
		}

		#region Properties

		//=============================================================================
		protected DrawingControl m_DC = null;

		//=============================================================================
		/// <summary>
		/// Geomtry which owns this grip point.
		/// </summary>
		protected BaseRectangleGeometry m_Geometry = null;
		public BaseRectangleGeometry Geometry { get { return m_Geometry; } }

		//=============================================================================
		protected int m_gripIndex = -1;
		public int Index { get { return m_gripIndex; } }

		/// <summary>
		/// Grip point border color
		/// </summary>
		protected Brush BorderBrush
		{
			get
			{
				Color borderColor = Colors.Black;

				if (CurrentGeometryColorsTheme.CurrentTheme != null)
				{
					Color colorValue;
					if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eGripPointBorderColor, out colorValue))
						borderColor = colorValue;
				}

				return new SolidColorBrush(borderColor);
			}
		}

		/// <summary>
		/// Fill color type, which determines fill brush for this grip point
		/// </summary>
		protected virtual eColorType FillColorType { get { return eColorType.eGripPointFillColor; } }
		/// <summary>
		/// Grip point fill brush
		/// </summary>
		protected Brush FillBrush
		{
			get
			{
				Color fillColor = Colors.Blue;

				if (CurrentGeometryColorsTheme.CurrentTheme != null)
				{
					Color colorValue;
					if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(FillColorType, out colorValue))
						fillColor = colorValue;
				}

				return new SolidColorBrush(fillColor);
			}
		}

		#endregion

		#region Methods

		//=============================================================================
		public Point GetGlobalPoint()
		{
			if (m_Geometry != null && m_gripIndex >= 0)
			{
				List<Point> pnts = m_Geometry.GetGripPoints();
				if (pnts != null && m_gripIndex < pnts.Count)
					return pnts[m_gripIndex];
			}

			return new Point();
		}

		//=============================================================================
		public virtual Point GetGripScreenPoint()
		{
			if (m_DC != null && m_Geometry != null && m_Geometry.Sheet != null)
				return m_DC.GetLocalPoint(m_Geometry.Sheet, GetGlobalPoint());

			return new Point(0.0, 0.0);
		}

		//=============================================================================
		protected double _ConvertToScreenLength(double rGlobalLength)
		{
			if (m_DC != null && m_Geometry != null && m_Geometry.Sheet != null)
				return m_DC.GetWidthInPixels(rGlobalLength, m_Geometry.Sheet.UnitsPerCameraPixel);

			return 0.0;
		}

		//=============================================================================
		protected double _ConvertToScreenWidth(double rGlobalWidth)
		{
			if (m_DC != null && m_Geometry != null && m_Geometry.Sheet != null)
				return m_DC.GetHeightInPixels(rGlobalWidth, m_Geometry.Sheet.UnitsPerCameraPixel);

			return 0.0;
		}

		//=============================================================================
		public virtual bool Move(Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			bool result = false;
			if (m_Geometry != null && m_gripIndex >= 0)
				result = m_Geometry.SetGripPoint(m_gripIndex, globalPoint, DrawingLength, DrawingWidth);
			this.Draw(true);

			return result;
		}

		//=============================================================================
		public void Draw(bool bShow)
		{
			using (DrawingContext dc = this.RenderOpen())
			{
				// if there is no one line to draw you steel need to call RenderOpen() to remove all old lines
				if (!bShow)
					return;

				if (m_DC == null)
					return;

				// Apply clip only if sheet is not fully displayed.
				bool bApplyClip = false;
				if (m_Geometry != null && m_Geometry.Sheet != null && !m_Geometry.Sheet.IsSheetFullyDisplayed)
					bApplyClip = true;
				// DrawingControl can display part of sheet if it is scales and have offset.
				// So clip any drawing which is placed outside control.
				if (bApplyClip)
				{
					Point clipRectTopLeftPnt = new Point(0.0, 0.0);
					Point clipRectBotRightPnt = new Point(m_DC.ActualWidth, m_DC.ActualHeight);
					dc.PushClip(new RectangleGeometry(new Rect(clipRectTopLeftPnt, clipRectBotRightPnt)));
				}

				this._Draw(dc);

				if(bApplyClip)
					dc.Pop();
			}
		}
		protected virtual void _Draw(DrawingContext dc)
		{
			if (dc == null)
				return;

			Vector vec = new Vector(sGripSize, sGripSize);
			Point pnt = GetGripScreenPoint();
			Point pnt1 = pnt - vec;
			Point pnt2 = pnt + vec;
			Rect rect = new Rect(pnt1, pnt2);
			Pen pen = new Pen(this.BorderBrush, 1.0);

			dc.DrawRectangle(this.FillBrush, pen, rect);
		}

		#endregion
	}

	// Rotate geometry grip point.
	public class RotateGripPoint : GripPoint
	{
		public RotateGripPoint(DrawingControl dc, BaseRectangleGeometry owner, int gripIndex, int rotateGripIndex)
			: base(dc, owner, gripIndex)
		{
			m_RotateGripIndex = rotateGripIndex;
		}

		#region Properties

		private double m_rArrowSize = 4;
		private double m_rOffset = 6;
		private double m_rRadius = 7.5;
		private int m_RotateGripIndex = -1;

		#endregion

		#region Methods

		//=============================================================================
		public void Rotate(double DrawingLength, double DrawingWidth)
		{
			if (m_Geometry != null)
			{
				if(m_Geometry is Rack)
					m_Geometry.Rotate(DrawingLength, DrawingWidth);
				else if(m_Geometry is Column)
					m_Geometry.Rotate(DrawingLength, DrawingWidth, BaseRectangleGeometry.eRotateRelativePoint.eCenter, true, false);

				// check max available height for the rack because it depends on the roof properties and rack position
				Rack changedRack = m_Geometry as Rack;
				if (changedRack != null && changedRack.IsInit)
					changedRack.CheckRackHeight();
			}
		}

		//=============================================================================
		public override bool Move(Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			return false;
		}

		//=============================================================================
		protected override void _Draw(DrawingContext dc)
		{
			if (dc == null)
				return;

			Pen pen = new Pen(this.FillBrush, 2.5);

			// ------------ DRAW ARC
			PathGeometry pathArcGeom = new PathGeometry();

			Point startPoint = GetGripScreenPoint();
			Point endPoint = startPoint;
			if (GripPoint.GRIP_ROTATE == m_RotateGripIndex)
			{
				startPoint.Y -= m_rRadius;
				startPoint.Y -= m_rOffset;

				endPoint = startPoint;
				endPoint.Y -= m_rRadius;
				endPoint.X += m_rRadius;
			}

			PathFigure pathFigure = new PathFigure();
			pathFigure.StartPoint = startPoint;
			pathFigure.IsClosed = false;
			pathArcGeom.Figures.Add(pathFigure);

			ArcSegment arcSegment = new ArcSegment(endPoint, new Size(m_rRadius, m_rRadius), 0, true, SweepDirection.Clockwise, true);
			pathFigure.Segments.Add(arcSegment);

			dc.DrawGeometry(null, pen, pathArcGeom);


			// ----------- DRAW ARROW
			PathGeometry pathArrowGeom = new PathGeometry();

			//
			RotateTransform arrRotateTransform = null;
			//
			PathFigure pathArrow = new PathFigure();
			pathArrow.IsClosed = false;
			// arrow
			if (GripPoint.GRIP_ROTATE == m_RotateGripIndex)
			{
				endPoint.Y += 1;

				Point arrPnt1 = endPoint;
				arrPnt1.Y -= m_rArrowSize;
				arrPnt1.X += m_rArrowSize;

				Point arrPnt2 = endPoint;

				Point arrPnt3 = endPoint;
				arrPnt3.Y -= m_rArrowSize;
				arrPnt3.X -= m_rArrowSize;

				pathArrow.StartPoint = arrPnt1;

				LineSegment line1 = new LineSegment(arrPnt2, true);
				pathArrow.Segments.Add(line1);

				LineSegment line2 = new LineSegment(arrPnt3, true);
				pathArrow.Segments.Add(line2);

				//
				arrRotateTransform = new RotateTransform(-10, endPoint.X, endPoint.Y);
			}
			pathArrowGeom.Figures.Add(pathArrow);

			if (arrRotateTransform != null)
				dc.PushTransform(arrRotateTransform);

			dc.DrawGeometry(null, pen, pathArrowGeom);

			if (arrRotateTransform != null)
				dc.Pop();
		}

		#endregion
	}

	/// <summary>
	/// Create horizontal rack row grip point.
	/// </summary>
	public class CreateRackRowGripPoint : GripPoint
	{
		public CreateRackRowGripPoint(DrawingControl drawing, BaseRectangleGeometry owner, int gripIndex)
			: base(drawing, owner, gripIndex) { }

		#region Properties

		protected override eColorType FillColorType { get { return eColorType.eCreateRackRowGripPointFillColor; } }

		#endregion

		#region Methods

		//=============================================================================
		public override bool Move(Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			if (DrawingDocument._sDrawing == null)
				return false;

			Rack baseRack = this.Geometry as Rack;
			if (baseRack == null)
				return false;
			DrawingSheet sheet = baseRack.Sheet;
			if (sheet == null)
				return false;

			if (globalPoint.X < 0 || globalPoint.Y < 0)
				return false;
			if (globalPoint.X > sheet.Length || globalPoint.Y > sheet.Width)
				return false;

			try
			{
				// the first rack in the row must have 100% width, all others decreased by 80 mm
				double rRackLength = baseRack.Length_X;
				if (baseRack.IsFirstInRowColumn)
				{
					rRackLength = rRackLength - baseRack.DiffBetween_M_and_A;
					rRackLength = Utils.CheckWholeNumber(rRackLength, baseRack.MinLength_X, baseRack.MaxLength_X);
				}

				double rDistanceFromOriginalRack = globalPoint.X - (baseRack.TopLeft_GlobalPoint.X + baseRack.Length_X);
				// only right direction
				if (rDistanceFromOriginalRack < 0)
					return false;
				//
				int tempRackCount = Convert.ToInt32(Math.Truncate(rDistanceFromOriginalRack / rRackLength));

				if (sheet.TemporaryRacksList.Count < tempRackCount)
				{
					// need to create
					for (int i = sheet.TemporaryRacksList.Count; i < tempRackCount; ++i)
					{
						Rack prevRack = null;
						if (i == 0)
							prevRack = baseRack;
						else
							prevRack = sheet.TemporaryRacksList[i - 1] as Rack;
						if (prevRack == null)
							continue;

						Point tempRackTopLeftPoint = prevRack.TopRight_GlobalPoint;
						tempRackTopLeftPoint.X += Rack.sHorizontalRow_GlobalGap;
						//
						Rack tempRack = prevRack.Clone() as Rack;
						if (tempRack == null)
							return false;
						tempRack.Sheet = sheet;
						tempRack.TopLeft_GlobalPoint = tempRackTopLeftPoint;
						tempRack.Length_X = rRackLength;
						//
						tempRack.IsFirstInRowColumn = false;

						// check layout for new temp rack
						// if layout is not correct then stop row\column creating
						if (!sheet.IsLayoutCorrect(tempRack))
							break;

						//
						sheet.TemporaryRacksList.Add(tempRack);
					}

					if (DrawingDocument._sDrawing != null)
						DrawingDocument._sDrawing.UpdateDrawing(true);
				}
				else if (tempRackCount < sheet.TemporaryRacksList.Count)
				{
					sheet.TemporaryRacksList.RemoveRange(tempRackCount, sheet.TemporaryRacksList.Count - tempRackCount);
					if (DrawingDocument._sDrawing != null)
						DrawingDocument._sDrawing.UpdateDrawing(true);
				}

				return true;
			}
			catch { }

			return false;
		}

		//=============================================================================
		protected override void _Draw(DrawingContext dc)
		{
			if (dc == null)
				return;

			Pen pen = new Pen(this.BorderBrush, 2.0);
			Brush fillBrush = this.FillBrush;

			Point pnt = GetGripScreenPoint();
			pnt.X += 3 * sGripSize;
			pnt.Y += 3 * sGripSize;

			Point pnt1 = pnt;
			pnt1.X -= 2;
			pnt1.Y -= 2;
			Point pnt2 = pnt;
			pnt2.X += 2;
			pnt2.Y += 2;

			Rect rect = new Rect(pnt1, pnt2);
			dc.DrawRectangle(fillBrush, pen, rect);

			pnt1.X += 2 * sGripSize;
			pnt2.X += 2 * sGripSize;
			rect = new Rect(pnt1, pnt2);
			dc.DrawRectangle(fillBrush, pen, rect);

			pnt1.X += 2 * sGripSize;
			pnt2.X += 2 * sGripSize;
			rect = new Rect(pnt1, pnt2);
			dc.DrawRectangle(fillBrush, pen, rect);
		}

		#endregion
	}

	/// <summary>
	/// Create vertical rack column grip point.
	/// </summary>
	public class CreateRackColumnGripPoint : GripPoint
	{
		public CreateRackColumnGripPoint(DrawingControl drawing, BaseRectangleGeometry owner, int gripIndex)
			: base(drawing, owner, gripIndex) { }

		#region Properties

		protected override eColorType FillColorType { get { return eColorType.eCreateRackRowGripPointFillColor; } }

		#endregion

		#region Methods

		//=============================================================================
		public override bool Move(Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			if (DrawingDocument._sDrawing == null)
				return false;

			Rack baseRack = this.Geometry as Rack;
			if (baseRack == null)
				return false;
			DrawingSheet sheet = baseRack.Sheet;
			if (sheet == null)
				return false;

			if (globalPoint.X < 0 || globalPoint.Y < 0)
				return false;
			if (globalPoint.X > sheet.Length || globalPoint.Y > sheet.Width)
				return false;

			try
			{
				// the first rack in the column must have 100% height, all others - 90%
				double rRackWidth = baseRack.Length_Y;
				if (baseRack.IsFirstInRowColumn)
				{
					rRackWidth = rRackWidth - baseRack.DiffBetween_M_and_A;
					rRackWidth = Utils.CheckWholeNumber(rRackWidth, baseRack.MinLength_Y, baseRack.MaxLength_Y);
				}

				double rDistanceFromOriginalRack = globalPoint.Y - baseRack.BottomLeft_GlobalPoint.Y;
				// only down direction
				if (rDistanceFromOriginalRack < 0)
					return false;

				//
				int tempRackCount = Convert.ToInt32(Math.Truncate(rDistanceFromOriginalRack / rRackWidth));

				if (sheet.TemporaryRacksList.Count < tempRackCount)
				{
					// need to create
					for (int i = sheet.TemporaryRacksList.Count; i < tempRackCount; ++i)
					{
						Rack prevRack = null;
						if (i == 0)
							prevRack = baseRack;
						else
							prevRack = sheet.TemporaryRacksList[i - 1] as Rack;
						if (prevRack == null)
							continue;

						Point tempRackTopLeftPoint = prevRack.BottomLeft_GlobalPoint;
						tempRackTopLeftPoint.Y += Rack.sVerticalColumn_GlobalGap;
						//
						Rack tempRack = prevRack.Clone() as Rack;
						if (tempRack == null)
							return false;
						tempRack.Sheet = sheet;
						tempRack.TopLeft_GlobalPoint = tempRackTopLeftPoint;
						tempRack.Length_Y = rRackWidth;
						//
						tempRack.IsFirstInRowColumn = false;

						// check layout for new temp rack
						// if layout is not correct then stop row\column creating
						if (!sheet.IsLayoutCorrect(tempRack))
							break;

						//
						sheet.TemporaryRacksList.Add(tempRack);
					}

					if (DrawingDocument._sDrawing != null)
						DrawingDocument._sDrawing.UpdateDrawing(true);
				}
				else if (tempRackCount < sheet.TemporaryRacksList.Count)
				{
					sheet.TemporaryRacksList.RemoveRange(tempRackCount, sheet.TemporaryRacksList.Count - tempRackCount);
					if (DrawingDocument._sDrawing != null)
						DrawingDocument._sDrawing.UpdateDrawing(true);
				}

				return true;
			}
			catch { }

			return false;
		}

		//=============================================================================
		protected override void _Draw(DrawingContext dc)
		{
			if (dc == null)
				return;

			Pen pen = new Pen(this.BorderBrush, 2.0);
			Brush fillBrush = this.FillBrush;

			Point startPoint = GetGripScreenPoint();
			startPoint.X += 2 * sGripSize;
			startPoint.Y += 2 * sGripSize;

			Point pnt1 = startPoint;
			Point pnt2 = startPoint;
			pnt2.X += GripPoint.sGripSize;
			pnt2.Y += GripPoint.sGripSize;
			Rect rect = new Rect(pnt1, pnt2);
			dc.DrawRectangle(fillBrush, pen, rect);

			pnt1.Y += 2 * sGripSize;
			pnt2.Y += 2 * sGripSize;
			rect = new Rect(pnt1, pnt2);
			dc.DrawRectangle(fillBrush, pen, rect);

			pnt1.Y += 2 * sGripSize;
			pnt2.Y += 2 * sGripSize;
			rect = new Rect(pnt1, pnt2);
			dc.DrawRectangle(fillBrush, pen, rect);
		}

		#endregion
	}

	/// <summary>
	/// Selects the rack row or column group, which contains this.Owner rack.
	/// </summary>
	public class SelectRackGroupGripPoint : GripPoint
	{
		public SelectRackGroupGripPoint(DrawingControl drawing, BaseRectangleGeometry owner)
			: base(drawing, owner, BaseRectangleGeometry.GRIP_CENTER) { }

		#region Properties

		protected override eColorType FillColorType { get { return eColorType.eSelectRackRowGripPointFillColor; } }

		#endregion

		#region Methods

		//=============================================================================
		public override bool Move(Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			return false;
		}

		//=============================================================================
		protected override void _Draw(DrawingContext dc)
		{
			if (dc == null)
				return;

			Pen pen = new Pen(this.BorderBrush, 2.0);
			Brush fillBrush = Brushes.Aqua;

			Point startPoint = GetGripScreenPoint();
			startPoint.X -= 3 * sGripSize;

			dc.DrawEllipse(fillBrush, pen, startPoint, GripPoint.sGripSize, GripPoint.sGripSize);
		}

		#endregion
	}

	/// <summary>
	/// Moves racks row or column group.
	/// </summary>
	public class MoveRacksGroupGripPoint : GripPoint
	{
		public MoveRacksGroupGripPoint(DrawingControl drawing, List<Rack> racksGroup)
			: base(drawing, null, -1)
		{
			if (racksGroup != null)
				m_RacksGroup.AddRange(racksGroup);
		}

		#region Properties

		protected override eColorType FillColorType { get { return eColorType.eRacksGroupGripPointFillColor; } }

		//=============================================================================
		List<Rack> m_RacksGroup = new List<Rack>();
		public List<Rack> RacksGroup { get { return m_RacksGroup; } }

		//=============================================================================
		private Rack First
		{
			get
			{
				if (m_RacksGroup.Count > 0)
					return m_RacksGroup[0];

				return null;
			}
		}

		//=============================================================================
		private Rack Last
		{
			get
			{
				if (m_RacksGroup.Count > 0)
					return m_RacksGroup[m_RacksGroup.Count - 1];

				return null;
			}
		}

		//=============================================================================
		private Point Center_GlobalPoint
		{
			get
			{
				Point gripCenter_ScreenPoint = new Point();

				if (m_DC != null && m_RacksGroup.Count > 0 && m_RacksGroup[0].Sheet != null)
				{
					if (m_DC != null && m_Geometry != null && m_Geometry.Sheet != null)
						return m_DC.GetLocalPoint(m_Geometry.Sheet, GetGlobalPoint());

					gripCenter_ScreenPoint = m_DC.GetLocalPoint(m_RacksGroup[0].Sheet, First.TopLeft_GlobalPoint);
					gripCenter_ScreenPoint.X += m_DC.GetWidthInPixels(_GetGlobalLength() / 2, m_RacksGroup[0].Sheet.UnitsPerCameraPixel);
					gripCenter_ScreenPoint.Y += m_DC.GetHeightInPixels(_GetGlobalWidth() / 2, m_RacksGroup[0].Sheet.UnitsPerCameraPixel);
					// make it whole number
					gripCenter_ScreenPoint.X = Convert.ToInt32(Math.Truncate(gripCenter_ScreenPoint.X));
					gripCenter_ScreenPoint.Y = Convert.ToInt32(Math.Truncate(gripCenter_ScreenPoint.Y));
				}

				return gripCenter_ScreenPoint;
			}
		}

		#endregion

		#region Methods

		//=============================================================================
		private double _GetGlobalLength()
		{
			if (m_RacksGroup.Count > 0)
			{
				if (First.IsHorizontal)
					return Last.TopRight_GlobalPoint.X - First.TopLeft_GlobalPoint.X;
				else
					return First.Length_X;
			}

			return 0.0;
		}

		//=============================================================================
		private double _GetGlobalWidth()
		{
			if (m_RacksGroup.Count > 0)
			{
				if (First.IsHorizontal)
					return First.Length_Y;
				else
					return Last.BottomLeft_GlobalPoint.Y - First.TopLeft_GlobalPoint.Y;
			}

			return 0.0;
		}

		//=============================================================================
		public override bool Move(Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			Rack _first = First;
			Rack _last = Last;
			if (_first == null || _last == null)
				return false;

			if (m_DC == null)
				return false;

			DrawingSheet _currSheet = m_DC.Sheet;
			if (_currSheet == null)
				return false;

			Point newCenterGlobalPnt = globalPoint;
			//
			double groupLength = _GetGlobalLength();
			double groupWidth = _GetGlobalWidth();
			double marginX = _first.MarginX;
			double marginY = _first.MarginY;

			// check borders
			newCenterGlobalPnt = Utils.CheckBorders(newCenterGlobalPnt, 0, 0, DrawingLength - groupLength / 2, DrawingWidth - groupWidth / 2, marginX, marginY);

			// get old center point
			Point oldCenter_GlobalPoint = Center_GlobalPoint;

			// calc new top left point for entire row\column
			Point newTopLef_GlobalPoint = newCenterGlobalPnt;
			newTopLef_GlobalPoint.X -= groupLength / 2;
			newTopLef_GlobalPoint.Y -= groupWidth / 2;
			// make it whole number
			newTopLef_GlobalPoint.X = Convert.ToInt32(Math.Truncate(newTopLef_GlobalPoint.X));
			newTopLef_GlobalPoint.Y = Convert.ToInt32(Math.Truncate(newTopLef_GlobalPoint.Y));
			//
			newTopLef_GlobalPoint = Utils.CheckBorders(newTopLef_GlobalPoint, 0, 0, DrawingLength, DrawingWidth, marginX, marginY);

			// check layout
			Rack temporaryRack = _first.Clone() as Rack;
			if (temporaryRack == null)
				return false;
			temporaryRack.Sheet = _currSheet;
			temporaryRack.TopLeft_GlobalPoint = newTopLef_GlobalPoint;
			temporaryRack.Length_X = groupLength;
			temporaryRack.Length_Y = groupWidth;

			//
			List<BaseRectangleGeometry> _rectanglesToIgnore = new List<BaseRectangleGeometry>();
			if(m_RacksGroup != null)
				_rectanglesToIgnore.AddRange(m_RacksGroup);
			//
			List<BaseRectangleGeometry> overlappedRectangles;
			if (!_currSheet.IsLayoutCorrect(temporaryRack, _rectanglesToIgnore, out overlappedRectangles))
			{
				// try to fix it
				Point newTopLeftPoint;
				double newGlobalLength;
				double newGlobalWidth;
				//
				// infinity loop protection
				int iMaxLoopCount = 100;
				int iLoopCount = 0;
				//
				while (temporaryRack._CalculateNotOverlapPosition(overlappedRectangles, BaseRectangleGeometry.GRIP_CENTER, DrawingLength, DrawingWidth, out newTopLeftPoint, out newGlobalLength, out newGlobalWidth))
				{
					temporaryRack.TopLeft_GlobalPoint = newTopLeftPoint;

					//
					if (_currSheet.IsLayoutCorrect(temporaryRack, _rectanglesToIgnore, out overlappedRectangles))
						break;

					++iLoopCount;
					if (iLoopCount >= iMaxLoopCount)
						break;
				}

				if (!_currSheet.IsLayoutCorrect(temporaryRack, _rectanglesToIgnore, out overlappedRectangles))
					return false;
				else
					newTopLef_GlobalPoint = temporaryRack.TopLeft_GlobalPoint;
			}

			// update all racks
			First.TopLeft_GlobalPoint = newTopLef_GlobalPoint;
			for(int i=1; i<m_RacksGroup.Count; ++i)
			{
				Rack prevRack = m_RacksGroup[i - 1];
				Rack currentRack = m_RacksGroup[i];

				if(prevRack != null && currentRack != null)
				{
					Point curRackTopLeft_GlobalPoint = new Point();
					if (currentRack.IsHorizontal)
					{
						curRackTopLeft_GlobalPoint = prevRack.TopRight_GlobalPoint;
						curRackTopLeft_GlobalPoint.X += Rack.sHorizontalRow_GlobalGap;
					}
					else
					{
						curRackTopLeft_GlobalPoint = prevRack.BottomLeft_GlobalPoint;
						curRackTopLeft_GlobalPoint.Y += Rack.sVerticalColumn_GlobalGap;
					}

					currentRack.TopLeft_GlobalPoint = curRackTopLeft_GlobalPoint;
				}
			}

			return true;
		}

		//=============================================================================
		protected override void _Draw(DrawingContext dc)
		{
			if (dc == null)
				return;

			if (m_RacksGroup.Count < 1)
				return;

			// calc center point
			Point gripCenter_ScreenPoint = Center_GlobalPoint;

			Vector vec = new Vector(sGripSize, sGripSize);
			Point pnt1 = gripCenter_ScreenPoint - vec;
			Point pnt2 = gripCenter_ScreenPoint + vec;
			Rect rect = new Rect(pnt1, pnt2);

			Pen pen = new Pen(this.BorderBrush, 1.0);

			dc.DrawRectangle(this.FillBrush, pen, rect);
		}

		#endregion
	}

	/// <summary>
	/// Stretches racks in row or column group in X or Y direction.
	/// </summary>
	public class StretchRacksGroupGripPoint : GripPoint
	{
		public StretchRacksGroupGripPoint(DrawingControl drawing, List<Rack> racksRowColumn, int iGripIndex)
			: base(drawing, null, iGripIndex)
		{
			if (racksRowColumn != null)
				m_RacksRowColumn.AddRange(racksRowColumn);
		}

		#region Properties

		protected override eColorType FillColorType { get { return eColorType.eRacksGroupGripPointFillColor; } }

		//=============================================================================
		List<Rack> m_RacksRowColumn = new List<Rack>();
		public List<Rack> Racks { get { return m_RacksRowColumn; } }

		//=============================================================================
		private Rack First
		{
			get
			{
				if (m_RacksRowColumn.Count > 0)
					return m_RacksRowColumn[0];

				return null;
			}
		}

		//=============================================================================
		private Rack Last
		{
			get
			{
				if (m_RacksRowColumn.Count > 0)
					return m_RacksRowColumn[m_RacksRowColumn.Count - 1];

				return null;
			}
		}

		//=============================================================================
		private Point Center_GlobalPoint
		{
			get
			{
				Point gripCenter_ScreenPoint = new Point();

				if (m_DC != null && m_RacksRowColumn.Count > 0 && m_RacksRowColumn[0].Sheet != null)
				{
					gripCenter_ScreenPoint = m_DC.GetLocalPoint(m_RacksRowColumn[0].Sheet, First.TopLeft_GlobalPoint);
					gripCenter_ScreenPoint.X += m_DC.GetWidthInPixels(_GetGlobalLength() / 2, m_RacksRowColumn[0].Sheet.UnitsPerCameraPixel);
					gripCenter_ScreenPoint.Y += m_DC.GetHeightInPixels(_GetGlobalWidth() / 2, m_RacksRowColumn[0].Sheet.UnitsPerCameraPixel);
					// make it whole number
					gripCenter_ScreenPoint.X = Convert.ToInt32(Math.Truncate(gripCenter_ScreenPoint.X));
					gripCenter_ScreenPoint.Y = Convert.ToInt32(Math.Truncate(gripCenter_ScreenPoint.Y));
				}

				return gripCenter_ScreenPoint;
			}
		}

		#endregion

		#region Methods

		//=============================================================================
		private double _GetGlobalLength()
		{
			if (m_RacksRowColumn.Count > 0)
			{
				if (First.IsHorizontal)
					return Last.TopRight_GlobalPoint.X - First.TopLeft_GlobalPoint.X;
				else
					return First.Length_X;
			}

			return 0.0;
		}

		//=============================================================================
		private double _GetGlobalWidth()
		{
			if (m_RacksRowColumn.Count > 0)
			{
				if (First.IsHorizontal)
					return First.Length_Y;
				else
					return Last.BottomLeft_GlobalPoint.Y - First.TopLeft_GlobalPoint.Y;
			}

			return 0.0;
		}

		//=============================================================================
		private int _Get_CountOfAbledToChange(bool bIncrease)
		{
			int iCount = 0;

			foreach (Rack r in m_RacksRowColumn)
			{
				if (_IsRackAbleToChange(r, bIncrease))
					++iCount;
			}

			return iCount;
		}

		//=============================================================================
		private bool _IsRackAbleToChange(Rack r, bool bIncrease)
		{
			if (r == null)
				return false;

			if (r.IsHorizontal)
			{
				if (bIncrease)
				{
					if (r.MaxLength_X - r.Length_X >= r.StepLength_X)
						return true;
				}
				else
				{
					if (r.Length_X - r.MinLength_X >= r.StepLength_X)
						return true;
				}
			}
			else
			{
				if (bIncrease)
				{
					if (r.MaxLength_Y - r.Length_Y >= r.StepLength_Y)
						return true;
				}
				else
				{
					if (r.Length_Y - r.MinLength_Y >= r.StepLength_Y)
						return true;
				}
			}

			return false;
		}

		//=============================================================================
		public override bool Move(Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			//
			if (BaseRectangleGeometry.GRIP_TOP_LEFT != m_gripIndex && BaseRectangleGeometry.GRIP_BOTTOM_RIGHT != m_gripIndex)
				return false;

			//
			Rack _first = First;
			Rack _last = Last;
			if (_first == null || _last == null)
				return false;

			//
			if (m_DC == null)
				return false;

			DrawingSheet _currSheet = m_DC.Sheet;
			if (_currSheet == null)
				return false;

			//
			globalPoint = Utils.CheckBorders(globalPoint, 0, 0, DrawingLength, DrawingWidth, 0, 0);

			// old value of point to move
			Point old_GlobalPoint = new Point();
			// point that doesnt move
			Point fixed_GlobalPoint = new Point();
			if (BaseRectangleGeometry.GRIP_TOP_LEFT == m_gripIndex)
			{
				fixed_GlobalPoint = _last.BottomRight_GlobalPoint;
				old_GlobalPoint = _first.TopLeft_GlobalPoint;
			}
			else if (BaseRectangleGeometry.GRIP_BOTTOM_RIGHT == m_gripIndex)
			{
				fixed_GlobalPoint = _first.TopLeft_GlobalPoint;
				old_GlobalPoint = _last.BottomRight_GlobalPoint;
			}

			//
			double oldLength = _GetGlobalLength();
			double oldWidth = _GetGlobalWidth();
			//
			double marginX = _first.MarginX;
			double marginY = _first.MarginY;

			//
			globalPoint = Utils.GetWholePoint(globalPoint);
			globalPoint = Utils.CheckBorders(globalPoint, 0, 0, DrawingLength, DrawingWidth, marginX, marginY);

			// new rack's row\column coordiantes
			Point newTopLeft_GlobalPoint = _first.TopLeft_GlobalPoint;
			Point newBotRight_GlobalPoint = _last.BottomRight_GlobalPoint;
			double newLength = oldLength;
			double newWidth = oldWidth;
			//
			if(BaseRectangleGeometry.GRIP_TOP_LEFT == m_gripIndex)
				newTopLeft_GlobalPoint = globalPoint;
			else if (BaseRectangleGeometry.GRIP_BOTTOM_RIGHT == m_gripIndex)
				newBotRight_GlobalPoint = globalPoint;
			//
			newTopLeft_GlobalPoint = Utils.CheckBorders(newTopLeft_GlobalPoint, 0, 0, DrawingLength, DrawingWidth, marginX, marginY);
			newBotRight_GlobalPoint = Utils.CheckBorders(newBotRight_GlobalPoint, 0, 0, DrawingLength, DrawingWidth, marginX, marginY);
			//
			newLength = newBotRight_GlobalPoint.X - newTopLeft_GlobalPoint.X;
			newWidth = newBotRight_GlobalPoint.Y - newTopLeft_GlobalPoint.Y;
			// check min max values
			if (_first.IsHorizontal)
			{
				// check height
				newWidth = Utils.CheckWholeNumber(newWidth, _first.MinLength_Y, _first.MaxLength_Y);
			}
			else
			{
				// check width
				newLength = Utils.CheckWholeNumber(newLength, _first.MinLength_X, _first.MaxLength_X);
			}
			//
			newLength = Utils.GetWholeNumber(newLength);
			newWidth = Utils.GetWholeNumber(newWidth);
			//
			if (newLength <= 0 || newWidth <= 0)
				return false;

			// change all racks by step
			// check able to change count
			//
			bool bIncrease = true;
			if (_first.IsHorizontal)
				bIncrease = newLength > oldLength;
			else
				bIncrease = newWidth > oldWidth;
			// count of racks that are able to change in the row\column direction
			int _CountAbleToChange = _Get_CountOfAbledToChange(bIncrease);
			//
			// correct width and height
			int _iStepMultiplier = 0;
			if (_first.IsHorizontal)
			{
				if (_CountAbleToChange > 0)
				{
					double lengthDelta = newLength - oldLength;
					lengthDelta = Utils.GetWholeNumberByStep(lengthDelta, _first.StepLength_X * _CountAbleToChange);
					_iStepMultiplier = Utils.GetWholeNumber(lengthDelta / (_first.StepLength_X * _CountAbleToChange));
					newLength = oldLength + lengthDelta;
				}
				else
					newLength = oldLength;

				double widthDelta = newWidth - oldWidth;
				widthDelta = Utils.GetWholeNumberByStep(widthDelta, _first.StepLength_Y);
				newWidth = oldWidth + widthDelta;
			}
			else
			{
				double lengthDelta = newLength - oldLength;
				lengthDelta = Utils.GetWholeNumberByStep(lengthDelta, _first.StepLength_X);
				newLength = oldLength + lengthDelta;

				if (_CountAbleToChange > 0)
				{
					double widthDelta = newWidth - oldWidth;
					widthDelta = Utils.GetWholeNumberByStep(widthDelta, _first.StepLength_Y * _CountAbleToChange);
					_iStepMultiplier = Utils.GetWholeNumber(widthDelta / (_first.StepLength_Y * _CountAbleToChange));
					newWidth = oldWidth + widthDelta;
				}
				else
					newWidth = oldWidth;
			}
			//
			if (newLength == oldLength && newWidth == oldLength)
				return false;
			// correct topleft or botright point
			if(BaseRectangleGeometry.GRIP_TOP_LEFT == m_gripIndex)
			{
				newTopLeft_GlobalPoint = newBotRight_GlobalPoint;
				newTopLeft_GlobalPoint.X -= newLength;
				newTopLeft_GlobalPoint.Y -= newWidth;
				//
				newTopLeft_GlobalPoint = Utils.GetWholePoint(newTopLeft_GlobalPoint);
			}
			else if(BaseRectangleGeometry.GRIP_BOTTOM_RIGHT == m_gripIndex)
			{
				newBotRight_GlobalPoint = newTopLeft_GlobalPoint;
				newBotRight_GlobalPoint.X += newLength;
				newBotRight_GlobalPoint.Y += newWidth;
				//
				newBotRight_GlobalPoint = Utils.GetWholePoint(newBotRight_GlobalPoint);
			}


			// check layout
			Rack temporaryRack = _first.Clone() as Rack;
			if (temporaryRack == null)
				return false;
			temporaryRack.Sheet = _currSheet;
			temporaryRack.TopLeft_GlobalPoint = newTopLeft_GlobalPoint;
			temporaryRack.Length_X = newLength;
			temporaryRack.Length_Y = newWidth;
			//
			List<BaseRectangleGeometry> _rectanglesToIgnore = new List<BaseRectangleGeometry>();
			if (m_RacksRowColumn != null)
				_rectanglesToIgnore.AddRange(m_RacksRowColumn);
			//
			List<BaseRectangleGeometry> overlappedRectangles;
			if (!_currSheet.IsLayoutCorrect(temporaryRack, _rectanglesToIgnore, out overlappedRectangles))
			{
				return false;
			}

			// update all racks
			First.TopLeft_GlobalPoint = newTopLeft_GlobalPoint;
			for (int i = 0; i < m_RacksRowColumn.Count; ++i)
			{
				Rack currentRack = m_RacksRowColumn[i];
				//
				newTopLeft_GlobalPoint = Utils.GetWholePoint(newTopLeft_GlobalPoint);
				currentRack.TopLeft_GlobalPoint = newTopLeft_GlobalPoint;
				//
				if (currentRack.IsHorizontal)
				{
					// width
					if (this._IsRackAbleToChange(currentRack, bIncrease))
						currentRack.Length_X += _iStepMultiplier * currentRack.StepLength_X;

					// height
					currentRack.Length_Y = newWidth;
				}
				else
				{
					// width
					currentRack.Length_X = newLength;

					// height
					if (this._IsRackAbleToChange(currentRack, bIncrease))
						currentRack.Length_Y += _iStepMultiplier * currentRack.StepLength_Y;
				}
				// whole numbers only
				currentRack.Length_X = Utils.CheckWholeNumber(currentRack.Length_X, currentRack.MinLength_X, currentRack.MaxLength_X);
				currentRack.Length_Y = Utils.CheckWholeNumber(currentRack.Length_Y, currentRack.MinLength_Y, currentRack.MaxLength_Y);

				if (currentRack.IsHorizontal)
				{
					newTopLeft_GlobalPoint = currentRack.TopRight_GlobalPoint;
					newTopLeft_GlobalPoint.X += Rack.sHorizontalRow_GlobalGap;
				}
				else
				{
					newTopLeft_GlobalPoint = currentRack.BottomLeft_GlobalPoint;
					newTopLeft_GlobalPoint.Y += Rack.sVerticalColumn_GlobalGap;
				}
			}

			return true;
		}

		//=============================================================================
		protected override void _Draw(DrawingContext dc)
		{
			if (dc == null)
				return;

			if (m_RacksRowColumn.Count < 1)
				return;

			if (m_RacksRowColumn[0].Sheet == null)
				return;

			Rack first = First;
			Rack last = Last;
			if (first == null || last == null)
				return;

			// calc center point
			Point grip_ScreenPoint = new Point();
			if (BaseRectangleGeometry.GRIP_TOP_LEFT == m_gripIndex)
				grip_ScreenPoint = m_DC.GetLocalPoint(m_RacksRowColumn[0].Sheet, first.TopLeft_GlobalPoint);
			else if (BaseRectangleGeometry.GRIP_BOTTOM_RIGHT == m_gripIndex)
				grip_ScreenPoint = m_DC.GetLocalPoint(m_RacksRowColumn[0].Sheet, last.BottomRight_GlobalPoint);

			Vector vec = new Vector(sGripSize, sGripSize);
			Point pnt1 = grip_ScreenPoint - vec;
			Point pnt2 = grip_ScreenPoint + vec;
			Rect rect = new Rect(pnt1, pnt2);

			Pen pen = new Pen(this.BorderBrush, 1.0);

			dc.DrawRectangle(this.FillBrush, pen, rect);
		}

		#endregion
	}

	/// <summary>
	/// Creates column by defined pattern, using this.Owner column as initial position.
	/// </summary>
	public class CreateColumnPattern_GripPoint : GripPoint
	{
		public CreateColumnPattern_GripPoint(DrawingControl drawing, BaseRectangleGeometry columnOwner)
			: base(drawing, columnOwner, BaseRectangleGeometry.GRIP_BOTTOM_RIGHT)
		{
		}

		#region Properties

		protected override eColorType FillColorType { get { return eColorType.eCreateColumnPatternGripPointFillColor; } }

		#endregion

		#region Methods

		//=============================================================================
		public override bool Move(Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			return false;
		}

		//=============================================================================
		protected override void _Draw(DrawingContext dc)
		{
			if (dc == null)
				return;

			Pen pen = new Pen(this.BorderBrush, 2.0);

			Point pnt = GetGripScreenPoint();
			pnt.X += 3 * sGripSize;
			pnt.Y += 3 * sGripSize;

			Point pnt1 = pnt;
			pnt1.X -= sGripSize;
			pnt1.Y -= sGripSize;
			Point pnt2 = pnt;
			pnt2.X += sGripSize;
			pnt2.Y += sGripSize;
			
			Rect rect = new Rect(pnt1, pnt2);
			dc.DrawRectangle(this.FillBrush, pen, rect);
		}

		#endregion
	}

	/// <summary>
	/// Changes X-distance in column pattern.
	/// </summary>
	public class ColumnPatternOffsetX_GripPoint : GripPoint
	{
		public ColumnPatternOffsetX_GripPoint(DrawingControl drawing, BaseRectangleGeometry columnOwner)
			: base(drawing, columnOwner, BaseRectangleGeometry.GRIP_CENTER) { }

		#region Properties

		protected override eColorType FillColorType { get { return eColorType.eColumnPatternDistanceGripPointFillColor; } }

		#endregion

		#region Methods

		//=============================================================================
		private Column _GetColumn()
		{
			if (m_Geometry != null)
				return m_Geometry as Column;

			return null;
		}

		//=============================================================================
		private ColumnPattern _GetPattern()
		{
			Column _column = _GetColumn();
			if (_column != null)
				return _column.Pattern;

			return null;
		}

		//=============================================================================
		public override bool Move(Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			if (m_Geometry == null)
				return false;

			if (m_DC == null)
				return false;

			//
			Column _column = _GetColumn();
			if (_column == null)
				return false;

			//
			globalPoint = Utils.CheckBorders(globalPoint, 0.0, 0.0, DrawingLength, DrawingWidth, 0, 0);

			// calc new global offset x
			double newGlobalOffset_X = Math.Abs(Utils.GetWholeNumber(globalPoint.X - _column.Center_GlobalPoint.X));
			if (newGlobalOffset_X <= 0)
				return false;

			return ColumnPattern.ChangeOffsetX(_column, newGlobalOffset_X, DrawingLength, DrawingWidth, false);
		}

		//=============================================================================
		protected override void _Draw(DrawingContext dc)
		{
			if (dc == null)
				return;

			ColumnPattern pattern = _GetPattern();
			if (pattern == null)
				return;

			double Offset_X = _ConvertToScreenLength(pattern.GlobalOffset_X);

			Pen pen = new Pen(this.BorderBrush, 2.0);

			Point columnCenterPoint = GetGripScreenPoint();
			Point gripCenter = columnCenterPoint;
			gripCenter = columnCenterPoint;
			gripCenter.X += Offset_X;

			Point pnt1 = gripCenter;
			pnt1.X -= sGripSize;
			pnt1.Y -= sGripSize;
			Point pnt2 = gripCenter;
			pnt2.X += sGripSize;
			pnt2.Y += sGripSize;
			
			Rect rect = new Rect(pnt1, pnt2);
			dc.DrawRectangle(this.FillBrush, pen, rect);
		}

		#endregion
	}

	/// <summary>
	/// Changes Y-distance in column pattern.
	/// </summary>
	public class ColumnPatternOffsetY_GripPoint : GripPoint
	{
		public ColumnPatternOffsetY_GripPoint(DrawingControl drawing, BaseRectangleGeometry columnOwner)
			: base(drawing, columnOwner, BaseRectangleGeometry.GRIP_CENTER) { }

		#region Properties

		protected override eColorType FillColorType { get { return eColorType.eColumnPatternDistanceGripPointFillColor; } }

		#endregion

		#region Methods

		//=============================================================================
		private Column _GetColumn()
		{
			if (m_Geometry != null)
				return m_Geometry as Column;

			return null;
		}

		//=============================================================================
		private ColumnPattern _GetPattern()
		{
			Column _column = _GetColumn();
			if (_column != null)
				return _column.Pattern;

			return null;
		}

		//=============================================================================
		public override bool Move(Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			if (m_Geometry == null)
				return false;

			if (m_DC == null)
				return false;

			//
			Column _column = _GetColumn();
			if (_column == null)
				return false;

			//
			globalPoint = Utils.CheckBorders(globalPoint, 0.0, 0.0, DrawingLength, DrawingWidth, 0, 0);

			// calc new global offset Y
			double newGlobalOffset_Y = Math.Abs(Utils.GetWholeNumber(globalPoint.Y - _column.Center_GlobalPoint.Y));
			if (newGlobalOffset_Y <= 0)
				return false;

			return ColumnPattern.ChangeOffsetY(_column, newGlobalOffset_Y, DrawingLength, DrawingWidth, false);
		}

		//=============================================================================
		protected override void _Draw(DrawingContext dc)
		{
			if (dc == null)
				return;

			ColumnPattern pattern = _GetPattern();
			if (pattern == null)
				return;

			double Offset_Y = _ConvertToScreenLength(pattern.GlobalOffset_Y);

			Pen pen = new Pen(this.BorderBrush, 2.0);

			Point columnCenterPoint = GetGripScreenPoint();
			Point gripCenter = columnCenterPoint;
			gripCenter = columnCenterPoint;
			gripCenter.Y += Offset_Y;

			Point pnt1 = gripCenter;
			pnt1.X -= sGripSize;
			pnt1.Y -= sGripSize;
			Point pnt2 = gripCenter;
			pnt2.X += sGripSize;
			pnt2.Y += sGripSize;

			Rect rect = new Rect(pnt1, pnt2);
			dc.DrawRectangle(this.FillBrush, pen, rect);
		}

		#endregion
	}
}
