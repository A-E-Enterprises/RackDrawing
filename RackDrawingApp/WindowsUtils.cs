using System;
using System.Runtime.InteropServices;

namespace RackDrawingApp
{
	public static class WindowsUtils
	{
		public const int GWL_STYLE = -16;
		public const int WS_MAXIMIZEBOX = 0x10000; //maximize button
		public const int WS_MINIMIZEBOX = 0x20000; //minimize button

		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		/// <summary>
		/// Disable Maximize button on the window.
		/// </summary>
		public static void DisableMaximizeButton(IntPtr wndHandle)
		{
			if (wndHandle == null)
				return;

			SetWindowLong(wndHandle, GWL_STYLE, GetWindowLong(wndHandle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
		}

		/// <summary>
		/// Enable Minimize button on the window.
		/// </summary>
		public static void EnableMinimizeButton(IntPtr wndHandle)
		{
			if (wndHandle == IntPtr.Zero)
				return;

			SetWindowLong(wndHandle, GWL_STYLE, GetWindowLong(wndHandle, GWL_STYLE) | WS_MINIMIZEBOX);
		}
	}
}
