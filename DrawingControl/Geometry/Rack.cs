using AppColorTheme;
using AppInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DrawingControl
{
	public enum ePalletType
	{
		eUndefined = 0,
		eOverhang = 1,
		eFlush = 2
	}

	public enum eBracingType
	{
		eUndefined = 0,
		eGI = 1,
		ePowderCoated = 2
	}

	public enum eDeckPlateType
	{
		eUndefined = 0,
		eAlongDepth_UDL = 1,
		eAlongLength = 2,
		eAlongDepth_PalletSupport = 3
	}

	/// <summary>
	/// Determine on which frame place tie beam.
	/// If tie beam is placed on the frame then
	/// FrameHeight = MaxMaterialHeight + TIE_BEAM_ADDITIONAL_HEIGHT.
	/// Frame height should be multiple of 100.
	/// </summary>
	[Flags]
	public enum eTieBeamFrame
	{
		eNone = 0,
		eStartFrame = 1,
		eEndFrame = 2
	}

	[Flags]
	public enum ConectedAisleSpaceDirection
	{
		NONE = 1,
		TOP = 2,
		BOTTOM = 4,
		RIGHT = 8,
		LEFT = 16
	}

	[Serializable]
	public class RackLevelBeam : ISerializable, IDeserializationCallback, IClonable
	{
		public RackLevelBeam(RackLevel ownerLevel, RackBeam beam)
		{
			m_OwnerLevel = ownerLevel;
			if (beam != null)
				m_RackBeamGUID = beam.GUID;
		}
		public RackLevelBeam(RackLevelBeam beam)
		{
			this.m_MinLength = beam.m_MinLength;
			this.m_MaxLength = beam.m_MaxLength;
			this.m_MaxLoadCapacity = beam.m_MaxLoadCapacity;

			this.m_RackBeamGUID = beam.m_RackBeamGUID;
		}

		#region Properties

		//=============================================================================
		private RackLevel m_OwnerLevel = null;
		public RackLevel OwnerLevel
		{
			get { return m_OwnerLevel; }
			set { m_OwnerLevel = value; }
		}

		//=============================================================================
		// Guid of RackBeam from DrawingDocument.BeamsList.
		private Guid m_RackBeamGUID = Guid.Empty;
		public Guid RackBeamGUID { get { return m_RackBeamGUID; } }

		//=============================================================================
		public RackBeam Beam
		{
			get
			{
				if (m_RackBeamGUID == Guid.Empty)
					return null;

				if(m_OwnerLevel != null
					&& m_OwnerLevel.Owner != null
					&& m_OwnerLevel.Owner.Sheet != null
					&& m_OwnerLevel.Owner.Sheet.Document != null
					&& m_OwnerLevel.Owner.Sheet.Document.BeamsList != null)
				{
					return m_OwnerLevel.Owner.Sheet.Document.BeamsList.FirstOrDefault(beam => beam != null && beam.GUID == m_RackBeamGUID);
				}

				return null;
			}
		}

		//=============================================================================
		public double Height
		{
			get
			{
				RackBeam beam = this.Beam;
				if (beam != null)
					return beam.Height;

				return 0.0;
			}
		}

		//=============================================================================
		public double Depth { get { return 50; } }

		//=============================================================================
		public double Thickness
		{
			get
			{
				RackBeam beam = this.Beam;
				if (beam != null)
					return beam.Thickness;

				return 0.0;
			}
		}

		//=============================================================================
		private int m_MinLength = 1200;
		public int MinLength { get { return m_MinLength; } }

		//=============================================================================
		private int m_MaxLength = 2300;
		public int MaxLength { get { return m_MaxLength; } }

		//=============================================================================
		/// <summary>
		/// Max load per level.
		/// </summary>
		private int m_MaxLoadCapacity = 4000;
		public int MaxLoadCapacity { get { return m_MaxLoadCapacity; } }

		#endregion

		#region Public functions

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new RackLevelBeam(this);
		}

		#endregion

		#region Serialization

		//=============================================================================
		// 2.0 Use RackBeam inside
		protected static string _sRackLevelBeam_strMajor = "RackLevelBeam_MAJOR";
		protected static int _sRackLevelBeam_MAJOR = 2;
		protected static string _sRackLevelBeam_strMinor = "RackLevelBeam_MINOR";
		protected static int _sRackLevelBeam_MINOR = 0;
		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sRackLevelBeam_strMajor, _sRackLevelBeam_MAJOR);
			info.AddValue(_sRackLevelBeam_strMinor, _sRackLevelBeam_MINOR);

			//
			//info.AddValue("SizeX", m_SizeX);
			//info.AddValue("SizeY", m_SizeY);
			//info.AddValue("Thickness", m_Thickness);
			info.AddValue("MinLength", m_MinLength);
			info.AddValue("MaxLength", m_MaxLength);
			//info.AddValue("Length", m_Length);
			info.AddValue("MaxLoadCapacity", m_MaxLoadCapacity);

			// 2.0
			info.AddValue("RackBeamGUID", m_RackBeamGUID);
		}
		//=============================================================================
		public RackLevelBeam(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sRackLevelBeam_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sRackLevelBeam_strMinor, typeof(int));
			if (iMajor > _sRackLevelBeam_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sRackLevelBeam_MAJOR && iMinor > _sRackLevelBeam_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sRackLevelBeam_MAJOR)
			{
				//
				//m_SizeX = (int)info.GetValue("SizeX", typeof(int));
				//m_SizeY = (int)info.GetValue("SizeY", typeof(int));
				//m_Thickness = (double)info.GetValue("Thickness", typeof(double));
				m_MinLength = (int)info.GetValue("MinLength", typeof(int));
				m_MaxLength = (int)info.GetValue("MaxLength", typeof(int));
				//m_Length = (int)info.GetValue("Length", typeof(int));
				m_MaxLoadCapacity = (int)info.GetValue("MaxLoadCapacity", typeof(int));

				if (iMajor >= 2 && iMinor >= 0)
					m_RackBeamGUID = (Guid)info.GetValue("RackBeamGUID", typeof(Guid));
			}
			else
				++DrawingDocument._sBiggerMajorNumber;
		}
		//=============================================================================
		public virtual void OnDeserialization(object sender) { }

		#endregion
	}

	[Serializable]
	public class RackLevelAccessories : ISerializable, IDeserializationCallback, IClonable
	{
		public static string DECKING_PANEL_6BP_SHELVING = "Decking Panel 6BP (Shelving application)";
		public static string DECKING_PANEL_6BP_PALLET = "Decking Panel 6BP (Pallet application)";
		public static string DECKING_PANEL_4BP = "Decking Panel 4BP";
		public static string PALLET_STOPPER = "Pallet Stopper";
		public static string FORK_ENTRY_BAR = "Fork Entry Bar";
		public static string PALLET_SUPPORT_BAR = "Pallet Support Bar(PSB)";
		public static string GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER = "Guided Type Pallet Support With Stopper";
		public static string GUIDED_TYPE_PALLET_SUPPORT_WITH_PSB = "Guided Type Pallet Support With PSB";
		public static string GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER_AND_PSB = "Guided Type Pallet Support With Stopper and With PSB";

		public static string DECKING_PANEL_6BP_SHELVING_SHORT = "DP 6BP (Shelving application)";
		public static string DECKING_PANEL_6BP_PALLET_SHORT = "DP 6BP (Pallet application)";
		public static string DECKING_PANEL_4BP_SHORT = "DP 4BP";
		public static string PALLET_STOPPER_SHORT = "Pallet Stopper";
		public static string FORK_ENTRY_BAR_SHORT = "Fork Entry Bar";
		public static string PALLET_SUPPORT_BAR_SHORT = "Pallet Support Bar(PSB)";
		public static string GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER_SHORT = "GTPS With Stopper";
		public static string GUIDED_TYPE_PALLET_SUPPORT_WITH_PSB_SHORT = "GTPS With PSB";
		public static string GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER_AND_PSB_SHORT = "GTPS + Stopper and PSB";

		public RackLevelAccessories(RackLevel owner)
		{
			m_Owner = owner;
		}
		public RackLevelAccessories(RackLevelAccessories accessories)
		{
			this.m_bIsDeckPlateAvailable = accessories.m_bIsDeckPlateAvailable;
			this.m_DeckPlateType = accessories.m_DeckPlateType;

			this.m_bPalletStopper = accessories.m_bPalletStopper;
			this.m_bForkEntryBar = accessories.m_bForkEntryBar;
			this.m_bPalletSupportBar = accessories.m_bPalletSupportBar;
			this.m_bGuidedTypePalletSupport = accessories.m_bGuidedTypePalletSupport;
			this.m_bGuidedTypePalletSupport_WithStopper = accessories.m_bGuidedTypePalletSupport_WithStopper;
			this.m_bGuidedTypePalletSupport_WithPSB = accessories.m_bGuidedTypePalletSupport_WithPSB;
		}

		#region Properties

		//=============================================================================
		private RackLevel m_Owner = null;
		public RackLevel Owner
		{
			get { return m_Owner; }
			set { m_Owner = value; }
		}

		//=============================================================================
		private bool m_bIsDeckPlateAvailable = false;
		public bool IsDeckPlateAvailable
		{
			get { return m_bIsDeckPlateAvailable; }
			set { m_bIsDeckPlateAvailable = value; }
		}
		//=============================================================================
		private eDeckPlateType m_DeckPlateType = eDeckPlateType.eAlongDepth_UDL;
		public eDeckPlateType DeckPlateType
		{
			get { return m_DeckPlateType; }
			set { m_DeckPlateType = value; }
		}
		//=============================================================================
		private bool m_bPalletStopper = false;
		public bool PalletStopper
		{
			get { return m_bPalletStopper; }
			set { m_bPalletStopper = value; }
		}
		//=============================================================================
		private bool m_bForkEntryBar = false;
		public bool ForkEntryBar
		{
			get { return m_bForkEntryBar; }
			set { m_bForkEntryBar = value; }
		}
		//=============================================================================
		private bool m_bPalletSupportBar = false;
		public bool PalletSupportBar
		{
			get { return m_bPalletSupportBar; }
			set { m_bPalletSupportBar = value; }
		}
		//=============================================================================
		private bool m_bGuidedTypePalletSupport = false;
		public bool GuidedTypePalletSupport
		{
			get { return m_bGuidedTypePalletSupport; }
			set { m_bGuidedTypePalletSupport = value; }
		}
		//=============================================================================
		private bool m_bGuidedTypePalletSupport_WithStopper = false;
		public bool GuidedTypePalletSupport_WithStopper
		{
			get { return m_bGuidedTypePalletSupport_WithStopper; }
			set { m_bGuidedTypePalletSupport_WithStopper = value; }
		}
		//=============================================================================
		private bool m_bGuidedTypePalletSupport_WithPSB = false;
		public bool GuidedTypePalletSupport_WithPSB
		{
			get { return m_bGuidedTypePalletSupport_WithPSB; }
			set { m_bGuidedTypePalletSupport_WithPSB = value; }
		}

		#endregion

		#region Public functions

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new RackLevelAccessories(this);
		}

		#endregion

		#region Serialization

		//=============================================================================
		// 2.0 Rename and remopve properties.
		// 2.1 Add IsMeshCladdingEnabled and MeshHeight.
		// 2.2 Remove IsMeshCladdingEnabled and MeshHeight. They are placed at rack accessories.
		protected static string _sRackLevelAccessories_strMajor = "RackLevelAccessories_MAJOR";
		protected static int _sRackLevelAccessories_MAJOR = 2;
		protected static string _sRackLevelAccessories_strMinor = "RackLevelAccessories_MINOR";
		protected static int _sRackLevelAccessories_MINOR = 1;
		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sRackLevelAccessories_strMajor, _sRackLevelAccessories_MAJOR);
			info.AddValue(_sRackLevelAccessories_strMinor, _sRackLevelAccessories_MINOR);

			// 2.0
			info.AddValue("IsDeckPlateAvailable", m_bIsDeckPlateAvailable);
			info.AddValue("DeckPlateType", m_DeckPlateType);
			//
			info.AddValue("PalletStopper", m_bPalletStopper);
			info.AddValue("ForkEntryBar", m_bForkEntryBar);
			info.AddValue("PalletSupportBar", m_bPalletSupportBar);
			info.AddValue("GuidedTypePalletSupport", m_bGuidedTypePalletSupport);
			info.AddValue("GuidedTypePalletSupport_WithStopper", m_bGuidedTypePalletSupport_WithStopper);
			info.AddValue("GuidedTypePalletSupport_WithPSB", m_bGuidedTypePalletSupport_WithPSB);

			// 2.1
			// Removed in 2.2
			//info.AddValue("IsMeshCladdingEnabled", m_bIsMeshCladdingEnabled);
			//info.AddValue("MeshHeight", m_MeshHeight);
		}
		//=============================================================================
		public RackLevelAccessories(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sRackLevelAccessories_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sRackLevelAccessories_strMinor, typeof(int));
			if (iMajor > _sRackLevelAccessories_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sRackLevelAccessories_MAJOR && iMinor > _sRackLevelAccessories_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sRackLevelAccessories_MAJOR)
			{
				if (iMajor >= 1 && iMinor >= 0)
				{
					//
					m_bIsDeckPlateAvailable = (bool)info.GetValue("IsDeckPlateAvailable", typeof(bool));
					m_DeckPlateType = (eDeckPlateType)info.GetValue("DeckPlateType", typeof(eDeckPlateType));
					//
					m_bPalletStopper = (bool)info.GetValue("PalletStopper", typeof(bool));
				}

				if (iMajor == 1 && iMinor == 0)
				{
					m_bForkEntryBar = (bool)info.GetValue("PalletRiser", typeof(bool));
					m_bPalletSupportBar = (bool)info.GetValue("PalletSupport", typeof(bool));
					m_bGuidedTypePalletSupport = (bool)info.GetValue("PalletGuide", typeof(bool));
				}

				if (iMajor >= 2 && iMinor >= 0)
				{
					m_bForkEntryBar = (bool)info.GetValue("ForkEntryBar", typeof(bool));
					m_bPalletSupportBar = (bool)info.GetValue("PalletSupportBar", typeof(bool));
					m_bGuidedTypePalletSupport = (bool)info.GetValue("GuidedTypePalletSupport", typeof(bool));
					m_bGuidedTypePalletSupport_WithStopper = (bool)info.GetValue("GuidedTypePalletSupport_WithStopper", typeof(bool));
					m_bGuidedTypePalletSupport_WithPSB = (bool)info.GetValue("GuidedTypePalletSupport_WithPSB", typeof(bool));
				}

				// Removed in 2.2
				//if(iMajor >= 2 && iMinor >= 1)
				//{
				//	m_bIsMeshCladdingEnabled = (bool)info.GetValue("IsMeshCladdingEnabled", typeof(bool));
				//	m_MeshHeight = (double)info.GetValue("MeshHeight", typeof(double));
				//}
			}
			else
				++DrawingDocument._sBiggerMajorNumber;
		}
		//=============================================================================
		public virtual void OnDeserialization(object sender) { }

		#endregion

		//=============================================================================
		public static bool operator ==(RackLevelAccessories left, RackLevelAccessories right)
		{
			return _AreEquals(left, right, true);
		}
		public static bool operator !=(RackLevelAccessories left, RackLevelAccessories right)
		{
			return !_AreEquals(left, right, true);
		}
		public static bool _AreEquals(RackLevelAccessories left, RackLevelAccessories right, bool bComparePallets)
		{
			// Check for null values and compare run-time types.
			if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
				return false;

			//
			if (left.IsDeckPlateAvailable != right.IsDeckPlateAvailable)
				return false;
			if (left.IsDeckPlateAvailable && left.DeckPlateType != right.DeckPlateType)
				return false;

			//
			if (bComparePallets)
			{
				if (left.PalletStopper != right.PalletStopper)
					return false;
				if (left.ForkEntryBar != right.ForkEntryBar)
					return false;
				if (left.PalletSupportBar != right.PalletSupportBar)
					return false;
				if (left.GuidedTypePalletSupport_WithStopper != right.GuidedTypePalletSupport_WithStopper)
					return false;
				if (left.GuidedTypePalletSupport_WithPSB != right.GuidedTypePalletSupport_WithPSB)
					return false;
			}

			return true;
		}
	}

	[Serializable]
	public class Pallet : BaseViewModel, ISerializable, IDeserializationCallback, IClonable
	{
		public Pallet(RackLevel _level)
		{
			m_Level = _level;
		}
		public Pallet(Pallet pallet)
		{
			this.m_Length = pallet.m_Length;
			this.m_Width = pallet.m_Width;
			this.m_Height = pallet.m_Height;
			this.m_Load = pallet.m_Load;

			this.m_PalletConfigurationGUID = pallet.m_PalletConfigurationGUID;
		}

		#region Properties

		//=============================================================================
		private RackLevel m_Level = null;
		public RackLevel Level
		{
			get { return m_Level; }
			set { m_Level = value; }
		}

		//=============================================================================
		public int DisplayIndex { get { return _PalletIndex + 1; } }

		//=============================================================================
		public int _PalletIndex
		{
			get
			{
				if (m_Level != null && m_Level.Pallets != null && m_Level.Pallets.Contains(this))
					return m_Level.Pallets.IndexOf(this);

				return 0;
			}
		}

		//=============================================================================
		// PalletConfiguration from the DrawingDocument's PalletConfigurationCollection.
		// It drives pallet length, width, height and load.
		private Guid m_PalletConfigurationGUID = Guid.Empty;
		public PalletConfiguration PalletConfiguration
		{
			get
			{
				if (m_PalletConfigurationGUID == Guid.Empty)
					return null;

				if(this.Level != null
					&& this.Level.Owner != null
					&& this.Level.Owner.Sheet != null
					&& this.Level.Owner.Sheet.Document != null
					&& this.Level.Owner.Sheet.Document.PalletConfigurationCollection != null)
				{
					return this.Level.Owner.Sheet.Document.PalletConfigurationCollection.FirstOrDefault(p => p != null && p.GUID == m_PalletConfigurationGUID);
				}

				return null;
			}
			set
			{
				if (m_Level != null && m_Level.Owner != null)
				{
					string strPropSysName = RackLevel._MakeLevelProp_SystemName(m_Level, Rack.PROP_PALLET_CONFIGURATION);
					strPropSysName += _PalletIndex.ToString();

					string strError;
					m_Level.Owner.SetPropertyValue(strPropSysName, value, true, true, true, out strError);
				}
			}
		}
		public void Set_PalletConfiguration(PalletConfiguration pc)
		{
			// apply pallet configuration properties
			if (pc == null)
				m_PalletConfigurationGUID = Guid.Empty;
			else
			{
				m_PalletConfigurationGUID = pc.GUID;
				Set_Length((UInt32)Utils.GetWholeNumber(pc.Length), false);
				Set_Width((UInt32)Utils.GetWholeNumber(pc.Width), false);
				Set_Height((UInt32)Utils.GetWholeNumber(pc.Height), false);
				Set_Load((UInt32)Utils.GetWholeNumber(pc.Capacity), false);
			}

			NotifyPropertyChanged(() => PalletConfiguration);
		}

		//=============================================================================
		private UInt32 m_Length = 1000;
		public UInt32 Length
		{
			get { return m_Length; }
			set
			{
				if (m_Level != null && m_Level.Owner != null)
				{
					string strPropSysName = RackLevel._MakeLevelProp_SystemName(m_Level, Rack.PROP_PALLET_LENGTH);
					strPropSysName += _PalletIndex.ToString();

					string strError;
					m_Level.Owner.SetPropertyValue(strPropSysName, value, true, true, true, out strError);
				}
			}
		}
		public void Set_Length(UInt32 length, bool bRemovePalletConfig = true)
		{
			if (length != m_Length)
			{
				m_Length = length;
				_MarkStateChanged();
			}
			if (bRemovePalletConfig)
				Set_PalletConfiguration(null);
			NotifyPropertyChanged(() => Length);
		}

		//=============================================================================
		private UInt32 m_Width = 1200;
		public UInt32 Width
		{
			get { return m_Width; }
			set
			{
				if (m_Level != null && m_Level.Owner != null)
				{
					string strPropSysName = RackLevel._MakeLevelProp_SystemName(m_Level, Rack.PROP_PALLET_DEPTH);
					strPropSysName += _PalletIndex.ToString();

					string strError;
					m_Level.Owner.SetPropertyValue(strPropSysName, value, true, true, true, out strError);
				}
			}
		}
		public void Set_Width(UInt32 palletWidth, bool bRemovePalletConfig = true)
		{
			if (palletWidth != m_Width)
			{
				m_Width = palletWidth;
				_MarkStateChanged();
			}
			if (bRemovePalletConfig)
				Set_PalletConfiguration(null);
			NotifyPropertyChanged(() => Width);
		}

		//=============================================================================
		// The last level can have different level and pallet height
		private UInt32 m_Height = 900;
		public static UInt32 MAX_HEIGHT = 3000;
		public UInt32 Height
		{
			get { return m_Height; }
			set
			{
				if (m_Level != null && m_Level.Owner != null)
				{
					string strPropSysName = RackLevel._MakeLevelProp_SystemName(m_Level, Rack.PROP_PALLET_HEIGHT);
					strPropSysName += _PalletIndex.ToString();

					string strError;
					m_Level.Owner.SetPropertyValue(strPropSysName, value, true, true, true, out strError);
				}
			}
		}
		public void Set_Height(UInt32 palletHeight, bool bRemovePalletConfig = true)
		{
			if (palletHeight != m_Height)
			{
				m_Height = palletHeight;
				_MarkStateChanged();
			}
			if (bRemovePalletConfig)
				Set_PalletConfiguration(null);
			NotifyPropertyChanged(() => Height);
		}

		//=============================================================================
		private UInt32 m_Load = 1000;
		public UInt32 Load
		{
			get { return m_Load; }
			set
			{
				if (m_Level != null && m_Level.Owner != null)
				{
					string strPropSysName = RackLevel._MakeLevelProp_SystemName(m_Level, Rack.PROP_PALLET_LOAD);
					strPropSysName += _PalletIndex.ToString();

					string strError;
					m_Level.Owner.SetPropertyValue(strPropSysName, value, true, true, true, out strError);
				}
			}
		}
		public void Set_Load(UInt32 palletLoad, bool bRemovePalletConfig = true)
		{
			uint newPalletLoad = palletLoad;

			// check document PalletTruckMaxCapacity, new pallet load cant be greater than it
			if (this.Level != null
				&& this.Level.Owner != null
				&& this.Level.Owner.Sheet != null
				&& this.Level.Owner.Sheet.Document != null)
			{
				double palletTruckMaxCapacity = this.Level.Owner.Sheet.Document.PalletTruckMaxCapacity;
				if (!double.IsNaN(palletTruckMaxCapacity) && !double.IsInfinity(palletTruckMaxCapacity))
				{
					if (Utils.FGT(newPalletLoad, palletTruckMaxCapacity))
						newPalletLoad = (uint)Utils.GetWholeNumber(palletTruckMaxCapacity);
				}
			}

			if(newPalletLoad != m_Load)
			{
				m_Load = newPalletLoad;
				_MarkStateChanged();
			}

			if (bRemovePalletConfig)
				Set_PalletConfiguration(null);

			NotifyPropertyChanged(() => Load);
		}

		#endregion

		#region Public functions

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new Pallet(this);
		}

		#endregion

		#region Protected functions

		//=============================================================================
		private void _MarkStateChanged()
		{
			if(m_Level != null
				&& m_Level.Owner != null)
			{
				m_Level.Owner.MarkStateChanged();
			}
		}

		#endregion

		#region Serialization

		//=============================================================================
		// 1.1 Add PalletConfiguration.
		protected static string _sPallet_strMajor = "Pallet_MAJOR";
		protected static int _sPallet_MAJOR = 1;
		protected static string _sPallet_strMinor = "Pallet_MINOR";
		protected static int _sRPallet_MINOR = 1;
		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sPallet_strMajor, _sPallet_MAJOR);
			info.AddValue(_sPallet_strMinor, _sRPallet_MINOR);

			//
			info.AddValue("Length", m_Length);
			info.AddValue("Width", m_Width);
			info.AddValue("Height", m_Height);
			info.AddValue("Load", m_Load);

			// 1.1
			info.AddValue("PalletConfigurationGUID", m_PalletConfigurationGUID);
		}
		//=============================================================================
		public Pallet(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sPallet_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sPallet_strMinor, typeof(int));
			if (iMajor > _sPallet_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sPallet_MAJOR && iMinor > _sRPallet_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sPallet_MAJOR)
			{
				//
				m_Length = (UInt32)info.GetValue("Length", typeof(UInt32));
				m_Width = (UInt32)info.GetValue("Width", typeof(UInt32));
				m_Height = (UInt32)info.GetValue("Height", typeof(UInt32));
				m_Load = (UInt32)info.GetValue("Load", typeof(UInt32));

				if (iMajor >= 1 && iMinor >= 1)
					m_PalletConfigurationGUID = (Guid)info.GetValue("PalletConfigurationGUID", typeof(Guid));
			}
			else
				++DrawingDocument._sBiggerMajorNumber;
		}
		//=============================================================================
		public virtual void OnDeserialization(object sender) { }

		#endregion
	}

	[Serializable]
	public class RackLevel : ISerializable, IDeserializationCallback, IClonable
	{
		public RackLevel(Rack _owner)
		{
			Owner = _owner;
			Index = 0;

			m_Accessories = new RackLevelAccessories(this);
			Set_NumberOfPallets(2, false);
		}
		public RackLevel(RackLevel rackLevel)
		{
			this.Index = rackLevel.Index;
			this.m_IsSelected = rackLevel.m_IsSelected;
			this.m_LevelHeight = rackLevel.m_LevelHeight;
			this.m_bPalletsAreEqual = rackLevel.m_bPalletsAreEqual;

			// clone pallets
			if(rackLevel.m_Pallets != null)
			{
				foreach(Pallet pallet in rackLevel.m_Pallets)
				{
					if (pallet == null)
						continue;

					Pallet palletClone = pallet.Clone() as Pallet;
					if (palletClone == null)
						continue;

					palletClone.Level = this;
					m_Pallets.Add(palletClone);
				}
			}

			// clone accessories
			if(rackLevel.m_Accessories != null)
				this.m_Accessories = rackLevel.m_Accessories.Clone() as RackLevelAccessories;
			if (this.m_Accessories != null)
				this.m_Accessories.Owner = this;

			// clone beam
			if(rackLevel.m_Beam != null)
				this.m_Beam = rackLevel.m_Beam.Clone() as RackLevelBeam;
			if (this.m_Beam != null)
				this.m_Beam.OwnerLevel = this;
		}

		#region Properties

		//=============================================================================
		private Rack m_Owner = null;
		public Rack Owner
		{
			get { return m_Owner; }
			set
			{
				m_Owner = value;
			}
		}

		//=============================================================================
		private bool m_IsSelected = false;
		public bool IsSelected
		{
			get { return m_IsSelected; }
			set
			{
				// Create 2 racks with different indexes. Select one of them and go to Advanced properties.
				// There is selected level. Click on another rack. UI set previous rack selected level to false.
				// Have no idea what going on, but it is not correct.
				// If this rack is not selected disable changing selected level.
				if (m_Owner != null && m_Owner.IsSelected)
				{
					if (m_IsSelected != value)
					{
						m_IsSelected = value;
					}
				}
			}
		}

		//=============================================================================
		// Index of level.
		// Ground level has index 0.
		// Levels with beam have index >= 1.
		public UInt32 Index { get; set; }

		//=============================================================================
		public string DisplayName
		{
			get
			{
				if (Index > 0)
					return Index.ToString();
				else if (Index == 0)
					return "GR";

				return "123";
			}
		}

		//=============================================================================
		public bool IsItLastLevel
		{
			get
			{
				if (m_Owner != null && m_Owner.Levels != null && m_Owner.Levels.Contains(this))
					return m_Owner.Levels.LastOrDefault() == this;

				return false;
			}
		}

		//=============================================================================
		// Load of children Pallets.
		public UInt32 LevelLoad
		{
			get
			{
				UInt32 levelLoad = 0;

				if(Pallets != null)
				{
					foreach(Pallet pallet in Pallets)
					{
						if (pallet == null)
							continue;

						levelLoad += pallet.Load;
					}
				}

				return levelLoad;
			}
			set
			{
				if (m_Owner != null)
				{
					string strPropSysName = _MakeLevelProp_SystemName(this, Rack.PROP_LEVEL_LOAD);

					string strError;
					m_Owner.SetPropertyValue(strPropSysName, value, true, true, true, out strError);
				}
			}
		}
		public void Set_LevelLoad(UInt32 levelLoad)
		{
			if (Pallets == null || Pallets.Count == 0)
				return;

			// split level load between pallets
			double palletLoad = (double)levelLoad / Pallets.Count;
			int iPalletLoad = Utils.GetWholeNumber(palletLoad);
			foreach (Pallet pallet in Pallets)
			{
				if (pallet == null)
					continue;

				pallet.Set_Load((UInt32)iPalletLoad);
			}
		}

		//=============================================================================
		// NUmber of pallets value
		public UInt32 NumberOfPallets
		{
			get { return (UInt32)m_Pallets.Count; }
			set
			{
				if (value > 0)
				{
					if (m_Owner != null)
					{
						string strPropSysName = _MakeLevelProp_SystemName(this, Rack.PROP_NUMBER_OF_PALLETS);

						string strError;
						m_Owner.SetPropertyValue(strPropSysName, value, true, true, true, out strError);
					}
				}
			}
		}
		public void Set_NumberOfPallets(UInt32 numberOfPallets, bool bMarkStateChanged = true)
		{
			if (m_Pallets == null || m_Pallets.Count == numberOfPallets)
				return;

			if (numberOfPallets < m_Pallets.Count)
			{
				// delete last
				List<Pallet> palletsForDeleteList = new List<Pallet>();
				for (int i = (int)numberOfPallets; i < m_Pallets.Count; ++i)
					palletsForDeleteList.Add(m_Pallets[i]);
				//
				foreach (Pallet pallet in palletsForDeleteList)
					m_Pallets.Remove(pallet);

				if(bMarkStateChanged)
					_MarkStateChanged();
			}
			else if (numberOfPallets > m_Pallets.Count)
			{
				int newPalletsCount = (int)numberOfPallets - m_Pallets.Count;

				Pallet prevPallet = null;
				if (m_Pallets.Count > 0)
					prevPallet = m_Pallets[m_Pallets.Count - 1];
				else
					prevPallet = new Pallet(this);

				for (int i = 1; i <= newPalletsCount; ++i)
				{
					Pallet newPallet = prevPallet.Clone() as Pallet;
					if (newPallet == null)
						continue;

					newPallet.Level = this;
					m_Pallets.Add(newPallet);
				}

				if(bMarkStateChanged)
					_MarkStateChanged();
			}
		}
		//=============================================================================
		// Items source for pallets number combobox.
		private List<UInt32> m_NumberOfPalletsValues = new List<uint> { 1, 2, 3 };
		public List<UInt32> NumberOfPalletsValues { get { return m_NumberOfPalletsValues; } }
		//=============================================================================
		// Collection with pallets on this level
		private ObservableCollection<Pallet> m_Pallets = new ObservableCollection<Pallet>();
		public ObservableCollection<Pallet> Pallets { get { return m_Pallets; } }
		//=============================================================================
		/// <summary>
		/// View for Pallets collection.
		/// </summary>
		private ICollectionView m_PalletsCollectionView = null;
		public ICollectionView PalletsCollectionView
		{
			get
			{
				// Initialize pallets collection view.
				if(m_PalletsCollectionView == null)
				{
					m_PalletsCollectionView = CollectionViewSource.GetDefaultView(m_Pallets);
					if (m_PalletsCollectionView != null)
					{
						m_PalletsCollectionView.SortDescriptions.Add(new SortDescription("_PalletIndex", ListSortDirection.Ascending));
						m_PalletsCollectionView.Filter += new Predicate<object>(PalletsCollectionViewFilter);
					}
				}

				return m_PalletsCollectionView;
			}
		}

		//=============================================================================
		// True if pallets have different properties - length\width\height\PalletConfiguration.
		// PalletsAreEqual doesnt considered.
		// This property is used when user changes PalletsAreEqual property and need to make a decision - 
		// should pallets be recalculated. Need to recalculate pallets only if they have different properties.
		public bool PalletsHaveDifferentProperties
		{
			get
			{
				if(m_Pallets != null && m_Pallets.Count > 1)
				{
					for(int i=0; i<=m_Pallets.Count-2; ++i)
					{
						Pallet pallet1 = m_Pallets[i];
						Pallet pallet2 = m_Pallets[i+1];

						if (pallet1 == null || pallet2 == null)
							return true;

						// check pallets properties
						if(pallet1.Length != pallet2.Length
							|| pallet1.Width != pallet2.Width
							|| pallet1.Height != pallet2.Height
							|| pallet1.PalletConfiguration != pallet2.PalletConfiguration)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		//=============================================================================
		/// <summary>
		/// Returns height of the biggest pallet with ForkEntryBar(if it is checked).
		/// </summary>
		public UInt32 TheBiggestPalletHeightWithRiser
		{
			get
			{
				UInt32 maxPalletHeightWithRiser = 0;

				if (Pallets != null)
				{
					foreach (Pallet pallet in Pallets)
					{
						if (pallet == null)
							continue;

						UInt32 palletHeightWithRiser = pallet.Height;
						if (this.Accessories != null && this.Accessories.ForkEntryBar)
							palletHeightWithRiser += (UInt32)Utils.GetWholeNumber(Rack.PALLET_RISER_HEIGHT);

						if (palletHeightWithRiser > maxPalletHeightWithRiser)
							maxPalletHeightWithRiser = palletHeightWithRiser;
					}
				}

				return maxPalletHeightWithRiser;
			}
		}

		//=============================================================================
		// If true then all pallets on this level are equal.
		private bool m_bPalletsAreEqual = true;
		public bool PalletsAreEqual
		{
			get { return m_bPalletsAreEqual; }
			set
			{
				if(m_Owner != null)
				{
					string strPropSysName = _MakeLevelProp_SystemName(this, Rack.PROP_PALLETS_ARE_EQUAL);

					string strError;
					m_Owner.SetPropertyValue(strPropSysName, value, true, true, true, out strError);
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// Distance from the ground to the bot of this level beam.
		/// </summary>
		public double DistanceFromTheGround
		{
			get
			{
				if (Owner != null && Owner.Levels != null && Owner.Levels.Contains(this))
				{
					// If it is ground level then return 0.
					if (this.Index == 0)
						return 0.0;

					double distance = 0.0;
					if (Owner.IsUnderpassAvailable)
						distance = Owner.Underpass;
					else if (Owner.IsMaterialOnGround)
						distance = 0.0;

					foreach (RackLevel level in Owner.Levels)
					{
						if (level == null)
							continue;

						if (level == this)
						{
							// If IsUnderpassAvailable and IsMaterialOnGround are unchecked
							// and this level is first, then need to calculate distance to the ground here.
							// Otherwise it will be 0.
							if (!Owner.IsUnderpassAvailable && !Owner.IsMaterialOnGround && level.Index == 1)
							{
								distance += Rack.sFirstLevelOffset;
								// Remove beam height
								if (level.Beam != null)
									distance -= level.Beam.Height;
							}

							break;
						}

						// Read comment to Rack.sFirstLevelOffset.
						if (!Owner.IsUnderpassAvailable && !Owner.IsMaterialOnGround && level.Index == 1)
							distance += Rack.sFirstLevelOffset;
						else if (level.Index != 0 && level.Beam != null)
							distance += level.Beam.Height;

						distance += level.LevelHeight;
					}

					return distance;
				}

				return 0.0;
			}
		}


		//=============================================================================
		private RackLevelBeam m_Beam = null;
		public RackLevelBeam Beam
		{
			get
			{
				// ground level doesnt have any beam
				if (Index == 0)
					return null;

				return m_Beam;
			}
			set
			{
				if (Index != 0 && m_Beam != value)
				{
					m_Beam = value;
					_MarkStateChanged();
				}
			}
		}

		//=============================================================================
		public UInt32 MinLevelHeight
		{
			get
			{
				// If it is the last level then use 
				if (m_Owner != null && m_Owner.Levels != null && m_Owner.Levels.LastOrDefault() == this)
					return (UInt32)Rack.LAST_LEVEL_MIN_HEIGHT;

				return (UInt32)Rack.MIN_LEVEL_HEIGHT;
			}
		}
		//=============================================================================
		public UInt32 MaxLevelHeight
		{
			get
			{
				// If it is the last level then use Rack.LAST_LEVEL_MAX_HEIGHT.
				if (m_Owner != null && m_Owner.Levels != null && m_Owner.Levels.LastOrDefault() == this)
					return (UInt32)Rack.LAST_LEVEL_MAX_HEIGHT;

				// USL distance(beam top to top) cant be greater than Rack.MAX_USL_DISTANCE.
				double maxUSL = Rack.MAX_USL_DISTANCE;
				if (this.Owner != null && this.Owner.Levels != null)
				{
					int iCurrLevelIndex = this.Owner.Levels.IndexOf(this);
					if (iCurrLevelIndex >= 0)
					{
						int iNextLevelIndex = iCurrLevelIndex + 1;
						if (iNextLevelIndex < this.Owner.Levels.Count)
						{
							RackLevel nextLevel = this.Owner.Levels[iNextLevelIndex];
							if (nextLevel != null && nextLevel.Beam != null)
								maxUSL -= nextLevel.Beam.Height;
						}
					}
				}

				return (UInt32)Utils.GetWholeNumber(maxUSL);
			}
		}
		//=============================================================================
		/// <summary>
		/// Levels height consists of max pallet height + distance to the next level beam.
		/// It doesnt contain level beam height.
		/// </summary>
		private UInt32 m_LevelHeight = 1000;
		public UInt32 LevelHeight
		{
			get { return m_LevelHeight; }
			set
			{
				if (m_Owner != null)
				{
					string strPropSysName = _MakeLevelProp_SystemName(this, Rack.PROP_LEVEL_HEIGHT);

					string strError;
					m_Owner.SetPropertyValue(strPropSysName, value, true, true, true, out strError);
				}
			}
		}

		//=============================================================================
		private RackLevelAccessories m_Accessories = null;
		public RackLevelAccessories Accessories
		{
			get { return m_Accessories; }
			set
			{
				if (m_Owner != null)
				{
					string strPropSysName = _MakeLevelProp_SystemName(this, Rack.PROP_LEVEL_ACCESSORIES);

					string strError;
					m_Owner.SetPropertyValue(strPropSysName, value, true, true, true, out strError);
				}
			}
		}

		#endregion

		#region Public functions

		public string[] GetAccessoriesDescription()
		{
			List<string> accessories = new List<string>();

			if (Accessories != null)
			{
				if (this.Accessories.IsDeckPlateAvailable)
				{
					if (eDeckPlateType.eAlongDepth_UDL == this.Accessories.DeckPlateType)
						accessories.Add(RackLevelAccessories.DECKING_PANEL_6BP_SHELVING_SHORT);
					else if (eDeckPlateType.eAlongDepth_PalletSupport == this.Accessories.DeckPlateType)
						accessories.Add(RackLevelAccessories.DECKING_PANEL_6BP_PALLET_SHORT);
					else if (eDeckPlateType.eAlongLength == this.Accessories.DeckPlateType)
						accessories.Add(RackLevelAccessories.DECKING_PANEL_4BP_SHORT);
				}

				if (this.Accessories.PalletStopper)
					accessories.Add(RackLevelAccessories.PALLET_STOPPER_SHORT);

				if (this.Accessories.ForkEntryBar)
					accessories.Add(RackLevelAccessories.FORK_ENTRY_BAR_SHORT);

				if (this.Accessories.PalletSupportBar)
					accessories.Add(RackLevelAccessories.PALLET_SUPPORT_BAR_SHORT);

				if (this.Accessories.GuidedTypePalletSupport)
				{
					if (this.Accessories.GuidedTypePalletSupport_WithPSB && this.Accessories.GuidedTypePalletSupport_WithStopper)
						accessories.Add(RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER_AND_PSB_SHORT);
					else if (this.Accessories.GuidedTypePalletSupport_WithPSB)
						accessories.Add(RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_PSB_SHORT);
					else if (this.Accessories.GuidedTypePalletSupport_WithStopper)
						accessories.Add(RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER_SHORT);
					else
						accessories.Add(RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_PSB_SHORT);
				}
			}

			return accessories.ToArray();
		}

		//=============================================================================
		public void Set_PalletsAreEqual(bool bPalletsAreEqual)
		{
			if (bPalletsAreEqual != m_bPalletsAreEqual)
			{
				m_bPalletsAreEqual = bPalletsAreEqual;
				_MarkStateChanged();

				_RebuildPalletsCollection();
			}
		}

		//=============================================================================
		public void Set_PalletLength(UInt32 _palletIndex, UInt32 _palletLength)
		{
			if (m_Pallets == null || m_Pallets.Count == 0)
				return;

			if (_palletIndex >= 0 && _palletIndex < m_Pallets.Count)
				m_Pallets[(int)_palletIndex].Set_Length(_palletLength);

			_RebuildPalletsCollection();
		}
		//=============================================================================
		public void Set_PalletWidth(UInt32 _palletIndex, UInt32 _palletWidth)
		{
			if (m_Pallets == null || m_Pallets.Count == 0)
				return;

			if (_palletIndex >= 0 && _palletIndex < m_Pallets.Count)
				m_Pallets[(int)_palletIndex].Set_Width(_palletWidth);

			_RebuildPalletsCollection();
		}

		//=============================================================================
		public void Set_PalletHeight(UInt32 palletIndex, UInt32 palletHeight)
		{
			if (m_Pallets == null || m_Pallets.Count == 0)
				return;

			if (palletIndex >= 0 && palletIndex < m_Pallets.Count)
				m_Pallets[(int)palletIndex].Set_Height(palletHeight);

			_RebuildPalletsCollection();
		}
		//=============================================================================
		public void Set_PalletLoad(UInt32 _palletIndex, UInt32 _palletLoad)
		{
			if (m_Pallets == null || m_Pallets.Count == 0)
				return;

			if (_palletIndex >= 0 && _palletIndex < m_Pallets.Count)
				m_Pallets[(int)_palletIndex].Set_Load(_palletLoad);

			_RebuildPalletsCollection();
		}
		//=============================================================================
		public bool Set_PalletConfiguration(UInt32 _palletIndex, PalletConfiguration pc)
		{
			if (m_Pallets == null || m_Pallets.Count == 0)
				return false;

			if (_palletIndex >= 0 && _palletIndex < m_Pallets.Count)
				m_Pallets[(int)_palletIndex].Set_PalletConfiguration(pc);

			// level height should satisfy rules defined in Rack._RecalcLevelHeight() function.
			string strError;
			if (!this.RecalcLevelHeight(out strError))
				return false;

			_RebuildPalletsCollection();

			return true;
		}

		//=============================================================================
		public void Set_LevelHeight(UInt32 levelHeight)
		{
			if (levelHeight != m_LevelHeight)
			{
				m_LevelHeight = levelHeight;
				_MarkStateChanged();
			}
		}
		//=============================================================================
		public bool RecalcLevelHeight(out string strError)
		{
			// Level height should satisfy rules in Rack._RecalcLevelHeight() function.

			strError = string.Empty;
			if (m_Owner == null || m_Owner.Levels == null)
				return false;

			// If it is not the last level.
			// Height of the last level doesnt depend on the pallet height
			if (this != m_Owner.Levels.LastOrDefault())
			{
				UInt32 newLevelHeight = TheBiggestPalletHeightWithRiser;
				newLevelHeight += Rack.sDistanceBetweenPalletAndLevel;
				// Dont add PALLET_RISER_HEIGHT because it is already included in TheBiggestPalletHeightWithRiser.
				//
				//// If LevelAccessories.PalletRiser is checked then add PALLET_RISER_HEIGHT between level and pallets.
				//if (Accessories != null && Accessories.PalletRiser)
				//	newLevelHeight += (UInt32)Utils.GetWholeNumber(Rack.PALLET_RISER_HEIGHT);
				//
				int iNextLevelBeamHeight = 0;
				int iNextLevelIndex = m_Owner.Levels.IndexOf(this) + 1;
				if (iNextLevelIndex < m_Owner.Levels.Count)
				{
					RackLevel nextLevel = m_Owner.Levels[iNextLevelIndex];
					if (nextLevel != null && nextLevel.Beam != null)
						iNextLevelBeamHeight = Utils.GetWholeNumber(nextLevel.Beam.Height);
				}
				// Check rules in Rack._RecalcLevelHeight() function.
				int iMultiplicityPartsCount = (int)Math.Ceiling((double)(newLevelHeight + iNextLevelBeamHeight) / Rack.LEVEL_HEIGHT_MULTIPLICITY);
				newLevelHeight = (UInt32)((iMultiplicityPartsCount * Rack.LEVEL_HEIGHT_MULTIPLICITY) - iNextLevelBeamHeight);
				//
				if (this.Index == 0)
					newLevelHeight += 12;

				if (newLevelHeight < this.MinLevelHeight)
				{
					strError += "Calculated level height(";
					strError += newLevelHeight.ToString();
					strError += ") is less than min level height value(";
					strError += this.MinLevelHeight.ToString();
					strError += ").";

					return false;
				}
				else if (newLevelHeight > this.MaxLevelHeight)
				{
					strError += "Calculated level height(";
					strError += newLevelHeight.ToString();
					strError += ") is bigger than max level height value(";
					strError += this.MaxLevelHeight.ToString();
					strError += ").";

					return false;
				}

				this.Set_LevelHeight(newLevelHeight);
				return true;
			}

			return true;
		}

		//=============================================================================
		public void Set_Accessories(RackLevelAccessories _accessories) { m_Accessories = _accessories; }

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new RackLevel(this);
		}

		#endregion

		#region Private functions

		//=============================================================================
		// Remove all pallets from collection and add them later.
		// It changes collection and controls, bound to this collection will update their state.
		//
		// 1. Go to the advanced properties.
		// 2. Click "Unbind pallet configuration"
		// 3. Command_UnbindPalletConfiguration is executed, but without _RebuildPalletsCollection() commands list will
		//    be not updated.
		private void _RebuildPalletsCollection()
		{
			if (m_Pallets == null)
				return;

			List<Pallet> pallets = m_Pallets.ToList();
			//
			m_Pallets.Clear();
			foreach (Pallet p in pallets)
			{
				if (p == null)
					continue;

				m_Pallets.Add(p);
			}
		}

		//=============================================================================
		private bool PalletsCollectionViewFilter(object obj)
		{
			if (obj == null)
				return false;

			Pallet pallet = obj as Pallet;
			if (pallet == null)
				return false;

			if (pallet.Level == null)
				return false;

			if(pallet.Level.PalletsAreEqual)
			{
				// if all pallets are equal then display only first pallet
				if (pallet._PalletIndex == 0)
					return true;
				return false;
			}

			return true;
		}

		//=============================================================================
		private void _MarkStateChanged()
		{
			if (m_Owner != null)
				m_Owner.MarkStateChanged();
		}

		#endregion

		#region Serialization

		//=============================================================================
		// 2.0 Remove PalletsHasSameLength, PalletsHasSameWidth, PalletsHasSameHeight, PalletsHasSameLoad properties
		// 3.0 Remove m_LevelLoad, now it is calculated from pallets load.
		protected static string _sRackLevel_strMajor = "RackLevel_MAJOR";
		protected static int _sRackLevel_MAJOR = 3;
		protected static string _sRackLevel_strMinor = "RackLevel_MINOR";
		protected static int _sRackLevel_MINOR = 0;
		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sRackLevel_strMajor, _sRackLevel_MAJOR);
			info.AddValue(_sRackLevel_strMinor, _sRackLevel_MINOR);

			//
			info.AddValue("Index", Index);
			info.AddValue("IsSelected", m_IsSelected);
			// pallet
			info.AddValue("Pallets", m_Pallets);
			// level
			// Removed in 3.0
			//info.AddValue("LevelLoad", m_LevelLoad);
			info.AddValue("LevelHeight", m_LevelHeight);
			//
			info.AddValue("Accessories", m_Accessories);
			//
			info.AddValue("Beam", m_Beam);

			// 2.0
			info.AddValue("PalletsAreEqual", m_bPalletsAreEqual);
		}
		//=============================================================================
		public RackLevel(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sRackLevel_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sRackLevel_strMinor, typeof(int));
			if (iMajor > _sRackLevel_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sRackLevel_MAJOR && iMinor > _sRackLevel_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sRackLevel_MAJOR)
			{
				Index = (UInt32)info.GetValue("Index", typeof(UInt32));
				m_IsSelected = (bool)info.GetValue("IsSelected", typeof(bool));
				// pallet
				m_Pallets = (ObservableCollection<Pallet>)info.GetValue("Pallets", typeof(ObservableCollection<Pallet>));
				// level
				// Removed in 3.0
				//m_LevelLoad = (UInt32)info.GetValue("LevelLoad", typeof(UInt32));
				m_LevelHeight = (UInt32)info.GetValue("LevelHeight", typeof(UInt32));
				//
				m_Accessories = (RackLevelAccessories)info.GetValue("Accessories", typeof(RackLevelAccessories));
				//
				m_Beam = (RackLevelBeam)info.GetValue("Beam", typeof(RackLevelBeam));

				if (iMajor >= 2 && iMinor >= 0)
					m_bPalletsAreEqual = (bool)info.GetValue("PalletsAreEqual", typeof(bool));
				else
					m_bPalletsAreEqual = false;
			}
			else
				++DrawingDocument._sBiggerMajorNumber;
		}
		//=============================================================================
		public virtual void OnDeserialization(object sender)
		{
			if (m_Accessories != null)
				m_Accessories.Owner = this;

			if (m_Beam != null)
				m_Beam.OwnerLevel = this;

			if (m_Pallets != null)
			{
				foreach (Pallet pallet in m_Pallets)
				{
					if (pallet == null)
						continue;

					pallet.Level = this;
				}
			}
		}

		#endregion

		public static string _MakeLevelProp_SystemName(RackLevel _level, string strPropSystemName)
		{
			string strResult = string.Empty;

			if (_level != null && !string.IsNullOrEmpty(strPropSystemName))
			{
				strResult = Rack.PROP_RACK_LEVEL;
				strResult += Rack.PROP_RACK_LEVEL_DELIMITER;
				strResult += _level.Index;
				strResult += Rack.PROP_RACK_LEVEL_DELIMITER;
				strResult += strPropSystemName;
			}

			return strResult;
		}
		public static bool _ParseLevelProp_SystemName(string strFullSystemName, out UInt32 index, out string strPropSystemName)
		{
			index = 0;
			strPropSystemName = string.Empty;

			if (string.IsNullOrEmpty(strFullSystemName))
				return false;
			if (!strFullSystemName.StartsWith(Rack.PROP_RACK_LEVEL))
				return false;

			string[] arr = strFullSystemName.Split(Rack.PROP_RACK_LEVEL_DELIMITER);
			if (arr.Count() != 3)
				return false;

			if (string.IsNullOrEmpty(arr[1]) || string.IsNullOrEmpty(arr[2]))
				return false;

			try
			{
				index = Convert.ToUInt32(arr[1]);
				strPropSystemName = arr[2];
			}
			catch
			{
				return false;
			}

			return true;
		}
	}

	// State of M-rack for rack's index calculating.
	[Serializable]
	public class M_RackState : ISerializable, IClonable
	{
		public M_RackState(
			double _GlobalLength,
			double _GlobalWidth,
			int _GlobalHeight,
			//
			bool _IsUnderpassAvailable,
			UInt32 _Underpass,
			//
			bool _IsMaterialOnGround,
			//
			bool _bShowPallet,
			//
			ObservableCollection<RackLevel> _Levels,
			//
			Guid _ColumnGUID,
			//
			bool _SplitColumn,
			UInt32 _Column_FirstPartLength,
			UInt32 _Column_SecondPartLength
			)
		{
			Length_X = _GlobalLength;
			Length_Y = _GlobalWidth;
			Length_Z = _GlobalHeight;
			//
			IsUnderpassAvailable = _IsUnderpassAvailable;
			Underpass = _Underpass;
			//
			IsMaterialOnGround = _IsMaterialOnGround;
			//
			ShowPallet = _bShowPallet;
			// Clone levels
			Levels = new ObservableCollection<RackLevel>();
			if(_Levels != null)
			{
				foreach(RackLevel level in _Levels)
				{
					if (level == null)
						continue;

					RackLevel levelClone = level.Clone() as RackLevel;
					if (levelClone == null)
						continue;

					this.Levels.Add(levelClone);
				}
			}
			//
			ColumnGUID = _ColumnGUID;
			//
			SplitColumn = _SplitColumn;
			Column_FirstPartLength = _Column_FirstPartLength;
			Column_SecondPartLength = _Column_SecondPartLength;
		}
		public M_RackState(M_RackState state)
		{
			this.Levels = new ObservableCollection<RackLevel>();

			if(state != null)
			{
				Length_X = state.Length_X;
				Length_Y = state.Length_Y;
				Length_Z = state.Length_Z;
				//
				IsUnderpassAvailable = state.IsUnderpassAvailable;
				Underpass = state.Underpass;
				//
				IsMaterialOnGround = state.IsMaterialOnGround;
				//
				ShowPallet = state.ShowPallet;
				// Clone levels
				Levels = new ObservableCollection<RackLevel>();
				if (state.Levels != null)
				{
					foreach (RackLevel level in state.Levels)
					{
						if (level == null)
							continue;

						RackLevel levelClone = level.Clone() as RackLevel;
						if (levelClone == null)
							continue;

						this.Levels.Add(levelClone);
					}
				}
				//
				ColumnGUID = state.ColumnGUID;
				//
				SplitColumn = state.SplitColumn;
				Column_FirstPartLength = state.Column_FirstPartLength;
				Column_SecondPartLength = state.Column_SecondPartLength;
			}
		}

		#region Properties

		//=============================================================================
		public double Length_X { get; private set; }
		//=============================================================================
		public double Length_Y { get; private set; }
		//=============================================================================
		public int Length_Z { get; private set; }
		//=============================================================================
		public bool IsUnderpassAvailable { get; private set; }
		//=============================================================================
		public UInt32 Underpass { get; private set; }
		//=============================================================================
		public bool IsMaterialOnGround { get; private set; }
		//=============================================================================
		public bool ShowPallet { get; private set; }
		//=============================================================================
		public ObservableCollection<RackLevel> Levels { get; private set; }
		//=============================================================================
		public Guid ColumnGUID { get; private set; }
		//=============================================================================
		public bool SplitColumn { get; private set; }
		//=============================================================================
		public UInt32 Column_FirstPartLength { get; private set; }
		//=============================================================================
		public UInt32 Column_SecondPartLength { get; private set; }

		#endregion

		#region Public functions

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new M_RackState(this);
		}

		#endregion

		#region Serialization

		//=============================================================================
		// 1.0
		// 1.1 Write\read ShowPallet
		// 2.1 Remove Clear Available Height, it is calculated based on the roof type and rack position.
		// 3.1 PalletType is removed from the Rack and placed to the DrawingDocument.
		// 4.1 Replace ColumnType with ColumnGUID
		protected static string _sM_RackState_strMajor = "M_RackState_MAJOR";
		protected static int _sM_RackState_MAJOR = 4;
		protected static string _sM_RackState_strMinor = "M_RackState_MINOR";
		protected static int _sM_RackState_MINOR = 1;
		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sM_RackState_strMajor, _sM_RackState_MAJOR);
			info.AddValue(_sM_RackState_strMinor, _sM_RackState_MINOR);

			//
			info.AddValue("Length_X", Length_X);
			info.AddValue("Length_Y", Length_Y);
			info.AddValue("Length_Z", Length_Z);
			//
			//info.AddValue("ClearAvailableHeight", ClearAvailableHeight);
			//
			info.AddValue("IsUnderpassAvailable", IsUnderpassAvailable);
			info.AddValue("Underpass", Underpass);
			//
			info.AddValue("IsMaterialOnGround", IsMaterialOnGround);
			info.AddValue("MaterialHeightOnGround", 0);
			info.AddValue("MaterialWeightOnGround", 0);
			// 3.1 Removed
			//info.AddValue("PalletType", PalletType);
			//
			info.AddValue("Levels", Levels);
			// Removed in 4.1
			//info.AddValue("ColumnType", ColumnType);
			info.AddValue("ColumnGUID", ColumnGUID);
			//
			info.AddValue("SplitColumn", SplitColumn);
			info.AddValue("Column_FirstPartLength", Column_FirstPartLength);
			info.AddValue("Column_SecondPartLength", Column_SecondPartLength);

			// 1.1
			info.AddValue("ShowPallet", ShowPallet);
		}
		//=============================================================================
		public M_RackState(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sM_RackState_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sM_RackState_strMinor, typeof(int));
			if (iMajor > _sM_RackState_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sM_RackState_MAJOR && iMinor > _sM_RackState_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sM_RackState_MAJOR)
			{
				try
				{
					//
					Length_X = (double)info.GetValue("Length_X", typeof(double));
					Length_Y = (double)info.GetValue("Length_Y", typeof(double));
					Length_Z = (int)info.GetValue("Length_Z", typeof(int));
					//
					//ClearAvailableHeight = (UInt32)info.GetValue("ClearAvailableHeight", typeof(UInt32));
					//
					IsUnderpassAvailable = (bool)info.GetValue("IsUnderpassAvailable", typeof(bool));
					Underpass = (UInt32)info.GetValue("Underpass", typeof(UInt32));
					//
					IsMaterialOnGround = (bool)info.GetValue("IsMaterialOnGround", typeof(bool));
					// 3.1 Removed
					//PalletType = (ePalletType)info.GetValue("PalletType", typeof(ePalletType));
					//
					Levels = (ObservableCollection<RackLevel>)info.GetValue("Levels", typeof(ObservableCollection<RackLevel>));
					// Remplaced in 4.1
					//ColumnType = (eColumnType)info.GetValue("ColumnType", typeof(eColumnType));
					ColumnGUID = (Guid)info.GetValue("ColumnGUID", typeof(Guid));
					//
					SplitColumn = (bool)info.GetValue("SplitColumn", typeof(bool));
					Column_FirstPartLength = (UInt32)info.GetValue("Column_FirstPartLength", typeof(UInt32));
					Column_SecondPartLength = (UInt32)info.GetValue("Column_SecondPartLength", typeof(UInt32));

					if (iMajor >= 1 && iMinor >= 1)
						ShowPallet = (bool)info.GetValue("ShowPallet", typeof(bool));
				}
				catch { }
			}
			else
				++DrawingDocument._sBiggerMajorNumber;
		}

		#endregion

		//=============================================================================
		public static bool operator ==(M_RackState left, M_RackState right)
		{
			return _AreEquals(left, right);
		}
		public static bool operator !=(M_RackState left, M_RackState right)
		{
			return !_AreEquals(left, right);
		}
		private static bool _AreEquals(M_RackState left, M_RackState right)
		{
			// Check for null values and compare run-time types.
			if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
				return false;

			//
			if (left.Length_X != right.Length_X)
				return false;
			if (left.Length_Y != right.Length_Y)
				return false;
			if (left.Length_Z != right.Length_Z)
				return false;
			//
			if (left.IsUnderpassAvailable != right.IsUnderpassAvailable)
				return false;
			// Compare underpass value only if it is checked.
			// Underpass value can be initialized using document settings(overall height).
			if (left.IsUnderpassAvailable && left.Underpass != right.Underpass)
				return false;
			//
			if (left.IsMaterialOnGround != right.IsMaterialOnGround)
				return false;
			//
			if (left.ShowPallet != right.ShowPallet)
				return false;
			//
			if (left.Levels != null)
			{
				if (right.Levels == null)
					return false;
			}
			else
			{
				if (right.Levels != null)
					return false;
			}
			if (left.Levels != null && left.Levels.Count != right.Levels.Count)
				return false;
			for (int i = 0; i < left.Levels.Count; ++i)
			{
				//
				RackLevel _leftLevel = left.Levels[i];
				RackLevel _rightLevel = right.Levels[i];

				if (_leftLevel == null && _rightLevel != null)
					return false;
				if (_leftLevel != null && _rightLevel == null)
					return false;

				if (_leftLevel != null)
				{
					if (left.ShowPallet)
					{
						if (_leftLevel.NumberOfPallets != _rightLevel.NumberOfPallets)
							return false;
						if (_leftLevel.Pallets == null || _rightLevel.Pallets == null)
							return false;
						for (int _PalletIndex = 0; _PalletIndex < _leftLevel.NumberOfPallets; ++_PalletIndex)
						{
							Pallet _leftLevelPallet = _leftLevel.Pallets[_PalletIndex];
							Pallet _rightLevelPallet = _rightLevel.Pallets[_PalletIndex];

							if (_leftLevelPallet == null || _rightLevelPallet == null)
								return false;

							if (_leftLevelPallet.Length != _rightLevelPallet.Length)
								return false;

							if (_leftLevelPallet.Width != _rightLevelPallet.Width)
								return false;

							if (_leftLevelPallet.Height != _rightLevelPallet.Height)
								return false;

							if (_leftLevelPallet.Load != _rightLevelPallet.Load)
								return false;
						}
					}
					else
					{
						if (_leftLevel.LevelHeight != _rightLevel.LevelHeight)
							return false;
						if (_leftLevel.LevelLoad != _rightLevel.LevelLoad)
							return false;
					}

					if (!RackLevelAccessories._AreEquals(_leftLevel.Accessories, _rightLevel.Accessories, left.ShowPallet))
						return false;
				}
			}

			//
			if (left.ColumnGUID != right.ColumnGUID)
				return false;
			//
			if (left.SplitColumn != right.SplitColumn)
				return false;
			if (left.SplitColumn)
			{
				if (left.Column_FirstPartLength != right.Column_FirstPartLength)
					return false;
				if (left.Column_SecondPartLength != right.Column_SecondPartLength)
					return false;
			}

			return true;
		}
	}

	/// <summary>
	/// Rack accessories which are shared between all racks in the document.
	/// </summary>
	[Serializable]
	public class RackAccessories : ISerializable, IDeserializationCallback, IClonable
	{
		public RackAccessories() { }
		public RackAccessories(RackAccessories rackAcc)
		{
			if(rackAcc != null)
			{
				this.m_UprightGuard = rackAcc.m_UprightGuard;

				this.m_IsHeavyDutyEnabled = rackAcc.m_IsHeavyDutyEnabled;

				this.m_RowGuard = rackAcc.m_RowGuard;
				this.m_Signages = rackAcc.m_Signages;

				this.m_bIsMeshCladdingEnabled = rackAcc.m_bIsMeshCladdingEnabled;
				this.m_MeshHeight = rackAcc.m_MeshHeight;

				this.m_IsSafetyPrecautionsEnabled = rackAcc.m_IsSafetyPrecautionsEnabled;
				this.m_SafetyPrecautionsQuantity = rackAcc.m_SafetyPrecautionsQuantity;
				this.m_IsSafeWorkingLoadsEnabled = rackAcc.m_IsSafeWorkingLoadsEnabled;
				this.m_SafeWorkingLoadsQuantity = rackAcc.m_SafeWorkingLoadsQuantity;

				this.m_IsMenaEnabled = rackAcc.m_IsMenaEnabled;
			}
		}

		#region Properties

		//=============================================================================
		private bool m_UprightGuard = false;
		public bool UprightGuard
		{
			get { return m_UprightGuard; }
			set { m_UprightGuard = value; }
		}
		//=============================================================================
		private bool m_RowGuard = false;
		public bool RowGuard
		{
			get { return m_RowGuard; }
			set { m_RowGuard = value; }
		}
		//=============================================================================
		/// <summary>
		/// Should be enabled only if RowGuard is enabled.
		/// </summary>
		private bool m_IsHeavyDutyEnabled = false;
		public bool IsHeavyDutyEnabled
		{
			get { return m_IsHeavyDutyEnabled; }
			set { m_IsHeavyDutyEnabled = value; }
		}
		//=============================================================================
		private bool m_Signages = false;
		public bool Signages
		{
			get { return m_Signages; }
			set { m_Signages = value; }
		}
		//=============================================================================
		private bool m_IsMenaEnabled = false;
		public bool IsMenaEnabled
		{
			get { return m_IsMenaEnabled; }
			set { m_IsMenaEnabled = value; }
		}
		//=============================================================================
		private bool m_bIsMeshCladdingEnabled = false;
		public bool IsMeshCladdingEnabled
		{
			get { return m_bIsMeshCladdingEnabled; }
			set { m_bIsMeshCladdingEnabled = value; }
		}
		//=============================================================================
		private double m_MeshHeight = 100;
		public double MeshHeight
		{
			get { return m_MeshHeight; }
			set
			{
				if (Utils.FGE(value, 0.0))
					m_MeshHeight = value;
			}
		}
		//=============================================================================
		private bool m_IsSafetyPrecautionsEnabled = false;
		public bool IsSafetyPrecautionsEnabled
		{
			get { return m_IsSafetyPrecautionsEnabled; }
			set { m_IsSafetyPrecautionsEnabled = value; }
		}
		//=============================================================================
		private int m_SafetyPrecautionsQuantity = 1;
		public int SafetyPrecautionsQuantity
		{
			get { return m_SafetyPrecautionsQuantity; }
			set { m_SafetyPrecautionsQuantity = value; }
		}
		//=============================================================================
		private bool m_IsSafeWorkingLoadsEnabled = false;
		public bool IsSafeWorkingLoadsEnabled
		{
			get { return m_IsSafeWorkingLoadsEnabled; }
			set { m_IsSafeWorkingLoadsEnabled = value; }
		}
		//=============================================================================
		private int m_SafeWorkingLoadsQuantity = 1;
		public int SafeWorkingLoadsQuantity
		{
			get { return m_SafeWorkingLoadsQuantity; }
			set { m_SafeWorkingLoadsQuantity = value; }
		}

		#endregion

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new RackAccessories(this);
		}

		#region Serialization

		//=============================================================================
		// 2.0 Change properties name. Remove properties.
		// 2.1 Add IsMeshCladdingEnabled and MeshHeight.
		// 2.2 Add m_IsSafetyPrecautionsEnabled, m_SafetyPrecautionsQuantity, m_IsSafeWorkingLoadsEnabled and m_SafeWorkingLoadsQuantity
		// 2.3 Add m_IsMenaEnabled
		// 2.4 Add m_IsHeavyDutyEnabled
		protected static string _sRackAccessories_strMajor = "RackAccessories_MAJOR";
		protected static int _sRackAccessories_MAJOR = 2;
		protected static string _sRackAccessories_strMinor = "RackAccessories_MINOR";
		protected static int _sRackAccessories_MINOR = 4;
		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sRackAccessories_strMajor, _sRackAccessories_MAJOR);
			info.AddValue(_sRackAccessories_strMinor, _sRackAccessories_MINOR);

			// 2.0
			info.AddValue("UprightGuard", m_UprightGuard);

			info.AddValue("RowGuard", m_RowGuard);
			info.AddValue("Signages", m_Signages);

			// 2.1
			info.AddValue("IsMeshCladdingEnabled", m_bIsMeshCladdingEnabled);
			info.AddValue("MeshHeight", m_MeshHeight);

			// 2.2
			info.AddValue("IsSafetyPrecautionsEnabled", m_IsSafetyPrecautionsEnabled);
			info.AddValue("SafetyPrecautionsQuantity", m_SafetyPrecautionsQuantity);
			info.AddValue("IsSafeWorkingLoadsEnabled", m_IsSafeWorkingLoadsEnabled);
			info.AddValue("SafeWorkingLoadsQuantity", m_SafeWorkingLoadsQuantity);

			// 2.3
			info.AddValue("IsMenaEnabled", m_IsMenaEnabled);

			// 2.4
			info.AddValue("IsHeavyDutyEnabled", m_IsHeavyDutyEnabled);
		}
		//=============================================================================
		public RackAccessories(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sRackAccessories_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sRackAccessories_strMinor, typeof(int));
			if (iMajor > _sRackAccessories_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sRackAccessories_MAJOR && iMinor > _sRackAccessories_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sRackAccessories_MAJOR)
			{
				if(iMajor < 2 && iMinor == 0)
				{
					m_UprightGuard = (bool)info.GetValue("CTypeColumnGuards", typeof(bool));

					m_RowGuard = (bool)info.GetValue("RowGuard", typeof(bool));
					m_Signages = (bool)info.GetValue("NamePlate", typeof(bool));
				}

				if (iMajor >= 2 && iMinor >= 0)
				{
					m_UprightGuard = (bool)info.GetValue("UprightGuard", typeof(bool));

					m_RowGuard = (bool)info.GetValue("RowGuard", typeof(bool));
					m_Signages = (bool)info.GetValue("Signages", typeof(bool));
				}

				if (iMajor >= 2 && iMinor >= 1)
				{
					m_bIsMeshCladdingEnabled = (bool)info.GetValue("IsMeshCladdingEnabled", typeof(bool));
					m_MeshHeight = (double)info.GetValue("MeshHeight", typeof(double));
				}

				if(iMajor >= 2 && iMinor >= 2)
				{
					m_IsSafetyPrecautionsEnabled = (bool)info.GetValue("IsSafetyPrecautionsEnabled", typeof(bool));
					m_SafetyPrecautionsQuantity = (int)info.GetValue("SafetyPrecautionsQuantity", typeof(int));
					m_IsSafeWorkingLoadsEnabled = (bool)info.GetValue("IsSafeWorkingLoadsEnabled", typeof(bool));
					m_SafeWorkingLoadsQuantity = (int)info.GetValue("SafeWorkingLoadsQuantity", typeof(int));
				}

				if (iMajor >= 2 && iMinor >= 3)
					m_IsMenaEnabled = (bool)info.GetValue("IsMenaEnabled", typeof(bool));

				if (iMajor >= 2 && iMinor >= 4)
					m_IsHeavyDutyEnabled = (bool)info.GetValue("IsHeavyDutyEnabled", typeof(bool));
			}
			else
				++DrawingDocument._sBiggerMajorNumber;
		}
		//=============================================================================
		public virtual void OnDeserialization(object sender) { }

		#endregion
	}

	public class Rack_State : GeometryState
	{
		public Rack_State(Rack rack)
			: base(rack)
		{
			//
			this.IsFirstInRowColumn = rack.IsFirstInRowColumn;
			this.SizeIndex = rack.SizeIndex;
			this.CanCreateRow = rack.CanCreateRow;
			//
			this.IsUnderpassAvailable = rack.IsUnderpassAvailable;
			this.Underpass = rack.Underpass;
			//
			this.IsMaterialOnGround = rack.IsMaterialOnGround;
			//
			this.ShowPallet = rack.ShowPallet;
			//
			this.AreLevelsTheSame = rack.AreLevelsTheSame;
			this.Levels = new ObservableCollection<RackLevel>();
			if (rack.Levels != null)
			{
				foreach (RackLevel level in rack.Levels)
				{
					if (level == null)
						continue;

					RackLevel levelClone = level.Clone() as RackLevel;
					if (levelClone == null)
						continue;

					this.Levels.Add(levelClone);
				}
			}
			//
			this.SplitColumn = rack.SplitColumn;
			this.Column_FirstPartLength = rack.Column_FirstPartLength;
			this.Column_SecondPartLength = rack.Column_SecondPartLength;
			//
			this.DisableChangeSizeGripPoints = rack.DisableChangeSizeGripPoints;
			//
			this.MinColumnGUID = rack.MinColumnGUID;
			this.ColumnGUID = rack.ColumnGUID;

			//
			this.Bracing = rack.Bracing;
			this.XBracingHeight = rack.X_Bracing_Height;
			this.StiffenerHeight = rack.StiffenersHeight;

			this.TieBeamFrame = rack.TieBeamFrame;
			this.RequiredTieBeamFrames = rack.RequiredTieBeamFrames;
			this.RackHeightWithTieBeam_IsMoreThan_MaxHeight = rack.RackHeightWithTieBeam_IsMoreThan_MaxHeight;

			this.IsColumnSetManually = rack.IsColumnSetManually;

			this.ConectedAisleSpaceDirections = rack.ConectedAisleSpaceDirections;
		}
		public Rack_State(Rack_State state)
			: base(state)
		{
			//
			this.IsFirstInRowColumn = state.IsFirstInRowColumn;
			this.SizeIndex = state.SizeIndex;
			this.CanCreateRow = state.CanCreateRow;
			//
			this.IsUnderpassAvailable = state.IsUnderpassAvailable;
			this.Underpass = state.Underpass;
			//
			this.IsMaterialOnGround = state.IsMaterialOnGround;
			//
			this.ShowPallet = state.ShowPallet;
			//
			this.AreLevelsTheSame = state.AreLevelsTheSame;
			this.Levels = new ObservableCollection<RackLevel>();
			if (state.Levels != null)
			{
				foreach (RackLevel level in state.Levels)
				{
					if (level == null)
						continue;

					RackLevel levelClone = level.Clone() as RackLevel;
					if (levelClone == null)
						continue;

					this.Levels.Add(levelClone);
				}
			}
			//
			this.SplitColumn = state.SplitColumn;
			this.Column_FirstPartLength = state.Column_FirstPartLength;
			this.Column_SecondPartLength = state.Column_SecondPartLength;
			//
			this.DisableChangeSizeGripPoints = state.DisableChangeSizeGripPoints;
			//
			this.MinColumnGUID = state.MinColumnGUID;
			this.ColumnGUID = state.ColumnGUID;

			//
			this.Bracing = state.Bracing;
			this.XBracingHeight = state.XBracingHeight;
			this.StiffenerHeight = state.StiffenerHeight;

			this.TieBeamFrame = state.TieBeamFrame;
			this.RequiredTieBeamFrames = state.RequiredTieBeamFrames;
			this.RackHeightWithTieBeam_IsMoreThan_MaxHeight = state.RackHeightWithTieBeam_IsMoreThan_MaxHeight;

			this.IsColumnSetManually = state.IsColumnSetManually;

			this.ConectedAisleSpaceDirections = state.ConectedAisleSpaceDirections;
		}

		#region Properties

		//
		public bool IsFirstInRowColumn { get; private set; }
		public bool CanCreateRow { get; private set; }
		//
		public int SizeIndex { get; private set; }
		//
		public UInt32 ClearAvailableHeight { get; private set; }
		//
		public bool IsUnderpassAvailable { get; private set; }
		public UInt32 Underpass { get; private set; }
		//
		public bool IsMaterialOnGround { get; private set; }
		//
		public bool ShowPallet { get; private set; }
		//
		public bool AreLevelsTheSame { get; private set; }
		public ObservableCollection<RackLevel> Levels { get; private set; }
		//
		public Guid MinColumnGUID { get; private set; }
		public Guid ColumnGUID { get; private set; }
		public bool IsColumnSetManually { get; private set; }
		//
		public bool SplitColumn { get; private set; }
		public UInt32 Column_FirstPartLength { get; private set; }
		public UInt32 Column_SecondPartLength { get; private set; }
		//
		public bool DisableChangeSizeGripPoints { get; private set; }
		//
		public eColumnBracingType Bracing { get; private set; }
		public double XBracingHeight { get; private set; }
		public double StiffenerHeight { get; private set; }
		//
		public eTieBeamFrame TieBeamFrame { get; private set; }
		public eTieBeamFrame RequiredTieBeamFrames { get; private set; }
		public bool RackHeightWithTieBeam_IsMoreThan_MaxHeight { get; private set; }

		//
		public ConectedAisleSpaceDirection ConectedAisleSpaceDirections { get; private set; }

		#endregion

		//=============================================================================
		protected override GeometryState MakeDeepCopy()
		{
			return new Rack_State(this);
		}
	}

	[Serializable]
	public class Rack : BaseRectangleGeometry, ISerializable
	{
		// values in global coordinates
		public static double sHorizontalRow_GlobalGap = 0;
		public static double sVerticalColumn_GlobalGap = 0;

		// The distance in global coordinates between the same rotated racks.
		// For exmple, if racks R1 and R2 are horizontal(vertical) then it is the minimum
		// vertical(horizontal) distance between them.
		//public static double sSameRotaionRakcs_MinimumGlobalDistance = 200;

		/// <summary>
		///	Height of back to back racks connector
		/// </summary>
		public const int BackToBackRackConnectorHeight = 30;

		public static UInt32 sDefaultDistanceBetweenPallet = 100;

		public static UInt32 sDistanceBetweenPalletAndLevel = 100;

		// If Underpass and MaterialOnGround are not checked then the distance from the ground to top of the first level beam
		// should be 250.
		public static UInt32 sFirstLevelOffset = 250;

		/// <summary>
		/// First horizontal bracing line offset from the ground.
		/// </summary>
		public static UInt32 sBracingLinesBottomOffset = 162;
		/// <summary>
		/// Vertical offset of X bracings from the horizontal bracing.
		/// </summary>
		public static UInt32 sXBracingVerticalOffset = 50;
		/// <summary>
		/// If distance from topmost bracing point to the top of column is greater or equal sTopHorizontalBracingMinDistance,
		/// then need to add one additional horizontal bracing.
		/// Additional horizontal bracing should be placed with sTopHorizontalBracingOffset from the column top.
		/// </summary>
		public static UInt32 sTopHorizontalBracingMinDistance = 338;
		/// <summary>
		/// The minimum horizontal bracing offset from the column top.
		/// Also it is used in diagonal members count calculation - it is offset from the top of column.
		/// </summary>
		public static UInt32 sTopHorizontalBracingOffset = 138;
		// Vertical step for normal and X bracings.
		// Look at the advanced properties picture.
		public static int sBracingVerticalStep = 600;

		/// <summary>
		/// If this rack requires tie beam frame but doesnt have it, then red rectangle is displayed over it.
		/// This is rectangle's offset(in global coordinates, not pixels) from the borders of rack.
		/// </summary>
		private static int sFrameErrorRectOffset = 100;

		/// <summary>
		/// Max rack height without pallets. Tie beam additional height should be included in this value.
		/// </summary>
		private static int sMaxRackHeight = 12000;

		/// <summary>
		/// The minimum number of levels except the ground level.
		/// </summary>
		private static UInt32 sMinNumberOfLevels = 2;

		//
		public static string PALLET_OVERHANG = "Overhang";
		public static string PALLET_FLUSH = "Flush";
		public static string BRACING_TYPE_GI = "GI";
		public static string BRACING_TYPE_POWDER_COATED = "Powder coated";
		public static string DECK_PLATE_TYPE_ALONG_DEPTH_UDL = "6 BP (Shelving application)";
		public static string DECK_PLATE_TYPE_ALONG_DEPTH_PALLET_SUPPORT = "6 BP (Pallet application)";
		public static string DECK_PLATE_TYPE_ALONG_LENGTH = "4 BP";
		//
		public static string PROP_DISABLE_CHANGE_SIZE_GRIPPOINTS = "DisableChangeSizeGripPoints";
		//
		public static string PROP_CLEAR_AVAILABLE_HEIGHT = "ClearAvailableHeight";
		public static string PROP_IS_UNDERPASS_AVAILABLE = "IsUnderpassAvailable";
		public static string PROP_UNDERPASS = "Underpass";
		public static string PROP_IS_MATERIAL_ON_GROUND = "IsMaterialOnGround";
		//
		public static string PROP_NUMBER_OF_LEVELS_EXCEPT_GROUNDLEVEL = "NumberOfLevelsExceptGroundLevel";
		public static string PROP_ARE_LEVELS_THE_SAME = "AreLevelsTheSame";
		public static string PROP_SHOW_PALLET = "ShowPallet";
		//
		public static string PROP_WEIGHT = "Weight";
		public static string PROP_PALLETS_ARE_EQUAL = "PalletsAreEqual";
		public static string PROP_NUMBER_OF_PALLETS = "NumberOfPallets";
		public static string PROP_PALLET_LENGTH = "PalletLength";
		public static string PROP_PALLET_DEPTH = "PalletDepth";
		public static string PROP_PALLET_HEIGHT = "PalletHeight";
		public static string PROP_PALLET_LOAD = "PalletLoad";
		public static string PROP_PALLET_CONFIGURATION = "PalletConfiguration";
		public static string PROP_LEVEL_HEIGHT = "LevelHeight";
		public static string PROP_LEVEL_LOAD = "LevelLoad";
		public static string PROP_LEVEL_ACCESSORIES = "LevelAccessories";
		//
		public static string PROP_COLUMN = "Column";
		public static string PROP_MINIMUM_COLUMN = "MinColumn";
		public static string PROP_SPLIT_COLUMN = "SplitColumn";
		public static string PROP_COLUMN_FIRST_PART_LENGTH = "ColumnFirstPartLength";
		public static string PROP_COLUMN_SECOND_PART_LENGTH = "ColumnSecondPartLength";
		//
		//public static string PROP_BRACING = "Bracing";
		public static string PROP_BRACING_TYPE = "BracingType";
		//
		public static string PROP_ACCESSORIES = "Accessories";
		//
		public static string PROP_RACK_LEVEL = "RackLevel";
		public static char PROP_RACK_LEVEL_DELIMITER = '_';

		//
		private Brush m_CircleBorderBrush = Brushes.Black;

		public Rack(DrawingSheet ds, bool bSetInit)
			: base(ds)
		{
			m_bDontChangeOrder = true;

			Name = "Rack";
			CanCreateRow = true;
			IsFirstInRowColumn = true;
			IsHorizontal = true;

			//
			StepLength_X = 25;
			StepLength_Y = 25;
			StepLength_Z = 25;

			//
			MinLength_X = 1440;
			MaxLength_X = 4040;

			//
			MinLength_Y = 600;
			MaxLength_Y = 1500;

			//
			MinLength_Z = 1000;
			MaxLength_Z = 12000;

			//
			m_Length_X = 3500;
			m_Length_Y = 1000;
			m_Length_Z = 5000;

			//
			this.IsInit = bSetInit;

			// init
			m_Length_Z = 5170;
			m_Length_X = 2860;
			m_Length_Y = 1000;
			//
			m_IsUnderpassAvailable = false;
			// init underpass value
			if (m_Sheet != null && m_Sheet.Document != null && Utils.FGT(m_Sheet.Document.OverallHeightLowered, 0.0))
				m_Underpass = (UInt32)Utils.GetWholeNumber(m_Sheet.Document.OverallHeightLowered);
			m_bIsMaterialOnGround = true;
			//
			m_bShowPallet = true;
			m_bAreLevelsTheSame = true;
			// add levels
			// 0 - ground level
			RackLevel rackLevel = new RackLevel(this);
			rackLevel.Index = 0;
			rackLevel.Set_PalletsAreEqual(true);
			if (rackLevel.Pallets != null && rackLevel.Pallets.Count == 2)
			{
				foreach (Pallet pallet in rackLevel.Pallets)
				{
					pallet.Set_Length(1200);
					pallet.Set_Width(1200);
					pallet.Set_Height(1000);
					pallet.Set_Load(1000);
				}
			}
			m_Levels.Add(rackLevel);
			// 1 - 4
			for (UInt32 i = 1; i <= 4; ++i)
			{
				RackLevel levelClone = rackLevel.Clone() as RackLevel;
				levelClone.Owner = this;
				levelClone.Index = i;
				m_Levels.Add(levelClone);
			}
			// 5
			RackLevel lastLevel = rackLevel.Clone() as RackLevel;
			lastLevel.Owner = this;
			lastLevel.Index = 5;
			lastLevel.Set_LevelHeight(300);
			m_Levels.Add(lastLevel);
			// update levels height from pallets
			foreach(RackLevel level in m_Levels)
			{
				if (level == null)
					continue;

				string strError;
				level.RecalcLevelHeight(out strError);
			}
			//
			if (this.SelectedLevel == null && m_Levels.Count > 0)
				this.SelectedLevel = m_Levels[0];

			// Calculate rack length based on pallets length
			string strLengthError;
			_RecalcRackLength(false, false, out strLengthError);

			// recalculate column befor calculate length
			string strColumnCalcError;
			this.RecalculateColumn(false, out strColumnCalcError);

			string strRackLengthError;
			_RecalcRackLength(true, false, out strRackLengthError);
			//
			string strRackWidthError;
			_RecalcRackWidth(out strRackWidthError);
			// Need to check rack height and fix it.
			// _RecalcRackHeight is called inside _RecalcRackLength, but it doesnt fix height if there is error.
			//
			// Dont check rack height here, because rack is not initialized yet - it has incorrect position(0, 100) and
			// his height depends on the roof type. Do it after geometry is created.
			// CheckRackHeight();

			// Need to recalc rack height from the levels and then make sure that levels satisfy rules in _RecalcLevelHeight().
			Length_Z = (int)_CalcRackHeight(false);
			// recalc level height from the rack height
			string strHeightError;
			_On_Height_Changed(true, out strHeightError);

			m_bDontChangeOrder = false;
			// Clear change order
			m_ChangeOrder = -1;
		}

		#region Fields

		// true - rack's length and width are drived by level and pallets sizes
		// false - user can change rack's length and width in simple properties
		private bool m_bDisableChangeSizeGripPoints = false;
		//
		private bool m_IsUnderpassAvailable = false;
		private UInt32 m_Underpass = 2500;
		// when user checks and unchecks m_IsUnderpassAvailable need to restore old levels
		private List<RackLevel> m_LevelsBeforeUnderpassChecked = null;
		//
		private bool m_bIsMaterialOnGround = false;
		//
		private bool m_bShowPallet = false;
		//
		private bool m_bAreLevelsTheSame = true;
		private ObservableCollection<RackLevel> m_Levels = new ObservableCollection<RackLevel>();
		//
		private bool m_bSplitColumn = false;
		private UInt32 m_Column_FirstPartLength = 5000;
		private UInt32 m_Column_SecondPartLength = 500;


        #endregion

        #region Properties

        //=============================================================================
        public ConectedAisleSpaceDirection ConectedAisleSpaceDirections { get; set; }
        //=============================================================================
        public override double MinLength_X
		{
			get
			{
				if(this.IsHorizontal)
					return RackUtils.GetMinLengthDependsOnBeams(this);

				return base.MinLength_X;
			}
			set
			{
				base.MinLength_X = value;
			}
		}
		/// <summary>
		/// Min length X value for the first rack in the group - M rack.
		/// </summary>
		public double FirstRack_MinLength_X
		{
			get
			{
				if (this.IsHorizontal)
					return RackUtils.GetMinLengthDependsOnBeams(true, this.DiffBetween_M_and_A);

				return base.MinLength_X;
			}
		}
		//=============================================================================
		public override double MaxLength_X
		{
			get
			{
				if (this.IsHorizontal)
					return RackUtils.GetMaxLengthDependsOnBeams(this);

				return base.MaxLength_X;
			}
			set
			{
				base.MaxLength_X = value;
			}
		}
		/// <summary>
		/// Max length X value for the first rack in the group - M rack.
		/// </summary>
		public double FirstRack_MaxLength_X
		{
			get
			{
				if (this.IsHorizontal)
					return RackUtils.GetMaxLengthDependsOnBeams(true, this.DiffBetween_M_and_A);

				return base.MaxLength_X;
			}
		}
		//=============================================================================
		public override double MinLength_Y
		{
			get
			{
				if (!this.IsHorizontal)
					return RackUtils.GetMinLengthDependsOnBeams(this);

				return base.MinLength_Y;
			}
			set
			{
				base.MinLength_Y = value;
			}
		}
		/// <summary>
		/// Min length Y value for the first rack in the group - M rack.
		/// </summary>
		public double FirstRack_MinLength_Y
		{
			get
			{
				if (!this.IsHorizontal)
					return RackUtils.GetMinLengthDependsOnBeams(true, this.DiffBetween_M_and_A);

				return base.MinLength_Y;
			}
		}
		//=============================================================================
		public override double MaxLength_Y
		{
			get
			{
				if (!this.IsHorizontal)
					return RackUtils.GetMaxLengthDependsOnBeams(this);

				return base.MaxLength_Y;
			}
			set
			{
				base.MaxLength_Y = value;
			}
		}
		/// <summary>
		/// Max length Y value for the first rack in the group - M rack.
		/// </summary>
		public double FirstRack_MaxLength_Y
		{
			get
			{
				if (!this.IsHorizontal)
					return RackUtils.GetMaxLengthDependsOnBeams(true, this.DiffBetween_M_and_A);

				return base.MaxLength_Y;
			}
		}
		//=============================================================================
		/// <summary>
		/// The maximum value for the height of the rack without pallets.
		/// </summary>
		public override int MaxLength_Z
		{
			get
			{
				int iResult = -1;

				// Max rack height depends on the document MaxLoadingHeight property.
				// Compare base value with MaxLoadingHeight and return the smallest one.
				if(this.Sheet != null && this.Sheet.Document != null
					&& this.Levels != null)
				{
					// rack max length = MaxLoadingHeight + the last level height
					RackLevel lastLevel = this.Levels.LastOrDefault();
					if(lastLevel != null)
					{
						double rackMaxLoadingHeight = this.Sheet.Document.MaxLoadingHeight;
						if (!double.IsNaN(rackMaxLoadingHeight) && !double.IsInfinity(rackMaxLoadingHeight))
						{
							// Dont add last level height, add MaxLevelHeight value instead.
							rackMaxLoadingHeight += lastLevel.MaxLevelHeight;//lastLevel.LevelHeight;
							if (iResult == -1 || Utils.FLT(rackMaxLoadingHeight, iResult))
								iResult = Utils.GetWholeNumber(rackMaxLoadingHeight);
						}
					}
				}

				// compare with ClearAvailableHeight, it is calculated based on the roof type and position of this rack
				double clearAvailableHeight = this.ClearAvailableHeight;
				if (iResult == -1 || Utils.FLT(clearAvailableHeight, iResult))
					iResult = Utils.GetWholeNumber(clearAvailableHeight);

				if (iResult == -1)
					iResult = base.MaxLength_Z;

				if (iResult > Rack.sMaxRackHeight)
					iResult = Rack.sMaxRackHeight;

				return iResult;
			}
			set
			{
				base.MaxLength_Z = value;
			}
		}

		//=============================================================================
		// Pallets overhang value, it drives margin in the depth direction.
		// Pallet depth = rack depth + 2 * overhang value.
		public double PalletOverhangValue
		{
			get
			{
				// if pallet type is overhang then get overhang value from the document
				if(this.Sheet != null
					&& this.Sheet.Document != null
					&& this.Sheet.Document.RacksPalletType == ePalletType.eOverhang)
				{
					return this.Sheet.Document.RacksPalletsOverhangValue;
				}

				return 0.0;
			}
		}

		//=============================================================================
		public override double MarginX
		{
			get
			{
				// If pallets are visible and overhang they goes out rack's depth.
				// So its need a margin for pallets.
				if (this.ShowPallet && !this.IsHorizontal)
					return PalletOverhangValue;

				return 0.0;
			}
			set
			{
				base.MarginX = value;
			}
		}
		public override double MarginY
		{
			get
			{
				// If pallets are visible and overhang they goes out rack's depth.
				// So its need a margin for pallets.
				if (this.ShowPallet && this.IsHorizontal)
					return PalletOverhangValue;

				return 0.0;
			}
			set
			{
				base.MarginY = value;
			}
		}

		//=============================================================================
		private bool m_bIsFirstInRowColumn = true;
		public bool IsFirstInRowColumn
		{
			get { return m_bIsFirstInRowColumn; }
			set
			{
				if (m_bIsFirstInRowColumn != value)
				{
					m_bIsFirstInRowColumn = value;
					_MarkStateChanged();
					_RecalcText();
				}
			}
		}

		//=============================================================================
		// zero-based
		private int m_iSizeIndex = -1;
		public int SizeIndex
		{
			get { return m_iSizeIndex; }
			set
			{
				if (m_iSizeIndex != value)
				{
					m_iSizeIndex = value;
					// For compatibbility with old versions.
					// 
					m_FillColor = RackColors.GetColor(m_iSizeIndex);
					_RecalcText();
				}
			}
		}

		/// <summary>
		/// Rack rectangle fill color
		/// </summary>
		public override Color FillColor
		{
			get
			{
				if (CurrentGeometryColorsTheme.CurrentTheme != null)
				{
					eColorType colorType = CurrentGeometryColorsTheme.RackToFillColorType(this.SizeIndex);
					Color colorValue;
					if (eColorType.eUndefined != colorType && CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(colorType, out colorValue))
						return colorValue;
				}

				return base.FillColor;
			}
		}

		//=============================================================================
		public bool CanCreateRow { get; set; }

		//=============================================================================
		protected override bool _Is_HeightProperty_ReadOnly { get { return false; } }

		//=============================================================================
		public bool DisableChangeSizeGripPoints
		{
			get { return m_bDisableChangeSizeGripPoints; }
			set
			{
				string strError;
				SetPropertyValue(PROP_DISABLE_CHANGE_SIZE_GRIPPOINTS, value, true, true, true, out strError);
			}
		}

		//=============================================================================
		public UInt32 RackLoad
		{
			get
			{
				UInt32 result = 0;

				if (m_Levels != null)
				{
					foreach (RackLevel _level in m_Levels)
					{
						if (_level == null)
							continue;

						result += _level.LevelLoad;
					}
				}

				return result;
			}
		}
		//=============================================================================
		public double RackWeight
		{
			get
			{
				return _RecalcRackWeight();
			}
		}
		//=============================================================================
		// Height available for this rack. Depends on the roof type, roof height and position of this rack
		// Drives max height for this rack.
		public double ClearAvailableHeight
		{
			get
			{
				// Create sheet with gable roof and 4000 as min height, 15000 as max height.
				// Now create rack - rack height is changed, because rack is not init and placed at (0, 0) point.
				// ClearAvailableHeight for (0, 0) is 3800, so rack height is recalculated.
				// It is an error, because rack was not placed.
				// So, if rack is not init dont limit clear available height.
				if (!this.IsInit)
					return 100000;

				double result = Rack.sMaxRackHeight;
				// calculate Clear Available Height
				if(this.Sheet != null && this.Sheet.SelectedRoof != null)
					result = this.Sheet.SelectedRoof.CalculateMaxHeightForGeometry(this) - Rack.ROOF_HEIGHT_GAP;

				if (Utils.FGT(result, Rack.sMaxRackHeight))
					result = Rack.sMaxRackHeight;

				return result;
			}
		}
		//=============================================================================
		/// <summary>
		/// Roof height for this rack.
		/// BUT you need to consider Rack.ROOF_HEIGHT_GAP when calculate max availble height. Use ClearAvailableHeight for max available height.
		/// </summary>
		public double RoofHeight
		{
			get
			{
				if (this.Sheet != null && this.Sheet.SelectedRoof != null)
					return this.Sheet.SelectedRoof.CalculateMaxHeightForGeometry(this);

				return 12000.0;
			}
		}
		//=============================================================================
		public bool IsUnderpassAvailable
		{
			get { return m_IsUnderpassAvailable; }
			set
			{
				string strError;
				SetPropertyValue(PROP_IS_UNDERPASS_AVAILABLE, value, true, true, true, out strError);
			}
		}
		//=============================================================================
		public UInt32 Underpass
		{
			get { return m_Underpass; }
			set
			{
				string strError;
				SetPropertyValue(PROP_UNDERPASS, value, true, true, true, out strError);
			}
		}
		//=============================================================================
		public bool IsMaterialOnGround
		{
			get { return m_bIsMaterialOnGround; }
			set
			{
				string strError;
				SetPropertyValue(PROP_IS_MATERIAL_ON_GROUND, value, true, true, true, out strError);
			}
		}
		//=============================================================================
		public bool ShowPallet
		{
			get { return m_bShowPallet; }
			set
			{
				string strError;
				SetPropertyValue(PROP_SHOW_PALLET, value, true, true, true, out strError);
			}
		}
		//=============================================================================
		public bool AreLevelsTheSame
		{
			get { return m_bAreLevelsTheSame; }
			set
			{
				string strError;
				SetPropertyValue(PROP_ARE_LEVELS_THE_SAME, value, true, true, true, out strError);
			}
		}
		//=============================================================================
		// Number of levels in m_Levels list without ground level(it has Index property = 0).
		public UInt32 NumberOfLevels_WithoutGround
		{
			get
			{
				int iCount = m_Levels.Count;

				if (m_Levels != null)
				{
					RackLevel _grLevel = m_Levels.FirstOrDefault(l => l != null && l.Index == 0);
					if (_grLevel != null)
						iCount -= 1;
				}

				return (UInt32)iCount;
			}
			set
			{
				string strError;
				SetPropertyValue(PROP_NUMBER_OF_LEVELS_EXCEPT_GROUNDLEVEL, value, true, true, true, out strError);
			}
		}
		//=============================================================================
		private List<UInt32> m_NumberOfLevels_Values = new List<UInt32> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
		public List<UInt32> NumberOfLevels_Values
		{
			get { return m_NumberOfLevels_Values; }
		}
		//=============================================================================
		private RackLevel m_SelectedLevel = null;
		public RackLevel SelectedLevel
		{
			get { return m_SelectedLevel; }
			set
			{
				if (m_SelectedLevel != value)
					m_SelectedLevel = value;
			}
		}
		//=============================================================================
		public ObservableCollection<RackLevel> Levels { get { return m_Levels; } }
		//=============================================================================
		// RackColumn giud from the DrawingDocument's RacksColumnsList.
		// It drives column length, depth and thickness.
		// m_ColumnGUID is selected column.
		// All columns in the group should have the same column, min available column and bracing.
		private Guid m_ColumnGUID = Guid.Empty;
		public Guid ColumnGUID { get { return m_ColumnGUID; } }
		public RackColumn Column
		{
			get { return _GetColumnByGUID(m_ColumnGUID); }
			set
			{
				if (value != null && value.GUID != m_ColumnGUID)
				{
					string strError;
					SetPropertyValue(PROP_COLUMN, value, true, true, true, out strError);
				}
			}
		}
		// It is minimum available column for this rack. It depends on the beam length,
		// this and neighbor racks load. Selected column(m_ColumnGUID) size should be greater or equal than m_MinColumnGUID column.
		private Guid m_MinColumnGUID = Guid.Empty;
		public Guid MinColumnGUID { get { return m_MinColumnGUID; } }
		public RackColumn MinimumColumn
		{
			get { return _GetColumnByGUID(m_MinColumnGUID); }
			set
			{
				if (value != null)
				{
					string strError;
					SetPropertyValue(PROP_MINIMUM_COLUMN, value, true, true, true, out strError);
				}
			}
		}
		//=============================================================================
		/// <summary>
		/// Collection with RackColumns which is available for this rack.
		/// </summary>
		private ObservableCollection<RackColumn> m_ColumnsCollection = new ObservableCollection<RackColumn>();
		public ObservableCollection<RackColumn> ColumnsCollection
		{
			get
			{
				m_ColumnsCollection.Clear();

				RackColumn minColumn = MinimumColumn;
				if (minColumn != null && this.Sheet != null && this.Sheet.Document != null && this.Sheet.Document.RacksColumnsList != null)
				{
					foreach (RackColumn column in this.Sheet.Document.RacksColumnsList)
					{
						if (column == null)
							continue;

						// Add column if it is greater or equal than minimum column.
						// Compare by the length and thickness, look at the LoadChart.xlsx file.
						if (Utils.FGT(column.Length, minColumn.Length) || (Utils.FGE(column.Length, minColumn.Length) && Utils.FGE(column.Thickness, minColumn.Thickness)))
						{
							m_ColumnsCollection.Add(column);
						}
					}
				}

				return m_ColumnsCollection;
			}
		}
		//=============================================================================
		/// <summary>
		/// If TRUE then m_ColumnGUID is set manually by user from the properties tab.
		/// It means that need to keep selected column if it is possible(it is greater or equal to m_MinColumnGUID).
		/// If it is FALSE then need to use minimum available column.
		/// </summary>
		private bool m_IsColumnSetManually = false;
		public bool IsColumnSetManually { get { return m_IsColumnSetManually; } }
		//=============================================================================
		public bool SplitColumn
		{
			get { return m_bSplitColumn; }
			set
			{
				string strError;
				SetPropertyValue(PROP_SPLIT_COLUMN, value, true, true, true, out strError);
			}
		}
		//=============================================================================
		public UInt32 Column_FirstPartLength
		{
			get { return m_Column_FirstPartLength; }
			set
			{
				string strError;
				SetPropertyValue(PROP_COLUMN_FIRST_PART_LENGTH, value, true, true, true, out strError);
			}
		}
		//=============================================================================
		public UInt32 Column_SecondPartLength
		{
			get { return m_Column_SecondPartLength; }
			set
			{
				string strError;
				SetPropertyValue(PROP_COLUMN_SECOND_PART_LENGTH, value, true, true, true, out strError);
			}
		}
		//=============================================================================
		/// <summary>
		/// Name of the bracing for display it in UI.
		/// </summary>
		public string BracingDisplayName
		{
			get
			{
				if (eColumnBracingType.eNormalBracing == m_Bracing)
					return RackLoadUtils.NORMAL_BRACING;
				else if (eColumnBracingType.eXBracing == m_Bracing)
					return RackLoadUtils.X_BRACING;
				else if (eColumnBracingType.eNormalBracingWithStiffener == m_Bracing)
					return RackLoadUtils.NORMAL_BRACING_WITH_STIFFENER;
				else if (eColumnBracingType.eXBracingWithStiffener == m_Bracing)
					return RackLoadUtils.X_BRACING_WITH_STIFFENER;

				return "Undefined";
			}
		}
		//=============================================================================
		/// <summary>
		/// Column bracing. It is displayed at the SIDE VIEW at the advanced properties picture.
		/// It depends on the rack load.
		/// All columns in the group should have the same column, min available column and bracing.
		/// </summary>
		private eColumnBracingType m_Bracing = eColumnBracingType.eNormalBracing;
		public eColumnBracingType Bracing
		{
			get { return m_Bracing; }
			set
			{
				if (value != m_Bracing)
				{
					m_Bracing = value;
					_MarkStateChanged();
				}
			}
		}
		//=============================================================================
		/// <summary>
		/// Height of the level(with pallets) of maximum level which requires stiffener.
		/// </summary>
		private double m_StiffenersHeight = 0.0;
		public double StiffenersHeight
		{
			get { return m_StiffenersHeight; }
			set
			{
				if (Utils.FNE(value, m_StiffenersHeight))
				{
					if (Utils.FGT(value, 0.0))
						m_StiffenersHeight = value;
					else
						m_StiffenersHeight = 0.0;
					_MarkStateChanged();
				}
			}
		}
		/// <summary>
		/// Stiffeners count, it is actual only for "normal bracing with stiffener" and "X bracing with stiffener".
		/// </summary>
		public int StiffenersCount
		{
			get
			{
				// 997 is the height of for 1 stiffener & for every 600 aditional a stiffener will be added
				double rCount = (m_StiffenersHeight - 997) / 600;
				if (Utils.FLT(rCount, 0.0))
					rCount = 0.0;
				return Utils.GetWholeNumber(Math.Ceiling(rCount)) + 1;
			}
		}
		//=============================================================================
		// Height of X bracing. It is displayed at the advanced properties picture only when m_Bracing = eColumnBracingType.eXBracing.
		// It depends on the beam length and rack load.
		// All columns in the group should have the same column, min available column and bracing.
		private double m_X_BracingHeight = 0.0;
		public double X_Bracing_Height
		{
			get { return m_X_BracingHeight; }
			set
			{
				if (Utils.FGT(value, 0.0) && Utils.FNE(value, m_X_BracingHeight))
				{
					m_X_BracingHeight = value;
					_MarkStateChanged();
				}
			}
		}
		// Count of "X" bracings.
		public int X_Bracings_Count
		{
			get
			{
				if(this.Bracing == eColumnBracingType.eXBracing || this.Bracing == eColumnBracingType.eXBracingWithStiffener)
				{
					double count = (Utils.GetWholeNumber(this.X_Bracing_Height) - (int)Rack.sBracingLinesBottomOffset - sXBracingVerticalOffset) / Rack.sBracingVerticalStep;
					return (int)Math.Floor(count);
				}

				return 0;
			}
		}
		// If X bracing is choosen then it has the minimum height value. It is max of:
		// 1. 1400mm
		// 2. If material on ground or underpass is checked then up to the top of first level(not ground) beam.
		// 3. If material on ground is not checked then up to the top of second level beam.
		private static double X_BRACING_MIN_HEIGHT = 1400.0;
		public double MinXBracingHeight
		{
			get
			{
				double minHeight = X_BRACING_MIN_HEIGHT;

				// Consider neighbor racks connection points.
				if (m_Levels != null)
				{
					SortedDictionary<double, RackLevel> levelsHeightDictionary = new SortedDictionary<double, RackLevel>();
					RackUtils.BuildLevelsHeightDictionary(ref levelsHeightDictionary, m_Levels);

					Rack prevRack = null;
					Rack nextRack = null;
					if(this.Sheet != null)
					{
						List<Rack> racksGroup = this.Sheet.GetRackGroup(this);
						if (racksGroup != null)
						{
							int iThisIndex = racksGroup.IndexOf(this);
							if (iThisIndex > 0)
								prevRack = racksGroup[iThisIndex - 1];
							if (iThisIndex < racksGroup.Count - 1)
								nextRack = racksGroup[iThisIndex + 1];
						}
					}


					// if it is a single rack
					if (prevRack == null && nextRack == null)
					{
						double xBracingHeight;
						if (RackUtils.CalculateXBracingHeight(this, levelsHeightDictionary, out xBracingHeight))
						{
							if (Utils.FGT(xBracingHeight, minHeight))
								minHeight = xBracingHeight;
						}
					}
					else
					{
						// check column with prev rack
						if (prevRack != null)
						{
							RackUtils.BuildLevelsHeightDictionary(ref levelsHeightDictionary, prevRack.Levels);
							double prevRackXBracingHeight;
							if (RackUtils.CalculateXBracingHeight(this, levelsHeightDictionary, out prevRackXBracingHeight))
							{
								if (Utils.FGT(prevRackXBracingHeight, minHeight))
									minHeight = prevRackXBracingHeight;
							}
						}

						// check column with next rack
						if (nextRack != null)
						{
							levelsHeightDictionary.Clear();
							RackUtils.BuildLevelsHeightDictionary(ref levelsHeightDictionary, m_Levels);
							RackUtils.BuildLevelsHeightDictionary(ref levelsHeightDictionary, nextRack.Levels);
							double nextRackXBracingHeight;
							if (RackUtils.CalculateXBracingHeight(this, levelsHeightDictionary, out nextRackXBracingHeight))
							{
								if (Utils.FGT(nextRackXBracingHeight, minHeight))
									minHeight = nextRackXBracingHeight;
							}
						}
					}
				}

				// Fix min x bracing height value using vertical height of each "X" bracing - Rack.sBracingVerticalStep.
				// "X" bracings count should be whole number - 1, 2, 3 etc.
				int count = Utils.GetWholeNumber(Math.Ceiling((minHeight - Rack.sBracingLinesBottomOffset - Rack.sXBracingVerticalOffset) / Rack.sBracingVerticalStep));
				minHeight = Rack.sBracingLinesBottomOffset + Rack.sXBracingVerticalOffset + count * Rack.sBracingVerticalStep;

				return minHeight;
			}
		}
		//=============================================================================
		public eBracingType BracingType
		{
			get
			{
				if (m_Sheet != null && m_Sheet.Document != null)
					return m_Sheet.Document.Rack_BracingType;

				return eBracingType.eGI;
			}
			set
			{
				string strError;
				SetPropertyValue(PROP_BRACING_TYPE, value, true, true, true, out strError);
			}
		}
		private List<string> m_BracingTypeList = new List<string>
		{
			BRACING_TYPE_GI,
			BRACING_TYPE_POWDER_COATED
		};
		public List<string> BracingTypeList { get { return m_BracingTypeList; } }

		//=============================================================================
		// A-rack should be smaller then M-rack(the first one in the column\row)
		public UInt32 DiffBetween_M_and_A
		{
			get
			{
				RackColumn selectedColumn = this.Column;
				if (selectedColumn != null)
					return (UInt32)Utils.GetWholeNumber(selectedColumn.Length);

				return 0;
			}
		}

		//=============================================================================
		public RackAccessories Accessories
		{
			get
			{
				if (m_Sheet != null && m_Sheet.Document != null)
					return m_Sheet.Document.Rack_Accessories;

				return null;
			}
			set
			{
				string strError;
				SetPropertyValue(PROP_ACCESSORIES, value, true, false, true, out strError);
			}
		}

		//=============================================================================
		// Length of the beam.
		// Beam load calculated based on this value.
		public double BeamLength
		{
			get
			{
				// Read rules inside _Calc_M_RackLength().
				// Beam length = rack length - 2 * column_width - INNER_LENGTH_ADDITIONAL_GAP
				double beamLength = this.Length_X;
				if (!this.IsHorizontal)
					beamLength = this.Length_Y;
				// remove column
				if (this.IsFirstInRowColumn)
					beamLength -= 2 * this.DiffBetween_M_and_A;
				else
					beamLength -= this.DiffBetween_M_and_A;
				beamLength -= INNER_LENGTH_ADDITIONAL_GAP;

				return beamLength;
			}
		}

		//=============================================================================
		// Inner rack length - from column to column.
		public double InnerLength
		{
			get
			{
				// Read rules inside _Calc_M_RackLength().
				// Inner length = rack length - 2 * column_width
				double innerLength = this.Length_X;
				if (!this.IsHorizontal)
					innerLength = this.Length_Y;
				// convert it to M-rack
				if (!this.IsFirstInRowColumn)
					innerLength += this.DiffBetween_M_and_A;
				//
				innerLength -= 2 * this.DiffBetween_M_and_A;

				return innerLength;
			}
		}

		//=============================================================================
		// Max height of the pallet - the highest pallet on the last level.
		public double MaterialHeight
		{
			get
			{
				return _CalcRackHeight(true);
			}
		}

		//=============================================================================
		// Top of the topmost beam.
		public double MaxLoadingHeight
		{
			get
			{
				double loadingHeight = this.Length_Z;
				RackLevel _lastLevel = m_Levels.LastOrDefault();
				if (_lastLevel != null)
					loadingHeight -= _lastLevel.LevelHeight;
				return loadingHeight;
			}
		}

		//=============================================================================
		// The maximum height of the rack - the biggest value between 
		// rack height and rack height with pallets.
		public static uint TIE_BEAM_ADDITIONAL_HEIGHT = 500;
		public UInt32 MaxHeight
		{
			get
			{
				UInt32 _height = _CalcRackHeight(false);
				UInt32 _heightWithPallets = _CalcRackHeight(true);
				if (m_TieBeamFrame != 0)
				{
					_heightWithPallets += TIE_BEAM_ADDITIONAL_HEIGHT;
					_heightWithPallets = (UInt32)RackUtils.RoundColumnHeight(_heightWithPallets);
				}

				return Math.Max(_height, _heightWithPallets);
			}
		}

		//=============================================================================
		/// <summary>
		/// Returns frame height, it depends on the tie beam.
		/// </summary>
		public double FrameHeight
		{
			get
			{
				if (m_TieBeamFrame != 0)
				{
					// FrameHeight = MaxMaterialHeight + TIE_BEAM_ADDITIONAL_HEIGHT.
					double frameHeight = MaterialHeight + TIE_BEAM_ADDITIONAL_HEIGHT;
					return RackUtils.RoundColumnHeight(frameHeight);
				}

				return this.Length_Z;
			}
		}

		/// <summary>
		/// Contains tie beam frames which are required for this rack.
		/// Why does rack need it? - probably this rack requires tie beams, but it doesnt have an opposite rack
		/// and tie beam is not applied.
		/// Or column height with tie beam is greater than ClearAvailableHeight.
		/// We need to highligh this king of racks - which requires but dont have tie beam.
		/// m_TieBeamFrame contains rack frames with tie beams.
		/// m_RequiredTieBeamFrames contains rack frames which requies tie beams.
		/// </summary>
		private eTieBeamFrame m_RequiredTieBeamFrames = eTieBeamFrame.eNone;
		public eTieBeamFrame RequiredTieBeamFrames
		{
			get { return m_RequiredTieBeamFrames; }
			set
			{
				if (value != m_RequiredTieBeamFrames)
				{
					m_RequiredTieBeamFrames = value;
					_MarkStateChanged();
				}
			}
		}
		/// <summary>
		/// Returns true if tie beam is required for start frame, but rack doesnt have it.
		/// </summary>
		public bool StartFrameTieBeamError { get { return this.RequiredTieBeamFrames.HasFlag(eTieBeamFrame.eStartFrame) && !this.TieBeamFrame.HasFlag(eTieBeamFrame.eStartFrame); } }
		/// <summary>
		/// Returns true if tie beam is required for end frame, but rack doesnt have it.
		/// </summary>
		public bool EndFrameTieBeamError { get { return this.RequiredTieBeamFrames.HasFlag(eTieBeamFrame.eEndFrame) && !this.TieBeamFrame.HasFlag(eTieBeamFrame.eEndFrame); } }
		/// <summary>
		/// If it is true then tie beam can be added to this rack, but it is not
		/// added because rack height with tie beam is more than sMaxRackHeight.
		/// </summary>
		private bool m_RackHeightWithTieBeam_IsMoreThan_MaxHeight = false;
		public bool RackHeightWithTieBeam_IsMoreThan_MaxHeight
		{
			get { return m_RackHeightWithTieBeam_IsMoreThan_MaxHeight; }
			set
			{
				if(value != m_RackHeightWithTieBeam_IsMoreThan_MaxHeight)
				{
					m_RackHeightWithTieBeam_IsMoreThan_MaxHeight = value;
					_MarkStateChanged();
				}
			}
		}
		/// <summary>
		/// Determine on which frame place tie beam.
		/// If tie beam is placed on the frame then
		/// FrameHeight = MaxMaterialHeight + TIE_BEAM_ADDITIONAL_HEIGHT.
		/// </summary>
		private eTieBeamFrame m_TieBeamFrame = eTieBeamFrame.eNone;
		public eTieBeamFrame TieBeamFrame
		{
			get { return m_TieBeamFrame; }
			set
			{
				if (value != m_TieBeamFrame)
				{
					m_TieBeamFrame = value;
					_MarkStateChanged();
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// If true then dont change m_ChangeOrder.
		/// It is used inside rack constructor.
		/// </summary>
		public bool m_bDontChangeOrder = false;
		/// <summary>
		/// Order of this rack in all changed racks(during the command) list.
		/// It is used to correct calculate rack indexes in command end.
		/// </summary>
		private long m_ChangeOrder = -1;
		public long ChangeOrder { get { return m_ChangeOrder; } }

		//=============================================================================
		/// <summary>
		/// Returns warning message(for example, if rack requires tie beam but doesnt have it).
		/// </summary>
		public string WarningMessage
		{
			get
			{
				StringBuilder warningSB = new StringBuilder();
				if (this.StartFrameTieBeamError || this.EndFrameTieBeamError)
				{
					if (this.RackHeightWithTieBeam_IsMoreThan_MaxHeight)
						warningSB.Append("1.");
					warningSB.Append("This Rack needs Tie-Beam as per the stability guidelines.");
				}

				if (this.RackHeightWithTieBeam_IsMoreThan_MaxHeight)
				{
					if (warningSB.Length > 0)
						warningSB.Append(".\n\n2.");
					warningSB.Append("Maximum Material height +500 have to be equal or less than 12 M. Rack height need to be reduced appropriately.");
				}

				return warningSB.ToString();
			}
		}

		#endregion

		#region Overrides

		//=============================================================================
		protected override GeometryState _GetOriginalState()
		{
			return new Rack_State(this);
		}
		//=============================================================================
		protected override void _SetState(GeometryState state)
		{
			base._SetState(state);

			Rack_State rackState = state as Rack_State;
			if (rackState == null)
				return;

			//
			this.m_bIsFirstInRowColumn = rackState.IsFirstInRowColumn;
			this.m_iSizeIndex = rackState.SizeIndex;
			this.CanCreateRow = rackState.CanCreateRow;
			//
			this.m_IsUnderpassAvailable = rackState.IsUnderpassAvailable;
			this.m_Underpass = rackState.Underpass;
			//
			this.m_bIsMaterialOnGround = rackState.IsMaterialOnGround;
			//
			this.m_bShowPallet = rackState.ShowPallet;
			//
			this.m_bAreLevelsTheSame = rackState.AreLevelsTheSame;
			this.m_Levels = rackState.Levels;
			_SetLevelsOwner();
			//
			this.m_bSplitColumn = rackState.SplitColumn;
			this.m_Column_FirstPartLength = rackState.Column_FirstPartLength;
			this.m_Column_SecondPartLength = rackState.Column_SecondPartLength;
			//
			this.m_bDisableChangeSizeGripPoints = rackState.DisableChangeSizeGripPoints;
			//
			this.m_MinColumnGUID = rackState.MinColumnGUID;
			this.m_ColumnGUID = rackState.ColumnGUID;
			//
			this.m_Bracing = rackState.Bracing;
			this.m_X_BracingHeight = rackState.XBracingHeight;
			this.m_StiffenersHeight = rackState.StiffenerHeight;
			//
			this.m_TieBeamFrame = rackState.TieBeamFrame;
			this.m_RequiredTieBeamFrames = rackState.RequiredTieBeamFrames;
			this.m_IsColumnSetManually = rackState.IsColumnSetManually;
			this.m_RackHeightWithTieBeam_IsMoreThan_MaxHeight = rackState.RackHeightWithTieBeam_IsMoreThan_MaxHeight;
		}
		//=============================================================================
		protected override BaseRectangleGeometry CreateInstance() { return new Rack(null, true); }

		//=============================================================================
		protected override void _InitProperties()
		{
			base._InitProperties();

			if (m_Properties != null)
			{
				m_Properties.Add(new GeometryProperty(this, PROP_DISABLE_CHANGE_SIZE_GRIPPOINTS, "Advance properties drives rack size", false, false, "Size"));

				_UpdateProperties();
			}
		}

		//=============================================================================
		public override void Draw(DrawingContext dc, ICoordinateSystem cs, IGeomDisplaySettings geomDisplaySettings = null)
		{
			//
			if (dc == null)
				return;
			//
			if (cs == null)
				return;
			//
			if (m_Sheet == null)
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

			base.Draw(dc, cs, displaySettings);

			_DrawColumnSpaceOffset(dc, cs, displaySettings);

			// draw underpass symbol
			if (this.IsUnderpassAvailable)
			{
				int underpassSymbolOffset_LengthDirection = 15;
				int underpassSymbolOffset_DepthDirection = 5;

				Point underpassSymPnt01, underpassSymPnt02, underpassSymPnt01_01, underpassSymPnt02_01;
				if(m_bIsHorizontal)
				{
					//
					underpassSymPnt01 = GetLocalPoint(cs, this.TopRight_GlobalPoint);
					underpassSymPnt01.Y -= underpassSymbolOffset_DepthDirection;
					underpassSymPnt01.X -= underpassSymbolOffset_LengthDirection;
					//
					underpassSymPnt01_01 = GetLocalPoint(cs, this.TopRight_GlobalPoint);
					underpassSymPnt01_01.X -= underpassSymbolOffset_LengthDirection - underpassSymbolOffset_DepthDirection;
					//
					underpassSymPnt02 = GetLocalPoint(cs, this.BottomRight_GlobalPoint);
					underpassSymPnt02.Y += underpassSymbolOffset_DepthDirection;
					underpassSymPnt02.X -= underpassSymbolOffset_LengthDirection;
					//
					underpassSymPnt02_01 = GetLocalPoint(cs, this.BottomRight_GlobalPoint);
					underpassSymPnt02_01.X -= underpassSymbolOffset_LengthDirection + underpassSymbolOffset_DepthDirection;
				}
				else
				{
					//
					underpassSymPnt01 = GetLocalPoint(cs, this.TopLeft_GlobalPoint);
					underpassSymPnt01.X -= underpassSymbolOffset_DepthDirection;
					underpassSymPnt01.Y += underpassSymbolOffset_LengthDirection;
					//
					underpassSymPnt01_01 = GetLocalPoint(cs, this.TopLeft_GlobalPoint);
					underpassSymPnt01_01.Y += underpassSymbolOffset_LengthDirection - underpassSymbolOffset_DepthDirection;
					//
					underpassSymPnt02 = GetLocalPoint(cs, this.TopRight_GlobalPoint);
					underpassSymPnt02.X += underpassSymbolOffset_DepthDirection;
					underpassSymPnt02.Y += underpassSymbolOffset_LengthDirection;
					//
					underpassSymPnt02_01 = GetLocalPoint(cs, this.TopRight_GlobalPoint);
					underpassSymPnt02_01.Y += underpassSymbolOffset_LengthDirection + underpassSymbolOffset_DepthDirection;
				}

				Color rackUnderpassSymbolColor = Colors.Red;
				if (CurrentGeometryColorsTheme.CurrentTheme != null)
				{
					Color color;
					if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackUnderpassSymbolColor, out color))
						rackUnderpassSymbolColor = color;
				}
				SolidColorBrush rackUnderpassSymbolBrush = new SolidColorBrush(rackUnderpassSymbolColor);

				// draw underpass symbol
				Pen underpassSymbolPen = new Pen(rackUnderpassSymbolBrush, 2);
				dc.DrawLine(underpassSymbolPen, underpassSymPnt01_01, underpassSymPnt01);
				dc.DrawLine(underpassSymbolPen, underpassSymPnt01, underpassSymPnt02);
				dc.DrawLine(underpassSymbolPen, underpassSymPnt02, underpassSymPnt02_01);
			}

			// draw red circles for Rack only
			double LengthInPixels = GetWidthInPixels(cs, m_Length_X);
			double WidthInPixels = GetHeightInPixels(cs, m_Length_Y);
			//
			Point firstCirclePoint, secondCirclePoint;
			if (m_bIsHorizontal)
			{
				//
				firstCirclePoint = GetLocalPoint(cs, TopLeft_GlobalPoint);
				firstCirclePoint.X += LengthInPixels / 2;
				//
				secondCirclePoint = firstCirclePoint;
				secondCirclePoint.Y += WidthInPixels;
			}
			else
			{
				//
				firstCirclePoint = GetLocalPoint(cs, TopLeft_GlobalPoint);
				firstCirclePoint.Y += WidthInPixels / 2;
				//
				secondCirclePoint = firstCirclePoint;
				secondCirclePoint.X += LengthInPixels;
			}

			Color rackDotsColor = Colors.Red;
			if (CurrentGeometryColorsTheme.CurrentTheme != null)
			{
				Color color;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackDotsColor, out color))
					rackDotsColor = color;
			}
			SolidColorBrush rackDotsBrush = new SolidColorBrush(rackDotsColor);
			Pen circleBorderPen = new Pen(m_CircleBorderBrush, 1.0);
			// dots should have fixed size in pixels
			double rDotRadius = 0.2 * displaySettings.TextFontSize; //50 * (LengthInPixels / m_Length_X);
			dc.DrawEllipse(rackDotsBrush, circleBorderPen, firstCirclePoint, rDotRadius, rDotRadius);
			dc.DrawEllipse(rackDotsBrush, circleBorderPen, secondCirclePoint, rDotRadius, rDotRadius);

			bool startFrameError = this.StartFrameTieBeamError;
			bool endFrameError = this.EndFrameTieBeamError;
			if (startFrameError || endFrameError)
			{
				Color rackTieBeamErrorColor = Colors.Red;
				if (CurrentGeometryColorsTheme.CurrentTheme != null)
				{
					Color color;
					if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackTieBeamErrorRectangleColor, out color))
						rackTieBeamErrorColor = color;
				}
				SolidColorBrush rackTieBeamErrorBrush = new SolidColorBrush(rackTieBeamErrorColor);
				Pen frameErrorPen = new Pen(rackTieBeamErrorBrush, 2.0);

				Point frameError_TopLeftPnt = this.TopLeft_GlobalPoint;
				frameError_TopLeftPnt.X += Rack.sFrameErrorRectOffset;
				frameError_TopLeftPnt.Y += Rack.sFrameErrorRectOffset;

				Point frameError_BotRightPnt = this.BottomRight_GlobalPoint;
				frameError_BotRightPnt.X -= Rack.sFrameErrorRectOffset;
				frameError_BotRightPnt.Y -= Rack.sFrameErrorRectOffset;

				Rect frameErrorRect = new Rect(GetLocalPoint(cs, frameError_TopLeftPnt), GetLocalPoint(cs, frameError_BotRightPnt));
				dc.DrawRectangle(null, frameErrorPen, frameErrorRect);
			}

            // draw column guards if Rack has grip aisle space and upright guards enabled
            if (this.Accessories.UprightGuard)
            {
				_TryDrawColumnGuards(dc, cs, geomDisplaySettings);
			}

            // draw row guards if Rack has grip aisle space and row guards enabled
			if (this.Accessories.RowGuard)
            {
				_TryDrawRowGuards(dc, cs, geomDisplaySettings);
			}
		}

		

		//=============================================================================
		public override object GetPropertyValue(string strPropSysName)
		{
			if (PROP_CLEAR_AVAILABLE_HEIGHT == strPropSysName)
				return this.ClearAvailableHeight;
			//
			else if (PROP_IS_UNDERPASS_AVAILABLE == strPropSysName)
				return m_IsUnderpassAvailable;
			else if (PROP_UNDERPASS == strPropSysName)
				return m_Underpass;
			//
			else if (PROP_IS_MATERIAL_ON_GROUND == strPropSysName)
				return m_bIsMaterialOnGround;
			//
			else if (PROP_DISABLE_CHANGE_SIZE_GRIPPOINTS == strPropSysName)
				return m_bDisableChangeSizeGripPoints;
			else if (PROP_SHOW_PALLET == strPropSysName)
				return m_bShowPallet;
			//
			else if (PROP_NUMBER_OF_LEVELS_EXCEPT_GROUNDLEVEL == strPropSysName)
				return this.NumberOfLevels_WithoutGround;
			else if (PROP_ARE_LEVELS_THE_SAME == strPropSysName)
				return m_bAreLevelsTheSame;
			//
			else if (PROP_COLUMN == strPropSysName)
				return Column;
			else if (PROP_MINIMUM_COLUMN == strPropSysName)
				return MinimumColumn;
			else if (PROP_SPLIT_COLUMN == strPropSysName)
				return m_bSplitColumn;
			else if (PROP_COLUMN_FIRST_PART_LENGTH == strPropSysName)
				return m_Column_FirstPartLength;
			else if (PROP_COLUMN_SECOND_PART_LENGTH == strPropSysName)
				return m_Column_SecondPartLength;
			//
			//else if (PROP_BRACING == strPropSysName)
			//{
			//	if (m_Sheet != null && m_Sheet.Document != null)
			//		return m_Sheet.Document.Rack_Bracing;
			//
			//	return eBracing.eTypeA;
			//}
			else if (PROP_BRACING_TYPE == strPropSysName)
			{
				if (m_Sheet != null && m_Sheet.Document != null)
					return m_Sheet.Document.Rack_BracingType;

				return eBracingType.eGI;
			}
			else if (PROP_ACCESSORIES == strPropSysName)
			{
				if (m_Sheet != null && m_Sheet.Document != null)
					return m_Sheet.Document.Rack_Accessories;

				return null;
			}
			else if (!string.IsNullOrEmpty(strPropSysName) && strPropSysName.StartsWith(PROP_RACK_LEVEL))
			{
				UInt32 _levelIndex;
				string strLevelProp;
				if (RackLevel._ParseLevelProp_SystemName(strPropSysName, out _levelIndex, out strLevelProp) && !string.IsNullOrEmpty(strLevelProp))
				{
					if (_levelIndex > 0 && _levelIndex <= m_Levels.Count)
					{
						RackLevel _level = m_Levels[(int)_levelIndex - 1];
						if (_level != null)
						{
							if (PROP_PALLETS_ARE_EQUAL == strLevelProp)
								return _level.PalletsAreEqual;
							else if (PROP_NUMBER_OF_PALLETS == strLevelProp)
								return _level.NumberOfPallets;
							else if (strLevelProp.StartsWith(PROP_PALLET_CONFIGURATION))
							{
								if (_level.Pallets == null)
									return null;

								int _palletIndex = -1;
								string strPalletIndex = strLevelProp.Replace(PROP_PALLET_CONFIGURATION, string.Empty);
								if (strPalletIndex.Length > 0)
								{
									try
									{
										_palletIndex = Convert.ToInt32(strPalletIndex);
									}
									catch { }
								}

								if (_palletIndex >= 0 && _palletIndex < _level.Pallets.Count)
									return _level.Pallets[_palletIndex].PalletConfiguration;
							}
							else if (strLevelProp.StartsWith(PROP_PALLET_LENGTH))
							{
								if (_level.Pallets == null)
									return 0;

								int _palletIndex = -1;
								string strPalletIndex = strLevelProp.Replace(PROP_PALLET_LENGTH, string.Empty);
								if (strPalletIndex.Length > 0)
								{
									try
									{
										_palletIndex = Convert.ToInt32(strPalletIndex);
									}
									catch { }
								}

								if (_palletIndex >= 0 && _palletIndex < _level.Pallets.Count)
									return _level.Pallets[_palletIndex].Length;
							}
							else if (strLevelProp.StartsWith(PROP_PALLET_DEPTH))
							{
								if (_level.Pallets == null)
									return 0;

								int _palletIndex = -1;
								string strPalletIndex = strLevelProp.Replace(PROP_PALLET_DEPTH, string.Empty);
								if (strPalletIndex.Length > 0)
								{
									try
									{
										_palletIndex = Convert.ToInt32(strPalletIndex);
									}
									catch { }
								}

								if (_palletIndex >= 0 && _palletIndex < _level.Pallets.Count)
									return _level.Pallets[_palletIndex].Width;
							}
							else if (strLevelProp.StartsWith(PROP_PALLET_HEIGHT))
							{
								if (_level.Pallets == null)
									return 0;

								int _palletIndex = -1;
								string strPalletIndex = strLevelProp.Replace(PROP_PALLET_HEIGHT, string.Empty);
								if (strPalletIndex.Length > 0)
								{
									try
									{
										_palletIndex = Convert.ToInt32(strPalletIndex);
									}
									catch { }
								}

								if (_palletIndex >= 0 && _palletIndex < _level.Pallets.Count)
									return _level.Pallets[_palletIndex].Height;
							}
							else if (strLevelProp.StartsWith(PROP_PALLET_LOAD))
							{
								if (_level.Pallets == null)
									return 0;

								int _palletIndex = -1;
								string strPalletIndex = strLevelProp.Replace(PROP_PALLET_LOAD, string.Empty);
								if (strPalletIndex.Length > 0)
								{
									try
									{
										_palletIndex = Convert.ToInt32(strPalletIndex);
									}
									catch { }
								}

								if (_palletIndex >= 0 && _palletIndex < _level.Pallets.Count)
									return _level.Pallets[_palletIndex].Load;
							}
							else if (PROP_LEVEL_HEIGHT == strLevelProp)
								return _level.LevelHeight;
							else if (PROP_LEVEL_LOAD == strLevelProp)
								return _level.LevelLoad;
							else if (PROP_LEVEL_ACCESSORIES == strLevelProp)
								return _level.Accessories;
						}
					}
				}

				return string.Empty;
			}

			return base.GetPropertyValue(strPropSysName);
		}

		//=============================================================================
		public override bool SetPropertyValue(string strPropSysName, object propValue, bool bWasChangedViaProperties, bool bChangeTheSameRectangles, bool bNotifySheet, out string strError, bool bCheckLayout = true)
		{
			GeometryState oldState = this._GetClonedState();

			// Place rack which is edited right now at advanced properties tab in the end of rack change order.
			//
			// Probably user changes any property which calls RecalculateColumn.
			// RecalculateColumn() can change column on all racks in this group, so need to call recalc index
			// for the group.
			// But need to place this rack in the end of list, read comment below.
			// Place currect(changed) rack to the guarantees that current rack will receive new index if all other racks in the row were not changed.
			//
			// Otherwise this rack will keep old index(because it is first in rack change order) and all other racks in the group will receive new index.
			if (bChangeTheSameRectangles)
				m_bDontChangeOrder = true;

			// save levels data
			if (PROP_IS_UNDERPASS_AVAILABLE == strPropSysName && propValue is bool && (bool)propValue)
			{
				// Clone levels.
				List<RackLevel> oldLevelsList = new List<RackLevel>();
				if (m_Levels != null)
				{
					foreach (RackLevel rackOldLevel in m_Levels)
					{
						if (rackOldLevel == null)
							continue;

						RackLevel levelClone = rackOldLevel.Clone() as RackLevel;
						if (levelClone == null)
							continue;

						levelClone.Owner = this;
						oldLevelsList.Add(levelClone);
					}
				}

				m_LevelsBeforeUnderpassChecked = oldLevelsList;
			}

			// For Length property always pass A-rack(not first in the racks group) length.
			// A-rack length convert to M-rack length inside _SetPropertyValue() if it is necessary.
			bool bCanModifyValue = true;
			bool bChangeLength = (PROP_DIMENSION_X == strPropSysName && this.IsHorizontal) || (PROP_DIMENSION_Y == strPropSysName && !this.IsHorizontal);
			if (bChangeLength)
			{
				bCanModifyValue = false;
				if (bWasChangedViaProperties && bChangeTheSameRectangles)
				{
					// Only edited rack can modify value(using LengthStep).
					bCanModifyValue = true;
					if (this.IsFirstInRowColumn)
					{
						try
						{
							double rLength = Convert.ToDouble(propValue);
							rLength -= this.DiffBetween_M_and_A;
							propValue = rLength;
						}
						catch { }
					}
				}
			}

			bool bRes = _SetPropertyValue(strPropSysName, propValue, bWasChangedViaProperties, out strError, bCheckLayout, bCanModifyValue);
			// update properties because theay can depend on the rack state
			if (bRes)
				_UpdateProperties();

			// Length value can be modified inside _SetPropertyValue().
			// Update it for all other racks.
			if (bRes && bChangeLength && bCanModifyValue)
			{
				double rLength = this.Length;
				if (this.IsFirstInRowColumn)
					rLength -= this.DiffBetween_M_and_A;
				propValue = rLength;
			}

			// If change PROP_ARE_LEVELS_THE_SAME to true, synchronize levels properties only from first call - it has bChangeTheSameRectangles = true.
			if (bRes && PROP_ARE_LEVELS_THE_SAME == strPropSysName && m_bAreLevelsTheSame && m_Levels != null && m_Levels.Count > 0
				&& bChangeTheSameRectangles)
			{
				RackLevel selectedLevel = this.SelectedLevel;
				if (selectedLevel != null)
				{
					this.SynchronizeLevels(m_Levels.IndexOf(selectedLevel));
					// check rack height
					CheckRackHeight(false);
				}
			}

			// 
			if(bRes && 
				(PROP_UNDERPASS == strPropSysName
				|| PROP_ARE_LEVELS_THE_SAME == strPropSysName
				|| PROP_NUMBER_OF_LEVELS_EXCEPT_GROUNDLEVEL == strPropSysName
				|| PROP_COLUMN == strPropSysName
				|| strPropSysName.StartsWith(PROP_RACK_LEVEL)
				|| PROP_DIMENSION_X == strPropSysName
				|| PROP_DIMENSION_Y == strPropSysName
				|| PROP_DIMENSION_Z == strPropSysName))
			{
				if (m_LevelsBeforeUnderpassChecked != null)
					m_LevelsBeforeUnderpassChecked = null;
			}

			// any change on the advanced properties tab make m_bDisableChangeSizeGripPoints prop checked
			if(bRes &&
				(strPropSysName == PROP_IS_UNDERPASS_AVAILABLE
				|| strPropSysName == PROP_UNDERPASS
				|| strPropSysName == PROP_IS_MATERIAL_ON_GROUND
				|| strPropSysName == PROP_SHOW_PALLET
				|| strPropSysName == PROP_NUMBER_OF_LEVELS_EXCEPT_GROUNDLEVEL
				|| strPropSysName == PROP_ARE_LEVELS_THE_SAME
				|| strPropSysName == PROP_COLUMN
				|| strPropSysName == PROP_SPLIT_COLUMN
				|| strPropSysName == PROP_COLUMN_FIRST_PART_LENGTH
				|| strPropSysName == PROP_COLUMN_SECOND_PART_LENGTH
				//|| strPropSysName == PROP_BRACING
				|| strPropSysName == PROP_BRACING_TYPE
				|| strPropSysName == PROP_ACCESSORIES
				|| strPropSysName.StartsWith(PROP_RACK_LEVEL)))
			{
				if (!m_bDisableChangeSizeGripPoints)
					m_bDisableChangeSizeGripPoints = true;
			}

			if (PROP_ARE_LEVELS_THE_SAME == strPropSysName && m_bAreLevelsTheSame)
			{
				string strRackLengthError;
				this._RecalcRackLength(true, true, out strRackLengthError);
				//
				string strRackWidthError;
				this._RecalcRackWidth(out strRackWidthError);
				// Need to check rack height and fix it.
				// _RecalcRackHeight is called inside _RecalcRackLength, but it doesnt fix height if there is error.
				CheckRackHeight(false);
			}

			// Check Height of the rack, it depends on the roof properties and position\size of this rack.
			if(bRes && bCheckLayout)
				bRes = _RecalcRackHeight(out strError);

			if (bRes && IsInit)
			{
				//
				// Ask for changing all racks with the same index.
				// PROP_DIMENSION_X and PROP_DIMENSION_Y has more complex the same size racks changing algorithm
				// and it is placed inside _SetPropertyValue.
				if (//PROP_DIMENSION_X != strPropSysName
					//&& PROP_DIMENSION_Y != strPropSysName
					PROP_TOP_LEFT_POINT_X != strPropSysName
					&& PROP_TOP_LEFT_POINT_Y != strPropSysName
					// also ignore static properties - they are commnon for all racks
					//&& PROP_BRACING != strPropSysName
					&& PROP_BRACING_TYPE != strPropSysName
					&& PROP_ACCESSORIES != strPropSysName)
				{
					//
					List<Rack> racksToChangeList = new List<Rack>();

					if (bChangeTheSameRectangles)
					{
						// find rows and columns contains racks with the same index
						List<Rack> _allRacks = m_Sheet.GetAllRacks();
						foreach (Rack _r in _allRacks)
						{
							if (_r == null)
								continue;

							if (_r == this)
								continue;

							if (_r.SizeIndex == this.SizeIndex)
								racksToChangeList.Add(_r);
						}

						// ask user
						if (racksToChangeList.Count > 0)
						{
							int _index = SizeIndex + 1;
							string strPropLocalName = strPropSysName;
							if (m_Properties != null)
							{
								Property_ViewModel prop = m_Properties.FirstOrDefault(p => p != null && p.SystemName == strPropSysName);
								if (prop != null)
									strPropLocalName = prop.Name;
							}

							string strMessage = "Would you like to apply this change for all racks with same index?";
							MessageBoxResult res = MessageBox.Show(strMessage, "Warning", MessageBoxButton.YesNoCancel);
							//
							if (MessageBoxResult.No == res)
							{
								double oldDepthValue = oldState.Length_Y;
								if (!oldState.IsHorizontal)
									oldDepthValue = oldState.Length_X;
								double newDepthValue = this.Depth;
								//
								bool isDepthChanged = Utils.FNE(oldDepthValue, newDepthValue);
								// If depth is changed then racks should be regrouped, because all racks in the group should have the same depth value.
								// It means that probably some racks will be converted to M or A.
								// If current rack after depth change overlap any other geometry then delete overlapped geometry.
								//
								// For example user want to change depth of any A-rack in this row:
								// |   M  |  A  |  A  |  A  |
								if (isDepthChanged)
								{
									List<Rack> deletedRacksList = new List<Rack>();
									m_Sheet.CheckRacksGroups(out deletedRacksList);

									if (deletedRacksList.Contains(this))
									{
										bRes = false;
									}
									else
									{
										List<BaseRectangleGeometry> overlappedGeometryList = new List<BaseRectangleGeometry>();
										bRes = this.IsCorrect(out overlappedGeometryList);
										if (!bRes && overlappedGeometryList.Count > 0)
										{
											m_Sheet.DeleteGeometry(overlappedGeometryList, false, false);
											bRes = this.IsCorrect(out overlappedGeometryList);
										}
									}
								}

								m_bDontChangeOrder = false;

								if (bRes)
									_MarkStateChanged();

								if (m_Sheet != null && bNotifySheet)
									m_Sheet.OnPropertyChanged(this, strPropSysName, bRes, strError);

								this.UpdateProperties();

								return bRes;
							}
							else if(MessageBoxResult.Cancel == res)
							{
								m_bDontChangeOrder = false;

								bRes = false;
								// restore old state
								this._SetState(oldState);

								if (m_Sheet != null && bNotifySheet)
									m_Sheet.OnPropertyChanged(this, strPropSysName, bRes, strError);

								this.UpdateProperties();

								return bRes;
							}
						}
					}

					// For example user has 10 R_1 racks in the same row.
					// User increase length of the first rack the row and selects "change the size of all R_1 racks".
					// If onw of the racks doesnt fit in the layout then dont undo changes. Delte it instead.
					List<Rack> racksForDeleteList = new List<Rack>();
					// make changes
					foreach (Rack rack in racksToChangeList)
					{
						if (rack == null)
							continue;

						// synchronize levels
						// If change PROP_ARE_LEVELS_THE_SAME to true, synchronize levels properties only from first call - it has bChangeTheSameRectangles = true.
						//
						// Synchronize levels before _r.SetPropertyValue, because there will be call RecalcRackUniqueSize() inside and _r rack index recalculation.
						// So need make synchronize before it for correct new index calculation.
						if (bChangeTheSameRectangles && bRes && PROP_ARE_LEVELS_THE_SAME == strPropSysName && m_Levels != null && m_Levels.Count > 0)
						{
							RackLevel selectedLevel = this.SelectedLevel;
							if (selectedLevel != null)
								rack.SynchronizeLevels(m_Levels.IndexOf(selectedLevel));
						}

						// will call notify later
						string strChangeRackError;
						bool result = rack.SetPropertyValue(strPropSysName, propValue, bWasChangedViaProperties, false, false, out strChangeRackError);
						if(!result)
						{
							racksForDeleteList.Add(rack);
							continue;
						}
					}

					//
					if (racksForDeleteList.Count > 0 && this.Sheet != null && this.Sheet.Document != null)
					{
						List<BaseRectangleGeometry> deleteGeomList = new List<BaseRectangleGeometry>();
						deleteGeomList.AddRange(racksForDeleteList);
						this.Sheet.DeleteGeometry(deleteGeomList, false, false);

						foreach (Rack rackForDelete in racksForDeleteList)
							racksToChangeList.Remove(rackForDelete);
					}
				}
			}

			// Place edited rack to the end of rack change order.
			if (bChangeTheSameRectangles)
				m_bDontChangeOrder = false;

			if (bRes)
				_MarkStateChanged();

			if (!bRes)
				this._SetState(oldState);

			if (m_Sheet != null && bNotifySheet)
				m_Sheet.OnPropertyChanged(this, strPropSysName, bRes, strError);

			this.UpdateProperties();

			return bRes;
		}

		//=============================================================================
		public override bool SetGripPoint(int gripIndex, Point pnt, double DrawingLength, double DrawingWidth)
		{
			if (m_Sheet == null)
				return false;

			if (Wrapper == null || Wrapper.Owner == null)
				return false;

			// If this rack is in the row\column, need to stretch all row\column.
			// So need to check: is it available to stretch all row\column?
			if (GRIP_TOP_LEFT == gripIndex || GRIP_BOTTOM_RIGHT == gripIndex)
			{
				//
				List<Rack> _rackGroup = m_Sheet.GetRackGroup(this);
				//
				int currentRackIndex = _rackGroup.IndexOf(this);
				if (currentRackIndex >= 0 && _rackGroup.Count > 1)
				{
					//
					pnt = Utils.CheckBorders(pnt, 0, 0, DrawingLength, DrawingWidth, MarginX, MarginY);

					// if there are 2 or more racks in the group then
					// you can change only width or height for SINGLE rack in the row\column
					Point thisRack_NewTopLeft_GlobalPoint = this.TopLeft_GlobalPoint;
					Point thisRack_NewBotRight_GlobalPoint = this.BottomRight_GlobalPoint;
					if (this.IsHorizontal)
					{
						if (GRIP_TOP_LEFT == gripIndex)
							thisRack_NewTopLeft_GlobalPoint.X = pnt.X;
						else if (GRIP_BOTTOM_RIGHT == gripIndex)
							thisRack_NewBotRight_GlobalPoint.X = pnt.X;
					}
					else
					{
						if (GRIP_TOP_LEFT == gripIndex)
							thisRack_NewTopLeft_GlobalPoint.Y = pnt.Y;
						else if (GRIP_BOTTOM_RIGHT == gripIndex)
							thisRack_NewBotRight_GlobalPoint.Y = pnt.Y;
					}
					//
					thisRack_NewTopLeft_GlobalPoint = Utils.CheckBorders(thisRack_NewTopLeft_GlobalPoint, 0, 0, DrawingLength, DrawingWidth, MarginX, MarginY);
					thisRack_NewBotRight_GlobalPoint = Utils.CheckBorders(thisRack_NewBotRight_GlobalPoint, 0, 0, DrawingLength, DrawingWidth, MarginX, MarginY);
					// check full row\column borders
					if (GRIP_TOP_LEFT == gripIndex)
					{
						if (this.IsHorizontal)
						{
							//check left
							double rLeftX = thisRack_NewTopLeft_GlobalPoint.X;
							for (int i = currentRackIndex - 1; i >= 0; --i)
								rLeftX -= _rackGroup[i].Length_X + Rack.sHorizontalRow_GlobalGap;

							if (rLeftX < 0)
								thisRack_NewTopLeft_GlobalPoint.X += Math.Abs(rLeftX);
						}
						else
						{
							// check top
							double rTopY = thisRack_NewTopLeft_GlobalPoint.Y;
							for (int i = currentRackIndex - 1; i >= 0; --i)
								rTopY -= _rackGroup[i].Length_Y + Rack.sVerticalColumn_GlobalGap;

							if (rTopY < 0)
								thisRack_NewTopLeft_GlobalPoint.Y += Math.Abs(rTopY);
						}
					}
					else if (GRIP_BOTTOM_RIGHT == gripIndex)
					{
						if (this.IsHorizontal)
						{
							// check right
							double rRightX = thisRack_NewBotRight_GlobalPoint.X;
							for (int i = currentRackIndex + 1; i < _rackGroup.Count; ++i)
								rRightX += _rackGroup[i].Length_X + Rack.sHorizontalRow_GlobalGap;

							//
							if (rRightX > DrawingLength)
								thisRack_NewBotRight_GlobalPoint.X -= rRightX - DrawingLength;
						}
						else
						{
							// check bot
							double rBotY = thisRack_NewBotRight_GlobalPoint.Y;
							for (int i = currentRackIndex + 1; i < _rackGroup.Count; ++i)
								rBotY += _rackGroup[i].Length_Y + Rack.sVerticalColumn_GlobalGap;

							//
							if (rBotY > DrawingWidth)
								thisRack_NewBotRight_GlobalPoint.Y -= rBotY - DrawingWidth;
						}
					}
					//
					double thisRack_NewLength = Utils.GetWholeNumber(thisRack_NewBotRight_GlobalPoint.X - thisRack_NewTopLeft_GlobalPoint.X);
					thisRack_NewLength = Utils.GetWholeNumberByStep(thisRack_NewLength, StepLength_X);
					thisRack_NewLength = Utils.CheckWholeNumber(thisRack_NewLength, MinLength_X, MaxLength_X);
					if (GRIP_TOP_LEFT == gripIndex)
						thisRack_NewTopLeft_GlobalPoint.X = thisRack_NewBotRight_GlobalPoint.X - thisRack_NewLength;
					//
					double thisRack_NewWidth = Utils.GetWholeNumber(thisRack_NewBotRight_GlobalPoint.Y - thisRack_NewTopLeft_GlobalPoint.Y);
					thisRack_NewWidth = Utils.GetWholeNumberByStep(thisRack_NewWidth, StepLength_Y);
					thisRack_NewWidth = Utils.CheckWholeNumber(thisRack_NewWidth, MinLength_Y, MaxLength_Y);
					if (GRIP_TOP_LEFT == gripIndex)
						thisRack_NewTopLeft_GlobalPoint.Y = thisRack_NewBotRight_GlobalPoint.Y - thisRack_NewWidth;
					//
					if (thisRack_NewLength <= 0 || thisRack_NewWidth <= 0)
						return false;

					// calculate preview rack row\column
					//
					Rack thisPreviewRack = this.Clone() as Rack;
					if (thisPreviewRack == null)
						return false;
					thisPreviewRack.Sheet = m_Sheet;
					thisPreviewRack.TopLeft_GlobalPoint = thisRack_NewTopLeft_GlobalPoint;
					thisPreviewRack.Length_X = thisRack_NewLength;
					thisPreviewRack.Length_Y = thisRack_NewWidth;
					//
					List<Rack> _previewRacksGroup = new List<Rack>();
					_previewRacksGroup.Add(thisPreviewRack);
					// add previous racks
					Rack nextRack = thisPreviewRack;
					for (int i = currentRackIndex - 1; i >= 0; --i)
					{
						Rack previousRack = _rackGroup[i].Clone() as Rack;
						if (previousRack == null)
							return false;
						previousRack.Sheet = m_Sheet;
						previousRack.Length_X = _rackGroup[i].Length_X;
						previousRack.Length_Y = _rackGroup[i].Length_Y;
						//
						Point prevRackTopLeftPoint = nextRack.TopLeft_GlobalPoint;
						if (previousRack.IsHorizontal)
							prevRackTopLeftPoint.X -= previousRack.Length_X + Rack.sHorizontalRow_GlobalGap;
						else
							prevRackTopLeftPoint.Y -= previousRack.Length_Y + Rack.sVerticalColumn_GlobalGap;
						previousRack.TopLeft_GlobalPoint = prevRackTopLeftPoint;

						_previewRacksGroup.Insert(0, previousRack);
						nextRack = previousRack;
					}
					// add next racks
					Rack prevRack = thisPreviewRack;
					for (int j = currentRackIndex + 1; j < _rackGroup.Count; ++j)
					{
						nextRack = _rackGroup[j].Clone() as Rack;
						if (nextRack == null)
							return false;
						nextRack.Sheet = m_Sheet;
						nextRack.Length_X = _rackGroup[j].Length_X;
						nextRack.Length_Y = _rackGroup[j].Length_Y;
						//
						Point nextRackTopLeftPoint = prevRack.TopLeft_GlobalPoint;
						if (nextRack.IsHorizontal)
							nextRackTopLeftPoint.X += prevRack.Length_X + Rack.sHorizontalRow_GlobalGap;
						else
							nextRackTopLeftPoint.Y += prevRack.Length_Y + Rack.sVerticalColumn_GlobalGap;
						nextRack.TopLeft_GlobalPoint = nextRackTopLeftPoint;

						_previewRacksGroup.Add(nextRack);
						prevRack = nextRack;
					}

					//
					if (_previewRacksGroup.Count == 0)
						return false;

					//
					Rack _firstInPreview = _previewRacksGroup[0];
					Rack _lastInPreview = _previewRacksGroup[_previewRacksGroup.Count - 1];
					//
					double _previewLength = _lastInPreview.BottomRight_GlobalPoint.X - _firstInPreview.TopLeft_GlobalPoint.X;
					double _previewWidth = _lastInPreview.BottomRight_GlobalPoint.Y - _firstInPreview.TopLeft_GlobalPoint.Y;

					//
					Rack singleBigRack = this.Clone() as Rack;
					if (singleBigRack == null)
						return false;
					singleBigRack.Sheet = m_Sheet;
					singleBigRack.TopLeft_GlobalPoint = _firstInPreview.TopLeft_GlobalPoint;
					singleBigRack.Length_X = _previewLength;
					singleBigRack.Length_Y = _previewWidth;

					//
					Point singleBigRack_OldTopLeftPoint = singleBigRack.TopLeft_GlobalPoint;
					double singleBigRack_OldGlobalLength = singleBigRack.Length_X;
					double singleBigRack_OldGlobalWidth = singleBigRack.Length_Y;

					//
					List<BaseRectangleGeometry> _rectanglesToIgnore = new List<BaseRectangleGeometry>();
					if (_rackGroup != null)
						_rectanglesToIgnore.AddRange(_rackGroup);
					// check layout preview
					List<BaseRectangleGeometry> overlappedRectangles;
					if (!m_Sheet.IsLayoutCorrect(singleBigRack, _rectanglesToIgnore, out overlappedRectangles))
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
						while (singleBigRack._CalculateNotOverlapPosition(overlappedRectangles, gripIndex, DrawingLength, DrawingWidth, out newTopLeftPoint, out newGlobalLength, out newGlobalWidth))
						{
							singleBigRack.TopLeft_GlobalPoint = newTopLeftPoint;
							singleBigRack.Length_X = newGlobalLength;
							singleBigRack.Length_Y = newGlobalWidth;

							//
							if (m_Sheet.IsLayoutCorrect(singleBigRack, _rectanglesToIgnore, out overlappedRectangles))
								break;

							++iLoopCount;
							if (iLoopCount >= iMaxLoopCount)
								break;
						}

						if (!m_Sheet.IsLayoutCorrect(singleBigRack, _rectanglesToIgnore, out overlappedRectangles))
							return false;
						else
						{
							// fix
							if (GRIP_TOP_LEFT == gripIndex)
							{
								Point correctTopLeftPoint = thisPreviewRack.TopLeft_GlobalPoint;
								correctTopLeftPoint.X += singleBigRack.TopLeft_GlobalPoint.X - singleBigRack_OldTopLeftPoint.X;
								correctTopLeftPoint.Y += singleBigRack.TopLeft_GlobalPoint.Y - singleBigRack_OldTopLeftPoint.Y;
								thisPreviewRack.TopLeft_GlobalPoint = correctTopLeftPoint;
							}
							else if (GRIP_BOTTOM_RIGHT == gripIndex)
							{
								thisPreviewRack.Length_X += singleBigRack.Length_X - singleBigRack_OldGlobalLength;
								thisPreviewRack.Length_Y += singleBigRack.Length_Y - singleBigRack_OldGlobalWidth;
							}
						}
					}

					// Make changes.
					// Change order of current rack at the end, after all other racks in the group are changed.
					// Otherwise(if current rack is first in the row), current rack can keep current size index, but all
					// other racks in the row will change their index.
					this.m_bDontChangeOrder = true;
					this.TopLeft_GlobalPoint = thisPreviewRack.TopLeft_GlobalPoint;
					this.Length_X = thisPreviewRack.Length_X;
					this.Length_Y = thisPreviewRack.Length_Y;
					//
					nextRack = this;
					for (int i = currentRackIndex - 1; i >= 0; --i)
					{
						prevRack = _rackGroup[i];

						//
						Point prevRack_TopLeft_Point = nextRack.TopLeft_GlobalPoint;
						if (prevRack.IsHorizontal)
							prevRack_TopLeft_Point.X -= prevRack.Length_X + Rack.sHorizontalRow_GlobalGap;
						else
							prevRack_TopLeft_Point.Y -= prevRack.Length_Y + Rack.sVerticalColumn_GlobalGap;

						prevRack.TopLeft_GlobalPoint = prevRack_TopLeft_Point;
						nextRack = prevRack;
					}
					//
					prevRack = this;
					for (int i = currentRackIndex + 1; i < _rackGroup.Count; ++i)
					{
						nextRack = _rackGroup[i];

						//
						Point nextRack_TopLeft_Point = prevRack.TopLeft_GlobalPoint;
						if (prevRack.IsHorizontal)
							nextRack_TopLeft_Point.X += prevRack.Length_X + Rack.sHorizontalRow_GlobalGap;
						else
							nextRack_TopLeft_Point.Y += prevRack.Length_Y + Rack.sVerticalColumn_GlobalGap;

						nextRack.TopLeft_GlobalPoint = nextRack_TopLeft_Point;
						prevRack = nextRack;
					}

					//
					if (m_LevelsBeforeUnderpassChecked != null)
						m_LevelsBeforeUnderpassChecked = null;

					// Now add current rack to changes order
					m_bDontChangeOrder = false;
					_MarkStateChanged();

					this.UpdateProperties();

					return true;
				}
			}

			if (GRIP_CENTER == gripIndex)
			{
				if (!this.IsFirstInRowColumn)
				{
					// #42 - Move rack from the row using center grip point moves all next racks.
					// When user moves A-rack away from the row using center grip point then all next racks are moved.
					// It happens because moved rack get new size(converted from A to M) inside _ResetColumn() and
					// then i try to place it the row at old position. So, mark this rack not init and set new size,
					// it will not try to place it back to the row.
					bool bOldIsInit = this.IsInit;
					this.IsInit = false;
					//
					this.IsFirstInRowColumn = true;
					this._ResetColumn();
					//
					this.IsInit = bOldIsInit;
				}
			}

			bool bResult = base.SetGripPoint(gripIndex, pnt, DrawingLength, DrawingWidth);

			if(bResult)
				_MarkStateChanged();

			//
			if (bResult && (GRIP_TOP_LEFT == gripIndex || GRIP_BOTTOM_RIGHT == gripIndex))
			{
				if (m_LevelsBeforeUnderpassChecked != null)
					m_LevelsBeforeUnderpassChecked = null;
			}

			this.UpdateProperties();

			return bResult;
		}

		#endregion

		#region Public functions

		//=============================================================================
		public M_RackState Get_MRackState()
		{
			// Levels are cloned inside M_RackState constructor
			//// clone levels
			//ObservableCollection<RackLevel> _levels = Utils.DeepClone<ObservableCollection<RackLevel>>(m_Levels);

			// calc size of M-rack
			double _length = Length_X;
			double _width = Length_Y;
			if (!IsHorizontal)
			{
				_length = Length_Y;
				_width = Length_X;
			}
			//
			if (!IsFirstInRowColumn)
			{
				_length += DiffBetween_M_and_A;
				//
				// Bug: create rack with the biggest available length - click on create row - create row - A racks receive another index.
				//
				// Dont check MinLength_X and MaxLength_X, because it returns different length if current rack is M or A.
				// We always need M rack length in this method, so add column length if this rack is A.
				////
				//if (IsHorizontal)
				//	_length = Utils.CheckWholeNumber(_length, MinLength_X, MaxLength_X);
				//else
				//	_length = Utils.CheckWholeNumber(_length, MinLength_Y, MaxLength_Y);
			}

			return new M_RackState(
				_length,
				_width,
				Length_Z,
				//
				m_IsUnderpassAvailable,
				m_Underpass,
				//
				m_bIsMaterialOnGround,
				//
				m_bShowPallet,
				//
				m_Levels,
				//
				m_ColumnGUID,
				//
				m_bSplitColumn,
				m_Column_FirstPartLength,
				m_Column_SecondPartLength
				);
		}

		//=============================================================================
		public void SynchronizeLevels(int levelIndex_ZeroBased)
		{
			if (m_Levels == null || m_Levels.Count == 0)
				return;

			if (levelIndex_ZeroBased >= 0 && levelIndex_ZeroBased < m_Levels.Count)
			{
				RackLevel selectedLevel = m_Levels[levelIndex_ZeroBased];
				if (selectedLevel == null)
					return;

				RackLevel lastLevel = m_Levels.LastOrDefault();
				if (lastLevel == null)
					return;

				foreach (RackLevel level in m_Levels)
				{
					if (level == null)
						continue;

					if (level == selectedLevel)
						continue;

					//
					level.Set_PalletsAreEqual(selectedLevel.PalletsAreEqual);
					level.Set_NumberOfPallets(selectedLevel.NumberOfPallets);
					level.Set_Accessories(selectedLevel.Accessories);
					//
					foreach (Pallet pallet in selectedLevel.Pallets)
					{
						if (pallet == null)
							continue;

						UInt32 iPalletIndex = (UInt32)selectedLevel.Pallets.IndexOf(pallet);

						level.Set_PalletHeight(iPalletIndex, pallet.Height);
						level.Set_PalletLength(iPalletIndex, pallet.Length);
						level.Set_PalletWidth(iPalletIndex, pallet.Width);
						level.Set_PalletLoad(iPalletIndex, pallet.Load);
						level.Set_PalletConfiguration(iPalletIndex, pallet.PalletConfiguration);
					}

					//
					string strError;
					level.RecalcLevelHeight(out strError);
				}

				string strRackHeightError;
				_RecalcBeamSize(false, out strRackHeightError);
			}
		}

		//=============================================================================
		public bool OnLengthOrWidthChanged(out string strError, bool bRecalcPalletLength = true, bool bRecalcPalletWidth = true)
		{
			strError = string.Empty;

			CheckRackHeight();
			if (!_RecalcColumnSize(out strError))
				return false;
			if (!_RecalcBeamSize(true, out strError))
				return false;
			if(bRecalcPalletLength)
				_RecalcPalletLength();
			if(bRecalcPalletWidth)
				_RecalcPalletWidth();

			return true;
		}

		//=============================================================================
		// Check rack height and Max Available Height value.
		// Try to fix it, if rack height is greater than available height.
		public bool CheckRackHeight(bool bRecalculateLevelsHeightBeforeDeleteLevels = true)
		{
			string strError;
			if (!_RecalcRackHeight(out strError))
			{
				double newLengthZ = MaxLength_Z;
				// remove maximum value from between the last level pallet height and add last level height,
				// look at the picture at advanced properties tab
				if (Utils.FEQ(newLengthZ, ClearAvailableHeight) && m_Levels != null)
				{
					RackLevel lastLevel = m_Levels.LastOrDefault();
					if (lastLevel != null)
					{
						if(lastLevel.TheBiggestPalletHeightWithRiser > lastLevel.LevelHeight)
							newLengthZ -= lastLevel.TheBiggestPalletHeightWithRiser - lastLevel.LevelHeight;
					}
				}
				//
				newLengthZ = Math.Floor(newLengthZ);
				Length_Z = Utils.GetWholeNumber(newLengthZ);

				// Dont delete levels, try to recalculate levels height from RackHeight.
				if(bRecalculateLevelsHeightBeforeDeleteLevels)
				{
					if (_On_Height_Changed(true, out strError))
						return true;
				}

				//
				newLengthZ = Math.Floor(newLengthZ);
				Length_Z = Utils.GetWholeNumber(newLengthZ);
				// Try to delete last levels until rack height will be less than MaxLength_Z.
				bool isRackHeightOK = false;
				while(this.NumberOfLevels_WithoutGround > Rack.sMinNumberOfLevels)
				{
					string strNumberOfLevelsErr;
					// delete the last level
					if (!_On_NumberOfLevels_Changed(this.NumberOfLevels_WithoutGround - 1, false, out strNumberOfLevelsErr))
						break;

					// check max rack height
					if (_RecalcRackHeight(out strError))
					{
						isRackHeightOK = true;
						break;
					}
				}

				if (isRackHeightOK)
					return true;

				// Max rack height problem was not solved by delete levels.
				// So recalculate levels from max rack height.
				newLengthZ = Math.Floor(newLengthZ);
				Length_Z = Utils.GetWholeNumber(newLengthZ);
				//
				if (!_On_Height_Changed(true, out strError))
				{
					// Decrease number of levels, probably level height is less than MIN_LEVEL_HEIGHT value.
					UInt32 newNumberOfLevels = (UInt32)Math.Floor((double)this.NumberOfLevels_WithoutGround / 2);
					if (newNumberOfLevels < Rack.sMinNumberOfLevels)
						newNumberOfLevels = (UInt32)Rack.sMinNumberOfLevels;
					if (newNumberOfLevels != this.NumberOfLevels_WithoutGround)
					{
						if (_On_NumberOfLevels_Changed(newNumberOfLevels, true, out strError))
							return true;
					}

					return false;
				}
			}

			return true;
		}

		//=============================================================================
		// Underpass depends on the DrawingDocument.OverallHeightLowered property.
		public bool IsUnderpassCorrect(out string strError, bool bTryToFixIt)
		{
			strError = string.Empty;
			if (m_Sheet == null || m_Sheet.Document == null)
				return false;

			// Underpass value should be greater or equal OverallHeightLowered + RACK_UNDERPASS_GAP.
			// RACK_UNDERPASS_GAP is included in DrawingDocument.OverallHeightLowered.
			double underpassMinValue = m_Sheet.Document.OverallHeightLowered;
			double underpassMaxValue = MAX_USL_DISTANCE;

			//
			if(Utils.FGT(underpassMinValue, underpassMaxValue))
			{
				strError = "Error. Underpass can't be greater than " + MAX_USL_DISTANCE.ToString() + ".";
				return false;
			}

			if(m_Levels != null)
			{
				RackLevel firstLevel = m_Levels.FirstOrDefault();
				if (firstLevel != null && firstLevel.Beam != null)
					underpassMaxValue -= firstLevel.Beam.Height;
			}
			bool bLessThanMin = Utils.FLT(m_Underpass, underpassMinValue);
			bool bMoreThanMax = Utils.FGT(m_Underpass, underpassMaxValue);
			if (bLessThanMin || bMoreThanMax)
			{
				if (bTryToFixIt)
				{
					if (bLessThanMin)
						m_Underpass = (UInt32)Utils.GetWholeNumber(underpassMinValue);
					else if (bMoreThanMax)
					{
						// Dont fix underpass value. It cant be greater than MAX_USL_DISTANCE.
						//m_Underpass = (UInt32)Utils.GetWholeNumber(underpassMaxValue);
						strError = "Error. Underpass can't be greater than " + MAX_USL_DISTANCE.ToString() + ".";
						return false;
					}

					// Recalculate column and rack height only if Underpass is checked
					if (this.IsUnderpassAvailable)
					{
						// X Bracing height depends on the Underpass value.
						string strColumnCalcError;
						if (!RecalculateColumn(false, out strColumnCalcError))
						{
							strError = strColumnCalcError;
							return false;
						}

						//
						string strRackHeightError;
						if (!_RecalcRackHeight(out strRackHeightError))
						{
							strError = strRackHeightError;
							return false;
						}
					}

					return true;
				}
				else
				{
					if(bLessThanMin)
						strError = "Underpass value should be greater or equal than \"" + underpassMinValue + "\"(OverallHeightLowered + 200).";
					else if(bMoreThanMax)
						strError = "Underpass value should be less or equal than \"" + underpassMaxValue + "\"(max distance to the first beam).";

					return false;
				}
			}

			return true;
		}

		//=============================================================================
		/// <summary>
		/// Calculate load which takes column in current point.
		/// </summary>
		public double CalculateColumnLoadInPoint(double distanceToTheTopOfLevelBeam, out RackLevel previosLevel)
		{
			previosLevel = null;
			if (Utils.FLE(distanceToTheTopOfLevelBeam, 0.0))
				return 0.0;
			if (m_Levels == null)
				return 0.0;

			double load = 0.0;
			foreach(RackLevel level in m_Levels)
			{
				if (level == null)
					continue;
				if (level.Index == 0)
					continue;

				double distanceToTheTopOfBeam = level.DistanceFromTheGround;
				if (level.Beam != null)
					distanceToTheTopOfBeam += level.Beam.Height;
				if (Utils.FGE(distanceToTheTopOfBeam, distanceToTheTopOfLevelBeam))
					load += level.LevelLoad;
				else
					previosLevel = level;
			}

			// Each column takes only half of the load.
			return load / 2;
		}

		//=============================================================================
		/// <summary>
		/// Changes rack column if it is possible.
		/// </summary>
		public bool Set_Column(RackColumn rackColumn, bool bSetMinColumn, bool bChangeSelectedColumn, bool bRecalcLength, out string strError)
		{
			strError = string.Empty;

			if (rackColumn == null)
				return false;

			bool isColumnChanged = false;
			if (bSetMinColumn)
			{
				if (m_MinColumnGUID != rackColumn.GUID)
				{
					m_MinColumnGUID = rackColumn.GUID;
					isColumnChanged = true;
				}
				if (m_ColumnGUID == Guid.Empty || (bChangeSelectedColumn && !m_IsColumnSetManually && m_ColumnGUID != m_MinColumnGUID))
				{
					m_ColumnGUID = m_MinColumnGUID;
					if (!isColumnChanged)
						isColumnChanged = true;
				}
			}
			else
			{
				if (m_ColumnGUID != rackColumn.GUID)
				{
					m_ColumnGUID = rackColumn.GUID;
					isColumnChanged = true;
				}
				if (m_MinColumnGUID == Guid.Empty)
				{
					m_MinColumnGUID = m_ColumnGUID;
					if (!isColumnChanged)
						isColumnChanged = true;
				}
			}

			// Check selected column, it should be greater or equal than m_MinColumnGUID column.
			RackColumn minColumn = this._GetColumnByGUID(m_MinColumnGUID);
			RackColumn selectedColumn = this._GetColumnByGUID(m_ColumnGUID);
			if (minColumn == null || selectedColumn == null)
				return false;

			// Compare by the length and thickness, look at the LoadChart.xlsx file.
			if (Utils.FGT(minColumn.Length, selectedColumn.Length) || (Utils.FGE(minColumn.Length, selectedColumn.Length) && Utils.FGT(minColumn.Thickness, selectedColumn.Thickness)))
			{
				if (bSetMinColumn)
				{
					// Change the selected column, because min available column is greater than selected.
					m_ColumnGUID = m_MinColumnGUID;
					m_IsColumnSetManually = false;
					if (!isColumnChanged)
						isColumnChanged = true;
				}
				else
				{
					strError = "Selected column cant be less than minimum available column for this rack - " + minColumn.Name;
					return false;
				}
			}

			// If nothing was changed then dont call _RecalcRackLength(), because RackUtils.ChangeRack takes a lot of time.
			if (!isColumnChanged)
				return true;

			_MarkStateChanged();

			// Recalculate column for entire rack group.
			if (this.IsInit && !this.RecalculateColumn(bRecalcLength, out strError))
				return false;

			if(bRecalcLength)
				return this._RecalcRackLength(true, false, out strError);

			return true;
		}

		//=============================================================================
		public bool RecalculateColumn(bool bRecalcRackLength, out string strError)
		{
			strError = string.Empty;

			if(this.Sheet == null)
			{
				strError = "Cant calculate column for the rack, because Sheet reference is null.";
				return false;
			}

			if(this.Sheet.Document == null)
			{
				strError = "Cant calculate column for the rack, because Document reference is null.";
				return false;
			}

			if (this.Sheet.Document.RacksColumnsList == null || this.Sheet.Document.RacksColumnsList.Count == 0)
			{
				strError = "Cant calculate column for the rack, because Column List is empty.";
				return false;
			}

			// Need to check columns on this rack and neighbors.
			List<Rack> racksList = new List<Rack>();
			if(this.IsInit && this.Sheet != null)
			{
				// All rack in the group should have the same column and bracing.
				List<Rack> rackGroup = this.Sheet.GetRackGroup(this);
				if (rackGroup != null && rackGroup.Contains(this))
					racksList.AddRange(rackGroup);
			}
			if(racksList.Count == 0)
				racksList.Add(this);

			eColumnBracingType bracingType;
			//double xBracingHeight = 0.0;
			RackColumn minAvailableColumn = null;
			double stiffenersHeight = 0.0;
			if (!RackUtils.CalculateRacksColumnSizeAndBracingType(racksList, this.Sheet.Document.RacksColumnsList, out bracingType, /*out xBracingHeight,*/ out minAvailableColumn, out stiffenersHeight))
			{
				strError = "Cant calculate column for the rack.";
				return false;
			}

			// All racks in the group should have the same column, so take the biggest one.
			RackColumn selectedColumn = minAvailableColumn;
			foreach (Rack rack in racksList)
			{
				if (rack == null || rack.Column == null)
					continue;

				// Dont consider columns which are not set manually.
				// Racks group should have the minimum available column if it is not set manually.
				if (!rack.IsColumnSetManually)
					continue;

				if (rack.Column == selectedColumn)
					continue;
				else
				{
					if (Utils.FGT(rack.Column.Length, selectedColumn.Length) || (Utils.FGE(rack.Column.Length, selectedColumn.Length) && Utils.FGE(rack.Column.Thickness, selectedColumn.Thickness)))
					{
						RackColumn newSelectedColumn;
						eColumnBracingType newBracingType;
						//double newColumnBracingHeight = 0.0;
						// check X bracing height for the new column
						double newStiffenersHeight = 0.0;
						if (RackUtils.CalculateRacksColumnSizeAndBracingType(racksList, new List<RackColumn>() { rack.Column}, out newBracingType, /*out newColumnBracingHeight,*/ out newSelectedColumn, out newStiffenersHeight))
						{
							selectedColumn = rack.Column;
							bracingType = newBracingType;
							//xBracingHeight = newColumnBracingHeight;
							stiffenersHeight = newStiffenersHeight;
						}
					}
				}
			}

			// Apply column and bracing to all columns in the group.
			double xBracingHeight = 0.0;
			if ((eColumnBracingType.eXBracing == bracingType || eColumnBracingType.eXBracingWithStiffener == bracingType) /*&& Utils.FGT(xBracingHeight, 0.0)*/)
			{
				// Check min X bracing height.
				foreach(Rack rack in racksList)
				{
					if (rack == null)
						continue;

					double minXBracingHeight = rack.MinXBracingHeight;
					if (Utils.FGT(minXBracingHeight, xBracingHeight))
						xBracingHeight = minXBracingHeight;
				}
			}

			// If rack is not initialized then it doesnt have any group.
			// Just set column and dont check anything.
			if (!this.IsInit)
			{
				if(this.MinimumColumn != minAvailableColumn && !this.Set_Column(minAvailableColumn, true, false, true, out strError))
					return false;

				if(selectedColumn != this.Column)
					return this.Set_Column(selectedColumn, false, false, true, out strError);

				return true;
			}

			// Set IsInit to false and set Column with Length without layout check.
			bool bOldIsInitState = this.IsInit;
			this.IsInit = false;
			// Dont calc rack length inside Set_Column, because it can return an error.
			string strColumnError;
			this.Set_Column(minAvailableColumn, true, false, false, out strColumnError);
			if(selectedColumn != this.Column)
				this.Set_Column(selectedColumn, false, false, false, out strColumnError);
			// Now recalc beams and length. Ignore all errors.
			string strLengthCalcError;
			this._RecalcRackLength(true, false, out strLengthCalcError);
			this.Bracing = bracingType;
			this.X_Bracing_Height = xBracingHeight;
			this.StiffenersHeight = stiffenersHeight;
			this.IsInit = bOldIsInitState;
			// Now try to place this rack to his group.
			List<Rack> previewGroup = new List<Rack>();
			List<Rack> deletedRacksList;
			RackColumn minColumn = null;
			if (!RackUtils.TryToReplaceRackInGroup(this.Sheet, this, racksList.IndexOf(this), false, racksList, out previewGroup, out deletedRacksList, out minColumn) || minColumn == null)
			{
				strError = "Result layout is not correct.";
				return false;
			}

			//
			foreach (Rack rack in racksList)
			{
				if (deletedRacksList.Contains(rack))
					continue;

				rack.Bracing = bracingType;
				rack.X_Bracing_Height = xBracingHeight;
				rack.StiffenersHeight = stiffenersHeight;

				bool recalcLength = true;
				if (rack == this)
					recalcLength = bRecalcRackLength;

				rack.Set_Column(minAvailableColumn, true, false, recalcLength, out strError);
				if(rack.Column != selectedColumn)
					rack.Set_Column(selectedColumn, false, false, recalcLength, out strError);
			}

			return true;
		}

		//=============================================================================
		public void MarkStateChanged()
		{
			this._MarkStateChanged();
		}
		protected override void _MarkStateChanged()
		{
			// Update change order
			if (!m_bDontChangeOrder)
			{
				if (m_ChangeOrder < 0)
				{
					if (this.Sheet == null || this.Sheet.Document == null)
						m_ChangeOrder = 0;
					else
					{
						++this.Sheet.Document.m_RackChangeOrder;
						m_ChangeOrder = this.Sheet.Document.m_RackChangeOrder;
					}
				}
			}

			base._MarkStateChanged();
		}

		//=============================================================================
		/// <summary>
		/// Clear m_ChangeOrder field.
		/// Need to do it when command ended.
		/// </summary>
		public void ClearChangeOrder()
		{
			m_ChangeOrder = -1;
		}

		#endregion

		#region Private functions

		//=============================================================================
		private void _RecalcText()
		{
			// name
			int index = m_iSizeIndex + 1;
			string strName = RackUtils.GetAlphabetRackIndex(index);

			strName += "(";
			if (IsFirstInRowColumn)
				strName += "M";
			else
				strName += "A";
			strName += ")";

			Text = strName;
		}

		//=============================================================================
		/// <summary>
		/// Set number of levels.
		/// </summary>
		private bool _On_NumberOfLevels_Changed(UInt32 numOfLevels_ExceptGroundLevel, bool bCheckRackHeight, out string strError)
		{
			strError = string.Empty;

			// copy last level properties to last level
			RackLevel lastLevel = m_Levels.LastOrDefault();
			int lastLevelIndex = -1;
			int lastLevelHeight = -1;
			if (lastLevel != null)
			{
				lastLevelIndex = (int)lastLevel.Index;
				lastLevelHeight = (int)lastLevel.LevelHeight;
			}

			int levelsCount_ExceptGroundLevel = (int)this.NumberOfLevels_WithoutGround;
			// add levels and props
			bool bLevelsAdded = false;
			for (UInt32 i = 1; i <= numOfLevels_ExceptGroundLevel; ++i)
			{
				if (i <= levelsCount_ExceptGroundLevel)
					continue;

				RackLevel level = m_Levels.FirstOrDefault(l => l != null && l.Index == i);
				if (level == null)
				{
					level = new RackLevel(this);
					UInt32 iLevelIndex = i;
					//if (bDoesGroundLevelExist && iLevelIndex > 0)
					//	--iLevelIndex;
					level.Index = iLevelIndex;

					// stretch pallet length to rack length
					double _value = this.Length_X;
					if (!this.IsHorizontal)
						_value = this.Length_Y;
					// convert it to M-rack
					if (!this.IsFirstInRowColumn)
						_value += this.DiffBetween_M_and_A;
					//
					_value -= 2 * this.DiffBetween_M_and_A;
					//
					double _palletLength = _value;
					_palletLength -= (level.Pallets.Count + 1) * Rack.sDefaultDistanceBetweenPallet;
					_palletLength = _palletLength / level.Pallets.Count;
					//
					foreach (Pallet _pallet in level.Pallets)
					{
						if (_pallet == null)
							continue;

						_pallet.Set_Length((UInt32)Utils.GetWholeNumber(_palletLength));
					}

					m_Levels.Add(level);
					bLevelsAdded = true;

					string strRackHeightError;
					_RecalcBeamSize(level, false, out strRackHeightError);
				}
			}

			if(bLevelsAdded)
				_MarkStateChanged();

			// add new levels
			if (lastLevel != null && lastLevelIndex < numOfLevels_ExceptGroundLevel)
				lastLevel.RecalcLevelHeight(out strError);

			if (m_bAreLevelsTheSame && bLevelsAdded && m_Levels.Count > 0)
				this.SynchronizeLevels(0);

			// Remove levels
			List<RackLevel> levelsForRemove = new List<RackLevel>();
			for (UInt32 index = 1; index <= m_Levels.Count; ++index)
			{
				if (index > numOfLevels_ExceptGroundLevel)
				{
					RackLevel foundLevel = m_Levels.FirstOrDefault(level => level != null && level.Index == index);
					if (foundLevel == null)
						continue;

					levelsForRemove.Add(foundLevel);
				}
			}
			if(levelsForRemove.Count > 0)
				_MarkStateChanged();
			foreach (RackLevel level in levelsForRemove)
				m_Levels.Remove(level);

			// copy last level height to the last level
			if (lastLevel != null)
			{
				//
				RackLevel newLastLevel = m_Levels.LastOrDefault();
				if (newLastLevel != null && newLastLevel != lastLevel && lastLevelHeight > 0)
					newLastLevel.Set_LevelHeight((UInt32)lastLevelHeight);
			}

			//
			if (this.SelectedLevel == null && m_Levels.Count > 0)
				this.SelectedLevel = m_Levels[0];

			// Recalculate column. Column drives the length of the rack, so need to check length and result layout(overlap).
			if (!this.RecalculateColumn(true, out strError))
				return false;

			// If count of levels was changed then need to recalc rack's height.
			if (bCheckRackHeight)
			{
				CheckRackHeight();
				// #40 - Not possible to change number of levels from 10 to 5
				return _On_Height_Changed(false, out strError);
			}

			return true;
		}

		//=============================================================================
		private bool _On_Height_Changed(bool bRecalcLevelHeight, out string strMessage)
		{
			strMessage = string.Empty;

			// recalc level height before working with the total rack height with last level pallets
			if (bRecalcLevelHeight)
			{
				if (!_RecalcLevelHeight(out strMessage))
					return false;
			}

			// if Length_Z < 5500 then set SplitColumn = false
			if (Length_Z < 5500)
				m_bSplitColumn = false;
			// if height is more than 10000 then Split Column is mandatory and values will be 5000 + 5000
			else if (Length_Z > 10000)
			{
				m_bSplitColumn = true;

				m_Column_FirstPartLength = 5000;
				m_Column_SecondPartLength = 5000;
			}
			else
			{
				if (m_Column_FirstPartLength < Length_Z)
				{
					m_Column_SecondPartLength = (UInt32)Length_Z - m_Column_FirstPartLength;
				}
				else if (m_Column_SecondPartLength < Length_Z)
				{
					m_Column_FirstPartLength = (UInt32)Length_Z - m_Column_SecondPartLength;
				}
				else
				{
					m_Column_FirstPartLength = 5000;
					m_Column_SecondPartLength = (UInt32)Length_Z - m_Column_FirstPartLength;
				}
			}

			return true;
		}

		//=============================================================================
		/// <summary>
		/// Changes levels count when Unserpass is checked or restore old levels when underpass is unchecked.
		/// </summary>
		/// <param name="levelsOffsetFromTheGround">
		/// Level with Index=1 offset from the ground.
		/// Ground level(Index=0) is not considered because when Underpass is checked ground level should be removed.
		/// </param>
		private bool _On_UnderpassAvailable_Changed(int levelsOffsetFromTheGround, out string strError)
		{
			strError = string.Empty;

			if (this.IsUnderpassAvailable)
			{
				// the minimum number of levels(without ground levels) is sMinNumberOfLevels
				if (m_Levels.Count() <= sMinNumberOfLevels)
				{
					strError = "Levels count cant be less than " + sMinNumberOfLevels.ToString();
					return false;
				}

				// Update undeprass value
				string strUnderpassError;
				if (!this.IsUnderpassCorrect(out strUnderpassError, true))
				{
					strError = strUnderpassError;
					return false;
				}

				// try to remove levels which are placed in (0, m_Underpass) height.
				double totalRemovedHeight = levelsOffsetFromTheGround;
				List<RackLevel> levelsForRemoveList = new List<RackLevel>();
				for(int i=0; i<m_Levels.Count()-2; ++i)
				{
					RackLevel level = m_Levels[i];
					if (level == null)
						continue;

					levelsForRemoveList.Add(level);
					totalRemovedHeight += level.LevelHeight;
					// Ground level doesnt have beam.
					if (level.Index > 0 && level.Beam != null)
						totalRemovedHeight += level.Beam.Height;

					if (Utils.FGE(totalRemovedHeight, m_Underpass))
						break;
				}

				// dont change the result rack height, so set Underpass = totalRemovedHeight
				m_Underpass = (UInt32)Utils.GetWholeNumber(totalRemovedHeight);

				// remove levels
				foreach (RackLevel level in levelsForRemoveList)
				{
					if (level == null)
						continue;

					m_Levels.Remove(level);
				}
				// recalc levels index
				foreach(RackLevel level in m_Levels)
				{
					if (level == null)
						continue;

					string strLevelHeightError;
					level.RecalcLevelHeight(out strLevelHeightError);
					level.Index = (UInt32)(m_Levels.IndexOf(level) + 1);
				}
				// update selected level, probably it was removed
				if (this.SelectedLevel == null && m_Levels.Count > 0)
					this.SelectedLevel = m_Levels[0];

				// remove and add all levels for update collection in the tab
				List<RackLevel> allLevels = m_Levels.ToList();
				m_Levels.Clear();
				foreach (RackLevel level in allLevels)
				{
					if (level == null)
						continue;

					m_Levels.Add(level);
				}

				// the minimum number of levels(except ground levels) is sMinNumberOfLevels
				if (m_Levels.Count < sMinNumberOfLevels)
				{
					if (!_On_NumberOfLevels_Changed(sMinNumberOfLevels, false, out strError))
						return false;
				}

				// Check USL distance for the first level.
				RackLevel firstLevel = m_Levels.FirstOrDefault(lvl => lvl != null && lvl.Index == 1);
				if (firstLevel == null)
				{
					strError = "Cant find the first level.";
					return false;
				}
				else
				{
					double uslDistance = firstLevel.DistanceFromTheGround;
					if (firstLevel.Beam != null)
						uslDistance += firstLevel.Beam.Height;
					if (Utils.FGT(uslDistance, MAX_USL_DISTANCE))
					{
						strError = "The first level USL distance(" + uslDistance.ToString() + ") is greater than max USL distance(" + MAX_USL_DISTANCE.ToString() + ").";
						return false;
					}
				}

				_MarkStateChanged();

				CheckRackHeight();
			}
			else
			{
				// try to restore old levels
				if (m_LevelsBeforeUnderpassChecked != null)
				{
					m_Levels.Clear();
					foreach (RackLevel _level in m_LevelsBeforeUnderpassChecked)
					{
						if (_level == null)
							continue;

						m_Levels.Add(_level);
					}

					// If m_LevelsBeforeUnderpassChecked contains ground level then need to check m_bIsMaterialOnGround.
					// Ground level has Index = 0.
					m_bIsMaterialOnGround = m_Levels.FirstOrDefault(level => level != null && level.Index == 0) != null;

					_MarkStateChanged();

					//
					string strRackLengthError;
					_RecalcRackLength(true, true, out strRackLengthError);
					//
					string strRackWidthError;
					_RecalcRackWidth(out strRackWidthError);
					// Need to check rack height and fix it.
					// _RecalcRackHeight is called inside _RecalcRackLength, but it doesnt fix height if there is error.
					CheckRackHeight();
				}
				else
				{
					_MarkStateChanged();

					// recalc the levels like them are the same
					return _RecalcLevelHeight(out strError);
				}
			}

			return true;
		}


		//=============================================================================
		private bool _CheckMaterialOnGroundLevel(out string strError)
		{
			strError = string.Empty;

			if (m_Levels == null && m_Levels.Count > 1)
				return false;

			RackLevel groundLevel = m_Levels.FirstOrDefault(l => l != null && l.Index == 0);
			if (this.IsMaterialOnGround)
			{
				if (groundLevel != null)
					return true;

				groundLevel = m_Levels[0].Clone() as RackLevel;
				if(groundLevel == null)
				{
					strError = "Error. Cant clone level.";
					return false;
				}

				groundLevel.Owner = this;
				groundLevel.Index = 0;
				m_Levels.Insert(0, groundLevel);

				//
				if (!this.AreLevelsTheSame)
				{
					//
					_RecalcPalletLength(groundLevel);
					_RecalcPalletWidth(groundLevel);
				}
			}
			else
			{
				if (groundLevel != null)
					m_Levels.Remove(groundLevel);
			}

			// minimum number of levels - sMinNumberOfLevels
			if (m_Levels.Count == 1)
			{
				if (!_On_NumberOfLevels_Changed(Rack.sMinNumberOfLevels, false, out strError))
					return false;
			}
			if (this.SelectedLevel == null && m_Levels.Count > 0)
				this.SelectedLevel = m_Levels[0];

			// Recalc level height, because it should satisfy rules from Rack._RecalcLevelHeight() function.
			foreach (RackLevel level in m_Levels)
			{
				if (level == null)
					continue;

				if (!level.RecalcLevelHeight(out strError))
					return false;
			}
			// Check rack height after that, if rack height goes out limits then correct it
			this.CheckRackHeight();

			_MarkStateChanged();

			return true;
		}

		//=============================================================================
		private bool _SetPropertyValue(string strPropSysName, object propValue, bool bWasChangedViaProperties, out string strError, bool bCheckLayout = true, bool bCanModifyValue = true)
		{
			strError = string.Empty;

			if (m_Sheet == null || m_Sheet.Document == null)
				return false;

			// If the size(length and width) of rack was changed via properties, then 
			// need to ask user for change all the same racks.
			if (PROP_DIMENSION_X == strPropSysName || PROP_DIMENSION_Y == strPropSysName)
			{
				try
				{
					bool bChangeLength = (PROP_DIMENSION_X == strPropSysName && this.IsHorizontal) || (PROP_DIMENSION_Y == strPropSysName && !this.IsHorizontal);

					//
					double rNewValue = Convert.ToDouble(propValue);
					// If it is Length property then propValue contains A-rack length.
					// Read comment in SetPropertyValue().
					// Convert it to M-rack length if it is M-rack.
					if (this.IsFirstInRowColumn && bChangeLength)
						rNewValue += this.DiffBetween_M_and_A;
					if (Utils.FLE(rNewValue, 0.0))
						return false;

					double newLength_X = this.Length_X;
					double newLength_Y = this.Length_Y;

					//
					if (PROP_DIMENSION_X == strPropSysName)
					{
						// if value was changed via properties and it is not correct, then revert changes
						if (bWasChangedViaProperties)
						{
							if (Utils.FLT(rNewValue, MinLength_X))
							{
								strError += "Length is less then min value(";
								strError += MinLength_X.ToString();
								strError += ").";

								return false;
							}
							if (double.IsPositiveInfinity(MaxLength_X))
							{
								if (Utils.FGT(rNewValue, m_Sheet.Length))
								{
									strError += "Length is bigger then sheet length(";
									strError += m_Sheet.Length.ToString();
									strError += ").";

									return false;
								}
							}
							else
							{
								if (Utils.FGT(rNewValue, MaxLength_X))
								{
									strError += "Length is bigger then max value(";
									strError += MaxLength_X.ToString();
									strError += ").";

									return false;
								}
							}
						}

						//
						if (bCanModifyValue)
						{
							if (m_Length_X % StepLength_X != 0)
							{
								strError += "Length should be divisible by ";
								strError += StepLength_X.ToString();
								strError += " without remainder.";
							}

							rNewValue = Utils.GetWholeNumberByStep(rNewValue, StepLength_X);
							rNewValue = Utils.CheckWholeNumber(rNewValue, MinLength_X, MaxLength_X);
						}

						newLength_X = rNewValue;
					}
					else
					{
						// if value was changed via properties and it is not correct, then revert changes
						if (bWasChangedViaProperties)
						{
							if (Utils.FLT(rNewValue, MinLength_Y))
							{
								strError += "Depth is less then min value(";
								strError += MinLength_Y.ToString();
								strError += ").";

								return false;
							}
							if (double.IsPositiveInfinity(MaxLength_Y))
							{
								if (Utils.FGT(rNewValue, m_Sheet.Width))
								{
									strError += "Depth is bigger then sheet depth(";
									strError += m_Sheet.Width.ToString();
									strError += ").";

									return false;
								}
							}
							else
							{
								if (Utils.FGT(rNewValue, MaxLength_Y))
								{
									strError += "Depth is bigger then max value(";
									strError += MaxLength_Y.ToString();
									strError += ").";

									return false;
								}
							}
						}

						//
						if (bCanModifyValue)
						{
							if (m_Length_X % StepLength_X != 0)
							{
								strError += "Depth should be divisible by ";
								strError += StepLength_Y.ToString();
								strError += " without remainder.";
							}

							rNewValue = Utils.GetWholeNumberByStep(rNewValue, StepLength_Y);
							rNewValue = Utils.CheckWholeNumber(rNewValue, MinLength_Y, MaxLength_Y);
						}

						newLength_Y = rNewValue;
					}

					bool bRes = RackUtils.ChangeRack(this, newLength_X, newLength_Y, true, out strError);
					if (!bRes)
						return false;

					return this.OnLengthOrWidthChanged(out strError, bChangeLength, !bChangeLength);
				}
				catch { }

				return true;
			}
			else if (PROP_DIMENSION_Z == strPropSysName)
			{
				try
				{
					//
					int iNewValue = Convert.ToInt32(propValue);
					if (iNewValue <= 0)
						return false;

					// check height with pallets
					int lastLevelAdditionHeight = 0;
					if (m_Levels != null)
					{
						RackLevel lastLevel = m_Levels.LastOrDefault();
						if (lastLevel != null && lastLevel.TheBiggestPalletHeightWithRiser > lastLevel.LevelHeight)
							lastLevelAdditionHeight = (int)(lastLevel.TheBiggestPalletHeightWithRiser - lastLevel.LevelHeight);
					}
					int iHeightWithPallets = iNewValue + lastLevelAdditionHeight;

					// if value was changed via properties and it is not correct, then revert changes
					if (bWasChangedViaProperties)
					{
						if (Utils.FLT(iNewValue, MinLength_Z))
						{
							strError += "Height is less then min value(";
							strError += MinLength_Z.ToString();
							strError += ").";

							return false;
						}

						if (Utils.FGT(iNewValue, MaxLength_Z))
						{
							strError += "Height is bigger then max value(";
							strError += MaxLength_Z.ToString();
							strError += ").";

							return false;
						}

						if (Utils.FGT(iHeightWithPallets, ClearAvailableHeight))
						{
							strError += "Height with pallets (";
							strError += iHeightWithPallets.ToString();
							strError += ") is bigger then max value (";
							strError += MaxLength_Z.ToString();
							strError += ").";

							return false;
						}
					}

					//
					if (Utils.FGT(iNewValue + lastLevelAdditionHeight, ClearAvailableHeight))
						iNewValue = Utils.GetWholeNumber(ClearAvailableHeight - lastLevelAdditionHeight);

					//
					if (m_Length_X % StepLength_X != 0)
					{
						strError += "Height should be divisible by ";
						strError += StepLength_Z.ToString();
						strError += " without remainder.";
					}

					iNewValue = Utils.GetWholeNumberByStep(iNewValue, StepLength_Z);
					iNewValue = Utils.CheckWholeNumber(iNewValue, MinLength_Z, MaxLength_Z);

					//
					Length_Z = iNewValue;

					return _On_Height_Changed(true, out strError);
				}
				catch { }

				return true;
			}
			else if (PROP_TOP_LEFT_POINT_X == strPropSysName || PROP_TOP_LEFT_POINT_Y == strPropSysName)
			{
				try
				{
					// try to move all racks row\column
					Point newTopLeftPoint = this.TopLeft_GlobalPoint;
					if (PROP_TOP_LEFT_POINT_X == strPropSysName)
						newTopLeftPoint.X = Convert.ToDouble(propValue);
					else if (PROP_TOP_LEFT_POINT_Y == strPropSysName)
						newTopLeftPoint.Y = Convert.ToDouble(propValue);

					// make preview
					List<Rack> group = m_Sheet.GetRackGroup(this);
					List<Rack> _previewGroup = new List<Rack>();
					//
					Rack _previewThisRack = this.Clone() as Rack;
					if(_previewThisRack == null)
					{
						strError = "Error. Cant clone rack.";
						return false;
					}
					_previewThisRack.TopLeft_GlobalPoint = newTopLeftPoint;
					_previewGroup.Add(_previewThisRack);
					//
					int _thisRackIndex = group.IndexOf(this);
					Rack _previosRack = _previewThisRack;
					for (int i = _thisRackIndex - 1; i >= 0; --i)
					{
						Rack _previewRack = group[i].Clone() as Rack;
						if (_previewRack == null)
						{
							strError = "Error. Cant clone rack.";
							return false;
						}
						//
						Point _TopLeft_GlobalPoint = _previosRack.TopLeft_GlobalPoint;
						if (this.IsHorizontal)
							_TopLeft_GlobalPoint.X -= _previewRack.Length_X + Rack.sHorizontalRow_GlobalGap;
						else
							_TopLeft_GlobalPoint.Y -= _previewRack.Length_Y + Rack.sVerticalColumn_GlobalGap;
						_previewRack.TopLeft_GlobalPoint = _TopLeft_GlobalPoint;

						_previewGroup.Insert(0, _previewRack);

						_previosRack = _previewRack;
					}
					//
					_previosRack = _previewThisRack;
					for (int i = _thisRackIndex + 1; i < group.Count; ++i)
					{
						Rack _previewRack = group[i].Clone() as Rack;
						if (_previewRack == null)
						{
							strError = "Error. Cant clone rack.";
							return false;
						}
						//
						Point _TopLeft_GlobalPoint = _previosRack.TopLeft_GlobalPoint;
						if (this.IsHorizontal)
						{
							_TopLeft_GlobalPoint = _previosRack.TopRight_GlobalPoint;
							_TopLeft_GlobalPoint.X += Rack.sHorizontalRow_GlobalGap;
						}
						else
						{
							_TopLeft_GlobalPoint = _previosRack.BottomLeft_GlobalPoint;
							_TopLeft_GlobalPoint.Y += Rack.sVerticalColumn_GlobalGap;
						}
						_previewRack.TopLeft_GlobalPoint = _TopLeft_GlobalPoint;

						_previewGroup.Add(_previewRack);

						_previosRack = _previewRack;
					}

					// check layout
					Rack _first = _previewGroup[0];
					Rack _last = _previewGroup[_previewGroup.Count - 1];

					// check layout
					// make big rectangle
					Rack temporaryRack = _first.Clone() as Rack;
					if (temporaryRack == null)
					{
						strError = "Error. Cant clone rack.";
						return false;
					}
					if (temporaryRack.IsHorizontal)
						temporaryRack.Length_X = _last.TopRight_GlobalPoint.X - _first.TopLeft_GlobalPoint.X;
					else
						temporaryRack.Length_Y = _last.BottomLeft_GlobalPoint.Y - _first.TopLeft_GlobalPoint.Y;

					// check borders
					if (temporaryRack.Length_X > m_Sheet.Length || temporaryRack.Length_Y > m_Sheet.Width)
						return false;
					if (temporaryRack.TopLeft_GlobalPoint.X < 0)
						temporaryRack.TopLeft_GlobalPoint = new Point(0, _first.TopLeft_GlobalPoint.Y);
					if (temporaryRack.TopLeft_GlobalPoint.Y < 0)
						temporaryRack.TopLeft_GlobalPoint = new Point(_first.TopLeft_GlobalPoint.X, 0);
					if (temporaryRack.BottomRight_GlobalPoint.X > m_Sheet.Length)
					{
						Point newTopLeftGlobalPoint = temporaryRack.TopLeft_GlobalPoint;
						newTopLeftGlobalPoint.X -= temporaryRack.BottomRight_GlobalPoint.X - m_Sheet.Length;
						temporaryRack.TopLeft_GlobalPoint = newTopLeftGlobalPoint;
					}
					if (temporaryRack.BottomRight_GlobalPoint.Y > m_Sheet.Width)
					{
						Point newTopLeftGlobalPoint = temporaryRack.TopLeft_GlobalPoint;
						newTopLeftGlobalPoint.Y -= temporaryRack.BottomRight_GlobalPoint.Y - m_Sheet.Width;
						temporaryRack.TopLeft_GlobalPoint = newTopLeftGlobalPoint;
					}

					//
					List<BaseRectangleGeometry> overlappedRectangles;
					List<BaseRectangleGeometry> rectanglesToCheck = new List<BaseRectangleGeometry>() { temporaryRack };
					List<BaseRectangleGeometry> rectanglesToIgnore = new List<BaseRectangleGeometry>();
					rectanglesToIgnore.AddRange(group);
					if (!bCheckLayout || m_Sheet.IsLayoutCorrect(rectanglesToCheck, rectanglesToIgnore, out overlappedRectangles))
					{
						// Place this rack to the end of racks change order list.
						m_bDontChangeOrder = true;

						group[0].TopLeft_GlobalPoint = temporaryRack.TopLeft_GlobalPoint;
						for (int i = 1; i < group.Count; ++i)
						{
							Rack prevRack = group[i - 1];
							Rack currentRack = group[i];

							if (prevRack != null && currentRack != null)
							{
								Point curRackTopLeft_GlobalPoint = new Point();
								if (currentRack.IsHorizontal)
								{
									curRackTopLeft_GlobalPoint = prevRack.TopRight_GlobalPoint;
									curRackTopLeft_GlobalPoint.X += Rack.sHorizontalRow_GlobalGap;
								}
								else
								{
									curRackTopLeft_GlobalPoint = prevRack.BottomLeft_GlobalPoint;
									curRackTopLeft_GlobalPoint.Y += Rack.sVerticalColumn_GlobalGap;
								}

								currentRack.TopLeft_GlobalPoint = curRackTopLeft_GlobalPoint;
							}
						}

						//
						m_bDontChangeOrder = false;
						_MarkStateChanged();

						return true;
					}
				}
				catch { }

				return false;
			}
			else if(PROP_DISABLE_CHANGE_SIZE_GRIPPOINTS == strPropSysName)
			{
				try
				{
					m_bDisableChangeSizeGripPoints = Convert.ToBoolean(propValue);
					return true;
				}
				catch { }

				return false;
			}
			else if (PROP_CLEAR_AVAILABLE_HEIGHT == strPropSysName)
			{
				// Clear available height is calculated based on the sheet's roof type and position of this rack.
				return false;
			}
			else if (PROP_IS_MATERIAL_ON_GROUND == strPropSysName)
			{
				try
				{
					m_bIsMaterialOnGround = Convert.ToBoolean(propValue);
					// Only one of IsMaterialOnGround and IsUnderpassAvailable can be checked.
					if (m_bIsMaterialOnGround && m_IsUnderpassAvailable)
						m_IsUnderpassAvailable = false;

					//
					if (!_CheckMaterialOnGroundLevel(out strError))
						return false;

					// X Bracing height depends on the m_bIsMaterialOnGround property.
					string strColumnCalcError;
					if(!RecalculateColumn(false, out strColumnCalcError))
					{
						strError = strColumnCalcError;
						return false;
					}

					//
					string strRackHeightError;
					if (!_RecalcRackHeight(out strRackHeightError))
					{
						strError = strRackHeightError;
						return false;
					}

					return true;
				}
				catch { }

				return false;
			}
			else if (PROP_IS_UNDERPASS_AVAILABLE == strPropSysName)
			{
				try
				{
					//
					bool _oldMatOnGround = m_bIsMaterialOnGround;
					bool _oldUnderpassAvail = m_IsUnderpassAvailable;

					m_IsUnderpassAvailable = Convert.ToBoolean(propValue);
					// Only one of IsMaterialOnGround and IsUnderpassAvailable can be checked.
					if (m_IsUnderpassAvailable && m_bIsMaterialOnGround)
						m_bIsMaterialOnGround = false;

					//
					int levelsOffset = 0;
					if (!_oldMatOnGround && !_oldUnderpassAvail)
					{
						double firstLevelOffset = Rack.sFirstLevelOffset;
						if (m_Levels != null)
						{
							RackLevel firstLevel = m_Levels.FirstOrDefault(level => level != null && level.Index == 1);
							if (firstLevel != null && firstLevel.Beam != null)
								firstLevelOffset -= firstLevel.Beam.Height;
						}
						levelsOffset = Utils.GetWholeNumber(firstLevelOffset);
					}
					else if (_oldMatOnGround)
					{
						//levelsOffset = (int)m_MaterialHeightOnGround + (int)Rack.sDistanceBetweenPalletAndLevel;
						levelsOffset = 0;
					}
					if (!_On_UnderpassAvailable_Changed(levelsOffset, out strError))
						return false;

					// X Bracing height depends on the Underpass value.
					string strColumnCalcError;
					if (!RecalculateColumn(false, out strColumnCalcError))
					{
						strError = strColumnCalcError;
						return false;
					}

					//
					string strRackHeightError;
					if (!_RecalcRackHeight(out strRackHeightError))
					{
						strError = strRackHeightError;
						return false;
					}

					return true;
				}
				catch { }

				return false;
			}
			else if (PROP_UNDERPASS == strPropSysName)
			{
				try
				{
					m_Underpass = Convert.ToUInt32(propValue);

					return IsUnderpassCorrect(out strError, false);
				}
				catch { }

				return false;
			}
			else if (PROP_SHOW_PALLET == strPropSysName)
			{
				try
				{
					bool bRes = _TryToChange_ShowPallet(Convert.ToBoolean(propValue), out strError);

					if(bRes)
					{
						//
						eAppliedChanges appliedChanges = eAppliedChanges.eNothing;
						List<BaseRectangleGeometry> overlappedRectangles;
						if (bCheckLayout && !IsCorrect(appliedChanges, out overlappedRectangles))
						{
							// try to fix it
							Point newTopLeftPoint;
							double newGlobalLength;
							double newGlobalWidth;
							//
							// infinity loop protection
							int iMaxLoopCount = 100;
							int iLoopCount = 0;
							int iGripIndex = GRIP_CENTER;
							//
							while (this._CalculateNotOverlapPosition(overlappedRectangles, iGripIndex, m_Sheet.Length, m_Sheet.Width, out newTopLeftPoint, out newGlobalLength, out newGlobalWidth))
							{
								m_TopLeft_GlobalPoint = newTopLeftPoint;

								//
								if (IsCorrect(appliedChanges, out overlappedRectangles))
									break;

								++iLoopCount;
								if (iLoopCount >= iMaxLoopCount)
									break;
							}
						}

						bool bLayoutIsCorrect = IsInsideArea(m_Sheet.Length, m_Sheet.Width, true);
						if (bLayoutIsCorrect && !IsCorrect(appliedChanges, out overlappedRectangles))
							bLayoutIsCorrect = false;

						bRes = bLayoutIsCorrect;
						if (!bRes)
							strError = "Result layout is not correct.";
					}

					return bRes;
				}
				catch { }

				return false;
			}
			else if (PROP_COLUMN == strPropSysName)
			{
				RackColumn column = null;
				if (propValue is Guid)
					column = _GetColumnByGUID((Guid)propValue);
				else if (propValue is RackColumn)
					column = (RackColumn)propValue;

				if (column != null)
				{
					m_IsColumnSetManually = true;
					return Set_Column(column, false, true, true, out strError);
				}

				return false;
			}
			else if(PROP_MINIMUM_COLUMN == strPropSysName)
			{
				RackColumn column = null;
				if (propValue is Guid)
					column = _GetColumnByGUID((Guid)propValue);
				else if (propValue is RackColumn)
					column = (RackColumn)propValue;

				if (column != null)
					return Set_Column(column, true, true, true, out strError);

				return false;
			}
			else if (PROP_SPLIT_COLUMN == strPropSysName)
			{
				try
				{
					m_bSplitColumn = Convert.ToBoolean(propValue);
					return true;
				}
				catch { }

				return false;
			}
			else if (PROP_COLUMN_FIRST_PART_LENGTH == strPropSysName)
			{
				try
				{
					m_Column_FirstPartLength = Convert.ToUInt32(propValue);
					m_Column_SecondPartLength = (UInt32)Length_Z - m_Column_FirstPartLength;
					return true;
				}
				catch { }

				return false;
			}
			else if (PROP_COLUMN_SECOND_PART_LENGTH == strPropSysName)
			{
				try
				{
					m_Column_SecondPartLength = Convert.ToUInt32(propValue);
					m_Column_FirstPartLength = (UInt32)Length_Z - m_Column_SecondPartLength;
					return true;
				}
				catch { }

				return false;
			}
			//else if (PROP_BRACING == strPropSysName)
			//{
			//
			//}
			else if (PROP_BRACING_TYPE == strPropSysName)
			{
				try
				{
					if (m_Sheet == null || m_Sheet.Document == null)
						return false;

					if (propValue is eBracingType)
					{
						m_Sheet.Document.Rack_BracingType = (eBracingType)propValue;
						return true;
					}
					else if (propValue is string)
					{
						string _strVal = propValue as string;
						if (BRACING_TYPE_GI == _strVal)
						{
							m_Sheet.Document.Rack_BracingType = eBracingType.eGI;
							return true;
						}
						else if (BRACING_TYPE_POWDER_COATED == _strVal)
						{
							m_Sheet.Document.Rack_BracingType = eBracingType.ePowderCoated;
							return true;
						}
					}
				}
				catch { }

				return false;
			}
			else if (PROP_ACCESSORIES == strPropSysName)
			{
				if (m_Sheet == null || m_Sheet.Document == null)
					return false;

				RackAccessories _acc = propValue as RackAccessories;
				if (_acc != null)
				{
					m_Sheet.Document.Rack_Accessories = _acc;
					return true;
				}

				return false;
			}
			else if (PROP_NUMBER_OF_LEVELS_EXCEPT_GROUNDLEVEL == strPropSysName)
			{
				try
				{
					UInt32 numberOfLevels = Convert.ToUInt32(propValue);
					return _On_NumberOfLevels_Changed(numberOfLevels, true, out strError);
				}
				catch { }

				return false;
			}
			else if (PROP_ARE_LEVELS_THE_SAME == strPropSysName)
			{
				try
				{
					m_bAreLevelsTheSame = Convert.ToBoolean(propValue);
					return true;
				}
				catch { }

				return false;
			}
			else if (!string.IsNullOrEmpty(strPropSysName) && strPropSysName.StartsWith(PROP_RACK_LEVEL))
			{
				string strLevelPropName;
				UInt32 _levelIndex;
				if (RackLevel._ParseLevelProp_SystemName(strPropSysName, out _levelIndex, out strLevelPropName))
				{
					try
					{
						if (_levelIndex >= 0 && _levelIndex <= m_Levels.Count)
						{
							int iLevelsOffset = 1;
							RackLevel _grLevel = m_Levels.FirstOrDefault(l => l != null && l.Index == 0);
							if (_grLevel != null)
								iLevelsOffset = 0;

							List<RackLevel> _levelsForChange = new List<RackLevel>();
							if (m_bAreLevelsTheSame)
							{
								// Dont allow user to change height of the last level.
								// It is calculated automatically to make rack height without pallets be multiple of RACK_HEIGHT_MULTIPLICITY.
								if (PROP_LEVEL_HEIGHT == strLevelPropName && _levelIndex == NumberOfLevels_WithoutGround)
								{
									//_levelsForChange.Add(m_Levels[(int)_levelIndex - iLevelsOffset]);
									return false;
								}
								else
								{
									_levelsForChange.AddRange(m_Levels);
									// exclude the last level when user changes all levels height
									if(PROP_LEVEL_HEIGHT == strLevelPropName)
									{
										RackLevel lastLevel = m_Levels.LastOrDefault();
										if (lastLevel != null)
											_levelsForChange.Remove(lastLevel);
									}
								}
							}
							else
								_levelsForChange.Add(m_Levels[(int)_levelIndex - iLevelsOffset]);

							RackLevel _lastLevel = m_Levels.LastOrDefault();

							foreach (RackLevel _level in _levelsForChange)
							{
								if (_level == null)
									continue;

								if(PROP_PALLETS_ARE_EQUAL == strLevelPropName)
								{
									_level.Set_PalletsAreEqual(Convert.ToBoolean(propValue));
									// Recalc pallets length, width, height based on the rack dimensions.
									// Recalc pallets only if PalletsAreEqual is setted to true and pallets have different properties.
									if (_level.PalletsAreEqual && _level.PalletsHaveDifferentProperties)
									{
										_RecalcPalletLength(_level);
										_RecalcPalletWidth(_level);
										if (!_RecalcLevelHeight(out strError))
											return false;
									}
								}
								else if (PROP_NUMBER_OF_PALLETS == strLevelPropName)
								{
									UInt32 levelLoad = _level.LevelLoad;
									_level.Set_NumberOfPallets(Convert.ToUInt32(propValue));
									// Dont recalc pallets properties if NumberOfPallets ws changed manually.
									if (!bWasChangedViaProperties)
									{
										_level.Set_LevelLoad(levelLoad);
										
										_RecalcPalletLength();
										_RecalcPalletWidth();
										if (!_RecalcLevelHeight(out strError))
											return false;
									}
								}
								else if(strLevelPropName.StartsWith(PROP_PALLET_CONFIGURATION))
								{
									PalletConfiguration pc = propValue as PalletConfiguration;

									int _palletIndex = -1;
									string strPalletIndex = strLevelPropName.Replace(PROP_PALLET_CONFIGURATION, string.Empty);
									if (strPalletIndex.Length > 0)
									{
										try
										{
											_palletIndex = Convert.ToInt32(strPalletIndex);
										}
										catch { }
									}

									if (_level.PalletsAreEqual)
									{
										for (int _index = 0; _index < _level.Pallets.Count; ++_index)
										{
											if (!_level.Set_PalletConfiguration((UInt32)_index, pc))
												return false;
										}
									}
									else
									{
										if (_palletIndex < 0)
											_palletIndex = 0;

										if (!_level.Set_PalletConfiguration((UInt32)_palletIndex, pc))
											return false;
									}
								}
								else if (strLevelPropName.StartsWith(PROP_PALLET_LENGTH))
								{
									UInt32 _lengthVal = Convert.ToUInt32(propValue);

									if (_level.Pallets == null)
										return false;

									int _palletIndex = -1;
									string strPalletIndex = strLevelPropName.Replace(PROP_PALLET_LENGTH, string.Empty);
									if (strPalletIndex.Length > 0)
									{
										try
										{
											_palletIndex = Convert.ToInt32(strPalletIndex);
										}
										catch { }
									}

									if (_level.PalletsAreEqual)
									{
										for (int _index = 0; _index < _level.Pallets.Count; ++_index)
											_level.Set_PalletLength((UInt32)_index, _lengthVal);
									}
									else
									{
										if (_palletIndex < 0)
											_palletIndex = 0;

										_level.Set_PalletLength((UInt32)_palletIndex, _lengthVal);
									}
								}
								else if (strLevelPropName.StartsWith(PROP_PALLET_HEIGHT))
								{
									UInt32 _newPalleHeight = Convert.ToUInt32(propValue);

									// check pallet height value
									if (_newPalleHeight > Pallet.MAX_HEIGHT)
									{
										strError += "Pallet height(";
										strError += _newPalleHeight.ToString();
										strError += ") is bigger than max pallet height value(";
										strError += Pallet.MAX_HEIGHT.ToString();
										strError += ").";

										return false;
									}

									if (_level.Pallets == null)
										return false;

									int _palletIndex = -1;
									string strPalletIndex = strLevelPropName.Replace(PROP_PALLET_HEIGHT, string.Empty);
									if (strPalletIndex.Length > 0)
									{
										try
										{
											_palletIndex = Convert.ToInt32(strPalletIndex);
										}
										catch { }
									}

									if (_level.PalletsAreEqual)
									{
										for (int _index = 0; _index < _level.Pallets.Count; ++_index)
										{
											_level.Set_PalletHeight((UInt32)_index, _newPalleHeight);
										}
									}
									else
									{
										if (_palletIndex < 0)
											_palletIndex = 0;

										_level.Set_PalletHeight((UInt32)_palletIndex, _newPalleHeight);
									}

									if (!_level.RecalcLevelHeight(out strError))
										return false;
								}
								else if (strLevelPropName.StartsWith(PROP_PALLET_LOAD))
								{
									UInt32 _loadVal = Convert.ToUInt32(propValue);

									if (_level.Pallets == null)
										return false;

									int _palletIndex = -1;
									string strPalletIndex = strLevelPropName.Replace(PROP_PALLET_LOAD, string.Empty);
									if (strPalletIndex.Length > 0)
									{
										try
										{
											_palletIndex = Convert.ToInt32(strPalletIndex);
										}
										catch { }
									}

									if (_level.PalletsAreEqual)
									{
										for (int _index = 0; _index < _level.Pallets.Count; ++_index)
											_level.Set_PalletLoad((UInt32)_index, _loadVal);
									}
									else
									{
										if (_palletIndex < 0)
											_palletIndex = 0;

										_level.Set_PalletLoad((UInt32)_palletIndex, _loadVal);
									}
								}
								else if (strLevelPropName.StartsWith(PROP_PALLET_DEPTH))
								{
									UInt32 _depthVal = Convert.ToUInt32(propValue);

									if (_level.Pallets == null)
										return false;

									int _palletIndex = -1;
									string strPalletIndex = strLevelPropName.Replace(PROP_PALLET_DEPTH, string.Empty);
									if (strPalletIndex.Length > 0)
									{
										try
										{
											_palletIndex = Convert.ToInt32(strPalletIndex);
										}
										catch { }
									}

									if (_level.PalletsAreEqual)
									{
										for (int _index = 0; _index < _level.Pallets.Count; ++_index)
											_level.Set_PalletWidth((UInt32)_index, _depthVal);
									}
									else
									{
										if (_palletIndex < 0)
											_palletIndex = 0;

										_level.Set_PalletWidth((UInt32)_palletIndex, _depthVal);
									}
								}
								else if (PROP_LEVEL_HEIGHT == strLevelPropName)
								{
									// Level height = PALLET_RISER_HEIGHT + pallet height + clearance distance.
									UInt32 newLevelHeight = Convert.ToUInt32(propValue);
									// But pallet height + clearance + next level beam height should satisfy rules.
									// Check rules in _RecalcLevelHeight() function.
									UInt32 clearance = (UInt32)Rack.sDistanceBetweenPalletAndLevel;
									// if it is not the last level
									if (_level != m_Levels.LastOrDefault())
									{
										//
										int iNextLevelBeamHeight = 0;
										int iNextLevelIndex = m_Levels.IndexOf(_level) + 1;
										if (iNextLevelIndex < m_Levels.Count)
										{
											RackLevel nextLevel = m_Levels[iNextLevelIndex];
											if (nextLevel != null && nextLevel.Beam != null)
												iNextLevelBeamHeight = Utils.GetWholeNumber(nextLevel.Beam.Height);
										}
										// Check rules in _RecalcLevelHeight() function.
										int iMultiplicityPartsCount = (int)Math.Ceiling((double)(newLevelHeight + iNextLevelBeamHeight) / Rack.LEVEL_HEIGHT_MULTIPLICITY);
										UInt32 levelHeightWithRules = (UInt32)(iMultiplicityPartsCount * LEVEL_HEIGHT_MULTIPLICITY);
										UInt32 delta = levelHeightWithRules - newLevelHeight;
										//
										newLevelHeight += delta;
										clearance += delta;
										//
										if (_level.Index == 0)
											newLevelHeight += 12;
									}

									// check level height value
									if (newLevelHeight < _level.MinLevelHeight)
									{
										strError += "Calculated level height(";
										strError += newLevelHeight.ToString();
										strError += ") is less than min level height value(";
										strError += _level.MinLevelHeight.ToString();
										strError += ").";

										return false;
									}
									else if (newLevelHeight > _level.MaxLevelHeight)
									{
										strError += "Calculated level height(";
										strError += newLevelHeight.ToString();
										strError += ") is bigger than max level height value(";
										strError += _level.MaxLevelHeight.ToString();
										strError += ").";

										return false;
									}

									// Level height = PALLET_RISER_HEIGHT + pallet height + clearance(min value is 100).
									// So recalculate pallet height from the level height.
									UInt32 newPalletHeight = newLevelHeight - clearance;
									// If LevelAccessories.ForkEntryBar is checked then add PALLET_RISER_HEIGHT between level and pallets.
									if (_level.Accessories != null && _level.Accessories.ForkEntryBar)
									{
										if (newPalletHeight <= PALLET_RISER_HEIGHT)
											return false;

										newPalletHeight -= (UInt32)PALLET_RISER_HEIGHT;
									}
									//
									if (newPalletHeight > Pallet.MAX_HEIGHT)
									{
										strError += "Calculated pallet height(";
										strError += newPalletHeight.ToString();
										strError += ") is bigger than max pallet height value(";
										strError += Pallet.MAX_HEIGHT.ToString();
										strError += ").";

										return false;
									}

									// set level height
									_level.Set_LevelHeight(newLevelHeight);
									// set pallet height
									if (_level != _lastLevel && _level.Pallets != null)
									{
										foreach (Pallet _pallet in _level.Pallets)
										{
											if (_pallet == null)
												continue;

											_pallet.Set_Height(newPalletHeight);
										}
									}
								}
								else if (PROP_LEVEL_LOAD == strLevelPropName)
								{
									_level.Set_LevelLoad(Convert.ToUInt32(propValue));
								}
								else if (PROP_LEVEL_ACCESSORIES == strLevelPropName)
								{
									_level.Set_Accessories(propValue as RackLevelAccessories);
									// Update level height because it depends on the LevelAccessories.PalletRiser.
									bool bRes = _level.RecalcLevelHeight(out strError);
									if (!bRes)
										return false;
								}
							}

							// Need to recalc last level height before compare rack height with limits.
							// Rack height without pallets should be multiple of RACK_HEIGHT_MULTIPLICITY.
							_CheckLastLevelHeight();

							// If load was changed then recalc beam size before all.
							bool bRecalcBeam = true;
							string strCalcBeamError;
							if (PROP_LEVEL_LOAD == strLevelPropName || strLevelPropName.StartsWith(PROP_PALLET_LOAD) || strLevelPropName.StartsWith(PROP_PALLET_CONFIGURATION))
								bRecalcBeam = this._RecalcBeamSize(false, out strCalcBeamError);

							string strRackLengthError;
							bool _bRecalcLength = _RecalcRackLength(true, true, out strRackLengthError);

							// recalc rack height is called inside _RecalcRackLength
							//bool _bRecalcHeight = _RecalcRackHeight();

							string strRackWidthError;
							bool _bRecalcWidth = _RecalcRackWidth(out strRackWidthError);

							if (/*!_bRecalcHeight ||*/ !_bRecalcLength || !_bRecalcWidth)
							{
								strError = strRackLengthError;
								if(!string.IsNullOrEmpty(strError))
									strError += " ";
								if (!string.IsNullOrEmpty(strRackWidthError))
									strError += strRackWidthError;

								return false;
							}

							return true;
						}
					}
					catch { }
				}

				return false;
			}

			//
			return base.SetPropertyValue(strPropSysName, propValue, bWasChangedViaProperties, false, false, out strError, bCheckLayout);
		}

		//=============================================================================
		private bool _TryToChange_ShowPallet(bool newShowPalletVal, out string strError)
		{
			m_bShowPallet = newShowPalletVal;

			// Rack height with pallets depends on the 
			CheckRackHeight();

			return _RecalcBeamSize(true, out strError);
		}

		//=============================================================================
		public bool _RecalcRackHeight(out string strMessage)
		{
			strMessage = string.Empty;

			// Before rack height calcultaion need to check rack height.
			// It should be multiple of RACK_HEIGHT_MULTIPLICITY.
			_CheckLastLevelHeight();

			// Need to calculate 2 heights: with pallets and without pallets.
			// Otherwise total rack height will compare incorrect with available height(based on the roof
			// properties and this rack position).
			UInt32 _height = _CalcRackHeight(false);
			UInt32 _heightWithPallets = _CalcRackHeight(true);

			if (Utils.FLT(_height, MinLength_Z))
			{
				strMessage += "Calculated rack height(";
				strMessage += _height.ToString();
				strMessage += ") is less than min height value(";
				strMessage += MinLength_Z.ToString();
				strMessage += ").";

				return false;
			}

			// Compare height with pallets with ClearAvailableHeight because MaxLengthZ can be max loading height + last level height and
			// be less than ClearAvailableHeight.
			if (Utils.FGT(_height, MaxLength_Z) || Utils.FGT(_heightWithPallets, this.ClearAvailableHeight))
			{
				strMessage += "Calculated rack height ";
				if (Utils.FGT(_heightWithPallets, this.ClearAvailableHeight))
				{
					strMessage += " with pallets (";
					strMessage += _heightWithPallets.ToString();
				}
				else
				{
					strMessage += "(";
					strMessage += _height.ToString();
				}
				strMessage += ") is bigger than max height value(";
				strMessage += MaxLength_Z.ToString();
				strMessage += ").";

				return false;
			}

			Length_Z = (int)_height;
			return _On_Height_Changed(false, out strMessage);
		}
		private UInt32 _CalcRackHeight(bool bWithPallets)
		{
			UInt32 height = 0;

			if (this.m_IsUnderpassAvailable)
				height = this.m_Underpass;
			else if (this.m_bIsMaterialOnGround)
			{
				//_height = this.m_MaterialHeightOnGround;
				//_height += sDistanceBetweenPalletAndLevel;
				height = 0;
			}

			//
			RackLevel lastLevel = m_Levels.LastOrDefault();
			//
			foreach (RackLevel level in m_Levels)
			{
				if (level == null)
					continue;

				if (!this.IsUnderpassAvailable && !this.IsMaterialOnGround && level.Index == 1)
					height += Rack.sFirstLevelOffset;
				// Level0 is based on the ground
				else if (level.Index != 0 && level.Beam != null)
					height += (UInt32)Utils.GetWholeNumber(level.Beam.Height);

				if (bWithPallets && lastLevel == level)
				{
					height += level.TheBiggestPalletHeightWithRiser;
				}
				else
					height += level.LevelHeight;
			}

			return height;
		}
		// Read rules int the _RecalcLevelHeight().
		// Level to level height should be a multiple of LEVEL_HEIGHT_MULTIPLICITY.
		public static int LEVEL_HEIGHT_MULTIPLICITY = 50;
		// Additional distance under each pallet.
		// Add this distance between level and pallets if LevelAccessories.PalletRiser is checked.
		public static double PALLET_RISER_HEIGHT = 100.0;
		// Last level min and max height. Total rack height without pallets should be multiple of 100.
		public static int LAST_LEVEL_MIN_HEIGHT = 250;
		public static int LAST_LEVEL_MAX_HEIGHT = 350;
		// Minimum level height value. For all levels except the last level.
		public static int MIN_LEVEL_HEIGHT = 400;
		// Total rack height without pallets should be multiple of RACK_HEIGHT_MULTIPLICITY.
		public static int RACK_HEIGHT_MULTIPLICITY = 100;
		// The minimum gap between rack height(with pallets or not) and roof.
		public static int ROOF_HEIGHT_GAP = 200;
		// Loading height gap value. Max laoding height should be decreased by this value.
		// Loading height - the top of the topmost beam.
		public static double RACK_LOADING_HEIGHT_GAP = 200.0;
		// Underpass height should be greater or equal than (the smallest enabled MHE configuraition OverallHeightLowered + RACK_UNDERPASS_GAP).
		public static double RACK_UNDERPASS_GAP = 200.0;
		// Maximum value for USL distance.
		// USL distance - distance between top of the neighbor beams. Or from the first level beam to the ground.
		public static double MAX_USL_DISTANCE = 6000;
		private bool _RecalcLevelHeight(out string strMessage)
		{
			// Recalc level and pallet height from rack height.

			// Rules:
			// 1. If "Material on the ground is checked" then distance from the ground
			//    to the 1st level top of the beam should ends with 12 or 62.
			//    To satisfy this rule need to change ground level clearance(min value is 100).
			//    For example: the biggest pallet on the ground height(1000) + clearance(112) + 1st level beam height(100) = 1212.
			// 2. For all other levels distance from level to level should ends with 50 or 100.
			//    To satisfy this rule need to change clearance distance(min value is 100).
			//    For example: pallet height(100) + clearance(125) + beam height(75) = 1200.
			// 3. Last level height should be from LAST_LEVEL_MIN_HEIGHT to LAST_LEVEL_MAX_HEIGHT and total
			//    rack height without material should be multiple of RACK_HEIGHT_MULTIPLICITY.
			// 4. USL distance cant be greater than MAX_USL_DISTANCE.
			//    USL distance - distance between top of the neighbor beams. Or from the first level beam to the ground.

			// Need to calculate pallet and level height based on these rules.

			strMessage = string.Empty;

			if (m_Levels == null || m_Levels.Count < 2)
			{
				strMessage = "Levels count is less than 2.";
				return false;
			}

			RackLevel _lastLevel = m_Levels.LastOrDefault();
			if (_lastLevel == null)
			{
				strMessage = "Cant find the last level.";
				return false;
			}

			int _value = this.Length_Z;
			// compare with max available height
			if (Utils.FGT(_value, MaxLength_Z))
				_value = Utils.GetWholeNumber(MaxLength_Z);

			//
			if (this.m_bIsMaterialOnGround)
				_value -= 0;
			else if (this.m_IsUnderpassAvailable)
				_value -= (int)this.m_Underpass;
			else
				_value -= (int)Rack.sFirstLevelOffset;

			// The last level can have different height.
			// Just look at picture when edit advanced properties.
			//
			// Need to save last level height.
			int lastLevelHeight = (int)_lastLevel.LevelHeight;

			// If material on ground then remove 12 from height,
			// it changes the rule from ends with 12\62 to 0\50.
			if (this.IsMaterialOnGround)
				_value -= 12;
			else
			{
				// Remove first level beam height because
				// it should not satisfy rules above.
				// Rules drives the distance from top of the beam to top of another beam, but
				// they doesnt drive the distance from the ground to the top of first level beam.
				RackLevel firstLevel = m_Levels[0];
				if (firstLevel != null && firstLevel.Beam != null)
					_value -= Utils.GetWholeNumber(firstLevel.Beam.Height);
			}
			// Remove last level height from the calculation because it differs
			// from the other levels.
			if (this.ShowPallet && lastLevelHeight < _lastLevel.TheBiggestPalletHeightWithRiser)
			{
				// Place rack - set 6000 as roof height - infinitive loop exception.
				//
				// Rack height with pallets is greater than MaxLength_Z, so set MaxLength_Z as Rack.Length_Z 
				// and _RecalcLevelHeight() is called. But only last level height is removed from new Length_Z value.
				// Last level pallets height was not removed.
				_value -= (int)_lastLevel.TheBiggestPalletHeightWithRiser;
			}
			else
				_value -= lastLevelHeight;

			// Every level should be a multiple of 50,
			// so need to calc how many 50 "parts" we have
			int iTotalMultiplicityPartsCount = (int)Math.Floor((double)_value / LEVEL_HEIGHT_MULTIPLICITY);

			// Lets calculate how many 50 height parts each level should contain.
			// Need to divide iTotal50PartsCount by (number of levels - 1),
			// because the last level doesnt have any beam at the top, so dont satisfy 2nd rule for
			// the last level.
			int iMultiplicityPartsPerLevel = (int)Math.Floor((double)iTotalMultiplicityPartsCount / (m_Levels.Count - 1));

			// Calculate level height.
			// This value contains pallet height, clearance and beam height.
			double rLevelHeightWithBeam = iMultiplicityPartsPerLevel * LEVEL_HEIGHT_MULTIPLICITY;
			// Check that level height is not greater than MAX_USL_DISTANCE.
			if (Utils.FGT(rLevelHeightWithBeam, MAX_USL_DISTANCE))
				rLevelHeightWithBeam = MAX_USL_DISTANCE;
			// Remove the biggest beam height.
			// It is not possible to make all levels height(pallet height + clearance) equal
			// because they can have different beam height.
			// So lets make (beam height + pallet height + clearance) the same for all levels.
			// Different beam height we will remove by changing clearance distance.
			// Get the biggest beam height and add difference between it and current beam height to the clearance.
			double rBiggestBeamHeight = -1.0;
			foreach (RackLevel level in m_Levels)
			{
				if (level == null)
					continue;
				// dont calc grond level beam
				if (level.Index == 0)
					continue;
				if (level.Beam == null)
					continue;

				if (Utils.FGT(level.Beam.Height, 0.0) && (Utils.FLT(rBiggestBeamHeight, 0.0) || Utils.FGT(level.Beam.Height, rBiggestBeamHeight)))
					rBiggestBeamHeight = level.Beam.Height;
			}

			// level height = pallet riser + pallet height + clearance
			foreach (RackLevel level in m_Levels)
			{
				if (level == null)
					continue;

				double rBeamDelta = rBiggestBeamHeight;
				// get beam height from the next level
				int iCurrLevelIndex = m_Levels.IndexOf(level);
				if (iCurrLevelIndex < m_Levels.Count - 1)
				{
					int iNextLevelIndex = iCurrLevelIndex + 1;
					if (iNextLevelIndex < m_Levels.Count)
					{
						RackLevel nextLevel = m_Levels[iNextLevelIndex];
						if (nextLevel != null && nextLevel.Beam != null)
							rBeamDelta -= nextLevel.Beam.Height;
					}
				}
				else
					rBeamDelta = 0.0;
				double rClearance = Rack.sDistanceBetweenPalletAndLevel + rBeamDelta;
				double rLevelHeight = rLevelHeightWithBeam - rBiggestBeamHeight + rBeamDelta;
				// Add 12 to the ground level, read rules above.
				if(level.Index == 0)
				{
					rClearance += 12;
					rLevelHeight += 12;
				}
				double rPalletHeight = rLevelHeight - rClearance;
				// If LevelAccessories.ForkEntryBar is checked then add PALLET_RISER_HEIGHT between level and pallets.
				if (level.Accessories != null && level.Accessories.ForkEntryBar)
					rPalletHeight -= PALLET_RISER_HEIGHT;

				if (Utils.FLE(rPalletHeight, 0.0))
				{
					strMessage = "Calculated pallet height is less or equal 0.";
					return false;
				}

				if (Utils.FGT(rPalletHeight, Pallet.MAX_HEIGHT))
				{
					strMessage = "Calculated pallet height(";
					strMessage += rPalletHeight.ToString();
					strMessage += ") is bigger than max pallet height value(";
					strMessage += Pallet.MAX_HEIGHT.ToString();
					strMessage += ").";

					return false;
				}

				// Height of the last level can be changed only through the properties by user hands.
				// Last level height will have other value.
				if (level != _lastLevel)
				{
					if (Utils.FLT(rLevelHeight, MIN_LEVEL_HEIGHT))
					{
						strMessage = "Calculated level height (" + rLevelHeight.ToString() + ") is less than min level height value (" + MIN_LEVEL_HEIGHT.ToString() + ").";
						return false;
					}
					level.Set_LevelHeight((UInt32)Utils.GetWholeNumber(rLevelHeight));
				}

				if (level.Pallets != null)
				{
					foreach (Pallet pallet in level.Pallets)
					{
						if (pallet == null)
							continue;

						pallet.Set_Height((UInt32)rPalletHeight);
					}
				}

				_UpdatePalletsVisual(level);
			}

			// The last level can have different height.
			// Just look at picture when edit advanced properties.
			//
			// Last level height should be from LAST_LEVEL_MIN_HEIGHT to LAST_LEVEL_MAX_HEIGHT.
			// Total rack height without pallets should be multiple of RACK_HEIGHT_MULTIPLICITY.
			// Calculate last level height with rules above.
			_CheckLastLevelHeight();

			// always recalc rack height, because there can be values with points in level height
			CheckRackHeight();

			return true;
		}
		// Check last level height. It should be from LAST_LEVEL_MIN_HEIGHT to LAST_LEVEL_MAX_HEIGHT and
		// make total rack height without pallets be multiple of RACK_HEIGHT_MULTIPLICITY.
		private void _CheckLastLevelHeight()
		{
			// The last level can have different height.
			// Just look at picture when edit advanced properties.
			//
			// Last level height should be from LAST_LEVEL_MIN_HEIGHT to LAST_LEVEL_MAX_HEIGHT.
			// Total rack height without pallets should be multiple of RACK_HEIGHT_MULTIPLICITY.
			// Calculate last level height with rules above.
			RackLevel lastLevel = null;
			if (m_Levels != null)
				lastLevel = m_Levels.LastOrDefault();

			if (lastLevel != null)
			{
				double rackHeightWithoutPallets = _CalcRackHeight(false) - lastLevel.LevelHeight + LAST_LEVEL_MIN_HEIGHT;
				int iRackHeightMultiplicityPartsCount = (int)Math.Ceiling(rackHeightWithoutPallets / RACK_HEIGHT_MULTIPLICITY);
				double rLastLevelHeight = (iRackHeightMultiplicityPartsCount * RACK_HEIGHT_MULTIPLICITY - rackHeightWithoutPallets) + LAST_LEVEL_MIN_HEIGHT;
				int iLastLevelHeight = Utils.GetWholeNumber(rLastLevelHeight);
				lastLevel.Set_LevelHeight((UInt32)iLastLevelHeight);
			}
		}

		//=============================================================================
		public bool _RecalcRackLength(bool bRecalcBeam, bool bRecalcColumn, out string strMessage)
		{
			strMessage = string.Empty;

			// recalc column before calculating the length
			if (bRecalcColumn)
			{
				if (!this.RecalculateColumn(false, out strMessage))
					return false;
			}

			UInt32 _M_length = _Calc_M_RackLength();

			if (this.IsHorizontal)
			{
				if (_M_length < FirstRack_MinLength_X)
				{
					strMessage += "Calculated length of M-rack(";
					strMessage += _M_length.ToString();
					strMessage += ") is less then min length value(";
					strMessage += FirstRack_MinLength_X.ToString();
					strMessage += ").";

					return false;
				}

				if (_M_length > FirstRack_MaxLength_X)
				{
					strMessage += "Calculated length of M-rack(";
					strMessage += _M_length.ToString();
					strMessage += ") is bigger then max length value(";
					strMessage += FirstRack_MaxLength_X.ToString();
					strMessage += ").";

					return false;
				}
			}
			else
			{
				if (_M_length < FirstRack_MinLength_Y)
				{
					strMessage += "Calculated length of M-rack(";
					strMessage += _M_length.ToString();
					strMessage += ") is less then min length value(";
					strMessage += FirstRack_MinLength_Y.ToString();
					strMessage += ").";

					return false;
				}

				if (_M_length > FirstRack_MaxLength_Y)
				{
					strMessage += "Calculated length of M-rack(";
					strMessage += _M_length.ToString();
					strMessage += ") is bigger then max length value(";
					strMessage += FirstRack_MaxLength_Y.ToString();
					strMessage += ").";

					return false;
				}
			}

			//
			double _length = this.Length_X;
			double _width = this.Length_Y;
			// convert it to M and back to A
			if (this.IsHorizontal)
			{
				_length = _M_length;
				if (!this.IsFirstInRowColumn)
					_length -= this.DiffBetween_M_and_A;
			}
			else
			{
				_width = _M_length;
				if (!this.IsFirstInRowColumn)
					_width -= this.DiffBetween_M_and_A;
			}

			// just set new length and width if rack is not init
			if (!this.IsInit)
			{
				this.Length_X = _length;
				this.Length_Y = _width;

				strMessage = "Rack length is recalculated. ";

				if (bRecalcBeam)
				{
					string strBeamCalcMessage;
					bool bBeamCalcResult = this._RecalcBeamSize(true, out strBeamCalcMessage);

					if (!string.IsNullOrEmpty(strBeamCalcMessage))
						strMessage += strBeamCalcMessage;

					return bBeamCalcResult;
				}

				return true;
			}

			//
			bool bRes = RackUtils.ChangeRack(this, _length, _width, true, out strMessage);
			if (bRes && bRecalcBeam)
			{
				strMessage = "Rack length is recalculated. ";

				string strBeamCalcMessage;
				bool bBeamCalcResult = this._RecalcBeamSize(true, out strBeamCalcMessage);

				if (!string.IsNullOrEmpty(strBeamCalcMessage))
					strMessage += strBeamCalcMessage;

				return bBeamCalcResult;
			}

			return bRes;
		}
		// Read rules int the _Calc_M_RackLength().
		// Level length should be a multiple of LEVEL_LENGTH_MULTIPLICITY.
		public static int LEVEL_LENGTH_MULTIPLICITY = 100;
		// Additional gap between beam and columns.
		public static int INNER_LENGTH_ADDITIONAL_GAP = 10;
		private UInt32 _Calc_M_RackLength()
		{
			UInt32 length = 0;

			// Rack length rules:
			// Rack length = column length + (inner rack length) + column length.
			// Inner rack length = (pallet length + clearance between pallets) + INNER_LENGTH_ADDITIONAL_GAP.
			// Inner rack length(without INNER_LENGTH_ADDITIONAL_GAP) should be in multiple of LEVEL_LENGTH_MULTIPLICITY.
			//
			// Clearance between pallets should be from 75 to 100, but it is for the biggest level length.
			// It means that if we have 2 levels with pallets:
			// level 1 : pallet(300), pallet(300)
			// level 2 : pallet(1000), pallet(1000)
			// Then Clearance min, max values should be applied for the "level 2",
			// "level 1" pallets clearance will be greater than 100.
			//
			// Pallets clearance rules:
			// 1. When two pallets are stored per level then clearance is 100.
			// 2. When pallet length is less than 1000, then clearance is adjusted from 75 to 100.
			// 3. If there is only one pallet at level then clearance value is from 100 to 150.
			//
			// Beam length is rack inner length without INNER_LENGTH_ADDITIONAL_GAP.

			//
			length += 2 * this.DiffBetween_M_and_A;

			// Find level with the biggest sum of pallets length.
			RackLevel foundLevel = null;
			UInt32 maxSumPalletLength = 0;
			if (m_Levels != null)
			{
				foreach (RackLevel level in m_Levels)
				{
					if (level == null)
						continue;

					UInt32 sumPalletLength = 0;
					if (level.Pallets != null && level.Pallets.Count > 0)
					{
						foreach (Pallet pallet in level.Pallets)
						{
							if (pallet == null)
								continue;

							sumPalletLength += pallet.Length;
						}
					}

					if(sumPalletLength > maxSumPalletLength)
					{
						maxSumPalletLength = sumPalletLength;
						foundLevel = level;
					}
				}
			}

			// Calculate level length with rules above.
			if(foundLevel != null)
			{
				double minClearance = _CalculateMinPalletsClearance(foundLevel);

				double levelInnerLength = maxSumPalletLength + (foundLevel.Pallets.Count + 1) * minClearance;
				int iLengthMultiplicityPartsCount = (int)Math.Ceiling(levelInnerLength / LEVEL_LENGTH_MULTIPLICITY);
				length += (UInt32)(iLengthMultiplicityPartsCount * LEVEL_LENGTH_MULTIPLICITY);
				// add INNER_LENGTH_ADDITIONAL_GAP
				length += (UInt32)INNER_LENGTH_ADDITIONAL_GAP;
			}

			return length;
		}
		/// <summary>
		/// Returns min clearance distance between the pallets at level.
		/// </summary>
		private double _CalculateMinPalletsClearance(RackLevel level)
		{
			// Check pallet clearance distance rules in _Calc_M_RackLength().
			if (level == null || level.Pallets == null)
				return 0.0;

			foreach (Pallet pallet in level.Pallets)
			{
				if (pallet == null)
					continue;

				if (pallet.Length < 1000)
					return 75.0;
			}

			return 100.0;
		}
		private bool _RecalcPalletLength()
		{
			return _RecalcPalletLength(null);
		}
		private bool _RecalcPalletLength(RackLevel levelToChange)
		{
			// Recalc pallet length from rack length like pallets has the same length.
			// Check pallet and level length rules in _Calc_M_RackLength().

			if (m_Levels == null || m_Levels.Count < 2)
				return false;

			double beamLength = this.BeamLength;

			foreach (RackLevel level in m_Levels)
			{
				if (level == null || level.Pallets == null || level.Pallets.Count == 0)
					continue;

				if (levelToChange != null && level != levelToChange)
					continue;

				double minClearance = _CalculateMinPalletsClearance(level);
				double palletsLengthSum = beamLength - (level.NumberOfPallets + 1) * minClearance;
				double palletLength = Math.Floor(palletsLengthSum / level.NumberOfPallets);

				foreach (Pallet _pallet in level.Pallets)
				{
					if (_pallet == null)
						continue;

					_pallet.Set_Length((UInt32)Utils.GetWholeNumber(palletLength));
				}

				_UpdatePalletsVisual(level);
			}

			return true;
		}
		//=============================================================================
		/// <summary>
		/// Try to find column when rack's length was changed.
		/// </summary>
		/// <param name="strMessage"></param>
		/// <returns></returns>
		private bool _RecalcColumnSize(out string strMessage)
		{
			strMessage = string.Empty;
			if (this.Sheet == null || this.Sheet.Document == null)
				return false;
			if (this.Sheet.Document.RacksColumnsList == null || this.Sheet.Document.RacksColumnsList.Count == 0)
				return false;

			// Before recalculate column size need to check - does current column not correct for this rack.
			// Lets calculate min available column and compare with current column.
			//
			// Otherwise, if user selects another(greater) column for the rack group, selects all racks in this group and
			// just move them in another place - all racks indexes are recalculated, because we come here from Rack.OnLengthOrWidthChanged() and
			// selected column will be replaced with minimum available column.
			List<Rack> racksGroup = this.Sheet.GetRackGroup(this);
			if (racksGroup.Count == 0)
				racksGroup.Add(this);

			RackColumn currentColumn = this.Column;
			if (currentColumn != null)
			{
				eColumnBracingType bracingType;
				RackColumn minAvailableColumn = null;
				//double xBracingHeight = 0.0;
				double stiffenersHeight = 0.0;
				if (RackUtils.CalculateRacksColumnSizeAndBracingType(racksGroup, this.Sheet.Document.RacksColumnsList, out bracingType, /*out xBracingHeight,*/ out minAvailableColumn, out stiffenersHeight))
				{
					if (currentColumn == minAvailableColumn)
						return true;

					// compare current and min column
					if (Utils.FGT(currentColumn.Length, minAvailableColumn.Length) || (Utils.FGE(currentColumn.Length, minAvailableColumn.Length) && Utils.FGE(currentColumn.Thickness, minAvailableColumn.Thickness)))
						return true;
				}
			}

			// How to calc column size?
			// When rack length is changed then need to recalc beam length and column length:
			// Rack length = beam length + column length.
			//
			// BUT column length dependth on the beam length - first column in the LoadChart.xlsx.
			// So we receive infinitive loop.
			//
			// How to resolve it?
			// Go through the columns and use they for beam length calculation.
			// After that calculate column from the result beam length and load, if you receive different column then take next column and continue.
			bool bIsColumnFound = false;
			foreach (RackColumn column in this.Sheet.Document.RacksColumnsList)
			{
				if (column == null)
					continue;

				// Set column.
				// Dont worry about bracing type and bracing height - it will be checked at DrawingSheet.MartStateChanged()-CheckRacksColumnSizeAndBracingType().
				string strColumnError;
				this.Set_Column(column, false, false, false, out strColumnError);

				// Recalc beam
				string strBeamError;
				this._RecalcBeamSize(true, out strBeamError);

				// Now calculate column from the beam.
				// Need to check columns on this rack and neighbors.
				eColumnBracingType bracingType;
				RackColumn minAvailableColumn = null;
				//double xBracingHeight = 0.0;
				double stiffenersHeight = 0.0;
				if (!RackUtils.CalculateRacksColumnSizeAndBracingType(racksGroup, this.Sheet.Document.RacksColumnsList, out bracingType, /*out xBracingHeight,*/ out minAvailableColumn, out stiffenersHeight))
					continue;

				if (minAvailableColumn != column)
					continue;
				else
				{
					bIsColumnFound = true;
					break;
				}
			}

			if (!bIsColumnFound)
				strMessage = "Cant find the column for rack's size.";

			return bIsColumnFound;
		}

		//=============================================================================
		private bool _RecalcRackWidth(out string strMessage)
		{
			strMessage = string.Empty;

			UInt32 _newWidth = _CalcRackWidth();

			if (this.IsHorizontal)
			{
				if (_newWidth < MinLength_Y)
				{
					strMessage += "Calculated rack depth(";
					strMessage += _newWidth.ToString();
					strMessage += ") is less than min depth value(";
					strMessage += MinLength_Y.ToString();
					strMessage += ").";

					return false;
				}

				if (_newWidth > MaxLength_Y)
				{
					strMessage += "Calculated rack depth(";
					strMessage += _newWidth.ToString();
					strMessage += ") is bigger than max depth value(";
					strMessage += MaxLength_Y.ToString();
					strMessage += ").";

					return false;
				}
			}
			else
			{
				if (_newWidth < MinLength_X)
				{
					strMessage += "Calculated rack depth(";
					strMessage += _newWidth.ToString();
					strMessage += ") is less than min depth value(";
					strMessage += MinLength_X.ToString();
					strMessage += ").";

					return false;
				}

				if (_newWidth > MaxLength_X)
				{
					strMessage += "Calculated rack depth(";
					strMessage += _newWidth.ToString();
					strMessage += ") is bigger than max depth value(";
					strMessage += MaxLength_X.ToString();
					strMessage += ").";

					return false;
				}
			}

			//
			double _length = this.Length_X;
			double _width = this.Length_Y;
			// convert it to M and back to A
			if (this.IsHorizontal)
				_width = _newWidth;
			else
				_length = _newWidth;

			// just set new length and width if rack is not init
			if (!this.IsInit)
			{
				this.Length_X = _length;
				this.Length_Y = _width;
				return true;
			}

			//
			return RackUtils.ChangeRack(this, _length, _width, false, out strMessage);
		}
		private UInt32 _CalcRackWidth()
		{
			UInt32 width = 0;

			// calc the max level width
			UInt32 maxLevelWidth = 0;
			if (m_Levels != null)
			{
				foreach (RackLevel _level in m_Levels)
				{
					if (_level == null || _level.Pallets == null)
						continue;

					foreach (Pallet _pallet in _level.Pallets)
					{
						if (_pallet == null)
							continue;

						if (_pallet.Width > maxLevelWidth)
							maxLevelWidth = _pallet.Width;
					}
				}
			}
			//
			width += maxLevelWidth;
			width -= (UInt32)Math.Floor(2 * PalletOverhangValue);

			return width;
		}
		public bool _RecalcPalletWidth()
		{
			return _RecalcPalletWidth(null);
		}
		private bool _RecalcPalletWidth(RackLevel _levelToChange)
		{
			// flush: pallet width = rack depth
			// overhang: pallet width = rack depth + 2 * overhang value

			if (m_Levels == null)
				return false;

			int value = Utils.GetWholeNumber(this.Length_Y);
			if (!this.IsHorizontal)
				value = Utils.GetWholeNumber(this.Length_X);

			value += (int)Math.Floor(2 * PalletOverhangValue);

			if (value <= 0)
				return false;

			foreach (RackLevel _level in m_Levels)
			{
				if (_level == null)
					continue;

				if (_levelToChange != null && _level != _levelToChange)
					continue;

				foreach (Pallet _pallet in _level.Pallets)
				{
					if (_pallet == null)
						continue;

					_pallet.Set_Width((UInt32)value);
				}

				_UpdatePalletsVisual(_level);
			}

			return true;
		}
		/// <summary>
		/// Checks pallet width. It depends on the rack depth and overhang value.
		/// If pallet width is greater than rack depth + overhang value, then it will be fixed.
		/// </summary>
		private bool CheckPalletWidth()
		{
			// flush: pallet width = rack depth
			// overhang: pallet width = rack depth + 2 * overhang value
			if (m_Levels == null)
				return false;

			int palletWidthValue = Utils.GetWholeNumber(this.Length_Y);
			if (!this.IsHorizontal)
				palletWidthValue = Utils.GetWholeNumber(this.Length_X);
			palletWidthValue += (int)Math.Floor(2 * PalletOverhangValue);

			if (palletWidthValue <= 0)
				return false;

			foreach (RackLevel level in m_Levels)
			{
				if (level == null)
					continue;

				foreach (Pallet pallet in level.Pallets)
				{
					if (pallet == null)
						continue;

					if(pallet.Width > palletWidthValue)
						pallet.Set_Width((UInt32)palletWidthValue);
				}

				_UpdatePalletsVisual(level);
			}

			return true;
		}


		//=============================================================================
		private bool _RecalcBeamSize(bool bRecalcRackHeight, out string strMessage)
		{
			return _RecalcBeamSize(null, bRecalcRackHeight, out strMessage);
		}
		private bool _RecalcBeamSize(RackLevel levelToChange, bool bRecalcRackHeight, out string strMessage)
		{
			strMessage = string.Empty;

			// Read rules inside _Calc_M_RackLength().
			// Beam length = rack length - 2*column_width - INNER_LENGTH_ADDITIONAL_GAP.
			// Beam load = pallets load or level load

			if (m_Levels == null)
				return false;

			int iBeamSpan = Utils.GetWholeNumber(this.BeamLength);
			if (iBeamSpan <= 0)
				return false;

			foreach (RackLevel level in m_Levels)
			{
				if (level == null)
					continue;

				// there is no beam at the floor
				if (level.Index == 0)
					continue;

				if (levelToChange != null && level != levelToChange)
					continue;

				int beamLoad = (int)level.LevelLoad;

				RackBeam foundBeam = _FindBeam(iBeamSpan, beamLoad);
				if (foundBeam == null)
				{
					strMessage += "Cant find beam with (";
					strMessage += iBeamSpan.ToString();
					strMessage += ") length which can handle (";
					strMessage += beamLoad.ToString();
					strMessage += ") load at level \"";
					strMessage += level.DisplayName;
					strMessage += "\".";

					return false;
				}

				RackLevelBeam levelBeam = new RackLevelBeam(level, foundBeam);
				level.Beam = levelBeam;
			}

			//
			strMessage = "Beams are recalculated. ";

			// Level height should satisfy rules in Rack._RecalcLevelHeight() function.
			// It depends on the next level beam height, so update levels height.
			foreach (RackLevel level in m_Levels)
			{
				if (level == null)
					continue;

				string strLevelHeightMessage;
				if (!level.RecalcLevelHeight(out strLevelHeightMessage))
				{
					if (!string.IsNullOrEmpty(strLevelHeightMessage))
						strMessage += strLevelHeightMessage;

					return false;
				}
			}

			if (bRecalcRackHeight)
			{
				string strRackHeightMessage;
				bool bCalcHeightRes = this._RecalcRackHeight(out strRackHeightMessage);

				if (!string.IsNullOrEmpty(strRackHeightMessage))
					strMessage += strRackHeightMessage;

				return bCalcHeightRes;
			}

			return true;
		}
		private RackBeam _FindBeam(int iBeamSpan, int iBeamLoad)
		{
			// Beam depends on selected column.
			// Look at the Beams sheet at LoadChart.xlsx.
			RackColumn column = this.Column;
			if (column == null)
				return null;

			if (iBeamSpan <= 0 || iBeamLoad <= 0)
				return null;

			return column.FindBeam(iBeamSpan, iBeamLoad);
		}


		//=============================================================================
		private double _RecalcRackWeight()
		{
			double weight = 0;

			// column
			weight += 2 * (this.FrameHeight / 1000) * 4;
			// base plate
			weight += 0.3 * 4;
		
			int iTotalDiagonalBracingsCount = 0;
			double rTotalBracingHeight = 0;

			if (this.Bracing != eColumnBracingType.eUndefined)
			{
				double bottomOffset = Rack.sBracingLinesBottomOffset;
				// calculate X bracings
				if (this.Bracing == eColumnBracingType.eXBracing || this.Bracing == eColumnBracingType.eXBracingWithStiffener)
				{
					iTotalDiagonalBracingsCount += 2 * this.X_Bracings_Count;
					bottomOffset = this.X_Bracing_Height + Rack.sXBracingVerticalOffset;
				}
				// calculate normal bracings
				int normalBracingsCount = (int)Math.Floor((this.FrameHeight - bottomOffset - Rack.sTopHorizontalBracingOffset) / Rack.sBracingVerticalStep);
				iTotalDiagonalBracingsCount += normalBracingsCount;
				rTotalBracingHeight = bottomOffset + normalBracingsCount * Rack.sBracingVerticalStep;
			}

			if (iTotalDiagonalBracingsCount == 0)
				return 0.0;

			// Calculate horizontal bracings weight
			int iHorizontalBracingCount = 0;
			if (this.Bracing == eColumnBracingType.eNormalBracing || this.Bracing == eColumnBracingType.eNormalBracingWithStiffener)
				iHorizontalBracingCount = 2;
			else if (this.Bracing == eColumnBracingType.eXBracing || this.Bracing == eColumnBracingType.eXBracingWithStiffener)
			{
				// X bracing has 1 horizontal line at bot, X bracing top and top of the rack.
				iHorizontalBracingCount = 3;
			}
			//
			if (Utils.FGE(this.FrameHeight - rTotalBracingHeight, Rack.sTopHorizontalBracingMinDistance))
				iHorizontalBracingCount += 1;
			//
			double columnToColumnDistance = this.InnerLength;
			weight += iHorizontalBracingCount * (columnToColumnDistance / 1000) * 0.5;

			// Calculate diagonal bracings weight
			double rDiagonalMemberLength = 0;
			if (this.Bracing != eColumnBracingType.eUndefined)
				rDiagonalMemberLength = Math.Sqrt(Rack.sBracingVerticalStep * Rack.sBracingVerticalStep + columnToColumnDistance * columnToColumnDistance);
			//
			weight += iTotalDiagonalBracingsCount * (rDiagonalMemberLength / 1000) * 0.5;

			// spacer
			int iSpacerCount = 2;
			if (Utils.FGE(this.FrameHeight - rTotalBracingHeight, Rack.sTopHorizontalBracingMinDistance))
				iSpacerCount += 2;
			//
			weight += iSpacerCount * 0.07;

			// M8 bolt and M8 nut
			int iM8Bolt = 0;
			if (this.Bracing != eColumnBracingType.eUndefined)
			{
				iM8Bolt = iTotalDiagonalBracingsCount + 3;
				if (Utils.FGE(this.FrameHeight - rTotalBracingHeight, Rack.sTopHorizontalBracingMinDistance))
					iM8Bolt += 2;
			}
			// count of M8 bolts = count of M8 nuts
			weight += (0.15 + 0.05) * iM8Bolt;

			// load beam hook
			double rLoadBeamHook = this.NumberOfLevels_WithoutGround * 2 * 0.25;
			// load beam section
			double rLoadBeamSection = this.NumberOfLevels_WithoutGround * (this.BeamLength / 1000) * 1;
			// load beam
			weight += 2 * rLoadBeamHook + rLoadBeamSection;

			return weight;
		}

		//=============================================================================
		private void _UpdatePalletsVisual(RackLevel _level)
		{
			if (_level == null || _level.Pallets == null)
				return;

			// For update pallet remove all items and add them.
			// Otherwise all old pallets in the collection will have old value.
			List<Pallet> _palletsList = new List<Pallet>();
			_palletsList.AddRange(_level.Pallets);
			_level.Pallets.Clear();
			foreach (Pallet _pallet in _palletsList)
			{
				if (_pallet == null)
					continue;

				_level.Pallets.Add(_pallet);
			}
		}

		//=============================================================================
		private void _SetLevelsOwner()
		{
			if (m_Levels == null)
				return;

			m_SelectedLevel = m_Levels.FirstOrDefault(_level => _level != null && _level.IsSelected);
			if (m_SelectedLevel == null && m_Levels.Count > 0)
				m_SelectedLevel = m_Levels[0];

			// set owner
			foreach (RackLevel level in m_Levels)
			{
				if (level == null)
					continue;

				level.Owner = this;
				// Set beam owner.
				if (level.Beam != null && level.Beam.OwnerLevel != level)
					level.Beam.OwnerLevel = level;
			}
		}

		//=============================================================================
		// Update properties which depends on the rack state.
		private void _UpdateProperties()
		{
			if (m_Properties != null)
			{
				Property_ViewModel _lengthProp = m_Properties.FirstOrDefault(p => p != null && p.SystemName == PROP_DIMENSION_X);
				if (_lengthProp != null)
					_lengthProp.IsReadOnly = this.m_bDisableChangeSizeGripPoints;

				Property_ViewModel _widthProp = m_Properties.FirstOrDefault(p => p != null && p.SystemName == PROP_DIMENSION_Y);
				if (_widthProp != null)
					_widthProp.IsReadOnly = this.m_bDisableChangeSizeGripPoints;

				// if levels are not the same then user cant edit Rack Height property
				Property_ViewModel _heightProp = m_Properties.FirstOrDefault(p => p != null && p.SystemName == PROP_DIMENSION_Z);
				if (_heightProp != null)
					_heightProp.IsReadOnly = !this.m_bAreLevelsTheSame;
			}
		}

		//=============================================================================
		/// <summary>
		/// Returns column from DrawingDocument.RacksColumnsList by guid.
		/// </summary>
		private RackColumn _GetColumnByGUID(Guid columnGUID)
		{
			if (columnGUID == Guid.Empty)
				return null;

			if (this.Sheet != null
				&& this.Sheet.Document != null
				&& this.Sheet.Document.RacksColumnsList != null)
			{
				return this.Sheet.Document.RacksColumnsList.FirstOrDefault(column => column != null && column.GUID == columnGUID);
			}

			return null;
		}

		//=============================================================================
		/// <summary>
		/// Recalculate min available column and remove m_IsColumnSetManually flag.
		/// </summary>
		private void _ResetColumn()
		{
			eColumnBracingType bracingType;
			//double xBracingHeight = 0.0;
			RackColumn minAvailableColumn = null;
			double stiffenersHeight = 0.0;
			if (RackUtils.CalculateRacksColumnSizeAndBracingType(new List<Rack>() { this }, this.Sheet.Document.RacksColumnsList, out bracingType, /*out xBracingHeight,*/ out minAvailableColumn, out stiffenersHeight))
			{
				this.m_MinColumnGUID = minAvailableColumn.GUID;
				this.m_ColumnGUID = minAvailableColumn.GUID;
				this.m_IsColumnSetManually = false;
				this.Bracing = bracingType;
				if (eColumnBracingType.eXBracing == this.Bracing || eColumnBracingType.eXBracingWithStiffener == this.Bracing)
					this.X_Bracing_Height = Rack.X_BRACING_MIN_HEIGHT;//xBracingHeight;
				else
					this.X_Bracing_Height = 0.0;
				this.StiffenersHeight = stiffenersHeight;

				string strCalcLengthError;
				this._RecalcRackLength(false, false, out strCalcLengthError);
			}
		}

		private void _TryDrawColumnGuards(DrawingContext dc, ICoordinateSystem cs, IGeomDisplaySettings geomDisplaySettings = null)
		{
			double colSize = Column.Length;

			if (IsHorizontal)
			{
				if (this.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.TOP))
				{
					if (IsFirstInRowColumn)
					{
						_DrawColumnGuard(new Point(this.TopLeft_GlobalPoint.X + colSize, this.TopLeft_GlobalPoint.Y), ConectedAisleSpaceDirection.TOP, dc, cs, geomDisplaySettings);
					}

					_DrawColumnGuard(this.TopRight_GlobalPoint, ConectedAisleSpaceDirection.TOP, dc, cs, geomDisplaySettings);
				}

				if (this.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.BOTTOM))
				{
					if (IsFirstInRowColumn)
					{
						_DrawColumnGuard(new Point(this.BottomLeft_GlobalPoint.X + colSize, this.BottomLeft_GlobalPoint.Y), ConectedAisleSpaceDirection.BOTTOM, dc, cs, geomDisplaySettings);
					}

					_DrawColumnGuard(this.BottomRight_GlobalPoint, ConectedAisleSpaceDirection.BOTTOM, dc, cs, geomDisplaySettings);
				}
			}
			else
			{
				// for vertical top is left and bottom is right
				if (this.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.LEFT))
				{
					if (IsFirstInRowColumn)
					{
						_DrawColumnGuard(this.TopLeft_GlobalPoint, ConectedAisleSpaceDirection.LEFT, dc, cs, geomDisplaySettings);
					}

					_DrawColumnGuard(new Point(this.BottomLeft_GlobalPoint.X, this.BottomLeft_GlobalPoint.Y - colSize), ConectedAisleSpaceDirection.LEFT, dc, cs, geomDisplaySettings);
				}

				if (this.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.RIGHT))
				{
					if (IsFirstInRowColumn)
					{
						_DrawColumnGuard(this.TopRight_GlobalPoint, ConectedAisleSpaceDirection.RIGHT, dc, cs, geomDisplaySettings);
					}

					_DrawColumnGuard(new Point(this.BottomRight_GlobalPoint.X, this.BottomRight_GlobalPoint.Y - colSize), ConectedAisleSpaceDirection.RIGHT, dc, cs, geomDisplaySettings);
				}
			}
		}

		private void _TryDrawRowGuards(DrawingContext dc, ICoordinateSystem cs, IGeomDisplaySettings geomDisplaySettings = null)
		{
			if (IsHorizontal)
			{
				if (IsUnderpassAvailable || this.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.LEFT))
				{
					_DrawRowGuard(this.TopLeft_GlobalPoint, ConectedAisleSpaceDirection.LEFT, dc, cs, geomDisplaySettings);
				}

				if (IsUnderpassAvailable || this.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.RIGHT))
				{
					_DrawRowGuard(this.TopRight_GlobalPoint, ConectedAisleSpaceDirection.RIGHT, dc, cs, geomDisplaySettings);
				}
			}
			else
			{
				if (IsUnderpassAvailable || this.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.TOP))
				{
					_DrawRowGuard(this.TopLeft_GlobalPoint, ConectedAisleSpaceDirection.TOP, dc, cs, geomDisplaySettings);
				}

				if (IsUnderpassAvailable || this.ConectedAisleSpaceDirections.HasFlag(ConectedAisleSpaceDirection.BOTTOM))
				{
					_DrawRowGuard(this.BottomLeft_GlobalPoint, ConectedAisleSpaceDirection.BOTTOM, dc, cs, geomDisplaySettings);
				}
			}
		}


		private void _DrawColumnGuard(Point originPoint, ConectedAisleSpaceDirection dimPlacement, DrawingContext dc, ICoordinateSystem cs, IGeomDisplaySettings geomDisplaySettings = null)
		{
			double colSize = Column.Length;
			double guardSuportLinePixelOffset = 15;

			int xOffset = 40;
			int yOffset = 45;
			int guardHeight = 105;
			int guardAngleElevation = 15;

			Color rackColGuardColor = Colors.Red;
			if (CurrentGeometryColorsTheme.CurrentTheme != null)
			{
				Color color;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackColumnGuardDefault, out color))
					rackColGuardColor = color;
			}

			SolidColorBrush rackColGuardBrush = new SolidColorBrush(rackColGuardColor);
			Pen pen = new Pen(rackColGuardBrush, 2.0);

			List<Tuple<Point, Point>> lines = new List<Tuple<Point, Point>>(5);

			Point step;
			Point nextStep;

			switch (dimPlacement)
			{
				case ConectedAisleSpaceDirection.TOP:
					step = new Point(originPoint.X + xOffset, originPoint.Y - yOffset);
					nextStep = new Point(step.X, step.Y - (guardHeight - guardAngleElevation));
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = nextStep;
					nextStep = new Point(step.X - xOffset - (colSize / 2), step.Y - guardAngleElevation);
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = nextStep;
					nextStep = new Point(step.X - xOffset - (colSize / 2), step.Y + guardAngleElevation);
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));
					
					step = nextStep;
					nextStep = new Point(step.X, step.Y + (guardHeight - guardAngleElevation));
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = lines[0].Item1;
					nextStep = lines[3].Item2;
					step.Y -= GetWidthInPixels(cs, guardSuportLinePixelOffset);
					nextStep.Y -= GetWidthInPixels(cs, guardSuportLinePixelOffset);
					lines.Add(new Tuple<Point, Point>(step, nextStep));

					break;

				case ConectedAisleSpaceDirection.BOTTOM:
					step = new Point(originPoint.X + xOffset, originPoint.Y + yOffset);
					nextStep = new Point(step.X, step.Y + (guardHeight - guardAngleElevation)); ;
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = nextStep;
					nextStep = new Point(step.X - xOffset - (colSize / 2), step.Y + guardAngleElevation);
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = nextStep;
					nextStep = new Point(step.X - xOffset - (colSize / 2), step.Y - guardAngleElevation);
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = nextStep;
					nextStep = new Point(step.X, step.Y - (guardHeight - guardAngleElevation));
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = lines[0].Item1;
					nextStep = lines[3].Item2;
					step.Y += GetWidthInPixels(cs, guardSuportLinePixelOffset);
					nextStep.Y += GetWidthInPixels(cs, guardSuportLinePixelOffset);
					lines.Add(new Tuple<Point, Point>(step, nextStep));
					break;

				case ConectedAisleSpaceDirection.LEFT:
					step = new Point(originPoint.X - yOffset, originPoint.Y - xOffset);
					nextStep = new Point(step.X - (guardHeight - guardAngleElevation), step.Y); ;
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = nextStep;
					nextStep = new Point(step.X - guardAngleElevation, step.Y + xOffset + (colSize / 2));
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = nextStep;
					nextStep = new Point(step.X + guardAngleElevation, step.Y + xOffset + (colSize / 2));
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = nextStep;
					nextStep = new Point(step.X + (guardHeight - guardAngleElevation), step.Y);
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = lines[0].Item1;
					nextStep = lines[3].Item2;
					step.X -= GetWidthInPixels(cs, guardSuportLinePixelOffset);
					nextStep.X -= GetWidthInPixels(cs, guardSuportLinePixelOffset);
					lines.Add(new Tuple<Point, Point>(step, nextStep));
					break;

				case ConectedAisleSpaceDirection.RIGHT:
					step = new Point(originPoint.X + yOffset, originPoint.Y - xOffset);
					nextStep = new Point(step.X + (guardHeight - guardAngleElevation), step.Y); ;
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = nextStep;
					nextStep = new Point(step.X + guardAngleElevation, step.Y + xOffset + (colSize / 2));
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = nextStep;
					nextStep = new Point(step.X - guardAngleElevation, step.Y + xOffset + (colSize / 2));
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = nextStep;
					nextStep = new Point(step.X - (guardHeight - guardAngleElevation), step.Y);
					lines.Add(new Tuple<Point, Point>(GetLocalPoint(cs, step), GetLocalPoint(cs, nextStep)));

					step = lines[0].Item1;
					nextStep = lines[3].Item2;
					step.X += GetWidthInPixels(cs, guardSuportLinePixelOffset);
					nextStep.X += GetWidthInPixels(cs, guardSuportLinePixelOffset);
					lines.Add(new Tuple<Point, Point>(step, nextStep));
					break;
			}

            foreach (var line in lines)
            {
				dc.DrawLine(pen, line.Item1, line.Item2);
			}
        }

		private void _DrawRowGuard(Point topPoint, ConectedAisleSpaceDirection dimPlacement, DrawingContext dc, ICoordinateSystem cs, IGeomDisplaySettings geomDisplaySettings = null)
		{
			int offset = 50;
			double smallGuardSupportWidth = 160;
			double smallGuardSupportDepth = 70;
			double guardBeamLength = this.Depth - 100;
			double guardBeamWidth = 58;
			double guardBeamOffset = offset + ((smallGuardSupportWidth - guardBeamWidth) / 2);

			Color rackRowGuardColor = Colors.Orange;
			if (CurrentGeometryColorsTheme.CurrentTheme != null)
			{
				Color color;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eRackRowGuardDefault, out color))
					rackRowGuardColor = color;
			}

			SolidColorBrush rackRowGuardBrush = new SolidColorBrush(rackRowGuardColor);
			Pen pen = new Pen(rackRowGuardBrush, 1.0);
			Pen guardBeamPen = new Pen(rackRowGuardBrush, 58);

			Point right;
			Point left;

			int underpassDim = 1;
			
            if (IsUnderpassAvailable)
            {
				underpassDim = -1;
				rackRowGuardBrush.Opacity = 0.8;
                guardBeamPen.DashStyle = DashStyles.Dash;
            }

			switch (dimPlacement)
			{
				case ConectedAisleSpaceDirection.LEFT:
					right = new Point(topPoint.X - (underpassDim * offset), topPoint.Y + offset);
					left = new Point(topPoint.X - (underpassDim * offset) - (underpassDim * smallGuardSupportWidth), topPoint.Y + offset + smallGuardSupportDepth);

					dc.DrawRectangle(rackRowGuardBrush, pen, new Rect(GetLocalPoint(cs, right), GetLocalPoint(cs, left)));

					right = new Point(topPoint.X - (underpassDim * offset), topPoint.Y + offset + (guardBeamLength - smallGuardSupportDepth));
					left = new Point(topPoint.X - (underpassDim * offset) - (underpassDim * smallGuardSupportWidth), topPoint.Y + offset + guardBeamLength);

					dc.DrawRectangle(rackRowGuardBrush, pen, new Rect(GetLocalPoint(cs, right), GetLocalPoint(cs, left)));

					right = new Point(topPoint.X - (underpassDim * guardBeamOffset), topPoint.Y + offset + smallGuardSupportDepth);
					left = new Point(topPoint.X - (underpassDim * guardBeamOffset) - (underpassDim * guardBeamWidth), topPoint.Y + offset + guardBeamLength - smallGuardSupportDepth);

                    if (IsUnderpassAvailable)
                    {
						_FillRectDashed(right, left, true, dc, cs, rackRowGuardBrush, pen);
                    }
					else
                    {
						dc.DrawRectangle(rackRowGuardBrush, pen, new Rect(GetLocalPoint(cs, left), GetLocalPoint(cs, right)));
					}
                    break;

				case ConectedAisleSpaceDirection.TOP:
					right = new Point(topPoint.X + offset, topPoint.Y - (underpassDim * offset));
					left = new Point(topPoint.X + offset + smallGuardSupportDepth, topPoint.Y - (underpassDim * offset) - (underpassDim * smallGuardSupportWidth));

					dc.DrawRectangle(rackRowGuardBrush, pen, new Rect(GetLocalPoint(cs, right), GetLocalPoint(cs, left)));

					right = new Point(topPoint.X + offset + (guardBeamLength - smallGuardSupportDepth), topPoint.Y - (underpassDim * offset));
					left = new Point(topPoint.X + offset + guardBeamLength, topPoint.Y - (underpassDim * offset) - (underpassDim * smallGuardSupportWidth));

					dc.DrawRectangle(rackRowGuardBrush, pen, new Rect(GetLocalPoint(cs, right), GetLocalPoint(cs, left)));

					right = new Point(topPoint.X + offset, topPoint.Y - (underpassDim * guardBeamOffset));
					left = new Point(topPoint.X + offset + guardBeamLength, topPoint.Y - (underpassDim * offset) - (underpassDim * guardBeamOffset));

					if (IsUnderpassAvailable)
					{
						_FillRectDashed(left, right, false, dc, cs, rackRowGuardBrush, pen);
					}
					else
					{
						dc.DrawRectangle(rackRowGuardBrush, pen, new Rect(GetLocalPoint(cs, right), GetLocalPoint(cs, left)));
					}
					break;


				case ConectedAisleSpaceDirection.RIGHT:
					right = new Point(topPoint.X + (underpassDim * offset), topPoint.Y + offset);
					left = new Point(topPoint.X + (underpassDim * offset) + (underpassDim * smallGuardSupportWidth), topPoint.Y + offset + smallGuardSupportDepth);

					dc.DrawRectangle(rackRowGuardBrush, pen, new Rect(GetLocalPoint(cs, right), GetLocalPoint(cs, left)));

					right = new Point(topPoint.X + (underpassDim * offset), topPoint.Y + offset + guardBeamLength - smallGuardSupportDepth);
					left = new Point(topPoint.X + (underpassDim * offset) + (underpassDim * smallGuardSupportWidth), topPoint.Y + offset + guardBeamLength);

					dc.DrawRectangle(rackRowGuardBrush, pen, new Rect(GetLocalPoint(cs, right), GetLocalPoint(cs, left)));

					right = new Point(topPoint.X + (underpassDim * guardBeamOffset), topPoint.Y + offset + smallGuardSupportDepth);
					left = new Point(topPoint.X + (underpassDim * guardBeamOffset) + (underpassDim * guardBeamWidth), topPoint.Y + offset + guardBeamLength - smallGuardSupportDepth);
					
					if (IsUnderpassAvailable)
					{
						_FillRectDashed(right, left, true, dc, cs, rackRowGuardBrush, pen);
					}
					else
					{
						dc.DrawRectangle(rackRowGuardBrush, pen, new Rect(GetLocalPoint(cs, right), GetLocalPoint(cs, left)));
					}

                    break;

				case ConectedAisleSpaceDirection.BOTTOM:
					right = new Point(topPoint.X + offset, topPoint.Y + (underpassDim * offset));
					left = new Point(topPoint.X + offset + smallGuardSupportDepth, topPoint.Y + (underpassDim * offset) + (underpassDim * smallGuardSupportWidth));

					dc.DrawRectangle(rackRowGuardBrush, pen, new Rect(GetLocalPoint(cs, right), GetLocalPoint(cs, left)));

					right = new Point(topPoint.X + offset + guardBeamLength - smallGuardSupportDepth, topPoint.Y + (underpassDim * offset));
					left = new Point(topPoint.X + offset + guardBeamLength, topPoint.Y + (underpassDim * offset) + (underpassDim * smallGuardSupportWidth));

					dc.DrawRectangle(rackRowGuardBrush, pen, new Rect(GetLocalPoint(cs, right), GetLocalPoint(cs, left)));

					right = new Point(topPoint.X + offset, topPoint.Y + (underpassDim * guardBeamOffset));
					left = new Point(topPoint.X + offset + guardBeamLength, topPoint.Y + (underpassDim * offset) + (underpassDim * guardBeamOffset));

					if (IsUnderpassAvailable)
					{
						_FillRectDashed(left, right, false, dc, cs, rackRowGuardBrush, pen);
					}
					else
					{
						dc.DrawRectangle(rackRowGuardBrush, pen, new Rect(GetLocalPoint(cs, right), GetLocalPoint(cs, left)));
					}

					break;
			}
		}

		private void _FillRectDashed(Point higher, Point lower, bool isVertical, DrawingContext dc, ICoordinateSystem cs, Brush brush, Pen pen)
		{
			double dashInterval = 100;

			if (isVertical)
			{
				double lastY = higher.Y;

				lower.Y -= dashInterval;
				while (lower.Y >= lastY)
				{
					higher.Y = lower.Y - dashInterval;

					if (higher.Y < lastY)
						higher.Y = lastY;

					dc.DrawRectangle(brush, pen, new Rect(GetLocalPoint(cs, higher), GetLocalPoint(cs, lower)));

					lower.Y = higher.Y - dashInterval;
				}
            }
            else
            {
				double lastX = higher.X;

				lower.X += dashInterval;
				while (lower.X <= lastX)
				{
					higher.X = lower.X + dashInterval;

					if (higher.X > lastX)
						higher.X = lastX;

					dc.DrawRectangle(brush, pen, new Rect(GetLocalPoint(cs, higher), GetLocalPoint(cs, lower)));

					lower.X = higher.X + dashInterval;
				}
			}
		}

		private void _DrawColumnSpaceOffset(DrawingContext dc, ICoordinateSystem cs, IGeomDisplaySettings displaySettings)
		{
			Point start;
			Point end;
			if (IsHorizontal)
			{
				if (IsFirstInRowColumn)
				{
					start = TopLeft_GlobalPoint;
					start.X += Column.Length;

					end = BottomLeft_GlobalPoint;
					end.X += Column.Length;

					dc.DrawLine(BorderPen, GetLocalPoint(cs, start), GetLocalPoint(cs, end));
				}

				start = TopRight_GlobalPoint;
				start.X -= Column.Length;

				end = BottomRight_GlobalPoint;
				end.X -= Column.Length;
			}
			else
			{
				if (IsFirstInRowColumn)
				{
					start = TopLeft_GlobalPoint;
					start.Y += Column.Length;

					end = TopRight_GlobalPoint;
					end.Y += Column.Length;

					dc.DrawLine(BorderPen, GetLocalPoint(cs, start), GetLocalPoint(cs, end));
				}

				start = BottomLeft_GlobalPoint;
				start.Y -= Column.Length;

				end = BottomRight_GlobalPoint;
				end.Y -= Column.Length;
			}

			dc.DrawLine(BorderPen, GetLocalPoint(cs, start), GetLocalPoint(cs, end));
		}

		#endregion

		#region Serialization

		// (!!!) IF YOU WANT TO ADD NEW PROPERTY ALSO ADD IT TO Rack_State (!!!)
		// Otherwise, it will not be saved\restored after document undo\redo operations.
		//=============================================================================
		// 1.1 New UI properties
		// 2.1 Increase major, old drawing have incorrect min, max length values. Now they are depends on beams list.
		// 2.2 Increase minor - _On_Height_Changed doesnt calc m_ClearAvailableHeight with pallets height
		// 3.2 Remove Clear Available Height, it is calculated based on the roof type and rack position.
		// 4.2 PalletType(Overhang\Flush) is removed and placed in the DrawingDocument.
		// 5.2 Add ColumnGUID and MinColumnGUID instead ColumnType.
		//     Add Bracing and X_BracingHeight.
		//     Remove m_bIsColumnAutoSelectEnabled.
		// 5.3 Add TieBeamFrame flag.
		// 5.4 Add m_IsColumnSetManually bool property.
		// 5.5 Add m_TieBeamShouldBeAdded
		// 5.6 Add m_StiffenersHeight
		// 5.7 Remove m_TieBeamShouldBeAdded, add m_RequiredTieBeamFrames
		// 5.8 Add m_RackHeightWithTieBeam_IsMoreThan_MaxHeight
		// 5.9 Add ConectedAisleSpaceDirections
		protected static string _sRack_strMajor = "Rack_MAJOR";
		protected static int _sRack_MAJOR = 5;
		protected static string _sRack_strMinor = "Rack_MINOR";
		protected static int _sRack_MINOR = 8;
		//
		public bool _Call_On_Height_Changed = false;
		//=============================================================================
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			//
			info.AddValue(_sRack_strMajor, _sRack_MAJOR);
			info.AddValue(_sRack_strMinor, _sRack_MINOR);

			//
			info.AddValue("IsFirstInRowColumn", IsFirstInRowColumn);
			info.AddValue("SizeIndex", SizeIndex);
			info.AddValue("CanCreateRow", CanCreateRow);

			// 1.1
			//info.AddValue("ClearAvailableHeight", m_ClearAvailableHeight);
			//
			info.AddValue("IsUnderpassAvailable", m_IsUnderpassAvailable);
			info.AddValue("Underpass", m_Underpass);
			//
			info.AddValue("IsMaterialOnGround", m_bIsMaterialOnGround);
			info.AddValue("MaterialHeightOnGround", 0);
			info.AddValue("MaterialWeightOnGround", 0);
			//
			info.AddValue("ShowPallet", m_bShowPallet);
			// 4.2 Removed
			//info.AddValue("PalletType", m_PalletType);
			//
			info.AddValue("AreLevelsTheSame", m_bAreLevelsTheSame);
			info.AddValue("Levels", m_Levels);
			// Removed in 5.2
			//info.AddValue("IsColumnAutoSelectEnabled", m_bIsColumnAutoSelectEnabled);
			//info.AddValue("ColumnType", m_ColumnType);
			//
			info.AddValue("SplitColumn", m_bSplitColumn);
			info.AddValue("Column_FirstPartLength", m_Column_FirstPartLength);
			info.AddValue("Column_SecondPartLength", m_Column_SecondPartLength);
			//
			info.AddValue("DisableChangeSizeGripPoints", m_bDisableChangeSizeGripPoints);

			// 5.2
			info.AddValue("MinColumnGUID", m_MinColumnGUID);
			info.AddValue("ColumnGUID", m_ColumnGUID);
			info.AddValue("Bracing", m_Bracing);
			info.AddValue("X_BracingHeight", m_X_BracingHeight);

			// 5.3
			info.AddValue("TieBeamFrame", m_TieBeamFrame);

			// 5.4
			info.AddValue("IsColumnSetManually", m_IsColumnSetManually);

			// Remove in 5.7
			//// 5.5
			//info.AddValue("TieBeamShouldBeAdded", m_TieBeamShouldBeAdded);
			// Add for compatibility with old drawings
			info.AddValue("TieBeamShouldBeAdded", m_RequiredTieBeamFrames != eTieBeamFrame.eNone);

			// 5.6
			info.AddValue("StiffenersHeight", m_StiffenersHeight);

			// 5.7
			info.AddValue("RequiredTieBeamFrames", m_RequiredTieBeamFrames);

			// 5.8
			info.AddValue("RackHeightWithTieBeam_IsMoreThan_MaxHeight", m_RackHeightWithTieBeam_IsMoreThan_MaxHeight);

			// 5.9
			info.AddValue("ConectedAisleSpaceDirections", ConectedAisleSpaceDirections);
		}
		//=============================================================================
		public Rack(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sRack_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sRack_strMinor, typeof(int));
			if (iMajor > _sRack_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sRack_MAJOR && iMinor > _sRack_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sRack_MAJOR)
			{
				try
				{
					m_bIsFirstInRowColumn = (bool)info.GetValue("IsFirstInRowColumn", typeof(bool));
					m_iSizeIndex = (int)info.GetValue("SizeIndex", typeof(int));
					CanCreateRow = (bool)info.GetValue("CanCreateRow", typeof(bool));

					// 1.1
					if (iMajor >= 1 && iMinor >= 1)
					{
						//m_ClearAvailableHeight = (UInt32)info.GetValue("ClearAvailableHeight", typeof(UInt32));
						//
						m_IsUnderpassAvailable = (bool)info.GetValue("IsUnderpassAvailable", typeof(bool));
						m_Underpass = (UInt32)info.GetValue("Underpass", typeof(UInt32));
						//
						m_bIsMaterialOnGround = (bool)info.GetValue("IsMaterialOnGround", typeof(bool));
						//
						m_bShowPallet = (bool)info.GetValue("ShowPallet", typeof(bool));
						// 4.2 Removed
						//m_PalletType = (ePalletType)info.GetValue("PalletType", typeof(ePalletType));
						//
						m_bAreLevelsTheSame = (bool)info.GetValue("AreLevelsTheSame", typeof(bool));
						m_Levels = (ObservableCollection<RackLevel>)info.GetValue("Levels", typeof(ObservableCollection<RackLevel>));
						// Removed in 5.2
						//m_bIsColumnAutoSelectEnabled = (bool)info.GetValue("IsColumnAutoSelectEnabled", typeof(bool));
						//m_ColumnType = (eColumnType)info.GetValue("ColumnType", typeof(eColumnType));
						//
						m_bSplitColumn = (bool)info.GetValue("SplitColumn", typeof(bool));
						m_Column_FirstPartLength = (UInt32)info.GetValue("Column_FirstPartLength", typeof(UInt32));
						m_Column_SecondPartLength = (UInt32)info.GetValue("Column_SecondPartLength", typeof(UInt32));
						//
						m_bDisableChangeSizeGripPoints = (bool)info.GetValue("DisableChangeSizeGripPoints", typeof(bool));
					}

					if (iMajor <= 2 && iMinor <= 1)
						_Call_On_Height_Changed = true;

					if(iMajor >= 5 && iMinor >= 2)
					{
						m_MinColumnGUID = (Guid)info.GetValue("MinColumnGUID", typeof(Guid));
						m_ColumnGUID = (Guid)info.GetValue("ColumnGUID", typeof(Guid));
						m_Bracing = (eColumnBracingType)info.GetValue("Bracing", typeof(eColumnBracingType));
						m_X_BracingHeight = (double)info.GetValue("X_BracingHeight", typeof(double));
					}

					if(iMajor >= 5 && iMinor >= 3)
					{
						m_TieBeamFrame = (eTieBeamFrame)info.GetValue("TieBeamFrame", typeof(eTieBeamFrame));
					}

					if(iMajor >= 5 && iMinor >= 4)
					{
						m_IsColumnSetManually = (bool)info.GetValue("IsColumnSetManually", typeof(bool));
					}

					// Removed in 5.7
					//if (iMajor >= 5 && iMinor >= 5)
					//	m_TieBeamShouldBeAdded = (bool)info.GetValue("TieBeamShouldBeAdded", typeof(bool));

					if (iMajor >= 5 && iMinor >= 6)
						m_StiffenersHeight = (double)info.GetValue("StiffenersHeight", typeof(double));

					if (iMajor >= 5 && iMinor >= 7)
						m_RequiredTieBeamFrames = (eTieBeamFrame)info.GetValue("RequiredTieBeamFrames", typeof(eTieBeamFrame));

					if (iMajor >= 5 && iMinor >= 8)
						m_RackHeightWithTieBeam_IsMoreThan_MaxHeight = (bool)info.GetValue("RackHeightWithTieBeam_IsMoreThan_MaxHeight", typeof(bool));
					
					if (iMajor >= 5 && iMinor >= 9)
						ConectedAisleSpaceDirections = (ConectedAisleSpaceDirection)info.GetValue("ConectedAisleSpaceDirections", typeof(ConectedAisleSpaceDirection));
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

			//
			_SetLevelsOwner();
			//
			if (_Call_On_Height_Changed)
			{
				string strHeightError;
				_On_Height_Changed(false, out strHeightError);
			}
		}

		#endregion
	}
}
