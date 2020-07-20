using AppColorTheme;
using AppInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DrawingControl
{
	internal enum eMinDelta : int
	{
		eNothing = 0,
		eBot,
		eTop,
		eLeft,
		eRight
	}

	/// <summary>
	/// State of geometry.
	/// Use for save current geometry state before changing property.
	/// If set property is failed then restore old geometry values from saved state.
	/// </summary>
	public abstract class GeometryState : IClonable
	{
		public GeometryState(BaseRectangleGeometry geom)
			: this(
				  geom.TopLeft_GlobalPoint,
				  //
				  geom.Length_X,
				  geom.MinLength_X,
				  geom.MaxLength_X,
				  geom.StepLength_X,
				  geom.MarginX,
				  //
				  geom.Length_Y,
				  geom.MinLength_Y,
				  geom.MaxLength_Y,
				  geom.StepLength_Y,
				  geom.MarginY,
				  //
				  geom.Length_Z,
				  geom.MinLength_Z,
				  geom.MaxLength_Z,
				  geom.StepLength_Z,
				  //
				  geom.Text,
				  geom.Name,
				  geom.IsHorizontal,
				  //
				  geom.IsInit,
				  geom.IsSelected,
				  geom.FillColor
				)
		{ }
		public GeometryState(GeometryState geomState)
			: this(
				  geomState.TopLeftPnt,
				  //
				  geomState.Length_X,
				  geomState.MinLength_X,
				  geomState.MaxLength_X,
				  geomState.StepLength_X,
				  geomState.Margin_X,
				  //
				  geomState.Length_Y,
				  geomState.MinLength_Y,
				  geomState.MaxLength_Y,
				  geomState.StepLength_Y,
				  geomState.Margin_Y,
				  //
				  geomState.Length_Z,
				  geomState.MinLength_Z,
				  geomState.MaxLength_Z,
				  geomState.StepLength_Z,
				  //
				  geomState.Text,
				  geomState.Name,
				  geomState.IsHorizontal,
				  //
				  geomState.IsInit,
				  geomState.IsSelected,
				  geomState.FillColor
				  )
		{ }
		public GeometryState(
			Point topLeftPnt,
			//
			double LenX,
			double minLenX,
			double maxLenX,
			double stepLenX,
			double marginX,
			//
			double LenY,
			double minLenY,
			double maxLenY,
			double stepLenY,
			double marginY,
			//
			int LenZ,
			int minLenZ,
			int maxLenZ,
			int stepLenZ,
			//
			string text,
			string name,
			bool isHorizontal,
			//
			bool isInit,
			bool isSelected,
			Color fillColor
			)
		{
			TopLeftPnt = topLeftPnt;
			//
			Length_X = LenX;
			MinLength_X = minLenX;
			MaxLength_X = maxLenX;
			StepLength_X = stepLenX;
			Margin_X = marginX;
			//
			Length_Y = LenY;
			MinLength_Y = minLenY;
			MaxLength_Y = maxLenY;
			StepLength_Y = stepLenY;
			Margin_Y = marginY;
			//
			Length_Z = LenZ;
			MinLength_Z = minLenZ;
			MaxLength_Z = maxLenZ;
			StepLength_Z = stepLenZ;
			//
			Text = text;
			Name = name;
			IsHorizontal = isHorizontal;
			//
			IsInit = isInit;
			IsSelected = isSelected;
			FillColor = fillColor;
		}

		//
		public Point TopLeftPnt { get; private set; }
		//
		public double Length_X { get; private set; }
		public double MinLength_X { get; private set; }
		public double MaxLength_X { get; private set; }
		public double StepLength_X { get; private set; }
		public double Margin_X { get; private set; }
		//
		public double Length_Y { get; private set; }
		public double MinLength_Y { get; private set; }
		public double MaxLength_Y { get; private set; }
		public double StepLength_Y { get; private set; }
		public double Margin_Y { get; private set; }
		//
		public int Length_Z { get; private set; }
		public int MinLength_Z { get; private set; }
		public int MaxLength_Z { get; private set; }
		public int StepLength_Z { get; private set; }
		//
		public string Text { get; private set; }
		public string Name { get; private set; }
		public bool IsHorizontal { get; private set; }
		//
		public bool IsInit { get; private set; }
		public bool IsSelected { get; private set; }
		public Color FillColor { get; private set; }

		#region IClonable

		//=============================================================================
		public virtual IClonable Clone()
		{
			return this.MakeDeepCopy();
		}
		//
		protected abstract GeometryState MakeDeepCopy();

		#endregion
	}

	// Base rectangle logic
	[Serializable]
	public abstract class BaseRectangleGeometry : ISerializable, IDeserializationCallback, IClonable
	{
		public static int GRIP_TOP_LEFT = 0;
		public static int GRIP_CENTER = 1;
		public static int GRIP_BOTTOM_RIGHT = 2;

		//
		protected string m_strText = string.Empty;
		//
		protected string m_strRectangleName = string.Empty;
		// true - horizontal rectnagle, it means that Length is Distance_X
		// false - rotate by 90 degrees, it means that Length is Distance_Y now
		protected bool m_bIsHorizontal = true;

		//
		// m_TopLeft_GlobalPoint, m_GlobalLength, m_GlobalWidth should be whole numbers.
		//
		// All this values are presented in global coordinates.
		protected Point m_TopLeft_GlobalPoint = new Point(0, 0);
		//
		protected double m_Length_X = 3500.0;
		protected double m_Length_Y = 1000.0;
		protected int m_Length_Z = 1;
		//
		private double m_MinLength_X = 1000;
		private double m_MaxLength_X = double.PositiveInfinity;
		private double m_StepLength_X = 100;
		//
		private double m_MinLength_Y = 1000;
		private double m_MaxLength_Y = double.PositiveInfinity;
		private double m_StepLength_Y = 100;
		//
		private int m_MinLength_Z = 1;
		private int m_MaxLength_Z = 1;
		private int m_StepLength_Z = 1;
		//
		private double m_MarginX = 0.0;
		private double m_MarginY = 0.0;

		//
		protected DrawingSheet m_Sheet = null;

		// PROPERTIES
		public static string PROP_TOP_LEFT_POINT = "Top left point";
		public static string PROP_TOP_LEFT_POINT_X = "Top left point X";
		public static string PROP_TOP_LEFT_POINT_Y = "Top left point Y";
		public static string PROP_CENTER_POINT = "Center point";
		public static string PROP_CENTER_POINT_X = "Center point X";
		public static string PROP_CENTER_POINT_Y = "Center point Y";
		public static string PROP_BOT_RIGHT_POINT = "Bot right point";
		public static string PROP_DIMENSION_X = "Dimension X";
		public static string PROP_DIMENSION_Y = "Dimension Y";
		public static string PROP_DIMENSION_Z = "Dimension Z";
		public static string PROP_NAME = "Name";

		public BaseRectangleGeometry(DrawingSheet _sheet)
		{
			m_Sheet = _sheet;

			ShowRotationGrips = true;

			IsInit = true;
		}

		/// <summary>
		/// Point around which rotate command executes.
		/// </summary>
		public enum eRotateRelativePoint : int
		{
			/// <summary>
			/// Top left point will not be changed
			/// </summary>
			eTopLeft = 1,
			/// <summary>
			/// Center point will not be changed
			/// </summary>
			eCenter = 2
		}

		#region Properties

		//=============================================================================
		public GeometryWrapper Wrapper { get; set; }

		//=============================================================================
		public DrawingSheet Sheet
		{
			get { return m_Sheet; }
			set { m_Sheet = value; }
		}

		//=============================================================================
		public Size DrawingGlobalSize
		{
			get
			{
				Size size = new Size(1, 1);

				if (m_Sheet != null)
				{
					size.Width = m_Sheet.Length;
					size.Height = m_Sheet.Width;
				}

				return size;
			}
		}

		//=============================================================================
		public double Length
		{
			get
			{
				if (this.m_bIsHorizontal)
					return m_Length_X;

				return m_Length_Y;
			}
			set
			{
				if (this.m_bIsHorizontal)
					this.Length_X = value;
				else
					this.Length_Y = value;
			}
		}
		//=============================================================================
		public double Depth
		{
			get
			{
				if (this.m_bIsHorizontal)
					return m_Length_Y;

				return m_Length_X;
			}
			set
			{
				if (this.m_bIsHorizontal)
					this.Length_Y = value;
				else
					this.Length_X = value;
			}
		}

		//=============================================================================
		public double Length_X
		{
			get { return m_Length_X; }
			set
			{
				if (Utils.FNE(m_Length_X, value))
				{
					m_Length_X = value;
					_MarkStateChanged();
				}
			}
		}
		public virtual double Length_Y
		{
			get { return m_Length_Y; }
			set
			{
				if (Utils.FNE(m_Length_Y, value))
				{
					m_Length_Y = value;
					_MarkStateChanged();
				}
			}
		}
		public int Length_Z
		{
			get { return m_Length_Z; }
			set
			{
				if (Utils.FNE(m_Length_Z, value))
				{
					m_Length_Z = value;
					_MarkStateChanged();
				}
			}
		}

		//=============================================================================
		public virtual double MinLength_X
		{
			get { return m_MinLength_X; }
			set
			{
				if (Utils.FNE(m_MinLength_X, value))
				{
					m_MinLength_X = value;
					_MarkStateChanged();
				}
			}
		}
		public virtual double MaxLength_X
		{
			get { return m_MaxLength_X; }
			set
			{
				if (Utils.FNE(m_MaxLength_X, value))
				{
					m_MaxLength_X = value;
					_MarkStateChanged();
				}
			}
		}
		public double StepLength_X
		{
			get { return m_StepLength_X; }
			set
			{
				if (Utils.FNE(m_StepLength_X, value))
				{
					m_StepLength_X = value;
					_MarkStateChanged();
				}
			}
		}

		//=============================================================================
		public virtual double MinLength_Y
		{
			get { return m_MinLength_Y; }
			set
			{
				if (Utils.FNE(m_MinLength_Y, value))
				{
					m_MinLength_Y = value;
					_MarkStateChanged();
				}
			}
		}
		public virtual double MaxLength_Y
		{
			get { return m_MaxLength_Y; }
			set
			{
				if (Utils.FNE(m_MaxLength_Y, value))
				{
					m_MaxLength_Y = value;
					_MarkStateChanged();
				}
			}
		}
		public double StepLength_Y
		{
			get { return m_StepLength_Y; }
			set
			{
				if (Utils.FNE(m_StepLength_Y, value))
				{
					m_StepLength_Y = value;
					_MarkStateChanged();
				}
			}
		}

		//=============================================================================
		public virtual int MinLength_Z
		{
			get { return m_MinLength_Z; }
			set
			{
				if (Utils.FNE(m_MinLength_Z, value))
				{
					m_MinLength_Z = value;
					_MarkStateChanged();
				}
			}
		}
		public virtual int MaxLength_Z
		{
			get { return m_MaxLength_Z; }
			set
			{
				if (Utils.FNE(m_MaxLength_Z, value))
				{
					m_MaxLength_Z = value;
					_MarkStateChanged();
				}
			}
		}
		public int StepLength_Z
		{
			get { return m_StepLength_Z; }
			set
			{
				if (Utils.FNE(m_StepLength_Z, value))
				{
					m_StepLength_Z = value;
					_MarkStateChanged();
				}
			}
		}

		//=============================================================================
		public virtual double MarginX
		{
			get { return m_MarginX; }
			set
			{
				if (Utils.FNE(m_MarginX, value))
				{
					m_MarginX = value;
					_MarkStateChanged();
				}
			}
		}
		public virtual double MarginY
		{
			get { return m_MarginY; }
			set
			{
				if (Utils.FNE(m_MarginY, value))
				{
					m_MarginY = value;
					_MarkStateChanged();
				}
			}
		}

		//=============================================================================
		public bool ShowRotationGrips { get; set; }

		//=============================================================================
		public bool IsHorizontal
		{
			get { return m_bIsHorizontal; }
			set
			{
				if (m_bIsHorizontal != value)
				{
					m_bIsHorizontal = value;
					_MarkStateChanged();
				}
			}
		}

		//=============================================================================
		public string Text
		{
			get
			{
				if (string.IsNullOrEmpty(m_strText))
					return m_strRectangleName;

				return m_strText;
			}
			set
			{
				string strNewValue = string.Empty;
				if (!string.IsNullOrEmpty(value))
					strNewValue = value;

				m_strText = strNewValue;
			}
		}

		//=============================================================================
		public string Name
		{
			get { return m_strRectangleName; }
			set
			{
				if(value != m_strRectangleName)
				{
					string strNewValue = string.Empty;
					if (!string.IsNullOrEmpty(value))
						strNewValue = value;

					m_strRectangleName = strNewValue;
				}
			}
		}

		//=============================================================================
		public Point TopLeft_GlobalPoint
		{
			get { return m_TopLeft_GlobalPoint; }
			set
			{
				if (Utils.FNE(m_TopLeft_GlobalPoint.X, value.X) || Utils.FNE(m_TopLeft_GlobalPoint.Y, value.Y))
				{
					m_TopLeft_GlobalPoint = value;
					_MarkStateChanged();
					_CheckWholeNumbers();
				}
			}
		}

		//=============================================================================
		public Point BottomLeft_GlobalPoint
		{
			get
			{
				Point _bottomLeftPoint = m_TopLeft_GlobalPoint;
				_bottomLeftPoint.Y += m_Length_Y;
				return _bottomLeftPoint;
			}
		}

		//=============================================================================
		public Point BottomRight_GlobalPoint
		{
			get
			{
				Point rightBottomPoint = m_TopLeft_GlobalPoint;
				rightBottomPoint.X += m_Length_X;
				rightBottomPoint.Y += m_Length_Y;

				return rightBottomPoint;
			}
		}

		//=============================================================================
		public Point TopRight_GlobalPoint
		{
			get
			{
				Point rightTopPoint = m_TopLeft_GlobalPoint;
				rightTopPoint.X += m_Length_X;

				return rightTopPoint;
			}
		}

		//=============================================================================
		public Point Center_GlobalPoint
		{
			get
			{
				Point _centerPoint = m_TopLeft_GlobalPoint;
				_centerPoint.X += m_Length_X / 2;
				_centerPoint.Y += m_Length_Y / 2;
				return _centerPoint;
			}
		}

		//=============================================================================
		/// <summary>
		/// Is geometry placed in DrawingSheet.SelectedGeometryCollection.
		/// Need to read\write this falg in the stream.
		/// Because this flag is necessary to restore with DrawingDocument.ShowAdvancedProperties flag on undo\redo.
		/// If only DrawingDocument.ShowAdvancedProperties without DrawingSheet.SelectedGeometryCollection then
		/// DrawingControl can display empty space because ShowAdvancedProperties=true, but SelectedGeometry = null.
		/// 
		/// Probably it is possible to read\write DrawingSheet.SelectedGeometryCollection instead this falg.
		/// Need to check it.
		/// </summary>
		private bool m_IsSelected = false;
		public bool IsSelected
		{
			get { return m_IsSelected; }
			set
			{
				if (value != m_IsSelected)
					m_IsSelected = value;
			}
		}

		//=============================================================================
		// false - rectangle place at the center of Graphics Area for init. User allows to change its
		// size and rotations by grip points. Dont check layout for this rectangle.
		private bool m_IsInit = true;
		public virtual bool IsInit
		{
			get { return m_IsInit; }
			set
			{
				if (value != m_IsInit)
					m_IsInit = value;
			}
		}

		//=============================================================================
		/// <summary>
		/// Color which is used for fill geometry rectangle on draw
		/// </summary>
		protected Color m_FillColor = Colors.LightGray;
		public virtual Color FillColor
		{
			get
			{
				if(CurrentGeometryColorsTheme.CurrentTheme != null)
				{
					eColorType colorType = CurrentGeometryColorsTheme.GeometryToFillColorType(this);
					Color colorValue;
					if (eColorType.eUndefined != colorType && CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(colorType, out colorValue))
						return colorValue;
				}

				return m_FillColor;
			}
		}

		/// <summary>
		/// Rectangle border thickness and color
		/// </summary>
		protected double m_BorderThickness = 1.0;
		protected Color m_BorderColor = Colors.Gray;
		protected Pen BorderPen
		{
			get
			{
				Color borderColor = m_BorderColor;
				if (CurrentGeometryColorsTheme.CurrentTheme != null)
				{
					Color color;
					if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eGeometryBorderColor, out color))
						borderColor = color;
				}

				return new Pen(new SolidColorBrush(borderColor), m_BorderThickness);
			}
		}

		//=============================================================================
		protected ObservableCollection<Property_ViewModel> m_Properties = null;
		public ObservableCollection<Property_ViewModel> Properties
		{
			get
			{
				if (m_Properties == null)
				{
					m_Properties = new ObservableCollection<Property_ViewModel>();
					_InitProperties();
				}

				return m_Properties;
			}
		}

		//=============================================================================
		private ICollectionView m_PropertiesCollection = null;
		public ICollectionView PropertiesCollection
		{
			get
			{
				if(m_PropertiesCollection == null)
				{
					m_PropertiesCollection = CollectionViewSource.GetDefaultView(Properties);
					if(m_PropertiesCollection != null)
					{
						m_PropertiesCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
					}
				}

				return m_PropertiesCollection;
			}
		}

		//=============================================================================
		protected virtual bool _Is_HeightProperty_ReadOnly { get { return true; } }

		//=============================================================================
		/// <summary>
		/// 
		/// </summary>
		private long m_SheetUpdateNumber = 0;
		public long SheetUpdateNumber { get { return m_SheetUpdateNumber; } }

		#endregion

		#region Functions

		//=============================================================================
		public virtual void Draw(DrawingContext dc, ICoordinateSystem cs, IGeomDisplaySettings geomDisplaySettings = null)
		{
			if (dc == null)
				return;

			if (cs == null)
				return;

			if (m_Sheet == null)
				return;

			if (m_Sheet.Document == null)
				return;

			IGeomDisplaySettings displaySettings = geomDisplaySettings;
			// if NULL then get settings from the sheet
			if (displaySettings == null)
				displaySettings = m_Sheet;
			// if NULL then get default settings
			if (displaySettings == null)
				displaySettings = DefaultGeomDisplaySettings.GetInstance();
			if (displaySettings == null)
				return;

			Pen _pen = this.BorderPen;
			//
			// If fill with transparent color then circle fill area will act in HitTest.
			// Fill with null brush will disable circle HitTest on click in fill area.
			Color fillColor = displaySettings.GetFillColor(this);
			Color textColor = displaySettings.GetTextColor(this);
			//
			Brush fillBrush = new SolidColorBrush(fillColor);
			fillBrush.Opacity = displaySettings.FillBrushOpacity;

			//
			double LengthInPixels = GetWidthInPixels(cs, Length_X);
			double WidthInPixels = GetHeightInPixels(cs, Length_Y);

			//
			Point TopLeft_ScreenPoint = GetLocalPoint(cs, TopLeft_GlobalPoint);
			Point BottomRight_ScreenPoint = GetLocalPoint(cs, BottomRight_GlobalPoint);
			Point Center_ScreenPoint = GetLocalPoint(cs, Center_GlobalPoint);

			dc.DrawRectangle(fillBrush, _pen, new Rect(TopLeft_ScreenPoint, BottomRight_ScreenPoint));

			// draw text
			if (!string.IsNullOrEmpty(Text))
			{
				Brush br = new SolidColorBrush(textColor);
				br.Opacity = displaySettings.FillBrushOpacity;

				double TextWidth = LengthInPixels;
				double TextHeight = WidthInPixels;
				//
				if (!m_bIsHorizontal)
				{
					double rOldTextWidth = TextWidth;
					TextWidth = TextHeight;
					TextHeight = rOldTextWidth;
				}

				//
				FontFamily textFontFamily = new FontFamily("Arial");
				Typeface textTypeFace = new Typeface(textFontFamily, FontStyles.Normal, displaySettings.TextWeight, FontStretches.Normal);

				FormattedText formattedText = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, textTypeFace, displaySettings.TextFontSize, br);
				formattedText.MaxTextWidth = TextWidth;
				formattedText.MaxTextHeight = TextHeight;
				formattedText.TextAlignment = TextAlignment.Center;

				Point Text_ScreenPoint = GetLocalPoint(cs, TopLeft_GlobalPoint);
				Text_ScreenPoint.Y += WidthInPixels / 2;
				Text_ScreenPoint.Y -= formattedText.Height / 2;
				if (!m_bIsHorizontal)
				{
					Text_ScreenPoint.X += LengthInPixels / 2;
					Text_ScreenPoint.X -= formattedText.MaxTextWidth / 2;
				}

				//
				RotateTransform textRotateTransform = null;
				if (!m_bIsHorizontal)
					textRotateTransform = new RotateTransform(-90, Center_ScreenPoint.X, Center_ScreenPoint.Y);

				if (textRotateTransform != null)
					dc.PushTransform(textRotateTransform);

				dc.DrawText(formattedText, Text_ScreenPoint);

				if (textRotateTransform != null)
					dc.Pop();
			}
		}

		//=============================================================================
		public virtual void OnMouseMove(Point mousePoint, double DrawingLength, double DrawingWidth)
		{
			SetGripPoint(GRIP_CENTER, mousePoint, DrawingLength, DrawingWidth);
		}

		//=============================================================================
		public virtual List<Point> GetGripPoints()
		{
			List<Point> grips = new List<Point>();
			// 0 top left
			grips.Add(m_TopLeft_GlobalPoint);
			// 1 center
			Point centerPoint = m_TopLeft_GlobalPoint;
			centerPoint.X += m_Length_X / 2;
			centerPoint.Y += m_Length_Y / 2;
			grips.Add(centerPoint);
			// 2 bottom right
			Point bottomRight = m_TopLeft_GlobalPoint;
			bottomRight.X += m_Length_X;
			bottomRight.Y += m_Length_Y;
			grips.Add(bottomRight);

			return grips;
		}

		//=============================================================================
		public virtual bool SetGripPoint(int gripIndex, Point globalPoint, double DrawingLength, double DrawingWidth)
		{
			//
			if (GRIP_TOP_LEFT == gripIndex || GRIP_CENTER == gripIndex || GRIP_BOTTOM_RIGHT == gripIndex)
			{
				string strError;
				bool bRes = false;

				GeometryState oldState = this._GetClonedState();

				if (GRIP_TOP_LEFT == gripIndex)
					bRes = _SetPropertyValue(PROP_TOP_LEFT_POINT, globalPoint, DrawingLength, DrawingWidth, false, out strError);
				else if (GRIP_CENTER == gripIndex)
					bRes = _SetPropertyValue(PROP_CENTER_POINT, globalPoint, DrawingLength, DrawingWidth, false, out strError);
				else if( GRIP_BOTTOM_RIGHT == gripIndex)
					bRes = _SetPropertyValue(PROP_BOT_RIGHT_POINT, globalPoint, DrawingLength, DrawingWidth, false, out strError);

				if (!bRes)
					this._SetState(oldState);

				if(bRes)
					_MarkStateChanged();

				this.UpdateProperties();

				return bRes;
			}

			return false;
		}

		//=============================================================================
		public virtual object GetPropertyValue(string strPropSysName)
		{
			if (string.IsNullOrEmpty(strPropSysName))
				return null;

			if (PROP_TOP_LEFT_POINT_X == strPropSysName)
				return TopLeft_GlobalPoint.X;
			else if (PROP_TOP_LEFT_POINT_Y == strPropSysName)
				return TopLeft_GlobalPoint.Y;
			else if (PROP_DIMENSION_X == strPropSysName)
					return Length_X;
			else if (PROP_DIMENSION_Y == strPropSysName)
					return Length_Y;
			else if (PROP_DIMENSION_Z == strPropSysName)
				return m_Length_Z;
			else if (PROP_NAME == strPropSysName)
				return m_strRectangleName;

			return null;
		}

		//=============================================================================
		public virtual bool SetPropertyValue(string strPropSysName, object propValue, bool bWasChangedViaProperties, bool bChangeTheSameRectangles, bool bNotifySheet, out string strError, bool bCheckLayout = true)
		{
			strError = string.Empty;

			if (string.IsNullOrEmpty(strPropSysName))
				return false;

			if (m_Sheet == null)
				return false;

			GeometryState oldState = this._GetClonedState();

			bool bRes = false;
			try
			{
				if (PROP_TOP_LEFT_POINT_X == strPropSysName || PROP_TOP_LEFT_POINT_Y == strPropSysName)
				{
					Point newTopLeft_GlobalPoint = TopLeft_GlobalPoint;
					if (PROP_TOP_LEFT_POINT_X == strPropSysName)
						newTopLeft_GlobalPoint.X = Convert.ToDouble(propValue);
					else if (PROP_TOP_LEFT_POINT_Y == strPropSysName)
						newTopLeft_GlobalPoint.Y = Convert.ToDouble(propValue);

					// dont stretch it, just move by center point
					Point newCenterPoint = newTopLeft_GlobalPoint;
					newCenterPoint.X += this.Length_X / 2;
					newCenterPoint.Y += this.Length_Y / 2;

					//
					bRes = this._SetPropertyValue(PROP_CENTER_POINT, newCenterPoint, m_Sheet.Length, m_Sheet.Width, bWasChangedViaProperties, out strError, bCheckLayout);
				}
				else if (PROP_DIMENSION_X == strPropSysName
					|| PROP_DIMENSION_Y == strPropSysName
					|| PROP_DIMENSION_Z == strPropSysName
					|| PROP_NAME == strPropSysName)
				{
					string _strPropName = strPropSysName;
					bRes =  this._SetPropertyValue(_strPropName, propValue, m_Sheet.Length, m_Sheet.Width, bWasChangedViaProperties, out strError, bCheckLayout);
				}
				else
					bRes = this._SetPropertyValue(strPropSysName, propValue, m_Sheet.Length, m_Sheet.Width, bWasChangedViaProperties, out strError, bCheckLayout);
			}
			catch { }

			if(bRes)
				_MarkStateChanged();

			if (bNotifySheet && m_Sheet != null)
				m_Sheet.OnPropertyChanged(this, strPropSysName, bRes, strError);

			if (!bRes)
				this._SetState(oldState);

			this.UpdateProperties();

			return bRes;
		}

		//=============================================================================
		/// <summary>
		/// Rotates geometry by 90 degrees.
		/// Rotate applies to MinLength_X(Y), StepLength_X(Y), Length_X(Y).
		/// </summary>
		/// <param name="rotatePnt">Point which should keep its position during rotate</param>
		/// <param name="bCheckLayout">Check - does geometry after rotate overlaps any other geometry or goes out the drawing area.</param>
		/// <param name="bTryToFixLayout">If result layout is not correct after rotate, then try to fix layout by move rotated geometry.</param>
		public bool Rotate(double DrawingLength, double DrawingWidth, eRotateRelativePoint rotatePnt = eRotateRelativePoint.eTopLeft, bool bCheckLayout = true, bool bTryToFixLayout = true)
		{
			GeometryState oldState = this._GetClonedState();

			bool bRes = _RotateGeometry(DrawingLength, DrawingWidth, rotatePnt, bCheckLayout, bTryToFixLayout);

			if (!bRes)
				this._SetState(oldState);

			if (bRes)
				_MarkStateChanged();

			return bRes;
		}

		//=============================================================================
		public bool IsOverlap(BaseRectangleGeometry rectangle)
		{
			// SheetElevationGeometry can overlap all other geometry
			if (this is SheetElevationGeometry || rectangle is SheetElevationGeometry)
				return false;

			if (rectangle == null)
				return false;

			// if rectangles are not overlapped then one of them should be on the left or bottom side of other
			// There should be 0 gap between two rectangles.
			// For example: left rectangle has topright point (100, 0) and right rectangle has top left point (100, 0) - its correct.

			double _marginX = BaseRectangleGeometry.CalculateMargin(true, this, rectangle);
			// if one rectangle is on the left side of other
			if (Utils.FGE(this.TopLeft_GlobalPoint.X - _marginX, rectangle.BottomRight_GlobalPoint.X) || Utils.FGE(rectangle.TopLeft_GlobalPoint.X - _marginX, this.BottomRight_GlobalPoint.X))
				return false;

			double _marginY = BaseRectangleGeometry.CalculateMargin(false, this, rectangle);
			// if one rectangle is above other
			if (Utils.FGE(this.TopLeft_GlobalPoint.Y - _marginY, rectangle.BottomRight_GlobalPoint.Y) || Utils.FGE(rectangle.TopLeft_GlobalPoint.Y - _marginY, this.BottomRight_GlobalPoint.Y))
				return false;

			// Probably one of rectangles is swing door shutter.
			if((this is Shutter && !(rectangle is Shutter)) || (rectangle is Shutter && !(this is Shutter)))
			{
				Shutter shutter = this as Shutter;
				BaseRectangleGeometry notShutter = rectangle;
				if(shutter == null)
				{
					shutter = rectangle as Shutter;
					notShutter = this;
				}

				if(shutter != null && notShutter != null && shutter.SwingDoor)
				{
					// Check that notShutter geometry is placed in shutter door area.
					// Calculate bound box which contains shutter with doors.
					Point bbTopLeftPnt = new Point(0.0, 0.0);
					Point bbBotRightPnt = new Point(0.0, 0.0);
					if(shutter.IsHorizontal)
					{
						if(Utils.FLE(shutter.TopLeft_GlobalPoint.Y, 0.0))
						{
							bbTopLeftPnt = shutter.TopLeft_GlobalPoint;
							bbBotRightPnt = shutter.BottomRight_GlobalPoint + shutter.SwingDoorLength * new Vector(0.0, 1.0);
						}
						else
						{
							bbTopLeftPnt = shutter.TopLeft_GlobalPoint + shutter.SwingDoorLength * new Vector(0.0, -1.0);
							bbBotRightPnt = shutter.BottomRight_GlobalPoint;
						}
					}
					else
					{
						if(Utils.FLE(shutter.TopLeft_GlobalPoint.X, 0.0))
						{
							bbTopLeftPnt = shutter.TopLeft_GlobalPoint;
							bbBotRightPnt = shutter.BottomRight_GlobalPoint + shutter.SwingDoorLength * new Vector(1.0, 0.0);
						}
						else
						{
							bbTopLeftPnt = shutter.TopLeft_GlobalPoint + shutter.SwingDoorLength * new Vector(-1.0, 0.0);
							bbBotRightPnt = shutter.BottomRight_GlobalPoint;
						}
					}
					// Use the formaula above to determine - is notShutter intersect shutter bound box.
					// if one rectangle is on the left side of other
					if (Utils.FGE(bbTopLeftPnt.X - notShutter.MarginX, notShutter.BottomRight_GlobalPoint.X) || Utils.FGE(notShutter.TopLeft_GlobalPoint.X - notShutter.MarginX, bbBotRightPnt.X))
						return false;
					// if one rectangle is above other
					if (Utils.FGE(bbTopLeftPnt.Y - notShutter.MarginY, notShutter.BottomRight_GlobalPoint.Y) || Utils.FGE(notShutter.TopLeft_GlobalPoint.Y - notShutter.MarginY, bbBotRightPnt.Y))
						return false;

					// check that notShutter is not placed in open door area
					List<Point> shutterCornerPointsList = new List<Point>() { shutter.TopLeft_GlobalPoint, shutter.TopRight_GlobalPoint, shutter.BottomLeft_GlobalPoint, shutter.BottomRight_GlobalPoint };
					double swingDoorLength = shutter.SwingDoorLength;
					if(Utils.FGT(swingDoorLength, 0.0))
					{
						foreach (Point shutterCornerPnt in shutterCornerPointsList)
						{
							Point circleCenterPnt = shutterCornerPnt;

							double DeltaX = circleCenterPnt.X - Math.Max(notShutter.TopLeft_GlobalPoint.X, Math.Min(circleCenterPnt.X, notShutter.TopLeft_GlobalPoint.X + notShutter.Length_X));
							double DeltaY = circleCenterPnt.Y - Math.Max(notShutter.TopLeft_GlobalPoint.Y, Math.Min(circleCenterPnt.Y, notShutter.TopLeft_GlobalPoint.Y + notShutter.Length_Y));
							bool isIntersect = (DeltaX * DeltaX + DeltaY * DeltaY) < (swingDoorLength * swingDoorLength);
							if (isIntersect)
								return true;
						}
					}

					return false;
				}
			}

			return true;
		}
		public bool IsIntersectWithRectangle(Point firstPnt, Point secondPnt)
		{
			//
			Point botLeftPnt = firstPnt;
			Point topRightPnt = secondPnt;
			// x-positive direction is right
			if (Utils.FLT(secondPnt.X, firstPnt.X))
			{
				botLeftPnt.X = secondPnt.X;
				topRightPnt.X = firstPnt.X;
			}
			// y-positive direction is down
			if (Utils.FGT(secondPnt.Y, firstPnt.Y))
			{
				botLeftPnt.Y = secondPnt.Y;
				topRightPnt.Y = firstPnt.Y;
			}

			//
			if (Utils.FGE(this.TopLeft_GlobalPoint.X, topRightPnt.X) || Utils.FGE(botLeftPnt.X, this.BottomRight_GlobalPoint.X))
				return false;

			//
			if (Utils.FGE(this.TopLeft_GlobalPoint.Y, botLeftPnt.Y) || Utils.FGE(topRightPnt.Y, this.BottomRight_GlobalPoint.Y))
				return false;

			return true;
		}

		//=============================================================================
		public bool IsInsideArea(double DrawingLength, double DrawingWidth, bool bTryToFixIt)
		{
			bool bRes = true;
			if (Utils.FLT(m_TopLeft_GlobalPoint.X, MarginX)
				|| Utils.FLT(m_TopLeft_GlobalPoint.Y, MarginY)
				|| Utils.FGT(m_TopLeft_GlobalPoint.X + m_Length_X, DrawingLength - MarginX)
				|| Utils.FGT(m_TopLeft_GlobalPoint.Y + m_Length_Y, DrawingWidth - MarginY))
			{
				bRes = false;
			}

			if(!bRes && bTryToFixIt)
			{
				GeometryState oldState = this._GetClonedState();

				if (Utils.FLT(m_TopLeft_GlobalPoint.X, MarginX))
					m_TopLeft_GlobalPoint.X = MarginX;
				if (Utils.FLT(m_TopLeft_GlobalPoint.Y, MarginY))
					m_TopLeft_GlobalPoint.Y = MarginY;
				if (Utils.FGT(m_TopLeft_GlobalPoint.X + m_Length_X, DrawingLength - MarginX))
					m_TopLeft_GlobalPoint.X = DrawingLength - MarginX - m_Length_X;
				if (Utils.FGT(m_TopLeft_GlobalPoint.Y + m_Length_Y, DrawingWidth - MarginY))
					m_TopLeft_GlobalPoint.Y = DrawingWidth - MarginY - m_Length_Y;

				//
				eAppliedChanges appliedChanges = eAppliedChanges.eMoveCenterGripPoint;
				List<BaseRectangleGeometry> overlappedRectangles;
				if (!IsCorrect(appliedChanges, out overlappedRectangles))
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
					while (this._CalculateNotOverlapPosition(overlappedRectangles, GRIP_CENTER, DrawingLength, DrawingWidth, out newTopLeftPoint, out newGlobalLength, out newGlobalWidth))
					{
						m_TopLeft_GlobalPoint = newTopLeftPoint;
						//
						m_Length_X = newGlobalLength;
						m_Length_X = Utils.GetWholeNumberByStep(m_Length_X, StepLength_X);
						m_Length_X = Utils.CheckWholeNumber(m_Length_X, MinLength_X, MaxLength_X);
						//
						m_Length_Y = newGlobalWidth;
						m_Length_Y = Utils.GetWholeNumberByStep(m_Length_Y, StepLength_Y);
						m_Length_Y = Utils.CheckWholeNumber(m_Length_Y, MinLength_Y, MaxLength_Y);

						//
						if (IsCorrect(appliedChanges, out overlappedRectangles))
							break;

						++iLoopCount;
						if (iLoopCount >= iMaxLoopCount)
							break;
					}
				}

				bool bLayoutIsCorrect = IsInsideArea(DrawingLength, DrawingWidth, false);
				if (bLayoutIsCorrect && !IsCorrect(appliedChanges, out overlappedRectangles))
					bLayoutIsCorrect = false;

				if (!bLayoutIsCorrect)
					this._SetState(oldState);

				bRes = bLayoutIsCorrect;
			}

			return bRes;
		}

		//=============================================================================
		public bool _CalculateNotOverlapPosition(
			List<BaseRectangleGeometry> overlappedRectangles,
			int iGripIndex,
			double rGraphicsAreaLength,
			double rGraphicsAreaWidth,
			out Point newTopLeft_GlobalPoint,
			out double newGlobalLength,
			out double newGlobalWidth)
		{
			return _CalculateNotOverlapPosition(
				overlappedRectangles,
				iGripIndex,
				rGraphicsAreaLength,
				rGraphicsAreaWidth,
				false,
				out newTopLeft_GlobalPoint,
				out newGlobalLength,
				out newGlobalWidth);
		}
		public bool _CalculateNotOverlapPosition(
			List<BaseRectangleGeometry> overlappedRectangles,
			int iGripIndex,
			double rGraphicsAreaLength,
			double rGraphicsAreaWidth,
			bool bIgnoreNegativeValues,
			out Point newTopLeft_GlobalPoint,
			out double newGlobalLength,
			out double newGlobalWidth)
		{
			newTopLeft_GlobalPoint = m_TopLeft_GlobalPoint;
			newGlobalLength = m_Length_X;
			newGlobalWidth = m_Length_Y;

			if (GRIP_TOP_LEFT != iGripIndex && GRIP_CENTER == iGripIndex && GRIP_BOTTOM_RIGHT == iGripIndex)
				return false;

			bool bMakeSomeChanges = false;

			// current calculation points
			Point newTopRight_GlobalPoint = newTopLeft_GlobalPoint;
			newTopRight_GlobalPoint.X += newGlobalLength;
			//
			Point newBotLeft_GlobalPoint = newTopLeft_GlobalPoint;
			newBotLeft_GlobalPoint.Y += newGlobalWidth;
			//
			Point newBotRight_GlobalPoint = newTopRight_GlobalPoint;
			newBotRight_GlobalPoint.Y += newGlobalWidth;

			// this rectangle available area
			double left_X = -1;
			double right_X = -1;
			double top_Y = -1;
			double bot_Y = -1;

			//
			for (int i = 0; i < overlappedRectangles.Count; ++i)
			{
				BaseRectangleGeometry ovRect = overlappedRectangles[i];
				//
				double rVertGap = BaseRectangleGeometry.CalculateMargin(false, this, ovRect);
				double rHorizGap = BaseRectangleGeometry.CalculateMargin(true, this, ovRect);

				// check top
				if (newBotLeft_GlobalPoint.Y > ovRect.BottomLeft_GlobalPoint.Y)
				{
					if ((ovRect.TopLeft_GlobalPoint.Y - rVertGap <= newTopLeft_GlobalPoint.Y && newTopLeft_GlobalPoint.Y <= ovRect.BottomLeft_GlobalPoint.Y + rVertGap)
						|| (newTopLeft_GlobalPoint.Y <= ovRect.TopLeft_GlobalPoint.Y && ovRect.BottomLeft_GlobalPoint.Y <= newBotLeft_GlobalPoint.Y))
					{
						if (Utils.FGT(ovRect.BottomLeft_GlobalPoint.Y + rVertGap, top_Y))
						{
							top_Y = ovRect.BottomLeft_GlobalPoint.Y + rVertGap;
						}
					}
				}
				// check bottom
				if (newTopLeft_GlobalPoint.Y < ovRect.TopLeft_GlobalPoint.Y && ovRect.TopLeft_GlobalPoint.Y - newGlobalWidth > 0)
				{
					if ((ovRect.TopLeft_GlobalPoint.Y - rVertGap <= newBotRight_GlobalPoint.Y && newBotRight_GlobalPoint.Y <= ovRect.BottomLeft_GlobalPoint.Y + rVertGap)
						|| (newTopLeft_GlobalPoint.Y <= ovRect.TopLeft_GlobalPoint.Y && ovRect.BottomLeft_GlobalPoint.Y <= newBotLeft_GlobalPoint.Y))
					{
						if (bot_Y < 0 || ovRect.TopLeft_GlobalPoint.Y - rVertGap < bot_Y)
						{
							bot_Y = ovRect.TopLeft_GlobalPoint.Y - rVertGap;
						}
					}
				}
				// check left
				if (newTopRight_GlobalPoint.X > ovRect.TopRight_GlobalPoint.X)
				{
					if ((ovRect.TopLeft_GlobalPoint.X - rHorizGap <= newTopLeft_GlobalPoint.X && newTopLeft_GlobalPoint.X <= ovRect.TopRight_GlobalPoint.X + rHorizGap)
						|| (newTopLeft_GlobalPoint.X <= ovRect.TopLeft_GlobalPoint.X && ovRect.TopRight_GlobalPoint.X <= newTopRight_GlobalPoint.X))
					{
						if (ovRect.TopRight_GlobalPoint.X + rHorizGap > left_X)
						{
							left_X = ovRect.TopRight_GlobalPoint.X + rHorizGap;
						}
					}
				}
				// check right
				if (newTopLeft_GlobalPoint.X < ovRect.TopLeft_GlobalPoint.X)
				{
					if ((ovRect.TopLeft_GlobalPoint.X - rHorizGap <= newBotRight_GlobalPoint.X && newBotRight_GlobalPoint.X <= ovRect.TopRight_GlobalPoint.X + rHorizGap)
						|| (newTopLeft_GlobalPoint.X <= ovRect.TopLeft_GlobalPoint.X && ovRect.TopRight_GlobalPoint.X <= newTopRight_GlobalPoint.X))
					{
						if (right_X < 0 || ovRect.TopLeft_GlobalPoint.X - rHorizGap < right_X)
						{
							right_X = ovRect.TopLeft_GlobalPoint.X - rHorizGap;
						}
					}
				}

				// if top_y, bot_y, left_x, right_x are -1 then ovRect is inside this rectanle
				if (-1 == top_Y
					&& -1 == bot_Y
					&& -1 == left_X
					&& -1 == right_X)
				{
					top_Y = ovRect.BottomLeft_GlobalPoint.Y + rVertGap;
					bot_Y = ovRect.TopLeft_GlobalPoint.Y - rVertGap;
					left_X = ovRect.TopRight_GlobalPoint.X + rHorizGap;
					right_X = ovRect.TopLeft_GlobalPoint.X - rHorizGap;
				}
			}

			//
			if (GRIP_TOP_LEFT == iGripIndex && (left_X > 0 || top_Y > 0))
			{
				newTopLeft_GlobalPoint = BottomRight_GlobalPoint;

				//
				// make the smallest availbale change
				//
				double rDeltaX = -1;
				if (left_X > 0)
					rDeltaX = Math.Abs(TopLeft_GlobalPoint.X - left_X);// + 1;
				//
				double rDeltaY = -1;
				if (top_Y > 0)
					rDeltaY = Math.Abs(TopLeft_GlobalPoint.Y - top_Y);// + 1;

				//
				if (rDeltaX > 0 && (rDeltaY < 0 || rDeltaX < rDeltaY))
				{
					newGlobalLength -= rDeltaX;

					newGlobalLength = Convert.ToInt32(Math.Truncate(newGlobalLength));
					bMakeSomeChanges = true;
				}
				//
				if (!bMakeSomeChanges && rDeltaY > 0)
				{
					newGlobalWidth -= rDeltaY;

					newGlobalWidth = Convert.ToInt32(Math.Truncate(newGlobalWidth));
					bMakeSomeChanges = true;
				}
				// 
				newTopLeft_GlobalPoint.X -= newGlobalLength;
				newTopLeft_GlobalPoint.Y -= newGlobalWidth;
			}
			if (GRIP_BOTTOM_RIGHT == iGripIndex && (right_X > 0 || bot_Y > 0))
			{
				//
				double rDeltaX = -1;
				if (right_X > 0)
					rDeltaX = Math.Abs(BottomRight_GlobalPoint.X - right_X);// + 1;
				//
				double rDeltaY = -1;
				if (bot_Y > 0)
					rDeltaY = Math.Abs(BottomRight_GlobalPoint.Y - bot_Y);// + 1;

				// make the smallest available change
				if (rDeltaX > 0 && (rDeltaY < 0 || rDeltaX < rDeltaY))
				{
					newGlobalLength -= rDeltaX;

					newGlobalLength = Convert.ToInt32(Math.Truncate(newGlobalLength));
					bMakeSomeChanges = true;
				}
				//
				if (!bMakeSomeChanges && rDeltaY > 0)
				{
					newGlobalWidth -= rDeltaY;

					newGlobalWidth = Convert.ToInt32(Math.Truncate(newGlobalWidth));
					bMakeSomeChanges = true;
				}
			}
			if(GRIP_CENTER == iGripIndex)
			{
				// dont change size on GRIP_CENTER

				// make the smallest of available changes
				bool bTopOverlap = Utils.FGE(top_Y, 0.0) && Utils.FLE(top_Y + newGlobalWidth, rGraphicsAreaWidth);
				double rDeltaTop = -1.0;
				if (bTopOverlap)
					rDeltaTop = Math.Abs(newTopLeft_GlobalPoint.Y - top_Y);
				//
				bool bBotOverlap = Utils.FGE(bot_Y, 0.0) && Utils.FGE(bot_Y, newGlobalWidth);
				double rDeltaBot = -1.0;
				if(bBotOverlap)
					rDeltaBot = Math.Abs(newBotLeft_GlobalPoint.Y - bot_Y);
				//
				bool bLeftOverlap = Utils.FGE(left_X, 0.0) && Utils.FLE(left_X + newGlobalLength, rGraphicsAreaLength);
				double rDeltaLeft = -1.0;
				if(bLeftOverlap)
					rDeltaLeft = Math.Abs(newTopLeft_GlobalPoint.X - left_X);
				//
				bool bRightOverlap = Utils.FGE(right_X, 0.0) && Utils.FGE(right_X - newGlobalLength, 0.0);
				double rDeltaRight = -1;
				if(bRightOverlap)
					rDeltaRight = Math.Abs(newTopRight_GlobalPoint.X - right_X);

				//
				eMinDelta minDelta = eMinDelta.eNothing;
				double rDeltaValue = -1.0;
				if (bBotOverlap && (Utils.FLT(rDeltaValue, 0.0) || Utils.FLT(rDeltaBot, rDeltaValue)))
				{
					minDelta = eMinDelta.eBot;
					rDeltaValue = rDeltaBot;
				}
				if (bTopOverlap && (Utils.FLT(rDeltaValue, 0.0) || Utils.FLT(rDeltaTop, rDeltaValue)))
				{
					minDelta = eMinDelta.eTop;
					rDeltaValue = rDeltaTop;
				}
				if (bLeftOverlap && (Utils.FLT(rDeltaValue, 0.0) || Utils.FLT(rDeltaLeft, rDeltaValue)))
				{
					minDelta = eMinDelta.eLeft;
					rDeltaValue = rDeltaLeft;
				}
				if (bRightOverlap && (Utils.FLT(rDeltaValue, 0.0) || Utils.FLT(rDeltaRight, rDeltaValue)))
				{
					minDelta = eMinDelta.eRight;
					rDeltaValue = rDeltaRight;
				}

				if(eMinDelta.eBot == minDelta)
				{
					newTopLeft_GlobalPoint = newBotLeft_GlobalPoint;
					newTopLeft_GlobalPoint.Y = bot_Y - newGlobalWidth;
					bMakeSomeChanges = true;
				}
				else if(eMinDelta.eTop == minDelta)
				{
					newTopLeft_GlobalPoint.Y = top_Y;
					bMakeSomeChanges = true;
				}
				else if(eMinDelta.eLeft == minDelta)
				{
					newTopLeft_GlobalPoint.X = left_X;
					bMakeSomeChanges = true;
				}
				else if(eMinDelta.eRight == minDelta)
				{
					newTopLeft_GlobalPoint.X = right_X - newGlobalLength;
					bMakeSomeChanges = true;
				}
			}

			// recalc borders
			if(bMakeSomeChanges)
			{
				newBotRight_GlobalPoint = newTopLeft_GlobalPoint;
				newBotRight_GlobalPoint.X += newGlobalLength;
				newBotRight_GlobalPoint.Y += newGlobalWidth;
			}

			if (!bIgnoreNegativeValues)
			{
				if (newTopLeft_GlobalPoint.X < 0
					|| newTopLeft_GlobalPoint.Y < 0
					|| newBotRight_GlobalPoint.X > rGraphicsAreaLength
					|| newBotRight_GlobalPoint.Y > rGraphicsAreaWidth)
					return false;
			}

			return bMakeSomeChanges;
		}

		//=============================================================================
		/// <summary>
		/// Notifies property values are changed
		/// </summary>
		public void UpdateProperties()
		{
			if (this.Properties == null)
				return;

			foreach (Property_ViewModel propVM in this.Properties)
			{
				if (propVM == null)
					continue;

				propVM.Update_Value();
			}
		}

		#endregion

		#region Abstract functions

		protected abstract GeometryState _GetOriginalState();
		protected GeometryState _GetClonedState()
		{
			//return Utils.DeepClone<GeometryState>(this._GetOriginalState());
			return this._GetOriginalState();
		}
		protected virtual void _SetState(GeometryState state)
		{
			if (state == null)
				return;

			this.m_TopLeft_GlobalPoint = state.TopLeftPnt;
			//
			this.m_Length_X = state.Length_X;
			this.m_MinLength_X = state.MinLength_X;
			this.m_MaxLength_X = state.MaxLength_X;
			this.m_StepLength_X = state.StepLength_X;
			this.m_MarginX = state.Margin_X;
			//
			this.m_Length_Y = state.Length_Y;
			this.m_MinLength_Y = state.MinLength_Y;
			this.m_MaxLength_Y = state.MaxLength_Y;
			this.m_StepLength_Y = state.StepLength_Y;
			this.m_MarginY = state.Margin_Y;
			//
			this.m_Length_Z = state.Length_Z;
			this.m_MinLength_Z = state.MinLength_Z;
			this.m_MaxLength_Z = state.MaxLength_Z;
			this.m_StepLength_Z = state.StepLength_Z;
			//
			this.m_strText = state.Text;
			this.m_strRectangleName = state.Name;
			this.m_bIsHorizontal = state.IsHorizontal;
			//
			this.m_IsInit = state.IsInit;
			this.m_IsSelected = state.IsSelected;
			this.m_FillColor = state.FillColor;
		}

		#endregion

		#region Private and Protected functions

		//=============================================================================
		protected virtual void _InitProperties()
		{
			m_Properties.Add(new GeometryType_Property(this));

			//
			m_Properties.Add(new GeometryProperty(this, PROP_TOP_LEFT_POINT_X, "X", true, "Top left point"));
			m_Properties.Add(new GeometryProperty(this, PROP_TOP_LEFT_POINT_Y, "Y", true, "Top left point"));

			//
			if (m_bIsHorizontal)
			{
				m_Properties.Add(new GeometryProperty(this, PROP_DIMENSION_X, "Length", true, "Size"));
				m_Properties.Add(new GeometryProperty(this, PROP_DIMENSION_Y, "Depth", true, "Size"));
			}
			else
			{
				m_Properties.Add(new GeometryProperty(this, PROP_DIMENSION_Y, "Length", true, "Size"));
				m_Properties.Add(new GeometryProperty(this, PROP_DIMENSION_X, "Depth", true, "Size"));
			}
			m_Properties.Add(new GeometryProperty(this, PROP_DIMENSION_Z, "Height", _Is_HeightProperty_ReadOnly, true, "Size"));
		}

		//=============================================================================
		public enum eAppliedChanges
		{
			eNothing,
			eMoveCenterGripPoint
		};
		public bool IsCorrect(out List<BaseRectangleGeometry> overlappedRectangles)
		{
			overlappedRectangles = new List<BaseRectangleGeometry>();
			bool bResult = IsCorrect(eAppliedChanges.eNothing, out overlappedRectangles);

			return bResult;
		}
		public bool IsCorrect(eAppliedChanges appliedChanges, out List<BaseRectangleGeometry> overlappedRectangles)
		{
			overlappedRectangles = new List<BaseRectangleGeometry>();

			if (m_Sheet == null)
				return false;

			//
			List<BaseRectangleGeometry> _rectanglesToIgnore = new List<BaseRectangleGeometry>();

			if (eAppliedChanges.eMoveCenterGripPoint == appliedChanges)
			{
				//
				// need to check all racks row, otherwise cant change size of rack in row
				Rack _rack = this as Rack;
				if (_rack != null)
					_rectanglesToIgnore.AddRange(m_Sheet.GetRackGroup(_rack));
			}

			return m_Sheet.IsLayoutCorrect(this, _rectanglesToIgnore, out overlappedRectangles);
		}
		public bool IsCorrect(double DrawingLength, double DrawingWidth, int iGripIndex, bool bUseStep, bool bTryToFix, bool bCanChangeSize = true)
		{
			bool bResult = IsInsideArea(DrawingLength, DrawingWidth, bTryToFix);
			if (!bResult && !bTryToFix)
				return false;

			//
			eAppliedChanges appliedChanges = eAppliedChanges.eNothing;
			// check
			List<BaseRectangleGeometry> overlappedRectangles;
			if (!IsCorrect(appliedChanges, out overlappedRectangles))
			{
				if (!bTryToFix)
					return false;

				// try to fix it
				Point newTopLeftPoint;
				double newGlobalLength;
				double newGlobalWidth;
				//
				// infinity loop protection
				int iMaxLoopCount = 100;
				int iLoopCount = 0;
				//
				while (this._CalculateNotOverlapPosition(overlappedRectangles, iGripIndex, DrawingLength, DrawingWidth, out newTopLeftPoint, out newGlobalLength, out newGlobalWidth))
				{
					m_TopLeft_GlobalPoint = newTopLeftPoint;
					//
					if (bCanChangeSize)
					{
						double length_X_Value = newGlobalLength;
						// Dont change the sizes of rack when user drag it by center point.
						// For example user has 2860 M-rack and drag it to rack row. And while dragging layout is incorrect,
						// so we will go there and change the length of rack from 2860 to 2850 because 2860 is not divisible by 25.
						// But there is no index for the rack with 2850 length and DrawingDocument.GetRackUniqueSizeIndex() returns -1.
						if (bUseStep)
							length_X_Value = Utils.GetWholeNumberByStep(length_X_Value, StepLength_X);
						length_X_Value = Utils.CheckWholeNumber(length_X_Value, MinLength_X, MaxLength_X);
						this.Length_X = length_X_Value;
						//
						double length_Y_Value = newGlobalWidth;
						// Read comment above.
						if (bUseStep)
							length_Y_Value = Utils.GetWholeNumberByStep(length_Y_Value, StepLength_Y);
						length_Y_Value = Utils.CheckWholeNumber(length_Y_Value, MinLength_Y, MaxLength_Y);
						this.Length_Y = length_Y_Value;
					}

					//
					if (IsCorrect(appliedChanges, out overlappedRectangles))
						break;

					++iLoopCount;
					if (iLoopCount >= iMaxLoopCount)
						break;
				}
			}

			bool bLayoutIsCorrect = IsInsideArea(DrawingLength, DrawingWidth, false);
			if (bLayoutIsCorrect && !IsCorrect(appliedChanges, out overlappedRectangles))
				bLayoutIsCorrect = false;

			if (!bLayoutIsCorrect)
				return false;

			return true;
		}


		//=============================================================================
		private void _CheckWholeNumbers()
		{
			// When you move center grip point and width or height is odd number, then
			// top left point will not be whole number.
			// Because top left point is calculated like :
			// newCenterPoint.X - m_Width\2

			// ToplLeftPoint, width and height always should be whole numbers
			m_TopLeft_GlobalPoint.X = Convert.ToInt32(Math.Truncate(m_TopLeft_GlobalPoint.X));
			m_TopLeft_GlobalPoint.Y = Convert.ToInt32(Math.Truncate(m_TopLeft_GlobalPoint.Y));
			//
			m_Length_X = Convert.ToInt32(Math.Truncate(m_Length_X));
			m_Length_Y = Convert.ToInt32(Math.Truncate(m_Length_Y));
		}

		//=============================================================================
		protected bool _SetPropertyValue(string strPropSysName, object value, double DrawingLength, double DrawingWidth, bool bWasChangedViaProperties, out string strError, bool bCheckLayout = true)
		{
			strError = string.Empty;

			if (string.IsNullOrEmpty(strPropSysName))
				return false;

			//
			bool bResult = false;
			int iGripIndex = GRIP_CENTER;
			bool bRevertChanges = false;

			try
			{
				if(PROP_TOP_LEFT_POINT == strPropSysName)
				{
					if (!(value is Point))
						return false;

					Point newGlobalPoint = (Point)value;

					// change size
					Point bottomRightPoint = BottomRight_GlobalPoint;
					Point newTopLeftPoint = Utils.CheckBorders(newGlobalPoint, 0, 0, DrawingLength, DrawingWidth, MarginX, MarginY);
					// calc width 
					m_Length_X = bottomRightPoint.X - newTopLeftPoint.X;
					m_Length_X = Utils.GetWholeNumberByStep(m_Length_X, StepLength_X);
					m_Length_X = Utils.CheckWholeNumber(m_Length_X, MinLength_X, MaxLength_X);
					//
					m_Length_Y = bottomRightPoint.Y - newTopLeftPoint.Y;
					m_Length_Y = Utils.GetWholeNumberByStep(m_Length_Y, StepLength_Y);
					m_Length_Y = Utils.CheckWholeNumber(m_Length_Y, MinLength_Y, MaxLength_Y);
					//
					m_TopLeft_GlobalPoint = bottomRightPoint;
					m_TopLeft_GlobalPoint.X -= m_Length_X;
					m_TopLeft_GlobalPoint.Y -= m_Length_Y;

					//
					bResult = true;
					iGripIndex = GRIP_TOP_LEFT;
				}
				else if(PROP_CENTER_POINT == strPropSysName)
				{
					if (!(value is Point))
						return false;

					Point newGlobalPoint = (Point)value;

					// just drag rectangle
					Point newTopLeft_GlobalPoint = newGlobalPoint;
					newTopLeft_GlobalPoint.X -= m_Length_X / 2;
					newTopLeft_GlobalPoint.Y -= m_Length_Y / 2;

					//
					m_TopLeft_GlobalPoint = newTopLeft_GlobalPoint;

					//
					bResult = true;
					iGripIndex = GRIP_CENTER;
				}
				else if(PROP_BOT_RIGHT_POINT == strPropSysName)
				{
					if (!(value is Point))
						return false;

					Point newGlobalPoint = (Point)value;

					// change the size
					//
					m_Length_X = newGlobalPoint.X - m_TopLeft_GlobalPoint.X;
					// if value was changed via properties and it is not correct, then revert changes
					if(bWasChangedViaProperties)
					{
						if (Utils.FLT(m_Length_X, MinLength_X))
							bRevertChanges = true;
						if (double.IsPositiveInfinity(MaxLength_X))
						{
							if (Utils.FGT(m_Length_X, DrawingLength - MarginX))
								bRevertChanges = true;
						}
						else
						{
							if (Utils.FGT(m_Length_X, MaxLength_X))
								bRevertChanges = true;
						}
					}
					//
					if (!bRevertChanges)
					{
						m_Length_X = Utils.GetWholeNumberByStep(m_Length_X, StepLength_X);
						if (double.IsPositiveInfinity(MaxLength_X))
							m_Length_X = Utils.CheckWholeNumber(m_Length_X, MinLength_X, DrawingLength - MarginX);
						else
							m_Length_X = Utils.CheckWholeNumber(m_Length_X, MinLength_X, MaxLength_X);
					}

					//
					m_Length_Y = newGlobalPoint.Y - m_TopLeft_GlobalPoint.Y;
					// if value was changed via properties and it is not correct, then revert changes
					if (bWasChangedViaProperties)
					{
						if (Utils.FLT(m_Length_Y, MinLength_Y))
							bRevertChanges = true;
						if (double.IsPositiveInfinity(MaxLength_Y))
						{
							if (Utils.FGT(m_Length_Y, DrawingWidth - MarginY))
								bRevertChanges = true;
						}
						else
						{
							if (Utils.FGT(m_Length_Y, MaxLength_Y))
								bRevertChanges = true;
						}
					}
					//
					if (!bRevertChanges)
					{
						m_Length_Y = Utils.GetWholeNumberByStep(m_Length_Y, StepLength_Y);
						if (double.IsPositiveInfinity(MaxLength_Y))
							m_Length_Y = Utils.CheckWholeNumber(m_Length_Y, MinLength_Y, DrawingWidth - MarginY);
						else
							m_Length_Y = Utils.CheckWholeNumber(m_Length_Y, MinLength_Y, MaxLength_Y);
					}

					//
					bResult = !bRevertChanges;
					iGripIndex = GRIP_BOTTOM_RIGHT;
				}
				else if (PROP_DIMENSION_X == strPropSysName)
				{
					int iNewValue = Convert.ToInt32(value);

					// change the size
					//
					m_Length_X = iNewValue;
					// if value was changed via properties and it is not correct, then revert changes
					if (bWasChangedViaProperties)
					{
						if (m_Length_X < MinLength_X)
						{
							bRevertChanges = true;

							strError += "Length is less then min value(";
							strError += MinLength_X.ToString();
							strError += ").";
						}
						if (double.IsPositiveInfinity(MaxLength_X))
						{
							if (Utils.FGT(m_Length_X, DrawingLength - MarginX))
							{
								bRevertChanges = true;

								strError += "Length is bigger then available sheet length(";
								strError += (DrawingLength - MarginX).ToString();
								strError += ").";
							}
						}
						else
						{
							if (Utils.FGT(m_Length_X, MaxLength_X))
							{
								bRevertChanges = true;

								strError += "Length is bigger then max value(";
								strError += MaxLength_X.ToString();
								strError += ").";
							}
						}
					}
					//
					if (!bRevertChanges)
					{
						if(m_Length_X % StepLength_X != 0)
						{
							strError += "Length should be divisible by ";
							strError += StepLength_X.ToString();
							strError += " without remainder.";
						}

						m_Length_X = Utils.GetWholeNumberByStep(m_Length_X, StepLength_X);
						if (double.IsPositiveInfinity(MaxLength_X))
							m_Length_X = Utils.CheckWholeNumber(m_Length_X, MinLength_X, DrawingLength - MarginX);
						else
							m_Length_X = Utils.CheckWholeNumber(m_Length_X, MinLength_X, MaxLength_X);
					}

					//
					bResult = true;
					iGripIndex = GRIP_CENTER;
				}
				else if (PROP_DIMENSION_Y == strPropSysName)
				{
					int iNewValue = Convert.ToInt32(value);

					//
					m_Length_Y = iNewValue;
					// if value was changed via properties and it is not correct, then revert changes
					if (bWasChangedViaProperties)
					{
						if (Utils.FLT(m_Length_Y, MinLength_Y))
						{
							bRevertChanges = true;

							strError += "Depth is less then min value(";
							strError += MinLength_Y.ToString();
							strError += ").";
						}
						if (double.IsPositiveInfinity(MaxLength_Y))
						{
							if (Utils.FGT(m_Length_Y, DrawingWidth - MarginY))
							{
								bRevertChanges = true;

								strError += "Depth is bigger then available sheet depth(";
								strError += (DrawingWidth - MarginY).ToString();
								strError += ").";
							}
						}
						else
						{
							if (Utils.FGT(m_Length_Y, MaxLength_Y))
							{
								bRevertChanges = true;

								strError += "Depth is bigger then max value(";
								strError += MaxLength_Y.ToString();
								strError += ").";
							}
						}
					}
					//
					if (!bRevertChanges)
					{
						if (m_Length_X % StepLength_X != 0)
						{
							strError += "Depth should be divisible by ";
							strError += StepLength_Y.ToString();
							strError += " without remainder.";
						}

						m_Length_Y = Utils.GetWholeNumberByStep(m_Length_Y, StepLength_Y);
						if (double.IsPositiveInfinity(MaxLength_Y))
							m_Length_Y = Utils.CheckWholeNumber(m_Length_Y, MinLength_Y, DrawingWidth);
						else
							m_Length_Y = Utils.CheckWholeNumber(m_Length_Y, MinLength_Y, MaxLength_Y);
					}

					//
					bResult = true;
					iGripIndex = GRIP_CENTER;
				}
				else if (PROP_DIMENSION_Z == strPropSysName)
				{
					m_Length_Z = Convert.ToInt32(value);

					if (m_Length_Z % StepLength_Z != 0)
					{
						strError += "Height should be divisible by ";
						strError += StepLength_Z.ToString();
						strError += " without remainder.";
					}

					m_Length_Z = Utils.GetWholeNumberByStep(m_Length_Z, StepLength_Z);

					if(m_Length_Z < MinLength_Z)
					{
						strError += "Height is less then min value(";
						strError += MinLength_Z.ToString();
						strError += ").";

						// if value was changed via properties and it is not correct, then revert changes
						if (bWasChangedViaProperties)
							return false;
					}
					else if(MaxLength_Z > 0 && m_Length_Z > MaxLength_Z)
					{
						strError += "Height is bigger then max value(";
						strError += MaxLength_Z.ToString();
						strError += ").";

						// if value was changed via properties and it is not correct, then revert changes
						if (bWasChangedViaProperties)
							return false;
					}

					m_Length_Z = Utils.CheckWholeNumber(m_Length_Z, MinLength_Z, MaxLength_Z);

					return true;
				}
				else if (PROP_NAME == strPropSysName)
				{
					string strValue = string.Empty;
					if (value != null)
						strValue = value.ToString();

					m_strRectangleName = strValue;

					return true;
				}

				if (bRevertChanges)
					return false;

				//
				m_TopLeft_GlobalPoint = Utils.CheckBorders(m_TopLeft_GlobalPoint, 0.0, 0.0, DrawingLength, DrawingWidth, MarginX, MarginY);
				if (Utils.FGT(m_TopLeft_GlobalPoint.X + m_Length_X, DrawingLength - MarginX))
					m_TopLeft_GlobalPoint.X = DrawingLength - m_Length_X - MarginX;
				if (Utils.FGT(m_TopLeft_GlobalPoint.Y + m_Length_Y, DrawingWidth - MarginY))
					m_TopLeft_GlobalPoint.Y = DrawingWidth - m_Length_Y - MarginY;

				//
				_CheckWholeNumbers();

				// dont check layout if it is not init
				if (!IsInit)
					return bResult;

				if (!bCheckLayout)
					return bResult;

				bResult = this.IsCorrect(DrawingLength, DrawingWidth, iGripIndex, PROP_CENTER_POINT != strPropSysName, true);
			}
			catch { }

			if(bResult)
				_CheckWholeNumbers();

			return bResult;
		}

		//=============================================================================
		protected virtual void _MarkStateChanged()
		{
			if (this.Sheet == null)
				return;
			long sheetUpdateNumber = this.Sheet.UpdateCount;
			if (m_SheetUpdateNumber <= sheetUpdateNumber)
				m_SheetUpdateNumber = sheetUpdateNumber + 1;
		}

		//=============================================================================
		/// <summary>
		/// Rotates geometry by 90 degrees.
		/// Rotate applies to MinLength_X(Y), StepLength_X(Y), Length_X(Y).
		/// </summary>
		/// <param name="rotatePnt">Point which should keep its position during rotate</param>
		/// <param name="bCheckLayout">Check - does geometry after rotate overlaps any other geometry or goes out the drawing area.</param>
		/// <param name="bTryToFixLayout">If result layout is not correct after rotate, then try to fix layout by move rotated geometry.</param>
		private bool _RotateGeometry(double DrawingLength, double DrawingWidth, eRotateRelativePoint rotatePnt, bool bCheckLayout, bool bTryToFixLayout)
		{
			// Set marginx, min and max values before change m_bIsHorizontal, because rectangle can use m_bIsHorizontal to calc this values.
			// For example of this kind of rectangles - Rack.
			Point oldCenterPoint = Center_GlobalPoint;
			//
			double rOldMarginX = MarginX;
			MarginX = MarginY;
			MarginY = rOldMarginX;
			//
			double rVal = MinLength_X;
			MinLength_X = MinLength_Y;
			MinLength_Y = rVal;
			//
			rVal = MaxLength_X;
			MaxLength_X = MaxLength_Y;
			MaxLength_Y = rVal;
			//
			rVal = StepLength_X;
			StepLength_X = StepLength_Y;
			StepLength_Y = rVal;

			m_bIsHorizontal = !m_bIsHorizontal;

			double rOldWidth = m_Length_Y;
			m_Length_Y = m_Length_X;
			m_Length_X = rOldWidth;

			//
			if (eRotateRelativePoint.eCenter == rotatePnt)
			{
				m_TopLeft_GlobalPoint = oldCenterPoint;
				m_TopLeft_GlobalPoint.X -= m_Length_X / 2;
				m_TopLeft_GlobalPoint.Y -= m_Length_Y / 2;
			}

			//
			if (Utils.FGT(m_TopLeft_GlobalPoint.X + m_Length_X, DrawingLength - MarginX))
			{
				if (!bTryToFixLayout)
					return false;
				m_TopLeft_GlobalPoint.X = DrawingLength - m_Length_X - MarginX;
			}
			if (Utils.FGT(m_TopLeft_GlobalPoint.Y + m_Length_Y, DrawingWidth - MarginY))
			{
				if (!bTryToFixLayout)
					return false;
				m_TopLeft_GlobalPoint.Y = DrawingWidth - m_Length_Y - MarginY;
			}

			_CheckWholeNumbers();

			if (m_Properties != null)
			{
				m_Properties.Clear();
				_InitProperties();
			}

			// dont check layout if it is not init
			if (!IsInit)
				return true;

			if (!bCheckLayout)
				return true;

			IsInsideArea(DrawingLength, DrawingWidth, true);

			//
			eAppliedChanges appliedChanges = eAppliedChanges.eMoveCenterGripPoint;
			List<BaseRectangleGeometry> overlappedRectangles;
			if (!IsCorrect(appliedChanges, out overlappedRectangles))
			{
				if (!bTryToFixLayout)
					return false;

				// try to fix it
				Point newTopLeftPoint;
				double newGlobalLength;
				double newGlobalWidth;
				//
				// infinity loop protection
				int iMaxLoopCount = 100;
				int iLoopCount = 0;
				//
				while (this._CalculateNotOverlapPosition(overlappedRectangles, GRIP_CENTER, DrawingLength, DrawingWidth, out newTopLeftPoint, out newGlobalLength, out newGlobalWidth))
				{
					m_TopLeft_GlobalPoint = newTopLeftPoint;
					// dont change the size of the rack, only top left point

					//
					if (IsCorrect(appliedChanges, out overlappedRectangles))
						break;

					++iLoopCount;
					if (iLoopCount >= iMaxLoopCount)
						break;
				}
			}

			bool isLayoutCorrect = IsInsideArea(DrawingLength, DrawingWidth, false);
			if (isLayoutCorrect && !IsCorrect(appliedChanges, out overlappedRectangles))
				isLayoutCorrect = false;

			return isLayoutCorrect;
		}

		//=============================================================================
		/// <summary>
		/// Returns screen(control) local point based on the global point.
		/// </summary>
		protected Point GetLocalPoint(ICoordinateSystem cs, Point globalPoint)
		{
			Point defaultPoint = new Point(0.0, 0.0);
			if (cs == null)
				return defaultPoint;

			if (Sheet == null)
				return defaultPoint;

			return cs.GetLocalPoint(globalPoint, Sheet.UnitsPerCameraPixel, Sheet.GetCameraOffset());
		}
		//=============================================================================
		/// <summary>
		/// Returns screen(control) local distance in pixels based on the global drawing distance
		/// </summary>
		protected double GetWidthInPixels(ICoordinateSystem cs, double globalWidthValue)
		{
			double defaultValue = 0.0;
			if (cs == null)
				return defaultValue;

			if (Sheet == null)
				return defaultValue;

			return cs.GetWidthInPixels(globalWidthValue, Sheet.UnitsPerCameraPixel);
		}
		//=============================================================================
		/// <summary>
		/// Returns screen(control) local distance in pixels based on the global drawing distance
		/// </summary>
		protected double GetHeightInPixels(ICoordinateSystem cs, double globalHeightValue)
		{
			double defaultValue = 0.0;
			if (cs == null)
				return defaultValue;

			if (Sheet == null)
				return defaultValue;

			return cs.GetHeightInPixels(globalHeightValue, Sheet.UnitsPerCameraPixel);
		}

		#endregion


		#region Serialization

		//=============================================================================
		// 3.2 Add IsSelected
		protected static string _sBaseGeom_strMajor = "BaseGeom_MAJOR";
		protected static int _sBaseGeom_MAJOR = 3;
		protected static string _sBaseGeom_strMinor = "BaseGeom_MINOR";
		protected static int _sBaseGeom_MINOR = 2;

		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sBaseGeom_strMajor, _sBaseGeom_MAJOR);
			info.AddValue(_sBaseGeom_strMinor, _sBaseGeom_MINOR);

			// dont serialize\deserialize m_Sheet
			// There is infinitive loop - Sheet.OnDeserialization call serialize\deserialize for all his rectangles for get copy of state.
			//info.AddValue("Sheet", m_Sheet);

			// geom properties
			info.AddValue("TopLeft_GlobalPoint", m_TopLeft_GlobalPoint);
			//
			info.AddValue("Length_X", m_Length_X);
			info.AddValue("Min_GlobalLength", m_MinLength_X);
			info.AddValue("Max_GlobalLength", m_MaxLength_X);
			info.AddValue("StepLength_X", m_StepLength_X);
			//
			info.AddValue("Length_Y", m_Length_Y);
			info.AddValue("Min_GlobalWidth", m_MinLength_Y);
			info.AddValue("Max_GlobalWidth", m_MaxLength_Y);
			info.AddValue("StepLength_Y", m_StepLength_Y);
			//
			info.AddValue("Length_Z", m_Length_Z);
			info.AddValue("Min_GlobalHeight", m_MinLength_Z);
			info.AddValue("Max_GlobalHeight", m_MaxLength_Z);
			info.AddValue("StepLength_Z", m_StepLength_Z);

			// text
			info.AddValue("Text", m_strText);
			info.AddValue("RectangleName", m_strRectangleName);
			info.AddValue("IsHorizontal", m_bIsHorizontal);

			//
			info.AddValue("IsInit", m_IsInit);
			// CanMove is removed, but write some value for previous versions.
			info.AddValue("CanMove", true);

			// Colors
			info.AddValue("FillColor", m_FillColor.ToString());

			// 3.1
			info.AddValue("MarginX", m_MarginX);
			info.AddValue("MarginY", m_MarginY);

			// 3.2
			info.AddValue("IsSelected", m_IsSelected);
		}

		//=============================================================================
		public BaseRectangleGeometry(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sBaseGeom_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sBaseGeom_strMinor, typeof(int));
			if (iMajor > _sBaseGeom_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sBaseGeom_MAJOR && iMinor > _sBaseGeom_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sBaseGeom_MAJOR)
			{
				// restore
				try
				{
					//m_Sheet = (DrawingSheet)info.GetValue("Sheet", typeof(DrawingSheet));

					m_TopLeft_GlobalPoint = (Point)info.GetValue("TopLeft_GlobalPoint", typeof(Point));
					//
					m_Length_X = (double)info.GetValue("Length_X", typeof(double));
					m_MinLength_X = (double)info.GetValue("Min_GlobalLength", typeof(double));
					m_MaxLength_X = (double)info.GetValue("Max_GlobalLength", typeof(double));
					m_StepLength_X = (double)info.GetValue("StepLength_X", typeof(double));
					//
					m_Length_Y = (double)info.GetValue("Length_Y", typeof(double));
					m_MinLength_Y = (double)info.GetValue("Min_GlobalWidth", typeof(double));
					m_MaxLength_Y = (double)info.GetValue("Max_GlobalWidth", typeof(double));
					m_StepLength_Y = (double)info.GetValue("StepLength_Y", typeof(double));
					//
					m_Length_Z = (int)info.GetValue("Length_Z", typeof(int));
					m_MinLength_Z = (int)info.GetValue("Min_GlobalHeight", typeof(int));
					m_MaxLength_Z = (int)info.GetValue("Max_GlobalHeight", typeof(int));
					m_StepLength_Z = (int)info.GetValue("StepLength_Z", typeof(int));

					//
					m_strText = (string)info.GetValue("Text", typeof(string));
					m_strRectangleName = (string)info.GetValue("RectangleName", typeof(string));
					m_bIsHorizontal = (bool)info.GetValue("IsHorizontal", typeof(bool));

					//
					m_IsInit = (bool)info.GetValue("IsInit", typeof(bool));

					//
					try
					{
						m_FillColor = (Color)ColorConverter.ConvertFromString((string)info.GetValue("FillColor", typeof(string)));
					}
					catch
					{
						m_FillColor = Colors.LightGray;
					}

					//
					if (iMajor >= 3 && iMinor >= 1)
					{
						m_MarginX = (double)info.GetValue("MarginX", typeof(double));
						m_MarginY = (double)info.GetValue("MarginY", typeof(double));
					}

					if (iMajor >= 3 && iMinor >= 2)
						m_IsSelected = (bool)info.GetValue("IsSelected", typeof(bool));
					else
						m_IsSelected = false;
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
		public virtual void OnDeserialization(object sender) { }

		#endregion

		#region IClonable

		/// <summary>
		/// Returns deep copy of this object.
		/// </summary>
		public virtual IClonable Clone()
		{
			// Get current geometry state
			GeometryState geomState = this._GetOriginalState();
			// Create new geometry instance
			BaseRectangleGeometry newGeomInstance = this.CreateInstance();
			// Set state to the new geometry instance
			newGeomInstance._SetState(geomState);

			return newGeomInstance;
		}
		//
		protected abstract BaseRectangleGeometry CreateInstance();

		#endregion

		//=============================================================================
		/// <summary>
		/// Calculates margin between two geometry in X or Y directions.
		/// Geometry has MarginX\MarginY values, but sometimes they should not work and
		/// other margin rules should be applied.
		/// </summary>
		public static double CalculateMargin(bool bMarginX, BaseRectangleGeometry geom01, BaseRectangleGeometry geom02)
		{
			if (geom01 == null)
				return 0.0;

			if (geom02 == null)
				return 0.0;

			if (geom01.Sheet == null)
				return 0.0;
			if (geom01.Sheet.Document == null)
				return 0.0;

			// aisle space can should be placed with no margin to the rack
			if ((geom01 is Rack && geom02 is AisleSpace) || (geom01 is AisleSpace && geom02 is Rack))
				return 0.0;

			// min distance between rack and block is 100 in both X and Y directions
			if((geom01 is Rack && geom02 is Block)
				|| (geom02 is Rack && geom01 is Block))
			{
				return DrawingDocument.RACK_TO_BLOCK_MARGIN;
			}

			// min distance between rack and wall is 200 in both X and Y directions
			if ((geom01 is Rack && geom02 is Wall)
				|| (geom02 is Rack && geom01 is Wall))
			{
				return DrawingDocument.RACK_TO_WALL_MARGIN;
			}

			// for 2 racks with the same rotation(vertical\horizontal) use BTB-distance as a margin
			if (geom01 is Rack && geom02 is Rack && geom01.IsHorizontal == geom02.IsHorizontal)
			{
				// if geom is horizontal and asking for Y-margin
				// or
				// geom is vertical and asking for X-margin
				if((geom01.IsHorizontal && !bMarginX) || (!geom01.IsHorizontal && bMarginX))
				{
					// If pallet type is Flush then margin between the same rotated racks in depth direction is DrawingDocument.RACK_FLUSH_PALLETS_BTB_DISTANCE.
					// If Overhang then margin = 2*overhang + DrawingDocument.RACK_OVERHANG_PALLETS_BTB_DISTANCE_ADDITIONALVALUE.
					double sameRotatedRacksMargin = 0.0;
					if (ePalletType.eFlush == geom01.Sheet.Document.RacksPalletType)
					{
						sameRotatedRacksMargin = DrawingDocument.RACK_FLUSH_PALLETS_BTB_DISTANCE;
					}
					else
					{
						sameRotatedRacksMargin = DrawingDocument.RACK_OVERHANG_PALLETS_BTB_DISTANCE_ADDITIONALVALUE;
						if (bMarginX)
							sameRotatedRacksMargin += geom01.MarginX + geom02.MarginX;
						else
							sameRotatedRacksMargin += geom01.MarginY + geom02.MarginY;
					}


					return sameRotatedRacksMargin;
				}
			}

			// dont take max of margins, need to take sum of margins
			double margin = 0.0;
			if(bMarginX)
				margin = geom01.MarginX + geom02.MarginX;
			else
				margin = geom01.MarginY + geom02.MarginY;

			return margin;
		}
	}
}
