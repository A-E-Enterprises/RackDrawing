using DrawingControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RackDrawingApp
{
	/// <summary>
	/// View model for export document as PDF.
	/// </summary>
	public class ExportLayoutTemplateVM : BaseViewModel
	{
		public ExportLayoutTemplateVM(DrawingDocument doc, DrawingSheet sheet)
		{
			m_Document = doc;
			m_Sheet = sheet;
		}

		#region Properties

		//=============================================================================
		private DrawingDocument m_Document = null;

		//=============================================================================
		// If sheet is not null then rack and pallets statistcs will be displayed.
		private DrawingSheet m_Sheet = null;

		//=============================================================================
		public string Drawn
		{
			get
			{
				if (!string.IsNullOrEmpty(UserInfo.Login))
					return UserInfo.Login;

				return string.Empty;
			}
		}

		//=============================================================================
		public string ENQNo
		{
			get
			{
				if (m_Document != null)
					return m_Document.CustomerENQ;

				return string.Empty;
			}
		}

		//=============================================================================
		public string CustomerName
		{
			get
			{
				if (m_Document != null)
					return m_Document.CustomerName;

				return string.Empty;
			}
		}

		//=============================================================================
		public string CustomerAddress
		{
			get
			{
				if (m_Document != null)
					return m_Document.CustomerAddress;

				return string.Empty;
			}
		}

		//=============================================================================
		public string ProjectSite
		{
			get
			{
				if (m_Document != null)
					return m_Document.CustomerSite;

				return string.Empty;
			}
		}

		//=============================================================================
		private string m_Date = string.Empty;
		public string Date
		{
			get { return m_Date; }
			set
			{
				if(value != m_Date)
				{
					m_Date = value;
					NotifyPropertyChanged(() => Date);
				}
			}
		}

		//=============================================================================
		private int m_PageNumber = 1;
		public int PageNumber
		{
			get { return m_PageNumber; }
			set
			{
				if(value != m_PageNumber)
				{
					m_PageNumber = value;
					NotifyPropertyChanged(() => PageNumber);
				}
			}
		}

		//=============================================================================
		private string m_ImageHeaderText = string.Empty;
		public string ImageHeaderText
		{
			get { return m_ImageHeaderText; }
			set
			{
				if(value != m_ImageHeaderText)
				{
					m_ImageHeaderText = value;
					NotifyPropertyChanged(() => ImageHeaderText);
				}
			}
		}

		//=============================================================================
		private ImageSource m_ImageSource = null;
		public ImageSource ImageSrc
		{
			get { return m_ImageSource; }
			set
			{
				if(value != m_ImageSource)
				{
					m_ImageSource = value;
					NotifyPropertyChanged(() => ImageSrc);
				}
			}
		}

        //=============================================================================
        // Fill this if need to draw multiple images per page or use ImageSrc for single image
        private List<ImageSource> m_ImageSources = null;
		public List<ImageSource> ImageSources
		{
			get { return m_ImageSources; }
			set
			{
				if (value != m_ImageSources)
				{
					m_ImageSources = value;
					NotifyPropertyChanged(() => ImageSources);
				}
			}
		}

        //=============================================================================
        // If true then need to display rack statistics
        private bool m_bDisplayRackStatistics = false;
		public bool DisplayRackStatistics
		{
			get { return m_bDisplayRackStatistics; }
			set
			{
				if(value != m_bDisplayRackStatistics)
				{
					m_bDisplayRackStatistics = value;
					NotifyPropertyChanged(() => DisplayRackStatistics);
				}
			}
		}

		//=============================================================================
		// If true then need to display pallete statistics
		private bool m_bDisplayPalleteStatistics = false;
		public bool DisplayPalleteStatistics
		{
			get { return m_bDisplayPalleteStatistics; }
			set
			{
				if (value != m_bDisplayPalleteStatistics)
				{
					m_bDisplayPalleteStatistics = value;
					NotifyPropertyChanged(() => DisplayPalleteStatistics);
				}
			}
		}

		//=============================================================================
		private bool m_bDisplayRackAccessories = false;
		public bool DisplayRackAccessories
		{
			get { return m_bDisplayRackAccessories; }
			set
			{
				if(value != m_bDisplayRackAccessories)
				{
					m_bDisplayRackAccessories = value;
					NotifyPropertyChanged(() => DisplayRackAccessories);
				}
			}
		}

		//=============================================================================
		private bool m_bDisplayRackLevelAccessories = false;
		public bool DisplayRackLevelAccessories
		{
			get { return m_bDisplayRackLevelAccessories; }
			set
			{
				if (value != m_bDisplayRackLevelAccessories)
				{
					m_bDisplayRackLevelAccessories = value;
					NotifyPropertyChanged(() => DisplayRackLevelAccessories);
				}
			}
		}

		//=============================================================================
		public ICollectionView RacksStatisticsView
		{
			get
			{
				if (m_Sheet != null)
					return m_Sheet.StatisticsCollection;

				return null;
			}
		}

		//=============================================================================
		public ICollectionView PalletsStatisticsView
		{
			get
			{
				if (m_Sheet != null)
					return m_Sheet.PalletsStatisticsCollectionView;

				return null;
			}
		}

		//=============================================================================
		private bool m_DisplayNotes = false;
		public bool DisplayNotes
		{
			get { return m_DisplayNotes && m_Sheet != null; }
			set
			{
				if(value != m_DisplayNotes)
				{
					m_DisplayNotes = value;
					NotifyPropertyChanged(() => DisplayNotes);
				}
			}
		}
		public string Notes
		{
			get
			{
				if (m_Sheet != null)
					return m_Sheet.Notes;

				return string.Empty;
			}
		}

		//=============================================================================
		private string m_RackAccessories = string.Empty;
		public string RackAccessories
		{
			get { return m_RackAccessories; }
			set
			{
				if(value != m_RackAccessories)
				{
					m_RackAccessories = value;
					NotifyPropertyChanged(() => RackAccessories);
				}
			}
		}
		//=============================================================================
		private string m_RackLevelAccessories = string.Empty;
		public string RackLevelAccessories
		{
			get { return m_RackLevelAccessories; }
			set
			{
				if (value != m_RackLevelAccessories)
				{
					m_RackLevelAccessories = value;
					NotifyPropertyChanged(() => RackLevelAccessories);
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// Display MHE details block in the right column of PDF sheet
		/// </summary>
		private bool m_DisplayMHEDetails = false;
		public bool DisplayMHEDetails
		{
			get { return m_DisplayMHEDetails; }
			set
			{
				if(value != m_DisplayMHEDetails)
				{
					m_DisplayMHEDetails = value;
					NotifyPropertyChanged(() => DisplayMHEDetails);
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// Display "Important notes on flooring" block in the right column of PDF sheet
		/// </summary>
		private bool m_DisplayImportantNotesOnFlooring = false;
		public bool DisplayImportantNotesOnFlooring
		{
			get { return m_DisplayImportantNotesOnFlooring; }
			set
			{
				if (value != m_DisplayImportantNotesOnFlooring)
				{
					m_DisplayImportantNotesOnFlooring = value;
					NotifyPropertyChanged(() => DisplayImportantNotesOnFlooring);
				}
			}
		}

		//=============================================================================
		public string PickingAisleWidth
		{
			get
			{
				string value = string.Empty;
				if(m_Document != null)
				{
					double pickingAisleWidth = m_Document.PickingAisleWidth;
					if (!double.IsNaN(pickingAisleWidth))
						value = pickingAisleWidth.ToString("0.");
				}

				return value;
			}
		}

		//=============================================================================
		public string CrossAisleWidth
		{
			get
			{
				string value = string.Empty;
				if (m_Document != null)
				{
					double crossAisleWidth = m_Document.CrossAisleWidth;
					if (!double.IsNaN(crossAisleWidth))
						value = crossAisleWidth.ToString("0.");
				}

				return value;
			}
		}

		//=============================================================================
		public string MaxLoadingHeight
		{
			get
			{
				string value = string.Empty;
				if (m_Document != null)
				{
					double maxLoadingHeight = m_Document.MaxLoadingHeight;
					if (!double.IsNaN(maxLoadingHeight))
						value = maxLoadingHeight.ToString("0.");
				}

				return value;
			}
		}

		//=============================================================================
		public string PalletTruckMaxCapacity
		{
			get
			{
				string value = string.Empty;
				if (m_Document != null)
				{
					double maxCapacity = m_Document.PalletTruckMaxCapacity;
					if (!double.IsNaN(maxCapacity))
						value = maxCapacity.ToString("0.");
				}

				return value;
			}
		}

		//=============================================================================
		public string EnbaledMHETypes
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				if(m_Document != null && m_Document.EnabledMHEConfigsList != null)
				{
					bool bAddComma = false;
					foreach(MHEConfiguration config in m_Document.EnabledMHEConfigsList)
					{
						if (config == null)
							continue;

						if(!string.IsNullOrEmpty(config.Type))
						{
							if (bAddComma)
								sb.Append(", ");
							else
								bAddComma = true;
							sb.Append(config.Type);
						}
					}
				}

				return sb.ToString();
			}
		}

		//=============================================================================
		private ObservableCollection<AccessoryVisualPresentation> m_AccessoriesVisualization = new ObservableCollection<AccessoryVisualPresentation>();
		public ObservableCollection<AccessoryVisualPresentation> AccessoriesVisualization
		{
			get { return m_AccessoriesVisualization; }
			set
			{
				m_AccessoriesVisualization = value;
				NotifyPropertyChanged(() => AccessoriesVisualization);
			}
		}

		#endregion

		public void ParseVisualAccessories()
		{
			// Rack Accessories
			if (m_Document != null && m_Document.Rack_Accessories != null)
			{
				if (m_Document.Rack_Accessories.UprightGuard)
					AccessoriesVisualization.Add(new AccessoryVisualPresentation("Upright Guard", "/DrawingFactoryApp;component/Properties/Accessories/RackAccessory_UprightGuard.png"));
				if (m_Document.Rack_Accessories.RowGuard)
				{
					if (m_Document.Rack_Accessories.IsHeavyDutyEnabled)
						AccessoriesVisualization.Add(new AccessoryVisualPresentation("Heavy Duty Row Guard", "/DrawingFactoryApp;component/Properties/Accessories/RackAccessory_RowGuard.png"));
					else
						AccessoriesVisualization.Add(new AccessoryVisualPresentation("Row Guard", "/DrawingFactoryApp;component/Properties/Accessories/RackAccessory_RowGuard_HeavyDuty.png"));
				}
				if (m_Document.Rack_Accessories.Signages)
					AccessoriesVisualization.Add(new AccessoryVisualPresentation("Signages", "/DrawingFactoryApp;component/Properties/Accessories/RackAccessory_Signages.png"));
				if (m_Document.Rack_Accessories.IsMeshCladdingEnabled)
					AccessoriesVisualization.Add(new AccessoryVisualPresentation("Mesh Cladding", "/DrawingFactoryApp;component/Properties/Accessories/RackAccessory_MeshCladding.png"));
			}

			//if (m_Sheet != null && m_Sheet.TieBeamsList != null && m_Sheet.TieBeamsList.Count > 0)
			//	AccessoriesVisualization.Add(new AccessoryVisualPresentation("Tie Beams", "/DrawingFactoryApp;component/Properties/Accessories/.png"));

			List<Tuple<string, string>> usedAccessories = new List<Tuple<string, string>>();

			// Levels Accessories
			foreach (Rack rack in m_Sheet.GetAllRacks())
			{
				if (rack == null)
					continue;
				if (rack.Levels == null)
					continue;

				foreach (RackLevel level in rack.Levels)
				{
					if (level == null)
						continue;
					if (level.Accessories == null)
						continue;

					if (level.Accessories.IsDeckPlateAvailable)
					{
						if (eDeckPlateType.eAlongDepth_UDL == level.Accessories.DeckPlateType)
                        {
                            if (usedAccessories.All(x => x.Item1 != DrawingControl.RackLevelAccessories.DECKING_PANEL_6BP_SHELVING))
                            {
								usedAccessories.Add(new Tuple<string, string>(DrawingControl.RackLevelAccessories.DECKING_PANEL_6BP_SHELVING, "/DrawingFactoryApp;component/Properties/Accessories/LevelAccessory_DeckingPanel_6bpShelving.png"));
							}
						}
						else if (eDeckPlateType.eAlongDepth_PalletSupport == level.Accessories.DeckPlateType)
                        {
							if (usedAccessories.All(x => x.Item1 != DrawingControl.RackLevelAccessories.DECKING_PANEL_6BP_PALLET))
							{
								usedAccessories.Add(new Tuple<string, string>(DrawingControl.RackLevelAccessories.DECKING_PANEL_6BP_PALLET, "/DrawingFactoryApp;component/Properties/Accessories/LevelAccessory_DeckingPanel_6bpPallete.png"));
							}
						}
						else if (eDeckPlateType.eAlongLength == level.Accessories.DeckPlateType)
						{
							if (usedAccessories.All(x => x.Item1 != DrawingControl.RackLevelAccessories.DECKING_PANEL_4BP))
							{
								usedAccessories.Add(new Tuple<string, string>(DrawingControl.RackLevelAccessories.DECKING_PANEL_4BP, "/DrawingFactoryApp;component/Properties/Accessories/LevelAccessory_DeckingPanel_4bp.png"));
							}
						}
					}

					if (level.Accessories.PalletStopper)
					{
						if (usedAccessories.All(x => x.Item1 != DrawingControl.RackLevelAccessories.PALLET_STOPPER))
						{
							usedAccessories.Add(new Tuple<string, string>(DrawingControl.RackLevelAccessories.PALLET_STOPPER, "/DrawingFactoryApp;component/Properties/Accessories/LevelAccessory_PalletStopper.png"));
						}
					}
					if (level.Accessories.ForkEntryBar)
					{
						if (usedAccessories.All(x => x.Item1 != DrawingControl.RackLevelAccessories.FORK_ENTRY_BAR))
						{
							usedAccessories.Add(new Tuple<string, string>(DrawingControl.RackLevelAccessories.FORK_ENTRY_BAR, "/DrawingFactoryApp;component/Properties/Accessories/LevelAccessory_ForkEntryBar.png"));
						}
					}
					if (level.Accessories.PalletSupportBar)
					{
						if (usedAccessories.All(x => x.Item1 != DrawingControl.RackLevelAccessories.PALLET_SUPPORT_BAR))
						{
							usedAccessories.Add(new Tuple<string, string>(DrawingControl.RackLevelAccessories.PALLET_SUPPORT_BAR, "/DrawingFactoryApp;component/Properties/Accessories/LevelAccessory_PSB.png"));
						}
					}
					if (level.Accessories.GuidedTypePalletSupport)
					{
						if (level.Accessories.GuidedTypePalletSupport_WithPSB && level.Accessories.GuidedTypePalletSupport_WithStopper)
						{
							if (usedAccessories.All(x => x.Item1 != DrawingControl.RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER))
							{
								usedAccessories.Add(new Tuple<string, string>(DrawingControl.RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER, "/DrawingFactoryApp;component/Properties/Accessories/LevelAccessory_GSTP_Stopper.png"));
							}
						}
						else if (level.Accessories.GuidedTypePalletSupport_WithPSB)
						{
							if (usedAccessories.All(x => x.Item1 != DrawingControl.RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_PSB))
							{
								usedAccessories.Add(new Tuple<string, string>(DrawingControl.RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_PSB, "/DrawingFactoryApp;component/Properties/Accessories/LevelAccessory_GSTP_PSB.png"));
							}
						}
						else if (level.Accessories.GuidedTypePalletSupport_WithStopper)
						{
							if (usedAccessories.All(x => x.Item1 != DrawingControl.RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER_AND_PSB))
							{
								usedAccessories.Add(new Tuple<string, string>(DrawingControl.RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER_AND_PSB, "/DrawingFactoryApp;component/Properties/Accessories/LevelAccessory_GSTP_StopperAndPSB.png"));
							}
						}
					}

					if (rack.AreLevelsTheSame)
						break;
				}
			}

			// Map found to Visual collection
            foreach (var accessory in usedAccessories)
            {
				AccessoriesVisualization.Add(new AccessoryVisualPresentation(accessory.Item1, accessory.Item2));
			}
		}
	}

	public class AccessoryVisualPresentation : BaseViewModel
	{
        public BitmapImage ImageSource { get; private set; }
        public string Name { get; private set; }

        public AccessoryVisualPresentation(string name, string path)
        {
			Name = name;
			ImageSource = new BitmapImage(new Uri($"{path}", UriKind.RelativeOrAbsolute));
		}

    }
}
