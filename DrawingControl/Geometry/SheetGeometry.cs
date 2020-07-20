using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public class SheetGeometry_State : GeometryState
	{
		public SheetGeometry_State(SheetGeometry sheetGeometry)
			: base(sheetGeometry)
		{
			this.BoundSheetGUID = sheetGeometry.BoundSheetGUID;
		}
		public SheetGeometry_State(SheetGeometry_State state)
			: base(state)
		{
			this.BoundSheetGUID = state.BoundSheetGUID;
		}

		//
		public Guid BoundSheetGUID { get; private set; }

		//=============================================================================
		protected override GeometryState MakeDeepCopy()
		{
			return new SheetGeometry_State(this);
		}
	}

	/// <summary>
	/// Geometry which can have bound sheet.
	/// This geometry is used only at Warehouse sheet as a sheet preview.
	/// </summary>
	[Serializable]
	public class SheetGeometry : BaseRectangleGeometry, ISerializable
	{
		public SheetGeometry(DrawingSheet ds, DrawingSheet boundSheet)
			: base(ds)
		{
			m_FillColor = Colors.Transparent;

			// Set Length_X and Length_Y before bind sheet, otherwise values will be set to bound sheet.
			Length_X = 20000;
			Length_Y = 17000;

			this.BoundSheet = boundSheet;
			//
			m_OldSize.Width = Length_X;
			m_OldSize.Height = Length_Y;

			//
			MinLength_X = 0.0;
			MinLength_Y = 0.0;
			MaxLength_Y = double.PositiveInfinity;
			MaxLength_X = double.PositiveInfinity;
			StepLength_X = 1;
			StepLength_Y = 1;

			Name = "Without bound sheet";
		}

		#region Properties

		//=============================================================================
		/// <summary>
		/// Bound sheet, displayed by this geometry.
		/// One DrawingSheet can be bound to ONLY one SheetGeometry.
		/// Because SheetGeometry drives the position of DrawingSheet in Warehouse sheet.
		/// Only Warehouse sheet can have a roof, it means that SheetGeometry drives MAX height for the sheet depends on the position.
		/// So, if we have a 2 different SheetGeometry bound to the same DrawingSheet than we can have 2 different MAX height.
		/// It is not possible.
		/// </summary>
		private Guid m_BoundSheetGUID = Guid.Empty;
		public Guid BoundSheetGUID
		{
			get { return m_BoundSheetGUID; }
			set { m_BoundSheetGUID = value; }
		}
		public DrawingSheet BoundSheet
		{
			get
			{
				if(this.Sheet != null
					&& this.Sheet.Document != null)
				{
					return this.Sheet.Document.GetSheetByGUID(m_BoundSheetGUID);
				}

				return null;
			}
			set
			{
				if (value != null)
					m_BoundSheetGUID = value.GUID;
				else
					m_BoundSheetGUID = Guid.Empty;

				// Init length and width.
				DrawingSheet boundSheet = this.BoundSheet;
				if(boundSheet != null)
				{
					m_Length_X = boundSheet.Length;
					m_Length_Y = boundSheet.Width;
				}
			}
		}

		//=============================================================================
		public override double Length_X
		{
			get
			{
				DrawingSheet boundSheet = this.BoundSheet;
				if (this.IsInit && !m_bDontChangeBoundSheetSize && boundSheet != null)
					return boundSheet.Length;

				return base.Length_X;
			}
			set
			{
				DrawingSheet boundSheet = this.BoundSheet;
				if (this.IsInit && !m_bDontChangeBoundSheetSize && boundSheet != null)
				{
					if (Utils.FNE(boundSheet.Length, value))
					{
						boundSheet.Set_Length((UInt32)Utils.GetWholeNumber(value), false, false);
						_MarkStateChanged();
					}
				}
				else
					base.Length_X = value;
			}
		}
		//
		public override double Length_Y
		{
			get
			{
				DrawingSheet boundSheet = this.BoundSheet;
				if (this.IsInit && !m_bDontChangeBoundSheetSize && boundSheet != null)
					return boundSheet.Width;

				return base.Length_Y;
			}
			set
			{
				DrawingSheet boundSheet = this.BoundSheet;
				if (this.IsInit && !m_bDontChangeBoundSheetSize && boundSheet != null)
				{
					if (Utils.FNE(boundSheet.Width, value))
					{
						boundSheet.Set_Width((UInt32)Utils.GetWholeNumber(value), false, false);
						_MarkStateChanged();
					}
				}
				else
					base.Length_Y = value;
			}
		}

		//=============================================================================
		public override bool IsInit
		{
			get
			{
				return base.IsInit;
			}
			set
			{
				if (value != this.IsInit)
				{
					if (value)
					{
						Vector boundSheetOffset = this.TopLeft_GlobalPoint - m_OldTopLeftPoint;
						boundSheetOffset += m_AdditionalOffset;
						base.IsInit = _TryToChangeBoundSheetSize(boundSheetOffset, m_Length_X, m_Length_Y);
						if(base.IsInit)
						{
							m_AdditionalOffset.X = 0;
							m_AdditionalOffset.Y = 0;
						}
					}
					else
					{
						base.IsInit = value;
						m_OldTopLeftPoint = this.TopLeft_GlobalPoint;
						m_AdditionalOffset.X = 0;
						m_AdditionalOffset.Y = 0;
					}
				}
			}
		}

		//=============================================================================
		public override string Text
		{
			get
			{
				DrawingSheet boundSheet = this.BoundSheet;
				if (boundSheet != null)
					return boundSheet.Name;

				return base.Text;
			}
			set
			{
				DrawingSheet boundSheet = this.BoundSheet;
				if (boundSheet != null)
					boundSheet.Name = value;
				else
					base.Text = value;
			}
		}

		//=============================================================================
		// If user moves top left or right bot grip point then length or depth of rectangle is changed.
		// If any sheet is bound then sheet length\width is changed and MarkStateChanged() is called.
		// MarkStateChanged() stops move grip point command.
		private bool m_bDontChangeBoundSheetSize = false;
		// Old top left point position
		private Point m_OldTopLeftPoint = new Point(0.0, 0.0);
		// Size before start grip point move.
		// It is used to draw bound sheet content and clip it.
		private Size m_OldSize = new Size(0.0, 0.0);
		private bool m_IsCenterGripPointMoving = false;
		/// <summary>
		/// Additional offset.
		/// It is used when user moves non initialized SheetGeometry by the center grip point.
		/// It accumulates top left point offset.
		/// </summary>
		private Vector m_AdditionalOffset = new Vector(0.0, 0.0);

		#endregion

		#region Functions overrides

		//=============================================================================
		protected override GeometryState _GetOriginalState()
		{
			return new SheetGeometry_State(this);
		}
		//=============================================================================
		protected override void _SetState(GeometryState state)
		{
			base._SetState(state);

			SheetGeometry_State sheetGeomState = state as SheetGeometry_State;
			if (sheetGeomState == null)
				return;

			this.m_BoundSheetGUID = sheetGeomState.BoundSheetGUID;
		}
		//=============================================================================
		protected override BaseRectangleGeometry CreateInstance() { return new SheetGeometry(null, null); }

		//=============================================================================
		protected override void _InitProperties()
		{
			base._InitProperties();

			if (m_Properties != null)
			{
				try
				{
					// Remove height property because bound sheet doesnt have any height.
					// Also bound sheet doesnt have any roof, only warehouse sheet has roof.
					Property_ViewModel heightProp = m_Properties.FirstOrDefault(prop => prop != null && prop.SystemName == PROP_DIMENSION_Z);
					if (heightProp != null)
						m_Properties.Remove(heightProp);

					// Remove GeometryType_Property.
					Property_ViewModel typeProp = m_Properties.FirstOrDefault(prop => prop != null && prop is GeometryType_Property);
					if (typeProp != null)
						m_Properties.Remove(typeProp);

					if (m_Properties.FirstOrDefault(p => p.SystemName == BaseRectangleGeometry.PROP_NAME) == null)
						m_Properties.Insert(0, new GeometryProperty(this, BaseRectangleGeometry.PROP_NAME, false, "Geometry"));
				}
				catch { }
			}
		}

		//=============================================================================
		public override void Draw(DrawingContext dc, ICoordinateSystem cs, IGeomDisplaySettings geomDisplaySettings = null)
		{
			// STEP 1.
			// Draw sheet content with opacity.
			DrawingSheet boundSheet = this.BoundSheet;
			if (boundSheet != null)
			{
				dc.PushOpacity(0.3);
				//
				Point zeroLocalPnt = GetLocalPoint(cs, this.TopLeft_GlobalPoint);
				int LengthInPixels = Utils.GetWholeNumber(GetWidthInPixels(cs, Length_X));
				int WidthInPixels = Utils.GetWholeNumber(GetHeightInPixels(cs, Length_Y));
				dc.PushClip(new RectangleGeometry(new Rect(zeroLocalPnt.X, zeroLocalPnt.Y, LengthInPixels, WidthInPixels)));
				//
				Vector globalOffsetVec = new Vector(this.TopLeft_GlobalPoint.X, this.TopLeft_GlobalPoint.Y);
				double lengthVal = Length_X;
				double heightVal = Length_Y;
				if((m_bDontChangeBoundSheetSize || !this.IsInit) && Utils.FGT(m_OldSize.Width, 0.0) && Utils.FGT(m_OldSize.Height, 0.0))
				{
					lengthVal = m_OldSize.Width;
					heightVal = m_OldSize.Height;
				}
				if (m_bDontChangeBoundSheetSize || !this.IsInit)
				{
					if (!m_IsCenterGripPointMoving)
					{
						globalOffsetVec.X = m_OldTopLeftPoint.X;
						globalOffsetVec.Y = m_OldTopLeftPoint.Y;
					}
					globalOffsetVec -= m_AdditionalOffset;
				}

				int boundSheetLengthInPixels = Utils.GetWholeNumber(GetWidthInPixels(cs, lengthVal));
				int boundSheetWidthInPixels = Utils.GetWholeNumber(GetHeightInPixels(cs, heightVal));
				Point topLeftScreenPoint = GetLocalPoint(cs, new Point(0.0, 0.0) + globalOffsetVec);
				Vector offsetInPixels = topLeftScreenPoint - new Point(0.0, 0.0);
				ImageCoordinateSystem ics = new ImageCoordinateSystem(boundSheetLengthInPixels, boundSheetWidthInPixels, offsetInPixels, new Size(boundSheet.Length, boundSheet.Width), 0.0);
				//
				DefaultGeomDisplaySettings displaySettings = new DefaultGeomDisplaySettings();
				displaySettings.DisplayText = false;
				//
				if (boundSheet.Rectangles != null)
				{
					foreach (BaseRectangleGeometry geom in boundSheet.Rectangles)
					{
						if (geom == null)
							continue;

						// Dont display SheetElevationGeometry, because it displayed in pixels fixed size
						if (geom is SheetElevationGeometry)
							continue;

						geom.Draw(dc, ics, displaySettings);
					}
				}
				// Pop clip
				dc.Pop();
				// Pop opacity
				dc.Pop();
			}

			// STEP 2.
			// Draw sheet borders and name.
			base.Draw(dc, cs, geomDisplaySettings);
		}

		//=============================================================================
		public void BeforeGripPointMove(int iGripIndex)
		{
			// Ignore center grip point because it doesnt change size and top left point position.
			if (BaseRectangleGeometry.GRIP_CENTER == iGripIndex)
			{
				m_IsCenterGripPointMoving = true;
				if (!this.IsInit)
				{
					m_AdditionalOffset += this.TopLeft_GlobalPoint - m_OldTopLeftPoint;
					m_OldTopLeftPoint = this.TopLeft_GlobalPoint;
				}
			}

			// If any sheet is bound to this geometry then need to
			// save old offset and set m_bDontChangeBoundSheetSize true.
			//
			// It is necessary for draw old sheet content picture inside this geometry.
			// And dont change bound sheet length\width while grip point is moving, because
			// it will call MarkStateChanged() which will stop move grip point command.
			DrawingSheet boundSheet = this.BoundSheet;
			if (boundSheet == null)
				return;

			//
			m_bDontChangeBoundSheetSize = true;

			if (this.IsInit)
			{
				m_OldSize.Width = Length_X;
				m_OldSize.Height = Length_Y;
				m_OldTopLeftPoint = m_TopLeft_GlobalPoint;

				m_Length_X = boundSheet.Length;
				m_Length_Y = boundSheet.Width;
			}
		}

		//=============================================================================
		public void AfterGripPointMove(int iGripIndex)
		{
			// Ignore center grip point because it doesnt change size and top left point position.
			if (BaseRectangleGeometry.GRIP_CENTER == iGripIndex)
			{
				m_IsCenterGripPointMoving = false;
				m_OldTopLeftPoint = this.TopLeft_GlobalPoint;
			}

			// Read comment inside BeforeGripPointMove() function.
			// Apply changes to the bound sheet.

			DrawingSheet boundSheet = this.BoundSheet;
			if (this.IsInit && boundSheet != null)
			{
				Vector boundSheetTopLeftPointOffset = this.TopLeft_GlobalPoint - m_OldTopLeftPoint;
				if(!_TryToChangeBoundSheetSize(boundSheetTopLeftPointOffset, m_Length_X, m_Length_Y))
				{
					m_Length_X = boundSheet.Length;
					m_Length_Y = boundSheet.Width;
				}
			}

			m_bDontChangeBoundSheetSize = false;
			if (this.IsInit)
			{
				m_OldSize.Width = 0.0;
				m_OldSize.Height = 0.0;
				m_OldTopLeftPoint = new Point(0.0, 0.0);
			}
		}

		//=============================================================================
		public override bool SetGripPoint(int gripIndex, Point pnt, double DrawingLength, double DrawingWidth)
		{
			return base.SetGripPoint(gripIndex, pnt, DrawingLength, DrawingWidth);
		}

		//=============================================================================
		public override object GetPropertyValue(string strPropSysName)
		{
			if(BaseRectangleGeometry.PROP_NAME == strPropSysName)
			{
				DrawingSheet boundSheet = this.BoundSheet;
				if(boundSheet != null)
				{
					return boundSheet.Name;
				}
			}

			return base.GetPropertyValue(strPropSysName);
		}

		//=============================================================================
		public override bool SetPropertyValue(string strPropSysName, object propValue, bool bWasChangedViaProperties, bool bChangeTheSameRectangles, bool bNotifySheet, out string strError, bool bCheckLayout = true)
		{
			strError = string.Empty;

			// dont notify sheet, because name is not changed yet
			bool bResult = false;
			if(BaseRectangleGeometry.PROP_NAME == strPropSysName)
			{
				DrawingSheet boundSheet = this.BoundSheet;
				if(boundSheet == null)
					bResult = base.SetPropertyValue(strPropSysName, propValue, bWasChangedViaProperties, bChangeTheSameRectangles, false, out strError, bCheckLayout);
				else
				{
					try
					{
						boundSheet.Name = Convert.ToString(propValue);
						bResult = true;
					}
					catch { }
				}
			}
			else
				bResult = base.SetPropertyValue(strPropSysName, propValue, bWasChangedViaProperties, bChangeTheSameRectangles, false, out strError, bCheckLayout);

			if(bResult)
				_MarkStateChanged();

			if (m_Sheet != null && bNotifySheet)
				m_Sheet.OnPropertyChanged(this, strPropSysName, bResult, strError);

			this.UpdateProperties();

			return bResult;
		}

		#endregion

		#region Private functions

		//=============================================================================
		private bool _TryToChangeBoundSheetSize(Vector topLeftPointOffset, double length, double width)
		{
			if (Utils.FLE(length, 0.0) || Utils.FLE(width, 0.0))
				return false;

			DrawingSheet boundSheet = this.BoundSheet;
			if (boundSheet != null)
			{
				bool bChangeSize = false;

				UInt32 sheetLength = (UInt32)Utils.GetWholeNumber(length);
				UInt32 sheetWidth = (UInt32)Utils.GetWholeNumber(width);
				if (boundSheet.IsThereRectanglesOutsideGraphicsArea(topLeftPointOffset, sheetLength, sheetWidth, true))
				{
					// ask user
					if (MessageBox.Show("The updated length cannot fit some of the block(s).These blocks will be deleted.Do you want to continue?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
					{
						// delete
						boundSheet.IsThereRectanglesOutsideGraphicsArea(topLeftPointOffset, sheetLength, sheetWidth, false);
						bChangeSize = true;
					}
				}
				else
					bChangeSize = true;

				if (bChangeSize)
				{
					boundSheet.ChangeSize(topLeftPointOffset, sheetLength, sheetWidth);
					return true;
				}

				return false;
			}

			return true;
		}

		#endregion

		#region Serialization

		//=============================================================================
		// 1.0
		// 1.1 Use GUID of bound sheet instead DrawingSheet
		protected static string _sSheetGeometry_strMajor = "SheetGeometry_MAJOR";
		protected static int _sSheetGeometry_MAJOR = 1;
		protected static string _sSheetGeometry_strMinor = "SheetGeometry_MINOR";
		protected static int _sSheetGeometry_MINOR = 1;
		//=============================================================================
		public SheetGeometry(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sSheetGeometry_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sSheetGeometry_strMinor, typeof(int));
			if (iMajor > _sSheetGeometry_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sSheetGeometry_MAJOR && iMinor > _sSheetGeometry_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sSheetGeometry_MAJOR)
			{
				try
				{
					// 1.0
					// removed in 1.1
					//m_BoundSheet = (DrawingSheet)info.GetValue("BoundSheet", typeof(DrawingSheet));

					// 1.1
					if (iMajor >= 1 && iMinor >= 1)
						m_BoundSheetGUID = (Guid)info.GetValue("BoundSheetGUID", typeof(Guid));
				}
				catch
				{
					++DrawingDocument._sStreamReadException;
				}
			}
		}
		//=============================================================================
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			//
			info.AddValue(_sSheetGeometry_strMajor, _sSheetGeometry_MAJOR);
			info.AddValue(_sSheetGeometry_strMinor, _sSheetGeometry_MINOR);

			// 1.0
			//info.AddValue("BoundSheet", m_BoundSheet);
			info.AddValue("BoundSheet", null);

			// 1.1
			info.AddValue("BoundSheetGUID", m_BoundSheetGUID);
		}

		#endregion
	}
}
