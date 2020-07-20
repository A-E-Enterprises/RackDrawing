using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DrawingControl
{
	public static class RackColors
	{
		// rack size index - rack fill brush
		private static Dictionary<int, Color> m_IndexToColor = new Dictionary<int, Color>()
		{
			{ 0, Colors.YellowGreen },
			{ 1, Colors.Violet },
			{ 2, Colors.Thistle },
			{ 3, Colors.Tomato },
			{ 4, Colors.SlateBlue },
			{ 5, Colors.SkyBlue },
			{ 6, Colors.Salmon },
			{ 7, Colors.RosyBrown },
			{ 8, Colors.Plum },
			{ 9, Colors.Pink },
			{ 10, Colors.PaleVioletRed },
			{ 11, Colors.PaleGreen },
			{ 12, Colors.PaleTurquoise },
			{ 13, Colors.PaleGoldenrod },
			{ 14, Colors.Olive },
			{ 15, Colors.LightCoral },
			{ 16, Colors.Chocolate },
			{ 17, Colors.LightSteelBlue }
		};

		//=============================================================================
		public static Color GetColor(int iRackSizeIndex)
		{
			if (m_IndexToColor.ContainsKey(iRackSizeIndex))
				return m_IndexToColor[iRackSizeIndex];
			else
			{
				int iLastKey = m_IndexToColor.Keys.Last();
				return m_IndexToColor[iLastKey];
			}
		}
	}
}
