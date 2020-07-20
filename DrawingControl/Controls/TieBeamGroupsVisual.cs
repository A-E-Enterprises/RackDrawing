using AppColorTheme;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	/// <summary>
	/// Displays tie beam groups(racks groups) which are used for calculate - should tie beam be placed?
	/// </summary>
	public class TieBeamGroupsVisual : DrawingVisual
	{
		private DrawingControl m_DC = null;

		public TieBeamGroupsVisual(DrawingControl owner)
		{
			m_DC = owner;
		}

		//=============================================================================
		public DrawingControl Owner { get { return m_DC; } }

		//=============================================================================
		private static int offset = 200;
		public void Draw()
		{
			using (DrawingContext thisDC = this.RenderOpen())
			{
				DrawingSheet currentSheet = m_DC.Sheet;

				// draw tie beams groups
				if (currentSheet != null && currentSheet.Document != null && m_DC != null)
				{
					double racksBTBDistance = 0.0;
					if (ePalletType.eFlush == currentSheet.Document.RacksPalletType)
						racksBTBDistance = DrawingDocument.RACK_FLUSH_PALLETS_BTB_DISTANCE;
					else
						racksBTBDistance = DrawingDocument.RACK_OVERHANG_PALLETS_BTB_DISTANCE_ADDITIONALVALUE;

					TieBeamGroupsInfo tieBeamGroupsInfo = new TieBeamGroupsInfo(currentSheet.RacksGroups, currentSheet.Document.RacksPalletType, racksBTBDistance);

					Color borderColor = Colors.Indigo;
					if (CurrentGeometryColorsTheme.CurrentTheme != null)
					{
						Color colorValue;
						if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eTieBeamGroupBorderColor, out colorValue))
							borderColor = colorValue;
					}
					Pen tieBeamGroupsPen = new Pen(new SolidColorBrush(borderColor), 2.0);

					// If camera displays part of the current sheet then dont display geometry which is outside of displayed area.
					bool isSheetFullyDisplayed = currentSheet.IsSheetFullyDisplayed;
					Point displayedArea_TopLeftPoint = new Point(0.0, 0.0);
					Point displayedArea_BotRightPoint = new Point(0.0, 0.0);
					if (!isSheetFullyDisplayed)
					{
						displayedArea_TopLeftPoint = m_DC.GetGlobalPoint(currentSheet, new Point(0.0, 0.0));
						displayedArea_BotRightPoint = m_DC.GetGlobalPoint(currentSheet, new Point(m_DC.ActualWidth, m_DC.ActualHeight));

						// crip all graphics outside DrawingControl
						thisDC.PushClip(new RectangleGeometry(new Rect(new Point(0.0, 0.0), new Point(m_DC.ActualWidth, m_DC.ActualHeight))));
					}

					foreach (TieBeam_SingleGroup singleGroup in tieBeamGroupsInfo.m_SingleGroupsList)
					{
						if (singleGroup == null)
							continue;

						Point borderTopLeftGlobalPnt = singleGroup.TopLeftPnt + offset * new Vector(1, 1);
						Point borderBotRightGlobalPnt = singleGroup.BotRightPnt - offset * new Vector(1, 1);

						bool bForceHide = false;
						if (!isSheetFullyDisplayed)
						{
							// if one rectangle is on the left side of other
							if (Utils.FGE(displayedArea_TopLeftPoint.X, borderBotRightGlobalPnt.X) || Utils.FGE(borderTopLeftGlobalPnt.X, displayedArea_BotRightPoint.X))
								bForceHide = true;

							// if one rectangle is above other
							if (!bForceHide && Utils.FGE(displayedArea_TopLeftPoint.Y, borderBotRightGlobalPnt.Y) || Utils.FGE(borderTopLeftGlobalPnt.Y, displayedArea_BotRightPoint.Y))
								bForceHide = true;
						}

						if (bForceHide)
							continue;

						Point topLeftPnt = m_DC.GetLocalPoint(currentSheet, borderTopLeftGlobalPnt);
						Point botRightPnt = m_DC.GetLocalPoint(currentSheet, borderBotRightGlobalPnt);
						thisDC.DrawRectangle(null, tieBeamGroupsPen, new Rect(topLeftPnt, botRightPnt));
					}

					foreach (TieBeam_BTBGroup btbGroup in tieBeamGroupsInfo.m_BTBGroupsList)
					{
						if (btbGroup == null)
							continue;

						Point borderTopLeftGlobalPnt = btbGroup.TopLeftPnt + offset * new Vector(1, 1);
						Point borderBotRightGlobalPnt = btbGroup.BotRightPnt - offset * new Vector(1, 1);

						bool bForceHide = false;
						if (!isSheetFullyDisplayed)
						{
							// if one rectangle is on the left side of other
							if (Utils.FGE(displayedArea_TopLeftPoint.X, borderBotRightGlobalPnt.X) || Utils.FGE(borderTopLeftGlobalPnt.X, displayedArea_BotRightPoint.X))
								bForceHide = true;

							// if one rectangle is above other
							if (!bForceHide && Utils.FGE(displayedArea_TopLeftPoint.Y, borderBotRightGlobalPnt.Y) || Utils.FGE(borderTopLeftGlobalPnt.Y, displayedArea_BotRightPoint.Y))
								bForceHide = true;
						}

						if (bForceHide)
							continue;

						Point topLeftPnt = m_DC.GetLocalPoint(currentSheet, borderTopLeftGlobalPnt);
						Point botRightPnt = m_DC.GetLocalPoint(currentSheet, borderBotRightGlobalPnt);
						thisDC.DrawRectangle(null, tieBeamGroupsPen, new Rect(topLeftPnt, botRightPnt));
					}

					// Pop clip
					if (!isSheetFullyDisplayed)
						thisDC.Pop();
				}
			}
		}
	}
}
