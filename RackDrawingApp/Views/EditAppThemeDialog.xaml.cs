using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace RackDrawingApp
{
	/// <summary>
	/// Edit application colors theme dialog
	/// </summary>
	public partial class EditAppThemeDialog : UserControl
	{
		public EditAppThemeDialog(EditAppThemeViewModel vm)
		{
			InitializeComponent();

			m_VM = vm;
			this.DataContext = m_VM;

			if (CurrentTheme.CurrentColorTheme != null)
				CurrentTheme.CurrentColorTheme.ApplyTheme(this.Resources);
		}

		#region Properties

		private EditAppThemeViewModel m_VM = null;

		#endregion

		#region Methods

		//=============================================================================
		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			TextBox tb = sender as TextBox;
			if (tb == null)
				return;

			if (e.Key == Key.Enter)
			{
				BindingExpression be = tb.GetBindingExpression(TextBox.TextProperty);
				if (be != null)
					be.UpdateSource();

				tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));

				//
				e.Handled = true;
				return;
			}

			if (e.Key == Key.Escape)
			{
				BindingExpression be = tb.GetBindingExpression(TextBox.TextProperty);
				if (be != null)
					be.UpdateTarget();

				//
				e.Handled = true;
				return;
			}
		}

		//=============================================================================
		private void OnPickColorButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Button button = sender as Button;
			if (button == null)
				return;

			ColorViewModel vm = button.DataContext as ColorViewModel;
			if (vm == null)
				return;

			System.Windows.Forms.ColorDialog clrDialog = new System.Windows.Forms.ColorDialog();
			clrDialog.Color = System.Drawing.Color.FromArgb(vm.Value.A, vm.Value.R, vm.Value.G, vm.Value.B); ;
			clrDialog.AllowFullOpen = true;
			clrDialog.FullOpen = true;
			if(clrDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				vm.Value = System.Windows.Media.Color.FromArgb(clrDialog.Color.A, clrDialog.Color.R, clrDialog.Color.G, clrDialog.Color.B);
			}
		}

		#endregion

	}
}
