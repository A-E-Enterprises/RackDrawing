using System.Windows.Controls;

namespace RackDrawingApp
{
	/// <summary>
	/// Interaction logic for PlaceSheetDialog.xaml
	/// </summary>
	public partial class PlaceSheetDialog : UserControl
	{
		public PlaceSheetDialog(PlaceSheetDialogVM vm)
		{
			InitializeComponent();

			m_VM = vm;
			this.DataContext = m_VM;

			if (CurrentTheme.CurrentColorTheme != null)
				CurrentTheme.CurrentColorTheme.ApplyTheme(this.Resources);
		}

		private PlaceSheetDialogVM m_VM = null;
	}
}
