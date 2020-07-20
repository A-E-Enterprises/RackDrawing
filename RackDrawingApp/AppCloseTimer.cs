using System;
using System.Threading.Tasks;

namespace RackDrawingApp
{
	public static class AppCloseTimer
	{
		//=============================================================================
		public static System.Timers.Timer m_AppCloseTimer = null;

		//=============================================================================
		// Application shutdown delegate.
		public delegate void AppShutdownDelegate();
		private static AppShutdownDelegate m_AppShutdownDelegate = null;

		//=============================================================================
		// Before application shutdown delegate.
		public delegate Task<object> BeforeShutdownDelegate();
		private static BeforeShutdownDelegate m_BeforeShutdownDelegate = null;
		//=============================================================================
		public static void SetBeforeShutdownDelegate(BeforeShutdownDelegate beforeShutdownDelegate)
		{
			m_BeforeShutdownDelegate = beforeShutdownDelegate;
		}

		//=============================================================================
		public static void Initialize(AppShutdownDelegate appShutdownDelegate)
		{
			// Application should be closed after 24 hours run.
			//m_AppCloseTimer = new System.Timers.Timer(10 * 1000);
			m_AppCloseTimer = new System.Timers.Timer(24 * 60 * 60 * 1000);
			m_AppCloseTimer.Elapsed += _AppCloseTimer_Elapsed;
			m_AppCloseTimer.Start();

			m_AppShutdownDelegate = appShutdownDelegate;
		}

		//=============================================================================
		private static void _AppCloseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			m_AppCloseTimer.Stop();

			//await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
			//{
			//	m_BeforeShutdownDelegate()
			//	.ContinueWith(new Action<Task>(task =>
			//	{
			//		if (m_AppShutdownDelegate != null)
			//			m_AppShutdownDelegate();
			//	}));
			//}));

			System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
			{
				if (m_BeforeShutdownDelegate != null)
				{
					m_BeforeShutdownDelegate()
					.ContinueWith(new Action<Task>(task =>
					{
						if (m_AppShutdownDelegate != null)
							m_AppShutdownDelegate();
					}));
				}
				else
				{
					System.Windows.MessageBox.Show("Application session is limited by 24 hours. Application will be closed.");

					if (m_AppShutdownDelegate != null)
						m_AppShutdownDelegate();
				}
			}));
		}
	}
}
