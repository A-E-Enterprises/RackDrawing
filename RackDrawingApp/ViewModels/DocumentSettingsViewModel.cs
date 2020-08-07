using DrawingControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace RackDrawingApp
{
	public class DocumentSettingsViewModel : BaseViewModel
	{
		public DocumentSettingsViewModel(DrawingDocument doc)
		{
			m_Document = doc;

			if (doc != null)
			{
				m_RacksPalletsType = doc.RacksPalletType;
				m_RacksPalletsOverhangValue = doc.RacksPalletsOverhangValue;
				//
				m_Currency = doc.Currency;
				m_Rate = doc.Rate;
				m_Discount = doc.Discount;
				//
				if (doc.PalletConfigurationCollection != null)
				{
					foreach (PalletConfiguration palletConfig in doc.PalletConfigurationCollection)
					{
						if (palletConfig == null)
							continue;

						m_PalletConfigurationCollection.Add(palletConfig);
					}
				}
				//
				if(doc.MHEConfigurationsColl != null)
				{
					foreach(MHEConfiguration mheConfig in doc.MHEConfigurationsColl)
					{
						if (mheConfig == null)
							continue;

						m_MHEConfigsCollection.Add(mheConfig);
					}
				}

				// create collection view
				m_PalletConfigurationCollView = CollectionViewSource.GetDefaultView(m_PalletConfigurationCollection);
				// add sorting
				if (m_PalletConfigurationCollView != null)
					m_PalletConfigurationCollView.SortDescriptions.Add(new SortDescription("UniqueIndex", ListSortDirection.Ascending));

				IsPrintRackElevations = doc.IsPrintRackElevations;
				IsPrintAllRackElevationsInSinglePage = doc.IsPrintAllRackElevationsInSinglePage;

				PrintingSheetElevationMaxLength = doc.PrintingSheetElevationMaxLength;
				PrintingSheetElevationMaxHeight = doc.PrintingSheetElevationMaxHeight;
			}
		}

		//=============================================================================
		private DrawingDocument m_Document = null;

		//=============================================================================
		// Drives rack's pallets depth relative to the rack's depth.
		private ePalletType m_RacksPalletsType = ePalletType.eOverhang;
		public ePalletType RacksPalletsType
		{
			get { return m_RacksPalletsType; }
			set
			{
				m_RacksPalletsType = value;
				NotifyPropertyChanged(() => RacksPalletsType);
			}
		}
		//
		//=============================================================================
		private List<string> m_PalletTypesList = new List<string>() { Rack.PALLET_FLUSH, Rack.PALLET_OVERHANG };
		public List<string> PalletTypesList { get { return m_PalletTypesList; } }
		//=============================================================================
		// Drives pallets overhang value.
		// Pallet depth = rack depth + 2 * overhang.
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
		//
		private List<double> m_RacksPalletsOverhangValuesList = new List<double>()
		{
			25.0,
			50.0,
			75.0,
			100.0,
			125.0,
			150.0,
			175.0
		};
		public List<double> RacksPalletsOverhangValuesList { get { return m_RacksPalletsOverhangValuesList; } }

		//=============================================================================
		private string m_Currency = string.Empty;
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
					m_Rate = Math.Round(value, 2);

				NotifyPropertyChanged(() => Rate);
			}
		}
		//=============================================================================
		private double m_Discount = 1.0;
		public double Discount
		{
			get { return m_Discount; }
			set
			{
				if (value != m_Discount)
					m_Discount = Math.Round(value, 2);

				NotifyPropertyChanged(() => Discount);
			}
		}

		//=============================================================================
		private bool m_IsPrintRackElevations;
		/// <summary>
		/// Configuration to print in pdf rack elevations
		/// </summary>
		public bool IsPrintRackElevations
		{
			get { return m_IsPrintRackElevations; }
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
			get { return m_IsPrintAllRackElevationsInSinglePage; }
			set
			{
				m_IsPrintAllRackElevationsInSinglePage = value;
				NotifyPropertyChanged(() => IsPrintAllRackElevationsInSinglePage);
			}
		}

		private int m_printing_sheet_elevation_max_length;
		/// <summary>
		///	Configuration to combine racks with same index in single page
		/// </summary>
		public int PrintingSheetElevationMaxLength
		{
			get { return m_printing_sheet_elevation_max_length; }
			set
			{
				m_printing_sheet_elevation_max_length = value;
				NotifyPropertyChanged(() => PrintingSheetElevationMaxLength);
			}
		}

		private int m_printing_sheet_elevation_max_height;
		/// <summary>
		///	Configuration to combine racks with same index in single page
		/// </summary>
		public int PrintingSheetElevationMaxHeight
		{
			get { return m_printing_sheet_elevation_max_height; }
			set
			{
				m_printing_sheet_elevation_max_height = value;
				NotifyPropertyChanged(() => PrintingSheetElevationMaxHeight);
			}
		}

		//=============================================================================
		// Collection with MHEConfigurationm which drives aisle space min length\width,
		// topmost beam height etc.
		private ObservableCollection<MHEConfiguration> m_MHEConfigsCollection = new ObservableCollection<MHEConfiguration>();
		public ObservableCollection<MHEConfiguration> MHEConfigurationsCollection { get { return m_MHEConfigsCollection; } }

		//=============================================================================
		// Collection with PalletConfigurations, which can be used in the racks.
		private ObservableCollection<PalletConfiguration> m_PalletConfigurationCollection = new ObservableCollection<PalletConfiguration>();
		public ObservableCollection<PalletConfiguration> PalletConfigurationCollection { get { return m_PalletConfigurationCollection; } }

		//=============================================================================
		// View of m_PalletConfigurationCollection.
		// Allow to add collection sorting.
		private ICollectionView m_PalletConfigurationCollView = null;
		public ICollectionView PalletConfigurationCollView { get { return m_PalletConfigurationCollView; } }

		//=============================================================================
		private ICommand m_DeletePalletConfigCommand = new Command_DeletePalletConfiguration();
		public ICommand DeletePalletConfigCommand { get { return m_DeletePalletConfigCommand; } }
		//=============================================================================
		private ICommand m_RestorePalletConfigCommand = new Command_RestorePalletConfiguration();
		public ICommand RestorePalletConfigCommand { get { return m_RestorePalletConfigCommand; } }
	}
}
