using AppInterfaces;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace AppColorTheme
{
	/// <summary>
	/// IGeomteryColorsTheme implementation
	/// </summary>
	public class GeometryColorTheme : IGeometryColorsTheme
	{
		#region Properties

		/// <summary>
		/// Dictionary with color type - color value pairs.
		/// </summary>
		private Dictionary<eColorType, Color> m_ColorsDictionary = new Dictionary<eColorType, Color>();

		#endregion

		#region Methods

		public bool GetGeometryColor(eColorType colorType, out Color colorValue)
		{
			colorValue = Colors.Black;

			if(m_ColorsDictionary != null && m_ColorsDictionary.ContainsKey(colorType))
			{
				colorValue = m_ColorsDictionary[colorType];
				return true;
			}

			return false;
		}

		public void SetGeometryColor(eColorType colorType, Color colorValue)
		{
			if (m_ColorsDictionary == null)
				m_ColorsDictionary = new Dictionary<eColorType, Color>();

			m_ColorsDictionary[colorType] = colorValue;
		}

		public IClonable Clone()
		{
			GeometryColorTheme cloneInstance = new GeometryColorTheme();

			if(m_ColorsDictionary != null)
			{
				foreach(eColorType colorType in m_ColorsDictionary.Keys)
				{
					cloneInstance.SetGeometryColor(colorType, m_ColorsDictionary[colorType]);
				}
			}

			return cloneInstance;
		}

		#endregion
	}
}
