using DrawingControl;

namespace RackDrawingApp
{
	public class SheetNameDialog_ViewModel : BaseViewModel
	{
		DrawingSheet m_CurrSheet = null;
		public SheetNameDialog_ViewModel(DrawingSheet currSheet)
		{
			m_CurrSheet = currSheet;

			IsOkButtonEnabled = _IsNameUnique;
		}

		//=============================================================================
		private string m_Name = string.Empty;
		public string Name
		{
			get { return m_Name; }
			set
			{
				if(m_Name != value)
				{
					m_Name = value;
					if (m_Name == null)
						m_Name = string.Empty;

					IsOkButtonEnabled = _IsNameUnique;

					NotifyPropertyChanged(() => Name);
				}
			}
		}

		//=============================================================================
		private bool m_bIsOKButtonEnabled = true;
		public bool IsOkButtonEnabled
		{
			get { return m_bIsOKButtonEnabled; }
			set
			{
				if(m_bIsOKButtonEnabled != value)
				{
					m_bIsOKButtonEnabled = value;
					NotifyPropertyChanged(() => IsOkButtonEnabled);
				}
			}
		}

		//=============================================================================
		private bool _IsNameUnique
		{
			get
			{
				if (m_CurrSheet == null || m_CurrSheet.Document == null || m_CurrSheet.Document.Sheets == null)
					return true;

				foreach(DrawingSheet _sheet in m_CurrSheet.Document.Sheets)
				{
					if (_sheet == null)
						continue;

					if (_sheet == m_CurrSheet)
						continue;

					if (_sheet.Name == m_Name)
						return false;
				}

				return true;
			}
		}
	}
}
