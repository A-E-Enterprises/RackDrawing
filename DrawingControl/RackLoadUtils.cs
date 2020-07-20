using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DrawingControl
{
	public enum eColumnBracingType : int
	{
		eUndefined = 1,
		eNormalBracing = 2,
		eXBracing = 3,
		eNormalBracingWithStiffener = 4,
		eXBracingWithStiffener = 5
	};

	/// <summary>
	/// Read data from the LoadChart.xlsx file.
	/// </summary>
	public static class RackLoadUtils
	{
		// If true then need to call ReadData() on RacksColumnsList.get
		// because m_RacksColumnsList is not initialized.
		private static bool m_bReadDataFromExcel = true;

		/// <summary>
		/// Dictionary with frame load data from the LoadChart.xlsx file.
		/// </summary>
		private static List<RackColumn> m_RacksColumnsList = new List<RackColumn>();
		public static List<RackColumn> RacksColumnsList
		{
			get
			{
				if(m_bReadDataFromExcel)
				{
					string strError;
					ReadData(out strError);
					m_bReadDataFromExcel = false;
				}

				return m_RacksColumnsList;
			}
		}
		//
		private static bool _AddRow(string rackColumnName, double maxBeamSpan, double usl, string typeOfBracing, double maxLoad)
		{
			if (string.IsNullOrEmpty(rackColumnName) || Utils.FLE(maxBeamSpan, 0.0) || Utils.FLE(usl, 0.0) || string.IsNullOrEmpty(typeOfBracing) || Utils.FLE(maxLoad, 0.0))
				return false;

			// Lets try to find RackColumn with rackColumnName.
			RackColumn rackColumn = m_RacksColumnsList.FirstOrDefault(c => c != null && c.Name == rackColumnName);
			if (rackColumn == null)
			{
				rackColumn = new RackColumn(rackColumnName);
				m_RacksColumnsList.Add(rackColumn);
			}

			rackColumn.AddRow(maxBeamSpan, usl, typeOfBracing, maxLoad);
			return true;
		}

		// List with beams from BeamWeight sheet of LoadChart.xlsx
		private static List<RackBeam> m_BeamsList = new List<RackBeam>();
		// List with beams which are realy used in RackColumn.
		private static List<RackBeam> m_ReallyUsedBeamsList = new List<RackBeam>();
		public static List<RackBeam> ReallyUsedBeamsList
		{
			get
			{
				if (m_bReadDataFromExcel)
				{
					string strError;
					ReadData(out strError);
					m_bReadDataFromExcel = false;
				}

				return m_ReallyUsedBeamsList;
			}
		}

		//=============================================================================
		/// <summary>
		/// Read data from the LoadChart.xlsx file
		/// </summary>
		public static bool ReadData(out string strError)
		{
			strError = string.Empty;

			try
			{
				string assemblyDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				string strFilePath = System.IO.Path.Combine(assemblyDir, "..\\..\\..\\LoadChart.xlsx");

				Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
				//Microsoft.Office.Interop.Excel.Workbook wb = excel.Workbooks.Open(strFilePath, ReadOnly: true, Password: "PricesPass");
				Microsoft.Office.Interop.Excel.Workbook workbook = excel.Workbooks.Open(strFilePath, ReadOnly: true);

				// read frame bracing
				_ReadFrameSheet(workbook.Sheets[1]);

				// read beams weight
				// read it before "Beams" sheet, because it(Beam sheet) depends on the m_BeamsList
				_ReadBeamWeightSheet(workbook.Sheets[3]);

				// read beams
				_ReadBeamSheet(workbook.Sheets[2]);

				// close excel file
				workbook.Close(true);
				excel.Quit();

				Marshal.ReleaseComObject(workbook);
				Marshal.ReleaseComObject(excel);

				if (m_bReadDataFromExcel)
					m_bReadDataFromExcel = false;

				// sort Beams
				foreach(RackColumn column in m_RacksColumnsList)
				{
					if (column == null)
						continue;

					column.SortBeams();
				}

				return true;
			}
			catch(Exception ex)
			{
				strError = ex.Message;
			}

			return false;
		}

		//=============================================================================
		// Index of "Max beam span, mm" column
		private static int MAX_BEAM_SPAN_COLUMN_INDEX = 1;
		// Index of "USL" column
		private static int USL_COLUMN_INDEX = 2;
		// Index of "Type of bracing" column
		private static int TYPE_OF_BRACING_COLUMN_INDEX = 3;
		// Index of the first row, from which need to read data.
		// Skip the first row, because it contains "Load per Frame" string.
		private static int FRAME_SHEET_START_ROW_INDEX = 2;
		/// <summary>
		/// Read load per frame data from the Frame sheet.
		/// Bracing depends on the frame load and USL(Unsupported Length) distance.
		/// </summary>
		private static void _ReadFrameSheet(Microsoft.Office.Interop.Excel._Worksheet frameSheet)
		{
			try
			{
				m_RacksColumnsList.Clear();

				if (frameSheet == null)
					return;

				// Data structure:
				// column A - max beam span, mm
				// column B - USL
				// column C - type of bracing
				// all next columns contain rack column name

				Microsoft.Office.Interop.Excel.Range xlRange = frameSheet.UsedRange;

				// Find maximum column index - find the first column with empty [2;index + 1] cell
				int iMaxColumnIndex = -1;
				// Sheet column index - rack column name.
				// For example: column 4 - "GXL 90-1.6".
				Dictionary<int, string> columnIndexToRackColumnNameDict = new Dictionary<int, string>();
				for (int i = TYPE_OF_BRACING_COLUMN_INDEX + 1; i <= 100; ++i)
				{
					object objRackColumnName = xlRange[2, i].Value2;
					if (objRackColumnName == null)
						break;
					else
					{
						string strRackColumnName = objRackColumnName.ToString();
						if (!string.IsNullOrEmpty(strRackColumnName))
						{
							columnIndexToRackColumnNameDict[i] = strRackColumnName;
							iMaxColumnIndex = i;
						}
					}
				}
				//
				if (iMaxColumnIndex < 0)
					return;

				// Read data from the rows.
				// If MAX_BEAM_SPAN_COLUMN_INDEX column contains empty row, then go to the next row.
				// If there are 3 empty rows then break data read.
				int iMaxEmptyRowsCellCount = 3;
				int iEmptyRowCellCount = 0;
				//
				double rMaxBeamSpan = -1.0;
				double rUSL = -1.0;
				string strBracing = string.Empty;
				for(int iRow = FRAME_SHEET_START_ROW_INDEX + 1; iRow < 10000; ++iRow)
				{
					//
					if (iEmptyRowCellCount == iMaxEmptyRowsCellCount)
						break;

					for (int iColumn = 1; iColumn <= iMaxColumnIndex; ++iColumn)
					{
						// Read Max beam span.
						if (MAX_BEAM_SPAN_COLUMN_INDEX == iColumn)
						{
							object objMaxBeamSpan = xlRange[iRow, iColumn].Value2;
							if (objMaxBeamSpan == null)
							{
								++iEmptyRowCellCount;
								break;
							}
							else
								iEmptyRowCellCount = 0;

							try
							{
								double rCellValue = Convert.ToDouble(objMaxBeamSpan);
								if (Utils.FLE(rCellValue, 0.0))
									break;
								rMaxBeamSpan = rCellValue;
							}
							catch
							{
								break;
							}
						}
						// Read USL
						else if(USL_COLUMN_INDEX == iColumn)
						{
							object objUSL = xlRange[iRow, iColumn].Value2;
							// USL cell can be null or empty, just skip cell in this case and use previous USL value.
							if (objUSL == null)
								continue;

							try
							{
								double rCellValue = Convert.ToDouble(objUSL);
								if (Utils.FLE(rCellValue, 0.0))
									break;
								rUSL = rCellValue;
							}
							catch
							{
								break;
							}
						}
						// Read Type of Bracing
						else if(TYPE_OF_BRACING_COLUMN_INDEX == iColumn)
						{
							object objTypeOfBracing = xlRange[iRow, iColumn].Value2;
							if (objTypeOfBracing == null)
								break;

							try
							{
								string strValue = Convert.ToString(objTypeOfBracing);
								if (string.IsNullOrEmpty(strValue))
									break;
								strBracing = strValue;
							}
							catch
							{
								break;
							}
						}
						else
						{
							// If rack column name doesnt exist in columnIndexToRackColumnNameDict then skip this cell.
							if (!columnIndexToRackColumnNameDict.ContainsKey(iColumn))
								continue;

							// Read maximum load value.
							object objMaxLoad = xlRange[iRow, iColumn].Value2;
							if (objMaxLoad == null)
								continue;
							//
							try
							{
								double rMaxLoadValue = Convert.ToDouble(objMaxLoad);
								if (Utils.FLE(rMaxLoadValue, 0.0))
									continue;

								// Create new RackColumn.
								string strRackColumnName = columnIndexToRackColumnNameDict[iColumn];
								_AddRow(strRackColumnName, rMaxBeamSpan, rUSL, strBracing, rMaxLoadValue);
							}
							catch
							{
								continue;
							}
						}
					}
				}
			}
			catch { }
		}

		//=============================================================================
		// Index of the first row, from which need to read data.
		// Skip the first row, because it contains "Load per level in kg (Per pair of beams)" string.
		private static int BEAM_SHEET_START_ROW_INDEX = 2;
		private static void _ReadBeamSheet(Microsoft.Office.Interop.Excel._Worksheet beamSheet)
		{
			if (beamSheet == null)
				return;

			try
			{
				m_ReallyUsedBeamsList.Clear();

				// Data structure
				// column 1(A) - column system name
				// column 2(B) - max beam span
				// all other columns contains beams names with MaxLoad

				Microsoft.Office.Interop.Excel.Range xlRange = beamSheet.UsedRange;

				// Find maximum column index - find the first column with empty [3;index + 1] cell
				int iMaxColumnIndex = -1;
				// Column index to beam dictionary.
				Dictionary<int, RackBeam> columnIndexToBeamDict = new Dictionary<int, RackBeam>();
				// Fill dictionary
				for (int i = 3; i <= 100; ++i)
				{
					object objColumnName = xlRange[2, i].Value2;
					if (objColumnName == null)
						break;
					else
					{
						string strColumnName = objColumnName.ToString();
						if (!string.IsNullOrEmpty(strColumnName))
						{
							// try to find this beam in m_BeamsList
							RackBeam foundBeam = m_BeamsList.FirstOrDefault(beam => beam != null && beam.Name == strColumnName);
							if (foundBeam == null)
								continue;

							columnIndexToBeamDict[i] = foundBeam;
							iMaxColumnIndex = i;
						}
					}
				}
				//
				if (iMaxColumnIndex < 0)
					return;


				// Read data from the rows.
				// If there are 3 empty row then break data read.
				int iMaxEmptyRowsCellCount = 3;
				int iEmptyRowCellCount = 0;
				//
				for (int iRow = BEAM_SHEET_START_ROW_INDEX + 1; iRow < 10000; ++iRow)
				{
					if (iEmptyRowCellCount == iMaxEmptyRowsCellCount)
						break;

					// read column name
					RackColumn foundColumn = null;
					object obColumnName = xlRange[iRow, 1].Value2;
					if (obColumnName == null)
					{
						++iEmptyRowCellCount;
						continue;
					}
					else
					{
						string strColumnName = obColumnName.ToString();
						if(string.IsNullOrEmpty(strColumnName))
						{
							++iEmptyRowCellCount;
							continue;
						}
						// Column should exist in m_RacksColumnsList.
						foundColumn = m_RacksColumnsList.FirstOrDefault(c => c != null && c.Name == strColumnName);
						if (foundColumn == null)
							continue;
					}

					// read max beam span
					double rMaxBeamSpan = 0.0;
					object objMaxBeamSpan = xlRange[iRow, 2].Value2;
					if(objMaxBeamSpan == null)
					{
						continue;
					}
					else
					{
						try
						{
							rMaxBeamSpan = Convert.ToDouble(objMaxBeamSpan);
						}
						catch
						{
							continue;
						}
					}
					if (Utils.FLE(rMaxBeamSpan, 0.0))
						continue;

					// Read beams
					for (int iColumn = 3; iColumn <= iMaxColumnIndex; ++iColumn)
					{
						RackBeam foundBeam = null;
						if (columnIndexToBeamDict.ContainsKey(iColumn))
							foundBeam = columnIndexToBeamDict[iColumn];
						if (foundBeam == null)
							continue;

						// Read max beam load
						object objMaxBeamLoad = xlRange[iRow, iColumn].Value2;
						if (objMaxBeamLoad == null)
							continue;
						else
						{
							try
							{
								int maxBeamLoad = Convert.ToInt32(objMaxBeamLoad);
								if (foundColumn.AddBeam(rMaxBeamSpan, maxBeamLoad, foundBeam) && !m_ReallyUsedBeamsList.Contains(foundBeam))
									m_ReallyUsedBeamsList.Add(foundBeam);
							}
							catch { }
						}
					}

					iEmptyRowCellCount = 0;
				}
			}
			catch { }
		}

		//=============================================================================
		// Index of the first row, from which need to read data.
		private static int BEAMWEIGHT_SHEET_START_ROW_INDEX = 2;
		private static void _ReadBeamWeightSheet(Microsoft.Office.Interop.Excel._Worksheet beamWeightSheet)
		{
			if (beamWeightSheet == null)
				return;

			try
			{
				m_BeamsList.Clear();

				// Data structure
				// column 1(A) - beam name
				// column 2(B) - beam weight for 1 meter
				// column 3(C) - two end connectors weight

				Microsoft.Office.Interop.Excel.Range xlRange = beamWeightSheet.UsedRange;

				// Read data from the rows.
				// If there are 1 empty row then break data read.
				int iMaxEmptyRowsCellCount = 1;
				int iEmptyRowCellCount = 0;
				//
				for (int iRow = BEAMWEIGHT_SHEET_START_ROW_INDEX + 1; iRow < 1000; ++iRow)
				{
					if (iEmptyRowCellCount == iMaxEmptyRowsCellCount)
						break;

					string strBeamName = string.Empty;
					// Beam name
					object objBeamName = xlRange[iRow, 1].Value2;
					if (objBeamName == null)
					{
						++iEmptyRowCellCount;
						continue;
					}
					else
					{
						strBeamName = objBeamName.ToString();
					}

					double weightPerMeter = 0.0;
					// Weight per meter
					object objWeightPerMeter = xlRange[iRow, 2].Value2;
					if(objWeightPerMeter == null)
					{
						++iEmptyRowCellCount;
						continue;
					}
					else
					{
						try
						{
							weightPerMeter = Convert.ToDouble(objWeightPerMeter);
							if (Utils.FLE(weightPerMeter, 0.0))
								continue;
						}
						catch
						{
							continue;
						}
					}

					double endConnWeight = 0.0;
					// Both end connectors weight
					object objEndConnWeight = xlRange[iRow, 3].Value2;
					if (objEndConnWeight == null)
					{
						++iEmptyRowCellCount;
						continue;
					}
					else
					{
						try
						{
							endConnWeight = Convert.ToDouble(objEndConnWeight);
							if (Utils.FLE(endConnWeight, 0.0))
								continue;
						}
						catch
						{
							continue;
						}
					}

					iEmptyRowCellCount = 0;

					// Add beam
					if(!string.IsNullOrEmpty(strBeamName) && Utils.FGT(weightPerMeter, 0.0) && Utils.FGT(endConnWeight, 0.0))
					{
						// try to find beam with the same name
						RackBeam foundBeam = m_BeamsList.FirstOrDefault(beam => beam != null && beam.Name == strBeamName);
						if (foundBeam != null)
							continue;

						m_BeamsList.Add(new RackBeam(strBeamName, weightPerMeter, endConnWeight));
					}
				}
			}
			catch { }
		}


		//=============================================================================
		/// <summary>
		/// Return the column which can handle passed parameter.
		/// </summary>
		public static RackColumn GetRackColumn(List<RackColumn> RacksColumnsList, double beamSpan, double usl, double load, out eColumnBracingType bracingType)
		{
			bracingType = eColumnBracingType.eUndefined;

			if (RacksColumnsList == null)
				return null;

			if (Utils.FLE(beamSpan, 0.0) || Utils.FLE(usl, 0.0) || Utils.FLE(load, 0.0))
				return null;

			foreach(RackColumn rackColumn in RacksColumnsList)
			{
				if (rackColumn == null)
					continue;

				// Search algo:
				// 1. Go through the column and look at the bracing type.
				// 2. If bracing type is NormalBracing or XBracing then stop search and return this column.
				// 3. Otherwise take next column and compare.
				//
				// Column are ordered by the way they are ordered in the excel file.
				eColumnBracingType columnBracingType = rackColumn.GetBracing(beamSpan, usl, load);
				if (eColumnBracingType.eUndefined != columnBracingType)
				{
					bracingType = columnBracingType;
					return rackColumn;
				}
			}

			return null;
		}

		//=============================================================================
		public static string NORMAL_BRACING = "Normal Bracing";
		public static string X_BRACING = "X Bracing";
		public static string NORMAL_BRACING_WITH_STIFFENER = "Normal Bracing+Stiffener";
		public static string X_BRACING_WITH_STIFFENER = "X Bracing+Stiffener";
		//
		public static eColumnBracingType GetColumnBracingTypeByString(string strBracingType)
		{
			if (string.IsNullOrEmpty(strBracingType))
				return eColumnBracingType.eUndefined;

			if (NORMAL_BRACING == strBracingType)
				return eColumnBracingType.eNormalBracing;
			else if (X_BRACING == strBracingType)
				return eColumnBracingType.eXBracing;
			else if (NORMAL_BRACING_WITH_STIFFENER == strBracingType)
				return eColumnBracingType.eNormalBracingWithStiffener;
			else if (X_BRACING_WITH_STIFFENER == strBracingType)
				return eColumnBracingType.eXBracingWithStiffener;

			return eColumnBracingType.eUndefined;
		}
		//
		public static string GetColumnBracingTypeByEnum(eColumnBracingType bracingType)
		{
			if (eColumnBracingType.eNormalBracing == bracingType)
				return NORMAL_BRACING;
			else if (eColumnBracingType.eXBracing == bracingType)
				return X_BRACING;
			else if (eColumnBracingType.eNormalBracingWithStiffener == bracingType)
				return NORMAL_BRACING_WITH_STIFFENER;
			else if (eColumnBracingType.eXBracingWithStiffener == bracingType)
				return X_BRACING_WITH_STIFFENER;

			return string.Empty;
		}
	}

	[Serializable]
	public class RackBeam : ISerializable, IDeserializationCallback
	{
		public RackBeam(string name, double weightPerMeter, double bothEndConnectorsWeight)
		{
			m_Name = name;
			m_WeightPerMeter = weightPerMeter;
			m_BothEndConnectorsWeight = bothEndConnectorsWeight;

			// Parse name for beam height.
			try
			{
				// Remove all till the first space.
				// For example - "BEAM1 123 x 90".
				string sizeOnly = Regex.Replace(m_Name, @"^\S+\s+", "");
				// remove all except numbers and 'x'
				sizeOnly = Regex.Replace(sizeOnly, "[^0-9.x]", "");
				if (!string.IsNullOrEmpty(sizeOnly))
				{
					string[] sizeArr = sizeOnly.Split('x');
					if (sizeArr.Count() == 2)
					{
						double rValue;
						if (Utils.ConvertToDouble(sizeArr[0], out rValue) && Utils.FGT(rValue, 0.0))
							m_Height = rValue;
						if (Utils.ConvertToDouble(sizeArr[1], out rValue) && Utils.FGT(rValue, 0.0))
							m_Thickness = rValue;
					}
				}
			}
			catch { }
		}

		//=============================================================================
		// Name of beam, like "HEM 110 x 1.5".
		private string m_Name = string.Empty;
		public string Name { get { return m_Name; } }

		//=============================================================================
		// Weight for each meter of this beam.
		// It doesnt include end connectors weight.
		private double m_WeightPerMeter = 1.0;
		public double WeightPerMeter { get { return m_WeightPerMeter; } }

		//=============================================================================
		// Weight of both end connectors.
		private double m_BothEndConnectorsWeight = 1.0;
		public double BothEndConnectorsWeight { get { return m_BothEndConnectorsWeight; } }

		//=============================================================================
		// Unique ID of RackBeam
		private Guid m_GUID = Guid.NewGuid();
		public Guid GUID { get { return m_GUID; } }

		//=============================================================================
		// Height of the beam.
		private double m_Height = 0.0;
		public double Height { get { return m_Height; } }

		//=============================================================================
		// Thickness of the beam.
		private double m_Thickness = 0.0;
		public double Thickness { get { return m_Thickness; } }

		#region Serialization

		// 1.0
		protected static string _sRackBeam_strMajor = "RackBeam_MAJOR";
		protected static int _sRackBeam_MAJOR = 1;
		protected static string _sRackBeam_strMinor = "RackBeam_MINOR";
		protected static int _sRackBeam_MINOR = 0;

		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sRackBeam_strMajor, _sRackBeam_MAJOR);
			info.AddValue(_sRackBeam_strMinor, _sRackBeam_MINOR);

			// 1.0
			info.AddValue("Name", m_Name);
			info.AddValue("WeightPerMeter", m_WeightPerMeter);
			info.AddValue("BothEndConnectorsWeight", m_BothEndConnectorsWeight);
			info.AddValue("m_GUID", m_GUID);
			info.AddValue("Height", m_Height);
			info.AddValue("Thickness", m_Thickness);
		}

		//=============================================================================
		public RackBeam(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sRackBeam_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sRackBeam_strMinor, typeof(int));
			if (iMajor > _sRackBeam_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sRackBeam_MAJOR && iMinor > _sRackBeam_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sRackBeam_MAJOR)
			{
				// restore
				try
				{
					if (iMajor >= 1 && iMinor >= 0)
					{
						m_Name = (string)info.GetValue("Name", typeof(string));
						m_WeightPerMeter = (double)info.GetValue("WeightPerMeter", typeof(double));
						m_BothEndConnectorsWeight = (double)info.GetValue("BothEndConnectorsWeight", typeof(double));
						m_GUID = (Guid)info.GetValue("m_GUID", typeof(Guid));
						m_Height = (double)info.GetValue("Height", typeof(double));
						m_Thickness = (double)info.GetValue("Thickness", typeof(double));
					}
				}
				catch
				{
					++DrawingDocument._sStreamReadException;
				}
			}
			else
				++DrawingDocument._sBiggerMajorNumber;
		}
		//=============================================================================
		public virtual void OnDeserialization(object sender) { }

		#endregion
	}

	[Serializable]
	public class RackColumn : ISerializable, IDeserializationCallback
	{
		internal RackColumn(string strName)
		{
			m_Name = strName;

			// Parse name.
			// Get length, depth and thickness.
			try
			{
				string sizeOnly = Regex.Replace(m_Name, "[^0-9. -]", "");
				if(!string.IsNullOrEmpty(sizeOnly))
				{
					string[] sizeArr = sizeOnly.Split('-');
					if(sizeArr.Count() == 3)
					{
						double rValue;
						if (Utils.ConvertToDouble(sizeArr[0], out rValue))
							m_Length = rValue;
						if (Utils.ConvertToDouble(sizeArr[1], out rValue))
							m_Depth = rValue;
						if (Utils.ConvertToDouble(sizeArr[2], out rValue))
							m_Thickness = rValue;
					}
				}
			}
			catch { }
		}

		//=============================================================================
		/// <summary>
		/// Max beam length.
		/// USL - unsupported length value.
		/// Bracing type. Displayed at the advanced properties picture in the SIDE rack view.
		/// Max load value for this beam span, USL and bracing type.
		/// 
		/// MaxBeamSpan - USL - eColumnBracingType - max load value
		/// </summary>
		private Dictionary<double, Dictionary<double, Dictionary<eColumnBracingType, double>>> m_FrameLoadDictionary = new Dictionary<double, Dictionary<double, Dictionary<eColumnBracingType, double>>>();

		//=============================================================================
		/// <summary>
		/// Dictionary with max load for the beams.
		/// 
		/// Path:
		/// Max beam span - max beam load - beams list.
		/// Max beam span means beam length without Rack.INNER_LENGTH_ADDITIONAL_GAP.
		/// </summary>
		private SortedDictionary<double, SortedDictionary<double, List<RackBeam>>> m_BeamMaxLoadDictionary = new SortedDictionary<double, SortedDictionary<double, List<RackBeam>>>();

		//=============================================================================
		/// <summary>
		/// Full name of the column.
		/// Example: "GXL 90-70-1.6".
		/// </summary>
		private string m_Name = string.Empty;
		public string Name { get { return m_Name; } }

		//=============================================================================
		/// <summary>
		/// Name for display in the UI.
		/// It is full name without depth.
		/// </summary>
		private string m_DisplayName = null;
		public string DisplayName
		{
			get
			{
				if (m_DisplayName == null)
				{
					// Get display name.
					// Get letters before the first space character and add length and thickness.
					string[] strArr = m_Name.Split(' ');
					if (strArr.Count() == 2 && !string.IsNullOrEmpty(strArr[0]))
					{
						m_DisplayName = strArr[0];
						m_DisplayName += " ";
						m_DisplayName += m_Length.ToString();
						m_DisplayName += "-";
						m_DisplayName += m_Thickness.ToString();
					}
					else
						m_DisplayName = m_Name;

					if (m_DisplayName != null)
						m_DisplayName = m_DisplayName.Replace(",", ".");
				}

				return m_DisplayName;
			}
		}

		//=============================================================================
		// Unique ID of RackColumn
		private Guid m_GUID = Guid.NewGuid();
		public Guid GUID { get { return m_GUID; } }

		//=============================================================================
		private double m_Length = 90;
		public double Length { get { return m_Length; } }
		//=============================================================================
		private double m_Depth = 70;
		public double Depth { get { return m_Depth; } }
		//=============================================================================
		private double m_Thickness = 1.6;
		public double Thickness { get { return m_Thickness; } }

		//=============================================================================
		/// <summary>
		/// Add row to this column.
		/// </summary>
		public bool AddRow(double maxBeamSpan, double usl, string strBracingType, double maxLoad)
		{
			if (Utils.FLE(maxBeamSpan, 0.0) || Utils.FLE(usl, 0.0) || string.IsNullOrEmpty(strBracingType) || Utils.FLE(maxLoad, 0.0))
				return false;

			eColumnBracingType bracingType = RackLoadUtils.GetColumnBracingTypeByString(strBracingType.Trim());
			if (eColumnBracingType.eUndefined == bracingType)
				return false;

			//
			if (!m_FrameLoadDictionary.ContainsKey(maxBeamSpan) || m_FrameLoadDictionary[maxBeamSpan] == null)
				m_FrameLoadDictionary[maxBeamSpan] = new Dictionary<double, Dictionary<eColumnBracingType, double>>();
			//
			if (!m_FrameLoadDictionary[maxBeamSpan].ContainsKey(usl) || m_FrameLoadDictionary[maxBeamSpan][usl] == null)
				m_FrameLoadDictionary[maxBeamSpan][usl] = new Dictionary<eColumnBracingType, double>();

			m_FrameLoadDictionary[maxBeamSpan][usl][bracingType] = maxLoad;
			return true;
		}

		//=============================================================================
		// Add beam to m_BeamMaxLoadDictionary.
		// maxBeamSpan meanth beam length without Rack.INNER_LENGTH_ADDITIONAL_GAP.
		public bool AddBeam(double maxBeamSpan, double maxBeamLoad, RackBeam beam)
		{
			if (Utils.FLE(maxBeamSpan, 0.0) || Utils.FLE(maxBeamLoad, 0.0) || beam == null)
				return false;

			if (!m_BeamMaxLoadDictionary.ContainsKey(maxBeamSpan))
				m_BeamMaxLoadDictionary[maxBeamSpan] = new SortedDictionary<double, List<RackBeam>>();

			if (!m_BeamMaxLoadDictionary[maxBeamSpan].ContainsKey(maxBeamLoad))
				m_BeamMaxLoadDictionary[maxBeamSpan][maxBeamLoad] = new List<RackBeam>();

			// Probably this beam already exists in the list.
			if (m_BeamMaxLoadDictionary[maxBeamSpan][maxBeamLoad].Contains(beam))
				return false;

			m_BeamMaxLoadDictionary[maxBeamSpan][maxBeamLoad].Add(beam);
			return true;
		}

		//=============================================================================
		public eColumnBracingType GetBracing(double beamSpan, double usl, double load)
		{
			if (Utils.FLE(beamSpan, 0.0) || Utils.FLE(usl, 0.0) || Utils.FLE(load, 0.0))
				return eColumnBracingType.eUndefined;

			foreach(double beamSpanKey in m_FrameLoadDictionary.Keys)
			{
				if (Utils.FGT(beamSpan, beamSpanKey))
					continue;

				if (m_FrameLoadDictionary[beamSpanKey] == null)
					continue;

				foreach(double uslKey in m_FrameLoadDictionary[beamSpanKey].Keys)
				{
					if (Utils.FGT(usl, uslKey))
						continue;

					if (m_FrameLoadDictionary[beamSpanKey][uslKey] == null)
						continue;

					foreach(eColumnBracingType bracingTypeKey in m_FrameLoadDictionary[beamSpanKey][uslKey].Keys)
					{
						if (eColumnBracingType.eUndefined == bracingTypeKey)
							continue;

						double maxLoadValue = m_FrameLoadDictionary[beamSpanKey][uslKey][bracingTypeKey];
						if (Utils.FLE(load, maxLoadValue))
							return bracingTypeKey;
					}
				}
			}

			return eColumnBracingType.eUndefined;
		}

		//=============================================================================
		/// <summary>
		/// Find beam in m_BeamMaxLoadDictionary.
		/// </summary>
		/// <param name="maxBeamSpan">
		/// It means beam length without Rack.INNER_LENGTH_ADDITIONAL_GAP.
		/// </param>
		/// <param name="beamLoad">
		/// Level current load.
		/// </param>
		/// <returns></returns>
		public RackBeam FindBeam(double beamSpan, double beamLoad)
		{
			if (m_BeamMaxLoadDictionary == null)
				return null;

			// Go through m_BeamMaxLoadDictionary and try to find beam.

			// Find maxBeamSpan
			foreach(double maxBeamSpan in m_BeamMaxLoadDictionary.Keys)
			{
				if (Utils.FGT(beamSpan, maxBeamSpan))
					continue;

				if (m_BeamMaxLoadDictionary[maxBeamSpan] == null)
					continue;

				// Find maxBeamLoad
				foreach(double maxBeamLoad in m_BeamMaxLoadDictionary[maxBeamSpan].Keys)
				{
					if (Utils.FGT(beamLoad, maxBeamLoad))
						continue;

					if (m_BeamMaxLoadDictionary[maxBeamSpan][maxBeamLoad] == null)
						continue;
					if (m_BeamMaxLoadDictionary[maxBeamSpan][maxBeamLoad].Count == 0)
						continue;

					// Just take the first beam from list, because they are sorted by the weight after
					// load from LoadChart.xlsx using RackColumn.SortBeams() method.
					foreach(RackBeam beam in m_BeamMaxLoadDictionary[maxBeamSpan][maxBeamLoad])
					{
						if (beam == null)
							continue;

						return beam;
					}
				}
			}

			return null;
		}

		//=============================================================================
		// Sort beams in m_BeamMaxLoadDictionary by weight.
		public void SortBeams()
		{
			if (m_BeamMaxLoadDictionary == null)
				return;

			foreach(double maxBeamsSpan in m_BeamMaxLoadDictionary.Keys)
			{
				foreach(double maxBeamLoad in m_BeamMaxLoadDictionary[maxBeamsSpan].Keys)
				{
					m_BeamMaxLoadDictionary[maxBeamsSpan][maxBeamLoad].Sort(delegate (RackBeam beam_01, RackBeam beam_02)
					{
						if (beam_01 == null && beam_02 == null)
							return 0;
						else if (beam_01 == null)
							return -1;
						else if (beam_02 == null)
							return 1;
						else
						{
							double weight_01 = beam_01.WeightPerMeter + beam_01.BothEndConnectorsWeight;
							double weight_02 = beam_02.WeightPerMeter + beam_02.BothEndConnectorsWeight;

							if (Utils.FEQ(weight_01, weight_02))
								return 0;
							else if (Utils.FLT(weight_01, weight_02))
								return -1;
							else
								return 1;
						}

						return 0;
					});
				}
			}
		}

		#region Serialization

		// 1.0
		// 2.0 Add m_BeamMaxLoadDictionary
		protected static string _sRackColumn_strMajor = "RackColumn_MAJOR";
		protected static int _sRackColumn_MAJOR = 2;
		protected static string _sRackColumn_strMinor = "RackColumn_MINOR";
		protected static int _sRackColumn_MINOR = 0;

		//=============================================================================
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sRackColumn_strMajor, _sRackColumn_MAJOR);
			info.AddValue(_sRackColumn_strMinor, _sRackColumn_MINOR);

			// 1.0
			info.AddValue("Name", m_Name);
			info.AddValue("Length", m_Length);
			info.AddValue("Depth", m_Depth);
			info.AddValue("Thickness", m_Thickness);
			info.AddValue("FrameLoadDictionary", m_FrameLoadDictionary);
			info.AddValue("m_GUID", m_GUID);

			// 2.0
			info.AddValue("BeamMaxLoadDictionary", m_BeamMaxLoadDictionary);
		}

		//=============================================================================
		public RackColumn(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sRackColumn_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sRackColumn_strMinor, typeof(int));
			if (iMajor > _sRackColumn_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sRackColumn_MAJOR && iMinor > _sRackColumn_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sRackColumn_MAJOR)
			{
				// restore
				try
				{
					if (iMajor >= 1 && iMinor >= 0)
					{
						m_Name = (string)info.GetValue("Name", typeof(string));
						m_Length = (double)info.GetValue("Length", typeof(double));
						m_Depth = (double)info.GetValue("Depth", typeof(double));
						m_Thickness = (double)info.GetValue("Thickness", typeof(double));
						m_FrameLoadDictionary = (Dictionary<double, Dictionary<double, Dictionary<eColumnBracingType, double>>>)info.GetValue("FrameLoadDictionary", typeof(Dictionary<double, Dictionary<double, Dictionary<eColumnBracingType, double>>>));
						m_GUID = (Guid)info.GetValue("m_GUID", typeof(Guid));
					}

					if(iMajor >= 2 && iMinor >= 0)
					{
						m_BeamMaxLoadDictionary = (SortedDictionary<double, SortedDictionary<double, List<RackBeam>>>)info.GetValue("BeamMaxLoadDictionary", typeof(SortedDictionary<double, SortedDictionary<double, List<RackBeam>>>));
					}
				}
				catch
				{
					++DrawingDocument._sStreamReadException;
				}
			}
			else
				++DrawingDocument._sBiggerMajorNumber;
		}
		//=============================================================================
		public virtual void OnDeserialization(object sender)
		{
			// call deserialization on dictionaries, otherwise you can work with non-deserialised dictionary
			m_FrameLoadDictionary.OnDeserialization(sender);
		}

		#endregion
	}
}
