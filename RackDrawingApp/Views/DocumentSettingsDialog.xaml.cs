using DrawingControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
	/// Interaction logic for DocumentSettingsDialog.xaml
	/// </summary>
	public partial class DocumentSettingsDialog : UserControl
	{
		public DocumentSettingsDialog(DocumentSettingsViewModel vm)
		{
			InitializeComponent();

			m_VM = vm;
			this.DataContext = m_VM;

			if (CurrentTheme.CurrentColorTheme != null)
				CurrentTheme.CurrentColorTheme.ApplyTheme(this.Resources);
		}

		//=============================================================================
		private DocumentSettingsViewModel m_VM = null;

		//=============================================================================
		private void AddNewPalletConfig_ButtonClick(object sender, RoutedEventArgs e)
		{
			if (m_VM == null)
				return;

			int index = Utils.GetUniquePalletConfigurationIndex(m_VM.PalletConfigurationCollection);
			if (index < 1)
				return;

			m_VM.PalletConfigurationCollection.Add(new PalletConfiguration(index));
		}

		//=============================================================================
		private void TextBox_PreviewNumericTextInput(object sender, TextCompositionEventArgs e)
		{
			// enable only numeric input
			e.Handled = !IsTextAllowed(e.Text);
		}
		//=============================================================================
		private void TextBoxNumericPasting(object sender, DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(typeof(String)))
			{
				String text = (String)e.DataObject.GetData(typeof(String));
				if (!IsTextAllowed(text))
				{
					e.CancelCommand();
				}
			}
			else
			{
				e.CancelCommand();
			}
		}

		//=============================================================================
		private static readonly Regex _regex = new Regex("[^0-9.,-]+"); //regex that matches disallowed text
		private static bool IsTextAllowed(string text)
		{
			return !_regex.IsMatch(text);
		}
	}
}
