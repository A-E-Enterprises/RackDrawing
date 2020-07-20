using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public class Wall_State : GeometryState
	{
		public Wall_State(Wall wall)
			: base(wall)
		{
			this.WallPosition = wall.WallPosition;
		}
		public Wall_State(Wall_State state)
			: base(state)
		{
			this.WallPosition = state.WallPosition;
		}

		//
		public eWallPosition WallPosition { get; protected set; }

		//=============================================================================
		protected override GeometryState MakeDeepCopy()
		{
			return new Wall_State(this);
		}
	}

	//[Flags]
	public enum eWallPosition
	{
		eUndefined = 0,
		eLeft = 1,
		eTop,
		eRight,
		eBot
	}

	[Serializable]
	public class Wall : BaseRectangleGeometry, ISerializable
	{
		//
		// Wall can be placed only at the border of the graphics area.
		// Wall always stretchs and take full width of the graphics area side.
		// Display shutters over wall.
		public Wall(DrawingSheet ds)
			: base(ds)
		{
			m_FillColor = Colors.DarkSlateGray;

			//
			MaxLength_Y = double.PositiveInfinity;
			MaxLength_X = double.PositiveInfinity;

			//
			IsHorizontal = true;

			//
			Name = "Wall";
		}

		//=============================================================================
		public static double THICKNESS = 100.0;

		//=============================================================================
		private eWallPosition m_WallPosition = eWallPosition.eUndefined;
		public eWallPosition WallPosition
		{
			get { return m_WallPosition; }
			set
			{
				if (value != m_WallPosition)
				{
					m_WallPosition = value;

					m_TopLeft_GlobalPoint = new Point(0.0, 0.0);
					if (eWallPosition.eLeft == m_WallPosition)
					{
						m_TopLeft_GlobalPoint.X -= THICKNESS;
						m_Length_X = THICKNESS;
						m_Length_Y = DrawingGlobalSize.Height;
					}
					else if (eWallPosition.eTop == m_WallPosition)
					{
						m_TopLeft_GlobalPoint.Y -= THICKNESS;
						m_Length_X = DrawingGlobalSize.Width;
						m_Length_Y = THICKNESS;
					}
					else if (eWallPosition.eRight == m_WallPosition)
					{
						m_TopLeft_GlobalPoint.X = DrawingGlobalSize.Width;
						m_Length_X = THICKNESS;
						m_Length_Y = DrawingGlobalSize.Height;
					}
					else if (eWallPosition.eBot == m_WallPosition)
					{
						m_TopLeft_GlobalPoint.Y = DrawingGlobalSize.Height;
						m_Length_X = DrawingGlobalSize.Width;
						m_Length_Y = THICKNESS;
					}

					_MarkStateChanged();
					this.UpdateProperties();
				}
			}
		}

		#region Functions

		//=============================================================================
		protected override GeometryState _GetOriginalState()
		{
			return new Wall_State(this);
		}
		//=============================================================================
		protected override void _SetState(GeometryState state)
		{
			base._SetState(state);

			Wall_State wallState = state as Wall_State;
			if (wallState == null)
				return;

			this.m_WallPosition = wallState.WallPosition;
		}
		//=============================================================================
		protected override BaseRectangleGeometry CreateInstance() { return new Wall(null); }

		//=============================================================================
		protected override void _InitProperties()
		{
			m_Properties.Add(new GeometryType_Property(this));

			string strLengthXPropName = "Length";
			string strLengthYPropName = "Thickness";
			if(eWallPosition.eLeft == m_WallPosition || eWallPosition.eRight == m_WallPosition)
			{
				string strTemp = strLengthXPropName;
				strLengthXPropName = strLengthYPropName;
				strLengthYPropName = strTemp;
			}
			//
			m_Properties.Add(new GeometryProperty(this, PROP_DIMENSION_X, strLengthXPropName, true, true, "Geometry"));
			m_Properties.Add(new GeometryProperty(this, PROP_DIMENSION_Y, strLengthYPropName, true, true, "Geometry"));
			m_Properties.Add(new GeometryProperty(this, PROP_DIMENSION_Z, "Height", true, true, "Geometry"));
		}

		//=============================================================================
		public override List<Point> GetGripPoints()
		{
			// wall doesnt have any grip points
			return new List<Point>();
		}

		//=============================================================================
		public override bool SetGripPoint(int gripIndex, Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			// wall doesnt have any grip points
			return false;
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

			if (eWallPosition.eUndefined == m_WallPosition)
				return;

			IGeomDisplaySettings displaySettings = geomDisplaySettings;
			// if NULL then get settings from the sheet
			if (displaySettings == null)
				displaySettings = m_Sheet;
			//
			if (displaySettings == null)
				displaySettings = new DefaultGeomDisplaySettings();
			if (displaySettings == null)
				return;

			Pen _pen = this.BorderPen;
			//
			// If fill with transparent color then circle fill area will act in HitTest.
			// Fill with null brush will disable circle HitTest on click in fill area.
			Color fillColor = displaySettings.GetFillColor(this);
			Brush fillBrush = new SolidColorBrush(fillColor);
			fillBrush.Opacity = displaySettings.FillBrushOpacity;

			//
			Point sreenPnt01 = GetLocalPoint(cs, TopLeft_GlobalPoint);
			Point sreenPnt02 = GetLocalPoint(cs, BottomRight_GlobalPoint);

			dc.DrawRectangle(fillBrush, _pen, new Rect(sreenPnt01, sreenPnt02));
		}

		//=============================================================================
		public override bool SetPropertyValue(string strPropSysName, object propValue, bool bWasChangedViaProperties, bool bChangeTheSameRectangles, bool bNotifySheet, out string strError, bool bCheckLayout = true)
		{
			strError = string.Empty;

			if (this.Sheet == null)
				return false;

			return false;
		}

		#endregion

		#region Serialization

		//=============================================================================
		protected static string _sWall_strMajor = "Wall_MAJOR";
		protected static int _sWall_MAJOR = 1;
		protected static string _sWall_strMinor = "Wall_MINOR";
		protected static int _sWall_MINOR = 0;
		//=============================================================================
		public Wall(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sWall_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sWall_strMinor, typeof(int));
			if (iMajor > _sWall_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sWall_MAJOR && iMinor > _sWall_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sWall_MAJOR)
			{
				try
				{
					if (iMajor >= 1 && iMinor >= 0)
						m_WallPosition = (eWallPosition)info.GetValue("WallPosition", typeof(eWallPosition));
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
			info.AddValue(_sWall_strMajor, _sWall_MAJOR);
			info.AddValue(_sWall_strMinor, _sWall_MINOR);

			// 1.0
			info.AddValue("WallPosition", m_WallPosition);
		}

		#endregion
	}
}
