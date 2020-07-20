using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public class Shutter_State : GeometryState
	{
		public Shutter_State(Shutter shutter)
			: base(shutter)
		{
			this.SwingDoor = shutter.SwingDoor;
		}
		public Shutter_State(Shutter_State state)
			: base(state)
		{
			this.SwingDoor = state.SwingDoor;
		}

		#region Properties

		public bool SwingDoor { get; set; }

		#endregion

		//=============================================================================
		protected override GeometryState MakeDeepCopy()
		{
			return new Shutter_State(this);
		}
	}

	[Serializable]
	public class Shutter : BaseRectangleGeometry, ISerializable
	{
		public static string PROP_SWING_DOOR = "Swing Door";

		/// <summary>
		/// Shutter depth value.
		/// It can be changed if it is Swing Door shutter.
		/// </summary>
		public static double SHUTTER_DEPTH = 100;

		//
		// Shutter is placed on the borders only.
		// If it is horizontal border then shutter has fixed height SHUTTER_DEPTH,
		// if it is vertical - fixed width SHUTTER_DEPTH.
		//
		// No one block cant overlap shutter.
		public Shutter(DrawingSheet ds)
			: base(ds)
		{
			m_FillColor = Colors.Pink;

			//
			MinLength_X = SHUTTER_DEPTH;
			MinLength_Y = SHUTTER_DEPTH;

			//
			MaxLength_Y = double.PositiveInfinity;
			MaxLength_X = double.PositiveInfinity;
			MaxLength_Z = 100000;

			//
			IsHorizontal = true;
			Length_Y = SHUTTER_DEPTH;

			//
			StepLength_X = 50;
			StepLength_Y = 50;

			//
			Name = "Shutter";
		}

		#region Properties

		//=============================================================================
		/// <summary>
		/// On which side of drawing area this shutter is placed.
		/// </summary>
		public eWallPosition WallPosition
		{
			get
			{
				if(this.Sheet != null)
				{
					if(this.IsHorizontal)
					{
						if (Utils.FLT(this.TopLeft_GlobalPoint.Y, 0.0))
							return eWallPosition.eTop;
						else if (Utils.FGE(this.TopLeft_GlobalPoint.Y, this.Sheet.Width))
							return eWallPosition.eBot;
					}
					else
					{
						if (Utils.FLT(this.TopLeft_GlobalPoint.X, 0.0))
							return eWallPosition.eLeft;
						else if (Utils.FGE(this.TopLeft_GlobalPoint.X, this.Sheet.Length))
							return eWallPosition.eRight;
					}
				}

				return eWallPosition.eUndefined;
			}
		}

		//=============================================================================
		protected override bool _Is_HeightProperty_ReadOnly { get { return false; } }

		//=============================================================================
		/// <summary>
		/// The maximum height value
		/// </summary>
		public override int MaxLength_Z
		{
			get
			{
				//
				if (this.Sheet != null && this.Sheet.Document != null)
				{
					double maxHeightValue;
					if (this.Sheet.Document.CalculateMaxHeightForGeometry(this, out maxHeightValue))
					{
						maxHeightValue -= Rack.ROOF_HEIGHT_GAP;
						if (Utils.FGT(maxHeightValue, 0.0))
							return Utils.GetWholeNumber(maxHeightValue);
					}
				}

				return 12000;
			}
			set
			{
				base.MaxLength_Z = value;
			}
		}

		//=============================================================================
		/// <summary>
		/// The minimum height value
		/// </summary>
		public override int MinLength_Z
		{
			get
			{
				// 
				if (this.Sheet != null && this.Sheet.Document != null)
				{
					double minLenghtZ = this.Sheet.Document.OverallHeightLowered;
					if(Utils.FGT(minLenghtZ, 0.0))
						return Utils.GetWholeNumber(minLenghtZ);
				}

				return base.MinLength_Z;
			}
			set
			{
				base.MinLength_Z = value;
			}
		}

		/// <summary>
		///  If true then it is Swing Door shutter.
		///  It changes shutter display view and shutter size.
		///  Only wall and Aisle Space can overlap Swing Door Shutter.
		/// </summary>
		private bool m_SwingDoor = false;
		public bool SwingDoor
		{
			get { return m_SwingDoor; }
			set
			{
				string strError;
				SetPropertyValue(PROP_SWING_DOOR, value, false, false, true, out strError);
			}
		}

		/// <summary>
		/// Length of one swing door.
		/// </summary>
		public double SwingDoorLength
		{
			get
			{
				if (SwingDoor)
					return this.Length / 2;

				return 0.0;
			}
		}

		public override double MarginX
		{
			get
			{
				if(m_SwingDoor)
				{
					if (!this.IsHorizontal)
						return SwingDoorLength;
				}

				return base.MarginX;
			}

			set
			{
				base.MarginX = value;
			}
		}

		public override double MarginY
		{
			get
			{
				if(m_SwingDoor)
				{
					if (this.IsHorizontal)
						return SwingDoorLength;
				}

				return base.MarginY;
			}

			set
			{
				base.MarginY = value;
			}
		}

		#endregion

		#region Functions

		//=============================================================================
		protected override GeometryState _GetOriginalState()
		{
			return new Shutter_State(this);
		}
		//=============================================================================
		protected override void _SetState(GeometryState state)
		{
			base._SetState(state);

			Shutter_State shutterState = state as Shutter_State;
			if (shutterState == null)
				return;

			this.m_SwingDoor = shutterState.SwingDoor;
		}
		//=============================================================================
		protected override BaseRectangleGeometry CreateInstance() { return new Shutter(null); }


		//=============================================================================
		protected override void _InitProperties()
		{
			m_Properties.Add(new GeometryType_Property(this));

			//
			m_Properties.Add(new GeometryProperty(this, PROP_DIMENSION_X, "Length", true, "Geometry"));
			m_Properties.Add(new GeometryProperty(this, PROP_DIMENSION_Y, "Depth", true, "Geometry"));
			m_Properties.Add(new GeometryProperty(this, PROP_DIMENSION_Z, "Height", _Is_HeightProperty_ReadOnly, true, "Geometry"));
			m_Properties.Add(new GeometryProperty(this, PROP_SWING_DOOR, "Swing Door", false, "Geometry"));

			// make Depth read-only
			Property_ViewModel depthProp = m_Properties.FirstOrDefault(p => p != null && p.SystemName == PROP_DIMENSION_Y);
			if (depthProp != null)
				depthProp.IsReadOnly = true;

			_UpdateProperties();
		}

		//=============================================================================
		public override bool SetGripPoint(int gripIndex, Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			if (m_Sheet == null)
				return false;

			if (Wrapper == null || Wrapper.Owner == null)
				return false;

			GeometryState oldState = this._GetClonedState();

			//
			if (BaseRectangleGeometry.GRIP_CENTER == gripIndex)
			{
				// make a decision - to which border should it be snapped?
				double deltaTop = Math.Abs(globalPoint.Y);
				double deltaBot = Math.Abs(globalPoint.Y - DrawingWidth);
				double deltaLeft = Math.Abs(globalPoint.X);
				double deltaRight = Math.Abs(globalPoint.X - DrawingLength);

				// horiz or vertical
				double minVert = Math.Min(deltaLeft, deltaRight);
				double minHoriz = Math.Min(deltaBot, deltaTop);

				bool oldIsHorizontal = IsHorizontal;
				if (minHoriz <= minVert)
					IsHorizontal = true;
				else
					IsHorizontal = false;
				//
				if(oldIsHorizontal != IsHorizontal)
				{
					double rVal = Length_X;
					Length_X = Length_Y;
					Length_Y = rVal;
				}

				// try to change position
				// mouse point is the center of this rectangle
				Point newTopLeftPoint = globalPoint;
				//
				if (IsHorizontal)
				{
					newTopLeftPoint.X -= Length_X / 2;
					//
					if (newTopLeftPoint.X < 0)
						newTopLeftPoint.X = 0;
					if (newTopLeftPoint.X + Length_X > DrawingLength)
						newTopLeftPoint.X = DrawingLength - Length_X;

					//
					if (deltaTop < deltaBot)
					{
						newTopLeftPoint.Y = 0;
						newTopLeftPoint.Y -= SHUTTER_DEPTH;
					}
					else
						newTopLeftPoint.Y = DrawingWidth;
				}
				else
				{
					newTopLeftPoint.Y -= Length_Y / 2;
					//
					if (newTopLeftPoint.Y < 0)
						newTopLeftPoint.Y = 0;
					if (newTopLeftPoint.Y + Length_Y > DrawingWidth)
						newTopLeftPoint.Y = DrawingWidth - Length_Y;

					// snap it to the left or right border
					if (deltaLeft < deltaRight)
					{
						newTopLeftPoint.X = 0;
						newTopLeftPoint.X -= SHUTTER_DEPTH;
					}
					else
						newTopLeftPoint.X = DrawingLength;
				}

				m_TopLeft_GlobalPoint = newTopLeftPoint;
			}
			else if(BaseRectangleGeometry.GRIP_TOP_LEFT == gripIndex)
			{
				if(IsHorizontal)
				{
					m_TopLeft_GlobalPoint = TopRight_GlobalPoint;
					//
					Length_X = m_TopLeft_GlobalPoint.X - globalPoint.X;
					Length_X = Utils.GetWholeNumberByStep(Length_X, StepLength_X);
					Length_X = Utils.CheckWholeNumber(Length_X, MinLength_X, MaxLength_X);
					//
					m_TopLeft_GlobalPoint.X -= Length_X;
				}
				else
				{
					m_TopLeft_GlobalPoint = BottomLeft_GlobalPoint;
					//
					Length_Y = m_TopLeft_GlobalPoint.Y - globalPoint.Y;
					Length_Y = Utils.GetWholeNumberByStep(Length_Y, StepLength_Y);
					Length_Y = Utils.CheckWholeNumber(Length_Y, MinLength_Y, MaxLength_Y);

					m_TopLeft_GlobalPoint.Y -= Length_Y;
				}
			}
			else if(BaseRectangleGeometry.GRIP_BOTTOM_RIGHT == gripIndex)
			{
				if (IsHorizontal)
				{
					//
					Length_X = globalPoint.X - m_TopLeft_GlobalPoint.X;
					Length_X = Utils.GetWholeNumberByStep(Length_X, StepLength_X);
					Length_X = Utils.CheckWholeNumber(Length_X, MinLength_X, MaxLength_X);
				}
				else
				{
					//
					Length_Y = globalPoint.Y - m_TopLeft_GlobalPoint.Y;
					Length_Y = Utils.GetWholeNumberByStep(Length_Y, StepLength_Y);
					Length_Y = Utils.CheckWholeNumber(Length_Y, MinLength_Y, MaxLength_Y);
				}
			}

			List<BaseRectangleGeometry> rectanglesToIgnore = new List<BaseRectangleGeometry>();
			rectanglesToIgnore.Add(this);
			// check
			List<BaseRectangleGeometry> overlappedRectangles;
			if (!m_Sheet.IsLayoutCorrect(this, rectanglesToIgnore, out overlappedRectangles))
			{
				// try to fix it
				Point _newTopLeftPoint;
				double _newGlobalLength;
				double _newGlobalWidth;
				//
				// infinity loop protection
				int iMaxLoopCount = 100;
				int iLoopCount = 0;
				//
				bool bIncorrectCalc = false;
				//
				while (this._CalculateNotOverlapPosition(overlappedRectangles, gripIndex, DrawingLength, DrawingWidth, true, out _newTopLeftPoint, out _newGlobalLength, out _newGlobalWidth))
				{
					// check
					if(IsHorizontal)
					{
						//
						if (_newTopLeftPoint.Y > -Length_Y / 2 && _newTopLeftPoint.Y < DrawingWidth + Length_Y / 2)
						{
							bIncorrectCalc = true;
							break;
						}
						//
						if (_newTopLeftPoint.X < 0)
							_newTopLeftPoint.X = 0;
						if (_newTopLeftPoint.X + Length_X > DrawingLength)
							_newTopLeftPoint.X = DrawingLength - Length_X;
					}
					else
					{
						if (_newTopLeftPoint.X > -Length_X / 2 && _newTopLeftPoint.X < DrawingLength + Length_X / 2)
						{
							bIncorrectCalc = true;
							break;
						}
						//
						if (_newTopLeftPoint.Y < 0)
							_newTopLeftPoint.Y = 0;
						if (_newTopLeftPoint.Y + Length_Y > DrawingWidth)
							_newTopLeftPoint.Y = DrawingWidth - Length_Y;
					}

					m_TopLeft_GlobalPoint = _newTopLeftPoint;
					m_Length_X = _newGlobalLength;
					m_Length_Y = _newGlobalWidth;

					//
					if (IsCorrect(eAppliedChanges.eMoveCenterGripPoint, out overlappedRectangles))
						break;
			
					++iLoopCount;
					if (iLoopCount >= iMaxLoopCount)
						break;
				}

				if (bIncorrectCalc || !IsCorrect(eAppliedChanges.eMoveCenterGripPoint, out overlappedRectangles))
				{
					this._SetState(oldState);
					_UpdateProperties();

					this.UpdateProperties();

					return false;
				}
			}

			_MarkStateChanged();
			_UpdateProperties();

			this.UpdateProperties();

			return true;
		}

		//=============================================================================
		public override void Draw(DrawingContext dc, ICoordinateSystem cs, IGeomDisplaySettings geomDisplaySettings = null)
		{
			if (dc == null)
				return;

			if (cs == null)
				return;

			if (m_Sheet == null)
				return;

			IGeomDisplaySettings displaySettings = geomDisplaySettings;
			if (displaySettings == null)
				displaySettings = new DefaultGeomDisplaySettings();
			if (displaySettings == null)
				return;

			Pen pen = this.BorderPen;
			//
			// If fill with transparent color then circle fill area will act in HitTest.
			// Fill with null brush will disable circle HitTest on click in fill area.
			Color fillColor = displaySettings.GetFillColor(this);
			Brush fillBrush = new SolidColorBrush(fillColor);
			fillBrush.Opacity = displaySettings.FillBrushOpacity;

			// Draw shutter rectangle without SwingDoor
			Point TopLeft_ScreenPoint = GetLocalPoint(cs, m_TopLeft_GlobalPoint);
			Point BottomRight_ScreenPoint = GetLocalPoint(cs, BottomRight_GlobalPoint);
			dc.DrawRectangle(fillBrush, pen, new Rect(TopLeft_ScreenPoint, BottomRight_ScreenPoint));

			// draw Swing Door
			if(m_SwingDoor)
			{
				double doorLength = SwingDoorLength;
				double doorLengthInPixels = GetWidthInPixels(cs, doorLength);
				PathGeometry swingDoorGeom = new PathGeometry();

				if(this.IsHorizontal)
				{
					if(Utils.FLE(m_TopLeft_GlobalPoint.Y, 0.0))
					{
						// shutter is placed at top border
						PathFigure leftDoorFigure = new PathFigure();
						leftDoorFigure.StartPoint = GetLocalPoint(cs, BottomLeft_GlobalPoint);
						leftDoorFigure.Segments.Add(new LineSegment(GetLocalPoint(cs, BottomLeft_GlobalPoint + doorLength * new Vector(0.0, 1.0)), true));
						leftDoorFigure.Segments.Add(new ArcSegment(
														GetLocalPoint(cs, BottomLeft_GlobalPoint + doorLength * new Vector(1.0, 0.0)),
														new Size(doorLengthInPixels, doorLengthInPixels),
														0.0,
														false,
														SweepDirection.Counterclockwise,
														true)
														);
						swingDoorGeom.Figures.Add(leftDoorFigure);

						PathFigure rightDoorFigure = new PathFigure();
						rightDoorFigure.StartPoint = GetLocalPoint(cs, BottomRight_GlobalPoint);
						rightDoorFigure.Segments.Add(new LineSegment(GetLocalPoint(cs, BottomRight_GlobalPoint + doorLength * new Vector(0.0, 1.0)), true));
						rightDoorFigure.Segments.Add(new ArcSegment(
														GetLocalPoint(cs, BottomRight_GlobalPoint + doorLength * new Vector(-1.0, 0.0)),
														new Size(doorLengthInPixels, doorLengthInPixels),
														0.0,
														false,
														SweepDirection.Clockwise,
														true)
														);
						swingDoorGeom.Figures.Add(rightDoorFigure);
					}
					else
					{
						// shutter is placed at bot border
						PathFigure leftDoorFigure = new PathFigure();
						leftDoorFigure.StartPoint = GetLocalPoint(cs, m_TopLeft_GlobalPoint);
						leftDoorFigure.Segments.Add(new LineSegment(GetLocalPoint(cs, m_TopLeft_GlobalPoint + doorLength * new Vector(0.0, -1.0)), true));
						leftDoorFigure.Segments.Add(new ArcSegment(
														GetLocalPoint(cs, m_TopLeft_GlobalPoint + doorLength * new Vector(1.0, 0.0)),
														new Size(doorLengthInPixels, doorLengthInPixels),
														0.0,
														false,
														SweepDirection.Clockwise,
														true)
														);
						swingDoorGeom.Figures.Add(leftDoorFigure);

						PathFigure rightDoorFigure = new PathFigure();
						rightDoorFigure.StartPoint = GetLocalPoint(cs, TopRight_GlobalPoint);
						rightDoorFigure.Segments.Add(new LineSegment(GetLocalPoint(cs, TopRight_GlobalPoint + doorLength * new Vector(0.0, -1.0)), true));
						rightDoorFigure.Segments.Add(new ArcSegment(
														GetLocalPoint(cs, TopRight_GlobalPoint + doorLength * new Vector(-1.0, 0.0)),
														new Size(doorLengthInPixels, doorLengthInPixels),
														0.0,
														false,
														SweepDirection.Counterclockwise,
														true)
														);
						swingDoorGeom.Figures.Add(rightDoorFigure);
					}
				}
				else
				{
					if (Utils.FLE(m_TopLeft_GlobalPoint.X, 0.0))
					{
						// shutter is placed at left border
						PathFigure topDoorFigure = new PathFigure();
						topDoorFigure.StartPoint = GetLocalPoint(cs, TopRight_GlobalPoint);
						topDoorFigure.Segments.Add(new LineSegment(GetLocalPoint(cs, TopRight_GlobalPoint + doorLength * new Vector(1.0, 0.0)), true));
						topDoorFigure.Segments.Add(new ArcSegment(
														GetLocalPoint(cs, TopRight_GlobalPoint + doorLength * new Vector(0.0, 1.0)),
														new Size(doorLengthInPixels, doorLengthInPixels),
														0.0,
														false,
														SweepDirection.Clockwise,
														true)
														);
						swingDoorGeom.Figures.Add(topDoorFigure);

						PathFigure botDoorFigure = new PathFigure();
						botDoorFigure.StartPoint = GetLocalPoint(cs, BottomRight_GlobalPoint);
						botDoorFigure.Segments.Add(new LineSegment(GetLocalPoint(cs, BottomRight_GlobalPoint + doorLength * new Vector(1.0, 0.0)), true));
						botDoorFigure.Segments.Add(new ArcSegment(
														GetLocalPoint(cs, BottomRight_GlobalPoint + doorLength * new Vector(0.0, -1.0)),
														new Size(doorLengthInPixels, doorLengthInPixels),
														0.0,
														false,
														SweepDirection.Counterclockwise,
														true)
														);
						swingDoorGeom.Figures.Add(botDoorFigure);
					}
					else
					{
						// shutter is placed at right border
						PathFigure topDoorFigure = new PathFigure();
						topDoorFigure.StartPoint = GetLocalPoint(cs, m_TopLeft_GlobalPoint);
						topDoorFigure.Segments.Add(new LineSegment(GetLocalPoint(cs, m_TopLeft_GlobalPoint + doorLength * new Vector(-1.0, 0.0)), true));
						topDoorFigure.Segments.Add(new ArcSegment(
														GetLocalPoint(cs, m_TopLeft_GlobalPoint + doorLength * new Vector(0.0, 1.0)),
														new Size(doorLengthInPixels, doorLengthInPixels),
														0.0,
														false,
														SweepDirection.Counterclockwise,
														true)
														);
						swingDoorGeom.Figures.Add(topDoorFigure);

						PathFigure botDoorFigure = new PathFigure();
						botDoorFigure.StartPoint = GetLocalPoint(cs, BottomLeft_GlobalPoint);
						botDoorFigure.Segments.Add(new LineSegment(GetLocalPoint(cs, BottomLeft_GlobalPoint + doorLength * new Vector(-1.0, 0.0)), true));
						botDoorFigure.Segments.Add(new ArcSegment(
														GetLocalPoint(cs, BottomLeft_GlobalPoint + doorLength * new Vector(0.0, -1.0)),
														new Size(doorLengthInPixels, doorLengthInPixels),
														0.0,
														false,
														SweepDirection.Clockwise,
														true)
														);
						swingDoorGeom.Figures.Add(botDoorFigure);
					}
				}

				// Draw transparent rectangle over all swing door.
				// Otherwise, user cant select shutter by click swing door.
				dc.DrawGeometry(Brushes.Transparent, null, swingDoorGeom);
				// Draw swing door with pen
				dc.DrawGeometry(null, pen, swingDoorGeom);
			}
		}

		//=============================================================================
		public override object GetPropertyValue(string strPropSysName)
		{
			// length
			if(PROP_DIMENSION_X == strPropSysName)
			{
				return Length;
			}
			// depth
			else if(PROP_DIMENSION_Y == strPropSysName)
			{
				return Depth;
			}
			else if(PROP_SWING_DOOR == strPropSysName)
			{
				return m_SwingDoor;
			}

			return base.GetPropertyValue(strPropSysName);
		}

		//=============================================================================
		public override bool SetPropertyValue(string strPropSysName, object propValue, bool bWasChangedViaProperties, bool bChangeTheSameRectangles, bool bNotifySheet, out string strError, bool bCheckLayout = true)
		{
			strError = string.Empty;

			if (PROP_DIMENSION_Z == strPropSysName)
				return base.SetPropertyValue(strPropSysName, propValue, bWasChangedViaProperties, bChangeTheSameRectangles, bNotifySheet, out strError, bCheckLayout);

			if (!m_bIsHorizontal)
			{
				if (PROP_DIMENSION_X == strPropSysName)
					strPropSysName = PROP_DIMENSION_Y;
			}

			GeometryState oldState = this._GetClonedState();

			bool bRes = _SetPropertyValue(strPropSysName, propValue, bCheckLayout && this.IsInit, out strError);

			if(bRes)
				_MarkStateChanged();

			if (!bRes)
				this._SetState(oldState);

			if (m_Sheet != null && bNotifySheet)
				m_Sheet.OnPropertyChanged(this, strPropSysName, bRes, strError);

			this.UpdateProperties();

			return bRes;
		}

		//=============================================================================
		private void _UpdateProperties()
		{
			if (m_Properties == null)
				return;

			if (m_bIsHorizontal)
			{
				// remove Y-pos
				Property_ViewModel _TopLeftPoint_Prop = m_Properties.FirstOrDefault(p => p != null && p.SystemName == PROP_TOP_LEFT_POINT_Y);
				if (_TopLeftPoint_Prop != null)
					m_Properties.Remove(_TopLeftPoint_Prop);
				// add X-pos
				Property_ViewModel topLeftPointX_prop = m_Properties.FirstOrDefault(p => p != null && p.SystemName == PROP_TOP_LEFT_POINT_X);
				if (topLeftPointX_prop == null)
					m_Properties.Add(new GeometryProperty(this, PROP_TOP_LEFT_POINT_X, true, "Geometry"));
			}
			else
			{
				// remove X-pos
				Property_ViewModel _TopLeftPoint_Prop = m_Properties.FirstOrDefault(p => p != null && p.SystemName == PROP_TOP_LEFT_POINT_X);
				if (_TopLeftPoint_Prop != null)
					m_Properties.Remove(_TopLeftPoint_Prop);
				// add Y-pos
				Property_ViewModel topLeftPointY_prop = m_Properties.FirstOrDefault(p => p != null && p.SystemName == PROP_TOP_LEFT_POINT_Y);
				if (topLeftPointY_prop == null)
					m_Properties.Add(new GeometryProperty(this, PROP_TOP_LEFT_POINT_Y, true, "Geometry"));
			}
		}

		//=============================================================================
		private bool _SetPropertyValue(string strPropSysName, object propValue, bool bCheckLayout, out string strError)
		{
			strError = string.Empty;

			if (this.Sheet == null)
				return false;

			if (PROP_DIMENSION_X == strPropSysName)
			{
				try
				{
					Length_X = Convert.ToDouble(propValue);
					Length_X = Utils.GetWholeNumberByStep(Length_X, StepLength_X);
					Length_X = Utils.CheckWholeNumber(Length_X, MinLength_X, MaxLength_X);

					return this.SetGripPoint(GRIP_CENTER, Center_GlobalPoint, this.Sheet.Length, this.Sheet.Width);
				}
				catch { }
			}
			else if (PROP_DIMENSION_Y == strPropSysName)
			{
				try
				{
					Length_Y = Convert.ToDouble(propValue);
					Length_Y = Utils.GetWholeNumberByStep(Length_Y, StepLength_Y);
					Length_Y = Utils.CheckWholeNumber(Length_Y, MinLength_Y, MaxLength_Y);

					return this.SetGripPoint(GRIP_CENTER, Center_GlobalPoint, this.Sheet.Length, this.Sheet.Width);
				}
				catch { }
			}
			else if (PROP_TOP_LEFT_POINT_X == strPropSysName)
			{
				try
				{
					Point newTopLeftPoint = TopLeft_GlobalPoint;
					newTopLeftPoint.X = Convert.ToInt32(propValue);
					TopLeft_GlobalPoint = newTopLeftPoint;

					return this.SetGripPoint(GRIP_CENTER, Center_GlobalPoint, this.Sheet.Length, this.Sheet.Width);
				}
				catch { }
			}
			else if (PROP_TOP_LEFT_POINT_Y == strPropSysName)
			{
				try
				{
					Point newTopLeftPoint = TopLeft_GlobalPoint;
					newTopLeftPoint.Y = Convert.ToInt32(propValue);
					TopLeft_GlobalPoint = newTopLeftPoint;

					return this.SetGripPoint(GRIP_CENTER, Center_GlobalPoint, this.Sheet.Length, this.Sheet.Width);
				}
				catch { }
			}
			else if(PROP_SWING_DOOR == strPropSysName)
			{
				try
				{
					bool newValue = Convert.ToBoolean(propValue);
					if (newValue == m_SwingDoor)
						return true;
					m_SwingDoor = newValue;

					// Swing Door changes margin, so need to check layout.
					// Probably this shutter overlaps other geometry.
					if (!bCheckLayout)
						return true;

					List<BaseRectangleGeometry> overlappedGeometryList = new List<BaseRectangleGeometry>();
					bool bRes = this.IsCorrect(out overlappedGeometryList);
					if(!bRes || (overlappedGeometryList != null && overlappedGeometryList.Count > 0))
					{
						strError = "Swing Door cant be applied to the shutter, because it overlaps other geometry.";
						return false;
					}

					return true;
				}
				catch { }
			}

			return false;
		}

		#endregion

		#region Serialization

		//=============================================================================
		// 1.1 Add SwingDoor
		protected static string _sShutter_strMajor = "Shutter_MAJOR";
		protected static int _sShutter_MAJOR = 1;
		protected static string _sShutter_strMinor = "Shutter_MINOR";
		protected static int _sShutter_MINOR = 1;
		//=============================================================================
		public Shutter(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sShutter_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sShutter_strMinor, typeof(int));
			if (iMajor > _sShutter_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sShutter_MAJOR && iMinor > _sShutter_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if(iMajor <= _sShutter_MAJOR)
			{
				try
				{
					// 1.1
					if (iMajor >= 1 && iMinor >= 1)
						m_SwingDoor = (bool)info.GetValue("SwingDoor", typeof(bool));
					else
						m_SwingDoor = false;
				}
				catch
				{
					++DrawingDocument._sStreamReadException;
				}
			}
			else
				++DrawingDocument._sBiggerMajorNumber;
		}
		//=============================================================================
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			//
			info.AddValue(_sShutter_strMajor, _sShutter_MAJOR);
			info.AddValue(_sShutter_strMinor, _sShutter_MINOR);

			// 1.1
			info.AddValue("SwingDoor", m_SwingDoor);
		}

		#endregion
	}
}
