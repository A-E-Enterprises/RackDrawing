using DrawingControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RackDrawingApp
{
	public class SaveChangesDialog_ViewModel : BaseViewModel
	{
		public SaveChangesDialog_ViewModel() { }

		//=============================================================================
		private string m_strText = string.Empty;
		public string Text
		{
			get { return m_strText; }
			set
			{
				m_strText = value;
				NotifyPropertyChanged(() => Text);
			}
		}

		//=============================================================================
		private bool m_bIsSaveButtonVisible = true;
		public bool IsSaveButtonVisible
		{
			get { return m_bIsSaveButtonVisible; }
			set
			{
				if (m_bIsSaveButtonVisible != value)
				{
					m_bIsSaveButtonVisible = value;
					NotifyPropertyChanged(() => IsSaveButtonVisible);
				}
			}
		}

		//=============================================================================
		private bool m_bIsCancelButtonVisible = true;
		public bool IsCancelButtonVisible
		{
			get { return m_bIsCancelButtonVisible; }
			set
			{
				if (m_bIsCancelButtonVisible != value)
				{
					m_bIsCancelButtonVisible = value;
					NotifyPropertyChanged(() => m_bIsCancelButtonVisible);
				}
			}
		}
	}
}
