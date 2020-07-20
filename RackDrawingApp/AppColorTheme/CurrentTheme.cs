using AppColorTheme;
using DrawingControl;

namespace RackDrawingApp
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
				// apply geometry colors theme
				if (m_CurrentColorTheme.GeometryColorsTheme != null)
					CurrentGeometryColorsTheme.CurrentTheme = m_CurrentColorTheme.GeometryColorsTheme;
			}
		}
	}
}
