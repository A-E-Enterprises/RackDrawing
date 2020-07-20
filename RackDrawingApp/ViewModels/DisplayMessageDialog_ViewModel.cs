using DrawingControl;

namespace RackDrawingApp
{
	public class DisplayMessageDialog_ViewModel : BaseViewModel
	{
		public DisplayMessageDialog_ViewModel(string strMessage)
		{
			Message = strMessage;
		}

		//=============================================================================
		private string m_strMessage = string.Empty;
		public string Message
		{
			get { return m_strMessage; }
			set
			{
				m_strMessage = value;
				NotifyPropertyChanged(() => Message);
			}
		}
	}
}
