using System;
using System.Windows;
using System.Windows.Interop;

namespace RackDrawingApp
{
	/// <summary>
	/// Interaction logic for StartupWindow.xaml
	/// </summary>
	public partial class StartupWindow : Window
	{
		public StartupWindow()
		{
			InitializeComponent();

			this.SourceInitialized += StartupWindow_SourceInitialized;

			if (CurrentTheme.CurrentColorTheme != null)
				CurrentTheme.CurrentColorTheme.ApplyTheme(this.Resources);
		}

		//=============================================================================
		private IntPtr m_WndHandle = IntPtr.Zero;

		//=============================================================================
		private void StartupWindow_SourceInitialized(object sender, EventArgs e)
		{
			//
			m_WndHandle = new WindowInteropHelper(this).Handle;
			WindowsUtils.DisableMaximizeButton(m_WndHandle);
			WindowsUtils.EnableMinimizeButton(m_WndHandle);

			//
			WindowInteropHelper helper = new WindowInteropHelper(this);
			HwndSource source = HwndSource.FromHwnd(helper.Handle);
			source.AddHook(WndProc);
		}

		//=============================================================================
		const int WM_SYSCOMMAND = 0x0112;
		const int SC_MOVE = 0xF010;
		const int SC_RESTORE = 0xF120;
		//=============================================================================
		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case WM_SYSCOMMAND:
					int command = wParam.ToInt32() & 0xfff0;
					if (command == SC_MOVE)
					{
						// prevent user from moving the window
						handled = true;
					}
					else if (command == SC_RESTORE && WindowState == WindowState.Maximized)
					{
						// prevent user from restoring the window while it is maximized
						// (but allow restoring when it is minimized)
						handled = true;
					}
					break;
				default:
					break;
			}
			return IntPtr.Zero;
		}

		//=============================================================================
		private void PalletRackButton_Click(object sender, RoutedEventArgs e)
		{
			StartOldApp();
		}

		//=============================================================================
		private void LongSpanButton_Click(object sender, RoutedEventArgs e)
		{
			//StartOldApp();
		}

		//=============================================================================
		private void HeavyDutyShelvingButton_Click(object sender, RoutedEventArgs e)
		{
			//StartOldApp();
		}

		//=============================================================================
		public void StartOldApp()
		{
			MainWindow_ViewModel vm = new MainWindow_ViewModel();

			// Check - is view model correct initialized.
			string strError = string.Empty;
			if (vm == null
				|| vm.CurrentDocument == null
				|| vm.CurrentDocument.RacksColumnsList == null
				|| vm.CurrentDocument.RacksColumnsList.Count == 0
				|| vm.CurrentDocument.BeamsList == null
				|| vm.CurrentDocument.BeamsList.Count == 0)
			{
				strError = "Error occurred during apllication initializing. Application will be closed.";
			}

			if (string.IsNullOrEmpty(strError))
			{
				MainWindow mainWnd = new MainWindow(vm);
				mainWnd.Show();
			}
			else
			{
				MessageBox.Show(strError, "Error");
			}

			this.Close();
		}
	}
}
