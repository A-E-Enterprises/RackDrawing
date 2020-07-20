using HelixToolkit.Wpf;
using RackDrawingApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DrawingControl
{
	/// <summary>
	/// Factory which builds 3D models meshs.
	/// </summary>
	public static class Factory3D
	{
		//=============================================================================
		// Meshes, which contains geometry with the same material and color.
		// For example, all racks columns are placed inside sRacksColumns_MeshBuilder.
		// It is used for better WPF 3D performance. These meshes builders are used for build big Model3D.
		// If every rack(or any other geometry) will be wrapped in the separate Model3D we will get big performance problems.
		// User will get freezes when he tries rotate camera with 711 objects.
		//

		// key - rack column color
		// value - mesh builder
		public static Dictionary<Color, MeshBuilder> sRacksColumns_MeshBuilderDictionary = null;
		public static MeshBuilder sRacksBracings_MeshBuilder = null;
		// key - rack beam color
		// value - mesh builder
		public static Dictionary<Color, MeshBuilder> sRacksBeams_MeshBuilderDictionary = null;
		public static MeshBuilder sRacksDeckingPlates_MeshBuilder = null;
		public static MeshBuilder sRacksPallets_MeshBuilder = null;
		public static MeshBuilder sRacksPalletsRisers_MeshBuilder = null;
		public static MeshBuilder sRoofs_MeshBuilder = null;
		// key - block color
		// value - mesh builder
		public static Dictionary<Color, MeshBuilder> sBlocks_MeshBuilderDictionary = null;
		// key - aisle space color
		// value - mesh builder
		public static Dictionary<Color, MeshBuilder> sAisleSpaces_MeshBuilderDictionary = null;
		// key - column color
		// value - mesh builder
		public static Dictionary<Color, MeshBuilder> sColumns_MeshBuilderDictionary = null;
		// key - wall color
		// value - mesh builder
		public static Dictionary<Color, MeshBuilder> sWalls_MeshBuilderDictionary = null;
		//
		public static MeshBuilder sFloor_MeshBuilder = null;

		/// <summary>
		/// If true then rack's beam and columns will be drawn using rack color.
		/// It is used for display sheet with racks. Rack doesnt have any text on it and it is difficult to understand which racks have the same index(A, B, C...).
		/// So lets change color of racks beams and column according to their index.
		/// </summary>
		public static bool sDrawRackBeamsColumnsWithRackColor = false;

		//=============================================================================
		/// <summary>
		/// Create new empty mesh builders.
		/// </summary>
		public static void ResetBuilders()
		{
			sRacksColumns_MeshBuilderDictionary = new Dictionary<Color, MeshBuilder>();
			sRacksBracings_MeshBuilder = new MeshBuilder();
			sRacksBeams_MeshBuilderDictionary = new Dictionary<Color, MeshBuilder>();
			sRacksDeckingPlates_MeshBuilder = new MeshBuilder();
			sRacksPallets_MeshBuilder = new MeshBuilder();
			sRacksPalletsRisers_MeshBuilder = new MeshBuilder();
			sRoofs_MeshBuilder = new MeshBuilder();
			sBlocks_MeshBuilderDictionary = new Dictionary<Color, MeshBuilder>();
			sAisleSpaces_MeshBuilderDictionary = new Dictionary<Color, MeshBuilder>();
			sColumns_MeshBuilderDictionary = new Dictionary<Color, MeshBuilder>();
			sWalls_MeshBuilderDictionary = new Dictionary<Color, MeshBuilder>();
			sFloor_MeshBuilder = new MeshBuilder();
		}
		//=============================================================================
		/// <summary>
		/// Build Model3D from all mesh builders.
		/// </summary>
		/// <returns></returns>
		public static List<Model3D> GetModelsList()
		{
			List<Model3D> modelsList = new List<Model3D>();
			//
			if (sRacksColumns_MeshBuilderDictionary != null)
			{
				//Material columnsMaterial = new DiffuseMaterial(RackAdvancedDrawingControl.m_ColumnFillBrush);
				//modelsList.Add(Factory3D.CreateGeometryModelFromMesh(sRacksColumns_MeshBuilder, columnsMaterial));

				foreach (Color rackColumnFillColor in sRacksColumns_MeshBuilderDictionary.Keys)
				{
					MeshBuilder rackColumnMeshBuilder = sRacksColumns_MeshBuilderDictionary[rackColumnFillColor];
					if (rackColumnMeshBuilder == null)
						continue;

					SolidColorBrush rackColumnFillBrush = new SolidColorBrush(rackColumnFillColor);
					modelsList.Add(Factory3D.CreateGeometryModelFromMesh(rackColumnMeshBuilder, new DiffuseMaterial(rackColumnFillBrush)));
				}
			}
			//
			if (sRacksBracings_MeshBuilder != null)
			{
				Brush bracingLineBrush = Brushes.Gray;
				Color bracingLineColor;
				if (CurrentTheme.CurrentColorTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme.GetGeometryColor(AppColorTheme.eColorType.eRackAdvProp_BracingLinesColor, out bracingLineColor))
					bracingLineBrush = new SolidColorBrush(bracingLineColor);
				modelsList.Add(Factory3D.CreateGeometryModelFromMesh(sRacksBracings_MeshBuilder, new DiffuseMaterial(bracingLineBrush)));
			}
			//
			if (sRacksBeams_MeshBuilderDictionary != null)
			{
				foreach (Color rackBeamFillColor in sRacksBeams_MeshBuilderDictionary.Keys)
				{
					MeshBuilder rackBeamMeshBuilder = sRacksBeams_MeshBuilderDictionary[rackBeamFillColor];
					if (rackBeamMeshBuilder == null)
						continue;

					SolidColorBrush rackBeamFillBrush = new SolidColorBrush(rackBeamFillColor);
					modelsList.Add(Factory3D.CreateGeometryModelFromMesh(rackBeamMeshBuilder, new DiffuseMaterial(rackBeamFillBrush)));
				}
			}
			//
			if (sRacksDeckingPlates_MeshBuilder != null)
			{
				Brush levelDeckingPlateBrush = Brushes.DarkSlateGray;
				Color levelDeckingPlateColor;
				if (CurrentTheme.CurrentColorTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme.GetGeometryColor(AppColorTheme.eColorType.eRackAdvProp_DeckingPlateFillColor, out levelDeckingPlateColor))
					levelDeckingPlateBrush = new SolidColorBrush(levelDeckingPlateColor);

				modelsList.Add(Factory3D.CreateGeometryModelFromMesh(sRacksDeckingPlates_MeshBuilder, new DiffuseMaterial(levelDeckingPlateBrush)));
			}
			//
			if (sRacksPallets_MeshBuilder != null)
			{
				Brush palletBrush = Brushes.Tan;
				Color palletColor;
				if (CurrentTheme.CurrentColorTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme.GetGeometryColor(AppColorTheme.eColorType.eRackAdvProp_PalletFillColor, out palletColor))
					palletBrush = new SolidColorBrush(palletColor);

				modelsList.Add(Factory3D.CreateGeometryModelFromMesh(sRacksPallets_MeshBuilder, new DiffuseMaterial(palletBrush)));
			}
			//
			if (sRacksPalletsRisers_MeshBuilder != null)
			{
				Brush palletRiserBrush = Brushes.Tan;
				Color palletRiserColor;
				if (CurrentTheme.CurrentColorTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme.GetGeometryColor(AppColorTheme.eColorType.eRackAdvProp_PalletRiserFillColor, out palletRiserColor))
					palletRiserBrush = new SolidColorBrush(palletRiserColor);

				modelsList.Add(Factory3D.CreateGeometryModelFromMesh(sRacksPalletsRisers_MeshBuilder, new DiffuseMaterial(palletRiserBrush)));
			}
			//
			if(sBlocks_MeshBuilderDictionary != null)
			{
				foreach (Color blockFillColor in sBlocks_MeshBuilderDictionary.Keys)
				{
					MeshBuilder blockMeshBuilder = sBlocks_MeshBuilderDictionary[blockFillColor];
					if (blockMeshBuilder == null)
						continue;

					SolidColorBrush blockFillBrush = new SolidColorBrush(blockFillColor);
					modelsList.Add(Factory3D.CreateGeometryModelFromMesh(blockMeshBuilder, new DiffuseMaterial(blockFillBrush)));
				}
			}
			//
			if (sAisleSpaces_MeshBuilderDictionary != null)
			{
				foreach (Color aisleSpaceFillColor in sAisleSpaces_MeshBuilderDictionary.Keys)
				{
					MeshBuilder aisleSpaceMeshBuilder = sAisleSpaces_MeshBuilderDictionary[aisleSpaceFillColor];
					if (aisleSpaceMeshBuilder == null)
						continue;

					SolidColorBrush aisleSpaceFillBrush = new SolidColorBrush(aisleSpaceFillColor);
					modelsList.Add(Factory3D.CreateGeometryModelFromMesh(aisleSpaceMeshBuilder, new DiffuseMaterial(aisleSpaceFillBrush)));
				}
			}
			//
			if (sColumns_MeshBuilderDictionary != null)
			{
				foreach (Color columnFillColor in sColumns_MeshBuilderDictionary.Keys)
				{
					MeshBuilder columnMeshBuilder = sColumns_MeshBuilderDictionary[columnFillColor];
					if (columnMeshBuilder == null)
						continue;

					SolidColorBrush columnFillBrush = new SolidColorBrush(columnFillColor);
					modelsList.Add(Factory3D.CreateGeometryModelFromMesh(columnMeshBuilder, new DiffuseMaterial(columnFillBrush)));
				}
			}
			//
			if (sWalls_MeshBuilderDictionary != null)
			{
				foreach (Color wallFillColor in sWalls_MeshBuilderDictionary.Keys)
				{
					MeshBuilder wallMeshBuilder = sWalls_MeshBuilderDictionary[wallFillColor];
					if (wallMeshBuilder == null)
						continue;

					Color color = wallFillColor;
					color.A = 120;
					SolidColorBrush wallFillBrush = new SolidColorBrush(color);
					modelsList.Add(Factory3D.CreateGeometryModelFromMesh(wallMeshBuilder, new DiffuseMaterial(wallFillBrush)));
				}
			}
			//
			if (sFloor_MeshBuilder != null)
			{
				System.Windows.Media.Color floorColor = System.Windows.Media.Colors.LightGray;
				Color color;
				if (CurrentTheme.CurrentColorTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme.GetGeometryColor(AppColorTheme.eColorType.eFill_Floor, out color))
					floorColor = color;
				floorColor.A = 120;
				System.Windows.Media.Brush floorFillBrush = new System.Windows.Media.SolidColorBrush(floorColor);
				Material floorMaterial = new DiffuseMaterial(floorFillBrush);
				modelsList.Add(Factory3D.CreateGeometryModelFromMesh(sFloor_MeshBuilder, floorMaterial));
			}
			//
			if (sRoofs_MeshBuilder != null)
			{
				System.Windows.Media.Color roofColor = System.Windows.Media.Colors.LightSalmon;
				Color color;
				if (CurrentTheme.CurrentColorTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme.GetGeometryColor(AppColorTheme.eColorType.eFill_Roof, out color))
					roofColor = color;
				roofColor.A = 120;
				System.Windows.Media.Brush roofFillBrush = new System.Windows.Media.SolidColorBrush(roofColor);
				Material roofMaterial = new DiffuseMaterial(roofFillBrush);
				modelsList.Add(Factory3D.CreateGeometryModelFromMesh(sRoofs_MeshBuilder, roofMaterial));
			}

			return modelsList;
		}

		//=============================================================================
		// originPoint - top left point of the rack
		//
		// warehouseOffsetVector - offset from the warehouse top left corner.
		// It is used for calculate points which depends on the roof height.
		// Only warehouse sheet can have a roof.
		public static bool AddGeometryVisual(BaseRectangleGeometry geometry, Point3D originPoint, bool isRoofAvailable, Vector warehouseOffsetVector, bool bApplyRotate)
		{
			if (geometry == null)
				return false;

			Rack rackGeometry = geometry as Rack;
			if (rackGeometry != null)
			{
				//GeometryVisual3D rackVisual = new GeometryVisual3D();
				//rackVisual.Content = Utils3D.BuildRackModel(rackGeometry, originPoint);
				//return rackVisual;
				return Factory3D.AddRackModel(rackGeometry, originPoint, bApplyRotate);
			}

			Block blockGeometry = geometry as Block;
			if (blockGeometry != null)
			{
				//GeometryVisual3D blockVisual = new GeometryVisual3D();
				//blockVisual.Content = Utils3D.BuildBlockModel(blockGeometry, originPoint);
				//return blockVisual;
				return Factory3D.AddBlockModel(blockGeometry, originPoint);
			}

			AisleSpace aisleSpaceGeometry = geometry as AisleSpace;
			if (aisleSpaceGeometry != null)
			{
				//GeometryVisual3D aisleSpaceVisual = new GeometryVisual3D();
				//aisleSpaceVisual.Content = Utils3D.BuildAisleSpaceModel(aisleSpaceGeometry, originPoint);
				//return aisleSpaceVisual;
				return Factory3D.AddAisleSpaceModel(aisleSpaceGeometry, originPoint);
			}

			Column columnGeometry = geometry as Column;
			if (columnGeometry != null && geometry.Sheet != null && geometry.Sheet.Document != null)
			{
				//GeometryVisual3D columnVisual = new GeometryVisual3D();
				//columnVisual.Content = Utils3D.BuildColumnModel(columnGeometry, geometry.Sheet.Document, originPoint, offsetVector, isRoofAvailable);
				//return columnVisual;
				return Factory3D.BuildColumnModel(columnGeometry, geometry.Sheet.Document, originPoint, warehouseOffsetVector, isRoofAvailable);
			}

			return false;
		}

		//=============================================================================
		// warehouseOffsetVector - offset from the warehouse top left corner.
		// It is used for calculate points which depends on the roof height.
		// Only warehouse sheet can have a roof.
		public static bool AddWalls(DrawingSheet sheet, Point3D originPoint, bool isRoofAvailable, Vector warehouseOffsetVector)
		{
			if (sheet == null || sheet.Rectangles == null)
				return false;

			// Get all shutters and walls
			List<Shutter> shuttersList = new List<Shutter>();
			List<Wall> wallsList = new List<Wall>();
			foreach (BaseRectangleGeometry geom in sheet.Rectangles)
			{
				if (geom == null)
					continue;

				Shutter shutterGeom = geom as Shutter;
				if (shutterGeom != null)
				{
					shuttersList.Add(shutterGeom);
					continue;
				}

				Wall wallGeom = geom as Wall;
				if (wallGeom != null)
				{
					wallsList.Add(wallGeom);
					continue;
				}
			}

			// Display shutters as cutouts in the wall
			foreach (Wall wallGeom in wallsList)
			{
				if (wallGeom == null)
					continue;

				//GeometryVisual3D wallVisual = new GeometryVisual3D();
				//wallVisual.Content = Utils3D.BuildWallModel(wallGeom, shuttersList, wallGeom.Sheet.Document, originPoint, offsetVector, isRoofAvailable);
				Factory3D.AddWallModel(wallGeom, shuttersList, wallGeom.Sheet.Document, originPoint, warehouseOffsetVector, isRoofAvailable);
			}

			return true;
		}

		//=============================================================================
		public static bool AddFloor(DrawingSheet sheet, Point3D originPoint)
		{
			if (sheet == null)
				return false;
			if (sheet.Length == 0 || sheet.Width == 0)
				return false;

			return Factory3D.AddFloorModel(sheet, originPoint);
		}

		//=============================================================================
		public static bool AddRoof(WarehouseSheet whSheet)
		{
			if (whSheet == null)
				return false;

			return Factory3D._AddRoofModel(whSheet.SelectedRoof, whSheet.Length, whSheet.Width);
			//if(roofModel != null)
			//{
			//	GeometryVisual3D roofVisual = new GeometryVisual3D();
			//	roofVisual.Content = roofModel;
			//	return roofVisual;
			//}
			//return null;
		}

		//=============================================================================
		/// <summary>
		/// Build sheet's geometry visuals list
		/// </summary>
		public static bool AddSheetModel(DrawingSheet sheet, Point3D originPoint, bool displayWarehouseModel, bool showWalls, bool bDrawFloor)
		{
			// originPoint - top left point of the rack

			if (sheet == null || sheet.Rectangles == null)
				return false;

			bool isRoofAvailable = false;
			if (sheet is WarehouseSheet || sheet.BoundSheetGeometry != null)
				isRoofAvailable = true;

			// Offset from the warehouse top left corner.
			// It is used for calculate points which depends on the roof height.
			// Only warehouse sheet can have a roof.
			Vector warehouseOffsetVector = new Vector(0.0, 0.0);
			if (!displayWarehouseModel && sheet.BoundSheetGeometry != null)
			{
				warehouseOffsetVector.X = sheet.BoundSheetGeometry.TopLeft_GlobalPoint.X;
				warehouseOffsetVector.Y = sheet.BoundSheetGeometry.TopLeft_GlobalPoint.Y;
			}

			foreach (BaseRectangleGeometry geom in sheet.Rectangles)
			{
				if (geom == null)
					continue;

				SheetGeometry sheetGeom = geom as SheetGeometry;
				if(sheetGeom != null)
				{
					// add bound sheet content
					DrawingSheet boundSheet = sheetGeom.BoundSheet;
					if (boundSheet != null)
					{
						Point3D boundSheetOriginPoint = new Point3D();
						boundSheetOriginPoint.X = sheetGeom.TopLeft_GlobalPoint.X;
						boundSheetOriginPoint.Y = sheetGeom.TopLeft_GlobalPoint.Y;
						boundSheetOriginPoint.Z = 0;
						Factory3D.AddSheetModel(boundSheet, boundSheetOriginPoint, displayWarehouseModel, showWalls, false);
					}

					continue;
				}

				Point3D geomOriginPoint = originPoint;
				geomOriginPoint.X += geom.TopLeft_GlobalPoint.X;
				geomOriginPoint.Y += geom.TopLeft_GlobalPoint.Y;
				Factory3D.AddGeometryVisual(geom, geomOriginPoint, isRoofAvailable, warehouseOffsetVector, true);
			}

			// Build walls and shutters
			if(showWalls)
				Factory3D.AddWalls(sheet, originPoint, isRoofAvailable, warehouseOffsetVector);

			// Build floor
			if (bDrawFloor)
				Factory3D.AddFloor(sheet, originPoint);

			return true;
		}

		//=============================================================================
		/// <summary>
		/// Create geometry model using mesh and material.
		/// </summary>
		public static GeometryModel3D CreateGeometryModelFromMesh(MeshBuilder builder, Material material)
		{
			GeometryModel3D model = new GeometryModel3D();
			model.Material = material;
			model.Geometry = builder.ToMesh();

			return model;
		}

		//=============================================================================
		private static double DECKING_PLATE_WIDTH = 80;
		private static double DECKING_PLATE_HEIGHT = 10;
		private static double MIN_DECKING_PLATE_DISTANCE = 10;
		private static double BRACING_HEIGHT = 40;
		private static double BRACING_WIDTH = 20;
		/// <summary>
		/// Add rack 3D model.
		/// </summary>
		public static bool AddRackModel(Rack rack, Point3D originPoint, bool bApplyRotate)
		{
			// originPoint - top left point of the rack
			// bApplyRotate - rotate rack if IsHorizontal = false

			if (rack == null)
				return false;

			RackColumn column = rack.Column;
			if (column == null)
				return false;

			// IsHorizontal = true
			// X axis - rack length
			// Y axis - rack depth
			// Z axis - rack height

			bool bRackShouldBeVertical = bApplyRotate && !rack.IsHorizontal;

			// get column color
			Color columnColor = Colors.DarkBlue;
			if (sDrawRackBeamsColumnsWithRackColor)
				columnColor = rack.FillColor;
			else
			{
				Color color;
				if (CurrentTheme.CurrentColorTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme.GetGeometryColor(AppColorTheme.eColorType.eRackAdvProp_ColumnFillColor, out color))
					columnColor = color;
			}
			if (!sRacksColumns_MeshBuilderDictionary.ContainsKey(columnColor))
				sRacksColumns_MeshBuilderDictionary[columnColor] = new MeshBuilder();
			// get beam color
			Color beamColor = Colors.DarkOrange;
			if (sDrawRackBeamsColumnsWithRackColor)
				beamColor = rack.FillColor;
			else
			{
				Color color;
				if (CurrentTheme.CurrentColorTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme != null && CurrentTheme.CurrentColorTheme.GeometryColorsTheme.GetGeometryColor(AppColorTheme.eColorType.eRackAdvProp_LevelShelfColor, out color))
					beamColor = color;
			}
			if (!sRacksBeams_MeshBuilderDictionary.ContainsKey(beamColor))
				sRacksBeams_MeshBuilderDictionary[beamColor] = new MeshBuilder();

			Vector3D lengthDirection = new Vector3D(1, 0, 0);
			Vector3D depthDirection = new Vector3D(0, 1, 0);
			double columnLength_X = column.Length;
			double columnLength_Y = column.Depth;
			if(bRackShouldBeVertical)
			{
				columnLength_X = column.Depth;
				columnLength_Y = column.Length;

				lengthDirection = new Vector3D(0, 1, 0);
				depthDirection = new Vector3D(1, 0, 0);
			}

			// CREATE COLUMNS
			double availableWidthForBracings = rack.Depth - 2 * column.Depth;
			// left columns should be displayed only for the first rack in the row\column
			if (rack.IsFirstInRowColumn)
			{
				double startColumnsHeight = rack.Length_Z;
				if (rack.TieBeamFrame.HasFlag(eTieBeamFrame.eStartFrame))
					startColumnsHeight = rack.FrameHeight;

				// top left column
				Factory3D.sRacksColumns_MeshBuilderDictionary[columnColor].AddBox(new Rect3D(originPoint.X, originPoint.Y, originPoint.Z, columnLength_X, columnLength_Y, startColumnsHeight));
				// the second column
				Point3D leftFrameColumnPnt = originPoint + (rack.Depth - column.Depth) * depthDirection;
				Factory3D.sRacksColumns_MeshBuilderDictionary[columnColor].AddBox(new Rect3D(leftFrameColumnPnt.X, leftFrameColumnPnt.Y, leftFrameColumnPnt.Z, columnLength_X, columnLength_Y, startColumnsHeight));

				// CREATE BRACINGS
				Point3D bracingsStartPoint01 = originPoint + (column.Length / 2) * lengthDirection + column.Depth * depthDirection;
				_AddBracings(bracingsStartPoint01, rack, availableWidthForBracings, true, depthDirection, lengthDirection);
			}
			// end column
			// For example, current rack has 5000 end frame heiught, but the next rack has 8000 frame height.
			// If look at them at 3D then column between them is not correct. Column should be 8000 height, not 5000.
			Rack nextRack = null;
			if(rack.Sheet != null)
			{
				List<Rack> racksGroup = rack.Sheet.GetRackGroup(rack);
				int currRackIndex = racksGroup.IndexOf(rack);
				if (racksGroup != null && currRackIndex >= 0 && currRackIndex + 1 < racksGroup.Count())
					nextRack = racksGroup[currRackIndex + 1];
			}
			//
			double endColumnsHeight = rack.Length_Z;
			if (rack.TieBeamFrame.HasFlag(eTieBeamFrame.eEndFrame))
				endColumnsHeight = rack.FrameHeight;
			// compare with next rack column height
			if (nextRack != null && Utils.FLT(endColumnsHeight, nextRack.Length_Z))
				endColumnsHeight = nextRack.Length_Z;
			// bot right column
			Point3D columnPnt = originPoint + (rack.Length - column.Length) * lengthDirection + (rack.Depth - column.Depth) * depthDirection;
			Factory3D.sRacksColumns_MeshBuilderDictionary[columnColor].AddBox(new Rect3D(columnPnt.X, columnPnt.Y, columnPnt.Z, columnLength_X, columnLength_Y, endColumnsHeight));
			// the second column
			columnPnt = originPoint + (rack.Length - column.Length) * lengthDirection;
			Factory3D.sRacksColumns_MeshBuilderDictionary[columnColor].AddBox(new Rect3D(columnPnt.X, columnPnt.Y, columnPnt.Z, columnLength_X, columnLength_Y, endColumnsHeight));
			// CREATE BRACINGS
			Point3D bracingsStartPoint02 = originPoint + (rack.Length - column.Length / 2) * lengthDirection + column.Depth * depthDirection;
			_AddBracings(bracingsStartPoint02, rack, availableWidthForBracings, false, depthDirection, lengthDirection);

			// ADD LEVELS AND PALLETS
			double LevelOffset_Z = 0;
			if (rack.IsUnderpassAvailable)
				LevelOffset_Z = rack.Underpass;
			else if (rack.IsMaterialOnGround)
				LevelOffset_Z = 0;
			else
			{
				double firstLevelOffset = Rack.sFirstLevelOffset;
				if (rack.Levels != null)
				{
					RackLevel firstLevel = rack.Levels.FirstOrDefault(level => level != null && level.Index == 1);
					if (firstLevel != null && firstLevel.Beam != null)
						firstLevelOffset -= firstLevel.Beam.Height;
				}
				LevelOffset_Z = Utils.GetWholeNumber(firstLevelOffset);
			}
			if (rack.Levels != null)
			{
				foreach (RackLevel level in rack.Levels)
				{
					if (level == null)
						continue;

					RackLevelBeam beam = level.Beam;
					int beamHeight = 0;
					if (beam != null)
						beamHeight = Utils.GetWholeNumber(beam.Height);

					// ADD LEVELS BEAMS
					// Dont draw beams for the ground level(Index=0).
					if (level.Index != 0 && beam != null)
					{
						double beamLength_X = rack.InnerLength;
						double beamLength_Y = beam.Depth;
						if(bRackShouldBeVertical)
						{
							beamLength_X = beam.Depth;
							beamLength_Y = rack.InnerLength;
						}

						//
						Point3D beam01_StartPnt = originPoint;
						if (rack.IsFirstInRowColumn)
							beam01_StartPnt += column.Length * lengthDirection;
						beam01_StartPnt += (column.Depth - beam.Depth) * depthDirection;
						beam01_StartPnt.Z += LevelOffset_Z;
						Factory3D.sRacksBeams_MeshBuilderDictionary[beamColor].AddBox(new Rect3D(beam01_StartPnt.X, beam01_StartPnt.Y, beam01_StartPnt.Z, beamLength_X, beamLength_Y, beam.Height));

						//
						Point3D beam02_StartPnt = originPoint;
						if (rack.IsFirstInRowColumn)
							beam02_StartPnt += column.Length * lengthDirection;
						beam02_StartPnt += (rack.Depth - column.Depth) * depthDirection;
						beam02_StartPnt.Z += LevelOffset_Z;
						Factory3D.sRacksBeams_MeshBuilderDictionary[beamColor].AddBox(new Rect3D(beam02_StartPnt.X, beam02_StartPnt.Y, beam02_StartPnt.Z, beamLength_X, beamLength_Y, beam.Height));
					}

					// DECKING PLATE
					if (level.Index != 0 && level.Accessories != null && level.Accessories.IsDeckPlateAvailable)
					{
						double deckingPlateSpace = rack.InnerLength;
						double deckingPlateLength = rack.Depth - 2 * column.Depth;
						if (level.Accessories.DeckPlateType == eDeckPlateType.eAlongLength)
						{
							deckingPlateSpace = rack.Depth - 2 * column.Depth;
							deckingPlateLength = rack.InnerLength;
						}

						double rDeckingPlateCount = deckingPlateSpace - MIN_DECKING_PLATE_DISTANCE;
						rDeckingPlateCount = rDeckingPlateCount / (DECKING_PLATE_WIDTH + MIN_DECKING_PLATE_DISTANCE);
						int deckingPlateCount = (int)Math.Floor(rDeckingPlateCount);

						double distanceBetweenPlates = (deckingPlateSpace - deckingPlateCount * DECKING_PLATE_WIDTH) / deckingPlateCount;

						Point3D deckingPlateStartPoint = originPoint + column.Length * lengthDirection + column.Depth * depthDirection;
						deckingPlateStartPoint.Z += LevelOffset_Z + beamHeight - DECKING_PLATE_HEIGHT;
						if (level.Accessories.DeckPlateType == eDeckPlateType.eAlongLength)
							deckingPlateStartPoint += MIN_DECKING_PLATE_DISTANCE * depthDirection;
						else
							deckingPlateStartPoint += MIN_DECKING_PLATE_DISTANCE * lengthDirection;

						for (int i = 0; i < deckingPlateCount; ++i)
						{
							// ADD DECKING PLATE
							if (bRackShouldBeVertical)
							{
								if (level.Accessories.DeckPlateType == eDeckPlateType.eAlongLength)
									Factory3D.sRacksDeckingPlates_MeshBuilder.AddBox(new Rect3D(deckingPlateStartPoint.X, deckingPlateStartPoint.Y, deckingPlateStartPoint.Z, DECKING_PLATE_WIDTH, deckingPlateLength, DECKING_PLATE_HEIGHT));
								else
									Factory3D.sRacksDeckingPlates_MeshBuilder.AddBox(new Rect3D(deckingPlateStartPoint.X, deckingPlateStartPoint.Y, deckingPlateStartPoint.Z, deckingPlateLength, DECKING_PLATE_WIDTH, DECKING_PLATE_HEIGHT));
							}
							else
							{
								if (level.Accessories.DeckPlateType == eDeckPlateType.eAlongLength)
									Factory3D.sRacksDeckingPlates_MeshBuilder.AddBox(new Rect3D(deckingPlateStartPoint.X, deckingPlateStartPoint.Y, deckingPlateStartPoint.Z, deckingPlateLength, DECKING_PLATE_WIDTH, DECKING_PLATE_HEIGHT));
								else
									Factory3D.sRacksDeckingPlates_MeshBuilder.AddBox(new Rect3D(deckingPlateStartPoint.X, deckingPlateStartPoint.Y, deckingPlateStartPoint.Z, DECKING_PLATE_WIDTH, deckingPlateLength, DECKING_PLATE_HEIGHT));
							}

							if (level.Accessories.DeckPlateType == eDeckPlateType.eAlongLength)
								deckingPlateStartPoint += (distanceBetweenPlates + DECKING_PLATE_WIDTH) * depthDirection;
							else
								deckingPlateStartPoint += (distanceBetweenPlates + DECKING_PLATE_WIDTH) * lengthDirection;
						}
					}

					// ADD PALLETS
					if (rack.ShowPallet && level.Pallets != null && level.Pallets.Count > 0)
					{
						double DistanceBetweenPallets = rack.BeamLength;
						foreach (Pallet _pallet in level.Pallets)
						{
							if (_pallet == null)
								continue;

							DistanceBetweenPallets -= _pallet.Length;
						}
						DistanceBetweenPallets /= (level.Pallets.Count + 1);

						bool bAddPalletRiser = false;
						if (level.Accessories != null && level.Accessories.ForkEntryBar)
							bAddPalletRiser = true;

						//
						double PalletOffset = DistanceBetweenPallets;
						foreach (Pallet pallet in level.Pallets)
						{
							if (pallet == null)
								continue;

							Point3D palletStartPoint = originPoint + (PalletOffset + Rack.INNER_LENGTH_ADDITIONAL_GAP / 2) * lengthDirection;
							if (rack.IsFirstInRowColumn)
								palletStartPoint += column.Length * lengthDirection;
							palletStartPoint.Z += LevelOffset_Z + beamHeight;
							if (bAddPalletRiser)
								palletStartPoint.Z += Rack.PALLET_RISER_HEIGHT;
							palletStartPoint += (rack.Depth / 2 - pallet.Width / 2) * depthDirection;

							double palletLength_X = pallet.Length;
							double palletLength_Y = pallet.Width;
							if(bRackShouldBeVertical)
							{
								palletLength_X = pallet.Width;
								palletLength_Y = pallet.Length;
							}

							//
							Factory3D.sRacksPallets_MeshBuilder.AddBox(new Rect3D(palletStartPoint.X, palletStartPoint.Y, palletStartPoint.Z, palletLength_X, palletLength_Y, pallet.Height));

							// add pallet riser
							if (bAddPalletRiser)
							{
								double palletRiserLength_X = Rack.PALLET_RISER_HEIGHT;
								double palletRiserLength_Y = pallet.Width;
								if(bRackShouldBeVertical)
								{
									palletRiserLength_X = pallet.Width;
									palletRiserLength_Y = Rack.PALLET_RISER_HEIGHT;
								}

								// left
								Point3D palletRiserStartPoint = palletStartPoint + Rack.PALLET_RISER_HEIGHT * lengthDirection;
								palletRiserStartPoint.Z -= Rack.PALLET_RISER_HEIGHT;
								//
								Factory3D.sRacksPalletsRisers_MeshBuilder.AddBox(new Rect3D(palletRiserStartPoint.X, palletRiserStartPoint.Y, palletRiserStartPoint.Z, palletRiserLength_X, palletRiserLength_Y, Rack.PALLET_RISER_HEIGHT));

								// right
								palletRiserStartPoint += (pallet.Length - 3 * Rack.PALLET_RISER_HEIGHT) * lengthDirection;
								//
								Factory3D.sRacksPalletsRisers_MeshBuilder.AddBox(new Rect3D(palletRiserStartPoint.X, palletRiserStartPoint.Y, palletRiserStartPoint.Z, palletRiserLength_X, palletRiserLength_Y, Rack.PALLET_RISER_HEIGHT));
							}

							PalletOffset += pallet.Length + DistanceBetweenPallets;
						}
					}

					if (level.Index != 0)
						LevelOffset_Z += beamHeight;
					LevelOffset_Z += level.LevelHeight;
				}
			}

			return true;
		}
		//=============================================================================
		private static void _AddBracings(Point3D startPoint, Rack rack, double availableWidth, bool startColumns, Vector3D bracingLengthDirection, Vector3D bracingDepthDirection)
		{
			if (rack == null)
				return;

			if (Utils.FLE(availableWidth, 0.0))
				return;

			// add bracing lines
			if (rack.Bracing != eColumnBracingType.eUndefined)
			{
				double height = rack.Length_Z;
				if (startColumns)
				{
					if (rack.TieBeamFrame.HasFlag(eTieBeamFrame.eStartFrame))
						height = rack.FrameHeight;
				}
				else
				{
					if (rack.TieBeamFrame.HasFlag(eTieBeamFrame.eEndFrame))
						height = rack.FrameHeight;
				}

				Vector3D heightDirection = new Vector3D(0, 0, 1);
				double heightOffset = Rack.sBracingLinesBottomOffset;

				// add horizontal bracing
				Point3D bracingStartPnt = startPoint + heightOffset * heightDirection;
				if (startColumns)
					bracingStartPnt -= (BRACING_WIDTH / 2) * bracingDepthDirection;
				else
					bracingStartPnt += (BRACING_WIDTH / 2) * bracingDepthDirection;
				Point3D bracingEndPnt = bracingStartPnt + availableWidth * bracingLengthDirection;
				_AddBracing(bracingDepthDirection, ref bracingStartPnt, ref bracingEndPnt);

				// add X bracings
				if (rack.Bracing == eColumnBracingType.eXBracing || rack.Bracing == eColumnBracingType.eXBracingWithStiffener)
				{
					int xBracingsCount = rack.X_Bracings_Count;
					if (xBracingsCount > 0)
					{
						heightOffset += Rack.sXBracingVerticalOffset;

						for (int i = 1; i <= xBracingsCount; ++i)
						{
							bracingStartPnt = startPoint + heightOffset * heightDirection;
							if (startColumns)
								bracingStartPnt += (BRACING_WIDTH / 2) * bracingDepthDirection;
							else
								bracingStartPnt -= (BRACING_WIDTH / 2) * bracingDepthDirection;
							bracingEndPnt = bracingStartPnt + Rack.sBracingVerticalStep * heightDirection + availableWidth * bracingLengthDirection;
							_AddBracing(bracingDepthDirection, ref bracingStartPnt, ref bracingEndPnt);

							double xBracingStartPnt_Y = bracingStartPnt.Y;
							bracingStartPnt.Y = bracingEndPnt.Y;
							bracingEndPnt.Y = xBracingStartPnt_Y;
							if (startColumns)
							{
								bracingStartPnt.X = startPoint.X - BRACING_WIDTH / 2;
								bracingEndPnt.X = startPoint.X - BRACING_WIDTH / 2;
							}
							else
							{
								bracingStartPnt.X = startPoint.X + BRACING_WIDTH / 2;
								bracingEndPnt.X = startPoint.X + BRACING_WIDTH / 2;
							}
							_AddBracing(bracingDepthDirection, ref bracingStartPnt, ref bracingEndPnt);

							heightOffset += Rack.sBracingVerticalStep;
						}

						heightOffset += Rack.sXBracingVerticalOffset;

						// draw horizontal bracing
						bracingStartPnt = startPoint + heightOffset * heightDirection;
						if (startColumns)
							bracingStartPnt -= (BRACING_WIDTH / 2) * bracingDepthDirection;
						else
							bracingStartPnt += (BRACING_WIDTH / 2) * bracingDepthDirection;
						bracingEndPnt = bracingStartPnt + availableWidth * bracingLengthDirection;
						_AddBracing(bracingDepthDirection, ref bracingStartPnt, ref bracingEndPnt);
					}
				}

				// add normal bracings
				int iTotalLines = (int)Math.Floor((Utils.GetWholeNumber(height) - heightOffset - Rack.sTopHorizontalBracingOffset) / Rack.sBracingVerticalStep);
				for (int i = 1; i <= iTotalLines; ++i)
				{
					int iSign = 1;
					if (i % 2 == 0)
						iSign = -1;
					if (!startColumns)
						iSign *= -1;

					bracingStartPnt = startPoint + heightOffset * heightDirection + iSign * (BRACING_WIDTH / 2) * bracingDepthDirection;
					bracingEndPnt = bracingStartPnt + availableWidth * bracingLengthDirection;
					if (i % 2 != 0)
						bracingEndPnt += Rack.sBracingVerticalStep * heightDirection;
					else
						bracingStartPnt += Rack.sBracingVerticalStep * heightDirection;
					_AddBracing(bracingDepthDirection, ref bracingStartPnt, ref bracingEndPnt);

					// draw horizontal lines
					if (i == iTotalLines)
					{
						Point3D vertLineStartPoint = bracingStartPnt - iSign * BRACING_WIDTH * bracingDepthDirection;
						if (i % 2 != 0)
							vertLineStartPoint = bracingEndPnt - iSign * BRACING_WIDTH * bracingDepthDirection - availableWidth * bracingLengthDirection;
						Point3D vertLineEndPoint = vertLineStartPoint + availableWidth * bracingLengthDirection;
						_AddBracing(bracingDepthDirection, ref vertLineStartPoint, ref vertLineEndPoint);
					}

					heightOffset += Rack.sBracingVerticalStep;
				}

				// if there is more than 338 height at the top then show additional line
				if (iTotalLines > 0)
				{
					double topHeightRemainder = height - heightOffset;
					if (Utils.FGE(topHeightRemainder, Rack.sTopHorizontalBracingMinDistance))
					{
						bracingStartPnt = startPoint + (height - Rack.sTopHorizontalBracingOffset) * heightDirection;
						if (startColumns)
							bracingStartPnt -= (BRACING_WIDTH / 2) * bracingDepthDirection;
						else
							bracingStartPnt += (BRACING_WIDTH / 2) * bracingDepthDirection;
						bracingEndPnt = bracingStartPnt + availableWidth * bracingLengthDirection;
						_AddBracing(bracingDepthDirection, ref bracingStartPnt, ref bracingEndPnt);
					}
				}
			}
		}
		//=============================================================================
		private static void _AddBracing(Vector3D bracingDepthDirection, ref Point3D startPoint, ref Point3D endPoint)
		{
			Vector3D vec = endPoint - startPoint;
			Point3D centerPnt = startPoint + vec / 2;
			//
			Vector3D dirX = vec;
			dirX.Normalize();
			//
			Matrix3D rotateMatrix = Matrix3D.Identity;
			rotateMatrix.Rotate(new Quaternion(bracingDepthDirection, 90));
			Vector3D dirY = rotateMatrix.Transform(dirX);

			//
			Factory3D.sRacksBracings_MeshBuilder.AddBox(centerPnt, dirX, dirY, vec.Length + BRACING_HEIGHT, BRACING_HEIGHT, BRACING_WIDTH);
		}

		//=============================================================================
		/// <summary>
		/// Add block 3D model
		/// </summary>
		public static bool AddBlockModel(Block block, Point3D originPoint)
		{
			if (block == null)
				return false;

			if (!sBlocks_MeshBuilderDictionary.ContainsKey(block.FillColor))
				sBlocks_MeshBuilderDictionary[block.FillColor] = new MeshBuilder();

			sBlocks_MeshBuilderDictionary[block.FillColor].AddBox(new Rect3D(originPoint.X, originPoint.Y, originPoint.Z, block.Length_X, block.Length_Y, block.Length_Z));
			return true;
		}

		//=============================================================================
		/// <summary>
		/// Add aisle space 3D model
		/// </summary>
		public static bool AddAisleSpaceModel(AisleSpace aisleSpaceGeom, Point3D originPoint)
		{
			if (aisleSpaceGeom == null)
				return false;

			if (!sAisleSpaces_MeshBuilderDictionary.ContainsKey(aisleSpaceGeom.FillColor))
				sAisleSpaces_MeshBuilderDictionary[aisleSpaceGeom.FillColor] = new MeshBuilder();

			sAisleSpaces_MeshBuilderDictionary[aisleSpaceGeom.FillColor].AddBox(new Rect3D(originPoint.X, originPoint.Y, originPoint.Z, aisleSpaceGeom.Length_X, aisleSpaceGeom.Length_Y, aisleSpaceGeom.Length_Z));
			return true;
		}

		//=============================================================================
		private static double ROOF_THICKNESS = 100;
		private static double ROOF_OFFSET = 600;
		/// <summary>
		/// Build roof 3D model.
		/// </summary>
		private static bool _AddRoofModel(Roof roof, double length, double width)
		{
			if (roof == null)
				return false;

			if (Utils.FLE(length, 0.0) || Utils.FLE(width, 0.0))
				return false;

			//
			FlatRoof flatRoof = roof as FlatRoof;
			if(flatRoof != null)
			{
				sRoofs_MeshBuilder.AddBox(new Point3D(length / 2, width / 2, flatRoof.Height + ROOF_THICKNESS / 2), length + 2 * ROOF_OFFSET, width + 2 * ROOF_OFFSET, ROOF_THICKNESS);
				return true;
			}
			//
			GableRoof gableRoof = roof as GableRoof;
			if(gableRoof != null)
			{
				// Roof bot plane points.
				Point3D botPoint_01 = new Point3D(0.0, 0.0, gableRoof.MinHeight);
				Point3D botPoint_02 = new Point3D(0.0, width, gableRoof.MinHeight);
				Point3D botPoint_03 = new Point3D(length, width, gableRoof.MinHeight);
				Point3D botPoint_04 = new Point3D(length, 0.0, gableRoof.MinHeight);
				// Middle points
				Point3D bot_MiddleLeft_Point = new Point3D(0.0, width / 2, gableRoof.MaxHeight);
				Point3D bot_MiddleRight_Point = new Point3D(length, width / 2, gableRoof.MaxHeight);
				Point3D bot_MiddleTop_Point = new Point3D(length / 2, 0.0, gableRoof.MaxHeight);
				Point3D bot_MiddleBot_Point = new Point3D(length / 2, width, gableRoof.MaxHeight);

				// applly offset
				if(gableRoof.HorizontalRidgeDirection)
				{
					//
					Vector3D vec1 = bot_MiddleLeft_Point - botPoint_01;
					vec1.Normalize();
					Vector3D vec2 = botPoint_04 - botPoint_01;
					vec2.Normalize();
					botPoint_01 -= (vec1 + vec2) * ROOF_OFFSET;
					//
					vec1 = bot_MiddleRight_Point - botPoint_04;
					vec1.Normalize();
					vec2 = botPoint_01 - botPoint_04;
					vec2.Normalize();
					botPoint_04 -= (vec1 + vec2) * ROOF_OFFSET;
					//
					vec1 = bot_MiddleLeft_Point - botPoint_02;
					vec1.Normalize();
					vec2 = botPoint_03 - botPoint_02;
					vec2.Normalize();
					botPoint_02 -= (vec1 + vec2) * ROOF_OFFSET;
					//
					vec1 = bot_MiddleRight_Point - botPoint_03;
					vec1.Normalize();
					vec2 = botPoint_02 - botPoint_03;
					vec2.Normalize();
					botPoint_03 -= (vec1 + vec2) * ROOF_OFFSET;
					//
					bot_MiddleLeft_Point.X = botPoint_01.X;
					bot_MiddleRight_Point.X = botPoint_04.X;
					//
					bot_MiddleTop_Point.Y = botPoint_01.Y;
					bot_MiddleTop_Point.Z = botPoint_01.Z;
					bot_MiddleBot_Point.Y = botPoint_04.Y;
					bot_MiddleBot_Point.Z = botPoint_04.Z;
				}
				else
				{
					//
					Vector3D vec1 = bot_MiddleTop_Point - botPoint_01;
					vec1.Normalize();
					Vector3D vec2 = botPoint_02 - botPoint_01;
					vec2.Normalize();
					botPoint_01 -= (vec1 + vec2) * ROOF_OFFSET;
					//
					vec1 = bot_MiddleTop_Point - botPoint_04;
					vec1.Normalize();
					vec2 = botPoint_03 - botPoint_04;
					vec2.Normalize();
					botPoint_04 -= (vec1 + vec2) * ROOF_OFFSET;
					//
					vec1 = bot_MiddleBot_Point - botPoint_02;
					vec1.Normalize();
					vec2 = botPoint_01 - botPoint_02;
					vec2.Normalize();
					botPoint_02 -= (vec1 + vec2) * ROOF_OFFSET;
					//
					vec1 = bot_MiddleBot_Point - botPoint_03;
					vec1.Normalize();
					vec2 = botPoint_04 - botPoint_03;
					vec2.Normalize();
					botPoint_03 -= (vec1 + vec2) * ROOF_OFFSET;
					//
					bot_MiddleLeft_Point.X = botPoint_01.X;
					bot_MiddleLeft_Point.Z = botPoint_01.Z;
					bot_MiddleRight_Point.X = botPoint_04.X;
					bot_MiddleRight_Point.Z = botPoint_04.Z;
					//
					bot_MiddleTop_Point.Y = botPoint_01.Y;
					bot_MiddleBot_Point.Y = botPoint_02.Y;
				}

				// Roof top plane points.
				Vector3D topPlaneOffsetVec = new Vector3D(0.0, 0.0, ROOF_THICKNESS);
				//
				Point3D topPoint_01 = botPoint_01 + topPlaneOffsetVec;
				Point3D topPoint_02 = botPoint_02 + topPlaneOffsetVec;
				Point3D topPoint_03 = botPoint_03 + topPlaneOffsetVec;
				Point3D topPoint_04 = botPoint_04 + topPlaneOffsetVec;
				//
				Point3D top_MiddleLeft_Point = bot_MiddleLeft_Point + topPlaneOffsetVec;
				Point3D top_MiddleRight_Point = bot_MiddleRight_Point + topPlaneOffsetVec;
				Point3D top_MiddleTop_Point = bot_MiddleTop_Point + topPlaneOffsetVec;
				Point3D top_MiddleBot_Point = bot_MiddleBot_Point + topPlaneOffsetVec;

				// Build roof.
				// Order of the points drives which side of the plane will be visible.
				if(gableRoof.HorizontalRidgeDirection)
				{
					// bottom planes
					sRoofs_MeshBuilder.AddQuad(botPoint_01, bot_MiddleLeft_Point, bot_MiddleRight_Point, botPoint_04);
					//roofMeshBuilder.AddQuad(botPoint_04, bot_MiddleRight_Point, bot_MiddleLeft_Point, botPoint_01);
					sRoofs_MeshBuilder.AddQuad(botPoint_03, bot_MiddleRight_Point, bot_MiddleLeft_Point, botPoint_02);
					//roofMeshBuilder.AddQuad(botPoint_02, bot_MiddleLeft_Point, bot_MiddleRight_Point, botPoint_03);
					// top planes
					//roofMeshBuilder.AddQuad(topPoint_01, top_MiddleLeft_Point, top_MiddleRight_Point, topPoint_04);
					sRoofs_MeshBuilder.AddQuad(topPoint_04, top_MiddleRight_Point, top_MiddleLeft_Point, topPoint_01);
					//roofMeshBuilder.AddQuad(topPoint_03, top_MiddleRight_Point, top_MiddleLeft_Point, topPoint_02);
					sRoofs_MeshBuilder.AddQuad(topPoint_02, top_MiddleLeft_Point, top_MiddleRight_Point, topPoint_03);
					// left corner planes
					//roofMeshBuilder.AddQuad(botPoint_01, bot_MiddleLeft_Point, top_MiddleLeft_Point, topPoint_01);
					sRoofs_MeshBuilder.AddQuad(topPoint_01, top_MiddleLeft_Point, bot_MiddleLeft_Point, botPoint_01);
					//roofMeshBuilder.AddQuad(bot_MiddleLeft_Point, botPoint_02, topPoint_02, top_MiddleLeft_Point);
					sRoofs_MeshBuilder.AddQuad(top_MiddleLeft_Point, topPoint_02, botPoint_02, bot_MiddleLeft_Point);
					// right corner plane
					sRoofs_MeshBuilder.AddQuad(botPoint_03, topPoint_03, top_MiddleRight_Point, bot_MiddleRight_Point);
					sRoofs_MeshBuilder.AddQuad(topPoint_04, botPoint_04, bot_MiddleRight_Point, top_MiddleRight_Point);
					// top corner plane
					sRoofs_MeshBuilder.AddQuad(topPoint_04, topPoint_01, botPoint_01, botPoint_04);
					// bot corner plane
					sRoofs_MeshBuilder.AddQuad(topPoint_02, topPoint_03, botPoint_03, botPoint_02);
				}
				else
				{
					// bottom planes
					sRoofs_MeshBuilder.AddQuad(botPoint_01, botPoint_02, bot_MiddleBot_Point, bot_MiddleTop_Point);
					sRoofs_MeshBuilder.AddQuad(bot_MiddleTop_Point, bot_MiddleBot_Point, botPoint_03, botPoint_04);
					// top planes
					sRoofs_MeshBuilder.AddQuad(topPoint_02, topPoint_01, top_MiddleTop_Point, top_MiddleBot_Point);
					sRoofs_MeshBuilder.AddQuad(topPoint_04, topPoint_03, top_MiddleBot_Point, top_MiddleTop_Point);
					// left corner plane
					sRoofs_MeshBuilder.AddQuad(topPoint_01, topPoint_02, botPoint_02, botPoint_01);
					// right corner plane
					sRoofs_MeshBuilder.AddQuad(topPoint_03, topPoint_04, botPoint_04, botPoint_03);
					// top corner planes
					sRoofs_MeshBuilder.AddQuad(topPoint_01, botPoint_01, bot_MiddleTop_Point, top_MiddleTop_Point);
					sRoofs_MeshBuilder.AddQuad(botPoint_04, topPoint_04, top_MiddleTop_Point, bot_MiddleTop_Point);
					// bot corner planes
					sRoofs_MeshBuilder.AddQuad(botPoint_02, topPoint_02, top_MiddleBot_Point, bot_MiddleBot_Point);
					sRoofs_MeshBuilder.AddQuad(topPoint_03, botPoint_03, bot_MiddleBot_Point, top_MiddleBot_Point);
				}

				return true;
			}

			//
			ShedRoof shedRoof = roof as ShedRoof;
			if(shedRoof != null)
			{
				// Roof bot plane points.
				Point3D botPoint_01 = new Point3D(0.0, 0.0, shedRoof.MinHeight);
				Point3D botPoint_02 = new Point3D(0.0, width, shedRoof.MinHeight);
				Point3D botPoint_03 = new Point3D(length, width, shedRoof.MinHeight);
				Point3D botPoint_04 = new Point3D(length, 0.0, shedRoof.MinHeight);
				// apply pitch to the points
				if(ShedRoof.ePitchDirection.eLeftToRight == shedRoof.PitchDirection)
				{
					botPoint_01.Z = shedRoof.MaxHeight;
					botPoint_02.Z = shedRoof.MaxHeight;

					//
					Vector3D vec1 = botPoint_01 - botPoint_04;
					vec1.Normalize();
					Vector3D vec2 = botPoint_01 - botPoint_02;
					vec2.Normalize();
					botPoint_01 += (vec1 + vec2) * ROOF_OFFSET;
					botPoint_02 += (vec1 - vec2) * ROOF_OFFSET;
					//
					vec1 = botPoint_04 - botPoint_01;
					vec1.Normalize();
					vec2 = botPoint_04 - botPoint_03;
					vec2.Normalize();
					botPoint_04 += (vec1 + vec2) * ROOF_OFFSET;
					botPoint_04 += (vec1 - vec2) * ROOF_OFFSET;
				}
				else if (ShedRoof.ePitchDirection.eRightToLeft == shedRoof.PitchDirection)
				{
					botPoint_03.Z = shedRoof.MaxHeight;
					botPoint_04.Z = shedRoof.MaxHeight;

					//
					Vector3D vec1 = botPoint_01 - botPoint_04;
					vec1.Normalize();
					Vector3D vec2 = botPoint_01 - botPoint_02;
					vec2.Normalize();
					botPoint_01 += (vec1 + vec2) * ROOF_OFFSET;
					botPoint_02 += (vec1 - vec2) * ROOF_OFFSET;
					//
					vec1 = botPoint_04 - botPoint_01;
					vec1.Normalize();
					vec2 = botPoint_04 - botPoint_03;
					vec2.Normalize();
					botPoint_04 += (vec1 + vec2) * ROOF_OFFSET;
					botPoint_04 += (vec1 - vec2) * ROOF_OFFSET;
				}
				else if (ShedRoof.ePitchDirection.eTopToBot == shedRoof.PitchDirection)
				{
					botPoint_01.Z = shedRoof.MaxHeight;
					botPoint_04.Z = shedRoof.MaxHeight;

					//
					Vector3D vec1 = botPoint_01 - botPoint_02;
					vec1.Normalize();
					Vector3D vec2 = botPoint_01 - botPoint_04;
					vec2.Normalize();
					botPoint_01 += (vec1 + vec2) * ROOF_OFFSET;
					botPoint_04 += (vec1 - vec2) * ROOF_OFFSET;
					//
					vec1 = botPoint_02 - botPoint_01;
					vec1.Normalize();
					vec2 = botPoint_02 - botPoint_03;
					vec2.Normalize();
					botPoint_02 += (vec1 + vec2) * ROOF_OFFSET;
					botPoint_03 += (vec1 - vec2) * ROOF_OFFSET;
				}
				else if (ShedRoof.ePitchDirection.eBotToTop == shedRoof.PitchDirection)
				{
					botPoint_02.Z = shedRoof.MaxHeight;
					botPoint_03.Z = shedRoof.MaxHeight;

					//
					Vector3D vec1 = botPoint_01 - botPoint_02;
					vec1.Normalize();
					Vector3D vec2 = botPoint_01 - botPoint_04;
					vec2.Normalize();
					botPoint_01 += (vec1 + vec2) * ROOF_OFFSET;
					botPoint_04 += (vec1 - vec2) * ROOF_OFFSET;
					//
					vec1 = botPoint_02 - botPoint_01;
					vec1.Normalize();
					vec2 = botPoint_02 - botPoint_03;
					vec2.Normalize();
					botPoint_02 += (vec1 + vec2) * ROOF_OFFSET;
					botPoint_03 += (vec1 - vec2) * ROOF_OFFSET;
				}

				// Roof top plane points.
				Vector3D topPlaneOffsetVec = new Vector3D(0.0, 0.0, ROOF_THICKNESS);
				Point3D topPoint_01 = botPoint_01 + topPlaneOffsetVec;
				Point3D topPoint_02 = botPoint_02 + topPlaneOffsetVec;
				Point3D topPoint_03 = botPoint_03 + topPlaneOffsetVec;
				Point3D topPoint_04 = botPoint_04 + topPlaneOffsetVec;

				// Order of the points drives which side of the plane will be visible.
				//
				// bottom plane
				sRoofs_MeshBuilder.AddQuad(botPoint_01, botPoint_02, botPoint_03, botPoint_04);
				// top plane
				sRoofs_MeshBuilder.AddQuad(topPoint_04, topPoint_03, topPoint_02, topPoint_01);
				// left corner plane
				//roofMeshBuilder.AddQuad(botPoint_01, botPoint_02, topPoint_02, topPoint_01);
				sRoofs_MeshBuilder.AddQuad(topPoint_01, topPoint_02, botPoint_02, botPoint_01);
				// right corner plane
				//roofMeshBuilder.AddQuad(botPoint_03, botPoint_04, topPoint_04, topPoint_03);
				sRoofs_MeshBuilder.AddQuad(topPoint_03, topPoint_04, botPoint_04, botPoint_03);
				// top corner plane
				//roofMeshBuilder.AddQuad(botPoint_04, botPoint_01, topPoint_01, topPoint_04);
				sRoofs_MeshBuilder.AddQuad(topPoint_04, topPoint_01, botPoint_01, botPoint_04);
				// bot corner plane
				//roofMeshBuilder.AddQuad(botPoint_02, botPoint_03, topPoint_03, topPoint_02);
				sRoofs_MeshBuilder.AddQuad(topPoint_02, topPoint_03, botPoint_03, botPoint_02);

				return true;
			}

			return false;
		}

		//=============================================================================
		public static bool BuildColumnModel(Column column, DrawingDocument doc, Point3D originPoint, Vector offsetVec, bool isRoofAvailable)
		{
			if (column == null)
				return false;

			if (doc == null)
				return false;

			if (!sColumns_MeshBuilderDictionary.ContainsKey(column.FillColor))
				sColumns_MeshBuilderDictionary[column.FillColor] = new MeshBuilder();

			// Bot points
			Point3D botPoint_01 = originPoint;
			Point3D botPoint_02 = new Point3D(originPoint.X, originPoint.Y + column.Length_Y, originPoint.Z);
			Point3D botPoint_03 = new Point3D(originPoint.X + column.Length_X, originPoint.Y + column.Length_Y, originPoint.Z);
			Point3D botPoint_04 = new Point3D(originPoint.X + column.Length_X, originPoint.Y, originPoint.Z);
			// Top points
			Point3D topPoint_01 = _CalculateRoofPoint(botPoint_01, doc, isRoofAvailable, offsetVec);
			Point3D topPoint_02 = _CalculateRoofPoint(botPoint_02, doc, isRoofAvailable, offsetVec);
			Point3D topPoint_03 = _CalculateRoofPoint(botPoint_03, doc, isRoofAvailable, offsetVec);
			Point3D topPoint_04 = _CalculateRoofPoint(botPoint_04, doc, isRoofAvailable, offsetVec);

			// left side
			sColumns_MeshBuilderDictionary[column.FillColor].AddQuad(topPoint_01, topPoint_02, botPoint_02, botPoint_01);
			// right side
			sColumns_MeshBuilderDictionary[column.FillColor].AddQuad(topPoint_03, topPoint_04, botPoint_04, botPoint_03);
			// top side
			sColumns_MeshBuilderDictionary[column.FillColor].AddQuad(topPoint_04, topPoint_01, botPoint_01, botPoint_04);
			// bot side
			sColumns_MeshBuilderDictionary[column.FillColor].AddQuad(topPoint_02, topPoint_03, botPoint_03, botPoint_02);
			// bot
			sColumns_MeshBuilderDictionary[column.FillColor].AddQuad(botPoint_01, botPoint_02, botPoint_03, botPoint_04);
			// top
			sColumns_MeshBuilderDictionary[column.FillColor].AddQuad(topPoint_04, topPoint_03, topPoint_02, topPoint_01);

			return true;
		}
		private static Point3D _CalculateRoofPoint(Point3D pnt, DrawingDocument doc, bool isRoofAvailable)
		{
			return _CalculateRoofPoint(pnt, doc, isRoofAvailable, new Vector(0.0, 0.0));
		}
		private static Point3D _CalculateRoofPoint(Point3D pnt, DrawingDocument doc, bool isRoofAvailable, Vector offsetVector)
		{
			Point3D result = pnt;

			if (doc == null || !isRoofAvailable)
			{
				result.Z = 12000;
				return result;
			}

			double height;
			if (doc.CalculateMaxHeightForPoint(new System.Windows.Point(pnt.X + offsetVector.X, pnt.Y + offsetVector.Y), out height))
				result.Z = Math.Floor(height);

			return result;
		}

		//=============================================================================
		/// <summary>
		/// Build wall 3D model.
		/// </summary>
		/// <param name="warehouseOffsetVector">
		/// Offset from the top left corner of Warehouse sheet. It is used for calculate roof points because only Warehouse sheet can have a roof.
		/// </param>
		/// <returns></returns>
		public static bool AddWallModel(Wall wall, List<Shutter> shuttersList, DrawingDocument doc, Point3D originPoint, Vector warehouseOffsetVector, bool isRoofAvailable)
		{
			if (wall == null || wall.WallPosition == eWallPosition.eUndefined)
				return false;

			if (doc == null)
				return false;

			if (!sWalls_MeshBuilderDictionary.ContainsKey(wall.FillColor))
				sWalls_MeshBuilderDictionary[wall.FillColor] = new MeshBuilder();

			// sort shutters
			List<Shutter> wallShuttersList = new List<Shutter>();
			if(shuttersList != null)
			{
				foreach(Shutter shutterGeom in shuttersList)
				{
					if (shutterGeom == null)
						continue;

					if (shutterGeom.WallPosition == wall.WallPosition)
						wallShuttersList.Add(shutterGeom);
				}
			}

			if(eWallPosition.eBot == wall.WallPosition || eWallPosition.eTop == wall.WallPosition)
				wallShuttersList = wallShuttersList.OrderBy(shutterGeom => shutterGeom.TopLeft_GlobalPoint.X).ToList();
			else
				wallShuttersList = wallShuttersList.OrderBy(shutterGeom => shutterGeom.TopLeft_GlobalPoint.Y).ToList();

			Point middleRoof2DPoint_01 = new Point(0.0, 0.0);
			Point middleRoof2DPoint_02 = new Point(0.0, 0.0);
			bool bUseMiddlePoints = false;
			bool bHorizontalRidge = false;
			if (isRoofAvailable)
			{
				GableRoof gableRoof = doc.SelectedRoof as GableRoof;
				if (gableRoof != null)
				{
					double warehouseSheetLengthX = doc.WarehouseSheet.Length;
					double warehouseSheetLengthY = doc.WarehouseSheet.Width;

					if (Utils.FGT(warehouseSheetLengthX, 0.0) && Utils.FGT(warehouseSheetLengthY, 0.0))
					{
						bUseMiddlePoints = gableRoof.GetRidgePoints(warehouseSheetLengthX, warehouseSheetLengthY, out middleRoof2DPoint_01, out middleRoof2DPoint_02);
						bHorizontalRidge = gableRoof.HorizontalRidgeDirection;
					}
				}
			}

			//
			if (wallShuttersList.Count == 0)
			{
				Point3D botPoint_01 = new Point3D(originPoint.X + wall.TopLeft_GlobalPoint.X, originPoint.Y + wall.TopLeft_GlobalPoint.Y, originPoint.Z);
				Point3D botPoint_02 = new Point3D(originPoint.X + wall.BottomLeft_GlobalPoint.X, originPoint.Y + wall.BottomLeft_GlobalPoint.Y, originPoint.Z);
				Point3D botPoint_03 = new Point3D(originPoint.X + wall.BottomRight_GlobalPoint.X, originPoint.Y + wall.BottomRight_GlobalPoint.Y, originPoint.Z);
				Point3D botPoint_04 = new Point3D(originPoint.X + wall.TopRight_GlobalPoint.X, originPoint.Y + wall.TopRight_GlobalPoint.Y, originPoint.Z);
				Point3D topPoint_01 = _CalculateRoofPoint(botPoint_01, doc, isRoofAvailable, warehouseOffsetVector);
				Point3D topPoint_02 = _CalculateRoofPoint(botPoint_02, doc, isRoofAvailable, warehouseOffsetVector);
				Point3D topPoint_03 = _CalculateRoofPoint(botPoint_03, doc, isRoofAvailable, warehouseOffsetVector);
				Point3D topPoint_04 = _CalculateRoofPoint(botPoint_04, doc, isRoofAvailable, warehouseOffsetVector);

				// without shutters
				if (eWallPosition.eTop == wall.WallPosition || eWallPosition.eBot == wall.WallPosition)
				{
					// left side
					sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_01, topPoint_02, botPoint_02, botPoint_01);
					// right side
					sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_03, topPoint_04, botPoint_04, botPoint_03);
					// bottom
					sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(botPoint_01, botPoint_02, botPoint_03, botPoint_04);

					// outside
					sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_04, topPoint_01, botPoint_01, botPoint_04);
					if (bUseMiddlePoints && !bHorizontalRidge && Utils.FLT(botPoint_01.X + warehouseOffsetVector.X, middleRoof2DPoint_01.X) && Utils.FGT(botPoint_04.X + warehouseOffsetVector.X, middleRoof2DPoint_01.X))
					{
						Point3D middlePoint = botPoint_01;
						middlePoint.X = middleRoof2DPoint_01.X - warehouseOffsetVector.X;
						middlePoint = _CalculateRoofPoint(middlePoint, doc, isRoofAvailable, warehouseOffsetVector);

						// add triangle
						sWalls_MeshBuilderDictionary[wall.FillColor].AddTriangle(topPoint_01, topPoint_04, middlePoint);
					}

					// inside
					sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_02, topPoint_03, botPoint_03, botPoint_02);
					if (bUseMiddlePoints && !bHorizontalRidge && Utils.FLT(botPoint_01.X + warehouseOffsetVector.X, middleRoof2DPoint_01.X) && Utils.FGT(botPoint_04.X + warehouseOffsetVector.X, middleRoof2DPoint_01.X))
					{
						Point3D middlePoint = topPoint_02;
						middlePoint.X = middleRoof2DPoint_01.X - warehouseOffsetVector.X;
						middlePoint = _CalculateRoofPoint(middlePoint, doc, isRoofAvailable, warehouseOffsetVector);

						// add triangle
						sWalls_MeshBuilderDictionary[wall.FillColor].AddTriangle(topPoint_03, topPoint_02, middlePoint);
					}

					// top side
					if(bUseMiddlePoints && !bHorizontalRidge && Utils.FLT(botPoint_01.X + warehouseOffsetVector.X, middleRoof2DPoint_01.X) && Utils.FGT(botPoint_04.X + warehouseOffsetVector.X, middleRoof2DPoint_01.X))
					{
						Point3D middlePoint_01 = botPoint_01;
						middlePoint_01.X = middleRoof2DPoint_01.X - warehouseOffsetVector.X;
						middlePoint_01 = _CalculateRoofPoint(middlePoint_01, doc, isRoofAvailable, warehouseOffsetVector);

						Point3D middlePoint_02 = topPoint_02;
						middlePoint_02.X = middleRoof2DPoint_01.X - warehouseOffsetVector.X;
						middlePoint_02 = _CalculateRoofPoint(middlePoint_02, doc, isRoofAvailable, warehouseOffsetVector);

						sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_02, topPoint_01, middlePoint_01, middlePoint_02);
						sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_04, topPoint_03, middlePoint_02, middlePoint_01);
					}
					else
						sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_04, topPoint_03, topPoint_02, topPoint_01);
				}
				else if(eWallPosition.eLeft == wall.WallPosition || eWallPosition.eRight == wall.WallPosition)
				{
					// upper side
					sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(botPoint_01, botPoint_04, topPoint_04, topPoint_01);
					// down side
					sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_02, topPoint_03, botPoint_03, botPoint_02);
					// bottom side
					sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(botPoint_01, botPoint_02, botPoint_03, botPoint_04);

					// outside
					sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_01, topPoint_02, botPoint_02, botPoint_01);
					if (bUseMiddlePoints && bHorizontalRidge && Utils.FLT(botPoint_01.Y + warehouseOffsetVector.Y, middleRoof2DPoint_01.Y) && Utils.FGT(botPoint_02.Y + warehouseOffsetVector.Y, middleRoof2DPoint_01.Y))
					{
						Point3D middlePoint = botPoint_01;
						middlePoint.Y = middleRoof2DPoint_01.Y - warehouseOffsetVector.Y;
						middlePoint = _CalculateRoofPoint(middlePoint, doc, isRoofAvailable, warehouseOffsetVector);

						// add triangle
						sWalls_MeshBuilderDictionary[wall.FillColor].AddTriangle(topPoint_02, topPoint_01, middlePoint);
					}

					// inside
					sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_03, topPoint_04, botPoint_04, botPoint_03);
					if (bUseMiddlePoints && bHorizontalRidge && Utils.FLT(botPoint_01.Y + warehouseOffsetVector.Y, middleRoof2DPoint_01.Y) && Utils.FGT(botPoint_02.Y + warehouseOffsetVector.Y, middleRoof2DPoint_01.Y))
					{
						Point3D middlePoint = topPoint_02;
						middlePoint.Y = middleRoof2DPoint_01.Y - warehouseOffsetVector.Y;
						middlePoint = _CalculateRoofPoint(middlePoint, doc, isRoofAvailable, warehouseOffsetVector);

						// add triangle
						sWalls_MeshBuilderDictionary[wall.FillColor].AddTriangle(topPoint_04, topPoint_03, middlePoint);
					}

					// top side
					if (bUseMiddlePoints && bHorizontalRidge && Utils.FLT(botPoint_01.Y + warehouseOffsetVector.Y, middleRoof2DPoint_01.Y) && Utils.FGT(botPoint_02.Y + warehouseOffsetVector.Y, middleRoof2DPoint_01.Y))
					{
						Point3D middlePoint_01 = botPoint_01;
						middlePoint_01.Y = middleRoof2DPoint_01.Y - warehouseOffsetVector.Y;
						middlePoint_01 = _CalculateRoofPoint(middlePoint_01, doc, isRoofAvailable, warehouseOffsetVector);

						Point3D middlePoint_02 = topPoint_04;
						middlePoint_02.Y = middleRoof2DPoint_01.Y - warehouseOffsetVector.Y;
						middlePoint_02 = _CalculateRoofPoint(middlePoint_02, doc, isRoofAvailable, warehouseOffsetVector);

						sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_01, topPoint_04, middlePoint_02, middlePoint_01);
						sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_03, topPoint_02, middlePoint_01, middlePoint_02);
					}
					else
						sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topPoint_04, topPoint_03, topPoint_02, topPoint_01);
				}
			}
			else
			{
				if(eWallPosition.eTop == wall.WallPosition || eWallPosition.eBot == wall.WallPosition)
				{
					Point3D botPoint_01 = new Point3D(originPoint.X + wall.TopLeft_GlobalPoint.X, originPoint.Y + wall.TopLeft_GlobalPoint.Y, originPoint.Z);
					Point3D botPoint_02 = new Point3D(originPoint.X + wall.BottomLeft_GlobalPoint.X, originPoint.Y + wall.BottomLeft_GlobalPoint.Y, originPoint.Z);

					double curDistanceFromTheGround = originPoint.Z;

					foreach(Shutter shutterGeom in wallShuttersList)
					{
						Point3D botPoint_03 = new Point3D(originPoint.X + shutterGeom.BottomLeft_GlobalPoint.X, originPoint.Y + shutterGeom.BottomLeft_GlobalPoint.Y, originPoint.Z);
						Point3D botPoint_04 = new Point3D(originPoint.X + shutterGeom.TopLeft_GlobalPoint.X, originPoint.Y + shutterGeom.TopLeft_GlobalPoint.Y, originPoint.Z);

						// left wall side
						if(wallShuttersList.IndexOf(shutterGeom) == 0)
						{
							Point3D botQuadPoint_01 = botPoint_01;
							Point3D botQuadPoint_02 = botPoint_02;
							if (Utils.FEQ(botPoint_01.X, shutterGeom.TopLeft_GlobalPoint.X))
							{
								botQuadPoint_01.Z = shutterGeom.Length_Z;
								botQuadPoint_02.Z = shutterGeom.Length_Z;
							}
							Point3D topQuadPoint_01 = _CalculateRoofPoint(botQuadPoint_01, doc, isRoofAvailable, warehouseOffsetVector);
							Point3D topQuadPoint_02 = _CalculateRoofPoint(botQuadPoint_02, doc, isRoofAvailable, warehouseOffsetVector);

							sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topQuadPoint_01, topQuadPoint_02, botQuadPoint_02, botQuadPoint_01);
						}

						// draw wall sides
						if (Utils.FLT(botPoint_01.X, botPoint_04.X))
						{
							bool wallUseMiddlePnt = false;
							if (bUseMiddlePoints && !bHorizontalRidge && Utils.FLT(botPoint_01.X + warehouseOffsetVector.X, middleRoof2DPoint_01.X) && Utils.FGT(botPoint_04.X + warehouseOffsetVector.X, middleRoof2DPoint_01.X))
								wallUseMiddlePnt = true;
							_BuildWallSides(doc, isRoofAvailable, warehouseOffsetVector, wall.WallPosition, sWalls_MeshBuilderDictionary[wall.FillColor], botPoint_01, botPoint_02, botPoint_03, botPoint_04, wallUseMiddlePnt, middleRoof2DPoint_01);

							curDistanceFromTheGround = originPoint.Z;
						}

						//
						bool bDrawShutterLeftSide = Utils.FNE(originPoint.X + shutterGeom.TopLeft_GlobalPoint.X - botPoint_01.X, 0.0) || (wallShuttersList.IndexOf(shutterGeom) > 0 && Utils.FLT(curDistanceFromTheGround, shutterGeom.Length_Z));

						// draw shutter sides
						botPoint_01 = new Point3D(originPoint.X + shutterGeom.TopLeft_GlobalPoint.X, originPoint.Y + shutterGeom.TopLeft_GlobalPoint.Y, originPoint.Z + shutterGeom.Length_Z);
						botPoint_02 = new Point3D(originPoint.X + shutterGeom.BottomLeft_GlobalPoint.X, originPoint.Y + shutterGeom.BottomLeft_GlobalPoint.Y, originPoint.Z + shutterGeom.Length_Z);
						botPoint_03 = new Point3D(originPoint.X + shutterGeom.BottomRight_GlobalPoint.X, originPoint.Y + shutterGeom.BottomRight_GlobalPoint.Y, originPoint.Z + shutterGeom.Length_Z);
						botPoint_04 = new Point3D(originPoint.X + shutterGeom.TopRight_GlobalPoint.X, originPoint.Y + shutterGeom.TopRight_GlobalPoint.Y, originPoint.Z + shutterGeom.Length_Z);
						//
						bool useMiddlePnt = false;
						if (bUseMiddlePoints && !bHorizontalRidge && Utils.FLT(botPoint_01.X + warehouseOffsetVector.X, middleRoof2DPoint_01.X) && Utils.FGT(botPoint_04.X + warehouseOffsetVector.X, middleRoof2DPoint_01.X))
							useMiddlePnt = true;
						_BuildWallSides(doc, isRoofAvailable, warehouseOffsetVector, wall.WallPosition, sWalls_MeshBuilderDictionary[wall.FillColor], botPoint_01, botPoint_02, botPoint_03, botPoint_04, useMiddlePnt, middleRoof2DPoint_01);

						// left shutter side
						if (bDrawShutterLeftSide)
						{
							Point3D botQuadPoint_01 = botPoint_01;
							botQuadPoint_01.Z = curDistanceFromTheGround;
							Point3D botQuadPoint_02 = botPoint_02;
							botQuadPoint_02.Z = curDistanceFromTheGround;
							Point3D topQuadPoint_01 = botQuadPoint_01;
							topQuadPoint_01.Z = shutterGeom.Length_Z;
							Point3D topQuadPoint_02 = botQuadPoint_02;
							topQuadPoint_02.Z = shutterGeom.Length_Z;

							sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topQuadPoint_02, topQuadPoint_01, botQuadPoint_01, botQuadPoint_02);
						}

						// shutter right side
						Shutter nextShutter = null;
						if (wallShuttersList.IndexOf(shutterGeom) + 1 < wallShuttersList.Count)
							nextShutter = wallShuttersList[wallShuttersList.IndexOf(shutterGeom) + 1];
						bool bDrawShutterRightSide = false;
						if((nextShutter != null && Utils.FLT(shutterGeom.TopRight_GlobalPoint.X, nextShutter.TopLeft_GlobalPoint.X))
							|| (nextShutter != null && Utils.FEQ(shutterGeom.TopRight_GlobalPoint.X, nextShutter.TopLeft_GlobalPoint.X) && Utils.FGT(shutterGeom.Length_Z, nextShutter.Length_Z))
							|| (nextShutter == null && Utils.FLT(shutterGeom.TopRight_GlobalPoint.X, wall.TopRight_GlobalPoint.X)))
						{
							bDrawShutterRightSide = true;
						}
						if(bDrawShutterRightSide && Utils.FLT(originPoint.Z, shutterGeom.Length_Z))
						{
							Point3D botQuadPoint_01 = botPoint_04;
							Point3D botQuadPoint_02 = botPoint_03;
							if (nextShutter != null && Utils.FEQ(shutterGeom.TopRight_GlobalPoint.X, nextShutter.TopLeft_GlobalPoint.X) && Utils.FGT(shutterGeom.Length_Z, nextShutter.Length_Z))
							{
								botQuadPoint_01.Z = nextShutter.Length_Z;
								botQuadPoint_02.Z = nextShutter.Length_Z;
							}
							else
							{
								botQuadPoint_01.Z = originPoint.Z;
								botQuadPoint_02.Z = originPoint.Z;
							}
							Point3D topQuadPoint_01 = botQuadPoint_01;
							topQuadPoint_01.Z = shutterGeom.Length_Z;
							Point3D topQuadPoint_02 = botQuadPoint_02;
							topQuadPoint_02.Z = shutterGeom.Length_Z;

							sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topQuadPoint_01, topQuadPoint_02, botQuadPoint_02, botQuadPoint_01);
							//wallMeshBuilder.AddQuad(botQuadPoint_01, botQuadPoint_02, topQuadPoint_02, topQuadPoint_01);
						}

						//
						botPoint_01 = botPoint_04;
						botPoint_01.Z = originPoint.Z;
						botPoint_02 = botPoint_03;
						botPoint_02.Z = originPoint.Z;

						curDistanceFromTheGround = shutterGeom.Length_Z;

						// draw right wall side
						if (wallShuttersList.LastOrDefault() == shutterGeom)
						{
							botPoint_03 = new Point3D(originPoint.X + wall.BottomRight_GlobalPoint.X, originPoint.Y + wall.BottomRight_GlobalPoint.Y, originPoint.Z);
							botPoint_04 = new Point3D(originPoint.X + wall.TopRight_GlobalPoint.X, originPoint.Y + wall.TopRight_GlobalPoint.Y, originPoint.Z);
							// draw wall
							if (Utils.FNE(botPoint_01.X, originPoint.X + wall.TopRight_GlobalPoint.X))
							{
								// other wall sides
								bool wallUseMiddlePnt = false;
								if (bUseMiddlePoints && !bHorizontalRidge && Utils.FLT(botPoint_01.X + warehouseOffsetVector.X, middleRoof2DPoint_01.X) && Utils.FGT(botPoint_04.X + warehouseOffsetVector.X, middleRoof2DPoint_01.X))
									wallUseMiddlePnt = true;
								_BuildWallSides(doc, isRoofAvailable, warehouseOffsetVector, wall.WallPosition, sWalls_MeshBuilderDictionary[wall.FillColor], botPoint_01, botPoint_02, botPoint_03, botPoint_04, wallUseMiddlePnt, middleRoof2DPoint_01);
							}

							// draw wall right side
							Point3D botQuadPoint_01 = botPoint_04;
							Point3D botQuadPoint_02 = botPoint_03;
							if (Utils.FEQ(wall.TopRight_GlobalPoint.X, shutterGeom.TopRight_GlobalPoint.X))
							{
								botQuadPoint_01.Z = shutterGeom.Length_Z;
								botQuadPoint_02.Z = shutterGeom.Length_Z;
							}
							Point3D topQuadPoint_01 = _CalculateRoofPoint(botQuadPoint_01, doc, isRoofAvailable, warehouseOffsetVector);
							Point3D topQuadPoint_02 = _CalculateRoofPoint(botQuadPoint_02, doc, isRoofAvailable, warehouseOffsetVector);

							sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topQuadPoint_02, topQuadPoint_01, botQuadPoint_01, botQuadPoint_02);
						}
					}
				}
				else if(eWallPosition.eLeft == wall.WallPosition || eWallPosition.eRight == wall.WallPosition)
				{
					Point3D botPoint_01 = new Point3D(originPoint.X + wall.TopLeft_GlobalPoint.X, originPoint.Y + wall.TopLeft_GlobalPoint.Y, originPoint.Z);
					Point3D botPoint_02 = new Point3D(originPoint.X + wall.TopRight_GlobalPoint.X, originPoint.Y + wall.TopRight_GlobalPoint.Y, originPoint.Z);

					double curDistanceFromTheGround = originPoint.Z;

					foreach (Shutter shutterGeom in wallShuttersList)
					{
						Point3D botPoint_03 = new Point3D(originPoint.X + shutterGeom.TopRight_GlobalPoint.X, originPoint.Y + shutterGeom.TopRight_GlobalPoint.Y, originPoint.Z);
						Point3D botPoint_04 = new Point3D(originPoint.X + shutterGeom.TopLeft_GlobalPoint.X, originPoint.Y + shutterGeom.TopLeft_GlobalPoint.Y, originPoint.Z);

						// top wall side
						if (wallShuttersList.IndexOf(shutterGeom) == 0)
						{
							Point3D botQuadPoint_01 = botPoint_01;
							Point3D botQuadPoint_02 = botPoint_02;
							if (Utils.FEQ(botPoint_01.Y, shutterGeom.TopLeft_GlobalPoint.Y))
							{
								botQuadPoint_01.Z = shutterGeom.Length_Z;
								botQuadPoint_02.Z = shutterGeom.Length_Z;
							}
							Point3D topQuadPoint_01 = _CalculateRoofPoint(botQuadPoint_01, doc, isRoofAvailable, warehouseOffsetVector);
							Point3D topQuadPoint_02 = _CalculateRoofPoint(botQuadPoint_02, doc, isRoofAvailable, warehouseOffsetVector);

							sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topQuadPoint_02, topQuadPoint_01, botQuadPoint_01, botQuadPoint_02);
						}

						// draw wall sides
						if (Utils.FLT(botPoint_01.Y, botPoint_04.Y))
						{
							bool wallUseMiddlePnt = false;
							if (bUseMiddlePoints && bHorizontalRidge && Utils.FLT(botPoint_01.Y + warehouseOffsetVector.Y, middleRoof2DPoint_01.Y) && Utils.FGT(botPoint_04.Y + warehouseOffsetVector.Y, middleRoof2DPoint_01.Y))
								wallUseMiddlePnt = true;
							_BuildWallSides(doc, isRoofAvailable, warehouseOffsetVector, wall.WallPosition, sWalls_MeshBuilderDictionary[wall.FillColor], botPoint_01, botPoint_02, botPoint_03, botPoint_04, wallUseMiddlePnt, middleRoof2DPoint_01);

							curDistanceFromTheGround = originPoint.Z;
						}

						//
						bool bDrawShutterTopSide = Utils.FNE(originPoint.Y + shutterGeom.TopLeft_GlobalPoint.Y - botPoint_01.Y, 0.0) || (wallShuttersList.IndexOf(shutterGeom) > 0 && Utils.FLT(curDistanceFromTheGround, shutterGeom.Length_Z));

						// draw shutter sides
						botPoint_01 = new Point3D(originPoint.X + shutterGeom.TopLeft_GlobalPoint.X, originPoint.Y + shutterGeom.TopLeft_GlobalPoint.Y, originPoint.Z + shutterGeom.Length_Z);
						botPoint_02 = new Point3D(originPoint.X + shutterGeom.TopRight_GlobalPoint.X, originPoint.Y + shutterGeom.TopRight_GlobalPoint.Y, originPoint.Z + shutterGeom.Length_Z);
						botPoint_03 = new Point3D(originPoint.X + shutterGeom.BottomRight_GlobalPoint.X, originPoint.Y + shutterGeom.BottomRight_GlobalPoint.Y, originPoint.Z + shutterGeom.Length_Z);
						botPoint_04 = new Point3D(originPoint.X + shutterGeom.BottomLeft_GlobalPoint.X, originPoint.Y + shutterGeom.BottomLeft_GlobalPoint.Y, originPoint.Z + shutterGeom.Length_Z);
						//
						bool useMiddlePnt = false;
						if (bUseMiddlePoints && bHorizontalRidge && Utils.FLT(botPoint_01.Y + warehouseOffsetVector.Y, middleRoof2DPoint_01.Y) && Utils.FGT(botPoint_04.Y + warehouseOffsetVector.Y, middleRoof2DPoint_01.Y))
							useMiddlePnt = true;
						_BuildWallSides(doc, isRoofAvailable, warehouseOffsetVector, wall.WallPosition, sWalls_MeshBuilderDictionary[wall.FillColor], botPoint_01, botPoint_02, botPoint_03, botPoint_04, useMiddlePnt, middleRoof2DPoint_01);

						// left shutter side
						if (bDrawShutterTopSide)
						{
							Point3D botQuadPoint_01 = botPoint_01;
							botQuadPoint_01.Z = curDistanceFromTheGround;
							Point3D botQuadPoint_02 = botPoint_02;
							botQuadPoint_02.Z = curDistanceFromTheGround;
							Point3D topQuadPoint_01 = botQuadPoint_01;
							topQuadPoint_01.Z = shutterGeom.Length_Z;
							Point3D topQuadPoint_02 = botQuadPoint_02;
							topQuadPoint_02.Z = shutterGeom.Length_Z;

							sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topQuadPoint_01, topQuadPoint_02, botQuadPoint_02, botQuadPoint_01);
						}

						// shutter right side
						Shutter nextShutter = null;
						if (wallShuttersList.IndexOf(shutterGeom) + 1 < wallShuttersList.Count)
							nextShutter = wallShuttersList[wallShuttersList.IndexOf(shutterGeom) + 1];
						bool bDrawShutterRightSide = false;
						if ((nextShutter != null && Utils.FLT(shutterGeom.BottomLeft_GlobalPoint.Y, nextShutter.TopLeft_GlobalPoint.Y))
							|| (nextShutter != null && Utils.FEQ(shutterGeom.BottomLeft_GlobalPoint.Y, nextShutter.TopLeft_GlobalPoint.Y) && Utils.FGT(shutterGeom.Length_Z, nextShutter.Length_Z))
							|| (nextShutter == null && Utils.FLT(shutterGeom.BottomLeft_GlobalPoint.Y, wall.BottomLeft_GlobalPoint.Y)))
						{
							bDrawShutterRightSide = true;
						}
						if (bDrawShutterRightSide && Utils.FLT(originPoint.Z, shutterGeom.Length_Z))
						{
							Point3D botQuadPoint_01 = botPoint_04;
							Point3D botQuadPoint_02 = botPoint_03;
							if (nextShutter != null && Utils.FEQ(shutterGeom.BottomLeft_GlobalPoint.Y, nextShutter.TopLeft_GlobalPoint.Y) && Utils.FGT(shutterGeom.Length_Z, nextShutter.Length_Z))
							{
								botQuadPoint_01.Z = nextShutter.Length_Z;
								botQuadPoint_02.Z = nextShutter.Length_Z;
							}
							else
							{
								botQuadPoint_01.Z = originPoint.Z;
								botQuadPoint_02.Z = originPoint.Z;
							}
							Point3D topQuadPoint_01 = botQuadPoint_01;
							topQuadPoint_01.Z = shutterGeom.Length_Z;
							Point3D topQuadPoint_02 = botQuadPoint_02;
							topQuadPoint_02.Z = shutterGeom.Length_Z;

							sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topQuadPoint_02, topQuadPoint_01, botQuadPoint_01, botQuadPoint_02);
							//wallMeshBuilder.AddQuad(botQuadPoint_01, botQuadPoint_02, topQuadPoint_02, topQuadPoint_01);
						}

						//
						botPoint_01 = botPoint_04;
						botPoint_01.Z = originPoint.Z;
						botPoint_02 = botPoint_03;
						botPoint_02.Z = originPoint.Z;

						curDistanceFromTheGround = shutterGeom.Length_Z;

						// draw right wall side
						if (wallShuttersList.LastOrDefault() == shutterGeom)
						{
							botPoint_03 = new Point3D(originPoint.X + wall.BottomRight_GlobalPoint.X, originPoint.Y + wall.BottomRight_GlobalPoint.Y, originPoint.Z);
							botPoint_04 = new Point3D(originPoint.X + wall.BottomLeft_GlobalPoint.X, originPoint.Y + wall.BottomLeft_GlobalPoint.Y, originPoint.Z);
							// draw wall
							if (Utils.FNE(botPoint_01.Y, originPoint.Y + wall.BottomLeft_GlobalPoint.Y))
							{
								// other wall sides
								bool wallUseMiddlePnt = false;
								if (bUseMiddlePoints && bHorizontalRidge && Utils.FLT(botPoint_01.Y + warehouseOffsetVector.Y, middleRoof2DPoint_01.Y) && Utils.FGT(botPoint_04.Y + warehouseOffsetVector.Y, middleRoof2DPoint_01.Y))
									wallUseMiddlePnt = true;
								_BuildWallSides(doc, isRoofAvailable, warehouseOffsetVector, wall.WallPosition, sWalls_MeshBuilderDictionary[wall.FillColor], botPoint_01, botPoint_02, botPoint_03, botPoint_04, wallUseMiddlePnt, middleRoof2DPoint_01);
							}
						
							// draw wall right side
							Point3D botQuadPoint_01 = botPoint_04;
							Point3D botQuadPoint_02 = botPoint_03;
							if (Utils.FEQ(wall.BottomLeft_GlobalPoint.Y, shutterGeom.BottomLeft_GlobalPoint.Y))
							{
								botQuadPoint_01.Z = shutterGeom.Length_Z;
								botQuadPoint_02.Z = shutterGeom.Length_Z;
							}
							Point3D topQuadPoint_01 = _CalculateRoofPoint(botQuadPoint_01, doc, isRoofAvailable, warehouseOffsetVector);
							Point3D topQuadPoint_02 = _CalculateRoofPoint(botQuadPoint_02, doc, isRoofAvailable, warehouseOffsetVector);

							sWalls_MeshBuilderDictionary[wall.FillColor].AddQuad(topQuadPoint_01, topQuadPoint_02, botQuadPoint_02, botQuadPoint_01);
						}
					}
				}
			}

			return true;
		}
		private static void _BuildWallSides(
			DrawingDocument doc,
			bool isRoofAvailable,
			Vector warehouseOffsetVector,
			//
			eWallPosition wallPosition,
			MeshBuilder wallMeshBuilder,
			//
			Point3D botPoint_01,
			Point3D botPoint_02,
			Point3D botPoint_03,
			Point3D botPoint_04,
			bool bUseMiddlePoint,
			Point middlePoint)
		{
			if (doc == null)
				return;

			if (wallMeshBuilder == null)
				return;

			Point3D topPoint_01 = _CalculateRoofPoint(botPoint_01, doc, isRoofAvailable, warehouseOffsetVector);
			Point3D topPoint_02 = _CalculateRoofPoint(botPoint_02, doc, isRoofAvailable, warehouseOffsetVector);
			Point3D topPoint_03 = _CalculateRoofPoint(botPoint_03, doc, isRoofAvailable, warehouseOffsetVector);
			Point3D topPoint_04 = _CalculateRoofPoint(botPoint_04, doc, isRoofAvailable, warehouseOffsetVector);

			if (eWallPosition.eTop == wallPosition || eWallPosition.eBot == wallPosition)
			{
				// bottom side
				if (Utils.FNE(botPoint_01.X, botPoint_03.X))
					wallMeshBuilder.AddQuad(botPoint_01, botPoint_02, botPoint_03, botPoint_04);
				// outside
				wallMeshBuilder.AddQuad(botPoint_01, botPoint_04, topPoint_04, topPoint_01);
				// inside
				wallMeshBuilder.AddQuad(topPoint_02, topPoint_03, botPoint_03, botPoint_02);
				// top
				if (bUseMiddlePoint)
				{
					Point3D middleTopPoint_01 = botPoint_01;
					middleTopPoint_01.X = middlePoint.X - warehouseOffsetVector.X;
					middleTopPoint_01 = _CalculateRoofPoint(middleTopPoint_01, doc, isRoofAvailable, warehouseOffsetVector);

					Point3D middleTopPoint_02 = botPoint_02;
					middleTopPoint_02.X = middlePoint.X - warehouseOffsetVector.X;
					middleTopPoint_02 = _CalculateRoofPoint(middleTopPoint_02, doc, isRoofAvailable, warehouseOffsetVector);

					// add outside triangle
					wallMeshBuilder.AddTriangle(topPoint_01, topPoint_04, middleTopPoint_01);

					// add inside triangle
					wallMeshBuilder.AddTriangle(topPoint_03, topPoint_02, middleTopPoint_02);

					// add top
					wallMeshBuilder.AddQuad(topPoint_02, topPoint_01, middleTopPoint_01, middleTopPoint_02);
					wallMeshBuilder.AddQuad(topPoint_04, topPoint_03, middleTopPoint_02, middleTopPoint_01);
				}
				else
				{
					// add top
					wallMeshBuilder.AddQuad(topPoint_04, topPoint_03, topPoint_02, topPoint_01);
				}
			}
			else if(eWallPosition.eLeft == wallPosition || eWallPosition.eRight == wallPosition)
			{
				// bottom side
				if (Utils.FNE(botPoint_01.Y, botPoint_03.Y))
					wallMeshBuilder.AddQuad(botPoint_04, botPoint_03, botPoint_02, botPoint_01);
				// outside
				wallMeshBuilder.AddQuad(topPoint_01, topPoint_04, botPoint_04, botPoint_01);
				// inside
				wallMeshBuilder.AddQuad(topPoint_03, topPoint_02, botPoint_02, botPoint_03);
				// top
				if (bUseMiddlePoint)
				{
					Point3D middleTopPoint_01 = botPoint_01;
					middleTopPoint_01.Y = middlePoint.Y - warehouseOffsetVector.Y;
					middleTopPoint_01 = _CalculateRoofPoint(middleTopPoint_01, doc, isRoofAvailable, warehouseOffsetVector);

					Point3D middleTopPoint_02 = botPoint_02;
					middleTopPoint_02.Y = middlePoint.Y - warehouseOffsetVector.Y;
					middleTopPoint_02 = _CalculateRoofPoint(middleTopPoint_02, doc, isRoofAvailable, warehouseOffsetVector);

					// add outside triangle
					wallMeshBuilder.AddTriangle(middleTopPoint_01, topPoint_04, topPoint_01);

					// add inside triangle
					wallMeshBuilder.AddTriangle(middleTopPoint_02, topPoint_02, topPoint_03);

					// add top
					wallMeshBuilder.AddQuad(topPoint_01, topPoint_02, middleTopPoint_02, middleTopPoint_01);
					wallMeshBuilder.AddQuad(topPoint_03, topPoint_04, middleTopPoint_01, middleTopPoint_02);
				}
				else
				{
					// add top
					wallMeshBuilder.AddQuad(topPoint_01, topPoint_02, topPoint_03, topPoint_04);
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// Build floor 3D model
		/// </summary>
		private static double FLOOR_THICKNESS = 10;
		public static bool AddFloorModel(DrawingSheet sheet, Point3D originPoint)
		{
			if (sheet == null)
				return false;

			Point3D floorCenterPnt = originPoint;
			floorCenterPnt.X += (double)sheet.Length / 2;
			floorCenterPnt.Y += (double)sheet.Width / 2;
			floorCenterPnt.Z -= FLOOR_THICKNESS / 2;

			sFloor_MeshBuilder.AddBox(floorCenterPnt, sheet.Length, sheet.Width, FLOOR_THICKNESS);
			return true;
		}
	}
}
