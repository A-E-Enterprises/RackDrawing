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
	/// Interaction logic for SaveChangesDialog.xaml
	/// </summary>
	public partial class SaveChangesDialog : UserControl
	{
		SaveChangesDialog_ViewModel m_vm = null;

		public SaveChangesDialog(SaveChangesDialog_ViewModel vm)
		{
			InitializeComponent();

			m_vm = vm;
			DataContext = m_vm;

			if (CurrentTheme.CurrentColorTheme != null)
				CurrentTheme.CurrentColorTheme.ApplyTheme(this.Resources);
		}
	}
}
