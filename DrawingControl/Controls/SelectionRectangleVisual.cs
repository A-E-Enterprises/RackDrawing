using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	/// <summary>
	/// Draws selection rectangle when user holds mouse left button and move mouse.
	/// </summary>
	public class SelectionRectangleVisual : DrawingVisual
	{
		public SelectionRectangleVisual(DrawingControl dc)
		{
			m_DC = dc;
		}

		//=============================================================================
		private DrawingControl m_DC = null;

		//=============================================================================
		public void Draw()
		{
			if (m_DC == null)
				return;

			DrawingSheet currSheet = m_DC.Sheet;
			if (currSheet == null)
				return;

			//
			// if there is no one line to draw you steel need to call RenderOpen() to remove all old lines

			this.Opacity = 0.35;
			// draw online snapped line
			using (DrawingContext thisDC = this.RenderOpen())
			{
				if (!m_DC.DisplaySelectionRectangle)
					return;

				Point pnt01 = m_DC.GetLocalPoint(currSheet, m_DC.SelectionFirstGlobalPnt);
				Point pnt02 = m_DC.GetLocalPoint(currSheet, m_DC.SelectionSecondGlobalPnt);
				//
				thisDC.DrawRectangle(m_DC.SelectionRectangleFillBrush, new Pen(m_DC.SelectionRectangleBorderBrush, 1), new Rect(pnt01, pnt02));
			}
		}
	}
}
