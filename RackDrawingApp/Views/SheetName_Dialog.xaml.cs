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
	/// Interaction logic for SheetName_Dialog.xaml
	/// </summary>
	public partial class SheetName_Dialog : UserControl
	{
		private SheetNameDialog_ViewModel m_VM = null;

		public SheetName_Dialog(SheetNameDialog_ViewModel vm)
		{
			InitializeComponent();

			m_VM = vm;
			DataContext = m_VM;

			if (CurrentTheme.CurrentColorTheme != null)
				CurrentTheme.CurrentColorTheme.ApplyTheme(this.Resources);
		}
	}
}
