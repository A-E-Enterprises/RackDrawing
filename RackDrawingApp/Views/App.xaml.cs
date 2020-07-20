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

namespace RackDrawingApp
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static string ARG_LOGIN = "/login=";
		private static string ARG_PASSWORD = "/password=";
		private static string ARG_LICENSE = "/license=";
		private static string ARG_CUSTOMER_NAME = "/customer_name=";
		private static string ARG_ENQ_NO = "/enq_no=";
		private static string ARG_CONTACT_NO = "/contact_no=";
		private static string ARG_EMAIL_ID = "/email_id=";
		private static string ARG_BILLING_ADDRESS = "/billing_address=";
		private static string ARG_SITE_ADDRESS = "/site_address=";
		private static string ARG_DRAWING_PATH = "/drawing_path=";

		//=============================================================================
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			// Apply application theme from TXT file
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "RackDrawingApp.Resources.ApplicationTheme.txt";
			//
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				ColorTheme appTheme = ColorTheme.ReadFromStream(stream);

				// apply theme
				if(appTheme != null)
					CurrentTheme.CurrentColorTheme = appTheme;
			}

			// Application should be closed after 24 hours run.
			AppCloseTimer.Initialize(AppShutdown);

			bool bShowLoginWindows = true;
			// If some arguments are passed, then try to get login, password and license path.
			if(e.Args.Count() > 0)
			{
				string strLogin = string.Empty;
				string strPassword = string.Empty;
				string strLicensePath = string.Empty;
				string strCutomerName = string.Empty;
				string strEnqNo = string.Empty;
				string strContactNo = string.Empty;
				string strEmailID = string.Empty;
				string strBillingAddress = string.Empty;
				string strSiteAddress = string.Empty;
				string strDrawingPath = string.Empty;
				//
				foreach(string strArg in e.Args)
				{
					if (string.IsNullOrEmpty(strArg))
						continue;

					if(strArg.StartsWith(ARG_LOGIN))
						strLogin = strArg.Replace(ARG_LOGIN, string.Empty);
					else if(strArg.StartsWith(ARG_PASSWORD))
						strPassword = strArg.Replace(ARG_PASSWORD, string.Empty);
					else if (strArg.StartsWith(ARG_LICENSE))
						strLicensePath = strArg.Replace(ARG_LICENSE, string.Empty);
					else if (strArg.StartsWith(ARG_CUSTOMER_NAME))
						strCutomerName = strArg.Replace(ARG_CUSTOMER_NAME, string.Empty);
					else if (strArg.StartsWith(ARG_ENQ_NO))
						strEnqNo = strArg.Replace(ARG_ENQ_NO, string.Empty);
					else if (strArg.StartsWith(ARG_CONTACT_NO))
						strContactNo = strArg.Replace(ARG_CONTACT_NO, string.Empty);
					else if (strArg.StartsWith(ARG_EMAIL_ID))
						strEmailID = strArg.Replace(ARG_EMAIL_ID, string.Empty);
					else if (strArg.StartsWith(ARG_BILLING_ADDRESS))
						strBillingAddress = strArg.Replace(ARG_BILLING_ADDRESS, string.Empty);
					else if (strArg.StartsWith(ARG_SITE_ADDRESS))
						strSiteAddress = strArg.Replace(ARG_SITE_ADDRESS, string.Empty);
					else if (strArg.StartsWith(ARG_DRAWING_PATH))
						strDrawingPath = strArg.Replace(ARG_DRAWING_PATH, string.Empty);
				}

				UserInfo.Login = strLogin;
				UserInfo.CustomerName = strCutomerName;
				UserInfo.EnqNo = strEnqNo;
				UserInfo.CustomerContactNo = strContactNo;
				UserInfo.CustomerEmailID = strEmailID;
				UserInfo.CustomerBillingAddress = strBillingAddress;
				UserInfo.CustomerSiteAddress = strSiteAddress;
				UserInfo.DrawingPath = strDrawingPath;
				if (!string.IsNullOrEmpty(strLicensePath))
				{
					string strError;
					int iRes = LicenseUtilities.IsValidLicense(strLicensePath, strLogin, strPassword, out strError);
					if (iRes >= 0)
					{
						bShowLoginWindows = false;
					}
				}
			}

			if (bShowLoginWindows)
			{
				LoginWindow loginWnd = new LoginWindow();
				loginWnd.Show();
			}
			else
			{
				StartupWindow startupWnd = new StartupWindow();
				if (!string.IsNullOrEmpty(UserInfo.DrawingPath))
					startupWnd.StartOldApp();
				else
					startupWnd.Show();
			}
		}

		//=============================================================================
		private void AppShutdown()
		{
			this.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Normal);
		}
	}
}
