using CommonUtilities;
using DrawingControl;
using LoginServer;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace RackDrawingApp
{
	/// <summary>
	/// Interaction logic for LoginWindow.xaml
	/// </summary>
	public partial class LoginWindow : Window
	{
		public static string _sRegistryPath = @"HKEY_CURRENT_USER\Software\RackDrawingApp";
		public static string _sLicensePathValue = "LicensePath";
		public static string _sConnectToServerValue = "ConnectToServer";

		public LoginWindow()
		{
			InitializeComponent();

			m_VM = new LicenseWindowViewModel();

			// read settings from registry
			try
			{
				string licensePath = Registry.GetValue(_sRegistryPath, _sLicensePathValue, string.Empty).ToString();
				m_VM.LicenseFilePath = licensePath;

				string strConnectToServer = Registry.GetValue(_sRegistryPath, _sConnectToServerValue, "True").ToString();
				m_VM.ConnectToServer = Convert.ToBoolean(strConnectToServer);
			}
			catch { }

			DataContext = m_VM;

			if (CurrentTheme.CurrentColorTheme != null)
				CurrentTheme.CurrentColorTheme.ApplyTheme(this.Resources);
		}

		//=============================================================================
		private LicenseWindowViewModel m_VM = null;

		//=============================================================================
		private void SelectLicenseFileButton_Click(object sender, RoutedEventArgs e)
		{
			if (m_VM == null)
				return;

			string strFilePath = FileUtils._OpenFileDialog("License files |*.lic", ".lic", null);
			m_VM.LicenseFilePath = strFilePath;
		}

		//=============================================================================
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			// save settings to registry
			if (m_VM == null)
				return;

			try
			{
				Registry.SetValue(_sRegistryPath, _sLicensePathValue, m_VM.LicenseFilePath);
				Registry.SetValue(_sRegistryPath, _sConnectToServerValue, m_VM.ConnectToServer);
			}
			catch { }
		}

		//=============================================================================
		private void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			if (m_VM == null)
				return;

			string strLicenseFilePath = string.Empty;
			if (m_VM.ConnectToServer)
			{
				try
				{
					// Try to connect to the login server.
					using (WebClient webClient = new WebClient())
					{
						webClient.QueryString.Add(LoginServerData.PARAM_USERNAME, m_VM.UserName);
						webClient.QueryString.Add(LoginServerData.PARAM_PASSWORD, _PasswordBox.Password.ToString());

						// Try to create license file in the assembly directory.
						string assemblyFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
						strLicenseFilePath = assemblyFolder + "\\RackDrawingAppLicense.lic";

						//
						webClient.DownloadFile(LoginServerData.LOGIN_SERVER_URI, strLicenseFilePath);
					}
				}
				catch(Exception exception)
				{
					m_VM.Error = exception.Message;
					return;
				}
			}
			else
			{
				strLicenseFilePath = m_VM.LicenseFilePath;
			}

			if (string.IsNullOrEmpty(strLicenseFilePath))
			{
				if (m_VM.ConnectToServer)
					m_VM.Error = "An error occurred while downloading the license file";
				else
					m_VM.Error = "License file is not selected.";
				return;
			}

			if (!File.Exists(strLicenseFilePath))
			{
				if (m_VM.ConnectToServer)
					m_VM.Error = "An error occurred while downloading the license file";
				else
					m_VM.Error = "License file doesnt exists.";
				return;
			}

			string strError;
			int iRes = LicenseUtilities.IsValidLicense(strLicenseFilePath, m_VM.UserName, _PasswordBox.Password.ToString(), out strError);
			if (iRes >= 0)
			{
				UserInfo.Login = m_VM.UserName;

				StartupWindow startupWnd = new StartupWindow();
				if (!string.IsNullOrEmpty(UserInfo.DrawingPath))
					startupWnd.StartOldApp();
				else
					startupWnd.Show();
				this.Close();
			}
			else
				m_VM.Error = strError;
		}

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			using (Process.Start(e.Uri.ToString()))
			{
				e.Handled = true;
			}
		}
	}
}
