using CommonUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RackDrawingApp_LicenseDebugger
{
	public class MainWindowViewModel : BaseViewModel
	{
		public MainWindowViewModel() { }

		//=============================================================================
		private LicenseData m_LicenseData = new LicenseData();
		public LicenseData License
		{
			get { return m_LicenseData; }
			set
			{
				m_LicenseData = value;
				//
				NotifyPropertyChanged(() => UserName);
				NotifyPropertyChanged(() => Password);
				NotifyPropertyChanged(() => EthernetAddress);
				NotifyPropertyChanged(() => GUID);
				NotifyPropertyChanged(() => IncludeDate);
				NotifyPropertyChanged(() => CanRunTill);
				NotifyPropertyChanged(() => WindowsVersion);
				NotifyPropertyChanged(() => ExcelVersion);
			}
		}

		//=============================================================================
		public string UserName
		{
			get { return m_LicenseData.Username; }
		}

		//=============================================================================
		public string Password
		{
			get { return m_LicenseData.Password; }
		}

		//=============================================================================
		public string EthernetAddress
		{
			get { return m_LicenseData.EthernetAddress; }
		}

		//=============================================================================
		public string GUID
		{
			get { return m_LicenseData.GUID; }
		}

		//=============================================================================
		public bool IncludeDate
		{
			get { return m_LicenseData.IncludeDate; }
		}

		//=============================================================================
		public DateTime CanRunTill
		{
			get { return m_LicenseData.CanRunTill; }
		}

		//=============================================================================
		public WindowsVersion WindowsVersion
		{
			get
			{
				WindowsVersion founded = windowsVersionsList.Find(
					w =>
					w.PlatformID.ToString() == m_LicenseData.PlatformID
					&& w.Version.Major.ToString() == m_LicenseData.WindowsVersionMajor
					&& w.Version.Minor.ToString() == m_LicenseData.WindowsVersionMinor
					);
				if (founded == null)
				{
					try
					{
						System.PlatformID plID = (System.PlatformID)Enum.Parse(typeof(System.PlatformID), m_LicenseData.PlatformID);
						int iMajor = Convert.ToInt32(m_LicenseData.WindowsVersionMajor);
						int iMinor = Convert.ToInt32(m_LicenseData.WindowsVersionMinor);
						founded = new WindowsVersion(plID, new Version(iMajor, iMinor));
						windowsVersionsList.Add(founded);
					}
					catch { }
				}

				return founded;
			}
			set
			{
				int iMajor = 0;
				int iMinor = 0;
				System.PlatformID plID = PlatformID.Win32NT;

				if (value != null)
				{
					iMajor = value.Version.Major;
					iMinor = value.Version.Minor;
					plID = value.PlatformID;
				}

				m_LicenseData.PlatformID = plID.ToString();
				m_LicenseData.WindowsVersionMajor = iMajor.ToString();
				m_LicenseData.WindowsVersionMinor = iMajor.ToString();

				NotifyPropertyChanged(() => WindowsVersion);
			}
		}
		//=============================================================================
		public List<WindowsVersion> windowsVersionsList { get { return LicenseData.WindowsVersionsList; } }

		//=============================================================================
		public ExcelVersion ExcelVersion
		{
			get
			{
				ExcelVersion founded = ExcelVersionsList.Find(e => e.Version.ToString() == m_LicenseData.ExcelVersion);
				if (founded == null)
				{
					try
					{
						int iVersion = Convert.ToInt32(m_LicenseData.ExcelVersion);
						founded = new ExcelVersion(iVersion);
						ExcelVersionsList.Add(founded);
					}
					catch { }
				}

				return founded;
			}
			set
			{
				int iVersion = 0;

				if (value != null)
					iVersion = value.Version;

				m_LicenseData.ExcelVersion = iVersion.ToString();

				NotifyPropertyChanged(() => ExcelVersion);
			}
		}
		//=============================================================================
		public List<ExcelVersion> ExcelVersionsList { get { return LicenseData.ExcelVersionList; } }

		//=============================================================================
		private string m_strLicenseFilePath = string.Empty;
		public string LicenseFilePath
		{
			get { return m_strLicenseFilePath; }
			set
			{
				if (value != m_strLicenseFilePath)
				{
					string strPath = string.Empty;
					if (value != null)
						strPath = value;

					m_strLicenseFilePath = strPath.Trim();
					NotifyPropertyChanged(() => LicenseFilePath);
				}
			}
		}

		//=============================================================================
		private string m_strStatus = string.Empty;
		public string Status
		{
			get { return m_strStatus; }
			set
			{
				if (value != Status)
				{
					m_strStatus = value;
					NotifyPropertyChanged(() => Status);
				}
			}
		}

		//=============================================================================
		private bool m_DoesStatusContainsError = false;
		public bool DoesStatusContainsError
		{
			get { return m_DoesStatusContainsError; }
			set
			{
				if (value != m_DoesStatusContainsError)
				{
					m_DoesStatusContainsError = value;
					NotifyPropertyChanged(() => DoesStatusContainsError);
				}
			}
		}
	}
}
