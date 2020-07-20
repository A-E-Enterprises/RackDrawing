using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	// Draws snap lines
	public class SnappingLinesControl : DrawingVisual
	{
		private DrawingControl m_DC = null;

		public SnappingLinesControl(DrawingControl dc)
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

			// if there is no one line to draw you steel need to call RenderOpen() to remove all old lines
			//if (m_owner.m_VerticalLines.Count == 0 && m_owner.m_HorizontalLines.Count == 0)
			//	return;

			// draw online snapped line
			using (DrawingContext thisDC = this.RenderOpen())
			{
				if (!bShow)
					return;

				// 
				Pen linesPen = new Pen(m_DC.SnappingLinesBrush, 1.0);

				// draw vert
				if(m_DC.VertLine_X > 0)
				{
					//
					Point startPnt = new Point(m_DC.VertLine_X, 0);
					startPnt = m_DC.GetLocalPoint(currSheet, startPnt);

					Point endPnt = new Point(m_DC.VertLine_X, currSheet.Width);
					endPnt = m_DC.GetLocalPoint(currSheet, endPnt);

					//
					thisDC.DrawLine(linesPen, startPnt, endPnt);
				}

				// draw horiz
				if(m_DC.HorizLine_Y > 0)
				{
					//
					Point startPnt = new Point(0, m_DC.HorizLine_Y);
					startPnt = m_DC.GetLocalPoint(currSheet, startPnt);

					Point endPnt = new Point(currSheet.Length, m_DC.HorizLine_Y);
					endPnt = m_DC.GetLocalPoint(currSheet, endPnt);

					//
					thisDC.DrawLine(linesPen, startPnt, endPnt);
				}
			}
		}
	}
}
