using AppInterfaces;
using System;
using System.Runtime.Serialization;
using System.Windows;

namespace DrawingControl
{
	/// <summary>
	/// Roof type for DrawingSheet layout.
	/// It drives max Z height of the geometry.
	/// </summary>
	[Serializable]
	public abstract class Roof : BaseViewModel, ISerializable, IDeserializationCallback, IClonable
	{
		public Roof() { }
		public Roof(Roof roof)
		{
			if(roof != null)
			{
				this.m_bIsSelected = roof.m_bIsSelected;
			}
		}

		#region Properties

		//=============================================================================
		// Name of the roof, which can be displayed for user.
		public abstract string DisplayName { get; }

		//=============================================================================
		private bool m_bIsSelected = false;
		public bool IsSelected
		{
			get { return m_bIsSelected; }
			set
			{
				if(value != m_bIsSelected)
				{
					m_bIsSelected = value;
					NotifyPropertyChanged(() => IsSelected);
				}
			}
		}

		#endregion

		//=============================================================================
		// Returns max available height for the geometry based on it's position and roof type.
		public abstract double CalculateMaxHeightForGeometry(BaseRectangleGeometry geom);

		//=============================================================================
		// Returns max available height for the 2D point based on it's position and roof type.
		public abstract double CalculateMaxHeightForPoint(Point pnt, double areaLength, double areaWidth);

		//=============================================================================
		public virtual IClonable Clone() { return null; }

		#region Serialization

		protected static string _sRoof_strMajor = "Roof_MAJOR";
		protected static int _sRoof_MAJOR = 1;
		protected static string _sRoof_strMinor = "Roof_MINOR";
		protected static int _sRoof_MINOR = 0;

		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sRoof_strMajor, _sRoof_MAJOR);
			info.AddValue(_sRoof_strMinor, _sRoof_MINOR);

			// 1.0
			info.AddValue("IsSelected", m_bIsSelected);
		}

		//=============================================================================
		public Roof(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sRoof_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sRoof_strMinor, typeof(int));
			if (iMajor > _sRoof_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sRoof_MAJOR && iMinor > _sRoof_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sRoof_MAJOR)
			{
				// restore
				try
				{
					if (iMajor >= 1 && iMinor >= 0)
						m_bIsSelected = (bool)info.GetValue("IsSelected", typeof(bool));
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
	}

	[Serializable]
	public class FlatRoof : Roof, ISerializable
	{
		public FlatRoof() { }
		public FlatRoof(FlatRoof roof)
			: base(roof)
		{
			if(roof != null)
			{
				m_Height = roof.m_Height;
			}
		}

		#region Properties

		//=============================================================================
		public override string DisplayName { get { return "Flat"; } }

		//=============================================================================
		private double m_Height = 12000;
		public double Height
		{
			get { return m_Height; }
			set
			{
				if(!double.IsNaN(value)
					&& Utils.FGT(value, 0.0)
					&& value != m_Height)
				{
					m_Height = value;
				}

				NotifyPropertyChanged(() => Height);
			}
		}

		#endregion

		//=============================================================================
		public override double CalculateMaxHeightForGeometry(BaseRectangleGeometry geom)
		{
			// just return roof height becuase it is flat
			return this.Height;
		}

		//=============================================================================
		public override double CalculateMaxHeightForPoint(Point pnt, double areaLength, double areaWidth)
		{
			return this.Height;
		}

		//=============================================================================
		public override IClonable Clone()
		{
			return new FlatRoof(this);
		}

		#region Serialization

		//=============================================================================
		protected static string _sFlatRoof_strMajor = "FlatRoof_MAJOR";
		protected static int _sFlatRoof_MAJOR = 1;
		protected static string _sFlatRoof_strMinor = "FlatRoof_MINOR";
		protected static int _sFlatRoof_MINOR = 0;
		//=============================================================================
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			//
			info.AddValue(_sFlatRoof_strMajor, _sFlatRoof_MAJOR);
			info.AddValue(_sFlatRoof_strMinor, _sFlatRoof_MINOR);

			// 1.0
			info.AddValue("Height", m_Height);
		}
		//=============================================================================
		public FlatRoof(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sFlatRoof_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sFlatRoof_strMinor, typeof(int));
			if (iMajor > _sFlatRoof_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sFlatRoof_MAJOR && iMinor > _sFlatRoof_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sFlatRoof_MAJOR)
			{
				try
				{
					if (iMajor >= 1 && iMinor >= 0)
						m_Height = (double)info.GetValue("Height", typeof(double));
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
		public override void OnDeserialization(object sender)
		{
			base.OnDeserialization(sender);
		}

		#endregion
	}

	[Serializable]
	public class GableRoof : Roof, ISerializable
	{
		public GableRoof() { }
		public GableRoof(GableRoof roof)
			: base(roof)
		{
			if(roof != null)
			{
				m_MinHeight = roof.m_MinHeight;
				m_MaxHeight = roof.m_MaxHeight;
				m_bHorizontalRidgeDirection = roof.m_bHorizontalRidgeDirection;
			}
		}

		#region Properties

		//=============================================================================
		public override string DisplayName { get { return "Gable"; } }

		//=============================================================================
		// Height at the wall placement.
		private double m_MinHeight = 12000;
		public double MinHeight
		{
			get { return m_MinHeight; }
			set
			{
				if (!double.IsNaN(value)
					&& Utils.FGT(value, 0.0)
					&& value != m_MinHeight)
				{
					m_MinHeight = value;

					if (Utils.FGT(m_MinHeight, m_MaxHeight))
						MaxHeight = m_MinHeight;
				}

				NotifyPropertyChanged(() => MinHeight);
			}
		}

		//=============================================================================
		// Height at the center of roof.
		private double m_MaxHeight = 15000;
		public double MaxHeight
		{
			get { return m_MaxHeight; }
			set
			{
				if (!double.IsNaN(value)
					&& Utils.FGT(value, 0.0)
					&& value != m_MaxHeight)
				{
					m_MaxHeight = value;

					if (Utils.FLT(m_MaxHeight, m_MinHeight))
						MinHeight = m_MaxHeight;
				}

				NotifyPropertyChanged(() => MaxHeight);
			}
		}

		//=============================================================================
		// If it is TRUE then ridge direction is X-axis, it means the biggest height is placed at the middle of the layout height.
		// Otherwise direction is Y-axis and it means that the bigest height is placed at the middle of the layout length.
		private bool m_bHorizontalRidgeDirection = true;
		public bool HorizontalRidgeDirection
		{
			get { return m_bHorizontalRidgeDirection; }
			set
			{
				if(value != m_bHorizontalRidgeDirection)
				{
					m_bHorizontalRidgeDirection = value;
					NotifyPropertyChanged(() => HorizontalRidgeDirection);
				}
			}
		}

		#endregion

		//=============================================================================
		public override double CalculateMaxHeightForGeometry(BaseRectangleGeometry geom)
		{
			if (geom == null)
				return MinHeight;

			if (geom.Sheet == null)
				return MinHeight;

			// get height at the geometry points and return the lowest height
			double topLeftHeight = GetHeightInPoint(geom.TopLeft_GlobalPoint, geom.Sheet.Length, geom.Sheet.Width);
			double topRightHeight = GetHeightInPoint(geom.TopRight_GlobalPoint, geom.Sheet.Length, geom.Sheet.Width);
			double botLeftHeight = GetHeightInPoint(geom.BottomLeft_GlobalPoint, geom.Sheet.Length, geom.Sheet.Width);
			double botRightHeight = GetHeightInPoint(geom.BottomRight_GlobalPoint, geom.Sheet.Length, geom.Sheet.Width);

			if (double.IsNaN(topLeftHeight) || double.IsInfinity(topLeftHeight)
				|| double.IsNaN(topRightHeight) || double.IsInfinity(topRightHeight)
				|| double.IsNaN(botLeftHeight) || double.IsInfinity(botLeftHeight)
				|| double.IsNaN(botRightHeight) || double.IsInfinity(botRightHeight)
				)
			{
				return MinHeight;
			}

			return Math.Min(topLeftHeight,
				Math.Min(topRightHeight,
				Math.Min(botLeftHeight, botRightHeight)));
		}

		//=============================================================================
		public override double CalculateMaxHeightForPoint(Point pnt, double areaLength, double areaWidth)
		{
			return GetHeightInPoint(pnt, areaLength, areaWidth);
		}

		//=============================================================================
		private double GetHeightInPoint(Point pnt, double drawingLengthX, double drawingLengthY)
		{
			if (double.IsNaN(pnt.X) || double.IsNaN(pnt.Y) || double.IsNaN(drawingLengthX) || double.IsNaN(drawingLengthY))
				return MinHeight;

			double heightDelta = MaxHeight - MinHeight;
			if (Utils.FLE(heightDelta, 0.0))
				return MinHeight;

			// point projection on the pitch
			double pntProjection = 0.0;
			double ridgeProjection = 0.0;
			if (HorizontalRidgeDirection)
			{
				pntProjection = pnt.Y;
				ridgeProjection = drawingLengthY / 2;
			}
			else
			{
				pntProjection = pnt.X;
				ridgeProjection = drawingLengthX / 2;
			}

			try
			{
				// changing the height value when move away from ridge
				double increment = heightDelta / ridgeProjection;

				double resultHeight = MaxHeight;
				resultHeight -= Math.Abs(pntProjection - ridgeProjection) * increment;

				return resultHeight;
			}
			catch { }

			return MinHeight;
		}

		//=============================================================================
		public override IClonable Clone()
		{
			return new GableRoof(this);
		}

		#region Serialization

		//=============================================================================
		protected static string _sGableRoof_strMajor = "GableRoof_MAJOR";
		protected static int _sGableRoof_MAJOR = 1;
		protected static string _sGableRoof_strMinor = "GableRoof_MINOR";
		protected static int _sGableRoof_MINOR = 0;
		//=============================================================================
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			//
			info.AddValue(_sGableRoof_strMajor, _sGableRoof_MAJOR);
			info.AddValue(_sGableRoof_strMinor, _sGableRoof_MINOR);

			// 1.0
			info.AddValue("MinHeight", m_MinHeight);
			info.AddValue("MaxHeight", m_MaxHeight);
			info.AddValue("HorizontalRidgeDirection", m_bHorizontalRidgeDirection);
		}
		//=============================================================================
		public GableRoof(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sGableRoof_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sGableRoof_strMinor, typeof(int));
			if (iMajor > _sGableRoof_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sGableRoof_MAJOR && iMinor > _sGableRoof_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sGableRoof_MAJOR)
			{
				try
				{
					if (iMajor >= 1 && iMinor >= 0)
					{
						m_MinHeight = (double)info.GetValue("MinHeight", typeof(double));
						m_MaxHeight = (double)info.GetValue("MaxHeight", typeof(double));
						m_bHorizontalRidgeDirection = (bool)info.GetValue("HorizontalRidgeDirection", typeof(bool));
					}
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
		public override void OnDeserialization(object sender)
		{
			base.OnDeserialization(sender);
		}

		#endregion
	}

	[Serializable]
	public class ShedRoof : Roof, ISerializable
	{
		public ShedRoof() { }
		public ShedRoof(ShedRoof roof)
			: base(roof)
		{
			if(roof != null)
			{
				m_MinHeight = roof.m_MinHeight;
				m_MaxHeight = roof.m_MaxHeight;
				m_PitchDirection = roof.m_PitchDirection;
			}
		}

		#region Properties

		//=============================================================================
		public override string DisplayName { get { return "Shed"; } }

		//=============================================================================
		private double m_MinHeight = 12000;
		public double MinHeight
		{
			get { return m_MinHeight; }
			set
			{
				if (!double.IsNaN(value)
					&& Utils.FGT(value, 0.0)
					&& value != m_MinHeight)
				{
					m_MinHeight = value;

					if (Utils.FGT(m_MinHeight, m_MaxHeight))
						MaxHeight = m_MinHeight;
				}

				NotifyPropertyChanged(() => MinHeight);
			}
		}

		//=============================================================================
		private double m_MaxHeight = 15000;
		public double MaxHeight
		{
			get { return m_MaxHeight; }
			set
			{
				if (!double.IsNaN(value)
					&& Utils.FGT(value, 0.0)
					&& value != m_MaxHeight)
				{
					m_MaxHeight = value;

					if (Utils.FLT(m_MaxHeight, m_MinHeight))
						MinHeight = m_MaxHeight;
				}

				NotifyPropertyChanged(() => MaxHeight);
			}
		}

		//=============================================================================
		public enum ePitchDirection : int
		{
			// Max height is left side
			eLeftToRight,
			// Max height is right side
			eRightToLeft,
			// Max height is top side
			eTopToBot,
			// Max height is bot side
			eBotToTop
		}
		private ePitchDirection m_PitchDirection = ePitchDirection.eLeftToRight;
		public ePitchDirection PitchDirection
		{
			get { return m_PitchDirection; }
			set
			{
				if (value != m_PitchDirection)
				{
					m_PitchDirection = value;
					NotifyPropertyChanged(() => m_PitchDirection);
				}
			}
		}

		#endregion

		//=============================================================================
		public override double CalculateMaxHeightForGeometry(BaseRectangleGeometry geom)
		{
			if (geom == null)
				return MinHeight;

			if (geom.Sheet == null)
				return MinHeight;

			// get height at the geometry points and return the lowest height
			double topLeftHeight = GetHeightInPoint(geom.TopLeft_GlobalPoint, geom.Sheet.Length, geom.Sheet.Width);
			double topRightHeight = GetHeightInPoint(geom.TopRight_GlobalPoint, geom.Sheet.Length, geom.Sheet.Width);
			double botLeftHeight = GetHeightInPoint(geom.BottomLeft_GlobalPoint, geom.Sheet.Length, geom.Sheet.Width);
			double botRightHeight = GetHeightInPoint(geom.BottomRight_GlobalPoint, geom.Sheet.Length, geom.Sheet.Width);

			if(double.IsNaN(topLeftHeight) || double.IsInfinity(topLeftHeight)
				|| double.IsNaN(topRightHeight) || double.IsInfinity(topRightHeight)
				|| double.IsNaN(botLeftHeight) || double.IsInfinity(botLeftHeight)
				|| double.IsNaN(botRightHeight) || double.IsInfinity(botRightHeight)
				)
			{
				return MinHeight;
			}

			return Math.Min(topLeftHeight,
				Math.Min(topRightHeight,
				Math.Min(botLeftHeight, botRightHeight)));
		}

		//=============================================================================
		public override double CalculateMaxHeightForPoint(Point pnt, double areaLength, double areaWidth)
		{
			return GetHeightInPoint(pnt, areaLength, areaWidth);
		}


		private double GetHeightInPoint(Point pnt, double drawingLengthX, double drawingLengthY)
		{
			if (double.IsNaN(pnt.X) || double.IsNaN(pnt.Y) || double.IsNaN(drawingLengthX) || double.IsNaN(drawingLengthY))
				return MinHeight;

			double heightDelta = MaxHeight - MinHeight;
			if (Utils.FLE(heightDelta, 0.0))
				return MinHeight;

			// point projection on the pitch
			double pntProjection = 0.0;
			double ridgeProjection = 0.0;
			double ridgeLength = 0.0;
			if (ePitchDirection.eLeftToRight == PitchDirection || ePitchDirection.eRightToLeft == PitchDirection)
			{
				ridgeLength = drawingLengthX;
				pntProjection = pnt.X;
				if (ePitchDirection.eLeftToRight == PitchDirection)
					ridgeProjection = 0.0;
				else if (ePitchDirection.eRightToLeft == PitchDirection)
					ridgeProjection = drawingLengthX;
			}
			else if(ePitchDirection.eBotToTop == PitchDirection || ePitchDirection.eTopToBot == PitchDirection)
			{
				ridgeLength = drawingLengthY;
				pntProjection = pnt.Y;
				if (ePitchDirection.eTopToBot == PitchDirection)
					ridgeProjection = 0.0;
				else if (ePitchDirection.eBotToTop == PitchDirection)
					ridgeProjection = drawingLengthY;
			}

			try
			{
				// changing the height value when move away from ridge
				double increment = heightDelta / ridgeLength;

				double resultHeight = MaxHeight;
				resultHeight -= Math.Abs(pntProjection - ridgeProjection) * increment;

				return resultHeight;
			}
			catch { }

			return MinHeight;
		}

		//=============================================================================
		public override IClonable Clone()
		{
			return new ShedRoof(this);
		}

		#region Serialization

		//=============================================================================
		protected static string _sShedRoof_strMajor = "ShedRoof_MAJOR";
		protected static int _sShedRoof_MAJOR = 1;
		protected static string _sShedRoof_strMinor = "ShedRoof_MINOR";
		protected static int _sShedRoof_MINOR = 0;
		//=============================================================================
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			//
			info.AddValue(_sShedRoof_strMajor, _sShedRoof_MAJOR);
			info.AddValue(_sShedRoof_strMinor, _sShedRoof_MINOR);

			// 1.0
			info.AddValue("MinHeight", m_MinHeight);
			info.AddValue("MaxHeight", m_MaxHeight);
			info.AddValue("PitchDirection", m_PitchDirection);
		}
		//=============================================================================
		public ShedRoof(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sShedRoof_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sShedRoof_strMinor, typeof(int));
			if (iMajor > _sShedRoof_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sShedRoof_MAJOR && iMinor > _sShedRoof_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sShedRoof_MAJOR)
			{
				try
				{
					if (iMajor >= 1 && iMinor >= 0)
					{
						m_MinHeight = (double)info.GetValue("MinHeight", typeof(double));
						m_MaxHeight = (double)info.GetValue("MaxHeight", typeof(double));
						m_PitchDirection = (ePitchDirection)info.GetValue("PitchDirection", typeof(ePitchDirection));
					}
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
		public override void OnDeserialization(object sender)
		{
			base.OnDeserialization(sender);
		}

		#endregion
	}
}
