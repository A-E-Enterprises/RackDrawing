using DrawingControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RackDrawingApp
{
	public class ExportLayoutTemplateVM : BaseViewModel
	{
		public ExportLayoutTemplateVM(DrawingDocument doc, DrawingSheet sheet)
		{
			m_Document = doc;
			m_Sheet = sheet;
		}

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
		// If true then need to display racks and pallets statistics
		private bool m_bDisplayStatistics = true;
		public bool DisplayStatistics
		{
			get { return m_bDisplayStatistics && m_Sheet != null; }
			set
			{
				if(value != m_bDisplayStatistics)
				{
					m_bDisplayStatistics = value;
					NotifyPropertyChanged(() => DisplayStatistics);
				}
			}
		}

		//=============================================================================
		private bool m_bDisplayNotesAndAccessoryDetails = true;
		public bool DisplayNotesAndAccessoryDetails
		{
			get { return m_bDisplayNotesAndAccessoryDetails; }
			set
			{
				if(value != m_bDisplayNotesAndAccessoryDetails)
				{
					m_bDisplayNotesAndAccessoryDetails = value;
					NotifyPropertyChanged(() => DisplayNotesAndAccessoryDetails);
				}
			}
		}

		//=============================================================================
		public bool RackStatisticsEmpty
		{
			get
			{
				if (m_Sheet != null && m_Sheet.RackStatistics != null)
					return m_Sheet.RackStatistics.Count == 0;

				return true;
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
		public bool PalletStatisticsEmpty
		{
			get
			{
				if (m_Sheet != null && m_Sheet.PalletsStatistics != null)
					return m_Sheet.PalletsStatistics.Count == 0;

				return true;
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
		private bool m_DisplayAccessory = false;
		public bool DisplayAccessory
		{
			get { return m_DisplayAccessory; }
			set
			{
				if (value != m_DisplayAccessory)
				{
					m_DisplayAccessory = value;
					NotifyPropertyChanged(() => DisplayAccessory);
				}
			}
		}
		private string m_Accessory = string.Empty;
		public string Accessory
		{
			get { return m_Accessory; }
			set
			{
				if(value != m_Accessory)
				{
					m_Accessory = value;
					NotifyPropertyChanged(() => Accessory);
				}
			}
		}
	}
}
