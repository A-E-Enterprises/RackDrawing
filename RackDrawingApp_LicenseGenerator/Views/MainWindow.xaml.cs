using CommonUtilities;
using LicenseServer;
using System.Linq;
using System.Net;
using System.Windows;

namespace RackDrawingApp_LicenseGenerator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			DataContext = m_VM;
		}

		//=============================================================================
		private MainWindowViewModel m_VM = new MainWindowViewModel();

		//=============================================================================
		private void CreateButton_Click(object sender, RoutedEventArgs e)
		{
			if (m_VM == null)
				return;

			if (m_VM.License == null)
				return;

			// Try to get create license via license server.
			// For test only.
			if (false)
			{
				WebClient webClient = new WebClient();
				webClient.QueryString.Add(LicenseServerData.PARAM_USERNAME, m_VM.License.Username);
				webClient.QueryString.Add(LicenseServerData.PARAM_PASSWORD, m_VM.License.Password);
				webClient.QueryString.Add(LicenseServerData.PARAM_ETHERNET_ADDRESS, m_VM.License.EthernetAddress);
				webClient.QueryString.Add(LicenseServerData.PARAM_GUID, m_VM.License.GUID);
				webClient.QueryString.Add(LicenseServerData.PARAM_PLATFORM_ID, m_VM.License.PlatformID);
				webClient.QueryString.Add(LicenseServerData.PARAM_WINVER_MAJOR, m_VM.License.WindowsVersionMajor);
				webClient.QueryString.Add(LicenseServerData.PARAM_WINVER_MINOR, m_VM.License.WindowsVersionMinor);
				webClient.QueryString.Add(LicenseServerData.PARAM_EXCEL_VERSION, m_VM.License.ExcelVersion);

				webClient.DownloadFile(LicenseServerData.LICENSE_GENERATOR_SERVER_URI, "D:\\LicenseTest.lic");
			}

			string strPath = System.IO.Path.Combine(m_VM.LicenseFileDirectory.Trim(), m_VM.LicenseFileName.Trim());
			string strError;
			bool bError = !LicenseUtilities.sCreateLicense(m_VM.License, strPath, out strError);

			if(bError)
			{
				m_VM.DoesStatusContainsError = true;
				m_VM.Status = strError;
			}
			else
			{
				m_VM.DoesStatusContainsError = false;
				m_VM.Status = "File successfully created.";
			}
		}

		//=============================================================================
		private void ChooseDirectoryButton_Click(object sender, RoutedEventArgs e)
		{
			if (m_VM == null)
				return;

			using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
			{
				dialog.Description = "RackDrawingApp license file folder";

				System.Windows.Forms.DialogResult result = dialog.ShowDialog();
				if(result == System.Windows.Forms.DialogResult.OK)
				{
					m_VM.LicenseFileDirectory = dialog.SelectedPath;
				}
			}
		}

		//=============================================================================
		private void Window_Drop(object sender, DragEventArgs e)
		{
			if (m_VM == null)
				return;

			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// Note that you can have more than one file.
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				//
				if(files.Count() > 0)
				{
					string strFilePath = files[0];

					if (!System.IO.File.Exists(strFilePath))
					{
						m_VM.DoesStatusContainsError = true;
						m_VM.Status = "Dropped file doesnt exist.";
						return;
					}

					//
					try
					{
						LicenseData licData = Utilities.Deserialize<LicenseData>(strFilePath, true);

						if(licData == null)
						{
							m_VM.DoesStatusContainsError = true;
							m_VM.Status = "Error occurred while extracting data from the license file.";
							return;
						}

						m_VM.License = licData;
						m_VM.LicenseFileDirectory = System.IO.Path.GetDirectoryName(strFilePath);
						m_VM.LicenseFileName = System.IO.Path.GetFileName(strFilePath);
					}
					catch
					{
						m_VM.DoesStatusContainsError = true;
						m_VM.Status = "Error occurred while extracting data from the license file.";
						return;
					}
				}
			}
		}
	}
}
