using DrawingControl;
using System.Windows.Controls;
using System.Windows.Media;

namespace RackDrawingApp
{
	/// <summary>
	/// Interaction logic for ExportLayoutTemplate02_Sheet02.xaml
	/// </summary>
	public partial class ExportLayoutTemplate02_Sheet02 : UserControl
	{
		public ExportLayoutTemplate02_Sheet02(ExportLayoutTemplateVM vm, ImageSource watermarkImage)
		{
			InitializeComponent();

			//
			m_WatermarkImage = watermarkImage;
			//
			m_WatermarkVisual = new WatermarkVisual(this);
			this.AddLogicalChild(m_WatermarkVisual);
			this.AddVisualChild(m_WatermarkVisual);

			m_VM = vm;
			this.DataContext = m_VM;
		}

		//=============================================================================
		private ExportLayoutTemplateVM m_VM = null;

		//=============================================================================
		// Visual that draws watermark image over parent control.
		// Add it as a child to this control.
		private WatermarkVisual m_WatermarkVisual = null;

		//=============================================================================
		// Watermark images which will be drawn over current control.
		private ImageSource m_WatermarkImage = null;

		//=============================================================================
		protected override int VisualChildrenCount
		{
			get
			{
				int iCount = base.VisualChildrenCount;

				// watermark
				iCount += 1;

				return iCount;
			}
		}

		//=============================================================================
		protected override Visual GetVisualChild(int index)
		{
			if (index < base.VisualChildrenCount)
				return base.GetVisualChild(index);

			return m_WatermarkVisual;
		}

		//=============================================================================
		/// <summary>
		/// Override OnRender and draw watermark image over control.
		/// </summary>
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			if (m_WatermarkVisual != null && m_WatermarkImage != null)
				m_WatermarkVisual.Draw(m_WatermarkImage);
		}
	}
}