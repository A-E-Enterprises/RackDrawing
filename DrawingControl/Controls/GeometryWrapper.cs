using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public class GeometryWrapper : DrawingVisual
	{
		public GeometryWrapper(DrawingControl owner, BaseRectangleGeometry geometry)
		{
			m_DC = owner;
			m_Geometry = geometry;

			if (m_Geometry != null)
				m_Geometry.Wrapper = this;
		}

		#region Properties

		//=============================================================================
		private DrawingControl m_DC = null;
		public DrawingControl Owner { get { return m_DC; } }


		//=============================================================================
		private BaseRectangleGeometry m_Geometry = null;
		public BaseRectangleGeometry RectangleGeometry { get { return m_Geometry; } }

		#endregion

		//=============================================================================
		public void Draw(bool bShow)
		{
			if (m_Geometry == null)
				return;

			// if there is non initialized rectangle, then show all other rectangle with 0.5 opacity
			if (m_DC != null && m_DC.Sheet != null)
			{
				double rOpacity = 1;
				List<BaseRectangleGeometry> nonInitGeomList = m_DC.Sheet.NonInitSelectedGeometryList;
				if (nonInitGeomList.Count > 0)
				{
					if (nonInitGeomList.Contains(m_Geometry))
						rOpacity = 1;
					else
						rOpacity = 0.5;
				}
				this.Opacity = rOpacity;
			}

			using (DrawingContext thisDC = this.RenderOpen())
			{
				// if there is no one line to draw you steel need to call RenderOpen() to remove all old lines
				if (bShow)
				{
					// DrawingControl can display part of sheet if it is scales and have offset.
					// So clip any drawing which is placed outside control.
					Point clipRectTopLeftPnt = new Point(0.0, 0.0);
					Point clipRectBotRightPnt = new Point(m_DC.ActualWidth, m_DC.ActualHeight);
					// If sheet is fully displayed then Wall and Shutter are displayed outside control and they are cut, so change clip rect size.
					if (m_DC != null && m_Geometry.Sheet != null && m_Geometry.Sheet.IsSheetFullyDisplayed && (m_Geometry is Wall || m_Geometry is Shutter))
					{
						double additionalSpace = Wall.THICKNESS;
						if (m_Geometry is Shutter)
							additionalSpace = Shutter.SHUTTER_DEPTH;
					
						clipRectTopLeftPnt.X -= m_DC.GetWidthInPixels(additionalSpace, m_Geometry.Sheet.UnitsPerCameraPixel);
						clipRectTopLeftPnt.Y -= m_DC.GetHeightInPixels(additionalSpace, m_Geometry.Sheet.UnitsPerCameraPixel);
						clipRectBotRightPnt.X += m_DC.GetWidthInPixels(additionalSpace, m_Geometry.Sheet.UnitsPerCameraPixel);
						clipRectBotRightPnt.Y += m_DC.GetHeightInPixels(additionalSpace, m_Geometry.Sheet.UnitsPerCameraPixel);
					}
					thisDC.PushClip(new RectangleGeometry(new Rect(clipRectTopLeftPnt, clipRectBotRightPnt)));

					m_Geometry.Draw(thisDC, m_DC);

					thisDC.Pop();
				}
			}
		}
	}
}
