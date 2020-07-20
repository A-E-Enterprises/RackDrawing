using AppColorTheme;
using CommonUtilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace RackDrawingApp_LicenseGenerator
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static string ARG_USERNAME = "/username=";
		private static string ARG_PASSWORD = "/password=";
		private static string ARG_ETHERNET_ADDRESS = "/ethernet_address=";
		private static string ARG_GUID = "/guid="; // optional
		private static string ARG_END_DATE = "/end_date="; // optional
		// platform ID - winver major - winver minor
		// Win32NT - 6 - 0 Windows Vista
		// Win32NT - 6 - 1 Windows 7
		// Win32NT - 6 - 2 Windows 8
		// Win32NT - 6 - 3 Windows 8.1
		// Win32NT - 10 - 0 Windows 10
		private static string ARG_PLATFORM_ID = "/platform_id=";
		private static string ARG_WINVER_MAJOR = "/winver_major=";
		private static string ARG_WINVER_MINOR = "/winver_minor=";
		// 7 - excel 97
		// 8 - excel 98
		// 9 - excel 2000
		// 10 - excel XP
		// 11 - excel 2003
		// 12 - excel 2007
		// 14 - excel 2010
		// 15 - excel 2013
		// 16 - excel 2016
		private static string ARG_EXCEL_VERSION = "/excel_version=";
		private static string ARG_PATH = "/path=";

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			// Apply application theme from TXT file
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "RackDrawingApp_LicenseGenerator.Resources.ApplicationTheme.txt";
			//
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				ColorTheme appTheme = ColorTheme.ReadFromStream(stream);

				// apply theme
				if (appTheme != null)
					CurrentTheme.CurrentColorTheme = appTheme;
			}

			// If arguments are empty then display window.
			// Otherwise try to parse arguments.
			if (e.Args.Count() == 0)
			{
				MainWindow mainWindow = new MainWindow();
				mainWindow.Show();
			}
			else
			{
				string strUsername = string.Empty;
				string strPassword = string.Empty;
				string strEthAddr = string.Empty;
				string strGUID = string.Empty;
				string strEndDate = string.Empty;
				string strPlatformID = string.Empty;
				string strWinMajor = string.Empty;
				string strWinMinor = string.Empty;
				string strExcelVersion = string.Empty;
				string strPath = string.Empty;
				//
				foreach (string strArg in e.Args)
				{
					if (string.IsNullOrEmpty(strArg))
						continue;

					if (strArg.StartsWith(ARG_USERNAME))
						strUsername = strArg.Replace(ARG_USERNAME, string.Empty);
					else if (strArg.StartsWith(ARG_PASSWORD))
						strPassword = strArg.Replace(ARG_PASSWORD, string.Empty);
					else if (strArg.StartsWith(ARG_ETHERNET_ADDRESS))
						strEthAddr = strArg.Replace(ARG_ETHERNET_ADDRESS, string.Empty);
					else if (strArg.StartsWith(ARG_GUID))
						strGUID = strArg.Replace(ARG_GUID, string.Empty);
					else if (strArg.StartsWith(ARG_END_DATE))
						strGUID = strArg.Replace(ARG_END_DATE, string.Empty);
					else if (strArg.StartsWith(ARG_PLATFORM_ID))
						strPlatformID = strArg.Replace(ARG_PLATFORM_ID, string.Empty);
					else if (strArg.StartsWith(ARG_WINVER_MAJOR))
						strWinMajor = strArg.Replace(ARG_WINVER_MAJOR, string.Empty);
					else if (strArg.StartsWith(ARG_WINVER_MINOR))
						strWinMinor = strArg.Replace(ARG_WINVER_MINOR, string.Empty);
					else if (strArg.StartsWith(ARG_EXCEL_VERSION))
						strExcelVersion = strArg.Replace(ARG_EXCEL_VERSION, string.Empty);
					else if (strArg.StartsWith(ARG_PATH))
						strPath = strArg.Replace(ARG_PATH, string.Empty);
				}

				LicenseData licenseData = new LicenseData();
				licenseData.Username = strUsername;
				licenseData.Password = strPassword;
				licenseData.EthernetAddress = strEthAddr;
				licenseData.GUID = strGUID;
				//
				if(string.IsNullOrEmpty(strEndDate))
				{
					licenseData.IncludeDate = false;
				}
				else
				{
					licenseData.IncludeDate = true;
					try
					{
						licenseData.CanRunTill = Convert.ToDateTime(strEndDate);
					}
					catch
					{
						return;
					}
				}
				//
				licenseData.PlatformID = strPlatformID;
				licenseData.WindowsVersionMajor = strWinMajor;
				licenseData.WindowsVersionMinor = strWinMinor;
				licenseData.ExcelVersion = strExcelVersion;

				string strError;
				LicenseUtilities.sCreateLicense(licenseData, strPath, out strError);
				return;
			}
		}
	}
}
