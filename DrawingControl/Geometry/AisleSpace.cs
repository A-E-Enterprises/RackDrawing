using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public class AisleSpace_State : GeometryState
	{
		public AisleSpace_State(AisleSpace aisleSpace)
			: base(aisleSpace) { }
		public AisleSpace_State(AisleSpace_State state)
			: base(state) { }

		//=============================================================================
		protected override GeometryState MakeDeepCopy()
		{
			return new AisleSpace_State(this);
		}
	}

	[Serializable]
	public class AisleSpace : BaseRectangleGeometry, ISerializable
	{
		public AisleSpace(DrawingSheet ds)
			: base(ds)
		{
			//
			m_FillColor = Colors.Khaki;

			//
			MinLength_X = 100;
			MinLength_Y = 100;

			//
			MaxLength_Y = double.PositiveInfinity;
			MaxLength_X = double.PositiveInfinity;

			//
			StepLength_X = 5;
			StepLength_Y = 5;

			// calculate LengthX and LengthY based on the MHE travel width options
			Length_X = Utils.CheckWholeNumber(Length_X, MinLength_X, MaxLength_X);
			Length_Y = Utils.CheckWholeNumber(Length_Y, MinLength_Y, MaxLength_Y);

			//
			Name = "Aisle space";
			OnSizeChanged();
		}

		#region Properties

		//=============================================================================
		// Minimum length depends on the enabled MHE configuration (document property).
		public override double MinLength_X
		{
			get
			{
				double minMHETravelWidth = _GetMinAisleWidth();
				if (Utils.FGT(minMHETravelWidth, 0.0))
					return minMHETravelWidth;

				return base.MinLength_X;
			}
			set
			{
				base.MinLength_X = value;
			}
		}

		//=============================================================================
		// Minimum length depends on the enabled MHE configuration (document property).
		public override double MinLength_Y
		{
			get
			{
				double minMHETravelWidth = _GetMinAisleWidth();
				if (Utils.FGT(minMHETravelWidth, 0.0))
					return minMHETravelWidth;

				return base.MinLength_Y;
			}
			set
			{
				base.MinLength_Y = value;
			}
		}

		#endregion

		#region Functions

		//=============================================================================
		protected override GeometryState _GetOriginalState()
		{
			return new AisleSpace_State(this);
		}
		//=============================================================================
		protected override void _SetState(GeometryState state)
		{
			base._SetState(state);
		}
		//=============================================================================
		protected override BaseRectangleGeometry CreateInstance() { return new AisleSpace(null); }

		//=============================================================================
		public override bool SetGripPoint(int gripIndex, Point pnt, double DrawingLength, double DrawingWidth)
		{
			//
			if (GRIP_TOP_LEFT == gripIndex || GRIP_CENTER == gripIndex || GRIP_BOTTOM_RIGHT == gripIndex)
			{
				string strError;
				bool bRes = false;

				GeometryState oldState = this._GetClonedState();

				if (GRIP_TOP_LEFT == gripIndex)
					bRes = SetPropertyValue(PROP_TOP_LEFT_POINT, pnt, false, false, false, out strError);
				else if (GRIP_CENTER == gripIndex)
					bRes = SetPropertyValue(PROP_CENTER_POINT, pnt, false, false, false, out strError);
				else if (GRIP_BOTTOM_RIGHT == gripIndex)
					bRes = SetPropertyValue(PROP_BOT_RIGHT_POINT, pnt, false, false, false, out strError);

				if (!bRes)
					this._SetState(oldState);

				if(bRes)
					_MarkStateChanged();

				this.UpdateProperties();

				return bRes;
			}

			bool bResult = base.SetGripPoint(gripIndex, pnt, DrawingLength, DrawingWidth);
			OnSizeChanged();

			if(bResult)
				_MarkStateChanged();

			this.UpdateProperties();

			return bResult;
		}

		//=============================================================================
		public override bool SetPropertyValue(string strPropSysName, object propValue, bool bWasChangedViaProperties, bool bChangeTheSameRectangles, bool bNotifySheet, out string strError, bool bCheckLayout = true)
		{
			strError = string.Empty;

			if (m_Sheet == null)
				return false;

			GeometryState oldState = this._GetClonedState();

			bool bResult = false;
			// MinLengthX and MinLengthY of Aisle Space depends on the position and size.
			// base.SetPropertyValue check MinLengthX and MinLengthY while changing the property, it is not correct.
			// Need to set property with 0 as MinLengthX and MinLengthY. After the size and position of Aisle Space is set then
			// need to check MinLengthX and MinLengthY.
			if (PROP_TOP_LEFT_POINT == strPropSysName
				|| PROP_CENTER_POINT == strPropSysName
				|| PROP_BOT_RIGHT_POINT == strPropSysName
				|| PROP_TOP_LEFT_POINT_X == strPropSysName
				|| PROP_TOP_LEFT_POINT_Y == strPropSysName)
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
					bResult = this._SetPropertyValue(PROP_CENTER_POINT, newCenterPoint, m_Sheet.Length, m_Sheet.Width, bWasChangedViaProperties, out strError, bCheckLayout);
				}
				else
					bResult = this._SetPropertyValue(strPropSysName, propValue, m_Sheet.Length, m_Sheet.Width, bWasChangedViaProperties, out strError, bCheckLayout);
			}
			else
			{
				// dont notify sheet, because name is not changed yet
				bResult = base.SetPropertyValue(strPropSysName, propValue, bWasChangedViaProperties, bChangeTheSameRectangles, false, out strError, false);
			}

			OnSizeChanged();

			if(bResult)
				_MarkStateChanged();

			if (m_Sheet != null && bNotifySheet)
				m_Sheet.OnPropertyChanged(this, strPropSysName, bResult, strError);

			if (!bResult)
				this._SetState(oldState);

			this.UpdateProperties();

			return bResult;
		}

		//=============================================================================
		public void OnSizeChanged()
		{
			m_strText = Name;
			m_strText += "\n(";
			// show the minimum dimension - Length_Y or m_GlobalLength
			// if they are equal - show them both
			if (m_Length_X < m_Length_Y)
			{
				m_strText += m_Length_X.ToString(".");
			}
			else if(m_Length_Y < m_Length_X)
			{
				m_strText += m_Length_Y.ToString(".");
			}
			else
			{
				m_strText += m_Length_X.ToString(".");
				m_strText += " x ";
				m_strText += m_Length_Y.ToString(".");
			}
			m_strText += ")";
		}

		//=============================================================================
		// Aisle Space min length\width depends on the enabled MHE configurations of the document.
		//
		// If any parallel rack is adjusted to the aisle space then use PickingAisleWidth as
		// min aisle space length\width.
		//
		// If any perpendicular rack is adjusted to the aisle space then use CrossAisleWidth as
		// min aisle space length\width.
		//
		// If any block or wall is adjust to the aisle space then use EndAisleWidth as
		// min aisle space length\width.
		private double _GetMinAisleWidth()
		{
			if(Sheet != null && Sheet.Document != null)
			{
				double pickingWidth = Sheet.Document.PickingAisleWidth;
				double crossWidth = Sheet.Document.CrossAisleWidth;
				double endWidth = Sheet.Document.EndAisleWidth;

				// return if all width values are not a number
				if ((double.IsNaN(pickingWidth) || double.IsInfinity(pickingWidth))
					&& (double.IsNaN(crossWidth) || double.IsInfinity(crossWidth))
					&& (double.IsNaN(endWidth) || double.IsInfinity(endWidth)))
					return 0.0;

				// try to find racks, blocks and walls which are adjusted to this aisle space
				Dictionary<Rack, Utils.eAdjustedSide> adjustedRacksDict = new Dictionary<Rack, Utils.eAdjustedSide>();
				bool bAdjustedBlockWallIsFound = false;
				foreach(BaseRectangleGeometry geom in Sheet.Rectangles)
				{
					if (geom == null)
						continue;

					Rack rack = geom as Rack;
					if(rack != null)
					{
						Utils.eAdjustedSide adjustedSide = Utils.GetAdjustedSide(this, rack);
						if (Utils.eAdjustedSide.eNotAdjusted != adjustedSide)
							adjustedRacksDict[rack] = adjustedSide;

						continue;
					}

					if(!bAdjustedBlockWallIsFound && (geom is Block || geom is Wall))
					{
						Utils.eAdjustedSide adjustedSide = Utils.GetAdjustedSide(this, geom);
						if (Utils.eAdjustedSide.eNotAdjusted != adjustedSide)
							bAdjustedBlockWallIsFound = true;

						continue;
					}
				}

				bool isPickingAisleWidthIncluded = !double.IsNaN(pickingWidth) && !double.IsInfinity(pickingWidth);
				bool isCrossAisleWidthIncluded = !double.IsNaN(crossWidth) && !double.IsInfinity(crossWidth);
				bool isEndAisleWidthIncluded = !double.IsNaN(endWidth) && !double.IsInfinity(endWidth);
				// try to get Picking Aisle Width - find parallel rack
				// try to get Cross Aisle Width - find perpendicular rack
				bool bAdjustedParallelRackIsFound = false;
				bool bAdjustedPerpendicularRackIsFound = false;
				if (isPickingAisleWidthIncluded || isCrossAisleWidthIncluded)
				{
					foreach (Rack rack in adjustedRacksDict.Keys)
					{
						if (rack == null)
							continue;

						Utils.eAdjustedSide adjustedSide = adjustedRacksDict[rack];
						if (Utils.eAdjustedSide.eNotAdjusted == adjustedSide)
							continue;

						if (Utils.eAdjustedSide.eLeft == adjustedSide || Utils.eAdjustedSide.eRight == adjustedSide)
						{
							if (!rack.IsHorizontal)
								bAdjustedParallelRackIsFound = true;
							else
								bAdjustedPerpendicularRackIsFound = true;
						}
						else if (Utils.eAdjustedSide.eTop == adjustedSide || Utils.eAdjustedSide.eBot == adjustedSide)
						{
							if (rack.IsHorizontal)
								bAdjustedParallelRackIsFound = true;
							else
								bAdjustedPerpendicularRackIsFound = true;
						}

						if (bAdjustedParallelRackIsFound && bAdjustedPerpendicularRackIsFound)
							break;
					}
				}

				if(isPickingAisleWidthIncluded && bAdjustedParallelRackIsFound
					&& (!isCrossAisleWidthIncluded || !bAdjustedPerpendicularRackIsFound || (isCrossAisleWidthIncluded && bAdjustedPerpendicularRackIsFound && Utils.FGT(pickingWidth, crossWidth)))
					&& (!isEndAisleWidthIncluded || !bAdjustedBlockWallIsFound || (isEndAisleWidthIncluded && bAdjustedBlockWallIsFound && Utils.FGT(pickingWidth, endWidth))))
				{
					return pickingWidth;
				}
				else if(isCrossAisleWidthIncluded && bAdjustedPerpendicularRackIsFound
					&& (!isEndAisleWidthIncluded || !bAdjustedBlockWallIsFound || (isEndAisleWidthIncluded && bAdjustedBlockWallIsFound && Utils.FGT(crossWidth, endWidth))))
				{
					return crossWidth;
				}
				else if(isEndAisleWidthIncluded && bAdjustedBlockWallIsFound)
				{
					return endWidth;
				}
			}

			return 0.0;
		}

		//=============================================================================
		private bool _SetPropertyValue(string strPropSysName, object value, double DrawingLength, double DrawingWidth, bool bWasChangedViaProperties, out string strError, bool bCheckLayout = true)
		{
			strError = string.Empty;

			if (string.IsNullOrEmpty(strPropSysName))
				return false;

			bool bResult = false;
			bool bRevertChanges = false;
			int iGripIndex = GRIP_CENTER;
			//
			try
			{
				if (PROP_TOP_LEFT_POINT == strPropSysName)
				{
					if (!(value is Point))
						return false;

					Point newGlobalPoint = (Point)value;

					// save old bot right pnt for size calculation
					Point oldBotRightPnt = BottomRight_GlobalPoint;
					//
					m_TopLeft_GlobalPoint = Utils.CheckBorders(newGlobalPoint, 0, 0, DrawingLength, DrawingWidth, MarginX, MarginY);
					// calc lengthX and lengthY, dont use MinLengthX and MinLengthY because it depends on the size and position of Aisle Space
					m_Length_X = oldBotRightPnt.X - m_TopLeft_GlobalPoint.X;
					m_Length_X = Utils.GetWholeNumberByStep(m_Length_X, StepLength_X);
					m_Length_X = Utils.CheckWholeNumber(m_Length_X, 0.0, MaxLength_X);
					//
					m_Length_Y = oldBotRightPnt.Y - m_TopLeft_GlobalPoint.Y;
					m_Length_Y = Utils.GetWholeNumberByStep(m_Length_Y, StepLength_Y);
					m_Length_Y = Utils.CheckWholeNumber(m_Length_Y, 0.0, MaxLength_Y);
					//
					m_TopLeft_GlobalPoint = oldBotRightPnt;
					m_TopLeft_GlobalPoint.X -= m_Length_X;
					m_TopLeft_GlobalPoint.Y -= m_Length_Y;

					//
					bResult = true;
					iGripIndex = GRIP_TOP_LEFT;
				}
				else if (PROP_CENTER_POINT == strPropSysName)
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
				else if (PROP_BOT_RIGHT_POINT == strPropSysName)
				{
					if (!(value is Point))
						return false;

					Point newGlobalPoint = (Point)value;

					// change the size
					m_Length_X = newGlobalPoint.X - m_TopLeft_GlobalPoint.X;
					m_Length_X = Utils.GetWholeNumberByStep(m_Length_X, StepLength_X);
					double maxLengthX = MaxLength_X;
					if (double.IsNaN(maxLengthX) || double.IsPositiveInfinity(maxLengthX))
						maxLengthX = DrawingLength - MarginX;
					m_Length_X = Utils.CheckWholeNumber(m_Length_X, 0.0, maxLengthX);
					//
					m_Length_Y = newGlobalPoint.Y - m_TopLeft_GlobalPoint.Y;
					m_Length_Y = Utils.GetWholeNumberByStep(m_Length_Y, StepLength_Y);
					double maxLengthY = MaxLength_Y;
					if (double.IsNaN(maxLengthY) || double.IsPositiveInfinity(maxLengthY))
						maxLengthY = DrawingWidth - MarginY;
					m_Length_Y = Utils.CheckWholeNumber(m_Length_Y, 0.0, maxLengthY);

					//
					bResult = !bRevertChanges;
					iGripIndex = GRIP_BOTTOM_RIGHT;
				}
				else if (PROP_DIMENSION_X == strPropSysName)
				{
					m_Length_X = Convert.ToInt32(value);

					//
					bResult = true;
					iGripIndex = GRIP_CENTER;
				}
				else if (PROP_DIMENSION_Y == strPropSysName)
				{
					m_Length_Y = Convert.ToInt32(value);

					//
					bResult = true;
					iGripIndex = GRIP_CENTER;
				}

				// After property changed it is possible to check MinLengthX and MinLengthY, because
				// position and size of the Aisle Space is set.
				if (bResult)
				{
					if (!bCheckLayout)
						return true;

					// Check layout before checking MinLengthX and MinLengthY, probably AisleSpace is
					// overlaping another rectangle and should be moved.
					if(bCheckLayout)
						bResult = this.IsCorrect(DrawingLength, DrawingWidth, iGripIndex, PROP_CENTER_POINT != strPropSysName, true);

					if (!bResult)
						return false;

					// check MinLengthX and MinLengthY
					m_Length_X = Utils.CheckWholeNumber(m_Length_X, MinLength_X, MaxLength_X);
					m_Length_Y = Utils.CheckWholeNumber(m_Length_Y, MinLength_Y, MaxLength_Y);

					// check layout again, because lengthX or lengthY is changed probably
					if (bCheckLayout)
						bResult = this.IsCorrect(DrawingLength, DrawingWidth, iGripIndex, PROP_CENTER_POINT != strPropSysName, true);
				}
			}
			catch { }

			return bResult;
		}

		#endregion

		#region Serialization

		//=============================================================================
		protected static string _sAisleSpace_strMajor = "AisleSpace_MAJOR";
		protected static int _sAisleSpace_MAJOR = 1;
		protected static string _sAisleSpace_strMinor = "AisleSpace_MINOR";
		protected static int _sAisleSpace_MINOR = 0;
		//=============================================================================
		public AisleSpace(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sAisleSpace_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sAisleSpace_strMinor, typeof(int));
			if (iMajor > _sAisleSpace_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sAisleSpace_MAJOR && iMinor > _sAisleSpace_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sAisleSpace_MAJOR)
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
			info.AddValue(_sAisleSpace_strMajor, _sAisleSpace_MAJOR);
			info.AddValue(_sAisleSpace_strMinor, _sAisleSpace_MINOR);
		}

		#endregion
	}
}
