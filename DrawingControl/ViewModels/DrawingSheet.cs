using AppColorTheme;
using AppInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace DrawingControl
{
	[Serializable]
	public class DrawingSheet : BaseViewModel, ISerializable, IDeserializationCallback, IGeomDisplaySettings, IClonable
	{
		public DrawingSheet(DrawingDocument doc)
		{
			m_GUID = Guid.NewGuid();

			Document = doc;
			m_StatisticsCollectionView = CollectionViewSource.GetDefaultView(m_RackStatistics);
			m_PalletsStatisticsCollectionView = CollectionViewSource.GetDefaultView(m_PalletsStatistics);
			m_SelectedGeometryCollection.CollectionChanged += SelectedGeometryCollection_CollectionChanged;

			IsNewSheet = true;

			_InitName();
			_InitRoofList();
			CheckSheetElevationGeometry();
		}
		public DrawingSheet(DrawingSheet sheet)
		{
			if(sheet != null)
			{
				// Clone roofs
				m_RoofsList.Clear();
				if(sheet.RoofsList != null)
				{
					foreach(Roof roof in sheet.RoofsList)
					{
						if (roof == null)
							continue;

						Roof roofClone = roof.Clone() as Roof;
						if (roofClone == null)
							continue;

						m_RoofsList.Add(roofClone);
					}
				}

				// Dictionary with old rack as a key and cloned rack as a value.
				// It is used for restore RacksGroups list, because RacksGroups list cant be cloned.
				Dictionary<Rack, Rack> oldRackToClondedRackDict = new Dictionary<Rack, Rack>();
				// Clone geometry
				if(sheet.Rectangles != null)
				{
					foreach(BaseRectangleGeometry geom in sheet.Rectangles)
					{
						if (geom == null)
							continue;

						BaseRectangleGeometry geomClone = geom.Clone() as BaseRectangleGeometry;
						if (geomClone == null)
							continue;

						geomClone.Sheet = this;
						this.Rectangles.Add(geomClone);

						Rack oldRack = geom as Rack;
						Rack clonedRack = geomClone as Rack;
						if (oldRack != null && clonedRack != null)
							oldRackToClondedRackDict[oldRack] = clonedRack;
					}
				}
				// Restore RacksGroups list.
				this.RacksGroups.Clear();
				foreach(List<Rack> rackGroup in sheet.RacksGroups)
				{
					if (rackGroup == null)
						continue;

					List<Rack> rackGroupClone = new List<Rack>();
					foreach(Rack oldRack in rackGroup)
					{
						if (oldRack == null)
							continue;

						Rack rackClone = null;
						if (oldRackToClondedRackDict.ContainsKey(oldRack))
							rackClone = oldRackToClondedRackDict[oldRack];

						if (rackClone != null)
							rackGroupClone.Add(rackClone);
					}

					if (rackGroupClone.Count > 0)
						this.RacksGroups.Add(rackGroupClone);
				}
				// Clone column patterns
				if(sheet.m_patterns != null)
				{
					foreach(ColumnPattern columnPattern in sheet.m_patterns)
					{
						if (columnPattern == null)
							continue;

						ColumnPattern columnPatternClone = columnPattern.Clone() as ColumnPattern;
						if (columnPatternClone == null)
							continue;

						columnPatternClone.Sheet = this;
						this.m_patterns.Add(columnPatternClone);
					}
				}

				//
				this.m_Name = sheet.m_Name;
				if (string.IsNullOrEmpty(Name))
					_InitName();
				this.m_Length = sheet.m_Length;
				this.m_Width = sheet.m_Width;
				this.m_Notes = sheet.m_Notes;
				this.m_UnitsPerCameraPixel = sheet.m_UnitsPerCameraPixel;
				this.m_Is_UnitsPerCameraPixel_Init = sheet.m_Is_UnitsPerCameraPixel_Init;
				this.m_MaxUnitsPerCameraPixel = sheet.m_MaxUnitsPerCameraPixel;
				this.m_CameraOffset = sheet.m_CameraOffset;

				// Clone tie beams
				if(sheet.TieBeamsList != null)
				{
					foreach (TieBeam tieBeamGeom in sheet.TieBeamsList)
					{
						if (tieBeamGeom == null)
							continue;

						TieBeam tieBeamGeomClone = tieBeamGeom.Clone() as TieBeam;
						if (tieBeamGeomClone == null)
							continue;

						tieBeamGeomClone.Sheet = this;
						this.TieBeamsList.Add(tieBeamGeomClone);
					}
				}

				//
				this.m_GUID = sheet.m_GUID;

				// Copy snapping lines
				m_HorizontalLines.Clear();
				m_HorizontalLines.AddRange(sheet.m_HorizontalLines);
				m_VerticalLines.Clear();
				m_VerticalLines.AddRange(sheet.m_VerticalLines);

				// Copy racks and pallets statistics
				if(sheet.m_RackStatistics != null)
				{
					foreach(StatRackItem rackStatItem in sheet.m_RackStatistics)
					{
						if (rackStatItem == null)
							continue;

						StatRackItem newItem = new StatRackItem(rackStatItem.RackIndex, m_RackStatistics);
						newItem.Count_A = rackStatItem.Count_A;
						newItem.Count_M = rackStatItem.Count_M;
						newItem.Height = rackStatItem.Height;
						newItem.Length_A = rackStatItem.Length_A;
						newItem.Length_M = rackStatItem.Length_M;
						newItem.Width = rackStatItem.Width;
						newItem.Load = rackStatItem.Load;
						m_RackStatistics.Add(newItem);
					}
				}
				if(sheet.m_PalletsStatistics != null)
				{
					foreach(StatPalletItem palletStatItem in sheet.m_PalletsStatistics)
					{
						if (palletStatItem == null)
							continue;

						StatPalletItem newPalletStatItem = new StatPalletItem(palletStatItem.ZeroBasedIndex);
						newPalletStatItem.Count = palletStatItem.Count;
						newPalletStatItem.Height = palletStatItem.Height;
						newPalletStatItem.Length = palletStatItem.Length;
						newPalletStatItem.Load = palletStatItem.Load ;
						newPalletStatItem.Width = palletStatItem.Width;
						m_PalletsStatistics.Add(newPalletStatItem);
					}
				}
			}

			//// RacksGroups are not cloned, so recalculate racks groups
			//this.m_CalculationState |= eCalculationState.eForceRecalculateRacksGroups;
			//this._UpdateRackRowsColumns(true, false, false);

			// Restore selected geometry collection
			//
			// Dont clear m_SelectedGeometryCollection, because BaseRectangleGeometry.IsSelected is set to false inside
			// collection change callback.
			// Inside next foreach loop geometry will not be added in m_SelectedGeometryCollection.
			//m_SelectedGeometryCollection.Clear();
			foreach (BaseRectangleGeometry geom in this.Rectangles)
			{
				if (geom == null)
					continue;

				if (geom.IsSelected && !m_SelectedGeometryCollection.Contains(geom))
					m_SelectedGeometryCollection.Add(geom);
			}

			m_SelectedGeometryCollection.CollectionChanged += SelectedGeometryCollection_CollectionChanged;
			m_StatisticsCollectionView = CollectionViewSource.GetDefaultView(m_RackStatistics);
			m_PalletsStatisticsCollectionView = CollectionViewSource.GetDefaultView(m_PalletsStatistics);
		}

		#region Fields

		//
		private ObservableCollection<StatPalletItem> m_PalletsStatistics = new ObservableCollection<StatPalletItem>();

		//
		private List<ColumnPattern> m_patterns = new List<ColumnPattern>();

		// snapping lines
		private double m_SnapDistance = 100;
		// contains Y coordinates
		public List<double> m_HorizontalLines = new List<double>();
		// containx X coordinates
		public List<double> m_VerticalLines = new List<double>();

		[Flags]
		private enum eCalculationState : int
		{
			eNone = 0,
			eForceRecalculateRacksGroups = 1,
		}
		private eCalculationState m_CalculationState = eCalculationState.eNone;

		/// <summary>
		/// Drawing sheet updates(recalculates) after the last DrawingDocument.MarkStateChanged() call.
		/// It is used to determine which geometry was changed after sheet update.
		/// </summary>
		private long m_UpdateCount = 0;
		public long UpdateCount { get { return m_UpdateCount; } }

		#endregion

		#region Properties

		//=============================================================================
		/// <summary>
		/// Guid of this sheet.
		/// </summary>
		private Guid m_GUID = Guid.Empty;
		public Guid GUID { get { return m_GUID; } }

		//=============================================================================
		public DrawingDocument Document { get; set; }

		//=============================================================================
		private string m_Name = string.Empty;
		public string Name
		{
			get
			{
				return m_Name;
			}
			set
			{
				if(m_Name != value)
				{
					m_Name = value;

					if (string.IsNullOrEmpty(m_Name))
						_InitName();

					NotifyPropertyChanged(() => Name);
					NotifyPropertyChanged(() => DisplayName);
				}
			}
		}
		//=============================================================================
		public string DisplayName
		{
			get
			{
				string strName = m_Name;

				if (HasChanges)
					strName += "*";

				return strName;
			}
		}

		//=============================================================================
		private bool m_bIsSelected = false;
		public bool IsSelected
		{
			get { return m_bIsSelected; }
			set
			{
				if(m_bIsSelected != value)
				{
					m_bIsSelected = value;
					NotifyPropertyChanged(() => IsSelected);
				}
			}
		}

		//=============================================================================
		public bool IsNewSheet { get; set; }

		//=============================================================================
		// If true then this sheet has some changes which are not saved yet.
		public bool HasChanges { get { return m_ChangedSheetsGuidsSet.Contains(this.GUID); } }
		// Set with GUID of DrawingSheets which have changes.
		public static HashSet<Guid> m_ChangedSheetsGuidsSet = new HashSet<Guid>();

		//=============================================================================
		/// <summary>
		/// All rectangles at this sheet - racks, columns, blocks etc.
		/// Temprorary rectangles are not included in this list.
		/// </summary>
		public List<BaseRectangleGeometry> Rectangles = new List<BaseRectangleGeometry>();

		//=============================================================================
		// Tie beams between racks. They are used to support racks with hight Height\Depth ratio.
		public List<TieBeam> TieBeamsList = new List<TieBeam>();

		//=============================================================================
		/// <summary>
		/// Temporary columns which are created while dragging create column pattern grip point.
		/// </summary>
		public List<List<BaseRectangleGeometry>> TemporaryColumnsInPattern = new List<List<BaseRectangleGeometry>>();
		/// <summary>
		/// Temporary racks which are created while user draggion create rack's row\column grip point.
		/// </summary>
		public List<BaseRectangleGeometry> TemporaryRacksList = new List<BaseRectangleGeometry>();

		//=============================================================================
		// Rack rows and columns.
		public List<List<Rack>> RacksGroups = new List<List<Rack>>();

		//=============================================================================
		/// <summary>
		/// List with selected geometry
		/// </summary>
		private ObservableCollection<BaseRectangleGeometry> m_SelectedGeometryCollection = new ObservableCollection<BaseRectangleGeometry>();
		public ObservableCollection<BaseRectangleGeometry> SelectedGeometryCollection { get { return m_SelectedGeometryCollection; } }
		//=============================================================================
		/// <summary>
		/// List with non initialized geometry from SelectedGeometryCollection
		/// </summary>
		public List<BaseRectangleGeometry> NonInitSelectedGeometryList
		{
			get
			{
				List<BaseRectangleGeometry> nonInitSelectedGeomList = new List<BaseRectangleGeometry>();

				foreach (BaseRectangleGeometry geom in SelectedGeometryCollection)
				{
					if (geom == null)
						continue;

					if (!geom.IsInit)
						nonInitSelectedGeomList.Add(geom);
				}

				return nonInitSelectedGeomList;
			}
		}
		//=============================================================================
		/// <summary>
		/// Returns single geometry if m_SelectedGeometryCollection contains only one item.
		/// Used for display properties.
		/// </summary>
		public BaseRectangleGeometry SingleSelectedGeometry
		{
			get
			{
				if (m_SelectedGeometryCollection.Count == 1)
					return m_SelectedGeometryCollection[0];
				return null;
			}
		}

		//=============================================================================
		public List<BaseRectangleGeometry> HiglightedRectangles = new List<BaseRectangleGeometry>();

		//=============================================================================
		/// <summary>
		/// Collection with racks statistics.
		/// It contains racks count, A and M length, count, etc.
		/// </summary>
		private ObservableCollection<StatRackItem> m_RackStatistics = new ObservableCollection<StatRackItem>();
		public ObservableCollection<StatRackItem> RackStatistics
		{
			get { return m_RackStatistics; }
		}

		//=============================================================================
		/// <summary>
		/// Racks statistics collection
		/// </summary>
		private ICollectionView m_StatisticsCollectionView = null;
		public ICollectionView StatisticsCollection
		{
			get
			{
				return m_StatisticsCollectionView;
			}
		}

		//=============================================================================
		public ObservableCollection<StatPalletItem> PalletsStatistics { get { return m_PalletsStatistics; } }

		//=============================================================================
		private ICollectionView m_PalletsStatisticsCollectionView = null;
		public ICollectionView PalletsStatisticsCollectionView { get { return m_PalletsStatisticsCollectionView; } }

		//=============================================================================
		private UInt32 m_Length = 30000;
		public UInt32 Length
		{
			get { return m_Length; }
			set
			{
				if (m_Length != value)
				{
					bool bSetValue = false;

					if (IsThereRectanglesOutsideGraphicsArea(value, m_Width, true))
					{
						if (DrawingDocument._sDrawing != null)
						{
							DrawingDocument._sDrawing.PreviewGlobalLength = (int)value;
							DrawingDocument._sDrawing.UpdateDrawing(false);
						}

						// ask user
						if (MessageBox.Show("The updated length cannot fit some of the block(s).These blocks will be deleted.Do you want to continue?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
						{
							// delete
							IsThereRectanglesOutsideGraphicsArea(value, m_Width, false);
							bSetValue = true;
						}

						//
						if (DrawingDocument._sDrawing != null)
						{
							DrawingDocument._sDrawing.PreviewGlobalLength = -1;
							DrawingDocument._sDrawing.PreviewGlobalWidth = -1;
						}
					}
					else
						bSetValue = true;

					if (bSetValue)
					{
						m_Length = value;

						// If length is increased then it affect on the roof and max height for all rectangles.
						this.CheckRackHeight(Rectangles, true);
						this.CheckTieBeams();

						this.CheckSheetElevationGeometry();

						MarkStateChanged();
					}

					NotifyPropertyChanged(() => Length);

					if (Document != null)
						Document.OnCurrentSheetSizeChanged();
				}
			}
		}

		//=============================================================================
		private UInt32 m_Width = 20000;
		public UInt32 Width
		{
			get { return m_Width; }
			set
			{
				if (m_Width != value)
				{
					bool bSetValue = false;

					if (IsThereRectanglesOutsideGraphicsArea(m_Length, value, true))
					{
						if (DrawingDocument._sDrawing != null)
						{
							DrawingDocument._sDrawing.PreviewGlobalWidth = (int)value;
							DrawingDocument._sDrawing.UpdateDrawing(false);
						}
						// ask user
						if (MessageBox.Show("The updated width cannot fit some of the block(s).These blocks will be deleted.Do you want to continue?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
						{
							// delete
							IsThereRectanglesOutsideGraphicsArea(m_Length, value, false);
							bSetValue = true;
						}

						//
						if (DrawingDocument._sDrawing != null)
						{
							DrawingDocument._sDrawing.PreviewGlobalLength = -1;
							DrawingDocument._sDrawing.PreviewGlobalWidth = -1;
						}
					}
					else
						bSetValue = true;

					if (bSetValue)
					{
						m_Width = value;

						// If width is increased then it affect on the roof and max height for all rectangles.
						this.CheckRackHeight(Rectangles, true);
						this.CheckTieBeams();

						this.CheckSheetElevationGeometry();

						MarkStateChanged();
					}

					NotifyPropertyChanged(() => Width);

					if (Document != null)
						Document.OnCurrentSheetSizeChanged();
				}
			}
		}

		//=============================================================================
		// List with available roofs.
		private List<Roof> m_RoofsList = new List<Roof>();
		public List<Roof> RoofsList { get { return m_RoofsList; } }

		//=============================================================================
		// Selected roof from RoofsList.
		public Roof SelectedRoof { get { return m_RoofsList.FirstOrDefault(r => r != null && r.IsSelected); } }

		//=============================================================================
		/// <summary>
		/// Sheet notes.
		/// </summary>
		private string m_Notes = string.Empty;
		public string Notes
		{
			get { return m_Notes; }
			set { m_Notes = value; }
		}

		//=============================================================================
		/// <summary>
		/// Returns true if this sheet contains racks which requires tie beams, but doesnt have it.
		/// </summary>
		public bool ContainsTieBeamsErrors
		{
			get
			{
				if(this.Rectangles != null)
				{
					foreach(BaseRectangleGeometry geom in this.Rectangles)
					{
						if (geom == null)
							continue;

						Rack rackGeom = geom as Rack;
						if (rackGeom == null)
							continue;

						if (rackGeom.StartFrameTieBeamError || rackGeom.EndFrameTieBeamError)
							return true;
					}
				}

				return false;
			}
		}

		//=============================================================================
		/// <summary>
		/// Display scale, which should be applied for display sheet geometry.
		/// 
		/// What does this number mean?
		/// It means how many Drawing Units is placed in 1 camera pixel.
		/// For example, m_UnitsPerCameraPixel = 12.5 means that 1 camera pixel displays 12.5 Drawing Units.
		/// </summary>
		private double m_UnitsPerCameraPixel = 1.0;
		public double UnitsPerCameraPixel
		{
			get { return m_UnitsPerCameraPixel; }
			set
			{
				if(Utils.FNE(m_UnitsPerCameraPixel, value))
				{
					m_UnitsPerCameraPixel = value;

					if (Utils.FLE(m_UnitsPerCameraPixel, 0.0))
						m_UnitsPerCameraPixel = 1.0;
					if (Utils.FGT(m_UnitsPerCameraPixel, m_MaxUnitsPerCameraPixel))
						m_UnitsPerCameraPixel = m_MaxUnitsPerCameraPixel;

					NotifyPropertyChanged(() => UnitsPerCameraPixel);
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// If true then m_UnitsPerCameraPixel is initialized.
		/// </summary>
		private bool m_Is_UnitsPerCameraPixel_Init = false;
		public bool Is_UnitsPerCameraPixel_Init
		{
			get { return m_Is_UnitsPerCameraPixel_Init; }
			set
			{
				if(value != m_Is_UnitsPerCameraPixel_Init)
				{
					m_Is_UnitsPerCameraPixel_Init = value;

					if (Utils.FGT(m_UnitsPerCameraPixel, m_MaxUnitsPerCameraPixel))
						m_UnitsPerCameraPixel = m_MaxUnitsPerCameraPixel;

					NotifyPropertyChanged(() => Is_UnitsPerCameraPixel_Init);
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// Maximum value for m_UnitsPerCameraPixel.
		/// If m_UnitsPerCameraPixel is equal to m_MaxUnitsPerCameraPixel then this sheet is
		/// fully displayed in the DrawingControl.
		/// </summary>
		private double m_MaxUnitsPerCameraPixel = 1.0;
		public double MaxUnitsPerCameraPixel
		{
			get { return m_MaxUnitsPerCameraPixel; }
			set
			{
				if(value != m_MaxUnitsPerCameraPixel)
				{
					m_MaxUnitsPerCameraPixel = value;

					NotifyPropertyChanged(() => MaxUnitsPerCameraPixel);
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// If true then this sheet is fully displayed in the DrawingControl.
		/// </summary>
		public bool IsSheetFullyDisplayed
		{
			get
			{
				if (!m_Is_UnitsPerCameraPixel_Init)
					return false;

				if (Utils.FGE(m_UnitsPerCameraPixel, m_MaxUnitsPerCameraPixel))
					return true;

				return false;
			}
		}

		//=============================================================================
		/// <summary>
		/// Camera offset in global coordinates from sheet top left point(0.0, 0.0).
		/// Camera position depends on m_CameraScale and m_CameraOffset.
		/// </summary>
		private Vector m_CameraOffset = new Vector(0.0, 0.0);
		public Vector CameraOffset
		{
			get { return m_CameraOffset; }
			set
			{
				if(Utils.FNE(m_CameraOffset.X, value.X) || Utils.FNE(m_CameraOffset.Y, value.Y))
				{
					m_CameraOffset = value;

					// Remove temporary camera offset
					m_TemporaryCameraOffset = new Vector(0.0, 0.0);

					NotifyPropertyChanged(() => CameraOffset);
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// Sheet camera temporary offset vector.
		/// It is used when user holds mouse wheel button and moves mouse.
		/// It is used by Sheet Minimap Control and DrawingControl for calculate camera position
		/// </summary>
		private Vector m_TemporaryCameraOffset = new Vector(0.0, 0.0);
		public Vector TemporaryCameraOffset
		{
			get { return m_TemporaryCameraOffset; }
			set
			{
				if(Utils.FNE(m_TemporaryCameraOffset.X, value.X) || Utils.FNE(m_TemporaryCameraOffset.Y, value.Y))
				{
					m_TemporaryCameraOffset = value;

					NotifyPropertyChanged(() => TemporaryCameraOffset);
				}
			}
		}

		#endregion

		#region IGeomDisplaySettings implementation

		// use default geometry display settings
		private IGeomDisplaySettings m_DefaultDisplaSettings = DefaultGeomDisplaySettings.GetInstance();

		//=============================================================================
		public double FillBrushOpacity
		{
			get
			{
				if (m_DefaultDisplaSettings != null)
					return m_DefaultDisplaSettings.FillBrushOpacity;

				return 1.0;
			}
		}

		//=============================================================================
		public bool DisplayText { get { return true; } }

		//=============================================================================
		public FontWeight TextWeight
		{
			get
			{
				if (m_DefaultDisplaSettings != null)
					return m_DefaultDisplaSettings.TextWeight;

				return FontWeights.Normal;
			}
		}

		//=============================================================================
		public double TextFontSize
		{
			get
			{
				if (m_DefaultDisplaSettings != null)
					return m_DefaultDisplaSettings.TextFontSize;

				return 14.0;
			}
			set
			{
				if (m_DefaultDisplaSettings != null)
					m_DefaultDisplaSettings.TextFontSize = value;
			}
		}

		//=============================================================================
		public Color GetTextColor(BaseRectangleGeometry geom)
		{
			if(geom != null)
			{
				// is geometry property source rack
				if (this.Document != null && this.Document.PropertySourceRack == geom)
				{
					if (CurrentGeometryColorsTheme.CurrentTheme != null)
					{
						Color propSourceRackTextColor;
						if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.ePropertySourceRackTextColor, out propSourceRackTextColor))
							return propSourceRackTextColor;
					}
					return Colors.DarkSlateGray;
				}
			}

			if (CurrentGeometryColorsTheme.CurrentTheme != null)
			{
				Color colorValue;
				if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eGeometryTextColor, out colorValue))
					return colorValue;
			}

			return Colors.Black;
		}

		//=============================================================================
		public Color GetFillColor(BaseRectangleGeometry geom)
		{
			if (geom == null)
				return Colors.White;

			// is geometry highlighted
			if (this.HiglightedRectangles != null && this.HiglightedRectangles.Contains(geom))
			{
				if (CurrentGeometryColorsTheme.CurrentTheme != null)
				{
					Color highlightColor;
					if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eFillHighlightGeometry, out highlightColor))
						return highlightColor;
				}
				return Colors.Red;
			}

			// is geometry property source rack
			if (this.Document != null && this.Document.PropertySourceRack == geom)
			{
				if(CurrentGeometryColorsTheme.CurrentTheme != null)
				{
					Color propSourceRackFillColor;
					if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.ePropertySourceRackFill, out propSourceRackFillColor))
						return propSourceRackFillColor;
				}
				return Colors.DarkSlateGray;
			}

			// is geometry in the multiselection
			if (this.SelectedGeometryCollection != null && this.SelectedGeometryCollection.Count > 1 && this.SelectedGeometryCollection.Contains(geom))
			{
				if (CurrentGeometryColorsTheme.CurrentTheme != null)
				{
					Color multiselectionColor;
					if (CurrentGeometryColorsTheme.CurrentTheme.GetGeometryColor(eColorType.eFillMultiselection, out multiselectionColor))
						return multiselectionColor;
				}
				return Colors.LightPink;
			}

			// return default fill color
			if (m_DefaultDisplaSettings != null)
				return m_DefaultDisplaSettings.GetFillColor(geom);

			return Colors.White;
		}

		#endregion

		#region Functions

		//=============================================================================
		// Create geometry functions.
		public void CreateBlock()
		{
			Cancel();

			if (DrawingDocument._sDrawing == null)
				return;

			Block block = new Block(this);
			_AfterCreateNewGeom(block);
		}
		//
		public void CreateAisleSpace()
		{
			Cancel();

			if (DrawingDocument._sDrawing == null)
				return;

			AisleSpace aisleSpace = new AisleSpace(this);
			_AfterCreateNewGeom(aisleSpace);
		}
		//
		public void CreateShutter()
		{
			Cancel();

			if (DrawingDocument._sDrawing == null)
				return;

			Shutter shutter = new Shutter(this);
			_AfterCreateNewGeom(shutter);
		}
		//
		public void CreateRack()
		{
			Cancel();

			if (DrawingDocument._sDrawing == null)
				return;

			Rack rack = new Rack(this, false);
			_AfterCreateNewGeom(rack);
			rack.CheckRackHeight();

			if (DrawingDocument._sDrawing != null)
				DrawingDocument._sDrawing.UpdateDrawing(true);
		}
		//
		public void CreateColumn()
		{
			Cancel();

			if (DrawingDocument._sDrawing == null)
				return;

			// init
			ColumnPattern pattern = _CreateColumnPattern();

			Column column = new Column(this);
			column.ColumnPattern_ID = pattern.ID;
			//
			_AfterCreateNewGeom(column);
		}
		//
		public void CreateWall()
		{
			Cancel();

			if (DrawingDocument._sDrawing == null)
				return;

			// If user creates the wall then need to display entire sheet.
			// Remove camera scale and camera offset.
			// Probably current camera position doesnt display new wall.
			this.FullyDisplaySheet();

			Wall newWall = new Wall(this);
			newWall.IsInit = false;
			newWall.Sheet = this;
			AddGeometry(newWall);

			if (Document != null)
				Document.IsInCommand = true;

			DrawingDocument._sDrawing.CreateWall();
		}


		//=============================================================================
		/// <summary>
		/// Add BaseRectangleGemetry to the Rectangles collection.
		/// </summary>
		public void AddGeometry(List<BaseRectangleGeometry> geometryToAdd)
		{
			if (geometryToAdd == null)
				return;

			foreach (BaseRectangleGeometry geom in geometryToAdd)
			{
				if (geom != null)
					AddGeometry(geom);
			}
		}
		public void AddGeometry(BaseRectangleGeometry geom)
		{
			if (geom == null)
				return;

			try
			{
				// Shutters should be on the top of walls, so add them to the end of m_Children.
				//
				// Column should be on the top of block and aisle place.
				// If wrapper is a column - place it to the end, otherwise - place it before the first column.
				// All columns should be in the end of m_Children.
				if (!(geom is Column && geom is Shutter))
				{
					int indexOfFirstColumn = Rectangles.FindIndex(g => g != null && (g is Column || g is Shutter));
					if (indexOfFirstColumn >= 0)
						Rectangles.Insert(indexOfFirstColumn, geom);
					else
						Rectangles.Add(geom);
				}
				else
					Rectangles.Add(geom);

				if (geom is Rack)
					this._MarkCalculationStateChanged(eCalculationState.eForceRecalculateRacksGroups);
				geom.Sheet = this;
			}
			catch { }
		}



		//=============================================================================
		/// <summary>
		/// Copy geometry in selection.
		/// </summary>
		public void CopySelectedGeometry()
		{
			if (Document == null)
				return;

			List<BaseRectangleGeometry> copyList = new List<BaseRectangleGeometry>();
			// If user wants to copy-paste columns then need to find columns with the same pattern id, copy-paste them
			// and create another pattern for them. Because columns with the same pattern should have the same margin between them.
			// It is not possible to save the margin between original and new columns.
			//
			// old column pattern id - new column pattern
			Dictionary<int, ColumnPattern> columnsPatternsDict = new Dictionary<int, ColumnPattern>();
			foreach(BaseRectangleGeometry geom in SelectedGeometryCollection)
			{
				if (geom == null)
					continue;

				// Dont copy-paste SheetElevationGeometry, only one instance of this geometry can be placed at the drawing
				if (geom is SheetElevationGeometry)
					continue;

				// dont copy-paste shutters and walls
				if (geom is Shutter || geom is Wall)
					continue;

				BaseRectangleGeometry cloneGeom = geom.Clone() as BaseRectangleGeometry;
				if (cloneGeom == null)
					continue;

				// Bind new columns to new columns patterns.
				Column clonedColumn = cloneGeom as Column;
				if (clonedColumn != null)
				{
					ColumnPattern newPattern = null;
					if (columnsPatternsDict.ContainsKey(clonedColumn.ColumnPattern_ID))
						newPattern = columnsPatternsDict[clonedColumn.ColumnPattern_ID];

					if (newPattern == null)
					{
						Column originalColumn = geom as Column;
						if (originalColumn != null && originalColumn.Pattern != null)
						{
							newPattern = originalColumn.Pattern.Clone() as ColumnPattern;
							if (newPattern != null)
							{
								newPattern.ID = _GetFreePatternID();
								newPattern.Sheet = this;
								m_patterns.Add(newPattern);
							}
						}
						
						if(newPattern == null)
							newPattern = _CreateColumnPattern();

						if (newPattern != null)
							columnsPatternsDict[clonedColumn.ColumnPattern_ID] = newPattern;
					}

					if (newPattern == null)
						continue;

					clonedColumn.ColumnPattern_ID = newPattern.ID;
				}

				// Otherwise RackUtils.Convert_A_to_M_Rack() works incorrect, because DiffBetween_M_and_A returns 0 as column length.
				// DiffBetween_M_and_A depends on the Sheet.
				cloneGeom.Sheet = this;
				copyList.Add(cloneGeom);
			}

			Document.CopiedGeomList = copyList;

			// group racks in row\columns
			List<Rack> racksList = new List<Rack>();
			foreach(BaseRectangleGeometry geom in Document.CopiedGeomList)
			{
				if (geom == null)
					continue;

				Rack rack = geom as Rack;
				if (rack == null)
					continue;

				// Otherwise RackUtils.Convert_A_to_M_Rack() works incorrect, because DiffBetween_M_and_A returns 0 as column length.
				// DiffBetween_M_and_A depends on the Sheet.
				rack.Sheet = this;
				racksList.Add(rack);
			}
			// convert A-racks to M-racks and vice versa
			if(racksList.Count > 0)
			{
				List<List<Rack>> racksGroups = new List<List<Rack>>();
				if(_GroupRacks(racksList, out racksGroups))
				{
					foreach(List<Rack> rackGroup in racksGroups)
					{
						if (rackGroup == null)
							continue;

						if (rackGroup.Count == 0)
							continue;

						foreach(Rack rack in rackGroup)
						{
							if (rack == null)
								continue;

							int index = rackGroup.IndexOf(rack);
							if(index == 0)
							{
								if (!rack.IsFirstInRowColumn)
								{
									RackUtils.Convert_A_to_M_Rack(this, rack, true, false);
									// move all next racks
									foreach(Rack nextRack in  rackGroup)
									{
										if (nextRack == null)
											continue;

										int nextRackIndex = rackGroup.IndexOf(nextRack);
										if (nextRackIndex <= index)
											continue;

										// move top left point
										Point newTopLeftPnt = nextRack.TopLeft_GlobalPoint;
										if (nextRack.IsHorizontal)
											newTopLeftPnt.X += rack.DiffBetween_M_and_A;
										else
											newTopLeftPnt.Y += rack.DiffBetween_M_and_A;
										nextRack.TopLeft_GlobalPoint = newTopLeftPnt;
									}
								}
							}
							else
							{
								if (rack.IsFirstInRowColumn)
								{
									RackUtils.Convert_A_to_M_Rack(this, rack, false, false);
									// move all next racks
									foreach (Rack nextRack in rackGroup)
									{
										if (nextRack == null)
											continue;

										int nextRackIndex = rackGroup.IndexOf(nextRack);
										if (nextRackIndex <= index)
											continue;

										// move top left point
										Point newTopLeftPnt = nextRack.TopLeft_GlobalPoint;
										if (nextRack.IsHorizontal)
											newTopLeftPnt.X -= rack.DiffBetween_M_and_A;
										else
											newTopLeftPnt.Y -= rack.DiffBetween_M_and_A;
										nextRack.TopLeft_GlobalPoint = newTopLeftPnt;
									}
								}
							}
						}
					}
				}
			}

			Cancel();
		}
		//=============================================================================
		/// <summary>
		/// Paste geometry from DrawingDocument.CopiedGeomList.
		/// </summary>
		public void PasteGeometry()
		{
			if (DrawingDocument._sDrawing == null)
				return;

			if (Document == null)
				return;

			//
			List<BaseRectangleGeometry> nonInitSelectionGeom = this.NonInitSelectedGeometryList;
			if (nonInitSelectionGeom.Count > 0)
				return;
			if (Document.CopiedGeomList == null)
				return;

			List<BaseRectangleGeometry> geomCopyList = new List<BaseRectangleGeometry>();
			List<ColumnPattern> newPatternsList = new List<ColumnPattern>();
			foreach(BaseRectangleGeometry geom in Document.CopiedGeomList)
			{
				if (geom == null)
					continue;

				// Dont copy-paste SheetElevationGeometry, only one instance of this geometry can be placed at the drawing
				if (geom is SheetElevationGeometry)
					continue;

				// dont copy-paste shutters
				if (geom is Shutter)
					continue;

				BaseRectangleGeometry geomCopy = geom.Clone() as BaseRectangleGeometry;
				if (geomCopy == null)
					continue;

				// Sheet is required for calculate Margins for the Rack geometry.
				geomCopy.Sheet = this;
				geomCopyList.Add(geomCopy);

				Column columnGeom = geomCopy as Column;
				if (columnGeom != null && columnGeom.Pattern != null)
					newPatternsList.Add(columnGeom.Pattern);
			}

			// Check borders.
			// Calculate pasted geometry bound box.
			double leftX = 0.0;
			double rightX = 0.0;
			double topY = 0.0;
			double botY = 0.0;
			bool isInitLeftX = false;
			bool isInitRightX = false;
			bool isInitTopY = false;
			bool isInitBotY = false;
			foreach (BaseRectangleGeometry geom in geomCopyList)
			{
				if (geom == null)
					continue;

				double geomLeftX = geom.TopLeft_GlobalPoint.X - geom.MarginX;
				double geomRightX = geom.BottomRight_GlobalPoint.X + geom.MarginX;
				double geomTopY = geom.TopLeft_GlobalPoint.Y - geom.MarginY;
				double geomBotY = geom.BottomRight_GlobalPoint.Y + geom.MarginY;

				if (!isInitLeftX)
				{
					leftX = geomLeftX;
					isInitLeftX = true;
				}
				else if (Utils.FLT(geomLeftX, leftX))
					leftX = geomLeftX;

				if (!isInitRightX)
				{
					rightX = geomRightX;
					isInitRightX = true;
				}
				else if (Utils.FGT(geomRightX, rightX))
					rightX = geomRightX;

				if (!isInitTopY)
				{
					topY = geomTopY;
					isInitTopY = true;
				}
				else if (Utils.FLT(geomTopY, topY))
					topY = geomTopY;

				if (!isInitBotY)
				{
					botY = geomBotY;
					isInitBotY = true;
				}
				else if (Utils.FGT(geomBotY, botY))
					botY = geomBotY;
			}

			// Try to paste geometry in current camera position.
			Vector pastedGeometryOffsetVector = new Vector(0.0, 0.0);
			if (isInitLeftX && isInitRightX && isInitTopY && isInitBotY)
			{
				double boundBoxLength = rightX - leftX;
				double boundBoxHeight = botY - topY;

				Point cameraTopLeftGlobalPnt = DrawingDocument._sDrawing.GetGlobalPoint(this, new Point(0.0, 0.0));
				Point cameraBotRightGlobalPnt = DrawingDocument._sDrawing.GetGlobalPoint(this, new Point(DrawingDocument._sDrawing.ActualWidth, DrawingDocument._sDrawing.ActualHeight));
				double cameraGlobalLength = cameraBotRightGlobalPnt.X - cameraTopLeftGlobalPnt.X;
				double cameraGlobalHeight = cameraBotRightGlobalPnt.Y - cameraTopLeftGlobalPnt.Y;

				// If pasted geometry doesnt fit in this sheet, then display error message.
				if(Utils.FGT(boundBoxLength, this.Length) || Utils.FGT(boundBoxHeight, this.Width))
				{
					this.Document.DocumentError = "Error. Pasted geometry bound box is greater than sheet size. Paste geometry command is canceled.";
					return;
				}

				// If pasted geometry doesnt fit in the camera area then remove all zoom and fully display sheet.
				if (!IsSheetFullyDisplayed)
				{
					if (Utils.FGT(boundBoxLength, cameraGlobalLength) || Utils.FGT(boundBoxHeight, cameraGlobalHeight))
						this.FullyDisplaySheet();
					else
					{
						// Otherwise, move pasted geometry to the center of camera.
						pastedGeometryOffsetVector = (cameraTopLeftGlobalPnt - new Point(0.0, 0.0)) + (cameraBotRightGlobalPnt - cameraTopLeftGlobalPnt) / 2;
						pastedGeometryOffsetVector.X -= leftX + boundBoxLength / 2;
						pastedGeometryOffsetVector.Y -= topY + boundBoxHeight / 2;
						pastedGeometryOffsetVector.X = Utils.GetWholeNumber(pastedGeometryOffsetVector.X);
						pastedGeometryOffsetVector.Y = Utils.GetWholeNumber(pastedGeometryOffsetVector.Y);
					}
				}

				// Check that pasted geometry position is inside sheet.
				// For example, geometry top left point can have value outside sheet.
				if (Utils.FGT(rightX, this.Length))
					pastedGeometryOffsetVector.X -= rightX - this.Length;
				if (Utils.FGT(botY, this.Width))
					pastedGeometryOffsetVector.Y -= botY - this.Width;
			}

			SelectedGeometryCollection.Clear();
			foreach(BaseRectangleGeometry geom in geomCopyList)
			{
				if (geom == null)
					continue;

				// mark new geometry not inited
				geom.IsInit = false;
				AddGeometry(geom);

				geom.TopLeft_GlobalPoint += pastedGeometryOffsetVector;
				SelectedGeometryCollection.Add(geom);
			}

			//
			foreach (ColumnPattern pattern in newPatternsList)
			{
				if (pattern == null)
					continue;
				pattern.Update();
			}

			Document.IsInCommand = true;
			if(geomCopyList.Count > 1)
				DrawingDocument._sDrawing.MoveSelection();
			DrawingDocument._sDrawing.UpdateDrawing(true);
		}
		//=============================================================================
		/// <summary>
		/// Delete selected geometry
		/// </summary>
		public void DeleteGeometry(List<BaseRectangleGeometry> geomForDeleteList, bool bMarkStateChanged, bool bUpdateDrawing)
		{
			if (geomForDeleteList == null)
				return;

			//
			foreach (BaseRectangleGeometry geom in geomForDeleteList)
			{
				if (geom == null)
					continue;

				// Dont delete SheetElevationGeometry, only one instance of this geometry can be placed at the drawing
				if (geom is SheetElevationGeometry)
					continue;

				//
				ColumnPattern pattern = null;

				//
				Column columnForDelete = geom as Column;
				if (columnForDelete != null)
					pattern = columnForDelete.Pattern;

				// change size indexes
				if (columnForDelete != null)
					Document.RemoveColumnUniqueSize(columnForDelete);
				//
				Rack rackForDelete = geom as Rack;
				if (rackForDelete != null)
				{
					Document.RemoveRackUniqueSize(rackForDelete);
					this._MarkCalculationStateChanged(eCalculationState.eForceRecalculateRacksGroups);
				}

				//
				Rectangles.Remove(geom);

				//
				if (pattern != null)
				{
					bool bDeletePattern = true;

					// check columns count in the pattern
					Point patternStartPoint;
					int columnsCount;
					int rowsCount;
					List<Column> patternColumnsList;
					List<List<Column>> patternRows;
					if (pattern.GetPatternInfo(out patternStartPoint, out columnsCount, out rowsCount, out patternColumnsList, out patternRows))
					{
						if (patternColumnsList.Count > 0)
						{
							bDeletePattern = false;
							// update columns in the pattern
							pattern.Update();
						}
					}

					if (bDeletePattern)
						m_patterns.Remove(pattern);
				}
			}

			if (bMarkStateChanged)
			{
				Cancel();
				MarkStateChanged();
			}

			if(bUpdateDrawing && DrawingDocument._sDrawing != null)
				DrawingDocument._sDrawing.UpdateDrawing(true);
		}
		//=============================================================================
		public void DeleteNonInitializedGeometry(bool bMarkStateChanged, bool bCallRedraw)
		{
			// delete last not initialized
			List<BaseRectangleGeometry> nonInitGeomList = new List<BaseRectangleGeometry>();
			foreach (BaseRectangleGeometry geom in this.Rectangles)
			{
				if (geom == null)
					continue;

				// Dont delete SheetElevationGeometry, only one instance of this geometry can be placed at the drawing
				if (geom is SheetElevationGeometry)
					continue;

				if (!geom.IsInit)
					nonInitGeomList.Add(geom);
			}
			//
			foreach (BaseRectangleGeometry geom in nonInitGeomList)
			{
				if (geom == null)
					continue;

				// Dont delete SheetElevationGeometry, only one instance of this geometry can be placed at the drawing
				if (geom is SheetElevationGeometry)
					continue;

				if (geom is Rack)
					this._MarkCalculationStateChanged(eCalculationState.eForceRecalculateRacksGroups);

				Rectangles.Remove(geom);
			}

			if (bMarkStateChanged)
				MarkStateChanged();

			if (bCallRedraw && DrawingDocument._sDrawing != null)
				DrawingDocument._sDrawing.UpdateDrawing(true);
		}
		//=============================================================================
		public void Cancel()
		{
			DeleteNonInitializedGeometry(false, false);
			this.SelectedGeometryCollection.Clear();

			//
			if (Document != null)
				Document.IsInCommand = false;

			//
			if (DrawingDocument._sDrawing != null)
				DrawingDocument._sDrawing.UpdateDrawing(true);
		}
		//=============================================================================
		public void AfterSave()
		{
			// Remove GUID from set with changed sheets guids.
			if (m_ChangedSheetsGuidsSet != null && m_ChangedSheetsGuidsSet.Contains(this.GUID))
				m_ChangedSheetsGuidsSet.Remove(this.GUID);

			Cancel();

			//
			IsNewSheet = false;
			NotifyPropertyChanged(() => HasChanges);
			NotifyPropertyChanged(() => DisplayName);
		}
		//=============================================================================
		public void Show(bool bShow)
		{
			if (!bShow)
			{
				// revert any uncompleted changes
				Cancel();
			}

			if (DrawingDocument._sDrawing != null)
				DrawingDocument._sDrawing.UpdateDrawing(true);
		}



		//=============================================================================
		/// <summary>
		/// Add racks from TempRackRow (temporary geometry list) to Rectangles collection.
		/// </summary>
		public void AddTempRacks()
		{
			if (Document == null)
				return;

			List<Rack> tempRacksList = new List<Rack>();
			foreach (Rack rack in TemporaryRacksList)
			{
				if (rack == null)
					continue;

				tempRacksList.Add(rack);
			}

			if (tempRacksList.Count == 0)
				return;

			// add racks
			List<BaseRectangleGeometry> tempGeomList = new List<BaseRectangleGeometry>(tempRacksList);
			AddGeometry(tempGeomList);
			// 
			foreach (Rack rack in tempRacksList)
			{
				if (rack == null)
					continue;

				rack.CheckRackHeight();
				rack.SizeIndex = Document.AddRackUniqueSize(rack);
			}

			TemporaryRacksList.Clear();
		}
		//=============================================================================
		/// <summary>
		/// Returns rack group(row or column) from RacksGroups collection which contains this rack.
		/// </summary>
		public List<Rack> GetRackGroup(Rack rack)
		{
			List<Rack> result = new List<Rack>();

			if (rack != null)
			{
				foreach (List<Rack> row in RacksGroups)
				{
					if (row.Contains(rack))
					{
						result = row;
						break;
					}
				}
			}

			return result;
		}
		//=============================================================================
		/// <summary>
		/// Returns all racks from Rectangles list.
		/// </summary>
		/// <returns></returns>
		public List<Rack> GetAllRacks()
		{
			List<Rack> result = new List<Rack>();

			// get all racks
			foreach (BaseRectangleGeometry geom in Rectangles)
			{
				Rack rack = geom as Rack;
				if (rack != null)
					result.Add(rack);
			}

			return result;
		}


		//=============================================================================
		public void CreateColumnPattern(Column original, Point globalPoint, double DrawingWidth, double DrawingHeight)
		{
			if (DrawingDocument._sDrawing == null)
				return;

			if (original == null)
				return;

			//
			if (globalPoint.X < 0 || globalPoint.Y < 0)
				return;

			//
			if (globalPoint.X > this.Length || globalPoint.Y > this.Width)
				return;

			//
			ColumnPattern pattern = original.Pattern;
			if (pattern == null)
				return;

			// distances between the centers of columns
			double offset_X = pattern.GlobalOffset_X;
			double offset_Y = pattern.GlobalOffset_Y;

			try
			{
				// add or remove in horizontal direction
				Point patternStartPoint;
				int columnsCount;
				int rowsCount;
				List<Column> patternColumnsList;
				List<List<Column>> patternRows;
				if (!pattern.GetPatternInfo(out patternStartPoint, out columnsCount, out rowsCount, out patternColumnsList, out patternRows))
					return;

				//
				int horisExistedCount = columnsCount;
				int vertExistedCount = rowsCount;

				// "Original" column can be not placed at the right corner. For example, if user delete the bot right column in the pattern.
				//double rDist_X = globalPoint.X - original.Center_GlobalPoint.X;
				double rDist_X = globalPoint.X - (patternStartPoint.X + offset_X * (columnsCount - 1));
				// only right direction
				if (rDist_X < 0)
					rDist_X = 0;

				// "Original" column can be not placed at the right corner. For example, if user delete the bot right column in the pattern.
				//double rDist_Y = globalPoint.Y - original.Center_GlobalPoint.Y;
				double rDist_Y = globalPoint.Y - (patternStartPoint.Y + offset_Y * (rowsCount - 1));
				// only bottom direction
				if (rDist_Y < 0)
					rDist_Y = 0;

				//
				int newHorizColumnsCount = Utils.GetWholeNumber(rDist_X / pattern.GlobalOffset_X) + horisExistedCount;
				int newVertColumnsCount = Utils.GetWholeNumber(rDist_Y / pattern.GlobalOffset_Y) + vertExistedCount;

				// add existing rows
				for (int i = TemporaryColumnsInPattern.Count; i < vertExistedCount; ++i)
					TemporaryColumnsInPattern.Add(new List<BaseRectangleGeometry>());

				//
				int iRowLength = TemporaryColumnsInPattern[0].Count;

				// check horiz
				bool bIncorrectLayout = false;
				if (iRowLength + horisExistedCount < newHorizColumnsCount)
				{
					foreach (List<BaseRectangleGeometry> row in TemporaryColumnsInPattern)
					{
						int iRowIndex = TemporaryColumnsInPattern.IndexOf(row);
						//
						for (int i = iRowLength + horisExistedCount; i < newHorizColumnsCount; ++i)
						{
							Column newTempColumn = null;
							if (i >= horisExistedCount)
							{
								newTempColumn = original.Clone() as Column;
								if (newTempColumn == null)
									continue;
								newTempColumn.Sheet = this;
								newTempColumn.ColumnPattern_ID = pattern.ID;

								Point topLeftPoint = patternStartPoint;
								topLeftPoint.X += pattern.GlobalOffset_X * i;
								topLeftPoint.X -= newTempColumn.Length_X / 2;
								topLeftPoint.Y += pattern.GlobalOffset_Y * iRowIndex;
								topLeftPoint.Y -= newTempColumn.Length_Y / 2;
								//
								topLeftPoint = Utils.GetWholePoint(topLeftPoint);
								//
								newTempColumn.TopLeft_GlobalPoint = topLeftPoint;

								// check borders & layout
								if (newTempColumn.TopLeft_GlobalPoint.X < 0
									|| newTempColumn.TopLeft_GlobalPoint.Y < 0
									|| newTempColumn.BottomRight_GlobalPoint.X > DrawingWidth
									|| newTempColumn.BottomRight_GlobalPoint.Y > DrawingHeight
									|| !this.IsLayoutCorrect(newTempColumn))
								{
									bIncorrectLayout = true;
									iRowLength = newHorizColumnsCount;
									newHorizColumnsCount = i;
									break;
								}
							}

							//
							if (newTempColumn != null)
								row.Add(newTempColumn);
						}

						if (bIncorrectLayout)
							break;
					}
				}

				if (iRowLength + horisExistedCount > newHorizColumnsCount)
				{
					foreach (List<BaseRectangleGeometry> row in TemporaryColumnsInPattern)
					{
						int iRowIndex = TemporaryColumnsInPattern.IndexOf(row);

						int iAdditionalValue = 0;
						if (iRowIndex < vertExistedCount)
							iAdditionalValue = horisExistedCount;

						row.RemoveRange(newHorizColumnsCount - iAdditionalValue, row.Count + iAdditionalValue - newHorizColumnsCount);
					}
				}

				// check vert
				bIncorrectLayout = false;
				if (TemporaryColumnsInPattern.Count < newVertColumnsCount)
				{
					for (int iRowIndex = TemporaryColumnsInPattern.Count; iRowIndex < newVertColumnsCount; ++iRowIndex)
					{
						List<BaseRectangleGeometry> newRow = new List<BaseRectangleGeometry>();
						//
						for (int i = 0; i < newHorizColumnsCount; ++i)
						{
							Column newTempColumn = original.Clone() as Column;
							if (newTempColumn == null)
								continue;
							newTempColumn.Sheet = this;
							newTempColumn.ColumnPattern_ID = pattern.ID;

							Point topLeftPoint = patternStartPoint;
							topLeftPoint.X += pattern.GlobalOffset_X * i;
							topLeftPoint.Y += pattern.GlobalOffset_Y * iRowIndex;
							topLeftPoint.X -= newTempColumn.Length_X / 2;
							topLeftPoint.Y -= newTempColumn.Length_Y / 2;
							//
							topLeftPoint = Utils.GetWholePoint(topLeftPoint);
							//
							newTempColumn.TopLeft_GlobalPoint = topLeftPoint;

							// check borders & layout
							if (newTempColumn.TopLeft_GlobalPoint.X < 0
								|| newTempColumn.TopLeft_GlobalPoint.Y < 0
								|| newTempColumn.BottomRight_GlobalPoint.X > DrawingWidth
								|| newTempColumn.BottomRight_GlobalPoint.Y > DrawingHeight
								|| !this.IsLayoutCorrect(newTempColumn))
							{
								bIncorrectLayout = true;
								break;
							}

							if (newTempColumn != null)
								newRow.Add(newTempColumn);
						}

						//
						if (bIncorrectLayout)
							break;

						//
						TemporaryColumnsInPattern.Add(newRow);
					}
				}
				else if (TemporaryColumnsInPattern.Count > newVertColumnsCount)
				{
					List<List<BaseRectangleGeometry>> rowsForDelete = new List<List<BaseRectangleGeometry>>();
					for (int i = newVertColumnsCount; i < TemporaryColumnsInPattern.Count; ++i)
					{
						List<BaseRectangleGeometry> _row = TemporaryColumnsInPattern[i];
						rowsForDelete.Add(_row);
					}

					foreach (List<BaseRectangleGeometry> _row in rowsForDelete)
						TemporaryColumnsInPattern.Remove(_row);
				}
			}
			catch { }

			if (DrawingDocument._sDrawing != null)
				DrawingDocument._sDrawing.UpdateDrawing(true);
		}
		//=============================================================================
		public void AddTempColumns()
		{
			if (Document == null)
				return;

			if (TemporaryColumnsInPattern.Count > 0)
			{
				Column _firstColumn = null;

				foreach (List<BaseRectangleGeometry> row in TemporaryColumnsInPattern)
				{
					foreach (BaseRectangleGeometry geom in row)
					{
						if (geom != null)
						{
							if (_firstColumn == null)
								_firstColumn = geom as Column;

							AddGeometry(geom);

							Column _c = geom as Column;
							if (_c != null)
								Document.AddColumnUniqueSize(_c);
						}
					}
				}

				// update column pattern
				if (_firstColumn != null)
				{
					if (_firstColumn.Pattern != null)
						_firstColumn.Pattern.Update();
				}

				//
				TemporaryColumnsInPattern.Clear();
			}
		}
		//=============================================================================
		public List<Column> GetAllColumns()
		{
			List<Column> result = new List<Column>();

			// get all columns
			foreach (BaseRectangleGeometry geom in Rectangles)
			{
				Column column = geom as Column;
				if (column != null)
					result.Add(column);
			}

			return result;
		}
		//=============================================================================
		public ColumnPattern GetColumnPattern(int iColumnPatternID)
		{
			if (iColumnPatternID < 0)
				return null;

			foreach(ColumnPattern _pattern in m_patterns)
			{
				if (_pattern == null)
					continue;

				if (_pattern.ID == iColumnPatternID)
					return _pattern;
			}

			return null;
		}
		//=============================================================================
		/// <summary>
		/// Updates all columns SizeIndex property.
		/// </summary>
		public void UpdateAllColumnsIndex()
		{
			if (Document == null)
				return;

			List<Column> allColumns = this.GetAllColumns();
			foreach (Column columnGeom in allColumns)
			{
				if (columnGeom == null)
					continue;

				columnGeom.SizeIndex = Document.GetColumnUniqueSizeIndex(columnGeom);
			}
		}


		//=============================================================================
		/// <summary>
		/// Copy properties from propertySource to racks in the selection
		/// </summary>
		public async void RackMatchProperties(Rack propertySource, List<BaseRectangleGeometry> geometryList)
		{
			if (this.Document == null)
				return;

			if (this.Document.DisplayDialog == null)
				return;

			if (propertySource == null)
				return;

			if (geometryList == null)
				return;

			// get all racks from the selection
			List<Rack> racksList = new List<Rack>();
			foreach(BaseRectangleGeometry geom in geometryList)
			{
				if (geom == null)
					continue;

				Rack rack = geom as Rack;
				if (rack != null)
					racksList.Add(rack);
			}

			// group racks in rows\columns
			List<List<Rack>> racksLocalGroups = new List<List<Rack>>();
			if (!_GroupRacks(racksList, out racksLocalGroups))
				return;

			//
			M_RackState propertySourceState = propertySource.Get_MRackState();
			List<Rack> deletedRacksList = new List<Rack>();
			// List with target racks from racksList for which match properties command is not possible.
			List<Rack> notPossibleMatchPropsList = new List<Rack>();
			//
			foreach(List<Rack> localGroup in racksLocalGroups)
			{
				if (localGroup == null)
					continue;

				bool bPrevRackWasSkipped = false;
				bool bCancel = false;
				foreach (Rack rack in localGroup)
				{
					if (rack == null)
						continue;

					if (deletedRacksList.Contains(rack))
						break;

					// if propertySource rack and current rack has the same state then do nothing
					M_RackState rackState = rack.Get_MRackState();
					if (rackState == propertySourceState)
						continue;

					// if rack and propertySource has different depth then display warning
					bool bDiffDepth = false;
					if (Utils.FNE(rack.Depth, propertySource.Depth))
					{
						bDiffDepth = true;

						if (!DrawingControl.sGeomMathPropertiesCommand_RememberTheChoice)
						{
							IYesNoCancelViewModel vm = null;
							// true - yes
							// false - no
							// null - cancel
							object result = await this.Document.DisplayDialog.YesNoCancelDialog("Property source rack and \"" + rack.Text + "\" rack from selection has different depth. Are you sure?", out vm);
							if (result is bool)
							{
								DrawingControl.sGeomMatchPropertiesCommand_MatchIfDepthDiff = (bool)result;
								if (vm != null)
									DrawingControl.sGeomMathPropertiesCommand_RememberTheChoice = vm.RememberTheChoice;
							}
							else if (result == null)
							{
								// null is cancel
								bCancel = true;
							}
						}
					}

					// get global group
					List<Rack> group = GetRackGroup(rack);
					int globalIndex = group.IndexOf(rack);
					int localIndex = localGroup.IndexOf(rack);

					// instead copy all properties from propertySource rack to current rack
					// lets clone propertySource and replace current rack
					Rack propSourceClone = null;
					bool propertySourceRackWasCloned = false;
					// rack was converted to M or A
					bool rackWasConverted = false;
					if (bDiffDepth && (!DrawingControl.sGeomMatchPropertiesCommand_MatchIfDepthDiff || (bCancel && localIndex > 0)))
					{
						// convert current rack to M
						propSourceClone = rack.Clone() as Rack;
						if (propSourceClone == null)
							return;
						if (globalIndex == 0 && !rack.IsFirstInRowColumn)
						{
							rackWasConverted = true;
							RackUtils.Convert_A_to_M_Rack(this, propSourceClone, true, false);
						}
					}
					else
					{
						propertySourceRackWasCloned = true;

						propSourceClone = propertySource.Clone() as Rack;
						if (propSourceClone == null)
							return;
						bool bPropSourceShouldBeM = (bDiffDepth && (localIndex == 0 || !DrawingControl.sGeomMatchPropertiesCommand_MatchIfDepthDiff || bPrevRackWasSkipped)) || (!bDiffDepth && localIndex == 0 && globalIndex == 0); // || group.IndexOf(rack) == 0
						if (propSourceClone.IsFirstInRowColumn != bPropSourceShouldBeM)
						{
							rackWasConverted = true;
							RackUtils.Convert_A_to_M_Rack(this, propSourceClone, bPropSourceShouldBeM, false);
						}
					}
					propSourceClone.TopLeft_GlobalPoint = rack.TopLeft_GlobalPoint;
					// Without sheet ClearAvailableHeight(which drives max height) is calculated without roof properties and rack position.
					propSourceClone.Sheet = this;
					// need to rotate cloned rack
					if (propSourceClone.IsHorizontal != rack.IsHorizontal)
						propSourceClone.Rotate(this.Length, this.Width, bCheckLayout: false);

					// Rack's height depends on the roof properties and rack postition\sizes.
					//
					// 1. If propertySource is cloned then need to check max available height and skip match if cloned rack height is higher than available height.
					//    Because when user clicks rack match properties command he wants to receive the same rack.
					// 2. If rack is skipped and converted to the M-rack(it will increase length) then need to check max available height
					//    and change it if it is not correct. There is no choise, because result layout should be correct.
					if (propertySourceRackWasCloned)
					{
						string strError;
						if(!propSourceClone._RecalcRackHeight(out strError))
						{
							notPossibleMatchPropsList.Add(rack);

							// Rack height is greater than available height.
							// Use original rack, probably convert it to M.
							propSourceClone = rack.Clone() as Rack;
							if (propSourceClone == null)
								return;
							if (globalIndex == 0 && !rack.IsFirstInRowColumn)
							{
								rackWasConverted = true;
								RackUtils.Convert_A_to_M_Rack(this, propSourceClone, true, false);
							}
							propSourceClone.TopLeft_GlobalPoint = rack.TopLeft_GlobalPoint;
							// Without sheet ClearAvailableHeight(which drives max height) is calculated without roof properties and rack position.
							propSourceClone.Sheet = this;
							propSourceClone.CheckRackHeight();
						}
					}
					else
					{
						if(rackWasConverted)
							propSourceClone.CheckRackHeight();
					}

					List<Rack> _previewGroup = new List<Rack>();
					RackColumn minColumn = null;
					if (!RackUtils.TryToReplaceRackInGroup(this, propSourceClone, globalIndex, true, group, out _previewGroup, out deletedRacksList, out minColumn) || minColumn == null)
					{
						this.Document.DocumentError = "Result layout is not correct.";
						return;
					}

					// add propertySource clone to DrawingControl if it was not deleted
					if (_previewGroup.Contains(propSourceClone))
					{
						this.AddGeometry(propSourceClone);
						// Get size index after propSourceClone.RecalculateColumn().
						// Otherwise, it can be incorrect here.
						propSourceClone.SizeIndex = -1;//Document.AddRackUniqueSize(propSourceClone);
					}

					bPrevRackWasSkipped = bDiffDepth && !DrawingControl.sGeomMatchPropertiesCommand_MatchIfDepthDiff;
					//
					this.RegroupRacks();
					//
					string strColumnError;
					if (!propSourceClone.RecalculateColumn(true, out strColumnError))
					{
						// undo
						this.Document.SetTheLastState();
						return;
					}
					//
					propSourceClone.SizeIndex = Document.AddRackUniqueSize(propSourceClone);
					//
					this.RegroupRacks();
					if (DrawingDocument._sDrawing != null)
						DrawingDocument._sDrawing.UpdateDrawing(true);

					if (bCancel)
						break;
				} // foreach(Rack rack in group)

				if (bCancel)
					break;
			}

			// rack can be grouped only if they have the same depth
			// need to check all groups - probably the first rack in the group is not M after match properties
			this.CheckRacksGroups(out deletedRacksList);
			
			this._UpdateRackRowsColumns();
			this.SelectedGeometryCollection.Clear();
			if (DrawingDocument._sDrawing != null)
				DrawingDocument._sDrawing.UpdateDrawing(true);
			// Display count of rack for which match properties command is not possible.
			if (notPossibleMatchPropsList.Count > 0)
			{
				string strError = "Not possible execute match properties command for ";
				strError += notPossibleMatchPropsList.Count.ToString();
				strError += " rack";
				if (notPossibleMatchPropsList.Count > 1)
					strError += "s";

				this.Document.DocumentError = strError;
			}

			// display message if layout is not correct
			if(!this.IsLayoutCorrect())
				await this.Document.DisplayDialog.DisplayMessageDialog("After match properties command layout is not correct. It is not recommended to save this drawing.");
		}



		//=============================================================================
		public void OnPropertyChanged(BaseRectangleGeometry owner, string propSystemName, bool bSuccessfull, string strError)
		{
			if (bSuccessfull)
			{
				if (owner == null)
					return;

				if (owner.IsInit)
					MarkStateChanged();
			}
			else
			{
				// Create rack group - select the first rack in the group - change length to 3000 - 
				// beams are recalculated - height is recalculated and is greater than MaxLength_Z - error - 
				// rack set the last state, but all other racks in the group change their position and dont restore it.
				// So, need to restore last document state - it will restore all geometry states.
				if (this.Document != null)
					this.Document.SetTheLastState();
			}

			if(this.Document != null)
				this.Document.DocumentError = strError;

			// always reread properties from object
			if (DrawingDocument._sDrawing != null)
			{
				DrawingDocument._sDrawing.ResetGrips();
				// Need to redraw geometry.
				// Probably user changed property on non initialized geometry, so MarkStateChanged() methods is not called and 
				// geometry is not redrawn.
				DrawingDocument._sDrawing.UpdateDrawing(true);
				// Need to update SelectedGeometryCollection and SingleSelectedGeometry properties.
				// If SelectedGeometry is Rack there can be advanced properties.
				// For update them lets mark "SelectedGeometry" as changed.
				NotifyPropertyChanged(() => SelectedGeometryCollection);
				NotifyPropertyChanged(() => SingleSelectedGeometry);
			}
		}
		//=============================================================================
		public void OnGripPointMoved(GripPoint grip)
		{
			if (Document == null)
				return;

			if (grip == null)
				return;

			// dont calc recalc column or rack unique size if move temp geometry
			if (grip.Geometry != null && !grip.Geometry.IsInit)
			{
				// update rack's pallets
				if (!(grip is CreateRackRowGripPoint || grip is CreateRackColumnGripPoint || grip is StretchRacksGroupGripPoint)
					&& (grip.Index == BaseRectangleGeometry.GRIP_TOP_LEFT || grip.Index == BaseRectangleGeometry.GRIP_BOTTOM_RIGHT))
				{
					Rack rack = grip.Geometry as Rack;
					if(rack != null)
					{
						string strError;
						if (!rack.OnLengthOrWidthChanged(out strError))
						{
							Document.SetTheLastState();
							return;
						}
					}
				}
				return;
			}

			bool bUpdateSelectedGeometry = false;
			// update columns and racks
			if (grip.Index == BaseRectangleGeometry.GRIP_TOP_LEFT || grip.Index == BaseRectangleGeometry.GRIP_BOTTOM_RIGHT)
			{
				// update rack's pallets
				if (!(grip is CreateRackRowGripPoint || grip is CreateRackColumnGripPoint || grip is StretchRacksGroupGripPoint))
				{
					Rack rack = grip.Geometry as Rack;
					if (rack != null)
					{
						string strError;
						if(!rack.OnLengthOrWidthChanged(out strError))
						{
							Document.SetTheLastState();
							return;
						}
						bUpdateSelectedGeometry = true;
					}
				}

				// try to change other columns with same size index
				if (grip.Geometry != null && grip.Geometry.IsInit)
				{
					Column column = grip.Geometry as Column;
					if (column != null)
						OnColumnSizeChanged(column);
				}
			}

			//
			StretchRacksGroupGripPoint stretchRacksGroupGP = grip as StretchRacksGroupGripPoint;
			if (stretchRacksGroupGP != null && stretchRacksGroupGP.Racks != null)
			{
				foreach (Rack rack in stretchRacksGroupGP.Racks)
				{
					string strError;
					if(!rack.OnLengthOrWidthChanged(out strError))
					{
						Document.SetTheLastState();
						return;
					}
				}
			}

			// Racks max available height depends on the roof type and rack position.
			// So, if user move rack's row or column group need to check height
			MoveRacksGroupGripPoint moveRacksGroupGP = grip as MoveRacksGroupGripPoint;
			if(moveRacksGroupGP != null && moveRacksGroupGP.RacksGroup != null)
			{
				foreach (Rack rack in moveRacksGroupGP.RacksGroup)
				{
					string strError;
					if (!rack.OnLengthOrWidthChanged(out strError))
					{
						Document.SetTheLastState();
						return;
					}
				}
			}

			Rack changedRack = grip.Geometry as Rack;
			// if single rack is moved, rotated or size changed then need to check height
			if (grip.Geometry is Rack
				&& !(grip is CreateRackRowGripPoint
					|| grip is CreateRackColumnGripPoint
					|| grip is SelectRackGroupGripPoint
					|| grip is MoveRacksGroupGripPoint
					|| grip is StretchRacksGroupGripPoint))
			{
				// Create vertical racks column:
				// -----
				// 
				//   M
				//
				// -----
				//   A
				// -----
				//   A
				// -----
				// Select and move middle A-racks in place where it will hit the roof.
				// After move command ends all racks are moved. RackUtils.TryToReplaceRackInGroup() inside CheckRackHeight() moves entire rack group.
				// Need to call RegroupRacks() before check rack height.
				this.RegroupRacks();

				changedRack.CheckRackHeight();
			}

			bool bMovingTempGeom = false;
			if (grip.Geometry != null && grip.Geometry != null && !grip.Geometry.IsInit)
				bMovingTempGeom = true;

			if (!bMovingTempGeom)
				MarkStateChanged();

			if(bUpdateSelectedGeometry)
			{
				// always reread properties from object
				if (DrawingDocument._sDrawing != null)
				{
					//UpdateDrawing(false);
					//DrawingDocument._sDrawing.UpdateDrawing(true);
					// Need to update SelectedGeometry properties.
					// If SelectedGeometry is Rack there can be advanced properties.
					// For update them lets mark "SelectedGeometry" as changed.
					NotifyPropertyChanged(() => SelectedGeometryCollection);
					NotifyPropertyChanged(() => SingleSelectedGeometry);
				}
			}
		}




		//=============================================================================
		/// <summary>
		/// 
		/// </summary>
		/// <param name="rGlobalWidth"></param>
		/// <param name="rGlobalHeight"></param>
		/// <param name="bJustCheck"> true - just check, false - delete all rectangles outside graphics area</param>
		/// <returns></returns>
		public bool IsThereRectanglesOutsideGraphicsArea(UInt32 _length, UInt32 _width, bool bJustCheck)
		{
			if (DrawingDocument._sDrawing == null)
				return false;

			List<BaseRectangleGeometry> geomForDelete = new List<BaseRectangleGeometry>();

			foreach (BaseRectangleGeometry geom in Rectangles)
			{
				if (geom == null)
					continue;

				// Ignore SheetElevationGeometry
				if (geom is SheetElevationGeometry)
					continue;

				// ignore walls
				if (geom is Wall)
					continue;

				// Check only bottom right point, because grpahics layout top left point is (0, 0)-point.
				// X-axis is directed to the right, Y-axis is directed to the bottom.
				if (geom.BottomRight_GlobalPoint.X > _length || geom.BottomRight_GlobalPoint.Y > _width)
				{
					if (bJustCheck)
						return true;
					else
						geomForDelete.Add(geom);
				}
			}

			//
			if (!bJustCheck && geomForDelete.Count > 0)
			{
				this.DeleteGeometry(geomForDelete, false, false);
			}

			return false;
		}


		//=============================================================================
		/// <summary>
		/// Move geometry in SelectedGeometryCollection by moveOffset.
		/// </summary>
		/// <param name="moveOffset">
		/// move offset
		/// </param>
		/// <param name="bCheckRackHeight">
		/// rack's max available height depends on the roof properties and it(rack) position, 
		/// so need to call rack check height when dragging is over
		/// </param>
		/// <returns></returns>
		public bool MoveSelection(Vector moveOffset, bool bCheckRackHeight, out Vector appliedOffset)
		{
			appliedOffset = new Vector(0.0, 0.0);
			if (SelectedGeometryCollection.Count == 0)
				return false;

			// get all racks from the selection and try to group them
			List<Rack> racksList = new List<Rack>();
			foreach (BaseRectangleGeometry geom in SelectedGeometryCollection)
			{
				Rack rack = geom as Rack;
				if (rack != null)
					racksList.Add(rack);
			}
			//
			List<List<Rack>> _racks_RowsColumns;
			DrawingSheet._GroupRacks(racksList, out _racks_RowsColumns);

			// calc new global offset
			double newGlobalOffset_X = Utils.GetWholeNumber(moveOffset.X);
			double newGlobalOffset_Y = Utils.GetWholeNumber(moveOffset.Y);

			// make preview and check layout
			int iLoopCount = 0;
			int iMaxLoopCount = 20;
			while (true)
			{
				++iLoopCount;
				if (iLoopCount == iMaxLoopCount)
					return false;

				List<BaseRectangleGeometry> rectanglesToSkip = new List<BaseRectangleGeometry>();
				List<BaseRectangleGeometry> rectanglesPreview = new List<BaseRectangleGeometry>();
				foreach (BaseRectangleGeometry geom in SelectedGeometryCollection)
				{
					if (geom is Wall)
						continue;

					// Dont move SheetElevationGeometry, only one instance of this geometry can be placed at the drawing
					if (geom is SheetElevationGeometry)
						continue;

					if (rectanglesToSkip.Contains(geom))
						continue;

					//
					rectanglesToSkip.Add(geom);

					// If it is a Column then need to move all columns in the pattern. You can move one column from the pattern.
					Column column = geom as Column;
					if (column != null)
					{
						ColumnPattern pattern = column.Pattern;
						if (pattern == null)
							continue;

						Point patternStartPoint;
						int columnsCount;
						int rowsCount;
						List<Column> patternColumnsList;
						List<List<Column>> patternRows;
						if (!pattern.GetPatternInfo(out patternStartPoint, out columnsCount, out rowsCount, out patternColumnsList, out patternRows))
							continue;

						rectanglesToSkip.AddRange(patternColumnsList);

						//
						foreach (Column columnGeom in patternColumnsList)
						{
							
							Column previewColumnGeom = columnGeom.Clone() as Column;
							if(previewColumnGeom != null)
								rectanglesPreview.Add(previewColumnGeom);
						}

						continue;
					}

					// If it is Rack just convert it to M-rack when make changes
					Rack rack = geom as Rack;
					if (rack != null && !rack.IsFirstInRowColumn)
					{
						bool bShouldBeTheFirst = false;
						foreach (List<Rack> rackRowColumn in _racks_RowsColumns)
						{
							if (rackRowColumn == null || rackRowColumn.Count == 0)
								continue;

							if (rackRowColumn[0] != rack)
								continue;

							bShouldBeTheFirst = true;
							break;
						}

						if (bShouldBeTheFirst)
						{
							// change rack
							Point rackTopLeftPoint = rack.TopLeft_GlobalPoint;
							if (rack.IsHorizontal)
							{
								rackTopLeftPoint.X -= rack.DiffBetween_M_and_A;
								rack.Length_X += rack.DiffBetween_M_and_A;
							}
							else
							{
								rackTopLeftPoint.Y -= rack.DiffBetween_M_and_A;
								rack.Length_Y += rack.DiffBetween_M_and_A;
							}
							rack.TopLeft_GlobalPoint = rackTopLeftPoint;
							rack.IsFirstInRowColumn = true;

							Rack previewRack = rack.Clone() as Rack;
							if (previewRack == null)
								continue;
							previewRack.Sheet = this;
							rectanglesPreview.Add(previewRack);

							continue;
						}
					}

					//
					Shutter shutter = geom as Shutter;
					if (shutter != null)
					{
						Point _shutterTopLeftPoint = shutter.TopLeft_GlobalPoint;
						if (shutter.IsHorizontal)
							_shutterTopLeftPoint.X += newGlobalOffset_X;
						else
							_shutterTopLeftPoint.Y += newGlobalOffset_Y;

						Shutter _previewShutter = shutter.Clone() as Shutter;
						if (_previewShutter == null)
							continue;
						_previewShutter.Sheet = this;
						_previewShutter.TopLeft_GlobalPoint = _shutterTopLeftPoint;

						//
						rectanglesPreview.Add(_previewShutter);

						continue;
					}

					BaseRectangleGeometry previewRect = geom.Clone() as BaseRectangleGeometry;
					if (previewRect != null)
					{
						previewRect.Sheet = this;
						rectanglesPreview.Add(previewRect);
					}
				}

				// check borders
				bool bRecreatePreview = false;
				foreach (BaseRectangleGeometry previewRect in rectanglesPreview)
				{
					Shutter shutter = previewRect as Shutter;
					if (shutter != null)
					{
						if (shutter.IsHorizontal)
						{
							//
							if (shutter.TopLeft_GlobalPoint.X < 0)
								return false;
							if (shutter.TopLeft_GlobalPoint.X + shutter.Length_X > this.Length)
								return false;
						}
						else
						{
							//
							if (shutter.TopLeft_GlobalPoint.Y < 0)
								return false;
							if (shutter.TopLeft_GlobalPoint.Y + shutter.Length_Y > this.Width)
								return false;
						}

						continue;
					}

					if (Utils.FLT(previewRect.TopLeft_GlobalPoint.X + newGlobalOffset_X, 0 + previewRect.MarginX))
					{
						newGlobalOffset_X = 0 + previewRect.MarginX - previewRect.TopLeft_GlobalPoint.X;
						bRecreatePreview = true;
						break;
					}
					if (Utils.FLT(previewRect.TopLeft_GlobalPoint.Y + newGlobalOffset_Y, 0 + previewRect.MarginY))
					{
						newGlobalOffset_Y = 0 + previewRect.MarginY - previewRect.TopLeft_GlobalPoint.Y;
						bRecreatePreview = true;
						break;
					}

					if (Utils.FGT(previewRect.BottomRight_GlobalPoint.X + previewRect.MarginX + newGlobalOffset_X, this.Length))
					{
						newGlobalOffset_X = this.Length - previewRect.MarginX - previewRect.BottomRight_GlobalPoint.X;
						bRecreatePreview = true;
						break;
					}
					if (Utils.FGT(previewRect.BottomRight_GlobalPoint.Y + previewRect.MarginY + newGlobalOffset_Y, this.Width))
					{
						newGlobalOffset_Y = this.Width - previewRect.MarginY - previewRect.BottomRight_GlobalPoint.Y;
						bRecreatePreview = true;
						break;
					}
				}
				//
				if (bRecreatePreview)
					continue;

				//
				if (Utils.FEQ(newGlobalOffset_X, 0.0) && Utils.FEQ(newGlobalOffset_Y, 0.0))
				{
					// check rack's height because it depends on the roof properties and rack position
					if (bCheckRackHeight)
					{
						foreach (Rack rack in racksList)
						{
							if (rack == null)
								continue;

							rack.CheckRackHeight();
						}
					}

					return true;
				}
				// apply offset
				foreach (BaseRectangleGeometry previewRect in rectanglesPreview)
				{
					Point previewTopLeftPoint = previewRect.TopLeft_GlobalPoint;
					previewTopLeftPoint.X += newGlobalOffset_X;
					previewTopLeftPoint.Y += newGlobalOffset_Y;
					// dont call SetGripPoint() because it is layout correction inside
					//_previewRect.SetGripPoint(BaseRectangleGeometry.GRIP_CENTER, _previewCenterPoint, DrawingLength, DrawingWidth);
					previewRect.TopLeft_GlobalPoint = previewTopLeftPoint;
				}

				// check layout and try to fix offset
				bRecreatePreview = false;
				foreach (BaseRectangleGeometry previewGeom in rectanglesPreview)
				{
					if (previewGeom == null)
						continue;

					List<BaseRectangleGeometry> overlappedGeometryList;
					if (!IsLayoutCorrect(previewGeom, rectanglesToSkip, out overlappedGeometryList))
					{
						Point oldTopLeftPoint = previewGeom.TopLeft_GlobalPoint;
						// try to fix it
						Point newTopLeftPoint;
						double newGlobalLength;
						double newGlobalWidth;
						//
						// infinity loop protection
						int iMaxFixLoopCount = 10;
						int iFixLoopCount = 0;
						//
						while (previewGeom._CalculateNotOverlapPosition(overlappedGeometryList, BaseRectangleGeometry.GRIP_CENTER, this.Length, this.Width, out newTopLeftPoint, out newGlobalLength, out newGlobalWidth))
						{
							previewGeom.TopLeft_GlobalPoint = newTopLeftPoint;

							//
							if (IsLayoutCorrect(previewGeom, rectanglesToSkip, out overlappedGeometryList))
								break;

							++iFixLoopCount;
							if (iFixLoopCount >= iMaxFixLoopCount)
								break;
						}

						bool bLayoutIsCorrect = previewGeom.IsInsideArea(this.Length, this.Width, false);
						if (bLayoutIsCorrect && !IsLayoutCorrect(previewGeom, rectanglesToSkip, out overlappedGeometryList))
							bLayoutIsCorrect = false;

						if (!bLayoutIsCorrect)
							return false;

						// change offset and rectreate preview geometry
						Vector newOffset = previewGeom.TopLeft_GlobalPoint - oldTopLeftPoint;
						newGlobalOffset_X += newOffset.X;
						newGlobalOffset_Y += newOffset.Y;
						bRecreatePreview = true;
					}

					if (bRecreatePreview)
						break;
				}

				if (bRecreatePreview)
					continue;

				// make changes
				rectanglesToSkip.Clear();
				foreach (BaseRectangleGeometry _geom in SelectedGeometryCollection)
				{
					if (rectanglesToSkip.Contains(_geom))
						continue;

					//
					rectanglesToSkip.Add(_geom);

					Point _CenterPoint = _geom.Center_GlobalPoint;
					_CenterPoint.X += newGlobalOffset_X;
					_CenterPoint.Y += newGlobalOffset_Y;

					// If it is a Column then need to move all columns in the pattern. You can move one column from the pattern.
					Column column = _geom as Column;
					if (column != null)
					{
						ColumnPattern pattern = column.Pattern;
						if (pattern == null)
							continue;

						Point patternStartPoint;
						int columnsCount;
						int rowsCount;
						List<Column> patternColumnsList;
						List<List<Column>> patternRows;
						if (!pattern.GetPatternInfo(out patternStartPoint, out columnsCount, out rowsCount, out patternColumnsList, out patternRows))
							continue;

						rectanglesToSkip.AddRange(patternColumnsList);

						//
						column.SetGripPoint(BaseRectangleGeometry.GRIP_CENTER, _CenterPoint, this.Length, this.Width);
						continue;
					}

					//
					Shutter _shutter = _geom as Shutter;
					if (_shutter != null)
					{
						Point _shutterTopLeftPoint = _shutter.TopLeft_GlobalPoint;
						if (_shutter.IsHorizontal)
							_shutterTopLeftPoint.X += newGlobalOffset_X;
						else
							_shutterTopLeftPoint.Y += newGlobalOffset_Y;

						_shutter.TopLeft_GlobalPoint = _shutterTopLeftPoint;

						continue;
					}

					// dont use Rack.SetGripPoint(BaseRectangleGeometry.GRIP_CENTER, ...) because there is set IsFirstInRowColumn = true inside
					// which increase rack's length
					//_geom.SetGripPoint(BaseRectangleGeometry.GRIP_CENTER, _CenterPoint, DrawingLength, DrawingWidth);
					string strError;
					_geom.SetPropertyValue(BaseRectangleGeometry.PROP_CENTER_POINT, _CenterPoint, false, false, false, out strError, false);
				}

				// check rack's height because it depends on the roof properties and rack position
				if (bCheckRackHeight)
				{
					foreach (Rack rack in racksList)
					{
						if (rack == null)
							continue;

						rack.CheckRackHeight();
					}
				}

				appliedOffset.X = newGlobalOffset_X;
				appliedOffset.Y = newGlobalOffset_Y;

				return true;
			}
		}
		//=============================================================================
		/// <summary>
		/// Add to selection rectangles between globalPnt01 and globalPnt02
		/// </summary>
		public void SelectByRectangle(Point globalPnt01, Point globalPnt02)
		{
			// remove old selection
			SelectedGeometryCollection.Clear();

			// add rectangles to the selection
			List<BaseRectangleGeometry> selectionList = new List<BaseRectangleGeometry>();
			foreach (BaseRectangleGeometry geom in Rectangles)
			{
				if (geom == null)
					continue;

				// Dont select SheetElevationGeometry, only one instance of this geometry can be placed at the drawing
				if (geom is SheetElevationGeometry)
					continue;

				if (geom.IsIntersectWithRectangle(globalPnt01, globalPnt02))
					SelectedGeometryCollection.Add(geom);
			}
		}



		//=============================================================================
		public bool _GetSnapPoint(Point globalPoint, out Point snapGlobalPoint, out double snap_X, out double snap_Y)
		{
			snapGlobalPoint = globalPoint;
			snap_X = -1;
			snap_Y = -1;

			//
			double coord_X = -1;
			double delta_X = -1;
			foreach (double _x in m_VerticalLines)
			{
				double newDelta_X = Math.Abs(globalPoint.X - _x);
				if (newDelta_X > m_SnapDistance)
					continue;

				if (delta_X < 0 || (delta_X >= 0 && newDelta_X < delta_X))
				{
					delta_X = newDelta_X;
					coord_X = _x;
				}
			}

			double coord_Y = -1;
			double delta_Y = -1;
			foreach (double _y in m_HorizontalLines)
			{
				double newDelta_Y = Math.Abs(globalPoint.Y - _y);
				if (newDelta_Y > m_SnapDistance)
					continue;

				if (delta_Y < 0 || (delta_Y >= 0 && newDelta_Y < delta_Y))
				{
					delta_Y = newDelta_Y;
					coord_Y = _y;
				}
			}

			//
			snap_X = coord_X;
			snap_Y = coord_Y;
			//
			if (coord_X >= 0 || coord_Y >= 0)
			{
				if (coord_X >= 0)
					snapGlobalPoint.X = coord_X;
				if (coord_Y >= 0)
					snapGlobalPoint.Y = coord_Y;

				return true;
			}

			return false;
		}
		//=============================================================================
		public bool _GetBestCenterSnapPoint(BaseRectangleGeometry movingRect, Point centerGlobalPoint, out Point snapCenterGlobalPoint, out double snap_X, out double snap_Y)
		{
			//
			snap_X = -1;
			snap_Y = -1;
			//
			snapCenterGlobalPoint = centerGlobalPoint;

			// check topleft, topright, botleft, botright and center points for movingRect
			// and select the best one
			if (movingRect == null)
				return false;

			//
			double absDelta_X = -1;
			double absDelta_Y = -1;

			//
			double coord_X;
			double coord_Y;

			Point snapPoint;
			// top left
			Point pntToCheck = centerGlobalPoint;
			pntToCheck.X -= movingRect.Length_X / 2;
			pntToCheck.Y -= movingRect.Length_Y / 2;
			pntToCheck = Utils.GetWholePoint(pntToCheck);
			if (_GetSnapPoint(pntToCheck, out snapPoint, out coord_X, out coord_Y))
			{
				double newAbsDelta_X = Math.Abs(snapPoint.X - pntToCheck.X);
				if (newAbsDelta_X > 0 && newAbsDelta_X <= m_SnapDistance)
				{
					if (absDelta_X < 0 || (absDelta_X >= 0 && newAbsDelta_X < absDelta_X))
					{
						absDelta_X = newAbsDelta_X;
						//
						snapCenterGlobalPoint.X = snapPoint.X + movingRect.Length_X / 2;
						//
						snap_X = coord_X;
					}
				}

				double newAbsDelta_Y = Math.Abs(snapPoint.Y - pntToCheck.Y);
				if (newAbsDelta_Y > 0 && newAbsDelta_Y <= m_SnapDistance)
				{
					if (absDelta_Y < 0 || (absDelta_Y >= 0 && newAbsDelta_Y < absDelta_Y))
					{
						absDelta_Y = newAbsDelta_Y;
						snapCenterGlobalPoint.Y = snapPoint.Y + movingRect.Length_Y / 2;
						//
						snap_Y = coord_Y;
					}
				}
			}
			// top right
			pntToCheck = centerGlobalPoint;
			pntToCheck.X += movingRect.Length_X / 2;
			pntToCheck.Y -= movingRect.Length_Y / 2;
			pntToCheck = Utils.GetWholePoint(pntToCheck);
			if (_GetSnapPoint(pntToCheck, out snapPoint, out coord_X, out coord_Y))
			{
				double newAbsDelta_X = Math.Abs(snapPoint.X - pntToCheck.X);
				if (newAbsDelta_X > 0 && newAbsDelta_X <= m_SnapDistance)
				{
					if (absDelta_X < 0 || (absDelta_X >= 0 && newAbsDelta_X < absDelta_X))
					{
						absDelta_X = newAbsDelta_X;
						//
						snapCenterGlobalPoint.X = snapPoint.X - movingRect.Length_X / 2;
						//
						snap_X = coord_X;
					}
				}

				double newAbsDelta_Y = Math.Abs(snapPoint.Y - pntToCheck.Y);
				if (newAbsDelta_Y > 0 && newAbsDelta_Y <= m_SnapDistance)
				{
					if (absDelta_Y < 0 || (absDelta_Y >= 0 && newAbsDelta_Y < absDelta_Y))
					{
						absDelta_Y = newAbsDelta_Y;
						snapCenterGlobalPoint.Y = snapPoint.Y + movingRect.Length_Y / 2;
						//
						snap_Y = coord_Y;
					}
				}
			}
			// bot left
			pntToCheck = centerGlobalPoint;
			pntToCheck.X -= movingRect.Length_X / 2;
			pntToCheck.Y += movingRect.Length_Y / 2;
			pntToCheck = Utils.GetWholePoint(pntToCheck);
			if (_GetSnapPoint(pntToCheck, out snapPoint, out coord_X, out coord_Y))
			{
				double newAbsDelta_X = Math.Abs(snapPoint.X - pntToCheck.X);
				if (newAbsDelta_X > 0 && newAbsDelta_X <= m_SnapDistance)
				{
					if (absDelta_X < 0 || (absDelta_X >= 0 && newAbsDelta_X < absDelta_X))
					{
						absDelta_X = newAbsDelta_X;
						//
						snapCenterGlobalPoint.X = snapPoint.X + movingRect.Length_X / 2;
						//
						snap_X = coord_X;
					}
				}

				double newAbsDelta_Y = Math.Abs(snapPoint.Y - pntToCheck.Y);
				if (newAbsDelta_Y > 0 && newAbsDelta_Y <= m_SnapDistance)
				{
					if (absDelta_Y < 0 || (absDelta_Y >= 0 && newAbsDelta_Y < absDelta_Y))
					{
						absDelta_Y = newAbsDelta_Y;
						snapCenterGlobalPoint.Y = snapPoint.Y - movingRect.Length_Y / 2;
						//
						snap_Y = coord_Y;
					}
				}
			}
			// bot right
			pntToCheck = centerGlobalPoint;
			pntToCheck.X += movingRect.Length_X / 2;
			pntToCheck.Y += movingRect.Length_Y / 2;
			pntToCheck = Utils.GetWholePoint(pntToCheck);
			if (_GetSnapPoint(pntToCheck, out snapPoint, out coord_X, out coord_Y))
			{
				double newAbsDelta_X = Math.Abs(snapPoint.X - pntToCheck.X);
				if (newAbsDelta_X > 0 && newAbsDelta_X <= m_SnapDistance)
				{
					if (absDelta_X < 0 || (absDelta_X >= 0 && newAbsDelta_X < absDelta_X))
					{
						absDelta_X = newAbsDelta_X;
						//
						snapCenterGlobalPoint.X = snapPoint.X - movingRect.Length_X / 2;
						//
						snap_X = coord_X;
					}
				}

				double newAbsDelta_Y = Math.Abs(snapPoint.Y - pntToCheck.Y);
				if (newAbsDelta_Y > 0 && newAbsDelta_Y <= m_SnapDistance)
				{
					if (absDelta_Y < 0 || (absDelta_Y >= 0 && newAbsDelta_Y < absDelta_Y))
					{
						absDelta_Y = newAbsDelta_Y;
						snapCenterGlobalPoint.Y = snapPoint.Y - movingRect.Length_Y / 2;
						//
						snap_Y = coord_Y;
					}
				}
			}
			// center
			pntToCheck = centerGlobalPoint;
			if (_GetSnapPoint(pntToCheck, out snapPoint, out coord_X, out coord_Y))
			{
				double newAbsDelta_X = Math.Abs(snapPoint.X - pntToCheck.X);
				if (newAbsDelta_X > 0 && newAbsDelta_X <= m_SnapDistance)
				{
					if (absDelta_X < 0 || (absDelta_X >= 0 && newAbsDelta_X < absDelta_X))
					{
						absDelta_X = newAbsDelta_X;
						//
						snapCenterGlobalPoint.X = snapPoint.X;
						//
						snap_X = coord_X;
					}
				}

				double newAbsDelta_Y = Math.Abs(snapPoint.Y - pntToCheck.Y);
				if (newAbsDelta_Y > 0 && newAbsDelta_Y <= m_SnapDistance)
				{
					if (absDelta_Y < 0 || (absDelta_Y >= 0 && newAbsDelta_Y < absDelta_Y))
					{
						absDelta_Y = newAbsDelta_Y;
						snapCenterGlobalPoint.Y = snapPoint.Y;
						//
						snap_Y = coord_Y;
					}
				}
			}

			//
			if (absDelta_X >= 0 || absDelta_Y >= 0)
				return true;

			return false;
		}


		//=============================================================================
		// Rack can be grouped only if they have the same depth.
		// This function goes through all rack groups and check 2 cases:
		// 1. the first rack in the group should be M-rack
		// 2. all other racks shoudl be A-racks
		public void CheckRacksGroups(out List<Rack> deletedRacksList)
		{
			deletedRacksList = new List<Rack>();

			this.RegroupRacks();
			// rack can be grouped only if they have the same depth
			// need to check all groups - probably the first rack in the group is not M after match properties
			foreach (List<Rack> rackGroup in RacksGroups)
			{
				if (rackGroup == null)
					continue;
				if (rackGroup.Count == 0)
					continue;

				bool bChangesAreMade = false;
				foreach (Rack rack in rackGroup)
				{
					if (rack == null)
						continue;

					int iRackIndex = rackGroup.IndexOf(rack);
					if (iRackIndex < 0)
						break;

					// can create row
					rack.CanCreateRow = rack == rackGroup.LastOrDefault();
					// show rotation grips
					bool bShowRotationGrips = rackGroup.Count == 1;
					rack.ShowRotationGrips = bShowRotationGrips;
					// Update rack index.
					// Create rack row - change length of the middle rack using grip point - copy and paste this rack in another place - 
					// move original rack out of the row using center grip point - rack receive incorrect index.
					//rack.SizeIndex = Document.GetRackUniqueSizeIndex(rack);

					if (iRackIndex == 0 && !rack.IsFirstInRowColumn)
					{
						Point oldBotRightPnt = rack.BottomRight_GlobalPoint;

						RackUtils.Convert_A_to_M_Rack(this, rack, true, false);

						// #42 Move rack from the row using center grip point moves all next racks. "X location" video bug.
						// If need to replace the first rack in the group then
						// try to keep the same position for all other racks in this group.
						Point newTopLeftPnt = rack.TopLeft_GlobalPoint;
						if (rack.IsHorizontal)
							newTopLeftPnt.X = oldBotRightPnt.X - rack.Length;
						else
							newTopLeftPnt.Y = oldBotRightPnt.Y - rack.Length;
						rack.TopLeft_GlobalPoint = newTopLeftPnt;
					}
					else if (iRackIndex > 0 && rack.IsFirstInRowColumn)
						RackUtils.Convert_A_to_M_Rack(this, rack, false, false);
					else
						continue;

					List<Rack> _previewGroup = new List<Rack>();
					RackColumn minColumn = null;
					if (!RackUtils.TryToReplaceRackInGroup(this, rack, iRackIndex, false, rackGroup, out _previewGroup, out deletedRacksList, out minColumn))
						continue;

					if (!bChangesAreMade)
						bChangesAreMade = true;
				}

				if(bChangesAreMade)
					this.RegroupRacks();
			}
		}


		//=============================================================================
		// Rack max available height depends on the roof type and rack position.
		// This function checks rack height.
		public void CheckRackHeight(List<BaseRectangleGeometry> geometryList, bool bRecalcRackUniquesSize)
		{
			if (geometryList == null)
				return;

			List<Rack> racksList = new List<Rack>();
			foreach (BaseRectangleGeometry geom in geometryList)
			{
				if (geom == null)
					continue;

				Rack rack = geom as Rack;
				if (rack == null)
					continue;

				racksList.Add(rack);
			}

			CheckRackHeight(racksList, bRecalcRackUniquesSize);
		}
		public void CheckRackHeight(List<Rack> racksList, bool bRecalcRackUniquesSize)
		{
			if (racksList == null)
				return;

			DrawingDocument currDoc = Document;
			if (currDoc == null)
				return;

			foreach (Rack rack in racksList)
			{
				if (rack == null)
					continue;

				rack.CheckRackHeight();
			}
		}

		//=============================================================================
		// Check Aisle Spaces at this sheet, their MinLengthX and MinLengthY values
		// depends on the adjusted geometry.
		public void CheckAisleSpaces()
		{
			List<BaseRectangleGeometry> incorrectAisleSpaceList = new List<BaseRectangleGeometry>();
			foreach (BaseRectangleGeometry geom in Rectangles)
			{
				if (geom == null)
					continue;

				AisleSpace aiGeom = geom as AisleSpace;
				if (aiGeom == null)
					continue;

				// calculate LengthX and LengthY based on the MHE travel width options
				aiGeom.Length_X = Utils.CheckWholeNumber(aiGeom.Length_X, aiGeom.MinLength_X, aiGeom.MaxLength_X);
				aiGeom.Length_Y = Utils.CheckWholeNumber(aiGeom.Length_Y, aiGeom.MinLength_Y, aiGeom.MaxLength_Y);
				aiGeom.OnSizeChanged();

				// check layout
				List<BaseRectangleGeometry> overlappedRectangles;
				List<BaseRectangleGeometry> ignoreGeomList = new List<BaseRectangleGeometry>();
				ignoreGeomList.AddRange(incorrectAisleSpaceList);
				bool bLayoutIsCorrect = true;
				if (!IsLayoutCorrect(aiGeom, ignoreGeomList, out overlappedRectangles))
				{
					bLayoutIsCorrect = false;

					// try to fix it
					Point newTopLeftPoint;
					double newGlobalLength;
					double newGlobalWidth;
					//
					// infinity loop protection
					int iMaxLoopCount = 100;
					int iLoopCount = 0;
					//
					while (aiGeom._CalculateNotOverlapPosition(overlappedRectangles, BaseRectangleGeometry.GRIP_CENTER, Length, Width, out newTopLeftPoint, out newGlobalLength, out newGlobalWidth))
					{
						// dont change the size of the aisle space, only top left point
						aiGeom.TopLeft_GlobalPoint = newTopLeftPoint;

						//
						if (IsLayoutCorrect(aiGeom, ignoreGeomList, out overlappedRectangles))
						{
							bLayoutIsCorrect = true;
							break;
						}

						++iLoopCount;
						if (iLoopCount >= iMaxLoopCount)
							break;
					}
				}

				if (!bLayoutIsCorrect)
					incorrectAisleSpaceList.Add(aiGeom);
			}

			// delete incorrect aisle space
			if(incorrectAisleSpaceList.Count > 0)
				DeleteGeometry(incorrectAisleSpaceList, false, true);
		}

		public void CheckAisleSpacesAndRacksCollisions()
		{
			List<AisleSpace> aisleSpaceList = new List<AisleSpace>();
			List<Rack> racksList = new List<Rack>();

			foreach (BaseRectangleGeometry geom in Rectangles)
			{
				if (geom == null)
					continue;

				AisleSpace aiGeom = geom as AisleSpace;
				if (aiGeom != null)
				{
					aisleSpaceList.Add(aiGeom);
					continue;
				}

				Rack rackGeom = geom as Rack;
				if (rackGeom != null)
				{
					rackGeom.ConectedAisleSpaceDirections = ConectedAisleSpaceDirection.NONE;
					racksList.Add(rackGeom);
					continue;
				}
			}

			foreach (AisleSpace aiGeom in aisleSpaceList)
			{
				foreach (Rack rack in racksList)
				{
					// Check is rack right edge contains in aisle space left edge
					if (Utils.IsSubline(
						Utils.GetWholePoint(rack.TopRight_GlobalPoint), Utils.GetWholePoint(rack.BottomRight_GlobalPoint),
						Utils.GetWholePoint(aiGeom.TopLeft_GlobalPoint), Utils.GetWholePoint(aiGeom.BottomLeft_GlobalPoint)))
					{
						rack.ConectedAisleSpaceDirections = rack.ConectedAisleSpaceDirections == ConectedAisleSpaceDirection.NONE 
							? ConectedAisleSpaceDirection.RIGHT 
							: rack.ConectedAisleSpaceDirections | ConectedAisleSpaceDirection.RIGHT;
					}

					// Check is rack left edge contains in aisle space right edge
					if (Utils.IsSubline(
						Utils.GetWholePoint(rack.TopLeft_GlobalPoint), Utils.GetWholePoint(rack.BottomLeft_GlobalPoint),
						Utils.GetWholePoint(aiGeom.TopRight_GlobalPoint), Utils.GetWholePoint(aiGeom.BottomRight_GlobalPoint)))
					{
						rack.ConectedAisleSpaceDirections = rack.ConectedAisleSpaceDirections == ConectedAisleSpaceDirection.NONE
							? ConectedAisleSpaceDirection.LEFT
							: rack.ConectedAisleSpaceDirections | ConectedAisleSpaceDirection.LEFT;
					}

					// Check is rack top edge contains in aisle space bottom edge
					if (Utils.IsSubline(
						Utils.GetWholePoint(rack.TopLeft_GlobalPoint), Utils.GetWholePoint(rack.TopRight_GlobalPoint),
						Utils.GetWholePoint(aiGeom.BottomLeft_GlobalPoint), Utils.GetWholePoint(aiGeom.BottomRight_GlobalPoint)))
					{
						rack.ConectedAisleSpaceDirections = rack.ConectedAisleSpaceDirections == ConectedAisleSpaceDirection.NONE
							? ConectedAisleSpaceDirection.TOP
							: rack.ConectedAisleSpaceDirections | ConectedAisleSpaceDirection.TOP;
					}

					// Check is rack bottom edge contains in aisle space top edge
					if (Utils.IsSubline(
						Utils.GetWholePoint(rack.BottomLeft_GlobalPoint), Utils.GetWholePoint(rack.BottomRight_GlobalPoint),
						Utils.GetWholePoint(aiGeom.TopLeft_GlobalPoint), Utils.GetWholePoint(aiGeom.TopRight_GlobalPoint)))
                    {
						rack.ConectedAisleSpaceDirections = rack.ConectedAisleSpaceDirections == ConectedAisleSpaceDirection.NONE
							? ConectedAisleSpaceDirection.BOTTOM
							: rack.ConectedAisleSpaceDirections | ConectedAisleSpaceDirection.BOTTOM;
					}
				}
			}
        }

		//=============================================================================
		/// <summary>
		/// Check rack's column size and bracing. It depends on the level weight and neighboring racks level weight.
		/// All racks in the row\column should have the same RackColumn.
		/// </summary>
		public void CheckRacksColumnSizeAndBracingType(bool bUpdateSheet)
		{
			// Go through all racks groups and calculate column size and bracing type.
			foreach (List<Rack> rackGroup in this.RacksGroups)
			{
				if (rackGroup == null)
					continue;

				CheckRacksColumnSizeAndBracingType(rackGroup);
			}
		}
		public void CheckRacksColumnSizeAndBracingType(List<Rack> racksGroup)
		{
			if (racksGroup == null)
				return;

			if (this.Document == null || this.Document.RacksColumnsList == null || this.Document.RacksColumnsList.Count == 0)
				return;

			eColumnBracingType bracingType;
			//double xBracingHeight = 0.0;
			double stiffenersHeight = 0.0;
			RackColumn minAvailableColumn = null;
			if (!RackUtils.CalculateRacksColumnSizeAndBracingType(racksGroup, this.Document.RacksColumnsList, out bracingType, /*out xBracingHeight,*/ out minAvailableColumn, out stiffenersHeight))
				return;

			// All racks in the group should have the same column, so take the biggest one.
			RackColumn selectedColumn = minAvailableColumn;
			foreach (Rack rack in racksGroup)
			{
				if (rack == null || rack.Column == null)
					continue;

				// Dont consider columns which are not set manually.
				// Racks group should have the minimum available column if it is not set manually.
				if (!rack.IsColumnSetManually)
					continue;

				if (rack.Column == selectedColumn)
					continue;
				else
				{
					if (Utils.FGT(rack.Column.Length, selectedColumn.Length) || (Utils.FGE(rack.Column.Length, selectedColumn.Length) && Utils.FGE(rack.Column.Thickness, selectedColumn.Thickness)))
					{
						eColumnBracingType newBracingType;
						//double newColumnBracingHeight = 0.0;
						double newStiffenersHeight = 0.0;
						// check X bracing height for the new column
						if (RackUtils.CalculateRacksColumnSizeAndBracingType(racksGroup, new List<RackColumn>() { rack.Column }, out newBracingType, /*out newColumnBracingHeight,*/ out selectedColumn, out newStiffenersHeight))
						{
							selectedColumn = rack.Column;
							bracingType = newBracingType;
							//xBracingHeight = newColumnBracingHeight;
							stiffenersHeight = newStiffenersHeight;
						}
					}
				}
			}

			double xBracingHeight = 0.0;
			if ((eColumnBracingType.eXBracing == bracingType || eColumnBracingType.eXBracingWithStiffener == bracingType) /*&& Utils.FGT(xBracingHeight, 0.0)*/)
			{
				// Check min X bracing height.
				foreach (Rack rack in racksGroup)
				{
					if (rack == null)
						continue;

					double minXBracingHeight = rack.MinXBracingHeight;
					if (Utils.FGT(minXBracingHeight, xBracingHeight))
						xBracingHeight = minXBracingHeight;
				}
			}
			// Apply column and bracing to all racks in the group.
			foreach (Rack rack in racksGroup)
			{
				if (rack == null)
					continue;

				string strError;
				rack.Set_Column(minAvailableColumn, true, false, true, out strError);
				if(selectedColumn != rack.Column)
					rack.Set_Column(selectedColumn, false, false, true, out strError);
				rack.Bracing = bracingType;
				rack.X_Bracing_Height = xBracingHeight;
				rack.StiffenersHeight = stiffenersHeight;
			}
		}


		//=============================================================================
		private static double MAX_TIE_BEAM_LENGTH = 4500.0;
		public void CheckTieBeams()
		{
			// Remove all tie beams.
			this.TieBeamsList.Clear();
			List<Rack> allRacksList = this.GetAllRacks();
			if(allRacksList != null)
			{
				foreach(Rack rack in allRacksList)
				{
					if (rack == null)
						continue;

					rack.RequiredTieBeamFrames = eTieBeamFrame.eNone;
					rack.TieBeamFrame = eTieBeamFrame.eNone;
					rack.RackHeightWithTieBeam_IsMoreThan_MaxHeight = false;
				}
			}

			if (this.Document == null)
				return;
			//if (this.Document.Rack_Accessories == null)
			//	return;
			// STEP 1.
			// Check Document.Rack_Accessories.TopTieMember property.
			// If it is TRUE then need to display tie beams.
			//if (!this.Document.Rack_Accessories.TopTieMember)
			//	return;

			// STEP 2.
			//
			// Tie beam can be placed only between 2 rack group, which have the same direction(vertical\horizontal),
			// stand opposite each other with AisleSpace between them.
			//
			// If you find that racks group then you need to calculate max beam placement length - you need to find the biggest length
			// where racks in the groups have the same length:
			// first_rack_length from goup 1  = first_rack_length from goup 2
			// second_rack_length from goup 1 = second_rack_length from goup 2
			// third_rack_length from goup 1  = third_rack_length from goup 2
			// fourth_rack_length from goup 1 = fourth_rack_length from goup 2
			// till you find the racks with different length.

			// STEP 3.
			// Prepare, calculate which racks requires tie beam.
			// Need to check all racks and set their Rack.RequiredTieBeamFrames property.
			//
			// Check Height to Depth ratio.
			// How to calc H\D ratio - take the biggest max loading height value from the racks group and
			// divide it by total racks group length.
			//
			// If pallet type is Flush then margin between the same rotated racks in depth direction is DrawingDocument.RACK_FLUSH_PALLETS_BTB_DISTANCE.
			// If Overhang then margin = 2*overhang + DrawingDocument.RACK_OVERHANG_PALLETS_BTB_DISTANCE_ADDITIONALVALUE.
			double racksBTBDistance = 0.0;
			if (ePalletType.eFlush == this.Document.RacksPalletType)
				racksBTBDistance = DrawingDocument.RACK_FLUSH_PALLETS_BTB_DISTANCE;
			else
				racksBTBDistance = DrawingDocument.RACK_OVERHANG_PALLETS_BTB_DISTANCE_ADDITIONALVALUE;
			//
			TieBeamGroupsInfo tieBeamGroupsInfo = new TieBeamGroupsInfo(RacksGroups, this.Document.RacksPalletType, racksBTBDistance);
			if (tieBeamGroupsInfo.m_TieBeamsGroupsList.Count == 0)
				return;

			// TieBeam can be placed only between 2 racks groups(row or column)
			if (this.RacksGroups == null || this.RacksGroups.Count <= 1)
				return;

			// Get AisleSpaces list.
			List<AisleSpace> aisleSpacesList = new List<AisleSpace>();
			foreach(BaseRectangleGeometry geom in this.Rectangles)
			{
				AisleSpace aisleSpaceGeom = geom as AisleSpace;
				if (aisleSpaceGeom == null)
					continue;

				// Max TieBeam length is MAX_TIE_BEAM_LENGTH, so if any dimension is less or equal MAX_TIE_BEAM_LENGTH then add this AisleSpace to the list.
				if (Utils.FLE(aisleSpaceGeom.Length_X, MAX_TIE_BEAM_LENGTH) || Utils.FLE(aisleSpaceGeom.Length_Y, MAX_TIE_BEAM_LENGTH))
					aisleSpacesList.Add(aisleSpaceGeom);
			}
			// If sheet doesnt contains any AisleSpace then tie beams cant be placed.
			if (aisleSpacesList.Count == 0)
				return;

			// STEP 4.
			// Dictionary which contains contact sides between aisle spaces and the first racks from the groups
			// eGeometryContactSide - side on the rack, which contacts with aisle space
			Dictionary<AisleSpace, Dictionary<eGeometryContactSide, List<Rack>>> aisleSpacesContactSidesDictionary = new Dictionary<AisleSpace, Dictionary<eGeometryContactSide, List<Rack>>>();
			Dictionary<AisleSpace, Dictionary<eGeometryContactSide, List<TieBeamGroup>>> tieBeamsGroupsContactSidesDictionary = new Dictionary<AisleSpace, Dictionary<eGeometryContactSide, List<TieBeamGroup>>>();
			if (!TieBeamUtils.CalculateContactSides(aisleSpacesList, RacksGroups, tieBeamGroupsInfo.m_TieBeamsGroupsList, out aisleSpacesContactSidesDictionary, out tieBeamsGroupsContactSidesDictionary))
				return;

			// STEP 5.
			// Clear the dictionary.
			// 1. If AisleSpace doesnt have two opposite contacts sides(bot and top, left and right) then remove this contact side.
			// 2. If aisle space is bigger than MAX_TIE_BEAM_LENGTH, then remove it. Tie Beam cant be greater than MAX_TIE_BEAM_LENGTH.
			// 3. If racks from the opposite sides dont need a tie beam then remove both sides.
			List<AisleSpace> aisleSpacesForClearList = new List<AisleSpace>();
			foreach(AisleSpace aisleSpaceGeom in aisleSpacesContactSidesDictionary.Keys)
			{
				Dictionary<eGeometryContactSide, List<TieBeamGroup>> groupsContactSidesDict = null;
				if (tieBeamsGroupsContactSidesDictionary.ContainsKey(aisleSpaceGeom))
					groupsContactSidesDict = tieBeamsGroupsContactSidesDictionary[aisleSpaceGeom];
				if(groupsContactSidesDict == null)
				{
					aisleSpacesForClearList.Add(aisleSpaceGeom);
					continue;
				}

				Dictionary<eGeometryContactSide, List<Rack>> contactsSidesDict = aisleSpacesContactSidesDictionary[aisleSpaceGeom];
				if (contactsSidesDict == null)
				{
					aisleSpacesForClearList.Add(aisleSpaceGeom);
					continue;
				}

				// 1.
				if((contactsSidesDict.ContainsKey(eGeometryContactSide.eBot) && !contactsSidesDict.ContainsKey(eGeometryContactSide.eTop))
					|| (!contactsSidesDict.ContainsKey(eGeometryContactSide.eBot) && contactsSidesDict.ContainsKey(eGeometryContactSide.eTop)))
				{
					contactsSidesDict.Remove(eGeometryContactSide.eBot);
					contactsSidesDict.Remove(eGeometryContactSide.eTop);
				}
				//
				if ((contactsSidesDict.ContainsKey(eGeometryContactSide.eLeft) && !contactsSidesDict.ContainsKey(eGeometryContactSide.eRight))
					|| (!contactsSidesDict.ContainsKey(eGeometryContactSide.eLeft) && contactsSidesDict.ContainsKey(eGeometryContactSide.eRight)))
				{
					contactsSidesDict.Remove(eGeometryContactSide.eLeft);
					contactsSidesDict.Remove(eGeometryContactSide.eRight);
				}

				// 2.
				if (contactsSidesDict.ContainsKey(eGeometryContactSide.eBot) && contactsSidesDict.ContainsKey(eGeometryContactSide.eTop))
				{
					double tieBeamLength = aisleSpaceGeom.Length_Y;
					if(Utils.FGT(tieBeamLength, MAX_TIE_BEAM_LENGTH))
					{
						contactsSidesDict.Remove(eGeometryContactSide.eBot);
						contactsSidesDict.Remove(eGeometryContactSide.eTop);
					}
				}
				else if (contactsSidesDict.ContainsKey(eGeometryContactSide.eLeft) && contactsSidesDict.ContainsKey(eGeometryContactSide.eRight))
				{
					double tieBeamLength = aisleSpaceGeom.Length_X;
					if (Utils.FGT(tieBeamLength, MAX_TIE_BEAM_LENGTH))
					{
						contactsSidesDict.Remove(eGeometryContactSide.eLeft);
						contactsSidesDict.Remove(eGeometryContactSide.eRight);
					}
				}

				// 3.
				if (contactsSidesDict.ContainsKey(eGeometryContactSide.eTop) && !groupsContactSidesDict.ContainsKey(eGeometryContactSide.eBot))
				{
					contactsSidesDict.Remove(eGeometryContactSide.eTop);
					groupsContactSidesDict.Remove(eGeometryContactSide.eBot);
				}
				if (contactsSidesDict.ContainsKey(eGeometryContactSide.eBot) && !groupsContactSidesDict.ContainsKey(eGeometryContactSide.eTop))
				{
					contactsSidesDict.Remove(eGeometryContactSide.eBot);
					groupsContactSidesDict.Remove(eGeometryContactSide.eTop);
				}
				//
				if (contactsSidesDict.ContainsKey(eGeometryContactSide.eLeft) && !groupsContactSidesDict.ContainsKey(eGeometryContactSide.eRight))
				{
					contactsSidesDict.Remove(eGeometryContactSide.eLeft);
					groupsContactSidesDict.Remove(eGeometryContactSide.eRight);
				}
				if (contactsSidesDict.ContainsKey(eGeometryContactSide.eRight) && !groupsContactSidesDict.ContainsKey(eGeometryContactSide.eLeft))
				{
					contactsSidesDict.Remove(eGeometryContactSide.eRight);
					groupsContactSidesDict.Remove(eGeometryContactSide.eLeft);
				}

				//
				if (contactsSidesDict.Keys.Count == 0)
					aisleSpacesForClearList.Add(aisleSpaceGeom);
			}
			//
			foreach (AisleSpace aisleSpaceGeom in aisleSpacesForClearList)
			{
				if (aisleSpaceGeom == null)
					continue;

				aisleSpacesContactSidesDictionary.Remove(aisleSpaceGeom);
				tieBeamsGroupsContactSidesDictionary.Remove(aisleSpaceGeom);
			}
			//
			if (aisleSpacesContactSidesDictionary.Keys.Count == 0 || tieBeamsGroupsContactSidesDictionary.Keys.Count == 0)
				return;

			// STEP 6.
			// Try to find the max placement distance.
			// Tie beam can be placed only over AisleSpace.
			List<TieBeamCalculationInfo> tieBeamCalcInfoList = new List<TieBeamCalculationInfo>();
			foreach(AisleSpace aisleSpaceGeom in tieBeamsGroupsContactSidesDictionary.Keys)
			{
				if (aisleSpaceGeom == null)
					continue;

				if (!aisleSpacesContactSidesDictionary.ContainsKey(aisleSpaceGeom) || !tieBeamsGroupsContactSidesDictionary.ContainsKey(aisleSpaceGeom))
					continue;

				Dictionary<eGeometryContactSide, List<Rack>> contactsSidesDict = aisleSpacesContactSidesDictionary[aisleSpaceGeom];
				// Racks groups which needs a tie beam
				Dictionary<eGeometryContactSide, List<TieBeamGroup>> groupsContactSidesDict = tieBeamsGroupsContactSidesDictionary[aisleSpaceGeom];
				if (contactsSidesDict == null || groupsContactSidesDict == null)
					continue;

				foreach (eGeometryContactSide contactSide in groupsContactSidesDict.Keys)
				{
					// List with headers racks(header of the racks row\column).
					// Rack from the headersRacksList_01 are opposite to the racks from headersRacksList_02, it means that theay are placed at the opposite sides
					// of the AisleSpace.
					List<Rack> oppositeHeadersRacksList = null;
					List<TieBeamGroup> tieBeamGroupsList = null;

					// Side opposite to contactSide.
					eGeometryContactSide oppositeSide = eGeometryContactSide.eWithoutContactSide;
					if (eGeometryContactSide.eBot == contactSide)
						oppositeSide = eGeometryContactSide.eTop;
					else if (eGeometryContactSide.eTop == contactSide)
						oppositeSide = eGeometryContactSide.eBot;
					else if (eGeometryContactSide.eLeft == contactSide)
						oppositeSide = eGeometryContactSide.eRight;
					else if (eGeometryContactSide.eRight == contactSide)
						oppositeSide = eGeometryContactSide.eLeft;

					if (contactsSidesDict.ContainsKey(oppositeSide))
						oppositeHeadersRacksList = contactsSidesDict[oppositeSide];
					if (groupsContactSidesDict.ContainsKey(contactSide))
						tieBeamGroupsList = groupsContactSidesDict[contactSide];

					if (oppositeHeadersRacksList == null || oppositeHeadersRacksList.Count == 0)
						continue;
					if (tieBeamGroupsList == null || tieBeamGroupsList.Count == 0)
						continue;

					// Get tie beam groups from tieBeamGroupsList and try to find opposite racks.
					foreach(TieBeamGroup tieBeamGroup in tieBeamGroupsList)
					{
						if (tieBeamGroup == null)
							continue;

						if (eTieBeamPlacement.eNone == tieBeamGroup.TieBeamPlacement)
							continue;

						Dictionary<Rack, eTieBeamFrame> tieBeamsRackDict = tieBeamGroup.GetTieBeamRacks(contactSide);
						if (tieBeamsRackDict == null || tieBeamsRackDict.Keys.Count == 0)
							continue;

						foreach(Rack oppositeHeaderRack in oppositeHeadersRacksList)
						{
							if (oppositeHeaderRack == null)
								continue;

							List<Rack> oppositeRacksGroup = RacksGroups.FirstOrDefault(group => group != null && group.Count > 0 && group[0] == oppositeHeaderRack);
							if (oppositeRacksGroup == null)
								continue;

							List<TieBeamCalculationInfo> tieBeamsInfoList = TieBeamUtils.CalculateTieBeams(aisleSpaceGeom, contactSide, tieBeamsRackDict, oppositeRacksGroup);
							if (tieBeamsInfoList == null || tieBeamsInfoList.Count == 0)
								continue;

							// add tie beam info
							foreach (TieBeamCalculationInfo tieBeamInfo in tieBeamsInfoList)
							{
								if (tieBeamInfo == null
									|| tieBeamInfo.OppositeRacksDict == null
									|| tieBeamInfo.OppositeRacksDict.Keys.Count != 2
									|| tieBeamInfo.OppositeRacksDict.Keys.ElementAt(0) == null
									|| tieBeamInfo.OppositeRacksDict.Keys.ElementAt(1) == null)
								{
									continue;
								}

								bool bSkip = false;
								foreach(Rack rackKey in tieBeamInfo.OppositeRacksDict.Keys)
								{
									if (rackKey == null)
									{
										bSkip = true;
										break;
									}

									if(tieBeamInfo.OppositeRacksDict[rackKey] == eTieBeamFrame.eNone)
									{
										bSkip = true;
										break;
									}
								}
								if (bSkip)
									continue;

								// probably TieBeamCalculationInfo with these racks already exist
								TieBeamCalculationInfo foundTieBeamInfo = tieBeamCalcInfoList.FirstOrDefault(
									info =>
									info.OppositeRacksDict != null
									&& info.OppositeRacksDict.ContainsKey(tieBeamInfo.OppositeRacksDict.Keys.ElementAt(0))
									&& info.OppositeRacksDict.ContainsKey(tieBeamInfo.OppositeRacksDict.Keys.ElementAt(1)));
								if (foundTieBeamInfo == null)
									tieBeamCalcInfoList.Add(tieBeamInfo);
								else
								{
									// tie beam info with these 2 racks already exist
									// just add eTieBeamFrame
									List<Rack> keysList = foundTieBeamInfo.OppositeRacksDict.Keys.ToList();
									foreach (Rack rackKey in keysList)
									{
										if (rackKey == null)
											continue;

										if (!tieBeamInfo.OppositeRacksDict.ContainsKey(rackKey) || tieBeamInfo.OppositeRacksDict[rackKey] == eTieBeamFrame.eNone)
											continue;

										foundTieBeamInfo.OppositeRacksDict[rackKey] |= tieBeamInfo.OppositeRacksDict[rackKey];
									}
								}
							}
						}
					}
				}
			}

			// Add tie beams.
			foreach (TieBeamCalculationInfo info in tieBeamCalcInfoList)
			{
				if (info == null)
					continue;
				if (info.OppositeRacksDict == null
					|| info.OppositeRacksDict.Count != 2)
				{
					continue;
				}

				Rack firstRack = info.OppositeRacksDict.Keys.ElementAt(0);
				Rack secondRack = info.OppositeRacksDict.Keys.ElementAt(1);
				if (firstRack == null || secondRack == null)
					continue;
				if (info.OppositeRacksDict[firstRack] == eTieBeamFrame.eNone || info.OppositeRacksDict[secondRack] == eTieBeamFrame.eNone)
					continue;

				bool bFirstRackHeightError = false;
				double firstRackHeight = RackUtils.RoundColumnHeight(firstRack.MaterialHeight + Rack.TIE_BEAM_ADDITIONAL_HEIGHT);
				if (Utils.FGT(firstRackHeight, firstRack.ClearAvailableHeight))
					bFirstRackHeightError = true;
				firstRack.RackHeightWithTieBeam_IsMoreThan_MaxHeight = bFirstRackHeightError;

				bool bSecondRackHeightError = false;
				double secondRackHeight = RackUtils.RoundColumnHeight(secondRack.MaterialHeight + Rack.TIE_BEAM_ADDITIONAL_HEIGHT);
				if (Utils.FGT(secondRackHeight, secondRack.ClearAvailableHeight))
					bSecondRackHeightError = true;
				secondRack.RackHeightWithTieBeam_IsMoreThan_MaxHeight = bSecondRackHeightError;

				// Racks can have 2 tie beams between them.
				// FirstRack - start & end frame.
				// SecondRack - start & end frame.
				// In this case 2 separate tie beams should be added - start and end frame tie beams.
				// Thats why we check start and end frame flags and create TieBeam separate.
				if (info.OppositeRacksDict[firstRack].HasFlag(eTieBeamFrame.eStartFrame) && info.OppositeRacksDict[secondRack].HasFlag(eTieBeamFrame.eStartFrame))
				{
					TieBeam startTieBeam = TieBeamUtils.CreateTieBeam(this, firstRack, secondRack, eTieBeamFrame.eStartFrame, eTieBeamFrame.eStartFrame);
					if (startTieBeam != null)
					{
						startTieBeam.AttachedRacksHeightError = bFirstRackHeightError || bSecondRackHeightError;
						TieBeamsList.Add(startTieBeam);

						if (!bFirstRackHeightError)
							firstRack.TieBeamFrame |= eTieBeamFrame.eStartFrame;

						if (!bSecondRackHeightError)
							secondRack.TieBeamFrame |= eTieBeamFrame.eStartFrame;
					}
				}
				if (info.OppositeRacksDict[firstRack].HasFlag(eTieBeamFrame.eEndFrame) && info.OppositeRacksDict[secondRack].HasFlag(eTieBeamFrame.eEndFrame))
				{
					TieBeam endTieBeam = TieBeamUtils.CreateTieBeam(this, firstRack, secondRack, eTieBeamFrame.eEndFrame, eTieBeamFrame.eEndFrame);
					if (endTieBeam != null)
					{
						endTieBeam.AttachedRacksHeightError = bFirstRackHeightError || bSecondRackHeightError;
						TieBeamsList.Add(endTieBeam);

						if (!bFirstRackHeightError)
							firstRack.TieBeamFrame |= eTieBeamFrame.eEndFrame;
						if (!bSecondRackHeightError)
							secondRack.TieBeamFrame |= eTieBeamFrame.eEndFrame;
					}
				}
				if (info.OppositeRacksDict[firstRack] != info.OppositeRacksDict[secondRack])
				{
					TieBeam tieBeam = TieBeamUtils.CreateTieBeam(this, firstRack, secondRack, info.OppositeRacksDict[firstRack], info.OppositeRacksDict[secondRack]);
					if (tieBeam != null)
					{
						tieBeam.AttachedRacksHeightError = bFirstRackHeightError || bSecondRackHeightError;
						TieBeamsList.Add(tieBeam);

						if (!bFirstRackHeightError)
							firstRack.TieBeamFrame |= info.OppositeRacksDict[firstRack];

						if (!bSecondRackHeightError)
							secondRack.TieBeamFrame |= info.OppositeRacksDict[secondRack];
					}
				}
			}
		}


		//=============================================================================
		// Check layout of the sheet.
		// Incorrect geometry is placed in the incorrectGeometryList.
		public bool IsLayoutCorrect(out List<BaseRectangleGeometry> incorrectGeometryList)
		{
			incorrectGeometryList = new List<BaseRectangleGeometry>();

			foreach(BaseRectangleGeometry geom in this.Rectangles)
			{
				if (geom == null)
					continue;

				// Dont check SheetElevationGeometry, it draws over all other geometry
				if (geom is SheetElevationGeometry)
					continue;

				// Dont check walls because they are placed around the graphics area and cant be moved.
				// So if wall overlaps any geometry inside the graphics area then need to move or delete this overlapped geometry.
				// Wall cant be moved.
				if (geom is Wall)
					continue;

				List<BaseRectangleGeometry> overlappedRectangles;
				if (!IsLayoutCorrect(geom, incorrectGeometryList, out overlappedRectangles))
				{
					incorrectGeometryList.Add(geom);
				}
			}

			if (incorrectGeometryList.Count > 0)
				return false;

			return true;
		}
		public bool IsLayoutCorrect()
		{
			List<BaseRectangleGeometry> overlappedRectangles;
			foreach (BaseRectangleGeometry geom in this.Rectangles)
			{
				if (geom == null)
					continue;

				if (!IsLayoutCorrect(geom, null, out overlappedRectangles))
					return false;
			}

			return true;
		}
		public bool IsLayoutCorrect(BaseRectangleGeometry rectangleToCheck)
		{
			List<BaseRectangleGeometry> overlappedRectangles;
			return IsLayoutCorrect(new List<BaseRectangleGeometry> { rectangleToCheck }, null, out overlappedRectangles);
		}
		public bool IsLayoutCorrect(BaseRectangleGeometry rectangleToCheck, List<BaseRectangleGeometry> rectanglesToIgnore, out List<BaseRectangleGeometry> overlappedRectangles)
		{
			return IsLayoutCorrect(new List<BaseRectangleGeometry> { rectangleToCheck }, rectanglesToIgnore, out overlappedRectangles);
		}
		public bool IsLayoutCorrect(List<BaseRectangleGeometry> rectanglesToCheck, List<BaseRectangleGeometry> rectanglesToIgnore, out List<BaseRectangleGeometry> overlappedRectangles)
		{
			return IsLayoutCorrect(rectanglesToCheck, null, rectanglesToIgnore, out overlappedRectangles);
		}
		/// <summary>
		/// rectanglesToIgnore - It is to expensive to change full racks row\column size and check layout and if it is not correct restore previous size.
		/// So create "test" rack row with new size and pass it as rectanglesToCheck, also pass original racks row\column as racksToIgnore because it will overlaps rectanglesToCheck.
		/// 
		/// temproraryRectangles
		/// rectanles that are not added to drawing yet, but rectanglesToCheck shouldnt overlap them
		/// </summary>
		public bool IsLayoutCorrect(List<BaseRectangleGeometry> rectanglesToCheck, List<BaseRectangleGeometry> temproraryRectangles, List<BaseRectangleGeometry> rectanglesToIgnore, out List<BaseRectangleGeometry> overlappedRectangles)
		{
			overlappedRectangles = new List<BaseRectangleGeometry>();

			if (Rectangles.Count < 1)
				return true;

			if (rectanglesToCheck == null)
				return false;

			if (rectanglesToCheck != null && rectanglesToCheck.Count < 1)
				return true;

			// get all rectangles
			List<BaseRectangleGeometry> childrenList = new List<BaseRectangleGeometry>();
			foreach (BaseRectangleGeometry _geom in Rectangles)
			{
				if (_geom != null)
					childrenList.Add(_geom);
			}
			//
			if (temproraryRectangles != null)
				childrenList.AddRange(temproraryRectangles);

			//
			// Racks cannot be overlapped by any other rectangles.
			// Shutter can be overlapped by ane rectangle, except Racks. Two shutters cannot overlap each other.
			// Block, aisle space cannot overlap each other.
			// Columns cannot overlap each other, but can draw on top of the block or aisle space.
			foreach (BaseRectangleGeometry rectToCheck in rectanglesToCheck)
			{
				foreach (BaseRectangleGeometry child in childrenList)
				{
					// rack row\column size changing
					// dont check racks in the same row\column 
					if (rectanglesToCheck.Contains(child))
						continue;

					if (rectanglesToIgnore != null && child != null && rectanglesToIgnore.Contains(child))
						continue;

					if (rectToCheck == child)
						continue;

					// SheetElevationGeometry can overlap all other geometry
					if (rectToCheck is SheetElevationGeometry || child is SheetElevationGeometry)
						continue;

					// wall can be overlapped by shutter
					if ((rectToCheck is Shutter && child is Wall) || (rectToCheck is Wall && child is Shutter))
						continue;
					// aisle space can overlaps swing door shutter
					if ((rectToCheck is Shutter && child is AisleSpace) || (rectToCheck is AisleSpace && child is Shutter))
						continue;

					// if geometry has MarginX or MarginY then need to check for overlap
					if (Utils.FGT(rectToCheck.MarginX, 0.0) || Utils.FGT(rectToCheck.MarginY, 0.0)
						|| Utils.FGT(child.MarginX, 0.0) || Utils.FGT(child.MarginY, 0.0))
					{
						if (child.IsOverlap(rectToCheck))
						{
							overlappedRectangles.Add(child);
							continue;
						}
					}

					// Rack cannot be overlapped by any rectangle.
					if (rectToCheck is Rack || child is Rack)
					{
						if (child.IsOverlap(rectToCheck))
						{
							overlappedRectangles.Add(child);
						}
					}
					else
					{
						// Shutter can be overlapped by ane rectangle, except Racks. Two shutters cannot overlap each other.
						if (rectToCheck is Shutter || child is Shutter)
						{
							if (rectToCheck is Shutter && child is Shutter)
							{
								if (child.IsOverlap(rectToCheck))
								{
									overlappedRectangles.Add(child);
								}
							}
						}
						// only column can overlap block and aisle space
						else if ((rectToCheck is Column && child is Column) || (!(rectToCheck is Column) && !(child is Column)))
						{
							if (child.IsOverlap(rectToCheck))
							{
								overlappedRectangles.Add(child);
							}
						}
					}

					//// check minimum distance between the same rotated racks
					//if (rectToCheck is Rack && child is Rack && rectToCheck.IsHorizontal == child.IsHorizontal)
					//{
					//	// if one rack is above\below other
					//	if (rectToCheck.IsHorizontal)
					//	{
					//		// dont calc min Y distance with any rack, we need only X-overlapped racks
					//		if ((Utils.FLE(rectToCheck.TopLeft_GlobalPoint.X, child.TopLeft_GlobalPoint.X) && child.TopLeft_GlobalPoint.X <= rectToCheck.TopRight_GlobalPoint.X)
					//			|| (child.TopLeft_GlobalPoint.X <= rectToCheck.TopLeft_GlobalPoint.X && rectToCheck.TopLeft_GlobalPoint.X <= child.TopRight_GlobalPoint.X))
					//		{
					//			if (rectToCheck.TopLeft_GlobalPoint.Y >= child.BottomLeft_GlobalPoint.Y)
					//			{
					//				if (Math.Abs(rectToCheck.TopLeft_GlobalPoint.Y - child.BottomLeft_GlobalPoint.Y) < Rack.sSameRotaionRakcs_MinimumGlobalDistance)
					//				{
					//					overlappedRectangles.Add(child);
					//					//return false;
					//				}
					//			}
					//			else if (rectToCheck.BottomLeft_GlobalPoint.Y <= child.TopLeft_GlobalPoint.Y)
					//			{
					//				if (Math.Abs(rectToCheck.BottomLeft_GlobalPoint.Y - child.TopLeft_GlobalPoint.Y) < Rack.sSameRotaionRakcs_MinimumGlobalDistance)
					//				{
					//					overlappedRectangles.Add(child);
					//				}
					//			}
					//		}
					//	}
					//	else
					//	{
					//		// dont calc min X distance with any rack, we need only Y-overlapped racks
					//		if ((rectToCheck.TopLeft_GlobalPoint.Y <= child.TopLeft_GlobalPoint.Y && child.TopLeft_GlobalPoint.Y <= rectToCheck.BottomLeft_GlobalPoint.Y)
					//			|| (child.TopLeft_GlobalPoint.Y <= rectToCheck.TopLeft_GlobalPoint.Y && rectToCheck.TopLeft_GlobalPoint.Y <= child.BottomLeft_GlobalPoint.Y))
					//		{
					//			// if one rack is left\right other
					//			if (rectToCheck.TopLeft_GlobalPoint.X >= child.TopRight_GlobalPoint.X)
					//			{
					//				if (Math.Abs(rectToCheck.TopLeft_GlobalPoint.X - child.TopRight_GlobalPoint.X) < Rack.sSameRotaionRakcs_MinimumGlobalDistance)
					//				{
					//					overlappedRectangles.Add(child);
					//					//return false;
					//				}
					//			}
					//			else if (rectToCheck.TopRight_GlobalPoint.X <= child.TopLeft_GlobalPoint.X)
					//			{
					//				if (Math.Abs(rectToCheck.TopRight_GlobalPoint.X - child.TopLeft_GlobalPoint.X) < Rack.sSameRotaionRakcs_MinimumGlobalDistance)
					//				{
					//					overlappedRectangles.Add(child);
					//					//return false;
					//				}
					//			}
					//		}
					//	}
					//}
				}
			}

			return overlappedRectangles.Count == 0;
		}

		//=============================================================================
		/// <summary>
		/// Creates new GUID for this sheet.
		/// Use this function only when open default document template.
		/// Otherwise you can have sheet with the same GUID in many documents, because they are just copy of the default document.
		/// </summary>
		public void CreateNewGUID()
		{
			m_GUID = Guid.NewGuid();
		}

		//=============================================================================
		/// <summary>
		/// Clear racks change order when command is ended and state is marked changed.
		/// </summary>
		public void OnDocumentStateChanged()
		{
			if (Rectangles == null)
				return;

			foreach(BaseRectangleGeometry geom in Rectangles)
			{
				if (geom == null)
					continue;

				Rack rackGeom = geom as Rack;
				if (rackGeom == null)
					continue;

				rackGeom.ClearChangeOrder();
			}
		}

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new DrawingSheet(this);
		}

		//=============================================================================
		/// <summary>
		/// Returns camera offset with temporary camera offset.
		/// </summary>
		public Vector GetCameraOffset() { return m_CameraOffset + m_TemporaryCameraOffset; }

		//=============================================================================
		/// <summary>
		/// Check that camera offset vector doesnt display part outside sheet.
		/// </summary>
		public void CheckCameraOffsetVector(double cameraWidthInPixels, double cameraHeightInPixels)
		{
			// If sheet is fully displayed then offset should be equal to 0.
			if (this.IsSheetFullyDisplayed)
			{
				this.CameraOffset = new Vector(0.0, 0.0);
				this.TemporaryCameraOffset = new Vector(0.0, 0.0);
				return;
			}

			double cameraPadding = DrawingSheet.GetCameraPadding();

			double cameraWidthInUnits = 0.0;
			if (m_Is_UnitsPerCameraPixel_Init)
				cameraWidthInUnits = cameraWidthInPixels * m_UnitsPerCameraPixel;
			double cameraHeightInUnits = 0.0;
			if (m_Is_UnitsPerCameraPixel_Init)
				cameraHeightInUnits = cameraHeightInPixels * m_UnitsPerCameraPixel;

			if (Utils.FLT(m_CameraOffset.X, 0.0 - cameraPadding))
				m_CameraOffset.X = 0.0 - cameraPadding;
			// If sheet width is less than camera width then horizontal offset should be equal to 0.
			bool checkHorizontalOffset = true;
			if (Utils.FLT(this.Length + 2 * cameraPadding, cameraWidthInUnits))
			{
				m_CameraOffset.X = 0.0;
				m_TemporaryCameraOffset.X = 0.0;
				checkHorizontalOffset = false;
			}

			if (Utils.FLT(m_CameraOffset.Y, 0.0 - cameraPadding))
				m_CameraOffset.Y = 0.0 - cameraPadding;
			// If sheet height is less than camera height then vertical offset should be equal to 0.
			bool checkVerticalOffset = true;
			if (Utils.FLT(this.Width + 2 * cameraPadding, cameraHeightInUnits))
			{
				m_CameraOffset.Y = 0.0;
				m_TemporaryCameraOffset.Y = 0.0;
				checkVerticalOffset = false;
			}

			if (checkHorizontalOffset && Utils.FGT(m_CameraOffset.X + cameraWidthInUnits, this.Length + cameraPadding))
				m_CameraOffset.X = this.Length + cameraPadding - cameraWidthInUnits;
			if (checkVerticalOffset && Utils.FGT(m_CameraOffset.Y + cameraHeightInUnits, this.Width + cameraPadding))
				m_CameraOffset.Y = this.Width + cameraPadding - cameraHeightInUnits;

			if (checkHorizontalOffset && Utils.FLT(m_TemporaryCameraOffset.X + m_CameraOffset.X, 0.0 - cameraPadding))
				m_TemporaryCameraOffset.X = 0.0 - cameraPadding - m_CameraOffset.X;
			if (checkHorizontalOffset && Utils.FGT(m_TemporaryCameraOffset.X + m_CameraOffset.X + cameraWidthInUnits, this.Length + cameraPadding))
				m_TemporaryCameraOffset.X = this.Length + cameraPadding - cameraWidthInUnits - m_CameraOffset.X;
			if (checkVerticalOffset && Utils.FLT(m_TemporaryCameraOffset.Y + m_CameraOffset.Y, 0.0 - cameraPadding))
				m_TemporaryCameraOffset.Y = 0.0 - cameraPadding - m_CameraOffset.Y;
			if (checkVerticalOffset && Utils.FGT(m_TemporaryCameraOffset.Y + m_CameraOffset.Y + cameraHeightInUnits, this.Width + cameraPadding))
				m_TemporaryCameraOffset.Y = this.Width + cameraPadding - cameraHeightInUnits - m_CameraOffset.Y;
		}

		//=============================================================================
		/// <summary>
		/// Changes camera settings and make sheet fully displayed.
		/// </summary>
		public void FullyDisplaySheet()
		{
			// Set UnitsPerCameraPixel to MaxUnitsPerCameraPixel and fully display sheet.
			// Probably camera is placed at the small area at the bot left part of the sheet, so
			// it doesnt display new sheet size.
			this.UnitsPerCameraPixel = this.MaxUnitsPerCameraPixel;
			this.CameraOffset = new Vector(0.0, 0.0);
			this.TemporaryCameraOffset = new Vector(0.0, 0.0);
		}

		//=============================================================================
		/// <summary>
		/// Returns horizontal and verical racks elevations lists.
		/// They contains only rack and used for create sheet elevation picture.
		/// 
		/// Racks in horizontalRacksElevationList are sorted from left to right.
		/// Racks in verticalRacksElevationList are sorted from bot to top. WPF Y-axis is directed to the bottom, (0,0) point is top left point.
		/// </summary>
		public bool GetSheetElevations(
			out List<Rack> horizontalRacksElevationList,
			out List<Rack> verticalRacksElevationList,
			out double horizontalSheetElevation_BiggestRackHeight,
			out double verticalSheetElevation_BiggestRackHeight)
		{
			Point sheetElevationPoint = new Point(this.Length / 2, this.Width / 2);
			// Get sheet elevation point coordinates from SheetElevationGeometry
			SheetElevationGeometry sheetElevationGeom = null;
			if (this.Rectangles != null)
				sheetElevationGeom = this.Rectangles.FirstOrDefault(geom => geom is SheetElevationGeometry) as SheetElevationGeometry;
			if (sheetElevationGeom != null)
			{
				sheetElevationPoint.X = sheetElevationGeom.TopLeft_GlobalPoint.X;
				sheetElevationPoint.Y = sheetElevationGeom.TopLeft_GlobalPoint.Y;
			}

			return this.GetSheetElevations(sheetElevationPoint, out horizontalRacksElevationList, out verticalRacksElevationList, out horizontalSheetElevation_BiggestRackHeight, out verticalSheetElevation_BiggestRackHeight);
		}
		public bool GetSheetElevations(
			Point sheetElevationPoint,
			out List<Rack> horizontalRacksElevationList,
			out List<Rack> verticalRacksElevationList,
			out double horizontalSheetElevation_BiggestRackHeight,
			out double verticalSheetElevation_BiggestRackHeight)
		{
			horizontalRacksElevationList = new List<Rack>();
			verticalRacksElevationList = new List<Rack>();
			horizontalSheetElevation_BiggestRackHeight = 0.0;
			verticalSheetElevation_BiggestRackHeight = 0.0;

			// find racks for sheet elevations
			foreach(BaseRectangleGeometry geom in this.Rectangles)
			{
				if (geom == null)
					continue;

				//// SheetGeometry is placed at warehouse sheet only.
				//// Need to get sheet elevations from the bound sheet using Warehouse sheet elevation point.
				//SheetGeometry sheetGeometry = geom as SheetGeometry;
				//if (sheetGeometry != null)
				//{
				//	DrawingSheet boundSheet = sheetGeometry.BoundSheet;
				//	if(boundSheet != null)
				//	{
				//		Point boundSheet_SheetElevationPoint = new Point(0.0, 0.0) + (sheetElevationPoint - sheetGeometry.TopLeft_GlobalPoint);
				//		// Point can be outside of the bound sheet
				//		//if (Utils.FGT(boundSheet_SheetElevationPoint.X, boundSheet.Length) || Utils.FGT(boundSheet_SheetElevationPoint.Y, boundSheet.Width))
				//		//	continue;
				//
				//		List<Rack> boundSheet_horizontalRacksElevationList;
				//		List<Rack> boundSheet_verticalRacksElevationList;
				//		boundSheet.GetSheetElevations(boundSheet_SheetElevationPoint, out boundSheet_horizontalRacksElevationList, out boundSheet_verticalRacksElevationList);
				//
				//		if (boundSheet_horizontalRacksElevationList != null && boundSheet_horizontalRacksElevationList.Count > 0)
				//			horizontalRacksElevationList.AddRange(boundSheet_horizontalRacksElevationList);
				//		if (boundSheet_verticalRacksElevationList != null && boundSheet_verticalRacksElevationList.Count > 0)
				//			verticalRacksElevationList.AddRange(boundSheet_verticalRacksElevationList);
				//	}
				//
				//	continue;
				//}

				Rack rackGeom = geom as Rack;
				if (rackGeom == null)
					continue;

				// horizontal sheet elevation
				if (Utils.FGE(sheetElevationPoint.Y, rackGeom.TopLeft_GlobalPoint.Y) && Utils.FLE(sheetElevationPoint.Y, rackGeom.BottomRight_GlobalPoint.Y))
					horizontalRacksElevationList.Add(rackGeom);
				// vertical sheet elevation
				if (Utils.FGE(sheetElevationPoint.X, rackGeom.TopLeft_GlobalPoint.X) && Utils.FLE(sheetElevationPoint.X, rackGeom.BottomRight_GlobalPoint.X))
					verticalRacksElevationList.Add(rackGeom);
			}

			// sort lists
			//if (this is WarehouseSheet)
			//{
			//	horizontalRacksElevationList = horizontalRacksElevationList.OrderBy(
			//		rack =>
			//		{
			//			double offsetX = rack.TopLeft_GlobalPoint.X;
			//			if (rack.Sheet != null && rack.Sheet.BoundSheetGeometry != null)
			//				offsetX += rack.Sheet.BoundSheetGeometry.TopLeft_GlobalPoint.X;
			//
			//			return offsetX;
			//		}
			//		).ToList();
			//
			//	verticalRacksElevationList = verticalRacksElevationList.OrderByDescending(
			//		rack =>
			//		{
			//			double offsetY = rack.BottomRight_GlobalPoint.Y;
			//			if (rack.Sheet != null && rack.Sheet.BoundSheetGeometry != null)
			//				offsetY += rack.Sheet.BoundSheetGeometry.BottomRight_GlobalPoint.Y;
			//
			//			return offsetY;
			//		}
			//		).ToList();
			//}
			//else
			{
				horizontalRacksElevationList = horizontalRacksElevationList.OrderBy(rack => rack.TopLeft_GlobalPoint.X).ToList();
				verticalRacksElevationList = verticalRacksElevationList.OrderByDescending(rack => rack.BottomRight_GlobalPoint.Y).ToList();
			}

			horizontalSheetElevation_BiggestRackHeight = RackUtils.GetTheBiggestRackHeight(horizontalRacksElevationList);
			verticalSheetElevation_BiggestRackHeight = RackUtils.GetTheBiggestRackHeight(verticalRacksElevationList);

			return true;
		}

		#endregion

		#region Private Functions

		//=============================================================================
		private void _PlaceAtCenterForInit(BaseRectangleGeometry geometry)
		{
			DrawingControl dc = DrawingDocument._sDrawing;
			if (dc == null)
				return;

			if (geometry == null)
				return;

			Point GraphAreaCenter = new Point(0, 0);
			if (!this.IsSheetFullyDisplayed)
			{
				Point graphAreaTopLeftPnt = dc.GetGlobalPoint(this, new Point(0.0, 0.0));
				Point graphAreaBotRightPnt = dc.GetGlobalPoint(this, new Point(dc.ActualWidth, dc.ActualHeight));
				GraphAreaCenter = graphAreaTopLeftPnt + (graphAreaBotRightPnt - graphAreaTopLeftPnt) / 2;
			}
			else
			{
				GraphAreaCenter.X += this.Length / 2;
				GraphAreaCenter.Y += this.Width / 2;
			}

			Point newTopLeftPoint = GraphAreaCenter;
			newTopLeftPoint.X -= geometry.Length_X / 2;
			newTopLeftPoint.Y -= geometry.Length_Y / 2;
			// check borders
			if (Utils.FLT(newTopLeftPoint.X, 0.0))
				newTopLeftPoint.X = 0.0;
			if (Utils.FLT(newTopLeftPoint.Y, 0.0))
				newTopLeftPoint.Y = 0.0;
			if (Utils.FGT(newTopLeftPoint.X + geometry.Length_X, this.Length))
				newTopLeftPoint.X = this.Length - geometry.Length_X;
			if (Utils.FGT(newTopLeftPoint.Y + geometry.Length_Y, this.Width))
				newTopLeftPoint.Y = this.Width - geometry.Length_Y;
			//
			newTopLeftPoint = Utils.GetWholePoint(newTopLeftPoint);

			geometry.TopLeft_GlobalPoint = newTopLeftPoint;
			geometry.IsInit = false;
		}
		//=============================================================================
		private void _AfterCreateNewGeom(BaseRectangleGeometry newGeom)
		{
			if (newGeom == null)
				return;

			if (DrawingDocument._sDrawing == null)
				return;

			if (Document != null)
				Document.IsInCommand = true;

			//
			newGeom.Sheet = this;
			_PlaceAtCenterForInit(newGeom);
			AddGeometry(newGeom);
			//
			SelectedGeometryCollection.Clear();
			SelectedGeometryCollection.Add(newGeom);

			if (DrawingDocument._sDrawing != null)
				DrawingDocument._sDrawing.UpdateDrawing(true);
		}
		//=============================================================================
		private void _UpdateRackRowsColumns()
		{
			_UpdateRackRowsColumns(true, true, true);
		}
		private void _UpdateRackRowsColumns(bool bRegroupRacks, bool bUpdateRacks, bool bCheckColumns)
		{
			if (Document == null)
				return;

			if (!bRegroupRacks && !bUpdateRacks && !bCheckColumns)
				return;

			List<Rack> allRacks = this.GetAllRacks();
			bool bUpdate = false;
			if (m_CalculationState.HasFlag(eCalculationState.eForceRecalculateRacksGroups))
			{
				bUpdate = true;
				m_CalculationState &= ~eCalculationState.eForceRecalculateRacksGroups;
			}
			if (!bUpdate)
			{
				foreach (Rack rack in allRacks)
				{
					if (rack == null)
						continue;

					if (rack.SheetUpdateNumber > m_UpdateCount)
					{
						bUpdate = true;
						break;
					}
				}
			}

			if (!bUpdate)
				return;

			// update racks groups
			if (bUpdate && bRegroupRacks)
			{
				List<List<Rack>> racks_RowsColumns;
				_GroupRacks(allRacks, out racks_RowsColumns);
				RacksGroups = racks_RowsColumns;

				++m_UpdateCount;
			}

			// update racks
			if (bUpdateRacks)
			{
				foreach (List<Rack> group in RacksGroups)
				{
					for (int i = 0; i < group.Count; ++i)
					{
						//
						bool bIsFirst = false;
						if (i == 0)
							bIsFirst = true;

						// if layout is not correct then doesnt apply new sizes to rack
						if (!group[i].IsFirstInRowColumn && bIsFirst)
							RackUtils.Convert_A_to_M_Rack(this, group[i], true, true, true);
						// if there is M rack appended to the end of row\column, convert it to A
						if (group[i].IsFirstInRowColumn && !bIsFirst && (i == group.Count - 1))
							RackUtils.Convert_A_to_M_Rack(this, group[i], false, true, true);

						//
						group[i].IsFirstInRowColumn = bIsFirst;

						// can create row
						if (i == group.Count - 1)
							group[i].CanCreateRow = true;
						else
							group[i].CanCreateRow = false;

						//
						bool bShowRotationGrips = group.Count == 1;
						group[i].ShowRotationGrips = bShowRotationGrips;
					}
				}
			}

			//
			if(bCheckColumns)
			{
				this.CheckRacksColumnSizeAndBracingType(false);
				_UpdateRackRowsColumns(bRegroupRacks, bUpdateRacks, false);
			}
		}
		//=============================================================================
		public static bool _GroupRacks(List<Rack> racks, out List<List<Rack>> _racks_RowsColumns)
		{
			_racks_RowsColumns = new List<List<Rack>>();
			if (racks == null)
				return false;

			//
			HashSet<Rack> racksToSkip = new HashSet<Rack>();

			foreach (Rack r in racks)
			{
				if (racksToSkip.Contains(r))
					continue;

				racksToSkip.Add(r);

				// try to add it to group
				bool bAdded = false;
				foreach (List<Rack> group in _racks_RowsColumns)
				{
					if (group.Count == 0)
						continue;

					// row or column is it?
					bool bRow = r.IsHorizontal;

					Rack firstRack = group[0];

					// check rotation
					if (r.IsHorizontal != firstRack.IsHorizontal)
						continue;

					// all racks in the row must have the same height
					// all racks in the column must have the same width
					if (bRow)
					{
						if (Utils.FNE(r.Length_Y, firstRack.Length_Y))
							continue;

						if (Utils.FNE(r.TopLeft_GlobalPoint.Y, firstRack.TopLeft_GlobalPoint.Y))
							continue;
					}
					else
					{
						if (Utils.FNE(r.Length_X, firstRack.Length_X))
							continue;

						if (Utils.FNE(r.TopLeft_GlobalPoint.X, firstRack.TopLeft_GlobalPoint.X))
							continue;
					}

					if (bRow)
					{
						// try to add to the start
						if (Utils.FLE(Math.Abs(r.TopRight_GlobalPoint.X - firstRack.TopLeft_GlobalPoint.X), Rack.sHorizontalRow_GlobalGap))
						{
							group.Insert(0, r);
							bAdded = true;
							break;
						}

						// try to add to the end 
						Rack lastRack = group[group.Count - 1];
						if (Utils.FLE(Math.Abs(r.TopLeft_GlobalPoint.X - lastRack.TopRight_GlobalPoint.X), Rack.sHorizontalRow_GlobalGap))
						{
							group.Add(r);
							bAdded = true;
							break;
						}
					}
					else
					{
						// try to add to the start
						if (Utils.FLE(Math.Abs(r.BottomLeft_GlobalPoint.Y - firstRack.TopLeft_GlobalPoint.Y), Rack.sHorizontalRow_GlobalGap))
						{
							group.Insert(0, r);
							bAdded = true;
							break;
						}

						// try to add to the end 
						Rack lastRack = group[group.Count - 1];
						if (Utils.FLE(Math.Abs(r.TopLeft_GlobalPoint.Y - lastRack.BottomLeft_GlobalPoint.Y), Rack.sHorizontalRow_GlobalGap))
						{
							group.Add(r);
							bAdded = true;
							break;
						}
					}
				}

				//
				if (!bAdded)
				{
					List<Rack> newGroup = new List<Rack>();
					newGroup.Add(r);
					_racks_RowsColumns.Add(newGroup);
				}
				else if (_racks_RowsColumns.Count > 1)
				{
					// after each adding new rack in the group need to try merge groups
					bool bMerged = false;
					do
					{
						bMerged = false;

						//
						foreach (List<Rack> group_1 in _racks_RowsColumns)
						{
							foreach (List<Rack> group_2 in _racks_RowsColumns)
							{
								if (group_1 != group_2 && group_1.Count > 0 && group_2.Count > 0)
								{
									// check rotation
									if (group_1[0].IsHorizontal != group_2[0].IsHorizontal)
										continue;

									//
									Rack group_1_first = group_1[0];
									Rack group_1_last = group_1[group_1.Count - 1];
									//
									Rack group_2_first = group_2[0];
									Rack group_2_last = group_2[group_2.Count - 1];

									// check sizes
									// all racks in the row must have the same height
									// all racks in the column must have the same width
									if (group_1_first.IsHorizontal)
									{
										if (Utils.FNE(group_1_first.Length_Y, group_2_first.Length_Y))
											continue;
									}
									else
									{
										if (Utils.FNE(group_1_first.Length_X, group_2_first.Length_X))
											continue;
									}

									// row merge
									if (group_1_first.IsHorizontal)
									{
										if (Utils.FNE(group_1_first.TopLeft_GlobalPoint.Y, group_2_first.TopLeft_GlobalPoint.Y))
											continue;

										// try to add group2 to the end of group1
										if (Utils.FLE(Math.Abs(group_2_first.TopLeft_GlobalPoint.X - group_1_last.TopRight_GlobalPoint.X), Rack.sHorizontalRow_GlobalGap))
										{
											group_1.AddRange(group_2);
											_racks_RowsColumns.Remove(group_2);
											bMerged = true;
											break;
										}

										// try to add group1 to the end of group2
										if (Utils.FLE(Math.Abs(group_1_first.TopLeft_GlobalPoint.X - group_2_last.TopRight_GlobalPoint.X), Rack.sHorizontalRow_GlobalGap))
										{
											group_2.AddRange(group_1);
											_racks_RowsColumns.Remove(group_1);
											bMerged = true;
											break;
										}
									}
									else
									{
										// column merge

										//
										if (Utils.FNE(group_1_first.TopLeft_GlobalPoint.X, group_2_first.TopLeft_GlobalPoint.X))
											continue;

										// try to add group2 to the end of group1
										if (Utils.FLE(Math.Abs(group_2_first.TopLeft_GlobalPoint.Y - group_1_last.BottomLeft_GlobalPoint.Y), Rack.sHorizontalRow_GlobalGap))
										{
											group_1.AddRange(group_2);
											_racks_RowsColumns.Remove(group_2);
											bMerged = true;
											break;
										}

										// try to add group1 to the end of group2
										if (Utils.FLE(Math.Abs(group_1_first.TopLeft_GlobalPoint.Y - group_2_last.BottomLeft_GlobalPoint.Y), Rack.sHorizontalRow_GlobalGap))
										{
											group_2.AddRange(group_1);
											_racks_RowsColumns.Remove(group_1);
											bMerged = true;
											break;
										}
									}
								}
							}

							if (bMerged)
								break;
						}
					} while (bMerged);
				}
			}

			return true;
		}

		//=============================================================================
		/// <summary>
		/// Update snapping lines arrays - m_HorizontalLines and m_VerticalLines.
		/// </summary>
		private void _UpdateSnappingLines()
		{
			//
			m_HorizontalLines.Clear();
			m_VerticalLines.Clear();

			//
			if (DrawingDocument._sDrawing == null)
				return;

			// check column pattern
			List<Column> _selectedColumnPattern = new List<Column>();
			foreach(BaseRectangleGeometry geom in SelectedGeometryCollection)
			{
				if (geom == null)
					continue;

				Column column = geom as Column;
				if (column != null && column.Pattern != null)
				{
					Point patternStartPoint;
					int columnsCount;
					int rowsCount;
					List<Column> patternColumnsList;
					List<List<Column>> patternRows;
					if (column.Pattern.GetPatternInfo(out patternStartPoint, out columnsCount, out rowsCount, out patternColumnsList, out patternRows))
						_selectedColumnPattern.AddRange(patternColumnsList);
				}
			}

			// get lines
			foreach (BaseRectangleGeometry _geom in Rectangles)
			{
				if (_geom == null)
					continue;

				if (SelectedGeometryCollection != null && SelectedGeometryCollection.Contains(_geom))
					continue;

				Column columnGeom = _geom as Column;
				if (columnGeom != null && _selectedColumnPattern.Contains(columnGeom))
					continue;

				// left
				_AddVerticalLine(this.Length, _geom.TopLeft_GlobalPoint);
				// right
				_AddVerticalLine(this.Length, _geom.TopRight_GlobalPoint);
				// top
				_AddHorizontalLine(this.Width, _geom.TopLeft_GlobalPoint);
				// bottom
				_AddHorizontalLine(this.Width, _geom.BottomLeft_GlobalPoint);
				// center
				_AddVerticalLine(this.Length, _geom.Center_GlobalPoint);
				_AddHorizontalLine(this.Width, _geom.Center_GlobalPoint);
			}
		}

		//=============================================================================
		/// <summary>
		/// Updates racks and pallets statistics collections.
		/// </summary>
		public void UpdateStatisticsCollections()
		{
			if (m_RackStatistics == null || m_PalletsStatistics == null)
				return;

			// Update rack statistics
			List<StatRackItem> rackStatItemsToDelete = new List<StatRackItem>();
			rackStatItemsToDelete.AddRange(m_RackStatistics);

			// remove old stat
			m_RackStatistics.Clear();
			m_PalletsStatistics.Clear();

			//
			List<Rack> racksList = this.GetAllRacks();
			foreach (Rack rackGeom in racksList)
			{
				if (rackGeom == null)
					continue;

				//
				StatRackItem foundedItem = null;
				foreach (StatRackItem statItem in m_RackStatistics)
				{
					if (statItem.RackIndex == rackGeom.SizeIndex)
					{
						foundedItem = statItem;
						break;
					}
				}

				//
				if (foundedItem == null)
				{
					foundedItem = new StatRackItem(rackGeom.SizeIndex, m_RackStatistics);
					// init length
					if(rackGeom.IsFirstInRowColumn)
					{
						foundedItem.Length_M = Utils.GetWholeNumber(rackGeom.Length);
						foundedItem.Length_A = Utils.GetWholeNumber((double)foundedItem.Length_M - rackGeom.DiffBetween_M_and_A);
					}
					else
					{
						foundedItem.Length_A = Utils.GetWholeNumber(rackGeom.Length);
						foundedItem.Length_M = Utils.GetWholeNumber((double)foundedItem.Length_A + rackGeom.DiffBetween_M_and_A);
					}
					// init width
					foundedItem.Width = Utils.GetWholeNumber(rackGeom.Depth);
					// init height
					foundedItem.Height = rackGeom.Length_Z;
					// init load
					foundedItem.Load = (int)rackGeom.RackLoad;

					m_RackStatistics.Add(foundedItem);
				}

				if (rackGeom.IsFirstInRowColumn)
					// it is M
					++foundedItem.Count_M;
				else
					// it is A
					++foundedItem.Count_A;


				// update pallets
				if (rackGeom.ShowPallet)
				{
					if (rackGeom.Levels == null)
						continue;
					foreach (RackLevel level in rackGeom.Levels)
					{
						if (level == null)
							continue;

						if (level.Pallets == null)
							continue;

						foreach (Pallet pallet in level.Pallets)
						{
							if (pallet == null)
								continue;

							StatPalletItem foundedPalletStatItem = null;
							foreach (StatPalletItem _palletStatItem in m_PalletsStatistics)
							{
								if (_palletStatItem == null)
									continue;

								if (_palletStatItem.Length == pallet.Length
									&& _palletStatItem.Width == pallet.Width
									&& _palletStatItem.Height == pallet.Height
									&& _palletStatItem.Load == pallet.Load)
								{
									foundedPalletStatItem = _palletStatItem;
									break;
								}
							}

							if (foundedPalletStatItem == null)
							{
								foundedPalletStatItem = new StatPalletItem(m_PalletsStatistics.Count);

								foundedPalletStatItem.Length = (int)pallet.Length;
								foundedPalletStatItem.Width = (int)pallet.Width;
								foundedPalletStatItem.Height = (int)pallet.Height;
								foundedPalletStatItem.Load = (int)pallet.Load;

								m_PalletsStatistics.Add(foundedPalletStatItem);
							}

							++foundedPalletStatItem.Count;
						}
					}
				}
			}

			//
			foreach (StatRackItem itemToDelete in rackStatItemsToDelete)
			{
				m_RackStatistics.Remove(itemToDelete);
			}

			//
			foreach (StatRackItem statItem in m_RackStatistics)
			{
				if (statItem == null)
					continue;

				statItem.Update_DisplayIndex();
			}
		}

		//=============================================================================
		/// <summary>
		/// Ask user for change columns with the same index in other patterns.
		/// </summary>
		public void OnColumnSizeChanged(Column changedColumn)
		{
			if (Document == null)
				return;

			if (changedColumn == null)
				return;

			if (DrawingDocument._sDrawing == null)
				return;

			// zero-based
			int iOldSizeIndex = changedColumn.SizeIndex;

			// change name of all columns in the pattern
			ColumnPattern pattern = changedColumn.Pattern;
			if (pattern == null)
				return;

			Point patternStartPoint;
			int columnsCount;
			int rowsCount;
			List<Column> patternColumnsList;
			List<List<Column>> patternRows;
			if (!pattern.GetPatternInfo(out patternStartPoint, out columnsCount, out rowsCount, out patternColumnsList, out patternRows))
				return;

			// try to find columns with the same text
			List<Column> allColumnsList = this.GetAllColumns();
			List<Column> sameSizeIndexColumnsList = new List<Column>();
			foreach (Column c in allColumnsList)
			{
				if (patternColumnsList.Contains(c))
					continue;

				if (c.Length_X == changedColumn.Length_X && c.Length_Y == changedColumn.Length_Y)
					continue;

				if (changedColumn.SizeIndex == c.SizeIndex)
					sameSizeIndexColumnsList.Add(c);
			}

			if (sameSizeIndexColumnsList.Count == 0)
			{
				// There is no columns with the same size in another patterns.
				// So need change old size to new size in m_ColumnsUniqueSizes list.
				Document.RecalcColumnUniqueSize(patternColumnsList);

				return;
			}

			// show message
			if (MessageBox.Show("There are another columns with the same size. Do you want to change them?", "Changing columns", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				foreach (Column c in sameSizeIndexColumnsList)
				{
					if (c == null)
						continue;

					pattern = c.Pattern;
					if (pattern == null)
						continue;

					if (!pattern.GetPatternInfo(out patternStartPoint, out columnsCount, out rowsCount, out patternColumnsList, out patternRows))
						continue;

					c.ChangeSize(this.Length, this.Width, true, changedColumn.Length, true, c.Depth, false, false, false, true);
				}

				// update index of all changed columns
				Document.RecalcColumnUniqueSize(sameSizeIndexColumnsList);
			}

			// update size index in changed column's pattern
			Document.RecalcColumnUniqueSize(patternColumnsList);
		}



		//=============================================================================
		private void _AddVerticalLine(double rGlobalLength, Point pnt)
		{
			if (pnt.X < 0 || pnt.X > rGlobalLength)
				return;

			if (!m_VerticalLines.Contains(pnt.X))
				m_VerticalLines.Add(pnt.X);
		}
		//=============================================================================
		private void _AddHorizontalLine(double rGlobalWidth, Point pnt)
		{
			if (pnt.Y < 0 || pnt.Y > rGlobalWidth)
				return;

			if (!m_HorizontalLines.Contains(pnt.Y))
				m_HorizontalLines.Add(pnt.Y);
		}



		//=============================================================================
		/// <summary>
		/// Mark state of this sheet changed and notify Document.
		/// </summary>
		public void MarkStateChanged()
		{
			if (!m_ChangedSheetsGuidsSet.Contains(this.GUID))
				m_ChangedSheetsGuidsSet.Add(this.GUID);
			NotifyPropertyChanged(() => DisplayName);

			if (Document == null)
				return;

			CheckWalls();
			CheckAisleSpaces();

			//
			foreach(BaseRectangleGeometry geom in NonInitSelectedGeometryList)
			{
				if (geom == null)
					continue;

				if (!IsLayoutCorrect(geom))
					return;
			}

			//
			foreach (BaseRectangleGeometry geom in NonInitSelectedGeometryList)
			{
				if (geom == null)
					continue;

				// Mark geometry initialized, otherwise Rack.ClearAvailableHeight will return incorrect value.
				geom.IsInit = true;

				Column newColumn = geom as Column;
				if (newColumn != null)
					newColumn.SizeIndex = Document.AddColumnUniqueSize(newColumn);

				Rack newRack = geom as Rack;
				if (newRack != null)
				{
					newRack.CheckRackHeight();
					newRack.SizeIndex = Document.AddRackUniqueSize(newRack);
				}
			}
			// dont clear selected geometry
			// SelectedGeometryCollection.Clear();

			// Recalculate M and A racks in the groups.
			// Call CheckRacksGroups() before _UpdateRackRowsColumns(), because
			// _UpdateRackRowsColumns() only change IsFirstInRowColumn flag and dont change rack's size if result layout is not correct.
			// CheckRacksGroups() try to change rack's size and make result layout correct.
			List<Rack> deletedRacks;
			this.CheckRacksGroups(out deletedRacks);

            // Search collisions of aisle spaces with racks for row and column guards directions
            if (Document.Rack_Accessories.RowGuard || Document.Rack_Accessories.UprightGuard)
            {
				CheckAisleSpacesAndRacksCollisions();
			}

			// CheckRacksColumnSizeAndBracingType() calculates column for racks group, so need to update racks groups before call it.
			// Probably new rack was inserted to the drawing.
			this._UpdateRackRowsColumns();
			CheckRacksColumnSizeAndBracingType(false);

			//
			_UpdateSheet();

			// update tie beams
			CheckTieBeams();
			// update snapping lines
			_UpdateSnappingLines();

			//
			Document.MarkStateChanged();
		}
		//=============================================================================
		public void _UpdateSheet()
		{
			_UpdateRackRowsColumns();
		}
		//=============================================================================
		public void RegroupRacks()
		{
			_UpdateRackRowsColumns(true, false, false);
		}



		//=============================================================================
		private ColumnPattern _CreateColumnPattern()
		{
			ColumnPattern _pattern = new ColumnPattern(this);
			int iNewPatternID = _GetFreePatternID();
			if (iNewPatternID < 0)
				return null;

			_pattern.ID = iNewPatternID;

			m_patterns.Add(_pattern);

			return _pattern;
		}
		//=============================================================================
		private int _GetFreePatternID()
		{
			List<int> _existPatternID = new List<int>();
			foreach (ColumnPattern p in m_patterns)
			{
				if (!_existPatternID.Contains(p.ID))
					_existPatternID.Add(p.ID);
			}

			for (int i = 0; i < ColumnPattern.sMaxPatternID; ++i)
			{
				if (!_existPatternID.Contains(i))
					return i;
			}

			return -1;
		}



		//=============================================================================
		/// <summary>
		/// Initialize Name property with string like "Sheet" + number.
		/// </summary>
		public void _InitName()
		{
			// init name
			string strName = "Sheet";
			if (Document != null && Document.Sheets != null)
			{
				for (int i = 1; i < 9999; ++i)
				{
					string strTempName = strName;
					strTempName += i.ToString();

					//
					bool bFoundWithSameName = false;
					foreach (DrawingSheet _sheet in Document.Sheets)
					{
						if (_sheet == null)
							continue;

						if (_sheet == this)
							continue;

						if(_sheet.Name == strTempName)
						{
							bFoundWithSameName = true;
							break;
						}
					}

					if(!bFoundWithSameName)
					{
						strName = strTempName;
						break;
					}
				}
			}

			m_Name = strName;
			//
			NotifyPropertyChanged(() => Name);
			NotifyPropertyChanged(() => DisplayName);
		}


		//=============================================================================
		private void SelectedGeometryCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			//
			//if(this.SelectedGeometryCollection != null && this.SelectedGeometryCollection.Count > 1)
			//{
			//	if (Document != null)
			//		Document.ShowAdvancedProperties = false;
			//}

			// change IsSelected property
			foreach(BaseRectangleGeometry geom in this.Rectangles)
			{
				if (geom == null)
					continue;

				geom.IsSelected = false;
			}
			//
			foreach(BaseRectangleGeometry geom in this.SelectedGeometryCollection)
			{
				if (geom == null)
					continue;

				geom.IsSelected = true;
			}

			_UpdateSnappingLines();
			if (DrawingDocument._sDrawing != null)
			{
				DrawingDocument._sDrawing.ResetGrips();
				DrawingDocument._sDrawing.InvalidateVisual();
			}

			NotifyPropertyChanged(() => SelectedGeometryCollection);
			NotifyPropertyChanged(() => SingleSelectedGeometry);
		}

		//=============================================================================
		// Check walls.
		// Stretches walls, if there is two wall near.
		private void CheckWalls()
		{
			// just check top and bot walls
			//
			// top wall
			Wall topWall = this.Rectangles.FirstOrDefault(r => r is Wall && ((Wall)r).WallPosition == eWallPosition.eTop) as Wall;
			if (topWall != null)
			{
				Point topLeftPnt = new Point(0.0, 0.0);
				topLeftPnt.Y -= topWall.Length_Y;

				topWall.TopLeft_GlobalPoint = topLeftPnt;
				topWall.Length_X = this.Length;
			}
			// bot wall
			Wall botWall = this.Rectangles.FirstOrDefault(r => r is Wall && ((Wall)r).WallPosition == eWallPosition.eBot) as Wall;
			if (botWall != null)
			{
				Point topLeftPnt = new Point(0.0, this.Width);
				botWall.TopLeft_GlobalPoint = topLeftPnt;
				botWall.Length_X = this.Length;
			}

			// stretch only left and right wall

			// left wall
			Wall leftWall = this.Rectangles.FirstOrDefault(r => r is Wall && ((Wall)r).WallPosition == eWallPosition.eLeft) as Wall;
			if(leftWall != null)
			{
				Point topLeftPnt = new Point(-leftWall.Length_X, 0.0);
				double lengthY = this.Width;

				// if top wall exists then stretch left wall to fill the top left rectangle
				if (topWall != null)
				{
					topLeftPnt.Y = -topWall.Length_Y;
					lengthY += topWall.Length_Y;
				}

				// if bot wall exists then stretch left wall to fill the bot left rectangle
				if (botWall != null)
					lengthY += botWall.Length_Y;

				leftWall.TopLeft_GlobalPoint = topLeftPnt;
				leftWall.Length_Y = lengthY;
			}

			// right wall
			Wall rightWall = this.Rectangles.FirstOrDefault(r => r is Wall && ((Wall)r).WallPosition == eWallPosition.eRight) as Wall;
			if (rightWall != null)
			{
				Point topLeftPnt = new Point(this.Length, 0.0);
				double lengthY = this.Width;

				// if top wall exists then stretch right wall to fill the top right rectangle
				if (topWall != null)
				{
					topLeftPnt.Y = -topWall.Length_Y;
					lengthY += topWall.Length_Y;
				}

				// if bot wall exists then stretch right wall to fill the bot right rectangle
				if (botWall != null)
					lengthY += botWall.Length_Y;

				rightWall.TopLeft_GlobalPoint = topLeftPnt;
				rightWall.Length_Y = lengthY;
			}
		}

		//=============================================================================
		private void _MarkCalculationStateChanged(eCalculationState state)
		{
			if (!m_CalculationState.HasFlag(state))
				m_CalculationState |= state;
		}

		//=============================================================================
		// default roof list
		private void _InitRoofList()
		{
			FlatRoof flatRoof = new FlatRoof();
			flatRoof.IsSelected = true;
			m_RoofsList.Add(flatRoof);
			m_RoofsList.Add(new GableRoof());
			m_RoofsList.Add(new ShedRoof());
		}


		//=============================================================================
		private static bool _isCameraPaddingInit = false;
		/// <summary>
		/// Additional padding around sheet.
		/// If user set camera scale to 2 and starts to change camera position then
		/// user can see the sheet from (0, 0) point to (sheet.Length, sheet.Width) point.
		/// All geometry outside this area will be cut. But it means that shutters and walls will be cut, because
		/// they are placed outside sheet.
		/// 
		/// How to display shutters and walls?
		/// Lets allow user to place camera from (-_cameraPadding, -_cameraPadding) point to (sheet.Length + _cameraPadding, sheet.Width + _cameraPadding) point.
		/// And set the double max wall or shutter width as _cameraPadding.
		/// </summary>
		private static double _cameraPadding = 0.0;
		public static double GetCameraPadding()
		{
			if(!_isCameraPaddingInit)
			{
				_cameraPadding = Math.Max(Wall.THICKNESS, Shutter.SHUTTER_DEPTH);
				_cameraPadding *= 2;
				_isCameraPaddingInit = true;
			}

			return _cameraPadding;
		}

		//=============================================================================
		/// <summary>
		/// Each sheet should have only one instance of SheetElevationGeometry.
		/// If this sheet doesnt have it, then it will be added.
		/// If this sheet has more than 1 instance, then other instances will be deleted.
		/// If SheetElevationGeometry is outside sheet, then it will be placed at the sheet center.
		/// </summary>
		private void CheckSheetElevationGeometry()
		{
			if (this.Rectangles == null)
				this.Rectangles = new List<BaseRectangleGeometry>();

			List<SheetElevationGeometry> sheetElevationsGeometriesList = new List<SheetElevationGeometry>();
			foreach(BaseRectangleGeometry geom in this.Rectangles)
			{
				if (geom == null)
					continue;

				SheetElevationGeometry sheetElevationGeom = geom as SheetElevationGeometry;
				if (sheetElevationGeom != null)
					sheetElevationsGeometriesList.Add(sheetElevationGeom);
			}

			if(sheetElevationsGeometriesList.Count == 0)
			{
				SheetElevationGeometry sheetElevationGeom = new SheetElevationGeometry(this);
				sheetElevationGeom.PlaceAtSheetCenter();
				this.AddGeometry(sheetElevationGeom);
			}
			else if(sheetElevationsGeometriesList.Count > 1)
			{
				List<BaseRectangleGeometry> sheetElevationsForDelete = new List<BaseRectangleGeometry>();
				for(int index = 1; index<sheetElevationsGeometriesList.Count; ++index)
				{
					SheetElevationGeometry sheetElevationGeom = sheetElevationsGeometriesList[index];
					if (sheetElevationGeom == null)
						continue;

					sheetElevationsForDelete.Add(sheetElevationGeom);
				}

				this.DeleteGeometry(sheetElevationsForDelete, false, false);
			}

			SheetElevationGeometry sheetElevGeom = this.Rectangles.FirstOrDefault(geom => geom is SheetElevationGeometry) as SheetElevationGeometry;
			if (sheetElevGeom != null)
			{
				bool bSetNewPoint = false;
				Point newTopLeftPnt = sheetElevGeom.TopLeft_GlobalPoint;

				if (Utils.FLT(newTopLeftPnt.X, 0.0) || Utils.FGT(newTopLeftPnt.X, this.Length))
				{
					newTopLeftPnt.X = this.Length / 2;
					bSetNewPoint = true;
				}

				if(Utils.FLT(newTopLeftPnt.Y, 0.0) || Utils.FGT(newTopLeftPnt.Y, this.Width))
				{
					newTopLeftPnt.Y = this.Width / 2;
					bSetNewPoint = true;
				}

				if (bSetNewPoint)
					sheetElevGeom.TopLeft_GlobalPoint = newTopLeftPnt;
			}
		}

		#endregion

		#region Serialization

		//=============================================================================
		//
		// 1.0
		// 1.1 Remove m_ColumnsUniqueSizes. Now its placed in Document
		// 1.2 Remove m_TheFirstRackUniqueSizes. Its placed in Document
		// 1.3 Store Name
		// 1.4 Add roofs list
		// 1.5 Add tie beams list
		// 1.6 Add GUID
		// 1.7 Add Notes
		// 1.8 Add UnitsPerCameraPixel, CameraOffset and Is_UnitsPerCameraPixel_Init
		// 2.8 Add SheetElevationGeometry
		protected static string _sSheet_strMajor = "Sheet_MAJOR";
		protected static int _sSheet_MAJOR = 2;
		protected static string _sSheet_strMinor = "Sheet_MINOR";
		protected static int _sSheet_MINOR = 8;
		//=============================================================================
		public DrawingSheet(SerializationInfo info, StreamingContext context)
		{
			//
			int iMajor = (int)info.GetValue(_sSheet_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sSheet_strMinor, typeof(int));
			if (iMajor > _sSheet_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sSheet_MAJOR && iMinor > _sSheet_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			m_SelectedGeometryCollection.CollectionChanged += SelectedGeometryCollection_CollectionChanged;
			m_StatisticsCollectionView = CollectionViewSource.GetDefaultView(m_RackStatistics);
			m_PalletsStatisticsCollectionView = CollectionViewSource.GetDefaultView(m_PalletsStatistics);

			if (iMajor <= _sSheet_MAJOR)
			{
				try
				{
					// There will be all null objects in the Rectangles.
					// They will be inited after all objects graph deserialization completed.
					// Need to add them as childs to DrawingControl, we can do it in Deserialization callback.
					Rectangles = (List<BaseRectangleGeometry>)info.GetValue("Rectangles", typeof(List<BaseRectangleGeometry>));

					m_patterns = (List<ColumnPattern>)info.GetValue("Patterns", typeof(List<ColumnPattern>));

					//
					if (iMajor <= 1 && iMinor <= 2)
						m_bConvert_To_1_3 = true;
					//
					if (iMajor >= 1 && iMinor >= 3)
					{
						Name = (string)info.GetValue("Name", typeof(string));

						m_Length = (UInt32)info.GetValue("Length", typeof(UInt32));
						m_Width = (UInt32)info.GetValue("Width", typeof(UInt32));
					}

					//
					if (iMajor >= 1 && iMinor >= 4)
					{
						m_RoofsList = (List<Roof>)info.GetValue("RoofsList", typeof(List<Roof>));
						if (m_RoofsList == null)
							m_RoofsList = new List<Roof>();
						if (m_RoofsList.Count == 0)
							_InitRoofList();
					}
					else
						_InitRoofList();

					if (iMajor >= 1 && iMinor >= 5)
						TieBeamsList = (List<TieBeam>)info.GetValue("TieBeamsList", typeof(List<TieBeam>));

					if (iMajor >= 1 && iMinor >= 6)
						m_GUID = (Guid)info.GetValue("GUID", typeof(Guid));
					else if (iMajor <= 1 && iMinor < 6)
						m_GUID = Guid.NewGuid();

					if (iMajor >= 1 && iMinor >= 7)
						m_Notes = (string)info.GetValue("Notes", typeof(string));

					if(iMajor >= 1 && iMinor >= 8)
					{
						m_UnitsPerCameraPixel = (double)info.GetValue("UnitsPerCameraPixel", typeof(double));
						m_CameraOffset = (Vector)info.GetValue("CameraOffset", typeof(Vector));
						try
						{
							m_Is_UnitsPerCameraPixel_Init = (bool)info.GetValue("Is_UnitsPerCameraPixel_Init", typeof(bool));
						}
						catch { }
					}
					else
					{
						m_UnitsPerCameraPixel = 1.0;
						m_CameraOffset = new Vector(0.0, 0.0);
						m_Is_UnitsPerCameraPixel_Init = false;
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
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//
			info.AddValue(_sSheet_strMajor, _sSheet_MAJOR);
			info.AddValue(_sSheet_strMinor, _sSheet_MINOR);

			info.AddValue("Rectangles", Rectangles);
			info.AddValue("Patterns", m_patterns);

			info.AddValue("Name", Name);

			info.AddValue("Length", m_Length);
			info.AddValue("Width", m_Width);

			// 1.4
			info.AddValue("RoofsList", m_RoofsList);

			// 1.5
			info.AddValue("TieBeamsList", TieBeamsList);

			// 1.6
			info.AddValue("GUID", m_GUID);

			// 1.7
			info.AddValue("Notes", m_Notes);

			// 1.8
			info.AddValue("UnitsPerCameraPixel", m_UnitsPerCameraPixel);
			info.AddValue("CameraOffset", m_CameraOffset);
			info.AddValue("Is_UnitsPerCameraPixel_Init", m_Is_UnitsPerCameraPixel_Init);
		}
		//=============================================================================
		public virtual void OnDeserialization(object sender)
		{
			this.IsNewSheet = false;

			this._MarkCalculationStateChanged(eCalculationState.eForceRecalculateRacksGroups);

			if (m_bConvert_To_1_3)
				_Convert_To_1_3();

			if (string.IsNullOrEmpty(Name))
				_InitName();

			//
			// Dont clear m_SelectedGeometryCollection, because BaseRectangleGeometry.IsSelected is set to false inside
			// collection change callback.
			// Inside next foreach loop geometry will not be added in m_SelectedGeometryCollection.
			//m_SelectedGeometryCollection.Clear();
			foreach (BaseRectangleGeometry geom in Rectangles)
			{
				if (geom == null)
					continue;

				geom.OnDeserialization(sender);
				geom.Sheet = this;

				if (geom.IsSelected && !m_SelectedGeometryCollection.Contains(geom))
					m_SelectedGeometryCollection.Add(geom);
			}
			//
			foreach(ColumnPattern _pattern in m_patterns)
			{
				if (_pattern == null)
					continue;

				_pattern.Sheet = this;
			}

			//
			foreach(TieBeam tieBeamGeometry in TieBeamsList)
			{
				if (tieBeamGeometry == null)
					continue;

				tieBeamGeometry.OnDeserialization(sender);
				tieBeamGeometry.Sheet = this;
			}

			CheckSheetElevationGeometry();

			List<Rack> deletedRacks;
			this.CheckRacksGroups(out deletedRacks);

			// Update racks groups
			_UpdateSheet();

			// Update racks and pallets statistics
			UpdateStatisticsCollections();
		}
		//=============================================================================
		private bool m_bConvert_To_1_3 = false;
		private void _Convert_To_1_3()
		{
			_InitName();
		}

		#endregion
	}
}
