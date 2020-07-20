using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	// Draw new rectangle area on changing drawing Length or Width
	// if there are some rectangles which are outside area and will be deleted.
	public class PreviewBordersControl : DrawingVisual
	{
		private DrawingControl m_DC = null;

		public PreviewBordersControl(DrawingControl dc)
		{
			m_DC = dc;
		}

		//=============================================================================
		public void Draw(bool bShow)
		{
			if (m_DC == null)
				return;

			DrawingSheet currSheet = m_DC.Sheet;
			if (currSheet == null)
				return;

			//
			// if there is no one line to draw you steel need to call RenderOpen() to remove all old lines

			this.Opacity = 0.3;
			// draw online snapped line
			using (DrawingContext thisDC = this.RenderOpen())
			{
				if (!bShow)
					return;

				// draw new rectangle
				if ((m_DC.PreviewGlobalLength > 0 && m_DC.PreviewGlobalLength < currSheet.Length)
					|| (m_DC.PreviewGlobalWidth > 0 && m_DC.PreviewGlobalWidth < currSheet.Width))
				{
					//
					Point startPnt = new Point(0, 0);
					startPnt = m_DC.GetLocalPoint(currSheet, startPnt);

					//
					double length = currSheet.Length;
					if (m_DC.PreviewGlobalLength > 0 && m_DC.PreviewGlobalLength < currSheet.Length)
						length = m_DC.PreviewGlobalLength;
					//
					double width = currSheet.Width;
					if (m_DC.PreviewGlobalWidth > 0 && m_DC.PreviewGlobalWidth < currSheet.Width)
						width = m_DC.PreviewGlobalWidth;
					//
					Point endPnt = new Point(length, width);
					endPnt = m_DC.GetLocalPoint(currSheet, endPnt);

					//
					thisDC.DrawRectangle(m_DC.NewSizePreviewFillBrush, new Pen(m_DC.NewSizePreviewBorderBrush, 2), new Rect(startPnt, endPnt));
				}
			}
		}
	}
}
