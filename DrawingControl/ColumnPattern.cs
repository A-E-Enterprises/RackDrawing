using AppInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DrawingControl
{
	/// <summary>
	/// Column pattern.
	/// It is used for group columns in the pattern.
	/// If columns are in the pattern, then they have the same X\Y distance between their center points.
	/// If user moves any column, then all other pattern columns will be moved too.
	/// And column in the pattern have equal Length\Depth.
	/// </summary>
	[Serializable]
	public class ColumnPattern : ISerializable, IClonable
	{
		public static int sMaxPatternID = 99999;

		public ColumnPattern(DrawingSheet ds)
		{
			m_Sheet = ds;

			ID = -1;
			GlobalOffset_X = 5000;
			GlobalOffset_Y = 5000;
		}
		public ColumnPattern(ColumnPattern columnPattern)
		{
			if(columnPattern != null)
			{
				this.ID = columnPattern.ID;
				this.GlobalOffset_X = columnPattern.GlobalOffset_X;
				this.GlobalOffset_Y = columnPattern.GlobalOffset_Y;
			}
		}

		#region Properties

		//=============================================================================
		private DrawingSheet m_Sheet = null;
		public DrawingSheet Sheet
		{
			get { return m_Sheet; }
			set { m_Sheet = value; }
		}

		//=============================================================================
		/// <summary>
		/// ID of the pattern.
		/// It should be unique for all patterns in the sheet.
		/// </summary>
		public int ID { get; set; }

		//=============================================================================
		// distance between the center of columns in pattern in X direction
		public double GlobalOffset_X { get; set; }

		//=============================================================================
		// distance between the center of columns in pattern in Y direction
		public double GlobalOffset_Y { get; set; }

		#endregion

		#region Public functions

		//=============================================================================
		public bool GetPatternInfo(out Point patternStartPoint, out int columnsCount, out int rowsCount)
		{
			List<Column> patternColumnsList;
			List<List<Column>> patternRows;
			return GetPatternInfo(out patternStartPoint, out columnsCount, out rowsCount, out patternColumnsList, out patternRows);
		}
		/// <summary>
		/// Returns pattern parameters.
		/// </summary>
		/// <param name="patternStartPoint">Initial pattern point, it is point of the center of top left pattern column. From this point pattern starts.</param>
		/// <param name="columnsCount">Count of columns(not geometry columns) in the pattern</param>
		/// <param name="rowsCount">Count of rows</param>
		/// <param name="patternColumnsList">List with all columns in this pattern</param>
		/// <param name="patternRows">All pattern columns grouped by rows.</param>
		public bool GetPatternInfo(out Point patternStartPoint, out int columnsCount, out int rowsCount, out List<Column> patternColumnsList, out List<List<Column>> patternRows)
		{
			patternStartPoint = new Point(0.0, 0.0);
			columnsCount = 0;
			rowsCount = 0;
			patternRows = new List<List<Column>>();
			patternColumnsList = GetColumns();
			if (patternColumnsList.Count < 1)
				return false;

			// Calculate pattern size.
			// Get all column center point and calculate pattern bound from them.
			double left_X = -1;
			double right_X = -1;
			double top_Y = -1;
			double bot_Y = -1;
			foreach (Column c in patternColumnsList)
			{
				//
				if (left_X < 0)
					left_X = c.Center_GlobalPoint.X;
				else if (c.Center_GlobalPoint.X < left_X)
					left_X = c.Center_GlobalPoint.X;

				//
				if (right_X < 0)
					right_X = c.Center_GlobalPoint.X;
				else if (c.Center_GlobalPoint.X > right_X)
					right_X = c.Center_GlobalPoint.X;

				//
				if (bot_Y < 0)
					bot_Y = c.Center_GlobalPoint.Y;
				else if (c.Center_GlobalPoint.Y > bot_Y)
					bot_Y = c.Center_GlobalPoint.Y;

				//
				if (top_Y < 0)
					top_Y = c.Center_GlobalPoint.Y;
				else if (c.Center_GlobalPoint.Y < top_Y)
					top_Y = c.Center_GlobalPoint.Y;
			}

			patternStartPoint.X = left_X;
			patternStartPoint.Y = top_Y;
			columnsCount = Utils.GetWholeNumber((right_X - left_X) / GlobalOffset_X) + 1;
			rowsCount = Utils.GetWholeNumber((bot_Y - top_Y) / GlobalOffset_Y) + 1;

			// init
			for (int i = 0; i < rowsCount; ++i)
			{
				List<Column> newRow = new List<Column>();
				//
				for (int j = 0; j < columnsCount; ++j)
					newRow.Add(null);

				patternRows.Add(newRow);
			}

			// place columns in rows
			foreach (Column c in patternColumnsList)
			{
				// calculate index zero-based
				int iRowIndex = Utils.GetWholeNumber((c.Center_GlobalPoint.Y - top_Y) / GlobalOffset_Y);
				int iColumnIndex = Utils.GetWholeNumber((c.Center_GlobalPoint.X - left_X) / GlobalOffset_X);
				//
				if (iRowIndex >= patternRows.Count)
					continue;

				List<Column> row = patternRows[iRowIndex];
				if (row == null)
					continue;
				if (iColumnIndex >= row.Count)
					continue;

				//
				row[iColumnIndex] = c;
			}

			return true;
		}

		//=============================================================================
		/// <summary>
		/// Updates CanContinuePattern property in columns in the pattern.
		/// </summary>
		public void Update()
		{
			Point patternStartPoint;
			int columnsCount;
			int rowsCount;
			List<Column> patternColumnsList;
			List<List<Column>> patternRows;
			if (!GetPatternInfo(out patternStartPoint, out columnsCount, out rowsCount, out patternColumnsList, out patternRows))
				return;

			Column continuePatternColumn = null;
			// set can continue pattern flag
			// only the bot right column can continue pattern
			foreach (Column c in patternColumnsList)
			{
				c.CanContinuePattern = false;

				//
				if (continuePatternColumn == null)
					continuePatternColumn = c;
				else
				{
					// it "c" at the bot
					if (c.Center_GlobalPoint.Y > continuePatternColumn.Center_GlobalPoint.Y)
						continuePatternColumn = c;
					// or "c" has the same Y-coordinate but placed to the right
					else if (c.Center_GlobalPoint.Y == continuePatternColumn.Center_GlobalPoint.Y && c.Center_GlobalPoint.X > continuePatternColumn.Center_GlobalPoint.X)
						continuePatternColumn = c;
				}
			}

			//
			if (continuePatternColumn != null)
				continuePatternColumn.CanContinuePattern = true;
		}

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new ColumnPattern(this);
		}

		#endregion

		#region Private functions

		//=============================================================================
		/// <summary>
		/// Returns list of columns included in this pattern.
		/// </summary>
		/// <returns></returns>
		private List<Column> GetColumns()
		{
			List<Column> result = new List<Column>();

			if (m_Sheet == null)
				return result;

			List<Column> allColumns = m_Sheet.GetAllColumns();
			foreach (Column c in allColumns)
			{
				if (c == null)
					continue;

				if (c.Pattern == this)
					result.Add(c);
			}

			return result;
		}

		#endregion

		#region Serialization

		//=============================================================================
		//
		// 1.0
		//
		protected static string _sColumnPattern_strMajor = "ColumnPattern_MAJOR";
		protected static int _sColumnPattern_MAJOR = 1;
		protected static string _sColumnPattern_strMinor = "ColumnPattern_MINOR";
		protected static int _sColumnPattern_MINOR = 0;
		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(_sColumnPattern_strMajor, _sColumnPattern_MAJOR);
			info.AddValue(_sColumnPattern_strMinor, _sColumnPattern_MINOR);

			info.AddValue("ID", ID);
			info.AddValue("GlobalOffset_X", GlobalOffset_X);
			info.AddValue("GlobalOffset_Y", GlobalOffset_Y);
		}
		//=============================================================================
		public ColumnPattern(SerializationInfo info, StreamingContext context)
		{
			// There are some drawings in which ColumnPattern didnt write major and minor to stream.
			// So wrap it in try-catch.
			int iMajor = 1;
			int iMinor = 0;
			try
			{
				//
				iMajor = (int)info.GetValue(_sColumnPattern_strMajor, typeof(int));
				iMinor = (int)info.GetValue(_sColumnPattern_strMinor, typeof(int));
			}
			catch { }
			//
			if (iMajor > _sColumnPattern_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sColumnPattern_MAJOR && iMinor > _sColumnPattern_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sColumnPattern_MAJOR)
			{
				try
				{
					ID = (int)info.GetValue("ID", typeof(int));
					GlobalOffset_X = (double)info.GetValue("GlobalOffset_X", typeof(double));
					GlobalOffset_Y = (double)info.GetValue("GlobalOffset_Y", typeof(double));
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

		//=============================================================================
		/// <summary>
		/// Try to set new ColumnPattern.GlobalOffset_X.
		/// </summary>
		public static bool ChangeOffsetX(Column fixedColumn, double newGlobalOffset_X, double DrawingLength, double DrawingWidth, bool bChangedViaProperties)
		{
			if (fixedColumn == null)
				return false;

			DrawingSheet sheet = fixedColumn.Sheet;
			if (sheet == null)
				return false;

			//
			newGlobalOffset_X = Utils.GetWholeNumber(newGlobalOffset_X);
			if (newGlobalOffset_X <= 0)
				return false;

			//
			DrawingControl drawing = null;
			if (fixedColumn.Wrapper != null)
				drawing = fixedColumn.Wrapper.Owner;

			//
			if (drawing == null)
				return false;

			//
			ColumnPattern pattern = fixedColumn.Pattern;
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
			if (columnsCount <= 0
				|| rowsCount <= 0
				|| patternColumnsList.Count < 1
				|| patternRows.Count < 1)
				return false;

			// columns in the pattern cannot overlap each other
			double rMaxLength = 0;
			foreach (List<Column> row in patternRows)
			{
				if (row == null)
					continue;
			
				if(row.Count == 1 && row[0] != null)
				{
					if (Utils.FGT(row[0].Length_X, rMaxLength))
						rMaxLength = row[0].Length_X;
					continue;
				}
			
				for(int i=0; i<row.Count-1; ++i)
				{
					Column currColumn = row[i];
					Column nextColumn = row[i + 1];
			
					if (currColumn == null || nextColumn == null)
						continue;

					double maxDist = currColumn.Length_X / 2 + nextColumn.Length_X / 2;
					if (Utils.FGT(maxDist, rMaxLength))
						rMaxLength = maxDist;
				}
			}
			//
			if (Utils.FLE(rMaxLength, 0.0))
				return false;
			//
			if (Utils.FLT(newGlobalOffset_X, rMaxLength))
			{
				if (bChangedViaProperties)
					return false;

				newGlobalOffset_X = rMaxLength;
			}

			// calculate index zero based
			int iCurRowIndex = Utils.GetWholeNumber((fixedColumn.Center_GlobalPoint.Y - patternStartPoint.Y) / pattern.GlobalOffset_Y);
			int iCurColumnIndex = Utils.GetWholeNumber((fixedColumn.Center_GlobalPoint.X - patternStartPoint.X) / pattern.GlobalOffset_X);

			//
			if (iCurRowIndex >= patternRows.Count)
				return false;

			// make preview column pattern and check layout
			List<List<Column>> previewRows = new List<List<Column>>();
			foreach (List<Column> row in patternRows)
			{
				List<Column> previewRow = new List<Column>();

				foreach (Column c in row)
				{
					Column previewColumn = null;
					if (c != null)
						previewColumn = c.Clone() as Column;

					if (previewColumn != null)
					{
						previewColumn.Sheet = sheet;
						previewRow.Add(previewColumn);
					}
				}

				previewRows.Add(previewRow);
			}

			// change offset
			List<BaseRectangleGeometry> _RectanglesToCheck = new List<BaseRectangleGeometry>();
			foreach (List<Column> previewRow in previewRows)
			{
				Point curCenterPoint = patternStartPoint;
				curCenterPoint.X += iCurColumnIndex * pattern.GlobalOffset_X;
				curCenterPoint.Y += previewRows.IndexOf(previewRow) * pattern.GlobalOffset_Y;

				// left side of the row
				for (int iColumn = iCurColumnIndex - 1; iColumn >= 0; --iColumn)
				{
					Column _previewColumn = previewRow[iColumn];
					if (_previewColumn != null)
					{
						Point newTopLeft_GlobalPoint = curCenterPoint;
						newTopLeft_GlobalPoint.X -= newGlobalOffset_X * Math.Abs(iCurColumnIndex - iColumn);
						newTopLeft_GlobalPoint.X -= _previewColumn.Length_X / 2;
						newTopLeft_GlobalPoint.Y -= _previewColumn.Length_Y / 2;
						//
						newTopLeft_GlobalPoint = Utils.GetWholePoint(newTopLeft_GlobalPoint);
						//
						_previewColumn.TopLeft_GlobalPoint = newTopLeft_GlobalPoint;

						_RectanglesToCheck.Add(_previewColumn);
					}
				}

				// right side of the row
				for (int iColumn = iCurColumnIndex + 1; iColumn < previewRow.Count; ++iColumn)
				{
					Column _previewColumn = previewRow[iColumn];
					if (_previewColumn != null)
					{
						Point newTopLeft_GlobalPoint = curCenterPoint;
						newTopLeft_GlobalPoint.X += newGlobalOffset_X * Math.Abs(iCurColumnIndex - iColumn);
						newTopLeft_GlobalPoint.X -= _previewColumn.Length_X / 2;
						newTopLeft_GlobalPoint.Y -= _previewColumn.Length_Y / 2;
						//
						newTopLeft_GlobalPoint = Utils.GetWholePoint(newTopLeft_GlobalPoint);
						//
						_previewColumn.TopLeft_GlobalPoint = newTopLeft_GlobalPoint;

						_RectanglesToCheck.Add(_previewColumn);
					}
				}
			}

			// check borders
			foreach (List<Column> previewRow in previewRows)
			{
				if (previewRow.Count == 0)
					continue;

				//
				Column _first = previewRow[0];
				Column _last = previewRow[previewRow.Count - 1];

				//
				if (_first == null || _last == null)
					continue;

				if (_first.TopLeft_GlobalPoint.X < 0)
					return false;
				if (_first.TopLeft_GlobalPoint.Y < 0)
					return false;

				if (_last.BottomRight_GlobalPoint.X > DrawingLength)
					return false;
				if (_last.BottomRight_GlobalPoint.Y > DrawingWidth)
					return false;
			}

			//
			List<BaseRectangleGeometry> _columnsToignore = new List<BaseRectangleGeometry>();
			_columnsToignore.AddRange(patternColumnsList);
			foreach (List<Column> _previewRow in previewRows)
			{
				_columnsToignore.AddRange(_previewRow);
			}
			// check layout
			List<BaseRectangleGeometry> overlappedRectangles;
			if (sheet.IsLayoutCorrect(_RectanglesToCheck, _columnsToignore, out overlappedRectangles))
			{
				Point fixedPoint = patternStartPoint;
				fixedPoint.X += iCurColumnIndex * pattern.GlobalOffset_X;
				//
				pattern.GlobalOffset_X = newGlobalOffset_X;

				// apply changes
				foreach (List<Column> _row in patternRows)
				{
					Point curCenterPoint = fixedPoint;
					curCenterPoint.Y += patternRows.IndexOf(_row) * pattern.GlobalOffset_Y;

					// left side of the row
					for (int iColumn = iCurColumnIndex - 1; iColumn >= 0; --iColumn)
					{
						Column _columnToChange = _row[iColumn];
						if (_columnToChange != null)
						{
							Point newTopLeft_GlobalPoint = curCenterPoint;
							newTopLeft_GlobalPoint.X -= newGlobalOffset_X * Math.Abs(iCurColumnIndex - iColumn);
							newTopLeft_GlobalPoint.X -= _columnToChange.Length_X / 2;
							newTopLeft_GlobalPoint.Y -= _columnToChange.Length_Y / 2;
							//
							newTopLeft_GlobalPoint = Utils.GetWholePoint(newTopLeft_GlobalPoint);
							//
							_columnToChange.TopLeft_GlobalPoint = newTopLeft_GlobalPoint;
						}
					}

					// right side of the row
					for (int iColumn = iCurColumnIndex + 1; iColumn < _row.Count; ++iColumn)
					{
						Column _columnToChange = _row[iColumn];
						if (_columnToChange != null)
						{
							Point newTopLeft_GlobalPoint = curCenterPoint;
							newTopLeft_GlobalPoint.X += newGlobalOffset_X * Math.Abs(iCurColumnIndex - iColumn);
							newTopLeft_GlobalPoint.X -= _columnToChange.Length_X / 2;
							newTopLeft_GlobalPoint.Y -= _columnToChange.Length_Y / 2;
							//
							newTopLeft_GlobalPoint = Utils.GetWholePoint(newTopLeft_GlobalPoint);
							//
							_columnToChange.TopLeft_GlobalPoint = newTopLeft_GlobalPoint;
						}
					}
				}

				return true;
			}

			return false;
		}

		//=============================================================================
		public static bool ChangeOffsetY(Column fixedColumn, double newGlobalOffset_Y, double DrawingLength, double DrawingWidth, bool bChangedViaProperties)
		{
			if (fixedColumn == null)
				return false;

			DrawingSheet _sheet = fixedColumn.Sheet;
			if (_sheet == null)
				return false;

			//
			newGlobalOffset_Y = Utils.GetWholeNumber(newGlobalOffset_Y);
			if (newGlobalOffset_Y <= 0)
				return false;

			//
			DrawingControl drawing = null;
			if (fixedColumn.Wrapper != null)
				drawing = fixedColumn.Wrapper.Owner;

			//
			if (drawing == null)
				return false;

			//
			ColumnPattern pattern = fixedColumn.Pattern;
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
			if (columnsCount <= 0
				|| rowsCount <= 0
				|| patternColumnsList.Count < 1
				|| patternRows.Count < 1)
				return false;

			// columns in the pattern cannot overlap each other
			double rMaxWidth = 0;
			for (int iColumnIndex=0; iColumnIndex<columnsCount; ++iColumnIndex)
			{
				for(int iRowIndex=0; iRowIndex < rowsCount-1; ++ iRowIndex)
				{
					Column currColumn = patternRows[iRowIndex][iColumnIndex];
					Column nextColumn = patternRows[iRowIndex + 1][iColumnIndex];

					if (currColumn == null || nextColumn == null)
						continue;

					double maxDist = currColumn.Length_Y / 2 + nextColumn.Length_Y / 2;
					if (Utils.FGT(maxDist, rMaxWidth))
						rMaxWidth = maxDist;
				}
			}
			if (Utils.FLE(rMaxWidth, 0.0))
				rMaxWidth = patternColumnsList[0].Length_Y;
			//
			if (Utils.FLE(rMaxWidth, 0.0))
				return false;
			//
			if (Utils.FLT(newGlobalOffset_Y, rMaxWidth))
			{
				if (bChangedViaProperties)
					return false;

				newGlobalOffset_Y = rMaxWidth;
			}

			// calculate index zero based
			int iCurRowIndex = Utils.GetWholeNumber((fixedColumn.Center_GlobalPoint.Y - patternStartPoint.Y) / pattern.GlobalOffset_Y);
			int iCurColumnIndex = Utils.GetWholeNumber((fixedColumn.Center_GlobalPoint.X - patternStartPoint.X) / pattern.GlobalOffset_X);

			//
			if (iCurRowIndex >= patternRows.Count)
				return false;

			// make preview column pattern and check layout
			List<List<Column>> _previewRows = new List<List<Column>>();
			foreach (List<Column> _row in patternRows)
			{
				List<Column> _previewRow = new List<Column>();

				foreach (Column c in _row)
				{
					Column _previewColumn = null;
					if (c != null)
						_previewColumn = c.Clone() as Column;;

					if (_previewColumn != null)
					{
						_previewColumn.Sheet = _sheet;
						_previewRow.Add(_previewColumn);
					}
				}

				_previewRows.Add(_previewRow);
			}

			//
			// change offset
			List<BaseRectangleGeometry> _RectanglesToCheck = new List<BaseRectangleGeometry>();
			foreach (List<Column> _previewRow in _previewRows)
			{
				int iPreviewRowIndex = _previewRows.IndexOf(_previewRow);
				int iSign = 1;
				if (iPreviewRowIndex < iCurRowIndex)
					iSign = -1;

				foreach (Column _previewColumn in _previewRow)
				{
					if (_previewColumn != null)
					{
						Point newTopLeftPoint = _previewColumn.Center_GlobalPoint;
						newTopLeftPoint.X -= _previewColumn.Length_X / 2;
						newTopLeftPoint.Y = fixedColumn.Center_GlobalPoint.Y;
						newTopLeftPoint.Y += iSign * Math.Abs(iCurRowIndex - iPreviewRowIndex) * newGlobalOffset_Y;
						newTopLeftPoint.Y -= _previewColumn.Length_Y / 2;
						//
						newTopLeftPoint = Utils.GetWholePoint(newTopLeftPoint);
						//
						_previewColumn.TopLeft_GlobalPoint = newTopLeftPoint;

						_RectanglesToCheck.Add(_previewColumn);
					}
				}
			}

			// check borders
			List<Column> _firstRow = _previewRows[0];
			if (_firstRow == null)
				return false;
			foreach (Column c in _firstRow)
			{
				if (c != null)
				{
					if (c.TopLeft_GlobalPoint.Y < 0)
						return false;
				}
			}
			//
			List<Column> _lastRow = _previewRows[_previewRows.Count - 1];
			if (_lastRow == null)
				return false;
			foreach (Column c in _lastRow)
			{
				if (c != null)
				{
					if (c.BottomRight_GlobalPoint.Y > DrawingWidth)
						return false;
				}
			}

			//
			List<BaseRectangleGeometry> _columnsToignore = new List<BaseRectangleGeometry>();
			_columnsToignore.AddRange(patternColumnsList);
			foreach (List<Column> _previewRow in _previewRows)
			{
				_columnsToignore.AddRange(_previewRow);
			}
			// check layout
			List<BaseRectangleGeometry> overlappedRectangles;
			if (_sheet.IsLayoutCorrect(_RectanglesToCheck, _columnsToignore, out overlappedRectangles))
			{
				pattern.GlobalOffset_Y = newGlobalOffset_Y;

				// apply changes
				foreach (List<Column> _row in patternRows)
				{
					int iRowIndex = patternRows.IndexOf(_row);
					if (iRowIndex == iCurRowIndex)
						continue;
					//
					int iSign = 1;
					if (iRowIndex < iCurRowIndex)
						iSign = -1;

					foreach (Column _columnToChange in _row)
					{
						if (_columnToChange != null)
						{
							Point newTopLeftPoint = _columnToChange.Center_GlobalPoint;
							newTopLeftPoint.X -= _columnToChange.Length_X / 2;
							newTopLeftPoint.Y = fixedColumn.Center_GlobalPoint.Y;
							newTopLeftPoint.Y += iSign * Math.Abs(iCurRowIndex - iRowIndex) * pattern.GlobalOffset_Y;
							newTopLeftPoint.Y -= _columnToChange.Length_Y / 2;
							//
							//
							newTopLeftPoint = Utils.GetWholePoint(newTopLeftPoint);
							//
							_columnToChange.TopLeft_GlobalPoint = newTopLeftPoint;
						}
					}
				}

				return true;
			}

			return false;
		}
	}
}
