using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public class Column_State : GeometryState
	{
		public Column_State(Column column)
			: base(column)
		{
			this.SizeIndex = column.SizeIndex;
			this.CanContinuePattern = column.CanContinuePattern;
			this.ColumnPattern_ID = column.ColumnPattern_ID;
		}
		public Column_State(Column_State state)
			: base(state)
		{
			this.SizeIndex = state.SizeIndex;
			this.CanContinuePattern = state.CanContinuePattern;
			this.ColumnPattern_ID = state.ColumnPattern_ID;
		}

		//
		public int SizeIndex { get; private set; }
		public bool CanContinuePattern { get; private set; }
		public int ColumnPattern_ID { get; private set; }

		//=============================================================================
		protected override GeometryState MakeDeepCopy()
		{
			return new Column_State(this);
		}
	}

	[Serializable]
	public class Column : BaseRectangleGeometry, ISerializable
	{
		public static string PROP_COLUMN_PATTERN_DISTANCE_X = "Column pattern distance X";
		public static string PROP_COLUMN_PATTERN_DISTANCE_Y = "Column pattern distance Y";

		public Column(DrawingSheet ds)
			: base(ds)
		{
			Text = "C";

			//
			StepLength_X = 25;
			StepLength_Y = 25;

			MinLength_Y = 300;
			MinLength_X = 300;

			//
			MaxLength_X = 1000;
			MaxLength_Y = 1000;

			//
			Length_X = 1000;
			Length_Y = 1000;

			//
			m_FillColor = Colors.LightSkyBlue;

			ColumnPattern_ID = -1;
			CanContinuePattern = true;
		}

		#region Properties

		//=============================================================================
		/// <summary>
		/// Read comment to Column.ColumnPattern_ID
		/// </summary>
		public ColumnPattern Pattern
		{
			get
			{
				if (m_Sheet != null)
					return m_Sheet.GetColumnPattern(ColumnPattern_ID);

				return null;
			}
		}

		//=============================================================================
		/// <summary>
		/// ColumnPattern ID.
		/// All column in the pattern have the same ColumnPattern ID.
		/// It is used to find all column in the pattern.
		/// </summary>
		public int ColumnPattern_ID { get; set; }

		//=============================================================================
		/// <summary>
		/// If true then "Continue Pattern" grip point will be displayed at this column.
		/// It means, that new columns can be created. Only bot right column in the pattern can have
		/// this grip point.
		/// </summary>
		public bool CanContinuePattern { get; set; }

		//=============================================================================
		private int m_iSizeIndex = -1;
		public int SizeIndex
		{
			get { return m_iSizeIndex; }
			set
			{
				if(m_iSizeIndex != value)
				{
					m_iSizeIndex = value;
					int _displayIndex = m_iSizeIndex + 1;
					Text = "C" + _displayIndex.ToString();
				}
			}
		}

		#endregion

		#region Functions

		//=============================================================================
		protected override void _InitProperties()
		{
			base._InitProperties();

			try
			{
				//
				if (m_Properties.FirstOrDefault(p => p.SystemName == Column.PROP_COLUMN_PATTERN_DISTANCE_X) == null)
					m_Properties.Add(new GeometryProperty(this, Column.PROP_COLUMN_PATTERN_DISTANCE_X, "Pattern distance X", true, "Pattern"));
				//
				if (m_Properties.FirstOrDefault(p => p.SystemName == Column.PROP_COLUMN_PATTERN_DISTANCE_Y) == null)
					m_Properties.Add(new GeometryProperty(this, Column.PROP_COLUMN_PATTERN_DISTANCE_Y, "Pattern distance Y", true, "Pattern"));
			}
			catch { }
		}

		//=============================================================================
		protected override GeometryState _GetOriginalState()
		{
			return new Column_State(this);
		}
		//=============================================================================
		protected override void _SetState(GeometryState state)
		{
			base._SetState(state);

			Column_State columnState = state as Column_State;
			if (columnState == null)
				return;

			this.m_iSizeIndex = columnState.SizeIndex;
			this.CanContinuePattern = columnState.CanContinuePattern;
			this.ColumnPattern_ID = columnState.ColumnPattern_ID;
		}
		//=============================================================================
		protected override BaseRectangleGeometry CreateInstance() { return new Column(null); }

		//=============================================================================
		public override bool SetGripPoint(int gripIndex, Point pnt, double DrawingLength, double DrawingWidth)
		{
			if (Wrapper == null || Wrapper.Owner == null)
				return false;

			if (GRIP_TOP_LEFT == gripIndex || GRIP_BOTTOM_RIGHT == gripIndex || GRIP_CENTER == gripIndex)
			{
				GeometryState oldState = this._GetClonedState();

				bool bRes = true;
				// change size of all columns in the column pattern
				if (GRIP_TOP_LEFT == gripIndex)
					bRes = _Column_SetPropertyValue(PROP_TOP_LEFT_POINT, pnt, DrawingLength, DrawingWidth, false, false);
				else if (GRIP_BOTTOM_RIGHT == gripIndex)
					bRes = _Column_SetPropertyValue(PROP_BOT_RIGHT_POINT, pnt, DrawingLength, DrawingWidth, false, false);
				else if (GRIP_CENTER == gripIndex)
					bRes = _Column_SetPropertyValue(PROP_CENTER_POINT, pnt, DrawingLength, DrawingWidth, false, true);

				if (!bRes)
					this._SetState(oldState);

				if(bRes)
					_MarkStateChanged();

				this.UpdateProperties();

				return bRes;
			}

			return base.SetGripPoint(gripIndex, pnt, DrawingLength, DrawingWidth);
		}

		//=============================================================================
		public override object GetPropertyValue(string strPropSysName)
		{
			if (string.IsNullOrEmpty(strPropSysName))
				return null;

			if (Column.PROP_COLUMN_PATTERN_DISTANCE_X == strPropSysName || Column.PROP_COLUMN_PATTERN_DISTANCE_Y == strPropSysName)
			{
				ColumnPattern _pattern = this.Pattern;

				if (Column.PROP_COLUMN_PATTERN_DISTANCE_X == strPropSysName)
				{
					if (_pattern != null)
						return _pattern.GlobalOffset_X;
					else
						return 0;
				}

				if (Column.PROP_COLUMN_PATTERN_DISTANCE_Y == strPropSysName)
				{
					if (_pattern != null)
						return _pattern.GlobalOffset_Y;
					else
						return 0;
				}
			}

			return base.GetPropertyValue(strPropSysName);
		}

		//=============================================================================
		public override bool SetPropertyValue(string strPropSysName, object propValue, bool bWasChangedViaProperties, bool bChangeTheSameRectangles, bool bNotifySheet, out string strError, bool bCheckLayout = true)
		{
			strError = string.Empty;

			if (m_Sheet == null)
				return false;

			GeometryState oldState = this._GetClonedState();

			bool bRes = false;
			bool bPropertyWasTriedToSet = false;
			try
			{
				if (Column.PROP_COLUMN_PATTERN_DISTANCE_X == strPropSysName || Column.PROP_COLUMN_PATTERN_DISTANCE_Y == strPropSysName)
				{
					bPropertyWasTriedToSet = true;

					if (Column.PROP_COLUMN_PATTERN_DISTANCE_X == strPropSysName)
					{
						//
						double rDistanceX = Convert.ToDouble(propValue);
						bRes = ColumnPattern.ChangeOffsetX(this, rDistanceX, m_Sheet.Length, m_Sheet.Width, true);
					}
					else if (Column.PROP_COLUMN_PATTERN_DISTANCE_Y == strPropSysName)
					{
						//
						double rDistanceY = Convert.ToDouble(propValue);
						bRes = ColumnPattern.ChangeOffsetY(this, rDistanceY, m_Sheet.Length, m_Sheet.Width, true);
					}
				}
				else if (PROP_DIMENSION_X == strPropSysName)
				{
					bPropertyWasTriedToSet = true;

					double rNewLength = Convert.ToDouble(propValue);
					bRes = _Column_SetPropertyValue(PROP_DIMENSION_X, rNewLength, m_Sheet.Length, m_Sheet.Width, bWasChangedViaProperties, false, bCheckLayout);
				}
				else if (PROP_DIMENSION_Y == strPropSysName)
				{
					bPropertyWasTriedToSet = true;

					double rNewWidth = Convert.ToDouble(propValue);
					bRes = _Column_SetPropertyValue(PROP_DIMENSION_Y, rNewWidth, m_Sheet.Length, m_Sheet.Width, bWasChangedViaProperties, false, bCheckLayout);
				}
				else if(PROP_TOP_LEFT_POINT_X == strPropSysName || PROP_TOP_LEFT_POINT_Y == strPropSysName)
				{
					bPropertyWasTriedToSet = true;

					double rNewValue = Convert.ToDouble(propValue);

					Point newTopLeftPoint = this.TopLeft_GlobalPoint;
					if (PROP_TOP_LEFT_POINT_X == strPropSysName)
						newTopLeftPoint.X = rNewValue;
					else if (PROP_TOP_LEFT_POINT_Y == strPropSysName)
						newTopLeftPoint.Y = rNewValue;

					bRes = _Column_SetPropertyValue(PROP_TOP_LEFT_POINT, newTopLeftPoint, m_Sheet.Length, m_Sheet.Width, bWasChangedViaProperties, true, bCheckLayout);
				}
			}
			catch { }

			if(!bPropertyWasTriedToSet)
				bRes = base.SetPropertyValue(strPropSysName, propValue, bWasChangedViaProperties, bChangeTheSameRectangles, false, out strError, bCheckLayout);

			if(bRes)
				_MarkStateChanged();

			if (m_Sheet != null && bNotifySheet)
				m_Sheet.OnPropertyChanged(this, strPropSysName, bRes, strError);

			if (!bRes)
				this._SetState(oldState);

			this.UpdateProperties();

			return bRes;
		}

		//=============================================================================
		private bool _Column_SetPropertyValue(string strPropSysName, object value, double DrawingLength, double DrawingWidth, bool bWasChangedViaProperties, bool bJustMove, bool bCheckLayout = true)
		{
			if (m_Sheet == null || m_Sheet.Document == null)
				return false;

			if (string.IsNullOrEmpty(strPropSysName))
				return false;

			try
			{
				// get the column pattern
				ColumnPattern pattern = Pattern;
				if (pattern == null)
					return false;
				//
				Point patternStartPoint;
				int columnsCount;
				int rowsCount;
				List<Column> patternColumnsList;
				List<List<Column>> patternRows;
				if (!pattern.GetPatternInfo(out patternStartPoint, out columnsCount, out rowsCount, out patternColumnsList, out patternRows))
					return false;

				//
				if (patternColumnsList == null)
					return false;

				// change size of all columns in the column pattern
				if ((PROP_TOP_LEFT_POINT == strPropSysName || PROP_BOT_RIGHT_POINT == strPropSysName) && !bJustMove)
				{
					if (!(value is Point))
						return false;

					Point newGlobalPoint = (Point)value;
					newGlobalPoint = Utils.CheckBorders(newGlobalPoint, 0.0, 0.0, DrawingLength, DrawingWidth, MarginX, MarginY);

					// calc new width and height
					double newLength = Length;
					double newDepth = Depth;
					//
					double changeX = 0.0;
					double changeY = 0.0;
					if (PROP_TOP_LEFT_POINT == strPropSysName)
					{
						changeX = Utils.GetWholeNumber(TopRight_GlobalPoint.X - newGlobalPoint.X);
						changeY = Utils.GetWholeNumber(BottomLeft_GlobalPoint.Y - newGlobalPoint.Y);
					}
					else if (PROP_BOT_RIGHT_POINT == strPropSysName)
					{
						changeX = Utils.GetWholeNumber(newGlobalPoint.X - TopLeft_GlobalPoint.X);
						changeY = Utils.GetWholeNumber(newGlobalPoint.Y - TopLeft_GlobalPoint.Y);
					}
					//
					if(this.IsHorizontal)
					{
						newLength = changeX;
						newDepth = changeY;
					}
					else
					{
						newLength = changeY;
						newDepth = changeX;
					}

					return ChangeSize(DrawingLength, DrawingWidth, true, newLength, true, newDepth, true, !bWasChangedViaProperties, bWasChangedViaProperties, bCheckLayout);
				}
				else if (PROP_CENTER_POINT == strPropSysName || (PROP_TOP_LEFT_POINT == strPropSysName && bJustMove))
				{
					// move all columns in the pattern

					if (!(value is Point))
						return false;

					Point newGlobalPoint = (Point)value;

					//
					newGlobalPoint = Utils.CheckBorders(newGlobalPoint, 0.0, 0.0, DrawingLength, DrawingWidth, MarginX, MarginY);

					// calc offset
					double OffsetX = Utils.GetWholeNumber(newGlobalPoint.X - this.Center_GlobalPoint.X);
					if (PROP_TOP_LEFT_POINT == strPropSysName && bJustMove)
						OffsetX = Utils.GetWholeNumber(newGlobalPoint.X - this.TopLeft_GlobalPoint.X);
					//
					double OffsetY = Utils.GetWholeNumber(newGlobalPoint.Y - this.Center_GlobalPoint.Y);
					if (PROP_TOP_LEFT_POINT == strPropSysName && bJustMove)
						OffsetY = Utils.GetWholeNumber(newGlobalPoint.Y - this.TopLeft_GlobalPoint.Y);

					// check borders
					foreach (Column c in patternColumnsList)
					{
						Point newTopLeft_GlobalPoint = c.TopLeft_GlobalPoint;
						newTopLeft_GlobalPoint.X += OffsetX;
						newTopLeft_GlobalPoint.Y += OffsetY;

						//
						if (newTopLeft_GlobalPoint.X < 0)
						{
							// just dont correct value is it was setted via properties
							if (bWasChangedViaProperties)
								return false;
							newTopLeft_GlobalPoint.X = 0;
						}
						//
						if (newTopLeft_GlobalPoint.Y < 0)
						{
							// just dont correct value is it was setted via properties
							if (bWasChangedViaProperties)
								return false;
							newTopLeft_GlobalPoint.Y = 0;
						}
						//
						if (newTopLeft_GlobalPoint.X + c.Length_X > DrawingLength)
						{
							// just dont correct value is it was setted via properties
							if (bWasChangedViaProperties)
								return false;
							newTopLeft_GlobalPoint.X = DrawingLength - c.Length_X;
						}
						//
						if (newTopLeft_GlobalPoint.Y + c.Length_Y > DrawingWidth)
						{
							// just dont correct value is it was setted via properties
							if (bWasChangedViaProperties)
								return false;
							newTopLeft_GlobalPoint.Y = DrawingWidth - c.Length_Y;
						}

						//
						OffsetX = Utils.GetWholeNumber(newTopLeft_GlobalPoint.X - c.TopLeft_GlobalPoint.X);
						OffsetY = Utils.GetWholeNumber(newTopLeft_GlobalPoint.Y - c.TopLeft_GlobalPoint.Y);
					}

					// make preview pattern
					List<BaseRectangleGeometry> _previewColumnsPattern = new List<BaseRectangleGeometry>();
					//
					foreach (Column c in patternColumnsList)
					{
						Column _previewColumn = c.Clone() as Column;
						if (_previewColumn == null)
							return false;
						_previewColumn.Sheet = m_Sheet;
						//
						Point _previewTopLeftPoint = c.TopLeft_GlobalPoint;
						_previewTopLeftPoint.X += OffsetX;
						_previewTopLeftPoint.Y += OffsetY;
						//
						_previewTopLeftPoint = Utils.GetWholePoint(_previewTopLeftPoint);
						//
						_previewColumn.TopLeft_GlobalPoint = _previewTopLeftPoint;
						_previewColumn.Length_X = c.Length_X;
						_previewColumn.Length_Y = c.Length_Y;

						//
						_previewColumnsPattern.Add(_previewColumn);
					}

					//
					List<BaseRectangleGeometry> _rectanglesToIgnore = new List<BaseRectangleGeometry>();
					_rectanglesToIgnore.AddRange(patternColumnsList);
					// check layout
					List<BaseRectangleGeometry> overlappedRectangles;
					if (!IsInit || !bCheckLayout || m_Sheet.IsLayoutCorrect(_previewColumnsPattern, _rectanglesToIgnore, out overlappedRectangles))
					{
						// layout is correct, make changes
						foreach (Column c in patternColumnsList)
						{
							//
							Point _newTopLeftPoint = c.TopLeft_GlobalPoint;
							_newTopLeftPoint.X += OffsetX;
							_newTopLeftPoint.Y += OffsetY;
							_newTopLeftPoint = Utils.GetWholePoint(_newTopLeftPoint);
							//
							c.TopLeft_GlobalPoint = _newTopLeftPoint;
						}

						return true;
					}
				}
				else if(PROP_DIMENSION_X == strPropSysName || PROP_DIMENSION_Y == strPropSysName)
				{
					if (!(value is double))
						return false;

					// true - change length
					// false - change depth
					bool bChangeLength = (PROP_DIMENSION_X == strPropSysName && this.IsHorizontal) || (PROP_DIMENSION_Y == strPropSysName && !this.IsHorizontal);
					double newValue = (double)value;

					//
					bool bChangeAllColumns = true;
					if (bWasChangedViaProperties && patternColumnsList.Count > 1)
					{
						MessageBoxResult mbResult = MessageBox.Show("Would you like to apply this change for all columns in the pattern?", "Warning", MessageBoxButton.YesNoCancel);
						if (mbResult == MessageBoxResult.Cancel)
							return false;
						else if (mbResult == MessageBoxResult.No)
							bChangeAllColumns = false;
					}

					double newLength = 0.0;
					double newDepth = 0.0;
					if (bChangeLength)
						newLength = newValue;
					else
						newDepth = newValue;
					// Dont update current column size index, because it is used for find same size index columns in other patterns.
					bool bRes = ChangeSize(DrawingLength, DrawingWidth, bChangeLength, newLength, !bChangeLength, newDepth, bChangeAllColumns, !bWasChangedViaProperties, false, bCheckLayout);

					// Try to change same size index columns in another patterns.
					if (bWasChangedViaProperties && this.IsInit && Sheet != null)
						Sheet.OnColumnSizeChanged(this);

					if(bWasChangedViaProperties && Sheet != null)
						Sheet.Document.RecalcColumnUniqueSize(patternColumnsList);

					return bRes;
				}
			}
			catch { }

			return false;
		}

		//=============================================================================
		/// <summary>
		/// Changes size of current column and other columns in the pattern.
		/// </summary>
		/// <param name="bChangeLength">If true, then column's length will be changed and newLength param should contains new length value</param>
		/// <param name="bChangeDepth">If true, then column's depth will be changed and newDepth param should contains new depth value</param>
		/// <param name="bChangeAllColumnsInThePattern">True - changes other columns in the pattern, False - changes only this column</param>
		/// <param name="bTryToFixIncorrectValues">If length or depth value is not correct, tries to fix it and apply step, min, max values</param>
		/// <param name="bRecalcColumnUniqueIndex">Call DrawingDocument.RecalcColumnUniqueSize() method for changed columns</param>
		/// <param name="bCheckLayout">If true then checks result layout for geometry overlap</param>
		/// <returns></returns>
		public bool ChangeSize(
			double DrawingLength,
			double DrawingWidth,
			bool bChangeLength,
			double newLength,
			bool bChangeDepth,
			double newDepth,
			//
			bool bChangeAllColumnsInThePattern,
			bool bTryToFixIncorrectValues,
			bool bRecalcColumnUniqueIndex,
			bool bCheckLayout)
		{
			if (bChangeLength && Utils.FLE(newLength, 0.0))
				return false;
			if (bChangeDepth && Utils.FLE(newDepth, 0.0))
				return false;

			ColumnPattern pattern = Pattern;
			if (pattern == null)
				return false;
			//
			Point patternStartPoint;
			int columnsCount;
			int rowsCount;
			List<Column> patternColumnsList;
			List<List<Column>> patternRows;
			if (!pattern.GetPatternInfo(out patternStartPoint, out columnsCount, out rowsCount, out patternColumnsList, out patternRows))
				return false;

			if (patternColumnsList == null)
				return false;

			double lengthValue = newLength;
			double depthValue = newDepth;

			// check borders
			foreach (Column c in patternColumnsList)
			{
				if (!bChangeAllColumnsInThePattern)
				{
					// change only this column
					if (c != this)
						continue;
				}

				if (bChangeLength)
				{
					if (c.IsHorizontal)
					{
						if (Utils.FLT(c.Center_GlobalPoint.X - lengthValue / 2, 0.0))
						{
							// just dont correct value is it was setted via properties
							if (!bTryToFixIncorrectValues)
								return false;
							lengthValue = 2 * c.Center_GlobalPoint.X;
						}
						if (Utils.FGT(c.Center_GlobalPoint.X + lengthValue / 2, DrawingLength))
						{
							// just dont correct value is it was setted via properties
							if (!bTryToFixIncorrectValues)
								return false;
							lengthValue = 2 * (DrawingLength - c.Center_GlobalPoint.X);
						}
					}
					else
					{
						if (Utils.FLT(c.Center_GlobalPoint.Y - lengthValue / 2, 0.0))
						{
							// just dont correct value is it was setted via properties
							if (!bTryToFixIncorrectValues)
								return false;
							lengthValue = 2 * c.Center_GlobalPoint.Y;
						}
						if (Utils.FGT(c.Center_GlobalPoint.Y + lengthValue / 2, DrawingWidth))
						{
							// just dont correct value is it was setted via properties
							if (!bTryToFixIncorrectValues)
								return false;
							lengthValue = 2 * (DrawingWidth - c.Center_GlobalPoint.Y);
						}
					}
				}
				
				if(bChangeDepth)
				{
					if (c.IsHorizontal)
					{
						if (Utils.FLT(c.Center_GlobalPoint.Y - depthValue / 2, 0.0))
						{
							// just dont correct value is it was setted via properties
							if (!bTryToFixIncorrectValues)
								return false;
							depthValue = 2 * c.Center_GlobalPoint.Y;
						}
						if (Utils.FGT(c.Center_GlobalPoint.Y + depthValue / 2, DrawingWidth))
						{
							// just dont correct value is it was setted via properties
							if (!bTryToFixIncorrectValues)
								return false;
							depthValue = 2 * (DrawingWidth - c.Center_GlobalPoint.Y);
						}
					}
					else
					{
						if (Utils.FLT(c.Center_GlobalPoint.X - depthValue / 2, 0.0))
						{
							// just dont correct value is it was setted via properties
							if (!bTryToFixIncorrectValues)
								return false;
							depthValue = 2 * c.Center_GlobalPoint.X;
						}
						if (Utils.FGT(c.Center_GlobalPoint.X + depthValue / 2, DrawingLength))
						{
							// just dont correct value is it was setted via properties
							if (!bTryToFixIncorrectValues)
								return false;
							depthValue = 2 * (DrawingLength - c.Center_GlobalPoint.X);
						}
					}
				}
			}

			double minLengthValue = 0.0;
			double maxLengthValue = double.PositiveInfinity;
			double lengthStepValue = 1;
			if (bChangeLength)
			{
				if (this.IsHorizontal)
				{
					minLengthValue = MinLength_X;
					maxLengthValue = MaxLength_X;
					if (double.IsPositiveInfinity(maxLengthValue))
						maxLengthValue = DrawingLength;
					lengthStepValue = StepLength_X;
				}
				else
				{
					minLengthValue = MinLength_Y;
					maxLengthValue = MaxLength_Y;
					if (double.IsPositiveInfinity(maxLengthValue))
						maxLengthValue = DrawingWidth;
					lengthStepValue = StepLength_Y;
				}
			}

			double minDepthValue = 0.0;
			double maxDepthValue = double.PositiveInfinity;
			double depthStepValue = 1;
			if (bChangeDepth)
			{
				if (this.IsHorizontal)
				{
					minDepthValue = MinLength_Y;
					maxDepthValue = MaxLength_Y;
					if (double.IsPositiveInfinity(maxDepthValue))
						maxDepthValue = DrawingWidth;
					depthStepValue = StepLength_Y;
				}
				else
				{
					minDepthValue = MinLength_X;
					maxDepthValue = MaxLength_X;
					if (double.IsPositiveInfinity(maxDepthValue))
						maxDepthValue = DrawingLength;
					depthStepValue = StepLength_X;
				}
			}
			// dont correct value if it was setted via properties
			if (!bTryToFixIncorrectValues)
			{
				if (bChangeLength)
				{
					if (Utils.FLT(newLength, minLengthValue))
						return false;
					if (double.IsPositiveInfinity(maxLengthValue))
						return false;
					else if (Utils.FGT(newLength, maxLengthValue))
						return false;
				}

				if(bChangeDepth)
				{
					if (Utils.FLT(newDepth, minDepthValue))
						return false;
					if (double.IsPositiveInfinity(maxDepthValue))
						return false;
					else if (Utils.FGT(newDepth, maxDepthValue))
						return false;
				}
			}
			// Columns in the pattern cant overlap each other.
			// Check hotizontal and vertical distance between columns in the pattern
			//
			// Check horizontal distance
			foreach (List<Column> row in patternRows)
			{
				if (row == null)
					continue;

				for (int i = 0; i < row.Count - 1; ++i)
				{
					Column currColumn = row[i];
					Column nextColumn = row[i + 1];

					if (currColumn == null || nextColumn == null)
						continue;

					// If check only this column then check only neighbors.
					if (!bChangeAllColumnsInThePattern && currColumn != this && nextColumn != this)
						continue;

					double currColumnHalfLengthX = currColumn.Length_X / 2;
					double nextColumnHalfLengthX = nextColumn.Length_X / 2;

					bool bChangeCurrColumn = (bChangeAllColumnsInThePattern || currColumn == this) && ((bChangeLength && currColumn.IsHorizontal) || (bChangeDepth && !currColumn.IsHorizontal));
					bool bChangeNextColumn = (bChangeAllColumnsInThePattern || nextColumn == this) && ((bChangeLength && nextColumn.IsHorizontal) || (bChangeDepth && !nextColumn.IsHorizontal));

					if (bChangeCurrColumn)
					{
						if(currColumn.IsHorizontal && bChangeLength)
							currColumnHalfLengthX = newLength / 2;
						else if(!currColumn.IsHorizontal && bChangeDepth)
							currColumnHalfLengthX = newDepth / 2;
					}
					if (bChangeNextColumn)
					{
						if(nextColumn.IsHorizontal && bChangeLength)
							nextColumnHalfLengthX = newLength / 2;
						else if(!nextColumn.IsHorizontal && bChangeDepth)
							nextColumnHalfLengthX = newDepth / 2;
					}

					double availableDistance = (nextColumn.Center_GlobalPoint - currColumn.Center_GlobalPoint).Length;
					double minDistanceBetweenCenterPnt = currColumnHalfLengthX + nextColumnHalfLengthX;
					if (Utils.FGT(minDistanceBetweenCenterPnt, availableDistance))
					{
						if(bChangeLength && bChangeDepth)
						{
							newLength = availableDistance;
							newDepth = availableDistance;
						}
						else if(bChangeLength)
						{
							if (bChangeCurrColumn && bChangeNextColumn)
								newLength = availableDistance;
							else if (bChangeCurrColumn)
								newLength = 2 * (availableDistance - nextColumnHalfLengthX);
							else if (bChangeNextColumn)
								newLength = 2 * (availableDistance - currColumnHalfLengthX);
						}
						else if(bChangeDepth)
						{
							if (bChangeCurrColumn && bChangeNextColumn)
								newDepth = availableDistance;
							else if (bChangeCurrColumn)
								newDepth = 2 * (availableDistance - nextColumnHalfLengthX);
							else if (bChangeNextColumn)
								newDepth = 2 * (availableDistance - currColumnHalfLengthX);
						}
					}
				}
			}
			// Check vertical distance
			for (int iColumnIndex = 0; iColumnIndex < columnsCount; ++iColumnIndex)
			{
				for (int iRowIndex = 0; iRowIndex < rowsCount - 1; ++iRowIndex)
				{
					Column currColumn = patternRows[iRowIndex][iColumnIndex];
					Column nextColumn = patternRows[iRowIndex + 1][iColumnIndex];

					if (currColumn == null || nextColumn == null)
						continue;

					// If check only this column then check only neighbors.
					if (!bChangeAllColumnsInThePattern && currColumn != this && nextColumn != this)
						continue;


					double currColumnHalfLengthY = currColumn.Length_Y / 2;
					double nextColumnHalfLengthY = nextColumn.Length_Y / 2;

					bool bChangeCurrColumn = (bChangeAllColumnsInThePattern || currColumn == this) && ((bChangeLength && !currColumn.IsHorizontal) || (bChangeDepth && currColumn.IsHorizontal));
					bool bChangeNextColumn = (bChangeAllColumnsInThePattern || nextColumn == this) && ((bChangeLength && !nextColumn.IsHorizontal) || (!bChangeDepth && nextColumn.IsHorizontal));

					if (bChangeCurrColumn)
					{
						if (currColumn.IsHorizontal && bChangeDepth)
							currColumnHalfLengthY = newDepth / 2;
						else if (!currColumn.IsHorizontal && bChangeLength)
							currColumnHalfLengthY = newLength / 2;
					}
					if (bChangeNextColumn)
					{
						if (nextColumn.IsHorizontal && bChangeDepth)
							nextColumnHalfLengthY = newDepth / 2;
						else if (!nextColumn.IsHorizontal && bChangeLength)
							nextColumnHalfLengthY = newLength / 2;
					}

					double availableDistance = (nextColumn.Center_GlobalPoint - currColumn.Center_GlobalPoint).Length;
					double minDistanceBetweenCenterPnt = currColumnHalfLengthY + nextColumnHalfLengthY;
					if (Utils.FGT(minDistanceBetweenCenterPnt, availableDistance))
					{
						if (bChangeLength && bChangeDepth)
						{
							newLength = availableDistance;
							newDepth = availableDistance;
						}
						else if (bChangeLength)
						{
							if (bChangeCurrColumn && bChangeNextColumn)
								newLength = availableDistance;
							else if (bChangeCurrColumn)
								newLength = 2 * (availableDistance - nextColumnHalfLengthY);
							else if (bChangeNextColumn)
								newLength = 2 * (availableDistance - currColumnHalfLengthY);
						}
						else if (bChangeDepth)
						{
							if (bChangeCurrColumn && bChangeNextColumn)
								newDepth = availableDistance;
							else if (bChangeCurrColumn)
								newDepth = 2 * (availableDistance - nextColumnHalfLengthY);
							else if (bChangeNextColumn)
								newDepth = 2 * (availableDistance - currColumnHalfLengthY);
						}
					}
				}
			}
			//
			if (bChangeLength)
			{
				newLength = Utils.GetWholeNumberByStep(newLength, lengthStepValue);
				newLength = Utils.CheckWholeNumber(newLength, minLengthValue, maxLengthValue);
			}
			if (bChangeDepth)
			{
				newDepth = Utils.GetWholeNumberByStep(newDepth, depthStepValue);
				newDepth = Utils.CheckWholeNumber(newDepth, minDepthValue, maxDepthValue);
			}

			// make preview pattern
			List<BaseRectangleGeometry> previewColumnsPattern = new List<BaseRectangleGeometry>();
			foreach (Column c in patternColumnsList)
			{
				Column previewColumn = c.Clone() as Column;
				if (previewColumn == null)
					return false;
				previewColumn.Sheet = c.Sheet;
				//
				Point previewTopLeftPoint = c.TopLeft_GlobalPoint;
				if (bChangeAllColumnsInThePattern || (!bChangeAllColumnsInThePattern && c == this))
				{
					if (bChangeLength)
						previewColumn.Length = newLength;
					if (bChangeDepth)
						previewColumn.Depth = newDepth;

					previewTopLeftPoint = c.Center_GlobalPoint;
					previewTopLeftPoint.X -= previewColumn.Length_X / 2;
					previewTopLeftPoint.Y -= previewColumn.Length_Y / 2;
				}
				previewTopLeftPoint = Utils.GetWholePoint(previewTopLeftPoint);
				previewColumn.TopLeft_GlobalPoint = previewTopLeftPoint;

				previewColumnsPattern.Add(previewColumn);
			}

			//
			List<BaseRectangleGeometry> rectanglesToIgnore = new List<BaseRectangleGeometry>();
			rectanglesToIgnore.AddRange(patternColumnsList);
			// check layout
			List<BaseRectangleGeometry> overlappedRectangles;
			if (!IsInit || !bCheckLayout || m_Sheet.IsLayoutCorrect(previewColumnsPattern, rectanglesToIgnore, out overlappedRectangles))
			{
				List<Column> changedColumns = new List<Column>();

				// layout is correct, make changes
				foreach (Column c in patternColumnsList)
				{
					if (!bTryToFixIncorrectValues && !bChangeAllColumnsInThePattern)
					{
						// change only this column
						if (c != this)
							continue;
					}

					//
					Point newTopLeftPoint = c.TopLeft_GlobalPoint;
					if (bChangeAllColumnsInThePattern || (!bChangeAllColumnsInThePattern && c == this))
					{
						newTopLeftPoint = c.Center_GlobalPoint;
						if (bChangeLength)
							c.Length = newLength;
						if (bChangeDepth)
							c.Depth = newDepth;
						newTopLeftPoint.X -= c.Length_X / 2;
						newTopLeftPoint.Y -= c.Length_Y / 2;
					}
					newTopLeftPoint = Utils.GetWholePoint(newTopLeftPoint);
					c.TopLeft_GlobalPoint = newTopLeftPoint;

					//
					changedColumns.Add(c);
				}

				if (bRecalcColumnUniqueIndex)
					m_Sheet.Document.RecalcColumnUniqueSize(changedColumns);

				return true;
			}

			return false;
		}

		//=============================================================================
		public static string _sGetKey(Column c)
		{
			string strKey = string.Empty;

			if(c != null)
			{
				strKey = c.Length.ToString(".");
				strKey += "_";
				strKey += c.Depth.ToString(".");
			}

			return strKey;
		}

		#endregion

		#region Serialization

		//=============================================================================
		//
		// 1.0
		//
		protected static string _sColumn_strMajor = "Column_MAJOR";
		protected static int _sColumn_MAJOR = 1;
		protected static string _sColumn_strMinor = "Column_MINOR";
		protected static int _sColumn_MINOR = 0;
		//=============================================================================
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			base.GetObjectData(info, context);

			//
			info.AddValue(_sColumn_strMajor, _sColumn_MAJOR);
			info.AddValue(_sColumn_strMinor, _sColumn_MINOR);

			//
			info.AddValue("SizeIndex", SizeIndex);

			//
			info.AddValue("CanContinuePattern", CanContinuePattern);
			info.AddValue("ColumnPattern_ID", ColumnPattern_ID);
		}
		//=============================================================================
		public Column(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sColumn_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sColumn_strMinor, typeof(int));
			if (iMajor > _sColumn_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sColumn_MAJOR && iMinor > _sColumn_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sColumn_MAJOR)
			{
				try
				{
					//
					SizeIndex = (int)info.GetValue("SizeIndex", typeof(int));

					//
					CanContinuePattern = (bool)info.GetValue("CanContinuePattern", typeof(bool));
					ColumnPattern_ID = (int)info.GetValue("ColumnPattern_ID", typeof(int));
				}
				catch
				{
					++DrawingDocument._sStreamReadException;
				}
			}
			else
				++DrawingDocument._sBiggerMajorNumber;
		}

		#endregion
	}
}
