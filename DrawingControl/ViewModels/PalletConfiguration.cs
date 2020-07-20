using AppInterfaces;
using System;
using System.Runtime.Serialization;

namespace DrawingControl
{
	// Predefined pallet configuration - length, width, height and load.
	// It is stored in the document and can be used in the rack.
	// When user changes pallet configuration in the document then changes should be
	// apllied to all racks, which use the same pallet configuration.
	[Serializable]
	public class PalletConfiguration : BaseViewModel, ISerializable, IDeserializationCallback, IClonable
	{
		public PalletConfiguration(int uniqueIndex)
		{
			m_UniqueIndex = uniqueIndex;
		}
		public PalletConfiguration(PalletConfiguration palletConfig)
		{
			if(palletConfig != null)
			{
				this.m_UniqueIndex = palletConfig.m_UniqueIndex;
				this.m_Length = palletConfig.m_Length;
				this.m_Width = palletConfig.m_Width;
				this.m_Height = palletConfig.m_Height;
				this.m_Capacity = palletConfig.m_Capacity;
				this.m_GUID = palletConfig.m_GUID;
			}
		}

		#region Properties

		//=============================================================================
		private int m_UniqueIndex = -1;
		public int UniqueIndex { get { return m_UniqueIndex; } }

		//=============================================================================
		// Unique ID of PalletConfiguration
		private Guid m_GUID = Guid.NewGuid();
		public Guid GUID { get { return m_GUID; } }

		//=============================================================================
		public string DisplayName
		{
			get
			{
				return "P" + m_UniqueIndex.ToString();
			}
		}

		//=============================================================================
		private double m_Length = 1200;
		public double Length
		{
			get { return m_Length; }
			set
			{
				if(Utils.FNE(value, m_Length))
					m_Length = value;

				NotifyPropertyChanged(() => Length);
			}
		}

		//=============================================================================
		private double m_Width = 1000;
		public double Width
		{
			get { return m_Width; }
			set
			{
				if (Utils.FNE(value, m_Width))
					m_Width = value;

				NotifyPropertyChanged(() => Width);
			}
		}

		//=============================================================================
		private double m_Height = 800;
		public double Height
		{
			get { return m_Height; }
			set
			{
				if (Utils.FNE(value, m_Height) && Utils.FLE(value, Pallet.MAX_HEIGHT))
					m_Height = value;

				NotifyPropertyChanged(() => Height);
			}
		}

		//=============================================================================
		private double m_Capacity = 1000;
		public double Capacity
		{
			get { return m_Capacity; }
			set
			{
				if (Utils.FNE(value, m_Capacity))
					m_Capacity = value;

				NotifyPropertyChanged(() => Capacity);
			}
		}

		//=============================================================================
		private bool m_bMarkDeleted = false;
		public bool MarkDeleted
		{
			get { return m_bMarkDeleted; }
			set
			{
				if(value != m_bMarkDeleted)
				{
					m_bMarkDeleted = value;
					NotifyPropertyChanged(() => MarkDeleted);
				}
			}
		}

		#endregion

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new PalletConfiguration(this);
		}

		#region Serialization

		protected static string _sPalletConfiguration_strMajor = "PalletConfiguration_MAJOR";
		protected static int _sPalletConfiguration_MAJOR = 1;
		protected static string _sPalletConfiguration_strMinor = "PalletConfiguration_MINOR";
		protected static int _sPalletConfiguration_MINOR = 0;

		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sPalletConfiguration_strMajor, _sPalletConfiguration_MAJOR);
			info.AddValue(_sPalletConfiguration_strMinor, _sPalletConfiguration_MINOR);

			// 1.0
			info.AddValue("m_UniqueIndex", m_UniqueIndex);
			info.AddValue("m_Length", m_Length);
			info.AddValue("m_Width", m_Width);
			info.AddValue("m_Height", m_Height);
			info.AddValue("m_Capacity", m_Capacity);
			info.AddValue("m_GUID", m_GUID);
		}
		//=============================================================================
		public PalletConfiguration(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sPalletConfiguration_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sPalletConfiguration_strMinor, typeof(int));
			if (iMajor > _sPalletConfiguration_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sPalletConfiguration_MAJOR && iMinor > _sPalletConfiguration_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sPalletConfiguration_MAJOR)
			{
				// restore
				try
				{
					if (iMajor >= 1 && iMinor >= 0)
					{
						m_UniqueIndex = (int)info.GetValue("m_UniqueIndex", typeof(int));
						m_Length = (double)info.GetValue("m_Length", typeof(double));
						m_Width = (double)info.GetValue("m_Width", typeof(double));
						m_Height = (double)info.GetValue("m_Height", typeof(double));
						m_Capacity = (double)info.GetValue("m_Capacity", typeof(double));
						m_GUID = (Guid)info.GetValue("m_GUID", typeof(Guid));
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
		public virtual void OnDeserialization(object sender) { }

		#endregion
	}
}
