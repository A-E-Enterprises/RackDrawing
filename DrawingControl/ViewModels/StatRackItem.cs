using AppColorTheme;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DrawingControl
{
	public class StatRackItem : BaseViewModel
	{
		public StatRackItem(int iRackIndex, ObservableCollection<StatRackItem> ownerCollection)
		{
			m_iRackIndex = iRackIndex;
			m_OwnerColl = ownerCollection;
		}

		//=============================================================================
		private ObservableCollection<StatRackItem> m_OwnerColl = null;

		//=============================================================================
		// zero-based
		private int m_iRackIndex = -1;
		public int RackIndex { get { return m_iRackIndex; } }

		//=============================================================================
		public int DisplayIndex { get { return m_OwnerColl.IndexOf(this) + 1; } }

		//=============================================================================
		public string Name_M
		{
			get
			{
				string strName_M = RackUtils.GetAlphabetRackIndex(m_iRackIndex + 1);
				strName_M += "(M)";
				return strName_M;
			}
		}

		//=============================================================================
		public string Name_A
		{
			get
			{
				string strName_A = RackUtils.GetAlphabetRackIndex(m_iRackIndex + 1);
				strName_A += "(A)";
				return strName_A;
			}
		}

		//=============================================================================
		private int m_RacksCount_M = 0;
		public int Count_M
		{
			get { return m_RacksCount_M; }
			set
			{
				if(value != m_RacksCount_M)
				{
					m_RacksCount_M = value;
					if (m_RacksCount_M < 0)
						m_RacksCount_M = 0;
					NotifyPropertyChanged(() => Count_M);
				}
			}
		}

		//=============================================================================
		private int m_RacksCount_A = 0;
		public int Count_A
		{
			get { return m_RacksCount_A; }
			set
			{
				if (value != m_RacksCount_A)
				{
					m_RacksCount_A = value;
					if (m_RacksCount_A < 0)
						m_RacksCount_A = 0;
					NotifyPropertyChanged(() => Count_A);
				}
			}
		}

		//=============================================================================
		public Color BackgroundColor
		{
			get
			{
				if (CurrentGeometryColorsTheme.CurrentTheme != null)
				{
					eColorType colorType = CurrentGeometryColorsTheme.RackToFillColorType(m_iRackIndex);
					Color colorValue;
					if (eColorType.eUndefined != colorType && CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(colorType, out colorValue))
						return colorValue;
				}

				if (m_iRackIndex >= 0)
					return RackColors.GetColor(m_iRackIndex);

				return Colors.Black;
			}
		}

		//=============================================================================
		public Color ForegroundColor
		{
			get
			{
				if (CurrentGeometryColorsTheme.CurrentTheme != null)
				{
					Color colorValue;
					if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eGeometryTextColor, out colorValue))
						return colorValue;
				}

				return Colors.Black;
			}
		}

		//=============================================================================
		private int m_Length_M = 0;
		public int Length_M
		{
			get { return m_Length_M; }
			set
			{
				if(value != m_Length_M)
				{
					m_Length_M = value;
					NotifyPropertyChanged(() => Length_M);
				}
			}
		}
		//=============================================================================
		private int m_Length_A = 0;
		public int Length_A
		{
			get { return m_Length_A; }
			set
			{
				if (value != m_Length_A)
				{
					m_Length_A = value;
					NotifyPropertyChanged(() => Length_A);
				}
			}
		}

		//=============================================================================
		private int m_Width = 0;
		public int Width
		{
			get { return m_Width; }
			set
			{
				if(value != m_Width)
				{
					m_Width = value;
					NotifyPropertyChanged(() => Width);
				}
			}
		}

		//=============================================================================
		private int m_Height = 0;
		public int Height
		{
			get { return m_Height; }
			set
			{
				if(value != m_Height)
				{
					m_Height = value;
					NotifyPropertyChanged(() => Height);
				}
			}
		}

		//=============================================================================
		private int m_Load = 0;
		public int Load
		{
			get { return m_Load; }
			set
			{
				if (value != m_Load)
				{
					m_Load = value;
					NotifyPropertyChanged(() => Load);
				}
			}
		}

		//=============================================================================
		public void Update_DisplayIndex()
		{
			NotifyPropertyChanged(() => DisplayIndex);
		}
	}
}
