using CommonUtilities;
using System;
using System.Linq;
using System.Windows;

namespace RackDrawingApp_LicenseDebugger
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
		private void SelectLicenseButton_Click(object sender, RoutedEventArgs e)
		{
			if (m_VM == null)
				return;

			string strFilePath = string.Empty;
			// Create SaveFileDialog
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			// Set filter for file extension and default file extension
			dlg.DefaultExt = ".lic";
			dlg.Filter = "License files |*.lic";
			dlg.Multiselect = false;

			// Display OpenFileDialog by calling ShowDialog method
			Nullable<bool> result = dlg.ShowDialog();

			// Get the selected file name and display in a TextBox
			if (result == true)
				strFilePath = dlg.FileName;

			if (string.IsNullOrEmpty(strFilePath))
				return;

			_OpenLicense(strFilePath);
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
				if (files.Count() > 0)
				{
					string strFilePath = files[0];

					_OpenLicense(strFilePath);
				}
			}
		}

		private void _OpenLicense(string strFilePath)
		{
			if(string.IsNullOrEmpty(strFilePath))
			{
				m_VM.DoesStatusContainsError = true;
				m_VM.Status = "License file path is empty.";
				return;
			}

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

				if (licData == null)
				{
					m_VM.DoesStatusContainsError = true;
					m_VM.Status = "Error occurred while extracting data from the license file.";
					return;
				}

				m_VM.License = licData;
				m_VM.LicenseFilePath = strFilePath;
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
