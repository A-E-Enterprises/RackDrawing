using DrawingControl;

namespace RackDrawingApp
{
	public class LicenseWindowViewModel : BaseViewModel
	{
		//=============================================================================
		private string m_strUsername = string.Empty;
		public string UserName
		{
			get { return m_strUsername; }
			set
			{
				if(value != m_strUsername)
				{
					m_strUsername = value;
					NotifyPropertyChanged(() => UserName);
				}
			}
		}

		//=============================================================================
		private string m_strLicenseFilePath = string.Empty;
		public string LicenseFilePath
		{
			get { return m_strLicenseFilePath; }
			set
			{
				if(value != m_strLicenseFilePath)
				{
					m_strLicenseFilePath = value;
					NotifyPropertyChanged(() => LicenseFilePath);
				}
			}
		}

		//=============================================================================
		private bool m_ConnectToServer = true;
		public bool ConnectToServer
		{
			get { return m_ConnectToServer; }
			set
			{
				if(value != m_ConnectToServer)
				{
					m_ConnectToServer = value;
					NotifyPropertyChanged(() => ConnectToServer);
				}
			}
		}

		//=============================================================================
		private string m_strError = string.Empty;
		public string Error
		{
			get { return m_strError; }
			set
			{
				if(value != m_strError)
				{
					m_strError = value;
					NotifyPropertyChanged(() => Error);
				}
			}
		}
	}
}
