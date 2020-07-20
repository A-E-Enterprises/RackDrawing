using DrawingControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RackDrawingApp
{
	public class RackLevelAccessoriesDialog_ViewModel : BaseViewModel
	{
		public RackLevelAccessoriesDialog_ViewModel(RackLevelAccessories accessories, bool _showPallet)
		{
			m_Accessories = accessories;
			m_bShowPallet = _showPallet;
		}

		//=============================================================================
		private RackLevelAccessories m_Accessories = null;
		public RackLevelAccessories Accessories { get { return m_Accessories; } }

		//=============================================================================
		private bool m_bShowPallet = false;
		public bool ShowPallet { get { return m_bShowPallet; } }

		//=============================================================================
		public bool IsDeckPlateAvailable
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.IsDeckPlateAvailable;

				return false;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.IsDeckPlateAvailable = value;

				NotifyPropertyChanged(() => IsDeckPlateAvailable);
			}
		}
		//=============================================================================
		public eDeckPlateType DeckPlateType
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.DeckPlateType;

				return eDeckPlateType.eAlongLength;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.DeckPlateType = value;

				NotifyPropertyChanged(() => DeckPlateType);
			}
		}
		//=============================================================================
		private List<string> m_DeckPlateTypeList = new List<string>()
		{
			Rack.DECK_PLATE_TYPE_ALONG_DEPTH_UDL,
			Rack.DECK_PLATE_TYPE_ALONG_DEPTH_PALLET_SUPPORT,
			Rack.DECK_PLATE_TYPE_ALONG_LENGTH
		};
		public List<string> DeckPlateTypeList { get { return m_DeckPlateTypeList; } }

		//=============================================================================
		public bool PalletStopper
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.PalletStopper;

				return false;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.PalletStopper = value;

				NotifyPropertyChanged(() => PalletStopper);
			}
		}
		//=============================================================================
		public bool ForkEntryBar
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.ForkEntryBar;

				return false;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.ForkEntryBar = value;

				NotifyPropertyChanged(() => ForkEntryBar);
			}
		}
		//=============================================================================
		public bool PalletSupportBar
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.PalletSupportBar;

				return false;
			}
			set
			{
				if (m_Accessories != null)
				{
					m_Accessories.PalletSupportBar = value;

					if(m_Accessories.PalletSupportBar)
					{
						m_Accessories.GuidedTypePalletSupport = false;
						m_Accessories.GuidedTypePalletSupport_WithPSB = false;
						m_Accessories.GuidedTypePalletSupport_WithStopper = false;
					}
				}

				NotifyPropertyChanged(() => GuidedTypePalletSupport);
				NotifyPropertyChanged(() => GuidedTypePalletSupport_WithStopper);
				NotifyPropertyChanged(() => GuidedTypePalletSupport_WithPSB);
				NotifyPropertyChanged(() => PalletSupportBar);
			}
		}

		//=============================================================================
		public bool GuidedTypePalletSupport
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.GuidedTypePalletSupport;

				return false;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.GuidedTypePalletSupport = value;

				NotifyPropertyChanged(() => GuidedTypePalletSupport);
			}
		}
		//=============================================================================
		public bool GuidedTypePalletSupport_WithStopper
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.GuidedTypePalletSupport_WithStopper;

				return false;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.GuidedTypePalletSupport_WithStopper = value;

				NotifyPropertyChanged(() => GuidedTypePalletSupport_WithStopper);
			}
		}
		//=============================================================================
		public bool GuidedTypePalletSupport_WithPSB
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.GuidedTypePalletSupport_WithPSB;

				return false;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.GuidedTypePalletSupport_WithPSB = value;

				NotifyPropertyChanged(() => GuidedTypePalletSupport_WithPSB);
			}
		}
	}
}
