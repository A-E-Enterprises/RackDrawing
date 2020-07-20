using AppInterfaces;
using System;
using System.Runtime.Serialization;

namespace DrawingControl
{
	/// <summary>
	/// MHE configuration.
	/// Drives the aisle space min length\width, max beam height, max pallet load.
	/// </summary>
	[Serializable]
	public class MHEConfiguration : BaseViewModel, ISerializable, IDeserializationCallback, IClonable
	{
		public static double END_AISLE_WIDTH_MIN_VALUE = 500;

		public MHEConfiguration(
			DrawingDocument document,
			bool isEnabled,
			string strType,
			double pickingAisleWidth,
			double crossAisleWidth,
			double endAisleWidth,
			double capacity,
			double maxLoadingHeight,
			double overallHeightLowered
			)
		{
			m_Document = document;
			m_bIsEnabled = isEnabled;
			m_Type = strType;
			m_PickingAisleWidth = pickingAisleWidth;
			m_CrossAisleWidth = crossAisleWidth;
			m_EndAisleWidth = endAisleWidth;
			m_Capacity = capacity;
			m_MaxLoadingHeight = maxLoadingHeight;
			m_OverallHeightLowered = overallHeightLowered;
		}
		public MHEConfiguration(MHEConfiguration mheConfig)
		{
			if(mheConfig != null)
			{
				//this.m_Document = mheConfig.m_Document;
				this.m_bIsEnabled = mheConfig.m_bIsEnabled;
				this.m_Type = mheConfig.m_Type;
				this.m_PickingAisleWidth = mheConfig.m_PickingAisleWidth;
				this.m_CrossAisleWidth = mheConfig.m_CrossAisleWidth;
				this.m_EndAisleWidth = mheConfig.m_EndAisleWidth;
				this.m_Capacity = mheConfig.m_Capacity;
				this.m_MaxLoadingHeight = mheConfig.m_MaxLoadingHeight;
				this.m_OverallHeightLowered = mheConfig.m_OverallHeightLowered;
			}
		}


		#region Properties

		//=============================================================================
		/// <summary>
		/// Document-owner.
		/// Only one MHE configuration can be enabled, so need access to MHE config collection for disable other configurations.
		/// DONT serialize\deserialize this field, otherwise it will be infinitive loop.
		/// </summary>
		private DrawingDocument m_Document = null;
		public DrawingDocument Document
		{
			get { return m_Document; }
			set { m_Document = value; }
		}

		//=============================================================================
		// If TRUE then this MHEConfiguration is included in calculations and can drive
		// aisle space min length\width, max beam height etc.
		private bool m_bIsEnabled = false;
		public bool IsEnabled
		{
			get { return m_bIsEnabled; }
			set
			{
				if (value != m_bIsEnabled)
				{
					m_bIsEnabled = value;

					// only one MHE config can be enabled
					if(m_bIsEnabled)
					{
						if(m_Document != null && m_Document.MHEConfigurationsColl != null)
						{
							foreach(MHEConfiguration mheConfig in m_Document.MHEConfigurationsColl)
							{
								if (mheConfig == null)
									continue;
								if (mheConfig == this)
									continue;

								if (mheConfig.IsEnabled)
									mheConfig.IsEnabled = false;
							}
						}
					}

					NotifyPropertyChanged(() => IsEnabled);
				}
			}
		}

		//=============================================================================
		// Type of MHE. Displayed in the document settings dialog.
		private string m_Type = string.Empty;
		public string Type
		{
			get { return m_Type; }
			set
			{
				if(value != m_Type)
				{
					m_Type = value;
					NotifyPropertyChanged(() => Type);
				}
			}
		}

		//=============================================================================
		// Picking aisle width (mm).
		// Picking aisle width > cross aisle width > end aisle width >= END_AISLE_WIDTH_MIN_VALUE.
		//
		// If any parallel rack is adjusted to the aisle space then use PickingAisleWidth as
		// min aisle space length\width.
		private double m_PickingAisleWidth = 1800;
		public double PickingAisleWidth
		{
			get { return m_PickingAisleWidth; }
			set
			{
				if(Utils.FGT(value, m_CrossAisleWidth) && value != m_PickingAisleWidth)
					m_PickingAisleWidth = value;
				NotifyPropertyChanged(() => PickingAisleWidth);
			}
		}

		//=============================================================================
		// Cross aisle width (mm).
		// Picking aisle width > cross aisle width > end aisle width >= END_AISLE_WIDTH_MIN_VALUE.
		//
		// If any perpendicular rack is adjusted to the aisle space then use CrossAisleWidth as
		// min aisle space length\width.
		private double m_CrossAisleWidth = 1500;
		public double CrossAisleWidth
		{
			get { return m_CrossAisleWidth; }
			set
			{
				if (Utils.FGT(value, m_EndAisleWidth) && Utils.FLT(value, m_PickingAisleWidth) && value != m_CrossAisleWidth)
					m_CrossAisleWidth = value;
				NotifyPropertyChanged(() => CrossAisleWidth);
			}
		}

		//=============================================================================
		// End aisle width (mm).
		// Picking aisle width > cross aisle width > end aisle width >= END_AISLE_WIDTH_MIN_VALUE.
		//
		// If any block or wall is adjust to the aisle space then use EndAisleWidth as
		// min aisle space length\width.
		private double m_EndAisleWidth = 700;
		public double EndAisleWidth
		{
			get { return m_EndAisleWidth; }
			set
			{
				if (Utils.FGE(value, END_AISLE_WIDTH_MIN_VALUE) && Utils.FLT(value, m_CrossAisleWidth) && value != m_EndAisleWidth)
					m_EndAisleWidth = value;
				NotifyPropertyChanged(() => EndAisleWidth);
			}
		}

		//=============================================================================
		// Capacity (kg).
		// Capacity of pallet's load truck.
		// Pallets load can't be greater than this value.
		private double m_Capacity = 1500;
		public double Capacity
		{
			get { return m_Capacity; }
			set
			{
				if (Utils.FGT(value, 0.0) && value != m_Capacity)
				{
					m_Capacity = value;
					NotifyPropertyChanged(() => Capacity);
				}
			}
		}

		//=============================================================================
		// Max loading height (mm).
		// Max loading height of the rack which is the top of the topmost beam.
		// This value drives the top of the topmost beam height and total rack height.
		private double m_MaxLoadingHeight = 5200;
		public double MaxLoadingHeight
		{
			get { return m_MaxLoadingHeight; }
			set
			{
				if (Utils.FGT(value, 0.0) && value != m_MaxLoadingHeight)
				{
					m_MaxLoadingHeight = value;
					NotifyPropertyChanged(() => MaxLoadingHeight);
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// Rack underpass height should be greater or equal than (OverallHeightLowered + Rack.RACK_UNDERPASS_GAP).
		/// </summary>
		private double m_OverallHeightLowered = 2000.0;
		public double OverallHeightLowered
		{
			get { return m_OverallHeightLowered; }
			set
			{
				if(Utils.FGT(m_OverallHeightLowered, 0.0) && Utils.FNE(m_OverallHeightLowered, value))
				{
					m_OverallHeightLowered = value;
				}

				NotifyPropertyChanged(() => OverallHeightLowered);
			}
		}

		#endregion

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new MHEConfiguration(this);
		}

		#region Serialization

		// 1.1 Add End Aisle Width
		// 1.2 Add OverallHeightLowered
		protected static string _sMHEConfiguration_strMajor = "MHEConfiguration_MAJOR";
		protected static int _sMHEConfiguration_MAJOR = 1;
		protected static string _sMHEConfiguration_strMinor = "MHEConfiguration_MINOR";
		protected static int _sMHEConfiguration_MINOR = 2;

		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sMHEConfiguration_strMajor, _sMHEConfiguration_MAJOR);
			info.AddValue(_sMHEConfiguration_strMinor, _sMHEConfiguration_MINOR);

			// 1.0
			info.AddValue("m_bIsEnabled", m_bIsEnabled);
			info.AddValue("m_Type", m_Type);
			info.AddValue("m_PickingAisleWidth", m_PickingAisleWidth);
			info.AddValue("m_CrossAisleWidth", m_CrossAisleWidth);
			info.AddValue("m_Capacity", m_Capacity);
			info.AddValue("m_MaxLoadingHeight", m_MaxLoadingHeight);

			// 1.1
			info.AddValue("m_EndAisleWidth", m_EndAisleWidth);

			// 1.2
			info.AddValue("m_OverallHeightLowered", m_OverallHeightLowered);
		}

		//=============================================================================
		public MHEConfiguration(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sMHEConfiguration_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sMHEConfiguration_strMinor, typeof(int));
			if (iMajor > _sMHEConfiguration_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sMHEConfiguration_MAJOR && iMinor > _sMHEConfiguration_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sMHEConfiguration_MAJOR)
			{
				// restore
				try
				{
					if (iMajor >= 1 && iMinor >= 0)
					{
						m_bIsEnabled = (bool)info.GetValue("m_bIsEnabled", typeof(bool));
						m_Type = (string)info.GetValue("m_Type", typeof(string));
						m_PickingAisleWidth = (double)info.GetValue("m_PickingAisleWidth", typeof(double));
						m_CrossAisleWidth = (double)info.GetValue("m_CrossAisleWidth", typeof(double));
						m_Capacity = (double)info.GetValue("m_Capacity", typeof(double));
						m_MaxLoadingHeight = (double)info.GetValue("m_MaxLoadingHeight", typeof(double));
					}

					if(iMajor >= 1 && iMinor >= 1)
						m_EndAisleWidth = (double)info.GetValue("m_EndAisleWidth", typeof(double));

					if(iMajor >= 1 && iMinor >= 2)
						m_OverallHeightLowered = (double)info.GetValue("m_OverallHeightLowered", typeof(double));
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
}
