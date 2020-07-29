using AppColorTheme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public class TieBeam_State : GeometryState
	{
		public TieBeam_State(TieBeam tieBeam)
			: base(tieBeam)
		{
			if(tieBeam != null)
				this.AttachedRacksHeightError = tieBeam.AttachedRacksHeightError;
				this.RackColumnLengthOffset = tieBeam.RackColumnLengthOffset;
		}
		public TieBeam_State(TieBeam_State state)
			: base(state)
		{
			if(state != null)
			{
				this.AttachedRacksHeightError = state.AttachedRacksHeightError;
				this.RackColumnLengthOffset = state.RackColumnLengthOffset;
			}
		}

		public bool AttachedRacksHeightError { get; private set; }
		public double RackColumnLengthOffset { get; private set; }

		//=============================================================================
		protected override GeometryState MakeDeepCopy()
		{
			return new TieBeam_State(this);
		}
	}

	[Serializable]
	public class TieBeam : BaseRectangleGeometry, ISerializable
	{
		//
		// Shutter is placed on the borders only.
		// If it is horizontal border then shutter has fixed height 100,
		// if it is vertical - fixed width 100.
		//
		// No one block cant overlap shutter.
		public TieBeam(DrawingSheet ds)
			: base(ds)
		{
			m_FillColor = Colors.Gray;

			MinLength_X = 0.0;
			MinLength_Y = 0.0;
			//
			MaxLength_Y = double.PositiveInfinity;
			MaxLength_X = double.PositiveInfinity;

			Name = "TieBeam";
		}

		#region Properties

		//
		public static double TIE_BEAM_DEPTH = 0;

		//=============================================================================
		// GUID of this tie beam.
		private Guid m_GUID = Guid.NewGuid();
		public Guid GUID { get { return m_GUID; } }

		//=============================================================================
		/// <summary>
		/// If true then one or both attached racks cant have this tie beam because
		/// rack height with tie beam is more than ClearAvailableHeight.
		/// </summary>
		private bool m_AttachedRacksHeightError = false;
		public bool AttachedRacksHeightError
		{
			get { return m_AttachedRacksHeightError; }
			set { m_AttachedRacksHeightError = value; }
		}

		private double m_RackColumnLengthOffset = 0.0;
		public double RackColumnLengthOffset {
			get { return m_RackColumnLengthOffset; } 
			set { m_RackColumnLengthOffset = value; } 
		}

        #endregion

        #region Functions

        //=============================================================================
        protected override GeometryState _GetOriginalState()
		{
			return new TieBeam_State(this);
		}
		//=============================================================================
		protected override void _SetState(GeometryState state)
		{
			base._SetState(state);

			TieBeam_State tieBeamState = state as TieBeam_State;
			if (tieBeamState == null)
				return;

			this.m_AttachedRacksHeightError = tieBeamState.AttachedRacksHeightError;
		}
		//=============================================================================
		protected override BaseRectangleGeometry CreateInstance() { return new TieBeam(null); }


		//=============================================================================
		protected override void _InitProperties()
		{
			m_Properties.Add(new GeometryType_Property(this));
		}

		//=============================================================================
		public override void OnMouseMove(Point mousePoint, double DrawingLength, double DrawingWidth)
		{
			// dont do anything
		}

		//=============================================================================
		public override bool SetGripPoint(int gripIndex, Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			// dont do anything
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
				displaySettings = DefaultGeomDisplaySettings.GetInstance();
			if (displaySettings == null)
				return;

			Brush penBrush = new SolidColorBrush(FillColor);
			if (m_AttachedRacksHeightError)
			{
				Color tieBeamErrorColor = Colors.Red;
				Color colorValue;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eFill_TieBeamWithError, out colorValue))
					tieBeamErrorColor = colorValue;
				penBrush = new SolidColorBrush(tieBeamErrorColor);
			}
			Pen pen = new Pen(penBrush, 2.0);
			pen.DashStyle = new DashStyle(new List<double> { 2 }, 1);
			//
			// If fill with transparent color then circle fill area will act in HitTest.
			// Fill with null brush will disable circle HitTest on click in fill area.
			//Color fillColor = displaySettings.GetFillColor(this);
			//Brush fillBrush = new SolidColorBrush(fillColor);
			//fillBrush.Opacity = displaySettings.FillBrushOpacity;

			//
			Point TopLeft_ScreenPoint = GetLocalPoint(cs, m_TopLeft_GlobalPoint);
			Point BottomRight_ScreenPoint = GetLocalPoint(cs, BottomRight_GlobalPoint);

			dc.DrawLine(pen, TopLeft_ScreenPoint, BottomRight_ScreenPoint);

			TopLeft_ScreenPoint = TopLeft_GlobalPoint;
			BottomRight_ScreenPoint = BottomRight_GlobalPoint;

			if (IsHorizontal)
            {
				TopLeft_ScreenPoint.Y += RackColumnLengthOffset;
				BottomRight_ScreenPoint.Y += RackColumnLengthOffset;
			}
            else
            {
				TopLeft_ScreenPoint.X += RackColumnLengthOffset;
				BottomRight_ScreenPoint.X += RackColumnLengthOffset;
			}

			dc.DrawLine(pen, GetLocalPoint(cs, TopLeft_ScreenPoint), GetLocalPoint(cs, BottomRight_ScreenPoint));
		}

		//=============================================================================
		public override object GetPropertyValue(string strPropSysName)
		{
			return base.GetPropertyValue(strPropSysName);
		}

		#endregion

		#region Serialization

		//=============================================================================
		// 1.1 Add m_AttachedRacksHeightError
		protected static string _sTieBeam_strMajor = "TieBeam_MAJOR";
		protected static int _sTieBeam_MAJOR = 1;
		protected static string _sTieBeam_strMinor = "TieBeam_MINOR";
		protected static int _sTieBeam_MINOR = 1;
		//=============================================================================
		public TieBeam(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sTieBeam_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sTieBeam_strMinor, typeof(int));
			if (iMajor > _sTieBeam_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sTieBeam_MAJOR && iMinor > _sTieBeam_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sTieBeam_MAJOR)
			{
				try
				{
					if(iMajor >= 1 && iMinor >= 0)
					{
						m_GUID = (Guid)info.GetValue("GUID", typeof(Guid));
					}

					if (iMajor >= 1 && iMinor >= 1)
						m_AttachedRacksHeightError = (bool)info.GetValue("AttachedRacksHeightError", typeof(bool));
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
			info.AddValue(_sTieBeam_strMajor, _sTieBeam_MAJOR);
			info.AddValue(_sTieBeam_strMinor, _sTieBeam_MINOR);

			// 1.0
			info.AddValue("GUID", m_GUID);

			// 1.1
			info.AddValue("AttachedRacksHeightError", m_AttachedRacksHeightError);
		}

		#endregion
	}
}
