using System.Windows.Controls;

namespace RackDrawingApp
{
	/// <summary>
	/// Interaction logic for ExportLayoutTemplate.xaml
	/// </summary>
	public partial class ExportLayoutTemplate : UserControl
	{
		public ExportLayoutTemplate(ExportLayoutTemplateVM vm)
		{
			InitializeComponent();

			m_VM = vm;
			this.DataContext = m_VM;
		}

		//=============================================================================
		private ExportLayoutTemplateVM m_VM = null;
	}
}
