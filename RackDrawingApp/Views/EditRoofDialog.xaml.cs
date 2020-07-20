using System.Windows.Controls;

namespace RackDrawingApp
{
	/// <summary>
	/// Interaction logic for EditRoofDialog.xaml
	/// </summary>
	public partial class EditRoofDialog : UserControl
	{
		public EditRoofDialog(EditRoofDialogViewModel vm)
		{
			InitializeComponent();

			m_VM = vm;
			this.DataContext = m_VM;

			if (CurrentTheme.CurrentColorTheme != null)
				CurrentTheme.CurrentColorTheme.ApplyTheme(this.Resources);
		}

		//=============================================================================
		private EditRoofDialogViewModel m_VM = null;
	}
}
