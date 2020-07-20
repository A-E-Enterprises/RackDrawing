using AppColorTheme;

namespace RackDrawingApp_LicenseGenerator
{
	public static class CurrentTheme
	{
		/// <summary>
		/// Current color theme instance
		/// </summary>
		private static ColorTheme m_CurrentColorTheme = new DefaultLightTheme();
		public static ColorTheme CurrentColorTheme
		{
			get { return m_CurrentColorTheme; }
			set
			{
				m_CurrentColorTheme = value;
				if (m_CurrentColorTheme != null)
					m_CurrentColorTheme.ApplyTheme(System.Windows.Application.Current.Resources);
			}
		}
	}
}
