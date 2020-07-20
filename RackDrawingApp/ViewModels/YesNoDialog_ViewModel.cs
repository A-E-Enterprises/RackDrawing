using DrawingControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RackDrawingApp
{
	public class YesNoDialog_ViewModel : BaseViewModel, IYesNoCancelViewModel
	{
		public YesNoDialog_ViewModel(string strText)
		{
			Text = strText;
		}

		//=============================================================================
		private bool m_RememberTheChoice = false;
		public bool RememberTheChoice
		{
			get { return m_RememberTheChoice; }
			set
			{
				if(value != m_RememberTheChoice)
				{
					m_RememberTheChoice = value;
					NotifyPropertyChanged(() => RememberTheChoice);
				}
			}
		}

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
	}
}
