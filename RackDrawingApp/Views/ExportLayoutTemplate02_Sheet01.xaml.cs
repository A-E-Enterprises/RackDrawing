using DrawingControl;
using System.Windows.Controls;
using System.Windows.Media;

namespace RackDrawingApp
{
	/// <summary>
	/// Interaction logic for ExportLayoutTemplate_02.xaml
	/// </summary>
	public partial class ExportLayoutTemplate02_Sheet01 : UserControl
	{
		public ExportLayoutTemplate02_Sheet01(ExportLayoutTemplateVM vm, ImageSource watermarkImage)
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

			this.DrawingPart.Content = new Views.ExportTemplate_GeneralLayout();
		}

		#region Properties

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

		//=============================================================================
		/// <summary>
		/// Height of rack statistics block height.
		/// It is used for calculate how many blocks can be placed at the PDF sheet.
		/// </summary>
		public double RackStatBlockHeight
		{
			get
			{
				if (RackStatGrid != null)
					return RackStatGrid.ActualHeight + RackStatGrid.Margin.Top + RackStatGrid.Margin.Bottom;

				return 0;
			}
		}
		//=============================================================================
		/// <summary>
		/// Height of pallete statistics block height.
		/// It is used for calculate how many blocks can be placed at the PDF sheet.
		/// </summary>
		public double PalleteStatBlockHeight
		{
			get
			{
				if (PalleteStatGrid != null)
					return PalleteStatGrid.ActualHeight + PalleteStatGrid.Margin.Top + PalleteStatGrid.Margin.Bottom;

				return 0;
			}
		}
		//=============================================================================
		/// <summary>
		/// Height of MHE details block height.
		/// It is used for calculate how many blocks can be placed at the PDF sheet.
		/// </summary>
		public double MHEDetailsBlockHeight
		{
			get
			{
				if (MHEDetailsGrid != null)
					return MHEDetailsGrid.ActualHeight + MHEDetailsGrid.Margin.Top + MHEDetailsGrid.Margin.Bottom;

				return 0;
			}
		}
		//=============================================================================
		/// <summary>
		/// Height of "Important notes on flooring" block height.
		/// It is used for calculate how many blocks can be placed at the PDF sheet.
		/// </summary>
		public double ImportantNotesOnFlooringBlockHeight
		{
			get
			{
				if (ImportantNotesOnFlooringGrid != null)
					return ImportantNotesOnFlooringGrid.ActualHeight + ImportantNotesOnFlooringGrid.Margin.Top + ImportantNotesOnFlooringGrid.Margin.Bottom;

				return 0;
			}
		}
		//=============================================================================
		/// <summary>
		/// Height of notes block height.
		/// It is used for calculate how many blocks can be placed at the PDF sheet.
		/// </summary>
		public double NotesBlockHeight
		{
			get
			{
				if (NotesGrid != null)
					return NotesGrid.ActualHeight + NotesGrid.Margin.Top + NotesGrid.Margin.Bottom;

				return 0;
			}
		}
		//=============================================================================
		/// <summary>
		/// Height of block, which can contains rack\pallete statistics, notes, mhe details.
		/// It is used for calculate how many blocks can be placed at the PDF sheet.
		/// </summary>
		public double DynamicFillBlockHeight
		{
			get
			{
				if (DynamicFillGrid != null)
					return DynamicFillGrid.ActualHeight;

				return 0;
			}
		}

		#endregion
	}
}
