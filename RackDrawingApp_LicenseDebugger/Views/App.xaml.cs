using AppColorTheme;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace RackDrawingApp_LicenseDebugger
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// Apply application theme from TXT file
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "RackDrawingApp_LicenseDebugger.Resources.ApplicationTheme.txt";
			//
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				ColorTheme appTheme = ColorTheme.ReadFromStream(stream);

				// apply theme
				if (appTheme != null)
					CurrentTheme.CurrentColorTheme = appTheme;
			}
		}
	}
}
