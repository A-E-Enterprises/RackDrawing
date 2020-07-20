using DrawingControl;
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
	/// Interaction logic for CustomerInfoDialog.xaml
	/// </summary>
	public partial class CustomerInfoDialog : UserControl
	{
		public CustomerInfoDialog(DrawingDocument doc)
		{
			InitializeComponent();

			m_DrawingDoc = doc;
			this.DataContext = m_DrawingDoc;

			if (CurrentTheme.CurrentColorTheme != null)
				CurrentTheme.CurrentColorTheme.ApplyTheme(this.Resources);
		}

		// Displayed document data
		private DrawingDocument m_DrawingDoc = null;
	}
}
