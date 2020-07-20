using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DrawingControl
{
	public static class RackUtils
	{
		//=============================================================================
		public static double GetMinLengthDependsOnBeams(Rack r)
		{
			return GetMinLengthDependsOnBeams(r.IsFirstInRowColumn, r.DiffBetween_M_and_A);
		}
		public static double GetMinLengthDependsOnBeams(bool IsItFirstRackInTheGroup, uint diffBetween_M_and_A)
		{
			// beam length + Rack.INNER_LENGTH_ADDITIONAL_GAP = rack length - 2*column_width

			// Dont check all beams from LoadChart.xlsx file.
			// All beams have 1000mm min length.
			double result = 1000 + Rack.INNER_LENGTH_ADDITIONAL_GAP;
			// add columns
			if (IsItFirstRackInTheGroup)
				result += 2 * diffBetween_M_and_A;
			else
				result += diffBetween_M_and_A;

			return result;
		}
		public static double GetMaxLengthDependsOnBeams(Rack r)
		{
			return GetMaxLengthDependsOnBeams(r.IsFirstInRowColumn, r.DiffBetween_M_and_A);
		}
		public static double GetMaxLengthDependsOnBeams(bool IsItFirstRackInTheGroup, uint diffBetween_M_and_A)
		{
			// beam length + Rack.INNER_LENGTH_ADDITIONAL_GAP = rack length - 2*column_width

			// Dont check all beams from LoadChart.xlsx file.
			// All beams have 4000mm min length.
			double result = 4000 + Rack.INNER_LENGTH_ADDITIONAL_GAP;
			// add columns
			if(IsItFirstRackInTheGroup)
				result += 2 * diffBetween_M_and_A;
			else
				result += diffBetween_M_and_A;

			return result;
		}

		//=============================================================================
		/// <summary>
		/// Returns previewGroup with newRack at index iReplaceRackIndex.
		/// </summary>
		public static bool TryToReplaceRackInGroup(DrawingSheet sheet, Rack newRack, int iReplaceRackIndex, bool bDeleteOldRack, List<Rack> group, out List<Rack> _previewGroup, out List<Rack> deletedRacksList, out RackColumn minColumn, bool bDeleteRacks = true, List<BaseRectangleGeometry> ignoreGeometryList = null)
		{
			// make preview and check layout
			_previewGroup = new List<Rack>();
			deletedRacksList = new List<Rack>();
			minColumn = null;

			if (sheet == null)
				return false;
			if (newRack == null)
				return false;
			if (group == null || group.Count == 0)
				return false;
			if (iReplaceRackIndex < 0 || iReplaceRackIndex >= group.Count)
				return false;

			// all racks in the group should has the same rotation
			// need to check newRack and rotate it
			if (newRack.IsHorizontal != group[0].IsHorizontal)
				newRack.Rotate(sheet.Length, sheet.Width, bCheckLayout: false);

			//
			Rack _previewChangedRack = newRack;
			_previewGroup.Add(_previewChangedRack);
			//
			int _changedRackIndex = iReplaceRackIndex;
			Rack _previosRack = _previewChangedRack;
			for (int i = _changedRackIndex - 1; i >= 0; --i)
			{
				Rack _previewRack = group[i].Clone() as Rack;
				if (_previewRack == null)
					return false;

				_previewRack.Sheet = sheet;
				//
				Point _TopLeft_GlobalPoint = _previosRack.TopLeft_GlobalPoint;
				if (newRack.IsHorizontal)
					_TopLeft_GlobalPoint.X -= _previewRack.Length_X + Rack.sHorizontalRow_GlobalGap;
				else
					_TopLeft_GlobalPoint.Y -= _previewRack.Length_Y + Rack.sVerticalColumn_GlobalGap;
				_previewRack.TopLeft_GlobalPoint = _TopLeft_GlobalPoint;

				_previewGroup.Insert(0, _previewRack);

				_previosRack = _previewRack;
			}
			//
			_previosRack = _previewChangedRack;
			for (int i = _changedRackIndex + 1; i < group.Count; ++i)
			{
				Rack _previewRack = group[i].Clone() as Rack;
				if (_previewRack == null)
					return false;

				_previewRack.Sheet = sheet;
				//
				Point _TopLeft_GlobalPoint = _previosRack.TopLeft_GlobalPoint;
				if (newRack.IsHorizontal)
				{
					_TopLeft_GlobalPoint = _previosRack.TopRight_GlobalPoint;
					_TopLeft_GlobalPoint.X += Rack.sHorizontalRow_GlobalGap;
				}
				else
				{
					_TopLeft_GlobalPoint = _previosRack.BottomLeft_GlobalPoint;
					_TopLeft_GlobalPoint.Y += Rack.sVerticalColumn_GlobalGap;
				}
				_previewRack.TopLeft_GlobalPoint = _TopLeft_GlobalPoint;

				_previewGroup.Add(_previewRack);

				_previosRack = _previewRack;
			}

			// Need to recalc column for the new group.
			// Rack length depends on the column.
			eColumnBracingType bracingType;
			//double xBracingHeight = 0.0;
			double stiffenersHeight = 0.0;
			if (!CalculateRacksColumnSizeAndBracingType(_previewGroup, sheet.Document.RacksColumnsList, out bracingType, /*out xBracingHeight,*/ out minColumn, out stiffenersHeight))
				return false;
			//RackColumn selectedColumn = minColumn;
			// All racks in the group should have the same column, so take the biggest one.
			RackColumn selectedCoumn = minColumn;
			foreach(Rack _previewRack in _previewGroup)
			{
				if (_previewRack == null || _previewRack.Column == null)
					continue;

				// Dont consider columns which are not set manually.
				// Racks group should have the minimum available column if it is not set manually.
				if (!_previewRack.IsColumnSetManually)
					continue;

				if (_previewRack.Column == minColumn)
					continue;
				else
				{
					if (Utils.FGT(_previewRack.Column.Length, minColumn.Length) || (Utils.FGE(_previewRack.Column.Length, minColumn.Length) && Utils.FGE(_previewRack.Column.Thickness, minColumn.Thickness)))
					{
						selectedCoumn = _previewRack.Column;
					}
				}
			}
			// Apply column to the racks.
			Rack prevRack = null;
			foreach(Rack previewRack in _previewGroup)
			{
				if (previewRack == null)
					continue;

				string strColumnError;
				// Dont check the layout while Set_Column().
				// Also ignore recalc beams and length errors.
				previewRack.IsInit = false;
				previewRack.Set_Column(minColumn, true, false, true, out strColumnError);
				if(selectedCoumn != previewRack.Column)
					previewRack.Set_Column(selectedCoumn, false, false, true, out strColumnError);
				previewRack.IsInit = true;

				if (prevRack != null)
				{
					Point TopLeft_GlobalPoint = prevRack.TopLeft_GlobalPoint;
					if (newRack.IsHorizontal)
					{
						TopLeft_GlobalPoint = prevRack.TopRight_GlobalPoint;
						TopLeft_GlobalPoint.X += Rack.sHorizontalRow_GlobalGap;
					}
					else
					{
						TopLeft_GlobalPoint = prevRack.BottomLeft_GlobalPoint;
						TopLeft_GlobalPoint.Y += Rack.sVerticalColumn_GlobalGap;
					}
					previewRack.TopLeft_GlobalPoint = TopLeft_GlobalPoint;
				}
				prevRack = previewRack;
			}

			// check layout
			Rack _first = null;
			Rack _last = null;
			Rack temporaryRack = null;
			while (true)
			{
				// check layout
				_first = _previewGroup[0];
				_last = _previewGroup[_previewGroup.Count - 1];

				// check layout
				// make big rectangle
				temporaryRack = _first.Clone() as Rack;
				if (temporaryRack == null)
					return false;
				//
				double maxGeomLengthX = 0.0;
				double maxGeomLengthY = 0.0;
				foreach(BaseRectangleGeometry geom in _previewGroup)
				{
					if (geom == null)
						continue;

					if (Utils.FGT(geom.Length_X, maxGeomLengthX))
						maxGeomLengthX = geom.Length_X;
					if (Utils.FGT(geom.Length_Y, maxGeomLengthY))
						maxGeomLengthY = geom.Length_Y;
				}
				//
				if (temporaryRack.IsHorizontal)
				{
					temporaryRack.Length_X = _last.TopRight_GlobalPoint.X - _first.TopLeft_GlobalPoint.X;
					temporaryRack.Length_Y = maxGeomLengthY;
				}
				else
				{
					temporaryRack.Length_Y = _last.BottomLeft_GlobalPoint.Y - _first.TopLeft_GlobalPoint.Y;
					temporaryRack.Length_X = maxGeomLengthX;
				}

				// check borders
				if (temporaryRack.Length_X > sheet.Length || temporaryRack.Length_Y > sheet.Width)
				{
					// delete next rack outside borders
					if (_changedRackIndex + 1 < _previewGroup.Count)
					{
						_previewGroup.RemoveAt(_previewGroup.Count - 1);
						continue;
					}
					else
					{
						// delete all racks
						_previewGroup.Clear();
						break;
					}
				}

				if (Utils.FLT(temporaryRack.TopLeft_GlobalPoint.X, 0 + temporaryRack.MarginX))
					temporaryRack.TopLeft_GlobalPoint = new Point(0 + temporaryRack.MarginX, temporaryRack.TopLeft_GlobalPoint.Y);
				if (Utils.FLT(temporaryRack.TopLeft_GlobalPoint.Y, 0 + temporaryRack.MarginY))
					temporaryRack.TopLeft_GlobalPoint = new Point(temporaryRack.TopLeft_GlobalPoint.X, 0 + temporaryRack.MarginY);
				if (Utils.FGT(temporaryRack.BottomRight_GlobalPoint.X, sheet.Length - temporaryRack.MarginX))
				{
					Point newTopLeftGlobalPoint = temporaryRack.TopLeft_GlobalPoint;
					newTopLeftGlobalPoint.X -= temporaryRack.BottomRight_GlobalPoint.X - sheet.Length - temporaryRack.MarginX;
					temporaryRack.TopLeft_GlobalPoint = newTopLeftGlobalPoint;
				}
				if (Utils.FGT(temporaryRack.BottomRight_GlobalPoint.Y, sheet.Width - temporaryRack.MarginY))
				{
					Point newTopLeftGlobalPoint = temporaryRack.TopLeft_GlobalPoint;
					newTopLeftGlobalPoint.Y -= temporaryRack.BottomRight_GlobalPoint.Y - sheet.Width - temporaryRack.MarginY;
					temporaryRack.TopLeft_GlobalPoint = newTopLeftGlobalPoint;
				}

				//
				List<BaseRectangleGeometry> overlappedRectangles;
				//
				List<BaseRectangleGeometry> rectanglesToCheck = new List<BaseRectangleGeometry>();
				rectanglesToCheck.Add(temporaryRack);
				List<BaseRectangleGeometry> rectanglesToIgnore = new List<BaseRectangleGeometry>();
				rectanglesToIgnore.AddRange(group);
				if (ignoreGeometryList != null && ignoreGeometryList.Count > 0)
					rectanglesToIgnore.AddRange(ignoreGeometryList);
				if (!sheet.IsLayoutCorrect(rectanglesToCheck, rectanglesToIgnore, out overlappedRectangles))
				{
					// try to fix it
					Point newTopLeftPoint;
					double newGlobalLength;
					double newGlobalWidth;
					//
					// infinity loop protection
					int iMaxLoopCount = 100;
					int iLoopCount = 0;
					//
					while (temporaryRack._CalculateNotOverlapPosition(overlappedRectangles, BaseRectangleGeometry.GRIP_CENTER, sheet.Length, sheet.Width, out newTopLeftPoint, out newGlobalLength, out newGlobalWidth))
					{
						temporaryRack.TopLeft_GlobalPoint = newTopLeftPoint;

						// check borders

						//
						if (sheet.IsLayoutCorrect(temporaryRack, rectanglesToIgnore, out overlappedRectangles))
							break;

						++iLoopCount;
						if (iLoopCount >= iMaxLoopCount)
							break;
					}
				}

				if (!sheet.IsLayoutCorrect(temporaryRack, rectanglesToIgnore, out overlappedRectangles))
				{
					// delete next racks
					if (_changedRackIndex + 1 < _previewGroup.Count)
					{
						_previewGroup.RemoveAt(_previewGroup.Count - 1);
						continue;
					}
					else
					{
						// delete all racks
						_previewGroup.Clear();
						break;
					}
				}

				// Change position of previewGroup.
				// Create rack row:
				// |   M  |  A  |  A  |
				// and change depth of M-rack without change all other racks with the same index, the second rack will be converted to M and overlap the first rack.
				if(Utils.FNE(_first.TopLeft_GlobalPoint.X, temporaryRack.TopLeft_GlobalPoint.X) || Utils.FNE(_first.TopLeft_GlobalPoint.Y, temporaryRack.TopLeft_GlobalPoint.Y))
				{
					Point rackTopLeftPoint = temporaryRack.TopLeft_GlobalPoint;
					foreach(Rack previewRack in _previewGroup)
					{
						if (previewRack == null)
							continue;

						previewRack.TopLeft_GlobalPoint = rackTopLeftPoint;
						if (previewRack.IsHorizontal)
							rackTopLeftPoint.X += previewRack.Length_X;
						else
							rackTopLeftPoint.Y += previewRack.Length_Y;
					}
				}

				break;
			}

			// mark racks for delete
			List<Rack> racksForDeleteList = new List<Rack>();
			// add current rack, because it will be replaced with clone of propertySource rack
			// if rack was skipped then dont delete it, it was converted to M-rack
			if(bDeleteOldRack)
				racksForDeleteList.Add(group[iReplaceRackIndex]);
			// delete racks and apply changes only if _previewGroup contains _previewChangedRack
			// otherwise just delete _previewChangedRack and suppose that passed group was correct(dont overlap any other geometry)
			if (_previewGroup.Contains(_previewChangedRack))
			{
				if (_previewGroup.Count < group.Count)
					racksForDeleteList.AddRange(group.GetRange(_previewGroup.Count, group.Count - _previewGroup.Count));

				// apply changes
				if (_previewGroup.Count <= group.Count)
				{
					for (int i = 0; i < _previewGroup.Count; ++i)
					{
						Rack previewRack = _previewGroup[i];
						if (previewRack == null)
							continue;

						Rack groupRack = group[i];
						if (i == iReplaceRackIndex)
							groupRack = newRack;
						if (groupRack == null)
							continue;

						groupRack.TopLeft_GlobalPoint = previewRack.TopLeft_GlobalPoint;
						groupRack.StiffenersHeight = stiffenersHeight;
						// set the column
						if (groupRack.Column != previewRack.Column)
						{
							bool bOldIsInitState = groupRack.IsInit;
							//
							groupRack.IsInit = false;
							string strCollError;
							groupRack.Set_Column(previewRack.MinimumColumn, true, false, true, out strCollError);
							if(previewRack.Column != groupRack.Column)
								groupRack.Set_Column(previewRack.Column, false, false, true, out strCollError);
							//
							groupRack.IsInit = bOldIsInitState;
						}
					}
				}
			}

			// remove racks
			if (bDeleteRacks)
			{
				if (racksForDeleteList.Count > 0)
				{
					List<BaseRectangleGeometry> geomForDeleteList = new List<BaseRectangleGeometry>();
					geomForDeleteList.AddRange(racksForDeleteList);
					sheet.DeleteGeometry(geomForDeleteList, false, false);

					foreach (Rack deleteRack in racksForDeleteList)
					{
						if (!deletedRacksList.Contains(deleteRack))
							deletedRacksList.Add(deleteRack);
					}
				}
			}

			return true;
		}

		//=============================================================================
		public static void Convert_A_to_M_Rack(DrawingSheet sheet, Rack rack, bool from_A_to_M, bool bCheckLayout, bool bTryToFixLayout = true)
		{
			if (sheet == null)
				return;

			if (rack == null)
				return;

			// try to add to the end
			Rack previewRack = rack.Clone() as Rack;
			if (previewRack == null)
				return;
			// Set sheet, otherwise DiffBetween_M_and_A returns 0.
			previewRack.Sheet = sheet;

			int iSign = 1;
			if (!from_A_to_M)
				iSign = -1;

			// increase width or height and check
			if (previewRack.IsHorizontal)
			{
				previewRack.Length_X += iSign * rack.DiffBetween_M_and_A;
				previewRack.Length_X = Utils.CheckWholeNumber(previewRack.Length_X, previewRack.MinLength_X, previewRack.MaxLength_X);
			}
			else
			{
				previewRack.Length_Y += iSign * rack.DiffBetween_M_and_A;
				previewRack.Length_Y = Utils.CheckWholeNumber(previewRack.Length_Y, previewRack.MinLength_Y, previewRack.MaxLength_Y);
			}

			//
			if (bCheckLayout)
			{
				List<BaseRectangleGeometry> _rectanglesToIgnore = new List<BaseRectangleGeometry>();
				_rectanglesToIgnore.Add(rack);
				//
				List<BaseRectangleGeometry> overlappedRectangles;
				if (!sheet.IsLayoutCorrect(previewRack, _rectanglesToIgnore, out overlappedRectangles))
				{
					if (!bTryToFixLayout)
						return;

					// try to fix it
					Point newTopLeftPoint;
					double newGlobalLength;
					double newGlobalWidth;
					//
					// infinity loop protection
					int iMaxLoopCount = 100;
					int iLoopCount = 0;
					//
					while (previewRack._CalculateNotOverlapPosition(overlappedRectangles, BaseRectangleGeometry.GRIP_CENTER, sheet.Length, sheet.Width, out newTopLeftPoint, out newGlobalLength, out newGlobalWidth))
					{
						previewRack.TopLeft_GlobalPoint = newTopLeftPoint;

						//
						if (sheet.IsLayoutCorrect(previewRack, _rectanglesToIgnore, out overlappedRectangles))
							break;

						++iLoopCount;
						if (iLoopCount >= iMaxLoopCount)
							break;
					}

					if (!sheet.IsLayoutCorrect(previewRack, _rectanglesToIgnore, out overlappedRectangles))
						return;
				}
			}

			rack.TopLeft_GlobalPoint = previewRack.TopLeft_GlobalPoint;
			rack.Length_X = previewRack.Length_X;
			rack.Length_Y = previewRack.Length_Y;
			// make it M or A
			rack.IsFirstInRowColumn = from_A_to_M;
		}

		//=============================================================================
		public static bool ChangeRack(Rack rackToChange, double rNewLength, double rNewWidth, bool bDeleteLast, out string strError)
		{
			strError = string.Empty;

			if (rNewLength <= 0 || rNewWidth <= 0)
				return false;

			if (rackToChange == null)
				return false;

			// If rack is not init then it is not placed in the layout.
			// Dont check layout in this case. This method is called from Rack's constructor,
			// when rack has (0,0) as TopLeftGlobalPoint, so if any other rack is placed
			// at (0, 0) point this method returns false and dont apply new length and width.
			if(!rackToChange.IsInit)
			{
				rackToChange.Length_X = rNewLength;
				rackToChange.Length_Y = rNewWidth;
				return true;
			}

			DrawingSheet _sheet = rackToChange.Sheet;
			if (_sheet == null)
				return false;

			DrawingDocument _doc = _sheet.Document;
			if (_doc == null)
				return false;

			List<Rack> _group = _sheet.GetRackGroup(rackToChange);
			if (_group != null && _group.Count == 0)
				_group.Add(rackToChange);
			if (_group == null || _group.Count < 1)
				return false;

			// make preview and check layout
			List<Rack> _previewGroup = new List<Rack>();
			//
			Rack _previewChangedRack = rackToChange.Clone() as Rack;
			if (_previewChangedRack == null)
				return false;
			_previewChangedRack.Length_X = rNewLength;
			_previewChangedRack.Length_Y = rNewWidth;
			_previewGroup.Add(_previewChangedRack);
			//
			int _changedRackIndex = _group.IndexOf(rackToChange);
			Rack _previosRack = _previewChangedRack;
			for (int i = _changedRackIndex - 1; i >= 0; --i)
			{
				Rack _previewRack = _group[i].Clone() as Rack;
				if (_previewRack == null)
					return false;
				//
				Point _TopLeft_GlobalPoint = _previosRack.TopLeft_GlobalPoint;
				if (rackToChange.IsHorizontal)
					_TopLeft_GlobalPoint.X -= _previewRack.Length_X + Rack.sHorizontalRow_GlobalGap;
				else
					_TopLeft_GlobalPoint.Y -= _previewRack.Length_Y + Rack.sVerticalColumn_GlobalGap;
				_previewRack.TopLeft_GlobalPoint = _TopLeft_GlobalPoint;

				_previewGroup.Insert(0, _previewRack);

				_previosRack = _previewRack;
			}
			//
			_previosRack = _previewChangedRack;
			for (int i = _changedRackIndex + 1; i < _group.Count; ++i)
			{
				Rack _previewRack = _group[i].Clone() as Rack;
				if (_previewRack == null)
					return false;
				//
				Point _TopLeft_GlobalPoint = _previosRack.TopLeft_GlobalPoint;
				if (rackToChange.IsHorizontal)
				{
					_TopLeft_GlobalPoint = _previosRack.TopRight_GlobalPoint;
					_TopLeft_GlobalPoint.X += Rack.sHorizontalRow_GlobalGap;
				}
				else
				{
					_TopLeft_GlobalPoint = _previosRack.BottomLeft_GlobalPoint;
					_TopLeft_GlobalPoint.Y += Rack.sVerticalColumn_GlobalGap;
				}
				_previewRack.TopLeft_GlobalPoint = _TopLeft_GlobalPoint;

				_previewGroup.Add(_previewRack);

				_previosRack = _previewRack;
			}

			//
			Rack _first = null;
			Rack _last = null;
			Rack temporaryRack = null;
			while (true)
			{
				// check layout
				_first = _previewGroup[0];
				_last = _previewGroup[_previewGroup.Count - 1];

				// check layout
				// make big rectangle
				temporaryRack = _first.Clone() as Rack;
				if (temporaryRack == null)
					return false;
				if (temporaryRack.IsHorizontal)
					temporaryRack.Length_X = _last.TopRight_GlobalPoint.X - _first.TopLeft_GlobalPoint.X;
				else
					temporaryRack.Length_Y = _last.BottomLeft_GlobalPoint.Y - _first.TopLeft_GlobalPoint.Y;

				// check borders
				if (temporaryRack.Length_X > _sheet.Length || temporaryRack.Length_Y > _sheet.Width)
				{
					if (bDeleteLast)
					{
						if (_changedRackIndex + 1 < _previewGroup.Count)
						{
							_previewGroup.RemoveAt(_previewGroup.Count - 1);
							continue;
						}
						else
						{
							strError = "Rack falls out of the storage area after length or depth change.";
							return false;
						}
					}
					else
					{
						strError = "Rack falls out of the storage area after length or depth change.";
						return false;
					}
				}

				if (Utils.FLT(temporaryRack.TopLeft_GlobalPoint.X, 0 + temporaryRack.MarginX))
					temporaryRack.TopLeft_GlobalPoint = new Point(0 + temporaryRack.MarginX, temporaryRack.TopLeft_GlobalPoint.Y);
				if (Utils.FLT(temporaryRack.TopLeft_GlobalPoint.Y, 0 + temporaryRack.MarginY))
					temporaryRack.TopLeft_GlobalPoint = new Point(temporaryRack.TopLeft_GlobalPoint.X, 0 + temporaryRack.MarginY);
				if (Utils.FGT(temporaryRack.BottomRight_GlobalPoint.X, _sheet.Length - temporaryRack.MarginX))
				{
					Point newTopLeftGlobalPoint = temporaryRack.TopLeft_GlobalPoint;
					newTopLeftGlobalPoint.X -= temporaryRack.BottomRight_GlobalPoint.X - _sheet.Length - temporaryRack.MarginX;
					temporaryRack.TopLeft_GlobalPoint = newTopLeftGlobalPoint;
				}
				if (Utils.FGT(temporaryRack.BottomRight_GlobalPoint.Y, _sheet.Width - temporaryRack.MarginY))
				{
					Point newTopLeftGlobalPoint = temporaryRack.TopLeft_GlobalPoint;
					newTopLeftGlobalPoint.Y -= temporaryRack.BottomRight_GlobalPoint.Y - _sheet.Width - temporaryRack.MarginY;
					temporaryRack.TopLeft_GlobalPoint = newTopLeftGlobalPoint;
				}

				//
				List<BaseRectangleGeometry> overlappedRectangles;
				//
				List<BaseRectangleGeometry> rectanglesToCheck = new List<BaseRectangleGeometry>() { temporaryRack };
				List<BaseRectangleGeometry> rectanglesToIgnore = new List<BaseRectangleGeometry>();
				rectanglesToIgnore.AddRange(_group);
				if (!_sheet.IsLayoutCorrect(rectanglesToCheck, rectanglesToIgnore, out overlappedRectangles))
				{
					// try to fix it
					Point newTopLeftPoint;
					double newGlobalLength;
					double newGlobalWidth;
					//
					// infinity loop protection
					int iMaxLoopCount = 100;
					int iLoopCount = 0;
					//
					while (temporaryRack._CalculateNotOverlapPosition(overlappedRectangles, BaseRectangleGeometry.GRIP_CENTER, _sheet.Length, _sheet.Width, out newTopLeftPoint, out newGlobalLength, out newGlobalWidth))
					{
						temporaryRack.TopLeft_GlobalPoint = newTopLeftPoint;

						// check borders

						//
						if (_sheet.IsLayoutCorrect(temporaryRack, rectanglesToIgnore, out overlappedRectangles))
							break;

						++iLoopCount;
						if (iLoopCount >= iMaxLoopCount)
							break;
					}
				}

				if (!_sheet.IsLayoutCorrect(temporaryRack, rectanglesToIgnore, out overlappedRectangles))
				{
					if (bDeleteLast)
					{
						if (_changedRackIndex + 1 < _previewGroup.Count)
						{
							_previewGroup.RemoveAt(_previewGroup.Count - 1);
							continue;
						}
						else
						{
							strError = "Rack overlaps other geometry after length or depth change.";
							return false;
						}
					}
					else
					{
						strError = "Rack overlaps other geometry after length or depth change.";
						return false;
					}
				}

				break;
			}

			// update all racks
			// Update order of rackToChange in the end, after all other racks are changed.
			// Otherwise, if rackToChange is first in the row, it can keep old SizeIndex and all other racks will receive new SizeIndex.
			rackToChange.m_bDontChangeOrder = true;
			rackToChange.Length_X = rNewLength;
			rackToChange.Length_Y = rNewWidth;
			// remove racks
			if (_previewGroup.Count < _group.Count)
			{
				List<Rack> racksForDeleteList = new List<Rack>();
				racksForDeleteList.AddRange(_group.GetRange(_previewGroup.Count, _group.Count - _previewGroup.Count));
				//
				List<BaseRectangleGeometry> geomForDeleteList = new List<BaseRectangleGeometry>();
				geomForDeleteList.AddRange(racksForDeleteList);
				_sheet.DeleteGeometry(geomForDeleteList, false, false);
				//
				foreach (Rack deletedRack in racksForDeleteList)
					_sheet.SelectedGeometryCollection.Remove(deletedRack);
				//
				if (_sheet.Document != null && _sheet.Document.ShowAdvancedProperties)
					_sheet.Document.Set_ShowAdvancedProperties(false, false);

				if (racksForDeleteList.Count > 0)
				{
					string strMessage = "Racks, which fall out of the storage area or overlap other geometry, are deleted due to change in rack length or depth. " + racksForDeleteList.Count.ToString();
					if (racksForDeleteList.Count > 1)
						strMessage += " racks were deleted.";
					else
						strMessage += " rack was deleted.";

					if(_sheet.Document != null)
						_sheet.Document.DocumentError = strMessage;
				}
			}
			//
			_group[0].TopLeft_GlobalPoint = temporaryRack.TopLeft_GlobalPoint;
			for (int i = 1; i < _group.Count; ++i)
			{
				Rack prevRack = _group[i - 1];
				Rack currentRack = _group[i];

				if (prevRack != null && currentRack != null)
				{
					Point curRackTopLeft_GlobalPoint = new Point();
					if (currentRack.IsHorizontal)
					{
						curRackTopLeft_GlobalPoint = prevRack.TopRight_GlobalPoint;
						curRackTopLeft_GlobalPoint.X += Rack.sHorizontalRow_GlobalGap;
					}
					else
					{
						curRackTopLeft_GlobalPoint = prevRack.BottomLeft_GlobalPoint;
						curRackTopLeft_GlobalPoint.Y += Rack.sVerticalColumn_GlobalGap;
					}

					currentRack.TopLeft_GlobalPoint = curRackTopLeft_GlobalPoint;
				}
			}

			// Update change order of the rack.
			rackToChange.m_bDontChangeOrder = false;
			rackToChange.MarkStateChanged();

			// dont update sheet - it will update rack index
			// we need update rack's row column if the last rack(s) was deleted
			_sheet.RegroupRacks();

			return true;
		}

		//=============================================================================
		/// <summary>
		/// Calculate the minimum available column for the racks.
		/// Racks group means racks row\column, where racks are placed one another one - the first rack has 0 index, the second rack has 1 index, etc.
		/// </summary>
		public static bool CalculateRacksColumnSizeAndBracingType(List<Rack> racksGroup, List<RackColumn> columnsList, out eColumnBracingType resultBracing, /*out double xBracingHeight,*/ out RackColumn minAvailableColumn, out double stiffenersHeight)
		{
			return CalculateRacksColumnSizeAndBracingType(racksGroup, columnsList, false, out resultBracing, /*out xBracingHeight,*/ out minAvailableColumn, out stiffenersHeight);
		}
		public static bool CalculateRacksColumnSizeAndBracingType(List<Rack> racksGroup, List<RackColumn> columnsList, bool bForceXBracingHeightCalc, out eColumnBracingType resultBracing, /*out double xBracingHeight,*/ out RackColumn minAvailableColumn, out double stiffenersHeight)
		{
			// Go through all racks in the rack group and calculate column size and bracing type.

			// xBracingHeight depends only on the rack properties(MaterialOnGround, Underpass and levels height)
			// Dont calculate xBracingHeight here, instead calculate eColumnBracingType only.
			// X bracing height will be calculated in Rack.MinXBracingHeight property.
			// See FrameGuidelines.docx.
			//xBracingHeight = 0.0;
			resultBracing = eColumnBracingType.eUndefined;
			minAvailableColumn = null;
			// Max height of the level(with pallets) which requires stiffener.
			stiffenersHeight = 0.0;

			// Column size and bracing calc algo:
			// 1. Consider left and right column of the rack. Each column takes half of the load of the left and right rack.
			// 2. Go through racks levels and calculate USL(length from prev level or ground) and load in level and column connection point.
			// 3. To calculate load need to get all connection point between column and level from the left and right rack.
			// 4. Go trough all connection point from bot to top:
			//    4.1 Calc USL - distance from connection point and previous level or ground.
			//    4.2 Calc load - sum of half of levels(from left and right rack) load which are greater of equal than current connection point.
			// 5. Get bracing type and column based on RackLoadUtils.m_RacksColumnsList(it is data from the LoadChart.xlsx).

			if (racksGroup == null)
				return false;

			if (columnsList == null || columnsList.Count == 0)
				return false;

			for (int i = 0; i < racksGroup.Count; ++i)
			{
				Rack currRack = racksGroup[i];
				if (currRack == null)
					continue;

				Rack nextRack = null;
				if (i + 1 < racksGroup.Count)
					nextRack = racksGroup[i + 1];

				List<Rack> racksIncludeInCalc = new List<Rack>();
				racksIncludeInCalc.Add(currRack);
				if (nextRack != null)
					racksIncludeInCalc.Add(nextRack);

				// Dict with distance from the ground to tops of levels beams.
				SortedDictionary<double, List<RackLevel>> levelsHeightFromGroundDict = new SortedDictionary<double, List<RackLevel>>();
				foreach (Rack rack in racksIncludeInCalc)
				{
					if (rack.Levels == null)
						continue;

					foreach (RackLevel level in rack.Levels)
					{
						if (level == null)
							continue;

						double levelDistanceFromTheGround = level.DistanceFromTheGround;
						if (Utils.FGT(levelDistanceFromTheGround, 0.0))
						{
							double distanceToTheTopOfBeam = levelDistanceFromTheGround;
							if (level.Beam != null)
								distanceToTheTopOfBeam += level.Beam.Height;

							if (!levelsHeightFromGroundDict.ContainsKey(distanceToTheTopOfBeam))
								levelsHeightFromGroundDict[distanceToTheTopOfBeam] = new List<RackLevel>();

							levelsHeightFromGroundDict[distanceToTheTopOfBeam].Add(level);
						}
					}
				}

				//
				if (levelsHeightFromGroundDict.Keys.Count == 0)
					return false;

				// Go through connection point and calculate load.
				// The lowest level has the maximum load, so the biggest column value we can receive only in that(the lowest) point for each rack.
				// Check column only once for each(current and next) rack, because it depends on the rack beam span.
				//
				// List with racks for column check, read comment above.
				List<Rack> racksColumnsToCheckList = new List<Rack>();
				racksColumnsToCheckList.Add(currRack);
				if (nextRack != null)
					racksColumnsToCheckList.Add(nextRack);
				// List with rack for bracing check. Normal bracing is the lowest available bracing, so if we receive normal bracing in the calculations
				// then dont calc bracing for this rack further.
				List<Rack> racksBracingToCheckList = new List<Rack>();
				racksBracingToCheckList.Add(currRack);
				if (nextRack != null)
					racksBracingToCheckList.Add(nextRack);
				//
				double prevLevelDistance = 0.0;
				foreach (double distanceToTopOfLevelBeam in levelsHeightFromGroundDict.Keys)
				{
					List<RackLevel> levelsList = levelsHeightFromGroundDict[distanceToTopOfLevelBeam];

					// Total load from thim point to the top of the rack.
					double totalLoadInPoint = 0.0;
					RackLevel prevLevel;
					totalLoadInPoint += currRack.CalculateColumnLoadInPoint(distanceToTopOfLevelBeam, out prevLevel);
					if (nextRack != null)
						totalLoadInPoint += nextRack.CalculateColumnLoadInPoint(distanceToTopOfLevelBeam, out prevLevel);

					// Measure USL distance from top of the current level to the nearest lower level on this rack or neighbor rack.
					double uslDistance = distanceToTopOfLevelBeam - prevLevelDistance;
					prevLevelDistance = distanceToTopOfLevelBeam;

					foreach (RackLevel level in levelsList)
					{
						if (racksBracingToCheckList.Count == 0 && racksColumnsToCheckList.Count == 0)
							break;

						// Calc bracing and column.
						eColumnBracingType bracing;
						RackColumn column = RackLoadUtils.GetRackColumn(columnsList, level.Owner.BeamLength, uslDistance, totalLoadInPoint, out bracing);
						if (column != null)
						{
							// Normal bracing is the lowest available bracing, so if we receive normal bracing in calc then
							// dont calc bracing for this rack further.
							if (eColumnBracingType.eNormalBracing == bracing && racksBracingToCheckList.Contains(level.Owner))
								racksBracingToCheckList.Remove(level.Owner);

							// Check column only once for each(current and next) rack.
							if (racksColumnsToCheckList.Contains(level.Owner))
							{
								racksColumnsToCheckList.Remove(level.Owner);

								// Change stiffener height only if minAvailableColumn changes or bracing type changes.
								bool bCheckStiffenerHeight = false;

								if (minAvailableColumn == null)
								{
									minAvailableColumn = column;
									resultBracing = bracing;
									bCheckStiffenerHeight = true;

									//if (bForceXBracingHeightCalc || eColumnBracingType.eXBracing == bracing || eColumnBracingType.eXBracingWithStiffener == bracing)
									//	xBracingHeight = distanceToTopOfLevelBeam;
									//else
									//	xBracingHeight = 0.0;
								}
								else
								{
									// Compare by the length and thickness, look at the LoadChart.xlsx file.
									if (Utils.FGT(column.Length, minAvailableColumn.Length) || (Utils.FGE(column.Length, minAvailableColumn.Length) && Utils.FGT(column.Thickness, minAvailableColumn.Thickness)))
									{
										minAvailableColumn = column;
										resultBracing = bracing;
										bCheckStiffenerHeight = true;

										//if (bForceXBracingHeightCalc || eColumnBracingType.eXBracing == bracing || eColumnBracingType.eXBracingWithStiffener == bracing)
										//	xBracingHeight = distanceToTopOfLevelBeam;
										//else
										//	xBracingHeight = 0.0;
									}
									else
									{
										// If columns are equal then check bracing.
										// Take the biggest calculated bracing for this column.
										if (resultBracing < bracing)
										{
											resultBracing = bracing;
											bCheckStiffenerHeight = true;

											//if (bForceXBracingHeightCalc || eColumnBracingType.eXBracing == bracing || eColumnBracingType.eXBracingWithStiffener == bracing)
											//	xBracingHeight = distanceToTopOfLevelBeam;
											//else
											//	xBracingHeight = 0.0;
										}
									}
								}

								if (bCheckStiffenerHeight)
								{
									if (eColumnBracingType.eNormalBracingWithStiffener == bracing || eColumnBracingType.eXBracingWithStiffener == bracing)
									{
										double newStiffenersHeight = distanceToTopOfLevelBeam;
										if (Utils.FLT(stiffenersHeight, newStiffenersHeight))
											stiffenersHeight = newStiffenersHeight;
									}
								}
							}
						}
						else
						{
							// Stop searching when column is null, probably it happens because USL distance is greater than MAX_USL_DISTANCE(3500) in the LoadChart.xlsx.
							return false;
						}
					}
				}
			}

			// Column is not found.
			if (minAvailableColumn == null)
				return false;

			return true;
		}

		//=============================================================================
		/// <summary>
		/// Go through headerRacks and try to find BTB-racks.
		/// Calculate H\D ratio for them. It affects tie beam placement.
		/// </summary>
		//public static Dictionary<Rack, eTieBeamPlacement> Check_BTB_Racks(List<Rack> headerRacks, List<List<Rack>> racksGroups)
		//{
		//	Dictionary<Rack, eTieBeamPlacement> rack
		//}

		//=============================================================================
		private static char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
		/// <summary>
		/// Returns alphabet rack index like "A", "B", "AA".
		/// </summary>
		public static string GetAlphabetRackIndex(int index)
		{
			string strAlphabetIndex = string.Empty;

			if (index == 1)
				strAlphabetIndex += alpha[0];
			else if (index > 0)
			{
				int iAlphabetCharsCount = alpha.Count();
				int iLettersCount = (int)Math.Ceiling(Math.Log((double)index, (double)iAlphabetCharsCount));
				for (int i = iLettersCount; i > 0; --i)
				{
					if (index == 0)
					{
						strAlphabetIndex += alpha[0];
						break;
					}

					int iDecade = (int)Math.Pow((double)iAlphabetCharsCount, (double)i - 1);
					int iLetterIndex = (int)Math.Floor((double)index / iDecade);

					if (iLetterIndex > 0 && iLetterIndex <= iAlphabetCharsCount)
						strAlphabetIndex += alpha[iLetterIndex - 1];

					index -= iLetterIndex * iDecade;
				}
			}
			else
				strAlphabetIndex += "@";

			return strAlphabetIndex;
		}

		//=============================================================================
		/// <summary>
		/// Build sorted dictionary. Ground level is not included.
		/// Key - distance from the ground to the top of level beam
		/// Value - level
		/// </summary>
		public static void BuildLevelsHeightDictionary(ref SortedDictionary<double, RackLevel> levelsHeightDict, IEnumerable<RackLevel> levelsList)
		{
			if (levelsHeightDict == null)
				return;

			if (levelsList == null)
				return;

			foreach(RackLevel level in levelsList)
			{
				if (level == null)
					continue;

				if (level.Index == 0)
					continue;

				double levelHeight = level.DistanceFromTheGround;
				if (Utils.FLE(levelHeight, 0.0))
					continue;

				if (level.Beam != null)
					levelHeight += level.Beam.Height;

				if (levelsHeightDict.ContainsKey(levelHeight))
					continue;

				levelsHeightDict[levelHeight] = level;
			}
		}
		//=============================================================================
		/// <summary>
		/// Calculate X bracing height.
		/// Read comment to Rack.X_BRACING_MIN_HEIGHT.
		/// </summary>
		public static bool CalculateXBracingHeight(Rack rack, SortedDictionary<double, RackLevel> levelsHeightDict, out double xBracingHeight)
		{
			xBracingHeight = 0.0;

			if (rack == null)
				return false;

			if (levelsHeightDict == null)
				return false;

			if (rack.IsMaterialOnGround || rack.IsUnderpassAvailable)
			{
				if (levelsHeightDict.Keys.Count < 1)
					return false;
				xBracingHeight = levelsHeightDict.ElementAt(0).Key;
			}
			else
			{
				if (levelsHeightDict.Keys.Count < 2)
					return false;
				xBracingHeight = levelsHeightDict.ElementAt(1).Key;
			}

			return true;
		}

		//=============================================================================
		/// <summary>
		/// Column height should be in multiples of 100.
		/// Round 0-50 to 0, 51-99 to 100.
		/// </summary>
		public static double RoundColumnHeight(double columnHeightValue)
		{
			double value = Math.Floor(columnHeightValue / 100) * 100;
			double remainder = columnHeightValue % 100;
			if (Utils.FGT(remainder, 50.0))
				value += 100;
			return value;
		}

		//=============================================================================
		/// <summary>
		/// Returns the biggest rack height from the racks list.
		/// </summary>
		public static double GetTheBiggestRackHeight(List<Rack> racksList)
		{
			double theBiggestRackHeght = 0.0;

			if(racksList != null)
			{
				foreach(Rack rack in racksList)
				{
					if (rack == null)
						continue;

					double rackMaxHeight = rack.MaxHeight;
					if (Utils.FGT(rackMaxHeight, theBiggestRackHeght))
						theBiggestRackHeght = rackMaxHeight;
				}
			}

			return theBiggestRackHeght;
		}
	}
}
