using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RackDrawingApp
{
	/// <summary>
	/// Interaction logic for YesNo_Dialog.xaml
	/// </summary>
	public partial class YesNo_Dialog : UserControl
	{
		YesNoDialog_ViewModel m_VM = null;

		public YesNo_Dialog(YesNoDialog_ViewModel vm)
		{
			InitializeComponent();

			m_VM = vm;
			DataContext = m_VM;

			if (CurrentTheme.CurrentColorTheme != null)
				CurrentTheme.CurrentColorTheme.ApplyTheme(this.Resources);
		}
	}
}
