using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	/// <summary>
	/// Draws tooltip text near mouse point
	/// </summary>
	public class ToolTipVisual : DrawingVisual
	{
		public ToolTipVisual(DrawingControl dc)
		{
			m_DC = dc;
		}

		//=============================================================================
		private DrawingControl m_DC = null;

		//=============================================================================
		/// <summary>
		/// Margin between the text and background rectangle
		/// </summary>
		private double m_RectMargin = 8;

		//=============================================================================
		public void Draw()
		{
			if (m_DC == null)
				return;

			// draw online snapped line
			using (DrawingContext thisDC = this.RenderOpen())
			{
				if (string.IsNullOrEmpty(m_DC.ToolTipText))
					return;

				if (!m_DC.CanDisplayToolTip)
					return;

				DrawingSheet currSheet = m_DC.Sheet;
				if (currSheet == null)
					return;

				//
				FontFamily textFontFamily = new FontFamily("Arial");
				Typeface textTypeFace = new Typeface(textFontFamily, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
				FormattedText fmtedText = new FormattedText(m_DC.ToolTipText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, textTypeFace, 16, m_DC.TooltipTextBrush);
				//
				Point textPnt = m_DC.MouseLocalPoint;
				textPnt.Y += 25;
				textPnt.X += 20;

				// draw background rectangle
				Point rectPnt_01 = textPnt;
				rectPnt_01.X -= m_RectMargin;
				rectPnt_01.Y -= m_RectMargin;
				Point rectPnt_02 = textPnt;
				rectPnt_02.X += fmtedText.Width + m_RectMargin;
				rectPnt_02.Y += fmtedText.Height + m_RectMargin;
				//
				thisDC.DrawRectangle(m_DC.TooltipBackgroundBrush, null, new Rect(rectPnt_01, rectPnt_02));

				// draw text
				thisDC.DrawText(fmtedText, textPnt);
			}
		}
	}
}
