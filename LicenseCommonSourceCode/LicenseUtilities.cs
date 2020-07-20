using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;

namespace CommonUtilities
{
	internal partial class LicenseUtilities
	{
		public static int IsValidLicense(string filePath, string username, string password, out string strError)
		{
			strError = string.Empty;

			if (!System.IO.File.Exists(filePath))
			{
				strError = "License file missing";
				return -1;
			}

			//
			// 
			//
			LicenseData licData = Utilities.Deserialize<LicenseData>(filePath, true);

			// Check etherned address
			if (!IsValidEthernetAddress(licData))
			{
				strError = "Invalid ethernet address";
				return -1;
			}

			// Check username and password
			if(licData.Username != username || licData.Password != password)
			{
				strError = "Username\\password is not valid";
				return -1;
			}

			// Check GUID
			if (licData.GUID != "")
			{
				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

				System.Runtime.InteropServices.GuidAttribute[] attributes = (System.Runtime.InteropServices.GuidAttribute[])assembly.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), true);
				if (attributes.Length > 0)
				{
					System.Runtime.InteropServices.GuidAttribute attribute = (System.Runtime.InteropServices.GuidAttribute)assembly.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), true)[0];
					string id = attribute.Value;
					if (licData.GUID.ToLower() != id.ToLower())
					{
						strError = "GUID mismatch";
						return -1;
					}
				}
			}


			//if (!licData.CADFormatLock.Contains(CADFormatWithVersion))
			//{
			//	strError = "Wrong CAD format/version";
			//	return -1;
			//}

			// check windows version
			OperatingSystem os = Environment.OSVersion;
			if(string.IsNullOrEmpty(licData.PlatformID) || string.IsNullOrEmpty(licData.WindowsVersionMajor) || string.IsNullOrEmpty(licData.WindowsVersionMinor))
			{
				strError = "License file doesnt contain windows version data.";
				return -1;
			}
			if(os.Platform.ToString() != licData.PlatformID || os.Version.Major.ToString() != licData.WindowsVersionMajor || os.Version.Minor.ToString() != licData.WindowsVersionMinor)
			{
				strError = "Your windows version doesnt match permitted windows version in the license file.";
				return -1;
			}

			// check installed excel
			int excelVersion_Installed = GetMajorVersion(GetComponentPath(OfficeComponent.Excel));
			if (string.IsNullOrEmpty(licData.ExcelVersion))
			{
				strError = "License file doesnt contain excel version data.";
				return -1;
			}
			int iExcelVersion_LicenseData = 0;
			try
			{
				iExcelVersion_LicenseData = Convert.ToInt32(licData.ExcelVersion);
			}
			catch
			{
				strError = "License file contains not a number excel version data.";
				return -1;
			}
			// Something is wrong with excel version checking.
			// C:\\Program Files\\WindowsApps\\Microsoft.Office.Desktop.Excel_16051.11601.20230.0_x86__8wekyb3d8bbwe\\Office16\\EXCEL.exe
			// Cant find iExcelVersion = 16.
			//if (!LicenseUtilities.CheckOfficeVersion(OfficeComponent.Excel, iExcelVersion))
			if(excelVersion_Installed != iExcelVersion_LicenseData)
			{
				strError = "Your installed excel version doesnt match permitted excel version in the license file.";
				return -1;
			}

			// Check date
			if (licData.IncludeDate)
			{
				int dateDiff = (licData.CanRunTill - System.DateTime.Today).Days;
				if (dateDiff >= 0)
				{
					return dateDiff + 1;
				}
				else
				{
					strError = "License validity expired";
					return -1;
				}
			}
			else
			{
				return 0;
			}
		}
		public static bool IsValidEthernetAddress(LicenseData licData)
		{
			if (licData == null)
				return false;

			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			if (nics == null || nics.Length < 1)
				return false;

			// For DEBUG
			//List<string> adaptersList = new List<string>();
			//foreach (NetworkInterface adapter in nics)
			//{
			//	if (adapter == null)
			//		continue;
			//
			//	string adapterInfo = adapter.NetworkInterfaceType.ToString();
			//	adapterInfo += "; ";
			//	adapterInfo += adapter.Name;
			//	adapterInfo += "; ";
			//	adapterInfo += adapter.OperationalStatus.ToString();
			//	adapterInfo += "; ";
			//	adapterInfo += adapter.GetPhysicalAddress();
			//
			//	adaptersList.Add(adapterInfo);
			//}

			foreach (NetworkInterface adapter in nics)
			{
				// need to check both Ethernet and WiFi adapters, because some users doesnt have Ethernet adapter at all
				if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet && adapter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
					continue;

				PhysicalAddress address = adapter.GetPhysicalAddress();
				byte[] bytes = address.GetAddressBytes();
				string addressString = "";
				for (int i = 0; i < bytes.Length; i++)
				{
					// Formats the physical address in hexadecimal.
					addressString += bytes[i].ToString("X2");
					// Insert a hyphen after each byte, unless we are at the end of the address.
					if (i != bytes.Length - 1)
					{
						addressString += "-";
					}
				}

				if (licData.EthernetAddress.ToLower() == addressString.ToLower())
					return true;
			}
			return false;
		}

		public enum OfficeComponent
		{
			Word,
			Excel,
			PowerPoint,
			Outlook
		}
		public static string GetComponentPath(OfficeComponent _component)
		{
			const string RegKey = @"Software\Microsoft\Windows\CurrentVersion\App Paths";
		
			string toReturn = string.Empty;
			string _key = string.Empty;

			switch (_component)
			{
				case OfficeComponent.Word:
					_key = "winword.exe";
					break;
				case OfficeComponent.Excel:
					_key = "excel.exe";
					break;
				case OfficeComponent.PowerPoint:
					_key = "powerpnt.exe";
					break;
				case OfficeComponent.Outlook:
					_key = "outlook.exe";
					break;
			}

			//looks inside CURRENT_USER:
			RegistryKey _mainKey = Registry.CurrentUser;
			try
			{
				_mainKey = _mainKey.OpenSubKey(RegKey + "\\" + _key, false);
				if (_mainKey != null)
				{
					toReturn = _mainKey.GetValue(string.Empty).ToString();
				}
			}
			catch
			{ }

			//if not found, looks inside LOCAL_MACHINE:
			_mainKey = Registry.LocalMachine;
			if (string.IsNullOrEmpty(toReturn))
			{
				try
				{
					_mainKey = _mainKey.OpenSubKey(RegKey + "\\" + _key, false);
					if (_mainKey != null)
					{
						toReturn = _mainKey.GetValue(string.Empty).ToString();
					}
				}
				catch
				{ }
			}

			//closing the handle:
			if (_mainKey != null)
				_mainKey.Close();

			return toReturn;
		}
		public static int GetMajorVersion(string _path)
		{
			int toReturn = 0;
			if (File.Exists(_path))
			{
				try
				{
					FileVersionInfo _fileVersion = FileVersionInfo.GetVersionInfo(_path);
					toReturn = _fileVersion.FileMajorPart;
				}
				catch
				{ }
			}
			return toReturn;
		}
		private static bool CheckOfficeVersion(OfficeComponent _component, int version)
		{
			string strRegKey = @"SOFTWARE\Microsoft\Office\";
			strRegKey += version.ToString(".0").Replace(',', '.');
			strRegKey += @"\";

			switch (_component)
			{
				case OfficeComponent.Word:
					strRegKey += "Word";
					break;
				case OfficeComponent.Excel:
					strRegKey += "Excel";
					break;
				case OfficeComponent.PowerPoint:
					strRegKey += "PowerPoint";
					break;
				case OfficeComponent.Outlook:
					strRegKey += "Outlook";
					break;
			}

			strRegKey += @"\InstallRoot";

			RegistryKey r = Registry.LocalMachine.OpenSubKey(strRegKey);
			//
			if (r == null)
				r = Registry.CurrentUser.OpenSubKey(strRegKey);
			// check 64bit
			if (r == null)
			{
				using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
				{
					r = hklm.OpenSubKey(strRegKey);
				}
			}
			if (r != null)
			{
				object val = r.GetValue("Path");
				r.Dispose();
				if(val != null)
					return true;
			}
			return false;
		}
	}
}