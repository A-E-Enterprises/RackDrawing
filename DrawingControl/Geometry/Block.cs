using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DrawingControl
{
	public class Block_State : GeometryState
	{
		public Block_State(Block block)
			: base(block) { }
		public Block_State(Block_State state)
			: base(state) { }

		//=============================================================================
		protected override GeometryState MakeDeepCopy()
		{
			return new Block_State(this);
		}
	}

	[Serializable]
	public class Block : BaseRectangleGeometry, ISerializable
	{
		public Block(DrawingSheet ds)
			: base(ds)
		{
			//
			MinLength_X = 100;
			MinLength_Y = 100;

			//
			MaxLength_Y = double.PositiveInfinity;
			MaxLength_X = double.PositiveInfinity;

			//
			StepLength_X = 5;
			StepLength_Y = 5;

			Name = "Block";

			//
			_OnSizeChanged();
		}

		#region Properties

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

		#endregion

		#region Functions

		//=============================================================================
		protected override GeometryState _GetOriginalState()
		{
			return new Block_State(this);
		}
		//=============================================================================
		protected override void _SetState(GeometryState state)
		{
			base._SetState(state);
		}
		//=============================================================================
		protected override BaseRectangleGeometry CreateInstance() { return new Block(null); }

		//=============================================================================
		protected override void _InitProperties()
		{
			base._InitProperties();

			try
			{
				if (m_Properties.FirstOrDefault(p => p.SystemName == BaseRectangleGeometry.PROP_NAME) == null)
					m_Properties.Add(new GeometryProperty(this, BaseRectangleGeometry.PROP_NAME, false, "Geometry"));
			}
			catch { }
		}

		//=============================================================================
		public override bool SetGripPoint(int gripIndex, Point pnt, double DrawingLength, double DrawingWidth)
		{
			bool bResult = base.SetGripPoint(gripIndex, pnt, DrawingLength, DrawingWidth);

			if(bResult)
				_MarkStateChanged();

			_OnSizeChanged();

			this.UpdateProperties();

			return bResult;
		}

		//=============================================================================
		public override bool SetPropertyValue(string strPropSysName, object propValue, bool bWasChangedViaProperties, bool bChangeTheSameRectangles, bool bNotifySheet, out string strError, bool bCheckLayout = true)
		{
			// dont notify sheet, because name is not changed yet
			bool bResult = base.SetPropertyValue(strPropSysName, propValue, bWasChangedViaProperties, bChangeTheSameRectangles, false, out strError, bCheckLayout);

			_OnSizeChanged();

			if(bResult)
				_MarkStateChanged();

			if (m_Sheet != null && bNotifySheet)
				m_Sheet.OnPropertyChanged(this, strPropSysName, bResult, strError);

			this.UpdateProperties();

			return bResult;
		}

		//=============================================================================
		private void _OnSizeChanged()
		{
			m_strText = Name;
			m_strText += "\n(";
			m_strText += m_Length_X.ToString(".");
			m_strText += " x ";
			m_strText += m_Length_Y.ToString("0.");
			m_strText += ")";
		}

		#endregion

		#region Serialization

		//=============================================================================
		protected static string _sBlock_strMajor = "Block_MAJOR";
		protected static int _sBlock_MAJOR = 1;
		protected static string _sBlock_strMinor = "Block_MINOR";
		protected static int _sBlock_MINOR = 0;
		//=============================================================================
		public Block(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sBlock_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sBlock_strMinor, typeof(int));
			if (iMajor > _sBlock_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sBlock_MAJOR && iMinor > _sBlock_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if(iMajor <= _sBlock_MAJOR)
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
			info.AddValue(_sBlock_strMajor, _sBlock_MAJOR);
			info.AddValue(_sBlock_strMinor, _sBlock_MINOR);
		}

		#endregion
	}
}
