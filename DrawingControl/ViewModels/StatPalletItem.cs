namespace DrawingControl
{
	public class StatPalletItem : BaseViewModel
	{
		public StatPalletItem(int zeroBasedIndex)
		{
			m_ZeroBasedIndex = zeroBasedIndex;
		}

		//=============================================================================
		private int m_ZeroBasedIndex = -1;
		public int ZeroBasedIndex { get { return m_ZeroBasedIndex; } }

		//=============================================================================
		public int DisplayIndex { get { return ZeroBasedIndex + 1; } }

		//=============================================================================
		public string DisplayName
		{
			get
			{
				string strName = "P_";
				strName += DisplayIndex;

				return strName;
			}
		}

		//=============================================================================
		private int m_Length = 0;
		public int Length
		{
			get { return m_Length; }
			set
			{
				if(value != m_Length)
				{
					m_Length = value;
					NotifyPropertyChanged(() => Length);
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
		private int m_Count = 0;
		public int Count
		{
			get { return m_Count; }
			set
			{
				if(value != m_Count)
				{
					m_Count = value;
					NotifyPropertyChanged(() => Count);
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
				if(value != m_Load)
				{
					m_Load = value;
					NotifyPropertyChanged(() => Load);
				}
			}
		}
	}
}
