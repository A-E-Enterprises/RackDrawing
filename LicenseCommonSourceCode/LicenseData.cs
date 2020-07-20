using System;
using System.Collections.Generic;

namespace CommonUtilities
{
	public class LicenseData
	{
		public LicenseData()
		{
			this.Username = string.Empty;
			this.Password = string.Empty;
			this.EthernetAddress = string.Empty;
			this.GUID = string.Empty;
			this.IncludeDate = true;
			this.CanRunTill = DateTime.Today;

			OperatingSystem os = Environment.OSVersion;
			this.PlatformID = os.Platform.ToString();
			this.WindowsVersionMajor = os.Version.Major.ToString();
			this.WindowsVersionMinor = os.Version.Minor.ToString();

			int iExcelVersion = LicenseUtilities.GetMajorVersion(LicenseUtilities.GetComponentPath(LicenseUtilities.OfficeComponent.Excel));
			if (iExcelVersion > 0)
				this.ExcelVersion = iExcelVersion.ToString();
		}

		//
		public string Username { get; set; }
		//
		public string Password { get; set; }
		//
		public string EthernetAddress { get; set; }
		//
		public string GUID { get; set; }
		//
		public bool IncludeDate { get; set; }
		//
		public DateTime CanRunTill { get; set; }

		// windows platform id
		public string PlatformID { get; set; }
		public string WindowsVersionMajor { get; set; }
		public string WindowsVersionMinor { get; set; }
		//
		public string ExcelVersion { get; set; }


		// Available windows version list
		public static List<WindowsVersion> WindowsVersionsList = new List<WindowsVersion>()
		{
			// Vista
			new WindowsVersion(System.PlatformID.Win32NT, new Version(6, 0)),
			// 7
			new WindowsVersion(System.PlatformID.Win32NT, new Version(6, 1)),
			// 8
			new WindowsVersion(System.PlatformID.Win32NT, new Version(6, 2)),
			// 8.1
			new WindowsVersion(System.PlatformID.Win32NT, new Version(6, 3)),
			// 10
			new WindowsVersion(System.PlatformID.Win32NT, new Version(10, 0))
		};
		// Available excel versions list
		public static List<ExcelVersion> ExcelVersionList = new List<ExcelVersion>()
		{
			new ExcelVersion(7),
			new ExcelVersion(8),
			new ExcelVersion(9),
			new ExcelVersion(10),
			new ExcelVersion(11),
			new ExcelVersion(12),
			new ExcelVersion(14),
			new ExcelVersion(15),
			new ExcelVersion(16),
		};
	}

	public class WindowsVersion
	{
		public WindowsVersion(System.PlatformID plID, Version version)
		{
			PlatformID = plID;
			Version = version;
		}

		public string DisplayName
		{
			get
			{
				if(PlatformID.Win32NT == this.PlatformID)
				{
					if(6 == this.Version.Major)
					{
						if (0 == this.Version.Minor)
							return "Vista";
						else if (1 == this.Version.Minor)
							return "7";
						else if (2 == this.Version.Minor)
							return "8";
						else if (3 == this.Version.Minor)
							return "8.1";
					}
					else if(10 == this.Version.Major)
					{
						if (0 == this.Version.Minor)
							return "10";
					}
				}

				return PlatformID.ToString() + " " + Version.ToString();
			}
		}
		//
		public PlatformID PlatformID { get; set; }
		//
		public Version Version { get; set; }
	}

	public class ExcelVersion
	{
		public ExcelVersion(int excelVersion)
		{
			this.Version = excelVersion;
		}

		public string DisplayName
		{
			get
			{
				if (7 == this.Version)
					return "Excel 97";
				else if (8 == this.Version)
					return "Excel 98";
				else if (9 == this.Version)
					return "Excel 2000";
				else if (10 == this.Version)
					return "Excel XP";
				else if (11 == this.Version)
					return "Excel 2003";
				else if (12 == this.Version)
					return "Excel 2007";
				else if (14 == this.Version)
					return "Excel 2010";
				else if (15 == this.Version)
					return "Excel 2013";
				else if (16 == this.Version)
					return "Excel 2016";

				return "Excel version " + Version.ToString();
			}
		}

		public int Version { get; set; }
	}
}