using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public class SheetElevationGeometry_State : GeometryState
	{
		public SheetElevationGeometry_State(SheetElevationGeometry sheetElevationGeomemtry)
			: base(sheetElevationGeomemtry) { }
		public SheetElevationGeometry_State(SheetElevationGeometry_State state)
			: base(state) { }

		//=============================================================================
		protected override GeometryState MakeDeepCopy()
		{
			return new SheetElevationGeometry_State(this);
		}
	}

	/// <summary>
	/// Describes (x;y)-position for vertical and horizontal sheet elevation pictures.
	/// Each sheet should contains exactly 1 instance of SheetElevationGeometry.
	/// It can't be copied or deleted.
	/// User can move this geometry, it means change (x;y)-position for sheet elevations.
	/// </summary>
	[Serializable]
	public class SheetElevationGeometry : BaseRectangleGeometry, ISerializable
	{
		// SheetElevationGeometry circle radius in global units(not pixels)
		private static double SHEET_ELEVATION_GEOMETRY_RADIUS = 300;
		private static double SHEET_ELEVATION_GEOMETRY_RADIUS_IN_PIXELS = 10;

		public SheetElevationGeometry(DrawingSheet ds)
			: base(ds)
		{
			Name = "Sheet Elevation";
			Text = "SE";

			this.Length_X = 1.0;
			this.Length_Y = 1.0;
			this.Length_Z = 1;

			this.ShowRotationGrips = false;
			this.IsInit = true;
		}

		#region Properties

		#endregion

		#region Methods

		//=============================================================================
		protected override BaseRectangleGeometry CreateInstance()
		{
			return new SheetElevationGeometry(null);
		}
		//=============================================================================
		protected override GeometryState _GetOriginalState()
		{
			return new SheetElevationGeometry_State(this);
		}
		//=============================================================================
		protected override void _SetState(GeometryState state)
		{
			base._SetState(state);
		}

		//=============================================================================
		protected override void _InitProperties()
		{
			if (m_Properties == null)
				m_Properties = new System.Collections.ObjectModel.ObservableCollection<Property_ViewModel>();

			GeometryProperty nameProp = new GeometryProperty(this, BaseRectangleGeometry.PROP_NAME, false, "Geometry");
			nameProp.IsReadOnly = true;
			m_Properties.Add(nameProp);
			m_Properties.Add(new GeometryProperty(this, PROP_CENTER_POINT_X, "Center point X", true, "Top left point"));
			m_Properties.Add(new GeometryProperty(this, PROP_CENTER_POINT_Y, "Center point Y", true, "Top left point"));
		}

		//=============================================================================
		public override object GetPropertyValue(string strPropSysName)
		{
			if (PROP_CENTER_POINT_X == strPropSysName)
				return m_TopLeft_GlobalPoint.X;
			else if (PROP_CENTER_POINT_Y == strPropSysName)
				return m_TopLeft_GlobalPoint.Y;

			return base.GetPropertyValue(strPropSysName);
		}

		//=============================================================================
		public override bool SetPropertyValue(string strPropSysName, object propValue, bool bWasChangedViaProperties, bool bChangeTheSameRectangles, bool bNotifySheet, out string strError, bool bCheckLayout = true)
		{
			string propertySystemName = strPropSysName;
			if (PROP_CENTER_POINT_X == strPropSysName)
				propertySystemName = PROP_TOP_LEFT_POINT_X;
			else if(PROP_CENTER_POINT_Y == strPropSysName)
				propertySystemName = PROP_TOP_LEFT_POINT_Y;

			return base.SetPropertyValue(propertySystemName, propValue, bWasChangedViaProperties, bChangeTheSameRectangles, bNotifySheet, out strError, bCheckLayout);
		}

		//=============================================================================
		public override List<Point> GetGripPoints()
		{
			// User can move geometry using center grip point only.
			List<Point> grips = new List<Point>();
			grips.Add(this.Center_GlobalPoint);

			return grips;
		}

		//=============================================================================
		public override bool SetGripPoint(int gripIndex, Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			// Only center grip point is allowed.
			return base.SetGripPoint(GRIP_CENTER, globalPoint, DrawingLength, DrawingWidth);
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

			Pen borderPen = this.BorderPen;
			//
			// If fill with transparent color then circle fill area will act in HitTest.
			// Fill with null brush will disable circle HitTest on click in fill area.
			Color fillColor = displaySettings.GetFillColor(this);
			Color textColor = displaySettings.GetTextColor(this);
			//
			Brush fillBrush = new SolidColorBrush(fillColor);
			fillBrush.Opacity = displaySettings.FillBrushOpacity;

			// Draw circle
			double circleRadiusInPixels = SHEET_ELEVATION_GEOMETRY_RADIUS_IN_PIXELS;// GetWidthInPixels(cs, SHEET_ELEVATION_GEOMETRY_RADIUS);
			Point TopLeft_ScreenPoint = GetLocalPoint(cs, m_TopLeft_GlobalPoint);
			dc.DrawEllipse(fillBrush, borderPen, TopLeft_ScreenPoint, circleRadiusInPixels, circleRadiusInPixels);

			// draw text
			if (displaySettings.DisplayText && !string.IsNullOrEmpty(Text))
			{
				Brush br = new SolidColorBrush(textColor);
				br.Opacity = displaySettings.FillBrushOpacity;

				//
				FontFamily textFontFamily = new FontFamily("Arial");
				Typeface textTypeFace = new Typeface(textFontFamily, FontStyles.Normal, displaySettings.TextWeight, FontStretches.Normal);

				FormattedText formattedText = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, textTypeFace, displaySettings.TextFontSize, br);
				formattedText.TextAlignment = TextAlignment.Center;

				Point Text_ScreenPoint = GetLocalPoint(cs, TopLeft_GlobalPoint);
				Text_ScreenPoint.Y -= formattedText.Height / 2;
				dc.DrawText(formattedText, Text_ScreenPoint);
			}
		}

		//=============================================================================
		/// <summary>
		/// Places this geometry at the center of the sheet
		/// </summary>
		public void PlaceAtSheetCenter()
		{
			if (this.Sheet == null)
				return;

			m_TopLeft_GlobalPoint.X = this.Sheet.Length / 2;
			m_TopLeft_GlobalPoint.Y = this.Sheet.Width / 2;
		}

		#endregion

		#region Serialization

		//=============================================================================
		protected static string _sSheetElevationGeometry_strMajor = "SheetElevationGeometry_MAJOR";
		protected static int _sSheetElevationGeometry_MAJOR = 1;
		protected static string _sSheetElevationGeometry_strMinor = "SheetElevationGeometry_MINOR";
		protected static int _sSheetElevationGeometry_MINOR = 0;
		//=============================================================================
		public SheetElevationGeometry(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sSheetElevationGeometry_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sSheetElevationGeometry_strMinor, typeof(int));
			if (iMajor > _sSheetElevationGeometry_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sSheetElevationGeometry_MAJOR && iMinor > _sSheetElevationGeometry_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sSheetElevationGeometry_MAJOR)
			{
				try
				{
					//
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
			info.AddValue(_sSheetElevationGeometry_strMajor, _sSheetElevationGeometry_MAJOR);
			info.AddValue(_sSheetElevationGeometry_strMinor, _sSheetElevationGeometry_MINOR);
		}

		#endregion
	}
}
