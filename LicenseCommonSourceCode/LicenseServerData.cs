namespace LicenseServer
{
	public static class LicenseServerData
	{
		// URI of LicenseGeneratorServer
		//public static string LICENSE_GENERATOR_SERVER_URI = "http://127.0.0.1:7143/";
		//public static string LICENSE_GENERATOR_SERVER_URI = "http://localhost:7143/";
		//
		// Only this URL works on the local network. But the application should have administrator rights.
		// https://stackoverflow.com/questions/9459656/difference-between-http-8080-and-http-8080/9459679#9459679
		public static string LICENSE_GENERATOR_SERVER_URI = "http://*:7143/";

		// Parameters for GET request to the LicenseGeneratorServer
		public static string PARAM_USERNAME = "username";
		public static string PARAM_PASSWORD = "password";
		public static string PARAM_ETHERNET_ADDRESS = "ethernet_address";
		public static string PARAM_GUID = "guid"; // optional
		public static string PARAM_END_DATE = "end_date"; // optional
		// platform ID - winver major - winver minor
		// Win32NT - 6 - 0 Windows Vista
		// Win32NT - 6 - 1 Windows 7
		// Win32NT - 6 - 2 Windows 8
		// Win32NT - 6 - 3 Windows 8.1
		// Win32NT - 10 - 0 Windows 10
		public static string PARAM_PLATFORM_ID = "platform_id";
		public static string PARAM_WINVER_MAJOR = "winver_major";
		public static string PARAM_WINVER_MINOR = "winver_minor";
		// 7 - excel 97
		// 8 - excel 98
		// 9 - excel 2000
		// 10 - excel XP
		// 11 - excel 2003
		// 12 - excel 2007
		// 14 - excel 2010
		// 15 - excel 2013
		// 16 - excel 2016
		public static string PARAM_EXCEL_VERSION = "excel_version";
		//public static string PARAM_PATH = "path";
	}
}