using System;
using System.IO;
using System.Windows;

namespace DrawingControl
{
	public static class FileUtils
	{
		public static string DEFAULT_SAVE_DIRECTORY = @"C:\DrawingFactory\";

		//=============================================================================
		public static string _SaveFileDialog(string strFilter, string strExtension, string strEnqNo, string preferredFileName = null, string strExistingFilePath = null)
		{
			string strFilePath = null;

			string strFileName = string.Empty;
			string strDirectory = string.Empty;
			if (!string.IsNullOrEmpty(strExistingFilePath))
			{
				strFileName = Path.GetFileName(strExistingFilePath);
				strDirectory = Path.GetDirectoryName(strExistingFilePath);
				if (!Directory.Exists(strDirectory))
					strDirectory = string.Empty;
			}

			if (!string.IsNullOrEmpty(preferredFileName))
				strFileName = preferredFileName;

			string strDefDir = BuildDefaultDirectory(strEnqNo);

			if (string.IsNullOrEmpty(strDirectory))
				strDirectory = strDefDir;

			// Create SaveFileDialog
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

			// Set filter for file extension and default file extension
			dlg.DefaultExt = strExtension;
			dlg.Filter = strFilter;
			dlg.FileOk += SaveFileDialog_FileOK;

			if (!string.IsNullOrEmpty(strFileName))
				dlg.FileName = strFileName;
			if (!string.IsNullOrEmpty(strDirectory))
				dlg.InitialDirectory = strDirectory;

			// Display SaveFileDialog by calling ShowDialog method
			Nullable<bool> result = dlg.ShowDialog();

			// Get the selected file name and display in a TextBox
			if (result == true)
				strFilePath = dlg.FileName;

			return strFilePath;
		}

		//=============================================================================
		private static void SaveFileDialog_FileOK(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// Dont restrict save file directory.
			return;

			Microsoft.Win32.SaveFileDialog dlg = sender as Microsoft.Win32.SaveFileDialog;
			if (dlg == null)
				return;

			string userChoice = Path.GetDirectoryName(dlg.FileName);
			if (!userChoice.StartsWith(DEFAULT_SAVE_DIRECTORY))
			{
				MessageBox.Show("You should save your file in the folder: " + DEFAULT_SAVE_DIRECTORY);
				e.Cancel = true;
			}
		}

		//=============================================================================
		public static string BuildDefaultDirectory(string strEnqNo)
		{
			string strDefDir = FileUtils.DEFAULT_SAVE_DIRECTORY;
			if (!string.IsNullOrEmpty(strEnqNo))
				strDefDir += strEnqNo;

			// Check that SAVE_DIRECTORY exists.
			// If it doesnt exist then create it.
			try
			{
				if (!Directory.Exists(strDefDir))
					Directory.CreateDirectory(strDefDir);
			}
			catch { }

			return strDefDir;
		}

		//=============================================================================
		public static string _OpenFileDialog(string strFilter, string strExtension, string strExistingFileName)
		{
			string strFilePath = null;

			string strFileName = string.Empty;
			string strDirectory = string.Empty;
			if (!string.IsNullOrEmpty(strExistingFileName))
			{
				strFileName = Path.GetFileName(strExistingFileName);
				strDirectory = Path.GetDirectoryName(strExistingFileName);
				if (!Directory.Exists(strDirectory))
					strDirectory = string.Empty;
			}

			// Create SaveFileDialog
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			// Set filter for file extension and default file extension
			dlg.DefaultExt = strExtension;
			dlg.Filter = strFilter;
			dlg.Multiselect = false;

			if (!string.IsNullOrEmpty(strFileName))
				dlg.FileName = strFileName;
			if (!string.IsNullOrEmpty(strDirectory))
				dlg.InitialDirectory = strDirectory;

			// Display OpenFileDialog by calling ShowDialog method
			Nullable<bool> result = dlg.ShowDialog();

			// Get the selected file name and display in a TextBox
			if (result == true)
				strFilePath = dlg.FileName;

			return strFilePath;
		}
	}
}
