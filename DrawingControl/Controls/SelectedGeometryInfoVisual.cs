using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	/// <summary>
	/// Visual which draws lines with distance value text from selected geometry to sheet borders.
	/// Also displays dimension of selected geometry.
	/// </summary>
	public class SelectedGeometryInfoVisual : DrawingVisual
	{
		public SelectedGeometryInfoVisual(DrawingControl dc)
		{
			m_DC = dc;
		}

		//=============================================================================
		private DrawingControl m_DC = null;

		//
		protected double m_DistanceLineThickness = 1.0;
		protected double m_rTextSize = 14;
		protected double m_rSizeLeaderLength = 20;

		//=============================================================================
		public void Draw()
		{
			using (DrawingContext thisDC = this.RenderOpen())
			{
				if (m_DC == null)
					return;

				DrawingSheet currSheet = m_DC.Sheet;
				if (currSheet == null)
					return;

				if (currSheet.Document == null)
					return;
				if (currSheet.Document.ShowAdvancedProperties)
					return;

				if (currSheet.SelectedGeometryCollection == null)
					return;

				bool isFullRowSelected = IsSelectedFullRackGroupOnly();

				// draw lines with distance value if only single geometry selected
				if (currSheet.SelectedGeometryCollection.Count != 1 && !isFullRowSelected)
                    return;

                BaseRectangleGeometry selectedGeom = currSheet.SelectedGeometryCollection[0];
                BaseRectangleGeometry lastSelectedGeom = currSheet.SelectedGeometryCollection[currSheet.SelectedGeometryCollection.Count - 1];
				if (selectedGeom == null || lastSelectedGeom == null)
					return;

				// ignore case when selected only 1 element
                if (selectedGeom != lastSelectedGeom)
                {
					foreach (BaseRectangleGeometry current in currSheet.SelectedGeometryCollection)
					{
						if (selectedGeom.IsHorizontal)
						{
							if (selectedGeom.Center_GlobalPoint.X > current.Center_GlobalPoint.X)
								selectedGeom = current;

							if (lastSelectedGeom.Center_GlobalPoint.X < current.Center_GlobalPoint.X)
								lastSelectedGeom = current;
						}
						else
						{
							if (selectedGeom.BottomLeft_GlobalPoint.Y > current.BottomLeft_GlobalPoint.Y)
								selectedGeom = current;

							if (lastSelectedGeom.BottomLeft_GlobalPoint.Y < current.BottomLeft_GlobalPoint.Y)
								lastSelectedGeom = current;
						}
					}
				}

				// 
				Pen _DistanceLinesPen = new Pen(m_DC.SelectedGeometryInfoBrush, m_DistanceLineThickness);
				FontFamily textFontFamily = new FontFamily("Arial");
				Typeface textTypeFace = new Typeface(textFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
				
				//
				Point TopLeft_ScreenPoint;
				Point TopRight_ScreenPoint;
				Point BottomRight_ScreenPoint;
				Point BottomLeft_ScreenPoint;

                if (selectedGeom.IsHorizontal)
                {
					TopLeft_ScreenPoint = m_DC.GetLocalPoint(currSheet, selectedGeom.TopLeft_GlobalPoint);
					TopRight_ScreenPoint = m_DC.GetLocalPoint(currSheet, lastSelectedGeom.TopRight_GlobalPoint);
					BottomRight_ScreenPoint = m_DC.GetLocalPoint(currSheet, lastSelectedGeom.BottomRight_GlobalPoint);
					BottomLeft_ScreenPoint = m_DC.GetLocalPoint(currSheet, selectedGeom.BottomLeft_GlobalPoint);
				}
                else
                {
                    TopLeft_ScreenPoint = m_DC.GetLocalPoint(currSheet, selectedGeom.TopLeft_GlobalPoint);
                    TopRight_ScreenPoint = m_DC.GetLocalPoint(currSheet, selectedGeom.TopRight_GlobalPoint);
                    BottomRight_ScreenPoint = m_DC.GetLocalPoint(currSheet, lastSelectedGeom.BottomRight_GlobalPoint);
                    BottomLeft_ScreenPoint = m_DC.GetLocalPoint(currSheet, lastSelectedGeom.BottomLeft_GlobalPoint);
                }

				Point Center_ScreenPoint = m_DC.GetLocalPoint(currSheet, selectedGeom.Center_GlobalPoint);

				if (Utils.FGE(TopLeft_ScreenPoint.X, 0.0) && Utils.FGE(TopLeft_ScreenPoint.Y, 0.0) && Utils.FLE(TopLeft_ScreenPoint.X, m_DC.ActualWidth) && Utils.FLE(TopLeft_ScreenPoint.Y, m_DC.ActualHeight))
				{
					// draw horizontal line and text
					Point LineStart_ScreenPoint = TopLeft_ScreenPoint;
					LineStart_ScreenPoint.X = 0;
					Point LineEnd_ScreenPoint = TopLeft_ScreenPoint;
					thisDC.DrawLine(_DistanceLinesPen, LineStart_ScreenPoint, LineEnd_ScreenPoint);

					// text, show global distance
					string strDistanceY = selectedGeom.TopLeft_GlobalPoint.Y.ToString(".");
					FormattedText formattedText = new FormattedText(strDistanceY, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, textTypeFace, m_rTextSize, m_DC.SelectedGeometryInfoBrush);
					formattedText.TextAlignment = TextAlignment.Center;

					Point Text_ScreenPoint = LineStart_ScreenPoint;
					Text_ScreenPoint.X -= formattedText.Width / 2;
					Text_ScreenPoint.Y -= formattedText.Height / 2;
					thisDC.DrawText(formattedText, Text_ScreenPoint);


					// draw vertical line and text
					LineStart_ScreenPoint = TopLeft_ScreenPoint;
					LineStart_ScreenPoint.Y = 0;
					LineEnd_ScreenPoint = TopLeft_ScreenPoint;
					thisDC.DrawLine(_DistanceLinesPen, LineStart_ScreenPoint, LineEnd_ScreenPoint);

					// text, show global distance
					string strDistanceX = selectedGeom.TopLeft_GlobalPoint.X.ToString(".");
					formattedText = new FormattedText(strDistanceX, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, textTypeFace, m_rTextSize, m_DC.SelectedGeometryInfoBrush);
					formattedText.TextAlignment = TextAlignment.Center;

					Text_ScreenPoint = LineStart_ScreenPoint;
					Text_ScreenPoint.Y -= formattedText.Height;
					thisDC.DrawText(formattedText, Text_ScreenPoint);
				}

				// Check that displayed rectangle is placed in displayed area.
				// If camera displays part of the current sheet then dont display geometry which is outside of displayed area.
				bool isAllSheetDisplayed = currSheet.IsSheetFullyDisplayed;
				Point displayedArea_TopLeftPoint = new Point(0.0, 0.0);
				Point displayedArea_BotRightPoint = new Point(0.0, 0.0);
				if (!isAllSheetDisplayed)
				{
					displayedArea_TopLeftPoint = m_DC.GetGlobalPoint(currSheet, new Point(0.0, 0.0));
					displayedArea_BotRightPoint = m_DC.GetGlobalPoint(currSheet, new Point(m_DC.ActualWidth, m_DC.ActualHeight));

					// if one rectangle is on the left side of other
					if (Utils.FGE(displayedArea_TopLeftPoint.X, selectedGeom.BottomRight_GlobalPoint.X) || Utils.FGE(selectedGeom.TopLeft_GlobalPoint.X, displayedArea_BotRightPoint.X))
						return;

					// if one rectangle is above other
					if (Utils.FGE(displayedArea_TopLeftPoint.Y, selectedGeom.BottomRight_GlobalPoint.Y) || Utils.FGE(selectedGeom.TopLeft_GlobalPoint.Y, displayedArea_BotRightPoint.Y))
						return;

					// Probably selected geometry is partial displayed, so need to clip drawing context.
					thisDC.PushClip(new RectangleGeometry(new Rect(new Point(0.0, 0.0), new Point(m_DC.ActualWidth, m_DC.ActualHeight))));
				}

				// Dont draw length and depth for SheetElevationGeometry
				if (selectedGeom is SheetElevationGeometry)
					return;

				// if this rectangle is selected than draw its size
				//
				// LENGTH AT THE BOTTOM
				//
				// draw size leaders
				Point BotLeft_SizeRack_EndScreenPoint = BottomLeft_ScreenPoint;
				BotLeft_SizeRack_EndScreenPoint.Y += m_rSizeLeaderLength;
				thisDC.DrawLine(_DistanceLinesPen, BottomLeft_ScreenPoint, BotLeft_SizeRack_EndScreenPoint);
				//
				Point BotRight_SizeRack_EndScreenPoint = BottomRight_ScreenPoint;
				BotRight_SizeRack_EndScreenPoint.Y += m_rSizeLeaderLength;
				thisDC.DrawLine(_DistanceLinesPen, BottomRight_ScreenPoint, BotRight_SizeRack_EndScreenPoint);

				//
				bool bDrawArrowLinesInside = (BotRight_SizeRack_EndScreenPoint.X - BotLeft_SizeRack_EndScreenPoint.X) > 20;

				Point BotLeft_WidthSizeLine_ScreenPoint = BottomLeft_ScreenPoint;
				BotLeft_WidthSizeLine_ScreenPoint.Y += 0.75 * m_rSizeLeaderLength;
				Point BotRight_WidthSizeLine_ScreenPoint = BottomRight_ScreenPoint;
				BotRight_WidthSizeLine_ScreenPoint.Y += 0.75 * m_rSizeLeaderLength;
				if (!bDrawArrowLinesInside)
				{
					BotLeft_WidthSizeLine_ScreenPoint.X -= 0.5 * m_rSizeLeaderLength;
					BotRight_WidthSizeLine_ScreenPoint.X += 0.5 * m_rSizeLeaderLength;
				}
				// 
				thisDC.DrawLine(_DistanceLinesPen, BotLeft_WidthSizeLine_ScreenPoint, BotRight_WidthSizeLine_ScreenPoint);
				// arrows
				Point LeftWidthArrow_ScreenPoint_1, LeftWidthArrow_ScreenPoint_2;
				Point RightWidthArrow_ScreenPoint_1, RightWidthArrow_ScreenPoint_2;
				Point LeftWidthArrow_ScreenPoint_3, RightWidthArrow_ScreenPoint_3;
				if (bDrawArrowLinesInside)
				{
					LeftWidthArrow_ScreenPoint_3 = BotLeft_WidthSizeLine_ScreenPoint;
					//
					LeftWidthArrow_ScreenPoint_1 = LeftWidthArrow_ScreenPoint_3;
					LeftWidthArrow_ScreenPoint_1.X += 0.3 * m_rSizeLeaderLength;
					LeftWidthArrow_ScreenPoint_1.Y -= 0.1 * m_rSizeLeaderLength;
					//
					LeftWidthArrow_ScreenPoint_2 = LeftWidthArrow_ScreenPoint_3;
					LeftWidthArrow_ScreenPoint_2.X += 0.3 * m_rSizeLeaderLength;
					LeftWidthArrow_ScreenPoint_2.Y += 0.1 * m_rSizeLeaderLength;

					RightWidthArrow_ScreenPoint_3 = BotRight_WidthSizeLine_ScreenPoint;
					//
					RightWidthArrow_ScreenPoint_1 = RightWidthArrow_ScreenPoint_3;
					RightWidthArrow_ScreenPoint_1.X -= 0.3 * m_rSizeLeaderLength;
					RightWidthArrow_ScreenPoint_1.Y -= 0.1 * m_rSizeLeaderLength;
					//
					RightWidthArrow_ScreenPoint_2 = RightWidthArrow_ScreenPoint_3;
					RightWidthArrow_ScreenPoint_2.X -= 0.3 * m_rSizeLeaderLength;
					RightWidthArrow_ScreenPoint_2.Y += 0.1 * m_rSizeLeaderLength;
				}
				else
				{
					LeftWidthArrow_ScreenPoint_3 = BotLeft_WidthSizeLine_ScreenPoint;
					LeftWidthArrow_ScreenPoint_3.X = BottomLeft_ScreenPoint.X;
					//
					LeftWidthArrow_ScreenPoint_1 = LeftWidthArrow_ScreenPoint_3;
					LeftWidthArrow_ScreenPoint_1.X -= 0.3 * m_rSizeLeaderLength;
					LeftWidthArrow_ScreenPoint_1.Y -= 0.1 * m_rSizeLeaderLength;
					//
					LeftWidthArrow_ScreenPoint_2 = LeftWidthArrow_ScreenPoint_3;
					LeftWidthArrow_ScreenPoint_2.X -= 0.3 * m_rSizeLeaderLength;
					LeftWidthArrow_ScreenPoint_2.Y += 0.1 * m_rSizeLeaderLength;

					RightWidthArrow_ScreenPoint_3 = BotRight_WidthSizeLine_ScreenPoint;
					RightWidthArrow_ScreenPoint_3.X = BottomRight_ScreenPoint.X;
					//
					RightWidthArrow_ScreenPoint_1 = RightWidthArrow_ScreenPoint_3;
					RightWidthArrow_ScreenPoint_1.X += 0.3 * m_rSizeLeaderLength;
					RightWidthArrow_ScreenPoint_1.Y -= 0.1 * m_rSizeLeaderLength;
					//
					RightWidthArrow_ScreenPoint_2 = RightWidthArrow_ScreenPoint_3;
					RightWidthArrow_ScreenPoint_2.X += 0.3 * m_rSizeLeaderLength;
					RightWidthArrow_ScreenPoint_2.Y += 0.1 * m_rSizeLeaderLength;
				}
				thisDC.DrawLine(_DistanceLinesPen, LeftWidthArrow_ScreenPoint_3, LeftWidthArrow_ScreenPoint_1);
				thisDC.DrawLine(_DistanceLinesPen, LeftWidthArrow_ScreenPoint_3, LeftWidthArrow_ScreenPoint_2);
				thisDC.DrawLine(_DistanceLinesPen, RightWidthArrow_ScreenPoint_3, RightWidthArrow_ScreenPoint_1);
				thisDC.DrawLine(_DistanceLinesPen, RightWidthArrow_ScreenPoint_3, RightWidthArrow_ScreenPoint_2);
				// text
				double length = 0;
				foreach (BaseRectangleGeometry current in m_DC.Sheet.SelectedGeometryCollection)
				{
					if (selectedGeom.IsHorizontal)
					{
						length += current.Length_X;
					}
					else
					{
						length = current.Length_X;
						break;
					}
				}
				FormattedText widthText = new FormattedText(length.ToString("."), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, textTypeFace, m_rTextSize, m_DC.SelectedGeometryInfoBrush);
				widthText.TextAlignment = TextAlignment.Center;
				Point WidthText_ScreenPoint = BotRight_SizeRack_EndScreenPoint;
				WidthText_ScreenPoint.X -= (BotRight_SizeRack_EndScreenPoint.X - BotLeft_SizeRack_EndScreenPoint.X) / 2;
				thisDC.DrawText(widthText, WidthText_ScreenPoint);

				//
				// WIDTH AT THE RIGHT BORDER
				//

				// draw size leaders
				Point TopRight_SizeRack_ScreenEndPoint = TopRight_ScreenPoint;
				TopRight_SizeRack_ScreenEndPoint.X += m_rSizeLeaderLength;
				//
				Point BottomRight_SizeRack_ScreenEndPoint = BottomRight_ScreenPoint;
				BottomRight_SizeRack_ScreenEndPoint.X += m_rSizeLeaderLength;
				//
				thisDC.DrawLine(_DistanceLinesPen, TopRight_ScreenPoint, TopRight_SizeRack_ScreenEndPoint);
				thisDC.DrawLine(_DistanceLinesPen, BottomRight_ScreenPoint, BottomRight_SizeRack_ScreenEndPoint);

				//
				bDrawArrowLinesInside = (BottomRight_SizeRack_ScreenEndPoint.Y - TopRight_SizeRack_ScreenEndPoint.Y) > 20;

				//
				Point TopHeightLine_ScreenPoint = m_DC.GetLocalPoint(currSheet, selectedGeom.IsHorizontal ? lastSelectedGeom.TopRight_GlobalPoint : selectedGeom.TopRight_GlobalPoint);
				TopHeightLine_ScreenPoint.X += 0.75 * m_rSizeLeaderLength;
				Point BottomHeightLine_ScreenPoint = m_DC.GetLocalPoint(currSheet, lastSelectedGeom.BottomRight_GlobalPoint);
				BottomHeightLine_ScreenPoint.X += 0.75 * m_rSizeLeaderLength;
				//
				if (!bDrawArrowLinesInside)
				{
					TopHeightLine_ScreenPoint.Y -= 0.5 * m_rSizeLeaderLength;
					BottomHeightLine_ScreenPoint.Y += 0.5 * m_rSizeLeaderLength;
				}
				//
				thisDC.DrawLine(_DistanceLinesPen, TopHeightLine_ScreenPoint, BottomHeightLine_ScreenPoint);

				// arrows
				Point TopHeightArrow_ScreenPoint_1, TopHeightArrow_ScreenPoint_3;
				Point BotHeightArrow_ScreenPoint_1, BotHeightArrow_ScreenPoint_3;
				Point TopHeightArrow_ScreenPoint_2, BotHeightArrow_ScreenPoint_2;
				if (bDrawArrowLinesInside)
				{
					TopHeightArrow_ScreenPoint_2 = TopHeightLine_ScreenPoint;
					//
					TopHeightArrow_ScreenPoint_1 = TopHeightArrow_ScreenPoint_2;
					TopHeightArrow_ScreenPoint_1.X -= 0.1 * m_rSizeLeaderLength;
					TopHeightArrow_ScreenPoint_1.Y += 0.3 * m_rSizeLeaderLength;
					//
					TopHeightArrow_ScreenPoint_3 = TopHeightArrow_ScreenPoint_2;
					TopHeightArrow_ScreenPoint_3.X += 0.1 * m_rSizeLeaderLength;
					TopHeightArrow_ScreenPoint_3.Y += 0.3 * m_rSizeLeaderLength;

					BotHeightArrow_ScreenPoint_2 = BottomHeightLine_ScreenPoint;
					//
					BotHeightArrow_ScreenPoint_1 = BotHeightArrow_ScreenPoint_2;
					BotHeightArrow_ScreenPoint_1.X -= 0.1 * m_rSizeLeaderLength;
					BotHeightArrow_ScreenPoint_1.Y -= 0.3 * m_rSizeLeaderLength;
					//
					BotHeightArrow_ScreenPoint_3 = BotHeightArrow_ScreenPoint_2;
					BotHeightArrow_ScreenPoint_3.X += 0.1 * m_rSizeLeaderLength;
					BotHeightArrow_ScreenPoint_3.Y -= 0.3 * m_rSizeLeaderLength;
				}
				else
				{
					TopHeightArrow_ScreenPoint_2 = TopHeightLine_ScreenPoint;
					TopHeightArrow_ScreenPoint_2.Y = TopRight_ScreenPoint.Y;
					//
					TopHeightArrow_ScreenPoint_1 = TopHeightArrow_ScreenPoint_2;
					TopHeightArrow_ScreenPoint_1.X -= 0.1 * m_rSizeLeaderLength;
					TopHeightArrow_ScreenPoint_1.Y -= 0.3 * m_rSizeLeaderLength;
					//
					TopHeightArrow_ScreenPoint_3 = TopHeightArrow_ScreenPoint_2;
					TopHeightArrow_ScreenPoint_3.X += 0.1 * m_rSizeLeaderLength;
					TopHeightArrow_ScreenPoint_3.Y -= 0.3 * m_rSizeLeaderLength;

					BotHeightArrow_ScreenPoint_2 = BottomHeightLine_ScreenPoint;
					BotHeightArrow_ScreenPoint_2.Y = BottomRight_ScreenPoint.Y;
					//
					BotHeightArrow_ScreenPoint_1 = BotHeightArrow_ScreenPoint_2;
					BotHeightArrow_ScreenPoint_1.X -= 0.1 * m_rSizeLeaderLength;
					BotHeightArrow_ScreenPoint_1.Y += 0.3 * m_rSizeLeaderLength;
					//
					BotHeightArrow_ScreenPoint_3 = BotHeightArrow_ScreenPoint_2;
					BotHeightArrow_ScreenPoint_3.X += 0.1 * m_rSizeLeaderLength;
					BotHeightArrow_ScreenPoint_3.Y += 0.3 * m_rSizeLeaderLength;
				}
				thisDC.DrawLine(_DistanceLinesPen, TopHeightArrow_ScreenPoint_2, TopHeightArrow_ScreenPoint_1);
				thisDC.DrawLine(_DistanceLinesPen, TopHeightArrow_ScreenPoint_2, TopHeightArrow_ScreenPoint_3);
				thisDC.DrawLine(_DistanceLinesPen, BotHeightArrow_ScreenPoint_2, BotHeightArrow_ScreenPoint_1);
				thisDC.DrawLine(_DistanceLinesPen, BotHeightArrow_ScreenPoint_2, BotHeightArrow_ScreenPoint_3);

				// text
				double height = 0;
				foreach (BaseRectangleGeometry current in m_DC.Sheet.SelectedGeometryCollection)
				{
					if (selectedGeom.IsHorizontal)
					{
						height = current.Length_Y;
						break;
					}
					else
					{
						height += current.Length_Y;
					}
				}
				FormattedText heightText = new FormattedText(height.ToString("."), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, textTypeFace, m_rTextSize, m_DC.SelectedGeometryInfoBrush);
				heightText.TextAlignment = TextAlignment.Center;
				Point heightTextPoint = TopRight_SizeRack_ScreenEndPoint;
				heightTextPoint.Y += (BottomRight_SizeRack_ScreenEndPoint.Y - TopRight_SizeRack_ScreenEndPoint.Y) / 2;
				heightTextPoint.Y -= heightText.Height / 2;
				heightTextPoint.X += heightText.Width / 2;
				thisDC.DrawText(heightText, heightTextPoint);

				

				// Pop clip
				if (!isAllSheetDisplayed)
					thisDC.Pop();
			}
		}

		private bool IsSelectedFullRackGroupOnly()
        {
            if (m_DC.Sheet.SelectedGeometryCollection.Count <= 1)
                return false;

            bool bItIsRacksSingleRowCoulumn = true;
			int iMultiSelectionRacksCount = 0;

			List<List<Rack>> foundedRackGroups = new List<List<Rack>>();

			foreach (BaseRectangleGeometry selectedRect in m_DC.Sheet.SelectedGeometryCollection)
			{
				Rack selectedRack = selectedRect as Rack;
				if (selectedRack != null)
				{
					++iMultiSelectionRacksCount;
					//
					List<Rack> selectedRackGroup = m_DC.Sheet.GetRackGroup(selectedRack);
					if (!foundedRackGroups.Contains(selectedRackGroup))
					{
						foundedRackGroups.Add(selectedRackGroup);
						if (foundedRackGroups.Count > 1)
						{
							bItIsRacksSingleRowCoulumn = false;
							break;
						}
					}
				}
				else
				{
					bItIsRacksSingleRowCoulumn = false;
					break;
				}
			}

			if (bItIsRacksSingleRowCoulumn)
			{
				// only if selected all racks in the row\column
				bItIsRacksSingleRowCoulumn = false;
				if (foundedRackGroups.Count == 1 && foundedRackGroups[0].Count == iMultiSelectionRacksCount)
					bItIsRacksSingleRowCoulumn = true;
			}

			return bItIsRacksSingleRowCoulumn;
		}
	}
}
