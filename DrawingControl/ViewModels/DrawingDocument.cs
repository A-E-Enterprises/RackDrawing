using AppInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace DrawingControl
{
	/// <summary>
	/// RackSizeIndex wrapper.
	/// It is used for display racks unique sizes in the app.
	/// In DEBUG mode.
	/// </summary>
	public class RackSizeIndexVM : BaseViewModel
	{
		public RackSizeIndexVM(int sizeIndex, RackSizeIndex rackSizeIndex)
		{
			m_SizeIndex = sizeIndex;

			if(rackSizeIndex != null)
			{
				m_Count = (int)rackSizeIndex.Count;
				m_LengthX = rackSizeIndex.State.Length_X;
			}
		}

		#region Properties

		//=============================================================================
		private int m_SizeIndex = 0;
		public int SizeIndex { get { return m_SizeIndex; } }

		//=============================================================================
		// Total count of racks with this index.
		private int m_Count = 0;
		public int Count { get { return m_Count; } }

		//=============================================================================
		private double m_LengthX = 0;
		public double LengthX { get { return m_LengthX; } }

		#endregion
	}

	[Serializable]
	public class ColumnSizeIndex : ISerializable, IDeserializationCallback, IClonable
	{
		public ColumnSizeIndex(string strKey)
		{
			Key = strKey;
			Count = 0;
		}
		public ColumnSizeIndex(ColumnSizeIndex columnSizeIndex)
		{
			this.Key = string.Empty;
			this.Count = 0;

			if(columnSizeIndex != null)
			{
				this.Key = columnSizeIndex.Key;
				this.Count = columnSizeIndex.Count;
			}
		}

		#region Properties

		// width_height
		public string Key { get; set; }
		// count of columns
		public UInt32 Count { get; set; }

		#endregion

		#region Serialization

		//=============================================================================
		//
		// 1.0
		//
		protected static string _sColumnSizeIndex_strMajor = "ColumnSizeIndex_MAJOR";
		protected static int _sColumnSizeIndex_MAJOR = 1;
		protected static string _sColumnSizeIndex_strMinor = "ColumnSizeIndex_MINOR";
		protected static int _sColumnSizeIndex_MINOR = 0;
		public ColumnSizeIndex(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sColumnSizeIndex_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sColumnSizeIndex_strMinor, typeof(int));
			if (iMajor > _sColumnSizeIndex_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sColumnSizeIndex_MAJOR && iMinor > _sColumnSizeIndex_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sColumnSizeIndex_MAJOR)
			{
				//
				Key = (string)info.GetValue("Key", typeof(string));
				Count = (UInt32)info.GetValue("Count", typeof(UInt32));
			}
			else
				++DrawingDocument._sBiggerMajorNumber;
		}
		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sColumnSizeIndex_strMajor, _sColumnSizeIndex_MAJOR);
			info.AddValue(_sColumnSizeIndex_strMinor, _sColumnSizeIndex_MINOR);

			//
			info.AddValue("Key", Key);
			info.AddValue("Count", Count);
		}
		//=============================================================================
		public virtual void OnDeserialization(object sender) { }

		#endregion

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new ColumnSizeIndex(this);
		}
	}

	[Serializable]
	public class RackSizeIndex : ISerializable, IDeserializationCallback, IClonable
	{
		public RackSizeIndex(M_RackState state)
		{
			State = state;
			Count = 1;
		}
		public RackSizeIndex(RackSizeIndex rackSizeIndex)
		{
			this.State = null;
			this.Count = 0;

			if(rackSizeIndex != null)
			{
				if(rackSizeIndex.State != null)
					this.State = rackSizeIndex.State.Clone() as M_RackState;
				this.Count = rackSizeIndex.Count;
			}
		}

		#region Properties

		//
		public M_RackState State { get; set; }
		// count of racks
		public UInt32 Count { get; set; }

		#endregion

		#region Serialization

		//=============================================================================
		//
		// 1.0
		// 2.0 change Key type from string to M_RackState. Also rename it to "State"
		//
		protected static string _sRackSizeIndex_strMajor = "RackSizeIndex_MAJOR";
		protected static int _sRackSizeIndex_MAJOR = 2;
		protected static string _sRackSizeIndex_strMinor = "RackSizeIndex_MINOR";
		protected static int _sRackSizeIndex_MINOR = 0;
		public RackSizeIndex(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sRackSizeIndex_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sRackSizeIndex_strMinor, typeof(int));
			if (iMajor > _sRackSizeIndex_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sRackSizeIndex_MAJOR && iMinor > _sRackSizeIndex_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sRackSizeIndex_MAJOR)
			{
				//
				// Key = (string)info.GetValue("Key", typeof(string));  // removed in 2.0
				Count = (UInt32)info.GetValue("Count", typeof(UInt32));
				if (iMajor >= 2 && iMinor >= 0)
					State = (M_RackState)info.GetValue("State", typeof(M_RackState));
			}
			else
				++DrawingDocument._sBiggerMajorNumber;
		}
		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sRackSizeIndex_strMajor, _sRackSizeIndex_MAJOR);
			info.AddValue(_sRackSizeIndex_strMinor, _sRackSizeIndex_MINOR);

			//
			info.AddValue("Count", Count);
			// 2.0 Write empty string. If open this drawing in old versions it will not throw exception.
			info.AddValue("Key", string.Empty);
			info.AddValue("State", State);
		}
		//=============================================================================
		public virtual void OnDeserialization(object sender) { }

		#endregion

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new RackSizeIndex(this);
		}
	}

	[Serializable]
	public class DrawingDocument : BaseViewModel, ISerializable, IDeserializationCallback, IDataErrorInfo, IClonable
	{
		public static int _sBiggerMajorNumber = 0;
		public static int _sNewVersion_StreamRead = 0;
		public static bool _sDontSupportDocument = false;
		public static int _sStreamReadException = 0;
		public static DrawingControl _sDrawing = null;
		public static void ClearErrors()
		{
			DrawingDocument._sBiggerMajorNumber = 0;
			DrawingDocument._sNewVersion_StreamRead = 0;
			DrawingDocument._sDontSupportDocument = false;
			DrawingDocument._sStreamReadException = 0;
		}

		//
		public static string FILE_EXTENSION = ".rda";
		public static string FILE_FILTER = "RackDrawingApp drawings (.rda)|*.rda";

		// Customer info properties
		public static string PROPERTY_CUSTOMER_NAME = "CustomerName";
		public static string PROPERTY_CUSTOMER_CONTACT_NO = "CustomerContactNo";
		public static string PROPERTY_CUSTOMER_EMAIL = "CustomerEMail";
		public static string PROPERTY_CUSTOMER_RFQ = "CustomerRFQ";
		public static string PROPERTY_CUSTOMER_ADDRESS = "CustomerAddress";

		// If pallet type is Flush then margin between the same roated(horizontal\vertical) racks in
		// depth direction(Y-axis if racks are horizontal, X-axis if vertical) is RACK_FLUSH_PALLETS_BTB_DISTANCE.
		public static double RACK_FLUSH_PALLETS_BTB_DISTANCE = 60.0;
		// If pallet type if Overhnag, then margin between racks in depth direction is
		// margin = 2 * Overhang + RACK_OVERHANG_PALLETS_BTB_DISTANCE_ADDITIONALVALUE.
		public static double RACK_OVERHANG_PALLETS_BTB_DISTANCE_ADDITIONALVALUE = 50.0;
		// Margin between rack and block in both X and Y direcions.
		public static double RACK_TO_BLOCK_MARGIN = 100.0;
		// Margin between rack and wall in both X and Y directions
		public static double RACK_TO_WALL_MARGIN = 200.0;

		public DrawingDocument(IDisplayDialog displayDialog)
		{
			this.DisplayDialog = displayDialog;
			m_RacksColumnsList = RackLoadUtils.RacksColumnsList;
			m_BeamsList = RackLoadUtils.ReallyUsedBeamsList;

			m_Sheets.Clear();
			DrawingSheet newSheet = new DrawingSheet(this);
			m_Sheets.Add(newSheet);
			this._SetCurrentSheet(newSheet, false);

			//
			foreach (DrawingSheet sheet in m_Sheets)
			{
				if (sheet == null)
					continue;

				sheet._UpdateSheet();
			}

			// Serialization and Deserialization used to save DrawingDocument with DrawingSheets collection to file.
			// We dont save some properties and dont restore them while Deserialization.
			// But we need to restore State after Deserialization. Otherwise opened sheet will have DefaultState.
			m_States.Clear();
			this.State = _GetState();

			_InitPalletConfigCollView();
			_InitMHEConfigurationsList();
		}
		public DrawingDocument(List<RackColumn> racksColumnsList, List<RackBeam> beamsList, DrawingDocument_State state)
		{
			m_RacksColumnsList = racksColumnsList;
			m_BeamsList = beamsList;

			m_States.Clear();
			if (state != null)
				this._SetState(state);
			this.State = _GetState();

			_InitPalletConfigCollView();
		}

		#region Fields

		// Unique sizes of column on the drawing.
		// Used to give each column index depend on size - C1, C2, C3
		private List<ColumnSizeIndex> m_ColumnsUniqueSizes = new List<ColumnSizeIndex>();

		//
		// Unique sizes of racks on the drawing.
		// Sizes of the first rack in the row\column - M
		// all A-racks should be smaller by 80
		private Dictionary<int, RackSizeIndex> m_RacksUniqueSizes = new Dictionary<int, RackSizeIndex>();

		// Properties which are common for all racks in the document.
		// Cant place this properties to the Rack class as static properties.
		// Because if they are static and you change them, then you change them in the all state history too.
		//
		private static eBracingType m_RackBracingType = eBracingType.eGI;
		//
		private static RackAccessories m_RackAccessories = new RackAccessories();

		#endregion

		#region Events

		/// <summary>
		/// State property is changed
		/// </summary>
		public event EventHandler StateChanged;

		#endregion

		#region Properties

		//=============================================================================
		public IDisplayDialog DisplayDialog { get; set; }

		//=============================================================================
		// Property source rack for rack match properties command
		public Rack PropertySourceRack = null;


		//=============================================================================
		// If sheets count is less than 2 then need to diable close sheet button.
		public bool IsCloseSheetButtonEnabled
		{
			get
			{
				return !IsInCommand && m_Sheets.Count > 1;
			}
		}

		//=============================================================================
		private ObservableCollection<DrawingSheet> m_Sheets = new ObservableCollection<DrawingSheet>();
		public ObservableCollection<DrawingSheet> Sheets { get { return m_Sheets; } }
		//=============================================================================
		/// <summary>
		/// Zero based index of current sheet in the m_Sheets collection.
		/// </summary>
		protected int m_CurrentSheetIndex = -1;
		public DrawingSheet CurrentSheet
		{
			get
			{
				if (m_CurrentSheetIndex >= 0 && m_CurrentSheetIndex < m_Sheets.Count)
					return m_Sheets[m_CurrentSheetIndex];

				return null;
			}
			set
			{
				_SetCurrentSheet(value, true);
			}
		}

		//=============================================================================
		public string Path { get; set; }

		//=============================================================================
		public bool IsItNewDocument
		{
			get
			{
				return string.IsNullOrEmpty(Path);
			}
		}

		//=============================================================================
		public bool HasChanges
		{
			get
			{
				if (IsItNewDocument)
					return true;

				if (m_States != null && m_States.Count > 1)
					return true;

				return false;
			}
		}

		//=============================================================================
		/// <summary>
		/// Name with extension
		/// </summary>
		public string DisplayName
		{
			get
			{
				if (IsItNewDocument)
					return "New Document";
				else
				{
					try
					{
						// regex and cut only filename
						string strPattern = @"[^\\]+\.rda$";
						Regex regex = new Regex(strPattern);
						Match match = regex.Match(Path);

						return match.Value;
					}
					catch { }
				}

				return "DrawingDocument";
			}
		}
		//=============================================================================
		/// <summary>
		/// Name without ".rda" extension.
		/// </summary>
		public string NameWithoutExtension
		{
			get
			{
				string strDisplayName = DisplayName;
				if (!string.IsNullOrEmpty(strDisplayName))
					return strDisplayName.Replace(".rda", string.Empty);
				return strDisplayName;
			}
		}

		//=============================================================================
		/// <summary>
		/// List with rectangles which are copied
		/// </summary>
		public List<BaseRectangleGeometry> CopiedGeomList = new List<BaseRectangleGeometry>();

		//=============================================================================
		// for debug only, should be deleted in release
		public int StatesCount { get { return m_States.Count; } }
		//=============================================================================
		private bool m_bDontUpdateState = false;
		private bool m_bAddStateToHistory = true;
		private int m_CurrentStateIndex = -1;
		private List<DrawingDocument_State> m_States = new List<DrawingDocument_State>();
		public DrawingDocument_State State
		{
			get
			{
				if (m_CurrentStateIndex >= 0 && m_CurrentStateIndex < m_States.Count)
					return m_States[m_CurrentStateIndex];

				return null;
			}
			set
			{
				if (m_bAddStateToHistory)
				{
					//add new state after current
					if (m_CurrentStateIndex < 0)
					{
						m_States.Add(value);
						m_CurrentStateIndex = 0;
					}
					else if (m_CurrentStateIndex >= 0 && m_CurrentStateIndex < m_States.Count)
					{
						if (m_CurrentStateIndex < m_States.Count - 1)
							m_States.RemoveRange(m_CurrentStateIndex + 1, m_States.Count - 1 - m_CurrentStateIndex);

						m_States.Add(value);
						++m_CurrentStateIndex;
					}
				}

				// Clear rack change order
				m_RackChangeOrder = 0;
				if (!m_bDontUpdateState)
					_SetState(value);

				_OnStateChanged();
			}
		}

		//=============================================================================
		// Is undo command available.
		public bool CanUndo
		{
			get
			{
				if (m_CurrentStateIndex > 0)
					return true;

				return false;
			}
		}
		//=============================================================================
		// Is redo command available
		public bool CanRedo
		{
			get
			{
				if (m_CurrentStateIndex >= 0 && m_CurrentStateIndex < m_States.Count - 1)
					return true;

				return false;
			}
		}

		//=============================================================================
		// If true rack's advanced properties tab is displayed.
		private bool m_bShowAdvancedProperties = false;
		public bool ShowAdvancedProperties
		{
			get { return m_bShowAdvancedProperties; }
			set
			{
				Set_ShowAdvancedProperties(value, true);
			}
		}

		//=============================================================================
		public eBracingType Rack_BracingType
		{
			get { return m_RackBracingType; }
			set { m_RackBracingType = value; }
		}
		//=============================================================================
		public RackAccessories Rack_Accessories
		{
			get { return m_RackAccessories; }
			set { m_RackAccessories = value; }
		}

		//=============================================================================
		private string m_CustomerName = string.Empty;
		public string CustomerName
		{
			get { return m_CustomerName; }
			set
			{
				if (value != m_CustomerName)
				{
					m_CustomerName = value;
					if (m_CustomerName == null)
						m_CustomerName = string.Empty;

					NotifyPropertyChanged(() => CustomerName);
				}
			}
		}
		//=============================================================================
		private string m_CustomerContactNo = string.Empty;
		public string CustomerContactNo
		{
			get { return m_CustomerContactNo; }
			set
			{
				if (value != m_CustomerContactNo)
				{
					m_CustomerContactNo = value;
					if (m_CustomerContactNo == null)
						m_CustomerContactNo = string.Empty;

					NotifyPropertyChanged(() => CustomerContactNo);
				}
			}
		}
		//=============================================================================
		private string m_CustomerEMail = string.Empty;
		public string CustomerEMail
		{
			get { return m_CustomerEMail; }
			set
			{
				if (value != m_CustomerEMail)
				{
					m_CustomerEMail = value;
					if (m_CustomerEMail == null)
						m_CustomerEMail = string.Empty;

					NotifyPropertyChanged(() => CustomerEMail);
				}
			}
		}
		//=============================================================================
		/// <summary>
		/// Customer ENQ. No.
		/// </summary>
		private string m_CustomerENQ = string.Empty;
		public string CustomerENQ
		{
			get { return m_CustomerENQ; }
			set
			{
				if (value != m_CustomerENQ)
				{
					m_CustomerENQ = value;
					if (m_CustomerENQ == null)
						m_CustomerENQ = string.Empty;

					NotifyPropertyChanged(() => CustomerENQ);
				}
			}
		}
		//=============================================================================
		private string m_CustomerAddress = string.Empty;
		public string CustomerAddress
		{
			get { return m_CustomerAddress; }
			set
			{
				if (value != m_CustomerAddress)
				{
					m_CustomerAddress = value;
					if (m_CustomerAddress == null)
						m_CustomerAddress = string.Empty;

					NotifyPropertyChanged(() => CustomerAddress);
				}
			}
		}
		//=============================================================================
		private string m_CustomerSite = string.Empty;
		public string CustomerSite
		{
			get { return m_CustomerSite; }
			set
			{
				if(value != m_CustomerSite)
				{
					m_CustomerSite = value;
					if (m_CustomerSite == null)
						m_CustomerSite = string.Empty;

					NotifyPropertyChanged(() => CustomerSite);
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// Revision of the document.
		/// It is readonly number, which is displayed in CustomerInfo dialog.
		/// User cant edit it, it is increased only through save the document with "Increase revision" button.
		/// </summary>
		private uint m_DocumentRevision = 1;
		public uint DocumentRevision { get { return m_DocumentRevision; } }

		//=============================================================================
		/// <summary>
		/// Pallets can be flush or overhang.
		/// If pallet is overhang then its depth is greater than rack's depth. It drives rack margin(RacksPalletsOverhangValue) in depth direction.
		/// If pallet is flush then depth are equal.
		/// Applies to all racks in the document.
		/// </summary>
		private ePalletType m_RacksPalletType = ePalletType.eOverhang;
		public ePalletType RacksPalletType
		{
			get { return m_RacksPalletType; }
			set
			{
				if(value != m_RacksPalletType)
					m_RacksPalletType = value;
				NotifyPropertyChanged(() => RacksPalletType);
			}
		}
		//=============================================================================
		/// <summary>
		/// Drives the pallet depth relative to the rack depth. Also it drives rack's margin in depth direction.
		/// Pallet depth = rack depth + 2 * RacksPalletsOverhangValue.
		/// Applied only if DrawingDocument.RacksPalletType = ePalletType.Overhang.
		/// Applied to all racks in the document.
		/// </summary>
		private double m_RacksPalletsOverhangValue = 100.0;
		public double RacksPalletsOverhangValue
		{
			get { return m_RacksPalletsOverhangValue; }
			set
			{
				if (Utils.FGT(value, 0.0) && Utils.FNE(value, m_RacksPalletsOverhangValue))
					m_RacksPalletsOverhangValue = value;
				NotifyPropertyChanged(() => RacksPalletsOverhangValue);
			}
		}

		//=============================================================================
		private string m_Currency = "INR";
		public string Currency
		{
			get { return m_Currency; }
			set
			{
				if (value != m_Currency)
					m_Currency = value;
				NotifyPropertyChanged(() => Currency);
			}
		}
		//=============================================================================
		private double m_Rate = 1.0;
		public double Rate
		{
			get { return m_Rate; }
			set
			{
				if (value != m_Rate)
					m_Rate = value;
				NotifyPropertyChanged(() => Rate);
			}
		}
		//=============================================================================
		private double m_Discount = 0.0;
		public double Discount
		{
			get { return m_Discount; }
			set
			{
				if (value != m_Discount)
					m_Discount = value;
				NotifyPropertyChanged(() => Discount);
			}
		}

		//=============================================================================
		// List with MHE configurations.
		// if MHE configuration is enabled then it can drive aisle space min length\width, max pallet load, etc.
		private ObservableCollection<MHEConfiguration> m_MHEConfigurationsColl = new ObservableCollection<MHEConfiguration>();
		public ObservableCollection<MHEConfiguration> MHEConfigurationsColl { get { return m_MHEConfigurationsColl; } }
		//=============================================================================
		/// <summary>
		/// List of MHEConfiguration from m_MHEConfigurationsList, which are enabled.
		/// </summary>
		public List<MHEConfiguration> EnabledMHEConfigsList
		{
			get
			{
				List<MHEConfiguration> result = new List<MHEConfiguration>();

				if (m_MHEConfigurationsColl == null)
					return result;

				foreach(MHEConfiguration mheConfig in m_MHEConfigurationsColl)
				{
					if (mheConfig == null)
						continue;

					if (mheConfig.IsEnabled)
						result.Add(mheConfig);
				}

				return result;
			}
		}

		//=============================================================================
		// Collection with PalletConfigurations, which can be used in the racks.
		private ObservableCollection<PalletConfiguration> m_PalletConfigurationCollection = new ObservableCollection<PalletConfiguration>();
		public ObservableCollection<PalletConfiguration> PalletConfigurationCollection
		{
			get { return m_PalletConfigurationCollection; }
		}

		//=============================================================================
		// View of m_PalletConfigurationCollection.
		// Allow to add collection sorting.
		private ICollectionView m_PalletConfigurationCollView = null;
		public ICollectionView PalletConfigurationCollView { get { return m_PalletConfigurationCollView; } }

		//=============================================================================
		// The lowest capacity value from enabled MHEConfigurations.
		// Can be infinity.
		public double PalletTruckMaxCapacity
		{
			get
			{
				List<MHEConfiguration> enabledMHEConfigsList = EnabledMHEConfigsList;

				bool isInit = false;
				double lowestCapacity = 0.0;
				foreach(MHEConfiguration mheConfig in enabledMHEConfigsList)
				{
					if (mheConfig == null)
						continue;

					if (double.IsNaN(mheConfig.Capacity) || double.IsInfinity(mheConfig.Capacity))
						continue;

					if (isInit)
					{
						if (Utils.FLT(mheConfig.Capacity, lowestCapacity))
							lowestCapacity = mheConfig.Capacity;
					}
					else
					{
						lowestCapacity = mheConfig.Capacity;
						isInit = true;
					}
				}

				if (isInit)
					return lowestCapacity;

				return double.PositiveInfinity;
			}
		}

		//=============================================================================
		// The lowest loading height value from enabled MHEConfigurations.
		// Can be infinity.
		// Max loading height of the rack which is the top of the topmost beam.
		// This value drives the top of the topmost beam height and total rack height.
		public double MaxLoadingHeight
		{
			get
			{
				List<MHEConfiguration> enabledMHEConfigsList = EnabledMHEConfigsList;

				bool isInit = false;
				double lowestLoadingHeight = 0.0;
				foreach (MHEConfiguration mheConfig in enabledMHEConfigsList)
				{
					if (mheConfig == null)
						continue;

					if (double.IsNaN(mheConfig.MaxLoadingHeight) || double.IsInfinity(mheConfig.MaxLoadingHeight))
						continue;

					if (isInit)
					{
						if (Utils.FLT(mheConfig.MaxLoadingHeight, lowestLoadingHeight))
							lowestLoadingHeight = mheConfig.MaxLoadingHeight;
					}
					else
					{
						lowestLoadingHeight = mheConfig.MaxLoadingHeight;
						isInit = true;
					}
				}

				if (isInit)
				{
					lowestLoadingHeight -= Rack.RACK_LOADING_HEIGHT_GAP;
					if (Utils.FLT(lowestLoadingHeight, 200.0))
						lowestLoadingHeight = 0.0;
				}

				if (isInit)
					return lowestLoadingHeight;

				return double.PositiveInfinity;
			}
		}

		//=============================================================================
		// The biggest picking aisle space value from enabled MHEConfigurations.
		// Can be infinity.
		//
		// If any parallel rack is adjusted to the aisle space then use PickingAisleWidth as
		// min aisle space length\width.
		public double PickingAisleWidth
		{
			get
			{
				List<MHEConfiguration> enabledMHEConfigsList = EnabledMHEConfigsList;

				bool isInit = false;
				double biggestPickingWidth = 0.0;
				foreach (MHEConfiguration mheConfig in enabledMHEConfigsList)
				{
					if (mheConfig == null)
						continue;

					if (double.IsNaN(mheConfig.PickingAisleWidth) || double.IsInfinity(mheConfig.PickingAisleWidth))
						continue;

					if (isInit)
					{
						if (Utils.FGT(mheConfig.PickingAisleWidth, biggestPickingWidth))
							biggestPickingWidth = mheConfig.PickingAisleWidth;
					}
					else
					{
						biggestPickingWidth = mheConfig.PickingAisleWidth;
						isInit = true;
					}
				}

				if (isInit)
					return biggestPickingWidth;

				return double.PositiveInfinity;
			}
		}
		//=============================================================================
		// The biggest cross aisle space value from enabled MHEConfigurations.
		// Can be infinity.
		//
		// If any perpendicular rack is adjusted to the aisle space then use CrossAisleWidth as
		// min aisle space length\width.
		public double CrossAisleWidth
		{
			get
			{
				List<MHEConfiguration> enabledMHEConfigsList = EnabledMHEConfigsList;

				bool isInit = false;
				double biggestCrossWidth = 0.0;
				foreach (MHEConfiguration mheConfig in enabledMHEConfigsList)
				{
					if (mheConfig == null)
						continue;

					if (double.IsNaN(mheConfig.CrossAisleWidth) || double.IsInfinity(mheConfig.CrossAisleWidth))
						continue;

					if (isInit)
					{
						if (Utils.FGT(mheConfig.CrossAisleWidth, biggestCrossWidth))
							biggestCrossWidth = mheConfig.CrossAisleWidth;
					}
					else
					{
						biggestCrossWidth = mheConfig.CrossAisleWidth;
						isInit = true;
					}
				}

				if (isInit)
					return biggestCrossWidth;

				return double.PositiveInfinity;
			}
		}
		//=============================================================================
		// The biggest end aisle space value from enabled MHEConfigurations.
		// Can be infinity.
		//
		// If any block or wall is adjust to the aisle space then use EndAisleWidth as
		// min aisle space length\width.
		public double EndAisleWidth
		{
			get
			{
				List<MHEConfiguration> enabledMHEConfigsList = EnabledMHEConfigsList;

				bool isInit = false;
				double biggestEndWidth = 0.0;
				foreach (MHEConfiguration mheConfig in enabledMHEConfigsList)
				{
					if (mheConfig == null)
						continue;

					if (double.IsNaN(mheConfig.EndAisleWidth) || double.IsInfinity(mheConfig.EndAisleWidth))
						continue;

					if (isInit)
					{
						if (Utils.FGT(mheConfig.EndAisleWidth, biggestEndWidth))
							biggestEndWidth = mheConfig.EndAisleWidth;
					}
					else
					{
						biggestEndWidth = mheConfig.EndAisleWidth;
						isInit = true;
					}
				}

				if (isInit)
					return biggestEndWidth;

				return double.PositiveInfinity;
			}
		}
		//=============================================================================
		/// <summary>
		/// The biggest OverallHeightLowered value from enabled MHE configurations.
		/// 
		/// Rack underpass height should be greater or equal than (OverallHeightLowered + Rack.RACK_UNDERPASS_GAP).
		/// </summary>
		public double OverallHeightLowered
		{
			get
			{
				List<MHEConfiguration> enabledMHEConfigsList = EnabledMHEConfigsList;

				bool isInit = false;
				double biggestOverallHeight = 0.0;
				foreach (MHEConfiguration mheConfig in enabledMHEConfigsList)
				{
					if (mheConfig == null)
						continue;

					if (double.IsNaN(mheConfig.OverallHeightLowered) || double.IsInfinity(mheConfig.OverallHeightLowered))
						continue;

					if (isInit)
					{
						if (Utils.FGT(mheConfig.OverallHeightLowered, biggestOverallHeight))
							biggestOverallHeight = mheConfig.OverallHeightLowered;
					}
					else
					{
						biggestOverallHeight = mheConfig.OverallHeightLowered;
						isInit = true;
					}
				}

				if (isInit)
					return biggestOverallHeight + Rack.RACK_UNDERPASS_GAP;

				return 0.0;
			}
		}

		//=============================================================================
		/// <summary>
		/// if true then command is running and another one command cant start.
		/// </summary>
		private bool m_bIsInCommand = false;
		public bool IsInCommand
		{
			get { return m_bIsInCommand; }
			set
			{
				if(value != m_bIsInCommand)
				{
					m_bIsInCommand = value;
					NotifyPropertyChanged(() => IsInCommand);
					NotifyPropertyChanged(() => IsCloseSheetButtonEnabled);
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// List with RackColumn which contains column data - max beam length, USL, PalletsTypes.
		/// </summary>
		private List<RackColumn> m_RacksColumnsList = null;
		public List<RackColumn> RacksColumnsList { get { return m_RacksColumnsList; } }

		//=============================================================================
		/// <summary>
		/// All available beams in this document.
		/// </summary>
		private List<RackBeam> m_BeamsList = null;
		public List<RackBeam> BeamsList { get { return m_BeamsList; } }

		//=============================================================================
		/// <summary>
		/// Index of changed rack during the command.
		/// If rack is changed then it receive this index and increase it.
		/// It is used for rack index calculation during the command.
		/// </summary>
		public long m_RackChangeOrder = 0;



		//=============================================================================
		/// <summary>
		/// Racks size index view models. Sometimes need to check actual state of RacksUniqueSizes.
		/// It is used only for DEBUG, dont do it in RELEASE.
		/// </summary>
		private ObservableCollection<RackSizeIndexVM> m_RacksSizesCollection = new ObservableCollection<RackSizeIndexVM>();
		public ObservableCollection<RackSizeIndexVM> RacksSizesCollection { get { return m_RacksSizesCollection; } }

		//=============================================================================
		// timer for clear error string
		private DispatcherTimer m_ErrorStringTimer = null;
		//=============================================================================
		/// <summary>
		/// Error which is displayed in the application dialog.
		/// </summary>
		private string m_DocumentError = string.Empty;
		public string DocumentError
		{
			get { return m_DocumentError; }
			set
			{
				if (m_DocumentError != value)
				{
					m_DocumentError = value;
					NotifyPropertyChanged(() => DocumentError);
					_StartErrorStringTimer();
				}
			}
		}

		/// <summary>
		/// Returns true if this document contains sheet with tie beam errors.
		/// </summary>
		public bool ContainsTieBeamsErrors
		{
			get
			{
				if (m_Sheets != null)
				{
					foreach(DrawingSheet sheet in m_Sheets)
					{
						if (sheet == null)
							continue;

						if (sheet.ContainsTieBeamsErrors)
							return true;
					}
				}

				return false;
			}
		}

		//============================================================================ Pdf printing settings
		private bool m_IsPrintRackElevations;
		/// <summary>
		/// Configuration to print in pdf rack elevations
		/// </summary>
		public bool IsPrintRackElevations
		{
			get => m_IsPrintRackElevations;
			set
			{
				m_IsPrintRackElevations = value;
				NotifyPropertyChanged(() => IsPrintRackElevations);
			}
		}

		private bool m_IsPrintAllRackElevationsInSinglePage;
		/// <summary>
		/// Configuration to pull all racks elevations to single page
		/// </summary>
		public bool IsPrintAllRackElevationsInSinglePage
		{
			get => m_IsPrintAllRackElevationsInSinglePage;
			set
			{
				m_IsPrintAllRackElevationsInSinglePage = value;
				NotifyPropertyChanged(() => IsPrintAllRackElevationsInSinglePage);
			}
		}

		private bool m_IsFitRackGroupToSamePage;
		/// <summary>
		///	Configuration to combine racks with same index in single page
		/// </summary>
		public bool IsFitRackGroupToSamePage
		{
			get => m_IsFitRackGroupToSamePage;
			set
			{
				m_IsFitRackGroupToSamePage = value;
				NotifyPropertyChanged(() => IsFitRackGroupToSamePage);
			}
		}

		#endregion

		#region Functions

		//=============================================================================
		public void Undo()
		{
			if (!CanUndo)
				return;

			--m_CurrentStateIndex;

			m_bAddStateToHistory = false;
			_SetState(State);
			m_bAddStateToHistory = true;

			_OnStateChanged();
		}
		//=============================================================================
		public void Redo()
		{
			if (!CanRedo)
				return;

			++m_CurrentStateIndex;

			m_bAddStateToHistory = false;
			_SetState(State);
			m_bAddStateToHistory = true;

			_OnStateChanged();
		}
		//=============================================================================
		public void SetTheLastState()
		{
			if (m_States.Count == 0)
				return;

			// User select any geometry and moves it by grip point. While movins user changes camera scale and offset.
			// User pushes "Esc" button, geometry move is canceled, but camera scale and offset are canceled too.
			// Camera scale and offset are stored inside DrawingSheet. When SetTheLastState() executes they are restored to old values.
			//
			// Need to keep new values for camera scale and offset after SetTheLastState().
			Guid oldSelectedSheetGuid = Guid.Empty;
			DrawingSheet olsSelectedSheet = this.CurrentSheet;
			bool is_UnitsPerCameraPixel_Init = false;
			double unitsPerCameraPixel = 1.0;
			Vector cameraOffset = new Vector(0.0, 0.0);
			Vector cameraTemporaryOffset = new Vector(0.0, 0.0);
			if(olsSelectedSheet != null)
			{
				oldSelectedSheetGuid = olsSelectedSheet.GUID;
				is_UnitsPerCameraPixel_Init = olsSelectedSheet.Is_UnitsPerCameraPixel_Init;
				unitsPerCameraPixel = olsSelectedSheet.UnitsPerCameraPixel;
				cameraOffset = olsSelectedSheet.CameraOffset;
				cameraTemporaryOffset = olsSelectedSheet.TemporaryCameraOffset;
			}

			DrawingDocument_State state = m_States[m_States.Count - 1];
			if (state == null)
				return;

			m_bAddStateToHistory = false;
			State = state;
			m_bAddStateToHistory = true;

			// If it the same sheet then need to restore camera settings
			DrawingSheet newSelectedSheet = this.CurrentSheet;
			if (newSelectedSheet != null && newSelectedSheet.GUID == oldSelectedSheetGuid)
			{
				newSelectedSheet.Is_UnitsPerCameraPixel_Init = is_UnitsPerCameraPixel_Init;
				newSelectedSheet.UnitsPerCameraPixel = unitsPerCameraPixel;
				newSelectedSheet.CameraOffset = cameraOffset;
				newSelectedSheet.TemporaryCameraOffset = cameraTemporaryOffset;

				// NotifyPropertyChanged() will not redraw DrawingControl, because sheet is the same.
				// Need manually invalidate DrawingControl.
				//NotifyPropertyChanged(() => CurrentSheet);
				if (DrawingDocument._sDrawing != null)
					DrawingDocument._sDrawing.InvalidateVisual();
			}

			m_CurrentStateIndex = m_States.IndexOf(state);
			NotifyPropertyChanged(() => State);
		}
		//=============================================================================
		public void RemoveAllStatesExceptCurrent()
		{
			m_States.RemoveRange(0, m_States.Count - 1);
			m_CurrentStateIndex = m_States.Count - 1;
			//
			m_bAddStateToHistory = false;
			State = m_States[0];
			m_bAddStateToHistory = true;
		}


		//=============================================================================
		/// <summary>
		/// Add sheet to the sheets list
		/// </summary>
		public void AddSheet(DrawingSheet ds, bool bSetAsCurrent)
		{
			if (ds == null)
				return;

			m_Sheets.Add(ds);
			if (bSetAsCurrent)
				this._SetCurrentSheet(ds, false);
		}
		//=============================================================================
		public void RemoveSheet(DrawingSheet ds)
		{
			if (ds == null)
				return;

			if (!m_Sheets.Contains(ds))
				return;

			int iRemoveSheetIndex = m_Sheets.IndexOf(ds);
			//
			ds.Show(false);
			// Remove all geometry from the sheet.
			// It is need for remove racks and columns indexes.
			List<BaseRectangleGeometry> geomForDeleteList = new List<BaseRectangleGeometry>();
			if (ds.Rectangles != null)
				geomForDeleteList.AddRange(ds.Rectangles);
			ds.DeleteGeometry(geomForDeleteList, false, false);
			// Remove sheet
			m_Sheets.Remove(ds);
			m_CurrentSheetIndex = -1;
			//
			DrawingSheet currSheet = null;
			if (iRemoveSheetIndex >= 0 && iRemoveSheetIndex < m_Sheets.Count)
				currSheet = m_Sheets[iRemoveSheetIndex];
			else
			{
				if (m_Sheets.Count > 0)
					currSheet = m_Sheets[0];
			}
			this._SetCurrentSheet(currSheet, false);
		}


		//=============================================================================
		public bool Save(string strNewPath, bool bIncreaseDocumentRevision = false)
		{
			string strPath = strNewPath;
			if (string.IsNullOrEmpty(strPath))
				strPath = Path;

			if (string.IsNullOrEmpty(strPath))
				return false;

			//
			if (strPath != Path)
				Path = strPath;

			// hide show advanced properties tab
			this.Set_ShowAdvancedProperties(false, false);

			// before save document remove all non initialized and incorrect geometry
			foreach (DrawingSheet sheet in this.Sheets)
			{
				if (sheet == null)
					continue;

				// delete non initialized geometry
				sheet.DeleteNonInitializedGeometry(true, false);

				// delete incorrect geometry
				List<BaseRectangleGeometry> incorrectGeometryList = new List<BaseRectangleGeometry>();
				if (!sheet.IsLayoutCorrect(out incorrectGeometryList))
					sheet.DeleteGeometry(incorrectGeometryList, true, true);
			}

			FileStream fs = new FileStream(Path, FileMode.OpenOrCreate);
			if (fs == null)
				return false;

			// increase document revision
			if(bIncreaseDocumentRevision)
				++m_DocumentRevision;

			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(fs, this);
			//
			fs.Close();
			fs.Dispose();

			// remove all state except current, make undo command unavailable
			if (m_States.Count > 0)
				RemoveAllStatesExceptCurrent();

			//
			foreach (DrawingSheet _sheet  in m_Sheets)
			{
				if (_sheet == null)
					continue;

				_sheet.AfterSave();
			}

			//
			this.IsInCommand = false;

			return true;
		}

		//=============================================================================
		/// <summary>
		/// Export document to TXT file.
		/// </summary>
		public bool ExportToTxt(string strFilePath)
		{
			if (string.IsNullOrEmpty(strFilePath))
				return false;

			StreamWriter fs = new StreamWriter(strFilePath, false);
			if (fs == null)
				return false;

			// date and customer info
			DateTime thisDate = DateTime.Today;
			fs.WriteLine("Date_" + thisDate.ToString("dd MMMM yyyy"));
			fs.WriteLine("Customer name: " + this.CustomerName);
			fs.WriteLine("Customer address: " + this.CustomerAddress);
			fs.WriteLine("Customer contact no.: " + this.CustomerContactNo);
			fs.WriteLine("Customer email: " + this.CustomerEMail);
			fs.WriteLine("Customer ENQ number: " + this.CustomerENQ);
			fs.WriteLine("Customer site: " + this.CustomerSite);
			fs.WriteLine("Rev.No._000");
			fs.WriteLine("Currency_\"" + this.Currency + "\"");
			fs.WriteLine("Rate_" + this.Rate.ToString(".00").Replace(',', '.'));
			fs.WriteLine("Discount_" + this.Discount.ToString(".00").Replace(',', '.'));
			fs.WriteLine("**");


			// Sheets
			fs.WriteLine("Layouts");
			//
			foreach (DrawingSheet _sheet in Sheets)
			{
				if (_sheet == null)
					continue;

				fs.WriteLine("**");
				// name
				fs.WriteLine("Layout_" + _sheet.Name);
				// length and width
				fs.WriteLine(_sheet.Length.ToString() + "," + _sheet.Width.ToString());

				// export racks
				foreach(List<Rack> _RowOrColumn in _sheet.RacksGroups)
				{
					if (_RowOrColumn == null)
						continue;

					foreach(Rack _rack in _RowOrColumn)
					{
						if (_rack == null)
							continue;

						string strBottomOffset = "ST";
						if (_rack.IsUnderpassAvailable)
							strBottomOffset = "UP";
						else if (_rack.IsMaterialOnGround)
							strBottomOffset = "GR";

						//
						double rackLength = _rack.Length_X;
						double rackWidth = _rack.Length_Y;
						if(!_rack.IsHorizontal)
						{
							rackLength = _rack.Length_Y;
							rackWidth = _rack.Length_X;
						}

						//
						string strRotation = "H";
						if (!_rack.IsHorizontal)
							strRotation = "V";

						//
						fs.WriteLine(
							"Rack_"
							+ _rack.Text
							+ ","
							+ _rack.TopLeft_GlobalPoint.X.ToString("0.")
							+ ","
							+ _rack.TopLeft_GlobalPoint.Y.ToString("0.")
							+ ","
							+ rackLength.ToString("0.")
							+ ","
							+ rackWidth.ToString("0.")
							+ ","
							+ _rack.Length_Z.ToString("0.")
							+ ","
							+ strBottomOffset
							+ ","
							+ strRotation
							+ ","
							+ _rack.FillColor.R
							+ ","
							+ _rack.FillColor.G
							+ ","
							+ _rack.FillColor.B
							);
					}
				}

				//
				List<Wall> wallsList = new List<Wall>();
				List<Block> blocksList = new List<Block>();
				List<Column> columnsList = new List<Column>();
				List<AisleSpace> aisleSpacesList = new List<AisleSpace>();
				List<Shutter> shuttersList = new List<Shutter>();
				foreach(BaseRectangleGeometry rect in _sheet.Rectangles)
				{
					if (rect == null)
						continue;

					Block b = rect as Block;
					if(b != null)
					{
						blocksList.Add(b);
						continue;
					}

					Column c = rect as Column;
					if(c != null)
					{
						columnsList.Add(c);
						continue;
					}

					AisleSpace _as = rect as AisleSpace;
					if(_as != null)
					{
						aisleSpacesList.Add(_as);
						continue;
					}

					Shutter shutter = rect as Shutter;
					if(shutter != null)
					{
						shuttersList.Add(shutter);
						continue;
					}

					Wall wall = rect as Wall;
					if(wall != null)
					{
						wallsList.Add(wall);
						continue;
					}
				}
				// other rects
				List<BaseRectangleGeometry> otherGeom = new List<BaseRectangleGeometry>();
				otherGeom.AddRange(wallsList);
				otherGeom.AddRange(blocksList);
				otherGeom.AddRange(columnsList);
				otherGeom.AddRange(aisleSpacesList);
				otherGeom.AddRange(shuttersList);
				foreach(BaseRectangleGeometry _rect in otherGeom)
				{
					if (_rect == null)
						continue;

					string strRectType = string.Empty;
					if (_rect is Block)
						strRectType = "Block_";
					else if (_rect is Column)
						strRectType = "Column_";
					else if (_rect is Wall)
						strRectType = "Wall";
					else if (_rect is Shutter)
						strRectType = "Shutter";
					else if (_rect is AisleSpace)
						strRectType = "AisleSpace";

					string strRectName = string.Empty;
					if (_rect is Block)
						strRectName = _rect.Name;
					else if (_rect is Column)
						strRectName = _rect.Text;

					fs.WriteLine(
							strRectType
							+ strRectName
							+ ","
							+ _rect.TopLeft_GlobalPoint.X.ToString("0.")
							+ ","
							+ _rect.TopLeft_GlobalPoint.Y.ToString("0.")
							+ ","
							+ _rect.Length_X.ToString("0.")
							+ ","
							+ _rect.Length_Y.ToString("0.")
							+ ","
							+ _rect.Length_Z.ToString("0.")
							);
				}

				// export tie beams
				foreach(TieBeam tb in _sheet.TieBeamsList)
				{
					if (tb == null)
						continue;

					fs.WriteLine(
							"Tie_Beam"
							+ ","
							+ tb.TopLeft_GlobalPoint.X.ToString("0.")
							+ ","
							+ tb.TopLeft_GlobalPoint.Y.ToString("0.")
							+ ","
							+ tb.BottomRight_GlobalPoint.X.ToString("0.")
							+ ","
							+ tb.BottomRight_GlobalPoint.Y.ToString("0.")
							);
				}

				fs.WriteLine("**");


				// Level details
				fs.WriteLine("Level details");

				List<int> exportedIndexesList = new List<int>();
				// export racks info
				foreach (List<Rack> _RowOrColumn in _sheet.RacksGroups)
				{
					if (_RowOrColumn == null)
						continue;

					if (_RowOrColumn.Count == 0)
						continue;

					foreach (Rack _rack in _RowOrColumn)
					{
						if (_rack == null)
							continue;

						if (exportedIndexesList.Contains(_rack.SizeIndex))
							continue;

						exportedIndexesList.Add(_rack.SizeIndex);

						int iBottomLevelOffset = 0;
						if (_rack.IsUnderpassAvailable)
							iBottomLevelOffset = (int)_rack.Underpass;
						else if (_rack.IsMaterialOnGround)
						{
							//iBottomLevelOffset = (int)_rack.MaterialHeightOnGround;
							iBottomLevelOffset = 0;
						}
						else
						{
							iBottomLevelOffset = (int)Rack.sFirstLevelOffset;
							if (_rack.Levels != null)
							{
								RackLevel firstLevel = _rack.Levels.FirstOrDefault(level => level != null && level.Index == 1);
								if (firstLevel != null && firstLevel.Beam != null)
								{
									iBottomLevelOffset -= Utils.GetWholeNumber(firstLevel.Beam.Height);
									if (iBottomLevelOffset < 0)
										iBottomLevelOffset = 0;
								}
							}
						}

						int iLastLevelHeight = 0;
						if(_rack.Levels != null)
						{
							RackLevel _lastLevel = _rack.Levels.LastOrDefault();
							if (_lastLevel != null)
								iLastLevelHeight = (int)_lastLevel.LevelHeight;
						}

						string strBracing = string.Empty;
						if (eColumnBracingType.eNormalBracing == _rack.Bracing)
							strBracing = "NBr";
						else if (eColumnBracingType.eXBracing == _rack.Bracing)
							strBracing = "NXBr_" + _rack.X_Bracings_Count.ToString();
						else if (eColumnBracingType.eNormalBracingWithStiffener == _rack.Bracing)
							strBracing = "SBr_" + _rack.StiffenersCount.ToString();
						else if (eColumnBracingType.eXBracingWithStiffener == _rack.Bracing)
							strBracing = "SXBr_" + _rack.X_Bracings_Count.ToString() + "_" + _rack.StiffenersCount.ToString();

						string strColumnName = string.Empty;
						if (_rack.Column != null)
							strColumnName = _rack.Column.DisplayName;

						UInt32 _SplitColumn_FirstPart = 0;
						if (_rack.SplitColumn)
							_SplitColumn_FirstPart = _rack.Column_FirstPartLength;

						string strAccessories = string.Empty;
						if(_rack.Accessories != null)
						{
							if (_rack.Accessories.UprightGuard)
								strAccessories += "1";
							else
								strAccessories += "0";

							strAccessories += ",";
							if (_rack.Accessories.RowGuard)
							{
								if(_rack.Accessories.IsHeavyDutyEnabled)
									strAccessories += "HD";
								else
									strAccessories += "1";
							}
							else
								strAccessories += "0";

							strAccessories += ",";
							if (_rack.Accessories.Signages)
							{
								if (_rack.Accessories.IsMenaEnabled)
									strAccessories += "mena";
								if (_rack.Accessories.IsSafetyPrecautionsEnabled && _rack.Accessories.IsSafeWorkingLoadsEnabled)
									strAccessories += "SP_" + _rack.Accessories.SafetyPrecautionsQuantity.ToString() + " SW_" + _rack.Accessories.SafeWorkingLoadsQuantity.ToString();
								else if (_rack.Accessories.IsSafetyPrecautionsEnabled)
									strAccessories += "SP_" + _rack.Accessories.SafetyPrecautionsQuantity.ToString();
								else if(_rack.Accessories.IsSafeWorkingLoadsEnabled)
									strAccessories += "SW_" + _rack.Accessories.SafeWorkingLoadsQuantity.ToString();
							}
							else
								strAccessories += "0";

							strAccessories += ",";
							if (_rack.Accessories.IsMeshCladdingEnabled)
								strAccessories += _rack.Accessories.MeshHeight.ToString();
							else
								strAccessories += "0";
						}

						double columnLength = 0.0;
						double columnDepth = 0.0;
						double columnThickness = 0.0;
						if(_rack.Column != null)
						{
							columnLength = _rack.Column.Length;
							columnDepth = _rack.Column.Depth;
							columnThickness = _rack.Column.Thickness;
						}
						//
						fs.WriteLine(
							_rack.Text
							+ ","
							+ _rack.NumberOfLevels_WithoutGround.ToString()
							+ ","
							+ iBottomLevelOffset.ToString()
							+ ","
							+ iLastLevelHeight.ToString()
							+ ","
							+ _rack.ClearAvailableHeight.ToString()
							+ ","
							+ strBracing
							+ ","
							+ strColumnName
							+ ","
							+ columnLength.ToString()
							+ ","
							+ columnDepth.ToString()
							+ ","
							+ columnThickness.ToString().Replace(',', '.')
							+ ","
							+ _SplitColumn_FirstPart.ToString()
							+ ","
							+ _rack.PalletOverhangValue.ToString("0.")
							+ ","
							+ strAccessories
							);

						// export levels
						if (_rack.Levels == null)
							continue;

						foreach(RackLevel _level in _rack.Levels)
						{
							if (_level == null)
								continue;

							string strLevelInfo = "Level_";
							if (_level.Index == 0)
								strLevelInfo += "GR";
							else
								strLevelInfo += _level.Index.ToString();

							string strBeamInfo = string.Empty;
							if (_level.Beam != null)
							{
								strBeamInfo += _level.Beam.Height.ToString();
								strBeamInfo += ",";
								strBeamInfo += _level.Beam.Depth.ToString();
								strBeamInfo += ",";
								strBeamInfo += _level.Beam.Thickness.ToString();
							}
							else
								strBeamInfo = "0,0,0";

							strLevelInfo += ",";
							strLevelInfo += strBeamInfo;
							strLevelInfo += ",";
							if (_level.IsItLastLevel)
								strLevelInfo += _level.TheBiggestPalletHeightWithRiser.ToString();
							else
								strLevelInfo += _level.LevelHeight.ToString();
							strLevelInfo += ",";
							if (_rack.ShowPallet)
								strLevelInfo += "0";
							else
								strLevelInfo += _level.LevelLoad.ToString();

							if (_level.Accessories != null)
							{
								strLevelInfo += ",";
								if (_level.Accessories.PalletStopper)
									strLevelInfo += "1";
								else
									strLevelInfo += "0";

								strLevelInfo += ",";
								if (_level.Accessories.ForkEntryBar)
									strLevelInfo += "1";
								else
									strLevelInfo += "0";

								strLevelInfo += ",";
								if (_level.Accessories.PalletSupportBar)
									strLevelInfo += "1";
								else
									strLevelInfo += "0";

								strLevelInfo += ",";
								if(!_level.Accessories.GuidedTypePalletSupport)
									strLevelInfo += "0";
								else if (_level.Accessories.GuidedTypePalletSupport && !_level.Accessories.GuidedTypePalletSupport_WithStopper && !_level.Accessories.GuidedTypePalletSupport_WithPSB)
									strLevelInfo += "1";
								else if (_level.Accessories.GuidedTypePalletSupport && _level.Accessories.GuidedTypePalletSupport_WithStopper && !_level.Accessories.GuidedTypePalletSupport_WithPSB)
									strLevelInfo += "2";
								else if (_level.Accessories.GuidedTypePalletSupport && !_level.Accessories.GuidedTypePalletSupport_WithStopper && _level.Accessories.GuidedTypePalletSupport_WithPSB)
									strLevelInfo += "3";
								else if (_level.Accessories.GuidedTypePalletSupport && _level.Accessories.GuidedTypePalletSupport_WithStopper && _level.Accessories.GuidedTypePalletSupport_WithPSB)
									strLevelInfo += "4";

								strLevelInfo += ",";
								if (_level.Accessories.IsDeckPlateAvailable)
								{
									if (_level.Accessories.DeckPlateType == eDeckPlateType.eAlongDepth_UDL)
										strLevelInfo += "DU";
									else if (_level.Accessories.DeckPlateType == eDeckPlateType.eAlongDepth_PalletSupport)
										strLevelInfo += "DP";
									else if (_level.Accessories.DeckPlateType == eDeckPlateType.eAlongLength)
										strLevelInfo += "L";
									else
										strLevelInfo += "0";
								}
								else
									strLevelInfo += "0";

								strLevelInfo += ",";
								if (_level.Beam != null && _level.Beam.Beam != null)
									strLevelInfo += _level.Beam.Beam.Name.Replace(",", ".");
							}

							//
							fs.WriteLine(strLevelInfo);

							// export pallets
							if (!_rack.ShowPallet)
								continue;
							if (_level.Pallets == null)
								continue;

							string strPalletInfo = "Pallet_Level_";
							if (_level.Index == 0)
								strPalletInfo += "GR";
							else
								strPalletInfo += _level.Index.ToString();

							strPalletInfo += ",";
							strPalletInfo += _level.Pallets.Count.ToString();

							strPalletInfo += ",";
							if (RacksPalletType == ePalletType.eFlush)
								strPalletInfo += "FL";
							else if (RacksPalletType == ePalletType.eOverhang)
								strPalletInfo += "OH";

							foreach (Pallet _pallet in _level.Pallets)
							{
								if (_pallet == null)
									continue;

								strPalletInfo += ",";
								strPalletInfo += _pallet.Length.ToString();

								strPalletInfo += ",";
								strPalletInfo += _pallet.Width.ToString();

								strPalletInfo += ",";
								strPalletInfo += _pallet.Height.ToString();

								strPalletInfo += ",";
								strPalletInfo += _pallet.Load.ToString();
							}

							//
							fs.WriteLine(strPalletInfo);
						}
					}
				}

				// Statistics
				fs.WriteLine("**");
				fs.WriteLine("Statistics");

				//
				if (_sheet.RackStatistics == null)
					continue;

				List<StatRackItem> _statList = new List<StatRackItem>();
				_statList.AddRange(_sheet.RackStatistics);
				_statList = _statList.OrderBy(_s => _s.RackIndex).ToList();

				foreach(StatRackItem _item in _statList)
				{
					if (_item == null)
						continue;

					string strRackStat = string.Empty;

					if (_item.Count_M > 0)
					{
						// M
						strRackStat = _item.Name_M;
						strRackStat += ",";
						strRackStat += _item.Length_M;
						strRackStat += ",";
						strRackStat += _item.Width;
						strRackStat += ",";
						strRackStat += _item.Height;
						strRackStat += ",";
						strRackStat += _item.Count_M;
						//
						fs.WriteLine(strRackStat);
					}

					if (_item.Count_A > 0)
					{
						// A
						strRackStat = _item.Name_A;
						strRackStat += ",";
						strRackStat += _item.Length_A;
						strRackStat += ",";
						strRackStat += _item.Width;
						strRackStat += ",";
						strRackStat += _item.Height;
						strRackStat += ",";
						strRackStat += _item.Count_A;
						//
						fs.WriteLine(strRackStat);
					}
				}

				// pallet stat
				if (_sheet.PalletsStatistics == null)
					continue;

				foreach(StatPalletItem _palletStatItem in _sheet.PalletsStatistics)
				{
					if (_palletStatItem == null)
						continue;

					string strPalletStat = _palletStatItem.DisplayName;
					strPalletStat += ",";
					strPalletStat += _palletStatItem.Length.ToString();
					strPalletStat += ",";
					strPalletStat += _palletStatItem.Width.ToString();
					strPalletStat += ",";
					strPalletStat += _palletStatItem.Height.ToString();
					strPalletStat += ",";
					strPalletStat += _palletStatItem.Load.ToString();
					strPalletStat += ",";
					strPalletStat += _palletStatItem.Count.ToString();

					//
					fs.WriteLine(strPalletStat);
				}

				// export notes
				fs.WriteLine("**");
				fs.WriteLine("Notes");
				if (!string.IsNullOrEmpty(_sheet.Notes))
					fs.WriteLine(_sheet.Notes);

				// MHE configuration
				fs.WriteLine("**");
				fs.WriteLine("MHE Configuration");
				if (this.EnabledMHEConfigsList != null)
				{
					foreach (MHEConfiguration enabledConfig in this.EnabledMHEConfigsList)
					{
						if (enabledConfig == null)
							continue;

						StringBuilder mheConfigSB = new StringBuilder();
						mheConfigSB.Append(enabledConfig.Type);
						mheConfigSB.Append(",");
						mheConfigSB.Append(enabledConfig.PickingAisleWidth.ToString("0."));
						mheConfigSB.Append(",");
						mheConfigSB.Append(enabledConfig.CrossAisleWidth.ToString("0."));
						mheConfigSB.Append(",");
						mheConfigSB.Append(enabledConfig.EndAisleWidth.ToString("0."));
						mheConfigSB.Append(",");
						mheConfigSB.Append(enabledConfig.Capacity.ToString("0."));
						mheConfigSB.Append(",");
						mheConfigSB.Append(enabledConfig.MaxLoadingHeight.ToString("0."));
						mheConfigSB.Append(",");
						mheConfigSB.Append(enabledConfig.OverallHeightLowered.ToString("0."));

						fs.WriteLine(mheConfigSB.ToString());
					}
				}
			}

			fs.Close();
			fs.Dispose();

			return true;
		}


		//=============================================================================
		// Changes m_bShowAdvancedProperties.
		public void Set_ShowAdvancedProperties(bool bShowAdvancedProperties, bool markStateChanged)
		{
			if (m_bShowAdvancedProperties != bShowAdvancedProperties)
				m_bShowAdvancedProperties = bShowAdvancedProperties;

			NotifyPropertyChanged(() => ShowAdvancedProperties);

			// Create rack - dont place it - go to the advanced properties - state is marked changed.
			// So dont mark state if command is running.
			if (markStateChanged && !IsInCommand)
				this.MarkStateChanged();
		}




		//=============================================================================
		public int GetColumnUniqueSizeIndex(Column c)
		{
			if(c != null)
			{
				string strKey = Column._sGetKey(c);
				ColumnSizeIndex founded = m_ColumnsUniqueSizes.Find(s => s.Key == strKey);
				if(founded != null)
					return m_ColumnsUniqueSizes.IndexOf(founded);
			}

			return -1;
		}
		//=============================================================================
		public int AddColumnUniqueSize(Column c)
		{
			if (c == null)
				return 0;

			string strKey = Column._sGetKey(c);
			ColumnSizeIndex founded = m_ColumnsUniqueSizes.Find(s => s.Key == strKey);
			if (founded == null)
			{
				founded = new ColumnSizeIndex(strKey);
				m_ColumnsUniqueSizes.Add(founded);
			}

			if (founded != null)
			{
				founded.Count++;
				return m_ColumnsUniqueSizes.IndexOf(founded);
			}

			return -1;
		}
		//=============================================================================
		public int RemoveColumnUniqueSize(Column c)
		{
			if (c == null)
				return -1;

			string strKey = Column._sGetKey(c);
			ColumnSizeIndex founded = m_ColumnsUniqueSizes.Find(s => s.Key == strKey);

			if (founded != null)
			{
				if(founded.Count >= 1)
					founded.Count--;

				return (int)founded.Count;
			}

			return -1;
		}
		//=============================================================================
		public bool RecalcColumnUniqueSize(List<Column> changedColumns)
		{
			if (changedColumns == null)
				return false;

			// try to save old index
			foreach(Column c in changedColumns)
			{
				if (c == null)
					continue;

				int iSizeIndex = c.SizeIndex;
				if (iSizeIndex >= 0 && iSizeIndex < m_ColumnsUniqueSizes.Count)
				{
					ColumnSizeIndex founded = m_ColumnsUniqueSizes[iSizeIndex];
					if (founded != null)
					{
						if (founded.Count >= 1)
							founded.Count -= 1;
					}
				}
			}

			// make changes
			foreach(Column c in changedColumns)
			{
				if (c == null)
					continue;

				ColumnSizeIndex _newSizeIndex = null;
				ColumnSizeIndex _oldSizeIndex = null;

				// find old
				int iSizeIndex = c.SizeIndex;
				if (iSizeIndex >= 0 && iSizeIndex < m_ColumnsUniqueSizes.Count)
				{
					_oldSizeIndex = m_ColumnsUniqueSizes[iSizeIndex];
				}

				// find new
				string strKey = Column._sGetKey(c);
				_newSizeIndex = m_ColumnsUniqueSizes.Find(s => s.Key == strKey);

				//
				if(_newSizeIndex != null)
				{
					_newSizeIndex.Count += 1;
				}
				else
				{
					// try to save old index
					if(_oldSizeIndex != null && _oldSizeIndex.Count == 0)
					{
						_oldSizeIndex.Key = strKey;
						_oldSizeIndex.Count += 1;
					}
					else
					{
						_newSizeIndex = new ColumnSizeIndex(strKey);
						_newSizeIndex.Count = 1;
						m_ColumnsUniqueSizes.Add(_newSizeIndex);
					}
				}
			}

			// remove all sizes with 0 columns
			List<ColumnSizeIndex> sizesToRemove = new List<ColumnSizeIndex>();
			foreach(ColumnSizeIndex _sizeIndex in m_ColumnsUniqueSizes)
			{
				if (_sizeIndex == null)
					continue;

				if (_sizeIndex.Count == 0)
					sizesToRemove.Add(_sizeIndex);
			}
			//
			foreach(ColumnSizeIndex _s in sizesToRemove)
			{
				if (_s == null)
					continue;

				m_ColumnsUniqueSizes.Remove(_s);
			}

			// update all columns
			foreach(DrawingSheet sheet in m_Sheets)
			{
				if (sheet == null)
					continue;

				sheet.UpdateAllColumnsIndex();
			}

			return true;
		}



		//=============================================================================
		public int GetRackUniqueSizeIndex(Rack r)
		{
			int key;
			RackSizeIndex value;
			if (_FindRackIndex(r, out key, out value))
				return key;

			return -1;
		}
		//=============================================================================
		public int AddRackUniqueSize(Rack r)
		{
			if (r == null)
				return 0;

			M_RackState _state = r.Get_MRackState();
			if (_state == null)
				return -1;

			int key;
			RackSizeIndex founded;
			_FindRackIndex(r, out key, out founded);
			//
			if (founded == null)
			{
				founded = new RackSizeIndex(_state);
				founded.Count = 1;
				int iNewIndex = _FintEmptyIndex();
				if (iNewIndex < 0)
					return -1;
				m_RacksUniqueSizes[iNewIndex] = founded;
				return iNewIndex;
			}

			if (founded != null)
			{
				founded.Count++;
				return key;
			}

			return -1;
		}
		//=============================================================================
		public void RemoveRackUniqueSize(Rack r)
		{
			if (r == null)
				return;

			M_RackState _state = r.Get_MRackState();
			if (_state == null)
				return;

			int key;
			RackSizeIndex founded;
			if(_FindRackIndex(r, out key, out founded))
			{
				if (key >= 0 && founded != null)
				{
					if (founded.Count >= 1)
						founded.Count--;

					if (founded.Count == 0)
						m_RacksUniqueSizes.Remove(key);
				}
			}
			// Try to remove using old key.
			// Otherwise need to add DrawingDocument.RecalcRackUniqueSize() before DrawingSheet.DeleteGeometry inside DrawingSheet.CheckDocument().
			// DrawingDocument.CheckDocument() can change rack inside it(for example recalculate pallet width), check result layout and delete changed rack.
			// In this case racks unique index will be not decreased, because rack was changed and _FindRackIndex() returns null.
			else if (r.SizeIndex >= 0 && m_RacksUniqueSizes.ContainsKey(r.SizeIndex))
			{
				founded = m_RacksUniqueSizes[r.SizeIndex];
				if(founded != null)
				{
					if (founded.Count >= 1)
						founded.Count--;

					if (founded.Count == 0)
						m_RacksUniqueSizes.Remove(key);
				}
			}
		}
		//=============================================================================
		public bool RecalcRackUniqueSize(SortedDictionary<long, List<Rack>> racksDictionary, bool bUpdateSheets)
		{
			if (racksDictionary == null)
				return false;

			// try to save old index
			foreach (long iChangeOrder in racksDictionary.Keys)
			{
				List<Rack> racksList = racksDictionary[iChangeOrder];
				if (racksList == null || racksList.Count == 0)
					continue;

				foreach (Rack rack in racksList)
				{
					if (rack == null)
						continue;

					int iSizeIndex = rack.SizeIndex;
					if (iSizeIndex >= 0 && m_RacksUniqueSizes.ContainsKey(iSizeIndex))
					{
						RackSizeIndex founded = m_RacksUniqueSizes[iSizeIndex];
						if (founded != null)
						{
							if (founded.Count >= 1)
								founded.Count -= 1;
						}
					}
				}
			}

			// make changes
			foreach (long iChangeOrder in racksDictionary.Keys)
			{
				List<Rack> racksList = racksDictionary[iChangeOrder];
				if (racksList == null || racksList.Count == 0)
					continue;

				foreach (Rack r in racksList)
				{
					if (r == null)
						continue;

					RackSizeIndex _newSizeIndex = null;
					RackSizeIndex _oldSizeIndex = null;

					// find old
					int iSizeIndex = r.SizeIndex;
					if (iSizeIndex >= 0 && m_RacksUniqueSizes.ContainsKey(iSizeIndex))
						_oldSizeIndex = m_RacksUniqueSizes[iSizeIndex];

					// find new
					int iNewKey;
					_FindRackIndex(r, out iNewKey, out _newSizeIndex);

					//
					if (_newSizeIndex != null)
					{
						_newSizeIndex.Count += 1;
					}
					else
					{
						M_RackState _state = r.Get_MRackState();

						// try to save old index
						if (_oldSizeIndex != null && _oldSizeIndex.Count == 0)
						{
							_oldSizeIndex.State = _state;
							_oldSizeIndex.Count += 1;
						}
						else
						{
							_newSizeIndex = new RackSizeIndex(_state);
							_newSizeIndex.Count = 1;

							iNewKey = _FintEmptyIndex();
							if (iNewKey >= 0)
								m_RacksUniqueSizes[iNewKey] = _newSizeIndex;
						}
					}
				}
			}

			// remove all sizes with 0 columns
			List<int> keysToRemoveList = new List<int>();
			foreach(int iKey in m_RacksUniqueSizes.Keys)
			{
				RackSizeIndex sizeIndex = m_RacksUniqueSizes[iKey];
				if (sizeIndex != null && sizeIndex.Count > 0)
					continue;

				keysToRemoveList.Add(iKey);
			}
			//
			foreach(int iKey in keysToRemoveList)
			{
				m_RacksUniqueSizes.Remove(iKey);
			}

			// update all racks
			if (bUpdateSheets)
			{
				foreach (DrawingSheet sheet in m_Sheets)
				{
					if (sheet == null)
						continue;

					sheet._UpdateSheet();
				}
			}

			return true;
		}

		//=============================================================================
		/// <summary>
		/// Mark state changed and place it to the undo\redo stack.
		/// </summary>
		public void MarkStateChanged()
		{
			RecalcRacksIndexes();

			//
			// Update State
			// Set m_bDontUpdateState = true to ignore State changing because
			// it is not changed really.
			m_bDontUpdateState = true;
			//
			State = _GetState();
			//
			m_bDontUpdateState = false;

			// Clear racks change order
			m_RackChangeOrder = 0;
			if(m_Sheets != null)
			{
				foreach(DrawingSheet sheet in m_Sheets)
				{
					if (sheet == null)
						continue;

					sheet.OnDocumentStateChanged();
				}
			}

			this.IsInCommand = false;
			if (DrawingDocument._sDrawing != null)
				DrawingDocument._sDrawing.InvalidateVisual();
		}

		//=============================================================================
		public void OnCurrentSheetSizeChanged()
		{
			// Invalidate visual, measure and arrange on DrawingControl to recalc new DrawingControl size.
			if(DrawingDocument._sDrawing != null)
			{
				DrawingDocument._sDrawing.InvalidateVisual();
				DrawingDocument._sDrawing.InvalidateMeasure();
				DrawingDocument._sDrawing.InvalidateArrange();
				//
				DrawingDocument._sDrawing.UpdateLayout();

				//
				DrawingDocument._sDrawing.ResetGrips();
				DrawingDocument._sDrawing.UpdateDrawing(true);
			}
		}

		//=============================================================================
		// Check document's sheets - does they have correct layout?
		// Ask user for remove incorrect geometry.
		public async Task<bool> CheckDocument(bool bDisplayRemoveGeometryDialog, bool RecalcRacksPalletsDepth)
		{
			if (m_Sheets == null)
				return true;

			// Count of deleted racks from all sheets.
			int totalDeletedRacksCount = 0;
			// list with changed racks, need to call recalc index for them
			List<Rack> changedRackList = new List<Rack>();
			// 1. If PalletConfiguration is removed then remove PalletConfiguration binding.
			// 2. Probably user open old document(which doesnt satisfy level height rules in Rack._RecalcLevelHeight),
			//    so need update level height from the pallet height and check rack height.
			foreach (DrawingSheet sheet in m_Sheets)
			{
				if (sheet == null)
					continue;

				sheet.CheckRacksColumnSizeAndBracingType(true);

				List<Rack> rackList = sheet.GetAllRacks();
				if (rackList.Count == 0)
					continue;

				List<List<Rack>> rackGroups = sheet.RacksGroups;
				foreach (Rack rackGeom in rackList)
				{
					if (rackGeom == null)
						continue;

					if (!sheet.Rectangles.Contains(rackGeom))
						continue;

					if (rackGeom.Levels == null)
						continue;

					// check - does this rack has binding to the PalletConfiguration
					bool bHasBinding = false;
					foreach(RackLevel level in rackGeom.Levels)
					{
						if (level == null)
							continue;
						if (level.Pallets == null)
							continue;

						foreach(Pallet pallet in level.Pallets)
						{
							if (pallet == null)
								continue;

							if(pallet.PalletConfiguration != null)
							{
								bHasBinding = true;
								break;
							}
						}

						if (bHasBinding)
							break;
					}

					bool bDeleteCurrent = false;
					// If rack has bindings to the pallet configuration, so need to update or remove it.
					if (bHasBinding)
					{
						foreach (RackLevel level in rackGeom.Levels)
						{
							if (level == null)
								continue;

							if (level.Pallets == null)
								continue;

							List<Pallet> palletsList = level.Pallets.ToList();
							foreach (Pallet pallet in palletsList)
							{
								if (pallet == null)
									continue;

								if (pallet.PalletConfiguration == null)
									continue;

								if (this.PalletConfigurationCollection.Contains(pallet.PalletConfiguration))
								{
									string strPropSysName = RackLevel._MakeLevelProp_SystemName(level, Rack.PROP_PALLET_CONFIGURATION);
									strPropSysName += pallet._PalletIndex.ToString();

									string strError;
									if (!rackGeom.SetPropertyValue(strPropSysName, pallet.PalletConfiguration, false, false, false, out strError, false))
									{
										bDeleteCurrent = true;
										break;
									}
									else
									{
										if (!changedRackList.Contains(rackGeom))
											changedRackList.Add(rackGeom);
									}
								}
								else
									pallet.Set_PalletConfiguration(null);

								if (bDeleteCurrent)
									break;

								if (level.PalletsAreEqual)
									break;
							}

							if (bDeleteCurrent)
								break;

							if (rackGeom.AreLevelsTheSame)
								break;
						}
					}

					// Need to update level height depend on the pallet height.
					// And check rack height after it.
					if (!bDeleteCurrent)
					{
						// Level height should satisfy some rules.
						foreach (RackLevel level in rackGeom.Levels)
						{
							if (level == null)
								continue;

							string strError;
							if(!level.RecalcLevelHeight(out strError))
							{
								bDeleteCurrent = true;
								break;
							}
						}

						// Rack height will be recalculated later - Rack.CheckRackHeight will be called.
					}

					if (bDeleteCurrent)
					{
						sheet.DeleteGeometry(new List<BaseRectangleGeometry>() { rackGeom }, false, false);
						++totalDeletedRacksCount;
					}

					sheet.RegroupRacks();
				}
			}
			// Check rack height, it depends on the MaxLoadingHeight property and PalletHeight\LevelHeight.
			// Check pallet load, it cant be greater than PalletTruckMaxCapacity.
			foreach (DrawingSheet sheet in m_Sheets)
			{
				if (sheet == null)
					continue;

				List<Rack> rackList = sheet.GetAllRacks();
				if (rackList.Count == 0)
					continue;

				foreach (Rack rackGeometry in rackList)
				{
					if (rackGeometry == null)
						continue;

					// Need to recalc pallet depth, it depends on the overhang value.
					// And check layout after it. Do it before rack height check, because
					// rack's position can be changed.
					bool bDeleteRack = false;
					if (RecalcRacksPalletsDepth)
					{
						rackGeometry._RecalcPalletWidth();
						if (!rackGeometry.IsCorrect(sheet.Length, sheet.Width, BaseRectangleGeometry.GRIP_CENTER, false, true, false))
						{
							bDeleteRack = true;
						}
					}

					// check rack's Underpass value, it depends on the DrawingDocument.OverallHeightLowered value.
					if(!bDeleteRack)
					{
						string strError;
						if (!rackGeometry.IsUnderpassCorrect(out strError, true))
							bDeleteRack = true;
					}

					// check rack max height
					if (!bDeleteRack)
						bDeleteRack = !rackGeometry.CheckRackHeight(false);

					if (bDeleteRack)
					{
						++totalDeletedRacksCount;
						sheet.DeleteGeometry(new List<BaseRectangleGeometry>() { rackGeometry }, false, false);
						sheet.RegroupRacks();
						continue;
					}

					if (!changedRackList.Contains(rackGeometry))
						changedRackList.Add(rackGeometry);

					// check pallet load
					if (rackGeometry.Levels == null)
						continue;

					double palletTruckMaxCap = this.PalletTruckMaxCapacity;
					if (double.IsNaN(palletTruckMaxCap) || double.IsInfinity(palletTruckMaxCap))
						continue;

					foreach (RackLevel level in rackGeometry.Levels)
					{
						if (level == null)
							continue;

						if (level.Pallets == null)
							continue;

						foreach (Pallet pallet in level.Pallets)
						{
							if (pallet == null)
								continue;

							if (Utils.FGT(pallet.Load, palletTruckMaxCap))
							{
								// Dont check level beam capacity, because it was already calculated with greater than PalletTruckMaxCapacity load.
								// PalletTruckMaxCapacity is less than old pallet load value.
								pallet.Set_Load((uint)Utils.GetWholeNumber(palletTruckMaxCap));
								if (!changedRackList.Contains(rackGeometry))
									changedRackList.Add(rackGeometry);
							}
						}
					}
				}
			}

			// regroup racks row\columns
			foreach (DrawingSheet sheet in m_Sheets)
			{
				if (sheet == null)
					continue;

				// rack can be grouped only if they have the same depth
				// need to check all groups - probably the first rack in the group is not M after match properties
				List<Rack> deletedRacksList = new List<Rack>();
				sheet.CheckRacksGroups(out deletedRacksList);
			}

			// check Aisle Space length\width because they depends on MHE travel width options
			foreach (DrawingSheet sheet in m_Sheets)
			{
				if (sheet == null)
					continue;

				sheet.CheckAisleSpaces();
			}

			bool bAskUser = true;
			foreach (DrawingSheet sheet in m_Sheets)
			{
				if (sheet == null)
					continue;

				//
				sheet.DeleteNonInitializedGeometry(false, false);

				//
				List<BaseRectangleGeometry> incorrectGeometryList = new List<BaseRectangleGeometry>();
				if (!sheet.IsLayoutCorrect(out incorrectGeometryList))
				{
					int countOfRacksForDelete = 0;
					foreach(BaseRectangleGeometry incorretGeom in incorrectGeometryList)
					{
						if (incorretGeom == null)
							continue;

						Rack rack = incorretGeom as Rack;
						if (rack == null)
							continue;

						++countOfRacksForDelete;
					}

					bool bRemoveGeometry = true;
					if (bDisplayRemoveGeometryDialog && bAskUser)
					{
						// highlight incorrect geometry
						sheet.HiglightedRectangles.Clear();
						sheet.HiglightedRectangles.AddRange(incorrectGeometryList);
						// display sheet layout
						this._SetCurrentSheet(sheet, false);

						IYesNoCancelViewModel vm = null;
						// true - yes
						// false - no
						// null - cancel
						object result = await this.DisplayDialog.YesNoCancelDialog("Sheet's layout is not correct. Do you want to delete incorrect geometry?", out vm);
						//
						sheet.HiglightedRectangles.Clear();
						//
						if (result is bool)
						{
							if (vm.RememberTheChoice)
								bAskUser = false;
							if (!(bool)result)
								return false;
						}
						else if (result == null)
						{
							// null is cancel
							return false;
						}
					}

					if (bRemoveGeometry)
						sheet.DeleteGeometry(incorrectGeometryList, false, true);
					totalDeletedRacksCount += countOfRacksForDelete;
				}

				// rack can be grouped only if they have the same depth
				// need to check all groups - probably the first rack in the group is not M after match properties
				List<Rack> deletedRacksList = new List<Rack>();
				sheet.CheckRacksGroups(out deletedRacksList);

				// check tie beams
				sheet.CheckTieBeams();
			}

			// Display message with total deleted racks count.
			if (totalDeletedRacksCount > 0)
				await this.DisplayDialog.DisplayMessageDialog("Total deleted racks count: " + totalDeletedRacksCount.ToString());

			return true;
		}

		//=============================================================================
		// Check m_PalletConfigurationCollection for PalletConfiguration with passed properties.
		public bool ContainsPalletConfig(double palletLength, double palletWidth, double palletHeight, double palletCapacity)
		{
			if (m_PalletConfigurationCollection == null)
				return false;

			if (m_PalletConfigurationCollection.Count == 0)
				return false;

			PalletConfiguration palletConfig = m_PalletConfigurationCollection.FirstOrDefault(
				p =>
				p != null
				&& Utils.FEQ(p.Length, palletLength)
				&& Utils.FEQ(p.Width, palletWidth)
				&& Utils.FEQ(p.Height, palletHeight)
				&& Utils.FEQ(p.Capacity, palletCapacity)
				);

			return palletConfig != null;
		}

		//=============================================================================
		/// <summary>
		/// Read RAckColumn from the LoadChart.xlsx file. It takes around 1 minute.
		/// </summary>
		public async void ReadColumnsFromExcel()
		{
			string strError;
			if (RackLoadUtils.ReadData(out strError))
			{
				m_RacksColumnsList = RackLoadUtils.RacksColumnsList;
				m_BeamsList = RackLoadUtils.ReallyUsedBeamsList;
			}
			else
				await this.DisplayDialog.DisplayMessageDialog("Exception was occurred while reading data from LoadChart.xlsx. Dont save this document it is incorrect probably.\n\n" + strError);
		}


		//=============================================================================
		public virtual IClonable Clone()
		{
			DrawingDocument_State curDocState = this._GetState();
			// Dont place racks columns and beams list inside DrawingDocument_State.
			// Because they will be cloned every time when DrawingDocument.MarkStateChanged() is called.
			DrawingDocument newDoc = new DrawingDocument(m_RacksColumnsList, m_BeamsList, curDocState);
			return newDoc;
		}

		#endregion

		#region Private functions

		//=============================================================================
		// Initialize PalletConfigurationCollView.
		private void _InitPalletConfigCollView()
		{
			// create collection view
			m_PalletConfigurationCollView = CollectionViewSource.GetDefaultView(m_PalletConfigurationCollection);
			// add sorting
			if (m_PalletConfigurationCollView != null)
				m_PalletConfigurationCollView.SortDescriptions.Add(new SortDescription("UniqueIndex", ListSortDirection.Ascending));
		}

		//=============================================================================
		// Initialize m_MHEConfigurationsList.
		private void _InitMHEConfigurationsList()
		{
			m_MHEConfigurationsColl.Add(new MHEConfiguration(this, true, "Forklift", 4200, 2000, 100, 1000, 5500, 2700));
			m_MHEConfigurationsColl.Add(new MHEConfiguration(this, false, "Stacker", 2700, 2000, 100, 1000, 5500, 2700));
			m_MHEConfigurationsColl.Add(new MHEConfiguration(this, false, "Articulated forklift", 2400, 3000, 500, 1000, 10600, 4800));
			m_MHEConfigurationsColl.Add(new MHEConfiguration(this, false, "Reach truck", 3200, 2000, 100, 1000, 9000, 3900));
			m_MHEConfigurationsColl.Add(new MHEConfiguration(this, false, "Pallet truck", 3000, 2800, 1800, 1000, 2500, 2000));
			m_MHEConfigurationsColl.Add(new MHEConfiguration(this, false, "Tow tractor", 2000, 1800, 850, 1500, 6200, 5300));
		}

		//=============================================================================
		private bool _FindRackIndex(Rack r, out int key, out RackSizeIndex value)
		{
			key = -1;
			value = null;

			if (r != null)
			{
				M_RackState _state = r.Get_MRackState();
				if (_state == null)
					return false;
				try
				{
					foreach (int iKey in m_RacksUniqueSizes.Keys)
					{
						RackSizeIndex rackIndex = m_RacksUniqueSizes[iKey];
						if (rackIndex != null && rackIndex.State == _state)
						{
							key = iKey;
							value = rackIndex;
							return true;
						}
					}
				}
				catch { }
			}

			return false;
		}
		private int _FintEmptyIndex()
		{
			for(int i=0; i<99999; ++i)
			{
				if (!m_RacksUniqueSizes.ContainsKey(i))
					return i;
			}

			return -1;
		}

		//=============================================================================
		/// <summary>
		/// Set sheet as current(selected) sheet.
		/// </summary>
		private void _SetCurrentSheet(DrawingSheet sheet, bool markStateChanged)
		{
			DrawingSheet currSheet = null;
			if (m_CurrentSheetIndex >= 0 && m_CurrentSheetIndex < m_Sheets.Count)
				currSheet = m_Sheets[m_CurrentSheetIndex];

			if (sheet != currSheet)
			{
				// hide prev
				if (currSheet != null)
					currSheet.Show(false);

				if (sheet == null)
				{
					m_CurrentSheetIndex = -1;
				}
				else
				{
					if (m_Sheets.Contains(sheet))
					{
						sheet.Show(true);
						m_CurrentSheetIndex = m_Sheets.IndexOf(sheet);
						sheet.IsSelected = true;
					}
				}

				this.Set_ShowAdvancedProperties(false, false);

				if (markStateChanged)
					this.MarkStateChanged();
			}

			NotifyPropertyChanged(() => CurrentSheet);
		}

		//=============================================================================
		private DrawingDocument_State _GetState()
		{
			List<DrawingSheet> sheets = new List<DrawingSheet>();
			foreach(DrawingSheet _sheet in m_Sheets)
			{
				if (_sheet == null)
					continue;
		
				sheets.Add(_sheet);
			}

			DrawingDocument_State currState = new DrawingDocument_State(
				sheets,
				m_CurrentSheetIndex,
				m_ColumnsUniqueSizes,
				m_RacksUniqueSizes,
				m_RackBracingType,
				m_RackAccessories,
				//
				CustomerName,
				CustomerContactNo,
				CustomerEMail,
				CustomerENQ,
				CustomerAddress,
				CustomerSite,
				//
				PalletConfigurationCollection.ToList(),
				m_MHEConfigurationsColl.ToList(),
				//
				m_bShowAdvancedProperties,
				//
				m_RacksPalletType,
				m_RacksPalletsOverhangValue,
				//
				m_Currency,
				m_Rate,
				m_Discount
				);
			return currState;
		}
		//=============================================================================
		private void _SetState(DrawingDocument_State state)
		{
			DrawingSheet _oldCurrSheet = CurrentSheet;
			if (_oldCurrSheet != null)
				_oldCurrSheet.Show(false);

			this.PropertySourceRack = null;
			m_Sheets.Clear();
			m_CurrentSheetIndex = -1;
			m_ColumnsUniqueSizes.Clear();
			m_RacksUniqueSizes.Clear();
			//
			m_RackBracingType = eBracingType.eGI;
			m_RackAccessories = new RackAccessories();
			//
			m_CustomerName = string.Empty;
			m_CustomerContactNo = string.Empty;
			m_CustomerEMail = string.Empty;
			m_CustomerENQ = string.Empty;
			m_CustomerAddress = string.Empty;
			m_CustomerSite = string.Empty;
			//
			m_PalletConfigurationCollection.Clear();
			m_MHEConfigurationsColl.Clear();
			//
			m_bShowAdvancedProperties = false;
			//
			m_RacksPalletType = ePalletType.eOverhang;
			m_RacksPalletsOverhangValue = 100.0;

			// Clear racks changes order.
			m_RackChangeOrder = 0;

			if (state == null)
				return;

			DrawingDocument_State stateClone = state.Clone() as DrawingDocument_State;
			if (stateClone == null)
				return;
		
			if(stateClone.Sheets != null)
			{
				foreach (DrawingSheet sheet in stateClone.Sheets)
				{
					if (sheet == null)
						continue;

					sheet.Document = this;
					m_Sheets.Add(sheet);
				}
			}

			m_CurrentSheetIndex = stateClone.CurrentSheetIndex;

			if (m_CurrentSheetIndex < 0 && m_Sheets.Count > 0)
				m_CurrentSheetIndex = 0;

			if(stateClone.ColumnSizes != null)
				m_ColumnsUniqueSizes = stateClone.ColumnSizes;
			if(stateClone.RackSizes != null)
				m_RacksUniqueSizes = stateClone.RackSizes;

			//
			m_RackBracingType = stateClone.Rack_BracingType;
			m_RackAccessories = stateClone.Rack_Accessories;
			//
			m_CustomerName = stateClone.CustomerName;
			if (m_CustomerName == null)
				m_CustomerName = string.Empty;
			m_CustomerContactNo = stateClone.CustomerContactNo;
			if (m_CustomerContactNo == null)
				m_CustomerContactNo = string.Empty;
			m_CustomerEMail = stateClone.CustomerEMail;
			if (m_CustomerEMail == null)
				m_CustomerEMail = string.Empty;
			m_CustomerENQ = stateClone.CustomerENQ;
			if (m_CustomerENQ == null)
				m_CustomerENQ = string.Empty;
			m_CustomerAddress = stateClone.CustomerAddress;
			if (m_CustomerAddress == null)
				m_CustomerAddress = string.Empty;
			m_CustomerSite = stateClone.CustomerSite;
			if (m_CustomerSite == null)
				m_CustomerSite = string.Empty;
			//
			if (stateClone.PalletConfigList != null)
			{
				foreach (PalletConfiguration palletConfig in stateClone.PalletConfigList)
				{
					if (palletConfig == null)
						continue;

					m_PalletConfigurationCollection.Add(palletConfig);
				}
			}
			if(stateClone.MHEConfigsList != null)
			{
				foreach(MHEConfiguration mheConfig in stateClone.MHEConfigsList)
				{
					if (mheConfig == null)
						continue;

					mheConfig.Document = this;
					m_MHEConfigurationsColl.Add(mheConfig);
				}
			}

			m_bShowAdvancedProperties = stateClone.ShowAdvancedProperties;
			m_RacksPalletType = stateClone.RacksPalletType;
			m_RacksPalletsOverhangValue = stateClone.RacksPalletsOverhangValue;

			m_Currency = stateClone.Currency;
			m_Rate = stateClone.Rate;
			m_Discount = stateClone.Discount;

			DrawingSheet currSheet = CurrentSheet;
			if (currSheet != null)
				currSheet.Show(true);

            // notify current sheet to update details for new state
            currSheet.MarkStateChanged();

            NotifyPropertyChanged(() => CurrentSheet);
		}

		//=============================================================================
		// Call all necessary notifications.
		private void _OnStateChanged()
		{
			NotifyPropertyChanged(() => CanUndo);
			NotifyPropertyChanged(() => CanRedo);
			NotifyPropertyChanged(() => State);
			NotifyPropertyChanged(() => StatesCount);
			NotifyPropertyChanged(() => CurrentSheet);
			NotifyPropertyChanged(() => ShowAdvancedProperties);
			NotifyPropertyChanged(() => IsCloseSheetButtonEnabled);

#if DEBUG
			_UpdateRacksSizesCollection();
#endif

			// call StateChanged event
			if (this.StateChanged != null)
				this.StateChanged(this, null);
		}

		//=============================================================================
		/// <summary>
		/// Updates racks unique sizes view models collection.
		/// It is used only for DEBUG, dont do it in RELEASE.
		/// </summary>
		private void _UpdateRacksSizesCollection()
		{
			m_RacksSizesCollection.Clear();

			if (m_RacksUniqueSizes == null)
				return;

			foreach (int sizeIndexKey in m_RacksUniqueSizes.Keys)
			{
				RackSizeIndex sizeIndexValue = m_RacksUniqueSizes[sizeIndexKey];
				if (sizeIndexValue == null)
					continue;

				RackSizeIndexVM rackSizeIndex = new RackSizeIndexVM(sizeIndexKey, sizeIndexValue);
				m_RacksSizesCollection.Add(rackSizeIndex);
			}
		}

		//=============================================================================
		/// <summary>
		/// Starts timer, which will be tick and remove displayed Error.
		/// </summary>
		private void _StartErrorStringTimer()
		{
			if (m_ErrorStringTimer != null)
				m_ErrorStringTimer.Stop();

			m_ErrorStringTimer = new DispatcherTimer();
			m_ErrorStringTimer.Tick += M_ErrorStringTimer_Tick;
			m_ErrorStringTimer.Interval = new TimeSpan(0, 0, 5);
			m_ErrorStringTimer.Start();
		}
		//
		private void M_ErrorStringTimer_Tick(object sender, EventArgs e)
		{
			//this.DocumentError = string.Empty;
			m_DocumentError = string.Empty;
			NotifyPropertyChanged(() => DocumentError);
		}

		//=============================================================================
		/// <summary>
		/// Recalculates rack indexes
		/// </summary>
		private void RecalcRacksIndexes()
		{
			// key - rack change order
			// value - racks list
			//
			// For example, -1(key) will contains list for not changed racks, try to save indexes for them.
			SortedDictionary<long, List<Rack>> racksDictionary = new SortedDictionary<long, List<Rack>>();

			if (m_Sheets != null)
			{
				foreach (DrawingSheet sheet in m_Sheets)
				{
					if (sheet == null)
						continue;

					List<Rack> racksList = sheet.GetAllRacks();
					if (racksList == null || racksList.Count == 0)
						continue;

					foreach(Rack rack in racksList)
					{
						if (rack == null)
							continue;

						long rackChangeOrder = rack.ChangeOrder;
						if (!racksDictionary.ContainsKey(rackChangeOrder))
							racksDictionary[rackChangeOrder] = new List<Rack>();

						racksDictionary[rackChangeOrder].Add(rack);
					}
				}
			}

			this.RecalcRackUniqueSize(racksDictionary, false);

			// Update racks index and racks statistics
			if (m_Sheets != null)
			{
				foreach (DrawingSheet sheet in m_Sheets)
				{
					if (sheet == null)
						continue;

					List<Rack> racksList = sheet.GetAllRacks();
					if (racksList == null || racksList.Count == 0)
					{
						// Update to avoid case that all deleted
						sheet.UpdateStatisticsCollections();
						continue;
					}

					foreach (Rack rack in racksList)
					{
						if (rack == null)
							continue;

						rack.SizeIndex = this.GetRackUniqueSizeIndex(rack);
					}

					// Update racks and pallets statistics
					sheet.UpdateStatisticsCollections();
				}
			}
		}

		#endregion

		#region IDataErrorInfo

		public string this[string columnName]
		{
			get
			{
				string error = string.Empty;
				switch (columnName)
				{
					case "CustomerENQ":
						{
							if (string.IsNullOrEmpty(this.CustomerENQ))
								error = "ENQ number cannot be empty.";
							else if (this.CustomerENQ.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
								error = "ENQ number contains characters which are invalid for the filename.";
							break;
						}
					default:
						break;
				}
				return error;
			}
		}
		public string Error
		{
			get { return null; }
		}

		#endregion


		#region Serialization

		//=============================================================================
		//
		// 2.0
		// 1.1 Add m_ColumnsUniqueSizes
		// 1.2 Add m_RacksUniqueSizes
		// 1.3 Add rack common properties
		// 2.3 Dont have a time for write converting old documents to new one with rack advanced properties,
		//     so just increase major and dont support all old documents.
		// 3.3 Increase majot, because M_RackState doesnt write\read ShowPallet in stream, but ShowPallet is included in rack index
		//     calculation. Dont have a time for restore old docs, so just dont support them.
		// 4.3 Increase major, remove Rack material height\weight on ground properties. Dont support old drawings.
		// 5.3 Increase major. Pallets can has different height, load, width. Dont support old drawings.
		// 6.3 Increase major. Add DisableChangeSizeGripPoints to the rack properties
		// 7.3 Increase major. Remove Qty and TopTieLength from rack accessories
		// 8.3 Increase major. Add Beam to Rack level
		// 8.4 Change rack size index list to dictionary
		// 9.4 Increase major, racks have incorrect min, max length values. They doesnt depends on the beams size.
		// 9.5 Add customer info data
		// 9.6 Increase minor of Rack - 2.2 because ClearAvailableHeight was not calculate correct. Need recalc rack index.
		// 9.7 Increase minor - add Back to Back distance.
		// 9.8 Increase minor - add MHE travel width.
		// 9.9 Add pallet truck max capacity.
		// 9.10 Add max loading height.
		// 9.11 Add pallet configurations collection.
		// 10.11 Remove Clear Available Height from the Rack geometry, it is calculated based on the roof type and rack position.
		// 10.12 BackToBackDistance minimim value is changed from 200 to 60. If BTBDistance is equal 60 than all racks should be flush.
		// 10.13 Remove eMHETravelWidth, max loading height and pallet truck max capacity. Add m_MHEConfigurationsList instead them.
		// 10.14 Add current sheet index
		// 10.15 Add ShowAdvancedProperties
		// 11.15 Implement level height rules. Check Rack._RecalculateLevelHeight.
		// 12.15 BackToBack distance is removed. PalletType(Overhang\Flush) and OverhangValues are added.
		// 13.15 Add RacksColumnsList with column data. Dont support old documents.
		//       RackBracing is removed. It is stored in the rack now.
		// 14.15 Add m_BeamsList. RackColumn now contains RackBeams inside.
		// 15.15 Increase major. Change max number of pallets per level from 4 to 3, it meanse that old documents are not correct.
		//       Also max number of levels changed from 8 to 20.
		// 16.15. Increase major. Change the first level offset(when MaterialOnGround and Underpass are unchecked).
		//        Old documents are not correct. Read comment to Rack.sFirstLevelOffset.
		// 16.16 Add Customer Site.
		// 17.16 Change rack max depth from 1600 to 1500. All old documents are not correct, because they can have 1600 depth racks.
		//       Dont support old documents.
		// 17.17 Add Currency, Rate and Discount.
		// 17.18 Add Pallet.MAX_HEIGHT. Dont support old documents - they are not correct.
		// 18.18 Change X\Y margin between rack and wall from 100 to 200. Dont support old documents.
		// 18.19 Add m_DocumentRevision.
		// 18.20 Column geometry is reworked and it can be rotated in this version. So, increase minor number for display warning message if user open new drawing in prev. version app.
		// 18.21 Add "Document_BRANCH"
		// 18.22 (RESERVED in RackDrawingApp_Master BRANCH) Add WarehouseSheet
		// 20.22 (RESERVED in RackDrawingApp_Master BRANCH) Only WarehouseSheet can have a roof, DrawingSheet cant. Dont support old document.
		// 21.22 (RESERVED in RackDrawingApp_Master BRANCH) Synchronize stream data with RackDrawingApp_01.407 branch. Dont support old documents, they have incorrect major and minor numbers.
		protected static string _sDocument_strMajor = "Document_MAJOR";
		protected static int _sDocument_MAJOR = 18;
		protected static string _sDocument_strMinor = "Document_MINOR";
		protected static int _sDocument_MINOR = 21;
		protected static string _sDocument_strBranch = "Document_BRANCH";
		// 0 - RackDrawingApp_Master. Development branch.
		// 1 - RackDrawingApp_01.407(this code). Release 01 branch, it is based on RackDrawingApp_Master.407 build.
		protected static int _sDocument_BRANCH_Master = 0;
		protected static int _sDocument_BRANCH_Release_01 = 1;
		//=============================================================================
		// In DrawingDocument(SerializationInfo info, StreamingContext context) deserialized
		// sheets collection will contain only null values instead DrawingSheet objects.
		// Because they are not deserialized yet. So we cant add them into m_Sheets collection.
		// We can add them in OnDeserialization() callback when all objects graph deserialization will be completed
		// and m_DeserializedSheets will contain DrawingSheet values instead null.
		private List<DrawingSheet> m_DeserializedSheets = null;
		public DrawingDocument(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sDocument_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sDocument_strMinor, typeof(int));

			int iBranch = -1;
			if (iMajor >= 18 && iMinor >= 21)
			{
				try
				{
					iBranch = (int)info.GetValue(_sDocument_strBranch, typeof(int));
				}
				catch { }
			}
			// Documents in this branch before 18.21 document version doesnt have "Document_BRANCH" parameter.
			// But we need to support them, so lets think that all documents before 18.21 are _sDocument_BRANCH_Release_01.
			else if (iMajor <= 18 && iMinor <= 20)
				iBranch = _sDocument_BRANCH_Release_01;

			// dont support other brances
			if (_sDocument_BRANCH_Release_01 != iBranch)
			{
				DrawingDocument._sDontSupportDocument = true;
				return;
			}

			if (iMajor > _sDocument_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sDocument_MAJOR && iMinor > _sDocument_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			// dont support old docs, read comment above
			if (iMajor < 18 || iMinor < 18)
			{
				DrawingDocument._sDontSupportDocument = true;
				return;
			}

			//if (iMajor <= 9 && iMinor <= 5)
			//	_RecalcRackIndex = true;

			if (iMajor <= _sDocument_MAJOR)
			{
				try
				{
					// There will be all null objects in the Rectangles.
					// They will be inited after all objects graph deserialization completed.
					// Need to add them as childs to DrawingControl, we can do it in Deserialization callback.
					m_DeserializedSheets = (List<DrawingSheet>)info.GetValue("Sheets", typeof(List<DrawingSheet>));

					//
					if (iMajor >= 1 && iMinor >= 1)
						m_ColumnsUniqueSizes = (List<ColumnSizeIndex>)info.GetValue("ColumnsUNiqueSizes", typeof(List<ColumnSizeIndex>));

					//
					if (iMajor >= 1 && iMinor >= 3)
					{
						// Removed in 13.15.
						//m_RackBracing = (eBracing)info.GetValue("Rack_Bracing", typeof(eBracing));
						m_RackBracingType = (eBracingType)info.GetValue("Rack_BracingType", typeof(eBracingType));
						m_RackAccessories = (RackAccessories)info.GetValue("Rack_Accessories", typeof(RackAccessories));
					}

					if (iMajor >= 8 && iMinor >= 4)
						m_RacksUniqueSizes = (Dictionary<int, RackSizeIndex>)info.GetValue("RacksUniqueSizes_Dictionary", typeof(Dictionary<int, RackSizeIndex>));

					if(iMajor >= 8 && iMinor >= 5)
					{
						m_CustomerName = (string)info.GetValue("CustomerName", typeof(string));
						if (m_CustomerName == null)
							m_CustomerName = string.Empty;

						m_CustomerAddress = (string)info.GetValue("CustomerAddress", typeof(string));
						if (m_CustomerAddress == null)
							m_CustomerAddress = string.Empty;

						m_CustomerContactNo = (string)info.GetValue("CustomerContactNo", typeof(string));
						if (m_CustomerContactNo == null)
							m_CustomerContactNo = string.Empty;

						m_CustomerEMail = (string)info.GetValue("CustomerEMail", typeof(string));
						if (m_CustomerEMail == null)
							m_CustomerEMail = string.Empty;

						m_CustomerENQ = (string)info.GetValue("CustomerRFQ", typeof(string));
						if (m_CustomerENQ == null)
							m_CustomerENQ = string.Empty;
					}

					// Removed in 12.15
					//if (iMajor >= 9 && iMinor >= 7)
					//{
					//	m_BackToBackDistance = (double)info.GetValue("BackToBackDistance", typeof(double));
					//	if (Utils.FLT(m_BackToBackDistance, BTB_DISTANCE_MIN_VALUE))
					//		m_BackToBackDistance = BTB_DISTANCE_MIN_VALUE;
					//}
					//else
					//{
					//	// use 200 as BackToBack distance
					//	m_BackToBackDistance = 200;
					//}

					// 10.13 Removed. Use m_MHEConfigurationsList instead it.
					//if (iMajor >= 9 && iMinor >= 8)
					//	m_MHE_TravelWidth = (eMHE_TravelWidth)info.GetValue("MHE_TravelWidth", typeof(eMHE_TravelWidth));
					//
					//if (iMajor >= 9 && iMinor >= 9)
					//	m_PalletTruckMaxCapacity = (double)info.GetValue("PalletTruckMaxCapacity", typeof(double));
					//
					//if (iMajor >= 9 && iMinor >= 10)
					//	m_MaxLoadingHeight = (double)info.GetValue("MaxLoadingHeight", typeof(double));

					if (iMajor >= 9 && iMinor >= 11)
						m_PalletConfigurationCollection = (ObservableCollection<PalletConfiguration>)info.GetValue("PalletConfigurationCollection", typeof(ObservableCollection<PalletConfiguration>));

					if (iMajor >= 9 && iMinor >= 13)
						m_MHEConfigurationsColl = (ObservableCollection<MHEConfiguration>)info.GetValue("MHEConfigurationsColl", typeof(ObservableCollection<MHEConfiguration>));
					else
						_InitMHEConfigurationsList();

					if (iMajor >= 10 && iMinor >= 14)
						m_CurrentSheetIndex = (int)info.GetValue("CurrentSheetIndex", typeof(int));
					else
						m_CurrentSheetIndex = -1;

					if (iMajor >= 10 && iMinor >= 15)
						m_bShowAdvancedProperties = (bool)info.GetValue("ShowAdvancedProperties", typeof(bool));
					else
						m_bShowAdvancedProperties = false;

					if(iMajor >= 12 && iMinor >= 15)
					{
						m_RacksPalletType = (ePalletType)info.GetValue("RacksPalletType", typeof(ePalletType));
						m_RacksPalletsOverhangValue = (double)info.GetValue("RacksPalletsOverhangValue", typeof(double));
					}

					if(iMajor >= 13 && iMinor >= 15)
					{
						m_RacksColumnsList = (List<RackColumn>)info.GetValue("RacksColumnsList", typeof(List<RackColumn>));
					}

					if(iMajor >= 14 && iMinor >= 15)
					{
						m_BeamsList = (List<RackBeam>)info.GetValue("BeamsList", typeof(List<RackBeam>));
					}

					if(iMajor >= 16 && iMinor >= 16)
						m_CustomerSite = (string)info.GetValue("CustomerSite", typeof(string));

					if(iMajor >= 17 && iMinor >= 17)
					{
						m_Currency = (string)info.GetValue("Currency", typeof(string));
						m_Rate = (double)info.GetValue("Rate", typeof(double));
						m_Discount = (double)info.GetValue("Discount", typeof(double));
					}

					if (iMajor >= 18 && iMinor >= 19)
						m_DocumentRevision = (uint)info.GetValue("DocumentRevision", typeof(uint));
					else
						m_DocumentRevision = 1;
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
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sDocument_strMajor, _sDocument_MAJOR);
			info.AddValue(_sDocument_strMinor, _sDocument_MINOR);
			// 18.21 Put "1", it is release 01 branch
			info.AddValue(_sDocument_strBranch, _sDocument_BRANCH_Release_01);

			// 
			List<DrawingSheet> sheets = new List<DrawingSheet>();
			foreach (DrawingSheet _sheet in m_Sheets)
			{
				if (_sheet == null)
					continue;

				sheets.Add(_sheet);
			}
			info.AddValue("Sheets", sheets);

			//
			info.AddValue("ColumnsUNiqueSizes", m_ColumnsUniqueSizes);
			// 1.3 rack common props
			//info.AddValue("Rack_Bracing", m_RackBracing); removed in 13.15
			info.AddValue("Rack_BracingType", m_RackBracingType);
			info.AddValue("Rack_Accessories", m_RackAccessories);

			// 8.4
			info.AddValue("RacksUniqueSizes_Dictionary", m_RacksUniqueSizes);

			// 9.5
			info.AddValue("CustomerName", m_CustomerName);
			info.AddValue("CustomerAddress", m_CustomerAddress);
			info.AddValue("CustomerContactNo", m_CustomerContactNo);
			info.AddValue("CustomerEMail", m_CustomerEMail);
			info.AddValue("CustomerRFQ", m_CustomerENQ);

			// Removed in 12.15
			//// 9.7
			//info.AddValue("BackToBackDistance", m_BackToBackDistance);

			// 9.8
			// 10.13 Removed. Use m_MHEConfigurationsList instead it.
			//info.AddValue("MHE_TravelWidth", 0);

			// 9.9
			// 10.13 Removed. Use m_MHEConfigurationsList instead it.
			//info.AddValue("PalletTruckMaxCapacity", 2000);

			// 9.10
			// 10.13 Removed. Use m_MHEConfigurationsList instead it.
			//info.AddValue("MaxLoadingHeight", 5200);

			// 9.11
			info.AddValue("PalletConfigurationCollection", m_PalletConfigurationCollection);

			// 10.13
			info.AddValue("MHEConfigurationsColl", m_MHEConfigurationsColl);

			// 10.14
			info.AddValue("CurrentSheetIndex", m_CurrentSheetIndex);

			// 10.15
			info.AddValue("ShowAdvancedProperties", m_bShowAdvancedProperties);

			// 12.15
			info.AddValue("RacksPalletType", m_RacksPalletType);
			info.AddValue("RacksPalletsOverhangValue", m_RacksPalletsOverhangValue);

			// 13.15
			info.AddValue("RacksColumnsList", m_RacksColumnsList);

			// 14.15
			info.AddValue("BeamsList", m_BeamsList);

			// 16.16
			info.AddValue("CustomerSite", m_CustomerSite);

			// 17.17
			info.AddValue("Currency", m_Currency);
			info.AddValue("Rate", m_Rate);
			info.AddValue("Discount", m_Discount);

			// 18.19
			info.AddValue("DocumentRevision", m_DocumentRevision);
		}
		//=============================================================================
		public virtual void OnDeserialization(object sender)
		{
			_InitPalletConfigCollView();

			// call deserialization on dictionaries, otherwise you can work with non-deserialised dictionary
			m_RacksUniqueSizes.OnDeserialization(sender);

			// Serialization and Deserialization used to save DrawingDocument with DrawingSheets collection to file.
			// Restore m_Sheets collection.
			m_Sheets.Clear();
			if(m_DeserializedSheets != null)
			{
				foreach (DrawingSheet sheet in m_DeserializedSheets)
				{
					if (sheet == null)
						continue;

					sheet.Document = this;
					sheet.OnDeserialization(sender);
					m_Sheets.Add(sheet);
				}
			}

			// Restore MHEConfiguration.Document property.
			// Also, only one MHE config can be enabled.
			if(m_MHEConfigurationsColl != null)
			{
				MHEConfiguration enabledMHEConfig = null;
				foreach(MHEConfiguration mheConfig in m_MHEConfigurationsColl)
				{
					if (mheConfig == null)
						continue;

					mheConfig.Document = this;

					if (mheConfig.IsEnabled)
					{
						if (enabledMHEConfig == null)
							enabledMHEConfig = mheConfig;
						else
							mheConfig.IsEnabled = false;
					}
				}
			}

			//
			if (m_Sheets.Count > 0 && m_CurrentSheetIndex < 0)
				m_CurrentSheetIndex = 0;

			// Serialization and Deserialization used to save DrawingDocument with DrawingSheets collection to file.
			// We dont save some properties and dont restore them while Deserialization.
			// But we need to restore State after Deserialization. Otherwise opened sheet will have DefaultState.
			m_States.Clear();
			State = _GetState();
		}

		#endregion
	}
}
