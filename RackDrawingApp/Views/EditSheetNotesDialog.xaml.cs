using System.Windows.Controls;

namespace RackDrawingApp
{
	/// <summary>
	/// Interaction logic for EditSheetNotesDialog.xaml
	/// </summary>
	public partial class EditSheetNotesDialog : UserControl
	{
		public EditSheetNotesDialog(EditSheetNotesDailogViewModel vm)
		{
			InitializeComponent();

			m_VM = vm;
			this.DataContext = m_VM;

			if (CurrentTheme.CurrentColorTheme != null)
				CurrentTheme.CurrentColorTheme.ApplyTheme(this.Resources);
		}

		#region Properties

		/// <summary>
		/// Dialog view model
		/// </summary>
		private EditSheetNotesDailogViewModel m_VM = null;

		#endregion
	}
}
